using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Thry.ThryEditor.Helpers;
using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor
{
    public class Presets : AssetPostprocessor
    {
        const string TAG_IS_MATERIAL_PRESET = "isPreset";
        const string TAG_IS_MATERIAL_SECTIONED_PRESET = "isSectionedPreset";
        const string TAG_MATERIAL_PRESET_NAME = "presetName";
        const string TAG_POSTFIX_IS_PROPERTY_PRESET = "_isPreset";
        const string TAG_POSTFIX_SECTION_NAME = "isSectionedPreset"; // Weird name, because of a leagcy bug
        const string FILE_NAME_CACHE = "Thry/preset_cache.txt";
        const string FILE_NAME_KNOWN_MATERIALS = "Thry/presets_known_materials.txt";
        const string PRESET_VERSION = "1.1.0";

        struct AppliedPreset
        {
            public string name;
            public Material preset;
            public Material prePresetState;
            public ShaderPart parent;

            public static AppliedPreset Create(string name, Material preset, Material currentState, ShaderPart parent)
            {
                AppliedPreset appliedPreset = new AppliedPreset();
                appliedPreset.name = name;
                appliedPreset.preset = preset;
                appliedPreset.prePresetState = new Material(currentState);
                appliedPreset.prePresetState.name = "Before " + name;
                appliedPreset.parent = parent;
                return appliedPreset;
            }
        }
        
        static Comparer<string> s_nameComparer = Comparer<string>.Create((a, b) =>
        {
            // Compare by name, names with more slashes (/) are considered to be more specific
            int aSlashCount = a.Count(c => c == '/');
            int bSlashCount = b.Count(c => c == '/');
            if (aSlashCount > bSlashCount) return -1;
            if (aSlashCount < bSlashCount) return 1;
            return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
        });

        class PresetsCollection
        {
            private SortedDictionary<string, string> _nameToGuid = new SortedDictionary<string, string>(s_nameComparer);
            private Dictionary<string, string> _guidToName = new Dictionary<string, string>();
            public IEnumerable<string> Guids => _nameToGuid.Values;
            public IEnumerable<string> Paths => _nameToGuid.Values.Select(g => AssetDatabase.GUIDToAssetPath(g));
            public IEnumerable<string> Names => _nameToGuid.Keys.OrderBy(s => s, s_nameComparer);
            public int Count => _nameToGuid.Count;

            public void Remove(string guid)
            {
                if (_guidToName.ContainsKey(guid))
                {
                    _nameToGuid.Remove(_guidToName[guid]);
                    _guidToName.Remove(guid);
                }
            }

            public bool Add(string name, string guid)
            {
                if (_nameToGuid.ContainsKey(name))
                {
                    return false;
                }
                if (_guidToName.ContainsKey(guid))
                {
                    return false;
                }
                _nameToGuid[name] = guid;
                _guidToName[guid] = name;
                return true;
            }

            public void AddOrUpdate(string name, string guid)
            {
                if (_guidToName.ContainsKey(guid))
                {
                    _nameToGuid.Remove(_guidToName[guid]);
                }
                _guidToName[guid] = name;
                _nameToGuid[name] = guid;
            }

            public void RemoveWithoutPath()
            {
                var guids = _guidToName.Keys.Where(k => string.IsNullOrWhiteSpace(AssetDatabase.GUIDToAssetPath(k))).ToList();
                foreach (string guid in guids)
                {
                    _nameToGuid.Remove(_guidToName[guid]);
                    _guidToName.Remove(guid);
                }
            }

            public bool ContainsName(string name)
            {
                return _nameToGuid.ContainsKey(name);
            }

            public string GetGuid(string name)
            {
                return _nameToGuid[name];
            }

            public void Serialize(StringBuilder sb)
            {
                foreach (KeyValuePair<string, string> entry in _nameToGuid)
                {
                    sb.AppendLine($"{entry.Key};{entry.Value}");
                }
            }

            public void AddSerialized(string line)
            {
                string[] split = line.Split(';');
                _nameToGuid[split[0]] = split[1];
                _guidToName[split[1]] = split[0];
            }
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

        static Dictionary<Material, AppliedPreset> s_appliedPresets = new Dictionary<Material, AppliedPreset>();
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

            if(File.Exists(FILE_NAME_CACHE))
            {
                LoadPresetCache();
            }else
            {
                CreatePresetCache();
            }
        }

        static void ClearCache()
        {
            s_presetCollections.Clear();
            s_presetCollections["_full_"] = new PresetsCollection();
        }

        static void LoadPresetCache()
        {
            string[] lines = File.ReadAllLines(FILE_NAME_CACHE);
            bool isEmpty = lines.Length == 0;
            bool isOutOfDate = !isEmpty && lines[0] != PRESET_VERSION;

            if(isEmpty || isOutOfDate)
            {
                if(isOutOfDate)
                {
                    ThryLogger.LogWarn("Preset cache is out of date, rebuilding...");
                }
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
                    s_presetCollections[currentCollection].AddSerialized(lines[i]);
                }                    
            }
        }

        static void CreatePresetCache()
        {
            // Delete old cache
            ClearCache();
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

        public static void RebuildCache()
        {
            CreatePresetCache();
        }

        static Dictionary<Shader, List<string>> s_headersInShader = new Dictionary<Shader, List<string>>();
        static List<string> GetHeadersInShader(Material m)
        {       
            if(s_headersInShader.ContainsKey(m.shader))
            {
                return s_headersInShader[m.shader];
            }
            string[] props = MaterialHelper.GetFloatPropertiesFromSerializedObject(m);
            return props.Where(p => p.StartsWith("m_", StringComparison.Ordinal)).ToList();
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
                collection.Value.RemoveWithoutPath();
                collection.Value.Serialize(sb);
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
                    ThryLogger.LogDetail($"Material Changed: {material.name} ({AssetDatabase.AssetPathToGUID(asset)})");
                    // Check if asset is preset
                    if (IsPreset(material))
                    {
                        // Add preset
                        RemovePreset(material);
                        AddPreset(material);
                    }
                    KnownMaterials.Add(AssetDatabase.AssetPathToGUID(asset));
                }
            }

            if(deletedAssets.Length > 0)
            { 
                // go through all preset collections
                Dictionary<string, string> pathsToGuids = PresetCollections.
                    SelectMany(c => c.Value.Guids).Distinct(). // Guids of all preset materials. Because of sectioned can exists multiples
                    Select(g => (AssetDatabase.GUIDToAssetPath(g), g)). // Tuple of path and guid
                    ToDictionary(k => k.Item1, v => v.Item2);
                // Check if any presets were deleted, iterate over all deleted materials
                foreach (string asset in deletedAssets.Where(a => a.EndsWith(".mat")))
                {
                    // Check if asset is preset
                    if (pathsToGuids.ContainsKey(asset))
                    {
                        // Remove preset
                        RemovePreset(pathsToGuids[asset]);
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
                        string name = material.GetTag(header + TAG_POSTFIX_SECTION_NAME, false, "").Replace(';', '_');
                        if(string.IsNullOrEmpty(name))
                        {
                            ThryLogger.LogErr($"Preset {material.name} has no name for section '{header}'");
                            continue;
                        }
                        if(!PresetCollections.ContainsKey(collectionName))
                        {
                            PresetCollections[collectionName] = new PresetsCollection();
                        }
                        
                        if(PresetCollections[collectionName].Add(name, guid))
                        {
                            ThryLogger.LogDetail($"Add preset for section '{header}': {name} ({guid})");
                        }else
                        {
                            ThryLogger.LogWarn($"Preset '{name}' already exists in section '{header}'");
                        }
                    }
                }
            }else
            {
                // Add to full preset collection
                string name = material.GetTag(TAG_MATERIAL_PRESET_NAME, false, material.name).Replace(';', '_');
                if(PresetCollections["_full_"].Add(name, guid))
                {
                    ThryLogger.LogDetail($"Add preset: {name} ({guid})");
                }else
                {
                    ThryLogger.LogWarn($"Preset '{name}' already exists");
                }
            }
            s_materalCache[guid] = material;

            // Save cache
            Save();
        }

        static void RemovePreset(Material material)
        {
            // Get guid
            ThryLogger.LogDetail($"Remove preset: {material.name}");
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(material));
            RemovePreset(guid);
        }

        static void RemovePreset(string guid)
        {
            foreach(PresetsCollection collection in PresetCollections.Values)
            {
                collection.Remove(guid);
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
                 PresetCollections[collection].ContainsName(presetName);
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
                window.Init(collection, PresetCollections[collection].Names.ToList(), PresetCollections[collection].Guids.ToList(), shaderEditor);
                window.titleContent = new GUIContent("Preset List");
                window.ShowUtility();
            }
            else
            {
                ThryLogger.Log($"Open Quick Presets Menu: {collection} ({PresetCollections[collection].Count} presets)");
                EditorUtility.DisplayCustomMenu(r, 
                    PresetCollections[collection].Names.Select(s => new GUIContent(s)).ToArray(), -1, 
                    ApplyQuickPreset, new object[]{shaderEditor, collection, shaderEditor.CurrentProperty});
            }
        }

        static void ApplyQuickPreset(object userData, string[] options, int selected)
        {
            ThryLogger.Log($"Apply quick preset '{options[selected]}'");
            ShaderEditor shaderEditor = (userData as object[])[0] as ShaderEditor;
            string collection = (userData as object[])[1] as string;
            ShaderPart parent = (userData as object[])[2] as ShaderPart;
            Apply(collection, options[selected], shaderEditor, parent);
        }

        public static void PresetEditorGUI(ShaderEditor shaderEditor)
        {
            if (shaderEditor.IsPresetEditor)
            {
                RectifiedLayout.Seperator();

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
                    string name = shaderEditor.Materials[0].GetTag(TAG_MATERIAL_PRESET_NAME, false, "");
                    EditorGUI.BeginChangeCheck();
                    name = EditorGUILayout.DelayedTextField(EditorLocale.editor.Get("preset_name"), name);
                    if (EditorGUI.EndChangeCheck())
                    {
                        InitializeDataStructures();
                        string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(shaderEditor.Materials[0]));
                        shaderEditor.Materials[0].SetOverrideTag(TAG_MATERIAL_PRESET_NAME, name);
                        FullPresets.AddOrUpdate(name, guid);
                        Save();
                    }
                }

                RectifiedLayout.Seperator();
                GUILayout.Space(10);
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
            Material key = shaderEditor.Materials[0];
            string guid = PresetCollections[collection].GetGuid(name);
            Material preset = GetPresetMaterial(guid);

            s_appliedPresets[key] = AppliedPreset.Create(name, preset, shaderEditor.Materials[0], parent);
            ApplyPresetInternal(shaderEditor, preset, preset, parent);
            foreach (Material m in shaderEditor.Materials)
                MaterialEditor.ApplyMaterialPropertyDrawers(m);
        }

        static void Revert(ShaderEditor shaderEditor)
        {
            Material key = shaderEditor.Materials[0];
            AppliedPreset appliedPreset = s_appliedPresets[key];
            
            ThryLogger.Log($"Revert '{appliedPreset.preset.name}' from '{key.name}'");
            ApplyPresetInternal(shaderEditor, appliedPreset.preset, appliedPreset.prePresetState, appliedPreset.parent);
            foreach (Material m in shaderEditor.Materials)
                MaterialEditor.ApplyMaterialPropertyDrawers(m);
            s_appliedPresets.Remove(key);
        }

        public static void ApplyFullList(ShaderEditor shaderEditor, Material[] originals, List<Material> presets)
        {
            for(int i=0;i<shaderEditor.Materials.Length && i < originals.Length;i++)
                shaderEditor.Materials[i].CopyPropertiesFromMaterial(originals[i]);
            shaderEditor.UpdatePropertyReferences();
            foreach (Material preset in presets)
            {
                ApplyPresetInternal(shaderEditor, preset, preset, null);
            }
            shaderEditor.ApplyDrawers();
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
                ThryLogger.LogDetail($"Apply preset '{preset.name}' to '{shaderEditor.Materials[0].name}'");
                foreach (ShaderPart part in shaderEditor.ShaderParts)
                {
                    if (IsPreset(preset, part))
                    {
                        if(part is ShaderGroup)
                            part.CopyFrom(copyFrom, applyDrawers: false, copyReferenceProperties: true, deepCopy: true);
                        else
                            part.CopyFrom(copyFrom, applyDrawers: false, copyReferenceProperties: false);
                    }
                }
            }else if(parent is ShaderGroup)
            {
                ThryLogger.LogDetail($"Apply values from '{copyFrom.name}' to '{parent.Content.text}' group");
                ApplyPresetRecursive(preset, copyFrom, parent as ShaderGroup);
            }
            preset.shader = prev;
        }
        
        static void ApplyPresetRecursive(Material preset, Material copyFrom, ShaderGroup parent)
        {
            foreach (ShaderPart part in parent.Children)
            {
                if(part is ShaderGroup)
                {
                    ApplyPresetRecursive(preset, copyFrom, part as ShaderGroup);
                }
                if (IsPreset(preset, part))
                {
                    // ThryDebug.Detail($"Apply values from '{copyFrom.name}' to '{part.Content.text}' ({copyFrom.name} -> {part.MaterialProperty.targets[0].name}) ({MaterialHelper.GetValue(part.MaterialProperty)} -> {MaterialHelper.GetValue(copyFrom, part.MaterialProperty.name)})");
                    part.CopyFrom(copyFrom, applyDrawers: false);
                }
            }
        }

        public static void SetProperty(Material m, ShaderPart prop, bool value)
        {
            if (prop.CustomStringTagID  != null) m.SetOverrideTag(prop.CustomStringTagID + TAG_POSTFIX_IS_PROPERTY_PRESET, value ? "true" : "");
            if (prop.MaterialProperty   != null) m.SetOverrideTag(prop.MaterialProperty.name + TAG_POSTFIX_IS_PROPERTY_PRESET, value ? "true" : "");
            if (prop.PropertyIdentifier != null) m.SetOverrideTag(prop.PropertyIdentifier    + TAG_POSTFIX_IS_PROPERTY_PRESET, value ? "true" : "");
        }

        public static bool IsPreset(Material m, ShaderPart prop)
        {
            if (prop.CustomStringTagID  != null) return m.GetTag(prop.CustomStringTagID + TAG_POSTFIX_IS_PROPERTY_PRESET, false, "") == "true";
            if (prop.MaterialProperty   != null) return m.GetTag(prop.MaterialProperty.name + TAG_POSTFIX_IS_PROPERTY_PRESET, false, "") == "true";
            if (prop.PropertyIdentifier != null) return m.GetTag(prop.PropertyIdentifier    + TAG_POSTFIX_IS_PROPERTY_PRESET, false, "") == "true";
            return false;
        }

        public static bool ArePreset(Material[] mats)
        {
            return mats.All(m => IsPreset(m));
        }

        public static bool IsPreset(Material m)
        {
            return m?.GetTag(TAG_IS_MATERIAL_PRESET, false, "false") == "true";
        }
        
        public static void SetPreset(IEnumerable<Material> mats, bool set)
        {
            if (set)
            {
                foreach (Material m in mats)
                {
                    if(m == null) continue;
                    m.SetOverrideTag(TAG_IS_MATERIAL_PRESET, "true");
                    if (m.GetTag("presetName", false, "") == "") m.SetOverrideTag("presetName", m.name);
                    Presets.AddPreset(m);
                }
            }
            else
            {
                foreach (Material m in mats)
                {
                    if(m == null) continue;
                    m.SetOverrideTag(TAG_IS_MATERIAL_PRESET, "");
                    Presets.RemovePreset(m);
                }
            }
        }

        public static bool IsMaterialSectionedPreset(Material m)
        {
            return m?.GetTag(TAG_IS_MATERIAL_SECTIONED_PRESET, false, "false") == "true";
        }

        public static void SetMaterialSectionedPreset(Material m, bool value)
        {
            m.SetOverrideTag(TAG_IS_MATERIAL_SECTIONED_PRESET, value ? "true" : "");
            RemovePreset(m);
            AddPreset(m);   
        }

        public static bool IsSectionPreset(Material m, string headerPropName)
        {
            return !string.IsNullOrWhiteSpace(m.GetTag(headerPropName + TAG_POSTFIX_SECTION_NAME, false, ""));
        }

        public static string GetSectionPresetName(Material m, string headerPropName)
        {
            return m.GetTag(headerPropName + TAG_POSTFIX_SECTION_NAME, false, "");
        }

        public static void SetSectionPreset(Material m, string headerPropName, string name)
        {
            m.SetOverrideTag(headerPropName + TAG_POSTFIX_SECTION_NAME, name);

            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m));
            if(!string.IsNullOrWhiteSpace(name))
            {
                if(!PresetCollections.ContainsKey(headerPropName))
                {
                    PresetCollections[headerPropName] = new PresetsCollection();
                }
    	        
                PresetCollections[headerPropName].AddOrUpdate(name, guid);
                ThryLogger.LogDetail($"Add preset for section '{headerPropName}': {name} ({guid})");
            }else
            {
                if(PresetCollections.ContainsKey(headerPropName))
                {
                    PresetCollections[headerPropName].Remove(guid);
                    ThryLogger.LogDetail($"Remove preset for section '{headerPropName}' ({guid})");
                }
            }
        }

        public static bool DoesSectionHavePresets(string headerPropName)
        {
            return PresetCollections.ContainsKey(headerPropName) && PresetCollections[headerPropName].Count > 0;
        }

#region Preset Validation

        /* This is a check for if the preset cache is invalid & should be rebuild from scratch */
        [InitializeOnLoadMethod]
        static void CheckPresetCache()
        {
            // Check if any chached presets do not exist anymore
            InitializeDataStructures();
            bool cacheInvalid = PresetCollections.Values.Any(c => c.Paths.Any(p => string.IsNullOrWhiteSpace(p)));

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
                ThryLogger.Log("Preset cache invalid, rebuilding...");
                CreatePresetCache();
            }
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
                    Rect r = GUILayoutUtility.GetRect(new GUIContent(), Styles.dropdownHeader);
                    r.x = EditorGUI.indentLevel * 15;
                    r.width -= r.x;
                    GUI.Box(r, name, Styles.dropdownHeader);
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
            shaderEditor.Reload(true);
        }
    }
}