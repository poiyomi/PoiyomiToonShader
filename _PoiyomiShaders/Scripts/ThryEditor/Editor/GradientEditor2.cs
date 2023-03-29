using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class GradientEditor2 : EditorWindow
    {
        private Gradient _gradient;
        private bool _allowSizeSelection;
        private Vector2Int _textureSizeMin;
        private Vector2Int _textureSizeMax;
        private Vector2Int _textureSize;
        private bool _makeTextureVertical;
        private Action<Gradient, Texture2D> _onGradientChanged;
        private object _gradientEditor;
        private object _gradientLibary;

        public static void Open(Gradient gradient, Action<Gradient, Texture2D> onGradientChanged, bool textureVertical, bool allowSizeSelection, Vector2Int minTextureSize, Vector2Int maxTextureSize)
        {
            var window = GetWindow<GradientEditor2>();
            window._gradient = gradient;
            window._allowSizeSelection = allowSizeSelection;
            window._textureSizeMin = minTextureSize;
            window._textureSizeMax = maxTextureSize;
            window._textureSize = minTextureSize;
            window._makeTextureVertical = textureVertical;
            window._onGradientChanged = onGradientChanged;
            window.titleContent = new GUIContent("Gradient Editor");
            // show in center of screen
            float width = 500;
            float height = 400;
            window.position = new Rect(Screen.width / 2 - width / 2, Screen.height / 2 - height / 2, width, height);
            window.Show();
        }

        static MethodInfo s_gradientEditorGUIMethodInfo = null;
        static MethodInfo GradientEditorGUI {
            get
            {
                if(s_gradientEditorGUIMethodInfo == null)
                {
                    Type gradient_editor_type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GradientEditor");
                    s_gradientEditorGUIMethodInfo = gradient_editor_type.GetMethod("OnGUI");
                }
                return s_gradientEditorGUIMethodInfo;
            }
        }

        static MethodInfo s_presetLibraryOnGUIMethodInfo = null;
        static MethodInfo PresetLibraryOnGUI 
        {
            get
            {
                if (s_presetLibraryOnGUIMethodInfo == null)
                {
                    Type gradient_preset_libary_type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GradientPresetLibrary");
                    Type preset_libary_editor_type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.PresetLibraryEditor`1");
                    Type gradient_preset_libary_editor_type = preset_libary_editor_type.MakeGenericType(gradient_preset_libary_type);
                    s_presetLibraryOnGUIMethodInfo = gradient_preset_libary_editor_type.GetMethod("OnGUI");
                }
                return s_presetLibraryOnGUIMethodInfo;
            }
        }

        static object GetGradientEditor(Gradient gradient)
        {
            Type gradientEditorType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GradientEditor");
            var gradientEditor = Activator.CreateInstance(gradientEditorType);
            SetGradient(gradientEditor, gradient);
            return gradientEditor;
        }

        static void SetGradient(object gradientEditor, Gradient gradient)
        {
            Type gradientEditorType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GradientEditor");
            var gradientEditorInit = gradientEditorType.GetMethod("Init");

#if UNITY_2020_1_OR_NEWER
            gradientEditorInit.Invoke(gradientEditor, new object[] { gradient, 0, true, ColorSpace.Linear });
#else
            gradientEditorInit.Invoke(gradientEditor, new object[] { gradient, 0, true });
#endif
        }

        public static object GetGradientLibary(Action<int, object> presetSelectedCallback)
        {
            Type presetLibraryEditorState_type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.PresetLibraryEditorState");
            var preset_libary_editor_state = Activator.CreateInstance(presetLibraryEditorState_type, "Gradient");
            MethodInfo transfer_editor_prefs_state = presetLibraryEditorState_type.GetMethod("TransferEditorPrefsState");
            transfer_editor_prefs_state.Invoke(preset_libary_editor_state, new object[] { true });

            Type scriptable_save_load_helper_type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ScriptableObjectSaveLoadHelper`1");
            Type gradient_preset_libary_type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GradientPresetLibrary");
            Type preset_libary_editor_type = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.PresetLibraryEditor`1");
            Type save_load_helper_type = scriptable_save_load_helper_type.MakeGenericType(gradient_preset_libary_type);
            Type gradient_preset_libary_editor_type = preset_libary_editor_type.MakeGenericType(gradient_preset_libary_type);

            object saveLoadHelper = Activator.CreateInstance(save_load_helper_type, "gradients", SaveType.Text);

            var preset_libary_editor = Activator.CreateInstance(gradient_preset_libary_editor_type, saveLoadHelper, preset_libary_editor_state, presetSelectedCallback);
            PropertyInfo show_header = gradient_preset_libary_editor_type.GetProperty("showHeader");
            show_header.SetValue(preset_libary_editor, true, null);
            PropertyInfo minMaxPreviewHeight = gradient_preset_libary_editor_type.GetProperty("minMaxPreviewHeight");
            minMaxPreviewHeight.SetValue(preset_libary_editor, new Vector2(14f, 14f), null);

            return preset_libary_editor;
        }

        private void OnGUI()
        {
            if(_gradientEditor == null) _gradientEditor = GetGradientEditor(_gradient);
            if(_gradientLibary == null) _gradientLibary = GetGradientLibary(PresetHasBeenSelected);

            int settingsHeight = 70;
            if(_allowSizeSelection) settingsHeight += 50;
            
            Rect mainUIRect = new Rect(20, 20, position.width - 40, position.height - settingsHeight);
            Rect gradientRect = new Rect(mainUIRect.x, mainUIRect.y, mainUIRect.width, mainUIRect.height * 0.6f);
            Rect presetRect = new Rect(mainUIRect.x, gradientRect.yMax + 10, mainUIRect.width, mainUIRect.height * 0.4f - 10);

            GradientEditorGUI.Invoke(_gradientEditor, new object[] { gradientRect });
            PresetLibraryOnGUI.Invoke(_gradientLibary, new object[] { presetRect, _gradient });

            Rect settingsRect = new Rect(20, position.height - settingsHeight, position.width - 40, settingsHeight);
            Rect buttonRect = new Rect(settingsRect.x + 20, settingsRect.yMax - 40, settingsRect.width - 20, 30);
            if(_allowSizeSelection)
            {
                Rect sizeRect = new Rect(settingsRect.x + 20, settingsRect.y + 30, settingsRect.width - 20, 30);
                _textureSize = EditorGUI.Vector2IntField(sizeRect, "Texture Size", _textureSize);
            }
            if (GUI.Button(buttonRect, "Apply"))
            {
                Apply();
            }
        }

        private void PresetHasBeenSelected(int index, object preset)
        {
            Gradient gradient = preset as Gradient;
            if (gradient == null)
                Debug.LogError("Incorrect object passed " + preset);
            // copy the data from the preset to the gradient
            _gradient.SetKeys(gradient.colorKeys, gradient.alphaKeys);
            _gradient.mode = gradient.mode;
            SetGradient(_gradientEditor, _gradient);
            Apply();
        }

        private void OnDestroy()
        {
            Apply();
        }

        void Apply()
        {
            Texture2D gradientTexture = Converter.GradientToTexture(_gradient, _textureSize.x, _textureSize.y, _makeTextureVertical);
            _onGradientChanged?.Invoke(_gradient, gradientTexture);
        }
    }
}