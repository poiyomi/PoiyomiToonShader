using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Poi.Tools
{
    public class DuplicateWithUniqueMaterials : EditorWindow
    {
        const string MaterialSwapDetectedDialogTitle = "Material Animations Detected";
        const string MaterialSwapDetectedDialogMessage = "Animations that swap materials on your avatar were detected. Would you like materials inside those animations to be duplicated as well? This won't change your animations.\nAffected animations:\n{0}";

        public static void DuplicateWithNewMaterials(GameObject avatar)
        {
            // Duplicate avatar as backup and replace all materials with duplicates on the original avatar
            // Seems like duplicating avatar and replacing materials on the new avatar doesn't work so this'll have to do for now

            Undo.SetCurrentGroupName($"Duplicate {avatar.name} with New Materials");
            int undoIndex = Undo.GetCurrentGroup();

            Undo.RegisterFullObjectHierarchyUndo(avatar, $"Undo {avatar.name}");

            bool duplicateMaterialsInAnimations = false;

            try
            {
                if(PoiHelpers.TryGetAnimationsWithMaterialSwapsInAvatar(avatar, out var clipsAndCurveBindings))
                {
                    var clipNames = string.Join("\n", clipsAndCurveBindings.Keys.Distinct().Select(clip => clip.name));
                    duplicateMaterialsInAnimations = EditorUtility.DisplayDialog(MaterialSwapDetectedDialogTitle, string.Format(MaterialSwapDetectedDialogMessage, clipNames), "Yes", "No");
                }

                string oldAvatarName = avatar.name;
                Selection.activeObject = avatar;
                SceneView.lastActiveSceneView.Focus();
                focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("Duplicate"));

                var avatarDuplicate = Selection.activeGameObject;
                avatarDuplicate.name = oldAvatarName;
                avatarDuplicate.transform.SetSiblingIndex(avatar.transform.GetSiblingIndex());
                avatar.transform.position += Vector3.left;
                avatar.name = $"{oldAvatarName}_copy";

                Undo.RegisterCreatedObjectUndo(avatarDuplicate, $"Duplicate {avatar.name}");
                Selection.activeGameObject = avatar;

                var renderers = avatar.GetComponentsInChildren<Renderer>(true);
                if(renderers.Length == 0)
                    return;

                var materials = renderers.SelectMany(ren => ren.sharedMaterials)
                    .Where(mat => mat != null)
                    .Distinct()
                    .ToList();

                var originalAndDuplicatedMaterials = new Dictionary<Material, Material>();

                string avatarAssetPath;
                if(PrefabUtility.IsPartOfAnyPrefab(avatar))
                {
                    avatarAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(avatar);
                }
                else // In case of an unpacked prefab, try to figure out what the heck the original asset path is.
                {
                    string warningLog = $"Duplicate with New Materials: <b>{avatar.name}</b> is not a prefab. Materials will be created ";
                    if(avatar.TryGetComponent<Animator>(out var animator) && animator.isHuman && animator.avatar)
                    {
                        warningLog += "in the fbx folder.";
                        avatarAssetPath = AssetDatabase.GetAssetPath(animator.avatar);
                    }
                    else
                    {
                        var renderer = avatar.GetComponentInChildren<Renderer>(true);

                        if(renderer is SkinnedMeshRenderer skinnedMeshRenderer && skinnedMeshRenderer.sharedMesh)
                        {
                            warningLog += "in the fbx folder.";
                            avatarAssetPath = AssetDatabase.GetAssetPath(skinnedMeshRenderer.sharedMesh);
                        }
                        else if(renderer is MeshRenderer meshRenderer && meshRenderer.TryGetComponent<MeshFilter>(out var meshFilter) && meshFilter.sharedMesh)
                        {
                            warningLog += "in the fbx folder.";
                            avatarAssetPath = AssetDatabase.GetAssetPath(meshFilter.sharedMesh);
                        }
                        else
                        {
                            warningLog += "at a default location.";
                            avatarAssetPath = $"Assets";
                        }
                    }
                    Debug.LogWarning(warningLog);
                }

                // Make sure our path is in assets
                if(!avatarAssetPath.StartsWith("Assets"))
                    avatarAssetPath = "Assets";

                if(avatarAssetPath != "Assets")
                    avatarAssetPath = PoiHelpers.NormalizePathSlashes(Path.GetDirectoryName(avatarAssetPath));

                // Ensure folders exist
                string sanitizedAvatarName = oldAvatarName.SanitizePathString();
                string newMaterialsFolder = $"{avatarAssetPath}/Duplicated Materials/{sanitizedAvatarName}";

                if(AssetDatabase.IsValidFolder(newMaterialsFolder))
                    newMaterialsFolder = AssetDatabase.GenerateUniqueAssetPath(newMaterialsFolder);
                PoiHelpers.EnsurePathExistsInAssets(newMaterialsFolder);

                // Get materials from animations
                if(duplicateMaterialsInAnimations)
                {
                    var animationMaterials = new HashSet<Material>();
                    foreach(var kv in clipsAndCurveBindings)
                    {
                        foreach(EditorCurveBinding curveBinding in kv.Value)
                        {
                            ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(kv.Key, curveBinding);
                            foreach(ObjectReferenceKeyframe keyframe in keyframes)
                            {
                                Material materialValue = keyframe.value as Material;
                                if(materialValue != null)
                                    animationMaterials.Add(materialValue);
                            }
                        }
                    }
                    materials.AddRange(animationMaterials);
                }

                // Duplicate materials
                foreach(Material mat in materials)
                {
                    string materialPath = AssetDatabase.GetAssetPath(mat);
                    string newMaterialPath = AssetDatabase.GenerateUniqueAssetPath($"{newMaterialsFolder}/{mat.name}.mat");

                    // Duplicate materials if they're not in assets or sub assets, like inside an fbx or if they're a default material
                    if(AssetDatabase.IsSubAsset(mat) || !materialPath.StartsWith("Assets/"))
                    {
                        var materialCopy = new Material(mat);
                        AssetDatabase.CreateAsset(materialCopy, newMaterialPath);
                    }
                    else
                    {
                        if(!AssetDatabase.CopyAsset(materialPath, newMaterialPath))
                            throw new Exception($"Failed to duplicate material: <b>{materialPath}</b> -> <b>{newMaterialPath}</b>");
                    }

                    AssetDatabase.ImportAsset(newMaterialPath);
                    var newMaterial = AssetDatabase.LoadAssetAtPath<Material>(newMaterialPath);
                    originalAndDuplicatedMaterials.Add(mat, newMaterial);
                }

                // Replace old materials with their translated copies
                foreach(var renderer in renderers)
                {
                    var sharedMats = new List<Material>();
                    renderer.GetSharedMaterials(sharedMats);
                    for(int i = 0; i < sharedMats.Count; i++)
                    {
                        if(sharedMats[i] == null)
                            continue;

                        if(originalAndDuplicatedMaterials.TryGetValue(sharedMats[i], out Material translatedMat))
                            sharedMats[i] = translatedMat;
                    }
                    renderer.sharedMaterials = sharedMats.ToArray();
                }
                var materialsFolderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(newMaterialsFolder);
                EditorGUIUtility.PingObject(materialsFolderAsset);
            }
            finally
            {
                Undo.CollapseUndoOperations(undoIndex);
            }
        }
    }
}