#ifndef POI_MSDF
    #define POI_MSDF
    
    sampler2D _TextGlyphs; float4 _TextGlyphs_ST; float4 _TextGlyphs_TexelSize;
    uint _TextFPSUV, _TextTimeUV, _TextPositionUV;
    float _TextPixelRange;
    
    float _TextFPSEnabled, _TextPositionEnabled, _TextTimeEnabled;
    
    
    float4 _TextFPSColor;
    half _TextFPSEmissionStrength;
    fixed4 _TextFPSPadding;
    half2 _TextFPSOffset, _TextFPSScale;
    half _TextFPSRotation;
    
    fixed _TextPositionVertical;
    float4 _TextPositionColor;
    half _TextPositionEmissionStrength;
    fixed4 _TextPositionPadding;
    half2 _TextPositionOffset, _TextPositionScale;
    half _TextPositionRotation;
    
    float4 _TextTimeColor;
    half _TextTimeEmissionStrength;
    fixed4 _TextTimePadding;
    half2 _TextTimeOffset, _TextTimeScale;
    half _TextTimeRotation;
    
    #define glyphWidth 0.0625
    
    #define ASCII_LEFT_PARENTHESIS 40
    #define ASCII_RIGHT_PARENTHESIS 41
    #define ASCII_POSITIVE 43
    #define ASCII_PERIOD 46
    #define ASCII_NEGATIVE 45
    #define ASCII_COMMA 44
    #define ASCII_E 69
    #define ASCII_F 70
    #define ASCII_I 73
    #define ASCII_M 77
    #define ASCII_O 79
    #define ASCII_P 80
    #define ASCII_S 83
    #define ASCII_T 54
    #define ASCII_SEMICOLON 58
    
    float3 globalTextEmission;
    
    half2 getAsciiCoordinate(float index)
    {
        return half2((index - 1) / 16, 1 - ((floor(index / 16 - glyphWidth)) / 16));
    }
    
    float median(float r, float g, float b)
    {
        return max(min(r, g), min(max(r, g), b));
    }
    
    void ApplyPositionText(inout float4 albedo, float2 uv)
    {
        float3 cameraPos = clamp(getCameraPosition(), -999, 999);
        float3 absCameraPos = abs(cameraPos);
        float totalCharacters = 20;
        float positionArray[20];
        positionArray[0] = cameraPos.x >= 0 ? ASCII_NEGATIVE: ASCII_POSITIVE;
        positionArray[1] = floor((absCameraPos.x * .01) % 10) + 48;
        positionArray[2] = floor((absCameraPos.x * .1) % 10) + 48;
        positionArray[3] = floor(absCameraPos.x % 10) + 48;
        positionArray[4] = ASCII_PERIOD;
        positionArray[5] = floor((absCameraPos.x * 10) % 10) + 48;
        positionArray[6] = ASCII_COMMA;
        positionArray[7] = cameraPos.y >= 0 ? ASCII_NEGATIVE: ASCII_POSITIVE;
        positionArray[8] = floor((absCameraPos.y * .01) % 10) + 48;
        positionArray[9] = floor((absCameraPos.y * .1) % 10) + 48;
        positionArray[10] = floor(absCameraPos.y % 10) + 48;
        positionArray[11] = ASCII_PERIOD;
        positionArray[12] = floor((absCameraPos.y * 10) % 10) + 48;
        positionArray[13] = ASCII_COMMA;
        positionArray[14] = cameraPos.z >= 0 ? ASCII_NEGATIVE: ASCII_POSITIVE;
        positionArray[15] = floor((absCameraPos.z * .01) % 10) + 48;
        positionArray[16] = floor((absCameraPos.z * .1) % 10) + 48;
        positionArray[17] = floor(absCameraPos.z % 10) + 48;
        positionArray[18] = ASCII_PERIOD;
        positionArray[19] = floor((absCameraPos.z * 10) % 10) + 48;
        
        uv = TransformUV(_TextPositionOffset, _TextPositionRotation, _TextPositionScale, uv);
        
        if (uv.x > 1 || uv.x < 0 || uv.y > 1 || uv.y < 0)
        {
            return;
        }
        
        float currentCharacter = floor(uv.x * totalCharacters);
        half2 glyphPos = getAsciiCoordinate(positionArray[currentCharacter]);
        
        float2 startUV = float2(1 / totalCharacters * currentCharacter, 0);
        float2 endUV = float2(1 / totalCharacters * (currentCharacter + 1), 1);
        
        _TextPositionPadding *= 1 / totalCharacters;
        uv = remapClamped(uv, startUV, endUV, float2(glyphPos.x + _TextPositionPadding.x, glyphPos.y - glyphWidth + _TextPositionPadding.y), float2(glyphPos.x + glyphWidth - _TextPositionPadding.z, glyphPos.y - _TextPositionPadding.w));
                
        if (uv.x > glyphPos.x + glyphWidth - _TextPositionPadding.z - .001 || uv.x < glyphPos.x + _TextPositionPadding.x + .001 || uv.y > glyphPos.y - _TextPositionPadding.w - .001 || uv.y < glyphPos.y - glyphWidth + _TextPositionPadding.y + .001)
        {
            return;
        }
        
        float3 samp = tex2D(_TextGlyphs, TRANSFORM_TEX(uv, _TextGlyphs)).rgb;
        float2 msdfUnit = _TextPixelRange / _TextGlyphs_TexelSize.zw;
        float sigDist = median(samp.r, samp.g, samp.b) - 0.5;
        sigDist *= max(dot(msdfUnit, 0.5 / fwidth(uv)), 1);
        float opacity = clamp(sigDist + 0.5, 0, 1);
        albedo.rgb = lerp(albedo.rgb, _TextPositionColor.rgb, opacity * _TextPositionColor.a);
        globalTextEmission += _TextPositionColor.rgb * opacity * _TextPositionEmissionStrength;
    }
    
    void ApplyTimeText(inout float4 albedo, float2 uv)
    {
        float instanceTime = _Time.y;
        float hours = instanceTime / 3600;
        float minutes = (instanceTime / 60) % 60;
        float seconds = instanceTime % 60;
        float totalCharacters = 8;
        float timeArray[8];
        timeArray[0] = floor((hours * .1) % 10) + 48;
        timeArray[1] = floor(hours % 10) + 48;
        timeArray[2] = ASCII_SEMICOLON;
        timeArray[3] = floor((minutes * .1) % 10) + 48;
        timeArray[4] = floor(minutes % 10) + 48;
        timeArray[5] = ASCII_SEMICOLON;
        timeArray[6] = floor((seconds * .1) % 10) + 48;
        timeArray[7] = floor(seconds % 10) + 48;
        
        uv = TransformUV(_TextTimeOffset, _TextTimeRotation, _TextTimeScale, uv);
        
        if(uv.x > 1 || uv.x < 0 || uv.y > 1 || uv.y < 0)
        {
            return;
        }
        
        float currentCharacter = floor(uv.x * totalCharacters);
        half2 glyphPos = getAsciiCoordinate(timeArray[currentCharacter]);
        // 0.1428571 = 1/7 = 1 / totalCharacters
        float startUV = 1 / totalCharacters * currentCharacter;
        float endUV = 1 / totalCharacters * (currentCharacter + 1);
        _TextTimePadding *= 1 / totalCharacters;
        uv = remapClamped(uv, float2(startUV, 0), float2(endUV, 1), float2(glyphPos.x + _TextTimePadding.x, glyphPos.y - glyphWidth + _TextTimePadding.y), float2(glyphPos.x + glyphWidth - _TextTimePadding.z, glyphPos.y - _TextTimePadding.w));
        
        if (uv.x > glyphPos.x + glyphWidth - _TextTimePadding.z - .001 || uv.x < glyphPos.x + _TextTimePadding.x + .001 || uv.y > glyphPos.y - _TextTimePadding.w - .001 || uv.y < glyphPos.y - glyphWidth + _TextTimePadding.y + .001)
        {
            return;
        }

        float3 samp = tex2D(_TextGlyphs, TRANSFORM_TEX(uv, _TextGlyphs)).rgb;
        float2 msdfUnit = _TextPixelRange / _TextGlyphs_TexelSize.zw;
        float sigDist = median(samp.r, samp.g, samp.b) - 0.5;
        sigDist *= max(dot(msdfUnit, 0.5 / fwidth(uv)), 1);
        float opacity = clamp(sigDist + 0.5, 0, 1);
        albedo.rgb = lerp(albedo.rgb, _TextTimeColor.rgb, opacity * _TextTimeColor.a);
        globalTextEmission += _TextTimeColor.rgb * opacity * _TextTimeEmissionStrength;
    }
    
    void ApplyFPSText(inout float4 albedo, float2 uv)
    {
        float smoothDeltaTime = clamp(unity_DeltaTime.w, 0, 999);
        float totalCharacters = 7;
        float fpsArray[7];
        fpsArray[0] = ASCII_F;
        fpsArray[1] = ASCII_P;
        fpsArray[2] = ASCII_S;
        fpsArray[3] = ASCII_SEMICOLON;
        fpsArray[4] = floor((smoothDeltaTime * .01) % 10) + 48;
        fpsArray[5] = floor((smoothDeltaTime * .1) % 10) + 48;
        fpsArray[6] = floor(smoothDeltaTime % 10) + 48;
        
        uv = TransformUV(_TextFPSOffset, _TextFPSRotation, _TextFPSScale, uv);
        
        if(uv.x > 1 || uv.x < 0 || uv.y > 1 || uv.y < 0)
        {
            return;
        }
        
        float currentCharacter = floor(uv.x * totalCharacters);
        half2 glyphPos = getAsciiCoordinate(fpsArray[currentCharacter]);
        // 0.1428571 = 1/7 = 1 / totalCharacters
        float startUV = 1 / totalCharacters * currentCharacter;
        float endUV = 1 / totalCharacters * (currentCharacter + 1);
        _TextFPSPadding *= 1 / totalCharacters;
        uv = remapClamped(uv, float2(startUV, 0), float2(endUV, 1), float2(glyphPos.x + _TextFPSPadding.x, glyphPos.y - glyphWidth + _TextFPSPadding.y), float2(glyphPos.x + glyphWidth - _TextFPSPadding.z, glyphPos.y - _TextFPSPadding.w));
        
        if (uv.x > glyphPos.x + glyphWidth - _TextFPSPadding.z - .001 || uv.x < glyphPos.x + _TextFPSPadding.x + .001 || uv.y > glyphPos.y - _TextFPSPadding.w - .001 || uv.y < glyphPos.y - glyphWidth + _TextFPSPadding.y + .001)
        {
            return;
        }
        
        float3 samp = tex2D(_TextGlyphs, TRANSFORM_TEX(uv, _TextGlyphs)).rgb;
        float2 msdfUnit = _TextPixelRange / _TextGlyphs_TexelSize.zw;
        float sigDist = median(samp.r, samp.g, samp.b) - 0.5;
        sigDist *= max(dot(msdfUnit, 0.5 / fwidth(uv)), 1);
        float opacity = clamp(sigDist + 0.5, 0, 1);
        albedo.rgb = lerp(albedo.rgb, _TextFPSColor.rgb, opacity * _TextFPSColor.a);
        globalTextEmission += _TextFPSColor.rgb * opacity * _TextFPSEmissionStrength;
    }
    
    void ApplyTextOverlayColor(inout float4 albedo)
    {
        globalTextEmission = 0;
        half positionalOpacity = 0;
        #ifdef EFFECT_BUMP
            UNITY_BRANCH
            if(_TextFPSEnabled)
            {
                ApplyFPSText(albedo, poiMesh.uv[_TextFPSUV]);
            }
            UNITY_BRANCH
            if(_TextPositionEnabled)
            {
                ApplyPositionText(albedo, poiMesh.uv[_TextPositionUV]);
            }
            UNITY_BRANCH
            if(_TextTimeEnabled)
            {
                ApplyTimeText(albedo, poiMesh.uv[_TextTimeUV]);
            }
        #endif
    }
    
    void ApplyTextOverlayEmission(inout float3 emission)
    {
        emission += globalTextEmission;
    }
    
    
    
#endif