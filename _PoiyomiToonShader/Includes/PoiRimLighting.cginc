#ifndef RIM_LIGHTING
    #define RIM_LIGHTING
    
    float4 _RimLightColor;
    float _RimWidth;
    float _RimStrength;
    float _RimSharpness;
    float _RimLightColorBias;
    float4 _RimTexPanSpeed;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_RimTex); float4 _RimTex_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_RimMask); float4 _RimMask_ST;
    
    float4 rimColor = float4(0, 0, 0, 0);
    float rim = 0;
    
    void calculateRimLighting(float2 uv, float cameraDotVert)
    {
        float rimMask = UNITY_SAMPLE_TEX2D_SAMPLER(_RimMask, _MainTex, TRANSFORM_TEX(uv, _RimMask));
        rimColor = UNITY_SAMPLE_TEX2D_SAMPLER(_RimTex, _MainTex, TRANSFORM_TEX(uv, _RimTex) + (_Time.y * _RimTexPanSpeed.xy)) * _RimLightColor;
        rim = pow((1 - cameraDotVert), (1 - _RimWidth) * 10);
        _RimSharpness /= 2;
        rim = (smoothstep(_RimSharpness, 1 - _RimSharpness, rim));
        rim *= _RimLightColor.a * rimColor.a * rimMask;
    }
    
    void applyRimColor(inout float4 finalColor)
    {
        finalColor.rgb = lerp(finalColor.rgb, lerp(finalColor.rgb, rimColor, _RimLightColorBias), rim);
    }
    
    void ApplyRimEmission(inout float4 finalColor)
    {
        finalColor.rgb += rim * rimColor * _RimStrength;
    }
    
#endif