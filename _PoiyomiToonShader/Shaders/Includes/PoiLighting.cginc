
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
    float _IndirectContribution;
    float _AttenuationMultiplier;
    float _EnableLighting;
    float _LightingControlledUseLightColor;
    uint _LightingAOUV;

    UNITY_DECLARE_TEX2D_NOSAMPLER(_AOMap); float4 _AOMap_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_LightingShadowMask); float4 _LightingShadowMask_ST;
    float _AOStrength;
    
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
    
    float calculateAOMap(float AOMap, float AOStrength)
    {
        return lerp(1, AOMap, AOStrength);
    }
    
    void calculateBasePassLighting()
    {
        float AOMap = 1;
        #ifndef OUTLINE
            AOMap = UNITY_SAMPLE_TEX2D_SAMPLER(_AOMap, _MainTex, TRANSFORM_TEX(poiMesh.uv[_LightingAOUV], _AOMap));
            AOMap = calculateAOMap(AOMap, _AOStrength);
        #endif
        
        float3 grayscale_vector = float3(.33333, .33333, .33333);
        float3 ShadeSH9Plus = GetSHLength();
        float3 ShadeSH9Minus = ShadeSH9(float4(0, 0, 0, 1));
        
        #ifndef OUTLINE
            float ShadowStrengthMap = UNITY_SAMPLE_TEX2D_SAMPLER(_LightingShadowMask, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _LightingShadowMask)).r;
            _ShadowStrength *= ShadowStrengthMap;
        #endif
        
        float bw_lightColor = dot(poiLight.color, grayscale_vector);
        float bw_directLighting = (((poiLight.nDotL * 0.5 + 0.5) * bw_lightColor * lerp(1, poiLight.attenuation, _AttenuationMultiplier)) + dot(ShadeSH9Normal(poiMesh.fragmentNormal), grayscale_vector));
        float bw_bottomIndirectLighting = dot(ShadeSH9Minus, grayscale_vector);
        float bw_topIndirectLighting = dot(ShadeSH9Plus, grayscale_vector);
        float lightDifference = ((bw_topIndirectLighting + bw_lightColor) - bw_bottomIndirectLighting);
        poiLight.lightMap = smoothstep(0, lightDifference, bw_directLighting - bw_bottomIndirectLighting);
        
        poiLight.directLighting = saturate(lerp(ShadeSH9Plus, poiLight.color, .75));
        poiLight.indirectLighting = saturate(ShadeSH9Minus);
        
        poiLight.rampedLightMap = tex2D(_ToonRamp, poiLight.lightMap * AOMap + _ShadowOffset);
        
        if (_LightingType == 0)
        {
            poiLight.finalLighting = lerp((poiLight.indirectLighting), lerp(poiLight.directLighting, poiLight.indirectLighting, _IndirectContribution), lerp(1, poiLight.rampedLightMap, _ShadowStrength)) ;
        }
        else if(_LightingType == 1)
        {
            float3 ramp0 = tex2D(_ToonRamp, float2(1, 1));
            poiLight.finalLighting = lerp(ramp0, poiLight.rampedLightMap, _ShadowStrength) * poiLight.directLighting;
        }
        else if(_LightingType == 2)
        {
            float3 real = ShadeSH9(float4(poiMesh.fragmentNormal, 1));
            poiLight.finalLighting = saturate(_LightColor0.rgb * AOMap * lerp(1, poiLight.attenuation, _AttenuationMultiplier) * (poiLight.nDotL * 0.5 + 0.5) + real);
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
        finalColor.rgb *= poiLight.finalLighting;
    }
#endif