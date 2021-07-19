#ifndef POI_MAINTEXTURE
#define POI_MAINTEXTURE


#if defined(PROP_CLIPPINGMASK) || !defined(OPTIMIZER_ENABLED)
    POI_TEXTURE_NOSAMPLER(_ClippingMask);
#endif
#if defined(PROP_MAINFADETEXTURE) || !defined(OPTIMIZER_ENABLED)
    POI_TEXTURE_NOSAMPLER(_MainFadeTexture);
#endif

float _Inverse_Clipping;
float4 _Color;
float _MainVertexColoring;
float _MainVertexColoringLinearSpace;
float _MainUseVertexColorAlpha;
float _Saturation;
float _MainDistanceFadeMin;
float _MainDistanceFadeMax;
half _MainMinAlpha;
half _MainMaxAlpha;
float _MainHueShift;
float _MainFadeType;
#ifdef COLOR_GRADING_HDR
    #if defined(PROP_MAINCOLORADJUSTTEXTURE) || !defined(OPTIMIZER_ENABLED)
        POI_TEXTURE_NOSAMPLER(_MainColorAdjustTexture);
    #endif
    float _MainHueShiftToggle;
    float _MainHueShiftSpeed;
    float _MainHueShiftReplace;
    float _MainSaturationShift;
    float _MainBrightness;
#endif

#ifdef FINALPASS
    #if defined(PROP_DETAILTEX) || !defined(OPTIMIZER_ENABLED)
        POI_TEXTURE_NOSAMPLER(_DetailTex);
    #endif
    half _DetailTexIntensity;
    half3 _DetailTint;
    float _DetailBrightness;
#endif
//globals
float alphaMask;
half3 diffColor;

#include "CGI_PoiBackFace.cginc"

float3 wireframeEmission;

inline FragmentCommonData SpecularSetup(float4 i_tex, inout float4 albedo)
{
    half4 specGloss = 0;
    half3 specColor = specGloss.rgb;
    half smoothness = specGloss.a;
    
    half oneMinusReflectivity;
    diffColor = EnergyConservationBetweenDiffuseAndSpecular(albedo.rgb, specColor, /*out*/ oneMinusReflectivity);
    
    FragmentCommonData o = (FragmentCommonData)0;
    o.diffColor = diffColor;
    o.specColor = specColor;
    o.oneMinusReflectivity = oneMinusReflectivity;
    o.smoothness = smoothness;
    return o;
}

inline FragmentCommonData FragmentSetup(float4 i_tex, half3 i_viewDirForParallax, float3 i_posWorld, inout float4 albedo)
{
    i_tex = i_tex;
    
    FragmentCommonData o = SpecularSetup(i_tex, albedo);
    o.normalWorld = float3(0, 0, 0);
    o.eyeVec = poiCam.viewDir;
    o.posWorld = i_posWorld;
    
    // NOTE: shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
    o.diffColor = PreMultiplyAlpha(o.diffColor, 1, o.oneMinusReflectivity, /*out*/ o.alpha);
    return o;
}

