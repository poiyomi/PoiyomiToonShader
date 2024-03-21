using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using static Thry.ThryEditor.ShaderTranslator;

namespace Thry.ThryEditor
{
    public class ShaderTranslator : ScriptableObject
    {
        public string Name;
        public string OriginShader;
        public string TargetShader;
        public bool MatchOriginShaderBasedOnRegex;
        public bool MatchTargetShaderBasedOnRegex;
        public string OriginShaderRegex;
        public string TargetShaderRegex;
        public List<PropertyTranslation> PropertyTranslations;

        [Serializable]
        public class PropertyTranslation
        {
            public string Origin;
            public string Target;
            public string Math;
        }

        public List<PropertyTranslation> GetPropertyTranslations()
        {
            if (PropertyTranslations == null)
            {
                PropertyTranslations = new List<PropertyTranslation>();
            }

            return PropertyTranslations;
        }

        public void Apply(ShaderEditor editor)
        {
            Shader originShader = Shader.Find(OriginShader);
            Shader targetShader = Shader.Find(TargetShader);
            SerializedObject serializedMaterial = new SerializedObject(editor.Materials[0]);

            foreach(PropertyTranslation trans in GetPropertyTranslations())
            {
                if (editor.PropertyDictionary.ContainsKey(trans.Target))
                {
                    SerializedProperty p;
                    switch (editor.PropertyDictionary[trans.Target].MaterialProperty.type)
                    {
                        case MaterialProperty.PropType.Float:
                        case MaterialProperty.PropType.Range:
                            p = GetProperty(serializedMaterial, "m_SavedProperties.m_Floats", trans.Origin);
                            if (p != null)
                            {
                                float f = p.FindPropertyRelative("second").floatValue;
                                if (string.IsNullOrWhiteSpace(trans.Math) == false) 
                                    f = Helper.SolveMath(trans.Math, f);
                                editor.PropertyDictionary[trans.Target].MaterialProperty.floatValue = f;
                            }
                            break;
#if UNITY_2022_1_OR_NEWER
                        case MaterialProperty.PropType.Int:
                            p = GetProperty(serializedMaterial, "m_SavedProperties.m_Ints", trans.Origin);
                            if (p != null)
                            {
                                float f = p.FindPropertyRelative("second").intValue;
                                if (string.IsNullOrWhiteSpace(trans.Math) == false) 
                                    f = Helper.SolveMath(trans.Math, f);
                                editor.PropertyDictionary[trans.Target].MaterialProperty.intValue = (int)f;
                            }
                            break;
#endif
                        case MaterialProperty.PropType.Vector:
                            p = GetProperty(serializedMaterial, "m_SavedProperties.m_Colors", trans.Origin);
                            if (p != null) editor.PropertyDictionary[trans.Target].MaterialProperty.vectorValue = p.FindPropertyRelative("second").vector4Value;
                            break;
                        case MaterialProperty.PropType.Color:
                            p = GetProperty(serializedMaterial, "m_SavedProperties.m_Colors", trans.Origin);
                            if (p != null) editor.PropertyDictionary[trans.Target].MaterialProperty.colorValue = p.FindPropertyRelative("second").colorValue;
                            break;
                        case MaterialProperty.PropType.Texture:
                            p = GetProperty(serializedMaterial, "m_SavedProperties.m_TexEnvs", trans.Origin);
                            if (p != null)
                            {
                                SerializedProperty values = p.FindPropertyRelative("second");
                                editor.PropertyDictionary[trans.Target].MaterialProperty.textureValue = 
                                    values.FindPropertyRelative("m_Texture").objectReferenceValue as Texture;
                                Vector2 scale = values.FindPropertyRelative("m_Scale").vector2Value;
                                Vector2 offset = values.FindPropertyRelative("m_Offset").vector2Value;
                                editor.PropertyDictionary[trans.Target].MaterialProperty.textureScaleAndOffset = 
                                    new Vector4(scale.x, scale.y , offset.x, offset.y);
                            }
                            break;
                    }
                }
            }
        }

