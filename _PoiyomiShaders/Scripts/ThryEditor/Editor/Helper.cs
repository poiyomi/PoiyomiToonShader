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

        public static int compareVersions(string v1, string v2)
        {
            //fix the string
            Match v1_match = Regex.Match(v1, @"(a|b)?\d+((\.|a|b)\d+)*(a|b)?");
            Match v2_match = Regex.Match(v2, @"(a|b)?\d+((\.|a|b)\d+)*(a|b)?");
            if (!v1_match.Success && !v2_match.Success) return 0;
            else if (!v1_match.Success) return 1;
            else if (!v2_match.Success) return -1;
            v1 = v1_match.Value;
            v2 = v2_match.Value;

            int index_v1 = 0;
            int index_v2 = 0;
            string chunk_v1;
            string chunk_v2;
            while (index_v1 < v1.Length || index_v2 < v2.Length)
            {
                //get a chunk of the strings
                if (index_v1 < v1.Length){
                    chunk_v1 = "";
                    if (v1[index_v1] == 'a')
                        chunk_v1 = "-2";
                    else if (v1[index_v1] == 'b')
                        chunk_v1 = "-1";
                    else
                    {
                        while (index_v1 < v1.Length && v1[index_v1] != 'a' && v1[index_v1] != 'b' && v1[index_v1] != '.')
                            chunk_v1 += v1[index_v1++];
                        if (index_v1 < v1.Length && (v1[index_v1] == 'a' || v1[index_v1] == 'b'))
                            index_v1--;
                    }
                    index_v1++;
                }else
                    chunk_v1 = "0";

                if (index_v2 < v2.Length)
                {
                    chunk_v2 = "";
                    if (v2[index_v2] == 'a')
                        chunk_v2 = "-2";
                    else if (v2[index_v2] == 'b')
                        chunk_v2 = "-1";
                    else
                    {
                        while (index_v2 < v2.Length && v2[index_v2] != 'a' && v2[index_v2] != 'b' && v2[index_v2] != '.')
                            chunk_v2 += v2[index_v2++];
                        if (index_v2 < v2.Length && (v2[index_v2] == 'a' || v2[index_v2] == 'b'))
                            index_v2--;
                    }
                    index_v2++;
                }
                else
                    chunk_v2 = "0";

                //compare chunks
                int v1P = int.Parse(chunk_v1);
                int v2P = int.Parse(chunk_v2);
                if (v1P > v2P) return -1;
                else if (v1P < v2P) return 1;
            }
            return 0;
        }

        public static bool IsPrimitive(Type t)
        {
            return t.IsPrimitive || t == typeof(Decimal) || t == typeof(String);
        }

        public static void testAltClick(Rect rect, ShaderPart property)
        {
            if (ShaderEditor.input.HadMouseDownRepaint && ShaderEditor.input.is_alt_down && rect.Contains(ShaderEditor.input.mouse_position))
            {
                if (property.options.altClick != null)
                    property.options.altClick.Perform();
            }
        }

        public static string GetStringBetweenBracketsAndAfterId(string input, string id, char[] brackets)
        {
            string[] parts = Regex.Split(input, id);
            if (parts.Length > 1)
            {
                char[] behind_id = parts[1].ToCharArray();
                int i = 0;
                int begin = 0;
                int end = behind_id.Length - 1;
                int depth = 0;
                bool escaped = false;
                while (i < behind_id.Length)
                {
                    if (behind_id[i] == brackets[0] && !escaped)
                    {
                        if (depth == 0)
                            begin = i;
                        depth++;
                    }
                    else if (behind_id[i] == brackets[1] && !escaped)
                    {
                        depth--;
                        if (depth == 0)
                        {
                            end = i;
                            break;
                        }
                    }

                    if (behind_id[i] == '\\')
                        escaped = !escaped;
                    else
                        escaped = false;
                    i++;
                }
                return parts[1].Substring(begin, end);
            }
            return input;
        }
    }

    public class PersistentData
    {
        public static string Get(string key)
        {
            return FileHelper.LoadValueFromFile(key, PATH.PERSISTENT_DATA);
        }

        public static void Set(string key, string value)
        {
            FileHelper.SaveValueToFile(key, value, PATH.PERSISTENT_DATA);
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

        private static Dictionary<string, Dictionary<string,string>> textFileData = new Dictionary<string, Dictionary<string, string>>();

        public static string LoadValueFromFile(string key, string path)
        {
            if (!textFileData.ContainsKey(path)) ReadFileIntoTextFileData(path);
            if (textFileData[path].ContainsKey(key))
                return textFileData[path][key];
            return null;
        }

        public static Dictionary<string,string> LoadDictionaryFromFile(string path)
        {
            if (!textFileData.ContainsKey(path)) ReadFileIntoTextFileData(path);
            return textFileData[path];
        }

        private static void ReadFileIntoTextFileData(string path)
        {
            string data = ReadFileIntoString(path);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            MatchCollection matchCollection = Regex.Matches(data, @".*\s*:=.*(?=\r?\n)");
            foreach(Match m in matchCollection)
            {
                string[] keyvalue = m.Value.Split(new string[] { ":=" }, 2, StringSplitOptions.RemoveEmptyEntries);
                if(keyvalue.Length>1)
                    dictionary[keyvalue[0]] = keyvalue[1];
            }
            textFileData[path] = dictionary; 
        }

        public static bool SaveValueToFile(string key, string value, string path)
        {
            if (!textFileData.ContainsKey(path)) ReadFileIntoTextFileData(path);
            textFileData[path][key] = value;
            return SaveDictionaryToFile(path, textFileData[path]);
        }

        public static void RemoveValueFromFile(string key, string path)
        {
            if (!textFileData.ContainsKey(path)) ReadFileIntoTextFileData(path);
            if (textFileData[path].ContainsKey(key)) textFileData[path].Remove(key);
        }

        public static bool SaveDictionaryToFile(string path, Dictionary<string,string> dictionary)
        {
            textFileData[path] = dictionary;
            string data = "";
            foreach (KeyValuePair<string, string> keyvalue in textFileData[path])
            {
                data += keyvalue.Key + ":=" + keyvalue.Value + "\n";
            }
            WriteStringToFile(data, path);
            return true;
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
            if (!File.Exists(path)) CreateFileWithDirectories(path);
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

        public static void ToggleKeyword(Material material, string keyword, bool turn_on)
        {
            bool is_on = material.IsKeywordEnabled(keyword);
            if (is_on && !turn_on)
                material.DisableKeyword(keyword);
            else if (!is_on && turn_on)
                material.EnableKeyword(keyword);
        }

        public static void ToggleKeyword(Material[] materials, string keyword, bool on)
        {
            foreach (Material m in materials)
                ToggleKeyword(m, keyword, on);
        }

        public static void ToggleKeyword(MaterialProperty p, string keyword, bool on)
        {
            foreach (UnityEngine.Object o in p.targets)
                ToggleKeyword((Material)o, keyword, on);
        }

        /// <summary>
        /// Set Material Property value or Renderqueue of current Editor.
        /// </summary>
        /// <param name="key">Property Name or "render_queue"</param>
        /// <param name="value"></param>
        public static void SetMaterialValue(string key, string value)
        {
            MaterialProperty p = ShaderEditor.FindProperty(ShaderEditor.currentlyDrawing.properties, key);
            Material[] materials = ShaderEditor.currentlyDrawing.materials;
            if (p != null)
            {
                MaterialHelper.SetMaterialPropertyValue(p, materials, value);
            }
            else if (key == "render_queue")
            {
                int q = 0;
                if (int.TryParse(value, out q))
                {
                    foreach (Material m in materials) m.renderQueue = q;
                }
            }
        }

        public static void SetMaterialPropertyValue(MaterialProperty p, Material[] materials, string value)
        {
            if (p.type == MaterialProperty.PropType.Texture)
            {
                Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(value);
                if (tex != null)
                    foreach (Material m in materials) m.SetTexture(p.name, tex);
            }
            else if (p.type == MaterialProperty.PropType.Float || p.type == MaterialProperty.PropType.Range)
            {
                float f_value;
                if (float.TryParse(value, out f_value))
                {
                    p.floatValue = f_value;
                    string[] drawer = ShaderHelper.GetDrawer(p);
                    if (drawer != null && drawer.Length > 1 && drawer[0] == "Toggle" && drawer[1] != "__")
                        MaterialHelper.ToggleKeyword(p, drawer[1], f_value == 1);
                }
            }
            else if (p.type == MaterialProperty.PropType.Vector)
            {
                string[] xyzw = value.Split(",".ToCharArray());
                Vector4 vector = new Vector4(float.Parse(xyzw[0]), float.Parse(xyzw[1]), float.Parse(xyzw[2]), float.Parse(xyzw[3]));
                foreach (Material m in materials) m.SetVector(p.name, vector);
            }
            else if (p.type == MaterialProperty.PropType.Color)
            {
                Color col = Converter.stringToColor(value);
                foreach (Material m in materials) m.SetColor(p.name, col);
            }
        }

        public static void CopyPropertyValueFromMaterial(MaterialProperty p, Material source)
        {
            switch (p.type)
            {
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    float f = source.GetFloat(p.name);
                    p.floatValue = f;
                    string[] drawer = ShaderHelper.GetDrawer(p);
                    if (drawer != null && drawer.Length > 1 && drawer[0] == "Toggle" && drawer[1] != "__")
                        ToggleKeyword(p, drawer[1], f == 1);
                    break;
                case MaterialProperty.PropType.Color:
                    Color c = source.GetColor(p.name);
                    p.colorValue = c;
                    break;
                case MaterialProperty.PropType.Vector:
                    Vector4 vector = source.GetVector(p.name);
                    p.vectorValue = vector;
                    break;
                case MaterialProperty.PropType.Texture:
                    Texture t = source.GetTexture(p.name);
                    Vector2 offset = source.GetTextureOffset(p.name);
                    Vector2 scale = source.GetTextureScale(p.name);
                    p.textureValue = t;
                    p.textureScaleAndOffset = new Vector4(scale.x, scale.y, offset.x, offset.y);
                    break;
            }
        }

        public static void CopyPropertyValueToMaterial(MaterialProperty source, Material target)
        {
            switch (source.type)
            {
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    float f = source.floatValue;
                    target.SetFloat(source.name, f);
                    string[] drawer = ShaderHelper.GetDrawer(source);
                    if (drawer != null && drawer.Length > 1 && drawer[0] == "Toggle" && drawer[1] != "__")
                        ToggleKeyword(target, drawer[1], f == 1);
                    break;
                case MaterialProperty.PropType.Color:
                    Color c = source.colorValue;
                    target.SetColor(source.name, c);
                    break;
                case MaterialProperty.PropType.Vector:
                    Vector4 vector = source.vectorValue;
                    target.SetVector(source.name, vector);
                    break;
                case MaterialProperty.PropType.Texture:
                    Texture t = source.textureValue;
                    Vector4 scaleoffset = source.textureScaleAndOffset;
                    target.SetTexture(source.name, t);
                    target.SetTextureOffset(source.name, new Vector2(scaleoffset.z,scaleoffset.w));
                    target.SetTextureScale(source.name, new Vector2(scaleoffset.x,scaleoffset.y));
                    break;
            }
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