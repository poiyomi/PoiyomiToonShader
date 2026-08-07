// Automated Shader Upgrading designed by BluWizard LABS.
// https://github.com/BluWizard10

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Thry.ThryEditor.Helpers;

namespace Poi.Tools.ShaderTranslator.VersionUpgrade
{
    /// <summary>
	/// Standalone editor watcher that auto-runs the Poiyomi version upgrade when a material's shader is changed from an
	/// older Poiyomi version to a newer one - e.g. when the user picks the new shader from the inspector's shader
	/// dropdown instead of using the "Update Poiyomi Shaders" context menu.
	///
	/// We do this instead of ShaderGUI.AssignNewShaderToMaterial as that keeps the behavior entirely inside on the
    /// Poiyomi-side of things. Once a swap happens the "old version" is otherwise lost. We solve this by making a
    /// snapshot of  the shader of any material currently being inspected, then detecting the moment it changes -
	/// at which point we still have both the old and new shader references, and the old property values are still
	/// serialized on the material.
	/// </summary>
    [InitializeOnLoad] 
    static class PoiyomiAutoUpgradeOnShaderSwap
    {
        const string EnabledPrefKey = "Poi.VersionUpgrade.AutoUpgradeOnShaderSwap";
		const string MenuPath = "Poi/Auto-Upgrade Materials On Shader Change";
		const string LockedShaderPrefix = "Hidden/Locked/";

        // Last-seen shader for each material we're currently watching, keyed by instance id.
		static Dictionary<int, Shader> _watchedShaders = new Dictionary<int, Shader>();

        static PoiyomiAutoUpgradeOnShaderSwap()
		{
			EditorApplication.update += OnEditorUpdate;
		}

		static bool Enabled
		{
			get => EditorPrefs.GetBool(EnabledPrefKey, true);
			set => EditorPrefs.SetBool(EnabledPrefKey, value);
		}

        [MenuItem(MenuPath, false)]
		static void ToggleEnabled() => Enabled = !Enabled;

		[MenuItem(MenuPath, true)]
		static bool ToggleEnabled_Validate()
		{
			UnityEditor.Menu.SetChecked(MenuPath, Enabled);
			return true;
		}

        static void OnEditorUpdate()
		{
			// Don't poke materials while Unity is busy; values can be mid-flight.
			if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;

			if (!Enabled)
			{
				if (_watchedShaders.Count > 0) _watchedShaders.Clear();
				return;
			}

			var next = new Dictionary<int, Shader>(_watchedShaders.Count);

			foreach (Material material in CollectInspectedMaterials())
			{
				int id = material.GetObjectId();
				Shader currentShader = material.shader;

				if (_watchedShaders.TryGetValue(id, out Shader previousShader) &&
					previousShader != null && currentShader != null && previousShader != currentShader)
				{
					TryAutoUpgrade(material, previousShader, currentShader);
					currentShader = material.shader; // re-read in case the upgrade changed anything
				}

				next[id] = currentShader;
			}

			_watchedShaders = next;
		}

        /// <summary>
		/// Materials reachable from the current selection - either selected directly as assets, or shown via a selected
		/// GameObject's renderer. These are the materials whose shader dropdown the user can actually be interacting with.
		/// </summary>
        static IEnumerable<Material> CollectInspectedMaterials()
		{
			var set = new HashSet<Material>();

			foreach (var obj in Selection.objects)
			{
				if (obj is Material material)
					set.Add(material);
			}

			foreach (var go in Selection.gameObjects)
			{
				if (go == null)
					continue;

				foreach (var renderer in go.GetComponents<Renderer>())
				{
					foreach (var material in renderer.sharedMaterials)
					{
						if (material != null)
							set.Add(material);
					}
				}
			}

			return set;
		}

        static void TryAutoUpgrade(Material material, Shader oldShader, Shader newShader)
		{
			// Ignore locked/optimized shaders in either direction - locking and unlocking are not version changes.
			if (IsLocked(oldShader) || IsLocked(newShader)) return;

			if (!IsPoiyomi(oldShader) || !IsPoiyomi(newShader)) return;

			if (!PoiyomiVersionDetector.TryGetVersionFromShader(oldShader, out Version oldVersion) || !PoiyomiVersionDetector.TryGetVersionFromShader(newShader, out Version newVersion)) return;

			// Only react to forward upgrades.
			if (oldVersion >= newVersion)
				return;

			// Stay within the same shader variant, except for a Toon <-> Pro edition swap of the same sub-variant
			// (e.g. Toon 9.3 -> Pro 10.0). Toon and Pro share a property layout, so settings shall carry across
			// cleanly. Any other variant change would corrupt the material, so we leave it alone.
			// PoiyomiVersionUpgradeController already enforces this as well, but bailing here avoids a no-op Undo entry.
			string oldVariant = PoiyomiVersionDetector.GetShaderVariant(oldShader);
			string newVariant = PoiyomiVersionDetector.GetShaderVariant(newShader);
			if (!string.Equals(oldVariant, newVariant, StringComparison.OrdinalIgnoreCase))
			{
				if (!PoiyomiVersionDetector.IsEditionSwap(oldShader, newShader)) return;
			}

			try
			{
				Undo.RegisterCompleteObjectUndo(material, $"Auto-Upgrade {material.name}");
				if (PoiyomiVersionUpgradeController.UpgradeAcrossShaderSwap(material, oldShader, newShader)) EditorUtility.SetDirty(material);
			}
			catch (Exception ex)
			{
				ThryLogger.LogErr($"Auto-upgrade failed for material <b>{material.name}</b>. Please report this bug with a screenshot of this stack trace!");
				Debug.LogException(ex);
			}
		}

        static bool IsLocked(Shader shader) => shader != null && shader.name.StartsWith(LockedShaderPrefix, StringComparison.OrdinalIgnoreCase);

		static bool IsPoiyomi(Shader shader) => shader != null && shader.name.IndexOf("poiyomi", StringComparison.OrdinalIgnoreCase) != -1;
    }
}
