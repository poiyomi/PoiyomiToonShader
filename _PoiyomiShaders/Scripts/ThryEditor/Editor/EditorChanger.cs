// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class EditorChanger : EditorWindow
    {
        // Add menu named "My Window" to the Window menu
        [MenuItem("Thry/Editor Tools/Use Thry Editor for other shaders")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            EditorChanger window = (EditorChanger)EditorWindow.GetWindow(typeof(EditorChanger));
            window.Show();
        }

        Vector2 scrollPos;

        bool[] setEditor;
        bool[] wasEditor;

        List<string> paths = null;
        List<Shader> shaders = null;

        void OnGUI()
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            bool init = false;

            if (paths == null)
            {
                paths = new List<string>();
                shaders = new List<Shader>();
                string[] shaderGuids = AssetDatabase.FindAssets("t:shader");

                for (int sguid = 0; sguid < shaderGuids.Length; sguid++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(shaderGuids[sguid]);
                    Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                    paths.Add(path);
                    shaders.Add(shader);
                }

                if (setEditor == null || setEditor.Length != shaderGuids.Length)
                {
                    setEditor = new bool[paths.Count];
                    wasEditor = new bool[paths.Count];
                }
                init = true;
            }

            for (int p = 0; p < paths.Count; p++)
            {
                if (init)
                {
                    EditorUtility.DisplayProgressBar("Load all shaders...", "", (float)p / paths.Count);
                    setEditor[p] = ShaderHelper.IsShaderUsingShaderEditor(shaders[p]);
                    wasEditor[p] = setEditor[p];
                }
                setEditor[p] = GUILayout.Toggle(setEditor[p], shaders[p].name);
            }
            if (init) EditorUtility.ClearProgressBar();

            GUILayout.EndScrollView();

            if (GUILayout.Button("Apply"))
            {
                for (int i = 0; i < paths.Count; i++)
                {
                    if (wasEditor[i] != setEditor[i])
                    {
                        string path = paths[i];
                        if (setEditor[i]) addShaderEditor(path);
                        else removeShaderEditor(path);
                    }

                    wasEditor[i] = setEditor[i];
                }
                AssetDatabase.Refresh();
                ShaderEditor.Repaint();
            }
        }

        private void addShaderEditor(string path)
        {
            replaceEditorInShader(path, "Thry.ShaderEditor");
            AddThryProperty(path);
        }

        public static void AddThryProperty(Shader shader)
        {
            string path = AssetDatabase.GetAssetPath(shader);
            AddThryProperty(path);
        }

        private static void AddThryProperty(string path)
        {
            addProperty(path, "[HideInInspector] shader_is_using_thry_editor(\"\", Float)", "0");
        }

        private void removeShaderEditor(string path)
        {
            revertEditor(path);
            RemoveThryProperty(path);
        }

        private void RemoveThryProperty(string path)
        {
            removeProperty(path, "[HideInInspector] shader_is_using_thry_editor(\"\", Float)", "0");
        }

        private static void addProperty(string path, string property, string value)
        {
            string shaderCode = FileHelper.ReadFileIntoString(path);
            string pattern = @"Properties.*\n?\s*{";
            RegexOptions options = RegexOptions.Multiline;
            shaderCode = Regex.Replace(shaderCode, pattern, "Properties \r\n  {" + " \r\n      " + property + "=" + value, options);

            FileHelper.WriteStringToFile(shaderCode, path);
        }

        private void removeProperty(string path, string property, string value)
        {
            string shaderCode = FileHelper.ReadFileIntoString(path);
            string pattern = @"\r?\n.*" + Regex.Escape(property) + " ?= ?" + value;
            RegexOptions options = RegexOptions.Multiline;

            shaderCode = Regex.Replace(shaderCode, pattern, "", options);

            FileHelper.WriteStringToFile(shaderCode, path);
        }

        private void revertEditor(string path)
        {
            string shaderCode = FileHelper.ReadFileIntoString(path);
            string pattern = @"//originalEditor.*\n";
            Match m = Regex.Match(shaderCode, pattern);
            if (m.Success)
            {
                string orignialEditor = m.Value.Replace("//originalEditor", "");
                pattern = @"//originalEditor.*\n.*\n";
                shaderCode = Regex.Replace(shaderCode, pattern, orignialEditor);
                FileHelper.WriteStringToFile(shaderCode, path);
            }
        }

        private void replaceEditorInShader(string path, string newEditor)
        {
            string shaderCode = FileHelper.ReadFileIntoString(path);
            string pattern = @"CustomEditor ?.*\n";
            Match m = Regex.Match(shaderCode, pattern);
            if (m.Success)
            {
                string oldEditor = "//originalEditor" + m.Value;
                shaderCode = Regex.Replace(shaderCode, pattern, oldEditor + "CustomEditor \"" + newEditor + "\"\r\n");
            }
            else
            {
                pattern = @"SubShader.*\r?\n?\s*{";
                RegexOptions options = RegexOptions.Multiline;
                shaderCode = Regex.Replace(shaderCode, pattern, "CustomEditor \"" + newEditor + "\" \r\n    SubShader \r\n  {", options);
            }

            FileHelper.WriteStringToFile(shaderCode, path);
        }

    }
}