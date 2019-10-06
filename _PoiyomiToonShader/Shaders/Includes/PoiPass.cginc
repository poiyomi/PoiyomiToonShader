/*
_SPECGLOSSMAP
_PARALLAXMAP
_EMISSION
_REQUIRE_UV2
_ALPHABLEND_ON
_DETAIL_MULX2
_FLIPBOOK_BLENDING
_GLOSSYREFLECTIONS_OFF
_METALLICGLOSSMAP
_TERRAIN_NORMAL_MAP
_COLOROVERLAY_ON
_COLORADDSUBDIFF_ON
_NORMALMAP
_SUNDISK_NONE
_COLORCOLOR_ON

_ALPHAMODULATE_ON
_ALPHAPREMULTIPLY_ON
_ALPHATEST_ON
_MAPPING_6_FRAMES_LAYOUT
_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
_SPECULARHIGHLIGHTS_OFF
_SUNDISK_HIGH_QUALITY
_SUNDISK_SIMPLE
BILLBOARD_FACE_CAMERA_POS
EFFECT_BUMP
EFFECT_HUE_VARIATION
ETC1_EXTERNAL_ALPHA
GEOM_TYPE_BRANCH
GEOM_TYPE_BRANCH_DETAIL
GEOM_TYPE_FROND
GEOM_TYPE_LEAF
GEOM_TYPE_MESH
LOD_FADE_CROSSFADE
PIXELSNAP_ON
SOFTPARTICLES_ON
STEREO_INSTANCING_ON
STEREO_MULTIVIEW_ON
UNITY_HDR_ON
UNITY_SINGLE_PASS_STEREO
UNITY_UI_ALPHACLIP
UNITY_UI_CLIP_RECT
*/


#ifndef POI_PASS
    #define POI_PASS
    
    #include "UnityCG.cginc"
    #include "Lighting.cginc"
    #include "UnityPBSLighting.cginc"
    #include "AutoLight.cginc"

    #ifdef POI_META_PASS
        #include "UnityMetaPass.cginc"
    #endif

    #include "Poicludes.cginc"
    #include "PoiHelpers.cginc"
    
    #ifdef _PARALLAXMAP
        #include "PoiParallax.cginc"
    #endif
    
    #ifdef _REQUIRE_UV2
        #include "PoiMirror.cginc"
    #endif
    
    #include "PoiData.cginc"
    
    #ifdef _SUNDISK_NONE
        #include "PoiRandom.cginc"
    #endif
    
    #ifdef _ALPHABLEND_ON
        #include "PoiDissolve.cginc"
    #endif
    
    #include "PoiMainTex.cginc"
    
    #ifdef _DETAIL_MULX2
        #include "PoiPanosphere.cginc"
    #endif
    
    #ifdef _NORMALMAP
        #include "PoiLighting.cginc"
    #endif
    
    #ifdef _FLIPBOOK_BLENDING
        #include "PoiFlipbook.cginc"
    #endif
    
    #ifdef _GLOSSYREFLECTIONS_OFF
        #include "PoiRimlighting.cginc"
    #endif
    
    #ifdef _METALLICGLOSSMAP
        #include "PoiMetal.cginc"
    #endif
    
    #ifdef _COLORADDSUBDIFF_ON
        #include "PoiMatcap.cginc"
    #endif
    
    #ifdef _SPECGLOSSMAP
        #include "PoiSpecular.cginc"
    #endif
    
    #ifdef _TERRAIN_NORMAL_MAP
        #include "PoiSubsurfaceScattering.cginc"
    #endif
    
    #ifdef REFRACTIVE
        #include "PoiRefraction.cginc"
    #endif
    
    #ifdef _EMISSION
        #include "PoiEmission.cginc"
    #endif
    
    #ifdef _COLORCOLOR_ON
        #include "PoiClearCoat.cginc"
    #endif
    
    #ifdef CUTOUT
        #include "PoiAlphaToCoverage.cginc"
    #endif
    
    #ifdef _COLOROVERLAY_ON
        #include "PoiDebug.cginc"
    #endif
    
    #include "PoiVert.cginc"
    #include "PoiFrag.cginc"
    
#endif