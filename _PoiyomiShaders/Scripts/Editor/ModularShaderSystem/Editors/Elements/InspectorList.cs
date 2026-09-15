using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Poiyomi.ModularShaderSystem.UI
{
    public interface IInspectorList
    {
        InspectorListItem draggedElement { get; set; }
        void HighlightDrops();
        void DeHighlightDrops();
    }

#if UNITY_6000_4_OR_NEWER
    [UxmlElement]
#endif
    public partial class InspectorList : BindableElement, IInspectorList
    {
        Foldout _listContainer;
        Button _addButton;
        SerializedProperty _array;
        private bool _showElementsButtons;

        private bool _hasFoldingBeenForced;

        public InspectorListItem draggedElement { get; set; }
        public bool _highlightDrops;

        private List<VisualElement> _drops;

        private VisualElement _currentDrop;

        public InspectorList()
        {
            _drops = new List<VisualElement>();
            _listContainer = new Foldout();
            _listContainer.text = "Unbound List";
            _listContainer.contentContainer.AddToClassList("inspector-list-container");
            _listContainer.value = false;
            _listContainer.RegisterCallback<MouseUpEvent>(e => Drop());
            _listContainer.RegisterCallback<MouseLeaveEvent>(e => Drop());

            _addButton = new Button(AddItem);
            _addButton.text = "Add";
            _addButton.AddToClassList("inspector-list-add-button");
            Add(_listContainer);
            if (enabledSelf)
                _listContainer.Add(_addButton);
            _listContainer.RegisterValueChangedCallback((e) => _array.isExpanded = e.newValue);
            var styleSheet = Resources.Load<StyleSheet>(MSSConstants.RESOURCES_FOLDER + "/MSSUIElements/InspectorList");
            styleSheets.Add(styleSheet);
        }

        private void Drop()
        {
            if (draggedElement == null) return;
            draggedElement.RemoveFromClassList("inspector-list-drag-enabled");

            if (_highlightDrops)
            {
                DeHighlightDrops();
                int dropIndex = _drops.IndexOf(_currentDrop);

                if (dropIndex == -1)
                {
                    draggedElement = null;
                    return;
                }

                if (dropIndex > draggedElement.index) dropIndex--;
                _array.MoveArrayElement(draggedElement.index, dropIndex);
                bool expanded = _array.GetArrayElementAtIndex(dropIndex).isExpanded;
                _array.GetArrayElementAtIndex(dropIndex).isExpanded = _array.GetArrayElementAtIndex(draggedElement.index).isExpanded;
                _array.GetArrayElementAtIndex(draggedElement.index).isExpanded = expanded;
                _array.serializedObject.ApplyModifiedProperties();
                UpdateList();
            }
            draggedElement = null;
        }

        public void HighlightDrops()
        {
            foreach (var item in _drops)
                item.AddToClassList("inspector-list-drop-area-highlight");

            _highlightDrops = true;
        }

        public void DeHighlightDrops()
        {
            foreach (var item in _drops)
                item.RemoveFromClassList("inspector-list-drop-area-highlight");

            _highlightDrops = false;
        }

#if UNITY_6000_2_OR_NEWER
        protected override void HandleEventBubbleUp(EventBase evt)
#elif UNITY_2022_1_OR_NEWER
        protected override void ExecuteDefaultAction(EventBase evt)
#else
        public override void HandleEvent(EventBase evt)
#endif
        {
            var type = evt.GetType(); //SerializedObjectBindEvent is internal, so need to use reflection here
            if ((type.Name == "SerializedPropertyBindEvent") && !string.IsNullOrWhiteSpace(bindingPath))
            {
                var obj = type.GetProperty("bindProperty")?.GetValue(evt) as SerializedProperty;
                _array = obj;
                if (obj != null)
                {
                    if (_hasFoldingBeenForced) obj.isExpanded = _listContainer.value;
                    else _listContainer.value = obj.isExpanded;
                }
                UpdateList();
            }
#if UNITY_6000_2_OR_NEWER
            base.HandleEventBubbleUp(evt);
#elif UNITY_2022_1_OR_NEWER
            base.ExecuteDefaultAction(evt);
#else
            base.HandleEvent(evt);
#endif
        }

        public void UpdateList()
        {
            _listContainer.Clear();
            _drops.Clear();

            if (_array == null)
                return;
            _listContainer.text = _array.displayName;
            CreateDrop();
            for (int i = 0; i < _array.arraySize; i++)
            {
                int index = i;
                var element = new PropertyField(_array.GetArrayElementAtIndex(index));
                element.Bind(_array.GetArrayElementAtIndex(index).serializedObject);
                var item = new InspectorListItem(this, element, _array, index, _showElementsButtons);
                item.removeButton.RegisterCallback<PointerUpEvent>((evt) => RemoveItem(index));
                item.duplicateButton.RegisterCallback<PointerUpEvent>((evt) => DuplicateItem(index));
                item.upButton.RegisterCallback<PointerUpEvent>((evt) => MoveUpItem(index));
                item.downButton.RegisterCallback<PointerUpEvent>((evt) => MoveDownItem(index));
                _listContainer.Add(item);
                CreateDrop();
            }
            if (enabledSelf)
                _listContainer.Add(_addButton);
        }

        private void CreateDrop()
        {
            VisualElement dropArea = new VisualElement();
            dropArea.AddToClassList("inspector-list-drop-area");
            dropArea.RegisterCallback<MouseEnterEvent>(e =>
            {
                if (_highlightDrops)
                {
                    dropArea.AddToClassList("inspector-list-drop-area-selected");
                    _currentDrop = dropArea;
                }
            });
            dropArea.RegisterCallback<MouseLeaveEvent>(e =>
            {
                if (_highlightDrops)
                {
                    dropArea.RemoveFromClassList("inspector-list-drop-area-selected");
                    if (_currentDrop == dropArea) _currentDrop = null;
                }
            });
            
            _listContainer.Add(dropArea);
            _drops.Add(dropArea);
        }

        public void RemoveItem(int index)
        {
            if (_array != null)
            {
                if (index < _array.arraySize - 1)
                    _array.GetArrayElementAtIndex(index).isExpanded = _array.GetArrayElementAtIndex(index + 1).isExpanded;
                _array.DeleteArrayElementAtIndex(index);
                _array.serializedObject.ApplyModifiedProperties();
            }

            UpdateList();
        }

        public void MoveUpItem(int index)
        {
            if (_array != null && index > 0)
            {
                _array.MoveArrayElement(index, index - 1);
                bool expanded = _array.GetArrayElementAtIndex(index).isExpanded;
                _array.GetArrayElementAtIndex(index).isExpanded = _array.GetArrayElementAtIndex(index - 1).isExpanded;
                _array.GetArrayElementAtIndex(index - 1).isExpanded = expanded;
                _array.serializedObject.ApplyModifiedProperties();
            }

            UpdateList();
        }

        public void MoveDownItem(int index)
        {
            if (_array != null && index < _array.arraySize - 1)
            {
                _array.MoveArrayElement(index, index + 1);
                bool expanded = _array.GetArrayElementAtIndex(index).isExpanded;
                _array.GetArrayElementAtIndex(index).isExpanded = _array.GetArrayElementAtIndex(index + 1).isExpanded;
                _array.GetArrayElementAtIndex(index + 1).isExpanded = expanded;
                _array.serializedObject.ApplyModifiedProperties();
            }

            UpdateList();
        }

        public void DuplicateItem(int index)
        {
            if (_array != null && index < _array.arraySize)
            {
                _array.InsertArrayElementAtIndex(index + 1);
                var sourceElement = _array.GetArrayElementAtIndex(index);
                var duplicateElement = _array.GetArrayElementAtIndex(index + 1);
                duplicateElement.objectReferenceValue = sourceElement.objectReferenceValue;
                duplicateElement.isExpanded = sourceElement.isExpanded;
                _array.serializedObject.ApplyModifiedProperties();
            }

            UpdateList();
        }

        public void AddItem()
        {
            if (_array != null)
            {
                _array.InsertArrayElementAtIndex(_array.arraySize);
                _array.serializedObject.ApplyModifiedProperties();
            }

            UpdateList();
        }

        public void SetFoldingState(bool open)
        {
            _listContainer.value = open;
            if (_array != null) _array.isExpanded = open;
            else _hasFoldingBeenForced = true;
        }

#if UNITY_6000_4_OR_NEWER
        [UxmlAttribute("show-elements-text")]
        public bool ShowElementsText { get => _showElementsButtons; set => _showElementsButtons = value; }
#else
        public new class UxmlFactory : UxmlFactory<InspectorList, UxmlTraits> { }

        public new class UxmlTraits : BindableElement.UxmlTraits
        {
            UxmlBoolAttributeDescription showElements =
                new UxmlBoolAttributeDescription { name = "show-elements-text", defaultValue = true };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                if (ve is InspectorList ate) ate._showElementsButtons = showElements.GetValueFromBag(bag, cc);
            }
        }
#endif

    }

    public class InspectorListItem : VisualElement
    {
        public Button removeButton;
        public Button upButton;
        public Button downButton;
        public Button duplicateButton;

        public VisualElement dragArea;
        public VisualElement buttonsArea;

        public Vector2 startPosition;

        public int index;

        private IInspectorList _list;
        public InspectorListItem(IInspectorList list, VisualElement element, SerializedProperty array, int index, bool showButtonsText)
        {
            this.index = index;
            _list = list;
            AddToClassList("inspector-list-item-container");

            dragArea = new VisualElement();
            dragArea.AddToClassList("inspector-list-drag-handle");

            dragArea.RegisterCallback<MouseDownEvent>(e =>
            {
                if (_list.draggedElement == this)
                {
                    e.StopImmediatePropagation();
                    return;
                }

                _list.draggedElement = this;
                _list.HighlightDrops();
                AddToClassList("inspector-list-drag-enabled");
            });

            buttonsArea = new VisualElement();

            RegisterCallback<GeometryChangedEvent>(e =>
            {
                buttonsArea.ClearClassList();
                if (e.newRect.height > 60)
                {
                    buttonsArea.AddToClassList("inspector-list-buttons-container-vertical");
                    buttonsArea.Add(removeButton);
                    buttonsArea.Add(duplicateButton);
                    buttonsArea.Add(upButton);
                    buttonsArea.Add(downButton);
                }
                else
                {
                    buttonsArea.AddToClassList("inspector-list-buttons-container-horizontal");
                    buttonsArea.Add(duplicateButton);
                    buttonsArea.Add(upButton);
                    buttonsArea.Add(downButton);
                    buttonsArea.Add(removeButton);
                }
            });

            duplicateButton = new Button();
            duplicateButton.name = "DuplicateInspectorListItem";
            duplicateButton.AddToClassList("inspector-list-duplicate-button");
            var duplicateIcon = EditorGUIUtility.IconContent("d_Toolbar Plus");
            if (duplicateIcon != null && duplicateIcon.image != null)
            {
                duplicateButton.style.backgroundImage = new StyleBackground(duplicateIcon.image as Texture2D);
            }
            upButton = new Button();
            upButton.name = "UpInspectorListItem";
            upButton.AddToClassList("inspector-list-up-button");
            var upIcon = EditorGUIUtility.IconContent("d_scrollup");
            if (upIcon != null && upIcon.image != null)
            {
                upButton.style.backgroundImage = new StyleBackground(upIcon.image as Texture2D);
            }
            if (index == 0)
                upButton.SetEnabled(false);
            downButton = new Button();
            downButton.name = "DownInspectorListItem";
            downButton.AddToClassList("inspector-list-down-button");
            var downIcon = EditorGUIUtility.IconContent("d_scrolldown");
            if (downIcon != null && downIcon.image != null)
            {
                downButton.style.backgroundImage = new StyleBackground(downIcon.image as Texture2D);
            }
            if (index >= array.arraySize - 1)
                downButton.SetEnabled(false);
            removeButton = new Button();
            removeButton.name = "RemoveInspectorListItem";
            removeButton.AddToClassList("inspector-list-remove-button");
            var removeIcon = EditorGUIUtility.IconContent("d_Toolbar Minus");
            if (removeIcon != null && removeIcon.image != null)
            {
                removeButton.style.backgroundImage = new StyleBackground(removeIcon.image as Texture2D);
            }

            if (showButtonsText)
            {
                duplicateButton.text = "dup";
                upButton.text = "up";
                downButton.text = "down";
                removeButton.text = "-";
            }

            var property = array.GetArrayElementAtIndex(index);
            element.AddToClassList("inspector-list-item");

            Add(dragArea);
            Add(element);
            Add(buttonsArea);
        }
    }
}