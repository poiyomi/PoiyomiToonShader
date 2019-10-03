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
    
    //enviro rim
    float _EnableEnvironmentalRim;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_RimEnviroMask); float4 _RimEnviroMask_ST;
    float _RimEnviroBlur;
    float _RimEnviroMinBrightness;
    float _RimEnviroWidth;
    float _RimEnviroSharpness;
    
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
        UNITY_BRANCH
        if (_RimLightingInvert)
        {
            poiCam.viewDotNormal = 1 - poiCam.viewDotNormal;
        }
        _RimWidth -= rimNoise;
        float rimMask = UNITY_SAMPLE_TEX2D_SAMPLER(_RimMask, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _RimMask));
        rimColor = UNITY_SAMPLE_TEX2D_SAMPLER(_RimTex, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _RimTex) + _Time.y * _RimTexPanSpeed.xy) * _RimLightColor;
        _RimWidth = lerp(_RimWidth, _RimWidth * lerp(0, 1, poiLight.lightMap - _ShadowMixThreshold) * _ShadowMixWidthMod, _ShadowMix);
        rim = 1 - smoothstep(min(_RimSharpness, _RimWidth), _RimWidth, poiCam.viewDotNormal);
        rim *= _RimLightColor.a * rimColor.a * rimMask;
    }
    
    void applyRimColor(inout float4 finalColor)
    {
        finalColor.rgb = lerp(finalColor.rgb, lerp(finalColor.rgb, rimColor, _RimLightColorBias) + lerp(finalColor.rgb, rimColor, _RimLightColorBias) * _RimBrighten, rim);
    }
    void ApplyRimEmission(inout float4 finalColor)
    {
        finalColor.rgb += rim * lerp(finalColor.rgb, rimColor, _RimLightColorBias) * _RimStrength;
    }
    
    void applyEnviroRim(inout float4 finalColor)
    {
        UNITY_BRANCH
        if(_EnableEnvironmentalRim)
        {
            float enviroRimAlpha = saturate(1 - smoothstep(min(_RimEnviroSharpness, _RimEnviroWidth), _RimEnviroWidth, poiCam.viewDotNormal));
            _RimEnviroBlur *= 1.7 - 0.7 * _RimEnviroBlur;
            
            float3 enviroRimColor = 0;
            float interpolator = unity_SpecCube0_BoxMin.w;
            UNITY_BRANCH
            if(interpolator < 0.99999)
            {
                //Probe 1
                float4 reflectionData0 = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, poiMesh.fragmentNormal, _RimEnviroBlur * UNITY_SPECCUBE_LOD_STEPS);
                float3 reflectionColor0 = DecodeHDR(reflectionData0, unity_SpecCube0_HDR);
                
                //Probe 2
                float4 reflectionData1 = UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(unity_SpecCube1, unity_SpecCube0, poiMesh.fragmentNormal, _RimEnviroBlur * UNITY_SPECCUBE_LOD_STEPS);
                float3 reflectionColor1 = DecodeHDR(reflectionData1, unity_SpecCube1_HDR);
                
                enviroRimColor = lerp(reflectionColor1, reflectionColor0, interpolator);
            }
            else
            {
                float4 reflectionData = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, poiMesh.fragmentNormal, _RimEnviroBlur * UNITY_SPECCUBE_LOD_STEPS);
                enviroRimColor = DecodeHDR(reflectionData, unity_SpecCube0_HDR);
            }
            
            half enviroMask = poiMax(UNITY_SAMPLE_TEX2D_SAMPLER(_RimEnviroMask, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _RimEnviroMask)).rgb);
            finalColor.rgb += lerp(0, max(0, (enviroRimColor - _RimEnviroMinBrightness) * albedo.rgb), enviroRimAlpha).rgb * enviroMask;
        }
    }
#endif