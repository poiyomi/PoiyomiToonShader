#ifndef POI_UV_DISTORTION
    #define POI_UV_DISTORTION
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_DistortionFlowTexture); float4 _DistortionFlowTexture_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_DistortionFlowTexture1); float4 _DistortionFlowTexture1_ST;
    
    float _DistortionStrength;
    float _DistortionStrength1;
    float2 _DistortionSpeed;
    float2 _DistortionSpeed1;
    
    float2 calculateDistortionUV(float2 uv)
    {
        _DistortionStrength *= .1;
        float4 flowVector = UNITY_SAMPLE_TEX2D_SAMPLER(_DistortionFlowTexture, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _DistortionFlowTexture) + _Time.x * _DistortionSpeed) * 2 - 1;
        float4 flowVector1 = UNITY_SAMPLE_TEX2D_SAMPLER(_DistortionFlowTexture1, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _DistortionFlowTexture1) + _Time.x * _DistortionSpeed1) * 2 - 1;
        flowVector *= _DistortionStrength;
        flowVector1 *= _DistortionStrength1;
        return uv + (flowVector.xy + flowVector1.xy) / 2;
    }
    
#endif