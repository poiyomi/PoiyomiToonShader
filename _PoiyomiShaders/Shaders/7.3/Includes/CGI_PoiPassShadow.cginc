#ifndef POI_PASS_SHADOW
    #define POI_PASS_SHADOW
    
    #pragma multi_compile_shadowcaster
    #include "UnityCG.cginc"
    #include "UnityShaderVariables.cginc"
    
    #include "CGI_PoiMacros.cginc"
    #include "CGI_PoiShadowIncludes.cginc"
    #include "CGI_PoiHelpers.cginc"
    #include "CGI_PoiMirror.cginc"
    #include "CGI_PoiSpawnInFrag.cginc"
    #ifdef WIREFRAME
        #include "CGI_PoiWireframe.cginc"
    #endif
    
    #ifdef _SUNDISK_HIGH_QUALITY
        #include "CGI_PoiFlipbook.cginc"
    #endif
    
    #ifdef _SUNDISK_NONE
        #include "CGI_PoiRandom.cginc"
    #endif
    #include "CGI_PoiDithering.cginc"
    #include "CGI_PoiDissolve.cginc"
    #include "CGI_PoiVertexManipulations.cginc"
    #include "CGI_PoiSpawnInVert.cginc"
    #include "CGI_PoiShadowVert.cginc"
    #include "CGI_PoiShadowFrag.cginc"
    
#endif