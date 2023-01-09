#ifndef MATCAP
    #define MATCAP
    
    #if defined(PROP_MATCAP) || !defined(OPTIMIZER_ENABLED)
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Matcap); float4 _Matcap_ST;
    #endif
    #if defined(PROP_MATCAPMASK) || !defined(OPTIMIZER_ENABLED)
        POI_TEXTURE_NOSAMPLER(_MatcapMask);
    #endif
    float _MatcapMaskInvert;
    float _MatcapBorder;
    float4 _MatcapColor;
    float _MatcapIntensity;
    float _MatcapReplace;
    float _MatcapMultiply;
    float _MatcapAdd;
    float _MatcapEnable;
    float _MatcapLightMask;
    float _MatcapEmissionStrength;
    float _MatcapNormal;
    float _MatcapHueShiftEnabled;
    float _MatcapHueShiftSpeed;
    float _MatcapHueShift;
    
    #ifdef COLOR_GRADING_HDR_3D
        #if defined(PROP_MATCAP2) || !defined(OPTIMIZER_ENABLED)
            UNITY_DECLARE_TEX2D_NOSAMPLER(_Matcap2);float4 _Matcap2_ST;
        #endif
        #if defined(PROP_MATCAP2MASK) || !defined(OPTIMIZER_ENABLED)
            POI_TEXTURE_NOSAMPLER(_Matcap2Mask);
        #endif
        float _Matcap2MaskInvert;
        float _Matcap2Border;
        float4 _Matcap2Color;
        float _Matcap2Intensity;
        float _Matcap2Replace;
        float _Matcap2Multiply;
        float _Matcap2Add;
        float _Matcap2Enable;
        float _Matcap2LightMask;
        float _Matcap2EmissionStrength;
        float _Matcap2Normal;
        float _Matcap2HueShiftEnabled;
        float _Matcap2HueShiftSpeed;
        float _Matcap2HueShift;
    #endif
    
    void blendMatcap(inout float4 finalColor, float add, float multiply, float replace, float4 matcapColor, float matcapMask, inout float3 matcapEmission, float emissionStrength
    #ifdef POI_LIGHTING
    , float matcapLightMask
    #endif
    #ifdef POI_BLACKLIGHT
    , uint blackLightMaskIndex
    #endif
    )
    {
        #ifdef POI_LIGHTING
            if (matcapLightMask)
            {
                matcapMask *= lerp(1, poiLight.rampedLightMap, matcapLightMask);
            }
        #endif
        #ifdef POI_BLACKLIGHT
            if(blackLightMaskIndex != 4)
            {
                matcapMask *= blackLightMask[blackLightMaskIndex];
            }
        #endif
        
        finalColor.rgb = lerp(finalColor.rgb, matcapColor.rgb, replace * matcapMask * matcapColor.a * .999999);
        finalColor.rgb *= lerp(1, matcapColor.rgb, multiply * matcapMask * matcapColor.a);
        finalColor.rgb += matcapColor.rgb * add * matcapMask * matcapColor.a;
        matcapEmission += matcapColor.rgb * emissionStrength * matcapMask * matcapColor.a;
    }
    
    void applyMatcap(inout float4 finalColor, inout float3 matcapEmission)
    {
        float4 matcap = 0;
        float matcapMask = 0;
        float4 matcap2 = 0;
        float matcap2Mask = 0;
        
        // Both matcaps use the same coordinates
        half3 worldViewUp = normalize(half3(0, 1, 0) - poiCam.viewDir * dot(poiCam.viewDir, half3(0, 1, 0)));
        half3 worldViewRight = normalize(cross(poiCam.viewDir, worldViewUp));
        
        // Matcap 1
        half2 matcapUV = half2(dot(worldViewRight, poiMesh.normals[_MatcapNormal]), dot(worldViewUp, poiMesh.normals[_MatcapNormal])) * _MatcapBorder + 0.5;
        
        #if defined(PROP_MATCAP) || !defined(OPTIMIZER_ENABLED)
            matcap = UNITY_SAMPLE_TEX2D_SAMPLER(_Matcap, _MainTex, TRANSFORM_TEX(matcapUV, _Matcap)) * _MatcapColor;
        #else
            matcap = _MatcapColor;
        #endif

        matcap.rgb *= _MatcapIntensity;
        #if defined(PROP_MATCAPMASK) || !defined(OPTIMIZER_ENABLED)
            matcapMask = POI2D_SAMPLER_PAN(_MatcapMask, _MainTex, poiMesh.uv[_MatcapMaskUV], _MatcapMaskPan);
        #else
            matcapMask = 1;
        #endif
        
        if (_MatcapMaskInvert)
        {
            matcapMask = 1 - matcapMask;
        }
        
        UNITY_BRANCH
        if(_MatcapHueShiftEnabled)
        {
            matcap.rgb = hueShift(matcap.rgb, _MatcapHueShift + _Time.x * _MatcapHueShiftSpeed);
        }
        
        blendMatcap(finalColor, _MatcapAdd, _MatcapMultiply, _MatcapReplace, matcap, matcapMask, matcapEmission, _MatcapEmissionStrength
        #ifdef POI_LIGHTING
        , _MatcapLightMask
        #endif
        #ifdef POI_BLACKLIGHT
        , _BlackLightMaskMatcap
        #endif
        );
        
        
        // Matcap 2
        #ifdef COLOR_GRADING_HDR_3D
            half2 matcapUV2 = half2(dot(worldViewRight, poiMesh.normals[_Matcap2Normal]), dot(worldViewUp, poiMesh.normals[_Matcap2Normal])) * _Matcap2Border + 0.5;
            #if defined(PROP_MATCAP2) || !defined(OPTIMIZER_ENABLED)
                matcap2 = UNITY_SAMPLE_TEX2D_SAMPLER(_Matcap2, _MainTex, TRANSFORM_TEX(matcapUV2, _Matcap2)) * _Matcap2Color;
            #else
                matcap2 = _Matcap2Color;
            #endif
            matcap2.rgb *= _Matcap2Intensity;
            #if defined(PROP_MATCAP2MASK) || !defined(OPTIMIZER_ENABLED)
                matcap2Mask = POI2D_SAMPLER_PAN(_Matcap2Mask, _MainTex, poiMesh.uv[_Matcap2MaskUV], _Matcap2MaskPan);
            #else
                matcap2Mask = 1;
            #endif
            if (_Matcap2MaskInvert)
            {
                matcap2Mask = 1 - matcap2Mask;
            }
            
            UNITY_BRANCH
            if(_Matcap2HueShiftEnabled)
            {
                matcap2.rgb = hueShift(matcap2.rgb, _Matcap2HueShift + _Time.x * _Matcap2HueShiftSpeed);
            }
            
            blendMatcap(finalColor, _Matcap2Add, _Matcap2Multiply, _Matcap2Replace, matcap2, matcap2Mask, matcapEmission, _Matcap2EmissionStrength
            #ifdef POI_LIGHTING
            , _Matcap2LightMask
            #endif
            #ifdef POI_BLACKLIGHT
            , _BlackLightMaskMatcap2
            #endif
            );
        #endif
    }
    
#endif