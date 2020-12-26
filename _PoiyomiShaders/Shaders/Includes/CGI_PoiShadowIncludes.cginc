#ifndef SHADOW_INCLUDES
    #define SHADOW_INCLUDES
    
    #define UNITY_STANDARD_USE_SHADOW_UVS 1
    
    float4 _Color;
    float _Clip;
    UNITY_DECLARE_TEX2D(_MainTex); float4 _MainTex_ST;
    sampler2D _AlphaMask; float4 _AlphaMask_ST;
    
    struct VertexInputShadow
    {
        float4 vertex: POSITION;
        float3 normal: NORMAL;
        float2 uv0: TEXCOORD0;
        float2 uv1: TEXCOORD1;
        float2 uv2: TEXCOORD2;
        float2 uv3: TEXCOORD3;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };
    
    #if !defined(V2F_SHADOW_CASTER_NOPOS_IS_EMPTY) || defined(UNITY_STANDARD_USE_SHADOW_UVS)
        struct V2FShadow
        {
            V2F_SHADOW_CASTER_NOPOS
            float4 pos: SV_POSITION;
            float2 uv: TEXCOORD1;
            float2 uv1: TEXCOORD2;
            float2 uv2: TEXCOORD3;
            float2 uv3: TEXCOORD4;
            float3 modelPos: TEXCOORD5;
            float4 worldPos: TEXCOORD6;
            float4 localPos: TEXCOORD7;
            float3 angleAlpha: TEXCOORD8;
            float4 grabPos: TEXCOORD9;
            fixed3 barycentricCoordinates: TEXCOORD10;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };
    #endif
    
#endif