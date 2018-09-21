/*
poiyomi master shader
version 0.2
*/

Shader ".poiyomi/Master/Transparent"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" { }
        [Normal]_NormalMap ("Normal Map", 2D) = "bump" { }
        _NormalIntensity ("Normal Intensity", Range(0, 10)) = 1
        
        [Toggle(_RGB_MASK)]_RGB_MASK ("Enable RGB Mask", Float) = 0
        _RGBMask ("RGB Mask", 2D) = "black" { }
        _RedTexture ("Red Texture", 2D) = "white" { }
        _GreenTexture ("Green Texture", 2D) = "white" { }
        _BlueTexture ("Blue Texture", 2D) = "white" { }
        
        _LineWidth ("Outline Width", Float) = 0
        _LineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineEmission ("Outline Emission", Float) = 0
        _OutlineTexture ("Outline Texture", 2D) = "white" { }
        _Speed ("Speed", Float) = 0.05
        
        [Toggle(_EMISSION)]_EMISSION ("Enable Emission", Float) = 0
        _EmissionColor ("Emission Color", Color) = (1, 1, 1, 1)
        _EmissionMap ("Emission Map", 2D) = "white" { }
        _EmissionStrength ("Emission Strength", Range(0, 5)) = 0
        
        [Toggle(_BLINKING_EMISSION)]_BLINKING_EMISSION ("Enable Blinking Emission", Float) = 0
        _EmissiveBlink_Min ("Emissive Blink Min", Float) = 1
        _EmissiveBlink_Max ("Emissive Blink Max", Float) = 3
        _EmissiveBlink_Velocity ("Emissive Blink Velocity", Float) = 4
        
        [Toggle(_SCROLLING_EMISSION)]_SCROLLING_EMISSION ("Enable Scrolling Emission", Float) = 0
        _EmissiveScroll_Direction ("Emissive Scroll Direction", Vector) = (0, -10, 0, 0)
        _EmissiveScroll_Width ("Emissive Scroll Width", Float) = 10
        _EmissiveScroll_Velocity ("Emissive Scroll Velocity", Float) = 10
        _EmissiveScroll_Interval ("Emissive Scroll Interval", Float) = 20
        
        [Toggle(_FAKE_LIGHTING)]_FAKE_LIGHTING ("Enable Fake Lighting", Float) = 1
        [NoScaleOffset]_LightingGradient ("Lighting Ramp", 2D) = "white" { }
        _ShadowStrength ("Shadow Strength", Range(0, 1)) = 0.25
        _LightDirection ("Fake Light Direction", Vector) = (0, 1, 0, 0)
        [Space]
        [Toggle(_SPECULAR_HIGHLIGHTS)]_SPECULAR_HIGHLIGHTS ("Enable Specular Highlights (requires Fake Lighting)", Float) = 0
        [Toggle(_HARD_SPECULAR)]_HARD_SPECULAR ("Enable Hard Cutoff", Float) = 0
        _Gloss ("Gloss", Range(0, 1)) = 0
        _SpecularMap ("Specular Map", 2D) = "white" { }
        _SpecularColor ("Specular Color", Color) = (1, 1, 1, 1)
        _SpecularStrength ("Specular Strength", Range(0, 1)) = .2
        _SpecularSize ("Specular Size", Range(0, 1)) = .05
        
        [KeywordEnum(Off, Soft, Hard)] _RimGlow ("Rim Glow Type", Float) = 0
        [Toggle(_INVERT_RIM)]_INVERT_RIM ("Invert Rim Lighting", Float) = 0
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimGlowStrength ("Rim Glow Strength", Range(0, 10)) = 1
        _RimWidth ("Rim Width", Range(0, 1)) = .5
        _RimColorBias ("Rim Color Bias", Range(0, 1)) = 1
        _RimTexTile ("Rim Texture Tile", Float) = 1
        [NoScaleOffset]_RimTex ("Rim Texture", 2D) = "white" { }
        _RimTexPanSpeed ("Rim Texture Pan Speed", Vector) = (0, 0, 0, 0)
        [Space]
        
        [KeywordEnum(Off, Soft, Hard)] _Blend ("Blending Type", Float) = 0
        _BlendTextureColor ("Blend Texture Color", Color) = (1, 1, 1, 1)
        [NoScaleOffset]_BlendTexture ("Blend Texture", 2D) = "white" { }
        [NoScaleOffset]_BlendNoiseTexture ("Blend Noise Texture", 2D) = "white" { }
        _BlendAlpha ("Blend Alpha", Range(0, 1)) = 0
        _BlendTiling ("Blend Tiling", Float) = 1
        [Toggle(_AUTO_BLEND)]_AUTO_BLEND ("Enable Auto Blending", Float) = 0
        [Gamma]_AutoBlendSpeed ("Auto Blend Speed", Float) = 2
        [Gamma]_AutoBlendDelay ("Auto Blend Delay", Float) = 2
        
        [KeywordEnum(Off, One, Two, Three)] _OVERLAY ("Number of Overlays", Float) = 0
        _OverlayColor1 ("Color 1", Color) = (1, 1, 1, 1)
        _OverlayTexture1 ("Overlay Texture 1", 2D) = "white" { }
        _Tex1Velocity ("Texture 1 Velocity", Vector) = (0, 0, 0, 0)
        _OverlayColor2 ("Color 2", Color) = (1, 1, 1, 1)
        _OverlayTexture2 ("Overlay Texture 2", 2D) = "white" { }
        _Tex2Velocity ("Texture 2 Velocity", Vector) = (0, 0, 0, 0)
        _OverlayColor3 ("Color 3", Color) = (1, 1, 1, 1)
        _OverlayTexture3 ("Overlay Texture 3", 2D) = "white" { }
        _Tex3Velocity ("Texture 3 Velocity", Vector) = (0, 0, 0, 0)
        
        [Toggle(_LIT)] _Lit ("Flat Lit?", Float) = 1
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4
        _Clip ("Clipping", Range(0, 1.001)) = 0.0
    }
    CustomEditor "PoiMaster"
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        
        Pass
        {
            Name "MyShader"
            Tags { "LightMode" = "ForwardBase" }
            Stencil
            {
                Ref 901 Comp Equal Pass keep
            }
            ZWrite On ZTest Always Cull Off
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
            struct appdata
            {
                float4 vertex: POSITION; float2 uv: TEXCOORD0;
            };
            struct v2f
            {
                UNITY_FOG_COORDS(1) float4 vertex: SV_POSITION;
            };
            v2f vert(appdata v)
            {
                v2f o; o.vertex = UnityObjectToClipPos(v.vertex);	return o;
            }
            float4 frag(v2f i): COLOR
            {
                return float4(243.0 / 255.0, 156.0 / 255.0, 18.0 / 255.0, 1);
            }
            ENDCG
            
        }
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
            Tags { "LightMode" = "ForwardBase" "RenderType" = "TransparentCutout" }
            Stencil
            {
                Ref 901 Comp NotEqual Pass keep
            }
            Cull [_Cull]
            ZTest [_ZTest]
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #define _GLOSSYENV 1
            #pragma shader_feature _LIT
            #pragma shader_feature _EMISSION
            #pragma shader_feature _RGB_MASK
            #pragma shader_feature _SCROLLING_EMISSION
            #pragma shader_feature _BLINKING_EMISSION
            #pragma shader_feature _FAKE_LIGHTING
            #pragma shader_feature _SPECULAR_HIGHLIGHTS
            #pragma shader_feature _HARD_SPECULAR
            #pragma multi_compile _BLEND_OFF _BLEND_HARD _BLEND_SOFT
            #pragma shader_feature _AUTO_BLEND
            #pragma multi_compile _RIMGLOW_OFF _RIMGLOW_HARD _RIMGLOW_SOFT
            #pragma shader_feature _INVERT_RIM
            #pragma multi_compile _OVERLAY_OFF _OVERLAY_ONE _OVERLAY_TWO _OVERLAY_THREE
            
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
                float phase: TEXCOORD9;
            };
            
            //Propertiesv
            float4 _Color;
            sampler2D _MainTex; float4 _MainTex_ST;
            sampler2D _NormalMap; float4 _NormalMap_ST;
            float _NormalIntensity;
            sampler2D _SpecularMap; float4 _SpecularMap_ST;
            float _Gloss;
            float4 _EmissionColor;
            sampler2D _EmissionMap; float4 _EmissionMap_ST;
            float _EmissionStrength;
            
            sampler2D _RGBMask; float4 _RGBMask_ST;
            sampler2D _RedTexture; float4 _RedTexture_ST;
            sampler2D _GreenTexture; float4 _GreenTexture_ST;
            sampler2D _BlueTexture; float4 _BlueTexture_ST;
            
            uniform float4 _EmissiveScroll_Direction;
            uniform float _EmissiveScroll_Width;
            uniform float _EmissiveScroll_Velocity;
            uniform float _EmissiveScroll_Interval;
            uniform float _EmissiveBlink_Min;
            uniform float _EmissiveBlink_Max;
            uniform float _EmissiveBlink_Velocity;
            
            float4 _BlendTextureColor;
            sampler2D _BlendTexture;
            sampler2D _BlendNoiseTexture;
            float _BlendAlpha;
            float _BlendTiling;
            float _AutoBlendSpeed;
            float _AutoBlendDelay;
            
            sampler2D _LightingGradient;
            float _ShadowStrength;
            float4 _LightDirection;
            float4 _SpecularColor;
            float _SpecularStrength;
            float _SpecularSize;
            
            float _RimTexTile;
            float4 _RimColor;
            float _RimGlowStrength;
            float _RimWidth;
            float _RimColorBias;
            float4 _RimTexPanSpeed;
            sampler2D _RimTex;
            
            float4 _OverlayColor1;
            sampler2D _OverlayTexture1; float4 _OverlayTexture1_ST;
            float4 _Tex1Velocity;
            float4 _OverlayColor2;
            sampler2D _OverlayTexture2; float4 _OverlayTexture2_ST;
            float4 _Tex2Velocity;
            float4 _OverlayColor3;
            sampler2D _OverlayTexture3; float4 _OverlayTexture3_ST;
            float4 _Tex3Velocity;
            
            float _Clip;
            
            //Functions
            float3 LightingFunction(float3 normal)
            {
                return ShadeSH9(half4(normal, 1.0));
            }
            
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
            
            float map(float s, float a1, float a2, float b1, float b2)
            {
                return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
            }
            
            //shader programs
            v2f vert(appdata v)
            {
                v2f o;
                TANGENT_SPACE_ROTATION;
                
                o.pos = UnityObjectToClipPos(v.vertex);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.cameraToVert = normalize(getCameraPosition() - mul(unity_ObjectToWorld, v.vertex));
                o.normalDir = normalize(mul(unity_ObjectToWorld, v.normal));
                o.uv = float4(v.texcoord.xy, 0, 0);
                
                o.tangentDir = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                
                o.phase = dot(v.vertex, _EmissiveScroll_Direction);
                o.phase -= _Time.y * _EmissiveScroll_Velocity;
                o.phase /= _EmissiveScroll_Interval;
                o.phase -= floor(o.phase);
                float width = _EmissiveScroll_Width;
                o.phase = (pow(o.phase, width) + pow(1 - o.phase, width * 4)) * 0.5;
                
                return o;
            }
            
            fixed4 frag(v2f i, float facing: VFACE): SV_Target
            {
                fixed4 col = tex2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex));
                col *= _Color;
                
                #ifdef _RGB_MASK
                    
                    fixed4 rgbMask_var = tex2D(_RGBMask, TRANSFORM_TEX(i.uv, _RGBMask));
                    fixed4 redTexture_var = tex2D(_RedTexture, TRANSFORM_TEX(i.uv, _RedTexture));
                    fixed4 greenTexture_var = tex2D(_GreenTexture, TRANSFORM_TEX(i.uv, _GreenTexture));
                    fixed4 blueTexture_var = tex2D(_BlueTexture, TRANSFORM_TEX(i.uv, _BlueTexture));
                    
                    col = lerp(lerp(lerp(col, blueTexture_var, rgbMask_var.b), greenTexture_var, rgbMask_var.g), redTexture_var, rgbMask_var.r);
                    
                #endif
                
                float facing_ing = dot(i.normalDir, i.cameraToVert);
                float isFrontFace = (facing_ing >= 0 ? 1: 0);
                float faceSign = (facing_ing >= 0 ? 1: - 1);
                i.normalDir *= faceSign;
                
                float3x3 tangentTransform = float3x3(i.tangentDir, i.bitangentDir, i.normalDir);
                float3 _Normal_var = UnpackNormal(tex2D(_NormalMap, TRANSFORM_TEX(i.uv, _NormalMap)));
                float3 normalLocal = lerp(float3(0, 0, 1), _Normal_var.rgb, _NormalIntensity);
                float3 normalDirection = normalize(mul(normalLocal, tangentTransform)); // Perturbed normals
                
                #ifdef _BLEND_SOFT
                    fixed4 blendCol = tex2D(_BlendTexture, i.uv) * _BlendTextureColor;
                    fixed blendNoise = tex2D(_BlendNoiseTexture, i.uv * _BlendTiling);
                    #ifdef _AUTO_BLEND
                        _BlendAlpha = (sin(_Time.y * _AutoBlendSpeed) + 1) / 2;
                    #endif
                    float blendAlpha = _BlendAlpha * 3 - 1;
                    blendAlpha = clamp(blendAlpha - blendNoise, 0, 1);
                    col = lerp(col, blendCol, blendAlpha);
                #elif _BLEND_HARD
                    fixed4 blendCol = tex2D(_BlendTexture, i.uv) * _BlendTextureColor;
                    fixed blendNoise = tex2D(_BlendNoiseTexture, i.uv * _BlendTiling);
                    #ifdef _AUTO_BLEND
                        _BlendAlpha = (clamp(sin(_Time.y * _AutoBlendSpeed / _AutoBlendDelay) * (_AutoBlendDelay + 1), -1, 1) + 1) / 2;
                    #endif
                    float blendAlpha = _BlendAlpha * 1.00001 + .499999;
                    blendAlpha = clamp(blendAlpha - blendNoise, 0, 1);
                    col = lerp(col, blendCol, floor(blendAlpha + .5));
                #endif
                
                clip(col.a - _Clip);
                
                #ifdef _OVERLAY_ONE
                    float4 overlayTex1 = tex2D(_OverlayTexture1, TRANSFORM_TEX(float2(i.uv + _Time.y * _Tex1Velocity.xy), _OverlayTexture1));
                    col.rgb = lerp(col.rgb, overlayTex1.rgb * _OverlayColor1.rgb, overlayTex1.a * _OverlayColor1.a);
                #endif
                #ifdef _OVERLAY_TWO
                    float4 overlayTex1 = tex2D(_OverlayTexture1, TRANSFORM_TEX(float2(i.uv + _Time.y * _Tex1Velocity.xy), _OverlayTexture1));
                    float4 overlayTex2 = tex2D(_OverlayTexture2, TRANSFORM_TEX(float2(i.uv + _Time.y * _Tex2Velocity.xy), _OverlayTexture2));
                    col.rgb = lerp(col.rgb, overlayTex1.rgb * _OverlayColor1.rgb, overlayTex1.a * _OverlayColor1.a);
                    col.rgb = lerp(col.rgb, overlayTex2.rgb * _OverlayColor2.rgb, overlayTex2.a * _OverlayColor2.a);
                #endif
                #ifdef _OVERLAY_THREE
                    float4 overlayTex1 = tex2D(_OverlayTexture1, TRANSFORM_TEX(float2(i.uv + _Time.y * _Tex1Velocity.xy), _OverlayTexture1));
                    float4 overlayTex2 = tex2D(_OverlayTexture2, TRANSFORM_TEX(float2(i.uv + _Time.y * _Tex2Velocity.xy), _OverlayTexture2));
                    float4 overlayTex3 = tex2D(_OverlayTexture3, TRANSFORM_TEX(float2(i.uv + _Time.y * _Tex3Velocity.xy), _OverlayTexture3));
                    col.rgb = lerp(col.rgb, overlayTex1.rgb * _OverlayColor1.rgb, overlayTex1.a * _OverlayColor1.a);
                    col.rgb = lerp(col.rgb, overlayTex2.rgb * _OverlayColor2.rgb, overlayTex2.a * _OverlayColor2.a);
                    col.rgb = lerp(col.rgb, overlayTex3.rgb * _OverlayColor3.rgb, overlayTex3.a * _OverlayColor3.a);
                #endif
                
                #if !defined(_RIMGLOW_OFF)
                    float3 rimColor = lerp(col.rgb, tex2D(_RimTex, i.uv * _RimTexTile + (_Time.y * _RimTexPanSpeed.xy)) * _RimColor, _RimColorBias);
                    float3 cameraVertDot = abs(dot(i.cameraToVert, normalDirection));
                    #ifdef _RIMGLOW_SOFT
                        float alpha = max(0, min(1, cameraVertDot * (10 - _RimWidth * 10)));
                        #ifdef _INVERT_RIM
                            alpha = 1 - alpha;
                        #endif
                        col.rgb = lerp(rimColor, col.rgb, alpha);
                    #elif _RIMGLOW_HARD
                        float alpha = step(_RimWidth, cameraVertDot);
                        #ifdef _INVERT_RIM
                            alpha = 1 - alpha;
                        #endif
                        col.rgb = lerp(rimColor, col.rgb, alpha);
                    #endif
                #endif
                
                #ifdef _FAKE_LIGHTING
                    float FakeLight = (dot(normalDirection, normalize(_LightDirection)) + 1) / 2;
                    float4 LightColor = tex2D(_LightingGradient, float2(FakeLight, 0));
                    
                    float3 specular = 0;
                    #ifdef _SPECULAR_HIGHLIGHTS
                        float Pi = 3.141592654;
                        
                        ///////// Gloss:
                        float gloss = _Gloss;
                        float specPow = exp2(gloss * 10.0 + 1.0);
                        ////// Specular:
                        float specMap_var = tex2D(_SpecularMap, TRANSFORM_TEX(i.uv, _SpecularMap));
                        float3 halfDirection = normalize(i.cameraToVert + _LightDirection);
                        float3 specularColor = ((col.a * _SpecularStrength * specMap_var) * _SpecularColor.rgb);
                        float normTerm = (specPow + 10) / (10 * Pi);
                        #ifdef _HARD_SPECULAR
                            float3 directSpecular = pow(max(0, step(1 - dot(halfDirection, normalDirection), _SpecularSize)), specPow) * normTerm * specularColor;
                        #else
                            float3 directSpecular = pow(max(0, dot(halfDirection, normalDirection)), specPow) * normTerm * specularColor;
                        #endif
                        
                        specular = directSpecular;
                    #endif
                    
                    col *= saturate(LightColor + (1 - _ShadowStrength));
                    col.rgb += specular;
                #endif
                
                #ifdef _LIT
                    float attenuation = LIGHT_ATTENUATION(i) / SHADOW_ATTENUATION(i);
                    float3 FlatLighting = saturate((LightingFunction(float3(0, 1, 0)) + (_LightColor0.rgb * attenuation)));
                    col.rgb *= FlatLighting;
                #endif
                
                #if !defined(_RIMGLOW_OFF)
                    col.rgb += lerp(rimColor * _RimGlowStrength, 0, alpha);
                #endif
                
                #ifdef _EMISSION
                    float4 _Emissive_Tex_var = tex2D(_EmissionMap, TRANSFORM_TEX(i.uv, _EmissionMap)) * _EmissionColor * _EmissionStrength;
                    
                    #ifdef _SCROLLING_EMISSION
                        _Emissive_Tex_var *= i.phase;
                    #endif
                    
                    #ifdef _BLINKING_EMISSION
                        float amplitude = (_EmissiveBlink_Max - _EmissiveBlink_Min) * 0.5f;
                        float base = _EmissiveBlink_Min + amplitude;
                        float emissiveBlink = sin(_Time.y * _EmissiveBlink_Velocity) * amplitude + base;
                        _Emissive_Tex_var *= emissiveBlink;
                    #endif
                    
                    col.rgb += _Emissive_Tex_var;
                #endif
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
            #define _GLOSSYENV 1
            #pragma shader_feature _RGB_MASK
            #pragma shader_feature _EMISSION
            #pragma shader_feature _FAKE_LIGHTING
            #pragma shader_feature _SPECULAR_HIGHLIGHTS
            #pragma multi_compile _BLEND_OFF _BLEND_HARD _BLEND_SOFT
            #pragma shader_feature _AUTO_BLEND
            #pragma multi_compile _RIMGLOW_OFF _RIMGLOW_HARD _RIMGLOW_SOFT
            #pragma shader_feature _INVERT_RIM
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
                SHADOW_COORDS(9)
            };
            
            //Propertiesv
            float4 _Color;
            sampler2D _MainTex; float4 _MainTex_ST;
            sampler2D _NormalMap; float4 _NormalMap_ST;
            float _NormalIntensity;
            sampler2D _SpecularMap; float4 _SpecularMap_ST;
            float _Gloss;
            float4 _EmissionColor;
            sampler2D _EmissionMap; float4 _EmissionMap_ST;
            float _EmissionStrength;
            
            sampler2D _RGBMask; float4 _RGBMask_ST;
            sampler2D _RedTexture; float4 _RedTexture_ST;
            sampler2D _GreenTexture; float4 _GreenTexture_ST;
            sampler2D _BlueTexture; float4 _BlueTexture_ST;
            
            float4 _BlendTextureColor;
            sampler2D _BlendTexture;
            sampler2D _BlendNoiseTexture;
            float _BlendAlpha;
            float _BlendTiling;
            float _AutoBlendSpeed;
            float _AutoBlendDelay;
            
            sampler2D _LightingGradient;
            float _ShadowStrength;
            float4 _LightDirection;
            
            float _RimTexTile;
            float4 _RimColor;
            float _RimGlowStrength;
            float _RimWidth;
            float _RimColorBias;
            float4 _RimTexPanSpeed;
            sampler2D _RimTex;
            
            sampler2D _OverlayTexture1; float4 _OverlayTexture1_ST;
            float4 _Tex1Velocity;
            sampler2D _OverlayTexture2; float4 _OverlayTexture2_ST;
            float4 _Tex2Velocity;
            sampler2D _OverlayTexture3; float4 _OverlayTexture3_ST;
            float4 _Tex3Velocity;
            
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
            
            float map(float s, float a1, float a2, float b1, float b2)
            {
                return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
            }
            
            //shader programs
            v2f vert(appdata v)
            {
                v2f o;
                TANGENT_SPACE_ROTATION;
                
                o.pos = UnityObjectToClipPos(v.vertex);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.cameraToVert = normalize(getCameraPosition() - mul(unity_ObjectToWorld, v.vertex));
                o.normalDir = normalize(mul(unity_ObjectToWorld, v.normal));
                o.uv = float4(v.texcoord.xy, 0, 0);
                o.tangentDir = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                TRANSFER_SHADOW(o);
                return o;
            }
            
            fixed4 frag(v2f i, float facing: VFACE): SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb *= _Color.rgb;
                
                #ifdef _RGB_MASK
                    fixed4 rgbMask_var = tex2D(_RGBMask, TRANSFORM_TEX(i.uv, _RGBMask));
                    fixed4 redTexture_var = tex2D(_RedTexture, TRANSFORM_TEX(i.uv, _RedTexture));
                    fixed4 greenTexture_var = tex2D(_GreenTexture, TRANSFORM_TEX(i.uv, _GreenTexture));
                    fixed4 blueTexture_var = tex2D(_BlueTexture, TRANSFORM_TEX(i.uv, _BlueTexture));
                    
                    col = lerp(lerp(lerp(col, blueTexture_var, rgbMask_var.b), greenTexture_var, rgbMask_var.g), redTexture_var, rgbMask_var.r);
                #endif
                
                float facing_ing = dot(i.normalDir, i.cameraToVert);
                float isFrontFace = (facing_ing >= 0 ? 1: 0);
                float faceSign = (facing_ing >= 0 ? 1: - 1);
                i.normalDir *= faceSign;
                
                float3x3 tangentTransform = float3x3(i.tangentDir, i.bitangentDir, i.normalDir);
                float3 _Normal_var = UnpackNormal(tex2D(_NormalMap, TRANSFORM_TEX(i.uv, _NormalMap)));
                float3 normalLocal = lerp(float3(0, 0, 1), _Normal_var.rgb, _NormalIntensity);
                float3 normalDirection = normalize(mul(normalLocal, tangentTransform)); // Perturbed normals
                
                #ifdef _BLEND_SOFT
                    fixed4 blendCol = tex2D(_BlendTexture, i.uv) * _BlendTextureColor;
                    fixed blendNoise = tex2D(_BlendNoiseTexture, i.uv * _BlendTiling);
                    #ifdef _AUTO_BLEND
                        _BlendAlpha = (sin(_Time.y * _AutoBlendSpeed) + 1) / 2;
                    #endif
                    float blendAlpha = _BlendAlpha * 3 - 1;
                    blendAlpha = clamp(blendAlpha - blendNoise, 0, 1);
                    col = lerp(col, blendCol, blendAlpha);
                #elif _BLEND_HARD
                    fixed4 blendCol = tex2D(_BlendTexture, i.uv) * _BlendTextureColor;
                    fixed blendNoise = tex2D(_BlendNoiseTexture, i.uv * _BlendTiling);
                    #ifdef _AUTO_BLEND
                        _BlendAlpha = (clamp(sin(_Time.y * _AutoBlendSpeed / _AutoBlendDelay) * (_AutoBlendDelay + 1), -1, 1) + 1) / 2;
                    #endif
                    float blendAlpha = _BlendAlpha * 1.00001 + .499999;
                    blendAlpha = clamp(blendAlpha - blendNoise, 0, 1);
                    col = lerp(col, blendCol, floor(blendAlpha + .5));
                #endif
                
                clip(col.a - _Clip);
                
                #ifdef _OVERLAY_ONE
                    float4 overlayTex1 = tex2D(_OverlayTexture1, TRANSFORM_TEX(float2(i.uv + _Time.y * _Tex1Velocity.xy), _OverlayTexture1));
                    col.rgb = lerp(col.rgb, overlayTex1.rgb, overlayTex1.a);
                #endif
                #ifdef _OVERLAY_TWO
                    float4 overlayTex1 = tex2D(_OverlayTexture1, TRANSFORM_TEX(float2(i.uv + _Time.y * _Tex1Velocity.xy), _OverlayTexture1));
                    float4 overlayTex2 = tex2D(_OverlayTexture2, TRANSFORM_TEX(float2(i.uv + _Time.y * _Tex2Velocity.xy), _OverlayTexture2));
                    col.rgb = lerp(col.rgb, overlayTex1.rgb, overlayTex1.a);
                    col.rgb = lerp(col.rgb, overlayTex2.rgb, overlayTex2.a);
                #endif
                #ifdef _OVERLAY_THREE
                    float4 overlayTex1 = tex2D(_OverlayTexture1, TRANSFORM_TEX(float2(i.uv + _Time.y * _Tex1Velocity.xy), _OverlayTexture1));
                    float4 overlayTex2 = tex2D(_OverlayTexture2, TRANSFORM_TEX(float2(i.uv + _Time.y * _Tex2Velocity.xy), _OverlayTexture2));
                    float4 overlayTex3 = tex2D(_OverlayTexture3, TRANSFORM_TEX(float2(i.uv + _Time.y * _Tex3Velocity.xy), _OverlayTexture3));
                    col.rgb = lerp(col.rgb, overlayTex1.rgb, overlayTex1.a);
                    col.rgb = lerp(col.rgb, overlayTex2.rgb, overlayTex2.a);
                    col.rgb = lerp(col.rgb, overlayTex3.rgb, overlayTex3.a);
                #endif
                
                #if !defined(_RIMGLOW_OFF)
                    float3 rimColor = lerp(col.rgb, tex2D(_RimTex, i.uv * _RimTexTile + (_Time.y * _RimTexPanSpeed.xy)) * _RimColor, _RimColorBias);
                    float3 cameraVertDot = abs(dot(i.cameraToVert, normalDirection));
                    #ifdef _RIMGLOW_SOFT
                        float alpha = max(0, min(1, cameraVertDot * (10 - _RimWidth * 10)));
                        #ifdef _AUTO_BLEND
                            _BlendAlpha = (sin(_Time.y * _AutoBlendSpeed) + 1) / 2;
                        #endif
                        col.rgb = lerp(rimColor, col.rgb, alpha);
                    #elif _RIMGLOW_HARD
                        float alpha = step(_RimWidth, cameraVertDot);
                        #ifdef _AUTO_BLEND
                            _BlendAlpha = (sin(_Time.y * _AutoBlendSpeed) + 1) / 2;
                        #endif
                        col.rgb = lerp(rimColor, col.rgb, alpha);
                    #endif
                #endif
                
                
                #ifdef _FAKE_LIGHTING
                    float FakeLight = (dot(normalDirection, normalize(_LightDirection)) + 1) / 2;
                    float4 LightColor = tex2D(_LightingGradient, float2(FakeLight, 0));
                    col *= clamp(LightColor + (1 - _ShadowStrength), 0, 1);
                #endif
                
                
                UNITY_LIGHT_ATTENUATION(atten, i, i.posWorld.xyz)
                fixed4 c = atten;
                // might want to take light color into account?
                c.rgb *= _LightColor0.rgb;
                
                return col * c;
            }
            ENDCG
            
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            Offset 1, 1
            Cull Off
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #define _GLOSSYENV 1
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "UnityPBSLighting.cginc"
            #include "UnityStandardBRDF.cginc"
            #include "AutoLight.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma only_renderers d3d9 d3d11 glcore gles
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            struct VertexInput
            {
                float4 vertex: POSITION;
                float2 texcoord0: TEXCOORD0;
            };
            struct VertexOutput
            {
                V2F_SHADOW_CASTER;
                float2 uv0: TEXCOORD1;
            };
            VertexOutput vert(VertexInput v)
            {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex);
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i, float facing: VFACE): COLOR
            {
                float isFrontFace = (facing >= 0 ? 1: 0);
                float faceSign = (facing >= 0 ? 1: - 1);
                float4 _MainTex_var = tex2D(_MainTex, TRANSFORM_TEX(i.uv0, _MainTex));
                float SurfaceAlpha = _MainTex_var.a;
                clip(SurfaceAlpha - 0.5);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
            
        }
    }
}
