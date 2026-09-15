// Complete ultimate Material Upgrading Utility for Poiyomi Shaders.
//
// Shows a project-wide list of Poiyomi materials with their versions,
// grouped by Shader, Folder, Avatar. A one-click "Upgrade to Latest"
// per material, group, or everything in between.
//
// This script reuses Thry.ThryEditor.MaterialLockScanner for the project
// scan, prefab-owner index and grouping so the two tools stay consistent,
// and routes upgrades through the PoiyomiVersionUpgradeController pipeline.
//
// Designed by BluWizard LABS for Poiyomi Shaders.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Thry.ThryEditor;
using Thry.ThryEditor.Helpers;
using Version = System.Version;

namespace Poi.Tools.ShaderTranslator.VersionUpgrade
{
    public class MaterialUpgraderWindow : EditorWindow
    {
        [MenuItem("Poi/Material Upgrade Utility", priority = 4)]
        static void Open() => GetWindow<MaterialUpgraderWindow>(false, "Material Upgrade Utility", true);

        enum UpgradeState
        {
            NotPoiyomi,
            UpToDate,
            NeedsUpgrade,
            ShaderMissing
        }
        enum UpgradeFilter
        {
            All,
            [InspectorName("Needs Upgrade")] NeedsUpgrade,
            [InspectorName("Up To Date")] UpToDate,
            [InspectorName("Shader Missing")] ShaderMissing
        }

        #region Layout Constants

        const float ToolbarHeight = 21f, SummaryHeight = 22f, NoticeHeight = 30f, FooterHeight = 26f;
        const float SectionHeaderHeight = 24f, GroupHeaderHeight = 24f, RowHeight = 20f, RowIndent = 16f;
        const float EdgePadding = 4f, ActionWidth = 74f, BadgeWidth = 70f, ScrollbarWidth = 16f;

        #endregion

        #region State

        [SerializeField] List<MaterialLockEntry> _entries;
        [SerializeField] List<string> _expandedKeys = new List<string>();
        [SerializeField] MaterialLockGrouping _grouping = MaterialLockGrouping.PrefabAndScene;
        [SerializeField] UpgradeFilter _filter = UpgradeFilter.NeedsUpgrade;
        [SerializeField] bool _includePackages;
        [SerializeField] string _search = "";
        [SerializeField] Vector2 _scroll;
        [SerializeField] bool _hasScanned;
        [SerializeField] int _scannedVersion;

        [NonSerialized] List<MaterialLockGroup> _groups =  new List<MaterialLockGroup>();
        [NonSerialized] readonly Dictionary<Material, (UpgradeState state, string label)> _classify = new Dictionary<Material, (UpgradeState, string)>();
        [NonSerialized] HashSet<string> _expanded;
        [NonSerialized] bool _needsViewRebuild = true;
        [NonSerialized] string _summaryText = "";
        [NonSerialized] int _shownUpgradable;

        // Queued Work, drained on Update so an upgrade never runs inside OnGUI.
        [NonSerialized] List<Material> _pendingUpgrade;
        [NonSerialized] bool _pendingRescan;

        // Staleness: Bumped by the asset watcher, compared against what the last scan saw.
        static int s_databaseVersion;

        #endregion

        #region Lifecycle

        void OnEnable()
        {
            titleContent = new GUIContent("Material Upgrade Utility", EditorGUIUtility.ObjectContent(null, typeof(Material)).image);
            minSize = new Vector2(560, 260);
            LoadPreferences();
            _needsViewRebuild = true;
            if (_scannedVersion > s_databaseVersion) _scannedVersion = s_databaseVersion; // static counter resets on domain reload
            if (_entries == null) _pendingRescan = true;
        }

        void OnDisable() => MaterialLockScanner.InvalidateOwnerIndex(); // the prefab-owner index is only needed while open

