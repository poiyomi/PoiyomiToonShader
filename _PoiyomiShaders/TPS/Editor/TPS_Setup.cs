// TPS Setup System. Setups up penetrators, orifices, and the animator. Uses a ton of VRC Functions so it doesnt make sense to make it non vrc compatible
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using static Thry.TPS.ThryAnimatorFunctions;
using System.Text.RegularExpressions;
using static Thry.TPS.BakeToVertexColors;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#if VRC_SDK_VRCSDK3 && !UDON
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.Contact.Components;
using static VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;
using static VRC.SDKBase.VRC_AvatarParameterDriver;
#endif

namespace Thry.TPS
{
    public class TPS_Setup : EditorWindow
    {
        const string VERSION = "1.2.12";

        [MenuItem("Poi/TPS Setup Wizard", priority = 100)]
        static void Init()
        {
            TPS_Setup window = (TPS_Setup)EditorWindow.GetWindow(typeof(TPS_Setup));
            window.titleContent = new GUIContent("TPS Setup Wizard");
            window.Show();
        }

        string UniquePath(string path, string postFix)
        {
            if (File.Exists(path + postFix))
            {
                int i = 0;
                while (File.Exists(path + i + postFix)) i++;
                path = path + i;
            }
            return path + postFix;
        }

        void FindAvatarDirectory()
        {
            string path = AssetDatabase.GetAssetPath(_avatar);
            if (string.IsNullOrEmpty(path) && _avatar.GetComponent<Animator>()) path = AssetDatabase.GetAssetPath(_avatar.GetComponent<Animator>().avatar);
            if (string.IsNullOrEmpty(path) && _animator != null) path = AssetDatabase.GetAssetPath(_animator);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("[TPS] Could not find avatar file path. Using Assets folder. Make sure your avatar is a prefab or your animator has an avatar assigned in the future.");
                _avatarDirectory = "Assets";
                return;
            }
            _avatarDirectory = Path.GetDirectoryName(path);
        }

        const float ORF_HOLE_RANGE_ID = 0.41f;
        const float ORF_RING_RANGE_ID = 0.42f;
        const float ORF_NORM_RANGE_ID = 0.45f;

        public enum OrificeType
        {
            Hole, Ring
        }
        public class PenetratorConfig
        {
            public Transform Transform;
            public Transform TransformTip;
            public bool IsBaked;
            public bool HasMesh;
            public Renderer Renderer;
            public Texture2D Mask;
            public bool EditMask;

            public bool Remove;
            public PenetratorConfig(Transform t)
            {
                Transform = t;
                Renderer = t.GetComponentInChildren<Renderer>();
                OnRendererChanged();
            }

            public void SetTransform(Transform t)
            {
                Transform = t;
                Renderer = t.GetComponentInChildren<Renderer>();
                OnRendererChanged();
            }

            void OnRendererChanged()
            {
                if (Renderer == null) return;
                HasMesh = GetMesh(Renderer) != null;
                if (!HasMesh) SetBaked(false);
                else if (Renderer is SkinnedMeshRenderer) SetBaked(AreVerteciesBaked(Renderer));
                else if (Renderer is MeshRenderer) SetBaked(false);
            }

            public void SetBaked(bool b)
            {
                if (Renderer == null) return;
                IsBaked = b;
                foreach (Material m in Renderer.sharedMaterials.Where(m => m != null))
                {
                    m.SetFloat("_TPS_IsSkinnedMeshRenderer", b ? 1 : 0);
                    if(b) m.EnableKeyword("TPS_IsSkinnedMesh");
                    else m.DisableKeyword("TPS_IsSkinnedMesh");
                }
            }
        }
        public class OrificeConfig
        {
            public Transform Transform;
            public OrificeType OrificeType;
            public Renderer Renderer;
            public string[] BlendshapeNames = new string[] { "none" };
            public int BlendShapeIndexEnter = 1;
            public int BlendShapeIndexIn = 2;
            public float MaxOpeningWidth = 1;
            public bool ScaleBlendshapesByWidth = true;
            public bool DoAnimatorSetup = true;
            public float MaxDepth = 1;

            public bool AllowTransformEditing;
            public bool Remove;

            public OrificeConfig()
            {
                AllowTransformEditing = true;
            }
            public OrificeConfig(Transform t)
            {
                Transform = t;
                Renderer = t.GetComponentsInChildren<Renderer>().Where(r => r != null && GetMesh(r) != null).OrderBy(r => r is SkinnedMeshRenderer ? GetMesh(r).blendShapeCount : 0).Reverse().FirstOrDefault();
                if (Renderer == null && t.parent != null) Renderer = t.parent.GetComponent<Renderer>();
                if (Renderer != null) SetRenderer(Renderer);
                OrificeType = t.GetComponentsInChildren<Light>().Any(l => l.range == ORF_RING_RANGE_ID) ? OrificeType.Ring : OrificeType.Hole;
                ConfigureLights();
            }

            public void SetRenderer(Renderer r)
            {
                Renderer = r;
                LoadBlendshapes();
                ChangedSelectedShapekeys();
                CalculateMaxDepth();
            }

            void CalculateMaxDepth()
            {
                if (Renderer == null) return;
                Mesh mesh = GetMesh(Renderer); ;
                if (mesh == null)
                {
                    Debug.LogWarning("[TPS][SetupPenetrator] Mesh is null.");
                    return;
                }
                Vector3 forwardVec = (Renderer.transform.worldToLocalMatrix * Transform.forward).normalized;
                IEnumerable<float> zDistances = mesh.vertices.Select(v => Vector3.Dot(v, forwardVec));
                MaxDepth = (zDistances.Max() - zDistances.Min()) * Renderer.transform.lossyScale.z;
            }

            public void LoadBlendshapes()
            {
                BlendshapeNames = new string[] { "none" };
                if (Renderer != null && Renderer is SkinnedMeshRenderer)
                {
                    Mesh skinnedMesh = (Renderer as SkinnedMeshRenderer).sharedMesh;
                    if (skinnedMesh != null && skinnedMesh.blendShapeCount > 0)
                    {
                        BlendshapeNames = new string[skinnedMesh.blendShapeCount + 1];
                        for (int b = 0; b < skinnedMesh.blendShapeCount; b++)
                            BlendshapeNames[b + 1] = skinnedMesh.GetBlendShapeName(b);
                        BlendshapeNames[0] = "~none~";
                    }
                }
            }

            public void ChangedSelectedShapekeys()
            {
                if (Renderer == null) return;
                if (Renderer is SkinnedMeshRenderer)
                {
                    MaxOpeningWidth = 0;
                    Mesh m = (Renderer as SkinnedMeshRenderer).sharedMesh;
                    Vector3[] vertecies = new Vector3[m.vertexCount];
                    Vector3[] normals = new Vector3[m.vertexCount];
                    Vector3[] tangents = new Vector3[m.vertexCount];
                    if (BlendShapeIndexEnter - 1 < m.blendShapeCount)
                    {
                        m.GetBlendShapeFrameVertices(BlendShapeIndexEnter - 1, m.GetBlendShapeFrameCount(BlendShapeIndexEnter - 1) - 1, vertecies, normals, tangents);
                        MaxOpeningWidth = Mathf.Max(MaxOpeningWidth, vertecies.Select(v => new Vector3(v.x * Renderer.transform.lossyScale.x, v.y * Renderer.transform.lossyScale.y, 0).magnitude).Max() * 2);
                    }
                    if (BlendShapeIndexIn - 1 < m.blendShapeCount)
                    {
                        m.GetBlendShapeFrameVertices(BlendShapeIndexIn - 1, m.GetBlendShapeFrameCount(BlendShapeIndexIn - 1) - 1, vertecies, normals, tangents);
                        MaxOpeningWidth = Mathf.Max(MaxOpeningWidth, vertecies.Select(v => new Vector3(v.x * Renderer.transform.lossyScale.x, v.y * Renderer.transform.lossyScale.y, 0).magnitude).Max() * 2);
                    }
                }
            }

            public string GetBlendshapeNameEnter()
            {
                return BlendshapeNames[BlendShapeIndexEnter];
            }

            public string GetBlendshapeNameIn()
            {
                return BlendshapeNames[BlendShapeIndexIn];
            }

            public void ConfigureLights()
            {
                float idPos = OrificeType == OrificeType.Hole ? ORF_HOLE_RANGE_ID : ORF_RING_RANGE_ID;
                float idNor = ORF_NORM_RANGE_ID;

                bool foundPos = false, foundNor = false;
                Light[] lights = Transform.GetComponentsInChildren<Light>(true);
                foreach (Light l in lights)
                {
                    if (!foundPos && l.range == idPos) foundPos = true;
                    else if (!foundNor && l.range == idNor) foundNor = true;
                    else if (PrefabUtility.IsPartOfAnyPrefab(l.gameObject)) DestroyImmediate(l);
                    else DestroyImmediate(l.gameObject);
                }
                if (!foundPos)
                {
                    Transform lt = SingletonChild(Transform, "Position");
                    if (OrificeType == OrificeType.Hole) AddLight(lt, ORF_HOLE_RANGE_ID);
                    else AddLight(lt, ORF_RING_RANGE_ID);
                }
                if (!foundNor)
                {
                    Transform lt = SingletonChild(Transform, "Normal");
                    lt.localPosition = Vector3.forward * 0.01f / Transform.lossyScale.z;
                    AddLight(lt, ORF_NORM_RANGE_ID);
                }
            }

            void AddLight(Transform t, float range)
            {
                Light l = t.gameObject.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = Color.black;
                l.range = range;
                l.shadows = LightShadows.None;
                l.renderMode = LightRenderMode.ForceVertex;
            }
        }


#region GUI

        static GUIStyle s_styleRichtText;
        static GUIStyle s_styleRichtTextCentered;

        void InitStyles()
        {
            s_styleRichtText = new GUIStyle(EditorStyles.boldLabel) { richText = true };
            s_styleRichtTextCentered = new GUIStyle(EditorStyles.boldLabel) { richText = true, alignment = TextAnchor.LowerCenter };
        }

        Transform _avatar;
        string _avatarDirectory;
        bool _doesAnimatorHaveWDOn;
        AnimatorController _animator;
        List<PenetratorConfig> _penetrators;
        List<OrificeConfig> _orifices;
        bool _doClear;
        Vector2 _scrolling;

        Prefab[] _prefabsOrifice;
        Prefab[] _prefabsPenetrator;

        Color _backgroundColor;
        private void OnGUI()
        {
            InitStyles();
            _backgroundColor = GUI.backgroundColor;

            GUILayout.Space(10);
            EditorGUILayout.LabelField($"<color=fuchsia><size=25> Thry's Penetration System </size><size=16>v{VERSION}</size></color>", s_styleRichtTextCentered, GUILayout.Height(30));
            //EditorGUILayout.HelpBox("Follow this tool to setup TPS on your avatar.", MessageType.None);
            _scrolling = EditorGUILayout.BeginScrollView(_scrolling);
            GUILayout.Space(10);
            if(Tools.pivotMode != PivotMode.Pivot || !SceneView.lastActiveSceneView.sceneLighting)
            {
                Box("<size=20>0. Unity Settings Problems</size>", 25, Color.red, GUI_EditorProblems, null);
                GUILayout.Space(10);
            }
            Box("<size=20>1. Add Prefabs to avatar</size> <size=10>Click here to refresh</size>", 25, Color.green, GUI_PrefabsList, FindTPSPrefabs);
            GUILayout.Space(5);
            Box("<size=20>2. Scan Avatar</size>", 25, Color.blue, GUI_Setup, null);
#if VRC_SDK_VRCSDK3 && !UDON
            if (_penetrators == null || _animator == null || _avatar == null)
#else
            if (_penetrators == null || _avatar == null)
#endif
            {
                EditorGUILayout.EndScrollView();
                return;
            }
            GUILayout.Space(5);
            Box("<size=20>2.3. Penetrators: </size><size=15>Make sure they have their vertex colors baked</size>", 25, Color.cyan, GUI_Penetrators, null);
            GUILayout.Space(5);
            Box("<size=20>2.4. Orifices: </size><size=15>Configure your orifice options</size>", 25, Color.yellow, GUI_Orifices, null);
            GUILayout.Space(5);
            Box("<size=20>3. !Make sure to apply your setup!</size>", 25, Color.red, GUI_Button_Apply, null);
            GUILayout.Space(5);
            Box("<size=20>Help and Information</size>", 25, Color.gray, GUI_Information, null);
            GUILayout.Space(5);
            Box("<size=20>Removal options</size>", 25, Color.gray, GUI_Buttons_Remove, null);
            EditorGUILayout.EndScrollView();
        }

