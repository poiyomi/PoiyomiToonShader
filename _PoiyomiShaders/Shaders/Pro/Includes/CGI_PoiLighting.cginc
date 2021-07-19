
#ifndef POI_LIGHTING
#define POI_LIGHTING

float _LightingRampType;
float _LightingIgnoreAmbientColor;
float _UseShadowTexture;
float _LightingEnableAO;
float _LightingDetailShadowsEnabled;

float _LightingOnlyUnityShadows;
float _LightingMode;
float _ForceLightDirection;
float _ShadowStrength;
float _OutlineShadowStrength;
float _ShadowOffset;
float3 _LightDirection;
float _ForceShadowStrength;
float _CastedShadowSmoothing;
float _AttenuationMultiplier;
float _EnableLighting;
float _LightingControlledUseLightColor;
fixed _LightingStandardSmoothness;
fixed _LightingStandardControlsToon;
fixed _LightingMinLightBrightness;
float _LightingUseShadowRamp;
float _LightingMinShadowBrightnessRatio;
fixed _LightingMonochromatic;

fixed _LightingGradientStart;
fixed _LightingGradientEnd;
float3 _LightingShadowColor;
float _AOStrength;
fixed _LightingDetailStrength;
fixed _LightingAdditiveDetailStrength;
fixed _LightingNoIndirectMultiplier;
fixed _LightingNoIndirectThreshold;
float _LightingUncapped;

float _LightingDirectColorMode;
float _LightingIndirectColorMode;
float _LightingAdditiveType;
fixed _LightingAdditiveGradientStart;
fixed _LightingAdditiveGradientEnd;
fixed _LightingAdditivePassthrough;
float _LightingDirectAdjustment;
float _LightingIndirect;
// HSL JUNK
float _LightingEnableHSL;
float _LightingShadowHue;
float _LightingShadowSaturation;
float _LightingShadowLightness;
float _LightingHSLIntensity;
// UTS Style Shade Mapping
float4 _1st_ShadeColor;
float _Use_BaseAs1st;
float4 _2nd_ShadeColor;
float _Use_1stAs2nd;
float _BaseColor_Step;
float _BaseShade_Feather;
float _ShadeColor_Step;
float _1st2nd_Shades_Feather;
float _Use_1stShadeMapAlpha_As_ShadowMask;
float _1stShadeMapMask_Inverse;
float _Tweak_1stShadingGradeMapLevel;
float _Use_2ndShadeMapAlpha_As_ShadowMask;
float _2ndShadeMapMask_Inverse;
float _Tweak_2ndShadingGradeMapLevel;
// Skin
float _SkinScatteringProperties;
float _SssWeight;
float _SssMaskCutoff ;
float _SssBias;
float _SssScale;
float _SssBumpBlur;
float4 _SssTransmissionAbsorption;
float4 _SssColorBleedAoWeights;
/*
UNITY_DECLARE_TEX2D_NOSAMPLER(_ToonRamp3);
half _LightingShadowStrength3;
half _ShadowOffset3;
*/

half4 shadowStrength;
sampler2D _SkinLUT;
UNITY_DECLARE_TEX2D(_ToonRamp);
POI_TEXTURE_NOSAMPLER(_1st_ShadeMap);
POI_TEXTURE_NOSAMPLER(_2nd_ShadeMap);
POI_TEXTURE_NOSAMPLER(_LightingDetailShadows);
POI_TEXTURE_NOSAMPLER(_LightingAOTex);
POI_TEXTURE_NOSAMPLER(_LightingShadowMask);

float3 directLighting;
float3 indirectLighting;
/*
*  DJLs code starts here
*/
float _LightingWrappedWrap;
float _LightingWrappedNormalization;

// Green’s model with adjustable energy
// http://blog.stevemcauley.com/2011/12/03/energy-conserving-wrapped-diffuse/
// Modified for adjustable conservation ratio and over-wrap to directionless
float RTWrapFunc(in float dt, in float w, in float norm)
{
    float cw = saturate(w);
    
    float o = (dt + cw) / ((1.0 + cw) * (1.0 + cw * norm));
    float flt = 1.0 - 0.85 * norm;
    if (w > 1.0)
    {
        o = lerp(o, flt, w - 1.0);
    }
    return o;
}
float3 GreenWrapSH(float fA) // Greens unoptimized and non-normalized

{
    float fAs = saturate(fA);
    float4 t = float4(fA + 1, fAs - 1, fA - 2, fAs + 1); // DJL edit: allow wrapping to L0-only at w=2
    return float3(t.x, -t.z * t.x / 3, 0.25 * t.y * t.y * t.w);
}
float3 GreenWrapSHOpt(float fW) // optimised and normalized https://blog.selfshadow.com/2012/01/07/righting-wrap-part-2/

