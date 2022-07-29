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
using System.Linq;
using Thry.ThryEditor;

namespace Thry
{
    public class ShaderEditor : ShaderGUI
    {
        public const string EXTRA_OPTIONS_PREFIX = "--";
        public const float MATERIAL_NOT_RESET = 69.12f;

        public const string PROPERTY_NAME_MASTER_LABEL = "shader_master_label";
        public const string PROPERTY_NAME_LABEL_FILE = "shader_properties_label_file";
        public const string PROPERTY_NAME_LOCALE = "shader_properties_locale";
        public const string PROPERTY_NAME_ON_SWAP_TO_ACTIONS = "shader_on_swap_to";
        public const string PROPERTY_NAME_SHADER_VERSION = "shader_version";
        public const string PROPERTY_NAME_EDITOR_DETECT = "shader_is_using_thry_editor";

        //Static
        private static string s_edtiorDirectoryPath;

        public static InputEvent Input = new InputEvent();
        public static ShaderEditor Active;

        // Stores the different shader properties
        public ShaderGroup MainGroup;
        private RenderQueueProperty _renderQueueProperty;
        private VRCFallbackProperty _vRCFallbackProperty;

        // UI Instance Variables

        public bool DoShowSearchBar;
        private string _enteredSearchTerm = "";
        private string _appliedSearchTerm = "";

        // shader specified values
        private ShaderHeaderProperty _shaderHeader = null;
        private List<FooterButton> _footers;

        // sates
        private bool _isFirstOnGUICall = true;
        private bool _wasUsed = false;
        private bool _doReloadNextDraw = false;
        private bool _didSwapToShader = false;

        //EditorData
        public MaterialEditor Editor;
        public MaterialProperty[] Properties;
        public Material[] Materials;
        public Shader Shader;
        public ShaderPart CurrentProperty;
        public Dictionary<string, ShaderProperty> PropertyDictionary;
        public List<ShaderPart> ShaderParts;
        public List<ShaderProperty> TextureArrayProperties;
        public bool IsFirstCall;
        public bool DoUseShaderOptimizer;
        public bool IsLockedMaterial;
        public bool IsInAnimationMode;
        public Renderer ActiveRenderer;
        public string RenamedPropertySuffix;
        public Locale Locale;
        public ShaderTranslator SuggestedTranslationDefinition;

        //Shader Versioning
        private Version _shaderVersionLocal;
        private Version _shaderVersionRemote;
        private bool _hasShaderUpdateUrl = false;
        private bool _isShaderUpToDate = true;
        private string _shaderUpdateUrl = null;

        //other
        ShaderProperty ShaderOptimizerProperty { get; set; }

        private DefineableAction[] _onSwapToActions = null;

        public bool IsDrawing { get; private set; } = false;
        public bool IsPresetEditor { get; private set; } = false;

        //-------------Init functions--------------------

        private Dictionary<string, string> LoadDisplayNamesFromFile()
        {
            //load display names from file if it exists
            MaterialProperty label_file_property = GetMaterialProperty(PROPERTY_NAME_LABEL_FILE);
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
            if (displayName.Contains(EXTRA_OPTIONS_PREFIX))
            {
                string[] parts = displayName.Split(new string[] { EXTRA_OPTIONS_PREFIX }, 2, System.StringSplitOptions.None);
                displayName = parts[0];
                parts[1] = parts[1].Replace("''", "\"");
                PropertyOptions options = Parser.ParseToObject<PropertyOptions>(parts[1]);
                if (options != null)
                {
                    if (options.condition_showS != null)
                    {
                        options.condition_show = DefineableCondition.Parse(options.condition_showS);
                    }
                    if (options.on_value != null)
                    {
                        options.on_value_actions = PropertyValueAction.ParseToArray(options.on_value);
                    }
                    return options;
                }
            }
            return new PropertyOptions();
        }

        private enum ThryPropertyType
        {
            none, property, master_label, footer, header, headerWithEnd, legacy_header, legacy_header_end, legacy_header_start, group_start, group_end, instancing, dsgi, lightmap_flags, locale, on_swap_to, space, shader_version
        }

