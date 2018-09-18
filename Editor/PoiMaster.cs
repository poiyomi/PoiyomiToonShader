using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PoiMaster : ShaderGUI
{
    private static class Styles
    {
        // sections
        public static GUIContent MiscSection = new GUIContent("Misc", "Miscellaneous settings");
        public static GUIContent MainSection = new GUIContent("Main", "Main configuration section");
        public static GUIContent OutlineSection = new GUIContent("Outline", "Outline configuration section");
        public static GUIContent RGBMaskSection = new GUIContent("RGB Mask", ""); // TODO:
        public static GUIContent EmissionSection = new GUIContent("Emission", "Configuration for emissiveness");
        public static GUIContent EmissiveBlinkSection = new GUIContent("Emissive Blink (requires emission)", "Configuration for emissive blinking");
        public static GUIContent EmissiveScrollSection = new GUIContent("Emissive Scroll (requires emission)", "Configuration for emissive scrolling");
        public static GUIContent FakeLightingSection = new GUIContent("Fake Lighting", "Simulates a local light if the world lacks one");
        public static GUIContent SpecularHighlightsSection = new GUIContent("Specular Highlights (requires fake lighting)", ""); // TODO:
        public static GUIContent RimLightSection = new GUIContent("Rim Light", ""); // TODO:
        public static GUIContent BlendSection = new GUIContent("2 Texture Blending", ""); // TODO:
        public static GUIContent AutoBlendSection = new GUIContent("Automatic Blending (requires blending)", ""); // TODO:
        public static GUIContent TextureOverlaySection = new GUIContent("Texture Overlays", "Overlay up to 3 textures"); // TODO:

        // no section
        public static GUIContent CullMode = new GUIContent("Cull Mode", "Controls which face of the mesh is rendered \nOff = Double sided \nFront = Single sided (reverse) \nBack = Single sided");
        public static GUIContent Lit = new GUIContent("Lit", "Toggles environmental lighting");
        public static GUIContent ZTest = new GUIContent("ZTest");
        public static GUIContent Clip = new GUIContent("Clip", "Transparency treshold");

        // main section
        public static GUIContent MainTex = new GUIContent("Main Tex", "Main texture for the shader");
        public static GUIContent Color = new GUIContent("Color", "Color used for tinting the main texture");
        public static GUIContent NormalMap = new GUIContent("Normal Map", "Bump map");

        // outlines
        public static GUIContent LineWidth = new GUIContent("Line Width", "Width of the outline");
        public static GUIContent OutlineColor = new GUIContent("Outline Color", "Tint of the outline");
        public static GUIContent OutlineEmission = new GUIContent("Outline Emission", "How much emission glows");
        public static GUIContent OutlineTexture = new GUIContent("Outline Texture", "Texture for the outline");
        public static GUIContent Speed = new GUIContent("Speed", "speed of texture scrolling on outline");

        // rgb masking
        public static GUIContent RGBMask = new GUIContent("RGB Mask", "Mask Used for rgb masking");
        public static GUIContent RedTexture = new GUIContent("Red", "Red color layer");
        public static GUIContent GreenTexture = new GUIContent("Green", "Green color layer");
        public static GUIContent BlueTexture = new GUIContent("Blue", "Blue color layer");

        // emission
        public static GUIContent EmissionColor = new GUIContent("Color", "Color used for emission");
        public static GUIContent EmissionMap = new GUIContent("Map", "Texture used for emission");
        public static GUIContent EmissionStrength = new GUIContent("Strength", "Strength multiplier for emission");

        // emissive blink
        public static GUIContent EmissiveBlinkMin = new GUIContent("Min", "Minimum value used for emissive blink");
        public static GUIContent EmissiveBlinkMax = new GUIContent("Max", "Maximum value used for emissive blink");
        public static GUIContent EmissiveBlinkVelocity = new GUIContent("Velocity", "Blinking speed");

        // emissive scroll
        public static GUIContent EmissiveScrollDirection = new GUIContent("Direction", "Emissive scrolling direction");
        public static GUIContent EmissiveScrollWidth = new GUIContent("Width", "Emissive scrolling width");
        public static GUIContent EmissiveScrollVelocity = new GUIContent("Velocity", "Emissive scrolling speed");
        public static GUIContent EmissiveScrollInterval = new GUIContent("Interval", "Delay between emissive scrolls");

        // fake lighting
        public static GUIContent LightingGradient = new GUIContent("Lighting Ramp", "Texture the fake light uses for tinting");
        public static GUIContent LightingNormalMultiplier = new GUIContent("Ramp Strength", "Strength of the ramp used for tinting");
        public static GUIContent LightingShadowStrength = new GUIContent("Shadow Strength", "Shadow intensity");
        public static GUIContent LightingDirection = new GUIContent("Light Direction", "Direction towards which the light will cast shadows");

        // specular
        public static GUIContent SpecularMap = new GUIContent("Map", ""); // TODO: all of these vvv
        public static GUIContent SpecularColor = new GUIContent("Color", "");
        public static GUIContent SpecularRamp = new GUIContent("Ramp", "");
        public static GUIContent SpecularBias = new GUIContent("Bias", "");
        public static GUIContent SpecularStrength = new GUIContent("Strength", "");
        public static GUIContent SpecularSize = new GUIContent("Size", "");
        public static GUIContent SpecularNormalMultiplier = new GUIContent("Normal Multiplier", "");

        // rim glow
        public static GUIContent RimGlow = new GUIContent("Rim Type", ""); 
        public static GUIContent InvertRimLighting = new GUIContent("Invert Rim Lighting", ""); 
        public static GUIContent RimColor = new GUIContent("Color", "");
        public static GUIContent RimGlowStrength = new GUIContent("Glow Strength", "");
        public static GUIContent RimWidth = new GUIContent("Width", "");
        public static GUIContent RimColorBias = new GUIContent("Color Bias", "");
        public static GUIContent RimTextureTile = new GUIContent("Texture Tile", "");
        public static GUIContent RimTexture = new GUIContent("Texture", "");
        public static GUIContent RimTexturePanSpeed = new GUIContent("Texture Pan Speed", "");
        public static GUIContent RimGlowNormalMultiplier = new GUIContent("Normal Multiplier", "");

        public static GUIContent BlendType = new GUIContent("Type", "");
        public static GUIContent BlendColor = new GUIContent("Color", "");
        public static GUIContent BlendTexture = new GUIContent("Texture", "");
        public static GUIContent BlendNoiseTexture = new GUIContent("Noise Texture", "");
        public static GUIContent BlendAlpha = new GUIContent("Alpha", "");
        public static GUIContent BlendTiling = new GUIContent("Tiling", "");

        public static GUIContent AutoBlendSpeed = new GUIContent("Speed", "");
        public static GUIContent AutoBlendDelay = new GUIContent("Delay", "");

        public static GUIContent NumOverlayTextures = new GUIContent("Num Overlay Textures", "");
        public static GUIContent OverlayColor1 = new GUIContent("Overlay Color 1", "");
        public static GUIContent OverlayTexture1 = new GUIContent("Overlay Texture 1", "");
        public static GUIContent OverlayTexture1Velocity = new GUIContent("Overlay Texture 1 Velocity", "");
        public static GUIContent OverlayColor2 = new GUIContent("Overlay Color 2", "");
        public static GUIContent OverlayTexture2 = new GUIContent("Overlay Texture 2", "");
        public static GUIContent OverlayTexture2Velocity = new GUIContent("Overlay Texture 2 Velocity", "");
        public static GUIContent OverlayColor3 = new GUIContent("Overlay Color 3", "");
        public static GUIContent OverlayTexture3 = new GUIContent("Overlay Texture 3", "");
        public static GUIContent OverlayTexture3Velocity = new GUIContent("Overlay Texture 3 Velocity", "");

    }

    GUIStyle m_sectionStyle;

    MaterialProperty m_cullMode = null;
    MaterialProperty m_lit = null;
    MaterialProperty m_zTest = null;
    MaterialProperty m_clip = null;

    MaterialProperty m_mainTex = null;
    MaterialProperty m_color = null;
    MaterialProperty m_normalMap = null;

    MaterialProperty m_lineWidth;
    MaterialProperty m_outlineColor;
    MaterialProperty m_outlineEmission;
    MaterialProperty m_outlineTexture;
    MaterialProperty m_speed;

    MaterialProperty m_rgbMask = null;
    MaterialProperty m_redTexture = null;
    MaterialProperty m_greenTexture = null;
    MaterialProperty m_blueTexture = null;

    MaterialProperty m_emissionColor = null;
    MaterialProperty m_emissionMap = null;
    MaterialProperty m_emissionStrength = null;

    MaterialProperty m_emissiveBlinkMin = null;
    MaterialProperty m_emissiveBlinkMax = null;
    MaterialProperty m_emissiveBlinkVelocity = null;

    MaterialProperty m_emissiveScrollDirection = null;
    MaterialProperty m_emissiveScrollWidth = null;
    MaterialProperty m_emissiveScrollVelocity = null;
    MaterialProperty m_emissiveScrollInterval = null;

    MaterialProperty m_lightingGradient = null;
    MaterialProperty m_lightingNormalMultiplier = null;
    MaterialProperty m_lightingShadowStrength = null;
    MaterialProperty m_lightingDirection = null;

    MaterialProperty m_specularMap = null;
    MaterialProperty m_specularColor = null;
    MaterialProperty m_specularRamp = null;
    MaterialProperty m_specularBias = null;
    MaterialProperty m_specularStrength = null;
    MaterialProperty m_specularSize = null;
    MaterialProperty m_specularNormalMultiplier = null;

    MaterialProperty m_rimGlow = null;
    MaterialProperty m_invertRimLighting = null;
    MaterialProperty m_rimColor = null;
    MaterialProperty m_rimGlowStrength = null;
    MaterialProperty m_rimWidth = null;
    MaterialProperty m_rimColorBias = null;
    MaterialProperty m_rimTexTile = null;
    MaterialProperty m_rimTex = null;
    MaterialProperty m_rimTexPanSpeed = null;
    MaterialProperty m_rimGlowNormalMultiplier = null;

    MaterialProperty m_blendType = null;
    MaterialProperty m_blendTexColor = null;
    MaterialProperty m_blendTex = null;
    MaterialProperty m_blendNoiseTex = null;
    MaterialProperty m_blendAlpha = null;
    MaterialProperty m_blendTiling = null;
    MaterialProperty m_autoBlendSpeed = null;
    MaterialProperty m_autoBlendDelay = null;

    MaterialProperty m_numOverlayTextures = null;
    MaterialProperty m_OverlayColor1 = null;
    MaterialProperty m_overlayTexture1 = null;
    MaterialProperty m_overlayTexture1Velocity =null;
    MaterialProperty m_OverlayColor2 = null;
    MaterialProperty m_overlayTexture2 = null;
    MaterialProperty m_overlayTexture2Velocity =null;
    MaterialProperty m_OverlayColor3 = null;
    MaterialProperty m_overlayTexture3 = null;
    MaterialProperty m_overlayTexture3Velocity =null;

    static bool m_miscOptions;
    static bool m_mainOptions;
    static bool m_outlineOptions;
    static bool m_rgbMaskOptions;
    static bool m_emissionOptions;
    static bool m_emissiveBlinkOptions;
    static bool m_emissiveScrollOptions;
    static bool m_fakeLightingOptions;
    static bool m_specularHighlightsOptions;
    static bool m_rimLightOptions;
    static bool m_blendOptions;
    static bool m_autoBlendOptions;
    static bool m_textureOverlayOptions;

    static bool m_rgbMaskEnabled = false;
    static bool m_useUv2Enabled = false;
    static bool m_emissionEnabled = false;
    static bool m_emissiveBlinkEnabled = false;
    static bool m_emissiveScrollEnabled = false;
    static bool m_fakeLightingEnabled = false;
    static bool m_specularHighlightsEnabled = false;
    static bool m_autoBlendEnabled = false;

    private void FindProperties(MaterialProperty[] props)
    {
        m_cullMode = FindProperty("_Cull", props);
        m_lit = FindProperty("_Lit", props);
        m_zTest = FindProperty("_ZTest", props);
        m_clip = FindProperty("_Clip", props);

        m_mainTex = FindProperty("_MainTex", props);
        m_color = FindProperty("_Color", props);
        m_normalMap = FindProperty("_NormalMap", props);

        m_lineWidth = FindProperty("_LineWidth", props);
        m_outlineColor = FindProperty("_LineColor", props);
        m_outlineEmission = FindProperty("_OutlineEmission", props);
        m_outlineTexture = FindProperty("_OutlineTexture", props);
        m_speed = FindProperty("_Speed", props);

        m_rgbMask = FindProperty("_RGBMask", props);
        m_redTexture = FindProperty("_RedTexture", props);
        m_greenTexture = FindProperty("_GreenTexture", props);
        m_blueTexture = FindProperty("_BlueTexture", props);

        m_emissionColor = FindProperty("_EmissionColor", props);
        m_emissionMap = FindProperty("_EmissionMap", props);
        m_emissionStrength = FindProperty("_EmissionStrength", props);

        m_emissiveBlinkMin = FindProperty("_EmissiveBlink_Min", props);
        m_emissiveBlinkMax = FindProperty("_EmissiveBlink_Max", props);
        m_emissiveBlinkVelocity = FindProperty("_EmissiveBlink_Velocity", props);

        m_emissiveScrollDirection = FindProperty("_EmissiveScroll_Direction", props);
        m_emissiveScrollWidth = FindProperty("_EmissiveScroll_Width", props);
        m_emissiveScrollVelocity = FindProperty("_EmissiveScroll_Velocity", props);
        m_emissiveScrollInterval = FindProperty("_EmissiveScroll_Interval", props);

        m_lightingGradient = FindProperty("_LightingGradient", props);
        m_lightingNormalMultiplier = FindProperty("_LightingNormalMultiplier", props);
        m_lightingShadowStrength = FindProperty("_ShadowStrength", props);
        m_lightingDirection = FindProperty("_LightDirection", props);

        m_specularMap = FindProperty("_SpecularMap", props);
        m_specularColor = FindProperty("_SpecularColor", props);
        m_specularRamp = FindProperty("_SpecularRamp", props);
        m_specularBias = FindProperty("_SpecularBias", props);
        m_specularStrength = FindProperty("_SpecularStrength", props);
        m_specularSize = FindProperty("_SpecularSize", props);
        m_specularNormalMultiplier = FindProperty("_SpecularNormalMultiplier", props);

        m_rimGlow = FindProperty("_RimGlow", props);
        m_invertRimLighting = FindProperty("_INVERT_RIM", props);
        m_rimColor = FindProperty("_RimColor", props);
        m_rimGlowStrength = FindProperty("_RimGlowStrength", props);
        m_rimWidth = FindProperty("_RimWidth", props);
        m_rimColorBias = FindProperty("_RimColorBias", props);
        m_rimTexTile = FindProperty("_RimTexTile", props);
        m_rimTex = FindProperty("_RimTex", props);
        m_rimTexPanSpeed = FindProperty("_RimTexPanSpeed", props);
        m_rimGlowNormalMultiplier = FindProperty("_RimGlowNormalMultiplier", props);

        m_blendType = FindProperty("_Blend", props);
        m_blendTexColor = FindProperty("_BlendTextureColor", props);
        m_blendTex = FindProperty("_BlendTexture", props);
        m_blendNoiseTex = FindProperty("_BlendNoiseTexture", props);
        m_blendAlpha = FindProperty("_BlendAlpha", props);
        m_blendTiling = FindProperty("_BlendTiling", props);

        m_autoBlendSpeed = FindProperty("_AutoBlendSpeed", props);
        m_autoBlendDelay = FindProperty("_AutoBlendDelay", props);

        m_numOverlayTextures = FindProperty("_OVERLAY", props);   
        m_OverlayColor1 = FindProperty("_OverlayColor1", props);   
        m_overlayTexture1 = FindProperty("_OverlayTexture1", props);
        m_overlayTexture1Velocity= FindProperty("_Tex1Velocity", props);
        m_OverlayColor2 = FindProperty("_OverlayColor2", props);   
        m_overlayTexture2 = FindProperty("_OverlayTexture2", props);
        m_overlayTexture2Velocity = FindProperty("_Tex2Velocity", props);
        m_OverlayColor3 = FindProperty("_OverlayColor3", props);   
        m_overlayTexture3 = FindProperty("_OverlayTexture3", props);
        m_overlayTexture3Velocity= FindProperty("_Tex3Velocity", props);
    }

    private void SetupStyle()
    {
        m_sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        m_sectionStyle.alignment = TextAnchor.MiddleCenter;
    }

    private void ToggleDefine(Material mat, string define, bool state)
    {
        if (state)
        {
            mat.EnableKeyword(define);
        }
        else
        {
            mat.DisableKeyword(define);
        }
    }
    void ToggleDefines(Material mat)
    {
        ToggleDefine(mat, "_RGB_MASK", m_rgbMaskEnabled);
        ToggleDefine(mat, "_USE_UV2", m_useUv2Enabled);
        ToggleDefine(mat, "_EMISSION", m_emissionEnabled);
        ToggleDefine(mat, "_BLINKING_EMISSION", m_emissiveBlinkEnabled);
        ToggleDefine(mat, "_SCROLLING_EMISSION", m_emissiveScrollEnabled);
        ToggleDefine(mat, "_FAKE_LIGHTING", m_fakeLightingEnabled);
        ToggleDefine(mat, "_SPECULAR_HIGHLIGHTS", m_specularHighlightsEnabled);
        ToggleDefine(mat, "_AUTO_BLEND", m_autoBlendEnabled);
    }

    void LoadDefaults(Material mat)
    {
        m_rgbMaskEnabled = mat.IsKeywordEnabled("_RGB_MASK");
        m_useUv2Enabled = mat.IsKeywordEnabled("_USE_UV2");
        m_emissionEnabled = mat.IsKeywordEnabled("_EMISSION");
        m_emissiveBlinkEnabled = mat.IsKeywordEnabled("_BLINKING_EMISSION");
        m_emissiveScrollEnabled = mat.IsKeywordEnabled("_SCROLLING_EMISSION");
        m_fakeLightingEnabled = mat.IsKeywordEnabled("_FAKE_LIGHTING");
        m_specularHighlightsEnabled = mat.IsKeywordEnabled("_SPECULAR_HIGHLIGHTS");
        m_autoBlendEnabled = mat.IsKeywordEnabled("_AUTO_BLEND");
    }

    void DrawHeader(ref bool enabled, ref bool options, GUIContent name)
    {
        var r = EditorGUILayout.BeginHorizontal("box");
        enabled = EditorGUILayout.Toggle(enabled, EditorStyles.radioButton, GUILayout.MaxWidth(15.0f));
        options = GUI.Toggle(r, options, GUIContent.none, new GUIStyle());
        EditorGUILayout.LabelField(name, m_sectionStyle);
        EditorGUILayout.EndHorizontal();
    }

    void DrawMasterLabel()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.richText = true;
        style.alignment = TextAnchor.MiddleCenter;

        EditorGUILayout.LabelField("<size=18><color=#de0653>Poiyomi's Master Shader</color></size>", style, GUILayout.MaxHeight(50));
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        Material material = materialEditor.target as Material;

        // map shader properties to script variables
        FindProperties(props);

        // set up style for the base look
        SetupStyle();

        // load default toggle values
        LoadDefaults(material);

        DrawMasterLabel();



        // main section
        m_mainOptions = GUI.Toggle(EditorGUILayout.BeginHorizontal("box"), m_mainOptions, GUIContent.none, "box");
        EditorGUILayout.Toggle(m_mainOptions, EditorStyles.foldout, GUILayout.MaxWidth(15.0f));
        EditorGUILayout.LabelField(Styles.MainSection, m_sectionStyle);
        EditorGUILayout.EndHorizontal();

        if (m_mainOptions)
        {
            EditorGUILayout.Space();

            materialEditor.ShaderProperty(m_color, Styles.Color);
            materialEditor.ShaderProperty(m_mainTex, Styles.MainTex);
            materialEditor.ShaderProperty(m_normalMap, Styles.NormalMap);

            EditorGUILayout.Space();
        }

        // outline section
        m_outlineOptions = GUI.Toggle(EditorGUILayout.BeginHorizontal("box"), m_outlineOptions, GUIContent.none, "box");
        EditorGUILayout.Toggle(m_outlineOptions, EditorStyles.foldout, GUILayout.MaxWidth(15.0f));
        EditorGUILayout.LabelField(Styles.OutlineSection, m_sectionStyle);
        EditorGUILayout.EndHorizontal();

        if (m_outlineOptions)
        {
            EditorGUILayout.Space();
            materialEditor.ShaderProperty(m_lineWidth, Styles.LineWidth);
            materialEditor.ShaderProperty(m_outlineColor, Styles.OutlineColor);
            materialEditor.ShaderProperty(m_outlineEmission, Styles.OutlineEmission);
            materialEditor.ShaderProperty(m_outlineTexture, Styles.OutlineTexture);
            materialEditor.ShaderProperty(m_speed, Styles.Speed);

            EditorGUILayout.Space();
        }

        // rgb masking
        DrawHeader(ref m_rgbMaskEnabled, ref m_rgbMaskOptions, Styles.RGBMaskSection);

        if (m_rgbMaskOptions)
        {
            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(!m_rgbMaskEnabled);
            m_useUv2Enabled = EditorGUILayout.Toggle("Use UV2?", m_useUv2Enabled);
            materialEditor.ShaderProperty(m_rgbMask, Styles.RGBMask);
            materialEditor.ShaderProperty(m_redTexture, Styles.RedTexture);
            materialEditor.ShaderProperty(m_greenTexture, Styles.GreenTexture);
            materialEditor.ShaderProperty(m_blueTexture, Styles.BlueTexture);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
        }

        // emissive section
        DrawHeader(ref m_emissionEnabled, ref m_emissionOptions, Styles.EmissionSection);

        if (!m_emissionEnabled)
        {
            m_emissiveBlinkEnabled = false;
            m_emissiveScrollEnabled = false;
        }

        if (m_emissionOptions)
        {         
            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(!m_emissionEnabled);
            materialEditor.ShaderProperty(m_emissionColor, Styles.EmissionColor);
            materialEditor.ShaderProperty(m_emissionMap, Styles.EmissionMap);
            materialEditor.ShaderProperty(m_emissionStrength, Styles.EmissionStrength);

            // emissive blink
            EditorGUI.indentLevel = 2;

            DrawHeader(ref m_emissiveBlinkEnabled, ref m_emissiveBlinkOptions, Styles.EmissiveBlinkSection);

            if (m_emissiveBlinkOptions)
            {
                EditorGUILayout.Space();

                EditorGUI.BeginDisabledGroup(!m_emissiveBlinkEnabled);
                materialEditor.ShaderProperty(m_emissiveBlinkMin, Styles.EmissiveBlinkMin);
                materialEditor.ShaderProperty(m_emissiveBlinkMax, Styles.EmissiveBlinkMax);
                materialEditor.ShaderProperty(m_emissiveBlinkVelocity, Styles.EmissiveBlinkVelocity);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space();
            }

            // emissive scroll
            DrawHeader(ref m_emissiveScrollEnabled, ref m_emissiveScrollOptions, Styles.EmissiveScrollSection);

            if (m_emissiveScrollOptions)
            {
                EditorGUILayout.Space();

                EditorGUI.BeginDisabledGroup(!m_emissiveScrollEnabled);
                materialEditor.ShaderProperty(m_emissiveScrollDirection, Styles.EmissiveScrollDirection);
                materialEditor.ShaderProperty(m_emissiveScrollWidth, Styles.EmissiveScrollWidth);
                materialEditor.ShaderProperty(m_emissiveScrollVelocity, Styles.EmissiveScrollVelocity);
                materialEditor.ShaderProperty(m_emissiveScrollInterval, Styles.EmissiveScrollInterval);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space();
            }

            EditorGUI.indentLevel = 0;

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
        }

        // fake lighting
        DrawHeader(ref m_fakeLightingEnabled, ref m_fakeLightingOptions, Styles.FakeLightingSection);

        if (!m_fakeLightingEnabled)
        {
            m_specularHighlightsEnabled = false;
        }

        if (m_fakeLightingOptions)
        {
            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(!m_fakeLightingEnabled);
            materialEditor.ShaderProperty(m_lightingGradient, Styles.LightingGradient);
            materialEditor.ShaderProperty(m_lightingNormalMultiplier, Styles.LightingNormalMultiplier);
            materialEditor.ShaderProperty(m_lightingShadowStrength, Styles.LightingShadowStrength);
            materialEditor.ShaderProperty(m_lightingDirection, Styles.LightingDirection);

            EditorGUI.indentLevel = 2;

            DrawHeader(ref m_specularHighlightsEnabled, ref m_specularHighlightsOptions, Styles.SpecularHighlightsSection);

            if (m_specularHighlightsOptions)
            {
                EditorGUILayout.Space();

                EditorGUI.BeginDisabledGroup(!m_specularHighlightsEnabled);
                materialEditor.ShaderProperty(m_specularMap, Styles.SpecularMap);
                materialEditor.ShaderProperty(m_specularColor, Styles.SpecularColor);
                materialEditor.ShaderProperty(m_specularRamp, Styles.SpecularRamp);
                materialEditor.ShaderProperty(m_specularBias, Styles.SpecularBias);
                materialEditor.ShaderProperty(m_specularStrength, Styles.SpecularStrength);
                materialEditor.ShaderProperty(m_specularSize, Styles.SpecularSize);
                materialEditor.ShaderProperty(m_specularNormalMultiplier, Styles.SpecularNormalMultiplier);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space();
            }

            EditorGUI.indentLevel = 0;

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
        }

        m_rimLightOptions = GUI.Toggle(EditorGUILayout.BeginHorizontal("box"), m_rimLightOptions, GUIContent.none, "box");
        EditorGUILayout.Toggle(m_rimLightOptions, EditorStyles.foldout, GUILayout.MaxWidth(15.0f));
        EditorGUILayout.LabelField(Styles.RimLightSection, m_sectionStyle);
        EditorGUILayout.EndHorizontal();

        if (m_rimLightOptions)
        {
            EditorGUILayout.Space();

            materialEditor.ShaderProperty(m_rimGlow, Styles.RimGlow);
            materialEditor.ShaderProperty(m_invertRimLighting, Styles.InvertRimLighting);
            materialEditor.ShaderProperty(m_rimColor, Styles.RimColor);
            materialEditor.ShaderProperty(m_rimGlowStrength, Styles.RimGlowStrength);
            materialEditor.ShaderProperty(m_rimWidth, Styles.RimWidth);
            materialEditor.ShaderProperty(m_rimColorBias, Styles.RimColorBias);
            materialEditor.ShaderProperty(m_rimTexTile, Styles.RimTextureTile);
            materialEditor.ShaderProperty(m_rimTex, Styles.RimTexture);
            materialEditor.ShaderProperty(m_rimTexPanSpeed, Styles.RimTexturePanSpeed);
            materialEditor.ShaderProperty(m_rimGlowNormalMultiplier, Styles.RimGlowNormalMultiplier);

            EditorGUILayout.Space();
        }

        //blending
        m_blendOptions = GUI.Toggle(EditorGUILayout.BeginHorizontal("box"), m_blendOptions, GUIContent.none, "box");
        EditorGUILayout.Toggle(m_blendOptions, EditorStyles.foldout, GUILayout.MaxWidth(15.0f));
        EditorGUILayout.LabelField(Styles.BlendSection, m_sectionStyle);
        EditorGUILayout.EndHorizontal();
        
        if (m_blendOptions)
        {
            EditorGUILayout.Space();

            materialEditor.ShaderProperty(m_blendType, Styles.BlendType);
            materialEditor.ShaderProperty(m_blendTexColor, Styles.BlendColor);
            materialEditor.ShaderProperty(m_blendTex, Styles.BlendTexture);
            materialEditor.ShaderProperty(m_blendNoiseTex, Styles.BlendNoiseTexture);
            materialEditor.ShaderProperty(m_blendAlpha, Styles.BlendAlpha);
            materialEditor.ShaderProperty(m_blendTiling, Styles.BlendTiling);

            DrawHeader(ref m_autoBlendEnabled, ref m_autoBlendOptions, Styles.AutoBlendSection);

            if (m_autoBlendOptions)
            {
                EditorGUI.indentLevel = 2;

                EditorGUILayout.Space();

                EditorGUI.BeginDisabledGroup(!m_autoBlendEnabled);
                materialEditor.ShaderProperty(m_autoBlendSpeed, Styles.AutoBlendSpeed);
                materialEditor.ShaderProperty(m_autoBlendDelay, Styles.AutoBlendDelay);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space();

                EditorGUI.indentLevel = 2;
            }

            EditorGUILayout.Space();
        }

        //overlay textures
        m_textureOverlayOptions = GUI.Toggle(EditorGUILayout.BeginHorizontal("box"), m_textureOverlayOptions, GUIContent.none, "box");
        EditorGUILayout.Toggle(m_textureOverlayOptions, EditorStyles.foldout, GUILayout.MaxWidth(15.0f));
        EditorGUILayout.LabelField(Styles.TextureOverlaySection, m_sectionStyle);
        EditorGUILayout.EndHorizontal();

        if (m_textureOverlayOptions)
        {
            EditorGUILayout.Space();

            materialEditor.ShaderProperty(m_numOverlayTextures, Styles.NumOverlayTextures);
            materialEditor.ShaderProperty(m_OverlayColor1, Styles.OverlayColor1);
            materialEditor.ShaderProperty(m_overlayTexture1, Styles.OverlayTexture1);
            materialEditor.ShaderProperty(m_overlayTexture1Velocity, Styles.OverlayTexture1Velocity);
            materialEditor.ShaderProperty(m_OverlayColor2, Styles.OverlayColor2);
            materialEditor.ShaderProperty(m_overlayTexture2, Styles.OverlayTexture2);
            materialEditor.ShaderProperty(m_overlayTexture2Velocity, Styles.OverlayTexture2Velocity);
            materialEditor.ShaderProperty(m_OverlayColor3, Styles.OverlayColor3);
            materialEditor.ShaderProperty(m_overlayTexture3, Styles.OverlayTexture3);
            materialEditor.ShaderProperty(m_overlayTexture3Velocity, Styles.OverlayTexture3Velocity);

            EditorGUILayout.Space();
        }

        m_miscOptions = GUI.Toggle(EditorGUILayout.BeginHorizontal("box"), m_miscOptions, GUIContent.none, "box");
        EditorGUILayout.Toggle(m_miscOptions, EditorStyles.foldout, GUILayout.MaxWidth(15.0f));
        EditorGUILayout.LabelField(Styles.MiscSection, m_sectionStyle);
        EditorGUILayout.EndHorizontal();

        if (m_miscOptions)
        {
            EditorGUILayout.Space();

            materialEditor.ShaderProperty(m_cullMode, Styles.CullMode);
            materialEditor.ShaderProperty(m_lit, Styles.Lit);
            materialEditor.ShaderProperty(m_zTest, Styles.ZTest);
            materialEditor.ShaderProperty(m_clip, Styles.Clip);

            EditorGUILayout.Space();
        }

        ToggleDefines(material);
    }
}
