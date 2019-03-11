//designed for 2.1.0

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PoiToon210 : ShaderGUI
{

    private static class PoiToonUI
    {
        public static bool Foldout(string title, bool display)
        {
            var style = new GUIStyle("ShurikenModuleTitle");
            style.font = new GUIStyle(EditorStyles.label).font;
            style.border = new RectOffset(15, 7, 4, 4);
            style.fixedHeight = 22;
            style.contentOffset = new Vector2(20f, -2f);

            var rect = GUILayoutUtility.GetRect(16f, 22f, style);
            GUI.Box(rect, title, style);

            var e = Event.current;

            var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
            }

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                display = !display;
                e.Use();
            }

            return display;
        }
    }

    private static class Styles
    {

        // sections
        public static GUIContent MainSection = new GUIContent("Main", "Main configuration section");
        public static GUIContent EmissionSection = new GUIContent("Emission", "Configuration for emissiveness");
        public static GUIContent FakeLightingSection = new GUIContent("Fake Lighting", "Simulates a local light if the world lacks one");
        public static GUIContent SpecularHighlightsSection = new GUIContent("Specular Highlights", "");
        public static GUIContent RimLightSection = new GUIContent("Rim Lighting", "");
        public static GUIContent MiscSection = new GUIContent("Misc", "Miscellaneous settings");

        // main section
        public static GUIContent Color = new GUIContent("Color", "Color used for tinting the main texture");
        public static GUIContent Desaturation = new GUIContent("Desaturation", "Desaturate the colors before applying Color");
        public static GUIContent MainTex = new GUIContent("Main Tex", "Main texture for the shader");
        public static GUIContent Clip = new GUIContent("Alpha Cutoff", "Alpha treshold");
        public static GUIContent NormalMap = new GUIContent("Normal Map", "Bump map");
        public static GUIContent NormalIntensity = new GUIContent("Normal Intensity", "Normal Strength");

        public static GUIContent CubeMap = new GUIContent("Baked CubeMap", "");
        public static GUIContent SampleWorld = new GUIContent("Force Baked Cubemap", "Don't sample the world for reflections if 1");
        public static GUIContent MetallicMap = new GUIContent("MetallicMap", "");
        public static GUIContent Metallic = new GUIContent("Metallic", "");
        public static GUIContent RoughnessMap = new GUIContent("RoughnessMap", "");
        public static GUIContent Roughness = new GUIContent("Smoothness", "");

        // outlines

        public static GUIContent LineWidth = new GUIContent("Line Width", "Width of the outline");
        public static GUIContent OutlineColor = new GUIContent("Outline Color", "Tint of the outline");
        public static GUIContent OutlineEmission = new GUIContent("Outline Emission", "How much emission glows");
        public static GUIContent OutlineTexture = new GUIContent("Outline Texture", "Texture for the outline");
        public static GUIContent Speed = new GUIContent("Speed", "speed of texture scrolling on outline");

        // emission
        public static GUIContent EmissionColor = new GUIContent("Color", "Color used for emission");
        public static GUIContent EmissionMap = new GUIContent("Map", "Texture used for emission");
        public static GUIContent EmissionScrollSpeed = new GUIContent("Emission Pan Speed", "");
        public static GUIContent EmissionStrength = new GUIContent("Strength", "Strength multiplier for emission");

        // emissive blink
        public static GUIContent EmissiveBlinkMin = new GUIContent("Min", "Minimum value used for emissive blink");
        public static GUIContent EmissiveBlinkMax = new GUIContent("Max", "Maximum value used for emissive blink");
        public static GUIContent EmissiveBlinkVelocity = new GUIContent("Velocity", "Blinking speed");

        // emissive scroll
        public static GUIContent EmissiveScrollEnabled = new GUIContent("Emissive Scrolling Enabled", "");
        public static GUIContent EmissiveScrollDirection = new GUIContent("Direction", "Emissive scrolling direction");
        public static GUIContent EmissiveScrollWidth = new GUIContent("Width", "Emissive scrolling width");
        public static GUIContent EmissiveScrollVelocity = new GUIContent("Velocity", "Emissive scrolling speed");
        public static GUIContent EmissiveScrollInterval = new GUIContent("Interval", "Delay between emissive scrolls");

        // fake lighting
        public static GUIContent LightingGradient = new GUIContent("Lighting Ramp", "Texture the fake light uses for tinting");
        public static GUIContent LightingShadowStrength = new GUIContent("Shadow Strength", "Shadow intensity");
        public static GUIContent LightingShadowOffsett = new GUIContent("Shadow Offset", "Shadow Offset");
        public static GUIContent LightingDirection = new GUIContent("Light Direction", "Direction towards which the light will cast shadows");
        public static GUIContent ForceLightDirection = new GUIContent("Force Light Direction", "");
        public static GUIContent MinBrightness = new GUIContent("Min Brightness", "Limit how dark you can get");

        // specular
        public static GUIContent HardSpecular = new GUIContent("Hard Specular?", ""); // TODO: all of these vvv
        public static GUIContent SpecularMap = new GUIContent("Map", ""); // TODO: all of these vvv
        public static GUIContent Gloss = new GUIContent("Gloss", ""); // TODO: all of these vvv
        public static GUIContent SpecularColor = new GUIContent("Color", "");
        public static GUIContent SpecularBias = new GUIContent("Specular Bias", "");
        public static GUIContent SpecularStrength = new GUIContent("Strength", "");
        public static GUIContent SpecularSize = new GUIContent("Specular Size", "");

        // rim Lighting
        public static GUIContent RimColor = new GUIContent("Rim Color", "");
        public static GUIContent RimWidth = new GUIContent("Width", "");
        public static GUIContent RimGlowStrength = new GUIContent("Glow Strength", "");
        public static GUIContent RimSharpness = new GUIContent("Rim Sharpness", "");
        public static GUIContent RimColorBias = new GUIContent("Color Bias", "");
        public static GUIContent RimTexture = new GUIContent("Texture", "");
        public static GUIContent RimTexturePanSpeed = new GUIContent("Texture Pan Speed", "");

        // stencils
        public static GUIContent StencilRef = new GUIContent("Stencil Value", "The refernce to be compared against");
        public static GUIContent StencilCompareFunction = new GUIContent("Stencil Compare Function", "How the referenced value is being compared");
        public static GUIContent StencilOp = new GUIContent("Stencil Op", "What to do if stencil comparison passes");

        // misc section
        public static GUIContent CullMode = new GUIContent("Cull Mode", "Controls which face of the mesh is rendered \nOff = Double sided \nFront = Single sided (reverse) \nBack = Single sided");
        public static GUIContent Lit = new GUIContent("Lit", "Toggles environmental lighting");
        public static GUIContent SrcBlend = new GUIContent("Src Blend", "");
        public static GUIContent DstBlend = new GUIContent("Dst Blend", "");
        public static GUIContent ZTest = new GUIContent("ZTest", "don't be an asshole");
    }

    GUIStyle m_sectionStyle;
    MaterialProperty m_color = null;
    MaterialProperty m_desaturation = null;
    MaterialProperty m_mainTex = null;
    MaterialProperty m_normalMap = null;
    MaterialProperty m_normalIntensity = null;

    MaterialProperty m_cubeMap;
    MaterialProperty m_sampleWorld;
    MaterialProperty m_metallicMap;
    MaterialProperty m_metallic;
    MaterialProperty m_roughnessMap;
    MaterialProperty m_roughness;

    MaterialProperty m_lineWidth;
    MaterialProperty m_outlineColor;
    MaterialProperty m_outlineEmission;
    MaterialProperty m_outlineTexture;
    MaterialProperty m_speed;

    MaterialProperty m_emissionColor = null;
    MaterialProperty m_emissionMap = null;
    MaterialProperty m_emissionScrollSpeed = null;
    MaterialProperty m_emissionStrength = null;

    MaterialProperty m_emissiveBlinkMin = null;
    MaterialProperty m_emissiveBlinkMax = null;
    MaterialProperty m_emissiveBlinkVelocity = null;

    MaterialProperty m_emissiveScrollEnabled = null;
    MaterialProperty m_emissiveScrollDirection = null;
    MaterialProperty m_emissiveScrollWidth = null;
    MaterialProperty m_emissiveScrollVelocity = null;
    MaterialProperty m_emissiveScrollInterval = null;

    MaterialProperty m_lightingGradient = null;
    MaterialProperty m_lightingShadowStrength = null;
    MaterialProperty m_lightingShadowOffset = null;
    MaterialProperty m_lightingDirection = null;
    MaterialProperty m_forceLightDirection = null;
    MaterialProperty m_minBrightness = null;

    MaterialProperty m_specularMap = null;
    MaterialProperty m_specularColor = null;
    MaterialProperty m_hardSpecular = null;
    MaterialProperty m_specularBias = null;
    MaterialProperty m_gloss = null;
    MaterialProperty m_specularStrength = null;
    MaterialProperty m_specularSize = null;

    MaterialProperty m_rimColor = null;
    MaterialProperty m_rimWidth = null;
    MaterialProperty m_rimStrength = null;
    MaterialProperty m_rimSharpness = null;
    MaterialProperty m__rimLightColorBias = null;
    MaterialProperty m_rimTex = null;
    MaterialProperty m_rimTexPanSpeed = null;

    MaterialProperty m_stencilRef = null;
    MaterialProperty m_stencilOp = null;
    MaterialProperty m_stencilCompareFunction = null;

    MaterialProperty m_cullMode = null;
    MaterialProperty m_lit = null;
    MaterialProperty m_clip = null;
    MaterialProperty m_srcBlend = null;
    MaterialProperty m_dstBlend = null;
    MaterialProperty m_zTest = null;


    bool m_mainOptions = true;
    bool m_metallicOptions;
    bool m_outlineOptions;
    bool m_emissionOptions;
    bool m_fakeLightingOptions;
    bool m_specularHighlightsOptions;
    bool m_stencilOptions;
    bool m_rimLightOptions;
    bool m_miscOptions;

    private void FindProperties(MaterialProperty[] props)
    {
        m_color = FindProperty("_Color", props);
        m_desaturation = FindProperty("_Desaturation", props);
        m_mainTex = FindProperty("_MainTex", props);
        m_normalMap = FindProperty("_NormalMap", props);
        m_normalIntensity = FindProperty("_NormalIntensity", props);

        m_cubeMap = FindProperty("_CubeMap", props);
        m_sampleWorld = FindProperty("_SampleWorld", props);
        m_metallicMap = FindProperty("_MetallicMap", props);
        m_metallic = FindProperty("_Metallic", props);
        m_roughnessMap = FindProperty("_RoughnessMap", props);
        m_roughness = FindProperty("_Roughness", props);

        m_lineWidth = FindProperty("_LineWidth", props);
        m_outlineColor = FindProperty("_LineColor", props);
        m_outlineEmission = FindProperty("_OutlineEmission", props);
        m_outlineTexture = FindProperty("_OutlineTexture", props);
        m_speed = FindProperty("_Speed", props);

        m_emissionColor = FindProperty("_EmissionColor", props);
        m_emissionMap = FindProperty("_EmissionMap", props);
        m_emissionScrollSpeed = FindProperty("_EmissionScrollSpeed", props);
        m_emissionStrength = FindProperty("_EmissionStrength", props);

        m_emissiveBlinkMin = FindProperty("_EmissiveBlink_Min", props);
        m_emissiveBlinkMax = FindProperty("_EmissiveBlink_Max", props);
        m_emissiveBlinkVelocity = FindProperty("_EmissiveBlink_Velocity", props);

        m_emissiveScrollEnabled = FindProperty("_SCROLLING_EMISSION", props);
        m_emissiveScrollDirection = FindProperty("_EmissiveScroll_Direction", props);
        m_emissiveScrollWidth = FindProperty("_EmissiveScroll_Width", props);
        m_emissiveScrollVelocity = FindProperty("_EmissiveScroll_Velocity", props);
        m_emissiveScrollInterval = FindProperty("_EmissiveScroll_Interval", props);

        m_lightingGradient = FindProperty("_LightingGradient", props);
        m_lightingShadowStrength = FindProperty("_ShadowStrength", props);
        m_lightingShadowOffset = FindProperty("_ShadowOffset", props);
        m_lightingDirection = FindProperty("_LightDirection", props);
        m_forceLightDirection = FindProperty("_ForceLightDirection", props);
        m_minBrightness = FindProperty("_MinBrightness", props);

        m_specularMap = FindProperty("_SpecularMap", props);
        m_specularColor = FindProperty("_SpecularColor", props);
        m_gloss = FindProperty("_Gloss", props);
        m_specularStrength = FindProperty("_SpecularStrength", props);
        m_specularBias = FindProperty("_SpecularBias", props);
        m_hardSpecular = FindProperty("_HARD_SPECULAR", props);
        m_specularSize = FindProperty("_SpecularSize", props);

        m_rimColor = FindProperty("_RimLightColor", props);
        m_rimWidth = FindProperty("_RimWidth", props);
        m_rimStrength = FindProperty("_RimStrength", props);
        m_rimSharpness = FindProperty("_RimSharpness", props);
        m__rimLightColorBias = FindProperty("_RimLightColorBias", props);
        m_rimTex = FindProperty("_RimTex", props);

        m_stencilRef = FindProperty("_StencilRef", props);
        m_stencilOp = FindProperty("_StencilOp", props);
        m_stencilCompareFunction = FindProperty("_StencilCompareFunction", props);

        m_rimTexPanSpeed = FindProperty("_RimTexPanSpeed", props);
        m_cullMode = FindProperty("_Cull", props);
        m_lit = FindProperty("_Lit", props);
        m_clip = FindProperty("_Clip", props);
        m_srcBlend = FindProperty("_SourceBlend", props);
        m_dstBlend = FindProperty("_DestinationBlend", props);
        m_zTest = FindProperty("_ZTest", props);
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
    }

    void LoadDefaults(Material mat)
    {
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

        EditorGUILayout.LabelField("<size=18><color=#de0653>Poiyomi Toon Shader V2.1.0</color></size>", style, GUILayout.MinHeight(16));
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
        m_mainOptions = PoiToonUI.Foldout("Main", m_mainOptions);
        if (m_mainOptions)
        {
            EditorGUILayout.Space();

            materialEditor.ShaderProperty(m_color, Styles.Color);
            materialEditor.ShaderProperty(m_desaturation, Styles.Desaturation);
            materialEditor.ShaderProperty(m_mainTex, Styles.MainTex);
            materialEditor.ShaderProperty(m_normalMap, Styles.NormalMap);
            materialEditor.ShaderProperty(m_normalIntensity, Styles.NormalIntensity);
            materialEditor.ShaderProperty(m_clip, Styles.Clip);

            EditorGUILayout.Space();
        }

        m_metallicOptions = PoiToonUI.Foldout("Metallic", m_metallicOptions);
        if (m_metallicOptions)
        {
            EditorGUILayout.Space();
            materialEditor.ShaderProperty(m_cubeMap, Styles.CubeMap);
            materialEditor.ShaderProperty(m_sampleWorld, Styles.SampleWorld);
            materialEditor.ShaderProperty(m_metallicMap, Styles.MetallicMap);
            materialEditor.ShaderProperty(m_metallic, Styles.Metallic);
            materialEditor.ShaderProperty(m_roughnessMap, Styles.RoughnessMap);
            materialEditor.ShaderProperty(m_roughness, Styles.Roughness);
            EditorGUILayout.Space();
        }

        // outline section
        m_outlineOptions = PoiToonUI.Foldout("Outline", m_outlineOptions);

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

        // emissive section
        m_emissionOptions = PoiToonUI.Foldout("Emission", m_emissionOptions);
        if (m_emissionOptions)
        {
            EditorGUILayout.Space();
            materialEditor.ShaderProperty(m_emissionColor, Styles.EmissionColor);
            materialEditor.ShaderProperty(m_emissionMap, Styles.EmissionMap);
            materialEditor.ShaderProperty(m_emissionScrollSpeed, Styles.EmissionScrollSpeed);
            materialEditor.ShaderProperty(m_emissionStrength, Styles.EmissionStrength);
            EditorGUILayout.Space();
            materialEditor.ShaderProperty(m_emissiveBlinkMin, Styles.EmissiveBlinkMin);
            materialEditor.ShaderProperty(m_emissiveBlinkMax, Styles.EmissiveBlinkMax);
            materialEditor.ShaderProperty(m_emissiveBlinkVelocity, Styles.EmissiveBlinkVelocity);
            EditorGUILayout.Space();
            materialEditor.ShaderProperty(m_emissiveScrollEnabled, Styles.EmissiveScrollEnabled);
            materialEditor.ShaderProperty(m_emissiveScrollDirection, Styles.EmissiveScrollDirection);
            materialEditor.ShaderProperty(m_emissiveScrollWidth, Styles.EmissiveScrollWidth);
            materialEditor.ShaderProperty(m_emissiveScrollVelocity, Styles.EmissiveScrollVelocity);
            materialEditor.ShaderProperty(m_emissiveScrollInterval, Styles.EmissiveScrollInterval);
            EditorGUILayout.Space();
        }

        // fake lighting
        m_fakeLightingOptions = PoiToonUI.Foldout("Fake Lighting", m_fakeLightingOptions);
        if (m_fakeLightingOptions)
        {
            EditorGUILayout.Space();
            materialEditor.ShaderProperty(m_lightingGradient, Styles.LightingGradient);
            materialEditor.ShaderProperty(m_lightingShadowStrength, Styles.LightingShadowStrength);
            materialEditor.ShaderProperty(m_lightingShadowOffset, Styles.LightingShadowOffsett);
            materialEditor.ShaderProperty(m_forceLightDirection, Styles.ForceLightDirection);
            materialEditor.ShaderProperty(m_lightingDirection, Styles.LightingDirection);
            materialEditor.ShaderProperty(m_minBrightness, Styles.MinBrightness);
            EditorGUILayout.Space();
        }

        // Specular Highlights
        m_specularHighlightsOptions = PoiToonUI.Foldout("Specular Highlight", m_specularHighlightsOptions);
        if (m_specularHighlightsOptions)
        {
            EditorGUILayout.Space();
            materialEditor.ShaderProperty(m_specularMap, Styles.SpecularMap);
            materialEditor.ShaderProperty(m_gloss, Styles.Gloss);
            materialEditor.ShaderProperty(m_specularColor, Styles.SpecularColor);
            materialEditor.ShaderProperty(m_specularStrength, Styles.SpecularStrength);
            materialEditor.ShaderProperty(m_specularBias, Styles.SpecularBias);
            materialEditor.ShaderProperty(m_hardSpecular, Styles.HardSpecular);
            materialEditor.ShaderProperty(m_specularSize, Styles.SpecularSize);
            EditorGUILayout.Space();
        }

        // Rim Lighting
        m_rimLightOptions = PoiToonUI.Foldout("Rim Lighting", m_rimLightOptions);
        if (m_rimLightOptions)
        {
            EditorGUILayout.Space();
            materialEditor.ShaderProperty(m_rimColor, Styles.RimColor);
            materialEditor.ShaderProperty(m_rimStrength, Styles.RimGlowStrength);
            materialEditor.ShaderProperty(m_rimSharpness, Styles.RimSharpness);
            materialEditor.ShaderProperty(m_rimWidth, Styles.RimWidth);
            materialEditor.ShaderProperty(m__rimLightColorBias, Styles.RimColorBias);
            materialEditor.ShaderProperty(m_rimTex, Styles.RimTexture);
            materialEditor.ShaderProperty(m_rimTexPanSpeed, Styles.RimTexturePanSpeed);
            EditorGUILayout.Space();
        }

        m_stencilOptions = PoiToonUI.Foldout("Stencil", m_stencilOptions);
        if (m_stencilOptions)
        {
            EditorGUILayout.Space();

            materialEditor.ShaderProperty(m_stencilRef, Styles.StencilRef);
            materialEditor.ShaderProperty(m_stencilCompareFunction, Styles.StencilCompareFunction);
            materialEditor.ShaderProperty(m_stencilOp, Styles.StencilOp);

            EditorGUILayout.Space();
        }

        m_miscOptions = PoiToonUI.Foldout("Misc", m_miscOptions);
        if (m_miscOptions)
        {
            EditorGUILayout.Space();

            materialEditor.ShaderProperty(m_cullMode, Styles.CullMode);
            materialEditor.ShaderProperty(m_lit, Styles.Lit);
            materialEditor.ShaderProperty(m_srcBlend, Styles.SrcBlend);
            materialEditor.ShaderProperty(m_dstBlend, Styles.DstBlend);
            materialEditor.ShaderProperty(m_zTest, Styles.ZTest);

            EditorGUILayout.Space();
        }

        ToggleDefines(material);
    }
}
