#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Poi.Tools
{
    public class PoiOutlineUtilWindow : EditorWindow
    {
        private static Vector2 scrollPosition = new Vector2(0,0);
        private static GameObject avatar;
        private static readonly Dictionary<int, MeshSettings[]> meshSettings = new Dictionary<int, MeshSettings[]>(); // <instanceID, <submesh, setting>>
        private static Dictionary<Mesh, Mesh> bakedMeshes = new Dictionary<Mesh, Mesh>();
        private static int lang = -1;
        private static bool isCancelled = false;
        private static readonly Color emptyColor = new Color(0.5f, 0.5f, 1.0f, 1.0f);
        private static GUIStyle marginBox;

        private struct MeshSettings
        {
            public string name;
            public bool isBakeTarget;
            public float shrinkTipStrength;
        }

        //------------------------------------------------------------------------------------------------------------------------------
        // GUI
        [MenuItem("Poi/Outline Vertex Color Baker")]
        static void Init()
        {
            PoiOutlineUtilWindow window = (PoiOutlineUtilWindow)GetWindow(typeof(PoiOutlineUtilWindow), false, TEXT_WINDOW_NAME);
            window.Show();
        }

        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Language
            if(lang == -1)
            {
                lang = Application.systemLanguage == SystemLanguage.Japanese ? 1 : 0;
            }
            lang = EditorGUILayout.Popup("Language", lang, TEXT_LANGUAGES);
            marginBox = new GUIStyle(EditorStyles.helpBox);
            marginBox.margin.left = 30;

            //------------------------------------------------------------------------------------------------------------------------------
            // 1. Select the mesh
            EditorGUILayout.LabelField(TEXT_STEP_SELECT_AVATAR[lang], EditorStyles.boldLabel);
            avatar = (GameObject)EditorGUILayout.ObjectField(TEXT_ITEM_DD_AVATAR[lang], avatar, typeof(GameObject), true);
            if(avatar == null)
            {
                EditorGUILayout.EndScrollView();
                return;
            }
            if(AssetDatabase.Contains(avatar))
            {
                EditorGUILayout.HelpBox(TEXT_WARN_SELECT_FROM_SCENE[lang], MessageType.Error);
                EditorGUILayout.EndScrollView();
                return;
            }
            EditorGUILayout.Space();

            //------------------------------------------------------------------------------------------------------------------------------
            // 2. Select the modify target
            EditorGUILayout.LabelField(TEXT_STEP_SELECT_SUBMESH[lang], EditorStyles.boldLabel);
            if (GUILayout.Button("Select All"))
            {
                foreach (var item in meshSettings)
                {
                    for (int i = 0; i < item.Value.Length; i++)
                    {
                        item.Value[i].isBakeTarget = true;
                    }
                }
            }
            Component[] skinnedMeshRenderers = avatar.GetComponentsInChildren(typeof(SkinnedMeshRenderer), true);
            Component[] meshRenderers = avatar.GetComponentsInChildren(typeof(MeshRenderer), true);
            DrawModifyTargetsGUI(skinnedMeshRenderers, meshRenderers);
            EditorGUILayout.Space();

            //------------------------------------------------------------------------------------------------------------------------------
            // 3. Generate the mesh, test it, then save
            GameObject bakedAvatar = FindBakedAvatar();

            EditorGUILayout.LabelField(TEXT_STEP_GENERATE_AND_SAVE[lang], EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button(TEXT_BUTTON_GENERATE_AND_TEST[lang]))
            {
                GenerateMeshes(bakedAvatar, skinnedMeshRenderers, meshRenderers);
            }

            if(bakedAvatar == null)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndScrollView();
                return;
            }

            // Save
            bakedMeshes = new Dictionary<Mesh, Mesh>();
            GetBakedMeshes(bakedAvatar, skinnedMeshRenderers, meshRenderers);
            bool isSaved = true;
            foreach(Mesh bakedMesh in bakedMeshes.Values)
            {
                if(!isSaved) break;
                if(bakedMesh == null) continue;
                isSaved = AssetDatabase.Contains(bakedMesh);
            }

            GUIStyle saveButton = new GUIStyle(GUI.skin.button);
            if(!isSaved)
            {
                saveButton.normal.textColor = Color.red;
                saveButton.fontStyle = FontStyle.Bold;
            }

            if(GUILayout.Button(TEXT_BUTTON_SAVE[lang], saveButton))
            {
                SaveMeshes();
            }
            EditorGUILayout.EndHorizontal();

            if(!isSaved)
            {
                EditorGUILayout.HelpBox(TEXT_WARN_MESH_NOT_SAVED[lang], MessageType.Warning);
            }

            EditorGUILayout.EndScrollView();
        }

        //------------------------------------------------------------------------------------------------------------------------------
        // 2. Select the modify target
        private static void DrawModifyTargetsGUI(Component[] skinnedMeshRenderers, Component[] meshRenderers)
        {
            foreach(SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
            {
                EditorGUILayout.LabelField(skinnedMeshRenderer.gameObject.name, EditorStyles.boldLabel);
                int id = skinnedMeshRenderer.gameObject.GetInstanceID();
                Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
                Material[] materials = skinnedMeshRenderer.sharedMaterials;
                EditorGUI.indentLevel++;
                DrawGUIPerComponent(id, sharedMesh, materials);
                EditorGUI.indentLevel--;
            }

            foreach(MeshRenderer meshRenderer in meshRenderers)
            {
                MeshFilter meshFilter = meshRenderer.gameObject.GetComponent<MeshFilter>();
                if(meshFilter == null)
                {
                    continue;
                }
                EditorGUILayout.LabelField(meshRenderer.gameObject.name, EditorStyles.boldLabel);
                int id = meshRenderer.gameObject.GetInstanceID();
                Mesh sharedMesh = meshFilter.sharedMesh;
                Material[] materials = meshRenderer.sharedMaterials;
                EditorGUI.indentLevel++;
                DrawGUIPerComponent(id, sharedMesh, materials);
                EditorGUI.indentLevel--;
            }
        }

        private static void DrawGUIPerComponent(int id, Mesh sharedMesh, Material[] materials)
        {
            Vector3[] vertices = sharedMesh?.vertices;
            Vector3[] normals = sharedMesh?.normals;
            Vector4[] tangents = sharedMesh?.tangents;
            Color[] colors = sharedMesh?.colors;
            Vector2[] uv = sharedMesh?.uv;
            bool hasColors = colors != null && colors.Length > 2;
            bool hasUV0 = uv != null || uv.Length > 2;

            // Draw error messages
            if(sharedMesh == null)
            {
                EditorGUILayout.HelpBox(TEXT_WARN_MESH_IS_EMPTY[lang], MessageType.Error);
                return;
            }

            if(!sharedMesh.isReadable)
            {
                EditorGUILayout.HelpBox(TEXT_WARN_MESH_NOT_READABLE[lang], MessageType.Error);
                return;
            }

            if(vertices == null || vertices.Length < 2)
            {
                EditorGUILayout.HelpBox(TEXT_WARN_MESH_HAS_NO_VERT[lang], MessageType.Error);
                return;
            }

            if(normals == null && normals.Length < 2)
            {
                EditorGUILayout.HelpBox(TEXT_WARN_MESH_HAS_NO_NORM[lang], MessageType.Error);
                return;
            }

            if(tangents == null && tangents.Length < 2)
            {
                EditorGUILayout.HelpBox(TEXT_WARN_MESH_HAS_NO_TANJ[lang], MessageType.Error);
                return;
            }

            // Generate empty settings
            if(!meshSettings.ContainsKey(id)) meshSettings[id] = null;
            if(meshSettings[id] == null || meshSettings[id].Length != sharedMesh.subMeshCount)
            {
                meshSettings[id] = new MeshSettings[sharedMesh.subMeshCount];
                for(int i = 0; i < sharedMesh.subMeshCount; i++)
                {
                    meshSettings[id][i] = new MeshSettings
                    {
                        name = null,
                        isBakeTarget = false,
                        shrinkTipStrength = 0.0f
                    };
                }
            }

            // Draw settings
            for(int i = 0; i < sharedMesh.subMeshCount; i++)
            {
                if(string.IsNullOrEmpty(meshSettings[id][i].name))
                {
                    meshSettings[id][i].name = i + ": ";
                    if(i < materials.Length && materials[i] != null && !string.IsNullOrEmpty(materials[i].name))
                    {
                        meshSettings[id][i].name += materials[i].name;
                    }
                }
                DrawMeshSettingsGUI(id, i, hasColors, hasUV0);
            }
        }

        private static void DrawMeshSettingsGUI(int id, int i, bool hasColors, bool hasUV0)
        {
            meshSettings[id][i].isBakeTarget = EditorGUILayout.ToggleLeft(meshSettings[id][i].name, meshSettings[id][i].isBakeTarget);

            int indentCopy = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            if(meshSettings[id][i].isBakeTarget)
            {
                EditorGUILayout.BeginVertical(marginBox);
                meshSettings[id][i].shrinkTipStrength = EditorGUILayout.FloatField(TEXT_ITEM_SHRINK_TIP[lang], meshSettings[id][i].shrinkTipStrength);
                EditorGUILayout.EndVertical();
            }
            EditorGUI.indentLevel = indentCopy;
        }

        //------------------------------------------------------------------------------------------------------------------------------
        // 3. Generate the mesh, test it, then save
        private static int[] GetChildIndices(GameObject root, GameObject child)
        {
            var indices = new List<int>();
            indices.Add(child.transform.GetSiblingIndex());
            Transform parent = child.transform.parent;
            while(parent != null && parent != root.transform)
            {
                indices.Add(parent.GetSiblingIndex());
                parent = parent.parent;
            }
            return indices.ToArray();
        }

        private static GameObject GetChild(GameObject root, int[] indices)
        {
            Transform current = root.transform;
            for(int i = indices.Length - 1; i >= 0; i--)
            {
                current = current.GetChild(indices[i]);
                if(current == null) return null;
            }
            return current.gameObject;
        }

        private static GameObject GetChildInstance(GameObject root, GameObject rootInstance, GameObject child)
        {
            return GetChild(rootInstance, GetChildIndices(root, child));
        }

        private static void GenerateMeshes(GameObject bakedAvatar, Component[] skinnedMeshRenderers, Component[] meshRenderers)
        {
            if(bakedAvatar == null)
            {
                bakedAvatar = Instantiate(avatar);
                bakedAvatar.name = avatar.name + " (VertexColorBaked)";
                bakedAvatar.transform.parent = avatar.transform.parent;
                bakedAvatar.SetActive(true);
            }

            isCancelled = false;
            foreach(SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
            {
                GameObject child = GetChildInstance(avatar, bakedAvatar, skinnedMeshRenderer.gameObject);
                if(child == null)
                {
                    Debug.LogWarning($"[{TEXT_WINDOW_NAME}] Child is not found");
                    continue;
                }
                SkinnedMeshRenderer bakedSkinnedMeshRenderer = child.GetComponent<SkinnedMeshRenderer>();
                if(bakedSkinnedMeshRenderer == null)
                {
                    Debug.LogWarning($"[{TEXT_WINDOW_NAME}] Component is not found");
                    continue;
                }

                int id = skinnedMeshRenderer.gameObject.GetInstanceID();
                Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
                Mesh bakedMesh = bakedSkinnedMeshRenderer.sharedMesh;
                if(bakedMesh == null || !bakedMesh.name.Contains("(Clone)"))
                {
                    bakedMesh = Instantiate(sharedMesh);
                }
                BakeVertexColors(ref bakedMesh, sharedMesh, id);
                if(isCancelled) break;

                if(bakedMesh == null)
                {
                    Debug.LogWarning($"[{TEXT_WINDOW_NAME}] Mesh is not found");
                    continue;
                }
                bakedSkinnedMeshRenderer.sharedMesh = bakedMesh;
            }

            foreach(MeshRenderer meshRenderer in meshRenderers)
            {
                MeshFilter meshFilter = meshRenderer.gameObject.GetComponent<MeshFilter>();
                if(meshFilter == null)
                {
                    continue;
                }
                GameObject child = GetChildInstance(avatar, bakedAvatar, meshRenderer.gameObject);
                if(child == null)
                {
                    Debug.LogWarning($"[{TEXT_WINDOW_NAME}] Child is not found");
                    continue;
                }
                MeshFilter bakedMeshFilter = child.GetComponent<MeshFilter>();
                if(bakedMeshFilter == null)
                {
                    Debug.LogWarning($"[{TEXT_WINDOW_NAME}] Component is not found");
                    continue;
                }

                int id = meshRenderer.gameObject.GetInstanceID();
                Mesh sharedMesh = meshFilter.sharedMesh;
                Mesh bakedMesh = bakedMeshFilter.sharedMesh;
                if(bakedMesh == null || !bakedMesh.name.Contains("(Clone)"))
                {
                    bakedMesh = Instantiate(sharedMesh);
                }
                BakeVertexColors(ref bakedMesh, sharedMesh, id);
                if(isCancelled) break;

                if(bakedMesh == null)
                {
                    Debug.LogWarning($"[{TEXT_WINDOW_NAME}] Mesh is not found");
                    continue;
                }
                bakedMeshFilter.sharedMesh = bakedMesh;
            }
            if(!isCancelled) EditorUtility.DisplayDialog(TEXT_WINDOW_NAME, "Complete!", "OK");
        }

        private static void GetBakedMeshes(GameObject bakedAvatar, Component[] skinnedMeshRenderers, Component[] meshRenderers)
        {
            foreach(SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
            {
                Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
                if(sharedMesh == null)
                {
                    Debug.LogWarning($"[{TEXT_WINDOW_NAME}] Mesh is not found");
                    continue;
                }
                GameObject child = GetChildInstance(avatar, bakedAvatar, skinnedMeshRenderer.gameObject);
                if(child == null)
                {
                    Debug.LogWarning($"[{TEXT_WINDOW_NAME}] Child is not found");
                    continue;
                }
                SkinnedMeshRenderer bakedSkinnedMeshRenderer = child.GetComponent<SkinnedMeshRenderer>();
                if(bakedSkinnedMeshRenderer == null)
                {
                    Debug.LogWarning($"[{TEXT_WINDOW_NAME}] Component is not found");
                    continue;
                }
                bakedMeshes[sharedMesh] = bakedSkinnedMeshRenderer.sharedMesh;
            }

            foreach(MeshRenderer meshRenderer in meshRenderers)
            {
                MeshFilter meshFilter = meshRenderer.gameObject.GetComponent<MeshFilter>();
                if(meshFilter == null)
                {
                    Debug.LogWarning($"[{TEXT_WINDOW_NAME}] Component is not found");
                    continue;
                }
                Mesh sharedMesh = meshFilter.sharedMesh;
                if(sharedMesh == null)
                {
                    Debug.LogWarning($"[{TEXT_WINDOW_NAME}] Mesh is not found");
                    continue;
                }
                GameObject child = GetChildInstance(avatar, bakedAvatar, meshRenderer.gameObject);
                if(child == null)
                {
                    Debug.LogWarning($"[{TEXT_WINDOW_NAME}] Child is not found");
                    continue;
                }
                MeshFilter bakedMeshFilter = child.GetComponent<MeshFilter>();
                if(bakedMeshFilter == null)
                {
                    Debug.LogWarning($"[{TEXT_WINDOW_NAME}] Component is not found");
                    continue;
                }
                bakedMeshes[sharedMesh] = bakedMeshFilter.sharedMesh;
            }
        }

        private static void SaveMeshes()
        {
            foreach(KeyValuePair<Mesh, Mesh> bakedMesh in bakedMeshes)
            {
                if(bakedMesh.Value == null || string.IsNullOrEmpty(bakedMesh.Value.name)) continue;

                string path = AssetDatabase.GetAssetPath(bakedMesh.Value);
                if(string.IsNullOrEmpty(path))
                {
                    path = AssetDatabase.GetAssetPath(bakedMesh.Key);
                    if(string.IsNullOrEmpty(path) || !path.StartsWith("Assets/"))
                    {
                        path = "Assets/BakedMeshes/" + bakedMesh.Value.name + ".asset";
                    }
                    else
                    {
                        path = Path.GetDirectoryName(path) + "/BakedMeshes/" + bakedMesh.Value.name + ".asset";
                    }
                    path = GetUniqueName(path);
                }

                string saveDirectory = Path.GetDirectoryName(path);
                if(!Directory.Exists(saveDirectory))
                {
                    Directory.CreateDirectory(saveDirectory);
                }
                if(!File.Exists(path))
                {
                    Debug.Log($"[{TEXT_WINDOW_NAME}] Create asset to: " + path);
                    AssetDatabase.CreateAsset(bakedMesh.Value, path);
                }
                else
                {
                    Debug.Log($"[{TEXT_WINDOW_NAME}] Overwrite mesh to: " + path);
                }
            }
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog(TEXT_WINDOW_NAME, "Complete!", "OK");
        }

        private static string GetUniqueName(string path)
        {
            if(!File.Exists(path)) return path;

            string baseName = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path);
            string outPath;
            int i = 1;
            while(true)
            {
                outPath = baseName + " " + i.ToString() + ".asset";
                if(!File.Exists(outPath)) return outPath;
                i++;
            }
        }

        //------------------------------------------------------------------------------------------------------------------------------
        // Mesh Generator
        private static void BakeVertexColors(ref Mesh mesh, Mesh sharedMesh, int id)
        {
            if(isCancelled || sharedMesh == null || !sharedMesh.isReadable) return;
            Vector3[] vertices = sharedMesh.vertices;
            Vector3[] normals = sharedMesh.normals;
            Vector4[] tangents = sharedMesh.tangents;

            if(vertices == null || vertices.Length < 2 ||
               normals == null && normals.Length < 2 ||
               tangents == null && tangents.Length < 2)
            {
                return;
            }

            Color[] outColors = Enumerable.Repeat(Color.white, vertices.Length).ToArray();

            isCancelled = false;
            for(int mi = 0; mi < sharedMesh.subMeshCount; mi++)
            {
                if(!meshSettings[id][mi].isBakeTarget) continue;
                int[] sharedIndices = GetOptIndices(sharedMesh, mi);
                BakeNormalAverage(ref outColors, sharedIndices, meshSettings[id][mi], vertices, normals, tangents);
                EditorUtility.ClearProgressBar();
                if(isCancelled) return;
            }

            FixIllegalDatas(ref outColors);
            mesh.SetColors(outColors);
            EditorUtility.SetDirty(mesh);
        }

        //------------------------------------------------------------------------------------------------------------------------------
        // Bake normal to color
        private static void BakeNormalAverage(ref Color[] outColors, int[] sharedIndices, MeshSettings settings, Vector3[] vertices, Vector3[] normals, Vector4[] tangents)
        {
            var normalAverages = NormalGatherer.GetNormalAveragesFast(sharedIndices, vertices, normals);
            string message = "Run bake in " + settings.name;

            for(int i = 0; i < sharedIndices.Length; ++i)
            {
                int index = sharedIndices[i];
                float width = 1.0f;
                Vector3 normal = normals[index];
                Vector4 tangent = tangents[index];
                Vector3 bitangent = Vector3.Cross(normal, tangent) * tangent.w;
                if(IsIllegalTangent(normal, tangent))
                {
                    outColors[index].r = 0.5f;
                    outColors[index].g = 0.5f;
                    outColors[index].b = 1.0f;
                    outColors[index].a = 1.0f;
                    continue;
                }
                Vector3 normalAverage = NormalGatherer.GetClosestNormal(normalAverages, vertices[index]);
                if(settings.shrinkTipStrength > 0) width *= Mathf.Pow(Mathf.Clamp01(Vector3.Dot(normal,normalAverage)), settings.shrinkTipStrength);
                outColors[index].r = Vector3.Dot(normalAverage, tangent) * 0.5f + 0.5f;
                outColors[index].g = Vector3.Dot(normalAverage, bitangent) * 0.5f + 0.5f;
                outColors[index].b = Vector3.Dot(normalAverage, normal) * 0.5f + 0.5f;
                outColors[index].a = width;
                if(DrawProgress(message, i, (float)i / (float)sharedIndices.Length)) return;
            }
        }

        public static bool DrawProgress(string message, int i, float progress)
        {
            if((i & 0b11111111) == 0b11111111) return isCancelled = isCancelled || EditorUtility.DisplayCancelableProgressBar(TEXT_WINDOW_NAME, message, progress);
            return isCancelled;
        }

        private static int[] GetOptIndices(Mesh mesh, int mi)
        {
            return mesh.GetIndices(mi).ToList().Distinct().ToArray();
        }

        private static bool IsIllegalTangent(Vector3 normal, Vector4 tangent)
        {
            return normal.x == tangent.x && normal.y == tangent.y && normal.z == tangent.z;
        }

        //------------------------------------------------------------------------------------------------------------------------------
        // Utils

        private static GameObject FindBakedAvatar()
        {
            if(avatar.transform.parent != null)
            {
                for(int i = 0; i < avatar.transform.parent.childCount; i++)
                {
                    GameObject childObject = avatar.transform.parent.GetChild(i).gameObject;
                    if(childObject.name.Contains(avatar.name + " (VertexColorBaked)"))
                    {
                        return childObject;
                    }
                }
            }

            return GameObject.Find(avatar.name + " (VertexColorBaked)");
        }

        private static void FixIllegalDatas(ref Color[] outColors)
        {
            for(int i = 0; i < outColors.Length; i++)
            {
                Color color = outColors[i];
                if(
                    color.r >= 0 && color.r <= 1 &&
                    color.g >= 0 && color.g <= 1 &&
                    color.b >= 0 && color.b <= 1 &&
                    color.a >= 0 && color.a <= 1
                )
                {
                    continue;
                }

                outColors[i] = emptyColor;
            }
        }

        //------------------------------------------------------------------------------------------------------------------------------
        // Languages
        private const string TEXT_WINDOW_NAME = "PoiOutlineUtil";

        private static readonly string[] TEXT_LANGUAGES                 = new[] {"English", "Japanese"};

        private static readonly string[] TEXT_STEP_SELECT_AVATAR        = new[] {"1. Select the avatar",                        "1. アバターを選択"};
        private static readonly string[] TEXT_STEP_SELECT_SUBMESH       = new[] {"2. Select the modify target",                 "2. 編集対象を選択"};
        private static readonly string[] TEXT_STEP_GENERATE_AND_SAVE    = new[] {"3. Generate the mesh, test it, then save",    "3. メッシュを生成・テスト・保存"};

        private static readonly string[] TEXT_WARN_SELECT_FROM_SCENE    = new[] {"Please select from the scene (hierarchy)",            "シーン（ヒエラルキー）から選択してください"};
        private static readonly string[] TEXT_WARN_MESH_NOT_READABLE    = new[] {"The selected mesh is not set to \"Read/Write\" on.",  "選択されたメッシュは\"Read/Write\"がオンになっていません。"};
        private static readonly string[] TEXT_WARN_MESH_IS_EMPTY        = new[] {"The selected mesh is empty!",         "選択したメッシュは空です"};
        private static readonly string[] TEXT_WARN_MESH_HAS_NO_VERT     = new[] {"The selected mesh has no vertices!",  "選択したメッシュは頂点がありません。"};
        private static readonly string[] TEXT_WARN_MESH_HAS_NO_NORM     = new[] {"The selected mesh has no normals!",   "選択したメッシュは法線がありません。"};
        private static readonly string[] TEXT_WARN_MESH_HAS_NO_TANJ     = new[] {"The selected mesh has no tangents!",  "選択したメッシュはタンジェントがありません。"};
        private static readonly string[] TEXT_WARN_MESH_NOT_SAVED       = new[] {"Generated mesh is not saved!",        "生成されたメッシュが保存されていません。"};

        private static readonly string[] TEXT_ITEM_DD_AVATAR            = new[] {"Avatar (D&D from scene)", "アバター (シーンからD&D)"};
        private static readonly string[] TEXT_ITEM_SHRINK_TIP           = new[] {"Shrink the tip",          "先端を細くする度合い"};

        private static readonly string[] TEXT_BUTTON_GENERATE_AND_TEST  = new[] {"Generate & Test",         "生成 & テスト"};
        private static readonly string[] TEXT_BUTTON_SAVE               = new[] {"Save",                    "保存"};
    }

    public class NormalGatherer
    {
        public static Dictionary<Vector3, Vector3> GetNormalAveragesFast(int[] sharedIndices, Vector3[] vertices, Vector3[] normals)
        {
            var normalAverages = new Dictionary<Vector3, Vector3>();
            string message = "Generating averages";

            for(int i = 0; i < sharedIndices.Length; i++)
            {
                int index = sharedIndices[i];
                Vector3 pos = vertices[index];
                if(!normalAverages.ContainsKey(pos))
                {
                    normalAverages[pos] = normals[index];
                    continue;
                }
                normalAverages[pos] += normals[index];
                if(PoiOutlineUtilWindow.DrawProgress(message, i, (float)i / (float)vertices.Length)) return normalAverages;
            }

            var keys = normalAverages.Keys.ToArray();
            for(int j = 0; j < keys.Length; j++)
            {
                normalAverages[keys[j]] = Vector3.Normalize(normalAverages[keys[j]]);
            }

            return normalAverages;
        }

        public static Vector3 GetClosestNormal(Dictionary<Vector3, Vector3> normalAverages, Vector3 pos)
        {
            if(normalAverages.ContainsKey(pos)) return normalAverages[pos];

            float closestDist = 1000.0f;
            Vector3 closestNormal = new Vector3(0,0,0);
            foreach(KeyValuePair<Vector3, Vector3> normalAverage in normalAverages)
            {
                float dist = Vector3.Distance(pos, normalAverage.Key);
                closestDist = dist < closestDist ? dist : closestDist;
                closestNormal = dist < closestDist ? normalAverage.Value : closestNormal;
            }

            return closestNormal;
        }
    }

}
#endif