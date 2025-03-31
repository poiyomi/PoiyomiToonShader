﻿// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using Thry.ThryEditor;
using Thry.ThryEditor.ShaderTranslations;
using JetBrains.Annotations;
using Thry.ThryEditor.Helpers;
using Thry.ThryEditor.Drawers;
using static Thry.ThryEditor.UnityHelper;

namespace Thry
{
    public class ShaderEditor : ShaderGUI
    {
        public const string EXTRA_OPTIONS_PREFIX = "--";
        public const string PROPERTY_NAME_MASTER_LABEL = "shader_master_label";
        public const string PROPERTY_NAME_LOCALE = "shader_locale";
        public const string PROPERTY_NAME_ON_SWAP_TO_ACTIONS = "shader_on_swap_to";
        public const string PROPERTY_NAME_SHADER_VERSION = "shader_version";
        public const string PROPERTY_NAME_IN_SHADER_PRESETS = "_Mode";

        //Static
        private static string s_edtiorDirectoryPath;

        public static InputEvent Input = new InputEvent();
        public static ShaderEditor Active { get; private set; }

        // Stores the different shader properties
        private ShaderGroup _mainGroup;
        private RenderQueueProperty _renderQueueProperty;
        private VRCFallbackProperty _vRCFallbackProperty;

        // UI Instance Variables

        private string _enteredSearchTerm = "";
        private string _appliedSearchTerm = "";
        public bool IsInSearchMode { private set; get; } = false;

        // shader specified values
        private ShaderHeaderProperty _shaderHeader = null;
        private List<FooterButton> _footers;

        // sates
        private bool _isFirstOnGUICall = true;
        private bool _doReloadNextDraw = false;
        private bool _didSwapToShader = false;
        private bool _didRegisterCallbacks = false;

        //EditorData
        public bool DoUseShaderOptimizer;
        public string RenamedPropertySuffix;
        public bool HasCustomRenameSuffix;
        public bool IsLockedMaterial;
        public bool IsCrossEditor = false;

        public MaterialEditor Editor { get; private set; }
        public MaterialProperty[] Properties { get; private set; }
        public Material[] Materials { get; private set; }
        public Shader Shader { get; private set; }
        public ShaderImporter ImporterShader { get; private set; }
        public Shader LastShader { get; private set; }
        public ShaderPart CurrentProperty;
        public Dictionary<string, ShaderProperty> PropertyDictionary { get; private set; }
        public List<ShaderPart> ShaderParts { get; private set; }
        public List<ShaderProperty> TextureArrayProperties { get; private set; }
        public bool IsFirstCall { get; private set; }
        public bool IsInAnimationMode { get; private set; }
        public Renderer ActiveRenderer { get; private set; }
        public Localization Locale { get; private set; }
        public ShaderTranslator SuggestedTranslationDefinition { get; private set; }
        private string _duplicatePropertyNamesString = null;

        //Shader Versioning
        private ThryEditor.Version _shaderVersionLocal;
        private ThryEditor.Version _shaderVersionRemote;
        private bool _hasShaderUpdateUrl = false;
        private bool _isShaderUpToDate = true;
        private string _shaderUpdateUrl = null;

        //other
        string ShaderOptimizerPropertyName = null;
        ShaderProperty ShaderOptimizerProperty { get; set; }
        ShaderProperty LocaleProperty { get; set; }
        ShaderProperty InShaderPresetsProperty { get; set; }

        [PublicAPI]
        public float ShaderRenderingPreset { get => InShaderPresetsProperty.FloatValue; set => InShaderPresetsProperty.FloatValue = value; }

        private DefineableAction[] _onSwapToActions = null;

        public bool IsDrawing { get; private set; } = false;
        public bool IsPresetEditor { get; private set; } = false;
        public bool IsSectionedPresetEditor
        {
            get
            {
                return IsPresetEditor && Presets.IsMaterialSectionedPreset(Materials[0]);
            }
        }

        public bool HasMixedCustomPropertySuffix
        {
            get
            {
                if (Materials.Length == 1) return false;
                string suffix = ShaderOptimizer.GetRenamedPropertySuffix(Materials[0]);
                for (int i = 1; i < Materials.Length; i++)
                {
                    if (suffix != ShaderOptimizer.GetRenamedPropertySuffix(Materials[i])) return true;
                }
                return false;
            }
        }

        public bool DidSwapToNewShader
        {
            get
            {
                return _didSwapToShader;
            }
        }

