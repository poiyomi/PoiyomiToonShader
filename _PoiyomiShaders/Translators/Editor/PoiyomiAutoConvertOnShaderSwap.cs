// Automated Shader Translation/Upgrading designed by BluWizard LABS.
// https://github.com/BluWizard10

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Thry.ThryEditor.Helpers;
using Poi.Tools.ShaderTranslator.Translations;

namespace Poi.Tools.ShaderTranslator.VersionUpgrade
{
    /// <summary>
    /// Standalone editor watcher that automatically converts a material when its shader is changed to Poiyomi
    /// from the inspector's shader dropdown, instead of through the "Translate to Poiyomi" / "Update Poiyomi
    /// Shaders" context menus. Two things can happen depending on where the material came from:
    ///
    /// - an older Poiyomi shader gets version-upgraded (9.x -> 10.0 and friends)
    /// - a supported third-party shader (lilToon, Unity Standard) gets translated
    ///
    /// Both are the same user action - "I picked a different shader and expect my material to survive" - so
    /// they share one watcher and one toggle rather than one per source shader.
    ///
    /// We do this instead of ShaderGUI.AssignNewShaderToMaterial as that keeps the behavior entirely on the
    /// Poiyomi side of things. Once a swap happens the old shader is otherwise lost. We solve this by making a
    /// snapshot of the shader of any material currently being inspected, then detecting the moment it changes -
    /// at which point we still have both the old and new shader references, and the old property values are
    /// still serialized on the material.
    /// </summary>
    [InitializeOnLoad]
    static class PoiyomiAutoConvertOnShaderSwap
    {
        // Key kept from when upgrades and translations had a toggle each, so anyone who turned the upgrade
        // side off stays opted out of both.
        const string EnabledPrefKey = "Poi.VersionUpgrade.AutoUpgradeOnShaderSwap";
        const string MenuPath = "Poi/Auto-Translate Materials On Shader Swap";
        const string LockedShaderPrefix = "Hidden/Locked/";

        // Last-seen shader for each material we're currently watching, keyed by instance id.
        static Dictionary<int, Shader> _watchedShaders = new Dictionary<int, Shader>();

        static PoiyomiAutoConvertOnShaderSwap()
        {
            EditorApplication.update += OnEditorUpdate;

            // ThryEditor invokes this just before the inspector's shader dropdown assigns a new shader, and
            // cancels the swap entirely if it returns false. We chain onto any existing handler and use it to
            // confirm Grab Pass upgrades, whose behavior changed enough between versions to warrant a heads-up
            // before the swap happens.
            var previous = Thry.ShaderEditor.OnBeforeAssignNewShader;
            Thry.ShaderEditor.OnBeforeAssignNewShader = (material, oldShader, newShader) =>
            {
                if (previous != null && !previous(material, oldShader, newShader)) return false;
                return ConfirmShaderSwap(material, oldShader, newShader);
            };
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
                    TryAutoConvert(material, previousShader, currentShader);
                    currentShader = material.shader; // re-read in case the conversion changed anything
                }

                next[id] = currentShader;
            }

