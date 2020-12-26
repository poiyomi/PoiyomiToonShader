#ifndef POI_RIM
    #define POI_RIM
    
    float4 _RimLightColor;
    float _RimLightingInvert;
    float _RimWidth;
    float _RimStrength;
    float _RimSharpness;
    float _RimLightColorBias;
    float _ShadowMix;
    float _ShadowMixThreshold;
    float _ShadowMixWidthMod;
    float _EnableRimLighting;
    float _RimBrighten;
    uint _RimLightNormal;
    
    POI_TEXTURE_NOSAMPLER(_RimTex);
    POI_TEXTURE_NOSAMPLER(_RimMask);
    POI_TEXTURE_NOSAMPLER(_RimWidthNoiseTexture);

    float _RimWidthNoiseStrength;
    
    float4 rimColor = float4(0, 0, 0, 0);
    float rim = 0;
    
    void calculateRimLighting()
    {
        float rimNoise = POI2D_SAMPLER_PAN(_RimWidthNoiseTexture, _MainTex, poiMesh.uv[_RimWidthNoiseTextureUV], _RimWidthNoiseTexturePan);
        rimNoise = (rimNoise - .5) * _RimWidthNoiseStrength;

        float viewDotNormal = abs(dot(poiCam.viewDir, poiMesh.normals[_RimLightNormal]));
        UNITY_BRANCH
        if (_RimLightingInvert)
        {
            viewDotNormal = 1 - abs(dot(poiCam.viewDir, poiMesh.normals[_RimLightNormal]));
        }
        _RimWidth -= rimNoise;
        float rimMask = POI2D_SAMPLER_PAN(_RimMask, _MainTex, poiMesh.uv[_RimMaskUV], _RimMaskPan);
        rimColor = POI2D_SAMPLER_PAN(_RimTex, _MainTex, poiMesh.uv[_RimTexUV], _RimTexPan) * _RimLightColor;
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