        void Box(string label, float labelHeight, Color color, Action guiFunction, Action onHeaderClick)
        {
            GUI.backgroundColor = color;
            using (new GUILayout.VerticalScope("Box"))
            {
                EditorGUILayout.LabelField(label, s_styleRichtText, GUILayout.Height(labelHeight));
                if (onHeaderClick != null && Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                    onHeaderClick.Invoke();
                GUI.backgroundColor = _backgroundColor;
                if (guiFunction != null) guiFunction.Invoke();
            }
        }

        void GUI_EditorProblems()
        {
            if (!SceneView.lastActiveSceneView.sceneLighting)
            {
                EditorGUILayout.HelpBox("Scene Lighting is not turned on. The penetrators will not move towards orifices.", MessageType.Error);
                if (GUILayout.Button("Fix SceneLighting")) SceneView.lastActiveSceneView.sceneLighting = true;
            }
            if (Tools.pivotMode != PivotMode.Pivot)
            {
                EditorGUILayout.HelpBox("Pivot Mode is set not set to pivot. This might be confusing while setting up penetrators.", MessageType.Warning);
                if (GUILayout.Button("Fix Pivot Mode")) Tools.pivotMode = PivotMode.Pivot;
            }
        }

        void GUI_PrefabsList()
        {
            if (_prefabsOrifice == null) FindTPSPrefabs();
            EditorGUI.indentLevel += 2;
            EditorGUILayout.LabelField("You can drag the listed assets directly from here into the scene", s_styleRichtText);
            EditorGUI.indentLevel -= 2;
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUI.backgroundColor = Color.black;
            using (new GUILayout.VerticalScope("Box"))
            {
                EditorGUILayout.LabelField("Penetrators", s_styleRichtText);
                GUI.backgroundColor = _backgroundColor;
                foreach (Prefab o in _prefabsPenetrator) PrefabListing(o);
            }

            GUI.backgroundColor = Color.black;
            using (new GUILayout.VerticalScope("Box"))
            {
                EditorGUILayout.LabelField("Orifices", s_styleRichtText);
                GUI.backgroundColor = _backgroundColor;
                foreach (Prefab o in _prefabsOrifice) PrefabListing(o);
            }
            GUILayout.EndHorizontal();
            if (Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(AssetDatabase.FindAssets("PenetratorSetup t:TextAsset").Select(g => AssetDatabase.GUIDToAssetPath(g)).Where(p => p.EndsWith(".txt")).FirstOrDefault(), typeof(TextAsset)));
        }

        void PrefabListing(Prefab prefab)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(70));
            Rect objectField = new Rect(r);
            objectField.width -= 70;
            Rect textureRect = new Rect(r);
            textureRect.x += objectField.width;
            textureRect.width = 70;
            GUI.DrawTexture(textureRect, prefab.GetTexture());
            if (Event.current.isMouse && Event.current.type == EventType.MouseDrag && r.Contains(Event.current.mousePosition))
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new UnityEngine.Object[] { prefab.GameObject };
                DragAndDrop.StartDrag("Dragging TPS Prefab");
                Event.current.Use();
            }
            EditorGUI.ObjectField(objectField, prefab.GameObject, typeof(GameObject), false);
        }

        void GUI_Setup()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginVertical();

            GUILayout.Space(5);
            EditorGUILayout.LabelField("<size=15>2.1 Select your avatar from the scene.</size>", s_styleRichtText);
            EditorGUI.BeginChangeCheck();
            EditorGUI.BeginChangeCheck();
            _avatar = (Transform)EditorGUILayout.ObjectField("Avatar", _avatar, typeof(Transform), true);
            if (EditorGUI.EndChangeCheck()) _animator = null;
            if(_avatar == null)
            {
#if VRC_SDK_VRCSDK3 && !UDON
                Scene scene = EditorSceneManager.GetActiveScene();
                if (scene != null) _avatar = scene.GetRootGameObjects().
                        Where(g => g.activeSelf && g.GetComponent<VRCAvatarDescriptor>()).Select(g => g.transform).
                         OrderByDescending(t => 
                            t.GetComponentsInChildren<Renderer>().Where(r => IsRendererPenetrator(r)).Count() +
                            t.GetComponentsInChildren<Light>().Where(l => GetOrificeRootFromLight(l.transform) != null).Count()).
                         FirstOrDefault();
#endif
            }
            AnimatorController prevAnim = _animator;
#if VRC_SDK_VRCSDK3 && !UDON
            _animator = (AnimatorController)EditorGUILayout.ObjectField("Animator", _animator, typeof(AnimatorController), false);
            bool changed = EditorGUI.EndChangeCheck();
            if (_animator == null && _avatar != null && _avatar.GetComponent<VRCAvatarDescriptor>())
            {
                GUI_ResolveAssets();
            }
            if (prevAnim != _animator && _animator != null)
            {
                GUI_CheckAnimatorHasWDOn();
            }
            if (_animator != null && _avatar != null)
            {
                if (_doesAnimatorHaveWDOn)
                {
                    EditorGUILayout.HelpBox("Your animator has at least one state with 'Write Defaults' turned on. This may cause problems with the TPS animator setup.", MessageType.Warning);
                }
                GUILayout.Space(10);
                EditorGUILayout.LabelField("<size=15>2.2 Scan your avatar for TPS objects. Confirm all have been found.</size>", s_styleRichtText);
                if (_penetrators == null)
                    ScanForTPS();
                if (GUILayout.Button("Scan Avatar for TPS") || changed)
                {
                    GUI_CheckAnimatorHasWDOn();
                    ScanForTPS();
                }
            }
#else
            bool changed = EditorGUI.EndChangeCheck();
            if (GUILayout.Button("Scan Avatar for TPS") || changed)
            {
                ScanForTPS();
            }
#endif

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        bool showAddPenetratorHelpbox;
        void GUI_Penetrators()
        {
            EditorGUILayout.HelpBox("Penetrators are identified by having TPS enabled on their material. Make sure the vertecies are baked. Use the tip field (optional) to update the direction.", MessageType.Info);
            foreach (PenetratorConfig p in _penetrators)
            {
                GUI_Penetrator(p);
            }
            _penetrators.RemoveAll(p => p.Remove);
            if (showAddPenetratorHelpbox)
            {
                EditorGUILayout.HelpBox("1.Add the penetrator onto your avatar 2.Enable TPS on a material of your penetrator 3. Scan again" +
                    "4. Create an empty gameobject at the tip of the penetrator and assign it to the tip field", MessageType.Info);
            }
            if (GUILayout.Button("Add Custom Penetrator")) showAddPenetratorHelpbox = !showAddPenetratorHelpbox;
        }

        void GUI_Orifices()
        {
            //EditorGUILayout.LabelField("<size=15>2.4 Orifices: Configure your orifice options.</size>", s_styleRichtText);
            EditorGUILayout.HelpBox("Orifices are identified by their name & lights.", MessageType.Info);
            foreach (OrificeConfig o in _orifices)
            {
                GUI_Orifice(o);
            }
            _orifices.RemoveAll(o => o.Remove);
            if(_orifices.Any(o => o.AllowTransformEditing))
            {
                EditorGUILayout.HelpBox("Transform should be where your new orifice is and what direction it should point in. " +
                    "I recommend creating an empty GameObject under your renderer and moving it to the correct spot. (Blue arrow should be pointing outwards from the orifice)", MessageType.Info);
            }
            if (GUILayout.Button("Add Custom Orifice")) _orifices.Add(new OrificeConfig());
        }

#if VRC_SDK_VRCSDK3 && !UDON
        void GUI_ResolveAssets()
        {
            VRCAvatarDescriptor d = _avatar.GetComponent<VRCAvatarDescriptor>();
            IEnumerable<CustomAnimLayer> fxlayers = d.baseAnimationLayers.Where(l => l.type == VRCAvatarDescriptor.AnimLayerType.FX && l.animatorController != null);
            if (fxlayers.Count() > 0)
            {
                _animator = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(fxlayers.First().animatorController));
                ScanForTPS();
            }
            else if (GUILayout.Button("Create FX Layer"))
            {
                FindAvatarDirectory();
                if (string.IsNullOrEmpty(_avatarDirectory) == false)
                {
                    string path = _avatarDirectory + "/FX_" + _avatar.name;
                    _animator = AnimatorController.CreateAnimatorControllerAtPathWithClip(UniquePath(path, ".asset"), EmptyClip);
                    _animator.layers[0].stateMachine.states[0].state.writeDefaultValues = false;
                    CustomAnimLayer[] layers = d.baseAnimationLayers;
                    if (layers.Length < 5) Array.Resize<CustomAnimLayer>(ref layers, 5);
                    layers[4] = new CustomAnimLayer() { animatorController = _animator, type = AnimLayerType.FX };
                    d.baseAnimationLayers = layers;
                    d.customizeAnimationLayers = true;
                }
            }
    }
