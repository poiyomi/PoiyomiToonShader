// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Hidden/Poi/TextureUnpacker"
{
	Properties
	{
		_Packed("Packed", 2D) = "white" {}
		_Mode("Mode", Range( 0 , 3)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}
	
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend Off
		Cull Back
		ColorMask RGBA
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		
		

		Pass
		{
			Name "Unlit"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			

			struct appdata
			{
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				float4 ase_texcoord : TEXCOORD0;
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			uniform float _Mode;
			uniform sampler2D _Packed;
			uniform float4 _Packed_ST;
			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_texcoord.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord.zw = 0;
				
				v.vertex.xyz +=  float3(0,0,0) ;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				fixed4 finalColor;
				float2 uv_Packed = i.ase_texcoord.xy * _Packed_ST.xy + _Packed_ST.zw;
				float4 tex2DNode32 = tex2D( _Packed, uv_Packed );
				float ifLocalVar34 = 0;
				if( _Mode == 0.0 )
				ifLocalVar34 = tex2DNode32.r;
				float ifLocalVar35 = 0;
				if( _Mode == 1.0 )
				ifLocalVar35 = tex2DNode32.g;
				float ifLocalVar36 = 0;
				if( _Mode == 2.0 )
				ifLocalVar36 = tex2DNode32.b;
				float ifLocalVar37 = 0;
				if( _Mode == 3.0 )
				ifLocalVar37 = tex2DNode32.a;
				float4 temp_cast_0 = (( ifLocalVar34 + ifLocalVar35 + ifLocalVar36 + ifLocalVar37 )).xxxx;
				
				
				finalColor = temp_cast_0;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=15902
193;147;1360;702;842.6202;284.3949;1.38019;True;False
Node;AmplifyShaderEditor.SamplerNode;32;-446.011,1.547681;Float;True;Property;_Packed;Packed;0;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;33;-414.2798,-86.22936;Float;False;Property;_Mode;Mode;1;0;Create;True;0;0;False;0;0;0;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;34;17.15299,-44.90865;Float;True;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;35;17.04439,123.3313;Float;True;False;5;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;36;17.16646,287.5046;Float;True;False;5;0;FLOAT;0;False;1;FLOAT;2;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;37;15.75801,456.534;Float;True;False;5;0;FLOAT;0;False;1;FLOAT;3;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;40;381.6085,-0.07576537;Float;True;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;632.4083,82.32277;Float;False;True;2;Float;ASEMaterialInspector;0;1;Hidden/Poi/TextureUnpacker;0770190933193b94aaa3065e307002fa;0;0;Unlit;2;True;0;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;True;0;False;-1;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;RenderType=Opaque=RenderType;True;2;0;False;False;False;False;False;False;False;False;False;False;0;;0;0;Standard;0;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;0
WireConnection;34;0;33;0
WireConnection;34;3;32;1
WireConnection;35;0;33;0
WireConnection;35;3;32;2
WireConnection;36;0;33;0
WireConnection;36;3;32;3
WireConnection;37;0;33;0
WireConnection;37;3;32;4
WireConnection;40;0;34;0
WireConnection;40;1;35;0
WireConnection;40;2;36;0
WireConnection;40;3;37;0
WireConnection;0;0;40;0
ASEEND*/
//CHKSM=42995AD2AB6FC08BF2F377F3474CE02E1DBC8E29