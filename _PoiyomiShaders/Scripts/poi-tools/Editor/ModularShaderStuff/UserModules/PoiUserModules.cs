using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Poiyomi.ModularShaderSystem;
using Poiyomi.ModularShaderSystem.CibbiExtensions;
using UnityEditor;
using UnityEngine;

namespace Poi.Tools
{
    /// <summary>
    /// Project-wide registry of user-installed shader modules.
    ///
    /// A "user module" is any ShaderModule asset in the project that didn't ship with Poiyomi.
    /// Enabled modules get written into every Poiyomi ModularShader's AdditionalModules list, so a
    /// rebuild folds them into the normal shaders. Materials keep pointing at "Poiyomi Toon" and the
    /// module's properties just show up in the inspector.
    ///
    /// The enabled list is stored outside the Assets folder so importing a Poiyomi update can't wipe
    /// it. An update does reset AdditionalModules on the shipped assets, which is exactly what
    /// NeedsRebuild picks up on.
    /// </summary>
    public static class PoiUserModules
    {
        const string SettingsFileName = "PoiInstalledModules.json";

        [Serializable]
        class Settings
        {
            public List<string> enabledGuids = new List<string>();

            /// <summary>Modules we've already told the user about, so an import only announces itself once.</summary>
            public List<string> seenGuids = new List<string>();
        }

        /// <summary>A user module found in the project, plus whatever is wrong with it.</summary>
        public class Entry
        {
            public ShaderModule Module;
            public string Guid;
            public string AssetPath;
            public bool Enabled;

            /// <summary>Non-null when the module can't be used (missing dependency, conflict, ...).</summary>
            public string Error;

            public string DisplayName => string.IsNullOrWhiteSpace(Module.Name) ? Module.name : Module.Name;
            public bool IsUsable => Error == null;
        }

        static Settings _settings;

        static Settings Data
        {
            get
            {
                if (_settings == null && !PoiSettingsUtility.TryLoadSettings(SettingsFileName, out _settings))
                    _settings = new Settings();
                if (_settings.enabledGuids == null)
                    _settings.enabledGuids = new List<string>();
                if (_settings.seenGuids == null)
                    _settings.seenGuids = new List<string>();
                return _settings;
            }
        }

        public static void Save() => PoiSettingsUtility.SaveSettings(SettingsFileName, Data);

        public static bool IsEnabled(string guid) => Data.enabledGuids.Contains(guid);

        /// <summary>Records a module as known. Returns true the first time it's seen.</summary>
        public static bool MarkSeen(string guid)
        {
            if (Data.seenGuids.Contains(guid))
                return false;
            Data.seenGuids.Add(guid);
            Save();
            return true;
        }

        public static void SetEnabled(string guid, bool enabled)
        {
            if (enabled && !Data.enabledGuids.Contains(guid))
                Data.enabledGuids.Add(guid);
            else if (!enabled)
                Data.enabledGuids.Remove(guid);
            Save();
        }

        // ------------------------------------------------------------------
        // Discovery
        // ------------------------------------------------------------------

        /// <summary>
        /// Folder holding the shipped modular shaders (normally
        /// Assets/_PoiyomiShaders/ModularShader/Editor). Found by locating the folder that holds the
        /// most ModularShader assets, so it survives the package being moved or renamed.
        /// Not the same thing as "PackageRoot" — shipped modules live outside this one. 
        /// </summary>
        public static string ShippingRoot
        {
            get
            {
                if (_shippingRoot != null)
                    return _shippingRoot;

                var byFolder = AssetDatabase.FindAssets("t:ModularShader")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .GroupBy(x => Path.GetDirectoryName(x).Replace('\\', '/'))
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();

                return _shippingRoot = byFolder?.Key ?? "Assets/_PoiyomiShaders/ModularShader/Editor";
            }
        }

