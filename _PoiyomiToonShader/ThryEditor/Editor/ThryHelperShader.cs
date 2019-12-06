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
            FileHelper.WriteStringToFile(shaderCode, newPath);
            ShaderImportFixer.scriptImportedAssetPaths.Add(newPath);
            if (import) AssetDatabase.ImportAsset(newPath);

            return Shader.Find(newerShaderName);
        }

        //------------Track ThryEditor shaders-------------------

        public class ThryEditorShader
        {
            public string path;
            public string name;
            public string version;
        }

        private static List<ThryEditorShader> shaders;
        public static List<ThryEditorShader> thry_editor_shaders
        {
            get
            {
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
            string data = FileHelper.ReadFileIntoString(PATH.THRY_EDITOR_SHADERS);
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
            FileHelper.WriteStringToFile(Parser.ObjectToString(thry_editor_shaders), PATH.THRY_EDITOR_SHADERS);
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
                        thry_editor_shaders.RemoveAt(i--);
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