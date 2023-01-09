#ifndef POI_BLENDING
    #define POI_BLENDING
    
    /*
    0: Zero	                float4(0.0, 0.0, 0.0, 0.0),
    1: One	                float4(1.0, 1.0, 1.0, 1.0),
    2: DstColor	            destinationColor,
    3: SrcColor	            sourceColor,
    4: OneMinusDstColor	    float4(1.0, 1.0, 1.0, 1.0) - destinationColor,
    5: SrcAlpha	            sourceColor.aaaa,
    6: OneMinusSrcColor	    float4(1.0, 1.0, 1.0, 1.0) - sourceColor,
    7: DstAlpha	            destinationColor.aaaa,
    8: OneMinusDstAlpha	    float4(1.0, 1.0, 1.0, 1.0) - destinationColor.,
    9: SrcAlphaSaturate     saturate(sourceColor.aaaa),
    10: OneMinusSrcAlpha	float4(1.0, 1.0, 1.0, 1.0) - sourceColor.aaaa,
    */
    
    float4 poiBlend(const float sourceFactor, const  float4 sourceColor, const  float destinationFactor, const  float4 destinationColor, const float4 blendFactor)
    {
        float4 sA = 1 - blendFactor;
        const float4 blendData[11] = {
            float4(0.0, 0.0, 0.0, 0.0),
            float4(1.0, 1.0, 1.0, 1.0),
            destinationColor,
            sourceColor,
            float4(1.0, 1.0, 1.0, 1.0) - destinationColor,
            sA,
            float4(1.0, 1.0, 1.0, 1.0) - sourceColor,
            sA,
            float4(1.0, 1.0, 1.0, 1.0) - sA,
            saturate(sourceColor.aaaa),
            1 - sA,
        };
        
        return lerp(blendData[sourceFactor] * sourceColor + blendData[destinationFactor] * destinationColor, sourceColor, sA);
    }
    
    // Average
    float3 blendAverage(float3 base, float3 blend)
    {
        return(base + blend) / 2.0;
    }
    
    // Color burn
    float blendColorBurn(float base, float blend)
    {
        return(blend == 0.0)?blend: max((1.0 - ((1.0 - base) / blend)), 0.0);
    }
    
    float3 blendColorBurn(float3 base, float3 blend)
    {
        return float3(blendColorBurn(base.r, blend.r), blendColorBurn(base.g, blend.g), blendColorBurn(base.b, blend.b));
    }
    
    // Color Dodge
    float blendColorDodge(float base, float blend)
    {
        return(blend == 1.0)?blend: min(base / (1.0 - blend), 1.0);
    }
    
    float3 blendColorDodge(float3 base, float3 blend)
    {
        return float3(blendColorDodge(base.r, blend.r), blendColorDodge(base.g, blend.g), blendColorDodge(base.b, blend.b));
    }
    
    // Darken
    float blendDarken(float base, float blend)
    {
        return min(blend, base);
    }
    
    float3 blendDarken(float3 base, float3 blend)
    {
        return float3(blendDarken(base.r, blend.r), blendDarken(base.g, blend.g), blendDarken(base.b, blend.b));
    }
    
    // Exclusion
    float3 blendExclusion(float3 base, float3 blend)
    {
        return base + blend - 2.0 * base * blend;
    }
    
    // Reflect
    float blendReflect(float base, float blend)
    {
        return(blend == 1.0)?blend: min(base * base / (1.0 - blend), 1.0);
    }
    
    float3 blendReflect(float3 base, float3 blend)
    {
        return float3(blendReflect(base.r, blend.r), blendReflect(base.g, blend.g), blendReflect(base.b, blend.b));
    }
    
    // Glow
    float3 blendGlow(float3 base, float3 blend)
    {
        return blendReflect(blend, base);
    }
    
    // Overlay
    float blendOverlay(float base, float blend)
    {
        return base < 0.5?(2.0 * base * blend): (1.0 - 2.0 * (1.0 - base) * (1.0 - blend));
    }
    
    float3 blendOverlay(float3 base, float3 blend)
    {
        return float3(blendOverlay(base.r, blend.r), blendOverlay(base.g, blend.g), blendOverlay(base.b, blend.b));
    }
    
    // Hard Light
    float3 blendHardLight(float3 base, float3 blend)
    {
        return blendOverlay(blend, base);
    }
    
    // Vivid light
    float blendVividLight(float base, float blend)
    {
        return(blend < 0.5)?blendColorBurn(base, (2.0 * blend)): blendColorDodge(base, (2.0 * (blend - 0.5)));
    }
    
    float3 blendVividLight(float3 base, float3 blend)
    {
        return float3(blendVividLight(base.r, blend.r), blendVividLight(base.g, blend.g), blendVividLight(base.b, blend.b));
    }
    
    // Hard mix
    float blendHardMix(float base, float blend)
    {
        return(blendVividLight(base, blend) < 0.5)?0.0: 1.0;
    }
    
    float3 blendHardMix(float3 base, float3 blend)
    {
        return float3(blendHardMix(base.r, blend.r), blendHardMix(base.g, blend.g), blendHardMix(base.b, blend.b));
    }
    
    // Lighten
    float blendLighten(float base, float blend)
    {
        return max(blend, base);
    }
    
    float3 blendLighten(float3 base, float3 blend)
    {
        return float3(blendLighten(base.r, blend.r), blendLighten(base.g, blend.g), blendLighten(base.b, blend.b));
    }
    
    // Linear Burn
    float blendLinearBurn(float base, float blend)
    {
        // Note : Same implementation as BlendSubtractf
        return max(base + blend - 1.0, 0.0);
    }
    
    float3 blendLinearBurn(float3 base, float3 blend)
    {
        // Note : Same implementation as BlendSubtract
        return max(base + blend - float3(1.0, 1.0, 1.0), float3(0.0, 0.0, 0.0));
    }
    
    // Linear Dodge
    float blendLinearDodge(float base, float blend)
    {
        // Note : Same implementation as BlendAddf
        return min(base + blend, 1.0);
    }
    
    float3 blendLinearDodge(float3 base, float3 blend)
    {
        // Note : Same implementation as BlendAdd
        return min(base + blend, float3(1.0, 1.0, 1.0));
    }
    
    // Linear light
    float blendLinearLight(float base, float blend)
    {
        return blend < 0.5?blendLinearBurn(base, (2.0 * blend)): blendLinearDodge(base, (2.0 * (blend - 0.5)));
    }
    
    float3 blendLinearLight(float3 base, float3 blend)
    {
        return float3(blendLinearLight(base.r, blend.r), blendLinearLight(base.g, blend.g), blendLinearLight(base.b, blend.b));
    }
    
    // Multiply
    float3 blendMultiply(float3 base, float3 blend)
    {
        return base * blend;
    }
    
    // Negation
    float3 blendNegation(float3 base, float3 blend)
    {
        return float3(1.0, 1.0, 1.0) - abs(float3(1.0, 1.0, 1.0) - base - blend);
    }
    
    // Normal
    float3 blendNormal(float3 base, float3 blend)
    {
        return blend;
    }
    
    // Phoenix
    float3 blendPhoenix(float3 base, float3 blend)
    {
        return min(base, blend) - max(base, blend) + float3(1.0, 1.0, 1.0);
    }
    
    // Pin light
    float blendPinLight(float base, float blend)
    {
        return(blend < 0.5)?blendDarken(base, (2.0 * blend)): blendLighten(base, (2.0 * (blend - 0.5)));
    }
    
    float3 blendPinLight(float3 base, float3 blend)
    {
        return float3(blendPinLight(base.r, blend.r), blendPinLight(base.g, blend.g), blendPinLight(base.b, blend.b));
    }
    
    // Screen
    float blendScreen(float base, float blend)
    {
        return 1.0 - ((1.0 - base) * (1.0 - blend));
    }
    
    float3 blendScreen(float3 base, float3 blend)
    {
        return float3(blendScreen(base.r, blend.r), blendScreen(base.g, blend.g), blendScreen(base.b, blend.b));
    }
    
    // Soft Light
    float blendSoftLight(float base, float blend)
    {
        return(blend < 0.5)?(2.0 * base * blend + base * base * (1.0 - 2.0 * blend)): (sqrt(base) * (2.0 * blend - 1.0) + 2.0 * base * (1.0 - blend));
    }
    
    float3 blendSoftLight(float3 base, float3 blend)
    {
        return float3(blendSoftLight(base.r, blend.r), blendSoftLight(base.g, blend.g), blendSoftLight(base.b, blend.b));
    }
    
    // Subtract
    float blendSubtract(float base, float blend)
    {
        return max(base - blend, 0.0);
    }
    
    float3 blendSubtract(float3 base, float3 blend)
    {
        return max(base - blend, 0.0);
    }
    
    // Difference
    float blendDifference(float base, float blend)
    {
        return abs(base - blend);
    }
    
    float3 blendDifference(float3 base, float3 blend)
    {
        return abs(base - blend);
    }
    
    // Divide
    float blendDivide(float base, float blend)
    {
        return base / max(blend, 0.0001);
    }
    
    float3 blendDivide(float3 base, float3 blend)
    {
        return base / max(blend, 0.0001);
    }
    
    float3 customBlend(float3 base, float3 blend, float blendType)
    {
        float3 ret = 0;
        switch(blendType)
        {
            case 0:
            {
                ret = blendNormal(base, blend);
                break;
            }
            case 1:
            {
                ret = blendDarken(base, blend);
                break;
            }
            case 2:
            {
                ret = blendMultiply(base, blend);
                break;
            }
            case 3:
            {
                ret = blendColorBurn(base, blend);
                break;
            }
            case 4:
            {
                ret = blendLinearBurn(base, blend);
                break;
            }
            case 5:
            {
                ret = blendLighten(base, blend);
                break;
            }
            case 6:
            {
                ret = blendScreen(base, blend);
                break;
            }
            case 7:
            {
                ret = blendColorDodge(base, blend);
                break;
            }
            case 8:
            {
                ret = blendLinearDodge(base, blend);
                break;
            }
            case 9:
            {
                ret = blendOverlay(base, blend);
                break;
            }
            case 10:
            {
                ret = blendSoftLight(base, blend);
                break;
            }
            case 11:
            {
                ret = blendHardLight(base, blend);
                break;
            }
            case 12:
            {
                ret = blendVividLight(base, blend);
                break;
            }
            case 13:
            {
                ret = blendLinearLight(base, blend);
                break;
            }
            case 14:
            {
                ret = blendPinLight(base, blend);
                break;
            }
            case 15:
            {
                ret = blendHardMix(base, blend);
                break;
            }
            case 16:
            {
                ret = blendDifference(base, blend);
                break;
            }
            case 17:
            {
                ret = blendExclusion(base, blend);
                break;
            }
            case 18:
            {
                ret = blendSubtract(base, blend);
                break;
            }
            case 19:
            {
                ret = blendDivide(base, blend);
                break;
            }
        }
        return ret;
    }
#endif
