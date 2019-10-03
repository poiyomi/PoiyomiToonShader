using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Thry
{

    [CustomEditor(typeof(TextureImporter))]
    [CanEditMultipleObjects]
    public class ThryTextureInspector : AssetImporterEditor
    {

        Editor defaultEditor;

        public new void OnEnable()
        {
            base.OnEnable();
            //When this inspector is created, also create the built-in inspector
            defaultEditor = Editor.CreateEditor(targets, typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.TextureImporterInspector"));
        }

        public new void OnDisable()
        {
            base.OnDisable();
            //When OnDisable is called, the default editor we created should be destroyed to avoid memory leakage.
            //Also, make sure to call any required methods like OnDisable
            MethodInfo disableMethod = defaultEditor.GetType().GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (disableMethod != null)
                disableMethod.Invoke(defaultEditor, null);
            DestroyImmediate(defaultEditor);
        }

        public override bool showImportedObject { get { return false; } }

        public override void OnInspectorGUI()
        {
            defaultEditor.OnInspectorGUI();
            Rect position = GUILayoutUtility.GetLastRect();
            position.width = 160;
            if (position.height > 30)
            {
                position.height = 18;
                position.y -= 20;
            }
            if (GUI.Button(position, "Create Texture Array"))
            {
                string[] paths = new string[Selection.objects.Length];
                for (int i = 0; i < paths.Length; i++) paths[i] = AssetDatabase.GetAssetPath(Selection.objects[i]);
                Converter.PathsToTexture2DArray(paths);
            }
            if (ThryEditor.currentlyDrawing.textureArrayProperties != null &&
                ThryEditor.currentlyDrawing.textureArrayProperties.Count > 0)
            {
                GUILayout.Label("Create Texture Array for " + Helper.MaterialsToString(ThryEditor.currentlyDrawing.materials));
                GUILayout.BeginHorizontal();
                foreach (ThryEditor.ShaderProperty p in ThryEditor.currentlyDrawing.textureArrayProperties)
                {
                    if (GUILayout.Button(p.content.text,GUILayout.ExpandWidth(false)))
                    {
                        string[] paths = new string[Selection.objects.Length];
                        for (int i = 0; i < paths.Length; i++) paths[i] = AssetDatabase.GetAssetPath(Selection.objects[i]);
                        Texture tex = Converter.PathsToTexture2DArray(paths);
                        Helper.UpdateTextureValue(p.materialProperty, tex);
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

    }
}