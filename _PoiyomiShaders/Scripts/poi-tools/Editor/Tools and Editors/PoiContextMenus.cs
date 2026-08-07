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
        public const int AssetsMenuBase = 1200;

        // GameObject menu priorities - organized with separators (gap of 11+ creates separator)
        // Lock/Unlock: Base, Base+1
        // --- separator ---
        // Cross Shader Editor: Base+12
        // --- separator ---
        // Translate options: Base+23 to Base+26
        // --- separator ---
        // Update Poiyomi: Base+37
        // --- separator ---
        // Tools: Base+48, Base+49
        // High priority (1000) puts Poiyomi near bottom with separator above
        public const int ContextGameObjectBase = 1000;
        public const int ContextGameObjectCrossEditor = ContextGameObjectBase + 12;
        public const int ContextGameObjectTranslate = ContextGameObjectBase + 23;
        public const int ContextGameObjectUpdate = ContextGameObjectBase + 37;
        public const int ContextGameObjectTools = ContextGameObjectBase + 48;

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

        [MenuItem("GameObject/Poiyomi/Lock Materials", priority = ContextGameObjectBase)]
        static void LockMaterialsInGameObject()
        {
            int undoIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Lock materials");
            foreach (var obj in Selection.gameObjects)
                Undo.RegisterFullObjectHierarchyUndo(obj, "Lock materials");

            ShaderOptimizer.LockMaterials(GetMaterialsInChildren(Selection.gameObjects), ShaderOptimizer.ProgressBar.Cancellable);

            Undo.CollapseUndoOperations(undoIndex);
        }

        [MenuItem("GameObject/Poiyomi/Unlock Materials", priority = ContextGameObjectBase + 1)]
        static void UnlockMaterialsInGameObject()
        {
            int undoIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Unlock materials");
            foreach (var obj in Selection.gameObjects)
                Undo.RegisterFullObjectHierarchyUndo(obj, "Unlock materials");

            ShaderOptimizer.UnlockMaterials(GetMaterialsInChildren(Selection.gameObjects), ShaderOptimizer.ProgressBar.Cancellable);

            Undo.CollapseUndoOperations(undoIndex);
        }

        [MenuItem("GameObject/Poiyomi/Open in Cross Shader Editor", priority = ContextGameObjectCrossEditor)]
        static void OpenInCrossShaderEditor()
        {
            CrossEditor.GetInstance().UpdateTargets(GetMaterialsInChildren(Selection.gameObjects));
        }

        static IEnumerable<Material> GetMaterialsInChildren(params GameObject[] objects)
        {
            return objects.SelectMany(o => o.GetComponentsInChildren<Renderer>(true)).SelectMany(r => r.sharedMaterials).Distinct();
        }

        #endregion

        #region Context - GameObject - Tools

        [MenuItem("GameObject/Poiyomi/Duplicate with New Materials", false, priority = ContextGameObjectTools)]
        public static void DuplicateWithNewMaterialsMenu(MenuCommand command)
        {
            DuplicateWithUniqueMaterials.DuplicateWithNewMaterials(command.context as GameObject);
        }

        [MenuItem("GameObject/Poiyomi/Duplicate Only Translatable Materials", false, priority = ContextGameObjectTools + 1)]
        public static void DuplicateTranslatableMaterialsMenu(MenuCommand command)
        {
            DuplicateWithUniqueMaterialsOnlyTranslatable.DuplicateWithNewMaterialsOnlyTranslatable(command.context as GameObject);
        }

        [MenuItem("GameObject/Poiyomi/Move Materials to Folder", false, priority = ContextGameObjectTools + 2)]
        public static void MoveMaterialsToFolderMenu(MenuCommand command)
        {
            MoveAvatarMaterialsToFolder.MoveMaterialsToNewFolder(command.context as GameObject);
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