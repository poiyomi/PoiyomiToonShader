#ifndef POI_PASS
    #define POI_PASS
    
    #include "Poicludes.cginc"
    #include "PoiHelpers.cginc"
    #ifndef DRAG_N_DROP
        #ifndef GOTTA_GO_FAST
            #include "PoiParallax.cginc"
        #endif
    #endif
    #include "PoiData.cginc"
    #include "PoiTextureBlending.cginc"
    #include "PoiPanosphere.cginc"
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
    
    #include "PoiEmission.cginc"
    #include "PoiDebug.cginc"
    #include "PoiVert.cginc"
    #include "PoiFrag.cginc"
    
#endif