void initTextureData(inout float4 albedo, inout float4 mainTexture, inout float3 backFaceEmission, inout float3 dissolveEmission, in half3 detailMask)
{
    dissolveEmission = 0;
    
    #if (defined(FORWARD_BASE_PASS) || defined(FORWARD_ADD_PASS))
        #ifdef POI_MIRROR
            applyMirrorTexture(mainTexture);
        #endif
    #endif
    #if defined(PROP_CLIPPINGMASK) || !defined(OPTIMIZER_ENABLED)
        alphaMask = POI2D_SAMPLER_PAN(_ClippingMask, _MainTex, poiMesh.uv[_ClippingMaskUV], _ClippingMaskPan).r;
    #else
        alphaMask = 1;
    #endif
    UNITY_BRANCH
    if (_Inverse_Clipping)
    {
        alphaMask = 1 - alphaMask;
    }
    mainTexture.a *= alphaMask;
    
    #ifndef POI_SHADOW
        float3 vertexColor = poiMesh.vertexColor.rgb;
        UNITY_BRANCH
        if(_MainVertexColoringLinearSpace)
        {
            vertexColor = GammaToLinearSpace(poiMesh.vertexColor.rgb);
        }

        albedo = float4(mainTexture.rgb * max(_Color.rgb, float3(0.000000001, 0.000000001, 0.000000001)) * lerp(1, vertexColor, _MainVertexColoring), mainTexture.a * max(_Color.a, 0.0000001));
        
        #if defined(POI_LIGHTING) && defined(FORWARD_BASE_PASS)
            applyShadeMaps(albedo);
        #endif
        
        albedo *= lerp(1, poiMesh.vertexColor.a, _MainUseVertexColorAlpha);
        #ifdef POI_RGBMASK
            albedo.rgb = calculateRGBMask(albedo.rgb);
        #endif
        
        albedo.a = saturate(_AlphaMod + albedo.a);
        
        wireframeEmission = 0;
        #ifdef POI_WIREFRAME
            applyWireframe(wireframeEmission, albedo);
        #endif
        float backFaceDetailIntensity = 1;
        
        float mixedHueShift = _MainHueShift;
        applyBackFaceTexture(backFaceDetailIntensity, mixedHueShift, albedo, backFaceEmission);
        
        #ifdef POI_FUR
            calculateFur();
        #endif
        
        #ifdef COLOR_GRADING_HDR
            #if defined(PROP_MAINCOLORADJUSTTEXTURE) || !defined(OPTIMIZER_ENABLED)
                float4 hueShiftAlpha = POI2D_SAMPLER_PAN(_MainColorAdjustTexture, _MainTex, poiMesh.uv[_MainColorAdjustTextureUV], _MainColorAdjustTexturePan);
            #else
                float4 hueShiftAlpha = 1;
            #endif
            
            if (_MainHueShiftReplace)
            {
                albedo.rgb = lerp(albedo.rgb, hueShift(albedo.rgb, mixedHueShift + _MainHueShiftSpeed * _Time.x), hueShiftAlpha.r);
            }
            else
            {
                albedo.rgb = hueShift(albedo.rgb, frac((mixedHueShift - (1 - hueShiftAlpha.r) + _MainHueShiftSpeed * _Time.x)));
            }
            
            albedo.rgb = lerp(albedo.rgb, dot(albedo.rgb, float3(0.3, 0.59, 0.11)), -_Saturation * hueShiftAlpha.b);
            albedo.rgb = saturate(albedo.rgb + _MainBrightness * hueShiftAlpha.g);
        #endif
        #ifdef FINALPASS
            #if defined(PROP_DETAILTEX) || !defined(OPTIMIZER_ENABLED)
                half3 detailTexture = POI2D_SAMPLER_PAN(_DetailTex, _MainTex, poiMesh.uv[_DetailTexUV], _DetailTexPan).rgb * _DetailTint.rgb;
            #else
                half3 detailTexture = 0.21763764082 * _DetailTint.rgb;
            #endif
            albedo.rgb *= LerpWhiteTo(detailTexture * _DetailBrightness * unity_ColorSpaceDouble.rgb, detailMask.r * _DetailTexIntensity * backFaceDetailIntensity);
        #endif
        albedo.rgb = saturate(albedo.rgb);
        
        #ifdef POI_HOLOGRAM
            ApplyHoloAlpha(albedo);
        #endif
        
        s = FragmentSetup(float4(poiMesh.uv[0], 1, 1), poiCam.viewDir, poiMesh.worldPos, albedo);
    #endif
    
    #ifdef DISTORT
        calculateDissolve(albedo, dissolveEmission);
    #endif
}

void distanceFade(inout float4 albedo)
{
    #if defined(PROP_MAINFADETEXTURE) || !defined(OPTIMIZER_ENABLED)
        half fadeMap = POI2D_SAMPLER_PAN(_MainFadeTexture, _MainTex, poiMesh.uv[_MainFadeTextureUV], _MainFadeTexturePan).r;
    #else
        half fadeMap = 1;
    #endif
    if (fadeMap)
    {
        float fadeDistance = _MainFadeType ? poiCam.distanceToVert : poiCam.distanceToModel;
        half fadeValue = lerp(_MainMinAlpha, _MainMaxAlpha, smoothstep(_MainDistanceFadeMin, _MainDistanceFadeMax, fadeDistance));
        albedo.a *= fadeValue;
    }
}
#endif