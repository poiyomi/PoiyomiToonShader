#ifndef POI_HELPER
#define POI_HELPER

#ifndef pi
    #define pi float(3.14159265359)
#endif

float linearSin(float x)
{
    return pow(min(cos(pi * x / 2.0), 1.0 - abs(x)), 1.0);
}

float random(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.2383))) * 43758.5453123);
}

float2 random2(float2 p)
{
    return frac(sin(float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)))) * 43758.5453);
}

float3 random3(float3 p)
{
    return frac(sin(float3(dot(p, float3(127.1, 311.7, 248.6)), dot(p, float3(269.5, 183.3, 423.3)), dot(p, float3(248.3, 315.9, 184.2)))) * 43758.5453);
}

float3 mod(float3 x, float y)
{
    return x - y * floor(x / y);
}
float2 mod(float2 x, float y)
{
    return x - y * floor(x / y);
}

//1/7
#define K 0.142857142857
//3/7
#define Ko 0.428571428571

// Permutation polynomial: (34x^2 + x) mod 289
float3 Permutation(float3 x)
{
    return mod((34.0 * x + 1.0) * x, 289.0);
}

bool IsInMirror()
{
    return unity_CameraProjection[2][0] != 0.f || unity_CameraProjection[2][1] != 0.f;
}

float3 BoxProjection(float3 direction, float3 position, float4 cubemapPosition, float3 boxMin, float3 boxMax)
{
    #if UNITY_SPECCUBE_BOX_PROJECTION
        UNITY_BRANCH
        if (cubemapPosition.w > 0)
        {
            float3 factors = ((direction > 0 ? boxMax : boxMin) - position) / direction;
            float scalar = min(min(factors.x, factors.y), factors.z);
            direction = direction * scalar + (position - cubemapPosition.xyz);
        }
    #endif
    return direction;
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
        float3 p1 = mul(unity_CameraToWorld, float4(0, 0, 1, 1)).xyz;
        float3 p2 = mul(unity_CameraToWorld, float4(0, 0, 0, 1)).xyz;
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

#endif

half2 calcScreenUVs(half4 grabPos)
{
    half2 uv = grabPos.xy / (grabPos.w + 0.0000000001);
    #if UNITY_SINGLE_PASS_STEREO
        uv.xy *= half2(_ScreenParams.x * 2, _ScreenParams.y);
    #else
        uv.xy *= _ScreenParams.xy;
    #endif
    
    return uv;
}

float inverseLerp(float A, float B, float T)
{
    return(T - A) / (B - A);
}

float inverseLerp2(float2 a, float2 b, float2 value)
{
    float2 AB = b - a;
    float2 AV = value - a;
    return dot(AV, AB) / dot(AB, AB);
}

float inverseLerp3(float3 a, float3 b, float3 value)
{
    float3 AB = b - a;
    float3 AV = value - a;
    return dot(AV, AB) / dot(AB, AB);
}

float inverseLerp4(float4 a, float4 b, float4 value)
{
    float4 AB = b - a;
    float4 AV = value - a;
    return dot(AV, AB) / dot(AB, AB);
}

// Dithering
inline half Dither8x8Bayer(int x, int y)
{
    const half dither[ 64 ] = {
        1, 49, 13, 61, 4, 52, 16, 64,
        33, 17, 45, 29, 36, 20, 48, 32,
        9, 57, 5, 53, 12, 60, 8, 56,
        41, 25, 37, 21, 44, 28, 40, 24,
        3, 51, 15, 63, 2, 50, 14, 62,
        35, 19, 47, 31, 34, 18, 46, 30,
        11, 59, 7, 55, 10, 58, 6, 54,
        43, 27, 39, 23, 42, 26, 38, 22
    };
    int r = y * 8 + x;
    return dither[r] / 64;
}

// UV Manipulation
float2 TransformUV(half2 offset, half rotation, half2 scale, float2 uv)
{
    float theta = radians(rotation);
    scale = 1 - scale;
    float cs = cos(theta);
    float sn = sin(theta);
    float2 centerPoint = offset + .5;
    uv = float2((uv.x - centerPoint.x) * cs - (uv.y - centerPoint.y) * sn + centerPoint.x, (uv.x - centerPoint.x) * sn + (uv.y - centerPoint.y) * cs + centerPoint.y);
    
    return remap(uv, float2(0, 0) + offset + (scale * .5), float2(1, 1) + offset - (scale * .5), float2(0, 0), float2(1, 1));
}

bool isVR()
{
    // USING_STEREO_MATRICES
    #if UNITY_SINGLE_PASS_STEREO
        return true;
    #else
        return false;
    #endif
}

bool isVRHandCamera()
{
    return !isVR() && abs(UNITY_MATRIX_V[0].y) > 0.0000005;
}

bool isDesktop()
{
    return !isVRHandCamera();
}

bool isVRHandCameraPreview()
{
    return isVRHandCamera() && _ScreenParams.y == 720;
}

bool isVRHandCameraPicture()
{
    return isVRHandCamera() && _ScreenParams.y == 1080;
}

bool isPanorama()
{
    // Crude method
    // FOV=90=camproj=[1][1]
    return unity_CameraProjection[1][1] == 1 && _ScreenParams.x == 1075 && _ScreenParams.y == 1025;
}

float calculateluminance(float3 color)
{
    return color.r * 0.299 + color.g * 0.587 + color.b * 0.114;
}