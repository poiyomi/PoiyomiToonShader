using UnityEngine;
using UnityEditor;
using System.Linq;
using Poiyomi.ModularShaderSystem;
using System.Collections.Generic;
using System.IO;
using Thry;
using UnityEditorInternal;

namespace Poi.Tools.ModularShaderSystem
{
    public class ModularShadersForThryEditor
    {
        private static bool IsExpanded = false;
        private static readonly Dictionary<string, ReorderableList> reorderableLists = new Dictionary<string, ReorderableList>();
        // TODO:
        // more invasive export checking
        const string shaderIsCustomProp = "shader_is_custom";
        public static void GUICustomPoiMSS(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor, ShaderEditor thryEditor)
        {
            int isCustomPropIndex = thryEditor.Shader.FindPropertyIndex(shaderIsCustomProp);
            if (isCustomPropIndex != -1)
            {
                string guid = thryEditor.Shader.GetPropertyDescription(isCustomPropIndex);
                string customShaderDirectory = Path.Combine("Assets", "_PoiyomiShadersCustom", guid);
                ModularShadersForThryEditorObject msfteo = AssetDatabase.LoadAssetAtPath<ModularShadersForThryEditorObject>(Path.Combine(customShaderDirectory, $"Settings-{guid}.asset"));

                // GUI
                Rect headerRect = EditorGUILayout.GetControlRect();
                if (headerRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                {
                    IsExpanded = !IsExpanded;
                    Event.current.Use();
                }
                GUI.Box(headerRect, new GUIContent("Custom Shader"), Thry.ThryEditor.Styles.dropdownHeader);
                if (Event.current.type == EventType.Repaint)
                {
                    var toggleRect = new Rect(headerRect.x + 4f, headerRect.y + 2f, 13f, 13f);
                    EditorStyles.foldout.Draw(toggleRect, false, false, IsExpanded, false);
                }
                if (IsExpanded)
                {
                    using (new EditorGUI.DisabledGroupScope(thryEditor.IsLockedMaterial))
                    {
                        EditorGUI.indentLevel++;
                        EditorGUI.BeginChangeCheck();
                        string shaderName = EditorGUILayout.TextField("Shader Name", msfteo.Name);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(msfteo, "Change Custom Shader Name");
                            msfteo.Name = string.Join("_", shaderName.Split(Path.GetInvalidFileNameChars()));
                            string newShaderName = msfteo.OriginalModularShader.ShaderPath.Insert(msfteo.OriginalModularShader.ShaderPath.LastIndexOf('/') + 1, "Custom/");
                            msfteo.ModularShader.ShaderPath = newShaderName + " - " + msfteo.Name;
                            EditorUtility.SetDirty(msfteo);
                        }

                        if (!reorderableLists.ContainsKey(guid) || reorderableLists[guid] == null)
                        {
                            reorderableLists.Remove(guid);
                            ReorderableList rlist = new ReorderableList(msfteo.Modules, typeof(ShaderModule), true, true, true, true);
                            rlist.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "Modules");
                            rlist.elementHeight = EditorGUIUtility.singleLineHeight * 1.4f;
                            rlist.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                            {
                                rect.height = 21f;
                                rect.y += 2f;
                                EditorGUI.BeginChangeCheck();
                                ShaderModule shaderModule = EditorGUI.ObjectField(rect, GUIContent.none, msfteo.Modules[index], typeof(ShaderModule), false) as ShaderModule;
                                if (EditorGUI.EndChangeCheck())
                                {
                                    Undo.RecordObject(msfteo, "Change Custom Shader Module");
                                    msfteo.Modules[index] = shaderModule;
                                    EditorUtility.SetDirty(msfteo);
                                }
                            };
                            rlist.onAddCallback = (ReorderableList list) =>
                            {
                                list.list.Add(null);
                            };
                            reorderableLists.Add(guid, rlist);
                        }
                        Rect listRect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false, reorderableLists[guid].GetHeight()));
                        EditorGUI.indentLevel--;
                        reorderableLists[guid].DoList(listRect);
                        EditorGUI.indentLevel++;

                        var _shaderName = msfteo.Shader.name.Substring(msfteo.OriginalModularShader.ShaderPath.Length + 3 + "Custom/".Length);
                        if (!_shaderName.Equals(msfteo.Name) || !AreModulesListsTheSame(msfteo.ModularShader.AdditionalModules, msfteo.Modules.Distinct().Where(x => x != null).ToList()))
                        {
                            EditorGUILayout.HelpBox("You have unsaved changes, please hit Generate to update them!", MessageType.Warning);
                        }

                        Rect buttonRect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
                        if (GUI.Button(buttonRect, "Generate"))
                        {
                            msfteo.ModularShader.AdditionalModules = msfteo.Modules.Distinct().Where(x => x != null).ToList();
                            AssetDatabase.SaveAssets();
                            ShaderGenerator.GenerateShader(customShaderDirectory, msfteo.ModularShader);
                            thryEditor.Reload();
                        }
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.Separator();
            }
            else
            {
                if (GUILayout.Button("Create Custom Shader"))
                {
                    var a = ScriptableObject.CreateInstance<MSFTEWindow>();
                    a.titleContent = new GUIContent("Poi Custom Shader Maker");
                    a.thryEditor = thryEditor;
                    a.ShowUtility();
                    a.position = new Rect(GUIUtility.GUIToScreenPoint(Event.current.mousePosition).x, GUIUtility.GUIToScreenPoint(Event.current.mousePosition).y, a.position.width, a.position.height);
                }
            }
        }
        private static bool AreModulesListsTheSame(List<ShaderModule> comp1, List<ShaderModule> comp2)
        {
            if (comp1.Count != comp2.Count)
                return false;
            var ids1 = comp1.Select(x => x.Id).ToList();
            var ids2 = comp2.Select(x => x.Id).ToList();
            if (ids1.Except(ids2).Any() && ids2.Except(ids1).Any())
                return false;
            return true;
        }
        private class MSFTEWindow : EditorWindow
        {

