Shader ".poiyomi/Drag and Drop/Cutout"
{
    Properties 
  { 
      [HideInInspector] shader_is_using_thry_editor("", Float)=0
        [HideInInspector] shader_master_label ("<color=#008080>❤ Poiyomi Toon Shader V3.0 ❤</color>", Float) = 0
        [HideInInspector] shader_presets ("poiToonPresets", Float) = 0
        [HideInInspector] shader_eable_poi_settings_selection("", Float) = 0
        
		[HideInInspector] footer_github("linkButton(Github,https://github.com/poiyomi/PoiyomiToonShader)", Float) = 0
		[HideInInspector] footer_discord("linkButton(Discord,https://discord.gg/Ays52PY)", Float) = 0
		[HideInInspector] footer_donate("linkButton(Donate,https://www.paypal.me/poiyomi)", Float) = 0
		[HideInInspector] footer_patreon("linkButton(Patreon,https://www.patreon.com/poiyomi)", Float) = 0

        [HideInInspector] m_mainOptions ("Main", Float) = 0
        _Color ("Color--hover=Color modifies the tint of the main texture (MainTexture * Color). The alpha value also controls the overall alpha of the material when used in the transparent version of the shader.", Color) = (1, 1, 1, 1)
        _Desaturation ("Desaturation--hover=When set to 1 the main texture will be void of all color. If set to negative 1 the main texture will become more saturated in color. Desaturation is applied before Color so that color may be used more effectively.", Range(-1, 1)) = 0
        _MainTex ("Texture--hover=The base texture used for the material. The transparent values are used for Alpha cutoff.", 2D) = "white" { }
        _AlphaMask ("Alpha Mask--hover=contributes to the cutoff or transparency of a material. The Alpha mask can be used to make things transparent that would otherwise be seen.", 2D) = "white" { }
        _AOMap ("AO Map--hover=An Ambient Occlusion (AO) map creates soft shadowing, as if the model was lit without a direct light source, like on a cloudy day.", 2D) = "white" { }
        _AOStrength ("AO Strength--hover=Controls the darkness of the shadows created by the AO map", Range(0, 1)) = 1
        _Clip ("Alpha Cuttoff--hover=If the Alpha/Opacity of the main texture is below that of the alpha cutoff it will be made invisible.", Range(0, 1.001)) = 0.5

        [HideInInspector] m_emissionOptions ("Emission", Float) = 0
        [HDR]_EmissionColor ("Emission Color--hover=The color of the emission or glow.", Color) = (1, 1, 1, 1)
        _EmissionMap ("Emission Map--hover=The color of the emission represented by a texture. Black will not glow at all.", 2D) = "white" { }
        _EmissionMask ("Emission Mask--hover=A mask of where emissions should take place. The mask should be in black and white. the emission color is multiplied by the mask. black = 0 white = 1", 2D) = "white" { }
        _EmissionScrollSpeed ("Emission Scroll Speed--hover=The speed at which which emission is panned across the model. Only X and Y values are used.", Vector) = (0, 0, 0, 0)
        _EmissionStrength ("Emission Strength--hover=The strength of the glow or emission. (Emission * Emission Strength)", Range(0, 20)) = 0
        
        [HideInInspector] m_start_blinkingEmissionOptions ("Blinking Emission", Float) = 0
        _EmissiveBlink_Min ("Emissive Blink Min--hover=The minimum value the emission will be multiplied by when blinking.", Float) = 1
        _EmissiveBlink_Max ("Emissive Blink Max--hover=The maximum value the emission will be multiplied by when blinking.", Float) = 1
        _EmissiveBlink_Velocity ("Emissive Blink Velocity--hover=the rate at which emission will travel between the min and max blinking emission values.", Float) = 4
        [HideInInspector] m_end_blinkingEmissionOptions ("Blinking Emission", Float) = 0
        
        [HideInInspector] m_start_scrollingEmissionOptions ("Scrolling Emission", Float) = 0
        [Toggle(_)] _ScrollingEmission ("Enable Scrolling Emission--hover=Enables a wave of a emission to travel across all emissive areas of a material.", Float) = 0
        _EmissiveScroll_Direction ("Emissive Scroll Direction--hover=The direction the emissive wave will travel over the material.", Vector) = (0, -10, 0, 0)
        _EmissiveScroll_Width ("Emissive Scroll Width--hover=The width of the emission wave.", Float) = 10
        _EmissiveScroll_Velocity ("Emissive Scroll Velocity--hover=The velocity at which the wave travels across the material.", Float) = 10
        _EmissiveScroll_Interval ("Emissive Scroll Interval--hover=The interval at which the wave will appear.", Float) = 20
        [HideInInspector] m_end_scrollingEmissionOptions ("Scrolling Emission", Float) = 0
        
        [HideInInspector] m_miscOptions ("Misc", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull--hover=Controls which sides of polygons should be culled (not drawn) Back: Don’t render polygons facing away from the viewer (default). Front: Don’t render polygons facing towards the viewer. Used for turning objects insideout. Off: Disables culling all faces are drawn. Used for special effects.", Float) = 2
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest--hover=How should depth testing be performed. Default is LEqual (draw objects in from or at the distance as existing objects; hide objects behind them).", Float) = 4
        [Enum(UnityEngine.Rendering.BlendMode)] _SourceBlend ("Source Blend--hover=When graphics are rendered, after all Shaders have executed and all Textures have been applied, the pixels are written to the screen. How they are combined with what is already there is controlled by the Blend command.", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DestinationBlend ("Destination Blend--hover=When graphics are rendered, after all Shaders have executed and all Textures have been applied, the pixels are written to the screen. How they are combined with what is already there is controlled by the Blend command.", Float) = 10
        [Enum(Off, 0, On, 1)] _ZWrite ("ZWrite--hover=Controls whether pixels from this object are written to the depth buffer (default is On). If you’re drawng solid objects, leave this on. If you’re drawing semitransparent effects, switch to ZWrite Off. For more details read below.", Int) = 1
    }
    
    //originalEditorCustomEditor "PoiToon"
CustomEditor "ThryEditor"
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        
        Pass
        {
            Name "MainPass"
            Tags { "LightMode" = "ForwardBase" }
            
            Stencil
            {
                Ref [_StencilRef]
                ReadMask [_StencilReadMaskRef]
                WriteMask [_StencilWriteMaskRef]
                Ref [_StencilRef]
                Comp [_StencilCompareFunction]
                Pass [_StencilPassOp]
                Fail [_StencilFailOp]
                ZFail [_StencilZFailOp]
            }
            ZWrite [_ZWrite] 
            Cull [_Cull]
            ZTest [_ZTest]
            CGPROGRAM
            #pragma target 3.0
            #define DRAG_N_DROP
            #define FORWARD_BASE_PASS
            #pragma multi_compile_instancing
            #pragma multi_compile_fwdbase
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_fog
            #pragma vertex vert
            #pragma fragment frag
            #include "../Includes/PoiPass.cginc"
            ENDCG
        }
        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }
            Stencil
            {
                Ref [_StencilRef]
                ReadMask [_StencilReadMaskRef]
                WriteMask [_StencilWriteMaskRef]
                Ref [_StencilRef]
                Comp [_StencilCompareFunction]
                Pass [_StencilPassOp]
                Fail [_StencilFailOp]
                ZFail [_StencilZFailOp]
            }
            ZWrite Off  
            Blend One One
            Cull [_Cull]
            ZTest [_ZTest]
            CGPROGRAM
            #pragma target 3.0
            #define DRAG_N_DROP
            #define FORWARD_ADD_PASS
            #pragma multi_compile_instancing
            #pragma multi_compile_fwdadd_fullshadows
            #pragma vertex vert
            #pragma fragment frag
            #include "../Includes/PoiPass.cginc"
            ENDCG
        }
        Pass
        {
            Tags { "LightMode" = "ShadowCaster" }
            Stencil
            {
                Ref [_StencilRef]
                ReadMask [_StencilReadMaskRef]
                WriteMask [_StencilWriteMaskRef]
                Ref [_StencilRef]
                Comp [_StencilCompareFunction]
                Pass [_StencilPassOp]
                Fail [_StencilFailOp]
                ZFail [_StencilZFailOp]
            }
            CGPROGRAM
            #pragma target 3.0
            #pragma multi_compile_instancing
            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster
            #include "../Includes/PoiPassShadow.cginc"
            ENDCG
        }
    }
}
