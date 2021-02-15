Shader ".poiyomi/• Poiyomi Toon •"
{
    Properties
    {
        [HideInInspector] shader_is_using_thry_editor ("", Float) = 0
        [HideInInspector] shader_master_label ("<color=#000000ff>Poiyomi Toon V7.0.100</color>", Float) = 0
        [HideInInspector] shader_presets ("poiToonPresets", Float) = 0
        [HideInInspector] shader_properties_label_file ("7PlusLabels", Float) = 0
        
        [HideInInspector] footer_youtube ("youtube footer button", Float) = 0
        [HideInInspector] footer_twitter ("twitter footer button", Float) = 0
        [HideInInspector] footer_patreon ("patreon footer button", Float) = 0
        [HideInInspector] footer_discord ("discord footer button", Float) = 0
        [HideInInspector] footer_github ("github footer button", Float) = 0
        
        // Keyword to remind users in the VRChat SDK that this material hasn't been locked.  Inelegant but it works.
        [HideInInspector] _ForgotToLockMaterial (";;YOU_FORGOT_TO_LOCK_THIS_MATERIAL;", Int) = 1
        [ThryShaderOptimizerLockButton] _ShaderOptimizerEnabled ("", Int) = 0
        [Helpbox(3)] _LockTooltip ("ALWAYS LOCK IN BEFORE UPLOADING. || RIGHT CLICK A PROPERTY IF YOU WANT TO ANIMATE IT.", Int) = 0

        [ThryWideEnum(Opaque, 0, Cutout, 1, TransClipping, 9, Fade, 2, Transparent, 3, Additive, 4, Soft Additive, 5, Multiplicative, 6, 2x Multiplicative, 7, Multiplicative Grab Pass, 8)]_Mode("Rendering Preset--{on_value_actions:[ 
            {value:0,actions:[{type:SET_PROPERTY,data:render_queue=2000}, {type:SET_PROPERTY,data:render_type=Opaque},            {type:SET_PROPERTY,data:_BlendOp=0}, {type:SET_PROPERTY,data:_BlendOpAlpha=0}, {type:SET_PROPERTY,data:_Cutoff=0},  {type:SET_PROPERTY,data:_SrcBlend=1}, {type:SET_PROPERTY,data:_DstBlend=0},  {type:SET_PROPERTY,data:_AlphaToMask=0},  {type:SET_PROPERTY,data:_ZWrite=1}, {type:SET_PROPERTY,data:_ZTest=4},   {type:SET_PROPERTY,data:_AlphaPremultiply=0}]},
            {value:1,actions:[{type:SET_PROPERTY,data:render_queue=2450}, {type:SET_PROPERTY,data:render_type=TransparentCutout}, {type:SET_PROPERTY,data:_BlendOp=0}, {type:SET_PROPERTY,data:_BlendOpAlpha=0}, {type:SET_PROPERTY,data:_Cutoff=.5}, {type:SET_PROPERTY,data:_SrcBlend=1}, {type:SET_PROPERTY,data:_DstBlend=0},  {type:SET_PROPERTY,data:_AlphaToMask=1},  {type:SET_PROPERTY,data:_ZWrite=1}, {type:SET_PROPERTY,data:_ZTest=4},   {type:SET_PROPERTY,data:_AlphaPremultiply=0}]},
            {value:9,actions:[{type:SET_PROPERTY,data:render_queue=2451}, {type:SET_PROPERTY,data:render_type=TransparentCutout}, {type:SET_PROPERTY,data:_BlendOp=0}, {type:SET_PROPERTY,data:_BlendOpAlpha=0}, {type:SET_PROPERTY,data:_Cutoff=0},  {type:SET_PROPERTY,data:_SrcBlend=5}, {type:SET_PROPERTY,data:_DstBlend=10}, {type:SET_PROPERTY,data:_AlphaToMask=0},  {type:SET_PROPERTY,data:_ZWrite=1}, {type:SET_PROPERTY,data:_ZTest=4},   {type:SET_PROPERTY,data:_AlphaPremultiply=0}]},
            {value:2,actions:[{type:SET_PROPERTY,data:render_queue=3000}, {type:SET_PROPERTY,data:render_type=Transparent},       {type:SET_PROPERTY,data:_BlendOp=0}, {type:SET_PROPERTY,data:_BlendOpAlpha=0}, {type:SET_PROPERTY,data:_Cutoff=0},  {type:SET_PROPERTY,data:_SrcBlend=5}, {type:SET_PROPERTY,data:_DstBlend=10}, {type:SET_PROPERTY,data:_AlphaToMask=0},  {type:SET_PROPERTY,data:_ZWrite=0}, {type:SET_PROPERTY,data:_ZTest=4},   {type:SET_PROPERTY,data:_AlphaPremultiply=0}]},
            {value:3,actions:[{type:SET_PROPERTY,data:render_queue=3000}, {type:SET_PROPERTY,data:render_type=Transparent},       {type:SET_PROPERTY,data:_BlendOp=0}, {type:SET_PROPERTY,data:_BlendOpAlpha=0}, {type:SET_PROPERTY,data:_Cutoff=0},  {type:SET_PROPERTY,data:_SrcBlend=1}, {type:SET_PROPERTY,data:_DstBlend=10}, {type:SET_PROPERTY,data:_AlphaToMask=0},  {type:SET_PROPERTY,data:_ZWrite=0}, {type:SET_PROPERTY,data:_ZTest=4},   {type:SET_PROPERTY,data:_AlphaPremultiply=1}]},
            {value:4,actions:[{type:SET_PROPERTY,data:render_queue=3000}, {type:SET_PROPERTY,data:render_type=Transparent},       {type:SET_PROPERTY,data:_BlendOp=0}, {type:SET_PROPERTY,data:_BlendOpAlpha=0}, {type:SET_PROPERTY,data:_Cutoff=0},  {type:SET_PROPERTY,data:_SrcBlend=1}, {type:SET_PROPERTY,data:_DstBlend=1},  {type:SET_PROPERTY,data:_AlphaToMask=0},  {type:SET_PROPERTY,data:_ZWrite=0}, {type:SET_PROPERTY,data:_ZTest=4},   {type:SET_PROPERTY,data:_AlphaPremultiply=0}]},
            {value:5,actions:[{type:SET_PROPERTY,data:render_queue=3000}, {type:SET_PROPERTY,data:RenderType=Transparent},        {type:SET_PROPERTY,data:_BlendOp=0}, {type:SET_PROPERTY,data:_BlendOpAlpha=0}, {type:SET_PROPERTY,data:_Cutoff=0},  {type:SET_PROPERTY,data:_SrcBlend=4}, {type:SET_PROPERTY,data:_DstBlend=1},  {type:SET_PROPERTY,data:_AlphaToMask=0},  {type:SET_PROPERTY,data:_ZWrite=0}, {type:SET_PROPERTY,data:_ZTest=4},   {type:SET_PROPERTY,data:_AlphaPremultiply=0}]},
            {value:6,actions:[{type:SET_PROPERTY,data:render_queue=3000}, {type:SET_PROPERTY,data:render_type=Transparent},       {type:SET_PROPERTY,data:_BlendOp=0}, {type:SET_PROPERTY,data:_BlendOpAlpha=0}, {type:SET_PROPERTY,data:_Cutoff=0},  {type:SET_PROPERTY,data:_SrcBlend=2}, {type:SET_PROPERTY,data:_DstBlend=0},  {type:SET_PROPERTY,data:_AlphaToMask=0},  {type:SET_PROPERTY,data:_ZWrite=0}, {type:SET_PROPERTY,data:_ZTest=4},   {type:SET_PROPERTY,data:_AlphaPremultiply=0}]},
            {value:7,actions:[{type:SET_PROPERTY,data:render_queue=3000}, {type:SET_PROPERTY,data:render_type=Transparent},       {type:SET_PROPERTY,data:_BlendOp=0}, {type:SET_PROPERTY,data:_BlendOpAlpha=0}, {type:SET_PROPERTY,data:_Cutoff=0},  {type:SET_PROPERTY,data:_SrcBlend=2}, {type:SET_PROPERTY,data:_DstBlend=3},  {type:SET_PROPERTY,data:_AlphaToMask=0},  {type:SET_PROPERTY,data:_ZWrite=0}, {type:SET_PROPERTY,data:_ZTest=4},   {type:SET_PROPERTY,data:_AlphaPremultiply=0}]}
            }]}]}", Int) = 0
        [HideInInspector] m_LockingInfo ("Locking Info--{button_right:{text:Tutorial,action:{type:URL,data:https://youtu.be/asWeDJb5LAo},hover:YouTube},is_hideable:true}", Float) = 0
        [Helpbox(1)] _HelpBoxLocking ("LOCKING IN THE SHADER WILL DRAMATICALLY INCREASE PERFORMANCE AND DRAMATICALLY LOWER THE FILE SIZE OF AN AVATAR PACKAGE. 
        
        LOCKED IN MATERIALS CANNOT BE ANIMATED WITHOUT SETTING THE SPECIFIC PROPERTY TO DYNAMIC. RIGHT CLICK MATERIAL PROPERTIES YOU WISH TO ANIMATE, A CLOCK ICON WILL APPEAR BESIDE THE PROPERTY SIGNIFYING THAT IT CAN BE ANIMATED. 
        
        FOR MORE INFORMATION ON LOCKING PLEASE WATCH THE LOCKING TUTORIAL IN THE HEADER ABOVE.", Int) = 0
        [Helpbox(2)] _HelpBoxHideLocking ("TO HIDE THIS CATEGORY SELECT CUSTOM UI AT THE TOP AND UNCHECK THE LOCKING INFO CATEGORY", Float) = 0
        // Main
        [HideInInspector] m_mainOptions ("Main", Float) = 0
        _Color ("Color & Alpha", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" { }
        [HideInInspector][Vector2]_MainTexPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _MainTextureUV ("UV", Int) = 0
        _Saturation ("Saturation", Range(-1, 1)) = 0
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
        [HideInInspector][Toggle(COLOR_GRADING_HDR)]_MainHueShiftToggle ("Toggle Hueshift", Float) = 0
        [ToggleUI]_MainHueShiftReplace ("Replace?", Float) = 1
        _MainHueShift ("Hue Shift", Range(0, 1)) = 0
        _MainHueShiftSpeed ("Shift Speed", Float) = 0
        _MainHueShiftMask ("Mask", 2D) = "white" { }
        [HideInInspector][Vector2]_MainHueShiftMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _MainHueShiftMaskUV ("UV", Int) = 0
        [HideInInspector] m_end_MainHueShift ("Hue Shift", Float) = 0
        
        // RGB Masking
        [HideInInspector] m_start_RGBMask ("RGB Color Masking", Float) = 0
        [HideInInspector][Toggle(VIGNETTE)]_RGBMaskEnabled ("RGB Mask Enabled", Float) = 0
        [ToggleUI]_RGBUseVertexColors ("Use Vertex Colors", Float) = 0
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
        // RGB MASKED NORMALS
        [Toggle(GEOM_TYPE_MESH)]_RgbNormalsEnabled ("Enable Normals", Float) = 0
        [ToggleUI]_RGBNormalBlend ("Blend with Base--{condition_show:{type:PROPERTY_BOOL,data:_RgbNormalsEnabled==1}}", Float) = 0
        [Normal]_RgbNormalR ("R Normal--{reference_properties:[_RgbNormalRPan, _RgbNormalRUV],condition_show:{type:PROPERTY_BOOL,data:_RgbNormalsEnabled==1}}", 2D) = "bump" { }
        [HideInInspector]_RgbNormalRPan ("Pan", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)]_RgbNormalRUV ("UV", int) = 0
        _RgbNormalRScale ("Intensity--{condition_show:{type:PROPERTY_BOOL,data:_RgbNormalsEnabled==1}}", Range(0,10)) = 0 
        [Normal]_RgbNormalG ("G Normal--{reference_properties:[_RgbNormalGPan, _RgbNormalGUV],condition_show:{type:PROPERTY_BOOL,data:_RgbNormalsEnabled==1}}", 2D) = "bump" { }
        [HideInInspector]_RgbNormalGPan ("Pan", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)]_RgbNormalGUV ("UV", int) = 0
        _RgbNormalGScale ("Intensity--{condition_show:{type:PROPERTY_BOOL,data:_RgbNormalsEnabled==1}}", Range(0,10)) = 0 
        [Normal]_RgbNormalB ("B Normal--{reference_properties:[_RgbNormalBPan, _RgbNormalBUV],condition_show:{type:PROPERTY_BOOL,data:_RgbNormalsEnabled==1}}", 2D) = "bump" { }
        [HideInInspector]_RgbNormalBPan ("Pan", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)]_RgbNormalBUV ("UV", int) = 0
        _RgbNormalBScale ("Intensity--{condition_show:{type:PROPERTY_BOOL,data:_RgbNormalsEnabled==1}}", Range(0,10)) = 0 
        [HideInInspector] m_end_RGBMask ("RGB Color Masking", Float) = 0
        
        // Detail Options
        [HideInInspector] m_start_DetailOptions ("Details--{reference_property:_DetailEnabled, button_right:{text:Tutorial,action:{type:URL,data:https://www.youtube.com/watch?v=9oIcQln9of4&list=PL4_Gy3VRJSmbXfQSldzUiChgABQsoBlLw},hover:YouTube},is_hideable:true}", Float) = 0
        [HideInInspector][Toggle(FINALPASS)]_DetailEnabled ("Enable", Float) = 0
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
        
        // Vertex Colors
        [HideInInspector] m_start_MainVertexColors ("Vertex Colors", Float) = 0
        _MainVertexColoring ("Use Vertex Color", Range(0, 1)) = 0
        _MainUseVertexColorAlpha ("Use Vertex Color Alpha", Range(0, 1)) = 0
        [HideInInspector] m_end_MainVertexColors ("Vertex Colors", Float) = 0
        
        //Vertex Manipulations
        [HideInInspector] m_start_vertexManipulation ("Vertex Offset--{reference_property:_VertexManipulationsEnabled, button_right:{text:Tutorial,action:{type:URL,data:https://www.youtube.com/watch?v=x728WN50JeA&list=PL4_Gy3VRJSmbXfQSldzUiChgABQsoBlLw},hover:YouTube},is_hideable:true}", Float) = 0
        [HideInInspector][Toggle(AUTO_EXPOSURE)]_VertexManipulationsEnabled ("Enabled", Float) = 0
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
        [HideInInspector] m_end_vertexManipulation ("Vertex Offset", Float) = 0
        
        // Alpha Options
        [HideInInspector] m_start_Alpha ("Alpha Options", Float) = 0
        _Cutoff ("Alpha Cuttoff", Range(0, 1.001)) = 0.5
        [ToggleUI]_DitheringEnabled ("Enable Dithering", Float) = 0
        _DitherGradient ("Dither Gradient", Range(0, 1)) = .1
        [ToggleUI]_ForceOpaque ("Force Opaque", Float) = 0
        _MainShadowClipMod ("Shadow Clip Mod", Range(-1, 1)) = 0
        [Enum(Off, 0, On, 1)] _AlphaToMask ("Alpha To Coverage", Float) = 0
        [ToggleUI]_MainAlphaToCoverage ("Sharpenned A2C--{condition_show:{type:PROPERTY_BOOL,data:_AlphaToMask==1}}", Float) = 0
        _MainMipScale ("Mip Level Alpha Scale--{condition_show:{type:PROPERTY_BOOL,data:_AlphaToMask==1}}", Range(0, 1)) = 0.25
        [ToggleUI]_AlphaPremultiply ("Alpha Premultiply", Float) = 0
        _AlphaMod ("Alpha Mod", Range(-1, 1)) = 0.0
        [HideInInspector] m_end_Alpha ("Alpha Options", Float) = 0
        
        // Decal Texture
        [HideInInspector] m_start_DecalSection ("Decal", Float) = 0
        [HideInInspector][Toggle(GEOM_TYPE_BRANCH)]_DecalEnabled ("Enable", Float) = 0
        _DecalColor ("Color", Color) = (1, 1, 1, 1)
        _DecalEmissionStrength ("Emission Strength", Range(0, 20)) = 0
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
        _BackFaceColor ("Color", Color) = (1, 1, 1, 1)
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
        [Toggle(VIGNETTE_MASKED)]_EnableLighting ("Enable Lighting", Float) = 1
        [Enum(Toon, 0, Realistic, 1)] _LightingMode ("Lighting Type", Int) = 0
        _LightingStandardSmoothness ("Smoothness--{condition_show:{type:PROPERTY_BOOL,data:_LightingMode==1}}", Range(0, 1)) = 0
        [ToggleUI]_LightingShadingEnabled ("Enable Shading--{condition_show:{type:PROPERTY_BOOL,data:_LightingMode==0}}", Float) = 0
        [Enum(Ramp Texture, 0, Math Gradient, 1)] _LightingRampType ("Ramp Type--{condition_show:{type:AND,condition1:{type:PROPERTY_BOOL,data:_LightingMode==0},condition2:{type:PROPERTY_BOOL,data:_LightingShadingEnabled==1}}}", Int) = 0
        [Gradient]_ToonRamp ("Lighting Ramp--{texture:{width:512,height:4,filterMode:Bilinear,wrapMode:Clamp},force_texture_options:true,condition_show:{type:AND,condition1:{type:PROPERTY_BOOL,data:_LightingShadingEnabled==1},condition2:{type:AND,condition1:{type:PROPERTY_BOOL,data:_LightingMode==0},condition2:{type:PROPERTY_BOOL,data:_LightingRampType==0}}}}", 2D) = "white" { }
        _LightingShadowMask ("Ramp Mask--{reference_properties:[_LightingShadowMaskPan, _LightingShadowMaskUV],condition_show:{type:AND,condition1:{type:PROPERTY_BOOL,data:_LightingMode==0},condition2:{type:PROPERTY_BOOL,data:_LightingShadingEnabled==1}}}", 2D) = "white" { }
        [HideInInspector][Vector2]_LightingShadowMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _LightingShadowMaskUV ("UV", Int) = 0
        _ShadowOffset ("Ramp Offset--{condition_show:{type:AND,condition1:{type:PROPERTY_BOOL,data:_LightingShadingEnabled==1},condition2:{type:AND,condition1:{type:PROPERTY_BOOL,data:_LightingMode==0},condition2:{type:PROPERTY_BOOL,data:_LightingRampType==0}}}}", Range(-1, 1)) = 0
        _LightingGradientStart ("Gradient Start--{condition_show:{type:AND,condition1:{type:PROPERTY_BOOL,data:_LightingShadingEnabled==1},condition2:{type:AND,condition1:{type:PROPERTY_BOOL,data:_LightingMode==0},condition2:{type:PROPERTY_BOOL,data:_LightingRampType==1}}}}", Range(0, 1)) = 0
        _LightingGradientEnd ("Gradient End--{condition_show:{type:AND,condition1:{type:PROPERTY_BOOL,data:_LightingShadingEnabled==1},condition2:{type:AND,condition1:{type:PROPERTY_BOOL,data:_LightingMode==0},condition2:{type:PROPERTY_BOOL,data:_LightingRampType==1}}}}", Range(0, 1)) = .5
        _LightingShadowColor ("Shadow Tint--{condition_show:{type:AND,condition1:{type:PROPERTY_BOOL,data:_LightingShadingEnabled==1},condition2:{type:AND,condition1:{type:PROPERTY_BOOL,data:_LightingMode==0},condition2:{type:PROPERTY_BOOL,data:_LightingRampType==1}}}}", Color) = (1, 1, 1, 1)
        _ShadowStrength ("Shadow Strength--{condition_show:{type:AND,condition1:{type:PROPERTY_BOOL,data:_LightingMode==0},condition2:{type:PROPERTY_BOOL,data:_LightingShadingEnabled==1}}}", Range(0, 1)) = 1
        _AttenuationMultiplier ("Unity Shadows--{condition_show:{type:OR,condition1:{type:PROPERTY_BOOL,data:_LightingMode==1},condition2:{type:PROPERTY_BOOL,data:_LightingShadingEnabled==1}}}", Range(0, 1)) = 0
        [ToggleUI]_LightingIgnoreAmbientColor ("Ignore Ambient Color--{condition_show:{type:AND,condition1:{type:PROPERTY_BOOL,data:_LightingMode==0},condition2:{type:PROPERTY_BOOL,data:_LightingShadingEnabled==1}}}", Float) = 0
        
        [HideInInspector] m_start_lightingModifiers ("Lighting Modifiers", Float) = 0
        [Enum(Poi Custom, 0, Correct, 1)] _LightingDirectColorMode ("Direct Light Color", Int) = 0
        [ToggleUI]_LightingIndirectColorMode ("Indirect Uses Normals", Float) = 0
        [ToggleUI]_LightingUncapped ("Uncapped Lighting", Float) = 0
        [ToggleUI]_LightingOnlyUnityShadows ("Only Unity Shadows", Float) = 0
        _LightingMonochromatic ("Monochromatic Lighting?", Range(0,1)) = 0
        _LightingMinLightBrightness ("Min Brightness", Range(0, 1)) = 0
        _LightingMinShadowBrightnessRatio ("Shadow:Light min Ratio", Range(0, 1)) = 0
        [HideInInspector] m_end_lightingModifiers ("Lighting Modifiers", Float) = 0

        [HideInInspector] m_start_detailShadows ("Detail Shadows--{reference_property:_LightingDetailShadowsEnabled}", Float) = 0
        [HideInInspector][ToggleUI]_LightingDetailShadowsEnabled ("Enabled Detail Shadows?", Float) = 0
        _LightingDetailShadows ("Detail Shadows--{reference_properties:[_LightingDetailShadowsPan, _LightingDetailShadowsUV]}", 2D) = "white" { }
        [HideInInspector][Vector2]_LightingDetailShadowsPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _LightingDetailShadowsUV ("UV", Int) = 0
        _LightingDetailStrength ("Detail Strength", Range(0, 1)) = 1
        [HideInInspector] m_end_detailShadows ("Detail Shadows", Float) = 0
        
        [HideInInspector] m_start_ambientOcclusion ("Ambient Occlusion--{reference_property:_LightingEnableAO}", Float) = 0
        [HideInInspector][ToggleUI]_LightingEnableAO ("Enable AO", Float) = 0
        _LightingAOTex ("AO Map", 2D) = "white" { }
        [HideInInspector][Vector2]_LightingAOTexPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _LightingAOTexUV ("UV", Int) = 0
        _AOStrength ("AO Strength", Range(0, 1)) = 1
        [HideInInspector] m_end_ambientOcclusion ("Ambient Occlusion", Float) = 0
        
        [HideInInspector] m_start_shadowTexture ("Shadow Texture--{reference_property:_UseShadowTexture, condition_show:{type:AND,condition1:{type:PROPERTY_BOOL,data:_LightingShadingEnabled==1},condition2:{type:AND,condition1:{type:PROPERTY_BOOL,data:_LightingMode==0},condition2:{type:PROPERTY_BOOL,data:_LightingRampType==1}}}}", Float) = 0
        [HideInInspector][ToggleUI]_UseShadowTexture ("EnableShadowTexture--{condition_show:{type:AND,condition1:{type:PROPERTY_BOOL,data:_LightingShadingEnabled==1},condition2:{type:AND,condition1:{type:PROPERTY_BOOL,data:_LightingMode==0},condition2:{type:PROPERTY_BOOL,data:_LightingRampType==1}}}}", Float) = 0
        _LightingShadowTexture ("Shadow Texture--{reference_properties:[_LightingShadowTexturePan, _LightingShadowTextureUV], condition_show:{type:AND,condition1:{type:PROPERTY_BOOL,data:_LightingShadingEnabled==1},condition2:{type:AND,condition1:{type:PROPERTY_BOOL,data:_LightingMode==0},condition2:{type:PROPERTY_BOOL,data:_LightingRampType==1}}}}", 2D) = "white" { }
        [HideInInspector][Vector2]_LightingShadowTexturePan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _LightingShadowTextureUV ("UV", Int) = 0
        [HideInInspector] m_end_shadowTexture ("Shadow Texture", Float) = 0
        
        // HSL Lighting
        [HideInInspector] m_start_lightingHSL ("HSL Lighting--{reference_property:_LightingEnableHSL, condition_show:{type:AND,condition1:{type:PROPERTY_BOOL,data:_LightingMode==0},condition2:{type:PROPERTY_BOOL,data:_LightingShadingEnabled==1}}}", Float) = 0
        [HideInInspector][ToggleUI]_LightingEnableHSL ("Enabled HSL Lighting", Float) = 0
        _LightingHSLIntensity ("Shadow HSL Intensity", Range(0, 1)) = 1
        _LightingShadowHue ("Shadow Hue Change", Range(0, 1)) = 0.5
        _LightingShadowSaturation ("Shadow Saturation Change", Range(0, 1)) = 0.5
        _LightingShadowLightness ("Shadow Lightness Change", Range(0, 1)) = 0.5
        [HideInInspector] m_end_lightingHSL ("HSL Lighting", Float) = 0
        
        // point/spot Light Settings
        [HideInInspector] m_start_lightingAdvanced ("Additive Lighting (Point/Spot)--{reference_property:_commentIfZero_LightingAdditiveEnable,button_right:{text:Tutorial,action:{type:URL,data:https://www.youtube.com/watch?v=at3p5yRRVU0&list=PL4_Gy3VRJSmbXfQSldzUiChgABQsoBlLw&index=12},hover:YouTube}}", Float) = 0
        [HideInInspector][ToggleUI]_commentIfZero_LightingAdditiveEnable ("Enable Additive", Float) = 1
        [Enum(Realistic, 0, Toon, 1)] _LightingAdditiveType ("Lighting Type", Int) = 1
        _LightingAdditiveGradientStart ("Gradient Start", Range(0, 1)) = 0
        _LightingAdditiveGradientEnd ("Gradient End", Range(0, 1)) = .5
        _LightingAdditivePassthrough ("Point Light Passthrough", Range(0, 1)) = .5
        _LightingAdditiveDetailStrength ("Detail Shadow Strength", Range(0, 1)) = 1
        [ToggleUI]_LightingAdditiveLimitIntensity ("Limit Intensity", Float) = 0
        _LightingAdditiveMaxIntensity ("Max Intensity--{condition_show:{type:PROPERTY_BOOL,data:_LightingAdditiveLimitIntensity==1}}", Range(0, 3)) = 1
        [Toggle(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)]_DisableDirectionalInAdd ("No Directional", Float) = 1
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
        _MatcapMask ("Mask--{reference_properties:[_MatcapMaskPan, _MatcapMaskUV, _MatcapMaskInvert]}", 2D) = "white" { }
        [HideInInspector][Vector2]_MatcapMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _MatcapMaskUV ("UV", Int) = 0
        [HideInInspector][ToggleUI]_MatcapMaskInvert("Invert", Float) = 0 
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
        _Matcap2Mask ("Mask--{reference_properties:[_Matcap2MaskPan, _Matcap2MaskUV, _Matcap2MaskInvert]}", 2D) = "white" { }
        [HideInInspector][Vector2]_Matcap2MaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _Matcap2MaskUV ("UV", Int) = 0
        [HideInInspector][ToggleUI]_Matcap2MaskInvert("Invert", Float) = 0 
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
        _SpecularTint ("Specular Tint", Color) = (1, 1, 1, 1)
        _SpecularMetallic ("Metallic", Range(0, 1)) = 0
        [Gradient]_SpecularMetallicMap ("Metallic Map--{reference_properties:[_SpecularMetallicMapPan, _SpecularMetallicMapUV]}", 2D) = "white" { }
        [HideInInspector][Vector2]_SpecularMetallicMapPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _SpecularMetallicMapUV ("UV", Int) = 0
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
        _SpecularTint1 ("Specular Tint", Color) = (1, 1, 1, 1)
        _SpecularMetallic1 ("Metallic", Range(0, 1)) = 0
        [Gradient]_SpecularMetallicMap1 ("Metallic Map--{reference_properties:[_SpecularMetallicMapPan, _SpecularMetallicMapUV]}", 2D) = "white" { }
        [HideInInspector][Vector2]_SpecularMetallicMap1Pan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _SpecularMetallicMap1UV ("UV", Int) = 0
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
        [ToggleUI]_SpecularAnisoJitterMirrored1 ("Mirrored?--{condition_show:{type:PROPERTY_BOOL,data:_SpecularType1==4}}", Float) = 0
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
        
        // First Emission
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
        [HideInInspector][ToggleUI]_EmissionCenterOutEnabled ("Enable Center Out", Float) = 0
        _EmissionCenterOutSpeed ("Flow Speed", Float) = 5
        [HideInInspector] m_end_CenterOutEmission ("inward out emission", Float) = 0
        
        // Glow in the dark Emission
        [HideInInspector] m_start_glowInDarkEmissionOptions ("Glow In The Dark Emission (Requires Lighting Enabled)", Float) = 0
        [HideInInspector][ToggleUI]_EnableGITDEmission ("Enable Glow In The Dark", Float) = 0
        [Enum(World, 0, Mesh, 1)] _GITDEWorldOrMesh ("Lighting Type", Int) = 0
        _GITDEMinEmissionMultiplier ("Min Emission Multiplier", Range(0, 1)) = 1
        _GITDEMaxEmissionMultiplier ("Max Emission Multiplier", Range(0, 1)) = 0
        _GITDEMinLight ("Min Lighting", Range(0, 1)) = 0
        _GITDEMaxLight ("Max Lighting", Range(0, 1)) = 1
        [HideInInspector] m_end_glowInDarkEmissionOptions ("Glow In The Dark Emission (Requires Lighting Enabled)", Float) = 0
        
        // Blinking Emission
        [HideInInspector] m_start_blinkingEmissionOptions ("Blinking Emission", Float) = 0
        [HideInInspector][ToggleUI]_EmissionBlinkingEnabled ("Enable Blinking", Float) = 0
        _EmissiveBlink_Min ("Emissive Blink Min", Float) = 0
        _EmissiveBlink_Max ("Emissive Blink Max", Float) = 1
        _EmissiveBlink_Velocity ("Emissive Blink Velocity", Float) = 4
        _EmissionBlinkingOffset ("Offset", Float) = 0
        [HideInInspector] m_end_blinkingEmissionOptions ("Blinking Emission", Float) = 0
        
        // Scrolling Emission
        [HideInInspector] m_start_scrollingEmissionOptions ("Scrolling Emission", Float) = 0
        [HideInInspector][ToggleUI] _ScrollingEmission ("Enable Scrolling Emission", Float) = 0
        [ToggleUI]_EmissionScrollingUseCurve ("Use Curve", float) = 0
        [Curve]_EmissionScrollingCurve ("Curve", 2D) = "white" { }
        [ToggleUI]_EmissionScrollingVertexColor ("VColor as position", float) = 0
        _EmissiveScroll_Direction ("Direction", Vector) = (0, -10, 0, 0)
        _EmissiveScroll_Width ("Width", Float) = 10
        _EmissiveScroll_Velocity ("Velocity", Float) = 10
        _EmissiveScroll_Interval ("Interval", Float) = 20
        _EmissionScrollingOffset ("Offset", Float) = 0
        [HideInInspector] m_end_scrollingEmissionOptions ("Scrolling Emission", Float) = 0
        [HideInInspector] m_end_emissionOptions ("Emission / Glow", Float) = 0
        
        // Second Enission
        [HideInInspector] m_start_emission1Options ("Emission / Glow 2 (Requires Emission 1 Enabled)", Float) = 0
        [Toggle(EFFECT_HUE_VARIATION)]_EnableEmission1 ("Enable Emission 2", Float) = 0
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
        [HideInInspector][ToggleUI]_EmissionCenterOutEnabled1 ("Enable Center Out", Float) = 0
        _EmissionCenterOutSpeed1 ("Flow Speed", Float) = 5
        [HideInInspector] m_end_CenterOutEmission1 ("inward out emission", Float) = 0
        
        // Second Glow In The Dark Emission
        [HideInInspector] m_start_glowInDarkEmissionOptions1 ("Glow In The Dark Emission (Requires Lighting Enabled)", Float) = 0
        [HideInInspector][ToggleUI]_EnableGITDEmission1 ("Enable Glow In The Dark", Float) = 0
        [Enum(World, 0, Mesh, 1)] _GITDEWorldOrMesh1 ("Lighting Type", Int) = 0
        _GITDEMinEmissionMultiplier1 ("Min Emission Multiplier", Range(0, 1)) = 1
        _GITDEMaxEmissionMultiplier1 ("Max Emission Multiplier", Range(0, 1)) = 0
        _GITDEMinLight1 ("Min Lighting", Range(0, 1)) = 0
        _GITDEMaxLight1 ("Max Lighting", Range(0, 1)) = 1
        [HideInInspector] m_end_glowInDarkEmissionOptions1 ("Glow In The Dark Emission (Requires Lighting Enabled)", Float) = 0
        
        // Second Blinking Emission
        [HideInInspector] m_start_blinkingEmissionOptions1 ("Blinking Emission", Float) = 0
        [HideInInspector][ToggleUI]_EmissionBlinkingEnabled1 ("Enable Blinking", Float) = 0
        _EmissiveBlink_Min1 ("Emissive Blink Min", Float) = 0
        _EmissiveBlink_Max1 ("Emissive Blink Max", Float) = 1
        _EmissiveBlink_Velocity1 ("Emissive Blink Velocity", Float) = 4
        _EmissionBlinkingOffset1 ("Offset", Float) = 0
        [HideInInspector] m_end_blinkingEmissionOptions1 ("Blinking Emission", Float) = 0
        
        // Scrolling Scrolling Emission
        [HideInInspector] m_start_scrollingEmissionOptions1 ("Scrolling Emission", Float) = 0
        [HideInInspector][ToggleUI] _ScrollingEmission1 ("Enable Scrolling Emission", Float) = 0
        [ToggleUI]_EmissionScrollingUseCurve1 ("Use Curve", float) = 0
        [Curve]_EmissionScrollingCurve1 ("Curve", 2D) = "white" { }
        [ToggleUI]_EmissionScrollingVertexColor1 ("VColor as position", float) = 0
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
        [ToggleUI]_DissolveUseVertexColors ("VertexColor.g Mask", Float) = 0
        [HideInInspector][Vector2]_DissolveMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _DissolveMaskUV ("UV", Int) = 0
        [HideInInspector][ToggleUI]_DissolveMaskInvert ("Invert?", Float) = 0
        _ContinuousDissolve ("Continuous Dissolve Speed", Float) = 0
        [HideInInspector] m_start_dissolveMasking ("Effect Masking", Float) = 0
        [Enum(Undissolved, 0, Dissolved, 1, Both, 2)] _DissolveEmissionSide ("Emission 1", Int) = 2
        [Enum(Undissolved, 0, Dissolved, 1, Both, 2)] _DissolveEmission1Side ("Emission 2", Int) = 2
        [HideInInspector] m_end_dissolveMasking ("Effect Masking", Float) = 0
        
        // Point to Point Dissolve
        [HideInInspector] m_start_pointToPoint ("point to point", Float) = 0
        [Enum(Local, 0, World, 1)] _DissolveP2PWorldLocal ("World/Local", Int) = 0
        _DissolveP2PEdgeLength ("Edge Length", Float) = 0.1
        [Vector3]_DissolveStartPoint ("Start Point", Vector) = (0, -1, 0, 0)
        [Vector3]_DissolveEndPoint ("End Point", Vector) = (0, 1, 0, 0)
        [HideInInspector] m_end_pointToPoint ("Point To Point", Float) = 0

        // Locked in anim sldiers
        [HideInInspector] m_start_BonusSliders ("Locked In Anim Sliders", Float) = 0
        _DissolveAlpha0 ("Dissolve Alpha 0", Range(-1, 1)) = 0
        _DissolveAlpha1 ("Dissolve Alpha 1", Range(-1, 1)) = 0
        _DissolveAlpha2 ("Dissolve Alpha 2", Range(-1, 1)) = 0
        _DissolveAlpha3 ("Dissolve Alpha 3", Range(-1, 1)) = 0
        _DissolveAlpha4 ("Dissolve Alpha 4", Range(-1, 1)) = 0
        _DissolveAlpha5 ("Dissolve Alpha 5", Range(-1, 1)) = 0
        _DissolveAlpha6 ("Dissolve Alpha 6", Range(-1, 1)) = 0
        _DissolveAlpha7 ("Dissolve Alpha 7", Range(-1, 1)) = 0
        _DissolveAlpha8 ("Dissolve Alpha 8", Range(-1, 1)) = 0
        _DissolveAlpha9 ("Dissolve Alpha 9", Range(-1, 1)) = 0
        [HideInInspector] m_end_BonusSliders ("Locked In Sliders", Float) = 0
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
        [Enum(Angle, 0, Linear Emission, 1)]_GlitterMode ("Mode", Int) = 0
        [Enum(Circle, 0, Square, 1)]_GlitterShape ("Shape", Int) = 0
        [Enum(Add, 0, Replace, 1)] _GlitterBlendType ("Blend Mode", Int) = 0
        [HDR]_GlitterColor ("Color", Color) = (1, 1, 1)
        _GlitterUseSurfaceColor ("Use Surface Color", Range(0, 1)) = 0
        _GlitterColorMap ("Glitter Color Map", 2D) = "white" { }
        [HideInInspector][Vector2]_GlitterColorMapPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _GlitterColorMapUV ("UV", Int) = 0
        [HideInInspector][Vector2]_GlitterPan ("Panning", Vector) = (0, 0, 0, 0)
        _GlitterMask ("Glitter Mask", 2D) = "white" { }
        [HideInInspector][Vector2]_GlitterMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _GlitterMaskUV ("UV", Int) = 0
        _GlitterTexture ("Glitter Texture--{reference_properties:[_GlitterTexturePan]}", 2D) = "white" { }
        [HideInInspector][Vector2]_GlitterTexturePan ("Panning", Vector) = (0, 0, 0, 0)
        _GlitterTextureRotation ("Rotation Speed", Float) = 0
        _GlitterFrequency ("Glitter Density", Float) = 300.0
        _GlitterJitter ("Glitter Jitter", Range(0, 1)) = 1.0
        _GlitterSpeed ("Glitter Speed", Float) = 10.0
        _GlitterSize ("Glitter Size", Range(0, 1)) = .3
        _GlitterContrast ("Glitter Contrast--{condition_show:{type:PROPERTY_BOOL,data:_GlitterMode==0}}", Range(1, 1000)) = 300
        _GlitterAngleRange ("Glitter Angle Range--{condition_show:{type:PROPERTY_BOOL,data:_GlitterMode==0}}", Range(0, 90)) = 90
        _GlitterMinBrightness ("Glitter Min Brightness", Range(0, 1)) = 0
        _GlitterBrightness ("Glitter Max Brightness", Range(0, 40)) = 3
        _GlitterBias ("Glitter Bias--{condition_show:{type:PROPERTY_BOOL,data:_GlitterMode==0}}", Range(0, 1)) = .8
        _GlitterCenterSize ("dim light--{condition_show:{type:AND,condition1:{type:PROPERTY_BOOL,data:_GlitterMode==1},condition2:{type:PROPERTY_BOOL,data:_GlitterShape==1}}}", Range(0, 1)) = .08
        _glitterFrequencyLinearEmissive ("Frequency--{condition_show:{type:PROPERTY_BOOL,data:_GlitterMode==1}}", Range(0, 100)) = 20
        _GlitterJaggyFix ("Jaggy Fix--{condition_show:{type:PROPERTY_BOOL,data:_GlitterShape==1}}", Range(0, .1)) = .0
        
        // Glitter Random Colors
        [HideInInspector] m_start_glitterRandom ("Random Things", Float) = 0
        [ToggleUI]_GlitterRandomColors ("Random Colors", Float) = 0
        [MultiSlider]_GlitterMinMaxSaturation ("Saturation Range", Vector) = (0.8, 1, 0, 1)
        [MultiSlider]_GlitterMinMaxBrightness ("Brightness Range", Vector) = (0.8, 1, 0, 1)
        [ToggleUI]_GlitterRandomSize("Random Size?", Float) = 0
        [MultiSlider]_GlitterMinMaxSize ("Size Range", Vector) = (0.1, 0.5, 0, 1)
        [ToggleUI]_GlitterRandomRotation("Random Tex Rotation", Float) = 0
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
        _DistortionMask ("Mask--{reference_properties:[_DistortionMaskPan, _DistortionMaskUV]}", 2D) = "white" { }
        [HideInInspector][Vector2]_DistortionMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] _DistortionMaskUV ("UV", Int) = 0
        _DistortionFlowTexture ("Distortion Texture 1", 2D) = "black" { }
        _DistortionFlowTexture1 ("Distortion Texture 2", 2D) = "black" { }
        _DistortionStrength ("Strength1", Float) = 0.5
        _DistortionStrength1 ("Strength2", Float) = 0.5
        [Vector2]_DistortionSpeed ("Speed1", Vector) = (0.5, 0.5, 0, 0)
        [Vector2]_DistortionSpeed1 ("Speed2", Vector) = (0.5, 0.5, 0, 0)
        [HideInInspector] m_end_distortionFlow ("UV Distortion", Float) = 0
        
        // Outline Options
        [HideInInspector] m_outlineOptions ("Outlines", Float) = 0
        [ToggleUI]_commentIfZero_EnableOutlinePass ("Enable Outlines", float) = 0
        [Enum(Basic, 0, Tint, 1, Rim Light, 2, Directional, 3, DropShadow, 4)]_OutlineMode ("Mode", Int) = 0
        _OutlineTintMix ("Tint Mix--{condition_show:{type:PROPERTY_BOOL,data:_OutlineMode==1}}", Range(0, 1)) = 0
        _OutlineRimLightBlend ("Rim Light Blend--{condition_show:{type:PROPERTY_BOOL,data:_OutlineMode==2}}", Range(0, 1)) = 0
        _OutlinePersonaDirection ("directional Offset XY--{condition_show:{type:PROPERTY_BOOL,data:_OutlineMode==3}}", Vector) = (1, 0, 0, 0)
        _OutlineDropShadowOffset ("Drop Direction XY--{condition_show:{type:PROPERTY_BOOL,data:_OutlineMode==4}}", Vector) = (1, 0, 0, 0)
        [ToggleUI]_OutlineFixedSize ("Fixed Size?", Float) = 0
        [Enum(Off, 0, Normals, 1, Mask VC.r, 2)]_OutlineUseVertexColors ("Vertex Color", Float) = 0
        [ToggleUI]_OutlineLit ("Enable Lighting", Float) = 1
        _LineWidth ("Width", Float) = 0
        _LineColor ("Color", Color) = (1, 1, 1, 1)
        _OutlineEmission ("Outline Emission", Float) = 0
        _OutlineTexture ("Outline Texture--{reference_properties:[_OutlineTexturePan, _OutlineTextureUV]}", 2D) = "white" { }
        [HideInInspector][Vector2]_OutlineTexturePan ("Outline Texture Pan", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _OutlineTextureUV ("UV", Int) = 0
        _OutlineMask ("Outline Mask--{reference_properties:[_OutlineMaskPan, _OutlineMaskUV]}", 2D) = "white" { }
        [HideInInspector][Vector2]_OutlineMaskPan ("Panning", Vector) = (0, 0, 0, 0)
        [HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, DistortedUV1, 4)] _OutlineMaskUV ("UV", Int) = 0
        _OutlineShadowStrength ("Shadow Strength", Range(0, 1)) = 1
        [HideInInspector] m_start_outlineAdvanced ("Advanced", Float) = 0
        [Vector2]_OutlineFadeDistance ("Outline distance Fade", Vector) = (0, 0, 0, 0)
        [Enum(UnityEngine.Rendering.CullMode)] _OutlineCull ("Cull", Float) = 1
        _OutlineOffsetFactor("Offset Factor", Float) = 0
        _OutlineOffsetUnits("Offset Units", Float) = 0
        [HideInInspector] m_end_outlineAdvanced ("Advanced", Float) = 0
        
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
        [Enum(Off, 0, On, 1)] _ZWrite ("ZWrite", Int) = 1
        [Enum(Thry.ColorMask)] _ColorMask ("Color Mask", Int) = 15
        _OffsetFactor ("Offset Factor", Float) = 0.0
        _OffsetUnits ("Offset Units", Float) = 0.0
        [ToggleUI]_IgnoreFog ("Ignore Fog", Float) = 0
        [HideInInspector] Instancing ("Instancing", Float) = 0 //add this property for instancing variants settings to be shown
        
        // Blending Options
        [HideInInspector] m_start_blending ("Blending", Float) = 0
        [Enum(Thry.BlendOp)]_BlendOp ("RGB Blend Op", Int) = 0
        [Enum(Thry.BlendOp)]_BlendOpAlpha ("Alpha Blend Op", Int) = 0
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Source Blend", Int) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Destination Blend", Int) = 0
        [HideInInspector] m_end_blending ("Blending", Float) = 0
        
        // Stencils
        [HideInInspector] m_start_StencilPassOptions ("Stencil", Float) = 0
        [IntRange] _StencilRef ("Stencil Reference Value", Range(0, 255)) = 0
        [IntRange] _StencilReadMask ("Stencil ReadMask Value", Range(0, 255)) = 255
        [IntRange] _StencilWriteMask ("Stencil WriteMask Value", Range(0, 255)) = 255
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilPassOp ("Stencil Pass Op", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilFailOp ("Stencil Fail Op", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFailOp ("Stencil ZFail Op", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilCompareFunction ("Stencil Compare Function", Float) = 8
        [HideInInspector] m_end_StencilPassOptions ("Stencil", Float) = 0
        
        // Outline Stencil
        [HideInInspector] m_start_OutlineStencil ("Outline Stencil--{is_hideable:true, condition_show:{type:PROPERTY_BOOL,data:_commentIfZero_EnableOutlinePass==1}}", Float) = 0
        [IntRange] _OutlineStencilRef ("Stencil Reference Value", Range(0, 255)) = 0
        [IntRange] _OutlineStencilReadMask ("Stencil ReadMask Value", Range(0, 255)) = 255
        [IntRange] _OutlineStencilWriteMask ("Stencil WriteMask Value", Range(0, 255)) = 255
        [Enum(UnityEngine.Rendering.StencilOp)] _OutlineStencilPassOp ("Stencil Pass Op", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _OutlineStencilFailOp ("Stencil Fail Op", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _OutlineStencilZFailOp ("Stencil ZFail Op", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _OutlineStencilCompareFunction ("Stencil Compare Function", Float) = 8
        [HideInInspector] m_end_OutlineStencil ("Outline Stencil", Float) = 0
        
        // Debug Options
        [HideInInspector] m_start_debugOptions ("Debug", Float) = 0
        [HideInInspector][Toggle(_COLOROVERLAY_ON)]_DebugEnabled ("Display Debug Info", Float) = 0
        _VertexUnwrap ("Unwrap", Range(0, 1)) = 0
        [Enum(Off, 0, Vertex Normal, 1, Pixel Normal, 2, Tangent, 3, Binormal, 4, Local 0 Distance, 5)] _DebugMeshData ("Mesh Data", Int) = 0
        [Enum(Off, 0, Attenuation, 1, Direct Lighting, 2, Indirect Lighting, 3, light Map, 4, Ramped Light Map, 5, Final Lighting, 6)] _DebugLightingData ("Lighting Data", Int) = 0
        [Enum(Off, 0, View Dir, 1, Tangent View Dir, 2, Forward Dir, 3, WorldPos, 4, View Dot Normal, 5)] _DebugCameraData ("Camera Data", Int) = 0
        [HideInInspector] m_end_debugOptions ("Debug", Float) = 0
        
        //[HideInInspector] m_animationToggles ("Animation Support Toggles", Float) = 0
        //[HelpBox(1)] _AnimationToggleHelp ("You don't need to search through this list. You can enable animation support on any property by right clicking it", Int) = 0
        
        // Main
        [HideInInspector]_ColorAnimated ("Color & Alpha", Int) = 0
        [HideInInspector]_MainTexAnimated ("Texture", Int) = 0
        [HideInInspector]_MainTex_STAnimated ("Texture Offset/Scale", Int) = 0
        [HideInInspector]_MainTexPanAnimated ("Panning", Int) = 0
        [HideInInspector]_MainTextureUVAnimated ("UV", Int) = 0
        [HideInInspector]_SaturationAnimated ("Saturation", Int) = 0
        [HideInInspector]_MainVertexColoringAnimated ("Use Vertex Color", Int) = 0
        [HideInInspector]_MainUseVertexColorAlphaAnimated ("Use Vertex Alpha", Int) = 0
        [HideInInspector]_MainEmissionStrengthAnimated ("Basic Emission", Int) = 0
        [HideInInspector]_BumpMapAnimated ("Normal Map", Int) = 0
        [HideInInspector]_BumpMapUVAnimated ("UV", Int) = 0
        [HideInInspector]_BumpMapPanAnimated ("Panning", Int) = 0
        [HideInInspector]_BumpScaleAnimated ("Normal Intensity", Int) = 0
        [HideInInspector]_AlphaMaskAnimated ("Alpha Map", Int) = 0
        [HideInInspector]_AlphaMaskPanAnimated ("Panning", Int) = 0
        [HideInInspector]_AlphaMaskUVAnimated ("UV", Int) = 0
        
        //Hue Shifting
        [HideInInspector]_MainHueShiftToggleAnimated ("Toggle Hueshift", Int) = 0
        [HideInInspector]_MainHueShiftReplaceAnimated ("Replace?", Int) = 0
        [HideInInspector]_MainHueShiftAnimated ("Hue Shift", Int) = 0
        [HideInInspector]_MainHueShiftSpeedAnimated ("Shift Speed", Int) = 0
        [HideInInspector]_MainHueShiftMaskAnimated ("Mask", Int) = 0
        [HideInInspector]_MainHueShiftMaskPanAnimated ("Panning", Int) = 0
        [HideInInspector]_MainHueShiftMaskUVAnimated ("UV", Int) = 0
        
        // RGB Masking
        [HideInInspector]_RGBMaskEnabledAnimated ("RGB Mask Enabled", Int) = 0
        [HideInInspector]_RGBUseVertexColorsAnimated ("Use Vertex Colors", Int) = 0
        [HideInInspector]_RGBBlendMultiplicativeAnimated ("Multiplicative?", Int) = 0
        [HideInInspector]_RGBMaskAnimated ("Mask", Int) = 0
        [HideInInspector]_RGBMaskPanningAnimated ("Panning", Int) = 0
        [HideInInspector]_RGBMaskUVAnimated ("UV", Int) = 0
        [HideInInspector]_RedColorAnimated ("R Color", Int) = 0
        [HideInInspector]_RedTexureAnimated ("R Texture", Int) = 0
        [HideInInspector]_RGBRedPanningAnimated ("Panning", Int) = 0
        [HideInInspector]_RGBRed_UVAnimated ("UV", Int) = 0
        [HideInInspector]_GreenColorAnimated ("G Color", Int) = 0
        [HideInInspector]_GreenTextureAnimated ("G Texture", Int) = 0
        [HideInInspector]_RGBGreenPanningAnimated ("Panning", Int) = 0
        [HideInInspector]_RGBGreen_UVAnimated ("UV", Int) = 0
        [HideInInspector]_BlueColorAnimated ("B Color", Int) = 0
        [HideInInspector]_BlueTextureAnimated ("B Texture", Int) = 0
        [HideInInspector]_RGBBluePanningAnimated ("Panning", Int) = 0
        [HideInInspector]_RGBBlue_UVAnimated ("UV", Int) = 0
        [HideInInspector]_RGBNormalBlendAnimated ("UV", Int) = 0
        
        // Detail Options
        [HideInInspector]_DetailMaskAnimated ("Detail Mask (R:Texture, G:Normal)", Int) = 0
        [HideInInspector]_DetailMaskPanAnimated ("Panning", Int) = 0
        [HideInInspector]_DetailMaskUVAnimated ("UV", Int) = 0
        [HideInInspector]_DetailTintAnimated ("Detail Texture Tint", Int) = 0
        [HideInInspector]_DetailTexAnimated ("Detail Texture", Int) = 0
        [HideInInspector]_DetailTexPanAnimated ("Panning", Int) = 0
        [HideInInspector]_DetailTexUVAnimated ("UV", Int) = 0
        [HideInInspector]_DetailTexIntensityAnimated ("Detail Tex Intensity", Int) = 0
        [HideInInspector]_DetailBrightnessAnimated ("Detail Brightness:", Int) = 0
        [HideInInspector]_DetailNormalMapAnimated ("Detail Normal", Int) = 0
        [HideInInspector]_DetailNormalMapScaleAnimated ("Detail Normal Intensity", Int) = 0
        [HideInInspector]_DetailNormalMapPanAnimated ("Panning", Int) = 0
        [HideInInspector]_DetailNormalMapUVAnimated ("UV", Int) = 0
        
        //Vertex Manipulations
        [HideInInspector]_VertexManipulationLocalTranslationAnimated ("Local Translation", Int) = 0
        [HideInInspector]_VertexManipulationLocalRotationAnimated ("Local Rotation", Int) = 0
        [HideInInspector]_VertexManipulationLocalScaleAnimated ("Local Scale", Int) = 0
        [HideInInspector]_VertexManipulationWorldTranslationAnimated ("World Translation", Int) = 0
        [HideInInspector]_VertexManipulationHeightAnimated ("Vertex Height", Int) = 0
        [HideInInspector]_VertexManipulationHeightMaskAnimated ("Height Map", Int) = 0
        [HideInInspector]_VertexManipulationHeightPanAnimated ("Panning", Int) = 0
        [HideInInspector]_VertexManipulationHeightUVAnimated ("UV", Int) = 0
        [HideInInspector]_VertexManipulationHeightBiasAnimated ("Mask Bias", Int) = 0
        [HideInInspector]_VertexRoundingEnabledAnimated ("Rounding Enabled", Int) = 0
        [HideInInspector]_VertexRoundingDivisionAnimated ("Division Amount", Int) = 0
        
        // Alpha Options
        [HideInInspector]_AlphaModAnimated ("Alpha Mod", Int) = 0
        [HideInInspector]_CutoffAnimated ("Alpha Cuttoff", Int) = 0
        [HideInInspector]_DitheringEnabledAnimated ("Enable Dithering", Int) = 0
        [HideInInspector]_DitherGradientAnimated ("Dither Gradient", Int) = 0
        [HideInInspector]_ForceOpaqueAnimated ("Force Opaque", Int) = 0
        [HideInInspector]_MainShadowClipModAnimated ("Shadow Clip Mod", Int) = 0
        [HideInInspector]_AlphaToMaskAnimated ("Alpha To Coverage", Int) = 0
        [HideInInspector]_MainAlphaToCoverageAnimated ("Sharpenned A2C", Int) = 0
        [HideInInspector]_AlphaPremultiplyAnimated ("Alpha Premultiply", Int) = 0
        [HideInInspector]_MainMipScaleAnimated ("Mip Level Alpha Scale", Int) = 0
        
        // Decal Texture
        [HideInInspector]_DecalEnabledAnimated ("Enable", Int) = 0
        [HideInInspector]_DecalColorAnimated ("Color", Int) = 0
        [HideInInspector]_DecalEmissionStrengthAnimated ("Color", Int) = 0
        [HideInInspector]_DecalTextureAnimated ("Decal", Int) = 0
        [HideInInspector]_DecalTexturePanAnimated ("Panning", Int) = 0
        [HideInInspector]_DecalTextureUVAnimated ("UV", Int) = 0
        [HideInInspector]_DecalMaskAnimated ("Mask", Int) = 0
        [HideInInspector]_DecalMaskPanAnimated ("Panning", Int) = 0
        [HideInInspector]_DecalMaskUVAnimated ("UV", Int) = 0
        [HideInInspector]_DecalTiledAnimated ("Tiled?", Int) = 0
        [HideInInspector]_DecalScaleAnimated ("Scale", Int) = 0
        [HideInInspector]_DecalPositionAnimated ("Position", Int) = 0
        [HideInInspector]_DecalRotationAnimated ("Rotation", Int) = 0
        [HideInInspector]_DecalRotationSpeedAnimated ("Rotation Speed", Int) = 0
        [HideInInspector]_DecalBlendAddAnimated ("Add", Int) = 0
        [HideInInspector]_DecalBlendMultiplyAnimated ("Multiply", Int) = 0
        [HideInInspector]_DecalBlendReplaceAnimated ("Replace", Int) = 0
        
        // Lighting
        [HideInInspector]_EnableLightingAnimated ("Enable Lighting", Int) = 0
        [HideInInspector]_LightingModeAnimated ("Lighting Type", Int) = 0
        [HideInInspector]_LightingStandardSmoothnessAnimated ("Smoothness", Int) = 0
        [HideInInspector]_LightingShadingEnabledAnimated ("Enable Shading-",Int) = 0
        [HideInInspector]_LightingRampTypeAnimated ("Ramp Type", Int) = 0
        [HideInInspector]_ToonRampAnimated ("Lighting Ramp", Int) = 0
        [HideInInspector]_LightingShadowMaskAnimated ("Ramp Mask", Int) = 0
        [HideInInspector]_LightingShadowMaskPanAnimated ("Panning", Int) = 0
        [HideInInspector]_LightingShadowMaskUVAnimated ("UV", Int) = 0
        [HideInInspector]_ShadowOffsetAnimated ("Ramp Offset", Int) = 0
        [HideInInspector]_LightingGradientStartAnimated ("Gradient Start", Int) = 0
        [HideInInspector]_LightingGradientEndAnimated ("Gradient End", Int) = 0
        [HideInInspector]_LightingShadowColorAnimated ("Shadow Tint", Int) = 0
        [HideInInspector]_ShadowStrengthAnimated ("Shadow Strength", Int) = 0
        [HideInInspector]_AttenuationMultiplierAnimated ("Unity Shadows", Int) = 0
        [HideInInspector]_LightingIgnoreAmbientColorAnimated ("Ignore Ambient Color", Int) = 0

        [HideInInspector]_LightingDirectColorModeAnimated ("D Color", Int) = 0
        [HideInInspector]_LightingIndirectColorModeAnimated ("I Color?", Int) = 0
        [HideInInspector]_LightingMonochromaticAnimated ("Monochromatic Lighting?", Int) = 0
        [HideInInspector]_LightingUncappedAnimated ("Uncapped Lighting", Int) = 0
        [HideInInspector]_LightingOnlyUnityShadowsAnimated ("Only Unity Shadows", Int) = 0
        [HideInInspector]_LightingMinLightBrightnessAnimated ("Min Brightnes", Int) = 0
        [HideInInspector]_LightingMinShadowBrightnessRatioAnimated ("Shadow:Light min Ratio", Int) = 0

        [HideInInspector]_LightingDetailShadowsEnabledAnimated ("Enabled Detail Shadows?", Int) = 0
        [HideInInspector]_LightingDetailShadowsAnimated ("Detail Shadows", Int) = 0
        [HideInInspector]_LightingDetailShadowsPanAnimated ("Panning", Int) = 0
        [HideInInspector]_LightingDetailShadowsUVAnimated ("UV", Int) = 0
        [HideInInspector]_LightingDetailStrengthAnimated ("Detail Strength", Int) = 0

        [HideInInspector]_LightingEnableAOAnimated ("Enable AO", Int) = 0
        [HideInInspector]_LightingAOTexAnimated ("AO Map", Int) = 0
        [HideInInspector]_LightingAOTexPanAnimated ("Panning", Int) = 0
        [HideInInspector]_LightingAOTexUVAnimated ("UV", Int) = 0
        [HideInInspector]_AOStrengthAnimated ("AO Strength", Range(0, 1)) = 0
        
        [HideInInspector]_UseShadowTextureAnimated ("EnableShadowTexture", Int) = 0
        [HideInInspector]_LightingShadowTextureAnimated ("Shadow Texture", Int) = 0
        [HideInInspector]_LightingShadowTexturePanAnimated ("Panning", Int) = 0
        [HideInInspector]_LightingShadowTextureUVAnimated ("UV", Int) = 0

        [HideInInspector]_LightingEnableHSLAnimated ("Enabled HSL Lighting", Int) = 0
        [HideInInspector]_LightingHSLIntensityAnimated ("Shadow HSL Intensity", Int) = 0
        [HideInInspector]_LightingShadowHueAnimated ("Shadow Hue Change", Int) = 0
        [HideInInspector]_LightingShadowSaturationAnimated ("Shadow Saturation Change", Int) = 0
        [HideInInspector]_LightingShadowLightnessAnimated ("Shadow Lightness Change", Int) = 0
        
        // point/spot Light Settings
        [HideInInspector]_commentIfZero_LightingAdditiveEnableAnimated ("Enable Additive", Int) = 0
        [HideInInspector]_LightingAdditiveTypeAnimated ("Lighting Type", Int) = 0
        [HideInInspector]_LightingAdditiveGradientStartAnimated ("Gradient Start", Int) = 0
        [HideInInspector]_LightingAdditiveGradientEndAnimated ("Gradient End", Int) = 0
        [HideInInspector]_LightingAdditivePassthroughAnimated ("Point Light Passthrough", Int) = 0
        [HideInInspector]_LightingAdditiveDetailStrengthAnimated ("Detail Shadow Strength", Int) = 0
        [HideInInspector]_LightingAdditiveLimitIntensityAnimated ("Limit Intensity", Int) = 0
        [HideInInspector]_LightingAdditiveMaxIntensityAnimated ("Max Intensity", Int) = 0
        
        // Subsurface Scattering
        [HideInInspector]_EnableSSSAnimated ("Enable Subsurface Scattering", Int) = 0
        [HideInInspector]_SSSColorAnimated ("Subsurface Color", Int) = 0
        [HideInInspector]_SSSThicknessMapAnimated ("Thickness Map", Int) = 0
        [HideInInspector]_SSSThicknessMapPanAnimated ("Panning", Int) = 0
        [HideInInspector]_SSSThicknessMapUVAnimated ("UV", Int) = 0
        [HideInInspector]_SSSThicknessModAnimated ("Thickness mod", Int) = 0
        [HideInInspector]_SSSSCaleAnimated ("Light Strength", Int) = 0
        [HideInInspector]_SSSPowerAnimated ("Light Spread", Int) = 0
        [HideInInspector]_SSSDistortionAnimated ("Light Distortion", Int) = 0
        
        // Rim Lighting
        [HideInInspector]_EnableRimLightingAnimated ("Enable Rim Lighting", Int) = 0
        [HideInInspector]_RimLightNormalAnimated ("Normal Select", Int) = 0
        [HideInInspector]_RimLightingInvertAnimated ("Invert Rim Lighting", Int) = 0
        [HideInInspector]_RimLightColorAnimated ("Rim Color", Int) = 0
        [HideInInspector]_RimWidthAnimated ("Rim Width", Int) = 0
        [HideInInspector]_RimSharpnessAnimated ("Rim Sharpness", Int) = 0
        [HideInInspector]_RimStrengthAnimated ("Rim Emission", Int) = 0
        [HideInInspector]_RimBrightenAnimated ("Rim Color Brighten", Int) = 0
        [HideInInspector]_RimLightColorBiasAnimated ("Rim Color Bias", Int) = 0
        [HideInInspector]_RimTexAnimated ("Rim Texture", Int) = 0
        [HideInInspector]_RimTexPanAnimated ("Panning", Int) = 0
        [HideInInspector]_RimTexUVAnimated ("UV", Int) = 0
        [HideInInspector]_RimMaskAnimated ("Rim Mask", Int) = 0
        [HideInInspector]_RimMaskPanAnimated ("Panning", Int) = 0
        [HideInInspector]_RimMaskUVAnimated ("UV", Int) = 0
        
        // Rim Noise
        [HideInInspector]_RimWidthNoiseTextureAnimated ("Rim Width Noise", Int) = 0
        [HideInInspector]_RimWidthNoiseTexturePanAnimated ("Panning", Int) = 0
        [HideInInspector]_RimWidthNoiseTextureUVAnimated ("UV", Int) = 0
        [HideInInspector]_RimWidthNoiseStrengthAnimated ("Intensity", Int) = 0
        
        // Rim Shadow Mix
        [HideInInspector]_ShadowMixAnimated ("Shadow Mix In", Int) = 0
        [HideInInspector]_ShadowMixThresholdAnimated ("Shadow Mix Threshold", Int) = 0
        [HideInInspector]_ShadowMixWidthModAnimated ("Shadow Mix Width Mod", Int) = 0
        
        // Environmental Rim Lighting
        [HideInInspector]_EnableEnvironmentalRimAnimated ("Enable Environmental Rim", Int) = 0
        [HideInInspector]_RimEnviroMaskAnimated ("Mask", Int) = 0
        [HideInInspector]_RimEnviroMaskPanAnimated ("Panning", Int) = 0
        [HideInInspector]_RimEnviroMaskUVAnimated ("UV", Int) = 0
        [HideInInspector]_RimEnviroBlurAnimated ("Blur", Int) = 0
        [HideInInspector]_RimEnviroWidthAnimated ("Rim Width", Int) = 0
        [HideInInspector]_RimEnviroSharpnessAnimated ("Rim Sharpness", Int) = 0
        [HideInInspector]_RimEnviroMinBrightnessAnimated ("Min Brightness Threshold", Int) = 0
        [HideInInspector]_RimEnviroIntensityAnimated ("Intensity", Int) = 0
        
        // Metallics
        [HideInInspector]_EnableMetallicAnimated ("Enable Metallics", Int) = 0
        [HideInInspector]_CubeMapAnimated ("Baked CubeMap", Int) = 0
        [HideInInspector]_SampleWorldAnimated ("Force Baked Cubemap", Int) = 0
        [HideInInspector]_MetalReflectionTintAnimated ("Reflection Tint", Int) = 0
        [HideInInspector]_MetallicTintMapAnimated ("Tint Map", Int) = 0
        [HideInInspector]_MetallicTintMapPanAnimated ("Panning", Int) = 0
        [HideInInspector]_MetallicTintMapUVAnimated ("UV", Int) = 0
        [HideInInspector]_MetallicMaskAnimated ("Metallic Mask", Int) = 0
        [HideInInspector]_MetallicMaskPanAnimated ("Panning", Int) = 0
        [HideInInspector]_MetallicMaskUVAnimated ("UV", Int) = 0
        [HideInInspector]_MetallicAnimated ("Metallic", Int) = 0
        [HideInInspector]_SmoothnessMaskAnimated ("Smoothness Map", Int) = 0
        [HideInInspector]_SmoothnessMaskPanAnimated ("Panning", Int) = 0
        [HideInInspector]_SmoothnessMaskUVAnimated ("UV", Int) = 0
        [HideInInspector]_InvertSmoothnessAnimated ("Invert Smoothness Map", Int) = 0
        [HideInInspector]_SmoothnessAnimated ("Smoothness", Int) = 0
        
        // Clearcoat
        [HideInInspector]_EnableClearCoatAnimated ("Enable Clear Coat", Int) = 0
        [HideInInspector]_ClearCoatNormalToUseAnimated ("What Normal?", Int) = 0
        [HideInInspector]_ClearCoatCubeMapAnimated ("Baked CubeMap", Int) = 0
        [HideInInspector]_ClearCoatSampleWorldAnimated ("Force Baked Cubemap", Int) = 0
        [HideInInspector]_ClearCoatTintAnimated ("Reflection Tint", Int) = 0
        [HideInInspector]_ClearCoatMaskAnimated ("Mask", Int) = 0
        [HideInInspector]_ClearCoatMaskPanAnimated ("Panning", Int) = 0
        [HideInInspector]_ClearCoatMaskUVAnimated ("UV", Int) = 0
        [HideInInspector]_ClearCoatAnimated ("Clear Coat", Int) = 0
        [HideInInspector]_ClearCoatSmoothnessMapAnimated ("Smoothness Map", Int) = 0
        [HideInInspector]_ClearCoatSmoothnessMapPanAnimated ("Panning", Int) = 0
        [HideInInspector]_ClearCoatSmoothnessMapUVAnimated ("UV", Int) = 0
        [HideInInspector]_ClearCoatInvertSmoothnessAnimated ("Invert Smoothness Map", Int) = 0
        [HideInInspector]_ClearCoatSmoothnessAnimated ("Smoothness", Int) = 0
        [HideInInspector]_ClearCoatForceLightingAnimated ("Force Lighting", Int) = 0
        
        // First Matcap
        [HideInInspector]_MatcapEnableAnimated ("Enable Matcap", Int) = 0
        [HideInInspector]_MatcapColorAnimated ("Color", Int) = 0
        [HideInInspector]_MatcapAnimated ("Matcap", Int) = 0
        [HideInInspector]_MatcapBorderAnimated ("Border", Int) = 0
        [HideInInspector]_MatcapMaskAnimated ("Mask", Int) = 0
        [HideInInspector]_MatcapMaskPanAnimated ("Panning", Int) = 0
        [HideInInspector]_MatcapMaskInvertAnimated ("Invert", Int) = 0
        [HideInInspector]_MatcapMaskUVAnimated ("UV", Int) = 0
        [HideInInspector]_MatcapEmissionStrengthAnimated ("Emission Strength", Int) = 0
        [HideInInspector]_MatcapIntensityAnimated ("Intensity", Int) = 0
        [HideInInspector]_MatcapLightMaskAnimated ("Hide in Shadow", Int) = 0
        [HideInInspector]_MatcapReplaceAnimated ("Replace With Matcap", Int) = 0
        [HideInInspector]_MatcapMultiplyAnimated ("Multiply Matcap", Int) = 0
        [HideInInspector]_MatcapAddAnimated ("Add Matcap", Int) = 0
        [HideInInspector]_MatcapNormalAnimated ("Normal to use", Int) = 0
        
        // Second Matcap
        [HideInInspector]_Matcap2EnableAnimated ("Enable Matcap 2", Int) = 0
        [HideInInspector]_Matcap2ColorAnimated ("Color", Int) = 0
        [HideInInspector]_Matcap2Animated ("Matcap", Int) = 0
        [HideInInspector]_Matcap2BorderAnimated ("Border", Int) = 0
        [HideInInspector]_Matcap2MaskAnimated ("Mask", Int) = 0
        [HideInInspector]_Matcap2MaskPanAnimated ("Panning", Int) = 0
        [HideInInspector]_Matcap2MaskUVAnimated ("UV", Int) = 0
        [HideInInspector]_Matcap2MaskInvertAnimated ("Invert", Int) = 0
        [HideInInspector]_Matcap2EmissionStrengthAnimated ("Emission Strength", Int) = 0
        [HideInInspector]_Matcap2IntensityAnimated ("Intensity", Int) = 0
        [HideInInspector]_Matcap2LightMaskAnimated ("Hide in Shadow", Int) = 0
        [HideInInspector]_Matcap2ReplaceAnimated ("Replace With Matcap", Int) = 0
        [HideInInspector]_Matcap2MultiplyAnimated ("Multiply Matcap", Int) = 0
        [HideInInspector]_Matcap2AddAnimated ("Add Matcap", Int) = 0
        [HideInInspector]_Matcap2NormalAnimated ("Normal to use", Int) = 0
        
        // Specular
        [HideInInspector]_EnableSpecularAnimated ("Enable Specular", Int) = 0
        [HideInInspector]_SpecularTypeAnimated ("Specular Type", Int) = 0
        [HideInInspector]_SpecularNormalAnimated ("Normal Select", Int) = 0
        [HideInInspector]_SpecularTintAnimated ("Specular Tint", Int) = 0
        [HideInInspector]_SpecularMetallicAnimated ("Metallic", Int) = 0
        [HideInInspector]_SpecularSmoothnessAnimated ("Smoothness", Int) = 0
        [HideInInspector]_SpecularMapAnimated ("Specular Map", Int) = 0
        [HideInInspector]_SpecularMapPanAnimated ("Panning", Int) = 0
        [HideInInspector]_SpecularMapUVAnimated ("UV", Int) = 0
        [HideInInspector]_SpecularInvertSmoothnessAnimated ("Invert Smoothness", Int) = 0
        [HideInInspector]_SpecularMaskAnimated ("Specular Mask", Int) = 0
        [HideInInspector]_SpecularMaskPanAnimated ("Panning", Int) = 0
        [HideInInspector]_SpecularMaskUVAnimated ("UV", Int) = 0
        [HideInInspector]_SmoothnessFromAnimated ("Smoothness From", Int) = 0
        [HideInInspector]_SpecWhatTangentAnimated ("(Bi)Tangent?", Int) = 0
        [HideInInspector]_AnisoSpec1AlphaAnimated ("Spec1 Alpha", Int) = 0
        [HideInInspector]_AnisoSpec2AlphaAnimated ("Spec2 Alpha", Int) = 0
        [HideInInspector]_Spec1OffsetAnimated ("Spec1 Offset", Int) = 0
        [HideInInspector]_Spec2SmoothnessAnimated ("Spec2 Smoothness", Int) = 0
        [HideInInspector]_AnisoUseTangentMapAnimated ("Use Directional Map?", Int) = 0
        [HideInInspector]_AnisoTangentMapAnimated ("Anisotropic Directional Map", Int) = 0
        [HideInInspector]_AnisoTangentMapPanAnimated ("Panning", Int) = 0
        [HideInInspector]_AnisoTangentMapUVAnimated ("UV", Int) = 0
        [HideInInspector]_SpecularToonStartAnimated ("Spec Toon Start", Int) = 0
        [HideInInspector]_SpecularToonEndAnimated ("Spec Toon End", Int) = 0
        //[ToggleUI]_CenterOutSpecColorAnimated ("Center Out SpecMap", Int) = 0
        [HideInInspector]_SpecularAnisoJitterMirroredAnimated ("Mirrored?", Int) = 0
        [HideInInspector]_SpecularAnisoJitterMicroAnimated ("Micro Shift", Int) = 0
        [HideInInspector]_SpecularAnisoJitterMicroMultiplierAnimated ("Micro Multiplier", Int) = 0
        [HideInInspector]_SpecularAnisoJitterMicroPanAnimated ("Panning", Int) = 0
        [HideInInspector]_SpecularAnisoJitterMicroUVAnimated ("UV", Int) = 0
        [HideInInspector]_SpecularAnisoJitterMacroAnimated ("Macro Shift", Int) = 0
        [HideInInspector]_SpecularAnisoJitterMacroMultiplierAnimated ("Macro Multiplier", Int) = 0
        [HideInInspector]_SpecularAnisoJitterMacroPanAnimated ("Panning", Int) = 0
        [HideInInspector]_SpecularAnisoJitterMacroUVAnimated ("UV", Int) = 0
        [HideInInspector]_SpecularToonInnerOuterAnimated ("Inner/Outer Edge", Int) = 0
        
        // Second Specular
        [HideInInspector]_EnableSpecular1Animated ("Enable Specular", Int) = 0
        [HideInInspector]_SpecularType1Animated ("Specular Type", Int) = 0
        [HideInInspector]_SpecularNormal1Animated ("Normal Select", Int) = 0
        [HideInInspector]_SpecularTint1Animated ("Specular Tint", Int) = 0
        [HideInInspector]_SpecularMetallic1Animated ("Metallic", Int) = 0
        [HideInInspector]_SpecularSmoothness1Animated ("Smoothness", Int) = 0
        [HideInInspector]_SpecularMap1Animated ("Specular Map", Int) = 0
        [HideInInspector]_SpecularMap1PanAnimated ("Panning", Int) = 0
        [HideInInspector]_SpecularMap1UVAnimated ("UV", Int) = 0
        [HideInInspector]_SpecularInvertSmoothness1Animated ("Invert Smoothness", Int) = 0
        [HideInInspector]_SpecularMask1Animated ("Specular Mask", Int) = 0
        [HideInInspector]_SpecularMask1PanAnimated ("Panning", Int) = 0
        [HideInInspector]_SpecularMask1UVAnimated ("UV", Int) = 0
        [HideInInspector]_SmoothnessFrom1Animated ("Smoothness From", Int) = 0
        [HideInInspector]_SpecWhatTangent1Animated ("(Bi)Tangent?", Int) = 0
        [HideInInspector]_AnisoSpec1Alpha1Animated ("Spec1 Alpha", Int) = 0
        [HideInInspector]_AnisoSpec2Alpha1Animated ("Spec2 Alpha", Int) = 0
        [HideInInspector]_Spec1Offset1Animated ("Spec1 Offset", Int) = 0
        [HideInInspector]_Spec2Smoothness1Animated ("Spec2 Smoothness", Int) = 0
        [HideInInspector]_AnisoUseTangentMap1Animated ("Use Directional Map?", Int) = 0
        [HideInInspector]_AnisoTangentMap1Animated ("Anisotropic Directional Map", Int) = 0
        [HideInInspector]_AnisoTangentMap1PanAnimated ("Panning", Int) = 0
        [HideInInspector]_AnisoTangentMap1UVAnimated ("UV", Int) = 0
        [HideInInspector]_SpecularToonStart1Animated ("Spec Toon Start", Int) = 0
        [HideInInspector]_SpecularToonEnd1Animated ("Spec Toon End", Int) = 0
        //[ToggleUI]_CenterOutSpecColor1Animated ("Center Out SpecMap", Int) = 0
        [HideInInspector]_SpecularAnisoJitterMirrored1Animated ("Mirrored?", Int) = 0
        [HideInInspector]_SpecularAnisoJitterMicro1Animated ("Micro Shift", Int) = 0
        [HideInInspector]_SpecularAnisoJitterMicroMultiplier1Animated ("Micro Multiplier", Int) = 0
        [HideInInspector]_SpecularAnisoJitterMicro1PanAnimated ("Panning", Int) = 0
        [HideInInspector]_SpecularAnisoJitterMicro1UVAnimated ("UV", Int) = 0
        [HideInInspector]_SpecularAnisoJitterMacro1Animated ("Macro Shift", Int) = 0
        [HideInInspector]_SpecularAnisoJitterMacroMultiplier1Animated ("Macro Multiplier", Int) = 0
        [HideInInspector]_SpecularAnisoJitterMacro1PanAnimated ("Panning", Int) = 0
        [HideInInspector]_SpecularAnisoJitterMacro1UVAnimated ("UV", Int) = 0
        [HideInInspector]_SpecularToonInnerOuter1Animated ("Inner/Outer Edge", Int) = 0
        
        // First Emission
        [HideInInspector]_EnableEmissionAnimated ("Enable Emission", Int) = 0
        [HideInInspector]_EmissionReplaceAnimated ("Replace Base Color", Int) = 0
        [HideInInspector]_EmissionColorAnimated ("Emission Color", Int) = 0
        [HideInInspector]_EmissionMapAnimated ("Emission Map", Int) = 0
        [HideInInspector]_EmissionBaseColorAsMapAnimated ("Base Color as Map?", Int) = 0
        [HideInInspector]_EmissionMapPanAnimated ("Panning", Int) = 0
        [HideInInspector]_EmissionMapUVAnimated ("UV", Int) = 0
        [HideInInspector]_EmissionMaskAnimated ("Emission Mask", Int) = 0
        [HideInInspector]_EmissionMaskPanAnimated ("Panning", Int) = 0
        [HideInInspector]_EmissionMaskUVAnimated ("UV", Int) = 0
        [HideInInspector]_EmissionStrengthAnimated ("Emission Strength", Int) = 0
        [HideInInspector]_EmissionHueShiftEnabledAnimated ("Enable Hue Shift", Int) = 0
        [HideInInspector]_EmissionHueShiftAnimated ("Hue Shift", Int) = 0
        
        // Center out emission
        [HideInInspector]_EmissionCenterOutEnabledAnimated ("Enable Center Out", Int) = 0
        [HideInInspector]_EmissionCenterOutSpeedAnimated ("Flow Speed", Int) = 0
        
        // Glow in the dark Emission
        [HideInInspector]_EnableGITDEmissionAnimated ("Enable Glow In The Dark", Int) = 0
        [HideInInspector]_GITDEWorldOrMeshAnimated ("Lighting Type", Int) = 0
        [HideInInspector]_GITDEMinEmissionMultiplierAnimated ("Min Emission Multiplier", Int) = 0
        [HideInInspector]_GITDEMaxEmissionMultiplierAnimated ("Max Emission Multiplier", Int) = 0
        [HideInInspector]_GITDEMinLightAnimated ("Min Lighting", Int) = 0
        [HideInInspector]_GITDEMaxLightAnimated ("Max Lighting", Int) = 0
        
        // Blinking Emission
        [HideInInspector]_EmissionBlinkingEnabledAnimated ("Blinking Enabled", Int) = 0
        [HideInInspector]_EmissiveBlink_MinAnimated ("Emissive Blink Min", Int) = 0
        [HideInInspector]_EmissiveBlink_MaxAnimated ("Emissive Blink Max", Int) = 0
        [HideInInspector]_EmissiveBlink_VelocityAnimated ("Emissive Blink Velocity", Int) = 0
        [HideInInspector]_EmissionBlinkingOffsetAnimated ("Offset", Int) = 0
        
        // Scrolling Emission
        [HideInInspector]_ScrollingEmissionAnimated ("Enable Scrolling Emission", Int) = 0
        [HideInInspector]_EmissionScrollingUseCurveAnimated ("Use Curve", Int) = 0
        [HideInInspector]_EmissionScrollingCurveAnimated ("Curve", Int) = 0
        [HideInInspector]_EmissionScrollingVertexColorAnimated ("use vcolor", Int) = 0
        [HideInInspector]_EmissiveScroll_DirectionAnimated ("Direction", Int) = 0
        [HideInInspector]_EmissiveScroll_WidthAnimated ("Width", Int) = 0
        [HideInInspector]_EmissiveScroll_VelocityAnimated ("Velocity", Int) = 0
        [HideInInspector]_EmissiveScroll_IntervalAnimated ("Interval", Int) = 0
        [HideInInspector]_EmissionScrollingOffsetAnimated ("Offset", Int) = 0
        
        // Second Enission
        [HideInInspector]_EnableEmission1Animated ("Enable Emission 2", Int) = 0
        [HideInInspector]_EmissionColor1Animated ("Emission Color", Int) = 0
        [HideInInspector]_EmissionMap1Animated ("Emission Map", Int) = 0
        [HideInInspector]_EmissionBaseColorAsMap1Animated ("Base Color as Map?", Int) = 0
        [HideInInspector]_EmissionMap1PanAnimated ("Panning", Int) = 0
        [HideInInspector]_EmissionMap1UVAnimated ("UV", Int) = 0
        [HideInInspector]_EmissionMask1Animated ("Emission Mask", Int) = 0
        [HideInInspector]_EmissionMask1PanAnimated ("Panning", Int) = 0
        [HideInInspector]_EmissionMask1UVAnimated ("UV", Int) = 0
        [HideInInspector]_EmissionStrength1Animated ("Emission Strength", Int) = 0
        [HideInInspector]_EmissionHueShiftEnabled1Animated ("Enable Hue Shift", Int) = 0
        [HideInInspector]_EmissionHueShift1Animated ("Hue Shift", Int) = 0
        
        // Second Center Out Enission
        [HideInInspector]_EmissionCenterOutEnabled1Animated ("Enable Center Out", Int) = 0
        [HideInInspector]_EmissionCenterOutSpeed1Animated ("Flow Speed", Int) = 0
        
        // Second Glow In The Dark Emission
        [HideInInspector]_EnableGITDEmission1Animated ("Enable Glow In The Dark", Int) = 0
        [HideInInspector]_GITDEWorldOrMesh1Animated ("Lighting Type", Int) = 0
        [HideInInspector]_GITDEMinEmissionMultiplier1Animated ("Min Emission Multiplier", Int) = 0
        [HideInInspector]_GITDEMaxEmissionMultiplier1Animated ("Max Emission Multiplier", Int) = 0
        [HideInInspector]_GITDEMinLight1Animated ("Min Lighting", Int) = 0
        [HideInInspector]_GITDEMaxLight1Animated ("Max Lighting", Int) = 0
        
        // Second Blinking Emission
        [HideInInspector]_EmissionBlinkingEnabledAnimated ("Blinking Enabled", Int) = 0
        [HideInInspector]_EmissiveBlink_Min1Animated ("Emissive Blink Min", Int) = 0
        [HideInInspector]_EmissiveBlink_Max1Animated ("Emissive Blink Max", Int) = 0
        [HideInInspector]_EmissiveBlink_Velocity1Animated ("Emissive Blink Velocity", Int) = 0
        [HideInInspector]_EmissionBlinkingOffset1Animated ("Offset", Int) = 0
        
        // Scrolling Scrolling Emission
        [HideInInspector]_ScrollingEmission1Animated ("Enable Scrolling Emission", Int) = 0
        [HideInInspector]_EmissionScrollingUseCurve1Animated ("Use Curve", Int) = 0
        [HideInInspector]_EmissionScrollingCurve1Animated ("Curve", Int) = 0
        [HideInInspector]_EmissionScrollingVertexColor1Animated ("use vcolor", Int) = 0
        [HideInInspector]_EmissiveScroll_Direction1Animated ("Direction", Int) = 0
        [HideInInspector]_EmissiveScroll_Width1Animated ("Width", Int) = 0
        [HideInInspector]_EmissiveScroll_Velocity1Animated ("Velocity", Int) = 0
        [HideInInspector]_EmissiveScroll_Interval1Animated ("Interval", Int) = 0
        [HideInInspector]_EmissionScrollingOffset1Animated ("Offset", Int) = 0
        
        // Flipbook
        [HideInInspector]_EnableFlipbookAnimated ("Enable Flipbook", Int) = 0
        [HideInInspector]_FlipbookAlphaControlsFinalAlphaAnimated ("Flipbook Controls Alpha?", Int) = 0
        [HideInInspector]_FlipbookIntensityControlsAlphaAnimated ("Intensity Controls Alpha?", Int) = 0
        [HideInInspector]_FlipbookColorReplacesAnimated ("Color Replaces Flipbook", Int) = 0
        [HideInInspector]_FlipbookTexArrayAnimated ("Texture Array", Int) = 0
        [HideInInspector]_FlipbookTexArrayUVAnimated ("UV", Int) = 0
        [HideInInspector]_FlipbookTexArrayPanAnimated ("Panning", Int) = 0
        [HideInInspector]_FlipbookMaskAnimated ("Mask", Int) = 0
        [HideInInspector]_FlipbookMaskUVAnimated ("UV", Int) = 0
        [HideInInspector]_FlipbookMaskPanAnimated ("Panning", Int) = 0
        [HideInInspector]_FlipbookColorAnimated ("Color & alpha", Int) = 0
        [HideInInspector]_FlipbookTotalFramesAnimated ("Total Frames", Int) = 0
        [HideInInspector]_FlipbookFPSAnimated ("FPS", Int) = 0
        [HideInInspector]_FlipbookScaleOffsetAnimated ("Scale | Offset", Int) = 0
        [HideInInspector]_FlipbookTiledAnimated ("Tiled?", Int) = 0
        [HideInInspector]_FlipbookEmissionStrengthAnimated ("Emission Strength", Int) = 0
        [HideInInspector]_FlipbookRotationAnimated ("Rotation", Int) = 0
        [HideInInspector]_FlipbookRotationSpeedAnimated ("Rotation Speed", Int) = 0
        [HideInInspector]_FlipbookReplaceAnimated ("Replace", Int) = 0
        [HideInInspector]_FlipbookMultiplyAnimated ("Multiply", Int) = 0
        [HideInInspector]_FlipbookAddAnimated ("Add", Int) = 0
        [HideInInspector]_FlipbookCurrentFrameAnimated ("Current Frame", Int) = 0
        
        // Dissolve
        [HideInInspector]_EnableDissolveAnimated ("Enable Dissolve", Int) = 0
        [HideInInspector]_DissolveTypeAnimated ("Dissolve Type", Int) = 0
        [HideInInspector]_DissolveEdgeWidthAnimated ("Edge Width", Int) = 0
        [HideInInspector]_DissolveEdgeHardnessAnimated ("Edge Hardness", Int) = 0
        [HideInInspector]_DissolveEdgeColorAnimated ("Edge Color", Int) = 0
        [HideInInspector]_DissolveEdgeGradientAnimated ("Edge Gradient", Int) = 0
        [HideInInspector]_DissolveEdgeEmissionAnimated ("Edge Emission", Int) = 0
        [HideInInspector]_DissolveTextureColorAnimated ("Dissolved Color", Int) = 0
        [HideInInspector]_DissolveToTextureAnimated ("Dissolved Texture", Int) = 0
        [HideInInspector]_DissolveToTexturePanAnimated ("Panning", Int) = 0
        [HideInInspector]_DissolveToTextureUVAnimated ("UV", Int) = 0
        [HideInInspector]_DissolveToEmissionStrengthAnimated ("Dissolved Emission Strength", Int) = 0
        [HideInInspector]_DissolveNoiseTextureAnimated ("Dissolve Noise", Int) = 0
        [HideInInspector]_DissolveNoiseTexturePanAnimated ("Panning", Int) = 0
        [HideInInspector]_DissolveNoiseTextureUVAnimated ("UV", Int) = 0
        [HideInInspector]_DissolveInvertNoiseAnimated ("Invert?", Int) = 0
        [HideInInspector]_DissolveDetailNoiseAnimated ("Dissolve Detail Noise", Int) = 0
        [HideInInspector]_DissolveDetailNoisePanAnimated ("Panning", Int) = 0
        [HideInInspector]_DissolveDetailNoiseUVAnimated ("UV", Int) = 0
        [HideInInspector]_DissolveInvertDetailNoiseAnimated ("Invert?", Int) = 0
        [HideInInspector]_DissolveDetailStrengthAnimated ("Dissolve Detail Strength", Int) = 0
        [HideInInspector]_DissolveAlphaAnimated ("Dissolve Alpha", Int) = 0
        [HideInInspector]_DissolveUseVertexColorsAnimated ("Dissolve Alpha", Int) = 0
        [HideInInspector]_DissolveMaskAnimated ("Dissolve Mask", Int) = 0
        [HideInInspector]_DissolveMaskPanAnimated ("Panning", Int) = 0
        [HideInInspector]_DissolveMaskUVAnimated ("UV", Int) = 0
        [HideInInspector]_DissolveMaskInvertAnimated ("Invert?", Int) = 0
        [HideInInspector]_ContinuousDissolveAnimated ("Continuous Dissolve Speed", Int) = 0
        [HideInInspector]_DissolveEmissionSideAnimated ("Emission 1", Int) = 0
        [HideInInspector]_DissolveEmission1SideAnimated ("Emission 2", Int) = 0
        
        // Point to Point Dissolve
        [HideInInspector]_DissolveP2PWorldLocalAnimated ("World/Local", Int) = 0
        [HideInInspector]_DissolveP2PEdgeLengthAnimated ("Edge Length", Int) = 0
        [HideInInspector]_DissolveStartPointAnimated ("Start Point", Int) = 0
        [HideInInspector]_DissolveEndPointAnimated ("End Point", Int) = 0
                        
        [HideInInspector]_DissolveAlpha0Animated ("_DissolveAlpha0", Int) = 0
        [HideInInspector]_DissolveAlpha1Animated ("_DissolveAlpha1", Int) = 0
        [HideInInspector]_DissolveAlpha2Animated ("_DissolveAlpha2", Int) = 0
        [HideInInspector]_DissolveAlpha3Animated ("_DissolveAlpha3", Int) = 0
        [HideInInspector]_DissolveAlpha4Animated ("_DissolveAlpha4", Int) = 0
        [HideInInspector]_DissolveAlpha5Animated ("_DissolveAlpha5", Int) = 0
        [HideInInspector]_DissolveAlpha6Animated ("_DissolveAlpha6", Int) = 0
        [HideInInspector]_DissolveAlpha7Animated ("_DissolveAlpha7", Int) = 0
        [HideInInspector]_DissolveAlpha8Animated ("_DissolveAlpha8", Int) = 0
        [HideInInspector]_DissolveAlpha9Animated ("_DissolveAlpha9", Int) = 0

        // Panosphere
        [HideInInspector]_PanoToggleAnimated ("Enable Panosphere", Int) = 0
        [HideInInspector]_PanoInfiniteStereoToggleAnimated ("Infinite Stereo", Int) = 0
        [HideInInspector]_PanosphereColorAnimated ("Color", Int) = 0
        [HideInInspector]_PanosphereTextureAnimated ("Texture", Int) = 0
        [HideInInspector]_PanoMaskAnimated ("Mask", Int) = 0
        [HideInInspector]_PanoMaskPanAnimated ("Panning", Int) = 0
        [HideInInspector]_PanoMaskUVAnimated ("UV", Int) = 0
        [HideInInspector]_PanoEmissionAnimated ("Emission Strength", Int) = 0
        [HideInInspector]_PanoBlendAnimated ("Alpha", Int) = 0
        [HideInInspector]_PanospherePanAnimated ("Pan Speed", Int) = 0
        [HideInInspector]_PanoCubeMapToggleAnimated ("Use Cubemap", Int) = 0
        [HideInInspector]_PanoCubeMapAnimated ("CubeMap", Int) = 0
        
        // Glitter
        [HideInInspector]_GlitterEnableAnimated ("Enable Glitter?", Int) = 0
        [HideInInspector]_GlitterModeAnimated ("Mode", Int) = 0
        [HideInInspector]_GlitterShapeAnimated ("Mode", Int) = 0
        [HideInInspector]_GlitterBlendTypeAnimated ("Blend Mode", Int) = 0
        [HideInInspector]_GlitterColorAnimated ("Color", Int) = 0
        [HideInInspector]_GlitterUseSurfaceColorAnimated ("Use Surface Color", Int) = 0
        [HideInInspector]_GlitterColorMapAnimated ("Glitter Color Map", Int) = 0
        [HideInInspector]_GlitterColorMapPanAnimated ("Panning", Int) = 0
        [HideInInspector]_GlitterColorMapUVAnimated ("UV", Int) = 0
        [HideInInspector]_GlitterPanAnimated ("Panning", Int) = 0
        [HideInInspector]_GlitterMaskAnimated ("Glitter Mask", Int) = 0
        [HideInInspector]_GlitterMaskPanAnimated ("Panning", Int) = 0
        [HideInInspector]_GlitterMaskUVAnimated ("UV", Int) = 0
        [HideInInspector]_GlitterTextureAnimated ("Glitter Mask", Int) = 0
        [HideInInspector]_GlitterTexturePanAnimated ("Panning", Int) = 0
        [HideInInspector]_GlitterRandomRotationAnimated ("Panning", Int) = 0
        [HideInInspector]_GlitterFrequencyAnimated ("Glitter Density", Int) = 0
        [HideInInspector]_GlitterJitterAnimated ("Glitter Jitter", Int) = 0
        [HideInInspector]_GlitterSpeedAnimated ("Glitter Wobble Speed", Int) = 0
        [HideInInspector]_GlitterSizeAnimated ("Glitter Size", Int) = 0
        [HideInInspector]_GlitterContrastAnimated ("Glitter Contrast", Int) = 0
        [HideInInspector]_GlitterAngleRangeAnimated ("Glitter Angle Range", Int) = 0
        [HideInInspector]_GlitterMinBrightnessAnimated ("Glitter Min Brightness", Int) = 0
        [HideInInspector]_GlitterBrightnessAnimated ("Glitter Max Brightness", Int) = 0
        [HideInInspector]_GlitterBiasAnimated ("Glitter Bias", Int) = 0
        [HideInInspector]_GlitterCenterSizeAnimated ("center size", Int) = 0
        [HideInInspector]_GlitterTextureRotationAnimated ("Rotation Speed", Int) = 0
        [HideInInspector]_glitterFrequencyLinearEmissiveAnimated ("Frequency", Int) = 0
        [HideInInspector]_GlitterJaggyFixAnimated ("Jaggy Fix", Int) = 0
        
        // Glitter Random Colors
        [HideInInspector]_GlitterRandomColorsAnimated ("Enable", Int) = 0
        [HideInInspector]_GlitterMinMaxSaturationAnimated ("Saturation Range", Int) = 0
        [HideInInspector]_GlitterMinMaxBrightnessAnimated ("Brightness Range", Int) = 0
        [HideInInspector]_GlitterRandomSizeAnimated ("random Size Toggle", Int) = 0
        [HideInInspector]_GlitterMinMaxSizeAnimated ("Min Max Random Size", Int) = 0
        
        // MSDF OVERLAY
        [HideInInspector]_TextGlyphsAnimated ("Font Array", Int) = 0
        [HideInInspector]_TextPixelRangeAnimated ("Pixel Range", Int) = 0
        [HideInInspector]_TextEnabledAnimated ("Text?", Int) = 0
        
        // FPS
        [HideInInspector]_TextFPSEnabledAnimated ("FPS Text?", Int) = 0
        [HideInInspector]_TextFPSUVAnimated ("FPS UV", Int) = 0
        [HideInInspector]_TextFPSColorAnimated ("Color", Int) = 0
        [HideInInspector]_TextFPSEmissionStrengthAnimated ("Emission Strength", Int) = 0
        [HideInInspector]_TextFPSOffsetAnimated ("Offset", Int) = 0
        [HideInInspector]_TextFPSRotationAnimated ("Rotation", Int) = 0
        [HideInInspector]_TextFPSScaleAnimated ("Scale", Int) = 0
        [HideInInspector]_TextFPSPaddingAnimated ("Padding Reduction", Int) = 0
        
        // POSITION
        [HideInInspector]_TextPositionEnabledAnimated ("Position Text?", Int) = 0
        [HideInInspector]_TextPositionUVAnimated ("Position UV", Int) = 0
        //[ToggleUI]_TextPositionVerticalAnimated ("Vertical?", Int) = 0
        [HideInInspector]_TextPositionColorAnimated ("Color", Int) = 0
        [HideInInspector]_TextPositionEmissionStrengthAnimated ("Emission Strength", Int) = 0
        [HideInInspector]_TextPositionOffsetAnimated ("Offset", Int) = 0
        [HideInInspector]_TextPositionRotationAnimated ("Rotation", Int) = 0
        [HideInInspector]_TextPositionScaleAnimated ("Scale", Int) = 0
        [HideInInspector]_TextPositionPaddingAnimated ("Padding Reduction", Int) = 0
        
        // INSTANCE TIME
        [HideInInspector]_TextTimeEnabledAnimated ("Time Text?", Int) = 0
        [HideInInspector]_TextTimeUVAnimated ("Time UV", Int) = 0
        [HideInInspector]_TextTimeColorAnimated ("Color", Int) = 0
        [HideInInspector]_TextTimeEmissionStrengthAnimated ("Emission Strength", Int) = 0
        [HideInInspector]_TextTimeOffsetAnimated ("Offset", Int) = 0
        [HideInInspector]_TextTimeRotationAnimated ("Rotation", Int) = 0
        [HideInInspector]_TextTimeScaleAnimated ("Scale", Int) = 0
        [HideInInspector]_TextTimePaddingAnimated ("Padding Reduction", Int) = 0
        
        // Mirror Rendering
        [HideInInspector]_EnableMirrorOptionsAnimated ("Enable Mirror Options", Int) = 0
        [HideInInspector]_MirrorAnimated ("Show in mirror", Int) = 0
        [HideInInspector]_EnableMirrorTextureAnimated ("Enable Mirror Texture", Int) = 0
        [HideInInspector]_MirrorTextureAnimated ("Mirror Tex", Int) = 0
        [HideInInspector]_MirrorTexturePanAnimated ("Panning", Int) = 0
        [HideInInspector]_MirrorTextureUVAnimated ("UV", Int) = 0
        
        // Distance Fade
        [HideInInspector]_MainMinAlphaAnimated ("Minimum Alpha", Int) = 0
        [HideInInspector]_MainFadeTextureAnimated ("Fade Mask", Int) = 0
        [HideInInspector]_MainFadeTexturePanAnimated ("Panning", Int) = 0
        [HideInInspector]_MainFadeTextureUVAnimated ("UV", Int) = 0
        [HideInInspector]_MainDistanceFadeAnimated ("Distance Fade X to Y", Int) = 0
        
        // Angular Fade
        [HideInInspector]_EnableRandomAnimated ("Enable Angular Fade", Int) = 0
        [HideInInspector]_AngleTypeAnimated ("Angle Type", Int) = 0
        [HideInInspector]_AngleCompareToAnimated ("Model or Vert Positon", Int) = 0
        [HideInInspector]_AngleForwardDirectionAnimated ("Forward Direction", Int) = 0
        [HideInInspector]_CameraAngleMinAnimated ("Camera Angle Min", Int) = 0
        [HideInInspector]_CameraAngleMaxAnimated ("Camera Angle Max", Int) = 0
        [HideInInspector]_ModelAngleMinAnimated ("Model Angle Min", Int) = 0
        [HideInInspector]_ModelAngleMaxAnimated ("Model Angle Max", Int) = 0
        [HideInInspector]_AngleMinAlphaAnimated ("Min Alpha", Int) = 0
        
        // UV Distortion
        [HideInInspector]_EnableDistortionAnimated ("Enabled?", Int) = 0
        [HideInInspector]_DistortionMaskAnimated ("Mask", Int) = 0
        [HideInInspector]_DistortionMaskPanAnimated ("Panning", Int) = 0
        [HideInInspector]_DistortionMaskUVAnimated ("Distortion Mask UV", Int) = 0
        [HideInInspector]_DistortionFlowTextureAnimated ("Distortion Texture 1", Int) = 0
        [HideInInspector]_DistortionFlowTexture1Animated ("Distortion Texture 2", Int) = 0
        [HideInInspector]_DistortionStrengthAnimated ("Strength1", Int) = 0
        [HideInInspector]_DistortionStrength1Animated ("Strength2", Int) = 0
        [HideInInspector]_DistortionSpeedAnimated ("Speed1", Int) = 0
        [HideInInspector]_DistortionSpeed1Animated ("Speed2", Int) = 0
        
        // Video Options
        [HideInInspector]_EnableVideoAnimated ("Enable Video", Int) = 0
        [HideInInspector]_VideoUVNumberAnimated ("Screen UV#", Int) = 0
        [HideInInspector]_VideoTypeAnimated ("Screen Type", Int) = 0
        [HideInInspector]_VideoBacklightAnimated ("Brightness", Int) = 0
        [HideInInspector]_VideoPixelTextureAnimated ("Pixel Texture", Int) = 0
        [HideInInspector]_VideoResolutionAnimated ("Resolution", Int) = 0
        [HideInInspector]_VideoMaskTextureAnimated ("Mask", Int) = 0
        [HideInInspector]_VideoMaskPanningAnimated ("Mask Pan Speed", Int) = 0
        [HideInInspector]_VideoEnableVideoPlayerAnimated ("Enable Video Player", Int) = 0
        [HideInInspector]_VideoPixelateToResolutionAnimated ("Pixelate To Resolution", Int) = 0
        [HideInInspector]_VideoRepeatVideoTextureAnimated ("Clamp To UV", Int) = 0
        [HideInInspector]_VideoPanningAnimated ("Panning Speed", Int) = 0
        [HideInInspector]_VideoTilingAnimated ("Tiling", Int) = 0
        [HideInInspector]_VideoOffsetAnimated ("Offset", Int) = 0
        [HideInInspector]_VideoSaturationAnimated ("Saturation", Int) = 0
        [HideInInspector]_VideoContrastAnimated ("Contrast boost", Int) = 0
        [HideInInspector]_VideoEnableDebugAnimated ("Enable Debug", Int) = 0
        [HideInInspector]_VideoDebugTextureAnimated ("Video Debug Tex", Int) = 0
        [HideInInspector]_VideoCRTRefreshRateAnimated ("Refresh Rate", Int) = 0
        [HideInInspector]_VideoCRTPixelEnergizedTimeAnimated ("Pixel Fade Time", Int) = 0
        [HideInInspector]_VideoGameboyRampAnimated ("Color Ramp", Int) = 0
        
        // TouchFX
        [HideInInspector]_EnableBulgeAnimated ("Bulge", Int) = 0
        [HideInInspector]_BulgeMaskAnimated ("Bulge Mask", Int) = 0
        [HideInInspector]_BuldgeFadeLengthAnimated ("Touch Distance", Int) = 0
        [HideInInspector]_BuldgeHeightAnimated ("Bulge Height", Int) = 0

        [HideInInspector]_EnableTouchGlowAnimated ("Enable Touch Glow", Int) = 0
        [HideInInspector]_DepthGradientTextureUVAnimated ("", Int) = 0
        [HideInInspector]_DepthGradientBlendAnimated ("", Int) = 0
        [HideInInspector]_DepthGradientPanAnimated ("", Int) = 0
        [HideInInspector]_DepthGradientUVAnimated ("", Int) = 0
        [HideInInspector]_DepthMaskPanAnimated ("", Int) = 0
        [HideInInspector]_DepthMaskUVAnimated ("", Int) = 0
        [HideInInspector]_DepthGlowColorAnimated ("Depth Glow Color", Int) = 0
        [HideInInspector]_DepthGradientAnimated ("Depth Gradient", Int) = 0
        [HideInInspector]_DepthMaskAnimated ("Depth Mask", Int) = 0
        [HideInInspector]_DepthGlowEmissionAnimated ("Depth Glow Emission", Int) = 0
        [HideInInspector]_FadeLengthAnimated ("Fade Length", Int) = 0
        [HideInInspector]_DepthAlphaMinAnimated ("Alpha Min", Int) = 0
        [HideInInspector]_DepthAlphaMaxAnimated ("Alpha Max", Int) = 0
        
        // Hologram
        [HideInInspector]_EnableHoloAnimated ("Enable Hologram Alpha", Int) = 0
        [HideInInspector]_HoloAlphaMapAnimated ("Alpha Map", Int) = 0
        [HideInInspector]_HoloCoordinateSpaceAnimated ("Coordinate Space", Int) = 0
        [HideInInspector]_HoloDirectionAnimated ("Scroll Direction", Int) = 0
        [HideInInspector]_HoloLineDensityAnimated ("Line Density", Int) = 0
        [HideInInspector]_HoloScrollSpeedAnimated ("Scroll Speed", Int) = 0
        [HideInInspector]_HoloFresnelAlphaAnimated ("Intensity", Int) = 0
        [HideInInspector]_HoloRimSharpnessAnimated ("Sharpness", Int) = 0
        [HideInInspector]_HoloRimWidthAnimated ("Width", Int) = 0
        
        // GrabPass
        [HideInInspector]_GrabPassUseAlphaAnimated ("Source Blend", Int) = 0
        [HideInInspector]_GrabPassBlendFactorAnimated ("Source Blend", Int) = 0
        [HideInInspector]_GrabSrcBlendAnimated ("Source Blend", Int) = 0
        [HideInInspector]_GrabDstBlendAnimated ("Destination Blend", Int) = 0
        [HideInInspector]_RefractionEnabledAnimated ("Enable Refraction,", Int) = 0
        [HideInInspector]_RefractionIndexAnimated ("Refraction", Int) = 0
        [HideInInspector]_RefractionChromaticAberattionAnimated ("Chromatic Aberration", Int) = 0
        [HideInInspector]_EnableBlurAnimated ("Enable Blur", Int) = 0
        [HideInInspector]_GrabBlurDistanceAnimated ("Blur Distance", Int) = 0
        [HideInInspector]_GrabBlurQualityAnimated ("Blur Quality", Int) = 0
        [HideInInspector]_GrabBlurDirectionsAnimated ("Blur Direction", Int) = 0
        
        // Iridescence
        [HideInInspector]_EnableIridescenceAnimated ("Enable Iridescence", Int) = 0
        [HideInInspector]_IridescenceRampAnimated ("Ramp", Int) = 0
        [HideInInspector]_IridescenceNormalToggleAnimated ("Custom Normals?", Int) = 0
        [HideInInspector]_IridescenceNormalMapAnimated ("Normal Map", Int) = 0
        [HideInInspector]_IridescenceMaskAnimated ("Mask", Int) = 0
        [HideInInspector]_IridescenceNormalIntensityAnimated ("Normal Intensity", Int) = 0
        [HideInInspector]_IridescenceNormalUVAnimated ("Normal UV", Int) = 0
        [HideInInspector]_IridescenceMaskUVAnimated ("Mask UV", Int) = 0
        [HideInInspector]_IridescenceNormalSelectionAnimated ("Normal Select", Int) = 0
        [HideInInspector]_IridescenceIntensityAnimated ("Intensity", Int) = 0
        [HideInInspector]_IridescenceAddBlendAnimated ("Blend Add", Int) = 0
        [HideInInspector]_IridescenceReplaceBlendAnimated ("Blend Replace", Int) = 0
        [HideInInspector]_IridescenceMultiplyBlendAnimated ("Blend Multiply", Int) = 0
        [HideInInspector]_IridescenceEmissionStrengthAnimated ("Emission Strength", Int) = 0
        [HideInInspector]_IridescenceTimeAnimated ("When To Run", Int) = 0
        
        // Vertex Glitching
        [HideInInspector]_EnableVertexGlitchAnimated ("Enable Vertex Glitching", Int) = 0
        [HideInInspector]_VertexGlitchFrequencyAnimated ("Glitch Interval", Int) = 0
        [HideInInspector]_VertexGlitchThresholdAnimated ("Glitch Threshold", Int) = 0
        [HideInInspector]_VertexGlitchStrengthAnimated ("Glitch Strength", Int) = 0
        
        // Spawn In Effects
        [HideInInspector]_EnableScifiSpawnInAnimated ("Enable Sci Fi Spawn", Int) = 0
        [HideInInspector]_SpawnInNoiseAnimated ("Spawn Noise", Int) = 0
        [HideInInspector]_SpawnInNoiseIntensityAnimated ("Noise Intensity", Int) = 0
        [HideInInspector]_SpawnInAlphaAnimated ("Spawn Alpha", Int) = 0
        [HideInInspector]_SpawnInGradientStartAnimated ("Gradient Start", Int) = 0
        [HideInInspector]_SpawnInGradientFinishAnimated ("Gradient End", Int) = 0
        [HideInInspector]_SpawnInEmissionColorAnimated ("Emission Color", Int) = 0
        [HideInInspector]_SpawnInEmissionOffsetAnimated ("Emission Width", Int) = 0
        [HideInInspector]_SpawnInVertOffsetAnimated ("Vertex Offset Speed", Int) = 0
        [HideInInspector]_SpawnInVertOffsetOffsetAnimated ("vert width", Int) = 0
        
        // Voronoi
        [HideInInspector]_VoronoiTypeAnimated ("Space", Int) = 0
        [HideInInspector]_VoronoiSpaceAnimated ("Space", Int) = 0
        [HideInInspector]_VoronoiBlendAnimated ("Blend", Int) = 0
        [HideInInspector]_EnableVoronoiAnimated ("Enable Voronoi", Int) = 0
        [HideInInspector]_VoronoiEffectsMaterialAlphaAnimated ("Enable Voronoi", Int) = 0
        [HideInInspector]_VoronoiMaskAnimated ("Mask", Int) = 0
        [HideInInspector]_VoronoiMaskPanAnimated ("Panning", Int) = 0
        [HideInInspector]_VoronoiMaskUVAnimated ("UV", Int) = 0
        [HideInInspector]_VoronoiNoiseAnimated ("Edge Noise", Int) = 0
        [HideInInspector]_VoronoiNoisePanAnimated ("Panning", Int) = 0
        [HideInInspector]_VoronoiNoiseUVAnimated ("UV", Int) = 0
        [HideInInspector]_VoronoiNoiseIntensityAnimated ("Noise Intensity", Int) = 0
        [HideInInspector]_VoronoiColor0Animated ("Color 0", Int) = 0
        [HideInInspector]_VoronoiEmission0Animated ("Emission 0", Int) = 0
        [HideInInspector]_VoronoiColor1Animated ("Color 1", Int) = 0
        [HideInInspector]_VoronoiEmission1Animated ("Emission 1", Int) = 0
        [HideInInspector]_VoronoiGradientAnimated ("Gradient", Int) = 0
        [HideInInspector]_VoronoiScaleAnimated ("Scale", Int) = 0
        [HideInInspector]_VoronoiSpeedAnimated ("Speed", Int) = 0
        [HideInInspector]_VoronoiEnableRandomCellColorAnimated ("Rando Cell Col", Int) = 0
        [HideInInspector]_VoronoiRandomMinMaxSaturationAnimated ("Saturation Range", Int) = 0
        [HideInInspector]_VoronoiRandomMinMaxBrightnessAnimated ("Brightness Range", Int) = 0
        
        // Blacklight mask
        [HideInInspector]_BlackLightMaskEnabledAnimated ("Black Light Mask Enabled", Int) = 0
        [HideInInspector]_BlackLightMaskKeysAnimated ("Mask Keys", Int) = 0
        [HideInInspector]_BlackLightMaskStartAnimated ("Gradient Start", Int) = 0
        [HideInInspector]_BlackLightMaskEndAnimated ("Gradient End", Int) = 0
        [HideInInspector]_BlackLightMaskDebugAnimated ("Visualize", Int) = 0
        [HideInInspector]_BlackLightMaskMetallicAnimated ("Metallic", Int) = 0
        [HideInInspector]_BlackLightMaskClearCoatAnimated ("Clear Coat", Int) = 0
        [HideInInspector]_BlackLightMaskMatcapAnimated ("Matcap 1", Int) = 0
        [HideInInspector]_BlackLightMaskMatcap2Animated ("Matcap 2", Int) = 0
        [HideInInspector]_BlackLightMaskEmissionAnimated ("Emission 1", Int) = 0
        [HideInInspector]_BlackLightMaskEmission2Animated ("Emission 2", Int) = 0
        [HideInInspector]_BlackLightMaskFlipbookAnimated ("Flipbook", Int) = 0
        [HideInInspector]_BlackLightMaskDissolveAnimated ("Dissolve", Int) = 0
        [HideInInspector]_BlackLightMaskPanosphereAnimated ("Panosphere", Int) = 0
        [HideInInspector]_BlackLightMaskGlitterAnimated ("Glitter", Int) = 0
        [HideInInspector]_BlackLightMaskIridescenceAnimated ("Iridescence", Int) = 0
        
        // Outline Options
        [HideInInspector]_OutlineModeAnimated ("Mode", Int) = 0
        [HideInInspector]_OutlineFixedSizeAnimated ("Fixed Size?", Int) = 0
        [HideInInspector]_OutlineUseVertexColorsAnimated ("V Color", Int) = 0
        [HideInInspector]_OutlineLitAnimated ("Enable Lighting", Int) = 0
        [HideInInspector]_LineWidthAnimated ("Width", Int) = 0
        [HideInInspector]_LineColorAnimated ("Color", Int) = 0
        [HideInInspector]_OutlineTintMixAnimated ("Tint Mix", Int) = 0
        [HideInInspector]_OutlineEmissionAnimated ("Outline Emission", Int) = 0
        [HideInInspector]_OutlineTextureAnimated ("Outline Texture", Int) = 0
        [HideInInspector]_OutlineMaskAnimated ("Outline Mask", Int) = 0
        [HideInInspector]_OutlineTexturePanAnimated ("Outline Texture Pan", Int) = 0
        [HideInInspector]_OutlineShadowStrengthAnimated ("Shadow Strength", Int) = 0
        [HideInInspector]_OutlineRimLightBlendAnimated ("Rim Light Blend", Int) = 0
        [HideInInspector]_OutlinePersonaDirectionAnimated ("directional Offset XY", Int) = 0
        [HideInInspector]_OutlineDropShadowOffsetAnimated ("Drop Direction XY", Int) = 0
        [HideInInspector]_OutlineFadeDistanceAnimated ("Outline distance Fade", Int) = 0
        [HideInInspector]_OutlineOffsetFactor ("Outline distance Fade", Int) = 0
        [HideInInspector]_OutlineOffsetUnits ("Outline distance Fade", Int) = 0
        [HideInInspector]_OutlineCullAnimated ("Cull", Int) = 0
        
        // Parallax Mapping
        [HideInInspector]_ParallaxMapAnimated ("Enable Parallax FX", Int) = 0
        [HideInInspector]_ParallaxHeightMapEnabledAnimated ("Enable Parallax Height", Int) = 0
        [HideInInspector]_ParallaxInternalMapEnabledAnimated ("Enable Parallax Internal", Int) = 0
        [HideInInspector]_ParallaxHeightMapPanAnimated ("Pan", Int) = 0
        [HideInInspector]_ParallaxUVAnimated ("Parallax UV", Int) = 0
        [HideInInspector]_ParallaxHeightMapAnimated ("Height Map", Int) = 0
        [HideInInspector]_ParallaxHeightMapMaskAnimated ("Mask", Int) = 0
        [HideInInspector]_ParallaxHeightMapMaskPanAnimated ("Panning", Int) = 0
        [HideInInspector]_ParallaxHeightMapMaskUVAnimated ("UV", Int) = 0
        [HideInInspector]_ParallaxStrengthAnimated ("Parallax Strength", Int) = 0
        [HideInInspector]_ParallaxInternalHeightmapModeAnimated ("Parallax Mode", Int) = 0
        [HideInInspector]_ParallaxInternalHeightFromAlphaAnimated ("HeightFromAlpha", Int) = 0
        [HideInInspector]_ParallaxInternalMapAnimated ("Internal Map", Int) = 0
        [HideInInspector]_ParallaxInternalMapMaskAnimated ("Mask", Int) = 0
        [HideInInspector]_ParallaxInternalMapMaskPanAnimated ("Panning", Int) = 0
        [HideInInspector]_ParallaxInternalMapMaskUVAnimated ("UV", Int) = 0
        [HideInInspector]_ParallaxInternalIterationsAnimated ("Parallax Internal Iterations", Int) = 0
        [HideInInspector]_ParallaxInternalMinDepthAnimated ("Min Depth", Int) = 0
        [HideInInspector]_ParallaxInternalMaxDepthAnimated ("Max Depth", Int) = 0
        [HideInInspector]_ParallaxInternalMinFadeAnimated ("Min Depth Brightness", Int) = 0
        [HideInInspector]_ParallaxInternalMaxFadeAnimated ("Max Depth Brightness", Int) = 0
        [HideInInspector]_ParallaxInternalMinColorAnimated ("Min Depth Color", Int) = 0
        [HideInInspector]_ParallaxInternalMaxColorAnimated ("Max Depth Color", Int) = 0
        [HideInInspector]_ParallaxInternalPanSpeedAnimated ("Pan Speed", Int) = 0
        [HideInInspector]_ParallaxInternalPanDepthSpeedAnimated ("Per Level Speed Multiplier", Int) = 0
        [HideInInspector]_ParallaxBiasAnimated ("Parallax Bias (0.42)", Int) = 0
        
        // Rendering Options
        [HideInInspector]_CullAnimated ("Cull", Int) = 0
        [HideInInspector]_ZTestAnimated ("ZTest", Int) = 0
        [HideInInspector]_ZWriteAnimated ("ZWrite", Int) = 0
        [HideInInspector]_ColorMaskAnimated ("Color Mask", Int) = 0
        [HideInInspector]_OffsetFactorAnimated ("Offset Factor", Int) = 0
        [HideInInspector]_OffsetUnitsAnimated ("Offset Units", Int) = 0
        [HideInInspector]_IgnoreFogAnimated ("Ignore Fog", Int) = 0
        
        // Blending Options
        [HideInInspector]_BlendOpAnimated ("RGB Blend Op", Int) = 0
        [HideInInspector]_BlendOpAlphaAnimated ("Alpha Blend Op", Int) = 0
        [HideInInspector]_SrcBlendAnimated ("RGB Source Blend", Int) = 0
        [HideInInspector]_DstBlendAnimated ("RGB Destination Blend", Int) = 0
        
        // Stencils
        [HideInInspector]_StencilRefAnimated ("Stencil Reference Value", Int) = 0
        [HideInInspector]_StencilReadMaskAnimated ("Stencil ReadMask Value", Int) = 0
        [HideInInspector]_StencilWriteMaskAnimated ("Stencil WriteMask Value", Int) = 0
        [HideInInspector]_StencilPassOpAnimated ("Stencil Pass Op", Int) = 0
        [HideInInspector]_StencilFailOpAnimated ("Stencil Fail Op", Int) = 0
        [HideInInspector]_StencilZFailOpAnimated ("Stencil ZFail Op", Int) = 0
        [HideInInspector]_StencilCompareFunctionAnimated ("Stencil Compare Function", Int) = 0
        
        // Outline Stencil
        [HideInInspector]_OutlineStencilRefAnimated ("Stencil Reference Value", Int) = 0
        [HideInInspector]_OutlineStencilReadMaskAnimated ("Stencil ReadMask Value", Int) = 0
        [HideInInspector]_OutlineStencilWriteMaskAnimated ("Stencil WriteMask Value", Int) = 0
        [HideInInspector]_OutlineStencilPassOpAnimated ("Stencil Pass Op", Int) = 0
        [HideInInspector]_OutlineStencilFailOpAnimated ("Stencil Fail Op", Int) = 0
        [HideInInspector]_OutlineStencilZFailOpAnimated ("Stencil ZFail Op", Int) = 0
        [HideInInspector]_OutlineStencilCompareFunctionAnimated ("Stencil Compare Function", Int) = 0
        
        // Debug Options
        [HideInInspector]_VertexUnwrapAnimated ("Unwrap", Range(0, 1)) = 0
        [HideInInspector]_DebugMeshDataAnimated ("Mesh Data", Int) = 0
        [HideInInspector]_DebugLightingDataAnimated ("Lighting Data", Int) = 0
        [HideInInspector]_DebugCameraDataAnimated ("Camera Data", Int) = 0
    }
    
    
    //originalEditorCustomEditor "PoiToon"
    CustomEditor "Thry.ShaderEditor"
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }

        //EnableOutlinePass
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "ForwardBase" }
            Stencil
            {
                Ref [_OutlineStencilRef]
                ReadMask [_OutlineStencilReadMask]
                WriteMask [_OutlineStencilWriteMask]
                Comp [_OutlineStencilCompareFunction]
                Pass [_OutlineStencilPassOp]
                Fail [_OutlineStencilFailOp]
                ZFail [_OutlineStencilZFailOp]
            }
            ZTest [_ZTest]
            ColorMask [_ColorMask]
            Offset [_OutlineOffsetFactor], [_OutlineOffsetUnits]
            BlendOp [_BlendOp], [_BlendOpAlpha]
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            AlphaToMask [_AlphaToMask]
            Cull [_OutlineCull]
            CGPROGRAM
            
            #pragma target 5.0
            #define FORWARD_BASE_PASS
            #define OUTLINE
            float _Mode;
            // patreon Additive
            // Black Light Mask
            #pragma shader_feature _SPECULARHIGHLIGHTS_OFF
            // voronoi
            #pragma shader_feature CHROMATIC_ABERRATION
            // UV Distortion
            #pragma shader_feature USER_LUT
            // Vertex Offsets
            #pragma shader_feature AUTO_EXPOSURE
            // Mirror
            #pragma shader_feature _REQUIRE_UV2
            // Random
            #pragma shader_feature _SUNDISK_NONE
            // Dissolve
            #pragma shader_feature DISTORT
            // Lighting
            #pragma shader_feature VIGNETTE_MASKED
            #pragma multi_compile_fwdbase
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag
            #include "../Includes/CGI_PoiPassOutline.cginc"
            ENDCG
            
        }
        //EnableOutlinePass

        Pass
        {
            Name "MainPass"
            Tags { "LightMode" = "ForwardBase" }
            
            Stencil
            {
                Ref [_StencilRef]
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
                Comp [_StencilCompareFunction]
                Pass [_StencilPassOp]
                Fail [_StencilFailOp]
                ZFail [_StencilZFailOp]
            }
            
            ZWrite [_ZWrite]
            Cull [_Cull]
            AlphaToMask [_AlphaToMask]
            ZTest [_ZTest]
            ColorMask [_ColorMask]
            Offset [_OffsetFactor], [_OffsetUnits]
            
            BlendOp [_BlendOp], [_BlendOpAlpha]
            Blend [_SrcBlend] [_DstBlend]
            
            CGPROGRAM
            
            #pragma target 5.0
            #define FORWARD_BASE_PASS
            float _Mode;
            // Base Pass Features
            // Decal
            #pragma shader_feature GEOM_TYPE_BRANCH
            #pragma multi_compile _ VERTEXLIGHT_ON
            // patreon Base
            // Black Light Mask
            #pragma shader_feature _SPECULARHIGHLIGHTS_OFF
            // voronoi
            #pragma shader_feature CHROMATIC_ABERRATION
            // UV Distortion
            #pragma shader_feature USER_LUT
            // Vertex Offsets
            #pragma shader_feature AUTO_EXPOSURE
            // Bulge
            #pragma shader_feature BLOOM_LOW
            // Hologram Alpha
            #pragma shader_feature DEPTH_OF_FIELD
            //Grab Pass Blur
            #pragma shader_feature CHROMATIC_ABERRATION_LOW
            //Video
            #pragma shader_feature BLOOM
            #pragma shader_feature _PARALLAXMAP
            // Mirror
            #pragma shader_feature _REQUIRE_UV2
            // Random
            #pragma shader_feature _SUNDISK_NONE
            // Hue Shift
            #pragma shader_feature COLOR_GRADING_HDR
            // Dissolve
            #pragma shader_feature DISTORT
            // Panosphere
            #pragma shader_feature _DETAIL_MULX2
            // Touch Color
            #pragma shader_feature GRAIN
            // Lighting
            #pragma shader_feature VIGNETTE_MASKED
            // Flipbook
            #pragma shader_feature _SUNDISK_HIGH_QUALITY
            // Rim Lighting
            #pragma shader_feature _GLOSSYREFLECTIONS_OFF
            // Enviro Rim
            #pragma shader_feature _MAPPING_6_FRAMES_LAYOUT
            // Metal
            #pragma shader_feature _METALLICGLOSSMAP
            // Iridescence
            #pragma shader_feature BLOOM_LENS_DIRT
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
            #pragma shader_feature VIGNETTE
            // RGB NORMALS
            #pragma shader_feature GEOM_TYPE_MESH
            //Details
            #pragma shader_feature FINALPASS
            // Text
            #pragma shader_feature EFFECT_BUMP
            // Emission 1
            #pragma shader_feature _EMISSION
            // Emission 2
            #pragma shader_feature EFFECT_HUE_VARIATION
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
        
        //LightingAdditiveEnable
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
            BlendOp [_BlendOp], [_BlendOpAlpha]
            Blend One One
            Cull [_Cull]
            ZTest [_ZTest]
            ColorMask [_ColorMask]
            Offset [_OffsetFactor], [_OffsetUnits]
            CGPROGRAM
            
            #pragma target 5.0
            #define FORWARD_ADD_PASS
            float _Mode;
            // Add Pass Features
            // Decal
            #pragma shader_feature GEOM_TYPE_BRANCH
            // patreon Additive
            // Black Light Mask
            #pragma shader_feature _SPECULARHIGHLIGHTS_OFF
            // voronoi
            #pragma shader_feature CHROMATIC_ABERRATION
            // UV Distortion
            #pragma shader_feature USER_LUT
            // Vertex Offsets
            #pragma shader_feature AUTO_EXPOSURE
            // Bulge
            #pragma shader_feature BLOOM_LOW
            #pragma shader_feature _PARALLAX_MAP
            // Mirror
            #pragma shader_feature _REQUIRE_UV2
            // Random
            #pragma shader_feature _SUNDISK_NONE
            // Hue Shift
            #pragma shader_feature COLOR_GRADING_HDR
            // Dissolve
            #pragma shader_feature DISTORT
            // Panosphere
            #pragma shader_feature _DETAIL_MULX2
            // Lighting
            #pragma shader_feature VIGNETTE_MASKED
            // Flipbook
            #pragma shader_feature _SUNDISK_HIGH_QUALITY
            // Rim Lighting
            #pragma shader_feature _GLOSSYREFLECTIONS_OFF
            // Metal
            #pragma shader_feature _METALLICGLOSSMAP
            // Iridescence
            #pragma shader_feature BLOOM_LENS_DIRT
            // Matcap
            #pragma shader_feature _COLORADDSUBDIFF_ON
            // Specular
            #pragma shader_feature _SPECGLOSSMAP
            // SubSurface
            #pragma shader_feature _TERRAIN_NORMAL_MAP
            // RGBMask
            #pragma shader_feature VIGNETTE
            // RGB NORMALS
            #pragma shader_feature GEOM_TYPE_MESH
            //Details
            #pragma shader_feature FINALPASS
            // Text
            #pragma shader_feature EFFECT_BUMP
            // Debug
            #pragma shader_feature _COLOROVERLAY_ON
            // Disable Directionals
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            #pragma multi_compile_instancing
            #pragma multi_compile_fwdadd_fullshadows
            #pragma vertex vert
            #pragma fragment frag
            #include "../Includes/CGI_PoiPass.cginc"
            ENDCG
            
        }
        //LightingAdditiveEnable
        
        //LightingCastShadows
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
            AlphaToMask Off
            ZWrite [_ZWrite]
            Cull [_Cull]
            ZTest [_ZTest]
            ColorMask [_ColorMask]
            Offset [_OffsetFactor], [_OffsetUnits]
            CGPROGRAM
            
            #pragma target 5.0
            #define POI_SHADOW
            float _Mode;
            // UV Distortion
            #pragma shader_feature USER_LUT
            // Vertex Offsets
            #pragma shader_feature AUTO_EXPOSURE
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
        //LightingCastShadows

        Pass
        {
            Tags { "LightMode" = "Meta" }
            Cull Off
            CGPROGRAM
            
            #pragma target 5.0
            #define POI_META_PASS
            float _Mode;
            // UV Distortion
            #pragma shader_feature USER_LUT
            // Hologram Alpha
            #pragma shader_feature DEPTH_OF_FIELD
            //Video
            #pragma shader_feature BLOOM
            #pragma shader_feature _PARALLAXMAP
            // Mirror
            #pragma shader_feature _REQUIRE_UV2
            // Random
            #pragma shader_feature _SUNDISK_NONE
            // Hue Shift
            #pragma shader_feature COLOR_GRADING_HDR
            // Dissolve
            #pragma shader_feature DISTORT
            // Panosphere
            #pragma shader_feature _DETAIL_MULX2
            // Lighting
            #pragma shader_feature VIGNETTE_MASKED
            // Flipbook
            #pragma shader_feature _SUNDISK_HIGH_QUALITY
            // Rim Lighting
            #pragma shader_feature _GLOSSYREFLECTIONS_OFF
            // Enviro Rim
            #pragma shader_feature _MAPPING_6_FRAMES_LAYOUT
            // Metal
            #pragma shader_feature _METALLICGLOSSMAP
            // Iridescence
            #pragma shader_feature BLOOM_LENS_DIRT
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
            #pragma shader_feature VIGNETTE
            // RGB NORMALS
            #pragma shader_feature GEOM_TYPE_MESH
            //Details
            #pragma shader_feature FINALPASS
            // Text
            #pragma shader_feature EFFECT_BUMP
            // Emission 1
            #pragma shader_feature _EMISSION
            // Emission 2
            #pragma shader_feature EFFECT_HUE_VARIATION
            // Clear Coat
            #pragma shader_feature _COLORCOLOR_ON
            #pragma vertex vert
            #pragma fragment frag
            #include "../Includes/CGI_PoiPass.cginc"
            ENDCG
            
        }
    }
}