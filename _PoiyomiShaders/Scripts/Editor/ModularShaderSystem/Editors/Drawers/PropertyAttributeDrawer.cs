using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Poiyomi.ModularShaderSystem.UI
{
    public class PropertyAttributeAttribute : PropertyAttribute 
    {
    }
    [CustomPropertyDrawer(typeof(PropertyAttributeAttribute))]
    public class PropertyAttributeDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            var value = new TextField();
            value.SetValueWithoutNotify(property.stringValue);
            root.Add(value);

            /*value.RegisterValueChangedCallback(evt =>
            {
                property.stringValue = evt.newValue;
                property.serializedObject.ApplyModifiedProperties();
            });*/
            value.RegisterCallback<FocusOutEvent>(evt =>
            {
                string v = value.value;
                if (v[v.Length - 1] == ']')
                {
                    v = v.Remove(v.Length - 1, 1);
                }
                if (v[0] == '[')
                {
                    v = v.Remove(0, 1);
                }
                property.stringValue = v;
                value.SetValueWithoutNotify(property.stringValue);
                property.serializedObject.ApplyModifiedProperties();
            });

            /*customTypeField.style.display = ((VariableType)typeField.value) == VariableType.Custom ? DisplayStyle.Flex : DisplayStyle.None;
            
            typeField.RegisterValueChangedCallback(e =>
            {
                customTypeField.style.display = ((VariableType)e.newValue) == VariableType.Custom ? DisplayStyle.Flex : DisplayStyle.None;
            });*/

            return root;
        }
    }
}