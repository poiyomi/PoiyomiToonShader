Shader "Thry/ThryEditorExample" {
	Properties {

		[HideInInspector] shader_is_using_thry_editor ("", Float) = 0
        [HideInInspector] shader_master_label ("<color=#008080>❤ Editor Example ❤</color>", Float) = 0
        [HideInInspector] shader_presets ("ThryPresetsExample", Float) = 0
		[HideInInspector] shader_properties_label_file ("ThryLabelExample", Float) = 0
		
		_Color ("Color", Color) = (1,1,1,1)
		[HideInInspector] m_textures ("Textures", Float) = 0
		[Texture] _MainTex0 ("", 2D) = "white" {}
		[TextureNoSO] _MainTex01 ("", 2D) = "white" {}
		[SmallTexture] _MainTex1 ("", 2D) = "white" {}
		[BigTexture] _MainTex2 ("", 2D) = "white" {}
		[SmallTextureNoSO] _MainTex3 ("", 2D) = "white" {}
		[BigTextureNoSO]_MainTex4 ("", 2D) = "white" {}
		[HideInInspector] m_start_special ("", Float) = 0
		[Gradient] _MainTex5 ("", 2D) = "white" {}
		[HideInInspector] m_end_special ("", Float) = 0
		[HideInInspector] m_other_stuff ("", Float) = 0
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		[Toggle] _ToggleSmth ("", Float) = 1
		[HideInInspector] m_already_shown_stuff ("", Float) = 0
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	CustomEditor "ThryEditor"
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex0;

		struct Input {
			float2 uv_MainTex0;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex0, IN.uv_MainTex0) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
