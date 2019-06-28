Shader ".poiyomi/Toon/Default/outlines Transparent"
{
    Properties
    {
        [HideInInspector] shader_is_using_thry_editor ("", Float) = 0
        [HideInInspector] shader_master_label ("<color=#ff69b4>❤ Poiyomi Toon Shader V3.1 ❤</color>", Float) = 0
        [HideInInspector] shader_presets ("poiToonPresets", Float) = 0
        [HideInInspector] shader_eable_poi_settings_selection ("", Float) = 0
        
        [HideInInspector] footer_github ("linkButton(Github,https://github.com/poiyomi/PoiyomiToonShader)", Float) = 0
        [HideInInspector] footer_discord ("linkButton(Discord,https://discord.gg/Ays52PY)", Float) = 0
        [HideInInspector] footer_donate ("linkButton(Donate,https://www.paypal.me/poiyomi)", Float) = 0
        [HideInInspector] footer_patreon ("linkButton(Patreon,https://www.patreon.com/poiyomi)", Float) = 0
        
        [HideInInspector] m_mainOptions ("Main", Float) = 0
        _Color ("Color & Alpha--hover=Color modifies the tint of the main texture (MainTexture * Color). The alpha value also controls the overall alpha of the material when used in the transparent version of the shader.", Color) = (1, 1, 1, 1)
        _Saturation ("Saturation--hover=When set to negative 1 the main texture will be void of all color. If set to 1 the main texture will become more saturated in color. Desaturation is applied before Color so that color may be used more effectively.", Range(-1, 1)) = 0
        _MainTex ("Texture--hover=The base texture used for the material. The transparent values are used for Alpha cutoff.", 2D) = "white" { }
        [Normal]_BumpMap ("Normal Map--hover=A texture used to fake bumps of dents in a material. modifies how many things react on the surface of the material including light, reflections, rim lighting, etc...", 2D) = "bump" { }
        _BumpScale ("Normal Intensity--hover=Controls the strength of the normal map. Pushes the normals away from a straight out orientation.", Range(0, 10)) = 1
        _AlphaMask ("Alpha Mask--hover=contributes to the cutoff or transparency of a material. The Alpha mask can be used to make things transparent that would otherwise be seen.", 2D) = "white" { }
        _Clip ("Alpha Cuttoff--hover=If the Alpha/Opacity of the main texture is below that of the alpha cutoff it will be made invisible.", Range(0, 1.001)) = 0.5
        [HideInInspector] m_start_mainAdvanced ("Advanced", Float) = 0
        _GlobalPanSpeed ("Pan Speed XY--hover=Pans the UV values based on the X and Y values inputted. This will rotate all effects as this modifies the base UV that all things derive texture positions from. Excludes things that do not use the UV for texture look up such as panosphere.", Vector) = (0, 0, 0, 0)
        [Normal]_DetailNormalMap ("Detail Map--hover=A second normal map which can be used to add finder detail to a material. This is blended with the primary normal map.", 2D) = "bump" { }
        _DetailNormalMask ("Detail Mask--hover=Controls the strength of the detail map in specific locations (Detail Mask * Detal Map). Black = 0 White = 1", 2D) = "white" { }
        _DetailNormalMapScale ("Detail Intensity--hover=The strength of the detail map. Note: If the detail mask is 0 this will have no effect. (Detail normal * Detail Mask * Detail Intensity)", Range(0, 10)) = 1
        [HideInInspector] m_end_mainAdvanced ("Advanced", Float) = 0
        
        [HideInInspector] m_metallicOptions ("Metallic", Float) = 0
        _CubeMap ("Baked CubeMap--hover=This cubemap will be used as a fallback when a map does not have reflection probes or a skybox from which to draw reflections.", Cube) = "" { }
        [Toggle(_)]_SampleWorld ("Force Baked Cubemap--hover=This forces the use of the baked cubemap rather than attempting to use a cubemap within the present scene.", Range(0, 1)) = 0
        _PurelyAdditive ("Purely Additive--hover=Forces reflections to be purely additive rather than multiplicative. This can be used with very dark materials or cubemaps that are being used for glossy effects.", Range(0, 1)) = 0
        _MetallicMask ("Metallic Mask--hover=Controls the areas that are set to metallic.", 2D) = "white" { }
        _Metallic ("Metallic--hover=How metallic a material is. Generally set to 0 or 1, an in between value can represent something like plastic.", Range(0, 1)) = 0
        _SmoothnessMask ("Smoothness Mask--hover=How smooth specific parts of a material are.", 2D) = "white" { }
        [Toggle(_)]_InvertSmoothness ("Invert Smoothness Mask--hover=Some Smoothness Masks are actually roughness maps and may require inversion to function properly.", Range(0, 1)) = 0
        _Smoothness ("Smoothness--hover=How blurred the reflection becomes on a material. 0 = No reflection visible. 1 = Mirror like reflections.", Range(0, 1)) = 0
        
        [HideInInspector] m_matcapOptions ("Matcap / Sphere Textures", Float) = 0
        _Matcap ("Matcap--hover=matcap stands for “material capture” – it is a complete material including lighting and reflections so you can add it to an object and not have any need for, well, lighting and reflections.", 2D) = "white" { }
        _MatcapMask ("Matcap Map--hover=Where the Matcap texture will be blended with the base material. White: FullBlend, Black: No Blend", 2D) = "white" { }
        _MatcapColor ("Matcap Color--hover=Tints the matcap color.", Color) = (1, 1, 1, 1)
        _MatcapBrightness ("Matcap Brightness--hover=How Bright the matcap is. Values above 1 will be emissive. Note: When above 1 It will only glow in worlds that have bloom post processing.", Range(0, 20)) = 1
        _ReplaceWithMatcap ("Replace With Matcap--hover=A 0 to 1 slider when at 1 replaces the underlying color with the matcap entirely. A value between 0 and 1 is a mix of the base and matcap colors", Range(0, 1)) = 0
        _MultiplyMatcap ("Multiply Matcap--hover=Multiplies the base color by that of the matcap. Useful when you have a black and white matcap representing specular reflections.", Range(0, 1)) = 0
        _AddMatcap ("Add Matcap--hover=Simply adds the matcap to base material. Useful when the base is black and you just want to add color or shine.", Range(0, 1)) = 0
        
        [HideInInspector] m_outlineOptions ("Outlines", Float) = 0
        _LineWidth ("Outline Width--hover=The width of the outline in cm.", Float) = 0
        _LineColor ("Outline Color--hover=The color of the outline.", Color) = (1, 1, 1, 1)
        _OutlineEmission ("Outline Emission--hover=How much the outline should glow.", Float) = 0
        _OutlineTexture ("Outline Texture--hover=A texture that can be applied to the outline.", 2D) = "white" { }
        _OutlineTexturePan ("Outline Texture Pan--hover=Pan the outline texture along x or y axis", Vector) = (0, 0, 0, 0)
        _OutlineShadowStrength ("Shadow Strength--hover=How Dark the shadows will appear on the outlines.", Range(0, 1)) = 1
        [HideInInspector] m_start_outlineAdvanced ("Advanced", Float) = 0
        _OutlineFadeDistance ("Outline distance Fade--hover=The outline will fade in linearly from x distance to y distance. anything below x will be 100% transparent. This is useful when your camera may go inside your mesh and you don't want to see outlines from close range.", Vector) = (0, 0, 0, 0)

        [Enum(UnityEngine.Rendering.CullMode)] _OutlineCull ("Cull--hover=Controls which side of the mesh is rendered. Example: Back culling will not render the back side of the polygons.", Float) = 1
        [HideInInspector] m_end_outlineAdvanced ("Advanced", Float) = 0
        
        [HideInInspector] m_emissionOptions ("Emission", Float) = 0
        [HDR]_EmissionColor ("Emission Color--hover=The color of the emission or glow.", Color) = (1, 1, 1, 1)
        _EmissionMap ("Emission Map--hover=The color of the emission represented by a texture. Black will not glow at all.", 2D) = "white" { }
        _EmissionMask ("Emission Mask--hover=A mask of where emissions should take place. The mask should be in black and white. the emission color is multiplied by the mask. black = 0 white = 1", 2D) = "white" { }
        _EmissionPan ("Emission Pan--hover=The speed at which which emission is panned across the model. Only X and Y values are used.", Vector) = (0, 0, 0, 0)
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
        
        [HideInInspector] m_fakeLightingOptions ("Lighting", Float) = 0
        [Enum(Natural, 0, Controlled, 1)] _LightingType("Lighting Type--hover=How shadows are handled. | Natural: world controled shadow Color * shadow ramp, Controlled: The color of the shadow ramp is the color of the shadow period", Int) = 0
        [NoScaleOffset][Gradient]_ToonRamp ("Lighting Ramp--hover=How light will be applied to the model. The top right side of the texture is maximum light and the bottom left is minimum light. Make sure the lighting ramp texture settings are set to clamp to avoid bugs.", 2D) = "white" { }        _ShadowStrength ("Shadow Strength--hover=How much the lighting ramp will alter the material color. If 0 your material will simply take the direct lighting of your scene.", Range(0, 1)) = 1
        _ShadowOffset ("Shadow Offset--hover=Offsets the lighting ramp location so you can change where shadows start and stop", Range(-1, 1)) = 0
        _MinBrightness ("Min Brightness--hover=The minimum brightness your material may become. Use this if you want your avatar to never go below a set brightness so you will be visible in the dark.", Range(0, 1)) = 0
        _MaxBrightness ("Max Brightness--hover=The maximum Brightness your material will achieve. Limit this if you don't want your material to get too bright.", Float) = 1
        _AOMap ("AO Map--hover=An Ambient Occlusion (AO) map creates soft shadowing, as if the model was lit without a direct light source, like on a cloudy day.", 2D) = "white" { }
        _AOStrength ("AO Strength--hover=Controls the darkness of the shadows created by the AO map", Range(0, 1)) = 1
        [HideInInspector] m_start_lightingAdvanced ("Advanced", Float) = 0
        _IndirectContribution ("Indirect Contribution--hover=Pushes a material into shadow. Useful when a shadow ramp never goes full dark. This allows you to maintain a more neutral color in specific situations.", Range(0, 1)) = 0
        _AdditiveSoftness ("Additive Softness--hover=Adjusts how softly additive lights are applied. Smooths the edge of the light. 0 = hard edge .5 = 100% soft", Range(0, 0.5)) = 0.005
        _AdditiveOffset ("Additive Offset--hover=Offsets the additive lighting edge.", Range(-0.5, 0.5)) = 0
        _AttenuationMultiplier ("Attenuation--hover=Whether or not shadows can be casted onto the material from itself and other objects.", Range(0, 1)) = 0
        [HideInInspector] m_end_lightingAdvanced ("Advanced", Float) = 0
        
        [HideInInspector] m_specularHighlightsOptions ("Specular Highlights", Float) = 0
        [Enum(Off, 0, Realistic, 1, Toon, 2, soon.jpg, 3)] _SpecularType ("Specular Type--hover=How the specular highlights will be styled", Int) = 0
        _SpecularTint ("Specular Tint--hover=Tints the specular color upon reflection", Color) = (1, 1, 1, 1)
        _SpecularSmoothness ("Smoothness--hover=How smooth or shiny a material is. Changing this will change how light reflects off the material.", Range(0, 1)) = 0
        _SpecularMap ("Specular Map--hover=Adjusts the smoothness of an area by the texture. white = 1 smooth, black = 0 smooth", 2D) = "white" { }
        [Enum(Alpha, 0, Grayscale, 1)] _SmoothnessFrom ("Smoothness From--hover=What it derives the smoothness value from using the texture", Int) = 1
        _SpecularHighTexture ("Specular High Tex--hover=When there would be specular sample this texture instead", 2D) = "white" { }
        [Enum(Lighting, 0, HighTexture, 1)] _SpecularColorFrom("Specular Color From--hover=Whether or not the shader should derive specular color from the hight texture or the light color.", Int) = 0

        [HideInInspector] m_subsurfaceOptions ("Subsurface Scattering", Float) = 0
        _SSSColor ("Subsurface Color--hover=The internal color of the material. Light is tinted this color when it shines through you", Color) = (1, 1, 1, 1)
        _SSSThicknessMap ("Thickness Map--hover=How thicc the material is. Controls how difficult it is for light to pass through.", 2D) = "black" { }
        _SSSThicknessMod ("Thickness mod--hover=Modifies the thickness map. adds or subtracts x from the map.", Range(-1, 1)) = 0
        _SSSSCale ("Light Strength--hover=How much of the light passes through the material", Range(0, 1)) = 0
        _SSSPower ("Light Spread--hover=How far the light spreads out inside the mesh.", Range(1, 100)) = 1
        _SSSDistortion ("Light Distortion--hover=The angle at which the light is visible through the mesh. Higher distortion means the light will only show up along the edges.", Range(0, 1)) = 0

        [HideInInspector] m_rimLightOptions ("Rim Lighting", Float) = 0
        _RimLightColor ("Rim Color--hover=The tint applied to the rim lighting.", Color) = (1, 1, 1, 1)
        _RimWidth ("Rim Width--hover=The width of the rim effect on the material.", Range(0, 1)) = 0.8
        _RimSharpness ("Rim Sharpness--hover=How sharp the edge of the rim effect is. 0 = smooth gradient 1 = hard edge", Range(0, 1)) = .25
        _RimStrength ("Rim Emission--hover=How emissive or glowing the rim light is on the material.", Range(0, 20)) = 0
        _RimLightColorBias ("Rim Color Bias--hover=smoothly blends between the color of the model and the selected rim color. 0 = material base color 1 = rim light color useful if you want to use the colors of your model for rim lighting.", Range(0, 1)) = 0
        _RimTex ("Rim Texture--hover=The texture used for the color of the rim light. Note: Alpha is respected and can cause lighting to go away.", 2D) = "white" { }
        _RimMask ("Rim Mask--hover=Where the rim lighting should occur", 2D) = "white" { }
        _RimTexPanSpeed ("Rim Texture Pan Speed--hover=Pans the texture used for rim lighting. Only X and Y have an effect.", Vector) = (0, 0, 0, 0)
        [HideInInspector] m_start_ShadowMix ("Shadow Mix", Float) = 0
        _ShadowMix ("Shadow Mix In--hover=The brightness of the lighting now effects the width of the rimlight.", Range(0, 1)) = 0
        _ShadowMixThreshold ("Shadow Mix Threshold--hover=Adjust this so the shadowmix fits your shadow ramp threshold", Range(0, 1)) = .5
        _ShadowMixWidthMod ("Shadow Mix Width Mod--hover=Modifies how wide the rim lighting is in the light", Range(0, 10)) = .5
        [HideInInspector] m_end_ShadowMix ("Shadow Mix", Float) = 0
        
        [HideInInspector] m_flipBook ("Flipbook", Float) = 0
        _FlipbookTexture ("Texture--hover=The spritesheet used for the animation. You can use ezgif.com to convert gifs to spritesheets.", 2D) = "red" {}
        _FlipbookColor ("Color & alpha--hover=Tints the flipbook texture and controls the alpha", Color) = (1, 1, 1, 1)
        _FlipbookTotalFrames ("Total Frames--hover=Total number of frames to be used in the animation.", Int) = 1
        _FlipbookRows ("Rows--hover=Total number of rows in the spritesheet", Float) = 1.0
        _FlipbookColumns ("Columns--hover=Total number of columns used for the spritesheet", Float) = 1.0
        _FlipbookFPS ("FPS--hover=The framerate you want to use for the gif", Float) = 30.0
        _FlipbookScaleOffset ("Scale | Offset--hover= X,Y controls the animation scale, Z,W controls the offset.", Vector) = (1,1,0,0)
        _FlipbookEmissionStrength("Emission Strength--hover=How emissive the animation is.", Range(0,20)) = 0
        _FlipbookRotation("Rotation--hover=Rotate the animation in degrees around the center point", Range(0,360)) = 0
        [HideInInspector] m_start_manualFlipbookControl ("Manual Control", Float) = 0
        _FlipbookCurrentFrame ("Current Frame--hover= Manually control which frame is currently set, negative numbers won't do anything.", Float) = -1
        [HideInInspector] m_end_manualFlipbookControl ("Manual Control", Float) = 0

        [HideInInspector] m_StencilPassOptions ("Stencil", Float) = 0
        [IntRange] _StencilRef ("Stencil Reference Value--hover=The value to be compared against (if Comp is anything else than always) and/or the value to be written to the buffer (if either Pass, Fail or ZFail is set to replace). 0–255 integer.", Range(0, 255)) = 0
        //[IntRange] _StencilReadMaskRef ("Stencil ReadMask Value--hover=An 8 bit mask as an 0–255 integer, used when comparing the reference value with the contents of the buffer (referenceValue & readMask) comparisonFunction (stencilBufferValue & readMask). Default: 255.", Range(0, 255)) = 0
        //[IntRange] _StencilWriteMaskRef ("Stencil WriteMask Value--hover=An 8 bit mask as an 0–255 integer, used when writing to the buffer. Note that, like other write masks, it specifies which bits of stencil buffer will be affected by write (i.e. WriteMask 0 means that no bits are affected and not that 0 will be written). Default: 255.", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilPassOp ("Stencil Pass Op--hover=What to do with the contents of the buffer if the stencil test (and the depth test) passes. Default: keep.", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilFailOp ("Stencil Fail Op--hover=What to do with the contents of the buffer if the stencil test fails. Default: keep.", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFailOp ("Stencil ZFail Op--hover=What to do with the contents of the buffer if the stencil test passes, but the depth test fails. Default: keep.", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilCompareFunction ("Stencil Compare Function--hover=The function used to compare the reference value to the current contents of the buffer. Default: always.", Float) = 8
        
        [HideInInspector] m_start_OutlineStencil ("Outline Stencil", Float) = 0
        [IntRange] _OutlineStencilRef ("Stencil Reference Value--hover=The value to be compared against (if Comp is anything else than always) and/or the value to be written to the buffer (if either Pass, Fail or ZFail is set to replace). 0–255 integer.", Range(0, 255)) = 0
        //[IntRange] _OutlineStencilReadMaskRef ("Stencil ReadMask Value--hover=An 8 bit mask as an 0–255 integer, used when comparing the reference value with the contents of the buffer (referenceValue & readMask) comparisonFunction (stencilBufferValue & readMask). Default: 255.", Range(0, 255)) = 0
        //[IntRange] _OutlineStencilWriteMaskRef ("Stencil WriteMask Value--hover=An 8 bit mask as an 0–255 integer, used when writing to the buffer. Note that, like other write masks, it specifies which bits of stencil buffer will be affected by write (i.e. WriteMask 0 means that no bits are affected and not that 0 will be written). Default: 255.", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _OutlineStencilPassOp ("Stencil Pass Op--hover=What to do with the contents of the buffer if the stencil test (and the depth test) passes. Default: keep.", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _OutlineStencilFailOp ("Stencil Fail Op--hover=What to do with the contents of the buffer if the stencil test fails. Default: keep.", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _OutlineStencilZFailOp ("Stencil ZFail Op--hover=What to do with the contents of the buffer if the stencil test passes, but the depth test fails. Default: keep.", Float) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _OutlineStencilCompareFunction ("Stencil Compare Function--hover=The function used to compare the reference value to the current contents of the buffer. Default: always.", Float) = 8
        [HideInInspector] m_end_OutlineStencil ("Outline Stencil", Float) = 0
        
        [HideInInspector] m_funOptions ("Fun", Float) = 0
        [Enum(ShowInBoth, 0, ShowOnlyInMirror, 1, DontShowInMirror, 2)] _Mirror ("Show in mirror--hover=ShowInBoth: shows your material in both the mirror and the world. ShowOnlyInMirror: shows your material only in the mirror. DontShowInMirror: Only shows your avatar in the world and does not reflect in mirror.", Int) = 0
        
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
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        //Blend SrcAlpha OneMinusSrcAlpha
        Blend [_SourceBlend] [_DestinationBlend]
        
        Pass
        {
            Name "MainPass"
            Tags { "LightMode" = "ForwardBase" }
            
            Stencil
            {
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
            
            #pragma target 5.0
            #define TRANSPARENT
            #define FORWARD_BASE_PASS
            #pragma multi_compile_instancing
            #pragma multi_compile_fwdbase
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_fog
            #pragma vertex vert
            #pragma fragment frag
            #include "../Includes/PoiDefaultPass.cginc"
            ENDCG
            
        }
        Pass
        {
            Tags { "LightMode" = "ForwardAdd" }
            Stencil
            {
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
            
            #pragma target 5.0
            #pragma multi_compile_instancing
            #define FORWARD_ADD_PASS
            #pragma multi_compile_fwdadd_fullshadows
            #define TRANSPARENT
            #pragma vertex vert
            #pragma fragment frag
            #include "../Includes/PoiDefaultPass.cginc"
            ENDCG
            
        }
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "ForwardBase" }
            Stencil
            {
                Ref [_OutlineStencilRef]
                Comp [_OutlineStencilCompareFunction]
                Pass [_OutlineStencilPassOp]
                Fail [_OutlineStencilFailOp]
                ZFail [_OutlineStencilZFailOp]
            }
            ZTest [_ZTest]
            ZWrite [_ZWrite]
            Cull [_OutlineCull]
            CGPROGRAM
            
            #pragma target 5.0
            #define TRANSPARENT
            #pragma multi_compile_instancing
            #pragma vertex vert
            #pragma fragment frag
            #include "../Includes/PoiPassOutline.cginc"
            ENDCG
            
        }
        Pass
        {
            Tags { "LightMode" = "ShadowCaster" }
            Stencil
            {
                Ref [_StencilRef]
                Comp [_StencilCompareFunction]
                Pass [_StencilPassOp]
                Fail [_StencilFailOp]
                ZFail [_StencilZFailOp]
            }
            CGPROGRAM
            
            #pragma target 5.0
            #define TRANSPARENT
            #pragma multi_compile_instancing
            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster
            #include "../Includes/PoiPassShadow.cginc"
            ENDCG
            
        }
    }
}
