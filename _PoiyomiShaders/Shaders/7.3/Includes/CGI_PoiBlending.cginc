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
    
    float4 poiBlend(const uint sourceFactor, const  float4 sourceColor, const  uint destinationFactor, const  float4 destinationColor)
    {
        const float4 blendData[11] = {
            float4(0.0, 0.0, 0.0, 0.0),
            float4(1.0, 1.0, 1.0, 1.0),
            destinationColor,
            sourceColor,
            float4(1.0, 1.0, 1.0, 1.0) - destinationColor,
            sourceColor.aaaa,
            float4(1.0, 1.0, 1.0, 1.0) - sourceColor,
            destinationColor.aaaa,
            float4(1.0, 1.0, 1.0, 1.0) - destinationColor.aaaa,
            saturate(sourceColor.aaaa),
            float4(1.0, 1.0, 1.0, 1.0) - sourceColor.aaaa,
        };
        
        return lerp(blendData[sourceFactor] * sourceColor + blendData[destinationFactor] * destinationColor,sourceColor, sourceColor.a);
    }
    
#endif
