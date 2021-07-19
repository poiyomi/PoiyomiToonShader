#ifndef POI_FLIPBOOK
#define POI_FLIPBOOK

#if defined(PROP_FLIPBOOKTEXARRAY) || !defined(OPTIMIZER_ENABLED)
    UNITY_DECLARE_TEX2DARRAY(_FlipbookTexArray); float4 _FlipbookTexArray_ST;
#endif
#if defined(PROP_FLIPBOOKMASK) || !defined(OPTIMIZER_ENABLED)
    POI_TEXTURE_NOSAMPLER(_FlipbookMask);
#endif

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

// anim
float _FlipbookMovementType;
float4 _FlipbookStartEndOffset;
float _FlipbookMovementSpeed;

// Crossfade
float _FlipbookCrossfadeEnabled;
float2 _FlipbookCrossfadeRange;

float _FlipbookHueShiftEnabled;
float _FlipbookHueShiftSpeed;
float _FlipbookHueShift;
// Global
float4 flipBookPixel;
float4 flipBookPixelMultiply;
float flipBookMask;

// Audio Link
half _AudioLinkFlipbookScaleBand;
half4 _AudioLinkFlipbookScale;
half _AudioLinkFlipbookAlphaBand;
half2 _AudioLinkFlipbookAlpha;
half _AudioLinkFlipbookEmissionBand;
half2 _AudioLinkFlipbookEmission;
half _AudioLinkFlipbookFrameBand;
half2 _AudioLinkFlipbookFrame;

#ifndef POI_SHADOW
    
    void applyFlipbook(inout float4 finalColor, inout float3 flipbookEmission)
    {

        #if defined(PROP_FLIPBOOKMASK) || !defined(OPTIMIZER_ENABLED)
            flipBookMask = POI2D_SAMPLER_PAN(_FlipbookMask, _MainTex, poiMesh.uv[_FlipbookMaskUV], _FlipbookMaskPan).r;
        #else
            flipBookMask = 1;
        #endif
        float4 flipbookScaleOffset = _FlipbookScaleOffset;

        #ifdef POI_AUDIOLINK
            flipbookScaleOffset.xy += lerp(_AudioLinkFlipbookScale.xy, _AudioLinkFlipbookScale.zw, poiMods.audioLink[_AudioLinkFlipbookScaleBand]);
        #endif

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
            if (max(newUV.x, newUV.y) > 1 || min(newUV.x, newUV.y) < 0)
            {
                flipBookPixel = 0;
                return;
            }
        }
        #if defined(PROP_FLIPBOOKTEXARRAY) || !defined(OPTIMIZER_ENABLED)
            float currentFrame = fmod(_FlipbookCurrentFrame, _FlipbookTotalFrames);
            if (_FlipbookCurrentFrame < 0)
            {
                currentFrame = (_Time.y / (1 / _FlipbookFPS)) % _FlipbookTotalFrames;
            }
            #ifdef POI_AUDIOLINK
                currentFrame += lerp(_AudioLinkFlipbookFrame.x, _AudioLinkFlipbookFrame.y, poiMods.audioLink[_AudioLinkFlipbookFrameBand]);
            #endif
            flipBookPixel = UNITY_SAMPLE_TEX2DARRAY(_FlipbookTexArray, float3(TRANSFORM_TEX(newUV, _FlipbookTexArray) + _Time.x * _FlipbookTexArrayPan, floor(currentFrame)));
            UNITY_BRANCH
            if (_FlipbookCrossfadeEnabled)
            {
                float4 flipbookNextPixel = UNITY_SAMPLE_TEX2DARRAY(_FlipbookTexArray, float3(TRANSFORM_TEX(newUV, _FlipbookTexArray) + _Time.x * _FlipbookTexArrayPan, floor((currentFrame + 1) % _FlipbookTotalFrames)));
                flipBookPixel = lerp(flipBookPixel, flipbookNextPixel, smoothstep(_FlipbookCrossfadeRange.x, _FlipbookCrossfadeRange.y, frac(currentFrame)));
            }
        #else
            flipBookPixel = 1;
        #endif
        
        UNITY_BRANCH
        if (_FlipbookIntensityControlsAlpha)
        {
            flipBookPixel.a = poiMax(flipBookPixel.rgb);
        }
        UNITY_BRANCH
        if (_FlipbookColorReplaces)
        {
            flipBookPixel.rgb = _FlipbookColor.rgb;
        }
        else
        {
            flipBookPixel.rgb *= _FlipbookColor.rgb;
        }
        
        #ifdef POI_BLACKLIGHT
            UNITY_BRANCH
            if (_BlackLightMaskFlipbook != 4)
            {
                flipBookMask *= blackLightMask[_BlackLightMaskFlipbook];
            }
        #endif
        
        UNITY_BRANCH
        if (_FlipbookHueShiftEnabled)
        {
            flipBookPixel.rgb = hueShift(flipBookPixel.rgb, _FlipbookHueShift + _Time.x * _FlipbookHueShiftSpeed);
        }
        half flipbookAlpha = 1;
        #ifdef POI_AUDIOLINK
            flipbookAlpha = saturate(lerp(_AudioLinkFlipbookAlpha.x, _AudioLinkFlipbookAlpha.y, poiMods.audioLink[_AudioLinkFlipbookAlphaBand]));
        #endif

        finalColor.rgb = lerp(finalColor.rgb, flipBookPixel.rgb, flipBookPixel.a * _FlipbookColor.a * _FlipbookReplace * flipBookMask * flipbookAlpha);
        finalColor.rgb = finalColor + flipBookPixel.rgb * _FlipbookAdd * flipBookMask * flipbookAlpha;
        finalColor.rgb = finalColor * lerp(1, flipBookPixel.rgb, flipBookPixel.a * _FlipbookColor.a * flipBookMask * _FlipbookMultiply * flipbookAlpha);
        
        UNITY_BRANCH
        if (_FlipbookAlphaControlsFinalAlpha)
        {
            finalColor.a = lerp(finalColor.a, flipBookPixel.a * _FlipbookColor.a, flipBookMask);
        }
        float flipbookEmissionStrength = _FlipbookEmissionStrength;
        #ifdef POI_AUDIOLINK
            flipbookEmissionStrength += max(lerp(_AudioLinkFlipbookEmission.x, _AudioLinkFlipbookEmission.y, poiMods.audioLink[_AudioLinkFlipbookEmissionBand]), 0);
        #endif
        flipbookEmission = lerp(0, flipBookPixel.rgb * flipbookEmissionStrength, flipBookPixel.a * _FlipbookColor.a * flipBookMask * flipbookAlpha);
    }
    