            public ShaderEditor thryEditor;
            private bool init = false;
            private string guid;
            private string shaderName;
            private ModularShader modularShader;
            private List<ShaderModule> modules = new List<ShaderModule>();
            private ReorderableList rlist;

            void Init()
            {
                if (init) return;
                init = true;
                guid = GUID.Generate().ToString().Substring(20);
                shaderName = guid;
                modularShader = AssetDatabase.FindAssets("t:ModularShader")
                                             .Select(x => AssetDatabase.GUIDToAssetPath(x))
                                             .Select(x => AssetDatabase.LoadAssetAtPath<ModularShader>(x))
                                             .Where(x => x.LastGeneratedShaders.Contains(thryEditor.Shader))
                                             .FirstOrDefault();
                rlist = new ReorderableList(modules, typeof(ShaderModule), true, true, true, true);
                rlist.drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, "Modules");
                rlist.elementHeight = EditorGUIUtility.singleLineHeight * 1.4f;
                rlist.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    rect.height = 21f;
                    rect.y += 2f;
                    EditorGUI.BeginChangeCheck();
                    ShaderModule shaderModule = EditorGUI.ObjectField(rect, GUIContent.none, modules[index], typeof(ShaderModule), false) as ShaderModule;
                    if (EditorGUI.EndChangeCheck())
                    {
                        modules[index] = shaderModule;
                    }
                };
                rlist.onAddCallback = (ReorderableList list) =>
                {
                    list.list.Add(null);
                };
            }

            void OnGUI()
            {
                Init();
                // EditorGUILayout.LabelField("GUID", guid);
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.ObjectField("Modular Shader", modularShader, typeof(ModularShader), false, null);
                }
                shaderName = EditorGUILayout.TextField("Shader Name", shaderName);

                rlist.DoLayoutList();

                if (modularShader == null)
                {
                    EditorGUILayout.HelpBox("Modular Shader is null (None) cannot create custom shader", MessageType.Error);
                }

                EditorGUILayout.BeginHorizontal();
                using (new EditorGUI.DisabledGroupScope(modularShader == null))
                {
                    if (GUILayout.Button("Create"))
                    {
                        try
                        {
                            Create();
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                        finally
                        {
                            EditorUtility.ClearProgressBar();
                        }
                    }
                }
                if (GUILayout.Button("Cancel"))
                {
                    this.Close();
                }
                EditorGUILayout.EndHorizontal();
            }

            private void Create()
            {
                if (!Directory.Exists(Path.Combine("Assets", "_PoiyomiShadersCustom")))
                {
                    Directory.CreateDirectory(Path.Combine("Assets", "_PoiyomiShadersCustom"));
                }
                string customShaderDirectory = Path.Combine("Assets", "_PoiyomiShadersCustom", guid);
                Directory.CreateDirectory(customShaderDirectory);

                EditorUtility.DisplayProgressBar("Creating Custom Shader", "Creating Template", 0.0f);
                string customShaderPropertyPath = Path.Combine(customShaderDirectory, $"CustomShaderProperty-{guid}.poiTemplate");
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<TemplateAsset>(), customShaderPropertyPath);
                // AssetDatabase.ImportAsset(customShaderPropertyPath, ImportAssetOptions.ForceUpdate);
                // Unity likes to shove the meta data into the ScriptableObject's Template for some reason?
                TemplateAsset customShaderPropTemplate = AssetDatabase.LoadAssetAtPath<TemplateAsset>(customShaderPropertyPath);
                customShaderPropTemplate.Template = $"[HideInInspector] {shaderIsCustomProp} (\"{guid}\", Int) = 0";

                string newModularShaderPath = Path.Combine(customShaderDirectory, modularShader.name + "-" + guid + ".asset");
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(modularShader), newModularShaderPath);

                AssetDatabase.ImportAsset(newModularShaderPath, ImportAssetOptions.ForceUpdate);

                ModularShader newModularShader = AssetDatabase.LoadAssetAtPath<ModularShader>(newModularShaderPath);
                newModularShader.Id += "-" + guid;
                newModularShader.Name += " - " + guid;
                string newShaderName = newModularShader.ShaderPath.Insert(newModularShader.ShaderPath.LastIndexOf('/') + 1, "Custom/");
                newModularShader.ShaderPath = $"{newShaderName} - {shaderName}";
                newModularShader.LastGeneratedShaders = new List<Shader>(0);
                newModularShader.Description = "Custom Shader";

                newModularShader.ShaderPropertiesTemplate = customShaderPropTemplate;
                newModularShader.AdditionalModules = modules.Distinct().Where(x => x != null).ToList();

                AssetDatabase.SaveAssets();
                EditorUtility.DisplayProgressBar("Creating Custom Shader", "Generating new Modular Shader", 0.2f);
                ShaderGenerator.GenerateShader(customShaderDirectory, newModularShader);
                AssetDatabase.SaveAssets();

                Shader newShader = newModularShader.LastGeneratedShaders.FirstOrDefault();

                ModularShadersForThryEditorObject msfteo = ScriptableObject.CreateInstance<ModularShadersForThryEditorObject>();
                msfteo.OriginalModularShader = modularShader;
                msfteo.ModularShader = newModularShader;
                msfteo.Shader = newShader;
                msfteo.Name = shaderName;
                msfteo.Modules = modules;
                EditorUtility.DisplayProgressBar("Creating Custom Shader", "Creating Settings Asset", 0.9f);
                AssetDatabase.CreateAsset(msfteo, Path.Combine(customShaderDirectory, $"Settings-{guid}.asset"));
                AssetDatabase.SaveAssets();
                foreach (var mat in thryEditor.Materials)
                {
                    mat.shader = newShader;
                }
                EditorUtility.DisplayProgressBar("Creating Custom Shader", "Reloading ThryEditor", 0.99f);
                thryEditor.Reload();
                this.Close();
            }
        }
    }
}