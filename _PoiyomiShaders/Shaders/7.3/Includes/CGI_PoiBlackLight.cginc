#ifndef POI_BLACKLIGHT
    #define POI_BLACKLIGHT
    
    float4 _BlackLightMaskStart;
    float4 _BlackLightMaskEnd;
    float4 _BlackLightMaskKeys;
    float _BlackLightMaskDebug;
    uint _BlackLightMaskDissolve;
    uint _BlackLightMaskMetallic;
    uint _BlackLightMaskClearCoat;
    uint _BlackLightMaskMatcap;
    uint _BlackLightMaskMatcap2;
    uint _BlackLightMaskEmission;
    uint _BlackLightMaskEmission2;
    uint _BlackLightMaskFlipbook;
    uint _BlackLightMaskPanosphere;
    uint _BlackLightMaskIridescence;
    
    half _BlackLightMaskGlitter;
    
    half4 blackLightMask;
    
    void createBlackLightMask()
    {
        blackLightMask = 0;
        #ifdef VERTEXLIGHT_ON
            
            for (int lightIndex = 0; lightIndex < 4; lightIndex ++)
            {
                float3 lightPos = float3(unity_4LightPosX0[lightIndex], unity_4LightPosY0[lightIndex], unity_4LightPosZ0[lightIndex]);
                if (!distance(unity_LightColor[lightIndex].rgb, float3(0, 0, 0)))
                {
                    for (int maskIndex = 0; maskIndex < 4; maskIndex ++)
                    {
                        if(unity_LightColor[lightIndex].a == _BlackLightMaskKeys[maskIndex])
                        {
                            blackLightMask[maskIndex] = max(blackLightMask[maskIndex], smoothstep(_BlackLightMaskEnd[maskIndex], _BlackLightMaskStart[maskIndex], distance(poiMesh.worldPos, lightPos)));
                        }
                    }
                }
            }
        #endif
    }
#endif

/*
#ifdef POI_BLACKLIGHT
    if (_BlackLightMaskDissolve != 4)
    {
        blackLightMask[mask];
    }
#endif
*/