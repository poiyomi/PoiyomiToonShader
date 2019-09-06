using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
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
            if (!File.Exists(path)) File.Create(path).Close();
            StreamReader reader = new StreamReader(path);
            string ret = reader.ReadToEnd();
            reader.Close();
            return ret;
        }

        public static void WriteStringToFile(string s, string path)
        {
            Directory.CreateDirectory(Regex.Match(path, @".*\/").Value);
            if (!File.Exists(path)) File.Create(path).Close();
            StreamWriter writer = new StreamWriter(path, false);
            writer.Write(s);
            writer.Close();
        }

        public static bool writeBytesToFile(byte[] bytes, string path)
        {
            Directory.CreateDirectory(Regex.Match(path, @".*\/").Value);
            if (!File.Exists(path)) File.Create(path).Close();
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
                Debug.Log("Exception caught in process: "+ ex.ToString());
                return false;
            }
        }

        private static Dictionary<string, string> textFileData = new Dictionary<string, string>();

        public static string LoadValueFromFile(string key, string path)
        {
            if (!textFileData.ContainsKey(path)) textFileData[path] = ReadFileIntoString(path);
            Match m = Regex.Match(textFileData[path], Regex.Escape(key) + @"\s*:=.*\r?\n");
            string value = Regex.Replace(m.Value, key+@"\s*:=\s*", "");
            if (m.Success) return value;
            return null;
        }


        public static bool SaveValueToFileKeyIsRegex(string keyRegex, string value, string path)
        {
            if (!textFileData.ContainsKey(path)) textFileData[path] = ReadFileIntoString(path);
            Match m = Regex.Match(textFileData[path], keyRegex + @"\s*:=.*\r?\n");
            if (m.Success) textFileData[path] = textFileData[path].Replace(m.Value, m.Value.Substring(0,m.Value.IndexOf(":=")) +":="+ value+"\n");
            else textFileData[path] += Regex.Unescape(keyRegex) + ":=" + value + "\n";
            WriteStringToFile(textFileData[path], path);
            return true;
        }

        public static bool SaveValueToFile(string key, string value, string path)
        {
            return SaveValueToFileKeyIsRegex(Regex.Escape(key), value, path);
        }

        //used to parse extra options in display name like offset
        public static int propertyOptionToInt(string optionName, string displayName)
        {
            int ret = 0;
            string value = getPropertyOptionValue(optionName, displayName);
            int.TryParse(value, out ret);
            return ret;
        }

        public static string getPropertyOptionValue(string optionName, string displayName)
        {
            string pattern = @"" + ThryEditor.EXTRA_OPTION_PREFIX + optionName + ThryEditor.EXTRA_OPTION_INFIX + "[^-]+";
            Match match = Regex.Match(displayName, pattern);
            if (match.Success)
            {
                string value = match.Value.Replace(ThryEditor.EXTRA_OPTION_PREFIX + optionName + ThryEditor.EXTRA_OPTION_INFIX, "");
                return value;
            }
            return "";
        }

        public static string getDefaultShaderName(string shaderName)
        {
            return shaderName.Split(new string[] { "-queue" }, System.StringSplitOptions.None)[0].Replace(".differentQueues/", "");
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

        public static int compareVersions(string v1, string v2)
        {
            if (v1 == "" && v2 == "") return 0;
            else if (v1 == "") return 1;
            else if (v2 == "") return -1;
            string[] v1Parts = Regex.Split(v1,@"\.");
            string[] v2Parts = Regex.Split(v2, @"\.");
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

        public static valuetype GetValueFromDictionary<keytype,valuetype>(Dictionary<keytype, valuetype> dictionary, keytype key)
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

        public static string FixUrl(string url)
        {
            if (!url.StartsWith("http"))
                url = "http://" + url;
            return url;
        }

        public static string GetBetween(string value, string prefix, string postfix)
        {
            string pattern = @"(?<="+ prefix + ").+?(?="+ postfix + ")";
            Match m = Regex.Match(value, pattern);
            if (m.Success)
                return m.Value;
            return value;
        }

        //returns data for name:{data} even if data containss brakets
        public static string GetBracket(string data, string bracketName)
        {
            Match m = Regex.Match(data, bracketName + ":");
            if (m.Success)
            {
                int startIndex = m.Index+bracketName.Length+2;
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

        public static Texture SaveTextureAsPNG(Texture2D texture, string path)
        {
            byte[] encoding = texture.EncodeToPNG();
            Debug.Log("Gradient saved at \"" + path + "\".");
            Helper.writeBytesToFile(encoding, path);

            AssetDatabase.ImportAsset(path);
            Texture tex = (Texture)EditorGUIUtility.Load(path);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Point;
            return SetTextureImporterFormat((Texture2D)tex, true);
        }

        public static Texture2D SetTextureImporterFormat(Texture2D texture, bool isReadable)
        {
            if (null == texture) return texture;
            string assetPath = AssetDatabase.GetAssetPath(texture);
            var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (tImporter != null)
            {
                tImporter.isReadable = isReadable;
                tImporter.filterMode = FilterMode.Point;
                tImporter.alphaIsTransparency = true;
                tImporter.wrapMode = TextureWrapMode.Clamp;

                AssetDatabase.ImportAsset(assetPath);
                AssetDatabase.Refresh();

                return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            }
            return texture;
        }

        public static float ColorDifference(Color col1, Color col2)
        {
            return Math.Abs(col1.r - col2.r) + Math.Abs(col1.g - col2.g) + Math.Abs(col1.b - col2.b) + Math.Abs(col1.a - col2.a);
        }

        public static Vector4 stringToVector(string s)
        {
            string[] split = s.Split(",".ToCharArray());
            float[] xyzw = new float[4];
            for (int i = 0; i < 4; i++) if (i < split.Length && split[i].Replace(" ", "") != "") xyzw[i] = float.Parse(split[i]); else xyzw[i] = 0;
            return new Vector4(xyzw[0], xyzw[1], xyzw[2], xyzw[3]);
        }

        public static Color stringToColor(string s)
        {
            string[] split = s.Split(",".ToCharArray());
            float[] rgba = new float[4] { 1, 1, 1, 1 };
            for (int i = 0; i < split.Length; i++) if (split[i].Replace(" ", "") != "") rgba[i] = float.Parse(split[i]);
            return new Color(rgba[0], rgba[1], rgba[2], rgba[3]);

        }

        public static string GetCurrentVRCSDKVersion()
        {
            string currentVersion = "";
            string versionTextPath = Application.dataPath + "/VRCSDK/version.txt";
            if (File.Exists(versionTextPath))
            {
                string[] versionFileLines = File.ReadAllLines(versionTextPath);
                if (versionFileLines.Length > 0)
                    currentVersion = versionFileLines[0];
            }
            return currentVersion;
        }

        public static void downloadFileToPath(string url, string path)
        {
            GameObject go = new GameObject();
            TextDownloaderTwo downloader = (TextDownloaderTwo)go.AddComponent(typeof(TextDownloaderTwo));
            downloader.StartDownload(url, path, save_as_file_callback);
        }

        private static void save_as_file_callback(string s, string path)
        {
            WriteStringToFile(s, path);
            AssetDatabase.ImportAsset(path);
        }

        public static void getStringFromUrl(string url, Action<string> callback)
        {
            GameObject go = new GameObject();
            TextDownloader downloader = (TextDownloader)go.AddComponent(typeof(TextDownloader));
            downloader.StartDownload(url, callback);
        }

        public static string MaterialsToString(Material[] materials)
        {
            string s = "";
            foreach (Material m in materials)
                s += "\""+m.name+"\"" + ",";
            s = s.TrimEnd(',');
            return s;
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
            float scaleX = ((float)texture.width)/width;
            float scaleY = ((float)texture.height) / height;
            for (int x = 0; x < width; x++)
            {
                for(int y = 0; y < height; y++)
                {
                    ret.SetPixel(x, y, texture.GetPixel((int)(scaleX * x), (int)(scaleY * y)));
                }
            }
            ret.Apply();
            return ret;
        }

        private class TextDownloaderTwo : MonoBehaviour
        {
            string url;
            Action<string,string> callback;
            string passThrough;

            public void StartDownload(string url, string passThrough, Action<string,string> callback)
            {
                this.url = url;
                this.callback = callback;
                this.passThrough = passThrough;
                StartCoroutine(GetTextFromWWW());
            }

            private IEnumerator GetTextFromWWW()
            {
                WWW webpage = new WWW(url);
                while (!webpage.isDone) yield return false;
                string content = webpage.text;
                callback(content, passThrough);
                while (this != null)
                    DestroyImmediate(this.gameObject);
            }
        }

        private class TextDownloader : MonoBehaviour
        {
            string url;
            Action<string> callback;

            public void StartDownload(string url, Action<string> callback)
            {
                this.url = url;
                this.callback = callback;
                StartCoroutine(GetTextFromWWW());
            }

            private IEnumerator GetTextFromWWW()
            {
                WWW webpage = new WWW(url);
                while (!webpage.isDone) yield return false;
                string content = webpage.text;
                DestroyImmediate(this.gameObject);
                callback(content);
            }
        }

        public static Color Subtract(Color col1, Color col2)
        {
            return ColorMath(col1, col2, 1, -1);
        }

        public static Color ColorMath(Color col1, Color col2, float multiplier1, float multiplier2)
        {
            return new Color(col1.r * multiplier1 + col2.r * multiplier2, col1.g * multiplier1 + col2.g * multiplier2,col1.b * multiplier1 + col2.b * multiplier2);
        }
    }
}