        /// <summary>
        /// Root of the Poiyomi install — Assets/_PoiyomiShaders normally, or the package folder on a
        /// VCC setup. Taken as the deepest folder shared by <see cref="ShippingRoot"/> and this
        /// script, so it holds up wherever the package sits and whatever it has been renamed to.
        ///
        /// Everything under here shipped with Poiyomi. Matching on ShippingRoot alone was not enough:
        /// Poiyomi keeps modules outside the modular shader folder (TPS is one), and those were being
        /// picked up as freshly installed user modules.
        /// </summary>
        public static string PackageRoot
        {
            get
            {
                if (_packageRoot != null)
                    return _packageRoot;

                string root = DeepestSharedFolder(ShippingRoot, ThisScriptFolder());

                // A single segment means the two paths only met at "Assets"/"Packages", so they are
                // not in a package together. Taking that as the root would mark the whole project as
                // shipped; fall back to the shader folder, which can only ever be too narrow.
                if (string.IsNullOrEmpty(root) || root.IndexOf('/') < 0)
                    root = ShippingRoot;

                return _packageRoot = root;
            }
        }

        static string _shippingRoot;
        static string _packageRoot;
        static List<ModularShader> _shippedShaders;

        /// <summary>
        /// Drops the cached asset lookups. Call after anything that could add or remove shader
        /// assets; the window does it on every refresh.
        /// </summary>
        public static void InvalidateCaches()
        {
            _shippingRoot = null;
            _packageRoot = null;
            _shippedShaders = null;
        }

        /// <summary>Deepest folder both paths sit under, or null if they share nothing.</summary>
        static string DeepestSharedFolder(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
                return null;

            var left = a.Split('/');
            var right = b.Split('/');

            int shared = 0;
            while (shared < left.Length && shared < right.Length &&
                   string.Equals(left[shared], right[shared], StringComparison.OrdinalIgnoreCase))
                shared++;

            return shared == 0 ? null : string.Join("/", left.Take(shared));
        }

        /// <summary>The folder this script lives in, used to work out which package it came from.</summary>
        static string ThisScriptFolder()
        {
            string path = AssetDatabase.FindAssets($"{nameof(PoiUserModules)} t:MonoScript")
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(x => Path.GetFileNameWithoutExtension(x) == nameof(PoiUserModules));

            return string.IsNullOrEmpty(path) ? null : Path.GetDirectoryName(path).Replace('\\', '/');
        }

