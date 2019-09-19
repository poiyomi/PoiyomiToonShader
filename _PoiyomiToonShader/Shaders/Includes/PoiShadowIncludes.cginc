#ifndef SHADOW_INCLUDES
    #define SHADOW_INCLUDES
    
    #define UNITY_STANDARD_USE_SHADOW_UVS 1
    
    float4      _Color;
    float       _Clip;
    UNITY_DECLARE_TEX2D(_MainTex); float4 _MainTex_ST;
    float4 		_GlobalPanSpeed;
    sampler2D _AlphaMask; float4 _AlphaMask_ST;
    
    struct VertexInput
    {
        float4 vertex: POSITION;
        float3 normal: NORMAL;
        float2 uv0: TEXCOORD0;
    };
    
    #if !defined(V2F_SHADOW_CASTER_NOPOS_IS_EMPTY) || defined(UNITY_STANDARD_USE_SHADOW_UVS)
        struct VertexOutputShadowCaster
        {
            V2F_SHADOW_CASTER_NOPOS
            #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
                float2 uv: TEXCOORD1;
            #endif
            float3 modelPos: TEXCOORD2;
            float3 worldPos: TEXCOORD3;
            float3 localPos: TEXCOORD4;
            float3 angleAlpha: TEXCOORD5;
        };
    #endif
    
#endif