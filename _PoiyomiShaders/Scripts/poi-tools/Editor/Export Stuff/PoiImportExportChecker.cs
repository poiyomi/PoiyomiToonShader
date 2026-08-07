using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Thry.ThryEditor.Helpers;
using System.Collections.Generic;

namespace Poi.Tools
{
    [InitializeOnLoad]
    public class PoiImportExportChecker
    {
        private static readonly string[] KnownPoiRoots = { "Packages/com.poiyomi.toon", "Assets/_PoiyomiShaders" };

        private const string DefaultPoiPath = "Assets/_PoiyomiShaders";
        private const string DefaultPoiFolderGUID = "62039c2d546096c4185a32a9e0647fcd";

        private const string WarningDialogTitle = "Bad Import Warning";
        private const string WarningDialogMessage = "You already have Poiyomi Shaders in your project! It is located at:\n{0}\n\n" + "You must delete the existing _PoiyomiShaders folder(s) " + "before importing a different version or using the VCC version.";
        private const string WarningDialogOk = "I Understand";
        
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

        private static string _poiPath = DefaultPoiPath;
        private static string PoiPath
        {
            get
            {
                if (!AssetDatabase.IsValidFolder(_poiPath))
                    _poiPath = FindPoiFolder();
                return _poiPath;
            }
        }

        private static string FindPoiFolder()
        {
            // 1) Canonical Location
            if (AssetDatabase.IsValidFolder(DefaultPoiPath)) return DefaultPoiPath;

            // 2) GUI Lookup
            string path = AssetDatabase.GUIDToAssetPath(DefaultPoiFolderGUID);
            if (!string.IsNullOrWhiteSpace(path)) return path;

            // 3) Nuclear (and probably slow) option - search entire Assets tree
            string[] dirs = Directory.GetDirectories(Application.dataPath, "_PoiyomiShaders", SearchOption.AllDirectories);

            return dirs.Length > 0 ? AbsolutePathToLocalAssetsPath(dirs[0]) : null;
        }

        private static string AbsolutePathToLocalAssetsPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;
            
            if (path.StartsWith(Application.dataPath)) path = "Assets" + path.Substring(Application.dataPath.Length);

            return path.Replace('\\', '/');
        }

        private static readonly Type PackageImportWindowType;
        private static readonly Type ImportPackageItemType;
        private static readonly FieldInfo ImportPackageItemsField;
        private static readonly FieldInfo ImportPackageItem_AssetPathField;
        private static readonly FieldInfo PackageImport_TreeField;
        private static readonly FieldInfo PackageImport_TreeViewStateField;

        private static EditorWindow _lastCheckedImportWindow;


        private static readonly Type importWindowType;
        private static readonly MethodInfo hasOpenInstancesMethod;
        private static readonly Type PackageExport_Type;
        private static readonly Type PackageExport_ExportPackageItemType;
        private static readonly FieldInfo PackageExport_AssetPathField;
        private static readonly FieldInfo PackageExport_ExportPackageItemsField;
        private static readonly MethodInfo PackageExport_ExportMethod;
        private static readonly MethodInfo CustomExport_Method;

