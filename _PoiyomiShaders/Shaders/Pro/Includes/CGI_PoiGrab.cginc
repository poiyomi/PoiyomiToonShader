#ifndef POI_GRAB
    #define POI_GRAB
    
    float _RefractionIndex;
    float _RefractionOpacity;
    float _RefractionChromaticAberattion;
    float _RefractionEnabled;
    float _GrabSrcBlend;
    float _GrabDstBlend;
    float _GrabPassUseAlpha;
    float _GrabPassBlendFactor;
    float _GrabBlurDistance;
    float _GrabBlurQuality;
    float _GrabBlurDirections;
    POI_TEXTURE_NOSAMPLER(_GrabPassBlendMap);
    
    float4 blur(float2 uv)
    {
        float two_pi = 6.28318530718;
        
        float2 radius = _GrabBlurDistance / _ScreenParams.xy * 100; // Arbitrary constant to match old blur
        float quality = floor(_GrabBlurQuality);
        float directions = floor(_GrabBlurDirections);
        
        // Pixel colour
        float4 color = tex2D(_PoiGrab, uv);
        
        float deltaAngle = two_pi / directions;
        float deltaQuality = 1.0 / quality;
        for (int i = 0; i < directions; i ++)
        {
            for (int j = 0; j < quality; j ++)
            {
                float angle = deltaAngle * i + j;
                float offset = deltaQuality * (j + 1);
                color += tex2D(_PoiGrab, uv + float2(cos(angle), sin(angle)) * radius * offset);
            }
        }
        
        // Output to screen
        color /= quality * directions + 1;
        return color;
    }
    
    inline float4 Refraction(float indexOfRefraction, float chromaticAberration, float2 projectedGrabPos)
    {
        float4 refractionColor;
        float3 worldViewDir = normalize(UnityWorldSpaceViewDir(poiMesh.worldPos));
        float3 refractionOffset = ((((indexOfRefraction - 1.0) * mul(UNITY_MATRIX_V, float4(poiMesh.normals[1], 0.0)).xyz) * (1.0 / (poiCam.grabPos.z + 1.0))) * (1.0 - dot(poiMesh.normals[1], worldViewDir)));
        float2 cameraRefraction = float2(refractionOffset.x, - (refractionOffset.y * _ProjectionParams.x));
        
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
            
            #ifdef CHROMATIC_ABERRATION_LOW
                refractionColor = blur(refractedGrab);
            #else
                refractionColor = tex2D(_PoiGrab, (refractedGrab));
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
            #ifdef CHROMATIC_ABERRATION_LOW
                refraction = blur(projectedGrabPos);
            #else
                refraction = tex2Dproj(_PoiGrab, poiCam.grabPos).rgb;
            #endif
        }
        
        float blendFactor = _GrabPassBlendFactor * POI2D_SAMPLER_PAN(_GrabPassBlendMap, _MainTex, poiMesh.uv[_GrabPassBlendMapUV], _GrabPassBlendMapPan).r;
        UNITY_BRANCH
        if(_GrabPassUseAlpha)
        {
            finalColor = poiBlend(_GrabSrcBlend, finalColor, _GrabDstBlend, float4(refraction, 1), blendFactor * (1 - finalColor.a));
            finalColor.a = 1;
        }
        else
        {
            finalColor = poiBlend(_GrabSrcBlend, finalColor, _GrabDstBlend, float4(refraction, 1), blendFactor);
        }
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