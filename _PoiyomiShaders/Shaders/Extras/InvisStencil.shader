Shader ".poiyomi/Extras/StencilInvis"
{
    properties
    {
		[HideInInspector] m_StencilPassOptions ("Stencil", Float) = 0
        [IntRange] _StencilRef ("Stencil Reference Value", Range(0, 255)) = 0
        //[IntRange] _StencilReadMaskRef ("Stencil ReadMask Value", Range(0, 255)) = 0
        //[IntRange] _StencilWriteMaskRef ("Stencil WriteMask Value", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilPassOp ("Stencil Pass Op", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilFailOp ("Stencil Fail Op", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFailOp ("Stencil ZFail Op", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilCompareFunction ("Stencil Compare Function", Float) = 8
        
        [HideInInspector] m_renderingOptions ("Rendering Options", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4
        [Enum(UnityEngine.Rendering.BlendMode)] _SourceBlend ("Source Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DestinationBlend ("Destination Blend", Float) = 10
        [Enum(Off, 0, On, 1)] _ZWrite ("ZWrite", Int) = 1
    }
    CustomEditor "ThryEditor"
    SubShader
    {
        
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        ColorMask 0
        ZWrite [_ZWrite]
        Cull [_Cull]
        ZTest [_ZTest]
            Offset [_ZBias], [_ZBias]
        Stencil
        {
            Ref [_StencilRef]
            Comp [_StencilCompareFunction]
            Pass [_StencilPassOp]
            Fail [_StencilFailOp]
            ZFail [_StencilZFailOp]
        }
        
        CGINCLUDE
        struct appdata
        {
            float4 vertex: POSITION;
        };
        struct v2f
        {
            float4 pos: SV_POSITION;
        };
        v2f vert(appdata v)
        {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            return o;
        }
        half4 frag(v2f i): COLOR
        {
            return half4(1, 1, 0, 1);
        }
        ENDCG
        
        Pass
        {
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
            
        }
    }
}