        private ThryPropertyType GetPropertyType(MaterialProperty p, PropertyOptions options)
        {
            string name = p.name;
            MaterialProperty.PropFlags flags = p.flags;

            if (DrawingData.LastPropertyDrawerType == DrawerType.Header)
                return (DrawingData.LastPropertyDrawer as ThryHeaderDrawer).GetEndProperty() != null ? ThryPropertyType.headerWithEnd : ThryPropertyType.header;

            if (flags == MaterialProperty.PropFlags.HideInInspector)
            {
                if (name == PROPERTY_NAME_MASTER_LABEL)
                    return ThryPropertyType.master_label;
                if (name == PROPERTY_NAME_ON_SWAP_TO_ACTIONS)
                    return ThryPropertyType.on_swap_to;
                if (name == PROPERTY_NAME_SHADER_VERSION)
                    return ThryPropertyType.shader_version;

                if (name.StartsWith("m_start", StringComparison.Ordinal))
                    return ThryPropertyType.legacy_header_start;
                if (name.StartsWith("m_end", StringComparison.Ordinal))
                    return ThryPropertyType.legacy_header_end;
                if (name.StartsWith("m_", StringComparison.Ordinal))
                    return ThryPropertyType.legacy_header;
                if (name.StartsWith("g_start", StringComparison.Ordinal))
                    return ThryPropertyType.group_start;
                if (name.StartsWith("g_end", StringComparison.Ordinal))
                    return ThryPropertyType.group_end;
                if (name.StartsWith("footer_", StringComparison.Ordinal))
                    return ThryPropertyType.footer;
                if (name == "Instancing")
                    return ThryPropertyType.instancing;
                if (name == "DSGI")
                    return ThryPropertyType.dsgi;
                if (name == "LightmapFlags")
                    return ThryPropertyType.lightmap_flags;
                if (name == PROPERTY_NAME_LOCALE)
                    return ThryPropertyType.locale;
                if (name.StartsWith("space"))
                    return ThryPropertyType.space;
            }
            else if(flags.HasFlag(MaterialProperty.PropFlags.HideInInspector) == false)
            {
                if (!options.hide_in_inspector)
                    return ThryPropertyType.property;
            }
            return ThryPropertyType.none;
        }

        private void LoadLocales()
        {
            MaterialProperty locales_property = GetMaterialProperty(PROPERTY_NAME_LOCALE);
            Locale = null;
            if (locales_property != null)
            {
                string displayName = locales_property.displayName;
                PropertyOptions options = ExtractExtraOptionsFromDisplayName(ref displayName);
                Locale = new Locale(options.file_name);
                Locale.selected_locale_index = (int)locales_property.floatValue;
            }
        }

