
#ifndef POI_LIGHTING
    #define POI_LIGHTING
    
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
    float _LightingIndirectContribution;
    float _AttenuationMultiplier;
    float _EnableLighting;
    float _LightingControlledUseLightColor;
    uint _LightingAOUV;
    fixed _LightingStandardSmoothness;
    fixed _LightingStandardControlsToon;
    fixed _LightingMinLightBrightness;

    UNITY_DECLARE_TEX2D_NOSAMPLER(_AOMap); float4 _AOMap_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_LightingShadowMask); float4 _LightingShadowMask_ST;
    float _AOStrength;
    
    /*
    * Standard stuff Start
    */
    UnityLight CreateLight()
    {
        UnityLight light;
        light.dir = poiLight.direction;
        light.color = saturate(_LightColor0.rgb * lerp(1, poiLight.attenuation, _AttenuationMultiplier));
        light.ndotl = DotClamped(poiMesh.fragmentNormal, poiLight.direction);
        return light;
    }
    
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
            attenuation = FadeShadows(lerp(1, poiLight.attenuation, _AttenuationMultiplier));
            
            float ndotl = saturate(dot(i.normal, _WorldSpaceLightPos0.xyz));
            float3 shadowedLightEstimate = ndotl * (1 - lerp(1, poiLight.attenuation, _AttenuationMultiplier)) * _LightColor0.rgb;
            float3 subtractedLight = indirectLight.diffuse - shadowedLightEstimate;
            subtractedLight = max(subtractedLight, unity_ShadowColor.rgb);
            subtractedLight = lerp(subtractedLight, indirectLight.diffuse, _LightShadowData.x);
            indirectLight.diffuse = min(subtractedLight, indirectLight.diffuse);
        #endif
    }
    
    UnityIndirect CreateIndirectLight()
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
                        indirectLight.diffuse, lightmapDirection, poiMesh.fragmentNormal
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
                        dynamicLightDiffuse, dynamicLightmapDirection, poiMesh.fragmentNormal
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
                            float4(poiMesh.fragmentNormal, 1), poiMesh.worldPos
                        );
                        indirectLight.diffuse = max(0, indirectLight.diffuse);
                        #if defined(UNITY_COLORSPACE_GAMMA)
                            indirectLight.diffuse = LinearToGammaSpace(indirectLight.diffuse);
                        #endif
                    }
                    else
                    {
                        indirectLight.diffuse += max(0, ShadeSH9(float4(poiMesh.fragmentNormal, 1)));
                    }
                #else
                    indirectLight.diffuse += max(0, ShadeSH9(float4(poiMesh.fragmentNormal, 1)));
                #endif
            #endif
            
            float3 reflectionDir = reflect(-poiCam.viewDir, poiMesh.fragmentNormal);
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
            indirectLight.diffuse = max(indirectLight.diffuse,_LightingMinLightBrightness);
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
        return UNITY_BRDF_PBS(colorToLight, 0, 0, _LightingStandardSmoothness,
        poiMesh.fragmentNormal, poiCam.viewDir, CreateLight(), CreateIndirectLight());
    }
    
    void calculateBasePassLighting()
    {
        UNITY_BRANCH
        if (_LightingType == 0 || _LightingType == 1)
        {
            float AOMap = 1;
            #ifndef OUTLINE
                AOMap = UNITY_SAMPLE_TEX2D_SAMPLER(_AOMap, _MainTex, TRANSFORM_TEX(poiMesh.uv[_LightingAOUV], _AOMap));
                AOMap = lerp(1, AOMap, _AOStrength);
            #endif
            
            float3 grayscale_vector = float3(.33333, .33333, .33333);
            float3 ShadeSH9Plus = GetSHLength();
            float3 ShadeSH9Minus = ShadeSH9(float4(0, 0, 0, 1));
            poiLight.directLighting = saturate(lerp(ShadeSH9Plus, poiLight.color, 1 - _LightingIndirectContribution));
            poiLight.indirectLighting = saturate(ShadeSH9Minus);
            
            #ifndef OUTLINE
                float ShadowStrengthMap = UNITY_SAMPLE_TEX2D_SAMPLER(_LightingShadowMask, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _LightingShadowMask)).r;
                _ShadowStrength *= ShadowStrengthMap;
            #endif
            
            UNITY_BRANCH
            if(!_LightingStandardControlsToon)
            {
                
                float bw_lightColor = dot(poiLight.color, grayscale_vector);
                float bw_directLighting = (((poiLight.nDotL * 0.5 + 0.5) * bw_lightColor * lerp(1, poiLight.attenuation, _AttenuationMultiplier)) + dot(ShadeSH9Normal(poiMesh.fragmentNormal), grayscale_vector));
                float bw_bottomIndirectLighting = dot(ShadeSH9Minus, grayscale_vector);
                float bw_topIndirectLighting = dot(ShadeSH9Plus, grayscale_vector);
                float lightDifference = ((bw_topIndirectLighting + bw_lightColor) - bw_bottomIndirectLighting);
                poiLight.lightMap = smoothstep(0, lightDifference, bw_directLighting - bw_bottomIndirectLighting);
                poiLight.rampedLightMap = tex2D(_ToonRamp, poiLight.lightMap * AOMap + _ShadowOffset);
            }
            else
            {
                float3 RealisticLighting = calculateRealisticLighting(1);
                poiLight.rampedLightMap = tex2D(_ToonRamp, (.5 + dot(RealisticLighting, float3(.33333, .33333, .33333)) * .5) + _ShadowOffset);
            }
            UNITY_BRANCH
            if(_LightingType == 0)
            {
                poiLight.finalLighting = lerp(poiLight.indirectLighting, poiLight.directLighting, lerp(1, poiLight.rampedLightMap, _ShadowStrength)) ;
            }
            UNITY_BRANCH
            if(_LightingType == 1)
            {
                float3 ramp0 = tex2D(_ToonRamp, float2(1, 1));
                poiLight.finalLighting = lerp(ramp0, poiLight.rampedLightMap, _ShadowStrength) * poiLight.directLighting;
            }
        }
    }
    
    void calculateLighting()
    {
        #ifdef OUTLINE
            _ShadowStrength = _OutlineShadowStrength;
        #endif
        #ifdef FORWARD_BASE_PASS
            calculateBasePassLighting();
        #else
            #if defined(POINT) || defined(SPOT)
                poiLight.finalLighting = poiLight.color * poiLight.attenuation * smoothstep(.5 - _AdditiveSoftness + _AdditiveOffset, .5 + _AdditiveSoftness + _AdditiveOffset, .5 * poiLight.nDotL + .5);
            #endif
        #endif
    }
    
    void applyLighting(inout float4 finalColor)
    {
        UNITY_BRANCH
        if(_LightingType == 2)
        {
            finalColor.rgb = calculateRealisticLighting(finalColor);
        }
        else
        {
            finalColor.rgb *= max(poiLight.finalLighting,_LightingMinLightBrightness);
        }
    }
#endif