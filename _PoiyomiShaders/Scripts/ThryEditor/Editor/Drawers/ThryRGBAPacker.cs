using System;
using System.Collections.Generic;
using System.Linq;
using Thry.ThryEditor.Helpers;
using Thry.ThryEditor.TexturePacker;
using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor.Drawers
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
            public TexturePacker.PackerSource[] _sources;
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
            GUILib.SmallTextureProperty(position, prop, label, editor, true, InlineTexturePackerGUI);
            if (_prop.textureValue != _current._packedTexture) _current._previousTexture = _prop.textureValue;
        }

        void InlineTexturePackerGUI()
        {
            Init();
            LoadLabels();
            bool didChange = false;
            didChange |= TexturePackerSlotGUI(_current._input_r, _label1);
            didChange |= TexturePackerSlotGUI(_current._input_g, _label2);
            if (_label3 != null) didChange |= TexturePackerSlotGUI(_current._input_b, _label3);
            if (_label4 != null) didChange |= TexturePackerSlotGUI(_current._input_a, _label4);
            didChange |= _current._input_r.Source.HasBeenModifiedExternally();
            didChange |= _current._input_g.Source.HasBeenModifiedExternally();
            didChange |= _current._input_b.Source.HasBeenModifiedExternally();
            didChange |= _current._input_a.Source.HasBeenModifiedExternally();
            if (didChange)
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

        bool TexturePackerSlotGUI(InlinePackerChannelConfig input, string label)
        {
            bool didChange = false;
            EditorGUI.BeginChangeCheck();

            Rect totalRect = EditorGUILayout.GetControlRect(false);
            totalRect = EditorGUI.IndentedRect(totalRect);
            Rect r = totalRect;

            int ind = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float texWidth = Math.Max(50, r.width - 130 - 30) - 5;
            r.x = totalRect.x;
            r.width = 30;
            EditorGUI.BeginChangeCheck();
            Texture2D changed = EditorGUI.ObjectField(r, input.Source.Texture, typeof(Texture2D), false) as Texture2D;
            if (EditorGUI.EndChangeCheck())
            {
                input.Source.SetInputTexture(changed);
            }

            r.x += r.width + 5;
            r.width = texWidth - 5;
            EditorGUI.LabelField(r, label);

            if (input.Source.Texture == null)
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

                // remap multi slider
                didChange = EditorGUI.EndChangeCheck();
                r.width = 150;
                r.x -= r.width + 5;
                EventType eventType = Event.current.type;
                EditorGUI.MinMaxSlider(r, ref input.Remapping.x, ref input.Remapping.y, 0, 1);
                input.Remapping.z = 0;
                input.Remapping.w = 1;
                didChange |= Event.current.type == EventType.Used && eventType == EventType.MouseUp;
            }

            EditorGUI.indentLevel = ind;

            return didChange;
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
                if (input.Source.Texture != null) m.SetOverrideTag(id + "_texPack_" + channel + "_guid", AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(input.Source.Texture)));
                else m.SetOverrideTag(id + "_texPack_" + channel + "_guid", "");
                m.SetOverrideTag(id + "_texPack_" + channel + "_fallback", input.Fallback.ToString());
                m.SetOverrideTag(id + "_texPack_" + channel + "_inverted", input.Invert.ToString());
                m.SetOverrideTag(id + "_texPack_" + channel + "_channel", ((int)input.Channel).ToString());
                m.SetOverrideTag(id + "_texPack_" + channel + "_srcRange", input.Remapping.ToString());
            }
        }

        InlinePackerChannelConfig LoadForChannel(Material m, string id, string channel)
        {
            InlinePackerChannelConfig packerChannelConfig = new InlinePackerChannelConfig();
            packerChannelConfig.Fallback = Parser.ParseFloat(m.GetTag(id + "_texPack_" + channel + "_fallback", false, "1"));
            packerChannelConfig.Invert = bool.Parse(m.GetTag(id + "_texPack_" + channel + "_inverted", false, "false"));
            packerChannelConfig.Channel = (TexturePacker.TextureChannelIn)int.Parse(m.GetTag(id + "_texPack_" + channel + "_channel", false, "4"));
            packerChannelConfig.Remapping = Parser.ParseVector4(m.GetTag(id + "_texPack_" + channel + "_srcRange", false, "(0,1,0,1)"));
            string guid = m.GetTag(id + "_texPack_" + channel + "_guid", false, "");
            if (string.IsNullOrEmpty(guid) == false)
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                if (p != null)
                    packerChannelConfig.Source.SetInputTexture(AssetDatabase.LoadAssetAtPath<Texture2D>(p));
            }
            _ = packerChannelConfig.Source.ComputeShaderTexture; // to cache uncompressed texture
            return packerChannelConfig;
        }

        void Pack()
        {
            _current._packedTexture = Packer.Pack(GetConfig());
            _prop.textureValue = _current._packedTexture;

            _current._hasTextureChanged = true;
        }

        TexturePackerConfig GetConfig()
        {
            TexturePackerConfig config = TexturePackerConfig.GetNewConfig();
            config.Sources = GetTextureSources();
            config.Targets = GetOutputConfigs();
            config.Connections = GetConnections().ToList();
            config.FileOutput.FilterMode = GetFiltermode();
            config.FileOutput.ColorSpace = _colorSpace;
            config.FileOutput.AlphaIsTransparency = _alphaIsTransparency;
            return config;
        }

        TexturePacker.PackerSource[] GetTextureSources()
        {
            // build sources array
            return new TexturePacker.PackerSource[4]{
                _current._input_r.Source, _current._input_g.Source, _current._input_b.Source, _current._input_a.Source };
        }

        TexturePacker.OutputTarget[] GetOutputConfigs()
        {
            // Build OutputConfig Array
            TexturePacker.OutputTarget[] outputConfigs = new TexturePacker.OutputTarget[4];

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

        Connection[] GetConnections()
        {
            // Build connections array
            Connection[] connections = new Connection[4];
            if (_firstTextureIsRGB)
            {
                connections[0] = new Connection(0, TextureChannelIn.R, TextureChannelOut.R, _current._input_r.RemappingMode, _current._input_r.Remapping);
                connections[1] = new Connection(0, TextureChannelIn.G, TextureChannelOut.G, _current._input_r.RemappingMode, _current._input_r.Remapping);
                connections[2] = new Connection(0, TextureChannelIn.B, TextureChannelOut.B, _current._input_r.RemappingMode, _current._input_r.Remapping);
                connections[3] = new Connection(1, _current._input_g.Channel, TextureChannelOut.A, _current._input_g.RemappingMode, _current._input_g.Remapping);
            }
            else
            {
                connections[0] = new Connection(0, _current._input_r.Channel, TextureChannelOut.R, _current._input_r.RemappingMode, _current._input_r.Remapping);
                connections[1] = new Connection(1, _current._input_g.Channel, TextureChannelOut.G, _current._input_g.RemappingMode, _current._input_g.Remapping);
                connections[2] = new Connection(2, _current._input_b.Channel, TextureChannelOut.B, _current._input_b.RemappingMode, _current._input_b.Remapping);
                connections[3] = new Connection(3, _current._input_a.Channel, TextureChannelOut.A, _current._input_a.RemappingMode, _current._input_a.Remapping);
            }
            return connections;
        }

        void OpenFullTexturePacker()
        {
            NodeGUI packer = NodeGUI.Open(GetConfig());
            packer.OnChange += FullTexturePackerOnChange;
            packer.OnSave += FullTexturePackerOnSave;
        }

        void FullTexturePackerOnSave(Texture2D tex)
        {
            _current._packedTexture = tex;
            _prop.textureValue = _current._packedTexture;
            _current._hasTextureChanged = false;
        }

        void FullTexturePackerOnChange(Texture2D tex, TexturePackerConfig config)
        {
            Connection connection_0 = config.Connections.Where(c => c.FromTextureIndex == 0).FirstOrDefault();
            Connection connection_1 = config.Connections.Where(c => c.FromTextureIndex == 1).FirstOrDefault();
            Connection connection_2 = config.Connections.Where(c => c.FromTextureIndex == 2).FirstOrDefault();
            Connection connection_3 = config.Connections.Where(c => c.FromTextureIndex == 3).FirstOrDefault();

            _current._input_r.Source = connection_0.FromTextureIndex != -1 ? config.Sources[0] : new PackerSource();
            _current._input_g.Source = connection_1.FromTextureIndex != -1 ? config.Sources[1] : new PackerSource();
            _current._input_b.Source = connection_2.FromTextureIndex != -1 ? config.Sources[2] : new PackerSource();
            _current._input_a.Source = connection_3.FromTextureIndex != -1 ? config.Sources[3] : new PackerSource();

            _current._input_r.FromOutputConfig(config.Targets[0]);
            _current._input_g.FromOutputConfig(config.Targets[1]);
            _current._input_b.FromOutputConfig(config.Targets[2]);
            _current._input_a.FromOutputConfig(config.Targets[3]);

            _current._input_r.Channel = connection_0.FromTextureIndex != -1 ? connection_0.FromChannel : TextureChannelIn.Max;
            _current._input_g.Channel = connection_1.FromTextureIndex != -1 ? connection_1.FromChannel : TextureChannelIn.Max;
            _current._input_b.Channel = connection_2.FromTextureIndex != -1 ? connection_2.FromChannel : TextureChannelIn.Max;
            _current._input_a.Channel = connection_3.FromTextureIndex != -1 ? connection_3.FromChannel : TextureChannelIn.Max;

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
            switch(Config.Instance.inlinePackerSaveLocation)
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
                    dir = Config.Instance.inlinePackerSaveLocationCustom;
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
            importer.crunchedCompression = Config.Instance.inlinePackerChrunchCompression;
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

        readonly static Vector4 s_RemappingDefault = new Vector4(0, 1, 0, 1);

        class InlinePackerChannelConfig
        {
            public PackerSource Source = new PackerSource();
            public bool Invert;
            public float Fallback;
            public TextureChannelIn Channel = TextureChannelIn.Max;
            public Vector4 Remapping = s_RemappingDefault;
            public RemapMode RemappingMode => Remapping == s_RemappingDefault ? RemapMode.None : RemapMode.RangeToRange;

            public OutputTarget ToOutputConfig()
            {
                return new OutputTarget(BlendMode.Add, Invert ? InvertMode.Invert : InvertMode.None, Fallback);
            }

            public void FromOutputConfig(OutputTarget config)
            {
                Invert = config.Invert == InvertMode.Invert;
                Fallback = config.Fallback;
            }

            public Texture2D GetTexture()
            {
                if (Source == null) return null;
                return Source.Texture;
            }
        }
    }

}