#ifndef POI_PASS_SHADOW
    #define POI_PASS_SHADOW
    
    #pragma multi_compile_shadowcaster
    #include "UnityCG.cginc"
    #include "UnityShaderVariables.cginc"
    
    #include "PoiShadowIncludes.cginc"
    #include "PoiHelpers.cginc"
    #include "poiMirror.cginc"
    
    #ifdef WIREFRAME
        #include "CGI_PoiWireframe.cginc"
    #endif

    #ifdef _SUNDISK_HIGH_QUALITY
        #include "PoiFlipbook.cginc"
    #endif
    
    #ifdef _SUNDISK_NONE
        #include "PoiRandom.cginc"
    #endif
    #include "PoiDithering.cginc"
    #include "PoiDissolve.cginc"
    #include "PoiVertexManipulations.cginc"
    #include "PoiShadowVert.cginc"
    #include "PoiShadowFrag.cginc"
    
#endif