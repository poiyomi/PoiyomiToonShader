float3 ShadeSH9Indirect()
{
    return ShadeSH9(half4(0.0, -1.0, 0.0, 1.0));
}

float3 ShadeSH9Direct()
{
    return ShadeSH9(half4(0.0, 1.0, 0.0, 1.0));
}

float3 grayscale_vector_node()
{
    return float3(0, 0.3823529, 0.01845836);
}

float3 grayscale_for_light()
{
    return float3(0.298912, 0.586611, 0.114478);
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

float3 getNewPoiLighting(float3 LightDirection, float3 normal, float shadowStrength, float attenuation, float attenuationMultiplier, float AO)
{
    float3 grayscale_vector = grayscale_vector_node();
    float3 ShadeSH9Plus = GetSHLength();
    float3 ShadeSH9Minus = ShadeSH9(float4(0, 0, 0, 1));
    float3 directLighting = ShadeSH9Plus + _LightColor0.rgb;
    float3 indirectLighting = ShadeSH9Minus;
    
    float bw_lightColor = dot(_LightColor0.rgb, grayscale_vector);
    float bw_directLighting = (((dot(LightDirection, normal) * 0.5 + 0.5) * bw_lightColor  * lerp(1,attenuation, attenuationMultiplier)) + dot(ShadeSH9Normal(normal), grayscale_vector));
    float bw_bottomIndirectLighting = dot(ShadeSH9Minus, grayscale_vector);
    float bw_topIndirectLighting = dot(ShadeSH9Plus, grayscale_vector);
    float lightDifference = ((bw_topIndirectLighting + bw_lightColor) - bw_bottomIndirectLighting);
    float remappedLight = smoothstep(0, lightDifference, bw_directLighting - bw_bottomIndirectLighting);
    float3 LightingRamp = tex2D(_ToonRamp, float2(remappedLight, remappedLight) + _ShadowOffset);

    return lerp(indirectLighting, lerp(directLighting, indirectLighting, _IndirectContribution), lerp(1, LightingRamp, shadowStrength)) * AO;
}