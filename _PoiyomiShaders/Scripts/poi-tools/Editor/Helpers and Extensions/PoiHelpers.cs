using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Poi.Tools
{
    //TODO: Split these into separate classes
    public static class PoiHelpers
    {
        static readonly string suffixSeparator = "_";

        /// <summary>
        /// Changes a path in Assets to an absolute windows path
        /// </summary>
        /// <param name="localPath"></param>
        /// <returns></returns>
        public static string LocalAssetsPathToAbsolutePath(string localPath)
        {
            localPath = NormalizePathSlashes(localPath);
            const string assets = "Assets/";
            if(localPath.StartsWith(assets))
            {
                localPath = localPath.Remove(0, assets.Length);
                localPath = $"{Application.dataPath}/{localPath}";
            }
            return localPath;
        }

        /// <summary>
        /// Replaces all forward slashes \ with back slashes /
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string NormalizePathSlashes(string path)
        {
            if(!string.IsNullOrEmpty(path))
                path = path.Replace('\\', '/');
            return path;
        }

        /// <summary>
        /// Ensures directory exists inside the assets folder
        /// </summary>
        /// <param name="assetPath"></param>
        public static void EnsurePathExistsInAssets(string assetPath)
        {
            Directory.CreateDirectory(LocalAssetsPathToAbsolutePath(assetPath));
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Adds a suffix to the end of the string then returns it
        /// </summary>
        /// <param name="str"></param>
        /// <param name="suffixes"></param>
        /// <returns></returns>
        public static string AddSuffix(string str, params string[] suffixes)
        {
            bool ignoreSeparatorOnce = string.IsNullOrWhiteSpace(str);
            StringBuilder sb = new StringBuilder(str);
            foreach(var suff in suffixes)
            {
                if(ignoreSeparatorOnce)
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
        /// Draws a GUI line
        /// </summary>
        /// <param name="spaceBefore"></param>
        /// <param name="spaceAfter"></param>
        public static void DrawLine(bool spaceBefore = true, bool spaceAfter = true)
        {
            float spaceHeight = 3f;
            if(spaceBefore)
                GUILayout.Space(spaceHeight);

            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));

            if(spaceAfter)
                GUILayout.Space(spaceHeight);
        }

        /// <summary>
        /// Destroys an object with DestroyImmediate in object mode and Destroy in play mode
        /// </summary>
        /// <param name="obj"></param>
        public static void DestroyAppropriate(UnityEngine.Object obj)
        {
            if(EditorApplication.isPlaying)
                UnityEngine.Object.Destroy(obj);
            else
                UnityEngine.Object.DestroyImmediate(obj);
        }

        /// <summary>
        /// Changes path from full windows path to a local path in the Assets folder
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Path starting with Assets</returns>
        public static string AbsolutePathToLocalAssetsPath(string path)
        {
            if(path.StartsWith(Application.dataPath))
                path = "Assets" + path.Substring(Application.dataPath.Length);
            return path;
        }

        /// <summary>
        /// Selects and highlights the asset in your unity Project tab
        /// </summary>
        /// <param name="path"></param>
        public static void PingAssetAtPath(string path)
        {
#if UNITY_6000_5_OR_NEWER
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            EditorGUIUtility.PingObject(obj);
#else
            var inst = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path).GetInstanceID();
            EditorGUIUtility.PingObject(inst);
#endif
        }

        public static void DrawWithLabelWidth(float width, Action action)
        {
            if(action == null)
                return;
            float old = EditorGUIUtility.labelWidth;
            action.Invoke();
            EditorGUIUtility.labelWidth = old;
        }

        static HashSet<char> IllegalFilenameChars
        {
            get
            {
                if(_illegalFilenameChars == null)
                    _illegalFilenameChars = new HashSet<char>(Path.GetInvalidFileNameChars());
                return _illegalFilenameChars;
            }
        }
        static HashSet<char> _illegalFilenameChars;

        /// <summary>
        /// Replace all illegal filename characters (as returned by Path.GetInvalidFileNameChars()) with <paramref name="replacement"/>
        /// </summary>
        /// <param name="original">Original filename</param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string ReplaceIllegalFilenameCharacters(string original, char replacement = '_')
        {
            var sb = new StringBuilder();
            foreach(var current in original)
            {
                if(IllegalFilenameChars.Contains(current))
                    sb.Append(replacement);
                else
                    sb.Append(current);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Duplicates a material and outs the new path. Returns true if succeeded
        /// </summary>
        /// <param name="materialAsset">Asset to duplicate</param>
        /// <param name="newMaterialAssetName">New asset name with or without file extension</param>
        /// <param name="materialCopyPath">The new asset path</param>
        /// <returns>True if succeeded</returns>
        public static bool TryDuplicateMaterialAsset(Material materialAsset, string newMaterialAssetName, out string materialCopyPath)
        {
            materialCopyPath = null;
            try
            {
                string materialAssetPath = AssetDatabase.GetAssetPath(materialAsset);
                if(string.IsNullOrWhiteSpace(materialAssetPath))
                    return false;

                string newPath;
                if(IsDefaultAsset(materialAsset))
                    newPath = $"Assets/Duplicated Materials/{newMaterialAssetName}{(newMaterialAssetName.EndsWith(".mat") ? "" : ".mat")}";
                else
                    newPath = $"{Path.GetDirectoryName(materialAssetPath)}/{newMaterialAssetName}{(newMaterialAssetName.EndsWith(".mat") ? "" : ".mat")}";

                materialCopyPath = NormalizePathSlashes(AssetDatabase.GenerateUniqueAssetPath(newPath));

                if(AssetDatabase.IsSubAsset(materialAsset) || IsDefaultAsset(materialAsset))
                {
                    AssetDatabase.CreateAsset(new Material(materialAsset), materialCopyPath);
                    return true;
                }
                return AssetDatabase.CopyAsset(materialAssetPath, materialCopyPath);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Is this a built in unity asset?
        /// </summary>
        /// <param name="asset">The asset to check</param>
        /// <returns>True if path is not empty and starts with Resources/unity_builtin_extra</returns>
        public static bool IsDefaultAsset(UnityEngine.Object asset)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            return IsDefaultAssetPath(path);
        }

        /// <summary>
        /// Is this the path of built in unity asset?
        /// </summary>
        /// <param name="assetPath">The path to check</param>
        /// <returns>True if path is not empty and starts with Resources/unity_builtin_extra</returns>
        public static bool IsDefaultAssetPath(string assetPath)
        {
            return !string.IsNullOrWhiteSpace(assetPath) && assetPath.StartsWith("Resources/unity_builtin_extra");
        }

        /// <summary>
        /// Get material swap animations in avatar
        /// </summary>
        /// <param name="gameObject">Avatar game object</param>
        /// <param name="animationClipAndMaterialSwapCurveBindings">A dictionary of animation clips and an array of curves that swap a material for each.</param>
        /// <returns>True if <paramref name="animationClipAndMaterialSwapCurveBindings"/>has any materials</returns>
        public static bool TryGetAnimationsWithMaterialSwapsInAvatar(GameObject gameObject, out Dictionary<AnimationClip, EditorCurveBinding[]> animationClipAndMaterialSwapCurveBindings)
        {
            animationClipAndMaterialSwapCurveBindings = new Dictionary<AnimationClip, EditorCurveBinding[]>();
            if(gameObject == null)
                return false;

            var allAnimators = gameObject.GetComponentsInChildren<Animator>(true)
                .Select(anim => anim.runtimeAnimatorController)
                .Where(ctrl => ctrl != null);

            var runtimeControllers = new List<RuntimeAnimatorController>(allAnimators);

#if VRC_SDK_VRCSDK3 && !UDON
            var descriptor = gameObject.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            if(descriptor && descriptor.customizeAnimationLayers)
            {
                runtimeControllers.AddRange(descriptor.baseAnimationLayers.Select(layer => layer.animatorController).Where(ctrl => ctrl != null));
                runtimeControllers.AddRange(descriptor.specialAnimationLayers.Select(layer => layer.animatorController).Where(ctrl => ctrl != null));
            }
#endif
            var allAnimationClips = allAnimators
                .SelectMany(anim => anim.animationClips)
                .Where(anim => anim != null)
                .Distinct();

            foreach(var clip in allAnimationClips)
            {
                var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
                var matSwapBindings = bindings
                    .Where(b => typeof(Renderer).IsAssignableFrom(b.type) && b.propertyName.StartsWith("m_Materials.Array.data")).ToArray();

                if(matSwapBindings != null && matSwapBindings.Length > 0)
                    animationClipAndMaterialSwapCurveBindings[clip] = matSwapBindings;
            }

            return animationClipAndMaterialSwapCurveBindings.Keys.Count > 0;
        }

        public static IEnumerable<Material> GetMaterialsFromMaterialSwapCurveBindings(AnimationClip animationClip, EditorCurveBinding[] materialSwapCurveBindings)
        {
            var matSwapKeyframes = materialSwapCurveBindings.SelectMany(binding => AnimationUtility.GetObjectReferenceCurve(animationClip, binding));
            return matSwapKeyframes.Select(frame => frame.value as Material).Distinct();
        }

        /// <summary>
        /// Collects all materials from a GameObject's renderers and optionally from material swap animations.
        /// Shows a dialog if animations are detected.
        /// </summary>
        /// <param name="gameObject">The root GameObject to collect materials from</param>
        /// <param name="promptForAnimations">If true, shows dialog when animations are found. If false, silently includes them.</param>
        /// <param name="dialogTitle">Title for the animation detection dialog</param>
        /// <param name="dialogMessage">Message format for the dialog. Use {0} for animation names.</param>
        /// <returns>Distinct list of materials</returns>
        public static List<Material> CollectMaterialsFromGameObject(GameObject gameObject, bool promptForAnimations = true,
            string dialogTitle = "Material Swap Animations Detected",
            string dialogMessage = "Animations that swap materials on your avatar were detected. Would you like materials inside those animations to be included as well?\n\nAffected animations:\n{0}")
        {
            var allMaterials = gameObject.GetComponentsInChildren<Renderer>(true)
                .SelectMany(r => r.sharedMaterials)
                .Where(m => m != null)
                .ToList();

            if (TryGetAnimationsWithMaterialSwapsInAvatar(gameObject, out var animClipAndCurveBindings))
            {
                bool includeAnimMaterials = !promptForAnimations;
                if (promptForAnimations)
                {
                    string clipNames = string.Join("\n", animClipAndCurveBindings.Keys.Select(clip => clip.name));
                    includeAnimMaterials = EditorUtility.DisplayDialog(dialogTitle, string.Format(dialogMessage, clipNames), "Yes", "No");
                }

                if (includeAnimMaterials)
                {
                    foreach (var animationAndCurveBinding in animClipAndCurveBindings)
                        allMaterials.AddRange(GetMaterialsFromMaterialSwapCurveBindings(animationAndCurveBinding.Key, animationAndCurveBinding.Value));
                }
            }

            return allMaterials.Where(m => m != null).Distinct().ToList();
        }

        /// <summary>
        /// Checks if a GameObject has any materials (including in animations) matching a predicate
        /// </summary>
        public static bool HasMaterialsMatching(GameObject gameObject, Func<Material, bool> predicate)
        {
            var allMaterials = gameObject.GetComponentsInChildren<Renderer>(true)
                .SelectMany(r => r.sharedMaterials)
                .Where(m => m != null)
                .ToList();

            if (TryGetAnimationsWithMaterialSwapsInAvatar(gameObject, out var animClipAndCurveBindings))
            {
                foreach (var animationAndCurveBinding in animClipAndCurveBindings)
                    allMaterials.AddRange(GetMaterialsFromMaterialSwapCurveBindings(animationAndCurveBinding.Key, animationAndCurveBinding.Value));
            }

            return allMaterials.Any(m => m != null && predicate(m));
        }

        public static bool IsURP()
        {
            UnityEngine.Rendering.RenderPipelineAsset graphicPipelineAsset = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            if (graphicPipelineAsset != null)
            {
                if (graphicPipelineAsset.GetType().Name.IndexOf("UniversalRenderPipelineAsset", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
