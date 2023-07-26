using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor
{
    public class Presets : AssetPostprocessor
    {
        const string TAG_IS_PRESET = "isPreset";
        const string TAG_POSTFIX_IS_PRESET = "_isPreset";
        const string TAG_PRESET_NAME = "presetName";
        const string FILE_NAME_CACHE = "Thry/preset_cache.txt";

        static Dictionary<Material, (Material, Material)> s_appliedPresets = new Dictionary<Material, (Material, Material)>();

        static List<string> p_presetNames;
        static List<string> s_presetGuids;
        static Dictionary<string, Material> s_presetMaterials;
        static List<string> s_presetNames { get
            {
                InitializeDataStructures();
                return p_presetNames;
            }
        }

        static void InitializeDataStructures()
        {
            if (p_presetNames != null) return;
            // Get current time
            var time = System.DateTime.Now;
            // Check if cache exists
            if(File.Exists(FILE_NAME_CACHE))
            {
                string raw = File.ReadAllText(FILE_NAME_CACHE);
                // If file is empty (no presets), create empty, parsing would throw error
                if(string.IsNullOrWhiteSpace(raw))
                {
                    s_presetGuids = new List<string>();
                    p_presetNames = new List<string>();
                    s_presetMaterials = new Dictionary<string, Material>();
                }
                else
                {
                    // Load from cache
                    string[][] lines = raw.Split('\n').Select(s => s.Replace("\r","").Split(';')).ToArray();
                    // Split into lines
                    p_presetNames = lines.Select(l => l[0]).ToList();
                    s_presetGuids = lines.Select(l => l[1]).ToList();
                    s_presetMaterials = new Dictionary<string, Material>();
                }
            }else
            {
                CreatePresetCache();
            }
            // Log time
            // Debug.Log($"Presets: {p_presetNames.Length} presets found in {System.DateTime.Now - time}");
        }

        static void CreatePresetCache()
        {
            // Create cache
            // Find all materials
            string[] guids = AssetDatabase.FindAssets("t:material");
            List<Material> presetMaterials = new List<Material>();
            for(int guid = 0; guid < guids.Length; guid++)
            {
                EditorUtility.DisplayProgressBar("Creating Preset Cache", $"Loading material {guid + 1}/{guids.Length}", (float)guid / guids.Length);
                // Load material
                Material material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guids[guid]));
                // Check if material is preset
                if (IsPreset(material))
                {
                    // Add to list
                    presetMaterials.Add(material);
                }
            }
            EditorUtility.ClearProgressBar();
            // Create data
            s_presetGuids = new List<string>();
            p_presetNames = new List<string>();
            s_presetMaterials = new Dictionary<string, Material>();
            for(int i = 0; i < presetMaterials.Count; i++)
            {
                string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(presetMaterials[i]));
                p_presetNames.Add(presetMaterials[i].GetTag(TAG_PRESET_NAME, false, presetMaterials[i].name).Replace(';', '_'));
                s_presetGuids.Add(guid);
                s_presetMaterials[guid] = presetMaterials[i];
            }
            // Save cache
            Save();
        }

        static void Save()
        {
            // Save cache
            FileHelper.CreateFileWithDirectories(FILE_NAME_CACHE);
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < s_presetGuids.Count; i++)
            {
                sb.AppendLine($"{p_presetNames[i]};{s_presetGuids[i]}");
            }
            File.WriteAllText(FILE_NAME_CACHE, sb.ToString().TrimEnd('\r', '\n'));
        }
        
        // On Asset Delete remove presets from cache
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if(s_presetNames == null) return; // should init stuff

            if(importedAssets.Length > 0)
            {
                // Check if any presets were imported, iterate over all imported materials
                foreach (string asset in importedAssets.Where(a => a.EndsWith(".mat")))
                {
                    Material material = AssetDatabase.LoadAssetAtPath<Material>(asset);
                    // Check if asset is preset
                    if (IsPreset(material))
                    {
                        // Add preset
                        AddPreset(material);
                    }
                }
            }

            if(deletedAssets.Length > 0)
            { 
                // get all paths from guids
                Dictionary<string,string> presetPaths = new Dictionary<string, string>();
                foreach(string guid in s_presetGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    // if path is empty, the asset was deleted somewhere between the last cache save and now
                    if(string.IsNullOrWhiteSpace(path))
                    {
                        // remove from cache
                        RemovePreset(guid);
                    }else
                    {
                        presetPaths[path] = guid;
                    }
                }
                // Check if any presets were deleted, iterate over all deleted materials
                foreach (string asset in deletedAssets.Where(a => a.EndsWith(".mat")))
                {
                    // Check if asset is preset
                    if (presetPaths.ContainsKey(asset))
                    {
                        // Remove preset
                        RemovePreset(presetPaths[asset]);
                    }
                }
            }
        }

        static void AddPreset(Material material)
        {
            // Get preset name
            string presetName = material.GetTag(TAG_PRESET_NAME, false, material.name).Replace(';', '_');
            // Get preset guid
            string presetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(material));
            // Add to cache
            if(s_presetGuids.Contains(presetGuid)) return;
            Debug.Log($"AddPreset: {presetName} ({presetGuid})");
            s_presetNames.Add(presetName);
            s_presetGuids.Add(presetGuid);
            s_presetMaterials[presetName] = material;
            // Save cache
            Save();
        }

        static void RemovePreset(Material material)
        {
            // Get guid
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(material));
            RemovePreset(guid);
        }

        static void RemovePreset(string guid)
        {
            if(!s_presetGuids.Contains(guid)) return;
            // Remove from cache
            int index = s_presetGuids.IndexOf(guid);
            Debug.Log($"RemovePreset: {p_presetNames[index]}");
            p_presetNames.RemoveAt(index);
            s_presetGuids.RemoveAt(index);
            s_presetMaterials.Remove(guid);
            // Save cache
            Save();
        }

        public static Material GetPresetMaterial(string guid)
        {
            if (s_presetMaterials.ContainsKey(guid))
            {
                return s_presetMaterials[guid];
            }
            else if(s_presetGuids.Contains(guid))
            {
                Material m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                s_presetMaterials[guid] = m;
                return m;
            }
            return null;
        }

        public static bool DoesPresetExist(string presetName)
        {
            return s_presetNames.Contains(presetName);
        }

        private static PresetsPopupGUI window;
        public static void OpenPresetsMenu(Rect r, ShaderEditor shaderEditor)
        {
            Event.current.Use();
            if (Event.current.button == 0)
            {
                Vector2 pos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                pos.x = Mathf.Min(EditorWindow.focusedWindow.position.x + EditorWindow.focusedWindow.position.width - 250, pos.x);
                pos.y = Mathf.Min(EditorWindow.focusedWindow.position.y + EditorWindow.focusedWindow.position.height - 200, pos.y);
                if (window != null)
                    window.Close();
                window = ScriptableObject.CreateInstance<PresetsPopupGUI>();
                window.position = new Rect(pos.x, pos.y, 250, 200);
                window.Init(s_presetNames, s_presetGuids, shaderEditor);
                window.titleContent = new GUIContent("Preset List");
                window.ShowUtility();
            }
            else
            {
                EditorUtility.DisplayCustomMenu(GUILayoutUtility.GetLastRect(), s_presetNames.Select(s => new GUIContent(s)).ToArray(), 0, ApplyQuickPreset, shaderEditor);
            }
        }

        static void ApplyQuickPreset(object userData, string[] options, int selected)
        {
            Apply(GetPresetMaterial(s_presetGuids[selected - 1]), userData as ShaderEditor);
        }

        public static void PresetEditorGUI(ShaderEditor shaderEditor)
        {
            if (shaderEditor.IsPresetEditor)
            {
                EditorGUILayout.LabelField(EditorLocale.editor.Get("preset_material_notify"), Styles.greenStyle);
                string name = shaderEditor.Materials[0].GetTag(TAG_PRESET_NAME, false, "");
                EditorGUI.BeginChangeCheck();
                name = EditorGUILayout.DelayedTextField(EditorLocale.editor.Get("preset_name"), name);
                if (EditorGUI.EndChangeCheck())
                {
                    InitializeDataStructures();
                    string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(shaderEditor.Materials[0]));
                    shaderEditor.Materials[0].SetOverrideTag(TAG_PRESET_NAME, name);
                    s_presetNames[s_presetGuids.IndexOf(guid)] = name;
                    Save();
                }
            }
            if (s_appliedPresets.ContainsKey(shaderEditor.Materials[0]))
            {
                if(GUILayout.Button(EditorLocale.editor.Get("preset_revert")+s_appliedPresets[shaderEditor.Materials[0]].Item1.name))
                {
                    Revert(shaderEditor);
                }
            }
        }

        public static void Apply(Material preset, ShaderEditor shaderEditor)
        {
            s_appliedPresets[shaderEditor.Materials[0]] = (preset, new Material(shaderEditor.Materials[0]));
            foreach (ShaderPart prop in shaderEditor.ShaderParts)
            {
                if (IsPreset(preset, prop))
                {
                    prop.CopyFromMaterial(preset);
                }
            }
            foreach (Material m in shaderEditor.Materials)
                MaterialEditor.ApplyMaterialPropertyDrawers(m);
        }

        static void Revert(ShaderEditor shaderEditor)
        {
            Material key = shaderEditor.Materials[0];
            Material preset = s_appliedPresets[key].Item1;
            Material prePreset = s_appliedPresets[key].Item2;
            foreach (ShaderPart prop in shaderEditor.ShaderParts)
            {
                if (IsPreset(preset, prop))
                {
                    prop.CopyFromMaterial(prePreset);
                }
            }
            foreach (Material m in shaderEditor.Materials)
                MaterialEditor.ApplyMaterialPropertyDrawers(m);
            s_appliedPresets.Remove(key);
        }

        public static void ApplyList(ShaderEditor shaderEditor, Material[] originals, List<Material> presets)
        {
            for(int i=0;i<shaderEditor.Materials.Length && i < originals.Length;i++)
                shaderEditor.Materials[i].CopyPropertiesFromMaterial(originals[i]);
            foreach (Material preset in presets)
            {
                string guuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(preset));
                foreach (ShaderPart prop in shaderEditor.ShaderParts)
                {
                    if (IsPreset(preset, prop))
                    {
                        prop.CopyFromMaterial(preset);
                    }
                }
            }
            MaterialEditor.ApplyMaterialPropertyDrawers(shaderEditor.Materials);
            shaderEditor.Reload();
        }

        public static void SetProperty(Material m, ShaderPart prop, bool value)
        {
            if (prop.MaterialProperty != null)   m.SetOverrideTag(prop.MaterialProperty.name + TAG_POSTFIX_IS_PRESET, value ? "true" : "");
            if (prop.PropertyIdentifier != null) m.SetOverrideTag(prop.PropertyIdentifier    + TAG_POSTFIX_IS_PRESET, value ? "true" : "");
        }

        public static bool IsPreset(Material m, ShaderPart prop)
        {
            if (prop.MaterialProperty != null)   return m.GetTag(prop.MaterialProperty.name + TAG_POSTFIX_IS_PRESET, false, "") == "true";
            if (prop.PropertyIdentifier != null) return m.GetTag(prop.PropertyIdentifier    + TAG_POSTFIX_IS_PRESET, false, "") == "true";
            return false;
        }

        public static bool ArePreset(Material[] mats)
        {
            return mats.All(m => IsPreset(m));
        }

        public static bool IsPreset(Material m)
        {
            return m.GetTag(TAG_IS_PRESET, false, "false") == "true";
        }

        [MenuItem("Assets/Thry/Mark as preset")]
        static void MarkAsPreset()
        {
            IEnumerable<Material> mats = Selection.assetGUIDs.Select(g => AssetDatabase.GUIDToAssetPath(g)).
                Where(p => AssetDatabase.GetMainAssetTypeAtPath(p) == typeof(Material)).Select(p => AssetDatabase.LoadAssetAtPath<Material>(p));
            foreach (Material m in mats)
            {
                m.SetOverrideTag(TAG_IS_PRESET, "true");
                if (m.GetTag("presetName", false, "") == "") m.SetOverrideTag("presetName", m.name);
                Presets.AddPreset(m);
            }
            p_presetNames = null;
        }

        [MenuItem("Assets/Thry/Mark as preset", true)]
        static bool MarkAsPresetValid()
        {
            return Selection.assetGUIDs.Select(g => AssetDatabase.GUIDToAssetPath(g)).
                All(p => AssetDatabase.GetMainAssetTypeAtPath(p) == typeof(Material));
        }

        [MenuItem("Assets/Thry/Remove as preset")]
        static void RemoveAsPreset()
        {
            IEnumerable<Material> mats = Selection.assetGUIDs.Select(g => AssetDatabase.GUIDToAssetPath(g)).
                Where(p => AssetDatabase.GetMainAssetTypeAtPath(p) == typeof(Material)).Select(p => AssetDatabase.LoadAssetAtPath<Material>(p));
            foreach (Material m in mats)
            {
                m.SetOverrideTag(TAG_IS_PRESET, "");
                Presets.RemovePreset(m);
            }
            p_presetNames = null;
        }

        [MenuItem("Assets/Thry/Remove as preset", true)]
        static bool RemoveAsPresetValid()
        {
            return Selection.assetGUIDs.Select(g => AssetDatabase.GUIDToAssetPath(g)).
                All(p => AssetDatabase.GetMainAssetTypeAtPath(p) == typeof(Material));
        }
        
        [MenuItem("Thry/Presets/Rebuild Cache", priority = 100)]
        static void RebuildCache()
        {
            Presets.CreatePresetCache();
        }
    }

    public class PresetsPopupGUI : EditorWindow
    {
        class PresetStruct
        {
            Dictionary<string,PresetStruct> dict;
            public List<PresetStruct> structure;
            string name;
            string fullName;
            string guid;
            bool hasPreset;
            bool isOpen = false;
            bool isOn;
            public PresetStruct(string name)
            {
                this.name = name;
                dict = new Dictionary<string, PresetStruct>();
                structure = new List<PresetStruct>();
            }

            public PresetStruct GetSubStruct(string name)
            {
                name = name.Trim();
                if (dict.ContainsKey(name) == false)
                {
                    dict.Add(name, new PresetStruct(name));
                    structure.Add(dict[name]);
                }
                return dict[name];
            }
            public void AddPresetStruct(bool b, string name, string fullName, string guid)
            {
                PresetStruct s = new PresetStruct(name);
                s.hasPreset = b;
                s.fullName = fullName;
                s.guid = guid;
                if(!dict.ContainsKey(fullName))
                {
                    dict.Add(fullName, s);
                }else
                {
                    PresetStruct dupl = dict[fullName];
                    if(dupl.fullName.EndsWith(dupl.name))
                        dupl.name = dupl.name + $" ({dupl.guid})";
                    s.name = s.name + $" ({guid})";
                }
                structure.Add(s);
            }
            public void StructGUI(PresetsPopupGUI popupGUI)
            {
                if(hasPreset)
                {
                    EditorGUI.BeginChangeCheck();
                    isOn = EditorGUILayout.ToggleLeft(name, isOn);
                    if (EditorGUI.EndChangeCheck())
                    {
                        popupGUI.ToggelPreset(Presets.GetPresetMaterial(guid), isOn);
                    }
                }
                if(structure.Count > 0)
                {
                    Rect r = GUILayoutUtility.GetRect(new GUIContent(), Styles.dropDownHeader);
                    r.x = EditorGUI.indentLevel * 15;
                    r.width -= r.x;
                    GUI.Box(r, name, Styles.dropDownHeader);
                    if (Event.current.type == EventType.Repaint)
                    {
                        var toggleRect = new Rect(r.x + 4f, r.y + 2f, 13f, 13f);
                        EditorStyles.foldout.Draw(toggleRect, false, false, isOpen, false);
                    }
                    if (Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                    {
                        isOpen = !isOpen;
                        ShaderEditor.Input.Use();
                    }
                    if (isOpen)
                    {
                        EditorGUI.indentLevel += 1;
                        foreach (PresetStruct struc in structure)
                        {
                            struc.StructGUI(popupGUI);
                        }
                        EditorGUI.indentLevel -= 1;
                    }
                }
                
            }

            public void Reset()
            {
                isOn = false;
                foreach (PresetStruct struc in structure)
                    struc.Reset();
            }
        }

        Material[] beforePreset;
        List<Material> tickedPresets = new List<Material>();
        PresetStruct mainStruct;
        ShaderEditor shaderEditor;
        public void Init(List<string> names, List<string> guids, ShaderEditor shaderEditor)
        {
            this.shaderEditor = shaderEditor;
            ShaderOptimizer.DetourApplyMaterialPropertyDrawers();
            this.beforePreset = shaderEditor.Materials.Select(m => new Material(m)).ToArray();
            ShaderOptimizer.RestoreApplyMaterialPropertyDrawers();
            mainStruct = new PresetStruct("");
            backgroundTextrure = new Texture2D(1,1);
            if (EditorGUIUtility.isProSkin) backgroundTextrure.SetPixel(0, 0, new Color(0.18f, 0.18f, 0.18f, 1));
            else backgroundTextrure.SetPixel(0, 0, new Color(0.9f, 0.9f, 0.9f, 1));
            backgroundTextrure.Apply();
            for (int i = 0; i < names.Count; i++)
            {
                string[] path = names[i].Split('/');
                PresetStruct addUnder = mainStruct;
                for (int j=0;j<path.Length - 1; j++)
                {
                    addUnder = addUnder.GetSubStruct(path[j]);
                }
                addUnder.AddPresetStruct(Presets.DoesPresetExist(names[i]), path[path.Length-1], names[i], guids[i]);
            }
        }

        void ToggelPreset(Material m, bool on)
        {
            if (tickedPresets.Contains(m) && !on) tickedPresets.Remove(m);
            if (!tickedPresets.Contains(m) && on) tickedPresets.Add(m);
            Presets.ApplyList(shaderEditor, beforePreset, tickedPresets);
        }

        static Texture2D backgroundTextrure;

        Vector2 scroll;
        bool _save;
        void OnGUI()
        {
            if (mainStruct == null) { this.Close(); return; }

            GUILayout.BeginHorizontal();
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(position.height - 55));

            GUILayoutUtility.GetRect(10, 5);
            TopStructGUI();

            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();

            if (GUI.Button(new Rect(5, this.position.height - 35, this.position.width / 2 - 5, 30), "Apply"))
            {
                _save = true;
                this.Close();
            }
                
            if (GUI.Button(new Rect(this.position.width / 2, this.position.height - 35, this.position.width / 2 - 5, 30), "Discard"))
            {
                Revert();
            }
        }
        private void OnDestroy()
        {
            if (!_save)
            {
                Revert();
            }
        }

        void TopStructGUI()
        {
            foreach (PresetStruct struc in mainStruct.structure)
            {
                struc.StructGUI(this);
            }
        }

        void Revert()
        {
            EditorUtility.DisplayProgressBar("Reverting", "Reverting", 0);
            for (int i = 0; i < shaderEditor.Materials.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Reverting", "Reverting", (float)i / shaderEditor.Materials.Length);
                shaderEditor.Materials[i].CopyPropertiesFromMaterial(beforePreset[i]);
                MaterialEditor.ApplyMaterialPropertyDrawers(shaderEditor.Materials[i]);
            }
            EditorUtility.ClearProgressBar();
            mainStruct.Reset();
            tickedPresets.Clear();
            shaderEditor.Reload();
        }
    }
}