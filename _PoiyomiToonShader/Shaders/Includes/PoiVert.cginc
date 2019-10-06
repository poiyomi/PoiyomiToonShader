#ifndef POI_VERT
    #define POI_VERT
    
    void ComputeVertexLightColor(inout v2f i)
    {
        #if defined(VERTEXLIGHT_ON)
            i.vertexLightColor = Shade4PointLights(
                unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                unity_LightColor[0].rgb, unity_LightColor[1].rgb,
                unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                unity_4LightAtten0, i.worldPos, i.normal
            );
        #endif
    }
    
    v2f vert(appdata v)
    {
        UNITY_SETUP_INSTANCE_ID(v);
        v2f o;
        
        #ifdef POI_META_PASS
            v.vertex.xy = v.uv1 * unity_LightmapST.xy + unity_LightmapST.zw;
            v.vertex.z = v.vertex.z > 0 ? 0.0001: 0;
        #endif
        
        UNITY_INITIALIZE_OUTPUT(v2f, o);
        UNITY_TRANSFER_INSTANCE_ID(v, o);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        
        #ifdef MIRROR
            applyMirrorRenderVert(v.vertex);
        #endif
        
        TANGENT_SPACE_ROTATION;
        o.localPos = v.vertex;
        o.pos = UnityObjectToClipPos(o.localPos);
        o.screenPos = ComputeScreenPos(o.pos);
        o.worldPos = mul(unity_ObjectToWorld, o.localPos);
        o.uv0 = v.uv0.xy;
        o.uv1 = v.uv1.xy;
        o.uv2 = v.uv2.xy;
        o.uv3 = v.uv3.xy;
        o.normal = UnityObjectToWorldNormal(v.normal);
        #ifdef POI_PARALLAX
            o.localTangent = v.tangent;
        #endif
        o.tangent = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
        o.bitangent = normalize(cross(o.normal, o.tangent) * v.tangent.w);
        o.modelPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
        
        #ifdef POI_TOUCH
            bulgyWolgy(o);
        #endif
        
        o.angleAlpha = 1;
        #ifdef POI_RANDOM
            o.angleAlpha = ApplyAngleBasedRendering(o.modelPos, o.worldPos);
        #endif
        
        #if defined(LIGHTMAP_ON)
            o.lightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
        #endif
        #ifdef DYNAMICLIGHTMAP_ON
            o.lightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
        #endif
        
        UNITY_TRANSFER_SHADOW(o, o.uv0);
        UNITY_TRANSFER_FOG(o, o.pos);
        ComputeVertexLightColor(o);
        return o;
    }
#endif