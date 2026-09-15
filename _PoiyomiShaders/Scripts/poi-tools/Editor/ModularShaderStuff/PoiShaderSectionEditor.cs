using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Poiyomi.ModularShaderSystem;
using Thry;
using Thry.ThryEditor;
using UnityEditor;
using UnityEngine;

namespace Poi.Tools
{
    [InitializeOnLoad]
    public static class PoiShaderSectionEditor
    {
        [Serializable]
        private class Draft
        {
            public bool editing;
            public string[] disabled = Array.Empty<string>();
            public string[] baseline = Array.Empty<string>();
        }

        private class Session
        {
            public ModularShader owner;
            public string key;
            public Draft draft;
            public HashSet<string> disabled;
            public bool busy;
            public string error;
        }

        private static readonly Dictionary<Shader, ModularShader> Owners = new Dictionary<Shader, ModularShader>();
        private static readonly Dictionary<ModularShader, Session> Sessions = new Dictionary<ModularShader, Session>();
        private static GUIStyle _toolbarButton, _toolbarApply;
        private static string _eligibilityMessage;

        // PropertyIdentifier is only initialized for synthetic Thry parts. Real shader headers
        // carry their stable name on MaterialProperty, just like normal material controls.
        private static string SectionId(ShaderPart part) => part.MaterialProperty?.name ?? part.PropertyIdentifier;

        static PoiShaderSectionEditor()
        {
            TopBarButtons.Register(Icon, "Edit Shader Sections", BeginEditing);
            SectionEditing.DrawToolbar += DrawToolbar;
            SectionEditing.IsEditingPart = IsEditingPart;
            SectionEditing.DrawHeaderToggle = DrawHeaderToggle;
            SectionEditing.IsExcluded = IsExcluded;
            SectionEditing.HidePart = HidePart;
            EditorApplication.projectChanged += () => Owners.Clear();
        }

        private static GUIStyle Icon()
        {
            return ToolbarIcons.Edit;
        }

        public static ModularShader FindOwner(Shader shader)
        {
            if (shader == null) return null;
            if (Owners.TryGetValue(shader, out var cached)) return cached;
            var candidates = AssetDatabase.FindAssets("t:ModularShader").Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ModularShader>).Where(s => s != null).ToArray();
            var exact = candidates.Where(s => s.LastGeneratedShaders != null && s.LastGeneratedShaders.Contains(shader)).ToArray();
            if (exact.Length == 0) exact = candidates.Where(s => s.ShaderPath == shader.name).ToArray();
            // Older generated shaders often have stale LastGeneratedShaders GUIDs. A unique exact
            // ShaderPath is an explicit fallback; never choose arbitrarily among multiple owners.
            return Owners[shader] = exact.Length == 1 ? exact[0] : null;
        }

