#include "UnityCG.cginc"
#include "UnityShaderVariables.cginc"

#define UNITY_STANDARD_USE_SHADOW_UVS 1

float4      _Color;
float       _Clip;
sampler2D   _MainTex;
float4      _MainTex_ST;
float4 		_GlobalPanSpeed;

struct VertexInput
{
    float4 vertex: POSITION;
    float3 normal: NORMAL;
    float2 uv0: TEXCOORD0;
};

#if !defined(V2F_SHADOW_CASTER_NOPOS_IS_EMPTY) || defined(UNITY_STANDARD_USE_SHADOW_UVS)
    struct VertexOutputShadowCaster
    {
        V2F_SHADOW_CASTER_NOPOS
        #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
            float2 uv: TEXCOORD1;
        #endif
    };
#endif


void vertShadowCaster(VertexInput v,
#if !defined(V2F_SHADOW_CASTER_NOPOS_IS_EMPTY) || defined(UNITY_STANDARD_USE_SHADOW_UVS)
    out VertexOutputShadowCaster o,
#endif
out float4 opos: SV_POSITION)
{
    TRANSFER_SHADOW_CASTER_NOPOS(o, opos)
    #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
        o.uv = TRANSFORM_TEX(v.uv0 + _GlobalPanSpeed.xy * float2(_Time.y,_Time.y), _MainTex);
    #endif
}


half4 fragShadowCaster(
    #if !defined(V2F_SHADOW_CASTER_NOPOS_IS_EMPTY) || defined(UNITY_STANDARD_USE_SHADOW_UVS)
        VertexOutputShadowCaster i
        #endif
        ): SV_Target
        {
            #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
                half alpha = tex2D(_MainTex, TRANSFORM_TEX(i.uv, _MainTex)).a * _Color.a;
                
                #ifdef CUTOUT
                    clip(alpha - _Clip);
                #endif
				#ifdef TRANSPARENT
					clip(alpha - 0.01);
				#endif
            #endif
            
            SHADOW_CASTER_FRAGMENT(i)
        }