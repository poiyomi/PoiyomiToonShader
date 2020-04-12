Shader ".poiyomi/Toon/Advanced/Outlines Transparent"
{
    Properties
    {
        [HideInInspector] shader_is_using_thry_editor ("", Float) = 0
        [HideInInspector] shader_master_label ("<color=#ff0000ff>❤</color> <color=#000000ff>Poiyomi Toon V5.4</color> <color=#ff0000ff>❤</color>", Float) = 0
        [HideInInspector] shader_presets ("poiToonPresets", Float) = 0
        [HideInInspector] shader_properties_label_file ("PoiLabels", Float) = 0
        
        [HideInInspector] footer_youtube ("youtube footer button", Float) = 0
        [HideInInspector] footer_twitter ("twitter footer button", Float) = 0
        [HideInInspector] footer_patreon ("patreon footer button", Float) = 0
        [HideInInspector] footer_discord ("discord footer button", Float) = 0
        [HideInInspector] footer_github ("github footer button", Float) = 0
        
        [HideInInspector] m_mainOptions ("Main", Float) = 0
        _Color ("Color & Alpha", Color) = (1, 1, 1, 1)
        _Saturation ("Saturation", Range(-1, 1)) = 0
        _MainVertexColoring ("Use Vertex Color", Range(0, 1)) = 0
        _MainEmissionStrength ("Basic Emission", Range(0, 20)) = 0
        _MainTex ("Texture", 2D) = "white" { }
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _MainTextureUV ("Tex UV#", Int) = 0
        _MainHueShift ("HueShift", Range(0, 1)) = 0
        [Normal]_BumpMap ("Normal Map", 2D) = "bump" { }
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _BumpMapUV ("Normal UV#", Int) = 0
        [HideInInspector][Vector2]_MainNormalPan ("Panning", Vector) = (0, 0, 0, 0)
        _BumpScale ("Normal Intensity", Range(0, 10)) = 1
        _AlphaMask ("Alpha Mask", 2D) = "white" { }
        [Vector2]_GlobalPanSpeed ("Global Pan Speed", Vector) = (0, 0, 0, 0)
        
        [HideInInspector] m_start_RGBMask ("RGB Color Masking", Float) = 0
        [Toggle(FXAA)]_RGBMaskEnabled ("RGB Mask Enabled", Float) = 0
        _RGBMask ("Mask", 2D) = "white" { }
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)]_RGBMaskUV ("Mask UV", int) = 0
        _RedColor ("R Color", Color) = (1, 1, 1, 1)
        _RedTexure ("R Texture", 2D) = "white" { }
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)]_RGBRed_UV ("Red UV", int) = 0
        _GreenColor ("G Color", Color) = (1, 1, 1, 1)
        _GreenTexture ("G Texture", 2D) = "white" { }
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)]_RGBGreen_UV ("Green UV", int) = 0
        _BlueColor ("B Color", Color) = (1, 1, 1, 1)
        _BlueTexture ("B Texture", 2D) = "white" { }
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)]_RGBBlue_UV ("Blue UV", int) = 0
        [HideInInspector] m_end_RGBMask ("RGB Color Masking", Float) = 0
        
        [HideInInspector] m_start_DetailOptions ("Details", Float) = 0
        _DetailMask ("Detail Mask (R:Texture, G:Normal)", 2D) = "white" { }
        _DetailTex ("Detail Texture", 2D) = "gray" { }
        [HideInInspector][Vector2]_DetailTexturePan ("Panning", Vector) = (0, 0, 0, 0)
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _DetailTexUV ("Detail Tex UV#", Int) = 0
        _DetailTexIntensity ("Detail Tex Intensity", Range(0, 10)) = 1
        _DetailBrightness ("Detail Brightness:", Range(0, 2)) = 1
        _DetailTint ("Detail Tint", Color) = (1, 1, 1)
        [Normal]_DetailNormalMap ("Detail Normal", 2D) = "bump" { }
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _DetailNormalUV ("Detail Normal UV#", Int) = 0
        _DetailNormalMapScale ("Detail Normal Intensity", Range(0, 10)) = 1
        [HideInInspector][Vector2]_MainDetailNormalPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector] m_end_DetailOptions ("Details", Float) = 0
        
        [HideInInspector] m_start_vertexManipulation ("Vertex Options", Float) = 0
        [Vector3]_VertexManipulationLocalTranslation ("Local Translation", Vector) = (0, 0, 0, 1)
        [Vector3]_VertexManipulationLocalRotation ("Local Rotation", Vector) = (0, 0, 0, 1)
        [Vector3]_VertexManipulationLocalScale ("Local Scale", Vector) = (1, 1, 1, 1)
        [Vector3]_VertexManipulationWorldTranslation ("World Translation", Vector) = (0, 0, 0, 1)
        _VertexManipulationHeight ("Vertex Height", Float) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] vertexManipulationUV ("Heightmap UV", Int) = 0
        _VertexManipulationHeightMask ("Height Map", 2D) = "while" { }
        _VertexManipulationHeightBias ("Mask Bias", Range(0, 1)) = 0
        [HideInInspector][Vector2]_VertexManipulationHeightPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector] m_end_vertexManipulation ("Vertex Options", Float) = 0
        
        [HideInInspector] m_start_Alpha ("Alpha Options", Float) = 0
        _AlphaMod ("Alpha Mod", Range(-1,1)) = 0.0 
        _Clip ("Alpha Cuttoff", Range(0, 1.001)) = 0.0
        [Toggle(_)]_DitheringEnabled ("Enable Dithering", Float) = 0
        [Toggle(_)]_ForceOpaque ("Force Opaque", Float) = 0
        [Toggle(_)]_MainAlphaToCoverage ("Alpha To Coverage", Float) = 1
        _MainShadowClipMod ("Shadow Clip Mod", Range(-1,1)) = 0
        _MainMipScale ("Mip Level Alpha Scale", Range(0, 1)) = 0.25
        [HideInInspector] m_end_Alpha ("Alpha Options", Float) = 0
        
        [HideInInspector] m_start_backFace ("Back Face", Float) = 0
        [Toggle(_)]_BackFaceEnabled ("Enable Back Face Options", Float) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)]_BackFaceTextureUV ("UV#", Int) = 0
        _BackFaceTexture ("Texture", 2D) = "white" { }
        [HideInInspector][Vector2]_BackFacePanning ("Panning", Vector) = (0, 0, 0, 0)
        _BackFaceDetailIntensity ("Detail Intensity", Range(0, 5)) = 1
        _BackFaceHueShift ("Hue Shift", Range(0, 1)) = 0
        _BackFaceEmissionStrength ("Emission Strength", Range(0, 20)) = 0
        [HideInInspector] m_end_backFace ("Back Face", Float) = 0
        
        [HideInInspector] m_lightingOptions ("Lighting", Float) = 0
        [HideInInspector] m_start_Lighting ("Light and Shadow", Float) = 0
        [Toggle(LOD_FADE_CROSSFADE)]_EnableLighting ("Enable Lighting", Float) = 1
        [Enum(Natural, 0, Controlled, 1, Standardish, 2)] _LightingType ("Lighting Type", Int) = 1
        [Gradient]_ToonRamp ("Lighting Ramp", 2D) = "white" { }
        _LightingShadowMask ("Shadow Mask (RGBA)", 2D) = "white" { }
        _ShadowStrength ("Shadow Strength", Range(0, 1)) = .2
        _ShadowOffset ("Shadow Offset", Range(-1, 1)) = 0
        _AOMap ("AO Map", 2D) = "white" { }
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _LightingAOUV ("AO Map UV#", Int) = 0
        _AoIndirectStrength ("AO Indirect Strength", Range(0, 1)) = 1
        _AOStrength ("AO Direct Strength", Range(0, 1)) = 0
        _LightingMinLightBrightness ("Min Brightness", Range(0, 1)) = 0
        _LightingIndirectContribution ("Indirect Contribution", Range(0, 1)) = .2
        _AttenuationMultiplier ("Recieve Casted Shadows?", Range(0, 1)) = 0
        [HideInInspector] m_start_lightingStandard ("Standardish Settings", Float) = 0
        _LightingStandardSmoothness ("Smoothness", Range(0, 1)) = 0
        [HideInInspector] m_end_lightingStandard ("Standardish Settings", Float) = 0
        [HideInInspector] m_start_lightingAdvanced ("Additive Lighting", Float) = 0
        //[Toggle(_)] _LightingUseShadowRamp("Use Shadow Ramp", Float) = 0
        _AdditiveSoftness ("Additive Softness", Range(0, 0.5)) = 0.005
        _AdditiveOffset ("Additive Offset", Range(-0.5, 0.5)) = 0
        _LightingAdditiveIntensity ("Additive Intensity", Range(0, 1)) = 1
        
        [HideInInspector] m_end_lightingAdvanced ("Additive Lighting", Float) = 0
        [HideInInspector] m_start_lightingBeta ("Beta", Float) = 0
        [Toggle(_)]_LightingStandardControlsToon ("Standard Lighting Controls Toon Ramp", Float) = 0
        [IntRange]_LightingNumRamps ("Num Ramps", Range(1, 3)) = 1
        [Gradient]_ToonRamp1 ("Lighting Ramp 2", 2D) = "white" { }
        _LightingShadowStrength1 ("Shadow Strength 2", Range(0, 1)) = 1
        _ShadowOffset1 ("Shadow Offset 2", Range(-1, 1)) = 0
        [Gradient]_ToonRamp2 ("Lighting Ramp 3", 2D) = "white" { }
        _LightingShadowStrength2 ("Shadow Strength 3", Range(0, 1)) = 1
        _ShadowOffset2 ("Shadow Offset 3", Range(-1, 1)) = 0
        [HideInInspector] m_end_lightingBeta ("Beta", Float) = 0
        [HideInInspector] m_end_Lighting ("Light and Shadow", Float) = 0
        
        [HideInInspector] m_start_subsurface ("Subsurface Scattering", Float) = 0
        [Toggle(_TERRAIN_NORMAL_MAP)]_EnableSSS ("Enable Subsurface Scattering", Float) = 0
        _SSSColor ("Subsurface Color", Color) = (1, 0, 0, 1)
        _SSSColorMap ("Color Map", 2D) = "white" { }
        _SSSThicknessMap ("Thickness Map", 2D) = "black" { }
        _SSSThicknessMod ("Thickness mod", Range(-1, 1)) = 0
        _SSSAttenuation ("Attenuation", Range(0, 1)) = 0.25
        _SSSPower ("Light Spread", Range(1, 20)) = 6
        _SSSDistortion ("Light Distortion", Range(0, 1)) = 1
        [Enum(vertex, 0, pixel, 1)] _SSSNormal ("Normal Select", Int) = 1
        [HideInInspector] m_end_subsurface ("Subsurface Scattering", Float) = 0
        
        [HideInInspector] m_start_rimLightOptions ("Rim Lighting", Float) = 0
        [Toggle(_GLOSSYREFLECTIONS_OFF)]_EnableRimLighting ("Enable Rim Lighting", Float) = 0
        [Enum(vertex, 0, pixel, 1)] _RimLightNormal ("Normal Select", Int) = 1
        [Toggle(_)]_RimLightingInvert ("Invert Rim Lighting", Float) = 0
        _RimLightColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimWidth ("Rim Width", Range(0, 1)) = 0.8
        _RimSharpness ("Rim Sharpness", Range(0, 1)) = .25
        _RimStrength ("Rim Emission", Range(0, 20)) = 0
        _RimBrighten ("Rim Color Brighten", Range(0, 3)) = 0
        _RimLightColorBias ("Rim Color Bias", Range(0, 1)) = 0
        _RimTex ("Rim Texture", 2D) = "white" { }
        _RimMask ("Rim Mask", 2D) = "white" { }
        [HideInInspector][Vector2]_RimTexPanSpeed ("Panning", Vector) = (0, 0, 0, 0)
        
        [HideInInspector] m_start_rimWidthNoise ("Width Noise", Float) = 0
        _RimWidthNoiseTexture ("Rim Width Noise", 2D) = "black" { }
        _RimWidthNoiseStrength ("Intensity", Range(0, 1)) = 0.1
        [HideInInspector][Vector2]_RimWidthNoisePan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector] m_end_rimWidthNoise ("Width Noise", Float) = 0
        [HideInInspector] m_start_ShadowMix ("Shadow Mix", Float) = 0
        _ShadowMix ("Shadow Mix In", Range(0, 1)) = 0
        _ShadowMixThreshold ("Shadow Mix Threshold", Range(0, 1)) = .5
        _ShadowMixWidthMod ("Shadow Mix Width Mod", Range(0, 10)) = .5
        [HideInInspector] m_end_ShadowMix ("Shadow Mix", Float) = 0
        [HideInInspector] m_end_rimLightOptions ("Rim Lighting", Float) = 0
        
        [HideInInspector] m_start_reflectionRim ("Environmental Rim", Float) = 0
        [Toggle(_MAPPING_6_FRAMES_LAYOUT)]_EnableEnvironmentalRim ("Enable Environmental Rim", Float) = 0
        _RimEnviroMask ("Mask", 2D) = "white" { }
        _RimEnviroBlur ("Blur", Range(0, 1)) = 0.7
        _RimEnviroWidth ("Rim Width", Range(0, 1)) = 0.45
        _RimEnviroSharpness ("Rim Sharpness", Range(0, 1)) = 0
        _RimEnviroMinBrightness ("Min Brightness Threshold", Range(0, 2)) = 0
        _RimEnviroIntensity ("Intensity", Range(0, 1)) = 1
        [HideInInspector] m_end_reflectionRim ("Environmental Rim", Float) = 0
        
        [HideInInspector] m_start_bakedLighting ("Baked Lighting", Float) = 0
        _GIEmissionMultiplier ("GI Emission Multiplier", Float) = 1
        [HideInInspector] DSGI ("DSGI", Float) = 0 //add this property for double sided illumination settings to be shown
        [HideInInspector] LightmapFlags ("Lightmap Flags", Float) = 0 //add this property for lightmap flags settings to be shown
        [HideInInspector] m_end_bakedLighting ("Baked Lighting", Float) = 0
        [HideInInspector] m_reflectionOptions ("Reflections", Float) = 0
        [HideInInspector] m_start_Metallic ("Metallics", Float) = 0
        [Toggle(_METALLICGLOSSMAP)]_EnableMetallic ("Enable Metallics", Float) = 0
        _CubeMap ("Baked CubeMap", Cube) = "" { }
        [Toggle(_)]_SampleWorld ("Force Baked Cubemap", Range(0, 1)) = 0
        _MetalReflectionTint ("Reflection Tint", Color) = (1, 1, 1)
        _MetallicMask ("Metallic Mask", 2D) = "white" { }
        _Metallic ("Metallic", Range(0, 1)) = 0
        _SmoothnessMask ("Smoothness Map", 2D) = "white" { }
        [Toggle(_)]_InvertSmoothness ("Invert Smoothness Map", Range(0, 1)) = 0
        _Smoothness ("Smoothness", Range(0, 1)) = 0
        [HideInInspector] m_end_Metallic ("Metallics", Float) = 0
        
        [HideInInspector] m_start_clearCoat ("Clear Coat", Float) = 0
        [Toggle(_COLORCOLOR_ON)]_EnableClearCoat ("Enable Clear Coat", Float) = 0
        [Enum(Vertex, 0, Pixel, 1)] _ClearCoatNormalToUse ("What Normal?", Int) = 0
        _ClearCoatCubeMap ("Baked CubeMap", Cube) = "" { }
        [Toggle(_)]_ClearCoatSampleWorld ("Force Baked Cubemap", Range(0, 1)) = 0
        _ClearCoatTint ("Reflection Tint", Color) = (1, 1, 1)
        _ClearCoatMask ("Mask", 2D) = "white" { }
        _ClearCoat ("Clear Coat", Range(0, 1)) = 1
        _ClearCoatSmoothnessMask ("Smoothness Map", 2D) = "white" { }
        [Toggle(_)]_ClearCoatInvertSmoothness ("Invert Smoothness Map", Range(0, 1)) = 0
        _ClearCoatSmoothness ("Smoothness", Range(0, 1)) = 0
        [Toggle(_)]_ClearCoatForceLighting ("Force Lighting", Float) = 0
        [HideInInspector] m_end_clearCoat ("Clear Coat", Float) = 0
        
        [HideInInspector] m_start_matcap ("Matcap / Sphere Textures", Float) = 0
        [Toggle(_COLORADDSUBDIFF_ON)]_MatcapEnable ("Enable Matcap", Float) = 0
        _MatcapColor ("Color", Color) = (1, 1, 1, 1)
        [TextureNoSO]_Matcap ("Matcap", 2D) = "white" { }
        _MatcapBorder ("Border", Range(0, .5)) = 0.43
        _MatcapMask ("Mask", 2D) = "white" { }
        _MatcapEmissionStrength ("Emission Strength", Range(0,20)) = 0
        _MatcapIntensity ("Intensity", Range(0, 5)) = 1
        _MatcapLightMask ("Hide in Shadow", Range(0, 1)) = 0
        _MatcapReplace ("Replace With Matcap", Range(0, 1)) = 1
        _MatcapMultiply ("Multiply Matcap", Range(0, 1)) = 0
        _MatcapAdd ("Add Matcap", Range(0, 1)) = 0
        [Enum(Vertex, 0, Pixel, 1)] _MatcapNormal ("Normal to use", Int) = 1
        [HideInInspector] m_end_matcap ("Matcap", Float) = 0
        [HideInInspector] m_start_Matcap2 ("Matcap 2", Float) = 0
        [Toggle(_)]_Matcap2Enable ("Enable Matcap 2", Float) = 0
        _Matcap2Color ("Color", Color) = (1, 1, 1, 1)
        [TextureNoSO]_Matcap2 ("Matcap", 2D) = "white" { }
        _Matcap2Border ("Border", Range(0, .5)) = 0.43
        _Matcap2Mask ("Mask", 2D) = "white" { }
        _Matcap2EmissionStrength ("Emission Strength", Range(0,20)) = 0
        _Matcap2Intensity ("Intensity", Range(0, 5)) = 1
        _Matcap2LightMask ("Hide in Shadow", Range(0, 1)) = 0
        _Matcap2Replace ("Replace With Matcap", Range(0, 1)) = 0
        _Matcap2Multiply ("Multiply Matcap", Range(0, 1)) = 0
        _Matcap2Add ("Add Matcap", Range(0, 1)) = 0
        [Enum(Vertex, 0, Pixel, 1)] _Matcap2Normal ("Normal to use", Int) = 1
        [HideInInspector] m_end_Matcap2 ("Matcap 2", Float) = 0
        
        [HideInInspector] m_start_specular ("Specular Reflections", Float) = 0
        [Toggle(_SPECGLOSSMAP)]_EnableSpecular ("Enable Specular", Float) = 0
        [Enum(Realistic, 1, Toon, 2, Anisotropic, 3)] _SpecularType ("Specular Type", Int) = 1
        [Enum(vertex, 0, pixel, 1)] _SpecularNormal ("Normal Select", Int) = 1
        _SpecularMinLightBrightness ("Min Light Brightness", Range(0, 1)) = 0
        _SpecularAttenuation("Attenuation Strength", Range(0,1)) = 1
        _SpecularTint ("Specular Tint", Color) = (.2, .2, .2, 1)
        _SpecularMixAlbedoIntoTint ("Mix Material Color Into Tint", Range(0, 1)) = 0
        _SpecularSmoothness ("Smoothness", Range(-2, 1)) = .75
        _SpecularMap ("Specular Map", 2D) = "white" { }
        [Toggle(_)]_SpecularInvertSmoothness ("Invert Smoothness", Float) = 0
        _SpecularMask ("Specular Mask", 2D) = "white" { }
        [Enum(Alpha, 0, Grayscale, 1)] _SmoothnessFrom ("Smoothness From", Int) = 1
        [HideInInspector] m_start_SpecularToon ("Toon", Float) = 0
        [MultiSlider]_SpecularToonInnerOuter ("Inner/Outer Edge", Vector) = (0.25, 0.3, 0, 1)
        [HideInInspector] m_end_SpecularToon ("Toon", Float) = 0
        [HideInInspector] m_start_Anisotropic ("Anisotropic", Float) = 0
        [Enum(Tangent, 0, Bitangent, 1)] _SpecWhatTangent ("(Bi)Tangent?", Int) = 0
        _AnisoSpec1Alpha ("Spec1 Alpha", Range(0, 1)) = 1
        _AnisoSpec2Alpha ("Spec2 Alpha", Range(0, 1)) = 1
        //_Spec1Offset ("Spec1 Offset", Float) = 0
        //_Spec1JitterStrength ("Spec1 Jitter Strength", Float) = 1.0
        _Spec2Smoothness ("Spec2 Smoothness", Range(0, 1)) = 0
        //_Spec2Offset ("Spec2 Offset", Float) = 0
        //_Spec2JitterStrength ("Spec2 Jitter Strength", Float) = 1.0
        [Toggle(_)]_AnisoUseTangentMap ("Use Directional Map?", Float) = 0
        _AnisoTangentMap ("Anisotropic Directional Map", 2D) = "bump" { }
        //_ShiftTexture ("Shift Texture", 2D) = "black" { }
        [HideInInspector] m_end_Anisotropic ("Anisotropic", Float) = 0
        [HideInInspector] m_end_specular ("Specular Reflections", Float) = 0

        [HideInInspector] m_start_specular1 ("Specular Reflections 2", Float) = 0
        [Toggle(_)]_EnableSpecular1 ("Enable Specular", Float) = 0
        [Enum(Realistic, 1, Toon, 2, Anisotropic, 3)] _SpecularType1 ("Specular Type", Int) = 1
        [Enum(vertex, 0, pixel, 1)] _SpecularNormal1 ("Normal Select", Int) = 1
        _SpecularMinLightBrightness1 ("Min Light Brightness", Range(0, 1)) = 0
        _SpecularAttenuation1("Attenuation Strength", Range(0,1)) = 1
        _SpecularTint1 ("Specular Tint", Color) = (.2, .2, .2, 1)
        _SpecularMixAlbedoIntoTint1 ("Mix Material Color Into Tint", Range(0, 1)) = 0
        _SpecularSmoothness1 ("Smoothness", Range(-2, 1)) = .75
        _SpecularMap1 ("Specular Map", 2D) = "white" { }
        [Toggle(_)]_SpecularInvertSmoothness1 ("Invert Smoothness", Float) = 0
        _SpecularMask1 ("Specular Mask", 2D) = "white" { }
        [Enum(Alpha, 0, Grayscale, 1)] _SmoothnessFrom1 ("Smoothness From", Int) = 1
        [HideInInspector] m_start_SpecularToon1 ("Toon", Float) = 0
        [MultiSlider]_SpecularToonInnerOuter1 ("Inner/Outer Edge", Vector) = (0.25, 0.3, 0, 1)
        [HideInInspector] m_end_SpecularToon1 ("Toon", Float) = 0
        [HideInInspector] m_start_Anisotropic1 ("Anisotropic", Float) = 0
        [Enum(Tangent, 0, Bitangent, 1)] _SpecWhatTangent1 ("(Bi)Tangent?", Int) = 0
        _AnisoSpec1Alpha1 ("Spec1 Alpha", Range(0, 1)) = 1
        _AnisoSpec2Alpha1 ("Spec2 Alpha", Range(0, 1)) = 1
        //_Spec1Offset ("Spec1 Offset", Float) = 0
        //_Spec1JitterStrength ("Spec1 Jitter Strength", Float) = 1.0
        _Spec2Smoothness1 ("Spec2 Smoothness", Range(0, 1)) = 0
        //_Spec2Offset ("Spec2 Offset", Float) = 0
        //_Spec2JitterStrength ("Spec2 Jitter Strength", Float) = 1.0
        [Toggle(_)]_AnisoUseTangentMap1 ("Use Directional Map?", Float) = 0
        _AnisoTangentMap1 ("Anisotropic Directional Map", 2D) = "bump" { }
        //_ShiftTexture ("Shift Texture", 2D) = "black" { }
        [HideInInspector] m_end_Anisotropic1 ("Anisotropic", Float) = 0
        [HideInInspector] m_end_specular1 ("Specular Reflections", Float) = 0        
        [HideInInspector] m_outlineOptions ("Outlines", Float) = 0
        [Enum(Basic, 0, Tint, 1, Rim Light, 2, Directional, 3, DropShadow, 4)]_OutlineMode ("Mode", Int) = 0
        [Toggle(_)]_OutlineFixedSize ("Fixed Size?", Float) = 0
        [Toggle(_)]_OutlineUseVertexColors ("V Color as Normal", Float) = 0
        [Toggle(_)]_OutlineLit ("Enable Lighting", Float) = 0
        _LineWidth ("Width", Float) = 0
        _LineColor ("Color", Color) = (1, 1, 1, 1)
        _OutlineEmission ("Outline Emission", Float) = 0
        _OutlineTexture ("Outline Texture", 2D) = "white" { }
        _OutlineMask ("Outline Mask", 2D) = "white" { }
        _OutlineTexturePan ("Outline Texture Pan", Vector) = (0, 0, 0, 0)
        _OutlineShadowStrength ("Shadow Strength", Range(0, 1)) = 1
        _OutlineRimLightBlend ("Rim Light Blend", Range(0, 1)) = 0
        _OutlinePersonaDirection ("directional Offset XY", Vector) = (1, 0, 0, 0)
        _OutlineDropShadowOffset ("Drop Direction XY", Vector) = (1, 0, 0, 0)
        [HideInInspector] m_start_outlineAdvanced ("Advanced", Float) = 0
        [Vector2]_OutlineFadeDistance ("Outline distance Fade", Vector) = (0, 0, 0, 0)
        [Enum(UnityEngine.Rendering.CullMode)] _OutlineCull ("Cull", Float) = 1
        [HideInInspector] m_end_outlineAdvanced ("Advanced", Float) = 0
        
        [HideInInspector] m_SpecialEffects ("Special Effects", Float) = 0
        [HideInInspector] m_start_emissionOptions ("Emission / Glow", Float) = 0
        [Toggle(_EMISSION)]_EnableEmission ("Enable Emission", Float) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _EmissionMaskUV ("Emission Mask UV", Int) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _EmissionMapUV ("Emission Map UV", Int) = 0
        [HDR]_EmissionColor ("Emission Color", Color) = (1, 1, 1, 1)
        [Gradient]_EmissionMap ("Emission Map", 2D) = "white" { }
        _EmissionMask ("Emission Mask", 2D) = "white" { }
        [HideInInspector][Vector2]_EmissionMapPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Vector2]_EmissionMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        _EmissionStrength ("Emission Strength", Range(0, 20)) = 0
        // Inward out emission
        [HideInInspector] m_start_CenterOutEmission ("Center Out Emission", Float) = 0
        [Toggle(_)]_EmissionCenterOutEnabled ("Enable Center Out", Float) = 0
        _EmissionCenterOutSpeed ("Flow Speed", Float) = 5
        [HideInInspector] m_end_CenterOutEmission ("inward out emission", Float) = 0
        //Glow in the dark Emission
        [HideInInspector] m_start_glowInDarkEmissionOptions ("Glow In The Dark Emission (Requires Lighting Enabled)", Float) = 0
        [Toggle(_)]_EnableGITDEmission ("Enable Glow In The Dark", Float) = 0
        [Enum(World, 0, Mesh, 1)] _GITDEWorldOrMesh ("Lighting Type", Int) = 0
        _GITDEMinEmissionMultiplier ("Min Emission Multiplier", Range(0, 1)) = 1
        _GITDEMaxEmissionMultiplier ("Max Emission Multiplier", Range(0, 1)) = 0
        _GITDEMinLight ("Min Lighting", Range(0, 1)) = 0
        _GITDEMaxLight ("Max Lighting", Range(0, 1)) = 1
        [HideInInspector] m_end_glowInDarkEmissionOptions ("Glow In The Dark Emission (Requires Lighting Enabled)", Float) = 0
        
        [HideInInspector] m_start_blinkingEmissionOptions ("Blinking Emission", Float) = 0
        _EmissiveBlink_Min ("Emissive Blink Min", Float) = 1
        _EmissiveBlink_Max ("Emissive Blink Max", Float) = 1
        _EmissiveBlink_Velocity ("Emissive Blink Velocity", Float) = 4
        [HideInInspector] m_end_blinkingEmissionOptions ("Blinking Emission", Float) = 0
        
        [HideInInspector] m_start_scrollingEmissionOptions ("Scrolling Emission", Float) = 0
        [Toggle(_)] _ScrollingEmission ("Enable Scrolling Emission", Float) = 0
        [Toggle(_)]_EmissionScrollingUseCurve ("Use Curve", float) = 0
        [Curve]_EmissionScrollingCurve ("Curve", 2D) = "white" { }
        _EmissiveScroll_Direction ("Emissive Scroll Direction", Vector) = (0, -10, 0, 0)
        _EmissiveScroll_Width ("Emissive Scroll Width", Float) = 10
        _EmissiveScroll_Velocity ("Emissive Scroll Velocity", Float) = 10
        _EmissiveScroll_Interval ("Emissive Scroll Interval", Float) = 20
        [HideInInspector] m_end_scrollingEmissionOptions ("Scrolling Emission", Float) = 0
        [HideInInspector] m_end_emissionOptions ("Emission / Glow", Float) = 0
        
        [HideInInspector] m_start_emission1Options ("Emission / Glow 2 (Requires Emission 1 Enabled)", Float) = 0
        [Toggle(_)]_EnableEmission1 ("Enable Emission 2", Float) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _EmissionMaskUV1("Emission Mask UV", Int) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _EmissionMapUV1 ("Emission UV#", Int) = 0
        [HDR]_EmissionColor1 ("Emission Color", Color) = (1, 1, 1, 1)
        [Gradient]_EmissionMap1 ("Emission Map", 2D) = "white" { }
        _EmissionMask1 ("Emission Mask", 2D) = "white" { }
        [HideInInspector][Vector2]_EmissionMapPan1 ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Vector2]_EmissionMaskPan1 ("Panning", Vector) = (0, 0, 0, 0)
        _EmissionStrength1 ("Emission Strength", Range(0, 20)) = 0
        [HideInInspector] m_start_CenterOutEmission1 ("Center Out Emission", Float) = 0
        [Toggle(_)]_EmissionCenterOutEnabled1 ("Enable Center Out", Float) = 0
        _EmissionCenterOutSpeed1 ("Flow Speed", Float) = 5
        [HideInInspector] m_end_CenterOutEmission1 ("inward out emission", Float) = 0
        [HideInInspector] m_start_glowInDarkEmissionOptions1 ("Glow In The Dark Emission (Requires Lighting Enabled)", Float) = 0
        [Toggle(_)]_EnableGITDEmission1 ("Enable Glow In The Dark", Float) = 0
        [Enum(World, 0, Mesh, 1)] _GITDEWorldOrMesh1 ("Lighting Type", Int) = 0
        _GITDEMinEmissionMultiplier1 ("Min Emission Multiplier", Range(0, 1)) = 1
        _GITDEMaxEmissionMultiplier1 ("Max Emission Multiplier", Range(0, 1)) = 0
        _GITDEMinLight1 ("Min Lighting", Range(0, 1)) = 0
        _GITDEMaxLight1 ("Max Lighting", Range(0, 1)) = 1
        [HideInInspector] m_end_glowInDarkEmissionOptions1 ("Glow In The Dark Emission (Requires Lighting Enabled)", Float) = 0
        [HideInInspector] m_start_blinkingEmissionOptions1 ("Blinking Emission", Float) = 0
        _EmissiveBlink_Min1 ("Emissive Blink Min", Float) = 1
        _EmissiveBlink_Max1 ("Emissive Blink Max", Float) = 1
        _EmissiveBlink_Velocity1 ("Emissive Blink Velocity", Float) = 4
        [HideInInspector] m_end_blinkingEmissionOptions1 ("Blinking Emission", Float) = 0
        [HideInInspector] m_start_scrollingEmissionOptions1 ("Scrolling Emission", Float) = 0
        [Toggle(_)] _ScrollingEmission1 ("Enable Scrolling Emission", Float) = 0
        [Toggle(_)]_EmissionScrollingUseCurve1 ("Use Curve", float) = 0
        [Curve]_EmissionScrollingCurve1 ("Curve", 2D) = "white" { }
        _EmissiveScroll_Direction1 ("Emissive Scroll Direction", Vector) = (0, -10, 0, 0)
        _EmissiveScroll_Width1 ("Emissive Scroll Width", Float) = 10
        _EmissiveScroll_Velocity1 ("Emissive Scroll Velocity", Float) = 10
        _EmissiveScroll_Interval1 ("Emissive Scroll Interval", Float) = 20
        [HideInInspector] m_end_scrollingEmission1Options ("Scrolling Emission", Float) = 0
        [HideInInspector] m_end_emission1Options ("Emission / Glow 2", Float) = 0
        
        [HideInInspector] m_start_flipBook ("Flipbook", Float) = 0
        [Toggle(_SUNDISK_HIGH_QUALITY)]_EnableFlipbook ("Enable Flipbook", Float) = 0
        [Toggle(_)]_FlipbookAlphaControlsFinalAlpha ("Flipbook Controls Alpha?", Float) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _FlipbookUV ("Flipbook UV#", Int) = 0
        [TextureArray]_FlipbookTexArray ("Texture Array", 2DArray) = "" { }
        _FlipbookMask ("Mask", 2D) = "white" { }
        _FlipbookColor ("Color & alpha", Color) = (1, 1, 1, 1)
        _FlipbookTotalFrames ("Total Frames", Int) = 1
        _FlipbookFPS ("FPS", Float) = 30.0
        _FlipbookScaleOffset ("Scale | Offset", Vector) = (1, 1, 0, 0)
        [Toggle(_)]_FlipbookTiled ("Tiled?", Float) = 0
        _FlipbookEmissionStrength ("Emission Strength", Range(0, 20)) = 0
        _FlipbookRotation ("Rotation", Range(0, 360)) = 0
        _FlipbookReplace ("Replace", Range(0, 1)) = 1
        _FlipbookMultiply ("Multiply", Range(0, 1)) = 0
        _FlipbookAdd ("Add", Range(0, 1)) = 0
        [HideInInspector] m_start_manualFlipbookControl ("Manual Control", Float) = 0
        _FlipbookCurrentFrame ("Current Frame", Float) = -1
        [HideInInspector] m_end_manualFlipbookControl ("Manual Control", Float) = 0
        [HideInInspector] m_end_flipBook ("Flipbook", Float) = 0
        
        [HideInInspector] m_start_dissolve ("Dissolve", Float) = 0
        [Toggle(_ALPHABLEND_ON)]_EnableDissolve ("Enable Dissolve", Float) = 0
        [Enum(Basic, 1, Point2Point, 2)] _DissolveType ("Dissolve Type", Int) = 1
        _DissolveEdgeWidth ("Edge Width", Range(0, .5)) = 0.025
        _DissolveEdgeHardness ("Edge Hardness", Range(0, 1)) = 0.5
        _DissolveEdgeColor ("Edge Color", Color) = (1, 1, 1, 1)
        [Gradient]_DissolveEdgeGradient ("Edge Gradient", 2D) = "white" { }
        _DissolveEdgeEmission ("Edge Emission", Range(0, 20)) = 0
        _DissolveTextureColor ("Dissolved Color", Color) = (1, 1, 1, 1)
        _DissolveToTexture ("Dissolved Texture", 2D) = "white" { }
        _DissolveToEmissionStrength ("Dissolved Emission Strength", Range(0, 20)) = 0
        [HideInInspector][Vector2]_DissolveToPanning ("Panning", Vector) = (0, 0, 0, 0)
        _DissolveNoiseTexture ("Dissolve Noise", 2D) = "white" { }
        [Toggle(_)]_DissolveInvertNoise ("Invert Noise", Float) = 0
        _DissolveDetailNoise ("Dissolve Detail Noise", 2D) = "black" { }
        [Toggle(_)]_DissolveInvertDetailNoise ("Invert Detail Noise", Float) = 0
        _DissolveDetailStrength ("Dissolve Detail Strength", Range(0, 1)) = 0.1
        [HideInInspector][Vector2]_DissolveNoisePan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Vector2]_DissolveDetailPan ("Panning", Vector) = (0, 0, 0, 0)
        _DissolveAlpha ("Dissolve Alpha", Range(0, 1)) = 0
        _DissolveMask ("Dissolve Mask", 2D) = "white" { }
        _ContinuousDissolve ("Continuous Dissolve Speed", Float) = 0
        [HideInInspector] m_start_pointToPoint ("point to point", Float) = 0
        [Enum(Local, 0, World, 1)] _DissolveP2PWorldLocal ("World/Local", Int) = 0
        _DissolveP2PEdgeLength ("Edge Length", Float) = 0.1
        [Vector3]_DissolveStartPoint ("Start Point", Vector) = (0, -1, 0, 0)
        [Vector3]_DissolveEndPoint ("End Point", Vector) = (0, 1, 0, 0)
        [HideInInspector] m_end_pointToPoint ("Point To Point", Float) = 0
        [HideInInspector] m_end_dissolve ("Dissolve", Float) = 0
        
        [HideInInspector] m_start_panosphereOptions ("Panosphere / Cubemaps", Float) = 0
        [Toggle(_DETAIL_MULX2)]_PanoToggle ("Enable Panosphere", Float) = 0
        _PanosphereColor ("Color", Color) = (1, 1, 1, 1)
        _PanosphereTexture ("Texture", 2D) = "white" { }
        _PanoMapTexture ("Mask", 2D) = "white" { }
        _PanoEmission ("Emission Strength", Range(0, 10)) = 0
        _PanoBlend ("Alpha", Range(0, 1)) = 0
        [Vector3]_PanospherePan ("Pan Speed", Vector) = (0, 0, 0, 0)
        [Toggle(_)]_PanoCubeMapToggle ("Use Cubemap", Float) = 0
        [TextureNoSO]_PanoCubeMap ("CubeMap", Cube) = "" { }
        [HideInInspector] m_end_panosphereOptions ("Panosphere / Cubemaps", Float) = 0
        
        [HideInInspector] m_start_glitter ("Glitter / Sparkle", Float) = 0
        [Toggle(_SUNDISK_SIMPLE)]_GlitterEnable ("Enable Glitter?", Float) = 0
        [HDR]_GlitterColor ("Color", Color) = (1, 1, 1)
        _GlitterUseSurfaceColor ("Use Surface Color", Range(0,1)) = 0
        _GlitterColorMap ("Glitter Color Map", 2D) = "white" { }
        [HideInInspector][Vector2]_GlitterPan ("Panning", Vector) = (0, 0, 0, 0)
        _GlitterMask ("Glitter Mask", 2D) = "white" { }
        _GlitterFrequency ("Glitter Density", Float) = 300.0
        _GlitterJitter ("Glitter Jitter", Range(0, 1)) = 1.0
        _GlitterSpeed ("Glitter Wobble Speed", Float) = 10.0
        _GlitterSize ("Glitter Size", Range(0, 1)) = .3
        _GlitterContrast ("Glitter Contrast", Range(1, 1000)) = 300
        _GlitterAngleRange ("Glitter Angle Range", Range(0, 90)) = 90
        _GlitterMinBrightness ("Glitter Min Brightness", Range(0, 1)) = 0
        _GlitterBrightness ("Glitter Max Brightness", Range(0, 40)) = 3
        _GlitterBias ("Glitter Bias", Range(0, 1)) = .8
        [HideInInspector] m_start_glitterRandom ("Random Colors", Float) = 0
        [Toggle(_)]_GlitterRandomColors ("Enable", Float) = 0
        [MultiSlider]_GlitterMinMaxSaturation ("Saturation Range", Vector) = (0.8, 1, 0, 1)
        [MultiSlider]_GlitterMinMaxBrightness ("Brightness Range", Vector) = (.8, 1, 0, 1)
        [HideInInspector] m_end_glitterRandom ("Random Colors", Float) = 0
        [HideInInspector] m_end_glitter ("Glitter / Sparkle", Float) = 0
        
        // MSDF OVERLAY
        [HideInInspector] m_start_Text ("MSDF Text Overlay", Float) = 0
        _TextGlyphs ("Font Array", 2D) = "black" { }
        _TextPixelRange ("Pixel Range", Float) = 4.0
        [Toggle(EFFECT_BUMP)]_TextEnabled ("Text?", Float) = 0
        // FPS
        [HideInInspector] m_start_TextFPS ("FPS", Float) = 0
        [Toggle(_)]_TextFPSEnabled ("FPS Text?", Float) = 0
        _TextFPSColor ("Color", Color) = (1, 1, 1, 1)
        _TextFPSEmissionStrength ("Emission Strength", Range(0, 20)) = 0
        [Vector2]_TextFPSOffset ("Offset", Vector) = (0, 0, 0, 0)
        _TextFPSRotation ("Rotation", Range(0, 360)) = 0
        [Vector2]_TextFPSScale ("Scale", Vector) = (1, 1, 1, 1)
        _TextFPSPadding ("Padding Reduction", Vector) = (0, 0, 0, 0)
        [HideInInspector] m_end_TextFPS ("FPS", Float) = 0
        // POSITION
        [HideInInspector] m_start_TextPosition ("Position", Float) = 0
        [Toggle(_)]_TextPositionEnabled ("Position Text?", Float) = 0
        //[Toggle(_)]_TextPositionVertical ("Vertical?", Float) = 0
        _TextPositionColor ("Color", Color) = (1, 0, 1, 1)
        _TextPositionEmissionStrength ("Emission Strength", Range(0, 20)) = 0
        [Vector2]_TextPositionOffset ("Offset", Vector) = (0, 0, 0, 0)
        _TextPositionRotation ("Rotation", Range(0, 360)) = 0
        [Vector2]_TextPositionScale ("Scale", Vector) = (1, 1, 1, 1)
        _TextPositionPadding ("Padding Reduction", Vector) = (0, 0, 0, 0)
        [HideInInspector] m_end_TextPosition ("Position", Float) = 0
        // INSTANCE TIME
        [HideInInspector] m_start_TextInstanceTime ("Instance Time", Float) = 0
        [Toggle(_)]_TextTimeEnabled ("Time Text?", Float) = 0
        _TextTimeColor ("Color", Color) = (1, 0, 1, 1)
        _TextTimeEmissionStrength ("Emission Strength", Range(0, 20)) = 0
        [Vector2]_TextTimeOffset ("Offset", Vector) = (0, 0, 0, 0)
        _TextTimeRotation ("Rotation", Range(0, 360)) = 0
        [Vector2]_TextTimeScale ("Scale", Vector) = (1,1, 1, 1)
        _TextTimePadding ("Padding Reduction", Vector) = (0, 0, 0, 0)
        [HideInInspector] m_end_TextInstanceTime ("Instance Time", Float) = 0
        [HideInInspector] m_end_Text ("MSDF Text Overlay", Float) = 0
        
        [HideInInspector] m_start_mirrorOptions ("Mirror", Float) = 0
        [Toggle(_REQUIRE_UV2)]_EnableMirrorOptions ("Enable Mirror Options", Float) = 0
        [Enum(ShowInBoth, 0, ShowOnlyInMirror, 1, DontShowInMirror, 2)] _Mirror ("Show in mirror", Int) = 0
        [Toggle(_)]_EnableMirrorTexture ("Enable Mirror Texture", Float) = 0
        _MirrorTexture ("Mirror Tex", 2D) = "white" { }
        [HideInInspector] m_end_mirrorOptions ("Mirror", Float) = 0
        
        [HideInInspector] m_start_distanceFade ("Distance Fade", Float) = 0
        _MainMinAlpha ("Minimum Alpha", Range(0, 1)) = 0
        _MainFadeTexture ("Fade Mask", 2D) = "white" { }
        [Vector2]_MainDistanceFade ("Distance Fade X to Y", Vector) = (0, 0, 0, 0)
        [HideInInspector] m_end_Fade ("Distance Fade", Float) = 0
        
        [HideInInspector] m_start_angularFade ("Angular Fade", Float) = 0
        [Toggle(_SUNDISK_NONE)]_EnableRandom ("Enable Angular Fade", Float) = 0
        [Enum(Camera Face Model, 0, Model Face Camera, 1, Face Each Other, 2)] _AngleType ("Angle Type", Int) = 0
        [Enum(Model, 0, Vertex, 1)] _AngleCompareTo ("Model or Vert Positon", Int) = 0
        [Vector3]_AngleForwardDirection ("Forward Direction", Vector) = (0, 0, 1, 0)
        _CameraAngleMin ("Camera Angle Min", Range(0, 180)) = 45
        _CameraAngleMax ("Camera Angle Max", Range(0, 180)) = 90
        _ModelAngleMin ("Model Angle Min", Range(0, 180)) = 45
        _ModelAngleMax ("Model Angle Max", Range(0, 180)) = 90
        _AngleMinAlpha ("Min Alpha", Range(0, 1)) = 0
        [HideInInspector] m_end_angularFade ("Angular Fade", Float) = 0

        [HideInInspector] m_start_distanceDithering ("Distance Dither", Float) = 0
        [Toggle(_)]_DitheringDistanceEnabled ("Distance Dithering", Float) = 0
        _DitheringOpaqueRange ("Dither Opaque Range", Float) = 20
        _DitheringInvisibleRange ("Dither Invisible Range", Float) = 30
        _DitheringDistanceMinAlpha ("Dither Min Dissolve", Range(0, 1.001)) = 0
        _DitheringDistanceMaxAlpha ("Dither Max Dissolve", Range(0, 1.001)) = 0.501
        [HideInInspector] m_end_distanceDithering ("Distance Dither", Float) = 0
        

        [HideInInspector] m_start_distortionFlow ("UV Distortion", Float) = 0
        [Toggle(USER_LUT)] _EnableDistortion ("Enabled?", Float) = 0
        _DistortionFlowTexture ("Distortion Texture 1", 2D) = "black" { }
        _DistortionFlowTexture1 ("Distortion Texture 2", 2D) = "black" { }
        _DistortionStrength ("Strength1", Float) = 0.5
        _DistortionStrength1 ("Strength2", Float) = 0.5
        [Vector2]_DistortionSpeed ("Speed1", Vector) = (0.5, 0.5, 0, 0)
        [Vector2]_DistortionSpeed1 ("Speed2", Vector) = (0.5, 0.5, 0, 0)
        [HideInInspector] m_end_distortionFlow ("UV Distortion", Float) = 0
        // End Special Effects
        

        
        [HideInInspector] m_ParallaxMap ("Parallax", Float) = 0
        [Toggle(_PARALLAXMAP)]_ParallaxMap ("Enable Parallax FX", Float) = 0
        [Toggle(_)]_ParallaxHeightMapEnabled ("Enable Parallax Height", Float) = 0
        [Toggle(_)]_ParallaxInternalMapEnabled ("Enable Parallax Internal", Float) = 0
                [HideInInspector] m_start_parallaxHeightmap ("Heightmap", Float) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] _ParallaxUV ("Parallax UV", Int) = 0
        _ParallaxHeightMap ("Height Map", 2D) = "black" { }
        _ParallaxStrength ("Parallax Strength", Range(0, 1)) = 0
        [HideInInspector] m_end_parallaxHeightmap ("Heightmap", Float) = 0
        [HideInInspector] m_start_parallaxInternal ("Internal", Float) = 0
        [Enum(Basic, 0, HeightMap, 1)] _ParallaxInternalHeightmapMode ("Parallax Mode", Int) = 0
        [Toggle(_)]_ParallaxInternalHeightFromAlpha ("HeightFromAlpha", Float) = 0
        _ParallaxInternalMap ("Internal Map", 2D) = "black" { }
        _ParallaxInternalIterations ("Parallax Internal Iterations", Range(1, 50)) = 1
        _ParallaxInternalMinDepth ("Min Depth", Float) = 0
        _ParallaxInternalMaxDepth ("Max Depth", Float) = 1
        _ParallaxInternalMinFade ("Min Depth Brightness", Range(0, 5)) = 0
        _ParallaxInternalMaxFade ("Max Depth Brightness", Range(0, 5)) = 1
        _ParallaxInternalMinColor ("Min Depth Color", Color) = (1, 1, 1, 1)
        _ParallaxInternalMaxColor ("Max Depth Color", Color) = (1, 1, 1, 1)
        [Vector2]_ParallaxInternalPanSpeed ("Pan Speed", Vector) = (0, 0, 0, 0)
        [Vector2]_ParallaxInternalPanDepthSpeed ("Per Level Speed Multiplier", Vector) = (0, 0, 0, 0)
        [HideInInspector] m_end_parallaxInternal ("Internal", Float) = 0
        [HideInInspector] m_start_parallaxAdvanced ("Advanced", Float) = 0
        _ParallaxBias ("Parallax Bias (0.42)", Float) = 0.42
        [HideInInspector] m_end_parallaxAdvanced ("Advanced", Float) = 0
        
        [HideInInspector] m_renderingOptions ("Rendering Options", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4
        [Enum(UnityEngine.Rendering.BlendMode)] _SourceBlend ("Source Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DestinationBlend ("Destination Blend", Float) = 10
        [Enum(Off, 0, On, 1)] _ZWrite ("ZWrite", Int) = 0
        _ZBias ("ZBias", Float) = 0.0
        [Toggle(_)]_IgnoreFog ("Ignore Fog", Float) = 0
        [HideInInspector] Instancing ("Instancing", Float) = 0 //add this property for instancing variants settings to be shown
        
        [HideInInspector] m_start_StencilPassOptions ("Stencil", Float) = 0
        [IntRange] _StencilRef ("Stencil Reference Value", Range(0, 255)) = 0
        //[IntRange] _StencilReadMaskRef ("Stencil ReadMask Value", Range(0, 255)) = 0
        //[IntRange] _StencilWriteMaskRef ("Stencil WriteMask Value", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilPassOp ("Stencil Pass Op", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilFailOp ("Stencil Fail Op", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFailOp ("Stencil ZFail Op", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilCompareFunction ("Stencil Compare Function", Float) = 8
        [HideInInspector] m_end_StencilPassOptions ("Stencil", Float) = 0
        
        [HideInInspector] m_start_OutlineStencil ("Outline Stencil", Float) = 0
        [IntRange] _OutlineStencilRef ("Stencil Reference Value", Range(0, 255)) = 0
        //[IntRange] _OutlineStencilReadMaskRef ("Stencil ReadMask Value", Range(0, 255)) = 0
        //[IntRange] _OutlineStencilWriteMaskRef ("Stencil WriteMask Value", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _OutlineStencilPassOp ("Stencil Pass Op", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _OutlineStencilFailOp ("Stencil Fail Op", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _OutlineStencilZFailOp ("Stencil ZFail Op", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _OutlineStencilCompareFunction ("Stencil Compare Function", Float) = 8
        [HideInInspector] m_end_OutlineStencil ("Outline Stencil", Float) = 0
        
        [HideInInspector] m_start_debugOptions ("Debug", Float) = 0
        [Toggle(_COLOROVERLAY_ON)]_DebugDisplayDebug ("Display Debug Info", Float) = 0
        [Enum(Off, 0, Vertex Normal, 1, Pixel Normal, 2, Tangent, 3, Binormal, 4)] _DebugMeshData ("Mesh Data", Int) = 0
        [Enum(Off, 0, Attenuation, 1, Direct Lighting, 2, Indirect Lighting, 3, light Map, 4, Ramped Light Map, 5, Final Lighting, 6)] _DebugLightingData ("Lighting Data", Int) = 0
        [Enum(Off, 0, finalSpecular, 1, tangentDirectionMap, 2, shiftTexture, 3)] _DebugSpecularData ("Specular Data", Int) = 0
        [Enum(Off, 0, View Dir, 1, Tangent View Dir, 2, Forward Dir, 3, WorldPos, 4, View Dot Normal, 5)] _DebugCameraData ("Camera Data", Int) = 0
        [HideInInspector] m_end_debugOptions ("Debug", Float) = 0
    }
    
    //originalEditorCustomEditor "PoiToon"
    CustomEditor "ThryEditor"
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        //Blend SrcAlpha OneMinusSrcAlpha
        Blend [_SourceBlend] [_DestinationBlend]
        
        Pass
        {
            Name "MainPass"
            Tags { "LightMode" = "ForwardBase" }
            
            Stencil
            {
                Ref [_StencilRef]
                Comp [_StencilCompareFunction]
                Pass [_StencilPassOp]
                Fail [_StencilFailOp]
                ZFail [_StencilZFailOp]
            }
            ZWrite [_ZWrite]
            Cull [_Cull]
            ZTest [_ZTest]
            Offset [_ZBias], [_ZBias]
            CGPROGRAM
            
            #pragma target 4.0
            #define TRANSPARENT
            #define FORWARD_BASE_PASS
            // UV Distortion
            #pragma shader_feature USER_LUT
            #pragma shader_feature _PARALLAXMAP
            // Mirror
            #pragma shader_feature _REQUIRE_UV2
            // Random
            #pragma shader_feature _SUNDISK_NONE
            // Dissolve
            #pragma shader_feature _ALPHABLEND_ON
            // Panosphere
            #pragma shader_feature _DETAIL_MULX2
            // Lighting
            #pragma shader_feature LOD_FADE_CROSSFADE
            // Flipbook
            #pragma shader_feature _SUNDISK_HIGH_QUALITY
            // Rim Lighting
            #pragma shader_feature _GLOSSYREFLECTIONS_OFF
            // Enviro Rim
            #pragma shader_feature _MAPPING_6_FRAMES_LAYOUT
            // Metal
            #pragma shader_feature _METALLICGLOSSMAP
            // Matcap
            #pragma shader_feature _COLORADDSUBDIFF_ON
            // Specular
            #pragma shader_feature _SPECGLOSSMAP
            // SubSurface
            #pragma shader_feature _TERRAIN_NORMAL_MAP
            // Debug
            #pragma shader_feature _COLOROVERLAY_ON
            // Glitter
            #pragma shader_feature _SUNDISK_SIMPLE
            // RGBMask
            #pragma shader_feature FXAA
            // Text
            #pragma shader_feature EFFECT_BUMP
            #pragma shader_feature _EMISSION
            // Clear Coat
            #pragma shader_feature _COLORCOLOR_ON
            #pragma multi_compile_instancing
            #pragma multi_compile_fwdbase
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_fog
            #pragma vertex vert
            #pragma fragment frag
            #include "../Includes/PoiPass.cginc"
            ENDCG
            
        }
        
        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }
            Stencil
            {
                Ref [_StencilRef]
                Comp [_StencilCompareFunction]
                Pass [_StencilPassOp]
                Fail [_StencilFailOp]
                ZFail [_StencilZFailOp]
            }
            ZWrite Off
            Blend One One
            Cull [_Cull]
            ZTest [_ZTest]
            Offset [_ZBias], [_ZBias]
            CGPROGRAM
            
            #pragma target 4.0
            #define FORWARD_ADD_PASS
            #define TRANSPARENT
            // UV Distortion
            #pragma shader_feature USER_LUT
            #pragma shader_feature _PARALLAX_MAP
            // Mirror
            #pragma shader_feature _REQUIRE_UV2
            // Random
            #pragma shader_feature _SUNDISK_NONE
            // Dissolve
            #pragma shader_feature _ALPHABLEND_ON
            // Panosphere
            #pragma shader_feature _DETAIL_MULX2
            // POI_LIGHTING
            #pragma shader_feature LOD_FADE_CROSSFADE
            // Flipbook
            #pragma shader_feature _SUNDISK_HIGH_QUALITY
            // Rim Lighting
            #pragma shader_feature _GLOSSYREFLECTIONS_OFF
            // Metal
            #pragma shader_feature _METALLICGLOSSMAP
            // Matcap
            #pragma shader_feature _COLORADDSUBDIFF_ON
            // Specular
            #pragma shader_feature _SPECGLOSSMAP
            // SubSurface
            #pragma shader_feature _TERRAIN_NORMAL_MAP
            // RGBMask
            #pragma shader_feature FXAA
            // Text
            #pragma shader_feature EFFECT_BUMP
            // Debug
            #pragma shader_feature _COLOROVERLAY_ON
            #pragma multi_compile_instancing
            #pragma multi_compile_fwdadd_fullshadows
            #pragma vertex vert
            #pragma fragment frag
            #include "../Includes/PoiPass.cginc"
            ENDCG
            
        }
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "ForwardBase" }
            Stencil
            {
                Ref [_OutlineStencilRef]
                Comp [_OutlineStencilCompareFunction]
                Pass [_OutlineStencilPassOp]
                Fail [_OutlineStencilFailOp]
                ZFail [_OutlineStencilZFailOp]
            }
            ZTest [_ZTest]
            Offset [_ZBias], [_ZBias]
            ZWrite [_ZWrite]
            Cull [_OutlineCull]
            CGPROGRAM
            
            #pragma target 4.0
            #define FORWARD_BASE_PASS
            #define TRANSPARENT
            #define OUTLINE
            // UV Distortion
            #pragma shader_feature USER_LUT
            // Mirror
            #pragma shader_feature _REQUIRE_UV2
            // Random
            #pragma shader_feature _SUNDISK_NONE
            // Dissolve
            #pragma shader_feature _ALPHABLEND_ON
            // Lighting
            #pragma shader_feature LOD_FADE_CROSSFADE
            #pragma multi_compile_fwdbase
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag
            #include "../Includes/PoiPassOutline.cginc"
            ENDCG
            
        }
        /*
        Pass
        {
            Tags { "LightMode" = "ShadowCaster" }
            Stencil
            {
                Ref [_StencilRef]
                Comp [_StencilCompareFunction]
                Pass [_StencilPassOp]
                Fail [_StencilFailOp]
                ZFail [_StencilZFailOp]
            }
            ZWrite [_ZWrite]
            Cull [_Cull]
            ZTest [_ZTest]
            Offset [_ZBias], [_ZBias]
            CGPROGRAM
            
            #pragma target 4.0
            #define TRANSPARENT
            #define POI_SHADOW
            // Flipbook
            #pragma shader_feature _SUNDISK_HIGH_QUALITY
            // Mirror
            #pragma shader_feature _REQUIRE_UV2
            // Random
            #pragma shader_feature _SUNDISK_NONE
            // Dissolve
            #pragma shader_feature _ALPHABLEND_ON
            #pragma multi_compile_instancing
            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster
            #include "../Includes/PoiPassShadow.cginc"
            ENDCG
            
        }
        */
        Pass
        {
            Tags { "LightMode" = "Meta" }
            Cull Off
            CGPROGRAM
            
            #define TRANSPARENT
            #define POI_META_PASS
            // UV Distortion
            #pragma shader_feature USER_LUT
            #pragma shader_feature _PARALLAXMAP
            // Mirror
            #pragma shader_feature _REQUIRE_UV2
            // Random
            #pragma shader_feature _SUNDISK_NONE
            // Dissolve
            #pragma shader_feature _ALPHABLEND_ON
            // Panosphere
            #pragma shader_feature _DETAIL_MULX2
            // Lighting
            #pragma shader_feature LOD_FADE_CROSSFADE
            // Flipbook
            #pragma shader_feature _SUNDISK_HIGH_QUALITY
            // Rim Lighting
            #pragma shader_feature _GLOSSYREFLECTIONS_OFF
            // Metal
            #pragma shader_feature _METALLICGLOSSMAP
            // Matcap
            #pragma shader_feature _COLORADDSUBDIFF_ON
            // Specular
            #pragma shader_feature _SPECGLOSSMAP
            // SubSurface
            #pragma shader_feature _TERRAIN_NORMAL_MAP
            // Debug
            #pragma shader_feature _COLOROVERLAY_ON
            // Glitter
            #pragma shader_feature _SUNDISK_SIMPLE
            // RGBMask
            #pragma shader_feature FXAA
            // Text
            #pragma shader_feature EFFECT_BUMP
            #pragma shader_feature _EMISSION
            // Clear Coat
            #pragma shader_feature _COLORCOLOR_ON
            #pragma vertex vert
            #pragma fragment frag
            #include "../Includes/PoiPass.cginc"
            ENDCG
            
        }
    }
    Fallback "Toon/Lit Cutout (Double)"
}
