using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Poiyomi.ModularShaderSystem.CibbiExtensions;

namespace Poiyomi.ModularShaderSystem.UI
{
#if UNITY_6000_4_OR_NEWER
	[UxmlElement]
#endif
	public partial class ModuleInspectorList : BindableElement, IInspectorList
	{
		Foldout _listContainer;
		Button _addButton;
		SerializedProperty _array;
		private bool _showElementsButtons;
		private List<string> _loadedModules;

		private bool _hasFoldingBeenForced;
		
		private ModularShader _modularShader;
		private ModuleCollection _moduleCollection;
		private List<bool> _localToggles;
		private string _searchFilter = "";
		private TextField _searchField;
		private VisualElement _toggleAllContainer;
		private readonly Dictionary<int, VisualElement> _moduleItemLookup = new Dictionary<int, VisualElement>();

        public InspectorListItem draggedElement { get; set; }
        public bool _highlightDrops;

        private List<VisualElement> _drops;

        private VisualElement _currentDrop;

        public ModuleInspectorList()
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
				var asset = _modularShader != null ? (Object)_modularShader : _moduleCollection;
				ModuleTogglePreferences.MoveToggle(asset, draggedElement.index, dropIndex);
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
			var type = evt.GetType();
			if ((type.Name == "SerializedPropertyBindEvent") && !string.IsNullOrWhiteSpace(bindingPath))
			{
				var obj = type.GetProperty("bindProperty")?.GetValue(evt) as SerializedProperty;
				_array = obj;
				if (obj != null)
				{
					if (_hasFoldingBeenForced) obj.isExpanded = _listContainer.value;
					else _listContainer.value = obj.isExpanded;
					
					_modularShader = obj.serializedObject.targetObject as ModularShader;
					if (_modularShader == null)
						_moduleCollection = obj.serializedObject.targetObject as ModuleCollection;
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

		private void SyncEnabledStateArray()
		{
			if (_array == null) return;
			var asset = _modularShader != null ? (Object)_modularShader : _moduleCollection;
			_localToggles = ModuleTogglePreferences.GetToggles(asset, _array.arraySize);
		}

        private bool MatchesSearch(ShaderModule module, string search)
        {
            if (string.IsNullOrWhiteSpace(search) || module == null)
                return true;
                
            string searchLower = search.ToLowerInvariant();
            
            if ((!string.IsNullOrEmpty(module.Name) && module.Name.ToLowerInvariant().Contains(searchLower)) ||
                (!string.IsNullOrEmpty(module.Id) && module.Id.ToLowerInvariant().Contains(searchLower)))
            {
                return true;
            }
            
            if (module.Templates != null)
            {
                foreach (var template in module.Templates)
                {
                    if (template.Keywords != null)
                    {
                        foreach (var keyword in template.Keywords)
                        {
                            if (!string.IsNullOrEmpty(keyword) && keyword.ToLowerInvariant().Contains(searchLower))
                                return true;
                        }
                    }
                    
                    if (template.Template != null && template.Template.Keywords != null)
                    {
                        foreach (var keyword in template.Template.Keywords)
                        {
                            if (!string.IsNullOrEmpty(keyword) && keyword.ToLowerInvariant().Contains(searchLower))
                                return true;
                        }
                    }
                }
            }
            
            return false;
        }

        private void ApplySearchHighlight()
        {
            foreach (var kvp in _moduleItemLookup)
            {
                int index = kvp.Key;
                var moduleItem = kvp.Value;
                if (moduleItem == null || _array == null || index >= _array.arraySize)
                    continue;

                ShaderModule module = (ShaderModule)_array.GetArrayElementAtIndex(index).objectReferenceValue;
                bool matchesSearch = MatchesSearch(module, _searchFilter);

                if (!string.IsNullOrWhiteSpace(_searchFilter))
                {
                    if (matchesSearch)
                    {
                        moduleItem.AddToClassList("module-search-match");
                        moduleItem.RemoveFromClassList("module-search-no-match");
                    }
                    else
                    {
                        moduleItem.AddToClassList("module-search-no-match");
                        moduleItem.RemoveFromClassList("module-search-match");
                    }
                }
                else
                {
                    moduleItem.RemoveFromClassList("module-search-match");
                    moduleItem.RemoveFromClassList("module-search-no-match");
                }
            }
        }

		public void UpdateList()
		{
			if (_array == null)
				return;
			
			bool searchFocused = _searchField != null && 
								 _searchField.focusController != null && 
								 _searchField.focusController.focusedElement == _searchField;
			
			if (searchFocused)
				return;
				
			SyncEnabledStateArray();
			
			_listContainer.text = _array.displayName;
			
			if (_localToggles != null && _localToggles.Count > 0)
			{
				if (_toggleAllContainer == null)
				{
					_toggleAllContainer = new VisualElement();
					_toggleAllContainer.style.flexDirection = FlexDirection.Row;
					_toggleAllContainer.style.marginBottom = 4;
					_toggleAllContainer.style.paddingLeft = 4;
					_toggleAllContainer.style.paddingRight = 4;
					_toggleAllContainer.name = "ToggleAllContainer";
					
					var toggleAllNew = new Toggle();
					toggleAllNew.text = "Toggle All";
					toggleAllNew.style.marginRight = 8;
					toggleAllNew.name = "ToggleAll";
					
					toggleAllNew.RegisterValueChangedCallback(evt =>
					{
						var asset = _modularShader != null ? (Object)_modularShader : _moduleCollection;
						ModuleTogglePreferences.SetAllToggles(asset, _array.arraySize, evt.newValue);
						UpdateList();
					});
					
					_toggleAllContainer.Add(toggleAllNew);
					
					_searchField = new TextField();
					_searchField.value = _searchFilter;
					_searchField.style.flexGrow = 1;
					_searchField.style.marginLeft = 8;
					_searchField.style.minWidth = 150;
					_searchField.name = "SearchField";
					
					var placeholder = new Label("Search modules...");
					placeholder.style.position = Position.Absolute;
					placeholder.style.left = 8;
					placeholder.style.top = 2;
					placeholder.style.color = new StyleColor(new Color(0.5f, 0.5f, 0.5f, 0.75f));
					placeholder.pickingMode = PickingMode.Ignore;
					placeholder.name = "SearchPlaceholder";
					if (!string.IsNullOrEmpty(_searchFilter))
						placeholder.style.display = DisplayStyle.None;
					_searchField.Add(placeholder);
					
					var clearButton = new Button(() =>
					{
						_searchFilter = "";
						_searchField.value = "";
						var ph = _searchField.Q<Label>("SearchPlaceholder");
						if (ph != null)
							ph.style.display = DisplayStyle.Flex;
						ApplySearchHighlight();
						_searchField.Focus();
					});
					clearButton.text = "✕";
					clearButton.style.width = 20;
					clearButton.style.height = 20;
					clearButton.style.marginLeft = 4;
					clearButton.style.paddingLeft = 0;
					clearButton.style.paddingRight = 0;
					clearButton.style.fontSize = 12;
					clearButton.name = "SearchClearButton";
					if (string.IsNullOrEmpty(_searchFilter))
						clearButton.style.display = DisplayStyle.None;
					
					_searchField.RegisterValueChangedCallback(evt =>
					{
						_searchFilter = evt.newValue;
						var ph = _searchField.Q<Label>("SearchPlaceholder");
						if (ph != null)
						{
							if (string.IsNullOrEmpty(evt.newValue))
								ph.style.display = DisplayStyle.Flex;
							else
								ph.style.display = DisplayStyle.None;
						}
						if (clearButton != null)
						{
							if (string.IsNullOrEmpty(evt.newValue))
								clearButton.style.display = DisplayStyle.None;
							else
								clearButton.style.display = DisplayStyle.Flex;
						}
						ApplySearchHighlight();
					});
					
					_toggleAllContainer.Add(_searchField);
					_toggleAllContainer.Add(clearButton);
					_listContainer.Add(_toggleAllContainer);
				}
				
				var toggleAll = _toggleAllContainer.Q<Toggle>("ToggleAll");
				if (toggleAll != null)
				{
					bool allEnabled = _localToggles.All(t => t);
					toggleAll.SetValueWithoutNotify(allEnabled);
				}
			}
            
            var itemsToRemove = new List<VisualElement>();
            foreach (var child in _listContainer.Children())
            {
                if (child != _toggleAllContainer && child != _addButton)
                    itemsToRemove.Add(child);
            }
            
            if (_toggleAllContainer != null)
            {
                var clearButton = _toggleAllContainer.Q<Button>("SearchClearButton");
                if (clearButton != null)
                {
                    if (string.IsNullOrEmpty(_searchFilter))
                        clearButton.style.display = DisplayStyle.None;
                    else
                        clearButton.style.display = DisplayStyle.Flex;
                }
            }
            foreach (var item in itemsToRemove)
            {
                item.RemoveFromHierarchy();
            }
            
            _drops.Clear();
            
            CreateDrop();

            _loadedModules = new List<string>();
            for (int i = 0; i < _array.arraySize; i++)
            {
                if (_array.GetArrayElementAtIndex(i).objectReferenceValue != null)
                {
                    ShaderModule shaderModule = ((ShaderModule)_array.GetArrayElementAtIndex(i).objectReferenceValue);
                    if (shaderModule is CibbiExtensions.ModuleCollection)
                        _loadedModules.AddRange(((CibbiExtensions.ModuleCollection)shaderModule).Modules.Where(x => x != null).Select(x => x.Id));
                    _loadedModules.Add(shaderModule?.Id);
                }
            }



			_moduleItemLookup.Clear();
			for (int i = 0; i < _array.arraySize; i++)
			{
				int index = i;

				var moduleItem = new VisualElement();
				moduleItem.style.flexDirection = FlexDirection.Row;
				moduleItem.style.alignItems = Align.Center;
				moduleItem.style.flexGrow = 1;
				moduleItem.style.minWidth = 0;
				
				Toggle toggle = null;
				if (_localToggles != null)
				{
					toggle = new Toggle();
					toggle.SetValueWithoutNotify(index < _localToggles.Count && _localToggles[index]);
					toggle.RegisterValueChangedCallback(evt =>
					{
						var asset = _modularShader != null ? (Object)_modularShader : _moduleCollection;
						ModuleTogglePreferences.SetToggle(asset, index, evt.newValue);
					});
					toggle.style.marginRight = 8;
					toggle.style.flexShrink = 0;
					moduleItem.Add(toggle);
				}
                
                var objectField = new ObjectField();//_array.GetArrayElementAtIndex(index));
                objectField.style.flexGrow = 1;
                objectField.style.minWidth = 0;
                objectField.style.flexShrink = 1;
                objectField.style.overflow = Overflow.Hidden;

                SerializedProperty propertyValue = _array.GetArrayElementAtIndex(index);

                objectField.objectType = typeof(ShaderModule);
                objectField.bindingPath = propertyValue.propertyPath;
                objectField.Bind(propertyValue.serializedObject);
                var infoLabel = new Label();
                moduleItem.Add(objectField);
                moduleItem.Add(infoLabel);

				EditorApplication.delayCall += () =>
				objectField.RegisterCallback<ChangeEvent<Object>>(x =>
                {
                    var newValue = (ShaderModule)x.newValue;
                    var oldValue = (ShaderModule)x.previousValue;

                    if (oldValue != null)
                    {
                        if (oldValue is CibbiExtensions.ModuleCollection)
                        {
                            ((CibbiExtensions.ModuleCollection)oldValue).Modules.Where(y => y != null).Select(y => _loadedModules.Remove(y.Id));
                        }
                        _loadedModules.Remove(oldValue.Id);
                    }
                    if (newValue != null)
                    {
                        if (newValue is CibbiExtensions.ModuleCollection)
                        {
                            _loadedModules.AddRange(((CibbiExtensions.ModuleCollection)newValue).Modules.Where(y => y != null).Select(y => y.Id));
                        }
                        _loadedModules.Add(newValue.Id);
                    }

                    for (int j = 0; j < _array.arraySize; j++)
                    {
                        var element = ((ObjectField)x.target).parent.parent.parent.ElementAt(j*2+1).ElementAt(1);
                        Label label = element.ElementAt(1) as Label;
                        if (index == j)
                            CheckModuleValidity(newValue, label, element);
                        else
                            CheckModuleValidity((ShaderModule)_array.GetArrayElementAtIndex(j).objectReferenceValue, label, element);
                    }
                });

                ShaderModule module = (ShaderModule)propertyValue.objectReferenceValue;
                
                var item = new InspectorListItem(this, moduleItem, _array, index, _showElementsButtons);
                item.removeButton.RegisterCallback<PointerUpEvent>((evt) => RemoveItem(index));
                item.duplicateButton.RegisterCallback<PointerUpEvent>((evt) => DuplicateItem(index));
                item.upButton.RegisterCallback<PointerUpEvent>((evt) => MoveUpItem(index));
                item.downButton.RegisterCallback<PointerUpEvent>((evt) => MoveDownItem(index));
                
                _listContainer.Add(item);
                _moduleItemLookup[index] = moduleItem;
                CreateDrop();

                CheckModuleValidity(module, infoLabel, moduleItem);
            }
            ApplySearchHighlight();

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

        private void CheckModuleValidity(ShaderModule newValue, Label infoLabel, VisualElement moduleItem)
        {

            List<string> problems = new List<string>();
            List<string> infos = new List<string>();
            if (newValue is CibbiExtensions.ModuleCollection)
            {
                bool hasDuplicate = false;
                foreach (var module in ((CibbiExtensions.ModuleCollection)newValue).Modules)
                {
                    if (module != null)
                    {
                        var moduleId = module.Id;
                        if (_loadedModules.Count(y => y.Equals(moduleId)) > 1 && !module.AllowDuplicates)
                            hasDuplicate = true;
                    }
                }
                if (hasDuplicate)
                    problems.Add("The ModuleCollection contains duplicate module(s)");
                    
            }

            if (newValue != null)
            {
                var moduleId = newValue.Id;
                if (_loadedModules.Count(y => y.Equals(moduleId)) > 1)
                {
                    if(!newValue.AllowDuplicates)
                        problems.Add("The module is duplicate");
                    else
                        infos.Add("The module is used multiple times");
                }

                List<string> missingDependencies = newValue.ModuleDependencies.Where(dependency => _loadedModules.Count(y => y.Equals(dependency)) == 0).ToList();
                List<string> incompatibilities = newValue.IncompatibleWith.Where(dependency => _loadedModules.Count(y => y.Equals(dependency)) > 0).ToList();

                if (missingDependencies.Count > 0)
                    problems.Add("Missing dependencies: " + string.Join(", ", missingDependencies));

                if (incompatibilities.Count > 0)
                    problems.Add("These incompatible modules are installed: " + string.Join(", ", incompatibilities));
            }

            infoLabel.text = string.Join("\n", problems.Concat(infos));

            if (!string.IsNullOrWhiteSpace(infoLabel.text))
            {
                if(problems.Count > 0)
                    moduleItem.AddToClassList("error-background");
                else
                    moduleItem.AddToClassList("info-background");
                infoLabel.visible = true;
            }
            else
            {
                moduleItem.RemoveFromClassList("info-background");
                moduleItem.RemoveFromClassList("error-background");
                infoLabel.visible = false;
            }
        }

		public void RemoveItem(int index)
		{
			if (_array != null)
			{
				var asset = _modularShader != null ? (Object)_modularShader : _moduleCollection;
				ModuleTogglePreferences.RemoveToggle(asset, index);
				if (index < _array.arraySize - 1)
					_array.GetArrayElementAtIndex(index).isExpanded = _array.GetArrayElementAtIndex(index + 1).isExpanded;
				var elementProperty = _array.GetArrayElementAtIndex(index);
				if (elementProperty.objectReferenceValue != null)
					elementProperty.objectReferenceValue = null;
				_array.DeleteArrayElementAtIndex(index);
				_array.serializedObject.ApplyModifiedProperties();
			}

			UpdateList();
		}

		public void MoveUpItem(int index)
		{
			if (_array != null && index > 0)
			{
				var asset = _modularShader != null ? (Object)_modularShader : _moduleCollection;
				ModuleTogglePreferences.MoveToggle(asset, index, index - 1);
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
				var asset = _modularShader != null ? (Object)_modularShader : _moduleCollection;
				ModuleTogglePreferences.MoveToggle(asset, index, index + 1);
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
				var asset = _modularShader != null ? (Object)_modularShader : _moduleCollection;
				ModuleTogglePreferences.DuplicateToggle(asset, index);
				_array.serializedObject.ApplyModifiedProperties();
			}

			UpdateList();
		}

		public void AddItem()
		{
			if (_array != null)
			{
				_array.InsertArrayElementAtIndex(_array.arraySize);
				var asset = _modularShader != null ? (Object)_modularShader : _moduleCollection;
				ModuleTogglePreferences.InsertToggle(asset, _array.arraySize - 1, true);
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
        public new class UxmlFactory : UxmlFactory<ModuleInspectorList, UxmlTraits> { }

        public new class UxmlTraits : BindableElement.UxmlTraits
        {
            UxmlBoolAttributeDescription showElements =
                new UxmlBoolAttributeDescription { name = "show-elements-text", defaultValue = true };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                if (ve is ModuleInspectorList ate) ate._showElementsButtons = showElements.GetValueFromBag(bag, cc);
            }
        }
#endif
    }
}