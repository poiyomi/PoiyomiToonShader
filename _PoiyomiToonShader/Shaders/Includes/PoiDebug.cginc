#ifndef POI_DEBUG
    #define POI_DEBUG
    
    float _DebugDisplayDebug;
    uint _DebugMeshData;
    uint _DebugLightingData;
    uint _DebugSpecularData;
    uint _DebugCameraData;
    
    void displayDebugInfo(inout float4 finalColor)
    {
        UNITY_BRANCH
        if (_DebugDisplayDebug != 0)
        {
            //Mesh Data
            if (_DebugMeshData == 1)
            {
                finalColor.rgb = poiMesh.vertexNormal;
                return;
            }
            else if(_DebugMeshData == 2)
            {
                finalColor.rgb = poiMesh.fragmentNormal;
                return;
            }
            else if(_DebugMeshData == 3)
            {
                finalColor.rgb = poiMesh.tangent;
                return;
            }
            else if(_DebugMeshData == 4)
            {
                finalColor.rgb = poiMesh.bitangent;
                return;
            }
            
            #ifdef POI_LIGHTING
                // Lighting
                if (_DebugLightingData == 1)
                {
                    finalColor.rgb = poiLight.attenuation;
                    return;
                }
                else if(_DebugLightingData == 2)
                {
                    finalColor.rgb = poiLight.directLighting;
                    return;
                }
                else if(_DebugLightingData == 3)
                {
                    finalColor.rgb = poiLight.indirectLighting;
                    return;
                }
                else if(_DebugLightingData == 4)
                {
                    finalColor.rgb = poiLight.lightMap;
                    return;
                }
                else if(_DebugLightingData == 5)
                {
                    finalColor.rgb = poiLight.rampedLightMap;
                    return;
                }
                else if(_DebugLightingData == 6)
                {
                    finalColor.rgb = poiLight.finalLighting;
                    return;
                }
                else if(_DebugLightingData == 7)
                {
                    finalColor.rgb = poiLight.nDotL;
                    return;
                }
            #endif
            
            #ifdef POI_SPECULAR
                //specular
                if (_DebugSpecularData == 1)
                {
                    finalColor.rgb = finalSpecular;
                    return;
                }
                else if(_DebugSpecularData == 2)
                {
                    finalColor.rgb = tangentDirectionMap;
                    return;
                }
                else if(_DebugSpecularData == 3)
                {
                    finalColor.rgb = shiftTexture;
                    return;
                }
            #endif
            
            if(_DebugCameraData == 1)
            {
                finalColor.rgb = poiCam.viewDir;
                return;
            }
            else if(_DebugCameraData == 2)
            {
                finalColor.rgb = poiCam.tangentViewDir;
                return;
            }
            else if(_DebugCameraData == 3)
            {
                finalColor.rgb = poiCam.forwardDir;
                return;
            }
            else if(_DebugCameraData == 4)
            {
                finalColor.rgb = poiCam.worldPos;
                return;
            }
            else if(_DebugCameraData == 5)
            {
                finalColor.rgb = poiCam.viewDotNormal;
                return;
            }
        }
    }
    
#endif