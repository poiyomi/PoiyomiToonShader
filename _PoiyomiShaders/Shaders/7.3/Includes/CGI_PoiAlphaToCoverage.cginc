#ifndef POI_ALPHA_TO_COVERAGE
    #define POI_ALPHA_TO_COVERAGE
    
    half _MainMipScale;
    float _MainAlphaToCoverage;
    
    float CalcMipLevel(float2 texture_coord)
    {
        float2 dx = ddx(texture_coord);
        float2 dy = ddy(texture_coord);
        float delta_max_sqr = max(dot(dx, dx), dot(dy, dy));
        
        return 0.5 * log2(delta_max_sqr);
    }
    
    void ApplyAlphaToCoverage(inout float4 finalColor)
    {
        // Force Model Opacity to 1 if desired
        
        UNITY_BRANCH
        if (_MainAlphaToCoverage)
        {
            // rescale alpha by mip level
            finalColor.a *= 1 + max(0, CalcMipLevel(poiMesh.uv[0] * _MainTex_TexelSize.zw)) * _MainMipScale;
            // rescale alpha by partial derivative
            finalColor.a = (finalColor.a - _Clip) / max(fwidth(finalColor.a), 0.0001) + _Clip;
        }
    }
#endif