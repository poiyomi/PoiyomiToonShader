using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using Object = UnityEngine.Object;

namespace Thry
{
    public class ShaderImportFixer : AssetPostprocessor
    {
        [MenuItem("Thry/Tools/Backup Materials")]
        static void Init()
        {
            backupAllMaterials();
        }

        private static bool ignore = false;

        private static bool restoring_in_progress = false;

        private class ShaderImportFixerGui : EditorWindow
        {
            [MenuItem("Thry/Tools/Restore All Materials")]
            static void Init()
            {
                fixAllMaterials();
            }

            void OnGUI()
            {
                if (GUILayout.Button("Fix queue shaders and materials"))
                {
                    fixMaterials();
                }
                if (GUILayout.Button("I'm working on shaders today, don't bother me!"))
                {
                    ignore = true;
                    this.Close();
                }
                GUILayout.Label("Imported shaders:", EditorStyles.boldLabel);
                foreach (string s in importedShaderPaths) GUILayout.Label(s);
            }

        }

        /*private static List<string> allShaderPaths = new List<string>();

        [InitializeOnLoad]
        public class Startup
        {
            static Startup()
            {
                loadAllShaderPaths();
            }
        }

        private static void loadAllShaderPaths()
        {
            allShaderPaths.Clear();
            foreach (string g in AssetDatabase.FindAssets("t:shader")) allShaderPaths.Add(AssetDatabase.GUIDToAssetPath(g));
        }*/

        public static void fixMaterials()
        {
            if (!File.Exists(MATERIALS_BACKUP_FILE_PATH))
            {
                backupAllMaterials();
                return;
            }

            List<string> importedShaderNames = new List<string>();
            List<Shader> importedShaders = new List<Shader>();
            foreach (string path in importedShaderPaths)
            {
                Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                importedShaders.Add(shader);
                importedShaderNames.Add(shader.name);
            }
            fixMaterials(importedShaders, importedShaderNames);
        }

        public static void fixAllMaterials()
        {
            if (!File.Exists(MATERIALS_BACKUP_FILE_PATH))
            {
                backupAllMaterials();
                return;
            }
            string[] guids = AssetDatabase.FindAssets("t:shader");
            List<string> importedShaderNames = new List<string>();
            List<Shader> importedShaders = new List<Shader>();
            for (int g = 0; g < guids.Length; g++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[g]);
                Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                EditorUtility.DisplayProgressBar("Restoring materials...", shader.name, (float)(g + 1) / guids.Length);
                importedShaders.Add(shader);
                importedShaderNames.Add(shader.name);
            }
            EditorUtility.ClearProgressBar();
            fixMaterials(importedShaders, importedShaderNames);
        }

        private static void fixMaterials(List<Shader> shaders, List<string> importedShaderNames)
        {
            restoring_in_progress = true;

            StreamReader reader = new StreamReader(MATERIALS_BACKUP_FILE_PATH);

            string l;
            while ((l = reader.ReadLine()) != null)
            {
                if (l == "") continue;
                string[] materialData = l.Split(new string[] { ":" }, System.StringSplitOptions.None);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(materialData[0]));
                if (importedShaderNames.Contains(materialData[1]))
                {
                    //Debug.Log("Restore this shader: " + materialData[1]);
                    Shader shader = shaders[importedShaderNames.IndexOf(materialData[1])];
                    //Debug.Log("Shader: " + shader.name);
                    material.shader = shader;
                    material.renderQueue = int.Parse(materialData[2]);
                    Helper.UpdateRenderQueue(material, shader);
                }
            }
            reader.Close();

