// Prototype: detection of Poiyomi materials stuck on a REMOVED version (9.2 and older).
// Designed by BluWizard LABS - https://github.com/BluWizard10
//
// The regular PoiyomiVersionDetector reads the version from a PRESENT shader's `shader_master_label`.
// That fails for 9.2-and-older because those shaders were deleted from the package:
//   - unlocked materials fall back to Hidden/InternalErrorShader (no label to read),
//   - locked materials can silently mis-resolve to 10.0 via the OriginalShader NAME tag.
//
// This detector recovers the "removed 9.x" state from signals that survive the shader removal:
//   1. OriginalShaderGUID tag - if the GUID no longer resolves to an asset, the original was DELETED.
//      This is collision-proof, unlike the name tag which can resolve to the 10.0 shader.
//   2. Property fingerprint - names that existed in 9.0-9.3 and were RENAMED/REMOVED in 10.0
//      (_ALUVPosition, _FlipbookScaleOffset, _RimSharpness, _ContinuousDissolve). Read straight from
//      the serialized property sheet, so it works even with no shader assigned.
//
// Neither signal may be read as "legacy" on its own: a dangling 10.0 reference is just as shader-less as a 9.2
// one. Signal 1 resolves the GUID against the shipped table, which lists 10.0 too, so a missing 10.0 shader comes
// back as version 10.0 rather than as an unknown; and signal 2 is discarded whenever the sheet also carries a
// property newer than 9.3, since Unity never drops the orphaned 9.x names from an upgraded material. Both halves
// need upkeep per release - PoiDevShaderGuidTableAudit reports the GUID half when it falls behind.
//
// Signal 2 tells us the VERSION, never the identity - those property names are shared with other toon shaders.
// Identity comes first, from a ".poiyomi/" shader name, the OriginalShader tag, or a shipped shader GUID; see
// IsPoiyomiIdentity. Without that gate a third-party material whose own shader was missing fingerprinted as
// legacy Poiyomi and got converted onto a Poiyomi shader.
//
// The empirical delta (see research): Toon 9.2 == 9.3 exactly; Pro 9.2 -> 9.3 only drops the DPS
// penetrator system + legacy anisotropic-noise map, both removed upstream. So the target is always 9.3
// and there are zero property remaps - this is a routing problem, not a translation problem.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Thry.ThryEditor;
// Thry.ThryEditor also declares a `Version` type (Config.cs), so bare `Version` is ambiguous here.
// We want System.Version - the type PoiyomiVersionDetector's version APIs use.
using Version = System.Version;

namespace Poi.Tools.ShaderTranslator.VersionUpgrade
{
	public enum PoiyomiEdition { Unknown, Toon, Pro }

	/// <summary>
	/// Everything the executor needs to route a removed-version material onto 9.3.
	/// </summary>
	public struct LegacyMaterialInfo
	{
		public bool IsLocked;              // material was optimized/locked (Hidden/Locked or _ShaderOptimizerEnabled)
		public bool LockedShaderBroken;    // locked AND the generated shader asset is also gone
		public bool ShaderMissing;         // the material's real shader is gone: on the error shader, or a locked material whose source shader no longer resolves. (Legacy materials still rendering on a present shader have this false.)
		public PoiyomiEdition Edition;     // Toon or Pro
		public string Variant;             // full variant, e.g. "Poiyomi Pro World"; null if only the edition is known
		public Version DetectedVersion;    // best-effort source version; null if only "pre-10.0" is known
		public bool UsesDps;               // DPS/penetrator toggle is ON - config will be dropped by 9.3
		public bool UsesLegacyAnisoNoise;  // legacy _AnisoNoiseMap assigned - dropped by 9.3
	}

	public static class LegacyMaterialDetector
	{
		public static readonly Version Nine3 = new Version(9, 3);

