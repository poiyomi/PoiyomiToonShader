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
        // High priority (1000) puts Poiyomi near bottom with separator above
        public const int ContextGameObjectBase = 1000;

        // One layout shared by every Poiyomi context menu, so the same actions appear in the same order
        // whichever way the menu is opened - hierarchy, renderer, material inspector or project view.
        // Unity draws a separator when consecutive priorities differ by 11 or more, hence the gaps.
        //
        // Lock/Unlock:          Base, Base+1
        // --- separator ---
        // Cross Shader Editor:  Base+12
        // --- separator ---
        // Translate:            Base+23 .. Base+26
        // --- separator ---
        // Update Poiyomi:       Base+37
        // --- separator ---
        // Tools:                Base+48 .. Base+50
        //
        // Add offsets to whichever root base applies. Not every action suits every root - the Tools
        // entries act on a GameObject, so they are absent from the material and project menus.
        public const int LockOffset = 0;
        public const int UnlockOffset = 1;
        public const int CrossEditorOffset = 12;
        public const int TranslateOffset = 23;
        public const int UpdateOffset = 37;
        public const int ToolsOffset = 48;

        public const int ContextGameObjectCrossEditor = ContextGameObjectBase + CrossEditorOffset;
        public const int ContextGameObjectTranslate = ContextGameObjectBase + TranslateOffset;
        public const int ContextGameObjectUpdate = ContextGameObjectBase + UpdateOffset;
        public const int ContextGameObjectTools = ContextGameObjectBase + ToolsOffset;

        #region Shared Actions

        // Every menu entry below is a thin wrapper: work out the materials from whatever was clicked, then
        // hand them to one of these. Keeping the work here is what lets the four menus stay identical.

        static void _SetLocked(IEnumerable<Material> materials, bool locked, string undoName, ShaderOptimizer.ProgressBar progressBar = ShaderOptimizer.ProgressBar.None)
        {
            int undoIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(undoName);

            if (locked) ShaderOptimizer.LockMaterials(materials, progressBar);
            else ShaderOptimizer.UnlockMaterials(materials, progressBar);

            Undo.CollapseUndoOperations(undoIndex);
        }

        static void _OpenInCrossShaderEditor(IEnumerable<Material> materials)
        {
            CrossEditor.GetInstance().UpdateTargets(materials);
        }

        #endregion

        #region Assets

        [MenuItem("Assets/Poiyomi/Materials/Lock Materials", priority = AssetsMenuBase + LockOffset)]
        static void LockMaterialsInAssets()
        {
            _SetLocked(_GetSelectedMaterials(), true, "Lock materials");
        }

        [MenuItem("Assets/Poiyomi/Materials/Unlock Materials", priority = AssetsMenuBase + UnlockOffset)]
        static void UnlockMaterialsInAssets()
        {
            _SetLocked(_GetSelectedMaterials(), false, "Unlock materials");
        }

        [MenuItem("Assets/Poiyomi/Materials/Open in Cross Shader Editor", priority = AssetsMenuBase + CrossEditorOffset)]
        static void OpenSelectedAssetsInCrossShaderEditor()
        {
            _OpenInCrossShaderEditor(_GetSelectedMaterials());
        }

        // Font conversion tool.
        // Both entries need the priority. A submenu takes its position from the lowest priority among its
        // children, and an item without one defaults to 1000 - which used to drag the whole Poiyomi submenu
        // up above lilToon in the project view instead of leaving it near the bottom.
        [MenuItem("Assets/Poiyomi/Fonts/Convert Font", true, priority = AssetsMenuBase + ToolsOffset)]
        public static bool ConvertFont_Validate()
        {
            return Selection.activeObject is Font;
        }

        [MenuItem("Assets/Poiyomi/Fonts/Convert Font", false, priority = AssetsMenuBase + ToolsOffset)]
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

        #region Context - Material

        [MenuItem("CONTEXT/Material/Poiyomi/Lock Materials", false, ContextMaterialBase + LockOffset)]
        static void LockMaterialContext(MenuCommand command)
        {
            Material material = command.context as Material;
            _SetLocked(new[] { material }, true, $"Lock material {material.name}");
        }

        [MenuItem("CONTEXT/Material/Poiyomi/Unlock Materials", false, ContextMaterialBase + UnlockOffset)]
        static void UnlockMaterialContext(MenuCommand command)
        {
            Material material = command.context as Material;
            _SetLocked(new[] { material }, false, $"Unlock material {material.name}");
        }

        [MenuItem("CONTEXT/Material/Poiyomi/Open in Cross Shader Editor", false, ContextMaterialBase + CrossEditorOffset)]
        static void OpenMaterialInCrossShaderEditor(MenuCommand command)
        {
            _OpenInCrossShaderEditor(new[] { command.context as Material });
        }

        #endregion

        #region Context - Renderer

        [MenuItem("CONTEXT/Renderer/Poiyomi/Lock Materials", false, ContextRendererBase + LockOffset)]
        static void LockRendererMaterials(MenuCommand command)
        {
            var renderer = command.context as Renderer;
            _SetLocked(renderer.sharedMaterials, true, $"Lock materials in {renderer.name}");
        }

        [MenuItem("CONTEXT/Renderer/Poiyomi/Unlock Materials", false, ContextRendererBase + UnlockOffset)]
        static void UnlockRendererMaterials(MenuCommand command)
        {
            var renderer = command.context as Renderer;
            _SetLocked(renderer.sharedMaterials, false, $"Unlock materials in {renderer.name}");
        }

        [MenuItem("CONTEXT/Renderer/Poiyomi/Open in Cross Shader Editor", false, ContextRendererBase + CrossEditorOffset)]
        static void OpenRendererInCrossShaderEditor(MenuCommand command)
        {
            _OpenInCrossShaderEditor((command.context as Renderer).sharedMaterials.Where(m => m != null).Distinct());
        }

        [MenuItem("CONTEXT/Renderer/Poiyomi/Duplicate with New Materials", false, ContextRendererBase + ToolsOffset)]
        static void DuplicateRendererWithNewMaterials(MenuCommand command)
        {
            DuplicateWithUniqueMaterials.DuplicateWithNewMaterials((command.context as Renderer).gameObject);
        }

        [MenuItem("CONTEXT/Renderer/Poiyomi/Duplicate Only Translatable Materials", false, ContextRendererBase + ToolsOffset + 1)]
        static void DuplicateRendererTranslatableMaterials(MenuCommand command)
        {
            DuplicateWithUniqueMaterialsOnlyTranslatable.DuplicateWithNewMaterialsOnlyTranslatable((command.context as Renderer).gameObject);
        }

        [MenuItem("CONTEXT/Renderer/Poiyomi/Move Materials to Folder", false, ContextRendererBase + ToolsOffset + 2)]
        static void MoveRendererMaterialsToFolder(MenuCommand command)
        {
            MoveAvatarMaterialsToFolder.MoveMaterialsToNewFolder((command.context as Renderer).gameObject);
        }

        #endregion

        #region Context - GameObject

        [MenuItem("GameObject/Poiyomi/Lock Materials", priority = ContextGameObjectBase + LockOffset)]
        static void LockMaterialsInGameObject()
        {
            foreach (var obj in Selection.gameObjects)
                Undo.RegisterFullObjectHierarchyUndo(obj, "Lock materials");

            _SetLocked(GetMaterialsInChildren(Selection.gameObjects), true, "Lock materials", ShaderOptimizer.ProgressBar.Cancellable);
        }

        [MenuItem("GameObject/Poiyomi/Unlock Materials", priority = ContextGameObjectBase + UnlockOffset)]
        static void UnlockMaterialsInGameObject()
        {
            foreach (var obj in Selection.gameObjects)
                Undo.RegisterFullObjectHierarchyUndo(obj, "Unlock materials");

            _SetLocked(GetMaterialsInChildren(Selection.gameObjects), false, "Unlock materials", ShaderOptimizer.ProgressBar.Cancellable);
        }

        [MenuItem("GameObject/Poiyomi/Open in Cross Shader Editor", priority = ContextGameObjectCrossEditor)]
        static void OpenInCrossShaderEditor()
        {
            _OpenInCrossShaderEditor(GetMaterialsInChildren(Selection.gameObjects));
        }

        static IEnumerable<Material> GetMaterialsInChildren(params GameObject[] objects)
        {
            return objects.SelectMany(o => o.GetComponentsInChildren<Renderer>(true)).SelectMany(r => r.sharedMaterials).Where(m => m != null).Distinct();
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