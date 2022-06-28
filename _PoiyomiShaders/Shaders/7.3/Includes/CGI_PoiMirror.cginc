#ifndef POI_MIRROR
    #define POI_MIRROR
    
    int _Mirror;
    float _EnableMirrorTexture;
    POI_TEXTURE_NOSAMPLER(_MirrorTexture);
    
    
    void applyMirrorRenderVert(inout float4 vertex)
    {
        UNITY_BRANCH
        if (_Mirror != 0)
        {
            bool inMirror = IsInMirror();
            if(_Mirror == 1 && inMirror)
            {
                return;
            }
            if(_Mirror == 1 && !inMirror)
            {
                vertex = -1;
                return;
            }
            if(_Mirror == 2 && inMirror)
            {
                vertex = -1;
                return;
            }
            if(_Mirror == 2 && !inMirror)
            {
                return;
            }
        }
    }
    
    void applyMirrorRenderFrag()
    {
        UNITY_BRANCH
        if(_Mirror != 0)
        {
            bool inMirror = IsInMirror();
            if(_Mirror == 1 && inMirror)
            {
                return;
            }
            if(_Mirror == 1 && !inMirror)
            {
                clip(-1);
                return;
            }
            if(_Mirror == 2 && inMirror)
            {
                clip(-1);
                return;
            }
            if(_Mirror == 2 && !inMirror)
            {
                return;
            }
        }
    }
    
    #if(defined(FORWARD_BASE_PASS) || defined(FORWARD_ADD_PASS))
        void applyMirrorTexture()
        {
            UNITY_BRANCH
            if(_EnableMirrorTexture)
            {
                if(IsInMirror())
                {
                    mainTexture = POI2D_SAMPLER_PAN(_MirrorTexture, _MainTex, poiMesh.uv[_MirrorTextureUV], _MirrorTexturePan);
                }
            }
        }
    #endif
    
#endif