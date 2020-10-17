#ifndef MATCAP
    #define MATCAP
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_Matcap);
    POI_TEXTURE_NOSAMPLER(_MatcapMask);
    float _MatcapBorder;
    float4 _MatcapColor;
    float _MatcapIntensity;
    float _MatcapReplace;
    float _MatcapMultiply;
    float _MatcapAdd;
    float _MatcapEnable;
    float _MatcapLightMask;
    float _MatcapEmissionStrength;
    uint _MatcapNormal;
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_Matcap2);
    POI_TEXTURE_NOSAMPLER(_Matcap2Mask);
    float _Matcap2Border;
    float4 _Matcap2Color;
    float _Matcap2Intensity;
    float _Matcap2Replace;
    float _Matcap2Multiply;
    float _Matcap2Add;
    float _Matcap2Enable;
    float _Matcap2LightMask;
    float _Matcap2EmissionStrength;
    uint _Matcap2Normal;
    
    void blendMatcap(inout float4 finalColor, float add, float multiply, float replace, float4 matcapColor, float matcapMask, inout float3 matcapEmission, float emissionStrength
    #ifdef POI_LIGHTING
        ,float matcapLightMask
    #endif
    #ifdef POI_BLACKLIGHT
        ,uint blackLightMaskIndex
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
        
        finalColor.rgb = lerp(finalColor.rgb, matcapColor.rgb, replace * matcapMask * matcapColor.a);
        finalColor.rgb *= lerp(1, matcapColor.rgb, multiply * matcapMask * matcapColor.a);
        finalColor.rgb += matcapColor.rgb * add * matcapMask * matcapColor.a;
        matcapEmission += matcapColor.rgb * emissionStrength * matcapMask * matcapColor.a;
    }
    
    float3 applyMatcap(inout float4 finalColor)
    {
        float4 matcap = 0;
        float matcapMask = 0;
        float4 matcap2 = 0;
        float matcap2Mask = 0;
        float3 matcapEmission = 0;
        
        // Both matcaps use the same coordinates
        half3 worldViewUp = normalize(half3(0, 1, 0) - poiCam.viewDir * dot(poiCam.viewDir, half3(0, 1, 0)));
        half3 worldViewRight = normalize(cross(poiCam.viewDir, worldViewUp));
        
        // Matcap 1
        half2 matcapUV = half2(dot(worldViewRight, poiMesh.normals[_MatcapNormal]), dot(worldViewUp, poiMesh.normals[_MatcapNormal])) * _MatcapBorder + 0.5;
        matcap = UNITY_SAMPLE_TEX2D_SAMPLER(_Matcap, _MainTex, matcapUV) * _MatcapColor;
        matcap.rgb *= _MatcapIntensity;
        matcapMask = POI2D_SAMPLER_PAN(_MatcapMask, _MainTex, poiMesh.uv[_MatcapMaskUV], _MatcapMaskPan);
        blendMatcap(finalColor, _MatcapAdd, _MatcapMultiply, _MatcapReplace, matcap, matcapMask, matcapEmission, _MatcapEmissionStrength
        #ifdef POI_LIGHTING 
            ,_MatcapLightMask
        #endif
        #ifdef POI_BLACKLIGHT 
            ,_BlackLightMaskMatcap
        #endif
        );
        
        // Matcap 2
        UNITY_BRANCH
        if (_Matcap2Enable)
        {
            half2 matcapUV2 = half2(dot(worldViewRight, poiMesh.normals[_Matcap2Normal]), dot(worldViewUp, poiMesh.normals[_Matcap2Normal])) * _Matcap2Border + 0.5;
            matcap2 = UNITY_SAMPLE_TEX2D_SAMPLER(_Matcap2, _MainTex, matcapUV2) * _Matcap2Color;
            matcap2.rgb *= _Matcap2Intensity;
            matcap2Mask = POI2D_SAMPLER_PAN(_Matcap2Mask, _MainTex, poiMesh.uv[_Matcap2MaskUV], _Matcap2MaskPan);
            blendMatcap(finalColor, _Matcap2Add, _Matcap2Multiply, _Matcap2Replace, matcap2, matcap2Mask, matcapEmission, _Matcap2EmissionStrength
            #ifdef POI_LIGHTING
                ,_Matcap2LightMask
            #endif
            #ifdef POI_BLACKLIGHT
                ,_BlackLightMaskMatcap2
            #endif
            );
        }
        
        return matcapEmission;
    }
    
#endif