		// Present in some pre-10.0 version, renamed or removed by 10.0. Presence => the material predates 10.0.
		// _ClippingMask is pre-9.0 (renamed to _AlphaMask in 9.0), so it also flags 8.x/7.x materials.
		static readonly string[] Pre10FingerprintMarkers =
		{
			"_ALUVPosition", "_ALUVScale", "_ALUVRotation",
			"_FlipbookScaleOffset", "_RimSharpness", "_Rim2Sharpness", "_ContinuousDissolve",
			"_ClippingMask"
		};

		// Names that did not exist in 9.3 or anything older. Presence => the material has been on a post-9.3 shader,
		// so it is NOT a removed-version material, whatever else the sheet still carries. Needed because Unity keeps
		// orphaned properties in a material's serialized sheet forever: a material upgraded 9.3 -> 10.0 still lists
		// _RimSharpness and friends, so the pre-10.0 fingerprint alone would read it as legacy the moment its shader
		// went missing.
		//
		// Deliberately NOT named for one release. The rule is "introduced after the legacy line and still declared by
		// the shaders that ship today", so if a future version renames or drops one of these, swap it for a property
		// that version introduces - the list only has to separate "modern" from the frozen 7.3-9.3 legacy line, not
		// tell one modern version from another. The five below are declared by all 46 of 10.0's shaders and none of
		// 9.3's, and all are Float/Int/Range/Vector: locking strips unassigned TEXTURE properties from the sheet
		// (ShaderOptimizer.LockApplyShader), so a texture-typed marker would go missing on exactly the locked
		// materials this guard exists for.
		static readonly string[] PostNine3Markers =
		{
			"_AlphaMaskChannelBlendMode", "_AlphaMaskGamma", "_AlphaMaskMinMax",
			"_ALDecalPosition", "_AudioLinkDebugEnabled"
		};

		// DPS toggles, read by VALUE (never by presence) so we can warn that an ENABLED penetrator/orifice config
		// won't survive the move to 9.3, which dropped DPS in favour of TPS/SPS. The DPS property NAMES are not an
		// edition signal - see ResolveEdition for why that reading was wrong.
		static readonly string[] DpsEnableToggles = { "_PenetratorEnabled", "_OrifaceEnabled" };
		const string LegacyAnisoNoiseTex = "_AnisoNoiseMap";

		const string LockedShaderPrefix = "Hidden/Locked/";

		/// <summary>
		/// True if the material is a Poiyomi material stranded on a removed version (9.2 or older) and should
		/// be routed to 9.3. Materials already on a present 9.3/10.0 shader are left to the normal pipeline.
		/// </summary>
		public static bool TryDetectLegacyNine(Material material, out LegacyMaterialInfo info)
		{
			info = default;
			if (material == null || material.shader == null)
				return false;

			string originalTag = material.GetTag(ShaderOptimizer.TAG_ORIGINAL_SHADER, false, string.Empty);

			if (!IsPoiyomiIdentity(material, originalTag)) return false;

			// The common case - an unlocked material on a present, healthy, current Poiyomi shader - can be answered
			// from the shader alone: it is not a removed version, so it is not ours. Deciding that only after reading
			// the whole serialized sheet cost ~55 ms and ~3 MB per call, and this is called from
			// ErrorShaderEditor.Awake, i.e. every time Unity creates a material editor - which it does once per
			// thumbnail regeneration, every drag frame of a slider. Anything that is not this exact case still goes
			// through the full sheet-based detection below, unchanged.
			if (!material.shader.IsBroken()
				&& !material.shader.name.StartsWith(LockedShaderPrefix, StringComparison.OrdinalIgnoreCase)
				&& PoiyomiVersionDetector.TryGetVersionFromShader(material.shader, out Version presentVersion)
				&& presentVersion >= Nine3)
				return false;

			var reader = MaterialSerializedReader.Read(material);

    		// Only counts as pre-10.0 evidence when the sheet carries nothing newer than 9.3 - see PostNine3Markers.
			bool hasPre10Fingerprint = reader.HasAny(Pre10FingerprintMarkers) && !reader.HasAny(PostNine3Markers);
			bool locked = IsLocked(material, reader);

			if (locked)
			{
				if (!TryDetectLocked(material, originalTag, hasPre10Fingerprint, ref info))
					return false;
			}
			else
			{
				if (!TryDetectUnlocked(material, hasPre10Fingerprint, ref info))
					return false;
			}

			// Fill in edition/variant, DPS/aniso usage from whatever we resolved plus the fingerprint.
			FinalizeInfo(reader, originalTag, ref info);
			return true;
		}

