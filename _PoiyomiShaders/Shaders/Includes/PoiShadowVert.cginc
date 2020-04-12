float _EnableTouchGlow, _EnableBulge;
    uint vertexManipulationUV;

void vertShadowCaster(VertexInput v,
#if !defined(V2F_SHADOW_CASTER_NOPOS_IS_EMPTY) || defined(UNITY_STANDARD_USE_SHADOW_UVS)
    out VertexOutputShadowCaster o,
#endif
out float4 opos: SV_POSITION)
{
    applyLocalVertexTransformation(v.normal, v.vertex);
    
    UNITY_INITIALIZE_OUTPUT(VertexOutputShadowCaster, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    
    
    TRANSFER_SHADOW_CASTER_NOPOS(o, opos)
    o.modelPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
    o.uv = TRANSFORM_TEX(v.uv0 + _GlobalPanSpeed.xy * float2(_Time.y, _Time.y), _MainTex);
    o.uv1 = v.uv1;
    o.uv2 = v.uv2;
    o.uv3 = v.uv3;
    
    
    float2 uvToUse = 0;
    UNITY_BRANCH
    if (vertexManipulationUV == 0)
    {
        uvToUse = v.uv0.xy;
    }
    UNITY_BRANCH
    if(vertexManipulationUV == 1)
    {
        uvToUse = v.uv1.xy;
    }
    UNITY_BRANCH
    if(vertexManipulationUV == 2)
    {
        uvToUse = v.uv2.xy;
    }
    UNITY_BRANCH
    if(vertexManipulationUV == 3)
    {
        uvToUse = v.uv3.xy;
    }
    float4 worldPos = float4(o.worldPos,1);
    float4 localPos = float4(o.localPos,1);
    applyWorldVertexTransformationShadow(worldPos, localPos , v.normal, uvToUse);
    o.worldPos = worldPos;
    o.localPos = localPos;
    
    UNITY_BRANCH
    if(_EnableTouchGlow || _EnableBulge)
    {
        opos = UnityObjectToClipPos(float3(0, 0, -5));
        o.localPos = float3(0, 0, -5);
        o.worldPos = mul(unity_ObjectToWorld, o.localPos);
    }
    else
    {
        o.localPos = v.vertex;
        o.worldPos = mul(unity_ObjectToWorld, o.localPos);
        o.grabPos = ComputeGrabScreenPos(UnityObjectToClipPos(o.localPos));
    }
    o.angleAlpha = 1;
    #ifdef POI_RANDOM
        o.angleAlpha = ApplyAngleBasedRendering(o.modelPos, o.worldPos);
    #endif
}