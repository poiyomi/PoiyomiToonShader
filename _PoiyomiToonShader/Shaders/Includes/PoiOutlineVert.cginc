#ifndef OutlineVert
    #define OutlineVert
    
    v2f vert(VertexInput v)
    {
        v2f o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        UNITY_TRANSFER_INSTANCE_ID(v, o);
        
        #ifdef FUN
            applyFun(v.vertex);
        #endif

        o.uv = v.texcoord0 + _OutlineGlobalPan.xy * _Time.y;
        
        o.normal = UnityObjectToWorldNormal(v.normal);
        float3 offset = o.normal * (_LineWidth/100);
        o.worldPos = mul(unity_ObjectToWorld, v.vertex) + float4(offset,0);
        
        o.pos = UnityWorldToClipPos(o.worldPos);
        
        UNITY_TRANSFER_SHADOW(o, o.uv);
        UNITY_TRANSFER_FOG(o, o.pos);
        return o;
    }
    
#endif