#ifndef SUBSURFACE
    #define SUBSURFACE
    
    float _SSSThicknessMod;
    float _SSSSCale;
    float _SSSPower;
    float _SSSDistortion;
    float4 _SSSColor;
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_SSSThicknessMap); float4 _SSSThicknessMap_ST;
    
    float3 finalSSS;
    
    void calculateSubsurfaceScattering(v2f i, float3 viewDir)
    {
        float SSS = 1 - UNITY_SAMPLE_TEX2D_SAMPLER(_SSSThicknessMap, _MainTex, TRANSFORM_TEX(i.uv, _SSSThicknessMap));
        
        half3 vLTLight = poiLight.direction + baseNormal * _SSSDistortion;
        half flTDot = pow(saturate(dot(viewDir, -vLTLight)), _SSSPower) * _SSSSCale;
        half3 fLT = poiLight.attenuation * (flTDot) * saturate(SSS + -1 * _SSSThicknessMod);
        
        finalSSS = fLT;
    }
    
    void applySubsurfaceScattering(inout float4 finalColor)
    {
        finalColor.rgb += finalSSS * poiLight.color * albedo * _SSSColor;
    }
    
#endif