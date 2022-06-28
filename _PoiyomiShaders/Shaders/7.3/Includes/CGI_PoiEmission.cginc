#ifndef POI_EMISSION
    #define POI_EMISSION
    
    float4 _EmissionColor;
    POI_TEXTURE_NOSAMPLER(_EmissionMap);
    POI_TEXTURE_NOSAMPLER(_EmissionMask);
    float _EmissionBaseColorAsMap;
    float _EmissionStrength;
    float _EnableEmission;
    float _EmissionHueShift;
    float4 _EmissiveScroll_Direction;
    float _EmissiveScroll_Width;
    float _EmissiveScroll_Velocity;
    float _EmissiveScroll_Interval;
    float _EmissiveBlink_Min;
    float _EmissiveBlink_Max;
    float _EmissiveBlink_Velocity;
    float _ScrollingEmission;
    float _EnableGITDEmission;
    float _GITDEMinEmissionMultiplier;
    float _GITDEMaxEmissionMultiplier;
    float _GITDEMinLight;
    float _GITDEMaxLight;
    uint _GITDEWorldOrMesh;
    float _EmissionCenterOutEnabled;
    float _EmissionCenterOutSpeed;
    float _EmissionHueShiftEnabled;
    float _EmissionBlinkingOffset;
    float _EmissionScrollingOffset;
    
    float4 _EmissionColor1;
    POI_TEXTURE_NOSAMPLER(_EmissionMap1);
    POI_TEXTURE_NOSAMPLER(_EmissionMask1);
    float _EmissionBaseColorAsMap1;
    float _EmissionStrength1;
    float _EnableEmission1;
    float _EmissionHueShift1;
    float4 _EmissiveScroll_Direction1;
    float _EmissiveScroll_Width1;
    float _EmissiveScroll_Velocity1;
    float _EmissiveScroll_Interval1;
    float _EmissiveBlink_Min1;
    float _EmissiveBlink_Max1;
    float _EmissiveBlink_Velocity1;
    float _ScrollingEmission1;
    float _EnableGITDEmission1;
    float _GITDEMinEmissionMultiplier1;
    float _GITDEMaxEmissionMultiplier1;
    float _GITDEMinLight1;
    float _GITDEMaxLight1;
    uint _GITDEWorldOrMesh1;
    float _EmissionCenterOutEnabled1;
    float _EmissionCenterOutSpeed1;
    float _EmissionHueShiftEnabled1;
    float _EmissionBlinkingOffset1;
    float _EmissionScrollingOffset1;
    
    float _EmissionReplace;
    
    float _EmissionScrollingUseCurve;
    float _EmissionScrollingUseCurve1;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_EmissionScrollingCurve); float4 _EmissionScrollingCurve_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_EmissionScrollingCurve1); float4 _EmissionScrollingCurve1_ST;
    
    float calculateGlowInTheDark(in float minLight, in float maxLight, in float minEmissionMultiplier, in float maxEmissionMultiplier, in float enabled)
    {
        float glowInTheDarkMultiplier = 1;
        
        #ifdef POI_LIGHTING
            float3 lightValue = _GITDEWorldOrMesh ? poiLight.finalLighting.rgb: poiLight.directLighting.rgb;
            float gitdeAlpha = (clamp(poiMax(lightValue), minLight, maxLight) - minLight) / (maxLight - minLight);
            glowInTheDarkMultiplier = lerp(minEmissionMultiplier, maxEmissionMultiplier, gitdeAlpha);
            glowInTheDarkMultiplier = lerp(1, glowInTheDarkMultiplier, enabled);
        #endif
        
        return glowInTheDarkMultiplier;
    }
    
    float calculateScrollingEmission(in float3 direction, in float velocity, in float interval, in float scrollWidth, in float enabled, float offset)
    {
        float phase = 0;
        phase = dot(poiMesh.localPos, direction);
        phase -= (_Time.y + offset) * velocity;
        phase /= interval;
        phase -= floor(phase);
        float width = scrollWidth;
        phase = (pow(phase, width) + pow(1 - phase, width * 4)) * 0.5;
        return lerp(1, phase, enabled);
    }
    
    float calculateBlinkingEmission(in float blinkMin, in float blinkMax, in float blinkVelocity, float offset)
    {
        float amplitude = (blinkMax - blinkMin) * 0.5f;
        float base = blinkMin + amplitude;
        return sin((_Time.y + offset) * blinkVelocity) * amplitude + base;
    }
    
    float3 calculateEmissionNew(in float4 baseColor, inout float4 finalColor)
    {
        // First Emission
        float3 emission0 = 0;
        float emissionStrength0 = _EmissionStrength;
        float3 emissionColor0 = 0;
        
        float glowInTheDarkMultiplier0 = calculateGlowInTheDark(_GITDEMinLight, _GITDEMaxLight, _GITDEMinEmissionMultiplier, _GITDEMaxEmissionMultiplier, _EnableGITDEmission);
        
        UNITY_BRANCH
        if (!_EmissionCenterOutEnabled)
        {
            emissionColor0 = POI2D_SAMPLER_PAN(_EmissionMap, _MainTex, poiMesh.uv[_EmissionMapUV], _EmissionMapPan) * lerp(1, baseColor, _EmissionBaseColorAsMap).rgb * _EmissionColor.rgb;
        }
        else
        {
            emissionColor0 = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionMap, _MainTex, ((.5 + poiLight.nDotV * .5) * _EmissionMap_ST.xy) + _Time.x * _EmissionCenterOutSpeed) * lerp(1, baseColor, _EmissionBaseColorAsMap).rgb * _EmissionColor.rgb;
        }
        
        UNITY_BRANCH
        if(_EmissionScrollingUseCurve)
        {
            emissionStrength0 *= lerp(1, UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionScrollingCurve, _MainTex, TRANSFORM_TEX(poiMesh.uv[_EmissionMapUV], _EmissionScrollingCurve) + (dot(poiMesh.localPos, _EmissiveScroll_Direction) * _EmissiveScroll_Interval) + _Time.x * _EmissiveScroll_Velocity), _ScrollingEmission);
        }
        else
        {
            emissionStrength0 *= calculateScrollingEmission(_EmissiveScroll_Direction, _EmissiveScroll_Velocity, _EmissiveScroll_Interval, _EmissiveScroll_Width, _ScrollingEmission, _EmissionScrollingOffset);
        }
        
        emissionStrength0 *= calculateBlinkingEmission(_EmissiveBlink_Min, _EmissiveBlink_Max, _EmissiveBlink_Velocity, _EmissionBlinkingOffset);
        emissionColor0 = hueShift(emissionColor0, _EmissionHueShift * _EmissionHueShiftEnabled);
        float emissionMask0 = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionMask, _MainTex, TRANSFORM_TEX(poiMesh.uv[_EmissionMaskUV], _EmissionMask) + _Time.x * _EmissionMaskPan);
        
        #ifdef POI_BLACKLIGHT
            if(_BlackLightMaskEmission != 4)
            {
                emissionMask0 *= blackLightMask[_BlackLightMaskEmission];
            }
        #endif
        
        emissionStrength0 *= glowInTheDarkMultiplier0 * emissionMask0;
        emission0 = emissionStrength0 * emissionColor0;
        
        #ifdef POI_DISSOLVE
            emission0 *= lerp(1 - dissolveAlpha, dissolveAlpha, _DissolveEmissionSide);
        #endif
        
        // Second Emission
        float3 emission1 = 0;
        float emissionStrength1 = 0;
        float3 emissionColor1 = 0;
        
        UNITY_BRANCH
        if (_EnableEmission1)
        {
            emissionStrength1 = _EmissionStrength1;
            float glowInTheDarkMultiplier1 = calculateGlowInTheDark(_GITDEMinLight1, _GITDEMaxLight1, _GITDEMinEmissionMultiplier1, _GITDEMaxEmissionMultiplier1, _EnableGITDEmission1);
            
            UNITY_BRANCH
            if(!_EmissionCenterOutEnabled1)
            {
                emissionColor1 = POI2D_SAMPLER_PAN(_EmissionMap1, _MainTex, poiMesh.uv[_EmissionMap1UV], _EmissionMap1Pan) * lerp(1, baseColor, _EmissionBaseColorAsMap1).rgb * _EmissionColor1.rgb;
            }
            else
            {
                emissionColor1 = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionMap1, _MainTex, ((.5 + poiLight.nDotV * .5) * _EmissionMap_ST.xy) + _Time.x * _EmissionCenterOutSpeed1).rgb * lerp(1, baseColor, _EmissionBaseColorAsMap1).rgb * _EmissionColor1.rgb;
            }
            
            UNITY_BRANCH
            if(_EmissionScrollingUseCurve1)
            {
                emissionStrength1 *= lerp(1, UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionScrollingCurve1, _MainTex, TRANSFORM_TEX(poiMesh.uv[_EmissionMap1UV], _EmissionScrollingCurve1) + (dot(poiMesh.localPos, _EmissiveScroll_Direction1) * _EmissiveScroll_Interval1) + _Time.x * _EmissiveScroll_Velocity1), _ScrollingEmission1);
            }
            else
            {
                emissionStrength1 *= calculateScrollingEmission(_EmissiveScroll_Direction1, _EmissiveScroll_Velocity1, _EmissiveScroll_Interval1, _EmissiveScroll_Width1, _ScrollingEmission1, _EmissionScrollingOffset1);
            }
            
            emissionStrength1 *= calculateBlinkingEmission(_EmissiveBlink_Min1, _EmissiveBlink_Max1, _EmissiveBlink_Velocity1, _EmissionBlinkingOffset1);
            emissionColor1 = hueShift(emissionColor1, _EmissionHueShift1 * _EmissionHueShiftEnabled1);
            float emissionMask1 = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionMask1, _MainTex, TRANSFORM_TEX(poiMesh.uv[_EmissionMask1UV], _EmissionMask1) + _Time.x * _EmissionMask1Pan);
            #ifdef POI_BLACKLIGHT
                if(_BlackLightMaskEmission2 != 4)
                {
                    emissionMask1 *= blackLightMask[_BlackLightMaskEmission2];
                }
            #endif
            emissionStrength1 *= glowInTheDarkMultiplier1 * emissionMask1;
            emission1 = emissionStrength1 * emissionColor1;
            
            #ifdef POI_DISSOLVE
                emission1 *= lerp(1 - dissolveAlpha, dissolveAlpha, _DissolveEmission1Side);
            #endif
        }
        
        finalColor.rgb = lerp(finalColor.rgb, saturate(emissionColor0 + emissionColor1), saturate(emissionStrength0 + emissionStrength1) * _EmissionReplace * poiMax(emission0 + emission1));
        
        return emission0 + emission1;
    }
    
#endif