        void Update()
        {
            if (_pendingUpgrade == null && !_pendingRescan && !_needsViewRebuild) return;

            List<Material> upgrade = _pendingUpgrade;
            bool rescan = _pendingRescan;
			_pendingUpgrade = null;
			_pendingRescan = false;

            if (upgrade != null && upgrade.Count > 0)
            {
                try
                {
                    PoiyomiVersionUpgradeController.UpgradeMaterials(upgrade);
                }
                catch (Exception e)
                {
                    ThryLogger.LogErr("Material Upgrader", $"Upgrade failed: {e}");
                }
                rescan = true;
            }

            if (rescan) Rescan();
            if (_needsViewRebuild) RebuildView();
            Repaint();
        }

        #endregion

        #region Model

        void Rescan()
        {
            _entries = MaterialLockScanner.Scan(_includePackages, out _);

            // MaterialLockScanner only returns optimizer-capable materials, so it misses an unlocked material whose
			// removed-version shader is gone (it's on the error shader). Add those back via the legacy detector.

            var known = new HashSet<Material>(_entries.Where(e => e != null && e.Material != null).Select(e => e.Material));
			string[] onlyAssets = _includePackages ? null : new[] { "Assets" };

			foreach (string guid in (onlyAssets == null ? AssetDatabase.FindAssets("t:Material") : AssetDatabase.FindAssets("t:Material", onlyAssets)))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
				Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
				if (m == null || known.Contains(m)) continue;
				if (LegacyMaterialDetector.IsLegacyShaderMissing(m)) _entries.Add(new MaterialLockEntry { Material = m, AssetPath = path, Shader = null, VariantRoot = m.GetRoot() });
            }

            _classify.Clear();
            _hasScanned = true;
            _scannedVersion = s_databaseVersion;
            _needsViewRebuild = true;
        }

        (UpgradeState state, string label) Classify(Material m)
        {
            if (m == null) return (UpgradeState.NotPoiyomi, "");
			if (_classify.TryGetValue(m, out var cached)) return cached;

            (UpgradeState, string) result;

            if (LegacyMaterialDetector.IsLegacyShaderMissing(m))
            {
                result = (UpgradeState.ShaderMissing, "missing");
            }
            else if (PoiyomiVersionDetector.IsPoiyomiShader(m))
            {
                if (PoiyomiVersionDetector.TryGetVersion(m, out Version v))
                {
                    result = (v < PoiyomiVersionDetector.LatestVersion ? UpgradeState.NeedsUpgrade : UpgradeState.UpToDate, $"{v.Major}.{v.Minor}");
                }
                else if (ShaderOptimizer.IsMaterialLocked(m) || m.shader.IsBroken())
                {
                    result = (UpgradeState.NeedsUpgrade, "?");   // Poiyomi, version lost with the shader - offer the upgrade
                }
                else
                {
                    result = (UpgradeState.NotPoiyomi, "");
                }
            }
            else if (LegacyMaterialDetector.NeedsLegacyUpgrade(m))
            {
                result = (UpgradeState.NeedsUpgrade, "legacy");
            }
            else
            {
                result = (UpgradeState.NotPoiyomi, "");
            }

            _classify[m] = result;
            return result;
        }

        bool NeedsUpgrade(Material m)
        {
            UpgradeState s = Classify(m).state;
            return s == UpgradeState.NeedsUpgrade || s == UpgradeState.ShaderMissing;
        }

        bool PassesFilter(Material m)
        {
            UpgradeState s = Classify(m).state;
            if (s == UpgradeState.NotPoiyomi) return false; // only Poiyomi materials ever appear
            switch (_filter)
            {
                case UpgradeFilter.NeedsUpgrade: return s == UpgradeState.NeedsUpgrade || s == UpgradeState.ShaderMissing;
                case UpgradeFilter.UpToDate: return s == UpgradeState.UpToDate;
                case UpgradeFilter.ShaderMissing: return s == UpgradeState.ShaderMissing;
                default: return true;
            }
        }

