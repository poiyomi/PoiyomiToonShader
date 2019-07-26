#ifndef POI_DATA
    #define POI_DATA
    
    UNITY_DECLARE_TEX2D(_MainTex); float4 _MainTex_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_BumpMap); float4 _BumpMap_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailNormalMap); float4 _DetailNormalMap_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailNormalMask); float4 _DetailNormalMask_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_AlphaMask); float4 _AlphaMask_ST;
    float4 _Color;
    float _Saturation;
    float4 _GlobalPanSpeed;
    float _BumpScale;
    float _DetailNormalMapScale;
    float _Clip;
    
    //globals
    float4 mainTexture;
    float alphaMask;
    float4 albedo;
    float3x3 tangentMatrix;
    
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
    };
    
    struct PoiMesh
    {
        float3 vertexNormal;
        float3 fragmentNormal;
        float3 tangent;
        float3 bitangent;
        float3 worldPos;
        float3 tangentSpaceNormal;
        float2 uv;
    };
    
    struct PoiTangentData
    {
        float3x3 tangentTransform;
        float3x3 tangentToWorld;
    };
    
    static PoiLighting poiLight;
    static PoiCamera poiCam;
    static PoiMesh poiMesh;
    static UnityGI gi;
    static FragmentCommonData s;
    static PoiTangentData poiTData;
    
    float FadeShadows(float attenuation, float3 worldPosition)
    {
        float viewZ = dot(_WorldSpaceCameraPos - worldPosition, UNITY_MATRIX_V[2].xyz);
        float shadowFadeDistance = UnityComputeShadowFadeDistance(worldPosition, viewZ);
        float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
        attenuation = saturate(attenuation + shadowFade);
        return attenuation;
    }
    
    void calculateAttenuation(v2f i)
    {
        UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos.xyz)
        poiLight.attenuation = FadeShadows(attenuation, i.worldPos.xyz);
    }
    
    void calculateLightDirection(v2f i)
    {
        #ifdef FORWARD_BASE_PASS
            poiLight.direction = normalize(_WorldSpaceLightPos0 + unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz);
        #else
            #if defined(POINT) || defined(SPOT)
                poiLight.direction = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
            #endif
        #endif
    }
    
    void calculateLightColor()
    {
        #ifdef FORWARD_BASE_PASS
            poiLight.color = _LightColor0.rgb + saturate(ShadeSH9(normalize(unity_SHAr + unity_SHAg + unity_SHAb)));
        #else
            #if defined(POINT) || defined(SPOT)
                poiLight.color = _LightColor0.rgb;
            #endif
        #endif
    }
    
    float3 getCameraForward()
    {
        #if UNITY_SINGLE_PASS_STEREO
            float3 p1 = mul(unity_StereoCameraToWorld[0], float4(0, 0, 1, 1));
            float3 p2 = mul(unity_StereoCameraToWorld[0], float4(0, 0, 0, 1));
        #else
            float3 p1 = mul(unity_CameraToWorld, float4(0, 0, 1, 1));
            float3 p2 = mul(unity_CameraToWorld, float4(0, 0, 0, 1));
        #endif
        return normalize(p2 - p1);
    }
    
    float3 CreateBinormal(float3 normal, float3 tangent, float binormalSign)
    {
        return cross(normal, tangent.xyz) * (binormalSign * unity_WorldTransformParams.w);
    }
    
    void InitializeMeshData(inout v2f i)
    {
        mainTexture = UNITY_SAMPLE_TEX2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex));
        poiMesh.vertexNormal = i.normal;
        
        float3 mainNormal = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap, _MainTex, TRANSFORM_TEX(i.uv, _BumpMap)), _BumpScale);
        float detailNormalMask = UNITY_SAMPLE_TEX2D_SAMPLER(_DetailNormalMask, _MainTex, TRANSFORM_TEX(i.uv, _DetailNormalMask));
        float3 detailNormal = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_DetailNormalMap, _MainTex, TRANSFORM_TEX(i.uv, _DetailNormalMap)), _DetailNormalMapScale * detailNormalMask);
        poiMesh.tangentSpaceNormal = BlendNormals(mainNormal, detailNormal);
        
        i.normal = normalize(
            poiMesh.tangentSpaceNormal.x * i.tangent +
            poiMesh.tangentSpaceNormal.y * i.bitangent +
            poiMesh.tangentSpaceNormal.z * i.normal
        );
        
        poiMesh.fragmentNormal = i.normal;
        poiMesh.bitangent = i.bitangent;
        poiMesh.tangent = i.tangent;
        poiMesh.worldPos = i.worldPos;
        poiMesh.uv = i.uv;
    }
    
    void initializeCamera(v2f i)
    {
        poiCam.viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
        poiCam.forwardDir = getCameraForward();
        poiCam.worldPos = _WorldSpaceCameraPos;
    }
    
    inline FragmentCommonData SpecularSetup(float4 i_tex)
    {
        half4 specGloss = 0;
        half3 specColor = specGloss.rgb;
        half smoothness = specGloss.a;
        
        half oneMinusReflectivity;
        half3 diffColor = EnergyConservationBetweenDiffuseAndSpecular(albedo, specColor, /*out*/ oneMinusReflectivity);
        
        FragmentCommonData o = (FragmentCommonData)0;
        o.diffColor = diffColor;
        o.specColor = specColor;
        o.oneMinusReflectivity = oneMinusReflectivity;
        o.smoothness = smoothness;
        return o;
    }
    
    inline FragmentCommonData FragmentSetup(float4 i_tex, half3 i_viewDirForParallax, float3 i_posWorld)
    {
        i_tex = i_tex;
        
        FragmentCommonData o = SpecularSetup(i_tex);
        o.normalWorld = float4(0, 0, 0, 1);
        o.eyeVec = poiCam.viewDir;
        o.posWorld = i_posWorld;
        
        // NOTE: shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
        o.diffColor = PreMultiplyAlpha(o.diffColor, 1, o.oneMinusReflectivity, /*out*/ o.alpha);
        return o;
    }
    
    void calculateTangentData()
    {
        poiTData.tangentTransform = float3x3(poiMesh.tangent, poiMesh.bitangent, poiMesh.vertexNormal);
        poiTData.tangentToWorld = transpose(float3x3(poiMesh.tangent, poiMesh.bitangent, poiMesh.vertexNormal));
    }
    
    void calculateData(inout v2f i)
    {
        UNITY_SETUP_INSTANCE_ID(i);
        mainTexture = UNITY_SAMPLE_TEX2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex));
        alphaMask = UNITY_SAMPLE_TEX2D_SAMPLER(_AlphaMask, _MainTex, TRANSFORM_TEX(i.uv, _AlphaMask));
        albedo = float4(lerp(mainTexture.rgb, dot(mainTexture.rgb, float3(0.3, 0.59, 0.11)), -_Saturation) * _Color.rgb, mainTexture.a * _Color.a * alphaMask);
        calculateAttenuation(i);
        calculateLightColor();
        calculateLightDirection(i);
        
        InitializeMeshData(i);
        initializeCamera(i);
        calculateTangentData();
        
        s = FragmentSetup(float4(i.uv, 1, 1), poiCam.viewDir, poiMesh.worldPos);
        
        poiLight.halfDir = Unity_SafeNormalize(poiLight.direction + poiCam.viewDir);
        
        poiLight.nDotV = dot(poiMesh.fragmentNormal, poiCam.viewDir);
        poiLight.nDotL = dot(poiMesh.fragmentNormal, poiLight.direction);
        poiLight.nDotH = dot(poiMesh.fragmentNormal, poiLight.halfDir);
        poiLight.lDotv = dot(poiLight.direction, poiCam.viewDir);
        poiLight.lDotH = dot(poiLight.direction, poiLight.halfDir);
        
        
        
        poiCam.viewDotNormal = abs(dot(poiCam.viewDir, poiMesh.fragmentNormal));
    }
    
#endif