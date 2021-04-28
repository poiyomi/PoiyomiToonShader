#ifndef POI_DEFINES
    #define POI_DEFINES
    
    #define DielectricSpec float4(0.04, 0.04, 0.04, 1.0 - 0.04)
    #define pi float(3.14159265359)
    
    #ifdef _SPECGLOSSMAP // Specular
        #ifndef POI_VAR_DOTNH
            #define POI_VAR_DOTNH
        #endif
        #ifndef POI_VAR_DOTLH
            #define POI_VAR_DOTLH
        #endif
    #endif
    
    #ifdef VIGNETTE_MASKED // Lighting
        #ifndef POI_VAR_DOTNL
            #define POI_VAR_DOTNL
        #endif
    #endif
    
#endif