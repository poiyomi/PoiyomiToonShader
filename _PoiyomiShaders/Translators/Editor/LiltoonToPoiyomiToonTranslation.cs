// Some property values taken from https://github.com/LinesGuy/lilToonToPoiyomiToon. Thanks lines!

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


namespace Poi.Tools.ShaderTranslator.Translations
{
    public class LiltoonToPoiyomiToonTranslation : ScriptedShaderTranslator
    {
        public override bool CanTranslateMaterial(Material sourceMaterial)
        {
            // Check if the material we're trying to translate uses liltoon
            return sourceMaterial.shader.name.IndexOf("liltoon", StringComparison.CurrentCultureIgnoreCase) != -1;
        }

        protected override void DoBeforeTranslation(TranslationContext context)
        {
            // Set render mode dropdown based on liltoon shader name.
            string shaderName = Path.GetFileName(SourceShader.Shader.name);

            if (shaderName.Contains("lilToonCutout"))
            {
                SetTargetRenderingPreset(context, PoiShaderRenderingPreset.Cutout);
            }
            else if (shaderName.Contains("lilToonTransparent") || shaderName.Contains("lilToonOnePassTransparent") || shaderName.Contains("lilToonTwoPassTransparent"))
            {
                if (GetSourcePropertyValue<bool>(context, "_UseReflection"))
                    SetTargetRenderingPreset(context, PoiShaderRenderingPreset.Transparent);
                else
                    SetTargetRenderingPreset(context, PoiShaderRenderingPreset.TransClipping);
            }
            else if (shaderName.Contains("lilToonRefraction"))
            {
                SetTargetRenderingPreset(context, PoiShaderRenderingPreset.Transparent);
            }
        }

        protected override void DoAfterTranslation(TranslationContext context)
        {
            //Unity 2019 doesn't have .Contains(string, StringComparison) so using .IndexOf() instead
            bool hasOutline = SourceShader.Shader.name.IndexOf("outline", StringComparison.CurrentCultureIgnoreCase) != -1;

            if (hasOutline)
                SetTargetPropertyValue(context, "_EnableOutlines", 1);

            // Match liltoon lighting settings
            SetTargetPropertyValue(context, "_LightingColorMode", 3);
            SetTargetPropertyValue(context, "_LightingMapMode", 1);
            SetTargetPropertyValue(context, "_LightingDirectionMode", 4);

            // Set Decal Symmetry Modes
            if (TryGetDecalMirrorModes(context, "2nd", out PoiUvMirrorMode mirrorModeDecal0, out PoiUvSymmetryMode symmetryModeDecal0))
            {
                SetTargetPropertyValue(context, "_DecalMirroredUVMode", mirrorModeDecal0);
                SetTargetPropertyValue(context, "_DecalSymmetryMode", symmetryModeDecal0);
            }
            if (TryGetDecalMirrorModes(context, "3rd", out PoiUvMirrorMode mirrorModeDecal1, out PoiUvSymmetryMode symmetryModeDecal1))
            {
                SetTargetPropertyValue(context, "_DecalMirroredUVMode1", mirrorModeDecal1);
                SetTargetPropertyValue(context, "_DecalSymmetryMode1", symmetryModeDecal1);
            }

            // Manually restore render queue
            SetTargetRenderQueue(context, context.originalRenderQueue);
        }

