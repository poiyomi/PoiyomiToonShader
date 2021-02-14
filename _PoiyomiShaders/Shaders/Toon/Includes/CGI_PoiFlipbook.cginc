#ifndef POI_FLIPBOOK
    #define POI_FLIPBOOK
    
    UNITY_DECLARE_TEX2DARRAY(_FlipbookTexArray); float4 _FlipbookTexArray_ST;
    float4 _FlipbookColor;
    float _FlipbookFPS;
    float _FlipbookTotalFrames;
    float4 _FlipbookScaleOffset;
    float _FlipbookTiled;
    float _FlipbookCurrentFrame;
    float _FlipbookEmissionStrength;
    float _FlipbookRotation;
    float _EnableFlipbook;
    float _FlipbookTexArrayUV;
    float _FlipbookAlphaControlsFinalAlpha;
    float _FlipbookRotationSpeed;
    float _FlipbookIntensityControlsAlpha;
    float _FlipbookColorReplaces;
    float2 _FlipbookTexArrayPan;
    
    // blending
    float _FlipbookReplace;
    float _FlipbookMultiply;
    float _FlipbookAdd;
    POI_TEXTURE_NOSAMPLER(_FlipbookMask);
    
    // anim
    float _FlipbookMovementType;
    float4 _FlipbookStartEndOffset;
    float _FlipbookMovementSpeed;
    
    // Global
    float4 flipBookPixel;
    float4 flipBookPixelMultiply;
    float flipBookMask;
    
    #ifndef POI_SHADOW
        
        void applyFlipbook(inout float4 finalColor, inout float3 flipbookEmission)
        {
            flipBookMask = POI2D_SAMPLER_PAN(_FlipbookMask, _MainTex, poiMesh.uv[_FlipbookMaskUV], _FlipbookMaskPan).r;
            float4 flipbookScaleOffset = _FlipbookScaleOffset;
            flipbookScaleOffset.xy = 1 - flipbookScaleOffset.xy;
            float2 uv = frac(poiMesh.uv[_FlipbookTexArrayUV]);
            float theta = radians(_FlipbookRotation + _Time.z * _FlipbookRotationSpeed);
            float cs = cos(theta);
            float sn = sin(theta);
            float2 spriteCenter = flipbookScaleOffset.zw + .5;
            // 2d rotation
            uv = float2((uv.x - spriteCenter.x) * cs - (uv.y - spriteCenter.y) * sn + spriteCenter.x, (uv.x - spriteCenter.x) * sn + (uv.y - spriteCenter.y) * cs + spriteCenter.y);
            
            float2 newUV = remap(uv, float2(0, 0) + flipbookScaleOffset.xy / 2 + flipbookScaleOffset.zw, float2(1, 1) - flipbookScaleOffset.xy / 2 + flipbookScaleOffset.zw, float2(0, 0), float2(1, 1));
            
            UNITY_BRANCH
            if (_FlipbookTiled == 0)
            {
                if(max(newUV.x, newUV.y) > 1 || min(newUV.x, newUV.y) < 0)
                {
                    flipBookPixel = 0;
                    return;
                }
            }
            
            uint currentFrame = floor(_FlipbookCurrentFrame) % _FlipbookTotalFrames;
            if(_FlipbookCurrentFrame < 0)
            {
                currentFrame = (_Time.y / (1 / _FlipbookFPS)) % _FlipbookTotalFrames;
            }
            flipBookPixel = UNITY_SAMPLE_TEX2DARRAY(_FlipbookTexArray, float3(TRANSFORM_TEX(newUV, _FlipbookTexArray) + _Time.x * _FlipbookTexArrayPan, currentFrame));
            UNITY_BRANCH
            if(_FlipbookIntensityControlsAlpha)
            {
                flipBookPixel.a = poiMax(flipBookPixel.rgb);
            }
            UNITY_BRANCH
            if(_FlipbookColorReplaces)
            {
                flipBookPixel.rgb = _FlipbookColor.rgb;
            }
            else
            {
                flipBookPixel.rgb *= _FlipbookColor.rgb;
            }
            
            #ifdef POI_BLACKLIGHT
                UNITY_BRANCH
                if(_BlackLightMaskFlipbook != 4)
                {
                    flipBookMask *= blackLightMask[_BlackLightMaskFlipbook];
                }
            #endif
            
            finalColor.rgb = lerp(finalColor, flipBookPixel.rgb, flipBookPixel.a * _FlipbookColor.a * _FlipbookReplace * flipBookMask);
            finalColor.rgb = finalColor + flipBookPixel.rgb * _FlipbookAdd * flipBookMask;
            finalColor.rgb = finalColor * lerp(1, flipBookPixel.rgb, flipBookPixel.a * _FlipbookColor.a * flipBookMask * _FlipbookMultiply);
            
            UNITY_BRANCH
            if(_FlipbookAlphaControlsFinalAlpha)
            {
                finalColor.a = lerp(finalColor.a, flipBookPixel.a * _FlipbookColor.a, flipBookMask);
            }
            flipbookEmission = lerp(0, flipBookPixel.rgb * _FlipbookEmissionStrength, flipBookPixel.a * _FlipbookColor.a * flipBookMask);
        }
        
    #else
        
        float applyFlipbookAlphaToShadow(float2 uv)
        {
            UNITY_BRANCH
            if(_FlipbookAlphaControlsFinalAlpha)
            {
                float flipbookShadowAlpha = 0;
                
                float4 flipbookScaleOffset = _FlipbookScaleOffset;
                flipbookScaleOffset.xy = 1 - flipbookScaleOffset.xy;
                float theta = radians(_FlipbookRotation);
                
                float cs = cos(theta);
                float sn = sin(theta);
                float2 spriteCenter = flipbookScaleOffset.zw + .5;
                uv = float2((uv.x - spriteCenter.x) * cs - (uv.y - spriteCenter.y) * sn + spriteCenter.x, (uv.x - spriteCenter.x) * sn + (uv.y - spriteCenter.y) * cs + spriteCenter.y);
                
                float2 newUV = remap(uv, float2(0, 0) + flipbookScaleOffset.xy / 2 + flipbookScaleOffset.zw, float2(1, 1) - flipbookScaleOffset.xy / 2 + flipbookScaleOffset.zw, float2(0, 0), float2(1, 1));
                
                
                uint currentFrame = floor(_FlipbookCurrentFrame) % _FlipbookTotalFrames;
                if(_FlipbookCurrentFrame < 0)
                {
                    currentFrame = (_Time.y / (1 / _FlipbookFPS)) % _FlipbookTotalFrames;
                }
                half4 flipbookColor = UNITY_SAMPLE_TEX2DARRAY(_FlipbookTexArray, float3(TRANSFORM_TEX(newUV, _FlipbookTexArray) + _Time.x * _FlipbookTexArrayPan, currentFrame));
                
                if(_FlipbookIntensityControlsAlpha)
                {
                    flipbookColor.a = poiMax(flipbookColor.rgb);
                }
                
                UNITY_BRANCH
                if(_FlipbookTiled == 0)
                {
                    if(max(newUV.x, newUV.y) > 1 || min(newUV.x, newUV.y) < 0)
                    {
                        flipbookColor.a = 0;
                    }
                }
                return flipbookColor.a * _FlipbookColor.a;
            }
            return 1;
        }
        
    #endif
#endif
