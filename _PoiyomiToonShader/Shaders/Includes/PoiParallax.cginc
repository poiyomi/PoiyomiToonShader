#ifndef POI_PARALLAX
    #define POI_PARALLAX
    
    sampler2D _ParallaxHeightMap; float4 _ParallaxHeightMap_ST;
    float _ParallaxStrength;
    float _ParallaxBias;
    
    float GetParallaxHeight(float2 uv)
    {
        return clamp(tex2D(_ParallaxHeightMap, TRANSFORM_TEX(uv, _ParallaxHeightMap)).g,0,.99999);
    }
    
    float2 ParallaxOffset(float2 uv, float2 viewDir)
    {
        float height = GetParallaxHeight(uv);
        height -= 0.5;
        height *= _ParallaxStrength;
        return viewDir * height;
    }
    
    float2 ParallaxRaymarching(float2 uv, float2 viewDir)
    {
        float2 uvOffset = 0;
        float stepSize = 0.1;
        float2 uvDelta = viewDir * (stepSize * _ParallaxStrength);

        float stepHeight = 1;
        float surfaceHeight = GetParallaxHeight(uv);
        
        float2 prevUVOffset = uvOffset;
        float prevStepHeight = stepHeight;
        float prevSurfaceHeight = surfaceHeight;
        
        for (int i = 1; i < 10 && stepHeight > surfaceHeight; i ++)
        {
            prevUVOffset = uvOffset;
            prevStepHeight = stepHeight;
            prevSurfaceHeight = surfaceHeight;
            
            uvOffset -= uvDelta;
            stepHeight -= stepSize;
            surfaceHeight = GetParallaxHeight(uv + uvOffset);
        }
        
        float prevDifference = prevStepHeight - prevSurfaceHeight;
        float difference = surfaceHeight - stepHeight;
        float t = prevDifference / (prevDifference + difference);
        uvOffset = prevUVOffset -uvDelta * t;
        
        return uvOffset;
    }
    
    void calculateandApplyParallax(inout v2f i)
    {
        #if defined(_PARALLAX_MAP)
            i.tangentViewDir = normalize(i.tangentViewDir);
            i.tangentViewDir.xy /= (i.tangentViewDir.z + _ParallaxBias);
            float2 uvOffset = ParallaxRaymarching(i.uv.xy, i.tangentViewDir.xy);
            i.uv.xy += uvOffset;
        #endif
    }
    
#endif