        protected override List<PropertyTranslation> AddProperties()
        {
            return new List<PropertyTranslation>()
            {
                #region Main Color
                new PropertyTranslation("_AlphaMaskMode", "_MainAlphaMaskMode"),
                new PropertyTranslation("_Cutoff", "_Cutoff"),
                new PropertyTranslation("_AlphaMaskScale", (prop, context) =>
                {
                    float alphaMaskScale = GetSourcePropertyValue<float>(context, prop);
                    bool isInverted = alphaMaskScale == -1;
                    SetTargetPropertyValue(context, "_AlphaMaskInvert", isInverted);

                    float alphaMaskValue = GetSourcePropertyValue<float>(context, "_AlphaMaskValue");
                    if(isInverted)
                        alphaMaskValue -= 1.0f;

                    SetTargetPropertyValue(context, "_AlphaMaskValue", alphaMaskValue);
                }),
                new PropertyTranslation("_MainColorAdjustMask", "_MainColorAdjustTexture"),
                new PropertyTranslation("_MainColorAdjustMask_ST", "_MainColorAdjustTexture_ST"),
                new PropertyTranslation("_MainTexHSVG", (context) =>
                {
                    return GetSourcePropertyValue<Vector4>(context, "_MainTexHSVG") != new Vector4(0,1,1,1);
                },
                (prop, context) =>
                {
                    var hsvg = GetSourcePropertyValue<Vector4>(context, "_MainTexHSVG");
                    SetTargetPropertyValue(context, "_MainColorAdjustToggle", 1);
                    SetTargetPropertyValue(context, "_MainHueShiftColorSpace", 1);
                    SetTargetPropertyValue(context, "_MainHueShiftToggle", 1);
                    SetTargetPropertyValue(context, "_MainHueShift", hsvg.x > 0 ? hsvg.x : -0.5f + hsvg.x + 1.5f );
                    SetTargetPropertyValue(context, "_Saturation", hsvg.y - 1.0f);
                    SetTargetPropertyValue(context, "_MainBrightness", hsvg.z - 1.0f);
                    SetTargetPropertyValue(context, "_MainGamma", hsvg.w);
                }),
                new PropertyTranslation("_MainGradationTex", "_MainGradationTex", (context) =>
                {
                    return GetSourcePropertyValue<Texture2D>(context, "_MainGradationTex") != null;
                },
                (prop, context) =>
                {
                    SetTargetPropertyValue(context, "_MainColorAdjustToggle", 1);
                    SetTargetPropertyValue(context, "_ColorGradingToggle", 1);
                    var gradStrength = GetSourcePropertyValue<float>(context, "_MainGradationStrength");
                    SetTargetPropertyValue(context, "_MainGradationStrength", gradStrength);
                }),

                new PropertyTranslation("_UseBumpMap", (prop, context) =>
                {
                    // If normal map is disabled in liltoon, set intensity to 0
                    if(!GetSourcePropertyValue<bool>(context, "_UseBumpMap"))
                        SetTargetPropertyValue(context, "_BumpScale", 0);
                }),
                new PropertyTranslation("_UseBump2ndMap", "_DetailEnabled", IsBump2ndEnabled),
                new PropertyTranslation("_Bump2ndMap", "_DetailNormalMap", IsBump2ndEnabled),
                new PropertyTranslation("_Bump2ndMap_ST", "_DetailNormalMap_ST", IsBump2ndEnabled),
                new PropertyTranslation("_Bump2ndScale", "_DetailNormalMapScale", IsBump2ndEnabled),
                new PropertyTranslation("_Bump2ndMap_UVMode", "_DetailNormalMapUV", IsBump2ndEnabled),
                #endregion

                #region Emission
                new PropertyTranslation("_UseEmission", "_EnableEmission"),
                new PropertyTranslation("_EmissionMap_UVMode", "_EmissionMapUV"),
                new PropertyTranslation("_EmissionColor", "_EmissionColor", (prop, context) =>
                {
                    float alphaStrength = GetSourcePropertyValue<Color>(context, prop).a;
                    SetTargetPropertyValue(context, "_EmissionStrength", alphaStrength);
                }),
                new PropertyTranslation("_EmissionBlendMask", "_EmissionMask"),
                new PropertyTranslation("_EmissionBlendMask_ST", "_EmissionMask_ST"),
                new PropertyTranslation("_EmissionBlendMask_ScrollRotate", (prop, context) =>
                {
                    var scrollVector = GetSourcePropertyValue<Vector4>(context, prop);
                    SetTargetPropertyValue(context, "_EmissionMaskPan", scrollVector * 20);
                }),
                new PropertyTranslation("_EmissionMainStrength", "_EmissionBaseColorAsMap"),
                #endregion

                #region Emission 2nd
                new PropertyTranslation("_UseEmission2nd", "_EnableEmission1"),
                new PropertyTranslation("_Emission2ndColor", "_EmissionColor1", (prop, context) =>
                {
                    float alphaStrength = GetSourcePropertyValue<Color>(context, prop).a;
                    SetTargetPropertyValue(context, "_EmissionStrength1", alphaStrength);
                }),
                new PropertyTranslation("_Emission2ndMap", "_EmissionMap1"),
                new PropertyTranslation("_Emission2ndMap_UVMode", "_EmissionMap1UV"),
                new PropertyTranslation("_Emission2ndMap_ST", "_EmissionMap1_ST"),
                new PropertyTranslation("_Emission2ndBlendMask", "_EmissionMask1"),
                new PropertyTranslation("_Emission2ndBlendMask_ST", "_EmissionMask1_ST"),
                new PropertyTranslation("_Emission2ndMainStrength", "_EmissionBaseColorAsMap1"),
                new PropertyTranslation("_Emission2ndBlendMask_ScrollRotate", (prop, context) =>
                {
                    var scrollVector = GetSourcePropertyValue<Vector4>(context, prop);
                    SetTargetPropertyValue(context, "_EmissionMask1Pan", scrollVector * 20);
                }),
                #endregion

                #region Lighting
                new PropertyTranslation("_LightMaxLimit", "_LightingCap"),
                new PropertyTranslation("_LightMinLimit", "_LightingMinLightBrightness"),
                new PropertyTranslation("_MonochromeLighting", "_LightingMonochromatic"),
                new PropertyTranslation("_ShadowEnvStrength", "_LightingCastedShadows"),
                #endregion

                #region Shadows
                new PropertyTranslation("_UseShadow", (prop, context) =>
                {
                    bool enabled = GetSourcePropertyValue<bool>(context, prop);
                    if(!enabled)
                        return;

                    SetTargetPropertyValue(context, "_LightingMode", 1);

                    // Get the raw vector values from a material
                    Vector4 shadowAOShift = GetSourcePropertyValue<Vector4>(context, "_ShadowAOShift");
                    Vector4 shadowAOShift2 = GetSourcePropertyValue<Vector4>(context, "_ShadowAOShift2");

                    Vector4 poiShadowAOShift = new Vector4();
                    Vector4 poiShadowAOShift2 = new Vector4();

                    // We can demonstrate the conversion by recalculating them
                    poiShadowAOShift.x = shadowAOShift.x != 0 ? -shadowAOShift.y / shadowAOShift.x : 0f;
                    poiShadowAOShift.y = shadowAOShift.x != 0 ? (1f - shadowAOShift.y) / shadowAOShift.x : 1f;
                    poiShadowAOShift.z = shadowAOShift.z != 0 ? -shadowAOShift.w / shadowAOShift.z : 0f;
                    poiShadowAOShift.w = shadowAOShift.z != 0 ? (1f - shadowAOShift.w) / shadowAOShift.z : 1f;
                    poiShadowAOShift2.x = shadowAOShift2.x != 0 ? -shadowAOShift2.y / shadowAOShift2.x : 0f;
                    poiShadowAOShift2.y = shadowAOShift2.x != 0 ? (1f - shadowAOShift2.y) / shadowAOShift2.x : 1f;

                    SetTargetPropertyValue(context, "_ShadowAOShift", poiShadowAOShift);
                    SetTargetPropertyValue(context, "_ShadowAOShift2", poiShadowAOShift2);
                    SetTargetPropertyValue(context, "_ShadowBorderMapToggle", 1.0f);
                }),
                #endregion

                #region Matcap
                new PropertyTranslation("_UseMatCap", "_MatcapEnable"),
                new PropertyTranslation("_MatCapTex", "_Matcap"),
                new PropertyTranslation("_MatCapTex_ST", "_Matcap_ST"),
                new PropertyTranslation("_MatCapMainStrength", "_MatcapBaseColorMix"),
                new PropertyTranslation("_MatCapNormalStrength", "_MatcapNormal"),
                new PropertyTranslation("_MatCapBlendMask", "_MatcapMask"),
                new PropertyTranslation("_MatCapBlendMask_ST", "_MatcapMask_ST"),
                new PropertyTranslation("_MatCapColor", (prop, context) =>
                {
                    Color matcapColor = GetSourcePropertyValue<Color>(context, prop);
                    float alpha = matcapColor.a;
                    int blendMode = GetSourcePropertyValue<int>(context, "_MatCapBlendMode");
                    float replaceValue = 0;
                    switch(blendMode)
                    {
                        // Liltoon matcap blend modes enum: Normal, Add, Screen, Multiply
                        case 0: replaceValue = alpha; break;
                        case 1: SetTargetPropertyValue(context, "_MatcapAdd", alpha); break;
                        case 2: SetTargetPropertyValue(context, "_MatcapScreen", alpha); break;
                        case 3: SetTargetPropertyValue(context, "_MatcapMultiply", alpha); break;
                    }
                    // Reset replace to 0 if it's not being used because by default it's on 1 in poi
                    SetTargetPropertyValue(context, "_MatcapReplace", replaceValue);
                    matcapColor.a = 1.0f; // If we don't do this, we double the intensity by accident
                    SetTargetPropertyValue(context, "_MatcapColor", matcapColor);
                    // liltoon doesn't have this option (AFAIK) and is 0.5f in code
                    SetTargetPropertyValue(context, "_MatcapBorder", 0.5f);
                }),
                new PropertyTranslation("_MatCapLod", (prop, context) =>
                {
                    float smoothness = GetSourcePropertyValue<float>(context, prop);

                    if(smoothness == 0f)
                        return;

                    SetTargetPropertyValue(context, "_MatcapSmoothnessEnabled", true);
                    SetTargetPropertyValue(context, "_MatcapSmoothness", 1 - (smoothness / 10)); // liltoon goes up to 10
                }),
                new PropertyTranslation("_MatCapCustomNormal", "_Matcap0CustomNormal"),
                new PropertyTranslation("_MatCapBumpScale", "_Matcap0NormalMapScale"),
                new PropertyTranslation("_MatCapBumpMap", "_Matcap0NormalMap"),
                new PropertyTranslation("_MatCapBumpMap_ST", "_Matcap0NormalMap_ST"),
                #endregion

                #region Matcap 2
                new PropertyTranslation("_UseMatCap2nd", "_Matcap2Enable"),
                new PropertyTranslation("_MatCap2ndTex", "_Matcap2"),
                new PropertyTranslation("_MatCap2ndTex_ST", "_Matcap2_ST"),
                new PropertyTranslation("_MatCap2ndColor", "_Matcap2Color"),
                new PropertyTranslation("_MatCap2ndMainStrength", "_Matcap2BaseColorMix"),
                new PropertyTranslation("_MatCap2ndNormalStrength", "_Matcap2Normal"),
                new PropertyTranslation("_MatCap2ndBlendMask", "_Matcap2Mask"),
                new PropertyTranslation("_MatCap2ndBlendMask_ST", "_Matcap2Mask_ST"),
                new PropertyTranslation("_MatCap2ndColor", (prop, context) =>
                {
                    float alpha = GetSourcePropertyValue<Color>(context, prop).a;
                    int blendMode = GetSourcePropertyValue<int>(context, "_MatCap2ndBlendMode");
                    float replaceValue = 0;
                    switch(blendMode)
                    {
                        // Liltoon matcap blend modes enum: Normal, Add, Screen, Multiply
                        case 0: replaceValue = alpha; break;
                        case 1: SetTargetPropertyValue(context, "_Matcap2Add", alpha); break;
                        case 2: SetTargetPropertyValue(context, "_Matcap2Screen", alpha); break;
                        case 3: SetTargetPropertyValue(context, "_Matcap2Multiply", alpha); break;
                    }
                }),
                new PropertyTranslation("_MatCap2ndLod", (prop, context) =>
                {
                    float smoothness = GetSourcePropertyValue<float>(context, prop);

                    if(smoothness == 0f)
                        return;

                    SetTargetPropertyValue(context, "_Matcap2SmoothnessEnabled", true);
                    SetTargetPropertyValue(context, "_Matcap2Smoothness", smoothness / 10); // liltoon goes up to 10
                }),
                new PropertyTranslation("_MatCap2ndCustomNormal", "_Matcap1CustomNormal"),
                new PropertyTranslation("_MatCap2ndBumpScale", "_Matcap1NormalMapScale"),
                new PropertyTranslation("_MatCap2ndBumpMap", "_Matcap1NormalMap"),
                new PropertyTranslation("_MatCap2ndBumpMap_ST", "_Matcap1NormalMap_ST"),
                #endregion

                #region Rim lights (uses Rimlight 2)
                new PropertyTranslation("_UseRim", "_EnableRim2Lighting", (prop, context) =>
                {   
                    // If rimlight is enabled, set the style to liltoon. All other properties are the same
                    if(GetSourcePropertyValue<bool>(context, prop))
                        SetTargetPropertyValue(context, "_Rim2Style", 2);
                }),
                new PropertyTranslation("_RimColor", "_Rim2Color"),
                new PropertyTranslation("_RimColorTex", "_Rim2ColorTex"),
                new PropertyTranslation("_RimColorTex_ST", "_Rim2ColorTex_ST"),
                new PropertyTranslation("_RimMainStrength", "_Rim2MainStrength"),
                new PropertyTranslation("_RimNormalStrength", "_Rim2NormalStrength"),
                new PropertyTranslation("_RimBorder", "_Rim2Border"),
                new PropertyTranslation("_RimBlur", "_Rim2Blur"),
                new PropertyTranslation("_RimFresnelPower", "_Rim2FresnelPower"),
                new PropertyTranslation("_RimEnableLighting", "_Rim2EnableLighting"),
                new PropertyTranslation("_RimShadowMask", "_Rim2ShadowMask"),
                new PropertyTranslation("_RimBackfaceMask", "_Rim2BackfaceMask"),
                new PropertyTranslation("_RimVRParallaxStrength", "_Rim2VRParallaxStrength"),
                // new PropertyTranslation("_RimApplyTransparency", "_Rim2ApplyTransparency"),
                new PropertyTranslation("_RimDirStrength", "_Rim2DirStrength"),
                new PropertyTranslation("_RimDirRange", "_Rim2DirRange"),
                new PropertyTranslation("_RimIndirRange", "_Rim2IndirRange"),
                new PropertyTranslation("_RimIndirColor", "_Rim2IndirColor"),
                new PropertyTranslation("_RimIndirBorder", "_Rim2IndirBorder"),
                new PropertyTranslation("_RimIndirBlur", "_Rim2IndirBlur"),
                new PropertyTranslation("_RimBlendMode", "_Rim2BlendMode"),

                #endregion

                #region Backlight
                new PropertyTranslation("_UseBacklight", "_BacklightEnabled"),
                #endregion

                #region Outline
                new PropertyTranslation("_UseOutline", "_EnableOutline"),
                new PropertyTranslation("_OutlineTex", "_OutlineTexture"),
                new PropertyTranslation("_OutlineTex_ST", "_OutlineTexture_ST"),
                new PropertyTranslation("_OutlineColor", "_LineColor"),
                new PropertyTranslation("_OutlineWidthMask", "_OutlineMask"),
                new PropertyTranslation("_OutlineWidthMask_ST", "_OutlineMask_ST"),
                new PropertyTranslation("_OutlineWidth", "_LineWidth"),
                new PropertyTranslation("_OutlineDeleteMesh", "_OutlineClipAtZeroWidth"),
                 new PropertyTranslation("_OutlineTexHSVG", (prop, context) =>
                {
                    var hsvg = GetSourcePropertyValue<Vector4>(context, prop);
                    if(hsvg.x != 0 && hsvg.y != 1 && hsvg.z != 1 && hsvg.w != 1)
                    {
                        SetTargetPropertyValue(context, "_OutlineHueShift", 1);
                        SetTargetPropertyValue(context, "_OutlineHue", hsvg.x > 0 ? hsvg.x : -0.5f + hsvg.x + 1.5f );
                        SetTargetPropertyValue(context, "_OutlineSaturation", hsvg.y);
                        SetTargetPropertyValue(context, "_OutlineValue", hsvg.z);
                        SetTargetPropertyValue(context, "_OutlineGamma", hsvg.w);
                    }
                }),
                #endregion

                #region Glitter
                new PropertyTranslation("_GlitterUVMode", "_GlitterUV"),
                new PropertyTranslation("_GlitterColorTex", "_GlitterColorMap"),
                new PropertyTranslation("_GlitterColorTex_UVMode", "_GlitterColorMapUV"),
                new PropertyTranslation("_GlitterColorTex_ST", "_GlitterColorMap_ST"),
                new PropertyTranslation("_GlitterMainStrength", "_GlitterUseSurfaceColor"),
                #endregion

                #region Decal
                new PropertyTranslation("_Main2ndBlendMask", "_DecalMask"),
                new PropertyTranslation("_UseMain2ndTex", "_DecalEnabled"),
                new PropertyTranslation("_Main2ndTex", "_DecalTexture"),
                new PropertyTranslation("_Main2ndTex_ST", (prop, context) =>
                {
                    var textureScaleAndOffset = GetSourcePropertyValue<Vector4>(context, prop);
                    if(GetSourcePropertyValue<bool>(context, "_Main2ndTexIsDecal"))
                    {
                        float ang = GetSourcePropertyValue<float>(context, "_Main2ndTexAngle");
                        float x = -((textureScaleAndOffset.z - 0.5f) / textureScaleAndOffset.x) - 0.5f;
                        float y = -((textureScaleAndOffset.w - 0.5f) / textureScaleAndOffset.y) - 0.5f;
                        float xPos = x * Mathf.Cos(-ang) - y * Mathf.Sin(-ang) + .5f;
                        float yPos = y * Mathf.Cos(-ang) + x * Mathf.Sin(-ang) + .5f;
                        SetTargetPropertyValue(context, "_DecalPosition", new Vector2(xPos, yPos));
                        SetTargetPropertyValue(context, "_DecalScale", new Vector2(1 / textureScaleAndOffset.x, 1 / textureScaleAndOffset.y));
                    }
                    else
                    {
                        SetTargetPropertyValue(context, "_DecalTexture_ST", textureScaleAndOffset);
                    }
                }),
                new PropertyTranslation("_Main2ndTexIsDecal", (prop, context) =>
                {
                    bool value = GetSourcePropertyValue<bool>(context, prop);
                    SetTargetPropertyValue(context,  "_DecalTiled", !value);
                }),
                new PropertyTranslation("_Main2ndTex_UVMode", "_DecalTextureUV"),
                new PropertyTranslation("_Main2ndTexBlendMode", (prop, context) =>
                {
                    //liltoon: normal, add, screen, multiply
                    //poiyomi: replace, darken, multiply, lighten, screen, subtract, add, overlay, mixed
                    int value = GetSourcePropertyValue<int>(context, prop);
                    switch(value)
                    {
                        case 1: value = 8; break;
                        case 2: value = 6; break;
                        case 3: value = 2; break;
                        default: value = 0; break;
                    }
                    SetTargetPropertyValue(context, "_DecalBlendType", value);
                }),
                new PropertyTranslation("_Main2ndTexAlphaMode", (prop, context) =>
                {
                    //liltoon: normal, replace, multiply, add, subtract
                    //poiyomi: off, multiply, add, subtract, min, max
                    //TODO: Not sure how these translate
                    int value = GetSourcePropertyValue<int>(context, prop);
                    switch(value)
                    {
                        case 2: value = 1; break;
                        case 3: value = 2; break;
                        case 4: value = 3; break;
                        default: break;
                    }
                    SetTargetPropertyValue(context, "_DecalOverrideAlpha", value);
                }),
                new PropertyTranslation("_Color2nd", "_DecalColor", (prop, context) =>
                {
                    Color colorValue = GetSourcePropertyValue<Color>(context, prop);
                    //SetTargetPropertyValue(context, "_DecalBlendAlpha", colorValue.a);
                }),
                new PropertyTranslation("_Main2ndEnableLighting", (prop, context) =>
                {
                    float value = GetSourcePropertyValue<float>(context, prop);
                    SetTargetPropertyValue(context, "_DecalEmissionStrength", value * -1);
                }),
                new PropertyTranslation("_Main2ndTex_ScrollRotate", (prop, context) =>
                {
                    Vector4 vectorValue = GetSourcePropertyValue<Vector4>(context, prop);
                    SetTargetPropertyValue(context, "_DecalTexturePan", vectorValue * 20);
                }),
                new PropertyTranslation("_Main2ndTexAngle", (prop, context) =>
                {
                    float floatValue = GetSourcePropertyValue<float>(context, prop);
                    SetTargetPropertyValue(context, "_DecalRotation", (floatValue * Mathf.Rad2Deg) % 360);
                }),
                new PropertyTranslation("_Main2ndTex_Cull", (prop, context) =>
                {
                    // liltoon: cull off, cull front, cull back
                    // poiyomi: off, front only, back only
                    int value = GetSourcePropertyValue<int>(context, prop);
                    switch(value)
                    {
                        case 1: value = 2; break;
                        case 2: value = 1; break;
                    }
                    SetTargetPropertyValue(context, "_Decal0FaceMask", value);
                }),
                // Decal Mirror modes are handled in DoAfterTranslation
                #endregion

                #region Decal 2
                new PropertyTranslation("_UseMain3rdTex", "_DecalEnabled1"),
                new PropertyTranslation("_Main3rdTex", "_DecalTexture1"),
                new PropertyTranslation("_Main3rdTex_ST", (prop, context) =>
                {
                    var textureScaleAndOffset = GetSourcePropertyValue<Vector4>(context, prop);
                    if(GetSourcePropertyValue<bool>(context, "_Main3rdTexIsDecal"))
                    {
                        float ang = GetSourcePropertyValue<float>(context, "_Main3rdTexAngle");
                        float x = -((textureScaleAndOffset.z - 0.5f) / textureScaleAndOffset.x) - 0.5f;
                        float y = -((textureScaleAndOffset.w - 0.5f) / textureScaleAndOffset.y) - 0.5f;
                        float xPos = x * Mathf.Cos(-ang) - y * Mathf.Sin(-ang) + .5f;
                        float yPos = y * Mathf.Cos(-ang) + x * Mathf.Sin(-ang) + .5f;
                        SetTargetPropertyValue(context, "_DecalPosition1", new Vector2(xPos, yPos));
                        SetTargetPropertyValue(context, "_DecalScale1", new Vector2(1 / textureScaleAndOffset.x, 1 / textureScaleAndOffset.y));
                    }
                    else
                    {
                        SetTargetPropertyValue(context, "_DecalTexture1_ST", textureScaleAndOffset);
                    }
                }),
                new PropertyTranslation("_Main3rdTexIsDecal", (prop, context) =>
                {
                    bool value = GetSourcePropertyValue<bool>(context, prop);
                    SetTargetPropertyValue(context, "_DecalTiled1", !value);
                }),
                new PropertyTranslation("_Main3rdTex_UVMode", "_DecalTexture1UV"),
                new PropertyTranslation("_Main3rdTexBlendMode", (prop, context) =>
                {
                    //liltoon: normal, add, screen, multiply
                    //poiyomi: replace, darken, multiply, lighten, screen, subtract, add, overlay, mixed
                    int value = GetSourcePropertyValue<int>(context, prop);
                    switch(value)
                    {
                        case 1: value = 6; break;
                        case 2: value = 4; break;
                        case 3: value = 2; break;
                        default: value = 0; break;
                    }
                    SetTargetPropertyValue(context, "_DecalBlendType1", value);
                }),
                new PropertyTranslation("_Main3rdTexAlphaMode", (prop, context) =>
                {
                    //liltoon: normal, replace, multiply, add, subtract
                    //poiyomi: off, multiply, add, subtract, min, max
                    //TODO: Not sure how these translate
                    int value = GetSourcePropertyValue<int>(context, prop);
                    switch(value)
                    {
                        case 2: value = 1; break;
                        case 3: value = 2; break;
                        case 4: value = 3; break;
                        default: break;
                    }
                    SetTargetPropertyValue(context, "_DecalOverrideAlpha1", value);
                }),
                new PropertyTranslation("_Color3rd", "_DecalColor1", (prop, context) =>
                {
                    Color colorValue = GetSourcePropertyValue<Color>(context, prop);
                    //SetTargetPropertyValue(context, "_DecalBlendAlpha1", colorValue.a);
                }),
                new PropertyTranslation("_Main3rdEnableLighting",  (prop, context) =>
                {
                    float value = GetSourcePropertyValue<float>(context, prop);
                    SetTargetPropertyValue(context, "_DecalEmissionStrength1", value * -1);
                }),
                new PropertyTranslation("_Main3rdTex_ScrollRotate", (prop, context) =>
                {
                    Vector4 vectorValue = GetSourcePropertyValue<Vector4>(context, prop);
                    SetTargetPropertyValue(context, "_DecalTexture1Pan", vectorValue * 20);
                }),
                new PropertyTranslation("_Main3rdTexAngle", (prop, context) =>
                {
                    float floatValue = GetSourcePropertyValue<float>(context, prop);
                    SetTargetPropertyValue(context, "_DecalRotation1", (floatValue * Mathf.Rad2Deg) % 360);
                }),
                new PropertyTranslation("_Main3rdTex_Cull", (prop, context) =>
                {
                    // liltoon: cull off, cull front, cull back
                    // poiyomi: off, front only, back only
                    int value = GetSourcePropertyValue<int>(context, prop);
                    switch(value)
                    {
                        case 1: value = 2; break;
                        case 2: value = 1; break;
                    }
                    SetTargetPropertyValue(context, "_Decal1FaceMask", value);
                }),
                // Decal mirror modes are handled in DoAfterTranslation
                #endregion

                #region Rim shade -> Rimlight 1
                new PropertyTranslation("_UseRimShade", IsRimShadeEnabled, (prop, context) =>
                {
                    // RimShade seems to just be Rim Lighting in "Multiply" mode
                    SetTargetPropertyValue(context, "_RimMaskOnlyMask", 1);
                    SetTargetPropertyValue(context, "_EnableRimLighting", 1);
                    SetTargetPropertyValue(context, "_RimStyle", 2);
                    SetTargetPropertyValue(context, "_RimShadowMask", 0); // Lines: lil's rimshade seems to ignore shadows completely
                    SetTargetPropertyValue(context, "_RimBlendMode", 3); // Lines: Multiply mode
                    SetTargetPropertyValue(context, "_RimEnableLighting", 0);
                    SetTargetPropertyValue(context, "_RimDirStrength", 0);
                    SetTargetPropertyValue(context, "_RimMainStrength", 0);
                }),
                new PropertyTranslation("_RimShadeBorder", "_RimBorder", IsRimShadeEnabled),
                new PropertyTranslation("_RimShadeBlur", "_RimBlur", IsRimShadeEnabled),
                new PropertyTranslation("_RimShadeFresnelPower", "_RimFresnelPower", IsRimShadeEnabled),
                new PropertyTranslation("_RimShadeColor", "_RimColor", IsRimShadeEnabled),
                new PropertyTranslation("_RimShadeMask", "_RimColorTex", IsRimShadeEnabled),
                new PropertyTranslation("_RimShadeMask_ST", "_RimColorTex_ST", IsRimShadeEnabled),

                #endregion

                #region Stylized Reflections
                new PropertyTranslation("_UseReflection", UseReflection, (prop, context) =>
                {
                    SetTargetPropertyValue(context, "_StylizedSpecular", 1);
                    SetTargetPropertyValue(context, "_StylizedReflectionMode", 1);
                }),

                #endregion

                #region  Rendering Options
                // Main Area
                new PropertyTranslation("_Cull", "_Cull"),
                new PropertyTranslation("_ZTest", "_ZTest"),
                new PropertyTranslation("_ZWrite", "_ZWrite"),
                new PropertyTranslation("_ColorMask", "_ColorMask"),
                new PropertyTranslation("_OffsetFactor", "_OffsetFactor"),
                new PropertyTranslation("_OffsetUnits", "_OffsetUnits"),
                new PropertyTranslation("_ZClip", "_ZClip"),
                new PropertyTranslation("_FlipNormal", "_FlipBackfaceNormals"),

                /*
                // Blending
                new PropertyTranslation("_BlendOp", "_BlendOp"),
                new PropertyTranslation("_SrcBlend", "_SrcBlend"),
                new PropertyTranslation("_DstBlend", "_DstBlend"),

                new PropertyTranslation("_BlendOpFA", "_AddBlendOp"),
                new PropertyTranslation("_SrcBlendFA", "_AddSrcBlend"),
                new PropertyTranslation("_DstBlendFA", "_AddDstBlend"),

                // Advanced Blending
                new PropertyTranslation("_BlendOpAlpha", "_BlendOpAlpha"),
                new PropertyTranslation("_SrcBlendAlpha", "_SrcBlendAlpha"),
                new PropertyTranslation("_DstBlendAlpha", "_DstBlendAlpha"),

                new PropertyTranslation("_BlendOpAlphaFA", "_AddBlendOpAlpha"),
                new PropertyTranslation("_SrcBlendAlphaFA", "_AddSrcBlendAlpha"),
                new PropertyTranslation("_DstBlendAlphaFA", "_AddDstBlendAlpha"),
                */
                #endregion

                #region UV Tile Discard
                new PropertyTranslation("_UDIMDiscardCompile", "_EnableUDIMDiscardOptions"),
                #endregion
                
                #region Stencils
                new PropertyTranslation("_StencilRef", "_StencilRef"),
                new PropertyTranslation("_StencilReadMask", "_StencilReadMask"),
                new PropertyTranslation("_StencilWriteMask", "_StencilWriteMask"),
                new PropertyTranslation("_StencilPass", "_StencilPassOp"),
                new PropertyTranslation("_StencilFail", "_StencilFailOp"),
                new PropertyTranslation("_StencilZFail", "_StencilZFailOp"),
                new PropertyTranslation("_StencilComp", "_StencilCompareFunction"),
                #endregion
            };
        }

