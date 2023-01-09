using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class TexturePacker : EditorWindow
    {
        [MenuItem("Thry/Texture Packer")]
        public static TexturePacker ShowWindow()
        {
            TexturePacker packer = (TexturePacker)GetWindow(typeof(TexturePacker));
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
        static string GetTypeEnding(SaveType t)
        {
            switch (t)
            {
                case SaveType.PNG: return ".png";
                case SaveType.JPG: return ".jpg";
                default: return ".png";
            }
        }
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

        public class TextureSource
        {
            public Texture2D Texture;
            Texture2D _loadedUnityTexture;
            Texture2D _loadedUncompressedTexture;
            public long LastHandledTextureEditTime;
            public bool DoReloadUncompressedTexture;
            public FilterMode FilterMode;

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
                DoReloadUncompressedTexture = true;
            }

            public Texture2D UncompressedTexture
            {
                get
                {
                    if (_loadedUnityTexture != Texture || _loadedUncompressedTexture == null || DoReloadUncompressedTexture)
                    {
                        string path = AssetDatabase.GetAssetPath(Texture);
                        if(path.EndsWith(".png") || path.EndsWith(".jpg"))
                        {
                            _loadedUncompressedTexture = new Texture2D(Texture.width, Texture.height, TextureFormat.ARGB32, false, true);
                            ImageConversion.LoadImage(_loadedUncompressedTexture, System.IO.File.ReadAllBytes(path));
                            _loadedUncompressedTexture.filterMode = Texture.filterMode;
                        }else if (path.EndsWith(".tga"))
                        {
                            _loadedUncompressedTexture = TextureHelper.LoadTGA(path);
                            _loadedUncompressedTexture.filterMode = Texture.filterMode;
                        }
                        else
                        {
                            _loadedUncompressedTexture = Texture;
                        }
                        _loadedUnityTexture = Texture;
                    }
                    return _loadedUncompressedTexture;
                }
            }

            public void FindMaxSize(ref int width, ref int height)
            {
                if (Texture == null) return;
                width = Mathf.Max(width, UncompressedTexture.width);
                height = Mathf.Max(height, UncompressedTexture.height);
            }
        }

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
                // remove if already exists
                int rm = packer._connections.RemoveAll(c => c.ToChannel == ToChannel && c.FromTextureIndex == FromTextureIndex && c.FromChannel == FromChannel);
                // Add new connection if not removed
                if(rm == 0) packer._connections.Add(this);
                packer._creatingConnection = null;
            }

            public void SetTo(TextureChannelOut channel, TexturePacker packer)
            {
                // cancle if selecting same channel
                if (ToChannel == channel)
                {
                    packer._creatingConnection = null;
                    return;
                }
                // set
                ToChannel = channel;
                // check if done
                if(FromTextureIndex == -1 || FromChannel == TextureChannelIn.None) return;
                // remove if already exists
                int rm = packer._connections.RemoveAll(c => c.ToChannel == ToChannel && c.FromTextureIndex == FromTextureIndex && c.FromChannel == FromChannel);
                // Add new connection if not removed
                if(rm == 0) packer._connections.Add(this);
                packer._creatingConnection = null;
            }
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
        

        List<Connection> _connections = new List<Connection>();
        Connection _creatingConnection;
        Texture2D _outputTexture;
        ColorSpace _colorSpace = ColorSpace.Uninitialized;
        FilterMode _filterMode = FilterMode.Bilinear;

        string _saveFolder;
        string _saveName;
        SaveType _saveType = SaveType.PNG;
        float _saveQuality = 1;

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

        public void InitilizeWithData(TextureSource[] sources, OutputConfig[] configs, IEnumerable<Connection> connections, FilterMode filterMode, ColorSpace colorSpace)
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
            Pack();
            DeterminePath();
            DetermineImportSettings(_textureSources[0]);
        }

        void InitilizeWithOneTexture(Texture2D texture)
        {
            _textureSources[0].SetTexture(texture);
            _textureSources[1].SetTexture(texture);
            _textureSources[2].SetTexture(texture);
            _textureSources[3].SetTexture(texture);
            // Add connections
            _connections.Add(Connection.CreateFull(0, TextureChannelIn.R, TextureChannelOut.R));
            _connections.Add(Connection.CreateFull(1, TextureChannelIn.G, TextureChannelOut.G));
            _connections.Add(Connection.CreateFull(2, TextureChannelIn.B, TextureChannelOut.B));
            _connections.Add(Connection.CreateFull(3, TextureChannelIn.A, TextureChannelOut.A));
            Pack();
            DeterminePath();
            DetermineImportSettings(_textureSources[0]);
        }

        const int TOP_OFFSET = 50;
        const int INPUT_PADDING = 20;
        const int OUTPUT_HEIGHT = 300;

        private void OnGUI()
        {
            // Draw three texture slots on the left, a space in the middle, and one texutre slot on the right
            GUILayout.BeginVertical();
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace(); 

            GUILayout.BeginVertical();
            GUILayout.Space(TOP_OFFSET);
            bool didFirstTexChange = DrawInput( _textureSources[0], 0);
            GUILayout.Space(INPUT_PADDING);
            DrawInput( _textureSources[1], 1);
            GUILayout.Space(INPUT_PADDING);
            DrawInput( _textureSources[2], 2);
            GUILayout.Space(INPUT_PADDING);
            DrawInput( _textureSources[3], 3);
            GUILayout.EndVertical();
            float inputHeight = 120 * 4 + INPUT_PADDING * 3 + TOP_OFFSET;

            GUILayout.Space(400);
            GUILayout.BeginVertical();
            GUILayout.Space(TOP_OFFSET);
            GUILayout.Space((inputHeight - TOP_OFFSET - OUTPUT_HEIGHT) / 2);
            DrawOutput(_outputTexture, OUTPUT_HEIGHT);
            _colorSpace = (ColorSpace)EditorGUILayout.EnumPopup(_colorSpace);
            _filterMode = (FilterMode)EditorGUILayout.EnumPopup(_filterMode);
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace(); 
            GUILayout.EndHorizontal();

            DrawConnections();

            if(didFirstTexChange)
            {
                DetermineImportSettings(_textureSources[0]); // Import Settings are based on the first texture
            }
            if(EditorGUI.EndChangeCheck())
            {
                Pack();
            }

            GUILayout.Space(20);
            DrawSaveGUI();
            GUILayout.EndVertical();
        }

        void DrawSaveGUI()
        {
            // Saving information
            // folder selection
            // determine folder & filename from asset name if not set
            if(string.IsNullOrEmpty(_saveFolder) || string.IsNullOrEmpty(_saveName))
            {
                DeterminePath();
            }

            // show current path
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Save to: ");
            GUILayout.Label(_saveFolder + "\\");
            _saveName = GUILayout.TextField(_saveName);
            _saveType = (SaveType)EditorGUILayout.EnumPopup(_saveType, GUILayout.Width(70));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("Change Folder", GUILayout.Width(100)))
            {
                string path = EditorUtility.OpenFolderPanel("Select folder", _saveFolder, "");
                if (!string.IsNullOrEmpty(path))
                {
                    _saveFolder = path;
                }
            }
            if(GUILayout.Button("Save", GUILayout.Width(100)))
            {
                Save();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        void DeterminePath()
        {
            foreach(TextureSource s in _textureSources)
            {
                if(s.Texture != null)
                {
                    string path = AssetDatabase.GetAssetPath(s.Texture);
                    _saveFolder = Path.GetDirectoryName(path);
                    _saveName = Path.GetFileNameWithoutExtension(path) + "_packed";
                    break;
                }
            }
        }

        void DetermineImportSettings(TextureSource s)
        {
            if(s.Texture != null)
            {
                TextureImporter importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(s.Texture)) as TextureImporter;
                if(importer != null)
                {
                    _colorSpace = importer.sRGBTexture ? ColorSpace.Gamma : ColorSpace.Linear;
                    _filterMode = importer.filterMode;
                }
            }
        }

        void DrawConnections()
        {
            // Draw connections as lines
            foreach (var connection in _connections)
            {
                Vector3 from = _positionsChannelIn[connection.FromTextureIndex * 5 + (int)connection.FromChannel];
                Vector3 to = _positionsChannelOut[(int)connection.ToChannel];
                Handles.DrawBezier(from, to, from + Vector3.right * 50, to + Vector3.left * 50, GetColor(connection.FromChannel), null, 2);
            }
        }

        void DrawOutput(Texture2D texture, int height = 200)
        {
            Rect rect = GUILayoutUtility.GetRect(height, height);
            EditorGUI.DrawTextureTransparent(rect, texture != null ? texture : Texture2D.blackTexture, ScaleMode.ScaleToFit, 1);
           // draw 4 channl boxes on the left side
            int channelWidth = height / 4;
            Rect rectR = new Rect(rect.x - channelWidth, rect.y, channelWidth, channelWidth);
            Rect rectG = new Rect(rect.x - channelWidth, rect.y + channelWidth, channelWidth, channelWidth);
            Rect rectB = new Rect(rect.x - channelWidth, rect.y + channelWidth * 2, channelWidth, channelWidth);
            Rect rectA = new Rect(rect.x - channelWidth, rect.y + channelWidth * 3, channelWidth, channelWidth);
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
            int buttonWidth = 80;
            int buttonHeight = 40;
            Rect buttonR = new Rect(rectR.x - buttonWidth - 5, rectR.y + rectR.height / 2 - buttonHeight / 2, buttonWidth, buttonHeight);
            Rect buttonG = new Rect(rectG.x - buttonWidth - 5, rectG.y + rectG.height / 2 - buttonHeight / 2, buttonWidth, buttonHeight);
            Rect buttonB = new Rect(rectB.x - buttonWidth - 5, rectB.y + rectB.height / 2 - buttonHeight / 2, buttonWidth, buttonHeight);
            Rect buttonA = new Rect(rectA.x - buttonWidth - 5, rectA.y + rectA.height / 2 - buttonHeight / 2, buttonWidth, buttonHeight);
            _positionsChannelOut[0] = new Vector2(buttonR.x, buttonR.y + buttonR.height / 2);
            _positionsChannelOut[1] = new Vector2(buttonG.x, buttonG.y + buttonG.height / 2);
            _positionsChannelOut[2] = new Vector2(buttonB.x, buttonB.y + buttonB.height / 2);
            _positionsChannelOut[3] = new Vector2(buttonA.x, buttonA.y + buttonA.height / 2);
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
            
            bool isSelected = _creatingConnection != null && _creatingConnection.ToChannel == channel;
            if ((isSelected && GUI.Button(channelRect, "X")) || (!isSelected && GUI.Button(channelRect, channel.ToString())))
            {
                if (_creatingConnection != null) _creatingConnection.SetTo(channel, this);
                else _creatingConnection = Connection.Create(channel);
            }
            if(DoFallback(channel))
            {
                config.Fallback = EditorGUI.FloatField(fallbackRect, config.Fallback);
            }else
            {
                config.BlendMode = (BlendMode)EditorGUI.EnumPopup(blendmodeRect, config.BlendMode);
                config.Invert = (InvertMode)EditorGUI.EnumPopup(invertRect, config.Invert);
            }
        }

        bool DrawInput(TextureSource texture, int index, int textureHeight = 100)
        {
            Rect rect = GUILayoutUtility.GetRect(textureHeight, textureHeight + 20);
            Rect textureRect = new Rect(rect.x, rect.y, textureHeight, textureHeight);
            Rect filterRect = new Rect(textureRect.x, textureRect.y + textureHeight, textureRect.width, 20);

            // Draw textrue & filtermode. Change filtermode if texture is changed
            EditorGUI.BeginChangeCheck();
            texture.Texture = (Texture2D)EditorGUI.ObjectField(textureRect, texture.Texture, typeof(Texture2D), false);
            bool didTextureChange = EditorGUI.EndChangeCheck();
            if(didTextureChange && texture.Texture != null) texture.FilterMode = texture.Texture.filterMode;
            texture.FilterMode = (FilterMode)EditorGUI.EnumPopup(filterRect, texture.FilterMode);

            // draw 4 channl boxes on the right side
            int channelWidth = textureHeight / 5;
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
            Rect circleR = new Rect(rectR.x + rectR.width + 5, rectR.y + rectR.height / 2 - 10, 40, 20);
            Rect circleG = new Rect(rectG.x + rectG.width + 5, rectG.y + rectG.height / 2 - 10, 40, 20);
            Rect circleB = new Rect(rectB.x + rectB.width + 5, rectB.y + rectB.height / 2 - 10, 40, 20);
            Rect circleA = new Rect(rectA.x + rectA.width + 5, rectA.y + rectA.height / 2 - 10, 40, 20);
            Rect circleMax = new Rect(rectMax.x + rectMax.width + 5, rectMax.y + rectMax.height / 2 - 10, 40, 20);
            _positionsChannelIn[index * 5 + 0] = new Vector2(circleR.x + circleR.width, circleR.y + circleR.height / 2);
            _positionsChannelIn[index * 5 + 1] = new Vector2(circleG.x + circleG.width, circleG.y + circleG.height / 2);
            _positionsChannelIn[index * 5 + 2] = new Vector2(circleB.x + circleB.width, circleB.y + circleB.height / 2);
            _positionsChannelIn[index * 5 + 3] = new Vector2(circleA.x + circleA.width, circleA.y + circleA.height / 2);
            _positionsChannelIn[index * 5 + 4] = new Vector2(circleMax.x + circleMax.width, circleMax.y + circleMax.height / 2);
            DrawInputChannel(circleR, index, TextureChannelIn.R);
            DrawInputChannel(circleG, index, TextureChannelIn.G);
            DrawInputChannel(circleB, index, TextureChannelIn.B);
            DrawInputChannel(circleA, index, TextureChannelIn.A);
            DrawInputChannel(circleMax, index, TextureChannelIn.Max);

            return didTextureChange;
        }

        void DrawInputChannel(Rect position, int index, TextureChannelIn channel)
        {
            bool isSelected = _creatingConnection != null && _creatingConnection.FromChannel == channel && _creatingConnection.FromTextureIndex == index;
            if((isSelected && GUI.Button(position, "X")) || (!isSelected && GUI.Button(position, channel.ToString())))
            {
                if (_creatingConnection == null) _creatingConnection = Connection.Create(index, channel);
                else _creatingConnection.SetFrom(index, channel, this);
            }
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

        // Packing Logic

        void Pack()
        {
            _outputTexture = Pack(_textureSources, _outputConfigs, _connections, _filterMode, _colorSpace);
            if(OnChange != null) OnChange(_outputTexture, _textureSources, _outputConfigs, _connections.ToArray());
        }

        public static Texture2D Pack(TextureSource[] sources, OutputConfig[] outputConfigs, IEnumerable<Connection> connections, FilterMode targetFilterMode, ColorSpace targetColorSpace)
        {
            int width = 16;
            int height = 16;
            //Find max size
            foreach(TextureSource source in sources)
            {
                source.FindMaxSize(ref width, ref height);
            }

            RenderTexture target = new RenderTexture(width,height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.filterMode = targetFilterMode;
            target.Create();

            ComputeShader.SetTexture(0, "Result", target);
            ComputeShader.SetFloat("Width", width);
            ComputeShader.SetFloat("Height", height);

            // Set Compute Shader Properties
            SetComputeValues(sources, connections, outputConfigs[0], TextureChannelOut.R);
            SetComputeValues(sources, connections, outputConfigs[1], TextureChannelOut.G);
            SetComputeValues(sources, connections, outputConfigs[2], TextureChannelOut.B);
            SetComputeValues(sources, connections, outputConfigs[3], TextureChannelOut.A);

            ComputeShader.Dispatch(0, width / 8 + 1, height / 8 + 1, 1);

            Texture2D atlas = new Texture2D(width, height, TextureFormat.RGBA32, true, targetColorSpace == ColorSpace.Linear);
            RenderTexture.active = target;
            atlas.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            atlas.filterMode = targetFilterMode;
            atlas.Apply();

            return atlas;
        }

        static void SetComputeValues(TextureSource[] sources, IEnumerable<Connection> allConnections, OutputConfig config, TextureChannelOut outChannel)
        {
            // Find all incoming connections
            Connection[] chnlConnections = allConnections.Where(c => c.ToChannel == outChannel && sources[c.FromTextureIndex].Texture != null).ToArray();
            
            // Set textures
            for(int i = 0; i < chnlConnections.Length; i++)
            {
                TextureSource s = sources[chnlConnections[i].FromTextureIndex];
                ComputeShader.SetTexture(0, outChannel.ToString() + "_Input_" + i, s.UncompressedTexture);
                ComputeShader.SetInt(outChannel.ToString() + "_Channel_" + i, (int)chnlConnections[i].FromChannel);
                ComputeShader.SetBool(outChannel.ToString() + "_IsBillinear_" + i, s.FilterMode != FilterMode.Point);
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
        }

        void Save()
        {
            if (_outputTexture == null) return;
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
            importer.crunchedCompression = true;
            importer.sRGBTexture = _colorSpace == ColorSpace.Gamma;
            importer.filterMode = _filterMode;
            importer.SaveAndReimport();

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if(OnSave != null) OnSave(tex);
        }
    }
}