        //finds all properties and headers and stores them in correct order
        private void CollectAllProperties()
        {
            //load display names from file if it exists
            MaterialProperty[] props = Properties;
            Dictionary<string, string> labels = LoadDisplayNamesFromFile();
            LoadLocales();

            PropertyDictionary = new Dictionary<string, ShaderProperty>();
            ShaderParts = new List<ShaderPart>();
            MainGroup = new ShaderGroup(this); //init top object that all Shader Objects are childs of
            Stack<ShaderGroup> headerStack = new Stack<ShaderGroup>(); //header stack. used to keep track if editorData header to parent new objects to
            headerStack.Push(MainGroup); //add top object as top object to stack
            headerStack.Push(MainGroup); //add top object a second time, because it get's popped with first actual header item
            _footers = new List<FooterButton>(); //init footer list
            int headerCount = 0;
            DrawingData.IsCollectingProperties = true;

            for (int i = 0; i < props.Length; i++)
            {
                string displayName = props[i].displayName;

                //Load from label file
                if (labels.ContainsKey(props[i].name)) displayName = labels[props[i].name];

                //Check for locale
                if (Locale != null)
                {
                    if (displayName.StartsWith("locale::", StringComparison.Ordinal))
                    {
                        if (Locale.Constains(displayName))
                        {
                            displayName = Locale.Get(displayName);
                        }
                    }
                }
                //extract json data from display name
                PropertyOptions options = ExtractExtraOptionsFromDisplayName(ref displayName);

                int offset = options.offset + headerCount;

                DrawingData.ResetLastDrawerData();
                Editor.GetPropertyHeight(props[i]);

                ThryPropertyType type = GetPropertyType(props[i], options);
                switch (type)
                {
                    case ThryPropertyType.header:
                        headerStack.Pop();
                        break;
                    case ThryPropertyType.legacy_header:
                        headerStack.Pop();
                        break;
                    case ThryPropertyType.headerWithEnd:
                    case ThryPropertyType.legacy_header_start:
                        offset = options.offset + ++headerCount;
                        break;
                    case ThryPropertyType.legacy_header_end:
                        headerStack.Pop();
                        headerCount--;
                        break;
                    case ThryPropertyType.on_swap_to:
                        _onSwapToActions = options.actions;
                        break;
                }
                ShaderProperty NewProperty = null;
                ShaderPart newPart = null;
                switch (type)
                {
                    case ThryPropertyType.master_label:
                        _shaderHeader = new ShaderHeaderProperty(this, props[i], displayName, 0, options, false);
                        break;
                    case ThryPropertyType.footer:
                        _footers.Add(new FooterButton(Parser.ParseToObject<ButtonData>(displayName)));
                        break;
                    case ThryPropertyType.header:
                    case ThryPropertyType.headerWithEnd:
                    case ThryPropertyType.legacy_header:
                    case ThryPropertyType.legacy_header_start:
                        ShaderHeader newHeader = new ShaderHeader(this, props[i], Editor, displayName, offset, options);
                        headerStack.Peek().addPart(newHeader);
                        headerStack.Push(newHeader);
                        newPart = newHeader;
                        break;
                    case ThryPropertyType.group_start:
                        ShaderGroup new_group = new ShaderGroup(this, options);
                        headerStack.Peek().addPart(new_group);
                        headerStack.Push(new_group);
                        newPart = new_group;
                        break;
                    case ThryPropertyType.group_end:
                        headerStack.Pop();
                        break;
                    case ThryPropertyType.none:
                    case ThryPropertyType.property:
                        if (props[i].type == MaterialProperty.PropType.Texture)
                            NewProperty = new TextureProperty(this, props[i], displayName, offset, options, props[i].flags.HasFlag(MaterialProperty.PropFlags.NoScaleOffset) == false, !DrawingData.LastPropertyUsedCustomDrawer, i);
                        else
                            NewProperty = new ShaderProperty(this, props[i], displayName, offset, options, false, i);
                        break;
                    case ThryPropertyType.lightmap_flags:
                        NewProperty = new GIProperty(this, props[i], displayName, offset, options, false);
                        break;
                    case ThryPropertyType.dsgi:
                        NewProperty = new DSGIProperty(this, props[i], displayName, offset, options, false);
                        break;
                    case ThryPropertyType.instancing:
                        NewProperty = new InstancingProperty(this, props[i], displayName, offset, options, false);
                        break;
                    case ThryPropertyType.locale:
                        NewProperty = new LocaleProperty(this, props[i], displayName, offset, options, false);
                        break;
                    case ThryPropertyType.shader_version:
                        _shaderVersionRemote = new Version(WebHelper.GetCachedString(options.remote_version_url));
                        _shaderVersionLocal = new Version(displayName);
                        _isShaderUpToDate = _shaderVersionLocal >= _shaderVersionRemote;
                        _shaderUpdateUrl = options.generic_string;
                        _hasShaderUpdateUrl = _shaderUpdateUrl != null;
                        break;
                }
                if (NewProperty != null)
                {
                    newPart = NewProperty;
                    if (PropertyDictionary.ContainsKey(props[i].name))
                        continue;
                    PropertyDictionary.Add(props[i].name, NewProperty);
                    if (type != ThryPropertyType.none)
                        headerStack.Peek().addPart(NewProperty);
                }
                //if new header is at end property
                if (headerStack.Peek() is ShaderHeader && (headerStack.Peek() as ShaderHeader).GetEndProperty() == props[i].name)
                {
                    headerStack.Pop();
                    headerCount--;
                }
                if (newPart != null)
                {
                    ShaderParts.Add(newPart);
                }
            }

            DrawingData.IsCollectingProperties = false;
        }


        // Not in use cause getPropertyHandlerMethod is really expensive
        private void HandleKeyworDrawers()
        {
            foreach (MaterialProperty p in Properties)
            {
                ShaderHelper.EnableDisableKeywordsBasedOnTheirFloatValue(Materials, Shader, p.name);
            }
        }
        

