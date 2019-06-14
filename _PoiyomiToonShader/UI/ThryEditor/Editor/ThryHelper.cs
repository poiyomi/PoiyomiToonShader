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

public class ThryHelper
{

    //copys og shader and changed render queue and name in there
    public static Shader createRenderQueueShaderIfNotExists(Shader defaultShader, int renderQueue, bool import)
    {
        string newShaderName = ".differentQueues/" + defaultShader.name + "-queue" + renderQueue;
        Shader renderQueueShader = Shader.Find(newShaderName);
        if (renderQueueShader != null) return renderQueueShader;

        string defaultPath = AssetDatabase.GetAssetPath(defaultShader);
        string shaderCode = readFileIntoString(defaultPath);
        string pattern = @"""Queue"" ?= ?""\w+(\+\d+)?""";
        string replacementQueue = "Background+" + (renderQueue - 1000);
        if (renderQueue == 1000) replacementQueue = "Background";
        else if (renderQueue < 1000) replacementQueue = "Background-" + (1000 - renderQueue);
        shaderCode = Regex.Replace(shaderCode, pattern, "\"Queue\" = \"" + replacementQueue + "\"");
        pattern = @"Shader *""(\w|\/|\.)+";
        string ogShaderName = Regex.Match(shaderCode, pattern).Value;
        ogShaderName = Regex.Replace(ogShaderName, @"Shader *""", "");
        string newerShaderName = ".differentQueues/" + ogShaderName + "-queue" + renderQueue;
        shaderCode = Regex.Replace(shaderCode, pattern, "Shader \""+newerShaderName);
        pattern = @"#include\s*""(?!(Lighting.cginc)|(AutoLight)|(UnityCG)|(UnityShaderVariables)|(HLSLSupport)|(TerrainEngine))";
        shaderCode = Regex.Replace(shaderCode, pattern, "#include \"../", RegexOptions.Multiline);
        string[] pathParts = defaultPath.Split('/');
        string fileName = pathParts[pathParts.Length - 1];
        string newPath = defaultPath.Replace(fileName, "") + "_differentQueues";
        Directory.CreateDirectory(newPath);
        newPath = newPath + "/" + fileName.Replace(".shader", "-queue" + renderQueue + ".shader");
        writeStringToFile(shaderCode, newPath);
        ThryShaderImportFixer.scriptImportedAssetPaths.Add(newPath);
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
            ThryShaderImportFixer.backupSingleMaterial(material);
        }
    }

    public static string readFileIntoString(string path)
    {
        if (!File.Exists(path)) File.Create(path).Close();
        StreamReader reader = new StreamReader(path);
        string ret = reader.ReadToEnd();
        reader.Close();
        return ret;
    }

    public static void writeStringToFile(string s, string path)
    {
        if (!File.Exists(path)) File.Create(path).Close();
        StreamWriter writer = new StreamWriter(path, false);
        writer.Write(s);
        writer.Close();
    }

    //used to parse extra options in display name like offset
    public static int propertyOptionToInt(string optionName, MaterialProperty p)
    {
        int ret = 0;
        string value = getPropertyOptionValue(optionName,p);
        int.TryParse(value, out ret);
        return ret;
    }

    public static string getPropertyOptionValue(string optionName, MaterialProperty p)
    {
        string pattern = @""+ ThryEditor.EXTRA_OPTION_PREFIX + optionName + ThryEditor.EXTRA_OPTION_INFIX+"[^-]+";
        Match match = Regex.Match(p.displayName, pattern);
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
        string[] v1Parts = v1.Split(new char[] { '.' });
        string[] v2Parts = v2.Split(new char[] { '.' });
        for(int i = 0; i < Math.Max(v1Parts.Length, v2Parts.Length); i++)
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

    public static Vector4 stringToVector(string s)
    {
        string[] split = s.Split(",".ToCharArray());
        float[] xyzw = new float[4];
        for (int i = 0; i < 4; i++) if (i < split.Length && split[i].Replace(" ","")!="") xyzw[i] = float.Parse(split[i]); else xyzw[i] = 0;
        return new Vector4(xyzw[0], xyzw[1], xyzw[2], xyzw[3]);
    }

    public static Color stringToColor(string s)
    {
        string[] split = s.Split(",".ToCharArray());
        float[] rgba = new float[4] { 1, 1, 1, 1 };
        for (int i = 0; i < split.Length; i++) if (split[i].Replace(" ", "") != "") rgba[i] = float.Parse(split[i]);
        return new Color(rgba[0], rgba[1], rgba[2], rgba[3]);

    }
}
