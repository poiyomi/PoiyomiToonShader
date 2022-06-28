#ifndef POI_DECAL
    #define POI_DECAL
    
    POI_TEXTURE_NOSAMPLER(_DecalTexture);
    POI_TEXTURE_NOSAMPLER(_DecalMask);
    float4 _DecalColor;
    fixed _DecalTiled;
    fixed _DecalBlendAdd;
    fixed _DecalBlendMultiply;
    fixed _DecalBlendReplace;
    half _DecalRotation;
    half2 _DecalScale;
    half2 _DecalPosition;
    half _DecalRotationSpeed;
    
    void applyDecal(inout float4 albedo)
    {
        float2 uv = poiMesh.uv[_DecalTextureUV];
        float2 decalCenter = _DecalPosition;
        float theta = radians(_DecalRotation + _Time.z * _DecalRotationSpeed);
        float cs = cos(theta);
        float sn = sin(theta);
        uv = float2((uv.x - decalCenter.x) * cs - (uv.y - decalCenter.y) * sn + decalCenter.x, (uv.x - decalCenter.x) * sn + (uv.y - decalCenter.y) * cs + decalCenter.y);
        uv = remap(uv, float2(0, 0) - _DecalScale / 2 + _DecalPosition, _DecalScale / 2 + _DecalPosition, float2(0, 0), float2(1, 1));
        
        half decalAlpha = 1;
        //float2 uv = TRANSFORM_TEX(poiMesh.uv[_DecalTextureUV], _DecalTexture) + _Time.x * _DecalTexturePan;
        float4 decalColor = POI2D_SAMPLER_PAN(_DecalTexture, _MainTex, uv, _DecalTexturePan);
        decalAlpha *= POI2D_SAMPLER_PAN(_DecalMask, _MainTex, poiMesh.uv[_DecalMaskUV], _DecalMaskPan).r;
        UNITY_BRANCH
        if (!_DecalTiled)
        {
            if(uv.x > 1 || uv.y > 1 || uv.x < 0 || uv.y < 0)
            {
                decalAlpha = 0;
            }
        }
        
        albedo.rgb = lerp(albedo.rgb, decalColor.rgb, decalColor.a * decalAlpha * _DecalBlendReplace);
        albedo.rgb *= lerp(1, decalColor.rgb, decalColor.a * decalAlpha * _DecalBlendMultiply);
        albedo.rgb += decalColor.rgb * decalColor.a * decalAlpha * _DecalBlendAdd;
        albedo = saturate(albedo);
    }
    
#endif