#ifndef POI_DEBUG
    #define POI_DEBUG
    
    float _DebugEnabled;
    float _DebugMeshData;
    float _DebugLightingData;
    float _DebugCameraData;
    
    void displayDebugInfo(inout float4 finalColor)
    {
        UNITY_BRANCH
        if (_DebugEnabled != 0)
        {
            //Mesh Data
            if (_DebugMeshData == 1)
            {
                finalColor.rgb = poiMesh.normals[0];
                return;
            }
            else if(_DebugMeshData == 2)
            {
                finalColor.rgb = poiMesh.normals[1];
                return;
            }
            else if(_DebugMeshData == 3)
            {
                finalColor.rgb = poiMesh.tangent;
                return;
            }
            else if(_DebugMeshData == 4)
            {
                finalColor.rgb = poiMesh.binormal;
                return;
            }
            else if(_DebugMeshData == 5)
            {
                finalColor.rgb = poiMesh.localPos;
                return;
            }
            
            #ifdef POI_LIGHTING
                if(_DebugLightingData == 1)
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