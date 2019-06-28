#ifndef POI_PASS_OUTLINE
    #define POI_PASS_OUTLINE
    
    #pragma target 5.0
    #pragma multi_compile_fwdbase
    #pragma fragmentoption ARB_precision_hint_fastest
    #pragma multi_compile_fog
    #pragma multi_compile_instancing
    #define FORWARD_BASE_PASS
    #define OUTLINE
    #include "PoiOutlineIncludes.cginc"
    #include "PoiHelpers.cginc"
    #include "PoiFun.cginc"
    #include "PoiLighting.cginc"
    #include "PoiOutlineVert.cginc"
    #include "PoiOutlineFrag.cginc"
    
#endif