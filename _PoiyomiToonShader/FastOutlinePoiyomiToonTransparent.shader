Shader ".poiyomi/Toon/outlines/Fast Transparent"
{
    Properties 
  { 
      [HideInInspector] shader_is_using_thry_editor("", Float)=0
        [HideInInspector] shader_master_label ("<color=#008080>❤ Poiyomi Toon Shader V2.7B ❤</color>", Float) = 0
        [HideInInspector] shader_presets ("poiToonPresets", Float) = 0
        [HideInInspector] shader_eable_poi_settings_selection("", Float) = 0
        
		[HideInInspector] footer_github("linkButton(Github,https://github.com/poiyomi/PoiyomiToonShader)", Float) = 0
		[HideInInspector] footer_discord("linkButton(Discord,https://discord.gg/Ays52PY)", Float) = 0
		[HideInInspector] footer_donate("linkButton(Donate,https://www.paypal.me/poiyomi)", Float) = 0
		[HideInInspector] footer_patreon("linkButton(Patreon,https://www.patreon.com/poiyomi)", Float) = 0

        [HideInInspector] m_mainOptions ("Main", Float) = 0
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Desaturation ("Desaturation", Range(-1, 1)) = 0
        _MainTex ("Texture", 2D) = "white" { }
        [Normal]_BumpMap ("Normal Map", 2D) = "bump" { }
        _BumpScale ("Normal Intensity", Range(0, 10)) = 1
        _AlphaMask ("Alpha Mask", 2D) = "white" { }
        _AOMap ("AO Map", 2D) = "white" { }
        _AOStrength ("AO Strength", Range(0,1)) = 1
        _Clip ("Alpha Cuttoff", Range(0, 1.001)) = 0.0
        [HideInInspector] m_start_mainAdvanced ("Advanced", Float) = 0
        _GlobalPanSpeed("Pan Speed XY", Vector) = (0,0,0,0)
        [Normal]_DetailNormalMap ("Detail Map", 2D) = "bump" { }
        _DetailNormalMask ("Detail Mask", 2D) = "white" { }
        _DetailNormalMapScale ("Detail Intensity", Range(0, 10)) = 1
        [HideInInspector] m_end_mainAdvanced ("Advanced", Float) = 0

        /*
        [HideInInspector] m_metallicOptions ("Metallic", Float) = 0
        _CubeMap ("Baked CubeMap", Cube) = "" { }
        [Toggle(_)]_SampleWorld ("Force Baked Cubemap", Range(0, 1)) = 0
        _PurelyAdditive ("Purely Additive", Range(0, 1)) = 0
        _MetallicMap ("Metallic Map", 2D) = "white" { }
        _Metallic ("Metallic", Range(0, 1)) = 0
        _SmoothnessMap ("Smoothness Map", 2D) = "white" { }
        [Toggle(_)]_InvertSmoothness("Invert Smoothness Map",Range(0,1)) = 0
        _Smoothness ("Smoothness", Range(0, 1)) = 0
        
        [HideInInspector] m_matcapOptions ("Matcap / Sphere Textures", Float) = 0
        _Matcap ("Matcap", 2D) = "white" { }
        _MatcapMap ("Matcap Map", 2D) = "white" { }
        _MatcapColor ("Matcap Color", Color) = (1, 1, 1, 1)
        _MatcapStrength ("Matcap Strength", Range(0, 20)) = 1
        _ReplaceWithMatcap ("Replace With Matcap", Range(0, 1)) = 0
        _MultiplyMatcap ("Multiply Matcap", Range(0, 1)) = 0
        _AddMatcap ("Add Matcap", Range(0, 1)) = 0
        */
        [HideInInspector] m_outlineOptions ("Outlines", Float) = 0
        _LineWidth ("Outline Width (cm)", Float) = 0
        _LineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineEmission ("Outline Emission", Float) = 0
        _OutlineTexture ("Outline Texture", 2D) = "white" { }
        _OutlineTexturePan ("Outline Texture Pan", Vector) = (0, 0, 0, 0)
        _OutlineShadowStrength ("Shadow Strength", Range(0, 1)) = 1
        [HideInInspector] m_start_outlineAdvanced ("Advanced", Float) = 0
        _OutlineFadeDistance ("Outline distance Fade X-Y", Vector) = (0,0,0,0)
        _OutlineGlobalPan ("Outline Global Pan", Vector) = (0,0,0,0)
        [Enum(UnityEngine.Rendering.CullMode)] _OutlineCull ("Cull", Float) = 1
        [HideInInspector] m_end_outlineAdvanced ("Advanced", Float) = 0
        
        [HideInInspector] m_emissionOptions ("Emission", Float) = 0
        [HDR]_EmissionColor ("Emission Color", Color) = (1, 1, 1, 1)
        _EmissionMap ("Emission Map", 2D) = "white" { }
        _EmissionMask ("Emission Mask", 2D) = "white" { }
        _EmissionScrollSpeed ("Emission Scroll Speed", Vector) = (0, 0, 0, 0)
        _EmissionStrength ("Emission Strength", Range(0, 20)) = 0
        
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
        
        [HideInInspector] m_fakeLightingOptions ("Lighting", Float) = 0
        [NoScaleOffset]_ToonRamp ("Lighting Ramp", 2D) = "white" { }
        _ShadowStrength ("Shadow Strength", Range(0, 1)) = 1
        _ShadowOffset ("Shadow Offset", Range(-1, 1)) = 0
        _MinBrightness ("Min Brightness", Range(0, 1)) = 0
        _MaxBrightness ("Max Brightness", Float) = 1
        [HideInInspector] m_start_lightingAdvanced ("Advanced", Float) = 0
        _IndirectContribution ("Indirect Contribution", Range(0, 1)) = 0
        _AdditiveSoftness ("Additive Softness", Range(0,0.5)) = 0.005
        _AdditiveOffset("Additive Offset", Range(-.5,.5)) = 0
        [HideInInspector]_AttenuationMultiplier ("Attenuation", Range(0,1)) = 0
        [HideInInspector] m_end_lightingAdvanced ("Advanced", Float) = 0
        /*
        [HideInInspector] m_specularHighlightsOptions ("Specular Highlights", Float) = 0
        _SpecularMap ("Specular Map", 2D) = "white" { }
        _SpecularSmoothness ("Smoothness", Range(0, 1)) = 0
        _SpecularTint ("Specular Tint", Color) = (1, 1, 1, 1)
        _IOR ("IOR", Range(1.0, 5.0)) = 1.45
        _Fresnel("Fresnel", Range(1.0, 8.0)) = 5.0

        [HideInInspector] m_panosphereOptions ("Panosphere", Float) = 0
        _PanosphereTexture ("Panoshpere Texture", 2D) = "white" { }
        _PanoMapTexture ("Pano Map Texture", 2D) = "white" { }
        _PanoEmission ("Pano Emission", Range(0,10)) = 0
        _PanoBlend ("Pano Blend", Range(0,1)) = 0
        _PanosphereColor ("Panosphere Color", Color) = (1, 1, 1, 1)
        _PanosphereScroll ("Panosphere Scrolling", Vector) = (0,0,0,0)

        [HideInInspector] m_rimLightOptions ("Rim Lighting", Float) = 0
        _RimLightColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimWidth ("Rim Width", Range(0, 1)) = 0.8
        _RimStrength ("Rim Emission", Range(0, 20)) = 0
        _RimSharpness ("Rim Sharpness", Range(0, 1)) = .25
        _RimLightColorBias ("Rim Color Bias", Range(0, 1)) = 0
        _RimTex ("Rim Texture", 2D) = "white" { }
        _RimMask("Rim Mask", 2D) = "white" { }
        _RimTexPanSpeed ("Rim Texture Pan Speed", Vector) = (0, 0, 0, 0)
        
        [HideInInspector] m_textureBlending ("Texture Blending", Float) = 0
        [Enum(Off, 0, Soft, 1, Hard, 2)] _Blend ("Blending Type", Int) = 0
        _BlendTextureColor ("Blend Texture Color", Color) = (1, 1, 1, 1)
        [NoScaleOffset]_BlendTexture ("Blend Texture", 2D) = "white" { }
        [NoScaleOffset]_BlendNoiseTexture ("Blend Noise Texture", 2D) = "white" { }
        _BlendAlpha ("Blend Alpha", Range(0, 1)) = 0
        [Toggle(_)]_AutoBlend ("Enable Auto Blending", Float) = 0
        [Gamma]_AutoBlendSpeed ("Auto Blend Speed", Float) = 2
        [Gamma]_AutoBlendDelay ("Auto Blend Delay", Float) = 2
*/
        [HideInInspector] m_StencilPassOptions ("Stencil", Float) = 0
        [IntRange] _StencilRef ("Stencil Reference Value", Range(0, 255)) = 0
        [IntRange] _StencilReadMaskRef ("Stencil ReadMask Value", Range(0, 255)) = 0
        [IntRange] _StencilWriteMaskRef ("Stencil WriteMask Value", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilPassOp ("Stencil Pass Op", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilFailOp ("Stencil Fail Op", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFailOp ("Stencil ZFail Op", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilCompareFunction ("Stencil Compare Function", Float) = 8
        
        [HideInInspector] m_start_OutlineStencil ("Outline Stencil", Float) = 0
        [IntRange] _OutlineStencilRef ("Stencil Reference Value", Range(0, 255)) = 0
        [IntRange] _OutlineStencilReadMaskRef ("Stencil ReadMask Value", Range(0, 255)) = 0
        [IntRange] _OutlineStencilWriteMaskRef ("Stencil WriteMask Value", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _OutlineStencilPassOp ("Stencil Pass Op", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _OutlineStencilFailOp ("Stencil Fail Op", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _OutlineStencilZFailOp ("Stencil ZFail Op", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _OutlineStencilCompareFunction ("Stencil Compare Function", Float) = 8
        [HideInInspector] m_end_OutlineStencil ("Outline Stencil", Float) = 0
        
                
        [HideInInspector] m_funOptions("Fun", Float) = 0
        [Enum(ShowInBoth, 0, ShowOnlyInMirror, 1, DontShowInMirror, 2)] _Mirror ("Show in mirror", Int) = 0
        
        [HideInInspector] m_miscOptions ("Misc", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4
        [Enum(UnityEngine.Rendering.BlendMode)] _SourceBlend ("Source Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DestinationBlend ("Destination Blend", Float) = 10
        [Enum(Off, 0, On, 1)] _ZWrite ("ZWrite", Int) = 0
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
                ReadMask [_StencilReadMaskRef]
                WriteMask [_StencilWriteMaskRef]
                Ref [_StencilRef]
                Comp [_StencilCompareFunction]
                Pass [_StencilPassOp]
                Fail [_StencilFailOp]
                ZFail [_StencilZFailOp]
            }
            ZWrite [_ZWrite] 
            Cull [_Cull]
            ZTest [_ZTest]
            CGPROGRAM
            #pragma target 3.0
            #define TRANSPARENT
            #define GOTTA_GO_FAST
            #define FORWARD_BASE_PASS
            #pragma multi_compile_instancing
            #pragma multi_compile_fwdbase
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_fog
            #pragma vertex vert
            #pragma fragment frag
            #include "Includes/PoiPass.cginc"
            ENDCG
        }
        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }
            Stencil
            {
                Ref [_StencilRef]
                ReadMask [_StencilReadMaskRef]
                WriteMask [_StencilWriteMaskRef]
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
            CGPROGRAM
            #pragma target 3.0
            #define TRANSPARENT
            #define GOTTA_GO_FAST
            #define FORWARD_ADD_PASS
            #pragma multi_compile_instancing
            #pragma multi_compile_fwdadd
            #pragma vertex vert
            #pragma fragment frag
            #include "Includes/PoiPass.cginc"
            ENDCG
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "ForwardBase" }
            Stencil
            {
                Ref [_OutlineStencilRef]
                ReadMask [_OutlineStencilReadMaskRef]
                WriteMask [_OutlineStencilWriteMaskRef]
                Ref [_OutlineStencilRef]
                Comp [_OutlineStencilCompareFunction]
                Pass [_OutlineStencilPassOp]
                Fail [_OutlineStencilFailOp]
                ZFail [_OutlineStencilZFailOp]
            }
            ZTest [_ZTest]
            ZWrite [_ZWrite] 
            Cull [_OutlineCull]
            CGPROGRAM
            #pragma target 3.0
            #define TRANSPARENT
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag
            #include "Includes/PoiPassOutline.cginc"
            ENDCG
        }

        Pass
        {
            Tags { "LightMode" = "ShadowCaster" }
            Stencil
            {
                Ref [_StencilRef]
                ReadMask [_StencilReadMaskRef]
                WriteMask [_StencilWriteMaskRef]
                Ref [_StencilRef]
                Comp [_StencilCompareFunction]
                Pass [_StencilPassOp]
                Fail [_StencilFailOp]
                ZFail [_StencilZFailOp]
            }
            CGPROGRAM
            #pragma target 3.0
            #define TRANSPARENT
            #pragma multi_compile_instancing
            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster
            #include "Includes/PoiPassShadow.cginc"
            ENDCG
        }
    }
}
