using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Poiyomi.ModularShaderSystem.UI
{
    [CustomPropertyDrawer(typeof(ShaderFunction))]
    public class FunctionPropertyDrawer : PropertyDrawer
    {
        private VisualElement _root;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            _root = new VisualElement();

            var visualTree =  Resources.Load<VisualTreeAsset>(MSSConstants.RESOURCES_FOLDER + "/MSSUIElements/FunctionPropertyDrawer");
            VisualElement template = visualTree.CloneTree();
            var foldout = new Foldout();
            foldout.text = property.displayName;
            foldout.RegisterValueChangedCallback((e) => property.isExpanded = e.newValue);
            foldout.value = property.isExpanded;
            
            var nameField = template.Q<TextField>("Name");
            nameField.RegisterValueChangedCallback(evt => foldout.text = evt.newValue);
            
            foldout.Add(template);
            _root.Add(foldout);

            return _root;
        }
    }
}