{
    const float4 t0 = float4(0.0, 1.0 / 4.0, -1.0 / 3.0, -1.0 / 2.0);
    const float4 t1 = float4(1.0, 2.0 / 3.0, 1.0 / 4.0, 0.0);
    float3 fWs = float3(fW, fW, saturate(fW)); // DJL edit: allow wrapping to L0-only at w=2
    
    float3 r;
    r.xyz = t0.xxy * fWs + t0.xzw;
    r.xyz = r.xyz * fWs + t1.xyz;
    return r;
}
float3 ShadeSH9_wrapped(float3 normal, float wrap)
{
    float3 x0, x1, x2;
    float3 conv = lerp(GreenWrapSH(wrap), GreenWrapSHOpt(wrap), _LightingWrappedNormalization); // Should try optimizing this...
    conv *= float3(1, 1.5, 4); // Undo pre-applied cosine convolution by using the inverse
    
    // Constant (L0)
    x0 = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
    // Remove pre-applied constant part from L(2,0) to apply correct convolution
    float3 L2_0 = float3(unity_SHBr.z, unity_SHBg.z, unity_SHBb.z) / - 3.0;
    x0 -= L2_0;
    
    // Linear (L1) polynomial terms
    x1.r = dot(unity_SHAr.xyz, normal);
    x1.g = dot(unity_SHAg.xyz, normal);
    x1.b = dot(unity_SHAb.xyz, normal);
    
    // 4 of the quadratic (L2) polynomials
    float4 vB = normal.xyzz * normal.yzzx;
    x2.r = dot(unity_SHBr, vB);
    x2.g = dot(unity_SHBg, vB);
    x2.b = dot(unity_SHBb, vB);
    
    // Final (5th) quadratic (L2) polynomial
    float vC = normal.x * normal.x - normal.y * normal.y;
    x2 += unity_SHC.rgb * vC;
    // Move back the constant part of L(2,0)
    x2 += L2_0;
    
    return x0 * conv.x + x1 * conv.y + x2 * conv.z;
}

/*
* MIT License
*
* Copyright (c) 2018 s-ilent
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

/*
* Silent's code starts here
*/

float shEvaluateDiffuseL1Geomerics_local(float L0, float3 L1, float3 n)
{
    // average energy
    float R0 = max(0, L0);
    
    // avg direction of incoming light
    float3 R1 = 0.5f * L1;
    
    // directional brightness
    float lenR1 = length(R1);
    
    // linear angle between normal and direction 0-1
    //float q = 0.5f * (1.0f + dot(R1 / lenR1, n));
    //float q = dot(R1 / lenR1, n) * 0.5 + 0.5;
    float q = dot(normalize(R1), n) * 0.5 + 0.5;
    q = saturate(q); // Thanks to ScruffyRuffles for the bug identity.
    
    // power for q
    // lerps from 1 (linear) to 3 (cubic) based on directionality
    float p = 1.0f + 2.0f * lenR1 / R0;
    
    // dynamic range constant
    // should vary between 4 (highly directional) and 0 (ambient)
    float a = (1.0f - lenR1 / R0) / (1.0f + lenR1 / R0);
    
    return R0 * (a + (1.0f - a) * (p + 1.0f) * pow(q, p));
}

half3 BetterSH9(half4 normal)
{
    float3 indirect;
    float3 L0 = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w) + float3(unity_SHBr.z, unity_SHBg.z, unity_SHBb.z) / 3.0;
    indirect.r = shEvaluateDiffuseL1Geomerics_local(L0.r, unity_SHAr.xyz, normal.xyz);
    indirect.g = shEvaluateDiffuseL1Geomerics_local(L0.g, unity_SHAg.xyz, normal.xyz);
    indirect.b = shEvaluateDiffuseL1Geomerics_local(L0.b, unity_SHAb.xyz, normal.xyz);
    indirect = max(0, indirect);
    indirect += SHEvalLinearL2(normal);
    return indirect;
}

float3 BetterSH9(float3 normal)
{
    return BetterSH9(float4(normal, 1));
}

/*
* Standard stuff starts here
*/
UnityLight CreateLight(float3 normal, fixed detailShadowMap)
{
    UnityLight light;
    light.dir = poiLight.direction;
    light.color = saturate(_LightColor0.rgb * lerp(1, poiLight.attenuation, _AttenuationMultiplier) * detailShadowMap);
    light.ndotl = DotClamped(normal, poiLight.direction);
    return light;
}

float FadeShadows(float attenuation)
{
    #if HANDLE_SHADOWS_BLENDING_IN_GI || ADDITIONAL_MASKED_DIRECTIONAL_SHADOWS
        // UNITY_LIGHT_ATTENUATION doesn't fade shadows for us.
        
        #if ADDITIONAL_MASKED_DIRECTIONAL_SHADOWS
            attenuation = lerp(1, poiLight.attenuation, _AttenuationMultiplier);
        #endif
        
        float viewZ = dot(_WorldSpaceCameraPos - poiMesh.worldPos, UNITY_MATRIX_V[2].xyz);
        float shadowFadeDistance = UnityComputeShadowFadeDistance(poiMesh.worldPos, viewZ);
        float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
        float bakedAttenuation = UnitySampleBakedOcclusion(poiMesh.lightmapUV.xy, poiMesh.worldPos);
        attenuation = UnityMixRealtimeAndBakedShadows(
            attenuation, bakedAttenuation, shadowFade
        );
    #endif
    
    return attenuation;
}

void ApplySubtractiveLighting(inout UnityIndirect indirectLight)
{
    #if SUBTRACTIVE_LIGHTING
        poiLight.attenuation = FadeShadows(lerp(1, poiLight.attenuation, _AttenuationMultiplier));
        
        float ndotl = saturate(dot(i.normal, _WorldSpaceLightPos0.xyz));
        float3 shadowedLightEstimate = ndotl * (1 - poiLight.attenuation) * _LightColor0.rgb;
        float3 subtractedLight = indirectLight.diffuse - shadowedLightEstimate;
        subtractedLight = max(subtractedLight, unity_ShadowColor.rgb);
        subtractedLight = lerp(subtractedLight, indirectLight.diffuse, _LightShadowData.x);
        indirectLight.diffuse = min(subtractedLight, indirectLight.diffuse);
    #endif
}

