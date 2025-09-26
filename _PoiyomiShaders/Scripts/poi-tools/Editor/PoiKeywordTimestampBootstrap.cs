using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// Ensures that a shipped required timestamp propagates into EditorPrefs on import
// so downstream projects get the correct gate without manual action.
static class PoiKeywordTimestampBootstrap
{
	const string REQUIRED_TS_KEY = "Poi.KeywordRequiredTimestamp";
	const string JSON_PATH = "Assets/_PoiyomiShaders/PoiKeywordTimestamp.json";

	[InitializeOnLoadMethod]
	static void Init()
	{
		EditorApplication.delayCall += () =>
		{
			try
			{
				if (!File.Exists(JSON_PATH)) return;
				string json = File.ReadAllText(JSON_PATH).Trim();
				long fileTs = ParseTimestamp(json);
				if (fileTs <= 0) return;
				long currentTs = 0; long.TryParse(EditorPrefs.GetString(REQUIRED_TS_KEY, "0"), out currentTs);
				if (fileTs > currentTs)
				{
					EditorPrefs.SetString(REQUIRED_TS_KEY, fileTs.ToString());
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"[Poi] Failed to read keyword timestamp: {ex.Message}");
			}
		};
	}

	// Accepts either a raw ticks number or a minimal JSON object {"requiredTimestamp":<ticks>}
	static long ParseTimestamp(string content)
	{
		if (long.TryParse(content, out var direct)) return direct;
		try
		{
			var wrapper = JsonUtility.FromJson<TsWrapper>(content);
			return wrapper != null ? wrapper.requiredTimestamp : 0;
		}
		catch { return 0; }
	}

	[Serializable]
	class TsWrapper { public long requiredTimestamp; }
}


