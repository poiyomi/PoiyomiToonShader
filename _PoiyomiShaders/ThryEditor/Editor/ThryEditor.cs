// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Thry;
using System;
using System.Reflection;

public class ThryEditor : ShaderGUI
{
    public const string EXTRA_OPTIONS_PREFIX = "--";
    public const float MATERIAL_NOT_RESET = 69.12f;

    public const string PROPERTY_NAME_MASTER_LABEL = "shader_master_label";
    public const string PROPERTY_NAME_PRESETS_FILE = "shader_presets";
    public const string PROPERTY_NAME_LABEL_FILE = "shader_properties_label_file";
    public const string PROPERTY_NAME_LOCALE = "shader_properties_locale";
    public const string PROPERTY_NAME_ON_SWAP_TO_ACTIONS = "shader_on_swap_to";

    // Stores the different shader properties
    private ShaderHeader shaderparts;

    // UI Instance Variables
	private PresetHandler presetHandler;
    private int customRenderQueueFieldInput = -1;

    // shader specified values
    private string masterLabelText = null;
    private List<ButtonData> footer;

    // sates
    private static bool reloadNextDraw = false;
    private bool firstOnGUICall = true;
    private bool wasUsed = false;

    public static InputEvent input = new InputEvent();
    // Contains Editor Data
    private EditorData current;
    public static EditorData currentlyDrawing;

    private DefineableAction[] on_swap_to_actions = null;
    private bool swapped_to_shader = false;

    //-------------Init functions--------------------

    private Dictionary<string, string> LoadDisplayNamesFromFile()
    {
        //load display names from file if it exists
        MaterialProperty label_file_property = null;
        foreach (MaterialProperty m in current.properties) if (m.name == PROPERTY_NAME_LABEL_FILE) label_file_property = m;
        Dictionary<string, string> labels = new Dictionary<string, string>();
        if (label_file_property != null)
        {
            string[] guids = AssetDatabase.FindAssets(label_file_property.displayName);
            if (guids.Length == 0)
            {
                Debug.LogWarning("Label File could not be found");
                return labels;
            }
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            string[] data = Regex.Split(Thry.FileHelper.ReadFileIntoString(path), @"\r?\n");
            foreach (string d in data)
            {
                string[] set = Regex.Split(d, ":=");
                if (set.Length > 1) labels[set[0]] = set[1];
            }
        }
        return labels;
    }

    private PropertyOptions ExtractExtraOptionsFromDisplayName(ref string displayName)
    {
        if (displayName.Contains("--"))
        {
            string[] parts = displayName.Split(new string[] { EXTRA_OPTIONS_PREFIX }, 2, System.StringSplitOptions.None);
            displayName = parts[0];
            PropertyOptions options = Parser.ParseToObject<PropertyOptions>(parts[1]);
            if (options!=null)
                return options;
        }
        return new PropertyOptions();
    }

    private enum ThryPropertyType
    {
        none,property,master_label,footer,header,header_end,header_start,group_start,group_end,instancing,dsgi,lightmap_flags,locale,on_swap_to,space
    }

    private ThryPropertyType GetPropertyType(MaterialProperty p, PropertyOptions options)
    {
        string name = p.name;
        MaterialProperty.PropFlags flags = p.flags;
        if (name.StartsWith("footer_") && flags == MaterialProperty.PropFlags.HideInInspector)
            return ThryPropertyType.footer;
        if (name.StartsWith("m_end") && flags == MaterialProperty.PropFlags.HideInInspector)
            return ThryPropertyType.header_end;
        if (name.StartsWith("m_start") && flags == MaterialProperty.PropFlags.HideInInspector)
            return ThryPropertyType.header_start;
        if (name.StartsWith("m_") && flags == MaterialProperty.PropFlags.HideInInspector)
            return ThryPropertyType.header;
        if (name.StartsWith("g_start") && flags == MaterialProperty.PropFlags.HideInInspector)
            return ThryPropertyType.group_start;
        if (name.StartsWith("g_end") && flags == MaterialProperty.PropFlags.HideInInspector)
            return ThryPropertyType.group_end;
        if (Regex.Match(name.ToLower(), @"^space\d*$").Success)
            return ThryPropertyType.space;
        if (name == PROPERTY_NAME_MASTER_LABEL)
            return ThryPropertyType.master_label;
        if (name == PROPERTY_NAME_ON_SWAP_TO_ACTIONS)
            return ThryPropertyType.on_swap_to;
        if (name.Replace(" ","") == "Instancing" && flags == MaterialProperty.PropFlags.HideInInspector)
            return ThryPropertyType.instancing;
        if (name.Replace(" ", "") == "DSGI" && flags == MaterialProperty.PropFlags.HideInInspector)
            return ThryPropertyType.dsgi;
        if (name.Replace(" ", "") == "LightmapFlags" && flags == MaterialProperty.PropFlags.HideInInspector)
            return ThryPropertyType.lightmap_flags;
        if (name.Replace(" ", "") == PROPERTY_NAME_LOCALE)
            return ThryPropertyType.locale;
        if (flags != MaterialProperty.PropFlags.HideInInspector && !options.hide_in_inspector)
            return ThryPropertyType.property;
        return ThryPropertyType.none;
    }

