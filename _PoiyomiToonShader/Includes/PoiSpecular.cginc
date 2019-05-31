#ifndef SPECULAR
    #define SPECULAR
    
    float _SpecularSmoothness;
    float4 _SpecularTint;
    float _IOR;
    float _Fresnel;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_SpecularMap); float4 _SpecularMap_ST;
    float3 finalSpecular;
    
    // Trowbridge-Reitz GGX
    float DistributionGGX(float3 n, float3 h)
    {
        float a = pow(1 - _SpecularSmoothness, 2.0);
        float a2 = pow(a, 2.0);
        float nh = dot(n, h);
        float nh2 = pow(nh, 2.0);
        
        float num = a2;
        float den = (nh2 * (a2 - 1.0) + 1.0);
        den = pi * pow(den, 2.0);
        
        return num / den;
    }
    
    // Schlick-Smith
    float VisibilitySmith(float3 n, float3 v, float3 l)
    {
        float k = pow(1 - _SpecularSmoothness, 2.0) * 0.5;
        
        float V = DotClamped(n, v) * (1.0 - k) + k;
        float L = DotClamped(n, l) * (1.0 - k) + k;
        
        return 0.25 / (V * L);
    }
    
    // Schlick
    float FresnelSchlick(float3 n, float3 v)
    {
        float cosTheta = DotClamped(n, v);
        
        float3 F0 = abs((1.0 - _IOR) / (1.0 + _IOR));
        F0 = pow(F0, 2.0);
        return F0 + (1.0 - F0) * pow(1.0 - cosTheta, _Fresnel);
    }
    
    // Based on Torrance-Sparrow microfacet model
    float Microfacet(float3 n, float3 h, float3 v, float3 l)
    {
        float D = DistributionGGX(h, n);
        float V = VisibilitySmith(n, v, l);
        float F = FresnelSchlick(n, v);
        
        float specularTerm = D * V * pi;
        specularTerm = max(0.0, specularTerm * DotClamped(n, l));
        
        return specularTerm * F;
    }
    
    void calculateSpecular(float3 normal, float4 albedo, float3 viewDir, float2 uv)
    {
        _SpecularSmoothness *= UNITY_SAMPLE_TEX2D_SAMPLER(_SpecularMap, _MainTex, TRANSFORM_TEX(uv, _SpecularMap));
        // Normalize the normal
        float3 n = normalize(normal);
        // Light vector from mesh's surface
        float3 l = normalize(poiLight.direction);
        // Viewport(camera) vector from mesh's surface
        float3 v = normalize(viewDir);
        // Halfway vector
        float3 h = normalize(l + v);
        
        float maxSpec = poiLight.color;
        float surfaceReduction = 1.0 / (pow(roughness, 2.0) + 1.0);
        finalSpecular = clamp(Microfacet(n, h, v, l) * albedo * poiLight.color * _SpecularTint * poiLight.attenuation, 0, maxSpec);
    }
    
    void applySpecular(inout float4 finalColor)
    {
        finalColor.rgb += finalSpecular;
    }
    
#endif