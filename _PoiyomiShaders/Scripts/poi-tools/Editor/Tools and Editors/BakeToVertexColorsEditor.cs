using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace Poi.Tools
{
    public class BakeToVertexColorsEditor : EditorWindow
    {
        //Window
        static readonly Vector2 MIN_WINDOW_SIZE = new Vector2(316, 210);

        // Version
        Version version = new Version(1, 2);
        string SubTitle
        {
            get
            {
                if(string.IsNullOrWhiteSpace(_subTitle))
                    _subTitle = "by Pumkin - v" + version.ToString();
                return _subTitle;
            }
        }


        //Strings
        const string log_prefix = "<color=blue>Poi:</color> "; //color is hex or name

        const string bakedSuffix_normals = "baked_normals";
        const string bakedSuffix_position = "baked_position";

        const string bakesFolderName = "Baked";
        const string defaultUnityAssetBakesFolder = "Default Unity Resources";

        const string hint_bakeAverageNormals = "Use this if you want seamless outlines";
        const string hint_bakeVertexPositions = "Use this if you want scrolling emission";

        const string button_bakeAverageNormals = "Bake Averaged Normals";
        const string button_bakeVertexPositions = "Bake Vertex Positions";

        const string warning_noMeshesDetected =
            "No meshes detected in selection. Make sure your object has a Skinned Mesh Renderer or a Mesh Renderer with a valid Mesh assigned";

        //Properties
        static GameObject Selection
        {
            get => _selection;
            set => _selection = value;
        }

        [MenuItem("Poi/Vertex Color Baker", priority = 11)]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow editorWindow = GetWindow(typeof(BakeToVertexColorsEditor));
            editorWindow.autoRepaintOnSceneChange = true;
            editorWindow.minSize = MIN_WINDOW_SIZE;

            editorWindow.Show();
            editorWindow.titleContent = new GUIContent("Bake Colors");
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Poi Vertex Color Baker", PoiStyles.TitleLabel);
            EditorGUILayout.LabelField(SubTitle);

            PoiHelpers.DrawLine();

            EditorGUI.BeginChangeCheck();
            GameObject obj = EditorGUILayout.ObjectField("Avatar", Selection, typeof(GameObject), true) as GameObject;
            if(EditorGUI.EndChangeCheck())
                Selection = obj;

            PoiHelpers.DrawLine();

            EditorGUI.BeginDisabledGroup(!Selection);
            {
                // EditorGUILayout.HelpBox(hint_bakeAverageNormals, MessageType.Info);
                // if(GUILayout.Button(button_bakeAverageNormals))
                // {
                //     var meshes = GetAllMeshInfos(Selection);
                //     if(meshes == null || meshes.Length == 0)
                //         Debug.LogWarning(log_prefix + warning_noMeshesDetected);
                //     else
                //         BakeAveragedNormalsToColors(meshes);
                // }

                PoiHelpers.DrawLine(true, false);
                EditorGUILayout.HelpBox(hint_bakeVertexPositions, MessageType.Info);
                if(GUILayout.Button(button_bakeVertexPositions))
                {
                    var meshes = GetAllMeshInfos(Selection);
                    if(meshes == null || meshes.Length == 0)
                        Debug.LogWarning(log_prefix + warning_noMeshesDetected);
                    else
                        BakePositionsToColors(meshes);
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// Saves a mesh in the same folder as the original asset
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="newName">The new name of the mesh</param>
        /// <returns>Returns the newly created mesh asset</returns>
        static Mesh SaveMeshAsset(Mesh mesh, string newName)
        {
            string assetPath = AssetDatabase.GetAssetPath(mesh);

            if(string.IsNullOrWhiteSpace(assetPath))
            {
                Debug.LogWarning(log_prefix + "Invalid asset path for " + mesh.name);
                return null;
            }

            //Figure out folder name
            string bakesDir = $"{Path.GetDirectoryName(assetPath)}";

            //Handle default assets
            if(bakesDir.StartsWith("Library"))
                bakesDir = $"Assets\\{defaultUnityAssetBakesFolder}";

            if(!bakesDir.EndsWith(bakesFolderName))
                bakesDir += $"\\{bakesFolderName}";

            if(!assetPath.Contains('.'))
                assetPath += '\\';

            PoiHelpers.EnsurePathExistsInAssets(bakesDir);

            //Generate path
            string pathNoExt = Path.Combine(bakesDir, newName);
            string newPath = AssetDatabase.GenerateUniqueAssetPath($"{pathNoExt}.mesh");

            //Save mesh, load it back, assign to renderer
            Mesh newMesh = Instantiate(mesh);
            AssetDatabase.CreateAsset(newMesh, newPath);

            newMesh = AssetDatabase.LoadAssetAtPath<Mesh>(newPath);

            if(newMesh == null)
            {
                Debug.Log(log_prefix + "Failed to load saved mesh");
                return null;
            }

            EditorGUIUtility.PingObject(newMesh);
            return newMesh;
        }

        /// <summary>
        /// Sets the sharedMesh of a Skinned Mesh Renderer or Mesh Filter attached to a Mesh Renderer
        /// </summary>
        /// <param name="render"></param>
        /// <param name="mesh"></param>
        /// <returns></returns>
        static bool SetRendererSharedMesh(Renderer render, Mesh mesh)
        {
            if(render is SkinnedMeshRenderer smr)
                smr.sharedMesh = mesh;
            else if(render is MeshRenderer mr)
            {
                var filter = mr.gameObject.GetComponent<MeshFilter>();
                filter.sharedMesh = mesh;
            }
            else
                return false;
            return true;
        }

        static MeshInfo[] GetAllMeshInfos(GameObject obj)
        {
            return GetAllMeshInfos(obj?.GetComponentsInChildren<Renderer>(true));
        }

        static MeshInfo[] GetAllMeshInfos(params Renderer[] renderers)
        {
            var infos = renderers?.Select(ren =>
            {
                MeshInfo info = new MeshInfo();
                if(ren is SkinnedMeshRenderer smr)
                {
                    Mesh bakedMesh = new Mesh();
                    Transform tr = smr.gameObject.transform;
                    Quaternion origRot = tr.localRotation;
                    Vector3 origScale = tr.localScale;

                    tr.localRotation = Quaternion.identity;
                    tr.localScale = Vector3.one;

                    smr.BakeMesh(bakedMesh);

                    tr.localRotation = origRot;
                    tr.localScale = origScale;

                    info.sharedMesh = smr.sharedMesh;
                    info.bakedVertices = bakedMesh?.vertices;
                    info.bakedNormals = bakedMesh?.normals;
                    info.ownerRenderer = smr;
                    if(!info.sharedMesh)
                        Debug.LogWarning(log_prefix + $"Skinned Mesh Renderer at <b>{info.ownerRenderer.gameObject.name}</b> doesn't have a valid mesh");
                }
                else if(ren is MeshRenderer mr)
                {
                    info.sharedMesh = mr.GetComponent<MeshFilter>()?.sharedMesh;
                    info.bakedVertices = info.sharedMesh?.vertices;
                    info.bakedNormals = info.sharedMesh?.normals;
                    info.ownerRenderer = mr;
                    if(!info.sharedMesh)
                        Debug.LogWarning(log_prefix + $"Mesh renderer at <b>{info.ownerRenderer.gameObject.name}</b> doesn't have a mesh filter with a valid mesh");
                }
                return info;
            }).ToArray();

            return infos;
        }

        static void BakePositionsToColors(MeshInfo[] meshInfos)
        {
            var queue = new Dictionary<MeshInfo, Mesh>();
            try
            {
                AssetDatabase.StartAssetEditing();
                foreach(var meshInfo in meshInfos)
                {
                    if(!meshInfo.sharedMesh)
                        continue;

                    Vector3[] verts = meshInfo.bakedVertices;    //accessing mesh.vertices on every iteration is very slow
                    Color[] colors = new Color[verts.Length];
                    for(int i = 0; i < verts.Length; i++)
                        colors[i] = new Color(verts[i].x, verts[i].y, verts[i].z);
                    meshInfo.sharedMesh.colors = colors;

                    //Create new mesh asset and add it to queue
                    string name = PoiHelpers.AddSuffix(meshInfo.ownerRenderer.gameObject.name, bakedSuffix_position);
                    Mesh newMesh = SaveMeshAsset(meshInfo.sharedMesh, name);
                    if(newMesh)
                        queue.Add(meshInfo, newMesh);
                }
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            //After all meshes are imported assign the meshes
            foreach(var kv in queue)
            {
                SetRendererSharedMesh(kv.Key.ownerRenderer, kv.Value);
            }
        }

        static void BakeAveragedNormalsToColors(params MeshInfo[] infos)
        {
            var queue = new Dictionary<MeshInfo, Mesh>();
            try
            {
                AssetDatabase.StartAssetEditing();
                foreach(var meshInfo in infos)
                {
                    if(!meshInfo.sharedMesh)
                        continue;

                    Vector3[] verts = meshInfo.bakedVertices;
                    Vector3[] normals = meshInfo.bakedNormals;
                    VertexInfo[] vertInfo = new VertexInfo[verts.Length];
                    for(int i = 0; i < verts.Length; i++)
                    {
                        vertInfo[i] = new VertexInfo()
                        {
                            vertex = verts[i],
                            originalIndex = i,
                            normal = normals[i]
                        };
                    }
                    var groups = vertInfo.GroupBy(x => x.vertex);
                    VertexInfo[] processedVertInfo = new VertexInfo[vertInfo.Length];
                    int index = 0;
                    foreach(IGrouping<Vector3, VertexInfo> group in groups)
                    {
                        Vector3 avgNormal = Vector3.zero;
                        foreach(VertexInfo item in group)
                            avgNormal += item.normal;

                        avgNormal /= group.Count();
                        foreach(VertexInfo item in group)
                        {
                            processedVertInfo[index] = new VertexInfo()
                            {
                                vertex = item.vertex,
                                originalIndex = item.originalIndex,
                                normal = item.normal,
                                averagedNormal = avgNormal
                            };
                            index++;
                        }
                    }
                    Color[] colors = new Color[verts.Length];
                    for(int i = 0; i < processedVertInfo.Length; i++)
                    {
                        VertexInfo info = processedVertInfo[i];

                        int origIndex = info.originalIndex;
                        Vector3 normal = info.averagedNormal;
                        Color normColor = new Color(normal.x, normal.y, normal.z, 1);
                        colors[origIndex] = normColor;
                    }
                    meshInfo.sharedMesh.colors = colors;

                    string name = PoiHelpers.AddSuffix(meshInfo.ownerRenderer.gameObject.name, bakedSuffix_normals);
                    Mesh newMesh = SaveMeshAsset(meshInfo.sharedMesh, name);
                    if(newMesh)
                        queue.Add(meshInfo, newMesh);
                }
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            //Assign all new meshes to their renderers
            foreach(var kv in queue)
                SetRendererSharedMesh(kv.Key.ownerRenderer, kv.Value);
        }

        struct MeshInfo
        {
            public Renderer ownerRenderer;
            public Mesh sharedMesh;
            public Vector3[] bakedVertices;
            public Vector3[] bakedNormals;
        }

        struct VertexInfo
        {
            public Vector3 vertex;
            public int originalIndex;
            public Vector3 normal;
            public Vector3 averagedNormal;
        }

        static GameObject _selection;
        private string _subTitle;
    }
}