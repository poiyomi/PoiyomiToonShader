#ifndef POI_SPECULAR
    #define POI_SPECULAR
    int _SpecWhatTangent;
    int _SpecularType;
    int _SmoothnessFrom;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_SpecularMap); float4 _SpecularMap_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_ShiftTexture); float4 _ShiftTexture_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_AnisoTangentMap); float4 _AnisoTangentMap_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_SpecularMask); float4 _SpecularMask_ST;
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
    half _SpecularMixAlbedoIntoTint;
    float _SpecularMinLightBrightness;
    uint _SpecularNormal;
    uint _SpecularNormal1;
    float _SpecularAttenuation;
    float _SpecularAttenuation1;
    // Toon
    half4 _SpecularToonInnerOuter;
    
    UnityIndirect ZeroIndirect()
    {
        UnityIndirect ind;
        ind.diffuse = 0;
        ind.specular = 0;
        return ind;
    }
    
    // From unity just putting it here in case I want to mod it
    half4 poiRealisticSpecular(half3 diffColor, half3 specColor, half oneMinusReflectivity, half smoothness,
    float3 normal,
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
        float nh = saturate(dot(normal, poiLight.halfDir));
        
        half lv = saturate(dot(light.dir, poiCam.viewDir));
        half lh = saturate(dot(light.dir, poiLight.halfDir));
        
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
    
    half3 calculateRealisticSpecular(float4 albedo, float2 uv, float4 specularTint, float specularSmoothness, float invertSmoothness, float mixAlbedoWithTint, float4 specularMap, float3 specularLight, float3 normal)
    {
        half oneMinusReflectivity;
        half3 finalSpecular;
        UnityLight unityLight;
        unityLight.color = specularLight;
        unityLight.dir = poiLight.direction;
        unityLight.ndotl = poiLight.nDotL;
        
        UNITY_BRANCH
        if (_SmoothnessFrom == 0)
        {
            half3 diffColor = EnergyConservationBetweenDiffuseAndSpecular(albedo, specularMap.rgb * specularTint.rgb, /*out*/ oneMinusReflectivity);
            finalSpecular = poiRealisticSpecular(diffColor, specularMap.rgb, oneMinusReflectivity, specularMap.a * specularSmoothness * lerp(1, -1, invertSmoothness), normal, unityLight, ZeroIndirect());
        }
        else
        {
            half3 diffColor = EnergyConservationBetweenDiffuseAndSpecular(albedo, specularTint.rgb, /*out*/ oneMinusReflectivity);
            float smoothness = max(max(specularMap.r, specularMap.g), specularMap.b);
            finalSpecular = poiRealisticSpecular(diffColor, 1, oneMinusReflectivity, smoothness * specularSmoothness * lerp(1, -1, invertSmoothness), normal, unityLight, ZeroIndirect());
        }
        finalSpecular *= lerp(1, albedo.rgb, mixAlbedoWithTint);
        return finalSpecular;
    }
    
    half3 calculateToonSpecular(float4 albedo, float2 uv, float2 specularToonInnerOuter, float specularMixAlbedoIntoTint, float smoothnessFrom, float4 specularMap, float3 specularLight, float3 normal)
    {
        /*
        finalSpecular = 1;
        calculateRealisticSpecular(albedo, uv);
        float specIntensity = dot(finalSpecular.rgb, grayscale_for_light());
        finalSpecular.rgb = smoothstep(0.99, 1, specIntensity) * poiLight.color.rgb * max(_SpecularMinLightBrightness,lerp(1,poiLight.attenuation,_SpecularAttenuation));
        */
        half3 finalSpecular = smoothstep(1 - specularToonInnerOuter.y, 1 - specularToonInnerOuter.x, dot(poiLight.halfDir, normal) * max(_SpecularMinLightBrightness,lerp(1,poiLight.attenuation,_SpecularAttenuation))) * specularLight;
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
    
    float3 strandSpecular(float TdotL, float TdotV, float specPower)
    {
        float Specular = saturate(poiLight.nDotL) * pow(saturate(sqrt(1.0 - (TdotL * TdotL)) * sqrt(1.0 - (TdotV * TdotV)) - TdotL * TdotV), specPower);
        half normalization = sqrt((specPower + 1) * ((specPower) + 1)) / (8 * pi);//
        Specular *= normalization;
        return Specular;
    }
    
    half3 AnisotropicSpecular(
        float specWhatTangent, float anisoUseTangentMap, float specularSmoothness, float spec2Smoothness,
        float anisoSpec1Alpha, float anisoSpec2Alpha, float4 specularTint, float specularMixAlbedoIntoTint, float4 specularMap, float3 specularLight)
    {
        float3 tangentOrBitangent = specWhatTangent ? poiMesh.tangent: poiMesh.bitangent;
        
        
        float4 packedTangentMap = UNITY_SAMPLE_TEX2D_SAMPLER(_AnisoTangentMap, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _AnisoTangentMap));
        float3 normalLocalAniso = lerp(float3(0, 0, 1), UnpackNormal(packedTangentMap), anisoUseTangentMap);
        normalLocalAniso = BlendNormals(normalLocalAniso, poiMesh.tangentSpaceNormal);
        //float3 normalDirection = normalize(mul(poiMesh.normals[_SpecularNormal], poiTData.tangentTransform));
        float3 normalDirectionAniso = Unity_SafeNormalize(mul(normalLocalAniso, poiTData.tangentTransform));
        float3 tangentDirection = mul(poiTData.tangentTransform, tangentOrBitangent).xyz;
        float3 viewReflectDirectionAniso = reflect(-poiCam.viewDir, normalDirectionAniso); // possible bad negation
        float3 tangentDirectionMap = mul(poiTData.tangentToWorld, float3(normalLocalAniso.rg, 0.0)).xyz;
        tangentDirectionMap = normalize(lerp(tangentOrBitangent, tangentDirectionMap, anisoUseTangentMap));
        float TdotL = dot(poiLight.direction, tangentDirectionMap);
        float TdotV = dot(poiCam.viewDir, tangentDirectionMap);
        float TdotH = dot(poiLight.halfDir, tangentDirectionMap);
        half specPower = RoughnessToSpecPower(1.0 - specularSmoothness * specularMap.a);
        half spec2Power = RoughnessToSpecPower(1.0 - spec2Smoothness * specularMap.a);
        half Specular = 0;
        
        float3 spec = strandSpecular(TdotL, TdotV, specPower) * anisoSpec1Alpha;
        float3 spec2 = strandSpecular(TdotL, TdotV, spec2Power) * anisoSpec2Alpha;
        
        return max(spec, spec2) * specularMap.rgb * specularTint.a * specularLight * lerp(1, albedo.rgb, specularMixAlbedoIntoTint);
    }
    
    float3 calculateSpecular0(float4 albedo)
    {
        half3 finalSpecular = 0;
        float4 specularMap = UNITY_SAMPLE_TEX2D_SAMPLER(_SpecularMap, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _SpecularMap));
        float3 specularLight = float3(max(poiLight.color.r, _SpecularMinLightBrightness), max(poiLight.color.g, _SpecularMinLightBrightness), max(poiLight.color.b, _SpecularMinLightBrightness));
        UNITY_BRANCH
        if (_SpecularType == 1) // Realistic
        {
            finalSpecular = calculateRealisticSpecular(albedo, poiMesh.uv[0], _SpecularTint, _SpecularSmoothness, _SpecularInvertSmoothness, _SpecularMixAlbedoIntoTint, specularMap, specularLight, poiMesh.normals[_SpecularNormal]);
            finalSpecular *= max(_SpecularMinLightBrightness,lerp(1,poiLight.attenuation,_SpecularAttenuation));
        }
        UNITY_BRANCH
        if (_SpecularType == 2) // Toon
        {
            finalSpecular = calculateToonSpecular(albedo, poiMesh.uv[0], _SpecularToonInnerOuter, _SpecularMixAlbedoIntoTint, _SmoothnessFrom, specularMap, specularLight, poiMesh.normals[_SpecularNormal]);
        }
        UNITY_BRANCH
        if (_SpecularType == 3) // anisotropic
        {
            finalSpecular = AnisotropicSpecular(_SpecWhatTangent, _AnisoUseTangentMap, _SpecularSmoothness, _Spec2Smoothness, _AnisoSpec1Alpha, _AnisoSpec2Alpha, _SpecularTint, _SpecularMixAlbedoIntoTint, specularMap, specularLight);
            finalSpecular *= max(_SpecularMinLightBrightness,lerp(1,poiLight.attenuation,_SpecularAttenuation));
        }
        
        half specularMask = UNITY_SAMPLE_TEX2D_SAMPLER(_SpecularMask, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _SpecularMask)).r;
        finalSpecular *= _SpecularTint.a;
        finalSpecular = finalSpecular.rgb * _SpecularTint.rgb * saturate(poiMax(specularLight));
        return finalSpecular * specularMask;
    }
    
    float _EnableSpecular1;
    int _SpecWhatTangent1;
    int _SpecularType1;
    int _SmoothnessFrom1;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_SpecularMap1); float4 _SpecularMap1_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_ShiftTexture1); float4 _ShiftTexture1_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_AnisoTangentMap1); float4 _AnisoTangentMap1_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_SpecularMask1); float4 _SpecularMask1_ST;
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
    half _SpecularMixAlbedoIntoTint1;
    float _SpecularMinLightBrightness1;
    // Toon
    half4 _SpecularToonInnerOuter1;
    
    float3 calculateSpecular1(float4 albedo)
    {
        UNITY_BRANCH
        if (_EnableSpecular1)
        {
            half3 finalSpecular = 0;
            float4 specularMap = UNITY_SAMPLE_TEX2D_SAMPLER(_SpecularMap1, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _SpecularMap1));
            float3 specularLight = float3(max(poiLight.color.r, _SpecularMinLightBrightness1), max(poiLight.color.g, _SpecularMinLightBrightness1), max(poiLight.color.b, _SpecularMinLightBrightness1));
            UNITY_BRANCH
            if(_SpecularType1 == 1) // Realistic
            {
                finalSpecular = calculateRealisticSpecular(albedo, poiMesh.uv[0], _SpecularTint1, _SpecularSmoothness1, _SpecularInvertSmoothness1, _SpecularMixAlbedoIntoTint1, specularMap, specularLight, poiMesh.normals[_SpecularNormal1]);
                finalSpecular *= max(_SpecularMinLightBrightness,lerp(1,poiLight.attenuation,_SpecularAttenuation1));
            }
            UNITY_BRANCH
            if (_SpecularType1 == 2) // Toon
            {
                finalSpecular = calculateToonSpecular(albedo, poiMesh.uv[0], _SpecularToonInnerOuter1, _SpecularMixAlbedoIntoTint1, _SmoothnessFrom1, specularMap, specularLight, poiMesh.normals[_SpecularNormal1]);
            }
            UNITY_BRANCH
            if (_SpecularType1 == 3) // anisotropic
            {
                finalSpecular = AnisotropicSpecular(_SpecWhatTangent1, _AnisoUseTangentMap1, _SpecularSmoothness1, _Spec2Smoothness1, _AnisoSpec1Alpha1, _AnisoSpec2Alpha1, _SpecularTint1, _SpecularMixAlbedoIntoTint1, specularMap, specularLight);
                finalSpecular *= max(_SpecularMinLightBrightness,lerp(1,poiLight.attenuation,_SpecularAttenuation1));
            }
            
            half specularMask = UNITY_SAMPLE_TEX2D_SAMPLER(_SpecularMask1, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _SpecularMask1)).r;
            finalSpecular *= _SpecularTint1.a;
            finalSpecular = finalSpecular.rgb * _SpecularTint1.rgb * saturate(poiMax(specularLight));
            return finalSpecular * specularMask;
        }
        return 0;
    }
    
#endif