        void RebuildView()
        {
            _needsViewRebuild = false;
			if (_entries == null) { _groups = new List<MaterialLockGroup>(); _summaryText = ""; _shownUpgradable = 0; return; }

            var visible = _entries.Where(e => e != null && e.Material != null).Where(e => PassesFilter(e.Material)).Where(e => MaterialLockScanner.MatchesSearch(e, _search)).ToList();

            if (_filter == UpgradeFilter.All)
            {
                _groups = BuildSection(visible.Where(e => NeedsUpgrade(e.Material)).ToList(), "Needs Upgrade");
                _groups.AddRange(BuildSection(visible.Where(e => !NeedsUpgrade(e.Material)).ToList(), "Up To Date"));
            }
            else
            {
                _groups = MaterialLockScanner.Group(visible, _grouping, _includePackages);
            }

            var shown = _groups.SelectMany(g => g.Entries).Select(e => e.Material).Distinct().ToList();
			_shownUpgradable = shown.Count(NeedsUpgrade);

            int poi = _entries.Count(e => e != null && e.Material != null && Classify(e.Material).state != UpgradeState.NotPoiyomi);
			int needs = _entries.Count(e => e != null && e.Material != null && NeedsUpgrade(e.Material));
			_summaryText = $"{poi} Poiyomi material{(poi == 1 ? "" : "s")}  ·  {needs} need upgrade";
			if (shown.Count != poi) _summaryText += $"     (showing {shown.Count})";
        }

        // Groups one type-bucket and tags every group with its section band. Keys are section-prefixed so the same
        // shader/folder/prefab appearing in two sections doesn't share a foldout.
        List<MaterialLockGroup> BuildSection(List<MaterialLockEntry> entries, string section)
        {
            List<MaterialLockGroup> groups = MaterialLockScanner.Group(entries, _grouping, _includePackages);
            int count = entries.Select(e => e.Material).Distinct().Count();
            foreach (MaterialLockGroup g in groups)
            {
                g.Section = section;
                g.SectionCount = count;
                g.Key = section + "/" + g.Key;
            }
            return groups;
        }

        void Enqueue(IEnumerable<MaterialLockEntry> entries)
        {
            var targets = entries.Where(e => e != null && e.Material != null && NeedsUpgrade(e.Material)).Select(e => e.Target ?? e.Material).Distinct().ToList();
			if (targets.Count > 0) _pendingUpgrade = targets;
        }

        // Extra line appended to an upgrade confirmation when the batch contains a Grab Pass material - its blend and
        // refraction settings carry over, but base-color/alpha handling and several effects can't be mapped to 10.0.
        string GrabPassNotice(IEnumerable<Material> materials)
        {
            bool any = materials.Any(m => m != null && NeedsUpgrade(m) && PoiyomiVersionDetector.UsesGrabPass(m));
            return any
                ? "\n\nOne of these materials use Grab Pass. Its Blend, Refraction and Chromatic Aberration settings carry over, but the Base Color and Alpha handling changed (\"Use Material Alpha\" is now driven by the Alpha Mask) and some options are non-transferrable (Hue Shift Replace, multi-directional Blur, the per-channel Blend Mask) - please re-check those materials afterwards."
                : "";
        }

        #endregion

        #region Expansion

        HashSet<string> Expanded => _expanded ?? (_expanded = new HashSet<string>(_expandedKeys ?? new List<string>()));
		bool IsExpanded(string key) => Expanded.Contains(key);
        void SetExpanded(string key, bool value)
        {
            if (value) Expanded.Add(key); else Expanded.Remove(key);
			_expandedKeys = Expanded.ToList();
        }

        #endregion

        #region GUI

        void OnGUI()
        {
            float y = DrawToolbar(0);
			y = DrawSummary(y);
			y = DrawNotice(y);
			DrawList(new Rect(0, y, position.width, Mathf.Max(0, position.height - y - FooterHeight)));
			DrawFooter(new Rect(0, position.height - FooterHeight, position.width, FooterHeight));
        }