        SerializedProperty GetProperty(SerializedObject o, string arrayPath, string propertyName)
        {
            SerializedProperty array = o.FindProperty(arrayPath);
            for(int i = 0; i < array.arraySize; i++)
            {
                if (array.GetArrayElementAtIndex(i).displayName == propertyName)
                {
                    return array.GetArrayElementAtIndex(i);
                }
            }
            return null;
        }

        static List<ShaderTranslator> s_translationDefinitions;
        static List<ShaderTranslator> TranslationDefinitions
        {
            get
            {
                if (s_translationDefinitions == null)
                    s_translationDefinitions = AssetDatabase.FindAssets("t:" + nameof(ShaderTranslator)).Select(
                        g => AssetDatabase.LoadAssetAtPath<ShaderTranslator>(AssetDatabase.GUIDToAssetPath(g))).ToList();
                return s_translationDefinitions;
            }
        }

        public static ShaderTranslator CheckForExistingTranslationFile(Shader origin, Shader target)
        {
            return TranslationDefinitions.FirstOrDefault(t => 
            t.MatchOriginShaderBasedOnRegex ? (Regex.IsMatch(origin.name, t.OriginShaderRegex)) : (t.OriginShader == origin.name) &&
            t.MatchTargetShaderBasedOnRegex ? (Regex.IsMatch(target.name, t.TargetShaderRegex)) : (t.TargetShader == target.name) );
        }

        public static void SuggestedTranslationButtonGUI(ShaderEditor editor)
        {
            if(editor.SuggestedTranslationDefinition != null)
            {
                GUILayoutUtility.GetRect(0, 5);
                Color backup = GUI.backgroundColor;
                GUI.backgroundColor = Color.green;
                if(GUILayout.Button($"Apply {editor.SuggestedTranslationDefinition.Name}"))
                {
                    editor.SuggestedTranslationDefinition.Apply(editor);
                    editor.SuggestedTranslationDefinition = null;
                }
                GUI.backgroundColor = backup;
                GUILayoutUtility.GetRect(0, 5);
            }
        }

        public static void TranslationSelectionGUI(ShaderEditor editor)
        {
            Rect r;
            if (GuiHelper.ButtonWithCursor(Styles.icon_style_shaders, "Shader Translation", 25, 25, out r))
            {
                EditorUtility.DisplayCustomMenu(r, TranslationDefinitions.Select(t => new GUIContent(t.Name)).ToArray(), -1, ConfirmTranslationSelection, editor);
            }
        }

        static void ConfirmTranslationSelection(object userData, string[] options, int selected)
        {
            TranslationDefinitions[selected].Apply(userData as ShaderEditor);
        }

        [MenuItem("Assets/Thry/ShaderTranslator/New Definition", false, 380)]
        public static void CreateNewTranslationDefinition()
        {
            ShaderTranslator shaderTranslator = CreateInstance<ShaderTranslator>();
            string path = UnityHelper.GetCurrentAssetExplorerFolder() + "/shaderTranslationDefinition.asset";
            AssetDatabase.CreateAsset(shaderTranslator, path);
            EditorGUIUtility.PingObject(shaderTranslator);
            TranslationDefinitions.Add(shaderTranslator);
        }
    }

    public class ShaderTranslatorSelecUI : EditorWindow
    {

    }

    [CustomEditor(typeof(ShaderTranslator))]
    public class ShaderTranslatorEditorUI : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            ShaderTranslator translator = serializedObject.targetObject as ShaderTranslator;

            translator.Name = EditorGUILayout.TextField("Translation File Name: " , translator.Name);

            GUILayout.Space(10);

