using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Thry.ThryEditor.Helpers;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using UnityEngine.Serialization;

namespace Thry.ThryEditor.ShaderTranslations
{
    public partial class ShaderTranslator : ScriptableObject
    {
        public string Name;
        public string OriginShader;
        public string TargetShader;
        public bool MatchOriginShaderBasedOnRegex;
        public bool MatchTargetShaderBasedOnRegex;
        public string OriginShaderRegex;
        public string TargetShaderRegex;
        public List<ShaderTranslationsContainer> PropertyTranslationContainers;
        public List<ShaderNameMatchedModifications> PreTranslationPropertyModifications;
        [FormerlySerializedAs("PropertyModifications")] public List<ShaderNameMatchedModifications> PostTranslationPropertyModifications;

        public List<PropertyTranslation> AllPropertyTranslations => PropertyTranslationContainers.SelectMany(x => x.PropertyTranslations).ToList();

        public void Apply(ShaderEditor editor, int? renderQueueOverride = null)
        {
            Shader originShader = editor.LastShader;
            Shader targetShader = editor.Shader;
            Material material = editor.Materials[0];
            SerializedObject serializedMaterial = new SerializedObject(material);

            List<PropertyTranslation> allTranslations = AllPropertyTranslations;

            _HandlePropertyModifications(editor, originShader, PreTranslationPropertyModifications);

            foreach(PropertyTranslation trans in allTranslations)
            {
                if(editor.PropertyDictionary.TryGetValue(trans.Target, out ShaderProperty targetProp))
                {
                    SerializedProperty p;
                    switch(targetProp.MaterialProperty.type)
                    {
                        case MaterialProperty.PropType.Float:
                        case MaterialProperty.PropType.Range:
                            p = GetProperty(serializedMaterial, "m_SavedProperties.m_Floats", trans.Origin);
                            if(p != null)
                            {
                                _HandleFloatProperty(editor, trans, p);
                                break;
                            }
                            // Convert a texture property to a 1 if assigned and to 0 if not
                            p = GetProperty(serializedMaterial, "m_SavedProperties.m_TexEnvs", trans.Origin);
                            if(p != null)
                            {
                                float textureValue = p.FindPropertyRelative("second").FindPropertyRelative("m_Texture") != null ? 1f : 0f;
                                string expression = trans.GetAppropriateExpression(textureValue);
                                if(!string.IsNullOrWhiteSpace(expression))
                                {
                                    // If we can parse the expression then our expression is just a number. Replace old value with ours
                                    if(float.TryParse(expression, out float result))
                                        textureValue = result;
                                    else
                                        textureValue = Helper.SolveMath(trans.Math, textureValue);
                                }
                                editor.PropertyDictionary[trans.Target].FloatValue = textureValue;
                            }
                            break;
#if UNITY_2022_1_OR_NEWER
                        case MaterialProperty.PropType.Int:
                            p = GetProperty(serializedMaterial, "m_SavedProperties.m_Ints", trans.Origin);
                            if(p != null)
                            {
                                _HandleIntProperty(editor, trans, p);
                                break;
                            }

                            p = GetProperty(serializedMaterial, "m_SavedProperties.m_Floats", trans.Origin);
                            if(p != null) // I'm sorry but I don't have time to not-copy paste this
                            {
                                float f = p.FindPropertyRelative("second").floatValue;
                                string expression = trans.GetAppropriateExpression(f);
                                if(!string.IsNullOrWhiteSpace(expression))
                                {
                                    // If we can parse the expression then our expression is just a number. Replace old value with ours
                                    if(float.TryParse(expression, out float result))
                                        f = result;
                                    else
                                        f = Helper.SolveMath(trans.Math, f);
                                }
                                editor.PropertyDictionary[trans.Target].FloatValue = (int)f;
                                break;
                            }
                            // Convert a texture property to a 1 if assigned and to 0 if not
                            p = GetProperty(serializedMaterial, "m_SavedProperties.m_TexEnvs", trans.Origin);
                            if(p != null)
                            {
                                float textureValue = p.FindPropertyRelative("second").FindPropertyRelative("m_Texture") != null ? 1f : 0f;
                                string expression = trans.GetAppropriateExpression(textureValue);
                                if(!string.IsNullOrWhiteSpace(expression))
                                {
                                    // If we can parse the expression then our expression is just a number. Replace old value with ours
                                    if(float.TryParse(expression, out float result))
                                        textureValue = result;
                                    else
                                        textureValue = Helper.SolveMath(trans.Math, textureValue);
                                }
                                editor.PropertyDictionary[trans.Target].FloatValue = (int)textureValue;
                            }
                            break;
#endif
                        case MaterialProperty.PropType.Vector:
                            p = GetProperty(serializedMaterial, "m_SavedProperties.m_Colors", trans.Origin);
                            if(p != null) editor.PropertyDictionary[trans.Target].VectorValue = p.FindPropertyRelative("second").vector4Value;
                            break;
                        case MaterialProperty.PropType.Color:
                            p = GetProperty(serializedMaterial, "m_SavedProperties.m_Colors", trans.Origin);
                            if(p != null) editor.PropertyDictionary[trans.Target].ColorValue = p.FindPropertyRelative("second").colorValue;
                            break;
                        case MaterialProperty.PropType.Texture:
                            p = GetProperty(serializedMaterial, "m_SavedProperties.m_TexEnvs", trans.Origin);
                            if(p != null)
                            {
                                SerializedProperty values = p.FindPropertyRelative("second");
                                editor.PropertyDictionary[trans.Target].TextureValue =
                                    values.FindPropertyRelative("m_Texture").objectReferenceValue as Texture;
                                Vector2 scale = values.FindPropertyRelative("m_Scale").vector2Value;
                                Vector2 offset = values.FindPropertyRelative("m_Offset").vector2Value;
                                editor.PropertyDictionary[trans.Target].MaterialProperty.textureScaleAndOffset =
                                    new Vector4(scale.x, scale.y, offset.x, offset.y);
                            }
                            break;
                    }
                }
            }

            // Post translation modifications
            _HandlePropertyModifications(editor, originShader, PostTranslationPropertyModifications);

            serializedMaterial.ApplyModifiedProperties();

            if(renderQueueOverride != null)
                material.renderQueue = (int)renderQueueOverride;

            ShaderEditor.FixKeywords(new Material[] { material });

            void _HandleFloatProperty(ShaderEditor _editor, PropertyTranslation trans, SerializedProperty p)
            {
                float f = p.FindPropertyRelative("second").floatValue;
                string expression = trans.GetAppropriateExpression(f);

                if(!string.IsNullOrWhiteSpace(expression))
                {
                    // If we can parse the expression then our expression is just a number. Replace old value with ours
                    if(float.TryParse(expression, out float result))
                        f = result;
                    else
                        f = Helper.SolveMath(trans.Math, f);
                }
                _editor.PropertyDictionary[trans.Target].FloatValue = f;
            }

#if UNITY_2022_1_OR_NEWER
            void _HandleIntProperty(ShaderEditor _editor, PropertyTranslation trans, SerializedProperty p)
            {
                float f = p.FindPropertyRelative("second").intValue;
                string expression = trans.GetAppropriateExpression(f);
                if(!string.IsNullOrWhiteSpace(expression))
                {
                    // If we can parse the expression then our expression is just a number. Replace old value with ours
                    if(float.TryParse(expression, out float result))
                        f = result;
                    else
                        f = Helper.SolveMath(trans.Math, f);
                }
                _editor.PropertyDictionary[trans.Target].FloatValue = (int)f;
            }
#endif

            void _HandlePropertyModifications(ShaderEditor _editor, Shader _originShader, List<ShaderNameMatchedModifications> modifications)
            {
                foreach(var mod in modifications)
                {
                    if(!mod.IsShaderNameMatch(_originShader.name))
                        continue;

                    foreach(var action in mod.propertyModifications)
                    {
                        switch(action.actionType)
                        {
                            case ShaderModificationAction.ActionType.ChangeTargetShader:
                                Shader newShader = Shader.Find(action.targetValue);
                                if(newShader)
                                    _editor.Materials[0].shader = newShader;
                                break;
                            case ShaderModificationAction.ActionType.SetTargetPropertyValue:
                                if(float.TryParse(action.targetValue, out float parsedFloat))
                                {
                                    if(action.propertyName == ShaderEditor.PROPERTY_NAME_IN_SHADER_PRESETS)
                                    {
                                        _editor.ShaderRenderingPreset = parsedFloat;
                                    }
                                    else
                                        SetPropertyValue(_editor, action.propertyName, parsedFloat);
                                }
                                break;
                        }
                    }
                }
            }
        }

