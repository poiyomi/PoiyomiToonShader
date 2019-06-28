Shader ".poiyomi/Toon/Extras/StencilInvis"
{
    properties
    {
		[HideInInspector] m_StencilPassOptions ("Stencil", Float) = 0
        [IntRange] _StencilRef ("Stencil Reference Value--hover=The value to be compared against (if Comp is anything else than always) and/or the value to be written to the buffer (if either Pass, Fail or ZFail is set to replace). 0–255 integer.", Range(0, 255)) = 0
        //[IntRange] _StencilReadMaskRef ("Stencil ReadMask Value--hover=An 8 bit mask as an 0–255 integer, used when comparing the reference value with the contents of the buffer (referenceValue & readMask) comparisonFunction (stencilBufferValue & readMask). Default: 255.", Range(0, 255)) = 0
        //[IntRange] _StencilWriteMaskRef ("Stencil WriteMask Value--hover=An 8 bit mask as an 0–255 integer, used when writing to the buffer. Note that, like other write masks, it specifies which bits of stencil buffer will be affected by write (i.e. WriteMask 0 means that no bits are affected and not that 0 will be written). Default: 255.", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilPassOp ("Stencil Pass Op--hover=What to do with the contents of the buffer if the stencil test (and the depth test) passes. Default: keep.", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilFailOp ("Stencil Fail Op--hover=What to do with the contents of the buffer if the stencil test fails. Default: keep.", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFailOp ("Stencil ZFail Op--hover=What to do with the contents of the buffer if the stencil test passes, but the depth test fails. Default: keep.", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilCompareFunction ("Stencil Compare Function--hover=The function used to compare the reference value to the current contents of the buffer. Default: always.", Float) = 8
        
        [HideInInspector] m_miscOptions ("Misc", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull--hover=Controls which sides of polygons should be culled (not drawn) Back: Don’t render polygons facing away from the viewer (default). Front: Don’t render polygons facing towards the viewer. Used for turning objects insideout. Off: Disables culling all faces are drawn. Used for special effects.", Float) = 2
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest--hover=How should depth testing be performed. Default is LEqual (draw objects in from or at the distance as existing objects; hide objects behind them).", Float) = 4
        [Enum(UnityEngine.Rendering.BlendMode)] _SourceBlend ("Source Blend--hover=When graphics are rendered, after all Shaders have executed and all Textures have been applied, the pixels are written to the screen. How they are combined with what is already there is controlled by the Blend command.", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DestinationBlend ("Destination Blend--hover=When graphics are rendered, after all Shaders have executed and all Textures have been applied, the pixels are written to the screen. How they are combined with what is already there is controlled by the Blend command.", Float) = 10
        [Enum(Off, 0, On, 1)] _ZWrite ("ZWrite--hover=Controls whether pixels from this object are written to the depth buffer (default is On). If you’re drawng solid objects, leave this on. If you’re drawing semitransparent effects, switch to ZWrite Off. For more details read below.", Int) = 1
    }
    CustomEditor "ThryEditor"
    SubShader
    {
        
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        ColorMask 0
        ZWrite [_ZWrite]
        Cull [_Cull]
        ZTest [_ZTest]
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