        float DrawToolbar(float y)
        {
            var bar = new Rect(0, y, position.width, ToolbarHeight);
			GUI.Label(bar, GUIContent.none, EditorStyles.toolbar);
			float x = EdgePadding;

			var refresh = new Rect(x, y, 70, ToolbarHeight);
			if (GUI.Button(refresh, new GUIContent(" Refresh", EditorGUIUtility.IconContent("Refresh").image, "Re-scan the project"), EditorStyles.toolbarButton)) _pendingRescan = true;
			x += refresh.width + 2;

            var grouping = new Rect(x, y, 132, ToolbarHeight);
			EditorGUI.BeginChangeCheck();
			var newGrouping = (MaterialLockGrouping)EditorGUI.EnumPopup(grouping, _grouping, EditorStyles.toolbarPopup);
			if (EditorGUI.EndChangeCheck())
            {
                _grouping = newGrouping;
                _needsViewRebuild = true;
                SavePreferences();
            }
			x += grouping.width + 2;

            var filter = new Rect(x, y, 130, ToolbarHeight);
			EditorGUI.BeginChangeCheck();
			var newFilter = (UpgradeFilter)EditorGUI.EnumPopup(filter, _filter, EditorStyles.toolbarPopup);
			if (EditorGUI.EndChangeCheck())
            {
                _filter = newFilter;
                _needsViewRebuild = true;
                SavePreferences();
            }
			x += filter.width + 2;

            var packages = new Rect(position.width - EdgePadding - 78, y, 78, ToolbarHeight);
			EditorGUI.BeginChangeCheck();
			bool newPkg = GUI.Toggle(packages, _includePackages, new GUIContent("Packages", "Include materials from packages."), EditorStyles.toolbarButton);
			if (EditorGUI.EndChangeCheck())
            {
                _includePackages = newPkg;
                _pendingRescan = true;
                SavePreferences();
            }

            var search = new Rect(x, y + 2, Mathf.Max(60, packages.x - 4 - x), ToolbarHeight - 4);
			EditorGUI.BeginChangeCheck();
			string newSearch = GUI.TextField(search, _search, EditorStyles.toolbarSearchField);
			if (EditorGUI.EndChangeCheck())
            {
                _search = newSearch;
                _needsViewRebuild = true;
            }

            return y + ToolbarHeight;
        }

        float DrawSummary(float y)
        {
            var bar = new Rect(0, y, position.width, SummaryHeight);
			EditorGUI.DrawRect(bar, new Color(0, 0, 0, 0.1f));
			GUI.Label(new Rect(bar.x + EdgePadding + 2, bar.y, bar.width - EdgePadding * 2, bar.height), _hasScanned ? _summaryText : "Not scanned yet.", EditorStyles.miniLabel);
			return y + SummaryHeight;
        }

        float DrawNotice(float y)
        {
            if (!_hasScanned || _scannedVersion == s_databaseVersion) return y;
			var bar = new Rect(0, y, position.width, NoticeHeight);
			EditorGUI.DrawRect(bar, new Color(1f, 0.8f, 0.2f, 0.12f));
			if (GUI.Button(new Rect(bar.xMax - 84 - EdgePadding, bar.y + 5, 84, NoticeHeight - 10), "Re-scan", EditorStyles.miniButton)) _pendingRescan = true;
			GUI.Label(new Rect(bar.x + EdgePadding + 2, bar.y, bar.width - 100, bar.height), "Materials have changed since this scan.");
			return y + NoticeHeight;
        }

        void DrawList(Rect area)
        {
            if (!_hasScanned)
            {
                GUI.Label(area, "Scanning Project...\nThis may take a few seconds, please wait...", EditorStyles.centeredGreyMiniLabel);
                return;
            }
			if (_groups.Count == 0)
            {
                GUI.Label(area, "No materials found from the filtered results.", EditorStyles.centeredGreyMiniLabel);
                return;
            }

			// Height pass - must stay in step with the draw loop below, including where the section bands fall.
            float contentHeight = 0; string heightSection = null;
            foreach (var g in _groups)
            {
                if (StartsSection(g, heightSection)) contentHeight += SectionHeaderHeight;
                heightSection = g.Section;
                contentHeight += GroupHeaderHeight + (IsExpanded(g.Key) ? g.Entries.Count * RowHeight : 0);
            }

			var view = new Rect(0, 0, area.width - ScrollbarWidth, contentHeight);
			_scroll = GUI.BeginScrollView(area, _scroll, view);

            float y = 0;
            string section = null;
            foreach (var g in _groups)
            {
                if (StartsSection(g, section))
                {
                    DrawSectionHeader(new Rect(0, y, view.width, SectionHeaderHeight), g);
                    y += SectionHeaderHeight;
                }
                section = g.Section;
                DrawGroupHeader(new Rect(0, y, view.width, GroupHeaderHeight), g);
				y += GroupHeaderHeight;
				if (IsExpanded(g.Key))
                {
                    foreach (var e in g.Entries)
                    {
                        DrawRow(new Rect(0, y, view.width, RowHeight), e);
                        y += RowHeight;
                    }
                }
            }
            GUI.EndScrollView();
        }

