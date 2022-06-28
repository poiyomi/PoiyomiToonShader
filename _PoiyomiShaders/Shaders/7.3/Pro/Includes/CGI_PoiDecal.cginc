#ifndef POI_DECAL
#define POI_DECAL


#if defined(PROP_DECALMASK) || !defined(OPTIMIZER_ENABLED)
    POI_TEXTURE_NOSAMPLER(_DecalMask);
#endif

#if defined(PROP_DECALTEXTURE) || !defined(OPTIMIZER_ENABLED)
    POI_TEXTURE_NOSAMPLER(_DecalTexture);
#else
    float2 _DecalTextureUV;
#endif
float4 _DecalColor;
fixed _DecalTiled;
float _DecalBlendType;
half _DecalRotation;
half2 _DecalScale;
half2 _DecalPosition;
half _DecalRotationSpeed;
float _DecalEmissionStrength;
float _DecalBlendAlpha;
float _DecalHueShiftEnabled;
float _DecalHueShift;
float _DecalHueShiftSpeed;

// Audio Link
half _AudioLinkDecal0ScaleBand;
float4 _AudioLinkDecal0Scale;
half _AudioLinkDecal0RotationBand;
float2 _AudioLinkDecal0Rotation;
half _AudioLinkDecal0AlphaBand;
float2 _AudioLinkDecal0Alpha;
half _AudioLinkDecal0EmissionBand;
float2 _AudioLinkDecal0Emission;

half _AudioLinkDecal1ScaleBand;
float4 _AudioLinkDecal1Scale;
half _AudioLinkDecal1RotationBand;
float2 _AudioLinkDecal1Rotation;
half _AudioLinkDecal1AlphaBand;
float2 _AudioLinkDecal1Alpha;
half _AudioLinkDecal1EmissionBand;
float2 _AudioLinkDecal1Emission;

half _AudioLinkDecal2ScaleBand;
float4 _AudioLinkDecal2Scale;
half _AudioLinkDecal2RotationBand;
float2 _AudioLinkDecal2Rotation;
half _AudioLinkDecal2AlphaBand;
float2 _AudioLinkDecal2Alpha;
half _AudioLinkDecal2EmissionBand;
float2 _AudioLinkDecal2Emission;

half _AudioLinkDecal3ScaleBand;
float4 _AudioLinkDecal3Scale;
half _AudioLinkDecal3RotationBand;
float2 _AudioLinkDecal3Rotation;
half _AudioLinkDecal3AlphaBand;
float2 _AudioLinkDecal3Alpha;
half _AudioLinkDecal3EmissionBand;
float2 _AudioLinkDecal3Emission;

#ifdef GEOM_TYPE_BRANCH_DETAIL
    #if defined(PROP_DECALTEXTURE1) || !defined(OPTIMIZER_ENABLED)
        POI_TEXTURE_NOSAMPLER(_DecalTexture1);
    #else
        float2 _DecalTexture1UV;
    #endif
    float4 _DecalColor1;
    fixed _DecalTiled1;
    float _DecalBlendType1;
    half _DecalRotation1;
    half2 _DecalScale1;
    half2 _DecalPosition1;
    half _DecalRotationSpeed1;
    float _DecalEmissionStrength1;
    float _DecalBlendAlpha1;
    float _DecalHueShiftEnabled1;
    float _DecalHueShift1;
    float _DecalHueShiftSpeed1;
#endif

#ifdef GEOM_TYPE_FROND
    #if defined(PROP_DECALTEXTURE2) || !defined(OPTIMIZER_ENABLED)
        POI_TEXTURE_NOSAMPLER(_DecalTexture2);
    #else
        float2 _DecalTexture2UV;
    #endif
    float4 _DecalColor2;
    fixed _DecalTiled2;
    float _DecalBlendType2;
    half _DecalRotation2;
    half2 _DecalScale2;
    half2 _DecalPosition2;
    half _DecalRotationSpeed2;
    float _DecalEmissionStrength2;
    float _DecalBlendAlpha2;
    float _DecalHueShiftEnabled2;
    float _DecalHueShift2;
    float _DecalHueShiftSpeed2;
#endif

