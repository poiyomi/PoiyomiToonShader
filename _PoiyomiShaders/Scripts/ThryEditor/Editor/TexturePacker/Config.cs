using System;
using System.Collections.Generic;
using System.Linq;
using Thry.ThryEditor.Helpers;
using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor.TexturePacker
{
    [Serializable]
    public class TexturePackerConfig
    {
        public PackerSource[] Sources;
        public OutputTarget[] Targets;
        public List<Connection> Connections;
        public FileOutput FileOutput;
        public ImageAdjust ImageAdjust;

        public KernelPreset KernelPreset;
        public KernelSettings KernelSettings;
        public Vector2 ScrollPosition;

        public string Serialize()
        {
            return "ThryTexturePackerConfig:" + JsonUtility.ToJson(this);
        }

        public static TexturePackerConfig Deserialize(string json)
        {
            if (json.StartsWith("ThryTexturePackerConfig:"))
            {
                return JsonUtility.FromJson<TexturePackerConfig>(json.Substring("ThryTexturePackerConfig:".Length));
            }
            return null;
        }

        public static TexturePackerConfig GetNewConfig()
        {
            TexturePackerConfig config = new TexturePackerConfig();
            config.Sources = new PackerSource[]
            {
                new PackerSource(),
                new PackerSource(),
                new PackerSource(),
                new PackerSource(),
            };
            config.Targets = new OutputTarget[]
        {
                new OutputTarget(fallback: 0),
                new OutputTarget(fallback: 0),
                new OutputTarget(fallback: 0),
                new OutputTarget(fallback: 1),
        };
            config.Connections = new List<Connection>();
            config.FileOutput = new FileOutput(
                saveFolder: "Assets/Textures/Packed",
                fileName: "output",
                saveType: SaveType.PNG,
                colorSpace: ColorSpace.Linear,
                filterMode: FilterMode.Bilinear,
                alphaIsTransparency: true,
                saveQuality: 75,
                resolution: new Vector2Int(16, 16)
            );
            config.ImageAdjust = new ImageAdjust();
            config.KernelPreset = KernelPreset.None;
            config.KernelSettings = null;
            config.ScrollPosition = NodeGUI.DefaultScrollPosition;
            return config;
        }

        public static bool TryGetFromTexture(Texture2D tex, out TexturePackerConfig config)
        {
            string path = AssetDatabase.GetAssetPath(tex);
            if (!string.IsNullOrEmpty(path))
            {
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    string json = importer.userData;
                    if (!string.IsNullOrEmpty(json))
                    {
                        try
                        {
                            config = TexturePackerConfig.Deserialize(json);
                            if (config.Sources.Length > 0)
                            {
                                return true;
                            }
                        }
                        catch (Exception) { }
                    }
                }
            }
            config = null;
            return false;
        }

        public void Fix()
        {
            foreach (var src in Sources)
            {
                src.FixImageTexture();
                src.UpdateGradientTexture(FileOutput.Resolution);
                src.UpdateColorTexture();
            }
        }

        public void SaveToImporter(TextureImporter importer)
        {
            if (importer == null || this == null) return;
            importer.userData = this.Serialize();
            if (!s_textureImporterList.Values.Contains(importer))
            {
                s_textureImporterList.Add(importer.assetPath.Replace("Assets/", ""), importer);
            }
        }

        private static SortedList<string, TextureImporter> s_textureImporterList = new SortedList<string, TextureImporter>();
        private static string[] s_importerGuids = null;
        private static bool s_isLoadingImportersDone = false;
        private const int LOADING_BATCH_SIZE = 50;
        private static int s_currentLoadingIndex = 0;
        public static IList<TextureImporter> AssetImporters => s_textureImporterList.Values;
        public static IList<string> AssetNames => s_textureImporterList.Keys;

        public static bool AreImportersLoaded()
        {
            return s_isLoadingImportersDone;
        }

        public static void LoadImportersBatch()
        {
            if(!s_isLoadingImportersDone)
            {
                if (s_importerGuids == null)
                {
                    s_textureImporterList.Clear();
                    s_importerGuids = AssetDatabase.FindAssets("t:Texture2D");
                    s_currentLoadingIndex = 0;
                }

                for (int i = s_currentLoadingIndex; i < Mathf.Min(s_currentLoadingIndex + LOADING_BATCH_SIZE, s_importerGuids.Length); i++)
                {
                    string guid = s_importerGuids[i];
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer != null)
                    {
                        if (importer.userData.StartsWith("ThryTexturePackerConfig:"))
                        {
                            s_textureImporterList.Add(path.Replace("Assets/", ""), importer);
                        }
                    }
                }
                s_currentLoadingIndex += LOADING_BATCH_SIZE;
                if(s_currentLoadingIndex >= s_importerGuids.Length)
                {
                    s_isLoadingImportersDone = true;
                }
            }
        }
    }
}