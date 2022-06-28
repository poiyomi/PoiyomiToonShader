#ifndef POI_ENVIRONMENTAL_RIM
    #define POI_ENVIRONMENTAL_RIM
    
    //enviro rim
    float _EnableEnvironmentalRim;
    float _RimEnviroBlur;
    float _RimEnviroMinBrightness;
    float _RimEnviroWidth;
    float _RimEnviroSharpness;
    float _RimEnviroIntensity;
    #if defined(PROP_RIMENVIROMASK) || !defined(OPTIMIZER_ENABLED)
        POI_TEXTURE_NOSAMPLER(_RimEnviroMask);
    #endif
    
    float3 calculateEnvironmentalRimLighting(in float4 albedo)
    {
        float enviroRimAlpha = saturate(1 - smoothstep(min(_RimEnviroSharpness, _RimEnviroWidth), _RimEnviroWidth, poiCam.viewDotNormal));
        _RimEnviroBlur *= 1.7 - 0.7 * _RimEnviroBlur;
        
        float3 enviroRimColor = 0;
        float interpolator = unity_SpecCube0_BoxMin.w;
        UNITY_BRANCH
        if (interpolator < 0.99999)
        {
            //Probe 1
            float4 reflectionData0 = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, poiMesh.normals[1], _RimEnviroBlur * UNITY_SPECCUBE_LOD_STEPS);
            float3 reflectionColor0 = DecodeHDR(reflectionData0, unity_SpecCube0_HDR);
            
            //Probe 2
            float4 reflectionData1 = UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(unity_SpecCube1, unity_SpecCube0, poiMesh.normals[1], _RimEnviroBlur * UNITY_SPECCUBE_LOD_STEPS);
            float3 reflectionColor1 = DecodeHDR(reflectionData1, unity_SpecCube1_HDR);
            
            enviroRimColor = lerp(reflectionColor1, reflectionColor0, interpolator);
        }
        else
        {
            float4 reflectionData = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, poiMesh.normals[1], _RimEnviroBlur * UNITY_SPECCUBE_LOD_STEPS);
            enviroRimColor = DecodeHDR(reflectionData, unity_SpecCube0_HDR);
        }
        #if defined(PROP_RIMENVIROMASK) || !defined(OPTIMIZER_ENABLED)
            half enviroMask = poiMax(POI2D_SAMPLER_PAN(_RimEnviroMask, _MainTex, poiMesh.uv[_RimEnviroMaskUV], _RimEnviroMaskPan).rgb);
        #else
            half enviroMask = 1;
        #endif
        return lerp(0, max(0, (enviroRimColor - _RimEnviroMinBrightness) * albedo.rgb), enviroRimAlpha).rgb * enviroMask * _RimEnviroIntensity;
    }
    
#endif