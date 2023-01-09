#ifndef POI_BLACKLIGHT
    #define POI_BLACKLIGHT
    
    float4 _BlackLightMaskStart;
    float4 _BlackLightMaskEnd;
    float4 _BlackLightMaskKeys;
    float _BlackLightMaskDebug;
    float _BlackLightMaskDissolve;
    float _BlackLightAdjustDissolve;
    float _BlackLightMaskMetallic;
    float _BlackLightMaskClearCoat;
    float _BlackLightMaskMatcap;
    float _BlackLightMaskMatcap2;
    float _BlackLightMaskEmission;
    float _BlackLightMaskEmission2;
    float _BlackLightMaskFlipbook;
    float _BlackLightMaskPanosphere;
    float _BlackLightMaskIridescence;
    float _BlackLightMove;
    
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
                        float4 comparison = _BlackLightMaskKeys;
                        if(unity_LightColor[lightIndex].a == comparison[maskIndex])
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