#else
    
    float applyFlipbookAlphaToShadow(float2 uv)
    {
        UNITY_BRANCH
        if (_FlipbookAlphaControlsFinalAlpha)
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
            
            #if defined(PROP_FLIPBOOKTEXARRAY) || !defined(OPTIMIZER_ENABLED)
                float currentFrame = fmod(_FlipbookCurrentFrame, _FlipbookTotalFrames);
                if (_FlipbookCurrentFrame < 0)
                {
                    currentFrame = (_Time.y / (1 / _FlipbookFPS)) % _FlipbookTotalFrames;
                }
                
                half4 flipbookColor = UNITY_SAMPLE_TEX2DARRAY(_FlipbookTexArray, float3(TRANSFORM_TEX(newUV, _FlipbookTexArray) + _Time.x * _FlipbookTexArrayPan, floor(currentFrame)));
                UNITY_BRANCH
                if (_FlipbookCrossfadeEnabled)
                {
                    float4 flipbookNextPixel = UNITY_SAMPLE_TEX2DARRAY(_FlipbookTexArray, float3(TRANSFORM_TEX(newUV, _FlipbookTexArray) + _Time.x * _FlipbookTexArrayPan, floor((currentFrame + 1) % _FlipbookTotalFrames)));
                    flipbookColor = lerp(flipbookColor, flipbookNextPixel, smoothstep(_FlipbookCrossfadeRange.x, _FlipbookCrossfadeRange.y, frac(currentFrame)));
                }
            #else
                half4 flipbookColor = 1;
            #endif
            
            if (_FlipbookIntensityControlsAlpha)
            {
                flipbookColor.a = poiMax(flipbookColor.rgb);
            }
            
            UNITY_BRANCH
            if (_FlipbookTiled == 0)
            {
                if (max(newUV.x, newUV.y) > 1 || min(newUV.x, newUV.y) < 0)
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
