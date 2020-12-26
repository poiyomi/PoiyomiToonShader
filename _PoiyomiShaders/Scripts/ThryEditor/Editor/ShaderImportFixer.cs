// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

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
    public class ShaderImportFixer
    {
        [MenuItem("Thry/Tools/Backup Materials")]
        static void BackupMats()
        {
            BackupAllMaterials();
        }

        [MenuItem("Thry/Tools/Restore All Materials")]
        static void RestoreMats()
        {
            RestoreAllMaterials(true);
        }

        private static bool restoring_in_progress = false;

        private static List<Material> GetBrokenMaterials()
        {
            List<Material> broken_materials = new List<Material>();
            string[] guids = AssetDatabase.FindAssets("t:material");
            foreach (string g in guids)
            {
                Material m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g));
                if (m.shader.name == "Hidden/InternalErrorShader")
                    broken_materials.Add(m);
            }
            return broken_materials;
        }

        public static List<string> scriptImportedAssetPaths = new List<string>();
        private static List<string> importedShaderPaths = new List<string>();

        public static void OnImport(string[] importedAssets)
        {
            importedShaderPaths.Clear();

            foreach (string str in importedAssets)
            {
                if (scriptImportedAssetPaths.Contains(str))
                {
                    scriptImportedAssetPaths.Remove(str);
                    continue;
                }
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(str);
                if (asset != null && asset.GetType() == typeof(Shader))
                {
                    Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(str);
                    importedShaderPaths.Add(str);
                    DeleteQueueShaders(shader, str);
                }
            }

            if (Config.Get().restore_materials)
            {
                if (importedShaderPaths.Count > 0)
                {
                    RestoreAllMaterials();
                    importedShaderPaths.Clear();
                }
            }
        }

        public static void OnCompile()
        {
            Selection.selectionChanged += OnSelectionChanged;
        }

        private static GameObject[] prev_gameobjects = new GameObject[] { };
        private static Material[] prev_selected_materials = new Material[] { };
        public static void OnSelectionChanged()
        {
            if (restoring_in_progress) return;

            for(int i=0;i< prev_gameobjects.Length;i++)
            {
                if (prev_gameobjects[i] == null)
                    continue;
                Renderer renderer = prev_gameobjects[i].GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material[] materials = renderer.sharedMaterials;
                    BackupMaterials(materials);
                }
            }
            BackupMaterials(prev_selected_materials);

            prev_gameobjects =  Selection.gameObjects;
            prev_selected_materials = Selection.GetFiltered<Material>(SelectionMode.Unfiltered);
        }

        private static void BackupMaterials(Material[] materials)
        {
            foreach(Material m in materials)
            {
                BackupMaterialWithTests(m);
            }
        }

        private static void BackupMaterialWithTests(Material m)
        {
            string guid = UnityHelper.GetGUID(m);
            string backedup_shader_name = FileHelper.LoadValueFromFile(guid, PATH.MATERIALS_BACKUP_FILE);
            if (m == null)
                return;
            if (MaterialShaderBroken(m))
                return;
            if (backedup_shader_name == m.shader.name)
                return;
            string default_shader_name = ShaderHelper.getDefaultShaderName(m.shader.name);
            if (default_shader_name.Equals(backedup_shader_name))
                return;
            FileHelper.SaveValueToFile(guid, default_shader_name, PATH.MATERIALS_BACKUP_FILE);
        }

        private static void DeleteQueueShaders(Shader defaultShader, string shaderPath)
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

        private static bool MaterialShaderBroken(Material m)
        {
            return m.shader == null || m.shader.name == "Hidden/InternalErrorShader";
        } 

        private static void BackupMaterial(Material m)
        {
            if (restoring_in_progress) return;
            if (!MaterialShaderBroken(m))
                FileHelper.SaveValueToFile(UnityHelper.GetGUID(m), ShaderHelper.getDefaultShaderName(m.shader.name),PATH.MATERIALS_BACKUP_FILE);
        }

        public static void BackupAllMaterials()
        {
            if (restoring_in_progress) return;
            EditorUtility.DisplayProgressBar("Backup materials", "", 0);

            Dictionary<string, string> materials_to_backup = new Dictionary<string, string>();
            string[] materialGuids = AssetDatabase.FindAssets("t:material");
            for (int mG = 0; mG < materialGuids.Length; mG++)
            {
                Material material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(materialGuids[mG]));
                if (!MaterialShaderBroken(material))
                {
                    materials_to_backup[materialGuids[mG]] = ShaderHelper.getDefaultShaderName(material.shader.name);
                }
                EditorUtility.DisplayProgressBar("Backup materials", material.name, (float)(mG + 1) / materialGuids.Length);
            }

            FileHelper.SaveDictionaryToFile(PATH.MATERIALS_BACKUP_FILE, materials_to_backup);
            EditorUtility.ClearProgressBar();
        }

        public static void RestoreAllMaterials(bool show_progressbar = false)
        {
            restoring_in_progress = true;
            if (!File.Exists(PATH.MATERIALS_BACKUP_FILE))
            {
                BackupAllMaterials();
                return;
            }
            if(show_progressbar)
                EditorUtility.DisplayProgressBar("Restoring materials", "", 0);
            Dictionary<string, string> materials_to_restore = FileHelper.LoadDictionaryFromFile(PATH.MATERIALS_BACKUP_FILE);
            int length = materials_to_restore.Count;
            int i = 0;
            foreach (KeyValuePair<string,string> keyvalue in materials_to_restore)
            {
                Material m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(keyvalue.Key));
                if (m == null)
                    continue;
                if (MaterialShaderBroken(m))
                {
                    Shader s = Shader.Find(keyvalue.Value);
                    if (s != null)
                        m.shader = s;
                    if(show_progressbar)
                        EditorUtility.DisplayProgressBar("Restoring materials", m.name, (float)(++i) / length);
                }
            }
            if(show_progressbar)
                EditorUtility.ClearProgressBar();
            restoring_in_progress = false;
        }
    }
}