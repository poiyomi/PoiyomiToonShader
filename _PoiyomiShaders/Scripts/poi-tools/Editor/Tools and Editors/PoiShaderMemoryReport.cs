#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace Poi.Tools
{
    /// <summary>
    /// Read-only snapshot of what shaders and materials are actually costing in the editor right now.
    ///
    /// Exists because the obvious explanations for editor memory pressure - the size of a material's
    /// serialized property sheet, the number of properties a shader declares - are easy to infer from
    /// file sizes and easy to be wrong about. Compiled shader variants are the other candidate, and they
    /// do not show up in any file on disk: every feature toggle on an unlocked material mints a fresh
    /// variant of a very large pass, and Unity keeps them resident for the session.
    ///
    /// Nothing here modifies anything. Open it, hit Refresh, and copy the report.
    /// </summary>
    public class PoiShaderMemoryReport : EditorWindow
    {
        const long MB = 1024 * 1024;

        string _report = "";
        Vector2 _scroll;

        [MenuItem("Poi/Tools/Shader Memory Report")]
        static void Open()
        {
            PoiShaderMemoryReport window = GetWindow<PoiShaderMemoryReport>();
            window.titleContent = new GUIContent("Shader Memory");
            window.minSize = new Vector2(560, 400);
            window.Show();
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Measures what is resident in the editor right now. For a representative reading, open the " +
                "scene you normally author in and click through a few materials first - variants are only " +
                "compiled once something needs them.",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Refresh", GUILayout.Height(24)))
                    _report = BuildReport();

                using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_report)))
                {
                    if (GUILayout.Button("Copy to Clipboard", GUILayout.Height(24)))
                        EditorGUIUtility.systemCopyBuffer = _report;
                }
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.TextArea(_report, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        #region Report

        static string BuildReport()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== Poiyomi Shader Memory Report ===");
            sb.AppendLine("Unity " + Application.unityVersion + "   " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
            sb.AppendLine();

            Shader[] shaders = Resources.FindObjectsOfTypeAll<Shader>();
            Material[] materials = Resources.FindObjectsOfTypeAll<Material>();

            AppendShaderSection(sb, shaders);
            AppendMaterialSection(sb, materials);
            AppendKeywordSection(sb, materials);

            return sb.ToString();
        }

        static bool IsInteresting(Shader s)
        {
            if (s == null) return false;
            string n = s.name;
            return n.IndexOf("poiyomi", StringComparison.OrdinalIgnoreCase) >= 0 || n.StartsWith("Hidden/Locked/", StringComparison.OrdinalIgnoreCase);
        }

        static void AppendShaderSection(StringBuilder sb, Shader[] shaders)
        {
            sb.AppendLine("-- Resident shaders --");

            var rows = shaders.Where(s => s != null)
                .Select(s => new
                {
                    Shader = s,
                    Bytes = Profiler.GetRuntimeMemorySizeLong(s),
                    Props = s.GetPropertyCount(),
                    Variants = TryGetVariantCount(s),
                    Locked = s.name.StartsWith("Hidden/Locked/", StringComparison.OrdinalIgnoreCase)
                })
                .OrderByDescending(r => r.Bytes)
                .ToList();

            long totalAll = rows.Sum(r => r.Bytes);
            var poi = rows.Where(r => IsInteresting(r.Shader)).ToList();
            long totalPoi = poi.Sum(r => r.Bytes);

            sb.AppendLine($"   {rows.Count} shaders resident, {totalAll / (float)MB:0.0} MB total");
            sb.AppendLine($"   of which Poiyomi/locked: {poi.Count} shaders, {totalPoi / (float)MB:0.0} MB");
            sb.AppendLine($"   locked (generated) shaders resident: {poi.Count(r => r.Locked)}");
            sb.AppendLine();
            sb.AppendLine("      MB   props  variants  name");

            foreach (var r in poi.Take(30))
            {
                string variants = r.Variants < 0 ? "?" : r.Variants.ToString();
                sb.AppendLine($"   {r.Bytes / (float)MB,5:0.0}  {r.Props,6}  {variants,8}  {Truncate(r.Shader.name, 70)}");
            }
            if (poi.Count > 30) sb.AppendLine($"   ... and {poi.Count - 30} more");
            sb.AppendLine();
        }

        static void AppendMaterialSection(StringBuilder sb, Material[] materials)
        {
            sb.AppendLine("-- Resident materials --");

            var byShader = new Dictionary<string, MatStats>(StringComparer.Ordinal);
            long totalBytes = 0;
            int totalEntries = 0;
            int counted = 0;

            foreach (Material m in materials)
            {
                if (m == null || m.shader == null) continue;
                if (!IsInteresting(m.shader)) continue;

                int entries = CountSerializedEntries(m);
                long bytes = Profiler.GetRuntimeMemorySizeLong(m);

                totalBytes += bytes;
                totalEntries += entries;
                counted++;

                MatStats stats;
                if (!byShader.TryGetValue(m.shader.name, out stats))
                {
                    stats = new MatStats();
                    byShader[m.shader.name] = stats;
                }
                stats.Count++;
                stats.Entries += entries;
                stats.Bytes += bytes;
            }

            sb.AppendLine($"   {counted} Poiyomi materials resident");
            sb.AppendLine($"   {totalEntries} serialized property entries total"
                + (counted > 0 ? $"  (avg {totalEntries / counted} per material)" : ""));
            sb.AppendLine($"   {totalBytes / (float)MB:0.0} MB reported by the profiler");
            sb.AppendLine();
            sb.AppendLine("    mats   entries      MB  shader");
            foreach (var kv in byShader.OrderByDescending(k => k.Value.Entries))
            {
                sb.AppendLine($"   {kv.Value.Count,5}  {kv.Value.Entries,8}  {kv.Value.Bytes / (float)MB,6:0.0}  {Truncate(kv.Key, 60)}");
            }
            sb.AppendLine();
        }

        /// <summary>
        /// The direct measure of variant pressure. Every distinct keyword set across materials on one
        /// shader forces at least one more compiled variant per pass, so this is the lower bound on how
        /// much the editor has had to compile - and it needs no internal Unity API to compute.
        /// </summary>
        static void AppendKeywordSection(StringBuilder sb, Material[] materials)
        {
            sb.AppendLine("-- Keyword variant pressure (unlocked materials) --");

            var byShader = new Dictionary<string, List<Material>>(StringComparer.Ordinal);
            foreach (Material m in materials)
            {
                if (m == null || m.shader == null) continue;
                if (!IsInteresting(m.shader)) continue;
                if (m.shader.name.StartsWith("Hidden/Locked/", StringComparison.OrdinalIgnoreCase)) continue;

                List<Material> list;
                if (!byShader.TryGetValue(m.shader.name, out list))
                {
                    list = new List<Material>();
                    byShader[m.shader.name] = list;
                }
                list.Add(m);
            }

            if (byShader.Count == 0)
            {
                sb.AppendLine("   (no unlocked Poiyomi materials resident)");
                sb.AppendLine();
                return;
            }

            foreach (var kv in byShader.OrderByDescending(k => k.Value.Count))
            {
                var sets = new HashSet<string>(StringComparer.Ordinal);
                var keywordUse = new Dictionary<string, int>(StringComparer.Ordinal);

                foreach (Material m in kv.Value)
                {
                    string[] kws = m.shaderKeywords.Where(k => !string.IsNullOrEmpty(k)).ToArray();
                    Array.Sort(kws, StringComparer.Ordinal);
                    sets.Add(string.Join(" ", kws));

                    foreach (string k in kws)
                    {
                        int n;
                        keywordUse.TryGetValue(k, out n);
                        keywordUse[k] = n + 1;
                    }
                }

                sb.AppendLine($"   {Truncate(kv.Key, 60)}");
                sb.AppendLine($"      {kv.Value.Count} materials -> {sets.Count} distinct keyword sets, {keywordUse.Count} distinct keywords in use");

                var top = keywordUse.OrderByDescending(k => k.Value).ThenBy(k => k.Key, StringComparer.Ordinal).Take(8);
                sb.AppendLine("      most used: " + string.Join(", ", top.Select(t => $"{t.Key}({t.Value})").ToArray()));
            }
            sb.AppendLine();
            sb.AppendLine("   Each distinct keyword set is at least one more compiled variant per pass.");
        }

        class MatStats
        {
            public int Count;
            public int Entries;
            public long Bytes;
        }

        #endregion

        #region Helpers

        static int CountSerializedEntries(Material m)
        {
            SerializedObject so = new SerializedObject(m);
            int total = 0;
            // arraySize only - deliberately not walking the elements, which would be thousands of
            // SerializedProperty lookups per material.
            foreach (string section in new[] { "m_Floats", "m_Colors", "m_TexEnvs", "m_Ints" })
            {
                SerializedProperty p = so.FindProperty("m_SavedProperties." + section);
                if (p != null && p.isArray) total += p.arraySize;
            }
            return total;
        }

        // ShaderUtil.GetVariantCount is not public API and its signature has moved between Unity
        // versions, so match on name and first parameter rather than an exact signature, filling any
        // remaining parameters with their defaults. Reports "?" instead of failing the whole report.
        static MethodInfo s_variantCountMethod;
        static object[] s_variantCountArgs;
        static bool s_variantCountResolved;

        static void ResolveVariantCountMethod()
        {
            s_variantCountResolved = true;

            foreach (MethodInfo m in typeof(ShaderUtil).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (m.Name != "GetVariantCount") continue;

                ParameterInfo[] ps = m.GetParameters();
                if (ps.Length == 0 || ps[0].ParameterType != typeof(Shader)) continue;

                object[] args = new object[ps.Length];
                for (int i = 1; i < ps.Length; i++)
                {
                    Type t = ps[i].ParameterType;
                    args[i] = t.IsValueType ? Activator.CreateInstance(t) : null;
                }

                s_variantCountMethod = m;
                s_variantCountArgs = args;
                return;
            }
        }

        static int TryGetVariantCount(Shader shader)
        {
            if (!s_variantCountResolved) ResolveVariantCountMethod();
            if (s_variantCountMethod == null) return -1;

            try
            {
                object[] args = (object[])s_variantCountArgs.Clone();
                args[0] = shader;
                return Convert.ToInt32(s_variantCountMethod.Invoke(null, args));
            }
            catch (Exception)
            {
                return -1;
            }
        }

        static string Truncate(string s, int max)
        {
            if (string.IsNullOrEmpty(s) || s.Length <= max) return s;
            return "..." + s.Substring(s.Length - (max - 3));
        }

        #endregion
    } 
}
#endif