        // True for the first group of each run of a section - a pure function of the list, so the height pass and
        // the draw pass always agree on where the bands go.
        static bool StartsSection(MaterialLockGroup g, string previousSection) => g.Section != null && g.Section != previousSection;

        void DrawSectionHeader(Rect r, MaterialLockGroup g)
        {
            EditorGUI.DrawRect(r, new Color(0, 0, 0, 0.15f));
            EditorGUI.DrawRect(new Rect(r.x, r.yMax - 1, r.width, 1), new Color(0, 0, 0, 0.3f)); // bottom rule reads as a divider
            GUI.Label(new Rect(r.x + EdgePadding + 2, r.y, r.width - EdgePadding * 2, r.height), $"{g.Section}  ({g.SectionCount})", EditorStyles.boldLabel);
        }

        void DrawGroupHeader(Rect r, MaterialLockGroup g)
        {
            EditorGUI.DrawRect(r, new Color(0, 0, 0, 0.06f));
			int upgradable = g.Entries.Select(e => e.Material).Distinct().Count(NeedsUpgrade);

			var fold = new Rect(r.x + 4, r.y + 3, r.width - ActionWidth - 12, r.height - 4);
			bool exp = EditorGUI.Foldout(fold, IsExpanded(g.Key), new GUIContent($"{g.DisplayName}   ({g.Entries.Count})"), true);
			if (exp != IsExpanded(g.Key)) SetExpanded(g.Key, exp);

            using (new EditorGUI.DisabledScope(upgradable == 0))
            {
                if (GUI.Button(new Rect(r.xMax - ActionWidth - 4, r.y + 3, ActionWidth, r.height - 6), $"Upgrade {upgradable}", EditorStyles.miniButton) && EditorUtility.DisplayDialog("Upgrade Group", $"You are about to upgrade the {upgradable} material{(upgradable == 1 ? "" : "s")} in \"{g.DisplayName}\" to the latest Poiyomi Shaders version. This changes the material{(upgradable == 1 ? "" : "s")} in place.\n\nTo ensure nothing is missed, it is recommended you inspect your material{(upgradable == 1 ? "" : "s")} post-upgrade to ensure they look right to you.\n\nTHIS ACTION CANNOT BE UNDONE!\n\nAre you sure you want to upgrade the material{(upgradable == 1 ? "" : "s")}?" + GrabPassNotice(g.Entries.Select(e => e.Material)),"Do it!", "Cancel")) Enqueue(g.Entries);
            }
        }

        void DrawRow(Rect r, MaterialLockEntry e)
        {
            Rect pingArea = new Rect(r.x, r.y, r.width - ActionWidth - 8, r.height);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && pingArea.Contains(Event.current.mousePosition))
			{
				EditorGUIUtility.PingObject(e.Material);
				if (Event.current.clickCount == 2) Selection.activeObject = e.Material;
				Event.current.Use();
			}

            var (state, label) = Classify(e.Material);
			GUI.Label(new Rect(r.x + RowIndent, r.y, r.width - RowIndent - ActionWidth - BadgeWidth - 12, r.height), new GUIContent(e.Name, AssetPreview.GetMiniThumbnail(e.Material)), EditorStyles.label);

            DrawBadge(new Rect(r.xMax - ActionWidth - BadgeWidth - 8, r.y + 2, BadgeWidth, r.height - 4), state, label);