float3 weightedBlend(float3 layer1, float3 layer2, float2 weights)
{
    return(weights.x * layer1 + weights.y * layer2) / (weights.x + weights.y);
}

UnityIndirect CreateIndirectLight(float3 normal)
{
    UnityIndirect indirectLight;
    indirectLight.diffuse = 0;
    indirectLight.specular = 0;
    
    #if defined(FORWARD_BASE_PASS)
        #if defined(LIGHTMAP_ON)
            indirectLight.diffuse = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, poiMesh.lightmapUV.xy));
            
            #if defined(DIRLIGHTMAP_COMBINED)
                float4 lightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(
                    unity_LightmapInd, unity_Lightmap, poiMesh.lightmapUV.xy
                );
                indirectLight.diffuse = DecodeDirectionalLightmap(
                    indirectLight.diffuse, lightmapDirection, normal
                );
            #endif
            ApplySubtractiveLighting(indirectLight);
        #endif
        
        #if defined(DYNAMICLIGHTMAP_ON)
            float3 dynamicLightDiffuse = DecodeRealtimeLightmap(
                UNITY_SAMPLE_TEX2D(unity_DynamicLightmap, poiMesh.lightmapUV.zw)
            );
            
            #if defined(DIRLIGHTMAP_COMBINED)
                float4 dynamicLightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(
                    unity_DynamicDirectionality, unity_DynamicLightmap,
                    poiMesh.lightmapUV.zw
                );
                indirectLight.diffuse += DecodeDirectionalLightmap(
                    dynamicLightDiffuse, dynamicLightmapDirection, normal
                );
            #else
                indirectLight.diffuse += dynamicLightDiffuse;
            #endif
        #endif
        
        #if !defined(LIGHTMAP_ON) && !defined(DYNAMICLIGHTMAP_ON)
            #if UNITY_LIGHT_PROBE_PROXY_VOLUME
                if (unity_ProbeVolumeParams.x == 1)
                {
                    indirectLight.diffuse = SHEvalLinearL0L1_SampleProbeVolume(
                        float4(normal, 1), poiMesh.worldPos
                    );
                    indirectLight.diffuse = max(0, indirectLight.diffuse);
                    #if defined(UNITY_COLORSPACE_GAMMA)
                        indirectLight.diffuse = LinearToGammaSpace(indirectLight.diffuse);
                    #endif
                }
                else
                {
                    indirectLight.diffuse += max(0, ShadeSH9(float4(normal, 1)));
                }
            #else
                indirectLight.diffuse += max(0, ShadeSH9(float4(normal, 1)));
            #endif
        #endif
        
        float3 reflectionDir = reflect(-poiCam.viewDir, normal);
        Unity_GlossyEnvironmentData envData;
        envData.roughness = 1 - _LightingStandardSmoothness;
        envData.reflUVW = BoxProjection(
            reflectionDir, poiMesh.worldPos.xyz,
            unity_SpecCube0_ProbePosition,
            unity_SpecCube0_BoxMin.xyz, unity_SpecCube0_BoxMax.xyz
        );
        float3 probe0 = Unity_GlossyEnvironment(
            UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData
        );
        envData.reflUVW = BoxProjection(
            reflectionDir, poiMesh.worldPos.xyz,
            unity_SpecCube1_ProbePosition,
            unity_SpecCube1_BoxMin.xyz, unity_SpecCube1_BoxMax.xyz
        );
        #if UNITY_SPECCUBE_BLENDING
            float interpolator = unity_SpecCube0_BoxMin.w;
            UNITY_BRANCH
            if (interpolator < 0.99999)
            {
                float3 probe1 = Unity_GlossyEnvironment(
                    UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1, unity_SpecCube0),
                    unity_SpecCube0_HDR, envData
                );
                indirectLight.specular = lerp(probe1, probe0, interpolator);
            }
            else
            {
                indirectLight.specular = probe0;
            }
        #else
            indirectLight.specular = probe0;
        #endif
        float occlusion = 1;
        UNITY_BRANCH
        if (_LightingEnableAO)
        {
            occlusion = lerp(1, POI2D_SAMPLER_PAN(_LightingAOTex, _MainTex, poiMesh.uv[_LightingAOTexUV], _LightingAOTexPan).r, _AOStrength);
        }
        
        indirectLight.diffuse *= occlusion;
        indirectLight.diffuse = max(indirectLight.diffuse, _LightingMinLightBrightness);
        indirectLight.specular *= occlusion;
    #endif
    
    return indirectLight;
}

/*
* Poiyomi's cool as heck code starts here :smug:
*/

half PoiDiffuse(half NdotV, half NdotL, half LdotH)
{
    half fd90 = 0.5 + 2 * LdotH * LdotH * SmoothnessToPerceptualRoughness(.5);
    // Two schlick fresnel term
    half lightScatter = (1 + (fd90 - 1) * Pow5(1 - NdotL));
    half viewScatter = (1 + (fd90 - 1) * Pow5(1 - NdotV));
    
    return lightScatter * viewScatter;
}

float3 ShadeSH9Indirect()
{
    return ShadeSH9(half4(0.0, -1.0, 0.0, 1.0));
}