		/// <summary>Convenience wrapper for menu validation.</summary>
		public static bool NeedsLegacyUpgrade(Material material) => TryDetectLegacyNine(material, out _);

		/// <summary>
		/// True only when the material is legacy AND its shader is genuinely gone (on the error shader, or a locked
		/// material whose source shader no longer resolves). A legacy material still rendering on a present pre-9.3
		/// shader returns false - it's fine to keep using or to upgrade from the menu, so no error-shader prompt.
		/// </summary>
		public static bool IsLegacyShaderMissing(Material material) =>
			TryDetectLegacyNine(material, out LegacyMaterialInfo info) && info.ShaderMissing;

		// --- Locked path -------------------------------------------------------------------------------

		// The OriginalShaderGUID tag is the reliable signal: if it no longer resolves to an asset, the
		// original shader was deleted (a removed version). If it DOES resolve, read that shader's version -
		// a locked 9.3/10.0 material is the normal pipeline's job, not ours.
		static bool TryDetectLocked(Material material, string originalTag,
			bool hasPre10Fingerprint, ref LegacyMaterialInfo info)
		{
			info.IsLocked = true;
			info.LockedShaderBroken = material.shader.IsBroken();

			string guid = material.GetTag(ShaderOptimizer.TAG_ORIGINAL_SHADER_GUID, false, string.Empty);
			if (!string.IsNullOrEmpty(guid))
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				if (!string.IsNullOrEmpty(path))
				{
					// Original shader asset still exists - resolve its version and only claim it if pre-9.3.
					Shader original = AssetDatabase.LoadAssetAtPath<Shader>(path);
					if (original != null && PoiyomiVersionDetector.TryGetVersionFromShader(original, out Version v))
					{
						info.DetectedVersion = v;
						info.Variant = PoiyomiVersionDetector.GetShaderVariant(original);
						// Source shader exists - only "missing" if the baked shader itself is gone (material on error shader).
						info.ShaderMissing = info.LockedShaderBroken;
						return v < Nine3;
					}
				}
			}

			// GUID dead => the original shader was removed from the project. The GUID itself still identifies
			// exactly which shader it was, so resolve it against the shipped-GUID table before falling back to
			// name parsing (which can't tell 9.0 from 9.2, and mis-orders variants like "Outline Early").
			if (TryResolveKnownShaderGuid(material, guid, out PoiyomiLegacyShaderGuids.LegacyShaderId knownLocked))
			{
				info.Variant = knownLocked.Variant;
				info.DetectedVersion = knownLocked.Version;
				info.ShaderMissing = true;
				return knownLocked.Version <= Nine3;
			}

			// Not a GUID we ship, and no live shader to read a version off. The OriginalShader tag can't stand in for
			// one: a material locked on 10.0 carries the same ".poiyomi/Poiyomi Toon" tag a 9.2 one does, so accepting
			// the name here claimed every locked material whose shader had gone missing - 10.0 included - and offered
			// to translate it down to 9.3. The property fingerprint is the only pre-10.0 evidence left, so require it.
			if (!hasPre10Fingerprint)
				return false;

			info.Variant = VariantFromLockedName(material.shader.name, originalTag);
			info.DetectedVersion = ParseVersionFromTag(originalTag);
			info.ShaderMissing = true; // source shader no longer resolves
			return true;
		}

