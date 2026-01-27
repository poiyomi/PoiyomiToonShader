Shader ".poiyomi/Extras/Poiyomi Fake Shadow"
{
	Properties
	{
		[HideInInspector] m_mainOptions ("Fake Shadow", Float) = 0
		[HDR][Gamma]_Color ("Shadow Color", Color) = (0.85, 0.65, 0.7, 1)
		_MainTex ("Texture--{reference_properties:[_MainTexPan]}", 2D) = "white" {}
		[HideInInspector][Vector2]_MainTexPan ("Panning", Vector) = (0, 0, 0, 0)
		_ShadowDistance ("Shadow Distance", Range(0, 0.2)) = 0.05
		[Vector3]_ShadowDirectionBias ("Direction Bias--{tooltip:Manual offset to adjust shadow direction in local space}", Vector) = (0, 0, 0, 0)
		
		[HideInInspector] m_stencilOptions ("Stencil", Float) = 0
		[IntRange]_StencilRef ("Reference", Range(0, 255)) = 51
		[IntRange]_StencilReadMask ("Read Mask", Range(0, 255)) = 255
		[IntRange]_StencilWriteMask ("Write Mask", Range(0, 255)) = 255
		[Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp ("Compare Function", Int) = 3
		[Enum(UnityEngine.Rendering.StencilOp)]_StencilPass ("Pass Op", Int) = 0
		[Enum(UnityEngine.Rendering.StencilOp)]_StencilFail ("Fail Op", Int) = 0
		[Enum(UnityEngine.Rendering.StencilOp)]_StencilZFail ("ZFail Op", Int) = 0
		
		[HideInInspector] m_renderingOptions ("Rendering", Float) = 0
		[Enum(UnityEngine.Rendering.CullMode)]_Cull ("Cull", Int) = 2
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest ("ZTest", Int) = 4
		[Enum(Off, 0, On, 1)]_ZWrite ("ZWrite", Int) = 1
		[Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend ("Source Blend", Int) = 1
		[Enum(UnityEngine.Rendering.BlendMode)]_DstBlend ("Destination Blend", Int) = 0
		_Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
	}
	
	SubShader
	{
		Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest+55" }
		
		Pass
		{
			Name "FakeShadow"
			Tags { "LightMode"="ForwardBase" }
			
			Stencil
			{
				Ref [_StencilRef]
				ReadMask [_StencilReadMask]
				WriteMask [_StencilWriteMask]
				Comp [_StencilComp]
				Pass [_StencilPass]
				Fail [_StencilFail]
				ZFail [_StencilZFail]
			}
			
			Cull [_Cull]
			ZTest [_ZTest]
			ZWrite [_ZWrite]
			Blend [_SrcBlend] [_DstBlend]
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			
			UNITY_DECLARE_TEX2D(_MainTex);
			float4 _MainTex_ST;
			float2 _MainTexPan;
			float4 _Color;
			float4 _ShadowDirectionBias;
			float _ShadowDistance;
			float _Cutoff;
			
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				
				// Standard position
				o.pos = UnityObjectToClipPos(v.vertex);
				
				// Calculate light direction from main light + SH probes
				float3 lightDir = _WorldSpaceLightPos0.xyz;
				float mainLightLum = dot(lightDir, float3(0.299, 0.587, 0.114));
				float3 shDir = unity_SHAr.xyz * 0.333 + unity_SHAg.xyz * 0.333 + unity_SHAb.xyz * 0.333;
				lightDir = normalize(lightDir * mainLightLum + shDir + float3(0, 0.001, 0));
				
				// Add manual direction bias (transformed to world space)
				float biasLength = length(_ShadowDirectionBias.xyz);
				if (biasLength > 0.001)
				{
					float3 worldBias = normalize(mul((float3x3)unity_ObjectToWorld, _ShadowDirectionBias.xyz));
					lightDir = normalize(lightDir + biasLength * worldBias);
				}
				
				// Offset position in clip space (opposite to light direction)
				float4 lightShift = mul(UNITY_MATRIX_VP, float4(lightDir * _ShadowDistance, 0));
				o.pos -= lightShift;
				
				o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw + _Time.y * _MainTexPan;
				UNITY_TRANSFER_FOG(o, o.pos);
				
				return o;
			}
			
			float4 frag(v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				
				float4 col = UNITY_SAMPLE_TEX2D(_MainTex, i.uv) * _Color;
				clip(col.a - _Cutoff);
				
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
	
	CustomEditor "Thry.ShaderEditor"
}
