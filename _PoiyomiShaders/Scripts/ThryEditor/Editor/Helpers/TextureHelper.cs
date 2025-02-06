using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace Thry
{
    public class TextureHelper
    {
        public static Gradient GetGradient(Texture texture)
        {
            if (texture != null)
            {
                string path = AssetDatabase.GetAssetPath(texture);
                string gradient_data_string = null;
                if (path != null) gradient_data_string = FileHelper.LoadValueFromFile(AssetDatabase.AssetPathToGUID(path), PATH.GRADIENT_INFO_FILE);
                //For Backwards compatibility check old id (name) if guid cant be found
                if (gradient_data_string == null) gradient_data_string = FileHelper.LoadValueFromFile(texture.name, PATH.GRADIENT_INFO_FILE);
                if (gradient_data_string != null)
                {
                    Debug.Log(texture.name + " Gradient loaded from file.");
                    Gradient g = Parser.Deserialize<Gradient>(gradient_data_string);
                    return g;
                }
                Debug.Log(texture.name + " Converted into Gradient.");
                return Converter.TextureToGradient(GetReadableTexture(texture));
            }
            return new Gradient();
        }

        private static Texture2D s_BackgroundTexture;

        public static Texture2D GetBackgroundTexture()
        {
            if (s_BackgroundTexture == null)
                s_BackgroundTexture = CreateCheckerTexture(32, 4, 4, Color.white, new Color(0.7f, 0.7f, 0.7f));
            return s_BackgroundTexture;
        }

        public static Texture2D CreateCheckerTexture(int numCols, int numRows, int cellPixelWidth, Color col1, Color col2)
        {
            int height = numRows * cellPixelWidth;
            int width = numCols * cellPixelWidth;

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.hideFlags = HideFlags.HideAndDontSave;
            Color[] pixels = new Color[width * height];

            for (int i = 0; i < numRows; i++)
                for (int j = 0; j < numCols; j++)
                    for (int ci = 0; ci < cellPixelWidth; ci++)
                        for (int cj = 0; cj < cellPixelWidth; cj++)
                            pixels[(i * cellPixelWidth + ci) * width + j * cellPixelWidth + cj] = ((i + j) % 2 == 0) ? col1 : col2;

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        public static Texture SaveTextureAsPNG(Texture2D texture, string path, TextureData settings = null)
        {
            if (!path.EndsWith(".png"))
                path += ".png";
            byte[] encoding = texture.EncodeToPNG();
            Debug.Log("Texture saved at \"" + path + "\".");
            FileHelper.WriteBytesToFile(encoding, path);

            AssetDatabase.ImportAsset(path);
            if (settings != null)
                settings.ApplyModes(path);
            Texture saved = AssetDatabase.LoadAssetAtPath<Texture>(path);
            return saved;
        }

        public static Texture2D ConvertToGamma(Texture2D texture)
        {
            Texture2D ret = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, texture.mipmapCount > 0);
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    Color c = texture.GetPixel(x, y);
                    c.r = Mathf.Pow(c.r, 1/2.2f);
                    c.g = Mathf.Pow(c.g, 1/2.2f);
                    c.b = Mathf.Pow(c.b, 1/2.2f);
                    ret.SetPixel(x, y, c);
                }
            }
            ret.Apply();
            return ret;
        }

        public static void MakeTextureReadible(string path)
        {
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);
            if (!importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }
        }

        public static Texture2D GetReadableTexture(Texture texture)
        {
            RenderTexture temp = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            Graphics.Blit(texture, temp);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = temp;
            Texture2D ret = new Texture2D(texture.width, texture.height);
            ret.ReadPixels(new Rect(0, 0, temp.width, temp.height), 0, 0);
            ret.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(temp);
            return ret;
        }

        public static Texture2D Resize(Texture2D texture, int width, int height)
        {
            Texture2D ret = new Texture2D(width, height, texture.format, texture.mipmapCount > 0);
            float scaleX = ((float)texture.width) / width;
            float scaleY = ((float)texture.height) / height;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    ret.SetPixel(x, y, texture.GetPixel((int)(scaleX * x), (int)(scaleY * y)));
                }
            }
            ret.Apply();
            return ret;
        }

        //===============TGA Loader by aaro4130 https://forum.unity.com/threads/tga-loader-for-unity3d.172291/==============

        public static Texture2D LoadTGA(string TGAFile, bool displayProgressbar = false)
        {
            using (BinaryReader r = new BinaryReader(File.Open(TGAFile, FileMode.Open)))
            {
                byte IDLength = r.ReadByte();
                byte ColorMapType = r.ReadByte();
                byte ImageType = r.ReadByte();
                Int16 CMapStart = r.ReadInt16();
                Int16 CMapLength = r.ReadInt16();
                byte CMapDepth = r.ReadByte();
                Int16 XOffset = r.ReadInt16();
                Int16 YOffset = r.ReadInt16();
                Int16 Width = r.ReadInt16();
                Int16 Height = r.ReadInt16();
                byte PixelDepth = r.ReadByte();
                byte ImageDescriptor = r.ReadByte();
                if (ImageType == 0)
                {
                    EditorUtility.DisplayDialog("Error", "Unsupported TGA file! No image data", "OK");
                    Debug.LogError("Unsupported TGA file! No image data");
                }
                else if (ImageType == 3 | ImageType == 11)
                {
                    EditorUtility.DisplayDialog("Error", "Unsupported TGA file! 8-bit grayscale images are not supported", "OK");
                    Debug.LogError("Unsupported TGA file! Not truecolor");
                }
                else if (ImageType == 9 | ImageType == 10)
                {
                    EditorUtility.DisplayDialog("Error", "Unsupported TGA file! Run-length encoded images are not supported", "OK");
                    Debug.LogError("Unsupported TGA file! Colormapped");

                }
                bool startsAtTop = (ImageDescriptor & 1 << 5) >> 5 == 1;
                bool startsAtRight = (ImageDescriptor & 1 << 4) >> 4 == 1;
                //     MsgBox("Dimensions are "  Width  ","  Height)
                Texture2D b = new Texture2D(Width, Height, TextureFormat.ARGB32, false);
                Color[] colors = new Color[Width * Height];
                int texX = 0;
                int texY = 0;
                int index = 0;
                float red = 0, green = 0, blue = 0, alpha = 0;
                Byte[] bytes = r.ReadBytes((PixelDepth == 32 ? 4 : 3) * Width * Height);

                int byteIndex = 0;
                for (int y = 0; y < b.height; y++)
                {
                    if (displayProgressbar && y % 50 == 0) EditorUtility.DisplayProgressBar("Loading Raw TGA", "Loading " + TGAFile, (float)y / b.height);
                    for (int x = 0; x < b.width; x++)
                    {
                        texX = x;
                        texY = y;
                        if (startsAtRight) texX = b.width - x - 1;
                        if (startsAtTop) texY = b.height - y - 1;
                        index = texX + texY * b.width;

                        blue = Convert.ToSingle(bytes[byteIndex++]);
                        green = Convert.ToSingle(bytes[byteIndex++]);
                        red = Convert.ToSingle(bytes[byteIndex++]);

                        blue = Mathf.Pow(blue / 255, 0.45454545454f);
                        green = Mathf.Pow(green / 255, 0.45454545454f);
                        red = Mathf.Pow(red / 255, 0.45454545454f);

                        // blue /= 255;
                        // green /= 255;
                        // red /= 255;

                        colors[index].r = red;
                        colors[index].g = green;
                        colors[index].b = blue;

                        if (PixelDepth == 32)
                        {
                            alpha = Convert.ToSingle(bytes[byteIndex++]);
                            alpha /= 255;
                            colors[index].a = alpha;
                        }
                        else
                        {
                            colors[index].a = 1;
                        }
                    }
                }
                b.SetPixels(colors);
                b.Apply();
                if (displayProgressbar) EditorUtility.ClearProgressBar();

                return b;
            }
        }

        public class VRAM
        {
            static Dictionary<TextureImporterFormat, int> BPP = new Dictionary<TextureImporterFormat, int>()
    {
        { TextureImporterFormat.BC7 , 8 },
        { TextureImporterFormat.DXT5 , 8 },
        { TextureImporterFormat.DXT5Crunched , 8 },
        { TextureImporterFormat.RGBA32 , 32 },
        { TextureImporterFormat.RGBA16 , 16 },
        { TextureImporterFormat.DXT1 , 4 },
        { TextureImporterFormat.DXT1Crunched , 4 },
        { TextureImporterFormat.RGB24 , 32 },
        { TextureImporterFormat.RGB16 , 16 },
        { TextureImporterFormat.BC5 , 8 },
        { TextureImporterFormat.BC4 , 4 },
        { TextureImporterFormat.R8 , 8 },
        { TextureImporterFormat.R16 , 16 },
        { TextureImporterFormat.Alpha8 , 8 },
        { TextureImporterFormat.RGBAHalf , 64 },
        { TextureImporterFormat.BC6H , 8 },
        { TextureImporterFormat.RGB9E5 , 32 },
        { TextureImporterFormat.ETC2_RGBA8Crunched , 8 },
        { TextureImporterFormat.ETC2_RGB4 , 4 },
        { TextureImporterFormat.ETC2_RGBA8 , 8 },
        { TextureImporterFormat.ETC2_RGB4_PUNCHTHROUGH_ALPHA , 4 },
        { TextureImporterFormat.PVRTC_RGB2 , 2 },
        { TextureImporterFormat.PVRTC_RGB4 , 4 },
        { TextureImporterFormat.ARGB32 , 32 },
        { TextureImporterFormat.ARGB16 , 16 },
        #if (UNITY_2020_1_OR_NEWER || UNITY_2019_4_23 || UNITY_2019_4_24 || UNITY_2019_4_25 || UNITY_2019_4_26 || UNITY_2019_4_27 || UNITY_2019_4_28 || UNITY_2019_4_29 || UNITY_2019_4_30 || UNITY_2019_4_31 || UNITY_2019_4_32 || UNITY_2019_4_33 || UNITY_2019_4_34 || UNITY_2019_4_35 || UNITY_2019_4_36 || UNITY_2019_4_37 || UNITY_2019_4_38 || UNITY_2019_4_39 || UNITY_2019_4_40)
        { TextureImporterFormat.RGBA64 , 64 },
        { TextureImporterFormat.RGB48 , 64 },
        { TextureImporterFormat.RG32 , 32 },
        #endif
    };

            static Dictionary<RenderTextureFormat, int> RT_BPP = new Dictionary<RenderTextureFormat, int>()
        {
            { RenderTextureFormat.ARGB32 , 32 },
            { RenderTextureFormat.Depth , 0 },
            { RenderTextureFormat.ARGBHalf , 64 },
            { RenderTextureFormat.Shadowmap , 8 }, //guessed bpp
            { RenderTextureFormat.RGB565 , 32 }, //guessed bpp
            { RenderTextureFormat.ARGB4444 , 16 },
            { RenderTextureFormat.ARGB1555 , 16 },
            { RenderTextureFormat.Default , 32 },
            { RenderTextureFormat.ARGB2101010 , 32 },
            { RenderTextureFormat.DefaultHDR , 128 },
            { RenderTextureFormat.ARGB64 , 64 },
            { RenderTextureFormat.ARGBFloat , 128 },
            { RenderTextureFormat.RGFloat , 64 },
            { RenderTextureFormat.RGHalf , 32 },
            { RenderTextureFormat.RFloat , 32 },
            { RenderTextureFormat.RHalf , 16 },
            { RenderTextureFormat.R8 , 8 },
            { RenderTextureFormat.ARGBInt , 128 },
            { RenderTextureFormat.RGInt , 64 },
            { RenderTextureFormat.RInt , 32 },
            { RenderTextureFormat.BGRA32 , 32 },
            { RenderTextureFormat.RGB111110Float , 32 },
            { RenderTextureFormat.RG32 , 32 },
            { RenderTextureFormat.RGBAUShort , 64 },
            { RenderTextureFormat.RG16 , 16 },
            { RenderTextureFormat.BGRA10101010_XR , 40 },
            { RenderTextureFormat.BGR101010_XR , 30 },
            { RenderTextureFormat.R16 , 16 }
        };

            public static string ToByteString(long l)
            {
                if (l < 1000) return l + " B";
                if (l < 1000000) return (l / 1000f).ToString("n2") + " KB";
                if (l < 1000000000) return (l / 1000000f).ToString("n2") + " MB";
                else return (l / 1000000000f).ToString("n2") + " GB";
            }

            public static (long size, string format) CalcSize(Texture t)
            {
                string add = "";
                long bytesCount = 0;

                string path = AssetDatabase.GetAssetPath(t);
                if (t != null && path != null && t is RenderTexture == false && t.dimension == UnityEngine.Rendering.TextureDimension.Tex2D)
                {
                    AssetImporter assetImporter = AssetImporter.GetAtPath(path);
                    if (assetImporter is TextureImporter)
                    {
                        TextureImporter textureImporter = (TextureImporter)assetImporter;
                        TextureImporterFormat textureFormat = textureImporter.GetPlatformTextureSettings("PC").format;
#pragma warning disable CS0618
                        if (textureFormat == TextureImporterFormat.AutomaticCompressed) textureFormat = textureImporter.GetAutomaticFormat("PC");
#pragma warning restore CS0618

                        if (BPP.ContainsKey(textureFormat))
                        {
                            add = textureFormat.ToString();
                            double mipmaps = 1;
                            for (int i = 0; i < t.mipmapCount; i++) mipmaps += Math.Pow(0.25, i + 1);
                            bytesCount = (long)(BPP[textureFormat] * t.width * t.height * (textureImporter.mipmapEnabled ? mipmaps : 1) / 8);
                            //Debug.Log(bytesCount);
                        }
                        else
                        {
                            Debug.LogWarning("[Thry][VRAM] Does not have BPP for " + textureFormat);
                        }
                    }
                    else
                    {
                        bytesCount = Profiler.GetRuntimeMemorySizeLong(t);
                    }
                }
                else if (t is RenderTexture)
                {
                    RenderTexture rt = t as RenderTexture;
                    double mipmaps = 1;
                    for (int i = 0; i < rt.mipmapCount; i++) mipmaps += Math.Pow(0.25, i + 1);
                    bytesCount = (long)((RT_BPP[rt.format] + rt.depth) * rt.width * rt.height * (rt.useMipMap ? mipmaps : 1) / 8);
                }
                else
                {
                    bytesCount = Profiler.GetRuntimeMemorySizeLong(t);
                }

                return (bytesCount, add);
            }
        }
    }

}