#endif

        void GUI_CheckAnimatorHasWDOn()
        {
            _doesAnimatorHaveWDOn = _animator.layers.Any(l => FindAllStates(l.stateMachine).Any(s => s.writeDefaultValues));
        }

        IEnumerable<AnimatorState> FindAllStates(AnimatorStateMachine m)
        {
            if (m == null) return new AnimatorState[0];
            return m.stateMachines.SelectMany(sm => FindAllStates(sm.stateMachine)).Concat(m.states.Select(s => s.state));
        }

        void GUI_Penetrator(PenetratorConfig p)
        {
            GUI.backgroundColor = Color.black;
            using (new GUILayout.VerticalScope("box"))
            {
                GUI.backgroundColor = _backgroundColor;
                GUILayout.BeginHorizontal();
               
                EditorGUILayout.ObjectField(p.Transform, typeof(Transform), true, GUILayout.Width(150));

                EditorGUILayout.BeginVertical();
                p.TransformTip = EditorGUILayout.ObjectField("Tip", p.TransformTip, typeof(Transform), true) as Transform;
                if (!p.EditMask) if (GUILayout.Button("Edit Mask")) p.EditMask = true;
                if(p.EditMask) p.Mask = EditorGUILayout.ObjectField("Mask", p.Mask, typeof(Texture2D), false) as Texture2D;
                if (p.Renderer is MeshRenderer && p.EditMask &&GUI.Button(EditorGUILayout.GetControlRect(), "Apply Mask"))
                {
                    foreach (Material m in p.Renderer?.sharedMaterials.Where(m => m != null))
                        m.SetTexture("_TPS_BakedMesh", p.Mask);
                }
                if (p.Renderer == null) EditorGUILayout.LabelField("No Renderer");
                else if (!p.HasMesh) EditorGUILayout.LabelField("No Mesh");
                else if (p.Renderer is MeshRenderer) EditorGUILayout.LabelField("Static");
                else if (p.IsBaked && !p.EditMask) EditorGUILayout.LabelField("Is baked");
                else if (GUI.Button(EditorGUILayout.GetControlRect(), "Bake Now"))
                {
                    Texture2D tex = BakeToVertexColors.BakePositionsToTexture(p.Renderer, p.Mask);
                    foreach (Material m in p.Renderer?.sharedMaterials.Where(m => m != null))
                        m.SetTexture("_TPS_BakedMesh", tex);
                    p.SetBaked(true);
                    p.EditMask = false;
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        void GUI_Orifice(OrificeConfig o)
        {
            GUI.backgroundColor = Color.black;
            using (new GUILayout.VerticalScope("box"))
            {
                GUI.backgroundColor = _backgroundColor;
                GUILayout.BeginHorizontal();

                if (o.AllowTransformEditing)
                {
                    o.Transform = EditorGUILayout.ObjectField(o.Transform, typeof(Transform), true, GUILayout.Width(150)) as Transform;
                }
                else 
                { 
                    EditorGUILayout.ObjectField(o.Transform, typeof(Transform), true, GUILayout.Width(150)); 
                }
                EditorGUILayout.BeginVertical();
                EditorGUI.BeginChangeCheck();
                o.OrificeType = (OrificeType)EditorGUILayout.EnumPopup("Type", o.OrificeType);
                if (EditorGUI.EndChangeCheck())
                {
                    o.ConfigureLights();
                }
#if VRC_SDK_VRCSDK3 && !UDON
                o.DoAnimatorSetup = EditorGUILayout.Toggle("Animator Setup (optional)", o.DoAnimatorSetup);
                if (o.DoAnimatorSetup)
                {
                    EditorGUI.BeginChangeCheck();
                    Renderer newR = EditorGUILayout.ObjectField("Renderer", o.Renderer, typeof(Renderer), true) as Renderer;
                    if (EditorGUI.EndChangeCheck()) o.SetRenderer(newR);
                    o.MaxDepth = EditorGUILayout.FloatField("Max Depth", o.MaxDepth);
                    if (o.Renderer != null && o.Renderer is SkinnedMeshRenderer)
                    {
                        using (new GUILayout.VerticalScope("Shapekeys", EditorStyles.helpBox))
                        {
                            GUILayout.Space(15);
                            EditorGUI.BeginChangeCheck();
                            o.BlendShapeIndexEnter = EditorGUILayout.Popup("Entering", o.BlendShapeIndexEnter, o.BlendshapeNames);
                            o.BlendShapeIndexIn = EditorGUILayout.Popup("Full Penetration", o.BlendShapeIndexIn, o.BlendshapeNames);
                            if (EditorGUI.EndChangeCheck()) o.ChangedSelectedShapekeys();
                            o.MaxOpeningWidth = EditorGUILayout.FloatField("Max Orfice Width", o.MaxOpeningWidth);
                            o.ScaleBlendshapesByWidth = EditorGUILayout.Toggle("Scale Blendshapes by Width", o.ScaleBlendshapesByWidth);
                        }
                    }
                }
#endif
                EditorGUILayout.EndVertical();
                GUILayout.EndHorizontal();
                if(o.AllowTransformEditing && GUILayout.Button("Remove", GUILayout.Width(150))) o.Remove = true;
            }
        }

        void GUI_Button_Apply()
        {
            using (new EditorGUI.DisabledScope(false))
            {
                if (GUILayout.Button("Apply"))
                {
                    if (!Directory.Exists(_avatarDirectory + "/TPS_"+_avatar.name)) AssetDatabase.CreateFolder(_avatarDirectory, "TPS_" + _avatar.name);
                    string dir = _avatarDirectory + "/TPS_" + _avatar.name;
                    AssetDatabase.StartAssetEditing();
                    s_debugIndex = 0;
                    try
                    {
                        RemoveTPSFromAnimator();
                        _penetrators = _penetrators.Where(p => p.Transform != null).ToList();
                        _orifices = _orifices.Where(o => o.Transform != null).ToList();
                        for (int i = 0; i < _penetrators.Count; i++)
                        {
                            SetupPenetrator(_avatar, _animator, _penetrators[i], _penetrators, i, dir);
                        }
                        for (int i = 0; i < _orifices.Count; i++)
                        {
                            _orifices[i].ConfigureLights();
                            SetupOrifice(_avatar, _animator, _orifices[i].Transform, _orifices[i].Renderer, _orifices[i].OrificeType, _orifices[i], i, dir);
                        }

                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                    finally
                    {
                        AssetDatabase.StopAssetEditing();
                        AssetDatabase.Refresh();
                        if (_animator != null)
                        {
                            EditorUtility.SetDirty(_animator);
                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_animator));
                        }
                    }
                }
            }
        }

        void GUI_Information()
        {
            EditorGUILayout.LabelField("<size=15>Blendshapes don't move</size>", s_styleRichtText);
            EditorGUILayout.HelpBox("The blendshapes & Buffered depth are driven by Avatar Dynamics Contacts. " +
                "To see its effect in the editor you need Lyumas Avatar Emulator: https://github.com/lyuma/Av3Emulator/releases. Click here to open the link.", MessageType.Info);
            if (Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                Application.OpenURL("https://github.com/lyuma/Av3Emulator/releases");
        }

        void GUI_Buttons_Remove()
        {
#if VRC_SDK_VRCSDK3 && !UDON
            if (GUILayout.Button("Remove TPS From Animator & Physics"))
            {
                RemoveTPSFromAnimator();
                RemoveVRCSendersAndRecievers(_avatar);
            }
#endif
            if (GUILayout.Button("Remove TPS Objects")) _doClear = !_doClear;
            if (_doClear)
            {
                GUILayout.Label("Remove all TPS Objects ? (not reversible)");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Yes"))
                {
                    Transform[] transforms = _penetrators.Select(p => p.Transform).Concat(_orifices.Select(o => o.Transform)).ToArray();
                    foreach (Transform t in transforms)
                    {
                        if (t != null)
                        {
                            try
                            {
                                Undo.DestroyObjectImmediate(t.gameObject);
                            }
                            catch (Exception e)
                            {
                                Debug.Log(e);
                            }
                        }
                    }
                    _doClear = false;
                }
                if (GUILayout.Button("No"))
                {
                    _doClear = false;
                }
                GUILayout.EndHorizontal();
            }
        }

        class Prefab
        {
            public GameObject GameObject;
            public Editor Editor;
            public Texture2D Texture;

            public Prefab(GameObject o)
            {
                GameObject = o;
            }

            public Texture2D GetTexture()
            {
                if (Texture) return Texture;
                if (Application.isPlaying) return null;
                GameObject temp = GameObject.Instantiate(GameObject);
                foreach (SkinnedMeshRenderer smr in temp.GetComponentsInChildren<SkinnedMeshRenderer>())
                    smr.updateWhenOffscreen = true;
                Editor = UnityEditor.Editor.CreateEditor(temp);
                Texture = Editor.RenderStaticPreview(AssetDatabase.GetAssetPath(GameObject), null, 200, 200);
                EditorWindow.DestroyImmediate(Editor);
                EditorWindow.DestroyImmediate(temp);
                return Texture;
            }
        }

        #endregion
        #region Helpers

        void ScanForTPS()
        {
            _penetrators = _avatar.GetComponentsInChildren<Renderer>(true).Where(r => IsRendererPenetrator(r)).Select(
                        r => (PrefabUtility.IsPartOfAnyPrefab(r.gameObject) ? PrefabUtility.GetNearestPrefabInstanceRoot(r.gameObject) : r.gameObject).transform)
                        .Select(t => new PenetratorConfig(t)).ToList();
            //_orifices = _avatar.GetComponentsInChildren<Transform>().Where(t => t.name.StartsWith("[TPS][Orifice]")).Select(t => new OrificeConfig(t)).ToList();
            _orifices = _avatar.GetComponentsInChildren<Renderer>(true).Where(r => r != null).Select(r => GetOrificeRootFromRenderer(r.transform)).Concat(
                _avatar.GetComponentsInChildren<Light>(true).Where(l => l != null).Select(l => GetOrificeRootFromLight(l.transform))).Where(t => t != null).Distinct()
                        .Select(t => new OrificeConfig(t)).ToList();
            FindAvatarDirectory();
        }

        void FindTPSPrefabs()
        {
            _prefabsPenetrator = AssetDatabase.FindAssets("[TPS][Penetrator] t:prefab").Select(g => new Prefab(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(g)))).ToArray();
            _prefabsOrifice = AssetDatabase.FindAssets("[TPS][Orifice] t:prefab").Select(g => new Prefab(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(g)))).ToArray();
        }

        void RemoveTPSFromAnimator()
        {
            AnimatorControllerParameter[] parameters = _animator.parameters;
            parameters = parameters.Where(p => p.name.StartsWith("TPS") == false).ToArray();
            _animator.parameters = parameters;
            AnimatorControllerLayer[] layers = _animator.layers;
            layers = layers.Where(l => l.name.StartsWith("[TPS]") == false).ToArray();
            _animator.layers = layers;
            //Remove state machines from asset
            UnityEngine.Object[] objects = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(_animator));
            foreach (UnityEngine.Object o in objects)
                if (o != null && o.name.StartsWith("[TPS]"))
                    AssetDatabase.RemoveObjectFromAsset(o);
        }

        static bool IsRendererPenetrator(Renderer r)
        {
            if (r == null) return false;
            return r.sharedMaterials.Any(m => m != null && m.HasProperty("_TPSPenetratorEnabled") && m.GetFloat("_TPSPenetratorEnabled") == 1);
        }

        Transform GetOrificeRootFromRenderer(Transform t)
        {
            if (t == null) return null;
            if (t.gameObject.name.StartsWith("[TPS][Orifice]")) return t;
            /*if (t.transform.GetComponentsInChildren<Light>(true).Any(l => l.range == ORF_NORM_RANGE_ID))
            {
                if (PrefabUtility.IsPartOfAnyPrefab(t.gameObject) && PrefabUtility.GetNearestPrefabInstanceRoot(t.gameObject) != _avatar.gameObject)
                    return PrefabUtility.GetNearestPrefabInstanceRoot(t.gameObject).transform;
                return t;
            }*/
            Transform p = t;
            while (p != null)
            {
                if (p.gameObject.name.StartsWith("[TPS][Orifice]")) return p;
                p = p.parent;
            }
            return null;
        }
        
        Transform GetOrificeRootFromLight(Transform t)
        {
            if (t == null) return null;
            float range = t.transform.GetComponent<Light>().range;
            if (range == ORF_HOLE_RANGE_ID || range == ORF_RING_RANGE_ID) return t.parent;
            return null;
        }

        static Mesh GetMesh(Renderer r)
        {
            if (r is SkinnedMeshRenderer)
            {
                SkinnedMeshRenderer smr = (r as SkinnedMeshRenderer);
                /*Mesh bakedMesh = new Mesh();
                Transform tr = r.transform;
                Quaternion origRot = tr.localRotation;
                Vector3 origScale = tr.localScale;

                tr.localRotation = Quaternion.identity;
                tr.localScale = Vector3.one;

                smr.BakeMesh(bakedMesh);

                tr.localRotation = origRot;
                tr.localScale = origScale;

                return bakedMesh;*/
                return smr.sharedMesh;
            }
            if (r is MeshRenderer) return r.transform.GetComponent<MeshFilter>().sharedMesh;
            return null;
        }

        static Transform SingletonChild(Transform parent, string childName)
        {
            //parent.Find does not find the objects in prefabs, so researching manually
            for (int i = 0; i < parent.childCount; i++) if (parent.GetChild(i).name == childName) return parent.GetChild(i);
            Transform t = new GameObject(childName).transform;
            t.parent = parent;
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            return t;
        }

        static bool AreVerteciesBaked(Renderer r)
        {
            MeshInfo meshInfo = GetAllMeshInfos(r)[0];
            Texture tex = r.sharedMaterials.Where(m => m != null).Select(m => m.GetTexture("_TPS_BakedMesh")).FirstOrDefault();
            if (tex == null) return false;
            if (tex is Texture2D == false) return false;
            if (tex.width < 8190) return false;
            Texture2D tex2d = tex as Texture2D;
            Vector3[] vertices = meshInfo.bakedVertices;
            Color32[] colors = tex2d.GetPixels32();
            for (int i = 0; i < vertices.Length; i++)
            {
                if (Mathf.Abs(vertices[i].x - BakeToVertexColors.DecodeFloatFromARGB8(colors[i * 6 + 0])) > 0.000001f) return false;
                if (Mathf.Abs(vertices[i].y - BakeToVertexColors.DecodeFloatFromARGB8(colors[i * 6 + 1])) > 0.000001f) return false;
                if (Mathf.Abs(vertices[i].z - BakeToVertexColors.DecodeFloatFromARGB8(colors[i * 6 + 2])) > 0.000001f) return false;
            }
            return true;
        }

        static void InstanciateMaterials(Transform avatar, Renderer r, string id, string directory, params Renderer[] instanciateIfMaterialReferencedByTheseRenderers)
        {
            if (PrefabUtility.IsPartOfAnyPrefab(r.gameObject) && PrefabUtility.GetNearestPrefabInstanceRoot(r.gameObject).transform != avatar)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(r.gameObject));
                string localPath = AnimationUtility.CalculateTransformPath(r.transform, PrefabUtility.GetNearestPrefabInstanceRoot(r.gameObject).transform);
                Renderer prefabRenderer = prefab.transform.Find(localPath)?.GetComponent<Renderer>();
                if (prefabRenderer != null)
                {
                    IEnumerable<Material> otherMaterials = instanciateIfMaterialReferencedByTheseRenderers.SelectMany(ren => ren.sharedMaterials);
                    Material[] prefabMaterials = prefabRenderer.sharedMaterials;
                    Material[] materials = r.sharedMaterials;
                    for (int i = 0; i < materials.Length; i++)
                    {
                        if (prefabMaterials[i] != materials[i] && otherMaterials.Contains(materials[i]) == false) continue;
                        Material copy = new Material(materials[i]);
                        AssetDatabase.CreateAsset(copy, directory + "/" + id + "_" + copy.name + ".mat");
                        materials[i] = copy;
                    }
                    r.sharedMaterials = materials;
                }
            }
        }

        private static void SetDefineSymbol(string symbol, bool active, bool refresh_if_changed)
        {
            try
            {
                string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                        BuildTargetGroup.Standalone);
                if (!symbols.Contains(symbol) && active)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(
                                  BuildTargetGroup.Standalone, symbols + ";" + symbol);
                    if (refresh_if_changed)
                        AssetDatabase.Refresh();
                }
                else if (symbols.Contains(symbol) && !active)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(
                                  BuildTargetGroup.Standalone, Regex.Replace(symbols, @";?" + @symbol, ""));
                    if (refresh_if_changed)
                        AssetDatabase.Refresh();
                }
            }
            catch (Exception e)
            {
                e.ToString();
            }
        }

        public static float RendererMaxDistanceIntoDirection(Renderer r, Vector3[] vertices, Vector3 localRendererDirection)
        {
            IEnumerable<float> zDistances = vertices.Select(v => Vector3.Dot(v, localRendererDirection));
            return zDistances.Max() * Mathf.Abs(Vector3.Dot(r.transform.lossyScale, localRendererDirection));
        }

        public static Vector3 NearestPointOnLine(Vector3 linePnt, Vector3 lineDir, Vector3 pnt)
        {
            lineDir.Normalize();//this needs to be a unit vector
            var v = pnt - linePnt;
            var d = Vector3.Dot(v, lineDir);
            return linePnt + lineDir * d;
        }

        static Vector3 RoundVector(Vector3 v, float steptsize)
        {
            return new Vector3(
                (v.x + steptsize / 2) - (v.x + steptsize / 2) % steptsize,
                (v.y + steptsize / 2) - (v.y + steptsize / 2) % steptsize,
                (v.z + steptsize / 2) - (v.z + steptsize / 2) % steptsize);
        }

        #endregion

        [InitializeOnLoad]
        public class OnCompileHandler
        {
            static OnCompileHandler()
            {
                //Init Editor Variables with paths
                SetDefineSymbol("TPS", true, true);
            }
        }

