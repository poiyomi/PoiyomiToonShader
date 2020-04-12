
#ifndef POI_LIGHTING
    #define POI_LIGHTING
    
    int _LightingType;
    float _AdditiveSoftness;
    float _AdditiveOffset;
    float _ForceLightDirection;
    float _ShadowStrength;
    float _OutlineShadowStrength;
    float _ShadowOffset;
    float3 _LightDirection;
    float _ForceShadowStrength;
    float _CastedShadowSmoothing;
    float _LightingIndirectContribution;
    float _AttenuationMultiplier;
    float _EnableLighting;
    float _LightingControlledUseLightColor;
    uint _LightingAOUV;
    fixed _LightingStandardSmoothness;
    fixed _LightingStandardControlsToon;
    fixed _LightingMinLightBrightness;
    fixed _LightingAdditiveIntensity;
    fixed _AoIndirectStrength;
    float _LightingUseShadowRamp;
    UNITY_DECLARE_TEX2D(_ToonRamp);
    
    uint _LightingNumRamps;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_ToonRamp1);
    UNITY_DECLARE_TEX2D_NOSAMPLER(_ToonRamp2);
    half _LightingShadowStrength1;
    half _LightingShadowStrength2;
    half _ShadowOffset1;
    half _ShadowOffset2;
    /*
    UNITY_DECLARE_TEX2D_NOSAMPLER(_ToonRamp3);
    half _LightingShadowStrength3;
    half _ShadowOffset3;
    */
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_AOMap); float4 _AOMap_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_LightingShadowMask); float4 _LightingShadowMask_ST;
    float _AOStrength;
    
    /*
    * Standard stuff Start
    */
    UnityLight CreateLight(float3 normal)
    {
        UnityLight light;
        light.dir = poiLight.direction;
        light.color = saturate(_LightColor0.rgb * lerp(1, poiLight.attenuation, _AttenuationMultiplier));
        light.ndotl = DotClamped(normal, poiLight.direction);
        return light;
    }
    
    float FadeShadows(float attenuation)
    {
        #if HANDLE_SHADOWS_BLENDING_IN_GI || ADDITIONAL_MASKED_DIRECTIONAL_SHADOWS
            // UNITY_LIGHT_ATTENUATION doesn't fade shadows for us.
            
            #if ADDITIONAL_MASKED_DIRECTIONAL_SHADOWS
                attenuation = lerp(1, poiLight.attenuation, _AttenuationMultiplier);
            #endif
            
            float viewZ = dot(_WorldSpaceCameraPos - poiMesh.worldPos, UNITY_MATRIX_V[2].xyz);
            float shadowFadeDistance = UnityComputeShadowFadeDistance(poiMesh.worldPos, viewZ);
            float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
            float bakedAttenuation = UnitySampleBakedOcclusion(poiMesh.lightmapUV.xy, poiMesh.worldPos);
            attenuation = UnityMixRealtimeAndBakedShadows(
                attenuation, bakedAttenuation, shadowFade
            );
        #endif
        
        return attenuation;
    }
    
    void ApplySubtractiveLighting(inout UnityIndirect indirectLight)
    {
        #if SUBTRACTIVE_LIGHTING
            poiLight.attenuation = FadeShadows(lerp(1, poiLight.attenuation, _AttenuationMultiplier));
            
            float ndotl = saturate(dot(i.normal, _WorldSpaceLightPos0.xyz));
            float3 shadowedLightEstimate = ndotl * (1 - poiLight.attenuation) * _LightColor0.rgb;
            float3 subtractedLight = indirectLight.diffuse - shadowedLightEstimate;
            subtractedLight = max(subtractedLight, unity_ShadowColor.rgb);
            subtractedLight = lerp(subtractedLight, indirectLight.diffuse, _LightShadowData.x);
            indirectLight.diffuse = min(subtractedLight, indirectLight.diffuse);
        #endif
    }
    
    UnityIndirect CreateIndirectLight(float3 normal)
    {
        UnityIndirect indirectLight;
        indirectLight.diffuse = 0;
        indirectLight.specular = 0;
        
        #if defined(VERTEXLIGHT_ON)
            indirectLight.diffuse = poiLight.vertexLightColor;
        #endif
        
        #if defined(FORWARD_BASE_PASS)
            #if defined(LIGHTMAP_ON)
                indirectLight.diffuse = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, poiMesh.lightmapUV.xy));
                
                #if defined(DIRLIGHTMAP_COMBINED)
                    float4 lightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(
                        unity_LightmapInd, unity_Lightmap, poiMesh.lightmapUV.xy
                    );
                    indirectLight.diffuse = DecodeDirectionalLightmap(
                        indirectLight.diffuse, lightmapDirection, normal
                    );
                #endif
                ApplySubtractiveLighting(indirectLight);
            #endif
            
            #if defined(DYNAMICLIGHTMAP_ON)
                float3 dynamicLightDiffuse = DecodeRealtimeLightmap(
                    UNITY_SAMPLE_TEX2D(unity_DynamicLightmap, poiMesh.lightmapUV.zw)
                );
                
                #if defined(DIRLIGHTMAP_COMBINED)
                    float4 dynamicLightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(
                        unity_DynamicDirectionality, unity_DynamicLightmap,
                        poiMesh.lightmapUV.zw
                    );
                    indirectLight.diffuse += DecodeDirectionalLightmap(
                        dynamicLightDiffuse, dynamicLightmapDirection, normal
                    );
                #else
                    indirectLight.diffuse += dynamicLightDiffuse;
                #endif
            #endif
            
            #if !defined(LIGHTMAP_ON) && !defined(DYNAMICLIGHTMAP_ON)
                #if UNITY_LIGHT_PROBE_PROXY_VOLUME
                    if (unity_ProbeVolumeParams.x == 1)
                    {
                        indirectLight.diffuse = SHEvalLinearL0L1_SampleProbeVolume(
                            float4(normal, 1), poiMesh.worldPos
                        );
                        indirectLight.diffuse = max(0, indirectLight.diffuse);
                        #if defined(UNITY_COLORSPACE_GAMMA)
                            indirectLight.diffuse = LinearToGammaSpace(indirectLight.diffuse);
                        #endif
                    }
                    else
                    {
                        indirectLight.diffuse += max(0, ShadeSH9(float4(normal, 1)));
                    }
                #else
                    indirectLight.diffuse += max(0, ShadeSH9(float4(normal, 1)));
                #endif
            #endif
            
            float3 reflectionDir = reflect(-poiCam.viewDir, normal);
            Unity_GlossyEnvironmentData envData;
            envData.roughness = 1 - _LightingStandardSmoothness;
            envData.reflUVW = BoxProjection(
                reflectionDir, poiMesh.worldPos.xyz,
                unity_SpecCube0_ProbePosition,
                unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax
            );
            float3 probe0 = Unity_GlossyEnvironment(
                UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData
            );
            envData.reflUVW = BoxProjection(
                reflectionDir, poiMesh.worldPos.xyz,
                unity_SpecCube1_ProbePosition,
                unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax
            );
            #if UNITY_SPECCUBE_BLENDING
                float interpolator = unity_SpecCube0_BoxMin.w;
                UNITY_BRANCH
                if(interpolator < 0.99999)
                {
                    float3 probe1 = Unity_GlossyEnvironment(
                        UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1, unity_SpecCube0),
                        unity_SpecCube0_HDR, envData
                    );
                    indirectLight.specular = lerp(probe1, probe0, interpolator);
                }
                else
                {
                    indirectLight.specular = probe0;
                }
            #else
                indirectLight.specular = probe0;
            #endif
            
            float occlusion = lerp(1, UNITY_SAMPLE_TEX2D_SAMPLER(_AOMap, _MainTex, TRANSFORM_TEX(poiMesh.uv[_LightingAOUV], _AOMap)), _AOStrength);
            
            indirectLight.diffuse *= occlusion;
            indirectLight.diffuse = max(indirectLight.diffuse, _LightingMinLightBrightness);
            indirectLight.specular *= occlusion;
        #endif
        
        return indirectLight;
    }
    
    /*
    * Standard stuff End
    */
    
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
    
    float3 calculateRealisticLighting(float4 colorToLight)
    {
        return UNITY_BRDF_PBS(1, 0, 0, _LightingStandardSmoothness,
        poiMesh.normals[1], poiCam.viewDir, CreateLight(poiMesh.normals[1]), CreateIndirectLight(poiMesh.normals[1]));
    }
    
    void calculateBasePassLighting()
    {
        #ifdef SIMPLE
            _LightingType = 1;
        #endif
        UNITY_BRANCH
        if (_LightingType == 0 || _LightingType == 1)
        {
            float AOMap = 1;
            #ifndef OUTLINE
                AOMap = UNITY_SAMPLE_TEX2D_SAMPLER(_AOMap, _MainTex, TRANSFORM_TEX(poiMesh.uv[_LightingAOUV], _AOMap));
            #endif
            
            
            
            float3 grayscale_vector = float3(.33333, .33333, .33333);
            float3 ShadeSH9Plus = GetSHLength();
            float3 ShadeSH9Minus = ShadeSH9(float4(0, 0, 0, 1));
            poiLight.directLighting = saturate(lerp(ShadeSH9Plus, poiLight.color, 1 - _LightingIndirectContribution));
            poiLight.indirectLighting = saturate(ShadeSH9Minus);
            
            half4 shadowStrength = 1;
            #ifndef OUTLINE
                #ifndef SIMPLE
                    shadowStrength = UNITY_SAMPLE_TEX2D_SAMPLER(_LightingShadowMask, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _LightingShadowMask));
                #endif
                shadowStrength *= half4(_ShadowStrength, _LightingShadowStrength1, _LightingShadowStrength2, 0);
            #else
                shadowStrength = _OutlineShadowStrength;
            #endif
            
            float bw_lightColor = dot(poiLight.color, grayscale_vector);
            float bw_directLighting = (((poiLight.nDotL * 0.5 + 0.5) * bw_lightColor * lerp(1, poiLight.attenuation, _AttenuationMultiplier)) + dot(ShadeSH9Normal(poiMesh.normals[1]), grayscale_vector));
            float bw_bottomIndirectLighting = dot(ShadeSH9Minus, grayscale_vector);
            float bw_topIndirectLighting = dot(ShadeSH9Plus, grayscale_vector);
            float lightDifference = ((bw_topIndirectLighting + bw_lightColor) - bw_bottomIndirectLighting);
            poiLight.lightMap = smoothstep(0, lightDifference, bw_directLighting - bw_bottomIndirectLighting) * lerp(1, AOMap, _AOStrength);
            poiLight.rampedLightMap = lerp(1, UNITY_SAMPLE_TEX2D(_ToonRamp, poiLight.lightMap + _ShadowOffset), shadowStrength.r);
            
            UNITY_BRANCH
            if(_LightingNumRamps >= 2)
            {
                poiLight.rampedLightMap *= lerp(1, UNITY_SAMPLE_TEX2D_SAMPLER(_ToonRamp1, _ToonRamp, poiLight.lightMap + _ShadowOffset1), shadowStrength.g);
            }
            UNITY_BRANCH
            if(_LightingNumRamps >= 3)
            {
                poiLight.rampedLightMap *= lerp(1, UNITY_SAMPLE_TEX2D_SAMPLER(_ToonRamp2, _ToonRamp, poiLight.lightMap + _ShadowOffset2), shadowStrength.b);
            }
            
            UNITY_BRANCH
            if(_LightingStandardControlsToon)
            {
                float3 RealisticLighting = calculateRealisticLighting(1);
                poiLight.rampedLightMap = UNITY_SAMPLE_TEX2D(_ToonRamp, (.5 + dot(RealisticLighting, float3(.33333, .33333, .33333)) * .5) + _ShadowOffset);
                return;
            }
            
            UNITY_BRANCH
            if(_LightingType == 0)
            {
                poiLight.finalLighting = lerp(poiLight.indirectLighting * lerp(1, AOMap, _AoIndirectStrength), poiLight.directLighting, poiLight.rampedLightMap);
            }
            UNITY_BRANCH
            if(_LightingType == 1)
            {
                poiLight.finalLighting = poiLight.rampedLightMap * poiLight.directLighting;
            }
        }
    }
    
    float3 calculateLighting()
    {
        #ifdef FORWARD_BASE_PASS
            calculateBasePassLighting();
        #else
            #if defined(POINT) || defined(SPOT)
                #ifndef SIMPLE
                    UNITY_BRANCH
                    if(_LightingUseShadowRamp)
                    {
                        float uv = poiLight.nDotL;
                        float3 lighting = UNITY_SAMPLE_TEX2D_SAMPLER(_ToonRamp1, _ToonRamp, uv + _ShadowOffset1) * poiLight.color;
                        poiLight.finalLighting *= lighting;
                    }
                    else
                    {
                        poiLight.finalLighting = poiLight.color * poiLight.attenuation * smoothstep(.5 - _AdditiveSoftness + _AdditiveOffset, .5 + _AdditiveSoftness + _AdditiveOffset, .5 * poiLight.nDotL + .5);
                        poiLight.finalLighting *= _LightingAdditiveIntensity;
                    }
                #else
                    poiLight.finalLighting = poiLight.color * poiLight.attenuation;
                #endif
            #endif
        #endif
        
        #ifdef FORWARD_BASE_PASS
            UNITY_BRANCH
            if(_LightingType == 2)
            {
                return calculateRealisticLighting(finalColor).rgb;
            }
            else
            {
                return max(poiLight.finalLighting, _LightingMinLightBrightness);
            }
        #else
            return max(poiLight.finalLighting, _LightingMinLightBrightness);
        #endif
    }
#endif