        void SetPropertyValue(ShaderEditor editor, string propertyName, float value)
        {
            if(!editor.PropertyDictionary.TryGetValue(propertyName, out var prop))
                return;

            switch(prop.MaterialProperty.type)
            {
                case MaterialProperty.PropType.Float:
#if UNITY_2021_1_OR_NEWER
                case MaterialProperty.PropType.Int:
#endif
                    prop.MaterialProperty.SetNumber(value);
                    break;
                // If our property is 0f, clear texture
                case MaterialProperty.PropType.Texture:
                    if(Convert.ToInt32(value) == 0)
                        prop.MaterialProperty.textureValue = null;
                    break;
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
        public static List<ShaderTranslator> TranslationDefinitions
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
                    editor.ApplySuggestedTranslationDefinition();
                }
                GUI.backgroundColor = backup;
                GUILayoutUtility.GetRect(0, 5);
            }
        }

        public static void TranslationSelectionGUI(Rect r, ShaderEditor editor)
        {
            if (GUILib.ButtonWithCursor(r, Icons.shaders, "Shader Translation"))
            {
                EditorUtility.DisplayCustomMenu(r, TranslationDefinitions.Select(t => new GUIContent(t.Name)).ToArray(), -1, ConfirmTranslationSelection, editor);
            }
        }

        static void ConfirmTranslationSelection(object userData, string[] options, int selected)
        {
            TranslationDefinitions[selected].Apply(userData as ShaderEditor);
        }

        [MenuItem("Assets/Thry/Shaders/New Translator Definition", priority = 380)]
        static void CreateNewTranslationDefinition()
        {
            // This allows you to name your asset before creating it
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
               0,
               CreateInstance<DoCreateNewTranslationDefinition>(),
               "New Translation Definition.asset",
               EditorGUIUtility.IconContent("ScriptableObject Icon").image as Texture2D,
               null);
        }

        class DoCreateNewTranslationDefinition : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                var translator = CreateInstance<ShaderTranslator>();
                translator.name = Path.GetFileNameWithoutExtension(pathName);
                AssetDatabase.CreateAsset(translator, pathName);
                Selection.activeObject = translator;
                TranslationDefinitions.Add(translator);
            }
        }
    }
}
