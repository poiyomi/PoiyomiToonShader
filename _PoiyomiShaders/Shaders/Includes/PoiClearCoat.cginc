#ifndef POI_CLEARCOAT
    #define POI_CLEARCOAT
    
    samplerCUBE _ClearCoatCubeMap;
    float _ClearCoatSampleWorld;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_ClearCoatMask); float4 _ClearCoatMask_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_ClearCoatSmoothnessMask); float4 _ClearCoatSmoothnessMask_ST;
    float _ClearCoatInvertSmoothness;
    float _ClearCoat;
    float _ClearCoatSmoothness;
    float3 _ClearCoatTint;
    uint _ClearCoatNormalToUse;
    float _ClearCoatForceLighting;
    float lighty_clear_boy_uwu_var;


    float3 CalculateClearCoatEnvironmentalReflections()
    {
        float3 reflectionColor;

        float smoothnessMap = (UNITY_SAMPLE_TEX2D_SAMPLER(_ClearCoatSmoothnessMask, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _ClearCoatSmoothnessMask)));
        if (_ClearCoatInvertSmoothness == 1)
        {
            smoothnessMap = 1 - smoothnessMap;
        }
        smoothnessMap *= _ClearCoatSmoothness;
        float roughness = 1 - smoothnessMap;
        
        lighty_clear_boy_uwu_var = 0;

        float3 reflectedDir = _ClearCoatNormalToUse == 0 ? poiCam.vertexReflectionDir : poiCam.reflectionDir;
        
        float4 envSample = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectedDir, roughness * UNITY_SPECCUBE_LOD_STEPS);
        bool no_probe = unity_SpecCube0_HDR.a == 0 && envSample.a == 0;

        UNITY_BRANCH
        if(_ClearCoatSampleWorld == 0 && no_probe == 0)
        {
            
            Unity_GlossyEnvironmentData envData;
            envData.roughness = roughness;
            envData.reflUVW = BoxProjection(
                reflectedDir, poiMesh.worldPos.xyz,
                unity_SpecCube0_ProbePosition,
                unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax
            );
            float3 probe0 = Unity_GlossyEnvironment(
                UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData
            );
            envData.reflUVW = BoxProjection(
                reflectedDir, poiMesh.worldPos.xyz,
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
                reflectionColor = lerp(probe1, probe0, interpolator);
            }
            else
            {
                reflectionColor = probe0;
            }
        }
        else
        {
            lighty_clear_boy_uwu_var = 1;
            reflectionColor = texCUBElod(_ClearCoatCubeMap, float4(reflectedDir, roughness * UNITY_SPECCUBE_LOD_STEPS));
        }

        if(_ClearCoatForceLighting)
        {
            lighty_clear_boy_uwu_var = 1;
        }

        return reflectionColor * _ClearCoatTint;
    }
    
    void calculateAndApplyClearCoat(inout float4 finalColor)
    {
        half clearCoatMap = UNITY_SAMPLE_TEX2D_SAMPLER(_ClearCoatMask, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _ClearCoatMask));
        half3 reflectionColor = CalculateClearCoatEnvironmentalReflections();
        
        float NormalDotView = abs(dot(_ClearCoat,_ClearCoatNormalToUse == 0 ? poiLight.vNDotV : poiLight.nDotV).r);
        #ifdef POI_LIGHTING
            finalColor.rgb = lerp(finalColor.rgb, reflectionColor * lerp(1, poiLight.finalLighting, lighty_clear_boy_uwu_var), clearCoatMap * _ClearCoat * clamp(FresnelTerm(_ClearCoat, NormalDotView),0,1));
            //finalColor.rgb += reflectionColor;
            //finalColor.rgb = finalColor.rgb * (1- (reflectionColor.r + reflectionColor.g + reflectionColor.b)/3) + reflectionColor * clearCoatMap * lerp(1, poiLight.finalLighting, lighty_clear_boy_uwu_var);
        #else
            lerp(finalColor.rgb, reflectionColor, clearCoatMap * _ClearCoat * clamp(FresnelTerm(_ClearCoat, NormalDotView),0,1));
        #endif
    }
    
#endif