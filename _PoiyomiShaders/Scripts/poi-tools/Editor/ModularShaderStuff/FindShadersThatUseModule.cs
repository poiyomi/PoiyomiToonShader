#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Poiyomi.ModularShaderSystem;

namespace Poi.Tools
{
    public class FindShadersThatUseModule : EditorWindow
    {
        private ShaderModule _module;
        private HashSet<ModularShader> _shaders = new HashSet<ModularShader>();
        private Vector2 scrollPosition;
        private string _moduleGUID;

        [MenuItem("Poi/Tools/Modular Shader/FindShadersThatUseModule")]
        public static void Init()
        {
            FindShadersThatUseModule findShadersThatUseModule = GetWindow<FindShadersThatUseModule>();
            findShadersThatUseModule.Show();
        }

        void OnGUI()
        {
            bool update = false;
            EditorGUI.BeginChangeCheck();
            _module = EditorGUILayout.ObjectField("Module", _module, typeof(ShaderModule), false, null) as ShaderModule;
            if (EditorGUI.EndChangeCheck())
            {
                if (_module != null)
                {
                    update = true;
                    _moduleGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_module));
                }
                else
                {
                    _shaders.Clear();
                }
            }
            EditorGUI.BeginChangeCheck();
            _moduleGUID = EditorGUILayout.TextField("Module GUID", _moduleGUID);
            if (EditorGUI.EndChangeCheck())
            {
                string v = AssetDatabase.GUIDToAssetPath(_moduleGUID);
                if (!string.IsNullOrEmpty(v))
                {
                    _module = AssetDatabase.LoadAssetAtPath<ShaderModule>(v);
                    update = true;
                }
                else
                {
                    _shaders.Clear();
                }
            }
            if (update)
            {
                _shaders.Clear();
                string[] guids = AssetDatabase.FindAssets($"t:{nameof(ModularShader)}");
                var modularShaders = guids.Select(x => AssetDatabase.GUIDToAssetPath(x)).Select(x => AssetDatabase.LoadAssetAtPath<ModularShader>(x));
                foreach (var modularShader in modularShaders)
                {
                    if (modularShader.BaseModules.Contains(_module))
                    {
                        _shaders.Add(modularShader);
                    }
                }
            }
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (var modularShader in _shaders)
            {
                EditorGUILayout.ObjectField(modularShader, typeof(ModularShader), false, null);
            }
            EditorGUILayout.EndScrollView();
        }
    }
}
#endif