using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Poi.Tools
{
    public class ThirdPartyIncluder : UnityEditor.Editor
    {
        private static List<ThirdPartyIncluderAsset> i_thirdPartyIncluders = new List<ThirdPartyIncluderAsset>();
        public static List<ThirdPartyIncluderAsset> thirdPartyIncluders
        {
            get
            {
                if (i_thirdPartyIncluders.Count == 0)
                {
                    i_thirdPartyIncluders = GetAllThirdPartyIncluders();
                }
                return i_thirdPartyIncluders;
            }
        }
        private static string i_projectPath;
        public static string projectPath
        {
            get
            {
                if (string.IsNullOrEmpty(i_projectPath))
                {
                    i_projectPath = Path.GetDirectoryName(Application.dataPath);
                }
                return i_projectPath;
            }
        }
        private static List<ThirdPartyIncluderAsset> GetAllThirdPartyIncluders()
        {
            return AssetDatabase.FindAssets($"t: {typeof(ThirdPartyIncluderAsset).Name}")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<ThirdPartyIncluderAsset>)
                    .ToList();
        }
        public static string GetPathFromGUIDIfNoSource(string sourcePath, string sourceGUID)
        {
            if (!File.Exists(sourcePath))
            {
                if (!string.IsNullOrEmpty(sourceGUID))
                {
                    return AssetDatabase.GUIDToAssetPath(sourceGUID);
                }
                else
                {
                    return string.Empty;
                }
            }
            return sourcePath;
        }
        public static bool CopyIfGood(string sourcePath, string sourceGUID, string destinationPath)
        {
            try
            {
                sourcePath = GetPathFromGUIDIfNoSource(sourcePath, sourceGUID);
                if (!File.Exists(sourcePath)) return false;
                if (!File.Exists(destinationPath)) return false;
                if (new System.IO.FileInfo(sourcePath).Length == new System.IO.FileInfo(destinationPath).Length) return false;
                File.Copy(sourcePath, destinationPath, true);
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{sourcePath}, {sourceGUID} or {destinationPath} are broken and threw exception: {e}");
            }
            return false;
        }
        public static bool WriteDefineIfExists(string sourcePath, string sourceGUID, string destinationPath, string defineName)
        {
            try
            {
                sourcePath = GetPathFromGUIDIfNoSource(sourcePath, sourceGUID);
                if (!File.Exists(sourcePath)) return false;
                if (!File.Exists(destinationPath)) return false;
                if (new System.IO.FileInfo(destinationPath).Length > 4) return false;
                File.WriteAllText(destinationPath, $"#define {defineName}");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"{sourcePath}, {sourceGUID} or {destinationPath} are broken and threw exception: {e}");
            }
            return false;
        }
        private static void DoAllThirdPartyIncluders()
        {
            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (var thirdPartyIncluder in thirdPartyIncluders)
                {
                    int i = 0;
                    foreach (var thirdPartyInclude in thirdPartyIncluder.ThirdPartyIncludes)
                    {
                        string sourcePath = Path.Combine(projectPath, thirdPartyInclude.sourcePath);
                        string destinationPath = Path.Combine(projectPath, thirdPartyInclude.destinationPath);
                        if (thirdPartyInclude.type == ThirdPartyIncluderAsset.ThirdPartyIncludeType.FileCopy)
                        {
                            if (CopyIfGood(sourcePath, thirdPartyInclude.sourceGUID, destinationPath))
                            {
                                i++;
                            }
                        }
                        else if (thirdPartyInclude.type == ThirdPartyIncluderAsset.ThirdPartyIncludeType.DefineIfExists)
                        {
                            if (WriteDefineIfExists(sourcePath, thirdPartyInclude.sourceGUID, destinationPath, thirdPartyInclude.defineName))
                            {
                                i++;
                            }
                        }
                    }
                    if (i > 0) Debug.Log($"[ThirdPartyIncluder] Found {thirdPartyIncluder.name} and did {i}/{thirdPartyIncluder.ThirdPartyIncludes.Length} actions", thirdPartyIncluder);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }
        }
        [InitializeOnLoadMethod]
        private static void OnProjectLoadedInEditor()
        {
            EditorApplication.delayCall += DoAllThirdPartyIncluders;
        }
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void DidReloadScripts()
        {
            EditorApplication.delayCall += DoAllThirdPartyIncluders;
        }
        [MenuItem("Poi/Tools/ThirdPartyIncluder/Refresh")]
        private static void mi_ThirdPartyIncluderRefresh()
        {
            DoAllThirdPartyIncluders();
        }
        [MenuItem("Poi/Tools/ThirdPartyIncluder/Clean Destination Assets")]
        public static void mi_ThirdPartyIncluderCleanDestAssets()
        {
            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (var thirdPartyIncluder in thirdPartyIncluders)
                {
                    foreach (var thirdPartyInclude in thirdPartyIncluder.ThirdPartyIncludes)
                    {
                        string destinationPath = Path.Combine(projectPath, thirdPartyInclude.destinationPath);
                        File.WriteAllText(destinationPath, "");
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }
        }
    }
    public class ThirdPartyIncluderAssetPostProcessor : AssetPostprocessor
    {
#if UNITY_2021_2_OR_NEWER
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
#else
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
#endif
        {
            try
            {
                AssetDatabase.StartAssetEditing();
                foreach (var thirdPartyIncluder in ThirdPartyIncluder.thirdPartyIncluders)
                {
                    int i = 0;
                    foreach (var thirdPartyInclude in thirdPartyIncluder.ThirdPartyIncludes)
                    {
                        foreach (string importedAsset in importedAssets)
                        {
                            if (thirdPartyInclude.sourcePath.Replace("\\", "/") == importedAsset)
                            {
                                string sourcePath = Path.Combine(ThirdPartyIncluder.projectPath, importedAsset);
                                string destinationPath = Path.Combine(ThirdPartyIncluder.projectPath, thirdPartyInclude.destinationPath);
                                if (thirdPartyInclude.type == ThirdPartyIncluderAsset.ThirdPartyIncludeType.FileCopy)
                                {
                                    if (ThirdPartyIncluder.CopyIfGood(sourcePath, thirdPartyInclude.sourceGUID, destinationPath))
                                    {
                                        i++;
                                    }
                                }
                                else if (thirdPartyInclude.type == ThirdPartyIncluderAsset.ThirdPartyIncludeType.DefineIfExists)
                                {
                                    if (ThirdPartyIncluder.WriteDefineIfExists(sourcePath, thirdPartyInclude.sourceGUID, destinationPath, thirdPartyInclude.defineName))
                                    {
                                        i++;
                                    }
                                }
                            }
                        }
                    }
                    if (i > 0) Debug.Log($"[ThirdPartyIncluder] Found {thirdPartyIncluder.name} and did {i}/{thirdPartyIncluder.ThirdPartyIncludes.Length} actions", thirdPartyIncluder);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }
        }
    }
}