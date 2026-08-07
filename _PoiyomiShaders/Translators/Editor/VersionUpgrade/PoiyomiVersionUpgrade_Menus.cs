using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Poi.Tools.Menus;

namespace Poi.Tools.ShaderTranslator.VersionUpgrade.Menu
{
	static class PoiyomiVersionUpgrade_Menus
	{
		const int UpgradeMenuPriority = 50;

		#region Context - Material

		[MenuItem("CONTEXT/Material/Poiyomi/Update Poiyomi Shaders", false, PoiContextMenus.ContextMaterialBase + UpgradeMenuPriority)]
		static void UpgradeMaterial(MenuCommand command)
		{
			var material = command.context as Material;
			if (material == null)
				return;

			Undo.RegisterCompleteObjectUndo(material, $"Update Poiyomi Shaders on {material.name}");
			PoiyomiVersionUpgradeController.UpgradeToLatest(material);
		}

		[MenuItem("CONTEXT/Material/Poiyomi/Update Poiyomi Shaders", true)]
		static bool UpgradeMaterial_Validate(MenuCommand command)
		{
			var material = command.context as Material;
			return material != null && PoiyomiVersionDetector.NeedsUpgrade(material);
		}

		#endregion

		#region Context - Renderer

		[MenuItem("CONTEXT/Renderer/Poiyomi/Update Poiyomi Shaders", false, PoiContextMenus.ContextRendererBase + UpgradeMenuPriority)]
		static void UpgradeRendererMaterials(MenuCommand command)
		{
			var renderer = command.context as Renderer;
			if (renderer == null)
				return;

			var materials = renderer.sharedMaterials.Where(m => m != null).ToArray();
			if (materials.Length == 0)
				return;

			int undoIndex = Undo.GetCurrentGroup();
			Undo.SetCurrentGroupName($"Update Poiyomi Shaders on {renderer.name}");

			foreach (var material in materials)
				Undo.RegisterCompleteObjectUndo(material, $"Update {material.name}");

			PoiyomiVersionUpgradeController.UpgradeMaterials(materials);
			Undo.CollapseUndoOperations(undoIndex);
		}

		[MenuItem("CONTEXT/Renderer/Poiyomi/Update Poiyomi Shaders", true)]
		static bool UpgradeRendererMaterials_Validate(MenuCommand command)
		{
			var renderer = command.context as Renderer;
			if (renderer == null)
				return false;

			return renderer.sharedMaterials.Any(m => m != null && PoiyomiVersionDetector.NeedsUpgrade(m));
		}

		#endregion

		#region Context - GameObject

		[MenuItem("GameObject/Poiyomi/Update Poiyomi Shaders", false, priority = PoiContextMenus.ContextGameObjectUpdate)]
		static void UpgradeGameObjectMaterials(MenuCommand command)
		{
			var obj = command.context as GameObject;
			if (obj == null)
				return;

			var materials = PoiHelpers.CollectMaterialsFromGameObject(obj, true,
				"Material Swap Animations Detected",
				"Animations that swap materials on your avatar were detected. Would you like materials inside those animations to be updated as well?\n\nAffected animations:\n{0}");

			if (materials.Count == 0)
				return;

			int undoIndex = Undo.GetCurrentGroup();
			Undo.SetCurrentGroupName($"Update Poiyomi Shaders on {obj.name}");

			foreach (var material in materials)
				Undo.RegisterCompleteObjectUndo(material, $"Update {material.name}");

			PoiyomiVersionUpgradeController.UpgradeMaterials(materials);
			Undo.CollapseUndoOperations(undoIndex);
		}

		[MenuItem("GameObject/Poiyomi/Update Poiyomi Shaders", true)]
		static bool UpgradeGameObjectMaterials_Validate()
		{
			return Selection.activeGameObject != null &&
				PoiHelpers.HasMaterialsMatching(Selection.activeGameObject, PoiyomiVersionDetector.NeedsUpgrade);
		}

		#endregion

		#region Assets

		[MenuItem("Assets/Poiyomi/Materials/Update Poiyomi Shaders", false, PoiContextMenus.AssetsMenuBase + UpgradeMenuPriority)]
		static void UpgradeSelectedMaterials()
		{
			var materials = GetSelectedMaterials();
			if (materials.Count == 0)
				return;

			int undoIndex = Undo.GetCurrentGroup();
			Undo.SetCurrentGroupName("Update Poiyomi Shaders");

			foreach (var material in materials)
				Undo.RegisterCompleteObjectUndo(material, $"Update {material.name}");

			PoiyomiVersionUpgradeController.UpgradeMaterials(materials);
			Undo.CollapseUndoOperations(undoIndex);
		}

		[MenuItem("Assets/Poiyomi/Materials/Update Poiyomi Shaders", true)]
		static bool UpgradeSelectedMaterials_Validate()
		{
			return GetSelectedMaterials().Any(m => PoiyomiVersionDetector.NeedsUpgrade(m));
		}

		#endregion

		#region Helper Functions

		static List<Material> GetSelectedMaterials()
		{
			var materialList = new List<Material>();

			foreach (var obj in Selection.objects)
			{
				if (obj == null)
					continue;

				if (obj is Material mat)
				{
					materialList.Add(mat);
				}
				else if (obj is DefaultAsset)
				{
					string folderPath = AssetDatabase.GetAssetPath(obj);
					if (!AssetDatabase.IsValidFolder(folderPath))
						continue;

					var guids = AssetDatabase.FindAssets("t:Material", new string[] { folderPath });
					foreach (var guid in guids)
					{
						string path = AssetDatabase.GUIDToAssetPath(guid);
						var material = AssetDatabase.LoadAssetAtPath<Material>(path);
						if (material != null)
							materialList.Add(material);
					}
				}
			}

			return materialList;
		}

		#endregion
	}
}
