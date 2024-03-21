using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Policy;
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
        const string TAG_IS_SECTION_PRESET = "isSectionedPreset";
        const string TAG_SECTION_NAME_POSTFIX = "_presetName";
        const string FILE_NAME_CACHE = "Thry/preset_cache.txt";
        const string FILE_NAME_KNOWN_MATERIALS = "Thry/presets_known_materials.txt";
        const string PRESET_VERSION = "1.0.0";

        static Dictionary<Material, (string name, string guid, Material prePreset)> s_appliedPresets = new Dictionary<Material, (string, string, Material)>();
        
        class PresetsCollection
        {
            public List<string> Names = new List<string>();
            public List<string> Guids = new List<string>();
        }
        public class MaterialsList
        {
            string _filepath;
            HashSet<string> _guids;
            bool _isDirty = false;
            public MaterialsList(string filepath)
            {
                _filepath = filepath;
                _guids = new HashSet<string>();
                if (File.Exists(_filepath))
                {
                    string[] lines = File.ReadAllLines(_filepath);
                    foreach (string line in lines)
                    {
                        _guids.Add(line);
                    }
                }
            }

            public bool Contains(string guid)
            {
                return _guids.Contains(guid);
            }

            public void SetCollection(IEnumerable<string> guids)
            {
                _guids.Clear();
                _guids.UnionWith(guids);
                _isDirty = true;
            }

            public void Add(string guid)
            {
                _guids.Add(guid);
                _isDirty = true;
            }

            public void AllAll(IEnumerable<string> guids)
            {
                _guids.UnionWith(guids);
                _isDirty = true;
            }

            public void Save()
            {
                if (_isDirty)
                {
                    FileHelper.CreateFileWithDirectories(_filepath);
                    File.WriteAllLines(_filepath, _guids.ToArray());
                    _isDirty = false;
                }
            }
        }

        static Dictionary<string, Material> s_materalCache;
        static Dictionary<string, PresetsCollection> s_presetCollections;
        static Dictionary<string, PresetsCollection> PresetCollections
        {
            get
            {
                InitializeDataStructures();
                return s_presetCollections;
            }
        }
        static PresetsCollection FullPresets
        {
            get
            {
                InitializeDataStructures();
                return s_presetCollections["_full_"];
            }
        }

        public static MaterialsList KnownMaterials = new MaterialsList(FILE_NAME_KNOWN_MATERIALS);

        static void InitializeDataStructures()
        {
            if (s_presetCollections != null) return;
            s_presetCollections = new Dictionary<string, PresetsCollection>();
            s_presetCollections["_full_"] = new PresetsCollection();
            s_materalCache = new Dictionary<string, Material>();
            // Get current time
            // var time = System.DateTime.Now;
            // Check if cache exists
            if(File.Exists(FILE_NAME_CACHE))
            {
                string[] lines = File.ReadAllLines(FILE_NAME_CACHE);

                string presetVersion = lines[0];
                if(presetVersion != PRESET_VERSION)
                {
                    CreatePresetCache();
                    return;
                }

                bool nextLineIsPresetsCollectionsName = false;
                string currentCollection = null;
                for(int i = 1; i < lines.Length; i++)
                {
                    if(string.IsNullOrWhiteSpace(lines[i]))
                    {
                        nextLineIsPresetsCollectionsName = true;
                        continue;
                    }
                    if(nextLineIsPresetsCollectionsName)
                    {
                        nextLineIsPresetsCollectionsName = false;
                        currentCollection = lines[i];
                        s_presetCollections[currentCollection] = new PresetsCollection();
                    }else
                    {
                        string[] split = lines[i].Split(';');
                        s_presetCollections[currentCollection].Names.Add(split[0]);
                        s_presetCollections[currentCollection].Guids.Add(split[1]);
                    }                    
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
            for(int guid = 0; guid < guids.Length; guid++)
            {
                EditorUtility.DisplayProgressBar("Creating Preset Cache", $"Loading material {guid + 1}/{guids.Length}", (float)guid / guids.Length);
                // Load material
                Material material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guids[guid]));
                // Check if material is preset
                if (IsPreset(material))
                {
                    // Add to list
                    AddPreset(material);
                }
            }

            KnownMaterials.SetCollection(guids);
            KnownMaterials.Save();
            
            EditorUtility.ClearProgressBar();
        }

        static Dictionary<Shader, List<string>> s_headersInShader = new Dictionary<Shader, List<string>>();
        static List<string> GetHeadersInShader(Material m)
        {       
            if(s_headersInShader.ContainsKey(m.shader))
            {
                return s_headersInShader[m.shader];
            }
            MaterialProperty[] props = MaterialEditor.GetMaterialProperties(new Material[] { m });
            List<string> headers = new List<string>();
            foreach (MaterialProperty prop in props)
            {
                if (prop.flags == MaterialProperty.PropFlags.HideInInspector &&
                    prop.name.StartsWith("m_", StringComparison.Ordinal)
                    )
                {
                    headers.Add(prop.name);
                }
            }
            s_headersInShader[m.shader] = headers;
            return headers;
        }

        static void Save()
        {
            // Save cache
            FileHelper.CreateFileWithDirectories(FILE_NAME_CACHE);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(PRESET_VERSION);

            foreach(KeyValuePair<string, PresetsCollection> collection in PresetCollections)
            {
                sb.AppendLine();
                sb.AppendLine(collection.Key);
                for(int i = 0; i < collection.Value.Names.Count; i++)
                {
                    sb.AppendLine($"{collection.Value.Names[i]};{collection.Value.Guids[i]}");
                }
            }

            File.WriteAllText(FILE_NAME_CACHE, sb.ToString().TrimEnd('\r', '\n'));
        }
        
        // On Asset Delete remove presets from cache
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
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
                        RemovePreset(material);
                        AddPreset(material);
                    }
                    Debug.Log($"OnPostprocessAllAssets: {material.name} ({AssetDatabase.AssetPathToGUID(asset)})");
                    KnownMaterials.Add(AssetDatabase.AssetPathToGUID(asset));
                }
            }

            if(deletedAssets.Length > 0)
            { 
                // go through all preset collections
                Dictionary<string, string> presetPaths = new Dictionary<string, string>();
                foreach(KeyValuePair<string, PresetsCollection> collection in PresetCollections)
                {
                    // go through all presets in collection
                    for(int i = 0; i < collection.Value.Guids.Count; i++)
                    {
                        string guid = collection.Value.Guids[i];
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

            KnownMaterials.Save();
        }

        static void AddPreset(Material material)
        {
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(material));
            s_materalCache[guid] = material;
            
            if(IsMaterialSectionedPreset(material))
            {
                // Find sections that are presets
                List<string> headers = GetHeadersInShader(material);
                foreach(string header in headers)
                {
                    if(IsSectionPreset(material, header))
                    {
                        // Add to preset collection
                        string collectionName = header;
                        string name = material.GetTag(header + TAG_SECTION_NAME_POSTFIX, false, material.name).Replace(';', '_');
                        if(!PresetCollections.ContainsKey(collectionName))
                        {
                            PresetCollections[collectionName] = new PresetsCollection();
                        }
                        
                        if(!PresetCollections[collectionName].Guids.Contains(guid))
                        {
                            Debug.Log($"AddPreset: {name} ({guid})");
                            PresetCollections[collectionName].Names.Add(name);
                            PresetCollections[collectionName].Guids.Add(guid);
                        }
                    }
                }
            }else
            {
                // Add to full preset collection
                string name = material.GetTag(TAG_PRESET_NAME, false, material.name).Replace(';', '_');
                if(!PresetCollections["_full_"].Guids.Contains(guid))
                {
                    Debug.Log($"AddPreset: {name} ({guid})");
                    PresetCollections["_full_"].Names.Add(name);
                    PresetCollections["_full_"].Guids.Add(guid);
                }
            }
            s_materalCache[guid] = material;

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
            // go over all preset collections
            foreach(KeyValuePair<string, PresetsCollection> collection in PresetCollections)
            {
                // go over all presets in collection
                for(int i = 0; i < collection.Value.Guids.Count; i++)
                {
                    // if guid matches, remove from collection
                    if(collection.Value.Guids[i] == guid)
                    {
                        Debug.Log($"RemovePreset: {collection.Value.Names[i]} ({guid})");
                        collection.Value.Guids.RemoveAt(i);
                        collection.Value.Names.RemoveAt(i);
                        break;
                    }
                }
            }
            // Save cache
            Save();
        }

        public static Material GetPresetMaterial(string guid)
        {   
            if (s_materalCache.ContainsKey(guid))
            {
                return s_materalCache[guid];
            }
            Material m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
            s_materalCache[guid] = m;
            return m;
        }

        public static bool DoesPresetExist(string collection, string presetName)
        {
            return PresetCollections.ContainsKey(collection) &&
                 PresetCollections[collection].Names.Contains(presetName);
        }

        private static PresetsPopupGUI window;
        public static void OpenPresetsMenu(Rect r, ShaderEditor shaderEditor, bool forceQuick, string collection = "_full_")
        {
            Event.current.Use();
            
            Vector2 pos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            pos.x = Mathf.Min(EditorWindow.focusedWindow.position.x + EditorWindow.focusedWindow.position.width - 250, pos.x);
            pos.y = Mathf.Min(EditorWindow.focusedWindow.position.y + EditorWindow.focusedWindow.position.height - 200, pos.y);
                
            if (Event.current.button == 0 && !forceQuick)
            {
                if (window != null)
                    window.Close();
                window = ScriptableObject.CreateInstance<PresetsPopupGUI>();
                window.position = new Rect(pos.x, pos.y, 250, 200);
                window.Init(collection, PresetCollections[collection].Names, PresetCollections[collection].Guids, shaderEditor);
                window.titleContent = new GUIContent("Preset List");
                window.ShowUtility();
            }
            else
            {
                Debug.Log($"OpenPresetsMenu: {collection} ({PresetCollections[collection].Names.Count} presets)");
                EditorUtility.DisplayCustomMenu(r, 
                    PresetCollections[collection].Names.Select(s => new GUIContent(s)).ToArray(), -1, 
                    ApplyQuickPreset, new object[]{shaderEditor, collection, shaderEditor.CurrentProperty});
            }
        }

        static void ApplyQuickPreset(object userData, string[] options, int selected)
        {
            ShaderEditor shaderEditor = (userData as object[])[0] as ShaderEditor;
            string collection = (userData as object[])[1] as string;
            ShaderPart parent = (userData as object[])[2] as ShaderPart;
            Apply(collection, options[selected], shaderEditor, parent);
        }

        public static void PresetEditorGUI(ShaderEditor shaderEditor)
        {
            if (shaderEditor.IsPresetEditor)
            {
                EditorGUILayout.LabelField(EditorLocale.editor.Get("preset_material_notify"), Styles.greenStyle);
                EditorGUI.BeginChangeCheck();
                bool isSectionPreset = IsMaterialSectionedPreset(shaderEditor.Materials[0]);
                isSectionPreset = EditorGUILayout.Toggle(EditorLocale.editor.Get("preset_section_preset"), isSectionPreset);
                if(EditorGUI.EndChangeCheck())
                {
                    SetMaterialSectionedPreset(shaderEditor.Materials[0], isSectionPreset);
                }
                if(!isSectionPreset)
                {
                    string name = shaderEditor.Materials[0].GetTag(TAG_PRESET_NAME, false, "");
                    EditorGUI.BeginChangeCheck();
                    name = EditorGUILayout.DelayedTextField(EditorLocale.editor.Get("preset_name"), name);
                    if (EditorGUI.EndChangeCheck())
                    {
                        InitializeDataStructures();
                        string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(shaderEditor.Materials[0]));
                        shaderEditor.Materials[0].SetOverrideTag(TAG_PRESET_NAME, name);
                        FullPresets.Names[FullPresets.Guids.IndexOf(guid)] = name;
                        Save();
                    }
                }
            }
            if (s_appliedPresets.ContainsKey(shaderEditor.Materials[0]))
            {
                if(GUILayout.Button(EditorLocale.editor.Get("preset_revert")+s_appliedPresets[shaderEditor.Materials[0]].name))
                {
                    Revert(shaderEditor);
                }
            }
        }

        public static void Apply(string collection, string name, ShaderEditor shaderEditor, ShaderPart parent)
        {
            string guid = PresetCollections[collection].Guids[PresetCollections[collection].Names.IndexOf(name)];
            Material presetMaterial = GetPresetMaterial(guid);

            s_appliedPresets[shaderEditor.Materials[0]] = (name, guid, new Material(shaderEditor.Materials[0]));
            ApplyPresetInternal(shaderEditor, presetMaterial, presetMaterial, parent);
            foreach (Material m in shaderEditor.Materials)
                MaterialEditor.ApplyMaterialPropertyDrawers(m);
        }

        static void Revert(ShaderEditor shaderEditor)
        {
            Material key = shaderEditor.Materials[0];
            string name = s_appliedPresets[key].name;
            Material presetMaterial = GetPresetMaterial(s_appliedPresets[key].guid);
            Material prePreset = s_appliedPresets[key].prePreset;
            ShaderPart parent = shaderEditor.ShaderParts.FirstOrDefault(p => p.MaterialProperty?.name == name);
            ApplyPresetInternal(shaderEditor, presetMaterial, prePreset, parent);
            foreach (Material m in shaderEditor.Materials)
                MaterialEditor.ApplyMaterialPropertyDrawers(m);
            s_appliedPresets.Remove(key);
        }

        public static void ApplyFullList(ShaderEditor shaderEditor, Material[] originals, List<Material> presets)
        {
            for(int i=0;i<shaderEditor.Materials.Length && i < originals.Length;i++)
                shaderEditor.Materials[i].CopyPropertiesFromMaterial(originals[i]);
            foreach (Material preset in presets)
            {
                ApplyPresetInternal(shaderEditor, preset, preset, null);
            }
            foreach(Material m in shaderEditor.Materials)
                MaterialEditor.ApplyMaterialPropertyDrawers(m);
            shaderEditor.Reload();
        }

        static void ApplyPresetInternal(ShaderEditor shaderEditor, Material preset, Material copyFrom, ShaderPart parent)
        {
            // Swap shader to make sure all properties are available
            // And prevent stuff like missing shaders making presets unusable
            Shader prev = preset.shader;
            preset.shader = shaderEditor.Shader;

            if(!IsMaterialSectionedPreset(preset))
            {
                foreach (ShaderPart prop in shaderEditor.ShaderParts)
                {
                    if (IsPreset(preset, prop))
                    {
                        prop.CopyFromMaterial(preset);
                    }
                }
            }else if(parent is ShaderGroup)
            {
                ApplyPresetRecursive(shaderEditor, preset, parent as ShaderGroup);
            }
            preset.shader = prev;
        }
        
        static void ApplyPresetRecursive(ShaderEditor shaderEditor, Material preset, ShaderGroup parent)
        {
            foreach (ShaderPart prop in parent.parts)
            {
                if(prop is ShaderGroup)
                {
                    ApplyPresetRecursive(shaderEditor, preset, prop as ShaderGroup);
                }else
                {
                    if (IsPreset(preset, prop))
                    {
                        prop.CopyFromMaterial(preset);
                    }
                }
            }
        }

        public static void SetProperty(Material m, ShaderPart prop, bool value)
        {
            if (prop.CustomStringTagID != null) m.SetOverrideTag(prop.CustomStringTagID + TAG_POSTFIX_IS_PRESET, value ? "true" : "");
            if (prop.MaterialProperty != null)   m.SetOverrideTag(prop.MaterialProperty.name + TAG_POSTFIX_IS_PRESET, value ? "true" : "");
            if (prop.PropertyIdentifier != null) m.SetOverrideTag(prop.PropertyIdentifier    + TAG_POSTFIX_IS_PRESET, value ? "true" : "");
        }

        public static bool IsPreset(Material m, ShaderPart prop)
        {
            if (prop.CustomStringTagID != null) return m.GetTag(prop.CustomStringTagID + TAG_POSTFIX_IS_PRESET, false, "") == "true";
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

        public static bool IsMaterialSectionedPreset(Material m)
        {
            return m.GetTag(TAG_IS_SECTION_PRESET, false, "false") == "true";
        }

        public static void SetMaterialSectionedPreset(Material m, bool value)
        {
            m.SetOverrideTag(TAG_IS_SECTION_PRESET, value ? "true" : "");
        }

        public static bool IsSectionPreset(Material m, string headerPropName)
        {
            return !string.IsNullOrWhiteSpace(m.GetTag(headerPropName + TAG_IS_SECTION_PRESET, false, ""));
        }

        public static string GetSectionPresetName(Material m, string headerPropName)
        {
            return m.GetTag(headerPropName + TAG_IS_SECTION_PRESET, false, "");
        }

        public static void SetSectionPreset(Material m, string headerPropName, string name)
        {
            m.SetOverrideTag(headerPropName + TAG_IS_SECTION_PRESET, name);

            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m));
            if(!string.IsNullOrWhiteSpace(name))
            {
                if(!PresetCollections.ContainsKey(headerPropName))
                {
                    PresetCollections[headerPropName] = new PresetsCollection();
                }
    	        
                if(!PresetCollections[headerPropName].Guids.Contains(guid))
                {
                    Debug.Log($"AddPreset: {name} ({guid})");
                    PresetCollections[headerPropName].Names.Add(name);
                    PresetCollections[headerPropName].Guids.Add(guid);
                }else
                {
                    Debug.Log($"SetSectionPreset: {name} ({guid})");
                    PresetCollections[headerPropName].Names[PresetCollections[headerPropName].Guids.IndexOf(guid)] = name;
                }
            }else
            {
                if(PresetCollections.ContainsKey(headerPropName))
                {
                    if(PresetCollections[headerPropName].Guids.Contains(guid))
                    {
                        Debug.Log($"RemovePreset: {PresetCollections[headerPropName].Names[PresetCollections[headerPropName].Guids.IndexOf(guid)]} ({guid})");
                        PresetCollections[headerPropName].Names.RemoveAt(PresetCollections[headerPropName].Guids.IndexOf(guid));
                        PresetCollections[headerPropName].Guids.RemoveAt(PresetCollections[headerPropName].Guids.IndexOf(guid));
                    }
                }
            }
        }

        public static bool DoesSectionHavePresets(string headerPropName)
        {
            return PresetCollections.ContainsKey(headerPropName);
        }