        static bool IsShipped(string assetPath) =>
            assetPath.Replace('\\', '/').StartsWith(PackageRoot + "/", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// The Poiyomi modular shaders that user modules get folded into. Cached, since discovery
        /// and drift checks both walk this list and the window refreshes on every focus.
        /// </summary>
        public static List<ModularShader> GetShippedShaders()
        {
            // Unity can invalidate the loaded references out from under us on reimport.
            if (_shippedShaders != null && _shippedShaders.All(x => x != null))
                return _shippedShaders;

            return _shippedShaders = AssetDatabase.FindAssets("t:ModularShader")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(IsShipped)
                .Select(AssetDatabase.LoadAssetAtPath<ModularShader>)
                .Where(x => x != null)
                .ToList();
        }

        /// <summary>
        /// Every ShaderModule in the project that isn't part of Poiyomi itself, with enabled state
        /// and validation applied. Modules nested inside a discovered ModuleCollection are hidden so
        /// a collection shows up as one installable thing rather than a dozen.
        /// </summary>
        public static List<Entry> Discover()
        {
            var candidates = AssetDatabase.FindAssets("t:ShaderModule")
                .Select(guid => new { guid, path = AssetDatabase.GUIDToAssetPath(guid) })
                .Where(x => !string.IsNullOrEmpty(x.path) && !IsShipped(x.path))
                .Select(x => new { x.guid, x.path, module = AssetDatabase.LoadAssetAtPath<ShaderModule>(x.path) })
                .Where(x => x.module != null)
                .ToList();

            // Anything reachable from a user ModuleCollection is presented as part of that collection.
            var nested = new HashSet<ShaderModule>();
            foreach (var collection in candidates.Select(x => x.module).OfType<ModuleCollection>())
            {
                if (collection.Modules == null) continue;
                foreach (var child in collection.Modules.Where(x => x != null))
                    nested.Add(child);
            }

            var entries = candidates
                .Where(x => !nested.Contains(x.module))
                .Select(x => new Entry
                {
                    Module = x.module,
                    Guid = x.guid,
                    AssetPath = x.path,
                    Enabled = IsEnabled(x.guid)
                })
                .OrderBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            Validate(entries);
            return entries;
        }

        /// <summary>
        /// Flags modules that can't be installed: unmet dependencies, or a conflict with Poiyomi's
        /// own modules or another enabled one. Checked against a reference shader rather than all
        /// of them, since the base module set barely differs between variants.
        /// </summary>
        static void Validate(List<Entry> entries)
        {
            var reference = GetShippedShaders()
                .OrderByDescending(x => x.BaseModules?.Count ?? 0)
                .FirstOrDefault();
            if (reference == null) return;

            var baseModules = ShaderGenerator.FindAllModules(reference);
            var enabled = entries.Where(x => x.Enabled).Select(x => x.Module).ToList();

            foreach (var entry in entries)
            {
                var set = new List<ShaderModule>(baseModules);
                set.AddRange(enabled.Where(x => x != entry.Module));
                set.Add(entry.Module);

                // CheckShaderIssues reports on the whole set. Only surface problems naming this
                // module, so pre-existing quirks in Poiyomi's own modules don't get blamed on it.
                string id = entry.Module.Id ?? "";
                var issue = ShaderGenerator.CheckShaderIssues(set)
                    .FirstOrDefault(x => x.Contains($"({id})"));

                entry.Error = issue;
            }
        }

        public static List<ShaderModule> GetEnabledModules(List<Entry> entries = null)
        {
            entries = entries ?? Discover();
            return entries.Where(x => x.Enabled && x.IsUsable).Select(x => x.Module).ToList();
        }

        // ------------------------------------------------------------------
        // Applying / drift detection
        // ------------------------------------------------------------------

        /// <summary>True if the project is currently rendering through URP.</summary>
        public static bool IsProjectUsingURP()
        {
            var pipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            return pipeline != null && pipeline.GetType().Name.IndexOf("Universal", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Narrows a shader list to one render pipeline. Only the pipeline in use is worth
        /// rebuilding, and skipping the other roughly halves the build.
        /// </summary>
        public static List<ModularShader> FilterByPipeline(List<ModularShader> shaders, bool urp)
        {
            return shaders
                .Where(x => ((x.Name ?? "").IndexOf("URP", StringComparison.OrdinalIgnoreCase) >= 0) == urp)
                .ToList();
        }

        /// <summary>
        /// The shaders that get rebuilt: the shipped shaders for the pipeline in use.
        /// </summary>
        public static List<ModularShader> GetTargetShaders() =>
            FilterByPipeline(GetShippedShaders(), IsProjectUsingURP());

        /// <summary>
        /// Target shaders whose AdditionalModules don't match the enabled set. Non-empty right after
        /// a module is toggled, and again after a Poiyomi update replaces the shader assets. Shaders
        /// outside the active pipeline are left alone, so switching pipeline later reports them as
        /// out of date rather than silently shipping stale files.
        /// </summary>
        public static List<ModularShader> GetOutOfDateShaders(List<ModularShader> targets = null, List<ShaderModule> enabled = null)
        {
            var wanted = new HashSet<ShaderModule>(enabled ?? GetEnabledModules());

            return (targets ?? GetTargetShaders())
                .Where(shader =>
                {
                    var applied = new HashSet<ShaderModule>((shader.AdditionalModules ?? new List<ShaderModule>()).Where(x => x != null));
                    return !applied.SetEquals(wanted);
                })
                .ToList();
        }

        /// <summary>
        /// Writes the enabled modules into each target shader's AdditionalModules. The shaders still
        /// need regenerating afterwards for the change to reach the .shader files.
        /// </summary>
        public static void ApplyToShaderAssets(List<ModularShader> targets = null, List<ShaderModule> enabled = null)
        {
            enabled = enabled ?? GetEnabledModules();

            foreach (var shader in targets ?? GetTargetShaders())
            {
                var applied = new HashSet<ShaderModule>((shader.AdditionalModules ?? new List<ShaderModule>()).Where(x => x != null));
                if (applied.SetEquals(enabled))
                    continue;

                shader.AdditionalModules = new List<ShaderModule>(enabled);
                EditorUtility.SetDirty(shader);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
