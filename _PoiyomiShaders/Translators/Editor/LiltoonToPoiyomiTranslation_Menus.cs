using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Poi.Tools.Menus;

namespace Poi.Tools.ShaderTranslator.Translations.Menu
{
    class LiltoonToPoiyomiTranslation_Menus
    {
        const string ShaderNameBase = ".poiyomi/Poiyomi";
        const string MaterialSwapAnimationsDetectedTitle = "Material Swap Animations Detected";
        const string MaterialSwapAnimationsDetectedMessage_Translate = "Animations that swap materials on your avatar were detected. Would you like materials inside those animations to be translated as well? This won't change your animations.\n\nAffected animations:\n{0}";
        const string MaterialSwapAnimationsDetectedMessage_TranslateCopy = "Animations that swap materials on your avatar were detected. Would you like materials inside those animations to be duplicated and the duplicates translated? This won't change your animations.\n\nAffected animations:\n{0}";

        static string[] AllShaderNames
        {
            get
            {
                if(_allShaderNames == null)
                    _allShaderNames = ShaderUtil.GetAllShaderInfo()
                        .Select(shader => shader.name)
                        .Where(name => !name.StartsWith("Hidden/"))
                        .ToArray();

                return _allShaderNames;
            }
        }
        static string[] _allShaderNames;

        static LiltoonToPoiyomiToonTranslation ToonInstance
        {
            get
            {
                if(_toonInstance == null)
                    _toonInstance = new LiltoonToPoiyomiToonTranslation();
                return _toonInstance;
            }
        }
        static LiltoonToPoiyomiToonTranslation _toonInstance;

        static LiltoonToPoiyomiProTranslation ProInstance
        {
            get
            {
                if(_proInstance == null)
                    _proInstance = new LiltoonToPoiyomiProTranslation();
                return _proInstance;
            }
        }
        static LiltoonToPoiyomiProTranslation _proInstance;

        #region Context - Material

        [MenuItem("CONTEXT/Material/Poiyomi/Translate to Poiyomi Toon", false, PoiContextMenus.ContextMaterialBase)]
        static void TranslateSelectedMaterialToPoiToon(MenuCommand command)
        {
            _TranslateSelectedMaterialContext((Material)command.context, false);
        }

        [MenuItem("CONTEXT/Material/Poiyomi/Translate Copy to Poiyomi Toon", false, PoiContextMenus.ContextMaterialBase + 1)]
        static void TranslateCopyToPoiyomiToon(MenuCommand command)
        {
            var material = command.context as Material;
            if(!PoiHelpers.TryDuplicateMaterialAsset(material, $"{material.name}_PoiToon", out string newPath))
                return;

            Material newMaterial = AssetDatabase.LoadAssetAtPath<Material>(newPath);
            _TranslateSelectedMaterialContext(newMaterial, false);
            EditorGUIUtility.PingObject(newMaterial);
        }

        [MenuItem("CONTEXT/Material/Poiyomi/Translate to Poiyomi Pro", false, PoiContextMenus.ContextMaterialBase + 14)]
        static void TranslateSelectedMaterialToPoiPro(MenuCommand command)
        {
            _TranslateSelectedMaterialContext((Material)command.context, true);
        }

        [MenuItem("CONTEXT/Material/Poiyomi/Translate Copy to Poiyomi Pro", false, PoiContextMenus.ContextMaterialBase + 15)]
        static void TranslateSelectedMaterialCopyToPoiPro(MenuCommand command)
        {
            var material = command.context as Material;
            if(!PoiHelpers.TryDuplicateMaterialAsset(material, $"{material.name}_PoiPro", out string newPath))
                return;

            Material newMaterial = AssetDatabase.LoadAssetAtPath<Material>(newPath);
            _TranslateSelectedMaterialContext(newMaterial, true);
            EditorGUIUtility.PingObject(newMaterial);
        }

        [MenuItem("CONTEXT/Material/Poiyomi/Translate to Poiyomi Pro", true)]
        [MenuItem("CONTEXT/Material/Poiyomi/Translate Copy To Poiyomi Pro", true)]
        static bool TranslateMaterialToPro_Validate() => _ProjectHasPro();

        #endregion

        #region Context - Renderer

        [MenuItem("CONTEXT/Renderer/Poiyomi/Translate Materials to Poiyomi Toon", false, PoiContextMenus.ContextRendererBase)]
        static void TranslateMaterialsToPoiyomiToon(MenuCommand command)
        {
            Undo.SetCurrentGroupName($"Translate <b>{command.context.name}</b>'s Materials to <b>Poiyomi Toon</b>");
            int undoGroupIndex = Undo.GetCurrentGroup();
            var renderer = command.context as Renderer;
            try
            {
                foreach(var material in renderer.sharedMaterials)
                {
                    _TranslateSelectedMaterialContext(material, false);
                }
            }
            finally
            {
                Undo.CollapseUndoOperations(undoGroupIndex);
            }
        }