		// --- Unlocked path -----------------------------------------------------------------------------

		static bool TryDetectUnlocked(Material material,
			bool hasPre10Fingerprint, ref LegacyMaterialInfo info)
		{
			// Present, healthy shader: only our job if it reads as a pre-9.3 Poiyomi shader (rare post-removal).
			if (!material.shader.IsBroken())
			{
				if (PoiyomiVersionDetector.TryGetVersionFromShader(material.shader, out Version v))
				{
					if (v >= Nine3)
						return false; // on a present 9.3/10.0 shader - not our case
					info.DetectedVersion = v;
					info.Variant = PoiyomiVersionDetector.GetShaderVariant(material.shader);
					info.ShaderMissing = false; // still rendering on its (present) pre-9.3 shader
					return true;
				}
				return false;
			}

			// Broken/missing shader (the classic unlocked-9.2 error-shader state).
			// The dangling reference still carries the GUID of the shader this material was authored against,
			// so try to identify it exactly first. This also catches materials whose properties are all still at
			// their defaults, which the fingerprint below would miss.
			string leftoverGuid = material.GetTag(ShaderOptimizer.TAG_ORIGINAL_SHADER_GUID, false, string.Empty);
			if (TryResolveKnownShaderGuid(material, leftoverGuid, out PoiyomiLegacyShaderGuids.LegacyShaderId knownUnlocked))
			{
				info.Variant = knownUnlocked.Variant;
				info.DetectedVersion = knownUnlocked.Version;
				info.ShaderMissing = true;
				return knownUnlocked.Version <= Nine3;
			}

			// Unknown GUID - fall back to the property fingerprint.
			if (!hasPre10Fingerprint)
				return false;

			// Variant can't be recovered from an unrecognised dangling GUID - only the edition, from the fingerprint.
			info.Variant = null;
			info.DetectedVersion = null; // "pre-10.0", exact version unknown
			info.ShaderMissing = true;   // on the error shader
			return true;
		}

		// --- Shared finalization -----------------------------------------------------------------------

		static void FinalizeInfo(MaterialSerializedReader reader, string originalTag, ref LegacyMaterialInfo info)
		{
			info.Edition = ResolveEdition(info.Variant, originalTag, reader);

			// DPS is Pro-only and only matters if actually enabled - that's the config a 9.3 swap drops.
			foreach (string toggle in DpsEnableToggles)
			{
				if (reader.GetFloat(toggle, 0f) > 0.5f) { info.UsesDps = true; break; }
			}
			info.UsesLegacyAnisoNoise = reader.HasTextureAssigned(LegacyAnisoNoiseTex);
		}

		// Features only Poiyomi Pro has ever shipped: verified present in all 18 of 9.3's Pro variants and all 46 of
		// 10.0's, and in none of either version's Toon variants. Unlike the DPS names, these were never in Toon, so
		// finding one is evidence of the EDITION rather than evidence of the material's history.
		static readonly string[] ProOnlyMarkers = { "_ConstellationEnable", "_ConstellationDensity", "_GT7TM_ChromaFadeStart" };

