#ifndef POI_METAL
    #define POI_METAL
    
    samplerCUBE _CubeMap;
    float _SampleWorld;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_MetallicMask); float4 _MetallicMask_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_SmoothnessMask); float4 _SmoothnessMask_ST;
    float _Metallic;
    float _InvertSmoothness;
    float _Smoothness;
    float _EnableMetallic;
    float3 _MetalReflectionTint;
    
    float3 finalreflections;
    float metalicMap;
    float3 reflection;
    float roughness;
    float lighty_boy_uwu_var;
    
    void CalculateEnvironmentalReflections(inout float lightingAlpha, inout float bakedCubemap)
    {
        metalicMap = UNITY_SAMPLE_TEX2D_SAMPLER(_MetallicMask, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _MetallicMask)) * _Metallic;
        float smoothnessMap = (UNITY_SAMPLE_TEX2D_SAMPLER(_SmoothnessMask, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _SmoothnessMask)));
        if (_InvertSmoothness == 1)
        {
            smoothnessMap = 1 - smoothnessMap;
        }
        smoothnessMap *= _Smoothness;
        roughness = 1 - smoothnessMap;
        
        lighty_boy_uwu_var = 0;
        
        float4 envSample = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, poiCam.reflectionDir, roughness * UNITY_SPECCUBE_LOD_STEPS);
        bool no_probe = unity_SpecCube0_HDR.a == 0 && envSample.a == 0;

        UNITY_BRANCH
        if(_SampleWorld == 0 && no_probe == 0)
        {
            
            Unity_GlossyEnvironmentData envData;
            envData.roughness = roughness;
            envData.reflUVW = BoxProjection(
                poiCam.reflectionDir, poiMesh.worldPos.xyz,
                unity_SpecCube0_ProbePosition,
                unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax
            );
            float3 probe0 = Unity_GlossyEnvironment(
                UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData
            );
            envData.reflUVW = BoxProjection(
                poiCam.reflectionDir, poiMesh.worldPos.xyz,
                unity_SpecCube1_ProbePosition,
                unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax
            );
            
            float interpolator = unity_SpecCube0_BoxMin.w;
            UNITY_BRANCH
            if(interpolator < 0.99999)
            {
                float3 probe1 = Unity_GlossyEnvironment(
                    UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1, unity_SpecCube0),
                    unity_SpecCube0_HDR, envData
                );
                reflection = lerp(probe1, probe0, interpolator);
            }
            else
            {
                reflection = probe0;
            }
            bakedCubemap = 0;
        }
        else
        {
            lighty_boy_uwu_var = 1;
            reflection = texCUBElod(_CubeMap, float4(poiCam.reflectionDir, roughness * UNITY_SPECCUBE_LOD_STEPS));
            bakedCubemap = 1;
        }
            lightingAlpha = (1 - metalicMap);
    }
    
    void applyReflections(inout float4 finalColor, float4 finalColorBeforeLighting)
    {
        #ifdef FORWARD_BASE_PASS
            finalreflections = reflection.rgb * finalColorBeforeLighting.rgb * _MetalReflectionTint;
            #ifdef POI_LIGHTING
                finalColor.rgb += (finalreflections * ((1 - roughness + metalicMap) / 2));
            #else
                finalColor.rgb += (finalreflections * ((1 - roughness + metalicMap) / 2));
            #endif
        #endif
    }
    
    void applyAdditiveReflectiveLighting(inout float4 finalColor)
    {
        finalColor.rgb *= (1 - metalicMap);
    }
#endif