#ifndef POI_PASS_SHADOW
    #define POI_PASS_SHADOW
    
    #pragma multi_compile_shadowcaster
    #include "UnityCG.cginc"
    #include "UnityShaderVariables.cginc"
    #include "PoiShadowIncludes.cginc"
    #include "PoiHelpers.cginc"
    #include "poiMirror.cginc"
    #ifdef _SUNDISK_NONE
        #include "PoiRandom.cginc"
    #endif
    #include "PoiDissolve.cginc"
    #include "PoiShadowVert.cginc"
    #include "PoiShadowFrag.cginc"
    
#endif