#region Penetrator

        const string CONTACT_ORF_ROOT = "TPS_Orf_Root";
        const string CONTACT_ORF_NORM = "TPS_Orf_Norm";

        const string CONTACT_PEN_PENETRATING = "TPS_Pen_Penetrating";
        const string CONTACT_PEN_WIDTH = "TPS_Pen_Width";

        static int s_debugIndex = 0;
        const float TPS_RECIEVER_DIST = 0.01f;
        public static void SetupPenetrator(Transform avatar, AnimatorController animator, PenetratorConfig penetrator, List<PenetratorConfig> allPenetrators, int index, string directory, bool placeContacts = true, bool instanciateMaterials = true, bool configureMaterial = true, string pathSenderIsPenetrating = null, string pathSenderWidth = null)
        {
#if VRC_SDK_VRCSDK3 && !UDON
            //Remove senders, recievers
            if (placeContacts) RemoveVRCSendersAndRecievers(penetrator.Transform);
            //Get renderer & mesh
            Renderer r = penetrator.Transform.GetComponentInChildren<Renderer>();
            Transform rotationTransform = r.transform;
            bool isSMR = r is SkinnedMeshRenderer;
            // Find armature transform
            if (isSMR)
            {
                rotationTransform = GetArmatureTransform(r as SkinnedMeshRenderer);
                // Set root bone to armeture
                (r as SkinnedMeshRenderer).rootBone = rotationTransform;
            }
            // get Forward vector 
            Vector3 forward = r.sharedMaterials.Where(m => m.HasProperty("_TPS_PenetratorForward")).Select(m => m.GetVector("_TPS_PenetratorForward")).FirstOrDefault();
            Vector3 right = r.sharedMaterials.Where(m => m.HasProperty("_TPS_PenetratorRight")).Select(m => m.GetVector("_TPS_PenetratorRight")).FirstOrDefault();
            Vector3 up = r.sharedMaterials.Where(m => m.HasProperty("_TPS_PenetratorUp")).Select(m => m.GetVector("_TPS_PenetratorUp")).FirstOrDefault();
            if (penetrator.TransformTip)
            {
                Vector3 worldForward = (penetrator.TransformTip.position - penetrator.Transform.position).normalized;
                Vector3 worldRight = penetrator.TransformTip.right;
                if (Vector3.Dot(worldForward, Vector3.up) < 1) worldRight = Vector3.Cross(Vector3.up, worldForward).normalized;

                forward = (rotationTransform.transform.worldToLocalMatrix * worldForward).normalized;
                right = (rotationTransform.transform.worldToLocalMatrix * worldRight).normalized;
                up = (Vector3.Cross(forward, right)).normalized;
            }
            forward = RoundVector(forward, 0.0001f);
            right = RoundVector(right, 0.0001f);
            up = RoundVector(up, 0.0001f);
            // Instanciate material
            if (instanciateMaterials) InstanciateMaterials(avatar, r, "Pen" + index, directory, allPenetrators.Where(p => p != penetrator && p.Renderer).Select(p => p.Renderer).ToArray());
            Mesh mesh = GetMesh(r); ;
            if (mesh == null)
            {
                Debug.LogError("[TPS][SetupPenetrator] Mesh is null.");
                return;
            }
            //Calc length
            float length = RendererMaxDistanceIntoDirection(r, mesh.vertices, forward);
            float lengthBack = RendererMaxDistanceIntoDirection(r, mesh.vertices, -forward);
            float width = mesh.vertices.Select(v => v - NearestPointOnLine(Vector3.zero, forward, v)).
                Select(v => (v * Mathf.Abs(Vector3.Dot(r.transform.lossyScale, v.normalized))).magnitude).Average() * 2;
            //Configure material
            if (configureMaterial)
            {
                foreach (Material m in r.sharedMaterials)
                {
                    m.EnableKeyword("TPS_Penetrator");
                    m.SetFloat("_TPSPenetratorEnabled", 1);
                    m.SetFloat("_TPS_PenetratorLength", length);
                    m.SetVector("_TPS_PenetratorScale", r.transform.lossyScale);
                    m.SetVector("_TPS_PenetratorForward", forward);
                    m.SetVector("_TPS_PenetratorRight", right);
                    m.SetVector("_TPS_PenetratorUp", up);
                }
            }
            // Setup bounds
            if (r is SkinnedMeshRenderer)
            {
                SkinnedMeshRenderer smr = r as SkinnedMeshRenderer;
                //Fix bounding box (if bounding box is too small, penetrator will not get light data early enough
                Bounds bounds = smr.localBounds;
                bounds.Encapsulate(new Vector3(2 * length / r.transform.lossyScale.x, 0, 0));
                bounds.Encapsulate(new Vector3(-2 * length / r.transform.lossyScale.x, 0, 0));
                bounds.Encapsulate(new Vector3(0, 2 * length / r.transform.lossyScale.y, 0));
                bounds.Encapsulate(new Vector3(0, -2 * length / r.transform.lossyScale.y, 0));
                bounds.Encapsulate(new Vector3(0, 0, 2 * length / r.transform.lossyScale.z));
                smr.localBounds = bounds;
                smr.updateWhenOffscreen = false;
            }  

            //parameter names
            string paramFloatRootRoot = "TPS_Internal/Pen/" + index + "/RootRoot";
            string paramFloatRootFrwd = "TPS_Internal/Pen/" + index + "/RootForw";
            string paramFloatBackRoot = "TPS_Internal/Pen/" + index + "/BackRoot";

            string paramFloatComp1 =    "TPS_Internal/Pen/" + index + "/Comp1";
            string paramFloatComp2 =    "TPS_Internal/Pen/" + index + "/Comp2";

            string paramBlendToCurrentDepth = "TPS_Internal/Pen/" + index + "/BlendToDepthVelocity";

            string paramFloatBufferedDepth =    "TPS_Pen_" + index + "_BufferedDepth";
            string paramFloatBufferedStrength = "TPS_Pen_" + index + "_BufferedDepthStrength";
            string paramIsPenetrating =         "TPS_Pen_" + index + "_IsPenetrating";

            //Add contacts
            if (placeContacts)
            {
                pathSenderIsPenetrating = AnimationUtility.CalculateTransformPath(
                    PlaceVRCContactSenderOnChildObject(penetrator.Transform, Vector3.zero, length, CONTACT_PEN_PENETRATING), avatar);
                pathSenderWidth = AnimationUtility.CalculateTransformPath(
                    PlaceVRCContactSenderOnChildObject(penetrator.Transform, Vector3.zero, Mathf.Max(0.01f, length - width), CONTACT_PEN_WIDTH), avatar);
                // Orf -> Pen: Orientation
                PlaceVRCContactReceiverProximityOnChildObject(animator, penetrator.Transform, Vector3.back * 0, paramFloatRootRoot, length, CONTACT_ORF_ROOT);
                PlaceVRCContactReceiverProximityOnChildObject(animator, penetrator.Transform, Vector3.back * 0, paramFloatRootFrwd, length, CONTACT_ORF_NORM);
                PlaceVRCContactReceiverProximityOnChildObject(animator, penetrator.Transform, Vector3.back * 0.01f, paramFloatBackRoot, length, CONTACT_ORF_ROOT);
                // pen root pointed correctly && pen root in front of orf root && Collision
                // TPS_Pen_RootRoot_ > TPS_Pen_RootForw_ && TPS_Pen_BackRoot_ > TPS_Pen_RootRoot_ && TPS_Pen_Collision_
            }

            CreateTwoParameterComparissionLayer(animator, "[TPS][Pen" + index + "] 1/3", paramFloatRootRoot, paramFloatRootFrwd, paramFloatComp1, "TPS/Pen/" + index + "/Comp1/", directory, "Pen_" + index + "_Comp1");
            CreateTwoParameterComparissionLayer(animator, "[TPS][Pen" + index + "] 2/3", paramFloatBackRoot, paramFloatRootRoot, paramFloatComp2, "TPS/Pen/" + index + "/Comp2/", directory, "Pen_" + index + "_Comp2");

            string rendererPath = AnimationUtility.CalculateTransformPath(r.transform, avatar);

            AnimatorControllerLayer bufferLayer = SingletonLayer(animator, "[TPS][Pen" + index + "] 3/3", true);

            AddParameter(animator, paramFloatBufferedDepth, AnimatorControllerParameterType.Float);
            AddParameter(animator, paramFloatBufferedStrength, AnimatorControllerParameterType.Float);
            AddParameter(animator, paramIsPenetrating, AnimatorControllerParameterType.Bool);
            AddParameter(animator, paramBlendToCurrentDepth, AnimatorControllerParameterType.Float, 0.01f);

            Func<string, bool, bool, bool, AnimationClip> makeBufferClip = (string name, bool penetrating, bool depth1, bool strength1) =>
                {
                    return CreateClip("TPS/Pen/" + index + "/Buffer/" + name,
                    new CurveConfig(pathSenderIsPenetrating, "m_Enabled", typeof(VRCContactSender), penetrating ? OnCurve : OffCurve),
                    new CurveConfig(pathSenderWidth, "m_Enabled", typeof(VRCContactSender), penetrating ? OnCurve : OffCurve),
                    new CurveConfig("", paramFloatBufferedDepth, typeof(Animator), depth1 ? OnCurve : OffCurve),
                    new CurveConfig("", paramFloatBufferedStrength, typeof(Animator), strength1 ? OnCurve : OffCurve),
                    new CurveConfig(rendererPath, "material._TPS_BufferedDepth", typeof(Renderer), depth1 ? OnCurve : OffCurve),
                    new CurveConfig(rendererPath, "material._TPS_BufferedStrength", typeof(Renderer), strength1 ? OnCurve : OffCurve),
                    new CurveConfig(rendererPath, "m_UpdateWhenOffscreen", typeof(Renderer), OffCurve)); //Disable UpdateWhenOffscreen to keep custom boudning box for local player (vrc changes value on load. local -> on, remote ->off)
                };

            AnimationClip depth0Strength0True = makeBufferClip("True_00", true, false, false);
            AnimationClip depth0Strength1True = makeBufferClip("True_01", true, false, true);
            AnimationClip depth1Strength0True = makeBufferClip("True_10", true, true, false);
            AnimationClip depth1Strength1True = makeBufferClip("True_11", true, true, true);

            AnimationClip depth0Strength0False = makeBufferClip("False_00", false, false, false);
            AnimationClip depth0Strength1False = makeBufferClip("False_01", false, false, true);
            AnimationClip depth1Strength0False = makeBufferClip("False_10", false, true, false);
            AnimationClip depth1Strength1False = makeBufferClip("False_11", false, true, true);

            //when penetrating:
            //Max Tree containing: 
            //  currentDepthIncStrength => Depth : Depth , Strength : Blend Up
            //  bufferDepthIncStrength  => Depth : Buffer, Strength : Blend Up
            BlendTree currentDepthStrength0 = new BlendTree()
            {
                name = "Depth = Current, Strength = 0",
                blendParameter = paramFloatRootRoot,
                useAutomaticThresholds = false,
                children = new ChildMotion[]
                {
                    new ChildMotion(){ motion = depth0Strength0True, threshold = 0, timeScale = 1 },
                    new ChildMotion(){ motion = depth1Strength0True, threshold = 1, timeScale = 1 },
                }
            };
            BlendTree currentDepthStrength1 = new BlendTree()
            {
                name = "Depth = Current, Strength = 1",
                blendParameter = paramFloatRootRoot,
                useAutomaticThresholds = false,
                children = new ChildMotion[]
                {
                    new ChildMotion(){ motion = depth0Strength1True, threshold = 0, timeScale = 1 },
                    new ChildMotion(){ motion = depth1Strength1True, threshold = 1, timeScale = 1 },
                }
            };
            BlendTree currentDepthIncStrength = new BlendTree()
            {
                name = "Depth = Current, Strength = Strength + 1",
                blendParameter = paramFloatBufferedStrength,
                useAutomaticThresholds = false,
                children = new ChildMotion[] {
                    new ChildMotion(){ motion = currentDepthStrength0, threshold = -0.01f, timeScale = 1 },
                    new ChildMotion(){ motion = currentDepthStrength1, threshold = 0.99f, timeScale = 1 },
                }
            };
            BlendTree bufferDepthStrength0Penetrating = new BlendTree()
            {
                name = "Depth = Buffer, Strength = 0",
                blendParameter = paramFloatBufferedDepth,
                useAutomaticThresholds = false,
                children = new ChildMotion[]
                {
                    new ChildMotion(){ motion = depth0Strength0True, threshold = 0, timeScale = 1 },
                    new ChildMotion(){ motion = depth1Strength0True, threshold = 1, timeScale = 1 },
                }
            };
            BlendTree bufferDepthStrength0Outside = new BlendTree()
            {
                name = "Depth = Buffer, Strength = 0",
                blendParameter = paramFloatBufferedDepth,
                useAutomaticThresholds = false,
                children = new ChildMotion[]
                {
                    new ChildMotion(){ motion = depth0Strength0False, threshold = 0, timeScale = 1 },
                    new ChildMotion(){ motion = depth1Strength0False, threshold = 1, timeScale = 1 },
                }
            };
            BlendTree bufferDepthStrength1Penetrating = new BlendTree()
            {
                name = "Depth = Buffer, Strength = 1",
                blendParameter = paramFloatBufferedDepth,
                useAutomaticThresholds = false,
                children = new ChildMotion[]
                {
                    new ChildMotion(){ motion = depth0Strength1True, threshold = 0, timeScale = 1 },
                    new ChildMotion(){ motion = depth1Strength1True, threshold = 1, timeScale = 1 },
                }
            };
            BlendTree bufferDepthStrength1Outside = new BlendTree()
            {
                name = "Depth = Buffer, Strength = 1",
                blendParameter = paramFloatBufferedDepth,
                useAutomaticThresholds = false,
                children = new ChildMotion[]
                {
                    new ChildMotion(){ motion = depth0Strength1False, threshold = 0, timeScale = 1 },
                    new ChildMotion(){ motion = depth1Strength1False, threshold = 1, timeScale = 1 },
                }
            };
            BlendTree bufferDepthIncStrength = new BlendTree()
            {
                name = "Depth = Buffer, Strength = Strength + 1",
                blendParameter = paramFloatBufferedStrength,
                useAutomaticThresholds = false,
                children = new ChildMotion[] {
                    new ChildMotion(){ motion = bufferDepthStrength0Penetrating, threshold = -0.01f, timeScale = 1 },
                    new ChildMotion(){ motion = bufferDepthStrength1Penetrating, threshold = 0.99f, timeScale = 1 },
                }
            };
            BlendTree bufferDepthDecStrength = new BlendTree()
            {
                name = "Depth = Current, Strength = Strength + 1",
                blendParameter = paramFloatBufferedStrength,
                useAutomaticThresholds = false,
                children = new ChildMotion[] {
                    new ChildMotion(){ motion = bufferDepthStrength0Outside, threshold = 0.001f, timeScale = 1 },
                    new ChildMotion(){ motion = bufferDepthStrength1Outside, threshold = 1.001f, timeScale = 1 },
                }
            };
            BlendTree bufferSlowlyBlendToCurrentDepthIncStrength = new BlendTree()
            {
                name = "Depth = Blend to Current, Strength = Inc",
                blendParameter = paramBlendToCurrentDepth,
                useAutomaticThresholds = false,
                children = new ChildMotion[] {
                    new ChildMotion(){ motion = bufferDepthIncStrength,  threshold = 0, timeScale = 1 },
                    new ChildMotion(){ motion = currentDepthIncStrength, threshold = 1, timeScale = 1 },
                }
            };
            //if current depth is bigger than buffer use current depth
            //else use buffer, but slowly blend down to current depth
            BlendTree maxDepthBuffer = new BlendTree()
            {
                name = "Max(Depth,Buffer)",
                blendType = BlendTreeType.FreeformCartesian2D,
                blendParameter = paramFloatRootRoot,
                blendParameterY = paramFloatBufferedDepth,
                useAutomaticThresholds = false,
                children = new ChildMotion[] {
                    new ChildMotion(){ motion = currentDepthIncStrength, position = new Vector2(1.000f,0.000f), timeScale = 1 },
                    new ChildMotion(){ motion = currentDepthIncStrength, position = new Vector2(0.001f,0.000f), timeScale = 1 },
                    new ChildMotion(){ motion = currentDepthIncStrength, position = new Vector2(0.501f,0.499f), timeScale = 1 },
                    new ChildMotion(){ motion = currentDepthIncStrength, position = new Vector2(1.000f,0.999f), timeScale = 1 },

                    new ChildMotion(){ motion = bufferSlowlyBlendToCurrentDepthIncStrength , position = new Vector2(0.000f,1.000f), timeScale = 1 },
                    new ChildMotion(){ motion = bufferSlowlyBlendToCurrentDepthIncStrength , position = new Vector2(0.000f,0.001f), timeScale = 1 },
                    new ChildMotion(){ motion = bufferSlowlyBlendToCurrentDepthIncStrength , position = new Vector2(0.499f,0.501f), timeScale = 1 },
                    new ChildMotion(){ motion = bufferSlowlyBlendToCurrentDepthIncStrength , position = new Vector2(0.999f,1.000f), timeScale = 1 },
                }
            };

            //AnimatorState stateNotPenetrating = CreateState("Not Penetrating", bufferLayer, depth0Strength0False);
            AnimatorState statePenetration = CreateState("Penetration", bufferLayer, maxDepthBuffer);
            AnimatorState stateNotPenetrating = CreateState("No Penetration", bufferLayer, bufferDepthDecStrength);

            CreateTransition(stateNotPenetrating, statePenetration, new Condition(paramFloatComp1, CompareType.GREATER, 0), new Condition(paramFloatComp2, CompareType.GREATER, 0), new Condition(paramFloatRootRoot, CompareType.GREATER, 0));
            //CreateTransition(decreaseOutside, stateNotPenetrating, new Condition(paramFloatComp1, CompareType.GREATER, 0), new Condition(paramFloatComp2, CompareType.GREATER, 0), new Condition(paramFloatRootRoot, CompareType.GREATER, 0));
            CreateTransition(statePenetration, stateNotPenetrating, new Condition(paramFloatComp1, CompareType.LESS, 0.001f));
            CreateTransition(statePenetration, stateNotPenetrating, new Condition(paramFloatComp2, CompareType.LESS, 0.001f));
            CreateTransition(statePenetration, stateNotPenetrating, new Condition(paramFloatRootRoot, CompareType.LESS, 0.001f));
            bufferLayer.stateMachine.defaultState = stateNotPenetrating;

            AddParameterDriver(stateNotPenetrating, (paramIsPenetrating, ChangeType.Set, 0));
            //AddParameterDriver(decreaseOutside, (paramIsPenetrating, ChangeType.Set, 0));
            AddParameterDriver(statePenetration, (paramIsPenetrating, ChangeType.Set, 1));

            AssetDatabase.CreateAsset(bufferDepthDecStrength, directory + "/Pen_" + index + "_DepthBlendTree.asset");
            AssetDatabase.AddObjectToAsset(depth0Strength0False, bufferDepthDecStrength);
            AssetDatabase.AddObjectToAsset(depth0Strength1False, bufferDepthDecStrength);
            AssetDatabase.AddObjectToAsset(depth1Strength0False, bufferDepthDecStrength);
            AssetDatabase.AddObjectToAsset(depth1Strength1False, bufferDepthDecStrength);
            AssetDatabase.AddObjectToAsset(depth0Strength0True, bufferDepthDecStrength);
            AssetDatabase.AddObjectToAsset(depth0Strength1True, bufferDepthDecStrength);
            AssetDatabase.AddObjectToAsset(depth1Strength0True, bufferDepthDecStrength);
            AssetDatabase.AddObjectToAsset(depth1Strength1True, bufferDepthDecStrength);

            AssetDatabase.AddObjectToAsset(currentDepthStrength0, bufferDepthDecStrength);
            AssetDatabase.AddObjectToAsset(currentDepthStrength1, bufferDepthDecStrength);
            AssetDatabase.AddObjectToAsset(bufferDepthStrength0Penetrating, bufferDepthDecStrength);
            AssetDatabase.AddObjectToAsset(bufferDepthStrength0Outside, bufferDepthDecStrength);
            AssetDatabase.AddObjectToAsset(bufferDepthStrength1Penetrating, bufferDepthDecStrength);
            AssetDatabase.AddObjectToAsset(bufferDepthStrength1Outside, bufferDepthDecStrength);
            AssetDatabase.AddObjectToAsset(bufferDepthIncStrength, bufferDepthDecStrength);
            AssetDatabase.AddObjectToAsset(currentDepthIncStrength, bufferDepthDecStrength);
            AssetDatabase.AddObjectToAsset(bufferSlowlyBlendToCurrentDepthIncStrength, bufferDepthDecStrength);

            AssetDatabase.AddObjectToAsset(maxDepthBuffer, bufferDepthDecStrength);
            AssetDatabase.ImportAsset(directory + "/Pen_" + index + "_DepthBlendTree.asset");
#endif
        }

