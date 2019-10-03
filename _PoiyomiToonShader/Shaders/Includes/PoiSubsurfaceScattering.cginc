#ifndef SUBSURFACE
    #define SUBSURFACE
    
    float _SSSThicknessMod;
    float _SSSSCale;
    float _SSSPower;
    float _SSSDistortion;
    float4 _SSSColor;
    float _EnableSSS;
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_SSSThicknessMap); float4 _SSSThicknessMap_ST;
    
    float3 finalSSS;
    
    void calculateSubsurfaceScattering()
    {
        float SSS = 1 - UNITY_SAMPLE_TEX2D_SAMPLER(_SSSThicknessMap, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _SSSThicknessMap));
        
        half3 vLTLight = poiLight.direction + poiMesh.vertexNormal * _SSSDistortion;
        half flTDot = pow(saturate(dot(poiCam.viewDir, -vLTLight)), _SSSPower) * _SSSSCale;
        #ifdef FORWARD_BASE_PASS
            half3 fLT = (flTDot) * saturate(SSS + - 1 * _SSSThicknessMod);
        #else
            half3 fLT = poiLight.attenuation * (flTDot) * saturate(SSS + - 1 * _SSSThicknessMod);
        #endif
        
        finalSSS = fLT;
    }
    
    void applySubsurfaceScattering(inout float4 finalColor)
    {
        finalColor.rgb += finalSSS * poiLight.color * albedo * _SSSColor;
    }
    
#endif