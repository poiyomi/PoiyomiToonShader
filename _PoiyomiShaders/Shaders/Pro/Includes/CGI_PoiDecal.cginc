#ifndef POI_DECAL
    #define POI_DECAL
    
    
    #if defined(PROP_DECALMASK) || !defined(OPTIMIZER_ENABLED)
        POI_TEXTURE_NOSAMPLER(_DecalMask);
    #endif
    
    #if defined(PROP_DECALTEXTURE) || !defined(OPTIMIZER_ENABLED)
        POI_TEXTURE_NOSAMPLER(_DecalTexture);
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
    
    #ifdef GEOM_TYPE_BRANCH_DETAIL
        #if defined(PROP_DECALTEXTURE1) || !defined(OPTIMIZER_ENABLED)
            POI_TEXTURE_NOSAMPLER(_DecalTexture1);
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
    
    float2 decalUV(float uvNumber, float2 position, half rotation, half rotationSpeed, half2 scale)
    {
        float2 uv = poiMesh.uv[uvNumber];
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
        if(!enabled)
        {
            if(uv.x > 1 || uv.y > 1 || uv.x < 0 || uv.y < 0)
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
        #if defined(PROP_DECALTEXTURE) || !defined(OPTIMIZER_ENABLED)
            uv = decalUV(_DecalTextureUV, _DecalPosition, _DecalRotation, _DecalRotationSpeed, _DecalScale);
            decalColor = POI2D_SAMPLER_PAN(_DecalTexture, _MainTex, uv, _DecalTexturePan) * _DecalColor;
        #else
            decalColor = _DecalColor;
        #endif
        decalColor.rgb = decalHueShift(_DecalHueShiftEnabled, decalColor.rgb, _DecalHueShift, _DecalHueShiftSpeed);
        decalColor.a *= applyTilingClipping(_DecalTiled, uv) * decalMask.r;
        albedo.rgb = lerp(albedo.rgb, customBlend(albedo.rgb, decalColor.rgb, _DecalBlendType), decalColor.a * _DecalBlendAlpha);
        
        decalEmission += decalColor.rgb * decalColor.a * _DecalEmissionStrength;
        
        #ifdef GEOM_TYPE_BRANCH_DETAIL
            // Decal 1
            #if defined(PROP_DECALTEXTURE1) || !defined(OPTIMIZER_ENABLED)
                uv = decalUV(_DecalTexture1UV, _DecalPosition1, _DecalRotation1, _DecalRotationSpeed1, _DecalScale1);
                decalColor = POI2D_SAMPLER_PAN(_DecalTexture1, _MainTex, uv, _DecalTexture1Pan) * _DecalColor1;
            #else
                decalColor = _DecalColor1;
            #endif
            decalColor.rgb = decalHueShift(_DecalHueShiftEnabled1, decalColor.rgb, _DecalHueShift1, _DecalHueShiftSpeed1);
            decalColor.a *= applyTilingClipping(_DecalTiled, uv) * decalMask.g;
            albedo.rgb = lerp(albedo.rgb, customBlend(albedo.rgb, decalColor.rgb, _DecalBlendType1), decalColor.a * _DecalBlendAlpha1);
            decalEmission += decalColor.rgb * decalColor.a * _DecalEmissionStrength1;
        #endif
        #ifdef GEOM_TYPE_FROND
            // Decal 2
            #if defined(PROP_DECALTEXTURE2) || !defined(OPTIMIZER_ENABLED)
                uv = decalUV(_DecalTexture2UV, _DecalPosition2, _DecalRotation2, _DecalRotationSpeed2, _DecalScale2);
                decalColor = POI2D_SAMPLER_PAN(_DecalTexture2, _MainTex, uv, _DecalTexture2Pan) * _DecalColor2;
            #else
                decalColor = _DecalColor2;
            #endif
            decalColor.rgb = decalHueShift(_DecalHueShiftEnabled2, decalColor.rgb, _DecalHueShift2, _DecalHueShiftSpeed2);
            decalColor.a *= applyTilingClipping(_DecalTiled, uv) * decalMask.b;
            albedo.rgb = lerp(albedo.rgb, customBlend(albedo.rgb, decalColor.rgb, _DecalBlendType2), decalColor.a * _DecalBlendAlpha2);
            decalEmission += decalColor.rgb * decalColor.a * _DecalEmissionStrength2;
        #endif
        #ifdef DEPTH_OF_FIELD_COC_VIEW
            // Decal 3
            #if defined(PROP_DECALTEXTURE3) || !defined(OPTIMIZER_ENABLED)
                uv = decalUV(_DecalTexture3UV, _DecalPosition3, _DecalRotation3, _DecalRotationSpeed3, _DecalScale3);
                decalColor = POI2D_SAMPLER_PAN(_DecalTexture3, _MainTex, uv, _DecalTexture3Pan) * _DecalColor3;
            #else
                decalColor = _DecalColor3;
            #endif
            decalColor.rgb = decalHueShift(_DecalHueShiftEnabled3, decalColor.rgb, _DecalHueShift3, _DecalHueShiftSpeed3);
            decalColor.a *= applyTilingClipping(_DecalTiled, uv) * decalMask.a;
            albedo.rgb = lerp(albedo.rgb, customBlend(albedo.rgb, decalColor.rgb, _DecalBlendType3), decalColor.a * _DecalBlendAlpha3);
            decalEmission += decalColor.rgb * decalColor.a * _DecalEmissionStrength3;
        #endif
        
        albedo = saturate(albedo);
    }
    
#endif