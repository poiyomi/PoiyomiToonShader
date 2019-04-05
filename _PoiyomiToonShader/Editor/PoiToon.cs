//for most shaders

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class PoiToon : ShaderGUI
{

    public const string EXTRA_OFFSET_OPTION = "extraOffset"; //can be used to specify and extra x-offset for properties
    public const string CONFIG_FILE_PATH = "./Assets/poiToonEditorConfig.json"; //location of config file

    public static Config config;

    //variables for the presets
    private bool hasPresets = false;
    private bool presetsLoaded = false;
    private string presetsFilePath = null;
    private Dictionary<string, List<string[]>> presets = new Dictionary<string, List<string[]>>(); //presets

    //variabled for the preset selector
    public int selectedPreset = 0;
    private string[] presetOptions;

    ShaderHeader shaderparts; //stores headers and properties in correct order
    GUIStyle m_sectionStyle;

    public Dictionary<string, bool> showTextureScaleOffset = new Dictionary<string, bool>(); //if texture scale/offset fields are extended or not

    private class PoiToonHeader
    {
        private List<MaterialProperty> propertyes;
        private bool currentState;

        public PoiToonHeader(MaterialEditor materialEditor, string propertyName)
        {
            this.propertyes = new List<MaterialProperty>();
            foreach (Material materialEditorTarget in materialEditor.targets)
            {
                Object[] asArray = new Object[] { materialEditorTarget };
                propertyes.Add(MaterialEditor.GetMaterialProperty(asArray, propertyName));
            }

            this.currentState = fetchState();
        }

        public bool fetchState()
        {
            foreach (MaterialProperty materialProperty in propertyes)
            {
                if (materialProperty.floatValue == 1)
                    return true;
            }



            return false;
        }

        public bool getState()
        {
            return this.currentState;
        }

        public void Toggle()
        {

            if (getState())
            {
                foreach (MaterialProperty materialProperty in propertyes)
                {
                    materialProperty.floatValue = 0;
                }
            }
            else
            {
                foreach (MaterialProperty materialProperty in propertyes)
                {
                    materialProperty.floatValue = 1;
                }
            }

            this.currentState = !this.currentState;
        }
    }

    public static void linkButton(int Width, int Height, string title, string link)
    {
        if (GUILayout.Button(title, GUILayout.Width(Width), GUILayout.Height(Height)))
        {
            Application.OpenURL(link);
        }
    }

    private static class PoiToonUI
    {

        public static PoiToonHeader Foldout(ShaderHeader header)
        {
            var style = new GUIStyle("ShurikenModuleTitle");
            style.font = new GUIStyle(EditorStyles.label).font;
            style.border = new RectOffset(15, 7, 4, 4);
            style.fixedHeight = 22;
            style.contentOffset = new Vector2(20f, -2f);
            style.margin.left = 30 * header.xOffset;

            var rect = GUILayoutUtility.GetRect(16f + 20f, 22f, style);
            GUI.Box(rect, header.name, style);

            var e = Event.current;

            var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, header.header.getState(), false);
            }

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                header.header.Toggle();
                e.Use();
            }

            return header.header;
        }
    }

    //Condig class
    public class Config
    {
        public bool bigTextures;

        public string SaveToString()
        {
            return JsonUtility.ToJson(this);
        }

        public static Config GetDefaultConfig()
        {
            Config config = new Config();
            config.bigTextures = false;
            return config;
        }
    }

    //load the config from file
    public static void LoadConfig()
    {
        if (File.Exists(CONFIG_FILE_PATH))
        {
            StreamReader reader = new StreamReader(CONFIG_FILE_PATH);
            config = JsonUtility.FromJson<Config>(reader.ReadToEnd());
        }
        else
        {
            File.CreateText(CONFIG_FILE_PATH).Close();
            config = Config.GetDefaultConfig();
            saveConfig();
        }
    }

    //save the config to the file
    public static void saveConfig()
    {
        StreamWriter writer = new StreamWriter(CONFIG_FILE_PATH, true);
        writer.WriteLine(config.SaveToString());
        writer.Close();
        AssetDatabase.ImportAsset(CONFIG_FILE_PATH);
    }

    //classes for storing property data
    private class ShaderPart
    {
        public int xOffset = 0;
    }

    private class ShaderHeader : ShaderPart
    {
        public PoiToonHeader header;
        public List<ShaderPart> parts = new List<ShaderPart>();
        public string name;

        public ShaderHeader(PoiToonHeader header)
        {
            this.header = header;
        }

        public ShaderHeader(MaterialProperty prop, MaterialEditor materialEditor)
        {
            this.header = new PoiToonHeader(materialEditor, prop.name); ;
            this.name = prop.displayName;
        }

        public ShaderHeader(MaterialProperty prop, MaterialEditor materialEditor, int xOffset) : this(prop, materialEditor)
        {
            this.xOffset = xOffset;
        }

        public void addPart(ShaderPart part)
        {
            parts.Add(part);
        }
    }

    private class ShaderProperty : ShaderPart
    {
        public MaterialProperty materialProperty;
        public GUIContent style;


        public ShaderProperty(MaterialProperty materialProperty, GUIContent style)
        {
            this.materialProperty = materialProperty;
            this.style = style;
        }

        public ShaderProperty(MaterialProperty materialProperty)
        {
            this.materialProperty = materialProperty;
            this.style = new GUIContent(materialProperty.displayName, materialProperty.name + materialProperty.type);
        }

        public ShaderProperty(MaterialProperty materialProperty, string displayName, int xOffset)
        {
            this.xOffset = xOffset;
            this.materialProperty = materialProperty;
            this.style = new GUIContent(displayName, materialProperty.name + materialProperty.type);
        }
    }

    //used to parse extra options in display name like offset
    public static int propertyOptionToInt(string optionName, MaterialProperty p)
    {
        string pattern = @"-" + optionName + "=\\d+";
        Match match = Regex.Match(p.displayName, pattern);
        if (match.Success)
        {
            int ret = 0;
            string value = match.Value.Replace("-" + optionName + "=", "");
            int.TryParse(value, out ret);
            return ret;
        }
        return 0;
    }

    //finds all properties and headers and stores them in correct order
    private void CollectAllProperties(MaterialProperty[] props, MaterialEditor materialEditor)
    {
        shaderparts = new ShaderHeader(null);
        Stack<ShaderHeader> headerStack = new Stack<ShaderHeader>();
        headerStack.Push(shaderparts);
        headerStack.Push(shaderparts);
        int headerCount = 0;
        for (int i = 0; i < props.Length; i++)
        {
            if (props[i].name.StartsWith("m_end") && props[i].flags == MaterialProperty.PropFlags.HideInInspector)
            {
                headerStack.Pop();
                headerCount--;
            }
            else if (props[i].name.StartsWith("m_start") && props[i].flags == MaterialProperty.PropFlags.HideInInspector)
            {
                headerCount++;
                ShaderHeader newHeader = new ShaderHeader(props[i], materialEditor, headerCount);
                headerStack.Peek().addPart(newHeader);
                headerStack.Push(newHeader);
            }
            else if (props[i].name.StartsWith("m_") && props[i].flags == MaterialProperty.PropFlags.HideInInspector)
            {
                ShaderHeader newHeader = new ShaderHeader(props[i], materialEditor, headerCount);
                headerStack.Pop();
                headerStack.Peek().addPart(newHeader);
                headerStack.Push(newHeader);
            }
            else if (props[i].flags != MaterialProperty.PropFlags.HideInInspector)
            {
                int extraOffset = 0;
                extraOffset = propertyOptionToInt(EXTRA_OFFSET_OPTION, props[i]);
                string displayName = props[i].displayName.Replace("-" + EXTRA_OFFSET_OPTION + "=" + extraOffset, "");
                ShaderProperty newPorperty = new ShaderProperty(props[i], displayName, headerCount + extraOffset);
                headerStack.Peek().addPart(newPorperty);
            }

        }
    }

    private void SetupStyle()
    {
        m_sectionStyle = new GUIStyle(EditorStyles.boldLabel);
        m_sectionStyle.alignment = TextAnchor.MiddleCenter;
    }

    private void ToggleDefine(Material mat, string define, bool state)
    {
        if (state)
        {
            mat.EnableKeyword(define);
        }
        else
        {
            mat.DisableKeyword(define);
        }
    }

    void ToggleDefines(Material mat)
    {
    }

    void LoadDefaults(Material mat)
    {
    }

    void DrawHeader(ref bool enabled, ref bool options, GUIContent name)
    {
        var r = EditorGUILayout.BeginHorizontal("box");
        enabled = EditorGUILayout.Toggle(enabled, EditorStyles.radioButton, GUILayout.MaxWidth(15.0f));
        options = GUI.Toggle(r, options, GUIContent.none, new GUIStyle());
        EditorGUILayout.LabelField(name, m_sectionStyle);
        EditorGUILayout.EndHorizontal();
    }

    void DrawMasterLabel(string shaderName)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.richText = true;
        style.alignment = TextAnchor.MiddleCenter;

        EditorGUILayout.LabelField("<size=16>" + shaderName + "</size>", style, GUILayout.MinHeight(18));
    }

    //function to handle the drawing of header or property
    void drawShaderPart(ShaderPart part, MaterialEditor materialEditor)
    {
        if (part is ShaderHeader)
        {
            ShaderHeader header = (ShaderHeader)part;
            drawShaderHeader(header, materialEditor);
        }
        else
        {
            ShaderProperty property = (ShaderProperty)part;
            drawShaderProperty(property, materialEditor);
        }
    }

    //draw header
    void drawShaderHeader(ShaderHeader header, MaterialEditor materialEditor)
    {
        header.header = PoiToonUI.Foldout(header);
        if (header.header.getState())
        {
            EditorGUILayout.Space();
            foreach (ShaderPart part in header.parts)
            {
                drawShaderPart(part, materialEditor);
            }
            EditorGUILayout.Space();
        }
    }

    //draw property
    void drawShaderProperty(ShaderProperty property, MaterialEditor materialEditor)
    {
        if (property.materialProperty.type == MaterialProperty.PropType.Texture && config.bigTextures == false)
        {
            int oldIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = property.xOffset * 2 + 1;
            Rect rect = materialEditor.TexturePropertySingleLine(new GUIContent(property.materialProperty.displayName, "Click here for scale / offset"), property.materialProperty);

            var e = Event.current;
            bool value = false;
            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                if (showTextureScaleOffset.TryGetValue(property.materialProperty.name, out value))
                {
                    showTextureScaleOffset.Remove(property.materialProperty.name);
                }
                value = !value;
                showTextureScaleOffset.Add(property.materialProperty.name, value);

                e.Use();
            }
            if (!showTextureScaleOffset.TryGetValue(property.materialProperty.name, out value))
            {
                value = false;
            }
            if (value) materialEditor.TextureScaleOffsetProperty(property.materialProperty);

            EditorGUI.indentLevel = oldIndentLevel;
        }
        else
        {
            materialEditor.ShaderProperty(property.materialProperty, property.style.text, property.xOffset * 2 + 1);
        }
    }

    //finds a property in props by name, if it doesnt exist return null
    public static MaterialProperty FindProperty(MaterialProperty[] props, string name)
    {
        MaterialProperty ret = null;
        foreach (MaterialProperty p in props)
        {
            if (p.name == name) { ret = p; }
        }
        return ret;
    }

    //tries to load presets from file
    public void loadPresets(MaterialProperty[] props)
    {
        MaterialProperty presetsProperty = FindProperty(props, "shader_presets");
        if (!(presetsProperty == null))
        {
            string presetsFileName = presetsProperty.displayName;
            hasPresets = true;
            presets.Clear();

            string[] guid = AssetDatabase.FindAssets(presetsFileName, null);
            if (guid.Length > 0)
            {
                presetsFilePath = AssetDatabase.GUIDToAssetPath(guid[0]);
                StreamReader reader = new StreamReader(presetsFilePath);
                string line;
                List<string[]> currentPreset = null;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length > 0 && !line.StartsWith("//"))
                    {
                        if (line.Contains("="))
                        {
                            currentPreset.Add(line.Split(new string[] { " = " }, System.StringSplitOptions.None));
                        }
                        else
                        {
                            currentPreset = new List<string[]>();
                            presets.Add(line, currentPreset);
                        }
                    }
                }
                reader.Close();
                presetOptions = new string[presets.Count + 2];
                presetOptions[0] = "Presets";
                presetOptions[presets.Count + 1] = "+ New +";
                int i = 1;
                foreach (string k in presets.Keys) presetOptions[i++] = k;
            }
            else
            {
                hasPresets = false;
            }
        }
        presetsLoaded = true;
    }

    //draws presets if exists
    public void drawPresets(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        if (hasPresets)
        {
            int newSelectedPreset = EditorGUILayout.Popup(selectedPreset, presetOptions, GUILayout.MaxWidth(100));
            if (newSelectedPreset != selectedPreset)
            {
                selectedPreset = newSelectedPreset;
                if (selectedPreset != presetOptions.Length - 1) applyPreset(presetOptions[selectedPreset], materialEditor, props);
            }
            if (selectedPreset == presetOptions.Length - 1) drawNewPreset(props);
        }
    }

    string newPresetName = "Preset Name";

    public void drawNewPreset(MaterialProperty[] props)
    {
        newPresetName = GUILayout.TextField(newPresetName, GUILayout.MaxWidth(100));

        if (GUILayout.Button("Add", GUILayout.Width(40), GUILayout.Height(20)))
        {
            addNewPreset(newPresetName, props);
        }
    }

    public void addNewPreset(string name, MaterialProperty[] props)
    {
        //find all non default values

        //add to presets list
        List<string[]> sets = new List<string[]>();

        foreach (MaterialProperty p in props)
        {
            string[] set = new string[] { p.name, "" };
            bool empty = false;
            switch (p.type)
            {
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    set[1] = "" + p.floatValue;
                    break;
                case MaterialProperty.PropType.Texture:
                    if (p.textureValue == null) empty = true;
                    else set[1] = "" + p.textureValue.name;
                    break;
                case MaterialProperty.PropType.Color:
                    if (p.colorValue == null) empty = true;
                    else set[1] = "" + p.colorValue.r + "," + p.colorValue.g + "," + p.colorValue.b;
                    break;
            }
            if (p.flags != MaterialProperty.PropFlags.HideInInspector && !empty) sets.Add(set);
        }

        //fix all preset variables
        presets.Add(name, sets);
        string[] newPresetOptions = new string[presetOptions.Length + 1];
        for (int i = 0; i < presetOptions.Length; i++) newPresetOptions[i] = presetOptions[i];
        newPresetOptions[newPresetOptions.Length - 1] = presetOptions[newPresetOptions.Length - 2];
        newPresetOptions[newPresetOptions.Length - 2] = name;
        presetOptions = newPresetOptions;
        newPresetName = "Preset Name";

        //save all presets into file
        StreamWriter writer = new StreamWriter(presetsFilePath, false);
        foreach (KeyValuePair<string, List<string[]>> preset in presets)
        {
            writer.WriteLine(preset.Key);
            foreach (string[] set in preset.Value) writer.WriteLine(set[0] + " = " + set[1]);
            writer.WriteLine("");
        }
        writer.Close();
    }

    public void applyPreset(string presetName, MaterialEditor materialEditor, MaterialProperty[] props)
    {
        List<string[]> sets;
        if (presets.TryGetValue(presetName, out sets))
        {
            foreach (string[] set in sets)
            {
                MaterialProperty p = FindProperty(props, set[0]);
                if (p != null)
                {
                    if (p.type == MaterialProperty.PropType.Texture)
                    {
                        string[] guids = AssetDatabase.FindAssets(set[1] + " t:Texture", null);
                        if (guids.Length == 0) Debug.LogError("Couldn't find texture: " + set[1]);
                        else
                        {
                            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                            Texture tex = (Texture)EditorGUIUtility.Load(path);
#pragma warning disable CS0618 // Type or member is obsolete
                            materialEditor.SetTexture(set[0], tex);
                        }
                    }
                    else if (p.type == MaterialProperty.PropType.Float || p.type == MaterialProperty.PropType.Range)
                    {
                        float value;
                        if (float.TryParse(set[1], out value)) materialEditor.SetFloat(set[0], value);
                    }
                    else if (p.type == MaterialProperty.PropType.Color)
                    {
                        float[] rgb = new float[3];
                        string[] rgbString = set[1].Split(',');
                        float.TryParse(rgbString[0], out rgb[0]);
                        float.TryParse(rgbString[1], out rgb[1]);
                        float.TryParse(rgbString[2], out rgb[2]);
                        materialEditor.SetColor(set[0], new Color(rgb[0], rgb[1], rgb[2]));
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                }
            }
        }
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        Material material = materialEditor.target as Material;

        if (config == null) LoadConfig();
        if (presetsLoaded == false) loadPresets(props);

        SetupStyle();

        CollectAllProperties(props, materialEditor);


        // load default toggle values
        LoadDefaults(material);

        EditorGUILayout.BeginHorizontal();

        string shaderName = FindProperty(props, "shader_name").displayName;
        if (shaderName != null) DrawMasterLabel(shaderName);

        drawPresets(materialEditor, props);
        EditorGUILayout.EndHorizontal();

        foreach (ShaderPart part in shaderparts.parts)
        {
            drawShaderPart(part, materialEditor);
        }

        ToggleDefines(material);

        config.bigTextures = EditorGUILayout.Toggle("Big Texture Fields", config.bigTextures);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        PoiToon.linkButton(70, 20, "Github", "https://github.com/poiyomi/PoiyomiToonShader");
        GUILayout.Space(2);
        PoiToon.linkButton(70, 20, "Discord", "https://discord.gg/Ays52PY");
        GUILayout.Space(2);
        PoiToon.linkButton(70, 20, "Donate", "https://www.paypal.me/poiyomi");
        GUILayout.Space(2);
        PoiToon.linkButton(70, 20, "Patreon", "https://www.patreon.com/poiyomi");
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }
}
