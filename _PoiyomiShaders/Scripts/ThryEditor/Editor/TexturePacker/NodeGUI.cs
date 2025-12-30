using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Thry.ThryEditor.Helpers;
using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor.TexturePacker
{
    public class NodeGUI : EditorWindow
    {
        static NodeGUI s_instance;
        const int MIN_WIDTH = 850;
        const int MIN_HEIGHT = 810;

        const string CHANNEL_PREVIEW_SHADER = "Hidden/Thry/ChannelPreview";

        TexturePackerConfig _config;
        TextureImporter _associatedImporter;
        Dictionary<Connection, ConnectionBezierPoints> _connectionPoints = new Dictionary<Connection, ConnectionBezierPoints>();
        IPackerUIDragable _currentlyDraggingNode = null;
        bool _kernelEditHorizontal = true;
        Connection? _creatingConnection;
        Texture2D _outputTexture;
        Texture2D _channelPreviewTexture;
        bool _showTransparency = true;

        bool[] _channel_export = new bool[4] { true, true, true, true };

        public Action<Texture2D> OnSave;
        public Action<Texture2D, TexturePackerConfig> OnChange;

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

        Vector2[] _positionsChannelOut = new Vector2[4];
        Rect[] _rectsChannelOut = new Rect[4];

        [MenuItem("Assets/Thry/Textures/Open in Texture Packer")]
        public static void OpenTexturePackerWithOneTexture()
        {
            Open(Selection.activeObject as Texture2D);
        }

        [MenuItem("Assets/Thry/Textures/Open in Texture Packer", true)]
        public static bool OpenTexturePackerWithOneTextureValidate()
        {
            return Selection.activeObject is Texture2D;
        }

        public static NodeGUI Open()
        {
            return ShowWindow().InitilizeWithData(TexturePackerConfig.GetNewConfig());
        }

        public static NodeGUI Open(Texture2D tex)
        {
            if (TexturePackerConfig.TryGetFromTexture(tex, out TexturePackerConfig config))
            {
                return ShowWindow().InitilizeWithData(config);
            }
            return ShowWindow().InitilizeWithOneTexture(tex);
        }

        public static NodeGUI Open(TexturePackerConfig config)
        {
            return ShowWindow().InitilizeWithData(config);
        }

        NodeGUI InitilizeWithData(TexturePackerConfig config, TextureImporter importer = null)
        {
            _config = config;
            _associatedImporter = importer;
            _config.Fix();
            Packer.DeterminePathAndFileNameIfEmpty(_config);
            Packer.DetermineImportSettings(_config);
            Packer.DetermineOutputResolution(_config);
            Pack();
            InitializeUIPositions();
            return this;
        }

        NodeGUI InitilizeWithOneTexture(Texture2D texture)
        {
            _config = TexturePackerConfig.GetNewConfig();
            _config.Sources[0].SetInputTexture(texture);
            _config.Sources[1].SetInputTexture(texture);
            _config.Sources[2].SetInputTexture(texture);
            _config.Sources[3].SetInputTexture(texture);
            // Add connections
            _config.Connections.Add(new Connection(0, TextureChannelIn.R, TextureChannelOut.R));
            _config.Connections.Add(new Connection(1, TextureChannelIn.G, TextureChannelOut.G));
            _config.Connections.Add(new Connection(2, TextureChannelIn.B, TextureChannelOut.B));
            _config.Connections.Add(new Connection(3, TextureChannelIn.A, TextureChannelOut.A));
            // Reset Color Adjust
            _config.ImageAdjust = new ImageAdjust();
            Packer.DeterminePathAndFileNameIfEmpty(_config, true);
            Packer.DetermineImportSettings(_config);
            Packer.DetermineOutputResolution(_config);
            Pack();
            InitializeUIPositions();
            return this;
        }

        void InitializeUIPositions()
        {
            for(int i = 0; i < _config.Sources.Length; i++)
            {
                if(_config.Sources[i].UIPosition == Vector2.zero)
                {
                    _config.Sources[i].UIPosition = new Vector2(20, 60 + i * 160);
                }
            }
            if (_config.ImageAdjust.UIPosition == Vector2.zero)
            {
                _config.ImageAdjust.UIPosition = new Vector2(500, 150);
            }
        }

        static NodeGUI ShowWindow()
        {
            s_instance = (NodeGUI)GetWindow(typeof(NodeGUI));
            s_instance.minSize = new Vector2(MIN_WIDTH, MIN_HEIGHT);
            s_instance.titleContent = new GUIContent("Thry Texture Packer");
            s_instance.OnSave = null; // clear save callback
            s_instance.OnChange = null; // clear save callback
            return s_instance;
        }

        public static Vector2 DefaultScrollPosition = new Vector2(0, 115);

        const int TOP_OFFSET = 50;
        const int INPUT_PADDING = 20;
        const int OUTPUT_HEIGHT = 300;

        bool _changeCheckForPacking;
        private void OnGUI()
        {
            s_instance = this;
            if (_config == null)
            {
                _config = TexturePackerConfig.GetNewConfig();
            }

            _changeCheckForPacking = false;

            bool didInputTexturesChange = false;
            for (int i = 0; i < _config.Sources.Length; i++)
            {
                didInputTexturesChange |= DrawInput(_config.Sources[i], i);
            }
            if (didInputTexturesChange)
            {
                Packer.DetermineImportSettings(_config);
            }

            Rect outputRect = GetCanvasRect(_config.ImageAdjust, OUTPUT_HEIGHT, OUTPUT_HEIGHT);
            Rect outputAdjustmentRect = outputRect;
            outputAdjustmentRect.y += outputRect.height;
            outputAdjustmentRect.height = 210 + (_config.KernelPreset == KernelPreset.None ? 0 : 250);

            DrawOutput(_outputTexture, _channelPreviewTexture, outputRect);
            DrawOutputImageAdjustmentGUI(outputAdjustmentRect);
            DrawConnections();
            
            if (_changeCheckForPacking)
            {
                Pack();
                Repaint();
            }
            HandleConnectionEditing();
            HandleConnectionCreation();
            HandleNodeDragging();
            DoConnectionContextMenus();

            HandleCanvasDragging();
            DoCanvasContextMenu();

            DrawTopBar();
        }

        Rect GetCanvasRect(float x, float y, int width, int height)
        {
            return new Rect(x + _config.ScrollPosition.x, y + _config.ScrollPosition.y, width, height);
        }
        
        Rect GetCanvasRect(IPackerUIDragable node, int width, int height)
        {
            return GetCanvasRect(node.UIPosition.x, node.UIPosition.y, width, height);
        }

        void HandleConnectionEditing()
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                InteractionWithConnection toEdit = CheckIfConnectionClicked(50);
                if (toEdit.ListIndex != -1)
                {
                    _config.Connections.RemoveAt(toEdit.ListIndex);
                    // Remove the connection on one side
                    if (toEdit.DistanceX > 0.5)
                        _creatingConnection = new Connection(toEdit.Data.FromTextureIndex, toEdit.Data.FromChannel);
                    else
                        _creatingConnection = new Connection(-1, TextureChannelIn.None, toEdit.Data.ToChannel);
                    Pack();
                    Repaint();
                }
            }
        }

        void AddNewConnection(Connection connection)
        {
            // check if both channels are set
            if (connection.FromChannel == TextureChannelIn.None || connection.ToChannel == TextureChannelOut.None)
            {
                return;
            }
            // check if already exists
            if (_config.Connections.Exists(c => c.ToChannel == connection.ToChannel && c.FromTextureIndex == connection.FromTextureIndex && c.FromChannel == connection.FromChannel))
            {
                return;
            }
            _config.Connections.Add(connection);
            _changeCheckForPacking = true;
            Pack();
            Repaint();
        }

        void HandleConnectionCreation()
        {
            // Connections are not nullable anymore, since they are serialized
            if (_creatingConnection != null && (_creatingConnection.Value.FromChannel != TextureChannelIn.None || _creatingConnection.Value.ToChannel != TextureChannelOut.None))
            {
                // if user clicked anywhere on the screen, stop creating the connection
                if (Event.current.type == EventType.MouseUp)
                {
                    // Check if mouse position is over any input / output slot
                    Vector2 mousePosition = Event.current.mousePosition;
                    for (int t = 0; t < _config.Sources.Length; t++)
                    {
                        for (int c = 0; c < 5; c++)
                        {
                            if (_config.Sources[t].ChannelRects[c].Contains(mousePosition))
                            {
                                AddNewConnection(new Connection(t, (TextureChannelIn)c, _creatingConnection.Value.ToChannel));
                                _creatingConnection = null;
                                return;
                            }
                        }
                    }
                    for (int c = 0; c < 4; c++)
                    {
                        if (_rectsChannelOut[c].Contains(mousePosition))
                        {
                            AddNewConnection(new Connection(_creatingConnection.Value.FromTextureIndex, _creatingConnection.Value.FromChannel, (TextureChannelOut)c));
                            _creatingConnection = null;
                            return;
                        }
                    }
                    _creatingConnection = null;
                    return;
                }

                Vector2 bezierStart, bezierEnd, bezierStartTangent, bezierEndTangent;
                Color color = Color.white;

                bezierEnd = Event.current.mousePosition;

                if (_creatingConnection.Value.FromChannel != TextureChannelIn.None)
                {
                    bezierStart = _config.Sources[_creatingConnection.Value.FromTextureIndex].ChannelPositions[(int)_creatingConnection.Value.FromChannel];
                    bezierStartTangent = bezierStart + Vector2.right * 50;
                    bezierEndTangent = bezierEnd + Vector2.left * 50;
                    color = Packer.GetColor(_creatingConnection.Value.FromChannel);
                }
                else
                {
                    bezierStart = _positionsChannelOut[(int)_creatingConnection.Value.ToChannel];
                    bezierStartTangent = bezierStart + Vector2.left * 50;
                    bezierEndTangent = bezierEnd + Vector2.right * 50;
                    color = Packer.GetColor(_creatingConnection.Value.ToChannel);
                }

                Handles.DrawBezier(bezierStart, bezierEnd, bezierStartTangent, bezierEndTangent, color, null, 2);
                Repaint();
            }
        }

        void HandleNodeDragging()
        {
            if (_currentlyDraggingNode == null)
                return;
            if (Event.current.type == EventType.MouseDrag)
            {
                _currentlyDraggingNode.UIPosition += Event.current.delta;
                Event.current.Use();
                Repaint();
            }
            if (Event.current.type == EventType.MouseUp)
            {
                _currentlyDraggingNode = null;
                Event.current.Use();
                Repaint();
            }
        }

        void CheckForNodeDraggingStart(IPackerUIDragable node, Rect nodeRect)
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                if (nodeRect.Contains(Event.current.mousePosition))
                {
                    _currentlyDraggingNode = node;
                    Event.current.Use();
                    Repaint();
                }
            }
        }

        void HandleCanvasDragging()
        {
            // if middle mouse button is pressed, drag the entire canvas
            if (Event.current.button == 2)
            {
                if (Event.current.type == EventType.MouseDrag)
                {
                    _config.ScrollPosition += Event.current.delta;
                    Event.current.Use();
                    Repaint();
                }
            }
        }

        void DoCanvasContextMenu()
        {
            if (Event.current.type == EventType.ContextClick)
            {
                Vector2 mousePos = Event.current.mousePosition - _config.ScrollPosition;
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Add Source"), false, () =>
                {
                    _config.Sources =
                        _config.Sources.Append(new PackerSource()
                        {
                            UIPosition = mousePos
                        }).ToArray();
                    Repaint();
                });
                menu.ShowAsContext();
                Event.current.Use();
            }
        }

        void DoSourceContextMenu(PackerSource source, Rect rect)
        {
            if (Event.current.type == EventType.ContextClick && rect.Contains(Event.current.mousePosition))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Remove Source"), false, () =>
                {
                    int deletingIndex = Array.IndexOf(_config.Sources, source);
                    _config.Sources = _config.Sources.Where(s => s != source).ToArray();
                    // go through all connections and remove those that reference the deleted source
                    _config.Connections = _config.Connections.Where(c => c.FromTextureIndex != deletingIndex).ToList();
                    // go through all connections and decrement those that reference sources after the deleted source
                    for (int i = 0; i < _config.Connections.Count; i++)
                    {
                        if (_config.Connections[i].FromTextureIndex > deletingIndex)
                        {
                            Connection c = _config.Connections[i];
                            c.FromTextureIndex--;
                            _config.Connections[i] = c;
                        }
                    }
                });
                menu.ShowAsContext();
                Event.current.Use();
            }
        }

        void DoConnectionContextMenus()
        {
            foreach (var c in _config.Connections)
            {
                DoConnectionContextMenu(c);
            }
        }

        void DoConnectionContextMenu(Connection c)
        {
            if (Event.current.type == EventType.ContextClick && HandleUtility.DistancePointBezier(Event.current.mousePosition, _connectionPoints[c].Start, _connectionPoints[c].End, _connectionPoints[c].StartTangent, _connectionPoints[c].EndTangent) < 10f)
            {
                OpenConnectionContextMenu(c);
            }
        }

        void DoConnectionContextMenu(Connection c, Rect rect)
        {
            if (Event.current.type == EventType.ContextClick && rect.Contains(Event.current.mousePosition))
            {
                OpenConnectionContextMenu(c);
            }
        }
        
        void OpenConnectionContextMenu(Connection c)
        {
            Vector2 mousePos = Event.current.mousePosition;
            GenericMenu menu = new GenericMenu();
            if (c.RemappingMode == RemapMode.None)
            {
                menu.AddItem(new GUIContent("Enable Remapping"), false, () =>
                {
                    int index = _config.Connections.IndexOf(c);
                    Connection newC = c;
                    newC.RemappingMode = RemapMode.RangeToRange;
                    _config.Connections[index] = newC;
                    Pack();
                    Repaint();
                });
            }else
            {
                menu.AddItem(new GUIContent("Disable Remapping"), false, () =>
                {
                    int index = _config.Connections.IndexOf(c);
                    Connection newC = c;
                    newC.RemappingMode = RemapMode.None;
                    _config.Connections[index] = newC;
                    Pack();
                    Repaint();
                });
            }
            menu.ShowAsContext();
            Event.current.Use();
        }

        InteractionWithConnection CheckIfConnectionClicked(float maxDistance)
        {
            Vector2 mousePos = Event.current.mousePosition;
            float minDistance = maxDistance;
            InteractionWithConnection clickedConnection = new InteractionWithConnection();
            clickedConnection.ListIndex = -1;
            for (int i = 0; i < _config.Connections.Count; i++)
            {
                Connection c = _config.Connections[i];
                var points = _connectionPoints[c];
                Vector3 from = points.Start;
                Vector3 to = points.End;
                float topY = Mathf.Max(from.y, to.y);
                float bottomY = Mathf.Min(from.y, to.y);
                float leftX = Mathf.Min(from.x, to.x);
                float rightX = Mathf.Max(from.x, to.x);
                // check if mouse is in the area of the bezier curve
                if (mousePos.x > leftX && mousePos.x < rightX)
                {
                    if (mousePos.y > bottomY && mousePos.y < topY)
                    {
                        // check if mouse is close to the bezier curve
                        float distance = HandleUtility.DistancePointBezier(mousePos, points.Start, points.End, points.StartTangent, points.EndTangent);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            clickedConnection.ListIndex = i;
                            clickedConnection.Data = c;
                            clickedConnection.DistanceX = (mousePos.x - leftX) / (rightX - leftX);
                        }
                    }
                }
            }
            return clickedConnection;
        }

        void DrawTopBar()
        {
            Rect uberBG = new Rect(0, 0, position.width, 160);
            Color uberBGColor = Colors.backgroundDark * 0.5f;
            uberBGColor.a = 1f;

            GUI.DrawTexture(uberBG, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, uberBGColor, 0, 0);

            DrawConfigGUI();
            DrawSaveGUI(new Rect(20, 50, position.width - 40, 100));
        }

        void DrawConfigGUI()
        {
            Rect bg = new Rect(20, 10, position.width - 40, 30);
            Rect rObjField = new RectOffset(5, 100, 5, 5).Remove(bg);

            GUI.DrawTexture(bg, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Colors.backgroundDark, 0, 10);

            if (!TexturePackerConfig.AreImportersLoaded())
            {
                TexturePackerConfig.LoadImportersBatch();
                Repaint();
            }

            int selectedIndex = TexturePackerConfig.AssetImporters.IndexOf(_associatedImporter);
            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUI.Popup(rObjField, "Load previous project", selectedIndex, TexturePackerConfig.AssetNames.ToArray());

            if (EditorGUI.EndChangeCheck() && selectedIndex >= 0 && selectedIndex < TexturePackerConfig.AssetImporters.Count)
            {

                TexturePackerConfig newConfig = TexturePackerConfig.Deserialize(TexturePackerConfig.AssetImporters[selectedIndex].userData);
                try
                {
                    InitilizeWithData(newConfig, TexturePackerConfig.AssetImporters[selectedIndex]);
                }
                catch (Exception e)
                {
                    ThryLogger.LogErr("TexturePacker", $"Could not correctly load config from {TexturePackerConfig.AssetNames[selectedIndex]}: {e.Message}");
                }
            }

            Rect rButton = new Rect(rObjField.x + rObjField.width + 5, rObjField.y, 90, rObjField.height);
            if (GUI.Button(rButton, "Clear"))
            {
                _config = TexturePackerConfig.GetNewConfig();
                _outputTexture = null;
                _channelPreviewTexture = null;
                InitilizeWithData(_config);
            }
        }
        
        void DrawOutputImageAdjustmentGUI(Rect rect)
        {
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 1, Colors.backgroundDark, 0, 10);
            GUILayout.BeginArea(new RectOffset(5, 5, 5, 5).Remove(rect));

            EditorGUI.BeginChangeCheck();
            _config.FileOutput.ColorSpace = (ColorSpace)EditorGUILayout.EnumPopup(_config.FileOutput.ColorSpace);
            _config.FileOutput.FilterMode = (FilterMode)EditorGUILayout.EnumPopup(_config.FileOutput.FilterMode);
            _changeCheckForPacking |= EditorGUI.EndChangeCheck();

            // Make the sliders delayed, else the UX feels terrible
            EditorGUI.BeginChangeCheck();
            EventType eventTypeBeforerSliders = Event.current.type;
            bool wasWide = EditorGUIUtility.wideMode;
            EditorGUIUtility.wideMode = true;
            _config.FileOutput.Resolution = EditorGUILayout.Vector2IntField("Resolution", _config.FileOutput.Resolution);
            _config.ImageAdjust.Scale = EditorGUILayout.Vector2Field("Scale", _config.ImageAdjust.Scale);
            _config.ImageAdjust.Offset = EditorGUILayout.Vector2Field("Offset", _config.ImageAdjust.Offset);
            _config.ImageAdjust.Rotation = EditorGUILayout.Slider("Rotation", _config.ImageAdjust.Rotation, -180, 180);
            _config.ImageAdjust.Hue = EditorGUILayout.Slider("Hue", _config.ImageAdjust.Hue, 0, 1);
            _config.ImageAdjust.Saturation = EditorGUILayout.Slider("Saturation", _config.ImageAdjust.Saturation, 0, 3);
            _config.ImageAdjust.Brightness = EditorGUILayout.Slider("Brightness", _config.ImageAdjust.Brightness, 0, 3);
            _config.ImageAdjust.ChangeCheck |= EditorGUI.EndChangeCheck();
            EditorGUIUtility.wideMode = wasWide;
            if (_config.ImageAdjust.ChangeCheck && (eventTypeBeforerSliders == EventType.MouseUp || (eventTypeBeforerSliders == EventType.KeyUp && Event.current.keyCode == KeyCode.Return)))
            {
                _changeCheckForPacking = true;
                _config.ImageAdjust.ChangeCheck = false;
            }
            DrawKernelGUI();
            GUILayout.EndArea();
        }

        void DrawKernelGUI()
        {
            Rect r_enum = EditorGUILayout.GetControlRect(false, 20);

            EditorGUI.BeginChangeCheck();
            _config.KernelPreset = (KernelPreset)EditorGUI.EnumPopup(r_enum, "Kernel Filter", _config.KernelPreset);
            if (EditorGUI.EndChangeCheck())
            {
                if (_config.KernelSettings == null)
                {
                    _config.KernelSettings = new KernelSettings();
                }
                _config.KernelSettings.X = _config.KernelSettings.GetKernel(_config.KernelPreset, true);
                _config.KernelSettings.Y = _config.KernelSettings.GetKernel(_config.KernelPreset, false);
                _config.KernelSettings.LoadPreset(_config.KernelPreset);
                Pack();
                Repaint();
            }

            // this.minSize = new Vector2(MIN_WIDTH, _config.KernelPreset == KernelPreset.None ? MIN_HEIGHT : MIN_HEIGHT + 250);

            if (_config.KernelPreset != KernelPreset.None)
            {
                EventType eventTypeBeforerSliders = Event.current.type;
                _config.KernelSettings.Loops = EditorGUILayout.IntSlider("Loops", _config.KernelSettings.Loops, 1, 25);
                _config.KernelSettings.Strength = EditorGUILayout.Slider("Strength", _config.KernelSettings.Strength, 0, 1);
                _config.KernelSettings.TwoPass = EditorGUILayout.Toggle("Two Pass", _config.KernelSettings.TwoPass);
                _config.KernelSettings.GrayScale = EditorGUILayout.Toggle("Gray Scale", _config.KernelSettings.GrayScale);
                Rect r_channels = EditorGUILayout.GetControlRect(false, 20);
                r_channels.width /= 4;
                float prevLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 10;
                _config.KernelSettings.Channels[0] = EditorGUI.Toggle(r_channels, "R", _config.KernelSettings.Channels[0]);
                r_channels.x += r_channels.width;
                _config.KernelSettings.Channels[1] = EditorGUI.Toggle(r_channels, "G", _config.KernelSettings.Channels[1]);
                r_channels.x += r_channels.width;
                _config.KernelSettings.Channels[2] = EditorGUI.Toggle(r_channels, "B", _config.KernelSettings.Channels[2]);
                r_channels.x += r_channels.width;
                _config.KernelSettings.Channels[3] = EditorGUI.Toggle(r_channels, "A", _config.KernelSettings.Channels[3]);
                EditorGUIUtility.labelWidth = prevLabelWidth;
                if (Event.current.type == EventType.Used && (eventTypeBeforerSliders == EventType.MouseUp || (eventTypeBeforerSliders == EventType.KeyUp && Event.current.keyCode == KeyCode.Return)))
                {
                    Pack();
                    Repaint();
                }

                if (_config.KernelSettings.TwoPass)
                {
                    Rect r_buttons = EditorGUILayout.GetControlRect(false, 20);
                    if (GUI.Button(new Rect(r_buttons.x, r_buttons.y, r_buttons.width / 2, r_buttons.height), "X")) _kernelEditHorizontal = true;
                    if (GUI.Button(new Rect(r_buttons.x + r_buttons.width / 2, r_buttons.y, r_buttons.width / 2, r_buttons.height), "Y")) _kernelEditHorizontal = false;
                }

                Rect r = EditorGUILayout.GetControlRect(false, 130);
                EditorGUI.BeginChangeCheck();
                // draw 5x5 matrix inside the r_kernelX rect
                for (int x = 0; x < 5; x++)
                {
                    for (int y = 0; y < 5; y++)
                    {
                        Rect r_cell = new Rect(r.x + x * r.width / 5, r.y + y * r.height / 5, r.width / 5, r.height / 5);
                        if (_kernelEditHorizontal || !_config.KernelSettings.TwoPass) _config.KernelSettings.X[x + y * 5] = EditorGUI.DelayedFloatField(r_cell, _config.KernelSettings.X[x + y * 5]);
                        else _config.KernelSettings.Y[x + y * 5] = EditorGUI.DelayedFloatField(r_cell, _config.KernelSettings.Y[x + y * 5]);
                    }
                }
                if (EditorGUI.EndChangeCheck())
                {
                    _config.KernelPreset = KernelPreset.Custom;
                    Pack();
                    Repaint();
                }
            }
        }

        void DrawSaveGUI(Rect rect)
        {
            // Saving information
            // folder selection
            // determine folder & filename from asset name if not set
            if (string.IsNullOrEmpty(_config.FileOutput.SaveFolder))
            {
                Packer.DeterminePathAndFileNameIfEmpty(_config);
            }

            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Colors.backgroundDark, 0, 10);
            
            GUILayout.BeginArea(new RectOffset(5, 5, 5, 5).Remove(rect));
            Rect r = EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();
            // show current path
            GUILayout.Label("Save to: ");
            GUILayout.Label(_config.FileOutput.SaveFolder + "\\");
            _config.FileOutput.FileName = GUILayout.TextField(_config.FileOutput.FileName, GUILayout.MinWidth(50));
            _config.FileOutput.SaveType = (SaveType)EditorGUILayout.EnumPopup(_config.FileOutput.SaveType, GUILayout.Width(70));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            _config.FileOutput.AlphaIsTransparency = EditorGUILayout.Toggle("Alpha is Transparency", _config.FileOutput.AlphaIsTransparency);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Change Folder", GUILayout.Width(100)))
            {
                string path = EditorUtility.OpenFolderPanel("Select folder", _config.FileOutput.SaveFolder, "");
                if (!string.IsNullOrEmpty(path))
                {
                    // Make path relative to Assets folder
                    path = path.Replace(Application.dataPath, "Assets");
                    _config.FileOutput.SaveFolder = path;
                }
            }
            if (GUILayout.Button("Save", GUILayout.Width(100)))
            {
                _associatedImporter = Packer.Save(_outputTexture, _config);
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
            if (GUILayout.Button("Export Channels", GUILayout.Width(130)))
            {
                ExportChannels(false);
            }
            if (GUILayout.Button("Export Channels (B&W)", GUILayout.Width(150)))
            {
                ExportChannels(true);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        void DrawConnections()
        {
            // Draw connections as lines
            for(int i = 0; i < _config.Connections.Count; i++)
            {
                Connection c = _config.Connections[i];
                _connectionPoints[c] = new ConnectionBezierPoints(c, _config.Sources, _positionsChannelOut);
                var points = _connectionPoints[c];
                Handles.DrawBezier(points.Start, points.End, points.StartTangent, points.EndTangent, Packer.GetColor(c.FromChannel), null, 2);
                
                if(c.RemappingMode == RemapMode.None)
                {
                    continue;
                }
                // Draw remapping input in center of curve
                Vector3 center = (points.Start + points.End) / 2;
                Rect rect = new Rect(center.x - 50, center.y - 30, 100, 60);
                Color channelColor = Packer.GetColor(c.FromChannel);
                // backgroundColor.a = 0.5f;
                GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Colors.backgroundDark, 0, 10);
                GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, channelColor, 1, 10);

                Rect contentRect = new RectOffset(7, 7, 3, 3).Remove(rect);

                Rect headerRect = contentRect;
                headerRect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(headerRect, $"Remap", Styles.editorHeaderLabel);

                Rect inputRect = headerRect;
                inputRect.y += EditorGUIUtility.singleLineHeight- 2;

                Vector4 remap = c.Remapping;
                EditorGUI.MinMaxSlider(inputRect, ref remap.x, ref remap.y, 0, 1);
                inputRect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.MinMaxSlider(inputRect, ref remap.z, ref remap.w, 0, 1);
                if (remap != c.Remapping)
                {
                    c.Remapping = remap;
                    _config.Connections[i] = c;
                    _changeCheckForPacking = true;
                }

                DoConnectionContextMenu(c, rect);
            }
        }

        void DrawOutput(Texture2D finalTexture, Texture2D channelPreviewTexture, Rect rect)
        {
            int channelWidth = (int)rect.height / 4;

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
            GUI.DrawTexture(background, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 1, Colors.backgroundDark, 0, 10);

            if (_showTransparency)
                EditorGUI.DrawTextureTransparent(rect, finalTexture != null ? finalTexture : Texture2D.blackTexture, ScaleMode.ScaleToFit, 1);
            else
                EditorGUI.DrawPreviewTexture(rect, finalTexture != null ? finalTexture : Texture2D.blackTexture, null, ScaleMode.ScaleToFit);

            // Show transparency toggle
            Rect rectTransparency = new Rect(rect.x + 8, rect.y - 20, rect.width, 20);
            _showTransparency = EditorGUI.Toggle(rectTransparency, "Show Transparency", _showTransparency);

            // draw 4 channl boxes on the left side
            if (channelPreviewTexture != null)
            {
                ChannelPreviewMaterial.SetTexture("_MainTex", channelPreviewTexture);
                ChannelPreviewMaterial.SetFloat("_Channel", 0);
                EditorGUI.DrawPreviewTexture(rectR, channelPreviewTexture, ChannelPreviewMaterial, ScaleMode.ScaleToFit);
                ChannelPreviewMaterial.SetFloat("_Channel", 1);
                EditorGUI.DrawPreviewTexture(rectG, channelPreviewTexture, ChannelPreviewMaterial, ScaleMode.ScaleToFit);
                ChannelPreviewMaterial.SetFloat("_Channel", 2);
                EditorGUI.DrawPreviewTexture(rectB, channelPreviewTexture, ChannelPreviewMaterial, ScaleMode.ScaleToFit);
                ChannelPreviewMaterial.SetFloat("_Channel", 3);
                EditorGUI.DrawPreviewTexture(rectA, channelPreviewTexture, ChannelPreviewMaterial, ScaleMode.ScaleToFit);
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
            _config.Targets[0] = DrawOutputChannel(buttonR, TextureChannelOut.R, _config.Targets[0]);
            _config.Targets[1] = DrawOutputChannel(buttonG, TextureChannelOut.G, _config.Targets[1]);
            _config.Targets[2] = DrawOutputChannel(buttonB, TextureChannelOut.B, _config.Targets[2]);
            _config.Targets[3] = DrawOutputChannel(buttonA, TextureChannelOut.A, _config.Targets[3]);

            CheckForNodeDraggingStart(_config.ImageAdjust, background);
        }

        OutputTarget DrawOutputChannel(Rect position, TextureChannelOut channel, OutputTarget config)
        {
            // RGBA on the left side
            // fallback or (blendmode & invert) on the right side
            Rect channelRect = new Rect(position.x, position.y, 20, position.height);
            Rect fallbackRect = new Rect(position.x + 20, position.y, position.width - 20, position.height);
            Rect blendmodeRect = new Rect(fallbackRect.x, fallbackRect.y, fallbackRect.width, fallbackRect.height / 2);
            Rect invertRect = new Rect(fallbackRect.x, fallbackRect.y + fallbackRect.height / 2, fallbackRect.width, fallbackRect.height / 2);

            if (Event.current.type == EventType.MouseDown && channelRect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                if (_creatingConnection != null) _creatingConnection = new Connection(_creatingConnection.Value.FromTextureIndex, _creatingConnection.Value.FromChannel, channel);
                else _creatingConnection = new Connection(-1, TextureChannelIn.None, channel);
            }
            GUI.Button(channelRect, channel.ToString());

            float fallback = config.Fallback;
            BlendMode blendmode = config.BlendMode;
            InvertMode invert = config.Invert;

            EditorGUI.BeginChangeCheck();
            if (DoFallback(channel))
            {
                fallback = EditorGUI.FloatField(fallbackRect, fallback);
            }
            else
            {
                blendmode = (BlendMode)EditorGUI.EnumPopup(blendmodeRect, blendmode);
                invert = (InvertMode)EditorGUI.EnumPopup(invertRect, invert);
            }
            _changeCheckForPacking |= EditorGUI.EndChangeCheck();

            return new OutputTarget(blendmode, invert, fallback);
        }

        bool DrawInput(PackerSource source, int index, int textureHeight = 100)
        {
            int channelWidth = textureHeight / 5;
            // Rect rect = GUILayoutUtility.GetRect(textureHeight, textureHeight + 40);
            Rect rect = GetCanvasRect(source, textureHeight, textureHeight + 40);
            Rect typeRect = new Rect(rect.x, rect.y, textureHeight, 20);
            Rect textureRect = new Rect(rect.x, rect.y + 20, textureHeight, textureHeight);
            Rect filterRect = new Rect(textureRect.x, textureRect.y + textureHeight, textureRect.width, 20);

            Rect background = new Rect(rect.x - 5, rect.y - 5, rect.width + channelWidth + 40, rect.height + 10);
            GUI.DrawTexture(background, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 1, Colors.backgroundDark, 0, 10);

            // Draw textrue & filtermode. Change filtermode if texture is changed
            EditorGUI.BeginChangeCheck();
            source.InputType = (InputType)EditorGUI.EnumPopup(typeRect, source.InputType);
            bool didTextureChange = false;
            switch (source.InputType)
            {
                case InputType.Texture:
                    EditorGUI.BeginChangeCheck();
                    source.ImageTexture = (Texture2D)EditorGUI.ObjectField(textureRect, source.ImageTexture, typeof(Texture2D), false);
                    didTextureChange = EditorGUI.EndChangeCheck();
                    if (didTextureChange && source.Texture != null) source.FilterMode = source.Texture.filterMode;
                    if (didTextureChange) Packer.DetermineOutputResolution(_config);
                    source.FilterMode = (FilterMode)EditorGUI.EnumPopup(filterRect, source.FilterMode);
                    break;
                case InputType.Gradient:
                    if (source.GradientTexture == null) EditorGUI.DrawRect(textureRect, Color.black);
                    else EditorGUI.DrawPreviewTexture(textureRect, source.GradientTexture);
                    if (Event.current.type == EventType.MouseDown && textureRect.Contains(Event.current.mousePosition))
                    {
                        if (source.Gradient == null) source.Gradient = new Gradient();
                        GradientEditor2.Open(source.Gradient, (Gradient gradient, Texture2D tex) =>
                        {
                            source.Gradient = gradient;
                            source.GradientTexture = tex;
                            // Needs to call these itself because it's in a callback not the OnGUI method
                            Pack();
                            Repaint();
                        }, source.GradientDirection == GradientDirection.Vertical, false, _config.FileOutput.Resolution, new Vector2Int(8192, 8192));

                    }
                    EditorGUI.BeginChangeCheck();
                    source.GradientDirection = (GradientDirection)EditorGUI.EnumPopup(filterRect, source.GradientDirection);
                    if (EditorGUI.EndChangeCheck() && source.Gradient != null)
                    {
                        source.GradientTexture = Converter.GradientToTexture(source.Gradient, _config.FileOutput.Resolution.x, _config.FileOutput.Resolution.y, source.GradientDirection == GradientDirection.Vertical);
                    }
                    break;
                case InputType.Color:
                    EditorGUI.BeginChangeCheck();
                    source.Color = EditorGUI.ColorField(textureRect, source.Color);
                    if (EditorGUI.EndChangeCheck())
                    {
                        source.ColorTexture = Converter.ColorToTexture(source.Color, 16, 16);
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
            if (source.Texture != null)
            {
                ChannelPreviewMaterial.SetTexture("_MainTex", source.Texture);
                ChannelPreviewMaterial.SetFloat("_Channel", 0);
                EditorGUI.DrawPreviewTexture(rectR, source.Texture, ChannelPreviewMaterial, ScaleMode.ScaleToFit);
                ChannelPreviewMaterial.SetFloat("_Channel", 1);
                EditorGUI.DrawPreviewTexture(rectG, source.Texture, ChannelPreviewMaterial, ScaleMode.ScaleToFit);
                ChannelPreviewMaterial.SetFloat("_Channel", 2);
                EditorGUI.DrawPreviewTexture(rectB, source.Texture, ChannelPreviewMaterial, ScaleMode.ScaleToFit);
                ChannelPreviewMaterial.SetFloat("_Channel", 3);
                EditorGUI.DrawPreviewTexture(rectA, source.Texture, ChannelPreviewMaterial, ScaleMode.ScaleToFit);
                ChannelPreviewMaterial.SetFloat("_Channel", 4);
                EditorGUI.DrawPreviewTexture(rectMax, source.Texture, ChannelPreviewMaterial, ScaleMode.ScaleToFit);
            }
            else
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
            source.ChannelPositions[0] = new Vector2(circleR.x + circleR.width, circleR.y + circleR.height / 2);
            source.ChannelPositions[1] = new Vector2(circleG.x + circleG.width, circleG.y + circleG.height / 2);
            source.ChannelPositions[2] = new Vector2(circleB.x + circleB.width, circleB.y + circleB.height / 2);
            source.ChannelPositions[3] = new Vector2(circleA.x + circleA.width, circleA.y + circleA.height / 2);
            source.ChannelPositions[4] = new Vector2(circleMax.x + circleMax.width, circleMax.y + circleMax.height / 2);
            source.ChannelRects[0] = circleR;
            source.ChannelRects[1] = circleG;
            source.ChannelRects[2] = circleB;
            source.ChannelRects[3] = circleA;
            source.ChannelRects[4] = circleMax;
            DrawInputChannel(circleR, index, TextureChannelIn.R);
            DrawInputChannel(circleG, index, TextureChannelIn.G);
            DrawInputChannel(circleB, index, TextureChannelIn.B);
            DrawInputChannel(circleA, index, TextureChannelIn.A);
            DrawInputChannel(circleMax, index, TextureChannelIn.Max);

            CheckForNodeDraggingStart(source, background);
            DoSourceContextMenu(source, background);
            return didTextureChange;
        }

        void DrawInputChannel(Rect position, int index, TextureChannelIn channel)
        {
            if (Event.current.type == EventType.MouseDown && position.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                if (_creatingConnection == null) _creatingConnection = new Connection(index, channel, TextureChannelOut.None);
                else _creatingConnection = new Connection(index, channel, _creatingConnection.Value.ToChannel);
            }
            GUI.Button(position, channel.ToString());
        }



        bool DoFallback(TextureChannelOut channel)
        {
            return _config.Connections.Any(c => c.ToChannel == channel && c.FromTextureIndex != -1
                && _config.Sources[c.FromTextureIndex].Texture != null) == false;
        }

        void Pack()
        {
            _outputTexture = Packer.Pack(_config);
            _channelPreviewTexture = PackForChannelPreview();
            if (OnChange != null) OnChange(_outputTexture, _config);
        }

        Texture2D PackForChannelPreview()
        {
            ImageAdjust tempAdjust = new ImageAdjust();
            KernelSettings tempKernel = new KernelSettings();
            ImageAdjust adjust = _config.ImageAdjust;
            KernelSettings kernel = _config.KernelSettings;

            if(tempAdjust == adjust && tempKernel == kernel)
            {
                return _outputTexture;
            }

            _config.ImageAdjust = tempAdjust;
            _config.KernelSettings = tempKernel;

            Texture2D previewTexture = Packer.Pack(_config);

            _config.ImageAdjust = adjust;
            _config.KernelSettings = kernel;
            return previewTexture;
        }

        void ExportChannels(bool exportAsBlackAndWhite)
        {
            Pack();
            Packer.DeterminePathAndFileNameIfEmpty(_config);
            Packer.ExportChannels(_outputTexture, _config, _channel_export, exportAsBlackAndWhite);
        }

        public class TextureChangeHandler : AssetPostprocessor
        {
            static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                if (s_instance == null) return;

                string[] active_textures = s_instance._config.Sources
                    .Where(source => source.InputType == InputType.Texture && source.Texture != null)
                    .Select(source => AssetDatabase.GetAssetPath(source.Texture))
                    .ToArray();

                if (importedAssets.Any(path => active_textures.Contains(path, StringComparer.OrdinalIgnoreCase)))
                {
                    ThryLogger.Log("TexturePacker", "Detected external texture change, repacking texture.");
                    s_instance.Pack();
                    s_instance.Repaint();
                }
            }
        }
            
    }
}