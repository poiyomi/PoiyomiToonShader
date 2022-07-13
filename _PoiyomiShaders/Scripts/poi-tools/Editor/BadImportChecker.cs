using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Poi
{
    [InitializeOnLoad]
    public class BadImportChecker
    {
        static readonly string[] PackageStartsWithNames = new[]
        {
            "poiyomiToon",
            "poiyomi_free",
            "poiyomi_toon",
            "poi_toon",
            "poiToon",
            "poiyomi_pro",
            "poi_pro",
            "poiyomiPro"
        };
        
        const string warningDialogTitle = "Bad Import Warning";
        const string warningDialogMessage = "You already have Poiyomi Shaders in your project.\n\nTo update the shader you have to delete the old folder located at:\n{0}";
        const string warningDialogOk = "I Understand";
        
        static string PoiPath
        {
            get
            {
                if(!AssetDatabase.IsValidFolder(_poiPath))
                    _poiPath = FindPoiFolder();
                return _poiPath;
            }
        }
        static string _poiPath = DefaultPoiPath;
        
        const string DefaultPoiPath = "Assets/_PoiyomiShaders";
        const string DefaultPoiFolderGUID = "62039c2d546096c4185a32a9e0647fcd";
        
        static readonly Type importWindowType;
        static readonly MethodInfo hasOpenInstancesMethod;

        static BadImportChecker()
        {
            AssetDatabase.importPackageStarted -= AssetDatabaseOnimportPackageStarted;
            AssetDatabase.importPackageStarted += AssetDatabaseOnimportPackageStarted;

            importWindowType = Assembly.Load("UnityEditor.dll").GetType("UnityEditor.PackageImport");
            hasOpenInstancesMethod = typeof(EditorWindow).GetMethod(nameof(EditorWindow.HasOpenInstances), BindingFlags.Static | BindingFlags.Public)?.MakeGenericMethod(importWindowType); 
        }

        static void AssetDatabaseOnimportPackageStarted(string packagename)
        {
            if(!PackageStartsWithNames.Any(name => packagename.StartsWith(name, StringComparison.OrdinalIgnoreCase)) || !AssetDatabase.IsValidFolder(PoiPath))
                return;

            EditorUtility.DisplayDialog(warningDialogTitle, string.Format(warningDialogMessage, _poiPath), warningDialogOk);

            EditorApplication.update -= WaitForImportWindow;
            EditorApplication.update += WaitForImportWindow;
        }

        static void WaitForImportWindow()
        {
            if(!(bool)hasOpenInstancesMethod.Invoke(null, null))
                return;

            EditorApplication.update -= WaitForImportWindow;
            EditorWindow.GetWindow(importWindowType).Close();
        }
        
        static string FindPoiFolder()
        {
            if(AssetDatabase.IsValidFolder(DefaultPoiPath))
                return DefaultPoiPath;

            string path = AssetDatabase.GUIDToAssetPath(DefaultPoiFolderGUID);
            if(!string.IsNullOrWhiteSpace(path))
                return path;

            // Nuclear (and probably slow) option
            string[] dirs = Directory.GetDirectories(Application.dataPath, "_PoiyomiShaders", SearchOption.AllDirectories);
            return dirs.Length > 0 ? AbsolutePathToLocalAssetsPath(dirs[0]) : null;
        }
        
        static string AbsolutePathToLocalAssetsPath(string path)
        {
            if(path.StartsWith(Application.dataPath))
                path = "Assets" + path.Substring(Application.dataPath.Length);
            return path;
        }
    }
}