#ifndef POI_DATA
    #define POI_DATA
    
    float _ParallaxBias;
    
    void calculateAttenuation(v2f i)
    {
        UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos.xyz)
        poiLight.attenuation = attenuation;
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
            //poiLight.color = saturate(_LightColor0.rgb) + saturate(ShadeSH9(normalize(unity_SHAr + unity_SHAg + unity_SHAb)));
            float3 magic = saturate(ShadeSH9(normalize(unity_SHAr + unity_SHAg + unity_SHAb)));
            float3 normalLight = saturate(_LightColor0.rgb);
            poiLight.color = saturate(magic + normalLight);
        #else
            #if defined(POINT) || defined(SPOT)
                poiLight.color = _LightColor0.rgb;
            #endif
        #endif
    }
    
    float3 CreateBinormal(float3 normal, float3 tangent, float binormalSign)
    {
        return cross(normal, tangent.xyz) * (binormalSign * unity_WorldTransformParams.w);
    }
    
    void InitializeMeshData(inout v2f i)
    {
        poiMesh.normals[0] = normalize(i.normal);
        poiMesh.bitangent = CreateBinormal(i.normal, i.tangent.xyz, i.tangent.w);
        poiMesh.tangent = i.tangent.xyz;
        poiMesh.worldPos = i.worldPos;
        poiMesh.localPos = i.localPos;
        poiMesh.barycentricCoordinates = i.barycentricCoordinates;
        poiMesh.uv[0] = i.uv0.xy;
        poiMesh.uv[1] = i.uv0.zw;
        poiMesh.uv[2] = i.uv1.xy;
        poiMesh.uv[3] = i.uv1.zw;

        #ifdef POI_UV_DISTORTION
            poiMesh.uv[4] = calculateDistortionUV(i.uv0.xy);
            #else
            poiMesh.uv[4] = poiMesh.uv[0];
        #endif

        poiMesh.vertexColor = i.vertexColor;
        #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
            poiMesh.lightmapUV = i.lightmapUV;
        #endif
        poiMesh.modelPos = i.modelPos;
    }
    
    void initializeCamera(v2f i)
    {
        poiCam.viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
        poiCam.forwardDir = getCameraForward();
        poiCam.worldPos = _WorldSpaceCameraPos;
        poiCam.distanceToModel = distance(poiMesh.modelPos, poiCam.worldPos);
        poiCam.distanceToVert = distance(poiMesh.worldPos, poiCam.worldPos);
        poiCam.grabPos = i.grabPos;
        poiCam.screenUV = calcScreenUVs(i.grabPos);
        poiCam.clipPos = i.pos;
        #if defined(GRAIN)
            poiCam.screenPos = i.screenPos;
        #endif
        
        poiCam.tangentViewDir = normalize(i.tangentViewDir);
        poiCam.tangentViewDir.xy /= (poiCam.tangentViewDir.z + _ParallaxBias);
    }
    
    void calculateTangentData()
    {
        poiTData.tangentTransform = float3x3(poiMesh.tangent, poiMesh.bitangent, poiMesh.normals[0]);
        poiTData.tangentToWorld = transpose(float3x3(poiMesh.tangent, poiMesh.bitangent, poiMesh.normals[0]));
    }
    
    void InitData(inout v2f i)
    {
        UNITY_SETUP_INSTANCE_ID(i);
        
        calculateAttenuation(i);
        calculateLightColor();
        #if defined(VERTEXLIGHT_ON)
            poiLight.vertexLightColor = i.vertexLightColor;
        #endif
        calculateLightDirection(i);
        
        InitializeMeshData(i);
        initializeCamera(i);
        calculateTangentData();
        
        poiLight.halfDir = Unity_SafeNormalize(poiLight.direction + poiCam.viewDir);
    }
    
    void CalculateReflectionData()
    {
        #if defined(_METALLICGLOSSMAP) || defined(_COLORCOLOR_ON)
            poiCam.reflectionDir = reflect(-poiCam.viewDir, poiMesh.normals[1]);
            poiCam.vertexReflectionDir = reflect(-poiCam.viewDir, poiMesh.normals[0]);
        #endif
    }
#endif