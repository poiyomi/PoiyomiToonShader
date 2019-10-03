#ifndef OutlineVert
    #define OutlineVert
    
    uint _OutlineMode;
    float4 _OutlinePersonaDirection;
    float4 _OutlineDropShadowOffset;
    float _OutlineUseVertexColors;
    
    sampler2D _OutlineMask; float4 _OutlineMask_ST;
    v2f vert(appdata v)
    {
        UNITY_SETUP_INSTANCE_ID(v);
        v2f o;
        UNITY_INITIALIZE_OUTPUT(v2f, o);
        UNITY_TRANSFER_INSTANCE_ID(v, o);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        
        #ifdef MIRROR
            applyMirrorRenderVert(v.vertex);
        #endif
        
        o.uv0 = v.uv0 + _OutlineGlobalPan.xy * _Time.y;
        float outlineMask = poiMax(tex2Dlod(_OutlineMask, float4(TRANSFORM_TEX(o.uv0, _OutlineMask) + _Time.x * _OutlineTexturePan.zw, 0, 0)).rgb);
        
        UNITY_BRANCH
        if (_OutlineUseVertexColors == 0)
        {
            o.normal = UnityObjectToWorldNormal(v.normal);
        }
        else
        {
            o.normal = UnityObjectToWorldNormal(v.color);
        }
        
        
        float3 offset = o.normal * (_LineWidth / 100) * outlineMask;
        
        half offsetMultiplier = 1;
        UNITY_BRANCH
        if(_OutlineMode == 2)
        {
            float3 lightDirection = poiLight.direction = normalize(_WorldSpaceLightPos0 + unity_SHAr.xyz + unity_SHAg.xyz + unity_SHAb.xyz);
            offsetMultiplier = saturate(dot(lightDirection, o.normal));
            offset *= offsetMultiplier;
        }
        else if(_OutlineMode == 3)
        {
            half3 viewNormal = mul((float3x3)UNITY_MATRIX_V, o.normal);
            offsetMultiplier = saturate(dot(viewNormal.xy, normalize(_OutlinePersonaDirection.xy)));
            
            offset *= offsetMultiplier;
        }
        else if(_OutlineMode == 4)
        {
            offset = mul((float3x3)transpose(UNITY_MATRIX_V), _OutlineDropShadowOffset);
        }
        
        o.worldPos = mul(unity_ObjectToWorld, v.vertex) + float4(offset, 0);
        o.modelPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
        o.pos = UnityWorldToClipPos(o.worldPos);
        
        o.angleAlpha = 1;
        #ifdef POI_RANDOM
            o.angleAlpha = ApplyAngleBasedRendering(o.modelPos);
        #endif
        
        UNITY_TRANSFER_SHADOW(o, o.uv0);
        UNITY_TRANSFER_FOG(o, o.pos);
        return o;
    }
    
#endif