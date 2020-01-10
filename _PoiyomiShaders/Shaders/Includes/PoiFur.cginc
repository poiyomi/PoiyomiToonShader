#ifndef FUR
    #define FUR
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_FurTexture); float4 _FurTexture_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_FurHeightMap); float4 _FurHeightMap_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_FurMask); float4 _FurMask_ST;
    float _FurAO;
    float4 _FurColor;
    float _FurLayers;
    float _FurMinDistance;
    float _FurMaxDistance;
    float _FurHeightMapMin;
    float _FurHeightMapMax;
    float _FurDebug;
    float _AoRampMin;
    float _AoRampMax;
    
    //globals
    half furHeightMap;
    half furMask;
    
    void calculateVertexFur()
    {
        
    }
    
    void calculatePixelFur(float furAlpha, float2 uv)
    {
        
        furHeightMap = UNITY_SAMPLE_TEX2D_SAMPLER(_FurHeightMap, _MainTex, TRANSFORM_TEX(uv, _FurHeightMap)).x;
        furHeightMap = remap(furHeightMap, 0, _FurHeightMapMax, _FurHeightMapMin, 1);
        furMask = poiMax(UNITY_SAMPLE_TEX2D_SAMPLER(_FurMask, _MainTex, TRANSFORM_TEX(uv, _FurMask)).xyz);
        half3 furTexture = UNITY_SAMPLE_TEX2D_SAMPLER(_FurTexture, _MainTex, TRANSFORM_TEX(uv, _FurTexture));
        clip(furHeightMap - furAlpha);
        clip(furMask - furAlpha);
        
        albedo.rgb = lerp(mainTexture.rgb, furTexture.rgb * _FurColor.rgb, ceil(furAlpha));
    }
    
#endif