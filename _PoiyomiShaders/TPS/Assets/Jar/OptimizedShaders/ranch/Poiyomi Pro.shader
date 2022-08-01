Shader "Hidden/Locked/.poiyomi/Poiyomi 8.1/Poiyomi Pro/663aad04491234d46b45ec0ed6b6c36a"
{
	Properties
	{
		[HideInInspector] shader_master_label ("<color=#E75898ff>Poiyomi 8.0.303</color>", Float) = 0
		[HideInInspector] shader_is_using_thry_editor ("", Float) = 0
		[HideInInspector] footer_youtube ("{texture:{name:icon-youtube,height:32},action:{type:URL,data:https://www.youtube.com/poiyomi},hover:YOUTUBE}", Float) = 0
		[HideInInspector] footer_twitter ("{texture:{name:icon-twitter,height:32},action:{type:URL,data:https://twitter.com/poiyomi},hover:TWITTER}", Float) = 0
		[HideInInspector] footer_patreon ("{texture:{name:icon-patreon,height:32},action:{type:URL,data:https://www.patreon.com/poiyomi},hover:PATREON}", Float) = 0
		[HideInInspector] footer_discord ("{texture:{name:icon-discord,height:32},action:{type:URL,data:https://discord.gg/Ays52PY},hover:DISCORD}", Float) = 0
		[HideInInspector] footer_github ("{texture:{name:icon-github,height:32},action:{type:URL,data:https://github.com/poiyomi/PoiyomiToonShader},hover:GITHUB}", Float) = 0
		[HideInInspector] _ForgotToLockMaterial (";;YOU_FORGOT_TO_LOCK_THIS_MATERIAL;", Int) = 1
		[ThryShaderOptimizerLockButton] _ShaderOptimizerEnabled ("", Int) = 0
		[Helpbox(1)] _LockTooltip ("Animations don't work by default when locked in. Right click a property if you want to animate it. The shader will lock in automatically at upload time.", Int) = 0
		[ThryWideEnum(Opaque, 0, Cutout, 1, TransClipping, 9, Fade, 2, Transparent, 3, Additive, 4, Soft Additive, 5, Multiplicative, 6, 2x Multiplicative, 7)]_Mode("Rendering Preset--{on_value_actions:[
		{value:0,actions:[{type:SET_PROPERTY,data:render_queue=2000}, {type:SET_PROPERTY,data:render_type=Opaque},            {type:SET_PROPERTY,data:_BlendOp=0}, {type:SET_PROPERTY,data:_BlendOpAlpha=0}, {type:SET_PROPERTY,data:_Cutoff=0},  {type:SET_PROPERTY,data:_SrcBlend=1}, {type:SET_PROPERTY,data:_DstBlend=0},  {type:SET_PROPERTY,data:_AlphaToMask=0},  {type:SET_PROPERTY,data:_ZWrite=1}, {type:SET_PROPERTY,data:_ZTest=4},   {type:SET_PROPERTY,data:_AlphaPremultiply=0}]},
		{value:1,actions:[{type:SET_PROPERTY,data:render_queue=2450}, {type:SET_PROPERTY,data:render_type=TransparentCutout}, {type:SET_PROPERTY,data:_BlendOp=0}, {type:SET_PROPERTY,data:_BlendOpAlpha=0}, {type:SET_PROPERTY,data:_Cutoff=.5}, {type:SET_PROPERTY,data:_SrcBlend=1}, {type:SET_PROPERTY,data:_DstBlend=0},  {type:SET_PROPERTY,data:_AlphaToMask=1},  {type:SET_PROPERTY,data:_ZWrite=1}, {type:SET_PROPERTY,data:_ZTest=4},   {type:SET_PROPERTY,data:_AlphaPremultiply=0}]},
		{value:9,actions:[{type:SET_PROPERTY,data:render_queue=2450}, {type:SET_PROPERTY,data:render_type=TransparentCutout}, {type:SET_PROPERTY,data:_BlendOp=0}, {type:SET_PROPERTY,data:_BlendOpAlpha=0}, {type:SET_PROPERTY,data:_Cutoff=0},  {type:SET_PROPERTY,data:_SrcBlend=5}, {type:SET_PROPERTY,data:_DstBlend=10}, {type:SET_PROPERTY,data:_AlphaToMask=0},  {type:SET_PROPERTY,data:_ZWrite=1}, {type:SET_PROPERTY,data:_ZTest=4},   {type:SET_PROPERTY,data:_AlphaPremultiply=0}]},
		{value:2,actions:[{type:SET_PROPERTY,data:render_queue=3000}, {type:SET_PROPERTY,data:render_type=Transparent},       {type:SET_PROPERTY,data:_BlendOp=0}, {type:SET_PROPERTY,data:_BlendOpAlpha=0}, {type:SET_PROPERTY,data:_Cutoff=0},  {type:SET_PROPERTY,data:_SrcBlend=5}, {type:SET_PROPERTY,data:_DstBlend=10}, {type:SET_PROPERTY,data:_AlphaToMask=0},  {type:SET_PROPERTY,data:_ZWrite=0}, {type:SET_PROPERTY,data:_ZTest=4},   {type:SET_PROPERTY,data:_AlphaPremultiply=0}]},
		{value:3,actions:[{type:SET_PROPERTY,data:render_queue=3000}, {type:SET_PROPERTY,data:render_type=Transparent},       {type:SET_PROPERTY,data:_BlendOp=0}, {type:SET_PROPERTY,data:_BlendOpAlpha=0}, {type:SET_PROPERTY,data:_Cutoff=0},  {type:SET_PROPERTY,data:_SrcBlend=1}, {type:SET_PROPERTY,data:_DstBlend=10}, {type:SET_PROPERTY,data:_AlphaToMask=0},  {type:SET_PROPERTY,data:_ZWrite=0}, {type:SET_PROPERTY,data:_ZTest=4},   {type:SET_PROPERTY,data:_AlphaPremultiply=1}]},
		{value:4,actions:[{type:SET_PROPERTY,data:render_queue=3000}, {type:SET_PROPERTY,data:render_type=Transparent},       {type:SET_PROPERTY,data:_BlendOp=0}, {type:SET_PROPERTY,data:_BlendOpAlpha=0}, {type:SET_PROPERTY,data:_Cutoff=0},  {type:SET_PROPERTY,data:_SrcBlend=1}, {type:SET_PROPERTY,data:_DstBlend=1},  {type:SET_PROPERTY,data:_AlphaToMask=0},  {type:SET_PROPERTY,data:_ZWrite=0}, {type:SET_PROPERTY,data:_ZTest=4},   {type:SET_PROPERTY,data:_AlphaPremultiply=0}]},
		{value:5,actions:[{type:SET_PROPERTY,data:render_queue=3000}, {type:SET_PROPERTY,data:RenderType=Transparent},        {type:SET_PROPERTY,data:_BlendOp=0}, {type:SET_PROPERTY,data:_BlendOpAlpha=0}, {type:SET_PROPERTY,data:_Cutoff=0},  {type:SET_PROPERTY,data:_SrcBlend=4}, {type:SET_PROPERTY,data:_DstBlend=1},  {type:SET_PROPERTY,data:_AlphaToMask=0},  {type:SET_PROPERTY,data:_ZWrite=0}, {type:SET_PROPERTY,data:_ZTest=4},   {type:SET_PROPERTY,data:_AlphaPremultiply=0}]},
		{value:6,actions:[{type:SET_PROPERTY,data:render_queue=3000}, {type:SET_PROPERTY,data:render_type=Transparent},       {type:SET_PROPERTY,data:_BlendOp=0}, {type:SET_PROPERTY,data:_BlendOpAlpha=0}, {type:SET_PROPERTY,data:_Cutoff=0},  {type:SET_PROPERTY,data:_SrcBlend=2}, {type:SET_PROPERTY,data:_DstBlend=0},  {type:SET_PROPERTY,data:_AlphaToMask=0},  {type:SET_PROPERTY,data:_ZWrite=0}, {type:SET_PROPERTY,data:_ZTest=4},   {type:SET_PROPERTY,data:_AlphaPremultiply=0}]},
		{value:7,actions:[{type:SET_PROPERTY,data:render_queue=3000}, {type:SET_PROPERTY,data:render_type=Transparent},       {type:SET_PROPERTY,data:_BlendOp=0}, {type:SET_PROPERTY,data:_BlendOpAlpha=0}, {type:SET_PROPERTY,data:_Cutoff=0},  {type:SET_PROPERTY,data:_SrcBlend=2}, {type:SET_PROPERTY,data:_DstBlend=3},  {type:SET_PROPERTY,data:_AlphaToMask=0},  {type:SET_PROPERTY,data:_ZWrite=0}, {type:SET_PROPERTY,data:_ZTest=4},   {type:SET_PROPERTY,data:_AlphaPremultiply=0}]}
		}]}]}", Int) = 0
		[HideInInspector] m_mainCategory ("Color & Normals", Float) = 0
		_Color ("Color & Alpha--{reference_property:_ColorThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _ColorThemeIndex ("", Int) = 0
		_MainTex ("Texture--{reference_properties:[_MainTexPan, _MainTexUV]}", 2D) = "white" { }
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _MainTexUV ("UV", Int) = 0
		[HideInInspector][Vector2]_MainTexPan ("Panning", Vector) = (0, 0, 0, 0)
		[Normal]_BumpMap ("Normal Map--{reference_properties:[_BumpMapPan, _BumpMapUV, _BumpScale]}", 2D) = "bump" { }
		[HideInInspector][Vector2]_BumpMapPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _BumpMapUV ("UV", Int) = 0
		[HideInInspector]_BumpScale ("Intensity", Range(0, 10)) = 1
		_ClippingMask ("Alpha Map--{reference_properties:[_ClippingMaskPan, _ClippingMaskUV, _Inverse_Clipping]}", 2D) = "white" { }
		[HideInInspector][Vector2]_ClippingMaskPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _ClippingMaskUV ("UV", Int) = 0
		[HideInInspector][ToggleUI]_Inverse_Clipping ("Invert", Float) = 0
		_Cutoff ("Alpha Cutoff", Range(0, 1.001)) = 0.5
		[HideInInspector] m_start_MainHueShift ("Color Adjust--{reference_property:_MainColorAdjustToggle}", Float) = 0
		[HideInInspector][ThryToggle(COLOR_GRADING_HDR)] _MainColorAdjustToggle ("Adjust Colors", Float) = 0
		[ThryRGBAPacker(R Hue Mask, G Brightness Mask, B Saturation Mask, A Nothing)]_MainColorAdjustTexture ("Mask (Expand)--{reference_properties:[_MainColorAdjustTexturePan, _MainColorAdjustTextureUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_MainColorAdjustTexturePan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _MainColorAdjustTextureUV ("UV", Int) = 0
		_Saturation ("Saturation", Range(-1, 10)) = 0
		_MainBrightness ("Brightness", Range(-1, 1)) = 0
		[ThryToggleUI(true)] _MainHueShiftToggle ("<size=13><b>  Hue Shift</b></size>", Float) = 0
		[ToggleUI]_MainHueShiftReplace ("Hue Replace?--{condition_showS:(_MainHueShiftToggle==1)}", Float) = 1
		_MainHueShift ("Hue Shift--{condition_showS:(_MainHueShiftToggle==1)}", Range(0, 1)) = 0
		_MainHueShiftSpeed ("Hue Shift Speed--{condition_showS:(_MainHueShiftToggle==1)}", Float) = 0
		[Space(4)]
		[ThryToggleUI(true)]_MainHueALCTEnabled ("<size=13><b>  Hue Shift Audio Link</b></size>--{condition_showS:(_MainHueShiftToggle==1 && _EnableAudioLink==1)}", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)]_MainALHueShiftBand ("Band--{condition_showS:(_MainHueShiftToggle==1 && _EnableAudioLink==1 && _MainHueALCTEnabled==1)}", Int) = 0
		[ThryWideEnum(Motion increases as intensity of band increases, 0, Above but Smooth, 1, Motion moves back and forth as a function of intensity, 2, Above but Smoooth, 3, Fixed speed increase when the band is dark Stationary when light, 4, Above but Smooooth, 5, Fixed speed increase when the band is dark Fixed speed decrease when light, 6, Above but Smoooooth, 7)]_MainALHueShiftCTIndex ("Motion Type--{condition_showS:(_MainHueShiftToggle==1 && _EnableAudioLink==1 && _MainHueALCTEnabled==1)}", Int) = 0
		_MainHueALMotionSpeed ("Motion Speed--{condition_showS:(_MainHueShiftToggle==1 && _EnableAudioLink==1 && _MainHueALCTEnabled==1)}", Float) = 1
		[HideInInspector] m_end_MainHueShift ("Hue Shift", Float) = 0
		[HideInInspector] m_start_Alpha ("Alpha Options", Float) = 0
		[ToggleUI]_AlphaForceOpaque ("Force Opaque", Float) = 0
		_AlphaMod ("Alpha Mod", Range(-1, 1)) = 0.0
		[ToggleUI]_AlphaPremultiply ("Alpha Premultiply", Float) = 0
		[Space(4)]
		[ThryToggleUI(true)] _AlphaToCoverage ("<size=13><b>  Alpha To Coverage</b></size>", Float) = 0
		[ToggleUI]_AlphaSharpenedA2C ("Sharpened  A2C--{condition_showS:(_AlphaToCoverage==1)}", Float) = 0
		_AlphaMipScale ("Mip Level Alpha Scale--{condition_showS:(_AlphaToCoverage==1)}", Range(0, 1)) = 0.25
		[Space(4)]
		[ThryToggleUI(true)] _AlphaDithering ("<size=13><b>  Dithering</b></size>", Float) = 0
		_AlphaDitherGradient ("Dither Gradient--{condition_showS:(_AlphaDithering==1)}", Range(0, 1)) = .1
		[Space(4)]
		[ThryToggleUI(true)] _AlphaDistanceFade ("<size=13><b>  Distance Alpha</b></size>", Float) = 0
		[Enum(Object Position, 0, Pixel Position, 1)] _AlphaDistanceFadeType ("Pos To Use--{condition_showS:(_AlphaDistanceFade==1)}", Int) = 1
		_AlphaDistanceFadeMinAlpha ("Min Distance Alpha--{condition_showS:(_AlphaDistanceFade==1)}", Range(0, 1)) = 0
		_AlphaDistanceFadeMaxAlpha ("Max Distance Alpha--{condition_showS:(_AlphaDistanceFade==1)}", Range(0, 1)) = 1
		_AlphaDistanceFadeMin ("Min Distance--{condition_showS:(_AlphaDistanceFade==1)}", Float) = 0
		_AlphaDistanceFadeMax ("Max Distance--{condition_showS:(_AlphaDistanceFade==1)}", Float) = 0
		[Space(4)]
		[ThryToggleUI(true)] _AlphaFresnel ("<size=13><b>  Fresnel Alpha</b></size>", Float) = 0
		_AlphaFresnelAlpha ("Intensity--{condition_showS:(_AlphaFresnel==1)}", Range(0, 1)) = 0
		_AlphaFresnelSharpness ("Sharpness--{condition_showS:(_AlphaFresnel==1)}", Range(0, 1)) = .5
		_AlphaFresnelWidth ("Width--{condition_showS:(_AlphaFresnel==1)}", Range(0, 1)) = .5
		[ToggleUI]_AlphaFresnelInvert ("Invert--{condition_showS:(_AlphaFresnel==1)}", Float) = 0
		[Space(4)]
		[ThryToggleUI(true)] _AlphaAngular ("<size=13><b>  Angular Alpha</b></size>", Float) = 0
		[Enum(Camera Face Model, 0, Model Face Camera, 1, Face Each Other, 2)] _AngleType ("Angle Type--{condition_showS:(_AlphaAngular==1)}", Int) = 0
		[Enum(Model, 0, Vertex, 1)] _AngleCompareTo ("Model or Vert Positon--{condition_showS:(_AlphaAngular==1)}", Int) = 0
		[Vector3]_AngleForwardDirection ("Forward Direction--{condition_showS:(_AlphaAngular==1)}", Vector) = (0, 0, 1)
		_CameraAngleMin ("Camera Angle Min--{condition_showS:(_AlphaAngular==1)}", Range(0, 180)) = 45
		_CameraAngleMax ("Camera Angle Max--{condition_showS:(_AlphaAngular==1)}", Range(0, 180)) = 90
		_ModelAngleMin ("Model Angle Min--{condition_showS:(_AlphaAngular==1)}", Range(0, 180)) = 45
		_ModelAngleMax ("Model Angle Max--{condition_showS:(_AlphaAngular==1)}", Range(0, 180)) = 90
		_AngleMinAlpha ("Min Alpha--{condition_showS:(_AlphaAngular==1)}", Range(0, 1)) = 0
		[Space(4)]
		[ThryToggleUI(true)]_AlphaAudioLinkEnabled ("<size=13><b>  Alpha Audio Link</b></size>--{condition_showS:(_EnableAudioLink==1)}", Float) = 0
		[Vector2]_AlphaAudioLinkAddRange ("Add Range--{ condition_showS:(_AlphaAudioLinkEnabled==1 && _EnableAudioLink==1)}", Vector) = (0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AlphaAudioLinkAddBand ("Add Band--{ condition_showS:(_AlphaAudioLinkEnabled==1 && _EnableAudioLink==1)}", Int) = 0
		[HideInInspector] m_end_Alpha ("Alpha Options", Float) = 0
		[HideInInspector] m_start_DetailOptions ("Details--{reference_property:_DetailEnabled}", Float) = 0
		[HideInInspector][ThryToggle(FINALPASS)]_DetailEnabled ("Enable", Float) = 0
		[ThryRGBAPacker(R Texture Mask, G Normal Mask, B Nothing, A Nothing)]_DetailMask ("Detail Mask (Expand)--{reference_properties:[_DetailMaskPan, _DetailMaskUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_DetailMaskPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _DetailMaskUV ("UV", Int) = 0
		_DetailTint ("Detail Texture Tint--{reference_property:_DetailTintThemeIndex}", Color) = (1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _DetailTintThemeIndex ("", Int) = 0
		_DetailTex ("Detail Texture--{reference_properties:[_DetailTexPan, _DetailTexUV]}", 2D) = "gray" { }
		[HideInInspector][Vector2]_DetailTexPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _DetailTexUV ("UV", Int) = 0
		_DetailTexIntensity ("Detail Tex Intensity", Range(0, 10)) = 1
		_DetailBrightness ("Detail Brightness:", Range(0, 2)) = 1
		[Normal]_DetailNormalMap ("Detail Normal--{reference_properties:[_DetailNormalMapPan, _DetailNormalMapUV, _DetailNormalMapScale]}", 2D) = "bump" { }
		[HideInInspector]_DetailNormalMapScale ("Detail Normal Intensity", Range(0, 10)) = 1
		[HideInInspector][Vector2]_DetailNormalMapPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _DetailNormalMapUV ("UV", Int) = 0
		[HideInInspector] m_end_DetailOptions ("Details", Float) = 0
		[HideInInspector] m_start_vertexManipulation ("Vertex Options--{reference_property:_VertexManipulationsEnabled, button_help:{text:Tutorial,action:{type:URL,data:https://www.youtube.com/watch?v=x728WN50JeA&list=PL4_Gy3VRJSmbXfQSldzUiChgABQsoBlLw},hover:YouTube}}", Float) = 0
		[HideInInspector][ThryToggle(AUTO_EXPOSURE)]_VertexManipulationsEnabled ("Enabled", Float) = 0
		[Vector3]_VertexManipulationLocalTranslation ("Local Translation", Vector) = (0, 0, 0, 1)
		[Vector3]_VertexManipulationLocalRotation ("Local Rotation", Vector) = (0, 0, 0, 1)
		[Vector3]_VertexManipulationLocalRotationSpeed ("Local Rotation Speed", Vector) = (0, 0, 0, 1)
		_VertexManipulationLocalScale ("Local Scale", Vector) = (1, 1, 1, 1)
		[Vector3]_VertexManipulationWorldTranslation ("World Translation", Vector) = (0, 0, 0, 1)
		_VertexManipulationHeight ("Vertex Height", Float) = 0
		_VertexManipulationHeightMask ("Height Map--{reference_properties:[_VertexManipulationHeightMaskPan, _VertexManipulationHeightMaskUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_VertexManipulationHeightMaskPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][Enum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)] _VertexManipulationHeightMaskUV ("UV", Int) = 0
		_VertexManipulationHeightBias ("Mask Bias", Range(0, 1)) = 0
		[ToggleUI]_VertexRoundingEnabled ("Rounding Enabled", Float) = 0
		_VertexRoundingDivision ("Rounding Interval", Float) = 0.02
		[Space(10)]
		[ThryToggleUI(true)]_VertexAudioLinkEnabled ("<size=13><b>  Audio Link</b></size>--{condition_showS:(_EnableAudioLink==1)}", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _VertexLocalTranslationALBand ("Local Translate Band--{ condition_showS:(_EnableAudioLink==1 && _VertexAudioLinkEnabled==1)}", Int) = 0
		[Vector3]_VertexLocalTranslationALMin ("Local Translate Min--{ condition_showS:(_EnableAudioLink==1 && _VertexAudioLinkEnabled==1)}", Vector) = (0,0,0)
		[Vector3]_VertexLocalTranslationALMax ("Local Translate Max--{ condition_showS:(_EnableAudioLink==1 && _VertexAudioLinkEnabled==1)}", Vector) = (0,0,0)
		[Space(10)]
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _VertexLocalRotationALBandX("Rotation Band X--{ condition_showS:(_EnableAudioLink==1 && _VertexAudioLinkEnabled==1)}", Int) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _VertexLocalRotationALBandY ("Rotation Band Y--{ condition_showS:(_EnableAudioLink==1 && _VertexAudioLinkEnabled==1)}", Int) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _VertexLocalRotationALBandZ ("Rotation Band Z--{ condition_showS:(_EnableAudioLink==1 && _VertexAudioLinkEnabled==1)}", Int) = 0
		[Vector3]_VertexLocalRotationAL ("Rotation--{ condition_showS:(_EnableAudioLink==1 && _VertexAudioLinkEnabled==1)}", Vector) = (0,0,0)
		[Space(10)]
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _VertexLocalRotationCTALBandX ("Band X--{ condition_showS:(_EnableAudioLink==1 && _VertexAudioLinkEnabled==1)}", Int) = 0
		[ThryWideEnum(Motion increases as intensity of band increases, 0, Above but Smooth, 1, Motion moves back and forth as a function of intensity, 2, Above but Smoooth, 3, Fixed speed increase when the band is dark Stationary when light, 4, Above but Smooooth, 5, Fixed speed increase when the band is dark Fixed speed decrease when light, 6, Above but Smoooooth, 7)]_VertexLocalRotationCTALTypeX ("Motion Type X--{ condition_showS:(_EnableAudioLink==1 && _VertexAudioLinkEnabled==1)}", Int) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _VertexLocalRotationCTALBandY ("Band Y--{ condition_showS:(_EnableAudioLink==1 && _VertexAudioLinkEnabled==1)}", Int) = 0
		[ThryWideEnum(Motion increases as intensity of band increases, 0, Above but Smooth, 1, Motion moves back and forth as a function of intensity, 2, Above but Smoooth, 3, Fixed speed increase when the band is dark Stationary when light, 4, Above but Smooooth, 5, Fixed speed increase when the band is dark Fixed speed decrease when light, 6, Above but Smoooooth, 7)]_VertexLocalRotationCTALTypeY ("Motion Type Y--{ condition_showS:(_EnableAudioLink==1 && _VertexAudioLinkEnabled==1)}", Int) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _VertexLocalRotationCTALBandZ ("Band Z--{ condition_showS:(_EnableAudioLink==1 && _VertexAudioLinkEnabled==1)}", Int) = 0
		[ThryWideEnum(Motion increases as intensity of band increases, 0, Above but Smooth, 1, Motion moves back and forth as a function of intensity, 2, Above but Smoooth, 3, Fixed speed increase when the band is dark Stationary when light, 4, Above but Smooooth, 5, Fixed speed increase when the band is dark Fixed speed decrease when light, 6, Above but Smoooooth, 7)]_VertexLocalRotationCTALTypeZ ("Motion Type Z--{ condition_showS:(_EnableAudioLink==1 && _VertexAudioLinkEnabled==1)}", Int) = 0
		[Vector3]_VertexLocalRotationCTALSpeed ("Rotation Speed--{ condition_showS:(_EnableAudioLink==1 && _VertexAudioLinkEnabled==1)}", Vector) = (0,0,0)
		[Space(10)]
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _VertexLocalScaleALBand ("Scale Band--{ condition_showS:(_EnableAudioLink==1 && _VertexAudioLinkEnabled==1)}", Int) = 0
		_VertexLocalScaleALMin ("Scale Min--{ condition_showS:(_EnableAudioLink==1 && _VertexAudioLinkEnabled==1)}", Vector) = (0,0,0,0)
		_VertexLocalScaleALMax ("Scale Max--{ condition_showS:(_EnableAudioLink==1 && _VertexAudioLinkEnabled==1)}", Vector) = (0,0,0,0)
		[Space(10)]
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _VertexWorldTranslationALBand ("World Translation Band--{ condition_showS:(_EnableAudioLink==1 && _VertexAudioLinkEnabled==1)}", Int) = 0
		[Vector3]_VertexWorldTranslationALMin ("World Translation Min--{ condition_showS:(_EnableAudioLink==1 && _VertexAudioLinkEnabled==1)}", Vector) = (0,0,0)
		[Vector3]_VertexWorldTranslationALMax ("World Translation Max--{ condition_showS:(_EnableAudioLink==1 && _VertexAudioLinkEnabled==1)}", Vector) = (0,0,0)
		[Space(10)]
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _VertexManipulationHeightBand ("Vertex Height Band--{ condition_showS:(_EnableAudioLink==1 && _VertexAudioLinkEnabled==1)}", Int) = 0
		[Vector2]_VertexManipulationHeightAL ("Vertex Height--{ condition_showS:(_EnableAudioLink==1 && _VertexAudioLinkEnabled==1)}", Vector) = (0,0,0)
		[Space(10)]
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _VertexRoundingRangeBand ("Rounding Band--{ condition_showS:(_EnableAudioLink==1 && _VertexAudioLinkEnabled==1)}", Int) = 0
		[Vector2]_VertexRoundingRangeAL ("Rounding Range--{ condition_showS:(_EnableAudioLink==1 && _VertexAudioLinkEnabled==1)}", Vector) = (0,0,0)
		[HideInInspector] m_end_vertexManipulation ("Vertex Offset", Float) = 0
		[HideInInspector] m_start_MainVertexColors ("Vertex Colors", Float) = 0
		[ToggleUI]_MainVertexColoringLinearSpace ("Linear Colors", Float) = 1
		_MainVertexColoring ("Use Vertex Color", Range(0, 1)) = 0
		_MainUseVertexColorAlpha ("Use Vertex Color Alpha", Range(0, 1)) = 0
		[HideInInspector] m_end_MainVertexColors ("Vertex Colors", Float) = 0
		[HideInInspector] m_start_backFace ("Back Face--{reference_property:_BackFaceEnabled}", Float) = 0
		[HideInInspector][ThryToggle(POI_BACKFACE)]_BackFaceEnabled ("Backface Enabled", Float) = 0
		_BackFaceColor ("Color--{reference_property:_BackFaceColorThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _BackFaceColorThemeIndex ("", Int) = 0
		_BackFaceEmissionStrength ("Emission Strength", Range(0, 20)) = 0
		_BackFaceAlpha ("Alpha", Range(0,1)) = 1
		_BackFaceTexture ("Texture--{reference_properties:[_BackFaceTexturePan, _BackFaceTextureUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_BackFaceTexturePan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_BackFaceTextureUV ("UV#", Int) = 0
		_BackFaceMask ("Mask--{reference_properties:[_BackFaceMaskPan, _BackFaceMaskUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_BackFaceMaskPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_BackFaceMaskUV ("UV#", Int) = 0
		_BackFaceDetailIntensity ("Detail Intensity", Range(0, 5)) = 1
		[ToggleUI]_BackFaceReplaceAlpha ("Replace Alpha", Float) = 0
		_BackFaceEmissionLimiter ("Emission Limiter", Range(0,1)) = 1
		[Space(10)]
		[ThryToggleUI(true)]_BackFaceHueShiftEnabled ("<size=13><b>  Hue Shift</b></size>", Float) = 0
		_BackFaceHueShift ("Hue Shift--{condition_showS:(_BackFaceHueShiftEnabled==1)}", Range(0, 1)) = 0
		_BackFaceHueShiftSpeed ("Hue Shift Speed--{condition_showS:(_BackFaceHueShiftEnabled==1)}", Float) = 0
		[HideInInspector] m_end_backFace ("Back Face", Float) = 0
		[HideInInspector] m_start_RGBMask ("RGBA Color Masking--{reference_property:_RGBMaskEnabled}", Float) = 0
		[HideInInspector][ThryToggle(VIGNETTE)]_RGBMaskEnabled ("RGB Mask Enabled", Float) = 0
		[ToggleUI]_RGBUseVertexColors ("Use Vertex Colors", Float) = 0
		[ToggleUI]_RGBBlendMultiplicative ("Multiplicative?", Float) = 0
		[ThryRGBAPacker(R Mask,G Mask,B Mask,A Mask)]_RGBMask ("Mask--{reference_properties:[_RGBMaskPan, _RGBMaskUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_RGBMaskPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_RGBMaskUV ("UV", int) = 0
		_RedColor ("R Color--{reference_property:_RedColorThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _RedColorThemeIndex ("", Int) = 0
		_RedTexture ("R Texture--{reference_properties:[_RedTexturePan, _RedTextureUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_RedTexturePan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_RedTextureUV ("UV", int) = 0
		_GreenColor ("G Color--{reference_property:_GreenColorThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _GreenColorThemeIndex ("", Int) = 0
		_GreenTexture ("G Texture--{reference_properties:[_GreenTexturePan, _GreenTextureUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_GreenTexturePan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_GreenTextureUV ("UV", int) = 0
		_BlueColor ("B Color--{reference_property:_BlueColorThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _BlueColorThemeIndex ("", Int) = 0
		_BlueTexture ("B Texture--{reference_properties:[_BlueTexturePan, _BlueTextureUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_BlueTexturePan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_BlueTextureUV ("UV", int) = 0
		_AlphaColor ("A Color--{reference_property:_AlphaColorThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _AlphaColorThemeIndex ("", Int) = 0
		_AlphaTexture ("A Texture--{reference_properties:[_AlphaTexturePan, _AlphaTextureUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_AlphaTexturePan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_AlphaTextureUV ("UV", int) = 0
		[ThryToggle(GEOM_TYPE_MESH)]_RgbNormalsEnabled ("Enable Normals", Float) = 0
		[ToggleUI]_RGBNormalBlend ("Blend with Base--{condition_show:{type:PROPERTY_BOOL,data:_RgbNormalsEnabled==1}}", Float) = 0
		[Normal]_RgbNormalR ("R Normal--{reference_properties:[_RgbNormalRPan, _RgbNormalRUV],condition_show:{type:PROPERTY_BOOL,data:_RgbNormalsEnabled==1}}", 2D) = "bump" { }
		[HideInInspector][Vector2]_RgbNormalRPan ("Pan", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_RgbNormalRUV ("UV", int) = 0
		_RgbNormalRScale ("Intensity--{condition_show:{type:PROPERTY_BOOL,data:_RgbNormalsEnabled==1}}", Range(0, 10)) = 0
		[Normal]_RgbNormalG ("G Normal--{reference_properties:[_RgbNormalGPan, _RgbNormalGUV],condition_show:{type:PROPERTY_BOOL,data:_RgbNormalsEnabled==1}}", 2D) = "bump" { }
		[HideInInspector][Vector2]_RgbNormalGPan ("Pan", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_RgbNormalGUV ("UV", int) = 0
		_RgbNormalGScale ("Intensity--{condition_show:{type:PROPERTY_BOOL,data:_RgbNormalsEnabled==1}}", Range(0, 10)) = 0
		[Normal]_RgbNormalB ("B Normal--{reference_properties:[_RgbNormalBPan, _RgbNormalBUV],condition_show:{type:PROPERTY_BOOL,data:_RgbNormalsEnabled==1}}", 2D) = "bump" { }
		[HideInInspector][Vector2]_RgbNormalBPan ("Pan", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_RgbNormalBUV ("UV", int) = 0
		_RgbNormalBScale ("Intensity--{condition_show:{type:PROPERTY_BOOL,data:_RgbNormalsEnabled==1}}", Range(0, 10)) = 0
		[Normal]_RgbNormalA ("A Normal--{reference_properties:[_RgbNormalAPan, _RgbNormalAUV],condition_show:{type:PROPERTY_BOOL,data:_RgbNormalsEnabled==1}}", 2D) = "bump" { }
		[HideInInspector][Vector2]_RgbNormalAPan ("Pan", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_RgbNormalAUV ("UV", int) = 0
		_RgbNormalAScale ("Intensity--{condition_show:{type:PROPERTY_BOOL,data:_RgbNormalsEnabled==1}}", Range(0, 10)) = 0
		[HideInInspector] m_end_RGBMask ("RGB Color Masking", Float) = 0
		[HideInInspector] m_start_DecalSection ("Decals--{button_help:{text:Tutorial,action:{type:URL,data:https://www.youtube.com/watch?v=xHoQVN_F7JE&list=PL4_Gy3VRJSmbXfQSldzUiChgABQsoBlLw},hover:YouTube}}", Float) = 0
		[ThryRGBAPacker(Decal 0 Mask, Decal 1 Mask, Decal 2 Mask, Decal 3 Mask)]_DecalMask ("Decal RGBA Mask--{reference_properties:[_DecalMaskPan, _DecalMaskUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_DecalMaskPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _DecalMaskUV ("UV", Int) = 0
		[ThryToggleUI(true)] _DecalTPSDepthMaskEnabled ("<size=13><b>  TPS Depth Enabled</b></size>", Float) = 0
		_Decal0TPSMaskStrength ("Mask Strength--{condition_showS:(_DecalTPSDepthMaskEnabled==1)}", Range(0, 1)) = 1
		_Decal1TPSMaskStrength ("Mask Strength--{condition_showS:(_DecalTPSDepthMaskEnabled==1)}", Range(0, 1)) = 1
		_Decal2TPSMaskStrength ("Mask Strength--{condition_showS:(_DecalTPSDepthMaskEnabled==1)}", Range(0, 1)) = 1
		_Decal3TPSMaskStrength ("Mask Strength--{condition_showS:(_DecalTPSDepthMaskEnabled==1)}", Range(0, 1)) = 1
		[HideInInspector] m_start_Decal0 ("Decal 0--{reference_property:_DecalEnabled}", Float) = 0
		[HideInInspector][ThryToggle(GEOM_TYPE_BRANCH)]_DecalEnabled ("Enable", Float) = 0
		[Enum(R, 0, G, 1, B, 2, A, 3)] _Decal0MaskChannel ("Mask Channel", Int) = 0
		_DecalColor ("Color--{reference_property:_DecalColorThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _DecalColorThemeIndex ("", Int) = 0
		_DecalEmissionStrength ("Emission Strength", Range(0, 20)) = 0
		_DecalTexture ("Decal--{reference_properties:[_DecalTexturePan, _DecalTextureUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_DecalTexturePan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _DecalTextureUV ("UV", Int) = 0
		[ToggleUI]_DecalTiled ("Tiled?", Float) = 0
		_Decal0Depth ("Depth", Float) = 0
		[Vector2]_DecalScale ("Scale", Vector) = (1, 1, 0, 0)
		_DecalSideOffset ("Side Offset ←→↓↑", Vector) = (0, 0, 0, 0)
		[Vector2]_DecalPosition ("Position", Vector) = (.5, .5, 0, 0)
		_DecalRotation ("Rotation", Range(0, 360)) = 0
		_DecalRotationSpeed ("Rotation Speed", Float) = 0
		[ThryWideEnum(Replace, 0, Darken, 1, Multiply, 2, Color Burn, 3, Linear Burn, 4, Lighten, 5, Screen, 6, Color Dodge, 7, Linear Dodge(Add), 8, Overlay, 9, Soft Lighten, 10, Hard Light, 11, Vivid Light, 12, Linear Light, 13, Pin Light, 14, Hard Mix, 15, Difference, 16, Exclusion, 17, Subtract, 18, Divide, 19)]_DecalBlendType ("Blending", Range(0, 1)) = 0
		_DecalBlendAlpha ("Alpha", Range(0, 1)) = 1
		[ToggleUI]_DecalOverrideAlpha ("Override Alpha", Float) = 0
		[ThryToggleUI(true)]_DecalHueShiftEnabled ("<size=13><b>Hue Shift</b></size>", Float) = 0
		_DecalHueShiftSpeed ("Shift Speed--{condition_showS:(_DecalHueShiftEnabled==1)}", Float) = 0
		_DecalHueShift ("Hue Shift--{condition_showS:(_DecalHueShiftEnabled==1)}", Range(0, 1)) = 0
		_Decal0HueAngleStrength ("Hue Angle Power--{condition_showS:(_DecalHueShiftEnabled==1)}", Float) = 0
		[HideInInspector] m_start_Decal0AudioLink ("Audio Link ♫--{ condition_showS:_EnableAudioLink==1}", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkDecal0ScaleBand ("Scale Band", Int) = 0
		_AudioLinkDecal0Scale ("Scale Mod", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkDecal0SideBand ("Side Band", Int) = 0
		_AudioLinkDecal0SideMin ("Side Mod Min", Vector) = (0, 0, 0, 0)
		_AudioLinkDecal0SideMax ("Side Mod Max", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkDecal0RotationBand ("Rotation Band", Int) = 0
		[Vector2]_AudioLinkDecal0Rotation ("Rotation Mod", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkDecal0AlphaBand ("Alpha Band", Int) = 0
		[Vector2]_AudioLinkDecal0Alpha ("Alpha Mod", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkDecal0EmissionBand ("Emission Band", Int) = 0
		[Vector2]_AudioLinkDecal0Emission ("Emission Mod", Vector) = (0, 0, 0, 0)
		[ToggleUI]_AudioLinkDecalCC0 ("CC Strip", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _DecalRotationCTALBand0 ("Chrono Rotation Band", Int) = 0
		[ThryWideEnum(Motion increases as intensity of band increases, 0, Above but Smooth, 1, Motion moves back and forth as a function of intensity, 2, Above but Smoooth, 3, Fixed speed increase when the band is dark Stationary when light, 4, Above but Smooooth, 5, Fixed speed increase when the band is dark Fixed speed decrease when light, 6, Above but Smoooooth, 7)]_DecalRotationCTALType0 ("Chrono Motion Type", Int) = 0
		_DecalRotationCTALSpeed0 ("Chrono Rotation Speed", Float) = 0
		[HideInInspector] m_end_Decal0AudioLink ("Audio Link", Float) = 0
		[HideInInspector] m_end_Decal0 ("Decal 0", Float) = 0
		[HideInInspector] m_start_Decal1 ("Decal 1--{reference_property:_DecalEnabled1}", Float) = 0
		[HideInInspector][ThryToggle(GEOM_TYPE_BRANCH_DETAIL)]_DecalEnabled1 ("Enable", Float) = 0
		[Enum(R, 0, G, 1, B, 2, A, 3)] _Decal1MaskChannel ("Mask Channel", Int) = 1
		_DecalColor1 ("Color--{reference_property:_DecalColor1ThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _DecalColor1ThemeIndex ("", Int) = 0
		_DecalEmissionStrength1 ("Emission Strength", Range(0, 20)) = 0
		_DecalTexture1 ("Decal--{reference_properties:[_DecalTexture1Pan, _DecalTexture1UV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_DecalTexture1Pan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _DecalTexture1UV ("UV", Int) = 0
		[ToggleUI]_DecalTiled1 ("Tiled?", Float) = 0
		_Decal1Depth ("Depth", Float) = 0
		[Vector2]_DecalScale1 ("Scale", Vector) = (1, 1, 0, 0)
		_DecalSideOffset1 ("Side Offset ←→↓↑", Vector) = (0, 0, 0, 0)
		[Vector2]_DecalPosition1 ("Position", Vector) = (.5, .5, 0, 0)
		_DecalRotation1 ("Rotation", Range(0, 360)) = 0
		_DecalRotationSpeed1 ("Rotation Speed", Float) = 0
		[ThryWideEnum(Replace, 0, Darken, 1, Multiply, 2, Color Burn, 3, Linear Burn, 4, Lighten, 5, Screen, 6, Color Dodge, 7, Linear Dodge(Add), 8, Overlay, 9, Soft Lighten, 10, Hard Light, 11, Vivid Light, 12, Linear Light, 13, Pin Light, 14, Hard Mix, 15, Difference, 16, Exclusion, 17, Subtract, 18, Divide, 19)]_DecalBlendType1 ("Blending", Range(0, 1)) = 0
		_DecalBlendAlpha1 ("Alpha", Range(0, 1)) = 1
		[ToggleUI]_DecalOverrideAlpha1 ("Override Alpha", Float) = 0
		[ThryToggleUI(true)]_DecalHueShiftEnabled1 ("<size=13><b>Hue Shift</b></size>", Float) = 0
		_DecalHueShiftSpeed1 ("Shift Speed--{condition_showS:(_DecalHueShiftEnabled1==1)}", Float) = 0
		_DecalHueShift1 ("Hue Shift--{condition_showS:(_DecalHueShiftEnabled1==1)}", Range(0, 1)) = 0
		_Decal1HueAngleStrength ("Hue Angle Power--{condition_showS:(_DecalHueShiftEnabled1==1)}", Float) = 0
		[HideInInspector] m_start_Decal1AudioLink ("Audio Link ♫--{ condition_showS:_EnableAudioLink==1}", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkDecal1ScaleBand ("Scale Band", Int) = 0
		_AudioLinkDecal1Scale ("Scale Mod", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkDecal1SideBand ("Side Band", Int) = 0
		_AudioLinkDecal1SideMin ("Side Mod Min", Vector) = (0, 0, 0, 0)
		_AudioLinkDecal1SideMax ("Side Mod Max", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkDecal1RotationBand ("Rotation Band", Int) = 0
		[Vector2]_AudioLinkDecal1Rotation ("Rotation Mod", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkDecal1AlphaBand ("Alpha Band", Int) = 0
		[Vector2]_AudioLinkDecal1Alpha ("Alpha Mod", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkDecal1EmissionBand ("Emission Band", Int) = 0
		[Vector2]_AudioLinkDecal1Emission ("Emission Mod", Vector) = (0, 0, 0, 0)
		[ToggleUI]_AudioLinkDecalCC1 ("CC Strip", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _DecalRotationCTALBand1 ("Chrono Rotation Band", Int) = 0
		[ThryWideEnum(Motion increases as intensity of band increases, 0, Above but Smooth, 1, Motion moves back and forth as a function of intensity, 2, Above but Smoooth, 3, Fixed speed increase when the band is dark Stationary when light, 4, Above but Smooooth, 5, Fixed speed increase when the band is dark Fixed speed decrease when light, 6, Above but Smoooooth, 7)]_DecalRotationCTALType1 ("Chrono Motion Type", Int) = 0
		_DecalRotationCTALSpeed1 ("Chrono Rotation Speed", Float) = 0
		[HideInInspector] m_end_Decal1AudioLink ("Audio Link", Float) = 0
		[HideInInspector] m_end_Decal1 ("Decal 0", Float) = 0
		[HideInInspector] m_start_Decal2 ("Decal 2--{reference_property:_DecalEnabled2}", Float) = 0
		[HideInInspector][ThryToggle(GEOM_TYPE_FROND)]_DecalEnabled2 ("Enable", Float) = 0
		[Enum(R, 0, G, 1, B, 2, A, 3)] _Decal2MaskChannel ("Mask Channel", Int) = 2
		_DecalColor2 ("Color--{reference_property:_DecalColor2ThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _DecalColor2ThemeIndex ("", Int) = 0
		_DecalEmissionStrength2 ("Emission Strength", Range(0, 20)) = 0
		_DecalTexture2 ("Decal--{reference_properties:[_DecalTexture2Pan, _DecalTexture2UV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_DecalTexture2Pan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _DecalTexture2UV ("UV", Int) = 0
		[ToggleUI]_DecalTiled2 ("Tiled?", Float) = 0
		_Decal2Depth ("Depth", Float) = 0
		[Vector2]_DecalScale2 ("Scale", Vector) = (1, 1, 0, 0)
		_DecalSideOffset2 ("Side Offset ←→↓↑", Vector) = (0, 0, 0, 0)
		[Vector2]_DecalPosition2 ("Position", Vector) = (.5, .5, 0, 0)
		_DecalRotation2 ("Rotation", Range(0, 360)) = 0
		_DecalRotationSpeed2 ("Rotation Speed", Float) = 0
		[ThryWideEnum(Replace, 0, Darken, 1, Multiply, 2, Color Burn, 3, Linear Burn, 4, Lighten, 5, Screen, 6, Color Dodge, 7, Linear Dodge(Add), 8, Overlay, 9, Soft Lighten, 10, Hard Light, 11, Vivid Light, 12, Linear Light, 13, Pin Light, 14, Hard Mix, 15, Difference, 16, Exclusion, 17, Subtract, 18, Divide, 19)]_DecalBlendType2 ("Blending", Range(0, 1)) = 0
		_DecalBlendAlpha2 ("Alpha", Range(0, 1)) = 1
		[ToggleUI]_DecalOverrideAlpha2 ("Override Alpha", Float) = 0
		[ThryToggleUI(true)]_DecalHueShiftEnabled2 ("<size=13><b>Hue Shift</b></size>", Float) = 0
		_DecalHueShiftSpeed2 ("Shift Speed--{condition_showS:(_DecalHueShiftEnabled2==1)}", Float) = 0
		_DecalHueShift2 ("Hue Shift--{condition_showS:(_DecalHueShiftEnabled2==1)}", Range(0, 1)) = 0
		_Decal2HueAngleStrength ("Hue Angle Power--{condition_showS:(_DecalHueShiftEnabled2==1)}", Float) = 0
		[HideInInspector] m_start_Decal2AudioLink ("Audio Link ♫--{ condition_showS:_EnableAudioLink==1}", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkDecal2ScaleBand ("Scale Band", Int) = 0
		_AudioLinkDecal2Scale ("Scale Mod", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkDecal2SideBand ("Side Band", Int) = 0
		_AudioLinkDecal2SideMin ("Side Mod Min", Vector) = (0, 0, 0, 0)
		_AudioLinkDecal2SideMax ("Side Mod Max", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkDecal2RotationBand ("Rotation Band", Int) = 0
		[Vector2]_AudioLinkDecal2Rotation ("Rotation Mod", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkDecal2AlphaBand ("Alpha Band", Int) = 0
		[Vector2]_AudioLinkDecal2Alpha ("Alpha Mod", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkDecal2EmissionBand ("Emission Band", Int) = 0
		[Vector2]_AudioLinkDecal2Emission ("Emission Mod", Vector) = (0, 0, 0, 0)
		[ToggleUI]_AudioLinkDecalCC2 ("CC Strip", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _DecalRotationCTALBand2 ("Chrono Rotation Band", Int) = 0
		[ThryWideEnum(Motion increases as intensity of band increases, 0, Above but Smooth, 1, Motion moves back and forth as a function of intensity, 2, Above but Smoooth, 3, Fixed speed increase when the band is dark Stationary when light, 4, Above but Smooooth, 5, Fixed speed increase when the band is dark Fixed speed decrease when light, 6, Above but Smoooooth, 7)]_DecalRotationCTALType2 ("Chrono Motion Type", Int) = 0
		_DecalRotationCTALSpeed2 ("Chrono Rotation Speed", Float) = 0
		[HideInInspector] m_end_Decal2AudioLink ("Audio Link", Float) = 0
		[HideInInspector] m_end_Decal2 ("Decal 0", Float) = 0
		[HideInInspector] m_start_Decal3 ("Decal 3--{reference_property:_DecalEnabled3}", Float) = 0
		[HideInInspector][ThryToggle(DEPTH_OF_FIELD_COC_VIEW)]_DecalEnabled3 ("Enable", Float) = 0
		[Enum(R, 0, G, 1, B, 2, A, 3)] _Decal3MaskChannel ("Mask Channel", Int) = 3
		_DecalColor3 ("Color--{reference_property:_DecalColor3ThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _DecalColor3ThemeIndex ("", Int) = 0
		_DecalEmissionStrength3 ("Emission Strength", Range(0, 20)) = 0
		_DecalTexture3 ("Decal--{reference_properties:[_DecalTexture3Pan, _DecalTexture3UV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_DecalTexture3Pan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _DecalTexture3UV ("UV", Int) = 0
		[ToggleUI]_DecalTiled3 ("Tiled?", Float) = 0
		_Decal3Depth ("Depth", Float) = 0
		[Vector2]_DecalScale3 ("Scale", Vector) = (1, 1, 0, 0)
		_DecalSideOffset3 ("Side Offset ←→↓↑", Vector) = (0, 0, 0, 0)
		[Vector2]_DecalPosition3 ("Position", Vector) = (.5, .5, 0, 0)
		_DecalRotation3 ("Rotation", Range(0, 360)) = 0
		_DecalRotationSpeed3 ("Rotation Speed", Float) = 0
		[ThryWideEnum(Replace, 0, Darken, 1, Multiply, 2, Color Burn, 3, Linear Burn, 4, Lighten, 5, Screen, 6, Color Dodge, 7, Linear Dodge(Add), 8, Overlay, 9, Soft Lighten, 10, Hard Light, 11, Vivid Light, 12, Linear Light, 13, Pin Light, 14, Hard Mix, 15, Difference, 16, Exclusion, 17, Subtract, 18, Divide, 19)]_DecalBlendType3 ("Blending", Range(0, 1)) = 0
		_DecalBlendAlpha3 ("Alpha", Range(0, 1)) = 1
		[ToggleUI]_DecalOverrideAlpha3 ("Override Alpha", Float) = 0
		[ThryToggleUI(true)]_DecalHueShiftEnabled3 ("<size=13><b>Hue Shift</b></size>", Float) = 0
		_DecalHueShiftSpeed3 ("Shift Speed--{condition_showS:(_DecalHueShiftEnabled3==1)}", Float) = 0
		_DecalHueShift3 ("Hue Shift--{condition_showS:(_DecalHueShiftEnabled3==1)}", Range(0, 1)) = 0
		_Decal3HueAngleStrength ("Hue Angle Power--{condition_showS:(_DecalHueShiftEnabled3==1)}", Float) = 0
		[HideInInspector] m_start_Decal3AudioLink ("Audio Link ♫--{ condition_showS:_EnableAudioLink==1}", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkDecal3ScaleBand ("Scale Band", Int) = 0
		_AudioLinkDecal3Scale ("Scale Mod", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkDecal3SideBand ("Side Band", Int) = 0
		_AudioLinkDecal3SideMin ("Side Mod Min", Vector) = (0, 0, 0, 0)
		_AudioLinkDecal3SideMax ("Side Mod Max", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkDecal3RotationBand ("Rotation Band", Int) = 0
		[Vector2]_AudioLinkDecal3Rotation ("Rotation Mod", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkDecal3AlphaBand ("Alpha Band", Int) = 0
		[Vector2]_AudioLinkDecal3Alpha ("Alpha Mod", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkDecal3EmissionBand ("Emission Band", Int) = 0
		[Vector2]_AudioLinkDecal3Emission ("Emission Mod", Vector) = (0, 0, 0, 0)
		[ToggleUI]_AudioLinkDecalCC3 ("CC Strip", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _DecalRotationCTALBand3 ("Chrono Rotation Band", Int) = 0
		[ThryWideEnum(Motion increases as intensity of band increases, 0, Above but Smooth, 1, Motion moves back and forth as a function of intensity, 2, Above but Smoooth, 3, Fixed speed increase when the band is dark Stationary when light, 4, Above but Smooooth, 5, Fixed speed increase when the band is dark Fixed speed decrease when light, 6, Above but Smoooooth, 7)]_DecalRotationCTALType3 ("Chrono Motion Type", Int) = 0
		_DecalRotationCTALSpeed3 ("Chrono Rotation Speed", Float) = 0
		[HideInInspector] m_end_Decal3AudioLink ("Audio Link", Float) = 0
		[HideInInspector] m_end_Decal3 ("Decal 0", Float) = 0
		[HideInInspector] m_end_DecalSection ("Decal", Float) = 0
		[HideInInspector] m_start_tps_penetrator("Penetrator--{reference_property:_TPSPenetratorEnabled,tooltip:Enable TPS Penetrator: Requires the TPS Setup Wizard to be run (under Poi/TPS)}", Float) = 0
		[HideInInspector] m_start_pen_autoConfig("Configured By Tool", Float) = 0
		_TPS_PenetratorLength("Length of Penetrator Model--{tooltip:The length from the root of the P to the very tip}", Range(0 , 3)) = 1
		_TPS_PenetratorScale("Scale of Penetrator Model", Vector) = (1,1,1,1)
		[HideInInspector] m_end_pen_autoConfig("TPS", Float) = 0
		[Helpbox(1)]_TPSHelpbox("Penetrator allows your mesh to bend in the direction of an orifice. It is fully compatible with DPS. Requires the TPS Setup Wizard to be run afterwards. Click here to open the setup window.--{onClick:Thry.TPS.TPS_Setup}", Float) = 0
		[HideInInspector][ThryToggle(TPS_Penetrator)]_TPSPenetratorEnabled("Enabled", Float) = 0
		[Space(10)]
		[ThryRichLabel(13)]_TPSBezierHeader("Bezier--{tooltip: Changes how the penetrator bends}", Float) = 0
		_TPS_BezierStart("Bezier Start--{tooltip:Start later down the penetrator}", Range(0,0.3)) = 0.0
		_TPS_BezierSmoothness("Bezier Smoothness--{tooltip:Smoothness of bending}", Range(0.2,0.05)) = 0.09
		[ThryRichLabel(13)]_TPSSqueezeHeader("Squeeze--{tooltip:Penetrator contracts when entering an orifice}", Float) = 0
		_TPS_Squeeze("Squeeze Strength--{tooltip:Percentage penetrator squeezes}", Range(0,1)) = 0.3
		_TPS_SqueezeDistance("Squeeze Distance--{tooltip:Width of the squeezing}", Range(0.01,1)) = 0.2
		[ThryRichLabel(13)]_TPSBuldgeHeader("Buldge--{tooltip: Penetrator expands in front of the orifice}", Float) = 0
		_TPS_Buldge("Buldge--{tooltip:Amount in percentage}", Range(0,3)) = 0.3
		_TPS_BuldgeDistance("Buldge Distance--{tooltip:Width of the buldging}", Range(0.01,1)) = 0.2
		_TPS_BuldgeFalloffDistance("Buldge Falloff--{tooltip:Width of bulding in front of orifice}", Range(0.01,0.5)) = 0.05
		[ThryRichLabel(13)]_TPSPulsingHeader("Pulsing--{tooltip: Penetrator expands in pulses while entering orifice}", Float) = 0
		_TPS_PumpingStrength("Pumping Strength--{tooltip:Amount in percentage}", Range(0,1)) = 0
		_TPS_PumpingSpeed("Pumping Speed--{tooltip:Frequenzy of pulsing}", Range(0,10)) = 0
		_TPS_PumpingWidth("Pumping Width--{tooltip:Width of pulsing}", Range(0.01,1)) = 0.2
		[ThryRichLabel(13)]_TPSIdleHeader("Idle--{tooltip: Changes how the penetrator bends while no orifice is near}", Float) = 0
		_TPS_IdleGravity("Idle Gravity--{tooltip:P hangs down while not penetrating}", Range(0,1)) = 0
		_TPS_IdleSkrinkWidth("Idle Shrink Width--{tooltip:P shrinks while not penetrating}", Range(0,1)) = 1
		_TPS_IdleSkrinkLength("Idle Shrink Length--{tooltip:P shrinks while not penetrating}", Range(0,1)) = 1
		_TPS_IdleMovementStrength("Idle Movement--{tooltip:P swings while not penetrating}", Range(0, 10)) = 0
		[HideInInspector][Toggle]_TPS_VertexColors("Baked Vertex Colors", Float) = 1
		[HideInInspector]_TPS2_BufferedDepth   ("_TPS2_BufferedDepth NL", Float) = 0
		[HideInInspector]_TPS2_BufferedStrength("_TPS2_BufferedStrength NL", Float) = 0
		[HideInInspector] m_end_tps_penetrator("", Float) = 0
		[HideInInspector] m_start_GlobalThemes ("Global Themes", Float) = 0
		[HDR]_GlobalThemeColor0 ("Color 0", Color) = (1, 1, 1, 1)
		[HDR]_GlobalThemeColor1 ("Color 1", Color) = (1, 1, 1, 1)
		[HDR]_GlobalThemeColor2 ("Color 2", Color) = (1, 1, 1, 1)
		[HDR]_GlobalThemeColor3 ("Color 3", Color) = (1, 1, 1, 1)
		[HideInInspector] m_end_GlobalThemes ("Global Themes", Float) = 0
		[HideInInspector] m_lightingCategory ("Shading", Float) = 0
		[HideInInspector] m_start_PoiLightData ("Light Data ", Float) = 0
		_LightingAOMaps ("AO Maps (expand)--{reference_properties:[_LightingAOMapsPan, _LightingAOMapsUV,_LightDataAOStrengthR,_LightDataAOStrengthG,_LightDataAOStrengthB,_LightDataAOStrengthA]}", 2D) = "white" { }
		[HideInInspector][Vector2]_LightingAOMapsPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _LightingAOMapsUV ("UV", Int) = 0
		[HideInInspector]_LightDataAOStrengthR ("R Strength", Range(0, 1)) = 1
		[HideInInspector]_LightDataAOStrengthG ("G Strength", Range(0, 1)) = 0
		[HideInInspector]_LightDataAOStrengthB ("B Strength", Range(0, 1)) = 0
		[HideInInspector]_LightDataAOStrengthA ("A Strength", Range(0, 1)) = 0
		_LightingDetailShadowMaps ("Detail Shadows (expand)--{reference_properties:[_LightingDetailShadowMapsPan, _LightingDetailShadowMapsUV,_LightingDetailShadowStrengthR,_LightingDetailShadowStrengthG,_LightingDetailShadowStrengthB,_LightingDetailShadowStrengthA]}", 2D) = "white" { }
		[HideInInspector][Vector2]_LightingDetailShadowMapsPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _LightingDetailShadowMapsUV ("UV", Int) = 0
		[HideInInspector]_LightingDetailShadowStrengthR ("R Strength", Range(0, 1)) = 1
		[HideInInspector]_LightingDetailShadowStrengthG ("G Strength", Range(0, 1)) = 0
		[HideInInspector]_LightingDetailShadowStrengthB ("B Strength", Range(0, 1)) = 0
		[HideInInspector]_LightingDetailShadowStrengthA ("A Strength", Range(0, 1)) = 0
		_LightingShadowMasks ("Shadow Masks (expand)--{reference_properties:[_LightingShadowMasksPan, _LightingShadowMasksUV,_LightingShadowMaskStrengthR,_LightingShadowMaskStrengthG,_LightingShadowMaskStrengthB,_LightingShadowMaskStrengthA]}", 2D) = "white" { }
		[HideInInspector][Vector2]_LightingShadowMasksPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _LightingShadowMasksUV ("UV", Int) = 0
		[HideInInspector]_LightingShadowMaskStrengthR ("R Strength", Range(0, 1)) = 1
		[HideInInspector]_LightingShadowMaskStrengthG ("G Strength", Range(0, 1)) = 0
		[HideInInspector]_LightingShadowMaskStrengthB ("B Strength", Range(0, 1)) = 0
		[HideInInspector]_LightingShadowMaskStrengthA ("A Strength", Range(0, 1)) = 0
		[Space(15)]
		[ThryHeaderLabel(Base Pass Lighting, 13)]
		[Space(4)]
		[Enum(Poi Custom, 0, Standard, 1, UTS2, 2)] _LightingColorMode ("Light Color Mode", Int) = 0
		[Enum(Poi Custom, 0, Normalized NDotL, 1, Saturated NDotL, 2)] _LightingMapMode ("Light Map Mode", Int) = 0
		[Enum(Poi Custom, 0, Forced Local Direction, 1, Forced World Direction, 2, UTS2, 3)] _LightingDirectionMode ("Light Direction Mode", Int) = 0
		[Vector3]_LightngForcedDirection ("Forced Direction--{condition_showS:(_LightingDirectionMode==1 || _LightingDirectionMode==2)}", Vector) = (0, 0, 0)
		[ToggleUI]_LightingForceColorEnabled ("Force Light Color", Float) = 0
		_LightingForcedColor ("Forced Color--{condition_showS:(_LightingForceColorEnabled==1), reference_property:_LightingForcedColorThemeIndex}", Color) = (1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _LightingForcedColorThemeIndex ("", Int) = 0
		_Unlit_Intensity ("Unlit_Intensity--{condition_showS:(_LightingColorMode==2)}", Range(0.001, 4)) = 1
		[ToggleUI]_LightingCapEnabled ("Limit Brightness", Float) = 1
		_LightingCap ("Max Brightness--{condition_showS:(_LightingCapEnabled==1)}", Range(0, 10)) = 1
		_LightingMinLightBrightness ("Min Brightness", Range(0, 1)) = 0
		_LightingIndirectUsesNormals ("Indirect Uses Normals--{condition_showS:(_LightingColorMode==0)}", Range(0, 1)) = 0
		_LightingCastedShadows ("Receive Casted Shadows", Range(0, 1)) = 0
		_LightingMonochromatic ("Grayscale Lighting?", Range(0, 1)) = 0
		[Space(15)]
		[ThryHeaderLabel(Add Pass Lighting, 13)]
		[Space(4)]
		[ThryToggle(POI_LIGHT_DATA_ADDITIVE_ENABLE)]_LightingAdditiveEnable ("Enable Additive", Float) = 1
		[ThryToggle(POI_LIGHT_DATA_ADDITIVE_DIRECTIONAL_ENABLE)]_DisableDirectionalInAdd ("Ignore Directional--{condition_showS:(_LightingAdditiveEnable==1)}", Float) = 1
		[ToggleUI]_LightingAdditiveLimited ("Limit Brightness?--{condition_showS:(_LightingAdditiveEnable==1)}", Float) = 0
		_LightingAdditiveLimit ("Max Brightness--{ condition_showS:(_LightingAdditiveLimited==1&&_LightingAdditiveEnable==1)}", Range(0, 10)) = 1
		_LightingAdditiveMonochromatic ("Grayscale Lighting?", Range(0, 1)) = 0
		_LightingAdditivePassthrough ("Point Light Passthrough--{condition_showS:(_LightingAdditiveEnable==1)}", Range(0, 1)) = .5
		[Space(15)]
		[ThryHeaderLabel(Vertex Lighting, 13)]
		[Space(4)]
		[ThryToggle(POI_VERTEXLIGHT_ON)]_LightingVertexLightingEnabled ("Enabled", Float) = 1
		[Space(15)]
		[ThryHeaderLabel(Debug Visualization, 13)]
		[Space(4)]
		[ThryToggle(POI_LIGHT_DATA_DEBUG)]_LightDataDebugEnabled ("Debug", Float) = 0
		[ThryWideEnum(Direct Color, 0, Indirect Color, 1, Light Map, 2, Attenuation, 3, N Dot L, 4, Half Dir, 5, Direction, 6, Add Color, 7, Add Attenuation, 8, Add Shadow, 9, Add N Dot L, 10)] _LightingDebugVisualize ("Visualize--{condition_showS:(_LightDataDebugEnabled==1)}", Int) = 0
		[HideInInspector] m_end_PoiLightData ("Light Data", Float) = 0
		[HideInInspector] m_start_PoiShading (" Shading--{reference_property:_ShadingEnabled}", Float) = 0
		[HideInInspector][ThryToggle(VIGNETTE_MASKED)]_ShadingEnabled ("Enable Shading", Float) = 1
		[ThryHeaderLabel(Base Pass Shading, 13)]
		[Space(4)]
		[KeywordEnum(TextureRamp, Multilayer Math, Wrapped, Skin, ShadeMap, Flat, Realistic, Cloth)] _LightingMode ("Lighting Type", Float) = 5
		_LightingShadowColor ("Shadow Tint--{condition_showS:(_LightingMode!=4 && _LightingMode!=1 && _LightingMode!=5)}", Color) = (1, 1, 1)
		[Gradient]_ToonRamp ("Lighting Ramp--{texture:{width:512,height:4,filterMode:Bilinear,wrapMode:Clamp},force_texture_options:true,condition_showS:(_LightingMode==0)}", 2D) = "white" { }
		_ShadowOffset ("Ramp Offset--{condition_showS:(_LightingMode==0)}", Range(-1, 1)) = 0
		_LightingWrappedWrap ("Wrap--{condition_showS:(_LightingMode==2)}", Range(0, 2)) = 0
		_LightingWrappedNormalization ("Normalization--{condition_showS:(_LightingMode==2)}", Range(0, 1)) = 0
		_ShadowColorTex ("Shadow Color--{reference_properties:[_ShadowColorTexPan, _ShadowColorTexUV], condition_showS:(_LightingMode==1)}", 2D) = "black" { }
		[HideInInspector][Vector2]_ShadowColorTexPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _ShadowColorTexUV ("UV", Int) = 0
		_ShadowColor ("Shadow Color--{condition_showS:(_LightingMode==1)}", Color) = (0.7, 0.75, 0.85, 1.0)
		_ShadowBorder ("Border--{condition_showS:(_LightingMode==1)}", Range(0, 1)) = 0.5
		_ShadowBlur ("Blur--{condition_showS:(_LightingMode==1)}", Range(0, 1)) = 0.1
		_Shadow2ndColorTex ("2nd Color--{reference_properties:[_Shadow2ndColorTexPan, _Shadow2ndColorTexUV], condition_showS:(_LightingMode==1)}", 2D) = "black" { }
		[HideInInspector][Vector2]_Shadow2ndColorTexPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _Shadow2ndColorTexUV ("UV", Int) = 0
		_Shadow2ndColor ("2nd Color--{condition_showS:(_LightingMode==1)}", Color) = (0, 0, 0, 0)
		_Shadow2ndBorder ("2nd Border--{condition_showS:(_LightingMode==1)}", Range(0, 1)) = 0.5
		_Shadow2ndBlur ("2nd Blur--{condition_showS:(_LightingMode==1)}", Range(0, 1)) = 0.3
		_Shadow3rdColorTex ("3rd Color--{reference_properties:[_Shadow3rdColorTexPan, _Shadow3rdColorTexUV], condition_showS:(_LightingMode==1)}", 2D) = "black" { }
		[HideInInspector][Vector2]_Shadow3rdColorTexPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _Shadow3rdColorTexUV ("UV", Int) = 0
		_Shadow3rdColor ("3rd Color--{condition_showS:(_LightingMode==1)}", Color) = (0, 0, 0, 0)
		_Shadow3rdBorder ("3rd Border--{condition_showS:(_LightingMode==1)}", Range(0, 1)) = 0.25
		_Shadow3rdBlur ("3rd Blur--{condition_showS:(_LightingMode==1)}", Range(0, 1)) = 0.1
		_ShadowBorderColor ("Border Color--{condition_showS:(_LightingMode==1)}", Color) = (1, 0, 0, 1)
		_ShadowBorderRange ("Border Range--{condition_showS:(_LightingMode==1)}", Range(0, 1)) = 0
		_LightingGradientStart ("Gradient Start--{condition_showS:(_LightingMode==2)}", Range(0, 1)) = 0
		_LightingGradientEnd ("Gradient End--{condition_showS:(_LightingMode==2)}", Range(0, 1)) = .5
		_1st_ShadeColor ("1st ShadeColor--{condition_showS:(_LightingMode==4)}", Color) = (1, 1, 1)
		_1st_ShadeMap ("1st ShadeMap--{reference_properties:[_1st_ShadeMapPan, _1st_ShadeMapUV, _Use_1stShadeMapAlpha_As_ShadowMask, _1stShadeMapMask_Inverse],condition_showS:(_LightingMode==4)}", 2D) = "white" { }
		[HideInInspector][Vector2]_1st_ShadeMapPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _1st_ShadeMapUV ("UV", Int) = 0
		[HideInInspector][ToggleUI]_Use_1stShadeMapAlpha_As_ShadowMask ("1st ShadeMap.a As ShadowMask", Float) = 0
		[HideInInspector][ToggleUI]_1stShadeMapMask_Inverse ("1st ShadeMapMask Inverse", Float) = 0
		[ToggleUI] _Use_BaseAs1st ("Use BaseMap as 1st ShadeMap--{condition_showS:(_LightingMode==4)}", Float) = 0
		_2nd_ShadeColor ("2nd ShadeColor--{condition_showS:(_LightingMode==4)}", Color) = (1, 1, 1, 1)
		_2nd_ShadeMap ("2nd ShadeMap--{reference_properties:[_2nd_ShadeMapPan, _2nd_ShadeMapUV, _Use_2ndShadeMapAlpha_As_ShadowMask, _2ndShadeMapMask_Inverse],condition_showS:(_LightingMode==4)}", 2D) = "white" { }
		[HideInInspector][Vector2]_2nd_ShadeMapPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _2nd_ShadeMapUV ("UV", Int) = 0
		[HideInInspector][ToggleUI]_Use_2ndShadeMapAlpha_As_ShadowMask ("2nd ShadeMap.a As ShadowMask", Float) = 0
		[HideInInspector][ToggleUI]_2ndShadeMapMask_Inverse ("2nd ShadeMapMask Inverse", Float) = 0
		[ToggleUI] _Use_1stAs2nd ("Use 1st ShadeMap as 2nd_ShadeMap--{condition_showS:(_LightingMode==4)}", Float) = 0
		_BaseColor_Step ("BaseColor_Step--{condition_showS:(_LightingMode==4)}", Range(0.01, 1)) = 0.5
		_BaseShade_Feather ("Base/Shade_Feather--{condition_showS:(_LightingMode==4)}", Range(0.0001, 1)) = 0.0001
		_ShadeColor_Step ("ShadeColor_Step--{condition_showS:(_LightingMode==4)}", Range(0, 1)) = 0
		_1st2nd_Shades_Feather ("1st/2nd_Shades_Feather--{condition_showS:(_LightingMode==4)}", Range(0.0001, 1)) = 0.0001
		[Enum(Replace, 0, Multiply, 1)]_ShadingShadeMapBlendType ("Blend Mode--{condition_showS:(_LightingMode==4)}", Int) = 0
		_SkinLUT ("LUT--{condition_showS:(_LightingMode==3)}", 2D) = "white" { }
		_SssScale ("Scale--{condition_showS:(_LightingMode==3)}", Range(0, 1)) = 1
		[HideInInspector]_SssBumpBlur ("Bump Blur--{condition_showS:(_LightingMode==3)}", Range(0, 1)) = 0.7
		[HideInInspector][Vector3]_SssTransmissionAbsorption ("Absorption--{condition_showS:(_LightingMode==3)}", Vector) = (-8, -40, -64, 0)
		[HideInInspector][Vector3]_SssColorBleedAoWeights ("AO Color Bleed--{condition_showS:(_LightingMode==3)}", Vector) = (0.4, 0.15, 0.13, 0)
		[NonModifiableTextureData] [NoScaleOffset] _ClothDFG ("MultiScatter Cloth DFG--{condition_showS:(_LightingMode==7)}", 2D) = "black" { }
		[ThryRGBAPacker(Metallic Map, Cloth Mask, Reflectance, Smoothness)]_ClothMetallicSmoothnessMap ("Maps (Expand)--{reference_properties:[_ClothMetallicSmoothnessMapPan, _ClothMetallicSmoothnessMapUV, _ClothMetallicSmoothnessMapInvert],condition_showS:(_LightingMode==7)}", 2D) = "white" { }
		[HideInInspector][Vector2] _ClothMetallicSmoothnessMapPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ToggleUI] _ClothMetallicSmoothnessMapInvert ("Invert Smoothness", Float) = 0
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _ClothMetallicSmoothnessMapUV ("UV", Int) = 0
		_ClothReflectance ("Reflectance--{condition_showS:(_LightingMode==7)}", Range(0.35, 1)) = 0.5
		_ClothSmoothness ("Smoothness--{condition_showS:(_LightingMode==7)}", Range(0, 1)) = 0.5
		_ShadowStrength ("Shadow Strength--{condition_showS:(_LightingMode<=4)}", Range(0, 1)) = 1
		_LightingIgnoreAmbientColor ("Ignore Ambient Color--{condition_showS:(_LightingMode<=3)}", Range(0, 1)) = 0
		[Space(15)]
		[ThryHeaderLabel(Add Pass Shading, 13)]
		[Space(4)]
		[Enum(Realistic, 0, Toon, 1)] _LightingAdditiveType ("Lighting Type", Int) = 1
		_LightingAdditiveGradientStart ("Gradient Start--{condition_showS:(_LightingAdditiveType==1)}", Range(0, 1)) = 0
		_LightingAdditiveGradientEnd ("Gradient End--{condition_showS:(_LightingAdditiveType==1)}", Range(0, 1)) = .5
		[HideInInspector] m_end_PoiShading ("Shading", Float) = 0
		[HideInInspector] m_start_Aniso (" Anisotropics--{reference_property:_EnableAniso}", Float) = 0
		[HideInInspector][ThryToggle(POI_ANISOTROPICS)]_EnableAniso ("Enable Aniso", Float) = 0
		[ThryRGBAPacker(1, RGB Color, A Mask, 1)]_AnisoColorMap ("Color & Offset--{reference_properties:[_AnisoColorMapPan, _AnisoColorMapUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_AnisoColorMapPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _AnisoColorMapUV ("UV", Int) = 0
		_AnisoUseLightColor ("Mix Light Color", Range(0, 1)) = 1
		_AnisoUseBaseColor ("Mix Base Color", Range(0, 1)) = 0
		_AnisoReplace ("Replace Blending", Range(0, 1)) = 0
		_AnisoAdd ("Add Blending", Range(0, 1)) = 1
		_AnisoHideInShadow ("Hide In Shadow", Range(0, 1)) = 1
		[Space(10)]
		[ThryHeaderLabel(Top Layer, 13)]
		_Aniso0Power ("Power", Range(0, 1)) = 0
		_Aniso0Strength ("Strength", Range(0, 1)) = 1
		_Aniso0Offset ("Offset", Range(-10, 10)) = 0
		_Aniso0OffsetMapStrength ("Map Offset Strength", Range(0, 1)) = 0
		_Aniso0Tint ("Tint--{reference_property:_Aniso0TintIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _Aniso0TintIndex ("", Int) = 0
		[ThryToggleUI(true)] _Aniso0ToonMode ("Toon Mode", Float) = 0
		_Aniso0Edge ("Edge--{condition_showS:(_Aniso0ToonMode==1)}", Range(0, 1)) = .5
		_Aniso0Blur ("Blur--{condition_showS:(_Aniso0ToonMode==1)}", Range(0, 1)) = 0
		[Space(10)]
		[ThryHeaderLabel(Bottom Layer, 13)]
		_Aniso1Power ("Power", Range(0, 1)) = .1
		_Aniso1Strength ("Strength", Range(0, 1)) = 1
		_Aniso1Offset ("Offset", Range(-1, 1)) = 0
		_Aniso1OffsetMapStrength ("Map Offset Strength", Range(0, 1)) = 0
		_Aniso1Tint ("Tint--{reference_property:_Aniso1TintIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _Aniso1TintIndex ("", Int) = 0
		[ThryToggleUI(true)] _Aniso1ToonMode ("Toon Mode", Float) = 0
		_Aniso1Edge ("Edge--{condition_showS:(_Aniso1ToonMode==1)}", Range(0, 1)) = .5
		_Aniso1Blur ("Blur--{condition_showS:(_Aniso1ToonMode==1)}", Range(0, 1)) = 0
		[Space(4)]
		[ThryToggle(POI_ANISOTROPICS_DEBUG)]_AnisoDebugToggle ("Debug", Float) = 0
		[ThryWideEnum(Off, 0, Overall Specular, 1, Specular 0, 2, Specular 1, 3)] _AnisoDebugMode ("Visualize--{condition_showS:(_AnisoDebugToggle==1)}", Int) = 0
		[HideInInspector] m_end_Ansio ("Anisotropics", Float) = 0
		[HideInInspector] m_start_matcap ("Matcap 0--{reference_property:_MatcapEnable}", Float) = 0
		[HideInInspector][ThryToggle(POI_MATCAP0)]_MatcapEnable ("Enable Matcap", Float) = 0
		[ThryWideEnum(UTS Style, 0, Top Pinch, 1, Double Sided, 2)] _MatcapUVMode ("UV Mode", Int) = 1
		_MatcapColor ("Color--{reference_property:_MatcapColorThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _MatcapColorThemeIndex ("", Int) = 0
		[TextureNoSO]_Matcap ("Matcap", 2D) = "white" { }
		_MatcapBorder ("Border", Range(0, .5)) = 0.43
		_MatcapMask ("Mask--{reference_properties:[_MatcapMaskPan, _MatcapMaskUV, _MatcapMaskInvert]}", 2D) = "white" { }
		[HideInInspector][Vector2]_MatcapMaskPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _MatcapMaskUV ("UV", Int) = 0
		[HideInInspector][ToggleUI]_MatcapMaskInvert ("Invert", Float) = 0
		_MatcapEmissionStrength ("Emission Strength", Range(0, 20)) = 0
		_MatcapIntensity ("Intensity", Range(0, 5)) = 1
		_MatcapLightMask ("Hide in Shadow", Range(0, 1)) = 0
		_MatcapReplace ("Replace Blend", Range(0, 1)) = 1
		_MatcapMultiply ("Multiply Blend", Range(0, 1)) = 0
		_MatcapAdd ("Add Blend", Range(0, 1)) = 0
		_MatcapMixed ("Mixed Blend", Range(0, 1)) = 0
		_MatcapAddToLight ("Add To Light", Range(0, 1)) = 0
		_MatcapAlphaOverride ("Override Alpha", Range(0, 1)) = 0
		[Enum(Vertex, 0, Pixel, 1)] _MatcapNormal ("Normal to use", Int) = 1
		[ThryToggle(POI_MATCAP0_CUSTOM_NORMAL, true)] _Matcap0CustomNormal ("<size=13><b>  Custom Normal</b></size>", Float) = 0
		[Normal]_Matcap0NormalMap ("Normal Map--{reference_properties:[_Matcap0NormalMapPan, _Matcap0NormalMapUV, _Matcap0NormalMapScale], condition_showS:(_Matcap0CustomNormal==1)}", 2D) = "bump" { }
		[HideInInspector][Vector2]_Matcap0NormalMapPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _Matcap0NormalMapUV ("UV", Int) = 0
		[HideInInspector]_Matcap0NormalMapScale ("Intensity", Range(0, 10)) = 1
		[ThryToggleUI(true)] _MatcapHueShiftEnabled ("<size=13><b>  Hue Shift</b></size>", Float) = 0
		_MatcapHueShiftSpeed ("Shift Speed--{condition_showS:(_MatcapHueShiftEnabled==1)}", Float) = 0
		_MatcapHueShift ("Hue Shift--{condition_showS:(_MatcapHueShiftEnabled==1)}", Range(0, 1)) = 0
		[ThryToggleUI(true)] _MatcapTPSDepthEnabled ("<size=13><b>  TPS Depth Mask Enabled</b></size>", Float) = 0
		_MatcapTPSMaskStrength ("TPS Mask Strength--{condition_showS:(_MatcapTPSDepthEnabled==1)}", Range(0, 1)) = 1
		[HideInInspector] m_end_matcap ("Matcap--{condition_showS:(_MatcapHueShiftEnabled==1)}", Float) = 0
		[HideInInspector] m_start_Matcap2 ("Matcap 1--{reference_property:_Matcap2Enable}", Float) = 0
		[HideInInspector][ThryToggle(COLOR_GRADING_HDR_3D)]_Matcap2Enable ("Enable Matcap 2", Float) = 0
		[ThryWideEnum(UTS Style, 0, Top Pinch, 1, Double Sided, 2)] _Matcap2UVMode ("UV Mode", Int) = 1
		_Matcap2Color ("Color--{reference_property:_Matcap2ColorThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _Matcap2ColorThemeIndex ("", Int) = 0
		[TextureNoSO]_Matcap2 ("Matcap", 2D) = "white" { }
		_Matcap2Border ("Border", Range(0, .5)) = 0.43
		_Matcap2Mask ("Mask--{reference_properties:[_Matcap2MaskPan, _Matcap2MaskUV, _Matcap2MaskInvert]}", 2D) = "white" { }
		[HideInInspector][Vector2]_Matcap2MaskPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _Matcap2MaskUV ("UV", Int) = 0
		[HideInInspector][ToggleUI]_Matcap2MaskInvert ("Invert", Float) = 0
		_Matcap2EmissionStrength ("Emission Strength", Range(0, 20)) = 0
		_Matcap2Intensity ("Intensity", Range(0, 5)) = 1
		_Matcap2LightMask ("Hide in Shadow", Range(0, 1)) = 0
		_Matcap2Replace ("Replace Blend", Range(0, 1)) = 0
		_Matcap2Multiply ("Multiply Blend", Range(0, 1)) = 0
		_Matcap2Add ("Add Blend", Range(0, 1)) = 0
		_Matcap2Mixed ("Mixed Blend", Range(0, 1)) = 0
		_Matcap2AddToLight ("Add To Light", Range(0, 1)) = 0
		_Matcap2AlphaOverride ("Override Alpha", Range(0, 1)) = 0
		[Enum(Vertex, 0, Pixel, 1)] _Matcap2Normal ("Normal to use", Int) = 1
		[ThryToggle(POI_MATCAP1_CUSTOM_NORMAL, true)] _Matcap1CustomNormal ("<size=13><b>  Custom Normal</b></size>", Float) = 0
		[ThryToggle()]_Matcap1CustomNormal ("Custom Normal", Float) = 0
		[Normal]_Matcap1NormalMap ("Normal Map--{reference_properties:[_Matcap1NormalMapPan, _Matcap1NormalMapUV, _Matcap1NormalMapScale], condition_showS:(_Matcap1CustomNormal==1)}", 2D) = "bump" { }
		[HideInInspector][Vector2]_Matcap1NormalMapPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _Matcap1NormalMapUV ("UV", Int) = 0
		[HideInInspector]_Matcap1NormalMapScale ("Intensity", Range(0, 10)) = 1
		[ThryToggleUI(true)] _Matcap2HueShiftEnabled ("<size=13><b>  Hue Shift</b></size>", Float) = 0
		_Matcap2HueShiftSpeed ("Shift Speed--{condition_showS:(_Matcap2HueShiftEnabled==1)}", Float) = 0
		_Matcap2HueShift ("Hue Shift--{condition_showS:(_Matcap2HueShiftEnabled==1)}", Range(0, 1)) = 0
		[ThryToggleUI(true)] _Matcap2TPSDepthEnabled ("<size=13><b>  TPS Depth Mask Enabled</b></size>", Float) = 0
		_Matcap2TPSMaskStrength ("TPS Mask Strength--{condition_showS:(_Matcap2TPSDepthEnabled==1)}", Range(0, 1)) = 1
		[HideInInspector] m_end_Matcap2 ("Matcap 2--{condition_showS:(_Matcap2HueShiftEnabled==1)}", Float) = 0
		[HideInInspector] m_start_CubeMap ("CubeMap--{reference_property:_CubeMapEnabled}", Float) = 0
		[HideInInspector][ThryToggle(_CUBEMAP)]_CubeMapEnabled ("Enable CubeMap", Float) = 0
		[ThryWideEnum(Skybox, 0, Reflection, 1)] _CubeMapUVMode ("UV Mode", Int) = 1
		_CubeMapColor ("Color--{reference_property:_CubeMapColorThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _CubeMapColorThemeIndex ("", Int) = 0
		[TextureNoSO]_CubeMap ("CubeMap", Cube) = "" { }
		_CubeMapMask ("Mask--{reference_properties:[_CubeMapMaskPan, _CubeMapMaskUV, _CubeMapMaskInvert]}", 2D) = "white" { }
		[HideInInspector][Vector2]_CubeMapMaskPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _CubeMapMaskUV ("UV", Int) = 0
		[HideInInspector][ToggleUI]_CubeMapMaskInvert ("Invert", Float) = 0
		_CubeMapEmissionStrength ("Emission Strength", Range(0, 20)) = 0
		_CubeMapIntensity ("Color Strength", Range(0, 5)) = 1
		_CubeMapLightMask ("Hide in Shadow", Range(0, 1)) = 0
		_CubeMapReplace ("Replace With CubeMap", Range(0, 1)) = 1
		_CubeMapMultiply ("Multiply CubeMap", Range(0, 1)) = 0
		_CubeMapAdd ("Add CubeMap", Range(0, 1)) = 0
		[Enum(Vertex, 0, Pixel, 1)] _CubeMapNormal ("Normal to use", Int) = 1
		[Space(10)]
		[ThryHeaderLabel(Hue Shift, 13)]
		[Space(4)]
		[ToggleUI]_CubeMapHueShiftEnabled ("Enabled", Float) = 0
		_CubeMapHueShiftSpeed ("Shift Speed--{condition_showS:(_CubeMapHueShiftEnabled==1)}", Float) = 0
		_CubeMapHueShift ("Hue Shift--{condition_showS:(_CubeMapHueShiftEnabled==1)}", Range(0, 1)) = 0
		[HideInInspector] m_end_CubeMap ("CubeMap", Float) = 0
		[HideInInspector] m_start_rimLightOptions ("Rim Lighting--{reference_property:_EnableRimLighting}", Float) = 0
		[HideInInspector][ThryToggle(_GLOSSYREFLECTIONS_OFF)]_EnableRimLighting ("Enable Rim Lighting", Float) = 0
		[KeywordEnum(Poiyomi, UTS2)] _RimStyle ("Style", Float) = 0
		_RimTex ("Rim Texture--{reference_properties:[_RimTexPan, _RimTexUV], condition_showS:_RimStyle==0}", 2D) = "white" { }
		[HideInInspector][Vector2]_RimTexPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _RimTexUV ("UV", Int) = 0
		_RimMask ("Rim Mask--{reference_properties:[_RimMaskPan, _RimMaskUV], condition_showS:_RimStyle==0}", 2D) = "white" { }
		[HideInInspector][Vector2]_RimMaskPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _RimMaskUV ("UV", Int) = 0
		_Is_NormalMapToRimLight ("Normal Strength", Range(0, 1)) = 1
		[ToggleUI]_RimLightingInvert ("Invert Rim Lighting--{ condition_showS:_RimStyle==0}", Float) = 0
		_RimLightColor ("Rim Color--{reference_property:_RimLightColorThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _RimLightColorThemeIndex ("", Int) = 0
		_RimWidth ("Rim Width--{ condition_showS:_RimStyle==0}", Range(0, 1)) = 0.8
		_RimSharpness ("Rim Sharpness--{ condition_showS:_RimStyle==0}", Range(0, 1)) = .25
		_RimPower ("Rim Power--{ condition_showS:_RimStyle==0}", Range(0, 10)) = 1
		_RimStrength ("Rim Emission--{ condition_showS:_RimStyle==0}", Range(0, 20)) = 0
		_RimBaseColorMix ("Mix Base Color--{ condition_showS:_RimStyle==0}", Range(0, 1)) = 0
		[ThryWideEnum(Add, 0, Replace, 1, Multiply, 2, Mixed, 3)] _RimBlendMode ("Blend Mode--{ condition_showS:_RimStyle==0}", Int) = 0
		_RimBlendStrength ("Blend Strength--{ condition_showS:_RimStyle==0}", Range(0, 10)) = 1
		_Is_LightColor_RimLight ("Mix Light Color--{ condition_showS:_RimStyle==1}", Range(0, 1)) = 1
		_RimLight_Power ("Rim Power--{ condition_showS:_RimStyle==1}", Range(0, 1)) = 0.1
		_RimLight_InsideMask ("Inside Mask--{ condition_showS:_RimStyle==1}", Range(0.0001, 1)) = 0.0001
		[Toggle(_)] _RimLight_FeatherOff ("Feather Off--{ condition_showS:_RimStyle==1}", Float) = 0
		[ThryToggleUI(true)] _LightDirection_MaskOn ("<size=13><b>  Light Direction Mask</b></size>--{ condition_showS:_RimStyle==1}", Float) = 0
		_Tweak_LightDirection_MaskLevel ("Light Dir Mask Level--{ condition_showS:_LightDirection_MaskOn==1&&_RimStyle==1}", Range(0, 0.5)) = 0
		[ThryToggleUI(true)] _Add_Antipodean_RimLight ("<size=13><b>  Antipodean(Ap) Rim</b></size>--{ condition_showS:_LightDirection_MaskOn==1&&_RimStyle==1}", Float) = 0
		_Is_LightColor_Ap_RimLight ("Ap Light Color Mix--{ condition_showS:_LightDirection_MaskOn==1&&_Add_Antipodean_RimLight==1&&_RimStyle==1}", Range(0, 1)) = 1
		_Ap_RimLightColor ("Ap Color--{reference_property:_RimApColorThemeIndex, condition_showS:_LightDirection_MaskOn==1&&_Add_Antipodean_RimLight==1&&_RimStyle==1}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _RimApColorThemeIndex ("", Int) = 0
		_Ap_RimLight_Power ("Ap Power--{ condition_showS:_LightDirection_MaskOn==1&&_Add_Antipodean_RimLight==1&&_RimStyle==1}", Range(0, 1)) = 0.1
		[Toggle(_)] _Ap_RimLight_FeatherOff ("Ap Feather Off--{ condition_showS:_LightDirection_MaskOn==1&&_Add_Antipodean_RimLight==1&&_RimStyle==1}", Float) = 0
		_Set_RimLightMask ("Set_RimLightMask--{ condition_showS:_LightDirection_MaskOn==1&&_RimStyle==1}", 2D) = "white" { }
		_Tweak_RimLightMaskLevel ("Tweak_RimLightMaskLevel--{ condition_showS:_LightDirection_MaskOn==1&&_RimStyle==1}", Range(-1, 1)) = 0
		[ThryToggleUI(true)] _RimShadowToggle ("<size=13><b>  Light Direction Mask</b></size>--{ condition_showS:_RimStyle==0}", Float) = 0
		[Enum(Shadow Map, 0, Custom, 1)]_RimShadowMaskRampType ("Light Falloff Type--{ condition_showS:_RimStyle==0&&_RimShadowToggle==1}", Int) = 0
		_RimShadowMaskStrength ("Shadow Mask Strength--{ condition_showS:_RimStyle==0&&_RimShadowToggle==1}", Range(0, 1)) = 1
		[MultiSlider]_RimShadowAlpha ("Hide In Shadow--{ condition_showS:_RimStyle==0&&_RimShadowToggle==1&&_RimShadowMaskRampType==1}", Vector) = (0.0, 0.0, 0, 1)
		_RimShadowWidth ("Shrink In Shadow--{ condition_showS:_RimStyle==0&&_RimShadowToggle==1}", Range(0, 1)) = 0
		[ThryToggleUI(true)] _RimHueShiftEnabled ("<size=13><b>  Hue Shift</b></size>", Float) = 0
		_RimHueShiftSpeed ("Shift Speed--{condition_showS:(_RimHueShiftEnabled==1)}", Float) = 0
		_RimHueShift ("Hue Shift--{condition_showS:(_RimHueShiftEnabled==1)}", Range(0, 1)) = 0
		[HideInInspector] m_start_RimAudioLink ("Audio Link ♫--{ condition_showS:_EnableAudioLink==1}", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkRimWidthBand ("Width Add Band", Int) = 0
		[Vector2] _AudioLinkRimWidthAdd ("Width Add (XMin, YMax)", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkRimEmissionBand ("Emission Add Band", Int) = 0
		[Vector2] _AudioLinkRimEmissionAdd ("Emission Add (XMin, YMax)", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkRimBrightnessBand ("Brightness Band", Int) = 0
		[Vector2] _AudioLinkRimBrightnessAdd ("Brightness Add (XMin, YMax)", Vector) = (0, 0, 0, 0)
		[HideInInspector] m_end_RimAudioLink ("Audio Link", Float) = 0
		[HideInInspector] m_end_rimLightOptions ("Rim Lighting", Float) = 0
		[HideInInspector] m_start_depthRimLightOptions ("Depth Rim Lighting--{reference_property:_EnableDepthRimLighting}", Float) = 0
		[HideInInspector][ThryToggle(_POI_DEPTH_RIMLIGHT)]_EnableDepthRimLighting ("", Float) = 0
		[Enum(vertex, 0, pixel, 1)] _DepthRimNormalToUse ("Normal To Use", Int) = 1
		_DepthRimWidth ("Width", Range(0, 1)) = .2
		_DepthRimSharpness ("Sharpness", Range(0, 1)) = .2
		[ToggleUI]_DepthRimHideInShadow ("Hide In Shadow", Float) = 0
		[Space][ThryHeaderLabel(Color and Blending, 13)]
		_DepthRimMixBaseColor ("Use Base Color", Range(0, 1)) = 0
		_DepthRimMixLightColor ("Light Color Mix", Range(0, 1)) = 0
		_DepthRimColor ("Rim Color--{reference_property:_DepthRimColorThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _DepthRimColorThemeIndex ("", Int) = 0
		_DepthRimEmission ("Emission", Range(0, 20)) = 0
		_DepthRimReplace ("Replace", Range(0, 1)) = 0
		_DepthRimAdd ("Add", Range(0, 1)) = 0
		_DepthRimMultiply ("Multiply", Range(0, 1)) = 0
		_DepthRimAdditiveLighting ("Add to Light", Range(0, 1)) = 0
		[HideInInspector] m_end_depthRimLightOptions ("Rim Lighting", Float) = 0
		[HideInInspector] m_start_brdf ("Reflections & Specular--{reference_property:_MochieBRDF}", Float) = 0
		[HideInInspector][ThryToggle(MOCHIE_PBR)]_MochieBRDF ("Enable", Float) = 0
		_MochieReflectionStrength ("Reflection Strength", Range(0, 1)) = 1
		_MochieSpecularStrength ("Specular Strength", Range(0, 1)) = 1
		_MochieMetallicMultiplier ("Metallic", Range(0, 1)) = 0
		_MochieRoughnessMultiplier ("Smoothness", Range(0, 1)) = 1
		_MochieReflectionTint ("Reflection Tint--{reference_property:_MochieReflectionTintThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _MochieReflectionTintThemeIndex ("", Int) = 0
		_MochieSpecularTint ("Specular Tint--{reference_property:_MochieSpecularTintThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _MochieSpecularTintThemeIndex ("", Int) = 0
		[Space(8)]
		[ThryRGBAPacker(R Metallic Map, G Smoothness Map, B Reflection Mask, A Specular Mask)]_MochieMetallicMaps ("Maps [Expand]--{reference_properties:[_MochieMetallicMapsPan, _MochieMetallicMapsUV, _MochieMetallicMapInvert, _MochieRoughnessMapInvert, _MochieReflectionMaskInvert, _MochieSpecularMaskInvert]}", 2D) = "white" { }
		[HideInInspector][Vector2]_MochieMetallicMapsPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_MochieMetallicMapsUV ("UV", Int) = 0
		[HideInInspector][ToggleUI]_MochieMetallicMapInvert ("Invert Metallic", Float) = 0
		[HideInInspector][ToggleUI]_MochieRoughnessMapInvert ("Invert Smoothness", Float) = 0
		[HideInInspector][ToggleUI]_MochieReflectionMaskInvert ("Invert Reflection Mask", Float) = 0
		[HideInInspector][ToggleUI]_MochieSpecularMaskInvert ("Invert Specular Mask", Float) = 0
		[ThryToggleUI(true)]_PBRSplitMaskSample ("<size=13><b>  Split Mask Sampling</b></size>", Float) = 0
		_PBRMaskScaleTiling ("ScaleXY TileZW--{condition_showS:(_PBRSplitMaskSample==1)}", Vector) = (1, 1, 0, 0)
		[ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_MochieMetallicMasksUV ("UV--{condition_showS:(_PBRSplitMaskSample==1)}", Int) = 0
		[Vector2]_MochieMetallicMasksPan ("Panning--{condition_showS:(_PBRSplitMaskSample==1)}", Vector) = (0, 0, 0, 0)
		[ThryToggleUI(true)]_Specular2ndLayer ("<size=13><b>  2nd Specular</b></size>", Float) = 0
		_MochieSpecularStrength2 ("Strength--{condition_showS:(_Specular2ndLayer==1)}", Range(0, 1)) = 1
		_MochieRoughnessMultiplier2 ("Smoothness--{condition_showS:(_Specular2ndLayer==1)}", Range(0, 1)) = 1
		[ThryToggleUI(true)] _BRDFTPSDepthEnabled ("<size=13><b>  TPS Depth Enabled</b></size>", Float) = 0
		_BRDFTPSReflectionMaskStrength ("Reflection Mask Strength--{condition_showS:(_BRDFTPSDepthEnabled==1)}", Range(0,1)) = 1
		_BRDFTPSSpecularMaskStrength ("Specular Mask Strength--{condition_showS:(_BRDFTPSDepthEnabled==1)}", Range(0,1)) = 1
		[ToggleUI]_IgnoreCastedShadows ("Ignore Casted Shadows", Float) = 0
		[Space(8)]
		[ThryTexture][NoScaleOffset]_MochieReflCube ("Fallback Cubemap", Cube) = "" { }
		[ToggleUI]_MochieForceFallback ("Force Fallback", Int) = 0
		[ToggleUI]_MochieLitFallback ("Lit Fallback", Float) = 0
		[ThryToggleUI(true)]_MochieGSAAEnabled ("<size=13><b>  GSAA</b></size>", Float) = 1
		_PoiGSAAVariance ("GSAA Variance", Range(0, 1)) = 0.15
		_PoiGSAAThreshold ("GSAA Threshold", Range(0, 1)) = 0.1
		_RefSpecFresnel ("Fresnel Reflection", Range(0, 1)) = 1
		[HideInInspector] m_end_brdf ("", Float) = 0
		[HideInInspector] m_start_clearCoat ("Clear Coat--{reference_property:_ClearCoatBRDF}", Float) = 0
		[HideInInspector][ThryToggle(POI_CLEARCOAT)]_ClearCoatBRDF ("Enable", Float) = 0
		_ClearCoatStrength ("ClearCoat Strength", Range(0, 1)) = 1
		_ClearCoatSmoothness ("Smoothness", Range(0, 1)) = 1
		_ClearCoatReflectionStrength ("Reflections Strength", Range(0, 1)) = 1
		_ClearCoatSpecularStrength ("Speculavvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvr Strength", Range(0, 1)) = 1
		_ClearCoatReflectionTint ("Reflection Tint--{reference_property:_ClearCoatReflectionTintThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _ClearCoatReflectionTintThemeIndex ("", Int) = 0
		_ClearCoatSpecularTint ("Specular Tint--{reference_property:_ClearCoatSpecularTintThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _ClearCoatSpecularTintThemeIndex ("", Int) = 0
		[Space(8)]
		[ThryRGBAPacker(ClearCoat Mask, Smoothness Map, Reflection Mask, Specular Mask)]_ClearCoatMaps ("Maps [Expand]--{reference_properties:[_ClearCoatMapsPan, _ClearCoatMapsUV, _ClearCoatMaskInvert, _ClearCoatSmoothnessMapInvert, _ClearCoatReflectionMaskInvert, _ClearCoatSpecularMaskInvert]}", 2D) = "white" { }
		[HideInInspector][Vector2]_ClearCoatMapsPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_ClearCoatMapsUV ("UV", Int) = 0
		[HideInInspector][ToggleUI]_ClearCoatMaskInvert ("Invert ClearCoat Mask", Float) = 0
		[HideInInspector][ToggleUI]_ClearCoatSmoothnessMapInvert ("Invert Smoothness", Float) = 0
		[HideInInspector][ToggleUI]_ClearCoatReflectionMaskInvert ("Invert Reflection Mask", Float) = 0
		[HideInInspector][ToggleUI]_ClearCoatSpecularMaskInvert ("Invert Specular Mask", Float) = 0
		[Space(8)]
		[ThryTexture][NoScaleOffset]_ClearCoatFallback ("Fallback Cubemap", Cube) = "" { }
		[ToggleUI]_ClearCoatForceFallback ("Force Fallback", Int) = 0
		[ToggleUI]_ClearCoatLitFallback ("Lit Fallback", Float) = 0
		[ToggleUI]_CCIgnoreCastedShadows ("Ignore Casted Shadows", Float) = 0
		[ThryToggleUI(true)]_ClearCoatGSAAEnabled ("<size=13><b>  GSAA</b></size>", Float) = 1
		_ClearCoatGSAAVariance ("GSAA Variance", Range(0, 1)) = 0.15
		_ClearCoatGSAAThreshold ("GSAA Threshold", Range(0, 1)) = 0.1
		[ThryToggleUI(true)] _ClearCoatTPSDepthMaskEnabled ("<size=13><b>  TPS Depth Enabled</b></size>", Float) = 0
		_ClearCoatTPSMaskStrength ("Mask Strength--{condition_showS:(_ClearCoatTPSDepthMaskEnabled==1)}", Range(0,1)) = 1
		[HideInInspector] m_end_clearCoat ("", Float) = 0
		[HideInInspector] m_start_reflectionRim ("Environmental Rim--{reference_property:_EnableEnvironmentalRim}", Float) = 0
		[HideInInspector][ThryToggle(POI_ENVIRORIM)]_EnableEnvironmentalRim ("Enable", Float) = 0
		_RimEnviroMask ("Mask--{reference_properties:[_RimEnviroMaskPan, _RimEnviroMaskUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_RimEnviroMaskPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_RimEnviroMaskUV ("UV", Int) = 0
		_RimEnviroBlur ("Blur", Range(0, 1)) = 0.7
		_RimEnviroWidth ("Rim Width", Range(0, 1)) = 0.45
		_RimEnviroSharpness ("Rim Sharpness", Range(0, 1)) = 0
		_RimEnviroMinBrightness ("Min Brightness Threshold", Range(0, 2)) = 0
		_RimEnviroIntensity ("Intensity", Range(0, 1)) = 1
		[HideInInspector] m_end_reflectionRim ("", Float) = 0
		[HideInInspector] m_start_stylizedSpec (" Stylized Specular--{reference_property:_StylizedSpecular}", Float) = 0
		[HideInInspector][ThryToggle(POI_STYLIZED_StylizedSpecular)]_StylizedSpecular ("Enable", Float) = 0
		[ThryTexture]_HighColor_Tex ("Specular Map--{reference_properties:[_HighColor_TexPan, _HighColor_TexUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_HighColor_TexPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_HighColor_TexUV ("UV", Int) = 0
		_HighColor ("Tint--{reference_property:_HighColorThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _HighColorThemeIndex ("", Int) = 0
		_Set_HighColorMask ("Mask--{reference_properties:[_Set_HighColorMaskPan, _Set_HighColorMaskUV, _Tweak_HighColorMaskLevel]}", 2D) = "white" { }
		[HideInInspector][Vector2]_Set_HighColorMaskPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_Set_HighColorMaskUV ("UV", Int) = 0
		[HideInInspector]_Tweak_HighColorMaskLevel ("Mask Level", Range(-1, 1)) = 0
		[ThryWideEnum(Toon, 0, Realistic, 1)]_Is_SpecularToHighColor ("Specular Mode", Float) = 0
		[ThryWideEnum(Replace, 0, Add, 1)]_Is_BlendAddToHiColor ("Color Blend Mode", Int) = 0
		_StylizedSpecularStrength ("Strength", Float) = 1
		[ToggleUI] _UseLightColor ("Use Light Color", Float) = 1
		[ToggleUI]_SSIgnoreCastedShadows ("Ignore Casted Shadows", Float) = 0
		[Space(8)]
		[ThryHeaderLabel(Layer 1, 13)]
		_HighColor_Power ("Size", Range(0, 1)) = 0.2
		_StylizedSpecularFeather ("Feather--{condition_showS:(_Is_SpecularToHighColor==0)}", Range(0, 1)) = 0
		_Layer1Strength ("Strength", Range(0, 1)) = 1
		[Space(8)]
		[ThryHeaderLabel(Layer 2, 13)]
		_Layer2Size ("Size", Range(0, 1)) = 0
		_StylizedSpecular2Feather ("Feather--{condition_showS:(_Is_SpecularToHighColor==0)}", Range(0, 1)) = 0
		_Layer2Strength ("Strength", Range(0, 1)) = 0
		[HideInInspector] m_end_stylizedSpec ("", Float) = 0
		[HideInInspector] m_specialFXCategory ("Special FX", Float) = 0
		[HideInInspector] m_start_udimdiscardOptions ("UDIM Discard--{reference_property:_EnableUDIMDiscardOptions}", Float) = 0
		[HideInInspector][ThryToggle(POI_UDIMDISCARD)]_EnableUDIMDiscardOptions ("Enable UDIM Discard Options", Float) = 0
		[ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3)]_UDIMDiscardUV ("Discard UV", Int) = 0
		[Enum(Vertex, 0, Pixel, 1)] _UDIMDiscardMode ("Discard Mode", Int) = 1
		[Vector4Toggles]_UDIMDiscardRow3 ("y = 3", Vector) = (1,1,1,1)
		[Vector4Toggles]_UDIMDiscardRow2 ("y = 2", Vector) = (1,1,1,1)
		[Vector4Toggles]_UDIMDiscardRow1 ("y = 1", Vector) = (1,1,1,1)
		[Vector4Toggles]_UDIMDiscardRow0 ("y = 0", Vector) = (1,1,1,1)
		[HideInInspector] m_end_udimdiscardOptions ("UDIM Discard", Float) = 0
		[HideInInspector] m_start_dissolve ("Dissolve--{reference_property:_EnableDissolve}", Float) = 0
		[HideInInspector][ThryToggle(DISTORT)]_EnableDissolve ("Enable Dissolve", Float) = 0
		[Enum(Basic, 1, Point2Point, 2)] _DissolveType ("Dissolve Type", Int) = 1
		_DissolveEdgeWidth ("Edge Width", Range(0, .5)) = 0.025
		_DissolveEdgeHardness ("Edge Hardness", Range(0, 1)) = 0.5
		_DissolveEdgeColor ("Edge Color--{reference_property:_DissolveEdgeColorThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _DissolveEdgeColorThemeIndex ("", Int) = 0
		[Gradient]_DissolveEdgeGradient ("Edge Gradient", 2D) = "white" { }
		_DissolveEdgeEmission ("Edge Emission", Range(0, 20)) = 0
		_DissolveTextureColor ("Dissolved Color--{reference_property:_DissolveTextureColorThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _DissolveTextureColorThemeIndex ("", Int) = 0
		_DissolveToTexture ("Dissolved Texture--{reference_properties:[_DissolveToTexturePan, _DissolveToTextureUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_DissolveToTexturePan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _DissolveToTextureUV ("UV", Int) = 0
		_DissolveToEmissionStrength ("Dissolved Emission Strength", Range(0, 20)) = 0
		_DissolveNoiseTexture ("Dissolve Gradient--{reference_properties:[_DissolveNoiseTexturePan, _DissolveNoiseTextureUV, _DissolveInvertNoise]}", 2D) = "white" { }
		[HideInInspector][Vector2]_DissolveNoiseTexturePan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _DissolveNoiseTextureUV ("UV", Int) = 0
		[HideInInspector][ToggleUI]_DissolveInvertNoise ("Invert?", Float) = 0
		_DissolveDetailNoise ("Dissolve Noise--{reference_properties:[_DissolveDetailNoisePan, _DissolveDetailNoiseUV, _DissolveInvertDetailNoise]}", 2D) = "black" { }
		[HideInInspector][Vector2]_DissolveDetailNoisePan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _DissolveDetailNoiseUV ("UV", Int) = 0
		[HideInInspector][ToggleUI]_DissolveInvertDetailNoise ("Invert?", Float) = 0
		_DissolveDetailStrength ("Dissolve Detail Strength", Range(0, 1)) = 0.1
		_DissolveAlpha ("Dissolve Alpha", Range(0, 1)) = 0
		_DissolveMask ("Dissolve Mask--{reference_properties:[_DissolveMaskPan, _DissolveMaskUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_DissolveMaskPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _DissolveMaskUV ("UV", Int) = 0
		[ToggleUI]_DissolveUseVertexColors ("VertexColor.g Mask", Float) = 0
		[HideInInspector][ToggleUI]_DissolveMaskInvert ("Invert?", Float) = 0
		_ContinuousDissolve ("Continuous Dissolve Speed", Float) = 0
		[Space(10)]
		[ThryToggleUI(true)] _EnableDissolveAudioLink ("<size=13><b>  Audio Link</b></size>--{ condition_showS:_EnableAudioLink==1}", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkDissolveAlphaBand ("Dissolve Alpha Band--{ condition_showS:(_EnableDissolveAudioLink==1 && _EnableAudioLink==1)}", Int) = 0
		[Vector2]_AudioLinkDissolveAlpha ("Dissolve Alpha Mod--{ condition_showS:(_EnableDissolveAudioLink==1 && _EnableAudioLink==1)}", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkDissolveDetailBand ("Dissolve Detail Band--{ condition_showS:(_EnableDissolveAudioLink==1 && _EnableAudioLink==1)}", Int) = 0
		[Vector2]_AudioLinkDissolveDetail ("Dissolve Detail Mod--{ condition_showS:(_EnableDissolveAudioLink==1 && _EnableAudioLink==1)}", Vector) = (0, 0, 0, 0)
		[HideInInspector] m_start_pointToPoint ("point to point--{condition_showS:(_DissolveType==2)}", Float) = 0
		[Enum(Local, 0, World, 1, Vertex Colors, 2)] _DissolveP2PWorldLocal ("World/Local", Int) = 0
		_DissolveP2PEdgeLength ("Edge Length", Float) = 0.1
		[Vector3]_DissolveStartPoint ("Start Point", Vector) = (0, -1, 0, 0)
		[Vector3]_DissolveEndPoint ("End Point", Vector) = (0, 1, 0, 0)
		[HideInInspector] m_end_pointToPoint ("Point To Point", Float) = 0
		[HideInInspector] m_start_dissolveHueShift ("Hue Shift--{reference_property:_DissolveHueShiftEnabled}", Float) = 0
		[HideInInspector][ToggleUI]_DissolveHueShiftEnabled ("Dissolved Enabled", Float) = 0
		_DissolveHueShiftSpeed ("Dissolved Speed", Float) = 0
		_DissolveHueShift ("Dissolved Shift", Range(0, 1)) = 0
		[ToggleUI]_DissolveEdgeHueShiftEnabled ("Edge Enabled", Float) = 0
		_DissolveEdgeHueShiftSpeed ("Edge Speed", Float) = 0
		_DissolveEdgeHueShift ("Edge Shift", Range(0, 1)) = 0
		[HideInInspector] m_end_dissolveHueShift ("Hue Shift", Float) = 0
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
		[HideInInspector] m_start_flipBook ("Flipbook--{reference_property:_EnableFlipbook}", Float) = 0
		[HideInInspector][ThryToggle(_SUNDISK_HIGH_QUALITY)]_EnableFlipbook ("Enable Flipbook", Float) = 0
		[ToggleUI]_FlipbookAlphaControlsFinalAlpha ("Flipbook Controls Alpha?", Float) = 0
		[ToggleUI]_FlipbookIntensityControlsAlpha ("Intensity Controls Alpha?", Float) = 0
		[ToggleUI]_FlipbookColorReplaces ("Color Replaces Flipbook", Float) = 0
		[TextureArray]_FlipbookTexArray ("Texture Array--{reference_properties:[_FlipbookTexArrayPan, _FlipbookTexArrayUV]}", 2DArray) = "" { }
		[HideInInspector][Vector2]_FlipbookTexArrayPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _FlipbookTexArrayUV ("UV", Int) = 0
		_FlipbookMask ("Mask--{reference_properties:[_FlipbookMaskPan, _FlipbookMaskUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_FlipbookMaskPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _FlipbookMaskUV ("UV", Int) = 0
		_FlipbookColor ("Color & alpha--{reference_property:_FlipbookColorThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _FlipbookColorThemeIndex ("", Int) = 0
		_FlipbookTotalFrames ("Total Frames", Float) = 1
		_FlipbookFPS ("FPS", Float) = 30.0
		_FlipbookScaleOffset ("Scale | Offset", Vector) = (1, 1, 0, 0)
		_FlipbookSideOffset ("Side Offset ←→↓↑", Vector) = (0, 0, 0, 0)
		[ToggleUI]_FlipbookTiled ("Tiled?", Float) = 0
		_FlipbookEmissionStrength ("Emission Strength", Range(0, 20)) = 0
		_FlipbookRotation ("Rotation", Range(0, 360)) = 0
		_FlipbookRotationSpeed ("Rotation Speed", Float) = 0
		_FlipbookReplace ("Replace", Range(0, 1)) = 1
		_FlipbookMultiply ("Multiply", Range(0, 1)) = 0
		_FlipbookAdd ("Add", Range(0, 1)) = 0
		[ThryToggleUI(true)]_FlipbookManualFrameControl ("<size=13><b>  Manual Frame Control</b></size>", Float) = 0
		_FlipbookCurrentFrame ("Current Frame--{ condition_showS:_FlipbookManualFrameControl==1}", Float) = 0
		[ThryToggleUI(true)]_FlipbookCrossfadeEnabled ("<size=13><b>  Crossfade</b></size>", Float) = 0
		[MultiSlider]_FlipbookCrossfadeRange ("Fade Range--{ condition_showS:_FlipbookCrossfadeEnabled==1}", Vector) = (0.75, 1, 0, 1)
		[ThryToggleUI(true)]_FlipbookHueShiftEnabled ("<size=13><b>  Hue Shift</b></size>", Float) = 0
		_FlipbookHueShiftSpeed ("Shift Speed--{ condition_showS:_FlipbookHueShiftEnabled==1}", Float) = 0
		_FlipbookHueShift ("Hue Shift--{ condition_showS:_FlipbookHueShiftEnabled==1}", Range(0, 1)) = 0
		[HideInInspector] m_start_FlipbookAudioLink ("Audio Link ♫--{ condition_showS:_EnableAudioLink==1}", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkFlipbookScaleBand ("Scale Band", Int) = 0
		_AudioLinkFlipbookScale ("Scale Mod", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkFlipbookAlphaBand ("Alpha Band", Int) = 0
		[Vector2]_AudioLinkFlipbookAlpha ("Alpha Mod", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkFlipbookEmissionBand ("Emission Band", Int) = 0
		[Vector2]_AudioLinkFlipbookEmission ("Emission Mod", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkFlipbookFrameBand ("Frame Band", Int) = 0
		[Vector2]_AudioLinkFlipbookFrame ("Frame control", Vector) = (0, 0, 0, 0)
		[ToggleUI]_FlipbookChronotensityEnabled ("Chronotensity?", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _FlipbookChronotensityBand ("Chrono Band--{ condition_showS:_FlipbookChronotensityEnabled==1}", Int) = 0
		[ThryWideEnum(Motion increases as intensity of band increases, 0, Above but Smooth, 1, Motion moves back and forth as a function of intensity, 2, Above but Smoooth, 3, Fixed speed increase when the band is dark Stationary when light, 4, Above but Smooooth, 5, Fixed speed increase when the band is dark Fixed speed decrease when light, 6, Above but Smoooooth, 7)]_FlipbookChronoType ("Chrono Type--{ condition_showS:_FlipbookChronotensityEnabled==1}", Int) = 0
		_FlipbookChronotensitySpeed ("Chrono Speed--{ condition_showS:_FlipbookChronotensityEnabled==1}", Float) = 0
		[HideInInspector] m_end_FlipbookAudioLink ("Audio Link", Float) = 0
		[HideInInspector] m_end_flipBook ("Flipbook", Float) = 0
		[HideInInspector] m_start_emissions ("Emissions", Float) = 0
		[HideInInspector] m_start_emissionOptions ("Emission 0--{reference_property:_EnableEmission}", Float) = 0
		[HideInInspector][ThryToggle(_EMISSION)]_EnableEmission ("Enable Emission", Float) = 0
		[ToggleUI]_EmissionReplace0 ("Replace Base Color", Float) = 0
		[HDR]_EmissionColor ("Emission Color--{reference_property:_EmissionColorThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _EmissionColorThemeIndex ("", Int) = 0
		[Gradient]_EmissionMap ("Emission Map--{reference_properties:[_EmissionMapPan, _EmissionMapUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_EmissionMapPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _EmissionMapUV ("UV", Int) = 0
		[ToggleUI]_EmissionBaseColorAsMap ("Base Color as Map?", Float) = 0
		_EmissionMask ("Emission Mask--{reference_properties:[_EmissionMaskPan, _EmissionMaskUV, _EmissionMaskInvert]}", 2D) = "white" { }
		[HideInInspector][Vector2]_EmissionMaskPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _EmissionMaskUV ("UV", Int) = 0
		[ToggleUI]_EmissionMaskInvert ("Invert", Float) = 0
		_EmissionStrength ("Emission Strength", Range(0, 20)) = 0
		[Space(4)]
		[ThryToggleUI(true)]_EmissionHueShiftEnabled ("<size=13><b>  Hue Shift</b></size>", Float) = 0
		_EmissionHueShift ("Hue Shift--{condition_showS:(_EmissionHueShiftEnabled==1)}", Range(0, 1)) = 0
		_EmissionHueShiftSpeed ("Hue Shift Speed--{condition_showS:(_EmissionHueShiftEnabled==1)}", Float) = 0
		[Space(4)]
		[ThryToggleUI(true)]_EmissionCenterOutEnabled ("<size=13><b>  Center Out</b></size>", Float) = 0
		_EmissionCenterOutSpeed ("Flow Speed--{condition_showS:(_EmissionCenterOutEnabled==1)}", Float) = 5
		[Space(4)]
		[ThryToggleUI(true)]_EnableGITDEmission ("<size=13><b>  Light Based</b></size>", Float) = 0
		[Enum(World, 0, Mesh, 1)] _GITDEWorldOrMesh ("Lighting Type--{condition_showS:(_EnableGITDEmission==1)}", Int) = 0
		_GITDEMinEmissionMultiplier ("Min Emission Multiplier--{condition_showS:(_EnableGITDEmission==1)}", Range(0, 1)) = 1
		_GITDEMaxEmissionMultiplier ("Max Emission Multiplier--{condition_showS:(_EnableGITDEmission==1)}", Range(0, 1)) = 0
		_GITDEMinLight ("Min Lighting--{condition_showS:(_EnableGITDEmission==1)}", Range(0, 1)) = 0
		_GITDEMaxLight ("Max Lighting--{condition_showS:(_EnableGITDEmission==1)}", Range(0, 1)) = 1
		[Space(4)]
		[ThryToggleUI(true)]_EmissionBlinkingEnabled ("<size=13><b>  Blinking</b></size>", Float) = 0
		_EmissiveBlink_Min ("Emissive Blink Min--{condition_showS:(_EmissionBlinkingEnabled==1)}", Float) = 0
		_EmissiveBlink_Max ("Emissive Blink Max--{condition_showS:(_EmissionBlinkingEnabled==1)}", Float) = 1
		_EmissiveBlink_Velocity ("Emissive Blink Velocity--{condition_showS:(_EmissionBlinkingEnabled==1)}", Float) = 4
		_EmissionBlinkingOffset ("Offset--{condition_showS:(_EmissionBlinkingEnabled==1)}", Float) = 0
		[Space(4)]
		[ThryToggleUI(true)] _ScrollingEmission ("<size=13><b>  Scrolling</b></size>", Float) = 0
		[ToggleUI]_EmissionScrollingUseCurve ("Use Curve--{condition_showS:(_ScrollingEmission==1)}", float) = 0
		[Curve]_EmissionScrollingCurve ("Curve--{condition_showS:(_ScrollingEmission==1&&_EmissionScrollingUseCurve==1)}", 2D) = "white" { }
		[ToggleUI]_EmissionScrollingVertexColor ("VColor as position--{condition_showS:(_ScrollingEmission==1)}", float) = 0
		_EmissiveScroll_Direction ("Direction--{condition_showS:(_ScrollingEmission==1)}", Vector) = (0, -10, 0, 0)
		_EmissiveScroll_Width ("Width--{condition_showS:(_ScrollingEmission==1)}", Float) = 10
		_EmissiveScroll_Velocity ("Velocity--{condition_showS:(_ScrollingEmission==1)}", Float) = 10
		_EmissiveScroll_Interval ("Interval--{condition_showS:(_ScrollingEmission==1)}", Float) = 20
		_EmissionScrollingOffset ("Offset--{condition_showS:(_ScrollingEmission==1)}", Float) = 0
		[Space(4)]
		[ThryToggleUI(true)] _EmissionAL0Enabled ("<size=13><b>  Audio Link</b></size>--{ condition_showS:_EnableAudioLink==1}", Float) = 0
		[Vector2]_EmissionAL0StrengthMod ("Emission Strength Add--{ condition_showS:(_EmissionAL0Enabled==1 && _EnableAudioLink==1)}", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _EmissionAL0StrengthBand ("Emission Add Band--{ condition_showS:(_EmissionAL0Enabled==1 && _EnableAudioLink==1)}", Int) = 0
		[Vector2] _AudioLinkEmission0CenterOut ("Center Out--{ condition_showS:(_EmissionAL0Enabled==1 && _EnableAudioLink==1)}", Vector) = (0, 0, 0, 0)
		_AudioLinkEmission0CenterOutSize ("Intensity Threshold--{ condition_showS:(_EmissionAL0Enabled==1 && _EnableAudioLink==1)}", Range(0, 1)) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkEmission0CenterOutBand ("Center Out Band--{ condition_showS:(_EmissionAL0Enabled==1 && _EnableAudioLink==1)}", Int) = 0
		[HideInInspector] m_end_emissionOptions ("", Float) = 0
		[HideInInspector] m_start_emission1Options ("Emission 1--{reference_property:_EnableEmission1}", Float) = 0
		[HideInInspector][ThryToggle(POI_EMISSION_1)]_EnableEmission1 ("Enable Emission 2", Float) = 0
		[ToggleUI]_EmissionReplace1 ("Replace Base Color", Float) = 0
		[HDR]_EmissionColor1 ("Emission Color--{reference_property:_EmissionColor1ThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _EmissionColor1ThemeIndex ("", Int) = 0
		[Gradient]_EmissionMap1 ("Emission Map--{reference_properties:[_EmissionMap1Pan, _EmissionMap1UV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_EmissionMap1Pan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _EmissionMap1UV ("UV", Int) = 0
		[ToggleUI]_EmissionBaseColorAsMap1 ("Base Color as Map?", Float) = 0
		_EmissionMask1 ("Emission Mask--{reference_properties:[_EmissionMask1Pan, _EmissionMask1UV, _EmissionMaskInvert1]}", 2D) = "white" { }
		[HideInInspector][Vector2]_EmissionMask1Pan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _EmissionMask1UV ("UV", Int) = 0
		[ToggleUI]_EmissionMaskInvert1 ("Invert", Float) = 0
		_EmissionStrength1 ("Emission Strength", Range(0, 20)) = 0
		[Space(4)]
		[ThryToggleUI(true)]_EmissionHueShiftEnabled1 ("<size=13><b>  Hue Shift</b></size>", Float) = 0
		_EmissionHueShift1 ("Hue Shift--{condition_showS:(_EmissionHueShiftEnabled1==1)}", Range(0, 1)) = 0
		_EmissionHueShiftSpeed1 ("Hue Shift Speed--{condition_showS:(_EmissionHueShiftEnabled1==1)}", Float) = 0
		[Space(4)]
		[ThryToggleUI(true)]_EmissionCenterOutEnabled1 ("<size=13><b>  Center Out</b></size>", Float) = 0
		_EmissionCenterOutSpeed1 ("Flow Speed--{condition_showS:(_EmissionCenterOutEnabled1==1)}", Float) = 5
		[Space(4)]
		[ThryToggleUI(true)]_EnableGITDEmission1 ("<size=13><b>  Light Based</b></size>", Float) = 0
		[Enum(World, 0, Mesh, 1)] _GITDEWorldOrMesh1 ("Lighting Type--{condition_showS:(_EnableGITDEmission1==1)}", Int) = 0
		_GITDEMinEmissionMultiplier1 ("Min Emission Multiplier--{condition_showS:(_EnableGITDEmission1==1)}", Range(0, 1)) = 1
		_GITDEMaxEmissionMultiplier1 ("Max Emission Multiplier--{condition_showS:(_EnableGITDEmission1==1)}", Range(0, 1)) = 0
		_GITDEMinLight1 ("Min Lighting--{condition_showS:(_EnableGITDEmission1==1)}", Range(0, 1)) = 0
		_GITDEMaxLight1 ("Max Lighting--{condition_showS:(_EnableGITDEmission1==1)}", Range(0, 1)) = 1
		[Space(4)]
		[ThryToggleUI(true)]_EmissionBlinkingEnabled1 ("<size=13><b>  Blinking</b></size>", Float) = 0
		_EmissiveBlink_Min1 ("Emissive Blink Min--{condition_showS:(_EmissionBlinkingEnabled1==1)}", Float) = 0
		_EmissiveBlink_Max1 ("Emissive Blink Max--{condition_showS:(_EmissionBlinkingEnabled1==1)}", Float) = 1
		_EmissiveBlink_Velocity1 ("Emissive Blink Velocity--{condition_showS:(_EmissionBlinkingEnabled1==1)}", Float) = 4
		_EmissionBlinkingOffset1 ("Offset--{condition_showS:(_EmissionBlinkingEnabled1==1)}", Float) = 0
		[Space(4)]
		[ThryToggleUI(true)] _ScrollingEmission1 ("<size=13><b>  Scrolling</b></size>", Float) = 0
		[ToggleUI]_EmissionScrollingUseCurve1 ("Use Curve--{condition_showS:(_ScrollingEmission1==1)}", float) = 0
		[Curve]_EmissionScrollingCurve1 ("Curve--{condition_showS:(_ScrollingEmission1==1&&_EmissionScrollingUseCurve1==1)}", 2D) = "white" { }
		[ToggleUI]_EmissionScrollingVertexColor1 ("VColor as position--{condition_showS:(_ScrollingEmission1==1)}", float) = 0
		_EmissiveScroll_Direction1 ("Direction--{condition_showS:(_ScrollingEmission1==1)}", Vector) = (0, -10, 0, 0)
		_EmissiveScroll_Width1 ("Width--{condition_showS:(_ScrollingEmission1==1)}", Float) = 10
		_EmissiveScroll_Velocity1 ("Velocity--{condition_showS:(_ScrollingEmission1==1)}", Float) = 10
		_EmissiveScroll_Interval1 ("Interval--{condition_showS:(_ScrollingEmission1==1)}", Float) = 20
		_EmissionScrollingOffset1 ("Offset--{condition_showS:(_ScrollingEmission1==1)}", Float) = 0
		[Space(4)]
		[ThryToggleUI(true)] _EmissionAL1Enabled ("<size=13><b>  Audio Link</b></size>--{ condition_showS:_EnableAudioLink==1}", Float) = 0
		[Vector2]_EmissionAL1StrengthMod ("Emission Strength Add--{ condition_showS:(_EmissionAL1Enabled==1 && _EnableAudioLink==1)}", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _EmissionAL1StrengthBand ("Emission Add Band--{ condition_showS:(_EmissionAL1Enabled==1 && _EnableAudioLink==1)}", Int) = 0
		[Vector2] _AudioLinkEmission1CenterOut ("Center Out--{ condition_showS:(_EmissionAL1Enabled==1 && _EnableAudioLink==1)}", Vector) = (0, 0, 0, 0)
		_AudioLinkEmission1CenterOutSize ("Intensity Threshold--{ condition_showS:(_EmissionAL1Enabled==1 && _EnableAudioLink==1)}", Range(0, 1)) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkEmission1CenterOutBand ("Center Out Band--{ condition_showS:(_EmissionAL1Enabled==1 && _EnableAudioLink==1)}", Int) = 0
		[HideInInspector] m_end_emission1Options ("", Float) = 0
		[HideInInspector] m_start_emission2Options ("Emission 2--{reference_property:_EnableEmission2}", Float) = 0
		[HideInInspector][ThryToggle(POI_EMISSION_2)]_EnableEmission2 ("Enable Emission 2", Float) = 0
		[ToggleUI]_EmissionReplace2 ("Replace Base Color", Float) = 0
		[HDR]_EmissionColor2 ("Emission Color--{reference_property:_EmissionColor2ThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _EmissionColor2ThemeIndex ("", Int) = 0
		[Gradient]_EmissionMap2 ("Emission Map--{reference_properties:[_EmissionMap2Pan, _EmissionMap2UV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_EmissionMap2Pan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _EmissionMap2UV ("UV", Int) = 0
		[ToggleUI]_EmissionBaseColorAsMap2 ("Base Color as Map?", Float) = 0
		_EmissionMask2 ("Emission Mask--{reference_properties:[_EmissionMask2Pan, _EmissionMask2UV, _EmissionMaskInvert2]}", 2D) = "white" { }
		[HideInInspector][Vector2]_EmissionMask2Pan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _EmissionMask2UV ("UV", Int) = 0
		[ToggleUI]_EmissionMaskInvert2 ("Invert", Float) = 0
		_EmissionStrength2 ("Emission Strength", Range(0, 20)) = 0
		[Space(4)]
		[ThryToggleUI(true)]_EmissionHueShiftEnabled2 ("<size=13><b>  Hue Shift</b></size>", Float) = 0
		_EmissionHueShift2 ("Hue Shift--{condition_showS:(_EmissionHueShiftEnabled2==1)}", Range(0, 1)) = 0
		_EmissionHueShiftSpeed2 ("Hue Shift Speed--{condition_showS:(_EmissionHueShiftEnabled2==1)}", Float) = 0
		[Space(4)]
		[ThryToggleUI(true)]_EmissionCenterOutEnabled2 ("<size=13><b>  Center Out</b></size>", Float) = 0
		_EmissionCenterOutSpeed2 ("Flow Speed--{condition_showS:(_EmissionCenterOutEnabled2==1)}", Float) = 5
		[Space(4)]
		[ThryToggleUI(true)]_EnableGITDEmission2 ("<size=13><b>  Light Based</b></size>", Float) = 0
		[Enum(World, 0, Mesh, 1)] _GITDEWorldOrMesh2 ("Lighting Type--{condition_showS:(_EnableGITDEmission2==1)}", Int) = 0
		_GITDEMinEmissionMultiplier2 ("Min Emission Multiplier--{condition_showS:(_EnableGITDEmission2==1)}", Range(0, 1)) = 1
		_GITDEMaxEmissionMultiplier2 ("Max Emission Multiplier--{condition_showS:(_EnableGITDEmission2==1)}", Range(0, 1)) = 0
		_GITDEMinLight2 ("Min Lighting--{condition_showS:(_EnableGITDEmission2==1)}", Range(0, 1)) = 0
		_GITDEMaxLight2 ("Max Lighting--{condition_showS:(_EnableGITDEmission2==1)}", Range(0, 1)) = 1
		[Space(4)]
		[ThryToggleUI(true)]_EmissionBlinkingEnabled2 ("<size=13><b>  Blinking</b></size>", Float) = 0
		_EmissiveBlink_Min2 ("Emissive Blink Min--{condition_showS:(_EmissionBlinkingEnabled2==1)}", Float) = 0
		_EmissiveBlink_Max2 ("Emissive Blink Max--{condition_showS:(_EmissionBlinkingEnabled2==1)}", Float) = 1
		_EmissiveBlink_Velocity2 ("Emissive Blink Velocity--{condition_showS:(_EmissionBlinkingEnabled2==1)}", Float) = 4
		_EmissionBlinkingOffset2 ("Offset--{condition_showS:(_EmissionBlinkingEnabled2==1)}", Float) = 0
		[Space(4)]
		[ThryToggleUI(true)] _ScrollingEmission2 ("<size=13><b>  Scrolling</b></size>", Float) = 0
		[ToggleUI]_EmissionScrollingUseCurve2 ("Use Curve--{condition_showS:(_ScrollingEmission2==1)}", float) = 0
		[Curve]_EmissionScrollingCurve2 ("Curve--{condition_showS:(_ScrollingEmission1==1&&_EmissionScrollingUseCurve2==1)}", 2D) = "white" { }
		[ToggleUI]_EmissionScrollingVertexColor2 ("VColor as position--{condition_showS:(_ScrollingEmission2==1)}", float) = 0
		_EmissiveScroll_Direction2 ("Direction--{condition_showS:(_ScrollingEmission2==1)}", Vector) = (0, -10, 0, 0)
		_EmissiveScroll_Width2 ("Width--{condition_showS:(_ScrollingEmission2==1)}", Float) = 10
		_EmissiveScroll_Velocity2 ("Velocity--{condition_showS:(_ScrollingEmission2==1)}", Float) = 10
		_EmissiveScroll_Interval2 ("Interval--{condition_showS:(_ScrollingEmission2==1)}", Float) = 20
		_EmissionScrollingOffset2 ("Offset--{condition_showS:(_ScrollingEmission2==1)}", Float) = 0
		[Space(4)]
		[ThryToggleUI(true)] _EmissionAL2Enabled ("<size=13><b>  Audio Link</b></size>--{ condition_showS:_EnableAudioLink==1}", Float) = 0
		[Vector2]_EmissionAL2StrengthMod ("Emission Strength Add--{ condition_showS:(_EmissionAL2Enabled==1 && _EnableAudioLink==1)}", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _EmissionAL2StrengthBand ("Emission Add Band--{ condition_showS:(_EmissionAL2Enabled==1 && _EnableAudioLink==1)}", Int) = 0
		[Vector2] _AudioLinkEmission2CenterOut ("Center Out--{ condition_showS:(_EmissionAL2Enabled==1 && _EnableAudioLink==1)}", Vector) = (0, 0, 0, 0)
		_AudioLinkEmission2CenterOutSize ("Intensity Threshold--{ condition_showS:(_EmissionAL2Enabled==1 && _EnableAudioLink==1)}", Range(0, 1)) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkEmission2CenterOutBand ("Center Out Band--{ condition_showS:(_EmissionAL2Enabled==1 && _EnableAudioLink==1)}", Int) = 0
		[HideInInspector] m_end_emission2Options ("", Float) = 0
		[HideInInspector] m_start_emission3Options ("Emission 3--{reference_property:_EnableEmission3}", Float) = 0
		[HideInInspector][ThryToggle(POI_EMISSION_3)]_EnableEmission3 ("Enable Emission 3", Float) = 0
		[ToggleUI]_EmissionReplace3 ("Replace Base Color", Float) = 0
		[HDR]_EmissionColor3 ("Emission Color--{reference_property:_EmissionColor3ThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _EmissionColor3ThemeIndex ("", Int) = 0
		[Gradient]_EmissionMap3 ("Emission Map--{reference_properties:[_EmissionMap3Pan, _EmissionMap3UV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_EmissionMap3Pan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _EmissionMap3UV ("UV", Int) = 0
		[ToggleUI]_EmissionBaseColorAsMap3 ("Base Color as Map?", Float) = 0
		_EmissionMask3 ("Emission Mask--{reference_properties:[_EmissionMask3Pan, _EmissionMask3UV, _EmissionMaskInvert3]}", 2D) = "white" { }
		[HideInInspector][Vector2]_EmissionMask3Pan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _EmissionMask3UV ("UV", Int) = 0
		[ToggleUI]_EmissionMaskInvert3 ("Invert", Float) = 0
		_EmissionStrength3 ("Emission Strength", Range(0, 20)) = 0
		[Space(4)]
		[ThryToggleUI(true)]_EmissionHueShiftEnabled3 ("<size=13><b>  Hue Shift</b></size>", Float) = 0
		_EmissionHueShift3 ("Hue Shift--{condition_showS:(_EmissionHueShiftEnabled3==1)}", Range(0, 1)) = 0
		_EmissionHueShiftSpeed3 ("Hue Shift Speed--{condition_showS:(_EmissionHueShiftEnabled3==1)}", Float) = 0
		[Space(4)]
		[ThryToggleUI(true)]_EmissionCenterOutEnabled3 ("<size=13><b>  Center Out</b></size>", Float) = 0
		_EmissionCenterOutSpeed3 ("Flow Speed--{condition_showS:(_EmissionCenterOutEnabled3==1)}", Float) = 5
		[Space(4)]
		[ThryToggleUI(true)]_EnableGITDEmission3 ("<size=13><b>  Light Based</b></size>", Float) = 0
		[Enum(World, 0, Mesh, 1)] _GITDEWorldOrMesh3 ("Lighting Type--{condition_showS:(_EnableGITDEmission3==1)}", Int) = 0
		_GITDEMinEmissionMultiplier3 ("Min Emission Multiplier--{condition_showS:(_EnableGITDEmission3==1)}", Range(0, 1)) = 1
		_GITDEMaxEmissionMultiplier3 ("Max Emission Multiplier--{condition_showS:(_EnableGITDEmission3==1)}", Range(0, 1)) = 0
		_GITDEMinLight3 ("Min Lighting--{condition_showS:(_EnableGITDEmission3==1)}", Range(0, 1)) = 0
		_GITDEMaxLight3 ("Max Lighting--{condition_showS:(_EnableGITDEmission3==1)}", Range(0, 1)) = 1
		[Space(4)]
		[ThryToggleUI(true)]_EmissionBlinkingEnabled3 ("<size=13><b>  Blinking</b></size>", Float) = 0
		_EmissiveBlink_Min3 ("Emissive Blink Min--{condition_showS:(_EmissionBlinkingEnabled3==1)}", Float) = 0
		_EmissiveBlink_Max3 ("Emissive Blink Max--{condition_showS:(_EmissionBlinkingEnabled3==1)}", Float) = 1
		_EmissiveBlink_Velocity3 ("Emissive Blink Velocity--{condition_showS:(_EmissionBlinkingEnabled3==1)}", Float) = 4
		_EmissionBlinkingOffset3 ("Offset--{condition_showS:(_EmissionBlinkingEnabled3==1)}", Float) = 0
		[Space(4)]
		[ThryToggleUI(true)] _ScrollingEmission3 ("<size=13><b>  Scrolling</b></size>", Float) = 0
		[ToggleUI]_EmissionScrollingUseCurve3 ("Use Curve--{condition_showS:(_ScrollingEmission3==1)}", float) = 0
		[Curve]_EmissionScrollingCurve3 ("Curve--{condition_showS:(_ScrollingEmission1==1&&_EmissionScrollingUseCurve3==1)}", 2D) = "white" { }
		[ToggleUI]_EmissionScrollingVertexColor3 ("VColor as position--{condition_showS:(_ScrollingEmission3==1)}", float) = 0
		_EmissiveScroll_Direction3 ("Direction--{condition_showS:(_ScrollingEmission3==1)}", Vector) = (0, -10, 0, 0)
		_EmissiveScroll_Width3 ("Width--{condition_showS:(_ScrollingEmission3==1)}", Float) = 10
		_EmissiveScroll_Velocity3 ("Velocity--{condition_showS:(_ScrollingEmission3==1)}", Float) = 10
		_EmissiveScroll_Interval3 ("Interval--{condition_showS:(_ScrollingEmission3==1)}", Float) = 20
		_EmissionScrollingOffset3 ("Offset--{condition_showS:(_ScrollingEmission3==1)}", Float) = 0
		[Space(4)]
		[ThryToggleUI(true)] _EmissionAL3Enabled ("<size=13><b>  Audio Link</b></size>--{ condition_showS:_EnableAudioLink==1}", Float) = 0
		[Vector2]_EmissionAL3StrengthMod ("Emission Strength Add--{ condition_showS:(_EmissionAL3Enabled==1 && _EnableAudioLink==1)}", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _EmissionAL3StrengthBand ("Emission Add Band--{ condition_showS:(_EmissionAL3Enabled==1 && _EnableAudioLink==1)}", Int) = 0
		[Vector2] _AudioLinkEmission3CenterOut ("Center Out--{ condition_showS:(_EmissionAL3Enabled==1 && _EnableAudioLink==1)}", Vector) = (0, 0, 0, 0)
		_AudioLinkEmission3CenterOutSize ("Intensity Threshold--{ condition_showS:(_EmissionAL3Enabled==1 && _EnableAudioLink==1)}", Range(0, 1)) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkEmission3CenterOutBand ("Center Out Band--{ condition_showS:(_EmissionAL3Enabled==1 && _EnableAudioLink==1)}", Int) = 0
		[HideInInspector] m_end_emission3Options ("", Float) = 0
		[HideInInspector] m_end_emissions ("Emissions", Float) = 0
		[HideInInspector] m_start_glitter ("Glitter / Sparkle--{reference_property:_GlitterEnable}", Float) = 0
		[HideInInspector][ThryToggle(_SUNDISK_SIMPLE)]_GlitterEnable ("Enable Glitter?", Float) = 0
		[ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _GlitterUV ("UV", Int) = 0
		[Enum(Angle, 0, Linear Emission, 1, Light Reflections, 2)]_GlitterMode ("Mode", Int) = 0
		[Enum(Circle, 0, Square, 1)]_GlitterShape ("Shape", Int) = 0
		[Enum(Add, 0, Replace, 1)] _GlitterBlendType ("Blend Mode", Int) = 0
		[HDR]_GlitterColor ("Color--{reference_property:_GlitterColorThemeIndex}", Color) = (1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _GlitterColorThemeIndex ("", Int) = 0
		_GlitterUseSurfaceColor ("Use Surface Color", Range(0, 1)) = 0
		_GlitterColorMap ("Glitter Color Map--{reference_properties:[_GlitterColorMapPan, _GlitterColorMapUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_GlitterColorMapPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _GlitterColorMapUV ("UV", Int) = 0
		[HideInInspector][Vector2]_GlitterPan ("Panning", Vector) = (0, 0, 0, 0)
		_GlitterMask ("Glitter Mask--{reference_properties:[_GlitterMaskPan, _GlitterMaskUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_GlitterMaskPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _GlitterMaskUV ("UV", Int) = 0
		_GlitterTexture ("Glitter Texture--{reference_properties:[_GlitterTexturePan]}", 2D) = "white" { }
		[HideInInspector][Vector2]_GlitterTexturePan ("Panning", Vector) = (0, 0, 0, 0)
		[Vector2]_GlitterUVPanning ("Panning Speed", Vector) = (0, 0, 0, 0)
		_GlitterTextureRotation ("Rotation Speed", Float) = 0
		_GlitterFrequency ("Glitter Density", Float) = 300.0
		_GlitterJitter ("Glitter Jitter", Range(0, 1)) = 1.0
		_GlitterSpeed ("Glitter Speed", Float) = 10.0
		_GlitterSize ("Glitter Size", Range(0, 1)) = .3
		_GlitterContrast ("Glitter Contrast--{condition_showS:(_GlitterMode==0||_GlitterMode==2)}", Range(1, 1000)) = 300
		_GlitterAngleRange ("Glitter Angle Range--{condition_showS:(_GlitterMode==0||_GlitterMode==2)}", Range(0, 90)) = 90
		_GlitterMinBrightness ("Glitter Min Brightness", Range(0, 1)) = 0
		_GlitterBrightness ("Glitter Max Brightness", Range(0, 40)) = 3
		_GlitterBias ("Glitter Bias--{condition_show:(_GlitterMode==0)}", Range(0, 1)) = .8
		_GlitterHideInShadow ("Hide in shadow", Range(0, 1)) = 0
		_GlitterCenterSize ("dim light--{condition_show:{type:AND,condition1:{type:PROPERTY_BOOL,data:_GlitterMode==1},condition2:{type:PROPERTY_BOOL,data:_GlitterShape==1}}}", Range(0, 1)) = .08
		_glitterFrequencyLinearEmissive ("Frequency--{condition_show:{type:PROPERTY_BOOL,data:_GlitterMode==1}}", Range(0, 100)) = 20
		_GlitterJaggyFix ("Jaggy Fix--{condition_show:{type:PROPERTY_BOOL,data:_GlitterShape==1}}", Range(0, .1)) = .0
		[Space(10)]
		[ThryToggleUI(true)]_GlitterHueShiftEnabled ("<size=13><b>  Hue Shift</b></size>", Float) = 0
		_GlitterHueShiftSpeed ("Shift Speed--{condition_showS:(_GlitterHueShiftEnabled==1)}", Float) = 0
		_GlitterHueShift ("Hue Shift--{condition_showS:(_GlitterHueShiftEnabled==1)}", Range(0, 1)) = 0
		[Space(10)]
		[ThryToggleUI(true)]_GlitterRandomColors ("<size=13><b>  Random Stuff</b></size>", Float) = 0
		[MultiSlider]_GlitterMinMaxSaturation ("Saturation Range--{condition_showS:(_GlitterRandomColors==1)}", Vector) = (0.8, 1, 0, 1)
		[MultiSlider]_GlitterMinMaxBrightness ("Brightness Range--{condition_showS:(_GlitterRandomColors==1)}", Vector) = (0.8, 1, 0, 1)
		[ToggleUI]_GlitterRandomSize ("Random Size?--{condition_showS:(_GlitterRandomColors==1)}", Float) = 0
		[MultiSlider]_GlitterMinMaxSize ("Size Range--{condition_showS:(_GlitterRandomColors==1)}", Vector) = (0.1, 0.5, 0, 1)
		[ToggleUI]_GlitterRandomRotation ("Random Tex Rotation--{condition_showS:(_GlitterRandomColors==1)}", Float) = 0
		[HideInInspector] m_end_glitter ("Glitter / Sparkle--{condition_showS:(_GlitterRandomColors==1)}", Float) = 0
		[HideInInspector] m_start_pathing ("Pathing--{reference_property: _EnablePathing}", Float) = 0
		[HideInInspector][ThryToggle(POI_PATHING)] _EnablePathing ("Enable Pathing", Float) = 0
		[Enum(Split Channels, 0, Merged Channels, 1)]_PathGradientType ("Gradient Type", Float) = 0
		[ToggleUI]_PathingOverrideAlpha ("Override alpha", Float) = 0
		[ThryRGBAPacker(R Path, G Path, B Path, A Path)]_PathingMap ("RGBA Path Map--{reference_properties:[_PathingMapPan, _PathingMapUV]}", 2D) = "white" { }
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_PathingMapUV ("UV", Int) = 0
		[HideInInspector][Vector2]_PathingMapPan ("Panning", Vector) = (0, 0, 0, 0)
		[ThryRGBAPacker(1, RGB Color, A Mask, 1)]_PathingColorMap ("Color & Mask (Expand)--{reference_properties:[_PathingColorMapPan, _PathingColorMapUV]}", 2D) = "white" { }
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_PathingColorMapUV ("UV", Int) = 0
		[HideInInspector][Vector2]_PathingColorMapPan ("Panning", Vector) = (0, 0, 0, 0)
		[Enum(Fill, 0, Path, 1, Loop, 2)]_PathTypeR ("R Path Type", Float) = 0
		[Enum(Fill, 0, Path, 1, Loop, 2)]_PathTypeG ("G Path Type", Float) = 0
		[Enum(Fill, 0, Path, 1, Loop, 2)]_PathTypeB ("B Path Type", Float) = 0
		[Enum(Fill, 0, Path, 1, Loop, 2)]_PathTypeA ("A Path Type", Float) = 0
		[HDR]_PathColorR ("R Color--{reference_property:_PathColorRThemeIndex}", Color) = (1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _PathColorRThemeIndex ("", Int) = 0
		[HDR]_PathColorG ("G Color--{reference_property:_PathColorGThemeIndex}", Color) = (1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _PathColorGThemeIndex ("", Int) = 0
		[HDR]_PathColorB ("B Color--{reference_property:_PathColorBThemeIndex}", Color) = (1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _PathColorBThemeIndex ("", Int) = 0
		[HDR]_PathColorA ("A Color--{reference_property:_PathColorAThemeIndex}", Color) = (1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _PathColorAThemeIndex ("", Int) = 0
		_PathEmissionStrength ("Emission Strength", Vector) = (0.0, 0.0, 0.0, 0.0)
		_PathSoftness ("Softness", Vector) = (1, 1, 1, 1)
		_PathSpeed ("Speed", Vector) = (1.0, 1.0, 1.0, 1.0)
		_PathWidth ("Length", Vector) = (0.03, 0.03, 0.03, 0.03)
		[Header(Timing Options)]
		_PathTime ("Manual Timing", Vector) = (-999.0, -999.0, -999.0, -999.0)
		_PathOffset ("Timing Offset", Vector) = (0.0, 0.0, 0.0, 0.0)
		_PathSegments ("Path Segments", Vector) = (0.0, 0.0, 0.0, 0.0)
		[HideInInspector] m_start_PathAudioLink ("Audio Link ♫--{ condition_showS:_EnableAudioLink==1}", Float) = 0
		[ThryToggleUI(true)]_PathALTimeOffset ("<size=13><b>  Time Offset</b></size>", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkPathTimeOffsetBandR ("Band	R--{condition_showS:(_PathALTimeOffset==1)}", Int) = 0
		[Vector2]_AudioLinkPathTimeOffsetR ("Offset	R--{condition_showS:(_PathALTimeOffset==1)}", Vector) = (0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkPathTimeOffsetBandG ("Band	G--{condition_showS:(_PathALTimeOffset==1)}", Int) = 0
		[Vector2]_AudioLinkPathTimeOffsetG ("Offset	G--{condition_showS:(_PathALTimeOffset==1)}", Vector) = (0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkPathTimeOffsetBandB ("Band	B--{condition_showS:(_PathALTimeOffset==1)}", Int) = 0
		[Vector2]_AudioLinkPathTimeOffsetB ("Offset	B--{condition_showS:(_PathALTimeOffset==1)}", Vector) = (0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkPathTimeOffsetBandA ("Band	A--{condition_showS:(_PathALTimeOffset==1)}", Int) = 0
		[Vector2]_AudioLinkPathTimeOffsetA ("Offset	A--{condition_showS:(_PathALTimeOffset==1)}", Vector) = (0, 0, 0)
		[Space(4)]
		[ThryToggleUI(true)]_PathALEmissionOffset ("<size=13><b>  Emission Offset</b></size>", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkPathEmissionAddBandR ("Band	R--{condition_showS:(_PathALEmissionOffset==1)}", Int) = 0
		[Vector2]_AudioLinkPathEmissionAddR ("Offset	R--{condition_showS:(_PathALEmissionOffset==1)}", Vector) = (0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkPathEmissionAddBandG ("Band	G--{condition_showS:(_PathALEmissionOffset==1)}", Int) = 0
		[Vector2]_AudioLinkPathEmissionAddG ("Offset	G--{condition_showS:(_PathALEmissionOffset==1)}", Vector) = (0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkPathEmissionAddBandB ("Band	B--{condition_showS:(_PathALEmissionOffset==1)}", Int) = 0
		[Vector2]_AudioLinkPathEmissionAddB ("Offset	B--{condition_showS:(_PathALEmissionOffset==1)}", Vector) = (0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkPathEmissionAddBandA ("Band	A--{condition_showS:(_PathALEmissionOffset==1)}", Int) = 0
		[Vector2]_AudioLinkPathEmissionAddA ("Offset	A--{condition_showS:(_PathALEmissionOffset==1)}", Vector) = (0, 0, 0)
		[Space(4)]
		[ThryToggleUI(true)]_PathALWidthOffset ("<size=13><b>  Width Offset</b></size>", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkPathWidthOffsetBandR ("Band	R--{condition_showS:(_PathALWidthOffset==1)}", Int) = 0
		[Vector2]_AudioLinkPathWidthOffsetR ("Offset	R--{condition_showS:(_PathALWidthOffset==1)}", Vector) = (0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkPathWidthOffsetBandG ("Band	G--{condition_showS:(_PathALWidthOffset==1)}", Int) = 0
		[Vector2]_AudioLinkPathWidthOffsetG ("Offset	G--{condition_showS:(_PathALWidthOffset==1)}", Vector) = (0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkPathWidthOffsetBandB ("Band	B--{condition_showS:(_PathALWidthOffset==1)}", Int) = 0
		[Vector2]_AudioLinkPathWidthOffsetB ("Offset	B--{condition_showS:(_PathALWidthOffset==1)}", Vector) = (0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _AudioLinkPathWidthOffsetBandA ("Band	A--{condition_showS:(_PathALWidthOffset==1)}", Int) = 0
		[Vector2]_AudioLinkPathWidthOffsetA ("Offset	A--{condition_showS:(_PathALWidthOffset==1)}", Vector) = (0, 0, 0)
		[Space(4)]
		[ThryToggleUI(true)]_PathALHistory ("<size=13><b>  History</b></size>", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _PathALHistoryBandR ("R Band--{condition_showS:(_PathALHistory==1)}", Int) = 0
		[ToggleUI]_PathALHistoryR ("R History--{condition_showS:(_PathALHistory==1)}", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _PathALHistoryBandG ("G Band--{condition_showS:(_PathALHistory==1)}", Int) = 0
		[ToggleUI]_PathALHistoryG ("G History--{condition_showS:(_PathALHistory==1)}", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _PathALHistoryBandB ("B Band--{condition_showS:(_PathALHistory==1)}", Int) = 0
		[ToggleUI]_PathALHistoryB ("B History--{condition_showS:(_PathALHistory==1)}", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _PathALHistoryBandA ("A Band--{condition_showS:(_PathALHistory==1)}", Int) = 0
		[ToggleUI]_PathALHistoryA ("A History--{condition_showS:(_PathALHistory==1)}", Float) = 0
		[Space(4)]
		[ThryToggleUI(true)]_PathALChrono ("<size=13><b>  Chrono Time</b></size>", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _PathChronoBandR ("R Band--{condition_showS:(_PathALChrono==1)}", Int) = 0
		[ThryWideEnum(Motion increases as intensity of band increases, 0, Above but Smooth, 1, Motion moves back and forth as a function of intensity, 2, Above but Smoooth, 3, Fixed speed increase when the band is dark Stationary when light, 4, Above but Smooooth, 5, Fixed speed increase when the band is dark Fixed speed decrease when light, 6, Above but Smoooooth, 7)]_PathChronoTypeR ("R Motion Type--{condition_showS:(_PathALChrono==1)}", Int) = 0
		_PathChronoSpeedR ("R Speed--{condition_showS:(_PathALChrono==1)}", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _PathChronoBandG ("G Band--{condition_showS:(_PathALChrono==1)}", Int) = 0
		[ThryWideEnum(Motion increases as intensity of band increases, 0, Above but Smooth, 1, Motion moves back and forth as a function of intensity, 2, Above but Smoooth, 3, Fixed speed increase when the band is dark Stationary when light, 4, Above but Smooooth, 5, Fixed speed increase when the band is dark Fixed speed decrease when light, 6, Above but Smoooooth, 7)]_PathChronoTypeG ("G Motion Type--{condition_showS:(_PathALChrono==1)}", Int) = 0
		_PathChronoSpeedG ("G Speed--{condition_showS:(_PathALChrono==1)}", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _PathChronoBandB ("B Band--{condition_showS:(_PathALChrono==1)}", Int) = 0
		[ThryWideEnum(Motion increases as intensity of band increases, 0, Above but Smooth, 1, Motion moves back and forth as a function of intensity, 2, Above but Smoooth, 3, Fixed speed increase when the band is dark Stationary when light, 4, Above but Smooooth, 5, Fixed speed increase when the band is dark Fixed speed decrease when light, 6, Above but Smoooooth, 7)]_PathChronoTypeB ("B Motion Type--{condition_showS:(_PathALChrono==1)}", Int) = 0
		_PathChronoSpeedB ("B Speed--{condition_showS:(_PathALChrono==1)}", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _PathChronoBandA ("A Band--{condition_showS:(_PathALChrono==1)}", Int) = 0
		[ThryWideEnum(Motion increases as intensity of band increases, 0, Above but Smooth, 1, Motion moves back and forth as a function of intensity, 2, Above but Smoooth, 3, Fixed speed increase when the band is dark Stationary when light, 4, Above but Smooooth, 5, Fixed speed increase when the band is dark Fixed speed decrease when light, 6, Above but Smoooooth, 7)]_PathChronoTypeA ("A Motion Type--{condition_showS:(_PathALChrono==1)}", Int) = 0
		_PathChronoSpeedA ("A Speed--{condition_showS:(_PathALChrono==1)}", Float) = 0
		[Space(4)]
		[ThryToggleUI(true)]_PathALAutoCorrelator ("<size=13><b>  Auto Correlator</b></size>", Float) = 0
		[Enum(Off, 0, On, 1, Mirrored, 2)]_PathALAutoCorrelatorR ("R Type--{condition_showS:(_PathALAutoCorrelator==1)}", Int) = 0
		[Enum(Off, 0, On, 1, Mirrored, 2)]_PathALAutoCorrelatorG ("G Type--{condition_showS:(_PathALAutoCorrelator==1)}", Int) = 0
		[Enum(Off, 0, On, 1, Mirrored, 2)]_PathALAutoCorrelatorB ("B Type--{condition_showS:(_PathALAutoCorrelator==1)}", Int) = 0
		[Enum(Off, 0, On, 1, Mirrored, 2)]_PathALAutoCorrelatorA ("A Type--{condition_showS:(_PathALAutoCorrelator==1)}", Int) = 0
		[Space(4)]
		[ToggleUI]_PathALCCR ("R Color Chord Strip", Float) = 0
		[ToggleUI]_PathALCCG ("G Color Chord Strip", Float) = 0
		[ToggleUI]_PathALCCB ("B Color Chord Strip", Float) = 0
		[ToggleUI]_PathALCCA ("A Color Chord Strip", Float) = 0
		[HideInInspector] m_end_PathAudioLink ("", Float) = 0
		[HideInInspector] m_end_pathing ("", Float) = 0
		[HideInInspector] m_start_mirrorOptions ("Mirror--{reference_property:_EnableMirrorOptions}", Float) = 0
		[HideInInspector][ThryToggle(POI_MIRROR)]_EnableMirrorOptions ("Enable Mirror Options", Float) = 0
		[ThryWideEnum(Show In Both, 0, Show Only In Mirror, 1, Dont Show In Mirror, 2)] _Mirror ("Show in mirror", Int) = 0
		_MirrorTexture ("Mirror Texture--{reference_properties:[_MirrorTexturePan, _MirrorTextureUV]},", 2D) = "white" { }
		[HideInInspector][Vector2]_MirrorTexturePan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _MirrorTextureUV("UV", Int) = 0
		[HideInInspector] m_end_mirrorOptions ("Mirror", Float) = 0
		[HideInInspector] m_start_depthFX ("Depth FX--{reference_property:_EnableTouchGlow}", Float) = 0
		[HideInInspector][ThryToggle(GRAIN)]_EnableTouchGlow ("Enable Depth FX", Float) = 0
		_DepthMask ("Mask--{reference_properties:[_DepthMaskPan, _DepthMaskUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_DepthMaskPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _DepthMaskUV ("UV", Int) = 0
		[Space(10)]
		[ThryToggleUI(true)]_DepthColorToggle ("<size=13><b>  Color & Emission</b></size>", Float) = 0
		[ThryWideEnum(Replace, 0, Multiply, 1, Add, 2)] _DepthColorBlendMode ("Blend Type--{condition_showS:(_DepthColorToggle==1)}", Int) = 0
		_DepthTexture ("Depth Texture--{reference_properties:[_DepthTexturePan, _DepthTextureUV], condition_showS:(_DepthColorToggle==1)}", 2D) = "white" { }
		[HideInInspector][Vector2]_DepthTexturePan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7, Depth Gradient, 8)] _DepthTextureUV ("UV", Int) = 0
		_DepthColor ("Color--{condition_showS:(_DepthColorToggle==1), reference_property:_DepthColorThemeIndex}", Color) = (1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _DepthColorThemeIndex ("", Int) = 0
		_DepthEmissionStrength ("Emission Strength--{condition_showS:(_DepthColorToggle==1)}", Range(0, 20)) = 0
		_DepthColorMinDepth ("Min Depth--{condition_showS:(_DepthColorToggle==1)}", Float) = 0
		_DepthColorMaxDepth ("Max Depth--{condition_showS:(_DepthColorToggle==1)}", Float) = 1
		_DepthColorMinValue ("Min Color Blend--{condition_showS:(_DepthColorToggle==1)}", Range(0, 1)) = 0
		_DepthColorMaxValue ("Max Color Blend--{condition_showS:(_DepthColorToggle==1)}", Range(0, 1)) = 1
		[Space(10)]
		[ThryToggleUI(true)]_DepthAlphaToggle ("<size=13><b>  Alpha</b></size>", Float) = 0
		_DepthAlphaMinDepth ("Min Depth--{condition_showS:(_DepthAlphaToggle==1)}", Float) = 0
		_DepthAlphaMaxDepth ("Max Depth--{condition_showS:(_DepthAlphaToggle==1)}", Float) = 1
		_DepthAlphaMinValue ("Min Alpha--{condition_showS:(_DepthAlphaToggle==1)}", Range(0, 1)) = 1
		_DepthAlphaMaxValue ("Max Alpha--{condition_showS:(_DepthAlphaToggle==1)}", Range(0, 1)) = 0
		[HideInInspector] m_end_depthFX ("Depth FX", Float) = 0
		[HideInInspector] m_start_Iridescence ("Iridescence--{reference_property:_EnableIridescence}", Float) = 0
		[HideInInspector][ThryToggle(POI_IRIDESCENCE)]_EnableIridescence ("Enable Iridescence", Float) = 0
		[Gradient]_IridescenceRamp ("Ramp--{reference_properties:[_IridescenceRampPan]}", 2D) = "white" { }
		[HideInInspector][Vector2]_IridescenceRampPan ("Panning", Vector) = (0, 0, 0, 0)
		_IridescenceMask ("Mask--{reference_properties:[_IridescenceMaskPan, _IridescenceMaskUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_IridescenceMaskPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_IridescenceMaskUV ("Mask UV", Int) = 0
		[ToggleUI]_IridescenceNormalToggle ("Custom Normals?", Float) = 0
		[Normal]_IridescenceNormalMap ("Normal Map--{reference_properties:[_IridescenceNormalIntensity, _IridescenceNormalMapPan, _IridescenceNormalMapUV]}", 2D) = "bump" { }
		[HideInInspector]_IridescenceNormalIntensity ("Normal Intensity", Range(0, 10)) = 1
		[HideInInspector][Vector2]_IridescenceNormalMapPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_IridescenceNormalMapUV ("Normal UV", Int) = 0
		[Enum(Vertex, 0, Pixel, 1)] _IridescenceNormalSelection ("Normal Select", Int) = 1
		_IridescenceIntensity ("Intensity", Range(0, 10)) = 1
		_IridescenceAddBlend ("Blend Add", Range(0, 1)) = 0
		_IridescenceReplaceBlend ("Blend Replace", Range(0, 1)) = 0
		_IridescenceMultiplyBlend ("Blend Multiply", Range(0, 1)) = 0
		_IridescenceEmissionStrength ("Emission Strength", Range(0, 20)) = 0
		[ThryToggleUI(true)]_IridescenceHueShiftEnabled ("<size=13><b>  Hue Shift</b></size>", Float) = 0
		_IridescenceHueShiftSpeed ("Speed--{condition_showS:(_IridescenceHueShiftEnabled==1)}", Float) = 0
		_IridescenceHueShift ("Shift--{condition_showS:(_IridescenceHueShiftEnabled==1)}", Range(0, 1)) = 0
		[HideInInspector] m_start_IridescenceAudioLink ("Audio Link ♫--{ condition_showS:_EnableAudioLink==1}", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _IridescenceAudioLinkEmissionAddBand ("Emission Band", Int) = 0
		[Vector2]_IridescenceAudioLinkEmissionAdd ("Emission Mod", Vector) = (0, 0, 0, 0)
		[HideInInspector] m_end_IridescenceAudioLink ("Audio Link", Float) = 0
		[HideInInspector] m_end_Iridescence ("Iridescence", Float) = 0
		[HideInInspector] m_start_Text ("Stats Overlay--{reference_property:_TextEnabled}", Float) = 0
		_TextGlyphs ("Font Array", 2D) = "black" { }
		_TextPixelRange ("Pixel Range", Float) = 4.0
		[HideInInspector][ThryToggle(EFFECT_BUMP)]_TextEnabled ("Text?", Float) = 0
		[HideInInspector] m_start_TextFPS ("FPS--{reference_property:_TextFPSEnabled}", Float) = 0
		[HideInInspector][ToggleUI]_TextFPSEnabled ("FPS Text?", Float) = 0
		[ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _TextFPSUV ("FPS UV", Int) = 0
		_TextFPSColor ("Color--{reference_property:_TextFPSColorThemeIndex}", Color) = (1, 1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _TextFPSColorThemeIndex ("", Int) = 0
		_TextFPSEmissionStrength ("Emission Strength", Range(0, 20)) = 0
		[Vector2]_TextFPSOffset ("Offset", Vector) = (0, 0, 0, 0)
		_TextFPSRotation ("Rotation", Range(0, 360)) = 0
		[Vector2]_TextFPSScale ("Scale", Vector) = (1, 1, 1, 1)
		_TextFPSPadding ("Padding Reduction", Vector) = (0, 0, 0, 0)
		[HideInInspector] m_end_TextFPS ("FPS", Float) = 0
		[HideInInspector] m_start_TextPosition ("Position--{reference_property:_TextPositionEnabled}", Float) = 0
		[HideInInspector][ToggleUI]_TextPositionEnabled ("Position Text?", Float) = 0
		[ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _TextPositionUV ("Position UV", Int) = 0
		_TextPositionColor ("Color--{reference_property:_TextPositionColorThemeIndex}", Color) = (1, 0, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _TextPositionColorThemeIndex ("", Int) = 0
		_TextPositionEmissionStrength ("Emission Strength", Range(0, 20)) = 0
		[Vector2]_TextPositionOffset ("Offset", Vector) = (0, 0, 0, 0)
		_TextPositionRotation ("Rotation", Range(0, 360)) = 0
		[Vector2]_TextPositionScale ("Scale", Vector) = (1, 1, 1, 1)
		_TextPositionPadding ("Padding Reduction", Vector) = (0, 0, 0, 0)
		[HideInInspector] m_end_TextPosition ("Position", Float) = 0
		[HideInInspector] m_start_TextInstanceTime ("Instance Time--{reference_property:_TextTimeEnabled}", Float) = 0
		[HideInInspector][ToggleUI]_TextTimeEnabled ("Time Text?", Float) = 0
		[ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _TextTimeUV ("Time UV", Int) = 0
		_TextTimeColor ("Color--{reference_property:_TextTimeColorThemeIndex}", Color) = (1, 0, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _TextTimeColorThemeIndex ("", Int) = 0
		_TextTimeEmissionStrength ("Emission Strength", Range(0, 20)) = 0
		[Vector2]_TextTimeOffset ("Offset", Vector) = (0, 0, 0, 0)
		_TextTimeRotation ("Rotation", Range(0, 360)) = 0
		[Vector2]_TextTimeScale ("Scale", Vector) = (1, 1, 1, 1)
		_TextTimePadding ("Padding Reduction", Vector) = (0, 0, 0, 0)
		[HideInInspector] m_end_TextInstanceTime ("Instance Time", Float) = 0
		[HideInInspector] m_end_Text ("MSDF Text Overlay", Float) = 0
		[HideInInspector] m_start_FXProximityColor ("Proximity Color--{reference_property:_FXProximityColor}", Float) = 0
		[HideInInspector][ToggleUI]_FXProximityColor ("Enable", Float) = 0
		[Enum(Object Position, 0, Pixel Position, 1)]_FXProximityColorType ("Pos To Use", Int) = 1
		_FXProximityColorMinColor ("Min Distance Alpha", Color) = (0, 0, 0)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _FXProximityColorMinColorThemeIndex ("", Int) = 0
		_FXProximityColorMaxColor ("Max Distance Alpha", Color) = (1, 1, 1)
		[HideInInspector][ThryWideEnum(Off, 0, Theme Color 0, 1, Theme Color 1, 2, Theme Color 2, 3, Theme Color 3, 4, ColorChord 0, 5, ColorChord 1, 6, ColorChord 2, 7, ColorChord 3, 8, AL Theme 0, 9, AL Theme 1, 10, AL Theme 2, 11, AL Theme 3, 12)] _FXProximityColorMaxColorThemeIndex ("", Int) = 0
		_FXProximityColorMinDistance ("Min Distance", Float) = 0
		_FXProximityColorMaxDistance ("Max Distance", Float) = 1
		[HideInInspector] m_end_FXProximityColor ("", Float) = 0
		[HideInInspector] m_AudioLinkCategory (" Audio Link--{reference_property:_EnableAudioLink}", Float) = 0
		[HideInInspector] m_start_audioLink ("Audio Link", Float) = 0
		[HideInInspector][ThryToggle(POI_AUDIOLINK)] _EnableAudioLink ("Enabled?", Float) = 0
		[Helpbox(1)] _AudioLinkHelp ("This section houses the global controls for audio link. Controls for individual features are in their respective sections. (Emission, Dissolve, etc...)", Int) = 0
		[ToggleUI] _AudioLinkAnimToggle ("Anim Toggle", Float) = 1
		[ThryHeaderLabel(Debug Visualizer, 13)]
		[ToggleUI]_DebugWaveform("Waveform", Float) = 0
		[ToggleUI]_DebugDFT("DFT", Float) = 0
		[ToggleUI]_DebugBass("Bass", Float) = 0
		[ToggleUI]_DebugLowMids("Low Mids", Float) = 0
		[ToggleUI]_DebugHighMids("High Mids", Float) = 0
		[ToggleUI]_DebugTreble("Treble", Float) = 0
		[ToggleUI]_DebugCCColors("Colorchord Colors", Float) = 0
		[ToggleUI]_DebugCCStrip("Colorchord Strip", Float) = 0
		[ToggleUI]_DebugCCLights("Colorchord Lights", Float) = 0
		[ToggleUI]_DebugAutocorrelator("Autocorrelator", Float) = 0
		[ToggleUI]_DebugChronotensity("Chronotensity", Float) = 0
		[Helpbox(1)]_DebugVisualizerHelpbox ("Debug examples are best viewed on a flat surface with simple uvs like a default unity quad.", Int) = 0
		[HideInInspector] m_end_audioLink ("Audio Link", Float) = 0
		[HideInInspector] m_start_ALDecalSpectrum ("AL ♫ Spectrum--{  reference_property:_EnableALDecal}", Float) = 0
		[HideInInspector][ThryToggle(POI_AL_DECAL)]_EnableALDecal ("Enable AL Decal", Float) = 0
		[HideInInspector][ThryWideEnum(lil Spectrum, 0)] _ALDecalType ("AL Type--{ condition_showS:_EnableAudioLink==1}", Int) = 0
		[ThryHeaderLabel(Transform, 13)]
		[Space(4)]
		[Enum(Normal, 0, Circle, 1)] _ALDecalUVMode ("UV Mode", Float) = 0
		[ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _ALDecalUV ("UV", Int) = 0
		[Vector2]_ALUVPosition ("Position", Vector) = (.5, .5, 1)
		_ALUVScale ("Scale", Vector) = (1, 1, 1, 1)
		_ALUVRotation ("Rotation", Range(0, 360)) = 0
		_ALUVRotationSpeed ("Rotation Speed", Float) = 0
		_ALDecalLineWidth ("Line Width", Range(0, 1)) = 1.0
		_ALDecaldCircleDimensions ("Cirlce Dimensions--{ condition_showS:_ALDecalUVMode==1}", Vector) = (0, 1, 0, 1)
		[Space][ThryHeaderLabel(Volume, 13)]
		[Space(4)]
		_ALDecalVolumeStep ("Volume Step Num (0 = Off)", Float) = 0.0
		_ALDecalVolumeClipMin ("Volume Clip Min", Range(0, 1)) = 0.0
		_ALDecalVolumeClipMax ("Volume Clip Max", Range(0, 1)) = 1.0
		[Space][ThryHeaderLabel(Band, 13)]
		[Space(4)]
		_ALDecalBandStep ("Band Step Num (0 = Off)", Float) = 0.0
		_ALDecalBandClipMin ("Band Clip Min", Range(0, 1)) = 0.0
		_ALDecalBandClipMax ("Band Clip Max", Range(0, 1)) = 1.0
		[Space][ThryToggleUI(true)]_ALDecalShapeClip ("<size=13><b>  Shape Clip</b></size>", Float) = 0
		_ALDecalShapeClipVolumeWidth ("Volume Width--{ condition_showS:_ALDecalShapeClip==1}", Range(0, 1)) = 0.5
		_ALDecalShapeClipBandWidth ("Band Width--{ condition_showS:_ALDecalShapeClip==1}", Range(0, 1)) = 0.5
		[Space][ThryHeaderLabel(Audio Mods, 13)]
		[Space(4)]
		_ALDecalVolume ("Volume", Int) = 0.5
		_ALDecalBaseBoost ("Bass Boost", Float) = 5.0
		_ALDecalTrebleBoost ("Treble Boost", Float) = 1.0
		[Space][ThryHeaderLabel(Colors and Blending, 13)]
		[Space(4)]
		[ThryRGBAPacker(1, RGB Color, A Mask, 1)]_ALDecalColorMask ("Color & Mask--{reference_properties:[_ALDecalColorMaskPan, _ALDecalColorMaskUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_ALDecalColorMaskPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)] _ALDecalColorMaskUV ("UV", Int) = 0
		[Enum(UVX, 0, UVY, 1, Volume, 2)] _ALDecalVolumeColorSource ("Source", Int) = 1
		_ALDecalVolumeColorLow ("Volume Color Low", Color) = (0, 0, 1)
		_ALDecalLowEmission ("Low Emission", Range(0, 20)) = 0
		_ALDecalVolumeColorMid ("Volume Color Mid", Color) = (0, 1, 0)
		_ALDecalMidEmission ("Mid Emission", Range(0, 20)) = 0
		_ALDecalVolumeColorHigh ("Volume Color High", Color) = (1, 0, 0)
		_ALDecalHighEmission ("High Emission", Range(0, 20)) = 0
		[ThryWideEnum(Replace, 0, Darken, 1, Multiply, 2, Color Burn, 3, Linear Burn, 4, Lighten, 5, Screen, 6, Color Dodge, 7, Linear Dodge(Add), 8, Overlay, 9, Soft Lighten, 10, Hard Light, 11, Vivid Light, 12, Linear Light, 13, Pin Light, 14, Hard Mix, 15, Difference, 16, Exclusion, 17, Subtract, 18, Divide, 19)]_ALDecalBlendType ("Blend Type", Range(0, 1)) = 0
		_ALDecalBlendAlpha ("Alpha", Range(0, 1)) = 1
		_ALDecalControlsAlpha ("Override Alpha", Range(0, 1)) = 0
		[HideInInspector] m_end_ALDecalSpectrum ("AL ♫ Spectrum", Float) = 0
		[HideInInspector] m_modifierCategory ("UV Modifiers", Float) = 0
		[HideInInspector] m_start_uvDistortion (" Distortion UV--{reference_property:_EnableDistortion}", Float) = 0
		[HideInInspector][ThryToggle(USER_LUT)] _EnableDistortion ("Enabled?", Float) = 0
		[ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6)] _DistortionUvToDistort ("Distorted UV", Int) = 0
		_DistortionMask ("Mask--{reference_properties:[_DistortionMaskPan, _DistortionMaskUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_DistortionMaskPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6)] _DistortionMaskUV ("UV", Int) = 0
		_DistortionFlowTexture ("Distortion Texture 1--{reference_properties:[_DistortionFlowTexturePan, _DistortionFlowTextureUV]}", 2D) = "black" { }
		[HideInInspector][Vector2]_DistortionFlowTexturePan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6)] _DistortionFlowTextureUV ("UV", Int) = 0
		_DistortionFlowTexture1 ("Distortion Texture 2--{reference_properties:[_DistortionFlowTexture1Pan, _DistortionFlowTexture1UV]}", 2D) = "black" { }
		[HideInInspector][Vector2]_DistortionFlowTexture1Pan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6)] _DistortionFlowTexture1UV ("UV", Int) = 0
		_DistortionStrength ("Strength1", Float) = 0.03
		_DistortionStrength1 ("Strength2", Float) = 0.01
		[HideInInspector] m_start_DistortionAudioLink ("Audio Link ♫--{reference_property:_EnableDistortionAudioLink, condition_showS:_EnableAudioLink==1}", Float) = 0
		[HideInInspector][ToggleUI] _EnableDistortionAudioLink ("Enabled?", Float) = 0
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _DistortionStrengthAudioLinkBand ("Strength 1 Band", Int) = 0
		[Vector2]_DistortionStrengthAudioLink ("Strength 1 Offset Range", Vector) = (0, 0, 0, 0)
		[Enum(Bass, 0, Low Mid, 1, High Mid, 2, Treble, 3)] _DistortionStrength1AudioLinkBand ("Strength 2 Band", Int) = 0
		[Vector2]_DistortionStrength1AudioLink ("Strength 2 Offset Range", Vector) = (0, 0, 0, 0)
		[HideInInspector] m_end_DistortionAudioLink ("Audio Link", Float) = 0
		[HideInInspector] m_end_uvDistortion ("Distortion UV", Float) = 0
		[HideInInspector] m_start_uvPanosphere ("Panosphere UV", Float) = 0
		[ToggleUI] _StereoEnabled ("Stereo Enabled", Float) = 0
		[ToggleUI] _PanoUseBothEyes ("Perspective Correct (VR)", Float) = 1
		[HideInInspector] m_end_uvPanosphere ("Panosphere UV", Float) = 0
		[HideInInspector] m_start_uvPolar ("Polar UV", Float) = 0
		[ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5)] _PolarUV ("UV", Int) = 0
		[Vector2]_PolarCenter ("Center Coordinate", Vector) = (.5, .5, 0, 0)
		_PolarRadialScale ("Radial Scale", Float) = 1
		_PolarLengthScale ("Length Scale", Float) = 1
		_PolarSpiralPower ("Spiral Power", Float) = 0
		[HideInInspector] m_end_uvPolar ("Polar UV", Float) = 0
		[HideInInspector] m_start_parallax (" Parallax Heightmapping--{reference_property:_PoiParallax}", Float) = 0
		[HideInInspector][ThryToggle(POI_PARALLAX)]_PoiParallax ("Enable", Float) = 0
		[ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_ParallaxUV ("Applies To: ", Int) = 0
		[ThryTexture]_HeightMap ("Heightmap--{reference_properties:[_HeightMapPan, _HeightMapUV]}", 2D) = "white" { }
		[HideInInspector][Vector2]_HeightMapPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_HeightMapUV ("UV", Int) = 0
		[ThryTexture]_Heightmask ("Mask--{reference_properties:[_HeightmaskPan, _HeightmaskUV, _HeightmaskInvert]}", 2D) = "white" { }
		[HideInInspector][Vector2]_HeightmaskPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ToggleUI]_HeightmaskInvert ("Invert", Float) = 0
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_HeightmaskUV ("UV", Int) = 0
		_HeightStrength ("Strength", Range(0, 1)) = 0.4247461
		_CurvatureU ("Curvature U", Range(0, 100)) = 0
		_CurvatureV ("Curvature V", Range(0, 30)) = 0
		[IntRange]_HeightStepsMin ("Steps Min", Range(0, 128)) = 10
		[IntRange]_HeightStepsMax ("Steps Max", Range(0, 128)) = 128
		_CurvFix ("Curvature Bias", Range(0, 1)) = 1
		[HideInInspector] m_end_parallax ("Parallax Heightmapping", Float) = 0
		[HideInInspector] m_thirdpartyCategory ("Third Party", Float) = 0
		[HideInInspector] m_postprocessing ("Post Processing", Float) = 0
		[HideInInspector] m_start_PoiLightData ("PP Animations ", Float) = 0
		[Helpbox(1)] _PPHelp ("This section meant for real time adjustments through animations and not to be changed in unity", Int) = 0
		_PPLightingMultiplier ("Lighting Mulitplier", Float) = 1
		_PPLightingAddition ("Lighting Add", Float) = 0
		_PPEmissionMultiplier ("Emission Multiplier", Float) = 1
		_PPFinalColorMultiplier ("Final Color Multiplier", Float) = 1
		[HideInInspector] m_end_PoiLightData ("PP Animations ", Float) = 0
		[HideInInspector] m_start_postprocess ("Post Processing--{reference_property:_PostProcess}", Float) = 0
		[HideInInspector][ThryToggle(POSTPROCESS)]_PostProcess ("Enable", Float) = 0
		[ThryTexture] _PPMask("Mask--{reference_properties:[_PPMaskPan, _PPMaskUV, _PPMaskInvert]}", 2D) = "white" { }
		[HideInInspector][Vector2]_PPMaskPan ("Panning", Vector) = (0, 0, 0, 0)
		[HideInInspector][ToggleUI]_PPMaskInvert ("Invert", Float) = 0
		[HideInInspector][ThryWideEnum(UV0, 0, UV1, 1, UV2, 2, UV3, 3, Panosphere, 4, World Pos XZ, 5, Polar UV, 6, Distorted UV, 7)]_PPMaskUV ("UV", Int) = 0
		[NoScaleOffset][ThryTexture] _PPLUT("LUT", 2D) = "white" {}
		_PPLUTStrength("LUT Strength", Range(0,1)) = 0
		_PPHue("Hue", Range(0,1)) = 0
		[HDR]_PPTint("Tint", Color) = (1,1,1,1)
		[Vector3]_PPRGB("RGB", Vector) = (1,1,1,1)
		_PPContrast("Contrast", Float) = 1
		_PPSaturation("Saturation", Float) = 1
		_PPBrightness("Brightness", Float) = 1
		_PPLightness("Lightness", Float) = 0
		_PPHDR("HDR", Float) = 0
		[HideInInspector] m_end_postprocess ("", Float) = 0
		[HideInInspector] m_renderingCategory ("Rendering", Float) = 0
		[Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4
		[Enum(Off, 0, On, 1)] _ZWrite ("ZWrite", Int) = 1
		[Enum(Thry.ColorMask)] _ColorMask ("Color Mask", Int) = 15
		_OffsetFactor ("Offset Factor", Float) = 0.0
		_OffsetUnits ("Offset Units", Float) = 0.0
		[ToggleUI]_RenderingReduceClipDistance ("Reduce Clip Distance", Float) = 0
		[ToggleUI]_IgnoreFog ("Ignore Fog", Float) = 0
		[HideInInspector] Instancing ("Instancing", Float) = 0 //add this property for instancing variants settings to be shown
		[HideInInspector] m_start_blending ("Blending", Float) = 0
		[Enum(Thry.BlendOp)]_BlendOp ("RGB Blend Op", Int) = 0
		[Enum(Thry.BlendOp)]_BlendOpAlpha ("Alpha Blend Op", Int) = 0
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Source Blend", Int) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Destination Blend", Int) = 0
		[Space][ThryHeaderLabel(Additive Blending, 13)]
		[Enum(Thry.BlendOp)]_AddBlendOp ("RGB Blend Op", Int) = 0
		[Enum(Thry.BlendOp)]_AddBlendOpAlpha ("Alpha Blend Op", Int) = 0
		[Enum(UnityEngine.Rendering.BlendMode)] _AddSrcBlend ("Source Blend", Int) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _AddDstBlend ("Destination Blend", Int) = 1
		[HideInInspector] m_end_blending ("Blending", Float) = 0
		[HideInInspector] m_start_StencilPassOptions ("Stencil", Float) = 0
		[IntRange] _StencilRef ("Stencil Reference Value", Range(0, 255)) = 0
		[IntRange] _StencilReadMask ("Stencil ReadMask Value", Range(0, 255)) = 255
		[IntRange] _StencilWriteMask ("Stencil WriteMask Value", Range(0, 255)) = 255
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilPassOp ("Stencil Pass Op", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilFailOp ("Stencil Fail Op", Float) = 0
		[Enum(UnityEngine.Rendering.StencilOp)] _StencilZFailOp ("Stencil ZFail Op", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] _StencilCompareFunction ("Stencil Compare Function", Float) = 8
		[HideInInspector] m_end_StencilPassOptions ("Stencil", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "VRCFallback" = "Standard" }
		Pass
		{
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
			AlphaToMask [_AlphaToCoverage]
			ZTest [_ZTest]
			ColorMask [_ColorMask]
			Offset [_OffsetFactor], [_OffsetUnits]
			BlendOp [_BlendOp], [_BlendOpAlpha]
			Blend [_SrcBlend] [_DstBlend]
			CGPROGRAM
#define OPTIMIZER_ENABLED
#define POI_LIGHT_DATA_ADDITIVE_DIRECTIONAL_ENABLE
#define POI_LIGHT_DATA_ADDITIVE_ENABLE
#define POI_VERTEXLIGHT_ON
#define VIGNETTE_CLASSIC
#define VIGNETTE_MASKED
#define _LIGHTINGMODE_MULTILAYER_MATH
#define _RIMSTYLE_POIYOMI
#define _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
#define _TPS_VERTEXCOLORS_ON
#define PROPSHADER_MASTER_LABEL 0
#define PROPSHADER_IS_USING_THRY_EDITOR 69
#define PROPFOOTER_YOUTUBE 0
#define PROPFOOTER_TWITTER 0
#define PROPFOOTER_PATREON 0
#define PROPFOOTER_DISCORD 0
#define PROPFOOTER_GITHUB 0
#define PROP_FORGOTTOLOCKMATERIAL 1
#define PROP_SHADEROPTIMIZERENABLED 0
#define PROP_LOCKTOOLTIP 0
#define PROP_MODE 0
#define PROPM_MAINCATEGORY 0
#define PROP_COLORTHEMEINDEX 0
#define PROP_MAINTEX
#define PROP_MAINTEXUV 0
#define PROP_BUMPMAP
#define PROP_BUMPMAPUV 0
#define PROP_BUMPSCALE 1
#define PROP_CLIPPINGMASKUV 0
#define PROP_INVERSE_CLIPPING 0
#define PROP_CUTOFF 0.5
#define PROPM_START_MAINHUESHIFT 0
#define PROP_MAINCOLORADJUSTTOGGLE 0
#define PROP_MAINCOLORADJUSTTEXTUREUV 0
#define PROP_SATURATION 0
#define PROP_MAINBRIGHTNESS 0
#define PROP_MAINHUESHIFTTOGGLE 0
#define PROP_MAINHUESHIFTREPLACE 1
#define PROP_MAINHUESHIFT 0
#define PROP_MAINHUESHIFTSPEED 0
#define PROP_MAINHUEALCTENABLED 0
#define PROP_MAINALHUESHIFTBAND 0
#define PROP_MAINALHUESHIFTCTINDEX 0
#define PROP_MAINHUEALMOTIONSPEED 1
#define PROPM_END_MAINHUESHIFT 0
#define PROPM_START_ALPHA 0
#define PROP_ALPHAFORCEOPAQUE 0
#define PROP_ALPHAMOD 0
#define PROP_ALPHAPREMULTIPLY 0
#define PROP_ALPHATOCOVERAGE 0
#define PROP_ALPHASHARPENEDA2C 0
#define PROP_ALPHAMIPSCALE 0.25
#define PROP_ALPHADITHERING 0
#define PROP_ALPHADITHERGRADIENT 0.1
#define PROP_ALPHADISTANCEFADE 0
#define PROP_ALPHADISTANCEFADETYPE 1
#define PROP_ALPHADISTANCEFADEMINALPHA 0
#define PROP_ALPHADISTANCEFADEMAXALPHA 1
#define PROP_ALPHADISTANCEFADEMIN 0
#define PROP_ALPHADISTANCEFADEMAX 0
#define PROP_ALPHAFRESNEL 0
#define PROP_ALPHAFRESNELALPHA 0
#define PROP_ALPHAFRESNELSHARPNESS 0.5
#define PROP_ALPHAFRESNELWIDTH 0.5
#define PROP_ALPHAFRESNELINVERT 0
#define PROP_ALPHAANGULAR 0
#define PROP_ANGLETYPE 0
#define PROP_ANGLECOMPARETO 0
#define PROP_CAMERAANGLEMIN 45
#define PROP_CAMERAANGLEMAX 90
#define PROP_MODELANGLEMIN 45
#define PROP_MODELANGLEMAX 90
#define PROP_ANGLEMINALPHA 0
#define PROP_ALPHAAUDIOLINKENABLED 0
#define PROP_ALPHAAUDIOLINKADDBAND 0
#define PROPM_END_ALPHA 0
#define PROPM_START_DETAILOPTIONS 0
#define PROP_DETAILENABLED 0
#define PROP_DETAILMASKUV 0
#define PROP_DETAILTINTTHEMEINDEX 0
#define PROP_DETAILTEXUV 0
#define PROP_DETAILTEXINTENSITY 1
#define PROP_DETAILBRIGHTNESS 1
#define PROP_DETAILNORMALMAPSCALE 1
#define PROP_DETAILNORMALMAPUV 0
#define PROPM_END_DETAILOPTIONS 0
#define PROPM_START_VERTEXMANIPULATION 0
#define PROP_VERTEXMANIPULATIONSENABLED 0
#define PROP_VERTEXMANIPULATIONHEIGHT 0
#define PROP_VERTEXMANIPULATIONHEIGHTMASKUV 0
#define PROP_VERTEXMANIPULATIONHEIGHTBIAS 0
#define PROP_VERTEXROUNDINGENABLED 0
#define PROP_VERTEXROUNDINGDIVISION 500
#define PROP_VERTEXAUDIOLINKENABLED 0
#define PROP_VERTEXLOCALTRANSLATIONALBAND 0
#define PROP_VERTEXLOCALROTATIONALBANDX 0
#define PROP_VERTEXLOCALROTATIONALBANDY 0
#define PROP_VERTEXLOCALROTATIONALBANDZ 0
#define PROP_VERTEXLOCALROTATIONCTALBANDX 0
#define PROP_VERTEXLOCALROTATIONCTALTYPEX 0
#define PROP_VERTEXLOCALROTATIONCTALBANDY 0
#define PROP_VERTEXLOCALROTATIONCTALTYPEY 0
#define PROP_VERTEXLOCALROTATIONCTALBANDZ 0
#define PROP_VERTEXLOCALROTATIONCTALTYPEZ 0
#define PROP_VERTEXLOCALSCALEALBAND 0
#define PROP_VERTEXWORLDTRANSLATIONALBAND 0
#define PROP_VERTEXMANIPULATIONHEIGHTBAND 0
#define PROP_VERTEXROUNDINGRANGEBAND 0
#define PROPM_END_VERTEXMANIPULATION 0
#define PROPM_START_MAINVERTEXCOLORS 0
#define PROP_MAINVERTEXCOLORINGLINEARSPACE 1
#define PROP_MAINVERTEXCOLORING 0
#define PROP_MAINUSEVERTEXCOLORALPHA 0
#define PROPM_END_MAINVERTEXCOLORS 0
#define PROPM_START_BACKFACE 0
#define PROP_BACKFACEENABLED 0
#define PROP_BACKFACECOLORTHEMEINDEX 0
#define PROP_BACKFACEEMISSIONSTRENGTH 0
#define PROP_BACKFACEALPHA 1
#define PROP_BACKFACETEXTUREUV 0
#define PROP_BACKFACEMASKUV 0
#define PROP_BACKFACEDETAILINTENSITY 1
#define PROP_BACKFACEREPLACEALPHA 0
#define PROP_BACKFACEEMISSIONLIMITER 1
#define PROP_BACKFACEHUESHIFTENABLED 0
#define PROP_BACKFACEHUESHIFT 0
#define PROP_BACKFACEHUESHIFTSPEED 0
#define PROPM_END_BACKFACE 0
#define PROPM_START_RGBMASK 0
#define PROP_RGBMASKENABLED 0
#define PROP_RGBUSEVERTEXCOLORS 0
#define PROP_RGBBLENDMULTIPLICATIVE 0
#define PROP_RGBMASKUV 0
#define PROP_REDCOLORTHEMEINDEX 0
#define PROP_REDTEXTUREUV 0
#define PROP_GREENCOLORTHEMEINDEX 0
#define PROP_GREENTEXTUREUV 0
#define PROP_BLUECOLORTHEMEINDEX 0
#define PROP_BLUETEXTUREUV 0
#define PROP_ALPHACOLORTHEMEINDEX 0
#define PROP_ALPHATEXTUREUV 0
#define PROP_RGBNORMALSENABLED 0
#define PROP_RGBNORMALBLEND 0
#define PROP_RGBNORMALRUV 0
#define PROP_RGBNORMALRSCALE 0
#define PROP_RGBNORMALGUV 0
#define PROP_RGBNORMALGSCALE 0
#define PROP_RGBNORMALBUV 0
#define PROP_RGBNORMALBSCALE 0
#define PROP_RGBNORMALAUV 0
#define PROP_RGBNORMALASCALE 0
#define PROPM_END_RGBMASK 0
#define PROPM_START_DECALSECTION 0
#define PROP_DECALMASKUV 0
#define PROP_DECALTPSDEPTHMASKENABLED 0
#define PROP_DECAL0TPSMASKSTRENGTH 1
#define PROP_DECAL1TPSMASKSTRENGTH 1
#define PROP_DECAL2TPSMASKSTRENGTH 1
#define PROP_DECAL3TPSMASKSTRENGTH 1
#define PROPM_START_DECAL0 0
#define PROP_DECALENABLED 0
#define PROP_DECAL0MASKCHANNEL 0
#define PROP_DECALCOLORTHEMEINDEX 0
#define PROP_DECALEMISSIONSTRENGTH 0
#define PROP_DECALTEXTUREUV 0
#define PROP_DECALTILED 0
#define PROP_DECAL0DEPTH 0
#define PROP_DECALROTATION 0
#define PROP_DECALROTATIONSPEED 0
#define PROP_DECALBLENDTYPE 0
#define PROP_DECALBLENDALPHA 1
#define PROP_DECALOVERRIDEALPHA 0
#define PROP_DECALHUESHIFTENABLED 0
#define PROP_DECALHUESHIFTSPEED 0
#define PROP_DECALHUESHIFT 0
#define PROP_DECAL0HUEANGLESTRENGTH 0
#define PROPM_START_DECAL0AUDIOLINK 0
#define PROP_AUDIOLINKDECAL0SCALEBAND 0
#define PROP_AUDIOLINKDECAL0SIDEBAND 0
#define PROP_AUDIOLINKDECAL0ROTATIONBAND 0
#define PROP_AUDIOLINKDECAL0ALPHABAND 0
#define PROP_AUDIOLINKDECAL0EMISSIONBAND 0
#define PROP_AUDIOLINKDECALCC0 0
#define PROP_DECALROTATIONCTALBAND0 0
#define PROP_DECALROTATIONCTALTYPE0 0
#define PROP_DECALROTATIONCTALSPEED0 0
#define PROPM_END_DECAL0AUDIOLINK 0
#define PROPM_END_DECAL0 0
#define PROPM_START_DECAL1 0
#define PROP_DECALENABLED1 0
#define PROP_DECAL1MASKCHANNEL 1
#define PROP_DECALCOLOR1THEMEINDEX 0
#define PROP_DECALEMISSIONSTRENGTH1 0
#define PROP_DECALTEXTURE1UV 0
#define PROP_DECALTILED1 0
#define PROP_DECAL1DEPTH 0
#define PROP_DECALROTATION1 0
#define PROP_DECALROTATIONSPEED1 0
#define PROP_DECALBLENDTYPE1 0
#define PROP_DECALBLENDALPHA1 1
#define PROP_DECALOVERRIDEALPHA1 0
#define PROP_DECALHUESHIFTENABLED1 0
#define PROP_DECALHUESHIFTSPEED1 0
#define PROP_DECALHUESHIFT1 0
#define PROP_DECAL1HUEANGLESTRENGTH 0
#define PROPM_START_DECAL1AUDIOLINK 0
#define PROP_AUDIOLINKDECAL1SCALEBAND 0
#define PROP_AUDIOLINKDECAL1SIDEBAND 0
#define PROP_AUDIOLINKDECAL1ROTATIONBAND 0
#define PROP_AUDIOLINKDECAL1ALPHABAND 0
#define PROP_AUDIOLINKDECAL1EMISSIONBAND 0
#define PROP_AUDIOLINKDECALCC1 0
#define PROP_DECALROTATIONCTALBAND1 0
#define PROP_DECALROTATIONCTALTYPE1 0
#define PROP_DECALROTATIONCTALSPEED1 0
#define PROPM_END_DECAL1AUDIOLINK 0
#define PROPM_END_DECAL1 0
#define PROPM_START_DECAL2 0
#define PROP_DECALENABLED2 0
#define PROP_DECAL2MASKCHANNEL 2
#define PROP_DECALCOLOR2THEMEINDEX 0
#define PROP_DECALEMISSIONSTRENGTH2 0
#define PROP_DECALTEXTURE2UV 0
#define PROP_DECALTILED2 0
#define PROP_DECAL2DEPTH 0
#define PROP_DECALROTATION2 0
#define PROP_DECALROTATIONSPEED2 0
#define PROP_DECALBLENDTYPE2 0
#define PROP_DECALBLENDALPHA2 1
#define PROP_DECALOVERRIDEALPHA2 0
#define PROP_DECALHUESHIFTENABLED2 0
#define PROP_DECALHUESHIFTSPEED2 0
#define PROP_DECALHUESHIFT2 0
#define PROP_DECAL2HUEANGLESTRENGTH 0
#define PROPM_START_DECAL2AUDIOLINK 0
#define PROP_AUDIOLINKDECAL2SCALEBAND 0
#define PROP_AUDIOLINKDECAL2SIDEBAND 0
#define PROP_AUDIOLINKDECAL2ROTATIONBAND 0
#define PROP_AUDIOLINKDECAL2ALPHABAND 0
#define PROP_AUDIOLINKDECAL2EMISSIONBAND 0
#define PROP_AUDIOLINKDECALCC2 0
#define PROP_DECALROTATIONCTALBAND2 0
#define PROP_DECALROTATIONCTALTYPE2 0
#define PROP_DECALROTATIONCTALSPEED2 0
#define PROPM_END_DECAL2AUDIOLINK 0
#define PROPM_END_DECAL2 0
#define PROPM_START_DECAL3 0
#define PROP_DECALENABLED3 0
#define PROP_DECAL3MASKCHANNEL 3
#define PROP_DECALCOLOR3THEMEINDEX 0
#define PROP_DECALEMISSIONSTRENGTH3 0
#define PROP_DECALTEXTURE3UV 0
#define PROP_DECALTILED3 0
#define PROP_DECAL3DEPTH 0
#define PROP_DECALROTATION3 0
#define PROP_DECALROTATIONSPEED3 0
#define PROP_DECALBLENDTYPE3 0
#define PROP_DECALBLENDALPHA3 1
#define PROP_DECALOVERRIDEALPHA3 0
#define PROP_DECALHUESHIFTENABLED3 0
#define PROP_DECALHUESHIFTSPEED3 0
#define PROP_DECALHUESHIFT3 0
#define PROP_DECAL3HUEANGLESTRENGTH 0
#define PROPM_START_DECAL3AUDIOLINK 0
#define PROP_AUDIOLINKDECAL3SCALEBAND 0
#define PROP_AUDIOLINKDECAL3SIDEBAND 0
#define PROP_AUDIOLINKDECAL3ROTATIONBAND 0
#define PROP_AUDIOLINKDECAL3ALPHABAND 0
#define PROP_AUDIOLINKDECAL3EMISSIONBAND 0
#define PROP_AUDIOLINKDECALCC3 0
#define PROP_DECALROTATIONCTALBAND3 0
#define PROP_DECALROTATIONCTALTYPE3 0
#define PROP_DECALROTATIONCTALSPEED3 0
#define PROPM_END_DECAL3AUDIOLINK 0
#define PROPM_END_DECAL3 0
#define PROPM_END_DECALSECTION 0
#define PROPM_START_TPS_PENETRATOR 0
#define PROPM_START_PEN_AUTOCONFIG 0
#define PROP_TPS_PENETRATORLENGTH 1
#define PROPM_END_PEN_AUTOCONFIG 0
#define PROP_TPSHELPBOX 0
#define PROP_TPSPENETRATORENABLED 0
#define PROP_TPSBEZIERHEADER 0
#define PROP_TPS_BEZIERSTART 0
#define PROP_TPS_BEZIERSMOOTHNESS 0.09
#define PROP_TPSSQUEEZEHEADER 0
#define PROP_TPS_SQUEEZE 0.3
#define PROP_TPS_SQUEEZEDISTANCE 0.2
#define PROP_TPSBULDGEHEADER 0
#define PROP_TPS_BULDGE 0.3
#define PROP_TPS_BULDGEDISTANCE 0.2
#define PROP_TPS_BULDGEFALLOFFDISTANCE 0.05
#define PROP_TPSPULSINGHEADER 0
#define PROP_TPS_PUMPINGSTRENGTH 0
#define PROP_TPS_PUMPINGSPEED 0
#define PROP_TPS_PUMPINGWIDTH 0.2
#define PROP_TPSIDLEHEADER 0
#define PROP_TPS_IDLEGRAVITY 0
#define PROP_TPS_IDLESKRINKWIDTH 1
#define PROP_TPS_IDLESKRINKLENGTH 1
#define PROP_TPS_IDLEMOVEMENTSTRENGTH 0
#define PROP_TPS_VERTEXCOLORS 1
#define PROP_TPS2_BUFFEREDDEPTH 0
#define PROP_TPS2_BUFFEREDSTRENGTH 0
#define PROPM_END_TPS_PENETRATOR 0
#define PROPM_START_GLOBALTHEMES 0
#define PROPM_END_GLOBALTHEMES 0
#define PROPM_LIGHTINGCATEGORY 0
#define PROPM_START_POILIGHTDATA 0
#define PROP_LIGHTINGAOMAPSUV 0
#define PROP_LIGHTDATAAOSTRENGTHR 1
#define PROP_LIGHTDATAAOSTRENGTHG 0
#define PROP_LIGHTDATAAOSTRENGTHB 0
#define PROP_LIGHTDATAAOSTRENGTHA 0
#define PROP_LIGHTINGDETAILSHADOWMAPSUV 0
#define PROP_LIGHTINGDETAILSHADOWSTRENGTHR 1
#define PROP_LIGHTINGDETAILSHADOWSTRENGTHG 0
#define PROP_LIGHTINGDETAILSHADOWSTRENGTHB 0
#define PROP_LIGHTINGDETAILSHADOWSTRENGTHA 0
#define PROP_LIGHTINGSHADOWMASKSUV 0
#define PROP_LIGHTINGSHADOWMASKSTRENGTHR 1
#define PROP_LIGHTINGSHADOWMASKSTRENGTHG 0
#define PROP_LIGHTINGSHADOWMASKSTRENGTHB 0
#define PROP_LIGHTINGSHADOWMASKSTRENGTHA 0
#define PROP_LIGHTINGCOLORMODE 0
#define PROP_LIGHTINGMAPMODE 0
#define PROP_LIGHTINGDIRECTIONMODE 0
#define PROP_LIGHTINGFORCECOLORENABLED 0
#define PROP_LIGHTINGFORCEDCOLORTHEMEINDEX 0
#define PROP_UNLIT_INTENSITY 1
#define PROP_LIGHTINGCAPENABLED 1
#define PROP_LIGHTINGCAP 1
#define PROP_LIGHTINGMINLIGHTBRIGHTNESS 0
#define PROP_LIGHTINGINDIRECTUSESNORMALS 0
#define PROP_LIGHTINGCASTEDSHADOWS 0
#define PROP_LIGHTINGMONOCHROMATIC 0
#define PROP_LIGHTINGADDITIVEENABLE 1
#define PROP_DISABLEDIRECTIONALINADD 1
#define PROP_LIGHTINGADDITIVELIMITED 0
#define PROP_LIGHTINGADDITIVELIMIT 1
#define PROP_LIGHTINGADDITIVEMONOCHROMATIC 0
#define PROP_LIGHTINGADDITIVEPASSTHROUGH 0.5
#define PROP_LIGHTINGVERTEXLIGHTINGENABLED 1
#define PROP_LIGHTDATADEBUGENABLED 0
#define PROP_LIGHTINGDEBUGVISUALIZE 0
#define PROPM_END_POILIGHTDATA 0
#define PROPM_START_POISHADING 0
#define PROP_SHADINGENABLED 1
#define PROP_LIGHTINGMODE 1
#define PROP_TOONRAMP
#define PROP_SHADOWOFFSET 0
#define PROP_LIGHTINGWRAPPEDWRAP 0
#define PROP_LIGHTINGWRAPPEDNORMALIZATION 0
#define PROP_SHADOWCOLORTEXUV 0
#define PROP_SHADOWBORDER 0.5
#define PROP_SHADOWBLUR 0.1
#define PROP_SHADOW2NDCOLORTEXUV 0
#define PROP_SHADOW2NDBORDER 0.5
#define PROP_SHADOW2NDBLUR 0.3
#define PROP_SHADOW3RDCOLORTEXUV 0
#define PROP_SHADOW3RDBORDER 0.25
#define PROP_SHADOW3RDBLUR 0.1
#define PROP_SHADOWBORDERRANGE 0
#define PROP_LIGHTINGGRADIENTSTART 0
#define PROP_LIGHTINGGRADIENTEND 0.5
#define PROP_1ST_SHADEMAPUV 0
#define PROP_USE_1STSHADEMAPALPHA_AS_SHADOWMASK 0
#define PROP_1STSHADEMAPMASK_INVERSE 0
#define PROP_USE_BASEAS1ST 0
#define PROP_2ND_SHADEMAPUV 0
#define PROP_USE_2NDSHADEMAPALPHA_AS_SHADOWMASK 0
#define PROP_2NDSHADEMAPMASK_INVERSE 0
#define PROP_USE_1STAS2ND 0
#define PROP_BASECOLOR_STEP 0.5
#define PROP_BASESHADE_FEATHER 0.0001
#define PROP_SHADECOLOR_STEP 0
#define PROP_1ST2ND_SHADES_FEATHER 0.0001
#define PROP_SHADINGSHADEMAPBLENDTYPE 0
#define PROP_SKINLUT
#define PROP_SSSSCALE 1
#define PROP_SSSBUMPBLUR 0.7
#define PROP_CLOTHDFG
#define PROP_CLOTHMETALLICSMOOTHNESSMAPINVERT 0
#define PROP_CLOTHMETALLICSMOOTHNESSMAPUV 0
#define PROP_CLOTHREFLECTANCE 0.5
#define PROP_CLOTHSMOOTHNESS 0.5
#define PROP_SHADOWSTRENGTH 1
#define PROP_LIGHTINGIGNOREAMBIENTCOLOR 0
#define PROP_LIGHTINGADDITIVETYPE 1
#define PROP_LIGHTINGADDITIVEGRADIENTSTART 0
#define PROP_LIGHTINGADDITIVEGRADIENTEND 0.5
#define PROPM_END_POISHADING 0
#define PROPM_START_ANISO 0
#define PROP_ENABLEANISO 0
#define PROP_ANISOCOLORMAPUV 0
#define PROP_ANISOUSELIGHTCOLOR 1
#define PROP_ANISOUSEBASECOLOR 0
#define PROP_ANISOREPLACE 0
#define PROP_ANISOADD 1
#define PROP_ANISOHIDEINSHADOW 1
#define PROP_ANISO0POWER 0
#define PROP_ANISO0STRENGTH 1
#define PROP_ANISO0OFFSET 0
#define PROP_ANISO0OFFSETMAPSTRENGTH 0
#define PROP_ANISO0TINTINDEX 0
#define PROP_ANISO0TOONMODE 0
#define PROP_ANISO0EDGE 0.5
#define PROP_ANISO0BLUR 0
#define PROP_ANISO1POWER 0.1
#define PROP_ANISO1STRENGTH 1
#define PROP_ANISO1OFFSET 0
#define PROP_ANISO1OFFSETMAPSTRENGTH 0
#define PROP_ANISO1TINTINDEX 0
#define PROP_ANISO1TOONMODE 0
#define PROP_ANISO1EDGE 0.5
#define PROP_ANISO1BLUR 0
#define PROP_ANISODEBUGTOGGLE 0
#define PROP_ANISODEBUGMODE 0
#define PROPM_END_ANSIO 0
#define PROPM_START_MATCAP 0
#define PROP_MATCAPENABLE 0
#define PROP_MATCAPUVMODE 1
#define PROP_MATCAPCOLORTHEMEINDEX 0
#define PROP_MATCAPBORDER 0.43
#define PROP_MATCAPMASKUV 0
#define PROP_MATCAPMASKINVERT 0
#define PROP_MATCAPEMISSIONSTRENGTH 0
#define PROP_MATCAPINTENSITY 1
#define PROP_MATCAPLIGHTMASK 0
#define PROP_MATCAPREPLACE 1
#define PROP_MATCAPMULTIPLY 0
#define PROP_MATCAPADD 0
#define PROP_MATCAPMIXED 0
#define PROP_MATCAPADDTOLIGHT 0
#define PROP_MATCAPALPHAOVERRIDE 0
#define PROP_MATCAPNORMAL 1
#define PROP_MATCAP0CUSTOMNORMAL 0
#define PROP_MATCAP0NORMALMAPUV 0
#define PROP_MATCAP0NORMALMAPSCALE 1
#define PROP_MATCAPHUESHIFTENABLED 0
#define PROP_MATCAPHUESHIFTSPEED 0
#define PROP_MATCAPHUESHIFT 0
#define PROP_MATCAPTPSDEPTHENABLED 0
#define PROP_MATCAPTPSMASKSTRENGTH 1
#define PROPM_END_MATCAP 0
#define PROPM_START_MATCAP2 0
#define PROP_MATCAP2ENABLE 0
#define PROP_MATCAP2UVMODE 1
#define PROP_MATCAP2COLORTHEMEINDEX 0
#define PROP_MATCAP2BORDER 0.43
#define PROP_MATCAP2MASKUV 0
#define PROP_MATCAP2MASKINVERT 0
#define PROP_MATCAP2EMISSIONSTRENGTH 0
#define PROP_MATCAP2INTENSITY 1
#define PROP_MATCAP2LIGHTMASK 0
#define PROP_MATCAP2REPLACE 0
#define PROP_MATCAP2MULTIPLY 0
#define PROP_MATCAP2ADD 0
#define PROP_MATCAP2MIXED 0
#define PROP_MATCAP2ADDTOLIGHT 0
#define PROP_MATCAP2ALPHAOVERRIDE 0
#define PROP_MATCAP2NORMAL 1
#define PROP_MATCAP1CUSTOMNORMAL 0
#define PROP_MATCAP1CUSTOMNORMAL 0
#define PROP_MATCAP1NORMALMAPUV 0
#define PROP_MATCAP1NORMALMAPSCALE 1
#define PROP_MATCAP2HUESHIFTENABLED 0
#define PROP_MATCAP2HUESHIFTSPEED 0
#define PROP_MATCAP2HUESHIFT 0
#define PROP_MATCAP2TPSDEPTHENABLED 0
#define PROP_MATCAP2TPSMASKSTRENGTH 1
#define PROPM_END_MATCAP2 0
#define PROPM_START_CUBEMAP 0
#define PROP_CUBEMAPENABLED 0
#define PROP_CUBEMAPUVMODE 1
#define PROP_CUBEMAPCOLORTHEMEINDEX 0
#define PROP_CUBEMAP
#define PROP_CUBEMAPMASKUV 0
#define PROP_CUBEMAPMASKINVERT 0
#define PROP_CUBEMAPEMISSIONSTRENGTH 0
#define PROP_CUBEMAPINTENSITY 1
#define PROP_CUBEMAPLIGHTMASK 0
#define PROP_CUBEMAPREPLACE 1
#define PROP_CUBEMAPMULTIPLY 0
#define PROP_CUBEMAPADD 0
#define PROP_CUBEMAPNORMAL 1
#define PROP_CUBEMAPHUESHIFTENABLED 0
#define PROP_CUBEMAPHUESHIFTSPEED 0
#define PROP_CUBEMAPHUESHIFT 0
#define PROPM_END_CUBEMAP 0
#define PROPM_START_RIMLIGHTOPTIONS 0
#define PROP_ENABLERIMLIGHTING 0
#define PROP_RIMSTYLE 0
#define PROP_RIMTEXUV 0
#define PROP_RIMMASKUV 0
#define PROP_IS_NORMALMAPTORIMLIGHT 1
#define PROP_RIMLIGHTINGINVERT 0
#define PROP_RIMLIGHTCOLORTHEMEINDEX 0
#define PROP_RIMWIDTH 0.8
#define PROP_RIMSHARPNESS 0.25
#define PROP_RIMPOWER 1
#define PROP_RIMSTRENGTH 0
#define PROP_RIMBASECOLORMIX 0
#define PROP_RIMBLENDMODE 0
#define PROP_RIMBLENDSTRENGTH 1
#define PROP_IS_LIGHTCOLOR_RIMLIGHT 1
#define PROP_RIMLIGHT_POWER 0.1
#define PROP_RIMLIGHT_INSIDEMASK 0.0001
#define PROP_RIMLIGHT_FEATHEROFF 0
#define PROP_LIGHTDIRECTION_MASKON 0
#define PROP_TWEAK_LIGHTDIRECTION_MASKLEVEL 0
#define PROP_ADD_ANTIPODEAN_RIMLIGHT 0
#define PROP_IS_LIGHTCOLOR_AP_RIMLIGHT 1
#define PROP_RIMAPCOLORTHEMEINDEX 0
#define PROP_AP_RIMLIGHT_POWER 0.1
#define PROP_AP_RIMLIGHT_FEATHEROFF 0
#define PROP_TWEAK_RIMLIGHTMASKLEVEL 0
#define PROP_RIMSHADOWTOGGLE 0
#define PROP_RIMSHADOWMASKRAMPTYPE 0
#define PROP_RIMSHADOWMASKSTRENGTH 1
#define PROP_RIMSHADOWWIDTH 0
#define PROP_RIMHUESHIFTENABLED 0
#define PROP_RIMHUESHIFTSPEED 0
#define PROP_RIMHUESHIFT 0
#define PROPM_START_RIMAUDIOLINK 0
#define PROP_AUDIOLINKRIMWIDTHBAND 0
#define PROP_AUDIOLINKRIMEMISSIONBAND 0
#define PROP_AUDIOLINKRIMBRIGHTNESSBAND 0
#define PROPM_END_RIMAUDIOLINK 0
#define PROPM_END_RIMLIGHTOPTIONS 0
#define PROPM_START_DEPTHRIMLIGHTOPTIONS 0
#define PROP_ENABLEDEPTHRIMLIGHTING 0
#define PROP_DEPTHRIMNORMALTOUSE 1
#define PROP_DEPTHRIMWIDTH 0.2
#define PROP_DEPTHRIMSHARPNESS 0.2
#define PROP_DEPTHRIMHIDEINSHADOW 0
#define PROP_DEPTHRIMMIXBASECOLOR 0
#define PROP_DEPTHRIMMIXLIGHTCOLOR 0
#define PROP_DEPTHRIMCOLORTHEMEINDEX 0
#define PROP_DEPTHRIMEMISSION 0
#define PROP_DEPTHRIMREPLACE 0
#define PROP_DEPTHRIMADD 0
#define PROP_DEPTHRIMMULTIPLY 0
#define PROP_DEPTHRIMADDITIVELIGHTING 0
#define PROPM_END_DEPTHRIMLIGHTOPTIONS 0
#define PROPM_START_BRDF 1
#define PROP_MOCHIEBRDF 0
#define PROP_MOCHIEREFLECTIONSTRENGTH 1
#define PROP_MOCHIESPECULARSTRENGTH 1
#define PROP_MOCHIEMETALLICMULTIPLIER 0
#define PROP_MOCHIEROUGHNESSMULTIPLIER 1
#define PROP_MOCHIEREFLECTIONTINTTHEMEINDEX 0
#define PROP_MOCHIESPECULARTINTTHEMEINDEX 0
#define PROP_MOCHIEMETALLICMAPSUV 0
#define PROP_MOCHIEMETALLICMAPINVERT 0
#define PROP_MOCHIEROUGHNESSMAPINVERT 0
#define PROP_MOCHIEREFLECTIONMASKINVERT 0
#define PROP_MOCHIESPECULARMASKINVERT 0
#define PROP_PBRSPLITMASKSAMPLE 0
#define PROP_MOCHIEMETALLICMASKSUV 0
#define PROP_SPECULAR2NDLAYER 0
#define PROP_MOCHIESPECULARSTRENGTH2 1
#define PROP_MOCHIEROUGHNESSMULTIPLIER2 1
#define PROP_BRDFTPSDEPTHENABLED 0
#define PROP_BRDFTPSREFLECTIONMASKSTRENGTH 1
#define PROP_BRDFTPSSPECULARMASKSTRENGTH 1
#define PROP_IGNORECASTEDSHADOWS 0
#define PROP_MOCHIEFORCEFALLBACK 0
#define PROP_MOCHIELITFALLBACK 0
#define PROP_MOCHIEGSAAENABLED 1
#define PROP_POIGSAAVARIANCE 0.15
#define PROP_POIGSAATHRESHOLD 0.1
#define PROP_REFSPECFRESNEL 1
#define PROPM_END_BRDF 0
#define PROPM_START_CLEARCOAT 0
#define PROP_CLEARCOATBRDF 0
#define PROP_CLEARCOATSTRENGTH 1
#define PROP_CLEARCOATSMOOTHNESS 1
#define PROP_CLEARCOATREFLECTIONSTRENGTH 1
#define PROP_CLEARCOATSPECULARSTRENGTH 1
#define PROP_CLEARCOATREFLECTIONTINTTHEMEINDEX 0
#define PROP_CLEARCOATSPECULARTINTTHEMEINDEX 0
#define PROP_CLEARCOATMAPSUV 0
#define PROP_CLEARCOATMASKINVERT 0
#define PROP_CLEARCOATSMOOTHNESSMAPINVERT 0
#define PROP_CLEARCOATREFLECTIONMASKINVERT 0
#define PROP_CLEARCOATSPECULARMASKINVERT 0
#define PROP_CLEARCOATFORCEFALLBACK 0
#define PROP_CLEARCOATLITFALLBACK 0
#define PROP_CCIGNORECASTEDSHADOWS 0
#define PROP_CLEARCOATGSAAENABLED 1
#define PROP_CLEARCOATGSAAVARIANCE 0.15
#define PROP_CLEARCOATGSAATHRESHOLD 0.1
#define PROP_CLEARCOATTPSDEPTHMASKENABLED 0
#define PROP_CLEARCOATTPSMASKSTRENGTH 1
#define PROPM_END_CLEARCOAT 0
#define PROPM_START_REFLECTIONRIM 0
#define PROP_ENABLEENVIRONMENTALRIM 0
#define PROP_RIMENVIROMASKUV 0
#define PROP_RIMENVIROBLUR 0.7
#define PROP_RIMENVIROWIDTH 0.45
#define PROP_RIMENVIROSHARPNESS 0
#define PROP_RIMENVIROMINBRIGHTNESS 0
#define PROP_RIMENVIROINTENSITY 1
#define PROPM_END_REFLECTIONRIM 0
#define PROPM_START_STYLIZEDSPEC 0
#define PROP_STYLIZEDSPECULAR 0
#define PROP_HIGHCOLOR_TEXUV 0
#define PROP_HIGHCOLORTHEMEINDEX 0
#define PROP_SET_HIGHCOLORMASKUV 0
#define PROP_TWEAK_HIGHCOLORMASKLEVEL 0
#define PROP_IS_SPECULARTOHIGHCOLOR 0
#define PROP_IS_BLENDADDTOHICOLOR 0
#define PROP_STYLIZEDSPECULARSTRENGTH 1
#define PROP_USELIGHTCOLOR 1
#define PROP_SSIGNORECASTEDSHADOWS 0
#define PROP_HIGHCOLOR_POWER 0.2
#define PROP_STYLIZEDSPECULARFEATHER 0
#define PROP_LAYER1STRENGTH 1
#define PROP_LAYER2SIZE 0
#define PROP_STYLIZEDSPECULAR2FEATHER 0
#define PROP_LAYER2STRENGTH 0
#define PROPM_END_STYLIZEDSPEC 0
#define PROPM_SPECIALFXCATEGORY 0
#define PROPM_START_UDIMDISCARDOPTIONS 0
#define PROP_ENABLEUDIMDISCARDOPTIONS 0
#define PROP_UDIMDISCARDUV 0
#define PROP_UDIMDISCARDMODE 1
#define PROPM_END_UDIMDISCARDOPTIONS 0
#define PROPM_START_DISSOLVE 0
#define PROP_ENABLEDISSOLVE 0
#define PROP_DISSOLVETYPE 1
#define PROP_DISSOLVEEDGEWIDTH 0.025
#define PROP_DISSOLVEEDGEHARDNESS 0.5
#define PROP_DISSOLVEEDGECOLORTHEMEINDEX 0
#define PROP_DISSOLVEEDGEEMISSION 0
#define PROP_DISSOLVETEXTURECOLORTHEMEINDEX 0
#define PROP_DISSOLVETOTEXTUREUV 0
#define PROP_DISSOLVETOEMISSIONSTRENGTH 0
#define PROP_DISSOLVENOISETEXTUREUV 0
#define PROP_DISSOLVEINVERTNOISE 0
#define PROP_DISSOLVEDETAILNOISEUV 0
#define PROP_DISSOLVEINVERTDETAILNOISE 0
#define PROP_DISSOLVEDETAILSTRENGTH 0.1
#define PROP_DISSOLVEALPHA 0
#define PROP_DISSOLVEMASKUV 0
#define PROP_DISSOLVEUSEVERTEXCOLORS 0
#define PROP_DISSOLVEMASKINVERT 0
#define PROP_CONTINUOUSDISSOLVE 0
#define PROP_ENABLEDISSOLVEAUDIOLINK 0
#define PROP_AUDIOLINKDISSOLVEALPHABAND 0
#define PROP_AUDIOLINKDISSOLVEDETAILBAND 0
#define PROPM_START_POINTTOPOINT 0
#define PROP_DISSOLVEP2PWORLDLOCAL 0
#define PROP_DISSOLVEP2PEDGELENGTH 0.1
#define PROPM_END_POINTTOPOINT 0
#define PROPM_START_DISSOLVEHUESHIFT 0
#define PROP_DISSOLVEHUESHIFTENABLED 0
#define PROP_DISSOLVEHUESHIFTSPEED 0
#define PROP_DISSOLVEHUESHIFT 0
#define PROP_DISSOLVEEDGEHUESHIFTENABLED 0
#define PROP_DISSOLVEEDGEHUESHIFTSPEED 0
#define PROP_DISSOLVEEDGEHUESHIFT 0
#define PROPM_END_DISSOLVEHUESHIFT 0
#define PROPM_START_BONUSSLIDERS 0
#define PROP_DISSOLVEALPHA0 0
#define PROP_DISSOLVEALPHA1 0
#define PROP_DISSOLVEALPHA2 0
#define PROP_DISSOLVEALPHA3 0
#define PROP_DISSOLVEALPHA4 0
#define PROP_DISSOLVEALPHA5 0
#define PROP_DISSOLVEALPHA6 0
#define PROP_DISSOLVEALPHA7 0
#define PROP_DISSOLVEALPHA8 0
#define PROP_DISSOLVEALPHA9 0
#define PROPM_END_BONUSSLIDERS 0
#define PROPM_END_DISSOLVE 0
#define PROPM_START_FLIPBOOK 0
#define PROP_ENABLEFLIPBOOK 0
#define PROP_FLIPBOOKALPHACONTROLSFINALALPHA 0
#define PROP_FLIPBOOKINTENSITYCONTROLSALPHA 0
#define PROP_FLIPBOOKCOLORREPLACES 0
#define PROP_FLIPBOOKTEXARRAYUV 0
#define PROP_FLIPBOOKMASKUV 0
#define PROP_FLIPBOOKCOLORTHEMEINDEX 0
#define PROP_FLIPBOOKTOTALFRAMES 1
#define PROP_FLIPBOOKFPS 30
#define PROP_FLIPBOOKTILED 0
#define PROP_FLIPBOOKEMISSIONSTRENGTH 0
#define PROP_FLIPBOOKROTATION 0
#define PROP_FLIPBOOKROTATIONSPEED 0
#define PROP_FLIPBOOKREPLACE 1
#define PROP_FLIPBOOKMULTIPLY 0
#define PROP_FLIPBOOKADD 0
#define PROP_FLIPBOOKMANUALFRAMECONTROL 0
#define PROP_FLIPBOOKCURRENTFRAME -1
#define PROP_FLIPBOOKCROSSFADEENABLED 0
#define PROP_FLIPBOOKHUESHIFTENABLED 0
#define PROP_FLIPBOOKHUESHIFTSPEED 0
#define PROP_FLIPBOOKHUESHIFT 0
#define PROPM_START_FLIPBOOKAUDIOLINK 0
#define PROP_AUDIOLINKFLIPBOOKSCALEBAND 0
#define PROP_AUDIOLINKFLIPBOOKALPHABAND 0
#define PROP_AUDIOLINKFLIPBOOKEMISSIONBAND 0
#define PROP_AUDIOLINKFLIPBOOKFRAMEBAND 0
#define PROP_FLIPBOOKCHRONOTENSITYENABLED 0
#define PROP_FLIPBOOKCHRONOTENSITYBAND 0
#define PROP_FLIPBOOKCHRONOTYPE 0
#define PROP_FLIPBOOKCHRONOTENSITYSPEED 0
#define PROPM_END_FLIPBOOKAUDIOLINK 0
#define PROPM_END_FLIPBOOK 0
#define PROPM_START_EMISSIONS 0
#define PROPM_START_EMISSIONOPTIONS 0
#define PROP_ENABLEEMISSION 0
#define PROP_EMISSIONREPLACE0 0
#define PROP_EMISSIONCOLORTHEMEINDEX 0
#define PROP_EMISSIONMAPUV 0
#define PROP_EMISSIONBASECOLORASMAP 0
#define PROP_EMISSIONMASKUV 0
#define PROP_EMISSIONMASKINVERT 0
#define PROP_EMISSIONSTRENGTH 0
#define PROP_EMISSIONHUESHIFTENABLED 0
#define PROP_EMISSIONHUESHIFT 0
#define PROP_EMISSIONHUESHIFTSPEED 0
#define PROP_EMISSIONCENTEROUTENABLED 0
#define PROP_EMISSIONCENTEROUTSPEED 5
#define PROP_ENABLEGITDEMISSION 0
#define PROP_GITDEWORLDORMESH 0
#define PROP_GITDEMINEMISSIONMULTIPLIER 1
#define PROP_GITDEMAXEMISSIONMULTIPLIER 0
#define PROP_GITDEMINLIGHT 0
#define PROP_GITDEMAXLIGHT 1
#define PROP_EMISSIONBLINKINGENABLED 0
#define PROP_EMISSIVEBLINK_MIN 0
#define PROP_EMISSIVEBLINK_MAX 1
#define PROP_EMISSIVEBLINK_VELOCITY 4
#define PROP_EMISSIONBLINKINGOFFSET 0
#define PROP_SCROLLINGEMISSION 0
#define PROP_EMISSIONSCROLLINGUSECURVE 0
#define PROP_EMISSIONSCROLLINGVERTEXCOLOR 0
#define PROP_EMISSIVESCROLL_WIDTH 10
#define PROP_EMISSIVESCROLL_VELOCITY 10
#define PROP_EMISSIVESCROLL_INTERVAL 20
#define PROP_EMISSIONSCROLLINGOFFSET 0
#define PROP_EMISSIONAL0ENABLED 0
#define PROP_EMISSIONAL0STRENGTHBAND 0
#define PROP_AUDIOLINKEMISSION0CENTEROUTSIZE 0
#define PROP_AUDIOLINKEMISSION0CENTEROUTBAND 0
#define PROPM_END_EMISSIONOPTIONS 0
#define PROPM_START_EMISSION1OPTIONS 0
#define PROP_ENABLEEMISSION1 0
#define PROP_EMISSIONREPLACE1 0
#define PROP_EMISSIONCOLOR1THEMEINDEX 0
#define PROP_EMISSIONMAP1UV 0
#define PROP_EMISSIONBASECOLORASMAP1 0
#define PROP_EMISSIONMASK1UV 0
#define PROP_EMISSIONMASKINVERT1 0
#define PROP_EMISSIONSTRENGTH1 0
#define PROP_EMISSIONHUESHIFTENABLED1 0
#define PROP_EMISSIONHUESHIFT1 0
#define PROP_EMISSIONHUESHIFTSPEED1 0
#define PROP_EMISSIONCENTEROUTENABLED1 0
#define PROP_EMISSIONCENTEROUTSPEED1 5
#define PROP_ENABLEGITDEMISSION1 0
#define PROP_GITDEWORLDORMESH1 0
#define PROP_GITDEMINEMISSIONMULTIPLIER1 1
#define PROP_GITDEMAXEMISSIONMULTIPLIER1 0
#define PROP_GITDEMINLIGHT1 0
#define PROP_GITDEMAXLIGHT1 1
#define PROP_EMISSIONBLINKINGENABLED1 0
#define PROP_EMISSIVEBLINK_MIN1 0
#define PROP_EMISSIVEBLINK_MAX1 1
#define PROP_EMISSIVEBLINK_VELOCITY1 4
#define PROP_EMISSIONBLINKINGOFFSET1 0
#define PROP_SCROLLINGEMISSION1 0
#define PROP_EMISSIONSCROLLINGUSECURVE1 0
#define PROP_EMISSIONSCROLLINGVERTEXCOLOR1 0
#define PROP_EMISSIVESCROLL_WIDTH1 10
#define PROP_EMISSIVESCROLL_VELOCITY1 10
#define PROP_EMISSIVESCROLL_INTERVAL1 20
#define PROP_EMISSIONSCROLLINGOFFSET1 0
#define PROP_EMISSIONAL1ENABLED 0
#define PROP_EMISSIONAL1STRENGTHBAND 0
#define PROP_AUDIOLINKEMISSION1CENTEROUTSIZE 0
#define PROP_AUDIOLINKEMISSION1CENTEROUTBAND 0
#define PROPM_END_EMISSION1OPTIONS 0
#define PROPM_START_EMISSION2OPTIONS 0
#define PROP_ENABLEEMISSION2 0
#define PROP_EMISSIONREPLACE2 0
#define PROP_EMISSIONCOLOR2THEMEINDEX 0
#define PROP_EMISSIONMAP2UV 0
#define PROP_EMISSIONBASECOLORASMAP2 0
#define PROP_EMISSIONMASK2UV 0
#define PROP_EMISSIONMASKINVERT2 0
#define PROP_EMISSIONSTRENGTH2 0
#define PROP_EMISSIONHUESHIFTENABLED2 0
#define PROP_EMISSIONHUESHIFT2 0
#define PROP_EMISSIONHUESHIFTSPEED2 0
#define PROP_EMISSIONCENTEROUTENABLED2 0
#define PROP_EMISSIONCENTEROUTSPEED2 5
#define PROP_ENABLEGITDEMISSION2 0
#define PROP_GITDEWORLDORMESH2 0
#define PROP_GITDEMINEMISSIONMULTIPLIER2 1
#define PROP_GITDEMAXEMISSIONMULTIPLIER2 0
#define PROP_GITDEMINLIGHT2 0
#define PROP_GITDEMAXLIGHT2 1
#define PROP_EMISSIONBLINKINGENABLED2 0
#define PROP_EMISSIVEBLINK_MIN2 0
#define PROP_EMISSIVEBLINK_MAX2 1
#define PROP_EMISSIVEBLINK_VELOCITY2 4
#define PROP_EMISSIONBLINKINGOFFSET2 0
#define PROP_SCROLLINGEMISSION2 0
#define PROP_EMISSIONSCROLLINGUSECURVE2 0
#define PROP_EMISSIONSCROLLINGVERTEXCOLOR2 0
#define PROP_EMISSIVESCROLL_WIDTH2 10
#define PROP_EMISSIVESCROLL_VELOCITY2 10
#define PROP_EMISSIVESCROLL_INTERVAL2 20
#define PROP_EMISSIONSCROLLINGOFFSET2 0
#define PROP_EMISSIONAL2ENABLED 0
#define PROP_EMISSIONAL2STRENGTHBAND 0
#define PROP_AUDIOLINKEMISSION2CENTEROUTSIZE 0
#define PROP_AUDIOLINKEMISSION2CENTEROUTBAND 0
#define PROPM_END_EMISSION2OPTIONS 0
#define PROPM_START_EMISSION3OPTIONS 0
#define PROP_ENABLEEMISSION3 0
#define PROP_EMISSIONREPLACE3 0
#define PROP_EMISSIONCOLOR3THEMEINDEX 0
#define PROP_EMISSIONMAP3UV 0
#define PROP_EMISSIONBASECOLORASMAP3 0
#define PROP_EMISSIONMASK3UV 0
#define PROP_EMISSIONMASKINVERT3 0
#define PROP_EMISSIONSTRENGTH3 0
#define PROP_EMISSIONHUESHIFTENABLED3 0
#define PROP_EMISSIONHUESHIFT3 0
#define PROP_EMISSIONHUESHIFTSPEED3 0
#define PROP_EMISSIONCENTEROUTENABLED3 0
#define PROP_EMISSIONCENTEROUTSPEED3 5
#define PROP_ENABLEGITDEMISSION3 0
#define PROP_GITDEWORLDORMESH3 0
#define PROP_GITDEMINEMISSIONMULTIPLIER3 1
#define PROP_GITDEMAXEMISSIONMULTIPLIER3 0
#define PROP_GITDEMINLIGHT3 0
#define PROP_GITDEMAXLIGHT3 1
#define PROP_EMISSIONBLINKINGENABLED3 0
#define PROP_EMISSIVEBLINK_MIN3 0
#define PROP_EMISSIVEBLINK_MAX3 1
#define PROP_EMISSIVEBLINK_VELOCITY3 4
#define PROP_EMISSIONBLINKINGOFFSET3 0
#define PROP_SCROLLINGEMISSION3 0
#define PROP_EMISSIONSCROLLINGUSECURVE3 0
#define PROP_EMISSIONSCROLLINGVERTEXCOLOR3 0
#define PROP_EMISSIVESCROLL_WIDTH3 10
#define PROP_EMISSIVESCROLL_VELOCITY3 10
#define PROP_EMISSIVESCROLL_INTERVAL3 20
#define PROP_EMISSIONSCROLLINGOFFSET3 0
#define PROP_EMISSIONAL3ENABLED 0
#define PROP_EMISSIONAL3STRENGTHBAND 0
#define PROP_AUDIOLINKEMISSION3CENTEROUTSIZE 0
#define PROP_AUDIOLINKEMISSION3CENTEROUTBAND 0
#define PROPM_END_EMISSION3OPTIONS 0
#define PROPM_END_EMISSIONS 0
#define PROPM_START_GLITTER 0
#define PROP_GLITTERENABLE 0
#define PROP_GLITTERUV 0
#define PROP_GLITTERMODE 0
#define PROP_GLITTERSHAPE 0
#define PROP_GLITTERBLENDTYPE 0
#define PROP_GLITTERCOLORTHEMEINDEX 0
#define PROP_GLITTERUSESURFACECOLOR 0
#define PROP_GLITTERCOLORMAPUV 0
#define PROP_GLITTERMASKUV 0
#define PROP_GLITTERTEXTUREROTATION 0
#define PROP_GLITTERFREQUENCY 300
#define PROP_GLITTERJITTER 1
#define PROP_GLITTERSPEED 10
#define PROP_GLITTERSIZE 0.3
#define PROP_GLITTERCONTRAST 300
#define PROP_GLITTERANGLERANGE 90
#define PROP_GLITTERMINBRIGHTNESS 0
#define PROP_GLITTERBRIGHTNESS 3
#define PROP_GLITTERBIAS 0.8
#define PROP_GLITTERHIDEINSHADOW 0
#define PROP_GLITTERCENTERSIZE 0.08
#define PROP_GLITTERFREQUENCYLINEAREMISSIVE 20
#define PROP_GLITTERJAGGYFIX 0
#define PROP_GLITTERHUESHIFTENABLED 0
#define PROP_GLITTERHUESHIFTSPEED 0
#define PROP_GLITTERHUESHIFT 0
#define PROP_GLITTERRANDOMCOLORS 0
#define PROP_GLITTERRANDOMSIZE 0
#define PROP_GLITTERRANDOMROTATION 0
#define PROPM_END_GLITTER 0
#define PROPM_START_PATHING 0
#define PROP_ENABLEPATHING 0
#define PROP_PATHGRADIENTTYPE 0
#define PROP_PATHINGOVERRIDEALPHA 0
#define PROP_PATHINGMAPUV 0
#define PROP_PATHINGCOLORMAPUV 0
#define PROP_PATHTYPER 0
#define PROP_PATHTYPEG 0
#define PROP_PATHTYPEB 0
#define PROP_PATHTYPEA 0
#define PROP_PATHCOLORRTHEMEINDEX 0
#define PROP_PATHCOLORGTHEMEINDEX 0
#define PROP_PATHCOLORBTHEMEINDEX 0
#define PROP_PATHCOLORATHEMEINDEX 0
#define PROPM_START_PATHAUDIOLINK 0
#define PROP_PATHALTIMEOFFSET 0
#define PROP_AUDIOLINKPATHTIMEOFFSETBANDR 0
#define PROP_AUDIOLINKPATHTIMEOFFSETBANDG 0
#define PROP_AUDIOLINKPATHTIMEOFFSETBANDB 0
#define PROP_AUDIOLINKPATHTIMEOFFSETBANDA 0
#define PROP_PATHALEMISSIONOFFSET 0
#define PROP_AUDIOLINKPATHEMISSIONADDBANDR 0
#define PROP_AUDIOLINKPATHEMISSIONADDBANDG 0
#define PROP_AUDIOLINKPATHEMISSIONADDBANDB 0
#define PROP_AUDIOLINKPATHEMISSIONADDBANDA 0
#define PROP_PATHALWIDTHOFFSET 0
#define PROP_AUDIOLINKPATHWIDTHOFFSETBANDR 0
#define PROP_AUDIOLINKPATHWIDTHOFFSETBANDG 0
#define PROP_AUDIOLINKPATHWIDTHOFFSETBANDB 0
#define PROP_AUDIOLINKPATHWIDTHOFFSETBANDA 0
#define PROP_PATHALHISTORY 0
#define PROP_PATHALHISTORYBANDR 0
#define PROP_PATHALHISTORYR 0
#define PROP_PATHALHISTORYBANDG 0
#define PROP_PATHALHISTORYG 0
#define PROP_PATHALHISTORYBANDB 0
#define PROP_PATHALHISTORYB 0
#define PROP_PATHALHISTORYBANDA 0
#define PROP_PATHALHISTORYA 0
#define PROP_PATHALCHRONO 0
#define PROP_PATHCHRONOBANDR 0
#define PROP_PATHCHRONOTYPER 0
#define PROP_PATHCHRONOSPEEDR 0
#define PROP_PATHCHRONOBANDG 0
#define PROP_PATHCHRONOTYPEG 0
#define PROP_PATHCHRONOSPEEDG 0
#define PROP_PATHCHRONOBANDB 0
#define PROP_PATHCHRONOTYPEB 0
#define PROP_PATHCHRONOSPEEDB 0
#define PROP_PATHCHRONOBANDA 0
#define PROP_PATHCHRONOTYPEA 0
#define PROP_PATHCHRONOSPEEDA 0
#define PROP_PATHALAUTOCORRELATOR 0
#define PROP_PATHALAUTOCORRELATORR 0
#define PROP_PATHALAUTOCORRELATORG 0
#define PROP_PATHALAUTOCORRELATORB 0
#define PROP_PATHALAUTOCORRELATORA 0
#define PROP_PATHALCCR 0
#define PROP_PATHALCCG 0
#define PROP_PATHALCCB 0
#define PROP_PATHALCCA 0
#define PROPM_END_PATHAUDIOLINK 0
#define PROPM_END_PATHING 0
#define PROPM_START_MIRROROPTIONS 0
#define PROP_ENABLEMIRROROPTIONS 0
#define PROP_MIRROR 0
#define PROP_MIRRORTEXTUREUV 0
#define PROPM_END_MIRROROPTIONS 0
#define PROPM_START_DEPTHFX 0
#define PROP_ENABLETOUCHGLOW 0
#define PROP_DEPTHMASKUV 0
#define PROP_DEPTHCOLORTOGGLE 0
#define PROP_DEPTHCOLORBLENDMODE 0
#define PROP_DEPTHTEXTUREUV 0
#define PROP_DEPTHCOLORTHEMEINDEX 0
#define PROP_DEPTHEMISSIONSTRENGTH 0
#define PROP_DEPTHCOLORMINDEPTH 0
#define PROP_DEPTHCOLORMAXDEPTH 1
#define PROP_DEPTHCOLORMINVALUE 0
#define PROP_DEPTHCOLORMAXVALUE 1
#define PROP_DEPTHALPHATOGGLE 0
#define PROP_DEPTHALPHAMINDEPTH 0
#define PROP_DEPTHALPHAMAXDEPTH 1
#define PROP_DEPTHALPHAMINVALUE 1
#define PROP_DEPTHALPHAMAXVALUE 0
#define PROPM_END_DEPTHFX 0
#define PROPM_START_IRIDESCENCE 0
#define PROP_ENABLEIRIDESCENCE 0
#define PROP_IRIDESCENCEMASKUV 0
#define PROP_IRIDESCENCENORMALTOGGLE 0
#define PROP_IRIDESCENCENORMALINTENSITY 1
#define PROP_IRIDESCENCENORMALMAPUV 0
#define PROP_IRIDESCENCENORMALSELECTION 1
#define PROP_IRIDESCENCEINTENSITY 1
#define PROP_IRIDESCENCEADDBLEND 0
#define PROP_IRIDESCENCEREPLACEBLEND 0
#define PROP_IRIDESCENCEMULTIPLYBLEND 0
#define PROP_IRIDESCENCEEMISSIONSTRENGTH 0
#define PROP_IRIDESCENCEHUESHIFTENABLED 0
#define PROP_IRIDESCENCEHUESHIFTSPEED 0
#define PROP_IRIDESCENCEHUESHIFT 0
#define PROPM_START_IRIDESCENCEAUDIOLINK 0
#define PROP_IRIDESCENCEAUDIOLINKEMISSIONADDBAND 0
#define PROPM_END_IRIDESCENCEAUDIOLINK 0
#define PROPM_END_IRIDESCENCE 0
#define PROPM_START_TEXT 0
#define PROP_TEXTPIXELRANGE 4
#define PROP_TEXTENABLED 0
#define PROPM_START_TEXTFPS 0
#define PROP_TEXTFPSENABLED 0
#define PROP_TEXTFPSUV 0
#define PROP_TEXTFPSCOLORTHEMEINDEX 0
#define PROP_TEXTFPSEMISSIONSTRENGTH 0
#define PROP_TEXTFPSROTATION 0
#define PROPM_END_TEXTFPS 0
#define PROPM_START_TEXTPOSITION 0
#define PROP_TEXTPOSITIONENABLED 0
#define PROP_TEXTPOSITIONUV 0
#define PROP_TEXTPOSITIONCOLORTHEMEINDEX 0
#define PROP_TEXTPOSITIONEMISSIONSTRENGTH 0
#define PROP_TEXTPOSITIONROTATION 0
#define PROPM_END_TEXTPOSITION 0
#define PROPM_START_TEXTINSTANCETIME 0
#define PROP_TEXTTIMEENABLED 0
#define PROP_TEXTTIMEUV 0
#define PROP_TEXTTIMECOLORTHEMEINDEX 0
#define PROP_TEXTTIMEEMISSIONSTRENGTH 0
#define PROP_TEXTTIMEROTATION 0
#define PROPM_END_TEXTINSTANCETIME 0
#define PROPM_END_TEXT 0
#define PROPM_START_FXPROXIMITYCOLOR 0
#define PROP_FXPROXIMITYCOLOR 0
#define PROP_FXPROXIMITYCOLORTYPE 1
#define PROP_FXPROXIMITYCOLORMINCOLORTHEMEINDEX 0
#define PROP_FXPROXIMITYCOLORMAXCOLORTHEMEINDEX 0
#define PROP_FXPROXIMITYCOLORMINDISTANCE 0
#define PROP_FXPROXIMITYCOLORMAXDISTANCE 1
#define PROPM_END_FXPROXIMITYCOLOR 0
#define PROPM_AUDIOLINKCATEGORY 0
#define PROPM_START_AUDIOLINK 0
#define PROP_ENABLEAUDIOLINK 0
#define PROP_AUDIOLINKHELP 0
#define PROP_AUDIOLINKANIMTOGGLE 1
#define PROP_DEBUGWAVEFORM 0
#define PROP_DEBUGDFT 0
#define PROP_DEBUGBASS 0
#define PROP_DEBUGLOWMIDS 0
#define PROP_DEBUGHIGHMIDS 0
#define PROP_DEBUGTREBLE 0
#define PROP_DEBUGCCCOLORS 0
#define PROP_DEBUGCCSTRIP 0
#define PROP_DEBUGCCLIGHTS 0
#define PROP_DEBUGAUTOCORRELATOR 0
#define PROP_DEBUGCHRONOTENSITY 0
#define PROP_DEBUGVISUALIZERHELPBOX 0
#define PROPM_END_AUDIOLINK 0
#define PROPM_START_ALDECALSPECTRUM 0
#define PROP_ENABLEALDECAL 0
#define PROP_ALDECALTYPE 0
#define PROP_ALDECALUVMODE 0
#define PROP_ALDECALUV 0
#define PROP_ALUVROTATION 0
#define PROP_ALUVROTATIONSPEED 0
#define PROP_ALDECALLINEWIDTH 1
#define PROP_ALDECALVOLUMESTEP 0
#define PROP_ALDECALVOLUMECLIPMIN 0
#define PROP_ALDECALVOLUMECLIPMAX 1
#define PROP_ALDECALBANDSTEP 0
#define PROP_ALDECALBANDCLIPMIN 0
#define PROP_ALDECALBANDCLIPMAX 1
#define PROP_ALDECALSHAPECLIP 0
#define PROP_ALDECALSHAPECLIPVOLUMEWIDTH 0.5
#define PROP_ALDECALSHAPECLIPBANDWIDTH 0.5
#define PROP_ALDECALVOLUME 0.5
#define PROP_ALDECALBASEBOOST 5
#define PROP_ALDECALTREBLEBOOST 1
#define PROP_ALDECALCOLORMASKUV 0
#define PROP_ALDECALVOLUMECOLORSOURCE 1
#define PROP_ALDECALLOWEMISSION 0
#define PROP_ALDECALMIDEMISSION 0
#define PROP_ALDECALHIGHEMISSION 0
#define PROP_ALDECALBLENDTYPE 0
#define PROP_ALDECALBLENDALPHA 1
#define PROP_ALDECALCONTROLSALPHA 0
#define PROPM_END_ALDECALSPECTRUM 0
#define PROPM_MODIFIERCATEGORY 0
#define PROPM_START_UVDISTORTION 0
#define PROP_ENABLEDISTORTION 0
#define PROP_DISTORTIONUVTODISTORT 0
#define PROP_DISTORTIONMASKUV 0
#define PROP_DISTORTIONFLOWTEXTUREUV 0
#define PROP_DISTORTIONFLOWTEXTURE1UV 0
#define PROP_DISTORTIONSTRENGTH 0.5
#define PROP_DISTORTIONSTRENGTH1 0.5
#define PROPM_START_DISTORTIONAUDIOLINK 0
#define PROP_ENABLEDISTORTIONAUDIOLINK 0
#define PROP_DISTORTIONSTRENGTHAUDIOLINKBAND 0
#define PROP_DISTORTIONSTRENGTH1AUDIOLINKBAND 0
#define PROPM_END_DISTORTIONAUDIOLINK 0
#define PROPM_END_UVDISTORTION 0
#define PROPM_START_UVPANOSPHERE 0
#define PROP_STEREOENABLED 0
#define PROP_PANOUSEBOTHEYES 1
#define PROPM_END_UVPANOSPHERE 0
#define PROPM_START_UVPOLAR 0
#define PROP_POLARUV 0
#define PROP_POLARRADIALSCALE 1
#define PROP_POLARLENGTHSCALE 1
#define PROP_POLARSPIRALPOWER 0
#define PROPM_END_UVPOLAR 0
#define PROPM_START_PARALLAX 0
#define PROP_POIPARALLAX 0
#define PROP_PARALLAXUV 0
#define PROP_HEIGHTMAPUV 0
#define PROP_HEIGHTMASKINVERT 0
#define PROP_HEIGHTMASKUV 0
#define PROP_HEIGHTSTRENGTH 0.4247461
#define PROP_CURVATUREU 0
#define PROP_CURVATUREV 0
#define PROP_HEIGHTSTEPSMIN 10
#define PROP_HEIGHTSTEPSMAX 128
#define PROP_CURVFIX 1
#define PROPM_END_PARALLAX 0
#define PROPM_THIRDPARTYCATEGORY 0
#define PROPM_POSTPROCESSING 0
#define PROPM_START_POILIGHTDATA 0
#define PROP_PPHELP 0
#define PROP_PPLIGHTINGMULTIPLIER 1
#define PROP_PPLIGHTINGADDITION 0
#define PROP_PPEMISSIONMULTIPLIER 1
#define PROP_PPFINALCOLORMULTIPLIER 1
#define PROPM_END_POILIGHTDATA 0
#define PROPM_START_POSTPROCESS 0
#define PROP_POSTPROCESS 0
#define PROP_PPMASKINVERT 0
#define PROP_PPMASKUV 0
#define PROP_PPLUTSTRENGTH 0
#define PROP_PPHUE 0
#define PROP_PPCONTRAST 1
#define PROP_PPSATURATION 1
#define PROP_PPBRIGHTNESS 1
#define PROP_PPLIGHTNESS 0
#define PROP_PPHDR 0
#define PROPM_END_POSTPROCESS 0
#define PROPM_RENDERINGCATEGORY 0
#define PROP_CULL 2
#define PROP_ZTEST 4
#define PROP_ZWRITE 1
#define PROP_COLORMASK 15
#define PROP_OFFSETFACTOR 0
#define PROP_OFFSETUNITS 0
#define PROP_RENDERINGREDUCECLIPDISTANCE 0
#define PROP_IGNOREFOG 0
#define PROPINSTANCING 0
#define PROPM_START_BLENDING 0
#define PROP_BLENDOP 0
#define PROP_BLENDOPALPHA 0
#define PROP_SRCBLEND 1
#define PROP_DSTBLEND 0
#define PROP_ADDBLENDOP 0
#define PROP_ADDBLENDOPALPHA 0
#define PROP_ADDSRCBLEND 1
#define PROP_ADDDSTBLEND 1
#define PROPM_END_BLENDING 0
#define PROPM_START_STENCILPASSOPTIONS 0
#define PROP_STENCILREF 0
#define PROP_STENCILREADMASK 255
#define PROP_STENCILWRITEMASK 255
#define PROP_STENCILPASSOP 0
#define PROP_STENCILFAILOP 0
#define PROP_STENCILZFAILOP 0
#define PROP_STENCILCOMPAREFUNCTION 8
#define PROPM_END_STENCILPASSOPTIONS 0

			#pragma target 5.0
			#pragma skip_variants DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING DIRLIGHTMAP_COMBINED SHADOWS_SHADOWMASK
			#pragma multi_compile_fwdbase
			#pragma multi_compile_instancing
			#pragma multi_compile_fog
			#pragma multi_compile _ VERTEXLIGHT_ON
			#define POI_PASS_BASE
			#include "UnityCG.cginc"
			#include "UnityStandardUtils.cginc"
			#include "AutoLight.cginc"
			#include "UnityLightingCommon.cginc"
			#include "UnityPBSLighting.cginc"
			#ifdef POI_PASS_META
			#include "UnityMetaPass.cginc"
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#define DielectricSpec float4(0.04, 0.04, 0.04, 1.0 - 0.04)
			#define PI float(3.14159265359)
			#define POI2D_SAMPLER_PAN(tex, texSampler, uv, pan) (UNITY_SAMPLE_TEX2D_SAMPLER(tex, texSampler, uv + _Time.x * pan))
			#define POI2D_SAMPLER_PANGRAD(tex, texSampler, uv, pan, ddx, ddy) (UNITY_SAMPLE_TEX2D_SAMPLER(tex, texSampler, uv + _Time.x * pan, ddx, ddy))
			#define POI2D_SAMPLER(tex, texSampler, uv) (UNITY_SAMPLE_TEX2D_SAMPLER(tex, texSampler, uv))
			#define POI2D_PAN(tex, uv, pan) (tex2D(tex, uv + _Time.x * pan))
			#define POI2D(tex, uv) (tex2D(tex, uv))
			#define POI_SAMPLE_TEX2D(tex, uv) (UNITY_SAMPLE_TEX2D(tex, uv))
			#define POI_SAMPLE_TEX2D_PAN(tex, uv, pan) (UNITY_SAMPLE_TEX2D(tex, uv + _Time.x * pan))
			#define POI2D_MAINTEX_SAMPLER_PAN_INLINED(tex, poiMesh) (POI2D_SAMPLER_PAN(tex, _MainTex, poiUV(poiMesh.uv[tex##UV], tex##_ST), tex##Pan))
			#define POI_SAFE_RGB0 float4(mainTexture.rgb * .0001, 0)
			#define POI_SAFE_RGB1 float4(mainTexture.rgb * .0001, 1)
			#define POI_SAFE_RGBA mainTexture
			#if defined(UNITY_COMPILER_HLSL)
			#define PoiInitStruct(type, name) name = (type)0;
			#else
			#define PoiInitStruct(type, name)
			#endif
			#define POI_ERROR(poiMesh, gridSize) lerp(float3(1, 0, 1), float3(0, 0, 0), fmod(floor((poiMesh.worldPos.x) * gridSize) + floor((poiMesh.worldPos.y) * gridSize) + floor((poiMesh.worldPos.z) * gridSize), 2) == 0)
			#define POI_MODE_OPAQUE 0
			#define POI_MODE_CUTOUT 1
			#define POI_MODE_FADE 2
			#define POI_MODE_TRANSPARENT 3
			#define POI_MODE_ADDITIVE 4
			#define POI_MODE_SOFTADDITIVE 5
			#define POI_MODE_MULTIPLICATIVE 6
			#define POI_MODE_2XMULTIPLICATIVE 7
			#define POI_MODE_TRANSCLIPPING 9
			#define ALPASS_DFT                      uint2(0,4)   //Size: 128, 2
			#define ALPASS_WAVEFORM                 uint2(0,6)   //Size: 128, 16
			#define ALPASS_AUDIOLINK                uint2(0,0)   //Size: 128, 4
			#define ALPASS_AUDIOBASS                uint2(0,0)   //Size: 128, 1
			#define ALPASS_AUDIOLOWMIDS             uint2(0,1)   //Size: 128, 1
			#define ALPASS_AUDIOHIGHMIDS            uint2(0,2)   //Size: 128, 1
			#define ALPASS_AUDIOTREBLE              uint2(0,3)   //Size: 128, 1
			#define ALPASS_AUDIOLINKHISTORY         uint2(1,0)   //Size: 127, 4
			#define ALPASS_GENERALVU                uint2(0,22)  //Size: 12, 1
			#define ALPASS_CCINTERNAL               uint2(12,22) //Size: 12, 2
			#define ALPASS_CCCOLORS                 uint2(25,22) //Size: 11, 1
			#define ALPASS_CCSTRIP                  uint2(0,24)  //Size: 128, 1
			#define ALPASS_CCLIGHTS                 uint2(0,25)  //Size: 128, 2
			#define ALPASS_AUTOCORRELATOR           uint2(0,27)  //Size: 128, 1
			#define ALPASS_GENERALVU_INSTANCE_TIME  uint2(2,22)
			#define ALPASS_GENERALVU_LOCAL_TIME     uint2(3,22)
			#define ALPASS_GENERALVU_NETWORK_TIME   uint2(4,22)
			#define ALPASS_GENERALVU_PLAYERINFO     uint2(6,22)
			#define ALPASS_FILTEREDAUDIOLINK        uint2(0,28)  //Size: 16, 4
			#define ALPASS_CHRONOTENSITY            uint2(16,28) //Size: 8, 4
			#define ALPASS_THEME_COLOR0             uint2(0,23)
			#define ALPASS_THEME_COLOR1             uint2(1,23)
			#define ALPASS_THEME_COLOR2             uint2(2,23)
			#define ALPASS_THEME_COLOR3             uint2(3,23)
			#define ALPASS_FILTEREDVU               uint2(24,28) //Size: 4, 4
			#define ALPASS_FILTEREDVU_INTENSITY     uint2(24,28) //Size: 4, 1
			#define ALPASS_FILTEREDVU_MARKER        uint2(24,29) //Size: 4, 1
			#define AUDIOLINK_SAMPHIST              3069        // Internal use for algos, do not change.
			#define AUDIOLINK_SAMPLEDATA24          2046
			#define AUDIOLINK_EXPBINS               24
			#define AUDIOLINK_EXPOCT                10
			#define AUDIOLINK_ETOTALBINS (AUDIOLINK_EXPBINS * AUDIOLINK_EXPOCT)
			#define AUDIOLINK_WIDTH                 128
			#define AUDIOLINK_SPS                   48000       // Samples per second
			#define AUDIOLINK_ROOTNOTE              0
			#define AUDIOLINK_4BAND_FREQFLOOR       0.123
			#define AUDIOLINK_4BAND_FREQCEILING     1
			#define AUDIOLINK_BOTTOM_FREQUENCY      13.75
			#define AUDIOLINK_BASE_AMPLITUDE        2.5
			#define AUDIOLINK_DELAY_COEFFICIENT_MIN 0.3
			#define AUDIOLINK_DELAY_COEFFICIENT_MAX 0.9
			#define AUDIOLINK_DFT_Q                 4.0
			#define AUDIOLINK_TREBLE_CORRECTION     5.0
			#define COLORCHORD_EMAXBIN              192
			#define COLORCHORD_IIR_DECAY_1          0.90
			#define COLORCHORD_IIR_DECAY_2          0.85
			#define COLORCHORD_CONSTANT_DECAY_1     0.01
			#define COLORCHORD_CONSTANT_DECAY_2     0.0
			#define COLORCHORD_NOTE_CLOSEST         3.0
			#define COLORCHORD_NEW_NOTE_GAIN        8.0
			#define COLORCHORD_MAX_NOTES            10
			#ifndef glsl_mod
			#define glsl_mod(x, y) (((x) - (y) * floor((x) / (y))))
			#endif
			uniform float4               _AudioTexture_TexelSize;
			#ifdef SHADER_TARGET_SURFACE_ANALYSIS
			#define AUDIOLINK_STANDARD_INDEXING
			#endif
			#ifdef AUDIOLINK_STANDARD_INDEXING
			sampler2D _AudioTexture;
			#define AudioLinkData(xycoord) tex2Dlod(_AudioTexture, float4(uint2(xycoord) * _AudioTexture_TexelSize.xy, 0, 0))
			#else
			uniform Texture2D<float4> _AudioTexture;
			SamplerState sampler_AudioTexture;
			#define AudioLinkData(xycoord) _AudioTexture[uint2(xycoord)]
			#endif
			float _Mode;
			float4 _GlobalThemeColor0;
			float4 _GlobalThemeColor1;
			float4 _GlobalThemeColor2;
			float4 _GlobalThemeColor3;
			float _StereoEnabled;
			float _PolarUV;
			float2 _PolarCenter;
			float _PolarRadialScale;
			float _PolarLengthScale;
			float _PolarSpiralPower;
			float _PanoUseBothEyes;
			#if defined(PROP_LIGHTINGAOMAPS) || !defined(OPTIMIZER_ENABLED)
			Texture2D _LightingAOMaps;
			#endif
			float4 _LightingAOMaps_ST;
			float2 _LightingAOMapsPan;
			float _LightingAOMapsUV;
			float _LightDataAOStrengthR;
			float _LightDataAOStrengthG;
			float _LightDataAOStrengthB;
			float _LightDataAOStrengthA;
			#if defined(PROP_LIGHTINGDETAILSHADOWMAPS) || !defined(OPTIMIZER_ENABLED)
			Texture2D _LightingDetailShadowMaps;
			#endif
			float4 _LightingDetailShadowMaps_ST;
			float2 _LightingDetailShadowMapsPan;
			float _LightingDetailShadowMapsUV;
			float _LightingDetailShadowStrengthR;
			float _LightingDetailShadowStrengthG;
			float _LightingDetailShadowStrengthB;
			float _LightingDetailShadowStrengthA;
			#if defined(PROP_LIGHTINGSHADOWMASKS) || !defined(OPTIMIZER_ENABLED)
			Texture2D _LightingShadowMasks;
			#endif
			float4 _LightingShadowMasks_ST;
			float2 _LightingShadowMasksPan;
			float _LightingShadowMasksUV;
			float _LightingShadowMaskStrengthR;
			float _LightingShadowMaskStrengthG;
			float _LightingShadowMaskStrengthB;
			float _LightingShadowMaskStrengthA;
			float _Unlit_Intensity;
			float _LightingColorMode;
			float _LightingMapMode;
			float _LightingDirectionMode;
			float3 _LightngForcedDirection;
			float _LightingIndirectUsesNormals;
			float _LightingCapEnabled;
			float _LightingCap;
			float _LightingForceColorEnabled;
			float3 _LightingForcedColor;
			float _LightingForcedColorThemeIndex;
			float _LightingCastedShadows;
			float _LightingMonochromatic;
			float _LightingAdditiveMonochromatic;
			float _LightingMinLightBrightness;
			float _LightingAdditiveLimited;
			float _LightingAdditiveLimit;
			float _LightingAdditivePassthrough;
			float _LightingDebugVisualize;
			float _IgnoreFog;
			float _RenderingReduceClipDistance;
			float4 _Color;
			float _ColorThemeIndex;
			UNITY_DECLARE_TEX2D(_MainTex);
			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
			float4 _MainTex_ST;
			float2 _MainTexPan;
			float _MainTexUV;
			float4 _MainTex_TexelSize;
			Texture2D _BumpMap;
			float4 _BumpMap_ST;
			float2 _BumpMapPan;
			float _BumpMapUV;
			float _BumpScale;
			Texture2D _ClippingMask;
			float4 _ClippingMask_ST;
			float2 _ClippingMaskPan;
			float _ClippingMaskUV;
			float _Inverse_Clipping;
			float _Cutoff;
			float _MainColorAdjustToggle;
			#if defined(PROP_MAINCOLORADJUSTTEXTURE) || !defined(OPTIMIZER_ENABLED)
			Texture2D _MainColorAdjustTexture;
			#endif
			float4 _MainColorAdjustTexture_ST;
			float2 _MainColorAdjustTexturePan;
			float _MainColorAdjustTextureUV;
			float _MainHueShiftToggle;
			float _MainHueShiftReplace;
			float _MainHueShift;
			float _MainHueShiftSpeed;
			float _Saturation;
			float _MainBrightness;
			float _MainHueALCTEnabled;
			float _MainALHueShiftBand;
			float _MainALHueShiftCTIndex;
			float _MainHueALMotionSpeed;
			SamplerState sampler_linear_clamp;
			SamplerState sampler_linear_repeat;
			float _AlphaForceOpaque;
			float _AlphaMod;
			float _AlphaPremultiply;
			float _AlphaToCoverage;
			float _AlphaSharpenedA2C;
			float _AlphaMipScale;
			float _AlphaDithering;
			float _AlphaDitherGradient;
			float _AlphaDistanceFade;
			float _AlphaDistanceFadeType;
			float _AlphaDistanceFadeMinAlpha;
			float _AlphaDistanceFadeMaxAlpha;
			float _AlphaDistanceFadeMin;
			float _AlphaDistanceFadeMax;
			float _AlphaFresnel;
			float _AlphaFresnelAlpha;
			float _AlphaFresnelSharpness;
			float _AlphaFresnelWidth;
			float _AlphaFresnelInvert;
			float _AlphaAngular;
			float _AngleType;
			float _AngleCompareTo;
			float3 _AngleForwardDirection;
			float _CameraAngleMin;
			float _CameraAngleMax;
			float _ModelAngleMin;
			float _ModelAngleMax;
			float _AngleMinAlpha;
			float _AlphaAudioLinkEnabled;
			float2 _AlphaAudioLinkAddRange;
			float _AlphaAudioLinkAddBand;
			float _MainVertexColoringLinearSpace;
			float _MainVertexColoring;
			float _MainUseVertexColorAlpha;
			#if defined(PROP_DECALMASK) || !defined(OPTIMIZER_ENABLED)
			Texture2D _DecalMask;
			float4 _DecalMask_ST;
			float2 _DecalMaskPan;
			float _DecalMaskUV;
			#endif
			float _ShadowOffset;
			float _ShadowStrength;
			float _LightingIgnoreAmbientColor;
			float _LightingGradientStart;
			float _LightingGradientEnd;
			float3 _LightingShadowColor;
			float _LightingGradientStartWrap;
			float _LightingGradientEndWrap;
			#ifdef _LIGHTINGMODE_SHADEMAP
			float3 _1st_ShadeColor;
			#if defined(PROP_1ST_SHADEMAP) || !defined(OPTIMIZER_ENABLED)
			Texture2D _1st_ShadeMap;
			#endif
			float4 _1st_ShadeMap_ST;
			float2 _1st_ShadeMapPan;
			float _1st_ShadeMapUV;
			float _Use_1stShadeMapAlpha_As_ShadowMask;
			float _1stShadeMapMask_Inverse;
			float _Use_BaseAs1st;
			float3 _2nd_ShadeColor;
			#if defined(PROP_2ND_SHADEMAP) || !defined(OPTIMIZER_ENABLED)
			Texture2D _2nd_ShadeMap;
			#endif
			float4 _2nd_ShadeMap_ST;
			float2 _2nd_ShadeMapPan;
			float _2nd_ShadeMapUV;
			float _Use_2ndShadeMapAlpha_As_ShadowMask;
			float _2ndShadeMapMask_Inverse;
			float _Use_1stAs2nd;
			float _BaseColor_Step;
			float _BaseShade_Feather;
			float _ShadeColor_Step;
			float _1st2nd_Shades_Feather;
			float _ShadingShadeMapBlendType;
			#endif
			sampler2D _SkinLUT;
			float _SssScale;
			float _SssBumpBlur;
			float3 _SssTransmissionAbsorption;
			float3 _SssColorBleedAoWeights;
			#ifdef _LIGHTINGMODE_MULTILAYER_MATH
			float4 _ShadowColor;
			#if defined(PROP_SHADOWCOLORTEX) || !defined(OPTIMIZER_ENABLED)
			Texture2D _ShadowColorTex;
			float4 _ShadowColorTex_ST;
			float2 _ShadowColorTexPan;
			float _ShadowColorTexUV;
			#endif
			float _ShadowBorder;
			float _ShadowBlur;
			float4 _Shadow2ndColor;
			#if defined(PROP_SHADOW2NDCOLORTEX) || !defined(OPTIMIZER_ENABLED)
			Texture2D _Shadow2ndColorTex;
			float4 _Shadow2ndColorTex_ST;
			float2 _Shadow2ndColorTexPan;
			float _Shadow2ndColorTexUV;
			#endif
			float _Shadow2ndBorder;
			float _Shadow2ndBlur;
			float4 _Shadow3rdColor;
			#if defined(PROP_SHADOW3RDCOLORTEX) || !defined(OPTIMIZER_ENABLED)
			Texture2D _Shadow3rdColorTex;
			float4 _Shadow3rdColorTex_ST;
			float2 _Shadow3rdColorTexPan;
			float _Shadow3rdColorTexUV;
			#endif
			float _Shadow3rdBorder;
			float _Shadow3rdBlur;
			float4 _ShadowBorderColor;
			float _ShadowBorderRange;
			#endif
			#ifdef _LIGHTINGMODE_CLOTH
			Texture2D_float _ClothDFG;
			SamplerState sampler_ClothDFG;
			#if defined(PROP_CLOTHMETALLICSMOOTHNESSMAP) || !defined(OPTIMIZER_ENABLED)
			Texture2D _ClothMetallicSmoothnessMap;
			#endif
			float4 _ClothMetallicSmoothnessMap_ST;
			float2 _ClothMetallicSmoothnessMapPan;
			float _ClothMetallicSmoothnessMapUV;
			float _ClothMetallicSmoothnessMapInvert;
			float _ClothMetallic;
			float _ClothReflectance;
			float _ClothSmoothness;
			#endif
			#ifdef _LIGHTINGMODE_SDF
			#if defined(PROP_SDFSHADINGTEXTURE) || !defined(OPTIMIZER_ENABLED)
			Texture2D _SDFShadingTexture;
			float _SDFShadingTextureUV;
			float2 _SDFShadingTexturePan;
			float4 _SDFShadingTexture_ST;
			#endif
			#endif
			float _LightingAdditiveType;
			float _LightingAdditiveGradientStart;
			float _LightingAdditiveGradientEnd;
			float _LightingAdditiveDetailStrength;
			float4 _MochieReflCube_HDR;
			#if defined(PROP_DEPTHMASK) || !defined(OPTIMIZER_ENABLED)
			Texture2D _DepthMask;
			#endif
			float4 _DepthMask_ST;
			float2 _DepthMaskPan;
			float _DepthMaskUV;
			float _DepthColorToggle;
			float _DepthColorBlendMode;
			#if defined(PROP_DEPTHTEXTURE) || !defined(OPTIMIZER_ENABLED)
			Texture2D _DepthTexture;
			#endif
			float4 _DepthTexture_ST;
			float2 _DepthTexturePan;
			float _DepthTextureUV;
			float3 _DepthColor;
			float _DepthColorThemeIndex;
			float _DepthColorMinDepth;
			float _DepthColorMaxDepth;
			float _DepthColorMinValue;
			float _DepthColorMaxValue;
			float _DepthEmissionStrength;
			float _DepthAlphaToggle;
			float _DepthAlphaMinValue;
			float _DepthAlphaMaxValue;
			float _DepthAlphaMinDepth;
			float _DepthAlphaMaxDepth;
			float _PPLightingMultiplier;
			float _PPLightingAddition;
			float _PPEmissionMultiplier;
			float _PPFinalColorMultiplier;
			float _FXProximityColor;
			float _FXProximityColorType;
			float3 _FXProximityColorMinColor;
			float3 _FXProximityColorMaxColor;
			float _FXProximityColorMinColorThemeIndex;
			float _FXProximityColorMaxColorThemeIndex;
			float _FXProximityColorMinDistance;
			float _FXProximityColorMaxDistance;
			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 color : COLOR;
				float2 uv0 : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float2 uv2 : TEXCOORD2;
				float2 uv3 : TEXCOORD3;
				uint vertexId : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv[4] : TEXCOORD0;
				float3 objNormal : TEXCOORD4;
				float3 normal : TEXCOORD5;
				float3 tangent : TEXCOORD6;
				float3 binormal : TEXCOORD7;
				float4 worldPos : TEXCOORD8;
				float4 localPos : TEXCOORD9;
				float3 objectPos : TEXCOORD10;
				float4 vertexColor : TEXCOORD11;
				float4 lightmapUV : TEXCOORD12;
				float4 grabPos: TEXCOORD13;
				float4 worldDirection: TEXCOORD14;
				UNITY_SHADOW_COORDS(15)
				UNITY_FOG_COORDS(16)
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			struct PoiMesh
			{
				float3 normals[2];
				float3 objNormal;
				float3 tangentSpaceNormal;
				float3 binormal;
				float3 tangent;
				float3 worldPos;
				float3 localPos;
				float3 objectPosition;
				float isFrontFace;
				float4 vertexColor;
				float4 lightmapUV;
				float2 uv[8];
				float2 parallaxUV;
			};
			struct PoiCam
			{
				float3 viewDir;
				float3 forwardDir;
				float3 worldPos;
				float distanceToVert;
				float4 clipPos;
				float3 reflectionDir;
				float3 vertexReflectionDir;
				float3 tangentViewDir;
				float4 grabPos;
				float2 screenUV;
				float vDotN;
				float4 worldDirection;
			};
			struct PoiMods
			{
				float4 Mask;
				float4 audioLink;
				float audioLinkAvailable;
				float audioLinkVersion;
				float4 audioLinkTexture;
				float2 detailMask;
				float2 backFaceDetailIntensity;
				float globalEmission;
				float4 globalColorTheme[12];
				float ALTime[8];
			};
			struct PoiLight
			{
				float3 direction;
				float attenuation;
				float attenuationStrength;
				float3 directColor;
				float3 indirectColor;
				float occlusion;
				float shadowMask;
				float detailShadow;
				float3 halfDir;
				float lightMap;
				float3 rampedLightMap;
				float vertexNDotL;
				float nDotL;
				float nDotV;
				float vertexNDotV;
				float nDotH;
				float vertexNDotH;
				float lDotv;
				float lDotH;
				float nDotLSaturated;
				float nDotLNormalized;
				#ifdef UNITY_PASS_FORWARDADD
				float additiveShadow;
				#endif
				float3 finalLighting;
				float3 finalLightAdd;
				#if defined(VERTEXLIGHT_ON) && defined(POI_VERTEXLIGHT_ON)
				float4 vDotNL;
				float4 vertexVDotNL;
				float3 vColor[4];
				float4 vCorrectedDotNL;
				float4 vAttenuation;
				float4 vAttenuationDotNL;
				float3 vPosition[4];
				float3 vDirection[4];
				float3 vFinalLighting;
				float3 vHalfDir[4];
				half4 vDotNH;
				half4 vertexVDotNH;
				half4 vDotLH;
				#endif
			};
			struct PoiVertexLights
			{
				float3 direction;
				float3 color;
				float attenuation;
			};
			struct PoiFragData
			{
				float3 baseColor;
				float3 finalColor;
				float alpha;
				float3 emission;
			};
			float2 poiUV(float2 uv, float4 tex_st)
			{
				return uv * tex_st.xy + tex_st.zw;
			}
			float calculateluminance(float3 color)
			{
				return color.r * 0.299 + color.g * 0.587 + color.b * 0.114;
			}
			bool IsInMirror()
			{
				return unity_CameraProjection[2][0] != 0.f || unity_CameraProjection[2][1] != 0.f;
			}
			bool IsOrthographicCamera()
			{
				return unity_OrthoParams.w == 1 || UNITY_MATRIX_P[3][3] == 1;
			}
			float shEvaluateDiffuseL1Geomerics_local(float L0, float3 L1, float3 n)
			{
				float R0 = max(0, L0);
				float3 R1 = 0.5f * L1;
				float lenR1 = length(R1);
				float q = dot(normalize(R1), n) * 0.5 + 0.5;
				q = saturate(q); // Thanks to ScruffyRuffles for the bug identity.
				float p = 1.0f + 2.0f * lenR1 / R0;
				float a = (1.0f - lenR1 / R0) / (1.0f + lenR1 / R0);
				return R0 * (a + (1.0f - a) * (p + 1.0f) * pow(q, p));
			}
			half3 BetterSH9(half4 normal)
			{
				float3 indirect;
				float3 L0 = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w) + float3(unity_SHBr.z, unity_SHBg.z, unity_SHBb.z) / 3.0;
				indirect.r = shEvaluateDiffuseL1Geomerics_local(L0.r, unity_SHAr.xyz, normal.xyz);
				indirect.g = shEvaluateDiffuseL1Geomerics_local(L0.g, unity_SHAg.xyz, normal.xyz);
				indirect.b = shEvaluateDiffuseL1Geomerics_local(L0.b, unity_SHAb.xyz, normal.xyz);
				indirect = max(0, indirect);
				indirect += SHEvalLinearL2(normal);
				return indirect;
			}
			float3 getCameraForward()
			{
				#if UNITY_SINGLE_PASS_STEREO
				float3 p1 = mul(unity_StereoCameraToWorld[0], float4(0, 0, 1, 1));
				float3 p2 = mul(unity_StereoCameraToWorld[0], float4(0, 0, 0, 1));
				#else
				float3 p1 = mul(unity_CameraToWorld, float4(0, 0, 1, 1)).xyz;
				float3 p2 = mul(unity_CameraToWorld, float4(0, 0, 0, 1)).xyz;
				#endif
				return normalize(p2 - p1);
			}
			half3 GetSHLength()
			{
				half3 x, x1;
				x.r = length(unity_SHAr);
				x.g = length(unity_SHAg);
				x.b = length(unity_SHAb);
				x1.r = length(unity_SHBr);
				x1.g = length(unity_SHBg);
				x1.b = length(unity_SHBb);
				return x + x1;
			}
			float3 BoxProjection(float3 direction, float3 position, float4 cubemapPosition, float3 boxMin, float3 boxMax)
			{
				#if UNITY_SPECCUBE_BOX_PROJECTION
				if (cubemapPosition.w > 0)
				{
					float3 factors = ((direction > 0 ? boxMax : boxMin) - position) / direction;
					float scalar = min(min(factors.x, factors.y), factors.z);
					direction = direction * scalar + (position - cubemapPosition.xyz);
				}
				#endif
				return direction;
			}
			float poiMax(float2 i)
			{
				return max(i.x, i.y);
			}
			float poiMax(float3 i)
			{
				return max(max(i.x, i.y), i.z);
			}
			float poiMax(float4 i)
			{
				return max(max(max(i.x, i.y), i.z), i.w);
			}
			float3 calculateNormal(in float3 baseNormal, in PoiMesh poiMesh, in Texture2D normalTexture, in float4 normal_ST, in float2 normalPan, in float normalUV, in float normalIntensity)
			{
				float3 normal = UnpackScaleNormal(POI2D_SAMPLER_PAN(normalTexture, _MainTex, poiUV(poiMesh.uv[normalUV], normal_ST), normalPan), normalIntensity);
				return normalize(
				normal.x * poiMesh.tangent +
				normal.y * poiMesh.binormal +
				normal.z * baseNormal
				);
			}
			float remap(float x, float minOld, float maxOld, float minNew = 0, float maxNew = 1)
			{
				return minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld);
			}
			float2 remap(float2 x, float2 minOld, float2 maxOld, float2 minNew = 0, float2 maxNew = 1)
			{
				return minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld);
			}
			float3 remap(float3 x, float3 minOld, float3 maxOld, float3 minNew = 0, float3 maxNew = 1)
			{
				return minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld);
			}
			float4 remap(float4 x, float4 minOld, float4 maxOld, float4 minNew = 0, float4 maxNew = 1)
			{
				return minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld);
			}
			float remapClamped(float minOld, float maxOld, float x, float minNew = 0, float maxNew = 1)
			{
				return clamp(minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld), minNew, maxNew);
			}
			float2 remapClamped(float2 minOld, float2 maxOld, float2 x, float2 minNew, float2 maxNew)
			{
				return clamp(minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld), minNew, maxNew);
			}
			float3 remapClamped(float3 minOld, float3 maxOld, float3 x, float3 minNew, float3 maxNew)
			{
				return clamp(minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld), minNew, maxNew);
			}
			float4 remapClamped(float4 minOld, float4 maxOld, float4 x, float4 minNew, float4 maxNew)
			{
				return clamp(minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld), minNew, maxNew);
			}
			float2 calcParallax(in float height, in PoiCam poiCam)
			{
				return ((height * - 1) + 1) * (poiCam.tangentViewDir.xy / poiCam.tangentViewDir.z);
			}
			float4 poiBlend(const float sourceFactor, const  float4 sourceColor, const  float destinationFactor, const  float4 destinationColor, const float4 blendFactor)
			{
				float4 sA = 1 - blendFactor;
				const float4 blendData[11] = {
					float4(0.0, 0.0, 0.0, 0.0),
					float4(1.0, 1.0, 1.0, 1.0),
					destinationColor,
					sourceColor,
					float4(1.0, 1.0, 1.0, 1.0) - destinationColor,
					sA,
					float4(1.0, 1.0, 1.0, 1.0) - sourceColor,
					sA,
					float4(1.0, 1.0, 1.0, 1.0) - sA,
					saturate(sourceColor.aaaa),
					1 - sA,
				};
				return lerp(blendData[sourceFactor] * sourceColor + blendData[destinationFactor] * destinationColor, sourceColor, sA);
			}
			float3 blendAverage(float3 base, float3 blend)
			{
				return (base + blend) / 2.0;
			}
			float blendColorBurn(float base, float blend)
			{
				return (blend == 0.0)?blend : max((1.0 - ((1.0 - base) / blend)), 0.0);
			}
			float3 blendColorBurn(float3 base, float3 blend)
			{
				return float3(blendColorBurn(base.r, blend.r), blendColorBurn(base.g, blend.g), blendColorBurn(base.b, blend.b));
			}
			float blendColorDodge(float base, float blend)
			{
				return (blend == 1.0)?blend : min(base / (1.0 - blend), 1.0);
			}
			float3 blendColorDodge(float3 base, float3 blend)
			{
				return float3(blendColorDodge(base.r, blend.r), blendColorDodge(base.g, blend.g), blendColorDodge(base.b, blend.b));
			}
			float blendDarken(float base, float blend)
			{
				return min(blend, base);
			}
			float3 blendDarken(float3 base, float3 blend)
			{
				return float3(blendDarken(base.r, blend.r), blendDarken(base.g, blend.g), blendDarken(base.b, blend.b));
			}
			float3 blendExclusion(float3 base, float3 blend)
			{
				return base + blend - 2.0 * base * blend;
			}
			float blendReflect(float base, float blend)
			{
				return (blend == 1.0)?blend : min(base * base / (1.0 - blend), 1.0);
			}
			float3 blendReflect(float3 base, float3 blend)
			{
				return float3(blendReflect(base.r, blend.r), blendReflect(base.g, blend.g), blendReflect(base.b, blend.b));
			}
			float3 blendGlow(float3 base, float3 blend)
			{
				return blendReflect(blend, base);
			}
			float blendOverlay(float base, float blend)
			{
				return base < 0.5?(2.0 * base * blend) : (1.0 - 2.0 * (1.0 - base) * (1.0 - blend));
			}
			float3 blendOverlay(float3 base, float3 blend)
			{
				return float3(blendOverlay(base.r, blend.r), blendOverlay(base.g, blend.g), blendOverlay(base.b, blend.b));
			}
			float3 blendHardLight(float3 base, float3 blend)
			{
				return blendOverlay(blend, base);
			}
			float blendVividLight(float base, float blend)
			{
				return (blend < 0.5)?blendColorBurn(base, (2.0 * blend)) : blendColorDodge(base, (2.0 * (blend - 0.5)));
			}
			float3 blendVividLight(float3 base, float3 blend)
			{
				return float3(blendVividLight(base.r, blend.r), blendVividLight(base.g, blend.g), blendVividLight(base.b, blend.b));
			}
			float blendHardMix(float base, float blend)
			{
				return (blendVividLight(base, blend) < 0.5)?0.0 : 1.0;
			}
			float3 blendHardMix(float3 base, float3 blend)
			{
				return float3(blendHardMix(base.r, blend.r), blendHardMix(base.g, blend.g), blendHardMix(base.b, blend.b));
			}
			float blendLighten(float base, float blend)
			{
				return max(blend, base);
			}
			float3 blendLighten(float3 base, float3 blend)
			{
				return float3(blendLighten(base.r, blend.r), blendLighten(base.g, blend.g), blendLighten(base.b, blend.b));
			}
			float blendLinearBurn(float base, float blend)
			{
				return max(base + blend - 1.0, 0.0);
			}
			float3 blendLinearBurn(float3 base, float3 blend)
			{
				return max(base + blend - float3(1.0, 1.0, 1.0), float3(0.0, 0.0, 0.0));
			}
			float blendLinearDodge(float base, float blend)
			{
				return min(base + blend, 1.0);
			}
			float3 blendLinearDodge(float3 base, float3 blend)
			{
				return min(base + blend, float3(1.0, 1.0, 1.0));
			}
			float blendLinearLight(float base, float blend)
			{
				return blend < 0.5?blendLinearBurn(base, (2.0 * blend)) : blendLinearDodge(base, (2.0 * (blend - 0.5)));
			}
			float3 blendLinearLight(float3 base, float3 blend)
			{
				return float3(blendLinearLight(base.r, blend.r), blendLinearLight(base.g, blend.g), blendLinearLight(base.b, blend.b));
			}
			float3 blendMultiply(float3 base, float3 blend)
			{
				return base * blend;
			}
			float3 blendNegation(float3 base, float3 blend)
			{
				return float3(1.0, 1.0, 1.0) - abs(float3(1.0, 1.0, 1.0) - base - blend);
			}
			float3 blendNormal(float3 base, float3 blend)
			{
				return blend;
			}
			float3 blendPhoenix(float3 base, float3 blend)
			{
				return min(base, blend) - max(base, blend) + float3(1.0, 1.0, 1.0);
			}
			float blendPinLight(float base, float blend)
			{
				return (blend < 0.5)?blendDarken(base, (2.0 * blend)) : blendLighten(base, (2.0 * (blend - 0.5)));
			}
			float3 blendPinLight(float3 base, float3 blend)
			{
				return float3(blendPinLight(base.r, blend.r), blendPinLight(base.g, blend.g), blendPinLight(base.b, blend.b));
			}
			float blendScreen(float base, float blend)
			{
				return 1.0 - ((1.0 - base) * (1.0 - blend));
			}
			float3 blendScreen(float3 base, float3 blend)
			{
				return float3(blendScreen(base.r, blend.r), blendScreen(base.g, blend.g), blendScreen(base.b, blend.b));
			}
			float blendSoftLight(float base, float blend)
			{
				return (blend < 0.5)?(2.0 * base * blend + base * base * (1.0 - 2.0 * blend)) : (sqrt(base) * (2.0 * blend - 1.0) + 2.0 * base * (1.0 - blend));
			}
			float3 blendSoftLight(float3 base, float3 blend)
			{
				return float3(blendSoftLight(base.r, blend.r), blendSoftLight(base.g, blend.g), blendSoftLight(base.b, blend.b));
			}
			float blendSubtract(float base, float blend)
			{
				return max(base - blend, 0.0);
			}
			float3 blendSubtract(float3 base, float3 blend)
			{
				return max(base - blend, 0.0);
			}
			float blendDifference(float base, float blend)
			{
				return abs(base - blend);
			}
			float3 blendDifference(float3 base, float3 blend)
			{
				return abs(base - blend);
			}
			float blendDivide(float base, float blend)
			{
				return base / max(blend, 0.0001);
			}
			float3 blendDivide(float3 base, float3 blend)
			{
				return base / max(blend, 0.0001);
			}
			float3 customBlend(float3 base, float3 blend, float blendType)
			{
				float3 ret = 0;
				switch(blendType)
				{
					case 0:
					{
						ret = blendNormal(base, blend);
						break;
					}
					case 1:
					{
						ret = blendDarken(base, blend);
						break;
					}
					case 2:
					{
						ret = blendMultiply(base, blend);
						break;
					}
					case 3:
					{
						ret = blendColorBurn(base, blend);
						break;
					}
					case 4:
					{
						ret = blendLinearBurn(base, blend);
						break;
					}
					case 5:
					{
						ret = blendLighten(base, blend);
						break;
					}
					case 6:
					{
						ret = blendScreen(base, blend);
						break;
					}
					case 7:
					{
						ret = blendColorDodge(base, blend);
						break;
					}
					case 8:
					{
						ret = blendLinearDodge(base, blend);
						break;
					}
					case 9:
					{
						ret = blendOverlay(base, blend);
						break;
					}
					case 10:
					{
						ret = blendSoftLight(base, blend);
						break;
					}
					case 11:
					{
						ret = blendHardLight(base, blend);
						break;
					}
					case 12:
					{
						ret = blendVividLight(base, blend);
						break;
					}
					case 13:
					{
						ret = blendLinearLight(base, blend);
						break;
					}
					case 14:
					{
						ret = blendPinLight(base, blend);
						break;
					}
					case 15:
					{
						ret = blendHardMix(base, blend);
						break;
					}
					case 16:
					{
						ret = blendDifference(base, blend);
						break;
					}
					case 17:
					{
						ret = blendExclusion(base, blend);
						break;
					}
					case 18:
					{
						ret = blendSubtract(base, blend);
						break;
					}
					case 19:
					{
						ret = blendDivide(base, blend);
						break;
					}
				}
				return ret;
			}
			float random(float2 p)
			{
				return frac(sin(dot(p, float2(12.9898, 78.2383))) * 43758.5453123);
			}
			float2 random2(float2 p)
			{
				return frac(sin(float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)))) * 43758.5453);
			}
			float3 random3(float3 p)
			{
				return frac(sin(float3(dot(p, float3(127.1, 311.7, 248.6)), dot(p, float3(269.5, 183.3, 423.3)), dot(p, float3(248.3, 315.9, 184.2)))) * 43758.5453);
			}
			float3 randomFloat3(float2 Seed, float maximum)
			{
				return (.5 + float3(
				frac(sin(dot(Seed.xy, float2(12.9898, 78.233))) * 43758.5453),
				frac(sin(dot(Seed.yx, float2(12.9898, 78.233))) * 43758.5453),
				frac(sin(dot(float2(Seed), float2(12.9898, 78.233))) * 43758.5453)
				) * .5) * (maximum);
			}
			float3 randomFloat3Range(float2 Seed, float Range)
			{
				return (float3(
				frac(sin(dot(Seed.xy, float2(12.9898, 78.233))) * 43758.5453),
				frac(sin(dot(Seed.yx, float2(12.9898, 78.233))) * 43758.5453),
				frac(sin(dot(float2(Seed.x * Seed.y, Seed.y + Seed.x), float2(12.9898, 78.233))) * 43758.5453)
				) * 2 - 1) * Range;
			}
			float3 randomFloat3WiggleRange(float2 Seed, float Range, float wiggleSpeed)
			{
				float3 rando = (float3(
				frac(sin(dot(Seed.xy, float2(12.9898, 78.233))) * 43758.5453),
				frac(sin(dot(Seed.yx, float2(12.9898, 78.233))) * 43758.5453),
				frac(sin(dot(float2(Seed.x * Seed.y, Seed.y + Seed.x), float2(12.9898, 78.233))) * 43758.5453)
				) * 2 - 1);
				float speed = 1 + wiggleSpeed;
				return float3(sin((_Time.x + rando.x * PI) * speed), sin((_Time.x + rando.y * PI) * speed), sin((_Time.x + rando.z * PI) * speed)) * Range;
			}
			void Unity_RandomRange_float(float2 Seed, float Min, float Max, out float Out)
			{
				float randomno = frac(sin(dot(Seed, float2(12.9898, 78.233))) * 43758.5453);
				Out = lerp(Min, Max, randomno);
			}
			void poiChannelMixer(float3 In, float3 _ChannelMixer_Red, float3 _ChannelMixer_Green, float3 _ChannelMixer_Blue, out float3 Out)
			{
				Out = float3(dot(In, _ChannelMixer_Red), dot(In, _ChannelMixer_Green), dot(In, _ChannelMixer_Blue));
			}
			void poiContrast(float3 In, float Contrast, out float3 Out)
			{
				float midpoint = pow(0.5, 2.2);
				Out = (In - midpoint) * Contrast + midpoint;
			}
			void poiInvertColors(float4 In, float4 InvertColors, out float4 Out)
			{
				Out = abs(InvertColors - In);
			}
			void poiReplaceColor(float3 In, float3 From, float3 To, float Range, float Fuzziness, out float3 Out)
			{
				float Distance = distance(From, In);
				Out = lerp(To, In, saturate((Distance - Range) / max(Fuzziness, 0.00001)));
			}
			void poiSaturation(float3 In, float Saturation, out float3 Out)
			{
				float luma = dot(In, float3(0.2126729, 0.7151522, 0.0721750));
				Out = luma.xxx + Saturation.xxx * (In - luma.xxx);
			}
			void poiDither(float4 In, float4 ScreenPosition, out float4 Out)
			{
				float2 uv = ScreenPosition.xy * _ScreenParams.xy;
				float DITHER_THRESHOLDS[16] = {
					1.0 / 17.0, 9.0 / 17.0, 3.0 / 17.0, 11.0 / 17.0,
					13.0 / 17.0, 5.0 / 17.0, 15.0 / 17.0, 7.0 / 17.0,
					4.0 / 17.0, 12.0 / 17.0, 2.0 / 17.0, 10.0 / 17.0,
					16.0 / 17.0, 8.0 / 17.0, 14.0 / 17.0, 6.0 / 17.0
				};
				uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
				Out = In - DITHER_THRESHOLDS[index];
			}
			void poiColorMask(float3 In, float3 MaskColor, float Range, float Fuzziness, out float4 Out)
			{
				float Distance = distance(MaskColor, In);
				Out = saturate(1 - (Distance - Range) / max(Fuzziness, 0.00001));
			}
			static const float Epsilon = 1e-10;
			static const float3 HCYwts = float3(0.299, 0.587, 0.114);
			static const float HCLgamma = 3;
			static const float HCLy0 = 100;
			static const float HCLmaxL = 0.530454533953517; // == exp(HCLgamma / HCLy0) - 0.5
			static const float3 wref = float3(1.0, 1.0, 1.0);
			#define TAU 6.28318531
			float3 HUEtoRGB(in float H)
			{
				float R = abs(H * 6 - 3) - 1;
				float G = 2 - abs(H * 6 - 2);
				float B = 2 - abs(H * 6 - 4);
				return saturate(float3(R, G, B));
			}
			float3 RGBtoHCV(in float3 RGB)
			{
				float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0 / 3.0) : float4(RGB.gb, 0.0, -1.0 / 3.0);
				float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
				float C = Q.x - min(Q.w, Q.y);
				float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
				return float3(H, C, Q.x);
			}
			float3 HSVtoRGB(in float3 HSV)
			{
				float3 RGB = HUEtoRGB(HSV.x);
				return ((RGB - 1) * HSV.y + 1) * HSV.z;
			}
			float3 RGBtoHSV(in float3 RGB)
			{
				float3 HCV = RGBtoHCV(RGB);
				float S = HCV.y / (HCV.z + Epsilon);
				return float3(HCV.x, S, HCV.z);
			}
			float3 HSLtoRGB(in float3 HSL)
			{
				float3 RGB = HUEtoRGB(HSL.x);
				float C = (1 - abs(2 * HSL.z - 1)) * HSL.y;
				return (RGB - 0.5) * C + HSL.z;
			}
			float3 RGBtoHSL(in float3 RGB)
			{
				float3 HCV = RGBtoHCV(RGB);
				float L = HCV.z - HCV.y * 0.5;
				float S = HCV.y / (1 - abs(L * 2 - 1) + Epsilon);
				return float3(HCV.x, S, L);
			}
			float3 hueShift(float3 color, float hueOffset)
			{
				color = RGBtoHSV(color);
				color.x = frac(hueOffset +color.x);
				return HSVtoRGB(color);
			}
			float3 hueShiftClamped(float3 color, float hueOffset, float saturationOffset = 0, float valueOffset = 0)
			{
				color = RGBtoHSV(color);
				color.x = frac(hueOffset +color.x);
				color.y = saturate(saturationOffset +color.y);
				color.z = saturate(valueOffset +color.z);
				return HSVtoRGB(color);
			}
			float3 ModifyViaHSL(float3 color, float3 HSLMod)
			{
				float3 colorHSL = RGBtoHSL(color);
				colorHSL.r = frac(colorHSL.r + HSLMod.r);
				colorHSL.g = saturate(colorHSL.g + HSLMod.g);
				colorHSL.b = saturate(colorHSL.b + HSLMod.b);
				return HSLtoRGB(colorHSL);
			}
			float3 poiSaturation(float3 In, float Saturation)
			{
				float luma = dot(In, float3(0.2126729, 0.7151522, 0.0721750));
				return luma.xxx + Saturation.xxx * (In - luma.xxx);
			}
			float xyzF(float t)
			{
				return lerp(pow(t, 1. / 3.), 7.787037 * t + 0.139731, step(t, 0.00885645));
			}
			float xyzR(float t)
			{
				return lerp(t * t * t, 0.1284185 * (t - 0.139731), step(t, 0.20689655));
			}
			float3 rgb2lch(in float3 c)
			{
				c = mul(float3x3(0.4124, 0.3576, 0.1805,
				0.2126, 0.7152, 0.0722,
				0.0193, 0.1192, 0.9505), c);
				c.x = xyzF(c.x / wref.x);
				c.y = xyzF(c.y / wref.y);
				c.z = xyzF(c.z / wref.z);
				float3 lab = float3(max(0., 116.0 * c.y - 16.0), 500.0 * (c.x - c.y), 200.0 * (c.y - c.z));
				return float3(lab.x, length(float2(lab.y, lab.z)), atan2(lab.z, lab.y));
			}
			float3 lch2rgb(in float3 c)
			{
				c = float3(c.x, cos(c.z) * c.y, sin(c.z) * c.y);
				float lg = 1. / 116. * (c.x + 16.);
				float3 xyz = float3(wref.x * xyzR(lg + 0.002 * c.y),
				wref.y * xyzR(lg),
				wref.z * xyzR(lg - 0.005 * c.z));
				float3 rgb = mul(float3x3(3.2406, -1.5372, -0.4986,
				- 0.9689, 1.8758, 0.0415,
				0.0557, -0.2040, 1.0570), xyz);
				return rgb;
			}
			float lerpAng(in float a, in float b, in float x)
			{
				float ang = fmod(fmod((a - b), TAU) + PI * 3., TAU) - PI;
				return ang * x + b;
			}
			float3 lerpLch(in float3 a, in float3 b, in float x)
			{
				float hue = lerpAng(a.z, b.z, x);
				return float3(lerp(b.xy, a.xy, x), hue);
			}
			float3 poiExpensiveColorBlend(float3 col1, float3 col2, float alpha)
			{
				return lch2rgb(lerpLch(rgb2lch(col1), rgb2lch(col2), alpha));
			}
			float4x4 poiAngleAxisRotationMatrix(float angle, float3 axis)
			{
				axis = normalize(axis);
				float s = sin(angle);
				float c = cos(angle);
				float oc = 1.0 - c;
				return float4x4(oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s, 0.0,
				oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c, oc * axis.y * axis.z - axis.x * s, 0.0,
				oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c, 0.0,
				0.0, 0.0, 0.0, 1.0);
			}
			float4x4 poiRotationMatrixFromAngles(float x, float y, float z)
			{
				float angleX = radians(x);
				float c = cos(angleX);
				float s = sin(angleX);
				float4x4 rotateXMatrix = float4x4(1, 0, 0, 0,
				0, c, -s, 0,
				0, s, c, 0,
				0, 0, 0, 1);
				float angleY = radians(y);
				c = cos(angleY);
				s = sin(angleY);
				float4x4 rotateYMatrix = float4x4(c, 0, s, 0,
				0, 1, 0, 0,
				- s, 0, c, 0,
				0, 0, 0, 1);
				float angleZ = radians(z);
				c = cos(angleZ);
				s = sin(angleZ);
				float4x4 rotateZMatrix = float4x4(c, -s, 0, 0,
				s, c, 0, 0,
				0, 0, 1, 0,
				0, 0, 0, 1);
				return mul(mul(rotateXMatrix, rotateYMatrix), rotateZMatrix);
			}
			float4x4 poiRotationMatrixFromAngles(float3 angles)
			{
				float angleX = radians(angles.x);
				float c = cos(angleX);
				float s = sin(angleX);
				float4x4 rotateXMatrix = float4x4(1, 0, 0, 0,
				0, c, -s, 0,
				0, s, c, 0,
				0, 0, 0, 1);
				float angleY = radians(angles.y);
				c = cos(angleY);
				s = sin(angleY);
				float4x4 rotateYMatrix = float4x4(c, 0, s, 0,
				0, 1, 0, 0,
				- s, 0, c, 0,
				0, 0, 0, 1);
				float angleZ = radians(angles.z);
				c = cos(angleZ);
				s = sin(angleZ);
				float4x4 rotateZMatrix = float4x4(c, -s, 0, 0,
				s, c, 0, 0,
				0, 0, 1, 0,
				0, 0, 0, 1);
				return mul(mul(rotateXMatrix, rotateYMatrix), rotateZMatrix);
			}
			float3 getCameraPosition()
			{
				#ifdef USING_STEREO_MATRICES
				return lerp(unity_StereoWorldSpaceCameraPos[0], unity_StereoWorldSpaceCameraPos[1], 0.5);
				#endif
				return _WorldSpaceCameraPos;
			}
			half2 calcScreenUVs(half4 grabPos)
			{
				half2 uv = grabPos.xy / (grabPos.w + 0.0000000001);
				#if UNITY_SINGLE_PASS_STEREO
				uv.xy *= half2(_ScreenParams.x * 2, _ScreenParams.y);
				#else
				uv.xy *= _ScreenParams.xy;
				#endif
				return uv;
			}
			float CalcMipLevel(float2 texture_coord)
			{
				float2 dx = ddx(texture_coord);
				float2 dy = ddy(texture_coord);
				float delta_max_sqr = max(dot(dx, dx), dot(dy, dy));
				return 0.5 * log2(delta_max_sqr);
			}
			float inverseLerp(float A, float B, float T)
			{
				return (T - A) / (B - A);
			}
			float inverseLerp2(float2 a, float2 b, float2 value)
			{
				float2 AB = b - a;
				float2 AV = value - a;
				return dot(AV, AB) / dot(AB, AB);
			}
			float inverseLerp3(float3 a, float3 b, float3 value)
			{
				float3 AB = b - a;
				float3 AV = value - a;
				return dot(AV, AB) / dot(AB, AB);
			}
			float inverseLerp4(float4 a, float4 b, float4 value)
			{
				float4 AB = b - a;
				float4 AV = value - a;
				return dot(AV, AB) / dot(AB, AB);
			}
			float4 quaternion_conjugate(float4 v)
			{
				return float4(
				v.x, -v.yzw
				);
			}
			float4 quaternion_mul(float4 v1, float4 v2)
			{
				float4 result1 = (v1.x * v2 + v1 * v2.x);
				float4 result2 = float4(
				- dot(v1.yzw, v2.yzw),
				cross(v1.yzw, v2.yzw)
				);
				return float4(result1 + result2);
			}
			float4 get_quaternion_from_angle(float3 axis, float angle)
			{
				float sn = sin(angle * 0.5);
				float cs = cos(angle * 0.5);
				return float4(axis * sn, cs);
			}
			float4 quaternion_from_vector(float3 inVec)
			{
				return float4(0.0, inVec);
			}
			float degree_to_radius(float degree)
			{
				return (
				degree / 180.0 * PI
				);
			}
			float3 rotate_with_quaternion(float3 inVec, float3 rotation)
			{
				float4 qx = get_quaternion_from_angle(float3(1, 0, 0), radians(rotation.x));
				float4 qy = get_quaternion_from_angle(float3(0, 1, 0), radians(rotation.y));
				float4 qz = get_quaternion_from_angle(float3(0, 0, 1), radians(rotation.z));
				#define MUL3(A, B, C) quaternion_mul(quaternion_mul((A), (B)), (C))
				float4 quaternion = normalize(MUL3(qx, qy, qz));
				float4 conjugate = quaternion_conjugate(quaternion);
				float4 inVecQ = quaternion_from_vector(inVec);
				float3 rotated = (
				MUL3(quaternion, inVecQ, conjugate)
				).yzw;
				return rotated;
			}
			float4 transform(float4 input, float4 pos, float4 rotation, float4 scale)
			{
				input.rgb *= (scale.xyz * scale.w);
				input = float4(rotate_with_quaternion(input.xyz, rotation.xyz * rotation.w) + (pos.xyz * pos.w), input.w);
				return input;
			}
			float aaBlurStep(float gradient, float edge, float blur)
			{
				float edgeMin = saturate(edge);
				float edgeMax = saturate(edge + blur * (1 - edge));
				return smoothstep(0, 1, saturate((gradient - edgeMin) / saturate(edgeMax - edgeMin + fwidth(gradient))));
			}
			float3 poiThemeColor(in PoiMods poiMods, in float3 srcColor, in float themeIndex)
			{
				if (themeIndex == 0) return srcColor;
				themeIndex -= 1;
				if (themeIndex <= 3)
				{
					return poiMods.globalColorTheme[themeIndex];
				}
				return srcColor;
			}
			float lilIsIn0to1(float f)
			{
				float value = 0.5 - abs(f - 0.5);
				return saturate(value / clamp(fwidth(value), 0.0001, 1.0));
			}
			float lilIsIn0to1(float f, float nv)
			{
				float value = 0.5 - abs(f - 0.5);
				return saturate(value / clamp(fwidth(value), 0.0001, nv));
			}
			float lilTooningNoSaturate(float value, float border)
			{
				return (value - border) / clamp(fwidth(value), 0.0001, 1.0);
			}
			float lilTooningNoSaturate(float value, float border, float blur)
			{
				float borderMin = saturate(border - blur * 0.5);
				float borderMax = saturate(border + blur * 0.5);
				return (value - borderMin) / saturate(borderMax - borderMin + fwidth(value));
			}
			float lilTooningNoSaturate(float value, float border, float blur, float borderRange)
			{
				float borderMin = saturate(border - blur * 0.5 - borderRange);
				float borderMax = saturate(border + blur * 0.5);
				return (value - borderMin) / saturate(borderMax - borderMin + fwidth(value));
			}
			float lilTooning(float value, float border)
			{
				return saturate(lilTooningNoSaturate(value, border));
			}
			float lilTooning(float value, float border, float blur)
			{
				return saturate(lilTooningNoSaturate(value, border, blur));
			}
			float lilTooning(float value, float border, float blur, float borderRange)
			{
				return saturate(lilTooningNoSaturate(value, border, blur, borderRange));
			}
			inline float4 CalculateFrustumCorrection()
			{
				float x1 = -UNITY_MATRIX_P._31 / (UNITY_MATRIX_P._11 * UNITY_MATRIX_P._34);
				float x2 = -UNITY_MATRIX_P._32 / (UNITY_MATRIX_P._22 * UNITY_MATRIX_P._34);
				return float4(x1, x2, 0, UNITY_MATRIX_P._33 / UNITY_MATRIX_P._34 + x1 * UNITY_MATRIX_P._13 + x2 * UNITY_MATRIX_P._23);
			}
			inline float CorrectedLinearEyeDepth(float z, float B)
			{
				return 1.0 / (z / UNITY_MATRIX_P._34 + B);
			}
			v2f vert(appdata v)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				v2f o;
				PoiInitStruct(v2f, o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.objectPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
				o.objNormal = v.normal;
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.tangent = UnityObjectToWorldDir(v.tangent);
				o.binormal = cross(o.normal, o.tangent) * (v.tangent.w * unity_WorldTransformParams.w);
				o.vertexColor = v.color;
				o.uv[0] = v.uv0;
				o.uv[1] = v.uv1;
				o.uv[2] = v.uv2;
				o.uv[3] = v.uv3;
				#if defined(LIGHTMAP_ON)
				o.lightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif
				#ifdef DYNAMICLIGHTMAP_ON
				o.lightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif
				o.localPos = v.vertex;
				o.worldPos = mul(unity_ObjectToWorld, o.localPos);
				float3 localOffset = float3(0, 0, 0);
				float3 worldOffset = float3(0, 0, 0);
				o.localPos.rgb += localOffset;
				o.worldPos.rgb += worldOffset;
				o.pos = UnityObjectToClipPos(o.localPos);
				#ifdef POI_PASS_OUTLINE
				#if defined(UNITY_REVERSED_Z)
				o.pos.z += _Offset_Z * - 0.01;
				#else
				o.pos.z += _Offset_Z * 0.01;
				#endif
				#endif
				o.grabPos = ComputeGrabScreenPos(o.pos);
				#ifndef FORWARD_META_PASS
				#if !defined(UNITY_PASS_SHADOWCASTER)
				UNITY_TRANSFER_SHADOW(o, o.uv[0].xy);
				#else
				TRANSFER_SHADOW_CASTER_NOPOS(o, o.pos);
				#endif
				#endif
				UNITY_TRANSFER_FOG(o, o.pos);
				if (float(0))
				{
					if (o.pos.w < _ProjectionParams.y * 1.01 && o.pos.w > 0)
					{
						o.pos.z = o.pos.z * 0.0001 + o.pos.w * 0.999;
					}
				}
				#ifdef POI_PASS_META
				o.pos = UnityMetaVertexPosition(v.vertex, v.uv1.xy, v.uv2.xy, unity_LightmapST, unity_DynamicLightmapST);
				#endif
				#if defined(GRAIN)
				float4 worldDirection;
				worldDirection.xyz = o.worldPos.xyz - _WorldSpaceCameraPos;
				worldDirection.w = dot(o.pos, CalculateFrustumCorrection());
				o.worldDirection = worldDirection;
				#endif
				return o;
			}
			void calculateGlobalThemes(inout PoiMods poiMods)
			{
				poiMods.globalColorTheme[0] = float4(1,1,1,1);
				poiMods.globalColorTheme[1] = float4(1,1,1,1);
				poiMods.globalColorTheme[2] = float4(1,1,1,1);
				poiMods.globalColorTheme[3] = float4(1,1,1,1);
			}
			float2 calculatePolarCoordinate(in PoiMesh poiMesh)
			{
				float2 delta = poiMesh.uv[float(0)] - float4(0.5,0.5,0,0);
				float radius = length(delta) * 2 * float(1);
				float angle = atan2(delta.x, delta.y) * 1.0 / 6.28 * float(1);
				return float2(radius, angle + distance(poiMesh.uv[float(0)], float4(0.5,0.5,0,0)) * float(0));
			}
			float2 MonoPanoProjection(float3 coords)
			{
				float3 normalizedCoords = normalize(coords);
				float latitude = acos(normalizedCoords.y);
				float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
				float2 sphereCoords = float2(longitude, latitude) * float2(1.0 / UNITY_PI, 1.0 / UNITY_PI);
				sphereCoords = float2(1.0, 1.0) - sphereCoords;
				return(sphereCoords + float4(0, 1 - unity_StereoEyeIndex, 1, 1.0).xy) * float4(0, 1 - unity_StereoEyeIndex, 1, 1.0).zw;
			}
			float2 StereoPanoProjection(float3 coords)
			{
				float3 normalizedCoords = normalize(coords);
				float latitude = acos(normalizedCoords.y);
				float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
				float2 sphereCoords = float2(longitude, latitude) * float2(0.5 / UNITY_PI, 1.0 / UNITY_PI);
				sphereCoords = float2(0.5, 1.0) - sphereCoords;
				return(sphereCoords + float4(0, 1 - unity_StereoEyeIndex, 1, 0.5).xy) * float4(0, 1 - unity_StereoEyeIndex, 1, 0.5).zw;
			}
			float2 calculatePanosphereUV(in PoiMesh poiMesh)
			{
				float3 viewDirection = normalize(lerp(getCameraPosition().xyz, _WorldSpaceCameraPos.xyz, float(1)) - poiMesh.worldPos.xyz) * - 1;
				return lerp(MonoPanoProjection(viewDirection), StereoPanoProjection(viewDirection), float(0));
			}
			void applyAlphaOptions(inout PoiFragData poiFragData, in PoiMesh poiMesh, in PoiCam poiCam, in PoiMods poiMods)
			{
				poiFragData.alpha = saturate(poiFragData.alpha + float(0));
				if (float(0))
				{
					float3 position = float(1) ? poiMesh.worldPos : poiMesh.objectPosition;
					poiFragData.alpha *= lerp(float(0), float(1), smoothstep(float(0), float(0), distance(position, poiCam.worldPos)));
				}
				if (float(0))
				{
					float holoRim = saturate(1 - smoothstep(min(float(0.5), float(0.5)), float(0.5), poiCam.vDotN));
					holoRim = abs(lerp(1, holoRim, float(0)));
					poiFragData.alpha *= float(0) ?1 - holoRim : holoRim;
				}
				if (float(0))
				{
					half cameraAngleMin = float(45) / 180;
					half cameraAngleMax = float(90) / 180;
					half modelAngleMin = float(45) / 180;
					half modelAngleMax = float(90) / 180;
					float3 pos = float(0) == 0 ? poiMesh.objectPosition : poiMesh.worldPos;
					half3 cameraToModelDirection = normalize(pos - getCameraPosition());
					half3 modelForwardDirection = normalize(mul(unity_ObjectToWorld, normalize(float4(0,0,1,0).rgb)));
					half cameraLookAtModel = remapClamped(cameraAngleMax, cameraAngleMin, .5 * dot(cameraToModelDirection, getCameraForward()) + .5);
					half modelLookAtCamera = remapClamped(modelAngleMax, modelAngleMin, .5 * dot(-cameraToModelDirection, modelForwardDirection) + .5);
					if (float(0) == 0)
					{
						poiFragData.alpha *= max(cameraLookAtModel, float(0));
					}
					else if (float(0) == 1)
					{
						poiFragData.alpha *= max(modelLookAtCamera, float(0));
					}
					else if (float(0) == 2)
					{
						poiFragData.alpha *= max(cameraLookAtModel * modelLookAtCamera, float(0));
					}
				}
			}
			inline half Dither8x8Bayer(int x, int y)
			{
				const half dither[ 64 ] = {
					1, 49, 13, 61, 4, 52, 16, 64,
					33, 17, 45, 29, 36, 20, 48, 32,
					9, 57, 5, 53, 12, 60, 8, 56,
					41, 25, 37, 21, 44, 28, 40, 24,
					3, 51, 15, 63, 2, 50, 14, 62,
					35, 19, 47, 31, 34, 18, 46, 30,
					11, 59, 7, 55, 10, 58, 6, 54,
					43, 27, 39, 23, 42, 26, 38, 22
				};
				int r = y * 8 + x;
				return dither[r] / 64;
			}
			half calcDither(half2 grabPos)
			{
				return Dither8x8Bayer(fmod(grabPos.x, 8), fmod(grabPos.y, 8));
			}
			void applyDithering(inout PoiFragData poiFragData, in PoiCam poiCam)
			{
				if (float(0))
				{
					poiFragData.alpha = saturate(poiFragData.alpha - (calcDither(poiCam.screenUV) * (1 - poiFragData.alpha) * float(0.1)));
				}
			}
			void ApplyAlphaToCoverage(inout PoiFragData poiFragData, in PoiMesh poiMesh)
			{
				
				if (float(0) == 1)
				{
					
					if (float(0) && float(0))
					{
						poiFragData.alpha *= 1 + max(0, CalcMipLevel(poiMesh.uv[0] * float4(0.0004882813,0.0004882813,2048,2048).zw)) * float(0.25);
						poiFragData.alpha = (poiFragData.alpha - float(0.5)) / max(fwidth(poiFragData.alpha), 0.0001) + float(0.5);
						poiFragData.alpha = saturate(poiFragData.alpha);
					}
				}
			}
			void applyVertexColor(inout PoiFragData poiFragData, PoiMesh poiMesh)
			{
				#ifndef POI_PASS_OUTLINE
				float3 vertCol = lerp(poiMesh.vertexColor.rgb, GammaToLinearSpace(poiMesh.vertexColor.rgb), float(1));
				poiFragData.baseColor *= lerp(1, vertCol, float(0));
				#endif
				poiFragData.alpha *= lerp(1, poiMesh.vertexColor.a, float(0));
			}
			#if defined(GEOM_TYPE_BRANCH) || defined(GEOM_TYPE_BRANCH_DETAIL) || defined(GEOM_TYPE_FROND) || defined(DEPTH_OF_FIELD_COC_VIEW)
			float2 decalUV(float uvNumber, float4 uv_st, float2 position, half rotation, half rotationSpeed, half2 scale, float4 scaleOffset, float depth, in PoiMesh poiMesh, in PoiCam poiCam)
			{
				scaleOffset = float4(-scaleOffset.x, scaleOffset.y, -scaleOffset.z, scaleOffset.w);
				float2 uv = poiUV(poiMesh.uv[uvNumber], uv_st) + calcParallax(depth + 1, poiCam);
				float2 decalCenter = position;
				float theta = radians(rotation + _Time.z * rotationSpeed);
				float cs = cos(theta);
				float sn = sin(theta);
				uv = float2((uv.x - decalCenter.x) * cs - (uv.y - decalCenter.y) * sn + decalCenter.x, (uv.x - decalCenter.x) * sn + (uv.y - decalCenter.y) * cs + decalCenter.y);
				uv = remap(uv, float2(0, 0) - scale / 2 + position + scaleOffset.xz, scale / 2 + position + scaleOffset.yw, float2(0, 0), float2(1, 1));
				return uv;
			}
			inline float3 decalHueShift(float enabled, float3 color, float shift, float shiftSpeed)
			{
				if (enabled)
				{
					color = hueShift(color, shift + _Time.x * shiftSpeed);
				}
				return color;
			}
			inline float applyTilingClipping(float enabled, float2 uv)
			{
				float ret = 1;
				if (!enabled)
				{
					if (uv.x > 1 || uv.y > 1 || uv.x < 0 || uv.y < 0)
					{
						ret = 0;
					}
				}
				return ret;
			}
			void applyDecals(inout PoiFragData poiFragData, in PoiMesh poiMesh, in PoiCam poiCam, in PoiMods poiMods, in PoiLight poiLight)
			{
				float decalAlpha = 1;
				float alphaOverride = 0;
				#if defined(PROP_DECALMASK) || !defined(OPTIMIZER_ENABLED)
				float4 decalMask = POI2D_SAMPLER_PAN(_DecalMask, _MainTex, poiUV(poiMesh.uv[float(0)], float4(1,1,0,0)), float4(0,0,0,0));
				#else
				float4 decalMask = 1;
				#endif
				float4 decalColor = 1;
				float2 uv = 0;
				float2 decalScale = float2(1, 1);
				float decalRotation = 0;
				float2 ddxuv = 0;
				float2 ddyuv = 0;
				float4 sideMod = 0;
				if (alphaOverride)
				{
					poiFragData.alpha *= decalAlpha;
				}
				poiFragData.baseColor = saturate(poiFragData.baseColor);
			}
			#endif
			#ifdef VIGNETTE_MASKED
			#ifdef _LIGHTINGMODE_CLOTH
			#define HARD 0
			#define LERP 1
			#define CLOTHMODE HARD
			float V_SmithGGXCorrelated(float roughness, float NoV, float NoL)
			{
				float a2 = roughness * roughness;
				float lambdaV = NoL * sqrt((NoV - a2 * NoV) * NoV + a2);
				float lambdaL = NoV * sqrt((NoL - a2 * NoL) * NoL + a2);
				float v = 0.5 / (lambdaV + lambdaL);
				return v;
			}
			float D_GGX(float roughness, float NoH)
			{
				float oneMinusNoHSquared = 1.0 - NoH * NoH;
				float a = NoH * roughness;
				float k = roughness / (oneMinusNoHSquared + a * a);
				float d = k * k * (1.0 / UNITY_PI);
				return d;
			}
			float D_Charlie(float roughness, float NoH)
			{
				float invAlpha = 1.0 / roughness;
				float cos2h = NoH * NoH;
				float sin2h = max(1.0 - cos2h, 0.0078125); // 0.0078125 = 2^(-14/2), so sin2h^2 > 0 in fp16
				return (2.0 + invAlpha) * pow(sin2h, invAlpha * 0.5) / (2.0 * UNITY_PI);
			}
			float V_Neubelt(float NoV, float NoL)
			{
				return 1.0 / (4.0 * (NoL + NoV - NoL * NoV));
			}
			float Distribution(float roughness, float NoH, float cloth)
			{
				#if CLOTHMODE == LERP
				return lerp(GGXTerm(roughness, NoH), D_Charlie(roughness, NoH), cloth);
				#elif CLOTHMODE == HARD
				return cloth <= 0.5 ? GGXTerm(roughness, NoH) : D_Charlie(roughness, NoH);
				#endif
			}
			float Visibility(float roughness, float NoV, float NoL, float cloth)
			{
				#if CLOTHMODE == LERP
				return lerp(V_SmithGGXCorrelated(roughness, NoV, NoL), V_Neubelt(NoV, NoL), cloth);
				#elif CLOTHMODE == HARD
				return cloth <= 0.5 ? V_SmithGGXCorrelated(roughness, NoV, NoL) : V_Neubelt(NoV, NoL);
				#endif
			}
			float F_Schlick(float3 f0, float f90, float VoH)
			{
				return f0 + (f90 - f0) * pow(1.0 - VoH, 5);
			}
			float F_Schlick(float3 f0, float VoH)
			{
				float f = pow(1.0 - VoH, 5.0);
				return f + f0 * (1.0 - f);
			}
			float Fresnel(float3 f0, float LoH)
			{
				float f90 = saturate(dot(f0, float(50.0 * 0.33).xxx));
				return F_Schlick(f0, f90, LoH);
			}
			float Fd_Burley(float roughness, float NoV, float NoL, float LoH)
			{
				float f90 = 0.5 + 2.0 * roughness * LoH * LoH;
				float lightScatter = F_Schlick(1.0, f90, NoL);
				float viewScatter = F_Schlick(1.0, f90, NoV);
				return lightScatter * viewScatter;
			}
			float Fd_Wrap(float NoL, float w)
			{
				return saturate((NoL + w) / pow(1.0 + w, 2));
			}
			float4 SampleDFG(float NoV, float perceptualRoughness)
			{
				return _ClothDFG.Sample(sampler_ClothDFG, float3(NoV, perceptualRoughness, 0));
			}
			float3 EnvBRDF(float2 dfg, float3 f0)
			{
				return f0 * dfg.x + dfg.y;
			}
			float3 EnvBRDFMultiscatter(float3 dfg, float3 f0, float cloth)
			{
				#if CLOTHMODE == LERP
				return lerp(lerp(dfg.xxx, dfg.yyy, f0), f0 * dfg.z, cloth);
				#elif CLOTHMODE == HARD
				return cloth <= 0.5 ? lerp(dfg.xxx, dfg.yyy, f0) : f0 * dfg.z;
				#endif
			}
			float3 EnvBRDFEnergyCompensation(float3 dfg, float3 f0, float cloth)
			{
				#if CLOTHMODE == LERP
				return lerp(1.0 + f0 * (1.0 / dfg.y - 1.0), 1, cloth);
				#elif CLOTHMODE == HARD
				return cloth <= 0.5 ? 1.0 + f0 * (1.0 / dfg.y - 1.0) : 1;
				#endif
			}
			float ClothMetallic(float cloth)
			{
				#if CLOTHMODE == LERP
				return cloth;
				#elif CLOTHMODE == HARD
				return cloth <= 0.5 ? 1 : 0;
				#endif
			}
			float3 Specular(float roughness, PoiLight poiLight, float f0, float3 normal, float cloth)
			{
				float NoL = poiLight.nDotLSaturated;
				float NoH = poiLight.nDotH;
				float LoH = poiLight.lDotH;
				float NoV = poiLight.nDotV;
				float D = Distribution(roughness, NoH, cloth);
				float V = Visibility(roughness, NoV, NoL, cloth);
				float3 F = Fresnel(f0, LoH);
				return (D * V) * F;
			}
			float3 getBoxProjection(float3 direction, float3 position, float4 cubemapPosition, float3 boxMin, float3 boxMax)
			{
				#if UNITY_SPECCUBE_BOX_PROJECTION
				if (cubemapPosition.w > 0)
				{
					float3 factors = ((direction > 0 ? boxMax : boxMin) - position) / direction;
					float scalar = min(min(factors.x, factors.y), factors.z);
					direction = direction * scalar + (position - cubemapPosition.xyz);
				}
				#endif
				return direction;
			}
			float SpecularAO(float NoV, float ao, float roughness)
			{
				return clamp(pow(NoV + ao, exp2(-16.0 * roughness - 1.0)) - 1.0 + ao, 0.0, 1.0);
			}
			float3 IndirectSpecular(float3 dfg, float roughness, float occlusion, float energyCompensation, float cloth, float3 indirectDiffuse, float f0, PoiLight poiLight, PoiFragData poiFragData, PoiCam poiCam, PoiMesh poiMesh)
			{
				float3 normal = poiMesh.normals[1];
				float3 reflDir = reflect(-poiCam.viewDir, normal);
				Unity_GlossyEnvironmentData envData;
				envData.roughness = roughness;
				envData.reflUVW = getBoxProjection(reflDir, poiMesh.worldPos, unity_SpecCube0_ProbePosition,
				unity_SpecCube0_BoxMin.xyz, unity_SpecCube0_BoxMax.xyz);
				float3 probe0 = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData);
				float3 indirectSpecular = probe0;
				#if UNITY_SPECCUBE_BLENDING
				
				if (unity_SpecCube0_BoxMin.w < 0.99999)
				{
					envData.reflUVW = getBoxProjection(reflDir, poiMesh.worldPos, unity_SpecCube1_ProbePosition, unity_SpecCube1_BoxMin.xyz, unity_SpecCube1_BoxMax.xyz);
					float3 probe1 = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1, unity_SpecCube0), unity_SpecCube1_HDR, envData);
					indirectSpecular = lerp(probe1, probe0, unity_SpecCube0_BoxMin.w);
				}
				#endif
				float horizon = min(1 + dot(reflDir, normal), 1);
				indirectSpecular = indirectSpecular * horizon * horizon * energyCompensation * EnvBRDFMultiscatter(dfg, f0, cloth);
				indirectSpecular *= SpecularAO(poiLight.nDotV, occlusion, roughness);
				return indirectSpecular;
			};
			#undef LERP
			#undef HARD
			#undef CLOTHMODE
			#endif
			float _LightingWrappedWrap;
			float _LightingWrappedNormalization;
			float RTWrapFunc(in float dt, in float w, in float norm)
			{
				float cw = saturate(w);
				float o = (dt + cw) / ((1.0 + cw) * (1.0 + cw * norm));
				float flt = 1.0 - 0.85 * norm;
				if (w > 1.0)
				{
					o = lerp(o, flt, w - 1.0);
				}
				return o;
			}
			float3 GreenWrapSH(float fA) // Greens unoptimized and non-normalized
			{
				float fAs = saturate(fA);
				float4 t = float4(fA + 1, fAs - 1, fA - 2, fAs + 1); // DJL edit: allow wrapping to L0-only at w=2
				return float3(t.x, -t.z * t.x / 3, 0.25 * t.y * t.y * t.w);
			}
			float3 GreenWrapSHOpt(float fW) // optimised and normalized https://blog.selfshadow.com/2012/01/07/righting-wrap-part-2/
			{
				const float4 t0 = float4(0.0, 1.0 / 4.0, -1.0 / 3.0, -1.0 / 2.0);
				const float4 t1 = float4(1.0, 2.0 / 3.0, 1.0 / 4.0, 0.0);
				float3 fWs = float3(fW, fW, saturate(fW)); // DJL edit: allow wrapping to L0-only at w=2
				float3 r;
				r.xyz = t0.xxy * fWs + t0.xzw;
				r.xyz = r.xyz * fWs + t1.xyz;
				return r;
			}
			float3 ShadeSH9_wrapped(float3 normal, float wrap)
			{
				float3 x0, x1, x2;
				float3 conv = lerp(GreenWrapSH(wrap), GreenWrapSHOpt(wrap), float(0)); // Should try optimizing this...
				conv *= float3(1, 1.5, 4); // Undo pre-applied cosine convolution by using the inverse
				x0 = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
				float3 L2_0 = float3(unity_SHBr.z, unity_SHBg.z, unity_SHBb.z) / - 3.0;
				x0 -= L2_0;
				x1.r = dot(unity_SHAr.xyz, normal);
				x1.g = dot(unity_SHAg.xyz, normal);
				x1.b = dot(unity_SHAb.xyz, normal);
				float4 vB = normal.xyzz * normal.yzzx;
				x2.r = dot(unity_SHBr, vB);
				x2.g = dot(unity_SHBg, vB);
				x2.b = dot(unity_SHBb, vB);
				float vC = normal.x * normal.x - normal.y * normal.y;
				x2 += unity_SHC.rgb * vC;
				x2 += L2_0;
				return x0 * conv.x + x1 * conv.y + x2 * conv.z;
			}
			float3 GetSHDirectionL1()
			{
				return Unity_SafeNormalize((unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz));
			}
			half3 GetSHMaxL1()
			{
				float3 maxDirection = GetSHDirectionL1();
				return ShadeSH9_wrapped(maxDirection, 0);
			}
			#ifdef _LIGHTINGMODE_SHADEMAP
			void applyShadeMapping(inout PoiFragData poiFragData, PoiMesh poiMesh, inout PoiLight poiLight)
			{
				float MainColorFeatherStep = float(0.5) - float(0.0001);
				float firstColorFeatherStep = float(0) - float(0.0001);
				#if defined(PROP_1ST_SHADEMAP) || !defined(OPTIMIZER_ENABLED)
				float4 firstShadeMap = POI2D_SAMPLER_PAN(_1st_ShadeMap, _MainTex, poiUV(poiMesh.uv[float(0)], float4(1,1,0,0)), float4(0,0,0,0));
				#else
				float4 firstShadeMap = float4(1, 1, 1, 1);
				#endif
				firstShadeMap = lerp(firstShadeMap, float4(poiFragData.baseColor, 1), float(0));
				#if defined(PROP_2ND_SHADEMAP) || !defined(OPTIMIZER_ENABLED)
				float4 secondShadeMap = POI2D_SAMPLER_PAN(_2nd_ShadeMap, _MainTex, poiUV(poiMesh.uv[float(0)], float4(1,1,0,0)), float4(0,0,0,0));
				#else
				float4 secondShadeMap = float4(1, 1, 1, 1);
				#endif
				secondShadeMap = lerp(secondShadeMap, firstShadeMap, float(0));
				firstShadeMap.rgb *= float4(1,1,1,1).rgb; //* lighColor
				secondShadeMap.rgb *= float4(1,1,1,1).rgb; //* LightColor;
				float shadowMask = 1;
				shadowMask *= float(0) ?(float(0) ?(1.0 - firstShadeMap.a) : firstShadeMap.a) : 1;
				shadowMask *= float(0) ?(float(0) ?(1.0 - secondShadeMap.a) : secondShadeMap.a) : 1;
				float mainShadowMask = saturate(1 - ((poiLight.lightMap) - MainColorFeatherStep) / (float(0.5) - MainColorFeatherStep) * (shadowMask));
				float firstSecondShadowMask = saturate(1 - ((poiLight.lightMap) - firstColorFeatherStep) / (float(0) - firstColorFeatherStep) * (shadowMask));
				mainShadowMask *= poiLight.shadowMask * float(1);
				firstSecondShadowMask *= poiLight.shadowMask * float(1);
				if (float(0) == 0)
				{
					poiFragData.baseColor.rgb = lerp(poiFragData.baseColor.rgb, lerp(firstShadeMap.rgb, secondShadeMap.rgb, firstSecondShadowMask), mainShadowMask);
				}
				else
				{
					poiFragData.baseColor.rgb *= lerp(1, lerp(firstShadeMap.rgb, secondShadeMap.rgb, firstSecondShadowMask), mainShadowMask);
				}
				poiLight.rampedLightMap = 1 - mainShadowMask;
			}
			#endif
			void ApplySubtractiveLighting(inout UnityIndirect indirectLight)
			{
				#if SUBTRACTIVE_LIGHTING
				poiLight.attenuation = FadeShadows(lerp(1, poiLight.attenuation, _AttenuationMultiplier));
				float ndotl = saturate(dot(i.normal, _WorldSpaceLightPos0.xyz));
				float3 shadowedLightEstimate = ndotl * (1 - poiLight.attenuation) * _LightColor0.rgb;
				float3 subtractedLight = indirectLight.diffuse - shadowedLightEstimate;
				subtractedLight = max(subtractedLight, unity_ShadowColor.rgb);
				subtractedLight = lerp(subtractedLight, indirectLight.diffuse, _LightShadowData.x);
				indirectLight.diffuse = min(subtractedLight, indirectLight.diffuse);
				#endif
			}
			UnityIndirect CreateIndirectLight(in PoiMesh poiMesh, in PoiCam poiCam, in PoiLight poiLight)
			{
				UnityIndirect indirectLight;
				indirectLight.diffuse = 0;
				indirectLight.specular = 0;
				#if defined(LIGHTMAP_ON)
				indirectLight.diffuse = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, poiMesh.lightmapUV.xy));
				#if defined(DIRLIGHTMAP_COMBINED)
				float4 lightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(
				unity_LightmapInd, unity_Lightmap, poiMesh.lightmapUV.xy
				);
				indirectLight.diffuse = DecodeDirectionalLightmap(
				indirectLight.diffuse, lightmapDirection, poiMesh.normals[1]
				);
				#endif
				ApplySubtractiveLighting(indirectLight);
				#endif
				#if defined(DYNAMICLIGHTMAP_ON)
				float3 dynamicLightDiffuse = DecodeRealtimeLightmap(
				UNITY_SAMPLE_TEX2D(unity_DynamicLightmap, poiMesh.lightmapUV.zw)
				);
				#if defined(DIRLIGHTMAP_COMBINED)
				float4 dynamicLightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(
				unity_DynamicDirectionality, unity_DynamicLightmap,
				poiMesh.lightmapUV.zw
				);
				indirectLight.diffuse += DecodeDirectionalLightmap(
				dynamicLightDiffuse, dynamicLightmapDirection, poiMesh.normals[1]
				);
				#else
				indirectLight.diffuse += dynamicLightDiffuse;
				#endif
				#endif
				#if !defined(LIGHTMAP_ON) && !defined(DYNAMICLIGHTMAP_ON)
				#if UNITY_LIGHT_PROBE_PROXY_VOLUME
				if (unity_ProbeVolumeParams.x == 1)
				{
					indirectLight.diffuse = SHEvalLinearL0L1_SampleProbeVolume(
					float4(poiMesh.normals[1], 1), poiMesh.worldPos
					);
					indirectLight.diffuse = max(0, indirectLight.diffuse);
					#if defined(UNITY_COLORSPACE_GAMMA)
					indirectLight.diffuse = LinearToGammaSpace(indirectLight.diffuse);
					#endif
				}
				else
				{
					indirectLight.diffuse += max(0, ShadeSH9(float4(poiMesh.normals[1], 1)));
				}
				#else
				indirectLight.diffuse += max(0, ShadeSH9(float4(poiMesh.normals[1], 1)));
				#endif
				#endif
				indirectLight.diffuse *= poiLight.occlusion;
				return indirectLight;
			}
			void calculateShading(inout PoiLight poiLight, inout PoiFragData poiFragData, in PoiMesh poiMesh, in PoiCam poiCam)
			{
				#ifdef UNITY_PASS_FORWARDBASE
				float shadowStrength = float(1) * poiLight.shadowMask;
				#ifdef POI_PASS_OUTLINE
				shadowStrength = lerp(0, shadowStrength, _OutlineShadowStrength);
				#endif
				#ifdef _LIGHTINGMODE_FLAT
				poiLight.finalLighting = poiLight.directColor;
				poiLight.rampedLightMap = poiLight.nDotLSaturated;
				#endif
				#ifdef _LIGHTINGMODE_MULTILAYER_MATH
				float4 lns = float4(1, 1, 1, 1);
				lns.x = lilTooningNoSaturate(poiLight.lightMap, float(0.5), float(0.1));
				lns.y = lilTooningNoSaturate(poiLight.lightMap, float(0.5), float(0.3));
				lns.z = lilTooningNoSaturate(poiLight.lightMap, float(0.25), float(0.1));
				lns.w = lilTooningNoSaturate(poiLight.lightMap, float(0.5), float(0.1), float(0));
				lns = saturate(lns);
				float3 indirectColor = 1;
				if (float4(0.4479884,0.5225216,0.6920712,1).a > 0)
				{
					#if defined(PROP_SHADOWCOLORTEX) || !defined(OPTIMIZER_ENABLED)
					float4 shadowColorTex = POI2D_SAMPLER_PAN(_ShadowColorTex, _MainTex, poiUV(poiMesh.uv[float(0)], float4(1,1,0,0)), float4(0,0,0,0));
					#else
					float4 shadowColorTex = float4(1, 1, 1, 1);
					#endif
					indirectColor = lerp(float3(1, 1, 1), shadowColorTex.rgb, shadowColorTex.a) * float4(0.4479884,0.5225216,0.6920712,1).rgb;
				}
				if (float4(0,0,0,0).a > 0)
				{
					#if defined(PROP_SHADOW2NDCOLORTEX) || !defined(OPTIMIZER_ENABLED)
					float4 shadow2ndColorTex = POI2D_SAMPLER_PAN(_Shadow2ndColorTex, _MainTex, poiUV(poiMesh.uv[float(0)], float4(1,1,0,0)), float4(0,0,0,0));
					#else
					float4 shadow2ndColorTex = float4(1, 1, 1, 1);
					#endif
					shadow2ndColorTex.rgb = lerp(float3(1, 1, 1), shadow2ndColorTex.rgb, shadow2ndColorTex.a) * float4(0,0,0,0).rgb;
					lns.y = float4(0,0,0,0).a - lns.y * float4(0,0,0,0).a;
					indirectColor = lerp(indirectColor, shadow2ndColorTex.rgb, lns.y);
				}
				if (float4(0,0,0,0).a > 0)
				{
					#if defined(PROP_SHADOW3RDCOLORTEX) || !defined(OPTIMIZER_ENABLED)
					float4 shadow3rdColorTex = POI2D_SAMPLER_PAN(_Shadow3rdColorTex, _MainTex, poiUV(poiMesh.uv[float(0)], float4(1,1,0,0)), float4(0,0,0,0));
					#else
					float4 shadow3rdColorTex = float4(1, 1, 1, 1);
					#endif
					shadow3rdColorTex.rgb = lerp(float3(1, 1, 1), shadow3rdColorTex.rgb, shadow3rdColorTex.a) * float4(0,0,0,0).rgb;
					lns.z = float4(0,0,0,0).a - lns.z * float4(0,0,0,0).a;
					indirectColor = lerp(indirectColor, shadow3rdColorTex.rgb, lns.z);
				}
				poiLight.rampedLightMap = lns.x;
				indirectColor = lerp(indirectColor, 1, lns.w * float4(1,0,0,1).rgb);
				indirectColor = indirectColor * lerp(poiLight.indirectColor, poiLight.directColor, float(0));
				indirectColor = lerp(poiLight.directColor, indirectColor, float(1) * poiLight.shadowMask);
				poiLight.finalLighting = lerp(indirectColor, poiLight.directColor, lns.x);
				#endif
				#ifdef _LIGHTINGMODE_SHADEMAP
				poiLight.finalLighting = poiLight.directColor;
				#endif
				#ifdef _LIGHTINGMODE_REALISTIC
				UnityLight light;
				light.dir = poiLight.direction;
				light.color = saturate(_LightColor0.rgb * lerp(1, poiLight.attenuation, poiLight.attenuationStrength) * poiLight.detailShadow);
				light.ndotl = poiLight.nDotLSaturated;
				poiLight.rampedLightMap = poiLight.nDotLSaturated;
				poiLight.finalLighting = max(UNITY_BRDF_PBS(1, 0, 0, 0, poiMesh.normals[1], poiCam.viewDir, light, CreateIndirectLight(poiMesh, poiCam, poiLight)).xyz, float(0));
				#endif
				#ifdef _LIGHTINGMODE_CLOTH
				#if defined(PROP_MOCHIEMETALLICMAP) || !defined(OPTIMIZER_ENABLED)
				float4 clothmapsample = POI2D_MAINTEX_SAMPLER_PAN_INLINED(_ClothMetallicSmoothnessMap, poiMesh);
				float roughness = 1 - (clothmapsample.a * float(0.5));
				float reflectance = float(0.5) * clothmapsample.b;
				float clothmask = clothmapsample.g;
				float metallic = pow(clothmapsample.r * _ClothMetallic, 2) * ClothMetallic(clothmask);
				roughness = float(0) == 1 ? 1 - roughness : roughness;
				#else
				float roughness = 1 - (float(0.5));
				float metallic = pow(_ClothMetallic, 2);
				float reflectance = float(0.5);
				float clothmask = 1;
				#endif
				float perceptualRoughness = pow(roughness, 2);
				float clampedRoughness = max(0.002, perceptualRoughness);
				float f0 = 0.16 * reflectance * reflectance * (1 - metallic) + poiFragData.baseColor * metallic;
				float3 fresnel = Fresnel(f0, poiLight.nDotV);
				float3 dfg = SampleDFG(poiLight.nDotV, perceptualRoughness);
				float energyCompensation = EnvBRDFEnergyCompensation(dfg, f0, clothmask);
				poiLight.finalLighting = Fd_Burley(perceptualRoughness, poiLight.nDotV, poiLight.nDotLSaturated, poiLight.lDotH);
				poiLight.finalLighting *= _LightColor0 * poiLight.attenuation * poiLight.nDotLSaturated;
				float3 specular = max(0, Specular(clampedRoughness, poiLight, f0, poiMesh.normals[1], clothmask) * poiLight.finalLighting * energyCompensation * UNITY_PI); // (D * V) * F
				float3 L0 = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
				float3 indirectDiffuse;
				indirectDiffuse.r = shEvaluateDiffuseL1Geomerics_local(L0.r, unity_SHAr.xyz, poiMesh.normals[1]);
				indirectDiffuse.g = shEvaluateDiffuseL1Geomerics_local(L0.g, unity_SHAg.xyz, poiMesh.normals[1]);
				indirectDiffuse.b = shEvaluateDiffuseL1Geomerics_local(L0.b, unity_SHAb.xyz, poiMesh.normals[1]);
				indirectDiffuse = max(0, indirectDiffuse);
				float3 indirectSpecular = IndirectSpecular(dfg, roughness, poiLight.occlusion, energyCompensation, clothmask, indirectDiffuse, f0, poiLight, poiFragData, poiCam, poiMesh);
				poiLight.finalLightAdd += max(0, specular + indirectSpecular);
				poiLight.finalLighting += indirectDiffuse * poiLight.occlusion;
				poiFragData.baseColor.xyz *= (1 - metallic);
				#endif
				#ifdef _LIGHTINGMODE_WRAPPED
				#define GREYSCALE_VECTOR float3(.33333, .33333, .33333)
				float3 directColor = _LightColor0.rgb * saturate(RTWrapFunc(poiLight.nDotL, float(0), float(0))) * lerp(1, poiLight.attenuation, poiLight.attenuationStrength);
				float3 indirectColor = ShadeSH9_wrapped(poiMesh.normals[float(0)], float(0)) * poiLight.occlusion;
				float3 ShadeSH9Plus_2 = GetSHMaxL1();
				float bw_topDirectLighting_2 = dot(_LightColor0.rgb, GREYSCALE_VECTOR);
				float bw_directLighting = dot(directColor, GREYSCALE_VECTOR);
				float bw_indirectLighting = dot(indirectColor, GREYSCALE_VECTOR);
				float bw_topIndirectLighting = dot(ShadeSH9Plus_2, GREYSCALE_VECTOR);
				poiLight.lightMap = smoothstep(0, bw_topIndirectLighting + bw_topDirectLighting_2, bw_indirectLighting + bw_directLighting) * poiLight.detailShadow;
				poiLight.rampedLightMap = saturate((poiLight.lightMap - (1 - float(0.5))) / saturate((1 - float(0)) - (1 - float(0.5)) + fwidth(poiLight.lightMap)));
				float3 mathRamp = lerp(float3(1, 1, 1), saturate(lerp((float4(1,1,1,1) * lerp(indirectColor, 1, float(0))), float3(1, 1, 1), saturate(poiLight.rampedLightMap))), float(1));
				float3 finalWrap = directColor + indirectColor;
				if (float(1))
				{
					finalWrap = clamp(finalWrap, float(0), float(1));
				}
				else
				{
					finalWrap = max(finalWrap, float(0));
				}
				poiLight.finalLighting = finalWrap * saturate(mathRamp + 1 - float(1));
				#endif
				#ifdef _LIGHTINGMODE_SKIN
				float3 ambientNormalWorld = poiMesh.normals[1];//aTangentToWorld(s, s.blurredNormalTangent);
				poiLight.rampedLightMap = poiLight.nDotLSaturated;
				float subsurface = 1;
				float skinScattering = saturate(subsurface * float(1) * 2);
				half3 absorption = exp((1.0h - subsurface) * float4(-8,-40,-64,0).rgb);
				absorption *= saturate(poiFragData.baseColor * unity_ColorSpaceDouble.rgb);
				ambientNormalWorld = normalize(lerp(poiMesh.normals[1], ambientNormalWorld, float(0.7)));
				float ndlBlur = dot(poiMesh.normals[1], poiLight.direction) * 0.5h + 0.5h;
				float lumi = dot(poiLight.directColor, half3(0.2126h, 0.7152h, 0.0722h));
				float4 sssLookupUv = float4(ndlBlur, skinScattering * lumi, 0.0f, 0.0f);
				half3 sss = poiLight.lightMap * poiLight.attenuation * tex2Dlod(_SkinLUT, sssLookupUv).rgb;
				poiLight.finalLighting = min(lerp(poiLight.indirectColor * float4(1,1,1,1), float4(1,1,1,1), float(0)) + (sss * poiLight.directColor), poiLight.directColor);
				#endif
				#ifdef _LIGHTINGMODE_SDF
				#endif
				#endif
				#ifdef UNITY_PASS_FORWARDADD
				if (float(1) == 0)
				{
					poiLight.rampedLightMap = max(0, poiLight.nDotL);
					poiLight.finalLighting = poiLight.directColor * poiLight.attenuation * max(0, poiLight.nDotL) * poiLight.detailShadow * poiLight.additiveShadow;
				}
				if (float(1) == 1)
				{
					#if defined(POINT_COOKIE) || defined(DIRECTIONAL_COOKIE)
					float passthrough = 0;
					#else
					float passthrough = float(0.5);
					#endif
					if (float(0.5) == float(0)) float(0.5) += 0.001;
					poiLight.rampedLightMap = smoothstep(float(0.5), float(0), 1 - (.5 * poiLight.nDotL + .5));
					#if defined(POINT) || defined(SPOT)
					poiLight.finalLighting = lerp(poiLight.directColor * max(poiLight.additiveShadow, passthrough), poiLight.indirectColor, smoothstep(float(0), float(0.5), 1 - (.5 * poiLight.nDotL + .5))) * poiLight.attenuation * poiLight.detailShadow;
					#else
					poiLight.finalLighting = lerp(poiLight.directColor * max(poiLight.attenuation, passthrough), poiLight.indirectColor, smoothstep(float(0), float(0.5), 1 - (.5 * poiLight.nDotL + .5))) * poiLight.detailShadow;
					#endif
				}
				if (float(1) == 2)
				{
				}
				#endif
				#if defined(VERTEXLIGHT_ON) && defined(POI_VERTEXLIGHT_ON)
				float3 vertexLighting = float3(0, 0, 0);
				for (int index = 0; index < 4; index++)
				{
					if (float(1) == 0)
					{
						vertexLighting += poiLight.vColor[index] * poiLight.vAttenuationDotNL[index] * poiLight.detailShadow; // Realistic
					}
					if (float(1) == 1) // Toon
					{
						vertexLighting += lerp(poiLight.vColor[index] * poiLight.vAttenuation[index], poiLight.vColor[index] * float(0.5) * poiLight.vAttenuation[index], smoothstep(float(0), float(0.5), .5 * poiLight.vDotNL[index] + .5)) * poiLight.detailShadow;
					}
				}
				float3 mixedLight = poiLight.finalLighting;
				poiLight.finalLighting = vertexLighting + poiLight.finalLighting;
				#endif
			}
			#endif
			void blendMatcap(inout PoiLight poiLight, inout PoiFragData poiFragData, float add, float lightAdd, float multiply, float replace, float mixed, float4 matcapColor, float matcapMask, float emissionStrength, float matcapLightMask
			#ifdef POI_BLACKLIGHT
			, uint blackLightMaskIndex
			#endif
			)
			{
				if (matcapLightMask)
				{
					matcapMask *= lerp(1, poiLight.rampedLightMap, matcapLightMask);
				}
				#ifdef POI_BLACKLIGHT
				if (blackLightMaskIndex != 4)
				{
					matcapMask *= blackLightMask[blackLightMaskIndex];
				}
				#endif
				poiFragData.baseColor.rgb = lerp(poiFragData.baseColor.rgb, matcapColor.rgb, replace * matcapMask * matcapColor.a * .999999);
				poiFragData.baseColor.rgb *= lerp(1, matcapColor.rgb, multiply * matcapMask * matcapColor.a);
				poiFragData.baseColor.rgb += matcapColor.rgb * add * matcapMask * matcapColor.a;
				poiLight.finalLightAdd += matcapColor.rgb * lightAdd * matcapMask * matcapColor.a;
				poiFragData.baseColor.rgb = lerp(poiFragData.baseColor.rgb, poiFragData.baseColor.rgb + poiFragData.baseColor.rgb * matcapColor.rgb, mixed * matcapMask * matcapColor.a);
				poiFragData.emission += matcapColor.rgb * emissionStrength * matcapMask * matcapColor.a;
			}
			#if defined(POI_MATCAP0) || defined(COLOR_GRADING_HDR_3D)
			void applyMatcap(inout PoiFragData poiFragData, in PoiCam poiCam, in PoiMesh poiMesh, inout PoiLight poiLight, in PoiMods poiMods)
			{
				float4 matcap = 0;
				float matcapMask = 0;
				float4 matcap2 = 0;
				float matcap2Mask = 0;
				float2 matcapUV = 0;
			}
			#endif
			float calculateGlowInTheDark(in float minLight, in float maxLight, in float minEmissionMultiplier, in float maxEmissionMultiplier, in float enabled, in float worldOrMesh, in PoiLight poiLight)
			{
				float glowInTheDarkMultiplier = 1;
				if (enabled)
				{
					float3 lightValue = worldOrMesh ? calculateluminance(poiLight.finalLighting.rgb) : calculateluminance(poiLight.directColor.rgb);
					float gitdeAlpha = saturate(inverseLerp(minLight, maxLight, lightValue));
					glowInTheDarkMultiplier = lerp(minEmissionMultiplier, maxEmissionMultiplier, gitdeAlpha);
				}
				return glowInTheDarkMultiplier;
			}
			float calculateScrollingEmission(in float3 direction, in float velocity, in float interval, in float scrollWidth, float offset, float3 position)
			{
				scrollWidth = max(scrollWidth, 0);
				float phase = 0;
				phase = dot(position, direction);
				phase -= (_Time.y + offset) * velocity;
				phase /= interval;
				phase -= floor(phase);
				phase = saturate(phase);
				return(pow(phase, scrollWidth) + pow(1 - phase, scrollWidth * 4)) * 0.5;
			}
			float calculateBlinkingEmission(in float blinkMin, in float blinkMax, in float blinkVelocity, float offset)
			{
				float amplitude = (blinkMax - blinkMin) * 0.5f;
				float base = blinkMin + amplitude;
				return sin((_Time.y + offset) * blinkVelocity) * amplitude + base;
			}
			void applyALEmmissionStrength(in PoiMods poiMods, inout float emissionStrength, in float2 emissionStrengthMod, in float emissionStrengthBand, in float enabled)
			{
			}
			void applyALCenterOutEmission(in PoiMods poiMods, in float nDotV, inout float emissionStrength, in float size, in float band, in float2 emissionToAdd, in float enabled)
			{
			}
			#if defined(MOCHIE_PBR) || defined(POI_CLEARCOAT)
			float GSAA_Filament(float3 worldNormal, float perceptualRoughness, float gsaaVariance, float gsaaThreshold)
			{
				float3 du = ddx(worldNormal);
				float3 dv = ddy(worldNormal);
				float variance = gsaaVariance * (dot(du, du) + dot(dv, dv));
				float roughness = perceptualRoughness * perceptualRoughness;
				float kernelRoughness = min(2.0 * variance, gsaaThreshold);
				float squareRoughness = saturate(roughness * roughness + kernelRoughness);
				return sqrt(sqrt(squareRoughness));
			}
			bool SceneHasReflections()
			{
				float width, height;
				unity_SpecCube0.GetDimensions(width, height);
				return !(width * height < 2);
			}
			float3 GetWorldReflections(float3 reflDir, float3 worldPos, float roughness)
			{
				float3 baseReflDir = reflDir;
				reflDir = BoxProjection(reflDir, worldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
				float4 envSample0 = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflDir, roughness * UNITY_SPECCUBE_LOD_STEPS);
				float3 p0 = DecodeHDR(envSample0, unity_SpecCube0_HDR);
				float interpolator = unity_SpecCube0_BoxMin.w;
				
				if (interpolator < 0.99999)
				{
					float3 refDirBlend = BoxProjection(baseReflDir, worldPos, unity_SpecCube1_ProbePosition, unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax);
					float4 envSample1 = UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(unity_SpecCube1, unity_SpecCube0, refDirBlend, roughness * UNITY_SPECCUBE_LOD_STEPS);
					float3 p1 = DecodeHDR(envSample1, unity_SpecCube1_HDR);
					p0 = lerp(p1, p0, interpolator);
				}
				return p0;
			}
			float3 GetReflections(in PoiCam poiCam, in PoiLight pl, in PoiMesh poiMesh, float roughness, float ForceFallback, float LightFallback, samplerCUBE reflectionCube, float3 reflectionDir)
			{
				float3 reflections = 0;
				float3 lighting = pl.finalLighting;
				if (ForceFallback == 0)
				{
					
					if (SceneHasReflections())
					{
						#ifdef UNITY_PASS_FORWARDBASE
						reflections = GetWorldReflections(reflectionDir, poiMesh.worldPos.xyz, roughness);
						#endif
					}
					else
					{
						#ifdef UNITY_PASS_FORWARDBASE
						reflections = texCUBElod(reflectionCube, float4(reflectionDir, roughness * UNITY_SPECCUBE_LOD_STEPS));
						reflections = DecodeHDR(float4(reflections, 1), _MochieReflCube_HDR) * lerp(1, pl.finalLighting, LightFallback);
						#endif
						#ifdef POI_PASS_ADD
						if (LightFallback)
						{
							reflections = texCUBElod(reflectionCube, float4(reflectionDir, roughness * UNITY_SPECCUBE_LOD_STEPS));
							reflections = DecodeHDR(float4(reflections, 1), _MochieReflCube_HDR) * pl.finalLighting;
						}
						#endif
					}
				}
				else
				{
					#ifdef UNITY_PASS_FORWARDBASE
					reflections = texCUBElod(reflectionCube, float4(reflectionDir, roughness * UNITY_SPECCUBE_LOD_STEPS));
					reflections = DecodeHDR(float4(reflections, 1), _MochieReflCube_HDR) * lerp(1, pl.finalLighting, LightFallback);
					#endif
					#ifdef POI_PASS_ADD
					if (LightFallback)
					{
						reflections = texCUBElod(reflectionCube, float4(reflectionDir, roughness * UNITY_SPECCUBE_LOD_STEPS));
						reflections = DecodeHDR(float4(reflections, 1), _MochieReflCube_HDR) * pl.finalLighting;
					}
					#endif
				}
				reflections *= pl.occlusion;
				return reflections;
			}
			float GetGGXTerm(float nDotL, float nDotV, float nDotH, float roughness)
			{
				float visibilityTerm = 0;
				if (nDotL > 0)
				{
					float rough = roughness;
					float rough2 = roughness * roughness;
					float lambdaV = nDotL * (nDotV * (1 - rough) + rough);
					float lambdaL = nDotV * (nDotL * (1 - rough) + rough);
					visibilityTerm = 0.5f / (lambdaV + lambdaL + 1e-5f);
					float d = (nDotH * rough2 - nDotH) * nDotH + 1.0f;
					float dotTerm = UNITY_INV_PI * rough2 / (d * d + 1e-7f);
					visibilityTerm *= dotTerm * UNITY_PI;
				}
				return visibilityTerm;
			}
			void GetSpecFresTerm(float nDotL, float nDotV, float nDotH, float lDotH, inout float3 specularTerm, inout float3 fresnelTerm, float3 specCol, float roughness)
			{
				specularTerm = GetGGXTerm(nDotL, nDotV, nDotH, roughness);
				fresnelTerm = FresnelTerm(specCol, lDotH);
				specularTerm = max(0, specularTerm * max(0.00001, nDotL));
			}
			float GetRoughness(float smoothness)
			{
				float rough = 1 - smoothness;
				rough *= 1.7 - 0.7 * rough;
				return rough;
			}
			#endif
			float4 frag(v2f i, uint facing : SV_IsFrontFace) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				PoiMesh poiMesh;
				PoiInitStruct(PoiMesh, poiMesh);
				PoiLight poiLight;
				PoiInitStruct(PoiLight, poiLight);
				PoiVertexLights poiVertexLights;
				PoiInitStruct(PoiVertexLights, poiVertexLights);
				PoiCam poiCam;
				PoiInitStruct(PoiCam, poiCam);
				PoiMods poiMods;
				PoiInitStruct(PoiMods, poiMods);
				poiMods.globalEmission = 1;
				PoiFragData poiFragData;
				poiFragData.emission = 0;
				poiFragData.baseColor = float3(0, 0, 0);
				poiFragData.finalColor = float3(0, 0, 0);
				poiFragData.alpha = 1;
				poiMesh.objectPosition = i.objectPos;
				poiMesh.objNormal = i.objNormal;
				poiMesh.normals[0] = i.normal;
				poiMesh.tangent = i.tangent;
				poiMesh.binormal = i.binormal;
				poiMesh.worldPos = i.worldPos.xyz;
				poiMesh.localPos = i.localPos.xyz;
				poiMesh.vertexColor = i.vertexColor;
				poiMesh.isFrontFace = facing;
				#ifndef POI_PASS_OUTLINE
				if (!poiMesh.isFrontFace)
				{
					poiMesh.normals[0] *= -1;
					poiMesh.tangent *= -1;
					poiMesh.binormal *= -1;
				}
				#endif
				poiCam.viewDir = !IsOrthographicCamera() ? normalize(_WorldSpaceCameraPos - i.worldPos.xyz) : normalize(UNITY_MATRIX_I_V._m02_m12_m22);
				float3 tanToWorld0 = float3(i.tangent.x, i.binormal.x, i.normal.x);
				float3 tanToWorld1 = float3(i.tangent.y, i.binormal.y, i.normal.y);
				float3 tanToWorld2 = float3(i.tangent.z, i.binormal.z, i.normal.z);
				float3 ase_tanViewDir = tanToWorld0 * poiCam.viewDir.x + tanToWorld1 * poiCam.viewDir.y + tanToWorld2 * poiCam.viewDir.z;
				poiCam.tangentViewDir = normalize(ase_tanViewDir);
				#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
				poiMesh.lightmapUV = i.lightmapUV;
				#endif
				poiMesh.parallaxUV = poiCam.tangentViewDir.xy / max(poiCam.tangentViewDir.z, 0.0001);
				poiMesh.uv[0] = i.uv[0];
				poiMesh.uv[1] = i.uv[1];
				poiMesh.uv[2] = i.uv[2];
				poiMesh.uv[3] = i.uv[3];
				poiMesh.uv[4] = poiMesh.uv[0];
				poiMesh.uv[5] = poiMesh.worldPos.xz;
				poiMesh.uv[6] = poiMesh.uv[0];
				poiMesh.uv[7] = poiMesh.uv[0];
				poiMesh.uv[4] = calculatePanosphereUV(poiMesh);
				poiMesh.uv[6] = calculatePolarCoordinate(poiMesh);
				float4 mainTexture = UNITY_SAMPLE_TEX2D(_MainTex, poiUV(poiMesh.uv[float(0)].xy, float4(1,1,0,0)) + _Time.x * float4(0,0,0,0));
				float3 mainNormal = UnpackScaleNormal(POI2D_SAMPLER_PAN(_BumpMap, _MainTex, poiUV(poiMesh.uv[float(0)], float4(1,1,0,0)), float4(0,0,0,0)), float(1));
				poiMesh.tangentSpaceNormal = mainNormal;
				#if defined(FINALPASS) && !defined(UNITY_PASS_SHADOWCASTER) && !defined(POI_PASS_OUTLINE)
				ApplyDetailNormal(poiMods, poiMesh);
				#endif
				#if defined(GEOM_TYPE_MESH) && defined(VIGNETTE) && !defined(UNITY_PASS_SHADOWCASTER) && !defined(POI_PASS_OUTLINE)
				calculateRGBNormals(poiMesh);
				#endif
				poiMesh.normals[1] = normalize(
				poiMesh.tangentSpaceNormal.x * poiMesh.tangent.xyz +
				poiMesh.tangentSpaceNormal.y * poiMesh.binormal +
				poiMesh.tangentSpaceNormal.z * poiMesh.normals[0]
				);
				float3 fancyNormal = UnpackNormal(float4(0.5, 0.5, 1, 1));
				poiMesh.normals[0] = normalize(
				fancyNormal.x * poiMesh.tangent.xyz +
				fancyNormal.y * poiMesh.binormal +
				fancyNormal.z * poiMesh.normals[0]
				);
				poiCam.forwardDir = getCameraForward();
				poiCam.worldPos = _WorldSpaceCameraPos;
				poiCam.reflectionDir = reflect(-poiCam.viewDir, poiMesh.normals[1]);
				poiCam.vertexReflectionDir = reflect(-poiCam.viewDir, poiMesh.normals[0]);
				poiCam.distanceToVert = distance(poiMesh.worldPos, poiCam.worldPos);
				poiCam.grabPos = i.grabPos;
				poiCam.screenUV = calcScreenUVs(i.grabPos);
				poiCam.vDotN = abs(dot(poiCam.viewDir, poiMesh.normals[1]));
				poiCam.clipPos = i.pos;
				poiCam.worldDirection = i.worldDirection;
				calculateGlobalThemes(poiMods);
				poiLight.finalLightAdd = 0;
				#if defined(PROP_LIGHTINGAOMAPS) || !defined(OPTIMIZER_ENABLED)
				float4 AOMaps = POI2D_SAMPLER_PAN(_LightingAOMaps, _MainTex, poiUV(poiMesh.uv[float(0)], float4(1,1,0,0)), float4(0,0,0,0));
				poiLight.occlusion = lerp(1, AOMaps.r, float(1)) * lerp(1, AOMaps.g, float(0)) * lerp(1, AOMaps.b, float(0)) * lerp(1, AOMaps.a, float(0));
				#else
				poiLight.occlusion = 1;
				#endif
				#if defined(PROP_LIGHTINGDETAILSHADOWMAPS) || !defined(OPTIMIZER_ENABLED)
				float4 DetailShadows = POI2D_SAMPLER_PAN(_LightingDetailShadowMaps, _MainTex, poiUV(poiMesh.uv[float(0)], float4(1,1,0,0)), float4(0,0,0,0));
				poiLight.detailShadow = lerp(1, DetailShadows.r, float(1)) * lerp(1, DetailShadows.g, float(0)) * lerp(1, DetailShadows.b, float(0)) * lerp(1, DetailShadows.a, float(0));
				#else
				poiLight.detailShadow = 1;
				#endif
				#if defined(PROP_LIGHTINGSHADOWMASKS) || !defined(OPTIMIZER_ENABLED)
				float4 ShadowMasks = POI2D_SAMPLER_PAN(_LightingShadowMasks, _MainTex, poiUV(poiMesh.uv[float(0)], float4(1,1,0,0)), float4(0,0,0,0));
				poiLight.shadowMask = lerp(1, ShadowMasks.r, float(1)) * lerp(1, ShadowMasks.g, float(0)) * lerp(1, ShadowMasks.b, float(0)) * lerp(1, ShadowMasks.a, float(0));
				#else
				poiLight.shadowMask = 1;
				#endif
				#ifdef UNITY_PASS_FORWARDBASE
				bool lightExists = false;
				if (any(_LightColor0.rgb >= 0.002))
				{
					lightExists = true;
				}
				#if defined(VERTEXLIGHT_ON) && defined(POI_VERTEXLIGHT_ON)
				float4 toLightX = unity_4LightPosX0 - i.worldPos.x;
				float4 toLightY = unity_4LightPosY0 - i.worldPos.y;
				float4 toLightZ = unity_4LightPosZ0 - i.worldPos.z;
				float4 lengthSq = 0;
				lengthSq += toLightX * toLightX;
				lengthSq += toLightY * toLightY;
				lengthSq += toLightZ * toLightZ;
				float4 lightAttenSq = unity_4LightAtten0;
				float4 atten = 1.0 / (1.0 + lengthSq * lightAttenSq);
				float4 vLightWeight = saturate(1 - (lengthSq * lightAttenSq / 25));
				poiLight.vAttenuation = min(atten, vLightWeight * vLightWeight);
				poiLight.vDotNL = 0;
				poiLight.vDotNL += toLightX * poiMesh.normals[1].x;
				poiLight.vDotNL += toLightY * poiMesh.normals[1].y;
				poiLight.vDotNL += toLightZ * poiMesh.normals[1].z;
				float4 corr = rsqrt(lengthSq);
				poiLight.vertexVDotNL = max(0, poiLight.vDotNL * corr);
				poiLight.vertexVDotNL = 0;
				poiLight.vertexVDotNL += toLightX * poiMesh.normals[0].x;
				poiLight.vertexVDotNL += toLightY * poiMesh.normals[0].y;
				poiLight.vertexVDotNL += toLightZ * poiMesh.normals[0].z;
				poiLight.vertexVDotNL = max(0, poiLight.vDotNL * corr);
				poiLight.vAttenuationDotNL = saturate(poiLight.vAttenuation * saturate(poiLight.vDotNL));
				for (int index = 0; index < 4; index++)
				{
					poiLight.vPosition[index] = float3(unity_4LightPosX0[index], unity_4LightPosY0[index], unity_4LightPosZ0[index]);
					float3 vertexToLightSource = poiLight.vPosition[index] - poiMesh.worldPos;
					poiLight.vDirection[index] = normalize(vertexToLightSource);
					poiLight.vColor[index] = unity_LightColor[index].rgb;
					poiLight.vHalfDir[index] = Unity_SafeNormalize(poiLight.vDirection[index] + poiCam.viewDir);
					poiLight.vDotNL[index] = dot(poiMesh.normals[1], -poiLight.vDirection[index]);
					poiLight.vCorrectedDotNL[index] = .5 * (poiLight.vDotNL[index] + 1);
					poiLight.vDotLH[index] = saturate(dot(poiLight.vDirection[index], poiLight.vHalfDir[index]));
					poiLight.vDotNH[index] = dot(poiMesh.normals[1], poiLight.vHalfDir[index]);
					poiLight.vertexVDotNH[index] = saturate(dot(poiMesh.normals[0], poiLight.vHalfDir[index]));
				}
				#endif
				if (float(0) == 0) // Poi Custom Light Color
				{
					float3 magic = max(BetterSH9(normalize(unity_SHAr + unity_SHAg + unity_SHAb)), 0);
					float3 normalLight = _LightColor0.rgb + BetterSH9(float4(0, 0, 0, 1));
					float magiLumi = calculateluminance(magic);
					float normaLumi = calculateluminance(normalLight);
					float maginormalumi = magiLumi + normaLumi;
					float magiratio = magiLumi / maginormalumi;
					float normaRatio = normaLumi / maginormalumi;
					float target = calculateluminance(magic * magiratio + normalLight * normaRatio);
					float3 properLightColor = magic + normalLight;
					float properLuminance = calculateluminance(magic + normalLight);
					poiLight.directColor = properLightColor * max(0.0001, (target / properLuminance));
					poiLight.indirectColor = BetterSH9(float4(lerp(0, poiMesh.normals[1], float(0)), 1));
				}
				if (float(0) == 1) // More standard approach to light color
				{
					float3 indirectColor = BetterSH9(float4(poiMesh.normals[1], 1));
					if (lightExists)
					{
						poiLight.directColor = _LightColor0.rgb;
						poiLight.indirectColor = indirectColor;
					}
					else
					{
						poiLight.directColor = indirectColor * 0.6;
						poiLight.indirectColor = indirectColor * 0.5;
					}
				}
				if (float(0) == 2) // UTS style
				{
					poiLight.indirectColor = saturate(max(half3(0.05, 0.05, 0.05) * float(1), max(ShadeSH9(half4(0.0, 0.0, 0.0, 1.0)), ShadeSH9(half4(0.0, -1.0, 0.0, 1.0)).rgb) * float(1)));
					poiLight.directColor = max(poiLight.indirectColor, _LightColor0.rgb);
				}
				float lightMapMode = float(0);
				if (float(0) == 0)
				{
					poiLight.direction = _WorldSpaceLightPos0.xyz + unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz;
				}
				if (float(0) == 1 || float(0) == 2)
				{
					if (float(0) == 1)
					{
						poiLight.direction = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;;
					}
					if (float(0) == 2)
					{
						poiLight.direction = float4(0,0,0,1);
					}
					if (lightMapMode == 0)
					{
						lightMapMode == 1;
					}
				}
				if (float(0) == 3) // UTS
				{
					float3 defaultLightDirection = normalize(UNITY_MATRIX_V[2].xyz + UNITY_MATRIX_V[1].xyz);
					float3 lightDirection = normalize(lerp(defaultLightDirection, _WorldSpaceLightPos0.xyz, any(_WorldSpaceLightPos0.xyz)));
					poiLight.direction = lightDirection;
				}
				if (!any(poiLight.direction))
				{
					poiLight.direction = float3(.4, 1, .4);
				}
				poiLight.direction = normalize(poiLight.direction);
				poiLight.attenuationStrength = float(0);
				poiLight.attenuation = 1;
				if (!all(_LightColor0.rgb == 0.0))
				{
					UNITY_LIGHT_ATTENUATION(attenuation, i, poiMesh.worldPos)
					poiLight.attenuation *= attenuation;
				}
				if (!any(poiLight.directColor) && !any(poiLight.indirectColor) && lightMapMode == 0)
				{
					lightMapMode = 1;
					if (float(0) == 0)
					{
						poiLight.direction = normalize(float3(.4, 1, .4));
					}
				}
				poiLight.halfDir = normalize(poiLight.direction + poiCam.viewDir);
				poiLight.vertexNDotL = dot(poiMesh.normals[0], poiLight.direction);
				poiLight.nDotL = dot(poiMesh.normals[1], poiLight.direction);
				poiLight.nDotLSaturated = saturate(poiLight.nDotL);
				poiLight.nDotLNormalized = (poiLight.nDotL + 1) * 0.5;
				poiLight.nDotV = abs(dot(poiMesh.normals[1], poiCam.viewDir));
				poiLight.vertexNDotV = abs(dot(poiMesh.normals[0], poiCam.viewDir));
				poiLight.nDotH = dot(poiMesh.normals[1], poiLight.halfDir);
				poiLight.vertexNDotH = max(0.00001, dot(poiMesh.normals[0], poiLight.halfDir));
				poiLight.lDotv = dot(poiLight.direction, poiCam.viewDir);
				poiLight.lDotH = max(0.00001, dot(poiLight.direction, poiLight.halfDir));
				if (lightMapMode == 0)
				{
					float3 ShadeSH9Plus = GetSHLength();
					float3 ShadeSH9Minus = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w) + float3(unity_SHBr.z, unity_SHBg.z, unity_SHBb.z) / 3.0;
					float3 greyScaleVector = float3(.33333, .33333, .33333);
					float bw_lightColor = dot(poiLight.directColor, greyScaleVector);
					float bw_directLighting = (((poiLight.nDotL * 0.5 + 0.5) * bw_lightColor * lerp(1, poiLight.attenuation, poiLight.attenuationStrength)) + dot(ShadeSH9(float4(poiMesh.normals[1], 1)), greyScaleVector));
					float bw_bottomIndirectLighting = dot(ShadeSH9Minus, greyScaleVector);
					float bw_topIndirectLighting = dot(ShadeSH9Plus, greyScaleVector);
					float lightDifference = ((bw_topIndirectLighting + bw_lightColor) - bw_bottomIndirectLighting);
					poiLight.lightMap = smoothstep(0, lightDifference, bw_directLighting - bw_bottomIndirectLighting) * poiLight.detailShadow;
				}
				if (lightMapMode == 1)
				{
					poiLight.lightMap = poiLight.nDotLNormalized * lerp(1, poiLight.attenuation, poiLight.attenuationStrength);
				}
				if (lightMapMode == 2)
				{
					poiLight.lightMap = poiLight.nDotLSaturated * lerp(1, poiLight.attenuation, poiLight.attenuationStrength);
				}
				poiLight.directColor = max(poiLight.directColor, 0.0001);
				poiLight.indirectColor = max(poiLight.indirectColor, 0.0001);
				poiLight.directColor = max(poiLight.directColor, poiLight.directColor / max(0.0001, (calculateluminance(poiLight.directColor) / float(0))));
				poiLight.indirectColor = max(poiLight.indirectColor, poiLight.indirectColor / max(0.0001, (calculateluminance(poiLight.indirectColor) / float(0))));
				poiLight.directColor = lerp(poiLight.directColor, dot(poiLight.directColor, float3(0.299, 0.587, 0.114)), float(0));
				poiLight.indirectColor = lerp(poiLight.indirectColor, dot(poiLight.indirectColor, float3(0.299, 0.587, 0.114)), float(0));
				if (float(1))
				{
					poiLight.directColor = min(poiLight.directColor, float(1));
					poiLight.indirectColor = min(poiLight.indirectColor, float(1));
				}
				if (float(0))
				{
					poiLight.directColor = poiThemeColor(poiMods, float4(1,1,1,1), float(0));
				}
				#ifdef UNITY_PASS_FORWARDBASE
				poiLight.directColor = max(poiLight.directColor * float(1), 0);
				poiLight.directColor = max(poiLight.directColor + float(0), 0);
				poiLight.indirectColor = max(poiLight.indirectColor * float(1), 0);
				poiLight.indirectColor = max(poiLight.indirectColor + float(0), 0);
				#endif
				#endif
				#ifdef UNITY_PASS_FORWARDADD
				#if defined(POI_LIGHT_DATA_ADDITIVE_DIRECTIONAL_ENABLE) && defined(DIRECTIONAL)
				return float4(mainTexture.rgb * .0001, 1);
				#endif
				#if defined(POINT) || defined(SPOT)
				poiLight.direction = normalize(_WorldSpaceLightPos0.xyz - i.worldPos.xyz);
				#ifdef POINT
				poiLight.additiveShadow = UNITY_SHADOW_ATTENUATION(i, poiMesh.worldPos);
				unityShadowCoord3 lightCoord = mul(unity_WorldToLight, unityShadowCoord4(poiMesh.worldPos, 1)).xyz;
				poiLight.attenuation = tex2D(_LightTexture0, dot(lightCoord, lightCoord).rr).r;
				#endif
				#ifdef SPOT
				poiLight.additiveShadow = UNITY_SHADOW_ATTENUATION(i, poiMesh.worldPos);
				unityShadowCoord4 lightCoord = mul(unity_WorldToLight, unityShadowCoord4(poiMesh.worldPos, 1));
				poiLight.attenuation = (lightCoord.z > 0) * UnitySpotCookie(lightCoord) * UnitySpotAttenuate(lightCoord.xyz);
				#endif
				#else
				poiLight.direction = _WorldSpaceLightPos0.xyz;
				UNITY_LIGHT_ATTENUATION(attenuation, i, poiMesh.worldPos)
				poiLight.additiveShadow == 0;
				poiLight.attenuation = attenuation;
				#endif
				poiLight.directColor = float(0) ? min(float(1), _LightColor0.rgb) : _LightColor0.rgb;
				#if defined(POINT_COOKIE) || defined(DIRECTIONAL_COOKIE)
				poiLight.indirectColor = 0;
				#else
				poiLight.indirectColor = lerp(0, poiLight.directColor, float(0.5));
				#endif
				poiLight.directColor = lerp(poiLight.directColor, dot(poiLight.directColor, float3(0.299, 0.587, 0.114)), float(0));
				poiLight.indirectColor = lerp(poiLight.indirectColor, dot(poiLight.indirectColor, float3(0.299, 0.587, 0.114)), float(0));
				poiLight.halfDir = normalize(poiLight.direction + poiCam.viewDir);
				poiLight.nDotL = dot(poiMesh.normals[1], poiLight.direction);
				poiLight.nDotLSaturated = saturate(poiLight.nDotL);
				poiLight.nDotLNormalized = (poiLight.nDotL + 1) * 0.5;
				poiLight.nDotV = abs(dot(poiMesh.normals[1], poiCam.viewDir));
				poiLight.nDotH = dot(poiMesh.normals[1], poiLight.halfDir);
				poiLight.lDotv = dot(poiLight.direction, poiCam.viewDir);
				poiLight.lDotH = dot(poiLight.direction, poiLight.halfDir);
				poiLight.vertexNDotL = dot(poiMesh.normals[0], poiLight.direction);
				poiLight.vertexNDotV = abs(dot(poiMesh.normals[0], poiCam.viewDir));
				poiLight.vertexNDotH = max(0.00001, dot(poiMesh.normals[0], poiLight.halfDir));
				poiLight.lightMap = 1;
				#endif
				poiFragData.baseColor = mainTexture.rgb * poiThemeColor(poiMods, float4(1,1,1,1).rgb, float(0));
				poiFragData.alpha = mainTexture.a * float4(1,1,1,1).a;
				#if defined(PROP_CLIPPINGMASK) || !defined(OPTIMIZER_ENABLED)
				float alphaMask = POI2D_SAMPLER_PAN(_ClippingMask, _MainTex, poiUV(poiMesh.uv[float(0)], float4(1,1,0,0)), float4(0,0,0,0)).r;
				if (float(0))
				{
					alphaMask = 1 - alphaMask;
				}
				#else
				float alphaMask = 1;
				#endif
				poiFragData.alpha *= alphaMask;
				applyAlphaOptions(poiFragData, poiMesh, poiCam, poiMods);
				applyVertexColor(poiFragData, poiMesh);
				#if defined(GEOM_TYPE_BRANCH) || defined(GEOM_TYPE_BRANCH_DETAIL) || defined(GEOM_TYPE_FROND) || defined(DEPTH_OF_FIELD_COC_VIEW)
				applyDecals(poiFragData, poiMesh, poiCam, poiMods, poiLight);
				#endif
				#if defined(_LIGHTINGMODE_SHADEMAP) && defined(VIGNETTE_MASKED)
				#ifndef POI_PASS_OUTLINE
				#ifdef _LIGHTINGMODE_SHADEMAP
				applyShadeMapping(poiFragData, poiMesh, poiLight);
				#endif
				#endif
				#endif
				#ifdef VIGNETTE_MASKED
				#ifdef POI_PASS_OUTLINE
				if (_OutlineLit)
				{
					calculateShading(poiLight, poiFragData, poiMesh, poiCam);
				}
				else
				{
					poiLight.finalLighting = 1;
				}
				#else
				calculateShading(poiLight, poiFragData, poiMesh, poiCam);
				#endif
				#else
				poiLight.finalLighting = 1;
				poiLight.rampedLightMap = aaBlurStep(poiLight.nDotL, 0.1, .1);
				#endif
				#if defined(POI_MATCAP0) || defined(COLOR_GRADING_HDR_3D)
				applyMatcap(poiFragData, poiCam, poiMesh, poiLight, poiMods);
				#endif
				
				if (float(0))
				{
					poiFragData.baseColor *= saturate(poiFragData.alpha);
				}
				poiFragData.finalColor = poiFragData.baseColor;
				poiFragData.finalColor = poiFragData.baseColor * poiLight.finalLighting;
				if (float(0))
				{
					float3 position = float(1) ? poiMesh.worldPos : poiMesh.objectPosition;
					poiFragData.finalColor *= lerp(poiThemeColor(poiMods, float4(0,0,0,1).rgb, float(0)), poiThemeColor(poiMods, float4(1,1,1,1).rgb, float(0)), smoothstep(float(0), float(1), distance(position, poiCam.worldPos)));
				}
				#if defined(_EMISSION) || defined(POI_EMISSION_1) || defined(POI_EMISSION_2) || defined(POI_EMISSION_3)
				float3 emissionBaseReplace = 0;
				#endif
				#if defined(_EMISSION) || defined(POI_EMISSION_1) || defined(POI_EMISSION_2) || defined(POI_EMISSION_3)
				poiFragData.finalColor.rgb = lerp(poiFragData.finalColor.rgb, saturate(emissionBaseReplace), poiMax(emissionBaseReplace));
				#endif
				if (float(0) == 0)
				{
					UNITY_APPLY_FOG(i.fogCoord, poiFragData.finalColor);
				}
				poiFragData.alpha = float(0) ? 1 : poiFragData.alpha;
				ApplyAlphaToCoverage(poiFragData, poiMesh);
				applyDithering(poiFragData, poiCam);
				poiFragData.finalColor += poiLight.finalLightAdd;
				#ifdef UNITY_PASS_FORWARDBASE
				poiFragData.emission = max(poiFragData.emission * float(1), 0);
				poiFragData.finalColor = max(poiFragData.finalColor * float(1), 0);
				#endif
				if (float(0) == POI_MODE_OPAQUE)
				{
					poiFragData.alpha = 1;
				}
				clip(poiFragData.alpha - float(0.5));
				if (float(0) == POI_MODE_FADE)
				{
					clip(poiFragData.alpha - 0.01);
				}
				return float4(poiFragData.finalColor + poiFragData.emission * poiMods.globalEmission, poiFragData.alpha) + POI_SAFE_RGB0;
			}
			ENDCG
		}
		Pass
		{
			Tags { "LightMode" = "ForwardAdd" }
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
			ZWrite Off
			Cull [_Cull]
			AlphaToMask [_AlphaToCoverage]
			ZTest [_ZTest]
			ColorMask [_ColorMask]
			Offset [_OffsetFactor], [_OffsetUnits]
			BlendOp [_AddBlendOp], [_AddBlendOpAlpha]
			Blend [_AddSrcBlend] [_AddDstBlend]
			CGPROGRAM
#define OPTIMIZER_ENABLED
#define POI_LIGHT_DATA_ADDITIVE_DIRECTIONAL_ENABLE
#define POI_LIGHT_DATA_ADDITIVE_ENABLE
#define POI_VERTEXLIGHT_ON
#define VIGNETTE_CLASSIC
#define VIGNETTE_MASKED
#define _LIGHTINGMODE_MULTILAYER_MATH
#define _RIMSTYLE_POIYOMI
#define _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
#define _TPS_VERTEXCOLORS_ON
#define PROPSHADER_MASTER_LABEL 0
#define PROPSHADER_IS_USING_THRY_EDITOR 69
#define PROPFOOTER_YOUTUBE 0
#define PROPFOOTER_TWITTER 0
#define PROPFOOTER_PATREON 0
#define PROPFOOTER_DISCORD 0
#define PROPFOOTER_GITHUB 0
#define PROP_FORGOTTOLOCKMATERIAL 1
#define PROP_SHADEROPTIMIZERENABLED 0
#define PROP_LOCKTOOLTIP 0
#define PROP_MODE 0
#define PROPM_MAINCATEGORY 0
#define PROP_COLORTHEMEINDEX 0
#define PROP_MAINTEX
#define PROP_MAINTEXUV 0
#define PROP_BUMPMAP
#define PROP_BUMPMAPUV 0
#define PROP_BUMPSCALE 1
#define PROP_CLIPPINGMASKUV 0
#define PROP_INVERSE_CLIPPING 0
#define PROP_CUTOFF 0.5
#define PROPM_START_MAINHUESHIFT 0
#define PROP_MAINCOLORADJUSTTOGGLE 0
#define PROP_MAINCOLORADJUSTTEXTUREUV 0
#define PROP_SATURATION 0
#define PROP_MAINBRIGHTNESS 0
#define PROP_MAINHUESHIFTTOGGLE 0
#define PROP_MAINHUESHIFTREPLACE 1
#define PROP_MAINHUESHIFT 0
#define PROP_MAINHUESHIFTSPEED 0
#define PROP_MAINHUEALCTENABLED 0
#define PROP_MAINALHUESHIFTBAND 0
#define PROP_MAINALHUESHIFTCTINDEX 0
#define PROP_MAINHUEALMOTIONSPEED 1
#define PROPM_END_MAINHUESHIFT 0
#define PROPM_START_ALPHA 0
#define PROP_ALPHAFORCEOPAQUE 0
#define PROP_ALPHAMOD 0
#define PROP_ALPHAPREMULTIPLY 0
#define PROP_ALPHATOCOVERAGE 0
#define PROP_ALPHASHARPENEDA2C 0
#define PROP_ALPHAMIPSCALE 0.25
#define PROP_ALPHADITHERING 0
#define PROP_ALPHADITHERGRADIENT 0.1
#define PROP_ALPHADISTANCEFADE 0
#define PROP_ALPHADISTANCEFADETYPE 1
#define PROP_ALPHADISTANCEFADEMINALPHA 0
#define PROP_ALPHADISTANCEFADEMAXALPHA 1
#define PROP_ALPHADISTANCEFADEMIN 0
#define PROP_ALPHADISTANCEFADEMAX 0
#define PROP_ALPHAFRESNEL 0
#define PROP_ALPHAFRESNELALPHA 0
#define PROP_ALPHAFRESNELSHARPNESS 0.5
#define PROP_ALPHAFRESNELWIDTH 0.5
#define PROP_ALPHAFRESNELINVERT 0
#define PROP_ALPHAANGULAR 0
#define PROP_ANGLETYPE 0
#define PROP_ANGLECOMPARETO 0
#define PROP_CAMERAANGLEMIN 45
#define PROP_CAMERAANGLEMAX 90
#define PROP_MODELANGLEMIN 45
#define PROP_MODELANGLEMAX 90
#define PROP_ANGLEMINALPHA 0
#define PROP_ALPHAAUDIOLINKENABLED 0
#define PROP_ALPHAAUDIOLINKADDBAND 0
#define PROPM_END_ALPHA 0
#define PROPM_START_DETAILOPTIONS 0
#define PROP_DETAILENABLED 0
#define PROP_DETAILMASKUV 0
#define PROP_DETAILTINTTHEMEINDEX 0
#define PROP_DETAILTEXUV 0
#define PROP_DETAILTEXINTENSITY 1
#define PROP_DETAILBRIGHTNESS 1
#define PROP_DETAILNORMALMAPSCALE 1
#define PROP_DETAILNORMALMAPUV 0
#define PROPM_END_DETAILOPTIONS 0
#define PROPM_START_VERTEXMANIPULATION 0
#define PROP_VERTEXMANIPULATIONSENABLED 0
#define PROP_VERTEXMANIPULATIONHEIGHT 0
#define PROP_VERTEXMANIPULATIONHEIGHTMASKUV 0
#define PROP_VERTEXMANIPULATIONHEIGHTBIAS 0
#define PROP_VERTEXROUNDINGENABLED 0
#define PROP_VERTEXROUNDINGDIVISION 500
#define PROP_VERTEXAUDIOLINKENABLED 0
#define PROP_VERTEXLOCALTRANSLATIONALBAND 0
#define PROP_VERTEXLOCALROTATIONALBANDX 0
#define PROP_VERTEXLOCALROTATIONALBANDY 0
#define PROP_VERTEXLOCALROTATIONALBANDZ 0
#define PROP_VERTEXLOCALROTATIONCTALBANDX 0
#define PROP_VERTEXLOCALROTATIONCTALTYPEX 0
#define PROP_VERTEXLOCALROTATIONCTALBANDY 0
#define PROP_VERTEXLOCALROTATIONCTALTYPEY 0
#define PROP_VERTEXLOCALROTATIONCTALBANDZ 0
#define PROP_VERTEXLOCALROTATIONCTALTYPEZ 0
#define PROP_VERTEXLOCALSCALEALBAND 0
#define PROP_VERTEXWORLDTRANSLATIONALBAND 0
#define PROP_VERTEXMANIPULATIONHEIGHTBAND 0
#define PROP_VERTEXROUNDINGRANGEBAND 0
#define PROPM_END_VERTEXMANIPULATION 0
#define PROPM_START_MAINVERTEXCOLORS 0
#define PROP_MAINVERTEXCOLORINGLINEARSPACE 1
#define PROP_MAINVERTEXCOLORING 0
#define PROP_MAINUSEVERTEXCOLORALPHA 0
#define PROPM_END_MAINVERTEXCOLORS 0
#define PROPM_START_BACKFACE 0
#define PROP_BACKFACEENABLED 0
#define PROP_BACKFACECOLORTHEMEINDEX 0
#define PROP_BACKFACEEMISSIONSTRENGTH 0
#define PROP_BACKFACEALPHA 1
#define PROP_BACKFACETEXTUREUV 0
#define PROP_BACKFACEMASKUV 0
#define PROP_BACKFACEDETAILINTENSITY 1
#define PROP_BACKFACEREPLACEALPHA 0
#define PROP_BACKFACEEMISSIONLIMITER 1
#define PROP_BACKFACEHUESHIFTENABLED 0
#define PROP_BACKFACEHUESHIFT 0
#define PROP_BACKFACEHUESHIFTSPEED 0
#define PROPM_END_BACKFACE 0
#define PROPM_START_RGBMASK 0
#define PROP_RGBMASKENABLED 0
#define PROP_RGBUSEVERTEXCOLORS 0
#define PROP_RGBBLENDMULTIPLICATIVE 0
#define PROP_RGBMASKUV 0
#define PROP_REDCOLORTHEMEINDEX 0
#define PROP_REDTEXTUREUV 0
#define PROP_GREENCOLORTHEMEINDEX 0
#define PROP_GREENTEXTUREUV 0
#define PROP_BLUECOLORTHEMEINDEX 0
#define PROP_BLUETEXTUREUV 0
#define PROP_ALPHACOLORTHEMEINDEX 0
#define PROP_ALPHATEXTUREUV 0
#define PROP_RGBNORMALSENABLED 0
#define PROP_RGBNORMALBLEND 0
#define PROP_RGBNORMALRUV 0
#define PROP_RGBNORMALRSCALE 0
#define PROP_RGBNORMALGUV 0
#define PROP_RGBNORMALGSCALE 0
#define PROP_RGBNORMALBUV 0
#define PROP_RGBNORMALBSCALE 0
#define PROP_RGBNORMALAUV 0
#define PROP_RGBNORMALASCALE 0
#define PROPM_END_RGBMASK 0
#define PROPM_START_DECALSECTION 0
#define PROP_DECALMASKUV 0
#define PROP_DECALTPSDEPTHMASKENABLED 0
#define PROP_DECAL0TPSMASKSTRENGTH 1
#define PROP_DECAL1TPSMASKSTRENGTH 1
#define PROP_DECAL2TPSMASKSTRENGTH 1
#define PROP_DECAL3TPSMASKSTRENGTH 1
#define PROPM_START_DECAL0 0
#define PROP_DECALENABLED 0
#define PROP_DECAL0MASKCHANNEL 0
#define PROP_DECALCOLORTHEMEINDEX 0
#define PROP_DECALEMISSIONSTRENGTH 0
#define PROP_DECALTEXTUREUV 0
#define PROP_DECALTILED 0
#define PROP_DECAL0DEPTH 0
#define PROP_DECALROTATION 0
#define PROP_DECALROTATIONSPEED 0
#define PROP_DECALBLENDTYPE 0
#define PROP_DECALBLENDALPHA 1
#define PROP_DECALOVERRIDEALPHA 0
#define PROP_DECALHUESHIFTENABLED 0
#define PROP_DECALHUESHIFTSPEED 0
#define PROP_DECALHUESHIFT 0
#define PROP_DECAL0HUEANGLESTRENGTH 0
#define PROPM_START_DECAL0AUDIOLINK 0
#define PROP_AUDIOLINKDECAL0SCALEBAND 0
#define PROP_AUDIOLINKDECAL0SIDEBAND 0
#define PROP_AUDIOLINKDECAL0ROTATIONBAND 0
#define PROP_AUDIOLINKDECAL0ALPHABAND 0
#define PROP_AUDIOLINKDECAL0EMISSIONBAND 0
#define PROP_AUDIOLINKDECALCC0 0
#define PROP_DECALROTATIONCTALBAND0 0
#define PROP_DECALROTATIONCTALTYPE0 0
#define PROP_DECALROTATIONCTALSPEED0 0
#define PROPM_END_DECAL0AUDIOLINK 0
#define PROPM_END_DECAL0 0
#define PROPM_START_DECAL1 0
#define PROP_DECALENABLED1 0
#define PROP_DECAL1MASKCHANNEL 1
#define PROP_DECALCOLOR1THEMEINDEX 0
#define PROP_DECALEMISSIONSTRENGTH1 0
#define PROP_DECALTEXTURE1UV 0
#define PROP_DECALTILED1 0
#define PROP_DECAL1DEPTH 0
#define PROP_DECALROTATION1 0
#define PROP_DECALROTATIONSPEED1 0
#define PROP_DECALBLENDTYPE1 0
#define PROP_DECALBLENDALPHA1 1
#define PROP_DECALOVERRIDEALPHA1 0
#define PROP_DECALHUESHIFTENABLED1 0
#define PROP_DECALHUESHIFTSPEED1 0
#define PROP_DECALHUESHIFT1 0
#define PROP_DECAL1HUEANGLESTRENGTH 0
#define PROPM_START_DECAL1AUDIOLINK 0
#define PROP_AUDIOLINKDECAL1SCALEBAND 0
#define PROP_AUDIOLINKDECAL1SIDEBAND 0
#define PROP_AUDIOLINKDECAL1ROTATIONBAND 0
#define PROP_AUDIOLINKDECAL1ALPHABAND 0
#define PROP_AUDIOLINKDECAL1EMISSIONBAND 0
#define PROP_AUDIOLINKDECALCC1 0
#define PROP_DECALROTATIONCTALBAND1 0
#define PROP_DECALROTATIONCTALTYPE1 0
#define PROP_DECALROTATIONCTALSPEED1 0
#define PROPM_END_DECAL1AUDIOLINK 0
#define PROPM_END_DECAL1 0
#define PROPM_START_DECAL2 0
#define PROP_DECALENABLED2 0
#define PROP_DECAL2MASKCHANNEL 2
#define PROP_DECALCOLOR2THEMEINDEX 0
#define PROP_DECALEMISSIONSTRENGTH2 0
#define PROP_DECALTEXTURE2UV 0
#define PROP_DECALTILED2 0
#define PROP_DECAL2DEPTH 0
#define PROP_DECALROTATION2 0
#define PROP_DECALROTATIONSPEED2 0
#define PROP_DECALBLENDTYPE2 0
#define PROP_DECALBLENDALPHA2 1
#define PROP_DECALOVERRIDEALPHA2 0
#define PROP_DECALHUESHIFTENABLED2 0
#define PROP_DECALHUESHIFTSPEED2 0
#define PROP_DECALHUESHIFT2 0
#define PROP_DECAL2HUEANGLESTRENGTH 0
#define PROPM_START_DECAL2AUDIOLINK 0
#define PROP_AUDIOLINKDECAL2SCALEBAND 0
#define PROP_AUDIOLINKDECAL2SIDEBAND 0
#define PROP_AUDIOLINKDECAL2ROTATIONBAND 0
#define PROP_AUDIOLINKDECAL2ALPHABAND 0
#define PROP_AUDIOLINKDECAL2EMISSIONBAND 0
#define PROP_AUDIOLINKDECALCC2 0
#define PROP_DECALROTATIONCTALBAND2 0
#define PROP_DECALROTATIONCTALTYPE2 0
#define PROP_DECALROTATIONCTALSPEED2 0
#define PROPM_END_DECAL2AUDIOLINK 0
#define PROPM_END_DECAL2 0
#define PROPM_START_DECAL3 0
#define PROP_DECALENABLED3 0
#define PROP_DECAL3MASKCHANNEL 3
#define PROP_DECALCOLOR3THEMEINDEX 0
#define PROP_DECALEMISSIONSTRENGTH3 0
#define PROP_DECALTEXTURE3UV 0
#define PROP_DECALTILED3 0
#define PROP_DECAL3DEPTH 0
#define PROP_DECALROTATION3 0
#define PROP_DECALROTATIONSPEED3 0
#define PROP_DECALBLENDTYPE3 0
#define PROP_DECALBLENDALPHA3 1
#define PROP_DECALOVERRIDEALPHA3 0
#define PROP_DECALHUESHIFTENABLED3 0
#define PROP_DECALHUESHIFTSPEED3 0
#define PROP_DECALHUESHIFT3 0
#define PROP_DECAL3HUEANGLESTRENGTH 0
#define PROPM_START_DECAL3AUDIOLINK 0
#define PROP_AUDIOLINKDECAL3SCALEBAND 0
#define PROP_AUDIOLINKDECAL3SIDEBAND 0
#define PROP_AUDIOLINKDECAL3ROTATIONBAND 0
#define PROP_AUDIOLINKDECAL3ALPHABAND 0
#define PROP_AUDIOLINKDECAL3EMISSIONBAND 0
#define PROP_AUDIOLINKDECALCC3 0
#define PROP_DECALROTATIONCTALBAND3 0
#define PROP_DECALROTATIONCTALTYPE3 0
#define PROP_DECALROTATIONCTALSPEED3 0
#define PROPM_END_DECAL3AUDIOLINK 0
#define PROPM_END_DECAL3 0
#define PROPM_END_DECALSECTION 0
#define PROPM_START_TPS_PENETRATOR 0
#define PROPM_START_PEN_AUTOCONFIG 0
#define PROP_TPS_PENETRATORLENGTH 1
#define PROPM_END_PEN_AUTOCONFIG 0
#define PROP_TPSHELPBOX 0
#define PROP_TPSPENETRATORENABLED 0
#define PROP_TPSBEZIERHEADER 0
#define PROP_TPS_BEZIERSTART 0
#define PROP_TPS_BEZIERSMOOTHNESS 0.09
#define PROP_TPSSQUEEZEHEADER 0
#define PROP_TPS_SQUEEZE 0.3
#define PROP_TPS_SQUEEZEDISTANCE 0.2
#define PROP_TPSBULDGEHEADER 0
#define PROP_TPS_BULDGE 0.3
#define PROP_TPS_BULDGEDISTANCE 0.2
#define PROP_TPS_BULDGEFALLOFFDISTANCE 0.05
#define PROP_TPSPULSINGHEADER 0
#define PROP_TPS_PUMPINGSTRENGTH 0
#define PROP_TPS_PUMPINGSPEED 0
#define PROP_TPS_PUMPINGWIDTH 0.2
#define PROP_TPSIDLEHEADER 0
#define PROP_TPS_IDLEGRAVITY 0
#define PROP_TPS_IDLESKRINKWIDTH 1
#define PROP_TPS_IDLESKRINKLENGTH 1
#define PROP_TPS_IDLEMOVEMENTSTRENGTH 0
#define PROP_TPS_VERTEXCOLORS 1
#define PROP_TPS2_BUFFEREDDEPTH 0
#define PROP_TPS2_BUFFEREDSTRENGTH 0
#define PROPM_END_TPS_PENETRATOR 0
#define PROPM_START_GLOBALTHEMES 0
#define PROPM_END_GLOBALTHEMES 0
#define PROPM_LIGHTINGCATEGORY 0
#define PROPM_START_POILIGHTDATA 0
#define PROP_LIGHTINGAOMAPSUV 0
#define PROP_LIGHTDATAAOSTRENGTHR 1
#define PROP_LIGHTDATAAOSTRENGTHG 0
#define PROP_LIGHTDATAAOSTRENGTHB 0
#define PROP_LIGHTDATAAOSTRENGTHA 0
#define PROP_LIGHTINGDETAILSHADOWMAPSUV 0
#define PROP_LIGHTINGDETAILSHADOWSTRENGTHR 1
#define PROP_LIGHTINGDETAILSHADOWSTRENGTHG 0
#define PROP_LIGHTINGDETAILSHADOWSTRENGTHB 0
#define PROP_LIGHTINGDETAILSHADOWSTRENGTHA 0
#define PROP_LIGHTINGSHADOWMASKSUV 0
#define PROP_LIGHTINGSHADOWMASKSTRENGTHR 1
#define PROP_LIGHTINGSHADOWMASKSTRENGTHG 0
#define PROP_LIGHTINGSHADOWMASKSTRENGTHB 0
#define PROP_LIGHTINGSHADOWMASKSTRENGTHA 0
#define PROP_LIGHTINGCOLORMODE 0
#define PROP_LIGHTINGMAPMODE 0
#define PROP_LIGHTINGDIRECTIONMODE 0
#define PROP_LIGHTINGFORCECOLORENABLED 0
#define PROP_LIGHTINGFORCEDCOLORTHEMEINDEX 0
#define PROP_UNLIT_INTENSITY 1
#define PROP_LIGHTINGCAPENABLED 1
#define PROP_LIGHTINGCAP 1
#define PROP_LIGHTINGMINLIGHTBRIGHTNESS 0
#define PROP_LIGHTINGINDIRECTUSESNORMALS 0
#define PROP_LIGHTINGCASTEDSHADOWS 0
#define PROP_LIGHTINGMONOCHROMATIC 0
#define PROP_LIGHTINGADDITIVEENABLE 1
#define PROP_DISABLEDIRECTIONALINADD 1
#define PROP_LIGHTINGADDITIVELIMITED 0
#define PROP_LIGHTINGADDITIVELIMIT 1
#define PROP_LIGHTINGADDITIVEMONOCHROMATIC 0
#define PROP_LIGHTINGADDITIVEPASSTHROUGH 0.5
#define PROP_LIGHTINGVERTEXLIGHTINGENABLED 1
#define PROP_LIGHTDATADEBUGENABLED 0
#define PROP_LIGHTINGDEBUGVISUALIZE 0
#define PROPM_END_POILIGHTDATA 0
#define PROPM_START_POISHADING 0
#define PROP_SHADINGENABLED 1
#define PROP_LIGHTINGMODE 1
#define PROP_TOONRAMP
#define PROP_SHADOWOFFSET 0
#define PROP_LIGHTINGWRAPPEDWRAP 0
#define PROP_LIGHTINGWRAPPEDNORMALIZATION 0
#define PROP_SHADOWCOLORTEXUV 0
#define PROP_SHADOWBORDER 0.5
#define PROP_SHADOWBLUR 0.1
#define PROP_SHADOW2NDCOLORTEXUV 0
#define PROP_SHADOW2NDBORDER 0.5
#define PROP_SHADOW2NDBLUR 0.3
#define PROP_SHADOW3RDCOLORTEXUV 0
#define PROP_SHADOW3RDBORDER 0.25
#define PROP_SHADOW3RDBLUR 0.1
#define PROP_SHADOWBORDERRANGE 0
#define PROP_LIGHTINGGRADIENTSTART 0
#define PROP_LIGHTINGGRADIENTEND 0.5
#define PROP_1ST_SHADEMAPUV 0
#define PROP_USE_1STSHADEMAPALPHA_AS_SHADOWMASK 0
#define PROP_1STSHADEMAPMASK_INVERSE 0
#define PROP_USE_BASEAS1ST 0
#define PROP_2ND_SHADEMAPUV 0
#define PROP_USE_2NDSHADEMAPALPHA_AS_SHADOWMASK 0
#define PROP_2NDSHADEMAPMASK_INVERSE 0
#define PROP_USE_1STAS2ND 0
#define PROP_BASECOLOR_STEP 0.5
#define PROP_BASESHADE_FEATHER 0.0001
#define PROP_SHADECOLOR_STEP 0
#define PROP_1ST2ND_SHADES_FEATHER 0.0001
#define PROP_SHADINGSHADEMAPBLENDTYPE 0
#define PROP_SKINLUT
#define PROP_SSSSCALE 1
#define PROP_SSSBUMPBLUR 0.7
#define PROP_CLOTHDFG
#define PROP_CLOTHMETALLICSMOOTHNESSMAPINVERT 0
#define PROP_CLOTHMETALLICSMOOTHNESSMAPUV 0
#define PROP_CLOTHREFLECTANCE 0.5
#define PROP_CLOTHSMOOTHNESS 0.5
#define PROP_SHADOWSTRENGTH 1
#define PROP_LIGHTINGIGNOREAMBIENTCOLOR 0
#define PROP_LIGHTINGADDITIVETYPE 1
#define PROP_LIGHTINGADDITIVEGRADIENTSTART 0
#define PROP_LIGHTINGADDITIVEGRADIENTEND 0.5
#define PROPM_END_POISHADING 0
#define PROPM_START_ANISO 0
#define PROP_ENABLEANISO 0
#define PROP_ANISOCOLORMAPUV 0
#define PROP_ANISOUSELIGHTCOLOR 1
#define PROP_ANISOUSEBASECOLOR 0
#define PROP_ANISOREPLACE 0
#define PROP_ANISOADD 1
#define PROP_ANISOHIDEINSHADOW 1
#define PROP_ANISO0POWER 0
#define PROP_ANISO0STRENGTH 1
#define PROP_ANISO0OFFSET 0
#define PROP_ANISO0OFFSETMAPSTRENGTH 0
#define PROP_ANISO0TINTINDEX 0
#define PROP_ANISO0TOONMODE 0
#define PROP_ANISO0EDGE 0.5
#define PROP_ANISO0BLUR 0
#define PROP_ANISO1POWER 0.1
#define PROP_ANISO1STRENGTH 1
#define PROP_ANISO1OFFSET 0
#define PROP_ANISO1OFFSETMAPSTRENGTH 0
#define PROP_ANISO1TINTINDEX 0
#define PROP_ANISO1TOONMODE 0
#define PROP_ANISO1EDGE 0.5
#define PROP_ANISO1BLUR 0
#define PROP_ANISODEBUGTOGGLE 0
#define PROP_ANISODEBUGMODE 0
#define PROPM_END_ANSIO 0
#define PROPM_START_MATCAP 0
#define PROP_MATCAPENABLE 0
#define PROP_MATCAPUVMODE 1
#define PROP_MATCAPCOLORTHEMEINDEX 0
#define PROP_MATCAPBORDER 0.43
#define PROP_MATCAPMASKUV 0
#define PROP_MATCAPMASKINVERT 0
#define PROP_MATCAPEMISSIONSTRENGTH 0
#define PROP_MATCAPINTENSITY 1
#define PROP_MATCAPLIGHTMASK 0
#define PROP_MATCAPREPLACE 1
#define PROP_MATCAPMULTIPLY 0
#define PROP_MATCAPADD 0
#define PROP_MATCAPMIXED 0
#define PROP_MATCAPADDTOLIGHT 0
#define PROP_MATCAPALPHAOVERRIDE 0
#define PROP_MATCAPNORMAL 1
#define PROP_MATCAP0CUSTOMNORMAL 0
#define PROP_MATCAP0NORMALMAPUV 0
#define PROP_MATCAP0NORMALMAPSCALE 1
#define PROP_MATCAPHUESHIFTENABLED 0
#define PROP_MATCAPHUESHIFTSPEED 0
#define PROP_MATCAPHUESHIFT 0
#define PROP_MATCAPTPSDEPTHENABLED 0
#define PROP_MATCAPTPSMASKSTRENGTH 1
#define PROPM_END_MATCAP 0
#define PROPM_START_MATCAP2 0
#define PROP_MATCAP2ENABLE 0
#define PROP_MATCAP2UVMODE 1
#define PROP_MATCAP2COLORTHEMEINDEX 0
#define PROP_MATCAP2BORDER 0.43
#define PROP_MATCAP2MASKUV 0
#define PROP_MATCAP2MASKINVERT 0
#define PROP_MATCAP2EMISSIONSTRENGTH 0
#define PROP_MATCAP2INTENSITY 1
#define PROP_MATCAP2LIGHTMASK 0
#define PROP_MATCAP2REPLACE 0
#define PROP_MATCAP2MULTIPLY 0
#define PROP_MATCAP2ADD 0
#define PROP_MATCAP2MIXED 0
#define PROP_MATCAP2ADDTOLIGHT 0
#define PROP_MATCAP2ALPHAOVERRIDE 0
#define PROP_MATCAP2NORMAL 1
#define PROP_MATCAP1CUSTOMNORMAL 0
#define PROP_MATCAP1CUSTOMNORMAL 0
#define PROP_MATCAP1NORMALMAPUV 0
#define PROP_MATCAP1NORMALMAPSCALE 1
#define PROP_MATCAP2HUESHIFTENABLED 0
#define PROP_MATCAP2HUESHIFTSPEED 0
#define PROP_MATCAP2HUESHIFT 0
#define PROP_MATCAP2TPSDEPTHENABLED 0
#define PROP_MATCAP2TPSMASKSTRENGTH 1
#define PROPM_END_MATCAP2 0
#define PROPM_START_CUBEMAP 0
#define PROP_CUBEMAPENABLED 0
#define PROP_CUBEMAPUVMODE 1
#define PROP_CUBEMAPCOLORTHEMEINDEX 0
#define PROP_CUBEMAP
#define PROP_CUBEMAPMASKUV 0
#define PROP_CUBEMAPMASKINVERT 0
#define PROP_CUBEMAPEMISSIONSTRENGTH 0
#define PROP_CUBEMAPINTENSITY 1
#define PROP_CUBEMAPLIGHTMASK 0
#define PROP_CUBEMAPREPLACE 1
#define PROP_CUBEMAPMULTIPLY 0
#define PROP_CUBEMAPADD 0
#define PROP_CUBEMAPNORMAL 1
#define PROP_CUBEMAPHUESHIFTENABLED 0
#define PROP_CUBEMAPHUESHIFTSPEED 0
#define PROP_CUBEMAPHUESHIFT 0
#define PROPM_END_CUBEMAP 0
#define PROPM_START_RIMLIGHTOPTIONS 0
#define PROP_ENABLERIMLIGHTING 0
#define PROP_RIMSTYLE 0
#define PROP_RIMTEXUV 0
#define PROP_RIMMASKUV 0
#define PROP_IS_NORMALMAPTORIMLIGHT 1
#define PROP_RIMLIGHTINGINVERT 0
#define PROP_RIMLIGHTCOLORTHEMEINDEX 0
#define PROP_RIMWIDTH 0.8
#define PROP_RIMSHARPNESS 0.25
#define PROP_RIMPOWER 1
#define PROP_RIMSTRENGTH 0
#define PROP_RIMBASECOLORMIX 0
#define PROP_RIMBLENDMODE 0
#define PROP_RIMBLENDSTRENGTH 1
#define PROP_IS_LIGHTCOLOR_RIMLIGHT 1
#define PROP_RIMLIGHT_POWER 0.1
#define PROP_RIMLIGHT_INSIDEMASK 0.0001
#define PROP_RIMLIGHT_FEATHEROFF 0
#define PROP_LIGHTDIRECTION_MASKON 0
#define PROP_TWEAK_LIGHTDIRECTION_MASKLEVEL 0
#define PROP_ADD_ANTIPODEAN_RIMLIGHT 0
#define PROP_IS_LIGHTCOLOR_AP_RIMLIGHT 1
#define PROP_RIMAPCOLORTHEMEINDEX 0
#define PROP_AP_RIMLIGHT_POWER 0.1
#define PROP_AP_RIMLIGHT_FEATHEROFF 0
#define PROP_TWEAK_RIMLIGHTMASKLEVEL 0
#define PROP_RIMSHADOWTOGGLE 0
#define PROP_RIMSHADOWMASKRAMPTYPE 0
#define PROP_RIMSHADOWMASKSTRENGTH 1
#define PROP_RIMSHADOWWIDTH 0
#define PROP_RIMHUESHIFTENABLED 0
#define PROP_RIMHUESHIFTSPEED 0
#define PROP_RIMHUESHIFT 0
#define PROPM_START_RIMAUDIOLINK 0
#define PROP_AUDIOLINKRIMWIDTHBAND 0
#define PROP_AUDIOLINKRIMEMISSIONBAND 0
#define PROP_AUDIOLINKRIMBRIGHTNESSBAND 0
#define PROPM_END_RIMAUDIOLINK 0
#define PROPM_END_RIMLIGHTOPTIONS 0
#define PROPM_START_DEPTHRIMLIGHTOPTIONS 0
#define PROP_ENABLEDEPTHRIMLIGHTING 0
#define PROP_DEPTHRIMNORMALTOUSE 1
#define PROP_DEPTHRIMWIDTH 0.2
#define PROP_DEPTHRIMSHARPNESS 0.2
#define PROP_DEPTHRIMHIDEINSHADOW 0
#define PROP_DEPTHRIMMIXBASECOLOR 0
#define PROP_DEPTHRIMMIXLIGHTCOLOR 0
#define PROP_DEPTHRIMCOLORTHEMEINDEX 0
#define PROP_DEPTHRIMEMISSION 0
#define PROP_DEPTHRIMREPLACE 0
#define PROP_DEPTHRIMADD 0
#define PROP_DEPTHRIMMULTIPLY 0
#define PROP_DEPTHRIMADDITIVELIGHTING 0
#define PROPM_END_DEPTHRIMLIGHTOPTIONS 0
#define PROPM_START_BRDF 1
#define PROP_MOCHIEBRDF 0
#define PROP_MOCHIEREFLECTIONSTRENGTH 1
#define PROP_MOCHIESPECULARSTRENGTH 1
#define PROP_MOCHIEMETALLICMULTIPLIER 0
#define PROP_MOCHIEROUGHNESSMULTIPLIER 1
#define PROP_MOCHIEREFLECTIONTINTTHEMEINDEX 0
#define PROP_MOCHIESPECULARTINTTHEMEINDEX 0
#define PROP_MOCHIEMETALLICMAPSUV 0
#define PROP_MOCHIEMETALLICMAPINVERT 0
#define PROP_MOCHIEROUGHNESSMAPINVERT 0
#define PROP_MOCHIEREFLECTIONMASKINVERT 0
#define PROP_MOCHIESPECULARMASKINVERT 0
#define PROP_PBRSPLITMASKSAMPLE 0
#define PROP_MOCHIEMETALLICMASKSUV 0
#define PROP_SPECULAR2NDLAYER 0
#define PROP_MOCHIESPECULARSTRENGTH2 1
#define PROP_MOCHIEROUGHNESSMULTIPLIER2 1
#define PROP_BRDFTPSDEPTHENABLED 0
#define PROP_BRDFTPSREFLECTIONMASKSTRENGTH 1
#define PROP_BRDFTPSSPECULARMASKSTRENGTH 1
#define PROP_IGNORECASTEDSHADOWS 0
#define PROP_MOCHIEFORCEFALLBACK 0
#define PROP_MOCHIELITFALLBACK 0
#define PROP_MOCHIEGSAAENABLED 1
#define PROP_POIGSAAVARIANCE 0.15
#define PROP_POIGSAATHRESHOLD 0.1
#define PROP_REFSPECFRESNEL 1
#define PROPM_END_BRDF 0
#define PROPM_START_CLEARCOAT 0
#define PROP_CLEARCOATBRDF 0
#define PROP_CLEARCOATSTRENGTH 1
#define PROP_CLEARCOATSMOOTHNESS 1
#define PROP_CLEARCOATREFLECTIONSTRENGTH 1
#define PROP_CLEARCOATSPECULARSTRENGTH 1
#define PROP_CLEARCOATREFLECTIONTINTTHEMEINDEX 0
#define PROP_CLEARCOATSPECULARTINTTHEMEINDEX 0
#define PROP_CLEARCOATMAPSUV 0
#define PROP_CLEARCOATMASKINVERT 0
#define PROP_CLEARCOATSMOOTHNESSMAPINVERT 0
#define PROP_CLEARCOATREFLECTIONMASKINVERT 0
#define PROP_CLEARCOATSPECULARMASKINVERT 0
#define PROP_CLEARCOATFORCEFALLBACK 0
#define PROP_CLEARCOATLITFALLBACK 0
#define PROP_CCIGNORECASTEDSHADOWS 0
#define PROP_CLEARCOATGSAAENABLED 1
#define PROP_CLEARCOATGSAAVARIANCE 0.15
#define PROP_CLEARCOATGSAATHRESHOLD 0.1
#define PROP_CLEARCOATTPSDEPTHMASKENABLED 0
#define PROP_CLEARCOATTPSMASKSTRENGTH 1
#define PROPM_END_CLEARCOAT 0
#define PROPM_START_REFLECTIONRIM 0
#define PROP_ENABLEENVIRONMENTALRIM 0
#define PROP_RIMENVIROMASKUV 0
#define PROP_RIMENVIROBLUR 0.7
#define PROP_RIMENVIROWIDTH 0.45
#define PROP_RIMENVIROSHARPNESS 0
#define PROP_RIMENVIROMINBRIGHTNESS 0
#define PROP_RIMENVIROINTENSITY 1
#define PROPM_END_REFLECTIONRIM 0
#define PROPM_START_STYLIZEDSPEC 0
#define PROP_STYLIZEDSPECULAR 0
#define PROP_HIGHCOLOR_TEXUV 0
#define PROP_HIGHCOLORTHEMEINDEX 0
#define PROP_SET_HIGHCOLORMASKUV 0
#define PROP_TWEAK_HIGHCOLORMASKLEVEL 0
#define PROP_IS_SPECULARTOHIGHCOLOR 0
#define PROP_IS_BLENDADDTOHICOLOR 0
#define PROP_STYLIZEDSPECULARSTRENGTH 1
#define PROP_USELIGHTCOLOR 1
#define PROP_SSIGNORECASTEDSHADOWS 0
#define PROP_HIGHCOLOR_POWER 0.2
#define PROP_STYLIZEDSPECULARFEATHER 0
#define PROP_LAYER1STRENGTH 1
#define PROP_LAYER2SIZE 0
#define PROP_STYLIZEDSPECULAR2FEATHER 0
#define PROP_LAYER2STRENGTH 0
#define PROPM_END_STYLIZEDSPEC 0
#define PROPM_SPECIALFXCATEGORY 0
#define PROPM_START_UDIMDISCARDOPTIONS 0
#define PROP_ENABLEUDIMDISCARDOPTIONS 0
#define PROP_UDIMDISCARDUV 0
#define PROP_UDIMDISCARDMODE 1
#define PROPM_END_UDIMDISCARDOPTIONS 0
#define PROPM_START_DISSOLVE 0
#define PROP_ENABLEDISSOLVE 0
#define PROP_DISSOLVETYPE 1
#define PROP_DISSOLVEEDGEWIDTH 0.025
#define PROP_DISSOLVEEDGEHARDNESS 0.5
#define PROP_DISSOLVEEDGECOLORTHEMEINDEX 0
#define PROP_DISSOLVEEDGEEMISSION 0
#define PROP_DISSOLVETEXTURECOLORTHEMEINDEX 0
#define PROP_DISSOLVETOTEXTUREUV 0
#define PROP_DISSOLVETOEMISSIONSTRENGTH 0
#define PROP_DISSOLVENOISETEXTUREUV 0
#define PROP_DISSOLVEINVERTNOISE 0
#define PROP_DISSOLVEDETAILNOISEUV 0
#define PROP_DISSOLVEINVERTDETAILNOISE 0
#define PROP_DISSOLVEDETAILSTRENGTH 0.1
#define PROP_DISSOLVEALPHA 0
#define PROP_DISSOLVEMASKUV 0
#define PROP_DISSOLVEUSEVERTEXCOLORS 0
#define PROP_DISSOLVEMASKINVERT 0
#define PROP_CONTINUOUSDISSOLVE 0
#define PROP_ENABLEDISSOLVEAUDIOLINK 0
#define PROP_AUDIOLINKDISSOLVEALPHABAND 0
#define PROP_AUDIOLINKDISSOLVEDETAILBAND 0
#define PROPM_START_POINTTOPOINT 0
#define PROP_DISSOLVEP2PWORLDLOCAL 0
#define PROP_DISSOLVEP2PEDGELENGTH 0.1
#define PROPM_END_POINTTOPOINT 0
#define PROPM_START_DISSOLVEHUESHIFT 0
#define PROP_DISSOLVEHUESHIFTENABLED 0
#define PROP_DISSOLVEHUESHIFTSPEED 0
#define PROP_DISSOLVEHUESHIFT 0
#define PROP_DISSOLVEEDGEHUESHIFTENABLED 0
#define PROP_DISSOLVEEDGEHUESHIFTSPEED 0
#define PROP_DISSOLVEEDGEHUESHIFT 0
#define PROPM_END_DISSOLVEHUESHIFT 0
#define PROPM_START_BONUSSLIDERS 0
#define PROP_DISSOLVEALPHA0 0
#define PROP_DISSOLVEALPHA1 0
#define PROP_DISSOLVEALPHA2 0
#define PROP_DISSOLVEALPHA3 0
#define PROP_DISSOLVEALPHA4 0
#define PROP_DISSOLVEALPHA5 0
#define PROP_DISSOLVEALPHA6 0
#define PROP_DISSOLVEALPHA7 0
#define PROP_DISSOLVEALPHA8 0
#define PROP_DISSOLVEALPHA9 0
#define PROPM_END_BONUSSLIDERS 0
#define PROPM_END_DISSOLVE 0
#define PROPM_START_FLIPBOOK 0
#define PROP_ENABLEFLIPBOOK 0
#define PROP_FLIPBOOKALPHACONTROLSFINALALPHA 0
#define PROP_FLIPBOOKINTENSITYCONTROLSALPHA 0
#define PROP_FLIPBOOKCOLORREPLACES 0
#define PROP_FLIPBOOKTEXARRAYUV 0
#define PROP_FLIPBOOKMASKUV 0
#define PROP_FLIPBOOKCOLORTHEMEINDEX 0
#define PROP_FLIPBOOKTOTALFRAMES 1
#define PROP_FLIPBOOKFPS 30
#define PROP_FLIPBOOKTILED 0
#define PROP_FLIPBOOKEMISSIONSTRENGTH 0
#define PROP_FLIPBOOKROTATION 0
#define PROP_FLIPBOOKROTATIONSPEED 0
#define PROP_FLIPBOOKREPLACE 1
#define PROP_FLIPBOOKMULTIPLY 0
#define PROP_FLIPBOOKADD 0
#define PROP_FLIPBOOKMANUALFRAMECONTROL 0
#define PROP_FLIPBOOKCURRENTFRAME -1
#define PROP_FLIPBOOKCROSSFADEENABLED 0
#define PROP_FLIPBOOKHUESHIFTENABLED 0
#define PROP_FLIPBOOKHUESHIFTSPEED 0
#define PROP_FLIPBOOKHUESHIFT 0
#define PROPM_START_FLIPBOOKAUDIOLINK 0
#define PROP_AUDIOLINKFLIPBOOKSCALEBAND 0
#define PROP_AUDIOLINKFLIPBOOKALPHABAND 0
#define PROP_AUDIOLINKFLIPBOOKEMISSIONBAND 0
#define PROP_AUDIOLINKFLIPBOOKFRAMEBAND 0
#define PROP_FLIPBOOKCHRONOTENSITYENABLED 0
#define PROP_FLIPBOOKCHRONOTENSITYBAND 0
#define PROP_FLIPBOOKCHRONOTYPE 0
#define PROP_FLIPBOOKCHRONOTENSITYSPEED 0
#define PROPM_END_FLIPBOOKAUDIOLINK 0
#define PROPM_END_FLIPBOOK 0
#define PROPM_START_EMISSIONS 0
#define PROPM_START_EMISSIONOPTIONS 0
#define PROP_ENABLEEMISSION 0
#define PROP_EMISSIONREPLACE0 0
#define PROP_EMISSIONCOLORTHEMEINDEX 0
#define PROP_EMISSIONMAPUV 0
#define PROP_EMISSIONBASECOLORASMAP 0
#define PROP_EMISSIONMASKUV 0
#define PROP_EMISSIONMASKINVERT 0
#define PROP_EMISSIONSTRENGTH 0
#define PROP_EMISSIONHUESHIFTENABLED 0
#define PROP_EMISSIONHUESHIFT 0
#define PROP_EMISSIONHUESHIFTSPEED 0
#define PROP_EMISSIONCENTEROUTENABLED 0
#define PROP_EMISSIONCENTEROUTSPEED 5
#define PROP_ENABLEGITDEMISSION 0
#define PROP_GITDEWORLDORMESH 0
#define PROP_GITDEMINEMISSIONMULTIPLIER 1
#define PROP_GITDEMAXEMISSIONMULTIPLIER 0
#define PROP_GITDEMINLIGHT 0
#define PROP_GITDEMAXLIGHT 1
#define PROP_EMISSIONBLINKINGENABLED 0
#define PROP_EMISSIVEBLINK_MIN 0
#define PROP_EMISSIVEBLINK_MAX 1
#define PROP_EMISSIVEBLINK_VELOCITY 4
#define PROP_EMISSIONBLINKINGOFFSET 0
#define PROP_SCROLLINGEMISSION 0
#define PROP_EMISSIONSCROLLINGUSECURVE 0
#define PROP_EMISSIONSCROLLINGVERTEXCOLOR 0
#define PROP_EMISSIVESCROLL_WIDTH 10
#define PROP_EMISSIVESCROLL_VELOCITY 10
#define PROP_EMISSIVESCROLL_INTERVAL 20
#define PROP_EMISSIONSCROLLINGOFFSET 0
#define PROP_EMISSIONAL0ENABLED 0
#define PROP_EMISSIONAL0STRENGTHBAND 0
#define PROP_AUDIOLINKEMISSION0CENTEROUTSIZE 0
#define PROP_AUDIOLINKEMISSION0CENTEROUTBAND 0
#define PROPM_END_EMISSIONOPTIONS 0
#define PROPM_START_EMISSION1OPTIONS 0
#define PROP_ENABLEEMISSION1 0
#define PROP_EMISSIONREPLACE1 0
#define PROP_EMISSIONCOLOR1THEMEINDEX 0
#define PROP_EMISSIONMAP1UV 0
#define PROP_EMISSIONBASECOLORASMAP1 0
#define PROP_EMISSIONMASK1UV 0
#define PROP_EMISSIONMASKINVERT1 0
#define PROP_EMISSIONSTRENGTH1 0
#define PROP_EMISSIONHUESHIFTENABLED1 0
#define PROP_EMISSIONHUESHIFT1 0
#define PROP_EMISSIONHUESHIFTSPEED1 0
#define PROP_EMISSIONCENTEROUTENABLED1 0
#define PROP_EMISSIONCENTEROUTSPEED1 5
#define PROP_ENABLEGITDEMISSION1 0
#define PROP_GITDEWORLDORMESH1 0
#define PROP_GITDEMINEMISSIONMULTIPLIER1 1
#define PROP_GITDEMAXEMISSIONMULTIPLIER1 0
#define PROP_GITDEMINLIGHT1 0
#define PROP_GITDEMAXLIGHT1 1
#define PROP_EMISSIONBLINKINGENABLED1 0
#define PROP_EMISSIVEBLINK_MIN1 0
#define PROP_EMISSIVEBLINK_MAX1 1
#define PROP_EMISSIVEBLINK_VELOCITY1 4
#define PROP_EMISSIONBLINKINGOFFSET1 0
#define PROP_SCROLLINGEMISSION1 0
#define PROP_EMISSIONSCROLLINGUSECURVE1 0
#define PROP_EMISSIONSCROLLINGVERTEXCOLOR1 0
#define PROP_EMISSIVESCROLL_WIDTH1 10
#define PROP_EMISSIVESCROLL_VELOCITY1 10
#define PROP_EMISSIVESCROLL_INTERVAL1 20
#define PROP_EMISSIONSCROLLINGOFFSET1 0
#define PROP_EMISSIONAL1ENABLED 0
#define PROP_EMISSIONAL1STRENGTHBAND 0
#define PROP_AUDIOLINKEMISSION1CENTEROUTSIZE 0
#define PROP_AUDIOLINKEMISSION1CENTEROUTBAND 0
#define PROPM_END_EMISSION1OPTIONS 0
#define PROPM_START_EMISSION2OPTIONS 0
#define PROP_ENABLEEMISSION2 0
#define PROP_EMISSIONREPLACE2 0
#define PROP_EMISSIONCOLOR2THEMEINDEX 0
#define PROP_EMISSIONMAP2UV 0
#define PROP_EMISSIONBASECOLORASMAP2 0
#define PROP_EMISSIONMASK2UV 0
#define PROP_EMISSIONMASKINVERT2 0
#define PROP_EMISSIONSTRENGTH2 0
#define PROP_EMISSIONHUESHIFTENABLED2 0
#define PROP_EMISSIONHUESHIFT2 0
#define PROP_EMISSIONHUESHIFTSPEED2 0
#define PROP_EMISSIONCENTEROUTENABLED2 0
#define PROP_EMISSIONCENTEROUTSPEED2 5
#define PROP_ENABLEGITDEMISSION2 0
#define PROP_GITDEWORLDORMESH2 0
#define PROP_GITDEMINEMISSIONMULTIPLIER2 1
#define PROP_GITDEMAXEMISSIONMULTIPLIER2 0
#define PROP_GITDEMINLIGHT2 0
#define PROP_GITDEMAXLIGHT2 1
#define PROP_EMISSIONBLINKINGENABLED2 0
#define PROP_EMISSIVEBLINK_MIN2 0
#define PROP_EMISSIVEBLINK_MAX2 1
#define PROP_EMISSIVEBLINK_VELOCITY2 4
#define PROP_EMISSIONBLINKINGOFFSET2 0
#define PROP_SCROLLINGEMISSION2 0
#define PROP_EMISSIONSCROLLINGUSECURVE2 0
#define PROP_EMISSIONSCROLLINGVERTEXCOLOR2 0
#define PROP_EMISSIVESCROLL_WIDTH2 10
#define PROP_EMISSIVESCROLL_VELOCITY2 10
#define PROP_EMISSIVESCROLL_INTERVAL2 20
#define PROP_EMISSIONSCROLLINGOFFSET2 0
#define PROP_EMISSIONAL2ENABLED 0
#define PROP_EMISSIONAL2STRENGTHBAND 0
#define PROP_AUDIOLINKEMISSION2CENTEROUTSIZE 0
#define PROP_AUDIOLINKEMISSION2CENTEROUTBAND 0
#define PROPM_END_EMISSION2OPTIONS 0
#define PROPM_START_EMISSION3OPTIONS 0
#define PROP_ENABLEEMISSION3 0
#define PROP_EMISSIONREPLACE3 0
#define PROP_EMISSIONCOLOR3THEMEINDEX 0
#define PROP_EMISSIONMAP3UV 0
#define PROP_EMISSIONBASECOLORASMAP3 0
#define PROP_EMISSIONMASK3UV 0
#define PROP_EMISSIONMASKINVERT3 0
#define PROP_EMISSIONSTRENGTH3 0
#define PROP_EMISSIONHUESHIFTENABLED3 0
#define PROP_EMISSIONHUESHIFT3 0
#define PROP_EMISSIONHUESHIFTSPEED3 0
#define PROP_EMISSIONCENTEROUTENABLED3 0
#define PROP_EMISSIONCENTEROUTSPEED3 5
#define PROP_ENABLEGITDEMISSION3 0
#define PROP_GITDEWORLDORMESH3 0
#define PROP_GITDEMINEMISSIONMULTIPLIER3 1
#define PROP_GITDEMAXEMISSIONMULTIPLIER3 0
#define PROP_GITDEMINLIGHT3 0
#define PROP_GITDEMAXLIGHT3 1
#define PROP_EMISSIONBLINKINGENABLED3 0
#define PROP_EMISSIVEBLINK_MIN3 0
#define PROP_EMISSIVEBLINK_MAX3 1
#define PROP_EMISSIVEBLINK_VELOCITY3 4
#define PROP_EMISSIONBLINKINGOFFSET3 0
#define PROP_SCROLLINGEMISSION3 0
#define PROP_EMISSIONSCROLLINGUSECURVE3 0
#define PROP_EMISSIONSCROLLINGVERTEXCOLOR3 0
#define PROP_EMISSIVESCROLL_WIDTH3 10
#define PROP_EMISSIVESCROLL_VELOCITY3 10
#define PROP_EMISSIVESCROLL_INTERVAL3 20
#define PROP_EMISSIONSCROLLINGOFFSET3 0
#define PROP_EMISSIONAL3ENABLED 0
#define PROP_EMISSIONAL3STRENGTHBAND 0
#define PROP_AUDIOLINKEMISSION3CENTEROUTSIZE 0
#define PROP_AUDIOLINKEMISSION3CENTEROUTBAND 0
#define PROPM_END_EMISSION3OPTIONS 0
#define PROPM_END_EMISSIONS 0
#define PROPM_START_GLITTER 0
#define PROP_GLITTERENABLE 0
#define PROP_GLITTERUV 0
#define PROP_GLITTERMODE 0
#define PROP_GLITTERSHAPE 0
#define PROP_GLITTERBLENDTYPE 0
#define PROP_GLITTERCOLORTHEMEINDEX 0
#define PROP_GLITTERUSESURFACECOLOR 0
#define PROP_GLITTERCOLORMAPUV 0
#define PROP_GLITTERMASKUV 0
#define PROP_GLITTERTEXTUREROTATION 0
#define PROP_GLITTERFREQUENCY 300
#define PROP_GLITTERJITTER 1
#define PROP_GLITTERSPEED 10
#define PROP_GLITTERSIZE 0.3
#define PROP_GLITTERCONTRAST 300
#define PROP_GLITTERANGLERANGE 90
#define PROP_GLITTERMINBRIGHTNESS 0
#define PROP_GLITTERBRIGHTNESS 3
#define PROP_GLITTERBIAS 0.8
#define PROP_GLITTERHIDEINSHADOW 0
#define PROP_GLITTERCENTERSIZE 0.08
#define PROP_GLITTERFREQUENCYLINEAREMISSIVE 20
#define PROP_GLITTERJAGGYFIX 0
#define PROP_GLITTERHUESHIFTENABLED 0
#define PROP_GLITTERHUESHIFTSPEED 0
#define PROP_GLITTERHUESHIFT 0
#define PROP_GLITTERRANDOMCOLORS 0
#define PROP_GLITTERRANDOMSIZE 0
#define PROP_GLITTERRANDOMROTATION 0
#define PROPM_END_GLITTER 0
#define PROPM_START_PATHING 0
#define PROP_ENABLEPATHING 0
#define PROP_PATHGRADIENTTYPE 0
#define PROP_PATHINGOVERRIDEALPHA 0
#define PROP_PATHINGMAPUV 0
#define PROP_PATHINGCOLORMAPUV 0
#define PROP_PATHTYPER 0
#define PROP_PATHTYPEG 0
#define PROP_PATHTYPEB 0
#define PROP_PATHTYPEA 0
#define PROP_PATHCOLORRTHEMEINDEX 0
#define PROP_PATHCOLORGTHEMEINDEX 0
#define PROP_PATHCOLORBTHEMEINDEX 0
#define PROP_PATHCOLORATHEMEINDEX 0
#define PROPM_START_PATHAUDIOLINK 0
#define PROP_PATHALTIMEOFFSET 0
#define PROP_AUDIOLINKPATHTIMEOFFSETBANDR 0
#define PROP_AUDIOLINKPATHTIMEOFFSETBANDG 0
#define PROP_AUDIOLINKPATHTIMEOFFSETBANDB 0
#define PROP_AUDIOLINKPATHTIMEOFFSETBANDA 0
#define PROP_PATHALEMISSIONOFFSET 0
#define PROP_AUDIOLINKPATHEMISSIONADDBANDR 0
#define PROP_AUDIOLINKPATHEMISSIONADDBANDG 0
#define PROP_AUDIOLINKPATHEMISSIONADDBANDB 0
#define PROP_AUDIOLINKPATHEMISSIONADDBANDA 0
#define PROP_PATHALWIDTHOFFSET 0
#define PROP_AUDIOLINKPATHWIDTHOFFSETBANDR 0
#define PROP_AUDIOLINKPATHWIDTHOFFSETBANDG 0
#define PROP_AUDIOLINKPATHWIDTHOFFSETBANDB 0
#define PROP_AUDIOLINKPATHWIDTHOFFSETBANDA 0
#define PROP_PATHALHISTORY 0
#define PROP_PATHALHISTORYBANDR 0
#define PROP_PATHALHISTORYR 0
#define PROP_PATHALHISTORYBANDG 0
#define PROP_PATHALHISTORYG 0
#define PROP_PATHALHISTORYBANDB 0
#define PROP_PATHALHISTORYB 0
#define PROP_PATHALHISTORYBANDA 0
#define PROP_PATHALHISTORYA 0
#define PROP_PATHALCHRONO 0
#define PROP_PATHCHRONOBANDR 0
#define PROP_PATHCHRONOTYPER 0
#define PROP_PATHCHRONOSPEEDR 0
#define PROP_PATHCHRONOBANDG 0
#define PROP_PATHCHRONOTYPEG 0
#define PROP_PATHCHRONOSPEEDG 0
#define PROP_PATHCHRONOBANDB 0
#define PROP_PATHCHRONOTYPEB 0
#define PROP_PATHCHRONOSPEEDB 0
#define PROP_PATHCHRONOBANDA 0
#define PROP_PATHCHRONOTYPEA 0
#define PROP_PATHCHRONOSPEEDA 0
#define PROP_PATHALAUTOCORRELATOR 0
#define PROP_PATHALAUTOCORRELATORR 0
#define PROP_PATHALAUTOCORRELATORG 0
#define PROP_PATHALAUTOCORRELATORB 0
#define PROP_PATHALAUTOCORRELATORA 0
#define PROP_PATHALCCR 0
#define PROP_PATHALCCG 0
#define PROP_PATHALCCB 0
#define PROP_PATHALCCA 0
#define PROPM_END_PATHAUDIOLINK 0
#define PROPM_END_PATHING 0
#define PROPM_START_MIRROROPTIONS 0
#define PROP_ENABLEMIRROROPTIONS 0
#define PROP_MIRROR 0
#define PROP_MIRRORTEXTUREUV 0
#define PROPM_END_MIRROROPTIONS 0
#define PROPM_START_DEPTHFX 0
#define PROP_ENABLETOUCHGLOW 0
#define PROP_DEPTHMASKUV 0
#define PROP_DEPTHCOLORTOGGLE 0
#define PROP_DEPTHCOLORBLENDMODE 0
#define PROP_DEPTHTEXTUREUV 0
#define PROP_DEPTHCOLORTHEMEINDEX 0
#define PROP_DEPTHEMISSIONSTRENGTH 0
#define PROP_DEPTHCOLORMINDEPTH 0
#define PROP_DEPTHCOLORMAXDEPTH 1
#define PROP_DEPTHCOLORMINVALUE 0
#define PROP_DEPTHCOLORMAXVALUE 1
#define PROP_DEPTHALPHATOGGLE 0
#define PROP_DEPTHALPHAMINDEPTH 0
#define PROP_DEPTHALPHAMAXDEPTH 1
#define PROP_DEPTHALPHAMINVALUE 1
#define PROP_DEPTHALPHAMAXVALUE 0
#define PROPM_END_DEPTHFX 0
#define PROPM_START_IRIDESCENCE 0
#define PROP_ENABLEIRIDESCENCE 0
#define PROP_IRIDESCENCEMASKUV 0
#define PROP_IRIDESCENCENORMALTOGGLE 0
#define PROP_IRIDESCENCENORMALINTENSITY 1
#define PROP_IRIDESCENCENORMALMAPUV 0
#define PROP_IRIDESCENCENORMALSELECTION 1
#define PROP_IRIDESCENCEINTENSITY 1
#define PROP_IRIDESCENCEADDBLEND 0
#define PROP_IRIDESCENCEREPLACEBLEND 0
#define PROP_IRIDESCENCEMULTIPLYBLEND 0
#define PROP_IRIDESCENCEEMISSIONSTRENGTH 0
#define PROP_IRIDESCENCEHUESHIFTENABLED 0
#define PROP_IRIDESCENCEHUESHIFTSPEED 0
#define PROP_IRIDESCENCEHUESHIFT 0
#define PROPM_START_IRIDESCENCEAUDIOLINK 0
#define PROP_IRIDESCENCEAUDIOLINKEMISSIONADDBAND 0
#define PROPM_END_IRIDESCENCEAUDIOLINK 0
#define PROPM_END_IRIDESCENCE 0
#define PROPM_START_TEXT 0
#define PROP_TEXTPIXELRANGE 4
#define PROP_TEXTENABLED 0
#define PROPM_START_TEXTFPS 0
#define PROP_TEXTFPSENABLED 0
#define PROP_TEXTFPSUV 0
#define PROP_TEXTFPSCOLORTHEMEINDEX 0
#define PROP_TEXTFPSEMISSIONSTRENGTH 0
#define PROP_TEXTFPSROTATION 0
#define PROPM_END_TEXTFPS 0
#define PROPM_START_TEXTPOSITION 0
#define PROP_TEXTPOSITIONENABLED 0
#define PROP_TEXTPOSITIONUV 0
#define PROP_TEXTPOSITIONCOLORTHEMEINDEX 0
#define PROP_TEXTPOSITIONEMISSIONSTRENGTH 0
#define PROP_TEXTPOSITIONROTATION 0
#define PROPM_END_TEXTPOSITION 0
#define PROPM_START_TEXTINSTANCETIME 0
#define PROP_TEXTTIMEENABLED 0
#define PROP_TEXTTIMEUV 0
#define PROP_TEXTTIMECOLORTHEMEINDEX 0
#define PROP_TEXTTIMEEMISSIONSTRENGTH 0
#define PROP_TEXTTIMEROTATION 0
#define PROPM_END_TEXTINSTANCETIME 0
#define PROPM_END_TEXT 0
#define PROPM_START_FXPROXIMITYCOLOR 0
#define PROP_FXPROXIMITYCOLOR 0
#define PROP_FXPROXIMITYCOLORTYPE 1
#define PROP_FXPROXIMITYCOLORMINCOLORTHEMEINDEX 0
#define PROP_FXPROXIMITYCOLORMAXCOLORTHEMEINDEX 0
#define PROP_FXPROXIMITYCOLORMINDISTANCE 0
#define PROP_FXPROXIMITYCOLORMAXDISTANCE 1
#define PROPM_END_FXPROXIMITYCOLOR 0
#define PROPM_AUDIOLINKCATEGORY 0
#define PROPM_START_AUDIOLINK 0
#define PROP_ENABLEAUDIOLINK 0
#define PROP_AUDIOLINKHELP 0
#define PROP_AUDIOLINKANIMTOGGLE 1
#define PROP_DEBUGWAVEFORM 0
#define PROP_DEBUGDFT 0
#define PROP_DEBUGBASS 0
#define PROP_DEBUGLOWMIDS 0
#define PROP_DEBUGHIGHMIDS 0
#define PROP_DEBUGTREBLE 0
#define PROP_DEBUGCCCOLORS 0
#define PROP_DEBUGCCSTRIP 0
#define PROP_DEBUGCCLIGHTS 0
#define PROP_DEBUGAUTOCORRELATOR 0
#define PROP_DEBUGCHRONOTENSITY 0
#define PROP_DEBUGVISUALIZERHELPBOX 0
#define PROPM_END_AUDIOLINK 0
#define PROPM_START_ALDECALSPECTRUM 0
#define PROP_ENABLEALDECAL 0
#define PROP_ALDECALTYPE 0
#define PROP_ALDECALUVMODE 0
#define PROP_ALDECALUV 0
#define PROP_ALUVROTATION 0
#define PROP_ALUVROTATIONSPEED 0
#define PROP_ALDECALLINEWIDTH 1
#define PROP_ALDECALVOLUMESTEP 0
#define PROP_ALDECALVOLUMECLIPMIN 0
#define PROP_ALDECALVOLUMECLIPMAX 1
#define PROP_ALDECALBANDSTEP 0
#define PROP_ALDECALBANDCLIPMIN 0
#define PROP_ALDECALBANDCLIPMAX 1
#define PROP_ALDECALSHAPECLIP 0
#define PROP_ALDECALSHAPECLIPVOLUMEWIDTH 0.5
#define PROP_ALDECALSHAPECLIPBANDWIDTH 0.5
#define PROP_ALDECALVOLUME 0.5
#define PROP_ALDECALBASEBOOST 5
#define PROP_ALDECALTREBLEBOOST 1
#define PROP_ALDECALCOLORMASKUV 0
#define PROP_ALDECALVOLUMECOLORSOURCE 1
#define PROP_ALDECALLOWEMISSION 0
#define PROP_ALDECALMIDEMISSION 0
#define PROP_ALDECALHIGHEMISSION 0
#define PROP_ALDECALBLENDTYPE 0
#define PROP_ALDECALBLENDALPHA 1
#define PROP_ALDECALCONTROLSALPHA 0
#define PROPM_END_ALDECALSPECTRUM 0
#define PROPM_MODIFIERCATEGORY 0
#define PROPM_START_UVDISTORTION 0
#define PROP_ENABLEDISTORTION 0
#define PROP_DISTORTIONUVTODISTORT 0
#define PROP_DISTORTIONMASKUV 0
#define PROP_DISTORTIONFLOWTEXTUREUV 0
#define PROP_DISTORTIONFLOWTEXTURE1UV 0
#define PROP_DISTORTIONSTRENGTH 0.5
#define PROP_DISTORTIONSTRENGTH1 0.5
#define PROPM_START_DISTORTIONAUDIOLINK 0
#define PROP_ENABLEDISTORTIONAUDIOLINK 0
#define PROP_DISTORTIONSTRENGTHAUDIOLINKBAND 0
#define PROP_DISTORTIONSTRENGTH1AUDIOLINKBAND 0
#define PROPM_END_DISTORTIONAUDIOLINK 0
#define PROPM_END_UVDISTORTION 0
#define PROPM_START_UVPANOSPHERE 0
#define PROP_STEREOENABLED 0
#define PROP_PANOUSEBOTHEYES 1
#define PROPM_END_UVPANOSPHERE 0
#define PROPM_START_UVPOLAR 0
#define PROP_POLARUV 0
#define PROP_POLARRADIALSCALE 1
#define PROP_POLARLENGTHSCALE 1
#define PROP_POLARSPIRALPOWER 0
#define PROPM_END_UVPOLAR 0
#define PROPM_START_PARALLAX 0
#define PROP_POIPARALLAX 0
#define PROP_PARALLAXUV 0
#define PROP_HEIGHTMAPUV 0
#define PROP_HEIGHTMASKINVERT 0
#define PROP_HEIGHTMASKUV 0
#define PROP_HEIGHTSTRENGTH 0.4247461
#define PROP_CURVATUREU 0
#define PROP_CURVATUREV 0
#define PROP_HEIGHTSTEPSMIN 10
#define PROP_HEIGHTSTEPSMAX 128
#define PROP_CURVFIX 1
#define PROPM_END_PARALLAX 0
#define PROPM_THIRDPARTYCATEGORY 0
#define PROPM_POSTPROCESSING 0
#define PROPM_START_POILIGHTDATA 0
#define PROP_PPHELP 0
#define PROP_PPLIGHTINGMULTIPLIER 1
#define PROP_PPLIGHTINGADDITION 0
#define PROP_PPEMISSIONMULTIPLIER 1
#define PROP_PPFINALCOLORMULTIPLIER 1
#define PROPM_END_POILIGHTDATA 0
#define PROPM_START_POSTPROCESS 0
#define PROP_POSTPROCESS 0
#define PROP_PPMASKINVERT 0
#define PROP_PPMASKUV 0
#define PROP_PPLUTSTRENGTH 0
#define PROP_PPHUE 0
#define PROP_PPCONTRAST 1
#define PROP_PPSATURATION 1
#define PROP_PPBRIGHTNESS 1
#define PROP_PPLIGHTNESS 0
#define PROP_PPHDR 0
#define PROPM_END_POSTPROCESS 0
#define PROPM_RENDERINGCATEGORY 0
#define PROP_CULL 2
#define PROP_ZTEST 4
#define PROP_ZWRITE 1
#define PROP_COLORMASK 15
#define PROP_OFFSETFACTOR 0
#define PROP_OFFSETUNITS 0
#define PROP_RENDERINGREDUCECLIPDISTANCE 0
#define PROP_IGNOREFOG 0
#define PROPINSTANCING 0
#define PROPM_START_BLENDING 0
#define PROP_BLENDOP 0
#define PROP_BLENDOPALPHA 0
#define PROP_SRCBLEND 1
#define PROP_DSTBLEND 0
#define PROP_ADDBLENDOP 0
#define PROP_ADDBLENDOPALPHA 0
#define PROP_ADDSRCBLEND 1
#define PROP_ADDDSTBLEND 1
#define PROPM_END_BLENDING 0
#define PROPM_START_STENCILPASSOPTIONS 0
#define PROP_STENCILREF 0
#define PROP_STENCILREADMASK 255
#define PROP_STENCILWRITEMASK 255
#define PROP_STENCILPASSOP 0
#define PROP_STENCILFAILOP 0
#define PROP_STENCILZFAILOP 0
#define PROP_STENCILCOMPAREFUNCTION 8
#define PROPM_END_STENCILPASSOPTIONS 0

			#pragma target 5.0
			#pragma skip_variants DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING DIRLIGHTMAP_COMBINED SHADOWS_SHADOWMASK
			#pragma multi_compile_fwdadd_fullshadows
			#pragma multi_compile_instancing
			#pragma multi_compile_fog
			#define POI_PASS_ADD
			#include "UnityCG.cginc"
			#include "UnityStandardUtils.cginc"
			#include "AutoLight.cginc"
			#include "UnityLightingCommon.cginc"
			#include "UnityPBSLighting.cginc"
			#ifdef POI_PASS_META
			#include "UnityMetaPass.cginc"
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#define DielectricSpec float4(0.04, 0.04, 0.04, 1.0 - 0.04)
			#define PI float(3.14159265359)
			#define POI2D_SAMPLER_PAN(tex, texSampler, uv, pan) (UNITY_SAMPLE_TEX2D_SAMPLER(tex, texSampler, uv + _Time.x * pan))
			#define POI2D_SAMPLER_PANGRAD(tex, texSampler, uv, pan, ddx, ddy) (UNITY_SAMPLE_TEX2D_SAMPLER(tex, texSampler, uv + _Time.x * pan, ddx, ddy))
			#define POI2D_SAMPLER(tex, texSampler, uv) (UNITY_SAMPLE_TEX2D_SAMPLER(tex, texSampler, uv))
			#define POI2D_PAN(tex, uv, pan) (tex2D(tex, uv + _Time.x * pan))
			#define POI2D(tex, uv) (tex2D(tex, uv))
			#define POI_SAMPLE_TEX2D(tex, uv) (UNITY_SAMPLE_TEX2D(tex, uv))
			#define POI_SAMPLE_TEX2D_PAN(tex, uv, pan) (UNITY_SAMPLE_TEX2D(tex, uv + _Time.x * pan))
			#define POI2D_MAINTEX_SAMPLER_PAN_INLINED(tex, poiMesh) (POI2D_SAMPLER_PAN(tex, _MainTex, poiUV(poiMesh.uv[tex##UV], tex##_ST), tex##Pan))
			#define POI_SAFE_RGB0 float4(mainTexture.rgb * .0001, 0)
			#define POI_SAFE_RGB1 float4(mainTexture.rgb * .0001, 1)
			#define POI_SAFE_RGBA mainTexture
			#if defined(UNITY_COMPILER_HLSL)
			#define PoiInitStruct(type, name) name = (type)0;
			#else
			#define PoiInitStruct(type, name)
			#endif
			#define POI_ERROR(poiMesh, gridSize) lerp(float3(1, 0, 1), float3(0, 0, 0), fmod(floor((poiMesh.worldPos.x) * gridSize) + floor((poiMesh.worldPos.y) * gridSize) + floor((poiMesh.worldPos.z) * gridSize), 2) == 0)
			#define POI_MODE_OPAQUE 0
			#define POI_MODE_CUTOUT 1
			#define POI_MODE_FADE 2
			#define POI_MODE_TRANSPARENT 3
			#define POI_MODE_ADDITIVE 4
			#define POI_MODE_SOFTADDITIVE 5
			#define POI_MODE_MULTIPLICATIVE 6
			#define POI_MODE_2XMULTIPLICATIVE 7
			#define POI_MODE_TRANSCLIPPING 9
			#define ALPASS_DFT                      uint2(0,4)   //Size: 128, 2
			#define ALPASS_WAVEFORM                 uint2(0,6)   //Size: 128, 16
			#define ALPASS_AUDIOLINK                uint2(0,0)   //Size: 128, 4
			#define ALPASS_AUDIOBASS                uint2(0,0)   //Size: 128, 1
			#define ALPASS_AUDIOLOWMIDS             uint2(0,1)   //Size: 128, 1
			#define ALPASS_AUDIOHIGHMIDS            uint2(0,2)   //Size: 128, 1
			#define ALPASS_AUDIOTREBLE              uint2(0,3)   //Size: 128, 1
			#define ALPASS_AUDIOLINKHISTORY         uint2(1,0)   //Size: 127, 4
			#define ALPASS_GENERALVU                uint2(0,22)  //Size: 12, 1
			#define ALPASS_CCINTERNAL               uint2(12,22) //Size: 12, 2
			#define ALPASS_CCCOLORS                 uint2(25,22) //Size: 11, 1
			#define ALPASS_CCSTRIP                  uint2(0,24)  //Size: 128, 1
			#define ALPASS_CCLIGHTS                 uint2(0,25)  //Size: 128, 2
			#define ALPASS_AUTOCORRELATOR           uint2(0,27)  //Size: 128, 1
			#define ALPASS_GENERALVU_INSTANCE_TIME  uint2(2,22)
			#define ALPASS_GENERALVU_LOCAL_TIME     uint2(3,22)
			#define ALPASS_GENERALVU_NETWORK_TIME   uint2(4,22)
			#define ALPASS_GENERALVU_PLAYERINFO     uint2(6,22)
			#define ALPASS_FILTEREDAUDIOLINK        uint2(0,28)  //Size: 16, 4
			#define ALPASS_CHRONOTENSITY            uint2(16,28) //Size: 8, 4
			#define ALPASS_THEME_COLOR0             uint2(0,23)
			#define ALPASS_THEME_COLOR1             uint2(1,23)
			#define ALPASS_THEME_COLOR2             uint2(2,23)
			#define ALPASS_THEME_COLOR3             uint2(3,23)
			#define ALPASS_FILTEREDVU               uint2(24,28) //Size: 4, 4
			#define ALPASS_FILTEREDVU_INTENSITY     uint2(24,28) //Size: 4, 1
			#define ALPASS_FILTEREDVU_MARKER        uint2(24,29) //Size: 4, 1
			#define AUDIOLINK_SAMPHIST              3069        // Internal use for algos, do not change.
			#define AUDIOLINK_SAMPLEDATA24          2046
			#define AUDIOLINK_EXPBINS               24
			#define AUDIOLINK_EXPOCT                10
			#define AUDIOLINK_ETOTALBINS (AUDIOLINK_EXPBINS * AUDIOLINK_EXPOCT)
			#define AUDIOLINK_WIDTH                 128
			#define AUDIOLINK_SPS                   48000       // Samples per second
			#define AUDIOLINK_ROOTNOTE              0
			#define AUDIOLINK_4BAND_FREQFLOOR       0.123
			#define AUDIOLINK_4BAND_FREQCEILING     1
			#define AUDIOLINK_BOTTOM_FREQUENCY      13.75
			#define AUDIOLINK_BASE_AMPLITUDE        2.5
			#define AUDIOLINK_DELAY_COEFFICIENT_MIN 0.3
			#define AUDIOLINK_DELAY_COEFFICIENT_MAX 0.9
			#define AUDIOLINK_DFT_Q                 4.0
			#define AUDIOLINK_TREBLE_CORRECTION     5.0
			#define COLORCHORD_EMAXBIN              192
			#define COLORCHORD_IIR_DECAY_1          0.90
			#define COLORCHORD_IIR_DECAY_2          0.85
			#define COLORCHORD_CONSTANT_DECAY_1     0.01
			#define COLORCHORD_CONSTANT_DECAY_2     0.0
			#define COLORCHORD_NOTE_CLOSEST         3.0
			#define COLORCHORD_NEW_NOTE_GAIN        8.0
			#define COLORCHORD_MAX_NOTES            10
			#ifndef glsl_mod
			#define glsl_mod(x, y) (((x) - (y) * floor((x) / (y))))
			#endif
			uniform float4               _AudioTexture_TexelSize;
			#ifdef SHADER_TARGET_SURFACE_ANALYSIS
			#define AUDIOLINK_STANDARD_INDEXING
			#endif
			#ifdef AUDIOLINK_STANDARD_INDEXING
			sampler2D _AudioTexture;
			#define AudioLinkData(xycoord) tex2Dlod(_AudioTexture, float4(uint2(xycoord) * _AudioTexture_TexelSize.xy, 0, 0))
			#else
			uniform Texture2D<float4> _AudioTexture;
			SamplerState sampler_AudioTexture;
			#define AudioLinkData(xycoord) _AudioTexture[uint2(xycoord)]
			#endif
			float _Mode;
			float4 _GlobalThemeColor0;
			float4 _GlobalThemeColor1;
			float4 _GlobalThemeColor2;
			float4 _GlobalThemeColor3;
			float _StereoEnabled;
			float _PolarUV;
			float2 _PolarCenter;
			float _PolarRadialScale;
			float _PolarLengthScale;
			float _PolarSpiralPower;
			float _PanoUseBothEyes;
			#if defined(PROP_LIGHTINGAOMAPS) || !defined(OPTIMIZER_ENABLED)
			Texture2D _LightingAOMaps;
			#endif
			float4 _LightingAOMaps_ST;
			float2 _LightingAOMapsPan;
			float _LightingAOMapsUV;
			float _LightDataAOStrengthR;
			float _LightDataAOStrengthG;
			float _LightDataAOStrengthB;
			float _LightDataAOStrengthA;
			#if defined(PROP_LIGHTINGDETAILSHADOWMAPS) || !defined(OPTIMIZER_ENABLED)
			Texture2D _LightingDetailShadowMaps;
			#endif
			float4 _LightingDetailShadowMaps_ST;
			float2 _LightingDetailShadowMapsPan;
			float _LightingDetailShadowMapsUV;
			float _LightingDetailShadowStrengthR;
			float _LightingDetailShadowStrengthG;
			float _LightingDetailShadowStrengthB;
			float _LightingDetailShadowStrengthA;
			#if defined(PROP_LIGHTINGSHADOWMASKS) || !defined(OPTIMIZER_ENABLED)
			Texture2D _LightingShadowMasks;
			#endif
			float4 _LightingShadowMasks_ST;
			float2 _LightingShadowMasksPan;
			float _LightingShadowMasksUV;
			float _LightingShadowMaskStrengthR;
			float _LightingShadowMaskStrengthG;
			float _LightingShadowMaskStrengthB;
			float _LightingShadowMaskStrengthA;
			float _Unlit_Intensity;
			float _LightingColorMode;
			float _LightingMapMode;
			float _LightingDirectionMode;
			float3 _LightngForcedDirection;
			float _LightingIndirectUsesNormals;
			float _LightingCapEnabled;
			float _LightingCap;
			float _LightingForceColorEnabled;
			float3 _LightingForcedColor;
			float _LightingForcedColorThemeIndex;
			float _LightingCastedShadows;
			float _LightingMonochromatic;
			float _LightingAdditiveMonochromatic;
			float _LightingMinLightBrightness;
			float _LightingAdditiveLimited;
			float _LightingAdditiveLimit;
			float _LightingAdditivePassthrough;
			float _LightingDebugVisualize;
			float _IgnoreFog;
			float _RenderingReduceClipDistance;
			float4 _Color;
			float _ColorThemeIndex;
			UNITY_DECLARE_TEX2D(_MainTex);
			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
			float4 _MainTex_ST;
			float2 _MainTexPan;
			float _MainTexUV;
			float4 _MainTex_TexelSize;
			Texture2D _BumpMap;
			float4 _BumpMap_ST;
			float2 _BumpMapPan;
			float _BumpMapUV;
			float _BumpScale;
			Texture2D _ClippingMask;
			float4 _ClippingMask_ST;
			float2 _ClippingMaskPan;
			float _ClippingMaskUV;
			float _Inverse_Clipping;
			float _Cutoff;
			float _MainColorAdjustToggle;
			#if defined(PROP_MAINCOLORADJUSTTEXTURE) || !defined(OPTIMIZER_ENABLED)
			Texture2D _MainColorAdjustTexture;
			#endif
			float4 _MainColorAdjustTexture_ST;
			float2 _MainColorAdjustTexturePan;
			float _MainColorAdjustTextureUV;
			float _MainHueShiftToggle;
			float _MainHueShiftReplace;
			float _MainHueShift;
			float _MainHueShiftSpeed;
			float _Saturation;
			float _MainBrightness;
			float _MainHueALCTEnabled;
			float _MainALHueShiftBand;
			float _MainALHueShiftCTIndex;
			float _MainHueALMotionSpeed;
			SamplerState sampler_linear_clamp;
			SamplerState sampler_linear_repeat;
			float _AlphaForceOpaque;
			float _AlphaMod;
			float _AlphaPremultiply;
			float _AlphaToCoverage;
			float _AlphaSharpenedA2C;
			float _AlphaMipScale;
			float _AlphaDithering;
			float _AlphaDitherGradient;
			float _AlphaDistanceFade;
			float _AlphaDistanceFadeType;
			float _AlphaDistanceFadeMinAlpha;
			float _AlphaDistanceFadeMaxAlpha;
			float _AlphaDistanceFadeMin;
			float _AlphaDistanceFadeMax;
			float _AlphaFresnel;
			float _AlphaFresnelAlpha;
			float _AlphaFresnelSharpness;
			float _AlphaFresnelWidth;
			float _AlphaFresnelInvert;
			float _AlphaAngular;
			float _AngleType;
			float _AngleCompareTo;
			float3 _AngleForwardDirection;
			float _CameraAngleMin;
			float _CameraAngleMax;
			float _ModelAngleMin;
			float _ModelAngleMax;
			float _AngleMinAlpha;
			float _AlphaAudioLinkEnabled;
			float2 _AlphaAudioLinkAddRange;
			float _AlphaAudioLinkAddBand;
			float _MainVertexColoringLinearSpace;
			float _MainVertexColoring;
			float _MainUseVertexColorAlpha;
			#if defined(PROP_DECALMASK) || !defined(OPTIMIZER_ENABLED)
			Texture2D _DecalMask;
			float4 _DecalMask_ST;
			float2 _DecalMaskPan;
			float _DecalMaskUV;
			#endif
			float _ShadowOffset;
			float _ShadowStrength;
			float _LightingIgnoreAmbientColor;
			float _LightingGradientStart;
			float _LightingGradientEnd;
			float3 _LightingShadowColor;
			float _LightingGradientStartWrap;
			float _LightingGradientEndWrap;
			#ifdef _LIGHTINGMODE_SHADEMAP
			float3 _1st_ShadeColor;
			#if defined(PROP_1ST_SHADEMAP) || !defined(OPTIMIZER_ENABLED)
			Texture2D _1st_ShadeMap;
			#endif
			float4 _1st_ShadeMap_ST;
			float2 _1st_ShadeMapPan;
			float _1st_ShadeMapUV;
			float _Use_1stShadeMapAlpha_As_ShadowMask;
			float _1stShadeMapMask_Inverse;
			float _Use_BaseAs1st;
			float3 _2nd_ShadeColor;
			#if defined(PROP_2ND_SHADEMAP) || !defined(OPTIMIZER_ENABLED)
			Texture2D _2nd_ShadeMap;
			#endif
			float4 _2nd_ShadeMap_ST;
			float2 _2nd_ShadeMapPan;
			float _2nd_ShadeMapUV;
			float _Use_2ndShadeMapAlpha_As_ShadowMask;
			float _2ndShadeMapMask_Inverse;
			float _Use_1stAs2nd;
			float _BaseColor_Step;
			float _BaseShade_Feather;
			float _ShadeColor_Step;
			float _1st2nd_Shades_Feather;
			float _ShadingShadeMapBlendType;
			#endif
			sampler2D _SkinLUT;
			float _SssScale;
			float _SssBumpBlur;
			float3 _SssTransmissionAbsorption;
			float3 _SssColorBleedAoWeights;
			#ifdef _LIGHTINGMODE_MULTILAYER_MATH
			float4 _ShadowColor;
			#if defined(PROP_SHADOWCOLORTEX) || !defined(OPTIMIZER_ENABLED)
			Texture2D _ShadowColorTex;
			float4 _ShadowColorTex_ST;
			float2 _ShadowColorTexPan;
			float _ShadowColorTexUV;
			#endif
			float _ShadowBorder;
			float _ShadowBlur;
			float4 _Shadow2ndColor;
			#if defined(PROP_SHADOW2NDCOLORTEX) || !defined(OPTIMIZER_ENABLED)
			Texture2D _Shadow2ndColorTex;
			float4 _Shadow2ndColorTex_ST;
			float2 _Shadow2ndColorTexPan;
			float _Shadow2ndColorTexUV;
			#endif
			float _Shadow2ndBorder;
			float _Shadow2ndBlur;
			float4 _Shadow3rdColor;
			#if defined(PROP_SHADOW3RDCOLORTEX) || !defined(OPTIMIZER_ENABLED)
			Texture2D _Shadow3rdColorTex;
			float4 _Shadow3rdColorTex_ST;
			float2 _Shadow3rdColorTexPan;
			float _Shadow3rdColorTexUV;
			#endif
			float _Shadow3rdBorder;
			float _Shadow3rdBlur;
			float4 _ShadowBorderColor;
			float _ShadowBorderRange;
			#endif
			#ifdef _LIGHTINGMODE_CLOTH
			Texture2D_float _ClothDFG;
			SamplerState sampler_ClothDFG;
			#if defined(PROP_CLOTHMETALLICSMOOTHNESSMAP) || !defined(OPTIMIZER_ENABLED)
			Texture2D _ClothMetallicSmoothnessMap;
			#endif
			float4 _ClothMetallicSmoothnessMap_ST;
			float2 _ClothMetallicSmoothnessMapPan;
			float _ClothMetallicSmoothnessMapUV;
			float _ClothMetallicSmoothnessMapInvert;
			float _ClothMetallic;
			float _ClothReflectance;
			float _ClothSmoothness;
			#endif
			#ifdef _LIGHTINGMODE_SDF
			#if defined(PROP_SDFSHADINGTEXTURE) || !defined(OPTIMIZER_ENABLED)
			Texture2D _SDFShadingTexture;
			float _SDFShadingTextureUV;
			float2 _SDFShadingTexturePan;
			float4 _SDFShadingTexture_ST;
			#endif
			#endif
			float _LightingAdditiveType;
			float _LightingAdditiveGradientStart;
			float _LightingAdditiveGradientEnd;
			float _LightingAdditiveDetailStrength;
			float4 _MochieReflCube_HDR;
			#if defined(PROP_DEPTHMASK) || !defined(OPTIMIZER_ENABLED)
			Texture2D _DepthMask;
			#endif
			float4 _DepthMask_ST;
			float2 _DepthMaskPan;
			float _DepthMaskUV;
			float _DepthColorToggle;
			float _DepthColorBlendMode;
			#if defined(PROP_DEPTHTEXTURE) || !defined(OPTIMIZER_ENABLED)
			Texture2D _DepthTexture;
			#endif
			float4 _DepthTexture_ST;
			float2 _DepthTexturePan;
			float _DepthTextureUV;
			float3 _DepthColor;
			float _DepthColorThemeIndex;
			float _DepthColorMinDepth;
			float _DepthColorMaxDepth;
			float _DepthColorMinValue;
			float _DepthColorMaxValue;
			float _DepthEmissionStrength;
			float _DepthAlphaToggle;
			float _DepthAlphaMinValue;
			float _DepthAlphaMaxValue;
			float _DepthAlphaMinDepth;
			float _DepthAlphaMaxDepth;
			float _FXProximityColor;
			float _FXProximityColorType;
			float3 _FXProximityColorMinColor;
			float3 _FXProximityColorMaxColor;
			float _FXProximityColorMinColorThemeIndex;
			float _FXProximityColorMaxColorThemeIndex;
			float _FXProximityColorMinDistance;
			float _FXProximityColorMaxDistance;
			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 color : COLOR;
				float2 uv0 : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float2 uv2 : TEXCOORD2;
				float2 uv3 : TEXCOORD3;
				uint vertexId : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv[4] : TEXCOORD0;
				float3 objNormal : TEXCOORD4;
				float3 normal : TEXCOORD5;
				float3 tangent : TEXCOORD6;
				float3 binormal : TEXCOORD7;
				float4 worldPos : TEXCOORD8;
				float4 localPos : TEXCOORD9;
				float3 objectPos : TEXCOORD10;
				float4 vertexColor : TEXCOORD11;
				float4 lightmapUV : TEXCOORD12;
				float4 grabPos: TEXCOORD13;
				float4 worldDirection: TEXCOORD14;
				UNITY_SHADOW_COORDS(15)
				UNITY_FOG_COORDS(16)
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			struct PoiMesh
			{
				float3 normals[2];
				float3 objNormal;
				float3 tangentSpaceNormal;
				float3 binormal;
				float3 tangent;
				float3 worldPos;
				float3 localPos;
				float3 objectPosition;
				float isFrontFace;
				float4 vertexColor;
				float4 lightmapUV;
				float2 uv[8];
				float2 parallaxUV;
			};
			struct PoiCam
			{
				float3 viewDir;
				float3 forwardDir;
				float3 worldPos;
				float distanceToVert;
				float4 clipPos;
				float3 reflectionDir;
				float3 vertexReflectionDir;
				float3 tangentViewDir;
				float4 grabPos;
				float2 screenUV;
				float vDotN;
				float4 worldDirection;
			};
			struct PoiMods
			{
				float4 Mask;
				float4 audioLink;
				float audioLinkAvailable;
				float audioLinkVersion;
				float4 audioLinkTexture;
				float2 detailMask;
				float2 backFaceDetailIntensity;
				float globalEmission;
				float4 globalColorTheme[12];
				float ALTime[8];
			};
			struct PoiLight
			{
				float3 direction;
				float attenuation;
				float attenuationStrength;
				float3 directColor;
				float3 indirectColor;
				float occlusion;
				float shadowMask;
				float detailShadow;
				float3 halfDir;
				float lightMap;
				float3 rampedLightMap;
				float vertexNDotL;
				float nDotL;
				float nDotV;
				float vertexNDotV;
				float nDotH;
				float vertexNDotH;
				float lDotv;
				float lDotH;
				float nDotLSaturated;
				float nDotLNormalized;
				#ifdef UNITY_PASS_FORWARDADD
				float additiveShadow;
				#endif
				float3 finalLighting;
				float3 finalLightAdd;
				#if defined(VERTEXLIGHT_ON) && defined(POI_VERTEXLIGHT_ON)
				float4 vDotNL;
				float4 vertexVDotNL;
				float3 vColor[4];
				float4 vCorrectedDotNL;
				float4 vAttenuation;
				float4 vAttenuationDotNL;
				float3 vPosition[4];
				float3 vDirection[4];
				float3 vFinalLighting;
				float3 vHalfDir[4];
				half4 vDotNH;
				half4 vertexVDotNH;
				half4 vDotLH;
				#endif
			};
			struct PoiVertexLights
			{
				float3 direction;
				float3 color;
				float attenuation;
			};
			struct PoiFragData
			{
				float3 baseColor;
				float3 finalColor;
				float alpha;
				float3 emission;
			};
			float2 poiUV(float2 uv, float4 tex_st)
			{
				return uv * tex_st.xy + tex_st.zw;
			}
			float calculateluminance(float3 color)
			{
				return color.r * 0.299 + color.g * 0.587 + color.b * 0.114;
			}
			bool IsInMirror()
			{
				return unity_CameraProjection[2][0] != 0.f || unity_CameraProjection[2][1] != 0.f;
			}
			bool IsOrthographicCamera()
			{
				return unity_OrthoParams.w == 1 || UNITY_MATRIX_P[3][3] == 1;
			}
			float shEvaluateDiffuseL1Geomerics_local(float L0, float3 L1, float3 n)
			{
				float R0 = max(0, L0);
				float3 R1 = 0.5f * L1;
				float lenR1 = length(R1);
				float q = dot(normalize(R1), n) * 0.5 + 0.5;
				q = saturate(q); // Thanks to ScruffyRuffles for the bug identity.
				float p = 1.0f + 2.0f * lenR1 / R0;
				float a = (1.0f - lenR1 / R0) / (1.0f + lenR1 / R0);
				return R0 * (a + (1.0f - a) * (p + 1.0f) * pow(q, p));
			}
			half3 BetterSH9(half4 normal)
			{
				float3 indirect;
				float3 L0 = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w) + float3(unity_SHBr.z, unity_SHBg.z, unity_SHBb.z) / 3.0;
				indirect.r = shEvaluateDiffuseL1Geomerics_local(L0.r, unity_SHAr.xyz, normal.xyz);
				indirect.g = shEvaluateDiffuseL1Geomerics_local(L0.g, unity_SHAg.xyz, normal.xyz);
				indirect.b = shEvaluateDiffuseL1Geomerics_local(L0.b, unity_SHAb.xyz, normal.xyz);
				indirect = max(0, indirect);
				indirect += SHEvalLinearL2(normal);
				return indirect;
			}
			float3 getCameraForward()
			{
				#if UNITY_SINGLE_PASS_STEREO
				float3 p1 = mul(unity_StereoCameraToWorld[0], float4(0, 0, 1, 1));
				float3 p2 = mul(unity_StereoCameraToWorld[0], float4(0, 0, 0, 1));
				#else
				float3 p1 = mul(unity_CameraToWorld, float4(0, 0, 1, 1)).xyz;
				float3 p2 = mul(unity_CameraToWorld, float4(0, 0, 0, 1)).xyz;
				#endif
				return normalize(p2 - p1);
			}
			half3 GetSHLength()
			{
				half3 x, x1;
				x.r = length(unity_SHAr);
				x.g = length(unity_SHAg);
				x.b = length(unity_SHAb);
				x1.r = length(unity_SHBr);
				x1.g = length(unity_SHBg);
				x1.b = length(unity_SHBb);
				return x + x1;
			}
			float3 BoxProjection(float3 direction, float3 position, float4 cubemapPosition, float3 boxMin, float3 boxMax)
			{
				#if UNITY_SPECCUBE_BOX_PROJECTION
				if (cubemapPosition.w > 0)
				{
					float3 factors = ((direction > 0 ? boxMax : boxMin) - position) / direction;
					float scalar = min(min(factors.x, factors.y), factors.z);
					direction = direction * scalar + (position - cubemapPosition.xyz);
				}
				#endif
				return direction;
			}
			float poiMax(float2 i)
			{
				return max(i.x, i.y);
			}
			float poiMax(float3 i)
			{
				return max(max(i.x, i.y), i.z);
			}
			float poiMax(float4 i)
			{
				return max(max(max(i.x, i.y), i.z), i.w);
			}
			float3 calculateNormal(in float3 baseNormal, in PoiMesh poiMesh, in Texture2D normalTexture, in float4 normal_ST, in float2 normalPan, in float normalUV, in float normalIntensity)
			{
				float3 normal = UnpackScaleNormal(POI2D_SAMPLER_PAN(normalTexture, _MainTex, poiUV(poiMesh.uv[normalUV], normal_ST), normalPan), normalIntensity);
				return normalize(
				normal.x * poiMesh.tangent +
				normal.y * poiMesh.binormal +
				normal.z * baseNormal
				);
			}
			float remap(float x, float minOld, float maxOld, float minNew = 0, float maxNew = 1)
			{
				return minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld);
			}
			float2 remap(float2 x, float2 minOld, float2 maxOld, float2 minNew = 0, float2 maxNew = 1)
			{
				return minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld);
			}
			float3 remap(float3 x, float3 minOld, float3 maxOld, float3 minNew = 0, float3 maxNew = 1)
			{
				return minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld);
			}
			float4 remap(float4 x, float4 minOld, float4 maxOld, float4 minNew = 0, float4 maxNew = 1)
			{
				return minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld);
			}
			float remapClamped(float minOld, float maxOld, float x, float minNew = 0, float maxNew = 1)
			{
				return clamp(minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld), minNew, maxNew);
			}
			float2 remapClamped(float2 minOld, float2 maxOld, float2 x, float2 minNew, float2 maxNew)
			{
				return clamp(minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld), minNew, maxNew);
			}
			float3 remapClamped(float3 minOld, float3 maxOld, float3 x, float3 minNew, float3 maxNew)
			{
				return clamp(minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld), minNew, maxNew);
			}
			float4 remapClamped(float4 minOld, float4 maxOld, float4 x, float4 minNew, float4 maxNew)
			{
				return clamp(minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld), minNew, maxNew);
			}
			float2 calcParallax(in float height, in PoiCam poiCam)
			{
				return ((height * - 1) + 1) * (poiCam.tangentViewDir.xy / poiCam.tangentViewDir.z);
			}
			float4 poiBlend(const float sourceFactor, const  float4 sourceColor, const  float destinationFactor, const  float4 destinationColor, const float4 blendFactor)
			{
				float4 sA = 1 - blendFactor;
				const float4 blendData[11] = {
					float4(0.0, 0.0, 0.0, 0.0),
					float4(1.0, 1.0, 1.0, 1.0),
					destinationColor,
					sourceColor,
					float4(1.0, 1.0, 1.0, 1.0) - destinationColor,
					sA,
					float4(1.0, 1.0, 1.0, 1.0) - sourceColor,
					sA,
					float4(1.0, 1.0, 1.0, 1.0) - sA,
					saturate(sourceColor.aaaa),
					1 - sA,
				};
				return lerp(blendData[sourceFactor] * sourceColor + blendData[destinationFactor] * destinationColor, sourceColor, sA);
			}
			float3 blendAverage(float3 base, float3 blend)
			{
				return (base + blend) / 2.0;
			}
			float blendColorBurn(float base, float blend)
			{
				return (blend == 0.0)?blend : max((1.0 - ((1.0 - base) / blend)), 0.0);
			}
			float3 blendColorBurn(float3 base, float3 blend)
			{
				return float3(blendColorBurn(base.r, blend.r), blendColorBurn(base.g, blend.g), blendColorBurn(base.b, blend.b));
			}
			float blendColorDodge(float base, float blend)
			{
				return (blend == 1.0)?blend : min(base / (1.0 - blend), 1.0);
			}
			float3 blendColorDodge(float3 base, float3 blend)
			{
				return float3(blendColorDodge(base.r, blend.r), blendColorDodge(base.g, blend.g), blendColorDodge(base.b, blend.b));
			}
			float blendDarken(float base, float blend)
			{
				return min(blend, base);
			}
			float3 blendDarken(float3 base, float3 blend)
			{
				return float3(blendDarken(base.r, blend.r), blendDarken(base.g, blend.g), blendDarken(base.b, blend.b));
			}
			float3 blendExclusion(float3 base, float3 blend)
			{
				return base + blend - 2.0 * base * blend;
			}
			float blendReflect(float base, float blend)
			{
				return (blend == 1.0)?blend : min(base * base / (1.0 - blend), 1.0);
			}
			float3 blendReflect(float3 base, float3 blend)
			{
				return float3(blendReflect(base.r, blend.r), blendReflect(base.g, blend.g), blendReflect(base.b, blend.b));
			}
			float3 blendGlow(float3 base, float3 blend)
			{
				return blendReflect(blend, base);
			}
			float blendOverlay(float base, float blend)
			{
				return base < 0.5?(2.0 * base * blend) : (1.0 - 2.0 * (1.0 - base) * (1.0 - blend));
			}
			float3 blendOverlay(float3 base, float3 blend)
			{
				return float3(blendOverlay(base.r, blend.r), blendOverlay(base.g, blend.g), blendOverlay(base.b, blend.b));
			}
			float3 blendHardLight(float3 base, float3 blend)
			{
				return blendOverlay(blend, base);
			}
			float blendVividLight(float base, float blend)
			{
				return (blend < 0.5)?blendColorBurn(base, (2.0 * blend)) : blendColorDodge(base, (2.0 * (blend - 0.5)));
			}
			float3 blendVividLight(float3 base, float3 blend)
			{
				return float3(blendVividLight(base.r, blend.r), blendVividLight(base.g, blend.g), blendVividLight(base.b, blend.b));
			}
			float blendHardMix(float base, float blend)
			{
				return (blendVividLight(base, blend) < 0.5)?0.0 : 1.0;
			}
			float3 blendHardMix(float3 base, float3 blend)
			{
				return float3(blendHardMix(base.r, blend.r), blendHardMix(base.g, blend.g), blendHardMix(base.b, blend.b));
			}
			float blendLighten(float base, float blend)
			{
				return max(blend, base);
			}
			float3 blendLighten(float3 base, float3 blend)
			{
				return float3(blendLighten(base.r, blend.r), blendLighten(base.g, blend.g), blendLighten(base.b, blend.b));
			}
			float blendLinearBurn(float base, float blend)
			{
				return max(base + blend - 1.0, 0.0);
			}
			float3 blendLinearBurn(float3 base, float3 blend)
			{
				return max(base + blend - float3(1.0, 1.0, 1.0), float3(0.0, 0.0, 0.0));
			}
			float blendLinearDodge(float base, float blend)
			{
				return min(base + blend, 1.0);
			}
			float3 blendLinearDodge(float3 base, float3 blend)
			{
				return min(base + blend, float3(1.0, 1.0, 1.0));
			}
			float blendLinearLight(float base, float blend)
			{
				return blend < 0.5?blendLinearBurn(base, (2.0 * blend)) : blendLinearDodge(base, (2.0 * (blend - 0.5)));
			}
			float3 blendLinearLight(float3 base, float3 blend)
			{
				return float3(blendLinearLight(base.r, blend.r), blendLinearLight(base.g, blend.g), blendLinearLight(base.b, blend.b));
			}
			float3 blendMultiply(float3 base, float3 blend)
			{
				return base * blend;
			}
			float3 blendNegation(float3 base, float3 blend)
			{
				return float3(1.0, 1.0, 1.0) - abs(float3(1.0, 1.0, 1.0) - base - blend);
			}
			float3 blendNormal(float3 base, float3 blend)
			{
				return blend;
			}
			float3 blendPhoenix(float3 base, float3 blend)
			{
				return min(base, blend) - max(base, blend) + float3(1.0, 1.0, 1.0);
			}
			float blendPinLight(float base, float blend)
			{
				return (blend < 0.5)?blendDarken(base, (2.0 * blend)) : blendLighten(base, (2.0 * (blend - 0.5)));
			}
			float3 blendPinLight(float3 base, float3 blend)
			{
				return float3(blendPinLight(base.r, blend.r), blendPinLight(base.g, blend.g), blendPinLight(base.b, blend.b));
			}
			float blendScreen(float base, float blend)
			{
				return 1.0 - ((1.0 - base) * (1.0 - blend));
			}
			float3 blendScreen(float3 base, float3 blend)
			{
				return float3(blendScreen(base.r, blend.r), blendScreen(base.g, blend.g), blendScreen(base.b, blend.b));
			}
			float blendSoftLight(float base, float blend)
			{
				return (blend < 0.5)?(2.0 * base * blend + base * base * (1.0 - 2.0 * blend)) : (sqrt(base) * (2.0 * blend - 1.0) + 2.0 * base * (1.0 - blend));
			}
			float3 blendSoftLight(float3 base, float3 blend)
			{
				return float3(blendSoftLight(base.r, blend.r), blendSoftLight(base.g, blend.g), blendSoftLight(base.b, blend.b));
			}
			float blendSubtract(float base, float blend)
			{
				return max(base - blend, 0.0);
			}
			float3 blendSubtract(float3 base, float3 blend)
			{
				return max(base - blend, 0.0);
			}
			float blendDifference(float base, float blend)
			{
				return abs(base - blend);
			}
			float3 blendDifference(float3 base, float3 blend)
			{
				return abs(base - blend);
			}
			float blendDivide(float base, float blend)
			{
				return base / max(blend, 0.0001);
			}
			float3 blendDivide(float3 base, float3 blend)
			{
				return base / max(blend, 0.0001);
			}
			float3 customBlend(float3 base, float3 blend, float blendType)
			{
				float3 ret = 0;
				switch(blendType)
				{
					case 0:
					{
						ret = blendNormal(base, blend);
						break;
					}
					case 1:
					{
						ret = blendDarken(base, blend);
						break;
					}
					case 2:
					{
						ret = blendMultiply(base, blend);
						break;
					}
					case 3:
					{
						ret = blendColorBurn(base, blend);
						break;
					}
					case 4:
					{
						ret = blendLinearBurn(base, blend);
						break;
					}
					case 5:
					{
						ret = blendLighten(base, blend);
						break;
					}
					case 6:
					{
						ret = blendScreen(base, blend);
						break;
					}
					case 7:
					{
						ret = blendColorDodge(base, blend);
						break;
					}
					case 8:
					{
						ret = blendLinearDodge(base, blend);
						break;
					}
					case 9:
					{
						ret = blendOverlay(base, blend);
						break;
					}
					case 10:
					{
						ret = blendSoftLight(base, blend);
						break;
					}
					case 11:
					{
						ret = blendHardLight(base, blend);
						break;
					}
					case 12:
					{
						ret = blendVividLight(base, blend);
						break;
					}
					case 13:
					{
						ret = blendLinearLight(base, blend);
						break;
					}
					case 14:
					{
						ret = blendPinLight(base, blend);
						break;
					}
					case 15:
					{
						ret = blendHardMix(base, blend);
						break;
					}
					case 16:
					{
						ret = blendDifference(base, blend);
						break;
					}
					case 17:
					{
						ret = blendExclusion(base, blend);
						break;
					}
					case 18:
					{
						ret = blendSubtract(base, blend);
						break;
					}
					case 19:
					{
						ret = blendDivide(base, blend);
						break;
					}
				}
				return ret;
			}
			float random(float2 p)
			{
				return frac(sin(dot(p, float2(12.9898, 78.2383))) * 43758.5453123);
			}
			float2 random2(float2 p)
			{
				return frac(sin(float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)))) * 43758.5453);
			}
			float3 random3(float3 p)
			{
				return frac(sin(float3(dot(p, float3(127.1, 311.7, 248.6)), dot(p, float3(269.5, 183.3, 423.3)), dot(p, float3(248.3, 315.9, 184.2)))) * 43758.5453);
			}
			float3 randomFloat3(float2 Seed, float maximum)
			{
				return (.5 + float3(
				frac(sin(dot(Seed.xy, float2(12.9898, 78.233))) * 43758.5453),
				frac(sin(dot(Seed.yx, float2(12.9898, 78.233))) * 43758.5453),
				frac(sin(dot(float2(Seed), float2(12.9898, 78.233))) * 43758.5453)
				) * .5) * (maximum);
			}
			float3 randomFloat3Range(float2 Seed, float Range)
			{
				return (float3(
				frac(sin(dot(Seed.xy, float2(12.9898, 78.233))) * 43758.5453),
				frac(sin(dot(Seed.yx, float2(12.9898, 78.233))) * 43758.5453),
				frac(sin(dot(float2(Seed.x * Seed.y, Seed.y + Seed.x), float2(12.9898, 78.233))) * 43758.5453)
				) * 2 - 1) * Range;
			}
			float3 randomFloat3WiggleRange(float2 Seed, float Range, float wiggleSpeed)
			{
				float3 rando = (float3(
				frac(sin(dot(Seed.xy, float2(12.9898, 78.233))) * 43758.5453),
				frac(sin(dot(Seed.yx, float2(12.9898, 78.233))) * 43758.5453),
				frac(sin(dot(float2(Seed.x * Seed.y, Seed.y + Seed.x), float2(12.9898, 78.233))) * 43758.5453)
				) * 2 - 1);
				float speed = 1 + wiggleSpeed;
				return float3(sin((_Time.x + rando.x * PI) * speed), sin((_Time.x + rando.y * PI) * speed), sin((_Time.x + rando.z * PI) * speed)) * Range;
			}
			void Unity_RandomRange_float(float2 Seed, float Min, float Max, out float Out)
			{
				float randomno = frac(sin(dot(Seed, float2(12.9898, 78.233))) * 43758.5453);
				Out = lerp(Min, Max, randomno);
			}
			void poiChannelMixer(float3 In, float3 _ChannelMixer_Red, float3 _ChannelMixer_Green, float3 _ChannelMixer_Blue, out float3 Out)
			{
				Out = float3(dot(In, _ChannelMixer_Red), dot(In, _ChannelMixer_Green), dot(In, _ChannelMixer_Blue));
			}
			void poiContrast(float3 In, float Contrast, out float3 Out)
			{
				float midpoint = pow(0.5, 2.2);
				Out = (In - midpoint) * Contrast + midpoint;
			}
			void poiInvertColors(float4 In, float4 InvertColors, out float4 Out)
			{
				Out = abs(InvertColors - In);
			}
			void poiReplaceColor(float3 In, float3 From, float3 To, float Range, float Fuzziness, out float3 Out)
			{
				float Distance = distance(From, In);
				Out = lerp(To, In, saturate((Distance - Range) / max(Fuzziness, 0.00001)));
			}
			void poiSaturation(float3 In, float Saturation, out float3 Out)
			{
				float luma = dot(In, float3(0.2126729, 0.7151522, 0.0721750));
				Out = luma.xxx + Saturation.xxx * (In - luma.xxx);
			}
			void poiDither(float4 In, float4 ScreenPosition, out float4 Out)
			{
				float2 uv = ScreenPosition.xy * _ScreenParams.xy;
				float DITHER_THRESHOLDS[16] = {
					1.0 / 17.0, 9.0 / 17.0, 3.0 / 17.0, 11.0 / 17.0,
					13.0 / 17.0, 5.0 / 17.0, 15.0 / 17.0, 7.0 / 17.0,
					4.0 / 17.0, 12.0 / 17.0, 2.0 / 17.0, 10.0 / 17.0,
					16.0 / 17.0, 8.0 / 17.0, 14.0 / 17.0, 6.0 / 17.0
				};
				uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
				Out = In - DITHER_THRESHOLDS[index];
			}
			void poiColorMask(float3 In, float3 MaskColor, float Range, float Fuzziness, out float4 Out)
			{
				float Distance = distance(MaskColor, In);
				Out = saturate(1 - (Distance - Range) / max(Fuzziness, 0.00001));
			}
			static const float Epsilon = 1e-10;
			static const float3 HCYwts = float3(0.299, 0.587, 0.114);
			static const float HCLgamma = 3;
			static const float HCLy0 = 100;
			static const float HCLmaxL = 0.530454533953517; // == exp(HCLgamma / HCLy0) - 0.5
			static const float3 wref = float3(1.0, 1.0, 1.0);
			#define TAU 6.28318531
			float3 HUEtoRGB(in float H)
			{
				float R = abs(H * 6 - 3) - 1;
				float G = 2 - abs(H * 6 - 2);
				float B = 2 - abs(H * 6 - 4);
				return saturate(float3(R, G, B));
			}
			float3 RGBtoHCV(in float3 RGB)
			{
				float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0 / 3.0) : float4(RGB.gb, 0.0, -1.0 / 3.0);
				float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
				float C = Q.x - min(Q.w, Q.y);
				float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
				return float3(H, C, Q.x);
			}
			float3 HSVtoRGB(in float3 HSV)
			{
				float3 RGB = HUEtoRGB(HSV.x);
				return ((RGB - 1) * HSV.y + 1) * HSV.z;
			}
			float3 RGBtoHSV(in float3 RGB)
			{
				float3 HCV = RGBtoHCV(RGB);
				float S = HCV.y / (HCV.z + Epsilon);
				return float3(HCV.x, S, HCV.z);
			}
			float3 HSLtoRGB(in float3 HSL)
			{
				float3 RGB = HUEtoRGB(HSL.x);
				float C = (1 - abs(2 * HSL.z - 1)) * HSL.y;
				return (RGB - 0.5) * C + HSL.z;
			}
			float3 RGBtoHSL(in float3 RGB)
			{
				float3 HCV = RGBtoHCV(RGB);
				float L = HCV.z - HCV.y * 0.5;
				float S = HCV.y / (1 - abs(L * 2 - 1) + Epsilon);
				return float3(HCV.x, S, L);
			}
			float3 hueShift(float3 color, float hueOffset)
			{
				color = RGBtoHSV(color);
				color.x = frac(hueOffset +color.x);
				return HSVtoRGB(color);
			}
			float3 hueShiftClamped(float3 color, float hueOffset, float saturationOffset = 0, float valueOffset = 0)
			{
				color = RGBtoHSV(color);
				color.x = frac(hueOffset +color.x);
				color.y = saturate(saturationOffset +color.y);
				color.z = saturate(valueOffset +color.z);
				return HSVtoRGB(color);
			}
			float3 ModifyViaHSL(float3 color, float3 HSLMod)
			{
				float3 colorHSL = RGBtoHSL(color);
				colorHSL.r = frac(colorHSL.r + HSLMod.r);
				colorHSL.g = saturate(colorHSL.g + HSLMod.g);
				colorHSL.b = saturate(colorHSL.b + HSLMod.b);
				return HSLtoRGB(colorHSL);
			}
			float3 poiSaturation(float3 In, float Saturation)
			{
				float luma = dot(In, float3(0.2126729, 0.7151522, 0.0721750));
				return luma.xxx + Saturation.xxx * (In - luma.xxx);
			}
			float xyzF(float t)
			{
				return lerp(pow(t, 1. / 3.), 7.787037 * t + 0.139731, step(t, 0.00885645));
			}
			float xyzR(float t)
			{
				return lerp(t * t * t, 0.1284185 * (t - 0.139731), step(t, 0.20689655));
			}
			float3 rgb2lch(in float3 c)
			{
				c = mul(float3x3(0.4124, 0.3576, 0.1805,
				0.2126, 0.7152, 0.0722,
				0.0193, 0.1192, 0.9505), c);
				c.x = xyzF(c.x / wref.x);
				c.y = xyzF(c.y / wref.y);
				c.z = xyzF(c.z / wref.z);
				float3 lab = float3(max(0., 116.0 * c.y - 16.0), 500.0 * (c.x - c.y), 200.0 * (c.y - c.z));
				return float3(lab.x, length(float2(lab.y, lab.z)), atan2(lab.z, lab.y));
			}
			float3 lch2rgb(in float3 c)
			{
				c = float3(c.x, cos(c.z) * c.y, sin(c.z) * c.y);
				float lg = 1. / 116. * (c.x + 16.);
				float3 xyz = float3(wref.x * xyzR(lg + 0.002 * c.y),
				wref.y * xyzR(lg),
				wref.z * xyzR(lg - 0.005 * c.z));
				float3 rgb = mul(float3x3(3.2406, -1.5372, -0.4986,
				- 0.9689, 1.8758, 0.0415,
				0.0557, -0.2040, 1.0570), xyz);
				return rgb;
			}
			float lerpAng(in float a, in float b, in float x)
			{
				float ang = fmod(fmod((a - b), TAU) + PI * 3., TAU) - PI;
				return ang * x + b;
			}
			float3 lerpLch(in float3 a, in float3 b, in float x)
			{
				float hue = lerpAng(a.z, b.z, x);
				return float3(lerp(b.xy, a.xy, x), hue);
			}
			float3 poiExpensiveColorBlend(float3 col1, float3 col2, float alpha)
			{
				return lch2rgb(lerpLch(rgb2lch(col1), rgb2lch(col2), alpha));
			}
			float4x4 poiAngleAxisRotationMatrix(float angle, float3 axis)
			{
				axis = normalize(axis);
				float s = sin(angle);
				float c = cos(angle);
				float oc = 1.0 - c;
				return float4x4(oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s, 0.0,
				oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c, oc * axis.y * axis.z - axis.x * s, 0.0,
				oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c, 0.0,
				0.0, 0.0, 0.0, 1.0);
			}
			float4x4 poiRotationMatrixFromAngles(float x, float y, float z)
			{
				float angleX = radians(x);
				float c = cos(angleX);
				float s = sin(angleX);
				float4x4 rotateXMatrix = float4x4(1, 0, 0, 0,
				0, c, -s, 0,
				0, s, c, 0,
				0, 0, 0, 1);
				float angleY = radians(y);
				c = cos(angleY);
				s = sin(angleY);
				float4x4 rotateYMatrix = float4x4(c, 0, s, 0,
				0, 1, 0, 0,
				- s, 0, c, 0,
				0, 0, 0, 1);
				float angleZ = radians(z);
				c = cos(angleZ);
				s = sin(angleZ);
				float4x4 rotateZMatrix = float4x4(c, -s, 0, 0,
				s, c, 0, 0,
				0, 0, 1, 0,
				0, 0, 0, 1);
				return mul(mul(rotateXMatrix, rotateYMatrix), rotateZMatrix);
			}
			float4x4 poiRotationMatrixFromAngles(float3 angles)
			{
				float angleX = radians(angles.x);
				float c = cos(angleX);
				float s = sin(angleX);
				float4x4 rotateXMatrix = float4x4(1, 0, 0, 0,
				0, c, -s, 0,
				0, s, c, 0,
				0, 0, 0, 1);
				float angleY = radians(angles.y);
				c = cos(angleY);
				s = sin(angleY);
				float4x4 rotateYMatrix = float4x4(c, 0, s, 0,
				0, 1, 0, 0,
				- s, 0, c, 0,
				0, 0, 0, 1);
				float angleZ = radians(angles.z);
				c = cos(angleZ);
				s = sin(angleZ);
				float4x4 rotateZMatrix = float4x4(c, -s, 0, 0,
				s, c, 0, 0,
				0, 0, 1, 0,
				0, 0, 0, 1);
				return mul(mul(rotateXMatrix, rotateYMatrix), rotateZMatrix);
			}
			float3 getCameraPosition()
			{
				#ifdef USING_STEREO_MATRICES
				return lerp(unity_StereoWorldSpaceCameraPos[0], unity_StereoWorldSpaceCameraPos[1], 0.5);
				#endif
				return _WorldSpaceCameraPos;
			}
			half2 calcScreenUVs(half4 grabPos)
			{
				half2 uv = grabPos.xy / (grabPos.w + 0.0000000001);
				#if UNITY_SINGLE_PASS_STEREO
				uv.xy *= half2(_ScreenParams.x * 2, _ScreenParams.y);
				#else
				uv.xy *= _ScreenParams.xy;
				#endif
				return uv;
			}
			float CalcMipLevel(float2 texture_coord)
			{
				float2 dx = ddx(texture_coord);
				float2 dy = ddy(texture_coord);
				float delta_max_sqr = max(dot(dx, dx), dot(dy, dy));
				return 0.5 * log2(delta_max_sqr);
			}
			float inverseLerp(float A, float B, float T)
			{
				return (T - A) / (B - A);
			}
			float inverseLerp2(float2 a, float2 b, float2 value)
			{
				float2 AB = b - a;
				float2 AV = value - a;
				return dot(AV, AB) / dot(AB, AB);
			}
			float inverseLerp3(float3 a, float3 b, float3 value)
			{
				float3 AB = b - a;
				float3 AV = value - a;
				return dot(AV, AB) / dot(AB, AB);
			}
			float inverseLerp4(float4 a, float4 b, float4 value)
			{
				float4 AB = b - a;
				float4 AV = value - a;
				return dot(AV, AB) / dot(AB, AB);
			}
			float4 quaternion_conjugate(float4 v)
			{
				return float4(
				v.x, -v.yzw
				);
			}
			float4 quaternion_mul(float4 v1, float4 v2)
			{
				float4 result1 = (v1.x * v2 + v1 * v2.x);
				float4 result2 = float4(
				- dot(v1.yzw, v2.yzw),
				cross(v1.yzw, v2.yzw)
				);
				return float4(result1 + result2);
			}
			float4 get_quaternion_from_angle(float3 axis, float angle)
			{
				float sn = sin(angle * 0.5);
				float cs = cos(angle * 0.5);
				return float4(axis * sn, cs);
			}
			float4 quaternion_from_vector(float3 inVec)
			{
				return float4(0.0, inVec);
			}
			float degree_to_radius(float degree)
			{
				return (
				degree / 180.0 * PI
				);
			}
			float3 rotate_with_quaternion(float3 inVec, float3 rotation)
			{
				float4 qx = get_quaternion_from_angle(float3(1, 0, 0), radians(rotation.x));
				float4 qy = get_quaternion_from_angle(float3(0, 1, 0), radians(rotation.y));
				float4 qz = get_quaternion_from_angle(float3(0, 0, 1), radians(rotation.z));
				#define MUL3(A, B, C) quaternion_mul(quaternion_mul((A), (B)), (C))
				float4 quaternion = normalize(MUL3(qx, qy, qz));
				float4 conjugate = quaternion_conjugate(quaternion);
				float4 inVecQ = quaternion_from_vector(inVec);
				float3 rotated = (
				MUL3(quaternion, inVecQ, conjugate)
				).yzw;
				return rotated;
			}
			float4 transform(float4 input, float4 pos, float4 rotation, float4 scale)
			{
				input.rgb *= (scale.xyz * scale.w);
				input = float4(rotate_with_quaternion(input.xyz, rotation.xyz * rotation.w) + (pos.xyz * pos.w), input.w);
				return input;
			}
			float aaBlurStep(float gradient, float edge, float blur)
			{
				float edgeMin = saturate(edge);
				float edgeMax = saturate(edge + blur * (1 - edge));
				return smoothstep(0, 1, saturate((gradient - edgeMin) / saturate(edgeMax - edgeMin + fwidth(gradient))));
			}
			float3 poiThemeColor(in PoiMods poiMods, in float3 srcColor, in float themeIndex)
			{
				if (themeIndex == 0) return srcColor;
				themeIndex -= 1;
				if (themeIndex <= 3)
				{
					return poiMods.globalColorTheme[themeIndex];
				}
				return srcColor;
			}
			float lilIsIn0to1(float f)
			{
				float value = 0.5 - abs(f - 0.5);
				return saturate(value / clamp(fwidth(value), 0.0001, 1.0));
			}
			float lilIsIn0to1(float f, float nv)
			{
				float value = 0.5 - abs(f - 0.5);
				return saturate(value / clamp(fwidth(value), 0.0001, nv));
			}
			float lilTooningNoSaturate(float value, float border)
			{
				return (value - border) / clamp(fwidth(value), 0.0001, 1.0);
			}
			float lilTooningNoSaturate(float value, float border, float blur)
			{
				float borderMin = saturate(border - blur * 0.5);
				float borderMax = saturate(border + blur * 0.5);
				return (value - borderMin) / saturate(borderMax - borderMin + fwidth(value));
			}
			float lilTooningNoSaturate(float value, float border, float blur, float borderRange)
			{
				float borderMin = saturate(border - blur * 0.5 - borderRange);
				float borderMax = saturate(border + blur * 0.5);
				return (value - borderMin) / saturate(borderMax - borderMin + fwidth(value));
			}
			float lilTooning(float value, float border)
			{
				return saturate(lilTooningNoSaturate(value, border));
			}
			float lilTooning(float value, float border, float blur)
			{
				return saturate(lilTooningNoSaturate(value, border, blur));
			}
			float lilTooning(float value, float border, float blur, float borderRange)
			{
				return saturate(lilTooningNoSaturate(value, border, blur, borderRange));
			}
			inline float4 CalculateFrustumCorrection()
			{
				float x1 = -UNITY_MATRIX_P._31 / (UNITY_MATRIX_P._11 * UNITY_MATRIX_P._34);
				float x2 = -UNITY_MATRIX_P._32 / (UNITY_MATRIX_P._22 * UNITY_MATRIX_P._34);
				return float4(x1, x2, 0, UNITY_MATRIX_P._33 / UNITY_MATRIX_P._34 + x1 * UNITY_MATRIX_P._13 + x2 * UNITY_MATRIX_P._23);
			}
			inline float CorrectedLinearEyeDepth(float z, float B)
			{
				return 1.0 / (z / UNITY_MATRIX_P._34 + B);
			}
			v2f vert(appdata v)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				v2f o;
				PoiInitStruct(v2f, o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.objectPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
				o.objNormal = v.normal;
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.tangent = UnityObjectToWorldDir(v.tangent);
				o.binormal = cross(o.normal, o.tangent) * (v.tangent.w * unity_WorldTransformParams.w);
				o.vertexColor = v.color;
				o.uv[0] = v.uv0;
				o.uv[1] = v.uv1;
				o.uv[2] = v.uv2;
				o.uv[3] = v.uv3;
				#if defined(LIGHTMAP_ON)
				o.lightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif
				#ifdef DYNAMICLIGHTMAP_ON
				o.lightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif
				o.localPos = v.vertex;
				o.worldPos = mul(unity_ObjectToWorld, o.localPos);
				float3 localOffset = float3(0, 0, 0);
				float3 worldOffset = float3(0, 0, 0);
				o.localPos.rgb += localOffset;
				o.worldPos.rgb += worldOffset;
				o.pos = UnityObjectToClipPos(o.localPos);
				#ifdef POI_PASS_OUTLINE
				#if defined(UNITY_REVERSED_Z)
				o.pos.z += _Offset_Z * - 0.01;
				#else
				o.pos.z += _Offset_Z * 0.01;
				#endif
				#endif
				o.grabPos = ComputeGrabScreenPos(o.pos);
				#ifndef FORWARD_META_PASS
				#if !defined(UNITY_PASS_SHADOWCASTER)
				UNITY_TRANSFER_SHADOW(o, o.uv[0].xy);
				#else
				TRANSFER_SHADOW_CASTER_NOPOS(o, o.pos);
				#endif
				#endif
				UNITY_TRANSFER_FOG(o, o.pos);
				if (float(0))
				{
					if (o.pos.w < _ProjectionParams.y * 1.01 && o.pos.w > 0)
					{
						o.pos.z = o.pos.z * 0.0001 + o.pos.w * 0.999;
					}
				}
				#ifdef POI_PASS_META
				o.pos = UnityMetaVertexPosition(v.vertex, v.uv1.xy, v.uv2.xy, unity_LightmapST, unity_DynamicLightmapST);
				#endif
				#if defined(GRAIN)
				float4 worldDirection;
				worldDirection.xyz = o.worldPos.xyz - _WorldSpaceCameraPos;
				worldDirection.w = dot(o.pos, CalculateFrustumCorrection());
				o.worldDirection = worldDirection;
				#endif
				return o;
			}
			void calculateGlobalThemes(inout PoiMods poiMods)
			{
				poiMods.globalColorTheme[0] = float4(1,1,1,1);
				poiMods.globalColorTheme[1] = float4(1,1,1,1);
				poiMods.globalColorTheme[2] = float4(1,1,1,1);
				poiMods.globalColorTheme[3] = float4(1,1,1,1);
			}
			float2 calculatePolarCoordinate(in PoiMesh poiMesh)
			{
				float2 delta = poiMesh.uv[float(0)] - float4(0.5,0.5,0,0);
				float radius = length(delta) * 2 * float(1);
				float angle = atan2(delta.x, delta.y) * 1.0 / 6.28 * float(1);
				return float2(radius, angle + distance(poiMesh.uv[float(0)], float4(0.5,0.5,0,0)) * float(0));
			}
			float2 MonoPanoProjection(float3 coords)
			{
				float3 normalizedCoords = normalize(coords);
				float latitude = acos(normalizedCoords.y);
				float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
				float2 sphereCoords = float2(longitude, latitude) * float2(1.0 / UNITY_PI, 1.0 / UNITY_PI);
				sphereCoords = float2(1.0, 1.0) - sphereCoords;
				return(sphereCoords + float4(0, 1 - unity_StereoEyeIndex, 1, 1.0).xy) * float4(0, 1 - unity_StereoEyeIndex, 1, 1.0).zw;
			}
			float2 StereoPanoProjection(float3 coords)
			{
				float3 normalizedCoords = normalize(coords);
				float latitude = acos(normalizedCoords.y);
				float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
				float2 sphereCoords = float2(longitude, latitude) * float2(0.5 / UNITY_PI, 1.0 / UNITY_PI);
				sphereCoords = float2(0.5, 1.0) - sphereCoords;
				return(sphereCoords + float4(0, 1 - unity_StereoEyeIndex, 1, 0.5).xy) * float4(0, 1 - unity_StereoEyeIndex, 1, 0.5).zw;
			}
			float2 calculatePanosphereUV(in PoiMesh poiMesh)
			{
				float3 viewDirection = normalize(lerp(getCameraPosition().xyz, _WorldSpaceCameraPos.xyz, float(1)) - poiMesh.worldPos.xyz) * - 1;
				return lerp(MonoPanoProjection(viewDirection), StereoPanoProjection(viewDirection), float(0));
			}
			void applyAlphaOptions(inout PoiFragData poiFragData, in PoiMesh poiMesh, in PoiCam poiCam, in PoiMods poiMods)
			{
				poiFragData.alpha = saturate(poiFragData.alpha + float(0));
				if (float(0))
				{
					float3 position = float(1) ? poiMesh.worldPos : poiMesh.objectPosition;
					poiFragData.alpha *= lerp(float(0), float(1), smoothstep(float(0), float(0), distance(position, poiCam.worldPos)));
				}
				if (float(0))
				{
					float holoRim = saturate(1 - smoothstep(min(float(0.5), float(0.5)), float(0.5), poiCam.vDotN));
					holoRim = abs(lerp(1, holoRim, float(0)));
					poiFragData.alpha *= float(0) ?1 - holoRim : holoRim;
				}
				if (float(0))
				{
					half cameraAngleMin = float(45) / 180;
					half cameraAngleMax = float(90) / 180;
					half modelAngleMin = float(45) / 180;
					half modelAngleMax = float(90) / 180;
					float3 pos = float(0) == 0 ? poiMesh.objectPosition : poiMesh.worldPos;
					half3 cameraToModelDirection = normalize(pos - getCameraPosition());
					half3 modelForwardDirection = normalize(mul(unity_ObjectToWorld, normalize(float4(0,0,1,0).rgb)));
					half cameraLookAtModel = remapClamped(cameraAngleMax, cameraAngleMin, .5 * dot(cameraToModelDirection, getCameraForward()) + .5);
					half modelLookAtCamera = remapClamped(modelAngleMax, modelAngleMin, .5 * dot(-cameraToModelDirection, modelForwardDirection) + .5);
					if (float(0) == 0)
					{
						poiFragData.alpha *= max(cameraLookAtModel, float(0));
					}
					else if (float(0) == 1)
					{
						poiFragData.alpha *= max(modelLookAtCamera, float(0));
					}
					else if (float(0) == 2)
					{
						poiFragData.alpha *= max(cameraLookAtModel * modelLookAtCamera, float(0));
					}
				}
			}
			inline half Dither8x8Bayer(int x, int y)
			{
				const half dither[ 64 ] = {
					1, 49, 13, 61, 4, 52, 16, 64,
					33, 17, 45, 29, 36, 20, 48, 32,
					9, 57, 5, 53, 12, 60, 8, 56,
					41, 25, 37, 21, 44, 28, 40, 24,
					3, 51, 15, 63, 2, 50, 14, 62,
					35, 19, 47, 31, 34, 18, 46, 30,
					11, 59, 7, 55, 10, 58, 6, 54,
					43, 27, 39, 23, 42, 26, 38, 22
				};
				int r = y * 8 + x;
				return dither[r] / 64;
			}
			half calcDither(half2 grabPos)
			{
				return Dither8x8Bayer(fmod(grabPos.x, 8), fmod(grabPos.y, 8));
			}
			void applyDithering(inout PoiFragData poiFragData, in PoiCam poiCam)
			{
				if (float(0))
				{
					poiFragData.alpha = saturate(poiFragData.alpha - (calcDither(poiCam.screenUV) * (1 - poiFragData.alpha) * float(0.1)));
				}
			}
			void ApplyAlphaToCoverage(inout PoiFragData poiFragData, in PoiMesh poiMesh)
			{
				
				if (float(0) == 1)
				{
					
					if (float(0) && float(0))
					{
						poiFragData.alpha *= 1 + max(0, CalcMipLevel(poiMesh.uv[0] * float4(0.0004882813,0.0004882813,2048,2048).zw)) * float(0.25);
						poiFragData.alpha = (poiFragData.alpha - float(0.5)) / max(fwidth(poiFragData.alpha), 0.0001) + float(0.5);
						poiFragData.alpha = saturate(poiFragData.alpha);
					}
				}
			}
			void applyVertexColor(inout PoiFragData poiFragData, PoiMesh poiMesh)
			{
				#ifndef POI_PASS_OUTLINE
				float3 vertCol = lerp(poiMesh.vertexColor.rgb, GammaToLinearSpace(poiMesh.vertexColor.rgb), float(1));
				poiFragData.baseColor *= lerp(1, vertCol, float(0));
				#endif
				poiFragData.alpha *= lerp(1, poiMesh.vertexColor.a, float(0));
			}
			#if defined(GEOM_TYPE_BRANCH) || defined(GEOM_TYPE_BRANCH_DETAIL) || defined(GEOM_TYPE_FROND) || defined(DEPTH_OF_FIELD_COC_VIEW)
			float2 decalUV(float uvNumber, float4 uv_st, float2 position, half rotation, half rotationSpeed, half2 scale, float4 scaleOffset, float depth, in PoiMesh poiMesh, in PoiCam poiCam)
			{
				scaleOffset = float4(-scaleOffset.x, scaleOffset.y, -scaleOffset.z, scaleOffset.w);
				float2 uv = poiUV(poiMesh.uv[uvNumber], uv_st) + calcParallax(depth + 1, poiCam);
				float2 decalCenter = position;
				float theta = radians(rotation + _Time.z * rotationSpeed);
				float cs = cos(theta);
				float sn = sin(theta);
				uv = float2((uv.x - decalCenter.x) * cs - (uv.y - decalCenter.y) * sn + decalCenter.x, (uv.x - decalCenter.x) * sn + (uv.y - decalCenter.y) * cs + decalCenter.y);
				uv = remap(uv, float2(0, 0) - scale / 2 + position + scaleOffset.xz, scale / 2 + position + scaleOffset.yw, float2(0, 0), float2(1, 1));
				return uv;
			}
			inline float3 decalHueShift(float enabled, float3 color, float shift, float shiftSpeed)
			{
				if (enabled)
				{
					color = hueShift(color, shift + _Time.x * shiftSpeed);
				}
				return color;
			}
			inline float applyTilingClipping(float enabled, float2 uv)
			{
				float ret = 1;
				if (!enabled)
				{
					if (uv.x > 1 || uv.y > 1 || uv.x < 0 || uv.y < 0)
					{
						ret = 0;
					}
				}
				return ret;
			}
			void applyDecals(inout PoiFragData poiFragData, in PoiMesh poiMesh, in PoiCam poiCam, in PoiMods poiMods, in PoiLight poiLight)
			{
				float decalAlpha = 1;
				float alphaOverride = 0;
				#if defined(PROP_DECALMASK) || !defined(OPTIMIZER_ENABLED)
				float4 decalMask = POI2D_SAMPLER_PAN(_DecalMask, _MainTex, poiUV(poiMesh.uv[float(0)], float4(1,1,0,0)), float4(0,0,0,0));
				#else
				float4 decalMask = 1;
				#endif
				float4 decalColor = 1;
				float2 uv = 0;
				float2 decalScale = float2(1, 1);
				float decalRotation = 0;
				float2 ddxuv = 0;
				float2 ddyuv = 0;
				float4 sideMod = 0;
				if (alphaOverride)
				{
					poiFragData.alpha *= decalAlpha;
				}
				poiFragData.baseColor = saturate(poiFragData.baseColor);
			}
			#endif
			#ifdef VIGNETTE_MASKED
			#ifdef _LIGHTINGMODE_CLOTH
			#define HARD 0
			#define LERP 1
			#define CLOTHMODE HARD
			float V_SmithGGXCorrelated(float roughness, float NoV, float NoL)
			{
				float a2 = roughness * roughness;
				float lambdaV = NoL * sqrt((NoV - a2 * NoV) * NoV + a2);
				float lambdaL = NoV * sqrt((NoL - a2 * NoL) * NoL + a2);
				float v = 0.5 / (lambdaV + lambdaL);
				return v;
			}
			float D_GGX(float roughness, float NoH)
			{
				float oneMinusNoHSquared = 1.0 - NoH * NoH;
				float a = NoH * roughness;
				float k = roughness / (oneMinusNoHSquared + a * a);
				float d = k * k * (1.0 / UNITY_PI);
				return d;
			}
			float D_Charlie(float roughness, float NoH)
			{
				float invAlpha = 1.0 / roughness;
				float cos2h = NoH * NoH;
				float sin2h = max(1.0 - cos2h, 0.0078125); // 0.0078125 = 2^(-14/2), so sin2h^2 > 0 in fp16
				return (2.0 + invAlpha) * pow(sin2h, invAlpha * 0.5) / (2.0 * UNITY_PI);
			}
			float V_Neubelt(float NoV, float NoL)
			{
				return 1.0 / (4.0 * (NoL + NoV - NoL * NoV));
			}
			float Distribution(float roughness, float NoH, float cloth)
			{
				#if CLOTHMODE == LERP
				return lerp(GGXTerm(roughness, NoH), D_Charlie(roughness, NoH), cloth);
				#elif CLOTHMODE == HARD
				return cloth <= 0.5 ? GGXTerm(roughness, NoH) : D_Charlie(roughness, NoH);
				#endif
			}
			float Visibility(float roughness, float NoV, float NoL, float cloth)
			{
				#if CLOTHMODE == LERP
				return lerp(V_SmithGGXCorrelated(roughness, NoV, NoL), V_Neubelt(NoV, NoL), cloth);
				#elif CLOTHMODE == HARD
				return cloth <= 0.5 ? V_SmithGGXCorrelated(roughness, NoV, NoL) : V_Neubelt(NoV, NoL);
				#endif
			}
			float F_Schlick(float3 f0, float f90, float VoH)
			{
				return f0 + (f90 - f0) * pow(1.0 - VoH, 5);
			}
			float F_Schlick(float3 f0, float VoH)
			{
				float f = pow(1.0 - VoH, 5.0);
				return f + f0 * (1.0 - f);
			}
			float Fresnel(float3 f0, float LoH)
			{
				float f90 = saturate(dot(f0, float(50.0 * 0.33).xxx));
				return F_Schlick(f0, f90, LoH);
			}
			float Fd_Burley(float roughness, float NoV, float NoL, float LoH)
			{
				float f90 = 0.5 + 2.0 * roughness * LoH * LoH;
				float lightScatter = F_Schlick(1.0, f90, NoL);
				float viewScatter = F_Schlick(1.0, f90, NoV);
				return lightScatter * viewScatter;
			}
			float Fd_Wrap(float NoL, float w)
			{
				return saturate((NoL + w) / pow(1.0 + w, 2));
			}
			float4 SampleDFG(float NoV, float perceptualRoughness)
			{
				return _ClothDFG.Sample(sampler_ClothDFG, float3(NoV, perceptualRoughness, 0));
			}
			float3 EnvBRDF(float2 dfg, float3 f0)
			{
				return f0 * dfg.x + dfg.y;
			}
			float3 EnvBRDFMultiscatter(float3 dfg, float3 f0, float cloth)
			{
				#if CLOTHMODE == LERP
				return lerp(lerp(dfg.xxx, dfg.yyy, f0), f0 * dfg.z, cloth);
				#elif CLOTHMODE == HARD
				return cloth <= 0.5 ? lerp(dfg.xxx, dfg.yyy, f0) : f0 * dfg.z;
				#endif
			}
			float3 EnvBRDFEnergyCompensation(float3 dfg, float3 f0, float cloth)
			{
				#if CLOTHMODE == LERP
				return lerp(1.0 + f0 * (1.0 / dfg.y - 1.0), 1, cloth);
				#elif CLOTHMODE == HARD
				return cloth <= 0.5 ? 1.0 + f0 * (1.0 / dfg.y - 1.0) : 1;
				#endif
			}
			float ClothMetallic(float cloth)
			{
				#if CLOTHMODE == LERP
				return cloth;
				#elif CLOTHMODE == HARD
				return cloth <= 0.5 ? 1 : 0;
				#endif
			}
			float3 Specular(float roughness, PoiLight poiLight, float f0, float3 normal, float cloth)
			{
				float NoL = poiLight.nDotLSaturated;
				float NoH = poiLight.nDotH;
				float LoH = poiLight.lDotH;
				float NoV = poiLight.nDotV;
				float D = Distribution(roughness, NoH, cloth);
				float V = Visibility(roughness, NoV, NoL, cloth);
				float3 F = Fresnel(f0, LoH);
				return (D * V) * F;
			}
			float3 getBoxProjection(float3 direction, float3 position, float4 cubemapPosition, float3 boxMin, float3 boxMax)
			{
				#if UNITY_SPECCUBE_BOX_PROJECTION
				if (cubemapPosition.w > 0)
				{
					float3 factors = ((direction > 0 ? boxMax : boxMin) - position) / direction;
					float scalar = min(min(factors.x, factors.y), factors.z);
					direction = direction * scalar + (position - cubemapPosition.xyz);
				}
				#endif
				return direction;
			}
			float SpecularAO(float NoV, float ao, float roughness)
			{
				return clamp(pow(NoV + ao, exp2(-16.0 * roughness - 1.0)) - 1.0 + ao, 0.0, 1.0);
			}
			float3 IndirectSpecular(float3 dfg, float roughness, float occlusion, float energyCompensation, float cloth, float3 indirectDiffuse, float f0, PoiLight poiLight, PoiFragData poiFragData, PoiCam poiCam, PoiMesh poiMesh)
			{
				float3 normal = poiMesh.normals[1];
				float3 reflDir = reflect(-poiCam.viewDir, normal);
				Unity_GlossyEnvironmentData envData;
				envData.roughness = roughness;
				envData.reflUVW = getBoxProjection(reflDir, poiMesh.worldPos, unity_SpecCube0_ProbePosition,
				unity_SpecCube0_BoxMin.xyz, unity_SpecCube0_BoxMax.xyz);
				float3 probe0 = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData);
				float3 indirectSpecular = probe0;
				#if UNITY_SPECCUBE_BLENDING
				
				if (unity_SpecCube0_BoxMin.w < 0.99999)
				{
					envData.reflUVW = getBoxProjection(reflDir, poiMesh.worldPos, unity_SpecCube1_ProbePosition, unity_SpecCube1_BoxMin.xyz, unity_SpecCube1_BoxMax.xyz);
					float3 probe1 = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1, unity_SpecCube0), unity_SpecCube1_HDR, envData);
					indirectSpecular = lerp(probe1, probe0, unity_SpecCube0_BoxMin.w);
				}
				#endif
				float horizon = min(1 + dot(reflDir, normal), 1);
				indirectSpecular = indirectSpecular * horizon * horizon * energyCompensation * EnvBRDFMultiscatter(dfg, f0, cloth);
				indirectSpecular *= SpecularAO(poiLight.nDotV, occlusion, roughness);
				return indirectSpecular;
			};
			#undef LERP
			#undef HARD
			#undef CLOTHMODE
			#endif
			float _LightingWrappedWrap;
			float _LightingWrappedNormalization;
			float RTWrapFunc(in float dt, in float w, in float norm)
			{
				float cw = saturate(w);
				float o = (dt + cw) / ((1.0 + cw) * (1.0 + cw * norm));
				float flt = 1.0 - 0.85 * norm;
				if (w > 1.0)
				{
					o = lerp(o, flt, w - 1.0);
				}
				return o;
			}
			float3 GreenWrapSH(float fA) // Greens unoptimized and non-normalized
			{
				float fAs = saturate(fA);
				float4 t = float4(fA + 1, fAs - 1, fA - 2, fAs + 1); // DJL edit: allow wrapping to L0-only at w=2
				return float3(t.x, -t.z * t.x / 3, 0.25 * t.y * t.y * t.w);
			}
			float3 GreenWrapSHOpt(float fW) // optimised and normalized https://blog.selfshadow.com/2012/01/07/righting-wrap-part-2/
			{
				const float4 t0 = float4(0.0, 1.0 / 4.0, -1.0 / 3.0, -1.0 / 2.0);
				const float4 t1 = float4(1.0, 2.0 / 3.0, 1.0 / 4.0, 0.0);
				float3 fWs = float3(fW, fW, saturate(fW)); // DJL edit: allow wrapping to L0-only at w=2
				float3 r;
				r.xyz = t0.xxy * fWs + t0.xzw;
				r.xyz = r.xyz * fWs + t1.xyz;
				return r;
			}
			float3 ShadeSH9_wrapped(float3 normal, float wrap)
			{
				float3 x0, x1, x2;
				float3 conv = lerp(GreenWrapSH(wrap), GreenWrapSHOpt(wrap), float(0)); // Should try optimizing this...
				conv *= float3(1, 1.5, 4); // Undo pre-applied cosine convolution by using the inverse
				x0 = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
				float3 L2_0 = float3(unity_SHBr.z, unity_SHBg.z, unity_SHBb.z) / - 3.0;
				x0 -= L2_0;
				x1.r = dot(unity_SHAr.xyz, normal);
				x1.g = dot(unity_SHAg.xyz, normal);
				x1.b = dot(unity_SHAb.xyz, normal);
				float4 vB = normal.xyzz * normal.yzzx;
				x2.r = dot(unity_SHBr, vB);
				x2.g = dot(unity_SHBg, vB);
				x2.b = dot(unity_SHBb, vB);
				float vC = normal.x * normal.x - normal.y * normal.y;
				x2 += unity_SHC.rgb * vC;
				x2 += L2_0;
				return x0 * conv.x + x1 * conv.y + x2 * conv.z;
			}
			float3 GetSHDirectionL1()
			{
				return Unity_SafeNormalize((unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz));
			}
			half3 GetSHMaxL1()
			{
				float3 maxDirection = GetSHDirectionL1();
				return ShadeSH9_wrapped(maxDirection, 0);
			}
			#ifdef _LIGHTINGMODE_SHADEMAP
			void applyShadeMapping(inout PoiFragData poiFragData, PoiMesh poiMesh, inout PoiLight poiLight)
			{
				float MainColorFeatherStep = float(0.5) - float(0.0001);
				float firstColorFeatherStep = float(0) - float(0.0001);
				#if defined(PROP_1ST_SHADEMAP) || !defined(OPTIMIZER_ENABLED)
				float4 firstShadeMap = POI2D_SAMPLER_PAN(_1st_ShadeMap, _MainTex, poiUV(poiMesh.uv[float(0)], float4(1,1,0,0)), float4(0,0,0,0));
				#else
				float4 firstShadeMap = float4(1, 1, 1, 1);
				#endif
				firstShadeMap = lerp(firstShadeMap, float4(poiFragData.baseColor, 1), float(0));
				#if defined(PROP_2ND_SHADEMAP) || !defined(OPTIMIZER_ENABLED)
				float4 secondShadeMap = POI2D_SAMPLER_PAN(_2nd_ShadeMap, _MainTex, poiUV(poiMesh.uv[float(0)], float4(1,1,0,0)), float4(0,0,0,0));
				#else
				float4 secondShadeMap = float4(1, 1, 1, 1);
				#endif
				secondShadeMap = lerp(secondShadeMap, firstShadeMap, float(0));
				firstShadeMap.rgb *= float4(1,1,1,1).rgb; //* lighColor
				secondShadeMap.rgb *= float4(1,1,1,1).rgb; //* LightColor;
				float shadowMask = 1;
				shadowMask *= float(0) ?(float(0) ?(1.0 - firstShadeMap.a) : firstShadeMap.a) : 1;
				shadowMask *= float(0) ?(float(0) ?(1.0 - secondShadeMap.a) : secondShadeMap.a) : 1;
				float mainShadowMask = saturate(1 - ((poiLight.lightMap) - MainColorFeatherStep) / (float(0.5) - MainColorFeatherStep) * (shadowMask));
				float firstSecondShadowMask = saturate(1 - ((poiLight.lightMap) - firstColorFeatherStep) / (float(0) - firstColorFeatherStep) * (shadowMask));
				mainShadowMask *= poiLight.shadowMask * float(1);
				firstSecondShadowMask *= poiLight.shadowMask * float(1);
				if (float(0) == 0)
				{
					poiFragData.baseColor.rgb = lerp(poiFragData.baseColor.rgb, lerp(firstShadeMap.rgb, secondShadeMap.rgb, firstSecondShadowMask), mainShadowMask);
				}
				else
				{
					poiFragData.baseColor.rgb *= lerp(1, lerp(firstShadeMap.rgb, secondShadeMap.rgb, firstSecondShadowMask), mainShadowMask);
				}
				poiLight.rampedLightMap = 1 - mainShadowMask;
			}
			#endif
			void ApplySubtractiveLighting(inout UnityIndirect indirectLight)
			{
				#if SUBTRACTIVE_LIGHTING
				poiLight.attenuation = FadeShadows(lerp(1, poiLight.attenuation, _AttenuationMultiplier));
				float ndotl = saturate(dot(i.normal, _WorldSpaceLightPos0.xyz));
				float3 shadowedLightEstimate = ndotl * (1 - poiLight.attenuation) * _LightColor0.rgb;
				float3 subtractedLight = indirectLight.diffuse - shadowedLightEstimate;
				subtractedLight = max(subtractedLight, unity_ShadowColor.rgb);
				subtractedLight = lerp(subtractedLight, indirectLight.diffuse, _LightShadowData.x);
				indirectLight.diffuse = min(subtractedLight, indirectLight.diffuse);
				#endif
			}
			UnityIndirect CreateIndirectLight(in PoiMesh poiMesh, in PoiCam poiCam, in PoiLight poiLight)
			{
				UnityIndirect indirectLight;
				indirectLight.diffuse = 0;
				indirectLight.specular = 0;
				#if defined(LIGHTMAP_ON)
				indirectLight.diffuse = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, poiMesh.lightmapUV.xy));
				#if defined(DIRLIGHTMAP_COMBINED)
				float4 lightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(
				unity_LightmapInd, unity_Lightmap, poiMesh.lightmapUV.xy
				);
				indirectLight.diffuse = DecodeDirectionalLightmap(
				indirectLight.diffuse, lightmapDirection, poiMesh.normals[1]
				);
				#endif
				ApplySubtractiveLighting(indirectLight);
				#endif
				#if defined(DYNAMICLIGHTMAP_ON)
				float3 dynamicLightDiffuse = DecodeRealtimeLightmap(
				UNITY_SAMPLE_TEX2D(unity_DynamicLightmap, poiMesh.lightmapUV.zw)
				);
				#if defined(DIRLIGHTMAP_COMBINED)
				float4 dynamicLightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(
				unity_DynamicDirectionality, unity_DynamicLightmap,
				poiMesh.lightmapUV.zw
				);
				indirectLight.diffuse += DecodeDirectionalLightmap(
				dynamicLightDiffuse, dynamicLightmapDirection, poiMesh.normals[1]
				);
				#else
				indirectLight.diffuse += dynamicLightDiffuse;
				#endif
				#endif
				#if !defined(LIGHTMAP_ON) && !defined(DYNAMICLIGHTMAP_ON)
				#if UNITY_LIGHT_PROBE_PROXY_VOLUME
				if (unity_ProbeVolumeParams.x == 1)
				{
					indirectLight.diffuse = SHEvalLinearL0L1_SampleProbeVolume(
					float4(poiMesh.normals[1], 1), poiMesh.worldPos
					);
					indirectLight.diffuse = max(0, indirectLight.diffuse);
					#if defined(UNITY_COLORSPACE_GAMMA)
					indirectLight.diffuse = LinearToGammaSpace(indirectLight.diffuse);
					#endif
				}
				else
				{
					indirectLight.diffuse += max(0, ShadeSH9(float4(poiMesh.normals[1], 1)));
				}
				#else
				indirectLight.diffuse += max(0, ShadeSH9(float4(poiMesh.normals[1], 1)));
				#endif
				#endif
				indirectLight.diffuse *= poiLight.occlusion;
				return indirectLight;
			}
			void calculateShading(inout PoiLight poiLight, inout PoiFragData poiFragData, in PoiMesh poiMesh, in PoiCam poiCam)
			{
				#ifdef UNITY_PASS_FORWARDBASE
				float shadowStrength = float(1) * poiLight.shadowMask;
				#ifdef POI_PASS_OUTLINE
				shadowStrength = lerp(0, shadowStrength, _OutlineShadowStrength);
				#endif
				#ifdef _LIGHTINGMODE_FLAT
				poiLight.finalLighting = poiLight.directColor;
				poiLight.rampedLightMap = poiLight.nDotLSaturated;
				#endif
				#ifdef _LIGHTINGMODE_MULTILAYER_MATH
				float4 lns = float4(1, 1, 1, 1);
				lns.x = lilTooningNoSaturate(poiLight.lightMap, float(0.5), float(0.1));
				lns.y = lilTooningNoSaturate(poiLight.lightMap, float(0.5), float(0.3));
				lns.z = lilTooningNoSaturate(poiLight.lightMap, float(0.25), float(0.1));
				lns.w = lilTooningNoSaturate(poiLight.lightMap, float(0.5), float(0.1), float(0));
				lns = saturate(lns);
				float3 indirectColor = 1;
				if (float4(0.4479884,0.5225216,0.6920712,1).a > 0)
				{
					#if defined(PROP_SHADOWCOLORTEX) || !defined(OPTIMIZER_ENABLED)
					float4 shadowColorTex = POI2D_SAMPLER_PAN(_ShadowColorTex, _MainTex, poiUV(poiMesh.uv[float(0)], float4(1,1,0,0)), float4(0,0,0,0));
					#else
					float4 shadowColorTex = float4(1, 1, 1, 1);
					#endif
					indirectColor = lerp(float3(1, 1, 1), shadowColorTex.rgb, shadowColorTex.a) * float4(0.4479884,0.5225216,0.6920712,1).rgb;
				}
				if (float4(0,0,0,0).a > 0)
				{
					#if defined(PROP_SHADOW2NDCOLORTEX) || !defined(OPTIMIZER_ENABLED)
					float4 shadow2ndColorTex = POI2D_SAMPLER_PAN(_Shadow2ndColorTex, _MainTex, poiUV(poiMesh.uv[float(0)], float4(1,1,0,0)), float4(0,0,0,0));
					#else
					float4 shadow2ndColorTex = float4(1, 1, 1, 1);
					#endif
					shadow2ndColorTex.rgb = lerp(float3(1, 1, 1), shadow2ndColorTex.rgb, shadow2ndColorTex.a) * float4(0,0,0,0).rgb;
					lns.y = float4(0,0,0,0).a - lns.y * float4(0,0,0,0).a;
					indirectColor = lerp(indirectColor, shadow2ndColorTex.rgb, lns.y);
				}
				if (float4(0,0,0,0).a > 0)
				{
					#if defined(PROP_SHADOW3RDCOLORTEX) || !defined(OPTIMIZER_ENABLED)
					float4 shadow3rdColorTex = POI2D_SAMPLER_PAN(_Shadow3rdColorTex, _MainTex, poiUV(poiMesh.uv[float(0)], float4(1,1,0,0)), float4(0,0,0,0));
					#else
					float4 shadow3rdColorTex = float4(1, 1, 1, 1);
					#endif
					shadow3rdColorTex.rgb = lerp(float3(1, 1, 1), shadow3rdColorTex.rgb, shadow3rdColorTex.a) * float4(0,0,0,0).rgb;
					lns.z = float4(0,0,0,0).a - lns.z * float4(0,0,0,0).a;
					indirectColor = lerp(indirectColor, shadow3rdColorTex.rgb, lns.z);
				}
				poiLight.rampedLightMap = lns.x;
				indirectColor = lerp(indirectColor, 1, lns.w * float4(1,0,0,1).rgb);
				indirectColor = indirectColor * lerp(poiLight.indirectColor, poiLight.directColor, float(0));
				indirectColor = lerp(poiLight.directColor, indirectColor, float(1) * poiLight.shadowMask);
				poiLight.finalLighting = lerp(indirectColor, poiLight.directColor, lns.x);
				#endif
				#ifdef _LIGHTINGMODE_SHADEMAP
				poiLight.finalLighting = poiLight.directColor;
				#endif
				#ifdef _LIGHTINGMODE_REALISTIC
				UnityLight light;
				light.dir = poiLight.direction;
				light.color = saturate(_LightColor0.rgb * lerp(1, poiLight.attenuation, poiLight.attenuationStrength) * poiLight.detailShadow);
				light.ndotl = poiLight.nDotLSaturated;
				poiLight.rampedLightMap = poiLight.nDotLSaturated;
				poiLight.finalLighting = max(UNITY_BRDF_PBS(1, 0, 0, 0, poiMesh.normals[1], poiCam.viewDir, light, CreateIndirectLight(poiMesh, poiCam, poiLight)).xyz, float(0));
				#endif
				#ifdef _LIGHTINGMODE_CLOTH
				#if defined(PROP_MOCHIEMETALLICMAP) || !defined(OPTIMIZER_ENABLED)
				float4 clothmapsample = POI2D_MAINTEX_SAMPLER_PAN_INLINED(_ClothMetallicSmoothnessMap, poiMesh);
				float roughness = 1 - (clothmapsample.a * float(0.5));
				float reflectance = float(0.5) * clothmapsample.b;
				float clothmask = clothmapsample.g;
				float metallic = pow(clothmapsample.r * _ClothMetallic, 2) * ClothMetallic(clothmask);
				roughness = float(0) == 1 ? 1 - roughness : roughness;
				#else
				float roughness = 1 - (float(0.5));
				float metallic = pow(_ClothMetallic, 2);
				float reflectance = float(0.5);
				float clothmask = 1;
				#endif
				float perceptualRoughness = pow(roughness, 2);
				float clampedRoughness = max(0.002, perceptualRoughness);
				float f0 = 0.16 * reflectance * reflectance * (1 - metallic) + poiFragData.baseColor * metallic;
				float3 fresnel = Fresnel(f0, poiLight.nDotV);
				float3 dfg = SampleDFG(poiLight.nDotV, perceptualRoughness);
				float energyCompensation = EnvBRDFEnergyCompensation(dfg, f0, clothmask);
				poiLight.finalLighting = Fd_Burley(perceptualRoughness, poiLight.nDotV, poiLight.nDotLSaturated, poiLight.lDotH);
				poiLight.finalLighting *= _LightColor0 * poiLight.attenuation * poiLight.nDotLSaturated;
				float3 specular = max(0, Specular(clampedRoughness, poiLight, f0, poiMesh.normals[1], clothmask) * poiLight.finalLighting * energyCompensation * UNITY_PI); // (D * V) * F
				float3 L0 = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
				float3 indirectDiffuse;
				indirectDiffuse.r = shEvaluateDiffuseL1Geomerics_local(L0.r, unity_SHAr.xyz, poiMesh.normals[1]);
				indirectDiffuse.g = shEvaluateDiffuseL1Geomerics_local(L0.g, unity_SHAg.xyz, poiMesh.normals[1]);
				indirectDiffuse.b = shEvaluateDiffuseL1Geomerics_local(L0.b, unity_SHAb.xyz, poiMesh.normals[1]);
				indirectDiffuse = max(0, indirectDiffuse);
				float3 indirectSpecular = IndirectSpecular(dfg, roughness, poiLight.occlusion, energyCompensation, clothmask, indirectDiffuse, f0, poiLight, poiFragData, poiCam, poiMesh);
				poiLight.finalLightAdd += max(0, specular + indirectSpecular);
				poiLight.finalLighting += indirectDiffuse * poiLight.occlusion;
				poiFragData.baseColor.xyz *= (1 - metallic);
				#endif
				#ifdef _LIGHTINGMODE_WRAPPED
				#define GREYSCALE_VECTOR float3(.33333, .33333, .33333)
				float3 directColor = _LightColor0.rgb * saturate(RTWrapFunc(poiLight.nDotL, float(0), float(0))) * lerp(1, poiLight.attenuation, poiLight.attenuationStrength);
				float3 indirectColor = ShadeSH9_wrapped(poiMesh.normals[float(0)], float(0)) * poiLight.occlusion;
				float3 ShadeSH9Plus_2 = GetSHMaxL1();
				float bw_topDirectLighting_2 = dot(_LightColor0.rgb, GREYSCALE_VECTOR);
				float bw_directLighting = dot(directColor, GREYSCALE_VECTOR);
				float bw_indirectLighting = dot(indirectColor, GREYSCALE_VECTOR);
				float bw_topIndirectLighting = dot(ShadeSH9Plus_2, GREYSCALE_VECTOR);
				poiLight.lightMap = smoothstep(0, bw_topIndirectLighting + bw_topDirectLighting_2, bw_indirectLighting + bw_directLighting) * poiLight.detailShadow;
				poiLight.rampedLightMap = saturate((poiLight.lightMap - (1 - float(0.5))) / saturate((1 - float(0)) - (1 - float(0.5)) + fwidth(poiLight.lightMap)));
				float3 mathRamp = lerp(float3(1, 1, 1), saturate(lerp((float4(1,1,1,1) * lerp(indirectColor, 1, float(0))), float3(1, 1, 1), saturate(poiLight.rampedLightMap))), float(1));
				float3 finalWrap = directColor + indirectColor;
				if (float(1))
				{
					finalWrap = clamp(finalWrap, float(0), float(1));
				}
				else
				{
					finalWrap = max(finalWrap, float(0));
				}
				poiLight.finalLighting = finalWrap * saturate(mathRamp + 1 - float(1));
				#endif
				#ifdef _LIGHTINGMODE_SKIN
				float3 ambientNormalWorld = poiMesh.normals[1];//aTangentToWorld(s, s.blurredNormalTangent);
				poiLight.rampedLightMap = poiLight.nDotLSaturated;
				float subsurface = 1;
				float skinScattering = saturate(subsurface * float(1) * 2);
				half3 absorption = exp((1.0h - subsurface) * float4(-8,-40,-64,0).rgb);
				absorption *= saturate(poiFragData.baseColor * unity_ColorSpaceDouble.rgb);
				ambientNormalWorld = normalize(lerp(poiMesh.normals[1], ambientNormalWorld, float(0.7)));
				float ndlBlur = dot(poiMesh.normals[1], poiLight.direction) * 0.5h + 0.5h;
				float lumi = dot(poiLight.directColor, half3(0.2126h, 0.7152h, 0.0722h));
				float4 sssLookupUv = float4(ndlBlur, skinScattering * lumi, 0.0f, 0.0f);
				half3 sss = poiLight.lightMap * poiLight.attenuation * tex2Dlod(_SkinLUT, sssLookupUv).rgb;
				poiLight.finalLighting = min(lerp(poiLight.indirectColor * float4(1,1,1,1), float4(1,1,1,1), float(0)) + (sss * poiLight.directColor), poiLight.directColor);
				#endif
				#ifdef _LIGHTINGMODE_SDF
				#endif
				#endif
				#ifdef UNITY_PASS_FORWARDADD
				if (float(1) == 0)
				{
					poiLight.rampedLightMap = max(0, poiLight.nDotL);
					poiLight.finalLighting = poiLight.directColor * poiLight.attenuation * max(0, poiLight.nDotL) * poiLight.detailShadow * poiLight.additiveShadow;
				}
				if (float(1) == 1)
				{
					#if defined(POINT_COOKIE) || defined(DIRECTIONAL_COOKIE)
					float passthrough = 0;
					#else
					float passthrough = float(0.5);
					#endif
					if (float(0.5) == float(0)) float(0.5) += 0.001;
					poiLight.rampedLightMap = smoothstep(float(0.5), float(0), 1 - (.5 * poiLight.nDotL + .5));
					#if defined(POINT) || defined(SPOT)
					poiLight.finalLighting = lerp(poiLight.directColor * max(poiLight.additiveShadow, passthrough), poiLight.indirectColor, smoothstep(float(0), float(0.5), 1 - (.5 * poiLight.nDotL + .5))) * poiLight.attenuation * poiLight.detailShadow;
					#else
					poiLight.finalLighting = lerp(poiLight.directColor * max(poiLight.attenuation, passthrough), poiLight.indirectColor, smoothstep(float(0), float(0.5), 1 - (.5 * poiLight.nDotL + .5))) * poiLight.detailShadow;
					#endif
				}
				if (float(1) == 2)
				{
				}
				#endif
				#if defined(VERTEXLIGHT_ON) && defined(POI_VERTEXLIGHT_ON)
				float3 vertexLighting = float3(0, 0, 0);
				for (int index = 0; index < 4; index++)
				{
					if (float(1) == 0)
					{
						vertexLighting += poiLight.vColor[index] * poiLight.vAttenuationDotNL[index] * poiLight.detailShadow; // Realistic
					}
					if (float(1) == 1) // Toon
					{
						vertexLighting += lerp(poiLight.vColor[index] * poiLight.vAttenuation[index], poiLight.vColor[index] * float(0.5) * poiLight.vAttenuation[index], smoothstep(float(0), float(0.5), .5 * poiLight.vDotNL[index] + .5)) * poiLight.detailShadow;
					}
				}
				float3 mixedLight = poiLight.finalLighting;
				poiLight.finalLighting = vertexLighting + poiLight.finalLighting;
				#endif
			}
			#endif
			void blendMatcap(inout PoiLight poiLight, inout PoiFragData poiFragData, float add, float lightAdd, float multiply, float replace, float mixed, float4 matcapColor, float matcapMask, float emissionStrength, float matcapLightMask
			#ifdef POI_BLACKLIGHT
			, uint blackLightMaskIndex
			#endif
			)
			{
				if (matcapLightMask)
				{
					matcapMask *= lerp(1, poiLight.rampedLightMap, matcapLightMask);
				}
				#ifdef POI_BLACKLIGHT
				if (blackLightMaskIndex != 4)
				{
					matcapMask *= blackLightMask[blackLightMaskIndex];
				}
				#endif
				poiFragData.baseColor.rgb = lerp(poiFragData.baseColor.rgb, matcapColor.rgb, replace * matcapMask * matcapColor.a * .999999);
				poiFragData.baseColor.rgb *= lerp(1, matcapColor.rgb, multiply * matcapMask * matcapColor.a);
				poiFragData.baseColor.rgb += matcapColor.rgb * add * matcapMask * matcapColor.a;
				poiLight.finalLightAdd += matcapColor.rgb * lightAdd * matcapMask * matcapColor.a;
				poiFragData.baseColor.rgb = lerp(poiFragData.baseColor.rgb, poiFragData.baseColor.rgb + poiFragData.baseColor.rgb * matcapColor.rgb, mixed * matcapMask * matcapColor.a);
				poiFragData.emission += matcapColor.rgb * emissionStrength * matcapMask * matcapColor.a;
			}
			#if defined(POI_MATCAP0) || defined(COLOR_GRADING_HDR_3D)
			void applyMatcap(inout PoiFragData poiFragData, in PoiCam poiCam, in PoiMesh poiMesh, inout PoiLight poiLight, in PoiMods poiMods)
			{
				float4 matcap = 0;
				float matcapMask = 0;
				float4 matcap2 = 0;
				float matcap2Mask = 0;
				float2 matcapUV = 0;
			}
			#endif
			#if defined(MOCHIE_PBR) || defined(POI_CLEARCOAT)
			float GSAA_Filament(float3 worldNormal, float perceptualRoughness, float gsaaVariance, float gsaaThreshold)
			{
				float3 du = ddx(worldNormal);
				float3 dv = ddy(worldNormal);
				float variance = gsaaVariance * (dot(du, du) + dot(dv, dv));
				float roughness = perceptualRoughness * perceptualRoughness;
				float kernelRoughness = min(2.0 * variance, gsaaThreshold);
				float squareRoughness = saturate(roughness * roughness + kernelRoughness);
				return sqrt(sqrt(squareRoughness));
			}
			bool SceneHasReflections()
			{
				float width, height;
				unity_SpecCube0.GetDimensions(width, height);
				return !(width * height < 2);
			}
			float3 GetWorldReflections(float3 reflDir, float3 worldPos, float roughness)
			{
				float3 baseReflDir = reflDir;
				reflDir = BoxProjection(reflDir, worldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
				float4 envSample0 = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflDir, roughness * UNITY_SPECCUBE_LOD_STEPS);
				float3 p0 = DecodeHDR(envSample0, unity_SpecCube0_HDR);
				float interpolator = unity_SpecCube0_BoxMin.w;
				
				if (interpolator < 0.99999)
				{
					float3 refDirBlend = BoxProjection(baseReflDir, worldPos, unity_SpecCube1_ProbePosition, unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax);
					float4 envSample1 = UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(unity_SpecCube1, unity_SpecCube0, refDirBlend, roughness * UNITY_SPECCUBE_LOD_STEPS);
					float3 p1 = DecodeHDR(envSample1, unity_SpecCube1_HDR);
					p0 = lerp(p1, p0, interpolator);
				}
				return p0;
			}
			float3 GetReflections(in PoiCam poiCam, in PoiLight pl, in PoiMesh poiMesh, float roughness, float ForceFallback, float LightFallback, samplerCUBE reflectionCube, float3 reflectionDir)
			{
				float3 reflections = 0;
				float3 lighting = pl.finalLighting;
				if (ForceFallback == 0)
				{
					
					if (SceneHasReflections())
					{
						#ifdef UNITY_PASS_FORWARDBASE
						reflections = GetWorldReflections(reflectionDir, poiMesh.worldPos.xyz, roughness);
						#endif
					}
					else
					{
						#ifdef UNITY_PASS_FORWARDBASE
						reflections = texCUBElod(reflectionCube, float4(reflectionDir, roughness * UNITY_SPECCUBE_LOD_STEPS));
						reflections = DecodeHDR(float4(reflections, 1), _MochieReflCube_HDR) * lerp(1, pl.finalLighting, LightFallback);
						#endif
						#ifdef POI_PASS_ADD
						if (LightFallback)
						{
							reflections = texCUBElod(reflectionCube, float4(reflectionDir, roughness * UNITY_SPECCUBE_LOD_STEPS));
							reflections = DecodeHDR(float4(reflections, 1), _MochieReflCube_HDR) * pl.finalLighting;
						}
						#endif
					}
				}
				else
				{
					#ifdef UNITY_PASS_FORWARDBASE
					reflections = texCUBElod(reflectionCube, float4(reflectionDir, roughness * UNITY_SPECCUBE_LOD_STEPS));
					reflections = DecodeHDR(float4(reflections, 1), _MochieReflCube_HDR) * lerp(1, pl.finalLighting, LightFallback);
					#endif
					#ifdef POI_PASS_ADD
					if (LightFallback)
					{
						reflections = texCUBElod(reflectionCube, float4(reflectionDir, roughness * UNITY_SPECCUBE_LOD_STEPS));
						reflections = DecodeHDR(float4(reflections, 1), _MochieReflCube_HDR) * pl.finalLighting;
					}
					#endif
				}
				reflections *= pl.occlusion;
				return reflections;
			}
			float GetGGXTerm(float nDotL, float nDotV, float nDotH, float roughness)
			{
				float visibilityTerm = 0;
				if (nDotL > 0)
				{
					float rough = roughness;
					float rough2 = roughness * roughness;
					float lambdaV = nDotL * (nDotV * (1 - rough) + rough);
					float lambdaL = nDotV * (nDotL * (1 - rough) + rough);
					visibilityTerm = 0.5f / (lambdaV + lambdaL + 1e-5f);
					float d = (nDotH * rough2 - nDotH) * nDotH + 1.0f;
					float dotTerm = UNITY_INV_PI * rough2 / (d * d + 1e-7f);
					visibilityTerm *= dotTerm * UNITY_PI;
				}
				return visibilityTerm;
			}
			void GetSpecFresTerm(float nDotL, float nDotV, float nDotH, float lDotH, inout float3 specularTerm, inout float3 fresnelTerm, float3 specCol, float roughness)
			{
				specularTerm = GetGGXTerm(nDotL, nDotV, nDotH, roughness);
				fresnelTerm = FresnelTerm(specCol, lDotH);
				specularTerm = max(0, specularTerm * max(0.00001, nDotL));
			}
			float GetRoughness(float smoothness)
			{
				float rough = 1 - smoothness;
				rough *= 1.7 - 0.7 * rough;
				return rough;
			}
			#endif
			float4 frag(v2f i, uint facing : SV_IsFrontFace) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				PoiMesh poiMesh;
				PoiInitStruct(PoiMesh, poiMesh);
				PoiLight poiLight;
				PoiInitStruct(PoiLight, poiLight);
				PoiVertexLights poiVertexLights;
				PoiInitStruct(PoiVertexLights, poiVertexLights);
				PoiCam poiCam;
				PoiInitStruct(PoiCam, poiCam);
				PoiMods poiMods;
				PoiInitStruct(PoiMods, poiMods);
				poiMods.globalEmission = 1;
				PoiFragData poiFragData;
				poiFragData.emission = 0;
				poiFragData.baseColor = float3(0, 0, 0);
				poiFragData.finalColor = float3(0, 0, 0);
				poiFragData.alpha = 1;
				poiMesh.objectPosition = i.objectPos;
				poiMesh.objNormal = i.objNormal;
				poiMesh.normals[0] = i.normal;
				poiMesh.tangent = i.tangent;
				poiMesh.binormal = i.binormal;
				poiMesh.worldPos = i.worldPos.xyz;
				poiMesh.localPos = i.localPos.xyz;
				poiMesh.vertexColor = i.vertexColor;
				poiMesh.isFrontFace = facing;
				#ifndef POI_PASS_OUTLINE
				if (!poiMesh.isFrontFace)
				{
					poiMesh.normals[0] *= -1;
					poiMesh.tangent *= -1;
					poiMesh.binormal *= -1;
				}
				#endif
				poiCam.viewDir = !IsOrthographicCamera() ? normalize(_WorldSpaceCameraPos - i.worldPos.xyz) : normalize(UNITY_MATRIX_I_V._m02_m12_m22);
				float3 tanToWorld0 = float3(i.tangent.x, i.binormal.x, i.normal.x);
				float3 tanToWorld1 = float3(i.tangent.y, i.binormal.y, i.normal.y);
				float3 tanToWorld2 = float3(i.tangent.z, i.binormal.z, i.normal.z);
				float3 ase_tanViewDir = tanToWorld0 * poiCam.viewDir.x + tanToWorld1 * poiCam.viewDir.y + tanToWorld2 * poiCam.viewDir.z;
				poiCam.tangentViewDir = normalize(ase_tanViewDir);
				#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
				poiMesh.lightmapUV = i.lightmapUV;
				#endif
				poiMesh.parallaxUV = poiCam.tangentViewDir.xy / max(poiCam.tangentViewDir.z, 0.0001);
				poiMesh.uv[0] = i.uv[0];
				poiMesh.uv[1] = i.uv[1];
				poiMesh.uv[2] = i.uv[2];
				poiMesh.uv[3] = i.uv[3];
				poiMesh.uv[4] = poiMesh.uv[0];
				poiMesh.uv[5] = poiMesh.worldPos.xz;
				poiMesh.uv[6] = poiMesh.uv[0];
				poiMesh.uv[7] = poiMesh.uv[0];
				poiMesh.uv[4] = calculatePanosphereUV(poiMesh);
				poiMesh.uv[6] = calculatePolarCoordinate(poiMesh);
				float4 mainTexture = UNITY_SAMPLE_TEX2D(_MainTex, poiUV(poiMesh.uv[float(0)].xy, float4(1,1,0,0)) + _Time.x * float4(0,0,0,0));
				float3 mainNormal = UnpackScaleNormal(POI2D_SAMPLER_PAN(_BumpMap, _MainTex, poiUV(poiMesh.uv[float(0)], float4(1,1,0,0)), float4(0,0,0,0)), float(1));
				poiMesh.tangentSpaceNormal = mainNormal;
				#if defined(FINALPASS) && !defined(UNITY_PASS_SHADOWCASTER) && !defined(POI_PASS_OUTLINE)
				ApplyDetailNormal(poiMods, poiMesh);
				#endif
				#if defined(GEOM_TYPE_MESH) && defined(VIGNETTE) && !defined(UNITY_PASS_SHADOWCASTER) && !defined(POI_PASS_OUTLINE)
				calculateRGBNormals(poiMesh);
				#endif
				poiMesh.normals[1] = normalize(
				poiMesh.tangentSpaceNormal.x * poiMesh.tangent.xyz +
				poiMesh.tangentSpaceNormal.y * poiMesh.binormal +
				poiMesh.tangentSpaceNormal.z * poiMesh.normals[0]
				);
				float3 fancyNormal = UnpackNormal(float4(0.5, 0.5, 1, 1));
				poiMesh.normals[0] = normalize(
				fancyNormal.x * poiMesh.tangent.xyz +
				fancyNormal.y * poiMesh.binormal +
				fancyNormal.z * poiMesh.normals[0]
				);
				poiCam.forwardDir = getCameraForward();
				poiCam.worldPos = _WorldSpaceCameraPos;
				poiCam.reflectionDir = reflect(-poiCam.viewDir, poiMesh.normals[1]);
				poiCam.vertexReflectionDir = reflect(-poiCam.viewDir, poiMesh.normals[0]);
				poiCam.distanceToVert = distance(poiMesh.worldPos, poiCam.worldPos);
				poiCam.grabPos = i.grabPos;
				poiCam.screenUV = calcScreenUVs(i.grabPos);
				poiCam.vDotN = abs(dot(poiCam.viewDir, poiMesh.normals[1]));
				poiCam.clipPos = i.pos;
				poiCam.worldDirection = i.worldDirection;
				calculateGlobalThemes(poiMods);
				poiLight.finalLightAdd = 0;
				#if defined(PROP_LIGHTINGAOMAPS) || !defined(OPTIMIZER_ENABLED)
				float4 AOMaps = POI2D_SAMPLER_PAN(_LightingAOMaps, _MainTex, poiUV(poiMesh.uv[float(0)], float4(1,1,0,0)), float4(0,0,0,0));
				poiLight.occlusion = lerp(1, AOMaps.r, float(1)) * lerp(1, AOMaps.g, float(0)) * lerp(1, AOMaps.b, float(0)) * lerp(1, AOMaps.a, float(0));
				#else
				poiLight.occlusion = 1;
				#endif
				#if defined(PROP_LIGHTINGDETAILSHADOWMAPS) || !defined(OPTIMIZER_ENABLED)
				float4 DetailShadows = POI2D_SAMPLER_PAN(_LightingDetailShadowMaps, _MainTex, poiUV(poiMesh.uv[float(0)], float4(1,1,0,0)), float4(0,0,0,0));
				poiLight.detailShadow = lerp(1, DetailShadows.r, float(1)) * lerp(1, DetailShadows.g, float(0)) * lerp(1, DetailShadows.b, float(0)) * lerp(1, DetailShadows.a, float(0));
				#else
				poiLight.detailShadow = 1;
				#endif
				#if defined(PROP_LIGHTINGSHADOWMASKS) || !defined(OPTIMIZER_ENABLED)
				float4 ShadowMasks = POI2D_SAMPLER_PAN(_LightingShadowMasks, _MainTex, poiUV(poiMesh.uv[float(0)], float4(1,1,0,0)), float4(0,0,0,0));
				poiLight.shadowMask = lerp(1, ShadowMasks.r, float(1)) * lerp(1, ShadowMasks.g, float(0)) * lerp(1, ShadowMasks.b, float(0)) * lerp(1, ShadowMasks.a, float(0));
				#else
				poiLight.shadowMask = 1;
				#endif
				#ifdef UNITY_PASS_FORWARDBASE
				bool lightExists = false;
				if (any(_LightColor0.rgb >= 0.002))
				{
					lightExists = true;
				}
				#if defined(VERTEXLIGHT_ON) && defined(POI_VERTEXLIGHT_ON)
				float4 toLightX = unity_4LightPosX0 - i.worldPos.x;
				float4 toLightY = unity_4LightPosY0 - i.worldPos.y;
				float4 toLightZ = unity_4LightPosZ0 - i.worldPos.z;
				float4 lengthSq = 0;
				lengthSq += toLightX * toLightX;
				lengthSq += toLightY * toLightY;
				lengthSq += toLightZ * toLightZ;
				float4 lightAttenSq = unity_4LightAtten0;
				float4 atten = 1.0 / (1.0 + lengthSq * lightAttenSq);
				float4 vLightWeight = saturate(1 - (lengthSq * lightAttenSq / 25));
				poiLight.vAttenuation = min(atten, vLightWeight * vLightWeight);
				poiLight.vDotNL = 0;
				poiLight.vDotNL += toLightX * poiMesh.normals[1].x;
				poiLight.vDotNL += toLightY * poiMesh.normals[1].y;
				poiLight.vDotNL += toLightZ * poiMesh.normals[1].z;
				float4 corr = rsqrt(lengthSq);
				poiLight.vertexVDotNL = max(0, poiLight.vDotNL * corr);
				poiLight.vertexVDotNL = 0;
				poiLight.vertexVDotNL += toLightX * poiMesh.normals[0].x;
				poiLight.vertexVDotNL += toLightY * poiMesh.normals[0].y;
				poiLight.vertexVDotNL += toLightZ * poiMesh.normals[0].z;
				poiLight.vertexVDotNL = max(0, poiLight.vDotNL * corr);
				poiLight.vAttenuationDotNL = saturate(poiLight.vAttenuation * saturate(poiLight.vDotNL));
				for (int index = 0; index < 4; index++)
				{
					poiLight.vPosition[index] = float3(unity_4LightPosX0[index], unity_4LightPosY0[index], unity_4LightPosZ0[index]);
					float3 vertexToLightSource = poiLight.vPosition[index] - poiMesh.worldPos;
					poiLight.vDirection[index] = normalize(vertexToLightSource);
					poiLight.vColor[index] = unity_LightColor[index].rgb;
					poiLight.vHalfDir[index] = Unity_SafeNormalize(poiLight.vDirection[index] + poiCam.viewDir);
					poiLight.vDotNL[index] = dot(poiMesh.normals[1], -poiLight.vDirection[index]);
					poiLight.vCorrectedDotNL[index] = .5 * (poiLight.vDotNL[index] + 1);
					poiLight.vDotLH[index] = saturate(dot(poiLight.vDirection[index], poiLight.vHalfDir[index]));
					poiLight.vDotNH[index] = dot(poiMesh.normals[1], poiLight.vHalfDir[index]);
					poiLight.vertexVDotNH[index] = saturate(dot(poiMesh.normals[0], poiLight.vHalfDir[index]));
				}
				#endif
				if (float(0) == 0) // Poi Custom Light Color
				{
					float3 magic = max(BetterSH9(normalize(unity_SHAr + unity_SHAg + unity_SHAb)), 0);
					float3 normalLight = _LightColor0.rgb + BetterSH9(float4(0, 0, 0, 1));
					float magiLumi = calculateluminance(magic);
					float normaLumi = calculateluminance(normalLight);
					float maginormalumi = magiLumi + normaLumi;
					float magiratio = magiLumi / maginormalumi;
					float normaRatio = normaLumi / maginormalumi;
					float target = calculateluminance(magic * magiratio + normalLight * normaRatio);
					float3 properLightColor = magic + normalLight;
					float properLuminance = calculateluminance(magic + normalLight);
					poiLight.directColor = properLightColor * max(0.0001, (target / properLuminance));
					poiLight.indirectColor = BetterSH9(float4(lerp(0, poiMesh.normals[1], float(0)), 1));
				}
				if (float(0) == 1) // More standard approach to light color
				{
					float3 indirectColor = BetterSH9(float4(poiMesh.normals[1], 1));
					if (lightExists)
					{
						poiLight.directColor = _LightColor0.rgb;
						poiLight.indirectColor = indirectColor;
					}
					else
					{
						poiLight.directColor = indirectColor * 0.6;
						poiLight.indirectColor = indirectColor * 0.5;
					}
				}
				if (float(0) == 2) // UTS style
				{
					poiLight.indirectColor = saturate(max(half3(0.05, 0.05, 0.05) * float(1), max(ShadeSH9(half4(0.0, 0.0, 0.0, 1.0)), ShadeSH9(half4(0.0, -1.0, 0.0, 1.0)).rgb) * float(1)));
					poiLight.directColor = max(poiLight.indirectColor, _LightColor0.rgb);
				}
				float lightMapMode = float(0);
				if (float(0) == 0)
				{
					poiLight.direction = _WorldSpaceLightPos0.xyz + unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz;
				}
				if (float(0) == 1 || float(0) == 2)
				{
					if (float(0) == 1)
					{
						poiLight.direction = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;;
					}
					if (float(0) == 2)
					{
						poiLight.direction = float4(0,0,0,1);
					}
					if (lightMapMode == 0)
					{
						lightMapMode == 1;
					}
				}
				if (float(0) == 3) // UTS
				{
					float3 defaultLightDirection = normalize(UNITY_MATRIX_V[2].xyz + UNITY_MATRIX_V[1].xyz);
					float3 lightDirection = normalize(lerp(defaultLightDirection, _WorldSpaceLightPos0.xyz, any(_WorldSpaceLightPos0.xyz)));
					poiLight.direction = lightDirection;
				}
				if (!any(poiLight.direction))
				{
					poiLight.direction = float3(.4, 1, .4);
				}
				poiLight.direction = normalize(poiLight.direction);
				poiLight.attenuationStrength = float(0);
				poiLight.attenuation = 1;
				if (!all(_LightColor0.rgb == 0.0))
				{
					UNITY_LIGHT_ATTENUATION(attenuation, i, poiMesh.worldPos)
					poiLight.attenuation *= attenuation;
				}
				if (!any(poiLight.directColor) && !any(poiLight.indirectColor) && lightMapMode == 0)
				{
					lightMapMode = 1;
					if (float(0) == 0)
					{
						poiLight.direction = normalize(float3(.4, 1, .4));
					}
				}
				poiLight.halfDir = normalize(poiLight.direction + poiCam.viewDir);
				poiLight.vertexNDotL = dot(poiMesh.normals[0], poiLight.direction);
				poiLight.nDotL = dot(poiMesh.normals[1], poiLight.direction);
				poiLight.nDotLSaturated = saturate(poiLight.nDotL);
				poiLight.nDotLNormalized = (poiLight.nDotL + 1) * 0.5;
				poiLight.nDotV = abs(dot(poiMesh.normals[1], poiCam.viewDir));
				poiLight.vertexNDotV = abs(dot(poiMesh.normals[0], poiCam.viewDir));
				poiLight.nDotH = dot(poiMesh.normals[1], poiLight.halfDir);
				poiLight.vertexNDotH = max(0.00001, dot(poiMesh.normals[0], poiLight.halfDir));
				poiLight.lDotv = dot(poiLight.direction, poiCam.viewDir);
				poiLight.lDotH = max(0.00001, dot(poiLight.direction, poiLight.halfDir));
				if (lightMapMode == 0)
				{
					float3 ShadeSH9Plus = GetSHLength();
					float3 ShadeSH9Minus = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w) + float3(unity_SHBr.z, unity_SHBg.z, unity_SHBb.z) / 3.0;
					float3 greyScaleVector = float3(.33333, .33333, .33333);
					float bw_lightColor = dot(poiLight.directColor, greyScaleVector);
					float bw_directLighting = (((poiLight.nDotL * 0.5 + 0.5) * bw_lightColor * lerp(1, poiLight.attenuation, poiLight.attenuationStrength)) + dot(ShadeSH9(float4(poiMesh.normals[1], 1)), greyScaleVector));
					float bw_bottomIndirectLighting = dot(ShadeSH9Minus, greyScaleVector);
					float bw_topIndirectLighting = dot(ShadeSH9Plus, greyScaleVector);
					float lightDifference = ((bw_topIndirectLighting + bw_lightColor) - bw_bottomIndirectLighting);
					poiLight.lightMap = smoothstep(0, lightDifference, bw_directLighting - bw_bottomIndirectLighting) * poiLight.detailShadow;
				}
				if (lightMapMode == 1)
				{
					poiLight.lightMap = poiLight.nDotLNormalized * lerp(1, poiLight.attenuation, poiLight.attenuationStrength);
				}
				if (lightMapMode == 2)
				{
					poiLight.lightMap = poiLight.nDotLSaturated * lerp(1, poiLight.attenuation, poiLight.attenuationStrength);
				}
				poiLight.directColor = max(poiLight.directColor, 0.0001);
				poiLight.indirectColor = max(poiLight.indirectColor, 0.0001);
				poiLight.directColor = max(poiLight.directColor, poiLight.directColor / max(0.0001, (calculateluminance(poiLight.directColor) / float(0))));
				poiLight.indirectColor = max(poiLight.indirectColor, poiLight.indirectColor / max(0.0001, (calculateluminance(poiLight.indirectColor) / float(0))));
				poiLight.directColor = lerp(poiLight.directColor, dot(poiLight.directColor, float3(0.299, 0.587, 0.114)), float(0));
				poiLight.indirectColor = lerp(poiLight.indirectColor, dot(poiLight.indirectColor, float3(0.299, 0.587, 0.114)), float(0));
				if (float(1))
				{
					poiLight.directColor = min(poiLight.directColor, float(1));
					poiLight.indirectColor = min(poiLight.indirectColor, float(1));
				}
				if (float(0))
				{
					poiLight.directColor = poiThemeColor(poiMods, float4(1,1,1,1), float(0));
				}
				#ifdef UNITY_PASS_FORWARDBASE
				poiLight.directColor = max(poiLight.directColor * float(1), 0);
				poiLight.directColor = max(poiLight.directColor + float(0), 0);
				poiLight.indirectColor = max(poiLight.indirectColor * float(1), 0);
				poiLight.indirectColor = max(poiLight.indirectColor + float(0), 0);
				#endif
				#endif
				#ifdef UNITY_PASS_FORWARDADD
				#if defined(POI_LIGHT_DATA_ADDITIVE_DIRECTIONAL_ENABLE) && defined(DIRECTIONAL)
				return float4(mainTexture.rgb * .0001, 1);
				#endif
				#if defined(POINT) || defined(SPOT)
				poiLight.direction = normalize(_WorldSpaceLightPos0.xyz - i.worldPos.xyz);
				#ifdef POINT
				poiLight.additiveShadow = UNITY_SHADOW_ATTENUATION(i, poiMesh.worldPos);
				unityShadowCoord3 lightCoord = mul(unity_WorldToLight, unityShadowCoord4(poiMesh.worldPos, 1)).xyz;
				poiLight.attenuation = tex2D(_LightTexture0, dot(lightCoord, lightCoord).rr).r;
				#endif
				#ifdef SPOT
				poiLight.additiveShadow = UNITY_SHADOW_ATTENUATION(i, poiMesh.worldPos);
				unityShadowCoord4 lightCoord = mul(unity_WorldToLight, unityShadowCoord4(poiMesh.worldPos, 1));
				poiLight.attenuation = (lightCoord.z > 0) * UnitySpotCookie(lightCoord) * UnitySpotAttenuate(lightCoord.xyz);
				#endif
				#else
				poiLight.direction = _WorldSpaceLightPos0.xyz;
				UNITY_LIGHT_ATTENUATION(attenuation, i, poiMesh.worldPos)
				poiLight.additiveShadow == 0;
				poiLight.attenuation = attenuation;
				#endif
				poiLight.directColor = float(0) ? min(float(1), _LightColor0.rgb) : _LightColor0.rgb;
				#if defined(POINT_COOKIE) || defined(DIRECTIONAL_COOKIE)
				poiLight.indirectColor = 0;
				#else
				poiLight.indirectColor = lerp(0, poiLight.directColor, float(0.5));
				#endif
				poiLight.directColor = lerp(poiLight.directColor, dot(poiLight.directColor, float3(0.299, 0.587, 0.114)), float(0));
				poiLight.indirectColor = lerp(poiLight.indirectColor, dot(poiLight.indirectColor, float3(0.299, 0.587, 0.114)), float(0));
				poiLight.halfDir = normalize(poiLight.direction + poiCam.viewDir);
				poiLight.nDotL = dot(poiMesh.normals[1], poiLight.direction);
				poiLight.nDotLSaturated = saturate(poiLight.nDotL);
				poiLight.nDotLNormalized = (poiLight.nDotL + 1) * 0.5;
				poiLight.nDotV = abs(dot(poiMesh.normals[1], poiCam.viewDir));
				poiLight.nDotH = dot(poiMesh.normals[1], poiLight.halfDir);
				poiLight.lDotv = dot(poiLight.direction, poiCam.viewDir);
				poiLight.lDotH = dot(poiLight.direction, poiLight.halfDir);
				poiLight.vertexNDotL = dot(poiMesh.normals[0], poiLight.direction);
				poiLight.vertexNDotV = abs(dot(poiMesh.normals[0], poiCam.viewDir));
				poiLight.vertexNDotH = max(0.00001, dot(poiMesh.normals[0], poiLight.halfDir));
				poiLight.lightMap = 1;
				#endif
				poiFragData.baseColor = mainTexture.rgb * poiThemeColor(poiMods, float4(1,1,1,1).rgb, float(0));
				poiFragData.alpha = mainTexture.a * float4(1,1,1,1).a;
				#if defined(PROP_CLIPPINGMASK) || !defined(OPTIMIZER_ENABLED)
				float alphaMask = POI2D_SAMPLER_PAN(_ClippingMask, _MainTex, poiUV(poiMesh.uv[float(0)], float4(1,1,0,0)), float4(0,0,0,0)).r;
				if (float(0))
				{
					alphaMask = 1 - alphaMask;
				}
				#else
				float alphaMask = 1;
				#endif
				poiFragData.alpha *= alphaMask;
				applyAlphaOptions(poiFragData, poiMesh, poiCam, poiMods);
				applyVertexColor(poiFragData, poiMesh);
				#if defined(GEOM_TYPE_BRANCH) || defined(GEOM_TYPE_BRANCH_DETAIL) || defined(GEOM_TYPE_FROND) || defined(DEPTH_OF_FIELD_COC_VIEW)
				applyDecals(poiFragData, poiMesh, poiCam, poiMods, poiLight);
				#endif
				#if defined(_LIGHTINGMODE_SHADEMAP) && defined(VIGNETTE_MASKED)
				#ifndef POI_PASS_OUTLINE
				#ifdef _LIGHTINGMODE_SHADEMAP
				applyShadeMapping(poiFragData, poiMesh, poiLight);
				#endif
				#endif
				#endif
				#ifdef VIGNETTE_MASKED
				#ifdef POI_PASS_OUTLINE
				if (_OutlineLit)
				{
					calculateShading(poiLight, poiFragData, poiMesh, poiCam);
				}
				else
				{
					poiLight.finalLighting = 1;
				}
				#else
				calculateShading(poiLight, poiFragData, poiMesh, poiCam);
				#endif
				#else
				poiLight.finalLighting = 1;
				poiLight.rampedLightMap = aaBlurStep(poiLight.nDotL, 0.1, .1);
				#endif
				#if defined(POI_MATCAP0) || defined(COLOR_GRADING_HDR_3D)
				applyMatcap(poiFragData, poiCam, poiMesh, poiLight, poiMods);
				#endif
				if (float(0))
				{
					poiFragData.baseColor *= saturate(poiFragData.alpha);
				}
				poiFragData.finalColor = poiFragData.baseColor;
				poiFragData.finalColor = poiFragData.baseColor * poiLight.finalLighting;
				if (float(0))
				{
					float3 position = float(1) ? poiMesh.worldPos : poiMesh.objectPosition;
					poiFragData.finalColor *= lerp(poiThemeColor(poiMods, float4(0,0,0,1).rgb, float(0)), poiThemeColor(poiMods, float4(1,1,1,1).rgb, float(0)), smoothstep(float(0), float(1), distance(position, poiCam.worldPos)));
				}
				if (float(0) == 0)
				{
					UNITY_APPLY_FOG(i.fogCoord, poiFragData.finalColor);
				}
				poiFragData.alpha = float(0) ? 1 : poiFragData.alpha;
				ApplyAlphaToCoverage(poiFragData, poiMesh);
				applyDithering(poiFragData, poiCam);
				if (float(0) == POI_MODE_OPAQUE)
				{
					poiFragData.alpha = 1;
				}
				clip(poiFragData.alpha - float(0.5));
				if (float(0) == POI_MODE_FADE)
				{
					clip(poiFragData.alpha - 0.01);
				}
				return float4(poiFragData.finalColor * poiFragData.alpha, poiFragData.alpha) + POI_SAFE_RGB0;
			}
			ENDCG
		}
		Pass
		{
			Tags { "LightMode" = "ShadowCaster" }
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
			AlphaToMask Off
			ZTest [_ZTest]
			ColorMask [_ColorMask]
			Offset [_OffsetFactor], [_OffsetUnits]
			BlendOp [_BlendOp], [_BlendOpAlpha]
			Blend [_SrcBlend] [_DstBlend]
			CGPROGRAM
#define OPTIMIZER_ENABLED
#define POI_LIGHT_DATA_ADDITIVE_DIRECTIONAL_ENABLE
#define POI_LIGHT_DATA_ADDITIVE_ENABLE
#define POI_VERTEXLIGHT_ON
#define VIGNETTE_CLASSIC
#define VIGNETTE_MASKED
#define _LIGHTINGMODE_MULTILAYER_MATH
#define _RIMSTYLE_POIYOMI
#define _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
#define _TPS_VERTEXCOLORS_ON
#define PROPSHADER_MASTER_LABEL 0
#define PROPSHADER_IS_USING_THRY_EDITOR 69
#define PROPFOOTER_YOUTUBE 0
#define PROPFOOTER_TWITTER 0
#define PROPFOOTER_PATREON 0
#define PROPFOOTER_DISCORD 0
#define PROPFOOTER_GITHUB 0
#define PROP_FORGOTTOLOCKMATERIAL 1
#define PROP_SHADEROPTIMIZERENABLED 0
#define PROP_LOCKTOOLTIP 0
#define PROP_MODE 0
#define PROPM_MAINCATEGORY 0
#define PROP_COLORTHEMEINDEX 0
#define PROP_MAINTEX
#define PROP_MAINTEXUV 0
#define PROP_BUMPMAP
#define PROP_BUMPMAPUV 0
#define PROP_BUMPSCALE 1
#define PROP_CLIPPINGMASKUV 0
#define PROP_INVERSE_CLIPPING 0
#define PROP_CUTOFF 0.5
#define PROPM_START_MAINHUESHIFT 0
#define PROP_MAINCOLORADJUSTTOGGLE 0
#define PROP_MAINCOLORADJUSTTEXTUREUV 0
#define PROP_SATURATION 0
#define PROP_MAINBRIGHTNESS 0
#define PROP_MAINHUESHIFTTOGGLE 0
#define PROP_MAINHUESHIFTREPLACE 1
#define PROP_MAINHUESHIFT 0
#define PROP_MAINHUESHIFTSPEED 0
#define PROP_MAINHUEALCTENABLED 0
#define PROP_MAINALHUESHIFTBAND 0
#define PROP_MAINALHUESHIFTCTINDEX 0
#define PROP_MAINHUEALMOTIONSPEED 1
#define PROPM_END_MAINHUESHIFT 0
#define PROPM_START_ALPHA 0
#define PROP_ALPHAFORCEOPAQUE 0
#define PROP_ALPHAMOD 0
#define PROP_ALPHAPREMULTIPLY 0
#define PROP_ALPHATOCOVERAGE 0
#define PROP_ALPHASHARPENEDA2C 0
#define PROP_ALPHAMIPSCALE 0.25
#define PROP_ALPHADITHERING 0
#define PROP_ALPHADITHERGRADIENT 0.1
#define PROP_ALPHADISTANCEFADE 0
#define PROP_ALPHADISTANCEFADETYPE 1
#define PROP_ALPHADISTANCEFADEMINALPHA 0
#define PROP_ALPHADISTANCEFADEMAXALPHA 1
#define PROP_ALPHADISTANCEFADEMIN 0
#define PROP_ALPHADISTANCEFADEMAX 0
#define PROP_ALPHAFRESNEL 0
#define PROP_ALPHAFRESNELALPHA 0
#define PROP_ALPHAFRESNELSHARPNESS 0.5
#define PROP_ALPHAFRESNELWIDTH 0.5
#define PROP_ALPHAFRESNELINVERT 0
#define PROP_ALPHAANGULAR 0
#define PROP_ANGLETYPE 0
#define PROP_ANGLECOMPARETO 0
#define PROP_CAMERAANGLEMIN 45
#define PROP_CAMERAANGLEMAX 90
#define PROP_MODELANGLEMIN 45
#define PROP_MODELANGLEMAX 90
#define PROP_ANGLEMINALPHA 0
#define PROP_ALPHAAUDIOLINKENABLED 0
#define PROP_ALPHAAUDIOLINKADDBAND 0
#define PROPM_END_ALPHA 0
#define PROPM_START_DETAILOPTIONS 0
#define PROP_DETAILENABLED 0
#define PROP_DETAILMASKUV 0
#define PROP_DETAILTINTTHEMEINDEX 0
#define PROP_DETAILTEXUV 0
#define PROP_DETAILTEXINTENSITY 1
#define PROP_DETAILBRIGHTNESS 1
#define PROP_DETAILNORMALMAPSCALE 1
#define PROP_DETAILNORMALMAPUV 0
#define PROPM_END_DETAILOPTIONS 0
#define PROPM_START_VERTEXMANIPULATION 0
#define PROP_VERTEXMANIPULATIONSENABLED 0
#define PROP_VERTEXMANIPULATIONHEIGHT 0
#define PROP_VERTEXMANIPULATIONHEIGHTMASKUV 0
#define PROP_VERTEXMANIPULATIONHEIGHTBIAS 0
#define PROP_VERTEXROUNDINGENABLED 0
#define PROP_VERTEXROUNDINGDIVISION 500
#define PROP_VERTEXAUDIOLINKENABLED 0
#define PROP_VERTEXLOCALTRANSLATIONALBAND 0
#define PROP_VERTEXLOCALROTATIONALBANDX 0
#define PROP_VERTEXLOCALROTATIONALBANDY 0
#define PROP_VERTEXLOCALROTATIONALBANDZ 0
#define PROP_VERTEXLOCALROTATIONCTALBANDX 0
#define PROP_VERTEXLOCALROTATIONCTALTYPEX 0
#define PROP_VERTEXLOCALROTATIONCTALBANDY 0
#define PROP_VERTEXLOCALROTATIONCTALTYPEY 0
#define PROP_VERTEXLOCALROTATIONCTALBANDZ 0
#define PROP_VERTEXLOCALROTATIONCTALTYPEZ 0
#define PROP_VERTEXLOCALSCALEALBAND 0
#define PROP_VERTEXWORLDTRANSLATIONALBAND 0
#define PROP_VERTEXMANIPULATIONHEIGHTBAND 0
#define PROP_VERTEXROUNDINGRANGEBAND 0
#define PROPM_END_VERTEXMANIPULATION 0
#define PROPM_START_MAINVERTEXCOLORS 0
#define PROP_MAINVERTEXCOLORINGLINEARSPACE 1
#define PROP_MAINVERTEXCOLORING 0
#define PROP_MAINUSEVERTEXCOLORALPHA 0
#define PROPM_END_MAINVERTEXCOLORS 0
#define PROPM_START_BACKFACE 0
#define PROP_BACKFACEENABLED 0
#define PROP_BACKFACECOLORTHEMEINDEX 0
#define PROP_BACKFACEEMISSIONSTRENGTH 0
#define PROP_BACKFACEALPHA 1
#define PROP_BACKFACETEXTUREUV 0
#define PROP_BACKFACEMASKUV 0
#define PROP_BACKFACEDETAILINTENSITY 1
#define PROP_BACKFACEREPLACEALPHA 0
#define PROP_BACKFACEEMISSIONLIMITER 1
#define PROP_BACKFACEHUESHIFTENABLED 0
#define PROP_BACKFACEHUESHIFT 0
#define PROP_BACKFACEHUESHIFTSPEED 0
#define PROPM_END_BACKFACE 0
#define PROPM_START_RGBMASK 0
#define PROP_RGBMASKENABLED 0
#define PROP_RGBUSEVERTEXCOLORS 0
#define PROP_RGBBLENDMULTIPLICATIVE 0
#define PROP_RGBMASKUV 0
#define PROP_REDCOLORTHEMEINDEX 0
#define PROP_REDTEXTUREUV 0
#define PROP_GREENCOLORTHEMEINDEX 0
#define PROP_GREENTEXTUREUV 0
#define PROP_BLUECOLORTHEMEINDEX 0
#define PROP_BLUETEXTUREUV 0
#define PROP_ALPHACOLORTHEMEINDEX 0
#define PROP_ALPHATEXTUREUV 0
#define PROP_RGBNORMALSENABLED 0
#define PROP_RGBNORMALBLEND 0
#define PROP_RGBNORMALRUV 0
#define PROP_RGBNORMALRSCALE 0
#define PROP_RGBNORMALGUV 0
#define PROP_RGBNORMALGSCALE 0
#define PROP_RGBNORMALBUV 0
#define PROP_RGBNORMALBSCALE 0
#define PROP_RGBNORMALAUV 0
#define PROP_RGBNORMALASCALE 0
#define PROPM_END_RGBMASK 0
#define PROPM_START_DECALSECTION 0
#define PROP_DECALMASKUV 0
#define PROP_DECALTPSDEPTHMASKENABLED 0
#define PROP_DECAL0TPSMASKSTRENGTH 1
#define PROP_DECAL1TPSMASKSTRENGTH 1
#define PROP_DECAL2TPSMASKSTRENGTH 1
#define PROP_DECAL3TPSMASKSTRENGTH 1
#define PROPM_START_DECAL0 0
#define PROP_DECALENABLED 0
#define PROP_DECAL0MASKCHANNEL 0
#define PROP_DECALCOLORTHEMEINDEX 0
#define PROP_DECALEMISSIONSTRENGTH 0
#define PROP_DECALTEXTUREUV 0
#define PROP_DECALTILED 0
#define PROP_DECAL0DEPTH 0
#define PROP_DECALROTATION 0
#define PROP_DECALROTATIONSPEED 0
#define PROP_DECALBLENDTYPE 0
#define PROP_DECALBLENDALPHA 1
#define PROP_DECALOVERRIDEALPHA 0
#define PROP_DECALHUESHIFTENABLED 0
#define PROP_DECALHUESHIFTSPEED 0
#define PROP_DECALHUESHIFT 0
#define PROP_DECAL0HUEANGLESTRENGTH 0
#define PROPM_START_DECAL0AUDIOLINK 0
#define PROP_AUDIOLINKDECAL0SCALEBAND 0
#define PROP_AUDIOLINKDECAL0SIDEBAND 0
#define PROP_AUDIOLINKDECAL0ROTATIONBAND 0
#define PROP_AUDIOLINKDECAL0ALPHABAND 0
#define PROP_AUDIOLINKDECAL0EMISSIONBAND 0
#define PROP_AUDIOLINKDECALCC0 0
#define PROP_DECALROTATIONCTALBAND0 0
#define PROP_DECALROTATIONCTALTYPE0 0
#define PROP_DECALROTATIONCTALSPEED0 0
#define PROPM_END_DECAL0AUDIOLINK 0
#define PROPM_END_DECAL0 0
#define PROPM_START_DECAL1 0
#define PROP_DECALENABLED1 0
#define PROP_DECAL1MASKCHANNEL 1
#define PROP_DECALCOLOR1THEMEINDEX 0
#define PROP_DECALEMISSIONSTRENGTH1 0
#define PROP_DECALTEXTURE1UV 0
#define PROP_DECALTILED1 0
#define PROP_DECAL1DEPTH 0
#define PROP_DECALROTATION1 0
#define PROP_DECALROTATIONSPEED1 0
#define PROP_DECALBLENDTYPE1 0
#define PROP_DECALBLENDALPHA1 1
#define PROP_DECALOVERRIDEALPHA1 0
#define PROP_DECALHUESHIFTENABLED1 0
#define PROP_DECALHUESHIFTSPEED1 0
#define PROP_DECALHUESHIFT1 0
#define PROP_DECAL1HUEANGLESTRENGTH 0
#define PROPM_START_DECAL1AUDIOLINK 0
#define PROP_AUDIOLINKDECAL1SCALEBAND 0
#define PROP_AUDIOLINKDECAL1SIDEBAND 0
#define PROP_AUDIOLINKDECAL1ROTATIONBAND 0
#define PROP_AUDIOLINKDECAL1ALPHABAND 0
#define PROP_AUDIOLINKDECAL1EMISSIONBAND 0
#define PROP_AUDIOLINKDECALCC1 0
#define PROP_DECALROTATIONCTALBAND1 0
#define PROP_DECALROTATIONCTALTYPE1 0
#define PROP_DECALROTATIONCTALSPEED1 0
#define PROPM_END_DECAL1AUDIOLINK 0
#define PROPM_END_DECAL1 0
#define PROPM_START_DECAL2 0
#define PROP_DECALENABLED2 0
#define PROP_DECAL2MASKCHANNEL 2
#define PROP_DECALCOLOR2THEMEINDEX 0
#define PROP_DECALEMISSIONSTRENGTH2 0
#define PROP_DECALTEXTURE2UV 0
#define PROP_DECALTILED2 0
#define PROP_DECAL2DEPTH 0
#define PROP_DECALROTATION2 0
#define PROP_DECALROTATIONSPEED2 0
#define PROP_DECALBLENDTYPE2 0
#define PROP_DECALBLENDALPHA2 1
#define PROP_DECALOVERRIDEALPHA2 0
#define PROP_DECALHUESHIFTENABLED2 0
#define PROP_DECALHUESHIFTSPEED2 0
#define PROP_DECALHUESHIFT2 0
#define PROP_DECAL2HUEANGLESTRENGTH 0
#define PROPM_START_DECAL2AUDIOLINK 0
#define PROP_AUDIOLINKDECAL2SCALEBAND 0
#define PROP_AUDIOLINKDECAL2SIDEBAND 0
#define PROP_AUDIOLINKDECAL2ROTATIONBAND 0
#define PROP_AUDIOLINKDECAL2ALPHABAND 0
#define PROP_AUDIOLINKDECAL2EMISSIONBAND 0
#define PROP_AUDIOLINKDECALCC2 0
#define PROP_DECALROTATIONCTALBAND2 0
#define PROP_DECALROTATIONCTALTYPE2 0
#define PROP_DECALROTATIONCTALSPEED2 0
#define PROPM_END_DECAL2AUDIOLINK 0
#define PROPM_END_DECAL2 0
#define PROPM_START_DECAL3 0
#define PROP_DECALENABLED3 0
#define PROP_DECAL3MASKCHANNEL 3
#define PROP_DECALCOLOR3THEMEINDEX 0
#define PROP_DECALEMISSIONSTRENGTH3 0
#define PROP_DECALTEXTURE3UV 0
#define PROP_DECALTILED3 0
#define PROP_DECAL3DEPTH 0
#define PROP_DECALROTATION3 0
#define PROP_DECALROTATIONSPEED3 0
#define PROP_DECALBLENDTYPE3 0
#define PROP_DECALBLENDALPHA3 1
#define PROP_DECALOVERRIDEALPHA3 0
#define PROP_DECALHUESHIFTENABLED3 0
#define PROP_DECALHUESHIFTSPEED3 0
#define PROP_DECALHUESHIFT3 0
#define PROP_DECAL3HUEANGLESTRENGTH 0
#define PROPM_START_DECAL3AUDIOLINK 0
#define PROP_AUDIOLINKDECAL3SCALEBAND 0
#define PROP_AUDIOLINKDECAL3SIDEBAND 0
#define PROP_AUDIOLINKDECAL3ROTATIONBAND 0
#define PROP_AUDIOLINKDECAL3ALPHABAND 0
#define PROP_AUDIOLINKDECAL3EMISSIONBAND 0
#define PROP_AUDIOLINKDECALCC3 0
#define PROP_DECALROTATIONCTALBAND3 0
#define PROP_DECALROTATIONCTALTYPE3 0
#define PROP_DECALROTATIONCTALSPEED3 0
#define PROPM_END_DECAL3AUDIOLINK 0
#define PROPM_END_DECAL3 0
#define PROPM_END_DECALSECTION 0
#define PROPM_START_TPS_PENETRATOR 0
#define PROPM_START_PEN_AUTOCONFIG 0
#define PROP_TPS_PENETRATORLENGTH 1
#define PROPM_END_PEN_AUTOCONFIG 0
#define PROP_TPSHELPBOX 0
#define PROP_TPSPENETRATORENABLED 0
#define PROP_TPSBEZIERHEADER 0
#define PROP_TPS_BEZIERSTART 0
#define PROP_TPS_BEZIERSMOOTHNESS 0.09
#define PROP_TPSSQUEEZEHEADER 0
#define PROP_TPS_SQUEEZE 0.3
#define PROP_TPS_SQUEEZEDISTANCE 0.2
#define PROP_TPSBULDGEHEADER 0
#define PROP_TPS_BULDGE 0.3
#define PROP_TPS_BULDGEDISTANCE 0.2
#define PROP_TPS_BULDGEFALLOFFDISTANCE 0.05
#define PROP_TPSPULSINGHEADER 0
#define PROP_TPS_PUMPINGSTRENGTH 0
#define PROP_TPS_PUMPINGSPEED 0
#define PROP_TPS_PUMPINGWIDTH 0.2
#define PROP_TPSIDLEHEADER 0
#define PROP_TPS_IDLEGRAVITY 0
#define PROP_TPS_IDLESKRINKWIDTH 1
#define PROP_TPS_IDLESKRINKLENGTH 1
#define PROP_TPS_IDLEMOVEMENTSTRENGTH 0
#define PROP_TPS_VERTEXCOLORS 1
#define PROP_TPS2_BUFFEREDDEPTH 0
#define PROP_TPS2_BUFFEREDSTRENGTH 0
#define PROPM_END_TPS_PENETRATOR 0
#define PROPM_START_GLOBALTHEMES 0
#define PROPM_END_GLOBALTHEMES 0
#define PROPM_LIGHTINGCATEGORY 0
#define PROPM_START_POILIGHTDATA 0
#define PROP_LIGHTINGAOMAPSUV 0
#define PROP_LIGHTDATAAOSTRENGTHR 1
#define PROP_LIGHTDATAAOSTRENGTHG 0
#define PROP_LIGHTDATAAOSTRENGTHB 0
#define PROP_LIGHTDATAAOSTRENGTHA 0
#define PROP_LIGHTINGDETAILSHADOWMAPSUV 0
#define PROP_LIGHTINGDETAILSHADOWSTRENGTHR 1
#define PROP_LIGHTINGDETAILSHADOWSTRENGTHG 0
#define PROP_LIGHTINGDETAILSHADOWSTRENGTHB 0
#define PROP_LIGHTINGDETAILSHADOWSTRENGTHA 0
#define PROP_LIGHTINGSHADOWMASKSUV 0
#define PROP_LIGHTINGSHADOWMASKSTRENGTHR 1
#define PROP_LIGHTINGSHADOWMASKSTRENGTHG 0
#define PROP_LIGHTINGSHADOWMASKSTRENGTHB 0
#define PROP_LIGHTINGSHADOWMASKSTRENGTHA 0
#define PROP_LIGHTINGCOLORMODE 0
#define PROP_LIGHTINGMAPMODE 0
#define PROP_LIGHTINGDIRECTIONMODE 0
#define PROP_LIGHTINGFORCECOLORENABLED 0
#define PROP_LIGHTINGFORCEDCOLORTHEMEINDEX 0
#define PROP_UNLIT_INTENSITY 1
#define PROP_LIGHTINGCAPENABLED 1
#define PROP_LIGHTINGCAP 1
#define PROP_LIGHTINGMINLIGHTBRIGHTNESS 0
#define PROP_LIGHTINGINDIRECTUSESNORMALS 0
#define PROP_LIGHTINGCASTEDSHADOWS 0
#define PROP_LIGHTINGMONOCHROMATIC 0
#define PROP_LIGHTINGADDITIVEENABLE 1
#define PROP_DISABLEDIRECTIONALINADD 1
#define PROP_LIGHTINGADDITIVELIMITED 0
#define PROP_LIGHTINGADDITIVELIMIT 1
#define PROP_LIGHTINGADDITIVEMONOCHROMATIC 0
#define PROP_LIGHTINGADDITIVEPASSTHROUGH 0.5
#define PROP_LIGHTINGVERTEXLIGHTINGENABLED 1
#define PROP_LIGHTDATADEBUGENABLED 0
#define PROP_LIGHTINGDEBUGVISUALIZE 0
#define PROPM_END_POILIGHTDATA 0
#define PROPM_START_POISHADING 0
#define PROP_SHADINGENABLED 1
#define PROP_LIGHTINGMODE 1
#define PROP_TOONRAMP
#define PROP_SHADOWOFFSET 0
#define PROP_LIGHTINGWRAPPEDWRAP 0
#define PROP_LIGHTINGWRAPPEDNORMALIZATION 0
#define PROP_SHADOWCOLORTEXUV 0
#define PROP_SHADOWBORDER 0.5
#define PROP_SHADOWBLUR 0.1
#define PROP_SHADOW2NDCOLORTEXUV 0
#define PROP_SHADOW2NDBORDER 0.5
#define PROP_SHADOW2NDBLUR 0.3
#define PROP_SHADOW3RDCOLORTEXUV 0
#define PROP_SHADOW3RDBORDER 0.25
#define PROP_SHADOW3RDBLUR 0.1
#define PROP_SHADOWBORDERRANGE 0
#define PROP_LIGHTINGGRADIENTSTART 0
#define PROP_LIGHTINGGRADIENTEND 0.5
#define PROP_1ST_SHADEMAPUV 0
#define PROP_USE_1STSHADEMAPALPHA_AS_SHADOWMASK 0
#define PROP_1STSHADEMAPMASK_INVERSE 0
#define PROP_USE_BASEAS1ST 0
#define PROP_2ND_SHADEMAPUV 0
#define PROP_USE_2NDSHADEMAPALPHA_AS_SHADOWMASK 0
#define PROP_2NDSHADEMAPMASK_INVERSE 0
#define PROP_USE_1STAS2ND 0
#define PROP_BASECOLOR_STEP 0.5
#define PROP_BASESHADE_FEATHER 0.0001
#define PROP_SHADECOLOR_STEP 0
#define PROP_1ST2ND_SHADES_FEATHER 0.0001
#define PROP_SHADINGSHADEMAPBLENDTYPE 0
#define PROP_SKINLUT
#define PROP_SSSSCALE 1
#define PROP_SSSBUMPBLUR 0.7
#define PROP_CLOTHDFG
#define PROP_CLOTHMETALLICSMOOTHNESSMAPINVERT 0
#define PROP_CLOTHMETALLICSMOOTHNESSMAPUV 0
#define PROP_CLOTHREFLECTANCE 0.5
#define PROP_CLOTHSMOOTHNESS 0.5
#define PROP_SHADOWSTRENGTH 1
#define PROP_LIGHTINGIGNOREAMBIENTCOLOR 0
#define PROP_LIGHTINGADDITIVETYPE 1
#define PROP_LIGHTINGADDITIVEGRADIENTSTART 0
#define PROP_LIGHTINGADDITIVEGRADIENTEND 0.5
#define PROPM_END_POISHADING 0
#define PROPM_START_ANISO 0
#define PROP_ENABLEANISO 0
#define PROP_ANISOCOLORMAPUV 0
#define PROP_ANISOUSELIGHTCOLOR 1
#define PROP_ANISOUSEBASECOLOR 0
#define PROP_ANISOREPLACE 0
#define PROP_ANISOADD 1
#define PROP_ANISOHIDEINSHADOW 1
#define PROP_ANISO0POWER 0
#define PROP_ANISO0STRENGTH 1
#define PROP_ANISO0OFFSET 0
#define PROP_ANISO0OFFSETMAPSTRENGTH 0
#define PROP_ANISO0TINTINDEX 0
#define PROP_ANISO0TOONMODE 0
#define PROP_ANISO0EDGE 0.5
#define PROP_ANISO0BLUR 0
#define PROP_ANISO1POWER 0.1
#define PROP_ANISO1STRENGTH 1
#define PROP_ANISO1OFFSET 0
#define PROP_ANISO1OFFSETMAPSTRENGTH 0
#define PROP_ANISO1TINTINDEX 0
#define PROP_ANISO1TOONMODE 0
#define PROP_ANISO1EDGE 0.5
#define PROP_ANISO1BLUR 0
#define PROP_ANISODEBUGTOGGLE 0
#define PROP_ANISODEBUGMODE 0
#define PROPM_END_ANSIO 0
#define PROPM_START_MATCAP 0
#define PROP_MATCAPENABLE 0
#define PROP_MATCAPUVMODE 1
#define PROP_MATCAPCOLORTHEMEINDEX 0
#define PROP_MATCAPBORDER 0.43
#define PROP_MATCAPMASKUV 0
#define PROP_MATCAPMASKINVERT 0
#define PROP_MATCAPEMISSIONSTRENGTH 0
#define PROP_MATCAPINTENSITY 1
#define PROP_MATCAPLIGHTMASK 0
#define PROP_MATCAPREPLACE 1
#define PROP_MATCAPMULTIPLY 0
#define PROP_MATCAPADD 0
#define PROP_MATCAPMIXED 0
#define PROP_MATCAPADDTOLIGHT 0
#define PROP_MATCAPALPHAOVERRIDE 0
#define PROP_MATCAPNORMAL 1
#define PROP_MATCAP0CUSTOMNORMAL 0
#define PROP_MATCAP0NORMALMAPUV 0
#define PROP_MATCAP0NORMALMAPSCALE 1
#define PROP_MATCAPHUESHIFTENABLED 0
#define PROP_MATCAPHUESHIFTSPEED 0
#define PROP_MATCAPHUESHIFT 0
#define PROP_MATCAPTPSDEPTHENABLED 0
#define PROP_MATCAPTPSMASKSTRENGTH 1
#define PROPM_END_MATCAP 0
#define PROPM_START_MATCAP2 0
#define PROP_MATCAP2ENABLE 0
#define PROP_MATCAP2UVMODE 1
#define PROP_MATCAP2COLORTHEMEINDEX 0
#define PROP_MATCAP2BORDER 0.43
#define PROP_MATCAP2MASKUV 0
#define PROP_MATCAP2MASKINVERT 0
#define PROP_MATCAP2EMISSIONSTRENGTH 0
#define PROP_MATCAP2INTENSITY 1
#define PROP_MATCAP2LIGHTMASK 0
#define PROP_MATCAP2REPLACE 0
#define PROP_MATCAP2MULTIPLY 0
#define PROP_MATCAP2ADD 0
#define PROP_MATCAP2MIXED 0
#define PROP_MATCAP2ADDTOLIGHT 0
#define PROP_MATCAP2ALPHAOVERRIDE 0
#define PROP_MATCAP2NORMAL 1
#define PROP_MATCAP1CUSTOMNORMAL 0
#define PROP_MATCAP1CUSTOMNORMAL 0
#define PROP_MATCAP1NORMALMAPUV 0
#define PROP_MATCAP1NORMALMAPSCALE 1
#define PROP_MATCAP2HUESHIFTENABLED 0
#define PROP_MATCAP2HUESHIFTSPEED 0
#define PROP_MATCAP2HUESHIFT 0
#define PROP_MATCAP2TPSDEPTHENABLED 0
#define PROP_MATCAP2TPSMASKSTRENGTH 1
#define PROPM_END_MATCAP2 0
#define PROPM_START_CUBEMAP 0
#define PROP_CUBEMAPENABLED 0
#define PROP_CUBEMAPUVMODE 1
#define PROP_CUBEMAPCOLORTHEMEINDEX 0
#define PROP_CUBEMAP
#define PROP_CUBEMAPMASKUV 0
#define PROP_CUBEMAPMASKINVERT 0
#define PROP_CUBEMAPEMISSIONSTRENGTH 0
#define PROP_CUBEMAPINTENSITY 1
#define PROP_CUBEMAPLIGHTMASK 0
#define PROP_CUBEMAPREPLACE 1
#define PROP_CUBEMAPMULTIPLY 0
#define PROP_CUBEMAPADD 0
#define PROP_CUBEMAPNORMAL 1
#define PROP_CUBEMAPHUESHIFTENABLED 0
#define PROP_CUBEMAPHUESHIFTSPEED 0
#define PROP_CUBEMAPHUESHIFT 0
#define PROPM_END_CUBEMAP 0
#define PROPM_START_RIMLIGHTOPTIONS 0
#define PROP_ENABLERIMLIGHTING 0
#define PROP_RIMSTYLE 0
#define PROP_RIMTEXUV 0
#define PROP_RIMMASKUV 0
#define PROP_IS_NORMALMAPTORIMLIGHT 1
#define PROP_RIMLIGHTINGINVERT 0
#define PROP_RIMLIGHTCOLORTHEMEINDEX 0
#define PROP_RIMWIDTH 0.8
#define PROP_RIMSHARPNESS 0.25
#define PROP_RIMPOWER 1
#define PROP_RIMSTRENGTH 0
#define PROP_RIMBASECOLORMIX 0
#define PROP_RIMBLENDMODE 0
#define PROP_RIMBLENDSTRENGTH 1
#define PROP_IS_LIGHTCOLOR_RIMLIGHT 1
#define PROP_RIMLIGHT_POWER 0.1
#define PROP_RIMLIGHT_INSIDEMASK 0.0001
#define PROP_RIMLIGHT_FEATHEROFF 0
#define PROP_LIGHTDIRECTION_MASKON 0
#define PROP_TWEAK_LIGHTDIRECTION_MASKLEVEL 0
#define PROP_ADD_ANTIPODEAN_RIMLIGHT 0
#define PROP_IS_LIGHTCOLOR_AP_RIMLIGHT 1
#define PROP_RIMAPCOLORTHEMEINDEX 0
#define PROP_AP_RIMLIGHT_POWER 0.1
#define PROP_AP_RIMLIGHT_FEATHEROFF 0
#define PROP_TWEAK_RIMLIGHTMASKLEVEL 0
#define PROP_RIMSHADOWTOGGLE 0
#define PROP_RIMSHADOWMASKRAMPTYPE 0
#define PROP_RIMSHADOWMASKSTRENGTH 1
#define PROP_RIMSHADOWWIDTH 0
#define PROP_RIMHUESHIFTENABLED 0
#define PROP_RIMHUESHIFTSPEED 0
#define PROP_RIMHUESHIFT 0
#define PROPM_START_RIMAUDIOLINK 0
#define PROP_AUDIOLINKRIMWIDTHBAND 0
#define PROP_AUDIOLINKRIMEMISSIONBAND 0
#define PROP_AUDIOLINKRIMBRIGHTNESSBAND 0
#define PROPM_END_RIMAUDIOLINK 0
#define PROPM_END_RIMLIGHTOPTIONS 0
#define PROPM_START_DEPTHRIMLIGHTOPTIONS 0
#define PROP_ENABLEDEPTHRIMLIGHTING 0
#define PROP_DEPTHRIMNORMALTOUSE 1
#define PROP_DEPTHRIMWIDTH 0.2
#define PROP_DEPTHRIMSHARPNESS 0.2
#define PROP_DEPTHRIMHIDEINSHADOW 0
#define PROP_DEPTHRIMMIXBASECOLOR 0
#define PROP_DEPTHRIMMIXLIGHTCOLOR 0
#define PROP_DEPTHRIMCOLORTHEMEINDEX 0
#define PROP_DEPTHRIMEMISSION 0
#define PROP_DEPTHRIMREPLACE 0
#define PROP_DEPTHRIMADD 0
#define PROP_DEPTHRIMMULTIPLY 0
#define PROP_DEPTHRIMADDITIVELIGHTING 0
#define PROPM_END_DEPTHRIMLIGHTOPTIONS 0
#define PROPM_START_BRDF 1
#define PROP_MOCHIEBRDF 0
#define PROP_MOCHIEREFLECTIONSTRENGTH 1
#define PROP_MOCHIESPECULARSTRENGTH 1
#define PROP_MOCHIEMETALLICMULTIPLIER 0
#define PROP_MOCHIEROUGHNESSMULTIPLIER 1
#define PROP_MOCHIEREFLECTIONTINTTHEMEINDEX 0
#define PROP_MOCHIESPECULARTINTTHEMEINDEX 0
#define PROP_MOCHIEMETALLICMAPSUV 0
#define PROP_MOCHIEMETALLICMAPINVERT 0
#define PROP_MOCHIEROUGHNESSMAPINVERT 0
#define PROP_MOCHIEREFLECTIONMASKINVERT 0
#define PROP_MOCHIESPECULARMASKINVERT 0
#define PROP_PBRSPLITMASKSAMPLE 0
#define PROP_MOCHIEMETALLICMASKSUV 0
#define PROP_SPECULAR2NDLAYER 0
#define PROP_MOCHIESPECULARSTRENGTH2 1
#define PROP_MOCHIEROUGHNESSMULTIPLIER2 1
#define PROP_BRDFTPSDEPTHENABLED 0
#define PROP_BRDFTPSREFLECTIONMASKSTRENGTH 1
#define PROP_BRDFTPSSPECULARMASKSTRENGTH 1
#define PROP_IGNORECASTEDSHADOWS 0
#define PROP_MOCHIEFORCEFALLBACK 0
#define PROP_MOCHIELITFALLBACK 0
#define PROP_MOCHIEGSAAENABLED 1
#define PROP_POIGSAAVARIANCE 0.15
#define PROP_POIGSAATHRESHOLD 0.1
#define PROP_REFSPECFRESNEL 1
#define PROPM_END_BRDF 0
#define PROPM_START_CLEARCOAT 0
#define PROP_CLEARCOATBRDF 0
#define PROP_CLEARCOATSTRENGTH 1
#define PROP_CLEARCOATSMOOTHNESS 1
#define PROP_CLEARCOATREFLECTIONSTRENGTH 1
#define PROP_CLEARCOATSPECULARSTRENGTH 1
#define PROP_CLEARCOATREFLECTIONTINTTHEMEINDEX 0
#define PROP_CLEARCOATSPECULARTINTTHEMEINDEX 0
#define PROP_CLEARCOATMAPSUV 0
#define PROP_CLEARCOATMASKINVERT 0
#define PROP_CLEARCOATSMOOTHNESSMAPINVERT 0
#define PROP_CLEARCOATREFLECTIONMASKINVERT 0
#define PROP_CLEARCOATSPECULARMASKINVERT 0
#define PROP_CLEARCOATFORCEFALLBACK 0
#define PROP_CLEARCOATLITFALLBACK 0
#define PROP_CCIGNORECASTEDSHADOWS 0
#define PROP_CLEARCOATGSAAENABLED 1
#define PROP_CLEARCOATGSAAVARIANCE 0.15
#define PROP_CLEARCOATGSAATHRESHOLD 0.1
#define PROP_CLEARCOATTPSDEPTHMASKENABLED 0
#define PROP_CLEARCOATTPSMASKSTRENGTH 1
#define PROPM_END_CLEARCOAT 0
#define PROPM_START_REFLECTIONRIM 0
#define PROP_ENABLEENVIRONMENTALRIM 0
#define PROP_RIMENVIROMASKUV 0
#define PROP_RIMENVIROBLUR 0.7
#define PROP_RIMENVIROWIDTH 0.45
#define PROP_RIMENVIROSHARPNESS 0
#define PROP_RIMENVIROMINBRIGHTNESS 0
#define PROP_RIMENVIROINTENSITY 1
#define PROPM_END_REFLECTIONRIM 0
#define PROPM_START_STYLIZEDSPEC 0
#define PROP_STYLIZEDSPECULAR 0
#define PROP_HIGHCOLOR_TEXUV 0
#define PROP_HIGHCOLORTHEMEINDEX 0
#define PROP_SET_HIGHCOLORMASKUV 0
#define PROP_TWEAK_HIGHCOLORMASKLEVEL 0
#define PROP_IS_SPECULARTOHIGHCOLOR 0
#define PROP_IS_BLENDADDTOHICOLOR 0
#define PROP_STYLIZEDSPECULARSTRENGTH 1
#define PROP_USELIGHTCOLOR 1
#define PROP_SSIGNORECASTEDSHADOWS 0
#define PROP_HIGHCOLOR_POWER 0.2
#define PROP_STYLIZEDSPECULARFEATHER 0
#define PROP_LAYER1STRENGTH 1
#define PROP_LAYER2SIZE 0
#define PROP_STYLIZEDSPECULAR2FEATHER 0
#define PROP_LAYER2STRENGTH 0
#define PROPM_END_STYLIZEDSPEC 0
#define PROPM_SPECIALFXCATEGORY 0
#define PROPM_START_UDIMDISCARDOPTIONS 0
#define PROP_ENABLEUDIMDISCARDOPTIONS 0
#define PROP_UDIMDISCARDUV 0
#define PROP_UDIMDISCARDMODE 1
#define PROPM_END_UDIMDISCARDOPTIONS 0
#define PROPM_START_DISSOLVE 0
#define PROP_ENABLEDISSOLVE 0
#define PROP_DISSOLVETYPE 1
#define PROP_DISSOLVEEDGEWIDTH 0.025
#define PROP_DISSOLVEEDGEHARDNESS 0.5
#define PROP_DISSOLVEEDGECOLORTHEMEINDEX 0
#define PROP_DISSOLVEEDGEEMISSION 0
#define PROP_DISSOLVETEXTURECOLORTHEMEINDEX 0
#define PROP_DISSOLVETOTEXTUREUV 0
#define PROP_DISSOLVETOEMISSIONSTRENGTH 0
#define PROP_DISSOLVENOISETEXTUREUV 0
#define PROP_DISSOLVEINVERTNOISE 0
#define PROP_DISSOLVEDETAILNOISEUV 0
#define PROP_DISSOLVEINVERTDETAILNOISE 0
#define PROP_DISSOLVEDETAILSTRENGTH 0.1
#define PROP_DISSOLVEALPHA 0
#define PROP_DISSOLVEMASKUV 0
#define PROP_DISSOLVEUSEVERTEXCOLORS 0
#define PROP_DISSOLVEMASKINVERT 0
#define PROP_CONTINUOUSDISSOLVE 0
#define PROP_ENABLEDISSOLVEAUDIOLINK 0
#define PROP_AUDIOLINKDISSOLVEALPHABAND 0
#define PROP_AUDIOLINKDISSOLVEDETAILBAND 0
#define PROPM_START_POINTTOPOINT 0
#define PROP_DISSOLVEP2PWORLDLOCAL 0
#define PROP_DISSOLVEP2PEDGELENGTH 0.1
#define PROPM_END_POINTTOPOINT 0
#define PROPM_START_DISSOLVEHUESHIFT 0
#define PROP_DISSOLVEHUESHIFTENABLED 0
#define PROP_DISSOLVEHUESHIFTSPEED 0
#define PROP_DISSOLVEHUESHIFT 0
#define PROP_DISSOLVEEDGEHUESHIFTENABLED 0
#define PROP_DISSOLVEEDGEHUESHIFTSPEED 0
#define PROP_DISSOLVEEDGEHUESHIFT 0
#define PROPM_END_DISSOLVEHUESHIFT 0
#define PROPM_START_BONUSSLIDERS 0
#define PROP_DISSOLVEALPHA0 0
#define PROP_DISSOLVEALPHA1 0
#define PROP_DISSOLVEALPHA2 0
#define PROP_DISSOLVEALPHA3 0
#define PROP_DISSOLVEALPHA4 0
#define PROP_DISSOLVEALPHA5 0
#define PROP_DISSOLVEALPHA6 0
#define PROP_DISSOLVEALPHA7 0
#define PROP_DISSOLVEALPHA8 0
#define PROP_DISSOLVEALPHA9 0
#define PROPM_END_BONUSSLIDERS 0
#define PROPM_END_DISSOLVE 0
#define PROPM_START_FLIPBOOK 0
#define PROP_ENABLEFLIPBOOK 0
#define PROP_FLIPBOOKALPHACONTROLSFINALALPHA 0
#define PROP_FLIPBOOKINTENSITYCONTROLSALPHA 0
#define PROP_FLIPBOOKCOLORREPLACES 0
#define PROP_FLIPBOOKTEXARRAYUV 0
#define PROP_FLIPBOOKMASKUV 0
#define PROP_FLIPBOOKCOLORTHEMEINDEX 0
#define PROP_FLIPBOOKTOTALFRAMES 1
#define PROP_FLIPBOOKFPS 30
#define PROP_FLIPBOOKTILED 0
#define PROP_FLIPBOOKEMISSIONSTRENGTH 0
#define PROP_FLIPBOOKROTATION 0
#define PROP_FLIPBOOKROTATIONSPEED 0
#define PROP_FLIPBOOKREPLACE 1
#define PROP_FLIPBOOKMULTIPLY 0
#define PROP_FLIPBOOKADD 0
#define PROP_FLIPBOOKMANUALFRAMECONTROL 0
#define PROP_FLIPBOOKCURRENTFRAME -1
#define PROP_FLIPBOOKCROSSFADEENABLED 0
#define PROP_FLIPBOOKHUESHIFTENABLED 0
#define PROP_FLIPBOOKHUESHIFTSPEED 0
#define PROP_FLIPBOOKHUESHIFT 0
#define PROPM_START_FLIPBOOKAUDIOLINK 0
#define PROP_AUDIOLINKFLIPBOOKSCALEBAND 0
#define PROP_AUDIOLINKFLIPBOOKALPHABAND 0
#define PROP_AUDIOLINKFLIPBOOKEMISSIONBAND 0
#define PROP_AUDIOLINKFLIPBOOKFRAMEBAND 0
#define PROP_FLIPBOOKCHRONOTENSITYENABLED 0
#define PROP_FLIPBOOKCHRONOTENSITYBAND 0
#define PROP_FLIPBOOKCHRONOTYPE 0
#define PROP_FLIPBOOKCHRONOTENSITYSPEED 0
#define PROPM_END_FLIPBOOKAUDIOLINK 0
#define PROPM_END_FLIPBOOK 0
#define PROPM_START_EMISSIONS 0
#define PROPM_START_EMISSIONOPTIONS 0
#define PROP_ENABLEEMISSION 0
#define PROP_EMISSIONREPLACE0 0
#define PROP_EMISSIONCOLORTHEMEINDEX 0
#define PROP_EMISSIONMAPUV 0
#define PROP_EMISSIONBASECOLORASMAP 0
#define PROP_EMISSIONMASKUV 0
#define PROP_EMISSIONMASKINVERT 0
#define PROP_EMISSIONSTRENGTH 0
#define PROP_EMISSIONHUESHIFTENABLED 0
#define PROP_EMISSIONHUESHIFT 0
#define PROP_EMISSIONHUESHIFTSPEED 0
#define PROP_EMISSIONCENTEROUTENABLED 0
#define PROP_EMISSIONCENTEROUTSPEED 5
#define PROP_ENABLEGITDEMISSION 0
#define PROP_GITDEWORLDORMESH 0
#define PROP_GITDEMINEMISSIONMULTIPLIER 1
#define PROP_GITDEMAXEMISSIONMULTIPLIER 0
#define PROP_GITDEMINLIGHT 0
#define PROP_GITDEMAXLIGHT 1
#define PROP_EMISSIONBLINKINGENABLED 0
#define PROP_EMISSIVEBLINK_MIN 0
#define PROP_EMISSIVEBLINK_MAX 1
#define PROP_EMISSIVEBLINK_VELOCITY 4
#define PROP_EMISSIONBLINKINGOFFSET 0
#define PROP_SCROLLINGEMISSION 0
#define PROP_EMISSIONSCROLLINGUSECURVE 0
#define PROP_EMISSIONSCROLLINGVERTEXCOLOR 0
#define PROP_EMISSIVESCROLL_WIDTH 10
#define PROP_EMISSIVESCROLL_VELOCITY 10
#define PROP_EMISSIVESCROLL_INTERVAL 20
#define PROP_EMISSIONSCROLLINGOFFSET 0
#define PROP_EMISSIONAL0ENABLED 0
#define PROP_EMISSIONAL0STRENGTHBAND 0
#define PROP_AUDIOLINKEMISSION0CENTEROUTSIZE 0
#define PROP_AUDIOLINKEMISSION0CENTEROUTBAND 0
#define PROPM_END_EMISSIONOPTIONS 0
#define PROPM_START_EMISSION1OPTIONS 0
#define PROP_ENABLEEMISSION1 0
#define PROP_EMISSIONREPLACE1 0
#define PROP_EMISSIONCOLOR1THEMEINDEX 0
#define PROP_EMISSIONMAP1UV 0
#define PROP_EMISSIONBASECOLORASMAP1 0
#define PROP_EMISSIONMASK1UV 0
#define PROP_EMISSIONMASKINVERT1 0
#define PROP_EMISSIONSTRENGTH1 0
#define PROP_EMISSIONHUESHIFTENABLED1 0
#define PROP_EMISSIONHUESHIFT1 0
#define PROP_EMISSIONHUESHIFTSPEED1 0
#define PROP_EMISSIONCENTEROUTENABLED1 0
#define PROP_EMISSIONCENTEROUTSPEED1 5
#define PROP_ENABLEGITDEMISSION1 0
#define PROP_GITDEWORLDORMESH1 0
#define PROP_GITDEMINEMISSIONMULTIPLIER1 1
#define PROP_GITDEMAXEMISSIONMULTIPLIER1 0
#define PROP_GITDEMINLIGHT1 0
#define PROP_GITDEMAXLIGHT1 1
#define PROP_EMISSIONBLINKINGENABLED1 0
#define PROP_EMISSIVEBLINK_MIN1 0
#define PROP_EMISSIVEBLINK_MAX1 1
#define PROP_EMISSIVEBLINK_VELOCITY1 4
#define PROP_EMISSIONBLINKINGOFFSET1 0
#define PROP_SCROLLINGEMISSION1 0
#define PROP_EMISSIONSCROLLINGUSECURVE1 0
#define PROP_EMISSIONSCROLLINGVERTEXCOLOR1 0
#define PROP_EMISSIVESCROLL_WIDTH1 10
#define PROP_EMISSIVESCROLL_VELOCITY1 10
#define PROP_EMISSIVESCROLL_INTERVAL1 20
#define PROP_EMISSIONSCROLLINGOFFSET1 0
#define PROP_EMISSIONAL1ENABLED 0
#define PROP_EMISSIONAL1STRENGTHBAND 0
#define PROP_AUDIOLINKEMISSION1CENTEROUTSIZE 0
#define PROP_AUDIOLINKEMISSION1CENTEROUTBAND 0
#define PROPM_END_EMISSION1OPTIONS 0
#define PROPM_START_EMISSION2OPTIONS 0
#define PROP_ENABLEEMISSION2 0
#define PROP_EMISSIONREPLACE2 0
#define PROP_EMISSIONCOLOR2THEMEINDEX 0
#define PROP_EMISSIONMAP2UV 0
#define PROP_EMISSIONBASECOLORASMAP2 0
#define PROP_EMISSIONMASK2UV 0
#define PROP_EMISSIONMASKINVERT2 0
#define PROP_EMISSIONSTRENGTH2 0
#define PROP_EMISSIONHUESHIFTENABLED2 0
#define PROP_EMISSIONHUESHIFT2 0
#define PROP_EMISSIONHUESHIFTSPEED2 0
#define PROP_EMISSIONCENTEROUTENABLED2 0
#define PROP_EMISSIONCENTEROUTSPEED2 5
#define PROP_ENABLEGITDEMISSION2 0
#define PROP_GITDEWORLDORMESH2 0
#define PROP_GITDEMINEMISSIONMULTIPLIER2 1
#define PROP_GITDEMAXEMISSIONMULTIPLIER2 0
#define PROP_GITDEMINLIGHT2 0
#define PROP_GITDEMAXLIGHT2 1
#define PROP_EMISSIONBLINKINGENABLED2 0
#define PROP_EMISSIVEBLINK_MIN2 0
#define PROP_EMISSIVEBLINK_MAX2 1
#define PROP_EMISSIVEBLINK_VELOCITY2 4
#define PROP_EMISSIONBLINKINGOFFSET2 0
#define PROP_SCROLLINGEMISSION2 0
#define PROP_EMISSIONSCROLLINGUSECURVE2 0
#define PROP_EMISSIONSCROLLINGVERTEXCOLOR2 0
#define PROP_EMISSIVESCROLL_WIDTH2 10
#define PROP_EMISSIVESCROLL_VELOCITY2 10
#define PROP_EMISSIVESCROLL_INTERVAL2 20
#define PROP_EMISSIONSCROLLINGOFFSET2 0
#define PROP_EMISSIONAL2ENABLED 0
#define PROP_EMISSIONAL2STRENGTHBAND 0
#define PROP_AUDIOLINKEMISSION2CENTEROUTSIZE 0
#define PROP_AUDIOLINKEMISSION2CENTEROUTBAND 0
#define PROPM_END_EMISSION2OPTIONS 0
#define PROPM_START_EMISSION3OPTIONS 0
#define PROP_ENABLEEMISSION3 0
#define PROP_EMISSIONREPLACE3 0
#define PROP_EMISSIONCOLOR3THEMEINDEX 0
#define PROP_EMISSIONMAP3UV 0
#define PROP_EMISSIONBASECOLORASMAP3 0
#define PROP_EMISSIONMASK3UV 0
#define PROP_EMISSIONMASKINVERT3 0
#define PROP_EMISSIONSTRENGTH3 0
#define PROP_EMISSIONHUESHIFTENABLED3 0
#define PROP_EMISSIONHUESHIFT3 0
#define PROP_EMISSIONHUESHIFTSPEED3 0
#define PROP_EMISSIONCENTEROUTENABLED3 0
#define PROP_EMISSIONCENTEROUTSPEED3 5
#define PROP_ENABLEGITDEMISSION3 0
#define PROP_GITDEWORLDORMESH3 0
#define PROP_GITDEMINEMISSIONMULTIPLIER3 1
#define PROP_GITDEMAXEMISSIONMULTIPLIER3 0
#define PROP_GITDEMINLIGHT3 0
#define PROP_GITDEMAXLIGHT3 1
#define PROP_EMISSIONBLINKINGENABLED3 0
#define PROP_EMISSIVEBLINK_MIN3 0
#define PROP_EMISSIVEBLINK_MAX3 1
#define PROP_EMISSIVEBLINK_VELOCITY3 4
#define PROP_EMISSIONBLINKINGOFFSET3 0
#define PROP_SCROLLINGEMISSION3 0
#define PROP_EMISSIONSCROLLINGUSECURVE3 0
#define PROP_EMISSIONSCROLLINGVERTEXCOLOR3 0
#define PROP_EMISSIVESCROLL_WIDTH3 10
#define PROP_EMISSIVESCROLL_VELOCITY3 10
#define PROP_EMISSIVESCROLL_INTERVAL3 20
#define PROP_EMISSIONSCROLLINGOFFSET3 0
#define PROP_EMISSIONAL3ENABLED 0
#define PROP_EMISSIONAL3STRENGTHBAND 0
#define PROP_AUDIOLINKEMISSION3CENTEROUTSIZE 0
#define PROP_AUDIOLINKEMISSION3CENTEROUTBAND 0
#define PROPM_END_EMISSION3OPTIONS 0
#define PROPM_END_EMISSIONS 0
#define PROPM_START_GLITTER 0
#define PROP_GLITTERENABLE 0
#define PROP_GLITTERUV 0
#define PROP_GLITTERMODE 0
#define PROP_GLITTERSHAPE 0
#define PROP_GLITTERBLENDTYPE 0
#define PROP_GLITTERCOLORTHEMEINDEX 0
#define PROP_GLITTERUSESURFACECOLOR 0
#define PROP_GLITTERCOLORMAPUV 0
#define PROP_GLITTERMASKUV 0
#define PROP_GLITTERTEXTUREROTATION 0
#define PROP_GLITTERFREQUENCY 300
#define PROP_GLITTERJITTER 1
#define PROP_GLITTERSPEED 10
#define PROP_GLITTERSIZE 0.3
#define PROP_GLITTERCONTRAST 300
#define PROP_GLITTERANGLERANGE 90
#define PROP_GLITTERMINBRIGHTNESS 0
#define PROP_GLITTERBRIGHTNESS 3
#define PROP_GLITTERBIAS 0.8
#define PROP_GLITTERHIDEINSHADOW 0
#define PROP_GLITTERCENTERSIZE 0.08
#define PROP_GLITTERFREQUENCYLINEAREMISSIVE 20
#define PROP_GLITTERJAGGYFIX 0
#define PROP_GLITTERHUESHIFTENABLED 0
#define PROP_GLITTERHUESHIFTSPEED 0
#define PROP_GLITTERHUESHIFT 0
#define PROP_GLITTERRANDOMCOLORS 0
#define PROP_GLITTERRANDOMSIZE 0
#define PROP_GLITTERRANDOMROTATION 0
#define PROPM_END_GLITTER 0
#define PROPM_START_PATHING 0
#define PROP_ENABLEPATHING 0
#define PROP_PATHGRADIENTTYPE 0
#define PROP_PATHINGOVERRIDEALPHA 0
#define PROP_PATHINGMAPUV 0
#define PROP_PATHINGCOLORMAPUV 0
#define PROP_PATHTYPER 0
#define PROP_PATHTYPEG 0
#define PROP_PATHTYPEB 0
#define PROP_PATHTYPEA 0
#define PROP_PATHCOLORRTHEMEINDEX 0
#define PROP_PATHCOLORGTHEMEINDEX 0
#define PROP_PATHCOLORBTHEMEINDEX 0
#define PROP_PATHCOLORATHEMEINDEX 0
#define PROPM_START_PATHAUDIOLINK 0
#define PROP_PATHALTIMEOFFSET 0
#define PROP_AUDIOLINKPATHTIMEOFFSETBANDR 0
#define PROP_AUDIOLINKPATHTIMEOFFSETBANDG 0
#define PROP_AUDIOLINKPATHTIMEOFFSETBANDB 0
#define PROP_AUDIOLINKPATHTIMEOFFSETBANDA 0
#define PROP_PATHALEMISSIONOFFSET 0
#define PROP_AUDIOLINKPATHEMISSIONADDBANDR 0
#define PROP_AUDIOLINKPATHEMISSIONADDBANDG 0
#define PROP_AUDIOLINKPATHEMISSIONADDBANDB 0
#define PROP_AUDIOLINKPATHEMISSIONADDBANDA 0
#define PROP_PATHALWIDTHOFFSET 0
#define PROP_AUDIOLINKPATHWIDTHOFFSETBANDR 0
#define PROP_AUDIOLINKPATHWIDTHOFFSETBANDG 0
#define PROP_AUDIOLINKPATHWIDTHOFFSETBANDB 0
#define PROP_AUDIOLINKPATHWIDTHOFFSETBANDA 0
#define PROP_PATHALHISTORY 0
#define PROP_PATHALHISTORYBANDR 0
#define PROP_PATHALHISTORYR 0
#define PROP_PATHALHISTORYBANDG 0
#define PROP_PATHALHISTORYG 0
#define PROP_PATHALHISTORYBANDB 0
#define PROP_PATHALHISTORYB 0
#define PROP_PATHALHISTORYBANDA 0
#define PROP_PATHALHISTORYA 0
#define PROP_PATHALCHRONO 0
#define PROP_PATHCHRONOBANDR 0
#define PROP_PATHCHRONOTYPER 0
#define PROP_PATHCHRONOSPEEDR 0
#define PROP_PATHCHRONOBANDG 0
#define PROP_PATHCHRONOTYPEG 0
#define PROP_PATHCHRONOSPEEDG 0
#define PROP_PATHCHRONOBANDB 0
#define PROP_PATHCHRONOTYPEB 0
#define PROP_PATHCHRONOSPEEDB 0
#define PROP_PATHCHRONOBANDA 0
#define PROP_PATHCHRONOTYPEA 0
#define PROP_PATHCHRONOSPEEDA 0
#define PROP_PATHALAUTOCORRELATOR 0
#define PROP_PATHALAUTOCORRELATORR 0
#define PROP_PATHALAUTOCORRELATORG 0
#define PROP_PATHALAUTOCORRELATORB 0
#define PROP_PATHALAUTOCORRELATORA 0
#define PROP_PATHALCCR 0
#define PROP_PATHALCCG 0
#define PROP_PATHALCCB 0
#define PROP_PATHALCCA 0
#define PROPM_END_PATHAUDIOLINK 0
#define PROPM_END_PATHING 0
#define PROPM_START_MIRROROPTIONS 0
#define PROP_ENABLEMIRROROPTIONS 0
#define PROP_MIRROR 0
#define PROP_MIRRORTEXTUREUV 0
#define PROPM_END_MIRROROPTIONS 0
#define PROPM_START_DEPTHFX 0
#define PROP_ENABLETOUCHGLOW 0
#define PROP_DEPTHMASKUV 0
#define PROP_DEPTHCOLORTOGGLE 0
#define PROP_DEPTHCOLORBLENDMODE 0
#define PROP_DEPTHTEXTUREUV 0
#define PROP_DEPTHCOLORTHEMEINDEX 0
#define PROP_DEPTHEMISSIONSTRENGTH 0
#define PROP_DEPTHCOLORMINDEPTH 0
#define PROP_DEPTHCOLORMAXDEPTH 1
#define PROP_DEPTHCOLORMINVALUE 0
#define PROP_DEPTHCOLORMAXVALUE 1
#define PROP_DEPTHALPHATOGGLE 0
#define PROP_DEPTHALPHAMINDEPTH 0
#define PROP_DEPTHALPHAMAXDEPTH 1
#define PROP_DEPTHALPHAMINVALUE 1
#define PROP_DEPTHALPHAMAXVALUE 0
#define PROPM_END_DEPTHFX 0
#define PROPM_START_IRIDESCENCE 0
#define PROP_ENABLEIRIDESCENCE 0
#define PROP_IRIDESCENCEMASKUV 0
#define PROP_IRIDESCENCENORMALTOGGLE 0
#define PROP_IRIDESCENCENORMALINTENSITY 1
#define PROP_IRIDESCENCENORMALMAPUV 0
#define PROP_IRIDESCENCENORMALSELECTION 1
#define PROP_IRIDESCENCEINTENSITY 1
#define PROP_IRIDESCENCEADDBLEND 0
#define PROP_IRIDESCENCEREPLACEBLEND 0
#define PROP_IRIDESCENCEMULTIPLYBLEND 0
#define PROP_IRIDESCENCEEMISSIONSTRENGTH 0
#define PROP_IRIDESCENCEHUESHIFTENABLED 0
#define PROP_IRIDESCENCEHUESHIFTSPEED 0
#define PROP_IRIDESCENCEHUESHIFT 0
#define PROPM_START_IRIDESCENCEAUDIOLINK 0
#define PROP_IRIDESCENCEAUDIOLINKEMISSIONADDBAND 0
#define PROPM_END_IRIDESCENCEAUDIOLINK 0
#define PROPM_END_IRIDESCENCE 0
#define PROPM_START_TEXT 0
#define PROP_TEXTPIXELRANGE 4
#define PROP_TEXTENABLED 0
#define PROPM_START_TEXTFPS 0
#define PROP_TEXTFPSENABLED 0
#define PROP_TEXTFPSUV 0
#define PROP_TEXTFPSCOLORTHEMEINDEX 0
#define PROP_TEXTFPSEMISSIONSTRENGTH 0
#define PROP_TEXTFPSROTATION 0
#define PROPM_END_TEXTFPS 0
#define PROPM_START_TEXTPOSITION 0
#define PROP_TEXTPOSITIONENABLED 0
#define PROP_TEXTPOSITIONUV 0
#define PROP_TEXTPOSITIONCOLORTHEMEINDEX 0
#define PROP_TEXTPOSITIONEMISSIONSTRENGTH 0
#define PROP_TEXTPOSITIONROTATION 0
#define PROPM_END_TEXTPOSITION 0
#define PROPM_START_TEXTINSTANCETIME 0
#define PROP_TEXTTIMEENABLED 0
#define PROP_TEXTTIMEUV 0
#define PROP_TEXTTIMECOLORTHEMEINDEX 0
#define PROP_TEXTTIMEEMISSIONSTRENGTH 0
#define PROP_TEXTTIMEROTATION 0
#define PROPM_END_TEXTINSTANCETIME 0
#define PROPM_END_TEXT 0
#define PROPM_START_FXPROXIMITYCOLOR 0
#define PROP_FXPROXIMITYCOLOR 0
#define PROP_FXPROXIMITYCOLORTYPE 1
#define PROP_FXPROXIMITYCOLORMINCOLORTHEMEINDEX 0
#define PROP_FXPROXIMITYCOLORMAXCOLORTHEMEINDEX 0
#define PROP_FXPROXIMITYCOLORMINDISTANCE 0
#define PROP_FXPROXIMITYCOLORMAXDISTANCE 1
#define PROPM_END_FXPROXIMITYCOLOR 0
#define PROPM_AUDIOLINKCATEGORY 0
#define PROPM_START_AUDIOLINK 0
#define PROP_ENABLEAUDIOLINK 0
#define PROP_AUDIOLINKHELP 0
#define PROP_AUDIOLINKANIMTOGGLE 1
#define PROP_DEBUGWAVEFORM 0
#define PROP_DEBUGDFT 0
#define PROP_DEBUGBASS 0
#define PROP_DEBUGLOWMIDS 0
#define PROP_DEBUGHIGHMIDS 0
#define PROP_DEBUGTREBLE 0
#define PROP_DEBUGCCCOLORS 0
#define PROP_DEBUGCCSTRIP 0
#define PROP_DEBUGCCLIGHTS 0
#define PROP_DEBUGAUTOCORRELATOR 0
#define PROP_DEBUGCHRONOTENSITY 0
#define PROP_DEBUGVISUALIZERHELPBOX 0
#define PROPM_END_AUDIOLINK 0
#define PROPM_START_ALDECALSPECTRUM 0
#define PROP_ENABLEALDECAL 0
#define PROP_ALDECALTYPE 0
#define PROP_ALDECALUVMODE 0
#define PROP_ALDECALUV 0
#define PROP_ALUVROTATION 0
#define PROP_ALUVROTATIONSPEED 0
#define PROP_ALDECALLINEWIDTH 1
#define PROP_ALDECALVOLUMESTEP 0
#define PROP_ALDECALVOLUMECLIPMIN 0
#define PROP_ALDECALVOLUMECLIPMAX 1
#define PROP_ALDECALBANDSTEP 0
#define PROP_ALDECALBANDCLIPMIN 0
#define PROP_ALDECALBANDCLIPMAX 1
#define PROP_ALDECALSHAPECLIP 0
#define PROP_ALDECALSHAPECLIPVOLUMEWIDTH 0.5
#define PROP_ALDECALSHAPECLIPBANDWIDTH 0.5
#define PROP_ALDECALVOLUME 0.5
#define PROP_ALDECALBASEBOOST 5
#define PROP_ALDECALTREBLEBOOST 1
#define PROP_ALDECALCOLORMASKUV 0
#define PROP_ALDECALVOLUMECOLORSOURCE 1
#define PROP_ALDECALLOWEMISSION 0
#define PROP_ALDECALMIDEMISSION 0
#define PROP_ALDECALHIGHEMISSION 0
#define PROP_ALDECALBLENDTYPE 0
#define PROP_ALDECALBLENDALPHA 1
#define PROP_ALDECALCONTROLSALPHA 0
#define PROPM_END_ALDECALSPECTRUM 0
#define PROPM_MODIFIERCATEGORY 0
#define PROPM_START_UVDISTORTION 0
#define PROP_ENABLEDISTORTION 0
#define PROP_DISTORTIONUVTODISTORT 0
#define PROP_DISTORTIONMASKUV 0
#define PROP_DISTORTIONFLOWTEXTUREUV 0
#define PROP_DISTORTIONFLOWTEXTURE1UV 0
#define PROP_DISTORTIONSTRENGTH 0.5
#define PROP_DISTORTIONSTRENGTH1 0.5
#define PROPM_START_DISTORTIONAUDIOLINK 0
#define PROP_ENABLEDISTORTIONAUDIOLINK 0
#define PROP_DISTORTIONSTRENGTHAUDIOLINKBAND 0
#define PROP_DISTORTIONSTRENGTH1AUDIOLINKBAND 0
#define PROPM_END_DISTORTIONAUDIOLINK 0
#define PROPM_END_UVDISTORTION 0
#define PROPM_START_UVPANOSPHERE 0
#define PROP_STEREOENABLED 0
#define PROP_PANOUSEBOTHEYES 1
#define PROPM_END_UVPANOSPHERE 0
#define PROPM_START_UVPOLAR 0
#define PROP_POLARUV 0
#define PROP_POLARRADIALSCALE 1
#define PROP_POLARLENGTHSCALE 1
#define PROP_POLARSPIRALPOWER 0
#define PROPM_END_UVPOLAR 0
#define PROPM_START_PARALLAX 0
#define PROP_POIPARALLAX 0
#define PROP_PARALLAXUV 0
#define PROP_HEIGHTMAPUV 0
#define PROP_HEIGHTMASKINVERT 0
#define PROP_HEIGHTMASKUV 0
#define PROP_HEIGHTSTRENGTH 0.4247461
#define PROP_CURVATUREU 0
#define PROP_CURVATUREV 0
#define PROP_HEIGHTSTEPSMIN 10
#define PROP_HEIGHTSTEPSMAX 128
#define PROP_CURVFIX 1
#define PROPM_END_PARALLAX 0
#define PROPM_THIRDPARTYCATEGORY 0
#define PROPM_POSTPROCESSING 0
#define PROPM_START_POILIGHTDATA 0
#define PROP_PPHELP 0
#define PROP_PPLIGHTINGMULTIPLIER 1
#define PROP_PPLIGHTINGADDITION 0
#define PROP_PPEMISSIONMULTIPLIER 1
#define PROP_PPFINALCOLORMULTIPLIER 1
#define PROPM_END_POILIGHTDATA 0
#define PROPM_START_POSTPROCESS 0
#define PROP_POSTPROCESS 0
#define PROP_PPMASKINVERT 0
#define PROP_PPMASKUV 0
#define PROP_PPLUTSTRENGTH 0
#define PROP_PPHUE 0
#define PROP_PPCONTRAST 1
#define PROP_PPSATURATION 1
#define PROP_PPBRIGHTNESS 1
#define PROP_PPLIGHTNESS 0
#define PROP_PPHDR 0
#define PROPM_END_POSTPROCESS 0
#define PROPM_RENDERINGCATEGORY 0
#define PROP_CULL 2
#define PROP_ZTEST 4
#define PROP_ZWRITE 1
#define PROP_COLORMASK 15
#define PROP_OFFSETFACTOR 0
#define PROP_OFFSETUNITS 0
#define PROP_RENDERINGREDUCECLIPDISTANCE 0
#define PROP_IGNOREFOG 0
#define PROPINSTANCING 0
#define PROPM_START_BLENDING 0
#define PROP_BLENDOP 0
#define PROP_BLENDOPALPHA 0
#define PROP_SRCBLEND 1
#define PROP_DSTBLEND 0
#define PROP_ADDBLENDOP 0
#define PROP_ADDBLENDOPALPHA 0
#define PROP_ADDSRCBLEND 1
#define PROP_ADDDSTBLEND 1
#define PROPM_END_BLENDING 0
#define PROPM_START_STENCILPASSOPTIONS 0
#define PROP_STENCILREF 0
#define PROP_STENCILREADMASK 255
#define PROP_STENCILWRITEMASK 255
#define PROP_STENCILPASSOP 0
#define PROP_STENCILFAILOP 0
#define PROP_STENCILZFAILOP 0
#define PROP_STENCILCOMPAREFUNCTION 8
#define PROPM_END_STENCILPASSOPTIONS 0

			#pragma target 5.0
			#pragma skip_variants DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING DIRLIGHTMAP_COMBINED SHADOWS_SHADOWMASK
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#pragma multi_compile_instancing
			#pragma multi_compile_shadowcaster
			#pragma multi_compile_fog
			#define POI_PASS_SHADOW
			#include "UnityCG.cginc"
			#include "UnityStandardUtils.cginc"
			#include "AutoLight.cginc"
			#include "UnityLightingCommon.cginc"
			#include "UnityPBSLighting.cginc"
			#ifdef POI_PASS_META
			#include "UnityMetaPass.cginc"
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#define DielectricSpec float4(0.04, 0.04, 0.04, 1.0 - 0.04)
			#define PI float(3.14159265359)
			#define POI2D_SAMPLER_PAN(tex, texSampler, uv, pan) (UNITY_SAMPLE_TEX2D_SAMPLER(tex, texSampler, uv + _Time.x * pan))
			#define POI2D_SAMPLER_PANGRAD(tex, texSampler, uv, pan, ddx, ddy) (UNITY_SAMPLE_TEX2D_SAMPLER(tex, texSampler, uv + _Time.x * pan, ddx, ddy))
			#define POI2D_SAMPLER(tex, texSampler, uv) (UNITY_SAMPLE_TEX2D_SAMPLER(tex, texSampler, uv))
			#define POI2D_PAN(tex, uv, pan) (tex2D(tex, uv + _Time.x * pan))
			#define POI2D(tex, uv) (tex2D(tex, uv))
			#define POI_SAMPLE_TEX2D(tex, uv) (UNITY_SAMPLE_TEX2D(tex, uv))
			#define POI_SAMPLE_TEX2D_PAN(tex, uv, pan) (UNITY_SAMPLE_TEX2D(tex, uv + _Time.x * pan))
			#define POI2D_MAINTEX_SAMPLER_PAN_INLINED(tex, poiMesh) (POI2D_SAMPLER_PAN(tex, _MainTex, poiUV(poiMesh.uv[tex##UV], tex##_ST), tex##Pan))
			#define POI_SAFE_RGB0 float4(mainTexture.rgb * .0001, 0)
			#define POI_SAFE_RGB1 float4(mainTexture.rgb * .0001, 1)
			#define POI_SAFE_RGBA mainTexture
			#if defined(UNITY_COMPILER_HLSL)
			#define PoiInitStruct(type, name) name = (type)0;
			#else
			#define PoiInitStruct(type, name)
			#endif
			#define POI_ERROR(poiMesh, gridSize) lerp(float3(1, 0, 1), float3(0, 0, 0), fmod(floor((poiMesh.worldPos.x) * gridSize) + floor((poiMesh.worldPos.y) * gridSize) + floor((poiMesh.worldPos.z) * gridSize), 2) == 0)
			#define POI_MODE_OPAQUE 0
			#define POI_MODE_CUTOUT 1
			#define POI_MODE_FADE 2
			#define POI_MODE_TRANSPARENT 3
			#define POI_MODE_ADDITIVE 4
			#define POI_MODE_SOFTADDITIVE 5
			#define POI_MODE_MULTIPLICATIVE 6
			#define POI_MODE_2XMULTIPLICATIVE 7
			#define POI_MODE_TRANSCLIPPING 9
			#define ALPASS_DFT                      uint2(0,4)   //Size: 128, 2
			#define ALPASS_WAVEFORM                 uint2(0,6)   //Size: 128, 16
			#define ALPASS_AUDIOLINK                uint2(0,0)   //Size: 128, 4
			#define ALPASS_AUDIOBASS                uint2(0,0)   //Size: 128, 1
			#define ALPASS_AUDIOLOWMIDS             uint2(0,1)   //Size: 128, 1
			#define ALPASS_AUDIOHIGHMIDS            uint2(0,2)   //Size: 128, 1
			#define ALPASS_AUDIOTREBLE              uint2(0,3)   //Size: 128, 1
			#define ALPASS_AUDIOLINKHISTORY         uint2(1,0)   //Size: 127, 4
			#define ALPASS_GENERALVU                uint2(0,22)  //Size: 12, 1
			#define ALPASS_CCINTERNAL               uint2(12,22) //Size: 12, 2
			#define ALPASS_CCCOLORS                 uint2(25,22) //Size: 11, 1
			#define ALPASS_CCSTRIP                  uint2(0,24)  //Size: 128, 1
			#define ALPASS_CCLIGHTS                 uint2(0,25)  //Size: 128, 2
			#define ALPASS_AUTOCORRELATOR           uint2(0,27)  //Size: 128, 1
			#define ALPASS_GENERALVU_INSTANCE_TIME  uint2(2,22)
			#define ALPASS_GENERALVU_LOCAL_TIME     uint2(3,22)
			#define ALPASS_GENERALVU_NETWORK_TIME   uint2(4,22)
			#define ALPASS_GENERALVU_PLAYERINFO     uint2(6,22)
			#define ALPASS_FILTEREDAUDIOLINK        uint2(0,28)  //Size: 16, 4
			#define ALPASS_CHRONOTENSITY            uint2(16,28) //Size: 8, 4
			#define ALPASS_THEME_COLOR0             uint2(0,23)
			#define ALPASS_THEME_COLOR1             uint2(1,23)
			#define ALPASS_THEME_COLOR2             uint2(2,23)
			#define ALPASS_THEME_COLOR3             uint2(3,23)
			#define ALPASS_FILTEREDVU               uint2(24,28) //Size: 4, 4
			#define ALPASS_FILTEREDVU_INTENSITY     uint2(24,28) //Size: 4, 1
			#define ALPASS_FILTEREDVU_MARKER        uint2(24,29) //Size: 4, 1
			#define AUDIOLINK_SAMPHIST              3069        // Internal use for algos, do not change.
			#define AUDIOLINK_SAMPLEDATA24          2046
			#define AUDIOLINK_EXPBINS               24
			#define AUDIOLINK_EXPOCT                10
			#define AUDIOLINK_ETOTALBINS (AUDIOLINK_EXPBINS * AUDIOLINK_EXPOCT)
			#define AUDIOLINK_WIDTH                 128
			#define AUDIOLINK_SPS                   48000       // Samples per second
			#define AUDIOLINK_ROOTNOTE              0
			#define AUDIOLINK_4BAND_FREQFLOOR       0.123
			#define AUDIOLINK_4BAND_FREQCEILING     1
			#define AUDIOLINK_BOTTOM_FREQUENCY      13.75
			#define AUDIOLINK_BASE_AMPLITUDE        2.5
			#define AUDIOLINK_DELAY_COEFFICIENT_MIN 0.3
			#define AUDIOLINK_DELAY_COEFFICIENT_MAX 0.9
			#define AUDIOLINK_DFT_Q                 4.0
			#define AUDIOLINK_TREBLE_CORRECTION     5.0
			#define COLORCHORD_EMAXBIN              192
			#define COLORCHORD_IIR_DECAY_1          0.90
			#define COLORCHORD_IIR_DECAY_2          0.85
			#define COLORCHORD_CONSTANT_DECAY_1     0.01
			#define COLORCHORD_CONSTANT_DECAY_2     0.0
			#define COLORCHORD_NOTE_CLOSEST         3.0
			#define COLORCHORD_NEW_NOTE_GAIN        8.0
			#define COLORCHORD_MAX_NOTES            10
			#ifndef glsl_mod
			#define glsl_mod(x, y) (((x) - (y) * floor((x) / (y))))
			#endif
			uniform float4               _AudioTexture_TexelSize;
			#ifdef SHADER_TARGET_SURFACE_ANALYSIS
			#define AUDIOLINK_STANDARD_INDEXING
			#endif
			#ifdef AUDIOLINK_STANDARD_INDEXING
			sampler2D _AudioTexture;
			#define AudioLinkData(xycoord) tex2Dlod(_AudioTexture, float4(uint2(xycoord) * _AudioTexture_TexelSize.xy, 0, 0))
			#else
			uniform Texture2D<float4> _AudioTexture;
			SamplerState sampler_AudioTexture;
			#define AudioLinkData(xycoord) _AudioTexture[uint2(xycoord)]
			#endif
			float _Mode;
			float _StereoEnabled;
			float _PolarUV;
			float2 _PolarCenter;
			float _PolarRadialScale;
			float _PolarLengthScale;
			float _PolarSpiralPower;
			float _PanoUseBothEyes;
			float _IgnoreFog;
			float _RenderingReduceClipDistance;
			float4 _Color;
			float _ColorThemeIndex;
			UNITY_DECLARE_TEX2D(_MainTex);
			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
			float4 _MainTex_ST;
			float2 _MainTexPan;
			float _MainTexUV;
			float4 _MainTex_TexelSize;
			Texture2D _BumpMap;
			float4 _BumpMap_ST;
			float2 _BumpMapPan;
			float _BumpMapUV;
			float _BumpScale;
			Texture2D _ClippingMask;
			float4 _ClippingMask_ST;
			float2 _ClippingMaskPan;
			float _ClippingMaskUV;
			float _Inverse_Clipping;
			float _Cutoff;
			float _MainColorAdjustToggle;
			#if defined(PROP_MAINCOLORADJUSTTEXTURE) || !defined(OPTIMIZER_ENABLED)
			Texture2D _MainColorAdjustTexture;
			#endif
			float4 _MainColorAdjustTexture_ST;
			float2 _MainColorAdjustTexturePan;
			float _MainColorAdjustTextureUV;
			float _MainHueShiftToggle;
			float _MainHueShiftReplace;
			float _MainHueShift;
			float _MainHueShiftSpeed;
			float _Saturation;
			float _MainBrightness;
			float _MainHueALCTEnabled;
			float _MainALHueShiftBand;
			float _MainALHueShiftCTIndex;
			float _MainHueALMotionSpeed;
			SamplerState sampler_linear_clamp;
			SamplerState sampler_linear_repeat;
			float _AlphaForceOpaque;
			float _AlphaMod;
			float _AlphaPremultiply;
			float _AlphaToCoverage;
			float _AlphaSharpenedA2C;
			float _AlphaMipScale;
			float _AlphaDithering;
			float _AlphaDitherGradient;
			float _AlphaDistanceFade;
			float _AlphaDistanceFadeType;
			float _AlphaDistanceFadeMinAlpha;
			float _AlphaDistanceFadeMaxAlpha;
			float _AlphaDistanceFadeMin;
			float _AlphaDistanceFadeMax;
			float _AlphaFresnel;
			float _AlphaFresnelAlpha;
			float _AlphaFresnelSharpness;
			float _AlphaFresnelWidth;
			float _AlphaFresnelInvert;
			float _AlphaAngular;
			float _AngleType;
			float _AngleCompareTo;
			float3 _AngleForwardDirection;
			float _CameraAngleMin;
			float _CameraAngleMax;
			float _ModelAngleMin;
			float _ModelAngleMax;
			float _AngleMinAlpha;
			float _AlphaAudioLinkEnabled;
			float2 _AlphaAudioLinkAddRange;
			float _AlphaAudioLinkAddBand;
			float _MainVertexColoringLinearSpace;
			float _MainVertexColoring;
			float _MainUseVertexColorAlpha;
			#if defined(PROP_DEPTHMASK) || !defined(OPTIMIZER_ENABLED)
			Texture2D _DepthMask;
			#endif
			float4 _DepthMask_ST;
			float2 _DepthMaskPan;
			float _DepthMaskUV;
			float _DepthColorToggle;
			float _DepthColorBlendMode;
			#if defined(PROP_DEPTHTEXTURE) || !defined(OPTIMIZER_ENABLED)
			Texture2D _DepthTexture;
			#endif
			float4 _DepthTexture_ST;
			float2 _DepthTexturePan;
			float _DepthTextureUV;
			float3 _DepthColor;
			float _DepthColorThemeIndex;
			float _DepthColorMinDepth;
			float _DepthColorMaxDepth;
			float _DepthColorMinValue;
			float _DepthColorMaxValue;
			float _DepthEmissionStrength;
			float _DepthAlphaToggle;
			float _DepthAlphaMinValue;
			float _DepthAlphaMaxValue;
			float _DepthAlphaMinDepth;
			float _DepthAlphaMaxDepth;
			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float4 color : COLOR;
				float2 uv0 : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float2 uv2 : TEXCOORD2;
				float2 uv3 : TEXCOORD3;
				uint vertexId : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv[4] : TEXCOORD0;
				float3 objNormal : TEXCOORD4;
				float3 normal : TEXCOORD5;
				float3 tangent : TEXCOORD6;
				float3 binormal : TEXCOORD7;
				float4 worldPos : TEXCOORD8;
				float4 localPos : TEXCOORD9;
				float3 objectPos : TEXCOORD10;
				float4 vertexColor : TEXCOORD11;
				float4 lightmapUV : TEXCOORD12;
				float4 grabPos: TEXCOORD13;
				float4 worldDirection: TEXCOORD14;
				UNITY_SHADOW_COORDS(15)
				UNITY_FOG_COORDS(16)
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			struct PoiMesh
			{
				float3 normals[2];
				float3 objNormal;
				float3 tangentSpaceNormal;
				float3 binormal;
				float3 tangent;
				float3 worldPos;
				float3 localPos;
				float3 objectPosition;
				float isFrontFace;
				float4 vertexColor;
				float4 lightmapUV;
				float2 uv[8];
				float2 parallaxUV;
			};
			struct PoiCam
			{
				float3 viewDir;
				float3 forwardDir;
				float3 worldPos;
				float distanceToVert;
				float4 clipPos;
				float3 reflectionDir;
				float3 vertexReflectionDir;
				float3 tangentViewDir;
				float4 grabPos;
				float2 screenUV;
				float vDotN;
				float4 worldDirection;
			};
			struct PoiMods
			{
				float4 Mask;
				float4 audioLink;
				float audioLinkAvailable;
				float audioLinkVersion;
				float4 audioLinkTexture;
				float2 detailMask;
				float2 backFaceDetailIntensity;
				float globalEmission;
				float4 globalColorTheme[12];
				float ALTime[8];
			};
			struct PoiLight
			{
				float3 direction;
				float attenuation;
				float attenuationStrength;
				float3 directColor;
				float3 indirectColor;
				float occlusion;
				float shadowMask;
				float detailShadow;
				float3 halfDir;
				float lightMap;
				float3 rampedLightMap;
				float vertexNDotL;
				float nDotL;
				float nDotV;
				float vertexNDotV;
				float nDotH;
				float vertexNDotH;
				float lDotv;
				float lDotH;
				float nDotLSaturated;
				float nDotLNormalized;
				#ifdef UNITY_PASS_FORWARDADD
				float additiveShadow;
				#endif
				float3 finalLighting;
				float3 finalLightAdd;
				#if defined(VERTEXLIGHT_ON) && defined(POI_VERTEXLIGHT_ON)
				float4 vDotNL;
				float4 vertexVDotNL;
				float3 vColor[4];
				float4 vCorrectedDotNL;
				float4 vAttenuation;
				float4 vAttenuationDotNL;
				float3 vPosition[4];
				float3 vDirection[4];
				float3 vFinalLighting;
				float3 vHalfDir[4];
				half4 vDotNH;
				half4 vertexVDotNH;
				half4 vDotLH;
				#endif
			};
			struct PoiVertexLights
			{
				float3 direction;
				float3 color;
				float attenuation;
			};
			struct PoiFragData
			{
				float3 baseColor;
				float3 finalColor;
				float alpha;
				float3 emission;
			};
			float2 poiUV(float2 uv, float4 tex_st)
			{
				return uv * tex_st.xy + tex_st.zw;
			}
			float calculateluminance(float3 color)
			{
				return color.r * 0.299 + color.g * 0.587 + color.b * 0.114;
			}
			bool IsInMirror()
			{
				return unity_CameraProjection[2][0] != 0.f || unity_CameraProjection[2][1] != 0.f;
			}
			bool IsOrthographicCamera()
			{
				return unity_OrthoParams.w == 1 || UNITY_MATRIX_P[3][3] == 1;
			}
			float shEvaluateDiffuseL1Geomerics_local(float L0, float3 L1, float3 n)
			{
				float R0 = max(0, L0);
				float3 R1 = 0.5f * L1;
				float lenR1 = length(R1);
				float q = dot(normalize(R1), n) * 0.5 + 0.5;
				q = saturate(q); // Thanks to ScruffyRuffles for the bug identity.
				float p = 1.0f + 2.0f * lenR1 / R0;
				float a = (1.0f - lenR1 / R0) / (1.0f + lenR1 / R0);
				return R0 * (a + (1.0f - a) * (p + 1.0f) * pow(q, p));
			}
			half3 BetterSH9(half4 normal)
			{
				float3 indirect;
				float3 L0 = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w) + float3(unity_SHBr.z, unity_SHBg.z, unity_SHBb.z) / 3.0;
				indirect.r = shEvaluateDiffuseL1Geomerics_local(L0.r, unity_SHAr.xyz, normal.xyz);
				indirect.g = shEvaluateDiffuseL1Geomerics_local(L0.g, unity_SHAg.xyz, normal.xyz);
				indirect.b = shEvaluateDiffuseL1Geomerics_local(L0.b, unity_SHAb.xyz, normal.xyz);
				indirect = max(0, indirect);
				indirect += SHEvalLinearL2(normal);
				return indirect;
			}
			float3 getCameraForward()
			{
				#if UNITY_SINGLE_PASS_STEREO
				float3 p1 = mul(unity_StereoCameraToWorld[0], float4(0, 0, 1, 1));
				float3 p2 = mul(unity_StereoCameraToWorld[0], float4(0, 0, 0, 1));
				#else
				float3 p1 = mul(unity_CameraToWorld, float4(0, 0, 1, 1)).xyz;
				float3 p2 = mul(unity_CameraToWorld, float4(0, 0, 0, 1)).xyz;
				#endif
				return normalize(p2 - p1);
			}
			half3 GetSHLength()
			{
				half3 x, x1;
				x.r = length(unity_SHAr);
				x.g = length(unity_SHAg);
				x.b = length(unity_SHAb);
				x1.r = length(unity_SHBr);
				x1.g = length(unity_SHBg);
				x1.b = length(unity_SHBb);
				return x + x1;
			}
			float3 BoxProjection(float3 direction, float3 position, float4 cubemapPosition, float3 boxMin, float3 boxMax)
			{
				#if UNITY_SPECCUBE_BOX_PROJECTION
				if (cubemapPosition.w > 0)
				{
					float3 factors = ((direction > 0 ? boxMax : boxMin) - position) / direction;
					float scalar = min(min(factors.x, factors.y), factors.z);
					direction = direction * scalar + (position - cubemapPosition.xyz);
				}
				#endif
				return direction;
			}
			float poiMax(float2 i)
			{
				return max(i.x, i.y);
			}
			float poiMax(float3 i)
			{
				return max(max(i.x, i.y), i.z);
			}
			float poiMax(float4 i)
			{
				return max(max(max(i.x, i.y), i.z), i.w);
			}
			float3 calculateNormal(in float3 baseNormal, in PoiMesh poiMesh, in Texture2D normalTexture, in float4 normal_ST, in float2 normalPan, in float normalUV, in float normalIntensity)
			{
				float3 normal = UnpackScaleNormal(POI2D_SAMPLER_PAN(normalTexture, _MainTex, poiUV(poiMesh.uv[normalUV], normal_ST), normalPan), normalIntensity);
				return normalize(
				normal.x * poiMesh.tangent +
				normal.y * poiMesh.binormal +
				normal.z * baseNormal
				);
			}
			float remap(float x, float minOld, float maxOld, float minNew = 0, float maxNew = 1)
			{
				return minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld);
			}
			float2 remap(float2 x, float2 minOld, float2 maxOld, float2 minNew = 0, float2 maxNew = 1)
			{
				return minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld);
			}
			float3 remap(float3 x, float3 minOld, float3 maxOld, float3 minNew = 0, float3 maxNew = 1)
			{
				return minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld);
			}
			float4 remap(float4 x, float4 minOld, float4 maxOld, float4 minNew = 0, float4 maxNew = 1)
			{
				return minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld);
			}
			float remapClamped(float minOld, float maxOld, float x, float minNew = 0, float maxNew = 1)
			{
				return clamp(minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld), minNew, maxNew);
			}
			float2 remapClamped(float2 minOld, float2 maxOld, float2 x, float2 minNew, float2 maxNew)
			{
				return clamp(minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld), minNew, maxNew);
			}
			float3 remapClamped(float3 minOld, float3 maxOld, float3 x, float3 minNew, float3 maxNew)
			{
				return clamp(minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld), minNew, maxNew);
			}
			float4 remapClamped(float4 minOld, float4 maxOld, float4 x, float4 minNew, float4 maxNew)
			{
				return clamp(minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld), minNew, maxNew);
			}
			float2 calcParallax(in float height, in PoiCam poiCam)
			{
				return ((height * - 1) + 1) * (poiCam.tangentViewDir.xy / poiCam.tangentViewDir.z);
			}
			float4 poiBlend(const float sourceFactor, const  float4 sourceColor, const  float destinationFactor, const  float4 destinationColor, const float4 blendFactor)
			{
				float4 sA = 1 - blendFactor;
				const float4 blendData[11] = {
					float4(0.0, 0.0, 0.0, 0.0),
					float4(1.0, 1.0, 1.0, 1.0),
					destinationColor,
					sourceColor,
					float4(1.0, 1.0, 1.0, 1.0) - destinationColor,
					sA,
					float4(1.0, 1.0, 1.0, 1.0) - sourceColor,
					sA,
					float4(1.0, 1.0, 1.0, 1.0) - sA,
					saturate(sourceColor.aaaa),
					1 - sA,
				};
				return lerp(blendData[sourceFactor] * sourceColor + blendData[destinationFactor] * destinationColor, sourceColor, sA);
			}
			float3 blendAverage(float3 base, float3 blend)
			{
				return (base + blend) / 2.0;
			}
			float blendColorBurn(float base, float blend)
			{
				return (blend == 0.0)?blend : max((1.0 - ((1.0 - base) / blend)), 0.0);
			}
			float3 blendColorBurn(float3 base, float3 blend)
			{
				return float3(blendColorBurn(base.r, blend.r), blendColorBurn(base.g, blend.g), blendColorBurn(base.b, blend.b));
			}
			float blendColorDodge(float base, float blend)
			{
				return (blend == 1.0)?blend : min(base / (1.0 - blend), 1.0);
			}
			float3 blendColorDodge(float3 base, float3 blend)
			{
				return float3(blendColorDodge(base.r, blend.r), blendColorDodge(base.g, blend.g), blendColorDodge(base.b, blend.b));
			}
			float blendDarken(float base, float blend)
			{
				return min(blend, base);
			}
			float3 blendDarken(float3 base, float3 blend)
			{
				return float3(blendDarken(base.r, blend.r), blendDarken(base.g, blend.g), blendDarken(base.b, blend.b));
			}
			float3 blendExclusion(float3 base, float3 blend)
			{
				return base + blend - 2.0 * base * blend;
			}
			float blendReflect(float base, float blend)
			{
				return (blend == 1.0)?blend : min(base * base / (1.0 - blend), 1.0);
			}
			float3 blendReflect(float3 base, float3 blend)
			{
				return float3(blendReflect(base.r, blend.r), blendReflect(base.g, blend.g), blendReflect(base.b, blend.b));
			}
			float3 blendGlow(float3 base, float3 blend)
			{
				return blendReflect(blend, base);
			}
			float blendOverlay(float base, float blend)
			{
				return base < 0.5?(2.0 * base * blend) : (1.0 - 2.0 * (1.0 - base) * (1.0 - blend));
			}
			float3 blendOverlay(float3 base, float3 blend)
			{
				return float3(blendOverlay(base.r, blend.r), blendOverlay(base.g, blend.g), blendOverlay(base.b, blend.b));
			}
			float3 blendHardLight(float3 base, float3 blend)
			{
				return blendOverlay(blend, base);
			}
			float blendVividLight(float base, float blend)
			{
				return (blend < 0.5)?blendColorBurn(base, (2.0 * blend)) : blendColorDodge(base, (2.0 * (blend - 0.5)));
			}
			float3 blendVividLight(float3 base, float3 blend)
			{
				return float3(blendVividLight(base.r, blend.r), blendVividLight(base.g, blend.g), blendVividLight(base.b, blend.b));
			}
			float blendHardMix(float base, float blend)
			{
				return (blendVividLight(base, blend) < 0.5)?0.0 : 1.0;
			}
			float3 blendHardMix(float3 base, float3 blend)
			{
				return float3(blendHardMix(base.r, blend.r), blendHardMix(base.g, blend.g), blendHardMix(base.b, blend.b));
			}
			float blendLighten(float base, float blend)
			{
				return max(blend, base);
			}
			float3 blendLighten(float3 base, float3 blend)
			{
				return float3(blendLighten(base.r, blend.r), blendLighten(base.g, blend.g), blendLighten(base.b, blend.b));
			}
			float blendLinearBurn(float base, float blend)
			{
				return max(base + blend - 1.0, 0.0);
			}
			float3 blendLinearBurn(float3 base, float3 blend)
			{
				return max(base + blend - float3(1.0, 1.0, 1.0), float3(0.0, 0.0, 0.0));
			}
			float blendLinearDodge(float base, float blend)
			{
				return min(base + blend, 1.0);
			}
			float3 blendLinearDodge(float3 base, float3 blend)
			{
				return min(base + blend, float3(1.0, 1.0, 1.0));
			}
			float blendLinearLight(float base, float blend)
			{
				return blend < 0.5?blendLinearBurn(base, (2.0 * blend)) : blendLinearDodge(base, (2.0 * (blend - 0.5)));
			}
			float3 blendLinearLight(float3 base, float3 blend)
			{
				return float3(blendLinearLight(base.r, blend.r), blendLinearLight(base.g, blend.g), blendLinearLight(base.b, blend.b));
			}
			float3 blendMultiply(float3 base, float3 blend)
			{
				return base * blend;
			}
			float3 blendNegation(float3 base, float3 blend)
			{
				return float3(1.0, 1.0, 1.0) - abs(float3(1.0, 1.0, 1.0) - base - blend);
			}
			float3 blendNormal(float3 base, float3 blend)
			{
				return blend;
			}
			float3 blendPhoenix(float3 base, float3 blend)
			{
				return min(base, blend) - max(base, blend) + float3(1.0, 1.0, 1.0);
			}
			float blendPinLight(float base, float blend)
			{
				return (blend < 0.5)?blendDarken(base, (2.0 * blend)) : blendLighten(base, (2.0 * (blend - 0.5)));
			}
			float3 blendPinLight(float3 base, float3 blend)
			{
				return float3(blendPinLight(base.r, blend.r), blendPinLight(base.g, blend.g), blendPinLight(base.b, blend.b));
			}
			float blendScreen(float base, float blend)
			{
				return 1.0 - ((1.0 - base) * (1.0 - blend));
			}
			float3 blendScreen(float3 base, float3 blend)
			{
				return float3(blendScreen(base.r, blend.r), blendScreen(base.g, blend.g), blendScreen(base.b, blend.b));
			}
			float blendSoftLight(float base, float blend)
			{
				return (blend < 0.5)?(2.0 * base * blend + base * base * (1.0 - 2.0 * blend)) : (sqrt(base) * (2.0 * blend - 1.0) + 2.0 * base * (1.0 - blend));
			}
			float3 blendSoftLight(float3 base, float3 blend)
			{
				return float3(blendSoftLight(base.r, blend.r), blendSoftLight(base.g, blend.g), blendSoftLight(base.b, blend.b));
			}
			float blendSubtract(float base, float blend)
			{
				return max(base - blend, 0.0);
			}
			float3 blendSubtract(float3 base, float3 blend)
			{
				return max(base - blend, 0.0);
			}
			float blendDifference(float base, float blend)
			{
				return abs(base - blend);
			}
			float3 blendDifference(float3 base, float3 blend)
			{
				return abs(base - blend);
			}
			float blendDivide(float base, float blend)
			{
				return base / max(blend, 0.0001);
			}
			float3 blendDivide(float3 base, float3 blend)
			{
				return base / max(blend, 0.0001);
			}
			float3 customBlend(float3 base, float3 blend, float blendType)
			{
				float3 ret = 0;
				switch(blendType)
				{
					case 0:
					{
						ret = blendNormal(base, blend);
						break;
					}
					case 1:
					{
						ret = blendDarken(base, blend);
						break;
					}
					case 2:
					{
						ret = blendMultiply(base, blend);
						break;
					}
					case 3:
					{
						ret = blendColorBurn(base, blend);
						break;
					}
					case 4:
					{
						ret = blendLinearBurn(base, blend);
						break;
					}
					case 5:
					{
						ret = blendLighten(base, blend);
						break;
					}
					case 6:
					{
						ret = blendScreen(base, blend);
						break;
					}
					case 7:
					{
						ret = blendColorDodge(base, blend);
						break;
					}
					case 8:
					{
						ret = blendLinearDodge(base, blend);
						break;
					}
					case 9:
					{
						ret = blendOverlay(base, blend);
						break;
					}
					case 10:
					{
						ret = blendSoftLight(base, blend);
						break;
					}
					case 11:
					{
						ret = blendHardLight(base, blend);
						break;
					}
					case 12:
					{
						ret = blendVividLight(base, blend);
						break;
					}
					case 13:
					{
						ret = blendLinearLight(base, blend);
						break;
					}
					case 14:
					{
						ret = blendPinLight(base, blend);
						break;
					}
					case 15:
					{
						ret = blendHardMix(base, blend);
						break;
					}
					case 16:
					{
						ret = blendDifference(base, blend);
						break;
					}
					case 17:
					{
						ret = blendExclusion(base, blend);
						break;
					}
					case 18:
					{
						ret = blendSubtract(base, blend);
						break;
					}
					case 19:
					{
						ret = blendDivide(base, blend);
						break;
					}
				}
				return ret;
			}
			float random(float2 p)
			{
				return frac(sin(dot(p, float2(12.9898, 78.2383))) * 43758.5453123);
			}
			float2 random2(float2 p)
			{
				return frac(sin(float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)))) * 43758.5453);
			}
			float3 random3(float3 p)
			{
				return frac(sin(float3(dot(p, float3(127.1, 311.7, 248.6)), dot(p, float3(269.5, 183.3, 423.3)), dot(p, float3(248.3, 315.9, 184.2)))) * 43758.5453);
			}
			float3 randomFloat3(float2 Seed, float maximum)
			{
				return (.5 + float3(
				frac(sin(dot(Seed.xy, float2(12.9898, 78.233))) * 43758.5453),
				frac(sin(dot(Seed.yx, float2(12.9898, 78.233))) * 43758.5453),
				frac(sin(dot(float2(Seed), float2(12.9898, 78.233))) * 43758.5453)
				) * .5) * (maximum);
			}
			float3 randomFloat3Range(float2 Seed, float Range)
			{
				return (float3(
				frac(sin(dot(Seed.xy, float2(12.9898, 78.233))) * 43758.5453),
				frac(sin(dot(Seed.yx, float2(12.9898, 78.233))) * 43758.5453),
				frac(sin(dot(float2(Seed.x * Seed.y, Seed.y + Seed.x), float2(12.9898, 78.233))) * 43758.5453)
				) * 2 - 1) * Range;
			}
			float3 randomFloat3WiggleRange(float2 Seed, float Range, float wiggleSpeed)
			{
				float3 rando = (float3(
				frac(sin(dot(Seed.xy, float2(12.9898, 78.233))) * 43758.5453),
				frac(sin(dot(Seed.yx, float2(12.9898, 78.233))) * 43758.5453),
				frac(sin(dot(float2(Seed.x * Seed.y, Seed.y + Seed.x), float2(12.9898, 78.233))) * 43758.5453)
				) * 2 - 1);
				float speed = 1 + wiggleSpeed;
				return float3(sin((_Time.x + rando.x * PI) * speed), sin((_Time.x + rando.y * PI) * speed), sin((_Time.x + rando.z * PI) * speed)) * Range;
			}
			void Unity_RandomRange_float(float2 Seed, float Min, float Max, out float Out)
			{
				float randomno = frac(sin(dot(Seed, float2(12.9898, 78.233))) * 43758.5453);
				Out = lerp(Min, Max, randomno);
			}
			void poiChannelMixer(float3 In, float3 _ChannelMixer_Red, float3 _ChannelMixer_Green, float3 _ChannelMixer_Blue, out float3 Out)
			{
				Out = float3(dot(In, _ChannelMixer_Red), dot(In, _ChannelMixer_Green), dot(In, _ChannelMixer_Blue));
			}
			void poiContrast(float3 In, float Contrast, out float3 Out)
			{
				float midpoint = pow(0.5, 2.2);
				Out = (In - midpoint) * Contrast + midpoint;
			}
			void poiInvertColors(float4 In, float4 InvertColors, out float4 Out)
			{
				Out = abs(InvertColors - In);
			}
			void poiReplaceColor(float3 In, float3 From, float3 To, float Range, float Fuzziness, out float3 Out)
			{
				float Distance = distance(From, In);
				Out = lerp(To, In, saturate((Distance - Range) / max(Fuzziness, 0.00001)));
			}
			void poiSaturation(float3 In, float Saturation, out float3 Out)
			{
				float luma = dot(In, float3(0.2126729, 0.7151522, 0.0721750));
				Out = luma.xxx + Saturation.xxx * (In - luma.xxx);
			}
			void poiDither(float4 In, float4 ScreenPosition, out float4 Out)
			{
				float2 uv = ScreenPosition.xy * _ScreenParams.xy;
				float DITHER_THRESHOLDS[16] = {
					1.0 / 17.0, 9.0 / 17.0, 3.0 / 17.0, 11.0 / 17.0,
					13.0 / 17.0, 5.0 / 17.0, 15.0 / 17.0, 7.0 / 17.0,
					4.0 / 17.0, 12.0 / 17.0, 2.0 / 17.0, 10.0 / 17.0,
					16.0 / 17.0, 8.0 / 17.0, 14.0 / 17.0, 6.0 / 17.0
				};
				uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
				Out = In - DITHER_THRESHOLDS[index];
			}
			void poiColorMask(float3 In, float3 MaskColor, float Range, float Fuzziness, out float4 Out)
			{
				float Distance = distance(MaskColor, In);
				Out = saturate(1 - (Distance - Range) / max(Fuzziness, 0.00001));
			}
			static const float Epsilon = 1e-10;
			static const float3 HCYwts = float3(0.299, 0.587, 0.114);
			static const float HCLgamma = 3;
			static const float HCLy0 = 100;
			static const float HCLmaxL = 0.530454533953517; // == exp(HCLgamma / HCLy0) - 0.5
			static const float3 wref = float3(1.0, 1.0, 1.0);
			#define TAU 6.28318531
			float3 HUEtoRGB(in float H)
			{
				float R = abs(H * 6 - 3) - 1;
				float G = 2 - abs(H * 6 - 2);
				float B = 2 - abs(H * 6 - 4);
				return saturate(float3(R, G, B));
			}
			float3 RGBtoHCV(in float3 RGB)
			{
				float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0 / 3.0) : float4(RGB.gb, 0.0, -1.0 / 3.0);
				float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
				float C = Q.x - min(Q.w, Q.y);
				float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
				return float3(H, C, Q.x);
			}
			float3 HSVtoRGB(in float3 HSV)
			{
				float3 RGB = HUEtoRGB(HSV.x);
				return ((RGB - 1) * HSV.y + 1) * HSV.z;
			}
			float3 RGBtoHSV(in float3 RGB)
			{
				float3 HCV = RGBtoHCV(RGB);
				float S = HCV.y / (HCV.z + Epsilon);
				return float3(HCV.x, S, HCV.z);
			}
			float3 HSLtoRGB(in float3 HSL)
			{
				float3 RGB = HUEtoRGB(HSL.x);
				float C = (1 - abs(2 * HSL.z - 1)) * HSL.y;
				return (RGB - 0.5) * C + HSL.z;
			}
			float3 RGBtoHSL(in float3 RGB)
			{
				float3 HCV = RGBtoHCV(RGB);
				float L = HCV.z - HCV.y * 0.5;
				float S = HCV.y / (1 - abs(L * 2 - 1) + Epsilon);
				return float3(HCV.x, S, L);
			}
			float3 hueShift(float3 color, float hueOffset)
			{
				color = RGBtoHSV(color);
				color.x = frac(hueOffset +color.x);
				return HSVtoRGB(color);
			}
			float3 hueShiftClamped(float3 color, float hueOffset, float saturationOffset = 0, float valueOffset = 0)
			{
				color = RGBtoHSV(color);
				color.x = frac(hueOffset +color.x);
				color.y = saturate(saturationOffset +color.y);
				color.z = saturate(valueOffset +color.z);
				return HSVtoRGB(color);
			}
			float3 ModifyViaHSL(float3 color, float3 HSLMod)
			{
				float3 colorHSL = RGBtoHSL(color);
				colorHSL.r = frac(colorHSL.r + HSLMod.r);
				colorHSL.g = saturate(colorHSL.g + HSLMod.g);
				colorHSL.b = saturate(colorHSL.b + HSLMod.b);
				return HSLtoRGB(colorHSL);
			}
			float3 poiSaturation(float3 In, float Saturation)
			{
				float luma = dot(In, float3(0.2126729, 0.7151522, 0.0721750));
				return luma.xxx + Saturation.xxx * (In - luma.xxx);
			}
			float xyzF(float t)
			{
				return lerp(pow(t, 1. / 3.), 7.787037 * t + 0.139731, step(t, 0.00885645));
			}
			float xyzR(float t)
			{
				return lerp(t * t * t, 0.1284185 * (t - 0.139731), step(t, 0.20689655));
			}
			float3 rgb2lch(in float3 c)
			{
				c = mul(float3x3(0.4124, 0.3576, 0.1805,
				0.2126, 0.7152, 0.0722,
				0.0193, 0.1192, 0.9505), c);
				c.x = xyzF(c.x / wref.x);
				c.y = xyzF(c.y / wref.y);
				c.z = xyzF(c.z / wref.z);
				float3 lab = float3(max(0., 116.0 * c.y - 16.0), 500.0 * (c.x - c.y), 200.0 * (c.y - c.z));
				return float3(lab.x, length(float2(lab.y, lab.z)), atan2(lab.z, lab.y));
			}
			float3 lch2rgb(in float3 c)
			{
				c = float3(c.x, cos(c.z) * c.y, sin(c.z) * c.y);
				float lg = 1. / 116. * (c.x + 16.);
				float3 xyz = float3(wref.x * xyzR(lg + 0.002 * c.y),
				wref.y * xyzR(lg),
				wref.z * xyzR(lg - 0.005 * c.z));
				float3 rgb = mul(float3x3(3.2406, -1.5372, -0.4986,
				- 0.9689, 1.8758, 0.0415,
				0.0557, -0.2040, 1.0570), xyz);
				return rgb;
			}
			float lerpAng(in float a, in float b, in float x)
			{
				float ang = fmod(fmod((a - b), TAU) + PI * 3., TAU) - PI;
				return ang * x + b;
			}
			float3 lerpLch(in float3 a, in float3 b, in float x)
			{
				float hue = lerpAng(a.z, b.z, x);
				return float3(lerp(b.xy, a.xy, x), hue);
			}
			float3 poiExpensiveColorBlend(float3 col1, float3 col2, float alpha)
			{
				return lch2rgb(lerpLch(rgb2lch(col1), rgb2lch(col2), alpha));
			}
			float4x4 poiAngleAxisRotationMatrix(float angle, float3 axis)
			{
				axis = normalize(axis);
				float s = sin(angle);
				float c = cos(angle);
				float oc = 1.0 - c;
				return float4x4(oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s, 0.0,
				oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c, oc * axis.y * axis.z - axis.x * s, 0.0,
				oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c, 0.0,
				0.0, 0.0, 0.0, 1.0);
			}
			float4x4 poiRotationMatrixFromAngles(float x, float y, float z)
			{
				float angleX = radians(x);
				float c = cos(angleX);
				float s = sin(angleX);
				float4x4 rotateXMatrix = float4x4(1, 0, 0, 0,
				0, c, -s, 0,
				0, s, c, 0,
				0, 0, 0, 1);
				float angleY = radians(y);
				c = cos(angleY);
				s = sin(angleY);
				float4x4 rotateYMatrix = float4x4(c, 0, s, 0,
				0, 1, 0, 0,
				- s, 0, c, 0,
				0, 0, 0, 1);
				float angleZ = radians(z);
				c = cos(angleZ);
				s = sin(angleZ);
				float4x4 rotateZMatrix = float4x4(c, -s, 0, 0,
				s, c, 0, 0,
				0, 0, 1, 0,
				0, 0, 0, 1);
				return mul(mul(rotateXMatrix, rotateYMatrix), rotateZMatrix);
			}
			float4x4 poiRotationMatrixFromAngles(float3 angles)
			{
				float angleX = radians(angles.x);
				float c = cos(angleX);
				float s = sin(angleX);
				float4x4 rotateXMatrix = float4x4(1, 0, 0, 0,
				0, c, -s, 0,
				0, s, c, 0,
				0, 0, 0, 1);
				float angleY = radians(angles.y);
				c = cos(angleY);
				s = sin(angleY);
				float4x4 rotateYMatrix = float4x4(c, 0, s, 0,
				0, 1, 0, 0,
				- s, 0, c, 0,
				0, 0, 0, 1);
				float angleZ = radians(angles.z);
				c = cos(angleZ);
				s = sin(angleZ);
				float4x4 rotateZMatrix = float4x4(c, -s, 0, 0,
				s, c, 0, 0,
				0, 0, 1, 0,
				0, 0, 0, 1);
				return mul(mul(rotateXMatrix, rotateYMatrix), rotateZMatrix);
			}
			float3 getCameraPosition()
			{
				#ifdef USING_STEREO_MATRICES
				return lerp(unity_StereoWorldSpaceCameraPos[0], unity_StereoWorldSpaceCameraPos[1], 0.5);
				#endif
				return _WorldSpaceCameraPos;
			}
			half2 calcScreenUVs(half4 grabPos)
			{
				half2 uv = grabPos.xy / (grabPos.w + 0.0000000001);
				#if UNITY_SINGLE_PASS_STEREO
				uv.xy *= half2(_ScreenParams.x * 2, _ScreenParams.y);
				#else
				uv.xy *= _ScreenParams.xy;
				#endif
				return uv;
			}
			float CalcMipLevel(float2 texture_coord)
			{
				float2 dx = ddx(texture_coord);
				float2 dy = ddy(texture_coord);
				float delta_max_sqr = max(dot(dx, dx), dot(dy, dy));
				return 0.5 * log2(delta_max_sqr);
			}
			float inverseLerp(float A, float B, float T)
			{
				return (T - A) / (B - A);
			}
			float inverseLerp2(float2 a, float2 b, float2 value)
			{
				float2 AB = b - a;
				float2 AV = value - a;
				return dot(AV, AB) / dot(AB, AB);
			}
			float inverseLerp3(float3 a, float3 b, float3 value)
			{
				float3 AB = b - a;
				float3 AV = value - a;
				return dot(AV, AB) / dot(AB, AB);
			}
			float inverseLerp4(float4 a, float4 b, float4 value)
			{
				float4 AB = b - a;
				float4 AV = value - a;
				return dot(AV, AB) / dot(AB, AB);
			}
			float4 quaternion_conjugate(float4 v)
			{
				return float4(
				v.x, -v.yzw
				);
			}
			float4 quaternion_mul(float4 v1, float4 v2)
			{
				float4 result1 = (v1.x * v2 + v1 * v2.x);
				float4 result2 = float4(
				- dot(v1.yzw, v2.yzw),
				cross(v1.yzw, v2.yzw)
				);
				return float4(result1 + result2);
			}
			float4 get_quaternion_from_angle(float3 axis, float angle)
			{
				float sn = sin(angle * 0.5);
				float cs = cos(angle * 0.5);
				return float4(axis * sn, cs);
			}
			float4 quaternion_from_vector(float3 inVec)
			{
				return float4(0.0, inVec);
			}
			float degree_to_radius(float degree)
			{
				return (
				degree / 180.0 * PI
				);
			}
			float3 rotate_with_quaternion(float3 inVec, float3 rotation)
			{
				float4 qx = get_quaternion_from_angle(float3(1, 0, 0), radians(rotation.x));
				float4 qy = get_quaternion_from_angle(float3(0, 1, 0), radians(rotation.y));
				float4 qz = get_quaternion_from_angle(float3(0, 0, 1), radians(rotation.z));
				#define MUL3(A, B, C) quaternion_mul(quaternion_mul((A), (B)), (C))
				float4 quaternion = normalize(MUL3(qx, qy, qz));
				float4 conjugate = quaternion_conjugate(quaternion);
				float4 inVecQ = quaternion_from_vector(inVec);
				float3 rotated = (
				MUL3(quaternion, inVecQ, conjugate)
				).yzw;
				return rotated;
			}
			float4 transform(float4 input, float4 pos, float4 rotation, float4 scale)
			{
				input.rgb *= (scale.xyz * scale.w);
				input = float4(rotate_with_quaternion(input.xyz, rotation.xyz * rotation.w) + (pos.xyz * pos.w), input.w);
				return input;
			}
			float aaBlurStep(float gradient, float edge, float blur)
			{
				float edgeMin = saturate(edge);
				float edgeMax = saturate(edge + blur * (1 - edge));
				return smoothstep(0, 1, saturate((gradient - edgeMin) / saturate(edgeMax - edgeMin + fwidth(gradient))));
			}
			float3 poiThemeColor(in PoiMods poiMods, in float3 srcColor, in float themeIndex)
			{
				if (themeIndex == 0) return srcColor;
				themeIndex -= 1;
				if (themeIndex <= 3)
				{
					return poiMods.globalColorTheme[themeIndex];
				}
				return srcColor;
			}
			float lilIsIn0to1(float f)
			{
				float value = 0.5 - abs(f - 0.5);
				return saturate(value / clamp(fwidth(value), 0.0001, 1.0));
			}
			float lilIsIn0to1(float f, float nv)
			{
				float value = 0.5 - abs(f - 0.5);
				return saturate(value / clamp(fwidth(value), 0.0001, nv));
			}
			float lilTooningNoSaturate(float value, float border)
			{
				return (value - border) / clamp(fwidth(value), 0.0001, 1.0);
			}
			float lilTooningNoSaturate(float value, float border, float blur)
			{
				float borderMin = saturate(border - blur * 0.5);
				float borderMax = saturate(border + blur * 0.5);
				return (value - borderMin) / saturate(borderMax - borderMin + fwidth(value));
			}
			float lilTooningNoSaturate(float value, float border, float blur, float borderRange)
			{
				float borderMin = saturate(border - blur * 0.5 - borderRange);
				float borderMax = saturate(border + blur * 0.5);
				return (value - borderMin) / saturate(borderMax - borderMin + fwidth(value));
			}
			float lilTooning(float value, float border)
			{
				return saturate(lilTooningNoSaturate(value, border));
			}
			float lilTooning(float value, float border, float blur)
			{
				return saturate(lilTooningNoSaturate(value, border, blur));
			}
			float lilTooning(float value, float border, float blur, float borderRange)
			{
				return saturate(lilTooningNoSaturate(value, border, blur, borderRange));
			}
			inline float4 CalculateFrustumCorrection()
			{
				float x1 = -UNITY_MATRIX_P._31 / (UNITY_MATRIX_P._11 * UNITY_MATRIX_P._34);
				float x2 = -UNITY_MATRIX_P._32 / (UNITY_MATRIX_P._22 * UNITY_MATRIX_P._34);
				return float4(x1, x2, 0, UNITY_MATRIX_P._33 / UNITY_MATRIX_P._34 + x1 * UNITY_MATRIX_P._13 + x2 * UNITY_MATRIX_P._23);
			}
			inline float CorrectedLinearEyeDepth(float z, float B)
			{
				return 1.0 / (z / UNITY_MATRIX_P._34 + B);
			}
			v2f vert(appdata v)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				v2f o;
				PoiInitStruct(v2f, o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.objectPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
				o.objNormal = v.normal;
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.tangent = UnityObjectToWorldDir(v.tangent);
				o.binormal = cross(o.normal, o.tangent) * (v.tangent.w * unity_WorldTransformParams.w);
				o.vertexColor = v.color;
				o.uv[0] = v.uv0;
				o.uv[1] = v.uv1;
				o.uv[2] = v.uv2;
				o.uv[3] = v.uv3;
				#if defined(LIGHTMAP_ON)
				o.lightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif
				#ifdef DYNAMICLIGHTMAP_ON
				o.lightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				#endif
				o.localPos = v.vertex;
				o.worldPos = mul(unity_ObjectToWorld, o.localPos);
				float3 localOffset = float3(0, 0, 0);
				float3 worldOffset = float3(0, 0, 0);
				o.localPos.rgb += localOffset;
				o.worldPos.rgb += worldOffset;
				o.pos = UnityObjectToClipPos(o.localPos);
				#ifdef POI_PASS_OUTLINE
				#if defined(UNITY_REVERSED_Z)
				o.pos.z += _Offset_Z * - 0.01;
				#else
				o.pos.z += _Offset_Z * 0.01;
				#endif
				#endif
				o.grabPos = ComputeGrabScreenPos(o.pos);
				#ifndef FORWARD_META_PASS
				#if !defined(UNITY_PASS_SHADOWCASTER)
				UNITY_TRANSFER_SHADOW(o, o.uv[0].xy);
				#else
				TRANSFER_SHADOW_CASTER_NOPOS(o, o.pos);
				#endif
				#endif
				UNITY_TRANSFER_FOG(o, o.pos);
				if (float(0))
				{
					if (o.pos.w < _ProjectionParams.y * 1.01 && o.pos.w > 0)
					{
						o.pos.z = o.pos.z * 0.0001 + o.pos.w * 0.999;
					}
				}
				#ifdef POI_PASS_META
				o.pos = UnityMetaVertexPosition(v.vertex, v.uv1.xy, v.uv2.xy, unity_LightmapST, unity_DynamicLightmapST);
				#endif
				#if defined(GRAIN)
				float4 worldDirection;
				worldDirection.xyz = o.worldPos.xyz - _WorldSpaceCameraPos;
				worldDirection.w = dot(o.pos, CalculateFrustumCorrection());
				o.worldDirection = worldDirection;
				#endif
				return o;
			}
			float2 calculatePolarCoordinate(in PoiMesh poiMesh)
			{
				float2 delta = poiMesh.uv[float(0)] - float4(0.5,0.5,0,0);
				float radius = length(delta) * 2 * float(1);
				float angle = atan2(delta.x, delta.y) * 1.0 / 6.28 * float(1);
				return float2(radius, angle + distance(poiMesh.uv[float(0)], float4(0.5,0.5,0,0)) * float(0));
			}
			float2 MonoPanoProjection(float3 coords)
			{
				float3 normalizedCoords = normalize(coords);
				float latitude = acos(normalizedCoords.y);
				float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
				float2 sphereCoords = float2(longitude, latitude) * float2(1.0 / UNITY_PI, 1.0 / UNITY_PI);
				sphereCoords = float2(1.0, 1.0) - sphereCoords;
				return(sphereCoords + float4(0, 1 - unity_StereoEyeIndex, 1, 1.0).xy) * float4(0, 1 - unity_StereoEyeIndex, 1, 1.0).zw;
			}
			float2 StereoPanoProjection(float3 coords)
			{
				float3 normalizedCoords = normalize(coords);
				float latitude = acos(normalizedCoords.y);
				float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
				float2 sphereCoords = float2(longitude, latitude) * float2(0.5 / UNITY_PI, 1.0 / UNITY_PI);
				sphereCoords = float2(0.5, 1.0) - sphereCoords;
				return(sphereCoords + float4(0, 1 - unity_StereoEyeIndex, 1, 0.5).xy) * float4(0, 1 - unity_StereoEyeIndex, 1, 0.5).zw;
			}
			float2 calculatePanosphereUV(in PoiMesh poiMesh)
			{
				float3 viewDirection = normalize(lerp(getCameraPosition().xyz, _WorldSpaceCameraPos.xyz, float(1)) - poiMesh.worldPos.xyz) * - 1;
				return lerp(MonoPanoProjection(viewDirection), StereoPanoProjection(viewDirection), float(0));
			}
			void applyAlphaOptions(inout PoiFragData poiFragData, in PoiMesh poiMesh, in PoiCam poiCam, in PoiMods poiMods)
			{
				poiFragData.alpha = saturate(poiFragData.alpha + float(0));
				if (float(0))
				{
					float3 position = float(1) ? poiMesh.worldPos : poiMesh.objectPosition;
					poiFragData.alpha *= lerp(float(0), float(1), smoothstep(float(0), float(0), distance(position, poiCam.worldPos)));
				}
				if (float(0))
				{
					float holoRim = saturate(1 - smoothstep(min(float(0.5), float(0.5)), float(0.5), poiCam.vDotN));
					holoRim = abs(lerp(1, holoRim, float(0)));
					poiFragData.alpha *= float(0) ?1 - holoRim : holoRim;
				}
				if (float(0))
				{
					half cameraAngleMin = float(45) / 180;
					half cameraAngleMax = float(90) / 180;
					half modelAngleMin = float(45) / 180;
					half modelAngleMax = float(90) / 180;
					float3 pos = float(0) == 0 ? poiMesh.objectPosition : poiMesh.worldPos;
					half3 cameraToModelDirection = normalize(pos - getCameraPosition());
					half3 modelForwardDirection = normalize(mul(unity_ObjectToWorld, normalize(float4(0,0,1,0).rgb)));
					half cameraLookAtModel = remapClamped(cameraAngleMax, cameraAngleMin, .5 * dot(cameraToModelDirection, getCameraForward()) + .5);
					half modelLookAtCamera = remapClamped(modelAngleMax, modelAngleMin, .5 * dot(-cameraToModelDirection, modelForwardDirection) + .5);
					if (float(0) == 0)
					{
						poiFragData.alpha *= max(cameraLookAtModel, float(0));
					}
					else if (float(0) == 1)
					{
						poiFragData.alpha *= max(modelLookAtCamera, float(0));
					}
					else if (float(0) == 2)
					{
						poiFragData.alpha *= max(cameraLookAtModel * modelLookAtCamera, float(0));
					}
				}
			}
			inline half Dither8x8Bayer(int x, int y)
			{
				const half dither[ 64 ] = {
					1, 49, 13, 61, 4, 52, 16, 64,
					33, 17, 45, 29, 36, 20, 48, 32,
					9, 57, 5, 53, 12, 60, 8, 56,
					41, 25, 37, 21, 44, 28, 40, 24,
					3, 51, 15, 63, 2, 50, 14, 62,
					35, 19, 47, 31, 34, 18, 46, 30,
					11, 59, 7, 55, 10, 58, 6, 54,
					43, 27, 39, 23, 42, 26, 38, 22
				};
				int r = y * 8 + x;
				return dither[r] / 64;
			}
			half calcDither(half2 grabPos)
			{
				return Dither8x8Bayer(fmod(grabPos.x, 8), fmod(grabPos.y, 8));
			}
			void applyDithering(inout PoiFragData poiFragData, in PoiCam poiCam)
			{
				if (float(0))
				{
					poiFragData.alpha = saturate(poiFragData.alpha - (calcDither(poiCam.screenUV) * (1 - poiFragData.alpha) * float(0.1)));
				}
			}
			void ApplyAlphaToCoverage(inout PoiFragData poiFragData, in PoiMesh poiMesh)
			{
				
				if (float(0) == 1)
				{
					
					if (float(0) && float(0))
					{
						poiFragData.alpha *= 1 + max(0, CalcMipLevel(poiMesh.uv[0] * float4(0.0004882813,0.0004882813,2048,2048).zw)) * float(0.25);
						poiFragData.alpha = (poiFragData.alpha - float(0.5)) / max(fwidth(poiFragData.alpha), 0.0001) + float(0.5);
						poiFragData.alpha = saturate(poiFragData.alpha);
					}
				}
			}
			void applyVertexColor(inout PoiFragData poiFragData, PoiMesh poiMesh)
			{
				#ifndef POI_PASS_OUTLINE
				float3 vertCol = lerp(poiMesh.vertexColor.rgb, GammaToLinearSpace(poiMesh.vertexColor.rgb), float(1));
				poiFragData.baseColor *= lerp(1, vertCol, float(0));
				#endif
				poiFragData.alpha *= lerp(1, poiMesh.vertexColor.a, float(0));
			}
			float4 frag(v2f i, uint facing : SV_IsFrontFace) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				PoiMesh poiMesh;
				PoiInitStruct(PoiMesh, poiMesh);
				PoiLight poiLight;
				PoiInitStruct(PoiLight, poiLight);
				PoiVertexLights poiVertexLights;
				PoiInitStruct(PoiVertexLights, poiVertexLights);
				PoiCam poiCam;
				PoiInitStruct(PoiCam, poiCam);
				PoiMods poiMods;
				PoiInitStruct(PoiMods, poiMods);
				poiMods.globalEmission = 1;
				PoiFragData poiFragData;
				poiFragData.emission = 0;
				poiFragData.baseColor = float3(0, 0, 0);
				poiFragData.finalColor = float3(0, 0, 0);
				poiFragData.alpha = 1;
				poiMesh.objectPosition = i.objectPos;
				poiMesh.objNormal = i.objNormal;
				poiMesh.normals[0] = i.normal;
				poiMesh.tangent = i.tangent;
				poiMesh.binormal = i.binormal;
				poiMesh.worldPos = i.worldPos.xyz;
				poiMesh.localPos = i.localPos.xyz;
				poiMesh.vertexColor = i.vertexColor;
				poiMesh.isFrontFace = facing;
				#ifndef POI_PASS_OUTLINE
				if (!poiMesh.isFrontFace)
				{
					poiMesh.normals[0] *= -1;
					poiMesh.tangent *= -1;
					poiMesh.binormal *= -1;
				}
				#endif
				poiCam.viewDir = !IsOrthographicCamera() ? normalize(_WorldSpaceCameraPos - i.worldPos.xyz) : normalize(UNITY_MATRIX_I_V._m02_m12_m22);
				float3 tanToWorld0 = float3(i.tangent.x, i.binormal.x, i.normal.x);
				float3 tanToWorld1 = float3(i.tangent.y, i.binormal.y, i.normal.y);
				float3 tanToWorld2 = float3(i.tangent.z, i.binormal.z, i.normal.z);
				float3 ase_tanViewDir = tanToWorld0 * poiCam.viewDir.x + tanToWorld1 * poiCam.viewDir.y + tanToWorld2 * poiCam.viewDir.z;
				poiCam.tangentViewDir = normalize(ase_tanViewDir);
				#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
				poiMesh.lightmapUV = i.lightmapUV;
				#endif
				poiMesh.parallaxUV = poiCam.tangentViewDir.xy / max(poiCam.tangentViewDir.z, 0.0001);
				poiMesh.uv[0] = i.uv[0];
				poiMesh.uv[1] = i.uv[1];
				poiMesh.uv[2] = i.uv[2];
				poiMesh.uv[3] = i.uv[3];
				poiMesh.uv[4] = poiMesh.uv[0];
				poiMesh.uv[5] = poiMesh.worldPos.xz;
				poiMesh.uv[6] = poiMesh.uv[0];
				poiMesh.uv[7] = poiMesh.uv[0];
				poiMesh.uv[4] = calculatePanosphereUV(poiMesh);
				poiMesh.uv[6] = calculatePolarCoordinate(poiMesh);
				float4 mainTexture = UNITY_SAMPLE_TEX2D(_MainTex, poiUV(poiMesh.uv[float(0)].xy, float4(1,1,0,0)) + _Time.x * float4(0,0,0,0));
				float3 mainNormal = UnpackScaleNormal(POI2D_SAMPLER_PAN(_BumpMap, _MainTex, poiUV(poiMesh.uv[float(0)], float4(1,1,0,0)), float4(0,0,0,0)), float(1));
				poiMesh.tangentSpaceNormal = mainNormal;
				#if defined(FINALPASS) && !defined(UNITY_PASS_SHADOWCASTER) && !defined(POI_PASS_OUTLINE)
				ApplyDetailNormal(poiMods, poiMesh);
				#endif
				#if defined(GEOM_TYPE_MESH) && defined(VIGNETTE) && !defined(UNITY_PASS_SHADOWCASTER) && !defined(POI_PASS_OUTLINE)
				calculateRGBNormals(poiMesh);
				#endif
				poiMesh.normals[1] = normalize(
				poiMesh.tangentSpaceNormal.x * poiMesh.tangent.xyz +
				poiMesh.tangentSpaceNormal.y * poiMesh.binormal +
				poiMesh.tangentSpaceNormal.z * poiMesh.normals[0]
				);
				float3 fancyNormal = UnpackNormal(float4(0.5, 0.5, 1, 1));
				poiMesh.normals[0] = normalize(
				fancyNormal.x * poiMesh.tangent.xyz +
				fancyNormal.y * poiMesh.binormal +
				fancyNormal.z * poiMesh.normals[0]
				);
				poiCam.forwardDir = getCameraForward();
				poiCam.worldPos = _WorldSpaceCameraPos;
				poiCam.reflectionDir = reflect(-poiCam.viewDir, poiMesh.normals[1]);
				poiCam.vertexReflectionDir = reflect(-poiCam.viewDir, poiMesh.normals[0]);
				poiCam.distanceToVert = distance(poiMesh.worldPos, poiCam.worldPos);
				poiCam.grabPos = i.grabPos;
				poiCam.screenUV = calcScreenUVs(i.grabPos);
				poiCam.vDotN = abs(dot(poiCam.viewDir, poiMesh.normals[1]));
				poiCam.clipPos = i.pos;
				poiCam.worldDirection = i.worldDirection;
				poiFragData.baseColor = mainTexture.rgb * poiThemeColor(poiMods, float4(1,1,1,1).rgb, float(0));
				poiFragData.alpha = mainTexture.a * float4(1,1,1,1).a;
				#if defined(PROP_CLIPPINGMASK) || !defined(OPTIMIZER_ENABLED)
				float alphaMask = POI2D_SAMPLER_PAN(_ClippingMask, _MainTex, poiUV(poiMesh.uv[float(0)], float4(1,1,0,0)), float4(0,0,0,0)).r;
				if (float(0))
				{
					alphaMask = 1 - alphaMask;
				}
				#else
				float alphaMask = 1;
				#endif
				poiFragData.alpha *= alphaMask;
				applyAlphaOptions(poiFragData, poiMesh, poiCam, poiMods);
				applyVertexColor(poiFragData, poiMesh);
				poiFragData.finalColor = poiFragData.baseColor;
				if (float(0) == 0)
				{
					UNITY_APPLY_FOG(i.fogCoord, poiFragData.finalColor);
				}
				poiFragData.alpha = float(0) ? 1 : poiFragData.alpha;
				ApplyAlphaToCoverage(poiFragData, poiMesh);
				applyDithering(poiFragData, poiCam);
				if (float(0) == POI_MODE_OPAQUE)
				{
					poiFragData.alpha = 1;
				}
				clip(poiFragData.alpha - float(0.5));
				if (float(0) == POI_MODE_FADE)
				{
					clip(poiFragData.alpha - 0.01);
				}
				return float4(poiFragData.finalColor, poiFragData.alpha) + POI_SAFE_RGB0;
			}
			ENDCG
		}
	}
	CustomEditor "Thry.ShaderEditor"
}