#endregion
#region Orifice

        public static void SetupOrifice(Transform avatar, AnimatorController animator, Transform orifice, Renderer renderer, OrificeType type, OrificeConfig config, int index, string directory, bool placeContacts = true, bool instanciateMaterials = true)
        {
#if VRC_SDK_VRCSDK3 && !UDON
            if (placeContacts)
            {
                // Remove senders, recievers
                RemoveVRCSendersAndRecievers(orifice);

                // Orf -> Pen: Position, Normal for shader 
                // Orf -> Pen: Penetrating
                PlaceVRCContactSenderOnChildObject(orifice, Vector3.forward * 0.00f, 0.01f, CONTACT_ORF_ROOT);
                PlaceVRCContactSenderOnChildObject(orifice, Vector3.forward * 0.01f, 0.01f, CONTACT_ORF_NORM);

            }

            if (renderer == null || config.DoAnimatorSetup == false) return;
            //Get renderer & mesh
            string rendererPath = AnimationUtility.CalculateTransformPath(renderer.transform, avatar);
            //Instanciate material
            if (instanciateMaterials) InstanciateMaterials(avatar, renderer, "Orf" + index, directory);
            //Calc depth
            

            //Parameter names
            string paramDepthIn =  "TPS_Internal/Orf/" + index + "/Depth_In";
            string paramWidth1In = "TPS_Internal/Orf/" + index + "/Width1_In";
            string paramWidth2In = "TPS_Internal/Orf/" + index + "/Width2_In";
            string paramDepth =         "TPS_Orf_" + index + "_Depth";
            string paramWidth =         "TPS_Orf_" + index + "_Width";
            string paramIsPenetrating = "TPS_Orf_" + index + "_IsPenetrated";

            // Pen -> Orf: Penetrating
            PlaceVRCContactReceiverProximityOnChildObject(animator, orifice, Vector3.back * config.MaxDepth, paramDepthIn, config.MaxDepth, CONTACT_PEN_PENETRATING);

            // Pen -> Orf: Width
            float widthRecieverRadius = config.MaxOpeningWidth + 0.2f;
            PlaceVRCContactReceiverProximityOnChildObject(animator, orifice, Vector3.back * widthRecieverRadius * 0.5f, paramWidth1In, widthRecieverRadius, CONTACT_PEN_PENETRATING);
            PlaceVRCContactReceiverProximityOnChildObject(animator, orifice, Vector3.back * widthRecieverRadius * 0.5f, paramWidth2In, widthRecieverRadius, CONTACT_PEN_WIDTH);

            //Layer width calculation
            AnimatorControllerLayer widthLayer = SingletonLayer(animator, "[TPS][Orf" + index + "] 1/2", true);
            AddParameter(animator, paramWidth, AnimatorControllerParameterType.Float);

            AnimationClip widthZero = CreateClip("TPS/Orf/" + index + "/Width/Zero", new CurveConfig("", paramWidth, typeof(Animator), OffCurve));
            AnimationClip widthPos = CreateClip("TPS/Orf/" + index + "/Width/Pos", new CurveConfig("", paramWidth, typeof(Animator), OnCurve));
            AnimationClip widthNeg = CreateClip("TPS/Orf/" + index + "/Width/Neg", new CurveConfig("", paramWidth, typeof(Animator), FloatCurve(-1,1)));
            AnimationClip widthZeroToOne = CreateClip("TPS/Orf/" + index + "/Width/ZeroToOne", new CurveConfig("", paramWidth, typeof(Animator), AnimationCurve.Linear(0,0,1,1)));
            BlendTree subtracvtioTree = new BlendTree()
            {
                blendParameter = paramWidth1In,
                blendParameterY = paramWidth2In,
                useAutomaticThresholds = false,
                blendType = BlendTreeType.SimpleDirectional2D,
                children = new ChildMotion[]{
                    new ChildMotion(){ motion = widthZero, timeScale = 1, position = new Vector2(0 , 0) },
                    new ChildMotion(){ motion = widthZero, timeScale = 1, position = new Vector2(1 , 1) },
                    new ChildMotion(){ motion = widthPos, timeScale = 1, position = new Vector2(1 , 0) },
                    new ChildMotion(){ motion = widthNeg, timeScale = 1, position = new Vector2(0 , 1) },
                }
            };
            SaveBlendTree(subtracvtioTree, directory, "Orf_" + index + "_width", false, widthZeroToOne);

            AnimatorState hasntBeenColliding = CreateState("No Pen", widthLayer, widthZero, true);
            AnimatorState waitForNoCollisions = CreateState("Buffer", widthLayer, widthZeroToOne);
            waitForNoCollisions.timeParameterActive = true;
            waitForNoCollisions.timeParameter = paramWidth;
            AnimatorState calcWidth = CreateState("Calc", widthLayer, subtracvtioTree);
            CreateTransition(hasntBeenColliding, calcWidth, new Condition(paramDepthIn, CompareType.GREATER, 0));
            CreateTransition(waitForNoCollisions, hasntBeenColliding, new Condition(paramDepthIn, CompareType.LESS, 0.01f));
            CreateTransition(calcWidth, hasntBeenColliding, new Condition(paramDepthIn, CompareType.LESS, 0.01f));
            CreateTransition(calcWidth, waitForNoCollisions, new Condition(paramWidth, CompareType.GREATER, 0), new Condition(paramWidth1In, CompareType.GREATER, 0), new Condition(paramWidth2In, CompareType.GREATER, 0));

            AddParameter(animator, paramIsPenetrating, AnimatorControllerParameterType.Bool);
            AddParameterDriver(hasntBeenColliding, (paramIsPenetrating, ChangeType.Set, 0));
            AddParameterDriver(calcWidth, (paramIsPenetrating, ChangeType.Set, 1));

            //DebugLayer(paramWidth1In, renderer, "_TPSWidth1", directory, "DebugWidth1" + index);
            //DebugLayer(paramWidth2In, renderer, "_TPSWidth2", directory, "DebugWidth2" + index);
            //DebugLayer(paramWidth, renderer, "_TPSWidthDebug", directory, "DebugWidth" + index);

            //Orifice Depth layer. Length TPS_Orf_RootTip_, condition check
            AnimatorControllerLayer depthLayer = SingletonLayer(animator, "[TPS][Orf" + index + "] 2/2", true);
            AddParameter(animator, paramDepth, AnimatorControllerParameterType.Float);

            Func<string, float, float, float, AnimationClip> PenAnim = (string name,float depth, float blend1, float blend2) =>
               {
                   return CreateClip(name, new CurveConfig("", paramDepth, typeof(Animator), CustomCurve((1, depth))),
                    new CurveConfig(rendererPath, "blendShape." + config.GetBlendshapeNameEnter(), typeof(SkinnedMeshRenderer), CustomCurve((1, blend1))),
                    new CurveConfig(rendererPath, "blendShape." + config.GetBlendshapeNameIn(), typeof(SkinnedMeshRenderer), CustomCurve((1, blend2))));
               };
            
            AnimationClip startNoWidth = PenAnim("TPS/Orf/" + index + "/Blend/00", 0, 0, 0);
            AnimationClip startFullWidth = PenAnim("TPS/Orf/" + index + "/Blend/01", 0, 100, 0);
            AnimationClip endNoWidth = PenAnim("TPS/Orf/" + index + "/Blend/10", 1, 0, 0);
            AnimationClip endFullWidth = PenAnim("TPS/Orf/" + index + "/Blend/11", 1, 0, 100);
            float maxWidthThreshold = 1 - 0.2f / (0.2f + config.MaxOpeningWidth); 
            BlendTree in000 = new BlendTree()
            {
                name = "No Depth",
                blendParameter = paramWidth,
                useAutomaticThresholds = false,
                children = new ChildMotion[]{
                    new ChildMotion() { motion = startNoWidth, timeScale = 1, threshold = 0 },
                    new ChildMotion() { motion = startNoWidth, timeScale = 1, threshold = maxWidthThreshold }
                }
            };
            BlendTree in005 = new BlendTree()
            {
                name = "Sligh Depth",
                blendParameter = paramWidth,
                useAutomaticThresholds = false,
                children = new ChildMotion[]{
                    new ChildMotion() { motion = config.ScaleBlendshapesByWidth ? startNoWidth : startFullWidth, timeScale = 1, threshold = 0 },
                    new ChildMotion() { motion = startFullWidth, timeScale = 1, threshold = maxWidthThreshold }
                }
            };
            BlendTree in100 = new BlendTree()
            {
                name = "Full Depth",
                blendParameter = paramWidth,
                useAutomaticThresholds = false,
                children = new ChildMotion[]{
                    new ChildMotion() { motion = config.ScaleBlendshapesByWidth ? endNoWidth : endFullWidth, timeScale = 1, threshold = 0 },
                    new ChildMotion() { motion = endFullWidth, timeScale = 1, threshold = maxWidthThreshold }
                }
            };
            BlendTree penetrationTree = new BlendTree()
            {
                blendParameter = paramDepthIn,
                useAutomaticThresholds = false,
                children = new ChildMotion[]{
                    new ChildMotion() { motion = in000, timeScale = 1, threshold = 0 },
                    new ChildMotion() { motion = in005, timeScale = 1, threshold = 0.05f },
                    new ChildMotion() { motion = in100, timeScale = 1, threshold = 1 }
                }
            };
            SaveBlendTree(penetrationTree, directory, "Orf_" + index + "_0", true);

            AnimatorState penetration = CreateState("Penetrated", depthLayer, penetrationTree);
            AnimatorState noPenetration = CreateState("No Penetration", depthLayer, startNoWidth);

            depthLayer.stateMachine.defaultState = noPenetration;

            CreateTransition(noPenetration, penetration, new Condition(paramIsPenetrating, CompareType.EQUAL, true));
            CreateTransition(penetration, noPenetration, new Condition(paramIsPenetrating, CompareType.EQUAL, false));

            //DebugLayer(paramDepthIn, renderer, "_TPSWidthDebugDepth1", directory, "DebugDpeth1" +index);
            //DebugLayer(paramDepth, renderer, "_TPSWidthDebugDepth2", directory, "DebugDepth2" +index);
#endif
        }

