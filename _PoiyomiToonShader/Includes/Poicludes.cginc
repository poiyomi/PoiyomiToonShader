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
        UNITY_FOG_COORDS(7)
    };
    
    struct PoiLighting
    {
        float3 direction;
        float3 position;
        float3 color;
        float attenuation;
        float3 directLighting;
        float3 indirectLighting;
        float lightMap;
        float3 rampedLightMap;
        float3 finalLighting;
    };
    
    static PoiLighting poiLight;
    #define pi float(3.14159265359)
#endif
