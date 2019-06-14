#ifndef PANOSPHERE
    #define PANOSPHERE
    
    sampler2D _PanosphereTexture; float4 _PanosphereTexture_ST;
    sampler2D _PanoMapTexture; float4 _PanoMapTexture_ST;
    float _PanoEmission;
    float _PanoBlend;
    float4 _PanosphereColor;
    float4 _PanosphereScroll;
    
    float3 panoColor;
    float panoMask;
    
    float2 StereoPanoProjection(float3 coords)
    {
        float3 normalizedCoords = normalize(coords);
        float latitude = acos(normalizedCoords.y);
        float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
        float2 sphereCoords = float2(longitude + _Time.y * _PanosphereScroll.x, latitude + _Time.y * _PanosphereScroll.y) * float2(0.5 / UNITY_PI, 1.0 / UNITY_PI);
        sphereCoords = float2(0.5, 1.0) - sphereCoords;
        return(sphereCoords + float4(0, 1 - unity_StereoEyeIndex, 1, 0.5).xy) * float4(0, 1 - unity_StereoEyeIndex, 1, 0.5).zw;
    }
    
    void calculatePanosphere(float3 worldPos, float2 uv)
    {
        float2 _StereoEnabled_var = StereoPanoProjection(normalize(_WorldSpaceCameraPos.xyz - worldPos.xyz) * - 1);
        panoColor = tex2D(_PanosphereTexture, TRANSFORM_TEX(_StereoEnabled_var, _PanosphereTexture)) * _PanosphereColor.rgb;
        panoMask = tex2D(_PanoMapTexture, TRANSFORM_TEX(uv, _PanoMapTexture));
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
