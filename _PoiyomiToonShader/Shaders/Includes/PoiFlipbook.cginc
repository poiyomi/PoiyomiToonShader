#ifndef FLIPBOOK
    #define FLIPBOOK
    
    UNITY_DECLARE_TEX2DARRAY(_FlipbookTexArray); float4 _FlipbookTexArray_ST;
    float4 _FlipbookColor;
    float _FlipbookFPS;
    uint _FlipbookTotalFrames;
    float4 _FlipbookScaleOffset;
    float _FlipbookTiled;
    int _FlipbookCurrentFrame;
    float _FlipbookEmissionStrength;
    float _FlipbookRotation;
    
    // blending
    float _FlipbookReplace;
    float _FlipbookMultiply;
    float _FlipbookAdd;
    
    // anim
    uint _FlipbookMovementType;
    float4 _FlipbookStartEndOffset;
    float _FlipbookMovementSpeed;
    
    // Global
    float4 flipBookPixel;
    float4 flipBookPixelMultiply;
    void calculateFlipbook(float2 uv)
    {
        _FlipbookScaleOffset.xy = 1 - _FlipbookScaleOffset.xy;
        
        uv = remap(uv, float2(0, 0) + _FlipbookScaleOffset.xy / 2 + _FlipbookScaleOffset.zw, float2(1, 1) - _FlipbookScaleOffset.xy / 2 + _FlipbookScaleOffset.zw, float2(0, 0), float2(1, 1));
        float theta = radians(_FlipbookRotation);
        
        float cs = cos(theta);
        float sn = sin(theta);
        
        float2 newUV = float2((uv.x - .5) * cs - (uv.y - .5) * sn + .5, (uv.x - .5) * sn + (uv.y - .5) * cs + .5);
        
        UNITY_BRANCH
        if (_FlipbookTiled == 0)
        {
            if(max(newUV.x, newUV.y) > 1 || min(newUV.x, newUV.y) < 0)
            {
                flipBookPixel = 0;
                flipBookPixelMultiply = 1;
                return;
            }
        }
        
        uint currentFrame = floor(_FlipbookCurrentFrame) % _FlipbookTotalFrames;
        if(_FlipbookCurrentFrame < 0)
        {
            currentFrame = (_Time.y / (1 / _FlipbookFPS)) % _FlipbookTotalFrames;
        }
        flipBookPixel = UNITY_SAMPLE_TEX2DARRAY(_FlipbookTexArray, float3(TRANSFORM_TEX(newUV, _FlipbookTexArray),currentFrame));
        flipBookPixelMultiply = flipBookPixel;
    }
    
    void applyFlipbook(inout float4 finalColor)
    {
        finalColor.rgb = lerp(finalColor, flipBookPixel.rgb * _FlipbookColor.rgb, flipBookPixel.a * _FlipbookColor.a * _FlipbookReplace);
        finalColor.rgb = finalColor + flipBookPixel.rgb * _FlipbookColor.rgb * _FlipbookAdd;
        finalColor.rgb = finalColor * lerp(1, flipBookPixelMultiply.rgb * _FlipbookColor.rgb, _FlipbookMultiply * flipBookPixelMultiply.a * _FlipbookColor.a);
    }
    
    void applyFlipbookEmission(inout float4 finalColor)
    {
        finalColor.rgb += lerp(0, flipBookPixel.rgb * _FlipbookColor.rgb * _FlipbookEmissionStrength, flipBookPixel.a * _FlipbookColor.a);
    }
    
#endif
