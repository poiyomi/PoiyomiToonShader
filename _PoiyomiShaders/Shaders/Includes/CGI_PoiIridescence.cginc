#ifndef POI_IRIDESCENCE
    #define POI_IRIDESCENCE
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_IridescenceRamp); float4 _IridescenceRamp_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_IridescenceMask); float4 _IridescenceMask_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_IridescenceNormalMap); float4 _IridescenceNormalMap_ST;
    uint _IridescenceNormalUV;
    uint _IridescenceMaskUV;
    uint _IridescenceNormalSelection;
    float _IridescenceNormalIntensity;
    float _IridescenceNormalToggle;
    float _IridescenceIntensity;
    fixed _IridescenceAddBlend;
    fixed _IridescenceReplaceBlend;
    fixed _IridescenceMultiplyBlend;
    float _IridescenceEmissionStrength;
    
    //global
    
    float3 calculateNormal(float3 baseNormal)
    {
        float3 normal = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_IridescenceNormalMap, _MainTex, TRANSFORM_TEX(poiMesh.uv[_IridescenceNormalUV], _IridescenceNormalMap)), _IridescenceNormalIntensity);
        return normalize(
            normal.x * poiMesh.tangent +
            normal.y * poiMesh.binormal +
            normal.z * baseNormal
        );
    }
    
    float3 applyIridescence(inout float4 albedo)
    {
        float3 normal = poiMesh.normals[_IridescenceNormalSelection];
        
        // Use custom normal map
        UNITY_BRANCH
        if (_IridescenceNormalToggle)
        {
            normal = calculateNormal(normal);
        }
        
        float ndotv = dot(normal, poiCam.viewDir);
        
        float4 iridescenceColor = UNITY_SAMPLE_TEX2D_SAMPLER(_IridescenceRamp, _MainTex, 1 - abs(ndotv));
        float4 iridescenceMask = UNITY_SAMPLE_TEX2D_SAMPLER(_IridescenceMask, _MainTex, TRANSFORM_TEX(poiMesh.uv[_IridescenceMaskUV], _IridescenceMask));
        
        #ifdef POI_BLACKLIGHT
            if(_BlackLightMaskIridescence != 4)
            {
                iridescenceMask *= blackLightMask[_BlackLightMaskIridescence];
            }
        #endif
        
        
        albedo.rgb = lerp(albedo.rgb, saturate(iridescenceColor.rgb * _IridescenceIntensity), iridescenceColor.a * _IridescenceReplaceBlend * iridescenceMask);
        albedo.rgb += saturate(iridescenceColor.rgb * _IridescenceIntensity * iridescenceColor.a * _IridescenceAddBlend * iridescenceMask);
        albedo.rgb *= saturate(lerp(1, iridescenceColor.rgb * _IridescenceIntensity, iridescenceColor.a * _IridescenceMultiplyBlend * iridescenceMask));
        
        return saturate(iridescenceColor.rgb * _IridescenceIntensity) * iridescenceColor.a * iridescenceMask * _IridescenceEmissionStrength;
    }
    
#endif