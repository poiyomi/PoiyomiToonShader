#ifndef POI_SPECULAR
    #define POI_SPECULAR
    int _SpecWhatTangent;
    int _SpecularType;
    int _SmoothnessFrom;
    POI_TEXTURE_NOSAMPLER(_SpecularMap);
    fixed _CenterOutSpecColor;
    POI_TEXTURE_NOSAMPLER(_SpecularAnisoJitterMicro);
    float _SpecularAnisoJitterMirrored;
    POI_TEXTURE_NOSAMPLER(_SpecularAnisoJitterMacro);
    POI_TEXTURE_NOSAMPLER(_SpecularAnisoFakeUV);
    POI_TEXTURE_NOSAMPLER(_AnisoTangentMap);
    POI_TEXTURE_NOSAMPLER(_SpecularMask);
    float _SpecularAnisoJitterMicroMultiplier;
    float _SpecularAnisoJitterMacroMultiplier;
    float4 _SpecularTint;
    float _SpecularSmoothness;
    float _Spec1Offset;
    float _Spec1JitterStrength;
    float _Spec2Smoothness;
    float _Spec2Offset;
    float _Spec2JitterStrength;
    float _AnisoUseTangentMap;
    float _AnisoSpec1Alpha;
    float _AnisoSpec2Alpha;
    float _SpecularInvertSmoothness;
    half _SpecularMetallic;
    uint _SpecularNormal;
    uint _SpecularNormal1;
    float _SpecularAttenuation;
    float _SpecularAttenuation1;
    // Toon
    fixed _SpecularToonStart, _SpecularToonEnd;
    half4 _SpecularToonInnerOuter;
    
    float _EnableSpecular1;
    int _SpecWhatTangent1;
    int _SpecularType1;
    int _SmoothnessFrom1;
    POI_TEXTURE_NOSAMPLER(_SpecularMap1);
    POI_TEXTURE_NOSAMPLER(_SpecularAnisoJitterMicro1);
    POI_TEXTURE_NOSAMPLER(_SpecularAnisoJitterMacro1);
    float _SpecularAnisoJitterMirrored1;
    POI_TEXTURE_NOSAMPLER(_AnisoTangentMap1);
    POI_TEXTURE_NOSAMPLER(_SpecularMask1);
    float _SpecularAnisoJitterMicroMultiplier1;
    float _SpecularAnisoJitterMacroMultiplier1;
    float4 _SpecularTint1;
    float _SpecularSmoothness1;
    float _Spec1Offset1;
    float _Spec1JitterStrength1;
    float _Spec2Smoothness1;
    float _Spec2Offset1;
    float _Spec2JitterStrength1;
    float _AnisoUseTangentMap1;
    float _AnisoSpec1Alpha1;
    float _AnisoSpec2Alpha1;
    float _SpecularInvertSmoothness1;
    half _SpecularMetallic1;
    // Toon
    half4 _SpecularToonInnerOuter1;
    fixed _SpecularToonStart1, _SpecularToonEnd1;
    
    UnityIndirect ZeroIndirect()
    {
        UnityIndirect ind;
        ind.diffuse = 0;
        ind.specular = 0;
        return ind;
    }
    
    // From unity just putting it here in case I want to mod it
    half4 poiRealisticSpecular(half3 diffColor, half3 specColor, half oneMinusReflectivity, half smoothness,
    float3 normal, float3 halfDir,
    UnityLight light, UnityIndirect gi)
    {
        float perceptualRoughness = SmoothnessToPerceptualRoughness(smoothness);
        #define UNITY_HANDLE_CORRECTLY_NEGATIVE_NDOTV 0
        
        #if UNITY_HANDLE_CORRECTLY_NEGATIVE_NDOTV
            half shiftAmount = dot(normal, poiCam.viewDir);
            normal = shiftAmount < 0.0f ? normal + poiCam.viewDir * (-shiftAmount + 1e-5f): normal;
            float nv = saturate(dot(normal, poiCam.viewDir));
        #else
            half nv = abs(dot(normal, poiCam.viewDir));
        #endif
        
        float nl = saturate(dot(normal, light.dir));
        float nh = saturate(dot(normal, halfDir));
        
        half lv = saturate(dot(light.dir, poiCam.viewDir));
        half lh = saturate(dot(light.dir, halfDir));
        
        half diffuseTerm = DisneyDiffuse(nv, nl, lh, perceptualRoughness) * nl;
        
        float roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
        
        roughness = max(roughness, 0.002);
        float V = SmithJointGGXVisibilityTerm(nl, nv, roughness);
        float D = GGXTerm(nh, roughness);
        
        float specularTerm = V * D * UNITY_PI;
        
        #ifdef UNITY_COLORSPACE_GAMMA
            specularTerm = sqrt(max(1e-4h, specularTerm));
        #endif
        
        specularTerm = max(0, specularTerm * nl);
        #if defined(_POI_SPECULARHIGHLIGHTS_OFF)
            specularTerm = 0.0;
        #endif
        
        half surfaceReduction;
        #ifdef UNITY_COLORSPACE_GAMMA
            surfaceReduction = 1.0 - 0.28 * roughness * perceptualRoughness;
        #else
            surfaceReduction = 1.0 / (roughness * roughness + 1.0);
        #endif
        
        specularTerm *= any(specColor) ? 1.0: 0.0;
        
        half grazingTerm = saturate(smoothness + (1 - oneMinusReflectivity));
        half3 color = diffColor * (gi.diffuse + light.color * diffuseTerm)
        + specularTerm * light.color * FresnelTerm(specColor, lh)
        + surfaceReduction * gi.specular * FresnelLerp(specColor, grazingTerm, nv);
        
        return half4(color, 1);
    }
    
    half3 calculateRealisticSpecular(float4 albedo, float2 uv, float4 specularTint, float specularSmoothness, float invertSmoothness, float mixAlbedoWithTint, float4 specularMap, float3 specularLight, float3 normal, float attenuation, float3 lightDirection, float nDotL, float3 halfDir)
    {
        half oneMinusReflectivity;
        half3 finalSpecular;
        UnityLight unityLight;
        unityLight.color = specularLight;
        unityLight.dir = lightDirection;
        unityLight.ndotl = nDotL;
        
        UNITY_BRANCH
        if (_SmoothnessFrom == 0)
        {
            half3 diffColor = EnergyConservationBetweenDiffuseAndSpecular(albedo, specularMap.rgb * specularTint.rgb, /*out*/ oneMinusReflectivity);
            finalSpecular = poiRealisticSpecular(diffColor, specularMap.rgb, oneMinusReflectivity, specularMap.a * specularSmoothness * lerp(1, -1, invertSmoothness), normal, halfDir, unityLight, ZeroIndirect());
        }
        else
        {
            half3 diffColor = EnergyConservationBetweenDiffuseAndSpecular(albedo, specularTint.rgb, /*out*/ oneMinusReflectivity);
            float smoothness = max(max(specularMap.r, specularMap.g), specularMap.b);
            finalSpecular = poiRealisticSpecular(diffColor, 1, oneMinusReflectivity, smoothness * specularSmoothness * lerp(1, -1, invertSmoothness), normal, halfDir, unityLight, ZeroIndirect());
        }
        finalSpecular *= lerp(1, albedo.rgb, mixAlbedoWithTint);
        return finalSpecular;
    }
    
    half3 calculateToonSpecular(float4 albedo, float2 uv, float2 specularToonInnerOuter, float specularMixAlbedoIntoTint, float smoothnessFrom, float4 specularMap, float3 specularLight, float3 normal, float3 halfDir, float attenuation)
    {
        half3 finalSpecular = smoothstep(1 - specularToonInnerOuter.y, 1 - specularToonInnerOuter.x, dot(halfDir, normal) * lerp(1, attenuation, _SpecularAttenuation)) * specularLight;
        UNITY_BRANCH
        if (smoothnessFrom == 0)
        {
            finalSpecular.rgb *= specularMap.rgb * lerp(1, albedo.rgb, specularMixAlbedoIntoTint);
            finalSpecular *= specularMap.a;
        }
        else
        {
            finalSpecular *= specularMap.r * lerp(1, albedo.rgb, specularMixAlbedoIntoTint);
        }
        return finalSpecular;
    }
    
    float3 strandSpecular(float TdotL, float TdotV, float specPower, float nDotL)
    {
        #if defined(POINT) || defined(SPOT)
            nDotL *= poiLight.attenuation * poiLight.additiveShadow;
        #endif
        float Specular = saturate(nDotL) * pow(saturate(sqrt(1.0 - (TdotL * TdotL)) * sqrt(1.0 - (TdotV * TdotV)) - TdotL * TdotV), specPower);
        half normalization = sqrt((specPower + 1) * ((specPower) + 1)) / (8 * pi);
        Specular *= normalization;
        return Specular;
    }
    
    half3 AnisotropicSpecular(
        float specWhatTangent, float anisoUseTangentMap, float specularSmoothness, float spec2Smoothness,
        float anisoSpec1Alpha, float anisoSpec2Alpha, float4 specularTint, float specularMixAlbedoIntoTint, float4 specularMap, float3 specularLight, float3 lightDirection, float3 halfDir, float nDotL, float jitter, float4 packedTangentMap)
    {
        float3 tangentOrBinormal = specWhatTangent ? poiMesh.tangent: poiMesh.binormal;
        
        
        float3 normalLocalAniso = lerp(float3(0, 0, 1), UnpackNormal(packedTangentMap), anisoUseTangentMap);
        normalLocalAniso = BlendNormals(normalLocalAniso, poiMesh.tangentSpaceNormal);
        //float3 normalDirection = normalize(mul(poiMesh.normals[_SpecularNormal], poiTData.tangentTransform));
        float3 normalDirectionAniso = Unity_SafeNormalize(mul(normalLocalAniso, poiTData.tangentTransform));
        float3 tangentDirection = mul(poiTData.tangentTransform, tangentOrBinormal).xyz;
        float3 viewReflectDirectionAniso = reflect(-poiCam.viewDir, normalDirectionAniso); // possible bad negation
        float3 tangentDirectionMap = mul(poiTData.tangentToWorld, float3(normalLocalAniso.rg, 0.0)).xyz;
        tangentDirectionMap = normalize(lerp(tangentOrBinormal, tangentDirectionMap, anisoUseTangentMap));
        
        tangentDirectionMap += _Spec1Offset +jitter;
        
        float TdotL = dot(lightDirection, tangentDirectionMap);
        float TdotV = dot(poiCam.viewDir, tangentDirectionMap);
        float TdotH = dot(halfDir, tangentDirectionMap);
        half specPower = RoughnessToSpecPower(1.0 - specularSmoothness * specularMap.a);
        half spec2Power = RoughnessToSpecPower(1.0 - spec2Smoothness * specularMap.a);
        half Specular = 0;
        
        float3 spec = strandSpecular(TdotL, TdotV, specPower, nDotL) * anisoSpec1Alpha;
        float3 spec2 = strandSpecular(TdotL, TdotV, spec2Power, nDotL) * anisoSpec2Alpha;
        
        return max(spec, spec2) * specularMap.rgb * specularTint.a * specularLight * lerp(1, albedo.rgb, specularMixAlbedoIntoTint);
    }
    
    inline float3 toonAnisoSpecular(float specWhatTangent, float anisoUseTangentMap, float3 lightDirection, float halfDir, float4 specularMap, float nDotL, fixed gradientStart, fixed gradientEnd, float4 specColor, float4 finalColor, fixed metallic, float jitter, float mirrored, float4 packedTangentMap)
    {
        float3 tangentOrBinormal = specWhatTangent ? poiMesh.tangent: poiMesh.binormal;
        
        float3 normalLocalAniso = lerp(float3(0, 0, 1), UnpackNormal(packedTangentMap), anisoUseTangentMap);
        normalLocalAniso = BlendNormals(normalLocalAniso, poiMesh.tangentSpaceNormal);
        //float3 normalDirection = normalize(mul(poiMesh.normals[_SpecularNormal], poiTData.tangentTransform));
        float3 normalDirectionAniso = Unity_SafeNormalize(mul(normalLocalAniso, poiTData.tangentTransform));
        float3 tangentDirection = mul(poiTData.tangentTransform, tangentOrBinormal).xyz;
        float3 viewReflectDirectionAniso = reflect(-poiCam.viewDir, normalDirectionAniso); // possible bad negation
        float3 tangentDirectionMap = mul(poiTData.tangentToWorld, float3(normalLocalAniso.rg, 0.0)).xyz;
        tangentDirectionMap = normalize(lerp(tangentOrBinormal, tangentDirectionMap, anisoUseTangentMap));
        
        if (!mirrored)
        {
            tangentDirectionMap += jitter;
        }
        
        float TdotL = dot(lightDirection, tangentDirectionMap);
        float TdotV = dot(poiCam.viewDir, tangentDirectionMap);
        float TdotH = dot(halfDir, tangentDirectionMap);
        
        float specular = saturate(sqrt(1.0 - (TdotL * TdotL)) * sqrt(1.0 - (TdotV * TdotV)) - TdotL * TdotV);
        
        fixed smoothAlpha = specular;
        if(mirrored)
        {
            smoothAlpha = max(specular - jitter, 0);
        }
        
        specular = smoothstep(gradientStart, gradientEnd, smoothAlpha);
        
        /*
        UNITY_BRANCH
        if(_CenterOutSpecColor)
        {
            specularMap = POI2D_SAMPLER_PAN(_SpecularMap, _MainTex, clamp(float2(specular, specular), 0.01, .99), _SpecularMapPan);
        }
        */
        
        #if defined(POINT) || defined(SPOT)
            nDotL *= poiLight.attenuation * poiLight.additiveShadow;
        #endif
        
        return saturate(nDotL) * specular * poiLight.color * specColor * specularMap.rgb * lerp(1, finalColor, metallic) * specularMap.a;
    }
    
    inline float SpecularHQ(half roughness, half dotNH, half dotLH)
    {
        roughness = saturate(roughness);
        roughness = max((roughness * roughness), 0.002);
        half roughnessX2 = roughness * roughness;
        
        half denom = dotNH * dotNH * (roughnessX2 - 1.0) + 1.0f;
        half D = roughnessX2 / (3.14159 * denom * denom);
        
        half k = roughness / 2.0f;
        half k2 = k * k;
        half invK2 = 1.0f - k2;
        
        half vis = rcp(dotLH * dotLH * invK2 + k2);
        
        float specTerm = vis * D;
        
        return specTerm;
    }
    
    float3 calculateNewSpecular(in float3 specularMap, uint colorFrom, in float4 albedo, in float3 specularTint, in float specularMetallic, in float specularSmoothness, in half dotNH, in half dotLH, in float3 lightColor, in float attenuation)
    {
        float3 specColor = specularTint;
        float metallic = specularMetallic;
        float roughness = 1 - specularSmoothness;
        float perceptualRoughness = roughness;
        //float reflectInverse = DielectricSpec.a - metallic * DielectricSpec.a;
        //float reflectivity = 1.0h - reflectInverse;
        float3 specMapColor = lerp(specularMap, 1, colorFrom);
        float3 specularColor = lerp(DielectricSpec.rgb * specMapColor, lerp(specularMap, albedo.rgb, colorFrom), metallic);
        //albedo.rgb *= reflectInverse;
        
        return specularColor * lightColor * attenuation * specularTint * SpecularHQ(perceptualRoughness, dotNH, dotLH);
    }
    
    float3 calculateSpecular(in float4 albedo)
    {
        half3 finalSpecular = 0;
        half3 finalSpecular1 = 0;
        float4 realisticAlbedo = albedo;
        float4 realisticAlbedo1 = albedo;
        float4 specularMap = POI2D_SAMPLER_PAN(_SpecularMap, _MainTex, poiMesh.uv[_SpecularMapUV], _SpecularMapPan);
        half specularMask = POI2D_SAMPLER_PAN(_SpecularMask, _MainTex, poiMesh.uv[_SpecularMaskUV], _SpecularMaskPan).r;
        float attenuation = poiLight.attenuation;
        
        UNITY_BRANCH
        if (_SpecularType == 1) // Realistic
        {
            if (_SmoothnessFrom == 1)
            {
                specularMap.a = specularMap.r;
                specularMap.rgb = 1;
            }
            
            if(_SpecularInvertSmoothness)
            {
                specularMap.a = 1 - specularMap.a;
            }
            
            #ifdef FORWARD_BASE_PASS
                finalSpecular += calculateNewSpecular(specularMap.rgb, _SmoothnessFrom, realisticAlbedo, _SpecularTint, _SpecularMetallic, _SpecularSmoothness * specularMap.a, poiLight.dotNH, poiLight.dotLH, poiLight.color, saturate(poiLight.nDotL) * lerp(1, attenuation, _SpecularAttenuation));
            #else
                finalSpecular += calculateNewSpecular(specularMap.rgb, _SmoothnessFrom, realisticAlbedo, _SpecularTint, _SpecularMetallic, _SpecularSmoothness * specularMap.a, poiLight.dotNH, poiLight.dotLH, poiLight.color, lerp(1, attenuation, _SpecularAttenuation));
            #endif
        }
        
        UNITY_BRANCH
        if(_SpecularType == 4)
        {
            float jitter = 0;
            float microJitter = POI2D_SAMPLER_PAN(_SpecularAnisoJitterMicro, _MainTex, float2(poiMesh.uv[_SpecularAnisoJitterMicroUV]), _SpecularAnisoJitterMicroPan).r;
            fixed jitterOffset = (1 - _SpecularAnisoJitterMirrored) * .5;
            jitter += (POI2D_SAMPLER_PAN(_SpecularAnisoJitterMicro, _MainTex, float2(poiMesh.uv[_SpecularAnisoJitterMicroUV]), _SpecularAnisoJitterMicroPan).r - jitterOffset) * _SpecularAnisoJitterMicroMultiplier;
            jitter += (POI2D_SAMPLER_PAN(_SpecularAnisoJitterMacro, _MainTex, float2(poiMesh.uv[_SpecularAnisoJitterMacroUV]), _SpecularAnisoJitterMacroPan).r - jitterOffset) * _SpecularAnisoJitterMacroMultiplier;
            jitter += _Spec1Offset;
            
            float4 packedTangentMap = POI2D_SAMPLER_PAN(_AnisoTangentMap, _MainTex, poiMesh.uv[_AnisoTangentMapUV], _AnisoTangentMapPan);
            
            finalSpecular += toonAnisoSpecular(_SpecWhatTangent, _AnisoUseTangentMap, poiLight.direction, poiLight.halfDir, specularMap, poiLight.nDotL, _SpecularToonStart, _SpecularToonEnd, _SpecularTint, albedo, _SpecularMetallic, jitter, _SpecularAnisoJitterMirrored, packedTangentMap);
            finalSpecular *= lerp(1, poiLight.attenuation, _SpecularAttenuation);
        }
        
        #ifdef FORWARD_BASE_PASS
            UNITY_BRANCH
            if(_SpecularType == 2) // Toon
            {
                finalSpecular += calculateToonSpecular(albedo, poiMesh.uv[0], _SpecularToonInnerOuter, _SpecularMetallic, _SmoothnessFrom, specularMap, poiLight.color, poiMesh.normals[_SpecularNormal], poiLight.halfDir, poiLight.attenuation);
                finalSpecular *= _SpecularTint;
            }
            UNITY_BRANCH
            if (_SpecularType == 3) // anisotropic
            {
                float jitter = 0;
                float microJitter = POI2D_SAMPLER_PAN(_SpecularAnisoJitterMicro, _MainTex, float2(poiMesh.uv[_SpecularAnisoJitterMicroUV]), _SpecularAnisoJitterMicroPan).r;
                fixed jitterOffset = (1 - _SpecularAnisoJitterMirrored) * .5;
                jitter += (POI2D_SAMPLER_PAN(_SpecularAnisoJitterMicro, _MainTex, float2(poiMesh.uv[_SpecularAnisoJitterMicroUV]), _SpecularAnisoJitterMicroPan).r - jitterOffset) * _SpecularAnisoJitterMicroMultiplier;
                jitter += (POI2D_SAMPLER_PAN(_SpecularAnisoJitterMacro, _MainTex, float2(poiMesh.uv[_SpecularAnisoJitterMacroUV]), _SpecularAnisoJitterMacroPan).r - jitterOffset) * _SpecularAnisoJitterMacroMultiplier;
                jitter += _Spec1Offset;
                
                float4 packedTangentMap = POI2D_SAMPLER_PAN(_AnisoTangentMap, _MainTex, poiMesh.uv[_AnisoTangentMapUV], _AnisoTangentMapPan);
                
                finalSpecular += AnisotropicSpecular(_SpecWhatTangent, _AnisoUseTangentMap, _SpecularSmoothness, _Spec2Smoothness, _AnisoSpec1Alpha, _AnisoSpec2Alpha, _SpecularTint, _SpecularMetallic, specularMap, poiLight.color, poiLight.direction, poiLight.halfDir, poiLight.nDotL, jitter, packedTangentMap);
                finalSpecular *= _SpecularTint;
                finalSpecular *= lerp(1, poiLight.attenuation, _SpecularAttenuation);
            }
        #endif
        
        #ifdef VERTEXLIGHT_ON
            // Non Important Lights
            for (int index = 0; index < 4; index ++)
            {
                attenuation = poiLight.vAttenuationDotNL[index];
                UNITY_BRANCH
                if (_SpecularType == 1) // Realistic
                {
                    finalSpecular += calculateNewSpecular(specularMap.rgb, _SmoothnessFrom, realisticAlbedo, _SpecularTint, _SpecularMetallic, _SpecularSmoothness * specularMap.a, poiLight.vDotNH[index], poiLight.vDotLH[index], poiLight.vColor[index], poiLight.vAttenuationDotNL[index]);
                }
            }
        #endif
        
        finalSpecular *= _SpecularTint.a;
        finalSpecular = finalSpecular.rgb;
        finalSpecular *= specularMask;
        
        UNITY_BRANCH
        if (_EnableSpecular1)
        {
            float4 specularMap1 = POI2D_SAMPLER_PAN(_SpecularMap1, _MainTex, poiMesh.uv[_SpecularMap1UV], _SpecularMap1Pan);
            half specularMask1 = POI2D_SAMPLER_PAN(_SpecularMask1, _MainTex, poiMesh.uv[_SpecularMask1UV], _SpecularMask1Pan).r;
            float attenuation = poiLight.attenuation;
            UNITY_BRANCH
            if(_SpecularType1 == 1) // Realistic
            {
                UNITY_BRANCH
                if (_SmoothnessFrom1 == 1)
                {
                    specularMap1.a = specularMap1.r;
                    specularMap1.rgb = 1;
                }
                else
                {
                    realisticAlbedo1.rgb = specularMap1.rgb;
                }
                
                UNITY_BRANCH
                if(_SpecularInvertSmoothness1)
                {
                    specularMap1.a = 1 - specularMap1.a;
                }
                
                #ifdef FORWARD_BASE_PASS
                    finalSpecular1 = calculateNewSpecular(specularMap1.rgb, _SmoothnessFrom1, realisticAlbedo1, _SpecularTint1, _SpecularMetallic1, _SpecularSmoothness1 * specularMap1.a, poiLight.dotNH, poiLight.dotLH, poiLight.color, saturate(poiLight.nDotL) * lerp(1, attenuation, _SpecularAttenuation1));
                #else
                    finalSpecular1 = calculateNewSpecular(specularMap1.rgb, _SmoothnessFrom1, realisticAlbedo1, _SpecularTint1, _SpecularMetallic1, _SpecularSmoothness1 * specularMap1.a, poiLight.dotNH, poiLight.dotLH, poiLight.color, lerp(1, attenuation, _SpecularAttenuation1));
                #endif
            }
            
            UNITY_BRANCH
            if(_SpecularType1 == 4)
            {
                float jitter = 0;
                float microJitter = POI2D_SAMPLER_PAN(_SpecularAnisoJitterMicro1, _MainTex, float2(poiMesh.uv[_SpecularAnisoJitterMicro1UV]), _SpecularAnisoJitterMicro1Pan).r;
                fixed jitterOffset = (1 - _SpecularAnisoJitterMirrored1) * .5;
                jitter += (POI2D_SAMPLER_PAN(_SpecularAnisoJitterMicro1, _MainTex, float2(poiMesh.uv[_SpecularAnisoJitterMicro1UV]), _SpecularAnisoJitterMicro1Pan).r - jitterOffset) * _SpecularAnisoJitterMicroMultiplier1;
                jitter += (POI2D_SAMPLER_PAN(_SpecularAnisoJitterMacro1, _MainTex, float2(poiMesh.uv[_SpecularAnisoJitterMacro1UV]), _SpecularAnisoJitterMacro1Pan).r - jitterOffset) * _SpecularAnisoJitterMacroMultiplier1;
                jitter += _Spec1Offset1;
                
                float4 packedTangentMap = POI2D_SAMPLER_PAN(_AnisoTangentMap1, _MainTex, poiMesh.uv[_AnisoTangentMap1UV], _AnisoTangentMap1Pan);
                
                finalSpecular1 += toonAnisoSpecular(_SpecWhatTangent1, _AnisoUseTangentMap1, poiLight.direction, poiLight.halfDir, specularMap1, poiLight.nDotL, _SpecularToonStart1, _SpecularToonEnd1, _SpecularTint1, albedo, _SpecularMetallic1, jitter, _SpecularAnisoJitterMirrored1, packedTangentMap);
                finalSpecular1 *= lerp(1, poiLight.attenuation, _SpecularAttenuation1);
            }
            
            UNITY_BRANCH
            if(_SpecularType1 == 2) // Toon
            {
                finalSpecular1 = calculateToonSpecular(albedo, poiMesh.uv[0], _SpecularToonInnerOuter1, _SpecularMetallic1, _SmoothnessFrom1, specularMap1, poiLight.color, poiMesh.normals[_SpecularNormal1], poiLight.halfDir, poiLight.attenuation);
                finalSpecular1 *= _SpecularTint1;
            }
            UNITY_BRANCH
            if (_SpecularType1 == 3) // anisotropic
            {
                float jitter = 0;
                float microJitter = POI2D_SAMPLER_PAN(_SpecularAnisoJitterMicro1, _MainTex, float2(poiMesh.uv[_SpecularAnisoJitterMicro1UV]), _SpecularAnisoJitterMicro1Pan).r;
                fixed jitterOffset = (1 - _SpecularAnisoJitterMirrored1) * .5;
                jitter += (POI2D_SAMPLER_PAN(_SpecularAnisoJitterMicro1, _MainTex, float2(poiMesh.uv[_SpecularAnisoJitterMicro1UV]), _SpecularAnisoJitterMicro1Pan).r - jitterOffset) * _SpecularAnisoJitterMicroMultiplier1;
                jitter += (POI2D_SAMPLER_PAN(_SpecularAnisoJitterMacro1, _MainTex, float2(poiMesh.uv[_SpecularAnisoJitterMacro1UV]), _SpecularAnisoJitterMacro1Pan).r - jitterOffset) * _SpecularAnisoJitterMacroMultiplier1;
                jitter += _Spec1Offset1;
                
                float4 packedTangentMap = POI2D_SAMPLER_PAN(_AnisoTangentMap1, _MainTex, poiMesh.uv[_AnisoTangentMap1UV], _AnisoTangentMap1Pan);
                
                finalSpecular1 = AnisotropicSpecular(_SpecWhatTangent1, _AnisoUseTangentMap1, _SpecularSmoothness1, _Spec2Smoothness1, _AnisoSpec1Alpha1, _AnisoSpec2Alpha1, _SpecularTint1, _SpecularMetallic1, specularMap1, poiLight.color, poiLight.direction, poiLight.halfDir, poiLight.nDotL, jitter, packedTangentMap);
                finalSpecular1 *= _SpecularTint1;
                finalSpecular1 *= lerp(1, poiLight.attenuation, _SpecularAttenuation1);
            }
            
            #ifdef FORWARD_BASE_PASS
                // Non Important Lights
                #ifdef VERTEXLIGHT_ON
                    for (int index = 0; index < 4; index ++)
                    {
                        attenuation = poiLight.vAttenuationDotNL[index];
                        UNITY_BRANCH
                        if (_SpecularType == 1) // Realistic
                        {
                            finalSpecular1 += calculateNewSpecular(specularMap1.rgb, _SmoothnessFrom1, realisticAlbedo1, _SpecularTint1, _SpecularMetallic1, _SpecularSmoothness1 * specularMap1.a, poiLight.vDotNH[index], poiLight.vDotLH[index], poiLight.vColor[index], poiLight.vAttenuationDotNL[index]);
                        }
                    }
                #endif
            #endif
            
            finalSpecular1 *= _SpecularTint1.a;
            finalSpecular1 = finalSpecular1.rgb;
            finalSpecular1 *= specularMask1;
        }
        return finalSpecular + finalSpecular1;
    }
    
#endif