float3 ShadeSH9Direct()
{
    return ShadeSH9(half4(0.0, 1.0, 0.0, 1.0));
}

float3 ShadeSH9Normal(float3 normalDirection)
{
    return ShadeSH9(half4(normalDirection, 1.0));
}

half3 GetSHLength()
{
    half3 x, x1;
    x.r = length(unity_SHAr);
    x.g = length(unity_SHAg);
    x.b = length(unity_SHAb);
    x1.r = length(unity_SHBr);
    x1.g = length(unity_SHBg);
    x1.b = length(unity_SHBb);
    return x + x1;
}
half3 GetSHDirectionL1()
{
    //float3 grayscale = float3(.3, .59, .11);
    float3 grayscale = float3(.33333, .33333, .33333);
    half3 r = Unity_SafeNormalize(half3(unity_SHAr.x, unity_SHAr.y, unity_SHAr.z));
    half3 g = Unity_SafeNormalize(half3(unity_SHAg.x, unity_SHAg.y, unity_SHAg.z));
    half3 b = Unity_SafeNormalize(half3(unity_SHAb.x, unity_SHAb.y, unity_SHAb.z));
    return Unity_SafeNormalize(grayscale.r * r + grayscale.g * g + grayscale.b * b);
}
float3 GetSHDirectionL1_()
{
    // For efficiency, we only get the direction from L1.
    // Because getting it from L2 would be too hard!
    return Unity_SafeNormalize((unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz));
}
// Returns the value from SH in the lighting direction with the
// brightest intensity.
half3 GetSHMaxL1()
{
    float3 maxDirection = GetSHDirectionL1();
    return ShadeSH9_wrapped(maxDirection, 0);
}


float3 calculateRealisticLighting(float4 colorToLight, fixed detailShadowMap)
{
    return UNITY_BRDF_PBS(1, 0, 0, _LightingStandardSmoothness, poiMesh.normals[1], poiCam.viewDir, CreateLight(poiMesh.normals[1], detailShadowMap), CreateIndirectLight(poiMesh.normals[1])).xyz;
}

