// Upgrade NOTE: replaced 'UNITY_PASS_TEXCUBE(unity_SpecCube1)' with 'UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1,unity_SpecCube0)'

Shader ".poiyomi/Bloomer"
{
    Properties
    {
        
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Desaturation ("Desaturation", Range(-1, 1)) = 0
        _MainTex ("Texture", 2D) = "white" { }
        [Normal]_NormalMap ("Normal Map", 2D) = "bump" { }
        _NormalIntensity ("Normal Intensity", Range(0, 10)) = 1
        
        [Header(Post Processing)]
        [MaterialToggle] _Inverted ("Inverted?", Range(0, 1)) = 0
        [MaterialToggle] _PPNormalize ("Normalize?", Range(0, 1)) = 0
        _BlurDistance ("Blur Distance", Float) = 0

        [Header(Reflections)]
        _CubeMap ("Cube Reflection", Cube) = "" { }
        [MaterialToggle]_SampleWorld ("Force Baked Cubemap", Range(0, 1)) = 0
        _MetallicMap ("Metallic Map", 2D) = "white" { }
        _Metallic ("Metallic", Range(0, 1)) = 0
        _RoughnessMap ("Roughness Map", 2D) = "white" { }
        _Roughness ("Roughness", Range(0, 1)) = 0
        
        [Header(Emission)]
        [HDR]_EmissionColor ("Emission Color", Color) = (1, 1, 1, 1)
        _EmissionMap ("Emission Map", 2D) = "white" { }
        _EmissionScrollSpeed ("Emission Scroll Speed", Vector) = (0, 0, 0, 0)
        _EmissionStrength ("Emission Strength", Range(0, 20)) = 0
        
        [Header(Blinking Emission)]
        _EmissiveBlink_Min ("Emissive Blink Min", Float) = 1
        _EmissiveBlink_Max ("Emissive Blink Max", Float) = 1
        _EmissiveBlink_Velocity ("Emissive Blink Velocity", Float) = 4
        
        [Header(Scrolling Emission)]
        [Toggle(_SCROLLING_EMISSION)] _SCROLLING_EMISSION ("Enable Scrolling Emission", Float) = 0
        _EmissiveScroll_Direction ("Emissive Scroll Direction", Vector) = (0, -10, 0, 0)
        _EmissiveScroll_Width ("Emissive Scroll Width", Float) = 10
        _EmissiveScroll_Velocity ("Emissive Scroll Velocity", Float) = 10
        _EmissiveScroll_Interval ("Emissive Scroll Interval", Float) = 20
        
        [Header(Fake Lighting)]
        [NoScaleOffset]_LightingGradient ("Lighting Ramp", 2D) = "white" { }
        _ShadowStrength ("Shadow Strength", Range(0, 1)) = 0.25
        _ShadowOffset ("Shadow Offset", Range(-1, 1)) = 0
        [MaterialToggle] _ForceLightDirection ("Force Light Direction", Range(0, 1)) = 0
        _LightDirection ("Fake Light Direction", Vector) = (0, 1, 0, 0)
        
        [Header(Specular Highlights)]
        _SpecularMap ("Specular Map", 2D) = "white" { }
        _Gloss ("Glossiness", Range(0, 1)) = 0
        _SpecularColor ("Specular Color", Color) = (1, 1, 1, 1)
        _SpecularBias ("Specular Color Bias", Range(0, 1)) = 0
        _SpecularStrength ("Specular Strength", Range(0, 5)) = 0
        [Toggle(_HARD_SPECULAR)]_HARD_SPECULAR ("Enable Hard Specular", Float) = 0
        _SpecularSize ("Hard Specular Size", Range(0, 1)) = .005
        
        [Header(Outlines)]
        _LineWidth ("Outline Width", Float) = 0
        _LineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineEmission ("Outline Emission", Float) = 0
        _OutlineTexture ("Outline Texture", 2D) = "white" { }
        _Speed ("Speed", Float) = 0.05
        
        [Header(Rim Lighting)]
        _RimLightColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimWidth ("Rim Width", Range(0, 1)) = 0.8
        _RimStrength ("Rim Emission", Range(0, 20)) = 0
        _RimSharpness ("Rim Sharpness", Range(0, 1)) = .25
        _RimLightColorBias ("Rim Color Bias", Range(0, 1)) = 0
        _RimTex ("Rim Texture", 2D) = "white" { }
        _RimTexPanSpeed ("Rim Texture Pan Speed", Vector) = (0, 0, 0, 0)
        
        [IntRange] _StencilRef ("Stencil Reference Value", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Op", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilCompareFunction ("Stencil Compare Function", Float) = 0
        
        [Header(Misc)]
        [Toggle(_LIT)] _Lit ("Flat Lit?", Float) = 1
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4
        [Enum(UnityEngine.Rendering.BlendMode)] _SourceBlend ("Source Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DestinationBlend ("Destination Blend", Float) = 10
        _Clip ("Clipping", Range(0, 1.001)) = 0.5
    }
    //CustomEditor "PoiToonOutline"
    
    SubShader
    {
        Tags { "RenderType" = "TransparentCutout" "Queue" = "AlphaTest" }
        
        Pass
        {
            Name "MainPass"
            Tags { "LightMode" = "ForwardBase" }
            Stencil
            {
                Ref [_StencilRef]
                Comp [_StencilCompareFunction]
                Pass [_StencilOp]
            }
            Cull [_Cull]
            ZTest [_ZTest]
            CGPROGRAM
            
            #pragma target 3.0
            
            #pragma shader_feature _LIT
            #pragma shader_feature _HARD_SPECULAR
            #pragma shader_feature _SCROLLING_EMISSION
            
            #pragma vertex vert
            #pragma fragment frag
            #define FORWARD_BASE_PASS
            #include "PoiPass.cginc"
            
            ENDCG
            
        }
        
        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }
            ZWrite Off Blend One One
            Stencil
            {
                Ref [_StencilRef]
                Comp [_StencilCompareFunction]
                Pass [_StencilOp]
            }
            Cull [_Cull]
            ZTest [_ZTest]
            CGPROGRAM
            
            #pragma target 3.0
            #pragma shader_feature _LIT
            #pragma shader_feature _HARD_SPECULAR
            #pragma shader_feature _SCROLLING_EMISSION
            #pragma multi_compile DIRECTIONAL POINT SPOT
            #pragma vertex vert
            #pragma fragment frag
            
            #include "PoiPass.cginc"
            ENDCG
            
        }
        UsePass "VertexLit/SHADOWCASTER"
        GrabPass
        {
            "_GrabThePP"
        }
        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            sampler2D _GrabThePP;
            float _Inverted;
            float _BlurDistance;
            float _PPNormalize;

            struct appdata
            {
                float4 vertex: POSITION;
                float3 normal: NORMAL;
                float2 uv: TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos: SV_POSITION;
                float4 screenCoord: TEXCOORD1;
                float4 grabPos: TEXCOORD2;
                float3 objectDistance: TEXCOORD3;
            };
            
            //functions
            half3 Sample(float4 uv)
            {
                return tex2Dproj(_GrabThePP, uv).rgb;
            }
            
            half3 SampleBox(float4 uv, float delta)
            {
                float2 mainTexelSize = float2(1 / _ScreenParams.x, 1 / _ScreenParams.y);
                float4 o = mainTexelSize.xyxy * float2(-delta, delta).xxyy;
                half3 s = Sample(uv + float4(o.x, o.y, 0, 0)) + Sample(uv + float4(0, o.y, o.z, 0)) +
                Sample(uv + float4(o.x, 0, 0, o.w)) + Sample(uv + float4(0, 0, o.z, o.w));
                return s * 0.25f;
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(float4(v.vertex.xyz + v.normal * .1 / 10000, 1));
                o.screenCoord.xy = ComputeScreenPos(o.pos);
                o.grabPos = ComputeGrabScreenPos(o.pos);
                return o;
            }
            
            fixed4 frag(v2f i): SV_Target
            {
                float3 c = tex2Dproj(_GrabThePP, i.grabPos).xyz;
                c.rgb = SampleBox(i.grabPos, _BlurDistance);

                if (_Inverted == 1)
                {
                    c.rgb = 1 - c.rgb;
                }
                if (_PPNormalize == 1)
                {
                    c.rgb = normalize(c.rgb);
                }
                return float4(c, 1);
            }
            ENDCG
            
        }
    }
}