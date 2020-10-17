float _EnableTouchGlow, _EnableBulge;
uint _VertexManipulationHeightUV;

V2FShadow vertShadowCaster(VertexInputShadow v)
{
    V2FShadow o;
    UNITY_SETUP_INSTANCE_ID(v);
    
    applyLocalVertexTransformation(v.normal, v.vertex);
    UNITY_INITIALIZE_OUTPUT(V2FShadow, o);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    
    
    TRANSFER_SHADOW_CASTER_NOPOS(o, o.pos)
    o.modelPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
    o.uv = v.uv0;
    o.uv1 = v.uv1;
    o.uv2 = v.uv2;
    o.uv3 = v.uv3;
    
    
    float2 uvToUse = 0;
    UNITY_BRANCH
    if (_VertexManipulationHeightUV == 0)
    {
        uvToUse = v.uv0.xy;
    }
    UNITY_BRANCH
    if(_VertexManipulationHeightUV == 1)
    {
        uvToUse = v.uv1.xy;
    }
    UNITY_BRANCH
    if(_VertexManipulationHeightUV == 2)
    {
        uvToUse = v.uv2.xy;
    }
    UNITY_BRANCH
    if(_VertexManipulationHeightUV == 3)
    {
        uvToUse = v.uv3.xy;
    }
    float4 worldPos = float4(o.worldPos, 1);
    float4 localPos = float4(o.localPos, 1);
    applyWorldVertexTransformationShadow(worldPos, localPos, v.normal, uvToUse);
    o.worldPos = worldPos;
    o.localPos = localPos;
    
    UNITY_BRANCH
    if(_EnableTouchGlow || _EnableBulge)
    {
        o.pos = UnityObjectToClipPos(float3(0, 0, -5));
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
    return o;
}