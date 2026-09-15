// Automated Shader Translation designed by BluWizard LABS.
// https://github.com/BluWizard10

using System;
using System.Collections.Generic;
using System.IO;
using Thry.ThryEditor.Helpers;
using Thry.ThryEditor.TexturePacker;
using UnityEditor;
using UnityEngine;

namespace Poi.Tools.ShaderTranslator.Translations
{
    /// <summary>
    /// Translates Unity's built-in Standard shader family onto Poiyomi.
    ///
    /// Unity Standard and Poiyomi share a surprising number of property names (_Color, _MainTex, _BumpMap,
    /// _BumpScale, _Cutoff, _EmissionColor, _EmissionMap, _DetailNormalMap, _DetailNormalMapScale, _Mode),
    /// so those values survive the shader swap on their own and need no translation entry. This class covers
    /// the rest: the PBR maps that move into Reflections &amp; Specular, occlusion, the detail stack, emission
    /// and the lighting model.
    ///
    /// A few names collide with *different* meanings on the Poiyomi side (_MetallicGlossMap, _DetailMask,
    /// _Mode) - those are corrected here rather than left to carry over wrongly.
    /// </summary>
    public class UnityStandardToPoiyomiToonTranslation : ScriptedShaderTranslator
    {
        /// <summary>Poiyomi's Shadows > Lighting Type dropdown (_LightingMode).</summary>
        enum PoiLightingMode
        {
            TextureRamp = 0,
            MultilayerMath = 1,
            Wrapped = 2,
            Skin = 3,
            ShadeMap = 4,
            Flat = 5,
            Realistic = 6,
            Cloth = 7,
            SDF = 8
        }

        /// <summary>Channel picker used by Poiyomi's packed map properties. "White" ignores the map entirely.</summary>
        enum PoiMapChannel { R = 0, G = 1, B = 2, A = 3, White = 4 }

        const string StandardShaderName = "Standard";
        const string SpecularSetupShaderName = "Standard (Specular setup)";

        static readonly Vector4 IdentityScaleOffset = new Vector4(1, 1, 0, 0);

        /// <summary>
        /// Unity's Standard family, as shipped with the editor. "Standard" is the metallic workflow this
        /// translation targets in full; the Specular setup declares the same albedo/normal/occlusion/emission/
        /// detail properties, so it's accepted too and only its specular map warns instead of failing.
        /// </summary>
        public static bool IsUnityStandardShader(Shader shader)
        {
            if (shader == null) return false;

            return string.Equals(shader.name, StandardShaderName, StringComparison.Ordinal)
                || string.Equals(shader.name, SpecularSetupShaderName, StringComparison.Ordinal);
        }

        static bool IsSpecularSetup(Shader shader) => shader != null && string.Equals(shader.name, SpecularSetupShaderName, StringComparison.Ordinal);

        public override bool CanTranslateMaterial(Material sourceMaterial)
        {
            return sourceMaterial != null && IsUnityStandardShader(sourceMaterial.shader);
        }

        protected override void DoBeforeTranslation(TranslationContext context)
        {
            // Unity's _Mode has the same name as Poiyomi's Rendering Preset dropdown and its first four values
            // line up (Opaque, Cutout, Fade, Transparent), so the raw number carries over on the swap - but only
            // the dropdown, not the blend state, ZWrite and render queue that belong with it. Run the preset for
            // real so the two agree. Opaque is skipped: it's already Poiyomi's default and Unity leaves the same
            // _SrcBlend/_DstBlend/_ZWrite values behind.
            int mode = Mathf.RoundToInt(GetSourcePropertyValue<float>(context, "_Mode"));
            switch (mode)
            {
                case 1: SetTargetRenderingPreset(context, PoiShaderRenderingPreset.Cutout); break;
                case 2: SetTargetRenderingPreset(context, PoiShaderRenderingPreset.Fade); break;
                case 3: SetTargetRenderingPreset(context, PoiShaderRenderingPreset.Transparent); break;
            }
        }

        protected override void DoAfterTranslation(TranslationContext context)
        {
            TranslateReflectionsAndSpecular(context);
            TranslateDetails(context);
            TranslateEmission(context);
            TranslateSecondaryMapTiling(context);

            // Unity Standard is a physically based shader with no toon ramp of any kind, so Realistic is the
            // only shading mode that reproduces it. Set as a float - FixKeywords (run once the translation
            // finishes) turns it into the matching _LIGHTINGMODE_* keyword.
            SetTargetPropertyValue(context, "_LightingMode", (float)PoiLightingMode.Realistic);

            // Manually restore render queue
            SetTargetRenderQueue(context, context.originalRenderQueue);
        }

