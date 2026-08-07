using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Poi.Tools.ShaderTranslator.VersionUpgrade
{
	public static class PoiyomiVersionDetector
	{
		public static readonly Version LatestVersion = new Version(10, 0);

		static readonly Regex VersionRegex = new Regex(@"Poiyomi\s+(\d+)\.(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
			if (shader == null)
				return false;

#if UNITY_6000_0_OR_NEWER
			int propCount = shader.GetPropertyCount();
#else
			int propCount = ShaderUtil.GetPropertyCount(shader);
#endif
			for (int i = 0; i < propCount; i++)
			{
#if UNITY_6000_0_OR_NEWER
				string propName = shader.GetPropertyName(i);
#else
				string propName = ShaderUtil.GetPropertyName(shader, i);
#endif
				if (propName != "shader_master_label")
					continue;

#if UNITY_6000_0_OR_NEWER
				string description = shader.GetPropertyDescription(i);
#else
				string description = ShaderUtil.GetPropertyDescription(shader, i);
#endif
				return TryParseVersionFromLabel(description, out version);
			}
			return false;
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

			if (material.shader.name.StartsWith("Hidden/Locked/", StringComparison.OrdinalIgnoreCase))
			{
				Shader originalShader = Thry.ThryEditor.ShaderOptimizer.GetOriginalShader(material, false);
				if (originalShader != null)
					return originalShader;
			}
			return material.shader;
		}

		public static bool IsPoiyomiShader(Material material)
		{
			if (material == null || material.shader == null)
				return false;

			Shader shader = GetEffectiveShader(material);
			return shader != null && shader.name.IndexOf("poiyomi", StringComparison.OrdinalIgnoreCase) != -1;
		}

		public static bool NeedsUpgrade(Material material)
		{
			if (!IsPoiyomiShader(material))
				return false;

			if (!TryGetVersion(material, out Version version))
				return false;

			return version < LatestVersion;
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
