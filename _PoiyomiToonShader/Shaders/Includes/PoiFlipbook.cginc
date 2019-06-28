#ifndef FLIPBOOK
    #define FLIPBOOK
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_FlipbookTexture); float4 _FlipbookTexture_ST;
    float4 _FlipbookColor;
    float _FlipbookRows;
    float _FlipbookColumns;
    float _FlipbookFPS;
    uint _FlipbookTotalFrames;
    float4 _FlipbookScaleOffset;
    int _FlipbookCurrentFrame;
    float _FlipbookEmissionStrength;
    float _FlipbookRotation;
    
    float4 flipBookPixel;
    void calculateFlipbook(float2 uv)
    {
        _FlipbookScaleOffset.xy = 1 - _FlipbookScaleOffset.xy;
        
        uv = remap(uv, float2(0, 0) + _FlipbookScaleOffset.xy / 2 + _FlipbookScaleOffset.zw, float2(1, 1) - _FlipbookScaleOffset.xy / 2 + _FlipbookScaleOffset.zw, float2(0, 0), float2(1, 1));
        float theta = radians(_FlipbookRotation);
        
        float cs = cos(theta);
        float sn = sin(theta);
        
        float2 newUV = float2((uv.x - .5) * cs - (uv.y - .5) * sn + .5, (uv.x - .5) * sn + (uv.y - .5) * cs + .5);
        
        if (max(newUV.x, newUV.y) > 1 || min(newUV.x, newUV.y) < 0)
        {
            flipBookPixel = 0;
            return;
        }
        uint currentFrame = floor(_FlipbookCurrentFrame) % _FlipbookTotalFrames;
        if(_FlipbookCurrentFrame < 0)
        {
            currentFrame = (_Time.y / (1 / _FlipbookFPS)) % _FlipbookTotalFrames;
        }
        float2 uvScaler = float2(1.0 / _FlipbookColumns, 1.0 / _FlipbookRows);
        float2 uVOffset = float2(frac((currentFrame + 1) / _FlipbookColumns) - 1 / _FlipbookColumns, (1 - saturate(floor((currentFrame / _FlipbookColumns)) / _FlipbookRows)) - 1 / _FlipbookRows);
        flipBookPixel = UNITY_SAMPLE_TEX2D_SAMPLER(_FlipbookTexture, _MainTex, TRANSFORM_TEX(newUV * uvScaler + uVOffset, _FlipbookTexture));
    }
    
    void applyFlipbook(inout float4 finalColor)
    {
        finalColor.rgb = lerp(finalColor, flipBookPixel.rgb * _FlipbookColor.rgb, flipBookPixel.a * _FlipbookColor.a);
    }
    
    void applyFlipbookEmission(inout float4 finalColor)
    {
        finalColor.rgb += lerp(0, flipBookPixel.rgb * _FlipbookColor.rgb * _FlipbookEmissionStrength, flipBookPixel.a * _FlipbookColor.a);
    }
    
#endif