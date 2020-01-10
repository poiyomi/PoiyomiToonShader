#ifndef POI_REFRACTION
    #define POI_REFRACTION
    
    sampler2D _PoiGrab;
    float _RefractionIndex;
    float _RefractionOpacity;
    float _RefractionChromaticAberattion;
    float _RefractionEnabled;
    uint _SourceBlend, _DestinationBlend;

    UNITY_DECLARE_TEX2D_NOSAMPLER(_RefractionOpacityMask); float4 _RefractionOpacityMask_ST;
    
    inline float4 Refraction(float indexOfRefraction, float chromaticAberration)
    {
        float4 grabPos = poiCam.grabPos;
        #if UNITY_UV_STARTS_AT_TOP
            float scale = -1.0;
        #else
            float scale = 1.0;
        #endif
        float halfPosW = grabPos.w * 0.5;
        grabPos.y = (grabPos.y - halfPosW) * _ProjectionParams.x * scale + halfPosW;
        #if SHADER_API_D3D9 || SHADER_API_D3D11
            grabPos.w += 0.00000000001;
        #endif
        float2 projgrabPos = (grabPos / grabPos.w).xy;
        float3 worldViewDir = normalize(UnityWorldSpaceViewDir(poiMesh.worldPos));
        float3 refractionOffset = ((((indexOfRefraction - 1.0) * mul(UNITY_MATRIX_V, float4(poiMesh.normals[1], 0.0))) * (1.0 / (grabPos.z + 1.0))) * (1.0 - dot(poiMesh.normals[1], worldViewDir)));
        float2 cameraRefraction = float2(refractionOffset.x, - (refractionOffset.y * _ProjectionParams.x));
        //return tex2D(_PoiGrab, (projgrabPos + cameraRefraction));
        
        float4 redAlpha = tex2D(_PoiGrab, (projgrabPos + cameraRefraction));
        float green = tex2D(_PoiGrab, (projgrabPos + (cameraRefraction * (1.0 - chromaticAberration)))).g;
        float blue = tex2D(_PoiGrab, (projgrabPos + (cameraRefraction * (1.0 + chromaticAberration)))).b;
        return float4(redAlpha.r, green, blue, redAlpha.a);
    }
    
    void applyRefraction(inout float4 finalColor)
    {
        float3 refraction = 1;
        UNITY_BRANCH
        if (_RefractionEnabled == 1)
        {
            refraction = Refraction(_RefractionIndex, _RefractionChromaticAberattion).rgb;
        }
        else
        {
            refraction = tex2Dproj(_PoiGrab, poiCam.grabPos);
        }
        finalColor.a *= alphaMask;
        finalColor = poiBlend(_SourceBlend, finalColor, _DestinationBlend, float4(refraction,1));
        finalColor.a = 1;
    }
    
#endif