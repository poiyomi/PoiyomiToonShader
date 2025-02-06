#if UNITY_2022_1_OR_NEWER
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Thry.ThryEditor.ShaderTranslations
{
    public class TranslatorSectionListItem : BindableElement
    {
        string defaultFoldoutText;
        public ShaderTranslationsContainer container;
        ListView propertyList;

        public TranslatorSectionListItem(ShaderTranslatorEditor uiOwner)
        {
            var uxml = Resources.Load<VisualTreeAsset>("Shader Translator/TranslatorSectionListItem");
            uxml.CloneTree(this);

            propertyList = this.Q<ListView>("propertyList");
            propertyList.makeItem = () =>
            {
                var item = new TranslatorListItem(uiOwner);
                var innerList = item.Q<ListView>();
                item.RegisterCallback<PointerDownEvent>(_HandlePointer);

                void _HandlePointer(EventBase evt)
                {
                    if(innerList.Contains(evt.target as VisualElement))
                        evt.StopPropagation();
                }

                return item;
            };

            propertyList.itemsAdded += (indices) =>
            {
                if(indices.ToArray()[0] == 0)
                    propertyList.Rebuild();
            };

            var footer = propertyList.Q<VisualElement>("unity-list-view__footer");
            var buttonClasses = footer.Q<Button>("unity-list-view__remove-button").GetClasses();

            var clearButton = new Button(ClearList) { text = "x" };
            foreach(var className in buttonClasses)
                clearButton.AddToClassList(className);

            footer.Insert(0, clearButton);

            var foldout = this.Q<Foldout>("mainFoldout");
            defaultFoldoutText = foldout.text;

            var nameField = this.Q<TextField>("nameField")
                .RegisterValueChangedCallback(evt =>
                {
                    foldout.text = string.IsNullOrWhiteSpace(evt.newValue) ? defaultFoldoutText : evt.newValue;
                });

            void ClearList()
            {
                if(propertyList.itemsSource.Count == 0)
                    return;

                try // idk i'm sorry
                {
                    propertyList.viewController.ClearItems();
                }
                catch(ObjectDisposedException) { }
                EditorApplication.delayCall += () => propertyList.Rebuild();
            }
        }
    }
}
#endif