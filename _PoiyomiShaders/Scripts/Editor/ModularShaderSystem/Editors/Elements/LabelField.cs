using UnityEngine.UIElements;

namespace Poiyomi.ModularShaderSystem.UI
{
    public class LabelField : VisualElement
    {
        public string Label
        {
            get => _label;
            set
            {
                _label = value;
                _labelField.text = _label;
            }
        }

        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                _valueField.text = _value;
            }
        }

        private Label _labelField;
        private Label _valueField;
        private string _label;
        private string _value;

        public LabelField(string label, string value)
        {
            _label = label;
            _value = value;
            _labelField = new Label(label);
            _valueField = new Label(value);
            
            AddToClassList("unity-base-field");
            AddToClassList("unity-base-text-field");
            AddToClassList("unity-text-field");
            
            _labelField.AddToClassList("unity-text-element");
            _labelField.AddToClassList("unity-label");
            _labelField.AddToClassList("unity-base-field__label");
            _labelField.AddToClassList("unity-base-text-field__label");
            _labelField.AddToClassList("unity-text-field__label");
            _labelField.AddToClassList("label-field-title");
            _valueField.AddToClassList("label-field-value");
            
            Add(_labelField);
            Add(_valueField);
            
        }
    }
}