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

	private ShaderHeader shaderparts; //stores headers and properties in correct order
	private GUIStyle m_sectionStyle;

	private List<string> footer; //footers

	private PoiPresetHandler presetHandler; //handles the presets

	private int textureFieldsCount; //counts how many texture fields there are

	private Dictionary<string, bool> showTextureScaleOffset = new Dictionary<string, bool>(); //if texture scale/offset fields are extended or not

    private int customQueueFieldInput = -1;

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

		public void Foldout(int xOffset, string name)
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
	}

	private class ShaderHeader : ShaderPart
	{
		public PoiToonHeader guiElement;
		public List<ShaderPart> parts = new List<ShaderPart>();
		public string name;

		public ShaderHeader()
		{

		}

		public ShaderHeader(MaterialProperty prop, MaterialEditor materialEditor, int xOffset)
		{
			this.guiElement = new PoiToonHeader(materialEditor, prop.name);
			this.name = prop.displayName;
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

		public ShaderProperty(MaterialProperty materialProperty, string displayName, int xOffset)
		{
			this.xOffset = xOffset;
			this.materialProperty = materialProperty;
			this.style = new GUIContent(displayName, materialProperty.name + materialProperty.type);
		}
	}

	//---------------config functions---------------

	//Config class
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
            reader.Close();
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
			else if (props[i].name.StartsWith("footer_") && props[i].flags == MaterialProperty.PropFlags.HideInInspector)
			{
				footer.Add(props[i].displayName);
			}
			else if (props[i].flags != MaterialProperty.PropFlags.HideInInspector)
			{
				int extraOffset = 0;
				extraOffset = propertyOptionToInt(EXTRA_OFFSET_OPTION, props[i]);
				string displayName = props[i].displayName.Replace("-" + EXTRA_OFFSET_OPTION + "=" + extraOffset, "");
				ShaderProperty newPorperty = new ShaderProperty(props[i], displayName, headerCount + extraOffset);
				headerStack.Peek().addPart(newPorperty);
				if (props[i].type == MaterialProperty.PropType.Texture) textureFieldsCount++;
			}

		}
	}

	//----------Idk what this does-------------
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

    //-------------Functions------------------

    private void UpdateRenderQueue(Material material, Shader defaultShader)
    {
        if (material.shader.renderQueue != material.renderQueue)
        {
            EditorGUILayout.LabelField("Not the default render queue");
            Shader renderQueueShader = defaultShader;
            if (material.renderQueue != renderQueueShader.renderQueue)
            {
                string renderQueueShaderName = ".differentQueues/" + defaultShader.name + "-queue" + material.renderQueue;
                renderQueueShader = Shader.Find(renderQueueShaderName);
                if (renderQueueShader == null) renderQueueShader = createRenderQueueShader(defaultShader, material.renderQueue);
            }
            material.shader = renderQueueShader;
        }
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
		//header.header = PoiToonUI.Foldout(header);
		header.guiElement.Foldout(header.xOffset, header.name);
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

    //draw the render queue selector
    private void drawRenderQueueSelector(Material material, Shader defaultShader)
    {
        EditorGUILayout.BeginHorizontal();
        if (customQueueFieldInput == -1) customQueueFieldInput = material.renderQueue;
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
        if (customQueueFieldInput != material.renderQueue && isInput) material.renderQueue = customQueueFieldInput;
        if (customQueueFieldInput != material.renderQueue && !isInput) customQueueFieldInput = material.renderQueue;
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
    
    //-------------Main Function--------------
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
	{
		Material material = materialEditor.target as Material;

		if (config == null) LoadConfig();
		if (presetHandler == null) presetHandler = new PoiPresetHandler(props);
        else presetHandler.testPresetsChanged(props);

        SetupStyle();

		CollectAllProperties(props, materialEditor);

		// load default toggle values
		LoadDefaults(material);

		//shader name + presets
		EditorGUILayout.BeginHorizontal();
		MaterialProperty shader_master_label = FindProperty(props, "shader_master_label");
		if (shader_master_label != null) DrawMasterLabel(shader_master_label.displayName);
		presetHandler.drawPresets(materialEditor, props, material);
		EditorGUILayout.EndHorizontal();

		//shader properties
		foreach (ShaderPart part in shaderparts.parts)
		{
			drawShaderPart(part, materialEditor);
		}

        //Render Queue
        Shader shader = material.shader;
        string defaultShaderName = material.shader.name.Split(new string[] { "-queue" }, System.StringSplitOptions.None)[0].Replace(".differentQueues/","");
        Shader defaultShader = Shader.Find(defaultShaderName);
        drawRenderQueueSelector(material, defaultShader);
        EditorGUILayout.LabelField("Default: " + defaultShaderName);
        EditorGUILayout.LabelField("Shader: " + shader.name);
        
        ToggleDefines(material);

        //big/small texture toggle
        if (textureFieldsCount > 0) config.bigTextures = EditorGUILayout.Toggle("Big Texture Fields", config.bigTextures);

        //footer
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Space(2);
        foreach (string footNote in footer)
        {
            string[] splitNote = footNote.TrimEnd(')').Split("(".ToCharArray(), 2);
            string value = splitNote[1];
            string type = splitNote[0];
            if (type == "linkButton")
            {
                string[] values = value.Split(",".ToCharArray());
                drawLinkButton(70, 20, values[0], values[1]);
            }
            GUILayout.Space(2);
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        UpdateRenderQueue(material, defaultShader);
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

    //copys og shader and changed render queue and name in there
    public static Shader createRenderQueueShader(Shader defaultShader, int renderQueue)
    {
        string defaultPath = AssetDatabase.GetAssetPath(defaultShader);
        string shaderCode = readFileIntoString(defaultPath);
        string pattern = @"""Queue"" ?= ?""\w+(\+\d+)?""";
        string replacementQueue = "Background+" + (renderQueue-1000);
        if (renderQueue == 1000) replacementQueue = "Background";
        else if(renderQueue<1000) replacementQueue = "Background-" + (1000-renderQueue);
        shaderCode = Regex.Replace(shaderCode, pattern, "\"Queue\" = \""+ replacementQueue + "\"");
        pattern = @"Shader ?""(\w|\/|\.)+""";
        string newShaderName = ".differentQueues/"+defaultShader.name + "-queue" + renderQueue;
        shaderCode = Regex.Replace(shaderCode, pattern, "Shader \""+ newShaderName + "\"");
        pattern = @"#include ""(?!.*(Lighting)|(AutoLight)|(UnityCG)|(UnityShaderVariables)|(HLSLSupport)|(TerrainEngine))";
        shaderCode = Regex.Replace(shaderCode, pattern, "#include \"../", RegexOptions.Multiline);
        string[] pathParts = defaultPath.Split('/');
        string fileName = pathParts[pathParts.Length - 1];
        string newPath = defaultPath.Replace(fileName, "")+"_differentQueues";
        Directory.CreateDirectory(newPath);
        newPath = newPath+ "/" + fileName.Replace(".shader", "-queue" + renderQueue + ".shader");
        writeStringToFile(shaderCode, newPath);
        return Shader.Find(newShaderName);
    }

    public static string readFileIntoString(string path)
    {
        StreamReader reader = new StreamReader(path);
        string ret = reader.ReadToEnd();
        reader.Close();
        return ret;
    }

    public static void writeStringToFile(string s, string path)
    {
        StreamWriter writer = new StreamWriter(path, false);
        writer.Write(s);
        writer.Close();
        AssetDatabase.ImportAsset(path);
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
}