        [MenuItem("CONTEXT/Renderer/Poiyomi/Translate Material Copies to Poiyomi Toon", false, PoiContextMenus.ContextRendererBase + 1)]
        static void TranslateRendererMaterialCopiesToPoiyomiToon(MenuCommand command)
        {
            Renderer renderer = command.context as Renderer;

            int undoIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Translate renderer <b>{renderer.name}</b>'s Materials to <b>Poiyomi Toon</b>");
            Undo.RegisterCompleteObjectUndo(renderer, $"Translate to poi toon");

            _DuplicateRendererMaterialsAndTranslate(command.context as Renderer, false);

            Undo.CollapseUndoOperations(undoIndex);
        }

        [MenuItem("CONTEXT/Renderer/Poiyomi/Translate Materials to Poiyomi Pro", false, PoiContextMenus.ContextRendererBase + 20)]
        static void TranslateMaterialsToPoiyomiPro(MenuCommand command)
        {
            Undo.SetCurrentGroupName($"Translate <b>{command.context.name}</b>'s Materials to <b>Poiyomi Pro</b>");
            int undoGroupIndex = Undo.GetCurrentGroup();
            var renderer = command.context as Renderer;
            try
            {
                foreach(var material in renderer.sharedMaterials)
                {
                    _TranslateSelectedMaterialContext(material, true);
                }
            }
            finally
            {
                Undo.CollapseUndoOperations(undoGroupIndex);
            }
        }

        [MenuItem("CONTEXT/Renderer/Poiyomi/Translate Material Copies to Poiyomi Pro", false, PoiContextMenus.ContextRendererBase + 21)]
        static void TranslateRendererMaterialCopiesToPoiyomiPro(MenuCommand command)
        {
            Renderer renderer = command.context as Renderer;

            int undoIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Translate renderer <b>{renderer.name}</b>'s Materials to <b>Poiyomi Pro</b>");
            Undo.RegisterCompleteObjectUndo(renderer, $"Translate to poi pro");

            _DuplicateRendererMaterialsAndTranslate(command.context as Renderer, true);

            Undo.CollapseUndoOperations(undoIndex);
        }

        [MenuItem("CONTEXT/Renderer/Poiyomi/Translate Materials to Poiyomi Pro", true)]
        [MenuItem("CONTEXT/Renderer/Poiyomi/Translate Material Copies to Poiyomi Pro", true)]
        static bool TranslateMaterialsToPoiyomiPro_Validate() => _ProjectHasPro();

        #endregion

        #region Context - GameObject

        [MenuItem("GameObject/Poiyomi/Materials/Translate to Poiyomi Toon", false, priority = PoiContextMenus.ContextGameObjectMaterial + 10)]
        static void TranslateAllMaterialsInObjectToToon(MenuCommand command)
        {
            int undoIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Translate <b>{command.context.name}</b> to <b>Poiyomi Toon</b>");

            if(command.context is GameObject go)
                _TranslateMaterialsInObject(go, false);

            Undo.CollapseUndoOperations(undoIndex);
        }

        [MenuItem("GameObject/Poiyomi/Materials/Translate Copy to Poiyomi Toon", false, priority = PoiContextMenus.ContextGameObjectMaterial + 11)]
        static void TranslateAllMaterialsInObjectToToonCopy(MenuCommand command)
        {
            var obj = command.context as GameObject;
            int undoIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Duplicate materials in all renderers of <b>{obj.name}</b> and it's children and translate them to <b>Poiyomi Toon</b>");

            Material[] matSwappedMaterials = null;
            if(PoiHelpers.TryGetAnimationsWithMaterialSwapsInAvatar(obj, out var animClipAndCurveBindings))
            {
                string clipNames = string.Join("\n", animClipAndCurveBindings.Keys.Select(clip => clip.name));
                if(EditorUtility.DisplayDialog(MaterialSwapAnimationsDetectedTitle, string.Format(MaterialSwapAnimationsDetectedMessage_TranslateCopy, clipNames), "Yes", "No"))
                {
                    foreach(var animationAndCurveBinding in animClipAndCurveBindings)
                    {
                        matSwappedMaterials = PoiHelpers.GetMaterialsFromMaterialSwapCurveBindings(animationAndCurveBinding.Key, animationAndCurveBinding.Value).ToArray();
                    }
                }
            }

            var translationCache = new Dictionary<Material, Material>();
            foreach(var renderer in obj.GetComponentsInChildren<Renderer>(true))
            {
                _DuplicateRendererMaterialsAndTranslate(renderer, false, translationCache);
            }

            if(matSwappedMaterials != null)
            {
                foreach(var material in matSwappedMaterials)
                {
                    if(translationCache.ContainsKey(material))
                        continue;

                    PoiHelpers.TryDuplicateMaterialAsset(material, $"{material.name}_PoiToon", out _);
                }
            }

            Undo.CollapseUndoOperations(undoIndex);
        }

