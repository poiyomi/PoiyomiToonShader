#ifndef PANOSPHERE
    #define PANOSPHERE
    
    sampler2D _PanosphereTexture; float4 _PanosphereTexture_ST;
    sampler2D _PanoMapTexture; float4 _PanoMapTexture_ST;
    float _PanoEmission;
    float _PanoBlend;
    float4 _PanosphereColor;
    float3 _PanospherePan;
    float _PanoToggle;
    float _PanoCubeMapToggle;
    samplerCUBE _PanoCubeMap; half4 _PanoCubeMap_HDR;
    
    float3 panoColor;
    float panoMask;
    
    float2 projectIt(float3 coords)
    {
        float3 normalizedCoords = normalize(coords);
        float latitude = acos(normalizedCoords.y);
        float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
        float2 sphereCoords = float2(longitude + _Time.y * _PanospherePan.x, latitude + _Time.y * _PanospherePan.y) * float2(1.0 / UNITY_PI, 1.0 / UNITY_PI);
        sphereCoords = float2(1.0, 1.0) - sphereCoords;
        return(sphereCoords + float4(0, 1 - unity_StereoEyeIndex, 1, 1.0).xy) * float4(0, 1 - unity_StereoEyeIndex, 1, 1.0).zw;
    }
    
    void calculatePanosphere()
    {
        panoMask = tex2D(_PanoMapTexture, TRANSFORM_TEX(poiMesh.uv[0], _PanoMapTexture));
        
        UNITY_BRANCH
        if (_PanoCubeMapToggle)
        {
            float3 cubeUV =  mul(poiRotationMatrixFromAngles(_PanospherePan.xyz * _Time.y) ,float4(-poiCam.viewDir,1));
            half4 cubemap = texCUBE (_PanoCubeMap, cubeUV);
            panoColor = DecodeHDR (cubemap, _PanoCubeMap_HDR);
        }
        else
        {
            float2 _StereoEnabled_var = projectIt(normalize(poiCam.worldPos.xyz - poiMesh.worldPos.xyz) * - 1);
            panoColor = tex2D(_PanosphereTexture, TRANSFORM_TEX(_StereoEnabled_var, _PanosphereTexture)) * _PanosphereColor.rgb;
        }
    }
    
    void applyPanosphereColor(inout float4 finalColor)
    {
        finalColor.rgb = lerp(finalColor.rgb, panoColor, _PanoBlend * panoMask);
    }
    
    void applyPanosphereEmission(inout float4 finalColor)
    {
        finalColor.rgb += panoColor * _PanoBlend * panoMask * _PanoEmission;
    }
    
#endif
