#ifndef POI_DEPTH_COLOR
    #define POI_DEPTH_COLOR
    
    float4 _DepthGlowColor;
    float _DepthGlowEmission;
    float _FadeLength;
    float _DepthAlphaMin;
    float _DepthAlphaMax;
    float _DepthGradientTextureUV;
    float _DepthGradientBlend;
    
    #if defined(PROP_DEPTHGRADIENT) || !defined(OPTIMIZER_ENABLED)
        POI_TEXTURE_NOSAMPLER(_DepthGradient);
    #endif
    #if defined(PROP_DEPTHMASK) || !defined(OPTIMIZER_ENABLED)
        POI_TEXTURE_NOSAMPLER(_DepthMask);
    #endif
    
    /*
    void applyDepthColor(inout float4 finalColor, inout float3 depthTouchEmission, inout float3 finalEmission, float4 worldDirection, float4 clipPos)
    {
        float3 touchEmission = 0;
        if (!IsInMirror())
        {
            float fadeLength = _FadeLength;
            fadeLength *= 0.01;
            float depth = DecodeFloatRG(tex2Dproj(_CameraDepthTexture, worldDirection));
            depth = Linear01Depth(depth);
            if(depth != 1)
            {
                float diff = distance(depth, Linear01Depth(clipPos.z));
                float intersect = 0;
                if(diff > 0)
                {
                    intersect = clamp(1 - smoothstep(0, _ProjectionParams.w * fadeLength, diff), 0, 1);
                }
                half4 depthGradient = UNITY_SAMPLE_TEX2D_SAMPLER(_DepthGradient, _MainTex, intersect);
                half3 depthMask = UNITY_SAMPLE_TEX2D_SAMPLER(_DepthMask, _MainTex, poiMesh.uv[0]);
                half3 depthColor = depthGradient.rgb * _DepthGlowColor.rgb;
                finalColor.rgb = lerp(finalColor.rgb, depthColor, intersect * depthMask);
                finalColor.a *= lerp(_DepthAlphaMax, _DepthAlphaMin, intersect);
                touchEmission = depthColor * _DepthGlowEmission * intersect * depthMask;
            }
        }
        depthTouchEmission = touchEmission;
    }
    */
    
    inline float CorrectedLinearEyeDepth(float z, float B)
    {
        return 1.0 / (z / PM._34 + B);
    }
    
    void applyDepthColor(inout float4 finalColor, inout float3 depthTouchEmission, inout float3 finalEmission, in float4 worldDirection)
    {
        float3 touchEmission = 0;
        float fadeLength = _FadeLength;
        fadeLength *= 0.01;
        
        float perspectiveDivide = 1.0f / poiCam.clipPos.w;
        float4 direction = worldDirection * perspectiveDivide;
        float2 screenPos = poiCam.grabPos.xy * perspectiveDivide;
        float z = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenPos);
        
        #if UNITY_REVERSED_Z
            if (z == 0)
        #else
            if(z == 1)
        #endif
        return;
        
        float depth = CorrectedLinearEyeDepth(z, direction.w);
        float3 worldpos = direction * depth + _WorldSpaceCameraPos.xyz;
        /*
        finalColor.rgb = frac(worldpos);
        return;
        */
        
        float diff = distance(worldpos, poiMesh.worldPos);
        float intersect = 0;
        intersect = clamp(1 - smoothstep(0, fadeLength, diff), 0, 1);
        #if defined(PROP_DEPTHMASK) || !defined(OPTIMIZER_ENABLED)
            half3 depthMask = POI2D_SAMPLER_PAN(_DepthMask, _MainTex, poiMesh.uv[_DepthMaskUV], _DepthMaskPan);
        #else
            half3 depthMask = 1;
        #endif
        
        half4 depthGradient = 0;
        half3 depthColor = 0;
        
        UNITY_BRANCH
        if (_DepthGradientTextureUV == 0)
        {
            #if defined(PROP_DEPTHGRADIENT) || !defined(OPTIMIZER_ENABLED)
                depthGradient = POI2D_SAMPLER_PAN(_DepthGradient, _MainTex, float2(intersect, intersect), _DepthGradientPan);
            #else
                depthGradient = 1;
            #endif
            depthColor = depthGradient.rgb * _DepthGlowColor.rgb;
        }
        else
        {
            #if defined(PROP_DEPTHGRADIENT) || !defined(OPTIMIZER_ENABLED)
                depthGradient = POI2D_SAMPLER_PAN(_DepthGradient, _MainTex, poiMesh.uv[_DepthGradientUV], _DepthGradientPan);
            #else
                depthGradient = 1;
            #endif
            depthColor = depthGradient.rgb * _DepthGlowColor.rgb * intersect;
        }
        
        UNITY_BRANCH
        if(_DepthGradientBlend == 0) // rpelace
        {
            finalColor.rgb = lerp(finalColor.rgb, depthColor, intersect * depthMask);
        }
        else if (_DepthGradientBlend == 1) // add
        {
            finalColor.rgb += depthColor * intersect * depthMask;
        }
        else if (_DepthGradientBlend == 2) // multiply
        {
            finalColor.rgb *= lerp(1, depthColor, intersect * depthMask);
        }
        finalColor.a *= lerp(_DepthAlphaMax, _DepthAlphaMin, intersect * depthMask);
        touchEmission = depthColor * _DepthGlowEmission * intersect * depthMask;
        
        depthTouchEmission = touchEmission;
    }
#endif