using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Thry; // ShaderEditor
using Thry.ThryEditor; // ShaderOptimizer
using Thry.ThryEditor.Helpers; // ShaderHelper

public class PoiShaderKeywordPostprocessor : AssetPostprocessor
{
	static bool s_isQueued;
	static readonly List<Shader> s_importedShaders = new List<Shader>();

	const string REQUIRED_TS_KEY = "Poi.KeywordRequiredTimestamp";
	const string LAST_APPLIED_TS_KEY = "Poi.KeywordFixLastAppliedTimestamp";

	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		// Collect imported shaders that use Thry editor
		foreach (string path in importedAssets)
		{
			if (path.EndsWith(".shader", StringComparison.OrdinalIgnoreCase))
			{
				var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
				if (shader != null && ShaderHelper.IsShaderUsingThryEditor(shader))
					s_importedShaders.Add(shader);
			}
		}

		if (s_importedShaders.Count == 0 || s_isQueued) return;
		s_isQueued = true;

		// Defer the heavy work to after import finishes
		EditorApplication.delayCall += () =>
		{
			try
			{
				var shaders = s_importedShaders.Distinct().ToList();
				s_importedShaders.Clear();

				// Timestamp gate
				long requiredTs = 0;
				long.TryParse(EditorPrefs.GetString(REQUIRED_TS_KEY, "0"), out requiredTs);
				long lastAppliedTs = 0;
				long.TryParse(EditorPrefs.GetString(LAST_APPLIED_TS_KEY, "0"), out lastAppliedTs);
				if (requiredTs <= lastAppliedTs)
				{
					s_isQueued = false;
					return;
				}

				// Gather materials per imported shader and fix only those
				List<Material> matsToFix = new List<Material>();
				foreach (var s in shaders)
				{
					var mats = AssetDatabase.FindAssets("t:material")
						.Select(AssetDatabase.GUIDToAssetPath)
						.Where(p => string.IsNullOrEmpty(p) == false)
						.Select(p => AssetDatabase.LoadAssetAtPath<Material>(p))
						.Where(m => m != null && m.shader == s && ShaderOptimizer.IsMaterialLocked(m) == false);
					matsToFix.AddRange(mats);
				}

				if (matsToFix.Count > 0)
				{
					ShaderEditor.FixKeywords(matsToFix.Distinct());
					// Mark applied if we succeeded
					EditorPrefs.SetString(LAST_APPLIED_TS_KEY, requiredTs.ToString());
				}
			}
			finally
			{
				s_isQueued = false;
			}
		};
	}
}


