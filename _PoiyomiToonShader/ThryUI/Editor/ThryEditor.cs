using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Thry;

public class ThryEditor : ShaderGUI
{
    public const string EXTRA_OPTION_PREFIX = "--";
    public const string EXTRA_OPTION_INFIX = "=";
    public const string EXTRA_OPTION_EXTRA_OFFSET = "extraOffset"; //can be used to specify and extra x-offset for properties
    public const string EXTRA_OPTION_HOVER_TEXT = "hover";
    public const string EXTRA_OPTION_ALT_CLICK = "altClick";
    public const string EXTRA_OPTION_BUTTON_RIGHT = "button_right";
    public const float MATERIAL_NOT_RESET = 69.12f;

    private ShaderHeader shaderparts; //stores headers and properties in correct order
    private Texture2D settingsTexture;

    private string masterLabelText = null;

    private List<ButtonData> footer; //footers

	private PresetHandler presetHandler; //handles the presets

    private int customQueueFieldInput = -1;

    private static bool reloadNextDraw = false;
    private bool firstOnGUICall = true;

    public static bool HadMouseDownRepaint = false;
    public static bool HadMouseDown = false;
    public static bool MouseClick = false;

    public static Vector2 lastDragPosition;

    private bool wasUsed = false;

    private EditorData current;
    public static EditorData currentlyDrawing;

    public abstract class ShaderPart
    {
        public int xOffset = 0;
        public GUIContent content;
        public System.Object property_data = null;
        public PropertyOptions options;

        public ShaderPart(int xOffset, string displayName, PropertyOptions options)
        {
            this.xOffset = xOffset;
            this.options = options;
            this.content = new GUIContent(displayName, options.hover);
        }

        public abstract void Draw();
    }

    public class ShaderHeader : ShaderPart
    {
        public ThryEditorHeader guiElement;
        public List<ShaderPart> parts = new List<ShaderPart>();

        public ShaderHeader() : base(0, "", new PropertyOptions())
        {

        }

        public ShaderHeader(MaterialProperty prop, MaterialEditor materialEditor, string displayName, int xOffset, PropertyOptions options) : base(xOffset, displayName, options)
        {
            this.guiElement = new ThryEditorHeader(materialEditor, prop.name);
        }

        public void addPart(ShaderPart part)
        {
            parts.Add(part);
        }

        public override void Draw()
        {
            currentlyDrawing.currentProperty = this;
            guiElement.Foldout(xOffset, content, currentlyDrawing.gui);
            testAltClick(DrawingData.lastGuiObjectRect, this);
            if (guiElement.getState())
            {
                EditorGUILayout.Space();
                foreach (ShaderPart part in parts)
                {
                    part.Draw();
                }
                EditorGUILayout.Space();
            }
        }
    }

    public class ShaderProperty : ShaderPart
    {
        public MaterialProperty materialProperty;
        public bool drawDefault;

        public float setFloat;
        public bool updateFloat;

        public bool forceOneLine = false;

        public ShaderProperty(MaterialProperty materialProperty, string displayName, int xOffset, PropertyOptions options, bool forceOneLine) : base(xOffset, displayName, options)
        {
            this.materialProperty = materialProperty;
            drawDefault = false;
            this.forceOneLine = forceOneLine;
        }

        public override void Draw()
        {
            PreDraw();
            if (options.condition_show != null)
                if (!options.condition_show.Test())
                    return;
            currentlyDrawing.currentProperty = this;
            DrawingData.lastGuiObjectRect = new Rect(-1,-1,-1,-1);
            int oldIndentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = xOffset * 2 + 1;
            if (drawDefault)
                DrawDefault();
            else if (forceOneLine)
                currentlyDrawing.editor.ShaderProperty(GUILayoutUtility.GetRect(content, Styles.Get().vectorPropertyStyle), this.materialProperty, this.content);
            else
                currentlyDrawing.editor.ShaderProperty(this.materialProperty, this.content);
            EditorGUI.indentLevel = oldIndentLevel;
            if (DrawingData.lastGuiObjectRect.x==-1) DrawingData.lastGuiObjectRect = GUILayoutUtility.GetLastRect();

            testAltClick(DrawingData.lastGuiObjectRect, this);
        }

