#ifndef SPECULAR
    #define SPECULAR
    
    int _SpecularType;
    int _SmoothnessFrom;
    int _SpecularColorFrom;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_SpecularMap); float4 _SpecularMap_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_SpecularHighTexture); float4 _SpecularHighTexture_ST;
    float4 _SpecularTint;
    float _SpecularSmoothness;
    
    // Globals
    half4 finalSpecular;
    half4 highTexture;

    UnityIndirect ZeroIndirect()
    {
        UnityIndirect ind;
        ind.diffuse = 0;
        ind.specular = 0;
        return ind;
    }
    
    // From unity just putting it here in case I want to mod it
    half4 poiRealisticSpecular(half3 diffColor, half3 specColor, half oneMinusReflectivity, half smoothness,
    float3 normal, float3 viewDir,
    UnityLight light, UnityIndirect gi)
    {
        float perceptualRoughness = SmoothnessToPerceptualRoughness(smoothness);
        float3 halfDir = Unity_SafeNormalize(float3(light.dir) + viewDir);
        
        // NdotV should not be negative for visible pixels, but it can happen due to perspective projection and normal mapping
        // In this case normal should be modified to become valid (i.e facing camera) and not cause weird artifacts.
        // but this operation adds few ALU and users may not want it. Alternative is to simply take the abs of NdotV (less correct but works too).
        // Following define allow to control this. Set it to 0 if ALU is critical on your platform.
        // This correction is interesting for GGX with SmithJoint visibility function because artifacts are more visible in this case due to highlight edge of rough surface
        // Edit: Disable this code by default for now as it is not compatible with two sided lighting used in SpeedTree.
        #define UNITY_HANDLE_CORRECTLY_NEGATIVE_NDOTV 0
        
        #if UNITY_HANDLE_CORRECTLY_NEGATIVE_NDOTV
            // The amount we shift the normal toward the view vector is defined by the dot product.
            half shiftAmount = dot(normal, viewDir);
            normal = shiftAmount < 0.0f ? normal + viewDir * (-shiftAmount + 1e-5f): normal;
            // A re-normalization should be applied here but as the shift is small we don't do it to save ALU.
            //normal = normalize(normal);
            
            float nv = saturate(dot(normal, viewDir)); // TODO: this saturate should no be necessary here
        #else
            half nv = abs(dot(normal, viewDir));    // This abs allow to limit artifact
        #endif
        
        float nl = saturate(dot(normal, light.dir));
        float nh = saturate(dot(normal, halfDir));
        
        half lv = saturate(dot(light.dir, viewDir));
        half lh = saturate(dot(light.dir, halfDir));
        
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
    
    void calculateRealisticSpecular(float3 normal, float4 albedo, float3 viewDir, float2 uv)
    {
        half4 spec = UNITY_SAMPLE_TEX2D_SAMPLER(_SpecularMap, _MainTex, TRANSFORM_TEX(uv, _SpecularMap));
        
        half oneMinusReflectivity;
        
        UnityLight unityLight;
        unityLight.color = poiLight.color;
        unityLight.dir = poiLight.direction;
        unityLight.ndotl = poiLight.nDotL;

        UNITY_BRANCH
        if(_SmoothnessFrom == 0)
        {
            half3 diffColor = EnergyConservationBetweenDiffuseAndSpecular(albedo, spec.rgb * _SpecularTint.rgb, /*out*/ oneMinusReflectivity);
            finalSpecular = poiRealisticSpecular(diffColor, spec.rgb, oneMinusReflectivity, spec.a * _SpecularSmoothness, normal, viewDir, unityLight, ZeroIndirect());
        }
        else
        {
            half3 diffColor = EnergyConservationBetweenDiffuseAndSpecular(albedo, _SpecularTint.rgb, /*out*/ oneMinusReflectivity);
            float smoothness = max (max (spec.r, spec.g), spec.b);
            finalSpecular = poiRealisticSpecular(diffColor, 1, oneMinusReflectivity, smoothness * _SpecularSmoothness, normal, viewDir, unityLight, ZeroIndirect());
        }
    }
    
    void calculateToonSpecular(float3 normal, float4 albedo, float3 viewDir, float2 uv)
    {
        finalSpecular = 1;
        calculateRealisticSpecular(normal, albedo, viewDir, uv);
        float specIntensity = dot(finalSpecular.rgb, grayscale_for_light());
        finalSpecular.rgb = smoothstep(0.99,1, specIntensity) * poiLight.color.rgb * poiLight.attenuation;
    }
    
    void calculateSpecular(float3 normal, float4 albedo, float3 viewDir, float2 uv)
    {
        highTexture = UNITY_SAMPLE_TEX2D_SAMPLER(_SpecularHighTexture, _MainTex, TRANSFORM_TEX(uv, _SpecularHighTexture));

        UNITY_BRANCH
        if (_SpecularType == 0) // Off
        {
            return;
        }
        else if (_SpecularType == 1) // Realistic
        {
            calculateRealisticSpecular(normal, albedo, viewDir, uv);
            finalSpecular *= poiLight.attenuation;
        }
        else if (_SpecularType == 2) // Toon
        {
            calculateToonSpecular(normal, albedo, viewDir, uv);
        }
        else if (_SpecularType == 4) // anisotropic
        {
            return;
        }
    }
    
    void applySpecular(inout float4 finalColor)
    {
        if(_SpecularColorFrom == 0)
        {
            finalColor.rgb += finalSpecular.rgb * _SpecularTint.rgb;
        }
        else
        {
            float specIntensity = max (max (finalSpecular.r, finalSpecular.g), finalSpecular.b);
            finalColor.rgb += lerp(0, highTexture.rgb, saturate(specIntensity)) * _SpecularTint.rgb;
        }
    }
    
#endif