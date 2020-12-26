Shader ".poiyomi/Toon/Transparent"
{
    Properties
    {
        [HideInInspector] shader_is_using_thry_editor ("", Float) = 0
        [HideInInspector] shader_master_label ("<color=#ff0000ff>❤</color> <color=#000000ff>Poiyomi Toon V6.1</color> <color=#ff0000ff>❤</color>", Float) = 0
        [HideInInspector] shader_presets ("poiToonPresets", Float) = 0
        [HideInInspector] shader_properties_label_file ("PoiLabels", Float) = 0
        
        [HideInInspector] footer_youtube ("youtube footer button", Float) = 0
        [HideInInspector] footer_twitter ("twitter footer button", Float) = 0
        [HideInInspector] footer_patreon ("patreon footer button", Float) = 0
        [HideInInspector] footer_discord ("discord footer button", Float) = 0
        [HideInInspector] footer_github ("github footer button", Float) = 0
        
        // Main
        [HideInInspector] m_mainOptions ("Main", Float) = 0
        _Color ("Color & Alpha", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" { }
        [HideInInspector][Vector2]_MainTexPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _MainTextureUV ("UV", Int) = 0
        _Saturation ("Saturation", Range(-1, 1)) = 0
        _MainVertexColoring ("Use Vertex Color", Range(0, 1)) = 0
        _MainEmissionStrength ("Basic Emission", Range(0, 20)) = 0
        [Normal]_BumpMap ("Normal Map", 2D) = "bump" { }
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _BumpMapUV ("UV", Int) = 0
        [HideInInspector][Vector2]_BumpMapPan ("Panning", Vector) = (0, 0, 0, 0)
        _BumpScale ("Normal Intensity", Range(0, 10)) = 1
        _AlphaMask ("Alpha Map", 2D) = "white" { }
        [HideInInspector][Vector2]_AlphaMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _AlphaMaskUV ("UV", Int) = 0
        
        //Hue Shifting
        [HideInInspector] m_start_MainHueShift ("Hue Shift", Float) = 0
        [HideInInspector][ToggleUI]_MainHueShiftToggle ("Toggle Hueshift", Float) = 0
        [ToggleUI]_MainHueShiftReplace ("Replace?", Float) = 1
        _MainHueShift ("Hue Shift", Range(0, 1)) = 0
        _MainHueShiftSpeed ("Shift Speed", Float) = 0
        _MainHueShiftMask ("Mask", 2D) = "white" { }
        [HideInInspector][Vector2]_MainHueShiftMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _MainHueShiftMaskUV ("UV", Int) = 0
        [HideInInspector] m_end_MainHueShift ("Hue Shift", Float) = 0
        
        // RGB Masking
        [HideInInspector] m_start_RGBMask ("RGB Color Masking", Float) = 0
        [HideInInspector][Toggle(FXAA)]_RGBMaskEnabled ("RGB Mask Enabled", Float) = 0
        [ToggleUI]_RGBBlendMultiplicative ("Multiplicative?", Float) = 0
        _RGBMask ("Mask", 2D) = "white" { }
        [HideInInspector][Vector2]_RGBMaskPanning ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)]_RGBMaskUV ("UV", int) = 0
        _RedColor ("R Color", Color) = (1, 1, 1, 1)
        _RedTexure ("R Texture", 2D) = "white" { }
        [HideInInspector][Vector2]_RGBRedPanning ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)]_RGBRed_UV ("UV", int) = 0
        _GreenColor ("G Color", Color) = (1, 1, 1, 1)
        _GreenTexture ("G Texture", 2D) = "white" { }
        [HideInInspector][Vector2]_RGBGreenPanning ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)]_RGBGreen_UV ("UV", int) = 0
        _BlueColor ("B Color", Color) = (1, 1, 1, 1)
        _BlueTexture ("B Texture", 2D) = "white" { }
        [HideInInspector][Vector2]_RGBBluePanning ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)]_RGBBlue_UV ("UV", int) = 0
        [HideInInspector] m_end_RGBMask ("RGB Color Masking", Float) = 0
        
        // Detail Options
        [HideInInspector] m_start_DetailOptions ("Details", Float) = 0
        _DetailMask ("Detail Mask (R:Texture, G:Normal)", 2D) = "white" { }
        [HideInInspector][Vector2]_DetailMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _DetailMaskUV ("UV", Int) = 0
        _DetailTint ("Detail Texture Tint", Color) = (1, 1, 1)
        _DetailTex ("Detail Texture", 2D) = "gray" { }
        [HideInInspector][Vector2]_DetailTexPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _DetailTexUV ("UV", Int) = 0
        _DetailTexIntensity ("Detail Tex Intensity", Range(0, 10)) = 1
        _DetailBrightness ("Detail Brightness:", Range(0, 2)) = 1
        [Normal]_DetailNormalMap ("Detail Normal", 2D) = "bump" { }
        _DetailNormalMapScale ("Detail Normal Intensity", Range(0, 10)) = 1
        [HideInInspector][Vector2]_DetailNormalMapPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _DetailNormalMapUV ("UV", Int) = 0
        [HideInInspector] m_end_DetailOptions ("Details", Float) = 0
        
        //Vertex Manipulations
        [HideInInspector] m_start_vertexManipulation ("Vertex Options", Float) = 0
        [Vector3]_VertexManipulationLocalTranslation ("Local Translation", Vector) = (0, 0, 0, 1)
        [Vector3]_VertexManipulationLocalRotation ("Local Rotation", Vector) = (0, 0, 0, 1)
        [Vector3]_VertexManipulationLocalScale ("Local Scale", Vector) = (1, 1, 1, 1)
        [Vector3]_VertexManipulationWorldTranslation ("World Translation", Vector) = (0, 0, 0, 1)
        _VertexManipulationHeight ("Vertex Height", Float) = 0
        _VertexManipulationHeightMask ("Height Map", 2D) = "while" { }
        [HideInInspector][Vector2]_VertexManipulationHeightPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] _VertexManipulationHeightUV ("UV", Int) = 0
        _VertexManipulationHeightBias ("Mask Bias", Range(0, 1)) = 0
        [ToggleUI]_VertexRoundingEnabled ("Rounding Enabled", Float) = 0
        _VertexRoundingDivision ("Division Amount", Float) = 500
        [HideInInspector] m_end_vertexManipulation ("Vertex Options", Float) = 0
        
        // Alpha Options
        [HideInInspector] m_start_Alpha ("Alpha Options", Float) = 0
        _AlphaMod ("Alpha Mod", Range(-1, 1)) = 0.0
        _Clip ("Alpha Cuttoff", Range(0, 1.001)) = 0.01
        [ToggleUI]_DitheringEnabled ("Enable Dithering", Float) = 1
        _DitherGradient ("Dither Gradient", Range(0, 1)) = .1
        [ToggleUI]_ForceOpaque ("Force Opaque", Float) = 0
        _MainShadowClipMod ("Shadow Clip Mod", Range(-1, 1)) = 0
        [HideInInspector] m_end_Alpha ("Alpha Options", Float) = 0
        
        // Decal Texture
        [HideInInspector] m_start_DecalSection ("Decal", Float) = 0
        [HideInInspector][Toggle(GEOM_TYPE_BRANCH)]_DecalEnabled ("Enable", Float) = 0
        _DecalColor ("Color", Color) = (1, 1, 1, 1)
        _DecalTexture ("Decal", 2D) = "white" { }
        [HideInInspector][Vector2]_DecalTexturePan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _DecalTextureUV ("UV", Int) = 0
        _DecalMask ("Mask", 2D) = "white" { }
        [HideInInspector][Vector2]_DecalMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _DecalMaskUV ("UV", Int) = 0
        [ToggleUI]_DecalTiled ("Tiled?", Float) = 0
        [Vector2]_DecalScale ("Scale", Vector) = (1, 1, 0, 0)
        [Vector2]_DecalPosition ("Position", Vector) = (.5, .5, 0, 0)
        _DecalRotation ("Rotation", Range(0, 360)) = 0
        _DecalRotationSpeed ("Rotation Speed", Float) = 0
        _DecalBlendAdd ("Add", Range(0, 1)) = 0
        _DecalBlendMultiply ("Multiply", Range(0, 1)) = 0
        _DecalBlendReplace ("Replace", Range(0, 1)) = 0
        [HideInInspector] m_end_DecalSection ("Decal", Float) = 0
        
        // Back Face Textures and Emission
        [HideInInspector] m_start_backFace ("Back Face", Float) = 0
        [ToggleUI]_BackFaceEnabled ("Enable Back Face Options", Float) = 0
        _BackFaceTexture ("Texture", 2D) = "white" { }
        [HideInInspector][Vector2]_BackFacePanning ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)]_BackFaceTextureUV ("UV#", Int) = 0
        _BackFaceDetailIntensity ("Detail Intensity", Range(0, 5)) = 1
        _BackFaceHueShift ("Hue Shift", Range(0, 1)) = 0
        _BackFaceEmissionStrength ("Emission Strength", Range(0, 20)) = 0
        [HideInInspector] m_end_backFace ("Back Face", Float) = 0
        
        // Lighting
        [HideInInspector] m_lightingOptions ("Lighting", Float) = 0
        [HideInInspector] m_start_Lighting ("Light and Shadow", Float) = 0
        [Toggle(LOD_FADE_CROSSFADE)]_EnableLighting ("Enable Lighting", Float) = 1
        [Enum(Natural, 0, Controlled, 1, Standardish, 2, Math, 3)] _LightingType ("Lighting Type", Int) = 1
        [ToggleUI]_LightingMonochromatic ("Monochromatic?", Float) = 0
        _LightingGradientStart ("Gradient Start--{condition_show:{type:PROPERTY_BOOL,data:_LightingType==3}}", Range(0, 1)) = 0
        _LightingGradientEnd ("Gradient End--{condition_show:{type:PROPERTY_BOOL,data:_LightingType==3}}", Range(0, 1)) = .5
        _LightingStartColor ("Light Tint--{condition_show:{type:PROPERTY_BOOL,data:_LightingType==3}}", Color) = (1, 1, 1)
        _LightingEndColor ("Shadow Tint--{condition_show:{type:PROPERTY_BOOL,data:_LightingType==3}}", Color) = (1, 1, 1)
        [Gradient]_ToonRamp ("Lighting Ramp--{texture:{width:512,height:4,filterMode:Bilinear,wrapMode:Clamp},force_texture_options:true,condition_show:{type:OR,condition1:{type:PROPERTY_BOOL,data:_LightingType==0},condition2:{type:PROPERTY_BOOL,data:_LightingType==1}}}", 2D) = "white" { }
        _LightingShadowMask ("Shadow Mask (RGBA)--{reference_properties:[_LightingShadowMaskPan, _LightingShadowMaskUV],condition_show:{type:PROPERTY_BOOL,data:_LightingType!=2}}", 2D) = "white" { }
        [HideInInspector][Vector2]_LightingShadowMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _LightingShadowMaskUV ("UV", Int) = 0
        _ShadowStrength ("Shadow Strength--{condition_show:{type:OR,condition1:{type:PROPERTY_BOOL,data:_LightingType==0},condition2:{type:PROPERTY_BOOL,data:_LightingType==1}}}", Range(0, 1)) = .2
        _ShadowOffset ("Shadow Offset--{condition_show:{type:OR,condition1:{type:PROPERTY_BOOL,data:_LightingType==0},condition2:{type:PROPERTY_BOOL,data:_LightingType==1}}}", Range(-1, 1)) = 0
        _LightingAOTex ("AO Map", 2D) = "white" { }
        [HideInInspector][Vector2]_LightingAOTexPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _LightingAOTexUV ("UV", Int) = 0
        _AOStrength ("AO Strength", Range(0, 1)) = 0
        _LightingMinLightBrightness ("Min Brightness--{condition_show:{type:PROPERTY_BOOL,data:_LightingType!=2}}", Range(0, 1)) = 0
        _LightingIndirectContribution ("Indirect Contribution--{condition_show:{type:PROPERTY_BOOL,data:_LightingType!=2}}", Range(0, 1)) = .2
        _AttenuationMultiplier ("Recieve Casted Shadows?", Range(0, 1)) = 0
        _LightingDetailShadows ("Detail Shadows--{reference_properties:[_LightingDetailShadowsPan, _LightingDetailShadowsUV],condition_show:{type:PROPERTY_BOOL,data:_LightingType!=2}}", 2D) = "white" { }
        [HideInInspector][Vector2]_LightingDetailShadowsPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _LightingDetailShadowsUV ("UV", Int) = 0
        _LightingDetailStrength ("Detail Strength--{condition_show:{type:PROPERTY_BOOL,data:_LightingType!=2}}", Range(0, 1)) = 1
        _LightingStandardSmoothness ("Smoothness--{condition_show:{type:PROPERTY_BOOL,data:_LightingType==2}}", Range(0, 1)) = 0
        
        // Lighting Beta Options
        [HideInInspector] m_start_lightingBeta ("Extra Ramps--{condition_show:{type:OR,condition1:{type:PROPERTY_BOOL,data:_LightingType==0},condition2:{type:PROPERTY_BOOL,data:_LightingType==1}}}", Float) = 0
        _LightingNoIndirectThreshold ("Absent Indirect Threshold", Range(0, 1)) = 0.01
        _LightingNoIndirectMultiplier ("Absent Indirect Multiplier", Range(0, 1)) = 0.5
        [ToggleUI]_LightingStandardControlsToon ("Standard Lighting Controls Toon Ramp", Float) = 0
        [IntRange]_LightingNumRamps ("Num Ramps", Range(1, 3)) = 1
        [Gradient]_ToonRamp1 ("Lighting Ramp 2", 2D) = "white" { }
        _LightingShadowStrength1 ("Shadow Strength 2", Range(0, 1)) = 1
        _ShadowOffset1 ("Shadow Offset 2", Range(-1, 1)) = 0
        [Gradient]_ToonRamp2 ("Lighting Ramp 3", 2D) = "white" { }
        _LightingShadowStrength2 ("Shadow Strength 3", Range(0, 1)) = 1
        _ShadowOffset2 ("Shadow Offset 3", Range(-1, 1)) = 0
        [HideInInspector] m_end_lightingBeta ("Extra Ramps", Float) = 0
        
        // point/spot Light Settings
        [HideInInspector] m_start_lightingAdvanced ("Additive Lighting", Float) = 0
        [Enum(Standard, 0, Controlled, 1)] _LightingAdditiveType ("Lighting Type", Int) = 1
        _LightingAdditiveGradientStart ("Gradient Start", Range(0, 1)) = 0
        _LightingAdditiveGradientEnd ("Gradient End", Range(0, 1)) = .5
        _LightingAdditivePassthrough ("Point Light Passthrough", Range(0, 1)) = .5
        _LightingAdditiveDetailStrength ("Detail Shadow Strength", Range(0, 1)) = 1
        [HideInInspector] m_end_lightingAdvanced ("Additive Lighting", Float) = 0
        [HideInInspector] m_end_Lighting ("Light and Shadow", Float) = 0

        // Subsurface Scattering
        [HideInInspector] m_start_subsurface ("Subsurface Scattering", Float) = 0
        [Toggle(_TERRAIN_NORMAL_MAP)]_EnableSSS ("Enable Subsurface Scattering", Float) = 0
        _SSSColor ("Subsurface Color", Color) = (1, 0, 0, 1)
        _SSSThicknessMap ("Thickness Map", 2D) = "black" { }
        [HideInInspector][Vector2]_SSSThicknessMapPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _SSSThicknessMapUV ("UV", Int) = 0
        _SSSThicknessMod ("Thickness mod", Range(-1, 1)) = 0
        _SSSSCale ("Light Strength", Range(0, 1)) = 0.25
        _SSSPower ("Light Spread", Range(1, 100)) = 5
        _SSSDistortion ("Light Distortion", Range(0, 1)) = 1
        [HideInInspector] m_end_subsurface ("Subsurface Scattering", Float) = 0
        
        // Rim Lighting
        [HideInInspector] m_start_rimLightOptions ("Rim Lighting", Float) = 0
        [Toggle(_GLOSSYREFLECTIONS_OFF)]_EnableRimLighting ("Enable Rim Lighting", Float) = 0
        [Enum(vertex, 0, pixel, 1)] _RimLightNormal ("Normal Select", Int) = 1
        [ToggleUI]_RimLightingInvert ("Invert Rim Lighting", Float) = 0
        _RimLightColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimWidth ("Rim Width", Range(0, 1)) = 0.8
        _RimSharpness ("Rim Sharpness", Range(0, 1)) = .25
        _RimStrength ("Rim Emission", Range(0, 20)) = 0
        _RimBrighten ("Rim Color Brighten", Range(0, 3)) = 0
        _RimLightColorBias ("Rim Color Bias", Range(0, 1)) = 0
        _RimTex ("Rim Texture", 2D) = "white" { }
        [HideInInspector][Vector2]_RimTexPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _RimTexUV ("UV", Int) = 0
        _RimMask ("Rim Mask", 2D) = "white" { }
        [HideInInspector][Vector2]_RimMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _RimMaskUV ("UV", Int) = 0
        
        // Rim Noise
        [HideInInspector] m_start_rimWidthNoise ("Width Noise", Float) = 0
        _RimWidthNoiseTexture ("Rim Width Noise", 2D) = "black" { }
        [HideInInspector][Vector2]_RimWidthNoiseTexturePan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _RimWidthNoiseTextureUV ("UV", Int) = 0
        _RimWidthNoiseStrength ("Intensity", Range(0, 1)) = 0.1
        [HideInInspector] m_end_rimWidthNoise ("Width Noise", Float) = 0
        
        // Rim Shadow Mix
        [HideInInspector] m_start_ShadowMix ("Shadow Mix", Float) = 0
        _ShadowMix ("Shadow Mix In", Range(0, 1)) = 0
        _ShadowMixThreshold ("Shadow Mix Threshold", Range(0, 1)) = .5
        _ShadowMixWidthMod ("Shadow Mix Width Mod", Range(0, 10)) = .5
        [HideInInspector] m_end_ShadowMix ("Shadow Mix", Float) = 0
        [HideInInspector] m_end_rimLightOptions ("Rim Lighting", Float) = 0
        
        // Environmental Rim Lighting
        [HideInInspector] m_start_reflectionRim ("Environmental Rim", Float) = 0
        [Toggle(_MAPPING_6_FRAMES_LAYOUT)]_EnableEnvironmentalRim ("Enable Environmental Rim", Float) = 0
        _RimEnviroMask ("Mask", 2D) = "white" { }
        [HideInInspector][Vector2]_RimEnviroMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _RimEnviroMaskUV ("UV", Int) = 0
        _RimEnviroBlur ("Blur", Range(0, 1)) = 0.7
        _RimEnviroWidth ("Rim Width", Range(0, 1)) = 0.45
        _RimEnviroSharpness ("Rim Sharpness", Range(0, 1)) = 0
        _RimEnviroMinBrightness ("Min Brightness Threshold", Range(0, 2)) = 0
        _RimEnviroIntensity ("Intensity", Range(0, 1)) = 1
        [HideInInspector] m_end_reflectionRim ("Environmental Rim", Float) = 0
        
        // Baked Lighting
        [HideInInspector] m_start_bakedLighting ("Baked Lighting", Float) = 0
        _GIEmissionMultiplier ("GI Emission Multiplier", Float) = 1
        [HideInInspector] DSGI ("DSGI", Float) = 0 //add this property for double sided illumination settings to be shown
        [HideInInspector] LightmapFlags ("Lightmap Flags", Float) = 0 //add this property for lightmap flags settings to be shown
        [HideInInspector] m_end_bakedLighting ("Baked Lighting", Float) = 0
        
        // Metallics
        [HideInInspector] m_start_Metallic ("Metallics", Float) = 0
        [Toggle(_METALLICGLOSSMAP)]_EnableMetallic ("Enable Metallics", Float) = 0
        _CubeMap ("Baked CubeMap", Cube) = "" { }
        [ToggleUI]_SampleWorld ("Force Baked Cubemap", Range(0, 1)) = 0
        _MetalReflectionTint ("Reflection Tint", Color) = (1, 1, 1)
        _MetallicTintMap ("Tint Map", 2D) = "white" { }
        [HideInInspector][Vector2]_MetallicTintMapPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _MetallicTintMapUV ("UV", Int) = 0
        _MetallicMask ("Metallic Mask", 2D) = "white" { }
        [HideInInspector][Vector2]_MetallicMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _MetallicMaskUV ("UV", Int) = 0
        _Metallic ("Metallic", Range(0, 1)) = 0
        _SmoothnessMask ("Smoothness Map", 2D) = "white" { }
        [HideInInspector][Vector2]_SmoothnessMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _SmoothnessMaskUV ("UV", Int) = 0
        [ToggleUI]_InvertSmoothness ("Invert Smoothness Map", Range(0, 1)) = 0
        _Smoothness ("Smoothness", Range(0, 1)) = 0
        [HideInInspector] m_end_Metallic ("Metallics", Float) = 0
        
        // Clearcoat
        [HideInInspector] m_start_clearCoat ("Clear Coat", Float) = 0
        [Toggle(_COLORCOLOR_ON)]_EnableClearCoat ("Enable Clear Coat", Float) = 0
        [Enum(Vertex, 0, Pixel, 1)] _ClearCoatNormalToUse ("What Normal?", Int) = 0
        _ClearCoatCubeMap ("Baked CubeMap", Cube) = "" { }
        [ToggleUI]_ClearCoatSampleWorld ("Force Baked Cubemap", Range(0, 1)) = 0
        _ClearCoatTint ("Reflection Tint", Color) = (1, 1, 1)
        _ClearCoatMask ("Mask", 2D) = "white" { }
        [HideInInspector][Vector2]_ClearCoatMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _ClearCoatMaskUV ("UV", Int) = 0
        _ClearCoat ("Clear Coat", Range(0, 1)) = 1
        _ClearCoatSmoothnessMap ("Smoothness Map", 2D) = "white" { }
        [HideInInspector][Vector2]_ClearCoatSmoothnessMapPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _ClearCoatSmoothnessMapUV ("UV", Int) = 0
        [ToggleUI]_ClearCoatInvertSmoothness ("Invert Smoothness Map", Range(0, 1)) = 0
        _ClearCoatSmoothness ("Smoothness", Range(0, 1)) = 0
        [ToggleUI]_ClearCoatForceLighting ("Force Lighting", Float) = 0
        [HideInInspector] m_end_clearCoat ("Clear Coat", Float) = 0
        
        // First Matcap
        [HideInInspector] m_start_matcap ("Matcap / Sphere Textures", Float) = 0
        [Toggle(_COLORADDSUBDIFF_ON)]_MatcapEnable ("Enable Matcap", Float) = 0
        _MatcapColor ("Color", Color) = (1, 1, 1, 1)
        [TextureNoSO]_Matcap ("Matcap", 2D) = "white" { }
        _MatcapBorder ("Border", Range(0, .5)) = 0.43
        _MatcapMask ("Mask", 2D) = "white" { }
        [HideInInspector][Vector2]_MatcapMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _MatcapMaskUV ("UV", Int) = 0
        _MatcapEmissionStrength ("Emission Strength", Range(0, 20)) = 0
        _MatcapIntensity ("Intensity", Range(0, 5)) = 1
        _MatcapLightMask ("Hide in Shadow", Range(0, 1)) = 0
        _MatcapReplace ("Replace With Matcap", Range(0, 1)) = 1
        _MatcapMultiply ("Multiply Matcap", Range(0, 1)) = 0
        _MatcapAdd ("Add Matcap", Range(0, 1)) = 0
        [Enum(Vertex, 0, Pixel, 1)] _MatcapNormal ("Normal to use", Int) = 1
        [HideInInspector] m_end_matcap ("Matcap", Float) = 0
        
        // Second Matcap
        [HideInInspector] m_start_Matcap2 ("Matcap 2", Float) = 0
        [ToggleUI]_Matcap2Enable ("Enable Matcap 2", Float) = 0
        _Matcap2Color ("Color", Color) = (1, 1, 1, 1)
        [TextureNoSO]_Matcap2 ("Matcap", 2D) = "white" { }
        _Matcap2Border ("Border", Range(0, .5)) = 0.43
        _Matcap2Mask ("Mask", 2D) = "white" { }
        [HideInInspector][Vector2]_Matcap2MaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _Matcap2MaskUV ("UV", Int) = 0
        _Matcap2EmissionStrength ("Emission Strength", Range(0, 20)) = 0
        _Matcap2Intensity ("Intensity", Range(0, 5)) = 1
        _Matcap2LightMask ("Hide in Shadow", Range(0, 1)) = 0
        _Matcap2Replace ("Replace With Matcap", Range(0, 1)) = 0
        _Matcap2Multiply ("Multiply Matcap", Range(0, 1)) = 0
        _Matcap2Add ("Add Matcap", Range(0, 1)) = 0
        [Enum(Vertex, 0, Pixel, 1)] _Matcap2Normal ("Normal to use", Int) = 1
        [HideInInspector] m_end_Matcap2 ("Matcap 2", Float) = 0
        
        // Specular
        [HideInInspector] m_start_specular ("Specular Reflections", Float) = 0
        [Toggle(_SPECGLOSSMAP)]_EnableSpecular ("Enable Specular", Float) = 0
        [Enum(Realistic, 1, Toon, 2, Anisotropic, 3, Toon Aniso, 4)] _SpecularType ("Specular Type", Int) = 1
        [Enum(vertex, 0, pixel, 1)] _SpecularNormal ("Normal Select", Int) = 1
        _SpecularAttenuation ("Attenuation Strength", Range(0, 1)) = 1
        _SpecularTint ("Specular Tint", Color) = (1, 1, 1, 1)
        _SpecularMetallic ("Metallic", Range(0, 1)) = 0
        _SpecularSmoothness ("Smoothness--{condition_show:{type:OR,condition1:{type:PROPERTY_BOOL,data:_SpecularType==1},condition2:{type:PROPERTY_BOOL,data:_SpecularType==3}}}", Range(0, 1)) = 1
        [Gradient]_SpecularMap ("Specular Map", 2D) = "white" { }
        [HideInInspector][Vector2]_SpecularMapPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _SpecularMapUV ("UV", Int) = 0
        [ToggleUI]_SpecularInvertSmoothness ("Invert Smoothness", Float) = 0
        _SpecularMask ("Specular Mask", 2D) = "white" { }
        [HideInInspector][Vector2]_SpecularMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _SpecularMaskUV ("UV", Int) = 0
        [Enum(Alpha, 0, Grayscale, 1)] _SmoothnessFrom ("Smoothness From", Int) = 1
        // Anisotropic Specular
        [Enum(Tangent, 0, binormal, 1)] _SpecWhatTangent ("(Bi)Tangent?--{condition_show:{type:OR,condition1:{type:PROPERTY_BOOL,data:_SpecularType==3},condition2:{type:PROPERTY_BOOL,data:_SpecularType==4}}}", Int) = 0
        _AnisoSpec1Alpha ("Spec1 Alpha--{condition_show:{type:PROPERTY_BOOL,data:_SpecularType==3}}", Range(0, 1)) = 1
        _AnisoSpec2Alpha ("Spec2 Alpha--{condition_show:{type:PROPERTY_BOOL,data:_SpecularType==3}}", Range(0, 1)) = 1
        _Spec1Offset ("Spec1 Offset--{condition_show:{type:PROPERTY_BOOL,data:_SpecularType==3}}", Range(-1, 1)) = 0
        _Spec2Smoothness ("Spec2 Smoothness--{condition_show:{type:PROPERTY_BOOL,data:_SpecularType==3}}", Range(0, 1)) = 0
        [ToggleUI]_AnisoUseTangentMap ("Use Directional Map?--{condition_show:{type:OR,condition1:{type:PROPERTY_BOOL,data:_SpecularType==3},condition2:{type:PROPERTY_BOOL,data:_SpecularType==4}}}", Float) = 0
        _AnisoTangentMap ("Anisotropic Directional Map--{reference_properties:[_AnisoTangentMapPan, _AnisoTangentMapUV], condition_show:{type:OR,condition1:{type:PROPERTY_BOOL,data:_SpecularType==3},condition2:{type:PROPERTY_BOOL,data:_SpecularType==4}}}", 2D) = "bump" { }
        [HideInInspector][Vector2]_AnisoTangentMapPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _AnisoTangentMapUV ("UV", Int) = 0
        //toon aniso
        _SpecularToonStart ("Spec Toon Start--{condition_show:{type:PROPERTY_BOOL,data:_SpecularType==4}}", Range(0, 1)) = .95
        _SpecularToonEnd ("Spec Toon End--{condition_show:{type:PROPERTY_BOOL,data:_SpecularType==4}}", Range(0, 2)) = 1
        //[ToggleUI]_CenterOutSpecColor ("Center Out SpecMap--{condition_show:{type:PROPERTY_BOOL,data:_SpecularType==4}}", Float) = 0
        [ToggleUI]_SpecularAnisoJitterMirrored ("Mirrored?--{condition_show:{type:PROPERTY_BOOL,data:_SpecularType==4}}", Float) = 0
        [Curve]_SpecularAnisoJitterMicro ("Micro Shift--{reference_properties:[_SpecularAnisoJitterMicroPan, _SpecularAnisoJitterMicroUV], condition_show:{type:OR,condition1:{type:PROPERTY_BOOL,data:_SpecularType==3},condition2:{type:PROPERTY_BOOL,data:_SpecularType==4}}}", 2D) = "black" { }
        _SpecularAnisoJitterMicroMultiplier ("Micro Multiplier--{condition_show:{type:OR,condition1:{type:PROPERTY_BOOL,data:_SpecularType==3},condition2:{type:PROPERTY_BOOL,data:_SpecularType==4}}}", Range(0, 10)) = 0
        [HideInInspector][Vector2]_SpecularAnisoJitterMicroPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _SpecularAnisoJitterMicroUV ("UV", Int) = 0
        [Curve]_SpecularAnisoJitterMacro ("Macro Shift--{reference_properties:[_SpecularAnisoJitterMacroPan, _SpecularAnisoJitterMacroUV], condition_show:{type:OR,condition1:{type:PROPERTY_BOOL,data:_SpecularType==3},condition2:{type:PROPERTY_BOOL,data:_SpecularType==4}}}", 2D) = "black" { }
        _SpecularAnisoJitterMacroMultiplier ("Macro Multiplier--{condition_show:{type:OR,condition1:{type:PROPERTY_BOOL,data:_SpecularType==3},condition2:{type:PROPERTY_BOOL,data:_SpecularType==4}}}", Range(0, 10)) = 0
        [HideInInspector][Vector2]_SpecularAnisoJitterMacroPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _SpecularAnisoJitterMacroUV ("UV", Int) = 0
        // Toon Specular
        [MultiSlider]_SpecularToonInnerOuter ("Inner/Outer Edge--{condition_show:{type:PROPERTY_BOOL,data:_SpecularType==2}}", Vector) = (0.25, 0.3, 0, 1)
        [HideInInspector] m_end_specular ("Specular Reflections", Float) = 0
        
        // Second Specular
        [HideInInspector] m_start_specular1 ("Specular Reflections 2", Float) = 0
        [ToggleUI]_EnableSpecular1 ("Enable Specular", Float) = 0
        [Enum(Realistic, 1, Toon, 2, Anisotropic, 3, Toon Aniso, 4)] _SpecularType1 ("Specular Type", Int) = 1
        [Enum(vertex, 0, pixel, 1)] _SpecularNormal1 ("Normal Select", Int) = 1
        _SpecularAttenuation1 ("Attenuation Strength", Range(0, 1)) = 1
        _SpecularTint1 ("Specular Tint", Color) = (1, 1, 1, 1)
        _SpecularMetallic1 ("Metallic", Range(0, 1)) = 0
        _SpecularSmoothness1 ("Smoothness--{condition_show:{type:OR,condition1:{type:PROPERTY_BOOL,data:_SpecularType1==1},condition2:{type:PROPERTY_BOOL,data:_SpecularType1==3}}}", Range(-2, 1)) = .75
        _SpecularMap1 ("Specular Map", 2D) = "white" { }
        [HideInInspector][Vector2]_SpecularMap1Pan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _SpecularMap1UV ("UV", Int) = 0
        [ToggleUI]_SpecularInvertSmoothness1 ("Invert Smoothness", Float) = 0
        _SpecularMask1 ("Specular Mask", 2D) = "white" { }
        [HideInInspector][Vector2]_SpecularMask1Pan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _SpecularMask1UV ("UV", Int) = 0
        [Enum(Alpha, 0, Grayscale, 1)] _SmoothnessFrom1 ("Smoothness From", Int) = 1
        // Second Anisotropic Specular
        [Enum(Tangent, 0, binormal, 1)] _SpecWhatTangent1 ("(Bi)Tangent?--{condition_show:{type:OR,condition1:{type:PROPERTY_BOOL,data:_SpecularType1==3},condition2:{type:PROPERTY_BOOL,data:_SpecularType1==4}}}", Int) = 0
        _AnisoSpec1Alpha1 ("Spec1 Alpha--{condition_show:{type:PROPERTY_BOOL,data:_SpecularType1==3}}", Range(0, 1)) = 1
        _AnisoSpec2Alpha1 ("Spec2 Alpha--{condition_show:{type:PROPERTY_BOOL,data:_SpecularType1==3}}", Range(0, 1)) = 1
        _Spec1Offset1 ("Spec1 Offset--{condition_show:{type:PROPERTY_BOOL,data:_SpecularType1==3}}", Range(-1, 1)) = 0
        _Spec2Smoothness1 ("Spec2 Smoothness--{condition_show:{type:PROPERTY_BOOL,data:_SpecularType1==3}}", Range(0, 1)) = 0
        [ToggleUI]_AnisoUseTangentMap1 ("Use Directional Map?--{condition_show:{type:OR,condition1:{type:PROPERTY_BOOL,data:_SpecularType1==3},condition2:{type:PROPERTY_BOOL,data:_SpecularType1==4}}}", Float) = 0
        _AnisoTangentMap1 ("Anisotropic Directional Map--{reference_properties:[_AnisoTangentMap1Pan, _AnisoTangentMap1UV], condition_show:{type:OR,condition1:{type:PROPERTY_BOOL,data:_SpecularType1==3},condition2:{type:PROPERTY_BOOL,data:_SpecularType1==4}}}", 2D) = "bump" { }
        [HideInInspector][Vector2]_AnisoTangentMap1Pan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _AnisoTangentMap1UV ("UV", Int) = 0
        // Second toon aniso
        _SpecularToonStart1 ("Spec Toon Start--{condition_show:{type:PROPERTY_BOOL,data:_SpecularType1==4}}", Range(0, 1)) = .95
        _SpecularToonEnd1 ("Spec Toon End--{condition_show:{type:PROPERTY_BOOL,data:_SpecularType1==4}}", Range(0, 2)) = 1
        //[ToggleUI]_CenterOutSpecColor1 ("Center Out SpecMap--{condition_show:{type:PROPERTY_BOOL,data:_SpecularType1==4}}", Float) = 0
        [ToggleUI]_SpecularAnisoJitterMirrored1("Mirrored?--{condition_show:{type:PROPERTY_BOOL,data:_SpecularType1==4}}", Float) = 0
        [Curve]_SpecularAnisoJitterMicro1 ("Micro Shift--{reference_properties:[_SpecularAnisoJitterMicro1Pan, _SpecularAnisoJitterMicro1UV], condition_show:{type:OR,condition1:{type:PROPERTY_BOOL,data:_SpecularType1==3},condition2:{type:PROPERTY_BOOL,data:_SpecularType1==4}}}", 2D) = "black" { }
        _SpecularAnisoJitterMicroMultiplier1 ("Micro Multiplier--{condition_show:{type:OR,condition1:{type:PROPERTY_BOOL,data:_SpecularType1==3},condition2:{type:PROPERTY_BOOL,data:_SpecularType1==4}}}", Range(0, 10)) = 0
        [HideInInspector][Vector2]_SpecularAnisoJitterMicro1Pan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _SpecularAnisoJitterMicro1UV ("UV", Int) = 0
        [Curve]_SpecularAnisoJitterMacro1 ("Macro Shift--{reference_properties:[_SpecularAnisoJitterMacro1Pan, _SpecularAnisoJitterMacro1UV], condition_show:{type:OR,condition1:{type:PROPERTY_BOOL,data:_SpecularType1==3},condition2:{type:PROPERTY_BOOL,data:_SpecularType1==4}}}", 2D) = "black" { }
        _SpecularAnisoJitterMacroMultiplier1 ("Macro Multiplier--{condition_show:{type:OR,condition1:{type:PROPERTY_BOOL,data:_SpecularType1==3},condition2:{type:PROPERTY_BOOL,data:_SpecularType1==4}}}", Range(0, 10)) = 0
        [HideInInspector][Vector2]_SpecularAnisoJitterMacro1Pan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _SpecularAnisoJitterMacro1UV ("UV", Int) = 0
        // Second Toon Specular
        [MultiSlider]_SpecularToonInnerOuter1 ("Inner/Outer Edge--{condition_show:{type:PROPERTY_BOOL,data:_SpecularType1==2}}", Vector) = (0.25, 0.3, 0, 1)
        [HideInInspector] m_end_specular1 ("Specular Reflections", Float) = 0
        
        // Fist Emission
        [HideInInspector] m_Special_Effects ("Special Effects", Float) = 0
        [HideInInspector] m_start_emissionOptions ("Emission / Glow", Float) = 0
        [Toggle(_EMISSION)]_EnableEmission ("Enable Emission", Float) = 0
        [ToggleUI]_EmissionReplace ("Replace Base Color", Float) = 0
        [HDR]_EmissionColor ("Emission Color", Color) = (1, 1, 1, 1)
        [Gradient]_EmissionMap ("Emission Map", 2D) = "white" { }
        [ToggleUI]_EmissionBaseColorAsMap ("Base Color as Map?", Float) = 0
        [HideInInspector][Vector2]_EmissionMapPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _EmissionMapUV ("UV", Int) = 0
        _EmissionMask ("Emission Mask", 2D) = "white" { }
        [HideInInspector][Vector2]_EmissionMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _EmissionMaskUV ("UV", Int) = 0
        _EmissionStrength ("Emission Strength", Range(0, 20)) = 0
        [ToggleUI]_EmissionHueShiftEnabled ("Enable Hue Shift", Float) = 0
        _EmissionHueShift ("Hue Shift", Range(0, 1)) = 0
        
        // Center out emission
        [HideInInspector] m_start_CenterOutEmission ("Center Out Emission", Float) = 0
        [ToggleUI]_EmissionCenterOutEnabled ("Enable Center Out", Float) = 0
        _EmissionCenterOutSpeed ("Flow Speed", Float) = 5
        [HideInInspector] m_end_CenterOutEmission ("inward out emission", Float) = 0
        
        // Glow in the dark Emission
        [HideInInspector] m_start_glowInDarkEmissionOptions ("Glow In The Dark Emission (Requires Lighting Enabled)", Float) = 0
        [ToggleUI]_EnableGITDEmission ("Enable Glow In The Dark", Float) = 0
        [Enum(World, 0, Mesh, 1)] _GITDEWorldOrMesh ("Lighting Type", Int) = 0
        _GITDEMinEmissionMultiplier ("Min Emission Multiplier", Range(0, 1)) = 1
        _GITDEMaxEmissionMultiplier ("Max Emission Multiplier", Range(0, 1)) = 0
        _GITDEMinLight ("Min Lighting", Range(0, 1)) = 0
        _GITDEMaxLight ("Max Lighting", Range(0, 1)) = 1
        [HideInInspector] m_end_glowInDarkEmissionOptions ("Glow In The Dark Emission (Requires Lighting Enabled)", Float) = 0
        
        // Blinking Emission
        [HideInInspector] m_start_blinkingEmissionOptions ("Blinking Emission", Float) = 0
        _EmissiveBlink_Min ("Emissive Blink Min", Float) = 1
        _EmissiveBlink_Max ("Emissive Blink Max", Float) = 1
        _EmissiveBlink_Velocity ("Emissive Blink Velocity", Float) = 4
        _EmissionBlinkingOffset ("Offset", Float) = 0
        [HideInInspector] m_end_blinkingEmissionOptions ("Blinking Emission", Float) = 0
        
        // Scrolling Emission
        [HideInInspector] m_start_scrollingEmissionOptions ("Scrolling Emission", Float) = 0
        [ToggleUI] _ScrollingEmission ("Enable Scrolling Emission", Float) = 0
        [ToggleUI]_EmissionScrollingUseCurve ("Use Curve", float) = 0
        [Curve]_EmissionScrollingCurve ("Curve", 2D) = "white" { }
        _EmissiveScroll_Direction ("Direction", Vector) = (0, -10, 0, 0)
        _EmissiveScroll_Width ("Width", Float) = 10
        _EmissiveScroll_Velocity ("Velocity", Float) = 10
        _EmissiveScroll_Interval ("Interval", Float) = 20
        _EmissionScrollingOffset ("Offset", Float) = 0
        [HideInInspector] m_end_scrollingEmissionOptions ("Scrolling Emission", Float) = 0
        [HideInInspector] m_end_emissionOptions ("Emission / Glow", Float) = 0
        
        // Second Enission
        [HideInInspector] m_start_emission1Options ("Emission / Glow 2 (Requires Emission 1 Enabled)", Float) = 0
        [ToggleUI]_EnableEmission1 ("Enable Emission 2", Float) = 0
        [HDR]_EmissionColor1 ("Emission Color", Color) = (1, 1, 1, 1)
        [Gradient]_EmissionMap1 ("Emission Map", 2D) = "white" { }
        [ToggleUI]_EmissionBaseColorAsMap1 ("Base Color as Map?", Float) = 0
        [HideInInspector][Vector2]_EmissionMap1Pan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _EmissionMap1UV ("UV", Int) = 0
        _EmissionMask1 ("Emission Mask", 2D) = "white" { }
        [HideInInspector][Vector2]_EmissionMask1Pan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _EmissionMask1UV ("UV", Int) = 0
        _EmissionStrength1 ("Emission Strength", Range(0, 20)) = 0
        [ToggleUI]_EmissionHueShiftEnabled1 ("Enable Hue Shift", Float) = 0
        _EmissionHueShift1 ("Hue Shift", Range(0, 1)) = 0
        
        // Second Center Out Enission
        [HideInInspector] m_start_CenterOutEmission1 ("Center Out Emission", Float) = 0
        [ToggleUI]_EmissionCenterOutEnabled1 ("Enable Center Out", Float) = 0
        _EmissionCenterOutSpeed1 ("Flow Speed", Float) = 5
        [HideInInspector] m_end_CenterOutEmission1 ("inward out emission", Float) = 0
        
        // Second Glow In The Dark Emission
        [HideInInspector] m_start_glowInDarkEmissionOptions1 ("Glow In The Dark Emission (Requires Lighting Enabled)", Float) = 0
        [ToggleUI]_EnableGITDEmission1 ("Enable Glow In The Dark", Float) = 0
        [Enum(World, 0, Mesh, 1)] _GITDEWorldOrMesh1 ("Lighting Type", Int) = 0
        _GITDEMinEmissionMultiplier1 ("Min Emission Multiplier", Range(0, 1)) = 1
        _GITDEMaxEmissionMultiplier1 ("Max Emission Multiplier", Range(0, 1)) = 0
        _GITDEMinLight1 ("Min Lighting", Range(0, 1)) = 0
        _GITDEMaxLight1 ("Max Lighting", Range(0, 1)) = 1
        [HideInInspector] m_end_glowInDarkEmissionOptions1 ("Glow In The Dark Emission (Requires Lighting Enabled)", Float) = 0
        
        // Second Blinking Emission
        [HideInInspector] m_start_blinkingEmissionOptions1 ("Blinking Emission", Float) = 0
        _EmissiveBlink_Min1 ("Emissive Blink Min", Float) = 1
        _EmissiveBlink_Max1 ("Emissive Blink Max", Float) = 1
        _EmissiveBlink_Velocity1 ("Emissive Blink Velocity", Float) = 4
        _EmissionBlinkingOffset1 ("Offset", Float) = 0
        [HideInInspector] m_end_blinkingEmissionOptions1 ("Blinking Emission", Float) = 0
        
        // Scrolling Scrolling Emission
        [HideInInspector] m_start_scrollingEmissionOptions1 ("Scrolling Emission", Float) = 0
        [ToggleUI] _ScrollingEmission1 ("Enable Scrolling Emission", Float) = 0
        [ToggleUI]_EmissionScrollingUseCurve1 ("Use Curve", float) = 0
        [Curve]_EmissionScrollingCurve1 ("Curve", 2D) = "white" { }
        _EmissiveScroll_Direction1 ("Direction", Vector) = (0, -10, 0, 0)
        _EmissiveScroll_Width1 ("Width", Float) = 10
        _EmissiveScroll_Velocity1 ("Velocity", Float) = 10
        _EmissiveScroll_Interval1 ("Interval", Float) = 20
        _EmissionScrollingOffset1 ("Offset", Float) = 0
        [HideInInspector] m_end_scrollingEmission1Options ("Scrolling Emission", Float) = 0
        [HideInInspector] m_end_emission1Options ("Emission / Glow 2", Float) = 0
        
        // Flipbook
        [HideInInspector] m_start_flipBook ("Flipbook", Float) = 0
        [Toggle(_SUNDISK_HIGH_QUALITY)]_EnableFlipbook ("Enable Flipbook", Float) = 0
        [ToggleUI]_FlipbookAlphaControlsFinalAlpha ("Flipbook Controls Alpha?", Float) = 0
        [ToggleUI]_FlipbookIntensityControlsAlpha ("Intensity Controls Alpha?", Float) = 0
        [ToggleUI]_FlipbookColorReplaces ("Color Replaces Flipbook", Float) = 0
        [TextureArray]_FlipbookTexArray ("Texture Array", 2DArray) = "" { }
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _FlipbookTexArrayUV ("UV", Int) = 0
        [HideInInspector][Vector2]_FlipbookTexArrayPan ("Panning", Vector) = (0, 0, 0, 0)
        _FlipbookMask ("Mask", 2D) = "white" { }
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _FlipbookMaskUV ("UV", Int) = 0
        [HideInInspector][Vector2]_FlipbookMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        _FlipbookColor ("Color & alpha", Color) = (1, 1, 1, 1)
        _FlipbookTotalFrames ("Total Frames", Int) = 1
        _FlipbookFPS ("FPS", Float) = 30.0
        _FlipbookScaleOffset ("Scale | Offset", Vector) = (1, 1, 0, 0)
        [ToggleUI]_FlipbookTiled ("Tiled?", Float) = 0
        _FlipbookEmissionStrength ("Emission Strength", Range(0, 20)) = 0
        _FlipbookRotation ("Rotation", Range(0, 360)) = 0
        _FlipbookRotationSpeed ("Rotation Speed", Float) = 0
        _FlipbookReplace ("Replace", Range(0, 1)) = 1
        _FlipbookMultiply ("Multiply", Range(0, 1)) = 0
        _FlipbookAdd ("Add", Range(0, 1)) = 0
        
        // Flipbook Manual Control
        [HideInInspector] m_start_manualFlipbookControl ("Manual Control", Float) = 0
        _FlipbookCurrentFrame ("Current Frame", Float) = -1
        [HideInInspector] m_end_manualFlipbookControl ("Manual Control", Float) = 0
        [HideInInspector] m_end_flipBook ("Flipbook", Float) = 0
        
        // Dissolve
        [HideInInspector] m_start_dissolve ("Dissolve", Float) = 0
        [Toggle(DISTORT)]_EnableDissolve ("Enable Dissolve", Float) = 0
        [Enum(Basic, 1, Point2Point, 2)] _DissolveType ("Dissolve Type", Int) = 1
        _DissolveEdgeWidth ("Edge Width", Range(0, .5)) = 0.025
        _DissolveEdgeHardness ("Edge Hardness", Range(0, 1)) = 0.5
        _DissolveEdgeColor ("Edge Color", Color) = (1, 1, 1, 1)
        [Gradient]_DissolveEdgeGradient ("Edge Gradient", 2D) = "white" { }
        _DissolveEdgeEmission ("Edge Emission", Range(0, 20)) = 0
        _DissolveTextureColor ("Dissolved Color", Color) = (1, 1, 1, 1)
        _DissolveToTexture ("Dissolved Texture", 2D) = "white" { }
        [HideInInspector][Vector2]_DissolveToTexturePan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _DissolveToTextureUV ("UV", Int) = 0
        _DissolveToEmissionStrength ("Dissolved Emission Strength", Range(0, 20)) = 0
        _DissolveNoiseTexture ("Dissolve Noise", 2D) = "white" { }
        [HideInInspector][Vector2]_DissolveNoiseTexturePan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _DissolveNoiseTextureUV ("UV", Int) = 0
        [HideInInspector][ToggleUI]_DissolveInvertNoise ("Invert?", Float) = 0
        _DissolveDetailNoise ("Dissolve Detail Noise", 2D) = "black" { }
        [HideInInspector][Vector2]_DissolveDetailNoisePan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _DissolveDetailNoiseUV ("UV", Int) = 0
        [HideInInspector][ToggleUI]_DissolveInvertDetailNoise ("Invert?", Float) = 0
        _DissolveDetailStrength ("Dissolve Detail Strength", Range(0, 1)) = 0.1
        _DissolveAlpha ("Dissolve Alpha", Range(0, 1)) = 0
        _DissolveMask ("Dissolve Mask", 2D) = "white" { }
        [HideInInspector][Vector2]_DissolveMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _DissolveMaskUV ("UV", Int) = 0
        [HideInInspector][ToggleUI]_DissolveMaskInvert ("Invert?", Float) = 0
        _ContinuousDissolve ("Continuous Dissolve Speed", Float) = 0
        [HideInInspector] m_start_dissolveMasking ("Effect Masking", Float) = 0
        [Enum(Undissolved, 0, Dissolved, 1)] _DissolveEmissionSide ("Emission 1", Int) = 0
        [Enum(Undissolved, 0, Dissolved, 1)] _DissolveEmission1Side ("Emission 2", Int) = 0
        [HideInInspector] m_end_dissolveMasking ("Effect Masking", Float) = 0
        
        // Point to Point Dissolve
        [HideInInspector] m_start_pointToPoint ("point to point", Float) = 0
        [Enum(Local, 0, World, 1)] _DissolveP2PWorldLocal ("World/Local", Int) = 0
        _DissolveP2PEdgeLength ("Edge Length", Float) = 0.1
        [Vector3]_DissolveStartPoint ("Start Point", Vector) = (0, -1, 0, 0)
        [Vector3]_DissolveEndPoint ("End Point", Vector) = (0, 1, 0, 0)
        [HideInInspector] m_end_pointToPoint ("Point To Point", Float) = 0
        [HideInInspector] m_end_dissolve ("Dissolve", Float) = 0
        
        // Panosphere
        [HideInInspector] m_start_panosphereOptions ("Panosphere / Cubemaps", Float) = 0
        [Toggle(_DETAIL_MULX2)]_PanoToggle ("Enable Panosphere", Float) = 0
        [ToggleUI]_PanoInfiniteStereoToggle ("Infinite Stereo", Float) = 0
        _PanosphereColor ("Color", Color) = (1, 1, 1, 1)
        _PanosphereTexture ("Texture", 2D) = "white" { }
        _PanoMask ("Mask", 2D) = "white" { }
        [HideInInspector][Vector2]_PanoMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _PanoMaskUV ("UV", Int) = 0
        _PanoEmission ("Emission Strength", Range(0, 10)) = 0
        _PanoBlend ("Alpha", Range(0, 1)) = 0
        [Vector3]_PanospherePan ("Pan Speed", Vector) = (0, 0, 0, 0)
        [ToggleUI]_PanoCubeMapToggle ("Use Cubemap", Float) = 0
        [TextureNoSO]_PanoCubeMap ("CubeMap", Cube) = "" { }
        [HideInInspector] m_end_panosphereOptions ("Panosphere / Cubemaps", Float) = 0
        
        // Glitter
        [HideInInspector] m_start_glitter ("Glitter / Sparkle", Float) = 0
        [Toggle(_SUNDISK_SIMPLE)]_GlitterEnable ("Enable Glitter?", Float) = 0
        //[Enum(Add, 0, Replace, 1)] _GlitterBlendType ("Blend Type", Int) = 0
        [HDR]_GlitterColor ("Color", Color) = (1, 1, 1)
        _GlitterUseSurfaceColor ("Use Surface Color", Range(0, 1)) = 0
        _GlitterColorMap ("Glitter Color Map", 2D) = "white" { }
        [HideInInspector][Vector2]_GlitterColorMapPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _GlitterColorMapUV ("UV", Int) = 0
        [HideInInspector][Vector2]_GlitterPan ("Panning", Vector) = (0, 0, 0, 0)
        _GlitterMask ("Glitter Mask", 2D) = "white" { }
        [HideInInspector][Vector2]_GlitterMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _GlitterMaskUV ("UV", Int) = 0
        _GlitterFrequency ("Glitter Density", Float) = 300.0
        _GlitterJitter ("Glitter Jitter", Range(0, 1)) = 1.0
        _GlitterSpeed ("Glitter Wobble Speed", Float) = 10.0
        _GlitterSize ("Glitter Size", Range(0, 1)) = .3
        _GlitterContrast ("Glitter Contrast", Range(1, 1000)) = 300
        _GlitterAngleRange ("Glitter Angle Range", Range(0, 90)) = 90
        _GlitterMinBrightness ("Glitter Min Brightness", Range(0, 1)) = 0
        _GlitterBrightness ("Glitter Max Brightness", Range(0, 40)) = 3
        _GlitterBias ("Glitter Bias", Range(0, 1)) = .8
        
        // Glitter Random Colors
        [HideInInspector] m_start_glitterRandom ("Random Colors", Float) = 0
        [ToggleUI]_GlitterRandomColors ("Enable", Float) = 0
        [MultiSlider]_GlitterMinMaxSaturation ("Saturation Range", Vector) = (0.8, 1, 0, 1)
        [MultiSlider]_GlitterMinMaxBrightness ("Brightness Range", Vector) = (0.8, 1, 0, 1)
        [HideInInspector] m_end_glitterRandom ("Random Colors", Float) = 0
        [HideInInspector] m_end_glitter ("Glitter / Sparkle", Float) = 0
        
        // MSDF OVERLAY
        [HideInInspector] m_start_Text ("MSDF Text Overlay", Float) = 0
        _TextGlyphs ("Font Array", 2D) = "black" { }
        _TextPixelRange ("Pixel Range", Float) = 4.0
        [Toggle(EFFECT_BUMP)]_TextEnabled ("Text?", Float) = 0
        
        // FPS
        [HideInInspector] m_start_TextFPS ("FPS", Float) = 0
        [ToggleUI]_TextFPSEnabled ("FPS Text?", Float) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _TextFPSUV ("FPS UV", Int) = 0
        _TextFPSColor ("Color", Color) = (1, 1, 1, 1)
        _TextFPSEmissionStrength ("Emission Strength", Range(0, 20)) = 0
        [Vector2]_TextFPSOffset ("Offset", Vector) = (0, 0, 0, 0)
        _TextFPSRotation ("Rotation", Range(0, 360)) = 0
        [Vector2]_TextFPSScale ("Scale", Vector) = (1, 1, 1, 1)
        _TextFPSPadding ("Padding Reduction", Vector) = (0, 0, 0, 0)
        [HideInInspector] m_end_TextFPS ("FPS", Float) = 0
        
        // POSITION
        [HideInInspector] m_start_TextPosition ("Position", Float) = 0
        [ToggleUI]_TextPositionEnabled ("Position Text?", Float) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _TextPositionUV ("Position UV", Int) = 0
        //[ToggleUI]_TextPositionVertical ("Vertical?", Float) = 0
        _TextPositionColor ("Color", Color) = (1, 0, 1, 1)
        _TextPositionEmissionStrength ("Emission Strength", Range(0, 20)) = 0
        [Vector2]_TextPositionOffset ("Offset", Vector) = (0, 0, 0, 0)
        _TextPositionRotation ("Rotation", Range(0, 360)) = 0
        [Vector2]_TextPositionScale ("Scale", Vector) = (1, 1, 1, 1)
        _TextPositionPadding ("Padding Reduction", Vector) = (0, 0, 0, 0)
        [HideInInspector] m_end_TextPosition ("Position", Float) = 0
        
        // INSTANCE TIME
        [HideInInspector] m_start_TextInstanceTime ("Instance Time", Float) = 0
        [ToggleUI]_TextTimeEnabled ("Time Text?", Float) = 0
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _TextTimeUV ("Time UV", Int) = 0
        _TextTimeColor ("Color", Color) = (1, 0, 1, 1)
        _TextTimeEmissionStrength ("Emission Strength", Range(0, 20)) = 0
        [Vector2]_TextTimeOffset ("Offset", Vector) = (0, 0, 0, 0)
        _TextTimeRotation ("Rotation", Range(0, 360)) = 0
        [Vector2]_TextTimeScale ("Scale", Vector) = (1, 1, 1, 1)
        _TextTimePadding ("Padding Reduction", Vector) = (0, 0, 0, 0)
        [HideInInspector] m_end_TextInstanceTime ("Instance Time", Float) = 0
        [HideInInspector] m_end_Text ("MSDF Text Overlay", Float) = 0
        
        // Mirror Rendering
        [HideInInspector] m_start_mirrorOptions ("Mirror", Float) = 0
        [Toggle(_REQUIRE_UV2)]_EnableMirrorOptions ("Enable Mirror Options", Float) = 0
        [Enum(ShowInBoth, 0, ShowOnlyInMirror, 1, DontShowInMirror, 2)] _Mirror ("Show in mirror", Int) = 0
        [ToggleUI]_EnableMirrorTexture ("Enable Mirror Texture", Float) = 0
        _MirrorTexture ("Mirror Tex", 2D) = "white" { }
        [HideInInspector][Vector2]_MirrorTexturePan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _MirrorTextureUV ("UV", Int) = 0
        [HideInInspector] m_end_mirrorOptions ("Mirror", Float) = 0
        
        // Distance Fade
        [HideInInspector] m_start_distanceFade ("Distance Fade", Float) = 0
        _MainMinAlpha ("Minimum Alpha", Range(0, 1)) = 0
        _MainFadeTexture ("Fade Mask", 2D) = "white" { }
        [HideInInspector][Vector2]_MainFadeTexturePan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _MainFadeTextureUV ("UV", Int) = 0
        [Vector2]_MainDistanceFade ("Distance Fade X to Y", Vector) = (0, 0, 0, 0)
        [HideInInspector] m_end_distanceFade ("Distance Fade", Float) = 0
        
        // Angular Fade
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
        
        // UV Distortion
        [HideInInspector] m_start_distortionFlow ("UV Distortion", Float) = 0
        [Toggle(USER_LUT)] _EnableDistortion ("Enabled?", Float) = 0
        _DistortionFlowTexture ("Distortion Texture 1", 2D) = "black" { }
        _DistortionFlowTexture1 ("Distortion Texture 2", 2D) = "black" { }
        _DistortionStrength ("Strength1", Float) = 0.5
        _DistortionStrength1 ("Strength2", Float) = 0.5
        [Vector2]_DistortionSpeed ("Speed1", Vector) = (0.5, 0.5, 0, 0)
        [Vector2]_DistortionSpeed1 ("Speed2", Vector) = (0.5, 0.5, 0, 0)
        [HideInInspector] m_end_distortionFlow ("UV Distortion", Float) = 0
        
        // Parallax Mapping
        [HideInInspector] m_ParallaxMap ("Parallax", Float) = 0
        [Toggle(_PARALLAXMAP)]_ParallaxMap ("Enable Parallax FX", Float) = 0
        [ToggleUI]_ParallaxHeightMapEnabled ("Enable Parallax Height", Float) = 0
        [ToggleUI]_ParallaxInternalMapEnabled ("Enable Parallax Internal", Float) = 0
        [HideInInspector] m_start_parallaxHeightmap ("Heightmap", Float) = 0
        [Vector2]_ParallaxHeightMapPan ("Pan", Vector) = (0, 0, 0, 0)
        [Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] _ParallaxUV ("Parallax UV", Int) = 0
        _ParallaxHeightMap ("Height Map", 2D) = "black" { }
        _ParallaxHeightMapMask ("Mask", 2D) = "white" { }
        [HideInInspector][Vector2]_ParallaxHeightMapMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _ParallaxHeightMapMaskUV ("UV", Int) = 0
        _ParallaxStrength ("Parallax Strength", Range(0, 1)) = 0
        [HideInInspector] m_end_parallaxHeightmap ("Heightmap", Float) = 0
        [HideInInspector] m_start_parallaxInternal ("Internal", Float) = 0
        [Enum(Basic, 0, HeightMap, 1)] _ParallaxInternalHeightmapMode ("Parallax Mode", Int) = 0
        [ToggleUI]_ParallaxInternalHeightFromAlpha ("HeightFromAlpha", Float) = 0
        _ParallaxInternalMap ("Internal Map", 2D) = "black" { }
        _ParallaxInternalMapMask ("Mask", 2D) = "white" { }
        [HideInInspector][Vector2]_ParallaxInternalMapMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _ParallaxInternalMapMaskUV ("UV", Int) = 0
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
        
        // Rendering Options
        [HideInInspector] m_renderingOptions ("Rendering Options", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4
        [Enum(UnityEngine.Rendering.BlendMode)] _SourceBlend ("Source Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DestinationBlend ("Destination Blend", Float) = 10
        [Enum(Off, 0, On, 1)] _ZWrite ("ZWrite", Int) = 0
        _ZBias ("ZBias", Float) = 0.0
        [ToggleUI]_IgnoreFog ("Ignore Fog", Float) = 0
        [HideInInspector] Instancing ("Instancing", Float) = 0 //add this property for instancing variants settings to be shown
        
        // Stencils
        [HideInInspector] m_start_StencilPassOptions ("Stencil", Float) = 0
        [IntRange] _StencilRef ("Stencil Reference Value", Range(0, 255)) = 0
        //[IntRange] _StencilReadMaskRef ("Stencil ReadMask Value", Range(0, 255)) = 0
        //[IntRange] _StencilWriteMaskRef ("Stencil WriteMask Value", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilPassOp ("Stencil Pass Op", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilFailOp ("Stencil Fail Op", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFailOp ("Stencil ZFail Op", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilCompareFunction ("Stencil Compare Function", Float) = 8
        [HideInInspector] m_end_StencilPassOptions ("Stencil", Float) = 0
        
        // Debug Options
        [HideInInspector] m_start_debugOptions ("Debug", Float) = 0
        [HideInInspector][Toggle(_COLOROVERLAY_ON)]_DebugEnabled ("Display Debug Info", Float) = 0
        _VertexUnwrap ("Unwrap", Range(0, 1)) = 0
        [Enum(Off, 0, Vertex Normal, 1, Pixel Normal, 2, Tangent, 3, Binormal, 4)] _DebugMeshData ("Mesh Data", Int) = 0
        [Enum(Off, 0, Attenuation, 1, Direct Lighting, 2, Indirect Lighting, 3, light Map, 4, Ramped Light Map, 5, Final Lighting, 6)] _DebugLightingData ("Lighting Data", Int) = 0
        [Enum(Off, 0, View Dir, 1, Tangent View Dir, 2, Forward Dir, 3, WorldPos, 4, View Dot Normal, 5)] _DebugCameraData ("Camera Data", Int) = 0
        [HideInInspector] m_end_debugOptions ("Debug", Float) = 0
    }
    
    //originalEditorCustomEditor "PoiToon"
    CustomEditor "Thry.ShaderEditor"
    SubShader
    {
        //Blend SrcAlpha OneMinusSrcAlpha
        Blend [_SourceBlend] [_DestinationBlend]
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
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
            // Base Pass Features
            // Decal
            #pragma shader_feature GEOM_TYPE_BRANCH
            #pragma multi_compile _ VERTEXLIGHT_ON
            // UV Distortion
            #pragma shader_feature USER_LUT
            #pragma shader_feature _PARALLAXMAP
            // Mirror
            #pragma shader_feature _REQUIRE_UV2
            // Random
            #pragma shader_feature _SUNDISK_NONE
            // Dissolve
            #pragma shader_feature DISTORT
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
            #include "../Includes/CGI_PoiPass.cginc"
            ENDCG
            
        }
        
        Pass
        {
            Name "ForwardAddPass"
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
            #define TRANSPARENT
            #define FORWARD_ADD_PASS
            // Add Pass Features
            // Decal
            #pragma shader_feature GEOM_TYPE_BRANCH
            // UV Distortion
            #pragma shader_feature USER_LUT
            #pragma shader_feature _PARALLAX_MAP
            // Mirror
            #pragma shader_feature _REQUIRE_UV2
            // Random
            #pragma shader_feature _SUNDISK_NONE
            // Dissolve
            #pragma shader_feature DISTORT
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
            #include "../Includes/CGI_PoiPass.cginc"
            ENDCG
            
        }
        
        Pass
        {
            Name "ShadowCasterPass"
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
            // UV Distortion
            #pragma shader_feature USER_LUT
            // Flipbook
            #pragma shader_feature _SUNDISK_HIGH_QUALITY
            // Mirror
            #pragma shader_feature _REQUIRE_UV2
            // Random
            #pragma shader_feature _SUNDISK_NONE
            // Dissolve
            #pragma shader_feature DISTORT
            #pragma multi_compile_instancing
            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster
            #include "../Includes/CGI_PoiPassShadow.cginc"
            ENDCG
            
        }
        
        Pass
        {
            Tags { "LightMode" = "Meta" }
            Cull Off
            CGPROGRAM
            
            #define POI_META_PASS
            #define TRANSPARENT
            // UV Distortion
            #pragma shader_feature USER_LUT
            #pragma shader_feature _PARALLAXMAP
            // Mirror
            #pragma shader_feature _REQUIRE_UV2
            // Random
            #pragma shader_feature _SUNDISK_NONE
            // Dissolve
            #pragma shader_feature DISTORT
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
            #pragma vertex vert
            #pragma fragment frag
            #include "../Includes/CGI_PoiPass.cginc"
            ENDCG
            
        }
    }
    Fallback "Toon/Lit Cutout (Double)"
}