            string[] shaders = AssetDatabase.FindAssets("t:shader").Select(g => AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath(g)).name).
                Where(s => s.StartsWith("Hidden") == false).ToArray();

            EditorGUI.BeginChangeCheck();
            int originIndex = EditorGUILayout.Popup("From Shader", Array.IndexOf(shaders, translator.OriginShader), shaders);
            if (EditorGUI.EndChangeCheck()) translator.OriginShader = shaders[originIndex];

            EditorGUI.BeginChangeCheck();
            int targetIndex = EditorGUILayout.Popup("To Shader", Array.IndexOf(shaders, translator.TargetShader), shaders);
            if (EditorGUI.EndChangeCheck()) translator.TargetShader = shaders[targetIndex];

            translator.MatchOriginShaderBasedOnRegex = EditorGUILayout.ToggleLeft(new GUIContent("Match Origin Shader Using Regex", 
                "Match the origin shader for suggestions based on a regex definition."), translator.MatchOriginShaderBasedOnRegex);
            if (translator.MatchOriginShaderBasedOnRegex)
                translator.OriginShaderRegex = EditorGUILayout.TextField("Origin Shader Regex", translator.OriginShaderRegex);
            translator.MatchTargetShaderBasedOnRegex = EditorGUILayout.ToggleLeft(new GUIContent("Match Target Shader Using Regex",
                "Match the target shader for suggestions based on a regex definition."), translator.MatchTargetShaderBasedOnRegex);
            if (translator.MatchTargetShaderBasedOnRegex)
                translator.TargetShaderRegex = EditorGUILayout.TextField("Target Shader Regex", translator.TargetShaderRegex);

            if (originIndex < 0 || targetIndex < 0)
            {
                EditorGUILayout.HelpBox("Could not find origin or target shader.", MessageType.Error);
                return;
            }

            Shader origin = Shader.Find(shaders[originIndex]);
            Shader target = Shader.Find(shaders[targetIndex]);

            GUILayout.Space(10);

            using (new GUILayout.VerticalScope("box"))
            {
                GUILayout.Label("Property Translation", EditorStyles.boldLabel);
                GUILayout.BeginHorizontal();
                GUILayout.Label("From");
                GUILayout.Label("To");
                GUILayout.Label("Math");
                GUILayout.EndHorizontal();
                List<PropertyTranslation> remove = new List<PropertyTranslation>();
                foreach (PropertyTranslation trans in translator.GetPropertyTranslations())
                {
                    Rect fullWidth = EditorGUILayout.GetControlRect();
                    Rect r = fullWidth;
                    r.width = (r.width - 20) / 3;
                    if (GUI.Button(r, trans.Origin)) GuiHelper.SearchableEnumPopup.CreateSearchableEnumPopup(
                         MaterialEditor.GetMaterialProperties(new UnityEngine.Object[] { new Material(origin) }).Select(p => p.name).ToArray(), trans.Origin,
                         (newValue) => trans.Origin = newValue);
                    r.x += r.width;
                    if (GUI.Button(r, trans.Target)) GuiHelper.SearchableEnumPopup.CreateSearchableEnumPopup(
                         MaterialEditor.GetMaterialProperties(new UnityEngine.Object[] { new Material(target) }).Select(p => p.name).ToArray(), trans.Target,
                         (newValue) => trans.Target = newValue);
                    r.x += r.width;
                    trans.Math = EditorGUI.TextField(r, trans.Math);
                    r.x += r.width;
                    r.width = 20;
                    if (GUI.Button(r, GUIContent.none, Styles.icon_style_remove)) remove.Add(trans);
                }

                foreach (PropertyTranslation r in remove)
                    translator.GetPropertyTranslations().Remove(r);

                Rect buttonRect = EditorGUILayout.GetControlRect();
                buttonRect.x = buttonRect.width - 20;
                buttonRect.width = 20;
                if (GUI.Button(buttonRect, GUIContent.none, Styles.icon_style_add)) translator.GetPropertyTranslations().Add(new PropertyTranslation());
            }

            serializedObject.Update();
            EditorUtility.SetDirty(serializedObject.targetObject);
        }
    }
}