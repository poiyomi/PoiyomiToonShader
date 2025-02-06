using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Thry.ThryEditor.ShaderTranslations
{
    [CustomEditor(typeof(ShaderTranslator))]
    public class ShaderTranslatorEditor : Editor
    {
#if UNITY_2022_1_OR_NEWER
        List<string> shaderNames;
        public List<string> sourceShaderPropertyNames;
        public List<string> targetShaderPropertyNames;

        static UnityEngine.Object[] _material;
        ListView sectionsList;

        ShaderTranslator targetTranslator;

        public string SelectedSourceShaderName { get; private set; }
        public string SelectedTargetShaderName { get; private set; }

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            var treeAsset = Resources.Load<VisualTreeAsset>("Shader Translator/TranslatorEditor");
            treeAsset.CloneTree(root);

            targetTranslator = target as ShaderTranslator;

            shaderNames = ShaderUtil.GetAllShaderInfo().Select(s => s.name)
                .Where(s => !s.StartsWith("Hidden/")).ToList();

            sectionsList = root.Q<ListView>("sectionsList");
            sectionsList.makeItem = () =>
            {
                var item = new TranslatorSectionListItem(this);
                var innerList = item.Q<ListView>();
                item.RegisterCallback<PointerDownEvent>(_HandlePointer);

                void _HandlePointer(EventBase evt)
                {
                    if(innerList.Contains(evt.target as VisualElement))
                        evt.StopPropagation();
                }
                return item;
            };

            var translationsProp = serializedObject.FindProperty(nameof(ShaderTranslator.PropertyTranslationContainers));
            sectionsList.bindItem = (element, index) =>
            {
                var listItem = element as TranslatorSectionListItem;
                listItem.container = targetTranslator.PropertyTranslationContainers[index];
                listItem.BindProperty(translationsProp.GetArrayElementAtIndex(index));
            };

            SetupShaderSelectionUI(root.Q<VisualElement>("originShaderContainer"), true);
            SetupShaderSelectionUI(root.Q<VisualElement>("targetShaderContainer"), false);

            var preModsList = root.Q<ListView>("preModsList");
            preModsList.makeItem += () => new ShaderNameMatchedModificationsEditor();

            var postModsList = root.Q<ListView>("postModsList");
            postModsList.makeItem += () => new ShaderNameMatchedModificationsEditor();

            return root;
        }

        void SetupShaderSelectionUI(VisualElement container, bool isSourceShader)
        {
            var shaderText = container.Q<TextField>("shaderText");
            var shaderDropdown = container.Q<DropdownField>("shaderDropdown");

            shaderDropdown.choices = shaderNames;
            shaderDropdown.RegisterValueChangedCallback(evt => shaderText.value = evt.newValue);

            shaderText.RegisterValueChangedCallback(evt =>
            {
                UpdateShaderProperties(evt.newValue, ref isSourceShader ? ref sourceShaderPropertyNames : ref targetShaderPropertyNames);
                if(isSourceShader)
                    SelectedSourceShaderName = shaderText.value;
                else
                    SelectedTargetShaderName = shaderText.value;
            });

            var shaderRegexText = container.Q<TextField>("shaderRegexText");
            var regexToggle = container.Q<Toggle>("shaderRegexToggle");

            regexToggle.RegisterValueChangedCallback(evt => HandleRegexEnabled(shaderText, shaderRegexText, shaderDropdown, evt.newValue));

            EditorApplication.delayCall += () =>
            {
                HandleRegexEnabled(shaderText, shaderRegexText, shaderDropdown, regexToggle.value);
            };
        }

        void HandleRegexEnabled(TextField shaderText, TextField regexText, DropdownField shaderDropdown, bool isRegexEnabled)
        {
            UIElementsHelpers.SetTextFieldReadonly(shaderText, isRegexEnabled);
            UIElementsHelpers.SetTextFieldReadonly(regexText, !isRegexEnabled);

            shaderDropdown.SetEnabled(!isRegexEnabled);
        }

        void UpdateShaderProperties(string shaderName, ref List<string> properties)
        {
            var shader = Shader.Find(shaderName);
            if(!shader)
                return;

            if(_material == null)
                _material = new UnityEngine.Object[] { new Material(shader) };

            (_material[0] as Material).shader = shader;

            properties = MaterialEditor.GetMaterialPropertyNames(_material).ToList();

            sectionsList.Rebuild();
        }
#endif
    }
}