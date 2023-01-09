#ifndef POI_PASS_OUTLINE
#define POI_PASS_OUTLINE

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"
#include "CGI_PoiMacros.cginc"
#include "CGI_PoiDefines.cginc"
#include "CGI_FunctionsArtistic.cginc"
#include "CGI_Poicludes.cginc"
#include "CGI_PoiHelpers.cginc"
#include "CGI_PoiBlending.cginc"
#include "CGI_PoiPenetration.cginc"
#include "CGI_PoiVertexManipulations.cginc"
#include "CGI_PoiOutlineVert.cginc"
#ifdef TESSELATION
	#include "CGI_PoiTessellation.cginc"
#endif
#ifdef _REQUIRE_UV2
	#include "CGI_PoiMirror.cginc"
#endif
#ifdef DISTORT
	#include "CGI_PoiDissolve.cginc"
#endif
#include "CGI_PoiLighting.cginc"
#include "CGI_PoiMainTex.cginc"
#include "CGI_PoiData.cginc"
#include "CGI_PoiDithering.cginc"
#ifdef _COLOROVERLAY_ON
	#include "CGI_PoiDebug.cginc"
#endif
#include "CGI_PoiOutlineFrag.cginc"
#endif