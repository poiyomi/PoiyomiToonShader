Shader ".poiyomi/Toon/Opaque"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Desaturation ("Desaturation", Range(-1, 1)) = 0
        _MainTex ("Texture", 2D) = "white" { }
        [Normal]_NormalMap ("Normal Map", 2D) = "bump" { }
        _NormalIntensity ("Normal Intensity", Range(0, 10)) = 1
        
        [Header(Emission)]
        _EmissionColor ("Emission Color", Color) = (1, 1, 1, 1)
        _EmissionMap ("Emission Map", 2D) = "white" { }
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
        _LightDirection ("Fake Light Direction", Vector) = (0, 1, 0, 0)
        
        [Header(Specular Highlights)]
        _SpecularMap ("Specular Map", 2D) = "white" { }
        _Gloss ("Glossiness", Range(0, 1)) = 0
        _SpecularColor ("Specular Color", Color) = (1, 1, 1, 1)
        _SpecularBias ("Specular Color Bias", Range(0, 1)) = 0
        _SpecularStrength ("Specular Strength", Range(0, 1)) = 0
        [Toggle(_HARD_SPECULAR)]_HARD_SPECULAR ("Enable Hard Specular", Float) = 0
        _SpecularSize ("Hard Specular Size", Range(0, 1)) = .005
        /*
        [Header(Outlines)]
        _LineWidth ("Outline Width", Float) = 0
        _LineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineEmission ("Outline Emission", Float) = 0
        _OutlineTexture ("Outline Texture", 2D) = "white" { }
        _Speed ("Speed", Float) = 0.05
        */
        [Header(Rim Lighting)]
        _RimLightColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimWidth ("Rim Width", Range(0, 1)) = 0.8
        _RimStrength ("Rim Emission", Range(0, 20)) = 0
        _RimSharpness ("Rim Sharpness", Range(0, 1)) = .25
        _RimLightColorBias ("Rim Color Bias", Range(0, 1)) = 0
        _RimTex ("Rim Texture", 2D) = "white" { }
        _RimTexPanSpeed ("Rim Texture Pan Speed", Vector) = (0, 0, 0, 0)
        
        [Header(Misc)]
        [Toggle(_LIT)] _Lit ("Flat Lit?", Float) = 1
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
        _Clip ("Clipping", Range(0, 1.001)) = 0.5
    }
    
    SubShader
    {
        Tags { "RenderType" = "TransparentCutout" }
        
        Pass
        {
            Name "Outline"
            Tags {  }
            Cull Front
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma only_renderers d3d9 d3d11 glcore gles
            #pragma target 3.0
            uniform float _LineWidth;
            uniform float _OutlineEmission;
            uniform float4 _LineColor;
            uniform sampler2D _OutlineTexture; uniform float4 _OutlineTexture_ST;
            uniform float _Speed;
            struct VertexInput
            {
                float4 vertex: POSITION;
                float3 normal: NORMAL;
                float2 texcoord0: TEXCOORD0;
            };
            struct VertexOutput
            {
                float4 pos: SV_POSITION;
                float2 uv0: TEXCOORD0;
            };
            VertexOutput vert(VertexInput v)
            {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(float4(v.vertex.xyz + v.normal * _LineWidth / 10000, 1));
                return o;
            }
            float4 frag(VertexOutput i, float facing: VFACE): COLOR
            {
                clip(_LineWidth - 0.001);
                float isFrontFace = (facing >= 0 ? 1: 0);
                float faceSign = (facing >= 0 ? 1: - 1);
                fixed4 col = fixed4(tex2D(_OutlineTexture, TRANSFORM_TEX((i.uv0 + (_Speed * _Time.g)), _OutlineTexture)).rgb, 0) * _LineColor;
                return col + col * _OutlineEmission;
            }
            ENDCG
            
        }
        
        Pass
        {
            Name "MainPass"
            Tags { "LightMode" = "ForwardBase" "RenderType" = "TransparentCutout" }
            Stencil
            {
                Ref 173 Comp NotEqual Pass keep
            }
            Cull [_Cull]
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _LIT
            #pragma shader_feature _HARD_SPECULAR
            #pragma shader_feature _SCROLLING_EMISSION
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            
            //Structs
            struct appdata
            {
                float4 vertex: POSITION;
                float3 normal: NORMAL;
                float4 tangent: TANGENT;
                float2 texcoord: TEXCOORD0;
                float2 texcoord1: TEXCOORD1;
            };
            
            struct v2f
            {
                float2 uv: TEXCOORD0;
                float3 normalDir: TEXCOORD4;
                float3 cameraToVert: TEXCOORD5;
                float3 tangentDir: TEXCOORD6;
                float3 bitangentDir: TEXCOORD7;
                float4 pos: SV_POSITION;
                float4 posWorld: TEXCOORD8;
                float4 posLocal: TEXCOORD9;
            };
            
            //Properties
            float4 _Color;
            float _Desaturation;
            sampler2D _MainTex; float4 _MainTex_ST;
            sampler2D _NormalMap; float4 _NormalMap_ST;
            float _NormalIntensity;
            sampler2D _SpecularMap; float4 _SpecularMap_ST;
            float _Gloss;
            float4 _EmissionColor;
            sampler2D _EmissionMap; float4 _EmissionMap_ST;
            float _EmissionStrength;
            
            uniform float4 _EmissiveScroll_Direction;
            uniform float _EmissiveScroll_Width;
            uniform float _EmissiveScroll_Velocity;
            uniform float _EmissiveScroll_Interval;
            uniform float _EmissiveBlink_Min;
            uniform float _EmissiveBlink_Max;
            uniform float _EmissiveBlink_Velocity;
            
            sampler2D _LightingGradient;
            float _ShadowStrength;
            float _ShadowOffset;
            float4 _LightDirection;
            float4 _SpecularColor;
            float _SpecularBias;
            float _SpecularStrength;
            float _SpecularSize;
            
            float4 _RimLightColor;
            float _RimWidth;
            float _RimStrength;
            float _RimSharpness;
            float _RimLightColorBias;
            float4 _RimTexPanSpeed;
            sampler2D _RimTex; float4 _RimTex_ST;
            
            float _Clip;
            
            float3 getCameraPosition()
            {
                #ifdef USING_STEREO_MATRICES
                    return lerp(unity_StereoWorldSpaceCameraPos[0], unity_StereoWorldSpaceCameraPos[1], 0.5);
                #endif
                return _WorldSpaceCameraPos;
            }
            
            float3 getCameraForward()
            {
                #if UNITY_SINGLE_PASS_STEREO
                    float3 p1 = mul(unity_StereoCameraToWorld[0], float4(0, 0, 1, 1));
                    float3 p2 = mul(unity_StereoCameraToWorld[0], float4(0, 0, 0, 1));
                #else
                    float3 p1 = mul(unity_CameraToWorld, float4(0, 0, 1, 1));
                    float3 p2 = mul(unity_CameraToWorld, float4(0, 0, 0, 1));
                #endif
                return normalize(p2 - p1);
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                TANGENT_SPACE_ROTATION;
                o.posLocal = v.vertex;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.cameraToVert = normalize(getCameraPosition() - mul(unity_ObjectToWorld, v.vertex));
                o.normalDir = normalize(mul(unity_ObjectToWorld, v.normal));
                o.uv = float4(v.texcoord.xy, 0, 0);
                
                o.tangentDir = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                
                return o;
            }
            
            fixed4 frag(v2f i, float facing: VFACE): SV_Target
            {
                fixed4 mainTexVar = tex2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex));
                fixed4 col = mainTexVar;
                col.rgb = lerp(col.rgb, dot(col.rgb, float3(0.3, 0.59, 0.11)), _Desaturation);
                col.rgb *= _Color.rgb;
                
                float3x3 tangentTransform = float3x3(i.tangentDir, i.bitangentDir, i.normalDir);
                float3 _Normal_var = UnpackNormal(tex2D(_NormalMap, TRANSFORM_TEX(i.uv, _NormalMap)));
                float3 normalLocal = lerp(float3(0, 0, 1), _Normal_var, _NormalIntensity);
                float3 normalDirection = normalize(mul(normalLocal, tangentTransform));
                
                clip(col.a - _Clip);
                
                float3 rimColor = lerp(col.rgb, tex2D(_RimTex, TRANSFORM_TEX(i.uv, _RimTex) + (_Time.y * _RimTexPanSpeed.xy)) * _RimLightColor, _RimLightColorBias);
                float3 cameraVertDot = abs(dot(i.cameraToVert, normalDirection));
                
                half rim = pow((1 - cameraVertDot), (1 - _RimWidth) * 10);
                _RimSharpness /= 2;
                rim = (smoothstep(_RimSharpness, 1 - _RimSharpness, rim));
                col.rgb = lerp(col.rgb, rimColor, rim * _RimLightColor.a);
                
                float FakeLight = clamp((dot(normalDirection, normalize(_LightDirection)) + 1) / 2 + _ShadowOffset, 0, 1);
                float4 LightColor = tex2D(_LightingGradient, float2(FakeLight, FakeLight));
                float Pi = 3.141592654;
                
                float gloss = _Gloss;
                float specPow = exp2(gloss * 10.0 + 1.0);
                float specMap_var = tex2D(_SpecularMap, TRANSFORM_TEX(i.uv, _SpecularMap));
                float3 halfDirection = normalize(i.cameraToVert + _LightDirection);
                float3 specularColor = ((col.a * _SpecularStrength * specMap_var) * lerp(_SpecularColor.rgb, col.rgb, 1 - _SpecularBias));
                float normTerm = (specPow + 10) / (10 * Pi);
                #if _HARD_SPECULAR
                    float3 specular = pow(max(0, step(1 - dot(halfDirection, normalDirection), _SpecularSize)), specPow) * normTerm * specularColor;
                #else
                    float3 specular = pow(max(0, dot(halfDirection, normalDirection)), specPow) * normTerm * specularColor;
                #endif
                
                col *= saturate(LightColor + (1 - _ShadowStrength));
                col.rgb += specular;
                
                #if _LIT
                    float attenuation = LIGHT_ATTENUATION(i) / SHADOW_ATTENUATION(i);
                    float3 FlatLighting = saturate(ShadeSH9(half4(float3(0, 1, 0), 1.0)) + (_LightColor0.rgb * attenuation));
                    col.rgb *= FlatLighting;
                #endif
                
                col.rgb += rim * rimColor * _RimStrength;
                
                float4 _Emissive_Tex_var = tex2D(_EmissionMap, TRANSFORM_TEX(i.uv, _EmissionMap));
                float4 emissionVar = _Emissive_Tex_var * _EmissionColor * _EmissionStrength;
                
                #if _SCROLLING_EMISSION
                    float phase = dot(i.posLocal, _EmissiveScroll_Direction);
                    phase -= _Time.y * _EmissiveScroll_Velocity;
                    phase /= _EmissiveScroll_Interval;
                    phase -= floor(phase);
                    float width = _EmissiveScroll_Width;
                    phase = (pow(phase, width) + pow(1 - phase, width * 4)) * 0.5;
                    emissionVar *= phase;
                #endif
                
                float amplitude = (_EmissiveBlink_Max - _EmissiveBlink_Min) * 0.5f;
                float base = _EmissiveBlink_Min + amplitude;
                float emissiveBlink = sin(_Time.y * _EmissiveBlink_Velocity) * amplitude + base;
                emissionVar *= emissiveBlink;
                
                col.rgb += emissionVar;
                return col;
            }
            ENDCG
            
        }
        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }
            ZWrite Off Blend One One
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdadd_fullshadows
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            
            //Structs
            struct appdata
            {
                float4 vertex: POSITION;
                float3 normal: NORMAL;
                float4 tangent: TANGENT;
                float2 texcoord: TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv: TEXCOORD0;
                float4 pos: SV_POSITION;
                float4 posWorld: TEXCOORD1;
                SHADOW_COORDS(2)
            };
            
            float4 _Color;
            float _Desaturation;
            sampler2D _MainTex; float4 _MainTex_ST;
            
            float _Clip;
            
            v2f vert(appdata v)
            {
                v2f o;
                TANGENT_SPACE_ROTATION;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.uv = float4(v.texcoord.xy, 0, 0);
                
                TRANSFER_SHADOW(o);
                return o;
            }
            
            fixed4 frag(v2f i, float facing: VFACE): SV_Target
            {
                fixed4 col = tex2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex));
                col.rgb = lerp(col.rgb, dot(col.rgb, float3(0.3, 0.59, 0.11)), _Desaturation);
                col.rgb *= _Color.rgb;
                clip(col.a - _Clip);
                UNITY_LIGHT_ATTENUATION(atten, i, i.posWorld.xyz)
                fixed4 c = atten;
                c.rgb *= _LightColor0.rgb;
                return col * c;
            }
            ENDCG
            
        }
        UsePass "VertexLit/SHADOWCASTER"
    }
}