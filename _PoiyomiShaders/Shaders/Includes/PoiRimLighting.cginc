#ifndef POI_RIM
    #define POI_RIM
    
    float4 _RimLightColor;
    float _RimLightingInvert;
    float _RimWidth;
    float _RimStrength;
    float _RimSharpness;
    float _RimLightColorBias;
    float4 _RimTexPanSpeed;
    float _ShadowMix;
    float _ShadowMixThreshold;
    float _ShadowMixWidthMod;
    float _EnableRimLighting;
    float _RimBrighten;
    uint _RimLightNormal;
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_RimTex); float4 _RimTex_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_RimMask); float4 _RimMask_ST;
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_RimWidthNoiseTexture); float4 _RimWidthNoiseTexture_ST;
    float _RimWidthNoiseStrength;
    float4 _RimWidthNoisePan;
    
    float4 rimColor = float4(0, 0, 0, 0);
    float rim = 0;
    
    void calculateRimLighting()
    {
        _RimWidthNoiseTexture_ST.zw += _Time.y * _RimWidthNoisePan.xy;
        float rimNoise = UNITY_SAMPLE_TEX2D_SAMPLER(_RimWidthNoiseTexture, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _RimWidthNoiseTexture));
        rimNoise = (rimNoise - .5) * _RimWidthNoiseStrength;

        float viewDotNormal = abs(dot(poiCam.viewDir, poiMesh.normals[_RimLightNormal]));
        UNITY_BRANCH
        if (_RimLightingInvert)
        {
            viewDotNormal = 1 - poiCam.viewDotNormal;
        }
        _RimWidth -= rimNoise;
        float rimMask = UNITY_SAMPLE_TEX2D_SAMPLER(_RimMask, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _RimMask));
        rimColor = UNITY_SAMPLE_TEX2D_SAMPLER(_RimTex, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _RimTex) + _Time.y * _RimTexPanSpeed.xy) * _RimLightColor;
        _RimWidth = lerp(_RimWidth, _RimWidth * lerp(0, 1, poiLight.lightMap - _ShadowMixThreshold) * _ShadowMixWidthMod, _ShadowMix);
        rim = 1 - smoothstep(min(_RimSharpness, _RimWidth), _RimWidth, viewDotNormal);
        rim *= _RimLightColor.a * rimColor.a * rimMask;
    }
    
    void applyRimColor(inout float4 finalColor)
    {
        finalColor.rgb = lerp(finalColor.rgb, lerp(finalColor.rgb, rimColor, _RimLightColorBias) + lerp(finalColor.rgb, rimColor, _RimLightColorBias) * _RimBrighten, rim);
    }
    void ApplyRimEmission(inout float3 finalEmission)
    {
        finalEmission += rim * lerp(finalColor.rgb, rimColor, _RimLightColorBias) * _RimStrength;
    }
#endif