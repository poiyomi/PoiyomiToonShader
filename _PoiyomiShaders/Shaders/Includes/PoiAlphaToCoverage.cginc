#ifndef POI_ALPHA_TO_COVERAGE
    #define POI_ALPHA_TO_COVERAGE
    
    half _MainMipScale;
    float _ForceOpaque;
    float _MainAlphaToCoverage;

    float CalcMipLevel()
    {
        float2 dx = ddx(poiMesh.uv[0] * _MainTex_TexelSize.zw);
        float2 dy = ddy(poiMesh.uv[0] * _MainTex_TexelSize.zw);
        float delta_max_sqr = max(dot(dx, dx), dot(dy, dy));
        
        return max(0.0, 0.5 * log2(delta_max_sqr));
    }
    
    void ApplyAlphaToCoverage(inout float4 finalColor)
    {
        // Force Model Opacity to 1 if desired
        finalColor.a = max(_ForceOpaque, finalColor.a);
        UNITY_BRANCH
        if (_MainAlphaToCoverage)
        {
            // rescale alpha by mip level (if not using preserved coverage mip maps)
            finalColor.a *= 1 + max(0, CalcMipLevel()) * _MainMipScale;
            // rescale alpha by partial derivative
            finalColor.a = (finalColor.a - _Clip) / fwidth(finalColor.a) + 0.5;
        }
    }
#endif