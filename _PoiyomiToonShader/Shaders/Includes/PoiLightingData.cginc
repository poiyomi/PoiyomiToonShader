#ifndef POI_LIGHTING_DATA
    #define POI_LIGHTING_DATA

    struct PoiLighting
    {
        half3 direction;
        half3 color;
        half attenuation;
        half3 directLighting;
        half3 indirectLighting;
        half lightMap;
        half3 rampedLightMap;
        half3 finalLighting;
        half nDotL;
    };
    
    static PoiLighting poiLight;
    
    float FadeShadows(float attenuation, float3 worldPosition)
    {
        float viewZ = dot(_WorldSpaceCameraPos - worldPosition, UNITY_MATRIX_V[2].xyz);
        float shadowFadeDistance = UnityComputeShadowFadeDistance(worldPosition, viewZ);
        float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
        attenuation = saturate(attenuation + shadowFade);
        return attenuation;
    }
    
    void calculateAttenuation(v2f i)
    {
        UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos.xyz)
        poiLight.attenuation = FadeShadows(attenuation, i.worldPos.xyz);
    }
    
    void calculateLightDirection(v2f i)
    {
        #ifdef FORWARD_BASE_PASS
            poiLight.direction = _WorldSpaceLightPos0;
            poiLight.direction = normalize(poiLight.direction + unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz);
            
        #else
            #if defined(POINT) || defined(SPOT)
                poiLight.direction = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
            #endif
        #endif
    }
    
    void calculateLightColor()
    {
        #ifdef FORWARD_BASE_PASS
            poiLight.color = _LightColor0.rgb + saturate(ShadeSH9(normalize(unity_SHAr + unity_SHAg + unity_SHAb)));
        #else
            #if defined(POINT) || defined(SPOT)
                poiLight.color = _LightColor0.rgb;
            #endif
        #endif
    }
    
    void calculateLightingData(v2f i)
    {
        calculateAttenuation(i);
        calculateLightColor();
        calculateLightDirection(i);
        poiLight.nDotL = dot(i.normal, poiLight.direction);
    }
    
#endif