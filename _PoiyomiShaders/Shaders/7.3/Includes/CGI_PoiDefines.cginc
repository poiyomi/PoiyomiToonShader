#ifndef POI_DEFINES
    #define POI_DEFINES
    
    #define DielectricSpec float4(0.04, 0.04, 0.04, 1.0 - 0.04)
    
    #ifdef _SPECGLOSSMAP // Specular
        #ifndef POI_VAR_DOTNH
            #define POI_VAR_DOTNH
        #endif
        #ifndef POI_VAR_DOTLH
            #define POI_VAR_DOTLH
        #endif
    #endif
    
    #ifdef LOD_FADE_CROSSFADE // Lighting
        #ifndef POI_VAR_DOTNL
            #define POI_VAR_DOTNL
        #endif
    #endif
    
#endif