        //-------------Draw Functions----------------

        public void InitlizeThryUI()
        {
            Config config = Config.Singleton;
            Active = this;
            Helper.RegisterEditorUse();

            //get material targets
            Materials = Editor.targets.Select(o => o as Material).ToArray();

            Shader = Materials[0].shader;

            RenamedPropertySuffix = ShaderOptimizer.GetRenamedPropertySuffix(Materials[0]);

            IsPresetEditor = Materials.Length == 1 && Presets.ArePreset(Materials);

            //collect shader properties
            CollectAllProperties();

            if (ShaderOptimizer.IsShaderUsingThryOptimizer(Shader))
            {
                ShaderOptimizerProperty = PropertyDictionary[ShaderOptimizer.GetOptimizerPropertyName(Shader)];
                if(ShaderOptimizerProperty != null) ShaderOptimizerProperty.ExemptFromLockedDisabling = true;
            }

            _renderQueueProperty = new RenderQueueProperty(this);
            _vRCFallbackProperty = new VRCFallbackProperty(this);
            ShaderParts.Add(_renderQueueProperty);
            ShaderParts.Add(_vRCFallbackProperty);

            AddResetProperty();

            _isFirstOnGUICall = false;
        }

        private Dictionary<string, MaterialProperty> materialPropertyDictionary;
        public MaterialProperty GetMaterialProperty(string name)
        {
            if (materialPropertyDictionary == null)
            {
                materialPropertyDictionary = new Dictionary<string, MaterialProperty>();
                foreach (MaterialProperty p in Properties)
                    if (materialPropertyDictionary.ContainsKey(p.name) == false) materialPropertyDictionary.Add(p.name, p);
            }
            if (materialPropertyDictionary.ContainsKey(name))
                return materialPropertyDictionary[name];
            return null;
        }

        private void AddResetProperty()
        {
            if (Materials[0].HasProperty(PROPERTY_NAME_EDITOR_DETECT) == false)
            {
                EditorChanger.AddThryProperty(Materials[0].shader);
            }
            Materials[0].SetFloat(PROPERTY_NAME_EDITOR_DETECT, 69);
        }

        public override void OnClosed(Material material)
        {
            base.OnClosed(material);
            _isFirstOnGUICall = true;
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            //Unity sets the render queue to the shader defult when changing shader
            //This seems to be some deeper process that cant be disabled so i just set it again after the swap
            //Even material.shader = newShader resets the queue. (this is actually the only thing the base function does)
            int previousQueue = material.renderQueue;
            base.AssignNewShaderToMaterial(material, oldShader, newShader);
            material.renderQueue = previousQueue;
            SuggestedTranslationDefinition = ShaderTranslator.CheckForExistingTranslationFile(oldShader, newShader);
            _doReloadNextDraw = true;
            _didSwapToShader = true;
        }

        void InitEditorData(MaterialEditor materialEditor)
        {
            Editor = materialEditor;
            TextureArrayProperties = new List<ShaderProperty>();
            IsFirstCall = true;
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            IsDrawing = true;
            //Init
            bool reloadUI = _isFirstOnGUICall || (_doReloadNextDraw && Event.current.type == EventType.Layout) || (materialEditor.target as Material).shader != Shader;
            if (reloadUI) 
            {
                InitEditorData(materialEditor);
                Properties = props;
                InitlizeThryUI();
            }

            //Update Data
            Properties = props;
            Shader = Materials[0].shader;
            Input.Update(IsLockedMaterial);
            ActiveRenderer = Selection.activeTransform?.GetComponent<Renderer>();
            IsInAnimationMode = AnimationMode.InAnimationMode();

            Active = this;

            GUIManualReloadButton();
            GUIShaderVersioning();

            GUITopBar();
            GUISearchBar();
            Presets.PresetEditorGUI(this);
            ShaderTranslator.SuggestedTranslationButtonGUI(this);

            //PROPERTIES
            foreach (ShaderPart part in MainGroup.parts)
            {
                part.Draw();
            }

            //Render Queue selection
            if(VRCInterface.IsVRCSDKInstalled()) _vRCFallbackProperty.Draw();
            if (Config.Singleton.showRenderQueue) _renderQueueProperty.Draw();

            BetterTooltips.DrawActive();

            GUIFooters();

            HandleEvents();

            IsDrawing = false;
        }

