#ifndef POI_CLEARCOAT
    #define POI_CLEARCOAT
    
    samplerCUBE _ClearCoatCubeMap;
    float _ClearCoatSampleWorld;
    sampler2D _ClearCoatMask; float4 _ClearCoatMask_ST;
    sampler2D _ClearCoatSmoothnessMask; float4 _ClearCoatSmoothnessMask_ST;
    float _ClearCoatInvertSmoothness;
    float _ClearCoat;
    float _ClearCoatSmoothness;
    float3 _ClearCoatTint;
    uint _ClearCoatNormalToUse;
    
    float lighty_clear_boy_uwu_var;
    half3 calculateClearCoatRelfection()
    {
        float _Smoothness_map_var = (tex2D(_ClearCoatSmoothnessMask, TRANSFORM_TEX(poiMesh.uv[0], _ClearCoatSmoothnessMask)));
        if (_ClearCoatInvertSmoothness == 1)
        {
            _Smoothness_map_var = 1 - _Smoothness_map_var;
        }
        _Smoothness_map_var *= _ClearCoatSmoothness;
        half roughness = 1 - _Smoothness_map_var;
        roughness *= 1.7 - 0.7 * roughness;
        float3 reflectedDir = _ClearCoatNormalToUse == 0 ? poiCam.vertexReflectionDir : poiCam.reflectionDir;
        
        float4 envSample = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectedDir, roughness * UNITY_SPECCUBE_LOD_STEPS);
        
        float interpolator = unity_SpecCube0_BoxMin.w;
        half3 reflection = 0;
        UNITY_BRANCH
        if(interpolator < 0.99999)
        {
            //Probe 1
            float4 reflectionData0 = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectedDir, roughness * UNITY_SPECCUBE_LOD_STEPS);
            float3 reflectionColor0 = DecodeHDR(reflectionData0, unity_SpecCube0_HDR);
            
            //Probe 2
            float4 reflectionData1 = UNITY_SAMPLE_TEXCUBE_SAMPLER_LOD(unity_SpecCube1, unity_SpecCube0, reflectedDir, roughness * UNITY_SPECCUBE_LOD_STEPS);
            float3 reflectionColor1 = DecodeHDR(reflectionData1, unity_SpecCube1_HDR);
            
            reflection = lerp(reflectionColor1, reflectionColor0, interpolator);
        }
        else
        {
            float4 reflectionData = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectedDir, roughness * UNITY_SPECCUBE_LOD_STEPS);
            reflection = DecodeHDR(reflectionData, unity_SpecCube0_HDR);
        }
        
        bool no_probe = unity_SpecCube0_HDR.a == 0 && envSample.a == 0;
        lighty_clear_boy_uwu_var = 0;
        if (no_probe || _ClearCoatSampleWorld)
        {
            lighty_clear_boy_uwu_var = 1;
            reflection = texCUBElod(_ClearCoatCubeMap, float4(reflectedDir, roughness * UNITY_SPECCUBE_LOD_STEPS));
        }
        
        half3 finalreflection = reflection.rgb * _ClearCoatTint;
        
        return finalreflection;
    }
    
    void calculateAndApplyClearCoat(inout float4 finalColor)
    {
        half clearCoatMap = tex2D(_ClearCoatMask, TRANSFORM_TEX(poiMesh.uv[0], _ClearCoatMask));
        half3 reflectionColor = calculateClearCoatRelfection();
        
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