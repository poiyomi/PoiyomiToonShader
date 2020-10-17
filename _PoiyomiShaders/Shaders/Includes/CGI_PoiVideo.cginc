#ifndef POI_VIDEO
    #define POI_VIDEO
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_VideoPixelTexture); float4 _VideoPixelTexture_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_VideoMaskTexture); float4 _VideoMaskTexture_ST;
    
    uint _VideoUVNumber;
    uint _VideoType;
    float3 pixels;
    float2 _VideoResolution;
    sampler2D _VideoGameboyRamp;
    half _VideoBacklight;
    half _VideoCRTRefreshRate;
    half _VideoCRTPixelEnergizedTime;
    half _VideoEnableVideoPlayer;
    half _VideoRepeatVideoTexture;
    half _VideoPixelateToResolution;
    float2 _VideoMaskPanning;
    // Video Settings
    half _VideoSaturation;
    half _VideoContrast;
    float2 _VideoTiling;
    float2 _VideoOffset;
    float2 _VideoPanning;
    //Debug
    half _VideoEnableDebug;
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_VideoDebugTexture); float4 _VideoDebugTexture_ST;
    
    
    sampler2D _VRChat_VideoPlayer;
    float4 _VRChat_VideoPlayer_TexelSize;
    
    float4 globalVideoPlayerColor;
    float3 globalColorToDisplayOnScreen;
    float globalVideoOn;
    float3 globalVideoEmission;
    
    float3 applyBacklight(float3 finalColor, half backlightStrength)
    {
        return max(backlightStrength, finalColor.rgb);
    }
    
    float3 applyViewAngleTN(float3 finalColor)
    {
        float3 reflectionVector = normalize(reflect(poiCam.viewDir.rgb, poiMesh.normals[1].rgb));
        float upwardShift = dot(reflectionVector, poiMesh.binormal);
        upwardShift = pow(upwardShift, 1);
        float sideShift = dot(reflectionVector, poiMesh.tangent);
        sideShift *= pow(sideShift, 3);
        #if !UNITY_COLORSPACE_GAMMA
            finalColor = LinearToGammaSpace(finalColor);
        #endif
        finalColor = saturate(lerp(half3(0.5, 0.5, 0.5), finalColor, upwardShift + 1));
        #if !UNITY_COLORSPACE_GAMMA
            finalColor = GammaToLinearSpace(finalColor);
        #endif
        finalColor = (lerp(finalColor, finalColor.gbr, sideShift));
        return finalColor;
    }
    
    float calculateCRTPixelBrightness()
    {
        float totalPixels = _VideoResolution.x * _VideoResolution.y;
        float2 uvPixel = float2((floor((1 - poiMesh.uv[_VideoUVNumber].y) * _VideoResolution.y)) / _VideoResolution.y, (floor(poiMesh.uv[_VideoUVNumber].x * _VideoResolution.x)) / _VideoResolution.x);
        float currentPixelNumber = _VideoResolution.x * (_VideoResolution.y * uvPixel.x) + _VideoResolution.y * uvPixel.y;
        float currentPixelAlpha = currentPixelNumber / totalPixels;
        half electronBeamAlpha = frac(_Time.y * _VideoCRTRefreshRate);
        float electronBeamPixelNumber = totalPixels * electronBeamAlpha;
        
        float DistanceInPixelsFromCurrentElectronBeamPixel = 0;
        if (electronBeamPixelNumber >= currentPixelNumber)
        {
            DistanceInPixelsFromCurrentElectronBeamPixel = electronBeamPixelNumber - currentPixelNumber;
        }
        else
        {
            DistanceInPixelsFromCurrentElectronBeamPixel = electronBeamPixelNumber + (totalPixels - currentPixelNumber);
        }
        float CRTFrameTime = 1 / _VideoCRTRefreshRate;
        float timeSincecurrentPixelWasHitByElectronBeam = (DistanceInPixelsFromCurrentElectronBeamPixel / totalPixels);
        
        return saturate(_VideoCRTPixelEnergizedTime - timeSincecurrentPixelWasHitByElectronBeam);
    }
    
    void applyContrastSettings(inout float3 pixel)
    {
        #if !UNITY_COLORSPACE_GAMMA
            pixel = LinearToGammaSpace(pixel);
        #endif
        pixel = saturate(lerp(half3(0.5, 0.5, 0.5), pixel, _VideoContrast + 1));
        #if !UNITY_COLORSPACE_GAMMA
            pixel = GammaToLinearSpace(pixel);
        #endif
    }
    
    void applySaturationSettings(inout float3 pixel)
    {
        pixel = lerp(pixel.rgb, dot(pixel.rgb, float3(0.3, 0.59, 0.11)), -_VideoSaturation);
    }
    
    void applyVideoSettings(inout float3 pixel)
    {
        applySaturationSettings(pixel);
        applyContrastSettings(pixel);
    }
    
    void calculateLCD(inout float4 finalColor)
    {
        UNITY_BRANCH
        if(_VideoEnableVideoPlayer == 0)
        {
            globalColorToDisplayOnScreen = finalColor;
        }
        globalColorToDisplayOnScreen = applyBacklight(globalColorToDisplayOnScreen, _VideoBacklight * .01);
        applyVideoSettings(globalColorToDisplayOnScreen);
        finalColor.rgb = globalColorToDisplayOnScreen * pixels * _VideoBacklight;
    }
    void calculateTN(inout float4 finalColor)
    {
        if(_VideoEnableVideoPlayer == 0)
        {
            globalColorToDisplayOnScreen = finalColor;
        }
        globalColorToDisplayOnScreen = applyBacklight(globalColorToDisplayOnScreen, _VideoBacklight * .01);
        globalColorToDisplayOnScreen = applyViewAngleTN(globalColorToDisplayOnScreen);
        applyVideoSettings(globalColorToDisplayOnScreen);
        finalColor.rgb = globalColorToDisplayOnScreen * pixels * _VideoBacklight;
    }
    void calculateCRT(inout float4 finalColor)
    {
        UNITY_BRANCH
        if(_VideoEnableVideoPlayer == 0)
        {
            globalColorToDisplayOnScreen = finalColor;
        }
        float brightness = calculateCRTPixelBrightness();
        applyVideoSettings(globalColorToDisplayOnScreen);
        finalColor.rgb = globalColorToDisplayOnScreen * pixels * brightness * _VideoBacklight;
    }
    void calculateOLED(inout float4 finalColor)
    {
        UNITY_BRANCH
        if(_VideoEnableVideoPlayer == 0)
        {
            globalColorToDisplayOnScreen = finalColor;
        }
        applyVideoSettings(globalColorToDisplayOnScreen);
        finalColor.rgb = globalColorToDisplayOnScreen * pixels * _VideoBacklight;
    }
    void calculateGameboy(inout float4 finalColor)
    {
        UNITY_BRANCH
        if(_VideoEnableVideoPlayer == 0)
        {
            globalColorToDisplayOnScreen = finalColor;
        }
        applyVideoSettings(globalColorToDisplayOnScreen);
        half brightness = saturate((globalColorToDisplayOnScreen.r + globalColorToDisplayOnScreen.g + globalColorToDisplayOnScreen.b) * .3333333);
        finalColor.rgb = tex2D(_VideoGameboyRamp, brightness);
    }
    void calculateProjector(inout float4 finalColor, float4 finalColorBeforeLighting)
    {
        UNITY_BRANCH
        if(_VideoEnableVideoPlayer == 0)
        {
            globalColorToDisplayOnScreen = finalColor;
        }
        applyVideoSettings(globalColorToDisplayOnScreen);
        
        float3 projectorColor = finalColorBeforeLighting * globalColorToDisplayOnScreen * _VideoBacklight;
        finalColor.r = clamp(projectorColor.r, finalColor.r, 1000);
        finalColor.g = clamp(projectorColor.g, finalColor.g, 1000);
        finalColor.b = clamp(projectorColor.b, finalColor.b, 1000);
    }
    
    void applyScreenEffect(inout float4 finalColor, float4 finalColorBeforeLighting)
    {
        float4 finalColorBeforeScreen = finalColor;
        
        pixels = UNITY_SAMPLE_TEX2D_SAMPLER(_VideoPixelTexture, _MainTex, TRANSFORM_TEX(poiMesh.uv[_VideoUVNumber], _VideoPixelTexture) * _VideoResolution);
        globalVideoOn = 0;
        UNITY_BRANCH
        if(_VideoEnableVideoPlayer == 1)
        {
            float4 videoTexture = 0;
            UNITY_BRANCH
            if(_VideoPixelateToResolution)
            {
                UNITY_BRANCH
                if(_VideoEnableDebug)
                {
                    videoTexture = UNITY_SAMPLE_TEX2D_SAMPLER(_VideoDebugTexture, _MainTex, round(TRANSFORM_TEX(poiMesh.uv[_VideoUVNumber], _VideoDebugTexture) * _VideoResolution + .5) / _VideoResolution);
                }
                else
                {
                    videoTexture = tex2D(_VRChat_VideoPlayer, round(poiMesh.uv[_VideoUVNumber] * _VideoResolution + .5) / _VideoResolution);
                }
            }
            else
            {
                UNITY_BRANCH
                if(_VideoEnableDebug)
                {
                    videoTexture = UNITY_SAMPLE_TEX2D_SAMPLER(_VideoDebugTexture, _MainTex, TRANSFORM_TEX(poiMesh.uv[_VideoUVNumber], _VideoDebugTexture) * _VideoTiling + _VideoOffset);
                }
                else
                {
                    videoTexture = tex2D(_VRChat_VideoPlayer, ((poiMesh.uv[_VideoUVNumber] + _Time.x * _VideoPanning) * _VideoTiling) + _VideoOffset);
                }
            }
            if(videoTexture.a == 1)
            {
                globalColorToDisplayOnScreen = videoTexture.rgb;
                globalVideoOn = 1;
            }
        }
        
        UNITY_BRANCH
        if(_VideoRepeatVideoTexture == 1)
        {
            if(poiMesh.uv[_VideoUVNumber].x > 1 || poiMesh.uv[_VideoUVNumber].x < 0 || poiMesh.uv[_VideoUVNumber].y > 1 || poiMesh.uv[_VideoUVNumber].y < 0)
            {
                return;
            }
        }
        
        switch(_VideoType)
        {
            case 0: // LCD
            {
                calculateLCD(finalColor);
                break;
            }
            case 1: // TN
            {
                calculateTN(finalColor);
                break;
            }
            case 2: // CRT
            {
                calculateCRT(finalColor);
                break;
            }
            case 3: // OLED
            {
                calculateOLED(finalColor);
                break;
            }
            case 4: // Gameboy
            {
                calculateGameboy(finalColor);
                break;
            }
            case 5: // Projector
            {
                calculateProjector(finalColor, finalColorBeforeLighting);
                break;
            }
        }
        
        float screenMask = UNITY_SAMPLE_TEX2D_SAMPLER(_VideoMaskTexture, _MainTex, TRANSFORM_TEX(poiMesh.uv[_VideoUVNumber], _VideoMaskTexture) + _Time.x * _VideoMaskPanning);
        finalColor = lerp(finalColorBeforeScreen, finalColor, screenMask);
        globalVideoEmission = finalColor.rgb;
    }
    
#endif