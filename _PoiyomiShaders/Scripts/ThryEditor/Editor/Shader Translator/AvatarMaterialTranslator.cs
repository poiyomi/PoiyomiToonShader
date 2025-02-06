#if UNITY_2022_1_OR_NEWER
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Thry.ThryEditor.ShaderTranslations
{
    public class AvatarMaterialTranslator : EditorWindow
    {

        const string helpBoxText = "Please select an avatar, a target shader and a translation to apply from the list.";
        DropdownField targetShaderDropdown;

        ListView materialList, translationList;
        static ObjectField avatarField;

        static List<string> allShaderNames;
        List<Material> materials = new List<Material>();

        List<ShaderTranslator> translations = new List<ShaderTranslator>();

        bool AreAllFieldsValid
        {
            get // To apply we need an avatar, any non null materials, a target shader and a selected translation
            {
                return avatarField != null && avatarField.value != null
                    && materials.Count - materials.Count(mat => mat == null) > 0
                    && translationList != null && translationList.selectedItem != null
                    && targetShaderDropdown != null && !string.IsNullOrWhiteSpace(targetShaderDropdown.value);
            }
        }

        [MenuItem("GameObject/Thry/Materials/Translate Avatar")]
        public static void ShowWindow()
        {
            var window = GetWindow<AvatarMaterialTranslator>();
            window.titleContent = new GUIContent("Material Translator");
            window.Show();
        }

        void SelectAvatar()
        {
            if(!Selection.activeGameObject || avatarField == null)
                return;

            avatarField.value = Selection.activeGameObject;
            FillMaterialListFromAvatarRenderers(Selection.activeGameObject);
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            var treeAsset = Resources.Load<VisualTreeAsset>("Shader Translator/AvatarMaterialTranslator");
            treeAsset.CloneTree(root);

            avatarField = root.Q<ObjectField>("avatarField");
            avatarField.value = Selection.activeGameObject;

            avatarField.RegisterValueChangedCallback(evt => FillMaterialListFromAvatarRenderers(evt.newValue as GameObject));

            var itemTreeAsset = Resources.Load<VisualTreeAsset>("Shader Translator/AvatarMaterialTranslatorListItem");

            materialList = root.Q<ListView>("materialList");
            materialList.itemsSource = materials;
            materialList.makeItem += () =>
            {
                var itemRoot = new VisualElement();
                itemTreeAsset.CloneTree(itemRoot);
                var objField = itemRoot.Q<ObjectField>();
                objField.SetEnabled(false);
                return itemRoot;
            };

            materialList.bindItem += (element, index) =>
            {
                var objField = element.Q<ObjectField>();
                objField.value = materials[index];
                objField.RegisterValueChangedCallback(evt =>
                {
                    materials[index] = evt.newValue as Material;
                    if(evt.newValue != null || evt.previousValue != null)
                        UpdateTranslationsList(targetShaderDropdown.value);
                });
                element.Q<Button>().RegisterCallback<MouseUpEvent>(evt =>
                {
                    materialList.viewController.RemoveItem(index);
                    materialList.Rebuild();
                    UpdateTranslationsList(targetShaderDropdown.value);
                });
            };
            materialList.itemsSourceChanged += () => UpdateTranslationsList(targetShaderDropdown.value);
            materialList.itemsRemoved += (_) => UpdateTranslationsList(targetShaderDropdown.value);
            materialList.itemsAdded += (_) => UpdateTranslationsList(targetShaderDropdown.value);

            translationList = root.Q<ListView>("translationList");
            translationList.itemsSource = translations;
            translationList.makeItem += () => new Label();
            translationList.bindItem += (element, index) =>
            {
                var label = element as Label;
                label.text = translations[index].Name;
                label.AddToClassList("label-in-list");
            };

            var applyButtonsContainer = root.Q<VisualElement>("applyButtonsContainer");
            var helpBox = new HelpBox(helpBoxText, HelpBoxMessageType.Info);
            applyButtonsContainer.Insert(0, helpBox);

            translationList.selectionChanged += (evt) =>
            {
                HandleHelpBoxAndButtonVisibility(helpBox, applyButtonsContainer, AreAllFieldsValid);
            };
            HandleHelpBoxAndButtonVisibility(helpBox, applyButtonsContainer, AreAllFieldsValid);

            targetShaderDropdown = root.Q<DropdownField>("targetShader");

            allShaderNames = AssetDatabase.FindAssets("t:Shader")
                .Select(guid => AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath(guid)).name)
                .Where(x => !x.StartsWith("Hidden/"))
                .ToList();

            targetShaderDropdown.choices = allShaderNames;
            targetShaderDropdown.RegisterValueChangedCallback((evt) => UpdateTranslationsList(evt.newValue));

            root.Q<Button>("applyButton").RegisterCallback<MouseUpEvent>(ApplyPressed);
            root.Q<Button>("applyToCopyButton").RegisterCallback<MouseUpEvent>(ApplyToCopyPressed);

            SelectAvatar();
        }

        void UpdateTranslationsList(string newShaderName)
        {
            if(string.IsNullOrWhiteSpace(newShaderName) || materials.Count == 0)
            {
                translations.Clear();
                translationList.Rebuild();
                return;
            }

            // Filter translations based on all our listed material shaders then pick translations in which source shader
            // matches any of our current materials shaders and target shader matches our selected shader in the dropdown
            var selectedMaterialShaders = materials.Where(mat => mat != null).Select(mat => mat.shader.name).Distinct();
            translations = ShaderTranslator.TranslationDefinitions.Where(trans =>
            {
                if(trans.MatchTargetShaderBasedOnRegex)
                    return selectedMaterialShaders.Any(shaderName => Regex.IsMatch(shaderName, trans.OriginShaderRegex));
                else
                    return selectedMaterialShaders.Contains(trans.OriginShader);
            }).Where(trans =>
            {
                if(trans.MatchTargetShaderBasedOnRegex)
                    return Regex.IsMatch(newShaderName, trans.TargetShaderRegex);
                else
                    return trans.TargetShader == newShaderName;
            })
            .ToList();

            translationList.itemsSource = translations;

            if(translations.Count == 1)
                translationList.selectedIndex = 0;
        }

        void HandleHelpBoxAndButtonVisibility(VisualElement helpBox, VisualElement buttonContainer, bool hideHelpBox)
        {
            helpBox.style.display = hideHelpBox ? DisplayStyle.None : DisplayStyle.Flex;
            buttonContainer.SetEnabled(hideHelpBox);
        }

        void FillMaterialListFromAvatarRenderers(GameObject avatar)
        {
            if(!avatar)
                return;

            var renderers = avatar.GetComponentsInChildren<Renderer>(true);

            if(renderers.Length == 0)
                return;

            materials = renderers
                .Where(ren => ren is SkinnedMeshRenderer || ren is MeshRenderer)
                .SelectMany(ren => ren.sharedMaterials)
                .Where(mat => mat != null)
                .Distinct()
                .ToList();

            materialList.itemsSource = materials;
        }

        void ApplyToCopyPressed(MouseUpEvent evt)
        {
            if(avatarField.value == null)
            {
                Debug.LogError("Invalid avatar selected");
                return;
            }
            Undo.RegisterFullObjectHierarchyUndo(avatarField.value, $"Translate avatar {avatarField.value.name}");
            TranslateMaterials(translations[translationList.selectedIndex], true);
        }

        void ApplyPressed(MouseUpEvent evt)
        {
            if(avatarField.value == null)
            {
                Debug.LogError("Invalid avatar selected");
                return;
            }
            Undo.RegisterFullObjectHierarchyUndo(avatarField.value, $"Translate avatar {avatarField.value.name}");
            TranslateMaterials(translations[translationList.selectedIndex], false);
        }

        void TranslateMaterials(ShaderTranslator translator, bool createBackupAvatar)
        {
            var originalAndTranslatedMaterials = new Dictionary<Material, Material>();
            Shader newShader = Shader.Find(targetShaderDropdown.value);
            if(newShader == null)
                throw new Exception($"Can't run translation {translator.Name} because shader {targetShaderDropdown.value} couldn't be found");

            // Can't seem to assign materials to duplicate correctly, so I'm replacing them on the old avatar and swapping them around
            var avatar = avatarField.value as GameObject;

            if(createBackupAvatar)
            {
                string oldName = avatar.name;
                Selection.activeObject = avatar;
                SceneView.lastActiveSceneView.Focus();
                focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("Duplicate"));

                var avatarDuplicate = Selection.activeGameObject;
                avatarDuplicate.transform.position += Vector3.right;
                avatarDuplicate.name = oldName;
                avatar.name = $"{oldName}_translated";
                avatarDuplicate.transform.SetSiblingIndex(avatar.transform.GetSiblingIndex());
            }

            Selection.activeGameObject = avatar;

            foreach(Material mat in materials)
            {
                string materialPath = AssetDatabase.GetAssetPath(mat);
                string materialFolderPath = Path.GetDirectoryName(materialPath);
                string materialName = Path.GetFileNameWithoutExtension(materialPath);

                //TODO: Sanitize path
                string newMaterialPath = $"{materialFolderPath}/{translator.Name}/{materialName}_translated.mat";

                if(!AssetDatabase.IsValidFolder($"{materialFolderPath}/{translator.Name}"))
                    AssetDatabase.CreateFolder(materialFolderPath, translator.Name);

                if(!AssetDatabase.CopyAsset(materialPath, newMaterialPath))
                    throw new Exception($"Failed to duplicate material: <b>{materialPath}</b> -> <b>{newMaterialPath}</b>");

                AssetDatabase.ImportAsset(newMaterialPath);
                var newMaterial = AssetDatabase.LoadAssetAtPath<Material>(newMaterialPath);
                originalAndTranslatedMaterials.Add(mat, newMaterial);
                TranslateMaterial(newMaterial, newShader, translator);
            }

            // Replace old materials with their translated copies
            var renderers = avatar.GetComponentsInChildren<Renderer>(true);
            foreach(var renderer in renderers)
            {
                var sharedMats = new List<Material>();
                renderer.GetSharedMaterials(sharedMats);
                for(int i = 0; i < sharedMats.Count; i++)
                {
                    if(originalAndTranslatedMaterials.TryGetValue(sharedMats[i], out Material translatedMat))
                        sharedMats[i] = translatedMat;
                }
                renderer.SetSharedMaterials(sharedMats);
            }
        }

        void TranslateMaterial(Material mat, Shader newShader, ShaderTranslator translator)
        {
            var shaderEditor = new ShaderEditor();
            shaderEditor.SetShader(newShader, mat.shader);

            int renderQueue = mat.renderQueue;

            mat.shader = newShader;
            shaderEditor.FakePartialInitilizationForLocaleGathering(newShader);
            shaderEditor.Materials[0] = mat;

            translator.Apply(shaderEditor, renderQueue);
        }
    }
}
#endif