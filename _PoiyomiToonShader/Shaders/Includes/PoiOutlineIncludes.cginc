
#ifndef POICLUDES
    #define POICLUDES

    #include "PoiData.cginc"
    #include "UnityCG.cginc"
    #include "Lighting.cginc"
    #include "UnityPBSLighting.cginc"
    #include "AutoLight.cginc"
    
    UNITY_DECLARE_TEX2D(_MainTex); float4 _MainTex_ST;
    float _LineWidth;
    float _OutlineEmission;
    float4 _LineColor;
    float4 _Color;
    float _Clip;
    sampler2D _OutlineTexture; float4 _OutlineTexture_ST;
    sampler2D _AlphaMask; float4 _AlphaMask_ST;
    float4 _OutlineTexturePan;
    
    float4 _OutlineFadeDistance;
    float4 _OutlineGlobalPan;
    
    struct VertexInput
    {
        float4 vertex: POSITION;
        float3 normal: NORMAL;
        float2 texcoord0: TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };
    struct v2f
    {
        float4 pos: SV_POSITION;
        float2 uv: TEXCOORD0;
        float3 normal: TEXCOORD1;
        float3 worldPos: TEXCOORD2;
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
        UNITY_SHADOW_COORDS(3)
        UNITY_FOG_COORDS(4)
    };
    
    static PoiLighting poiLight;
    float pi;
    
#endif