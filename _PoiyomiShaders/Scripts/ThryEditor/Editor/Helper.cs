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
            string dir_path = Path.GetDirectoryName(path);
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
            string name = Path.GetFileName(path);
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
            }else if (key == "render_type")
            {
                foreach (Material m in materials) m.SetOverrideTag("RenderType", value);
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
                if (float.TryParse(Parser.GlobalizationFloat(value), out f_value))
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
            List<Color[]> changes = DeltaDeltaToChanges(delta_delta, values);
            changes = RemoveChangesUnderDistanceThreshold(changes);
            SortChanges(changes);
            //PrintColorList(changes);
            return ConstructGradient(changes, values);
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
            for (int i = 0; i < length; i++)
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
            for (int i = 1; i < values.Length - 1; i++)
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
            for (int i = 1; i < values.Length; i++)
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
                    deltadelta[i].a = i / 512.0f;
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
            for (int i = 1; i < changes.Count; i++)
            {

                if (changes[i][0].a - new_changes[new_changes.Count - 1][0].a < GRADIENT_DISTANCE_THRESHOLD)
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
            for (int i = 0; i < 6 && i < changes.Count; i++)
            {
                colors.Add(new GradientColorKey(changes[i][1], changes[i][0].a));
                //Debug.Log("key " + changes[i][0].a);
            }
            colors.Add(new GradientColorKey(values[0], 0));
            colors.Add(new GradientColorKey(values[values.Length - 1], 1));
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
        }
        private static void PrintColorList(List<Color[]> ar)
        {
            foreach (Color[] x in ar)
                Debug.Log(ColorValueForDelta(x[0]) + ":" + x[0].ToString());
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

        [MenuItem("Assets/Thry/Flipbooks/Images 2 TextureArray",false, 303)]
        static void SelectionImagesToTextureArray()
        {
            string[] paths = Selection.assetGUIDs.Select(g => AssetDatabase.GUIDToAssetPath(g)).ToArray();
            PathsToTexture2DArray(paths);
        }

        [MenuItem("Assets/Thry/Flipbooks/Images 2 TextureArray", true)]
        static bool SelectionImagesToTextureArrayValidator()
        {
            if (Selection.assetGUIDs != null && Selection.assetGUIDs.Length > 0)
            {
                return Selection.assetGUIDs.All(g => Regex.IsMatch(AssetDatabase.GUIDToAssetPath(g), @".*\.(png)|(jpg)"));
            }
            return false;
        }

        public static Texture2DArray PathsToTexture2DArray(string[] paths)
        {
            if (paths.Length == 0)
                return null;
            if (paths[0].EndsWith(".gif"))
            {
                return Converter.GifToTextureArray(paths[0]);
            }
            else
            {
#if SYSTEM_DRAWING
                Texture2D[] wew = paths.Where(p=> AssetDatabase.GetMainAssetTypeAtPath(p).IsAssignableFrom(typeof(Texture2D))).Select(p => AssetDatabase.LoadAssetAtPath<Texture2D>(p)).ToArray();
                Array.Sort(wew, (UnityEngine.Object one, UnityEngine.Object two) => one.name.CompareTo(two.name));
                Selection.objects = wew;
                Texture2DArray texture2DArray = new Texture2DArray(wew[0].width, wew[0].height, wew.Length, wew[0].format, true);

                string assetPath = AssetDatabase.GetAssetPath(wew[0]);
                assetPath = assetPath.Remove(assetPath.LastIndexOf('/')) + "/Texture2DArray.asset";

                for (int i = 0; i < wew.Length; i++)
                {
                    for (int m = 0; m < wew[i].mipmapCount; m++)
                    {
                        Graphics.CopyTexture(wew[i], 0, m, texture2DArray, i, m);
                    }
                }

                texture2DArray.anisoLevel = wew[0].anisoLevel;
                texture2DArray.wrapModeU = wew[0].wrapModeU;
                texture2DArray.wrapModeV = wew[0].wrapModeV;
                texture2DArray.Apply(false, true);

                AssetDatabase.CreateAsset(texture2DArray, assetPath);
                AssetDatabase.SaveAssets();

                Selection.activeObject = texture2DArray;
                return texture2DArray;
#endif
            }
            return null;
        }

        [MenuItem("Assets/Thry/Flipbooks/Gif 2 TextureArray",false, 303)]
        static void SelectionGifToTextureArray()
        {
            GifToTextureArray(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]));
        }

        [MenuItem("Assets/Thry/Flipbooks/Gif 2 TextureArray", true)]
        static bool SelectionGifToTextureArrayValidator()
        {
            if (Selection.assetGUIDs != null && Selection.assetGUIDs.Length > 0)
            {
                return AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]).EndsWith(".gif");
            }
            return false;
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
            var dimension = new System.Drawing.Imaging.FrameDimension(gifImage.FrameDimensionsList[0]);

            int width = Mathf.ClosestPowerOfTwo(gifImage.Width - 1);
            int height = Mathf.ClosestPowerOfTwo(gifImage.Height - 1);

            bool hasAlpha = false;

            int frameCount = gifImage.GetFrameCount(dimension);

            float totalProgress = frameCount * width;
            for (int i = 0; i < frameCount; i++)
            {
                gifImage.SelectActiveFrame(dimension, i);
                var ogframe = new System.Drawing.Bitmap(gifImage.Width, gifImage.Height);
                System.Drawing.Graphics.FromImage(ogframe).DrawImage(gifImage, System.Drawing.Point.Empty);
                var frame = ResizeBitmap(ogframe, width, height);

                Texture2D frameTexture = new Texture2D(frame.Width, frame.Height);

                float doneProgress = i * width;
                for (int x = 0; x < frame.Width; x++)
                {
                    if (x % 20 == 0)
                        if (EditorUtility.DisplayCancelableProgressBar("From GIF", "Frame " + i + ": " + (int)((float)x / width * 100) + "%", (doneProgress + x + 1) / totalProgress))
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
            for (int i = 0; i < frameCount; i++)
            {
                EditorUtility.CompressTexture(gifFrames[i], hasAlpha ? TextureFormat.DXT5 : TextureFormat.DXT1, UnityEditor.TextureCompressionQuality.Normal);
                gifFrames[i].Apply(true, false);
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

                using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
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

    public class ShaderHelper
    {

        private static Dictionary<Shader, Dictionary<string, string[]>> shader_property_drawers = new Dictionary<Shader, Dictionary<string, string[]>>();
        public static string[] GetDrawer(MaterialProperty property)
        {
            Shader shader = ((Material)property.targets[0]).shader;

            if (!shader_property_drawers.ContainsKey(shader))
                LoadShaderPropertyDrawers(shader);

            Dictionary<string, string[]> property_drawers = shader_property_drawers[shader];
            if (property_drawers.ContainsKey(property.name))
                return property_drawers[property.name];
            return null;
        }

        public static void LoadShaderPropertyDrawers(Shader shader)
        {
            string path = AssetDatabase.GetAssetPath(shader);
            string code = FileHelper.ReadFileIntoString(path);
            code = Helper.GetStringBetweenBracketsAndAfterId(code, "Properties", new char[] { '{', '}' });
            MatchCollection matchCollection = Regex.Matches(code, @"\[.*\].*(?=\()");
            Dictionary<string, string[]> property_drawers = new Dictionary<string, string[]>();
            foreach (Match match in matchCollection)
            {
                string[] drawers_or_flag_code = GetDrawersFlagsCode(match.Value);
                string drawer_code = GetNonFlagDrawer(drawers_or_flag_code);
                if (drawer_code == null)
                    continue;

                string property_name = Regex.Match(match.Value, @"(?<=\])[^\[]*$").Value.Trim();

                List<string> drawer_and_parameters = new List<string>();
                drawer_and_parameters.Add(Regex.Split(drawer_code, @"\(")[0]);

                GetDrawerParameters(drawer_code, drawer_and_parameters);

                property_drawers[property_name] = drawer_and_parameters.ToArray();
            }
            shader_property_drawers[shader] = property_drawers;
        }

        private static void GetDrawerParameters(string code, List<string> list)
        {
            MatchCollection matchCollection = Regex.Matches(code, @"(?<=\(|,).*?(?=\)|,)");
            foreach (Match m in matchCollection)
                list.Add(m.Value);
        }

        private static string GetNonFlagDrawer(string[] codes)
        {
            foreach (string c in codes)
                if (!DrawerIsFlag(c))
                    return c;
            return null;
        }

        private static bool DrawerIsFlag(string code)
        {
            return (code == "HideInInspector" || code == "NoScaleOffset" || code == "Normal" || code == "HDR" || code == "Gamma" || code == "PerRendererData");
        }

        private static string[] GetDrawersFlagsCode(string line)
        {
            MatchCollection matchCollection = Regex.Matches(line, @"(?<=\[).*?(?=\])");
            string[] codes = new string[matchCollection.Count];
            int i = 0;
            foreach (Match m in matchCollection)
                codes[i++] = m.Value;
            return codes;
        }
        //------------Track ShaderEditor shaders-------------------

        public class ShaderEditorShader
        {
            public string path;
            public string name;
            public string version;
        }

        private static List<ShaderEditorShader> shaders;
        private static Dictionary<string, ShaderEditorShader> dictionary;
        public static List<ShaderEditorShader> thry_editor_shaders
        {
            get
            {
                Init();
                return shaders;
            }
        }

        private static void Init()
        {
            if (shaders == null)
                LoadShaderEditorShaders();
        }

        private static void Add(ShaderEditorShader s)
        {
            Init();
            if (!dictionary.ContainsKey(s.name))
            {
                dictionary.Add(s.name, s);
                shaders.Add(s);
            }
        }

        private static void RemoveAt(int i)
        {
            dictionary.Remove(shaders[i].name);
            shaders.RemoveAt(i--);
        }

        public static string[] GetShaderEditorShaderNames()
        {
            string[] r = new string[thry_editor_shaders.Count];
            for (int i = 0; i < r.Length; i++)
                r[i] = thry_editor_shaders[i].name;
            return r;
        }

        public static bool IsShaderUsingShaderEditor(Shader shader)
        {
            Init();
            return dictionary.ContainsKey(shader.name);
        }


        private static void LoadShaderEditorShaders()
        {
            string data = FileHelper.ReadFileIntoString(PATH.THRY_EDITOR_SHADERS);
            if (data != "")
            {
                shaders = Parser.ParseToObject<List<ShaderEditorShader>>(data);
                InitDictionary();
            }
            else
            {
                dictionary = new Dictionary<string, ShaderEditorShader>();
                SearchAllShadersForShaderEditorUsage();
            }
            DeleteNull();
        }

        private static void InitDictionary()
        {
            dictionary = new Dictionary<string, ShaderEditorShader>();
            foreach (ShaderEditorShader s in shaders)
            {
                if (s.name != null && !dictionary.ContainsKey(s.name))
                    dictionary.Add(s.name, s);
            }
        }

        public static void SearchAllShadersForShaderEditorUsage()
        {
            shaders = new List<ShaderEditorShader>();
            string[] guids = AssetDatabase.FindAssets("t:shader");
            foreach (string g in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                TestShaderForShaderEditor(path);
            }
            Save();
        }

        private static void DeleteNull()
        {
            bool save = false;
            int length = shaders.Count;
            for (int i = 0; i < length; i++)
            {
                if (shaders[i] == null)
                {
                    RemoveAt(i--);
                    length--;
                    save = true;
                }
            }
            if (save)
                Save();
        }

        private static void Save()
        {
            FileHelper.WriteStringToFile(Parser.ObjectToString(shaders), PATH.THRY_EDITOR_SHADERS);
        }

        private static string GetActiveCustomEditorParagraph(string code)
        {
            Match match = Regex.Match(code, @"(^|\*\/)((.|\n)(?!(\/\*)))*CustomEditor\s*\""(\w|\d)*\""((.|\n)(?!(\/\*)))*");
            if (match.Success) return match.Value;
            return null;
        }

        private static bool ParagraphContainsActiveShaderEditorDefinition(string code)
        {
            Match match = Regex.Match(code, @"\n\s+CustomEditor\s+\""ShaderEditor\""");
            return match.Success;
        }

        private static bool ShaderUsesShaderEditor(string code)
        {
            string activeCustomEditorParagraph = GetActiveCustomEditorParagraph(code);
            if (activeCustomEditorParagraph == null)
                return false;
            return ParagraphContainsActiveShaderEditorDefinition(activeCustomEditorParagraph);
        }

        private static bool TestShaderForShaderEditor(string path)
        {
            string code = FileHelper.ReadFileIntoString(path);
            if (ShaderUsesShaderEditor(code))
            {
                ShaderEditorShader shader = new ShaderEditorShader();
                shader.path = path;
                Match name_match = Regex.Match(code, @"(?<=[Ss]hader)\s*\""[^\""]+(?=\""\s*{)");
                if (name_match.Success) shader.name = name_match.Value.TrimStart(new char[] { ' ', '"' });
                Match master_label_match = Regex.Match(code, @"\[HideInInspector\]\s*shader_master_label\s*\(\s*\""[^\""]*(?=\"")");
                if (master_label_match.Success) shader.version = GetVersionFromMasterLabel(master_label_match.Value);
                Add(shader);
                return true;
            }
            return false;
        }

        private static string GetVersionFromMasterLabel(string label)
        {
            Match match = Regex.Match(label, @"(?<=v|V)\d+(\.\d+)*");
            if (!match.Success)
                match = Regex.Match(label, @"\d+(\.\d+)+");
            if (match.Success)
                return match.Value;
            return null;
        }

        public static void AssetsImported(string[] paths)
        {
            bool save = false;
            foreach (string path in paths)
            {
                if (!path.EndsWith(".shader"))
                    continue;
                if (TestShaderForShaderEditor(path))
                    save = true;
            }
            if (save)
                Save();
        }

        public static void AssetsDeleted(string[] paths)
        {
            bool save = false;
            foreach (string path in paths)
            {
                if (!path.EndsWith(".shader"))
                    continue;
                int length = thry_editor_shaders.Count;
                for (int i = 0; i < length; i++)
                {
                    if (thry_editor_shaders[i] != null && thry_editor_shaders[i].path == path)
                    {
                        RemoveAt(i--);
                        length--;
                        save = true;
                    }
                }
            }
            if (save)
                Save();
        }

        public static void AssetsMoved(string[] old_paths, string[] paths)
        {
            bool save = false;
            for (int i = 0; i < paths.Length; i++)
            {
                if (!paths[i].EndsWith(".shader"))
                    continue;
                foreach (ShaderEditorShader s in thry_editor_shaders)
                {
                    if (s == null) continue;
                    if (s.path == old_paths[i])
                    {
                        s.path = paths[i];
                        save = true;
                    }
                }
            }
            if (save)
                Save();
        }

    }

    public class StringHelper
    {
        public static string GetBetween(string value, string prefix, string postfix)
        {
            return GetBetween(value, prefix, postfix, value);
        }

        public static string GetBetween(string value, string prefix, string postfix, string fallback)
        {
            string pattern = @"(?<=" + prefix + ").*?(?=" + postfix + ")";
            Match m = Regex.Match(value, pattern);
            if (m.Success)
                return m.Value;
            return fallback;
        }

        //returns data for name:{data} even if data containss brakets
        public static string GetBracket(string data, string bracketName)
        {
            Match m = Regex.Match(data, bracketName + ":");
            if (m.Success)
            {
                int startIndex = m.Index + bracketName.Length + 2;
                int i = startIndex;
                int depth = 0;
                while (++i < data.Length)
                {
                    if (data[i] == '{')
                        depth++;
                    else if (data[i] == '}')
                    {
                        if (depth == 0)
                            break;
                        depth--;
                    }
                }
                return data.Substring(startIndex, i - startIndex);
            }
            return data;
        }
    }

    public class VRCInterface
    {
        private static VRCInterface instance;
        public static VRCInterface Get()
        {
            if (instance == null) instance = new VRCInterface();
            return instance;
        }
        public static void Update()
        {
            instance = new VRCInterface();
        }

        public SDK_Information sdk_information;

        public class SDK_Information
        {
            public VRC_SDK_Type type;
            public string installed_version = "0";
        }

        public enum VRC_SDK_Type
        {
            NONE = 0,
            SDK_2 = 1,
            SDK_3_Avatar = 2,
            SDK_3_World = 3
        }

        private VRCInterface()
        {
            sdk_information = new SDK_Information();
            sdk_information.type = GetInstalledSDKType();
            InitInstalledSDKVersionAndPaths();
        }

        private void InitInstalledSDKVersionAndPaths()
        {
            string[] guids = AssetDatabase.FindAssets("version");
            string path = null;
            string u_path = null;
            foreach (string guid in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                if (p.Contains("VRCSDK/version"))
                    path = p;
            }
            if (path == null || !File.Exists(path))
                return;
            string persistent = PersistentData.Get("vrc_sdk_version");
            if (persistent != null)
                sdk_information.installed_version = persistent;
            else
                sdk_information.installed_version = Regex.Replace(FileHelper.ReadFileIntoString(path), @"\n?\r", "");
        }

        public VRC_SDK_Type GetInstalledSDKType()
        {
#if VRC_SDK_VRCSDK3 && UDON
            return VRC_SDK_Type.SDK_3_World;
#elif VRC_SDK_VRCSDK3
            return VRC_SDK_Type.SDK_3_Avatar;
#endif
#if VRC_SDK_VRCSDK2
            return VRC_SDK_Type.SDK_2;
#endif
            return VRC_SDK_Type.NONE;
        }

        private static bool IsVRCSDKInstalled()
        {
#if VRC_SDK_VRCSDK3
            return true;
#endif
#if VRC_SDK_VRCSDK2
            return true;
#endif
            return false;
        }
    }
}
