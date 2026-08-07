using UnityEditor;
using UnityEngine;
using Thry.ThryEditor;

namespace Poi.Tools.Decorators
{
    public class PoiPrefabSpawnerDrawer : MaterialPropertyDrawer
    {
        readonly string _prefabGuid;
        GameObject _cachedPrefab;
        bool _cacheAttempted;

        public PoiPrefabSpawnerDrawer(string prefabGuid)
        {
            _prefabGuid = prefabGuid;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return EditorGUIUtility.singleLineHeight + 6;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            position = new RectOffset(0, 0, 0, 3).Remove(EditorGUI.IndentedRect(position));

            if (!_cacheAttempted)
            {
                _cacheAttempted = true;
                string path = AssetDatabase.GUIDToAssetPath(_prefabGuid);
                if (!string.IsNullOrEmpty(path))
                    _cachedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            string buttonLabel = string.IsNullOrEmpty(label) ? "Spawn Prefab" : label;

            if (_cachedPrefab == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUI.Button(position, buttonLabel + " (Prefab Not Found)");
                EditorGUI.EndDisabledGroup();
                return;
            }

            if (GUI.Button(position, buttonLabel))
            {
                string path = AssetDatabase.GUIDToAssetPath(_prefabGuid);
                if (string.IsNullOrEmpty(path))
                {
                    Debug.LogError("[PoiPrefabSpawner] Could not resolve prefab GUID: " + _prefabGuid);
                    return;
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    Debug.LogError("[PoiPrefabSpawner] Failed to load prefab at: " + path);
                    return;
                }

                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance == null)
                {
                    Debug.LogError("[PoiPrefabSpawner] Failed to instantiate prefab: " + prefab.name);
                    return;
                }

                instance.transform.SetParent(null);
                Undo.RegisterCreatedObjectUndo(instance, "Spawn " + prefab.name);
                Selection.activeGameObject = instance;
                EditorGUIUtility.PingObject(instance);
            }
        }
    }
}
