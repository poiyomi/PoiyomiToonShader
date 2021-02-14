#ifndef POICLUDES
    #define POICLUDES
    
    UNITY_DECLARE_TEX2D(_MainTex); float4 _MainTex_ST; float4 _MainTex_TexelSize;
    sampler2D _PoiGrab;
    float _Clip;
    float _AlphaMod;
    
    //Structs
    struct appdata
    {
        float4 vertex: POSITION;
        float3 normal: NORMAL;
        float4 tangent: TANGENT;
        float4 color: COLOR;
        float2 uv0: TEXCOORD0;
        float2 uv1: TEXCOORD1;
        float2 uv2: TEXCOORD2;
        float2 uv3: TEXCOORD3;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };
    
    
    
    #ifdef OUTLINE
        float _LineWidth;
        float _OutlineEmission;
        float4 _LineColor;
        sampler2D _OutlineTexture; float4 _OutlineTexture_ST;
        float4 _OutlineTexturePan;
        float4 _OutlineFadeDistance;
        float4 _OutlineGlobalPan;
    #endif
    
    struct PoiLighting
    {
        half3 direction;
        half3 color;
        fixed attenuation;
        #if defined(POINT) || defined(SPOT)
            fixed additiveShadow;
        #endif
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
        half N0DotV;
        half diffuseTerm;
        
        // World normal dot half direction
        #ifdef POI_VAR_DOTNH
            half dotNH;
        #endif
        
        // Light direction dot half direction
        #ifdef POI_VAR_DOTLH
            half dotLH;
        #endif
        
        #ifdef VERTEXLIGHT_ON
            // Non Important Lights
            float4 vDotNL;
            float3 vColor[4];
            float4 vCorrectedDotNL;
            float4 vAttenuation;
            float4 vAttenuationDotNL;
            float3 vPosition[4];
            float3 vDirection[4];
            float3 vFinalLighting;
            float3 vHalfDir[4];
            
            #ifdef POI_VAR_DOTLH
                half4 vDotLH;
            #endif
            
            #ifdef POI_VAR_DOTNH
                half4 vDotNH;
            #endif
            
        #endif
    };
    
    struct PoiCamera
    {
        half3 viewDir;
        half3 tangentViewDir;
        half3 forwardDir;
        half3 worldPos;
        float viewDotNormal;
        float distanceToModel;
        float distanceToVert;
        float3 reflectionDir;
        float3 vertexReflectionDir;
        float2 screenUV;
        float4 clipPos;
        #if defined(GRAIN)
            float4 screenPos;
        #endif
        float4 grabPos;
    };
    
    struct PoiMesh
    {
        float3 normals[2];
        float3 tangent;
        float3 binormal;
        float3 localPos;
        float3 worldPos;
        float3 modelPos;
        float3 tangentSpaceNormal;
        float2 uv[5];
        float4 vertexColor;
        fixed3 barycentricCoordinates;
        #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
            float4 lightmapUV;
        #endif
        float isFrontFace;
        #ifdef FUR
            float furAlpha;
        #endif
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
    
    struct Debug
    {
        float debug1;
        float2 debug2;
        float3 debug3;
        float4 debug4;
    };
    
    static Debug debug;
    static PoiLighting poiLight;
    static PoiCamera poiCam;
    static PoiMesh poiMesh;
    static UnityGI gi;
    static FragmentCommonData s;
    static PoiTangentData poiTData;
    float4 finalColor;
    float3 finalEmission;
    float4 mainTexture;
    float4 albedo;
    
#endif
