// Automated Shader Translation designed by BluWizard LABS.
// https://github.com/BluWizard10

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Thry.ThryEditor.Helpers;
using Poi.Tools.ShaderTranslator.VersionUpgrade;

namespace Poi.Tools.ShaderTranslator.Translations
{
    /// <summary>
    /// Standalone editor watcher that auto-runs the lilToon -> Poiyomi translation when a material's shader is changed
    /// from lilToon to Poiyomi - e.g. when the user picks a Poiyomi shader from the inspector's shader dropdown instead
    /// of using the "Translate to Poiyomi" context menu.
    /// 
    /// Follows a similar pipeline to the PoiyomiAutoUpgradeOnShaderSwap, but tailored specifically for LilToon to Poiyomi
    /// translations. 
    ///
    /// The context-menu commands in LiltoonToPoiyomiTranslation_Menus are unchanged and simply adds a second automatic
    /// entry point for ease of use.
    /// </summary>
    [InitializeOnLoad]
    static class LiltoonToPoiyomiAutoTranslate
    {
        const string EnabledPrefKey = "Poi.Translator.AutoTranslateLiltoonOnShaderSwap";
        const string MenuPath = "Poi/Auto-Translate lilToon Materials On Shader Change";
        const string LockedShaderPrefix = "Hidden/Locked/";

        static Dictionary<int, Shader> _watchedShaders = new Dictionary<int, Shader>();

        static LiltoonToPoiyomiToonTranslation _toonInstance;
        static LiltoonToPoiyomiProTranslation _proInstance;

        static LiltoonToPoiyomiToonTranslation ToonInstance => _toonInstance ?? (_toonInstance = new LiltoonToPoiyomiToonTranslation());
        static LiltoonToPoiyomiProTranslation ProInstance => _proInstance ?? (_proInstance = new LiltoonToPoiyomiProTranslation());

        static LiltoonToPoiyomiAutoTranslate()
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
                    TryAutoTranslate(material, previousShader, currentShader);
                    currentShader = material.shader; // re-read in case the translation changed anything
                }

                next[id] = currentShader;
            }

            _watchedShaders = next;
        }

        static IEnumerable<Material> CollectInspectedMaterials()
        {
            var set = new HashSet<Material>();

            foreach (var obj in Selection.objects)
            {
                if (obj is Material material) set.Add(material);
            }

            foreach (var go in Selection.gameObjects)
            {
                if (go == null) continue;

                foreach (var renderer in go.GetComponents<Renderer>())
                {
                    foreach (var material in renderer.sharedMaterials)
                    {
                        if (material != null) set.Add(material);
                    }
                }
            }

            return set;
        }

        static void TryAutoTranslate(Material material, Shader oldShader, Shader newShader)
        {
            // Only react to genuine LilToon -> Poiyomi swap. Ignore Locked shaders in either direction.
            if (IsLocked(oldShader) || IsLocked(newShader)) return;
            if (!IsLiltoon(oldShader) || !IsPoiyomi(newShader)) return;

            var translator = PoiyomiVersionDetector.IsProShader(newShader) ? ProInstance : ToonInstance;

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

        static bool IsLocked(Shader shader) => shader != null && shader.name.StartsWith(LockedShaderPrefix, StringComparison.OrdinalIgnoreCase);
        static bool IsLiltoon(Shader shader) => shader != null && shader.name.IndexOf("liltoon", StringComparison.OrdinalIgnoreCase) != -1;
        static bool IsPoiyomi(Shader shader) => shader != null && shader.name.IndexOf("poiyomi", StringComparison.OrdinalIgnoreCase) != -1;
    } 
}