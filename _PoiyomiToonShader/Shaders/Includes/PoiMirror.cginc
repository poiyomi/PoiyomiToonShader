#ifndef POI_MIRROR
    #define POI_MIRROR
    
    int _Mirror;
    float _EnableMirrorTexture;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_MirrorTexture); float4 _MirrorTexture_ST;
    
    
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
                    mainTexture = UNITY_SAMPLE_TEX2D_SAMPLER(_MirrorTexture, _MainTex, TRANSFORM_TEX(poiMesh.uv[0], _MirrorTexture));
                }
            }
        }
    #endif
    
#endif