#ifndef POI_TOUCH
    #define POI_TOUCH

    sampler2D _BulgeMask;
    sampler2D _CameraDepthTexture;
    float _BuldgeFadeLength;
    float _BuldgeHeight;

    void bulgyWolgy(inout v2f o)
    {
        float depth = DecodeFloatRG(tex2Dlod(_CameraDepthTexture, float4(o.screenPos.xy / o.screenPos.w, 0, 0)));
        float bulgeMask = tex2Dlod(_BulgeMask,  float4(o.uv, 0, 0));
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

        offset = IsInMirror() ? 0 : offset;

        o.worldPos = mul(unity_ObjectToWorld, o.localPos) + offset;
        o.localPos = mul(unity_WorldToObject, o.worldPos);
        o.pos = UnityObjectToClipPos(o.localPos);
    }

#endif