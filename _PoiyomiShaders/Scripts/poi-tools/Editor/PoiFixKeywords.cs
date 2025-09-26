using UnityEditor;
using UnityEngine;

public static class PoiFixKeywords
{
	[MenuItem("Poi/Fix All Keywords", false, 1200)]
	public static void FixAllKeywords()
	{
		// Delegate to Thry's existing menu command to ensure identical behavior
		EditorApplication.ExecuteMenuItem("Thry/ThryEditor/Fix Keywords for All Materials (Slow)");
	}
}