        private void GUIManualReloadButton()
        {
            if (Config.Singleton.showManualReloadButton)
            {
                if(GUILayout.Button("Manual Reload"))
                {
                    this.Reload();
                }
            }
        }

        private void GUIShaderVersioning()
        {
            if (!_isShaderUpToDate)
            {
                Rect r = EditorGUILayout.GetControlRect(false, _hasShaderUpdateUrl ? 30 : 15);
                EditorGUI.LabelField(r, $"[New Shader Version available] {_shaderVersionLocal} -> {_shaderVersionRemote}" + (_hasShaderUpdateUrl ? "\n    Click here to download." : ""), Styles.redStyle);
                if(Input.HadMouseDownRepaint && _hasShaderUpdateUrl && GUILayoutUtility.GetLastRect().Contains(Input.mouse_position)) Application.OpenURL(_shaderUpdateUrl);
            }
        }

        private void GUITopBar()
        {
            //if header is texture, draw it first so other ui elements can be positions below
            if (_shaderHeader != null && _shaderHeader.Options.texture != null) _shaderHeader.Draw();

            bool drawAboveToolbar = EditorGUIUtility.wideMode == false;
            if(drawAboveToolbar) _shaderHeader.Draw(new CRect(EditorGUILayout.GetControlRect()));

            Rect mainHeaderRect = EditorGUILayout.BeginHorizontal();
            //draw editor settings button
            if (GuiHelper.ButtonWithCursor(Styles.icon_style_settings, "Settings", 25, 25))
            {
                EditorWindow.GetWindow<Settings>(false, "Thry Settings", true);
            }
            if (GuiHelper.ButtonWithCursor(Styles.icon_style_search, "Search", 25, 25))
            {
                DoShowSearchBar = !DoShowSearchBar;
                if(!DoShowSearchBar) ClearSearch();
            }
            Presets.PresetGUI(this);

            //draw master label text after ui elements, so it can be positioned between
            if (_shaderHeader != null && !drawAboveToolbar) _shaderHeader.Draw(new CRect(mainHeaderRect));

            GUILayout.FlexibleSpace();
            Rect popupPosition;
            if (GuiHelper.ButtonWithCursor(Styles.icon_style_tools, "Tools", 25, 25, out popupPosition))
            {
                PopupTools(popupPosition);
            }
            ShaderTranslator.TranslationSelectionGUI(this);
            if (GuiHelper.ButtonWithCursor(Styles.icon_style_thryIcon, "Thryrallo", 25, 25))
                Application.OpenURL("https://www.twitter.com/thryrallo");
            EditorGUILayout.EndHorizontal();
        }

        private void GUISearchBar()
        {
            if (DoShowSearchBar)
            {
                EditorGUI.BeginChangeCheck();
                _enteredSearchTerm = EditorGUILayout.TextField(_enteredSearchTerm);
                if (EditorGUI.EndChangeCheck())
                {
                    _appliedSearchTerm = _enteredSearchTerm.ToLower();
                    UpdateSearch(MainGroup);
                }
            }
        }

        private void GUIFooters()
        {
            try
            {
                FooterButton.DrawList(_footers);
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }
            if (GUILayout.Button("@UI Made by Thryrallo", Styles.made_by_style))
                Application.OpenURL("https://www.twitter.com/thryrallo");
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
        }

