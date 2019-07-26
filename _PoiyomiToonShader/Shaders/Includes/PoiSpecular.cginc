#ifndef SPECULAR
    #define SPECULAR
    int _SpecWhatTangent;
    int _SpecularType;
    int _SmoothnessFrom;
    int _SpecularColorFrom;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_SpecularMap); float4 _SpecularMap_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_SpecularHighTexture); float4 _SpecularHighTexture_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_ShiftTexture); float4 _ShiftTexture_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_AnisoTangentMap); float4 _AnisoTangentMap_ST;
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
    // Globals
    half3 finalSpecular;
    half4 highTexture;
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
        
        // NdotV should not be negative for visible pixels, but it can happen due to perspective projection and normal mapping
        // In this case normal should be modified to become valid (i.e facing camera) and not cause weird artifacts.
        // but this operation adds few ALU and users may not want it. Alternative is to simply take the abs of NdotV (less correct but works too).
        // Following define allow to control this. Set it to 0 if ALU is critical on your platform.
        // This correction is interesting for GGX with SmithJoint visibility function because artifacts are more visible in this case due to highlight edge of rough surface
        // Edit: Disable this code by default for now as it is not compatible with two sided lighting used in SpeedTree.
        #define UNITY_HANDLE_CORRECTLY_NEGATIVE_NDOTV 0
        
        #if UNITY_HANDLE_CORRECTLY_NEGATIVE_NDOTV
            // The amount we shift the normal toward the view vector is defined by the dot product.
            half shiftAmount = dot(normal, poiCam.viewDir);
            normal = shiftAmount < 0.0f ? normal + poiCam.viewDir * (-shiftAmount + 1e-5f): normal;
            // A re-normalization should be applied here but as the shift is small we don't do it to save ALU.
            //normal = normalize(normal);
            
            float nv = saturate(dot(normal, poiCam.viewDir)); // TODO: this saturate should no be necessary here
        #else
            half nv = abs(dot(normal, poiCam.viewDir));    // This abs allow to limit artifact
        #endif
        
        float nl = saturate(dot(normal, light.dir));
        float nh = saturate(dot(normal, poiLight.halfDir));
        
        half lv = saturate(dot(light.dir, poiCam.viewDir));
        half lh = saturate(dot(light.dir, poiLight.halfDir));
        
        // Diffuse term
        half diffuseTerm = DisneyDiffuse(nv, nl, lh, perceptualRoughness) * nl;
        
        // Specular term
        // HACK: theoretically we should divide diffuseTerm by Pi and not multiply specularTerm!
        // BUT 1) that will make shader look significantly darker than Legacy ones
        // and 2) on engine side "Non-important" lights have to be divided by Pi too in cases when they are injected into ambient SH
        float roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
        
        // GGX with roughtness to 0 would mean no specular at all, using max(roughness, 0.002) here to match HDrenderloop roughtness remapping.
        roughness = max(roughness, 0.002);
        float V = SmithJointGGXVisibilityTerm(nl, nv, roughness);
        float D = GGXTerm(nh, roughness);
        
        float specularTerm = V * D * UNITY_PI; // Torrance-Sparrow model, Fresnel is applied later
        
        #ifdef UNITY_COLORSPACE_GAMMA
            specularTerm = sqrt(max(1e-4h, specularTerm));
        #endif
        
        // specularTerm * nl can be NaN on Metal in some cases, use max() to make sure it's a sane value
        specularTerm = max(0, specularTerm * nl);
        #if defined(_SPECULARHIGHLIGHTS_OFF)
            specularTerm = 0.0;
        #endif
        
        // surfaceReduction = Int D(NdotH) * NdotH * Id(NdotL>0) dH = 1/(roughness^2+1)
        half surfaceReduction;
        #ifdef UNITY_COLORSPACE_GAMMA
            surfaceReduction = 1.0 - 0.28 * roughness * perceptualRoughness;      // 1-0.28*x^3 as approximation for (1/(x^4+1))^(1/2.2) on the domain [0;1]
        #else
            surfaceReduction = 1.0 / (roughness * roughness + 1.0);           // fade \in [0.5;1]
        #endif
        
        // To provide true Lambert lighting, we need to be able to kill specular completely.
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
            finalSpecular = poiRealisticSpecular(diffColor, specularMap.rgb, oneMinusReflectivity, specularMap.a * _SpecularSmoothness, poiMesh.fragmentNormal, unityLight, ZeroIndirect());
        }
        else
        {
            half3 diffColor = EnergyConservationBetweenDiffuseAndSpecular(albedo, _SpecularTint.rgb, /*out*/ oneMinusReflectivity);
            float smoothness = max(max(specularMap.r, specularMap.g), specularMap.b);
            finalSpecular = poiRealisticSpecular(diffColor, 1, oneMinusReflectivity, smoothness * _SpecularSmoothness, poiMesh.fragmentNormal, unityLight, ZeroIndirect());
        }
    }
    
    void calculateToonSpecular(float4 albedo, float2 uv)
    {
        finalSpecular = 1;
        calculateRealisticSpecular(albedo, uv);
        float specIntensity = dot(finalSpecular.rgb, grayscale_for_light());
        finalSpecular.rgb = smoothstep(0.99, 1, specIntensity) * poiLight.color.rgb * poiLight.attenuation;
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
        float3 tangentOrBitangent = _SpecWhatTangent ? poiMesh.tangent : poiMesh.bitangent;
        

        float4 packedTangentMap = UNITY_SAMPLE_TEX2D_SAMPLER(_AnisoTangentMap, _MainTex, TRANSFORM_TEX(poiMesh.uv, _AnisoTangentMap));
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

        finalSpecular = max(spec, spec2) * specularMap.rgb * _SpecularTint.a * poiLight.color;
    }
    
    void calculateSpecular(float4 albedo, float2 uv)
    {
        highTexture = UNITY_SAMPLE_TEX2D_SAMPLER(_SpecularHighTexture, _MainTex, TRANSFORM_TEX(uv, _SpecularHighTexture));
        
        UNITY_BRANCH
        if (_SpecularType != 0) // Off
        {
            specularMap = UNITY_SAMPLE_TEX2D_SAMPLER(_SpecularMap, _MainTex, TRANSFORM_TEX(uv, _SpecularMap));
            
            UNITY_BRANCH
            if (_SpecularType == 1) // Realistic
            {
                calculateRealisticSpecular(albedo, uv);
                finalSpecular *= poiLight.attenuation;
            }
            UNITY_BRANCH
            if (_SpecularType == 2) // Toon
            {
                calculateToonSpecular(albedo, uv);
            }
            UNITY_BRANCH
            if (_SpecularType == 3) // anisotropic
            {
                AnisotropicSpecular();
            }
        }
    }
    
    void applySpecular(inout float4 finalColor)
    {
        UNITY_BRANCH
        if (_SpecularType != 0) // Off
        {
            if (_SpecularColorFrom == 0)
            {
                finalSpecular = finalSpecular.rgb * _SpecularTint.rgb * saturate(poiMax(poiLight.color.rgb));
                finalColor.rgb += finalSpecular;
            }
            else
            {
                float specIntensity = max(max(finalSpecular.r, finalSpecular.g), finalSpecular.b);
                finalSpecular = lerp(0, highTexture.rgb, saturate(specIntensity)) * _SpecularTint.rgb;
                finalColor.rgb += finalSpecular;
            }
        }
    }
    
#endif