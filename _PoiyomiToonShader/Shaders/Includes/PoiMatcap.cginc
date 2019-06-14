#ifndef MATCAP
    #define MATCAP
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_Matcap);
    UNITY_DECLARE_TEX2D_NOSAMPLER(_MatcapMap); float4 _MatcapMap_ST;
    float4 _MatcapColor;
    float  _MatcapStrength;
    float _ReplaceWithMatcap;
    float _MultiplyMatcap;
    float _AddMatcap;
    
    float3 matcap;
    float matcapMask;
    
    float2 getMatcapUV(float3 viewDirection, float3 normalDirection)
    {
        half3 worldViewUp = normalize(half3(0, 1, 0) - viewDirection * dot(viewDirection, half3(0, 1, 0)));
        half3 worldViewRight = normalize(cross(viewDirection, worldViewUp));
        half2 matcapUV = half2(dot(worldViewRight, normalDirection), dot(worldViewUp, normalDirection)) * 0.5 + 0.5;
        return matcapUV;
    }
    
    void calculateMatcap(float3 cameraToVert, float3 normal, float2 uv)
    {
        float2 matcapUV = getMatcapUV(cameraToVert, normal);
        matcap = UNITY_SAMPLE_TEX2D_SAMPLER(_Matcap, _MainTex, matcapUV) * _MatcapColor * _MatcapStrength;
        matcapMask = UNITY_SAMPLE_TEX2D_SAMPLER(_MatcapMap, _MainTex, TRANSFORM_TEX(uv, _MatcapMap));
    }
    
    void applyMatcap(inout float4 finalColor)
    {
        finalColor.rgb = lerp(finalColor, matcap, _ReplaceWithMatcap * matcapMask);
        finalColor.rgb *= lerp(1, matcap, _MultiplyMatcap * matcapMask);
        finalColor.rgb += matcap * _AddMatcap * matcapMask;
    }
#endif