using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Thry{
    public class Localization : ScriptableObject
    {
        [SerializeField] Shader[] ValidateWithShaders;
        [SerializeField] string DefaultLanguage = "English";
        [SerializeField] string[] Languages = new string[0];
        [SerializeField] int SelectedLanguage = -1;
        [SerializeField] string[] _keys = new string[0]; 
        [SerializeField] string[] _values = new string[0];

        Dictionary<string, string[]> _localizedStrings = new Dictionary<string, string[]>();
        string[] _allLanguages;
        bool _isLoaded = false;
        bool _couldNotLoad = false;

        // Use
        public static Localization Load(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Localization l = AssetDatabase.LoadAssetAtPath<Localization>(path);
            if(l == null)
            {
                l = ScriptableObject.CreateInstance<Localization>();
                l._couldNotLoad = true;
                return l;
            }
            l.Load();
            return l;
        }

        void Load()
        {
            // Load languages
            _allLanguages = new string[Languages.Length + 1];
            _allLanguages[0] = DefaultLanguage;
            Array.Copy(Languages, 0, _allLanguages, 1, Languages.Length);
            
            _localizedStrings = new Dictionary<string, string[]>();
            for (int i = 0; i < _keys.Length; i++)
            {
                string[] ar = new string[Languages.Length];
                Array.Copy(_values, i * Languages.Length , ar, 0, Languages.Length);
                _localizedStrings[_keys[i]] = ar;
            }
            _isLoaded = true;
        }

        public static Localization Create()
        {
            Localization l = ScriptableObject.CreateInstance<Localization>();
            l._allLanguages = new string[l.Languages.Length + 1];
            l._allLanguages[0] = l.DefaultLanguage;
            Array.Copy(l.Languages, 0, l._allLanguages, 1, l.Languages.Length);
            l._localizedStrings = new Dictionary<string, string[]>();
            return l;
        }

        public void DrawDropdown()
        {
            if(_couldNotLoad)
            {
                EditorGUILayout.HelpBox("Could not load localization file", MessageType.Warning);
                return;
            }
            EditorGUI.BeginChangeCheck();
            SelectedLanguage = EditorGUILayout.Popup(SelectedLanguage + 1, _allLanguages) - 1;
            if(EditorGUI.EndChangeCheck())
            {
                ShaderEditor.Active.Reload();
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }

        public string Get(MaterialProperty prop, string defaultValue)
        {
            return Get(prop.name, defaultValue);
        }

        public string Get(MaterialProperty prop, FieldInfo field, string defaultValue)
        {
            string id = prop.name + "." + field.DeclaringType + "." + field.Name;
            return Get(id, defaultValue);
        }

        public string Get(string id, string defaultValue)
        {
            if(id == null) return defaultValue;
            if (_localizedStrings.ContainsKey(id))
            {
                string[] ar = _localizedStrings[id];
                if (ar.Length > SelectedLanguage && SelectedLanguage > -1)
                {
                    return ar[SelectedLanguage] ?? defaultValue;
                }
            }
            return defaultValue;
        }

        // Managment

        void AddLanguage(string language)
        {
            if (System.Array.IndexOf(Languages, language) == -1)
            {
                System.Array.Resize(ref Languages, Languages.Length + 1);
                Languages[Languages.Length - 1] = language;
                string[] keys = _localizedStrings.Keys.ToArray();
                foreach(string key in keys)
                {
                    string[] ar = _localizedStrings[key];
                    System.Array.Resize(ref ar, ar.Length + 1);
                    ar[ar.Length - 1] = null;
                    _localizedStrings[key] = ar;
                }
                Save();
            }
        }

        void RemoveLanguage(string language)
        {
            int index = System.Array.IndexOf(Languages, language);
            if (index != -1)
            {
                if(Languages.Length > 1)
                {
                    for (int i = index; i < Languages.Length - 1; i++)
                    {
                        Languages[i] = Languages[i + 1];
                    }
                    System.Array.Resize(ref Languages, Languages.Length - 1);
                    string[] keys = _localizedStrings.Keys.ToArray();
                    foreach (string key in keys)
                    {
                        string[] ar = _localizedStrings[key];
                        for (int i = index; i < ar.Length - 1; i++)
                        {
                            ar[i] = ar[i + 1];
                        }
                        System.Array.Resize(ref ar, ar.Length - 1);
                        _localizedStrings[key] = ar;
                    }
                }else
                {
                    Languages = new string[0];
                    _localizedStrings = new Dictionary<string, string[]>();
                }
                Save();
            }
        }

        void Save()
        {
            _keys = _localizedStrings.Keys.ToArray();
            _values = new string[_keys.Length * Languages.Length];
            for (int i = 0; i < _keys.Length; i++)
            {
                string[] ar = _localizedStrings[_keys[i]];
                Array.Copy(ar, 0, _values, i * Languages.Length, ar.Length);
            }
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        void Clear()
        {
            _keys = new string[0];
            _values = new string[0];
            Languages = new string[0];
            _localizedStrings.Clear();
        }

        [MenuItem("Assets/Thry/Shaders/Create Locale File", false)]
        static void CreateLocale()
        {
            Localization locale = ScriptableObject.CreateInstance<Localization>();
            Shader[] shaders = Selection.objects.Select(o => o as Shader).ToArray();
            string fileName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(shaders[0]));
            string folderPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(shaders[0]));
            locale.ValidateWithShaders = shaders;
            AssetDatabase.CreateAsset(locale, folderPath + "/" + fileName + "_Locale.asset");
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Assets/Thry/Shaders/Create Locale File", true)]
        static bool ValidateCreateLocale()
        {
            return Selection.objects.All(o => o is Shader);
        }

        [MenuItem("Assets/Thry/Shaders/Locale Property", false)]
        static void CreateShaderProperty()
        {
            Localization l = Selection.activeObject as Localization;
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(l));
            string outS = $"[HideInInspector] {ShaderEditor.PROPERTY_NAME_LOCALE} (\"{guid}\", Float) = 0";
            EditorGUIUtility.systemCopyBuffer = outS;
        }

        [MenuItem("Assets/Thry/Shaders/Locale Property", true)]
        static bool ValidateCreateShaderProperty()
        {
            return Selection.activeObject is Localization;
        }

        [CustomEditor(typeof(Localization))]
        public class LocaleEditor : Editor
        {
            List<(string key, string defaultValue, string newValue)> _missingKeys = new List<(string key, string defaultValue, string newValue)>();
            Dictionary<string, string> _defaultPropertyContent = new Dictionary<string, string>();
            int _selectedLanguageIndex = 0;
            string _searchById = "";
            string _searchByTranslation = "";
            string[] _searchResults = new string[0];

            string _translateByValueIn = "";
            string _translateByValueOut = "";
            string _autoTranslateLanguageShortCode = "EN";

            string ToCSVString(string s)
            {
                if(s == null)
                    return "";
                return "\"" + s.Replace("\"", "“") + "\"";
            }

            string FromCSVString(string s)
            {
                return s.Trim('"').Replace("“", "\"");
            }

            void ExportAsCSV(Localization locale)
            {
                string path = EditorUtility.SaveFilePanel("Export as CSV", "", locale.name, "csv");
                if (string.IsNullOrEmpty(path) == false)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    foreach (string language in locale.Languages)
                    {
                        sb.Append("," + ToCSVString(language));
                    }
                    sb.AppendLine();
                    for(int i = 0;i < locale._keys.Length; i++)
                    {
                        sb.Append(ToCSVString(locale._keys[i]));
                        for(int j = 0; j < locale.Languages.Length; j++)
                        {
                            sb.Append("," + ToCSVString(locale._values[i * locale.Languages.Length + j]));
                        }
                        sb.AppendLine();
                    }
                    File.WriteAllText(path, sb.ToString());
                }
            }

            void LoadFromCSV(Localization locale)
            {
                string path = EditorUtility.OpenFilePanel("Load from CSV", "", "csv");
                if (string.IsNullOrEmpty(path) == false)
                {
                    string[] lines = File.ReadAllLines(path);
                    if (lines.Length > 0)
                    {
                        locale.Clear();
                        string[] languages = lines[0].Split(',');
                        for (int i = 1; i < languages.Length; i++)
                        {
                            locale.AddLanguage(FromCSVString(languages[i]));
                        }

                        locale._values = new string[(lines.Length - 1) * (languages.Length - 1)];
                        locale._keys = new string[lines.Length - 1];

                        for (int i = 1; i < lines.Length; i++)
                        {
                            string[] values = lines[i].Split(',');
                            if (values.Length > 0)
                            {
                                string key = FromCSVString(values[0]);
                                locale._keys[i - 1] = key;
                                for (int j = 1; j < values.Length; j++)
                                {
                                    locale._values[(i - 1) * (languages.Length - 1) + j - 1] = FromCSVString(values[j]);
                                }
                            }
                        }
                        locale.Load();
                        locale.Save();
                    }
                }
            }

            void UpdateMissing(Localization locale)
            {
                _missingKeys.Clear();
                foreach(string key in locale._localizedStrings.Keys)
                {
                    if (string.IsNullOrEmpty(locale._localizedStrings[key][_selectedLanguageIndex]))
                    {
                        if(_defaultPropertyContent.ContainsKey(key) && !string.IsNullOrWhiteSpace(_defaultPropertyContent[key]))
                        {
                            _missingKeys.Add((key, _defaultPropertyContent[key], _defaultPropertyContent[key]));
                        }
                    }
                }
            }

            void UpdateData(Localization locale)
            {
                locale.Load();

                // Gather all keys from all shaders
                List<MaterialProperty> allProps = new List<MaterialProperty>();
                foreach(Shader s in locale.ValidateWithShaders)
                {
                    allProps.AddRange(
                        MaterialEditor.GetMaterialProperties(new Material[]{new Material(s)})
                    );
                }
                // Make unique by propname
                allProps = allProps.GroupBy(p => p.name).Select(g => g.First()).ToList();
                _defaultPropertyContent.Clear();

                // add all keys from shader
                foreach(var prop in allProps)
                {
                    string key = prop.name;
                    string value = prop.displayName;
                    int seperatorIndex = value.IndexOf("--", StringComparison.Ordinal);
                    if(seperatorIndex != -1)
                    {
                        value = value.Substring(0, seperatorIndex).Trim();
                    }

                    if(key.StartsWith("footer_")) continue;
                    if(key == ShaderEditor.PROPERTY_NAME_MASTER_LABEL) continue;
                    if(key == ShaderEditor.PROPERTY_NAME_LABEL_FILE) continue;
                    if(key == ShaderEditor.PROPERTY_NAME_LOCALE) continue;
                    if(key == ShaderEditor.PROPERTY_NAME_ON_SWAP_TO_ACTIONS) continue;
                    if(key == ShaderEditor.PROPERTY_NAME_SHADER_VERSION) continue;
                    if(key == ShaderEditor.PROPERTY_NAME_EDITOR_DETECT) continue;
                    if (!string.IsNullOrWhiteSpace(value) && !locale._localizedStrings.ContainsKey(key))
                    {
                        locale._localizedStrings.Add(key, new string[locale.Languages.Length]);
                    }
                    _defaultPropertyContent.Add(key, value);
                }
                // make missing keys a list of all keys that have an empty string in the selected language
                UpdateMissing(locale);
            }

            private void OnEnable() 
            {
                Localization locale = (Localization)target;
                locale.Load();
                UpdateData(locale);
            }

            private void Awake() {
                Localization locale = (Localization)target;
                locale.Load();
                UpdateData(locale);
            }

            public override void OnInspectorGUI()
            {
                Localization locale = (Localization)target;
                if(!locale._isLoaded)
                {
                    UpdateData(locale);
                }

                if(GUILayout.Button("Save", GUILayout.Height(50)))
                {
                    locale.Save();
                }
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                GUIShaders(locale);

                EditorGUILayout.Space(20);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.LabelField("Languages", EditorStyles.boldLabel);
                GUILanguages(locale);

                EditorGUILayout.Space(20);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.LabelField("Import / Export", EditorStyles.boldLabel);
                GUICSV(locale);

                if(locale.Languages.Length == 0) return;

                EditorGUILayout.Space(20);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.LabelField("Select Language To Edit", EditorStyles.boldLabel);
                GUIEditLanguageSelection(locale);

                EditorGUILayout.Space(20);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.LabelField("Missing Entries", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("This will search all properties and list all that have no translation for the selected language.", MessageType.Info);
                GUIMissingEntries(locale);

                EditorGUILayout.Space(20);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.LabelField("Automatic Translation using Google", EditorStyles.boldLabel);
                GUIGoogleTranslate(locale);

                EditorGUILayout.Space(20);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.LabelField("Translate entries by value", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("This will search all properties and translate all that have the exact display name with the selected value. Suggested usecase: Panning, UV", MessageType.Info);
                GUIValueTranslate(locale);

                EditorGUILayout.Space(20);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.LabelField("Existing Entries", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("This will search all properties allows editing or removing them.", MessageType.Info);
                GUIEdit(locale);

            }

            void GUIShaders(Localization locale)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ValidateWithShaders"));
                if(GUILayout.Button("Load Properties from Shaders"))
                {
                    // for each shader create a material & material editor so that the data is loaded into the localization object
                    foreach(Shader s in locale.ValidateWithShaders)
                    {
                        ShaderEditor se = new ShaderEditor();
                        se.FakePartialInitilizationForLocaleGathering(s);
                    }
                }
            }

            void GUILanguages(Localization locale)
            {
                locale.DefaultLanguage = EditorGUILayout.TextField("Default Language", locale.DefaultLanguage);

                EditorGUILayout.LabelField("Languages");
                for (int i = 0; i < locale.Languages.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    locale.Languages[i] = EditorGUILayout.TextField(locale.Languages[i]);
                    if (GUILayout.Button("Remove"))
                    {
                        locale.RemoveLanguage(locale.Languages[i]);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add"))
                {
                    locale.AddLanguage("New Language");
                }
                EditorGUILayout.EndHorizontal();
            }

            void GUIEditLanguageSelection(Localization locale)
            {
                EditorGUI.BeginChangeCheck();
                _selectedLanguageIndex = EditorGUILayout.Popup("Language to edit", _selectedLanguageIndex, locale.Languages);
                if(EditorGUI.EndChangeCheck())
                {
                    _missingKeys.Clear();
                }

                if(GUILayout.Button("Update Missing Entries"))
                {
                    UpdateData(locale);
                }
            }

            void GUICSV(Localization locale)
            {
                if(GUILayout.Button("Load from CSV"))
                {
                    LoadFromCSV(locale);
                }
                if(locale.Languages.Length == 0) return;
                if(GUILayout.Button("Export as CSV"))
                {
                    ExportAsCSV(locale);
                }
            }

            void GUIMissingEntries(Localization locale)
            {
                (string,string,string) kvToRemove = default;
                for(int i = 0; i < _missingKeys.Count && i < 10; i++)
                {
                    var kv = _missingKeys[i];
                    EditorGUILayout.BeginHorizontal();
                    kv.newValue = EditorGUILayout.DelayedTextField(kv.key, kv.newValue);
                    if(GUILayout.Button("Skip", GUILayout.Width(50)))
                    {
                        kvToRemove = kv;
                    }
                    if(GUILayout.Button("Apply", GUILayout.Width(50)))
                    {
                        if (!locale._localizedStrings.ContainsKey(kv.key))
                        {
                            locale._localizedStrings.Add(kv.key, new string[locale.Languages.Length]);
                        }
                        locale._localizedStrings[kv.key][_selectedLanguageIndex] = kv.newValue;
                        kvToRemove = kv;
                    }
                    _missingKeys[i] = kv;
                    EditorGUILayout.EndHorizontal();
                }
                if(_missingKeys.Count > 10)
                {
                    EditorGUILayout.LabelField("...");
                }
                if(kvToRemove != default)
                {
                    _missingKeys.Remove(kvToRemove);
                }
            }

            void GUIGoogleTranslate(Localization locale)
            {
                _autoTranslateLanguageShortCode = EditorGUILayout.TextField("Language Short Code", _autoTranslateLanguageShortCode);
                EditorGUILayout.HelpBox("Short code must be valid short code. See https://cloud.google.com/translate/docs/languages for a list of valid short codes.", MessageType.Info);   
                if(Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    Application.OpenURL("https://cloud.google.com/translate/docs/languages");
                }
                if(GUILayout.Button("Auto Translate"))
                {
                    int _missingKeysCount = _missingKeys.Count;
                    int i = 0;
                    foreach((string key, string defaultValue, string newValue) in _missingKeys)
                    {
                        EditorUtility.DisplayProgressBar("Auto Translate", $"Translating {i}/{_missingKeysCount}", (float)i / _missingKeysCount);
                        try
                        {
                            if (!locale._localizedStrings.ContainsKey(key))
                            {
                                locale._localizedStrings.Add(key, new string[locale.Languages.Length]);
                            }
                            locale._localizedStrings[key][_selectedLanguageIndex] = WebHelper.Translate(defaultValue, _autoTranslateLanguageShortCode);
                        }
                        catch(Exception e)
                        {
                            Debug.LogError(e);
                        }
                        i += 1;
                    }
                    EditorUtility.ClearProgressBar();
                    locale.Save();
                }
            }

            void GUIValueTranslate(Localization locale)
            {
                _translateByValueIn = EditorGUILayout.TextField("Search for", _translateByValueIn);
                _translateByValueOut = EditorGUILayout.TextField("Translate with", _translateByValueOut);
                if(GUILayout.Button("Execute"))
                {
                    foreach(var kv in _defaultPropertyContent)
                    {
                        if(kv.Value == _translateByValueIn)
                        {
                            locale._localizedStrings[kv.Key][_selectedLanguageIndex] = _translateByValueOut;
                        }
                    }
                    UpdateMissing(locale);
                }
            }

            void GUIEdit(Localization locale)
            {
                EditorGUI.BeginChangeCheck();
                _searchById = EditorGUILayout.TextField("Search by id", _searchById);
                _searchByTranslation = EditorGUILayout.TextField("Search by translation", _searchByTranslation);
                if(EditorGUI.EndChangeCheck())
                {
                    List<string> res = new List<string>();
                    foreach (string key in locale._localizedStrings.Keys)
                    {
                        if(locale._localizedStrings[key][_selectedLanguageIndex] == null) continue;
                        if(locale._localizedStrings[key][_selectedLanguageIndex].IndexOf(_searchByTranslation, StringComparison.OrdinalIgnoreCase) != -1
                         && key.IndexOf(_searchById, StringComparison.OrdinalIgnoreCase) != -1)
                        {
                            res.Add(key);
                        }
                    }
                    _searchResults = res.ToArray();
                }
                EditorGUILayout.Space(5);
                if(_searchById.Length > 0 || _searchByTranslation.Length > 0)
                {
                    int count = 0;
                    foreach (string key in _searchResults)
                    {
                        if(count > 50)
                        {
                            EditorGUILayout.LabelField("...");
                            break;
                        }
                        EditorGUILayout.BeginHorizontal();
                        string value = EditorGUILayout.DelayedTextField(key, locale._localizedStrings[key][_selectedLanguageIndex]);
                        if (GUILayout.Button("Remove", GUILayout.Width(65)))
                        {
                            locale._localizedStrings[key][_selectedLanguageIndex] = "";
                        }
                        EditorGUILayout.EndHorizontal();
                        locale._localizedStrings[key][_selectedLanguageIndex] = value;
                        count++;
                    }
                }
            }
        }
    }
}