        protected override List<PropertyTranslation> AddProperties()
        {
            return new List<PropertyTranslation>()
            {
                #region Detail Normals & Texture
                // Unity's "Detail Albedo x2" is Poiyomi's Detail Texture: both multiply the base color by the
                // detail sample doubled through unity_ColorSpaceDouble, so the maps swap over untouched.
                // _DetailNormalMap and _DetailNormalMapScale share their names with Poiyomi and carry over
                // by themselves. Poiyomi's _DetailTexIntensity/_DetailBrightness already default to 1, which
                // is exactly Unity's blend, so they're deliberately left alone.
                new PropertyTranslation("_DetailAlbedoMap", "_DetailTex", HasDetailMaps),

                // Unity transforms the detail UV with _DetailAlbedoMap_ST only and feeds the result to *both*
                // detail maps, so Poiyomi's separate detail normal tiling has to come from it as well.
                new PropertyTranslation("_DetailAlbedoMap_ST", "_DetailTex_ST", HasDetailMaps),
                new PropertyTranslation("_DetailAlbedoMap_ST", "_DetailNormalMap_ST", HasDetailMaps),

                // "UV Set for secondary textures" (UV0 / UV1) applies to both detail maps. The values line up
                // with the first two entries of Poiyomi's UV dropdown.
                new PropertyTranslation("_UVSec", "_DetailTexUV", HasDetailMaps),
                new PropertyTranslation("_UVSec", "_DetailNormalMapUV", HasDetailMaps),
                #endregion

                #region Occlusion
                new PropertyTranslation("_OcclusionMap", "_LightingAOMaps", HasOcclusionMap, (prop, context) =>
                {
                    // Unity's Occlusion() reads the map's GREEN channel, so drive Poiyomi's G strength with
                    // Unity's Occlusion Strength and clear R, which is the channel Poiyomi defaults to.
                    SetTargetPropertyValue(context, "_LightDataAOStrengthR", 0f);
                    SetTargetPropertyValue(context, "_LightDataAOStrengthG", GetSourcePropertyValue<float>(context, "_OcclusionStrength"));
                }),
                #endregion
            };
        }

        #region Reflections & Specular

