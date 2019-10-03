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
    // Toon
    half4 _SpecularToonInnerOuter;
    
    // Globals
    half3 finalSpecular;
    float shiftTexture;
    float3 tangentDirectionMap;
    float4 specularMap;
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
    
    void calculateRealisticSpecular(float4 albedo, float2 uv)
    {
        
        half oneMinusReflectivity;
        
        UnityLight unityLight;
        unityLight.color = poiLight.color;
        unityLight.dir = poiLight.direction;
        unityLight.ndotl = poiLight.nDotL;
        
        UNITY_BRANCH
        if (_SmoothnessFrom == 0)
        {
            half3 diffColor = EnergyConservationBetweenDiffuseAndSpecular(albedo, specularMap.rgb * _SpecularTint.rgb, /*out*/ oneMinusReflectivity);
            finalSpecular = poiRealisticSpecular(diffColor, specularMap.rgb, oneMinusReflectivity, specularMap.a * _SpecularSmoothness * lerp(1,-1,_SpecularInvertSmoothness), poiMesh.fragmentNormal, unityLight, ZeroIndirect());
        }
        else
        {
            half3 diffColor = EnergyConservationBetweenDiffuseAndSpecular(albedo, _SpecularTint.rgb, /*out*/ oneMinusReflectivity);
            float smoothness = max(max(specularMap.r, specularMap.g), specularMap.b);
            finalSpecular = poiRealisticSpecular(diffColor, 1, oneMinusReflectivity, smoothness * _SpecularSmoothness * lerp(1,-1,_SpecularInvertSmoothness), poiMesh.fragmentNormal, unityLight, ZeroIndirect());
        }
        finalSpecular *= lerp(1, albedo.rgb, _SpecularMixAlbedoIntoTint);
    }
    
    void calculateToonSpecular(float4 albedo, float2 uv)
    {
        /*
        finalSpecular = 1;
        calculateRealisticSpecular(albedo, uv);
        float specIntensity = dot(finalSpecular.rgb, grayscale_for_light());
        finalSpecular.rgb = smoothstep(0.99, 1, specIntensity) * poiLight.color.rgb * poiLight.attenuation;
        */
        finalSpecular = smoothstep(1 - _SpecularToonInnerOuter.y, 1 - _SpecularToonInnerOuter.x, dot(poiLight.halfDir, poiMesh.fragmentNormal) * poiLight.attenuation) * poiLight.color.rgb;
        UNITY_BRANCH
        if (_SmoothnessFrom == 0)
        {
            finalSpecular.rgb *= specularMap.rgb * lerp(1, albedo.rgb, _SpecularMixAlbedoIntoTint);
            finalSpecular *= specularMap.a;
        }
        else
        {
            finalSpecular *= specularMap.r * lerp(1, albedo.rgb, _SpecularMixAlbedoIntoTint);
        }
    }
    
    float3 strandSpecular(float TdotL, float TdotV, float specPower)
    {
        float Specular = saturate(poiLight.nDotL) * pow(saturate(sqrt(1.0 - (TdotL * TdotL)) * sqrt(1.0 - (TdotV * TdotV)) - TdotL * TdotV), specPower);
        half normalization = sqrt((specPower + 1) * ((specPower) + 1)) / (8 * pi);//
        Specular *= normalization;
        return Specular;
    }
    
    void AnisotropicSpecular()
    {
        float3 tangentOrBitangent = _SpecWhatTangent ? poiMesh.tangent: poiMesh.bitangent;
        
        
        float4 packedTangentMap = UNITY_SAMPLE_TEX2D_SAMPLER(_AnisoTangentMap, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _AnisoTangentMap));
        float3 normalLocalAniso = lerp(float3(0, 0, 1), UnpackNormal(packedTangentMap), _AnisoUseTangentMap);
        normalLocalAniso = BlendNormals(normalLocalAniso, poiMesh.tangentSpaceNormal);
        //float3 normalDirection = normalize(mul(poiMesh.fragmentNormal, poiTData.tangentTransform));
        float3 normalDirectionAniso = Unity_SafeNormalize(mul(normalLocalAniso, poiTData.tangentTransform));
        float3 tangentDirection = mul(poiTData.tangentTransform, tangentOrBitangent).xyz;
        float3 viewReflectDirectionAniso = reflect(-poiCam.viewDir, normalDirectionAniso); // possible bad negation
        tangentDirectionMap = mul(poiTData.tangentToWorld, float3(normalLocalAniso.rg, 0.0)).xyz;
        tangentDirectionMap = normalize(lerp(tangentOrBitangent, tangentDirectionMap, _AnisoUseTangentMap));
        float TdotL = dot(poiLight.direction, tangentDirectionMap);
        float TdotV = dot(poiCam.viewDir, tangentDirectionMap);
        float TdotH = dot(poiLight.halfDir, tangentDirectionMap);
        half specPower = RoughnessToSpecPower(1.0 - _SpecularSmoothness * specularMap.a);
        half spec2Power = RoughnessToSpecPower(1.0 - _Spec2Smoothness * specularMap.a);
        half Specular = 0;
        
        float3 spec = strandSpecular(TdotL, TdotV, specPower) * _AnisoSpec1Alpha;
        float3 spec2 = strandSpecular(TdotL, TdotV, spec2Power) * _AnisoSpec2Alpha;
        
        finalSpecular = max(spec, spec2) * specularMap.rgb * _SpecularTint.a * poiLight.color * lerp(1, albedo.rgb, _SpecularMixAlbedoIntoTint);
    }
    
    void calculateSpecular(float4 albedo)
    {
        specularMap = UNITY_SAMPLE_TEX2D_SAMPLER(_SpecularMap, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _SpecularMap));
        
        UNITY_BRANCH
        if (_SpecularType == 1) // Realistic
        {
            calculateRealisticSpecular(albedo, poiMesh.uv[0]);
            finalSpecular *= poiLight.attenuation;
        }
        UNITY_BRANCH
        if (_SpecularType == 2) // Toon
        {
            calculateToonSpecular(albedo, poiMesh.uv[0]);
        }
        UNITY_BRANCH
        if (_SpecularType == 3) // anisotropic
        {
            AnisotropicSpecular();
        }
    }
    
    void applySpecular(inout float4 finalColor)
    {
        half specularMask = UNITY_SAMPLE_TEX2D_SAMPLER(_SpecularMask, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _SpecularMask)).r;
        finalSpecular *= _SpecularTint.a;
        finalSpecular = finalSpecular.rgb * _SpecularTint.rgb * saturate(poiMax(poiLight.color.rgb));
        finalColor.rgb += finalSpecular * specularMask;
    }
    
#endif