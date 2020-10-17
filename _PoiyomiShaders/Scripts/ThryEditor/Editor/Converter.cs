// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Collections;
using System.Collections.Generic;
#if SYSTEM_DRAWING
using System.Drawing.Imaging;
#endif
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class Converter
    {

        public static Color stringToColor(string s)
        {
            s = s.Trim(new char[] { '(', ')' });
            string[] split = s.Split(",".ToCharArray());
            float[] rgba = new float[4] { 1, 1, 1, 1 };
            for (int i = 0; i < split.Length; i++) if (split[i].Replace(" ", "") != "") rgba[i] = float.Parse(split[i]);
            return new Color(rgba[0], rgba[1], rgba[2], rgba[3]);

        }

        public static Vector4 stringToVector(string s)
        {
            s = s.Trim(new char[] { '(', ')' });
            string[] split = s.Split(",".ToCharArray());
            float[] xyzw = new float[4];
            for (int i = 0; i < 4; i++) if (i < split.Length && split[i].Replace(" ", "") != "") xyzw[i] = float.Parse(split[i]); else xyzw[i] = 0;
            return new Vector4(xyzw[0], xyzw[1], xyzw[2], xyzw[3]);
        }

        public static string MaterialsToString(Material[] materials)
        {
            string s = "";
            foreach (Material m in materials)
                s += "\"" + m.name + "\"" + ",";
            s = s.TrimEnd(',');
            return s;
        }

        public static string ArrayToString(object[] a)
        {
            string ret = "";
            foreach (object o in a)
                ret += o.ToString() + ",";
            return ret.TrimEnd(new char[] { ',' });
        }

        public static string ArrayToString(Array a)
        {
            string ret = "";
            foreach (object o in a)
                ret += o.ToString() + ",";
            return ret.TrimEnd(new char[] { ',' });
        }

        //--Start--Gradient
        public static Gradient TextureToGradient(Texture2D texture)
        {
            texture = Gradient_Resize(texture);
            Color[] values = Gradient_Sample(texture);
            //values = Gradient_Smooth(values);
            Color[] delta = CalcDelta(values);
            delta[0] = delta[1];
            Color[] delta_delta = CalcDelta(delta);
            //PrintColorArray(delta_delta);
            List<Color[]> changes = DeltaDeltaToChanges(delta_delta,values);
            changes = RemoveChangesUnderDistanceThreshold(changes);
            SortChanges(changes);
            //PrintColorList(changes);
            return ConstructGradient(changes,values);
        }

        private static Texture2D Gradient_Resize(Texture2D texture)
        {
            return TextureHelper.Resize(texture, 512, 512);
        }

        private static Color[] Gradient_Sample(Texture2D texture)
        {
            texture.wrapMode = TextureWrapMode.Clamp;
            int length = texture.width;
            Color[] ar = new Color[length];
            for(int i = 0; i < length; i++)
            {
                ar[i] = texture.GetPixel(i, i);
            }
            return ar;
        }

        private static Color[] Gradient_Smooth(Color[] values)
        {
            Color[] ar = new Color[values.Length];
            ar[0] = values[0];
            ar[ar.Length - 1] = values[ar.Length - 1];
            for(int i = 1; i < values.Length-1; i++)
            {
                ar[i] = new Color();
                ar[i].r = (values[i - 1].r + values[i].r + values[i + 1].r) / 3;
                ar[i].g = (values[i - 1].g + values[i].g + values[i + 1].g) / 3;
                ar[i].b = (values[i - 1].b + values[i].b + values[i + 1].b) / 3;
            }
            return ar;
        }

        private static Color[] CalcDelta(Color[] values)
        {
            Color[] delta = new Color[values.Length];
            delta[0] = new Color(0, 0, 0);
            for(int i = 1; i < values.Length; i++)
            {
                delta[i] = ColorSubtract(values[i - 1], values[i]);
            }
            return delta;
        }

        private static List<Color[]> DeltaDeltaToChanges(Color[] deltadelta, Color[] values)
        {
            List<Color[]> changes = new List<Color[]>();
            for (int i = 0; i < deltadelta.Length; i++)
            {
                if (deltadelta[i].r != 0 || deltadelta[i].g != 0 || deltadelta[i].b != 0)
                {
                    deltadelta[i].a = i/512.0f;
                    Color[] new_change = new Color[2];
                    new_change[0] = deltadelta[i];
                    new_change[1] = values[i];
                    changes.Add(new_change);
                }
            }
            return changes;
        }

        const float GRADIENT_DISTANCE_THRESHOLD = 0.05f;
        private static List<Color[]> RemoveChangesUnderDistanceThreshold(List<Color[]> changes)
        {
            List<Color[]> new_changes = new List<Color[]>();
            new_changes.Add(changes[0]);
            for(int i = 1; i < changes.Count; i++)
            {

                if (changes[i][0].a-new_changes[new_changes.Count-1][0].a < GRADIENT_DISTANCE_THRESHOLD)
                {
                    if (ColorValueForDelta(new_changes[new_changes.Count - 1][0]) < ColorValueForDelta(changes[i][0]))
                    {
                        new_changes.RemoveAt(new_changes.Count - 1);
                        new_changes.Add(changes[i]);
                    }
                }
                else
                {
                    new_changes.Add(changes[i]);
                }
            }
            return new_changes;
        }

        private static void SortChanges(List<Color[]> changes)
        {
            changes.Sort(delegate (Color[] x, Color[] y)
            {
                float sizeX = ColorValueForDelta(x[0]);
                float sizeY = ColorValueForDelta(y[0]);
                if (sizeX < sizeY) return 1;
                else if (sizeY < sizeX) return -1;
                return 0;
            });
        }

        private static Gradient ConstructGradient(List<Color[]> changes, Color[] values)
        {
            List<GradientAlphaKey> alphas = new List<GradientAlphaKey>();
            List<GradientColorKey> colors = new List<GradientColorKey>();
            for(int i = 0; i < 6 && i < changes.Count; i++)
            {
                colors.Add(new GradientColorKey(changes[i][1], changes[i][0].a));
                //Debug.Log("key " + changes[i][0].a);
            }
            colors.Add(new GradientColorKey(values[0], 0));
            colors.Add(new GradientColorKey(values[values.Length-1], 1));
            alphas.Add(new GradientAlphaKey(1, 0));
            alphas.Add(new GradientAlphaKey(1, 1));
            Gradient gradient = new Gradient();
            gradient.SetKeys(colors.ToArray(), alphas.ToArray());
            return gradient;
        }

        private static void PrintColorArray(Color[] ar)
        {
            foreach (Color c in ar)
                Debug.Log(c.ToString());
        }private static void PrintColorList(List<Color[]> ar)
        {
            foreach (Color[] x in ar)
                Debug.Log(ColorValueForDelta (x[0])+ ":"+x[0].ToString());
        }

        private static float ColorValueForDelta(Color col)
        {
            return Mathf.Abs(col.r) + Mathf.Abs(col.g) + Mathf.Abs(col.b);
        }

        private static Color ColorAdd(Color col1, Color col2)
        {
            return new Color(col1.r + col2.r, col1.g + col2.g, col1.b + col2.b);
        }
        private static Color ColorSubtract(Color col1, Color col2)
        {
            return new Color(col1.r - col2.r, col1.g - col2.g, col1.b - col2.b);
        }

        public static Texture2D GradientToTexture(Gradient gradient, int width, int height)
        {
            Texture2D texture = new Texture2D(width, height);
            for (int x = 0; x < width; x++)
            {
                Color col = gradient.Evaluate((float)x / width);
                for (int y = 0; y < height; y++) texture.SetPixel(x, y, col);
            }
            texture.Apply();
            return texture;
        }

        //--End--Gradient

        public static Texture2D CurveToTexture(AnimationCurve curve, TextureData texture_settings)
        {
            Texture2D texture = new Texture2D(texture_settings.width, texture_settings.height);
            for (int i = 0; i < texture_settings.width; i++)
            {
                Color color = new Color();
                float value = curve.Evaluate((float)i / texture_settings.width);
                value = Mathf.Clamp01(value);
                if (texture_settings.channel == 'r')
                    color.r = value;
                else if (texture_settings.channel == 'g')
                    color.g = value;
                else if (texture_settings.channel == 'b')
                    color.b = value;
                else if (texture_settings.channel == 'a')
                    color.a = value;
                if (texture_settings.channel != 'a')
                    color.a = 1;
                for (int y = 0; y < texture_settings.height; y++)
                    texture.SetPixel(i, y, color);
            }
            texture.Apply();
            texture_settings.ApplyModes(texture);
            return texture;
        }

        //==============Texture Array=================

        public static Texture2DArray PathsToTexture2DArray(string[] paths)
        {
            if (paths[0].EndsWith(".gif"))
            {
                return Converter.GifToTextureArray(paths[0]);
                
            }
            else if(paths != null && paths.Length>0)
            {
                List<Texture2D> textures = new List<Texture2D>();
                foreach (string p in paths)
                {
                    if (AssetDatabase.GetMainAssetTypeAtPath(p).IsSubclassOf(typeof(Texture)))
                        textures.Add(TextureHelper.GetReadableTexture(AssetDatabase.LoadAssetAtPath<Texture>(p)));
                }
                if (textures.Count > 0)
                {
                    Texture2DArray arrayTexture = Textre2DArrayToAsset(textures.ToArray());
                    AssetDatabase.CreateAsset(arrayTexture, paths[0].RemoveFileExtension()+".asset");
                    AssetDatabase.SaveAssets();
                    return arrayTexture;
                }
            }
            return null;
        }

        public static Texture2DArray GifToTextureArray(string path)
        {
            List<Texture2D> array = GetGifFrames(path);
            if (array == null) return null;
            if (array.Count == 0)
            {
                Debug.LogError("Gif is empty or System.Drawing is not working. Try right clicking and reimporting the \"Thry Editor\" Folder!");
                return null;
            }
            Texture2DArray arrayTexture = Textre2DArrayToAsset(array.ToArray());
            AssetDatabase.CreateAsset(arrayTexture, path.Replace(".gif", ".asset"));
            AssetDatabase.SaveAssets();
            return arrayTexture;
        }

        public static List<Texture2D> GetGifFrames(string path)
        {
            List<Texture2D> gifFrames = new List<Texture2D>();
#if SYSTEM_DRAWING
            var gifImage = System.Drawing.Image.FromFile(path);
            var dimension = new FrameDimension(gifImage.FrameDimensionsList[0]);

            int width = Mathf.ClosestPowerOfTwo(gifImage.Width-1);
            int height = Mathf.ClosestPowerOfTwo(gifImage.Height-1);

            bool hasAlpha = false;

            int frameCount = gifImage.GetFrameCount(dimension);

            float totalProgress = frameCount * width;
            for (int i = 0; i < frameCount; i++)
            {
                gifImage.SelectActiveFrame(dimension, i);
                var ogframe = new System.Drawing.Bitmap(gifImage.Width, gifImage.Height);
                System.Drawing.Graphics.FromImage(ogframe).DrawImage(gifImage, System.Drawing.Point.Empty);
                var frame = ResizeBitmap(ogframe,width,height);

                Texture2D frameTexture = new Texture2D(frame.Width, frame.Height);

                float doneProgress = i * width;
                for (int x = 0; x < frame.Width; x++)
                {
                    if(x%20 == 0)
                    if (EditorUtility.DisplayCancelableProgressBar("From GIF", "Frame "+i+": "+(int)((float)x/width*100)+"%", (doneProgress + x + 1) / totalProgress))
                    {
                        EditorUtility.ClearProgressBar();
                        return null;
                    }

                    for (int y = 0; y < frame.Height; y++)
                    {
                        System.Drawing.Color sourceColor = frame.GetPixel(x, y);
                        frameTexture.SetPixel(x, frame.Height - 1 - y, new UnityEngine.Color32(sourceColor.R, sourceColor.G, sourceColor.B, sourceColor.A));
                        if (sourceColor.A < 255.0f)
                        {
                            hasAlpha = true;
                        }
                    }
                }

                frameTexture.Apply();
                gifFrames.Add(frameTexture);
            }
            EditorUtility.ClearProgressBar();
            //Debug.Log("has alpha? " + hasAlpha);
            for(int i = 0; i < frameCount; i++)
            {
                EditorUtility.CompressTexture(gifFrames[i], hasAlpha?TextureFormat.DXT5 : TextureFormat.DXT1, UnityEditor.TextureCompressionQuality.Normal);
                gifFrames[i].Apply(true,false);
            }
#endif
            return gifFrames;
        }

#if SYSTEM_DRAWING
        public static System.Drawing.Bitmap ResizeBitmap(System.Drawing.Image image, int width, int height)
        {
            var destRect = new System.Drawing.Rectangle(0, 0, width, height);
            var destImage = new System.Drawing.Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = System.Drawing.Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, System.Drawing.GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
#endif

        private static Texture2DArray Textre2DArrayToAsset(Texture2D[] array)
        {
            Texture2DArray texture2DArray = new Texture2DArray(array[0].width, array[0].height, array.Length, array[0].format, true);

#if SYSTEM_DRAWING
            for (int i = 0; i < array.Length; i++)
            {
                for (int m = 0; m < array[i].mipmapCount; m++)
                {
                    UnityEngine.Graphics.CopyTexture(array[i], 0, m, texture2DArray, i, m);
                }
            }
#endif

            texture2DArray.anisoLevel = array[0].anisoLevel;
            texture2DArray.wrapModeU = array[0].wrapModeU;
            texture2DArray.wrapModeV = array[0].wrapModeV;

            texture2DArray.Apply(false, true);

            return texture2DArray;
        }
    }
}