        [MenuItem("GameObject/Poiyomi/Materials/Translate to Poiyomi Pro", false, priority = PoiContextMenus.ContextGameObjectMaterial + 20)]
        static void TranslateAllMaterialsInObjectToPro(MenuCommand command)
        {
            int undoIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Translate <b>{command.context.name}</b> to <b>Poiyomi Pro</b>");

            if(command.context is GameObject go)
                _TranslateMaterialsInObject(go, true);

            Undo.CollapseUndoOperations(undoIndex);
        }

        [MenuItem("GameObject/Poiyomi/Materials/Translate Copy to Poiyomi Pro", false, priority = PoiContextMenus.ContextGameObjectMaterial + 21)]
        static void TranslateAllMaterialsInObjectToProCopy(MenuCommand command)
        {
            var obj = command.context as GameObject;
            int undoIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Duplicate materials in all renderers of <b>{obj.name}</b> and it's children and translate them to <b>Poiyomi Pro</b>");

            Material[] matSwappedMaterials = null;
            if(PoiHelpers.TryGetAnimationsWithMaterialSwapsInAvatar(obj, out var animClipAndCurveBindings))
            {
                string clipNames = string.Join("\n", animClipAndCurveBindings.Keys.Select(clip => clip.name));
                if(EditorUtility.DisplayDialog(MaterialSwapAnimationsDetectedTitle, string.Format(MaterialSwapAnimationsDetectedMessage_TranslateCopy, clipNames), "Yes", "No"))
                {
                    foreach(var animationAndCurveBinding in animClipAndCurveBindings)
                    {
                        matSwappedMaterials = PoiHelpers.GetMaterialsFromMaterialSwapCurveBindings(animationAndCurveBinding.Key, animationAndCurveBinding.Value).ToArray();
                    }
                }
            }

            var translationCache = new Dictionary<Material, Material>();
            foreach(var renderer in obj.GetComponentsInChildren<Renderer>(true))
                _DuplicateRendererMaterialsAndTranslate(renderer, true, translationCache);

            if(matSwappedMaterials != null)
            {
                foreach(var material in matSwappedMaterials)
                {
                    if(translationCache.ContainsKey(material))
                        continue;

                    PoiHelpers.TryDuplicateMaterialAsset(material, $"{material.name}_PoiPro", out _);
                }
            }

            Undo.CollapseUndoOperations(undoIndex);
        }

        [MenuItem("GameObject/Poiyomi/Materials/Translate to Poiyomi Pro", true)]
        [MenuItem("GameObject/Poiyomi/Materials/Translate Copy to Poiyomi Pro", true)]
        static bool TranslateCopyToPoiyomiPro_Validate() => _ProjectHasPro();

        #endregion

        #region Assets

        [MenuItem("Assets/Poiyomi/Materials/Translate to Poiyomi Toon", false, PoiContextMenus.AssetsMenuBase + 20)]
        static void TranslateSelectedMaterialsInAssetsToToon()
        {
            int undoIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Translate selection to <b>Poiyomi Toon</b>");

            _TranslateSelectedMaterialsInAssets(false);

            Undo.CollapseUndoOperations(undoIndex);
        }

        [MenuItem("Assets/Poiyomi/Materials/Translate Copy to Poiyomi Toon", false, PoiContextMenus.AssetsMenuBase + 21)]
        static void TranslateSelectedMaterialCopyInAssetsToToon()
        {
            var materials = _GetSelectedMaterials();
            if(materials.Count == 0)
            {
                Debug.LogWarning("No materials selected to translate");
                return;
            }

            _DuplicateAndTranslateMaterials(materials, false);
            EditorGUIUtility.PingObject(materials.Last());
        }

        [MenuItem("Assets/Poiyomi/Materials/Translate to Poiyomi Pro", false, PoiContextMenus.AssetsMenuBase + 32)]
        static void TranslateSelectedMaterialsInAssetsToPro()
        {
            int undoIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName($"Translate selection to <b>Poiyomi Pro</b>");

            _TranslateSelectedMaterialsInAssets(true);

            Undo.CollapseUndoOperations(undoIndex);
        }