    public Locale locale;

    private void LoadLocales()
    {
        MaterialProperty locales_property = null;
        locale = null;
        foreach (MaterialProperty m in current.properties) if (m.name == PROPERTY_NAME_LOCALE) locales_property = m;
        if (locales_property != null)
        {
            string displayName = locales_property.displayName;
            PropertyOptions options = ExtractExtraOptionsFromDisplayName(ref displayName);
            locale = new Locale(options.file_name);
            locale.selected_locale_index = (int)locales_property.floatValue;
        }
    }

    //finds all properties and headers and stores them in correct order
    private void CollectAllProperties()
	{
        //load display names from file if it exists
        MaterialProperty[] props = current.properties;
        Dictionary<string, string> labels = LoadDisplayNamesFromFile();
        LoadLocales();

        current.propertyDictionary = new Dictionary<string, ShaderProperty>();
        shaderparts = new ShaderHeader(); //init top object that all Shader Objects are childs of
		Stack<ShaderGroup> headerStack = new Stack<ShaderGroup>(); //header stack. used to keep track if current header to parent new objects to
		headerStack.Push(shaderparts); //add top object as top object to stack
		headerStack.Push(shaderparts); //add top object a second time, because it get's popped with first actual header item
		footer = new List<ButtonData>(); //init footer list
		int headerCount = 0;

        Type materialPropertyDrawerType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.MaterialPropertyHandler");
        MethodInfo getPropertyHandlerMethod = materialPropertyDrawerType.GetMethod("GetShaderPropertyHandler", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        PropertyInfo drawerProperty = materialPropertyDrawerType.GetProperty("propertyDrawer");

        Type materialToggleDrawerType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.MaterialToggleDrawer");
        FieldInfo keyWordField = materialToggleDrawerType.GetField("keyword", BindingFlags.Instance | BindingFlags.NonPublic);

        for (int i = 0; i < props.Length; i++)
		{
            string displayName = props[i].displayName;
            if (locale != null)
                foreach (string key in locale.GetAllKeys())
                    if(displayName.Contains("locale::" + key))
                        displayName = displayName.Replace("locale::" + key, locale.Get(key));
            displayName = Regex.Replace(displayName, @"''", "\"");

            if (labels.ContainsKey(props[i].name)) displayName = labels[props[i].name];
            PropertyOptions options = ExtractExtraOptionsFromDisplayName(ref displayName);

            int offset = options.offset + headerCount;

            //Handle keywords
            object propertyHandler = getPropertyHandlerMethod.Invoke(null, new object[] { current.shader, props[i].name });
            //if has custom drawer
            if (propertyHandler != null)
            {
                object propertyDrawer = drawerProperty.GetValue(propertyHandler, null);
                //if custom drawer exists
                if (propertyDrawer != null)
                {
                    if (propertyDrawer.GetType().ToString() == "UnityEditor.MaterialToggleDrawer")
                    {
                        object keyword = keyWordField.GetValue(propertyDrawer);
                        if (keyword != null)
                        {
                            foreach(Material m in current.materials)
                            {
                                if (m.GetFloat(props[i].name) == 1)
                                    m.EnableKeyword((string)keyword);
                                else
                                    m.DisableKeyword((string)keyword);
                            }
                        }
                    }
                }
            }


            ThryPropertyType type = GetPropertyType(props[i],options);
            switch (type)
            {
                case ThryPropertyType.header:
                    headerStack.Pop();
                    break;
                case ThryPropertyType.header_start:
                    offset = options.offset + ++headerCount;
                    break;
                case ThryPropertyType.header_end:
                    headerStack.Pop();
                    headerCount--;
                    break;
                case ThryPropertyType.on_swap_to:
                    on_swap_to_actions = options.actions;
                    break;
            }
            ShaderProperty newPorperty = null;
            switch (type)
            {
                case ThryPropertyType.master_label:
                    masterLabelText = displayName;
                    break;
                case ThryPropertyType.footer:
                    footer.Add(Parser.ParseToObject<ButtonData>(displayName));
                    break;
                case ThryPropertyType.header:
                case ThryPropertyType.header_start:
                    ShaderHeader newHeader = new ShaderHeader(props[i], current.editor, displayName, offset, options);
                    headerStack.Peek().addPart(newHeader);
                    headerStack.Push(newHeader);
                    break;
                case ThryPropertyType.group_start:
                    ShaderGroup new_group = new ShaderGroup(options);
                    headerStack.Peek().addPart(new_group);
                    headerStack.Push(new_group);
                    break;
                case ThryPropertyType.group_end:
                    headerStack.Pop();
                    break;
                case ThryPropertyType.none:
                case ThryPropertyType.property:
                    DrawingData.lastPropertyUsedCustomDrawer = false;
                    current.editor.GetPropertyHeight(props[i]);
                    bool forceOneLine = props[i].type == MaterialProperty.PropType.Vector && !DrawingData.lastPropertyUsedCustomDrawer;
                    if (props[i].type == MaterialProperty.PropType.Texture)
                        newPorperty = new TextureProperty(props[i], displayName, offset, options, props[i].flags != MaterialProperty.PropFlags.NoScaleOffset ,!DrawingData.lastPropertyUsedCustomDrawer);
                    else
                        newPorperty = new ShaderProperty(props[i], displayName, offset, options, forceOneLine);
                    break;
                case ThryPropertyType.lightmap_flags:
                    newPorperty = new GIProperty(props[i], displayName, offset, options, false);
                    break;
                case ThryPropertyType.dsgi:
                    newPorperty = new DSGIProperty(props[i], displayName, offset, options, false);
                    break;
                case ThryPropertyType.instancing:
                    newPorperty = new InstancingProperty(props[i], displayName, offset, options, false);
                    break;
                case ThryPropertyType.locale:
                    newPorperty = new LocaleProperty(props[i], displayName, offset, options, false);
                    break;
            }
            if (newPorperty != null)
            {
                if (current.propertyDictionary.ContainsKey(props[i].name))
                    continue;
                current.propertyDictionary.Add(props[i].name, newPorperty);
                if (type != ThryPropertyType.none)
                    headerStack.Peek().addPart(newPorperty);
            }
        }
	}
    
    private MaterialProperty FindProperty(string name)
    {
        return System.Array.Find(current.properties,
                       element => element.name == name);
    }

    //-------------Functions------------------

    public void UpdateRenderQueueInstance()
    {
        if (current.materials != null) foreach (Material m in current.materials)
            if (m.shader.renderQueue != m.renderQueue)
                Thry.MaterialHelper.UpdateRenderQueue(m, current.defaultShader);
    }

    //-------------Draw Functions----------------

    public void OnOpen()
    {
        Config config = Config.Get();

        //get material targets
        UnityEngine.Object[] targets = current.editor.targets;
        current.materials = new Material[targets.Length];
        for (int i = 0; i < targets.Length; i++) current.materials[i] = targets[i] as Material;

        presetHandler = new PresetHandler(current.properties);

        current.shader = current.materials[0].shader;
        string defaultShaderName = current.materials[0].shader.name.Split(new string[] { "-queue" }, System.StringSplitOptions.None)[0].Replace(".differentQueues/", "");
        current.defaultShader = Shader.Find(defaultShaderName);

        //collect shader properties
        CollectAllProperties();

        //update render queue if render queue selection is deactivated
        if (!config.renderQueueShaders && !config.showRenderQueue)
        {
            current.materials[0].renderQueue = current.defaultShader.renderQueue;
            UpdateRenderQueueInstance();
        }

        current.materials[0].SetInt("thry_has_not_been_reset", 69);

        firstOnGUICall = false;
    }

    public override void OnClosed(Material  material)
    {
        base.OnClosed(material);
        firstOnGUICall = true;
    }

    public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
    {
        base.AssignNewShaderToMaterial(material, oldShader, newShader);
        firstOnGUICall = true;
        swapped_to_shader = true;
    }

    private void UpdateEvents()
    {
        Event e = Event.current;
        input.MouseClick = e.type == EventType.MouseDown;
        if (input.MouseClick) input.HadMouseDown = true;
        if (input.HadMouseDown && e.type == EventType.Repaint) input.HadMouseDownRepaint = true;
        input.is_alt_down = e.alt;
        input.mouse_position = e.mousePosition;
        input.is_drop_event = e.type == EventType.DragPerform;
        input.is_drag_drop_event = input.is_drop_event || e.type == EventType.DragUpdated;
    }

    //-------------Main Function--------------
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
	{
        if (firstOnGUICall || (reloadNextDraw && Event.current.type == EventType.Layout))
        {
            current = new EditorData();
            current.editor = materialEditor;
            current.gui = this;
            current.properties = props;
            current.textureArrayProperties = new List<ShaderProperty>();
            current.firstCall = true;
        }

        //handle events
        UpdateEvents();

        //first time call inits
        if (firstOnGUICall || (reloadNextDraw && Event.current.type == EventType.Layout)) OnOpen();
        current.shader = current.materials[0].shader;

        currentlyDrawing = current;

        //sync shader and get preset handler
        Config config = Config.Get();
        if(current.materials!=null)
            Mediator.SetActiveShader(current.materials[0].shader, presetHandler: presetHandler);


        //editor settings button + shader name + presets
        EditorGUILayout.BeginHorizontal();
        //draw editor settings button
        if (GUILayout.Button(new GUIContent(" Thry Editor",Styles.settings_icon), EditorStyles.largeLabel ,new GUILayoutOption[] { GUILayout.MaxHeight(20) })) {
            Settings window = Settings.getInstance();
            window.Show();
            window.Focus();
        }
        EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);

        //draw master label if exists
        if (masterLabelText != null) GuiHelper.DrawMasterLabel(masterLabelText, GUILayoutUtility.GetLastRect().y);
        //draw presets if exists

        presetHandler.drawPresets(current.properties, current.materials);
		EditorGUILayout.EndHorizontal();

		//shader properties
		foreach (ShaderPart part in shaderparts.parts)
		{
            part.Draw();
		}

        //Render Queue selection
        if (config.showRenderQueue)
        {
            if (config.renderQueueShaders)
            {
                customRenderQueueFieldInput = GuiHelper.drawRenderQueueSelector(current.defaultShader, customRenderQueueFieldInput);
                EditorGUILayout.LabelField("Default: " + current.defaultShader.name);
                EditorGUILayout.LabelField("Shader: " + current.shader.name);
            }
            else
            {
                materialEditor.RenderQueueField();
            }
        }

        //footer
        GuiHelper.drawFooters(footer);

        Event e = Event.current;
        bool isUndo = (e.type == EventType.ExecuteCommand || e.type == EventType.ValidateCommand) && e.commandName == "UndoRedoPerformed";
        if (reloadNextDraw && Event.current.type==EventType.Layout) reloadNextDraw = false;
        if (isUndo) reloadNextDraw = true;

        //on swap
        if (on_swap_to_actions != null && swapped_to_shader)
        {
            foreach (DefineableAction a in on_swap_to_actions)
                a.Perform();
            on_swap_to_actions = null;
            swapped_to_shader = false;
        }

        //test if material has been reset
        if (wasUsed && e.type == EventType.Repaint)
        {
            if (!current.materials[0].HasProperty("thry_has_not_been_reset"))
            {
                reloadNextDraw = true;
                HandleReset();
                wasUsed = true;
            }
        }

        if (e.type == EventType.Used) wasUsed = true;
        if (config.showRenderQueue && config.renderQueueShaders) UpdateRenderQueueInstance();
        if (input.HadMouseDownRepaint) input.HadMouseDown = false;
        input.HadMouseDownRepaint = false;
        current.firstCall = false;
    }