		static PoiyomiEdition ResolveEdition(string variant, string originalTag, MaterialSerializedReader reader)
		{
			if (!string.IsNullOrEmpty(variant))
				return variant.IndexOf(" Pro", StringComparison.OrdinalIgnoreCase) != -1 ? PoiyomiEdition.Pro : PoiyomiEdition.Toon;

			if (originalTag.IndexOf("Poiyomi Pro", StringComparison.OrdinalIgnoreCase) != -1) return PoiyomiEdition.Pro;
			if (originalTag.IndexOf("Poiyomi Toon", StringComparison.OrdinalIgnoreCase) != -1) return PoiyomiEdition.Toon;

			// Nothing names this material. Fall back to a Pro-exclusive feature, and to Toon when even that is silent.
			//
			// This used to read the mere PRESENCE of the DPS/penetrator property names as "Pro", which was wrong
			// twice over. DPS shipped in Poiyomi TOON through 8.x - Poiyomi's own UDIMDiscard Demo.mat sits on 9.3
			// Toon carrying eight of those names - and Unity keeps orphaned properties on a material forever, so
			// those names outlive the shader that declared them either way. The effect was an 8.x Toon material
			// routed onto 9.3 Pro, and it only surfaced once the user installed Poiyomi Pro: until then Shader.Find
			// for the Pro target returned null and the material was skipped rather than misrouted.
			//
			// Toon is the right default when nothing is known: it is what almost every legacy material is, and
			// landing a Pro material on 9.3 Toon costs Pro-only features that a shader swap restores, where the
			// reverse silently moved Toon materials onto a Pro shader they had never used.
			return reader.HasAny(ProOnlyMarkers) ? PoiyomiEdition.Pro : PoiyomiEdition.Toon;
		}

		// --- Helpers -----------------------------------------------------------------------------------

		// Identity Gate
		static bool IsPoiyomiIdentity(Material material, string originalTag)
		{
			if (PoiyomiVersionDetector.IsPoiyomiShaderName(material.shader.name)) return true;
			if (PoiyomiVersionDetector.IsPoiyomiShaderName(originalTag)) return true;

			// Table lookup, no disk access - cheap enough to try for every material.
			string tagGuid = material.GetTag(ShaderOptimizer.TAG_ORIGINAL_SHADER_GUID, false, string.Empty);
			if (PoiyomiLegacyShaderGuids.TryResolve(tagGuid, out _)) return true;

			// Nothing named this material. Only a dangling shader reference justifies reading the asset off disk for
			// its serialized shader GUID - a healthy shader that isn't ".poiyomi/..." simply isn't ours, and skipping
			// the read there keeps a project-wide scan off the file system for the bulk of its materials.
			if (!material.shader.IsBroken()) return false;

			return TryReadSerializedShaderGuid(material, out string serializedGuid) && PoiyomiLegacyShaderGuids.TryResolve(serializedGuid, out _);
		}

		static bool IsLocked(Material material, MaterialSerializedReader reader)
		{
			if (material.shader.name.StartsWith(LockedShaderPrefix, StringComparison.OrdinalIgnoreCase))
				return true;
			// Broken shader: _ShaderOptimizerEnabled survives on the serialized sheet even when the shader is gone.
			return reader.GetFloat("_ShaderOptimizerEnabled", 0f) > 0.5f;
		}

		static string VariantFromLockedName(string lockedOrErrorName, string originalTag)
		{
			// Prefer the locked shader name (Hidden/Locked/<original>/<guid>); fall back to the OriginalShader tag.
			string fromShader = GetVariantFromName(lockedOrErrorName);
			if (!string.IsNullOrEmpty(fromShader)) return fromShader;
			return GetVariantFromName(originalTag);
		}

		/// <summary>
		/// Extracts the "Poiyomi ..." variant from a raw shader-name string (a locked shader name or an
		/// OriginalShader tag). Mirrors PoiyomiVersionDetector.GetShaderVariant(Shader) but works on a string,
		/// since removed-version materials have no live Shader to read. Kept here so the whole legacy feature is
		/// self-contained and needs no edits to PoiyomiVersionDetector.
		/// </summary>
		public static string GetVariantFromName(string shaderName)
		{
			if (string.IsNullOrEmpty(shaderName))
				return null;

			// Find last "Poiyomi" - the variant always starts with it (not .poiyomi in the path).
			int poiIndex = shaderName.LastIndexOf("Poiyomi", StringComparison.OrdinalIgnoreCase);
			if (poiIndex < 0)
				return null;

			string name = shaderName.Substring(poiIndex);

			// For locked shaders, strip the guid suffix (everything after /).
			int slashIndex = name.IndexOf('/');
			if (slashIndex > 0)
				name = name.Substring(0, slashIndex);

			return name;
		}