#endregion
#region Animator Functions

        static void CreateTwoParameterComparissionLayer(AnimatorController animator, string layername, string paramName1, string paramName2, string output, string clipNamePrefix, string directory, string fileName)
        {
            AnimatorControllerLayer layer = SingletonLayer(animator, layername, true);

            AddParameter(animator, output, AnimatorControllerParameterType.Float);
            string at = output;
            Type t = typeof(Animator);

            BlendTree tree = new BlendTree();
            tree.blendType = BlendTreeType.FreeformCartesian2D;
            tree.useAutomaticThresholds = false;

            AnimationClip zero = CreateClip(clipNamePrefix + "Zero", new CurveConfig("", at, t, OffCurve));
            tree.AddChild(zero, new Vector2(0, 0));
            tree.AddChild(CreateClip(clipNamePrefix + "Neg", new CurveConfig("", at, t, FloatCurve(-1, 1))), new Vector2(1, 0));
            tree.AddChild(CreateClip(clipNamePrefix + "Pos", new CurveConfig("", at, t, FloatCurve(1, 1))), new Vector2(0, 1));
            tree.AddChild(zero, new Vector2(1, 1));
            tree.blendParameter = paramName1;
            tree.blendParameterY = paramName2;

            SaveBlendTree(tree, directory, fileName);
            CreateState("BlendTree", layer, tree, true);
        }

        AnimatorState CreateTwoParameterAbsoluteDifferenceLayer(AnimatorControllerLayer layer, string paramName1, string paramName2, string output, string directory, string fileName, params CurveConfig[] additionalTargets)
        {
            AddParameter(_animator, output, AnimatorControllerParameterType.Float);
            string at = output;
            Type t = typeof(Animator);

            BlendTree tree = new BlendTree();
            tree.blendType = BlendTreeType.FreeformCartesian2D;
            tree.useAutomaticThresholds = false;

            AnimationClip clip0 = CreateClip("zero", additionalTargets.Select(c => new CurveConfig(c.Path, c.Attribute, c.Type, OffCurve)).Append(new CurveConfig("", at, t, OffCurve)).ToArray());
            AnimationClip clip1 = CreateClip("one", additionalTargets.Select(c => new CurveConfig(c.Path, c.Attribute, c.Type, c.Curve)).Append(new CurveConfig("", at, t, FloatCurve(1, 1))).ToArray());

            tree.AddChild(clip0, new Vector2(0, 0));
            tree.AddChild(clip1, new Vector2(1, 0));
            tree.AddChild(clip1, new Vector2(0, 1));
            tree.AddChild(clip0, new Vector2(1, 1));
            tree.blendParameter = paramName1;
            tree.blendParameterY = paramName2;

            SaveBlendTree(tree, directory, fileName);
            return CreateState("BlendTree", layer, tree, true);
        }

        void DebugLayer(string param, Renderer renderer, string materialProp, string directory, string name)
        {
            AnimatorControllerLayer layer = new AnimatorControllerLayer()
            {
                name = "[TPS] Debug-" + name,
                stateMachine = new AnimatorStateMachine() { name = "[TPS] Debug" },
                defaultWeight = 1
            };
            _animator.AddLayer(layer);
            AssetDatabase.AddObjectToAsset(layer.stateMachine,
                        AssetDatabase.GetAssetPath(_animator));
            string path = AnimationUtility.CalculateTransformPath(renderer.transform, _avatar);
            BlendTree tree = new BlendTree()
            {
                blendParameter = param,
                useAutomaticThresholds = false,
                children = new ChildMotion[]
                {
                    new ChildMotion(){ motion = CreateClip("DebugNeg", new CurveConfig(path, "material."+materialProp, typeof(Renderer), FloatCurve(-1,1))), threshold = -1, timeScale = 1 },
                    new ChildMotion(){ motion = CreateClip("DebugPos" , new CurveConfig(path, "material."+materialProp, typeof(Renderer), OnCurve )), threshold = 1, timeScale = 1 },
                }
            };
            AnimatorState s = new AnimatorState() { name = "Debug", motion = tree, writeDefaultValues = false };
            layer.stateMachine.AddState(s, new Vector3(300, 100));
            layer.stateMachine.defaultState = s;
            SaveBlendTree(tree, directory, "Debug" + (s_debugIndex++));
        }

        public static void SaveBlendTree(BlendTree tree, string directory, string name, bool deepTrees = false, params AnimationClip[] clips)
        {
            string path = directory + "/" + name + ".asset";
            AssetDatabase.CreateAsset(tree, path);

            List<AnimationClip> allClips = new List<AnimationClip>();
            if (clips != null && clips.Length > 0)
            {
                allClips.AddRange(clips);
            }
            allClips.AddRange(tree.children.Select(c => c.motion).Where(m => m is AnimationClip && m != null).Select(m => m as AnimationClip));
            if (deepTrees)
            {
                List<BlendTree> allTrees = new List<BlendTree>();
                GatherAllSubtrees(tree, allTrees);
                foreach (BlendTree t in allTrees.Distinct())
                {
                    AssetDatabase.AddObjectToAsset(t, path);
                    allClips.AddRange(t.children.Select(c => c.motion).Where(m => m is AnimationClip && m != null).Select(m => m as AnimationClip));
                }
            }

            foreach (AnimationClip c in allClips.Distinct())
                AssetDatabase.AddObjectToAsset(c, path);

            AssetDatabase.ImportAsset(path);
        }

        static void GatherAllSubtrees(BlendTree tree, List<BlendTree> trees)
        {
            foreach (BlendTree t in tree.children.Select(c => c.motion).Where(c => c is BlendTree && c != null))
            {
                trees.Add(t);
                GatherAllSubtrees(t, trees);
            }
        }

        //https://github.com/akshayb6/trilateration-in-3d/blob/master/trilateration.py
        Vector3 Trilaterate3D(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float r1, float r2, float r3, float r4)
        {
            Vector3 e_x = (p2 - p1).normalized;
            float i = Vector3.Dot(e_x, p3 - p1);
            Vector3 e_y = (p3 - p1 - (i * e_x)).normalized;
            Vector3 e_z = Vector3.Cross(e_x, e_y);
            float d = (p2 - p1).magnitude;
            float j = Vector3.Dot(e_y, (p3 - p1));
            float x = (Mathf.Pow(r1, 2) - Mathf.Pow(r2, 2) + Mathf.Pow(d, 2)) / (2 * d);
            float y = ((Mathf.Pow(r1, 2) - Mathf.Pow(r3, 2) + Mathf.Pow(i, 2) + Mathf.Pow(j, 2)) / (2 * j)) - ((i / j) * (x));
            float z1 = Mathf.Sqrt(Mathf.Pow(r1, 2) - Mathf.Pow(x, 2) - Mathf.Pow(y, 2));
            float z2 = Mathf.Sqrt(Mathf.Pow(r1, 2) - Mathf.Pow(x, 2) - Mathf.Pow(y, 2)) * (-1);
            Vector3 ans1 = p1 + (x * e_x) + (y * e_y) + (z1 * e_z);
            Vector3 ans2 = p1 + (x * e_x) + (y * e_y) + (z2 * e_z);
            float dist1 = (p4 - ans1).magnitude;
            float dist2 = (p4 - ans2).magnitude;
            if (Mathf.Abs(r4 - dist1) < Mathf.Abs(r4 - dist2))
                return ans1;
            else
                return ans2;
        }

#if VRC_SDK_VRCSDK3 && !UDON
        static void RemoveVRCSendersAndRecievers(Transform transform)
        {
            IEnumerable<VRCContactReceiver> reciever = transform.GetComponentsInChildren<VRCContactReceiver>(true).Where(r => r.collisionTags.Any(t => t.StartsWith("TPS")) || r.parameter.StartsWith("TPS"));
            IEnumerable<VRCContactSender> sender = transform.GetComponentsInChildren<VRCContactSender>(true).Where(r => r.collisionTags.Any(t => t.StartsWith("TPS")));
            foreach (VRCContactReceiver r in reciever)
            {
                if (r.gameObject.GetComponents<Component>().Length == 2 && !PrefabUtility.IsPartOfAnyPrefab(r.gameObject)) DestroyImmediate(r.gameObject);
                else DestroyImmediate(r);
            }
            foreach (VRCContactSender s in sender)
            {
                if (s.gameObject.GetComponents<Component>().Length == 2 && !PrefabUtility.IsPartOfAnyPrefab(s.gameObject)) DestroyImmediate(s.gameObject);
                else DestroyImmediate(s);
            }
        }

        static void PlaceVRCContactReceiverProximity(AnimatorController animator, Transform transform, Vector3 position, string paramName, float radius, params string[] tags)
        {
            VRCContactReceiver reciever = transform.gameObject.AddComponent<VRCContactReceiver>();
            reciever.position = new Vector3(position.x / transform.lossyScale.x, position.y / transform.lossyScale.y, position.z / transform.lossyScale.z);
            reciever.parameter = paramName;
            reciever.radius = radius / transform.lossyScale.x;
            reciever.receiverType = VRC.Dynamics.ContactReceiver.ReceiverType.Proximity;
            reciever.collisionTags = new List<string>(tags);
            AddParameter(animator, paramName, AnimatorControllerParameterType.Float);
        }

        static Transform PlaceVRCContactReceiverProximityOnChildObject(AnimatorController animator, Transform parent, Vector3 localPosition, string paramName, float radius, params string[] tags)
        {
            string path = "R_" + tags[0] + "=>" + paramName;
            Transform transform = SingletonChild(parent, path);
            transform.position = parent.position + transform.rotation * localPosition;
            PlaceVRCContactReceiverProximity(animator, transform, Vector3.zero, paramName, radius, tags);
            return transform;
        }

        static VRCContactReceiver PlaceVRCContactReceiverBool(AnimatorController animator, Transform transform, Vector3 position, string paramName, float radius, params string[] tags)
        {
            VRCContactReceiver reciever = transform.gameObject.AddComponent<VRCContactReceiver>();
            reciever.position = new Vector3(position.x / transform.lossyScale.x, position.y / transform.lossyScale.y, position.z / transform.lossyScale.z);
            reciever.parameter = paramName;
            reciever.radius = radius / transform.lossyScale.x;
            reciever.receiverType = VRC.Dynamics.ContactReceiver.ReceiverType.Constant;
            reciever.collisionTags = new List<string>(tags);
            AddParameter(animator, paramName, AnimatorControllerParameterType.Float);
            return reciever;
        }

        static VRCContactSender PlaceVRCContactSender(Transform transform, Vector3 position, float radius, params string[] tags)
        {
            VRCContactSender sender = transform.gameObject.AddComponent<VRCContactSender>();
            sender.position = new Vector3(position.x / transform.lossyScale.x, position.y / transform.lossyScale.y, position.z / transform.lossyScale.z);
            sender.radius = radius / transform.lossyScale.x;
            sender.collisionTags = new List<string>(tags);
            return sender;
        }

        static Transform PlaceVRCContactSenderOnChildObject(Transform parent, Vector3 localPosition, float radius, params string[] tags)
        {
            string path = "S_" + tags[0];
            Transform transform = SingletonChild(parent, path);
            transform.position = parent.position + transform.rotation * localPosition;
            PlaceVRCContactSender(transform, Vector3.zero, radius, tags);
            return transform;
        }
