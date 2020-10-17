#ifndef POI_MACROS
    #define POI_MACROS
    
    #define POI_TEXTURE_NOSAMPLER(tex) Texture2D tex; float4 tex##_ST; float2 tex##Pan; uint tex##UV

    #define POI2D_SAMPLER_PAN(tex, texSampler, uv, pan) (UNITY_SAMPLE_TEX2D_SAMPLER(tex, texSampler, TRANSFORM_TEX(uv, tex) + _Time.x * pan))
    #define POI2D_SAMPLER(tex, texSampler, uv) (UNITY_SAMPLE_TEX2D_SAMPLER(tex, texSampler, TRANSFORM_TEX(uv, tex)))
    #define POI2D_PAN(tex, uv, pan) (tex2D(tex, TRANSFORM_TEX(uv, tex) + _Time.x * pan))
    #define POI2D(tex, uv) (tex2D(tex, TRANSFORM_TEX(uv, tex)))
    
    #ifdef POINT
    #   define POI_LIGHT_ATTENUATION(destName, shadow, input, worldPos) \
            unityShadowCoord3 lightCoord = mul(unity_WorldToLight, unityShadowCoord4(worldPos, 1)).xyz; \
            fixed shadow = UNITY_SHADOW_ATTENUATION(input, worldPos); \
            fixed destName = tex2D(_LightTexture0, dot(lightCoord, lightCoord).rr).r;
    #endif

    #ifdef SPOT
    #if !defined(UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS)
    #define DECLARE_LIGHT_COORD(input, worldPos) unityShadowCoord4 lightCoord = mul(unity_WorldToLight, unityShadowCoord4(worldPos, 1))
    #else
    #define DECLARE_LIGHT_COORD(input, worldPos) unityShadowCoord4 lightCoord = input._LightCoord
    #endif
    #   define POI_LIGHT_ATTENUATION(destName, shadow, input, worldPos) \
            DECLARE_LIGHT_COORD(input, worldPos); \
            fixed shadow = UNITY_SHADOW_ATTENUATION(input, worldPos); \
            fixed destName = (lightCoord.z > 0) * UnitySpotCookie(lightCoord) * UnitySpotAttenuate(lightCoord.xyz);
    #endif
#endif