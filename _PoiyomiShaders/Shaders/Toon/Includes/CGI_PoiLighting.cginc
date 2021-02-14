
#ifndef POI_LIGHTING
    #define POI_LIGHTING
    
    float _LightingShadingEnabled;
    float _LightingRampType;
    float _LightingIgnoreAmbientColor;
    float _UseShadowTexture;
    float _LightingEnableAO;
    float _LightingDetailShadowsEnabled;
    
    float _LightingOnlyUnityShadows;
    float _LightingMode;
    float _ForceLightDirection;
    float _ShadowStrength;
    float _OutlineShadowStrength;
    float _ShadowOffset;
    float3 _LightDirection;
    float _ForceShadowStrength;
    float _CastedShadowSmoothing;
    float _AttenuationMultiplier;
    float _EnableLighting;
    float _LightingControlledUseLightColor;
    fixed _LightingStandardSmoothness;
    fixed _LightingStandardControlsToon;
    fixed _LightingMinLightBrightness;
    float _LightingUseShadowRamp;
    float _LightingMinShadowBrightnessRatio;
    UNITY_DECLARE_TEX2D(_ToonRamp);
    fixed _LightingMonochromatic;
    
    fixed _LightingGradientStart;
    fixed _LightingGradientEnd;
    float3 _LightingShadowColor;
    POI_TEXTURE_NOSAMPLER(_LightingShadowTexture);
    float _AOStrength;
    fixed _LightingDetailStrength;
    fixed _LightingAdditiveDetailStrength;
    fixed _LightingNoIndirectMultiplier;
    fixed _LightingNoIndirectThreshold;
    float _LightingUncapped;
    
    float _LightingDirectColorMode;
    float _LightingIndirectColorMode;
    float _LightingAdditiveType;
    fixed _LightingAdditiveGradientStart;
    fixed _LightingAdditiveGradientEnd;
    fixed _LightingAdditivePassthrough;
    float _LightingDirectAdjustment;
    float _LightingIndirect;
    // HSL JUNK
    float _LightingEnableHSL;
    float _LightingShadowHue;
    float _LightingShadowSaturation;
    float _LightingShadowLightness;
    float _LightingHSLIntensity;
    /*
    UNITY_DECLARE_TEX2D_NOSAMPLER(_ToonRamp3);
    half _LightingShadowStrength3;
    half _ShadowOffset3;
    */
    
    half4 shadowStrength;
    
    POI_TEXTURE_NOSAMPLER(_LightingDetailShadows);
    POI_TEXTURE_NOSAMPLER(_LightingAOTex);
    POI_TEXTURE_NOSAMPLER(_LightingShadowMask);
    
    /*
    * MIT License
    *
    * Copyright (c) 2018 s-ilent
    *
    * Permission is hereby granted, free of charge, to any person obtaining a copy
    * of this software and associated documentation files (the "Software"), to deal
    * in the Software without restriction, including without limitation the rights
    * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    * copies of the Software, and to permit persons to whom the Software is
    * furnished to do so, subject to the following conditions:
    *
    * The above copyright notice and this permission notice shall be included in all
    * copies or substantial portions of the Software.
    *
    * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    * SOFTWARE.
    */
    
    /*
    * Silent's code starts here
    */
    
    float shEvaluateDiffuseL1Geomerics_local(float L0, float3 L1, float3 n)
    {
        // average energy
        float R0 = max(0, L0);
        
        // avg direction of incoming light
        float3 R1 = 0.5f * L1;
        
        // directional brightness
        float lenR1 = length(R1);
        
        // linear angle between normal and direction 0-1
        //float q = 0.5f * (1.0f + dot(R1 / lenR1, n));
        //float q = dot(R1 / lenR1, n) * 0.5 + 0.5;
        float q = dot(normalize(R1), n) * 0.5 + 0.5;
        q = saturate(q); // Thanks to ScruffyRuffles for the bug identity.
        
        // power for q
        // lerps from 1 (linear) to 3 (cubic) based on directionality
        float p = 1.0f + 2.0f * lenR1 / R0;
        
        // dynamic range constant
        // should vary between 4 (highly directional) and 0 (ambient)
        float a = (1.0f - lenR1 / R0) / (1.0f + lenR1 / R0);
        
        return R0 * (a + (1.0f - a) * (p + 1.0f) * pow(q, p));
    }
    
    half3 BetterSH9(half4 normal)
    {
        float3 indirect;
        float3 L0 = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w) + float3(unity_SHBr.z, unity_SHBg.z, unity_SHBb.z) / 3.0;
        indirect.r = shEvaluateDiffuseL1Geomerics_local(L0.r, unity_SHAr.xyz, normal);
        indirect.g = shEvaluateDiffuseL1Geomerics_local(L0.g, unity_SHAg.xyz, normal);
        indirect.b = shEvaluateDiffuseL1Geomerics_local(L0.b, unity_SHAb.xyz, normal);
        indirect = max(0, indirect);
        indirect += SHEvalLinearL2(normal);
        return indirect;
    }
    
    float3 BetterSH9(float3 normal)
    {
        return BetterSH9(float4(normal, 1));
    }
    
    /*
    * Standard stuff starts here
    */
    UnityLight CreateLight(float3 normal, fixed detailShadowMap)
    {
        UnityLight light;
        light.dir = poiLight.direction;
        light.color = saturate(_LightColor0.rgb * lerp(1, poiLight.attenuation, _AttenuationMultiplier) * detailShadowMap);
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
    
    float3 weightedBlend(float3 layer1, float3 layer2, float2 weights)
    {
        return(weights.x * layer1 + weights.y * layer2) / (weights.x + weights.y);
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
            float occlusion = 1;
            UNITY_BRANCH
            if(_LightingEnableAO)
            {
                occlusion = lerp(1, POI2D_SAMPLER_PAN(_LightingAOTex, _MainTex, poiMesh.uv[_LightingAOTexUV], _LightingAOTexPan), _AOStrength);
            }
            
            indirectLight.diffuse *= occlusion;
            indirectLight.diffuse = max(indirectLight.diffuse, _LightingMinLightBrightness);
            indirectLight.specular *= occlusion;
        #endif
        
        return indirectLight;
    }
    
    /*
    * Poiyomi's cool as heck code starts here :smug:
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
    
    float3 calculateRealisticLighting(float4 colorToLight, fixed detailShadowMap)
    {
        return UNITY_BRDF_PBS(1, 0, 0, _LightingStandardSmoothness, poiMesh.normals[1], poiCam.viewDir, CreateLight(poiMesh.normals[1], detailShadowMap), CreateIndirectLight(poiMesh.normals[1]));
    }
    
    void calculateBasePassLightMaps()
    {
        #if defined(FORWARD_BASE_PASS) || defined(POI_META_PASS)
            float AOMap = 1;
            float AOStrength = 1;
            float3 lightColor = poiLight.color;
            /*
            * Generate Basic Light Maps
            */
            
            bool lightExists = false;
            if (all(_LightColor0.rgb >= 0.002))
            {
                lightExists = true;
            }
            #ifndef OUTLINE
                UNITY_BRANCH
                if(_LightingEnableAO)
                {
                    AOMap = POI2D_SAMPLER_PAN(_LightingAOTex, _MainTex, poiMesh.uv[_LightingAOTexUV], _LightingAOTexPan);
                    AOStrength = _AOStrength;
                }
                
                #ifdef FORWARD_BASE_PASS
                    //poiLight.color = saturate(_LightColor0.rgb) + saturate(ShadeSH9(normalize(unity_SHAr + unity_SHAg + unity_SHAb)));
                    if (lightExists)
                    {
                        lightColor = _LightColor0.rgb + BetterSH9(float4(0, 0, 0, 1));
                    }
                    else
                    {
                        lightColor = max(BetterSH9(normalize(unity_SHAr + unity_SHAg + unity_SHAb)), 0);
                    }
                    
                    //lightColor = magic * magiratio + normalLight * normaRatio;
                    //lightColor = magic + normalLight;
                #endif
            #endif
            
            float3 grayscale_vector = float3(.33333, .33333, .33333);
            float3 ShadeSH9Plus = GetSHLength();
            float3 ShadeSH9Minus = BetterSH9(float4(0, 0, 0, 1));
            
            shadowStrength = 1;
            #ifndef OUTLINE
                shadowStrength = POI2D_SAMPLER_PAN(_LightingShadowMask, _MainTex, poiMesh.uv[_LightingShadowMaskUV], _LightingShadowMaskPan) * _ShadowStrength;
            #else
                shadowStrength = _OutlineShadowStrength;
            #endif
            
            float bw_lightColor = dot(lightColor, grayscale_vector);
            float bw_directLighting = (((poiLight.nDotL * 0.5 + 0.5) * bw_lightColor * lerp(1, poiLight.attenuation, _AttenuationMultiplier)) + dot(ShadeSH9Normal(poiMesh.normals[1]), grayscale_vector));
            float bw_bottomIndirectLighting = dot(ShadeSH9Minus, grayscale_vector);
            float bw_topIndirectLighting = dot(ShadeSH9Plus, grayscale_vector);
            float lightDifference = ((bw_topIndirectLighting + bw_lightColor) - bw_bottomIndirectLighting);
            
            fixed detailShadow = 1;
            UNITY_BRANCH
            if (_LightingDetailShadowsEnabled)
            {
                detailShadow = lerp(1, POI2D_SAMPLER_PAN(_LightingDetailShadows, _MainTex, poiMesh.uv[_LightingDetailShadowsUV], _LightingDetailShadowsPan), _LightingDetailStrength).r;
            }
            UNITY_BRANCH
            if(_LightingOnlyUnityShadows)
            {
                poiLight.lightMap = poiLight.attenuation;
            }
            else
            {
                poiLight.lightMap = smoothstep(0, lightDifference, bw_directLighting - bw_bottomIndirectLighting);
            }
            poiLight.lightMap *= detailShadow;
            
            /*
            * Decide on light colors
            */
            
            float3 indirectLighting = 0;
            float3 directLighting = 0;
            
            
            
            UNITY_BRANCH
            if (_LightingIndirectColorMode == 1)
            {
                indirectLighting = BetterSH9(float4(poiMesh.normals[0], 1));
            }
            else
            {
                indirectLighting = ShadeSH9Minus;
            }
            
            poiLight.directLighting = lightColor;
            poiLight.indirectLighting = indirectLighting;
            
            UNITY_BRANCH
            if(_LightingDirectColorMode == 0)
            {
                float3 magic = max(BetterSH9(normalize(unity_SHAr + unity_SHAg + unity_SHAb)), 0);
                float3 normalLight = _LightColor0.rgb + BetterSH9(float4(0, 0, 0, 1));
                
                float magiLumi = calculateluminance(magic);
                float normaLumi = calculateluminance(normalLight);
                float maginormalumi = magiLumi + normaLumi;
                
                float magiratio = magiLumi / maginormalumi;
                float normaRatio = normaLumi / maginormalumi;
                
                float target = calculateluminance(magic * magiratio + normalLight * normaRatio);
                float3 properLightColor = magic + normalLight;
                float properLuminance = calculateluminance(magic + normalLight);
                directLighting = properLightColor * max(0.0001, (target / properLuminance));
            }
            else
            {
                directLighting = lightColor;
            }
            
            UNITY_BRANCH
            if(!_LightingUncapped)
            {
                float directluminance = calculateluminance(directLighting);
                float indirectluminance = calculateluminance(indirectLighting);
                directLighting = min(directLighting, directLighting / max(0.0001, (directluminance / 1)));
                indirectLighting = min(indirectLighting, indirectLighting / max(0.0001, (indirectluminance / 1)));
            }
            
            directLighting = lerp(directLighting, dot(directLighting, float3(0.299, 0.587, 0.114)), _LightingMonochromatic);
            indirectLighting = lerp(indirectLighting, dot(indirectLighting, float3(0.299, 0.587, 0.114)), _LightingMonochromatic);
            
            if(max(max(indirectLighting.x, indirectLighting.y), indirectLighting.z) <= _LightingNoIndirectThreshold && max(max(directLighting.x, directLighting.y), directLighting.z) >= 0)
            {
                indirectLighting = directLighting * _LightingNoIndirectMultiplier;
            }
            
            UNITY_BRANCH
            if(_LightingMinShadowBrightnessRatio)
            {
                float directluminance = clamp(directLighting.r * 0.299 + directLighting.g * 0.587 + directLighting.b * 0.114, 0, 1);
                if(directluminance > 0)
                {
                    indirectLighting = max(0.001, indirectLighting);
                }
                float indirectluminance = clamp(indirectLighting.r * 0.299 + indirectLighting.g * 0.587 + indirectLighting.b * 0.114, 0, 1);
                float targetluminance = directluminance * _LightingMinShadowBrightnessRatio;
                if(indirectluminance < targetluminance)
                {
                    indirectLighting = indirectLighting / max(0.0001, indirectluminance / targetluminance);
                }
            }
            
            /*
            * Create Shade Maps
            */
            UNITY_BRANCH
            if (_LightingShadingEnabled)
            {
                switch(_LightingRampType)
                {
                    case 0: // Ramp Texture
                    {
                        poiLight.rampedLightMap = lerp(1, UNITY_SAMPLE_TEX2D(_ToonRamp, poiLight.lightMap + _ShadowOffset), shadowStrength.r);
                        UNITY_BRANCH
                        if (_LightingIgnoreAmbientColor)
                        {
                            poiLight.finalLighting = lerp(poiLight.rampedLightMap * directLighting * lerp(1, AOMap, AOStrength), directLighting, poiLight.rampedLightMap);
                        }
                        else
                        {
                            poiLight.finalLighting = lerp(indirectLighting * lerp(1, AOMap, AOStrength), directLighting, poiLight.rampedLightMap);
                        }
                    }
                    break;
                    case 1: // Math Gradient
                    {
                        poiLight.rampedLightMap = saturate(1 - smoothstep(_LightingGradientStart - .000001, _LightingGradientEnd, 1 - poiLight.lightMap));
                        float3 shadowColor = _LightingShadowColor;
                        UNITY_BRANCH
                        if (_UseShadowTexture)
                        {
                            shadowColor = 1;
                        }
                        UNITY_BRANCH
                        if(_LightingIgnoreAmbientColor)
                        {
                            poiLight.finalLighting = lerp((directLighting * shadowColor * lerp(1, AOMap, AOStrength)), (directLighting), saturate(poiLight.rampedLightMap + 1 - _ShadowStrength));
                        }
                        else
                        {
                            poiLight.finalLighting = lerp((indirectLighting * shadowColor * lerp(1, AOMap, AOStrength)), (directLighting), saturate(poiLight.rampedLightMap + 1 - _ShadowStrength));
                        }
                    }
                    break;
                }
            }
            else
            {
                poiLight.rampedLightMap = 1 - smoothstep(0, .5, 1 - poiLight.lightMap);
                poiLight.finalLighting = directLighting;
            }
            
            if(!_LightingUncapped)
            {
                poiLight.finalLighting = saturate(poiLight.finalLighting);
            }
            //poiLight.finalLighting *= .8;
        #endif
    }
    
    void applyShadowTexture(inout float4 albedo)
    {
        UNITY_BRANCH
        if (_UseShadowTexture && _LightingRampType == 1)
        {
            albedo.rgb = lerp(albedo.rgb, POI2D_SAMPLER_PAN(_LightingShadowTexture, _MainTex, poiMesh.uv[_LightingShadowTextureUV], _LightingShadowTexturePan) * _LightingShadowColor, (1 - poiLight.rampedLightMap) * shadowStrength);
        }
    }
    
    float3 calculateNonImportantLighting(float attenuation, float attenuationDotNL, float3 albedo, float3 lightColor, half dotNL)
    {
        fixed detailShadow = 1;
        UNITY_BRANCH
        if(_LightingDetailShadowsEnabled)
        {
            detailShadow = lerp(1, POI2D_SAMPLER_PAN(_LightingDetailShadows, _MainTex, poiMesh.uv[_LightingDetailShadowsUV], _LightingDetailShadowsPan), _LightingAdditiveDetailStrength).r;
        }
        UNITY_BRANCH
        if(_LightingAdditiveType == 0)
        {
            return lightColor * attenuationDotNL * detailShadow;
        }
        else
        {
            return lerp(lightColor * attenuation, lightColor * _LightingAdditivePassthrough * attenuation, smoothstep(_LightingAdditiveGradientStart, _LightingAdditiveGradientEnd, dotNL)) * detailShadow;
        }
    }
    
    
    float3 calculateFinalLighting(inout float3 albedo, float4 finalColor)
    {
        float3 finalLighting = 1;
        // Additive Lighting
        #ifdef FORWARD_ADD_PASS
            fixed detailShadow = 1;
            UNITY_BRANCH
            if (_LightingDetailShadowsEnabled)
            {
                detailShadow = lerp(1, POI2D_SAMPLER_PAN(_LightingDetailShadows, _MainTex, poiMesh.uv[_LightingDetailShadowsUV], _LightingDetailShadowsPan), _LightingAdditiveDetailStrength).r;
            }
            UNITY_BRANCH
            if(_LightingAdditiveType == 0)
            {
                finalLighting = poiLight.color * poiLight.attenuation * max(0, poiLight.nDotL) * detailShadow;
            }
            else
            {
                #if defined(POINT) || defined(SPOT)
                    finalLighting = lerp(poiLight.color * max(poiLight.additiveShadow, _LightingAdditivePassthrough), poiLight.color * _LightingAdditivePassthrough, smoothstep(_LightingAdditiveGradientStart, _LightingAdditiveGradientEnd, 1 - (.5 * poiLight.nDotL + .5))) * poiLight.attenuation * detailShadow;
                #else
                    finalLighting = lerp(poiLight.color * max(poiLight.attenuation, _LightingAdditivePassthrough), poiLight.color * _LightingAdditivePassthrough, smoothstep(_LightingAdditiveGradientStart, _LightingAdditiveGradientEnd, 1 - (.5 * poiLight.nDotL + .5))) * detailShadow;
                #endif
            }
        #endif
        
        // Base and Meta Lighting
        #if defined(FORWARD_BASE_PASS) || defined(POI_META_PASS)
            #ifdef VERTEXLIGHT_ON
                poiLight.vFinalLighting = 0;
                
                for (int index = 0; index < 4; index ++)
                {
                    poiLight.vFinalLighting += calculateNonImportantLighting(poiLight.vAttenuation[index], poiLight.vAttenuationDotNL[index], albedo, poiLight.vColor[index], poiLight.vCorrectedDotNL[index]);
                }
            #endif
            
            switch(_LightingMode)
            {
                case 0: // Toon Lighting
                {
                    // HSL Shading
                    UNITY_BRANCH
                    if (_LightingEnableHSL)
                    {
                        float3 HSLMod = float3(_LightingShadowHue * 2 - 1, _LightingShadowSaturation * 2 - 1, _LightingShadowLightness * 2 - 1) * (1 - poiLight.rampedLightMap);
                        albedo = lerp(albedo.rgb, ModifyViaHSL(albedo.rgb, HSLMod), _LightingHSLIntensity);
                    }
                    
                    // Normal Shading
                    UNITY_BRANCH
                    if (_LightingMinLightBrightness > 0)
                    {
                        poiLight.finalLighting = max(0.001, poiLight.finalLighting);
                        float finalluminance = calculateluminance(poiLight.finalLighting);
                        finalLighting = max(poiLight.finalLighting, poiLight.finalLighting / max(0.0001, (finalluminance / _LightingMinLightBrightness)));
                    }
                    else
                    {
                        finalLighting = poiLight.finalLighting;
                    }
                }
                break;
                case 1: // realistic
                {
                    fixed detailShadow = 1;
                    UNITY_BRANCH
                    if (_LightingDetailShadowsEnabled)
                    {
                        detailShadow = lerp(1, POI2D_SAMPLER_PAN(_LightingDetailShadows, _MainTex, poiMesh.uv[_LightingDetailShadowsUV], _LightingDetailShadowsPan), _LightingDetailStrength).r;
                    }
                    
                    float3 realisticLighting = calculateRealisticLighting(finalColor, detailShadow).rgb;
                    finalLighting = lerp(realisticLighting, dot(realisticLighting, float3(0.299, 0.587, 0.114)), _LightingMonochromatic);
                }
                break;
            }
        #endif
        return finalLighting;
    }
    
#endif