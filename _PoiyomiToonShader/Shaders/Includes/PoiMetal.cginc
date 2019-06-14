#ifndef METAL
    #define METAL
    
    samplerCUBE _CubeMap;
    float _SampleWorld;
    float _PurelyAdditive;
    sampler2D _MetallicMap; float4 _MetallicMap_ST;
    float _Metallic;
    sampler2D _SmoothnessMap; float4 _SmoothnessMap_ST;
    float _InvertSmoothness;
    float _Smoothness;
    
    float3 finalreflections;
    float metalicMap;
    float3 reflection;
    float roughness;
    float lighty_boy_uwu_var;
    
    void calculateReflections(float2 uv, float3 normal, float3 cameraToVert)
    {
        metalicMap = tex2D(_MetallicMap, TRANSFORM_TEX(uv, _MetallicMap)) * _Metallic;
        float _Smoothness_map_var = (tex2D(_SmoothnessMap, TRANSFORM_TEX(uv, _SmoothnessMap)));
        if (_InvertSmoothness == 1)
        {
            _Smoothness_map_var = 1 - _Smoothness_map_var;
        }
        _Smoothness_map_var *= _Smoothness;
        roughness = 1 - _Smoothness_map_var;
        roughness *= 1.7 - 0.7 * roughness;
        float3 reflectedDir = reflect(-cameraToVert, normal);
        
        float4 envSample = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectedDir, roughness * UNITY_SPECCUBE_LOD_STEPS);
        
        float interpolator = unity_SpecCube0_BoxMin.w;
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
            lighty_boy_uwu_var = 0;
        if (no_probe || _SampleWorld)
        {
            lighty_boy_uwu_var = 1;
            reflection = texCUBElod(_CubeMap, float4(reflectedDir, roughness * UNITY_SPECCUBE_LOD_STEPS));
        }
    }
    
    void applyReflections(inout float4 finalColor, float4 finalColorBeforeLighting)
    {
        #ifdef FORWARD_BASE_PASS
            finalreflections = reflection.rgb * lerp(finalColorBeforeLighting.rgb, 1, _PurelyAdditive);
            finalColor.rgb = finalColor.rgb * (1 - metalicMap);
            finalColor.rgb += (finalreflections * ((1 - roughness + metalicMap) / 2)) * lerp(1, poiLight.finalLighting, lighty_boy_uwu_var);
        #endif
    }
    
    void applyAdditiveReflectiveLighting(inout float4 finalColor)
    {
        finalColor *= (1 - metalicMap);
    }
    
#endif