            _watchedShaders = next;
        }

        /// <summary>
        /// Materials reachable from the current selection - either selected directly as assets, or shown via a
        /// selected GameObject's renderer. These are the materials whose shader dropdown the user can actually
        /// be interacting with.
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

        static void TryAutoConvert(Material material, Shader oldShader, Shader newShader)
        {
            // Ignore locked/optimized shaders in either direction - locking and unlocking are not shader changes
            // in any sense we care about here.
            if (IsLocked(oldShader) || IsLocked(newShader)) return;

            // Only ever react to something becoming Poiyomi.
            if (!IsPoiyomi(newShader)) return;

            if (IsPoiyomi(oldShader))
                TryAutoUpgrade(material, oldShader, newShader);
            else
                TryAutoTranslate(material, oldShader, newShader);
        }

        static void TryAutoUpgrade(Material material, Shader oldShader, Shader newShader)
        {
            if (!PoiyomiVersionDetector.TryGetVersionFromShader(oldShader, out Version oldVersion) || !PoiyomiVersionDetector.TryGetVersionFromShader(newShader, out Version newVersion)) return;

            // Only react to forward upgrades.
            if (oldVersion >= newVersion) return;

            // Stay within the same shader variant, except for a Toon <-> Pro edition swap of the same sub-variant
            // (e.g. Toon 9.3 -> Pro 10.0). Toon and Pro share a property layout, so settings shall carry across
            // cleanly. Any other variant change would corrupt the material, so we leave it alone.
            // PoiyomiVersionUpgradeController already enforces this as well, but bailing here avoids a no-op Undo entry.
            // Exception: pre-9.3 sources (8.x, or removed 9.0-9.2) have differently-named variants and are routed through
            // the legacy pipeline (which remaps the variant itself), so skip the guard and let the controller handle them.
            if (oldVersion >= new Version(9, 3))
            {
                string oldVariant = PoiyomiVersionDetector.GetShaderVariant(oldShader);
                string newVariant = PoiyomiVersionDetector.GetShaderVariant(newShader);
                if (!string.Equals(oldVariant, newVariant, StringComparison.OrdinalIgnoreCase))
                {
                    if (!PoiyomiVersionDetector.IsEditionSwap(oldShader, newShader)) return;
                }
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

        static void TryAutoTranslate(Material material, Shader oldShader, Shader newShader)
        {
            // The material already points at the new shader, so the translation has to be picked from the shader
            // its values were authored against rather than from the material.
            var translator = PoiyomiTranslatorRegistry.GetTranslatorForShader(oldShader, PoiyomiVersionDetector.IsProShader(newShader));
            if (translator == null) return;

            try
            {
                Undo.RegisterCompleteObjectUndo(material, $"Auto-Translate {material.name} to Poiyomi");
                translator.TranslateAcrossShaderSwap(material, oldShader, newShader);
                EditorUtility.SetDirty(material);
            }
            catch (Exception ex)
            {
                ThryLogger.LogErr($"Auto-translate failed for material <b>{material.name}</b>. Please report this bug with a screenshot of this stack trace!");
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// Confirmation gate for the inspector's shader dropdown. We only interrupt to warn about Grab Pass
        /// materials, whose base-color/alpha handling and refraction effects changed enough between versions
        /// that a silent auto-upgrade could surprise the user. Everything else proceeds untouched and is
        /// converted afterwards by the watcher above. Returns false to cancel the swap (leaving the material on
        /// its current shader).
        /// </summary>
        static bool ConfirmShaderSwap(Material material, Shader oldShader, Shader newShader)
        {
            if (!Enabled) return true;
            if (material == null || oldShader == null || newShader == null) return true;
            if (IsLocked(oldShader) || IsLocked(newShader)) return true;
            if (!IsPoiyomi(oldShader) || !IsPoiyomi(newShader)) return true;

            // Only worth interrupting for a Grab Pass material moving forward to a newer Poiyomi version.
            if (!PoiyomiVersionDetector.IsGrabPassShaderName(oldShader.name)) return true;
            if (!PoiyomiVersionDetector.TryGetVersionFromShader(oldShader, out Version oldVersion) ||
                !PoiyomiVersionDetector.TryGetVersionFromShader(newShader, out Version newVersion)) return true;
            if (oldVersion >= newVersion) return true;

            return EditorUtility.DisplayDialog("Upgrade Grab Pass Material",
                $"\"{material.name}\" uses Grab Pass, which was overhauled significantly in {newVersion.Major}.{newVersion.Minor}.\n\n" +
                "Blend, Refraction, Chromatic Aberration and the Global Masks will carry over. However, the Base Color and Alpha " +
                "handling has changed (\"Use Material Alpha\" is now driven by the Alpha Mask) and some options are non-transferrable " +
                "(Hue Shift Replace, multi-directional Blur, the per-channel Blend Mask).\n\n" +
                "This action cannot be undone, so please consider making a copy of this material before upgrading.\n\n" +
                "Are you sure you wish to continue upgrading this material?",
                "Do it!", "Cancel");
        }

        static bool IsLocked(Shader shader) => shader != null && shader.name.StartsWith(LockedShaderPrefix, StringComparison.OrdinalIgnoreCase);

        // Strict ".poiyomi/" match, not a substring search - a third-party shader with "poiyomi" in its name would
        // otherwise be taken for an older Poiyomi version and sent down the upgrade path instead of the translate one.
        static bool IsPoiyomi(Shader shader) => shader != null && PoiyomiVersionDetector.IsPoiyomiShaderName(shader.name);
    }
}