            restoring_in_progress = false;
            ThryEditor.repaint();
        }

        public static List<string> scriptImportedAssetPaths = new List<string>();
        private static List<string> importedShaderPaths = new List<string>();

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            importedShaderPaths.Clear();

            foreach (string str in importedAssets)
            {
                if (scriptImportedAssetPaths.Contains(str))
                {
                    scriptImportedAssetPaths.Remove(str);
                    continue;
                }
                //if (allShaderPaths.Contains(str)) continue;
                //else allShaderPaths.Add(str);
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(str);
                if (asset != null && asset.GetType() == typeof(Shader))
                {
                    Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(str);
                    importedShaderPaths.Add(str);
                    deleteQueueShaders(shader, str);
                }
                else if (asset != null && str.ToLower().Contains("ThryEditor")) ThryEditor.reload();
            }
            if (importedShaderPaths.Count == 0) return;

            if (ignore) return;
            if (!Config.Get().showImportPopup) return;
            EditorWindow window = Helper.FindEditorWindow(typeof(ShaderImportFixerGui));
            if (window == null) window = EditorWindow.CreateInstance<ShaderImportFixerGui>();
            window.Show();
        }

        private static void deleteQueueShaders(Shader defaultShader, string shaderPath)
        {
            string[] pathData = shaderPath.Split(new string[] { "/" }, System.StringSplitOptions.None);
            string defaultShaderFileName = pathData[pathData.Length - 1].Replace(".shader", "");

            string[] queueShaderGuids = AssetDatabase.FindAssets(defaultShaderFileName + "-queue t:shader");

            for (int i = 0; i < queueShaderGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(queueShaderGuids[i]);
                if (path.Contains(defaultShader.name) && path.Contains("-queue")) AssetDatabase.DeleteAsset(path);
            }
        }

        private static string readShaderName(Shader shader)
        {
            string defaultPath = AssetDatabase.GetAssetPath(shader);
            return readShaderName(defaultPath);
        }

        private static string readShaderName(string path)
        {
            string shaderCode = Helper.ReadFileIntoString(path);
            string pattern = @"Shader *""(\w|\/|\.)+";
            string ogShaderName = Regex.Match(shaderCode, pattern).Value;
            ogShaderName = Regex.Replace(ogShaderName, @"Shader *""", "");
            return ogShaderName;
        }

        //save mats
        public const string MATERIALS_BACKUP_FILE_PATH = "./Assets/.materialsBackup.txt";

        public static void backupAllMaterials()
        {
            if (restoring_in_progress) return;
            if (!File.Exists(MATERIALS_BACKUP_FILE_PATH)) File.CreateText(MATERIALS_BACKUP_FILE_PATH).Close();
            EditorUtility.DisplayProgressBar("Backup materials", "", 0);
            StreamWriter writer = new StreamWriter(MATERIALS_BACKUP_FILE_PATH, false);

            string[] materialGuids = AssetDatabase.FindAssets("t:material");
            for (int mG = 0; mG < materialGuids.Length; mG++)
            {
                Material material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(materialGuids[mG]));
                writer.WriteLine(materialGuids[mG] + ":" + Helper.getDefaultShaderName(material.shader.name) + ":" + material.renderQueue);
                EditorUtility.DisplayProgressBar("Backup materials", "", (float)(mG + 1) / materialGuids.Length);
            }

            writer.Close();
            EditorUtility.ClearProgressBar();
        }

        public static void backupSingleMaterial(Material m)
        {
            if (restoring_in_progress) return;
            string[] mats = new string[0];
            if (!File.Exists(MATERIALS_BACKUP_FILE_PATH)) File.CreateText(MATERIALS_BACKUP_FILE_PATH).Close();
            else mats = Helper.ReadFileIntoString(MATERIALS_BACKUP_FILE_PATH).Split(new string[] { "\n" }, System.StringSplitOptions.None);
            bool updated = false;
            string matGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m.GetInstanceID()));
            string newString = "";
            for (int mat = 0; mat < mats.Length; mat++)
            {
                if (mats[mat].Contains(matGuid))
                {
                    updated = true;
                    newString += matGuid + ":" + Helper.getDefaultShaderName(m.shader.name) + ":" + m.renderQueue + "\r\n";
                }
                else
                {
                    newString += mats[mat] + "\n";
                }

            }
            if (!updated) newString += matGuid + ":" + Helper.getDefaultShaderName(m.shader.name) + ":" + m.renderQueue;
            else newString = newString.Substring(0, newString.LastIndexOf("\n"));
            Helper.WriteStringToFile(newString, MATERIALS_BACKUP_FILE_PATH);
        }

        public static void restoreAllMaterials()
        {
            if (!File.Exists(MATERIALS_BACKUP_FILE_PATH))
            {
                backupAllMaterials();
                return;
            }
            StreamReader reader = new StreamReader(MATERIALS_BACKUP_FILE_PATH);

            string l;
            while ((l = reader.ReadLine()) != null)
            {
                string[] materialData = l.Split(new string[] { ":" }, System.StringSplitOptions.None);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(materialData[0]));
                Shader shader = Shader.Find(materialData[1]);
                material.shader = shader;
                material.renderQueue = int.Parse(materialData[2]);
                Helper.UpdateRenderQueue(material, shader);
            }
            ThryEditor.repaint();

            reader.Close();
        }
    }
}