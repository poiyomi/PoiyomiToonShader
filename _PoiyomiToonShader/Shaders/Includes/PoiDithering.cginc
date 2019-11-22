#ifndef POI_DITHERING
    #define POI_DITHERING
    
    float _DitheringEnabled;
    
    inline half Dither8x8Bayer(int x, int y)
    {
        const half dither[ 64 ] = {
            1, 49, 13, 61, 4, 52, 16, 64,
            33, 17, 45, 29, 36, 20, 48, 32,
            9, 57, 5, 53, 12, 60, 8, 56,
            41, 25, 37, 21, 44, 28, 40, 24,
            3, 51, 15, 63, 2, 50, 14, 62,
            35, 19, 47, 31, 34, 18, 46, 30,
            11, 59, 7, 55, 10, 58, 6, 54,
            43, 27, 39, 23, 42, 26, 38, 22
        };
        int r = y * 8 + x;
        return dither[r] / 64;
    }
    
    half calcDither(half2 screenPos)
    {
        half dither = Dither8x8Bayer(fmod(screenPos.x, 8), fmod(screenPos.y, 8));
        return dither;
    }
    
    void applyDithering(inout float4 finalColor)
    {
        UNITY_BRANCH
        if (_DitheringEnabled)
        {
            half dither = calcDither(poiCam.screenUV.xy);
            finalColor.a = finalColor.a - (dither * (1 - finalColor.a) * 0.15);
        }
    }
    
#endif