#ifndef POI_DITHERING
    #define POI_DITHERING
    fixed _DitheringEnabled;
    
    half calcDither(half2 grabPos)
    {
        half dither = Dither8x8Bayer(fmod(grabPos.x, 8), fmod(grabPos.y, 8));
        return dither;
    }
    
    #ifndef POI_SHADOW
        void applyDithering(inout float4 finalColor)
        {
            UNITY_BRANCH
            if (_DitheringEnabled)
            {
                half dither = calcDither(poiCam.screenUV.xy);
                finalColor.a = finalColor.a - (dither * (1 - finalColor.a) * 0.15);
            }
        }
    #else
        void applyShadowDithering(inout float alpha, float2 screenUV)
        {
            UNITY_BRANCH
            if(_DitheringEnabled)
            {
                half dither = calcDither(screenUV);
                alpha = alpha - (dither * (1 - alpha) * 0.15);
            }
        }
    #endif
    
#endif