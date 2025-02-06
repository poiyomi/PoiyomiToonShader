using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Poi.Tools
{
    [CustomEditor(typeof(Material))]
    public class ErrorShaderEditor : MaterialEditor
    {
        const string PoiyomiProUrl = "https://www.poiyomi.com/download#poiyomi-pro";
        const string ErrorShaderName = "Hidden/InternalErrorShader";
        const string LockedMaterialText = "Generated shader for this locked Poiyomi material is missing.\nYou can unlock this material by clicking below.";
        const string LockedMaterialText_Pro = "Generated shader for this locked Poiyomi Pro material is missing.\nYou can unlock this material by clicking below.";
        const string LockedMaterialText_ProMissing = "Generated shader for this locked Poiyomi Pro material is missing.\nTo unlock this material you need Poiyomi Pro which is missing from your project.";
        const string UnlockedMaterialText_ProMissing = "Poiyomi Pro material detected.\nTo use this material you need Poiyomi Pro which is missing from your project.";
        const string UnlockedMaterialText = "Poiyomi material detected.\nNot sure what happened but you can try to switch the shader to the latest toon below.";

        string originalShader;
        bool isErrorShader;
        bool isPoiyomiMaterial;
        bool isProMaterial;
        bool isLockedMaterial;

        static bool? ProjectHasPro
        {
            get
            {
                if(_projectHasPro == null)
                {
                    _projectHasPro = ShaderUtil.GetAllShaderInfo()
                        .Select(info => info.name)
                        .Where(name => !name.StartsWith("Hidden/"))
                        .Any(name => name.Contains("Poiyomi Pro"));
                }
                return (bool)_projectHasPro;
            }
        }
        static bool? _projectHasPro;

        Material targetMaterial;

        public override void Awake()
        {
            base.Awake();
            Initialize();
        }

        void Initialize()
        {
            targetMaterial = target as Material;

            isErrorShader = targetMaterial.shader.name == ErrorShaderName;

            if(!isErrorShader)
                return;

            originalShader = targetMaterial.GetTag("OriginalShader", false);

            isPoiyomiMaterial = !string.IsNullOrWhiteSpace(originalShader);

            if(!isPoiyomiMaterial)
                return;

            SerializedProperty floatProps = serializedObject.FindProperty("m_SavedProperties").FindPropertyRelative("m_Floats");
            for(int i = 0; i < floatProps.arraySize; i++)
            {
                var prop = floatProps.GetArrayElementAtIndex(i);
                if(prop.displayName == "_ShaderOptimizerEnabled")
                {
                    isLockedMaterial = Convert.ToBoolean(prop.FindPropertyRelative("second").floatValue);
                    break;
                }
            }
            //Unity 2019 doesn't have .Contains(string, StringComparison) so using .IndexOf() instead
            isProMaterial = originalShader.IndexOf("Poiyomi Pro", System.StringComparison.CurrentCultureIgnoreCase) != -1;
        }

        public override void OnInspectorGUI()
        {
            if(!isErrorShader || !isPoiyomiMaterial)
            {
                base.OnInspectorGUI();
                return;
            }

            EditorGUILayout.BeginVertical();

            if(isLockedMaterial)
            {
                if(isProMaterial)
                {
                    if((bool)ProjectHasPro)
                    {
                        EditorGUILayout.HelpBox(LockedMaterialText_Pro, MessageType.Warning);
                        if(GUILayout.Button("Unlock Material"))
                            SwitchShader(originalShader);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox(LockedMaterialText_ProMissing, MessageType.Warning);
                        if(GUILayout.Button("More info"))
                            Application.OpenURL(PoiyomiProUrl);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox(LockedMaterialText, MessageType.Warning);
                    if(GUILayout.Button("Unlock Material"))
                        SwitchShader(originalShader);
                }
            }
            else
            {
                if(isProMaterial && !(bool)ProjectHasPro)
                {
                    EditorGUILayout.HelpBox(UnlockedMaterialText_ProMissing, MessageType.Warning);
                    if(GUILayout.Button("More info"))
                        Application.OpenURL(PoiyomiProUrl);
                }
                else if(!isProMaterial)
                {
                    EditorGUILayout.HelpBox(UnlockedMaterialText, MessageType.Warning);
                    if(GUILayout.Button("Switch to latest Toon"))
                        SwitchShader(".poiyomi/Poiyomi Toon");
                }
            }

            EditorGUILayout.EndVertical();
        }

        void SwitchShader(string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            if(!shader)
            {
                Debug.LogError($"Couldn't find shader {shaderName} in the project.");
                return;
            }

            serializedObject.FindProperty("m_Shader").objectReferenceValue = shader;
            serializedObject.ApplyModifiedProperties();
            Initialize();
        }
    }
}
