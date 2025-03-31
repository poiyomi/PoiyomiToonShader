// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Thry.ThryEditor.Helpers;
using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor
{
    public class GradientEditor : EditorWindow
    {
        public class GradientData
        {
            public bool UsePreviewTexture;
            public Texture PreviewTexture;
            public Gradient Gradient;
        }
        public static void Open(GradientData data, MaterialProperty prop, TextureData predefinedTextureSettings, bool force_texture_options = false, bool show_texture_options=true, ColorSpace colorSpace=ColorSpace.Linear)
        {
            texture_settings_data = LoadTextureSettings(prop, predefinedTextureSettings, force_texture_options);
            data.Gradient = TextureHelper.GetGradient(prop.textureValue);
            data.UsePreviewTexture = true;
            GradientEditor window = (GradientEditor)EditorWindow.GetWindow(typeof(GradientEditor));
            window.titleContent = new GUIContent("Gradient '" +prop.name +"' of '"+ prop.targets[0].name + "'");
            window._colorSpace = colorSpace;
            window._privious_preview_texture = prop.textureValue;
            window._prop = prop;
            window._data = data;
            window._show_texture_options = show_texture_options;
            window.minSize = new Vector2(350, 350);
            window.Show();
        }

        private ColorSpace _colorSpace = ColorSpace.Linear;
        private GradientData _data;
        private MaterialProperty _prop;

        private object _gradient_editor;
        private MethodInfo _ongui;
        private MethodInfo _gradient_editor_init;

        private object _preset_libary_editor;
        private MethodInfo _preset_libary_onGUI;
        private object _preset_libary_editor_state;

        private bool _inited = false;

        private bool _show_texture_options = true;

        private bool _gradient_has_been_edited = false;
        private Texture _privious_preview_texture;

        private static TextureData LoadTextureSettings(MaterialProperty prop, TextureData predefinedTextureSettings, bool force_texture_options)
        {
            if (force_texture_options && predefinedTextureSettings != null)
                return predefinedTextureSettings;
            string json_texture_settings = FileHelper.LoadValueFromFile("gradient_texture_options_"+prop.name, PATH.PERSISTENT_DATA);
            if (json_texture_settings != null)
                return Parser.Deserialize<TextureData>(json_texture_settings);
            else if (predefinedTextureSettings != null)
                return predefinedTextureSettings;
            else
                return new TextureData();
        }
        private static TextureData texture_settings_data;
        private TextureData textureSettings
        {
            get
            {
                return texture_settings_data;
            }
        }

        public void Awake()
        {
            Type gradient_editor_type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GradientEditor");
            _gradient_editor = Activator.CreateInstance(gradient_editor_type);
            _gradient_editor_init = gradient_editor_type.GetMethod("Init");

            _ongui = gradient_editor_type.GetMethod("OnGUI");
        }

        public void OnDestroy()
        {
            if (_gradient_has_been_edited)
            {
                if (_data.PreviewTexture.GetType() == typeof(Texture2D))
                {
                    string file_name = GradientFileName(_data.Gradient, _prop.targets[0].name);
                    Texture2D toSave = (Texture2D)_data.PreviewTexture;
                    if(_colorSpace == ColorSpace.Gamma)
                    {
                        toSave = TextureHelper.ConvertToGamma(toSave);
                    }
                    Texture saved = TextureHelper.SaveTextureAsPNG(toSave, PATH.TEXTURES_DIR+"/Gradients/" + file_name, textureSettings);
                    file_name = Regex.Replace(file_name, @"\.((png)|(jpg))$", "");
                    FileHelper.SaveValueToFile(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(saved)), Parser.Serialize(_data.Gradient), PATH.GRADIENT_INFO_FILE);
                    _prop.textureValue = saved;
                    // change importer settings
                    TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(saved));
                     importer.textureCompression = TextureImporterCompression.CompressedHQ;
                     importer.sRGBTexture = _colorSpace != ColorSpace.Linear;
                    if(Config.Instance.gradientEditorCompressionOverwrite != TextureImporterFormat.Automatic)
                    {
                        importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
                        {
                            name = "PC",
                            overridden = true,
                            maxTextureSize = 2048,
                            format = Config.Instance.gradientEditorCompressionOverwrite
                        });
                    }
                    importer.SaveAndReimport();
                }
            }
            _data.UsePreviewTexture = false;
            ShaderEditor.RepaintActive();
        }

        private string GradientFileName(Gradient gradient, string material_name)
        {
            string hash = "" + gradient.GetHashCode();
            return GradientFileName(hash, material_name);
        }

        private string GradientFileName(string hash, string material_name)
        {
            Config config = Config.Instance;
            string ret = config.gradient_name;
            ret = Regex.Replace(ret, "<hash>", hash);
            ret = Regex.Replace(ret, "<material>", material_name);
            return ret;
        }

        private void InitSomeStuff()
        {
            Type presetLibraryEditorState_type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.PresetLibraryEditorState");
            _preset_libary_editor_state = Activator.CreateInstance(presetLibraryEditorState_type, "Gradient");
            MethodInfo transfer_editor_prefs_state = presetLibraryEditorState_type.GetMethod("TransferEditorPrefsState");
            transfer_editor_prefs_state.Invoke(_preset_libary_editor_state, new object[] { true });

            Type scriptable_save_load_helper_type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ScriptableObjectSaveLoadHelper`1");
            Type gradient_preset_libary_type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GradientPresetLibrary");
            Type preset_libary_editor_type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.PresetLibraryEditor`1");
            Type save_load_helper_type = scriptable_save_load_helper_type.MakeGenericType(gradient_preset_libary_type);
            Type gradient_preset_libary_editor_type = preset_libary_editor_type.MakeGenericType(gradient_preset_libary_type);

            object saveLoadHelper = Activator.CreateInstance(save_load_helper_type, "gradients", SaveType.Text);

            Action<int, object> preset_libary_editor_callback = PresetClickedCallback;
            _preset_libary_editor = Activator.CreateInstance(gradient_preset_libary_editor_type, saveLoadHelper, _preset_libary_editor_state, preset_libary_editor_callback);
            PropertyInfo show_header = gradient_preset_libary_editor_type.GetProperty("showHeader");
            show_header.SetValue(_preset_libary_editor, true, null);
            PropertyInfo minMaxPreviewHeight = gradient_preset_libary_editor_type.GetProperty("minMaxPreviewHeight");
            minMaxPreviewHeight.SetValue(_preset_libary_editor, new Vector2(14f, 14f), null);

            _preset_libary_onGUI = gradient_preset_libary_editor_type.GetMethod("OnGUI");

            SetGradient(_data.Gradient);
            _gradient_has_been_edited = false;

            _inited = true;
        }

        public void PresetClickedCallback(int clickCount, object presetObject)
        {
            Gradient gradient = presetObject as Gradient;
            if (gradient == null)
                Debug.LogError("Incorrect object passed " + presetObject);
            SetGradient(gradient);
        }

        void SetGradient(Gradient gradient)
        {
            _data.Gradient = gradient;
#if UNITY_2020_1_OR_NEWER
            _gradient_editor_init.Invoke(_gradient_editor, new object[] { gradient, 0, true, ColorSpace.Linear });
#else
            _gradient_editor_init.Invoke(_gradient_editor, new object[] { gradient, 0, true });
#endif
            UpdateGradientPreviewTexture();
        }

        void OnGUI()
        {
            if (!_inited)
                InitSomeStuff();
            float gradientEditorHeight = Mathf.Min(position.height, 146);
            float distBetween = 10f;
            float presetLibraryHeight = Mathf.Min(position.height - gradientEditorHeight - distBetween-135,130);

            Rect gradientEditorRect = new Rect(10, 10, position.width - 20, gradientEditorHeight - 20);
            Rect gradientLibraryRect = new Rect(0, gradientEditorHeight + distBetween, position.width, presetLibraryHeight);

            EditorGUI.BeginChangeCheck();
            _ongui.Invoke(_gradient_editor, new object[] { gradientEditorRect });
            if (EditorGUI.EndChangeCheck())
                UpdateGradientPreviewTexture();

            OverrideGradientTexture(gradientEditorRect);

            _preset_libary_onGUI.Invoke(_preset_libary_editor, new object[] { gradientLibraryRect, _data.Gradient });

            GUILayout.BeginVertical();
            GUILayout.Space(gradientEditorHeight+ presetLibraryHeight+ distBetween);
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Discard Changes",GUILayout.ExpandWidth(false)))
                DiscardChanges();
            GUILayout.EndHorizontal();
            if(_show_texture_options)
                TextureSettingsGUI();
        }

        private new void DiscardChanges()
        {
            _prop.textureValue = _privious_preview_texture;
            SetGradient(TextureHelper.GetGradient(_privious_preview_texture));
            _gradient_has_been_edited = false;
            ShaderEditor.RepaintActive();
        }

        private void TextureSettingsGUI()
        {
            EditorGUIUtility.labelWidth = 100;
            EditorGUIUtility.fieldWidth = 150;
            EditorGUILayout.LabelField("Texture options:",EditorStyles.boldLabel);
            bool changed = GUILib.GUIDataStruct<TextureData>(textureSettings, new string[]{"name"});
            if (changed)
            {
                FileHelper.SaveValueToFile("gradient_texture_options_" + _prop.name, Parser.Serialize(textureSettings), PATH.PERSISTENT_DATA);
                UpdateGradientPreviewTexture();
            }
        }

        private void UpdateGradientPreviewTexture()
        {
            _data.PreviewTexture = Converter.GradientToTexture(_data.Gradient, textureSettings.width, textureSettings.height);
            textureSettings.ApplyModes(_data.PreviewTexture);
            _prop.textureValue = _data.PreviewTexture;
            _gradient_has_been_edited = true;
            ShaderEditor.RepaintActive();
        }

        private void OverrideGradientTexture(Rect position)
        {
            Rect gradient_texture_position = new Rect(position);

            float modeHeight = 24f;
            float swatchHeight = 16f;
            float editSectionHeight = 26f;
            float gradientTextureHeight = gradient_texture_position.height - 2 * swatchHeight - editSectionHeight - modeHeight;
            gradient_texture_position.y += modeHeight;
            gradient_texture_position.y += swatchHeight;
            gradient_texture_position.height = gradientTextureHeight;


            Rect r2 = new Rect(gradient_texture_position.x + 1, gradient_texture_position.y + 1, gradient_texture_position.width - 2, gradient_texture_position.height - 2);

            Texture2D backgroundTexture = TextureHelper.GetBackgroundTexture();
            Rect texCoordsRect = new Rect(0, 0, r2.width / backgroundTexture.width, r2.height / backgroundTexture.height);
            GUI.DrawTextureWithTexCoords(r2, backgroundTexture, texCoordsRect, false);

            TextureWrapMode wrap_mode = _data.PreviewTexture.wrapMode;
            _data.PreviewTexture.wrapMode = TextureWrapMode.Clamp;
            GUI.DrawTexture(r2, _data.PreviewTexture, ScaleMode.StretchToFill, true);
            GUI.DrawTexture(gradient_texture_position, _data.PreviewTexture, ScaleMode.StretchToFill, false, 0, Color.grey, 1, 1);
            _data.PreviewTexture.wrapMode = wrap_mode;
        }

    }
}