// Prototype menus for the conservative removed-version (9.2 and older) -> 9.3 upgrade.
// Mirrors PoiyomiVersionUpgrade_Menus but routes to PoiyomiUpgrade_9_X_to_9_3 and is gated by the legacy
// detector, so it lights up for materials the normal "Update Poiyomi Shaders" item can't even see
// (error-shader unlocked 9.2, or locked 9.2 that mis-resolves to 10.0).

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Poi.Tools.Menus;

namespace Poi.Tools.ShaderTranslator.VersionUpgrade.Menu
{
	static class LegacyUpgrade_Menus
	{
		// Sits just below the normal "Update Poiyomi Shaders" item.
		const int LegacyMenuPriority = 51;
		const string MaterialItem = "CONTEXT/Material/Poiyomi/Update Legacy Material (9.x → 9.3)";
		const string RendererItem = "CONTEXT/Renderer/Poiyomi/Update Legacy Materials (9.x → 9.3)";
		const string GameObjectItem = "GameObject/Poiyomi/Update Legacy Materials (9.x → 9.3)";
		const string AssetsItem = "Assets/Poiyomi/Materials/Update Legacy Materials (9.x → 9.3)";

		#region Context - Material

		[MenuItem(MaterialItem, false, PoiContextMenus.ContextMaterialBase + LegacyMenuPriority)]
		static void UpgradeMaterial(MenuCommand command)
		{
			if (!(command.context is Material material))
				return;

			Undo.RegisterCompleteObjectUndo(material, $"Update Legacy Poiyomi Material on {material.name}");
			PoiyomiUpgrade_9_X_to_9_3.UpgradeToNine3(material);
		}

		[MenuItem(MaterialItem, true)]
		static bool UpgradeMaterial_Validate(MenuCommand command) =>
			command.context is Material material && LegacyMaterialDetector.NeedsLegacyUpgrade(material);

		#endregion

		#region Context - Renderer

		[MenuItem(RendererItem, false, PoiContextMenus.ContextRendererBase + LegacyMenuPriority)]
		static void UpgradeRendererMaterials(MenuCommand command)
		{
			if (!(command.context is Renderer renderer))
				return;

			var materials = renderer.sharedMaterials.Where(m => m != null).ToArray();
			if (materials.Length == 0)
				return;

			int undoIndex = Undo.GetCurrentGroup();
			Undo.SetCurrentGroupName($"Update Legacy Poiyomi Materials on {renderer.name}");
			foreach (var material in materials)
				Undo.RegisterCompleteObjectUndo(material, $"Update {material.name}");

			PoiyomiUpgrade_9_X_to_9_3.UpgradeMaterials(materials);
			Undo.CollapseUndoOperations(undoIndex);
		}

		[MenuItem(RendererItem, true)]
		static bool UpgradeRendererMaterials_Validate(MenuCommand command) =>
			command.context is Renderer renderer &&
			renderer.sharedMaterials.Any(m => m != null && LegacyMaterialDetector.NeedsLegacyUpgrade(m));

		#endregion

		#region Context - GameObject

		[MenuItem(GameObjectItem, false, priority = PoiContextMenus.ContextGameObjectUpdate + 1)]
		static void UpgradeGameObjectMaterials(MenuCommand command)
		{
			if (!(command.context is GameObject obj))
				return;

			var materials = PoiHelpers.CollectMaterialsFromGameObject(obj, true,
				"Material Swap Animations Detected",
				"Animations that swap materials on your avatar were detected. Would you like materials inside those animations to be updated as well?\n\nAffected animations:\n{0}");

			if (materials.Count == 0)
				return;

			int undoIndex = Undo.GetCurrentGroup();
			Undo.SetCurrentGroupName($"Update Legacy Poiyomi Materials on {obj.name}");
			foreach (var material in materials)
				Undo.RegisterCompleteObjectUndo(material, $"Update {material.name}");

			PoiyomiUpgrade_9_X_to_9_3.UpgradeMaterials(materials);
			Undo.CollapseUndoOperations(undoIndex);
		}

		[MenuItem(GameObjectItem, true)]
		static bool UpgradeGameObjectMaterials_Validate() =>
			Selection.activeGameObject != null &&
			PoiHelpers.HasMaterialsMatching(Selection.activeGameObject, LegacyMaterialDetector.NeedsLegacyUpgrade);

		#endregion

		#region Assets

		[MenuItem(AssetsItem, false, PoiContextMenus.AssetsMenuBase + LegacyMenuPriority)]
		static void UpgradeSelectedMaterials()
		{
			var materials = GetSelectedMaterials();
			if (materials.Count == 0)
				return;

			int undoIndex = Undo.GetCurrentGroup();
			Undo.SetCurrentGroupName("Update Legacy Poiyomi Materials");
			foreach (var material in materials)
				Undo.RegisterCompleteObjectUndo(material, $"Update {material.name}");

			PoiyomiUpgrade_9_X_to_9_3.UpgradeMaterials(materials);
			Undo.CollapseUndoOperations(undoIndex);
		}

		[MenuItem(AssetsItem, true)]
		static bool UpgradeSelectedMaterials_Validate() =>
			GetSelectedMaterials().Any(LegacyMaterialDetector.NeedsLegacyUpgrade);

		#endregion

		#region Helpers

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

					foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { folderPath }))
					{
						var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
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
