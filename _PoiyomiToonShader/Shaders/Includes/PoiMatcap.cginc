#ifndef MATCAP
    #define MATCAP
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_Matcap);
    UNITY_DECLARE_TEX2D_NOSAMPLER(_MatcapMask); float4 _MatcapMask_ST;
    float _MatcapBorder;
    float4 _MatcapColor;
    float  _MatcapIntensity;
    float _MatcapReplace;
    float _MatcapMultiply;
    float _MatcapAdd;
    float _MatcapEnable;
    float _MatcapLightMask;
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_Matcap2);
    UNITY_DECLARE_TEX2D_NOSAMPLER(_Matcap2Mask); float4 _Matcap2Mask_ST;
    float _Matcap2Border;
    float4 _Matcap2Color;
    float _Matcap2Intensity;
    float _Matcap2Replace;
    float _Matcap2Multiply;
    float _Matcap2Add;
    float _Matcap2Enable;
    float _Matcap2LightMask;
    
    float3 matcap;
    float matcapMask;
    float3 matcap2;
    float matcap2Mask;
    
    
    
    void calculateMatcap()
    {
        // Both matcaps use the same coordinates
        half3 worldViewUp = normalize(half3(0, 1, 0) - poiCam.viewDir * dot(poiCam.viewDir, half3(0, 1, 0)));
        half3 worldViewRight = normalize(cross(poiCam.viewDir, worldViewUp));
        
        // Matcap 1
        half2 matcapUV = half2(dot(worldViewRight, poiMesh.fragmentNormal), dot(worldViewUp, poiMesh.fragmentNormal)) * _MatcapBorder + 0.5;
        matcap = UNITY_SAMPLE_TEX2D_SAMPLER(_Matcap, _MainTex, matcapUV) * _MatcapColor;
        matcap.rgb *= _MatcapIntensity;
        matcapMask = UNITY_SAMPLE_TEX2D_SAMPLER(_MatcapMask, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _MatcapMask));
        #ifdef POI_LIGHTING
            if (_MatcapLightMask)
            {
                matcapMask *= lerp(1, poiLight.rampedLightMap, _MatcapLightMask);
            }
        #endif
        
        // Matcap 2
        UNITY_BRANCH
        if (_Matcap2Enable)
        {
            half2 matcapUV2 = half2(dot(worldViewRight, poiMesh.fragmentNormal), dot(worldViewUp, poiMesh.fragmentNormal)) * _Matcap2Border + 0.5;
            matcap2 = UNITY_SAMPLE_TEX2D_SAMPLER(_Matcap2, _MainTex, matcapUV2) * _Matcap2Color;
            matcap2 *= _Matcap2Intensity;
            matcap2Mask = UNITY_SAMPLE_TEX2D_SAMPLER(_Matcap2Mask, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _Matcap2Mask));
            #ifdef POI_LIGHTING
                if(_Matcap2LightMask)
                {
                    matcap2Mask *= lerp(1, poiLight.rampedLightMap, _Matcap2LightMask);
                }
            #endif
        }
    }
    
    void applyMatcap(inout float4 finalColor)
    {
        finalColor.rgb = lerp(finalColor, matcap, _MatcapReplace * matcapMask);
        finalColor.rgb *= lerp(1, matcap, _MatcapMultiply * matcapMask);
        finalColor.rgb += matcap * _MatcapAdd * matcapMask;
        
        UNITY_BRANCH
        if(_Matcap2Enable)
        {
            finalColor.rgb = lerp(finalColor, matcap2, _Matcap2Replace * matcap2Mask);
            finalColor.rgb *= lerp(1, matcap2, _Matcap2Multiply * matcap2Mask);
            finalColor.rgb += matcap2 * _Matcap2Add * matcap2Mask;
        }
    }
#endif