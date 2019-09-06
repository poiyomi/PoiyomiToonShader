#ifndef POI_MAINTEXTURE
    #define POI_MAINTEXTURE
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_BumpMap); float4 _BumpMap_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailNormalMap); float4 _DetailNormalMap_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_DetailNormalMask); float4 _DetailNormalMask_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_AlphaMask); float4 _AlphaMask_ST;
    float4 _Color;
    float _Saturation;
    float _BumpScale;
    float _DetailNormalMapScale;
    float2 _MainNormalPan;
    float2 _MainDetailNormalPan;
    float2 _MainDistanceFade;
    
    //globals
    float4 albedo;
    float alphaMask;
    half3 diffColor;
    
    inline FragmentCommonData SpecularSetup(float4 i_tex)
    {
        half4 specGloss = 0;
        half3 specColor = specGloss.rgb;
        half smoothness = specGloss.a;
        
        half oneMinusReflectivity;
        diffColor = EnergyConservationBetweenDiffuseAndSpecular(albedo, specColor, /*out*/ oneMinusReflectivity);
        
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
    
    void initTextureData()
    {
        mainTexture = UNITY_SAMPLE_TEX2D(_MainTex, TRANSFORM_TEX(poiMesh.uv, _MainTex));
        
        #ifdef MIRROR
            applyMirrorTexture();
        #endif
        
        #ifdef _ALPHABLEND_ON
            calculateDissolve();
        #endif
        
        #ifndef POISHADOW
            alphaMask = UNITY_SAMPLE_TEX2D_SAMPLER(_AlphaMask, _MainTex, TRANSFORM_TEX(poiMesh.uv, _AlphaMask));
            albedo = float4(lerp(mainTexture.rgb, dot(mainTexture.rgb, float3(0.3, 0.59, 0.11)), -_Saturation) * _Color.rgb, mainTexture.a * _Color.a * alphaMask);
            
            float3 mainNormal = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap, _MainTex, TRANSFORM_TEX(poiMesh.uv, _BumpMap) + _Time.y * _MainNormalPan), _BumpScale);
            float detailNormalMask = UNITY_SAMPLE_TEX2D_SAMPLER(_DetailNormalMask, _MainTex, TRANSFORM_TEX(poiMesh.uv, _DetailNormalMask));
            float3 detailNormal = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_DetailNormalMap, _MainTex, TRANSFORM_TEX(poiMesh.uv, _DetailNormalMap) + _Time.y * _MainDetailNormalPan), _DetailNormalMapScale * detailNormalMask);
            poiMesh.tangentSpaceNormal = BlendNormals(mainNormal, detailNormal);
            
            poiMesh.fragmentNormal = normalize(
                poiMesh.tangentSpaceNormal.x * poiMesh.tangent +
                poiMesh.tangentSpaceNormal.y * poiMesh.bitangent +
                poiMesh.tangentSpaceNormal.z * poiMesh.vertexNormal
            );
            
            poiLight.nDotV = dot(poiMesh.fragmentNormal, poiCam.viewDir);
            poiLight.nDotL = dot(poiMesh.fragmentNormal, poiLight.direction);
            poiLight.nDotH = dot(poiMesh.fragmentNormal, poiLight.halfDir);
            poiLight.lDotv = dot(poiLight.direction, poiCam.viewDir);
            poiLight.lDotH = dot(poiLight.direction, poiLight.halfDir);
            
            poiCam.viewDotNormal = abs(dot(poiCam.viewDir, poiMesh.fragmentNormal));
            
            
            s = FragmentSetup(float4(poiMesh.uv, 1, 1), poiCam.viewDir, poiMesh.worldPos);
        #endif
    }
    
    void distanceFade()
    {
        mainTexture.a *= smoothstep(_MainDistanceFade.x, _MainDistanceFade.y, poiCam.distanceToModel);
        albedo.a *= smoothstep(_MainDistanceFade.x, _MainDistanceFade.y, poiCam.distanceToModel);
    }
#endif