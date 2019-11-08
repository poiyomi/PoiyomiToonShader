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

    public static class StringExpensions
    {
        public static string RemovePath(this string url)
        {
            Match m = Regex.Match(url, @"(?<=\/|^)[^\/]+$");
            if (m.Success)
                return m.Value;
            return url;
        }

        /// <summary>
        /// returns string up to (excluding) last '.'
        /// </summary>
        /// <param name="file"></param>
        /// <returns>returns input string if not possible</returns>
        public static string RemoveFileExtension(this string file)
        {
            Match m = Regex.Match(file, @".+?(?=\.|$)");
            if (m.Success)
                return m.Value;
            return file;
        }

        /// <summary>
        /// returns string up to (including) last '/'
        /// </summary>
        /// <param name="file"></param>
        /// <returns>returns input string if not possible</returns>
        public static string RemoveFileName(this string file)
        {
            Match m = Regex.Match(file, @".+\/");
            if (m.Success)
                return m.Value;
            return file;
        }

        /// <summary>
        /// returns string up to (excluding) last '/'
        /// </summary>
        /// <param name="file"></param>
        /// <returns>returns emtpy string if not possible</returns>
        public static string GetDirectoryPath(this string file)
        {
            Match m = Regex.Match(file, @".+(?=\/)");
            if (m.Success)
                return m.Value;
            return "";
        }

        public static bool EndsOnFileExtension(this string s)
        {
            Match m = Regex.Match(s, @"(?<=\/|^)[^\/.]+$");
            return !m.Success;
        }

        public static string RemoveOneDirectory(this string s)
        {
            Match m = Regex.Match(s, @"^.*(?=\/[^\/]*)");
            if (m.Success)
                return m.Value;
            return s;
        }

    }

    public class Helper
    {
        /// <summary>
        /// Finds the path of the specified file in the Unity AssetDatabase
        /// </summary>
        /// <param name="filename">Name of file</param>
        /// <returns>if found returns path else returns filename</returns>
        public static string FindPathOfFileWithExtension(string filename)
        {
            string[] guids = AssetDatabase.FindAssets(filename.RemoveFileExtension());
            foreach (string s in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(s);
                if (path.EndsWith(filename))
                    return path;
            }
            return filename;
        }

        /// <summary>
        /// Finds the paths of all files with specified name in the Unity AssetDatabase
        /// </summary>
        /// <param name="filename">Name of files</param>
        /// <returns>if found returns paths list</returns>
        public static List<string> FindPathsOfFilesWithExtension(string filename)
        {
            List<string> ret = new List<string>();
            string[] guids = AssetDatabase.FindAssets(filename.RemoveFileExtension());
            foreach (string s in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(s);
                if (path.EndsWith(filename))
                    ret.Add(path);
            }
            return ret;
        }

        public static void SendAnalytics()
        {
            string url_values_postfix = "?hash="+ GetMacAddress().GetHashCode();
            if (Config.Get().share_installed_editor_version) url_values_postfix += "&editor=" + Config.Get().verion;
            if (Config.Get().share_installed_unity_version) url_values_postfix += "&unity=" + Application.unityVersion;
            if (Config.Get().share_used_shaders)
            {
                url_values_postfix += "&shaders=[";
                foreach(ShaderHelper.ThryEditorShader s in ShaderHelper.thry_editor_shaders)
                {
                    url_values_postfix += "{\"name\":\""+s.name+ "\",\"version\":\"";
                    if (s.version != null && s.version != "null") url_values_postfix += s.version;
                    url_values_postfix += "\"},";
                }
                url_values_postfix = url_values_postfix.TrimEnd(new char[] { ',' }) + "]";
            }
            DownloadStringASync(URL.DATA_SHARE_SEND+url_values_postfix, null);
        }

        public static string GetMacAddress()
        {
            return (from nic in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                where nic.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up
                select nic.GetPhysicalAddress().ToString()
            ).FirstOrDefault();
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

        /// If gradient is saved get exact value, else tries to build it from texture
        public static Gradient GetGradient(Texture texture)
        {
            if (texture != null)
            {
                string gradient_data_string = Helper.LoadValueFromFile(texture.name, PATH.GRADIENT_INFO_FILE);
                if (gradient_data_string != null)
                {
                    return Parser.ParseToObject<Gradient>(gradient_data_string);
                }
                return Converter.TextureToGradient(Helper.GetReadableTexture(texture));
            }
            return new Gradient();
        }

        //-----------------------string helpers

        public static string FixUrl(string url)
        {
            if (!url.StartsWith("http"))
                url = "http://" + url;
            return url;
        }

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

        //-----------------------Value To File Saver---------------------- TODO Move to own file

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

        //----------------------Shader UI Stuff---------------------

        public static string getDefaultShaderName(string shaderName)
        {
            return shaderName.Split(new string[] { "-queue" }, System.StringSplitOptions.None)[0].Replace(".differentQueues/", "");
        }

        //copys og shader and changed render queue and name in there
        public static Shader createRenderQueueShaderIfNotExists(Shader defaultShader, int renderQueue, bool import)
        {
            string newShaderName = ".differentQueues/" + defaultShader.name + "-queue" + renderQueue;
            Shader renderQueueShader = Shader.Find(newShaderName);
            if (renderQueueShader != null) return renderQueueShader;

            string defaultPath = AssetDatabase.GetAssetPath(defaultShader);
            string shaderCode = ReadFileIntoString(defaultPath);
            string pattern = @"""Queue"" ?= ?""\w+(\+\d+)?""";
            string replacementQueue = "Background+" + (renderQueue - 1000);
            if (renderQueue == 1000) replacementQueue = "Background";
            else if (renderQueue < 1000) replacementQueue = "Background-" + (1000 - renderQueue);
            shaderCode = Regex.Replace(shaderCode, pattern, "\"Queue\" = \"" + replacementQueue + "\"");
            pattern = @"Shader *""(\w|\/|\.)+";
            string ogShaderName = Regex.Match(shaderCode, pattern).Value;
            ogShaderName = Regex.Replace(ogShaderName, @"Shader *""", "");
            string newerShaderName = ".differentQueues/" + ogShaderName + "-queue" + renderQueue;
            shaderCode = Regex.Replace(shaderCode, pattern, "Shader \"" + newerShaderName);
            pattern = @"#include\s*""(?!(Lighting.cginc)|(AutoLight)|(UnityCG)|(UnityShaderVariables)|(HLSLSupport)|(TerrainEngine))";
            shaderCode = Regex.Replace(shaderCode, pattern, "#include \"../", RegexOptions.Multiline);
            string[] pathParts = defaultPath.Split('/');
            string fileName = pathParts[pathParts.Length - 1];
            string newPath = defaultPath.Replace(fileName, "") + "_differentQueues";
            Directory.CreateDirectory(newPath);
            newPath = newPath + "/" + fileName.Replace(".shader", "-queue" + renderQueue + ".shader");
            WriteStringToFile(shaderCode, newPath);
            ShaderImportFixer.scriptImportedAssetPaths.Add(newPath);
            if (import) AssetDatabase.ImportAsset(newPath);

            return Shader.Find(newerShaderName);
        }

        public static void UpdateRenderQueue(Material material, Shader defaultShader)
        {
            if (material.shader.renderQueue != material.renderQueue)
            {
                Shader renderQueueShader = defaultShader;
                if (material.renderQueue != renderQueueShader.renderQueue) renderQueueShader = createRenderQueueShaderIfNotExists(defaultShader, material.renderQueue, true);
                material.shader = renderQueueShader;
                ShaderImportFixer.backupSingleMaterial(material);
            }
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

        //-------------------Unity Helpers-----------------------------

        public static void SetDefineSymbol(string symbol, bool active)
        {
            SetDefineSymbol(symbol, active, false);
        }

        public static void SetDefineSymbol(string symbol, bool active, bool refresh_if_changed)
        {
            try
            {
                string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                        BuildTargetGroup.Standalone);
                if (!symbols.Contains(symbol) && active)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(
                                  BuildTargetGroup.Standalone, symbols + ";" + symbol);
                    AssetDatabase.Refresh();
                }
                else if (symbols.Contains(symbol) && !active)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(
                                  BuildTargetGroup.Standalone, Regex.Replace(symbols, @";?" + @symbol, ""));
                    AssetDatabase.Refresh();
                }
            }catch(Exception e)
            {
                e.ToString();
            }
        }

        public static void RepaintInspector(System.Type t)
        {
            Editor[] ed = (Editor[])Resources.FindObjectsOfTypeAll<Editor>();
            for (int i = 0; i < ed.Length; i++)
            {
                if (ed[i].GetType() == t)
                {
                    ed[i].Repaint();
                    return;
                }
            }
        }

        public static void RepaintEditorWindow(Type t)
        {
            EditorWindow window = FindEditorWindow(t);
            if (window != null) window.Repaint();
        }

        public static EditorWindow FindEditorWindow(System.Type t)
        {
            EditorWindow[] ed = (EditorWindow[])Resources.FindObjectsOfTypeAll<EditorWindow>();
            for (int i = 0; i < ed.Length; i++)
            {
                if (ed[i].GetType() == t)
                {
                    return ed[i];
                }
            }
            return null;
        }

        //--------------------------Materials stuff----------------------------------

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
        }

        //----------------------------Textures------------------------------------

        public static Texture SaveTextureAsPNG(Texture2D texture, string path, TextureData settings)
        {
            if (!path.EndsWith(".png"))
                path += ".png";
            byte[] encoding = texture.EncodeToPNG();
            Debug.Log("Texture saved at \"" + path + "\".");
            Helper.writeBytesToFile(encoding, path);

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

        //-------------------Downloaders-----------------------------

        [InitializeOnLoad]
        public class MainThreader
        {
            private struct CallData
            {
                public Action<string> action;
                public object[] arguments;
            }
            static List<CallData> queue;

            static MainThreader()
            {
                queue = new List<CallData>();
                EditorApplication.update += Update;
            }

            public static void Call(Action<string> action, params object[] args)
            {
                if (action == null)
                    return;
                CallData data = new CallData();
                data.action = action;
                data.arguments = args;
                if (args.Length == 0 || args[0] == null)
                    data.arguments = new object[] { "" };
                else
                    data.arguments = args;
                queue.Add(data);
            }

            public static void Update()
            {
                if (queue.Count > 0)
                {
                    try
                    {
                        queue[0].action.DynamicInvoke(queue[0].arguments);
                    }
                    catch { }
                    queue.RemoveAt(0);
                }
            }
        }

        public static void DownloadFile(string url, string path)
        {
            DownloadAsFile(url, path);
        }

        public static void DownloadFileASync(string url, string path, Action<string> callback)
        {
            DownloadAsBytesASync(url, delegate (object o, DownloadDataCompletedEventArgs a)
            {
                if (a.Cancelled || a.Error != null)
                    MainThreader.Call(callback, null);
                else
                {
                    Helper.writeBytesToFile(a.Result, path);
                    MainThreader.Call(callback, path);
                }
            });
        }

        public static string DownloadString(string url)
        {
            return DownloadAsString(url);
        }

        public static void DownloadStringASync(string url, Action<string> callback)
        {
            DownloadAsStringASync(url, delegate (object o, DownloadStringCompletedEventArgs e)
            {
                if (e.Cancelled || e.Error != null)
                    MainThreader.Call(callback, null);
                else
                    MainThreader.Call(callback, e.Result);
            });
        }

        private static void SetCertificate()
        {
            ServicePointManager.ServerCertificateValidationCallback =
        delegate (object s, X509Certificate certificate,
                 X509Chain chain, SslPolicyErrors sslPolicyErrors)
        { return true; };
        }

        private static string DownloadAsString(string url)
        {
            SetCertificate();
            string contents = null;
            using (var wc = new System.Net.WebClient())
                contents = wc.DownloadString(url);
            return contents;
        }

        private static void DownloadAsStringASync(string url, Action<object, DownloadStringCompletedEventArgs> callback)
        {
            SetCertificate();
            using (var wc = new System.Net.WebClient())
            {
                wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(callback);
                wc.DownloadStringAsync(new Uri(url));
            }
        }

        private static void DownloadAsFileASync(string url, string path, Action<object, AsyncCompletedEventArgs> callback)
        {
            SetCertificate();
            using (var wc = new System.Net.WebClient())
            {
                wc.DownloadFileCompleted += new AsyncCompletedEventHandler(callback);
                wc.DownloadFileAsync(new Uri(url), path);
            }
        }

        private static void DownloadAsFile(string url, string path)
        {
            SetCertificate();
            using (var wc = new System.Net.WebClient())
                wc.DownloadFile(url, path);
        }

        private static byte[] DownloadAsBytes(string url)
        {
            SetCertificate();
            byte[] contents = null;
            using (var wc = new System.Net.WebClient())
                contents = wc.DownloadData(url);
            return contents;
        }

        private static void DownloadAsBytesASync(string url, Action<object, DownloadDataCompletedEventArgs> callback)
        {
            SetCertificate();
            using (var wc = new System.Net.WebClient())
            {
                wc.DownloadDataCompleted += new DownloadDataCompletedEventHandler(callback);
                wc.DownloadDataAsync(new Uri(url));
            }
        }

        //---------------------Color-----------------------

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

        //---------------Converter-----------------------

        public static Color stringToColor(string s)
        {
            string[] split = s.Split(",".ToCharArray());
            float[] rgba = new float[4] { 1, 1, 1, 1 };
            for (int i = 0; i < split.Length; i++) if (split[i].Replace(" ", "") != "") rgba[i] = float.Parse(split[i]);
            return new Color(rgba[0], rgba[1], rgba[2], rgba[3]);

        }

        public static Vector4 stringToVector(string s)
        {
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

        public class TashHandler
        {
            public static void EmptyThryTrash()
            {
                if (Directory.Exists(PATH.DELETING_DIR))
                {
                    DeleteDirectory(PATH.DELETING_DIR);
                }
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
    }

    public class ShaderHelper
    {
        public class ThryEditorShader
        {
            public string path;
            public string name;
            public string version;
        }

        private static List<ThryEditorShader> shaders;
        public static List<ThryEditorShader> thry_editor_shaders
        {
            get{
                if (shaders == null)
                    LoadThryEditorShaders();
                return shaders;
            }
        }

        public static string[] GetThryEditorShaderNames()
        {
            string[] r = new string[thry_editor_shaders.Count];
            for (int i = 0; i < r.Length; i++)
                r[i] = thry_editor_shaders[i].name;
            return r;
        }


        private static void LoadThryEditorShaders()
        {
            string data = Helper.ReadFileIntoString(PATH.THRY_EDITOR_SHADERS);
            if (data != "")
                shaders = Parser.ParseToObject<List<ThryEditorShader>>(data);
            else
                SearchAllShadersForThryEditorUsage();
            DeleteNull();
        }

        public static void SearchAllShadersForThryEditorUsage()
        {
            shaders = new List<ThryEditorShader>();
            string[] guids = AssetDatabase.FindAssets("t:shader");
            foreach(string g in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                TestShaderForThryEditor(path);
            }
            Save();
        }

        private static void DeleteNull()
        {
            bool save = false;
            int length = thry_editor_shaders.Count;
            for (int i = 0; i < length; i++)
            {
                if (thry_editor_shaders[i] == null)
                {
                    thry_editor_shaders.RemoveAt(i--);
                    length--;
                    save = true;
                }
            }
            if (save)
                Save();
        }

        private static void Save()
        {
            Helper.WriteStringToFile(Parser.ObjectToString(thry_editor_shaders), PATH.THRY_EDITOR_SHADERS);
        }

        private static string GetActiveCustomEditorParagraph(string code)
        {
            Match match = Regex.Match(code, @"(^|\*\/)((.|\n)(?!(\/\*)))*CustomEditor\s*\""(\w|\d)*\""((.|\n)(?!(\/\*)))*");
            if (match.Success) return match.Value;
            return null;
        }

        private static bool ParagraphContainsActiveThryEditorDefinition(string code)
        {
            Match match = Regex.Match(code, @"\n\s+CustomEditor\s+\""ThryEditor\""");
            return match.Success;
        }

        private static bool ShaderUsesThryEditor(string code)
        {
            string activeCustomEditorParagraph = GetActiveCustomEditorParagraph(code);
            if (activeCustomEditorParagraph == null)
                return false;
            return ParagraphContainsActiveThryEditorDefinition(activeCustomEditorParagraph);
        }

        private static bool TestShaderForThryEditor(string path)
        {
            string code = Helper.ReadFileIntoString(path);
            if (ShaderUsesThryEditor(code))
            {
                ThryEditorShader shader = new ThryEditorShader();
                shader.path = path;
                Match name_match = Regex.Match(code, @"(?<=[Ss]hader)\s*\""[^\""]+(?=\""\s*{)");
                if (name_match.Success) shader.name = name_match.Value.TrimStart(new char[] { ' ', '"' });
                Match master_label_match = Regex.Match(code, @"\[HideInInspector\]\s*shader_master_label\s*\(\s*\""[^\""]*(?=\"")");
                if (master_label_match.Success) shader.version = GetVersionFromMasterLabel(master_label_match.Value);
                thry_editor_shaders.Add(shader);
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
            foreach(string path in paths)
            {
                if (!path.EndsWith(".shader"))
                    continue;
                if (TestShaderForThryEditor(path))
                    save = true;
            }
            if(save)
                Save();
        }

        public static void AssetsDeleted(string[] paths)
        {
            bool save = false;
            foreach(string path in paths)
            {
                if (!path.EndsWith(".shader"))
                    continue;
                int length = thry_editor_shaders.Count;
                for (int i= 0;i< length; i++)
                {
                    if (thry_editor_shaders[i].path == path)
                    {
                        thry_editor_shaders.RemoveAt(i--);
                        length--;
                        save = true;
                    }
                }
            }
            if(save)
                Save();
        }

        public static void AssetsMoved(string[] old_paths, string[] paths)
        {
            bool save = false;
            for(int i=0;i<paths.Length;i++)
            {
                if (!paths[i].EndsWith(".shader"))
                    continue;
                foreach (ThryEditorShader s in thry_editor_shaders)
                {
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
}