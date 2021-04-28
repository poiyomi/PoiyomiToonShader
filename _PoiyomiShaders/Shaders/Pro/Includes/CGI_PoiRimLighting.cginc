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
    float _RimLightNormal;
    float _RimHueShiftEnabled;
    float _RimHueShiftSpeed;
    float _RimHueShift;
    
    #if defined(PROP_RIMTEX) || !defined(OPTIMIZER_ENABLED)
        POI_TEXTURE_NOSAMPLER(_RimTex);
    #endif
    #if defined(PROP_RIMMASK) || !defined(OPTIMIZER_ENABLED)
        POI_TEXTURE_NOSAMPLER(_RimMask);
    #endif
    #if defined(PROP_RIMWIDTHNOISETEXTURE) || !defined(OPTIMIZER_ENABLED)
        POI_TEXTURE_NOSAMPLER(_RimWidthNoiseTexture);
    #endif
    
    float _RimWidthNoiseStrength;
    
    float4 rimColor = float4(0, 0, 0, 0);
    float rim = 0;
    
    void applyRimLighting(inout float4 albedo, inout float3 rimLightEmission)
    {
        #if defined(PROP_RIMWIDTHNOISETEXTURE) || !defined(OPTIMIZER_ENABLED)
            float rimNoise = POI2D_SAMPLER_PAN(_RimWidthNoiseTexture, _MainTex, poiMesh.uv[_RimWidthNoiseTextureUV], _RimWidthNoiseTexturePan);
        #else
            float rimNoise = 0;
        #endif
        rimNoise = (rimNoise - .5) * _RimWidthNoiseStrength;
        
        float viewDotNormal = abs(dot(poiCam.viewDir, poiMesh.normals[_RimLightNormal]));
        UNITY_BRANCH
        if (_RimLightingInvert)
        {
            viewDotNormal = 1 - abs(dot(poiCam.viewDir, poiMesh.normals[_RimLightNormal]));
        }
        float rimWidth = _RimWidth;
        rimWidth -= rimNoise;
        #if defined(PROP_RIMMASK) || !defined(OPTIMIZER_ENABLED)
            float rimMask = POI2D_SAMPLER_PAN(_RimMask, _MainTex, poiMesh.uv[_RimMaskUV], _RimMaskPan);
        #else
            float rimMask = 1;
        #endif
        
        #if defined(PROP_RIMTEX) || !defined(OPTIMIZER_ENABLED)
            rimColor = POI2D_SAMPLER_PAN(_RimTex, _MainTex, poiMesh.uv[_RimTexUV], _RimTexPan) * _RimLightColor;
        #else
            rimColor = _RimLightColor;
        #endif
        
        UNITY_BRANCH
        if(_RimHueShiftEnabled)
        {
            rimColor.rgb = hueShift(rimColor.rgb, _RimHueShift + _Time.x * _RimHueShiftSpeed);
        }
        
        rimWidth = max(lerp(rimWidth, rimWidth * lerp(0, 1, poiLight.lightMap - _ShadowMixThreshold) * _ShadowMixWidthMod, _ShadowMix), 0);
        rim = 1 - smoothstep(min(_RimSharpness, rimWidth), rimWidth, viewDotNormal);
        rim *= _RimLightColor.a * rimColor.a * rimMask;
        rimLightEmission = rim * lerp(albedo, rimColor, _RimLightColorBias) * _RimStrength;
        albedo.rgb = lerp(albedo.rgb, lerp(albedo.rgb, rimColor, _RimLightColorBias) + lerp(albedo.rgb, rimColor, _RimLightColorBias) * _RimBrighten, rim);
    }
#endif