#endif
    }

    public static class ExtensionMethods
    {
        public static IList<T> Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
            return list;
        }
    }

    public enum CompareType { EQUAL, NOT_EQUAL, LESS, GREATER }

    public class ThryAnimatorFunctions
    {
        public static AnimatorControllerLayer SingletonLayer(AnimatorController animator, string layerName, bool clearLayer = true, AnimatorControllerLayer needsToBeAboveLayer = null)
        {
            AnimatorControllerLayer layer = null;
            int layerIndex = 0;
            for (int i = animator.layers.Length - 1; i >= 0; i--)
            {
                if (animator.layers[i].name == layerName)
                {
                    if (clearLayer)
                    {
                        ClearLayer(animator.layers[i]);
                    }
                    layer = animator.layers[i];
                    layerIndex = i;
                    break;
                }
            }

            if (layer == null)
            {
                layer = new AnimatorControllerLayer();
                layer.name = layerName;
                layer.defaultWeight = 1;
                layer.stateMachine = new AnimatorStateMachine();
                layer.stateMachine.name = layer.name;
                layer.stateMachine.hideFlags = HideFlags.HideInHierarchy;
                if (AssetDatabase.GetAssetPath(animator) != "")
                {
                    AssetDatabase.AddObjectToAsset(layer.stateMachine,
                        AssetDatabase.GetAssetPath(animator));
                }
                layerIndex = animator.layers.Length;
                animator.AddLayer(layer);
            }

            if (needsToBeAboveLayer != null)
            {
                AnimatorControllerLayer[] layers = animator.layers;
                for (int i = 0; i < layers.Length; i++)
                {
                    if (layers[i].name == layer.name)
                    {
                        break;
                    }
                    if (layers[i].name == needsToBeAboveLayer.name)
                    {
                        needsToBeAboveLayer = layers[i];
                        layers[i] = layer;
                        for (int j = layerIndex; j > i; j--)
                            layers[j] = layers[j - 1];
                        layers[i + 1] = needsToBeAboveLayer;
                        break;
                    }
                }
                animator.layers = layers;
            }
            return layer;
        }

        public static void ClearLayer(AnimatorControllerLayer layer)
        {
            layer.stateMachine.states = new ChildAnimatorState[0];
            layer.stateMachine.stateMachines = new ChildAnimatorStateMachine[0];
            layer.stateMachine.anyStateTransitions = new AnimatorStateTransition[0];
            layer.stateMachine.entryTransitions = new AnimatorTransition[0];
            layer.stateMachine.behaviours = new StateMachineBehaviour[0];
        }

        private static AnimationClip LoadEmptyClipOrCreateClip(string name, string directory, float length)
        {
            string[] guids = AssetDatabase.FindAssets(name + " t:animationclip");
            if (guids.Length > 0) return AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(guids[0]));
            return CreateClip("Assets", "Empty_Clip", new CurveConfig("NanObject", "m_IsActive", typeof(GameObject), FloatCurve(1, length)));
        }

        private static AnimationClip _empty;
        public static AnimationClip EmptyClip
        {
            get
            {
                if (_empty == null) _empty = LoadEmptyClipOrCreateClip("Empty_Clip", "Assets", 1f / 60);
                return _empty;
            }
        }

        private static AnimationClip _empty1Sec;
        public static AnimationClip EmptyClip1Sec
        {
            get
            {
                if (_empty1Sec == null) _empty1Sec = LoadEmptyClipOrCreateClip("Empty_1_sec", "Assets", 1f / 60);
                return _empty1Sec;
            }
        }

        private static AnimationClip _empty2Sec;
        public static AnimationClip EmptyClip2Sec
        {
            get
            {
                if (_empty2Sec == null) _empty2Sec = LoadEmptyClipOrCreateClip("Empty_2_sec", "Assets", 1f / 60);
                return _empty2Sec;
            }
        }

        private static AnimationClip _empty5Sec;
        public static AnimationClip EmptyClip5Sec
        {
            get
            {
                if (_empty5Sec == null) _empty5Sec = LoadEmptyClipOrCreateClip("Empty_5_sec", "Assets", 1f / 60);
                return _empty5Sec;
            }
        }

#if VRC_SDK_VRCSDK3 && !UDON
        public static VRCAvatarParameterDriver AddParameterDriver(AnimatorState state, params (string, VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType, float)[] drive)
        {
            if (drive.Count() == 0) return null;
            VRCAvatarParameterDriver driver = state.behaviours.FirstOrDefault(b => b.GetType() == typeof(VRCAvatarParameterDriver)) as VRCAvatarParameterDriver;
            if (driver == null)
                driver = state.AddStateMachineBehaviour(typeof(VRCAvatarParameterDriver)) as VRCAvatarParameterDriver;
            foreach ((string, VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType, float) d in drive)
            {
                VRC.SDKBase.VRC_AvatarParameterDriver.Parameter driverParameter = new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter();
                driverParameter.name = d.Item1;
                driverParameter.value = d.Item3;
                driverParameter.type = d.Item2;
                driver.parameters.Add(driverParameter);
            }
            return driver;
        }
#endif

        public static AnimatorState CreateState(string name, AnimatorControllerLayer layer, Motion motion, bool setAsDefaultState = false)
        {
            AnimatorState state = layer.stateMachine.AddState(name);
            state.motion = motion;
            state.writeDefaultValues = false;
            if (setAsDefaultState) layer.stateMachine.defaultState = state;
            return state;
        }

        public static AnimatorState CreateState(string name, AnimatorStateMachine stateMachine, Motion motion, bool setAsDefaultState = false)
        {
            AnimatorState state = stateMachine.AddState(name);
            state.motion = motion;
            state.writeDefaultValues = false;
            if (setAsDefaultState) stateMachine.defaultState = state;
            return state;
        }

        public static AnimatorStateTransition CreateTransition(AnimatorState from, AnimatorState to, float duration = 0.01f, bool hasExitTime = true, float exitTime = 0.1f)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = hasExitTime;
            transition.duration = duration;
            transition.exitTime = exitTime;
            return transition;
        }

        public static AnimatorStateTransition CreateTransition(AnimatorState from, AnimatorStateMachine to, float duration = 0.01f, bool hasExitTime = true, float exitTime = 0.1f)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = hasExitTime;
            transition.duration = duration;
            transition.exitTime = exitTime;
            return transition;
        }

        public static AnimatorStateTransition CreateAnyStateTransition(AnimatorControllerLayer l, AnimatorState to, float duration = 0.01f, bool hasExitTime = false, float exitTime = 0.1f)
        {
            AnimatorStateTransition transition = l.stateMachine.AddAnyStateTransition(to);
            transition.hasExitTime = hasExitTime;
            transition.duration = duration;
            transition.exitTime = exitTime;
            transition.canTransitionToSelf = false;
            return transition;
        }

        public class Condition
        {
            public string ParamName;
            public CompareType CompareType;
            public object Value;
            public Condition(string n, CompareType t, object v)
            {
                this.ParamName = n;
                this.CompareType = t;
                this.Value = v;
            }
        }

        public static AnimatorStateTransition CreateTransition(AnimatorState from, AnimatorState to, params Condition[] conditions)
        {
            AnimatorStateTransition transition = CreateTransition(from, to, 0.00f, false, 0.0f);
            AddTransitionConditions(transition, conditions);
            return transition;
        }

        public static AnimatorStateTransition CreateTransition(AnimatorState from, AnimatorStateMachine to, params Condition[] conditions)
        {
            AnimatorStateTransition transition = CreateTransition(from, to, 0.01f, false, 0.0f);
            AddTransitionConditions(transition, conditions);
            return transition;
        }

        public static AnimatorTransition CreateTransition(AnimatorStateMachine from, AnimatorState to, AnimatorStateMachine parent, params Condition[] conditions)
        {
            AnimatorTransition newT = new AnimatorTransition();
            newT.destinationState = to;
            AddTransitionConditions(newT, conditions);
            AnimatorTransition[] transitions = parent.GetStateMachineTransitions(from);
            transitions = transitions.Append(newT).ToArray();
            parent.SetStateMachineTransitions(from, transitions);
            return newT;
        }

        public static AnimatorTransition CreateTransition(AnimatorStateMachine from, AnimatorStateMachine to, AnimatorStateMachine parent, params Condition[] conditions)
        {
            AnimatorTransition newT = new AnimatorTransition();
            newT.destinationStateMachine = to;
            AddTransitionConditions(newT, conditions);
            AnimatorTransition[] transitions = parent.GetStateMachineTransitions(from);
            transitions = transitions.Append(newT).ToArray();
            parent.SetStateMachineTransitions(from, transitions);
            return newT;
        }

        public static AnimatorStateTransition CreateTransition(AnimatorState from, AnimatorState to, float duration = 0.01f, bool hasExitTime = false, float exitTime = 0.1f, params Condition[] conditions)
        {
            AnimatorStateTransition transition = CreateTransition(from, to, duration, hasExitTime, exitTime);
            AddTransitionConditions(transition, conditions);
            return transition;
        }

        public static void AddTransitionConditions(AnimatorStateTransition transition, params Condition[] conditions)
        {
            foreach (Condition c in conditions)
            {
                if (c.Value.GetType() == typeof(float))
                {
                    if (c.CompareType == CompareType.LESS) transition.AddCondition(AnimatorConditionMode.Less, (float)c.Value, c.ParamName);
                    if (c.CompareType == CompareType.GREATER) transition.AddCondition(AnimatorConditionMode.Greater, (float)c.Value, c.ParamName);
                }
                else if (c.Value.GetType() == typeof(int))
                {
                    if (c.CompareType == CompareType.LESS) transition.AddCondition(AnimatorConditionMode.Less, (int)c.Value, c.ParamName);
                    if (c.CompareType == CompareType.GREATER) transition.AddCondition(AnimatorConditionMode.Greater, (int)c.Value, c.ParamName);
                    if (c.CompareType == CompareType.EQUAL) transition.AddCondition(AnimatorConditionMode.Equals, (int)c.Value, c.ParamName);
                    if (c.CompareType == CompareType.NOT_EQUAL) transition.AddCondition(AnimatorConditionMode.NotEqual, (int)c.Value, c.ParamName);
                }
                else if (c.Value.GetType() == typeof(bool))
                {
                    if ((bool)c.Value) transition.AddCondition(AnimatorConditionMode.If, 0, c.ParamName);
                    else transition.AddCondition(AnimatorConditionMode.IfNot, 0, c.ParamName);
                }
            }
        }

        public static void AddTransitionConditions(AnimatorTransition transition, params Condition[] conditions)
        {
            foreach (Condition c in conditions)
            {
                if (c.Value.GetType() == typeof(float))
                {
                    if (c.CompareType == CompareType.LESS) transition.AddCondition(AnimatorConditionMode.Less, (float)c.Value, c.ParamName);
                    if (c.CompareType == CompareType.GREATER) transition.AddCondition(AnimatorConditionMode.Greater, (float)c.Value, c.ParamName);
                }
                else if (c.Value.GetType() == typeof(int))
                {
                    if (c.CompareType == CompareType.LESS) transition.AddCondition(AnimatorConditionMode.Less, (int)c.Value, c.ParamName);
                    if (c.CompareType == CompareType.GREATER) transition.AddCondition(AnimatorConditionMode.Greater, (int)c.Value, c.ParamName);
                    if (c.CompareType == CompareType.EQUAL) transition.AddCondition(AnimatorConditionMode.Equals, (int)c.Value, c.ParamName);
                    if (c.CompareType == CompareType.NOT_EQUAL) transition.AddCondition(AnimatorConditionMode.NotEqual, (int)c.Value, c.ParamName);
                }
                else if (c.Value.GetType() == typeof(bool))
                {
                    if ((bool)c.Value) transition.AddCondition(AnimatorConditionMode.If, 0, c.ParamName);
                    else transition.AddCondition(AnimatorConditionMode.IfNot, 0, c.ParamName);
                }
            }
        }

        public static void AddParameter(AnimatorController animator, string param, AnimatorControllerParameterType type, object defaultValue = null)
        {
            if (animator.parameters.Where(p => p.name == param).Count() == 0)
            {
                animator.AddParameter(param, type);
            }
            if (defaultValue != null)
            {
                AnimatorControllerParameter[] parameters = animator.parameters;
                if (defaultValue.GetType() == typeof(bool)) parameters.First(p => p.name == param).defaultBool = (bool)defaultValue;
                if (defaultValue.GetType() == typeof(int)) parameters.First(p => p.name == param).defaultInt = (int)defaultValue;
                if (defaultValue.GetType() == typeof(float)) parameters.First(p => p.name == param).defaultFloat = (float)defaultValue;
                animator.parameters = parameters;
            }
        }

        public static BlendTree CreateFloatCopyBlendTree(string directory, string name, string fromParamter, string toParameter)
        {
            AnimationClip neg = CreateClip(name + "_neg", ("", typeof(Animator), toParameter, NegativeOneCurveOneFrame));
            AnimationClip pos = CreateClip(name + "_pos", ("", typeof(Animator), toParameter, PositiveOneCurveOneFrame));
            BlendTree tree = new BlendTree();
            tree.useAutomaticThresholds = false;
            tree.AddChild(neg, -1);
            tree.AddChild(pos, 1);
            tree.blendParameter = fromParamter;
            string path = directory + "/" + name + ".asset";
            AssetDatabase.CreateAsset(tree, path);
            AssetDatabase.AddObjectToAsset(neg, path);
            AssetDatabase.AddObjectToAsset(pos, path);
            return tree;
        }

        public static Dictionary<(GameObject, GameObject), string> savedPaths = new Dictionary<(GameObject, GameObject), string>();
        public static string GetPath(GameObject sensor, GameObject avatar)
        {
            if (savedPaths.ContainsKey((sensor, avatar))) return savedPaths[(sensor, avatar)];
            Transform o = sensor.transform.parent;
            List<Transform> path = new List<Transform>();
            while (o != avatar.transform && o != null)
            {
                path.Add(o);
                o = o.parent;
            }
            path.Reverse();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (Transform t in path)
            {
                sb.Append(t.name + "/");
            }
            sb.Append(sensor.name);
            string finalpath = sb.ToString();
            savedPaths.Add((sensor, avatar), finalpath);
            return finalpath;
        }

        public static AnimationClip CreateAnimation()
        {
            AnimationClip clip = new AnimationClip();

            return clip;
        }

        public static AnimationCurve OnCurve => new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 1) });
        public static AnimationCurve OnCurveOneFrame => new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1f / 60, 1) });
        public static AnimationCurve OffCurve => new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 0) });
        public static AnimationCurve OffCurveOneFrame => new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1f / 60, 0) });
        public static AnimationCurve PositiveOneCurveOneFrame => new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1f / 60, 1) });
        public static AnimationCurve NegativeOneCurveOneFrame => new AnimationCurve(new Keyframe[] { new Keyframe(0, -1), new Keyframe(1f / 60, -1) });
        public static AnimationCurve IntCurve(int value, float time) { return new AnimationCurve(new Keyframe[] { new Keyframe(0, value), new Keyframe(time, value) }); }
        public static AnimationCurve FloatCurve(float value, float time) { return new AnimationCurve(new Keyframe[] { new Keyframe(0, value), new Keyframe(time, value) }); }
        public static AnimationCurve CustomCurve(params (float, float)[] keys) { return new AnimationCurve(keys.Select(tv => new Keyframe(tv.Item1, tv.Item2)).ToArray()); }

        public struct CurveConfig
        {
            public string Path;
            public string Attribute;
            public Type Type;
            public AnimationCurve Curve;
            public CurveConfig(string path, string at, Type type, AnimationCurve curve)
            {
                this.Path = path;
                this.Attribute = at;
                this.Type = type;
                this.Curve = curve;
            }
        }

        public static AnimationClip CreateClip(string directory, string name, params (string, Type, string, AnimationCurve)[] curves)
        {
            AnimationClip clip = new AnimationClip();
            foreach ((string, Type, string, AnimationCurve) curve in curves)
            {
                clip.SetCurve(curve.Item1, curve.Item2, curve.Item3, curve.Item4);
            }
            AssetDatabase.CreateAsset(clip, directory + "/" + name + ".anim");
            return clip;
        }

        public static AnimationClip CreateClip(string directory, string name, params CurveConfig[] curves)
        {
            AnimationClip clip = new AnimationClip();
            foreach (CurveConfig c in curves)
            {
                clip.SetCurve(c.Path, c.Type, c.Attribute, c.Curve);
            }
            AssetDatabase.CreateAsset(clip, directory + "/" + name + ".anim");
            return clip;
        }

        public static AnimationClip CreateClip(string name, params (string, Type, string, AnimationCurve)[] curves)
        {
            AnimationClip clip = new AnimationClip();
            clip.name = name;
            foreach ((string, Type, string, AnimationCurve) curve in curves)
            {
                clip.SetCurve(curve.Item1, curve.Item2, curve.Item3, curve.Item4);
            }
            return clip;
        }

        public static AnimationClip CreateClip(string name, params CurveConfig[] curves)
        {
            AnimationClip clip = new AnimationClip();
            clip.name = name;
            foreach (CurveConfig c in curves)
            {
                clip.SetCurve(c.Path, c.Type, c.Attribute, c.Curve);
            }
            return clip;
        }

        public struct ThryCurveData
        {
            public string path;
            public string propertyName;
            public AnimationCurve curve;
        }

        public static ThryCurveData[] GetAllCurves(AnimationClip clip)
        {
            if (clip == null) return new ThryCurveData[0];
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
            ThryCurveData[] curves = new ThryCurveData[bindings.Length];
            for (int i = 0; i < curves.Length; i++)
            {
                ThryCurveData data = new ThryCurveData();
                data.path = bindings[i].path;
                data.propertyName = bindings[i].propertyName;
                data.curve = AnimationUtility.GetEditorCurve(clip, bindings[i]);
                curves[i] = data;
            }
            return curves;
        }

