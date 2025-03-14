using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor
{
    public class CrossEditor : EditorWindow
    {
        private static CrossEditor GetInstance()
        {
            CrossEditor window = GetWindow(typeof(CrossEditor)) as CrossEditor;
            window.name = "Cross Shader Editor";

            return window;
        }

        [MenuItem("Thry/Cross Shader Editor", priority = -20)]
        public static void ShowWindow()
        {
            GetInstance();
        }

        [MenuItem("Assets/Thry/Materials/Add to Cross Shader Editor", false , 400)]
        private static void OpenInCrossShaderEditor()
        {
            List<Material> materials = ShaderOptimizer.FindMaterials(ShaderOptimizer.GetSelectedFolders());
            materials.AddRange(Selection.objects.Where(o => o is Material).Cast<Material>());

            GetInstance().UpdateTargets(materials, true);
        }

        [MenuItem("Assets/Thry/Materials/Add to Cross Shader Editor", true, 400)]
        private static bool OpenInCrossShaderEditorValidation()
        {
            return Selection.objects.Any(o => o is Material) || ShaderOptimizer.GetSelectedFolders().Any();
        }

        [MenuItem("GameObject/Thry/Materials/Open All in Cross Shader Editor", false, 10)]
        private static void OpenAllInCrossShaderEditor()
        {
            GetInstance().UpdateTargets(Selection.gameObjects.SelectMany(o => o.GetComponentsInChildren<Renderer>(true)).SelectMany(r => r.sharedMaterials));
        }

        List<Material> _materialList = new List<Material>();
        List<Material> _targets = new List<Material>();
        Dictionary<Material,Shader> _targetShaders = new Dictionary<Material, Shader>();
        ShaderEditor _shaderEditor = null;
        MaterialEditor _materialEditor = null;
        MaterialProperty[] _materialProperties = null;
        Vector2 _scrollPosition = Vector2.zero;
        bool _showMaterials = true;
        Lazy<GUIStyle> LeftMargin = new Lazy<GUIStyle>(() => new GUIStyle() { margin = new RectOffset(30, 0, 0, 0) });

        private void UpdateTargets(IEnumerable<Material> materials, bool add = false)
        {
            _materialList = (add ?
                _materialList.Concat(materials) : // add
                materials) // replace
                .Distinct().ToList(); // deduplicate

            UpdateTargets();
        }

        private void UpdateTargets()
        {
            bool isShaderBroken(Shader s) => s == null || s.name == "Hidden/InternalErrorShader";

            _targets = _materialList.Where(t => t != null && !isShaderBroken(t.shader)).ToList();
            foreach(Material m in _materialList.Where(t => t != null && isShaderBroken(t.shader)))
                Debug.LogWarning("Material " + m.name + " has no shader assigned");

            _shaderEditor = null;
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            _showMaterials = EditorGUILayout.Foldout(_showMaterials, "Materials");

            EditorGUI.BeginChangeCheck();
            DrawMaterials();

            // Check if targets have changed
            bool didShadersChange = EditorGUI.EndChangeCheck();
            foreach (Material m in _materialList)
            {
                if (m == null || // Material is null
                    _targetShaders.ContainsKey(m) && _targetShaders[m] == m.shader) // Shader hasn't changed
                    continue;

                didShadersChange = true;
                _targetShaders[m] = m.shader;
            }

            if (didShadersChange) UpdateTargets();

            DrawShaderEditor();

            EditorGUILayout.EndScrollView();
        }

        // List of materials, remove button next to each
        // Add and Remove All buttons at bottom
        private void DrawMaterials()
        {
            if (!_showMaterials) return;

            using (new EditorGUILayout.VerticalScope(LeftMargin.Value))
            {
                for (int i = 0; i < _materialList.Count; i++) DrawMaterial(i);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Add", GUILayout.Width(100))) _materialList.Add(null);

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Remove All", GUILayout.Width(100))) _materialList.Clear();
                }
            }
        }

        private void DrawMaterial(int i)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                Material material = (Material)EditorGUILayout.ObjectField(_materialList[i], typeof(Material), false);

                if (material != _materialList[i])
                {
                    if (_materialList.Contains(material)) material = null;

                    _materialList[i] = material;
                }

                if (GUILayout.Button("Remove", GUILayout.Width(100))) _materialList.RemoveAt(i);
            }
        }

        private void DrawShaderEditor()
        {
            if (_targets.Count == 0) return;

            // Create shader editor
            CreateShaderEditor();

            // Seperator
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // Cursed but makes it render similar to the inspector
            using (new EditorGUILayout.VerticalScope(LeftMargin.Value))
            {
                bool wideMode = EditorGUIUtility.wideMode;
                EditorGUIUtility.wideMode = true;
                _shaderEditor.OnGUI(_materialEditor, _materialProperties);
                EditorGUIUtility.wideMode = wideMode;
            }
        }

        private void CreateShaderEditor()
        {
            if (_shaderEditor != null) return;

            _shaderEditor = new ShaderEditor(){ IsCrossEditor = true };
            _materialEditor = Editor.CreateEditor(_targets.ToArray()) as MaterialEditor;

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
            Dictionary<string, Material[]> propertyMaterials = new Dictionary<string, Material[]>();
            foreach (string property in propertiesOrdered)
            {
                propertyMaterials[property] = _targets.Where(t => shaderProperties[t.shader].Contains(property)).ToArray();
            }
            // Get MaterialProperties of all materials
            _materialProperties = propertiesOrdered.Select(p => MaterialEditor.GetMaterialProperty(propertyMaterials[p], p)).ToArray();
            Debug.Log(propertyMaterials["_EnableGrabpass"].Length);
            MaterialProperty test = _materialProperties.Where(p => p.name == "_EnableGrabpass").First();
            Debug.Log(test.displayName);
            Debug.Log(test.type);
            Debug.Log(test.flags);
            Shader s = (test.targets[0] as Material).shader;
            Debug.Log(string.Join(",", s.GetPropertyAttributes(s.FindPropertyIndex(test.name))));
        }
    }
}