#ifdef DEPTH_OF_FIELD_COC_VIEW
    #if defined(PROP_DECALTEXTURE3) || !defined(OPTIMIZER_ENABLED)
        POI_TEXTURE_NOSAMPLER(_DecalTexture3);
    #else
        float2 _DecalTexture3UV;
    #endif
    float4 _DecalColor3;
    fixed _DecalTiled3;
    float _DecalBlendType3;
    half _DecalRotation3;
    half2 _DecalScale3;
    half2 _DecalPosition3;
    half _DecalRotationSpeed3;
    float _DecalEmissionStrength3;
    float _DecalBlendAlpha3;
    float _DecalHueShiftEnabled3;
    float _DecalHueShift3;
    float _DecalHueShiftSpeed3;
#endif

// Parallax
float _Decal0Depth;
float _Decal1Depth;
float _Decal2Depth;
float _Decal3Depth;

float2 calcParallax(float height)
{
    return((height * - 1) + 1) * (poiCam.decalTangentViewDir.xy / poiCam.decalTangentViewDir.z);
}


float2 decalUV(float uvNumber, float2 position, half rotation, half rotationSpeed, half2 scale, float depth)
{
    float2 uv = poiMesh.uv[uvNumber] + calcParallax(depth + 1);
    float2 decalCenter = position;
    float theta = radians(rotation + _Time.z * rotationSpeed);
    float cs = cos(theta);
    float sn = sin(theta);
    uv = float2((uv.x - decalCenter.x) * cs - (uv.y - decalCenter.y) * sn + decalCenter.x, (uv.x - decalCenter.x) * sn + (uv.y - decalCenter.y) * cs + decalCenter.y);
    uv = remap(uv, float2(0, 0) - scale / 2 + position, scale / 2 + position, float2(0, 0), float2(1, 1));
    return uv;
}

inline float3 decalHueShift(float enabled, float3 color, float shift, float shiftSpeed)
{
    UNITY_BRANCH
    if (enabled)
    {
        color = hueShift(color, shift + _Time.x * shiftSpeed);
    }
    return color;
}

inline float applyTilingClipping(float enabled, float2 uv)
{
    float ret = 1;
    UNITY_BRANCH
    if (!enabled)
    {
        if (uv.x > 1 || uv.y > 1 || uv.x < 0 || uv.y < 0)
        {
            ret = 0;
        }
    }
    return ret;
}

