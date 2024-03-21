// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace Thry
{
    static class StringExtensions
    {
        public static string ReplaceVariables(this string s, params object[] values)
        {
            for(int i = 0; i < values.Length;i++)
            {
                s = s.Replace("{" + i + "}", values[i].ToString());
            }
            return s;
        }
    }

    public class Helper
    {
        static bool s_didTryRegsiterThisSession = false;

        public static bool ClassWithNamespaceExists(string classname)
        {
            return (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    from type in assembly.GetTypes()
                    where type.FullName == classname
                    select type).Count() > 0;
        }

        public static Type FindTypeByFullName(string fullname)
        {
            return (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    from type in assembly.GetTypes()
                    where type.FullName == fullname
                    select type).FirstOrDefault();
        }

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long GetCurrentUnixTimestampMillis()
        {
            return (long)(DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
        }

        public static long DatetimeToUnixSeconds(DateTime time)
        {
            return (long)(time - UnixEpoch).TotalSeconds;
        }

        public static long GetUnityStartUpTimeStamp()
        {
            return GetCurrentUnixTimestampMillis() - (long)EditorApplication.timeSinceStartup * 1000;
        }

        public static void RegisterEditorUse()
        {
            if (s_didTryRegsiterThisSession) return;
            if (!EditorPrefs.GetBool("thry_has_counted_user", false))
            {
                WebHelper.DownloadStringASync(URL.COUNT_USER, delegate (string s)
                {
                    if (s == "true")
                        EditorPrefs.SetBool("thry_has_counted_user", true);
                });
            }

            string projectPrefix = PlayerSettings.companyName + "." + PlayerSettings.productName;
            if (!EditorPrefs.GetBool(projectPrefix + "_thry_has_counted_project", false))
            {
                WebHelper.DownloadStringASync(URL.COUNT_PROJECT, delegate (string s)
                {
                    if (s == "true")
                        EditorPrefs.SetBool(projectPrefix + "_thry_has_counted_project", true);
                });
            }
            s_didTryRegsiterThisSession = true;
        }

        //-------------------Comparetors----------------------

        public static int CompareVersions(string v1, string v2)
        {
            //fix the string
            v1 = v1.Replace(",", ".");
            v2 = v2.Replace(",", ".");
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
                if (index_v1 < v1.Length)
                {
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
                }
                else
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

        public static float SolveMath(string exp, float parameter)
        {
            exp = exp.Replace("x", parameter.ToString(CultureInfo.InvariantCulture));
            exp = exp.Replace(" ", "");
            float f;
            if (ExpressionEvaluator.Evaluate<float>(exp, out f)) return f;
            return 0;
        }

        public static float Mod(float a, float b)
        {
            return a - b * Mathf.Floor(a / b);
        }

        // This code is an implementation of the pseudocode from the Wikipedia,
        // showing a naive implementation.
        // You should research an algorithm with better space complexity.
        public static int LevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];
            if (n == 0)
            {
                return m;
            }
            if (m == 0)
            {
                return n;
            }
            for (int i = 0; i <= n; d[i, 0] = i++)
                ;
            for (int j = 0; j <= m; d[0, j] = j++)
                ;
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }

        // Start of Detour methods
        // Modified from: https://github.com/apkd/UnityStaticBatchingSortingPatch/blob/e83bed8cf31fc98097586c4e47af77fa79d9bed5/StaticBatchingSortingPatch.cs
        // Modified by Behemoth/hill
        static Dictionary<MethodInfo, byte[]> s_patchedData = new Dictionary<MethodInfo, byte[]>();
        public static unsafe void TryDetourFromTo(MethodInfo src, MethodInfo dst)
        {
            try
            {
                if (IntPtr.Size == sizeof(Int64))
                {
                    // 64-bit systems use 64-bit absolute address and jumps
                    // 12 byte destructive

                    // Get function pointers
                    long Source_Base        = src     .MethodHandle.GetFunctionPointer().ToInt64();
                    long Destination_Base   = dst.MethodHandle.GetFunctionPointer().ToInt64();

                    // Backup Source Data
                    IntPtr Source_IntPtr    = src.MethodHandle.GetFunctionPointer();
                    var backup = new byte[0xC];
                    Marshal.Copy(Source_IntPtr, backup, 0, 0xC);
                    s_patchedData.Add(src, backup);

                    // Native source address
                    byte* Pointer_Raw_Source = (byte*)Source_Base;

                    // Pointer to insert jump address into native code
                    long* Pointer_Raw_Address = (long*)( Pointer_Raw_Source + 0x02 );

                    // Insert 64-bit absolute jump into native code (address in rax)
                    // mov rax, immediate64
                    // jmp [rax]
                    *( Pointer_Raw_Source + 0x00 ) = 0x48;
                    *( Pointer_Raw_Source + 0x01 ) = 0xB8;
                    *Pointer_Raw_Address           = Destination_Base; // ( Pointer_Raw_Source + 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 )
                    *( Pointer_Raw_Source + 0x0A ) = 0xFF;
                    *( Pointer_Raw_Source + 0x0B ) = 0xE0;
                }
                else
                {
                    // 32-bit systems use 32-bit relative offset and jump
                    // 5 byte destructive

                    // Get function pointers
                    int Source_Base        = src     .MethodHandle.GetFunctionPointer().ToInt32();
                    int Destination_Base   = dst.MethodHandle.GetFunctionPointer().ToInt32();

                    // Backup Source Data
                    IntPtr Source_IntPtr    = src.MethodHandle.GetFunctionPointer();
                    var backup = new byte[0x5];
                    Marshal.Copy(Source_IntPtr, backup, 0, 0x5);
                    s_patchedData.Add(src, backup);

                    // Native source address
                    byte* Pointer_Raw_Source = (byte*)Source_Base;

                    // Pointer to insert jump address into native code
                    int* Pointer_Raw_Address = (int*)( Pointer_Raw_Source + 1 );

                    // Jump offset (less instruction size)
                    int offset = ( Destination_Base - Source_Base ) - 5;

                    // Insert 32-bit relative jump into native code
                    *Pointer_Raw_Source = 0xE9;
                    *Pointer_Raw_Address = offset;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unable to detour: {src?.Name ?? "UnknownSrc"} -> {dst?.Name ?? "UnknownDst"}\n{ex}");
                throw;
            }
        }
        
        public static unsafe void RestoreDetour(MethodInfo src) {
            var Source_IntPtr = src.MethodHandle.GetFunctionPointer();
            var backup = s_patchedData[src];
            Marshal.Copy(backup, 0, Source_IntPtr, backup.Length);
            s_patchedData.Remove(src);
        }
        // End of Detour Methods
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

        public static T Get<T>(string key, T defaultValue)
        {
            string s = FileHelper.LoadValueFromFile(key, PATH.PERSISTENT_DATA);
            if (string.IsNullOrEmpty(s)) return defaultValue;
            T obj = Parser.Deserialize<T>(s);
            if (obj == null) return defaultValue;
            return obj;
        }

        public static void Set(string key, object value)
        {
            FileHelper.SaveValueToFile(key, Parser.Serialize(value), PATH.PERSISTENT_DATA);
        }
    }

    public class FileHelper
    {
        public static string FindFile(string name, string type=null)
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

        private static Dictionary<string, Dictionary<string,string>> s_textFileData = new Dictionary<string, Dictionary<string, string>>();

        public static string LoadValueFromFile(string key, string path)
        {
            if (!s_textFileData.ContainsKey(path)) ReadFileIntoTextFileData(path);
            if (s_textFileData[path].ContainsKey(key))
                return s_textFileData[path][key];
            return null;
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
            s_textFileData[path] = dictionary; 
        }

        public static bool SaveValueToFile(string key, string value, string path)
        {
            if (!s_textFileData.ContainsKey(path)) ReadFileIntoTextFileData(path);
            s_textFileData[path][key] = value;
            return SaveDictionaryToFile(path, s_textFileData[path]);
        }

        public static void RemoveValueFromFile(string key, string path)
        {
            if (!s_textFileData.ContainsKey(path)) ReadFileIntoTextFileData(path);
            if (s_textFileData[path].ContainsKey(key)) s_textFileData[path].Remove(key);
        }

        private static bool SaveDictionaryToFile(string path, Dictionary<string,string> dictionary)
        {
            s_textFileData[path] = dictionary;
            string data = s_textFileData[path].Aggregate("", (d1, d2) => d1 + d2.Key + ":=" + d2.Value + "\n");
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

        public static bool WriteBytesToFile(byte[] bytes, string path)
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
                string path = AssetDatabase.GetAssetPath(texture);
                string gradient_data_string = null;
                if(path != null) gradient_data_string = FileHelper.LoadValueFromFile(AssetDatabase.AssetPathToGUID(path), PATH.GRADIENT_INFO_FILE);
                //For Backwards compatibility check old id (name) if guid cant be found
                if(gradient_data_string == null) gradient_data_string  = FileHelper.LoadValueFromFile(texture.name, PATH.GRADIENT_INFO_FILE);
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
                    if(displayProgressbar && y % 50 == 0) EditorUtility.DisplayProgressBar("Loading Raw TGA", "Loading " + TGAFile, (float)y / b.height);
                    for (int x = 0; x < b.width; x++)
                    {
                        texX = x;
                        texY = y;
                        if(startsAtRight) texX = b.width - x - 1;
                        if(startsAtTop) texY = b.height - y - 1;
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
                if(displayProgressbar) EditorUtility.ClearProgressBar();

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

    public class MaterialHelper
    {
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
            ToggleKeyword(p.targets as Material[], keyword, on);
        }

        /// <summary>
        /// Set Material Property value or Renderqueue of current Editor.
        /// </summary>
        /// <param name="key">Property Name or "render_queue"</param>
        /// <param name="value"></param>
        public static void SetMaterialValue(string key, string value)
        {
            Material[] materials = ShaderEditor.Active.Materials;
            if (ShaderEditor.Active.PropertyDictionary.TryGetValue(key, out ShaderProperty p))
            {
                MaterialHelper.SetMaterialPropertyValue(p.MaterialProperty, value);
                p.UpdateKeywordFromValue();
            }
            else if (key == "render_queue")
            {
                int q = 0;
                if (int.TryParse(value, out q))
                {
                    foreach (Material m in materials) m.renderQueue = q;
                }
            }
            else if (key == "render_type")
            {
                foreach (Material m in materials) m.SetOverrideTag("RenderType", value);
            }
            else if (key == "preview_type")
            {
                foreach (Material m in materials) m.SetOverrideTag("PreviewType", value);
            }
            else if (key == "ignore_projector")
            {
                foreach (Material m in materials) m.SetOverrideTag("IgnoreProjector", value);
            }
        }

        public static void SetMaterialPropertyValue(MaterialProperty p, string value)
        {
            object prev = null;
            if (p.type == MaterialProperty.PropType.Texture)
            {
                prev = p.textureValue;
                p.textureValue = AssetDatabase.LoadAssetAtPath<Texture>(value);
            }
            else if (p.type == MaterialProperty.PropType.Float || p.type == MaterialProperty.PropType.Range)
            {
                prev = p.floatValue;
                p.floatValue = Parser.ParseFloat(value, p.floatValue);
            }
#if UNITY_2022_1_OR_NEWER
            else if (p.type == MaterialProperty.PropType.Int)
            {
                prev = p.intValue;
                p.intValue = (int)Parser.ParseFloat(value, p.intValue);
            }
#endif
            else if (p.type == MaterialProperty.PropType.Vector)
            {
                prev = p.vectorValue;
                p.vectorValue = Converter.StringToVector(value);
            }
            else if (p.type == MaterialProperty.PropType.Color)
            {
                prev = p.colorValue;
                p.colorValue = Converter.StringToColor(value);
            }
            if (p.applyPropertyCallback != null)
                p.applyPropertyCallback.Invoke(p, 1, prev);
        }

        public static void CopyPropertyValueFromMaterial(MaterialProperty p, Material source)
        {
            if (!source.HasProperty(p.name)) return;
            object prev = null;
            switch (p.type)
            {
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    prev = p.floatValue;
                    p.floatValue = source.GetNumber(p);
                    break;
#if UNITY_2022_1_OR_NEWER
                case MaterialProperty.PropType.Int:
                    prev = p.intValue;
                    p.intValue = source.GetInt(p.name);
                    break;
#endif
                case MaterialProperty.PropType.Color:
                    prev = p.colorValue;
                    p.colorValue = source.GetColor(p.name);
                    break;
                case MaterialProperty.PropType.Vector:
                    prev = p.vectorValue;
                    p.vectorValue = source.GetVector(p.name);
                    break;
                case MaterialProperty.PropType.Texture:
                    prev = p.textureValue;
                    p.textureValue = source.GetTexture(p.name);
                    Vector2 offset = source.GetTextureOffset(p.name);
                    Vector2 scale = source.GetTextureScale(p.name);
                    p.textureScaleAndOffset = new Vector4(scale.x, scale.y, offset.x, offset.y);
                    break;
            }
            if (p.applyPropertyCallback != null)
                p.applyPropertyCallback.Invoke(p, 1, prev);
        }

        public static void CopyMaterialValueFromProperty(MaterialProperty target, MaterialProperty source)
        {
            object prev = null;
            switch (target.type)
            {
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    prev = target.floatValue;
                    target.floatValue = source.floatValue;
                    break;
#if UNITY_2022_1_OR_NEWER
                case MaterialProperty.PropType.Int:
                    prev = target.intValue;
                    target.intValue = source.intValue;
                    break;
#endif
                case MaterialProperty.PropType.Color:
                    prev = target.colorValue;
                    target.colorValue = source.colorValue;
                    break;
                case MaterialProperty.PropType.Vector:
                    prev = target.vectorValue;
                    target.vectorValue = source.vectorValue;
                    break;
                case MaterialProperty.PropType.Texture:
                    prev = target.textureValue;
                    target.textureValue = source.textureValue;
                    target.textureScaleAndOffset = source.textureScaleAndOffset;
                    break;
            }
            if (target.applyPropertyCallback != null)
                target.applyPropertyCallback.Invoke(target, 1, prev);
        }

        public static void CopyPropertyValueToMaterial(MaterialProperty source, Material target)
        {
            CopyMaterialValueFromProperty(MaterialEditor.GetMaterialProperty(new Material[] { target }, source.name), source);
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

        public static Color StringToColor(string s)
        {
            s = s.Trim(new char[] { '(', ')' });
            string[] split = s.Split(",".ToCharArray());
            float[] rgba = new float[4] { 1, 1, 1, 1 };
            for (int i = 0; i < split.Length; i++) if (string.IsNullOrWhiteSpace(split[i]) == false) rgba[i] = float.Parse(split[i]);
            return new Color(rgba[0], rgba[1], rgba[2], rgba[3]);

        }

        public static Vector4 StringToVector(string s)
        {
            s = s.Trim(new char[] { '(', ')' });
            string[] split = s.Split(",".ToCharArray());
            float[] xyzw = new float[4];
            for (int i = 0; i < 4 && i < split.Length; i++) if (string.IsNullOrWhiteSpace(split[i]) == false) xyzw[i] = float.Parse(split[i]); else xyzw[i] = 0;
            return new Vector4(xyzw[0], xyzw[1], xyzw[2], xyzw[3]);
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

        public static Texture2D ColorToTexture(Color color, int width, int height)
        {
            width = Mathf.Max(0, Mathf.Min(8192, width));
            height = Mathf.Max(0, Mathf.Min(8192, height));
            Texture2D texture = new Texture2D(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    texture.SetPixel(x, y, color);
                }
            }
            texture.Apply();
            return texture;
        }
        
        public static Texture2D GradientToTexture(Gradient gradient, int width, int height, bool vertical = false)
        {
            width = Mathf.Max(0, Mathf.Min(8192, width));
            height = Mathf.Max(0, Mathf.Min(8192, height));
            Texture2D texture = new Texture2D(width, height);
            Color col;
            if(vertical)
            {
                for (int y = 0; y < height; y++)
                {
                    col = gradient.Evaluate((float)y / height);
                    for (int x = 0; x < width; x++) texture.SetPixel(x, y, col);
                }
            }else
            {
                for (int x = 0; x < width; x++)
                {
                    col = gradient.Evaluate((float)x / width);
                    for (int y = 0; y < height; y++) texture.SetPixel(x, y, col);
                }
            }
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
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
#else
                return null;
#endif
            }
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

        // [MenuItem("Thry/Shader Editor/Test")]
        // public static void Test()
        // {
        //     Shader shader = Shader.Find(".poiyomi/Poiyomi 8.1/Poiyomi Pro");
        //     Debug.Log(IsShaderUsingThryEditor(shader));
        // }

        public static bool IsShaderUsingThryEditor(Shader shader)
        {
            return IsShaderUsingThryEditor(new Material(shader));
        }
        public static bool IsShaderUsingThryEditor(Material material)
        {
            return IsShaderUsingThryEditor(MaterialEditor.CreateEditor(material) as MaterialEditor);
        }
        public static bool IsShaderUsingThryEditor(MaterialEditor materialEditor)
        {
            PropertyInfo shaderGUIProperty = typeof(MaterialEditor).GetProperty("customShaderGUI");
            var gui = shaderGUIProperty.GetValue(materialEditor);
            // gui is null for some shaders. I think it has to do with packages maybe
            return (gui != null) && gui.GetType() == typeof(ShaderEditor);
        }

        internal static List<(string prop, List<string> keywords)> GetPropertyKeywordsForShader(Shader s)
        {
            List<(string prop, List<string> keywords)> list = new List<(string prop, List<string> keywords)>();

            for (int i = 0; i < s.GetPropertyCount(); i++)
            {
                if (s.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Float)
                {
                    string prop = s.GetPropertyName(i);
                    List<string> keywords = null;
                    keywords = GetKeywordsFromShaderProperty(s, prop);

                    if(keywords.Count == 0)
                        continue;
                    else
                        list.Add((prop, keywords));
                }
            }

            return list;
        }

        // Logic Adapted from unity's reference implementation
        /// <summary> Returns a list of keywords for a given shader property. </summary>
        internal static List<string> GetKeywordsFromShaderProperty(Shader shader, string propertyName)
        {
            List<string> keywords = new List<string>();
            if (string.IsNullOrEmpty(propertyName)) 
                return keywords;
            
            int propertyIndex = shader.FindPropertyIndex(propertyName);
            if (propertyIndex < 0) 
                return keywords;

            string[] attributes = shader.GetPropertyAttributes(propertyIndex);
            if (attributes.Length == 0 || attributes == null) 
                return keywords;

            foreach (string attribute in attributes)
            {
                string args = "";
                // Regex based on Unity's reference implementation: Match a string of the form Keyword(Argument) and capture its components
                //   (\w+)    - Match a word (keyword name)
                //   \s*\(\s* - Match an opening parenthesis, allowing whitespace before and after
                //   ([^)]*)  - Match any number of characters that are not a closing parenthesis, and capture them into a group
                //   \s*\)    - Match a closing parenthesis, allowing whitespace before and after
                const string propertyDrawerRegex = @"(\w+)\s*\(\s*([^)]*)\s*\)";

                Match regexMatch = Regex.Match(attribute, propertyDrawerRegex);
                if (regexMatch.Success)
                {
                    string className = regexMatch.Groups[1].Value;
                    args = regexMatch.Groups[2].Value.Trim();

                    // Note that we don't handle ToggleOff as it would require extra logic to differentiate
                    if(className == "Toggle") // Unity Toggle drawer, toggles a keyword directly if provided as [Toggle(KEYWORD)] and toggles PropertyName
                    {
                        if(string.IsNullOrEmpty(args))
                            keywords.Add(GetUnityKeywordName(propertyName, "ON"));
                        else
                            keywords.Add(args);
                        break;
                    }
                    else if(className == "ThryToggle") // Thry Toggle drawer, toggles a keyword directly if provided as [Toggle(KEYWORD)]
                    {
                        // We only care about the first argument, the second is for UI
                        if(args.Contains(","))
                            args = args.Split(',')[0];

                        // Ignore ThryToggle's bools, since otherwise we get keywords that have the same name as HLSL language keywords
                        if(args != "false" && args != "true")
                            keywords.Add(args);

                        break;
                    }
                    else if(className == "KeywordEnum") // Keyword enum, enables one keyword out of a list of keywords provided as [KeywordEnum(KEYWORD1,KEYWORD2,KEYWORD3)]
                    {
                        string[] enumArgs = args.Split(',');
                        foreach(var enumArg in enumArgs)
                        {
                            keywords.Add(GetUnityKeywordName(propertyName, enumArg.Trim()));
                        }

                        break;
                    }
                }
            }
            return keywords;
        }

        // Logic from Unity defaults
        /// <summary> Gets a formatted Keyword name from a shader property name and a keyword name. </summary>
        private static string GetUnityKeywordName(string propertyName, string keywordName) => $"{propertyName}_{keywordName}".Replace(' ', '_').ToUpperInvariant();
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
        private static VRCInterface _Instance;
        public static VRCInterface Get()
        {
            if (_Instance == null) _Instance = new VRCInterface();
            return _Instance;
        }
        public static void Update()
        {
            _Instance = new VRCInterface();
        }

        public SDK_Information Sdk_information;

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
            Sdk_information = new SDK_Information();
            Sdk_information.type = GetInstalledSDKType();
            InitInstalledSDKVersionAndPaths();
        }

        private void InitInstalledSDKVersionAndPaths()
        {
            string[] guids = AssetDatabase.FindAssets("version");
            string path = null;
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
                Sdk_information.installed_version = persistent;
            else
                Sdk_information.installed_version = Regex.Replace(FileHelper.ReadFileIntoString(path), @"\n?\r", "");
        }

        public static VRC_SDK_Type GetInstalledSDKType()
        {
#if VRC_SDK_VRCSDK3 && UDON
            return VRC_SDK_Type.SDK_3_World;
#elif VRC_SDK_VRCSDK3
            return VRC_SDK_Type.SDK_3_Avatar;
#elif VRC_SDK_VRCSDK2
            return VRC_SDK_Type.SDK_2;
#else
            return VRC_SDK_Type.NONE;
#endif
        }

        public static bool IsVRCSDKInstalled()
        {
#if VRC_SDK_VRCSDK3
            return true;
#elif VRC_SDK_VRCSDK2
            return true;
#else
            return false;
#endif
        }
    }

    //Adapted from https://github.com/lukis101/VRCUnityStuffs/blob/master/Scripts/Editor/MaterialCleaner.cs
    //MIT License

    //Copyright (c) 2019 Dj Lukis.LT

    //Permission is hereby granted, free of charge, to any person obtaining a copy
    //of this software and associated documentation files (the "Software"), to deal
    //in the Software without restriction, including without limitation the rights
    //to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    //copies of the Software, and to permit persons to whom the Software is
    //furnished to do so, subject to the following conditions:

    //The above copyright notice and this permission notice shall be included in all
    //copies or substantial portions of the Software.

    //THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    //IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    //FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    //AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    //LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    //OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    //SOFTWARE.
    public class MaterialCleaner
    {
        public enum CleanPropertyType { Texture,Float,Color }

        private const string PropPath_Tex = "m_SavedProperties.m_TexEnvs";
        private const string PropPath_Float = "m_SavedProperties.m_Floats";
        private const string PropPath_Col = "m_SavedProperties.m_Colors";

        static string GetPath(CleanPropertyType type)
        {
            if (type == CleanPropertyType.Float) return PropPath_Float;
            if (type == CleanPropertyType.Color) return PropPath_Col;
            return PropPath_Tex;
        }
        public static int CountAllUnusedProperties(params Material[] materials)
        {;
            return materials.Sum(m =>
            {
                int count = 0;
                SerializedObject serObj = new SerializedObject(m);
                count += CountUnusedProperties(m, serObj, CleanPropertyType.Texture);
                count += CountUnusedProperties(m, serObj, CleanPropertyType.Float);
                count += CountUnusedProperties(m, serObj, CleanPropertyType.Color);
                return count;
            });
        }
        private static int CountUnusedProperties(Material mat, SerializedObject serObj, CleanPropertyType type, List<string> list = null)
        {
            var properties = serObj.FindProperty(GetPath(type));
            int count = 0;
            if (properties != null && properties.isArray)
            {
                for (int i = 0; i < properties.arraySize; i++)
                {
                    string propName = properties.GetArrayElementAtIndex(i).displayName;
                    if (!mat.HasProperty(propName))
                    {
                        if (list!=null) list.Add(propName);
                        count++;
                    }
                }
            }
            return count;
        }
        public static int ListUnusedProperties(CleanPropertyType type, params Material[] materials)
        {
            List<string> list = new List<string>();
            int count = materials.Sum(m => CountUnusedProperties(m, new SerializedObject(m), type, list));
            if(count > 0) ShaderEditor.Out($"Unbound properties of type {type}", list.Distinct().Select(s => $"↳{s}"));
            return count;
        }
        public static int CountUnusedProperties(CleanPropertyType type, params Material[] materials)
        {
            return materials.Sum(m => CountUnusedProperties(m, new SerializedObject(m), type));
        }

        private static int RemoveUnusedProperties(Material mat, SerializedObject serObj, CleanPropertyType type)
        {
            if (!mat.shader.isSupported)
            {
                Debug.LogWarning("Skipping \"" + mat.name + "\" cleanup because shader is unsupported!");
                return 0;
            }
            Undo.RecordObject(mat, "Material property cleanup");
            int removedprops = 0;
            string path = PropPath_Tex;
            if (type == CleanPropertyType.Float) path = PropPath_Float;
            if (type == CleanPropertyType.Color) path = PropPath_Col;
            var properties = serObj.FindProperty(path);
            if (properties != null && properties.isArray)
            {
                int amount = properties.arraySize;
                for (int i = amount - 1; i >= 0; i--) // reverse loop because array gets modified
                {
                    string propName = properties.GetArrayElementAtIndex(i).displayName;
                    if (!mat.HasProperty(propName))
                    {
                        properties.DeleteArrayElementAtIndex(i);
                        removedprops++;
                    }
                }
                if (removedprops > 0)
                    serObj.ApplyModifiedProperties();
            }
            return removedprops;
        }
        public static int RemoveUnusedProperties(CleanPropertyType type, params Material[] materials)
        {
            return materials.Sum(m => RemoveUnusedProperties(m, new SerializedObject(m), type));
        }
        private static int RemoveAllUnusedProperties(Material mat, SerializedObject serObj)
        {
            int removedprops = 0;
            removedprops += RemoveUnusedProperties(mat, serObj, CleanPropertyType.Texture);
            removedprops += RemoveUnusedProperties(mat, serObj, CleanPropertyType.Float);
            removedprops += RemoveUnusedProperties(mat, serObj, CleanPropertyType.Color);

            Debug.Log("Removed " + removedprops + " unused properties from " + mat.name);
            return removedprops;
        }
        public static int RemoveAllUnusedProperties(CleanPropertyType type, params Material[] materials)
        {
            return materials.Sum(m => RemoveAllUnusedProperties(m, new SerializedObject(m)));
        }
        private static void ClearKeywords(Material mat)
        {
            Undo.RecordObject(mat, "Material keyword clear");
            string[] keywords = mat.shaderKeywords;
            mat.shaderKeywords = new string[0];
        }
    }
}
