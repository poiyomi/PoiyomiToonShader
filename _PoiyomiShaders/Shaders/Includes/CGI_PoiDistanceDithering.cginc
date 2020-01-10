#ifndef POI_DISTANCE_DITHER
    #define POI_DISTANCE_DITHER
    
    fixed _DitheringDistanceEnabled;
    float _DitheringInvisibleRange, _DitheringOpaqueRange;
    float _DitheringDistanceMinAlpha, _DitheringDistanceMaxAlpha;
    
    void applyDistanceDithering()
    {
        UNITY_BRANCH
        if (_DitheringDistanceEnabled)
        {
            if(IsInMirror())
            {
                _DitheringInvisibleRange *= 2;
                _DitheringOpaqueRange *= 2;
            }
            half dither = Dither8x8Bayer(fmod(poiCam.screenUV.x, 8), fmod(poiCam.screenUV.y, 8));
            half distance = smoothstep(_DitheringOpaqueRange, _DitheringInvisibleRange, poiCam.distanceToVert) * 1.0001;
            clip(dither - clamp(distance,_DitheringDistanceMinAlpha, _DitheringDistanceMaxAlpha));
        }
    }
    
#endif