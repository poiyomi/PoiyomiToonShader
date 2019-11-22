// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Collections;
using System.Collections.Generic;
#if DOT_NET_TWO_POINT_ZERO_OR_ABOVE
#if IMAGING_DLL_EXISTS
using System.Drawing.Imaging;
#endif
#endif
using System.IO;
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
            Debug.Log("Texture converted to gradient.");

            int d = (int)Mathf.Sqrt(Mathf.Pow(texture.width, 2) + Mathf.Pow(texture.height, 2));
            List<GradientColorKey> colorKeys = new List<GradientColorKey>();
            List<GradientAlphaKey> alphaKeys = new List<GradientAlphaKey>();
            colorKeys.Add(new GradientColorKey(texture.GetPixel(texture.width - 1, texture.height - 1), 1));
            alphaKeys.Add(new GradientAlphaKey(texture.GetPixel(texture.width - 1, texture.height - 1).a, 1));
            colorKeys.Add(new GradientColorKey(texture.GetPixel(0, 0), 0));
            alphaKeys.Add(new GradientAlphaKey(texture.GetPixel(0, 0).a, 0));
            int colKeys = 0;
            int alphaKeysCount = 0;

            bool isFlat = false;
            bool isNotFlat = false;

            float[][] prevSteps = new float[][] { GetSteps(GetColorAtI(texture, 0, d), GetColorAtI(texture, 1, d)), GetSteps(GetColorAtI(texture, 0, d), GetColorAtI(texture, 1, d)) };

            bool wasFlat = false;
            int maxBetweenFlats = 3;
            int minFlat = 3;
            int flats = 0;
            int prevFlats = 0;
            int nonFlats = 0;
            float[][] steps = new float[d][];
            float[] alphaStep = new float[d];
            Color prevColor = GetColorAtI(texture, 0, d);
            for (int i = 0; i < d; i++)
            {
                Color col = GetColorAtI(texture, i, d);
                steps[i] = GetSteps(prevColor, col);
                alphaStep[i] = Mathf.Abs(prevColor.a - col.a);
                prevColor = col;
            }
            for (int r = 0; r < 1; r++)
            {
                for (int i = 1; i < d - 1; i++)
                {
                    //Debug.Log(i+": "+steps[i][0] + "," + steps[i][1] + ","+steps[i][0]);
                    bool returnToOldVal = false;
                    if (!SameSteps(steps[i], steps[i + 1]) && SimilarSteps(steps[i], steps[i + 1], 0.1f))
                    {
                        int n = i;
                        while (++n < d && SimilarSteps(steps[i - 1], steps[n], 0.1f))
                            if (SameSteps(steps[i - 1], steps[n])) returnToOldVal = true;
                    }
                    if (returnToOldVal) steps[i] = steps[i - 1];

                    returnToOldVal = false;
                    //Debug.Log(i + ": " + steps[i][0] + "," + steps[i][1] + "," + steps[i][0]);
                }
            }


            Color lastStableColor = GetColorAtI(texture, 0, d);
            float lastStableTime = 0;
            bool added = false;
            for (int i = 1; i < d; i++)
            {
                Color col = GetColorAtI(texture, i, d);
                float[] newColSteps = steps[i];
                float time = (float)(i) / d;

                float[] diff = new float[] { prevSteps[0][0] - newColSteps[0], prevSteps[0][1] - newColSteps[1], prevSteps[0][2] - newColSteps[2] };

                if (diff[0] == 0 && diff[1] == 0 && diff[2] == 0)
                {
                    lastStableColor = col;
                    lastStableTime = time;
                    added = false;
                }
                else
                {
                    if (added == false && colKeys++ < 6) colorKeys.Add(new GradientColorKey(lastStableColor, lastStableTime));
                    added = true;
                }

                float alphaDiff = Mathf.Abs(alphaStep[i - 1] - alphaStep[i]);
                if (alphaDiff > 0.05 && ++alphaKeysCount < 6) alphaKeys.Add(new GradientAlphaKey(col.a, time));

                prevSteps[1] = prevSteps[0];
                prevSteps[0] = newColSteps;

                bool thisOneFlat = newColSteps[0] == 0 && newColSteps[1] == 0 && newColSteps[2] == 0;
                if (thisOneFlat) flats++;
                else if (!wasFlat && !thisOneFlat) nonFlats++;
                else if (wasFlat && !thisOneFlat) { prevFlats = flats; flats = 0; nonFlats = 1; }
                if (flats >= minFlat && prevFlats >= minFlat && nonFlats <= maxBetweenFlats) isFlat = true;
                if (nonFlats > maxBetweenFlats) isNotFlat = true;
                wasFlat = thisOneFlat;
            }
            Gradient gradient = new Gradient();
            gradient.SetKeys(colorKeys.ToArray(), alphaKeys.ToArray());
            if (isFlat && !isNotFlat) gradient.mode = GradientMode.Fixed;
            return gradient;
        }

        private static bool SimilarSteps(float[] steps1, float[] steps2, float perc)
        {
            if (Mathf.Abs(steps1[0] - steps2[0]) > perc || Mathf.Abs(steps1[1] - steps2[1]) > perc || Mathf.Abs(steps1[2] - steps2[2]) > perc) return false;
            return steps1[0] == steps2[0] || steps1[1] == steps2[1] || steps1[2] == steps2[2];
        }

        private static bool SameSteps(float[] steps1, float[] steps2)
        {
            return steps1[0] == steps2[0] && steps1[1] == steps2[1] && steps1[2] == steps2[2];
        }

        private static float[] GetSteps(Color col1, Color col2)
        {
            return new float[] { col1.r - col2.r, col1.g - col2.g, col1.b - col2.b };
        }

        private static Color GetColorAtI(Texture2D texture, int i, int d)
        {
            int y = (int)(((float)i) / d * texture.height);
            int x = (int)(((float)i) / d * texture.width);
            Color col = texture.GetPixel(x, y);
            return col;
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

        public static Texture2DArray PathsToTexture2DArray(string[] paths)
        {
            if (paths[0].EndsWith(".gif"))
            {
                return Converter.GifToTextureArray(paths[0]);
                
            }
            else
            {
                List<Texture2D> textures = new List<Texture2D>();
                foreach (string p in paths)
                {
                    if (AssetDatabase.GetMainAssetTypeAtPath(p).IsSubclassOf(typeof(Texture)))
                        textures.Add(TextureHelper.GetReadableTexture(AssetDatabase.LoadAssetAtPath<Texture>(p)));
                }
                if (textures.Count > 0)
                {
                    return Converter.Testure2DListToTexture2DArray(textures,paths[0]);
                }
            }
            return null;
        }

        public static Texture2DArray GifToTextureArray(string path)
        {
            Texture2DArray array = null;
#if DOT_NET_TWO_POINT_ZERO_OR_ABOVE
#if IMAGING_DLL_EXISTS
            EditorUtility.DisplayProgressBar("Creating Texture Array for " + path, "", 0);
            System.Drawing.Image IMG = System.Drawing.Image.FromFile(path);
            int Length = IMG.GetFrameCount(FrameDimension.Time);

            IMG.SelectActiveFrame(FrameDimension.Time, 0);
            array = new Texture2DArray(IMG.Width, IMG.Height, Length, TextureFormat.RGBA32, true, false);

            for (int i = 0; i < Length; i++)
            {
                EditorUtility.DisplayProgressBar("Creating Texture Array for " + path, "Converting frame #" + i, (float)i / Length);
                IMG.SelectActiveFrame(FrameDimension.Time, i);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(IMG);
                MemoryStream msFinger = new MemoryStream();
                IMG.Save(msFinger, bitmap.RawFormat);
                Texture2D texture = new Texture2D(IMG.Width, IMG.Height);
                texture.LoadImage(msFinger.ToArray());
                array.SetPixels(texture.GetPixels(), i);
            }
            IMG.Dispose();
            EditorUtility.ClearProgressBar();

            array.Apply();
            string newPath = path.Replace(".gif", ".asset");
            AssetDatabase.CreateAsset(array, newPath);
#endif
#endif
            return array;
        }

        public static Texture2DArray Testure2DListToTexture2DArray(List<Texture2D> list, string path)
        {
            int[] size = new int[] { list[0].width, list[0].height };
            Texture2DArray array = new Texture2DArray(size[0],size[1], list.Count, list[0].format, true);
            int i = 0;
            foreach(Texture2D texture in list)
            {
                Texture2D resized_texture = texture;
                if (texture.width != size[0] || texture.height != size[1])
                    resized_texture = TextureHelper.Resize(texture, size[0], size[1]);
                array.SetPixels(resized_texture.GetPixels(), i++);
            }
            array.Apply();
            path = path.Remove(path.LastIndexOf('/')) + "/"+ AssetDatabase.LoadAssetAtPath<Texture>(path).name+ "_Texture2DArray.asset";
            AssetDatabase.CreateAsset(array, path);
            return array;
        }

        public static Texture2D CurveToTexture(AnimationCurve curve, int width, int height, char color_channel)
        {
            Texture2D texture = new Texture2D(width, height);
            for(int i = 0; i < width;i++)
            {
                Color color = new Color();
                float value = curve.Evaluate((float)i / width);
                value = Mathf.Clamp01(value);
                if (color_channel == 'r')
                    color.r = value;
                else if (color_channel == 'g')
                    color.g = value;
                else if (color_channel == 'b')
                    color.b = value;
                else if (color_channel == 'a')
                    color.a = value;
                if (color_channel != 'a')
                    color.a = 1;
                    for (int y = 0; y < height; y++)
                    texture.SetPixel(i, y, color);
            }
            texture.Apply();
            return texture;
        }
    }
}
