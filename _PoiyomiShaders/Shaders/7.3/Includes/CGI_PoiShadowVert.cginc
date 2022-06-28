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
    
    o.localPos = v.vertex;
    o.worldPos = mul(unity_ObjectToWorld, o.localPos);
    
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
    
    applyWorldVertexTransformation(o.worldPos, o.localPos, v.normal, uvToUse);
    applyVertexGlitching(o.worldPos, o.localPos);
    applySpawnInVert(o.worldPos, o.localPos, v.uv0.xy);
    applyVertexRounding(o.worldPos, o.localPos);
    o.pos = UnityObjectToClipPos(o.localPos);
    o.grabPos = ComputeGrabScreenPos(o.pos);
    o.modelPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
    
    UNITY_BRANCH
    if(_EnableTouchGlow || _EnableBulge)
    {
        o.pos = UnityObjectToClipPos(float3(0, 0, -5));
        o.localPos.xyz = float3(0, 0, -5);
        o.worldPos = mul(unity_ObjectToWorld, o.localPos);
    }

    o.angleAlpha = 1;
    #ifdef POI_RANDOM
        o.angleAlpha = ApplyAngleBasedRendering(o.modelPos, o.worldPos);
    #endif
    
    
    o.pos = UnityClipSpaceShadowCasterPos(o.localPos, v.normal);
    o.pos = UnityApplyLinearShadowBias(o.pos);
    
    return o;
}