void applyDecals(inout float4 albedo, inout float3 decalEmission)
{
    
    #if defined(PROP_DECALMASK) || !defined(OPTIMIZER_ENABLED)
        float4 decalMask = POI2D_SAMPLER_PAN(_DecalMask, _MainTex, poiMesh.uv[_DecalMaskUV], _DecalMaskPan);
    #else
        float4 decalMask = 1;
    #endif
    
    float4 decalColor = 1;
    float2 uv = 0;
    
    // Decal 0
    float2 decalScale = float2(1, 1);
    float decalRotation = 0;
    decalScale = _DecalScale;
    #if defined(PROP_DECALTEXTURE) || !defined(OPTIMIZER_ENABLED)
        #ifdef POI_AUDIOLINK
            UNITY_BRANCH
            if (poiMods.audioLinkTextureExists)
            {
                decalScale += lerp(_AudioLinkDecal0Scale.xy, _AudioLinkDecal0Scale.zw, poiMods.audioLink[_AudioLinkDecal0ScaleBand]);
                decalRotation += lerp(_AudioLinkDecal0Rotation.x, _AudioLinkDecal0Rotation.y, poiMods.audioLink[_AudioLinkDecal0RotationBand]);
            }
        #endif
        uv = decalUV(_DecalTextureUV, _DecalPosition, _DecalRotation + decalRotation, _DecalRotationSpeed, decalScale, _Decal0Depth);
        decalColor = POI2D_SAMPLER_PAN(_DecalTexture, _MainTex, uv, _DecalTexturePan) * _DecalColor;
    #else
        uv = decalUV(_DecalTextureUV, _DecalPosition, _DecalRotation + decalRotation, _DecalRotationSpeed, decalScale, _Decal0Depth);
        decalColor = _DecalColor;
    #endif
    decalColor.rgb = decalHueShift(_DecalHueShiftEnabled, decalColor.rgb, _DecalHueShift, _DecalHueShiftSpeed);
    decalColor.a *= applyTilingClipping(_DecalTiled, uv) * decalMask.r;

    float audioLinkDecalAlpha0 = 0;
    #ifdef POI_AUDIOLINK
        audioLinkDecalAlpha0 = lerp(_AudioLinkDecal0Alpha.x, _AudioLinkDecal0Alpha.y, poiMods.audioLink[_AudioLinkDecal0AlphaBand]) * poiMods.audioLinkTextureExists;
    #endif

    albedo.rgb = lerp(albedo.rgb, customBlend(albedo.rgb, decalColor.rgb, _DecalBlendType), decalColor.a * saturate(_DecalBlendAlpha + audioLinkDecalAlpha0));
    
    float audioLinkDecalEmission0 = 0;
    #ifdef POI_AUDIOLINK
        audioLinkDecalEmission0 = lerp(_AudioLinkDecal0Emission.x, _AudioLinkDecal0Emission.y, poiMods.audioLink[_AudioLinkDecal0EmissionBand]) * poiMods.audioLinkTextureExists;
    #endif

    decalEmission += decalColor.rgb * decalColor.a * max(_DecalEmissionStrength + audioLinkDecalEmission0, 0);
    #ifdef GEOM_TYPE_BRANCH_DETAIL
        // Decal 1
        decalScale = _DecalScale1;
        decalRotation = 0;
        #if defined(PROP_DECALTEXTURE1) || !defined(OPTIMIZER_ENABLED)
            #ifdef POI_AUDIOLINK
                UNITY_BRANCH
                if (poiMods.audioLinkTextureExists)
                {
                    decalScale += lerp(_AudioLinkDecal1Scale.xy, _AudioLinkDecal1Scale.zw, poiMods.audioLink[_AudioLinkDecal1ScaleBand]);
                    decalRotation += lerp(_AudioLinkDecal1Rotation.x, _AudioLinkDecal1Rotation.y, poiMods.audioLink[_AudioLinkDecal1RotationBand]);
                }
            #endif
            uv = decalUV(_DecalTexture1UV, _DecalPosition1, _DecalRotation1 + decalRotation, _DecalRotationSpeed1, decalScale, _Decal1Depth);
            decalColor = POI2D_SAMPLER_PAN(_DecalTexture1, _MainTex, uv, _DecalTexture1Pan) * _DecalColor1;
        #else
            uv = decalUV(_DecalTexture1UV, _DecalPosition1, _DecalRotation1 + decalRotation, _DecalRotationSpeed1, decalScale, _Decal1Depth);
            decalColor = _DecalColor1;
        #endif
        decalColor.rgb = decalHueShift(_DecalHueShiftEnabled1, decalColor.rgb, _DecalHueShift1, _DecalHueShiftSpeed1);
        decalColor.a *= applyTilingClipping(_DecalTiled1, uv) * decalMask.g;

        float audioLinkDecalAlpha1 = 0;
        #ifdef POI_AUDIOLINK
            audioLinkDecalAlpha1 = lerp(_AudioLinkDecal1Alpha.x, _AudioLinkDecal1Alpha.y, poiMods.audioLink[_AudioLinkDecal1AlphaBand]) * poiMods.audioLinkTextureExists;
        #endif

        albedo.rgb = lerp(albedo.rgb, customBlend(albedo.rgb, decalColor.rgb, _DecalBlendType1), decalColor.a * saturate(_DecalBlendAlpha1 + audioLinkDecalAlpha1));
        
        float audioLinkDecalEmission1 = 0;
        #ifdef POI_AUDIOLINK
            audioLinkDecalEmission1 = lerp(_AudioLinkDecal1Emission.x, _AudioLinkDecal1Emission.y, poiMods.audioLink[_AudioLinkDecal1EmissionBand]) * poiMods.audioLinkTextureExists;
        #endif
        
        decalEmission += decalColor.rgb * decalColor.a * max(_DecalEmissionStrength1 + audioLinkDecalEmission1, 0);
    #endif
    #ifdef GEOM_TYPE_FROND
        // Decal 2
        decalScale = _DecalScale2;
        decalRotation = 0;
        #if defined(PROP_DECALTEXTURE2) || !defined(OPTIMIZER_ENABLED)
            #ifdef POI_AUDIOLINK
                UNITY_BRANCH
                if (poiMods.audioLinkTextureExists)
                {
                    decalScale += lerp(_AudioLinkDecal2Scale.xy, _AudioLinkDecal2Scale.zw, poiMods.audioLink[_AudioLinkDecal2ScaleBand]);
                    decalRotation += lerp(_AudioLinkDecal2Rotation.x, _AudioLinkDecal2Rotation.y, poiMods.audioLink[_AudioLinkDecal2RotationBand]);
                }
            #endif
            uv = decalUV(_DecalTexture2UV, _DecalPosition2, _DecalRotation2 + decalRotation, _DecalRotationSpeed2, decalScale, _Decal2Depth);
            decalColor = POI2D_SAMPLER_PAN(_DecalTexture2, _MainTex, uv, _DecalTexture2Pan) * _DecalColor2;
        #else
            uv = decalUV(_DecalTexture2UV, _DecalPosition2, _DecalRotation2 + decalRotation, _DecalRotationSpeed2, decalScale, _Decal2Depth);
            decalColor = _DecalColor2;
        #endif
        decalColor.rgb = decalHueShift(_DecalHueShiftEnabled2, decalColor.rgb, _DecalHueShift2, _DecalHueShiftSpeed2);
        decalColor.a *= applyTilingClipping(_DecalTiled2, uv) * decalMask.b;

        float audioLinkDecalAlpha2 = 0;
        #ifdef POI_AUDIOLINK
            audioLinkDecalAlpha2 = lerp(_AudioLinkDecal2Alpha.x, _AudioLinkDecal2Alpha.y, poiMods.audioLink[_AudioLinkDecal2AlphaBand]) * poiMods.audioLinkTextureExists;
        #endif

        albedo.rgb = lerp(albedo.rgb, customBlend(albedo.rgb, decalColor.rgb, _DecalBlendType2), decalColor.a * saturate(_DecalBlendAlpha2 + audioLinkDecalAlpha2));

        float audioLinkDecalEmission2 = 0;
        #ifdef POI_AUDIOLINK
            audioLinkDecalEmission2 = lerp(_AudioLinkDecal2Emission.x, _AudioLinkDecal2Emission.y, poiMods.audioLink[_AudioLinkDecal2EmissionBand]) * poiMods.audioLinkTextureExists;
        #endif

        decalEmission += decalColor.rgb * decalColor.a * max(_DecalEmissionStrength2 + audioLinkDecalEmission2, 0);
    #endif
    #ifdef DEPTH_OF_FIELD_COC_VIEW
        // Decal 3
        decalScale = _DecalScale3;
        decalRotation = 0;
        #if defined(PROP_DECALTEXTURE3) || !defined(OPTIMIZER_ENABLED)
            #ifdef POI_AUDIOLINK
                UNITY_BRANCH
                if (poiMods.audioLinkTextureExists)
                {
                    decalScale += lerp(_AudioLinkDecal3Scale.xy, _AudioLinkDecal3Scale.zw, poiMods.audioLink[_AudioLinkDecal3ScaleBand]);
                    decalRotation += lerp(_AudioLinkDecal3Rotation.x, _AudioLinkDecal3Rotation.y, poiMods.audioLink[_AudioLinkDecal3RotationBand]);
                }
            #endif
            uv = decalUV(_DecalTexture3UV, _DecalPosition3, _DecalRotation3 + decalRotation, _DecalRotationSpeed3, decalScale, _Decal3Depth);
            decalColor = POI2D_SAMPLER_PAN(_DecalTexture3, _MainTex, uv, _DecalTexture3Pan) * _DecalColor3;
        #else
            uv = decalUV(_DecalTexture3UV, _DecalPosition3, _DecalRotation3 + decalRotation, _DecalRotationSpeed3, decalScale, _Decal3Depth);
            decalColor = _DecalColor3;
        #endif
        decalColor.rgb = decalHueShift(_DecalHueShiftEnabled3, decalColor.rgb, _DecalHueShift3, _DecalHueShiftSpeed3);
        decalColor.a *= applyTilingClipping(_DecalTiled3, uv) * decalMask.a;

        float audioLinkDecalAlpha3 = 0;
        #ifdef POI_AUDIOLINK
            audioLinkDecalAlpha3 = lerp(_AudioLinkDecal3Alpha.x, _AudioLinkDecal3Alpha.y, poiMods.audioLink[_AudioLinkDecal3AlphaBand]) * poiMods.audioLinkTextureExists;
        #endif

        albedo.rgb = lerp(albedo.rgb, customBlend(albedo.rgb, decalColor.rgb, _DecalBlendType3), decalColor.a * saturate(_DecalBlendAlpha3 + audioLinkDecalAlpha3));

        float audioLinkDecalEmission3 = 0;
        #ifdef POI_AUDIOLINK
            audioLinkDecalEmission3 = lerp(_AudioLinkDecal3Emission.x, _AudioLinkDecal3Emission.y, poiMods.audioLink[_AudioLinkDecal3EmissionBand]) * poiMods.audioLinkTextureExists;
        #endif

        decalEmission += decalColor.rgb * decalColor.a * max(_DecalEmissionStrength3 + audioLinkDecalEmission3, 0);
    #endif
    
    albedo = saturate(albedo);
}

#endif