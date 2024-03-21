using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class TexturePacker : EditorWindow
    {
        const int MIN_WIDTH = 850;
        const int MIN_HEIGHT = 790;

        [MenuItem("Thry/Texture Packer", priority = 100)]
        public static TexturePacker ShowWindow()
        {
            TexturePacker packer = (TexturePacker)GetWindow(typeof(TexturePacker));
            packer.minSize = new Vector2(MIN_WIDTH, MIN_HEIGHT);
            packer.titleContent = new GUIContent("Thry Texture Packer");
            packer.OnSave = null; // clear save callback
            packer.OnChange = null; // clear save callback
            return packer;
        }

        [MenuItem("Assets/Thry/Open in Texture Packer")]
        public static void OpenInTexturePacker()
        {
            TexturePacker packer = ShowWindow();
            packer.InitilizeWithOneTexture(Selection.activeObject as Texture2D);
        }

        [MenuItem("Assets/Thry/Open in Texture Packer", true)]
        public static bool OpenInTexturePackerValidate()
        {
            return Selection.activeObject is Texture2D;
        }

#region DataStructures
        public enum TextureChannelIn { R, G, B, A, Max, None }
        public enum TextureChannelOut { R, G, B, A, None }
        public enum BlendMode { Add, Multiply, Max, Min }
        public enum InvertMode { None, Invert}
        public enum SaveType { PNG, JPG }
        public enum InputType { Texture, Color, Gradient }
        public enum GradientDirection { Horizontal, Vertical }
        public enum KernelPreset { None, Custom, EdgeDetection, Sharpen, GaussianBlur3x3, GaussianBlur5x5 }
        [Serializable]
        public class ImageAdjust
        {
            public float Brightness = 1;
            public float Hue = 0;
            public float Saturation = 1;
            public float Rotation = 0;
            public Vector2 Scale = Vector2.one;
            public Vector2 Offset = Vector2Int.zero;
            public bool ChangeCheck = false;
            public Vector2Int Resolution = new Vector2Int(16, 16);
        }
        static string GetTypeEnding(SaveType t)
        {
            switch (t)
            {
                case SaveType.PNG: return ".png";
                case SaveType.JPG: return ".jpg";
                default: return ".png";
            }
        }
        [Serializable]
        public class  OutputConfig
        {
            public BlendMode BlendMode;
            public InvertMode Invert;
            public float Fallback;

            public OutputConfig(float fallback = 0)
            {
                Fallback = fallback;
            }
        }

        [Serializable]
        public class TextureSource
        {
            public Texture2D Texture;
            public long LastHandledTextureEditTime;
            public FilterMode FilterMode;
            public Color Color;
            public Gradient Gradient;
            public GradientDirection GradientDirection;
            public Texture2D GradientTexture;
            public Texture2D TextureTexture;

            InputType _inputType = InputType.Texture;
            public InputType InputType
            {
                get
                {
                    return _inputType;
                }
                set
                {
                    if(_inputType != value)
                    {
                        _inputType = value;
                        if(_inputType == InputType.Texture) Texture = TextureTexture;
                        if(_inputType == InputType.Gradient) Texture = GradientTexture;
                     }
                }
            }

            public TextureSource()
            {
            }

            public TextureSource(Texture2D tex)
            {
                SetTexture(tex);
            }

            public void SetTexture(Texture2D tex)
            {
                Texture = tex;
                FilterMode = tex != null ? tex.filterMode : FilterMode.Bilinear;
            }

            public static void SetUncompressedTextureDirty(Texture2D tex)
            {
                if (_cachedUncompressedTextures.ContainsKey(tex))
                {
                    _cachedUncompressedTexturesNeedsReload[tex] = true;
                }
            }

            static Dictionary<Texture2D, Texture2D> _cachedUncompressedTextures = new Dictionary<Texture2D, Texture2D>();
            static Dictionary<Texture2D, bool> _cachedUncompressedTexturesNeedsReload = new Dictionary<Texture2D, bool>();
            public Texture2D UncompressedTexture
            {
                get
                {
                    if(_cachedUncompressedTextures.ContainsKey(Texture) == false || _cachedUncompressedTexturesNeedsReload[Texture])
                    {
                        string path = AssetDatabase.GetAssetPath(Texture);
                        if(path.EndsWith(".png") || path.EndsWith(".jpg"))
                        {
                            EditorUtility.DisplayProgressBar("Loading Raw PNG", "Loading " + path, 0.5f);
                            Texture2D tex = new Texture2D(2,2, TextureFormat.RGBA32, false, true);
                            tex.LoadImage(System.IO.File.ReadAllBytes(path));
                            tex.filterMode = Texture.filterMode;
                            _cachedUncompressedTextures[Texture] = tex;
                            EditorUtility.ClearProgressBar();
                        }else if (path.EndsWith(".tga"))
                        {
                            Texture2D tex = TextureHelper.LoadTGA(path, true);
                            tex.filterMode = Texture.filterMode;
                            _cachedUncompressedTextures[Texture] = tex;
                        }
                        else
                        {
                            _cachedUncompressedTextures[Texture] = Texture;
                        }
                        _cachedUncompressedTexturesNeedsReload[Texture] = false;
                    }
                    return _cachedUncompressedTextures[Texture];
                }
            }

            public void FindMaxSize(ref int width, ref int height)
            {
                if (Texture == null) return;
                width = Mathf.Max(width, UncompressedTexture.width);
                height = Mathf.Max(height, UncompressedTexture.height);
            }
        }

        [Serializable]
        public class Connection
        {
            public int FromTextureIndex = -1;
            public TextureChannelIn FromChannel = TextureChannelIn.None;
            public TextureChannelOut ToChannel = TextureChannelOut.None;

            public static Connection CreateFull(int index, TextureChannelIn channel, TextureChannelOut toChannel)
            {
                Connection connection = new Connection();
                connection.FromTextureIndex = index;
                connection.FromChannel = channel;
                connection.ToChannel = toChannel;
                return connection;
            }

            public static Connection Create(int index, TextureChannelIn channel)
            {
                Connection connection = new Connection();
                connection.FromTextureIndex = index;
                connection.FromChannel = channel;
                return connection;
            }

            public static Connection Create(TextureChannelOut channel)
            {
                Connection connection = new Connection();
                connection.ToChannel = channel;
                return connection;
            }

            public void SetFrom(int index, TextureChannelIn channel, TexturePacker packer)
            {
                // cancle if selecting same channel
                if(FromTextureIndex == index && FromChannel == channel)
                {
                    packer._creatingConnection = null;
                    return;
                }
                // set
                FromTextureIndex = index;
                FromChannel = channel;
                // check if done
                if(ToChannel == TextureChannelOut.None) return;
                // check if already exists
                if(packer._connections.Exists(c => c.ToChannel == ToChannel && c.FromTextureIndex == FromTextureIndex && c.FromChannel == FromChannel))
                {
                    packer._creatingConnection = null;
                    return;
                }
                packer._connections.Add(this);
                packer._creatingConnection = null;
                packer._changeCheckForPacking = true;
            }

            public void SetTo(TextureChannelOut channel, TexturePacker packer)
            {
                // cancle if selecting same channel
                if (ToChannel == channel)
                {
                    return;
                }
                // set
                ToChannel = channel;
                // check if done
                if(FromTextureIndex == -1 || FromChannel == TextureChannelIn.None) return;
                // check if already exists
                if(packer._connections.Exists(c => c.ToChannel == ToChannel && c.FromTextureIndex == FromTextureIndex && c.FromChannel == FromChannel))
                {
                    packer._creatingConnection = null;
                    return;
                }
                packer._connections.Add(this);
                packer._creatingConnection = null;
                packer._changeCheckForPacking = true;
            }

            Vector3 _bezierStart, _bezierEnd, _bezierStartTangent, _bezierEndTangent;

            public void CalculateBezierPoints(Vector2[] positionsIn, Vector2[] positionsOut)
            {
                _bezierStart = positionsIn[FromTextureIndex * 5 + (int)FromChannel];
                _bezierEnd = positionsOut[(int)ToChannel];
                _bezierStartTangent = _bezierStart + Vector3.right * 50;
                _bezierEndTangent = _bezierEnd + Vector3.left * 50;
            }

            public Vector3 BezierStart { get { return _bezierStart; } }
            public Vector3 BezierEnd { get { return _bezierEnd; } }
            public Vector3 BezierStartTangent { get { return _bezierStartTangent; } }
            public Vector3 BezierEndTangent { get { return _bezierEndTangent; } }
        }
        struct InteractionWithConnection
        {
            public Connection connection;
            public float distanceX;
        }
#endregion

        const string CHANNEL_PREVIEW_SHADER = "Hidden/Thry/ChannelPreview";

        TextureSource[] _textureSources = new TextureSource[]
        {
            new TextureSource(),
            new TextureSource(),
            new TextureSource(),
            new TextureSource(),
        };
        OutputConfig[] _outputConfigs = new OutputConfig[]
        {
            new OutputConfig(0),
            new OutputConfig(0),
            new OutputConfig(0),
            new OutputConfig(1),
        };

        Vector2 _scrollPosition;
        

        TexturePackerConfig _config;
        List<Connection> _connections = new List<Connection>();
        Connection _creatingConnection;
        Texture2D _outputTexture;
        ColorSpace _colorSpace = ColorSpace.Uninitialized;
        FilterMode _filterMode = FilterMode.Bilinear;

        string _saveFolder;
        string _saveName;
        SaveType _saveType = SaveType.PNG;
        float _saveQuality = 1;
        bool _showTransparency = true;
        bool _alphaIsTransparency = true;

        KernelPreset _kernelPreset = KernelPreset.None;
        bool _kernelEditHorizontal = true;
        float[] _kernel_x = GetKernelPreset(KernelPreset.None, true);
        float[] _kernel_y = GetKernelPreset(KernelPreset.None, false);
        int _kernel_loops = 1;
        float _kernel_strength = 1;
        bool _kernel_twoPass;
        bool _kernel_grayScale;
        bool[] _kernel_channels = new bool[4] { true, true, true, true };
        bool[] _channel_export = new bool[4] { true, true, true, true };

        ImageAdjust _imageAdjust = new ImageAdjust();

        public Action<Texture2D> OnSave;
        public Action<Texture2D, TextureSource[], OutputConfig[], Connection[]> OnChange;
        
        static Material s_channelPreviewMaterial;
        static Material ChannelPreviewMaterial
        {
            get
            {
                if (s_channelPreviewMaterial == null)
                {
                    s_channelPreviewMaterial = new Material(Shader.Find(CHANNEL_PREVIEW_SHADER));
                }
                return s_channelPreviewMaterial;
            }
        }

        static ComputeShader s_computeShader;
        static ComputeShader ComputeShader
        {
            get
            {
                if (s_computeShader == null)
                {
                    s_computeShader = AssetDatabase.LoadAssetAtPath<ComputeShader>(AssetDatabase.GUIDToAssetPath("56f54c5664777a747b2552701571174d"));
                }
                return s_computeShader;
            }
        }

        Vector2[] _positionsChannelIn = new Vector2[20];
        Vector2[] _positionsChannelOut = new Vector2[4];
        Rect[] _rectsChannelIn = new Rect[20];
        Rect[] _rectsChannelOut = new Rect[4];

        public void InitilizeWithData(TextureSource[] sources, OutputConfig[] configs, IEnumerable<Connection> connections, FilterMode filterMode, ColorSpace colorSpace, bool alphaIsTransparency)
        {
            _textureSources = new TextureSource[]
            {
                new TextureSource(),
                new TextureSource(),
                new TextureSource(),
                new TextureSource(),
            };
            Array.Copy(sources, _textureSources, sources.Length);
            _outputConfigs = configs;
            _connections = connections.ToList();
            _filterMode = filterMode;
            _colorSpace = colorSpace;
            _alphaIsTransparency = alphaIsTransparency;
            // Reset Color Adjust
            _imageAdjust = new ImageAdjust();
            DeterminePathAndFileNameIfEmpty(true);
            DetermineImportSettings();
            Pack();
        }

        void InitilizeWithOneTexture(Texture2D texture)
        {
            _connections.Clear();
            _textureSources[0].SetTexture(texture);
            _textureSources[1].SetTexture(texture);
            _textureSources[2].SetTexture(texture);
            _textureSources[3].SetTexture(texture);
            // Add connections
            _connections.Add(Connection.CreateFull(0, TextureChannelIn.R, TextureChannelOut.R));
            _connections.Add(Connection.CreateFull(1, TextureChannelIn.G, TextureChannelOut.G));
            _connections.Add(Connection.CreateFull(2, TextureChannelIn.B, TextureChannelOut.B));
            _connections.Add(Connection.CreateFull(3, TextureChannelIn.A, TextureChannelOut.A));
            // Reset Color Adjust
            _imageAdjust = new ImageAdjust();
            DeterminePathAndFileNameIfEmpty(true);
            DetermineImportSettings();
            DetermineOutputResolution(_textureSources, _imageAdjust);
            Pack();
        }

        const int TOP_OFFSET = 50;
        const int INPUT_PADDING = 20;
        const int OUTPUT_HEIGHT = 300;

        bool _changeCheckForPacking;
        private void OnGUI()
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            DrawConfigGUI();
            // Draw three texture slots on the left, a space in the middle, and one texutre slot on the right
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); 

            _changeCheckForPacking = false;

            GUILayout.BeginVertical();
            GUILayout.Space(TOP_OFFSET);
            bool didInputTexturesChange = false;
            didInputTexturesChange |= DrawInput( _textureSources[0], 0);
            GUILayout.Space(INPUT_PADDING);
            didInputTexturesChange |= DrawInput( _textureSources[1], 1);
            GUILayout.Space(INPUT_PADDING);
            didInputTexturesChange |= DrawInput( _textureSources[2], 2);
            GUILayout.Space(INPUT_PADDING);
            didInputTexturesChange |= DrawInput( _textureSources[3], 3);
            GUILayout.EndVertical();
            float inputHeight = 120 * 4 + INPUT_PADDING * 3 + TOP_OFFSET;

            GUILayout.Space(400);
            Rect rect_outputAndSettings = EditorGUILayout.BeginVertical();
            float output_y_offset = TOP_OFFSET + (inputHeight - TOP_OFFSET - OUTPUT_HEIGHT) / 2;
            GUILayout.Space(output_y_offset);
            DrawOutput(_outputTexture, OUTPUT_HEIGHT);

            EditorGUILayout.Space(15);
            Rect backgroundImageSettings = EditorGUILayout.BeginVertical();
            backgroundImageSettings = new RectOffset(5, 5, 5, 5).Add(backgroundImageSettings);
            GUI.DrawTexture(backgroundImageSettings, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 1, Styles.COLOR_BACKGROUND_1, 0, 10);

            EditorGUI.BeginChangeCheck();
            _colorSpace = (ColorSpace)EditorGUILayout.EnumPopup(_colorSpace);
            _filterMode = (FilterMode)EditorGUILayout.EnumPopup(_filterMode);
            _changeCheckForPacking |= EditorGUI.EndChangeCheck();

            // Make the sliders delayed, else the UX feels terrible
            EditorGUI.BeginChangeCheck();
            EventType eventTypeBeforerSliders  = Event.current.type;
            bool wasWide = EditorGUIUtility.wideMode;
            EditorGUIUtility.wideMode = true;
            _imageAdjust.Resolution = EditorGUILayout.Vector2IntField("Resolution", _imageAdjust.Resolution);
            _imageAdjust.Scale = EditorGUILayout.Vector2Field("Scale", _imageAdjust.Scale);
            _imageAdjust.Offset = EditorGUILayout.Vector2Field("Offset", _imageAdjust.Offset);
            _imageAdjust.Rotation = EditorGUILayout.Slider("Rotation", _imageAdjust.Rotation, -180, 180);
            _imageAdjust.Hue = EditorGUILayout.Slider("Hue", _imageAdjust.Hue, 0, 1);
            _imageAdjust.Saturation = EditorGUILayout.Slider("Saturation", _imageAdjust.Saturation, 0, 3);
            _imageAdjust.Brightness = EditorGUILayout.Slider("Brightness", _imageAdjust.Brightness, 0, 3);
            _imageAdjust.ChangeCheck |= EditorGUI.EndChangeCheck();
            EditorGUIUtility.wideMode = wasWide;
            if(_imageAdjust.ChangeCheck && (eventTypeBeforerSliders == EventType.MouseUp || (eventTypeBeforerSliders == EventType.KeyUp && Event.current.keyCode == KeyCode.Return)))
            {
                _changeCheckForPacking = true;
                _imageAdjust.ChangeCheck = false;
            }

            DrawKernelGUI();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace(); 
            GUILayout.EndHorizontal();

            DrawConnections();

            if(didInputTexturesChange)
            {
                DetermineImportSettings();
            }
            if(_changeCheckForPacking)
            {
                Pack();
                Repaint();
            }

            GUILayout.Space(20);
            DrawSaveGUI();
            GUILayout.EndVertical();

            HandleConnectionEditing();
            HandleConnectionCreation();

            GUILayout.EndScrollView();
        }

        void HandleConnectionEditing()
        {
            if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                InteractionWithConnection toEdit = CheckIfConnectionClicked();
                if(toEdit.connection != null)
                {
                    _connections.Remove(toEdit.connection);
                    // Remove the connection on one side
                    if(toEdit.distanceX > 0.5)
                        toEdit.connection.ToChannel = TextureChannelOut.None;
                    else
                        toEdit.connection.FromChannel = TextureChannelIn.None;
                    _creatingConnection = toEdit.connection;
                    Pack();
                    Repaint();
                }
            }
        }

        void HandleConnectionCreation()
        {
            // Connections are not nullable anymore, since they are serialized
            if(_creatingConnection != null && (_creatingConnection.FromChannel != TextureChannelIn.None || _creatingConnection.ToChannel != TextureChannelOut.None))
            {
                // if user clicked anywhere on the screen, stop creating the connection
                if(Event.current.type == EventType.MouseUp)
                {
                    // Check if mouse position is over any input / output slot
                    Vector2 mousePosition = Event.current.mousePosition;
                    for(int t = 0; t < 4; t++)
                    {
                        for(int c = 0; c < 5; c++)
                        {
                            if(_rectsChannelIn[t * 5 + c].Contains(mousePosition))
                            {
                                _creatingConnection.SetFrom(t, (TextureChannelIn)c, this);
                                Pack();
                                Repaint();
                                return;
                            }
                        }
                    }
                    for(int c = 0; c < 4; c++)
                    {
                        if(_rectsChannelOut[c].Contains(mousePosition))
                        {
                            _creatingConnection.SetTo((TextureChannelOut)c, this);
                            Pack();
                            Repaint();
                            return;
                        }
                    }
                    _creatingConnection = null;
                    return;
                }

                Vector2 bezierStart, bezierEnd, bezierStartTangent, bezierEndTangent;
                Color color = Color.white;

                bezierEnd = Event.current.mousePosition;

                if(_creatingConnection.FromChannel != TextureChannelIn.None)
                {
                    bezierStart = _positionsChannelIn[_creatingConnection.FromTextureIndex * 5 + (int)_creatingConnection.FromChannel];
                    bezierStartTangent = bezierStart + Vector2.right * 50;
                    bezierEndTangent = bezierEnd + Vector2.left * 50;
                    color = GetColor(_creatingConnection.FromChannel);
                }
                else
                {
                    bezierStart = _positionsChannelOut[(int)_creatingConnection.ToChannel];
                    bezierStartTangent = bezierStart + Vector2.left * 50;
                    bezierEndTangent = bezierEnd + Vector2.right * 50;
                    color = GetColor(_creatingConnection.ToChannel);
                }

                Handles.DrawBezier(bezierStart, bezierEnd, bezierStartTangent, bezierEndTangent, color, null, 2);
                Repaint();
            }
        }

        InteractionWithConnection CheckIfConnectionClicked()
        {
            Vector2 mousePos = Event.current.mousePosition;
            float minDistance = 50;
            InteractionWithConnection clickedConnection = new InteractionWithConnection();
            foreach(Connection c in _connections)
            {
                Vector3 from = c.BezierStart;
                Vector3 to = c.BezierEnd;
                float topY = Mathf.Max(from.y, to.y);
                float bottomY = Mathf.Min(from.y, to.y);
                float leftX = Mathf.Min(from.x, to.x);
                float rightX = Mathf.Max(from.x, to.x);
                // check if mouse is in the area of the bezier curve
                if(mousePos.x > leftX && mousePos.x < rightX)
                {
                    if(mousePos.y > bottomY && mousePos.y < topY)
                    {
                        // check if mouse is close to the bezier curve
                        float distance = HandleUtility.DistancePointBezier(mousePos, c.BezierStart, c.BezierEnd, c.BezierStartTangent, c.BezierEndTangent);
                        if(distance < 50)
                        {
                            if(distance < minDistance)
                            {
                                minDistance = distance;
                                clickedConnection.connection = c;
                                clickedConnection.distanceX = (mousePos.x - leftX) / (rightX - leftX);
                            }
                        }
                    }
                }
            }
            return clickedConnection;
        }

        void DrawConfigGUI()
        {
            Rect bg = new Rect(position.width / 2 - 150, 10, 300, 30);
            Rect rObjField = new RectOffset(5, 5, 5, 5).Remove(bg);
            GUI.DrawTexture(bg, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Styles.COLOR_BACKGROUND_1, 0, 10);

            if(_config == null)
            {
                rObjField = new RectOffset(0, 100, 0, 0).Remove(rObjField);
                Rect rButton = new Rect(rObjField.x + rObjField.width + 5, rObjField.y, 95, rObjField.height);
                if(GUI.Button(rButton, "New config"))
                {
                    CreateConfig();
                }
            }

            EditorGUI.BeginChangeCheck();
            _config = (TexturePackerConfig)EditorGUI.ObjectField(rObjField, _config, typeof(TexturePackerConfig), false);
            if(EditorGUI.EndChangeCheck())
            {
                LoadConfig();
                Pack();
                Repaint();
            }
        }

        void DrawKernelGUI()
        {
            Rect r_enum = EditorGUILayout.GetControlRect(false, 20);

            EditorGUI.BeginChangeCheck();
            _kernelPreset = (KernelPreset)EditorGUI.EnumPopup(r_enum, "Kernel Filter", _kernelPreset);
            if(EditorGUI.EndChangeCheck())
            {
                _kernel_x = _kernelPreset == KernelPreset.Custom ? _kernel_x : GetKernelPreset(_kernelPreset, true);
                _kernel_y = _kernelPreset == KernelPreset.Custom ? _kernel_y : GetKernelPreset(_kernelPreset, false);
                KernelPresetSetValues(_kernelPreset);
                Pack();
                Repaint();
            }
            
            this.minSize = new Vector2(MIN_WIDTH, _kernelPreset == KernelPreset.None ? MIN_HEIGHT : MIN_HEIGHT + 250);

            if(_kernelPreset != KernelPreset.None)
            {
                EventType eventTypeBeforerSliders = Event.current.type;
                _kernel_loops = EditorGUILayout.IntSlider("Loops", _kernel_loops, 1, 25);
                _kernel_strength = EditorGUILayout.Slider("Strength", _kernel_strength, 0, 1);
                _kernel_twoPass = EditorGUILayout.Toggle("Two Pass", _kernel_twoPass);
                _kernel_grayScale = EditorGUILayout.Toggle("Gray Scale", _kernel_grayScale);
                Rect r_channels = EditorGUILayout.GetControlRect(false, 20);
                r_channels.width /= 4;
                float prevLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 10;
                _kernel_channels[0] = EditorGUI.Toggle(r_channels, "R", _kernel_channels[0]);
                r_channels.x += r_channels.width;
                _kernel_channels[1] = EditorGUI.Toggle(r_channels, "G", _kernel_channels[1]);
                r_channels.x += r_channels.width;
                _kernel_channels[2] = EditorGUI.Toggle(r_channels, "B", _kernel_channels[2]);
                r_channels.x += r_channels.width;
                _kernel_channels[3] = EditorGUI.Toggle(r_channels, "A", _kernel_channels[3]);
                EditorGUIUtility.labelWidth = prevLabelWidth;
                if(Event.current.type == EventType.Used && (eventTypeBeforerSliders == EventType.MouseUp || (eventTypeBeforerSliders == EventType.KeyUp && Event.current.keyCode == KeyCode.Return)))
                {
                    Pack();
                    Repaint();
                }

                if(_kernel_twoPass)
                {
                    Rect r_buttons = EditorGUILayout.GetControlRect(false, 20);
                    if(GUI.Button( new Rect(r_buttons.x, r_buttons.y, r_buttons.width / 2, r_buttons.height), "X")) _kernelEditHorizontal = true;
                    if(GUI.Button( new Rect(r_buttons.x + r_buttons.width / 2, r_buttons.y, r_buttons.width / 2, r_buttons.height), "Y")) _kernelEditHorizontal = false;
                }

                Rect r = EditorGUILayout.GetControlRect(false, 130);
                EditorGUI.BeginChangeCheck();
                // draw 5x5 matrix inside the r_kernelX rect
                for(int x = 0; x < 5; x++)
                {
                    for(int y = 0; y < 5; y++)
                    {
                        Rect r_cell = new Rect(r.x + x * r.width / 5, r.y + y * r.height / 5, r.width / 5, r.height / 5);
                        if(_kernelEditHorizontal || !_kernel_twoPass) _kernel_x[x + y * 5] = EditorGUI.DelayedFloatField(r_cell, _kernel_x[x + y * 5]);
                        else _kernel_y[x + y * 5] = EditorGUI.DelayedFloatField(r_cell, _kernel_y[x + y * 5]);
                    }
                }
                if(EditorGUI.EndChangeCheck())
                {
                    _kernelPreset = KernelPreset.Custom;
                    Pack();
                    Repaint();
                }
            }
        }

        void DrawSaveGUI()
        {
            // Saving information
            // folder selection
            // determine folder & filename from asset name if not set
            if(string.IsNullOrEmpty(_saveFolder))
            {
                DeterminePathAndFileNameIfEmpty();
            }

            Rect r = EditorGUILayout.BeginHorizontal();

            Rect background = new Rect(r.x + r.width / 2 - 400, r.y - 5, 800, 97);
            GUI.DrawTexture(background, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Styles.COLOR_BACKGROUND_1, 0, 10);

            GUILayout.FlexibleSpace();
            // show current path
            GUILayout.Label("Save to: ");
            GUILayout.Label(_saveFolder + "\\");
            _saveName = GUILayout.TextField(_saveName, GUILayout.MinWidth(50));
            _saveType = (SaveType)EditorGUILayout.EnumPopup(_saveType, GUILayout.Width(70));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            _alphaIsTransparency = EditorGUILayout.Toggle("Alpha is Transparency", _alphaIsTransparency);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Change Folder", GUILayout.Width(100)))
            {
                string path = EditorUtility.OpenFolderPanel("Select folder", _saveFolder, "");
                if (!string.IsNullOrEmpty(path))
                {
                    // Make path relative to Assets folder
                    path = path.Replace(Application.dataPath, "Assets");
                    _saveFolder = path;
                }
            }
            if(GUILayout.Button("Save", GUILayout.Width(100)))
            {
                Save();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            _channel_export[0] = GUILayout.Toggle(_channel_export[0], "R", GUILayout.Width(26));
            _channel_export[1] = GUILayout.Toggle(_channel_export[1], "G", GUILayout.Width(26));
            _channel_export[2] = GUILayout.Toggle(_channel_export[2], "B", GUILayout.Width(26));
            _channel_export[3] = GUILayout.Toggle(_channel_export[3], "A", GUILayout.Width(26));
            if(GUILayout.Button("Export Channels", GUILayout.Width(130)))
            {
                ExportChannels(false);
            }
            if(GUILayout.Button("Export Channels (B&W)", GUILayout.Width(150)))
            {
                ExportChannels(true);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        void DeterminePathAndFileNameIfEmpty(bool forceOverwrite = false)
        {
            foreach(TextureSource s in _textureSources)
            {
                if(s.Texture != null)
                {
                    string path = AssetDatabase.GetAssetPath(s.Texture);
                    if(string.IsNullOrWhiteSpace(path))
                        continue;
                    if(string.IsNullOrWhiteSpace(_saveFolder) || forceOverwrite)
                        _saveFolder = Path.GetDirectoryName(path);
                    if(string.IsNullOrWhiteSpace(_saveName) || forceOverwrite)
                        _saveName = Path.GetFileNameWithoutExtension(path) + "_packed";
                    break;
                }
            }
        }

        void DetermineImportSettings()
        {
            _colorSpace = ColorSpace.Gamma;
            _filterMode = FilterMode.Bilinear;
            foreach(TextureSource s in _textureSources)
            {
                if(DetermineImportSettings(s))
                    break;
            }
        }

        bool DetermineImportSettings(TextureSource s)
        {
            if(s.Texture != null)
            {
                string path = AssetDatabase.GetAssetPath(s.Texture);
                if(path == null)
                    return false;
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if(importer != null)
                {
                    _colorSpace = importer.sRGBTexture ? ColorSpace.Gamma : ColorSpace.Linear;
                    _filterMode = importer.filterMode;
                    return true;
                }
            }
            return false;
        }

        void DrawConnections()
        {
            // Draw connections as lines
            foreach (Connection c in _connections)
            {
                c.CalculateBezierPoints(_positionsChannelIn, _positionsChannelOut);
                Handles.DrawBezier(c.BezierStart, c.BezierEnd, c.BezierStartTangent, c.BezierEndTangent, GetColor(c.FromChannel), null, 2);
            }
        }

        void DrawOutput(Texture2D texture, int height = 200)
        {
            int channelWidth = height / 4;

            Rect rect = GUILayoutUtility.GetRect(height, height);

            // draw 4 channl boxes on the left side
            Rect rectR = new Rect(rect.x - channelWidth, rect.y, channelWidth, channelWidth);
            Rect rectG = new Rect(rect.x - channelWidth, rect.y + channelWidth, channelWidth, channelWidth);
            Rect rectB = new Rect(rect.x - channelWidth, rect.y + channelWidth * 2, channelWidth, channelWidth);
            Rect rectA = new Rect(rect.x - channelWidth, rect.y + channelWidth * 3, channelWidth, channelWidth);

            // Draw circle button bext to each channel box
            int buttonWidth = 80;
            int buttonHeight = 40;
            Rect buttonR = new Rect(rectR.x - buttonWidth - 5, rectR.y + rectR.height / 2 - buttonHeight / 2, buttonWidth, buttonHeight);
            Rect buttonG = new Rect(rectG.x - buttonWidth - 5, rectG.y + rectG.height / 2 - buttonHeight / 2, buttonWidth, buttonHeight);
            Rect buttonB = new Rect(rectB.x - buttonWidth - 5, rectB.y + rectB.height / 2 - buttonHeight / 2, buttonWidth, buttonHeight);
            Rect buttonA = new Rect(rectA.x - buttonWidth - 5, rectA.y + rectA.height / 2 - buttonHeight / 2, buttonWidth, buttonHeight);

            // Draw background
            Rect background = new Rect(buttonR.x + 10, rect.y - 20, (rect.x + rect.width + 5) - (buttonR.x + 10), rect.height + 25);
            GUI.DrawTexture(background, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 1, Styles.COLOR_BACKGROUND_1, 0, 10);

            if(_showTransparency)
                EditorGUI.DrawTextureTransparent(rect, texture != null ? texture : Texture2D.blackTexture, ScaleMode.ScaleToFit, 1);
            else 
                EditorGUI.DrawPreviewTexture(rect, texture != null ? texture : Texture2D.blackTexture, null, ScaleMode.ScaleToFit);

            // Show transparency toggle
            Rect rectTransparency = new Rect(rect.x + 8, rect.y - 20, rect.width, 20);
            _showTransparency = EditorGUI.Toggle(rectTransparency, "Show Transparency", _showTransparency);

           // draw 4 channl boxes on the left side
            if (texture != null)
            {
                ChannelPreviewMaterial.SetTexture("_MainTex", texture);
                ChannelPreviewMaterial.SetFloat("_Channel", 0);
                EditorGUI.DrawPreviewTexture(rectR, texture, ChannelPreviewMaterial, ScaleMode.ScaleToFit);
                ChannelPreviewMaterial.SetFloat("_Channel", 1);
                EditorGUI.DrawPreviewTexture(rectG, texture, ChannelPreviewMaterial, ScaleMode.ScaleToFit);
                ChannelPreviewMaterial.SetFloat("_Channel", 2);
                EditorGUI.DrawPreviewTexture(rectB, texture, ChannelPreviewMaterial, ScaleMode.ScaleToFit);
                ChannelPreviewMaterial.SetFloat("_Channel", 3);
                EditorGUI.DrawPreviewTexture(rectA, texture, ChannelPreviewMaterial, ScaleMode.ScaleToFit);
            }
            else
            {
                EditorGUI.DrawRect(rectR, Color.black);
                EditorGUI.DrawRect(rectG, Color.black);
                EditorGUI.DrawRect(rectB, Color.black);
                EditorGUI.DrawRect(rectA, Color.black);
            }
            // Draw circle button bext to each channel box
            _positionsChannelOut[0] = new Vector2(buttonR.x, buttonR.y + buttonR.height / 2);
            _positionsChannelOut[1] = new Vector2(buttonG.x, buttonG.y + buttonG.height / 2);
            _positionsChannelOut[2] = new Vector2(buttonB.x, buttonB.y + buttonB.height / 2);
            _positionsChannelOut[3] = new Vector2(buttonA.x, buttonA.y + buttonA.height / 2);
            _rectsChannelOut[0] = buttonR;
            _rectsChannelOut[1] = buttonG;
            _rectsChannelOut[2] = buttonB;
            _rectsChannelOut[3] = buttonA;
            DrawOutputChannel(buttonR, TextureChannelOut.R, _outputConfigs[0]);
            DrawOutputChannel(buttonG, TextureChannelOut.G, _outputConfigs[1]);
            DrawOutputChannel(buttonB, TextureChannelOut.B, _outputConfigs[2]);
            DrawOutputChannel(buttonA, TextureChannelOut.A, _outputConfigs[3]);
        }

        void DrawOutputChannel(Rect position, TextureChannelOut channel, OutputConfig config)
        {
            // RGBA on the left side
            // fallback or (blendmode & invert) on the right side
            Rect channelRect = new Rect(position.x, position.y, 20, position.height);
            Rect fallbackRect = new Rect(position.x + 20, position.y, position.width - 20, position.height);
            Rect blendmodeRect = new Rect(fallbackRect.x, fallbackRect.y, fallbackRect.width, fallbackRect.height / 2);
            Rect invertRect = new Rect(fallbackRect.x, fallbackRect.y + fallbackRect.height / 2, fallbackRect.width, fallbackRect.height / 2);
            
            if(Event.current.type == EventType.MouseDown && channelRect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                if (_creatingConnection != null) _creatingConnection.SetTo(channel, this);
                else _creatingConnection = Connection.Create(channel);
            }
            GUI.Button(channelRect, channel.ToString());
            
            EditorGUI.BeginChangeCheck();
            if(DoFallback(channel))
            {
                config.Fallback = EditorGUI.FloatField(fallbackRect, config.Fallback);
            }else
            {
                config.BlendMode = (BlendMode)EditorGUI.EnumPopup(blendmodeRect, config.BlendMode);
                config.Invert = (InvertMode)EditorGUI.EnumPopup(invertRect, config.Invert);
            }
            _changeCheckForPacking |= EditorGUI.EndChangeCheck();
        }

        bool DrawInput(TextureSource texture, int index, int textureHeight = 100)
        {
            int channelWidth = textureHeight / 5;
            Rect rect = GUILayoutUtility.GetRect(textureHeight, textureHeight + 40);
            Rect typeRect = new Rect(rect.x, rect.y, textureHeight, 20);
            Rect textureRect = new Rect(rect.x, rect.y + 20, textureHeight, textureHeight);
            Rect filterRect = new Rect(textureRect.x, textureRect.y + textureHeight, textureRect.width, 20);

            Rect background = new Rect(rect.x - 5, rect.y - 5, rect.width + channelWidth + 40, rect.height + 10);
            GUI.DrawTexture(background, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 1, Styles.COLOR_BACKGROUND_1, 0, 10);

            // Draw textrue & filtermode. Change filtermode if texture is changed
            EditorGUI.BeginChangeCheck();
            texture.InputType = (InputType)EditorGUI.EnumPopup(typeRect, texture.InputType);
            bool didTextureChange = false;
            switch(texture.InputType)
            {
                case InputType.Texture:
                    EditorGUI.BeginChangeCheck();
                    texture.Texture = (Texture2D)EditorGUI.ObjectField(textureRect, texture.Texture, typeof(Texture2D), false);
                    didTextureChange = EditorGUI.EndChangeCheck();
                    if(didTextureChange && texture.Texture != null) texture.FilterMode = texture.Texture.filterMode;
                    if(didTextureChange) DetermineOutputResolution(_textureSources, _imageAdjust);
                    texture.FilterMode = (FilterMode)EditorGUI.EnumPopup(filterRect, texture.FilterMode);
                    break;
                case InputType.Gradient:
                    if(texture.GradientTexture == null) EditorGUI.DrawRect(textureRect, Color.black);
                    else EditorGUI.DrawPreviewTexture(textureRect, texture.GradientTexture);
                    if(Event.current.type == EventType.MouseDown && textureRect.Contains(Event.current.mousePosition))
                    {
                        if(texture.Gradient == null) texture.Gradient = new Gradient();
                        GradientEditor2.Open(texture.Gradient, (Gradient gradient, Texture2D tex) => {
                            texture.Gradient = gradient;
                            texture.GradientTexture = tex;
                            texture.Texture = tex;
                            // Needs to call these itself because it's in a callback not the OnGUI method
                            Pack();
                            Repaint();
                        }, texture.GradientDirection == GradientDirection.Vertical, false, _imageAdjust.Resolution, new Vector2Int(4096, 4096));

                    }
                    EditorGUI.BeginChangeCheck();
                    texture.GradientDirection = (GradientDirection)EditorGUI.EnumPopup(filterRect, texture.GradientDirection);
                    if(EditorGUI.EndChangeCheck() && texture.Gradient != null)
                    {
                        texture.GradientTexture = Converter.GradientToTexture(texture.Gradient, _imageAdjust.Resolution.x, _imageAdjust.Resolution.y, texture.GradientDirection == GradientDirection.Vertical);
                        texture.Texture = texture.GradientTexture;
                    }
                    break;
                case InputType.Color:
                    EditorGUI.BeginChangeCheck();
                    texture.Color = EditorGUI.ColorField(textureRect, texture.Color);
                    if(EditorGUI.EndChangeCheck())
                    {
                        texture.Texture = Converter.ColorToTexture(texture.Color, 16, 16);
                    }
                    break;
            }
            
            _changeCheckForPacking |= EditorGUI.EndChangeCheck();

            // draw 4 channl boxes on the right side
            Rect rectR = new Rect(textureRect.x + textureRect.width, textureRect.y, channelWidth, channelWidth);
            Rect rectG = new Rect(textureRect.x + textureRect.width, textureRect.y + channelWidth, channelWidth, channelWidth);
            Rect rectB = new Rect(textureRect.x + textureRect.width, textureRect.y + channelWidth * 2, channelWidth, channelWidth);
            Rect rectA = new Rect(textureRect.x + textureRect.width, textureRect.y + channelWidth * 3, channelWidth, channelWidth);
            Rect rectMax = new Rect(textureRect.x + textureRect.width, textureRect.y + channelWidth * 4, channelWidth, channelWidth);
            if (texture.Texture != null)
            {
                ChannelPreviewMaterial.SetTexture("_MainTex", texture.Texture);
                ChannelPreviewMaterial.SetFloat("_Channel", 0);
                EditorGUI.DrawPreviewTexture(rectR, texture.Texture, ChannelPreviewMaterial, ScaleMode.ScaleToFit);
                ChannelPreviewMaterial.SetFloat("_Channel", 1);
                EditorGUI.DrawPreviewTexture(rectG, texture.Texture, ChannelPreviewMaterial, ScaleMode.ScaleToFit);
                ChannelPreviewMaterial.SetFloat("_Channel", 2);
                EditorGUI.DrawPreviewTexture(rectB, texture.Texture, ChannelPreviewMaterial, ScaleMode.ScaleToFit);
                ChannelPreviewMaterial.SetFloat("_Channel", 3);
                EditorGUI.DrawPreviewTexture(rectA, texture.Texture, ChannelPreviewMaterial, ScaleMode.ScaleToFit);
                ChannelPreviewMaterial.SetFloat("_Channel", 4);
                EditorGUI.DrawPreviewTexture(rectMax, texture.Texture, ChannelPreviewMaterial, ScaleMode.ScaleToFit);
            }else
            {
                EditorGUI.DrawRect(rectR, Color.black);
                EditorGUI.DrawRect(rectG, Color.black);
                EditorGUI.DrawRect(rectB, Color.black);
                EditorGUI.DrawRect(rectA, Color.black);
                EditorGUI.DrawRect(rectMax, Color.black);
            }
            // Draw circle button bext to each channel box
            Rect circleR = new Rect(rectR.x + rectR.width + 5, rectR.y + rectR.height / 2 - 10 + 1, 40, 18);
            Rect circleG = new Rect(rectG.x + rectG.width + 5, rectG.y + rectG.height / 2 - 10 + 1, 40, 18);
            Rect circleB = new Rect(rectB.x + rectB.width + 5, rectB.y + rectB.height / 2 - 10 + 1, 40, 18);
            Rect circleA = new Rect(rectA.x + rectA.width + 5, rectA.y + rectA.height / 2 - 10 + 1, 40, 18);
            Rect circleMax = new Rect(rectMax.x + rectMax.width + 5, rectMax.y + rectMax.height / 2 - 10, 40, 18);
            _positionsChannelIn[index * 5 + 0] = new Vector2(circleR.x + circleR.width, circleR.y + circleR.height / 2);
            _positionsChannelIn[index * 5 + 1] = new Vector2(circleG.x + circleG.width, circleG.y + circleG.height / 2);
            _positionsChannelIn[index * 5 + 2] = new Vector2(circleB.x + circleB.width, circleB.y + circleB.height / 2);
            _positionsChannelIn[index * 5 + 3] = new Vector2(circleA.x + circleA.width, circleA.y + circleA.height / 2);
            _positionsChannelIn[index * 5 + 4] = new Vector2(circleMax.x + circleMax.width, circleMax.y + circleMax.height / 2);
            _rectsChannelIn[index * 5 + 0] = circleR;
            _rectsChannelIn[index * 5 + 1] = circleG;
            _rectsChannelIn[index * 5 + 2] = circleB;
            _rectsChannelIn[index * 5 + 3] = circleA;
            _rectsChannelIn[index * 5 + 4] = circleMax;
            DrawInputChannel(circleR, index, TextureChannelIn.R);
            DrawInputChannel(circleG, index, TextureChannelIn.G);
            DrawInputChannel(circleB, index, TextureChannelIn.B);
            DrawInputChannel(circleA, index, TextureChannelIn.A);
            DrawInputChannel(circleMax, index, TextureChannelIn.Max);

            return didTextureChange;
        }

        void DrawInputChannel(Rect position, int index, TextureChannelIn channel)
        {
            if(Event.current.type == EventType.MouseDown && position.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                if (_creatingConnection == null) _creatingConnection = Connection.Create(index, channel);
                else _creatingConnection.SetFrom(index, channel, this);
            }
            GUI.Button(position, channel.ToString());
        }

        

        bool DoFallback(TextureChannelOut channel)
        {
            return _connections.Any(c => c.ToChannel == channel && c.FromTextureIndex != -1
                && _textureSources[c.FromTextureIndex].Texture != null) == false;
        }

        Color GetColor(TextureChannelIn c)
        {
            switch (c)
            {
                case TextureChannelIn.R: return Color.red;
                case TextureChannelIn.G: return Color.green;
                case TextureChannelIn.B: return Color.blue;
                case TextureChannelIn.A: return Color.white;
                case TextureChannelIn.Max: return Color.yellow;
                default: return Color.black;
            }
        }

        Color GetColor(TextureChannelOut c)
        {
            switch (c)
            {
                case TextureChannelOut.R: return Color.red;
                case TextureChannelOut.G: return Color.green;
                case TextureChannelOut.B: return Color.blue;
                case TextureChannelOut.A: return Color.white;
                default: return Color.black;
            }
        }

        // Packing Logic

        void Pack()
        {
            SaveConfig();
            // Update all gradient textures (incase max size changed)
            Vector2Int gradientSize = _imageAdjust.Resolution;
            foreach (TextureSource source in _textureSources)
            {
                if (source.InputType == InputType.Gradient && source.GradientTexture != null && source.GradientTexture.width != gradientSize.x && source.GradientTexture.height != gradientSize.y)
                {
                    source.GradientTexture = Converter.GradientToTexture(source.Gradient, gradientSize.x, gradientSize.y, source.GradientDirection == GradientDirection.Vertical);
                    source.Texture = source.GradientTexture;
                }
            }

            _outputTexture = Pack(_textureSources, _outputConfigs, _connections, _filterMode, _colorSpace, _imageAdjust, _kernel_x, _kernel_y, _kernel_loops, _kernel_strength, _kernel_twoPass, _kernel_grayScale, _kernel_channels, _alphaIsTransparency);
            if(OnChange != null) OnChange(_outputTexture, _textureSources, _outputConfigs, _connections.ToArray());
        }

        static void DetermineOutputResolution(TextureSource[] sources, ImageAdjust colorAdjust)
        {
            int width = 16;
            int height = 16;
            foreach (TextureSource source in sources)
            {
                source.FindMaxSize(ref width, ref height);
            }
            // round up to nearest power of 2
            width = Mathf.NextPowerOfTwo(width);
            height = Mathf.NextPowerOfTwo(height);
            // clamp to max size of 4096
            width = Mathf.Clamp(width, 16, 4096);
            height = Mathf.Clamp(height, 16, 4096);
            colorAdjust.Resolution = new Vector2Int(width, height);
        }

        public static Texture2D Pack(TextureSource[] sources, OutputConfig[] outputConfigs, IEnumerable<Connection> connections, FilterMode targetFilterMode, ColorSpace targetColorSpace, ImageAdjust colorAdjust = null,
            float[] kernelX = null, float[] kernelY = null, int kernelLoops = 1, float kernelStrength = 1, bool kernelTwoPass = false, bool kernelGrayscale = false, bool[] kernelChannels = null, bool alphaIsTransparency = false)
        {
            if(colorAdjust == null)
            {
                colorAdjust = new ImageAdjust();
                DetermineOutputResolution(sources, colorAdjust);
            }
            int width = colorAdjust.Resolution.x;
            int height = colorAdjust.Resolution.y;
            

            RenderTexture target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.filterMode = targetFilterMode;
            target.Create();

            ComputeShader.SetTexture(0, "Result", target);
            ComputeShader.SetFloat("Width", width);
            ComputeShader.SetFloat("Height", height);

            ComputeShader.SetFloat("Rotation", colorAdjust.Rotation / 360f * 2f * Mathf.PI);
            ComputeShader.SetVector("Scale", colorAdjust.Scale);
            ComputeShader.SetVector("Offset", colorAdjust.Offset);
            ComputeShader.SetFloat("Hue", colorAdjust.Hue);
            ComputeShader.SetFloat("Saturation", colorAdjust.Saturation);
            ComputeShader.SetFloat("Brightness", colorAdjust.Brightness);

            bool repeatTextures = Math.Abs(colorAdjust.Scale.x) > 1 || Math.Abs(colorAdjust.Scale.y) > 1;

            // Set Compute Shader Properties
            int rCons = SetComputeValues(sources, connections, outputConfigs[0], TextureChannelOut.R, repeatTextures);
            int gCons = SetComputeValues(sources, connections, outputConfigs[1], TextureChannelOut.G, repeatTextures);
            int bCons = SetComputeValues(sources, connections, outputConfigs[2], TextureChannelOut.B, repeatTextures);
            int aCons = SetComputeValues(sources, connections, outputConfigs[3], TextureChannelOut.A, repeatTextures);

            ComputeShader.Dispatch(0, width / 8 + 1, height / 8 + 1, 1);

            if(kernelX != null && kernelY != null)
            {
                // Settings Vector4s instead of floats because the SetFloats function is broken
                float[] kernelNone = GetKernelPreset(KernelPreset.None, false);
                ComputeShader.SetVectorArray("Kernel_X", kernelX.Select((f,i) => new Vector4(Mathf.Lerp(kernelNone[i], f, kernelStrength), 0, 0, 0)).ToArray());
                ComputeShader.SetVectorArray("Kernel_Y", kernelY.Select((f,i) => new Vector4(Mathf.Lerp(kernelNone[i], f, kernelStrength), 0, 0, 0)).ToArray());
                ComputeShader.SetBool("Kernel_Grayscale", kernelGrayscale);
                ComputeShader.SetBool("Kernel_TwoPass", kernelTwoPass);
                ComputeShader.SetVector("Kernel_Channels", new Vector4(kernelChannels[0] ? 1 : 0, kernelChannels[1] ? 1 : 0, kernelChannels[2] ? 1 : 0, kernelChannels[3] ? 1 : 0));

                // define the opposite way, because each loop flips it
                RenderTexture filterTarget = target;

                RenderTexture filterInput = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                filterInput.enableRandomWrite = true;
                filterInput.filterMode = targetFilterMode;
                filterInput.Create();

                for(int i = 0; i < kernelLoops; i++)
                {
                    RenderTexture temp = filterInput;
                    filterInput = filterTarget;
                    filterTarget = temp;

                    ComputeShader.SetTexture(1, "Kernel_Input", filterInput);
                    ComputeShader.SetTexture(1, "Result", filterTarget);
                    ComputeShader.Dispatch(1, width / 8 + 1, height / 8 + 1, 1);
                }

                target = filterTarget;
            }

            Texture2D atlas = new Texture2D(width, height, TextureFormat.RGBA32, true, targetColorSpace == ColorSpace.Linear);
            RenderTexture.active = target;
            atlas.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            atlas.filterMode = targetFilterMode;
            atlas.wrapMode = TextureWrapMode.Clamp;
            atlas.alphaIsTransparency = alphaIsTransparency;
            atlas.Apply();

            return atlas;
        }

        static int SetComputeValues(TextureSource[] sources, IEnumerable<Connection> allConnections, OutputConfig config, TextureChannelOut outChannel, bool repeatMode)
        {
            // Find all incoming connections
            Connection[] chnlConnections = allConnections.Where(c => c.ToChannel == outChannel && sources[c.FromTextureIndex].Texture != null).ToArray();
            
            // Set textures
            for(int i = 0; i < chnlConnections.Length; i++)
            {
                TextureSource s = sources[chnlConnections[i].FromTextureIndex];
                // set the sampler states correctly
                s.UncompressedTexture.wrapMode = repeatMode ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
                s.UncompressedTexture.filterMode = s.InputType == InputType.Gradient ? FilterMode.Bilinear : s.FilterMode;
                ComputeShader.SetTexture(0, outChannel.ToString() + "_Input_" + i, s.UncompressedTexture);
                ComputeShader.SetInt(outChannel.ToString() + "_Channel_" + i, (int)chnlConnections[i].FromChannel);
            }
            for(int i = chnlConnections.Length; i < 4; i++)
            {
                ComputeShader.SetTexture(0, outChannel.ToString() + "_Input_" + i, Texture2D.whiteTexture);
            }

            // Set other data
            ComputeShader.SetInt(outChannel.ToString() + "_Count", chnlConnections.Length);
            ComputeShader.SetInt(outChannel.ToString() + "_BlendMode", (int)config.BlendMode);
            ComputeShader.SetBool(outChannel.ToString() + "_Invert", config.Invert == InvertMode.Invert);
            ComputeShader.SetFloat(outChannel.ToString() + "_Fallback", config.Fallback);

            return chnlConnections.Length;
        }

#region  Kernel

        
        static float[] GetKernelPreset(KernelPreset preset, bool isXKernel)
        {
            // return a 5x5 kernel. always 25 values
            switch(preset)
            {
                case KernelPreset.Sharpen: return new float[]{ 0,0,0,0,0, 0,0,-0.5f, 0,0, 0,-0.5f, 3, -0.5f, 0, 0,0, -0.5f, 0,0, 0,0,0,0,0 };
                case KernelPreset.EdgeDetection:
                    if (isXKernel) return new float[]{ 0,0,0,0,0, 0,-1,0,1,0, 0,-2,0,2,0, 0,-1,0,1,0, 0,0,0,0,0 };
                    else return new float[]{ 0,0,0,0,0, 0,-1,-2,-1,0, 0,0,0,0,0, 0,1,2,1,0, 0,0,0,0,0 };
                case KernelPreset.GaussianBlur3x3: return new float[]{ 0,0,0,0,0, 0,0.0625f,0.125f,0.0625f,0, 0,0.125f,0.25f,0.125f,0, 0,0.0625f,0.125f,0.0625f,0, 0,0,0,0,0 };
                case KernelPreset.GaussianBlur5x5: return new float[]{ 0.003f, 0.0133f, 0.0219f, 0.0133f, 0.003f, 0.0133f, 0.0596f, 0.0983f, 0.0596f, 0.0133f, 0.0219f, 0.0983f, 0.1621f, 0.0983f, 0.0219f, 0.0133f, 0.0596f, 0.0983f, 0.0596f, 0.0133f, 0.003f, 0.0133f, 0.0219f, 0.0133f, 0.003f };
            }
            return new float[]{ 0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0 };
        }

        void KernelPresetSetValues(KernelPreset preset)
        {
            _kernel_loops = 1;
            _kernel_strength = 1;
            _kernel_twoPass = false;
            _kernel_grayScale = false;
            _kernel_channels = new bool[]{ true, true, true, true };
            if(preset == KernelPreset.GaussianBlur3x3 || preset == KernelPreset.GaussianBlur5x5) _kernel_loops = 10;
            if(preset == KernelPreset.EdgeDetection) _kernel_twoPass = true;
            if(preset == KernelPreset.EdgeDetection) _kernel_channels = new bool[]{ true, true, true, false };
        }

# endregion

# region Channel Unpacker



        void ExportChannel(RenderTexture renderTex, Vector4 lerpR, Vector4 lerpG, Vector4 lerpB, Vector4 lerpA , string namePostfix)
        {
            ComputeShader.SetVector("Channels_Strength_R", lerpR);
            ComputeShader.SetVector("Channels_Strength_G", lerpG);
            ComputeShader.SetVector("Channels_Strength_B", lerpB);
            ComputeShader.SetVector("Channels_Strength_A", lerpA);
            ComputeShader.Dispatch(2, _outputTexture.width / 8, _outputTexture.height / 8, 1);

            Texture2D tex = new Texture2D(renderTex.width, renderTex.height, TextureFormat.RGBA32, true, _colorSpace == ColorSpace.Linear);
            RenderTexture.active = renderTex;
            tex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            tex.filterMode = renderTex.filterMode;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.alphaIsTransparency = false;
            tex.Apply();

            Save(tex, _saveFolder, _saveName + namePostfix);
        }

        void ExportChannels(bool exportAsBlackAndWhite)
        {
            Pack();
            DeterminePathAndFileNameIfEmpty();

            RenderTexture target = new RenderTexture(_outputTexture.width, _outputTexture.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.filterMode = _outputTexture.filterMode;
            target.Create();
            ComputeShader.SetTexture(2, "Unpacker_Input", _outputTexture);
            ComputeShader.SetTexture(2, "Result", target);

            Vector4 r = new Vector4(1, 0, 0, 0);
            Vector4 g = new Vector4(0, 1, 0, 0);
            Vector4 b = new Vector4(0, 0, 1, 0);
            Vector4 a = new Vector4(0, 0, 0, 1);
            Vector4 none = new Vector4(0, 0, 0, 0);
            if(exportAsBlackAndWhite)
            {
                if(_channel_export[0])
                    ExportChannel(target, r, r, r, none, "_R");
                if(_channel_export[1])
                    ExportChannel(target, g, g, g, none, "_G");
                if(_channel_export[2])
                    ExportChannel(target, b, b, b, none, "_B");
                if(_channel_export[3])
                    ExportChannel(target, a, a, a, none, "_A");
            }
            else
            {
                if(_channel_export[0])
                    ExportChannel(target, r, none, none, none, "_R");
                if(_channel_export[1])
                    ExportChannel(target, none, g, none, none, "_G");
                if(_channel_export[2])
                    ExportChannel(target, none, none, b, none, "_B");
                if(_channel_export[3])
                    ExportChannel(target, none, none, none, a, "_A");
            }
        }

# endregion

#region Save
        void Save()
        {
            if (_outputTexture == null) return;
            DeterminePathAndFileNameIfEmpty();
            string path = _saveFolder + "/" + _saveName + GetTypeEnding(_saveType);
            byte[] bytes = null;
            if(File.Exists(path))
            {
                // open dialog
                if (!EditorUtility.DisplayDialog("File already exists", "Do you want to overwrite the file?", "Yes", "No"))
                {
                    return;
                }
            }
            switch (_saveType)
            {
                case SaveType.PNG: bytes = _outputTexture.EncodeToPNG(); break;
                case SaveType.JPG: bytes = _outputTexture.EncodeToJPG((int)_saveQuality); break;
            }
            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.streamingMipmaps = true;
            importer.sRGBTexture = _colorSpace == ColorSpace.Gamma;
            importer.filterMode = _filterMode;
            importer.alphaIsTransparency = _alphaIsTransparency;
            importer.textureCompression = TextureImporterCompression.Compressed;
            TextureImporterFormat overwriteFormat = importer.DoesSourceTextureHaveAlpha() ? 
                Config.Singleton.texturePackerCompressionWithAlphaOverwrite : Config.Singleton.texturePackerCompressionNoAlphaOverwrite;
            if(overwriteFormat != TextureImporterFormat.Automatic)
            {
                importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
                {
                    name = "PC",
                    overridden = true,
                    maxTextureSize = 2048,
                    format = overwriteFormat
                });
            }else
            {
                importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
                {
                    name = "PC",
                    overridden = false,
                });
            }
            importer.SaveAndReimport();

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if(OnSave != null) OnSave(tex);
        }
        
        void Save(Texture2D texture, string folder, string name)
        {
            string path = folder + "/" + name + GetTypeEnding(_saveType);
            byte[] bytes = null;
            if(File.Exists(path))
            {
                // open dialog
                if (!EditorUtility.DisplayDialog("File already exists", "Do you want to overwrite the file?", "Yes", "No"))
                {
                    return;
                }
            }
            switch (_saveType)
            {
                case SaveType.PNG: bytes = texture.EncodeToPNG(); break;
                case SaveType.JPG: bytes = texture.EncodeToJPG((int)_saveQuality); break;
            }
            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.streamingMipmaps = true;
            importer.sRGBTexture = _colorSpace == ColorSpace.Gamma;
            importer.filterMode = _filterMode;
            importer.alphaIsTransparency = texture.alphaIsTransparency;
            importer.textureCompression = TextureImporterCompression.Compressed;
            TextureImporterFormat overwriteFormat = importer.DoesSourceTextureHaveAlpha() ? 
                Config.Singleton.texturePackerCompressionWithAlphaOverwrite : Config.Singleton.texturePackerCompressionNoAlphaOverwrite;
            if(overwriteFormat != TextureImporterFormat.Automatic)
            {
                importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
                {
                    name = "PC",
                    overridden = true,
                    maxTextureSize = 2048,
                    format = overwriteFormat
                });
            }else
            {
                importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
                {
                    name = "PC",
                    overridden = false,
                });
            }
            importer.SaveAndReimport();
        }

        void CreateConfig()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Texture Packer Config", "TexturePackerConfig", "asset", "Save Texture Packer Config", _saveFolder);
            if (path.Length != 0)
            {
                _config = ScriptableObject.CreateInstance<TexturePackerConfig>();
                AssetDatabase.CreateAsset(_config, path);
                SaveConfig();
            }
        }

        void SaveConfig()
        {
            if (_config == null) return;
            _config.Sources = _textureSources;
            _config.Connections = _connections.ToArray();
            _config.OutputConfigs = _outputConfigs;
            _config.SaveFolder = _saveFolder;
            _config.SaveName = _saveName;
            _config.SaveType = _saveType;
            _config.SaveQuality = _saveQuality;
            _config.ColorSpace = _colorSpace;
            _config.FilterMode = _filterMode;
            _config.ImageAdjust = _imageAdjust;

            _config.KernelPreset = _kernelPreset;
            _config.KernelX = _kernel_x;
            _config.KernelY = _kernel_y;
            _config.KernelLoops = _kernel_loops;
            _config.KernelStrength = _kernel_strength;
            _config.KernelChannels = _kernel_channels;
            EditorUtility.SetDirty(_config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        void LoadConfig()
        {
            if(_config == null) return;
            _textureSources = _config.Sources;
            _connections = _config.Connections.ToList();
            _outputConfigs = _config.OutputConfigs;
            _saveFolder = _config.SaveFolder;
            _saveName = _config.SaveName;
            _saveType = _config.SaveType;
            _saveQuality = _config.SaveQuality;
            _colorSpace = _config.ColorSpace;
            _filterMode = _config.FilterMode;
            _imageAdjust = _config.ImageAdjust;

            _kernelPreset = _config.KernelPreset;
            _kernel_x = _config.KernelX;
            _kernel_y = _config.KernelY;
            _kernel_loops = _config.KernelLoops;
            _kernel_strength = _config.KernelStrength;
            _kernel_channels = _config.KernelChannels;
            // Expand sources to 4
            _textureSources = _textureSources.Concat(Enumerable.Repeat(new TextureSource(), 4 - _textureSources.Length)).ToArray();
            // Expand output configs to 4
            _outputConfigs = _outputConfigs.Concat(Enumerable.Repeat(new OutputConfig(), 4 - _outputConfigs.Length)).ToArray();

        }

#endregion
    }
}