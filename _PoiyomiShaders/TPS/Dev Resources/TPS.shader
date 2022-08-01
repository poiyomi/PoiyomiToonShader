// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Thry/TPS"
{
    Properties 
  { 
      [HideInInspector] shader_is_using_thry_editor("", Float)=0
        _MainTex ("Texture", 2D) = "white" {}

		[HideInInspector] m_start_tps_penetrator("Penetrator--{reference_property:_TPSPenetratorEnabled,tooltip:Enable TPS Penetrator: Requires the TPS Setup Wizard to be run (under Poi/TPS)}", Float) = 0
		[HideInInspector] m_start_pen_autoConfig("Configured By Tool", Float) = 0
		_TPS_PenetratorLength("Length of Penetrator Model--{tooltip:The length from the root of the P to the very tip}", Float) = 1
		[Vector3]_TPS_PenetratorScale("Scale of Penetrator Model", Vector) = (1,1,1,1)
		[Vector3]_TPS_PenetratorRight("Right Vector", Vector) = (1,0,0,0)
		[Vector3]_TPS_PenetratorUp("Up Vector", Vector) = (0,1,0,0)
		[Vector3]_TPS_PenetratorForward("Forward Vector", Vector) = (0,0,1,0)
		[Toggle(TPS_IsSkinnedMesh)]_TPS_IsSkinnedMeshRenderer("Baked Vertex Colors", Float) = 0
		_TPS_BakedMesh("Baked Mesh / Mask", 2D) = "white" {}
		[HideInInspector] m_end_pen_autoConfig("TPS", Float) = 0
		[Helpbox(1)]_TPSHelpbox("Penetrator allows your mesh to bend in the direction of an orifice. It is fully compatible with DPS. Requires the TPS Setup Wizard to be run afterwards. Click here to open the setup window.--{onClick:Thry.TPS.TPS_Setup}", Float) = 0
		[HideInInspector][ThryToggle(TPS_Penetrator)]_TPSPenetratorEnabled("Enabled", Float) = 0
			[Space(10)]
		[Toggle]_TPS_AnimatedToggle("Animatable Toggle--{tooltip:This is a toggle that can be animated}", Float) = 1
		[ThryRichLabel(13)]_TPSBezierHeader("Bezier--{tooltip: Changes how the penetrator bends}", Float) = 0
		_TPS_BezierStart("Bezier Start--{tooltip:Start later down the penetrator}", Range(0,5)) = 0.0
		_TPS_BezierSmoothness("Bezier Smoothness--{tooltip:Smoothness of bending}", Range(0.49,0.01)) = 0.4
		_TPS_SmoothStart("Smooth Start--{tooltip:When penetrator starts moving towards orifice}", Range(0.01,1)) = 1
		[ThryRichLabel(13)]_TPSSqueezeHeader("Squeeze--{tooltip:Penetrator contracts when entering an orifice}", Float) = 0
		_TPS_Squeeze("Squeeze Strength--{tooltip:Percentage penetrator squeezes}", Range(0,1)) = 0.3
		_TPS_SqueezeDistance("Squeeze Distance--{tooltip:Width of the squeezing}", Range(0.01,1)) = 0.2
		[ThryRichLabel(13)]_TPSBuldgeHeader("Buldge--{tooltip: Penetrator expands in front of the orifice}", Float) = 0
		_TPS_Buldge("Buldge--{tooltip:Amount in percentage}", Range(0,3)) = 0.3
		_TPS_BuldgeDistance("Buldge Distance--{tooltip:Width of the buldging}", Range(0.01,1)) = 0.2
		_TPS_BuldgeFalloffDistance("Buldge Falloff--{tooltip:Width of bulding in front of orifice}", Range(0.01,0.5)) = 0.05
		[ThryRichLabel(13)]_TPSPulsingHeader("Pulsing--{tooltip: Penetrator expands in pulses while entering orifice}", Float) = 0
		_TPS_PumpingStrength("Pumping Strength--{tooltip:Amount in percentage}", Range(0,1)) = 0
		_TPS_PumpingSpeed("Pumping Speed--{tooltip:Frequenzy of pulsing}", Range(0,10)) = 1
		_TPS_PumpingWidth("Pumping Width--{tooltip:Width of pulsing}", Range(0.01,1)) = 0.2
		[ThryRichLabel(13)]_TPSIdleHeader("Idle--{tooltip: Changes how the penetrator bends while no orifice is near}", Float) = 0
		[Helpbox(0)]_TPS_IdleGravity("Tip: For idle gravity & movement use physbones gravity & other functions", Float) = 0
		_TPS_IdleSkrinkWidth("Idle Shrink Width--{tooltip:P shrinks while not penetrating}", Range(0,1)) = 1
		_TPS_IdleSkrinkLength("Idle Shrink Length--{tooltip:P shrinks while not penetrating}", Range(0,1)) = 1
		//Hide These, animated only
		_TPS_BufferedDepth   ("_TPS2_BufferedDepth NL", Float) = 0
		_TPS_BufferedStrength("_TPS2_BufferedStrength NL", Float) = 0
		[HideInInspector] m_end_tps_penetrator("", Float) = 0
    }
	CustomEditor "Thry.ShaderEditor"
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase"}
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma target 4.0
			#pragma shader_feature TPS_IsSkinnedMesh

            #include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "AutoLight.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR0;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
				uint id : SV_VertexID;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

            struct v2f
            {
				float2 uv : TEXCOORD0;
				SHADOW_COORDS(1) // put shadows data into TEXCOORD1
					fixed3 diff : COLOR0;
				fixed3 ambient : COLOR1;
				float4 vertex : SV_POSITION;
				float4 vertexColor : COLOR2;

				UNITY_VERTEX_OUTPUT_STEREO
            };

			UNITY_DECLARE_TEX2D(_MainTex);

			#include "./tps.cginc"

            v2f vert (appdata v)
            {
                v2f o;

				UNITY_SETUP_INSTANCE_ID(v); 
				UNITY_INITIALIZE_OUTPUT(v2f, o); 
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); 

				ApplyTPSPenetrator(v.vertex, v.normal, v.color, v.id, v.uv);
                o.vertex = UnityObjectToClipPos(v.vertex);

				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				o.diff = nl * _LightColor0.rgb;
				o.ambient = ShadeSH9(half4(worldNormal, 1));
				o.vertexColor = v.color;
				// compute shadows data
				TRANSFER_SHADOW(o)
                return o;
            }

			

            fixed4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                fixed4 col = float4(1,1,1,0);
				fixed shadow = SHADOW_ATTENUATION(i);
				// darken light's illumination with shadow, keep ambient intact
				fixed3 lighting = i.diff * shadow + i.ambient;
				col.rgb *= lighting;

				col.rgb = lerp(col.rgb, float3(1, 0, 0), TPSBufferedDepth(i.vertex, i.vertexColor));
				//col.rgb = tex2Dlod(_TPS2_Grabpass, float4(0.05, 0.05, 0, 1)).rgb;

                return col;
            }
            ENDCG
        }

		Pass
		{
			Tags {"LightMode" = "ShadowCaster"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#pragma shader_feature TPS_IsSkinnedMesh
			#include "UnityCG.cginc"
			#pragma target 4.0

			struct v2f {
				V2F_SHADOW_CASTER;
			};

			UNITY_DECLARE_TEX2D(_MainTex);
			
			#include "./tps.cginc"

			v2f vert(appdata_full v, uint vid : SV_VertexID)
			{
				v2f o;
				ApplyTPSPenetrator(v.vertex, v.normal, v.color, vid, v.texcoord);
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
    }
}
