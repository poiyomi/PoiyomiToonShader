using System;
using System.Collections.Generic;
using System.Linq;
using Poiyomi.ModularShaderSystem;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Poi.Tools
{
    [CustomEditor(typeof(PoiModulesTogglesModuleDictionary))]
    public class PoiModulesTogglesModuleDictionaryEditor : Editor
    {
        SerializedProperty propertyToggles;
        SerializedProperty shaderModules;
        ReorderableList listModules;
        private void OnEnable()
        {
            propertyToggles = serializedObject.FindProperty(nameof(PoiModulesTogglesModuleDictionary.propertyToggles));

            shaderModules = serializedObject.FindProperty(nameof(PoiModulesTogglesModuleDictionary.shaderModules));
            listModules = new ReorderableList(serializedObject, shaderModules, false, true, true, true)
            {
                elementHeight = EditorGUIUtility.singleLineHeight * 1.4f + EditorGUIUtility.singleLineHeight,
                drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "Modules"),
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    Rect objectRect = rect;
                    objectRect.height = EditorGUIUtility.singleLineHeight;
                    objectRect.y += 2f;
                    EditorGUI.ObjectField(objectRect, shaderModules.GetArrayElementAtIndex(index), GUIContent.none);
                    Rect propertyRect = rect;
                    propertyRect.height = EditorGUIUtility.singleLineHeight;
                    propertyRect.y += EditorGUIUtility.singleLineHeight * 1.4f;
                    propertyRect.y -= 2f;
                    EditorGUI.DelayedTextField(propertyRect, propertyToggles.GetArrayElementAtIndex(index), GUIContent.none);
                },
                onAddCallback = (ReorderableList list) =>
                {
                    shaderModules.arraySize += 1;
                    propertyToggles.arraySize += 1;
                },
                onRemoveCallback = (ReorderableList list) =>
                {
                    int index = list.index;
                    if (shaderModules.GetArrayElementAtIndex(index).objectReferenceValue != null)
                    {
                        shaderModules.DeleteArrayElementAtIndex(index); // Unity requires UnityEngine.Object Elements to be deleted twice!!
                    }
                    shaderModules.DeleteArrayElementAtIndex(index);
                    propertyToggles.DeleteArrayElementAtIndex(index);
                    list.index = Mathf.Max(index - 1, 0);
                }
            };
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            listModules.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
    public class PoiModulesTogglesModuleDictionaryAssetPostprocessor : AssetPostprocessor
    {
        public static ShaderModule[] modulesToLoad;
#if UNITY_2021_2_OR_NEWER
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
#else
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
#endif
        {
            HashSet<Material> materials = new HashSet<Material>();
            foreach (var assetPath in importedAssets)
            {
                if (assetPath.EndsWith(".mat"))
                {
                    materials.Add(AssetDatabase.LoadAssetAtPath<Material>(assetPath));
                }
            }
            if (materials.Count == 0) return;
            HashSet<ShaderModule> _modulesToLoad = new HashSet<ShaderModule>();
            var dictionary = AssetDatabase.LoadAssetAtPath<PoiModulesTogglesModuleDictionary>(AssetDatabase.GUIDToAssetPath("ad2095b27fb85bd4ca9a97769e75ffad"));
            (ShaderModule[] shaderModules, string[] propertyToggles) = dictionary.GetOnlyDisabledModules();
            if (shaderModules == null) return;

            string data = "";
            foreach (var mat in materials)
            {
                // I wish I didn't have to do this
                data = System.IO.File.ReadAllText(AssetDatabase.GetAssetPath(mat));
                if (data.IndexOf("shader_master_label") == -1) continue;
                for (int i = 0; i < propertyToggles.Length; i++)
                {
                    int index = data.IndexOf(propertyToggles[i]);
                    if (index != -1)
                    {
                        int start = index + propertyToggles[i].Length + 2;
                        int endex = data.IndexOf("\n", index);
                        if (float.TryParse(data.Substring(start, endex - start), out float result))
                        {
                            if (result > 0.0f)
                            {
                                _modulesToLoad.Add(shaderModules[i]);
                            }
                        }
                    }
                }
                // This is really slow
                // var serMat = new SerializedObject(mat);
                // var floatsArray = serMat.FindProperty("m_SavedProperties.m_Floats");
                // if (floatsArray != null && floatsArray.isArray)
                // {
                //     for (int i = 0; i < floatsArray.arraySize; i++)
                //     {
                //         var serprop = floatsArray.GetArrayElementAtIndex(i);
                //         for (int k = 0; k < propertyToggles.Length; k++)
                //         {
                //             if (propertyToggles[k] == serprop.displayName)
                //             {
                //                 if (serprop.FindPropertyRelative("second").floatValue > 0.0f)
                //                 {
                //                     _modulesToLoad.Add(shaderModules[k]);
                //                 }
                //             }
                //         }
                //     }
                // }
                // This doesn't work because Unity
                // if (mat.HasProperty("shader_master_label"))
                // {
                //     for (int i = 0; i < propertyToggles.Length; i++)
                //     {
                //         Debug.Log(propertyToggles[i]);
                //         Debug.Log(mat.HasProperty(propertyToggles[i]));
                //         if (mat.HasProperty(asd[i]))
                //         {
                //             if (mat.GetFloat(asd[i]) > 0.0f)
                //             {
                //                 _modulesToLoad.Add(shaderModules[i]);
                //             }
                //         }
                //     }
                // }
            }
            if (_modulesToLoad.Count != 0)
            {
                modulesToLoad = _modulesToLoad.ToArray();
                EditorApplication.delayCall += DialogPopup;
            }
            // try
            // {
            //     AssetDatabase.StartAssetEditing();
            // }
            // finally
            // {
            //     AssetDatabase.StopAssetEditing();
            //     AssetDatabase.Refresh();
            // }
        }

        private static void DialogPopup()
        {
            if (modulesToLoad == null) return;
            bool result = EditorUtility.DisplayDialog("Poiyomi Modules Toggles", "Imported Materials contain uses of Modules that are currently disabled\nWould you like to enable them?\nThis might take a long time", "Yes", "No");
            if (result)
            {
                var proModules = PoiModulesToggles.moduleSettings.proShaderModules.ToList();
                var freeModules = PoiModulesToggles.moduleSettings.freeShaderModules.ToList();
                foreach (var module in modulesToLoad)
                {
                    int index = proModules.IndexOf(module);
                    if (index != -1)
                    {
                        PoiModulesToggles.moduleSettings.proShaderModulesEnabled[index] = true;
                    }
                    index = freeModules.IndexOf(module);
                    if (index != -1)
                    {
                        PoiModulesToggles.moduleSettings.freeShaderModulesEnabled[index] = true;
                    }
                }
                PoiModulesToggles.Init();
                PoiModulesToggles.Apply(skipDialog: true);
            }
        }
    }
}
