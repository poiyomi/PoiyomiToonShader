using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Poiyomi.ModularShaderSystem.UI
{
    [CustomPropertyDrawer(typeof(Variable))]
    public class VariablePropertyDrawer : PropertyDrawer
    {
        private VisualElement _root;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            _root = new VisualElement();

            var visualTree = Resources.Load<VisualTreeAsset>(MSSConstants.RESOURCES_FOLDER + "/MSSUIElements/VariablePropertyDrawer");
            VisualElement template = visualTree.CloneTree();
            var foldout = new Foldout();
            foldout.text = property.displayName;
            foldout.RegisterValueChangedCallback((e) => property.isExpanded = e.newValue);
            foldout.value = property.isExpanded;
            foldout.Add(template);
            _root.Add(foldout);
            
            var nameField = template.Q<TextField>("Name");
            nameField.RegisterValueChangedCallback(evt => foldout.text = evt.newValue);
            var typeField = template.Q<EnumField>("Type");
            var customTypeField = template.Q<VisualElement>("CustomType");

            typeField.Init((VariableType)property.FindPropertyRelative("Type").intValue);
            
            customTypeField.style.display = ((VariableType)typeField.value) == VariableType.Custom ? DisplayStyle.Flex : DisplayStyle.None;
            
            typeField.RegisterValueChangedCallback(e =>
            {
                customTypeField.style.display = ((VariableType)e.newValue) == VariableType.Custom ? DisplayStyle.Flex : DisplayStyle.None;
            });

            return _root;
        }
    }
}