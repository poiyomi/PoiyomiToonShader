#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class BakePosToColorEditor : EditorWindow
{
    string assetName = "BakedMesh";
    Mesh source;

    [MenuItem("Window/Bake Pos To Color Editor")]
    static void InitWindow()
    {
        BakePosToColorEditor window = (BakePosToColorEditor)EditorWindow.GetWindow(typeof(BakePosToColorEditor));
        window.Show();
    }

    void OnGUI()
    {
        assetName = GUILayout.TextField(assetName);
        source = (Mesh)EditorGUILayout.ObjectField(source, typeof(Mesh));
        if (GUILayout.Button("Bake") && assetName != string.Empty)
        {
            Vector3[] v = source.vertices;

            Mesh mesh = new Mesh();
            mesh.indexFormat = source.indexFormat;
            mesh.vertices = source.vertices;
            mesh.triangles = source.triangles;
            mesh.normals = source.normals;
            mesh.tangents = source.tangents;
            mesh.uv = source.uv;
            mesh.bounds = source.bounds;
            mesh.bindposes = source.bindposes;
            mesh.boneWeights = source.boneWeights;

            Color[] c = new Color[v.Length];
            for(int i = 0; i < c.Length; i++)
            {
                c[i] = new Color(v[i].x, v[i].y, v[i].z);
            }

            mesh.colors = c;

            AssetDatabase.CreateAsset(mesh, "Assets/Scripts/" + assetName + ".asset");
            AssetDatabase.SaveAssets();
        }
    }
}
#endif