    private void HandleReset()
    {
        MaterialLinker.UnlinkAll(current.materials[0]);
    }

    public static void reload()
    {
        reloadNextDraw = true;
    }

    public static void loadValuesFromMaterial()
    {
        if (currentlyDrawing.editor != null)
        {
            try
            {
                Material m = ((Material)currentlyDrawing.editor.target);
                foreach (MaterialProperty property in currentlyDrawing.properties)
                {
                    switch (property.type)
                    {
                        case MaterialProperty.PropType.Float:
                        case MaterialProperty.PropType.Range:
                            property.floatValue = m.GetFloat(property.name);
                            break;
                        case MaterialProperty.PropType.Texture:
                            property.textureValue = m.GetTexture(property.name);
                            break;
                        case MaterialProperty.PropType.Color:
                            property.colorValue = m.GetColor(property.name);
                            break;
                        case MaterialProperty.PropType.Vector:
                            property.vectorValue = m.GetVector(property.name);
                            break;
                    }

                }
            }
            catch (System.Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }

    public static void propertiesChanged()
    {
        if (currentlyDrawing.editor != null)
        {
            try
            {
                currentlyDrawing.editor.PropertiesChanged();
            }
            catch (System.Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }

    public static void addUndo(string label)
    {
        if (currentlyDrawing.editor != null)
        {
            try
            {
                currentlyDrawing.editor.RegisterPropertyChangeUndo(label);
            }
            catch (System.Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }

    public static void repaint()
    {
        if (currentlyDrawing.editor != null)
        {
            try
            {
                currentlyDrawing.editor.Repaint();
            }
            catch(System.Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }

    private static string edtior_directory_path;
    public static string GetThryEditorDirectoryPath()
    {
        if (edtior_directory_path == null)
        {
            string[] guids = AssetDatabase.FindAssets("ThryEditor");
            foreach (string g in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(g);
                if (p.EndsWith("/ThryEditor.cs"))
                {
                    edtior_directory_path = p.GetDirectoryPath().RemoveOneDirectory();
                    break;
                }
            }
        }
        return edtior_directory_path;
    }

    //----------Static Helper Functions

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
}
