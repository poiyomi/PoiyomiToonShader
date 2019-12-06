// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Thry
{

    public class Helper
    {

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long GetCurrentUnixTimestampMillis()
        {
            return (long)(DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
        }

        public static long GetUnityStartUpTimeStamp()
        {
            return GetCurrentUnixTimestampMillis() - (long)EditorApplication.timeSinceStartup * 1000;
        }

        public static bool ClassExists(string classname)
        {
            return System.Type.GetType(classname) != null;
        }

        public static bool NameSpaceExists(string namespace_name)
        {
            bool namespaceFound = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                   from type in assembly.GetTypes()
                                   where type.Namespace == namespace_name
                                   select type).Any();
            return namespaceFound;
        }

        public static valuetype GetValueFromDictionary<keytype, valuetype>(Dictionary<keytype, valuetype> dictionary, keytype key)
        {
            valuetype value = default(valuetype);
            if (dictionary.ContainsKey(key)) dictionary.TryGetValue(key, out value);
            return value;
        }

        public static valuetype GetValueFromDictionary<keytype, valuetype>(Dictionary<keytype, valuetype> dictionary, keytype key, valuetype defaultValue)
        {
            valuetype value = default(valuetype);
            if (dictionary.ContainsKey(key)) dictionary.TryGetValue(key, out value);
            else return defaultValue;
            return value;
        }

        //-------------------Comparetors----------------------

        /// <summary>
        /// -1 if v1 > v2
        /// 0 if v1 == v2
        /// 1 if v1 < v2
        /// </summary>
        public static int compareVersions(string v1, string v2)
        {
            Match v1_match = Regex.Match(v1, @"\d+(\.\d+)*");
            Match v2_match = Regex.Match(v2, @"\d+(\.\d+)*");
            if (!v1_match.Success && !v2_match.Success) return 0;
            else if (!v1_match.Success) return 1;
            else if (!v2_match.Success) return -1;
            string[] v1Parts = Regex.Split(v1_match.Value, @"\.");
            string[] v2Parts = Regex.Split(v2_match.Value, @"\.");
            for (int i = 0; i < Math.Max(v1Parts.Length, v2Parts.Length); i++)
            {
                if (i >= v1Parts.Length) return 1;
                else if (i >= v2Parts.Length) return -1;
                int v1P = int.Parse(v1Parts[i]);
                int v2P = int.Parse(v2Parts[i]);
                if (v1P > v2P) return -1;
                else if (v1P < v2P) return 1;
            }
            return 0;
        }

        public static bool IsPrimitive(Type t)
        {
            return t.IsPrimitive || t == typeof(Decimal) || t == typeof(String);
        }

        
    }

    public class FileHelper
    {
        public static string FindFile(string name)
        {
            return FindFile(name, null);
        }

        public static string FindFile(string name, string type)
        {
            string[] guids;
            if (type != null)
                guids = AssetDatabase.FindAssets(name + " t:" + type);
            else
                guids = AssetDatabase.FindAssets(name);
            if (guids.Length == 0)
                return null;
            return AssetDatabase.GUIDToAssetPath(guids[0]);
        }

        //-----------------------Value To File Saver----------------------

        private static Dictionary<string, string> textFileData = new Dictionary<string, string>();

        public static string LoadValueFromFile(string key, string path)
        {
            if (!textFileData.ContainsKey(path)) textFileData[path] = ReadFileIntoString(path);
            Match m = Regex.Match(textFileData[path], Regex.Escape(key) + @"\s*:=.*(?=\r?\n)");
            string value = Regex.Replace(m.Value, key + @"\s*:=\s*", "");
            if (m.Success) return value;
            return null;
        }


        public static bool SaveValueToFileKeyIsRegex(string keyRegex, string value, string path)
        {
            if (!textFileData.ContainsKey(path)) textFileData[path] = ReadFileIntoString(path);
            Match m = Regex.Match(textFileData[path], keyRegex + @"\s*:=.*\r?\n");
            if (m.Success) textFileData[path] = textFileData[path].Replace(m.Value, m.Value.Substring(0, m.Value.IndexOf(":=")) + ":=" + value + "\n");
            else textFileData[path] += Regex.Unescape(keyRegex) + ":=" + value + "\n";
            WriteStringToFile(textFileData[path], path);
            return true;
        }

        public static bool SaveValueToFile(string key, string value, string path)
        {
            return SaveValueToFileKeyIsRegex(Regex.Escape(key), value, path);
        }

        //-----------------------File Interaction---------------------

        public static string FindFileAndReadIntoString(string fileName)
        {
            string[] guids = AssetDatabase.FindAssets(fileName);
            if (guids.Length > 0)
                return ReadFileIntoString(AssetDatabase.GUIDToAssetPath(guids[0]));
            else return "";
        }

        public static void FindFileAndWriteString(string fileName, string s)
        {
            string[] guids = AssetDatabase.FindAssets(fileName);
            if (guids.Length > 0)
                WriteStringToFile(s, AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        public static string ReadFileIntoString(string path)
        {
            if (!File.Exists(path))
            {
                CreateFileWithDirectories(path);
                return "";
            }
            StreamReader reader = new StreamReader(path);
            string ret = reader.ReadToEnd();
            reader.Close();
            return ret;
        }

        public static void WriteStringToFile(string s, string path)
        {
            if (!File.Exists(path)) CreateFileWithDirectories(path);
            StreamWriter writer = new StreamWriter(path, false);
            writer.Write(s);
            writer.Close();
        }

        public static bool writeBytesToFile(byte[] bytes, string path)
        {
            if (!File.Exists(path)) if (!File.Exists(path)) CreateFileWithDirectories(path);
            try
            {
                using (var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fs.Write(bytes, 0, bytes.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Exception caught in process: " + ex.ToString());
                return false;
            }
        }

        public static void CreateFileWithDirectories(string path)
        {
            string dir_path = path.GetDirectoryPath();
            if (dir_path != "")
                Directory.CreateDirectory(dir_path);
            File.Create(path).Close();
        }
    }

    public class TrashHandler
    {
        public static void EmptyThryTrash()
        {
            if (Directory.Exists(PATH.DELETING_DIR))
            {
                DeleteDirectory(PATH.DELETING_DIR);
            }
        }

        public static void MoveDirectoryToTrash(string path)
        {
            string name = path.RemovePath();
            if (!Directory.Exists(PATH.DELETING_DIR))
                Directory.CreateDirectory(PATH.DELETING_DIR);
            int i = 0;
            string newpath = PATH.DELETING_DIR + "/" + name + i;
            while (Directory.Exists(newpath))
                newpath = PATH.DELETING_DIR + "/" + name + (++i);
            Directory.Move(path, newpath);
        }

        static void DeleteDirectory(string path)
        {
            foreach (string f in Directory.GetFiles(path))
                DeleteFile(f);
            foreach (string d in Directory.GetDirectories(path))
                DeleteDirectory(d);
            if (Directory.GetFiles(path).Length + Directory.GetDirectories(path).Length == 0)
                Directory.Delete(path);
        }
        static void DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception e)
            {
                e.GetType();
            }
        }
    }

    public class TextureHelper
    {
        public static Gradient GetGradient(Texture texture)
        {
            if (texture != null)
            {
                string gradient_data_string = FileHelper.LoadValueFromFile(texture.name, PATH.GRADIENT_INFO_FILE);
                if (gradient_data_string != null)
                {
                    return Parser.ParseToObject<Gradient>(gradient_data_string);
                }
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

        public static Texture SaveTextureAsPNG(Texture2D texture, string path, TextureData settings)
        {
            if (!path.EndsWith(".png"))
                path += ".png";
            byte[] encoding = texture.EncodeToPNG();
            Debug.Log("Texture saved at \"" + path + "\".");
            FileHelper.writeBytesToFile(encoding, path);

            AssetDatabase.ImportAsset(path);
            if (settings != null)
                settings.ApplyModes(path);
            Texture saved = AssetDatabase.LoadAssetAtPath<Texture>(path);
            return saved;
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
    }

    public class MaterialHelper
    {
        public static void UpdateRenderQueue(Material material, Shader defaultShader)
        {
            if (material.shader.renderQueue != material.renderQueue)
            {
                Shader renderQueueShader = defaultShader;
                if (material.renderQueue != renderQueueShader.renderQueue) renderQueueShader = ShaderHelper.createRenderQueueShaderIfNotExists(defaultShader, material.renderQueue, true);
                material.shader = renderQueueShader;
                ShaderImportFixer.backupSingleMaterial(material);
            }
        }

        public static void UpdateTargetsValue(MaterialProperty p, System.Object value)
        {
            if (p.type == MaterialProperty.PropType.Texture)
                foreach (UnityEngine.Object m in p.targets)
                    ((Material)m).SetTexture(p.name, (Texture)value);
            else if (p.type == MaterialProperty.PropType.Float)
            {
                foreach (UnityEngine.Object m in p.targets)
                    if (value.GetType() == typeof(float))
                        ((Material)m).SetFloat(p.name, (float)value);
                    else if (value.GetType() == typeof(int))
                        ((Material)m).SetFloat(p.name, (int)value);
            }
        }

        public static void UpdateTextureValue(MaterialProperty prop, Texture texture)
        {
            foreach (UnityEngine.Object m in prop.targets)
            {
                ((Material)m).SetTexture(prop.name, texture);
            }
            prop.textureValue = texture;
        }

        public static void UpdateFloatValue(MaterialProperty prop, float f)
        {
            foreach (UnityEngine.Object m in prop.targets)
            {
                ((Material)m).SetFloat(prop.name, f);
            }
            prop.floatValue = f;
        }
    }

    public class ColorHelper
    {
        public static Color Subtract(Color col1, Color col2)
        {
            return ColorMath(col1, col2, 1, -1);
        }

        public static Color ColorMath(Color col1, Color col2, float multiplier1, float multiplier2)
        {
            return new Color(col1.r * multiplier1 + col2.r * multiplier2, col1.g * multiplier1 + col2.g * multiplier2, col1.b * multiplier1 + col2.b * multiplier2);
        }

        public static float ColorDifference(Color col1, Color col2)
        {
            return Math.Abs(col1.r - col2.r) + Math.Abs(col1.g - col2.g) + Math.Abs(col1.b - col2.b) + Math.Abs(col1.a - col2.a);
        }
    }
}