#ifndef POI_DATA
    #define POI_DATA

    float FadeShadows(float attenuation, float3 worldPosition)
    {
        float viewZ = dot(_WorldSpaceCameraPos - worldPosition, UNITY_MATRIX_V[2].xyz);
        float shadowFadeDistance = UnityComputeShadowFadeDistance(worldPosition, viewZ);
        float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
        attenuation = saturate(attenuation + shadowFade);
        return attenuation;
    }
    
    void calculateAttenuation(v2f i)
    {
        UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos.xyz)
        poiLight.attenuation = FadeShadows(attenuation, i.worldPos.xyz);
    }
    
    void calculateLightDirection(v2f i)
    {
        #ifdef FORWARD_BASE_PASS
            poiLight.direction = normalize(_WorldSpaceLightPos0 + unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz);
        #else
            #if defined(POINT) || defined(SPOT)
                poiLight.direction = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
            #endif
        #endif
    }
    
    void calculateLightColor()
    {
        #ifdef FORWARD_BASE_PASS
            float3 magic = saturate(ShadeSH9(normalize(unity_SHAr + unity_SHAg + unity_SHAb)));
            float3 normalLight = saturate(_LightColor0.rgb);
            poiLight.color = magic+normalLight;
        #else
            #if defined(POINT) || defined(SPOT)
                poiLight.color = _LightColor0.rgb;
            #endif
        #endif
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
    
    float3 CreateBinormal(float3 normal, float3 tangent, float binormalSign)
    {
        return cross(normal, tangent.xyz) * (binormalSign * unity_WorldTransformParams.w);
    }
    
    void InitializeMeshData(inout v2f i)
    {
        poiMesh.vertexNormal = i.normal;
        poiMesh.bitangent = i.bitangent;
        poiMesh.tangent = i.tangent;
        poiMesh.worldPos = i.worldPos;
        poiMesh.localPos = i.localPos;
        poiMesh.uv = i.uv;
        poiMesh.modelPos = i.modelPos;
    }
    
    void initializeCamera(v2f i)
    {
        poiCam.viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
        poiCam.forwardDir = getCameraForward();
        poiCam.worldPos = _WorldSpaceCameraPos;
        poiCam.tangentViewDir = normalize(i.tangentViewDir);
        poiCam.distanceToModel = distance(poiMesh.modelPos, poiCam.worldPos);
    }
    
    void calculateTangentData()
    {
        poiTData.tangentTransform = float3x3(poiMesh.tangent, poiMesh.bitangent, poiMesh.vertexNormal);
        poiTData.tangentToWorld = transpose(float3x3(poiMesh.tangent, poiMesh.bitangent, poiMesh.vertexNormal));
    }

    void InitData(inout v2f i)
    {
        UNITY_SETUP_INSTANCE_ID(i);
        
        calculateAttenuation(i);
        calculateLightColor();
        calculateLightDirection(i);
        
        InitializeMeshData(i);
        initializeCamera(i);
        calculateTangentData();
        
        poiLight.halfDir = Unity_SafeNormalize(poiLight.direction + poiCam.viewDir);
        
    }
    
#endif