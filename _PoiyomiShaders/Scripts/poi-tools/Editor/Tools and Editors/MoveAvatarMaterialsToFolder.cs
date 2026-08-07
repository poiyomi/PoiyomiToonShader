using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Poi.Tools
{
    public static class MoveAvatarMaterialsToFolder
    {
        public static void MoveMaterialsToNewFolder(GameObject avatar)
        {
            Undo.SetCurrentGroupName($"Move {avatar.name} Materials to Folder");
            int undoIndex = Undo.GetCurrentGroup();

            try
            {
                var renderers = avatar.GetComponentsInChildren<Renderer>(true);
                if(renderers.Length == 0)
                {
                    Debug.LogWarning($"No renderers found on {avatar.name}");
                    return;
                }

                var materials = renderers.SelectMany(ren => ren.sharedMaterials)
                    .Where(mat => mat != null)
                    .Distinct()
                    .ToList();

                if(materials.Count == 0)
                {
                    Debug.LogWarning($"No materials found on {avatar.name}");
                    return;
                }

                // Figure out a base path near the avatar's asset
                string avatarAssetPath = GetAvatarAssetPath(avatar);

                if(avatarAssetPath != "Assets")
                    avatarAssetPath = PoiHelpers.NormalizePathSlashes(Path.GetDirectoryName(avatarAssetPath));

                // Ensure folder exists
                string sanitizedAvatarName = avatar.name.SanitizePathString();
                string newMaterialsFolder = $"{avatarAssetPath}/Materials/{sanitizedAvatarName}";

                if(AssetDatabase.IsValidFolder(newMaterialsFolder))
                    newMaterialsFolder = AssetDatabase.GenerateUniqueAssetPath(newMaterialsFolder);
                PoiHelpers.EnsurePathExistsInAssets(newMaterialsFolder);

                int movedCount = 0;
                int extractedCount = 0;

                foreach(Material mat in materials)
                {
                    string materialPath = AssetDatabase.GetAssetPath(mat);
                    string newMaterialPath = AssetDatabase.GenerateUniqueAssetPath($"{newMaterialsFolder}/{mat.name}.mat");

                    // Sub-assets (e.g. inside FBX) or built-in materials can't be moved, so extract a copy
                    if(AssetDatabase.IsSubAsset(mat) || !materialPath.StartsWith("Assets/"))
                    {
                        var materialCopy = new Material(mat);
                        AssetDatabase.CreateAsset(materialCopy, newMaterialPath);
                        AssetDatabase.ImportAsset(newMaterialPath);

                        // Replace references on renderers to point to the extracted material
                        var extractedMat = AssetDatabase.LoadAssetAtPath<Material>(newMaterialPath);
                        ReplaceMaterialOnRenderers(renderers, mat, extractedMat);
                        extractedCount++;
                    }
                    else
                    {
                        // Already a standalone .mat file, just move it
                        string result = AssetDatabase.MoveAsset(materialPath, newMaterialPath);
                        if(!string.IsNullOrEmpty(result))
                        {
                            Debug.LogError($"Failed to move material: {materialPath} -> {newMaterialPath}: {result}");
                            continue;
                        }
                        movedCount++;
                    }
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                string message = $"Organized {materials.Count} material(s) into {newMaterialsFolder}";
                if(movedCount > 0) message += $"\n  Moved: {movedCount}";
                if(extractedCount > 0) message += $"\n  Extracted from sub-assets: {extractedCount}";
                Debug.Log(message);

                var materialsFolderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(newMaterialsFolder);
                EditorGUIUtility.PingObject(materialsFolderAsset);
            }
            finally
            {
                Undo.CollapseUndoOperations(undoIndex);
            }
        }

        static void ReplaceMaterialOnRenderers(Renderer[] renderers, Material oldMat, Material newMat)
        {
            foreach(var renderer in renderers)
            {
                var sharedMats = renderer.sharedMaterials;
                bool changed = false;
                for(int i = 0; i < sharedMats.Length; i++)
                {
                    if(sharedMats[i] == oldMat)
                    {
                        sharedMats[i] = newMat;
                        changed = true;
                    }
                }
                if(changed)
                {
                    Undo.RecordObject(renderer, "Replace extracted material");
                    renderer.sharedMaterials = sharedMats;
                }
            }
        }

        static string GetAvatarAssetPath(GameObject avatar)
        {
            if(PrefabUtility.IsPartOfAnyPrefab(avatar))
                return PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(avatar);

            if(avatar.TryGetComponent<Animator>(out var animator) && animator.isHuman && animator.avatar)
                return AssetDatabase.GetAssetPath(animator.avatar);

            var renderer = avatar.GetComponentInChildren<Renderer>(true);
            if(renderer is SkinnedMeshRenderer skinnedMeshRenderer && skinnedMeshRenderer.sharedMesh)
                return AssetDatabase.GetAssetPath(skinnedMeshRenderer.sharedMesh);
            if(renderer is MeshRenderer meshRenderer && meshRenderer.TryGetComponent<MeshFilter>(out var meshFilter) && meshFilter.sharedMesh)
                return AssetDatabase.GetAssetPath(meshFilter.sharedMesh);

            return "Assets";
        }
    }
}
