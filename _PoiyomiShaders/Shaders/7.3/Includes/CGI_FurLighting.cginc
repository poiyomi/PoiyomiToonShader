
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
    uint _LightingAOTexUV;
    fixed _LightingStandardSmoothness;
    fixed _LightingStandardControlsToon;
    fixed _LightingMinLightBrightness;
    fixed _LightingAdditiveIntensity;
    fixed _AoIndirectStrength;
    UNITY_DECLARE_TEX2D(_ToonRamp);
    
    sampler2D _LightingAOTex; float4 _LightingAOTex_ST;
    sampler2D _LightingShadowMask; float4 _LightingShadowMask_ST;
    float _AOStrength;
    
    float3 BoxProjection(float3 direction, float3 position, float4 cubemapPosition, float3 boxMin, float3 boxMax)
    {
        #if UNITY_SPECCUBE_BOX_PROJECTION
            UNITY_BRANCH
            if (cubemapPosition.w > 0)
            {
                float3 factors = ((direction > 0 ? boxMax: boxMin) - position) / direction;
                float scalar = min(min(factors.x, factors.y), factors.z);
                direction = direction * scalar + (position - cubemapPosition);
            }
        #endif
        return direction;
    }
    
    /*
    * Standard stuff Start
    */
    UnityLight CreateLight(half3 lightDirection, half attenuation, half ndotl)
    {
        UnityLight light;
        light.dir = lightDirection;
        light.color = saturate(_LightColor0.rgb * lerp(1, attenuation, _AttenuationMultiplier));
        light.ndotl = ndotl;
        return light;
    }
    
    float FadeShadows(float attenuation)
    {
        #if HANDLE_SHADOWS_BLENDING_IN_GI || ADDITIONAL_MASKED_DIRECTIONAL_SHADOWS
            // UNITY_LIGHT_ATTENUATION doesn't fade shadows for us.
            
            #if ADDITIONAL_MASKED_DIRECTIONAL_SHADOWS
                attenuation = lerp(1, attenuation, _AttenuationMultiplier);
            #endif
            
            float viewZ = dot(_WorldSpaceCameraPos - worldPos, UNITY_MATRIX_V[2].xyz);
            float shadowFadeDistance = UnityComputeShadowFadeDistance(worldPos, viewZ);
            float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
            float bakedAttenuation = UnitySampleBakedOcclusion(lightmapUV.xy, worldPos);
            attenuation = UnityMixRealtimeAndBakedShadows(
                attenuation, bakedAttenuation, shadowFade
            );
        #endif
        
        return attenuation;
    }
    
    void ApplySubtractiveLighting(inout UnityIndirect indirectLight)
    {
        #if SUBTRACTIVE_LIGHTING
            attenuation = FadeShadows(lerp(1, attenuation, _AttenuationMultiplier));
            
            float nDotL = saturate(dot(i.normal, _WorldSpaceLightPos0.xyz));
            float3 shadowedLightEstimate = nDotL * (1 - attenuation) * _LightColor0.rgb;
            float3 subtractedLight = indirectLight.diffuse - shadowedLightEstimate;
            subtractedLight = max(subtractedLight, unity_ShadowColor.rgb);
            subtractedLight = lerp(subtractedLight, indirectLight.diffuse, _LightShadowData.x);
            indirectLight.diffuse = min(subtractedLight, indirectLight.diffuse);
        #endif
    }
    
    UnityIndirect CreateIndirectLight(float3 normal, float3 worldPos, half3 viewDir, float2 uv)
    {
        UnityIndirect indirectLight;
        indirectLight.diffuse = 0;
        indirectLight.specular = 0;
        
        #if defined(FORWARD_BASE_PASS)
            #if defined(LIGHTMAP_ON)
                indirectLight.diffuse = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, lightmapUV.xy));
                
                #if defined(DIRLIGHTMAP_COMBINED)
                    float4 lightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(
                        unity_LightmapInd, unity_Lightmap, lightmapUV.xy
                    );
                    indirectLight.diffuse = DecodeDirectionalLightmap(
                        indirectLight.diffuse, lightmapDirection, normal
                    );
                #endif
                ApplySubtractiveLighting(indirectLight);
            #endif
            
            #if defined(DYNAMICLIGHTMAP_ON)
                float3 dynamicLightDiffuse = DecodeRealtimeLightmap(
                    UNITY_SAMPLE_TEX2D(unity_DynamicLightmap, lightmapUV.zw)
                );
                
                #if defined(DIRLIGHTMAP_COMBINED)
                    float4 dynamicLightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(
                        unity_DynamicDirectionality, unity_DynamicLightmap,
                        lightmapUV.zw
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
                            float4(normal, 1), worldPos
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
            
            float3 reflectionDir = reflect(-viewDir, normal);
            Unity_GlossyEnvironmentData envData;
            envData.roughness = 1 - _LightingStandardSmoothness;
            envData.reflUVW = BoxProjection(
                reflectionDir, worldPos.xyz,
                unity_SpecCube0_ProbePosition,
                unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax
            );
            float3 probe0 = Unity_GlossyEnvironment(
                UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData
            );
            envData.reflUVW = BoxProjection(
                reflectionDir, worldPos.xyz,
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
            
            float occlusion = lerp(1, tex2D(_LightingAOTex, TRANSFORM_TEX(uv, _LightingAOTex)), _AOStrength);
            
            indirectLight.diffuse *= occlusion;
            indirectLight.diffuse = max(indirectLight.diffuse, _LightingMinLightBrightness);
            indirectLight.specular *= occlusion;
        #endif
        
        return indirectLight;
    }
    
    /*
    * Standard stuff End
    */
    
    half PoiDiffuse(half NdotV, half nDotL, half LdotH)
    {
        half fd90 = 0.5 + 2 * LdotH * LdotH * SmoothnessToPerceptualRoughness(.5);
        // Two schlick fresnel term
        half lightScatter = (1 + (fd90 - 1) * Pow5(1 - nDotL));
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
    
    float3 calculateRealisticLighting(float4 colorToLight, float3 normal, float3 viewDir, half3 lightDirection, half attenuation, half ndotl, float3 worldPos, float2 uv)
    {
        return UNITY_BRDF_PBS(colorToLight, 0, 0, _LightingStandardSmoothness,
        normal, viewDir, CreateLight(lightDirection, attenuation, ndotl), CreateIndirectLight(normal, worldPos, viewDir, uv));
    }
    
    half3 calculateBasePassLighting(float2 uv, float3 lightColor, half nDotL, half attenuation, half3 normal, half3 viewDir, half3 lightDirection, float3 worldPos)
    {
        half3 finalLighting = 0;
        UNITY_BRANCH
        if (_LightingType == 0 || _LightingType == 1)
        {
            float AOMap = 1;
            float DirectAO = 1;
            float IndirectAO = 1;
            #ifndef OUTLINE
                AOMap = tex2D(_LightingAOTex, TRANSFORM_TEX(uv, _LightingAOTex));
                DirectAO = lerp(1, AOMap, _AOStrength);
                IndirectAO = lerp(1, AOMap, _AoIndirectStrength);
            #endif
            
            half3 grayscale_vector = float3(.33333, .33333, .33333);
            half3 ShadeSH9Plus = GetSHLength();
            half3 ShadeSH9Minus = ShadeSH9(float4(0, 0, 0, 1));
            half3 directLighting = saturate(lerp(ShadeSH9Plus, lightColor, 1 - _LightingIndirectContribution));
            half3 indirectLighting = saturate(ShadeSH9Minus) * IndirectAO;
            
            half4 shadowStrength = 1;
            #ifndef OUTLINE
                shadowStrength = tex2D(_LightingShadowMask, TRANSFORM_TEX(uv, _LightingShadowMask));
                shadowStrength *= _ShadowStrength;
            #else
                shadowStrength = _OutlineShadowStrength;
            #endif
            
            float bw_lightColor = dot(lightColor, grayscale_vector);
            float bw_directLighting = (((nDotL * 0.5 + 0.5) * bw_lightColor * lerp(1, attenuation, _AttenuationMultiplier)) + dot(ShadeSH9Normal(normal), grayscale_vector));
            float bw_bottomIndirectLighting = dot(ShadeSH9Minus, grayscale_vector);
            float bw_topIndirectLighting = dot(ShadeSH9Plus, grayscale_vector);
            float lightDifference = ((bw_topIndirectLighting + bw_lightColor) - bw_bottomIndirectLighting);
            float lightMap = smoothstep(0, lightDifference, bw_directLighting - bw_bottomIndirectLighting) * DirectAO;
            float3 rampedLightMap = lerp(1, UNITY_SAMPLE_TEX2D(_ToonRamp, lightMap + _ShadowOffset), shadowStrength.r);
            
            UNITY_BRANCH
            if(_LightingType == 0)
            {
                finalLighting = lerp(indirectLighting, directLighting, rampedLightMap);
            }
            UNITY_BRANCH
            if(_LightingType == 1)
            {
                finalLighting = rampedLightMap * directLighting;
            }
        }
        return finalLighting;
    }
    
    void applyFurLighting(inout float4 finalColor, float2 uv, half attenuation, half3 normal, half3 viewDir, float3 worldPos)
    {
        float3 finalLighting = 0;
        float3 lightColor = 0;
        float3 lightDirection = 0;
        
        #ifdef FORWARD_BASE_PASS
            //lightColor = saturate(_LightColor0.rgb) + saturate(ShadeSH9(normalize(unity_SHAr + unity_SHAg + unity_SHAb)));
            float3 magic = saturate(ShadeSH9(normalize(unity_SHAr + unity_SHAg + unity_SHAb)));
            float3 normalLight = saturate(_LightColor0.rgb);
            lightColor = saturate(magic + normalLight);
        #else
            #if defined(POINT) || defined(SPOT)
                lightColor = _LightColor0.rgb;
            #endif
        #endif
        
        #ifdef FORWARD_BASE_PASS
            lightDirection = normalize(_WorldSpaceLightPos0 + unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz);
        #else
            #if defined(POINT) || defined(SPOT)
                lightDirection = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
            #endif
        #endif
        
        half nDotL = dot(normal, lightDirection);
        
        #ifdef FORWARD_BASE_PASS
            finalLighting = calculateBasePassLighting(uv, lightColor, nDotL, attenuation, normal, viewDir, lightDirection, worldPos);
        #else
            #if defined(POINT) || defined(SPOT)
                finalLighting = lightColor * attenuation * smoothstep(.5 - _AdditiveSoftness + _AdditiveOffset, .5 + _AdditiveSoftness + _AdditiveOffset, .5 * nDotL + .5);
                finalLighting *= _LightingAdditiveIntensity;
            #endif
        #endif
        
        #ifdef FORWARD_BASE_PASS
            UNITY_BRANCH
            if (_LightingType == 2)
            {
                finalColor.rgb = calculateRealisticLighting(finalColor, normal, viewDir, lightDirection, attenuation, nDotL, worldPos, uv);
            }
            else
            {
                finalColor.rgb *= max(finalLighting, _LightingMinLightBrightness);
            }
        #else
            finalColor.rgb *= max(finalLighting, _LightingMinLightBrightness);
        #endif
    }
#endif