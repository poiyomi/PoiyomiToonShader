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

namespace Thry
{
    public class ShaderEditor : ShaderGUI
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
        private int customRenderQueueFieldInput = -1;
        private PresetHandler presetHandler;

        private bool show_search_bar;
        private string header_search_term = "";
        private bool show_eyeIcon_tutorial = false;

        // shader specified values
        private string masterLabelText = null;
        private List<ButtonData> footer;

        // sates
        private static bool reloadNextDraw = false;
        private bool firstOnGUICall = true;
        private bool wasUsed = false;

        public static InputEvent input = new InputEvent();
        // Contains Editor Data
        public EditorData editorData;
        public static EditorData currentlyDrawing;
        public static ShaderEditor active;

        private DefineableAction[] on_swap_to_actions = null;
        private bool swapped_to_shader = false;

        //-------------Init functions--------------------

        private Dictionary<string, string> LoadDisplayNamesFromFile()
        {
            //load display names from file if it exists
            MaterialProperty label_file_property = null;
            foreach (MaterialProperty m in editorData.properties) if (m.name == PROPERTY_NAME_LABEL_FILE) label_file_property = m;
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
                if (options != null)
                    return options;
            }
            return new PropertyOptions();
        }

        private enum ThryPropertyType
        {
            none, property, master_label, footer, header, header_end, header_start, group_start, group_end, instancing, dsgi, lightmap_flags, locale, on_swap_to, space, shader_optimizer
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
            if (name.Replace(" ", "") == "Instancing" && flags == MaterialProperty.PropFlags.HideInInspector)
                return ThryPropertyType.instancing;
            if (name.Replace(" ", "") == "DSGI" && flags == MaterialProperty.PropFlags.HideInInspector)
                return ThryPropertyType.dsgi;
            if (name.Replace(" ", "") == "LightmapFlags" && flags == MaterialProperty.PropFlags.HideInInspector)
                return ThryPropertyType.lightmap_flags;
            if (name.Replace(" ", "") == PROPERTY_NAME_LOCALE)
                return ThryPropertyType.locale;
            if (name == "_ShaderOptimizerEnabled")
                return ThryPropertyType.shader_optimizer;
            if (flags != MaterialProperty.PropFlags.HideInInspector && !options.hide_in_inspector)
                return ThryPropertyType.property;
            return ThryPropertyType.none;
        }

        public Locale locale;

        private void LoadLocales()
        {
            MaterialProperty locales_property = null;
            locale = null;
            foreach (MaterialProperty m in editorData.properties) if (m.name == PROPERTY_NAME_LOCALE) locales_property = m;
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
            MaterialProperty[] props = editorData.properties;
            Dictionary<string, string> labels = LoadDisplayNamesFromFile();
            LoadLocales();

            editorData.propertyDictionary = new Dictionary<string, ShaderProperty>();
            editorData.shaderParts = new List<ShaderPart>();
            shaderparts = new ShaderHeader(); //init top object that all Shader Objects are childs of
            Stack<ShaderGroup> headerStack = new Stack<ShaderGroup>(); //header stack. used to keep track if editorData header to parent new objects to
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
                        if (displayName.Contains("locale::" + key))
                            displayName = displayName.Replace("locale::" + key, locale.Get(key));
                displayName = Regex.Replace(displayName, @"''", "\"");

                if (labels.ContainsKey(props[i].name)) displayName = labels[props[i].name];
                PropertyOptions options = ExtractExtraOptionsFromDisplayName(ref displayName);

                int offset = options.offset + headerCount;

                //Handle keywords
                object propertyHandler = getPropertyHandlerMethod.Invoke(null, new object[] { editorData.shader, props[i].name });
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
                                foreach (Material m in editorData.materials)
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


                ThryPropertyType type = GetPropertyType(props[i], options);
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
                ShaderPart newPart = null;
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
                        if (options.is_hideable) editorData.show_HeaderHider = true;
                        ShaderHeader newHeader = new ShaderHeader(props[i], editorData.editor, displayName, offset, options);
                        headerStack.Peek().addPart(newHeader);
                        headerStack.Push(newHeader);
                        HeaderHider.InitHidden(newHeader);
                        newPart = newHeader;
                        break;
                    case ThryPropertyType.group_start:
                        ShaderGroup new_group = new ShaderGroup(options);
                        headerStack.Peek().addPart(new_group);
                        headerStack.Push(new_group);
                        newPart = new_group;
                        break;
                    case ThryPropertyType.group_end:
                        headerStack.Pop();
                        break;
                    case ThryPropertyType.none:
                    case ThryPropertyType.property:
                        DrawingData.lastPropertyUsedCustomDrawer = false;
                        editorData.editor.GetPropertyHeight(props[i]);
                        bool forceOneLine = props[i].type == MaterialProperty.PropType.Vector && !DrawingData.lastPropertyUsedCustomDrawer;
                        if (props[i].type == MaterialProperty.PropType.Texture)
                            newPorperty = new TextureProperty(props[i], displayName, offset, options, props[i].flags.HasFlag(MaterialProperty.PropFlags.NoScaleOffset) == false, !DrawingData.lastPropertyUsedCustomDrawer);
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
                    case ThryPropertyType.shader_optimizer:
                        editorData.use_ShaderOptimizer = true;
                        newPorperty = new ShaderProperty(props[i], displayName, offset, options, false);
                        break;
                }
                if (newPorperty != null)
                {
                    newPart = newPorperty;
                    if (editorData.propertyDictionary.ContainsKey(props[i].name))
                        continue;
                    editorData.propertyDictionary.Add(props[i].name, newPorperty);
                    if (type != ThryPropertyType.none && type != ThryPropertyType.shader_optimizer)
                        headerStack.Peek().addPart(newPorperty);
                }
                if (newPart != null)
                    editorData.shaderParts.Add(newPart);
            }
        }

        private MaterialProperty FindProperty(string name)
        {
            return System.Array.Find(editorData.properties,
                           element => element.name == name);
        }

        //-------------Functions------------------

        public void UpdateRenderQueueInstance()
        {
            if (editorData.materials != null) foreach (Material m in editorData.materials)
                    if (m.shader.renderQueue != m.renderQueue)
                        Thry.MaterialHelper.UpdateRenderQueue(m, editorData.defaultShader);
        }

        //-------------Draw Functions----------------

        public void OnOpen()
        {
            Config config = Config.Get();

            show_eyeIcon_tutorial = !EditorPrefs.GetBool("thry_openeEyeIcon", false);

            currentlyDrawing = editorData;
            active = this;

            //get material targets
            UnityEngine.Object[] targets = editorData.editor.targets;
            editorData.materials = new Material[targets.Length];
            for (int i = 0; i < targets.Length; i++) editorData.materials[i] = targets[i] as Material;

            presetHandler = new PresetHandler(editorData.properties);

            editorData.shader = editorData.materials[0].shader;
            string defaultShaderName = editorData.materials[0].shader.name.Split(new string[] { "-queue" }, System.StringSplitOptions.None)[0].Replace(".differentQueues/", "");
            editorData.defaultShader = Shader.Find(defaultShaderName);

            //collect shader properties
            CollectAllProperties();

            //update render queue if render queue selection is deactivated
            if (!config.renderQueueShaders && !config.showRenderQueue)
            {
                editorData.materials[0].renderQueue = editorData.defaultShader.renderQueue;
                UpdateRenderQueueInstance();
            }

            AddResetProperty();

            firstOnGUICall = false;
        }

        private void AddResetProperty()
        {
            if (editorData.materials[0].HasProperty("shader_is_using_thry_editor") == false)
            {
                EditorChanger.AddThryProperty(editorData.materials[0].shader);
            }
            editorData.materials[0].SetFloat("shader_is_using_thry_editor", 69);
        }

        public override void OnClosed(Material material)
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
            input.MouseLeftClick = e.type == EventType.MouseDown && e.button == 0;
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
                editorData = new EditorData();
                editorData.editor = materialEditor;
                editorData.gui = this;
                editorData.textureArrayProperties = new List<ShaderProperty>();
                editorData.firstCall = true;
            }
            editorData.properties = props;

            CheckInAnimationRecordMode();
            m_RenderersForAnimationMode = MaterialEditor.PrepareMaterialPropertiesForAnimationMode(props, GUI.enabled);
            UpdateEvents();

            //first time call inits
            if (firstOnGUICall || (reloadNextDraw && Event.current.type == EventType.Layout)) OnOpen();
            editorData.shader = editorData.materials[0].shader;

            currentlyDrawing = editorData;
            active = this;

            //sync shader and get preset handler
            Config config = Config.Get();
            if (editorData.materials != null)
                Mediator.SetActiveShader(editorData.materials[0].shader, presetHandler: presetHandler);


            //TOP Bar
            Rect mainHeaderRect = EditorGUILayout.BeginHorizontal();
            //draw editor settings button
            if (GUILayout.Button(new GUIContent("", Styles.settings_icon), EditorStyles.largeLabel, GUILayout.MaxHeight(20), GUILayout.MaxWidth(20)))
            {
                Thry.Settings window = Thry.Settings.getInstance();
                window.Show();
                window.Focus();
            }
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            if (GUILayout.Button(Styles.search_icon, EditorStyles.largeLabel, GUILayout.MaxHeight(20)))
                show_search_bar = !show_search_bar;

            //draw master label if exists
            if (masterLabelText != null) GuiHelper.DrawMasterLabel(masterLabelText, mainHeaderRect);

            //draw presets if exists
            presetHandler.drawPresets(editorData.properties, editorData.materials);
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);

            EditorGUILayout.EndHorizontal();

            if (show_search_bar)
                header_search_term = EditorGUILayout.TextField(header_search_term);

            //Visibility menu
            if (editorData.show_HeaderHider)
            {
                HeaderHider.HeaderHiderGUI(editorData);
            }

            bool isMaterialLocked = editorData.use_ShaderOptimizer && editorData.propertyDictionary["_ShaderOptimizerEnabled"].materialProperty.floatValue == 1;
            if (editorData.use_ShaderOptimizer)
                editorData.propertyDictionary["_ShaderOptimizerEnabled"].Draw();

            EditorGUI.BeginDisabledGroup(isMaterialLocked);
            //PROPERTIES
            if (header_search_term == "" || show_search_bar == false)
            {
                foreach (ShaderPart part in shaderparts.parts)
                    part.Draw();
            }
            else
            {
                foreach (ShaderPart part in editorData.propertyDictionary.Values)
                    if (IsSearchedFor(part, header_search_term))
                        part.Draw();
            }
            EditorGUI.EndDisabledGroup();

            //Render Queue selection
            if (config.showRenderQueue)
            {
                if (config.renderQueueShaders)
                {
                    customRenderQueueFieldInput = GuiHelper.drawRenderQueueSelector(editorData.defaultShader, customRenderQueueFieldInput);
                    EditorGUILayout.LabelField("Default: " + editorData.defaultShader.name);
                    EditorGUILayout.LabelField("Shader: " + editorData.shader.name);
                }
                else
                {
                    materialEditor.RenderQueueField();
                }
            }

            //footer
            GuiHelper.drawFooters(footer);

            if (GUILayout.Button("@UI Made by Thryrallo", Styles.made_by_style))
                Application.OpenURL("https://www.twitter.com/thryrallo");
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);

            Event e = Event.current;
            bool isUndo = (e.type == EventType.ExecuteCommand || e.type == EventType.ValidateCommand) && e.commandName == "UndoRedoPerformed";
            if (reloadNextDraw && Event.current.type == EventType.Layout) reloadNextDraw = false;
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
                if (editorData.materials[0].HasProperty("shader_is_using_thry_editor") && editorData.materials[0].GetFloat("shader_is_using_thry_editor") != 69)
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
            editorData.firstCall = false;
        }

        private bool IsSearchedFor(ShaderPart part, string term)
        {

            string lowercaseTerm = header_search_term.ToLower();
            return part.content.text.ToLower().Contains(lowercaseTerm);
        }

        private void HandleReset()
        {
            MaterialLinker.UnlinkAll(editorData.materials[0]);
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
                catch (System.Exception e)
                {
                    Debug.Log(e.ToString());
                }
            }
        }

        private static string edtior_directory_path;
        public static string GetShaderEditorDirectoryPath()
        {
            if (edtior_directory_path == null)
            {
                string[] guids = AssetDatabase.FindAssets("ShaderEditor");
                foreach (string g in guids)
                {
                    string p = AssetDatabase.GUIDToAssetPath(g);
                    if (p.EndsWith("/ShaderEditor.cs"))
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

        //=============Animation Handling============

        private Renderer m_RenderersForAnimationMode;
        private Renderer rendererForAnimationMode
        {
            get
            {
                if (m_RenderersForAnimationMode == null)
                    return null;
                return m_RenderersForAnimationMode;
            }
        }

        private struct AnimatedCheckData
        {
            public MaterialProperty property;
            public Rect totalPosition;
            public Color color;
            public AnimatedCheckData(MaterialProperty property, Rect totalPosition, Color color)
            {
                this.property = property;
                this.totalPosition = totalPosition;
                this.color = color;
            }
        }

        private static Stack<AnimatedCheckData> s_AnimatedCheckStack = new Stack<AnimatedCheckData>();

        public void BeginAnimatedCheck(MaterialProperty prop)
        {
            if (rendererForAnimationMode == null)
                return;

            s_AnimatedCheckStack.Push(new AnimatedCheckData(prop, Rect.zero, GUI.backgroundColor));

            Color overrideColor;
            if (OverridePropertyColor(prop, rendererForAnimationMode, out overrideColor))
                GUI.backgroundColor = overrideColor;
        }

        public void EndAnimatedCheck()
        {
            if (rendererForAnimationMode == null)
                return;

            AnimatedCheckData data = s_AnimatedCheckStack.Pop();
            if (Event.current.type == EventType.ContextClick && data.totalPosition.Contains(Event.current.mousePosition))
            {
                //DoPropertyContextMenu(data.property);
            }

            GUI.backgroundColor = data.color;
        }

        private static FieldInfo _field_s_InAnimationRecordMode;
        public static bool AnimationIsRecording { get; private set; }

        private static void CheckInAnimationRecordMode()
        {
            if (_field_s_InAnimationRecordMode == null)
                _field_s_InAnimationRecordMode = (typeof(AnimationMode)).GetField("s_InAnimationRecordMode", BindingFlags.NonPublic | BindingFlags.Static);
            AnimationIsRecording = (bool)_field_s_InAnimationRecordMode.GetValue(null);
        }


        const string kMaterialPrefix = "material.";
        static public bool OverridePropertyColor(MaterialProperty materialProp, Renderer target, out Color color)
        {
            var propertyPaths = new List<string>();
            string basePropertyPath = kMaterialPrefix + materialProp.name;

            if (materialProp.type == MaterialProperty.PropType.Texture)
            {
                propertyPaths.Add(basePropertyPath + "_ST.x");
                propertyPaths.Add(basePropertyPath + "_ST.y");
                propertyPaths.Add(basePropertyPath + "_ST.z");
                propertyPaths.Add(basePropertyPath + "_ST.w");
            }
            else if (materialProp.type == MaterialProperty.PropType.Color)
            {
                propertyPaths.Add(basePropertyPath + ".r");
                propertyPaths.Add(basePropertyPath + ".g");
                propertyPaths.Add(basePropertyPath + ".b");
                propertyPaths.Add(basePropertyPath + ".a");
            }
            else if (materialProp.type == MaterialProperty.PropType.Vector)
            {
                propertyPaths.Add(basePropertyPath + ".x");
                propertyPaths.Add(basePropertyPath + ".y");
                propertyPaths.Add(basePropertyPath + ".z");
                propertyPaths.Add(basePropertyPath + ".w");
            }
            else
            {
                propertyPaths.Add(basePropertyPath);
            }

            if (propertyPaths.Exists(path => AnimationMode.IsPropertyAnimated(target, path)))
            {
                color = AnimationMode.animatedPropertyColor;
                if (AnimationIsRecording)
                    color = AnimationMode.recordedPropertyColor;
                //else if (propertyPaths.Exists(path => IsPropertyCandidate(target, path)))
                //    color = AnimationMode.candidatePropertyColor;

                return true;
            }

            color = Color.white;
            return false;
        }

        [MenuItem("Thry/Twitter")]
        static void Init()
        {
            Application.OpenURL("https://www.twitter.com/thryrallo");
        }
    }
}