        private static Session GetSession(ShaderEditor editor)
        {
            if (editor == null || editor.IsCrossEditor || editor.Shader == null) return null;
            var owner = FindOwner(editor.Shader);
            if (owner == null) return null;
            if (!Sessions.TryGetValue(owner, out var session))
            {
                string key = "Poi.ShaderSections." + AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(owner));
                string json = SessionState.GetString(key, "");
                Draft draft = string.IsNullOrEmpty(json) ? new Draft() : JsonUtility.FromJson<Draft>(json) ?? new Draft();
                session = new Session { owner = owner, key = key, draft = draft,
                    disabled = new HashSet<string>(draft.disabled ?? Array.Empty<string>()) };
                Sessions.Add(owner, session);
            }
            return session;
        }

        private static string Eligibility(ShaderEditor editor)
        {
            if (editor == null) return "Select a material to edit its shader sections.";
            if (editor.IsLockedMaterial) return "Unlock this material to edit the shared source shader.";
            if (editor.Materials == null || editor.Materials.Any(m => m == null || m.shader != editor.Shader))
                return "Select materials using the same shader to edit its sections.";
            if (editor.IsPresetEditor || editor.IsCrossEditor) return "Open the normal material inspector to edit shader sections.";
            if (FindOwner(editor.Shader) == null) return "This shader does not have a unique Modular Shader source asset.";
            return null;
        }

        private static void BeginEditing()
        {
            var editor = ShaderEditor.Active;
            var session = GetSession(editor);
            if (session != null && session.draft.editing)
            {
                CancelDraft(session, editor);
                return;
            }
            _eligibilityMessage = Eligibility(editor);
            if (_eligibilityMessage != null) { editor?.Editor?.Repaint(); return; }
            if (!session.draft.editing)
            {
                session.disabled = new HashSet<string>(session.owner.DisabledSections ?? new List<string>());
                session.draft.baseline = session.disabled.OrderBy(x => x).ToArray();
                session.draft.editing = true;
                session.error = null;
                SaveDraft(session);
            }
            editor.Editor.Repaint();
        }

        private static void CancelDraft(Session session, ShaderEditor editor)
        {
            if (session.busy) return;
            session.draft.editing = false;
            session.error = null;
            SaveDraft(session);
            editor.Editor.Repaint();
        }

        private static void SaveDraft(Session session)
        {
            session.draft.disabled = session.disabled.OrderBy(x => x).ToArray();
            SessionState.SetString(session.key, JsonUtility.ToJson(session.draft));
        }

        private static void DrawToolbar(ShaderEditor editor)
        {
            if (_eligibilityMessage != null)
            {
                EditorGUILayout.HelpBox(_eligibilityMessage, MessageType.Info);
                if (GUILayout.Button("Dismiss")) _eligibilityMessage = null;
            }
            var session = GetSession(editor);
            if (session == null || !session.draft.editing) return;
            string eligibility = Eligibility(editor);
            bool stale = !new HashSet<string>(session.draft.baseline ?? Array.Empty<string>()).SetEquals(session.owner.DisabledSections ?? new List<string>());
            if (_toolbarButton == null)
            {
                _toolbarButton = new GUIStyle(GUIStyle.none) { alignment = TextAnchor.MiddleCenter, fontSize = 11 };
                _toolbarButton.normal.textColor = Color.white;
                _toolbarButton.hover.textColor = Color.white;
                _toolbarButton.active.textColor = Color.white;
                _toolbarButton.focused.textColor = Color.white;
                _toolbarApply = new GUIStyle(_toolbarButton) { fontStyle = FontStyle.Bold };
            }

            // Match Thry's other full-width rows, including Unity's inspector indent correction.
            Rect row = RectifiedLayout.GetPaddedRect(26);
            const float padding = 2;
            float width = (row.width - padding * 5) / 4;
            Rect apply = new Rect(row.x + padding, row.y + padding, width, 22);
            Rect cancel = new Rect(apply.xMax + padding, apply.y, width, 22);
            Rect enableAll = new Rect(cancel.xMax + padding, apply.y, width, 22);
            Rect disableAll = new Rect(enableAll.xMax + padding, apply.y, width, 22);
            using (new EditorGUI.DisabledScope(session.busy || stale || eligibility != null))
                if (ToolbarButton(apply, new GUIContent(session.busy ? "Building…" : "Apply", "Rebuild the shared shader for all materials using it."), new Color(0.22f, 0.45f, 0.31f), _toolbarApply))
                {
                    session.busy = true;
                    string path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(editor.Shader));
                    EditorApplication.delayCall += () => ApplyDraft(session, path);
                }
            using (new EditorGUI.DisabledScope(session.busy))
            {
                if (ToolbarButton(cancel, new GUIContent("Cancel", "Discard these section changes."), new Color(0.32f, 0.32f, 0.32f), _toolbarButton))
                    CancelDraft(session, editor);
                if (ToolbarButton(enableAll, new GUIContent("Enable All", "Include all shader sections."), new Color(0.22f, 0.37f, 0.52f), _toolbarButton))
                {
                    session.disabled.Clear();
                    SaveDraft(session);
                }
                if (ToolbarButton(disableAll, new GUIContent("Disable All", "Exclude all shader sections."), new Color(0.49f, 0.27f, 0.29f), _toolbarButton))
                {
                    foreach (var group in editor.ShaderParts.OfType<ShaderGroup>())
                    {
                        string id = SectionId(group);
                        if (ShaderSectionFilter.IsSection(id)) session.disabled.Add(id);
                    }
                    SaveDraft(session);
                }
            }
            if (eligibility != null) EditorGUILayout.HelpBox(eligibility, MessageType.Info);
            if (stale) EditorGUILayout.HelpBox("The applied sections changed outside this draft. Cancel and reopen Edit Shader Sections to load them.", MessageType.Warning);
            if (!string.IsNullOrEmpty(session.error)) EditorGUILayout.HelpBox(session.error, MessageType.Error);
        }

        private static bool ToolbarButton(Rect rect, GUIContent content, Color fill, GUIStyle style)
        {
            if (Event.current.type == EventType.Repaint)
            {
                if (GUI.enabled && rect.Contains(Event.current.mousePosition)) fill *= 1.15f;
                fill.a = GUI.enabled ? 1 : 0.4f;
                GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, fill, Vector4.zero, 3);
            }
            if (GUI.enabled) EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            return GUI.Button(rect, content, style);
        }

        private static bool IsEditingPart(ShaderPart part)
        {
            if (!(part is ShaderGroup) || !ShaderSectionFilter.IsSection(SectionId(part))) return false;
            var session = GetSession(part.MyShaderUI);
            return session != null && session.draft.editing;
        }

        private static bool IsExcluded(ShaderGroup group)
        {
            if (!IsEditingPart(group)) return false;
            var session = GetSession(group.MyShaderUI);
            return DescendantIds(group).All(session.disabled.Contains);
        }

        private static void DrawHeaderToggle(ShaderGroup group, Rect toggle)
        {
            if (!IsEditingPart(group)) return;
            var session = GetSession(group.MyShaderUI);
            var ids = DescendantIds(group).ToArray();
            int enabled = ids.Count(x => !session.disabled.Contains(x));
            bool oldChanged = GUI.changed;
            bool oldEnabled = GUI.enabled;
            bool oldMixed = EditorGUI.showMixedValue;
            int oldIndent = EditorGUI.indentLevel;
            try
            {
                // Inclusion remains editable even when the material's feature toggle disables its controls.
                GUI.enabled = !session.busy;
                EditorGUI.indentLevel = 0;
                EditorGUI.showMixedValue = enabled > 0 && enabled < ids.Length;
                EditorGUI.BeginChangeCheck();
                bool next = SlidingToggle.Draw(toggle, enabled > 0, EditorGUI.showMixedValue,
                    new GUIContent("", "Include this section in the shared shader: " + group.Content.text));
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (string child in ids)
                        if (next) session.disabled.Remove(child); else session.disabled.Add(child);
                    if (next)
                        for (ShaderPart parent = group.Parent; parent != null; parent = parent.Parent)
                            if (SectionId(parent) != null) session.disabled.Remove(SectionId(parent));
                    SaveDraft(session);
                    ShaderEditor.Input.PowerUse();
                }
            }
            finally
            {
                EditorGUI.showMixedValue = oldMixed;
                EditorGUI.indentLevel = oldIndent;
                GUI.enabled = oldEnabled;
                GUI.changed = oldChanged;
            }
        }

        private static IEnumerable<string> DescendantIds(ShaderGroup group)
        {
            string id = SectionId(group);
            if (ShaderSectionFilter.IsSection(id)) yield return id;
            foreach (var child in group.Children.OfType<ShaderGroup>())
                foreach (string childId in DescendantIds(child)) yield return childId;
        }

        private static bool HidePart(ShaderPart part)
        {
            string id = SectionId(part);
            if (!(part is ShaderGroup) || !ShaderSectionFilter.IsSection(id)) return false;
            var session = GetSession(part.MyShaderUI);
            return session != null && !session.draft.editing && session.owner.DisabledSections != null &&
                session.owner.DisabledSections.Contains(id);
        }

        private static void ApplyDraft(Session session, string directory)
        {
            try
            {
                if (!new HashSet<string>(session.draft.baseline).SetEquals(session.owner.DisabledSections ?? new List<string>()))
                    throw new InvalidOperationException("Applied sections changed while this draft was open. Cancel and reopen edit mode.");
                ApplySelection(session.owner, directory, session.disabled);
                session.draft.editing = false;
                session.error = null;
            }
            catch (Exception exception)
            {
                session.error = exception.GetBaseException().Message;
                Debug.LogError("Shader section generation failed: " + exception.GetBaseException().Message);
            }
            finally
            {
                session.busy = false;
                SaveDraft(session);
                ShaderEditor.ReloadActive();
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
        }

        /// <summary>Shared by the inspector and integration tests. Does not modify material values.</summary>
        public static void ApplySelection(ModularShader owner, string directory, IEnumerable<string> disabled)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
                throw new DirectoryNotFoundException("The generated shader folder could not be found.");
            var previousSelection = owner.DisabledSections;
            var previousShaders = owner.LastGeneratedShaders;
            var backups = new Dictionary<string, byte[]>();
            var attributes = new Dictionary<string, FileAttributes>();
            bool wasDirty = EditorUtility.IsDirty(owner);
            try
            {
                owner.DisabledSections = disabled.Distinct().OrderBy(x => x).ToList();
                EditorUtility.DisplayProgressBar("Shader Sections", "Generating selected sections", 0.1f);
                var preparation = ShaderGenerator.PrepareShaderContexts(directory, owner);
                preparation.contexts.AsParallel().ForAll(c => c.GenerateShader(preparation.duplicates));
                foreach (var context in preparation.contexts)
                {
                    string path = Path.Combine(context.FilePath, context.VariantFileName);
                    backups[path] = File.Exists(path) ? File.ReadAllBytes(path) : null;
                    backups[path + ".meta"] = File.Exists(path + ".meta") ? File.ReadAllBytes(path + ".meta") : null;
                    if (File.Exists(path)) attributes[path] = File.GetAttributes(path);
                }
                EditorUtility.DisplayProgressBar("Shader Sections", "Importing generated shaders", 0.65f);
                ShaderGenerator.WriteShaderFiles(preparation.contexts);
                foreach (var context in preparation.contexts)
                {
                    string path = (context.FilePath + "/" + context.VariantFileName).Replace('\\', '/');
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                    var generated = AssetDatabase.LoadAssetAtPath<Shader>(path);
                    if (generated == null) throw new InvalidOperationException("Could not import " + path);
                    // Import alone may not compile the variants being displayed. Probe defaults
                    // and loaded materials' keyword combinations without changing those materials.
                    var probe = new Material(generated);
                    try
                    {
                        MaterialEditor.ApplyMaterialPropertyDrawers(probe);
                        var profiles = Resources.FindObjectsOfTypeAll<Material>()
                            .Where(m => m.shader == generated).Select(m => m.shaderKeywords.OrderBy(k => k).ToArray())
                            .GroupBy(k => string.Join("\n", k)).Select(g => g.First()).ToArray();
                        EditorUtility.DisplayProgressBar("Shader Sections", "Compiling " + generated.name, 0.8f);
                        foreach (var keywords in profiles)
                        {
                            probe.shaderKeywords = keywords;
                            for (int pass = 0; pass < probe.passCount; pass++) ShaderUtil.CompilePass(probe, pass, true);
                            if (ShaderUtil.GetShaderMessages(generated).Any(m => m.severity.ToString() == "Error")) break;
                        }
                    }
                    finally { UnityEngine.Object.DestroyImmediate(probe); }
                    var errors = ShaderUtil.GetShaderMessages(generated).Where(m => m.severity.ToString() == "Error").Take(6).ToArray();
                    if (errors.Length > 0)
                        throw new InvalidOperationException("The selected sections produced shader errors. Previous shader restored.\n" +
                            string.Join("\n", errors.Select(m => m.message + " (line " + m.line + ")")));
                    owner.LastGeneratedShaders.Add(generated);
                }
                ShaderGenerator.ApplyDefaultTextures(preparation.contexts);
                EditorUtility.SetDirty(owner);
                AssetDatabase.SaveAssetIfDirty(owner);
                Owners.Clear();
            }
            catch
            {
                owner.DisabledSections = previousSelection;
                owner.LastGeneratedShaders = previousShaders;
                foreach (var backup in backups)
                {
                    if (File.Exists(backup.Key)) File.SetAttributes(backup.Key, FileAttributes.Normal);
                    if (backup.Value == null) { if (File.Exists(backup.Key)) File.Delete(backup.Key); }
                    else File.WriteAllBytes(backup.Key, backup.Value);
                    if (attributes.TryGetValue(backup.Key, out var originalAttributes) && File.Exists(backup.Key))
                        File.SetAttributes(backup.Key, originalAttributes);
                }
                foreach (var path in backups.Keys.Where(p => p.EndsWith(".shader") && File.Exists(p)))
                    AssetDatabase.ImportAsset(path.Replace('\\', '/'), ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                if (!wasDirty) EditorUtility.ClearDirty(owner);
                throw;
            }
            finally { EditorUtility.ClearProgressBar(); }
        }
    }
}