        /// <summary>
        /// Moves Unity's metallic/smoothness workflow into Poiyomi's Reflections &amp; Specular (Mochie BRDF)
        /// module. Poiyomi multiplies a scalar by one channel of a single packed map
        /// (metallic = _MochieMetallicMultiplier * map[metallicChannel], likewise for smoothness), which lines
        /// up with Unity's _MetallicGlossMap layout of metallic in R and smoothness in A.
        /// </summary>
        void TranslateReflectionsAndSpecular(TranslationContext context)
        {
            bool isSpecularSetup = IsSpecularSetup(SourceShader.Shader);

            var metallicMap = GetSourcePropertyValue<Texture>(context, "_MetallicGlossMap");
            var specGlossMap = GetSourcePropertyValue<Texture>(context, "_SpecGlossMap");

            // Poiyomi declares its own _MetallicGlossMap (Stylized Reflections, which stays disabled here), so
            // Unity's texture lands in that slot on the swap. Drop it now that it lives in the packed map, or
            // the material keeps a second reference to it for no reason.
            SetTargetPropertyValue(context, "_MetallicGlossMap", null);

            // Both are [ToggleOff] floats on every Standard variant, on by default.
            bool specularOn = GetSourcePropertyValue<float>(context, "_SpecularHighlights") > 0.5f;
            bool reflectionsOn = GetSourcePropertyValue<float>(context, "_GlossyReflections") > 0.5f;

            if (!specularOn && !reflectionsOn)
            {
                ThryLogger.LogDetail($"<b>{context.Material.name}</b>: Specular Highlights and Reflections are both off, leaving Reflections & Specular disabled.");
                return;
            }

            SetTargetPropertyValue(context, "_MochieBRDF", 1f);
            SetTargetPropertyValue(context, "_MochieSpecularStrength", specularOn ? 1f : 0f);
            SetTargetPropertyValue(context, "_MochieReflectionStrength", reflectionsOn ? 1f : 0f);

            // Smoothness always comes out of the map's alpha, whichever variant we're translating. Poiyomi
            // falls back to a white texture when the slot is empty, so this is safe to set either way and the
            // multipliers below pass straight through.
            SetTargetPropertyValue(context, "_MochieMetallicMapsRoughnessChannel", (float)PoiMapChannel.A);

            if (isSpecularSetup)
            {
                // Specular workflow: _SpecGlossMap is specular color in RGB and smoothness in A. Poiyomi's BRDF
                // is metallic-only, so the smoothness half maps exactly and the specular tint has nowhere to go.
                SetTargetPropertyValue(context, "_MochieMetallicMaps", specGlossMap);
                SetTargetPropertyValue(context, "_MochieMetallicMapsMetallicChannel", (float)PoiMapChannel.White);
                SetTargetPropertyValue(context, "_MochieMetallicMultiplier", 0f);
                SetTargetPropertyValue(context, "_MochieRoughnessMultiplier", specGlossMap != null
                    ? GetSourcePropertyValue<float>(context, "_GlossMapScale")
                    : GetSourcePropertyValue<float>(context, "_Glossiness"));

                ThryLogger.LogWarn($"<b>{context.Material.name}</b> uses Standard (Specular setup). Smoothness carried over, but Poiyomi's Reflections & Specular is a metallic workflow - the Specular color and its map's RGB could not be translated and Metallic was left at 0. Please re-check the material.");
                return;
            }

            // Metallic workflow: _MetallicGlossMap is metallic in R and smoothness in A, exactly the layout
            // Poiyomi's packed map expects.
            SetTargetPropertyValue(context, "_MochieMetallicMaps", metallicMap);
            SetTargetPropertyValue(context, "_MochieMetallicMapsMetallicChannel", (float)PoiMapChannel.R);

            // With a map assigned Unity ignores the Metallic slider entirely and scales smoothness by
            // _GlossMapScale instead of _Glossiness; without one it uses the two sliders directly.
            SetTargetPropertyValue(context, "_MochieMetallicMultiplier", metallicMap != null
                ? 1f
                : GetSourcePropertyValue<float>(context, "_Metallic"));

            SetTargetPropertyValue(context, "_MochieRoughnessMultiplier", metallicMap != null
                ? GetSourcePropertyValue<float>(context, "_GlossMapScale")
                : GetSourcePropertyValue<float>(context, "_Glossiness"));

            // Standard can pull smoothness from the albedo's alpha instead of the metallic map's. Poiyomi's
            // packed map can only pick a channel, not a different texture, so that has to be done by hand.
            if (GetSourcePropertyValue<float>(context, "_SmoothnessTextureChannel") > 0.5f)
            {
                ThryLogger.LogWarn($"<b>{context.Material.name}</b> took its smoothness from the Albedo texture's Alpha channel. Poiyomi reads smoothness from the packed Reflections & Specular map, so the Smoothness Channel was left on Alpha of that map - please re-check it.");
            }
        }

        #endregion

        #region Details

        /// <summary>
        /// Enables the Detail Normals &amp; Texture module when Unity had either detail map assigned. The maps
        /// themselves are handled by the translations in <see cref="AddProperties"/>.
        /// </summary>
        void TranslateDetails(TranslationContext context)
        {
            if (!HasDetailMaps(context)) return;

            SetTargetPropertyValue(context, "_DetailEnabled", 1f);
            PackDetailMask(context);
        }

