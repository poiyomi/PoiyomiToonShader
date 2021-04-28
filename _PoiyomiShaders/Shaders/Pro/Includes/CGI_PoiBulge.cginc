#ifndef POI_BULGE
    #define POI_BULGE
    
    float _BuldgeFadeLength;
    float _BuldgeHeight;
    
    #if defined(PROP_BULGEMASK) || !defined(OPTIMIZER_ENABLED)
        sampler2D _BulgeMask;
    #endif
    
    void bulgyWolgy(inout v2f o)
    {
        float depth = DecodeFloatRG(tex2Dlod(_CameraDepthTexture, float4(o.grabPos.xy / o.grabPos.w, 0, 0)));
        #if defined(PROP_BULGEMASK) || !defined(OPTIMIZER_ENABLED)
            float bulgeMask = tex2Dlod(_BulgeMask, float4(o.uv0.xy, 0, 0));
        #else
            float bulgeMask = 1.0;
        #endif
        
        depth = Linear01Depth(depth);
        
        float intersect = 0;
        if (depth != 1)
        {
            float diff = distance(depth, Linear01Depth(o.pos.z / o.pos.w));
            if(diff > 0)
            {
                intersect = 1 - smoothstep(0, _ProjectionParams.w * _BuldgeFadeLength, diff);
            }
        }
        float4 offset = intersect * _BuldgeHeight * float4(o.normal, 0);
        
        offset = IsInMirror() ? 0: offset;
        offset *= bulgeMask;
        
        o.worldPos = mul(unity_ObjectToWorld, o.localPos) + offset;
        o.localPos = mul(unity_WorldToObject, o.worldPos);
        o.pos = UnityObjectToClipPos(o.localPos);
    }
    
#endif