        public string TargetName
        {
            get
            {
                if(Materials.Length == 1)
                {
                    return Materials[0].name;
                }
                return $"{Materials.Length} Materials";
            }
        }

        public void SetShader(Shader shader, Shader lastShader = null)
        {
            Shader = shader;
            LastShader = lastShader;
            ImporterShader = ShaderImporter.GetAtPath(AssetDatabase.GetAssetPath(Shader)) as ShaderImporter;
        }

        // Needed for the CrossEditor
        static Dictionary<Shader, ShaderImporter> s_shaderImporterCache = new Dictionary<Shader, ShaderImporter>();
        public ShaderImporter GetShaderImporter(Shader shader)
        {
            if(s_shaderImporterCache.ContainsKey(shader))
            {
                return s_shaderImporterCache[shader];
            }
            ShaderImporter importer = ShaderImporter.GetAtPath(AssetDatabase.GetAssetPath(shader)) as ShaderImporter;
            s_shaderImporterCache.Add(shader, importer);
            return importer;
        }

        // Needed for the CrossEditor
        Dictionary<string, MaterialEditor> s_editorCache = new Dictionary<string, MaterialEditor>();
        public MaterialEditor GetMaterialEditor(UnityEngine.Object[] targets)
        {
            string key = string.Join(",", targets.Select(t => t.name));
            if(s_editorCache.ContainsKey(key))
            {
                return s_editorCache[key];
            }
            MaterialEditor editor = MaterialEditor.CreateEditor(targets) as MaterialEditor;
            s_editorCache.Add(key, editor);
            return editor;
        }

        public void ApplySuggestedTranslationDefinition()
        {
            if(SuggestedTranslationDefinition != null)
            {
                SuggestedTranslationDefinition.Apply(this);
                SuggestedTranslationDefinition = null;
            }
        }

        //-------------Init functions--------------------

        public static string SplitOptionsFromDisplayName(ref string displayName)
        {
            int index = displayName.IndexOf(EXTRA_OPTIONS_PREFIX, StringComparison.Ordinal);
            if(index == -1) return null;
            string options = displayName.Substring(index + EXTRA_OPTIONS_PREFIX.Length);
            displayName = displayName.Substring(0, index);
            return options;
        }

        public static string GetMaterialPropertyDisplayNameWithoutOptions(MaterialProperty prop)
        {
            string displayName = prop.displayName;
            int index = displayName.IndexOf(EXTRA_OPTIONS_PREFIX, StringComparison.Ordinal);
            if(index == -1) return displayName;
            return displayName.Substring(0, index);
        }

        private enum ThryPropertyType
        {
            none, property, master_label, footer, header, header_end, header_start, group_start, group_end, section_start, section_end, instancing, dsgi, lightmap_flags, locale, on_swap_to, space, shader_version, optimizer, in_shader_presets
        }