        /// <summary>
        /// Unity reads its detail mask from the ALPHA channel and applies it to both detail maps; Poiyomi reads
        /// the texture mask from R and the normal mask from G. Repack alpha into both so the mask keeps working
        /// - carrying the texture over untouched would mask by whatever happens to sit in its red channel.
        /// </summary>
        void PackDetailMask(TranslationContext context)
        {
            var mask = GetSourcePropertyValue<Texture>(context, "_DetailMask") as Texture2D;
            if (mask == null) return;

            var config = TexturePackerConfig.GetNewConfig();
            config.Sources[0].SetInputTexture(mask);
            config.Connections.Add(new Connection(0, TextureChannelIn.A, TextureChannelOut.R)); // texture mask
            config.Connections.Add(new Connection(0, TextureChannelIn.A, TextureChannelOut.G)); // normal mask

            // Unused channels fall back to white so nothing else reading this map gets masked out.
            config.Targets[0] = new OutputTarget(BlendMode.Max, InvertMode.None, 1); // R
            config.Targets[1] = new OutputTarget(BlendMode.Max, InvertMode.None, 1); // G
            config.Targets[2] = new OutputTarget(BlendMode.Max, InvertMode.None, 1); // B
            config.Targets[3] = new OutputTarget(BlendMode.Max, InvertMode.None, 1); // A

            config.FileOutput.ColorSpace = ColorSpace.Linear;
            Packer.DetermineOutputResolution(config);
            Packer.DeterminePathAndFileNameIfEmpty(config);
            config.FileOutput.FileName = Path.GetFileNameWithoutExtension(config.FileOutput.FileName) + "_detailMask";

            Texture2D packed = Packer.Pack(config);
            if (packed == null)
            {
                ThryLogger.LogWarn($"Failed to repack the detail mask for material <b>{context.Material.name}</b>. The original mask was kept, but Poiyomi reads it from R/G while Unity used its Alpha - please check the Detail Mask.");
                SetTargetPropertyValue(context, "_DetailMask", mask);
                return;
            }

            string saveFolder = config.FileOutput.SaveFolder;
            if (!Directory.Exists(saveFolder))
                Directory.CreateDirectory(saveFolder);

            string savePath = Path.Combine(saveFolder, config.FileOutput.FileName + ".png");
            File.WriteAllBytes(savePath, packed.EncodeToPNG());
            AssetDatabase.ImportAsset(savePath);

            TextureImporter importer = AssetImporter.GetAtPath(savePath) as TextureImporter;
            if (importer != null)
            {
                importer.sRGBTexture = false; // Linear for mask data
                importer.streamingMipmaps = true;
                importer.textureCompression = TextureImporterCompression.Compressed;
                config.SaveToImporter(importer);
                importer.SaveAndReimport();
            }

            Texture2D packedAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(savePath);
            SetTargetPropertyValue(context, "_DetailMask", packedAsset != null ? packedAsset : mask);
        }

        #endregion

        #region Emission

        /// <summary>
        /// Unity's emission is simply _EmissionColor * _EmissionMap, and both properties share their names with
        /// Poiyomi's Emission 0 so the values are already on the material by the time we get here. What's left
        /// is opening the module up: Poiyomi's strength starts at 0, and it folds the color's alpha into that
        /// strength - which Standard's color picker never sets.
        /// </summary>
        void TranslateEmission(TranslationContext context)
        {
            Color emissionColor = GetSourcePropertyValue<Color>(context, "_EmissionColor");

            // Unity gates emission on the _EMISSION keyword, which its own inspector enables whenever the
            // emission color isn't (near) black. Checking both keeps this working if the keyword didn't
            // survive the shader swap.
            bool emissionEnabled = context.Material.IsKeywordEnabled("_EMISSION")
                || emissionColor.maxColorComponent > 0.1f / 255f;

            if (!emissionEnabled) return;

            SetTargetPropertyValue(context, "_EnableEmission", 1f);

            // Standard's emission color field leaves alpha at the shader default of 0. Poiyomi multiplies the
            // emission strength by that alpha, so it has to be opened up or the emission renders black.
            emissionColor.a = 1f;
            SetTargetPropertyValue(context, "_EmissionColor", emissionColor);

            // Unity has no strength slider - the HDR color is the whole intensity - so 1 reproduces it whether
            // or not an emission map is assigned.
            SetTargetPropertyValue(context, "_EmissionStrength", 1f);
        }

        #endregion

        /// <summary>
        /// Unity Standard transforms one UV with _MainTex_ST and samples every secondary map with it - normal,
        /// metallic/gloss, occlusion, emission and the detail mask all share the main texture's tiling. Poiyomi
        /// gives each map its own scale/offset, so push the main tiling onto them. Skipped when the main tiling
        /// is untouched, to keep default values off the material.
        /// </summary>
        void TranslateSecondaryMapTiling(TranslationContext context)
        {
            Vector4 mainScaleOffset = GetSourcePropertyValue<Vector4>(context, "_MainTex_ST");
            if (mainScaleOffset == IdentityScaleOffset) return;

            SetTargetPropertyValue(context, "_BumpMap_ST", mainScaleOffset);
            SetTargetPropertyValue(context, "_MochieMetallicMaps_ST", mainScaleOffset);
            SetTargetPropertyValue(context, "_LightingAOMaps_ST", mainScaleOffset);
            SetTargetPropertyValue(context, "_EmissionMap_ST", mainScaleOffset);
            SetTargetPropertyValue(context, "_DetailMask_ST", mainScaleOffset);
        }

        bool HasDetailMaps(TranslationContext context)
        {
            return GetSourcePropertyValue<Texture>(context, "_DetailAlbedoMap") != null
                || GetSourcePropertyValue<Texture>(context, "_DetailNormalMap") != null;
        }

        bool HasOcclusionMap(TranslationContext context)
        {
            return GetSourcePropertyValue<Texture>(context, "_OcclusionMap") != null;
        }
    }
}