void calculateBasePassLightMaps()
{
    #if defined(FORWARD_BASE_PASS) || defined(POI_META_PASS)
        float AOMap = 1;
        float AOStrength = 0;
        float3 lightColor = poiLight.color;
        /*
        * Generate Basic Light Maps
        */
        
        bool lightExists = false;
        if (any(_LightColor0.rgb >= 0.002))
        {
            lightExists = true;
        }
        #ifndef OUTLINE
            UNITY_BRANCH
            if (_LightingEnableAO)
            {
                AOMap = POI2D_SAMPLER_PAN(_LightingAOTex, _MainTex, poiMesh.uv[_LightingAOTexUV], _LightingAOTexPan).r;
                AOStrength = _AOStrength;
                poiLight.occlusion = lerp(1, AOMap, AOStrength);
            }
            #ifdef FORWARD_BASE_PASS
                //poiLight.color = saturate(_LightColor0.rgb) + saturate(ShadeSH9(normalize(unity_SHAr + unity_SHAg + unity_SHAb)));
                if (lightExists)
                {
                    lightColor = _LightColor0.rgb + BetterSH9(float4(0, 0, 0, 1));
                }
                else
                {
                    lightColor = BetterSH9(normalize(unity_SHAr + unity_SHAg + unity_SHAb));
                }
                
                //lightColor = magic * magiratio + normalLight * normaRatio;
                //lightColor = magic + normalLight;
            #endif
        #endif
        
        float3 grayscale_vector = float3(.33333, .33333, .33333);
        float3 ShadeSH9Plus = GetSHLength();
        float3 ShadeSH9Minus = float3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w) + float3(unity_SHBr.z, unity_SHBg.z, unity_SHBb.z) / 3.0;
        
        shadowStrength = 1;
        #ifndef OUTLINE
            shadowStrength = POI2D_SAMPLER_PAN(_LightingShadowMask, _MainTex, poiMesh.uv[_LightingShadowMaskUV], _LightingShadowMaskPan) * _ShadowStrength;
        #else
            shadowStrength = _OutlineShadowStrength;
        #endif
        
        float bw_lightColor = dot(lightColor, grayscale_vector);
        float bw_directLighting = (((poiLight.nDotL * 0.5 + 0.5) * bw_lightColor * lerp(1, poiLight.attenuation, _AttenuationMultiplier)) + dot(ShadeSH9Normal(poiMesh.normals[1]), grayscale_vector));
        float bw_bottomIndirectLighting = dot(ShadeSH9Minus, grayscale_vector);
        float bw_topIndirectLighting = dot(ShadeSH9Plus, grayscale_vector);
        float lightDifference = ((bw_topIndirectLighting + bw_lightColor) - bw_bottomIndirectLighting);
        
        fixed detailShadow = 1;
        UNITY_BRANCH
        if (_LightingDetailShadowsEnabled)
        {
            detailShadow = lerp(1, POI2D_SAMPLER_PAN(_LightingDetailShadows, _MainTex, poiMesh.uv[_LightingDetailShadowsUV], _LightingDetailShadowsPan), _LightingDetailStrength).r;
        }
        UNITY_BRANCH
        if (_LightingOnlyUnityShadows)
        {
            poiLight.lightMap = poiLight.attenuation;
        }
        else
        {
            poiLight.lightMap = smoothstep(0, lightDifference, bw_directLighting - bw_bottomIndirectLighting);
        }
        poiLight.lightMap *= detailShadow;
        
        /*
        * Decide on light colors
        */
        
        indirectLighting = 0;
        directLighting = 0;
        
        
        
        UNITY_BRANCH
        if (_LightingIndirectColorMode == 1)
        {
            indirectLighting = BetterSH9(float4(poiMesh.normals[1], 1));
        }
        else
        {
            indirectLighting = ShadeSH9Minus;
        }
        
        poiLight.directLighting = lightColor;
        poiLight.indirectLighting = indirectLighting;
        

        UNITY_BRANCH
        if (_LightingDirectColorMode == 0)
        {
            float3 magic = max(BetterSH9(normalize(unity_SHAr + unity_SHAg + unity_SHAb)), 0);
            float3 normalLight = _LightColor0.rgb + BetterSH9(float4(0, 0, 0, 1));
            
            float magiLumi = calculateluminance(magic);
            float normaLumi = calculateluminance(normalLight);
            float maginormalumi = magiLumi + normaLumi;
            
            float magiratio = magiLumi / maginormalumi;
            float normaRatio = normaLumi / maginormalumi;
            
            float target = calculateluminance(magic * magiratio + normalLight * normaRatio);
            float3 properLightColor = magic * poiLight.occlusion + normalLight;
            float properLuminance = calculateluminance(magic + normalLight);
            directLighting = properLightColor * max(0.0001, (target / properLuminance));
        }
        else
        {
            if (lightExists)
            {
                directLighting = _LightColor0.rgb + BetterSH9(float4(0, 0, 0, 1)) * poiLight.occlusion;
            }
            else
            {
                directLighting = max(BetterSH9(normalize(unity_SHAr + unity_SHAg + unity_SHAb)), 0);
            }
        }
        
        UNITY_BRANCH
        if (!_LightingUncapped)
        {
            float directluminance = calculateluminance(directLighting);
            float indirectluminance = calculateluminance(indirectLighting);
            directLighting = min(directLighting, directLighting / max(0.0001, (directluminance / 1)));
            indirectLighting = min(indirectLighting, indirectLighting / max(0.0001, (indirectluminance / 1)));
        }
        
        directLighting = lerp(directLighting, dot(directLighting, float3(0.299, 0.587, 0.114)), _LightingMonochromatic);
        indirectLighting = lerp(indirectLighting, dot(indirectLighting, float3(0.299, 0.587, 0.114)), _LightingMonochromatic);
        
        
        if (max(max(indirectLighting.x, indirectLighting.y), indirectLighting.z) <= _LightingNoIndirectThreshold && max(max(directLighting.x, directLighting.y), directLighting.z) >= 0)
        {
            indirectLighting = directLighting * _LightingNoIndirectMultiplier;
        }

        
        UNITY_BRANCH
        if (_LightingMinShadowBrightnessRatio)
        {
            float directluminance = clamp(directLighting.r * 0.299 + directLighting.g * 0.587 + directLighting.b * 0.114, 0, 1);
            if (directluminance > 0)
            {
                indirectLighting = max(0.001, indirectLighting);
            }
            float indirectluminance = clamp(indirectLighting.r * 0.299 + indirectLighting.g * 0.587 + indirectLighting.b * 0.114, 0, 1);
            float targetluminance = directluminance * _LightingMinShadowBrightnessRatio;
            if (indirectluminance < targetluminance)
            {
                indirectLighting = indirectLighting / max(0.0001, indirectluminance / targetluminance);
            }
        }

        poiLight.rampedLightMap = 1 - smoothstep(0, .5, 1 - poiLight.lightMap);
        poiLight.finalLighting = directLighting;
        
        indirectLighting = max(indirectLighting,0);
        directLighting = max(directLighting,0);

        /*
        * Create Gradiant Maps
        */
        switch(_LightingRampType)
        {
            case 0: // Ramp Texture

            {
                poiLight.rampedLightMap = lerp(1, UNITY_SAMPLE_TEX2D(_ToonRamp, poiLight.lightMap + _ShadowOffset).rgb, shadowStrength.r);
                UNITY_BRANCH
                if (_LightingIgnoreAmbientColor)
                {
                    poiLight.finalLighting = lerp(poiLight.rampedLightMap * directLighting * poiLight.occlusion, directLighting, poiLight.rampedLightMap);
                }
                else
                {
                    poiLight.finalLighting = lerp(indirectLighting * poiLight.occlusion, directLighting, poiLight.rampedLightMap);

                }
            }
            break;
            case 1: // Math Gradient

            {
                poiLight.rampedLightMap = saturate(1 - smoothstep(_LightingGradientStart - .000001, _LightingGradientEnd, 1 - poiLight.lightMap));
                float3 shadowColor = _LightingShadowColor;
                UNITY_BRANCH
                if (_UseShadowTexture)
                {
                    shadowColor = 1;
                }
                UNITY_BRANCH
                if (_LightingIgnoreAmbientColor)
                {
                    poiLight.finalLighting = lerp((directLighting * shadowColor * poiLight.occlusion), (directLighting), saturate(poiLight.rampedLightMap + 1 - _ShadowStrength));
                }
                else
                {
                    poiLight.finalLighting = lerp((indirectLighting * shadowColor * poiLight.occlusion), (directLighting), saturate(poiLight.rampedLightMap + 1 - _ShadowStrength));
                }
            }
            break;
            case 2:
            {
                poiLight.rampedLightMap = saturate(1 - smoothstep(0, .5, 1 - poiLight.lightMap));
                poiLight.finalLighting = directLighting;
            }
            break;
        }
        
        // DJL stuff
        if (_LightingMode == 2) // Wrapped

        {
            float wrap = _LightingWrappedWrap;
            
            float3 directcolor = (_LightColor0.rgb) * saturate(RTWrapFunc(poiLight.nDotL, wrap, _LightingWrappedNormalization));
            float directatten = lerp(1, poiLight.attenuation, _AttenuationMultiplier);
            
            uint normalsindex = _LightingIndirectColorMode > 0 ? 1: 0;
            // if (_LightingIndirectColorMode == 1)
            // {
                //     surfnormals = poiMesh.normals[1];
                // }
                // else
                // {
                    //     surfnormals = poiMesh.normals[0];
                    // }
                    float3 envlight = ShadeSH9_wrapped(poiMesh.normals[normalsindex], wrap);
                    envlight *= poiLight.occlusion;
                    
                    poiLight.directLighting = directcolor * detailShadow * directatten;
                    poiLight.indirectLighting = envlight;
                    

                    float3 ShadeSH9Plus_2 = GetSHMaxL1();
                    float bw_topDirectLighting_2 = dot(_LightColor0.rgb, grayscale_vector);
                    float bw_directLighting = dot(poiLight.directLighting, grayscale_vector);
                    float bw_indirectLighting = dot(poiLight.indirectLighting, grayscale_vector);
                    float bw_topIndirectLighting = dot(ShadeSH9Plus_2, grayscale_vector);
                    
                    //poiLight.lightMap = saturate(dot(poiLight.indirectLighting + poiLight.directLighting, grayscale_vector));
                    poiLight.lightMap = smoothstep(0, bw_topIndirectLighting + bw_topDirectLighting_2, bw_indirectLighting + bw_directLighting);
                    
                    poiLight.rampedLightMap = 1;
                    UNITY_BRANCH
                    if (_LightingRampType == 0) // Ramp Texture

                    {
                        poiLight.rampedLightMap = lerp(1, UNITY_SAMPLE_TEX2D(_ToonRamp, poiLight.lightMap + _ShadowOffset).rgb, shadowStrength.r);
                    }
                    else if (_LightingRampType == 1) // Math Gradient

                    {
                        poiLight.rampedLightMap = lerp(_LightingShadowColor * lerp(poiLight.indirectLighting, 1, _LightingIgnoreAmbientColor), float3(1, 1, 1), saturate(1 - smoothstep(_LightingGradientStart - .000001, _LightingGradientEnd, 1 - poiLight.lightMap)));
                        poiLight.rampedLightMap = lerp(float3(1, 1, 1), poiLight.rampedLightMap, shadowStrength.r);
                    }
                    
                    poiLight.finalLighting = (poiLight.indirectLighting + poiLight.directLighting) * saturate(poiLight.rampedLightMap + 1 - _ShadowStrength);
                }
                
                if (!_LightingUncapped)
                {
                    poiLight.finalLighting = saturate(poiLight.finalLighting);
                }
                //poiLight.finalLighting *= .8;
            #endif
        }
        
        /*
        void applyShadowTexture(inout float4 albedo)
        {
            UNITY_BRANCH
            if (_UseShadowTexture && _LightingRampType == 1)
            {
                albedo.rgb = lerp(albedo.rgb, POI2D_SAMPLER_PAN(_LightingShadowTexture, _MainTex, poiMesh.uv[_LightingShadowTextureUV], _LightingShadowTexturePan) * _LightingShadowColor, (1 - poiLight.rampedLightMap) * shadowStrength);
            }
        }
        */
        
        float3 calculateNonImportantLighting(float attenuation, float attenuationDotNL, float3 albedo, float3 lightColor, half dotNL, half correctedDotNL)
        {
            fixed detailShadow = 1;
            UNITY_BRANCH
            if (_LightingDetailShadowsEnabled)
            {
                detailShadow = lerp(1, POI2D_SAMPLER_PAN(_LightingDetailShadows, _MainTex, poiMesh.uv[_LightingDetailShadowsUV], _LightingDetailShadowsPan), _LightingAdditiveDetailStrength).r;
            }
            UNITY_BRANCH
            if (_LightingAdditiveType == 0)
            {
                return lightColor * attenuationDotNL * detailShadow; // Realistic
            }
            else if (_LightingAdditiveType == 1) // Toon

            {
                return lerp(lightColor * attenuation, lightColor * _LightingAdditivePassthrough * attenuation, smoothstep(_LightingAdditiveGradientStart, _LightingAdditiveGradientEnd, dotNL)) * detailShadow;
            }
            else //if(_LightingAdditiveType == 2) // Wrapped

            {
                float uv = saturate(RTWrapFunc(-dotNL, _LightingWrappedWrap, _LightingWrappedNormalization)) * detailShadow;
                
                poiLight.rampedLightMap = 1;
                if (_LightingRampType == 1) // Math Gradient
                poiLight.rampedLightMap = lerp(_LightingShadowColor, float3(1, 1, 1), saturate(1 - smoothstep(_LightingGradientStart - .000001, _LightingGradientEnd, 1 - uv)));
                // TODO: ramp texture or full shade/tint map for atlasing
                
                return lightColor * poiLight.rampedLightMap * saturate(attenuation * uv);
            }
        }
        
        void applyShadeMaps(inout float4 albedo)
        {
            UNITY_BRANCH
            if (_LightingRampType == 2)
            {
                float3 baseColor = albedo.rgb;
                
                float MainColorFeatherStep = _BaseColor_Step - _BaseShade_Feather;
                float firstColorFeatherStep = _ShadeColor_Step - _1st2nd_Shades_Feather;
                
                #if defined(PROP_1ST_SHADEMAP) || !defined(OPTIMIZER_ENABLED)
                    float4 firstShadeMap = POI2D_SAMPLER_PAN(_1st_ShadeMap, _MainTex, poiMesh.uv[_1st_ShadeMapUV], _1st_ShadeMapPan);
                #else
                    float4 firstShadeMap = float4(1, 1, 1, 1);
                #endif
                firstShadeMap = lerp(firstShadeMap, albedo, _Use_BaseAs1st);
                
                #if defined(PROP_2ND_SHADEMAP) || !defined(OPTIMIZER_ENABLED)
                    float4 secondShadeMap = POI2D_SAMPLER_PAN(_2nd_ShadeMap, _MainTex, poiMesh.uv[_2nd_ShadeMapUV], _2nd_ShadeMapPan);
                #else
                    float4 secondShadeMap = float4(1, 1, 1, 1);
                #endif
                secondShadeMap = lerp(secondShadeMap, firstShadeMap, _Use_1stAs2nd);
                
                firstShadeMap.rgb *= _1st_ShadeColor.rgb; //* lighColor
                secondShadeMap.rgb *= _2nd_ShadeColor.rgb; //* LightColor;
                
                float shadowMask = 1;
                shadowMask *= _Use_1stShadeMapAlpha_As_ShadowMask ?(_1stShadeMapMask_Inverse ?(1.0 - firstShadeMap.a): firstShadeMap.a): 1;
                shadowMask *= _Use_2ndShadeMapAlpha_As_ShadowMask ?(_2ndShadeMapMask_Inverse ?(1.0 - secondShadeMap.a): secondShadeMap.a): 1;
                
                float mainShadowMask = saturate(1 - ((poiLight.lightMap) - MainColorFeatherStep) / (_BaseColor_Step - MainColorFeatherStep) * (shadowMask));
                float firstSecondShadowMask = saturate(1 - ((poiLight.lightMap) - firstColorFeatherStep) / (_ShadeColor_Step - firstColorFeatherStep) * (shadowMask));
                
                #if defined(PROP_LIGHTINGSHADOWMASK) || !defined(OPTIMIZER_ENABLED)
                    float removeShadow = POI2D_SAMPLER_PAN(_LightingShadowMask, _MainTex, poiMesh.uv[_LightingShadowMaskUV], _LightingShadowMaskPan).r;
                #else
                    float removeShadow = 1;
                #endif
                mainShadowMask *= removeShadow;
                firstSecondShadowMask *= removeShadow;
                
                albedo.rgb = lerp(albedo.rgb, lerp(firstShadeMap.rgb, secondShadeMap.rgb, firstSecondShadowMask), mainShadowMask);
            }
        }
        
        float3 calculateFinalLighting(inout float3 albedo, float4 finalColor)
        {
            float3 finalLighting = 1;
            // Additive Lighting
            #ifdef FORWARD_ADD_PASS
                fixed detailShadow = 1;
                UNITY_BRANCH
                if (_LightingDetailShadowsEnabled)
                {
                    detailShadow = lerp(1, POI2D_SAMPLER_PAN(_LightingDetailShadows, _MainTex, poiMesh.uv[_LightingDetailShadowsUV], _LightingDetailShadowsPan), _LightingAdditiveDetailStrength).r;
                }
                UNITY_BRANCH
                if (_LightingAdditiveType == 0) // Realistic

                {
                    finalLighting = poiLight.color * poiLight.attenuation * max(0, poiLight.nDotL) * detailShadow * poiLight.additiveShadow;
                }
                else if (_LightingAdditiveType == 1) // Toon

                {
                    #if defined(POINT) || defined(SPOT)
                        finalLighting = lerp(poiLight.color * max(poiLight.additiveShadow, _LightingAdditivePassthrough), poiLight.color * _LightingAdditivePassthrough, smoothstep(_LightingAdditiveGradientStart, _LightingAdditiveGradientEnd, 1 - (.5 * poiLight.nDotL + .5))) * poiLight.attenuation * detailShadow;
                    #else
                        finalLighting = lerp(poiLight.color * max(poiLight.attenuation, _LightingAdditivePassthrough), poiLight.color * _LightingAdditivePassthrough, smoothstep(_LightingAdditiveGradientStart, _LightingAdditiveGradientEnd, 1 - (.5 * poiLight.nDotL + .5))) * detailShadow;
                    #endif
                }
                else //if(_LightingAdditiveType == 2) // Wrapped

                {
                    float uv = saturate(RTWrapFunc(poiLight.nDotL, _LightingWrappedWrap, _LightingWrappedNormalization)) * detailShadow;
                    
                    poiLight.rampedLightMap = 1;
                    UNITY_BRANCH
                    if (_LightingRampType == 1) // Math Gradient
                    poiLight.rampedLightMap = lerp(_LightingShadowColor, float3(1, 1, 1), saturate(1 - smoothstep(_LightingGradientStart - .000001, _LightingGradientEnd, 1 - uv)));
                    // TODO: ramp texture or full shade/tint map for atlasing
                    //poiLight.rampedLightMap = lerp(1, UNITY_SAMPLE_TEX2D(_ToonRamp, float2(uv + _ShadowOffset, 1)), shadowStrength.r);
                    
                    float shadowatten = max(poiLight.additiveShadow, _LightingAdditivePassthrough);
                    return poiLight.color * poiLight.rampedLightMap * saturate(poiLight.attenuation * uv * shadowatten);
                }
            #endif
            
            // Base and Meta Lighting
            #if defined(FORWARD_BASE_PASS) || defined(POI_META_PASS)
                #ifdef VERTEXLIGHT_ON
                    poiLight.vFinalLighting = 0;
                    
                    for (int index = 0; index < 4; index++)
                    {
                        poiLight.vFinalLighting += calculateNonImportantLighting(poiLight.vAttenuation[index], poiLight.vAttenuationDotNL[index], albedo, poiLight.vColor[index], poiLight.vDotNL[index], poiLight.vCorrectedDotNL[index]);
                    }
                #endif
                
                switch(_LightingMode)
                {
                    case 0: // Toon Lighting
                    case 2: // or wrapped

                    {
                        // HSL Shading
                        UNITY_BRANCH
                        if (_LightingEnableHSL)
                        {
                            float3 HSLMod = float3(_LightingShadowHue * 2 - 1, _LightingShadowSaturation * 2 - 1, _LightingShadowLightness * 2 - 1) * (1 - poiLight.rampedLightMap);
                            albedo = lerp(albedo.rgb, ModifyViaHSL(albedo.rgb, HSLMod), _LightingHSLIntensity);
                        }
                        
                        // Normal Shading
                        UNITY_BRANCH
                        if (_LightingMinLightBrightness > 0)
                        {
                            poiLight.finalLighting = max(0.001, poiLight.finalLighting);
                            float finalluminance = calculateluminance(poiLight.finalLighting);
                            finalLighting = max(poiLight.finalLighting, poiLight.finalLighting / max(0.0001, (finalluminance / _LightingMinLightBrightness)));
                            poiLight.finalLighting = finalLighting;
                        }
                        else
                        {
                            finalLighting = poiLight.finalLighting;
                        }
                    }
                    break;
                    case 1: // realistic

                    {
                        fixed detailShadow = 1;
                        UNITY_BRANCH
                        if (_LightingDetailShadowsEnabled)
                        {
                            detailShadow = lerp(1, POI2D_SAMPLER_PAN(_LightingDetailShadows, _MainTex, poiMesh.uv[_LightingDetailShadowsUV], _LightingDetailShadowsPan), _LightingDetailStrength).r;
                        }
                        
                        float3 realisticLighting = calculateRealisticLighting(finalColor, detailShadow).rgb;
                        finalLighting = lerp(realisticLighting, dot(realisticLighting, float3(0.299, 0.587, 0.114)), _LightingMonochromatic);
                    }
                    break;
                    case 3: // Skin

                    {
                        float subsurfaceShadowWeight = 0.0h;
                        float3 ambientNormalWorld = poiMesh.normals[1];//aTangentToWorld(s, s.blurredNormalTangent);
                        
                        // Scattering mask.
                        float subsurface = 1;
                        float skinScatteringMask = _SssWeight * saturate(1.0h / _SssMaskCutoff * subsurface);
                        float skinScattering = saturate(subsurface * _SssScale * 2 + _SssBias);
                        
                        // Skin subsurface depth absorption tint.
                        // cf http://www.crytek.com/download/2014_03_25_CRYENGINE_GDC_Schultz.pdf pg 35
                        half3 absorption = exp((1.0h - subsurface) * _SssTransmissionAbsorption.rgb);
                        
                        // Albedo scale for absorption assumes ~0.5 luminance for Caucasian skin.
                        absorption *= saturate(finalColor.rgb * unity_ColorSpaceDouble.rgb);
                        
                        // Blurred normals for indirect diffuse and direct scattering.
                        ambientNormalWorld = normalize(lerp(poiMesh.normals[1], ambientNormalWorld, _SssBumpBlur));
                        
                        float ndlBlur = dot(poiMesh.normals[1], poiLight.direction) * 0.5h + 0.5h;
                        float lumi = dot(poiLight.color, half3(0.2126h, 0.7152h, 0.0722h));
                        float4 sssLookupUv = float4(ndlBlur, skinScattering * lumi, 0.0f, 0.0f);
                        half3 sss = poiLight.lightMap * poiLight.attenuation * tex2Dlod(_SkinLUT, sssLookupUv).rgb;
                        finalLighting = min(lerp(indirectLighting * _LightingShadowColor, _LightingShadowColor, _LightingIgnoreAmbientColor) + (sss * directLighting), directLighting);
                    }
                    break;
                    case 4:
                    {
                        finalLighting = directLighting;
                    }
                    break;
                }
            #endif
            return finalLighting;
        }
        
        
        void applyLighting(inout float4 finalColor, float3 finalLighting)
        {
            #ifdef VERTEXLIGHT_ON
                finalColor.rgb *= finalLighting + poiLight.vFinalLighting;
            #else
                //finalColor.rgb = blendSoftLight(finalColor.rgb, finalLighting);
                //finalColor.rgb *= saturate(poiLight.directLighting);
                finalColor.rgb *= finalLighting;
            #endif
        }
    #endif