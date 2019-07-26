#ifndef LIGHTING
    #define LIGHTING
    
    int _LightingType;
    sampler2D _ToonRamp;
    float _AdditiveSoftness;
    float _AdditiveOffset;
    float _ForceLightDirection;
    float _ShadowStrength;
    float _OutlineShadowStrength;
    float _ShadowOffset;
    float3 _LightDirection;
    float _ForceShadowStrength;
    float _CastedShadowSmoothing;
    float _MinBrightness;
    float _MaxBrightness;
    float _IndirectContribution;
    float _AttenuationMultiplier;
    float _EnableLighting;
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_AOMap); float4 _AOMap_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_LightingShadowMask); float4 _LightingShadowMask_ST;
    float _AOStrength;
    
    inline UnityGI FragmentGI(FragmentCommonData s, half occlusion, half4 i_ambientOrLightmapUV, half atten, UnityLight light, bool reflections)
    {
        UnityGIInput d;
        d.light = light;
        d.worldPos = s.posWorld;
        d.worldViewDir = -s.eyeVec;
        d.atten = atten;
        #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
            d.ambient = 0;
            d.lightmapUV = i_ambientOrLightmapUV;
        #else
            d.ambient = i_ambientOrLightmapUV.rgb;
            d.lightmapUV = 0;
        #endif
        
        d.probeHDR[0] = unity_SpecCube0_HDR;
        d.probeHDR[1] = unity_SpecCube1_HDR;
        #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
            d.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
        #endif
        #ifdef UNITY_SPECCUBE_BOX_PROJECTION
            d.boxMax[0] = unity_SpecCube0_BoxMax;
            d.probePosition[0] = unity_SpecCube0_ProbePosition;
            d.boxMax[1] = unity_SpecCube1_BoxMax;
            d.boxMin[1] = unity_SpecCube1_BoxMin;
            d.probePosition[1] = unity_SpecCube1_ProbePosition;
        #endif
        
        if (reflections)
        {
            Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.smoothness, -s.eyeVec, s.normalWorld, s.specColor);
            // Replace the reflUVW if it has been compute in Vertex shader. Note: the compiler will optimize the calcul in UnityGlossyEnvironmentSetup itself
            #if UNITY_STANDARD_SIMPLE
                g.reflUVW = s.reflUVW;
            #endif
            
            return UnityGlobalIllumination(d, occlusion, s.normalWorld, g);
        }
        else
        {
            return UnityGlobalIllumination(d, occlusion, s.normalWorld);
        }
    }
    
    inline UnityGI FragmentGI(FragmentCommonData s, half occlusion, half4 i_ambientOrLightmapUV, half atten, UnityLight light)
    {
        return FragmentGI(s, occlusion, i_ambientOrLightmapUV, atten, light, true);
    }
    
    half PoiDiffuse(half NdotV, half NdotL, half LdotH)
    {
        half fd90 = 0.5 + 2 * LdotH * LdotH * SmoothnessToPerceptualRoughness(.5);
        // Two schlick fresnel term
        half lightScatter = (1 + (fd90 - 1) * Pow5(1 - NdotL));
        half viewScatter = (1 + (fd90 - 1) * Pow5(1 - NdotV));
        
        return lightScatter * viewScatter;
    }
    
    float3 ShadeSH9Indirect()
    {
        return ShadeSH9(half4(0.0, -1.0, 0.0, 1.0));
    }
    
    float3 ShadeSH9Direct()
    {
        return ShadeSH9(half4(0.0, 1.0, 0.0, 1.0));
    }
    
    float3 ShadeSH9Normal(float3 normalDirection)
    {
        return ShadeSH9(half4(normalDirection, 1.0));
    }
    
    half3 GetSHLength()
    {
        half3 x, x1;
        x.r = length(unity_SHAr);
        x.g = length(unity_SHAg);
        x.b = length(unity_SHAb);
        x1.r = length(unity_SHBr);
        x1.g = length(unity_SHBg);
        x1.b = length(unity_SHBb);
        return x + x1;
    }
    
    float calculateAOMap(float AOMap, float AOStrength)
    {
        return lerp(1, AOMap, AOStrength);
    }
    
    UnityLight MainLight()
    {
        UnityLight l = (UnityLight)0;
        
        l.color = poiLight.color.rgb;
        l.dir = poiLight.direction;
        
        return l;
    }
    
    void calculateBasePassLighting(float3 normal, float2 uv)
    {
        float AOMap = 1;
        #ifndef OUTLINE
            AOMap = UNITY_SAMPLE_TEX2D_SAMPLER(_AOMap, _MainTex, TRANSFORM_TEX(uv, _AOMap));
            AOMap = calculateAOMap(AOMap, _AOStrength);
        #endif
        
        float3 grayscale_vector = grayscale_for_light();
        float3 ShadeSH9Plus = GetSHLength();
        float3 ShadeSH9Minus = ShadeSH9(float4(0, 0, 0, 1));
        
        #ifndef OUTLINE
            float ShadowStrengthMap = UNITY_SAMPLE_TEX2D_SAMPLER(_LightingShadowMask, _MainTex, TRANSFORM_TEX(uv, _LightingShadowMask)).r;
            _ShadowStrength *= ShadowStrengthMap;
        #endif
        
        float bw_lightColor = dot(poiLight.color, grayscale_vector);
        float bw_directLighting = (((poiLight.nDotL * 0.5 + 0.5) * bw_lightColor * lerp(1, poiLight.attenuation, _AttenuationMultiplier)) + dot(ShadeSH9Normal(normal), grayscale_vector));
        float bw_bottomIndirectLighting = dot(ShadeSH9Minus, grayscale_vector);
        float bw_topIndirectLighting = dot(ShadeSH9Plus, grayscale_vector);
        float lightDifference = ((bw_topIndirectLighting + bw_lightColor) - bw_bottomIndirectLighting);
        poiLight.lightMap = smoothstep(0, lightDifference, bw_directLighting - bw_bottomIndirectLighting);
        
        gi = FragmentGI(s, AOMap, 0, poiLight.attenuation, MainLight());
        poiLight.directLighting = gi.indirect.diffuse + poiLight.color;
        poiLight.indirectLighting = gi.indirect.diffuse;
        
        poiLight.rampedLightMap = tex2D(_ToonRamp, poiLight.lightMap + _ShadowOffset);
        
        if (_LightingType == 0)
        {
            poiLight.finalLighting = lerp((poiLight.indirectLighting), lerp(poiLight.directLighting, poiLight.indirectLighting, _IndirectContribution), lerp(1, poiLight.rampedLightMap, _ShadowStrength)) * AOMap;
        }
        else
        {
            poiLight.finalLighting = (poiLight.directLighting) * lerp(1, poiLight.rampedLightMap, _ShadowStrength);
        }
        
        poiLight.finalLighting = clamp(poiLight.finalLighting, _MinBrightness, _MaxBrightness);
    }
    
    void calculateLighting(v2f i)
    {
        UNITY_BRANCH
        if(_EnableLighting)
        {
            #ifdef OUTLINE
                _ShadowStrength = _OutlineShadowStrength;
            #endif
            #ifdef FORWARD_BASE_PASS
                calculateBasePassLighting(poiMesh.fragmentNormal, i.uv);
            #else
                #if defined(POINT) || defined(SPOT)
                    poiLight.finalLighting = poiLight.color * poiLight.attenuation * smoothstep(.5 - _AdditiveSoftness + _AdditiveOffset, .5 + _AdditiveSoftness + _AdditiveOffset, .5 * poiLight.nDotL + .5);
                #endif
            #endif
        }
    }
    void applyLighting(inout float4 finalColor)
    {
        UNITY_BRANCH
        if(_EnableLighting)
        {
            finalColor.rgb *= poiLight.finalLighting;
        }
    }
#endif