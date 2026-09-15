using UnityEngine.UIElements;

namespace Poiyomi.ModularShaderSystem.UI
{
    public class VariableField : VisualElement
    {
        public Variable Variable { get; set; }
        
        private string _type;

        public VariableField(Variable variable)
        {
            Variable = variable;
            if(variable.Type == VariableType.Custom)
                _type = variable.CustomType;
            else
                _type = variable.Type.ToString();
            var nameField = new Label(variable.Name);
            var typeField = new Label(_type);

            nameField.style.flexGrow = 1;
            typeField.AddToClassList("variable-type-text");
            Add(nameField);
            Add(typeField);

            style.flexDirection = FlexDirection.Row;

        }
    }
}