        bool TryGetDecalMirrorModes(TranslationContext context, string decalPropertyNameNumberPart, out PoiUvMirrorMode uvMirrorMode, out PoiUvSymmetryMode uvSymmetryMode)
        {
            uvMirrorMode = PoiUvMirrorMode.Off;
            uvSymmetryMode = PoiUvSymmetryMode.Off;

            bool texIsEnabled = GetSourcePropertyValue<bool>(context, $"_UseMain{decalPropertyNameNumberPart}Tex");
            bool texIsDecal = GetSourcePropertyValue<bool>(context, $"_Main{decalPropertyNameNumberPart}TexIsDecal");
            if (!texIsEnabled || !texIsDecal)
                return false;

            bool isLeft = GetSourcePropertyValue<bool>(context, $"_Main{decalPropertyNameNumberPart}TexIsLeftOnly");
            bool isRight = GetSourcePropertyValue<bool>(context, $"_Main{decalPropertyNameNumberPart}TexIsRightOnly");
            bool shouldCopy = GetSourcePropertyValue<bool>(context, $"_Main{decalPropertyNameNumberPart}TexShouldCopy");
            bool shouldFlipMirror = GetSourcePropertyValue<bool>(context, $"_Main{decalPropertyNameNumberPart}TexShouldFlipMirror");
            bool shouldFlipCopy = GetSourcePropertyValue<bool>(context, $"_Main{decalPropertyNameNumberPart}TexShouldFlipCopy");

            if (shouldFlipMirror && isRight)
                uvMirrorMode = PoiUvMirrorMode.FlipRightOnly;
            else if (shouldFlipMirror)
                uvMirrorMode = PoiUvMirrorMode.Flip;
            else if (isLeft)
                uvMirrorMode = PoiUvMirrorMode.LeftOnly;
            else if (isRight)
                uvMirrorMode = PoiUvMirrorMode.RightOnly;

            if (shouldCopy && shouldFlipCopy)
                uvSymmetryMode = PoiUvSymmetryMode.Flipped;
            else if (shouldCopy)
                uvSymmetryMode = PoiUvSymmetryMode.Symmetry;

            return true;
        }
        bool UseReflection(TranslationContext context)
        {
            return GetSourcePropertyValue<bool>(context, "_UseReflection");
        }
        bool IsRimShadeEnabled(TranslationContext context)
        {
            return GetSourcePropertyValue<bool>(context, "_UseRimShade");
        }

        bool IsBump2ndEnabled(TranslationContext context)
        {
            return GetSourcePropertyValue<bool>(context, "_UseBump2ndMap");
        }
    }
}