        [MenuItem("Assets/Poiyomi/Materials/Translate Copy to Poiyomi Pro", false, PoiContextMenus.AssetsMenuBase + 33)]
        static void TranslateSelectedMaterialCopyInAssetsToPro()
        {
            var materials = _GetSelectedMaterials();
            if(materials.Count == 0)
            {
                Debug.LogWarning("No materials selected to translate");
                return;
            }

            _DuplicateAndTranslateMaterials(materials, true);
            EditorGUIUtility.PingObject(materials.Last());
        }

        [MenuItem("Assets/Poiyomi/Materials/Translate to Poiyomi Toon", true)]
        [MenuItem("Assets/Poiyomi/Materials/Translate Copy to Poiyomi Toon", true)]
        static bool TranslateSelectedMaterialsInAssetsToToon_Validate()
        {
            return Selection.activeObject != null && Selection.objects.Any(s => s is Material || s is DefaultAsset);
        }

        [MenuItem("Assets/Poiyomi/Materials/Translate to Poiyomi Pro", true)]
        [MenuItem("Assets/Poiyomi/Materials/Translate Copy to Poiyomi Pro", true)]
        static bool TranslateSelectedMaterialsInAssetsToPro_Validate()
        {
            //Unity 2019 doesn't have .Contains(string, StringComparison) so using .IndexOf() instead
            return Selection.activeObject != null
                && Selection.objects.Any(s => s is Material || s is DefaultAsset)
                && _ProjectHasPro();
        }

        #endregion

        #region Helper Functions

        static void _TranslateMaterialsInObject(GameObject obj, bool isPro)
        {
            var allMaterials = obj.GetComponentsInChildren<Renderer>(true).SelectMany(m => m.sharedMaterials).Where(m => m != null).ToList();

            if(PoiHelpers.TryGetAnimationsWithMaterialSwapsInAvatar(obj, out var animationClipsAndMatSwapCurveBindings))
            {
                string clipNames = string.Join("\n", animationClipsAndMatSwapCurveBindings.Keys.Select(clip => clip.name));
                if(EditorUtility.DisplayDialog(MaterialSwapAnimationsDetectedTitle, string.Format(MaterialSwapAnimationsDetectedMessage_Translate, clipNames), "Yes", "No"))
                {
                    foreach(var animationAndCurveBinding in animationClipsAndMatSwapCurveBindings)
                    {
                        var materials = PoiHelpers.GetMaterialsFromMaterialSwapCurveBindings(animationAndCurveBinding.Key, animationAndCurveBinding.Value);
                        allMaterials.AddRange(materials);
                    }
                }
            }

            foreach(var mat in allMaterials.Distinct())
                _TranslateSelectedMaterialContext(mat, isPro);
        }

        static void _TranslateSelectedMaterialContext(Material material, bool isPro)
        {
            if(material == null)
                return;

            if(PoiHelpers.IsDefaultAsset(material))
            {
                Debug.LogWarning($"Material <b>{material.name}</b> is a default asset and won't be translated. You should translate a copy of it instead.");
                return;
            }
            else if(AssetDatabase.IsSubAsset(material))
            {
                Debug.LogWarning($"Material <b>{material.name}</b> is a sub asset and won't be translated. You should translate a copy.");
                return;
            }

            string suffix = isPro ? "Pro" : "Toon";
            string shaderName = $"{ShaderNameBase} {suffix}";

            Undo.RegisterCompleteObjectUndo(material, $"Translate material {material.name} to {shaderName}");

            var instance = isPro ? ProInstance : ToonInstance;
            instance.Translate(material, shaderName);
        }

        static void _TranslateSelectedMaterialsInAssets(bool isPro)
        {
            Undo.SetCurrentGroupName("Translate Material(s) to <b>Poiyomi Toon</b>");
            int undoGroupIndex = Undo.GetCurrentGroup();
            try
            {
                foreach(var obj in Selection.objects)
                {
                    if(obj is Material mat)
                    {
                        if(PoiHelpers.IsDefaultAsset(mat))
                        {
                            Debug.LogWarning($"Material <b>{mat.name}</b> is a default asset and won't be translated. You should translate a copy of it instead.");
                            continue;
                        }

                        _TranslateMaterial(mat, isPro);
                    }
                    else if(obj is DefaultAsset)
                    {
                        string folderPath = AssetDatabase.GetAssetPath(obj);
                        if(!AssetDatabase.IsValidFolder(folderPath))
                        {
                            Debug.LogWarning($"{obj} at {folderPath} is not a folder asset uh oh");
                            continue;
                        }

                        _TranslateMaterialsInFolderAndItsSubfolders(folderPath, isPro);
                    }
                }
            }
            finally
            {
                Undo.CollapseUndoOperations(undoGroupIndex);
            }
        }

