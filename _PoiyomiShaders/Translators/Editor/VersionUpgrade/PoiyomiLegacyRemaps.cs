// 9.0-9.2 -> 9.3 needs none of this (identical layout). These remaps only fire when the old property is
// actually present in the material's serialized sheet, so running them on a 9.x material is a harmless no-op.
//
// Everything is read from a serialized snapshot (by name), so it works whether or not the 8.x source shader
// ships - matching the rest of the legacy pipeline. Confirmed against the restored 8.0 shaders:
//   _ClippingMask family  -> _AlphaMask family   (rename; alpha/transparency mask - correctness critical)
//   _CubeMapAdd/Multiply   -> _CubemapBlendType   (Replace=0, Multiply=1, Add=2)
//   _FlipbookAdd/Multiply  -> _FlipbookBlendType  (Replace=0, Multiply=2, Add=8)
//   _ParallaxUV* toggles   -> _ParallaxUV enum    (defensive; commented out in 8.0, active in 8.1/8.2)

using UnityEngine;
using Reader = Poi.Tools.ShaderTranslator.VersionUpgrade.LegacyMaterialDetector.MaterialSerializedReader;

namespace Poi.Tools.ShaderTranslator.VersionUpgrade
{
	public static class PoiyomiLegacyRemaps
	{
		const string StrippedTexTagPrefix = "_stripped_tex_";

		/// <summary>
		/// Apply the pre-9.0 -> 9.3 property remaps. <paramref name="material"/> must already be on the 9.3 shader;
		/// <paramref name="src"/> is a snapshot of the material's serialized values taken BEFORE the shader change,
		/// so renamed 8.x properties can still be read by name.
		/// </summary>
		public static void Apply(Material material, Reader src)
		{
			if (material == null || src == null)
				return;

			// --- 8.x alpha mask: _ClippingMask family -> _AlphaMask family (types line up 1:1) ---
			CopyTexture(src, material, "_ClippingMask", "_AlphaMask");
			CopyVector(src, material, "_ClippingMaskPan", "_AlphaMaskPan");
			CopyFloat(src, material, "_ClippingMaskUV", "_AlphaMaskUV");   // UV enum values 0-7 match
			CopyFloat(src, material, "_Inverse_Clipping", "_AlphaMaskInvert");

			// --- 8.x mutually-exclusive blend toggles -> 9.3 blend enums ---
			SetEnumFromToggles(material, src, "_CubemapBlendType", ("_CubeMapAdd", 2f), ("_CubeMapMultiply", 1f));
			SetEnumFromToggles(material, src, "_FlipbookBlendType", ("_FlipbookAdd", 8f), ("_FlipbookMultiply", 2f));

			// --- 8.x parallax UV toggles -> _ParallaxUV enum (values align with 9.3) ---
			SetEnumFromToggles(material, src, "_ParallaxUV",
				("_ParallaxUV1", 1f), ("_ParallaxUV2", 2f), ("_ParallaxUV3", 3f),
				("_ParallaxPano", 4f), ("_ParallaxWorldPos", 5f), ("_ParallaxPolar", 6f), ("_ParallaxDist", 7f));
		}

		/// <summary>
		/// Locked materials strip textures to _stripped_tex_&lt;prop&gt; tags on lock and restore them by name
		/// on unlock. Rename that tag so a stripped 8.x texture restores onto its 9.3 name (which the target shader
		/// declares) instead of the vanished old name. Call BEFORE unlocking.
		/// </summary>
		public static void RenameStrippedTextureTags(Material material)
		{
			if (material == null)
				return;

			RenameStrippedTag(material, "_ClippingMask", "_AlphaMask");
		}

		// --- copy helpers: only write when the source was present AND the 9.3 shader declares the target ---

		static void CopyTexture(Reader src, Material material, string srcName, string dstName)
		{
			if (!material.HasProperty(dstName) || !src.TryGetTexture(srcName, out Texture tex, out Vector4 st))
				return;

			material.SetTexture(dstName, tex);
			material.SetTextureScale(dstName, new Vector2(st.x, st.y));
			material.SetTextureOffset(dstName, new Vector2(st.z, st.w));
		}

		static void CopyVector(Reader src, Material material, string srcName, string dstName)
		{
			if (material.HasProperty(dstName) && src.TryGetVector(srcName, out Vector4 v))
				material.SetVector(dstName, v);
		}

		static void CopyFloat(Reader src, Material material, string srcName, string dstName)
		{
			if (material.HasProperty(dstName) && src.TryGetFloat(srcName, out float f))
				material.SetFloat(dstName, f);
		}

		// Pick the enum value for the first toggle that's on. If none is on, leave the target at its 9.3 shader
		// default (Replace / UV0). SetFloat is used directly so the value isn't clamped by the target's Range().
		static void SetEnumFromToggles(Material material, Reader src, string enumName, params (string toggle, float value)[] options)
		{
			if (!material.HasProperty(enumName))
				return;

			foreach (var (toggle, value) in options)
			{
				if (src.GetFloat(toggle, 0f) > 0.5f)
				{
					material.SetFloat(enumName, value);
					return;
				}
			}
		}

		static void RenameStrippedTag(Material material, string srcName, string dstName)
		{
			string tagSrc = StrippedTexTagPrefix + srcName;
			string guid = material.GetTag(tagSrc, false, string.Empty);
			if (string.IsNullOrEmpty(guid))
				return;

			material.SetOverrideTag(StrippedTexTagPrefix + dstName, guid);
			material.SetOverrideTag(tagSrc, string.Empty); // clear the old so unlock doesn't also target the gone prop
		}
	}
}
