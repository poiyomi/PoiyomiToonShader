using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor.TexturePacker
{
    public class Packer
    {
        static ComputeShader s_computeShader;
        static ComputeShader PackShader
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

        public static Color GetColor(TextureChannelIn c)
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

        public static Color GetColor(TextureChannelOut c)
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

        public static void DetermineOutputResolution(TexturePackerConfig config)
        {
            int width = 16;
            int height = 16;
            foreach (PackerSource source in config.Sources)
            {
                source.FindMaxSize(ref width, ref height);
            }
            // round up to nearest power of 2
            width = Mathf.NextPowerOfTwo(width);
            height = Mathf.NextPowerOfTwo(height);
            // clamp to max size of 4096
            width = Mathf.Clamp(width, 16, 4096);
            height = Mathf.Clamp(height, 16, 4096);
            config.FileOutput.Resolution = new Vector2Int(width, height);
        }

        public static void DeterminePathAndFileNameIfEmpty(TexturePackerConfig config, bool forceOverwrite = false)
        {
            foreach (PackerSource s in config.Sources)
            {
                if (s.Texture != null)
                {
                    string path = AssetDatabase.GetAssetPath(s.Texture);
                    if (string.IsNullOrWhiteSpace(path))
                        continue;
                    if (string.IsNullOrWhiteSpace(config.FileOutput.SaveFolder) || forceOverwrite)
                        config.FileOutput.SaveFolder = Path.GetDirectoryName(path);
                    if (string.IsNullOrWhiteSpace(config.FileOutput.FileName) || forceOverwrite)
                        config.FileOutput.FileName = Path.GetFileNameWithoutExtension(path) + "_packed";
                    break;
                }
            }
        }

        public static void DetermineImportSettings(TexturePackerConfig config)
        {
            config.FileOutput.ColorSpace = ColorSpace.Gamma;
            config.FileOutput.FilterMode = FilterMode.Bilinear;
            foreach (PackerSource s in config.Sources)
            {
                if (DetermineImportSettings(config, s))
                    break;
            }
        }

        static bool DetermineImportSettings(TexturePackerConfig config, PackerSource s)
        {
            if (s.Texture != null)
            {
                string path = AssetDatabase.GetAssetPath(s.Texture);
                if (path == null)
                    return false;
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    config.FileOutput.ColorSpace = importer.sRGBTexture ? ColorSpace.Gamma : ColorSpace.Linear;
                    config.FileOutput.FilterMode = importer.filterMode;
                    return true;
                }
            }
            return false;
        }

        public static Texture2D Pack(TexturePackerConfig config)
        {
            foreach (PackerSource source in config.Sources)
            {
                source.UpdateGradientTexture(config.FileOutput.Resolution);
                source.UpdateColorTexture();
            }

            if (config.ImageAdjust == null)
            {
                config.ImageAdjust = new ImageAdjust();
            }
            DetermineOutputResolution(config);
            int width = config.FileOutput.Resolution.x;
            int height = config.FileOutput.Resolution.y;


            RenderTexture target = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB64, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.filterMode = config.FileOutput.FilterMode;
            target.Create();

            PackShader.SetTexture(0, "Result", target);
            PackShader.SetFloat("Width", width);
            PackShader.SetFloat("Height", height);

            PackShader.SetFloat("Rotation", config.ImageAdjust.Rotation / 360f * 2f * Mathf.PI);
            PackShader.SetVector("Scale", config.ImageAdjust.Scale);
            PackShader.SetVector("Offset", config.ImageAdjust.Offset);
            PackShader.SetFloat("Hue", config.ImageAdjust.Hue);
            PackShader.SetFloat("Saturation", config.ImageAdjust.Saturation);
            PackShader.SetFloat("Brightness", config.ImageAdjust.Brightness);

            bool repeatTextures = Math.Abs(config.ImageAdjust.Scale.x) > 1 || Math.Abs(config.ImageAdjust.Scale.y) > 1;

            // Set Compute Shader Properties
            ComputeBuffer connectionsBuffer = new ComputeBuffer(config.Connections.Count + 1, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Connection)));
            connectionsBuffer.SetData(config.Connections.ToArray());
            PackShader.SetBuffer(0, "Connections", connectionsBuffer);
            PackShader.SetInt("ConnectionCount", config.Connections.Count);

            ComputeBuffer outputsBuffer = new ComputeBuffer(config.Targets.Length, System.Runtime.InteropServices.Marshal.SizeOf(typeof(OutputTarget)));
            outputsBuffer.SetData(config.Targets);
            PackShader.SetBuffer(0, "OutputChannels", outputsBuffer);

            for (int i = 0; i < config.Sources.Length; i++)
            {
                PackShader.SetTexture(0, $"Inputs[{i}]", config.Sources[i].ComputeShaderTexture);
            }
            for (int i = config.Sources.Length; i < 16; i++)
            {
                PackShader.SetTexture(0, $"Inputs[{i}]", Texture2D.blackTexture); // dummy textures for unused slots
            }
            // Vectors because int and float arrays are broken in compute shaders
            PackShader.SetVectorArray("InputTextureIsValid", config.Sources.Select(s => s.ComputeShaderTextureIsValid ? Vector4.one : Vector4.zero).ToArray());

            PackShader.Dispatch(0, width / 8 + 1, height / 8 + 1, 1);

            if (config.KernelSettings != null)
            {
                // Settings Vector4s instead of floats because the SetFloats function is broken
                float[] kernelNone = KernelSettings.GetKernelPreset(KernelPreset.None, false);
                PackShader.SetVectorArray("Kernel_X", config.KernelSettings.X.Select((f, i) => new Vector4(Mathf.Lerp(kernelNone[i], f, config.KernelSettings.Strength), 0, 0, 0)).ToArray());
                PackShader.SetVectorArray("Kernel_Y", config.KernelSettings.X.Select((f, i) => new Vector4(Mathf.Lerp(kernelNone[i], f, config.KernelSettings.Strength), 0, 0, 0)).ToArray());
                PackShader.SetBool("Kernel_Grayscale", config.KernelSettings.GrayScale);
                PackShader.SetBool("Kernel_TwoPass", config.KernelSettings.TwoPass);
                PackShader.SetVector("Kernel_Channels", new Vector4(config.KernelSettings.Channels[0] ? 1 : 0, config.KernelSettings.Channels[1] ? 1 : 0, config.KernelSettings.Channels[2] ? 1 : 0, config.KernelSettings.Channels[3] ? 1 : 0));

                // define the opposite way, because each loop flips it
                RenderTexture filterTarget = target;

                RenderTexture filterInput = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB64, RenderTextureReadWrite.Linear);
                filterInput.enableRandomWrite = true;
                filterInput.filterMode = config.FileOutput.FilterMode;
                filterInput.Create();

                for (int i = 0; i < config.KernelSettings.Loops; i++)
                {
                    RenderTexture temp = filterInput;
                    filterInput = filterTarget;
                    filterTarget = temp;

                    PackShader.SetTexture(1, "Kernel_Input", filterInput);
                    PackShader.SetTexture(1, "Result", filterTarget);
                    PackShader.Dispatch(1, width / 8 + 1, height / 8 + 1, 1);
                }

                target = filterTarget;
            }

            Texture2D atlas = new Texture2D(width, height, TextureFormat.RGBA64, true, config.FileOutput.ColorSpace == ColorSpace.Linear);
            RenderTexture.active = target;
            atlas.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            atlas.filterMode = config.FileOutput.FilterMode;
            atlas.wrapMode = TextureWrapMode.Clamp;
            atlas.alphaIsTransparency = config.FileOutput.AlphaIsTransparency;
            atlas.Apply();
            RenderTexture.active = null;

            return atlas;
        }

        #region Channel Unpacker



        static void ExportChannel(Texture2D input, RenderTexture renderTex, Vector4 lerpR, Vector4 lerpG, Vector4 lerpB, Vector4 lerpA, Vector4 add, string namePostfix, TexturePackerConfig config)
        {
            PackShader.SetVector("Channels_Strength_R", lerpR);
            PackShader.SetVector("Channels_Strength_G", lerpG);
            PackShader.SetVector("Channels_Strength_B", lerpB);
            PackShader.SetVector("Channels_Strength_A", lerpA);
            PackShader.SetVector("Channels_Add", add);
            PackShader.Dispatch(2, input.width / 8, input.height / 8, 1);

            Texture2D tex = new Texture2D(renderTex.width, renderTex.height, TextureFormat.RGBA64, true, config.FileOutput.ColorSpace == ColorSpace.Linear);
            RenderTexture.active = renderTex;
            tex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            tex.filterMode = renderTex.filterMode;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.alphaIsTransparency = false;
            tex.Apply();

            Save(tex, config, overwriteName: config.FileOutput.FileName + namePostfix);
        }

        public static void ExportChannels(Texture2D input, TexturePackerConfig config, bool[] exportChannels, bool exportAsBlackAndWhite)
        {
            RenderTexture target = new RenderTexture(input.width, input.height, 24, RenderTextureFormat.ARGB64, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.filterMode = input.filterMode;
            target.Create();
            PackShader.SetTexture(2, "Unpacker_Input", input);
            PackShader.SetTexture(2, "Result", target);

            Vector4 r = new Vector4(1, 0, 0, 0);
            Vector4 g = new Vector4(0, 1, 0, 0);
            Vector4 b = new Vector4(0, 0, 1, 0);
            Vector4 a = new Vector4(0, 0, 0, 1);
            Vector4 none = new Vector4(0, 0, 0, 0);
            Vector4 addAlpha = new Vector4(0, 0, 0, 1);
            if (exportAsBlackAndWhite)
            {
                if (exportChannels[0])
                    ExportChannel(input, target, r, r, r, none, addAlpha, "_R", config);
                if (exportChannels[1])
                    ExportChannel(input, target, g, g, g, none, addAlpha, "_G", config);
                if (exportChannels[2])
                    ExportChannel(input, target, b, b, b, none, addAlpha, "_B", config);
                if (exportChannels[3])
                    ExportChannel(input, target, a, a, a, none, addAlpha, "_A", config);
            }
            else
            {
                if (exportChannels[0])
                    ExportChannel(input, target, r, none, none, none, none, "_R", config);
                if (exportChannels[1])
                    ExportChannel(input, target, none, g, none, none, none, "_G", config);
                if (exportChannels[2])
                    ExportChannel(input, target, none, none, b, none, none, "_B", config);
                if (exportChannels[3])
                    ExportChannel(input, target, none, none, none, a, none, "_A", config);
            }
        }

        #endregion

        #region Save

        public static TextureImporter Save(Texture2D texture, TexturePackerConfig config, string overwriteName = null)
        {
            string path;
            if (!string.IsNullOrWhiteSpace(overwriteName))
            {
                path = Path.Combine(config.FileOutput.SaveFolder, overwriteName + config.FileOutput.SaveType.GetTypeEnding());
            }
            else
            {
                path = Path.Combine(config.FileOutput.SaveFolder, config.FileOutput.FileName + config.FileOutput.SaveType.GetTypeEnding());
            }
            if (File.Exists(path))
            {
                // open dialog
                if (!EditorUtility.DisplayDialog("File already exists", "Do you want to overwrite the file?", "Yes", "No"))
                {
                    return null;
                }
            }
            byte[] bytes = null;
            switch (config.FileOutput.SaveType)
            {
                case SaveType.PNG: bytes = texture.EncodeToPNG(); break;
                case SaveType.JPG: bytes = texture.EncodeToJPG(config.FileOutput.SaveQuality); break;
            }
            System.IO.File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.streamingMipmaps = true;
            importer.sRGBTexture = config.FileOutput.ColorSpace == ColorSpace.Gamma;
            importer.filterMode = config.FileOutput.FilterMode;
            importer.alphaIsTransparency = config.FileOutput.AlphaIsTransparency;
            importer.textureCompression = TextureImporterCompression.Compressed;
            TextureImporterFormat overwriteFormat = importer.DoesSourceTextureHaveAlpha() ?
                Config.Instance.texturePackerCompressionWithAlphaOverwrite : Config.Instance.texturePackerCompressionNoAlphaOverwrite;
            if (overwriteFormat != TextureImporterFormat.Automatic)
            {
                importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
                {
                    name = "PC",
                    overridden = true,
                    maxTextureSize = 2048,
                    format = overwriteFormat
                });
            }
            else
            {
                importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
                {
                    name = "PC",
                    overridden = false,
                });
            }
            config.SaveToImporter(importer);
            importer.SaveAndReimport();
            return importer;
        }
        #endregion
    }
}