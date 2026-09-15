using System;
using System.Collections.Generic;
using System.Linq;
using Poiyomi.ModularShaderSystem;
using UnityEditor;
using UnityEngine;

namespace Poi.Tools
{
    /// <summary>
    /// Watches for freshly imported user modules and points them out in the console, so importing a
    /// module package is all the user has to do to have it show up. Also nags once per session when
    /// installed modules have fallen out of the shaders, which is what a Poiyomi update does.
    ///
    /// Nothing in here opens a window. This runs from an asset postprocessor, which fires on project
    /// launch and on every reimport, so anything that shows UI here shows it unprompted.
    /// </summary>
    public class PoiUserModuleWatcher : AssetPostprocessor
    {
        const string GeneratingKey = "Poi_IsGeneratingShaders";
        const string DriftNoticeKey = "Poi_UserModules_DriftNoticeShown";

#if UNITY_2021_2_OR_NEWER
        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom, bool didDomainReload)
#else
        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
#endif
        {
            // Shader generation reimports a lot of assets; nothing here should react to that.
            if (SessionState.GetBool(GeneratingKey, false))
                return;

            // GetMainAssetTypeAtPath reads the importer metadata rather than loading the asset, so
            // this stays cheap on the large imports that follow a package update.
            bool touchedModules = imported.Concat(moved).Any(IsShaderModuleAsset);
            if (!touchedModules)
                return;

            EditorApplication.delayCall += HandleModuleChanges;
        }

        /// <summary>
        /// A Poiyomi update can land without any module asset being imported afterwards, so also
        /// check for drift once when the editor loads.
        /// </summary>
        [InitializeOnLoadMethod]
        static void CheckOnStartup()
        {
            // Runs on every domain reload, so bail on the session flag before doing any asset work.
            if (SessionState.GetBool(DriftNoticeKey, false))
                return;

            EditorApplication.delayCall += () =>
            {
                if (SessionState.GetBool(GeneratingKey, false))
                    return;
                WarnIfShadersDriftedFromModules(PoiUserModules.Discover());
            };
        }

        static bool IsShaderModuleAsset(string path)
        {
            if (!path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                return false;
            var type = AssetDatabase.GetMainAssetTypeAtPath(path);
            return type != null && typeof(ShaderModule).IsAssignableFrom(type);
        }

        static void HandleModuleChanges()
        {
            if (SessionState.GetBool(GeneratingKey, false))
                return;

            PoiUserModules.InvalidateCaches();
            var entries = PoiUserModules.Discover();

            var newlyFound = new List<PoiUserModules.Entry>();
            foreach (var entry in entries)
                if (PoiUserModules.MarkSeen(entry.Guid))
                    newlyFound.Add(entry);

            var window = Resources.FindObjectsOfTypeAll<PoiUserModulesWindow>().FirstOrDefault();
            if (window != null)
                window.Refresh();

            if (newlyFound.Count > 0)
            {
                string names = string.Join(", ", newlyFound.Select(x => x.DisplayName));
                Debug.Log($"[Poiyomi] Found {newlyFound.Count} new shader module{(newlyFound.Count == 1 ? "" : "s")}: {names}. " + "Open Poi > Installed Modules to turn them on.");
            }

            WarnIfShadersDriftedFromModules(entries);
        }

        /// <summary>
        /// After a Poiyomi update the shipped shader assets are replaced, dropping the installed
        /// modules. Tell the user once rather than leaving them wondering where their module went.
        /// </summary>
        static void WarnIfShadersDriftedFromModules(List<PoiUserModules.Entry> entries)
        {
            if (SessionState.GetBool(DriftNoticeKey, false))
                return;

            var enabled = PoiUserModules.GetEnabledModules(entries);
            if (enabled.Count == 0)
                return;

            var outOfDate = PoiUserModules.GetOutOfDateShaders(enabled: enabled);
            if (outOfDate.Count == 0)
                return;

            SessionState.SetBool(DriftNoticeKey, true);
            Debug.LogWarning($"[Poiyomi] {enabled.Count} installed module{(enabled.Count == 1 ? " is" : "s are")} " +
                            $"missing from {outOfDate.Count} shaders. Open Poi > Installed Modules and hit " +
                            "\"Apply & Rebuild Shaders\" to put them back.");
        }
    }
}
