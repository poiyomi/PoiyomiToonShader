#ifndef POIVERT
    #define POIVERT
    v2f vert(appdata v)
    {
        v2f o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        UNITY_TRANSFER_INSTANCE_ID(v, o);

        TANGENT_SPACE_ROTATION;
        o.localPos = v.vertex;
        o.pos = UnityObjectToClipPos(o.localPos);
        o.worldPos = mul(unity_ObjectToWorld, o.localPos);
        o.uv = v.texcoord.xy + _GlobalPanSpeed.xy * _Time.y;
        o.normal = UnityObjectToWorldNormal(v.normal);
        UNITY_TRANSFER_SHADOW(o, o.uv);
        #if defined(BINORMAL_PER_FRAGMENT)
            o.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
        #else
            o.tangent = UnityObjectToWorldDir(v.tangent.xyz);
            o.binormal = CreateBinormal(o.normal, o.tangent, v.tangent.w);
        #endif
        
        return o;
    }
#endif