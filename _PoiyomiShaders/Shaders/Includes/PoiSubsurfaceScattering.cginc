#ifndef SUBSURFACE
    #define SUBSURFACE
    
    float _SSSThicknessMod;
    float _SSSAttenuation;
    float _SSSPower;
    float _SSSDistortion;
    float4 _SSSColor;
    float _EnableSSS;
    float _SSSNormals;
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_SSSThicknessMap); float4 _SSSThicknessMap_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_SSSColorMap); float4 _SSSColorMap_ST;
    
    float3 finalSSS;
    
    float3 getSubsurfaceLighting()
    {
        float thiccMap = 1 - UNITY_SAMPLE_TEX2D_SAMPLER(_SSSThicknessMap, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _SSSThicknessMap)).r;
        float3 internalColor = UNITY_SAMPLE_TEX2D_SAMPLER(_SSSColorMap, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _SSSColorMap)).rgb * _SSSColor;
        
        float subsurfaceBase = dot(poiCam.viewDir, (poiLight.direction + poiMesh.normals[_SSSNormals] * _SSSDistortion) * - 1) * thiccMap;
        float InternalLerpAlpha = saturate(pow(subsurfaceBase, _SSSPower));
        //coloring
        float3 subsurfaceColor = lerp(subsurfaceBase * internalColor, poiLight.color, InternalLerpAlpha);
        subsurfaceColor *= poiMax(poiLight.color) * subsurfaceBase;
        subsurfaceColor *= lerp(1, poiLight.attenuation, _SSSAttenuation);
        return subsurfaceColor;
    }
    
    void calculateSubsurfaceScattering()
    {
        float SSS = 1 - UNITY_SAMPLE_TEX2D_SAMPLER(_SSSThicknessMap, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _SSSThicknessMap));
        
        half3 vLTLight = poiLight.direction + poiMesh.normals[0] * _SSSDistortion;
        half flTDot = pow(saturate(dot(poiCam.viewDir, -vLTLight)), _SSSPower) * _SSSAttenuation;
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