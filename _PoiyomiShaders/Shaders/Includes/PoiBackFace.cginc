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
            if(poiMesh.isFrontFace != 1)
            {
                albedo = UNITY_SAMPLE_TEX2D_SAMPLER(_BackFaceTexture, _MainTex, TRANSFORM_TEX(poiMesh.uv[_BackFaceTextureUV], _BackFaceTexture) + _BackFacePanning * _Time.x);
                _DetailTexIntensity = _BackFaceDetailIntensity;
                BackFaceColor = albedo.rgb;
                _MainHueShift = _BackFaceHueShift;
            }
        }
    }
    
#endif