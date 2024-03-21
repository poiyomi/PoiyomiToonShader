using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class CrossEditor : EditorWindow
    {
        [MenuItem("Thry/Cross Shader Editor", priority = -20)]
        public static void ShowWindow()
        {
            CrossEditor window = EditorWindow.GetWindow(typeof(CrossEditor)) as CrossEditor;
            window.name = "Cross Shader Editor";
        }

        [MenuItem("Assets/Thry/Materials/Open in Cross Shader Editor", false , 400)]
        public static void OpenInCrossShaderEditor()
        {
            CrossEditor window = EditorWindow.GetWindow(typeof(CrossEditor)) as CrossEditor;
            window.name = "Cross Shader Editor";
            window._materialList = Selection.objects.Where(o => o is Material).Cast<Material>().ToList();
            window.UpdateTargets();
            window._shaderEditor = null;
        }

        [MenuItem("Assets/Thry/Materials/Open in Cross Shader Editor", true, 400)]
        public static bool OpenInCrossShaderEditorValidation()
        {
            return Selection.objects.All(o => o is Material);
        }

        [MenuItem("GameObject/Thry/Materials/Open All in Cross Shader Editor", false, 10)]
        public static void OpenAllInCrossShaderEditor()
        {
            CrossEditor window = EditorWindow.GetWindow(typeof(CrossEditor)) as CrossEditor;
            window.name = "Cross Shader Editor";
            window._materialList = Selection.gameObjects.SelectMany(o => o.GetComponentsInChildren<Renderer>(true)).SelectMany(r => r.sharedMaterials).ToList();
            window.UpdateTargets();
            window._shaderEditor = null;
        }

        List<Material> _materialList = new List<Material>();
        List<Material> _targets = new List<Material>();
        Dictionary<Material,Shader> _targetShaders = new Dictionary<Material, Shader>();
        ShaderEditor _shaderEditor = null;
        MaterialEditor _materialEditor = null;
        MaterialProperty[] _materialProperties = null;
        Vector2 _scrollPosition = Vector2.zero;
        bool _showMaterials = true;

        void UpdateTargets()
        {
            bool isShaderBroken (Shader s) => s == null || s.name == "Hidden/InternalErrorShader";
            _targets = _materialList.Where(t => t != null && !isShaderBroken(t.shader)).ToList();
            foreach(Material m in _materialList.Where(t => t != null && isShaderBroken(t.shader)))
                Debug.LogWarning("Material " + m.name + " has no shader assigned");
        }

        private void OnGUI()
        {
            // List of materials, remove button next to each
            // Add button at bottom

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            _showMaterials = EditorGUILayout.Foldout(_showMaterials, "Materials");
            EditorGUI.BeginChangeCheck();
            if(_showMaterials)
            {
                EditorGUILayout.BeginVertical();
                for (int i = 0; i < _materialList.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    _materialList[i] = (Material)EditorGUILayout.ObjectField(_materialList[i], typeof(Material), false);
                    if (GUILayout.Button("Remove", GUILayout.Width(70)))
                    {
                        _materialList.RemoveAt(i);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(30);
                if (GUILayout.Button("Add", GUILayout.Width(70)))
                {
                    _materialList.Add(null);
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            // Check if targets have changed
            bool didShadersChange = false;
            foreach(Material m in _materialList)
            {
                if(m != null && (!_targetShaders.ContainsKey(m) || _targetShaders[m] != m.shader))
                {
                    didShadersChange = true;
                    _targetShaders[m] = m.shader;
                }
            }
            if (EditorGUI.EndChangeCheck() || didShadersChange)
            {
                _shaderEditor = null;
                UpdateTargets();
            }

            // Draw shader editor
            if (_targets.Count > 0)
            {
                // Create shader editor
                if (_shaderEditor == null)
                {
                    _shaderEditor = new ShaderEditor();
                    _materialEditor = MaterialEditor.CreateEditor(_targets.ToArray()) as MaterialEditor;

                    // group targets by shader, take one material per shader
                    IEnumerable<Material> materialsToSearchProperties = _targets.GroupBy(t => t.shader).Select(g => g.First());
                    // get properties for each shader
                    Dictionary<Shader, HashSet<string>> shaderProperties = materialsToSearchProperties.ToDictionary(m => m.shader, m => new HashSet<string>(MaterialEditor.GetMaterialProperties(new UnityEngine.Object[] { m }).Select(p => p.name)));
                    // get values of dict as string arrays
                    IEnumerable<string[]> propertiesPerShader = shaderProperties.Values.Select(v => v.ToArray());
                    // create intersection of all properties
                    List<string> propertiesOrdered = propertiesPerShader.Aggregate((a, b) => a.Intersect(b).ToArray()).ToList();
                    // expand the intersection to be a union, but add each property after the occurence of their predecessor
                    foreach (string[] properties in propertiesPerShader)
                    {
                        int index = 0;
                        foreach (string property in properties)
                        {
                            if (!propertiesOrdered.Contains(property))
                            {
                                if (index == 0)
                                    propertiesOrdered.Insert(0, property);
                                else
                                    propertiesOrdered.Insert(propertiesOrdered.IndexOf(properties[index - 1]) + 1, property);
                            }
                            index++;
                        }
                    }
                    // For each property get all materials, whos shader has this property
                    Dictionary<string, List<Material>> propertyMaterials = new Dictionary<string, List<Material>>();
                    foreach (string property in propertiesOrdered)
                    {
                        propertyMaterials[property] = _targets.Where(t => shaderProperties[t.shader].Contains(property)).ToList();
                    }
                    // Get MaterialProperties of all materials
                    _materialProperties = propertiesOrdered.Select(p => MaterialEditor.GetMaterialProperty(propertyMaterials[p].ToArray(), p)).ToArray();
                }

                // Seperator
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                // Cursed but makes it render similar to the inspector
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space(15);
                EditorGUILayout.BeginVertical();
                _shaderEditor.OnGUI(_materialEditor, _materialProperties);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                
            }

            EditorGUILayout.EndScrollView();
        }
    }
}