#endregion
    }

#region Vertex Color Baker

    public class BakeToVertexColors
    {
        //Strings
        const string log_prefix = "<color=blue>Poi:</color> "; //color is hex or name
        static readonly string suffixSeparator = "_";

        const string bakedSuffix_normals = "baked_normals";
        const string bakedSuffix_mesh = "baked_mesh";

        const string bakesFolderName = "Baked";
        const string defaultUnityAssetBakesFolder = "Default Unity Resources";
        //Properties
        static GameObject Selection
        {
            get => _selection;
            set => _selection = value;
        }

        /// <summary>
        /// Adds a suffix to the end of the string then returns it
        /// </summary>
        /// <param name="str"></param>
        /// <param name="suffixes"></param>
        /// <returns></returns>
        static string AddSuffix(string str, params string[] suffixes)
        {
            bool ignoreSeparatorOnce = string.IsNullOrWhiteSpace(str);
            StringBuilder sb = new StringBuilder(str);
            foreach (var suff in suffixes)
            {
                if (ignoreSeparatorOnce)
                {
                    sb.Append(suff);
                    ignoreSeparatorOnce = false;
                    continue;
                }
                sb.Append(suffixSeparator + suff);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Replaces all forward slashes \ with back slashes /
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static string NormalizePathSlashes(string path)
        {
            if (!string.IsNullOrEmpty(path))
                path = path.Replace('\\', '/');
            return path;
        }

        /// <summary>
        /// Changes a path in Assets to an absolute windows path
        /// </summary>
        /// <param name="localPath"></param>
        /// <returns></returns>
        static string LocalAssetsPathToAbsolutePath(string localPath)
        {
            localPath = NormalizePathSlashes(localPath);
            const string assets = "Assets/";
            if (localPath.StartsWith(assets))
            {
                localPath = localPath.Remove(0, assets.Length);
                localPath = $"{Application.dataPath}/{localPath}";
            }
            return localPath;
        }

        /// <summary>
        /// Ensures directory exists inside the assets folder
        /// </summary>
        /// <param name="assetPath"></param>
        static void EnsurePathExistsInAssets(string assetPath)
        {
            Directory.CreateDirectory(LocalAssetsPathToAbsolutePath(assetPath));
        }

        static Texture2D SaveTextureAsset(Mesh mesh, Texture2D tex, string newName)
        {
            string assetPath = AssetDatabase.GetAssetPath(mesh);

            if (string.IsNullOrWhiteSpace(assetPath))
            {
                Debug.LogWarning(log_prefix + "Invalid asset path for " + mesh.name);
                return null;
            }

            //Figure out folder name
            string bakesDir = $"{Path.GetDirectoryName(assetPath)}";

            //Handle default assets
            if (bakesDir.StartsWith("Library"))
                bakesDir = $"Assets\\{defaultUnityAssetBakesFolder}";

            if (!bakesDir.EndsWith(bakesFolderName))
                bakesDir += $"\\{bakesFolderName}";

            if (!assetPath.Contains('.'))
                assetPath += '\\';

            EnsurePathExistsInAssets(bakesDir);

            //Generate path
            string pathNoExt = Path.Combine(bakesDir, newName);
            string newPath = AssetDatabase.GenerateUniqueAssetPath($"{pathNoExt}.png");

            byte[] encoding = tex.EncodeToPNG();
            File.Create(newPath).Close();
            using (var fs = new FileStream(newPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                fs.Write(encoding, 0, encoding.Length);
            }
            AssetDatabase.ImportAsset(newPath);

            TextureImporter importer = TextureImporter.GetAtPath(newPath) as TextureImporter;
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.streamingMipmaps = true;
            importer.sRGBTexture = false;
            importer.crunchedCompression = false;
            importer.maxTextureSize = 8192;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.isReadable = true;

            AssetDatabase.ImportAsset(newPath);
            tex = AssetDatabase.LoadAssetAtPath<Texture2D>(newPath);

            if (tex == null)
            {
                Debug.Log(log_prefix + "Failed to load saved mesh");
                return null;
            }

            return tex;
        }

        public static Transform GetArmatureTransform(SkinnedMeshRenderer smr)
        {
            if (smr.bones.Length == 0) return smr.transform;
            IEnumerable<Transform> bones = smr.bones;
            Transform armature = bones.First();
            while (armature != null && bones.Contains(armature)) armature = armature.parent;
            if (armature == null) armature = smr.transform;
            return armature;
        }

        public static MeshInfo[] GetAllMeshInfos(params Renderer[] renderers)
        {
            var infos = renderers?.Select(ren =>
            {
                MeshInfo info = new MeshInfo();
                if (ren is SkinnedMeshRenderer smr)
                {
                    Mesh bakedMesh = new Mesh();

                    Queue<(Quaternion, Vector3)> ogTransformSettings = new Queue<(Quaternion, Vector3)>();
                    Transform tr = smr.transform;
                    while(tr != null)
                    {
                        ogTransformSettings.Enqueue((tr.localRotation, tr.localScale));
                        tr.localRotation = Quaternion.identity;
                        tr.localScale = Vector3.one;
                        tr = tr.parent;
                    }
                    Transform aTransform = GetArmatureTransform(ren as SkinnedMeshRenderer);
                    if(aTransform != smr.transform)
                    {
                        ogTransformSettings.Enqueue((aTransform.localRotation, aTransform.localScale));
                        aTransform.localRotation = Quaternion.identity;
                        aTransform.localScale = Vector3.one;
                    }

                    smr.BakeMesh(bakedMesh);

                    tr = smr.transform;
                    while (tr != null)
                    {
                        (Quaternion, Vector3) set = ogTransformSettings.Dequeue();
                        tr.localRotation = set.Item1;
                        tr.localScale = set.Item2;
                        tr = tr.parent;
                    }
                    if (aTransform != smr.transform)
                    {
                        (Quaternion, Vector3) set = ogTransformSettings.Dequeue();
                        aTransform.localRotation = set.Item1;
                        aTransform.localScale = set.Item2;
                    }

                    info.sharedMesh = smr.sharedMesh;
                    info.bakedVertices = bakedMesh?.vertices;
                    info.bakedNormals = bakedMesh?.normals;
                    info.ownerRenderer = smr;
                    if (!info.sharedMesh)
                        Debug.LogWarning(log_prefix + $"Skinned Mesh Renderer at <b>{info.ownerRenderer.gameObject.name}</b> doesn't have a valid mesh");
                }
                else if (ren is MeshRenderer mr)
                {
                    info.sharedMesh = mr.GetComponent<MeshFilter>()?.sharedMesh;
                    info.bakedVertices = info.sharedMesh?.vertices;
                    info.bakedNormals = info.sharedMesh?.normals;
                    info.ownerRenderer = mr;
                    if (!info.sharedMesh)
                        Debug.LogWarning(log_prefix + $"Mesh renderer at <b>{info.ownerRenderer.gameObject.name}</b> doesn't have a mesh filter with a valid mesh");
                }
                return info;
            }).ToArray();

            return infos;
        }

        static Color32 Uint8tof32(uint r, uint g, uint b, uint a)
        {
            Color32 split = new Color32(
                (byte)r,
                (byte)g,
                (byte)b,
                (byte)a
            );

            return split;
        }


        static Color32 EncodeFloatToARGB8(float f)
        {
            uint u = BitConverter.ToUInt32(BitConverter.GetBytes(f), 0);
            return Uint8tof32(u & 255, (u >> 8) & 255, (u >> 16) & 255, (u >> 24) & 255);
        }

        public static float DecodeFloatFromARGB8(Color32 c)
        {
            uint u = c.r + ((uint)c.g << 8) + ((uint)c.b << 16) + ((uint)c.a << 24);
            return BitConverter.ToSingle(BitConverter.GetBytes(u), 0);
        }

        public static Texture2D GetReadableTexture(Texture texture)
        {
            RenderTexture temp = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            Graphics.Blit(texture, temp);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = temp;
            Texture2D ret = new Texture2D(texture.width, texture.height);
            ret.ReadPixels(new Rect(0, 0, temp.width, temp.height), 0, 0);
            ret.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(temp);
            return ret;
        }

        public static Texture2D BakePositionsToTexture(Renderer renderer, Texture2D mask)
        {
            return BakePositionsToTexture(GetAllMeshInfos(renderer)[0], mask);
        }

        static Texture2D BakePositionsToTexture(MeshInfo meshInfo, Texture2D mask)
        {
            if (!meshInfo.sharedMesh)
                return null;
            Texture2D tex = null;

            if (mask != null) mask = GetReadableTexture(mask);

            try
            {
                Vector3[] verts = meshInfo.bakedVertices;    //accessing mesh.vertices on every iteration is very slow
                Vector3[] norms = meshInfo.bakedNormals;    //accessing mesh.vertices on every iteration is very slow
                Vector2[] uvs = meshInfo.sharedMesh.uv;
                tex = new Texture2D(8190, ((verts.Length * 2 * 3 - 1) / 8190) + 1, TextureFormat.RGBA32, false);
                Color32[] colors = new Color32[tex.width * tex.height];
                for (int i = 0; i < verts.Length; i++)
                {
                    colors[i * 6 + 0] = EncodeFloatToARGB8(verts[i].x);
                    colors[i * 6 + 1] = EncodeFloatToARGB8(verts[i].y);
                    colors[i * 6 + 2] = EncodeFloatToARGB8(verts[i].z);
                    if (mask != null)
                    {
                        Color m = mask.GetPixelBilinear(uvs[i].x, uvs[i].y);
                        if (m.maxColorComponent > 0 && m.a > 0) continue;
                    }
                    colors[i * 6 + 3] = EncodeFloatToARGB8(norms[i].x);
                    colors[i * 6 + 4] = EncodeFloatToARGB8(norms[i].y);
                    colors[i * 6 + 5] = EncodeFloatToARGB8(norms[i].z);
                }
                tex.SetPixels32(colors);
                tex.Apply();

                //Create new mesh asset and add it to queue
                string name = AddSuffix(meshInfo.ownerRenderer.gameObject.name, bakedSuffix_mesh);
                tex= SaveTextureAsset(meshInfo.sharedMesh, tex, name);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return tex;
        }

        public struct MeshInfo
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

#endregion
}