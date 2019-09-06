#ifndef POICLUDES
    #define POICLUDES
    
    #include "UnityCG.cginc"
    #include "Lighting.cginc"
    #include "UnityPBSLighting.cginc"
    #include "AutoLight.cginc"
    
    UNITY_DECLARE_TEX2D(_MainTex); float4 _MainTex_ST;
    
    
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
        float3 modelPos: TEXCOORD8;
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
        UNITY_SHADOW_COORDS(9)
        UNITY_FOG_COORDS(10)
    };
    
    struct PoiLighting
    {
        half3 direction;
        half3 color;
        half attenuation;
        half3 directLighting;
        half3 indirectLighting;
        half lightMap;
        half3 rampedLightMap;
        half3 finalLighting;
        half3 halfDir;
        half nDotL;
        half nDotH;
        half lDotv;
        half lDotH;
        half nDotV;
        half diffuseTerm;
    };
    
    struct PoiCamera
    {
        half3 viewDir;
        half3 tangentViewDir;
        half3 forwardDir;
        half3 worldPos;
        float viewDotNormal;
        float distanceToModel;
    };
    
    struct PoiMesh
    {
        float3 vertexNormal;
        float3 fragmentNormal;
        float3 tangent;
        float3 bitangent;
        float3 localPos;
        float3 worldPos;
        float3 modelPos;
        float3 tangentSpaceNormal;
        float2 uv;
    };
    
    struct PoiTangentData
    {
        float3x3 tangentTransform;
        float3x3 tangentToWorld;
    };
    
    struct FragmentCommonData
    {
        half3 diffColor, specColor;
        // Note: smoothness & oneMinusReflectivity for optimization purposes, mostly for DX9 SM2.0 level.
        // Most of the math is being done on these (1-x) values, and that saves a few precious ALU slots.
        half oneMinusReflectivity, smoothness;
        float3 normalWorld;
        float3 eyeVec;
        half alpha;
        float3 posWorld;
        
        #if UNITY_STANDARD_SIMPLE
            half3 reflUVW;
        #endif
        
        #if UNITY_STANDARD_SIMPLE
            half3 tangentSpaceNormal;
        #endif
    };
    
    static PoiLighting poiLight;
    static PoiCamera poiCam;
    static PoiMesh poiMesh;
    static UnityGI gi;
    static FragmentCommonData s;
    static PoiTangentData poiTData;
    float4 finalColor;
    float4 mainTexture;
    
    #define pi float(3.14159265359)
    
#endif
