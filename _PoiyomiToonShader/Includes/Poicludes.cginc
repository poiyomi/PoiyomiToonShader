#ifndef POICLUDES
    #define POICLUDES
    #include "UnityCG.cginc"
    #include "Lighting.cginc"
    #include "UnityPBSLighting.cginc"
    #include "AutoLight.cginc"
    
    //Structs
    struct appdata
    {
        float4 vertex: POSITION;
        float3 normal: NORMAL;
        float4 tangent: TANGENT;
        float2 texcoord: TEXCOORD0;
        float2 texcoord1: TEXCOORD1;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };
    
    struct v2f
    {
        float2 uv: TEXCOORD0;
        float3 normal: TEXCOORD1;
        #if defined(BINORMAL_PER_FRAGMENT)
            float4 tangent: TEXCOORD2;
        #else
            float3 tangent: TEXCOORD2;
            float3 binormal: TEXCOORD3;
        #endif
        float4 pos: SV_POSITION;
        float4 worldPos: TEXCOORD4;
        float4 localPos: TEXCOORD5;
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
        UNITY_SHADOW_COORDS(6)
    };
    
    //Properties
    float4 _Color;
    float _Desaturation;
    UNITY_DECLARE_TEX2D(_MainTex); float4 _MainTex_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_BumpMap); float4 _BumpMap_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_AOMap); float4 _AOMap_ST;
    float _AOStrength;
    float4 _GlobalPanSpeed;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailNormalMap); float4 _DetailNormalMap_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailNormalMask); float4 _DetailNormalMask_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_AlphaMask); float4 _AlphaMask_ST;
    float _BumpScale;
    float _DetailNormalMapScale;
    
    samplerCUBE _CubeMap;
    float _SampleWorld;
    float _PurelyAdditive;
    sampler2D _MetallicMap; float4 _MetallicMap_ST;
    float _Metallic;
    sampler2D _SmoothnessMap; float4 _SmoothnessMap_ST;
    float _InvertSmoothness;
    float _Smoothness;
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_Matcap);
    UNITY_DECLARE_TEX2D_NOSAMPLER(_MatcapMap); float4 _MatcapMap_ST;
    float4 _MatcapColor;
    float  _MatcapStrength;
    float _ReplaceWithMatcap;
    float _MultiplyMatcap;
    float _AddMatcap;
    
    #ifdef TEXTURE_BLENDING
        int _Blend;
        float4 _BlendTextureColor;
        sampler2D _BlendTexture; float4 _BlendTexture_ST;
        sampler2D _BlendNoiseTexture; float4 _BlendNoiseTexture_ST;
        float _BlendAlpha;
        float _BlendTiling;
        float _AutoBlend;
        float _AutoBlendSpeed;
        float _AutoBlendDelay;
    #endif
    
    float _Gloss;
    float4 _EmissionColor;
    sampler2D _EmissionMap; float4 _EmissionMap_ST;
    sampler2D _EmissionMask; float4 _EmissionMask_ST;
    float _EmissionStrength;
    
    float4 _EmissiveScroll_Direction;
    float4 _EmissionScrollSpeed;
    float _EmissiveScroll_Width;
    float _EmissiveScroll_Velocity;
    float _EmissiveScroll_Interval;
    float _EmissiveBlink_Min;
    float _EmissiveBlink_Max;
    float _EmissiveBlink_Velocity;
    float _ScrollingEmission;
    
    sampler2D _ToonRamp;
    sampler2D _AdditiveRamp;
    float _ForceLightDirection;
    float _ShadowStrength;
    float _ShadowOffset;
    float3 _LightDirection;
    float _ForceShadowStrength;
    float _CastedShadowSmoothing;
    float _MinBrightness;
    float _MaxBrightness;
    float _IndirectContribution;
    float _AttenuationMultiplier;
    
    sampler2D _SpecularMap; float4 _SpecularMap_ST;
    float4 _SpecularColor;
    float _SpecularBias;
    float _SpecularStrength;
    float _SpecularSize;
    float _HardSpecular;
    
    #ifdef PANOSPHERE
        sampler2D _PanosphereTexture; float4 _PanosphereTexture_ST;
        sampler2D _PanoMapTexture; float4 _PanoMapTexture_ST;
        float _PanoEmission;
        float _PanoBlend;
        float4 _PanosphereColor;
        float4 _PanosphereScroll;
    #endif
    
    float4 _RimLightColor;
    float _RimWidth;
    float _RimStrength;
    float _RimSharpness;
    float _RimLightColorBias;
    float4 _RimTexPanSpeed;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_RimTex); float4 _RimTex_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_RimMask); float4 _RimMask_ST;
    float _Clip;
#endif