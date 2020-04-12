/*
USED---------------------------------------------
"_PARALLAXMAP",
"_REQUIRE_UV2",
"_SUNDISK_NONE",
"_ALPHABLEND_ON",
"_DETAIL_MULX2",
"LOD_FADE_CROSSFADE",
"_GLOSSYREFLECTIONS_OFF",
"_METALLICGLOSSMAP",
"_COLORADDSUBDIFF_ON",
"_SPECGLOSSMAP",
"_TERRAIN_NORMAL_MAP",
"_SUNDISK_SIMPLE",
"_EMISSION",
"_COLORCOLOR_ON",
"_COLOROVERLAY_ON",
"_ALPHAMODULATE_ON",
"_SUNDISK_HIGH_QUALITY",
"_MAPPING_6_FRAMES_LAYOUT",
"_NORMALMAP"
"EFFECT_BUMP",
"BLOOM",
"BLOOM_LOW",
"GRAIN",
"DEPTH_OF_FIELD",
"USER_LUT",
"CHROMATIC_ABERRATION_LOW",
"FXAA",
"BLOOM_LENS_DIRT",

UNUSED-------------------------------------------
"EFFECT_BUMP",
"_ALPHAPREMULTIPLY_ON",
"_ALPHATEST_ON",
"_FADING_ON",
"_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A",
"_SPECULARHIGHLIGHTS_OFF",
"_SPECULARHIGHLIGHTS_OFF",
"BILLBOARD_FACE_CAMERA_POS",
"EFFECT_HUE_VARIATION",
"ETC1_EXTERNAL_ALPHA",
"GEOM_TYPE_BRANCH",
"GEOM_TYPE_FROND",
"GEOM_TYPE_LEAF",
"GEOM_TYPE_MESH",
"LOD_FADE_CROSSFADE",
"PIXELSNAP_ON",
"STEREO_INSTANCING_ON",
"STEREO_MULTIVIEW_ON",
"UNITY_HDR_ON",
"UNITY_SINGLE_PASS_STEREO",
"UNITY_UI_ALPHACLIP",
"UNITY_UI_CLIP_RECT",
// Post Processing Stack V1 and V2
// This is mostly just safe keeping somewhere
"FOG_OFF",
"FOG_LINEAR",
"FOG_EXP",
"FOG_EXP2",
"ANTI_FLICKER",
"UNITY_COLORSPACE_GAMMA",
"SOURCE_GBUFFER",
"AUTO_KEY_VALUE",
"DITHERING",
"TONEMAPPING_NEUTRAL",
"TONEMAPPING_FILMIC",
"CHROMATIC_ABERRATION",
"DEPTH_OF_FIELD_COC_VIEW",
"COLOR_GRADING",
"COLOR_GRADING_LOG_VIEW",
"VIGNETTE_CLASSIC",
"VIGNETTE_MASKED",
"",
"FXAA_LOW",
"FXAA_KEEP_ALPHA",
"STEREO_INSTANCING_ENABLED",
"STEREO_DOUBLEWIDE_TARGET",
"TONEMAPPING_ACES",
"TONEMAPPING_CUSTOM",
"APPLY_FORWARD_FOG",
"DISTORT",
"VIGNETTE",
"FINALPASS",
"COLOR_GRADING_HDR_3D",
"COLOR_GRADING_HDR",
"AUTO_EXPOSURE"

TODO: _ALPHAMODULATE_ON
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
    
    #ifdef _SUNDISK_NONE
        #include "PoiRandom.cginc"
    #endif
    
    #ifdef _REQUIRE_UV2
        #include "PoiMirror.cginc"
    #endif
    #include "PoiVertexManipulations.cginc"
    
    #include "CGI_PoiV2F.cginc"
    
    #ifdef BLOOM_LOW
        #include "CGI_PoiBulge.cginc"
    #endif
    
    #include "PoiVert.cginc"
    
    
    #include "CGI_PoiDistanceDithering.cginc"
    
    #ifdef CUTOUT
        #include "PoiDithering.cginc"
    #endif
    
    #ifdef _PARALLAXMAP
        #include "PoiParallax.cginc"
    #endif
    
    #ifdef USER_LUT
        #include "CGI_PoiUVDistortion.cginc"
    #endif
    
    #include "PoiData.cginc"
    
    #ifdef WIREFRAME
        #include "CGI_PoiWireframe.cginc"
    #endif
    
    #ifdef _ALPHABLEND_ON
        #include "PoiDissolve.cginc"
    #endif
    
    #ifdef DEPTH_OF_FIELD
        #include "CGI_PoiHologram.cginc"
    #endif
    
    #ifdef FXAA
        #include "CGI_PoiRGBMask.cginc"
    #endif
    
    #ifdef BLOOM_LENS_DIRT
        #include "CGI_PoiIridescence.cginc"
    #endif
    #include "PoiMainTex.cginc"
    

    #ifdef _DETAIL_MULX2
        #include "PoiPanosphere.cginc"
    #endif
    
    #ifdef EFFECT_BUMP
        #include "CGI_PoiMSDF.cginc"
    #endif
    
    #ifdef GRAIN
        #include "CGI_PoiDepthColor.cginc"
    #endif
    
    #ifdef LOD_FADE_CROSSFADE
        #include "PoiLighting.cginc"
    #endif
    
    #ifdef _SUNDISK_HIGH_QUALITY
        #include "PoiFlipbook.cginc"
    #endif
    
    #ifdef _GLOSSYREFLECTIONS_OFF
        #include "PoiRimlighting.cginc"
    #endif
    
    #ifdef _MAPPING_6_FRAMES_LAYOUT
        #include "CGI_PoiEnvironmentalRimLighting.cginc"
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
    
    #ifdef BLOOM
        #include "PoiVideo.cginc"
    #endif
    
    #ifdef _TERRAIN_NORMAL_MAP
        #include "PoiSubsurfaceScattering.cginc"
    #endif
    
    #ifdef POI_GRABS_ASS
        #include "PoiBlending.cginc"
        #include "CGI_PoiGrab.cginc"
    #endif
    
    #ifdef _SUNDISK_SIMPLE
        #include "CGI_PoiGlitter.cginc"
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
    #include "PoiFrag.cginc"
    
#endif