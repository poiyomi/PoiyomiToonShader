#ifndef POI_PASS_OUTLINE
    #define POI_PASS_OUTLINE
    
    #include "UnityCG.cginc"
    #include "Lighting.cginc"
    #include "UnityPBSLighting.cginc"
    #include "AutoLight.cginc"
    #include "Poicludes.cginc"
    #include "PoiHelpers.cginc"
    #include "PoiOutlineVert.cginc"
    #ifdef TESSELATION
        #include "CGI_PoiTessellation.cginc"
    #endif
    #ifdef _REQUIRE_UV2
        #include "PoiMirror.cginc"
    #endif
    #ifdef _ALPHABLEND_ON
        #include "PoiDissolve.cginc"
    #endif
    #include "PoiMainTex.cginc"
    #include "PoiData.cginc"
    #include "PoiDithering.cginc"
    #include "PoiLighting.cginc"
    #include "PoiOutlineFrag.cginc"
#endif