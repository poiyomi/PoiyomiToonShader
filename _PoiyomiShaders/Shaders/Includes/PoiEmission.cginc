#ifndef POI_EMISSION
    #define POI_EMISSION
    
    float4 _EmissionColor;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_EmissionMap); float4 _EmissionMap_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_EmissionMask); float4 _EmissionMask_ST;
    float _EmissionStrength;
    float _EnableEmission;
    float4 _EmissiveScroll_Direction;
    float2 _EmissionMapPan;
    float2 _EmissionMaskPan;
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
    uint _EmissionMapUV, _EmissionMaskUV;
    float _EmissionCenterOutEnabled;
    float _EmissionCenterOutSpeed;
    
    float4 _EmissionColor1;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_EmissionMap1); float4 _EmissionMap1_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_EmissionMask1); float4 _EmissionMask1_ST;
    float _EmissionStrength1;
    float _EnableEmission1;
    float4 _EmissiveScroll_Direction1;
    float2 _EmissionMapPan1;
    float2 _EmissionMaskPan1;
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
    uint _EmissionMapUV1, _EmissionMaskUV1;
    float _EmissionCenterOutEnabled1;
    float _EmissionCenterOutSpeed1;
    
    float _EmissionScrollingUseCurve;
    float _EmissionScrollingUseCurve1;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_EmissionScrollingCurve); float4 _EmissionScrollingCurve_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_EmissionScrollingCurve1); float4 _EmissionScrollingCurve1_ST;
    
    float3 calculateEmission()
    {
        float3 emission = 0;
        #ifdef POI_LIGHTING
            UNITY_BRANCH
            if (_EnableGITDEmission != 0)
            {
                float3 lightValue = _GITDEWorldOrMesh ? poiLight.finalLighting.rgb: poiLight.directLighting.rgb;
                float gitdeAlpha = (clamp(poiMax(lightValue), _GITDEMinLight, _GITDEMaxLight) - _GITDEMinLight) / (_GITDEMaxLight - _GITDEMinLight);
                _EmissionStrength *= lerp(_GITDEMinEmissionMultiplier, _GITDEMaxEmissionMultiplier, gitdeAlpha);
            }
        #endif
        
        UNITY_BRANCH
        if(!_EmissionCenterOutEnabled)
        {
            float4 _Emissive_Tex_var = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionMap, _MainTex, TRANSFORM_TEX(poiMesh.uv[_EmissionMapUV], _EmissionMap) + _Time.y * _EmissionMapPan);
            emission = _Emissive_Tex_var * _EmissionColor * _EmissionStrength;
        }
        
        UNITY_BRANCH
        if(_EmissionCenterOutEnabled)
        {
            emission = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionMap, _MainTex, ((.5 + poiLight.nDotV * .5) * _EmissionMap_ST.xy) + _Time.x * _EmissionCenterOutSpeed) * _EmissionColor * _EmissionStrength;
        }
        
        // scrolling emission
        if (_ScrollingEmission == 1)
        {
            float phase = 0;
            UNITY_BRANCH
            if(_EmissionScrollingUseCurve)
            {
                phase = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionScrollingCurve, _MainTex, TRANSFORM_TEX(poiMesh.uv[_EmissionMapUV], _EmissionScrollingCurve) + (dot(poiMesh.localPos, _EmissiveScroll_Direction) * _EmissiveScroll_Interval) + _Time.x * _EmissiveScroll_Velocity);
            }
            else
            {
                phase = dot(poiMesh.localPos, _EmissiveScroll_Direction);
                phase -= _Time.y * _EmissiveScroll_Velocity;
                phase /= _EmissiveScroll_Interval;
                phase -= floor(phase);
                float width = _EmissiveScroll_Width;
                phase = (pow(phase, width) + pow(1 - phase, width * 4)) * 0.5;
            }
            emission *= phase;
        }
        #ifndef SIMPLE
            // blinking emission
            float amplitude = (_EmissiveBlink_Max - _EmissiveBlink_Min) * 0.5f;
            float base = _EmissiveBlink_Min + amplitude;
            float emissiveBlink = sin(_Time.y * _EmissiveBlink_Velocity) * amplitude + base;
            emission *= emissiveBlink;
        #endif
        
        float _Emission_mask_var = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionMask, _MainTex, TRANSFORM_TEX(poiMesh.uv[_EmissionMaskUV], _EmissionMask) + _Time.x * _EmissionMaskPan);
        return emission * _Emission_mask_var;
    }
    
    float3 calculateEmission1()
    {
        float3 emission = 0;
        #ifdef POI_LIGHTING
            UNITY_BRANCH
            if (_EnableGITDEmission1 != 0)
            {
                float3 lightValue = _GITDEWorldOrMesh1 ? poiLight.finalLighting.rgb: poiLight.directLighting.rgb;
                float gitdeAlpha = (clamp(poiMax(lightValue), _GITDEMinLight1, _GITDEMaxLight1) - _GITDEMinLight1) / (_GITDEMaxLight1 - _GITDEMinLight1);
                _EmissionStrength1 *= lerp(_GITDEMinEmissionMultiplier1, _GITDEMaxEmissionMultiplier1, gitdeAlpha);
            }
        #endif
        
        UNITY_BRANCH
        if(!_EmissionCenterOutEnabled1)
        {
            float4 _Emissive_Tex_var1 = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionMap1, _MainTex, TRANSFORM_TEX(poiMesh.uv[_EmissionMapUV1], _EmissionMap1) + _Time.y * _EmissionMapPan1);
            emission = _Emissive_Tex_var1 * _EmissionColor1 * _EmissionStrength1;
        }
        
        UNITY_BRANCH
        if(_EmissionCenterOutEnabled1)
        {
            emission = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionMap1, _MainTex, ((.5 + poiLight.nDotV * .5) * _EmissionMap1_ST.xy) + _Time.x * _EmissionCenterOutSpeed1) * _EmissionColor1 * _EmissionStrength1;
        }
        
        // scrolling emission
        if (_ScrollingEmission1 == 1)
        {
            float phase = dot(poiMesh.localPos, _EmissiveScroll_Direction1);
            phase -= _Time.y * _EmissiveScroll_Velocity1;
            phase /= _EmissiveScroll_Interval1;
            phase -= floor(phase);
            float width = _EmissiveScroll_Width1;
            phase = (pow(phase, width) + pow(1 - phase, width * 4)) * 0.5;
            emission *= phase;
        }
        
        // blinking emission
        float amplitude = (_EmissiveBlink_Max1 - _EmissiveBlink_Min1) * 0.5f;
        float base = _EmissiveBlink_Min1 + amplitude;
        float emissiveBlink = sin(_Time.y * _EmissiveBlink_Velocity1) * amplitude + base;
        emission *= emissiveBlink;
        
        
        float _Emission_mask_var = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionMask1, _MainTex, TRANSFORM_TEX(poiMesh.uv[_EmissionMaskUV1], _EmissionMask1) + _Time.x * _EmissionMaskPan1);
        return emission * _Emission_mask_var;
    }
    
    void applyEmission(inout float3 finalEmission)
    {
        float3 emission = 0;
        float3 emission1 = 0;
        
        emission = calculateEmission();
        if (_EnableEmission1)
        {
            emission1 = calculateEmission1();
        }
        
        finalEmission += emission + emission1;
    }
#endif