		// Matches the shader reference at the top of a .mat, e.g.
		//   m_Shader: {fileID: 4800000, guid: 23f6705aff8bf964c87bfb3dd66ab345, type: 3}
		// The GUID survives in the asset text even when the shader it points at is gone, which is the only place
		// the original identity is still recorded for an unlocked material.
		static readonly Regex SerializedShaderGuidRegex =
			new Regex(@"m_Shader:\s*\{fileID:\s*-?\d+,\s*guid:\s*([0-9a-fA-F]{32})", RegexOptions.Compiled);

		/// <summary>
		/// Identifies the shader a material was authored against, using the shipped-GUID table. Checks the
		/// OriginalShaderGUID tag first (written at lock time, and it survives unlocking), then the material's own
		/// serialized shader reference. Works even when the shader asset is absent from the project entirely.
		/// </summary>
		static bool TryResolveKnownShaderGuid(Material material, string tagGuid, out PoiyomiLegacyShaderGuids.LegacyShaderId id)
		{
			if (PoiyomiLegacyShaderGuids.TryResolve(tagGuid, out id))
				return true;

			return TryReadSerializedShaderGuid(material, out string serializedGuid)
				&& PoiyomiLegacyShaderGuids.TryResolve(serializedGuid, out id);
		}

		/// <summary>
		/// Reads the raw shader GUID out of a material asset's serialized text. Unity's SerializedProperty API
		/// exposes nothing for a reference whose target is missing, so the asset file is the only source. Returns
		/// false for materials embedded in a scene/prefab (no .mat of their own) and for binary-serialized projects.
		/// </summary>
		static bool TryReadSerializedShaderGuid(Material material, out string guid)
		{
			guid = null;

			string path = AssetDatabase.GetAssetPath(material);
			if (string.IsNullOrEmpty(path) || !path.EndsWith(".mat", StringComparison.OrdinalIgnoreCase))
				return false;

			try
			{
				if (!File.Exists(path))
					return false;

				// m_Shader sits in the first few lines of the asset; don't scan the whole property sheet.
				int scanned = 0;
				foreach (string line in File.ReadLines(path))
				{
					Match match = SerializedShaderGuidRegex.Match(line);
					if (match.Success)
					{
						guid = match.Groups[1].Value.ToLowerInvariant();
						return true;
					}
					if (++scanned > 40)
						break;
				}
			}
			catch (IOException) { }
			catch (UnauthorizedAccessException) { }

			return false;
		}

		static Version ParseVersionFromTag(string originalTag)
		{
			// e.g. ".poiyomi/Old Versions/9.2/Poiyomi Toon" -> 9.2. Locked-as-current tags (".poiyomi/Poiyomi Toon")
			// carry no version, so this returns null and the caller treats it as generic pre-10.0.
			return PoiyomiVersionDetector.TryParseVersionFromLabel(originalTag, out Version v) ? v : null;
		}

		/// <summary>
		/// Reads a material's serialized property sheet directly, so property names/values remain visible even
		/// when the current shader declares none of them (missing shader, or the error shader).
		/// </summary>
		public sealed class MaterialSerializedReader
		{
			readonly HashSet<string> _names = new HashSet<string>(StringComparer.Ordinal);
			readonly Dictionary<string, float> _floats = new Dictionary<string, float>(StringComparer.Ordinal);
			readonly Dictionary<string, Color> _colors = new Dictionary<string, Color>(StringComparer.Ordinal);
			readonly Dictionary<string, (Texture tex, Vector4 st)> _textures = new Dictionary<string, (Texture, Vector4)>(StringComparer.Ordinal);
			readonly HashSet<string> _assignedTextures = new HashSet<string>(StringComparer.Ordinal);

