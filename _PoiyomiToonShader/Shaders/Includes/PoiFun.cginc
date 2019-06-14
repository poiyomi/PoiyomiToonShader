#ifndef FUN
    #define FUN
    
    int _Mirror;
    
    void applyFun(inout float4 vertex)
    {
        bool inMirror = IsInMirror();
        UNITY_BRANCH
        if (_Mirror == 0)
        {
            return;
        }
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
    
    void applyFunFrag()
    {
        bool inMirror = IsInMirror();
        UNITY_BRANCH
        if(_Mirror == 0)
        {
            return;
        }
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
    
#endif