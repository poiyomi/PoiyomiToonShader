using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Thry.TexturePacker;

namespace Thry
{
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
            public int _overwriteShowInline;

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
            // So showFoldoutProperties is reset to false a few events after an undo event.
            // To keep the texture packer open we just force it to ture until this reset is done (which happens in / after the ValidateCommand event, right before the KeyUp event)
            if (_current._overwriteShowInline > 0)
            {
                DrawingData.CurrentTextureProperty.showFoldoutProperties = true;
                if (Event.current.type == EventType.ValidateCommand)
                    _current._overwriteShowInline = 1;
                else if (Event.current.type == EventType.Layout && _current._overwriteShowInline == 1)
                    _current._overwriteShowInline = 0;
            }
            GUILib.SmallTextureProperty(position, prop, label, editor, true, TexturePackerGUI);
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
                foreach (Material m in ShaderEditor.Active.Materials)
                    Undo.RegisterCompleteObjectUndo(m, "Thry Packer Texture Change " + _prop.name);
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
                input.TextureSource.SetInputTexture(changed);
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

#if UNITY_2022_1_OR_NEWER
            Undo.undoRedoEvent += OnUndoRedo;
        }

        void OnUndoRedo(in UndoRedoInfo undoRedoInfo)
        {
            if (undoRedoInfo.undoName == "Thry Packer Texture Change " + _prop.name)
            {
                _current._isInit = false;
                _current._overwriteShowInline = 2;
                Undo.undoRedoEvent -= OnUndoRedo;
            }
#endif
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
                    packerChannelConfig.TextureSource.SetInputTexture(AssetDatabase.LoadAssetAtPath<Texture2D>(p));
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
            TexturePacker.Connection connection_0 = connections.Where(c => c.FromTextureIndex == 0).FirstOrDefault();
            TexturePacker.Connection connection_1 = connections.Where(c => c.FromTextureIndex == 1).FirstOrDefault();
            TexturePacker.Connection connection_2 = connections.Where(c => c.FromTextureIndex == 2).FirstOrDefault();
            TexturePacker.Connection connection_3 = connections.Where(c => c.FromTextureIndex == 3).FirstOrDefault();

            _current._input_r.TextureSource = connection_0 != null ? sources[0] : new TextureSource();
            _current._input_g.TextureSource = connection_1 != null ? sources[1] : new TextureSource();
            _current._input_b.TextureSource = connection_2 != null ? sources[2] : new TextureSource();
            _current._input_a.TextureSource = connection_3 != null ? sources[3] : new TextureSource();

            _current._input_r.FromOutputConfig(configs[0]);
            _current._input_g.FromOutputConfig(configs[1]);
            _current._input_b.FromOutputConfig(configs[2]);
            _current._input_a.FromOutputConfig(configs[3]);

            _current._input_r.Channel = connection_0 != null ? connection_0.FromChannel : TexturePacker.TextureChannelIn.Max;
            _current._input_g.Channel = connection_1 != null ? connection_1.FromChannel : TexturePacker.TextureChannelIn.Max;
            _current._input_b.Channel = connection_2 != null ? connection_2.FromChannel : TexturePacker.TextureChannelIn.Max;
            _current._input_a.Channel = connection_3 != null ? connection_3.FromChannel : TexturePacker.TextureChannelIn.Max;

            _current._packedTexture = tex;
            _prop.textureValue = _current._packedTexture;
            _current._hasTextureChanged = true;
            _current._hasConfigChanged = true;
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
            string dir;
            switch(Config.Singleton.inlinePackerSaveLocation)
            {
                case TextureSaveLocation.material:
                    dir = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(ShaderEditor.Active.Materials[0]));
                    break;
                case TextureSaveLocation.texture:
                    if(_current._input_r.GetTexture() != null) dir = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(_current._input_r.GetTexture()));
                    else if(_current._input_g.GetTexture() != null) dir = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(_current._input_g.GetTexture()));
                    else if(_current._input_b.GetTexture() != null) dir = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(_current._input_b.GetTexture()));
                    else if(_current._input_a.GetTexture() != null) dir = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(_current._input_a.GetTexture()));
                    else dir = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(ShaderEditor.Active.Materials[0]));
                    break;
                case TextureSaveLocation.custom:
                    dir = Config.Singleton.inlinePackerSaveLocationCustom;
                    break;
                case TextureSaveLocation.prompt:
                    dir = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
                    dir = dir.Replace(Application.dataPath, "Assets");
                    break;
                default:
                    dir = "Assets/Textures/Packed";
                    break;
            }
            string fileName = ShaderEditor.Active.Materials[0].name + _prop.name + ".png";
            string path = dir + "/" + fileName;
            _prop.textureValue = TextureHelper.SaveTextureAsPNG(_current._packedTexture, path);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.streamingMipmaps = true;
            importer.crunchedCompression = Config.Singleton.inlinePackerChrunchCompression;
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
            ShaderProperty.RegisterDrawer(this);
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

}