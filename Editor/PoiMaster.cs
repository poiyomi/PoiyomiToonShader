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
    public static GUIContent RimLightSection = new GUIContent("Rim Lighting", ""); // TODO:
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
    public static GUIContent NormalIntensity = new GUIContent("Normal Intensity", "Normal Strength");

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
    public static GUIContent LightingShadowStrength = new GUIContent("Shadow Strength", "Shadow intensity");
    public static GUIContent LightingDirection = new GUIContent("Light Direction", "Direction towards which the light will cast shadows");

    // specular
    public static GUIContent HardSpecular = new GUIContent("Hard Specular?", ""); // TODO: all of these vvv
    public static GUIContent SpecularMap = new GUIContent("Map", ""); // TODO: all of these vvv
    public static GUIContent Gloss = new GUIContent("Gloss", ""); // TODO: all of these vvv
    public static GUIContent SpecularColor = new GUIContent("Color", "");
    public static GUIContent SpecularStrength = new GUIContent("Strength", "");
    public static GUIContent SpecularSize = new GUIContent("Specular Size", "");

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

    public static GUIContent RGBMaskEnabled = new GUIContent("RGB Mask Enabled", "");
    public static GUIContent EmissionEnabled = new GUIContent("Emission Enabled", "");
    public static GUIContent EmissiveBlinkEnabled = new GUIContent("Emissive Blinking Enabled", "");
    public static GUIContent EmissiveScrollEnabled = new GUIContent("Emissive Scrolling Enabled", "");
    public static GUIContent FakeLightingEnabled = new GUIContent("Fake Lighting Enabled", "");
    public static GUIContent SpecularHighlightsEnabled = new GUIContent("Specular Highlights Enabled", "");
    public static GUIContent AutoBlendEnabled = new GUIContent("Auto Blending Enabled", "");

  }

  GUIStyle m_sectionStyle;

  MaterialProperty m_cullMode = null;
  MaterialProperty m_lit = null;
  MaterialProperty m_zTest = null;
  MaterialProperty m_clip = null;

  MaterialProperty m_mainTex = null;
  MaterialProperty m_color = null;
  MaterialProperty m_normalMap = null;
  MaterialProperty m_normalIntensity = null;

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
  MaterialProperty m_lightingShadowStrength = null;
  MaterialProperty m_lightingDirection = null;

  MaterialProperty m_specularMap = null;
  MaterialProperty m_specularColor = null;
  MaterialProperty m_gloss = null;
  MaterialProperty m_specularStrength = null;
  MaterialProperty m_hardSpecular = null;
  MaterialProperty m_specularSize = null;

  MaterialProperty m_rimGlow = null;
  MaterialProperty m_invertRimLighting = null;
  MaterialProperty m_rimColor = null;
  MaterialProperty m_rimGlowStrength = null;
  MaterialProperty m_rimWidth = null;
  MaterialProperty m_rimColorBias = null;
  MaterialProperty m_rimTexTile = null;
  MaterialProperty m_rimTex = null;
  MaterialProperty m_rimTexPanSpeed = null;
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
  MaterialProperty m_overlayTexture1Velocity = null;
  MaterialProperty m_OverlayColor2 = null;
  MaterialProperty m_overlayTexture2 = null;
  MaterialProperty m_overlayTexture2Velocity = null;
  MaterialProperty m_OverlayColor3 = null;
  MaterialProperty m_overlayTexture3 = null;
  MaterialProperty m_overlayTexture3Velocity = null;

  bool m_miscOptions;
  bool m_mainOptions;
  bool m_outlineOptions;
  bool m_rgbMaskOptions;
  bool m_emissionOptions;
  bool m_emissiveBlinkOptions;
  bool m_emissiveScrollOptions;
  bool m_fakeLightingOptions;
  bool m_specularHighlightsOptions;
  bool m_rimLightOptions;
  bool m_blendOptions;
  bool m_autoBlendOptions;
  bool m_textureOverlayOptions;

  MaterialProperty m_rgbMaskEnabled = null;
  MaterialProperty m_emissionEnabled = null;
  MaterialProperty m_emissiveBlinkEnabled = null;
  MaterialProperty m_emissiveScrollEnabled = null;
  MaterialProperty m_fakeLightingEnabled = null;
  MaterialProperty m_specularHighlightsEnabled = null;
  MaterialProperty m_autoBlendEnabled = null;

  private void FindProperties(MaterialProperty[] props)
  {
    m_cullMode = FindProperty("_Cull", props);
    m_lit = FindProperty("_Lit", props);
    m_zTest = FindProperty("_ZTest", props);
    m_clip = FindProperty("_Clip", props);

    m_mainTex = FindProperty("_MainTex", props);
    m_color = FindProperty("_Color", props);
    m_normalMap = FindProperty("_NormalMap", props);
    m_normalIntensity = FindProperty("_NormalIntensity", props);

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
    m_lightingShadowStrength = FindProperty("_ShadowStrength", props);
    m_lightingDirection = FindProperty("_LightDirection", props);

    m_specularMap = FindProperty("_SpecularMap", props);
    m_hardSpecular = FindProperty("_HARD_SPECULAR", props);
    m_specularColor = FindProperty("_SpecularColor", props);
    m_gloss = FindProperty("_Gloss", props);
    m_specularStrength = FindProperty("_SpecularStrength", props);
    m_specularSize = FindProperty("_SpecularSize", props);

    m_rimGlow = FindProperty("_RimGlow", props);
    m_invertRimLighting = FindProperty("_INVERT_RIM", props);
    m_rimColor = FindProperty("_RimColor", props);
    m_rimGlowStrength = FindProperty("_RimGlowStrength", props);
    m_rimWidth = FindProperty("_RimWidth", props);
    m_rimColorBias = FindProperty("_RimColorBias", props);
    m_rimTexTile = FindProperty("_RimTexTile", props);
    m_rimTex = FindProperty("_RimTex", props);
    m_rimTexPanSpeed = FindProperty("_RimTexPanSpeed", props);

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
    m_overlayTexture1Velocity = FindProperty("_Tex1Velocity", props);
    m_OverlayColor2 = FindProperty("_OverlayColor2", props);
    m_overlayTexture2 = FindProperty("_OverlayTexture2", props);
    m_overlayTexture2Velocity = FindProperty("_Tex2Velocity", props);
    m_OverlayColor3 = FindProperty("_OverlayColor3", props);
    m_overlayTexture3 = FindProperty("_OverlayTexture3", props);
    m_overlayTexture3Velocity = FindProperty("_Tex3Velocity", props);

    m_rgbMaskEnabled = FindProperty("_RGB_MASK", props);
    m_emissionEnabled = FindProperty("_EMISSION", props);
    m_emissiveBlinkEnabled = FindProperty("_BLINKING_EMISSION", props);
    m_emissiveScrollEnabled = FindProperty("_SCROLLING_EMISSION", props);
    m_fakeLightingEnabled = FindProperty("_FAKE_LIGHTING", props);
    m_specularHighlightsEnabled = FindProperty("_SPECULAR_HIGHLIGHTS", props);
    m_autoBlendEnabled = FindProperty("_AUTO_BLEND", props);
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
      materialEditor.ShaderProperty(m_normalIntensity, Styles.NormalIntensity);

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
    m_rgbMaskOptions = GUI.Toggle(EditorGUILayout.BeginHorizontal("box"), m_rgbMaskOptions, GUIContent.none, "box");
    EditorGUILayout.Toggle(m_rgbMaskOptions, EditorStyles.foldout, GUILayout.MaxWidth(15.0f));
    EditorGUILayout.LabelField(Styles.RGBMaskSection, m_sectionStyle);
    EditorGUILayout.EndHorizontal();

    if (m_rgbMaskOptions)
    {
      EditorGUILayout.Space();
      materialEditor.ShaderProperty(m_rgbMaskEnabled, Styles.RGBMaskEnabled);
      materialEditor.ShaderProperty(m_rgbMask, Styles.RGBMask);
      materialEditor.ShaderProperty(m_redTexture, Styles.RedTexture);
      materialEditor.ShaderProperty(m_greenTexture, Styles.GreenTexture);
      materialEditor.ShaderProperty(m_blueTexture, Styles.BlueTexture);
      EditorGUILayout.Space();
    }

    // emissive section
    m_emissionOptions = GUI.Toggle(EditorGUILayout.BeginHorizontal("box"), m_emissionOptions, GUIContent.none, "box");
    EditorGUILayout.Toggle(m_emissionOptions, EditorStyles.foldout, GUILayout.MaxWidth(15.0f));
    EditorGUILayout.LabelField(Styles.EmissionSection, m_sectionStyle);
    EditorGUILayout.EndHorizontal();

    if (m_emissionOptions)
    {
      EditorGUILayout.Space();
      materialEditor.ShaderProperty(m_emissionEnabled, Styles.EmissionEnabled);
      materialEditor.ShaderProperty(m_emissionColor, Styles.EmissionColor);
      materialEditor.ShaderProperty(m_emissionMap, Styles.EmissionMap);
      materialEditor.ShaderProperty(m_emissionStrength, Styles.EmissionStrength);
      EditorGUILayout.Space();
    }

    // emissive blink
    m_emissiveBlinkOptions = GUI.Toggle(EditorGUILayout.BeginHorizontal("box"), m_emissiveBlinkOptions, GUIContent.none, "box");
    EditorGUILayout.Toggle(m_emissiveBlinkOptions, EditorStyles.foldout, GUILayout.MaxWidth(15.0f));
    EditorGUILayout.LabelField(Styles.EmissiveBlinkSection, m_sectionStyle);
    EditorGUILayout.EndHorizontal();

    if (m_emissiveBlinkOptions)
    {
      EditorGUILayout.Space();
      materialEditor.ShaderProperty(m_rgbMaskEnabled, Styles.RGBMaskEnabled);
      materialEditor.ShaderProperty(m_emissiveBlinkMin, Styles.EmissiveBlinkMin);
      materialEditor.ShaderProperty(m_emissiveBlinkMax, Styles.EmissiveBlinkMax);
      materialEditor.ShaderProperty(m_emissiveBlinkVelocity, Styles.EmissiveBlinkVelocity);
      EditorGUILayout.Space();
    }

    // emissive scroll
    m_emissiveScrollOptions = GUI.Toggle(EditorGUILayout.BeginHorizontal("box"), m_emissiveScrollOptions, GUIContent.none, "box");
    EditorGUILayout.Toggle(m_emissiveScrollOptions, EditorStyles.foldout, GUILayout.MaxWidth(15.0f));
    EditorGUILayout.LabelField(Styles.EmissiveScrollSection, m_sectionStyle);
    EditorGUILayout.EndHorizontal();
    if (m_emissiveScrollOptions)
    {
      EditorGUILayout.Space();

      materialEditor.ShaderProperty(m_emissiveScrollEnabled, Styles.EmissiveScrollEnabled);
      materialEditor.ShaderProperty(m_emissiveScrollDirection, Styles.EmissiveScrollDirection);
      materialEditor.ShaderProperty(m_emissiveScrollWidth, Styles.EmissiveScrollWidth);
      materialEditor.ShaderProperty(m_emissiveScrollVelocity, Styles.EmissiveScrollVelocity);
      materialEditor.ShaderProperty(m_emissiveScrollInterval, Styles.EmissiveScrollInterval);

      EditorGUILayout.Space();
    }

    // fake lighting
    m_fakeLightingOptions = GUI.Toggle(EditorGUILayout.BeginHorizontal("box"), m_fakeLightingOptions, GUIContent.none, "box");
    EditorGUILayout.Toggle(m_fakeLightingOptions, EditorStyles.foldout, GUILayout.MaxWidth(15.0f));
    EditorGUILayout.LabelField(Styles.FakeLightingSection, m_sectionStyle);
    EditorGUILayout.EndHorizontal();
    if (m_fakeLightingOptions)
    {
      EditorGUILayout.Space();

      materialEditor.ShaderProperty(m_fakeLightingEnabled, Styles.FakeLightingEnabled);
      materialEditor.ShaderProperty(m_lightingGradient, Styles.LightingGradient);
      materialEditor.ShaderProperty(m_lightingShadowStrength, Styles.LightingShadowStrength);
      materialEditor.ShaderProperty(m_lightingDirection, Styles.LightingDirection);

      EditorGUILayout.Space();
    }
    m_specularHighlightsOptions = GUI.Toggle(EditorGUILayout.BeginHorizontal("box"), m_specularHighlightsOptions, GUIContent.none, "box");
    EditorGUILayout.Toggle(m_specularHighlightsOptions, EditorStyles.foldout, GUILayout.MaxWidth(15.0f));
    EditorGUILayout.LabelField(Styles.SpecularHighlightsSection, m_sectionStyle);
    EditorGUILayout.EndHorizontal();
    if (m_specularHighlightsOptions)
    {
      EditorGUILayout.Space();

      materialEditor.ShaderProperty(m_specularHighlightsEnabled, Styles.SpecularHighlightsEnabled);
      materialEditor.ShaderProperty(m_specularMap, Styles.SpecularMap);
      materialEditor.ShaderProperty(m_gloss, Styles.Gloss);
      materialEditor.ShaderProperty(m_specularColor, Styles.SpecularColor);
      materialEditor.ShaderProperty(m_specularStrength, Styles.SpecularStrength);
      materialEditor.ShaderProperty(m_hardSpecular, Styles.HardSpecular);
      materialEditor.ShaderProperty(m_specularSize, Styles.SpecularSize);

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

      EditorGUILayout.Space();
    }

    m_autoBlendOptions = GUI.Toggle(EditorGUILayout.BeginHorizontal("box"), m_autoBlendOptions, GUIContent.none, "box");
    EditorGUILayout.Toggle(m_autoBlendOptions, EditorStyles.foldout, GUILayout.MaxWidth(15.0f));
    EditorGUILayout.LabelField(Styles.AutoBlendSection, m_sectionStyle);
    EditorGUILayout.EndHorizontal();
    if (m_autoBlendOptions)
    {

      EditorGUILayout.Space();

      materialEditor.ShaderProperty(m_autoBlendEnabled, Styles.AutoBlendEnabled);
      materialEditor.ShaderProperty(m_autoBlendSpeed, Styles.AutoBlendSpeed);
      materialEditor.ShaderProperty(m_autoBlendDelay, Styles.AutoBlendDelay);

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
