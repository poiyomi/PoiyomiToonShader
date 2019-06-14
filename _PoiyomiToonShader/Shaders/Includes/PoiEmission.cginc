#ifndef EMISSION
    #define EMISSION
    
    float4 _EmissionColor;
    sampler2D _EmissionMap; float4 _EmissionMap_ST;
    sampler2D _EmissionMask; float4 _EmissionMask_ST;
    float _EmissionStrength;
    
    float4 _EmissiveScroll_Direction;
    float4 _EmissionScrollSpeed;
    float _EmissiveScroll_Width;
    float _EmissiveScroll_Velocity;
    float _EmissiveScroll_Interval;
    float _EmissiveBlink_Min;
    float _EmissiveBlink_Max;
    float _EmissiveBlink_Velocity;
    float _ScrollingEmission;
    
    float4 emission = 0;
    
    void calculateEmission(float2 uv, float3 localPos)
    {
        float4 _Emissive_Tex_var = tex2D(_EmissionMap, TRANSFORM_TEX(uv, _EmissionMap) + _Time.y * _EmissionScrollSpeed);
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
        
        float _Emission_mask_var = tex2D(_EmissionMask, TRANSFORM_TEX(uv, _EmissionMask));
        emission *= _Emission_mask_var;
    }
    
    void applyEmission(inout float4 finalColor)
    {
        finalColor.rgb += emission;
    }
#endif