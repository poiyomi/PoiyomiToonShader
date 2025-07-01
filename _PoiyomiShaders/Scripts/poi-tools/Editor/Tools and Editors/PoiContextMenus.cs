using System.Collections.Generic;
using System.Linq;
using Thry;
using UnityEditor;
using UnityEngine;
using Poi.Tools.Package;
using Thry.ThryEditor;

namespace Poi.Tools.Menus
{
    public class PoiContextMenus
    {
        public const int ContextMaterialBase = 2020;
        public const int ContextRendererBase = 2020;
        public const int AssetsMenuBase = 1200;//
        #if UNITY_2020_1_OR_NEWER
        public const int ContextGameObjectBase = 2020;
        public const int ContextGameObjectTools = 2120;
        public const int ContextGameObjectMaterial = 2030;
        #else // For some reason unity 2019 doesn't seem to like numbers too big here
        public const int ContextGameObjectBase = 24;
        public const int ContextGameObjectMaterial = 30;
        public const int ContextGameObjectTools = 40;
        #endif

        #region Assets

        [MenuItem("Assets/Poiyomi/Materials/Lock Materials", priority = AssetsMenuBase)]
        static void LockMaterialsInAssets()
        {
            var mats = _GetSelectedMaterials();
            ShaderOptimizer.LockMaterials(mats);
        }

        [MenuItem("Assets/Poiyomi/Materials/Unlock Materials", priority = AssetsMenuBase + 1)]
        static void UnlockMaterialsInAssets()
        {
            var mats = _GetSelectedMaterials();
            ShaderOptimizer.UnlockMaterials(mats);
        }

        // Font conversion tool
        [MenuItem("Assets/Poiyomi/Fonts/Convert Font", true, priority = AssetsMenuBase + 10)]
        public static bool ConvertFont_Validate()
        {
            return Selection.activeObject is Font;
        }

        [MenuItem("Assets/Poiyomi/Fonts/Convert Font", false)]
        public static async void ConvertFontContextMenu()
        {
            var package = await PoiPackageHandler.GetPackageInfoAsync(PoiExternalToolRegistry.ExternalPoiToolPackageName, true, true);
            if(package == null)
            {
                Debug.LogError("Package is not installed boss");
                return;
            }

            if(Selection.activeObject is Font font)
            {
                if(PoiExternalToolRegistry.TryGetTool(PoiExternalToolRegistry.PoiFontToolId, out IPoiExternalTool tool))
                    tool.Execute(font);
                else
                    Debug.LogError($"Tool {PoiExternalToolRegistry.PoiFontToolId} not found in project");
            }
        }

        #endregion

        #region Context - Renderer

        [MenuItem("CONTEXT/Renderer/Poiyomi/Lock Materials")]
        static void LockRendererMaterials(MenuCommand command)
        {
            var renderer = command.context as Renderer;
            int undoIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Lock materials in {renderer.name}");

            ShaderOptimizer.LockMaterials(renderer.sharedMaterials);

            Undo.CollapseUndoOperations(undoIndex);
        }

        [MenuItem("CONTEXT/Renderer/Poiyomi/Unlock Materials")]
        static void UnlockRendererMaterials(MenuCommand command)
        {
            var renderer = command.context as Renderer;
            int undoIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Lock materials in {renderer.name}");

            ShaderOptimizer.UnlockMaterials(renderer.sharedMaterials);

            Undo.CollapseUndoOperations(undoIndex);
        }

        #endregion

        #region Context - GameObject

        [MenuItem("GameObject/Poiyomi/Materials/Lock Materials", priority = ContextGameObjectMaterial)]
        static void LockMaterialsInGameObject(MenuCommand command)
        {
            GameObject obj = command.context as GameObject;

            int undoIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Lock materials in {obj.name}");
            Undo.RegisterFullObjectHierarchyUndo(obj, $"Lock materials in {obj.name}");

            ShaderOptimizer.LockMaterials(GetMaterialsInChildren(obj));

            Undo.CollapseUndoOperations(undoIndex);
        }

        [MenuItem("GameObject/Poiyomi/Materials/Unlock Materials", priority = ContextGameObjectMaterial + 1)]
        static void UnlockMaterialsInGameObject(MenuCommand command)
        {
            GameObject obj = command.context as GameObject;

            int undoIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Unlock materials in {obj.name}");
            Undo.RegisterFullObjectHierarchyUndo(obj, $"Unlock materials in {obj.name}");

            ShaderOptimizer.UnlockMaterials(GetMaterialsInChildren(obj));

            Undo.CollapseUndoOperations(undoIndex);
        }

        static IEnumerable<Material> GetMaterialsInChildren(params GameObject[] objects)
        {
            return objects.SelectMany(o => o.GetComponentsInChildren<Renderer>(true)).SelectMany(r => r.sharedMaterials).Distinct();
        }

        #endregion

        #region Context - GameObject - Tools

        [MenuItem("GameObject/Poiyomi/Tools/Duplicate with New Materials", false, priority = ContextGameObjectTools)]
        public static void NoWorkie1(MenuCommand command)
        {
            DuplicateWithUniqueMaterials.DuplicateWithNewMaterials(command.context as GameObject);
        }

        [MenuItem("GameObject/Poiyomi/Tools/Duplicate Only Translatable Materials", false, priority = ContextGameObjectTools + 1)]
        public static void NoWorkie2(MenuCommand command)
        {
            DuplicateWithUniqueMaterialsOnlyTranslatable.DuplicateWithNewMaterialsOnlyTranslatable(command.context as GameObject);
        }

        #endregion

        #region Helper Functions

        static List<Material> _GetSelectedMaterials()
        {
            var materialList = new List<Material>();
            foreach(var obj in Selection.objects)
            {
                if(obj == null)
                    continue;

                if(obj is Material)
                {
                    materialList.Add(obj as Material);
                }
                if(obj is DefaultAsset)
                {
                    string folderPath = AssetDatabase.GetAssetPath(obj);
                    if(!AssetDatabase.IsValidFolder(folderPath))
                        continue;

                    materialList.AddRange(AssetDatabase.FindAssets("t:Material", new string[] { folderPath })
                        .Select(guid => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid))));
                }
            }
            return materialList;
        }

        #endregion
    }
}