        public virtual void PreDraw() { }

        public virtual void DrawDefault() { }
    }

    public class TextureProperty : ShaderProperty
    {
        public bool showScaleOffset = false;
        public bool hasScaleOffset = false;

        public TextureProperty(MaterialProperty materialProperty, string displayName, int xOffset, PropertyOptions options, bool hasScaleOffset, bool forceThryUI) : base(materialProperty, displayName, xOffset, options, false)
        {
            drawDefault = forceThryUI;
            this.hasScaleOffset = hasScaleOffset;
        }

        public override void PreDraw()
        {
            DrawingData.currentTexProperty = this;
        }

        public override void DrawDefault()
        {
            Rect pos = GUILayoutUtility.GetRect(content, Styles.Get().vectorPropertyStyle);
            GuiHelper.drawConfigTextureProperty(pos, materialProperty, content, currentlyDrawing.editor, hasScaleOffset);
            DrawingData.lastGuiObjectRect = pos;
        }
    }

    //-------------Init functions--------------------

    private Dictionary<string, string> LoadDisplayNamesFromFile()
    {
        //load display names from file if it exists
        MaterialProperty label_file_property = null;
        foreach (MaterialProperty m in current.properties) if (m.name == "shader_properties_label_file") label_file_property = m;
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
            string[] data = Regex.Split(Thry.Helper.ReadFileIntoString(path), @"\r?\n");
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
            string[] parts = displayName.Split(new string[] { EXTRA_OPTION_PREFIX }, 2, System.StringSplitOptions.None);
            displayName = parts[0];
            PropertyOptions options = Parser.ParseToObject<PropertyOptions>(parts[1]);
            if(options!=null)
                return options;
        }
        return new PropertyOptions();
    }

    private enum ThryPropertyType
    {
        none,property, footer,header,header_end,header_start,instancing,dsgi,lightmap_flags,space
    }

    private ThryPropertyType GetPropertyType(MaterialProperty p)
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
        if (Regex.Match(name.ToLower(), @"^space\d*$").Success)
            return ThryPropertyType.space;
        if (name.Replace(" ","") == "Instancing" && flags == MaterialProperty.PropFlags.HideInInspector)
            return ThryPropertyType.instancing;
        if (name.Replace(" ", "") == "DSGI" && flags == MaterialProperty.PropFlags.HideInInspector)
            return ThryPropertyType.dsgi;
        if (name.Replace(" ", "") == "LightmapFlags" && flags == MaterialProperty.PropFlags.HideInInspector)
            return ThryPropertyType.lightmap_flags;
        if (flags != MaterialProperty.PropFlags.HideInInspector)
            return ThryPropertyType.property;
        return ThryPropertyType.none;
    }

    //finds all properties and headers and stores them in correct order
    private void CollectAllProperties()
	{
        //load display names from file if it exists
        MaterialProperty[] props = current.properties;
        Dictionary<string, string> labels = LoadDisplayNamesFromFile();

        current.propertyDictionary = new Dictionary<string, ShaderProperty>();
        shaderparts = new ShaderHeader(); //init top object that all Shader Objects are childs of
		Stack<ShaderHeader> headerStack = new Stack<ShaderHeader>(); //header stack. used to keep track if current header to parent new objects to
		headerStack.Push(shaderparts); //add top object as top object to stack
		headerStack.Push(shaderparts); //add top object a second time, because it get's popped with first actual header item
		footer = new List<ButtonData>(); //init footer list
		int headerCount = 0;
		for (int i = 0; i < props.Length; i++)
		{
            string displayName = props[i].displayName;
            displayName = Regex.Replace(displayName, @"''", "\"");
            if (labels.ContainsKey(props[i].name)) displayName = labels[props[i].name];
            PropertyOptions options = ExtractExtraOptionsFromDisplayName(ref displayName);

            int offset = options.offset + headerCount;

            ThryPropertyType type = GetPropertyType(props[i]);
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
            }
            switch (type)
            {
                case ThryPropertyType.footer:
                    footer.Add(Parser.ParseToObject<ButtonData>(displayName));
                    break;
                case ThryPropertyType.header:
                case ThryPropertyType.header_start:
                    ShaderHeader newHeader = new ShaderHeader(props[i], current.editor, displayName, offset, options);
                    headerStack.Peek().addPart(newHeader);
                    headerStack.Push(newHeader);
                    break;
                case ThryPropertyType.property:
                    ShaderProperty newPorperty = null;

                    DrawingData.lastPropertyUsedCustomDrawer = false;
                    current.editor.GetPropertyHeight(props[i]);

                    bool forceOneLine = props[i].type == MaterialProperty.PropType.Vector && !DrawingData.lastPropertyUsedCustomDrawer;
                    if (props[i].type == MaterialProperty.PropType.Texture)
                        newPorperty = new TextureProperty(props[i], displayName, offset, options, props[i].flags != MaterialProperty.PropFlags.NoScaleOffset ,!DrawingData.lastPropertyUsedCustomDrawer);
                    else
                        newPorperty = new ShaderProperty(props[i], displayName, offset, options, forceOneLine);
                    headerStack.Peek().addPart(newPorperty);
                    current.propertyDictionary.Add(props[i].name, newPorperty);
                    break;
                case ThryPropertyType.lightmap_flags:
                    current.draw_material_option_lightmap = true;
                    break;
                case ThryPropertyType.dsgi:
                    current.draw_material_option_dsgi = true;
                    break;
                case ThryPropertyType.instancing:
                    current.draw_material_option_instancing = true;
                    break;
            }
		}
	}

    //-------------Functions------------------

    public void UpdateRenderQueueInstance()
    {
        if (current.materials != null) foreach (Material m in current.materials)
            if (m.shader.renderQueue != m.renderQueue)
                Thry.Helper.UpdateRenderQueue(m, current.defaultShader);
    }

    //-------------Draw Functions----------------

    private static void testAltClick(Rect rect, ShaderPart property)
    {
        var e = Event.current;
        if (HadMouseDownRepaint && e.alt && rect.Contains(e.mousePosition))
        {
            if (property.options.altClick != null)
                property.options.altClick.Perform();
        }
    }

    public void OnOpen()
    {
        Config config = Config.Get();

        //get material targets
        Object[] targets = current.editor.targets;
        current.materials = new Material[targets.Length];
        for (int i = 0; i < targets.Length; i++) current.materials[i] = targets[i] as Material;

        //collect shader properties
        CollectAllProperties();

        //init settings texture
        byte[] fileData = File.ReadAllBytes(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("thrySettigsIcon")[0]));
        settingsTexture = new Texture2D(2, 2);
        settingsTexture.LoadImage(fileData);

        //init master label
        MaterialProperty shader_master_label = FindProperty(current.properties, "shader_master_label");
        if (shader_master_label != null) masterLabelText = shader_master_label.displayName;

        current.shader = current.materials[0].shader;
        string defaultShaderName = current.materials[0].shader.name.Split(new string[] { "-queue" }, System.StringSplitOptions.None)[0].Replace(".differentQueues/", "");
        current.defaultShader = Shader.Find(defaultShaderName);

        //update render queue if render queue selection is deactivated
        if (!config.renderQueueShaders && !config.showRenderQueue)
        {
            current.materials[0].renderQueue = current.defaultShader.renderQueue;
            UpdateRenderQueueInstance();
        }

        foreach (MaterialProperty p in current.properties) if (p.name == "shader_is_using_thry_editor") p.floatValue = MATERIAL_NOT_RESET;

        if (current.materials != null) foreach (Material m in current.materials) ShaderImportFixer.backupSingleMaterial(m);
        firstOnGUICall = false;
    }

    public override void OnClosed(Material  material)
    {
        base.OnClosed(material);
        if (current.materials != null) foreach (Material m in current.materials) ShaderImportFixer.backupSingleMaterial(m);
        firstOnGUICall = true;
    }

    public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
    {
        base.AssignNewShaderToMaterial(material, oldShader, newShader);
        firstOnGUICall = true;
    }

    //-------------Main Function--------------
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
	{
        if (firstOnGUICall || reloadNextDraw)
        {
            current = new EditorData();
            current.editor = materialEditor;
            current.gui = this;
            current.properties = props;
            current.textureArrayProperties = new List<ShaderProperty>();
            current.firstCall = true;
            current.draw_material_option_dsgi = false;
            current.draw_material_option_instancing = false;
            current.draw_material_option_lightmap = false;
        }

        //handle events
        Event e = Event.current;
        MouseClick = e.type == EventType.MouseDown;
        if (MouseClick) HadMouseDown = true;
        if (HadMouseDown && e.type == EventType.Repaint) HadMouseDownRepaint = true;

        //first time call inits
        if (firstOnGUICall || reloadNextDraw) OnOpen();

        currentlyDrawing = current;

        //sync shader and get preset handler
        Config config = Config.Get();
        Settings.setActiveShader(current.materials[0].shader);
        presetHandler = Settings.presetHandler;


        //editor settings button + shader name + presets
        EditorGUILayout.BeginHorizontal();
        //draw editor settings button
        if (GUILayout.Button(settingsTexture, new GUILayoutOption[] { GUILayout.MaxWidth(24), GUILayout.MaxHeight(18) })) {
            Settings window = Settings.getInstance();
            window.Show();
            window.Focus();
        }
        //draw master label if exists
		if (masterLabelText != null) GuiHelper.DrawMasterLabel(masterLabelText);
        //draw presets if exists

        presetHandler.drawPresets(current.properties, current.materials);
		EditorGUILayout.EndHorizontal();

		//shader properties
		foreach (ShaderPart part in shaderparts.parts)
		{
            part.Draw();
		}

        //Mateiral Options
        if (current.draw_material_option_lightmap)
            GuiHelper.DrawLightmapFlagsOptions();
        if (current.draw_material_option_instancing)
            GuiHelper.DrawInstancingOptions();
        if (current.draw_material_option_dsgi)
            GuiHelper.DrawDSGIOptions();

        //Render Queue selection
        if (config.showRenderQueue)
        {
            if (config.renderQueueShaders)
            {
                customQueueFieldInput = GuiHelper.drawRenderQueueSelector(current.defaultShader, customQueueFieldInput);
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

        bool isUndo = (e.type == EventType.ExecuteCommand || e.type == EventType.ValidateCommand) && e.commandName == "UndoRedoPerformed";
        if (reloadNextDraw) reloadNextDraw = false;
        if (isUndo) reloadNextDraw = true;

        //save last drag position, because mouse postion is wrong on drag dropped event
        if (Event.current.type == EventType.DragUpdated)
            lastDragPosition= Event.current.mousePosition;

        //test if material has been reset
        if (wasUsed && e.type == EventType.Repaint)
        {
            foreach (MaterialProperty p in props)
                if (p.name == "shader_is_using_thry_editor" && p.floatValue != MATERIAL_NOT_RESET)
                {
                    reloadNextDraw = true;
                    TextTextureDrawer.ResetMaterials();
                    break;
                }
            wasUsed = false;
        }

        if (e.type == EventType.Used) wasUsed = true;
        if (config.showRenderQueue && config.renderQueueShaders) UpdateRenderQueueInstance();
        if (HadMouseDownRepaint) HadMouseDown = false;
        HadMouseDownRepaint = false;
        current.firstCall = false;
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

    public static string GetThryEditorDirectoryPath()
    {
        string[] guids = AssetDatabase.FindAssets("ThryEditor");
        foreach (string g in guids)
        {
            string p = AssetDatabase.GUIDToAssetPath(g);
            if (p.EndsWith("/ThryEditor.cs"))
                return p.GetDirectoryPath().RemoveOneDirectory();
        }
        return null;
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
