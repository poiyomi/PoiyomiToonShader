
#ifndef POI_LIGHTING
    #define POI_LIGHTING
    
    int _LightingType;
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
    fixed _LightingStandardSmoothness;
    fixed _LightingStandardControlsToon;
    fixed _LightingMinLightBrightness;
    float _LightingUseShadowRamp;
    UNITY_DECLARE_TEX2D(_ToonRamp);
    fixed _LightingMonochromatic;
    
    uint _LightingNumRamps;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_ToonRamp1);
    UNITY_DECLARE_TEX2D_NOSAMPLER(_ToonRamp2);
    half _LightingShadowStrength1;
    half _LightingShadowStrength2;
    half _ShadowOffset1;
    half _ShadowOffset2;
    
    fixed _LightingGradientStart;
    fixed _LightingGradientEnd;
    float3 _LightingStartColor;
    float3 _LightingEndColor;
    float _AOStrength;
    fixed _LightingDetailStrength;
    fixed _LightingAdditiveDetailStrength;
    fixed _LightingNoIndirectMultiplier;
    fixed _LightingNoIndirectThreshold;
    
    uint _LightingAdditiveType;
    fixed _LightingAdditiveGradientStart;
    fixed _LightingAdditiveGradientEnd;
    fixed _LightingAdditivePassthrough;
    /*
    UNITY_DECLARE_TEX2D_NOSAMPLER(_ToonRamp3);
    half _LightingShadowStrength3;
    half _ShadowOffset3;
    */
    
    POI_TEXTURE_NOSAMPLER(_LightingDetailShadows);
    POI_TEXTURE_NOSAMPLER(_LightingAOTex);
    POI_TEXTURE_NOSAMPLER(_LightingShadowMask);
    
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
            
            float occlusion = lerp(1, POI2D_SAMPLER_PAN(_LightingAOTex, _MainTex, poiMesh.uv[_LightingAOTexUV], _LightingAOTexPan), _AOStrength);
            
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
            _LightingIndirectContribution = 0.2;
        #endif
        float AOMap = 1;
        float3 lightColor = poiLight.color;
        #ifndef OUTLINE
            AOMap = POI2D_SAMPLER_PAN(_LightingAOTex, _MainTex, poiMesh.uv[_LightingAOTexUV], _LightingAOTexPan);
            
            #ifdef FORWARD_BASE_PASS
                //poiLight.color = saturate(_LightColor0.rgb) + saturate(ShadeSH9(normalize(unity_SHAr + unity_SHAg + unity_SHAb)));
                float3 magic = saturate(ShadeSH9(normalize(unity_SHAr + unity_SHAg + unity_SHAb)));
                float3 normalLight = saturate(_LightColor0.rgb);
                lightColor = saturate(magic * lerp(1, AOMap, _AOStrength) + normalLight);
            #endif
        #endif
        
        float3 grayscale_vector = float3(.33333, .33333, .33333);
        float3 ShadeSH9Plus = GetSHLength();
        float3 ShadeSH9Minus = ShadeSH9(float4(0, 0, 0, 1));
        poiLight.directLighting = saturate(lerp(ShadeSH9Plus, lightColor, 1 - _LightingIndirectContribution));
        poiLight.indirectLighting = saturate(ShadeSH9Minus);
        
        float3 directLighting = lerp(poiLight.directLighting, dot(poiLight.directLighting, float3(0.299, 0.587, 0.114)), _LightingMonochromatic);
        float3 indirectLighting = lerp(poiLight.indirectLighting, dot(poiLight.indirectLighting, float3(0.299, 0.587, 0.114)), _LightingMonochromatic);

        if (max(max(indirectLighting.x, indirectLighting.y), indirectLighting.z) <= _LightingNoIndirectThreshold && max(max(directLighting.x, directLighting.y), directLighting.z) >= 0)
        {
            indirectLighting = directLighting * _LightingNoIndirectMultiplier;
        }
        
        half4 shadowStrength = 1;
        #ifndef OUTLINE
            #ifndef SIMPLE
                shadowStrength = POI2D_SAMPLER_PAN(_LightingShadowMask, _MainTex, poiMesh.uv[_LightingShadowMaskUV], _LightingShadowMaskPan);
            #endif
            shadowStrength *= half4(_ShadowStrength, _LightingShadowStrength1, _LightingShadowStrength2, 0);
        #else
            shadowStrength = _OutlineShadowStrength;
        #endif
        
        float bw_lightColor = dot(lightColor, grayscale_vector);
        float bw_directLighting = (((poiLight.nDotL * 0.5 + 0.5) * bw_lightColor * lerp(1, poiLight.attenuation, _AttenuationMultiplier)) + dot(ShadeSH9Normal(poiMesh.normals[1]), grayscale_vector));
        float bw_bottomIndirectLighting = dot(ShadeSH9Minus, grayscale_vector);
        float bw_topIndirectLighting = dot(ShadeSH9Plus, grayscale_vector);
        float lightDifference = ((bw_topIndirectLighting + bw_lightColor) - bw_bottomIndirectLighting);
        
        fixed detailShadow = lerp(1, POI2D_SAMPLER_PAN(_LightingDetailShadows, _MainTex, poiMesh.uv[_LightingDetailShadowsUV], _LightingDetailShadowsPan), _LightingDetailStrength).r;
        poiLight.lightMap = smoothstep(0, lightDifference, bw_directLighting - bw_bottomIndirectLighting);
        poiLight.lightMap *= detailShadow;
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
            float3 realisticLighting = calculateRealisticLighting(1);
            poiLight.rampedLightMap = UNITY_SAMPLE_TEX2D(_ToonRamp, (.5 + dot(realisticLighting, float3(.33333, .33333, .33333)) * .5) + _ShadowOffset);
            return;
        }
        
        UNITY_BRANCH
        if(_LightingType == 0)
        {
            poiLight.finalLighting = lerp(indirectLighting * lerp(1, AOMap, _AOStrength), directLighting, poiLight.rampedLightMap);
        }
        UNITY_BRANCH
        if(_LightingType == 1)
        {
            poiLight.finalLighting = lerp(poiLight.rampedLightMap * directLighting * lerp(1, AOMap, _AOStrength), directLighting, poiLight.rampedLightMap);
        }
        UNITY_BRANCH
        if(_LightingType == 3)
        {
            poiLight.finalLighting = lerp(saturate(directLighting * _LightingStartColor), saturate(indirectLighting * _LightingEndColor * lerp(1, AOMap, _AOStrength)), smoothstep(_LightingGradientStart, _LightingGradientEnd, 1 - poiLight.lightMap));
        }
    }
    
    float3 calculateNonImportantLighting(float attenuation, float attenuationDotNL, float3 albedo, float3 lightColor, half dotNL)
    {
        UNITY_BRANCH
        if(_LightingAdditiveType == 0)
        {
            return lightColor * attenuationDotNL;
        }
        else
        {
            fixed detailShadow = lerp(1, POI2D_SAMPLER_PAN(_LightingDetailShadows, _MainTex, poiMesh.uv[_LightingDetailShadowsUV], _LightingDetailShadowsPan), _LightingAdditiveDetailStrength).r;
            return lerp(lightColor * attenuation, lightColor * _LightingAdditivePassthrough * attenuation, smoothstep(_LightingAdditiveGradientStart, _LightingAdditiveGradientEnd, dotNL)) * detailShadow;
        }
    }
    
    float3 calculateLighting(float3 albedo)
    {
        #ifdef SIMPLE
            _LightingType = 1;
        #endif
        #ifdef FORWARD_BASE_PASS
            calculateBasePassLighting();

            #ifdef VERTEXLIGHT_ON
                poiLight.vFinalLighting = 0;
                
                for (int index = 0; index < 4; index ++)
                {
                    poiLight.vFinalLighting += calculateNonImportantLighting(poiLight.vAttenuation[index], poiLight.vAttenuationDotNL[index], albedo, poiLight.vColor[index], poiLight.vCorrectedDotNL[index]);
                }
            #endif
        #else
            #if defined(POINT) || defined(SPOT)
                #ifndef SIMPLE
                    fixed detailShadow = lerp(1, POI2D_SAMPLER_PAN(_LightingDetailShadows, _MainTex, poiMesh.uv[_LightingDetailShadowsUV], _LightingDetailShadowsPan), _LightingAdditiveDetailStrength).r;
                    UNITY_BRANCH
                    if(_LightingAdditiveType == 0)
                    {
                        return poiLight.color * poiLight.attenuation * max(0, poiLight.nDotL) * detailShadow;
                    }
                    else
                    {
                        return lerp(poiLight.color * max(poiLight.additiveShadow, _LightingAdditivePassthrough), poiLight.color * _LightingAdditivePassthrough, smoothstep(_LightingAdditiveGradientStart, _LightingAdditiveGradientEnd, 1 - (.5 * poiLight.nDotL + .5))) * poiLight.attenuation * detailShadow;
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
                float3 realisticLighting = calculateRealisticLighting(finalColor).rgb;
                return lerp(realisticLighting, dot(realisticLighting, float3(0.299, 0.587, 0.114)), _LightingMonochromatic);
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