        static PoiImportExportChecker()
        {
            var editorAsm = typeof(EditorWindow).Assembly;

            PackageImportWindowType = editorAsm.GetType("UnityEditor.PackageImport");
            ImportPackageItemType = editorAsm.GetType("UnityEditor.ImportPackageItem");

            if (PackageImportWindowType != null)
            {
                ImportPackageItemsField = PackageImportWindowType.GetField("m_ImportPackageItems", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                PackageImport_TreeField = PackageImportWindowType.GetField("m_Tree", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                PackageImport_TreeViewStateField = PackageImportWindowType.GetField("m_TreeViewState", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            if (ImportPackageItemType != null)
            {
                ImportPackageItem_AssetPathField = ImportPackageItemType.GetField("exportedAssetPath", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            EditorApplication.update -= CheckImportWindow;
            EditorApplication.update += CheckImportWindow;

            PackageExport_Type = editorAsm.GetType("UnityEditor.PackageExport");
            PackageExport_ExportPackageItemType = editorAsm.GetType("UnityEditor.ExportPackageItem");
            PackageExport_AssetPathField = PackageExport_ExportPackageItemType?.GetField("assetPath", BindingFlags.Public | BindingFlags.Instance);
            PackageExport_ExportPackageItemsField = PackageExport_Type?.GetField("m_ExportPackageItems", BindingFlags.NonPublic | BindingFlags.Instance);
            PackageExport_ExportMethod = PackageExport_Type?.GetMethod("Export", BindingFlags.NonPublic | BindingFlags.Instance);

            CustomExport_Method = typeof(PoiImportExportChecker).GetMethod(nameof(CustomExport), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            // DetourExportMethod();
        }

        private static void CheckImportWindow()
        {
            if (PackageImportWindowType == null || ImportPackageItemType == null || ImportPackageItemsField == null || ImportPackageItem_AssetPathField == null)
            {
                EditorApplication.update -= CheckImportWindow;
                return;
            }

            var importWindow = EditorWindow.focusedWindow;
            if (importWindow == null || !PackageImportWindowType.IsInstanceOfType(importWindow)) return;

            if (importWindow == _lastCheckedImportWindow) return;

            _lastCheckedImportWindow = importWindow;

            var rawItems = ImportPackageItemsField.GetValue(importWindow) as Array;
            if (rawItems == null || rawItems.Length == 0) return;

            var allProjectPaths = AssetDatabase.FindAssets("").Select(AssetDatabase.GUIDToAssetPath).Where(p => !string.IsNullOrEmpty(p)).SelectMany(WithDirectories).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(p => p).ToArray();

            var allPackagePaths = new List<string>();
            foreach (var item in rawItems)
            {
                var path = ImportPackageItem_AssetPathField.GetValue(item) as string;
                if (string.IsNullOrEmpty(path)) continue;

                allPackagePaths.AddRange(WithDirectories(path));
            }

            var allPackagePathsArray = allPackagePaths.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(p => p).ToArray();

            var poiProjectPaths = GetPoiyomiPaths(allProjectPaths);
            if (poiProjectPaths.Length == 0) return;

            var poiPackagePaths = GetPoiyomiPaths(allPackagePathsArray);
            if (poiPackagePaths.Length == 0) return;

            var packageIncludesPoiShaderFile = allPackagePathsArray.Any(packagePath => packagePath.EndsWith(".shader", StringComparison.OrdinalIgnoreCase) && poiPackagePaths.Any(p => packagePath.StartsWith(p + "/", StringComparison.OrdinalIgnoreCase)));

            if (!packageIncludesPoiShaderFile) return;

            var newItems = new List<object>();
            var removedPoiFile = false;

            foreach (var item in rawItems)
            {
                var path = ImportPackageItem_AssetPathField.GetValue(item) as string;
                if (string.IsNullOrEmpty(path))
                {
                    newItems.Add(item);
                    continue;
                }

                bool isPoiFile = poiPackagePaths.Any(p => string.Equals(path, p, StringComparison.OrdinalIgnoreCase) || path.StartsWith(p + "/", StringComparison.OrdinalIgnoreCase));

                if (isPoiFile)
                {
                    removedPoiFile = true;
                    continue;
                }

                newItems.Add(item);
            }

            if (!removedPoiFile) return;

            int removedCount = rawItems.Length - newItems.Count;

            if (newItems.Count == 0)
            {
                Debug.LogWarning($"[<color=#E75898ff>Poiyomi Import/Export Checker</color>] Blocked attempted double-import of Poiyomi Shaders because it is already installed in this project! Please make sure you delete your existing Poiyomi Shaders installation before importing a new version.\nCurrent Poiyomi Installation Path: {string.Join(", ", poiProjectPaths)}\n");
            }
            else
            {
                Debug.LogWarning($"[<color=#E75898ff>Poiyomi Import/Export Checker</color>] Automatically removed {removedCount} conflicting item(s) from the package importer because Poiyomi Shaders is already installed in this project! See Console for more details.\nIf you received this message while importing an Avatar's Unity Package, the original author attempted to include a copy of Poiyomi Shaders with it. This is bad practice and would have broken this project entirely if this safeguard didn't exist!\n\nItems Removed: {removedCount}\nCurrent Poiyomi Installation Path: {string.Join(", ", poiProjectPaths)}\n");
            }

            var arr = Array.CreateInstance(ImportPackageItemType, newItems.Count);
            newItems.ToArray().CopyTo(arr, 0);
            ImportPackageItemsField.SetValue(importWindow, arr);

            PackageImport_TreeViewStateField?.SetValue(importWindow, null);
            PackageImport_TreeField?.SetValue(importWindow, null);

            if (newItems.Count == 0)
            {
                importWindow.Close();

                EditorApplication.delayCall += () =>
                {
                    var poiLocations = poiProjectPaths.Length > 0 ? string.Join("\n", poiProjectPaths) : PoiPath ?? "(unknown location)";

                    EditorUtility.DisplayDialog(WarningDialogTitle,string.Format(WarningDialogMessage, poiLocations), WarningDialogOk);
                };
            }
        }

        private static IEnumerable<string> WithDirectories(string path)
        {
            if (string.IsNullOrEmpty(path)) yield break;

            path = path.Replace('\\', '/');

            while (!string.IsNullOrEmpty(path))
            {
                yield return path;

                var parent = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(parent) || string.Equals(parent, path, StringComparison.Ordinal)) break;

                path = parent.Replace('\\', '/');
            }
        }

        private static string[] GetPoiyomiPaths(IEnumerable<string> allPathsEnumerable)
        {
            if (allPathsEnumerable == null) return Array.Empty<string>();

            var allPaths = allPathsEnumerable.Where(p => !string.IsNullOrEmpty(p)).Select(p => p.Replace('\\', '/')).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var root in KnownPoiRoots)
            {
                if (allPaths.Any(p => string.Equals(p, root, StringComparison.OrdinalIgnoreCase))) result.Add(root);
            }

            foreach (var path in allPaths)
            {
                if (!path.EndsWith("poiToonPresets.txt", StringComparison.OrdinalIgnoreCase)) continue;

                var parent = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(parent)) continue;

                var leaf = Path.GetFileName(parent);
                if (!string.IsNullOrEmpty(leaf) && leaf.IndexOf("poiyomi", StringComparison.OrdinalIgnoreCase) >= 0) result.Add(parent.Replace('\\', '/'));
            }

            return result.ToArray();
        }

        public static void CustomExport()
        {
            if (PackageExport_Type == null || PackageExport_ExportPackageItemType == null || PackageExport_ExportPackageItemsField == null || PackageExport_AssetPathField == null || PackageExport_ExportMethod == null) return;

            var packageExportWindow = EditorWindow.GetWindow(PackageExport_Type);
            var exportItems = PackageExport_ExportPackageItemsField.GetValue(packageExportWindow) as object[];
            if (exportItems == null) return;

            var newList = new List<object>();

            for (int i = 0; i < exportItems.Length; i++)
            {
                var assetPath = PackageExport_AssetPathField.GetValue(exportItems[i]) as string;
                if (string.IsNullOrEmpty(assetPath))
                {
                    newList.Add(exportItems[i]);
                    continue;
                }

                if (assetPath.Contains("_PoiyomiShaders")) continue;

                var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                if (obj is Shader shader && ShaderHelper.IsShaderUsingThryEditor(shader))
                {
                    if (shader.name.ToLowerInvariant().Contains("poiyomi pro")) continue;
                }

                newList.Add(exportItems[i]);
            }

            if (newList.Count == exportItems.Length) return;

            var newArray = Array.CreateInstance(PackageExport_ExportPackageItemType, newList.Count);
            newList.ToArray().CopyTo(newArray, 0);
            PackageExport_ExportPackageItemsField.SetValue(packageExportWindow, newArray);

            Helper.RestoreDetour(PackageExport_ExportMethod);
            EditorApplication.delayCall += DetourExportMethod;

            PackageExport_ExportMethod.Invoke(packageExportWindow, null);
        }

        private static void DetourExportMethod()
        {
            if (PackageExport_ExportMethod == null || CustomExport_Method == null) return;
            
            Helper.TryDetourFromTo(PackageExport_ExportMethod, CustomExport_Method);
        }
    }
}