#region Preset Validation

        /* This is a check for if the preset cache is invalid & should be rebuild from scratch */
        [InitializeOnLoadMethod]
        static void CheckPresetCache()
        {
            // Check if any chached presets do not exist anymore
            InitializeDataStructures();
            bool cacheInvalid = false;
            foreach(KeyValuePair<string, PresetsCollection> collection in PresetCollections)
            {
                for(int i = 0; i < collection.Value.Guids.Count; i++)
                {
                    string guid = collection.Value.Guids[i];
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if(string.IsNullOrWhiteSpace(path))
                    {
                        cacheInvalid = true;
                        break;
                    }
                }
                if(cacheInvalid)
                {
                    break;
                }
            }

            // check if any material is not known
            if(!cacheInvalid)
            {
                string[] guids = AssetDatabase.FindAssets("t:material");
                foreach(string guid in guids)
                {
                    if(!KnownMaterials.Contains(guid))
                    {
                        cacheInvalid = true;
                        break;
                    }
                }
            }


            if(cacheInvalid)
            {
                Debug.Log("Preset cache invalid, rebuilding...");
                CreatePresetCache();
            }
        }

#endregion

#region Unity Menu Hooks

        [MenuItem("Assets/Thry/Materials/Mark as Preset",false,500)]
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
        }

        [MenuItem("Assets/Thry/Materials/Mark as Preset", true,500)]
        static bool MarkAsPresetValid()
        {
            return Selection.assetGUIDs.Select(g => AssetDatabase.GUIDToAssetPath(g)).
                All(p => AssetDatabase.GetMainAssetTypeAtPath(p) == typeof(Material));
        }

        [MenuItem("Assets/Thry/Materials/Remove as preset",false,500)]
        static void RemoveAsPreset()
        {
            IEnumerable<Material> mats = Selection.assetGUIDs.Select(g => AssetDatabase.GUIDToAssetPath(g)).
                Where(p => AssetDatabase.GetMainAssetTypeAtPath(p) == typeof(Material)).Select(p => AssetDatabase.LoadAssetAtPath<Material>(p));
            foreach (Material m in mats)
            {
                m.SetOverrideTag(TAG_IS_PRESET, "");
                Presets.RemovePreset(m);
            }
        }

        [MenuItem("Assets/Thry/Materials/Remove as preset", true,500)]
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
#endregion
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
                        popupGUI.TogglePreset(Presets.GetPresetMaterial(guid), isOn);
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
        string _collection;
        public void Init(string collection, List<string> names, List<string> guids, ShaderEditor shaderEditor)
        {
            this.shaderEditor = shaderEditor;
            this._collection = collection;
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
                addUnder.AddPresetStruct(Presets.DoesPresetExist(this._collection, names[i]), path[path.Length-1], names[i], guids[i]);
            }
        }

        void TogglePreset(Material m, bool on)
        {
            if (tickedPresets.Contains(m) && !on) tickedPresets.Remove(m);
            if (!tickedPresets.Contains(m) && on) tickedPresets.Add(m);
            Presets.ApplyFullList(shaderEditor, beforePreset, tickedPresets);
            shaderEditor.Repaint();
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