            bool needs = state == UpgradeState.NeedsUpgrade || state == UpgradeState.ShaderMissing;
            using (new EditorGUI.DisabledScope(!needs))
			{
				if (GUI.Button(new Rect(r.xMax - ActionWidth - 4, r.y + 1, ActionWidth, r.height - 2), needs ? "Upgrade" : "Latest", EditorStyles.miniButton) && EditorUtility.DisplayDialog("Upgrade Material", $"You are about to upgrade \"{e.Name}\" to the latest Poiyomi Shaders version. This changes the material in place.\n\nTo ensure nothing is missed, it is recommended to inspect your material post-upgrade to ensure they look right to you.\n\nTHIS ACTION CANNOT BE UNDONE!\n\nAre you sure you want to upgrade this material?" + GrabPassNotice(new[] { e.Material }), "Do it!", "Cancel")) Enqueue(new[] { e });
			}
        }

        void DrawBadge(Rect r, UpgradeState state, string label)
		{
			Color c = state == UpgradeState.UpToDate ? new Color(0.4f, 0.7f, 0.4f) : state == UpgradeState.ShaderMissing ? new Color(0.85f, 0.4f, 0.4f) : new Color(0.85f, 0.7f, 0.3f);
			var prev = GUI.color; GUI.color = c;
			GUI.Box(r, label, EditorStyles.helpBox);
			GUI.color = prev;
		}

        void DrawFooter(Rect r)
        {
            EditorGUI.DrawRect(r, new Color(0, 0, 0, 0.1f));
			using (new EditorGUI.DisabledScope(_shownUpgradable == 0))
			{
				if (GUI.Button(new Rect(r.xMax - 160 - EdgePadding, r.y + 3, 160, r.height - 6), $"Upgrade All Shown ({_shownUpgradable})"))
				{
					if (EditorUtility.DisplayDialog("Upgrade Materials", $"You are about to upgrade {_shownUpgradable} material{(_shownUpgradable == 1 ? "" : "s")} to the latest Poiyomi Shaders version. This will change the material{(_shownUpgradable == 1 ? "" : "s")} in place.\n\nTo ensure nothing is missed, it is recommended you inspect your materials post-upgrade to ensure they look right to you.\n\nTHIS ACTION CANNOT BE UNDONE!\n\nAre you sure you want to upgrade {_shownUpgradable} material{(_shownUpgradable == 1 ? "" : "s")}?" + GrabPassNotice(_groups.SelectMany(g => g.Entries).Select(e => e.Material)), "Do it!", "Cancel")) Enqueue(_groups.SelectMany(g => g.Entries));
				}
			}
			GUI.Label(new Rect(r.x + EdgePadding + 2, r.y, r.width - 200, r.height), "Upgrades route legacy 7.3/8.x/9.x → 9.3 → 10.0. Materials are left unlocked.", EditorStyles.miniLabel);
        }

        #endregion

        #region Preferences

        const string PrefGrouping = "Poi.MaterialUpgrader.Grouping", PrefFilter = "Poi.MaterialUpgrader.Filter", PrefPackages = "Poi.MaterialUpgrader.Packages";

        void LoadPreferences()
        {
            _grouping = (MaterialLockGrouping)EditorPrefs.GetInt(PrefGrouping, (int)MaterialLockGrouping.PrefabAndScene);
			_filter = (UpgradeFilter)EditorPrefs.GetInt(PrefFilter, (int)UpgradeFilter.NeedsUpgrade);
			_includePackages = EditorPrefs.GetBool(PrefPackages, false);
        }

        void SavePreferences()
        {
            EditorPrefs.SetInt(PrefGrouping, (int)_grouping);
			EditorPrefs.SetInt(PrefFilter, (int)_filter);
			EditorPrefs.SetBool(PrefPackages, _includePackages);
        }

        #endregion

        #region Staleness

        class ChangeWatcher : AssetPostprocessor
        {
            static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
			{
				if (imported.Any(IsMaterial) || deleted.Any(IsMaterial) || moved.Any(IsMaterial)) s_databaseVersion++;
			}
			static bool IsMaterial(string p) => p.EndsWith(".mat", StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
}
