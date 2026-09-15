using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Poiyomi.ModularShaderSystem.UI
{
    [CustomPropertyDrawer(typeof(ModuleTemplate))]
    public class ModuleTemplatePropertyDrawer : PropertyDrawer
    {
        private VisualElement _root;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            _root = new VisualElement();

            var visualTree = Resources.Load<VisualTreeAsset>(MSSConstants.RESOURCES_FOLDER + "/MSSUIElements/ModuleTemplatePropertyDrawer");
            VisualElement template = visualTree.CloneTree();
            var foldout = new Foldout();
            foldout.text = property.displayName;
            foldout.RegisterValueChangedCallback((e) => property.isExpanded = e.newValue);
            foldout.value = property.isExpanded;
            foldout.Add(template);
            _root.Add(foldout);

            return _root;
        }
    }
}