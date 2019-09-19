// Normals


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

float remapClamped(float x, float minOld, float maxOld, float minNew, float maxNew)
{
    return clamp(minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld), minNew, maxNew);
}

float2 remapClamped(float2 x, float2 minOld, float2 maxOld, float2 minNew, float2 maxNew)
{
    return clamp(minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld), minNew, maxNew);
}

float3 remapClamped(float3 x, float3 minOld, float3 maxOld, float3 minNew, float3 maxNew)
{
    return clamp(minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld), minNew, maxNew);
}

float4 remapClamped(float4 x, float4 minOld, float4 maxOld, float4 minNew, float4 maxNew)
{
    return clamp(minNew + (x - minOld) * (maxNew - minNew) / (maxOld - minOld), minNew, maxNew);
}

float poiMax(float2 i)
{
    return max(i.x, i.y);
}

float poiMax(float3 i)
{
    return max(max(i.x, i.y), i.z);
}

float poiMax(float4 i)
{
    return max(max(max(i.x, i.y), i.z), i.w);
}

float4x4 poiAngleAxisRotationMatrix(float angle, float3 axis)
{
    axis = normalize(axis);
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;
    
    return float4x4(oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s, 0.0,
    oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c, oc * axis.y * axis.z - axis.x * s, 0.0,
    oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c, 0.0,
    0.0, 0.0, 0.0, 1.0);
}

float4x4 poiRotationMatrixFromAngles(float x, float y, float z)
{
    float angleX = radians(x);
    float c = cos(angleX);
    float s = sin(angleX);
    float4x4 rotateXMatrix = float4x4(1, 0, 0, 0,
    0, c, -s, 0,
    0, s, c, 0,
    0, 0, 0, 1);
    
    float angleY = radians(y);
    c = cos(angleY);
    s = sin(angleY);
    float4x4 rotateYMatrix = float4x4(c, 0, s, 0,
    0, 1, 0, 0,
    - s, 0, c, 0,
    0, 0, 0, 1);
    
    float angleZ = radians(z);
    c = cos(angleZ);
    s = sin(angleZ);
    float4x4 rotateZMatrix = float4x4(c, -s, 0, 0,
    s, c, 0, 0,
    0, 0, 1, 0,
    0, 0, 0, 1);
    
    return mul(mul(rotateXMatrix, rotateYMatrix), rotateZMatrix);
}

float4x4 poiRotationMatrixFromAngles(float3 angles)
{
    float angleX = radians(angles.x);
    float c = cos(angleX);
    float s = sin(angleX);
    float4x4 rotateXMatrix = float4x4(1, 0, 0, 0,
    0, c, -s, 0,
    0, s, c, 0,
    0, 0, 0, 1);
    
    float angleY = radians(angles.y);
    c = cos(angleY);
    s = sin(angleY);
    float4x4 rotateYMatrix = float4x4(c, 0, s, 0,
    0, 1, 0, 0,
    - s, 0, c, 0,
    0, 0, 0, 1);
    
    float angleZ = radians(angles.z);
    c = cos(angleZ);
    s = sin(angleZ);
    float4x4 rotateZMatrix = float4x4(c, -s, 0, 0,
    s, c, 0, 0,
    0, 0, 1, 0,
    0, 0, 0, 1);
    
    return mul(mul(rotateXMatrix, rotateYMatrix), rotateZMatrix);
}