        private ThryPropertyType GetPropertyType(MaterialProperty p)
        {
            string name = p.name;
            MaterialProperty.PropFlags flags = p.flags;

            if (flags == MaterialProperty.PropFlags.HideInInspector)
            {
                if (name == PROPERTY_NAME_MASTER_LABEL)
                    return ThryPropertyType.master_label;
                if (name == PROPERTY_NAME_ON_SWAP_TO_ACTIONS)
                    return ThryPropertyType.on_swap_to;
                if (name == PROPERTY_NAME_SHADER_VERSION)
                    return ThryPropertyType.shader_version;

                if (name.StartsWith("m_start", StringComparison.Ordinal))
                    return ThryPropertyType.header_start;
                if (name.StartsWith("m_end", StringComparison.Ordinal))
                    return ThryPropertyType.header_end;
                if (name.StartsWith("m_", StringComparison.Ordinal))
                    return ThryPropertyType.header;
                if (name.StartsWith("g_start", StringComparison.Ordinal))
                    return ThryPropertyType.group_start;
                if (name.StartsWith("g_end", StringComparison.Ordinal))
                    return ThryPropertyType.group_end;
                if (name.StartsWith("s_start", StringComparison.Ordinal))
                    return ThryPropertyType.section_start;
                if (name.StartsWith("s_end", StringComparison.Ordinal))
                    return ThryPropertyType.section_end;
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
            else if (name == ShaderOptimizerPropertyName)
            {
                return ThryPropertyType.optimizer;
            }
            else if(name == PROPERTY_NAME_IN_SHADER_PRESETS)
            {
                return ThryPropertyType.in_shader_presets;
            }
            else if (flags.HasFlag(MaterialProperty.PropFlags.HideInInspector) == false)
            {
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
                string guid = locales_property.displayName;
                Locale = Localization.Load(guid);
            }else
            {
                Locale = Localization.Create();
            }
        }

        public void FakePartialInitilizationForLocaleGathering(Shader s)
        {
            Material material = new Material(s);
            Materials = new Material[] { material };
            Editor = MaterialEditor.CreateEditor(new UnityEngine.Object[] { material }) as MaterialEditor;
            Properties = MaterialEditor.GetMaterialProperties(Materials);
            RenamedPropertySuffix = ShaderOptimizer.GetRenamedPropertySuffix(Materials[0]);
            HasCustomRenameSuffix = ShaderOptimizer.HasCustomRenameSuffix(Materials[0]);
            ShaderEditor.Active = this;
            CollectAllProperties();
            UnityEngine.Object.DestroyImmediate(Editor);
            UnityEngine.Object.DestroyImmediate(material);
        }

        //finds all properties and headers and stores them in correct order
        private void CollectAllProperties()
        {
            if (ShaderOptimizer.IsShaderUsingThryOptimizer(Shader))
            {
                ShaderOptimizerPropertyName = ShaderOptimizer.GetOptimizerPropertyName(Shader);
            }

            //load display names from file if it exists
            MaterialProperty[] props = Properties;
            LoadLocales();

            PropertyDictionary = new Dictionary<string, ShaderProperty>();
            ShaderParts = new List<ShaderPart>();
            _mainGroup = new ShaderGroup(this); //init top object that all Shader Objects are childs of
            Stack<ShaderGroup> groupStack = new Stack<ShaderGroup>(); //header stack. used to keep track if editorData header to parent new objects to
            groupStack.Push(_mainGroup); //add top object as top object to stack
            groupStack.Push(_mainGroup); //add top object a second time, because it get's popped with first actual header item
            _footers = new List<FooterButton>(); //init footer list
            int offsetDepthCount = 0;

            HashSet<string> duplicatePropertiesSearch = new HashSet<string>(); // for debugging
            List<string> duplicateProperties = new List<string>(); // for debugging

            for (int i = 0; i < props.Length; i++)
            {
                string displayName = props[i].displayName;

                //extract json data from display name
                string optionsRaw = SplitOptionsFromDisplayName(ref displayName);

                displayName = Locale.Get(props[i], displayName);

                int offset = offsetDepthCount;
                
                // Duplicate property name check
                if(duplicatePropertiesSearch.Add(props[i].name) == false)
                    duplicateProperties.Add(props[i].name);

                ThryPropertyType type = GetPropertyType(props[i]);
                ShaderProperty NewProperty = null;
                ShaderPart newPart = null;
                // -- Group logic --
                // Change offset if needed
                if(type == ThryPropertyType.header_start)
                    offset = ++offsetDepthCount;
                if(type == ThryPropertyType.header_end)
                    offsetDepthCount--;
                // Create new group if needed
                switch(type)
                {
                    case ThryPropertyType.group_start:
                        newPart = new ShaderGroup(this, props[i], Editor, displayName, offset, optionsRaw, i);
                        break;
                    case ThryPropertyType.section_start:
                        newPart = new ShaderSection(this, props[i], Editor, displayName, offset, optionsRaw, i);
                        break;
                    case ThryPropertyType.header:
                    case ThryPropertyType.header_start:
                        newPart = new ShaderHeader(this, props[i], Editor, displayName, offset, optionsRaw, i);
                        break;
                }
                // pop if needed
                if(type == ThryPropertyType.header || type == ThryPropertyType.header_end || type == ThryPropertyType.group_end || type == ThryPropertyType.section_end)
                {
                    groupStack.Pop();
                }
                // push if needed
                if(newPart != null)
                {
                    groupStack.Peek().AddPart(newPart);
                    groupStack.Push(newPart as ShaderGroup);
                }
                
                switch (type)
                {
                    case ThryPropertyType.on_swap_to:
                        _onSwapToActions = PropertyOptions.Deserialize(optionsRaw).actions;
                        break;
                    case ThryPropertyType.master_label:
                        _shaderHeader = new ShaderHeaderProperty(this, props[i], displayName, 0, optionsRaw, false, i);
                        break;
                    case ThryPropertyType.footer:
                        _footers.Add(new FooterButton(Parser.Deserialize<ButtonData>(displayName)));
                        break;
                    case ThryPropertyType.none:
                    case ThryPropertyType.property:
                        if (props[i].type == MaterialProperty.PropType.Texture)
                            NewProperty = new ShaderTextureProperty(this, props[i], displayName, offset, optionsRaw, props[i].flags.HasFlag(MaterialProperty.PropFlags.NoScaleOffset) == false, false, i);
                        else
                            NewProperty = new ShaderProperty(this, props[i], displayName, offset, optionsRaw, false, i);
                        break;
                    case ThryPropertyType.lightmap_flags:
                        NewProperty = new GIProperty(this, props[i], displayName, offset, optionsRaw, false, i);
                        break;
                    case ThryPropertyType.dsgi:
                        NewProperty = new DSGIProperty(this, props[i], displayName, offset, optionsRaw, false, i);
                        break;
                    case ThryPropertyType.instancing:
                        NewProperty = new InstancingProperty(this, props[i], displayName, offset, optionsRaw, false, i);
                        break;
                    case ThryPropertyType.locale:
                        LocaleProperty = new LocaleProperty(this, props[i], displayName, offset, optionsRaw, false, i);
                        break;
                    case ThryPropertyType.shader_version:
                        PropertyOptions options = PropertyOptions.Deserialize(optionsRaw);
                        _shaderVersionRemote = new ThryEditor.Version(WebHelper.GetCachedString(options.remote_version_url));
                        _shaderVersionLocal = new ThryEditor.Version(displayName);
                        _isShaderUpToDate = _shaderVersionLocal >= _shaderVersionRemote;
                        _shaderUpdateUrl = options.generic_string;
                        _hasShaderUpdateUrl = _shaderUpdateUrl != null;
                        break;
                    case ThryPropertyType.optimizer:
                        ShaderOptimizerProperty = new ShaderProperty(this, props[i], displayName, offset, optionsRaw, false, i);
                        ShaderOptimizerProperty.SetIsExemptFromLockedDisabling(true);
                        break;
                    case ThryPropertyType.in_shader_presets:
                        InShaderPresetsProperty = new ShaderProperty(this, props[i], displayName, offset, optionsRaw, false, i);
                        break;
                }
                if (NewProperty != null)
                {
                    newPart = NewProperty;
                    if (type != ThryPropertyType.none)
                    {
                        groupStack.Peek().AddPart(NewProperty);
                    }
                }
                if (newPart != null)
                {
#if UNITY_2022_1_OR_NEWER // Unity 2019 needs to check if key exists before adding? (Information from pumkin did not check)
                    PropertyDictionary.TryAdd(props[i].name, NewProperty);
#else
                    if(!PropertyDictionary.ContainsKey(props[i].name))
                        PropertyDictionary.Add(props[i].name, NewProperty);
#endif
                    ShaderParts.Add(newPart);
                }
            }

            if(duplicateProperties.Count > 0 && Config.Instance.enableDeveloperMode)
                _duplicatePropertyNamesString = string.Join("\n ", duplicateProperties.ToArray());
        }

        //-------------Draw Functions----------------

        public void InitlizeThryUI()
        {
            Config config = Config.Instance;
            Active = this;
            Helper.RegisterEditorUse();
            RegisterCallacks();

            //get material targets
            Materials = Editor.targets.Select(o => o as Material).ToArray();

            SetShader(Materials[0].shader, LastShader);

            RenamedPropertySuffix = ShaderOptimizer.GetRenamedPropertySuffix(Materials[0]);
            HasCustomRenameSuffix = ShaderOptimizer.HasCustomRenameSuffix(Materials[0]);

            IsPresetEditor = Materials.Length == 1 && Presets.ArePreset(Materials);

            //collect shader properties
            CollectAllProperties();

            _renderQueueProperty = new RenderQueueProperty(this);
            _vRCFallbackProperty = new VRCFallbackProperty(this);
            ShaderParts.Add(_renderQueueProperty);
            ShaderParts.Add(_vRCFallbackProperty);

            if(Config.Instance.forceAsyncCompilationPreview)
            {
                ShaderUtil.allowAsyncCompilation = true;
            }

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

        
        private void RegisterCallacks()
        {
            if (_didRegisterCallbacks) return;
            _didRegisterCallbacks = true;
            //TODO: Handle these in Unity <2022.2
            #if UNITY_2022_2_OR_NEWER
            Undo.undoRedoEvent += UndoRedoEvent;
            #endif
        }

        private void UnregisterCallbacks()
        {
            if (_didRegisterCallbacks == false) return;
            _didRegisterCallbacks = false;
            //TODO: Handle these in Unity <2022.2
            #if UNITY_2022_2_OR_NEWER
            Undo.undoRedoEvent -= UndoRedoEvent;
            #endif
        }
        
        //TODO: Handle these in Unity <2022.2
        #if UNITY_2022_2_OR_NEWER
        private void UndoRedoEvent(in UndoRedoInfo undo)
        {
            if(Materials[0] != null && (undo.undoName.EndsWith(Materials[0].name, StringComparison.Ordinal) 
                || undo.undoName.EndsWith("Materials", StringComparison.Ordinal)))
            {
                EditorApplication.delayCall += () =>
                {
                    bool repaint = false;
                    foreach(ShaderPart part in ShaderParts)
                    {
                        repaint |= part.CheckForValueChange();
                    }
                    if(repaint)
                    {
                        Repaint();
                    }
                };   
            }
        }
        #endif

        public override void OnClosed(Material material)
        {
            base.OnClosed(material);
            UnregisterCallbacks();
            _isFirstOnGUICall = true;
        }

        #if UNITY_2021_2_OR_NEWER
        public override void ValidateMaterial(Material material)
        {
            base.ValidateMaterial(material);
            if(Undo.GetCurrentGroupName() == "Reset Material")
            {
                if(Active != null && Active.Materials[0] == material)
                    Active.OnReset();
            }
        }
        #endif

        private void OnReset()
        {
            ShaderOptimizer.DeleteTags(Materials);
            foreach(Material m in Materials)
                MaterialLinker.UnlinkAll(m);
            _vRCFallbackProperty?.SetPropertyValue((string)_vRCFallbackProperty.PropertyDefaultValue);
            _doReloadNextDraw = true;
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            this.ShaderOptimizerProperty = null;
            this.LocaleProperty = null;
            this.InShaderPresetsProperty = null;
            //Unity sets the render queue to the shader defult when changing shader
            //This seems to be some deeper process that cant be disabled so i just set it again after the swap
            //Even material.shader = newShader resets the queue. (this is actually the only thing the base function does)
            int previousQueue = material.renderQueue;
            base.AssignNewShaderToMaterial(material, oldShader, newShader);
            material.renderQueue = previousQueue;
            SuggestedTranslationDefinition = ShaderTranslator.CheckForExistingTranslationFile(oldShader, newShader);
            FixKeywords(new Material[] { material });
            _doReloadNextDraw = true;
            _didSwapToShader = true;
            SetShader(newShader, oldShader);
        }

        void InitEditorData(MaterialEditor materialEditor)
        {
            Editor = materialEditor;
            TextureArrayProperties = new List<ShaderProperty>();
            IsFirstCall = true;
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
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
            Input.Update(IsLockedMaterial);
            ActiveRenderer = Selection.activeTransform?.GetComponent<Renderer>();
            IsInAnimationMode = AnimationMode.InAnimationMode();

            Active = this;

            // Undos throw errors because the structure of the UI changes between layout and repaint
            // This is a workaround to prevent the error by exiting the GUI call early
            if((Event.current.type == EventType.ExecuteCommand || Event.current.type == EventType.ValidateCommand)
                && Event.current.commandName == "UndoRedoPerformed")
            {
                _doReloadNextDraw = true;
                GUIUtility.ExitGUI();
                return;
            }
            Draw();
            HandleEvents();
            _didSwapToShader = false;
        }

        void Draw()
        {
            
            IsDrawing = true;
#if UNITY_2022_1_OR_NEWER
            EditorGUI.indentLevel -= 2;
#endif

            DoVariantWarning();
            GUIManualReloadButton();
            GUIDevloperMode();
            GUIShaderVersioning();

            GUILayout.Space(5);
            GUITopBar();
            GUILayout.Space(5);
            GUISearchBar();
            GUILockinButton();
            GUIPresetsBar();

            Presets.PresetEditorGUI(this);
            ShaderTranslator.SuggestedTranslationButtonGUI(this);

#if UNITY_2022_1_OR_NEWER
            EditorGUI.indentLevel += 2;
#endif

            //PROPERTIES
            using ( new DetourMaterialPropertyVariantIcon())
            {
                foreach (ShaderPart part in _mainGroup.Children)
                {
                    part.Draw();
                }
            }

            //Render Queue selection
            if(VRCInterface.IsVRCSDKInstalled()) _vRCFallbackProperty.Draw();
            if (Config.Instance.showRenderQueue) _renderQueueProperty.Draw();

            BetterTooltips.DrawActive();

            GUIFooters();
            IsDrawing = false;
        }

        private void GUIManualReloadButton()
        {
            if (Config.Instance.showManualReloadButton)
            {
                if(GUILayout.Button("Manual Reload"))
                {
                    this.Reload();
                }
            }
        }

        private void GUIDevloperMode()
        {
            if (Config.Instance.enableDeveloperMode)
            {
                // Show duplicate property names
                if(_duplicatePropertyNamesString != null)
                {
                    EditorGUILayout.HelpBox("Duplicate Property Names:\n" + _duplicatePropertyNamesString, MessageType.Warning);
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
            if(_shaderHeader != null && drawAboveToolbar) _shaderHeader.Draw(EditorGUILayout.GetControlRect());

            Rect topBarRect = RectifiedLayout.GetRect(25);
            Rect iconRect = new Rect(topBarRect.x, topBarRect.y, 25, 25);
            if(GUILib.ButtonWithCursor(iconRect, Icons.settings, "Settings"))
            {
                EditorWindow.GetWindow<Settings>(false, "Thry Settings", true);
            }
            iconRect.x += 25;
            if (GUILib.ButtonWithCursor(iconRect, Icons.tools, "Tools"))
            {
                PopupTools(iconRect);
            }
            iconRect.x += 25;
            ShaderTranslator.TranslationSelectionGUI(iconRect, this);
            iconRect.x += 25;
            if (GUILib.ButtonWithCursor(iconRect, Icons.thryIcon, "Thryrallo"))
                Application.OpenURL("https://www.twitter.com/thryrallo");

            Rect headerRect = new Rect(topBarRect);
            headerRect.x = iconRect.x + 25;
            headerRect.width = topBarRect.width - headerRect.x;

            if (LocaleProperty != null)
            {
                Rect localeRect = new Rect(topBarRect);
                localeRect.width = 100;
                localeRect.x = topBarRect.width - 100;
                LocaleProperty.Draw(localeRect);
                headerRect.width -= localeRect.width;
            }
            

            //draw master label text after ui elements, so it can be positioned between
            if (_shaderHeader != null && !drawAboveToolbar) _shaderHeader.Draw(headerRect);
        }

        private void GUILockinButton()
        {
            if(ShaderOptimizerProperty != null)
                ShaderOptimizerProperty.Draw();
        }

        private void GUIPresetsBar()
        {
            Rect barRect = RectifiedLayout.GetRect(25);

            Rect inShaderRect = new Rect(barRect);
            inShaderRect.width /= 3;
            inShaderRect.x = barRect.width - inShaderRect.width;

            Rect presetsRect = new Rect(barRect);
            presetsRect.width = inShaderRect.width;
            presetsRect.height = 18;

            Rect presetsIcon = new Rect(presetsRect);
            presetsIcon.width = 18;
            presetsIcon.height = 18;
            presetsIcon.x = presetsRect.width - 20;

            if (GUI.Button(presetsRect, "Presets") | GUILib.Button(presetsIcon, Icons.presets))
                Presets.OpenPresetsMenu(barRect, this, false);
            ThryWideEnumDrawer.RenderLabel = false;
            if (InShaderPresetsProperty!= null)
                InShaderPresetsProperty.Draw(inShaderRect);
            ThryWideEnumDrawer.RenderLabel = true;
        }

        private void GUISearchBar()
        {
            _enteredSearchTerm = EditorGUILayout.TextField(_enteredSearchTerm, EditorStyles.toolbarSearchField);
            if(_enteredSearchTerm != _appliedSearchTerm)
            {
                _appliedSearchTerm = _enteredSearchTerm;
                UpdateSearch();
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
            if (GUILayout.Button("@UI Made by Thryrallo", Styles.madeByLabel))
                Application.OpenURL("https://www.twitter.com/thryrallo");
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
        }

        private void DoVariantWarning()
        {
#if UNITY_2022_1_OR_NEWER
            if(Materials[0].isVariant)
            {
                EditorGUILayout.HelpBox("This material is a variant, which isn't supported by Poiyomi at this time.\nThe material cannot be locked or uploaded to VRChat. To continue using this material, clear the Parent box above.", MessageType.Warning);
            }
#endif
        }

        private void PopupTools(Rect position)
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Fix Keywords"), false, delegate ()
            {
                FixKeywords(Materials);
            });
            menu.AddSeparator("");

            int unboundTextures = MaterialCleaner.CountUnusedProperties(MaterialCleaner.CleanPropertyType.Texture, Materials);
            int unboundProperties = MaterialCleaner.CountAllUnusedProperties(Materials);
            List<string> unusedTextures = new List<string>();
            _mainGroup.FindUnusedTextures(unusedTextures, true);
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
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Debug/Screenshot Material Inspector"),false, () =>
            {
                InspectorCapture.CaptureActiveInspector(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            });
            menu.AddItem(new GUIContent("Debug/Copy Non-Default Material Settings"), false, () =>
            {
                string materialString = MaterialToDebugString.ConvertMaterialToDebugString(this, true);
                if(string.IsNullOrEmpty(materialString))
                {
                    Debug.LogError($"Failed to copy material settings to clipboard");
                    return;
                }

                EditorGUIUtility.systemCopyBuffer = materialString;
                Debug.Log($"Copied material settings to clipboard:\n{materialString}");
            });
            menu.AddItem(new GUIContent("Debug/Copy All Material Settings"), false, () =>
            {
                string materialString = MaterialToDebugString.ConvertMaterialToDebugString(this, false); 
                if(string.IsNullOrEmpty(materialString))
                {
                    Debug.LogError($"Failed to copy material settings to clipboard");
                    return;
                }

                EditorGUIUtility.systemCopyBuffer = materialString;
                Debug.Log($"Copied material settings to clipboard:\n{materialString}");
            });
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Is Preset"), Presets.IsPreset(Materials[0]), delegate ()
            {
                Presets.SetPreset(Materials, !Presets.IsPreset(Materials[0]));
                this.Reload();
            });
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

            //on swap
            if (_onSwapToActions != null && _didSwapToShader)
            {
                foreach (DefineableAction a in _onSwapToActions)
                    a.Perform(Materials);
                _onSwapToActions = null;
            }

            if (Input.HadMouseDownRepaint) Input.HadMouseDown = false;
            Input.HadMouseDownRepaint = false;
            IsFirstCall = false;
            materialPropertyDictionary = null;
        }

        public void SetSearchTerm(string term)
        {
            _enteredSearchTerm = term;
            Editor.Repaint();
            GUIUtility.ExitGUI();
        }

        //iterate the same way drawing would iterate
        //if display part, display all parents parts
        List<ShaderGroup> _foundGroups = new List<ShaderGroup>();
        private void UpdateSearch()
        {
            IsInSearchMode = _enteredSearchTerm.Length > 0;
            foreach (ShaderGroup g in _foundGroups)
                g.SetSearchExpanded(false);
            _foundGroups.Clear();
            _mainGroup.Search(_enteredSearchTerm, _foundGroups);
            for(int i = 1; i < 11 && i <= _foundGroups.Count; i++)
            {
                _foundGroups[_foundGroups.Count - i].SetSearchExpanded(true);
            }
        }

        private void ClearSearch()
        {
            _enteredSearchTerm = "";
            UpdateSearch();
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
            this._doReloadNextDraw = true;
            // this.Repaint();
            ThryWideEnumDrawer.Reload();
            ThryRGBAPackerDrawer.Reload();
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

        // Cache property->keyword lookup for performance
        static Dictionary<Shader, List<(string prop, List<string> keywords)>> PropertyKeywordsByShader = new Dictionary<Shader, List<(string prop, List<string> keywords)>>();

        /// <summary> Iterate through all materials to ensure keywords list matches properties. </summary>
        public static void FixKeywords(IEnumerable<Material> materialsToFix)
        {
            // Process Shaders
            IEnumerable<Material> uniqueShadersMaterials = materialsToFix.GroupBy(m => m.shader).Select(g => g.First());
            IEnumerable<Shader> shadersWithThryEditor = uniqueShadersMaterials.Where(m => ShaderHelper.IsShaderUsingThryEditor(m)).Select(m => m.shader);

            // Clear cache every time if in developer mode, so that changes aren't missed
            if(Config.Instance.enableDeveloperMode)
                PropertyKeywordsByShader.Clear();

            float f = 0;
            int count = shadersWithThryEditor.Count();

            if(count > 1) EditorUtility.DisplayProgressBar("Validating Keywords", "Processing Shaders", 0);

            foreach (Shader s in shadersWithThryEditor)
            {
                if(count > 1) EditorUtility.DisplayProgressBar("Validating Keywords", $"Processing Shader: {s.name}", f++ / count);
                if(!PropertyKeywordsByShader.ContainsKey(s))
                    PropertyKeywordsByShader[s] = ShaderHelper.GetPropertyKeywordsForShader(s);
            }
            // Find Materials
            IEnumerable<Material> materials = materialsToFix.Where(m => PropertyKeywordsByShader.ContainsKey(m.shader));
            f = 0;
            count = materials.Count();

            // Set Keywords
            foreach(Material m in materials)
            {
                if(count > 1) EditorUtility.DisplayProgressBar("Validating Keywords", $"Validating Material: {m.name}", f++ / count);

                List<string> keywordsInMaterial = m.shaderKeywords.ToList();

                foreach((string prop, List<string> keywords) in PropertyKeywordsByShader[m.shader])
                {
                    switch(keywords.Count)
                    {
                        case 0:
                            break;
                        case 1:
                            string keyword = keywords[0];
                            keywordsInMaterial.Remove(keyword);

                            if(m.GetFloat(prop) == 1)
                                m.EnableKeyword(keyword);
                            else
                                m.DisableKeyword(keyword);
                            break;
                        default: // KeywordEnum
                            for (int i = 0; i < keywords.Count; i++)
                            {
                                keywordsInMaterial.Remove(keywords[i]);
                                if (m.GetFloat(prop) == i)
                                    m.EnableKeyword(keywords[i]);
                                else
                                    m.DisableKeyword(keywords[i]);
                            }
                            break;
                    }
                }

                // Disable any remaining keywords
                foreach(string keyword in keywordsInMaterial)
                    m.DisableKeyword(keyword);
            }
            if(count > 1) EditorUtility.ClearProgressBar();
        }

        /// <summary> Iterate through all materials with FixKeywords. </summary>
        [MenuItem("Thry/Shader Tools/Fix Keywords for All Materials (Slow)", priority = -20)]
        static void FixAllKeywords()
        {
            IEnumerable<Material> materialsToFix = AssetDatabase.FindAssets("t:material")
                .Select(g => AssetDatabase.GUIDToAssetPath(g))
                .Where(p => string.IsNullOrEmpty(p) == false)
                .Select(p => AssetDatabase.LoadAssetAtPath<Material>(p))
                .Where(m => m != null && m.shader != null)
                .Where(m => ShaderOptimizer.IsMaterialLocked(m) == false);

            FixKeywords(materialsToFix);
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

        [MenuItem("Thry/Shader Optimizer/Materials List", priority = 0)]
        static void MenuShaderOptUnlockedMaterials()
        {
            EditorWindow.GetWindow<UnlockedMaterialsList>(false, "Materials", true);
        }

        [MenuItem("Assets/Thry/Materials/Cleaner/List Unbound Properties", priority = 303)]
        static void AssetsCleanMaterials_ListUnboundProperties()
        {
            IEnumerable<Material> materials = Selection.objects.Where(o => o is Material).Select(o => o as Material);
            foreach (Material m in materials)
            {
                Debug.Log("_______Unbound Properties for " + m.name + "_______");
                MaterialCleaner.ListUnusedProperties(MaterialCleaner.CleanPropertyType.Texture, m);
                MaterialCleaner.ListUnusedProperties(MaterialCleaner.CleanPropertyType.Color, m);
                MaterialCleaner.ListUnusedProperties(MaterialCleaner.CleanPropertyType.Float, m);
            }
        }

        [MenuItem("Assets/Thry/Materials/Cleaner/Remove Unbound Textures", priority = 303)]
        static void AssetsCleanMaterials_CleanUnboundTextures()
        {
            IEnumerable<Material> materials = Selection.objects.Where(o => o is Material).Select(o => o as Material);
            foreach (Material m in materials)
            {
                Debug.Log("_______Removing Unbound Textures for " + m.name + "_______");
                MaterialCleaner.RemoveAllUnusedProperties(MaterialCleaner.CleanPropertyType.Texture, m);
            }
        }
    }
}
