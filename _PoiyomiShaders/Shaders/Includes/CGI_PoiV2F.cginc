#ifndef POI_V2F
    #define  POI_V2F
    
    struct v2f
    {
        float4 pos: SV_POSITION;
        float4 uv0: TEXCOORD0;
        float4 uv1: TEXCOORD1;
        float3 normal: TEXCOORD2;
        float3 tangentViewDir: TEXCOORD3;
        float4 tangent: TEXCOORD4;
        float4 worldPos: TEXCOORD5;
        float4 localPos: TEXCOORD6;
        float4 grabPos: TEXCOORD7;
        #if defined(GRAIN)
            float4 screenPos: TEXCOORD8;
        #endif
        #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
            float4 lightmapUV: TEXCOORD9;
        #endif
        float3 modelPos: TEXCOORD10;
        float angleAlpha: TEXCOORD11;
        #if defined(VERTEXLIGHT_ON)
            float3 vertexLightColor: TEXCOORD12;
        #endif
        float4 vertexColor: TEXCOORD13;
        UNITY_SHADOW_COORDS(14)
        UNITY_FOG_COORDS(15)
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
    };
    
#endif