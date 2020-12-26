#ifndef POI_DATA
    #define POI_DATA
    
    float _ParallaxBias;
    
    void calculateAttenuation(v2f i)
    {
        #if defined(POINT) || defined(SPOT)
            POI_LIGHT_ATTENUATION(attenuation, shadow, i, i.worldPos.xyz)
                poiLight.additiveShadow = shadow;
        #else
            UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos.xyz)
        #endif
        poiLight.attenuation = attenuation;
    }
    
    void calculateLightingData(in v2f i)
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
        
        #ifdef VERTEXLIGHT_ON
            float4 toLightX = unity_4LightPosX0 - i.worldPos.x;
            float4 toLightY = unity_4LightPosY0 - i.worldPos.y;
            float4 toLightZ = unity_4LightPosZ0 - i.worldPos.z;
            float4 lengthSq = 0;
            lengthSq += toLightX * toLightX;
            lengthSq += toLightY * toLightY;
            lengthSq += toLightZ * toLightZ;
            
            float4 lightAttenSq = unity_4LightAtten0;
            float4 atten = 1.0 / (1.0 + lengthSq * lightAttenSq);
            float4 vLightWeight = saturate(1 - (lengthSq * lightAttenSq / 25));
            poiLight.vAttenuation = min(atten, vLightWeight * vLightWeight);
            
            poiLight.vDotNL = 0;
            poiLight.vDotNL += toLightX * poiMesh.normals[1].x;
            poiLight.vDotNL += toLightY * poiMesh.normals[1].y;
            poiLight.vDotNL += toLightZ * poiMesh.normals[1].z;
            
            float4 corr = rsqrt(lengthSq);
            poiLight.vDotNL = max(0, poiLight.vDotNL * corr);
            poiLight.vAttenuationDotNL = poiLight.vAttenuation * poiLight.vDotNL;
            
            for (int index = 0; index < 4; index ++)
            {
                poiLight.vPosition[index] = float3(unity_4LightPosX0[index], unity_4LightPosY0[index], unity_4LightPosZ0[index]);
                
                float3 vertexToLightSource = poiLight.vPosition[index] - poiMesh.worldPos;
                poiLight.vDirection[index] = normalize(vertexToLightSource);
                //poiLight.vAttenuationDotNL[index] = 1.0 / (1.0 + unity_4LightAtten0[index] * poiLight.vDotNL[index]);
                poiLight.vColor[index] = unity_LightColor[index].rgb;
                poiLight.vHalfDir[index] = Unity_SafeNormalize(poiLight.vDirection[index] + poiCam.viewDir);
                poiLight.vDotNL[index] = dot(poiMesh.normals[1], -poiLight.vDirection[index]);
                poiLight.vCorrectedDotNL[index] = .5 * (poiLight.vDotNL[index] + 1);
                
                #ifdef POI_VAR_DOTLH
                    poiLight.vDotLH[index] = saturate(dot(poiLight.vDirection[index], poiLight.vHalfDir[index]));
                #endif
                
                #ifdef POI_VAR_DOTNH
                    poiLight.vDotNH[index] = saturate(dot(poiMesh.normals[1], poiLight.vHalfDir[index]));
                #endif
            }
        #endif
        
        #ifdef FORWARD_BASE_PASS
            poiLight.direction = normalize(_WorldSpaceLightPos0 + unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz);
        #else
            #if defined(POINT) || defined(SPOT)
                poiLight.direction = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
            #endif
        #endif
        
        poiLight.halfDir = normalize(poiLight.direction + poiCam.viewDir);
        
        #ifdef POI_VAR_DOTNH
            poiLight.dotNH = saturate(dot(poiMesh.normals[1], poiLight.halfDir));
        #endif
        
        #ifdef POI_VAR_DOTLH
            poiLight.dotLH = saturate(dot(poiLight.direction, poiLight.halfDir));
        #endif
        
        poiLight.nDotV = dot(poiMesh.normals[1], poiCam.viewDir);
        poiLight.N0DotV = dot(poiMesh.normals[0], poiCam.viewDir);
        poiLight.nDotL = dot(poiMesh.normals[1], poiLight.direction);
        poiLight.nDotH = dot(poiMesh.normals[1], poiLight.halfDir);
        poiLight.lDotv = dot(poiLight.direction, poiCam.viewDir);
        poiLight.lDotH = dot(poiLight.direction, poiLight.halfDir);
    }
    
    float3 CreateBinormal(float3 normal, float3 tangent, float binormalSign)
    {
        return cross(normal, tangent.xyz) * (binormalSign * unity_WorldTransformParams.w);
    }
    
    void InitializeMeshData(inout v2f i, uint facing)
    {
        poiMesh.normals[0] = normalize(i.normal);
        poiMesh.binormal = CreateBinormal(i.normal, i.tangent.xyz, i.tangent.w);
        poiMesh.tangent = i.tangent.xyz;
        
        #ifndef OUTLINE
            if (!facing)
            {
                poiMesh.normals[0] *= -1;
                poiMesh.tangent *= -1;
                poiMesh.binormal *= -1;
            }
        #endif
        
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
        
        #ifdef FUR
            poiMesh.furAlpha = i.furAlpha;
        #endif
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
        poiTData.tangentTransform = float3x3(poiMesh.tangent, poiMesh.binormal, poiMesh.normals[0]);
        poiTData.tangentToWorld = transpose(float3x3(poiMesh.tangent, poiMesh.binormal, poiMesh.normals[0]));
    }
    
    void InitData(inout v2f i, uint facing)
    {
        UNITY_SETUP_INSTANCE_ID(i);
        
        calculateAttenuation(i);
        
        InitializeMeshData(i, facing);
        initializeCamera(i);
        calculateTangentData();
    }
    
    void CalculateReflectionData()
    {
        #if defined(_METALLICGLOSSMAP) || defined(_COLORCOLOR_ON)
            poiCam.reflectionDir = reflect(-poiCam.viewDir, poiMesh.normals[1]);
            poiCam.vertexReflectionDir = reflect(-poiCam.viewDir, poiMesh.normals[0]);
        #endif
    }
#endif