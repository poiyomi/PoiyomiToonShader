#if UNITY_2022_1_OR_NEWER
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Thry.ThryEditor.ShaderTranslations
{
    public class TranslatorListItem : BindableElement
    {
        public TextField sourceField, targetField, expressionField;
        public Button openSourceFieldPopup, openTargetFieldPopup;
        public Toggle advancedToggle;
        public VisualElement conditionalContainer;
        public ListView conditionalList;

        public ShaderTranslatorEditor translatorEditor;

        public TranslatorListItem(ShaderTranslatorEditor uiOwner)
        {
            var uxml = Resources.Load<VisualTreeAsset>("Shader Translator/TranslatorListItem");
            uxml.CloneTree(this);

            translatorEditor = uiOwner;

            sourceField = this.Q<TextField>("sourceProperty");
            targetField = this.Q<TextField>("targetProperty");
            expressionField = this.Q<TextField>("mathExpression");
            advancedToggle = this.Q<Toggle>("advancedToggle");
            conditionalContainer = this.Q<VisualElement>("conditionalContainer");

            advancedToggle.RegisterValueChangedCallback((evt) =>
                SetContainerVisibleAndTextFieldDisabled(conditionalContainer, expressionField, evt.newValue));

            SetContainerVisibleAndTextFieldDisabled(conditionalContainer, expressionField, advancedToggle.value);

            conditionalList = this.Q<ListView>("conditionalList");
            conditionalList.makeItem = () => new ConditionalTranslationBlockListItem();

            SetupButton("sourcePropertyPopupButton", sourceField, translatorEditor, true);
            SetupButton("targetPropertyPopupButton", targetField, translatorEditor, false);
        }

        void SetupButton(string name, TextField targetField, ShaderTranslatorEditor editor, bool isSource)
        {
            var button = this.Q<Button>(name);
            button.clicked += () =>
            {
                if(EditorWindow.HasOpenInstances<TranslatorPropertySearchPopup>())
                    EditorWindow.GetWindow<TranslatorPropertySearchPopup>().Close();

                var window = EditorWindow.CreateInstance<TranslatorPropertySearchPopup>();
                window.titleContent = new GUIContent(isSource ? editor.SelectedSourceShaderName : editor.SelectedTargetShaderName);
                window.ShowUtility();
                window.Init(editor, isSource, (propertyName) =>
                {
                    targetField.value = propertyName;
                    window.Close();
                });
            };
        }

        void SetContainerVisibleAndTextFieldDisabled(VisualElement container, TextField textField, bool isVisible)
        {
            container.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
            UIElementsHelpers.SetTextFieldReadonly(textField, isVisible);
        }
    }
}
#endif