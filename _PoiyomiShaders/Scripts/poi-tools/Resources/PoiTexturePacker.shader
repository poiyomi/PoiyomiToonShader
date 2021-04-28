// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Hidden/Poi/TexturePacker"
{
	Properties
	{
		_Invert_Red("Invert_Red", Float) = 0
		_Invert_Green("Invert_Green", Float) = 0
		_Invert_Blue("Invert_Blue", Float) = 0
		_Invert_Alpha("Invert_Alpha", Float) = 0
		_Red("Red", 2D) = "white" {}
		_Green("Green", 2D) = "white" {}
		_Blue("Blue", 2D) = "white" {}
		_Alpha("Alpha", 2D) = "white" {}
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

			uniform sampler2D _Red;
			uniform float4 _Red_ST;
			uniform float _Invert_Red;
			uniform sampler2D _Green;
			uniform float4 _Green_ST;
			uniform float _Invert_Green;
			uniform sampler2D _Blue;
			uniform float4 _Blue_ST;
			uniform float _Invert_Blue;
			uniform sampler2D _Alpha;
			uniform float4 _Alpha_ST;
			uniform float _Invert_Alpha;
			
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
				float2 uv_Red = i.ase_texcoord.xy * _Red_ST.xy + _Red_ST.zw;
				float4 tex2DNode28 = tex2D( _Red, uv_Red );
				float4 temp_cast_0 = (_Invert_Red).xxxx;
				float4 lerpResult27 = lerp( tex2DNode28 , ( temp_cast_0 - tex2DNode28 ) , _Invert_Red);
				float2 uv_Green = i.ase_texcoord.xy * _Green_ST.xy + _Green_ST.zw;
				float4 tex2DNode12 = tex2D( _Green, uv_Green );
				float4 temp_cast_2 = (_Invert_Green).xxxx;
				float4 lerpResult20 = lerp( tex2DNode12 , ( temp_cast_2 - tex2DNode12 ) , _Invert_Green);
				float2 uv_Blue = i.ase_texcoord.xy * _Blue_ST.xy + _Blue_ST.zw;
				float4 tex2DNode14 = tex2D( _Blue, uv_Blue );
				float4 temp_cast_4 = (_Invert_Blue).xxxx;
				float4 lerpResult21 = lerp( tex2DNode14 , ( temp_cast_4 - tex2DNode14 ) , _Invert_Blue);
				float2 uv_Alpha = i.ase_texcoord.xy * _Alpha_ST.xy + _Alpha_ST.zw;
				float4 tex2DNode13 = tex2D( _Alpha, uv_Alpha );
				float4 temp_cast_6 = (_Invert_Alpha).xxxx;
				float4 lerpResult19 = lerp( tex2DNode13 , ( temp_cast_6 - tex2DNode13 ) , _Invert_Alpha);
				float4 appendResult30 = (float4(lerpResult27.r , lerpResult20.r , lerpResult21.r , lerpResult19.r));
				
				
				finalColor = appendResult30;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=15902
0;0;1368;850;1368.399;595.2781;1;True;False
Node;AmplifyShaderEditor.SamplerNode;14;-1193.289,314.7757;Float;True;Property;_Blue;Blue;6;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;25;-815.7044,759.9294;Float;False;Property;_Invert_Alpha;Invert_Alpha;3;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-819.5868,472.4816;Float;False;Property;_Invert_Blue;Invert_Blue;2;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-803.4256,177.2413;Float;False;Property;_Invert_Green;Invert_Green;1;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-795.8423,-109.6157;Float;False;Property;_Invert_Red;Invert_Red;0;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;28;-1189.017,-285.634;Float;True;Property;_Red;Red;4;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;12;-1199.358,5.317238;Float;True;Property;_Green;Green;5;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;13;-1182.523,665.4475;Float;True;Property;_Alpha;Alpha;7;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;16;-610.2974,-218.5994;Float;False;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;26;-570.7031,710.9296;Float;False;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;17;-612.9231,67.14128;Float;False;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;18;-589.0041,392.5837;Float;False;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;19;-279.5903,619.9736;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;27;-318.2486,-275.2707;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;20;-299.71,16.80488;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;21;-296.069,300.6409;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;30;98.28339,102.1202;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;369.802,98.57185;Float;False;True;2;Float;ASEMaterialInspector;0;1;Hidden/Poi/TexturePacker;0770190933193b94aaa3065e307002fa;0;0;Unlit;2;True;0;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;True;0;False;-1;0;False;-1;True;0;False;-1;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;RenderType=Opaque=RenderType;True;2;0;False;False;False;False;False;False;False;False;False;False;0;;0;0;Standard;0;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;0
WireConnection;16;0;29;0
WireConnection;16;1;28;0
WireConnection;26;0;25;0
WireConnection;26;1;13;0
WireConnection;17;0;31;0
WireConnection;17;1;12;0
WireConnection;18;0;15;0
WireConnection;18;1;14;0
WireConnection;19;0;13;0
WireConnection;19;1;26;0
WireConnection;19;2;25;0
WireConnection;27;0;28;0
WireConnection;27;1;16;0
WireConnection;27;2;29;0
WireConnection;20;0;12;0
WireConnection;20;1;17;0
WireConnection;20;2;31;0
WireConnection;21;0;14;0
WireConnection;21;1;18;0
WireConnection;21;2;15;0
WireConnection;30;0;27;0
WireConnection;30;1;20;0
WireConnection;30;2;21;0
WireConnection;30;3;19;0
WireConnection;0;0;30;0
ASEEND*/
//CHKSM=2C30DB01285F07958B9316BD81CB0A64AD7E3B0E