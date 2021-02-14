#ifndef POI_UV_DISTORTION
    #define POI_UV_DISTORTION
    
    UNITY_DECLARE_TEX2D_NOSAMPLER(_DistortionFlowTexture); float4 _DistortionFlowTexture_ST;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_DistortionFlowTexture1); float4 _DistortionFlowTexture1_ST;
    POI_TEXTURE_NOSAMPLER(_DistortionMask);

    float _DistortionStrength;
    float _DistortionStrength1;
    float2 _DistortionSpeed;
    float2 _DistortionSpeed1;
    
    float2 getTorusUv(float2 uv)
    {
        // translated to hlsl from https://www.shadertoy.com/view/Md3Bz7
        // http://web.cs.ucdavis.edu/~amenta/s12/findnorm.pdf
        float phi = 6.28318530718f * uv.x;
        float theta = 6.28318530718f * uv.y;
        float3 c = cos(float3(phi, phi + 1.57079632679f, theta));
        float2 result = float2(c.x * c.z, -c.y * c.z);
        return result * 0.5 + 0.5;
    }
    
    float2 calculateDistortionUV(float2 uv)
    {
        half distortionMask = POI2D_SAMPLER_PAN(_DistortionMask, _MainTex, poiMesh.uv[_DistortionMaskUV], _DistortionMaskPan).r; 
        float4 flowVector = UNITY_SAMPLE_TEX2D_SAMPLER(_DistortionFlowTexture, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _DistortionFlowTexture) + _Time.x * _DistortionSpeed) * 2 - 1;
        float4 flowVector1 = UNITY_SAMPLE_TEX2D_SAMPLER(_DistortionFlowTexture1, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _DistortionFlowTexture1) + _Time.x * _DistortionSpeed1) * 2 - 1;
        flowVector *= _DistortionStrength;
        flowVector1 *= _DistortionStrength1;
        return uv + ((flowVector.xy + flowVector1.xy) / 2) * distortionMask;
    }
    
#endif