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
        float3 barycentricCoordinates: TEXCOORD8;
        #if defined(GRAIN)
            float4 screenPos: TEXCOORD9;
        #endif
        #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
            float4 lightmapUV: TEXCOORD10;
        #endif
        float3 modelPos: TEXCOORD11;
        float angleAlpha: TEXCOORD12;
        float4 vertexColor: TEXCOORD14;
        #ifdef FUR
            float furAlpha: TEXCOORD15;
        #endif
        UNITY_SHADOW_COORDS(16)
        UNITY_FOG_COORDS(17)
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
    };
    
#endif