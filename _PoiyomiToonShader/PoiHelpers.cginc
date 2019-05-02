// Matcaps
float2 getMatcapUV(float3 viewDirection, float3 normalDirection)
{
    half3 worldViewUp = normalize(half3(0, 1, 0) - viewDirection * dot(viewDirection, half3(0, 1, 0)));
    half3 worldViewRight = normalize(cross(viewDirection, worldViewUp));
    half2 matcapUV = half2(dot(worldViewRight, normalDirection), dot(worldViewUp, normalDirection)) * 0.5 + 0.5;
    return matcapUV;
}

// Normals
float3 CreateBinormal(float3 normal, float3 tangent, float binormalSign)
{
    return cross(normal, tangent.xyz) *
    (binormalSign * unity_WorldTransformParams.w);
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

void InitializeFragmentNormal(inout v2f i)
{
    float3 mainNormal = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap, _MainTex,TRANSFORM_TEX(i.uv, _BumpMap)), _BumpScale);
    float3 detailNormal = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_DetailNormalMap, _MainTex,TRANSFORM_TEX(i.uv, _DetailNormalMap)), _DetailNormalMapScale);
    float3 tangentSpaceNormal = BlendNormals(mainNormal, detailNormal);
    
    #if defined(BINORMAL_PER_FRAGMENT)
        float3 binormal = CreateBinormal(i.normal, i.tangent.xyz, i.tangent.w);
    #else
        float3 binormal = i.binormal;
    #endif
    
    i.normal = normalize(
        tangentSpaceNormal.x * i.tangent +
        tangentSpaceNormal.y * binormal +
        tangentSpaceNormal.z * i.normal
    );
}

#ifdef PANOSPHERE
    // Panosphere
    float2 StereoPanoProjection(float3 coords)
    {
        float3 normalizedCoords = normalize(coords);
        float latitude = acos(normalizedCoords.y);
        float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
        float2 sphereCoords = float2(longitude + _Time.y * _PanosphereScroll.x, latitude + _Time.y * _PanosphereScroll.y) * float2(0.5 / UNITY_PI, 1.0 / UNITY_PI);
        sphereCoords = float2(0.5, 1.0) - sphereCoords;
        return(sphereCoords + float4(0, 1 - unity_StereoEyeIndex, 1, 0.5).xy) * float4(0, 1 - unity_StereoEyeIndex, 1, 0.5).zw;
    }
#endif