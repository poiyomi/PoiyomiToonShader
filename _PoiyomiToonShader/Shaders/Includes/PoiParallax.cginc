#ifndef POI_PARALLAX
    #define POI_PARALLAX
    
    sampler2D _ParallaxHeightMap; float4 _ParallaxHeightMap_ST;
    float _ParallaxHeightIterations;
    float _ParallaxStrength;
    float _ParallaxBias;
    float _ParallaxHeightMapEnabled;
    
    //Internal
    float _ParallaxInternalMapEnabled;
    sampler2D _ParallaxInternalMap; float4 _ParallaxInternalMap_ST;
    float _ParallaxInternalIterations;
    float _ParallaxInternalMinDepth;
    float _ParallaxInternalMaxDepth;
    float _ParallaxInternalMinFade;
    float _ParallaxInternalMaxFade;
    float4 _ParallaxInternalMinColor;
    float4 _ParallaxInternalMaxColor;
    float4 _ParallaxInternalPanSpeed;
    float4 _ParallaxInternalPanDepthSpeed;
    float _ParallaxInternalHeightmapMode;
    float _ParallaxInternalHeightFromAlpha;
    
    float GetParallaxHeight(float2 uv)
    {
        return clamp(tex2D(_ParallaxHeightMap, TRANSFORM_TEX(uv, _ParallaxHeightMap)).g, 0, .99999);
    }
    /*
    float2 ParallaxOffset(float2 viewDir)
    {
        float height = GetParallaxHeight();
        height -= 0.5;
        height *= _ParallaxStrength;
        return viewDir * height;
    }
    */
    float2 ParallaxRaymarching(float2 viewDir)
    {
        float2 uvOffset = 0;
        float stepSize = 0.1;
        float2 uvDelta = viewDir * (stepSize * _ParallaxStrength);
        
        float stepHeight = 1;
        float surfaceHeight = GetParallaxHeight(poiMesh.uv[0]);
        
        float2 prevUVOffset = uvOffset;
        float prevStepHeight = stepHeight;
        float prevSurfaceHeight = surfaceHeight;
        
        for (int i = 1; i < _ParallaxHeightIterations && stepHeight > surfaceHeight; i ++)
        {
            prevUVOffset = uvOffset;
            prevStepHeight = stepHeight;
            prevSurfaceHeight = surfaceHeight;
            
            uvOffset -= uvDelta;
            stepHeight -= stepSize;
            surfaceHeight = GetParallaxHeight(poiMesh.uv[0] + uvOffset);
        }
        
        float prevDifference = prevStepHeight - prevSurfaceHeight;
        float difference = surfaceHeight - stepHeight;
        float t = prevDifference / (prevDifference + difference);
        uvOffset = prevUVOffset -uvDelta * t;
        
        return uvOffset;
    }
    
    void calculateandApplyParallax(v2f i)
    {
        UNITY_BRANCH
        if (_ParallaxHeightMapEnabled)
        {
            i.tangentViewDir = normalize(i.tangentViewDir);
            i.tangentViewDir.xy /= (i.tangentViewDir.z + _ParallaxBias);
            poiMesh.uv[0] += ParallaxRaymarching(i.tangentViewDir.xy);
        }
    }
    
    void calculateAndApplyInternalParallax()
    {
        #if defined(_PARALLAXMAP)
            UNITY_BRANCH
            if(_ParallaxInternalMapEnabled)
            {
                float3 parallax = 0;
                for (int j = _ParallaxInternalIterations; j > 0; j --)
                {
                    float ratio = (float)j / _ParallaxInternalIterations;
                    float2 parallaxOffset = _Time.y * (_ParallaxInternalPanSpeed + (1 - ratio) * _ParallaxInternalPanDepthSpeed);
                    float fade = lerp(_ParallaxInternalMinFade, _ParallaxInternalMaxFade, ratio);
                    float4 parallaxColor = tex2D(_ParallaxInternalMap, TRANSFORM_TEX(poiMesh.uv[0], _ParallaxInternalMap) + lerp(_ParallaxInternalMinDepth, _ParallaxInternalMaxDepth, ratio) * - poiCam.tangentViewDir.xy + parallaxOffset);
                    float3 parallaxTint = lerp(_ParallaxInternalMinColor, _ParallaxInternalMaxColor, ratio);
                    float parallaxHeight;
                    if(_ParallaxInternalHeightFromAlpha)
                    {
                        parallaxTint *= parallaxColor.rgb;
                        parallaxHeight = parallaxColor.a;
                    }
                    else
                    {
                        parallaxHeight = parallaxColor.r;
                    }
                    //float parallaxColor *= lerp(_ParallaxInternalMinColor, _ParallaxInternalMaxColor, 1 - ratio);
                    UNITY_BRANCH
                    if (_ParallaxInternalHeightmapMode == 1)
                    {
                        parallax = lerp(parallax, parallaxTint * fade, parallaxHeight >= 1 - ratio);
                    }
                    else
                    {
                        parallax += parallaxTint * parallaxHeight * fade;
                    }
                }
                //parallax /= _ParallaxInternalIterations;
                finalColor.rgb += parallax;
            }
        #endif
    }
#endif