			public bool Has(string name) => _names.Contains(name);
			public bool HasAny(string[] names) { foreach (var n in names) if (_names.Contains(n)) return true; return false; }
			public bool HasTextureAssigned(string name) => _assignedTextures.Contains(name);
			public float GetFloat(string name, float fallback) => _floats.TryGetValue(name, out float v) ? v : fallback;
			public bool TryGetFloat(string name, out float value) => _floats.TryGetValue(name, out value);
			public bool TryGetColor(string name, out Color value) => _colors.TryGetValue(name, out value);
			// Vector4 material properties serialize into m_Colors, so a vector read is just a color read.
			public bool TryGetVector(string name, out Vector4 value)
			{
				bool found = _colors.TryGetValue(name, out Color c);
				value = c;
				return found;
			}
			public bool TryGetTexture(string name, out Texture texture, out Vector4 scaleOffset)
			{
				if (_textures.TryGetValue(name, out var t)) { texture = t.tex; scaleOffset = t.st; return true; }
				texture = null; scaleOffset = new Vector4(1, 1, 0, 0); return false;
			}

			public static MaterialSerializedReader Read(Material material)
			{
				var r = new MaterialSerializedReader();
				if (material == null) return r;

				var so = new SerializedObject(material);
				SerializedProperty saved = so.FindProperty("m_SavedProperties");
				if (saved == null) return r;

				ReadFloats(saved.FindPropertyRelative("m_Floats"), r);
				ReadFloats(saved.FindPropertyRelative("m_Ints"), r);   // null-safe; older Unity keeps ints here
				ReadColors(saved.FindPropertyRelative("m_Colors"), r);
				ReadTextures(saved.FindPropertyRelative("m_TexEnvs"), r);
				return r;
			}

			static void ReadFloats(SerializedProperty array, MaterialSerializedReader r)
			{
				if (array == null || !array.isArray) return;
				for (int i = 0; i < array.arraySize; i++)
				{
					SerializedProperty e = array.GetArrayElementAtIndex(i);
					string name = EntryName(e);
					if (string.IsNullOrEmpty(name)) continue;
					r._names.Add(name);
					SerializedProperty val = e.FindPropertyRelative("second");
					if (val != null) r._floats[name] = val.propertyType == SerializedPropertyType.Integer ? val.intValue : val.floatValue;
				}
			}

			static void ReadColors(SerializedProperty array, MaterialSerializedReader r)
			{
				if (array == null || !array.isArray) return;
				for (int i = 0; i < array.arraySize; i++)
				{
					SerializedProperty e = array.GetArrayElementAtIndex(i);
					string name = EntryName(e);
					if (string.IsNullOrEmpty(name)) continue;
					r._names.Add(name);
					SerializedProperty val = e.FindPropertyRelative("second");
					if (val != null) r._colors[name] = val.colorValue;
				}
			}

			static void ReadTextures(SerializedProperty array, MaterialSerializedReader r)
			{
				if (array == null || !array.isArray) return;
				for (int i = 0; i < array.arraySize; i++)
				{
					SerializedProperty e = array.GetArrayElementAtIndex(i);
					string name = EntryName(e);
					if (string.IsNullOrEmpty(name)) continue;
					r._names.Add(name);
					SerializedProperty second = e.FindPropertyRelative("second");
					var tex = second?.FindPropertyRelative("m_Texture")?.objectReferenceValue as Texture;
					Vector2 scale = second?.FindPropertyRelative("m_Scale")?.vector2Value ?? Vector2.one;
					Vector2 offset = second?.FindPropertyRelative("m_Offset")?.vector2Value ?? Vector2.zero;
					r._textures[name] = (tex, new Vector4(scale.x, scale.y, offset.x, offset.y));
					if (tex != null) r._assignedTextures.Add(name);
				}
			}

			static string EntryName(SerializedProperty entry)
			{
				SerializedProperty first = entry.FindPropertyRelative("first");
				if (first == null) return null;
				if (first.propertyType == SerializedPropertyType.String) return first.stringValue;
				return first.FindPropertyRelative("name")?.stringValue;
			}
		}
	}
}
