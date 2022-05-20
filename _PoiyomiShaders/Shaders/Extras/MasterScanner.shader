Shader ".poiyomi/Extras/MasterScanner"
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
    }
    CustomEditor "ThryEditor"
    SubShader
    {
        
        Tags { "Queue" = "background" }
        ColorMask 0
        ZWrite Off
        Cull Off
        ZTest Always
        Stencil
        {
            Ref 901
            Comp Always
            Pass Replace
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