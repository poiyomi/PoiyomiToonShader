#ifndef SCROLLING_LAYERS
    #define SCROLLING_LAYERS

    UNITY_DECLARE_TEX2D_NOSAMPLER(_LayerTexture); float4 _LayerTexture_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_LayerMask); float4 _LayerMask_ST;
    float4 _LayerColor;
    float4 _LayerSpeed;
    float _Layers;
    float _LayerShrinkage;

    float4 color;
    float4 calculateScrollingLayers(float2 uv)
    {
        color = 0;

        for (int i = 0; i < _Layers; i++) {
            float2 uvMod = (1 + i*_LayerShrinkage) + (_Time.y * _LayerSpeed.xy);// / (1+i/3);
            color = saturate(color + UNITY_SAMPLE_TEX2D_SAMPLER(_LayerTexture, _MainTex, TRANSFORM_TEX(uv, _LayerTexture) * uvMod ))  / (1+i*2);
        }

        return color * _LayerColor;
    }
#endif