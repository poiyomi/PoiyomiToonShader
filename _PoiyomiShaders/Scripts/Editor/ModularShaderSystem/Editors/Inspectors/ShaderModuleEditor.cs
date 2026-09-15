using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Poiyomi.ModularShaderSystem.UI
{
    [CustomEditor(typeof(ShaderModule))]
    public class ShaderModuleEditor : Editor
    {
        private VisualElement _root;

        public override VisualElement CreateInspectorGUI()
        {
            _root = new VisualElement();

            var visualTree = Resources.Load<VisualTreeAsset>(MSSConstants.RESOURCES_FOLDER + "/MSSUIElements/ShaderModuleEditor");
            VisualElement template = visualTree.CloneTree();
            _root.Add(template);

            // add listerner to AllowDuplicates toggle. show ForceDuplicateLogic toggle if true
            var allowDuplicatesToggle = template.Q<Toggle>("AllowDuplicates");
            var forceDuplicateLogicToggle = template.Q<Toggle>("ForceDuplicateLogic");
            allowDuplicatesToggle.RegisterValueChangedCallback(evt =>
            {
                forceDuplicateLogicToggle.SetEnabled(evt.newValue);
                if (!evt.newValue)
                {
                    forceDuplicateLogicToggle.SetValueWithoutNotify(false);
                }
            });
            forceDuplicateLogicToggle.SetEnabled(forceDuplicateLogicToggle.value);

            return _root;
        }
    }
}