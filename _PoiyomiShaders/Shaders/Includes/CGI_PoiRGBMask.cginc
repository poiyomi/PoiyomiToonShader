#ifndef POI_RGBMASK
    #define POI_RGBMASK
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_RGBMask); float4 _RGBMask_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_RedTexure); float4 _RedTexure_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_GreenTexture); float4 _GreenTexture_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_BlueTexture); float4 _BlueTexture_ST;
    
    float4 _RedColor;
    float4 _GreenColor;
    float4 _BlueColor;

    uint _RGBMaskUV;
    uint _RGBRed_UV;
    uint _RGBGreen_UV;
    uint _RGBBlue_UV;

    float3 calculateRGBMask(float3 baseColor)
    {
        float3 rgbMask = UNITY_SAMPLE_TEX2D_SAMPLER(_RGBMask, _MainTex, TRANSFORM_TEX(poiMesh.uv[_RGBMaskUV], _RGBMask)).rgb;
        float4 red = UNITY_SAMPLE_TEX2D_SAMPLER(_RedTexure, _MainTex, TRANSFORM_TEX(poiMesh.uv[_RGBRed_UV], _RedTexure));
        float4 green = UNITY_SAMPLE_TEX2D_SAMPLER(_GreenTexture, _MainTex, TRANSFORM_TEX(poiMesh.uv[_RGBGreen_UV], _GreenTexture));
        float4 blue = UNITY_SAMPLE_TEX2D_SAMPLER(_BlueTexture, _MainTex, TRANSFORM_TEX(poiMesh.uv[_RGBBlue_UV], _BlueTexture));

        baseColor = lerp(baseColor, red.rgb * _RedColor.rgb, rgbMask.r * red.a * _RedColor.a);
        baseColor = lerp(baseColor, green.rgb * _GreenColor.rgb, rgbMask.g * green.a * _GreenColor.a);
        baseColor = lerp(baseColor, blue.rgb * _BlueColor.rgb, rgbMask.b * blue.a * _BlueColor.a);

        return baseColor;
    }
    
#endif