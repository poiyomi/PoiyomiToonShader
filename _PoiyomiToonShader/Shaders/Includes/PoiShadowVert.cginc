void vertShadowCaster(VertexInput v,
#if !defined(V2F_SHADOW_CASTER_NOPOS_IS_EMPTY) || defined(UNITY_STANDARD_USE_SHADOW_UVS)
    out VertexOutputShadowCaster o,
#endif
out float4 opos: SV_POSITION)
{
    TRANSFER_SHADOW_CASTER_NOPOS(o, opos)
    #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
        o.uv = TRANSFORM_TEX(v.uv0 + _GlobalPanSpeed.xy * float2(_Time.y, _Time.y), _MainTex);
    #endif
}