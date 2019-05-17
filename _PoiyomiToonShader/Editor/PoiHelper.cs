using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class PoiHelper
{

    public static Config config;

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
        pattern = @"Shader ?""(\w|\/|\.)+""";
        shaderCode = Regex.Replace(shaderCode, pattern, "Shader \"" + newShaderName + "\"");
        pattern = @"#include ""(?!.*(AutoLight)|(UnityCG)|(UnityShaderVariables)|(HLSLSupport)|(TerrainEngine))";
        shaderCode = Regex.Replace(shaderCode, pattern, "#include \"../", RegexOptions.Multiline);
        string[] pathParts = defaultPath.Split('/');
        string fileName = pathParts[pathParts.Length - 1];
        string newPath = defaultPath.Replace(fileName, "") + "_differentQueues";
        Directory.CreateDirectory(newPath);
        newPath = newPath + "/" + fileName.Replace(".shader", "-queue" + renderQueue + ".shader");
        writeStringToFile(shaderCode, newPath);
        if (import) AssetDatabase.ImportAsset(newPath);

        return Shader.Find(newShaderName);
    }

    public static string readFileIntoString(string path)
    {
        StreamReader reader = new StreamReader(path);
        string ret = reader.ReadToEnd();
        reader.Close();
        return ret;
    }

    public static void writeStringToFile(string s, string path)
    {
        StreamWriter writer = new StreamWriter(path, false);
        writer.Write(s);
        writer.Close();
    }

    //save mats
    public const string POI_MATERIALS_FILE_PATH = "./Assets/.poiMaterials.json";

    public static void saveAllPoiMaterials()
    {
        StreamWriter writer = new StreamWriter(POI_MATERIALS_FILE_PATH, false);

        string[] materialGuids = AssetDatabase.FindAssets("t:material");
        foreach (string mG in materialGuids)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(mG));
            if (material.HasProperty(Shader.PropertyToID("shader_eable_poi_settings_selection")))
            {
                writer.WriteLine(mG + ":" + PoiHelper.getDefaultShaderName(material.shader.name) + ":" + material.renderQueue);
            }
        }

        writer.Close();
    }

    public static void restorePoiMaterials()
    {
        StreamReader reader = new StreamReader(POI_MATERIALS_FILE_PATH);

        string l;
        while ((l = reader.ReadLine()) != null)
        {
            string[] materialData = l.Split(new string[] { ":" }, System.StringSplitOptions.None);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(materialData[0]));
            Shader shader = Shader.Find(materialData[1]);
            material.shader = shader;
            material.renderQueue = int.Parse(materialData[2]);
            PoiToon.UpdateRenderQueue(material, shader);
        }
        RepaintAllMaterialEditors();

        reader.Close();
    }

    //used to parse extra options in display name like offset
    public static int propertyOptionToInt(string optionName, MaterialProperty p)
    {
        string pattern = @"-" + optionName + "=\\d+";
        Match match = Regex.Match(p.displayName, pattern);
        if (match.Success)
        {
            int ret = 0;
            string value = match.Value.Replace("-" + optionName + "=", "");
            int.TryParse(value, out ret);
            return ret;
        }
        return 0;
    }

    public static void updateQueueShadersIfNessecary()
    {
        string[] guids = AssetDatabase.FindAssets("poiUpdatedShaders");
        if (guids.Length == 0) return;
        string file_path = AssetDatabase.GUIDToAssetPath(guids[0]);
        if (!file_path.Contains("/poiUpdatedShaders")) return;
        StreamReader reader = new StreamReader(file_path);
        string shader_file_name;
        List<Material> materialsToUpdate = new List<Material>();
        List<int> renderQueues = new List<int>();
        while ((shader_file_name = reader.ReadLine()) != null)
        {
            //for each updated shader
            string[] shaderGuids = AssetDatabase.FindAssets(shader_file_name);
            string defaultShaderGuid = null;
            foreach (string g in shaderGuids) if (AssetDatabase.GUIDToAssetPath(g).EndsWith(shader_file_name + ".shader")) defaultShaderGuid = g;
            if (defaultShaderGuid == null) continue;
            Shader defaultShader = AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath(defaultShaderGuid));
            string[] matGuids = AssetDatabase.FindAssets("t:material");
            materialsToUpdate.Clear();
            renderQueues.Clear();
            foreach (string mG in matGuids)
            {
                Material material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(mG));
                if (material.shader.name.Contains(defaultShader.name) && material.shader.name.Length > defaultShader.name.Length)
                {
                    materialsToUpdate.Add(material);
                    renderQueues.Add(material.renderQueue);
                    material.shader = defaultShader;
                }
            }
            for (int i = 0; i < shaderGuids.Length; i++) if (shaderGuids[i] != defaultShaderGuid) AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(shaderGuids[i]));
            for (int i = 0; i < materialsToUpdate.Count; i++)
            {
                materialsToUpdate[i].renderQueue = renderQueues[i];
                PoiToon.UpdateRenderQueue(materialsToUpdate[i], defaultShader);
            }
        }
        reader.Close();
        File.Delete(file_path);
        AssetDatabase.DeleteAsset(file_path);
    }

    public static string getDefaultShaderName(string shaderName)
    {
        return shaderName.Split(new string[] { "-queue" }, System.StringSplitOptions.None)[0].Replace(".differentQueues/", "");
    }

    //---------------config functions---------------

    //Config class
    public class Config
    {
        public bool bigTextures = false;
        public bool useRenderQueueSelection = true;

        public string SaveToString()
        {
            return JsonUtility.ToJson(this);
        }

        public static Config GetDefaultConfig()
        {
            Config config = new Config();
            config.bigTextures = false;
            config.useRenderQueueSelection = true;
            return config;
        }

        //save the config to the file
        public void save()
        {
            StreamWriter writer = new StreamWriter(PoiToon.CONFIG_FILE_PATH, false);
            writer.WriteLine(this.SaveToString());
            writer.Close();
        }
    }

    public static Config GetConfig()
    {
        if (config == null) config = LoadConfig();
        return config;
    }

    //load the config from file
    private static Config LoadConfig()
    {
        Config config = null;
        if (File.Exists(PoiToon.CONFIG_FILE_PATH))
        {
            StreamReader reader = new StreamReader(PoiToon.CONFIG_FILE_PATH);
            config = JsonUtility.FromJson<Config>(reader.ReadToEnd());
            reader.Close();
        }
        else
        {
            File.CreateText(PoiToon.CONFIG_FILE_PATH).Close();
            config = Config.GetDefaultConfig();
            config.save();
        }
        return config;
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
                ed[i].Repaint();
                return ed[i];
            }
        }
        return null;
    }

    public static void RepaintAllMaterialEditors()
    {
        MaterialEditor[] ed = (MaterialEditor[])Resources.FindObjectsOfTypeAll<MaterialEditor>();
        for (int i = 0; i < ed.Length; i++)
        {
            ed[i].Repaint();
        }
    }
}