        static void _TranslateMaterial(Material material, bool isPro)
        {
            if(material == null)
                return;

            if(PoiHelpers.IsDefaultAsset(material))
            {
                Debug.LogWarning($"Material <b>{material.name}</b> is a default asset and won't be translated. You should translate a copy of it instead.");
                return;
            }
            else if(AssetDatabase.IsSubAsset(material))
            {
                Debug.LogWarning($"Material <b>{material.name}</b> is a sub asset and won't be translated. You should translate a copy.");
                return;
            }

            string shaderSuffix = isPro ? "Pro" : "Toon";
            Undo.RegisterFullObjectHierarchyUndo(material, $"Translate {material.name} to Poiyomi {shaderSuffix}");

            var instance = isPro ? ProInstance : ToonInstance;
            instance.Translate(material, $"{ShaderNameBase} {shaderSuffix}");
        }

        static void _TranslateMaterialsInFolderAndItsSubfolders(string folderPath, bool isPro)
        {
            if(!AssetDatabase.IsValidFolder(folderPath))
                return;

            var materials = AssetDatabase.FindAssets("t:Material", new string[] { folderPath })
                .Where(m => m != null)
                .Select(guid => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid)));

            foreach(var material in materials)
                _TranslateMaterial(material, isPro);
        }

        static void _DuplicateAndTranslateMaterials(List<Material> materials, bool isPro)
        {
            foreach(var material in materials)
            {
                if(material == null)
                    continue;

                // Skip material if it's not using liltoon
                var translator = isPro ? ProInstance : ToonInstance;
                if(!translator.CanTranslateMaterial(material))
                    continue;

                PoiHelpers.TryDuplicateMaterialAsset(material, $"{material.name}_{(isPro ? "PoiPro" : "PoiToon")}", out string newAssetPath);
                var materialCopy = AssetDatabase.LoadAssetAtPath<Material>(newAssetPath);
                _TranslateMaterial(materialCopy, isPro);
            }
        }

        static void _DuplicateRendererMaterialsAndTranslate(Renderer renderer, bool isPro, Dictionary<Material, Material> originalAndTranslatedMaterialsCache = null)
        {
            if(originalAndTranslatedMaterialsCache == null)
                originalAndTranslatedMaterialsCache = new Dictionary<Material, Material>();

            Undo.RegisterCompleteObjectUndo(renderer, $"Copy and translate materials of {renderer.name} to {(isPro ? "Pro" : "Toon")}");

            List<Material> sharedMaterials = new List<Material>();
            renderer.GetSharedMaterials(sharedMaterials);

            for(int i = 0; i < sharedMaterials.Count; i++)
            {
                Material material = sharedMaterials[i];
                if(material == null)
                    continue;

                if(originalAndTranslatedMaterialsCache.TryGetValue(material, out Material existingMaterial))
                {
                    sharedMaterials[i] = existingMaterial;
                    continue;
                }

                // Skip material if it's not using liltoon
                var translator = isPro ? ProInstance : ToonInstance;
                if(!translator.CanTranslateMaterial(material))
                    continue;

                if(!PoiHelpers.TryDuplicateMaterialAsset(material, $"{material.name}_Poi{(isPro ? "Pro" : "Toon")}", out string newPath))
                    return;

                Material newMaterial = AssetDatabase.LoadAssetAtPath<Material>(newPath);
                _TranslateSelectedMaterialContext(newMaterial, isPro);

                sharedMaterials[i] = newMaterial;
                originalAndTranslatedMaterialsCache.Add(material, newMaterial);
            }

            renderer.sharedMaterials = sharedMaterials.ToArray();
        }

        static bool _ProjectHasPro()
        {
            return AllShaderNames.Any(name => name.Contains("Poiyomi Pro"));
        }

        static List<Material> _GetSelectedMaterials()
        {
            var materialList = new List<Material>();
            foreach(var obj in Selection.objects)
            {
                if(obj == null)
                    continue;

                if(obj is Material)
                {
                    materialList.Add(obj as Material);
                }
                if(obj is DefaultAsset)
                {
                    string folderPath = AssetDatabase.GetAssetPath(obj);
                    if(!AssetDatabase.IsValidFolder(folderPath))
                        continue;

                    materialList.AddRange(AssetDatabase.FindAssets("t:Material", new string[] { folderPath })
                        .Select(guid => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid))));
                }
            }
            return materialList;
        }

        #endregion
    }
}
