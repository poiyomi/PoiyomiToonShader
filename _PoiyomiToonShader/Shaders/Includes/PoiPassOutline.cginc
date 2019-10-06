#ifndef POI_PASS_OUTLINE
    #define POI_PASS_OUTLINE

    #include "UnityCG.cginc"
    #include "Lighting.cginc"
    #include "UnityPBSLighting.cginc"
    #include "AutoLight.cginc"
    
    #include "Poicludes.cginc"
    #include "PoiHelpers.cginc"
    #include "PoiDissolve.cginc"
    #include "PoiMainTex.cginc"
    #include "PoiData.cginc"
    #ifdef _SUNDISK_NONE
        #include "PoiRandom.cginc"
    #endif
    #include "poiMirror.cginc"
    #include "PoiLighting.cginc"
    #include "PoiOutlineVert.cginc"
    #include "PoiOutlineFrag.cginc"
#endif