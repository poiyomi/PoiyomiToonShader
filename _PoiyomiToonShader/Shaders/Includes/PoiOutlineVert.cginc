#ifndef OutlineVert
    #define OutlineVert
    
    v2f vert(appdata v)
    {
        UNITY_SETUP_INSTANCE_ID(v);
        v2f o;
        UNITY_INITIALIZE_OUTPUT(v2f, o);
        UNITY_TRANSFER_INSTANCE_ID(v, o);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        
        #ifdef FUN
            applyFun(v.vertex);
        #endif

        o.uv = v.texcoord + _OutlineGlobalPan.xy * _Time.y;
        
        o.normal = UnityObjectToWorldNormal(v.normal);
        float3 offset = o.normal * (_LineWidth/100);
        o.worldPos = mul(unity_ObjectToWorld, v.vertex) + float4(offset,0);
        
        o.pos = UnityWorldToClipPos(o.worldPos);
        
        UNITY_TRANSFER_SHADOW(o, o.uv);
        UNITY_TRANSFER_FOG(o, o.pos);
        return o;
    }
    
#endif