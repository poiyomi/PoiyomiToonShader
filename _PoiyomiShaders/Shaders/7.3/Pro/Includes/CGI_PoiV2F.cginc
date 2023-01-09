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
        float4 binormal: TEXCOORD5;
        float4 worldPos: TEXCOORD6;
        float4 localPos: TEXCOORD7;
        float4 grabPos: TEXCOORD8;
        float3 barycentricCoordinates: TEXCOORD9;
        #if defined(GRAIN)
            float4 worldDirection: TEXCOORD10;
        #endif
        #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
            float4 lightmapUV: TEXCOORD11;
        #endif
        float3 modelPos: TEXCOORD12;
        float angleAlpha: TEXCOORD13;
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