#ifndef POI_BACKFACE
    #define POI_BACKFACE
    
    float _BackFaceEnabled;
    uint _BackFaceTextureUV;
    float _BackFaceDetailIntensity;
    float _BackFaceEmissionStrength;
    float2 _BackFacePanning;
    float _BackFaceHueShift;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_BackFaceTexture); float4 _BackFaceTexture_ST;
    
    float3 BackFaceColor;
    void applyBackFaceTexture()
    {
        BackFaceColor = 0;
        UNITY_BRANCH
        if (_BackFaceEnabled)
        {
            if(!poiMesh.isFrontFace)
            {
                albedo = POI2D_SAMPLER_PAN(_BackFaceTexture, _MainTex, poiMesh.uv[_BackFaceTextureUV], _BackFacePanning);
                _DetailTexIntensity = _BackFaceDetailIntensity;
                BackFaceColor = albedo.rgb;
                _MainHueShift = _BackFaceHueShift;
            }
        }
    }
    
#endif