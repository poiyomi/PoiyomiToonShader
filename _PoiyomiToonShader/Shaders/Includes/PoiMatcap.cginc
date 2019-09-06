#ifndef MATCAP
    #define MATCAP
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_Matcap);
    UNITY_DECLARE_TEX2D_NOSAMPLER(_MatcapMask); float4 _MatcapMask_ST;
    float4 _MatcapColor;
    float  _MatcapBrightness;
    float _ReplaceWithMatcap;
    float _MultiplyMatcap;
    float _AddMatcap;
    float _EnableMatcap;
    
    float3 matcap;
    float matcapMask;
    
    float2 getMatcapUV()
    {
        half3 worldViewUp = normalize(half3(0, 1, 0) - poiCam.viewDir * dot(poiCam.viewDir, half3(0, 1, 0)));
        half3 worldViewRight = normalize(cross(poiCam.viewDir, worldViewUp));
        return half2(dot(worldViewRight, poiMesh.fragmentNormal), dot(worldViewUp, poiMesh.fragmentNormal)) * 0.5 + 0.5;
    }
    
    void calculateMatcap()
    {
        float2 matcapUV = getMatcapUV();
        matcap = UNITY_SAMPLE_TEX2D_SAMPLER(_Matcap, _MainTex, matcapUV) * _MatcapColor * _MatcapBrightness;
        matcapMask = UNITY_SAMPLE_TEX2D_SAMPLER(_MatcapMask, _MainTex, TRANSFORM_TEX(poiMesh.uv, _MatcapMask));
    }
    
    void applyMatcap(inout float4 finalColor)
    {
        finalColor.rgb = lerp(finalColor, matcap, _ReplaceWithMatcap * matcapMask);
        finalColor.rgb *= lerp(1, matcap, _MultiplyMatcap * matcapMask);
        finalColor.rgb += matcap * _AddMatcap * matcapMask;
    }
#endif