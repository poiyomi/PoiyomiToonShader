using System;
using System.Collections.Generic;
using System.Linq;
using Thry;
using UnityEditor;
using UnityEngine;

namespace Poi.Tools.Menus
{
    public class PoiContextMenus
    {
        public const int ContextMaterialBase = 2020;
        public const int ContextRendererBase = 2020;
        public const int AssetsMenuBase = 1110;
        public const int ContextGameObjectBase = 24;
        public const int ContextGameObjectMaterial = 30;
        public const int ContextGameObjectTools = 40;

        #region Assets

        [MenuItem("Assets/Poiyomi/Materials/Lock Materials", priority = AssetsMenuBase)]
        static void LockMaterialsInAssets()
        {
            var mats = _GetSelectedMaterials();
            ShaderOptimizer.SetLockedForAllMaterials(mats, 1);
        }

        [MenuItem("Assets/Poiyomi/Materials/Unlock Materials", priority = AssetsMenuBase + 1)]
        static void UnlockMaterialsInAssets()
        {
            var mats = _GetSelectedMaterials();
            ShaderOptimizer.SetLockedForAllMaterials(mats, 0);
        }

        #endregion

        #region Context - Renderer

        [MenuItem("CONTEXT/Renderer/Poiyomi/Lock Materials")]
        static void LockRendererMaterials(MenuCommand command)
        {
            var renderer = command.context as Renderer;
            int undoIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Lock materials in {renderer.name}");

            ShaderOptimizer.SetLockedForAllMaterials(renderer.sharedMaterials, 1);

            Undo.CollapseUndoOperations(undoIndex);
        }

        [MenuItem("CONTEXT/Renderer/Poiyomi/Unlock Materials")]
        static void UnlockRendererMaterials(MenuCommand command)
        {
            var renderer = command.context as Renderer;
            int undoIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Lock materials in {renderer.name}");

            ShaderOptimizer.SetLockedForAllMaterials(renderer.sharedMaterials, 0);

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

            ShaderOptimizer.SetLockForAllChildren(new[] { command.context as GameObject }, 1);

            Undo.CollapseUndoOperations(undoIndex);
        }

        [MenuItem("GameObject/Poiyomi/Materials/Unlock Materials", priority = ContextGameObjectMaterial + 1)]
        static void UnlockMaterialsInGameObject(MenuCommand command)
        {
            GameObject obj = command.context as GameObject;

            int undoIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Unlock materials in {obj.name}");
            Undo.RegisterFullObjectHierarchyUndo(obj, $"Unlock materials in {obj.name}");

            ShaderOptimizer.SetLockForAllChildren(new[] { command.context as GameObject }, 0);

            Undo.CollapseUndoOperations(undoIndex);
        }

        #endregion

        #region Context - GameObject - Tools

        [MenuItem("GameObject/Poiyomi/Tools/Duplicate with New Materials", false, priority = ContextGameObjectTools)]
        public static void NoWorkie1(MenuCommand command)
        {
            DuplicateWithUniqueMaterials.DuplicateWithNewMaterials(command.context as GameObject);
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