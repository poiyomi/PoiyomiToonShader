// Normals
float3 CreateBinormal(float3 normal, float3 tangent, float binormalSign)
{
    return cross(normal, tangent.xyz) *
    (binormalSign * unity_WorldTransformParams.w);
}

bool IsInMirror()
{
    return unity_CameraProjection[2][0] != 0.f || unity_CameraProjection[2][1] != 0.f;
}

// Camera
float3 getCameraPosition()
{
    #ifdef USING_STEREO_MATRICES
        return lerp(unity_StereoWorldSpaceCameraPos[0], unity_StereoWorldSpaceCameraPos[1], 0.5);
    #endif
    return _WorldSpaceCameraPos;
}

float3 getCameraForward()
{
    #if UNITY_SINGLE_PASS_STEREO
        float3 p1 = mul(unity_StereoCameraToWorld[0], float4(0, 0, 1, 1));
        float3 p2 = mul(unity_StereoCameraToWorld[0], float4(0, 0, 0, 1));
    #else
        float3 p1 = mul(unity_CameraToWorld, float4(0, 0, 1, 1));
        float3 p2 = mul(unity_CameraToWorld, float4(0, 0, 0, 1));
    #endif
    return normalize(p2 - p1);
}

float3 grayscale_vector_node()
{
    return float3(0, 0.3823529, 0.01845836);
}

float3 grayscale_for_light()
{
    return float3(0.298912, 0.586611, 0.114478);
}

//Math Operators

float remap(float x, float minOld, float maxOld, float minNew, float maxNew)
{
    return minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld);
}

float2 remap(float2 x, float2 minOld, float2 maxOld, float2 minNew, float2 maxNew)
{
    return minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld);
}

float3 remap(float3 x, float3 minOld, float3 maxOld, float3 minNew, float3 maxNew)
{
    return minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld);
}

float4 remap(float4 x, float4 minOld, float4 maxOld, float4 minNew, float4 maxNew)
{
    return minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld);
}