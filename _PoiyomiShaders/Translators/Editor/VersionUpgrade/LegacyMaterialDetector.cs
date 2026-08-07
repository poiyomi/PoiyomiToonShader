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
// The empirical delta (see research): Toon 9.2 == 9.3 exactly; Pro 9.2 -> 9.3 only drops the DPS
// penetrator system + legacy anisotropic-noise map, both removed upstream. So the target is always 9.3
// and there are zero property remaps - this is a routing problem, not a translation problem.

using System;
using System.Collections.Generic;
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

		// Pro-only in 9.x. Any of these => Pro edition. Also the features 9.3 drops.
		static readonly string[] DpsMarkers =
		{
			"_PenetratorEnabled", "_OrifaceEnabled", "_OrificeData",
			"_Squeeze", "_Wriggle", "_Curvature", "_Length", "_Shape1Depth", "_BlendshapePower"
		};
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

			var reader = MaterialSerializedReader.Read(material);

			bool hasPre10Fingerprint = reader.HasAny(Pre10FingerprintMarkers);
			string originalTag = material.GetTag(ShaderOptimizer.TAG_ORIGINAL_SHADER, false, string.Empty);
			bool looksPoiyomi = ShaderNameIsPoiyomi(material.shader.name)
				|| originalTag.IndexOf("poiyomi", StringComparison.OrdinalIgnoreCase) != -1
				|| hasPre10Fingerprint;

			if (!looksPoiyomi)
				return false;

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
						return v < Nine3;
					}
				}
			}

			// GUID dead or unreadable => original was removed. Confirm it's really Poiyomi 9.x.
			if (originalTag.IndexOf("poiyomi", StringComparison.OrdinalIgnoreCase) == -1 && !hasPre10Fingerprint)
				return false;

			info.Variant = VariantFromLockedName(material.shader.name, originalTag);
			info.DetectedVersion = ParseVersionFromTag(originalTag);
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
					return true;
				}
				return false;
			}

			// Broken/missing shader (the classic unlocked-9.2 error-shader state). Fingerprint decides.
			if (!hasPre10Fingerprint)
				return false;

			// Variant can't be recovered from a dangling GUID - only the edition, from the fingerprint.
			info.Variant = null;
			info.DetectedVersion = null; // "pre-10.0", exact version unknown
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

		static PoiyomiEdition ResolveEdition(string variant, string originalTag, MaterialSerializedReader reader)
		{
			if (!string.IsNullOrEmpty(variant))
				return variant.IndexOf(" Pro", StringComparison.OrdinalIgnoreCase) != -1 ? PoiyomiEdition.Pro : PoiyomiEdition.Toon;

			if (originalTag.IndexOf("Poiyomi Pro", StringComparison.OrdinalIgnoreCase) != -1) return PoiyomiEdition.Pro;
			if (originalTag.IndexOf("Poiyomi Toon", StringComparison.OrdinalIgnoreCase) != -1) return PoiyomiEdition.Toon;

			// No name to go by (unlocked error shader): DPS/penetrator props are Pro-only.
			return reader.HasAny(DpsMarkers) ? PoiyomiEdition.Pro : PoiyomiEdition.Toon;
		}

		// --- Helpers -----------------------------------------------------------------------------------

		static bool ShaderNameIsPoiyomi(string name) =>
			!string.IsNullOrEmpty(name) && name.IndexOf("poiyomi", StringComparison.OrdinalIgnoreCase) != -1;

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
