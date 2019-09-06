Shader ".poiyomi/Toon/Default/Cutout"
{
    Properties
    {
        [HideInInspector] shader_is_using_thry_editor ("", Float) = 0
                [HideInInspector] shader_master_label ("<color=#ff0000ff>❤</color> <color=#000000ff>Poiyomi Toon Shader V4.0</color> <color=#ff0000ff>❤</color>", Float) = 0
        [HideInInspector] shader_presets ("poiToonPresets", Float) = 0
        [HideInInspector] shader_eable_poi_settings_selection ("", Float) = 0
        
        [HideInInspector] footer_github ("linkButton(Github,https://github.com/poiyomi/PoiyomiToonShader)", Float) = 0
        [HideInInspector] footer_discord ("linkButton(Discord,https://discord.gg/Ays52PY)", Float) = 0
        [HideInInspector] footer_donate ("linkButton(Donate,https://www.paypal.me/poiyomi)", Float) = 0
        [HideInInspector] footer_patreon ("linkButton(Patreon,https://www.patreon.com/poiyomi)", Float) = 0
        [HideInInspector] footer_patch ("linkButton(Patch,https://www.youtube.com/watch?v=WSKKynXWBpo)", Float) = 0
        
        [HideInInspector] m_mainOptions ("Main--button_right={text:Tutorial,action:{type:url,data:https://youtu.be/FX3Uvcj35Rk},hover:YouTube}", Float) = 0
        _Color ("Color & Alpha", Color) = (1, 1, 1, 1)
        _Saturation ("Saturation", Range(-1, 1)) = 0
        _MainTex ("Texture", 2D) = "white" { }
        [Normal]_BumpMap ("Normal Map", 2D) = "bump" { }
        _BumpScale ("Normal Intensity", Range(0, 10)) = 1
        _AlphaMask ("Alpha Mask", 2D) = "white" { }
        _Clip ("Alpha Cuttoff", Range(0, 1.001)) = 0.5
        [Toggle(_)]_ForceOpaque("Force Opaque", Float) = 1
        [HideInInspector] m_start_mainAdvanced ("Advanced", Float) = 0
        _MainDistanceFade("Distance Fade X to Y", Vector) = (0,0,0,0)
        _GlobalPanSpeed ("Pan Speed XY", Vector) = (0, 0, 0, 0)
        [Normal]_DetailNormalMap ("Detail Normal Map", 2D) = "bump" { }
        _DetailNormalMask ("Detail Mask", 2D) = "white" { }
        _DetailNormalMapScale ("Detail Intensity", Range(0, 10)) = 1
        [HideInInspector] m_end_mainAdvanced ("Advanced", Float) = 0
        
        [HideInInspector] m_metallicOptions ("Metallic--button_right={text:Tutorial,action:{type:url,data:https://youtu.be/DyluMaFn64M},hover:YouTube}", Float) = 0
        [Toggle(_METALLICGLOSSMAP)]_EnableMetallic("Enable Metallics", Float) = 0
        _CubeMap ("Baked CubeMap", Cube) = "" { }
        [Toggle(_)]_SampleWorld ("Force Baked Cubemap", Range(0, 1)) = 0
        _PurelyAdditive ("Purely Additive", Range(0, 1)) = 0
        _MetallicMask ("Metallic Mask", 2D) = "white" { }
        _Metallic ("Metallic", Range(0, 1)) = 0
        _SmoothnessMask ("Smoothness Mask", 2D) = "white" { }
        [Toggle(_)]_InvertSmoothness ("Invert Smoothness Mask", Range(0, 1)) = 0
        _Smoothness ("Smoothness", Range(0, 1)) = 0
        
        [HideInInspector] m_matcapOptions ("Matcap / Sphere Textures--button_right={text:Tutorial,action:{type:url,data:https://youtu.be/DFo87kuO1OI},hover:YouTube}", Float) = 0
        [Toggle(_COLORADDSUBDIFF_ON)]_EnableMatcap("Enable Matcap", Float) = 0
        _Matcap ("Matcap", 2D) = "white" { }
        _MatcapMask ("Matcap Mask", 2D) = "white" { }
        _MatcapColor ("Matcap Color", Color) = (1, 1, 1, 1)
        _MatcapBrightness ("Matcap Brightness", Range(0, 20)) = 1
        _ReplaceWithMatcap ("Replace With Matcap", Range(0, 1)) = 1
        _MultiplyMatcap ("Multiply Matcap", Range(0, 1)) = 0
        _AddMatcap ("Add Matcap", Range(0, 1)) = 0
        
        [HideInInspector] m_emissionOptions ("Emission / Glow--button_right={text:Tutorial,action:{type:url,data:https://youtu.be/iqk23gtwkz0},hover:YouTube}", Float) = 0
        [Toggle(_EMISSION)]_EnableEmission("Enable Emission", Float) = 0
        [HDR]_EmissionColor ("Emission Color", Color) = (1, 1, 1, 1)
        _EmissionMap ("Emission Map", 2D) = "white" { }
        _EmissionMask ("Emission Mask", 2D) = "white" { }
        _EmissionPan ("Map(XY) | Mask(ZW) Pan", Vector) = (0, 0, 0, 0)
        _EmissionStrength ("Emission Strength", Range(0, 20)) = 0

        [HideInInspector] m_start_glowInDarkEmissionOptions ("Glow In The Dark Emission (Requires Lighting Enabled)", Float) = 0
        [Toggle(_)]_EnableGITDEmission("Enable Glow In The Dark", Float) = 0
        [Enum(World, 0, Mesh, 1)] _GITDEWorldOrMesh("Lighting Type", Int) = 0
        _GITDEMinEmissionMultiplier ("Min Emission Multiplier", Range(0,1)) = 1
        _GITDEMaxEmissionMultiplier ("Max Emission Multiplier", Range(0,1)) = 0
        _GITDEMinLight ("Min Lighting", Range(0,1)) = 0
        _GITDEMaxLight ("Max Lighting", Range(0,1)) = 1
        [HideInInspector] m_end_glowInDarkEmissionOptions ("Glow In The Dark Emission (Requires Lighting Enabled)", Float) = 0

        [HideInInspector] m_start_blinkingEmissionOptions ("Blinking Emission", Float) = 0
        _EmissiveBlink_Min ("Emissive Blink Min", Float) = 1
        _EmissiveBlink_Max ("Emissive Blink Max", Float) = 1
        _EmissiveBlink_Velocity ("Emissive Blink Velocity", Float) = 4
        [HideInInspector] m_end_blinkingEmissionOptions ("Blinking Emission", Float) = 0
        
        [HideInInspector] m_start_scrollingEmissionOptions ("Scrolling Emission", Float) = 0
        [Toggle(_)] _ScrollingEmission ("Enable Scrolling Emission", Float) = 0
        _EmissiveScroll_Direction ("Emissive Scroll Direction", Vector) = (0, -10, 0, 0)
        _EmissiveScroll_Width ("Emissive Scroll Width", Float) = 10
        _EmissiveScroll_Velocity ("Emissive Scroll Velocity", Float) = 10
        _EmissiveScroll_Interval ("Emissive Scroll Interval", Float) = 20
        [HideInInspector] m_end_scrollingEmissionOptions ("Scrolling Emission", Float) = 0
        
        [HideInInspector] m_fakeLightingOptions ("Light & Shadow--button_right={text:Tutorial,action:{type:url,data:https://youtu.be/2B-EJutVjs8},hover:YouTube}", Float) = 0
        [Toggle(_NORMALMAP)]_EnableLighting("Enable Lighting", Float) = 1
        [Enum(Natural, 0, Controlled, 1)] _LightingType("Lighting Type", Int) = 0
        [Gradient]_ToonRamp ("Lighting Ramp", 2D) = "white" { }
        _LightingShadowMask ("Shadow Mask (R)", 2D) = "white" { }
        _ShadowStrength ("Shadow Strength", Range(0, 1)) = 1
        _ShadowOffset ("Shadow Offset", Range(-1, 1)) = 0
        _AOMap ("AO Map", 2D) = "white" { }
        _AOStrength ("AO Strength", Range(0, 1)) = 1
        [HideInInspector] m_start_lightingAdvanced ("Advanced", Float) = 0
        _IndirectContribution ("Indirect Contribution", Range(0, 1)) = 0
        _AdditiveSoftness ("Additive Softness", Range(0, 0.5)) = 0.005
        _AdditiveOffset ("Additive Offset", Range(-0.5, 0.5)) = 0
        _AttenuationMultiplier ("Attenuation", Range(0, 1)) = 0
        _LightingControlledUseLightColor("Controlled: Use Light Color", Range(0,1)) = 1
        [HideInInspector] m_end_lightingAdvanced ("Advanced", Float) = 0
        
        [HideInInspector] m_specularHighlightsOptions ("Specular Highlights--button_right={text:Tutorial,action:{type:url,data:https://youtu.be/YFFe8IrXVnc},hover:YouTube}", Float) = 0
        [Toggle(_SPECGLOSSMAP)]_EnableSpecular("Enable Specular", Float) = 0
        [Enum(Realistic, 1, Toon, 2, Anisotropic, 3)] _SpecularType ("Specular Type", Int) = 1
        _SpecularTint ("Specular Tint", Color) = (.2, .2, .2, 1)
        _SpecularSmoothness ("Smoothness", Range(0, 1)) = 0
        _SpecularMap ("Specular Map", 2D) = "white" { }
        [Enum(Alpha, 0, Grayscale, 1)] _SmoothnessFrom ("Smoothness From", Int) = 1
        [HideInInspector] m_start_SpecularToon ("Toon", Float) = 0
        [MultiSlider]_SpecularToonInnerOuter("Inner/Outer Edge", Vector) = (0.25,0.3,0,1)
        [HideInInspector] m_end_SpecularToon ("Toon", Float) = 0
        [HideInInspector] m_start_Anisotropic ("Anisotropic", Float) = 0
        [Enum(Tangent, 0, Bitangent, 1)] _SpecWhatTangent("(Bi)Tangent?", Int) = 0
        _AnisoSpec1Alpha("Spec1 Alpha", Range(0,1)) = 1
        _AnisoSpec2Alpha("Spec2 Alpha", Range(0,1)) = 1
        //_Spec1Offset ("Spec1 Offset", Float) = 0
        //_Spec1JitterStrength ("Spec1 Jitter Strength", Float) = 1.0
        _Spec2Smoothness ("Spec2 Smoothness", Range(0, 1)) = 0
        //_Spec2Offset ("Spec2 Offset", Float) = 0
        //_Spec2JitterStrength ("Spec2 Jitter Strength", Float) = 1.0
        [Toggle(_)]_AnisoUseTangentMap ("Use Directional Map?", Float) = 0
        _AnisoTangentMap ("Anisotropic Directional Map", 2D) = "bump" {}
        //_ShiftTexture ("Shift Texture", 2D) = "black" { }
        [HideInInspector] m_end_Anisotropic ("Anisotropic", Float) = 0

        [HideInInspector] m_parallaxMap ("Parallax", Float) = 0
        [Toggle(_PARALLAXMAP)]_ParallaxMap ("Enable Parallax FX", Float) = 0
        [Toggle(_)]_ParallaxHeightMapEnabled ("Enable Parallax Height", Float) = 0
        [Toggle(_)]_ParallaxInternalMapEnabled ("Enable Parallax Internal", Float) = 0
        [HideInInspector] m_start_parallaxHeightmap ("Heightmap", Float) = 0
		_ParallaxHeightMap ("Height Map", 2D) = "black" {}
		_ParallaxHeightIterations ("Parallax Height Iterations", Range(1, 20)) = 1
		_ParallaxStrength ("Parallax Strength", Range(0, 1)) = 0
        _ParallaxBias ("Parallax Bias (0.42)",Float) = 0.42
        [HideInInspector] m_end_parallaxHeightmap ("Heightmap", Float) = 0
        [HideInInspector] m_start_parallaxInternal ("Internal", Float) = 0
        [Enum(Basic, 0, HeightMap, 1)] _ParallaxInternalHeightmapMode("Parallax Mode", Int) = 0
        [Toggle(_)]_ParallaxInternalHeightFromAlpha ("HeightFromAlpha", Float) = 0
		_ParallaxInternalMap ("Internal Map", 2D) = "black" {}
		_ParallaxInternalIterations ("Parallax Internal Iterations", Range(1, 50)) = 1
        _ParallaxInternalMinDepth("Min Depth", Float) = 0
        _ParallaxInternalMaxDepth("Max Depth", Float) = 1
        _ParallaxInternalMinFade("Min Depth Brightness", Range(0,5)) = 1
        _ParallaxInternalMaxFade("Max Depth Brightness", Range(0,5)) = 0
        _ParallaxInternalMinColor("Min Depth Color", Color) = (1, 1, 1, 1)
        _ParallaxInternalMaxColor("Max Depth Color", Color) = (1, 1, 1, 1)
        _ParallaxInternalPanSpeed("Pan Speed", Vector) = (0,0,0,0)
        _ParallaxInternalPanDepthSpeed("Per Level Speed Multiplier", Vector) = (0,0,0,0)
        [HideInInspector] m_end_parallaxInternal ("Internal", Float) = 0

        [HideInInspector] m_subsurfaceOptions ("Subsurface Scattering--button_right={text:Tutorial,action:{type:url,data:https://youtu.be/hs1bnnPmDFc},hover:YouTube}", Float) = 0
        [Toggle(_TERRAIN_NORMAL_MAP)]_EnableSSS("Enable Subsurface Scattering", Float) = 0
        _SSSColor ("Subsurface Color", Color) = (1, 1, 1, 1)
        _SSSThicknessMap ("Thickness Map", 2D) = "black" { }
        _SSSThicknessMod ("Thickness mod", Range(-1, 1)) = 0
        _SSSSCale ("Light Strength", Range(0, 1)) = 0
        _SSSPower ("Light Spread", Range(1, 100)) = 1
        _SSSDistortion ("Light Distortion", Range(0, 1)) = 0

        [HideInInspector] m_rimLightOptions ("Rim Lighting--button_right={text:Tutorial,action:{type:url,data:https://youtu.be/JNxzcRY_BZ8},hover:YouTube}", Float) = 0
        [Toggle(_GLOSSYREFLECTIONS_OFF)]_EnableRimLighting("Enable Rim Lighting", Float) = 0
        [Toggle(_)]_RimLightingInvert("Invert Rim Lighting", Float) = 0
        _RimLightColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimWidth ("Rim Width", Range(0, 1)) = 0.8
        _RimSharpness ("Rim Sharpness", Range(0, 1)) = .25
        _RimStrength ("Rim Emission", Range(0, 20)) = 0
        _RimBrighten ("Rim Color Brighten", Range(0, 3)) = 0
        _RimLightColorBias ("Rim Color Bias", Range(0, 1)) = 0
        _RimTex ("Rim Texture", 2D) = "white" { }
        _RimMask ("Rim Mask", 2D) = "white" { }
        _RimTexPanSpeed ("Rim Texture Pan Speed", Vector) = (0, 0, 0, 0)
        
        [HideInInspector] m_start_reflectionRim ("Environmental Rim", Float) = 0
        [Toggle(_)]_EnableEnvironmentalRim("Enable Environmental Rim", Float) = 0
        _RimEnviroBlur("Blur", Range(0,1)) = 0.7
        _RimEnviroWidth("Rim Width", Range(0,1)) = 0.45
        _RimEnviroSharpness ("Rim Sharpness", Range(0, 1)) = 0
        _RimEnviroMinBrightness ("Min Brightness Threshold", Range(0, 2)) = 0
        [HideInInspector] m_end_reflectionRim ("Environmental Rim", Float) = 0
        [HideInInspector] m_start_rimWidthNoise ("Width Noise", Float) = 0
        _RimWidthNoiseTexture ("Rim Width Noise", 2D) = "black" { }
        _RimWidthNoiseStrength ("Intensity", Range(0,1)) = 0.1
        _RimWidthNoisePan ("Pan Speed (XY)", Vector) = (0,0,0,0)
        [HideInInspector] m_end_rimWidthNoise ("Width Noise", Float) = 0
        [HideInInspector] m_start_ShadowMix ("Shadow Mix", Float) = 0
        _ShadowMix ("Shadow Mix In", Range(0, 1)) = 0
        _ShadowMixThreshold ("Shadow Mix Threshold", Range(0, 1)) = .5
        _ShadowMixWidthMod ("Shadow Mix Width Mod", Range(0, 10)) = .5
        [HideInInspector] m_end_ShadowMix ("Shadow Mix", Float) = 0
        
        [HideInInspector] m_flipBook ("Flipbook--button_right={text:Tutorial,action:{type:url,data:https://youtu.be/NrA18CITEVs},hover:YouTube}", Float) = 0
        [Toggle(_FADING_ON)]_EnableFlipbook("Enable Flipbook", Float) = 0
        [TextureArray]_FlipbookTexArray ("Texture Array", 2DArray) = "" {}
        _FlipbookColor ("Color & alpha", Color) = (1, 1, 1, 1)
        _FlipbookTotalFrames ("Total Frames", Int) = 1
        _FlipbookFPS ("FPS", Float) = 30.0
        _FlipbookScaleOffset ("Scale | Offset", Vector) = (1,1,0,0)
        [Toggle(_)]_FlipbookTiled("Tiled?", Float) = 0
        _FlipbookEmissionStrength("Emission Strength", Range(0,20)) = 0
        _FlipbookRotation("Rotation", Range(0,360)) = 0
        _FlipbookReplace("Replace", Range(0,1)) = 1
        _FlipbookMultiply("Multiply", Range(0,1)) = 0
        _FlipbookAdd("Add", Range(0,1)) = 0
        //[Toggle(_)]_FlipbookControlsAlpha("Flipbook Controls Alpha", Float) = 0
        /*
        [HideInInspector] m_start_FlipbookOffsetLoop ("Offset Animation", Float) = 0
        [Enum(Off, 0, Loop, 1, Bounce, 2, Smooth Bounce, 3)]_FlipbookMovementType("Movement Type", Int) = 1
        _FlipbookStartEndOffset("Start & End Offset", Vector) = (0,0,0,0)
        _FlipbookMovementSpeed("Speed", Float) = 1.0
        [HideInInspector] m_end_FlipbookOffsetLoop ("Offset Animation", Float) = 0
        */
        [HideInInspector] m_start_manualFlipbookControl ("Manual Control", Float) = 0
        _FlipbookCurrentFrame ("Current Frame", Float) = -1
        [HideInInspector] m_end_manualFlipbookControl ("Manual Control", Float) = 0

        [HideInInspector] m_dissolve ("Dissolve--button_right={text:Tutorial,action:{type:url,data:https://youtu.be/abTejmV4yGU},hover:YouTube}", Float) = 0
        [Toggle(_ALPHABLEND_ON)]_EnableDissolve("Enable Dissolve", Float) = 0
        [Enum(Basic, 1, Point2Point, 2)] _DissolveType ("Dissolve Type", Int) = 1
        _DissolveEdgeWidth ("Edge Width", Range(0,.5)) = 0.025
        _DissolveEdgeHardness ("Edge Hardness", Range(0,1)) = 0.5
        _DissolveEdgeColor ("Edge Color", Color) = (1, 1, 1, 1)
        [Gradient]_DissolveEdgeGradient("Edge Gradient", 2D) = "white" { }
        _DissolveEdgeEmission ("Edge Emission", Range(0,20)) = 0
        _DissolveTextureColor ("Dissolve to Color", Color) = (1, 1, 1, 1)
        _DissolveToTexture ("Dissolve to Texture", 2D) = "white" { }
        _DissolveToEmissionStrength ("Dissolve to Emission Strength", Range(0,20)) = 0
        _DissolveToPanning ("Dissolve to Panning XY", Vector) = (0,0,0,0)
        _DissolveNoiseTexture ("Dissolve Noise", 2D) = "white" { }
        [Toggle(_)]_DissolveInvertNoise ("Invert Noise", Float) = 0
        _DissolveDetailNoise ("Dissolve Detail Noise", 2D) = "black" { }
        [Toggle(_)]_DissolveInvertDetailNoise ("Invert Detail Noise", Float) = 0
        _DissolveDetailStrength ("Dissolve Detail Strength", Range(0,1)) = 0.1
        _DissolvePan("Noise (XY) | Detail (ZW) Pan", Vector) = (0,0,0,0)
        _DissolveAlpha ("Dissolve Alpha", Range(0, 1)) = 0
        _DissolveMask("Dissolve Mask", 2D) = "white" { }
        _ContinuousDissolve ("Continuous Dissolve Speed", Float) = 0
        [HideInInspector] m_start_pointToPoint ("point to point", Float) = 0
        [Enum(Local, 0, World, 1)] _DissolveP2PWorldLocal ("World/Local", Int) = 0
        _DissolveP2PEdgeLength ("Edge Length", Float) = 0.1
        _DissolveStartPoint ("Start Point", Vector) = (0,-1,0,0)
        _DissolveEndPoint ("End Point", Vector) = (0,1,0,0)
        [HideInInspector] m_end_pointToPoint ("Point To Point", Float) = 0
        
        [HideInInspector] m_panosphereOptions ("Panosphere / Cubemaps--button_right={text:Tutorial,action:{type:url,data:https://youtu.be/wyCY6qE0_Vg},hover:YouTube}", Float) = 0
        [Toggle(_DETAIL_MULX2)]_PanoToggle("Enable Panosphere", Float) = 0
        _PanosphereColor ("Color", Color) = (1, 1, 1, 1)
        _PanosphereTexture ("Texture", 2D) = "white" { }
        _PanoMapTexture ("Mask", 2D) = "white" { }
        _PanoEmission ("Emission Strength", Range(0, 10)) = 0
        _PanoBlend ("Alpha", Range(0, 1)) = 0
        _PanospherePan ("Pan Speed", Vector) = (0, 0, 0, 0)
        [Toggle(_)]_PanoCubeMapToggle("Use Cubemap", Float) = 0
        _PanoCubeMap ("CubeMap", Cube) = "" { }

        [HideInInspector] m_StencilPassOptions ("Stencil--button_right={text:Tutorial,action:{type:url,data:https://youtu.be/uniOEpw88jk},hover:YouTube}", Float) = 0
        [IntRange] _StencilRef ("Stencil Reference Value", Range(0, 255)) = 0
        //[IntRange] _StencilReadMaskRef ("Stencil ReadMask Value", Range(0, 255)) = 0
        //[IntRange] _StencilWriteMaskRef ("Stencil WriteMask Value", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilPassOp ("Stencil Pass Op", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilFailOp ("Stencil Fail Op", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFailOp ("Stencil ZFail Op", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilCompareFunction ("Stencil Compare Function", Float) = 8
        
        [HideInInspector] m_mirrorOptions ("Mirror--button_right={text:Tutorial,action:{type:url,data:https://youtu.be/ptnVKyuijO4},hover:YouTube}", Float) = 0
        [Toggle(_REQUIRE_UV2)]_EnableMirrorOptions("Enable Mirror Options", Float) = 0
        [Enum(ShowInBoth, 0, ShowOnlyInMirror, 1, DontShowInMirror, 2)] _Mirror ("Show in mirror", Int) = 0
        [Toggle(_)]_EnableMirrorTexture("Enable Mirror Texture", Float) = 0
        _MirrorTexture ("Mirror Tex", 2D) = "white" {}

        [HideInInspector] m_miscOptions ("Misc--button_right={text:Tutorial,action:{type:url,data:https://youtu.be/P5KlE9rk9pg},hover:YouTube}", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4
        [Enum(UnityEngine.Rendering.BlendMode)] _SourceBlend ("Source Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DestinationBlend ("Destination Blend", Float) = 10
        [Enum(Off, 0, On, 1)] _ZWrite ("ZWrite", Int) = 1
        _ZBias ("ZBias", Float) = 0.0

        [HideInInspector] m_debugOptions ("Debug", Float) = 0
        [Toggle(_COLOROVERLAY_ON)]_DebugDisplayDebug("Display Debug Info", Float) = 0
        [Enum(Off, 0, Vertex Normal, 1, Pixel Normal, 2, Tangent, 3, Binormal, 4)] _DebugMeshData ("Mesh Data", Int) = 0
        [Enum(Off, 0, Attenuation, 1, Direct Lighting, 2, Indirect Lighting, 3, light Map, 4, Ramped Light Map, 5, Final Lighting, 6)] _DebugLightingData ("Lighting Data", Int) = 0
        [Enum(Off, 0, finalSpecular, 1, highTexture, 2, tangentDirectionMap, 3, shiftTexture, 4)] _DebugSpecularData ("Specular Data", Int) = 0
        [Enum(Off, 0, View Dir, 1, Tangent View Dir, 2, Forward Dir, 3, WorldPos, 4, View Dot Normal, 5)] _DebugCameraData ("Camera Data", Int) = 0

    }
    
    CustomEditor "ThryEditor"
    SubShader
    {
        Tags {"DisableBatching" = "True" "RenderType" = "TransparentCutout" "Queue" = "AlphaTest"}
        
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
            AlphaToMask On
            ZTest [_ZTest]
            Offset [_ZBias], [_ZBias]
            CGPROGRAM
            
            #pragma target 4.0
            #define FORWARD_BASE_PASS
            #pragma shader_feature _PARALLAXMAP
            // Mirror
            #pragma shader_feature _REQUIRE_UV2
            // Texture Blending
            #pragma shader_feature _ALPHABLEND_ON
            // Panosphere
            #pragma shader_feature _DETAIL_MULX2
            // Lighting
            #pragma shader_feature _NORMALMAP
            // Flipbook
            #pragma shader_feature _FADING_ON
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
            #pragma shader_feature _EMISSION
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
            AlphaToMask On
            ZTest [_ZTest]
            Offset [_ZBias], [_ZBias]
            CGPROGRAM
            
            #pragma target 4.0
            #define FORWARD_ADD_PASS
            #define BINORMAL_PER_FRAGMENT
            #pragma shader_feature _PARALLAX_MAP
            // Mirror
            #pragma shader_feature _REQUIRE_UV2
            // Texture Blending
            #pragma shader_feature _ALPHABLEND_ON
            // Panosphere
            #pragma shader_feature _DETAIL_MULX2
            // Lighting
            #pragma shader_feature _NORMALMAP
            // Flipbook
            #pragma shader_feature _FADING_ON
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
            #pragma multi_compile_instancing
            #pragma multi_compile_fwdadd_fullshadows
            #pragma vertex vert
            #pragma fragment frag
            #include "../Includes/PoiPass.cginc"
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
            #define CUTOUT
            #define POISHADOW
            // Mirror
            #pragma shader_feature _REQUIRE_UV2
            // Texture Blending
            #pragma shader_feature _ALPHABLEND_ON
            #pragma multi_compile_instancing
            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster
            #include "../Includes/PoiPassShadow.cginc"
            ENDCG
            
        }
    }
}