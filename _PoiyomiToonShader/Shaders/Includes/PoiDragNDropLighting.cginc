#ifndef DND_LIGHTING
    #define DND_LIGHTING
    
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
    float _MinBrightness;
    float _MaxBrightness;
    float _IndirectContribution;
    float _AttenuationMultiplier;
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_AOMap); float4 _AOMap_ST;
    float _AOStrength;
    
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
    
    float FadeShadows(float attenuation, float3 worldPosition)
    {
        float viewZ = dot(_WorldSpaceCameraPos - worldPosition, UNITY_MATRIX_V[2].xyz);
        float shadowFadeDistance = UnityComputeShadowFadeDistance(worldPosition, viewZ);
        float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
        attenuation = saturate(attenuation + shadowFade);
        return attenuation;
    }
    
    float calculateAOMap(float AOMap, float AOStrength)
    {
        return lerp(1, AOMap, AOStrength);
    }
    
    void calculateBasePassLighting(float3 normal, float2 uv)
    {
        poiLight.direction = _WorldSpaceLightPos0;
        poiLight.nDotL = dot(normal, poiLight.direction);
        float AOMap = 1;
        #ifndef OUTLINE
            AOMap = UNITY_SAMPLE_TEX2D_SAMPLER(_AOMap, _MainTex, TRANSFORM_TEX(uv, _AOMap));
            AOMap = calculateAOMap(AOMap, _AOStrength);
        #endif
        poiLight.finalLighting = saturate((GetSHLength() + poiLight.color) * AOMap);
    }
    
    void calculateDNDLighting(v2f i)
    {
        #ifdef OUTLINE
            _ShadowStrength = _OutlineShadowStrength;
        #endif
        UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos.xyz)
        poiLight.attenuation = FadeShadows(attenuation, i.worldPos.xyz);
        poiLight.color = _LightColor0.rgb;
        #ifdef FORWARD_BASE_PASS
            calculateBasePassLighting(poiMesh.fragmentNormal, i.uv);
        #else
            #if defined(POINT) || defined(SPOT)
                poiLight.position = _WorldSpaceLightPos0.xyz;
                poiLight.direction = normalize(poiLight.position - i.worldPos);
                poiLight.nDotL = dot(poiMesh.fragmentNormal, poiLight.direction);
                poiLight.finalLighting = poiLight.color * poiLight.attenuation * smoothstep(.499, .5, .5 * poiLight.nDotL + .5);
            #endif
        #endif
    }
    
    void applyDNDLighting(inout float4 finalColor)
    {
        finalColor.rgb *= poiLight.finalLighting;
    }

#endif