using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Poi.Tools.ShaderTranslator.VersionUpgrade
{
	public static class PoiyomiVersionDetector
	{
		public static readonly Version LatestVersion = new Version(10, 0);
		public const string PoiyomiShaderPrefix = ".poiyomi/";
		const string LockedShaderPrefix = "Hidden/Locked/";

		// Matches the label version. Modern labels are "Poiyomi 10.0.13"; 7.3 uses the older "Poiyomi Toon V7.3.050"
		// form, so tolerate an optional "Toon " word and a "V" prefix before the number.
		static readonly Regex VersionRegex = new Regex(@"Poiyomi\s+(?:Toon\s+)?V?(\d+)\.(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public static bool TryGetVersion(Material material, out Version version)
		{
			version = null;
			if (material == null || material.shader == null)
				return false;

			Shader shader = GetEffectiveShader(material);
			if (shader == null)
				return false;

			return TryGetVersionFromShader(shader, out version);
		}

		public static bool TryGetVersionFromShader(Shader shader, out Version version)
		{
			version = null;
			if (shader == null) return false;

			// Direct index lookup instead of walking every property by name: a Poiyomi shader declares ~4900, and
			// this runs on every material editor creation via the legacy detector, i.e. per thumbnail regeneration.
			// Shader.FindPropertyIndex and GetPropertyDescription exist since 2019.3.
			int index = shader.FindPropertyIndex("shader_master_label");
			if (index < 0) return false;
			return TryParseVersionFromLabel(shader.GetPropertyDescription(index), out version);
		}

		public static bool TryParseVersionFromLabel(string label, out Version version)
		{
			version = null;
			if (string.IsNullOrEmpty(label))
				return false;

			var match = VersionRegex.Match(label);
			if (!match.Success)
				return false;

			if (int.TryParse(match.Groups[1].Value, out int major) &&
				int.TryParse(match.Groups[2].Value, out int minor))
			{
				version = new Version(major, minor);
				return true;
			}
			return false;
		}

		public static Shader GetEffectiveShader(Material material)
		{
			if (material == null)
				return null;

			if (material.shader.name.StartsWith(LockedShaderPrefix, StringComparison.OrdinalIgnoreCase))
			{
				Shader originalShader = Thry.ThryEditor.ShaderOptimizer.GetOriginalShader(material, false);
				if (originalShader != null)
					return originalShader;
			}
			return material.shader;
		}

		/// <summary>
		/// True only for a shader name Poiyomi actually ships, i.e. one declared under ".poiyomi/". The
		/// "Hidden/Locked/" wrapper a locked material carries is stripped first, so a locked Poiyomi material still
		/// answers true from its own shader name even when the original shader asset can no longer be resolved.
		///
		/// Deliberately NOT a substring search for "poiyomi": third-party shaders, forks and user copies routinely
		/// carry the word somewhere in their name or folder, and matching on that made the upgrade tools claim
		/// non-Poiyomi materials and convert them onto a Poiyomi shader.
		/// </summary>
		public static bool IsPoiyomiShaderName(string shaderName)
		{
			if (string.IsNullOrEmpty(shaderName)) return false;

			string name = shaderName.StartsWith(LockedShaderPrefix, StringComparison.OrdinalIgnoreCase) ? shaderName.Substring(LockedShaderPrefix.Length) : shaderName;

			return name.StartsWith(PoiyomiShaderPrefix, StringComparison.OrdinalIgnoreCase);
		}

		public static bool IsPoiyomiShader(Material material)
		{
			if (material == null || material.shader == null) return false;

			if (IsPoiyomiShaderName(material.shader.name)) return true;

			Shader shader = GetEffectiveShader(material);
			return shader != null && IsPoiyomiShaderName(shader.name);
		}

		public static bool NeedsUpgrade(Material material)
		{
			// Materials on a present, readable Poiyomi shader older than the latest.
      		if (IsPoiyomiShader(material) && TryGetVersion(material, out Version version) && version < LatestVersion) return true;

			// Removed-version materials (8.x/9.0-9.2 on the error shader, or a locked material mis-resolving to 10.0)
			// aren't caught above - the legacy detector recognizes them from serialized fingerprints/tags.
			return LegacyMaterialDetector.NeedsLegacyUpgrade(material);
		}

		// Serialized markers only Grab Pass materials carry, used to recognize one whose shader has been
		// removed.
		static readonly string[] GrabPassMarkers = { "_GrabPassBlendGlobalMask", "_GrabPassUseAlpha", "_GrabPassUseAlpha" };

		/// <summary>
		/// Outputs true if a shader name is one of Poiyomi's Grab Pass / refraction shaders (Toon/Pro/URP Grab Pass, Pro Self
		/// Grab). Grab Pass was heavily reworked between 9.3 and 10.0, so upgrades of these materials need a heads-up.
		/// </summary>
		public static bool IsGrabPassShaderName(string shaderName)
		{
			return !string.IsNullOrEmpty(shaderName) && shaderName.IndexOf("Grab", StringComparison.OrdinalIgnoreCase) != -1;
		}

		/// <summary>
		/// Outputs true if this material renders with a Poiyomi Grab Pass shader. Resolves the original shader behind a locked
		/// material, falls back to the stored OriginalShader tag when the baked shader is gone, and finally sniffs the
		/// serialized properties so a removed-shader (error-shader) Grab Pass material is still recognized.
		/// </summary>
		public static bool UsesGrabPass(Material material)
		{
			if (material == null || material.shader == null) return false;

			if (IsGrabPassShaderName(material.shader.name)) return true;

			Shader effective = GetEffectiveShader(material);
			if (effective != null && IsGrabPassShaderName(effective.name)) return true;

			// Locked material whose baked shader was removed: the original name still survives as a tag.
			if (IsGrabPassShaderName(material.GetTag("OriginalShader", false, ""))) return true;

			// Unlocked material on the error shader (its removed shader is gone): name and tag are both unavailable, but
			// the Grab Pass properties are still serialized. Only sniff in this case, to avoid a serialized read per
			// material when the cheaper checks above are already authoritative.
			if (material.shader.name.IndexOf("InternalErrorShader", StringComparison.OrdinalIgnoreCase) != -1) return LegacyMaterialDetector.MaterialSerializedReader.Read(material).HasAny(GrabPassMarkers);

			return false;
		}

		public static string GetShaderVariant(Shader shader)
		{
			if (shader == null)
				return null;

			string name = shader.name;

			// Find last "Poiyomi" - the variant always starts with it (not .poiyomi in path)
			int poiIndex = name.LastIndexOf("Poiyomi", StringComparison.OrdinalIgnoreCase);
			if (poiIndex < 0)
				return null;

			name = name.Substring(poiIndex);

			// For locked shaders, strip the guid suffix (everything after /)
			int slashIndex = name.IndexOf('/');
			if (slashIndex > 0)
				name = name.Substring(0, slashIndex);

			return name;
		}

		public static bool IsProShader(Shader shader)
		{
			if (shader == null)
				return false;

			string variant = GetShaderVariant(shader);
			return variant != null && variant.IndexOf(" Pro", StringComparison.OrdinalIgnoreCase) != -1;
		}

		// Normalize so that settings can be carried over.
		const string ShaderPrefix = "Poiyomi ";
		const string EditionPlaceholder = "{edition}";
		static readonly string[] EditionTokens = { "Toon", "Pro" };

		// Return edition of the shader variant. Null if not recognized.
		public static string GetShaderEdition(Shader shader) => GetEditionFromVariant(GetShaderVariant(shader));

		static string GetEditionFromVariant(string variant)
		{
			if (string.IsNullOrEmpty(variant) || !variant.StartsWith(ShaderPrefix, StringComparison.OrdinalIgnoreCase)) return null;

			string rest = variant.Substring(ShaderPrefix.Length);
			foreach (string edition in EditionTokens)
			{
				if (rest.Equals(edition, StringComparison.OrdinalIgnoreCase) || rest.StartsWith(edition + " ", StringComparison.OrdinalIgnoreCase)) return edition;
			}
			return null;
		}

		// Return an agnostic key for the shader's variant, so that the Toon/Pro versions have cross-supported translation values.
		// This fixes a bug where the PoiyomiAutoUpgradeOnShaderSwap didn't account for this scenario.
		public static string GetVariantFamily(Shader shader)
		{
			string variant = GetShaderVariant(shader);
			string edition = GetEditionFromVariant(variant);
			if (edition == null) return null;

			string suffix = variant.Substring(ShaderPrefix.Length + edition.Length);
			return ShaderPrefix + EditionPlaceholder + suffix;
		}

		// Set true when two shaders are the same sub-variant in different editions. Values should transfer cleanly across the swap,
		// with only available features being different.
		public static bool IsEditionSwap(Shader oldShader, Shader newShader)
		{
			string oldEdition = GetShaderEdition(oldShader);
			string newEdition = GetShaderEdition(newShader);
			if (oldEdition == null || newEdition == null) return false;

			// IF the swap is the same edition (which isn't an edition swap), return false.
			if (string.Equals(oldEdition, newEdition, StringComparison.OrdinalIgnoreCase)) return false;

			string oldFamily = GetVariantFamily(oldShader);
			string newFamily = GetVariantFamily(newShader);

			return oldFamily != null && string.Equals(oldFamily, newFamily, StringComparison.OrdinalIgnoreCase);
		}
	}
}
