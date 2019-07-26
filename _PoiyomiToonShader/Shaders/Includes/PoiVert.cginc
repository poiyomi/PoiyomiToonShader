#ifndef POIVERT
    #define POIVERT
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
        
        TANGENT_SPACE_ROTATION;
        o.localPos = v.vertex;
        o.pos = UnityObjectToClipPos(o.localPos);
        o.screenPos = ComputeScreenPos(o.pos);
        o.worldPos = mul(unity_ObjectToWorld, o.localPos);
        o.uv = v.texcoord.xy + _GlobalPanSpeed.xy * _Time.y;
        o.normal = UnityObjectToWorldNormal(v.normal);
        o.tangent = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
        o.bitangent = normalize(cross(o.normal, o.tangent) * v.tangent.w);
        
        float3x3 objectToTangent = float3x3(
            v.tangent.xyz,
            cross(v.normal, v.tangent.xyz) * v.tangent.w,
            v.normal
        );
        o.tangentViewDir = mul(objectToTangent, ObjSpaceViewDir(v.vertex));
        
        UNITY_TRANSFER_SHADOW(o, o.uv);
        UNITY_TRANSFER_FOG(o, o.pos);
        return o;
    }
#endif