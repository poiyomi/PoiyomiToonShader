// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Collections;
using System.Collections.Generic;
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
            if ((Type.GetType("System.Drawing.Image, System.Drawing") != null))
            {
                Type t_img = Type.GetType("System.Drawing.Image, System.Drawing");
                MethodInfo m_from_file = t_img.GetMethod("FromFile", new Type[] { typeof(string) });
                MethodInfo m_get_frame_count = t_img.GetMethod("GetFrameCount");
                MethodInfo m_select_active_frame = t_img.GetMethod("SelectActiveFrame");

                MethodInfo m_dispose = t_img.GetMethod("Dispose");
                PropertyInfo p_width = t_img.GetProperty("Width");
                PropertyInfo p_height = t_img.GetProperty("Width");

                Type t_frame_dim = Type.GetType("System.Drawing.Imaging.FrameDimension, System.Drawing");
                PropertyInfo p_frame_time = t_frame_dim.GetProperty("Time");

                Type t_bitmap = Type.GetType("System.Drawing.Bitmap, System.Drawing");
                PropertyInfo p_bitmap_raw_format = t_bitmap.GetProperty("RawFormat");
                ConstructorInfo c_bitmap = t_bitmap.GetConstructor(new Type[] { t_img });

                Type t_format = Type.GetType("System.Drawing.Imaging.ImageFormat, System.Drawing");
                MethodInfo m_save = t_img.GetMethod("Save", new Type[] { typeof(MemoryStream), t_format });

                EditorUtility.DisplayProgressBar("Creating Texture Array for " + path, "", 0);
                object IMG = m_from_file.Invoke(null, new object[] { path });
                int Length = (int)m_get_frame_count.Invoke(IMG, new object[] { p_frame_time.GetValue(null, null) });

                m_select_active_frame.Invoke(IMG, new object[] { p_frame_time.GetValue(null, null), 0 });
                array = new Texture2DArray((int)p_width.GetValue(IMG,null), (int)p_height.GetValue(IMG, null), Length, TextureFormat.RGBA32, true, false);

                for (int i = 0; i < Length; i++)
                {
                    EditorUtility.DisplayProgressBar("Creating Texture Array for " + path, "Converting frame #" + i, (float)i / Length);
                    m_select_active_frame.Invoke(IMG, new object[] { p_frame_time.GetValue(null, null), i });
                    object bitmap = c_bitmap.Invoke(new object[] { IMG });
                    MemoryStream msFinger = new MemoryStream();
                    m_save.Invoke(IMG, new object[] { msFinger, p_bitmap_raw_format.GetValue(bitmap, null) });
                    Texture2D texture = new Texture2D((int)p_width.GetValue(IMG, null), (int)p_height.GetValue(IMG, null));
                    texture.LoadImage(msFinger.ToArray());
                    array.SetPixels(texture.GetPixels(), i);
                }
                m_dispose.Invoke(IMG, null);
                EditorUtility.ClearProgressBar();

                array.Apply();
                string newPath = path.Replace(".gif", ".asset");
                AssetDatabase.CreateAsset(array, newPath);

            }
            else
            {
                UnityFixer.CheckAPICompatibility();
                UnityFixer.CheckDrawingDll();
                Debug.LogWarning("System.Drawing could not be found. Trying to automatically fix by adding csc/mcs.");
            }
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

        public static Texture2D CurveToTexture(AnimationCurve curve, TextureData texture_settings)
        {
            Texture2D texture = new Texture2D(texture_settings.width, texture_settings.height);
            for(int i = 0; i < texture_settings.width; i++)
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
    }
}