        private void PopupTools(Rect position)
        {
            var menu = new GenericMenu();
            int unboundTextures = MaterialCleaner.CountUnusedProperties(MaterialCleaner.CleanPropertyType.Texture, Materials);
            int unboundProperties = MaterialCleaner.CountAllUnusedProperties(Materials);
            List<string> unusedTextures = new List<string>();
            MainGroup.FindUnusedTextures(unusedTextures, true);
            if (unboundTextures > 0 && !IsLockedMaterial)
            {
                menu.AddItem(new GUIContent($"Unbound Textures: {unboundTextures}/List in console"), false, delegate ()
                {
                    MaterialCleaner.ListUnusedProperties(MaterialCleaner.CleanPropertyType.Texture, Materials);
                });
                menu.AddItem(new GUIContent($"Unbound Textures: {unboundTextures}/Remove"), false, delegate ()
                {
                    MaterialCleaner.RemoveUnusedProperties(MaterialCleaner.CleanPropertyType.Texture, Materials);
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent($"Unbound textures: 0"));
            }
            if (unusedTextures.Count > 0 && !IsLockedMaterial)
            {
                menu.AddItem(new GUIContent($"Unused Textures: {unusedTextures.Count}/List in console"), false, delegate ()
                {
                    Out("Unused textures", unusedTextures.Select(s => $"↳{s}"));
                });
                menu.AddItem(new GUIContent($"Unused Textures: {unusedTextures.Count}/Remove"), false, delegate ()
                {
                    foreach (string t in unusedTextures) if (PropertyDictionary.ContainsKey(t)) PropertyDictionary[t].MaterialProperty.textureValue = null;
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent($"Unused textures: 0"));
            }
            if (unboundProperties > 0 && !IsLockedMaterial)
            {
                menu.AddItem(new GUIContent($"Unbound properties: {unboundProperties}/List in console"), false, delegate ()
                {
                    MaterialCleaner.ListUnusedProperties(MaterialCleaner.CleanPropertyType.Texture, Materials);
                    MaterialCleaner.ListUnusedProperties(MaterialCleaner.CleanPropertyType.Float, Materials);
                    MaterialCleaner.ListUnusedProperties(MaterialCleaner.CleanPropertyType.Color, Materials);
                });
                menu.AddItem(new GUIContent($"Unbound properties: {unboundProperties}/Remove"), false, delegate ()
                {
                    MaterialCleaner.RemoveAllUnusedProperties(MaterialCleaner.CleanPropertyType.Texture, Materials);
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent($"Unbound properties: 0"));
            }
            menu.DropDown(position);
        }

        public static void Out(string s)
        {
            Debug.Log($"<color=#ff80ff>[Thry]</color> {s}");
        }
        public static void Out(string header, params string[] lines)
        {
            Debug.Log($"<color=#ff80ff>[Thry]</color> <b>{header}</b>\n{lines.Aggregate((s1, s2) => s1 + "\n" + s2)}");
        }
        public static void Out(string header, IEnumerable<string> lines)
        {
            if (lines.Count() == 0) Out(header);
            else Debug.Log($"<color=#ff80ff>[Thry]</color> <b>{header}</b>\n{lines.Aggregate((s1, s2) => s1 + "\n" + s2)}");
        }
        public static void Out(string header, Color c, IEnumerable<string> lines)
        {
            if (lines.Count() == 0) Out(header);
            else Debug.Log($"<color=#ff80ff>[Thry]</color> <b><color={ColorUtility.ToHtmlStringRGB(c)}>{header}</b></color> \n{lines.Aggregate((s1, s2) => s1 + "\n" + s2)}");
        }

        private void HandleEvents()
        {
            Event e = Event.current;
            //if reloaded, set reload to false
            if (_doReloadNextDraw && Event.current.type == EventType.Layout) _doReloadNextDraw = false;

            //if was undo, reload
            bool isUndo = (e.type == EventType.ExecuteCommand || e.type == EventType.ValidateCommand) && e.commandName == "UndoRedoPerformed";
            if (isUndo) _doReloadNextDraw = true;


            //on swap
            if (_onSwapToActions != null && _didSwapToShader)
            {
                foreach (DefineableAction a in _onSwapToActions)
                    a.Perform(Materials);
                _onSwapToActions = null;
                _didSwapToShader = false;
            }

            //test if material has been reset
            if (_wasUsed && e.type == EventType.Repaint)
            {
                if (Materials[0].HasProperty("shader_is_using_thry_editor") && Materials[0].GetFloat("shader_is_using_thry_editor") != 69)
                {
                    _doReloadNextDraw = true;
                    HandleReset();
                    _wasUsed = true;
                }
            }

            if (e.type == EventType.Used) _wasUsed = true;
            if (Input.HadMouseDownRepaint) Input.HadMouseDown = false;
            Input.HadMouseDownRepaint = false;
            IsFirstCall = false;
            materialPropertyDictionary = null;
        }

        //iterate the same way drawing would iterate
        //if display part, display all parents parts
        private void UpdateSearch(ShaderPart part)
        {
            part.has_not_searchedFor = part.Content.text.ToLower().Contains(_appliedSearchTerm) == false;
            if (part is ShaderGroup)
            {
                foreach (ShaderPart p in (part as ShaderGroup).parts)
                {
                    UpdateSearch(p);
                    part.has_not_searchedFor &= p.has_not_searchedFor;
                }
            }
        }

        private void ClearSearch()
        {
            _appliedSearchTerm = "";
            UpdateSearch(MainGroup);
        }

        private void HandleReset()
        {
            MaterialLinker.UnlinkAll(Materials[0]);
            ShaderOptimizer.DeleteTags(Materials);
        }

        public void Repaint()
        {
            if (Materials.Length > 0)
                EditorUtility.SetDirty(Materials[0]);
        }

        public static void RepaintActive()
        {
            if (ShaderEditor.Active != null)
                Active.Repaint();
        }

        public void Reload()
        {
            this._isFirstOnGUICall = true;
            this._didSwapToShader = true;
            this._doReloadNextDraw = true;
            this.Repaint();
        }

        public static void ReloadActive()
        {
            if (ShaderEditor.Active != null)
                Active.Reload();
        }

        public void ApplyDrawers()
        {
            foreach (Material target in Materials)
                MaterialEditor.ApplyMaterialPropertyDrawers(target);
        }

        public static string GetShaderEditorDirectoryPath()
        {
            if (s_edtiorDirectoryPath == null)
            {
                IEnumerable<string> paths = AssetDatabase.FindAssets("ThryEditor").Select(g => AssetDatabase.GUIDToAssetPath(g));
                foreach (string p in paths)
                {
                    if (p.EndsWith("/ThryEditor.cs"))
                        s_edtiorDirectoryPath = Directory.GetParent(Path.GetDirectoryName(p)).FullName;
                }
            }
            return s_edtiorDirectoryPath;
        }

        [MenuItem("Thry/Shader Tools/Fix Keywords (Very Slow)", priority = -20)]
        static void FixKeywords()
        {
            IEnumerable<Material> materials = AssetDatabase.FindAssets("t:material").Select(g => AssetDatabase.GUIDToAssetPath(g)).Where(p => string.IsNullOrEmpty(p) == false)
                .Select(p => AssetDatabase.LoadAssetAtPath<Material>(p)).Where(m => m != null && m.shader != null)
                .Where(m => ShaderOptimizer.IsMaterialLocked(m) == false && ShaderHelper.IsShaderUsingThryShaderEditor(m.shader));
            float f = 0;
            int count = materials.Count();
            foreach(Material m in materials)
            {
                for(int i= 0;i< m.shader.GetPropertyCount(); i++){
                    if (m.shader.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Float)
                    {
                        ShaderHelper.EnableDisableKeywordsBasedOnTheirFloatValue(new Material[] { m }, m.shader, m.shader.GetPropertyName(i));
                    }
                }
                EditorUtility.DisplayProgressBar("Fixing Keywords", m.name, f++ / count);
            }
            EditorUtility.ClearProgressBar();
        }

        [MenuItem("Thry/Twitter", priority = -100)]
        static void MenuThryTwitter()
        {
            Application.OpenURL("https://www.twitter.com/thryrallo");
        }

        [MenuItem("Thry/ShaderUI/Settings",priority = -20)]
        static void MenuShaderUISettings()
        {
            EditorWindow.GetWindow<Settings>(false, "Thry Settings", true);
        }

        [MenuItem("Thry/Shader Optimizer/Upgraded Animated Properties", priority = -20)]
        static void MenuUpgradeAnimatedPropertiesToTagsOnAllMaterials()
        {
            ShaderOptimizer.UpgradeAnimatedPropertiesToTagsOnAllMaterials();
        }

        [MenuItem("Thry/ShaderUI/Use Thry Editor for other shaders", priority = 0)]
        static void MenuShaderUIAddToShaders()
        {
            EditorWindow.GetWindow<EditorChanger>(false, "UI Changer", true);
        }

        [MenuItem("Thry/Shader Optimizer/Unlocked Materials List", priority = 0)]
        static void MenuShaderOptUnlockedMaterials()
        {
            EditorWindow.GetWindow<UnlockedMaterialsList>(false, "Unlocked Materials", true);
        }
    }
}
