#ifndef POI_GRAB
    #define POI_GRAB
    
    float _RefractionIndex;
    float _RefractionOpacity;
    float _RefractionChromaticAberattion;
    float _RefractionEnabled;
    uint _SourceBlend, _DestinationBlend;
    float _GrabBlurDistance;
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_RefractionOpacityMask); float4 _RefractionOpacityMask_ST;
    
    inline float4 Refraction(float indexOfRefraction, float chromaticAberration, float2 projectedGrabPos)
    {
        float4 refractionColor;
        float3 worldViewDir = normalize(UnityWorldSpaceViewDir(poiMesh.worldPos));
        float3 refractionOffset = ((((indexOfRefraction - 1.0) * mul(UNITY_MATRIX_V, float4(poiMesh.normals[1], 0.0))) * (1.0 / (poiCam.grabPos.z + 1.0))) * (1.0 - dot(poiMesh.normals[1], worldViewDir)));
        float2 cameraRefraction = float2(refractionOffset.x, - (refractionOffset.y * _ProjectionParams.x));
        //return tex2D(_PoiGrab, (projgrabPos + cameraRefraction));
        UNITY_BRANCH
        if (_RefractionChromaticAberattion > 0)
        {
            float4 redAlpha = tex2D(_PoiGrab, (projectedGrabPos + cameraRefraction));
            float green = tex2D(_PoiGrab, (projectedGrabPos + (cameraRefraction * (1.0 - chromaticAberration)))).g;
            float blue = tex2D(_PoiGrab, (projectedGrabPos + (cameraRefraction * (1.0 + chromaticAberration)))).b;
            refractionColor = float4(redAlpha.r, green, blue, redAlpha.a);
        }
        else
        {
            float2 refractedGrab = projectedGrabPos + cameraRefraction;
            refractionColor = tex2D(_PoiGrab, (refractedGrab));
            #ifdef CHROMATIC_ABERRATION_LOW
                float3 offset = float3(0.0, .05, .12);
                float3 weight = float3(0.2270270270, 0.3162162162, 0.0702702703) * .5;
                refractionColor *= weight[0];
                
                
                refractionColor += tex2D(_PoiGrab, refractedGrab + float2(0.0, offset.y * _GrabBlurDistance)) * weight.y;
                refractionColor += tex2D(_PoiGrab, refractedGrab - float2(0.0, offset.y * _GrabBlurDistance)) * weight.y;
                refractionColor += tex2D(_PoiGrab, refractedGrab + float2(0.0, offset.z * _GrabBlurDistance)) * weight.z;
                refractionColor += tex2D(_PoiGrab, refractedGrab - float2(0.0, offset.z * _GrabBlurDistance)) * weight.z;
                refractionColor += tex2D(_PoiGrab, refractedGrab + float2(offset.y * _GrabBlurDistance, 0.0)) * weight.y;
                refractionColor += tex2D(_PoiGrab, refractedGrab - float2(offset.y * _GrabBlurDistance, 0.0)) * weight.y;
                refractionColor += tex2D(_PoiGrab, refractedGrab + float2(offset.z * _GrabBlurDistance, 0.0)) * weight.z;
                refractionColor += tex2D(_PoiGrab, refractedGrab - float2(offset.z * _GrabBlurDistance, 0.0)) * weight.z;
                
            #endif
        }
        return refractionColor;
    }
    
    void calculateRefraction(float2 projectedGrabPos, inout float4 finalColor)
    {
        float3 refraction = 1;
        UNITY_BRANCH
        if(_RefractionEnabled == 1)
        {
            refraction = Refraction(_RefractionIndex, _RefractionChromaticAberattion, projectedGrabPos).rgb;
        }
        else
        {
            refraction = tex2Dproj(_PoiGrab, poiCam.grabPos);
            
            #ifdef CHROMATIC_ABERRATION_LOW
                float3 offset = float3(0.0, .05, .12);
                float3 weight = float3(0.2270270270, 0.3162162162, 0.0702702703) * .5;
                refraction *= weight[0];
                
                
                refraction += tex2D(_PoiGrab, projectedGrabPos + float2(0.0, offset.y * _GrabBlurDistance)) * weight.y;
                refraction += tex2D(_PoiGrab, projectedGrabPos - float2(0.0, offset.y * _GrabBlurDistance)) * weight.y;
                refraction += tex2D(_PoiGrab, projectedGrabPos + float2(0.0, offset.z * _GrabBlurDistance)) * weight.z;
                refraction += tex2D(_PoiGrab, projectedGrabPos - float2(0.0, offset.z * _GrabBlurDistance)) * weight.z;
                refraction += tex2D(_PoiGrab, projectedGrabPos + float2(offset.y * _GrabBlurDistance, 0.0)) * weight.y;
                refraction += tex2D(_PoiGrab, projectedGrabPos - float2(offset.y * _GrabBlurDistance, 0.0)) * weight.y;
                refraction += tex2D(_PoiGrab, projectedGrabPos + float2(offset.z * _GrabBlurDistance, 0.0)) * weight.z;
                refraction += tex2D(_PoiGrab, projectedGrabPos - float2(offset.z * _GrabBlurDistance, 0.0)) * weight.z;
                
            #endif
        }
        
        finalColor.a *= alphaMask;
        finalColor = poiBlend(_SourceBlend, finalColor, _DestinationBlend, float4(refraction, 1));
        finalColor.a = 1;
    }
    
    float2 calculateGrabPosition()
    {
        float4 grabPos = poiCam.grabPos;
        #if UNITY_UV_STARTS_AT_TOP
            float scale = -1.0;
        #else
            float scale = 1.0;
        #endif
        float halfPosW = grabPos.w * 0.5;
        grabPos.y = (grabPos.y - halfPosW) * _ProjectionParams.x * scale + halfPosW;
        #if SHADER_API_D3D9 || SHADER_API_D3D11
            grabPos.w += 0.00000000001;
        #endif
        return(grabPos / grabPos.w).xy;
    }
    
    void applyGrabEffects(inout float4 finalColor)
    {
        float2 projectedGrabPos = calculateGrabPosition();
        calculateRefraction(projectedGrabPos, finalColor);
    }
    
#endif