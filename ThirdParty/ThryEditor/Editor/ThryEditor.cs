//for most shaders

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class ThryEditor : ShaderGUI
{
    public const string EXTRA_OPTION_PREFIX = "--";
    public const string EXTRA_OPTION_INFIX = "=";
    public const string EXTRA_OFFSET_OPTION = "extraOffset"; //can be used to specify and extra x-offset for properties
    public const string HOVER_OPTION = "hover";
    public const string ON_ALT_CLICK_OPTION = "altClick";

    private ShaderHeader shaderparts; //stores headers and properties in correct order
	private GUIStyle m_sectionStyle;
    private GUIStyle bigTextureStyle;
    private Texture2D settingsTexture;

    private string masterLabelText = null;

    private List<string> footer; //footers

	private ThryPresetHandler presetHandler; //handles the presets

	private int textureFieldsCount; //counts how many texture fields there are

	private Dictionary<string, bool> showTextureScaleOffset = new Dictionary<string, bool>(); //if texture scale/offset fields are extended or not

    private int customQueueFieldInput = -1;

    private bool isMouseClick = false;

    private bool firstOnGUICall = true;

    private Material[] materials;

    private Shader shader;
    private Shader defaultShader;

    private class ThryEditorHeader
	{
		private List<MaterialProperty> propertyes;
		private bool currentState;

		public ThryEditorHeader(MaterialEditor materialEditor, string propertyName)
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

		public void Foldout(int xOffset, string name, ThryEditor gui)
		{
			var style = new GUIStyle("ShurikenModuleTitle");
			style.font = new GUIStyle(EditorStyles.label).font;
			style.border = new RectOffset(15, 7, 4, 4);
			style.fixedHeight = 22;
			style.contentOffset = new Vector2(20f, -2f);
			style.margin.left = 30 * xOffset;

			var rect = GUILayoutUtility.GetRect(16f + 20f, 22f, style);
			GUI.Box(rect, name, style);

			var e = Event.current;

			var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
			if (e.type == EventType.Repaint)
			{
				EditorStyles.foldout.Draw(toggleRect, false, false, getState(), false);
			}

			if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
			{
				this.Toggle();
				e.Use();
			}
        }
	}

	//--------classes for storing property data---------
	private class ShaderPart
	{
		public int xOffset = 0;
        public string onHover = "";
        public string altClick = "";

        public ShaderPart(int xOffset, string onHover, string altClick)
        {
            this.xOffset = xOffset;
            this.onHover = onHover;
            this.altClick = altClick;
        }
	}

	private class ShaderHeader : ShaderPart
	{
		public ThryEditorHeader guiElement;
		public List<ShaderPart> parts = new List<ShaderPart>();
		public string name;

		public ShaderHeader() : base(0,"","")
		{
            
		}

		public ShaderHeader(MaterialProperty prop, MaterialEditor materialEditor, string displayName, int xOffset, string onHover, string altClick) : base(xOffset,onHover,altClick)
		{
			this.guiElement = new ThryEditorHeader(materialEditor, prop.name);
			this.name = displayName;
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

		public ShaderProperty(MaterialProperty materialProperty, string displayName, int xOffset, string onHover, string altClick) : base(xOffset, onHover, altClick)
        {
			this.materialProperty = materialProperty;
			this.style = new GUIContent(displayName, onHover);
		}
	}

	//-------------Init functions--------------------

	//finds all properties and headers and stores them in correct order
	private void CollectAllProperties(MaterialProperty[] props, MaterialEditor materialEditor)
	{
		shaderparts = new ShaderHeader();
		Stack<ShaderHeader> headerStack = new Stack<ShaderHeader>();
		headerStack.Push(shaderparts);
		headerStack.Push(shaderparts);
		footer = new List<string>();
		textureFieldsCount = 0;
		int headerCount = 0;
		for (int i = 0; i < props.Length; i++)
		{
            if (props[i].name.StartsWith("footer_") && props[i].flags == MaterialProperty.PropFlags.HideInInspector)
            {
                footer.Add(props[i].displayName);
            }else if (props[i].name.StartsWith("m_end") && props[i].flags == MaterialProperty.PropFlags.HideInInspector)
            {
                headerStack.Pop();
                headerCount--;
            } else
            {
                int extraOffset = 0;
                extraOffset = ThryHelper.propertyOptionToInt(EXTRA_OFFSET_OPTION, props[i]);
                int offset = extraOffset + headerCount;
                string displayName = props[i].displayName.Replace(EXTRA_OPTION_PREFIX + EXTRA_OFFSET_OPTION + EXTRA_OPTION_INFIX + extraOffset, "");
                string onHover = ThryHelper.getPropertyOptionValue(HOVER_OPTION, props[i]);
                displayName = displayName.Replace(EXTRA_OPTION_PREFIX + HOVER_OPTION + EXTRA_OPTION_INFIX + onHover, "");
                string altClick = ThryHelper.getPropertyOptionValue(ON_ALT_CLICK_OPTION, props[i]);
                displayName = displayName.Replace(EXTRA_OPTION_PREFIX + ON_ALT_CLICK_OPTION + EXTRA_OPTION_INFIX + altClick, "");
                if (props[i].name.StartsWith("m_start") && props[i].flags == MaterialProperty.PropFlags.HideInInspector)
                {
                    headerCount++;
                    ShaderHeader newHeader = new ShaderHeader(props[i], materialEditor, displayName, offset, onHover,altClick);
                    headerStack.Peek().addPart(newHeader);
                    headerStack.Push(newHeader);
                }
                else if (props[i].name.StartsWith("m_") && props[i].flags == MaterialProperty.PropFlags.HideInInspector)
                {
                    ShaderHeader newHeader = new ShaderHeader(props[i], materialEditor, displayName, offset, onHover, altClick);
                    headerStack.Pop();
                    headerStack.Peek().addPart(newHeader);
                    headerStack.Push(newHeader);
                }
                else if (props[i].flags != MaterialProperty.PropFlags.HideInInspector)
                {
                    ShaderProperty newPorperty = new ShaderProperty(props[i], displayName, offset, onHover, altClick);
                    headerStack.Peek().addPart(newPorperty);
                    if (props[i].type == MaterialProperty.PropType.Texture) textureFieldsCount++;
                }
            }
		}
	}

	//----------Idk what this does-------------
	private void SetupStyle()
	{
		m_sectionStyle = new GUIStyle(EditorStyles.boldLabel);
		m_sectionStyle.alignment = TextAnchor.MiddleCenter;

        bigTextureStyle = new GUIStyle();
        bigTextureStyle.fixedHeight = 65;
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

    //-------------Functions------------------

    public void UpdateRenderQueueInstance(Shader defaultShader)
    {
        if (materials != null) foreach (Material m in materials)
            if (m.shader.renderQueue != m.renderQueue)
                ThryHelper.UpdateRenderQueue(m, defaultShader);
    }

    //-------------Draw Functions----------------

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
		//header.header = ThryEditorUI.Foldout(header);
		header.guiElement.Foldout(header.xOffset, header.name, this);
		if (header.guiElement.getState())
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
        var e = Event.current;
        Rect rect;
        if (property.materialProperty.type == MaterialProperty.PropType.Texture)
		{
            if (ThryConfig.GetConfig().useBigTextures == false) {
                int oldIndentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = property.xOffset * 2 + 1;
                rect = materialEditor.TexturePropertySingleLine(new GUIContent(property.style.text, "Click here for scale / offset" + (property.style.tooltip != "" ? " | " : "") + property.style.tooltip), property.materialProperty);
                testAltClick(rect, property);
                bool showScaleOffset = false;
                if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
                {
                    if (showTextureScaleOffset.TryGetValue(property.materialProperty.name, out showScaleOffset))
                    {
                        showTextureScaleOffset.Remove(property.materialProperty.name);
                    }
                    showScaleOffset = !showScaleOffset;
                    showTextureScaleOffset.Add(property.materialProperty.name, showScaleOffset);

                    e.Use();
                }
                if (!showTextureScaleOffset.TryGetValue(property.materialProperty.name, out showScaleOffset))
                {
                    showScaleOffset = false;
                }
                if (showScaleOffset) materialEditor.TextureScaleOffsetProperty(property.materialProperty);

                EditorGUI.indentLevel = oldIndentLevel;
            }
            else
            {
                rect = GUILayoutUtility.GetRect(property.style, bigTextureStyle);
                materialEditor.ShaderProperty(rect,property.materialProperty, property.style, property.xOffset * 2 + 1);
                testAltClick(rect, property);
            }
		}
		else
		{
            materialEditor.ShaderProperty(property.materialProperty, property.style, property.xOffset * 2 + 1);
            rect = GUILayoutUtility.GetLastRect();
            testAltClick(rect, property);
        }
    }

    private void testAltClick(Rect rect, ShaderProperty property)
    {
        var e = Event.current;
        if (e.type == EventType.Repaint)
        {
            //if (isMouseClick && rect.Contains(e.mousePosition)) Debug.Log("clicked on " + property.style.text);
            if (e.alt && isMouseClick && rect.Contains(e.mousePosition))
            {
                if (property.altClick != "")
                {
                    if (property.altClick.StartsWith("url:")) Application.OpenURL(property.altClick.Replace("url:", ""));
                }
            }
        }
    }

    //draw the render queue selector
    private void drawRenderQueueSelector(Shader defaultShader)
    {
        EditorGUILayout.BeginHorizontal();
        if (customQueueFieldInput == -1) customQueueFieldInput = materials[0].renderQueue;
        int[] queueOptionsQueues = new int[] { defaultShader.renderQueue, 2000, 2450, 3000, customQueueFieldInput };
        string[] queueOptions = new string[] { "From Shader", "Geometry", "Alpha Test", "Transparency" };
        int queueSelection = 4;
        if (defaultShader.renderQueue == customQueueFieldInput) queueSelection = 0;
        else
        {
            string customOption = null;
            int q = customQueueFieldInput;
            if (q < 2000) customOption = queueOptions[1] + "-" + (2000 - q);
            else if (q < 2450) { if (q > 2000) customOption = queueOptions[1] + "+" + (q - 2000); else queueSelection = 1; }
            else if (q < 3000) { if (q > 2450) customOption = queueOptions[2] + "+" + (q - 2450); else queueSelection = 2; }
            else if (q < 5001) { if (q > 3000) customOption = queueOptions[3] + "+" + (q - 3000); else queueSelection = 3; }
            if (customOption != null) queueOptions = new string[] { "From Shader", "Geometry", "Alpha Test", "Transparency", customOption };
        }
        EditorGUILayout.LabelField("Render Queue", GUILayout.ExpandWidth(true));
        int newQueueSelection = EditorGUILayout.Popup(queueSelection, queueOptions, GUILayout.MaxWidth(100));
        int newQueue = queueOptionsQueues[newQueueSelection]; 
        if (queueSelection != newQueueSelection) customQueueFieldInput = newQueue;
        int newCustomQueueFieldInput = EditorGUILayout.IntField(customQueueFieldInput, GUILayout.MaxWidth(65));
        bool isInput = customQueueFieldInput!=newCustomQueueFieldInput || queueSelection != newQueueSelection;
        customQueueFieldInput = newCustomQueueFieldInput;
        foreach(Material m in materials)
            if (customQueueFieldInput != m.renderQueue && isInput) m.renderQueue = customQueueFieldInput;
        if (customQueueFieldInput != materials[0].renderQueue && !isInput) customQueueFieldInput = materials[0].renderQueue;
        EditorGUILayout.EndHorizontal();
    }

	//draw a button with a link
	public static void drawLinkButton(int Width, int Height, string title, string link)
	{
		if (GUILayout.Button(title, GUILayout.Width(Width), GUILayout.Height(Height)))
		{
			Application.OpenURL(link);
		}
	}

    //draw all collected footers
    public void drawFooters()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Space(2);
        foreach (string footNote in footer)
        {
            drawFooter(footNote);
            GUILayout.Space(2);
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    //draw single footer
    public static void drawFooter(string data)
    {
        string[] splitNote = data.TrimEnd(')').Split("(".ToCharArray(), 2);
        string value = splitNote[1];
        string type = splitNote[0];
        if (type == "linkButton")
        {
            string[] values = value.Split(",".ToCharArray());
            drawLinkButton(70, 20, values[0], values[1]);
        }
    }

    public void OnOpen(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        ThryConfig.Config config = ThryConfig.GetConfig();

        //collect shader properties
        CollectAllProperties(props, materialEditor);

        SetupStyle();

        //init settings texture
        byte[] fileData = File.ReadAllBytes(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("thrySettigsIcon")[0]));
        settingsTexture = new Texture2D(2, 2);
        settingsTexture.LoadImage(fileData);

        //init master label
        MaterialProperty shader_master_label = FindProperty(props, "shader_master_label");
        if (shader_master_label != null) masterLabelText = shader_master_label.displayName;

        //update render queue if render queue selection is deactivated
        if (!config.useRenderQueueSelection)
        {
            this.shader = materials[0].shader;
            string defaultShaderName = materials[0].shader.name.Split(new string[] { "-queue" }, System.StringSplitOptions.None)[0].Replace(".differentQueues/", "");
            this.defaultShader = Shader.Find(defaultShaderName);
            materials[0].renderQueue = defaultShader.renderQueue;
            UpdateRenderQueueInstance(defaultShader);
        }

            if (materials != null) foreach (Material m in materials) ThryShaderImportFixer.backupSingleMaterial(m);
        firstOnGUICall = false;
    }

    public override void OnClosed(Material  material)
    {
        base.OnClosed(material);
        if (materials != null) foreach (Material m in materials) ThryShaderImportFixer.backupSingleMaterial(m);
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
        //handle events
        Event e = Event.current;
        if (e.type == EventType.MouseDown) isMouseClick = true;

        //get material targets
        Object[] targets = materialEditor.targets;
        materials = new Material[targets.Length];
        for (int i = 0; i < targets.Length; i++) materials[i] = targets[i] as Material;

        //sync shader and get preset handler
        ThryConfig.Config config = ThryConfig.GetConfig();
        ThrySettings.setActiveShader(materials[0].shader);
        presetHandler = ThrySettings.presetHandler;

        //first time call inits
        if (firstOnGUICall) OnOpen(materialEditor,props);


        //editor settings button + shader name + presets
        EditorGUILayout.BeginHorizontal();
        //draw editor settings button
        if (GUILayout.Button(settingsTexture, new GUILayoutOption[] { GUILayout.MaxWidth(24), GUILayout.MaxHeight(18) })) {
            ThrySettings window = ThrySettings.getInstance();
            window.Show();
            window.Focus();
        }
        //draw master label if exists
		if (masterLabelText != null) DrawMasterLabel(masterLabelText);
        //draw presets if exists
		presetHandler.drawPresets(props, materials);
		EditorGUILayout.EndHorizontal();

		//shader properties
		foreach (ShaderPart part in shaderparts.parts)
		{
			drawShaderPart(part, materialEditor);
		}

        //Render Queue selection
        if (config.useRenderQueueSelection)
        {
            drawRenderQueueSelector(defaultShader);
            EditorGUILayout.LabelField("Default: " + defaultShader.name);
            EditorGUILayout.LabelField("Shader: " + shader.name);
        }

        //footer
        drawFooters();

        if (config.useRenderQueueSelection) UpdateRenderQueueInstance(defaultShader);
        if (e.type == EventType.Repaint) isMouseClick = false;
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
