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
                if (!AssetDatabase.IsValidFolder(_poiPath))
                    _poiPath = FindPoiFolder();
                return _poiPath;
            }
        }

        static string _poiPath = DefaultPoiPath;

        const string DefaultPoiPath = "Assets/_PoiyomiShaders";
        const string DefaultPoiFolderGUID = "62039c2d546096c4185a32a9e0647fcd";

        static readonly Type importWindowType;
        static readonly MethodInfo hasOpenInstancesMethod;
        static readonly Type PackageExport_Type;
        static readonly Type PackageExport_ExportPackageItemType;
        static readonly FieldInfo PackageExport_AssetPathField;
        static readonly FieldInfo PackageExport_ExportPackageItemsField;
        static readonly MethodInfo PackageExport_ExportMethod;
        static readonly MethodInfo CustomExport_Method;

        static PoiImportExportChecker()
        {
            AssetDatabase.importPackageStarted -= AssetDatabaseOnimportPackageStarted;
            AssetDatabase.importPackageStarted += AssetDatabaseOnimportPackageStarted;

            importWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.PackageImport");
            hasOpenInstancesMethod = typeof(EditorWindow).GetMethod(nameof(EditorWindow.HasOpenInstances), BindingFlags.Static | BindingFlags.Public)?.MakeGenericMethod(importWindowType);

            PackageExport_Type = typeof(EditorWindow).Assembly.GetType("UnityEditor.PackageExport");
            PackageExport_ExportPackageItemType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ExportPackageItem");
            PackageExport_AssetPathField = PackageExport_ExportPackageItemType.GetField("assetPath", BindingFlags.Public | BindingFlags.Instance);
            PackageExport_ExportPackageItemsField = PackageExport_Type.GetField("m_ExportPackageItems", BindingFlags.NonPublic | BindingFlags.Instance);
            PackageExport_ExportMethod = PackageExport_Type.GetMethod("Export", BindingFlags.NonPublic | BindingFlags.Instance);

            CustomExport_Method = typeof(PoiImportExportChecker).GetMethod(nameof(CustomExport));
            //DetourExportMethod();
        }
        public static void CustomExport()
        {
            var PackageExport_Window = EditorWindow.GetWindow(PackageExport_Type);
            var m_ExportPackageItemsArray = PackageExport_ExportPackageItemsField.GetValue(PackageExport_Window) as object[];
            if (m_ExportPackageItemsArray != null)
            {
                var newList = new System.Collections.Generic.List<object>();
                for (int i = 0; i < m_ExportPackageItemsArray.Length; i++)
                {
                    var assetPath = PackageExport_AssetPathField.GetValue(m_ExportPackageItemsArray[i]) as string;
                    if (assetPath.Contains("_PoiyomiShaders")) continue;
                    var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                    if (obj != null)
                    {
                        if (obj.GetType() == typeof(Shader))
                        {
                            var shader = obj as Shader;
                            int index = shader.FindPropertyIndex(Thry.ShaderEditor.PROPERTY_NAME_EDITOR_DETECT);
                            if (index != -1)
                            {
                                if (shader.name.ToLowerInvariant().Contains("poiyomi pro"))
                                {
                                    continue;
                                }
                            }
                        }
                    }
                    newList.Add(m_ExportPackageItemsArray[i]);
                }
                if (newList.Count == m_ExportPackageItemsArray.Length) return;
                var newListArray = System.Array.CreateInstance(PackageExport_ExportPackageItemType, newList.Count);
                newList.ToArray().CopyTo(newListArray, 0);
                PackageExport_ExportPackageItemsField.SetValue(PackageExport_Window, newListArray);
            }
            Thry.Helper.RestoreDetour(PackageExport_ExportMethod);
            EditorApplication.delayCall += DetourExportMethod;
            // Delay needed because the Invoke below likely exits somewhere
            // causing this method to stop calling (I think?!)
            PackageExport_ExportMethod.Invoke(PackageExport_Window, null);
        }

        private static void DetourExportMethod()
        {
            Thry.Helper.TryDetourFromTo(PackageExport_ExportMethod, CustomExport_Method);
        }

        static void AssetDatabaseOnimportPackageStarted(string packagename)
        {
            if (!PackageStartsWithNames.Any(name => packagename.StartsWith(name, StringComparison.OrdinalIgnoreCase)) || !AssetDatabase.IsValidFolder(PoiPath))
                return;

            EditorUtility.DisplayDialog(warningDialogTitle, string.Format(warningDialogMessage, _poiPath), warningDialogOk);

            EditorApplication.update -= WaitForImportWindow;
            EditorApplication.update += WaitForImportWindow;
        }

        static void WaitForImportWindow()
        {
            if (!(bool)hasOpenInstancesMethod.Invoke(null, null))
                return;

            EditorApplication.update -= WaitForImportWindow;
            EditorWindow.GetWindow(importWindowType).Close();
        }

        static string FindPoiFolder()
        {
            if (AssetDatabase.IsValidFolder(DefaultPoiPath))
                return DefaultPoiPath;

            string path = AssetDatabase.GUIDToAssetPath(DefaultPoiFolderGUID);
            if (!string.IsNullOrWhiteSpace(path))
                return path;

            // Nuclear (and probably slow) option
            string[] dirs = Directory.GetDirectories(Application.dataPath, "_PoiyomiShaders", SearchOption.AllDirectories);
            return dirs.Length > 0 ? AbsolutePathToLocalAssetsPath(dirs[0]) : null;
        }

        static string AbsolutePathToLocalAssetsPath(string path)
        {
            if (path.StartsWith(Application.dataPath))
                path = "Assets" + path.Substring(Application.dataPath.Length);
            return path;
        }
    }
}
