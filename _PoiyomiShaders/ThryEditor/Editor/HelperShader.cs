// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class ShaderHelper
    {

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
            string shaderCode = FileHelper.ReadFileIntoString(defaultPath);
            string pattern = @"""Queue"" ?= ?""\w+(\+\d+)?""";
            string replacementQueue = "Background+" + (renderQueue - 1000);
            if (renderQueue == 1000) replacementQueue = "Background";
            else if (renderQueue < 1000) replacementQueue = "Background-" + (1000 - renderQueue);
            shaderCode = Regex.Replace(shaderCode, pattern, "\"Queue\" = \"" + replacementQueue + "\"");
            pattern = @"Shader\s+""(\w|\s|\/|\.)+";
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
            FileHelper.WriteStringToFile(shaderCode, newPath);
            ShaderImportFixer.scriptImportedAssetPaths.Add(newPath);
            if (import) AssetDatabase.ImportAsset(newPath);

            return Shader.Find(newerShaderName);
        }

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
            return (code == "HideInInspector" || code == "NoScaleOffset" || code == "Normal" || code == "HDR" || code == "Gamma"|| code == "PerRendererData");
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
        //------------Track ThryEditor shaders-------------------

        public class ThryEditorShader
        {
            public string path;
            public string name;
            public string version;
        }

        private static List<ThryEditorShader> shaders;
        private static Dictionary<string, ThryEditorShader> dictionary;
        public static List<ThryEditorShader> thry_editor_shaders
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
                LoadThryEditorShaders();
        }

        private static void Add(ThryEditorShader s)
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

        public static string[] GetThryEditorShaderNames()
        {
            string[] r = new string[thry_editor_shaders.Count];
            for (int i = 0; i < r.Length; i++)
                r[i] = thry_editor_shaders[i].name;
            return r;
        }

        public static bool IsShaderUsingThryEditor(Shader shader)
        {
            Init();
            return dictionary.ContainsKey(shader.name);
        }


        private static void LoadThryEditorShaders()
        {
            string data = FileHelper.ReadFileIntoString(PATH.THRY_EDITOR_SHADERS);
            if (data != "")
            {
                shaders = Parser.ParseToObject<List<ThryEditorShader>>(data);
                InitDictionary();
            }
            else
            {
                dictionary = new Dictionary<string, ThryEditorShader>();
                SearchAllShadersForThryEditorUsage();
            }
            DeleteNull();
        }

        private static void InitDictionary()
        {
            dictionary = new Dictionary<string, ThryEditorShader>();
            foreach (ThryEditorShader s in shaders)
            {
                if(!dictionary.ContainsKey(s.name))
                    dictionary.Add(s.name, s);
            }
        }

        public static void SearchAllShadersForThryEditorUsage()
        {
            shaders = new List<ThryEditorShader>();
            string[] guids = AssetDatabase.FindAssets("t:shader");
            foreach (string g in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                TestShaderForThryEditor(path);
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
            string code = FileHelper.ReadFileIntoString(path);
            if (ShaderUsesThryEditor(code))
            {
                ThryEditorShader shader = new ThryEditorShader();
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
                if (TestShaderForThryEditor(path))
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
                    if (thry_editor_shaders[i].path == path)
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