#ifndef EMISSION
    #define EMISSION
    
    float4 _EmissionColor;
    sampler2D _EmissionMap; float4 _EmissionMap_ST;
    sampler2D _EmissionMask; float4 _EmissionMask_ST;
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
    float4 emission = 0;
    
    void calculateEmission(float2 uv, float3 localPos)
    {
        UNITY_BRANCH
        if (_EnableEmission)
        {
            
            #ifdef LIGHTING
                UNITY_BRANCH
                if(_EnableGITDEmission != 0)
                {
                    float3 lightValue = _GITDEWorldOrMesh ? poiLight.finalLighting.rgb : poiLight.directLighting.rgb;
                    float gitdeAlpha = (clamp(poiMax(lightValue), _GITDEMinLight, _GITDEMaxLight) - _GITDEMinLight)/(_GITDEMaxLight - _GITDEMinLight);
                    _EmissionStrength *= lerp(_GITDEMinEmissionMultiplier, _GITDEMaxEmissionMultiplier, gitdeAlpha);
                }
            #endif

            float4 _Emissive_Tex_var = tex2D(_EmissionMap, TRANSFORM_TEX(uv, _EmissionMap) + _Time.y * _EmissionPan.xy);
            emission = _Emissive_Tex_var * _EmissionColor * _EmissionStrength;
            
            // scrolling emission
            if (_ScrollingEmission == 1)
            {
                float phase = dot(localPos, _EmissiveScroll_Direction);
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
            
            float _Emission_mask_var = tex2D(_EmissionMask, TRANSFORM_TEX(uv, _EmissionMask) + _Time.y * _EmissionPan.zw);
            
            
            
            emission *= _Emission_mask_var;
        }
    }
    void applyEmission(inout float4 finalColor)
    {
        UNITY_BRANCH
        if (_EnableEmission)
        {
            finalColor.rgb += emission;
        }
    }
#endif