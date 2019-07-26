#ifndef POICLUDES
    #define POICLUDES
    
    #include "UnityCG.cginc"
    #include "Lighting.cginc"
    #include "UnityPBSLighting.cginc"
    #include "AutoLight.cginc"
    
    #ifdef OUTLINE
        float _LineWidth;
        float _OutlineEmission;
        float4 _LineColor;
        sampler2D _OutlineTexture; float4 _OutlineTexture_ST;
        float4 _OutlineTexturePan;
        
        float4 _OutlineFadeDistance;
        float4 _OutlineGlobalPan;
    #endif
    
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
        float4 pos: SV_POSITION;
        
        float2 uv: TEXCOORD0;
        float3 normal: TEXCOORD1;
        float3 tangent: TEXCOORD2;
        float3 bitangent: TEXCOORD3;
        float4 worldPos: TEXCOORD4;
        float4 localPos: TEXCOORD5;
        float4 screenPos: TEXCOORD6;
        float3 tangentViewDir: TEXCOORD7;
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
        UNITY_SHADOW_COORDS(8)
        UNITY_FOG_COORDS(9)
    };
    
    #define pi float(3.14159265359)
    
#endif
