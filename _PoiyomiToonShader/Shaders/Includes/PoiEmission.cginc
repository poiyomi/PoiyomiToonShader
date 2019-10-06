#ifndef POI_EMISSION
    #define POI_EMISSION
    
    float4 _EmissionColor;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_EmissionMap); float4 _EmissionMap_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_EmissionMask); float4 _EmissionMask_ST;
    float _EmissionStrength;
    float _EnableEmission;
    float4 _EmissiveScroll_Direction;
    float4 _EmissionPan;
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
    uint _EmissionUV;
    float4 emission = 0;
    float _EmissionCenterOutEnabled;
    float _EmissionCenterOutSpeed;
    
    void calculateEmission()
    {
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
            float4 _Emissive_Tex_var = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionMap, _MainTex, TRANSFORM_TEX(poiMesh.uv[_EmissionUV], _EmissionMap) + _Time.y * _EmissionPan.xy);
            emission = _Emissive_Tex_var * _EmissionColor * _EmissionStrength;
        }
        
        UNITY_BRANCH
        if(_EmissionCenterOutEnabled)
        {
            emission = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionMap, _MainTex, (.5 + poiLight.nDotV * .5) + _Time.x * _EmissionCenterOutSpeed) * _EmissionColor * _EmissionStrength;
        }
        
        
        // scrolling emission
        if (_ScrollingEmission == 1)
        {
            float phase = dot(poiMesh.localPos, _EmissiveScroll_Direction);
            phase -= _Time.y * _EmissiveScroll_Velocity;
            phase /= _EmissiveScroll_Interval;
            phase -= floor(phase);
            float width = _EmissiveScroll_Width;
            phase = (pow(phase, width) + pow(1 - phase, width * 4)) * 0.5;
            emission *= phase;
        }
        
        // blinking emission
        float amplitude = (_EmissiveBlink_Max - _EmissiveBlink_Min) * 0.5f;
        float base = _EmissiveBlink_Min + amplitude;
        float emissiveBlink = sin(_Time.y * _EmissiveBlink_Velocity) * amplitude + base;
        emission *= emissiveBlink;
        
        float _Emission_mask_var = UNITY_SAMPLE_TEX2D_SAMPLER(_EmissionMask, _MainTex, TRANSFORM_TEX(poiMesh.uv[_EmissionUV], _EmissionMask) + _Time.x * _EmissionPan.zw);
        emission *= _Emission_mask_var;
    }
    
    void applyEmission(inout float3 finalEmission)
    {
        finalEmission += emission;
    }
#endif