#if UNITY_2022_1_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Thry.ThryEditor.ShaderTranslations
{
    public class TranslatorPropertySearchPopup : EditorWindow
    {
        ShaderTranslatorEditor editor;
        bool isSourceProperties;

        List<string> filteredProperties;

        private void CreateGUI()
        {
            var uxml = Resources.Load<VisualTreeAsset>("Shader Translator/TranslatorPropertySearchPopup");
            uxml.CloneTree(rootVisualElement);
        }

        public void Init(ShaderTranslatorEditor ownerEditor, bool isSource, Action<string> onSelected)
        {
            editor = ownerEditor;
            isSourceProperties = isSource;
            filteredProperties = isSourceProperties ? ownerEditor.sourceShaderPropertyNames : ownerEditor.targetShaderPropertyNames;

            var searchField = rootVisualElement.Q<ToolbarSearchField>("searchField");
            searchField.Focus();

            var propertyList = rootVisualElement.Q<ListView>("propertyList");

            propertyList.itemsSource = filteredProperties;
            propertyList.makeItem = () => new Button();
            propertyList.bindItem = (elem, index) =>
            {
                var btn = elem as Button;
                string propertyName = filteredProperties[index];
                btn.text = propertyName;
                btn.clicked += () => onSelected.Invoke(btn.text);
            };

            searchField.RegisterValueChangedCallback(evt =>
            {
                FilterProperties(evt.newValue, propertyList);
            });
        }

        void FilterProperties(string filter, ListView list)
        {
            var allProperties = isSourceProperties ? editor.sourceShaderPropertyNames : editor.targetShaderPropertyNames;
            filteredProperties = new List<string>();

            foreach(var prop in allProperties)
            {
                if(!string.IsNullOrWhiteSpace(filter) && !prop.Contains(filter, StringComparison.CurrentCultureIgnoreCase))
                    continue;

                filteredProperties.Add(prop);
            }

            list.itemsSource = filteredProperties;
        }
    }
}
#endif