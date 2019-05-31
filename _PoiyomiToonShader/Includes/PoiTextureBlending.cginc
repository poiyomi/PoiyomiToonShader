#ifndef TEXTURE_BLENDING
    #define TEXTURE_BLENDING
    
    int _Blend;
    float4 _BlendTextureColor;
    sampler2D _BlendTexture; float4 _BlendTexture_ST;
    sampler2D _BlendNoiseTexture; float4 _BlendNoiseTexture_ST;
    float _BlendAlpha;
    float _BlendTiling;
    float _AutoBlend;
    float _AutoBlendSpeed;
    float _AutoBlendDelay;
    
    float blendAlpha = 0;
    
    void calculateTextureBlending(float blendAlpha, inout float4 mainTexture, inout float4 diffuse, float2 uv)
    {
        UNITY_BRANCH
        if (_Blend != 0)
        {
            float blendNoise = tex2D(_BlendNoiseTexture, TRANSFORM_TEX(uv, _BlendNoiseTexture));
            if(_AutoBlend > 0)
            {
                blendAlpha = (clamp(sin(_Time.y * _AutoBlendSpeed / _AutoBlendDelay) * (_AutoBlendDelay + 1), -1, 1) + 1) / 2;
            }
            blendAlpha = lerp(saturate((blendNoise - 1) + blendAlpha * 2), step(blendAlpha * 1.001, blendNoise), _Blend - 1);
            
            float4 blendCol = tex2D(_BlendTexture, TRANSFORM_TEX(uv, _BlendTexture)) * _BlendTextureColor;
            diffuse = lerp(diffuse, blendCol, blendAlpha);
            mainTexture.a = lerp(mainTexture.a, blendCol.a, blendAlpha);
        }
    }
#endif