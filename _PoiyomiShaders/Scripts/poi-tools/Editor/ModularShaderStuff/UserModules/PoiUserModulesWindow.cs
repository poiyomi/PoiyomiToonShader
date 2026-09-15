using System.Collections.Generic;
using System.Linq;
using Poiyomi.ModularShaderSystem;
using UnityEditor;
using UnityEngine;

namespace Poi.Tools
{
    /// <summary>
    /// The whole user-facing module workflow: import a module anywhere in the project, tick it,
    /// hit Apply. Materials keep using the same shader, so the module's settings just appear in the
    /// Poiyomi inspector afterwards.
    /// </summary>
    public class PoiUserModulesWindow : EditorWindow
    {
        List<PoiUserModules.Entry> _entries;
        List<ModularShader> _outOfDate;
        Vector2 _scroll;

        // Toggling rebuilds the entry list, which can't happen mid-layout. Deferred to the end of OnGUI.
        PoiUserModules.Entry _pendingToggle;
        bool _pendingToggleValue;

        [MenuItem("Poi/Installed Modules", priority = 20)]
        public static PoiUserModulesWindow Open()
        {
            var window = GetWindow<PoiUserModulesWindow>();
            window.titleContent = new GUIContent("Installed Modules");
            window.minSize = new Vector2(380, 260);
            window.Refresh();
            window.Show();
            return window;
        }

        void OnEnable() => Refresh();

        void OnFocus() => Refresh();

        public void Refresh()
        {
            PoiUserModules.InvalidateCaches();
            _entries = PoiUserModules.Discover();
            _outOfDate = PoiUserModules.GetOutOfDateShaders(enabled: PoiUserModules.GetEnabledModules(_entries));
            Repaint();
        }

        void OnGUI()
        {
            if (_entries == null)
                Refresh();

            DrawHeader();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            if (_entries.Count == 0)
                DrawEmptyState();
            else
                foreach (var entry in _entries)
                    DrawEntry(entry);
            EditorGUILayout.EndScrollView();

            DrawApplyBar();

            if (_pendingToggle != null)
            {
                PoiUserModules.SetEnabled(_pendingToggle.Guid, _pendingToggleValue);
                _pendingToggle = null;
                Refresh();
            }
        }

        void DrawHeader()
        {
            GUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Installed Modules", EditorStyles.boldLabel);
                if (GUILayout.Button("New Module", GUILayout.Width(90)))
                    PoiModuleCreatorWindow.Open();
                if (GUILayout.Button("Refresh", GUILayout.Width(70)))
                    Refresh();
            }
            EditorGUILayout.LabelField(
                "Modules you import anywhere in your project show up here.",
                EditorStyles.miniLabel);
            GUILayout.Space(2);
        }

        void DrawEmptyState()
        {
            GUILayout.Space(20);
            EditorGUILayout.HelpBox(
                "No modules installed.\n\n" +
                "Import a module package anywhere in your project and it will appear in this list. " +
                "Tick it, hit Apply, and its settings show up in the Poiyomi material inspector.",
                MessageType.Info);

            GUILayout.Space(6);
            if (GUILayout.Button("Create Your Own Module"))
                PoiModuleCreatorWindow.Open();
        }

        void DrawEntry(PoiUserModules.Entry entry)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(!entry.IsUsable))
                    {
                        EditorGUI.BeginChangeCheck();
                        bool enabled = EditorGUILayout.ToggleLeft(entry.DisplayName, entry.Enabled, EditorStyles.boldLabel);
                        if (EditorGUI.EndChangeCheck())
                        {
                            entry.Enabled = enabled;
                            _pendingToggle = entry;
                            _pendingToggleValue = enabled;
                        }
                    }

                    GUILayout.FlexibleSpace();
                    if (!string.IsNullOrWhiteSpace(entry.Module.Version))
                        EditorGUILayout.LabelField(entry.Module.Version, EditorStyles.miniLabel, GUILayout.Width(50));
                    if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(55)))
                        EditorGUIUtility.PingObject(entry.Module);
                }

                if (!string.IsNullOrWhiteSpace(entry.Module.Author))
                    EditorGUILayout.LabelField($"by {entry.Module.Author}", EditorStyles.miniLabel);

                if (!string.IsNullOrWhiteSpace(entry.Module.Description))
                    EditorGUILayout.LabelField(entry.Module.Description, EditorStyles.wordWrappedMiniLabel);

                if (!entry.IsUsable)
                    EditorGUILayout.HelpBox(entry.Error, MessageType.Error);
            }
        }

        void DrawApplyBar()
        {
            GUILayout.Space(2);

            int pending = _outOfDate?.Count ?? 0;
            if (pending > 0)
            {
                EditorGUILayout.HelpBox(
                    $"Changes not applied yet. {pending} shader{(pending == 1 ? "" : "s")} need rebuilding.",
                    MessageType.Warning);
            }
            else if (_entries.Any(x => x.Enabled))
            {
                EditorGUILayout.HelpBox("All modules are applied and up to date.", MessageType.Info);
            }

            using (new EditorGUI.DisabledScope(pending == 0))
            {
                if (GUILayout.Button("Apply & Rebuild Shaders", GUILayout.Height(30)))
                    ApplyAndRebuild();
            }

            EditorGUILayout.LabelField(
                PoiUserModules.IsProjectUsingURP()
                    ? "Rebuilding URP shaders (project is using URP)."
                    : "Rebuilding Built-in shaders.",
                EditorStyles.miniLabel);
            GUILayout.Space(4);
        }

        void ApplyAndRebuild()
        {
            var targets = PoiUserModules.GetTargetShaders();
            if (targets.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "No Shaders Found",
                    "Couldn't find any Poiyomi modular shaders to rebuild. Make sure the shader package is fully imported.",
                    "Ok");
                return;
            }

            var enabled = PoiUserModules.GetEnabledModules(_entries);

            bool proceed = EditorUtility.DisplayDialog(
                "Rebuild Shaders",
                $"This will rebuild {targets.Count} Poiyomi shaders with {enabled.Count} module{(enabled.Count == 1 ? "" : "s")} installed.\n\n" +
                "It can take several minutes. Your materials will keep their settings.",
                "Rebuild",
                "Cancel");
            if (!proceed) return;

            PoiUserModules.ApplyToShaderAssets(targets, enabled);
            ModularShadersGeneratorWindow.RebuildShaders(targets);
            Refresh();
        }
    }
}
