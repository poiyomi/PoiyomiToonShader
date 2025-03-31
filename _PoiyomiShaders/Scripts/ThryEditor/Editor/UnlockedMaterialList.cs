using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor
{
    public class UnlockedMaterialsList : EditorWindow
    {
        private Vector2 scrollPosition = Vector2.zero;

        static Dictionary<Shader, List<Material>> unlockedMaterialsByShader = new Dictionary<Shader, List<Material>>();
        static Dictionary<Shader, List<Material>> lockedMaterialsByShader = new Dictionary<Shader, List<Material>>();
        static Dictionary<Material, Shader> lockedMaterialsByOriginalShader = new Dictionary<Material, Shader>();
        static Dictionary<String, bool> unlockedFoldouts = new Dictionary<String, bool>();
        static Dictionary<String, bool> lockedFoldouts = new Dictionary<String, bool>();
        string searchTerm = "";

        private void OnEnable()
        {
            UpdateList();
            scrollPosition = Vector2.zero;
        }

        bool LockAllWarning(List<Material> materialsToLock)
        {
            return EditorUtility.DisplayDialog("Lock All Materials", $"You're about to lock {materialsToLock.Count} materials. This might take a while. Are you sure you want to proceed?", "Lock All", "Cancel");
        }

        bool UnlockAllWarning(List<Material> materialsToUnlock)
        {
            return EditorUtility.DisplayDialog("Unlock All Materials", $"You're about to unlock {materialsToUnlock.Count} materials. This might cause crashes if over 64 textures are used in all materials on a single shader.\n\nAre you sure you want to proceed?", "Unlock All", "Cancel");
        }

        void UpdateList()
        {
            lockedMaterialsByOriginalShader.Clear();
            unlockedMaterialsByShader.Clear();
            lockedMaterialsByShader.Clear();
            string[] guids = AssetDatabase.FindAssets($"t:material {searchTerm}");
            float step = 1.0f / guids.Length;
            float f = 0;
            EditorUtility.DisplayProgressBar("Searching materials...", "", f);

            List<Material> unlockedMaterials = new List<Material>();
            List<Material> lockedMaterials = new List<Material>();

            foreach (string g in guids)
            {
                Material m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g));
                if (m != null && m.shader != null && ShaderOptimizer.IsShaderUsingThryOptimizer(m.shader))
                {
                    if(ShaderOptimizer.IsMaterialLocked(m))
                        lockedMaterials.Add(m);
                    else
                        unlockedMaterials.Add(m);
                }
                f = f + step;
                EditorUtility.DisplayProgressBar("Searching materials...", m.name, f);
            }
            foreach (IGrouping<Shader, Material> materials in unlockedMaterials.GroupBy(m => m.shader))
            {
                unlockedMaterialsByShader.Add(materials.Key, materials.ToList());
                if(!unlockedFoldouts.ContainsKey(materials.Key.name))
                    unlockedFoldouts.Add(materials.Key.name, false);
            }

            foreach (Material material in lockedMaterials)
            {
                Shader originalShader = ShaderOptimizer.GetOriginalShader(material);
                if (originalShader == null) continue;
                
                if (!lockedMaterialsByShader.ContainsKey(originalShader))
                    lockedMaterialsByShader[originalShader] = new List<Material>();
                lockedMaterialsByShader[originalShader].Add(material);

                if (!lockedFoldouts.ContainsKey(originalShader.name))
                    lockedFoldouts.Add(originalShader.name, false);
            }
            EditorUtility.ClearProgressBar();
        }

        private void OnGUI()
        {
            List<Material> materialsToLock = new List<Material>();
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            searchTerm = EditorGUILayout.DelayedTextField(searchTerm);
            if (GUILayout.Button("Update/Search") || EditorGUI.EndChangeCheck()) 
                UpdateList();
            EditorGUILayout.EndHorizontal();
            int unlockedMaterials = unlockedMaterialsByShader.Values.SelectMany(col => col).ToList().Count;
            int lockedMaterials = lockedMaterialsByShader.Values.SelectMany(col => col).ToList().Count;

            EditorGUILayout.Space(10, true);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.LabelField($"Unlocked Materials ({unlockedMaterials})", Styles.editorHeaderLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Lock All")) 
            {
                List<Material> allUnlockedMaterials = unlockedMaterialsByShader.Values.SelectMany(col => col).ToList();
                if(LockAllWarning(allUnlockedMaterials))
                    materialsToLock = allUnlockedMaterials;
            }
            if (GUILayout.Button("Expand All")) unlockedFoldouts.Keys.ToList().ForEach(f => unlockedFoldouts[f] = true);
            if (GUILayout.Button("Collapse All")) unlockedFoldouts.Keys.ToList().ForEach(f => unlockedFoldouts[f] = false);

            EditorGUILayout.EndHorizontal();

            if (unlockedMaterialsByShader.Count == 0)
                GUILayout.Label("No Locked materials found for search term.", Styles.greenStyle);

            foreach (KeyValuePair<Shader, List<Material>> shaderMaterials in unlockedMaterialsByShader)
            {
                EditorGUILayout.Space();

                unlockedFoldouts[shaderMaterials.Key.name] = EditorGUILayout.BeginFoldoutHeaderGroup(unlockedFoldouts[shaderMaterials.Key.name], $"{shaderMaterials.Key.name} ({shaderMaterials.Value.Count.ToString()})");
                if (unlockedFoldouts[shaderMaterials.Key.name])
                {
                    if (GUILayout.Button("Lock All")) 
                    {
                        if(LockAllWarning(shaderMaterials.Value))
                            materialsToLock = shaderMaterials.Value;
                    }

                    foreach (Material m in shaderMaterials.Value)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(m, typeof(Material), false);
                        //EditorGUILayout.IntField(ShaderOptimizer.GetUsedTextureReferencesCount(m.shader));
                        if (GUILayout.Button("Lock"))
                            materialsToLock.Add(m);

                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            if (materialsToLock.Count > 0)
            {
                ShaderOptimizer.LockMaterials(materialsToLock, ShaderOptimizer.ProgressBar.Cancellable);
                materialsToLock.Clear();
                UpdateList();
            }

            EditorGUILayout.Space(10, true);

            EditorGUILayout.LabelField($"Locked Materials ({lockedMaterials})", Styles.editorHeaderLabel);
            List<Material> materialsToUnlock = new List<Material>();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Unlock All"))
            {
                List<Material> allLockedMaterials = lockedMaterialsByShader.Values.SelectMany(col => col).ToList();
                if (UnlockAllWarning(allLockedMaterials))
                {
                    materialsToUnlock = allLockedMaterials;
                }
            }
            if (GUILayout.Button("Expand All")) lockedFoldouts.Keys.ToList().ForEach(f => lockedFoldouts[f] = true);
            if (GUILayout.Button("Collapse All")) lockedFoldouts.Keys.ToList().ForEach(f => lockedFoldouts[f] = false);

            EditorGUILayout.EndHorizontal();

            if (lockedMaterialsByShader.Count == 0)
                GUILayout.Label("No Unlocked materials found for search term.", Styles.greenStyle);

            foreach (KeyValuePair<Shader, List<Material>> shaderMaterials in lockedMaterialsByShader)
            {
                EditorGUILayout.Space();

                lockedFoldouts[shaderMaterials.Key.name] = EditorGUILayout.BeginFoldoutHeaderGroup(lockedFoldouts[shaderMaterials.Key.name], $"{shaderMaterials.Key.name} ({shaderMaterials.Value.Count.ToString()})");
                if (lockedFoldouts[shaderMaterials.Key.name])
                {
                    if (GUILayout.Button("Unlock All")) 
                    {
                        if(UnlockAllWarning(shaderMaterials.Value))
                            materialsToUnlock = shaderMaterials.Value;
                    }

                    foreach (Material m in shaderMaterials.Value)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(m, typeof(Material), false);
                        //EditorGUILayout.IntField(ShaderOptimizer.GetUsedTextureReferencesCount(m.shader));
                        if (GUILayout.Button("Unlock"))
                            materialsToUnlock.Add(m);

                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            if (materialsToUnlock.Count > 0)
            {
                ShaderOptimizer.UnlockMaterials(materialsToUnlock, ShaderOptimizer.ProgressBar.Cancellable);
                materialsToUnlock.Clear();
                UpdateList();
            }
            EditorGUILayout.EndScrollView();
        }
    }
}