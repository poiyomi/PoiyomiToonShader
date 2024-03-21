// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    #region Texture Drawers
    public class ThryTextureDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            GuiHelper.ConfigTextureProperty(position, prop, label, editor, ((TextureProperty)ShaderEditor.Active.CurrentProperty).hasScaleOffset);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

    public class SmallTextureDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            GuiHelper.SmallTextureProperty(position, prop, label, editor, ((TextureProperty)ShaderEditor.Active.CurrentProperty).hasScaleOffset);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

    // For backwards compatibility
    public class BigTextureDrawer : SimpleLargeTextureDrawer
    {
        
    }
    
    // For backwards compatibility
    public class StylizedBigTextureDrawer : StylizedLargeTextureDrawer
    {
        
    }

    public class SimpleLargeTextureDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            GuiHelper.BigTexturePropertyBasic(position, prop, label, editor, ((TextureProperty)ShaderEditor.Active.CurrentProperty).hasScaleOffset);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

    public class StylizedLargeTextureDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            GuiHelper.StylizedBigTextureProperty(position, prop, label, editor, ((TextureProperty)ShaderEditor.Active.CurrentProperty).hasScaleOffset);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }
    #endregion

    #region Special Texture Drawers
    public class CurveDrawer : MaterialPropertyDrawer
    {
        public AnimationCurve curve;
        public EditorWindow window;
        public Texture2D texture;
        public bool saved = true;
        public TextureData imageData;

        public CurveDrawer()
        {
            curve = new AnimationCurve();
        }

        private void Init()
        {
            if (imageData == null)
            {
                if (ShaderEditor.Active.CurrentProperty.Options.texture == null)
                    imageData = new TextureData();
                else
                    imageData = ShaderEditor.Active.CurrentProperty.Options.texture;
            }
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            Init();
            Rect border_position = new Rect(position.x + EditorGUIUtility.labelWidth - 15, position.y, position.width - EditorGUIUtility.labelWidth + 15 - GuiHelper.GetSmallTextureVRAMWidth(prop), position.height);

            EditorGUI.BeginChangeCheck();
            curve = EditorGUI.CurveField(border_position, curve);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateCurveTexture(prop);
            }

            GuiHelper.SmallTextureProperty(position, prop, label, editor, DrawingData.CurrentTextureProperty.hasFoldoutProperties);

            CheckWindowForCurveEditor();

            if (window == null && !saved)
                Save(prop);
        }

        private void UpdateCurveTexture(MaterialProperty prop)
        {
            texture = Converter.CurveToTexture(curve, imageData);
            prop.textureValue = texture;
            saved = false;
        }

        private void CheckWindowForCurveEditor()
        {
            string windowName = "";
            if (EditorWindow.focusedWindow != null)
                windowName = EditorWindow.focusedWindow.titleContent.text;
            bool isCurveEditor = windowName == "Curve";
            if (isCurveEditor)
                window = EditorWindow.focusedWindow;
        }

        private void Save(MaterialProperty prop)
        {
            Debug.Log(prop.textureValue.ToString());
            Texture saved_texture = TextureHelper.SaveTextureAsPNG(texture, PATH.TEXTURES_DIR + "curves/" + curve.GetHashCode() + ".png", null);
            prop.textureValue = saved_texture;
            saved = true;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }
    public class ThryExternalTextureToolDrawer : MaterialPropertyDrawer
    {
        string _toolTypeName;
        string _toolHeader;

        Type t_ExternalToolType;
        MethodInfo _onGui;
        object _externalTool;
        MaterialProperty _prop;

        bool _isTypeLoaded;
        bool _doesExternalTypeExist;
        bool _isInit;
        bool _showTool;

        public ThryExternalTextureToolDrawer(string toolHeader, string toolTypeName)
        {
            this._toolTypeName = toolTypeName;
            this._toolHeader = toolHeader;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            LoadType();
            if (_doesExternalTypeExist)
            {
                _prop = prop;
                GuiHelper.SmallTextureProperty(position, prop, label, editor, DrawingData.CurrentTextureProperty.hasFoldoutProperties, ExternalGUI);
            }
            else
            {
                GuiHelper.SmallTextureProperty(position, prop, label, editor, DrawingData.CurrentTextureProperty.hasFoldoutProperties);
            }
        }

        void ExternalGUI()
        {
            if (GUI.Button(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect()), _toolHeader)) _showTool = !_showTool;
            if (_showTool)
            {
                Init();

                int indent = EditorGUI.indentLevel;
                GuiHelper.BeginCustomIndentLevel(0);
                GUILayout.BeginHorizontal();
                GUILayout.Space(indent * 15);
                GUILayout.BeginVertical();
                _onGui.Invoke(_externalTool, new object[0]);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GuiHelper.EndCustomIndentLevel();
            }
        }

        public void LoadType()
        {
            if (_isTypeLoaded) return;
            t_ExternalToolType = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(_toolTypeName)).Where(t => t != null).FirstOrDefault();
            _doesExternalTypeExist = t_ExternalToolType != null;
            _isTypeLoaded = true;
        }

        public void Init()
        {
            if (_isInit) return;
            if (_isTypeLoaded && _doesExternalTypeExist)
            {
                _onGui = t_ExternalToolType.GetMethod("OnGUI", BindingFlags.NonPublic | BindingFlags.Instance);
                _externalTool = ScriptableObject.CreateInstance(t_ExternalToolType);
                EventInfo eventTextureGenerated = t_ExternalToolType.GetEvent("TextureGenerated");
                if (eventTextureGenerated != null)
                    eventTextureGenerated.AddEventHandler(_externalTool, new EventHandler(TextureGenerated));
            }
            _isInit = true;
        }

        void TextureGenerated(object sender, EventArgs args)
        {
            if (args != null && args.GetType().GetField("generated_texture") != null)
            {
                Texture2D generated = args.GetType().GetField("generated_texture").GetValue(args) as Texture2D;
                _prop.textureValue = generated;
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

    public class ThryRGBAPackerDrawer : MaterialPropertyDrawer
    {
        // TODO : Load lacale by property name in the future: propname_r, propname_g, propname_b, propname_a
        class ThryRGBAPackerData
        {
            public Texture _previousTexture;
            public Texture2D _packedTexture;

            public InlinePackerChannelConfig _input_r;
            public InlinePackerChannelConfig _input_g;
            public InlinePackerChannelConfig _input_b;
            public InlinePackerChannelConfig _input_a;

            public bool _isInit;
            public bool _hasConfigChanged;
            public bool _hasTextureChanged;
            public long _lastConfirmTime;

            public TexturePacker.Connection[] _connections;
            public TexturePacker.TextureSource[] _sources;
        }

        Dictionary<UnityEngine.Object, ThryRGBAPackerData> materialPackerData = new Dictionary<UnityEngine.Object, ThryRGBAPackerData>();

        MaterialProperty _prop;
        ThryRGBAPackerData _current;

        string _label1;
        string _label2;
        string _label3;
        string _label4;
        bool _firstTextureIsRGB;
        ColorSpace _colorSpace = ColorSpace.Gamma;
        bool _alphaIsTransparency = true;

        // for locale changing
        // i tried using an array to save the default labels, but the data just got lost somewhere. not sure why
        string _defaultLabel1;
        string _defaultLabel2;
        string _defaultLabel3;
        string _defaultLabel4;
        int _reloadCount = -1;
        static int _reloadCountStatic;

        public static void Reload()
        {
            _reloadCountStatic++;
        }

        void LoadLabels()
        {
            if (_reloadCount == _reloadCountStatic) return;
            // using the string itself as a key for reuse in other places. this might cause issues, if it does in the future 
            // we can add the class name as a prefix to the key
            _label1 = ShaderEditor.Active.Locale.Get(_defaultLabel1, _defaultLabel1);
            _label2 = ShaderEditor.Active.Locale.Get(_defaultLabel2, _defaultLabel2);
            _label3 = ShaderEditor.Active.Locale.Get(_defaultLabel3, _defaultLabel3);
            _label4 = ShaderEditor.Active.Locale.Get(_defaultLabel4, _defaultLabel4);
            _reloadCount = _reloadCountStatic;
        }

        // end locale changing

        public ThryRGBAPackerDrawer(string label1, string label2, string label3, string label4, string colorspace, string alphaIsTransparency)
        {
            _label1 = string.IsNullOrWhiteSpace(label1) ? null : label1;
            _label2 = string.IsNullOrWhiteSpace(label2) ? null : label2;
            _label3 = string.IsNullOrWhiteSpace(label3) ? null : label3;
            _label4 = string.IsNullOrWhiteSpace(label4) ? null : label4;
            _defaultLabel1 = _label1;
            _defaultLabel2 = _label2;
            _defaultLabel3 = _label3;
            _defaultLabel4 = _label4;
            _colorSpace = (colorspace == "linear" || colorspace == "Linear") ? ColorSpace.Linear : ColorSpace.Gamma;
            _alphaIsTransparency = alphaIsTransparency == "true" || alphaIsTransparency == "True";
        }

        public ThryRGBAPackerDrawer(string label1, string label2, string colorspace, string alphaIsTransparency) : this(label1, label2, null, null, colorspace, alphaIsTransparency)
        {
            _firstTextureIsRGB = true;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            _prop = prop;
            if (materialPackerData.ContainsKey(_prop.targets[0]) == false) materialPackerData[_prop.targets[0]] = new ThryRGBAPackerData();
            _current = materialPackerData[_prop.targets[0]];
            GuiHelper.SmallTextureProperty(position, prop, label, editor, true, TexturePackerGUI);
            if (_prop.textureValue != _current._packedTexture) _current._previousTexture = _prop.textureValue;
        }

        bool DidTextureGetEdit(InlinePackerChannelConfig data)
        {
            if (data.TextureSource.Texture == null) return false;
            string path = AssetDatabase.GetAssetPath(data.TextureSource.Texture);
            if (System.IO.File.Exists(path) == false) return false;
            long lastEditTime = Helper.DatetimeToUnixSeconds(System.IO.File.GetLastWriteTime(path));
            bool hasBeenEdited = lastEditTime > _current._lastConfirmTime && lastEditTime != data.TextureSource.LastHandledTextureEditTime;
            data.TextureSource.LastHandledTextureEditTime = lastEditTime;
            if (hasBeenEdited) TexturePacker.TextureSource.SetUncompressedTextureDirty(data.TextureSource.Texture);
            return hasBeenEdited;
        }

        void TexturePackerGUI()
        {
            Init();
            LoadLabels();
            EditorGUI.BeginChangeCheck();
            _current._input_r = TexturePackerSlotGUI(_current._input_r, _label1);
            _current._input_g = TexturePackerSlotGUI(_current._input_g, _label2);
            if (_label3 != null) _current._input_b = TexturePackerSlotGUI(_current._input_b, _label3);
            if (_label4 != null) _current._input_a = TexturePackerSlotGUI(_current._input_a, _label4);
            bool changeCheck = EditorGUI.EndChangeCheck();
            changeCheck |= DidTextureGetEdit(_current._input_r);
            changeCheck |= DidTextureGetEdit(_current._input_g);
            changeCheck |= DidTextureGetEdit(_current._input_b);
            changeCheck |= DidTextureGetEdit(_current._input_a);
            if (changeCheck)
            {
                _current._hasConfigChanged = true;
                Save();
                Pack();
            }

            Rect buttonRect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
            buttonRect.width /= 3;
            EditorGUI.BeginDisabledGroup(!_current._hasConfigChanged);
            if (GUI.Button(buttonRect, "Confirm Merge")) Confirm();
            buttonRect.x += buttonRect.width;
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(!_current._hasTextureChanged);
            if (GUI.Button(buttonRect, "Revert")) Revert();
            EditorGUI.EndDisabledGroup();
            buttonRect.x += buttonRect.width;
            if (GUI.Button(buttonRect, "Advanced")) OpenFullTexturePacker();
        }

        InlinePackerChannelConfig TexturePackerSlotGUI(InlinePackerChannelConfig input, string label)
        {
            Rect totalRect = EditorGUILayout.GetControlRect(false);
            totalRect = EditorGUI.IndentedRect(totalRect);
            Rect r = totalRect;

            int ind = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float texWidth = Math.Max(50, r.width - 130 - 30) - 5;
            r.x = totalRect.x;
            r.width = 30;
            EditorGUI.BeginChangeCheck();
            Texture2D changed = EditorGUI.ObjectField(r, input.TextureSource.Texture, typeof(Texture2D), false) as Texture2D;
            if (EditorGUI.EndChangeCheck())
            {
                input.TextureSource.SetTexture(changed);
            }

            r.x += r.width + 5;
            r.width = texWidth - 5;
            EditorGUI.LabelField(r, label);

            if (input.TextureSource.Texture == null)
            {
                r.width = 70;
                r.x = totalRect.x + totalRect.width - r.width;
                input.Fallback = EditorGUI.FloatField(r, input.Fallback);

                r.width = 60;
                r.x -= r.width;
                EditorGUI.LabelField(r, "Fallback:");
            }
            else
            {
                r.width = 50;
                r.x = totalRect.x + totalRect.width - r.width;
                if (!_firstTextureIsRGB || input != _current._input_r)
                    input.Channel = (TexturePacker.TextureChannelIn)EditorGUI.EnumPopup(r, input.Channel);

                r.width = 20;
                r.x -= r.width;
                input.Invert = EditorGUI.Toggle(r, input.Invert);

                r.width = 60;
                r.x -= r.width;
                EditorGUI.LabelField(r, "Inverted:");
            }

            EditorGUI.indentLevel = ind;

            return input;
        }

        void Init()
        {
            if (_current._isInit) return;
            _current._input_r = LoadForChannel(ShaderEditor.Active.Materials[0], _prop.name, "r");
            _current._input_g = LoadForChannel(ShaderEditor.Active.Materials[0], _prop.name, "g");
            _current._input_b = LoadForChannel(ShaderEditor.Active.Materials[0], _prop.name, "b");
            _current._input_a = LoadForChannel(ShaderEditor.Active.Materials[0], _prop.name, "a");
            _current._lastConfirmTime = long.Parse(ShaderEditor.Active.Materials[0].GetTag(_prop.name + "_texPack_lastConfirmTime", false, "" + Helper.DatetimeToUnixSeconds(DateTime.Now)));
            _current._previousTexture = _prop.textureValue;
            _current._isInit = true;
        }

        void Save()
        {
            SaveForChannel(_current._input_r, _prop.name, "r");
            SaveForChannel(_current._input_g, _prop.name, "g");
            SaveForChannel(_current._input_b, _prop.name, "b");
            SaveForChannel(_current._input_a, _prop.name, "a");
            foreach (Material m in ShaderEditor.Active.Materials)
            {
                m.SetOverrideTag(_prop.name + "_texPack_lastConfirmTime", "" + _current._lastConfirmTime);
            }
        }

        void SaveForChannel(InlinePackerChannelConfig input, string id, string channel)
        {
            foreach (Material m in ShaderEditor.Active.Materials)
            {
                if (input.TextureSource.Texture != null) m.SetOverrideTag(id + "_texPack_" + channel + "_guid", AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(input.TextureSource.Texture)));
                else m.SetOverrideTag(id + "_texPack_" + channel + "_guid", "");
                m.SetOverrideTag(id + "_texPack_" + channel + "_fallback", input.Fallback.ToString());
                m.SetOverrideTag(id + "_texPack_" + channel + "_inverted", input.Invert.ToString());
                m.SetOverrideTag(id + "_texPack_" + channel + "_channel", ((int)input.Channel).ToString());
            }
        }

        InlinePackerChannelConfig LoadForChannel(Material m, string id, string channel)
        {
            InlinePackerChannelConfig packerChannelConfig = new InlinePackerChannelConfig();
            packerChannelConfig.Fallback = float.Parse(m.GetTag(id + "_texPack_" + channel + "_fallback", false, "1"));
            packerChannelConfig.Invert = bool.Parse(m.GetTag(id + "_texPack_" + channel + "_inverted", false, "false"));
            packerChannelConfig.Channel = (TexturePacker.TextureChannelIn)int.Parse(m.GetTag(id + "_texPack_" + channel + "_channel", false, "4"));
            string guid = m.GetTag(id + "_texPack_" + channel + "_guid", false, "");
            if (string.IsNullOrEmpty(guid) == false)
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                if (p != null)
                    packerChannelConfig.TextureSource.SetTexture(AssetDatabase.LoadAssetAtPath<Texture2D>(p));
            }
            return packerChannelConfig;
        }

        void Pack()
        {
            _current._packedTexture = TexturePacker.Pack(GetTextureSources(), GetOutputConfigs(), GetConnections(), GetFiltermode(), _colorSpace, alphaIsTransparency: _alphaIsTransparency);
            _prop.textureValue = _current._packedTexture;

            _current._hasTextureChanged = true;
        }

        TexturePacker.TextureSource[] GetTextureSources()
        {
            // build sources array
            return new TexturePacker.TextureSource[4]{
                _current._input_r.TextureSource, _current._input_g.TextureSource, _current._input_b.TextureSource, _current._input_a.TextureSource };
        }

        TexturePacker.OutputConfig[] GetOutputConfigs()
        {
            // Build OutputConfig Array
            TexturePacker.OutputConfig[] outputConfigs = new TexturePacker.OutputConfig[4];

            if (_firstTextureIsRGB)
            {
                outputConfigs[0] = _current._input_r.ToOutputConfig();
                outputConfigs[1] = _current._input_r.ToOutputConfig();
                outputConfigs[2] = _current._input_r.ToOutputConfig();
                outputConfigs[3] = _current._input_g.ToOutputConfig();
            }
            else
            {
                outputConfigs[0] = _current._input_r.ToOutputConfig();
                outputConfigs[1] = _current._input_g.ToOutputConfig();
                outputConfigs[2] = _current._input_b.ToOutputConfig();
                outputConfigs[3] = _current._input_a.ToOutputConfig();
            }
            return outputConfigs;
        }

        TexturePacker.Connection[] GetConnections()
        {
            // Build connections array
            TexturePacker.Connection[] connections = new TexturePacker.Connection[4];
            if (_firstTextureIsRGB)
            {
                connections[0] = TexturePacker.Connection.CreateFull(0, TexturePacker.TextureChannelIn.R, TexturePacker.TextureChannelOut.R);
                connections[1] = TexturePacker.Connection.CreateFull(0, TexturePacker.TextureChannelIn.G, TexturePacker.TextureChannelOut.G);
                connections[2] = TexturePacker.Connection.CreateFull(0, TexturePacker.TextureChannelIn.B, TexturePacker.TextureChannelOut.B);
                connections[3] = TexturePacker.Connection.CreateFull(1, _current._input_g.Channel, TexturePacker.TextureChannelOut.A);
            }
            else
            {
                connections[0] = TexturePacker.Connection.CreateFull(0, _current._input_r.Channel, TexturePacker.TextureChannelOut.R);
                connections[1] = TexturePacker.Connection.CreateFull(1, _current._input_g.Channel, TexturePacker.TextureChannelOut.G);
                connections[2] = TexturePacker.Connection.CreateFull(2, _current._input_b.Channel, TexturePacker.TextureChannelOut.B);
                connections[3] = TexturePacker.Connection.CreateFull(3, _current._input_a.Channel, TexturePacker.TextureChannelOut.A);
            }
            return connections;
        }

        void OpenFullTexturePacker()
        {
            TexturePacker packer = TexturePacker.ShowWindow();
            packer.InitilizeWithData(GetTextureSources(), GetOutputConfigs(), GetConnections(), GetFiltermode(), _colorSpace, _alphaIsTransparency);
            packer.OnChange += FullTexturePackerOnChange;
            packer.OnSave += FullTexturePackerOnSave;
        }

        void FullTexturePackerOnSave(Texture2D tex)
        {
            _current._packedTexture = tex;
            _prop.textureValue = _current._packedTexture;
            _current._hasTextureChanged = false;
        }

        void FullTexturePackerOnChange(Texture2D tex, TexturePacker.TextureSource[] sources, TexturePacker.OutputConfig[] configs, TexturePacker.Connection[] connections)
        {
            _current._input_r.TextureSource = sources[0];
            _current._input_g.TextureSource = sources[1];
            _current._input_b.TextureSource = sources[2];
            _current._input_a.TextureSource = sources[3];

            _current._input_r.FromOutputConfig(configs[0]);
            _current._input_g.FromOutputConfig(configs[1]);
            _current._input_b.FromOutputConfig(configs[2]);
            _current._input_a.FromOutputConfig(configs[3]);

            _current._input_r.Channel = connections.Length > 0 ? connections[0].FromChannel : TexturePacker.TextureChannelIn.Max;
            _current._input_g.Channel = connections.Length > 1 ? connections[1].FromChannel : TexturePacker.TextureChannelIn.Max;
            _current._input_b.Channel = connections.Length > 2 ? connections[2].FromChannel : TexturePacker.TextureChannelIn.Max;
            _current._input_a.Channel = connections.Length > 3 ? connections[3].FromChannel : TexturePacker.TextureChannelIn.Max;

            _current._packedTexture = tex;
            _prop.textureValue = _current._packedTexture;
            _current._hasTextureChanged = true;
        }

        FilterMode GetFiltermode()
        {
            if (_current._input_r.GetTexture() != null) return _current._input_r.GetTexture().filterMode;
            if (_current._input_g.GetTexture() != null) return _current._input_g.GetTexture().filterMode;
            if (_current._input_b.GetTexture() != null) return _current._input_b.GetTexture().filterMode;
            if (_current._input_a.GetTexture() != null) return _current._input_a.GetTexture().filterMode;
            return FilterMode.Bilinear;
        }

        void Confirm()
        {
            if (_current._packedTexture == null) Pack();
            string path = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(ShaderEditor.Active.Materials[0]));
            path = path + "/" + ShaderEditor.Active.Materials[0].name + _prop.name + ".png";
            _prop.textureValue = TextureHelper.SaveTextureAsPNG(_current._packedTexture, path);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.streamingMipmaps = true;
            importer.crunchedCompression = true;
            importer.sRGBTexture = _colorSpace == ColorSpace.Gamma;
            importer.filterMode = GetFiltermode();
            importer.alphaIsTransparency = _current._packedTexture.alphaIsTransparency;
            importer.SaveAndReimport();

            _current._hasConfigChanged = false;
            _current._hasTextureChanged = false;
            _current._lastConfirmTime = Helper.DatetimeToUnixSeconds(DateTime.Now);
        }

        void Revert()
        {
            _prop.textureValue = _current._previousTexture;
            _current._hasTextureChanged = false;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }

        class InlinePackerChannelConfig
        {
            public TexturePacker.TextureSource TextureSource = new TexturePacker.TextureSource();
            public bool Invert;
            public float Fallback;
            public TexturePacker.TextureChannelIn Channel = TexturePacker.TextureChannelIn.Max;

            public TexturePacker.OutputConfig ToOutputConfig()
            {
                TexturePacker.OutputConfig outputConfig = new TexturePacker.OutputConfig();
                outputConfig.BlendMode = TexturePacker.BlendMode.Add;
                outputConfig.Invert = Invert ? TexturePacker.InvertMode.Invert : TexturePacker.InvertMode.None;
                outputConfig.Fallback = Fallback;
                return outputConfig;
            }

            public void FromOutputConfig(TexturePacker.OutputConfig config)
            {
                Invert = config.Invert == TexturePacker.InvertMode.Invert;
                Fallback = config.Fallback;
            }

            public Texture2D GetTexture()
            {
                if (TextureSource == null) return null;
                return TextureSource.Texture;
            }
        }
    }

    public class GradientDrawer : MaterialPropertyDrawer
    {
        GradientData data;

        Rect border_position;
        Rect gradient_position;

        Dictionary<UnityEngine.Object, GradientData> _gradient_data = new Dictionary<UnityEngine.Object, GradientData>();

        private void Init(MaterialProperty prop, bool replace = false)
        {
            if (!replace && _gradient_data.ContainsKey(prop.targets[0]))
            {
                data = _gradient_data[prop.targets[0]];
                return;
            }
            data = new GradientData();
            data.PreviewTexture = prop.textureValue;
            _gradient_data[prop.targets[0]] = data;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            Init(prop);

            EditorGUI.BeginChangeCheck();
            if (EditorGUI.EndChangeCheck())
                Init(prop, true);

            if (Config.Singleton.default_texture_type == TextureDisplayType.small)
            {
                UpdateRects(position, prop);
                if (ShaderEditor.Input.Click && border_position.Contains(Event.current.mousePosition))
                    Open(prop);
                GuiHelper.SmallTextureProperty(position, prop, label, editor, DrawingData.CurrentTextureProperty.hasFoldoutProperties);
                GradientField();
            }
            else
            {
                position = new RectOffset(-30, 0, 0, 0).Add(position);
                Rect top_bg_rect = new Rect(position);
                Rect label_rect = new Rect(position);
                Rect button_select = new Rect(position);
                top_bg_rect = new RectOffset(0, 0, 0, 25).Add(top_bg_rect);
                label_rect = new RectOffset(-5, 5, -3, 3).Add(label_rect);
                button_select = new RectOffset((int)button_select.width - 120, 20, 2, 0).Remove(button_select);

                GUILayoutUtility.GetRect(position.width, 30); // get space for gradient
                border_position = new Rect(position.x, position.y + position.height, position.width, 30);
                border_position = new RectOffset(3, 3, 0, 0).Remove(border_position);
                gradient_position = new RectOffset(1, 1, 1, 1).Remove(border_position);
                if (ShaderEditor.Input.Click && border_position.Contains(Event.current.mousePosition))
                    Open(prop);

                GUI.DrawTexture(top_bg_rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Styles.COLOR_BACKGROUND_1, 3, 10);

                if (DrawingData.CurrentTextureProperty.hasScaleOffset || DrawingData.CurrentTextureProperty.Options.reference_properties != null)
                {
                    Rect extraPropsBackground = EditorGUILayout.BeginVertical();
                    extraPropsBackground.x = position.x;
                    extraPropsBackground.width = position.width;
                    extraPropsBackground.y = extraPropsBackground.y - 25;
                    extraPropsBackground.height = extraPropsBackground.height + 25;
                    float propertyX = extraPropsBackground.x + 15;
                    float propertyWidth = extraPropsBackground.width - 30;
                    GUI.DrawTexture(extraPropsBackground, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Styles.COLOR_BACKGROUND_1, 3, 10);
                    Rect r;
                    if (DrawingData.CurrentTextureProperty.hasScaleOffset)
                    {
                        r = GUILayoutUtility.GetRect(propertyWidth, 48);
                        r.x = propertyX;
                        r.y -= 8;
                        r.width = propertyWidth;
                        editor.TextureScaleOffsetProperty(r, prop);
                    }
                    if (DrawingData.CurrentTextureProperty.Options.reference_properties != null)
                    {
                        float labelWidth = EditorGUIUtility.labelWidth;
                        EditorGUIUtility.labelWidth = 100;
                        propertyX -= 30;
                        foreach (string pName in DrawingData.CurrentTextureProperty.Options.reference_properties)
                        {
                            ShaderProperty property = ShaderEditor.Active.PropertyDictionary[pName];
                            if (property != null)
                            {
                                r = GUILayoutUtility.GetRect(propertyWidth, editor.GetPropertyHeight(property.MaterialProperty, property.Content.text) + 3);
                                r.x = propertyX;
                                r.width = propertyWidth;
                                property.Draw(new CRect(r));
                            }
                        }
                        EditorGUIUtility.labelWidth = labelWidth;
                    }
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    GUILayoutUtility.GetRect(0, 5);
                    Rect backgroundBottom = new RectOffset(3, 3, -5, 10).Add(border_position);
                    GUI.DrawTexture(backgroundBottom, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Styles.COLOR_BACKGROUND_1, 3, 10);
                }

                bool changed = GuiHelper.HandleTexturePicker(prop);
                changed |= GuiHelper.AcceptDragAndDrop(border_position, prop);
                if (changed)
                    Init(prop, true);
                if (GUI.Button(button_select, "Select", EditorStyles.miniButton))
                {
                    GuiHelper.OpenTexturePicker(prop);
                }

                GradientField();
                GUI.Label(label_rect, label);

                GUILayoutUtility.GetRect(0, 5);
            }
        }

        private void Open(MaterialProperty prop)
        {
            ShaderEditor.Input.Use();
            PropertyOptions options = ShaderEditor.Active.CurrentProperty.Options;
            GradientEditor.Open(data, prop, options.texture, options.force_texture_options, !options.force_texture_options);
        }

        private void UpdateRects(Rect position, MaterialProperty prop)
        {
            border_position = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth - GuiHelper.GetSmallTextureVRAMWidth(prop), position.height);
            gradient_position = new Rect(border_position.x + 1, border_position.y + 1, border_position.width - 2, border_position.height - 2);
        }

        private void GradientField()
        {
            DrawBackgroundTexture();
            if (data.PreviewTexture != null)
                DrawGradientTexture();
            else
                GUI.DrawTexture(border_position, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, Color.grey, 1, 1);
        }

        private void DrawBackgroundTexture()
        {
            Texture2D backgroundTexture = TextureHelper.GetBackgroundTexture();
            Rect texCoordsRect = new Rect(0, 0, gradient_position.width / backgroundTexture.width, gradient_position.height / backgroundTexture.height);
            GUI.DrawTextureWithTexCoords(gradient_position, backgroundTexture, texCoordsRect, false);
        }

        private void DrawGradientTexture()
        {
            TextureWrapMode wrap_mode = data.PreviewTexture.wrapMode;
            data.PreviewTexture.wrapMode = TextureWrapMode.Clamp;
            bool vertical = data.PreviewTexture.height > data.PreviewTexture.width;
            Vector2 pivot = new Vector2();
            if (vertical)
            {
                pivot = new Vector2(gradient_position.x, gradient_position.y + gradient_position.height);
                GUIUtility.RotateAroundPivot(-90, pivot);
                gradient_position.y += gradient_position.height;
                float h = gradient_position.width;
                gradient_position.width = gradient_position.height;
                gradient_position.y += h;
                gradient_position.height = -h;
            }
            GUI.DrawTexture(gradient_position, data.PreviewTexture, ScaleMode.StretchToFill, true);
            if (vertical)
            {
                GUIUtility.RotateAroundPivot(90, pivot);
            }
            GUI.DrawTexture(border_position, data.PreviewTexture, ScaleMode.StretchToFill, false, 0, Color.grey, 1, 1);
            data.PreviewTexture.wrapMode = wrap_mode;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

    public class TextureArrayDrawer : MaterialPropertyDrawer
    {
        private string framesProperty;

        public TextureArrayDrawer() { }

        public TextureArrayDrawer(string framesProperty)
        {
            this.framesProperty = framesProperty;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            ShaderProperty shaderProperty = (ShaderProperty)ShaderEditor.Active.CurrentProperty;
            GuiHelper.ConfigTextureProperty(position, prop, label, editor, true, true);

            if ((ShaderEditor.Input.is_drag_drop_event) && position.Contains(ShaderEditor.Input.mouse_position))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (ShaderEditor.Input.is_drop_event)
                {
                    DragAndDrop.AcceptDrag();
                    HanldeDropEvent(prop, shaderProperty);
                }
            }
            if (ShaderEditor.Active.IsFirstCall)
                ShaderEditor.Active.TextureArrayProperties.Add(shaderProperty);
        }

        public void HanldeDropEvent(MaterialProperty prop, ShaderProperty shaderProperty)
        {
            string[] paths = DragAndDrop.paths;
            Texture2DArray tex;
            if (AssetDatabase.GetMainAssetTypeAtPath(paths[0]) != typeof(Texture2DArray))
                tex = Converter.PathsToTexture2DArray(paths);
            else
                tex = AssetDatabase.LoadAssetAtPath<Texture2DArray>(paths[0]);
            prop.textureValue = tex;
            UpdateFramesProperty(prop, shaderProperty, tex);
            EditorGUIUtility.ExitGUI();
        }

        private void UpdateFramesProperty(MaterialProperty prop, ShaderProperty shaderProperty, Texture2DArray tex)
        {
            if (framesProperty == null)
                framesProperty = shaderProperty.Options.reference_property;

            if (framesProperty != null)
            {
                if (ShaderEditor.Active.PropertyDictionary.ContainsKey(framesProperty))
                    ShaderEditor.Active.PropertyDictionary[framesProperty].MaterialProperty.SetNumber(tex.depth);
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }
    #endregion

    #region Decorators
    public class NoAnimateDecorator : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyDoesntAllowAnimation = true;
            return 0;
        }
    }

    public class ThrySeperatorDecorator : MaterialPropertyDrawer
    {
        Color _color = Styles.COLOR_FG;

        public ThrySeperatorDecorator() { }
        public ThrySeperatorDecorator(string c)
        {
            ColorUtility.TryParseHtmlString(c, out _color);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.RegisterDecorator(this);
            return 1;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            position = EditorGUI.IndentedRect(position);
            EditorGUI.DrawRect(position, _color);
        }
    }

    public class ThryHeaderLabelDecorator : MaterialPropertyDrawer
    {
        readonly string _text;
        readonly int _size;
        GUIStyle _style;

        public ThryHeaderLabelDecorator(string text) : this(text, EditorStyles.standardFont.fontSize)
        {
        }
        public ThryHeaderLabelDecorator(string text, float size)
        {
            this._text = text;
            this._size = (int)size;
        }


        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.RegisterDecorator(this);
            return _size + 6;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            // Done here instead of constructor because else unity throws warnings
            if(_style == null)
            {
                _style = new GUIStyle(EditorStyles.boldLabel);
                _style.fontSize = this._size;
            }

            float offst = position.height;
            position = EditorGUI.IndentedRect(position);
            GUI.Label(position, _text, _style);
        }
    }

    public class ThryRichLabelDrawer : MaterialPropertyDrawer
    {
        readonly int _size;
        GUIStyle _style;

        public ThryRichLabelDrawer(float size)
        {
            this._size = (int)size;
        }

        public ThryRichLabelDrawer() : this(EditorStyles.standardFont.fontSize) { }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return _size + 4;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            // Done here instead of constructor because else unity throws warnings
            if(_style == null)
            {
                _style = new GUIStyle(EditorStyles.boldLabel);
                _style.richText = true;
                _style.fontSize = this._size;
            }

            float offst = position.height;
            position = EditorGUI.IndentedRect(position);
            GUI.Label(position, label, _style);
        }
    }
    #endregion

    #region Vector Drawers
    public class ThryToggleDrawer : MaterialPropertyDrawer
    {
        public string keyword;
        private bool isFirstGUICall = true;
        public bool left = false;
        private bool hasKeyword = false;

        public ThryToggleDrawer()
        {
        }

        //the reason for weird string thing here is that you cant have bools as params for drawers
        public ThryToggleDrawer(string keywordLeft)
        {
            if (keywordLeft == "true") left = true;
            else if (keywordLeft == "false") left = false;
            else keyword = keywordLeft;
            hasKeyword = keyword != null;
        }

        public ThryToggleDrawer(string keyword, string left)
        {
            this.keyword = keyword;
            this.left = left == "true";
            hasKeyword = keyword != null;
        }

        protected void SetKeyword(MaterialProperty prop, bool on)
        {
            if (ShaderOptimizer.IsMaterialLocked(prop.targets[0] as Material)) return;
            SetKeywordInternal(prop, on, "_ON");
        }

        protected void CheckKeyword(MaterialProperty prop)
        {
            if (ShaderEditor.Active != null && ShaderOptimizer.IsMaterialLocked(prop.targets[0] as Material)) return;
            if (prop.hasMixedValue)
            {
                foreach (Material m in prop.targets)
                {
                    if (m.GetNumber(prop) == 1)
                        m.EnableKeyword(keyword);
                    else
                        m.DisableKeyword(keyword);
                }
            }
            else
            {
                foreach (Material m in prop.targets)
                {
                    if (prop.GetNumber() == 1)
                        m.EnableKeyword(keyword);
                    else
                        m.DisableKeyword(keyword);
                }
            }
        }

        static bool IsPropertyTypeSuitable(MaterialProperty prop)
        {
            return prop.type == MaterialProperty.PropType.Float
                   || prop.type == MaterialProperty.PropType.Range
#if UNITY_2022_1_OR_NEWER
                   || prop.type == MaterialProperty.PropType.Int;
#endif
                    ;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (!IsPropertyTypeSuitable(prop))
            {
                return EditorGUIUtility.singleLineHeight * 2.5f;
            }
            if (hasKeyword)
            {
                CheckKeyword(prop);
                DrawingData.LastPropertyDoesntAllowAnimation = true;
            }
            return base.GetPropertyHeight(prop, label, editor);
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (!IsPropertyTypeSuitable(prop))
            {
                return;
            }
            if (isFirstGUICall && !ShaderEditor.Active.IsLockedMaterial)
            {
                if (hasKeyword) CheckKeyword(prop);
                isFirstGUICall = false;
            }
            //why is this not inFirstGUICall ? cause it seems drawers are kept between different openings of the shader editor, so this needs to be set again every time the shader editor is reopened for that material
            (ShaderEditor.Active.PropertyDictionary[prop.name] as ShaderProperty).Keyword = keyword;

            EditorGUI.BeginChangeCheck();

            bool value = (Math.Abs(prop.GetNumber()) > 0.001f);
            EditorGUI.showMixedValue = prop.hasMixedValue;
            if (left) value = EditorGUI.ToggleLeft(position, label, value, Styles.style_toggle_left_richtext);
            else value = EditorGUI.Toggle(position, label, value);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                prop.SetNumber(value ? 1.0f : 0.0f);
                if (hasKeyword) SetKeyword(prop, value);
            }
        }

        public override void Apply(MaterialProperty prop)
        {
            base.Apply(prop);
            if (!IsPropertyTypeSuitable(prop))
                return;

            if (prop.hasMixedValue)
                return;

            if (hasKeyword) SetKeyword(prop, (Math.Abs(prop.GetNumber()) > 0.001f));
        }

        protected void SetKeywordInternal(MaterialProperty prop, bool on, string defaultKeywordSuffix)
        {
            // if no keyword is provided, use <uppercase property name> + defaultKeywordSuffix
            string kw = string.IsNullOrEmpty(keyword) ? prop.name.ToUpperInvariant() + defaultKeywordSuffix : keyword;
            // set or clear the keyword
            foreach (Material material in prop.targets)
            {
                if (on)
                    material.EnableKeyword(kw);
                else
                    material.DisableKeyword(kw);
            }
        }
    }

    //This class only exists for backward compatibility
    public class ThryToggleUIDrawer : ThryToggleDrawer
    {
        public ThryToggleUIDrawer()
        {
        }

        //the reason for weird string thing here is that you cant have bools as params for drawers
        public ThryToggleUIDrawer(string keywordLeft)
        {
            if (keywordLeft == "true") left = true;
            else if (keywordLeft == "false") left = false;
            else keyword = keywordLeft;
        }

        public ThryToggleUIDrawer(string keyword, string left)
        {
            this.keyword = keyword;
            this.left = left == "true";
        }
    }

    public class MultiSliderDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            GuiHelper.MinMaxSlider(position, label, prop);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

    public class VectorToSlidersDrawer : MaterialPropertyDrawer
    {
        class SliderConfig
        {
            public string Label;
            public float Min;
            public float Max;

            public SliderConfig(string l, string min, string max)
            {
                Label = l;
                Min = Parse(min);
                Max = Parse(max);
            }

            public SliderConfig(string l, float min, float max)
            {
                Label = l;
                Min = min;
                Max = max;
            }

            private float Parse(string s)
            {
                if(s.StartsWith("n", StringComparison.Ordinal))
                    return -float.Parse(s.Substring(1), System.Globalization.CultureInfo.InvariantCulture);
                return float.Parse(s.Substring(1), System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        SliderConfig _slider1;
        SliderConfig _slider2;
        SliderConfig _slider3;
        SliderConfig _slider4;
        bool _twoMinMaxDrawers;

        VectorToSlidersDrawer(SliderConfig slider1, SliderConfig slider2, SliderConfig slider3, SliderConfig slider4, float twoMinMaxDrawers)
        {
            _slider1 = slider1;
            _slider2 = slider2;
            _slider3 = slider3;
            _slider4 = slider4;
            _twoMinMaxDrawers = twoMinMaxDrawers == 1;
        }

        public VectorToSlidersDrawer(string label1, string min1, string max1, string label2, string min2, string max2, string label3, string min3, string max3, string label4, string min4, string max4) :
            this(new SliderConfig(label1, min1, max1), new SliderConfig(label2, min2, max2), new SliderConfig(label3, min3, max3), new SliderConfig(label4, min4, max4), 0)
        { }
        public VectorToSlidersDrawer(string label1, string min1, string max1, string label2, string min2, string max2, string label3, string min3, string max3) :
            this(new SliderConfig(label1, min1, max1), new SliderConfig(label2, min2, max2), new SliderConfig(label3, min3, max3), null, 0)
        { }
        public VectorToSlidersDrawer(string label1, string min1, string max1, string label2, string min2, string max2) :
            this(new SliderConfig(label1, min1, max1), new SliderConfig(label2, min2, max2), null, null, 0)
        { }
        public VectorToSlidersDrawer(float twoMinMaxDrawers, string label1, string min1, string max1, string label2, string min2, string max2) :
            this(new SliderConfig(label1, min1, max1), new SliderConfig(label2, min2, max2), null, null, twoMinMaxDrawers)
        { }

        public VectorToSlidersDrawer(string label1, float min1, float max1, string label2, float min2, float max2, string label3, float min3, float max3, string label4, float min4, float max4) :
            this(new SliderConfig(label1, min1, max1), new SliderConfig(label2, min2, max2), new SliderConfig(label3, min3, max3), new SliderConfig(label4, min4, max4), 0)
        { }
        public VectorToSlidersDrawer(string label1, float min1, float max1, string label2, float min2, float max2, string label3, float min3, float max3) :
            this(new SliderConfig(label1, min1, max1), new SliderConfig(label2, min2, max2), new SliderConfig(label3, min3, max3), null, 0)
        { }
        public VectorToSlidersDrawer(string label1, float min1, float max1, string label2, float min2, float max2) :
            this(new SliderConfig(label1, min1, max1), new SliderConfig(label2, min2, max2), null, null, 0)
        { }
        public VectorToSlidersDrawer(float twoMinMaxDrawers, string label1, float min1, float max1, string label2, float min2, float max2) :
            this(new SliderConfig(label1, min1, max1), new SliderConfig(label2, min2, max2), null, null, twoMinMaxDrawers)
        { }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            Vector4 vector = prop.vectorValue;
            EditorGUI.BeginChangeCheck();
            if (_twoMinMaxDrawers)
            {
                float min1 = vector.x;
                float max1 = vector.y;
                float min2 = vector.z;
                float max2 = vector.w;
                EditorGUI.showMixedValue = prop.hasMixedValue;
                EditorGUILayout.MinMaxSlider(_slider1.Label, ref min1, ref max1, _slider1.Min, _slider1.Max);
                EditorGUI.showMixedValue = prop.hasMixedValue;
                EditorGUILayout.MinMaxSlider(_slider2.Label, ref min2, ref max2, _slider2.Min, _slider2.Max);
                vector = new Vector4(min1, max1, min2, max2);
            }
            else
            {
                EditorGUI.showMixedValue = prop.hasMixedValue;
                vector.x = EditorGUILayout.Slider(_slider1.Label, vector.x, _slider1.Min, _slider1.Max);
                EditorGUI.showMixedValue = prop.hasMixedValue;
                vector.y = EditorGUILayout.Slider(_slider2.Label, vector.y, _slider2.Min, _slider2.Max);
                if (_slider3 != null)
                {
                    EditorGUI.showMixedValue = prop.hasMixedValue;
                    vector.z = EditorGUILayout.Slider(_slider3.Label, vector.z, _slider3.Min, _slider3.Max);
                }
                if (_slider4 != null)
                {
                    EditorGUI.showMixedValue = prop.hasMixedValue;
                    vector.w = EditorGUILayout.Slider(_slider4.Label, vector.w, _slider4.Min, _slider4.Max);
                }
            }
            if (EditorGUI.EndChangeCheck())
                prop.vectorValue = vector;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor) - EditorGUIUtility.singleLineHeight;
        }
    }

    public class Vector4TogglesDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;
            EditorGUI.LabelField(position, label);
            position.x += EditorGUIUtility.labelWidth;
            position.width = (position.width - EditorGUIUtility.labelWidth) / 4;
            bool b1 = GUI.Toggle(position, prop.vectorValue.x == 1, "");
            position.x += position.width;
            bool b2 = GUI.Toggle(position, prop.vectorValue.y == 1, "");
            position.x += position.width;
            bool b3 = GUI.Toggle(position, prop.vectorValue.z == 1, "");
            position.x += position.width;
            bool b4 = GUI.Toggle(position, prop.vectorValue.w == 1, "");
            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = new Vector4(b1 ? 1 : 0, b2 ? 1 : 0, b3 ? 1 : 0, b4 ? 1 : 0);
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

    public class ThryMultiFloatsDrawer : MaterialPropertyDrawer
    {
        string[] _otherProperties;
        MaterialProperty[] _otherMaterialProps;
        bool _displayAsToggles;

        public ThryMultiFloatsDrawer(string displayAsToggles, string p1, string p2, string p3, string p4, string p5, string p6, string p7) : this(displayAsToggles, new string[] { p1, p2, p3, p4, p5, p6, p7 }) { }
        public ThryMultiFloatsDrawer(string displayAsToggles, string p1, string p2, string p3, string p4, string p5, string p6) : this(displayAsToggles, new string[] { p1, p2, p3, p4, p5, p6 }) { }
        public ThryMultiFloatsDrawer(string displayAsToggles, string p1, string p2, string p3, string p4, string p5) : this(displayAsToggles, new string[] { p1, p2, p3, p4, p5 }) { }
        public ThryMultiFloatsDrawer(string displayAsToggles, string p1, string p2, string p3, string p4) : this(displayAsToggles, new string[] { p1, p2, p3, p4 }) { }
        public ThryMultiFloatsDrawer(string displayAsToggles, string p1, string p2, string p3) : this(displayAsToggles, new string[] { p1, p2, p3 }) { }
        public ThryMultiFloatsDrawer(string displayAsToggles, string p1, string p2) : this(displayAsToggles, new string[] { p1, p2 }) { }
        public ThryMultiFloatsDrawer(string displayAsToggles, string p1) : this(displayAsToggles, new string[] { p1 }) { }

        public ThryMultiFloatsDrawer(string displayAsToggles, params string[] extraProperties)
        {
            _displayAsToggles = displayAsToggles.ToLower() == "true" || displayAsToggles == "1";
            _otherProperties = extraProperties;
            _otherMaterialProps = new MaterialProperty[extraProperties.Length];
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            Rect labelR = new Rect(position);
            labelR.width = EditorGUIUtility.labelWidth;
            Rect contentR = new Rect(position);
            contentR.width = (contentR.width - labelR.width) / (_otherProperties.Length + 1);
            contentR.x += labelR.width;

            for (int i = 0; i < _otherProperties.Length; i++)
                _otherMaterialProps[i] = ShaderEditor.Active.PropertyDictionary[_otherProperties[i]].MaterialProperty;
            EditorGUI.BeginChangeCheck();

            EditorGUI.LabelField(labelR, label);
            int indentLevel = EditorGUI.indentLevel; //else it double indents
            EditorGUI.indentLevel = 0;
            PropGUI(prop, contentR, 0);
            if (ShaderEditor.Active.IsInAnimationMode)
                MaterialEditor.PrepareMaterialPropertiesForAnimationMode(_otherMaterialProps, true);
            for (int i = 0; i < _otherProperties.Length; i++)
            {
                PropGUI(_otherMaterialProps[i], contentR, i + 1);
            }
            EditorGUI.indentLevel = indentLevel;

            //If edited in animation mode mark as animated (needed cause other properties isnt checked in draw)
            if (EditorGUI.EndChangeCheck() && ShaderEditor.Active.IsInAnimationMode && !ShaderEditor.Active.CurrentProperty.IsAnimated)
                ShaderEditor.Active.CurrentProperty.SetAnimated(true, false);
            //make sure all are animated together
            bool animated = ShaderEditor.Active.CurrentProperty.IsAnimated;
            bool renamed = ShaderEditor.Active.CurrentProperty.IsRenaming;
            for (int i = 0; i < _otherProperties.Length; i++)
                ShaderEditor.Active.PropertyDictionary[_otherProperties[i]].SetAnimated(animated, renamed);
        }

        void PropGUI(MaterialProperty prop, Rect contentRect, int index)
        {
            contentRect.x += contentRect.width * index;
            contentRect.width -= 5;

            float val = prop.floatValue;
            EditorGUI.showMixedValue = prop.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            if (_displayAsToggles) val = EditorGUI.Toggle(contentRect, val == 1) ? 1 : 0;
            else val = EditorGUI.FloatField(contentRect, val);
            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = val;
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

    public class Vector3Drawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;
            Vector4 vec = EditorGUI.Vector3Field(position, label, prop.vectorValue);
            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = new Vector4(vec.x, vec.y, vec.z, prop.vectorValue.w);
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

    public class Vector2Drawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;
            Vector4 vec = EditorGUI.Vector2Field(position, label, prop.vectorValue);
            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = new Vector4(vec.x, vec.y, prop.vectorValue.z, prop.vectorValue.w);
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

    public class VectorLabelDrawer : MaterialPropertyDrawer
    {
        string[] _labelStrings = new string[4] { "X", "Y", "Z", "W" };
        int vectorChannels = 0;

        public VectorLabelDrawer(string labelX, string labelY)
        {
            _labelStrings[0] = labelX;
            _labelStrings[1] = labelY;
            vectorChannels = 2;
        }

        public VectorLabelDrawer(string labelX, string labelY, string labelZ)
        {
            _labelStrings[0] = labelX;
            _labelStrings[1] = labelY;
            _labelStrings[2] = labelZ;
            vectorChannels = 3;
        }

        public VectorLabelDrawer(string labelX, string labelY, string labelZ, string labelW)
        {
            _labelStrings[0] = labelX;
            _labelStrings[1] = labelY;
            _labelStrings[2] = labelZ;
            _labelStrings[3] = labelW;
            vectorChannels = 4;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;

            Rect labelR = new Rect(position.x, position.y, position.width * 0.41f, position.height);
            Rect contentR = new Rect(position.x + labelR.width, position.y, position.width - labelR.width, position.height);

            float[] values = new float[vectorChannels];
            GUIContent[] labels = new GUIContent[vectorChannels];

            for (int i = 0; i < vectorChannels; i++)
            {
                values[i] = prop.vectorValue[i];
                labels[i] = new GUIContent(_labelStrings[i]);
            }

            EditorGUI.LabelField(labelR, label);
            EditorGUI.MultiFloatField(contentR, labels, values);

            if (EditorGUI.EndChangeCheck())
            {
                switch (vectorChannels)
                {
                    case 2:
                        prop.vectorValue = new Vector4(values[0], values[1], prop.vectorValue.z, prop.vectorValue.w);
                        break;
                    case 3:
                        prop.vectorValue = new Vector4(values[0], values[1], values[2], prop.vectorValue.w);
                        break;
                    case 4:
                        prop.vectorValue = new Vector4(values[0], values[1], values[2], values[3]);
                        break;
                    default:
                        break;
                }
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

    // Enum with normal editor width, rather than MaterialEditor Default GUI widths
    // Would be nice if Decorators could access Drawers too so this wouldn't be necessary for something to trivial
    // Adapted from Unity interal MaterialEnumDrawer https://github.com/Unity-Technologies/UnityCsReference/
    public class ThryWideEnumDrawer : MaterialPropertyDrawer
    {
        // TODO: Consider Load locale by property name in the future (maybe, could have drawbacks)
        private GUIContent[] names;
        private readonly string[] defaultNames;
        private readonly float[] values;
        private int _reloadCount = -1;
        private static int _reloadCountStatic;

        // internal Unity AssemblyHelper can't be accessed
        private Type[] TypesFromAssembly(Assembly a)
        {
            if (a == null)
                return new Type[0];
            try
            {
                return a.GetTypes();
            }
            catch (ReflectionTypeLoadException)
            {
                return new Type[0];
            }
        }
        public ThryWideEnumDrawer(string enumName, int j)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(
                x => TypesFromAssembly(x)).ToArray();
            try
            {
                var enumType = types.FirstOrDefault(
                    x => x.IsEnum && (x.Name == enumName || x.FullName == enumName)
                );
                var enumNames = Enum.GetNames(enumType);
                names = new GUIContent[enumNames.Length];
                for (int i = 0; i < enumNames.Length; ++i)
                    names[i] = new GUIContent(enumNames[i]);

                var enumVals = Enum.GetValues(enumType);
                values = new float[enumVals.Length];
                for (int i = 0; i < enumVals.Length; ++i)
                    values[i] = (int)enumVals.GetValue(i);
            }
            catch (Exception)
            {
                Debug.LogWarningFormat("Failed to create  WideEnum, enum {0} not found", enumName);
                throw;
            }

        }

        public ThryWideEnumDrawer(string n1, float v1) : this(new[] { n1 }, new[] { v1 }) { }
        public ThryWideEnumDrawer(string n1, float v1, string n2, float v2) : this(new[] { n1, n2 }, new[] { v1, v2 }) { }
        public ThryWideEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3) : this(new[] { n1, n2, n3 }, new[] { v1, v2, v3 }) { }
        public ThryWideEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4) : this(new[] { n1, n2, n3, n4 }, new[] { v1, v2, v3, v4 }) { }
        public ThryWideEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5) : this(new[] { n1, n2, n3, n4, n5 }, new[] { v1, v2, v3, v4, v5 }) { }
        public ThryWideEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6) : this(new[] { n1, n2, n3, n4, n5, n6 }, new[] { v1, v2, v3, v4, v5, v6 }) { }
        public ThryWideEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6, string n7, float v7) : this(new[] { n1, n2, n3, n4, n5, n6, n7 }, new[] { v1, v2, v3, v4, v5, v6, v7 }) { }
        public ThryWideEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6, string n7, float v7, string n8, float v8) : this(new[] { n1, n2, n3, n4, n5, n6, n7, n8 }, new[] { v1, v2, v3, v4, v5, v6, v7, v8 }) { }
        public ThryWideEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6, string n7, float v7, string n8, float v8, string n9, float v9) : this(new[] { n1, n2, n3, n4, n5, n6, n7, n8, n9 }, new[] { v1, v2, v3, v4, v5, v6, v7, v8, v9 }) { }
        public ThryWideEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6, string n7, float v7, string n8, float v8, string n9, float v9, string n10, float v10) : this(new[] { n1, n2, n3, n4, n5, n6, n7, n8, n9, n10 }, new[] { v1, v2, v3, v4, v5, v6, v7, v8, v9, v10 }) { }
        public ThryWideEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6, string n7, float v7, string n8, float v8, string n9, float v9, string n10, float v10, string n11, float v11) : this(new[] { n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11 }, new[] { v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11 }) { }
        public ThryWideEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6, string n7, float v7, string n8, float v8, string n9, float v9, string n10, float v10, string n11, float v11, string n12, float v12) : this(new[] { n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11, n12 }, new[] { v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12 }) { }
        public ThryWideEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6, string n7, float v7, string n8, float v8, string n9, float v9, string n10, float v10, string n11, float v11, string n12, float v12, string n13, float v13) : this(new[] { n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11, n12, n13 }, new[] { v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13 }) { }
        public ThryWideEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6, string n7, float v7, string n8, float v8, string n9, float v9, string n10, float v10, string n11, float v11, string n12, float v12, string n13, float v13, string n14, float v14) : this(new[] { n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11, n12, n13, n14 }, new[] { v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14 }) { }
        public ThryWideEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6, string n7, float v7, string n8, float v8, string n9, float v9, string n10, float v10, string n11, float v11, string n12, float v12, string n13, float v13, string n14, float v14, string n15, float v15) : this(new[] { n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11, n12, n13, n14, n15 }, new[] { v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15 }) { }
        public ThryWideEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6, string n7, float v7, string n8, float v8, string n9, float v9, string n10, float v10, string n11, float v11, string n12, float v12, string n13, float v13, string n14, float v14, string n15, float v15, string n16, float v16) : this(new[] { n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11, n12, n13, n14, n15, n16 }, new[] { v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15, v16 }) { }
        public ThryWideEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6, string n7, float v7, string n8, float v8, string n9, float v9, string n10, float v10, string n11, float v11, string n12, float v12, string n13, float v13, string n14, float v14, string n15, float v15, string n16, float v16, string n17, float v17) : this(new[] { n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11, n12, n13, n14, n15, n16, n17 }, new[] { v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15, v16, v17 }) { }
        public ThryWideEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6, string n7, float v7, string n8, float v8, string n9, float v9, string n10, float v10, string n11, float v11, string n12, float v12, string n13, float v13, string n14, float v14, string n15, float v15, string n16, float v16, string n17, float v17, string n18, float v18) : this(new[] { n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11, n12, n13, n14, n15, n16, n17, n18 }, new[] { v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15, v16, v17, v18 }) { }
        public ThryWideEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6, string n7, float v7, string n8, float v8, string n9, float v9, string n10, float v10, string n11, float v11, string n12, float v12, string n13, float v13, string n14, float v14, string n15, float v15, string n16, float v16, string n17, float v17, string n18, float v18, string n19, float v19) : this(new[] { n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11, n12, n13, n14, n15, n16, n17, n18, n19 }, new[] { v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15, v16, v17, v18, v19 }) { }
        public ThryWideEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6, string n7, float v7, string n8, float v8, string n9, float v9, string n10, float v10, string n11, float v11, string n12, float v12, string n13, float v13, string n14, float v14, string n15, float v15, string n16, float v16, string n17, float v17, string n18, float v18, string n19, float v19, string n20, float v20) : this(new[] { n1, n2, n3, n4, n5, n6, n7, n8, n9, n10, n11, n12, n13, n14, n15, n16, n17, n18, n19, n20 }, new[] { v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15, v16, v17, v18, v19, v20 }) { }
        public ThryWideEnumDrawer(string[] enumNames, float[] vals)
        {
            defaultNames = enumNames;

            // Init without Locale to prevent errors
            names = new GUIContent[enumNames.Length];
            for (int i = 0; i < enumNames.Length; ++i)
                names[i] = new GUIContent(enumNames[i]);

            values = new float[vals.Length];
            for (int i = 0; i < vals.Length; ++i)
                values[i] = vals[i];
        }

        void LoadNames()
        {
            names = new GUIContent[defaultNames.Length];
            for (int i = 0; i < defaultNames.Length; ++i)
            {
                names[i] = new GUIContent(ShaderEditor.Active.Locale.Get(defaultNames[i], defaultNames[i]));
            }
        }
        public static void Reload()
        {
            _reloadCountStatic++;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            EditorGUI.showMixedValue = prop.hasMixedValue;
            float value = prop.GetNumber();
            int selectedIndex = Array.IndexOf(values, value);

            if (_reloadCount != _reloadCountStatic)
            {
                _reloadCount = _reloadCountStatic;
                LoadNames();
            }

            // Custom Change Check, so it triggers on reselect too
            bool wasClickEvent = Event.current.type == EventType.ExecuteCommand;
            int selIndex = EditorGUI.Popup(position, label, selectedIndex, names);
            EditorGUI.showMixedValue = false;
            if (wasClickEvent && Event.current.type == EventType.Used)
            {
                // Set GUI.changed to true, so it triggers a change event, even on reselection
                GUI.changed = true;
                prop.SetNumber(values[selIndex]);
            } 
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

    #endregion

    #region Float Drawers
    public class ThryIntRangeDrawer: MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            var range = prop.rangeLimits;
            EditorGUI.BeginChangeCheck();
            var value = EditorGUI.IntSlider(position, label, (int)prop.GetNumber(), (int)range.x, (int)range.y);
            if (EditorGUI.EndChangeCheck())
                prop.SetNumber(value);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }
    #endregion

    #region UI Drawers
    public class HelpboxDrawer : MaterialPropertyDrawer
    {
        readonly MessageType type;

        public HelpboxDrawer()
        {
            type = MessageType.Info;
        }

        public HelpboxDrawer(float f)
        {
            type = (MessageType)(int)f;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            EditorGUILayout.HelpBox(label.text, type);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            return 0;
        }
    }

    public class sRGBWarningDecorator : MaterialPropertyDrawer
    {
        bool _isSRGB = true;

        public sRGBWarningDecorator()
        {
            _isSRGB = false;
        }

        public sRGBWarningDecorator(string shouldHaveSRGB)
        {
            this._isSRGB = shouldHaveSRGB.ToLower() == "true";
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            GuiHelper.ColorspaceWarning(prop, _isSRGB);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.RegisterDecorator(this);
            return 0;
        }
    }

    public class LocalMessageDrawer : MaterialPropertyDrawer
    {
        protected ButtonData _buttonData;
        protected bool _isInit;
        protected virtual void Init(string s)
        {
            if (_isInit) return;
            _buttonData = Parser.Deserialize<ButtonData>(s);
            _isInit = true;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            Init(prop.displayName);
            if (_buttonData == null) return;
            if (_buttonData.text.Length > 0)
            {
                GUILayout.Label(new GUIContent(_buttonData.text, _buttonData.hover), _buttonData.center_position ? Styles.richtext_center : Styles.richtext);
                Rect r = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
                    _buttonData.action.Perform(ShaderEditor.Active?.Materials);
            }
            if (_buttonData.texture != null)
            {
                if (_buttonData.center_position) GUILayout.Label(new GUIContent(_buttonData.texture.loaded_texture, _buttonData.hover), EditorStyles.centeredGreyMiniLabel, GUILayout.MaxHeight(_buttonData.texture.height));
                else GUILayout.Label(new GUIContent(_buttonData.texture.loaded_texture, _buttonData.hover), GUILayout.MaxHeight(_buttonData.texture.height));
                Rect r = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
                    _buttonData.action.Perform(ShaderEditor.Active?.Materials);
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            return 0;
        }
    }

    public class RemoteMessageDrawer : LocalMessageDrawer
    {

        protected override void Init(string s)
        {
            if (_isInit) return;
            WebHelper.DownloadStringASync(s, (Action<string>)((string data) =>
            {
                _buttonData = Parser.Deserialize<ButtonData>(data);
            }));
            _isInit = true;
        }
    }

    public class ThryCustomGUIDrawer : MaterialPropertyDrawer
    {
        private MethodInfo _method;
        public ThryCustomGUIDrawer(string type, string namespaceName,string method)
        {
            Type t = Type.GetType(type + ", " + namespaceName);
            if(t != null)
            {
                _method = t.GetMethod(method);
            }
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (_method != null)
            {
                _method.Invoke(null, new object[] { position, prop, label, editor, ShaderEditor.Active });
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            return 0;
        }
    }
    #endregion

    #region enums
    public enum ColorMask
    {
        None,
        Alpha,
        Blue,
        BA,
        Green,
        GA,
        GB,
        GBA,
        Red,
        RA,
        RB,
        RBA,
        RG,
        RGA,
        RGB,
        RGBA
    }

    // DX11 only blend operations
    public enum BlendOp
    {
        Add,
        Subtract,
        ReverseSubtract,
        Min,
        Max,
        LogicalClear,
        LogicalSet,
        LogicalCopy,
        LogicalCopyInverted,
        LogicalNoop,
        LogicalInvert,
        LogicalAnd,
        LogicalNand,
        LogicalOr,
        LogicalNor,
        LogicalXor,
        LogicalEquivalence,
        LogicalAndReverse,
        LogicalAndInverted,
        LogicalOrReverse,
        LogicalOrInverted
    }
    #endregion

    public class ThryShaderOptimizerLockButtonDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty shaderOptimizer, string label, MaterialEditor materialEditor)
        {
            Material material = shaderOptimizer.targets[0] as Material;
            Shader shader = material.shader;
            // The GetPropertyDefaultFloatValue is changed from 0 to 1 when the shader is locked in
            bool isLocked = shader.name.StartsWith("Hidden/Locked/") ||
                (shader.name.StartsWith("Hidden/") && material.GetTag("OriginalShader", false, "") != "" && shader.GetPropertyDefaultFloatValue(shader.FindPropertyIndex(shaderOptimizer.name)) == 1);
            //this will make sure the button is unlocked if you manually swap to an unlocked shader
            //shaders that have the ability to be locked shouldnt really be hidden themself. at least it wouldnt make too much sense
            if (shaderOptimizer.hasMixedValue == false && shaderOptimizer.GetNumber() == 1 && isLocked == false)
            {
                shaderOptimizer.SetNumber(0);
            }
            else if (shaderOptimizer.hasMixedValue == false && shaderOptimizer.GetNumber() == 0 && isLocked)
            {
                shaderOptimizer.SetNumber(1);
            }

            bool disabled = false;
#if UNITY_2022_1_OR_NEWER
            disabled |= ShaderEditor.Active.Materials[0].isVariant;
#endif
            EditorGUI.BeginDisabledGroup(disabled); // for variant materials

            // Theoretically this shouldn't ever happen since locked in materials have different shaders.
            // But in a case where the material property says its locked in but the material really isn't, this
            // will display and allow users to fix the property/lock in
            ShaderEditor.Active.IsLockedMaterial = shaderOptimizer.GetNumber() == 1;
            if (shaderOptimizer.hasMixedValue)
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.Button(EditorLocale.editor.Get("lockin_button_multi").ReplaceVariables(materialEditor.targets.Length));
                if (EditorGUI.EndChangeCheck())
                {
                    SaveChangeStack();
                    ShaderOptimizer.SetLockedForAllMaterials(shaderOptimizer.targets.Select(t => t as Material), shaderOptimizer.floatValue == 1 ? 0 : 1, true, false, false, shaderOptimizer);
                    RestoreChangeStack();
                }
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                if (shaderOptimizer.GetNumber() == 0)
                {
                    if (materialEditor.targets.Length == 1)
                        GUILayout.Button(EditorLocale.editor.Get("lockin_button_single"));
                    else GUILayout.Button(EditorLocale.editor.Get("lockin_button_multi").ReplaceVariables(materialEditor.targets.Length));
                }
                else
                {
                    if (materialEditor.targets.Length == 1)
                        GUILayout.Button(EditorLocale.editor.Get("unlock_button_single"));
                    else GUILayout.Button(EditorLocale.editor.Get("unlock_button_multi").ReplaceVariables(materialEditor.targets.Length));
                }
                if (EditorGUI.EndChangeCheck())
                {
                    SaveChangeStack();
                    ShaderOptimizer.SetLockedForAllMaterials(shaderOptimizer.targets.Select(t => t as Material), shaderOptimizer.GetNumber() == 1 ? 0 : 1, true, false, false, shaderOptimizer);
                    RestoreChangeStack();
                }
            }
            if (Config.Singleton.allowCustomLockingRenaming || ShaderEditor.Active.HasCustomRenameSuffix)
            {
                EditorGUI.BeginDisabledGroup(!Config.Singleton.allowCustomLockingRenaming || ShaderEditor.Active.IsLockedMaterial);
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = ShaderEditor.Active.HasMixedCustomPropertySuffix;
                ShaderEditor.Active.RenamedPropertySuffix = EditorGUILayout.TextField("Locked property suffix: ", ShaderEditor.Active.RenamedPropertySuffix);
                if (EditorGUI.EndChangeCheck())
                {
                    // Make sure suffix that is saved is valid
                    ShaderEditor.Active.RenamedPropertySuffix = ShaderOptimizer.CleanStringForPropertyNames(ShaderEditor.Active.RenamedPropertySuffix.Replace(" ", "_"));
                    foreach (Material m in ShaderEditor.Active.Materials)
                        m.SetOverrideTag("thry_rename_suffix", ShaderEditor.Active.RenamedPropertySuffix);
                    if (ShaderEditor.Active.RenamedPropertySuffix == "")
                        ShaderEditor.Active.RenamedPropertySuffix = ShaderOptimizer.GetRenamedPropertySuffix(ShaderEditor.Active.Materials[0]);
                    ShaderEditor.Active.HasCustomRenameSuffix = ShaderOptimizer.HasCustomRenameSuffix(ShaderEditor.Active.Materials[0]);
                }
                if (!Config.Singleton.allowCustomLockingRenaming)
                {
                    EditorGUILayout.HelpBox("This feature is disabled in the config file. You can enable it by setting allowCustomLockingRenaming to true.", MessageType.Info);
                }
                EditorGUI.EndDisabledGroup();
            }

            EditorGUI.EndDisabledGroup(); // for variant materials
        }

        //This code purly exists cause Unity 2019 is a piece of shit that looses it's internal change stack on locking CAUSE FUCK IF I KNOW
        static System.Reflection.FieldInfo changeStack = typeof(EditorGUI).GetField("s_ChangedStack", BindingFlags.Static | BindingFlags.NonPublic);
        static int preLockStackSize = 0;
        private static void SaveChangeStack()
        {
            if (changeStack != null)
            {
                Stack<bool> stack = (Stack<bool>)changeStack.GetValue(null);
                if (stack != null)
                {
                    preLockStackSize = stack.Count();
                }
            }
        }

        private static void RestoreChangeStack()
        {
            if (changeStack != null)
            {
                Stack<bool> stack = (Stack<bool>)changeStack.GetValue(null);
                if (stack != null)
                {
                    int postLockStackSize = stack.Count();
                    //Restore change stack from before lock / unlocking
                    for (int i = postLockStackSize; i < preLockStackSize; i++)
                    {
                        EditorGUI.BeginChangeCheck();
                    }
                }
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            DrawingData.LastPropertyDoesntAllowAnimation = true;
            ShaderEditor.Active.DoUseShaderOptimizer = true;
            return -2;
        }
    }

    public class ThryDecalPositioningDecorator : MaterialPropertyDrawer
    {
        string _texturePropertyName;
        string _uvIndexPropertyName;
        string _positionPropertyName;
        string _rotationPropertyName;
        string _scalePropertyName;
        string _offsetPropertyName;
        DecalSceneTool _sceneTool;
        DecalTool _tool;

        public ThryDecalPositioningDecorator(string textureProp, string uvIndexPropertyName, string positionProp, string rotationProp, string scaleProp, string offsetProp)
        {
            _texturePropertyName = textureProp;
            _uvIndexPropertyName = uvIndexPropertyName;
            _positionPropertyName = positionProp;
            _rotationPropertyName = rotationProp;
            _offsetPropertyName = offsetProp;
            _scalePropertyName = scaleProp;
        }

        void CreateSceneTool()
        {
            DiscardSceneTool();
            _sceneTool = DecalSceneTool.Create(
                Selection.activeTransform.GetComponent<Renderer>(),
                ShaderEditor.Active.Materials[0],
                (int)ShaderEditor.Active.PropertyDictionary[_uvIndexPropertyName].MaterialProperty.GetNumber(),
                ShaderEditor.Active.PropertyDictionary[_positionPropertyName].MaterialProperty,
                ShaderEditor.Active.PropertyDictionary[_rotationPropertyName].MaterialProperty,
                ShaderEditor.Active.PropertyDictionary[_scalePropertyName].MaterialProperty,
                ShaderEditor.Active.PropertyDictionary[_offsetPropertyName].MaterialProperty);
        }

        void DiscardSceneTool()
        {
            if(_sceneTool != null)
            {
                _sceneTool.Deactivate();
                _sceneTool = null;
            }
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            position = new RectOffset(0, 0, 0, 3).Remove(EditorGUI.IndentedRect(position));
            bool isInScene = Selection.activeTransform != null && Selection.activeTransform.GetComponent<Renderer>() != null;
            if(isInScene)
            {
                position.width /= 3;
                ButtonGUI(position);
                position.x += position.width;
                ButtonRaycast(position);
                position.x += position.width;
                ButtonSceneTools(position);
                if(_sceneTool != null)
                {
                    _sceneTool.SetMaterialProperties(
                        ShaderEditor.Active.PropertyDictionary[_positionPropertyName].MaterialProperty,
                        ShaderEditor.Active.PropertyDictionary[_rotationPropertyName].MaterialProperty,
                        ShaderEditor.Active.PropertyDictionary[_scalePropertyName].MaterialProperty,
                        ShaderEditor.Active.PropertyDictionary[_offsetPropertyName].MaterialProperty);
                }
            }else
            {
                ButtonGUI(position);
            }
        }

        void ButtonGUI(Rect r)
        {
            if(GUI.Button(r, "Open Positioning Tool"))
            {
                _tool = DecalTool.OpenDecalTool(ShaderEditor.Active.Materials[0]);
            }
            // This is done because the tool didnt want to update if the data was changed from the outside
            if(_tool != null)
            {
                _tool.SetMaterialProperties(
                    ShaderEditor.Active.PropertyDictionary[_texturePropertyName].MaterialProperty,
                    ShaderEditor.Active.PropertyDictionary[_uvIndexPropertyName].MaterialProperty,
                    ShaderEditor.Active.PropertyDictionary[_positionPropertyName].MaterialProperty,
                    ShaderEditor.Active.PropertyDictionary[_rotationPropertyName].MaterialProperty,
                    ShaderEditor.Active.PropertyDictionary[_scalePropertyName].MaterialProperty,
                    ShaderEditor.Active.PropertyDictionary[_offsetPropertyName].MaterialProperty);
            }
        }

        void ButtonRaycast(Rect r)
        {
            if (GUI.Button(r, "Raycast"))
            {
                if(_sceneTool != null && _sceneTool.GetMode() == DecalSceneTool.Mode.Raycast)
                {
                    DiscardSceneTool();
                }
                else
                {
                    CreateSceneTool();
                    _sceneTool.StartRaycastMode();
                }
            }
            if(_sceneTool != null && _sceneTool.GetMode() == DecalSceneTool.Mode.Raycast)
                GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(0.5f, 0.5f, 0.5f, 0.5f), 0, 3);
        }

        void ButtonSceneTools(Rect r)
        {
            if (GUI.Button(r, "Scene Tools"))
            {
                if(_sceneTool != null && _sceneTool.GetMode() == DecalSceneTool.Mode.Handles)
                {
                    DiscardSceneTool();
                }
                else
                {
                    CreateSceneTool();
                    _sceneTool.StartHandleMode();
                }
            }
            if(_sceneTool != null && _sceneTool.GetMode() == DecalSceneTool.Mode.Handles)
                GUI.DrawTexture(r, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, new Color(0.5f, 0.5f, 0.5f, 0.5f), 0, 3);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            DrawingData.LastPropertyDoesntAllowAnimation = false;
            return EditorGUIUtility.singleLineHeight + 6;
        }
    }
}
