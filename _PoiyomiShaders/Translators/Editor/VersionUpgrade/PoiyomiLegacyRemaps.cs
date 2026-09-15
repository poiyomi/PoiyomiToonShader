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
using Thry.ThryEditor.Helpers;
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
			
			// --- 8.x Iridescence -> Matcap (UV Mode = Gradient). Only fires if iridescence was enabled. ---
    		MigrateIridescenceToMatcap(material, src);

			// --- 7.3 lighting/shading. Gated internally on the 7.3-only _LightingRampType, so it never touches 8.x/9.x. ---
			Migrate73Lighting(material, src);
		}

		/// <summary>
		/// 7.3's toon-shading config doesn't line up with 9.3 even though many props share names: the _LightingMode
		/// enum was re-indexed (so the raw value lands on the wrong mode), the separate _LightingRampType was folded
		/// into _LightingMode, and the AO/shadow/detail masks were restructured into RGBA-packed maps. This puts the
		/// material into the correct 9.3 lighting mode with its ramp, shadow and mask config intact. Only 7.3 has
		/// _LightingRampType, so the whole block is gated on it - it never runs for 8.x/9.x materials.
		/// NOTE: the specular / BRDF / reflection systems were rebuilt after 7.3 and are intentionally NOT migrated -
		/// their math changed, so they can't be reproduced; they reset to 9.3 defaults.
		/// </summary>
		static void Migrate73Lighting(Material material, Reader src)
		{
			if (!src.Has("_LightingRampType")) return; // not a 7.3 material

			ThryLogger.LogWarn($"<b>{material.name}</b>: migrated from Poiyomi 7.3. Lighting/shading was translated to 9.3, " + "be aware that specular, reflections and metallics were rebuilt after 7.3 and reset to defaults - please re-check them.");

			// _LightingMode re-index. 7.3: Toon 0, Realistic 1, Wrapped 2, Skin 3, Flat 4 (+ a separate _LightingRampType).
			// 9.3: TextureRamp 0, Multilayer Math 1, Wrapped 2, Skin 3, ShadeMap 4, Flat 5, Realistic 6, Cloth 7, SDF 8.
			// Carrying the raw value would send Realistic->Multilayer Math and Flat->ShadeMap; remap it explicitly.
			int mode73 = Mathf.RoundToInt(src.GetFloat("_LightingMode", 0f));
			int ramp73 = Mathf.RoundToInt(src.GetFloat("_LightingRampType", 0f));
			int mode93;
			switch (mode73)
			{
				case 1: mode93 = 6; break; // Realistic -> Realistic
				case 2: mode93 = 2; break; // Wrapped -> Wrapped
				case 3: mode93 = 3; break; // Skin -> Skin
				case 4: mode93 = 5; break; // Flat -> Flat
				default: // Toon: the 7.3 ramp type picks the 9.3 mode
				{
					mode93 = ramp73 == 2 ? 4 // Shade Mapping -> ShadeMap
						: ramp73 == 1 ? 1 // Math Gradient -> Multilayer Math
						: 0; // Ramp Texture  -> TextureRamp
					break;
				}
			}
			SetFloatIfPresent(material, "_LightingMode", mode93); // KeywordEnum - FixKeywords (run after Apply) sets the keyword

			// Light color modes. 7.3 _LightingDirectColorMode (Poi Custom 0 / Correct 1) -> 9.3 _LightingColorMode
      		// (Poi Custom 0 / Standard 1 / ...); values 0/1 line up. Indirect toggle -> the 9.3 strength.
			CopyFloat(src, material, "_LightingDirectColorMode", "_LightingColorMode");
			CopyFloat(src, material, "_LightingIndirectColorMode", "_LightingIndirectUsesNormals");

			// AO map: single texture -> RGBA-packed maps, occupying the R channel (strength from the 7.3 enable toggle).
			CopyTexture(src, material, "_LightingAOTex", "_LightingAOMaps");
			CopyVector(src, material, "_LightingAOTexPan", "_LightingAOMapsPan");
			CopyFloat(src, material, "_LightingAOTexUV", "_LightingAOMapsUV");
			SetFloatIfPresent(material, "_LightDataAOStrengthR", src.GetFloat("_LightingEnableAO", 0f) > 0.5f ? 1f : 0f);

			// Ramp/shadow mask -> shadow masks R channel.
			CopyTexture(src, material, "_LightingShadowMask", "_LightingShadowMasks");
			CopyVector(src, material, "_LightingShadowMaskPan", "_LightingShadowMasksPan");
			CopyFloat(src, material, "_LightingShadowMaskUV", "_LightingShadowMasksUV");
			SetFloatIfPresent(material, "_LightingShadowMaskStrengthR", 1f);

			// Detail shadows -> detail shadow maps R channel, carrying the 7.3 detail strength.
			CopyTexture(src, material, "_LightingDetailShadows", "_LightingDetailShadowMaps");
			CopyVector(src, material, "_LightingDetailShadowsPan", "_LightingDetailShadowMapsPan");
			CopyFloat(src, material, "_LightingDetailShadowsUV", "_LightingDetailShadowMapsUV");
			SetFloatIfPresent(material, "_LightingDetailShadowStrengthR", src.GetFloat("_LightingDetailShadowsEnabled", 0f) > 0.5f ? src.GetFloat("_LightingDetailStrength", 1f) : 0f);
		} 

		/// <summary>
		/// 8.x Iridescence was removed in 9.x; the same look is reproduced by a Matcap with UV Mode = Gradient
		/// (the iridescence ramp goes in the matcap texture slot). Migrates the settings onto a matcap slot: the
		/// base slot if the material's own matcap is unused, else the first free numbered slot (2-4). No-op if
		/// iridescence is off; warns and skips if every matcap slot is already in use.
		/// </summary>
		static void MigrateIridescenceToMatcap(Material material, Reader src)
		{
			string slot = ChooseMatcapSlotForIridescence(src, out bool iridescenceOn, out bool allSlotsFull);
			if (!iridescenceOn)
				return;
			if (slot == null)
			{
				if (allSlotsFull)
				ThryLogger.LogWarn($"<b>{material.name}</b>: iridescence couldn't be migrated - every matcap slot is already in use. " +
					"Re-create it as a matcap (UV Mode = Gradient) manually.");
				return;
			}

			// Normal/AudioLink sub-props live under _Matcap0* on the base slot, but under _Matcap{N}* on numbered slots.
			string sub = SubPrefix(slot);

			// Enable + gradient mode (the crux). Keyword (e.g. POI_MATCAP0) is set by FixKeywords after Apply.
			SetFloatIfPresent(material, slot + "Enable", 1f);
			SetFloatIfPresent(material, slot + "UVMode", 3f); // Gradient

			// Core look: ramp -> matcap texture slot (the prefix itself), panning, intensity, emission.
			CopyTexture(src, material, "_IridescenceRamp", slot);
			CopyVector(src, material, "_IridescenceRampPan", slot + "Pan");
			CopyFloat(src, material, "_IridescenceIntensity", slot + "Intensity");   // 0-10 vs 0-5 UI range; value copied as-is
			CopyFloat(src, material, "_IridescenceEmissionStrength", slot + "EmissionStrength");

			// Mask.
			CopyTexture(src, material, "_IridescenceMask", slot + "Mask");
			CopyVector(src, material, "_IridescenceMaskPan", slot + "MaskPan");
			CopyFloat(src, material, "_IridescenceMaskUV", slot + "MaskUV");

			// Blend toggles - blendMatcap() reads Add/Replace/Multiply directly for base AND numbered slots.
			CopyFloat(src, material, "_IridescenceAddBlend", slot + "Add");
			CopyFloat(src, material, "_IridescenceReplaceBlend", slot + "Replace");
			CopyFloat(src, material, "_IridescenceMultiplyBlend", slot + "Multiply");

			// Hue shift.
			CopyFloat(src, material, "_IridescenceHueShiftEnabled", slot + "HueShiftEnabled");
			CopyFloat(src, material, "_IridescenceHueShiftSpeed", slot + "HueShiftSpeed");
			CopyFloat(src, material, "_IridescenceHueShift", slot + "HueShift");

			// Custom normal + AudioLink emission.
			CopyFloat(src, material, "_IridescenceNormalToggle", sub + "CustomNormal");
			CopyTexture(src, material, "_IridescenceNormalMap", sub + "NormalMap");
			CopyVector(src, material, "_IridescenceNormalMapPan", sub + "NormalMapPan");
			CopyFloat(src, material, "_IridescenceNormalMapUV", sub + "NormalMapUV");
			CopyFloat(src, material, "_IridescenceNormalIntensity", sub + "NormalMapScale");
			CopyVector(src, material, "_IridescenceAudioLinkEmissionAdd", sub + "ALEmissionAdd");
			CopyFloat(src, material, "_IridescenceAudioLinkEmissionAddBand", sub + "ALEmissionAddBand");
			// _IridescenceNormalSelection (Vertex/Pixel) has no matcap equivalent - dropped.
		}

		// Returns the matcap slot prefix ("_Matcap", "_Matcap2".."_Matcap4") to migrate iridescence into, or null
		// (iridescence off, or all slots used). "Free" = that slot's Enable was 0 on the source material.
		static string ChooseMatcapSlotForIridescence(Reader src, out bool iridescenceOn, out bool allSlotsFull)
		{
			allSlotsFull = false;
			iridescenceOn = src.GetFloat("_EnableIridescence", 0f) > 0.5f;
			if (!iridescenceOn)
				return null;

			if (src.GetFloat("_MatcapEnable", 0f) <= 0.5f)
				return "_Matcap";                                   // base slot free
			for (int n = 2; n <= 4; n++)
				if (src.GetFloat($"_Matcap{n}Enable", 0f) <= 0.5f)
				return $"_Matcap{n}";                           // first free numbered slot

			allSlotsFull = true;
			return null;
		}

		static string SubPrefix(string slot) => slot == "_Matcap" ? "_Matcap0" : slot;

		static void SetFloatIfPresent(Material material, string name, float value)
		{
			if (material.HasProperty(name))
				material.SetFloat(name, value);
		}

		/// <summary>
		/// Locked materials strip textures to _stripped_tex_&lt;prop&gt; tags on lock and restore them by name
		/// on unlock. Rename that tag so a stripped 8.x texture restores onto its 9.3 name (which the target shader
		/// declares) instead of the vanished old name. Call BEFORE unlocking.
		/// </summary>
		public static void RenameStrippedTextureTags(Material material, Reader src)
		{
			if (material == null)
				return;

			RenameStrippedTag(material, "_ClippingMask", "_AlphaMask");

			// Iridescence textures are stripped on lock too; re-target them onto the same matcap slot the migration
			// will use, so they restore onto it during unlock. Same slot decision (from the pre-unlock snapshot),
			// so this and MigrateIridescenceToMatcap agree.
			if (src == null)
				return;

			string slot = ChooseMatcapSlotForIridescence(src, out bool iridescenceOn, out _);
			if (!iridescenceOn || slot == null)
				return;

			string sub = SubPrefix(slot);
			RenameStrippedTag(material, "_IridescenceRamp", slot);
			RenameStrippedTag(material, "_IridescenceMask", slot + "Mask");
			RenameStrippedTag(material, "_IridescenceNormalMap", sub + "NormalMap");
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
