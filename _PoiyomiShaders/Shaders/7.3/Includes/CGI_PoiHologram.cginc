#ifndef POI_HOLOGRAM
    #define POI_HOLOGRAM
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_HoloAlphaMap); float4 _HoloAlphaMap_ST;
    uint _HoloCoordinateSpace; // 0 World, 1 Local, 2 UV
    float3 _HoloDirection;
    float _HoloScrollSpeed;
    float _HoloLineDensity;

    fixed _HoloFresnelAlpha;
    fixed _HoloRimSharpness;
    fixed _HoloRimWidth;
    void ApplyHoloAlpha(inout float4 color)
    {
        float uv = 0;
        UNITY_BRANCH
        if (_HoloCoordinateSpace == 0)
        {
            uv = dot(normalize(_HoloDirection), poiMesh.worldPos * _HoloLineDensity) + _Time.x * _HoloScrollSpeed;
        }
        UNITY_BRANCH
        if(_HoloCoordinateSpace == 1)
        {
            uv = dot(normalize(_HoloDirection), poiMesh.localPos * _HoloLineDensity) + _Time.x * _HoloScrollSpeed;
        }
        UNITY_BRANCH
        if(_HoloCoordinateSpace == 2)
        {
            uv = dot(_HoloDirection, poiMesh.uv[0] * _HoloLineDensity) + _Time.x * _HoloScrollSpeed;
        }
        float holoRim = saturate(1 - smoothstep(min(_HoloRimSharpness, _HoloRimWidth), _HoloRimWidth, poiCam.viewDotNormal));
        holoRim = abs(lerp(1, holoRim, _HoloFresnelAlpha));
        color.a *= UNITY_SAMPLE_TEX2D_SAMPLER(_HoloAlphaMap, _MainTex, uv).r * holoRim;
    }
    
#endif