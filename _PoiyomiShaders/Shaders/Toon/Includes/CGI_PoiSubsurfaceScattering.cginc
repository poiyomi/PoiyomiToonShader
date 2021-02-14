#ifndef SUBSURFACE
    #define SUBSURFACE
    
    float _SSSThicknessMod;
    float _SSSSCale;
    float _SSSPower;
    float _SSSDistortion;
    float4 _SSSColor;
    float _EnableSSS;
    
    POI_TEXTURE_NOSAMPLER(_SSSThicknessMap);

    float3 calculateSubsurfaceScattering()
    {
        float SSS = 1 - POI2D_SAMPLER_PAN(_SSSThicknessMap, _MainTex, poiMesh.uv[_SSSThicknessMapUV], _SSSThicknessMapPan);
        
        half3 vLTLight = poiLight.direction + poiMesh.normals[0] * _SSSDistortion;
        half flTDot = pow(saturate(dot(poiCam.viewDir, -vLTLight)), _SSSPower) * _SSSSCale;
        #ifdef FORWARD_BASE_PASS
            half3 fLT = (flTDot) * saturate(SSS + - 1 * _SSSThicknessMod);
        #else
            half3 fLT = poiLight.attenuation * (flTDot) * saturate(SSS + - 1 * _SSSThicknessMod);
        #endif
        
        return fLT * poiLight.color * _SSSColor;
    }
    
#endif