#ifndef POI_RGBMASK
    #define POI_RGBMASK
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_RGBMask); float4 _RGBMask_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_RedTexure); float4 _RedTexure_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_GreenTexture); float4 _GreenTexture_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_BlueTexture); float4 _BlueTexture_ST;
    
    float4 _RedColor;
    float4 _GreenColor;
    float4 _BlueColor;
    
    float4 _RGBMaskPanning;
    float4 _RGBRedPanning;
    float4 _RGBGreenPanning;
    float4 _RGBBluePanning;
    
    float _RGBBlendMultiplicative;
    
    uint _RGBMaskUV;
    uint _RGBRed_UV;
    uint _RGBGreen_UV;
    uint _RGBBlue_UV;
    
    float3 calculateRGBMask(float3 baseColor)
    {
        float3 rgbMask = POI2D_SAMPLER_PAN(_RGBMask, _MainTex, poiMesh.uv[_RGBMaskUV], _RGBMaskPanning).rgb;
        float4 red = POI2D_SAMPLER_PAN(_RedTexure, _MainTex, poiMesh.uv[_RGBRed_UV], _RGBRedPanning);
        float4 green = POI2D_SAMPLER_PAN(_GreenTexture, _MainTex, poiMesh.uv[_RGBGreen_UV], _RGBGreenPanning);
        float4 blue = POI2D_SAMPLER_PAN(_BlueTexture, _MainTex, poiMesh.uv[_RGBBlue_UV], _RGBBluePanning);
        
        UNITY_BRANCH
        if (_RGBBlendMultiplicative)
        {
            float3 RGBColor = 1;
            RGBColor = lerp(RGBColor, red.rgb * _RedColor.rgb, rgbMask.r * red.a * _RedColor.a);
            RGBColor = lerp(RGBColor, green.rgb * _GreenColor.rgb, rgbMask.g * green.a * _GreenColor.a);
            RGBColor = lerp(RGBColor, blue.rgb * _BlueColor.rgb, rgbMask.b * blue.a * _BlueColor.a);
            baseColor *= RGBColor;
        }
        else
        {
            baseColor = lerp(baseColor, red.rgb * _RedColor.rgb, rgbMask.r * red.a * _RedColor.a);
            baseColor = lerp(baseColor, green.rgb * _GreenColor.rgb, rgbMask.g * green.a * _GreenColor.a);
            baseColor = lerp(baseColor, blue.rgb * _BlueColor.rgb, rgbMask.b * blue.a * _BlueColor.a);
        }
        
        return baseColor;
    }
    
#endif