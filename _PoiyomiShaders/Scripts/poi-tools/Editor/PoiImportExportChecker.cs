using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Poi.Tools
{
    [InitializeOnLoad]
    public class PoiImportExportChecker
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
        static readonly Type exportWindowType;
        static readonly MethodInfo exportWindowHasOpenInstancesMethod;
        static readonly FieldInfo m_ExportPackageItemsField;
        static readonly Type exportPackageItemType;
        static readonly FieldInfo assetPathField;
        static bool removedPoi = false;

        static PoiImportExportChecker()
        {
            AssetDatabase.importPackageStarted -= AssetDatabaseOnimportPackageStarted;
            AssetDatabase.importPackageStarted += AssetDatabaseOnimportPackageStarted;

            importWindowType = Assembly.Load("UnityEditor.dll").GetType("UnityEditor.PackageImport");
            hasOpenInstancesMethod = typeof(EditorWindow).GetMethod(nameof(EditorWindow.HasOpenInstances), BindingFlags.Static | BindingFlags.Public)?.MakeGenericMethod(importWindowType); 

            exportWindowType = Assembly.Load("UnityEditor.dll").GetType("UnityEditor.PackageExport");
            exportWindowHasOpenInstancesMethod = typeof(EditorWindow).GetMethod(nameof(EditorWindow.HasOpenInstances), BindingFlags.Static | BindingFlags.Public)?.MakeGenericMethod(exportWindowType); 
            m_ExportPackageItemsField = exportWindowType.GetField("m_ExportPackageItems", BindingFlags.NonPublic | BindingFlags.Instance);
            exportPackageItemType = Assembly.Load("UnityEditor.dll").GetType("UnityEditor.ExportPackageItem");
            assetPathField = exportPackageItemType.GetField("assetPath", BindingFlags.Public | BindingFlags.Instance);

            EditorApplication.update -= WaitForExportWindow;
            EditorApplication.update += WaitForExportWindow;
        }
        static void WaitForExportWindow()
        {
            if(!(bool)exportWindowHasOpenInstancesMethod.Invoke(null, null))
            {
                if (removedPoi) removedPoi = false;
                return;
            }
            if (removedPoi) return;

            var m_ExportPackageItemsArray = m_ExportPackageItemsField.GetValue(EditorWindow.GetWindow(exportWindowType)) as object[];
            if (m_ExportPackageItemsArray != null)
            {
                removedPoi = true;
                var newList = new System.Collections.Generic.List<object>();
                for (int i = 0; i < m_ExportPackageItemsArray.Length; i++)
                {
                    var assetPath = assetPathField.GetValue(m_ExportPackageItemsArray[i]) as string;
                    if (!assetPath.Contains("_PoiyomiShaders")) newList.Add(m_ExportPackageItemsArray[i]);
                }
                if (newList.Count == m_ExportPackageItemsArray.Length) return;
                var newListArray = System.Array.CreateInstance(exportPackageItemType, newList.Count);
                newList.ToArray().CopyTo(newListArray, 0);
                m_ExportPackageItemsField.SetValue(EditorWindow.GetWindow(exportWindowType), newListArray);
            }
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
