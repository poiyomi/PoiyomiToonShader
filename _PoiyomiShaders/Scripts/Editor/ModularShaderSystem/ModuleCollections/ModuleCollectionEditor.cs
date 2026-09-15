using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Poiyomi.ModularShaderSystem.CibbiExtensions.UI
{
    [CustomEditor(typeof(ModuleCollection))]
    public class ModuleCollectionEditor : Editor
    {
        private VisualElement _root;
        private ModuleCollection _collection;

        public override VisualElement CreateInspectorGUI()
        {
            _root = new VisualElement();
            _collection = (ModuleCollection)target;

            var visualTree = Resources.Load<VisualTreeAsset>(MSSConstants.RESOURCES_FOLDER + "/MSSUIElements/ModuleCollectionEditor");
            VisualElement template = visualTree.CloneTree();
            _root.Add(template);
            
			return _root;
        }
    }
}