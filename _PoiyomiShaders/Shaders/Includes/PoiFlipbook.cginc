#ifndef POI_FLIPBOOK
    #define POI_FLIPBOOK
    
    UNITY_DECLARE_TEX2DARRAY(_FlipbookTexArray); float4 _FlipbookTexArray_ST;
    float4 _FlipbookColor;
    float _FlipbookFPS;
    uint _FlipbookTotalFrames;
    float4 _FlipbookScaleOffset;
    float _FlipbookTiled;
    int _FlipbookCurrentFrame;
    float _FlipbookEmissionStrength;
    float _FlipbookRotation;
    float _EnableFlipbook;
    uint _FlipbookUV;
    float _FlipbookAlphaControlsFinalAlpha;
    // blending
    float _FlipbookReplace;
    float _FlipbookMultiply;
    float _FlipbookAdd;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_FlipbookMask); float4 _FlipbookMask_ST;
    
    // anim
    uint _FlipbookMovementType;
    float4 _FlipbookStartEndOffset;
    float _FlipbookMovementSpeed;
    
    // Global
    float4 flipBookPixel;
    float4 flipBookPixelMultiply;
    float flipBookMask;

    #ifndef POI_SHADOW
        
        void calculateFlipbook()
        {
            flipBookMask = UNITY_SAMPLE_TEX2D_SAMPLER(_FlipbookMask, _MainTex, TRANSFORM_TEX(poiMesh.uv[_FlipbookUV], _FlipbookMask)).r;
            _FlipbookScaleOffset.xy = 1 - _FlipbookScaleOffset.xy;
            float2 uv = poiMesh.uv[_FlipbookUV];
            float theta = radians(_FlipbookRotation);
            float cs = cos(theta);
            float sn = sin(theta);
            float2 spriteCenter = _FlipbookScaleOffset.zw + .5;
            // 2d rotation
            uv = float2((uv.x - spriteCenter.x) * cs - (uv.y - spriteCenter.y) * sn + spriteCenter.x, (uv.x - spriteCenter.x) * sn + (uv.y - spriteCenter.y) * cs + spriteCenter.y);
            
            float2 newUV = remap(uv, float2(0, 0) + _FlipbookScaleOffset.xy / 2 + _FlipbookScaleOffset.zw, float2(1, 1) - _FlipbookScaleOffset.xy / 2 + _FlipbookScaleOffset.zw, float2(0, 0), float2(1, 1));
            
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
            flipBookPixel = UNITY_SAMPLE_TEX2DARRAY(_FlipbookTexArray, float3(TRANSFORM_TEX(newUV, _FlipbookTexArray), currentFrame));
            flipBookPixelMultiply = flipBookPixel;
        }
        void applyFlipbook(inout float4 finalColor)
        {
            finalColor.rgb = lerp(finalColor, flipBookPixel.rgb * _FlipbookColor.rgb, flipBookPixel.a * _FlipbookColor.a * _FlipbookReplace * flipBookMask);
            finalColor.rgb = finalColor + flipBookPixel.rgb * _FlipbookColor.rgb * _FlipbookAdd * flipBookMask;
            finalColor.rgb = finalColor * lerp(1, flipBookPixelMultiply.rgb * _FlipbookColor.rgb, _FlipbookMultiply * flipBookPixelMultiply.a * _FlipbookColor.a * flipBookMask);
            
            UNITY_BRANCH
            if(_FlipbookAlphaControlsFinalAlpha)
            {
                finalColor.a = lerp(finalColor.a, flipBookPixel.a * _FlipbookColor.a, flipBookMask);
            }
        }
        void applyFlipbookEmission(inout float3 finalEmission)
        {
            finalEmission += lerp(0, flipBookPixel.rgb * _FlipbookColor.rgb * _FlipbookEmissionStrength, flipBookPixel.a * _FlipbookColor.a * flipBookMask);
        }
        
    #else
        
        float applyFlipbookAlphaToShadow(float2 uv)
        {
            UNITY_BRANCH
            if(_FlipbookAlphaControlsFinalAlpha)
            {
                float flipbookShadowAlpha = 0;
                
                _FlipbookScaleOffset.xy = 1 - _FlipbookScaleOffset.xy;
                float theta = radians(_FlipbookRotation);
                
                float cs = cos(theta);
                float sn = sin(theta);
                float2 spriteCenter = _FlipbookScaleOffset.zw + .5;
                uv = float2((uv.x - spriteCenter.x) * cs - (uv.y - spriteCenter.y) * sn + spriteCenter.x, (uv.x - spriteCenter.x) * sn + (uv.y - spriteCenter.y) * cs + spriteCenter.y);
                
                float2 newUV = remap(uv, float2(0, 0) + _FlipbookScaleOffset.xy / 2 + _FlipbookScaleOffset.zw, float2(1, 1) - _FlipbookScaleOffset.xy / 2 + _FlipbookScaleOffset.zw, float2(0, 0), float2(1, 1));

                
                uint currentFrame = floor(_FlipbookCurrentFrame) % _FlipbookTotalFrames;
                if(_FlipbookCurrentFrame < 0)
                {
                    currentFrame = (_Time.y / (1 / _FlipbookFPS)) % _FlipbookTotalFrames;
                }
                flipbookShadowAlpha = UNITY_SAMPLE_TEX2DARRAY(_FlipbookTexArray, float3(TRANSFORM_TEX(newUV, _FlipbookTexArray), currentFrame)).a;
                
                UNITY_BRANCH
                if(_FlipbookTiled == 0)
                {
                    if(max(newUV.x, newUV.y) > 1 || min(newUV.x, newUV.y) < 0)
                    {
                        flipbookShadowAlpha = 0;
                    }
                }
                return flipbookShadowAlpha * _FlipbookColor.a;
            }
            return 1;
        }
        
    #endif
#endif
