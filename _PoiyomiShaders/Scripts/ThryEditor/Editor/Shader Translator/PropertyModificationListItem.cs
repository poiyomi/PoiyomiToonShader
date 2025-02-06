using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
#if !UNITY_2022_1_OR_NEWER
using UnityEditor.UIElements;
#endif
namespace Thry.ThryEditor.ShaderTranslations
{
    public class PropertyModificationListItem : BindableElement
    {
        public PropertyModificationListItem()
        {
            var treeAsset = Resources.Load<VisualTreeAsset>("Shader Translator/ModificationListItem");
            treeAsset.CloneTree(this);

            var actionTypeField = this.Q<EnumField>("actionType");
            var propertyNameField = this.Q<TextField>("propertyName");

            actionTypeField.RegisterValueChangedCallback(evt =>
            {
                if(evt.newValue == null)
                    return;

                var value = (ShaderModificationAction.ActionType)evt.newValue;
                SetPropertyFieldVisible(propertyNameField, value == ShaderModificationAction.ActionType.SetTargetPropertyValue);
            });

            EditorApplication.delayCall += () =>
            {
                if(actionTypeField.value == null)
                {
                    SetPropertyFieldVisible(propertyNameField, false);
                    return;
                }

                var actionFieldValue = (ShaderModificationAction.ActionType)actionTypeField.value;
                SetPropertyFieldVisible(propertyNameField, ShaderModificationAction.ActionType.SetTargetPropertyValue == actionFieldValue);
            };
        }

        void SetPropertyFieldVisible(VisualElement field, bool isVisible)
        {
            field.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
