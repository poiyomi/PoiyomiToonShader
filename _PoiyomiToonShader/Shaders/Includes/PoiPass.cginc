#ifndef POI_PASS
    #define POI_PASS
    
    #include "Poicludes.cginc"
    #include "PoiHelpers.cginc"
    #include "PoiBasics.cginc"
    #include "PoiEmission.cginc"
    #ifdef PLUS_ULTRA
        #include "PoiTextureBlending.cginc"
        #include "PoiPanosphere.cginc"
    #endif
    #ifndef DRAG_N_DROP
        #include "PoiLighting.cginc"
        #include "PoiFun.cginc"
        #ifndef GOTTA_GO_FAST
            #include "PoiFlipbook.cginc"
            #include "PoiScrollingLayers.cginc"
            #include "PoiRimlighting.cginc"
            #include "PoiMetal.cginc"
            #include "PoiMatcap.cginc"
            #include "PoiSpecular.cginc"
            #include "PoiSubsurfaceScattering.cginc"
        #endif
    #else
        #include "PoiDragNDropLighting.cginc"
    #endif
    
    #include "PoiVert.cginc"
    #include "PoiFrag.cginc"
    
#endif