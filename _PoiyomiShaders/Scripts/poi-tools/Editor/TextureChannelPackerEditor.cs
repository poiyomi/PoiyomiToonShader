using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Poi
{
    public class TextureChannelPackerEditor : EditorWindow
    {
        const string log_prefix = "<color=blue>Poi:</color> "; //color is hex or name

        static readonly Vector2 MIN_WINDOW_SIZE = new Vector2(316, 420);

        const int AUTO_SELECT_CEILING = 2048;
        int[] SizePresets { get; } = { 128, 256, 512, 1024, 2048, 4096 };
        string[] SizePresetNames
        {
            get
            {
                if(_sizeNames == null)
                    _sizeNames = SizePresets.Select(i => i + " x " + i).ToArray();
                return _sizeNames;
            }
        }

        Vector2Int PackSize { get; set; } = new Vector2Int(1024, 1024);
        Vector2Int UnpackSize { get; set; } = new Vector2Int(1024, 1024);

        bool packSizeIsLinked = true;
        bool unpackSizeIsLinked = true;

        bool packSizeAutoSelect = true;
        bool unpackSizeAutoSelect = true;

        Texture2D packRed, packGreen, packBlue, packAlpha;
        Texture2D unpackSource;

        Shader PackerShader
        {
            get
            {
                return Shader.Find("Hidden/Poi/TexturePacker");
                //if(!_packerShader)
                //    _packerShader = Shader.Find("Hidden/Poi/TexturePacker");
                //return _packerShader;
            }
        }

        Shader UnpackerShader
        {
            get
            {
                return Shader.Find("Hidden/Poi/TextureUnpacker");
                //if(!_packerShader)
                //    _packerShader = Shader.Find("Hidden/Poi/TextureUnpacker");
                //return _packerShader;
            }
        }

        // UI
        enum Tab { Pack, Unpack }
        int selectedTab = 0;
        string[] TabNames
        {
            get
            {
                if(_tabNames == null)
                    _tabNames = Enum.GetNames(typeof(Tab));
                return _tabNames;
            }
        }

        string savePath = "Assets/_ChannelPacker";
        string packedName = "packed";
        string unpackedName = "unpacked";



        [MenuItem("Poi/Tools/Texture Packer")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow editorWindow = GetWindow(typeof(TextureChannelPackerEditor));
            editorWindow.autoRepaintOnSceneChange = true;
            editorWindow.minSize = MIN_WINDOW_SIZE;

            editorWindow.Show();
            editorWindow.titleContent = new GUIContent("Texture Packer");
        }

        void OnGUI()
        {
            PoiHelpers.DrawLine();
            selectedTab = GUILayout.Toolbar(selectedTab, TabNames);
            PoiHelpers.DrawLine();

            if(selectedTab == (int)Tab.Pack)
                DrawPackUI();
            else
                DrawUnpackUI();
        }

        void DrawPackUI()
        {
            EditorGUI.BeginChangeCheck();
            {
                packRed = EditorGUILayout.ObjectField("Red", packRed, typeof(Texture2D), true) as Texture2D;
                packGreen = EditorGUILayout.ObjectField("Green", packGreen, typeof(Texture2D), true) as Texture2D;
                packBlue = EditorGUILayout.ObjectField("Blue", packBlue, typeof(Texture2D), true) as Texture2D;
                packAlpha = EditorGUILayout.ObjectField("Alpha", packAlpha, typeof(Texture2D), true) as Texture2D;
            }
            if(EditorGUI.EndChangeCheck() && packSizeAutoSelect)
            {
                // Get biggest texture size from selections and make a selection in our sizes list
                var tempSize = PoiHelpers.GetMaxSizeFromTextures(packRed, packGreen, packBlue, packAlpha);
                if(tempSize != default)
                    PackSize = tempSize.ClosestPowerOfTwo(AUTO_SELECT_CEILING);
            }

            PoiHelpers.DrawLine();

            bool disabled = new bool[] { packRed, packGreen, packBlue, packAlpha }.Count(b => b) < 2;
            EditorGUI.BeginDisabledGroup(disabled);
            {
                packedName = EditorGUILayout.TextField("File name", packedName);

                EditorGUILayout.Space();

                PackSize = PoiHelpers.DrawResolutionPicker(PackSize, ref packSizeIsLinked, ref packSizeAutoSelect, SizePresets, SizePresetNames);

                EditorGUILayout.Space();
                PoiHelpers.DrawLine();

                if(GUILayout.Button("Pack"))
                {
                    var packResult = PackTexture(PackSize, packRed, packGreen, packBlue, packAlpha);
                    if(packResult)
                    {
                        string path = $"{savePath}/Packed/{packedName}.png";
                        packResult.SaveTextureAsset(path, true);
                        Debug.Log(log_prefix + "Finished packing texture at " + path);
                        PoiHelpers.PingAssetAtPath(path);
                    }
                }
            }
            PoiHelpers.DrawLine();
        }

        void DrawUnpackUI()
        {
            EditorGUI.BeginChangeCheck();
            {
                unpackSource =
                    EditorGUILayout.ObjectField("Packed Texture", unpackSource, typeof(Texture2D), true) as Texture2D;
            }
            if(EditorGUI.EndChangeCheck() && unpackSizeAutoSelect)
            {
                // Get biggest texture size from selections and make a selection in our sizes list
                var tempSize = PoiHelpers.GetMaxSizeFromTextures(unpackSource);
                if(tempSize != default)
                    UnpackSize = tempSize.ClosestPowerOfTwo(AUTO_SELECT_CEILING);
            }

            PoiHelpers.DrawLine();

            EditorGUI.BeginDisabledGroup(!unpackSource);
            {
                unpackedName = EditorGUILayout.TextField("File name", unpackedName);

                EditorGUILayout.Space();

                UnpackSize = PoiHelpers.DrawResolutionPicker(UnpackSize, ref unpackSizeIsLinked, ref unpackSizeAutoSelect, SizePresets,
                    SizePresetNames);

                PoiHelpers.DrawLine();

                if(GUILayout.Button("Unpack"))
                {
                    var output = UnpackTextureToChannels(unpackSource, UnpackSize);
                    string pingPath = null;
                    try
                    {
                        AssetDatabase.StartAssetEditing();
                        foreach(var kv in output)
                        {
                            if(string.IsNullOrWhiteSpace(pingPath))
                                pingPath = $"{savePath}/Unpacked/{unpackedName}_{kv.Key}.png";
                            kv.Value?.SaveTextureAsset($"{savePath}/Unpacked/{unpackedName}_{kv.Key}.png", true);
                        }
                    }
                    catch {}
                    finally
                    {
                        AssetDatabase.StopAssetEditing();
                    }

                    Debug.Log(log_prefix + "Finished unpacking texture at " + pingPath);
                    PoiHelpers.PingAssetAtPath(pingPath);
                }
            }
            EditorGUI.EndDisabledGroup();

            PoiHelpers.DrawLine();
        }

        Texture2D PackTexture(Vector2Int resolution, Texture2D red, Texture2D green, Texture2D blue, Texture2D alpha)
        {
            if(!PackerShader)
            {
                Debug.LogWarning(log_prefix + "Packer shader is missing or invalid. Can't pack textures.");
                return null;
            }

            // Setup Material
            var mat = new Material(PackerShader);

            mat.SetTexture("_Red", red);
            mat.SetTexture("_Green", green);
            mat.SetTexture("_Blue", blue);
            mat.SetTexture("_Alpha", alpha);

            // Create texture and render to it
            var tex = new Texture2D(resolution.x, resolution.y);
            tex.BakeMaterialToTexture(mat);

            // Cleanup
            PoiHelpers.DestroyAppropriate(mat);

            return tex;
        }

        Dictionary<string, Texture2D> UnpackTextureToChannels(Texture2D packedTexture, Vector2Int resolution)
        {
            var channels = new Dictionary<string, Texture2D>
            {
                {"red", new Texture2D(resolution.x, resolution.y, TextureFormat.RGB24, true)},
                {"green", new Texture2D(resolution.x, resolution.y, TextureFormat.RGB24, true)},
                {"blue", new Texture2D(resolution.x, resolution.y, TextureFormat.RGB24, true)},
                {"alpha", new Texture2D(resolution.x, resolution.y, TextureFormat.RGB24, true)}
            };

            var mat = new Material(UnpackerShader);
            mat.SetTexture("_Packed", packedTexture);

            for(int i = 0; i < 4; i++)
            {
                mat.SetFloat("_Mode", i);
                channels.ElementAt(i).Value.BakeMaterialToTexture(mat);
            }

            return channels;
        }

        string[] _tabNames;
        Shader _packerShader;
        Shader _unpackerShader;
        string[] _sizeNames;
    }
}