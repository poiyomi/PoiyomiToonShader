using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Poiyomi.ModularShaderSystem.UI;

namespace Poiyomi.ModularShaderSystem.Debug
{
    public class FunctionTimeline : IModularShaderDebuggerTab
    {
        public VisualElement TabContainer { get; set; }
        public string TabName { get; set; }

        public FunctionTimeline()
        {
            TabName = "Function Timeline";
            TabContainer = new VisualElement();
            
            var styleSheet = Resources.Load<StyleSheet>(MSSConstants.RESOURCES_FOLDER + "/MSSUIElements/FunctionTimelineStyle");
            TabContainer.styleSheets.Add(styleSheet);
            TabContainer.style.flexGrow = 1;
        }
        
        public void UpdateTab(ModularShader shader)
        {
            TabContainer.Clear();
            if (shader == null) return;
            TabContainer.Add(new TimelineContainer(shader));
        }
    }

    internal class FunctionItem : VisualElement
    {
        public ShaderFunction Function { get; }
        public TimelineRow Row { get; set; }
        public int Size { get; }
        public int Offset { get; }
        
        private TimelineRoot _root;
        
        public FunctionItem(TimelineRoot root, ShaderFunction function, int size, int offset)
        {
            _root = root;
            Function = function;
            Size = size;
            Offset = offset;
            
            style.left = Offset;
            style.width = Size;

            var label = new Label(Function.Name);
            label.AddToClassList("function-header-name");
            var queue = new Label("" + Function.Queue);
            queue.AddToClassList("function-header-queue");
            Add(label);
            Add(queue);
            
            RegisterCallback<MouseUpEvent>(evt =>
            {
                if (evt.button != 0) return;
                _root.SelectedFunction = this;
            });
        }
    }

    internal class TimelineRow : VisualElement
    {
        public ShaderModule Module { get; }
        
        public List<FunctionItem> Functions { get; set; }

        private VisualElement _title;
        public VisualElement _content;
        public List<VisualElement> _contentChilden;
        
        public TimelineRow(ShaderModule module)
        {
            Module = module;

            Functions = new List<FunctionItem>();
            _contentChilden = new List<VisualElement>();

            _title = new VisualElement();
            _content = new VisualElement();
            _title.AddToClassList("timeline-title");
            _content.AddToClassList("timeline-content");
            
            _title.Add(new Label(Module.Name));
            
            Add(_content);
            Add(_title);
        }

        public void SetContentSize(int size)
        {
            _content.style.width = size;
        }

        public void ApplyFunctionsToTimeline()
        {
            _content.Clear();
            _contentChilden.Clear();
            
            for (int index = Functions.Count - 1; index >= 0; index--)
            {
                FunctionItem function = Functions[index];
                int counter = 0;
                bool found = false;
                var functionStart = function.Offset;
                var functionEnd = function.Offset + function.Size;
                while (counter < _contentChilden.Count)
                {
                    if (!TimelineBusyAt(_contentChilden[counter], functionStart, functionEnd))
                    {
                        _contentChilden[counter].Add(function);
                        function.Row = this;
                        found = true;
                        break;
                    }
                    counter++;
                }
                if (found) continue;
                var newRow = new VisualElement();
                newRow.AddToClassList("timeline-content-row");
                _contentChilden.Add(newRow);
                _content.Add(newRow);
                newRow.Add(function);
                function.Row = this;
            }
        }

        private static bool TimelineBusyAt(VisualElement content, int functionStart, int functionEnd)
        {
            return content.Children().Cast<FunctionItem>().Any(x => functionStart < x.Offset + x.Size && functionStart > x.Offset && functionEnd <= x.Offset + x.Size);
        }

        public void SetRowsHeight()
        {
            foreach (VisualElement element in _contentChilden)
                element.style.height = element[0].resolvedStyle.height + 8;
        }
    }

    internal class TimelineRoot : VisualElement
    {
        public List<TimelineRow> Rows { get; set; }
        public List<FunctionItem> Functions { get; set; }

        public FunctionItem SelectedFunction
        {
            get => _selectedFunction;
            set
            { 
                _selectedFunction = value;
                ResetSelectedClass();
                OnSelectedFunctionChanged?.Invoke(_selectedFunction);
            }
        }

        public string Keyword { get; }

        public Action<FunctionItem> OnSelectedFunctionChanged { get; set; }
        public Action OnTimelineFirstDispatch { get; set; }
        
        private readonly Dictionary<ShaderFunction, ShaderModule> _moduleByFunction;
        private FunctionItem _selectedFunction;

        public TimelineRoot(Dictionary<ShaderFunction, ShaderModule> moduleByFunction, List<ShaderFunction> allFunctions, string rootKeyword)
        {
            Keyword = rootKeyword;
            Rows = new List<TimelineRow>();
            Functions = new List<FunctionItem>();
            int offset = 0;
            _moduleByFunction = moduleByFunction;
            foreach (var fn in allFunctions.Where(x => x.AppendAfter.Equals(rootKeyword)).OrderBy(x => x.Queue))
            {
                offset += CreateFunctionHierarchy(allFunctions, fn, offset);
            }

            foreach (TimelineRow row in Rows)
            {
                Add(row);
                row.SetContentSize(offset + 30);
                row.ApplyFunctionsToTimeline();
            }
            
            RegisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
        }
        
        private void GeometryChangedCallback(GeometryChangedEvent evt)
        {
            UnregisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
            
            foreach (TimelineRow row in Rows)
            {
                row.SetRowsHeight();
            }

            OnTimelineFirstDispatch?.Invoke();
        }
        
        private int CreateFunctionHierarchy(List<ShaderFunction> functions, ShaderFunction function, int offset)
        {
            int size = 0;
            var module = _moduleByFunction[function];
            var row = Rows.FirstOrDefault(x => x.Module == module);
            if (row == null)
            {
                row = new TimelineRow(module);
                Rows.Add(row);
            }

            foreach (var fn in functions.Where(x => x.AppendAfter.Equals(function.Name)).OrderBy(x => x.Queue))
                size += CreateFunctionHierarchy(functions, fn, offset + size + 30);

            if (size == 0) size = 224;
            else size += 30;

            var functionItem = new FunctionItem(this, function, size - 4, offset);
            row.Functions.Add(functionItem);
            Functions.Add(functionItem);

            return size;
        }
        
        public float GetScrollAdjustment()
        {
            float factor = 1.1f;
            if (Rows.Count <= 0) return factor;
            
            var width = Rows[0]._content.style.width.value.value;
            var screenWidth = Rows[0]._content.resolvedStyle.width;
            factor =  screenWidth / width;

            return factor;
        }

        public void Scroll(float f)
        {
            foreach (TimelineRow row in Rows)
            {
                var width = row._content.style.width.value.value;
                var screenWidth = row._content.resolvedStyle.width;
                if(width > screenWidth)
                    row._content.style.left = -((width- screenWidth) * f);
            }
        }

        public void ResetSelectedClass()
        {
            foreach (FunctionItem function in Functions)
            {
                if (function == SelectedFunction && !function.ClassListContains("selected-function"))
                    function.AddToClassList("selected-function");
                else if (function != SelectedFunction && function.ClassListContains("selected-function"))
                    function.RemoveFromClassList("selected-function");
            }
        }
    }
    
    internal class VariablesViewer : VisualElement
    {
        public Action<Variable> OnVariableSelected { get; set; }
        
        private List<VariableField> _variables;

        public VariablesViewer(ModularShader shader)
        {
            var variables = shader.BaseModules.Concat(shader.AdditionalModules).SelectMany(x => x.Functions).SelectMany(x => x.UsedVariables).Distinct().OrderBy(x => x.Type).ThenBy(x => x.Name);
            
            var title = new Label("Variables List");
            title.AddToClassList("area-title");
            var content = new ScrollView(ScrollViewMode.Vertical);
            content.AddToClassList("area-content");

            _variables = new List<VariableField>();

            foreach (Variable variable in variables)
            {
                var element = new VariableField(variable);
                _variables.Add(element);
                content.Add(element);

                element.RegisterCallback<MouseUpEvent>(evt =>
                {
                    if (evt.button != 0) return;
                    foreach (VariableField field in _variables)
                    {
                        if (field.ClassListContains("selected-variable-global"))
                            field.RemoveFromClassList("selected-variable-global");
                    }

                    element.AddToClassList("selected-variable-global");
                    OnVariableSelected?.Invoke(element.Variable);
                });
            }
            
            Add(title);
            Add(content);
        }
    }

    internal class FunctionViewer : VisualElement
    {
        public ShaderFunction SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                _appendAfter.Value = _selectedItem?.AppendAfter;
                _name.Value = _selectedItem?.Name;
                _queue.Value = "" + _selectedItem?.Queue;
                
                _variables.Clear();
                _variablesFoldout.Clear();
                _variableKeywordsFoldout.Clear();
                _codeKeywordsFoldout.Clear();
                if (_selectedItem == null)
                {
                    OnVariableSelected?.Invoke(null);
                    return;
                }
                
                foreach (Variable variable in _selectedItem.UsedVariables)
                {
                    var element = new VariableField(variable);
                    _variables.Add(element);
                    _variablesFoldout.Add(element);
                    
                    element.RegisterCallback<MouseUpEvent>(evt =>
                    {
                        if (evt.button != 0) return;
                        foreach (VariableField field in _variables)
                        {
                            if (field.ClassListContains("selected-variable"))
                                field.RemoveFromClassList("selected-variable");
                        }
                        element.AddToClassList("selected-variable");
                        OnVariableSelected?.Invoke(element.Variable);
                    });
                }

                
                foreach (string keyword in _selectedItem.VariableKeywords)
                    _variableKeywordsFoldout.Add(new Label(keyword));
                if(_variableKeywordsFoldout.childCount == 0)
                    _variableKeywordsFoldout.Add(new Label("None"));
                
                foreach (string keyword in _selectedItem.CodeKeywords)
                    _codeKeywordsFoldout.Add(new Label(keyword));
                if(_codeKeywordsFoldout.childCount == 0)
                    _codeKeywordsFoldout.Add(new Label("None"));
            }
        }
        
        public Action<Variable> OnVariableSelected { get; set; }

        private LabelField _appendAfter;
        private LabelField _name;
        private LabelField _queue;
        private ShaderFunction _selectedItem;
        private Foldout _variablesFoldout;
        private List<VariableField> _variables;
        private readonly Foldout _variableKeywordsFoldout;
        private readonly Foldout _codeKeywordsFoldout;

        public FunctionViewer()
        {
            var title = new Label("Selected function information");
            title.AddToClassList("area-title");
            var content = new ScrollView(ScrollViewMode.Vertical);
            content.AddToClassList("area-content");

            _name = new LabelField("Name", "");
            _appendAfter = new LabelField("Append After", "");
            _queue = new LabelField("Queue", "");

            _variablesFoldout = new Foldout();
            _variablesFoldout.text = "Variables";
            _variables = new List<VariableField>();
            
            _variableKeywordsFoldout = new Foldout();
            _variableKeywordsFoldout.text = "Variable Keywords";
            
            _codeKeywordsFoldout = new Foldout();
            _codeKeywordsFoldout.text = "Code Keywords";
            
            Add(title);
            Add(content);
            content.Add(_name);
            content.Add(_appendAfter);
            content.Add(_queue);
            content.Add(_variablesFoldout);
            content.Add(_variableKeywordsFoldout);
            content.Add(_codeKeywordsFoldout);
        }
    }

    internal class ModuleViewer : VisualElement
    {
        public ShaderModule SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                if (_selectedItem == null)
                {
                    _name.Value = null;
                    _id.Value = null;
                    _description.Value =null;
                    _author.Value = null;
                    _version.Value = null;
                    if(_content.Contains(_selectButton)) _content.Remove(_selectButton);
                    return;
                }
                _name.Value = _selectedItem.Name;
                _id.Value = _selectedItem.Id;
                _description.Value = _selectedItem.Description;
                _author.Value = _selectedItem.Author;
                _version.Value = _selectedItem.Version;
                
                if(!_content.Contains(_selectButton)) _content.Add(_selectButton);
            }
        }

        private ShaderModule _selectedItem;
        private readonly LabelField _name;
        private readonly LabelField _id;
        private readonly LabelField _author;
        private readonly LabelField _description;
        private readonly LabelField _version;
        private Button _selectButton;
        private ScrollView _content;

        public ModuleViewer()
        {
            var title = new Label("Function's module base info");
            title.AddToClassList("area-title");
            var content = new ScrollView(ScrollViewMode.Vertical);
            _content = content;
            _content.AddToClassList("area-content");
            
            _name = new LabelField("Name", "");
            _id = new LabelField("Id", "");
            _author = new LabelField("Author", "");
            _description = new LabelField("Description", "");
            _version = new LabelField("Version", "");

            _selectButton = new Button(() =>
            {
                if (_selectedItem == null) return;
                Selection.SetActiveObjectWithContext(_selectedItem,_selectedItem);
            });

            _selectButton.text = "Select module in inspector";
            
            Add(title);
            Add(_content);
            
            _content.Add(_name);
            _content.Add(_id);
            _content.Add(_author);
            _content.Add(_description);
            _content.Add(_version);
        }
    }

    internal class FunctionTemplateViewer : VisualElement
    {
        public string SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                _viewer.Text = _selectedItem;
            }
        }

        private CodeViewElement _viewer;
        private string _selectedItem;

        public FunctionTemplateViewer()
        {
            _viewer = new CodeViewElement();
            
            var title = new Label("Function code template");
            title.AddToClassList("area-title");
            
            Add(title);
            Add(_viewer);
        }
    }

    internal class TimelineContainer : VisualElement
    {
        private List<TimelineRoot> _roots;
        private PopupField<TimelineRoot> _popup;

        public TimelineContainer(ModularShader shader)
        {
            var left = new VisualElement();
            var right = new VisualElement();
            var bot = new VisualElement();
            var templateViewer = new FunctionTemplateViewer();
            var variablesViewer = new VariablesViewer(shader);
            var functionViewer = new FunctionViewer();
            var moduleViewer = new ModuleViewer();
            
            left.style.flexShrink = 0;
            left.style.width = new StyleLength(Length.Percent(70));
            right.style.width = new StyleLength(Length.Percent(30));
            bot.style.height = new StyleLength(Length.Percent(50));
            bot.style.flexDirection = FlexDirection.Row;
            style.flexDirection = FlexDirection.Row;
            Add(left);
            Add(right);
            
            var scroller = new Scroller(0,1,f =>
            {
                _popup.value.Scroll(f);
            }, SliderDirection.Horizontal);
            
            _roots = new List<TimelineRoot>();
            var functions = new List<ShaderFunction>();
            Dictionary<ShaderFunction, ShaderModule> moduleByFunction = new Dictionary<ShaderFunction, ShaderModule>();

            foreach (var module in shader.BaseModules)
            {
                functions.AddRange(module.Functions);
                foreach (var function in module.Functions)
                    if (!moduleByFunction.ContainsKey(function))
                        moduleByFunction.Add(function, module);
            }

            foreach (var module in shader.AdditionalModules)
            {
                functions.AddRange(module.Functions);
                foreach (var function in module.Functions)
                    if (!moduleByFunction.ContainsKey(function))
                        moduleByFunction.Add(function, module);
            }

            foreach (var keyword in functions.Select(x => x.AppendAfter).Distinct().Where(x => x.StartsWith("#K#")))
            {
                var root = new TimelineRoot(moduleByFunction, functions, keyword);
                root.OnSelectedFunctionChanged = item =>
                {
                    functionViewer.SelectedItem = item.Function;
                    moduleViewer.SelectedItem = item.Row.Module;
                    templateViewer.SelectedItem = item.Function.ShaderFunctionCode == null ? null : item.Function.ShaderFunctionCode.Template;

                    variablesViewer.OnVariableSelected = variable =>
                    {
                        foreach (FunctionItem f in root.Functions)
                        {
                            bool toHighlight = f.Function.UsedVariables.Any(x => x == variable);
                            
                            if(toHighlight && !f.ClassListContains("contains-variable-global"))
                                f.AddToClassList("contains-variable-global");
                            if(!toHighlight && f.ClassListContains("contains-variable-global"))
                                f.RemoveFromClassList("contains-variable-global");
                        }
                    };
                    
                    functionViewer.OnVariableSelected = variable =>
                    {
                        foreach (FunctionItem f in root.Functions)
                        {
                            bool toHighlight = f.Function.UsedVariables.Any(x => x == variable);
                            
                            if(toHighlight && !f.ClassListContains("contains-variable"))
                                f.AddToClassList("contains-variable");
                            if(!toHighlight && f.ClassListContains("contains-variable"))
                                f.RemoveFromClassList("contains-variable");
                        }
                    };
                    
                    foreach (FunctionItem f in root.Functions)
                    {
                        if(f.ClassListContains("contains-variable"))
                            f.RemoveFromClassList("contains-variable");
                    }
                };
                root.OnTimelineFirstDispatch = () =>
                {
                    scroller.Adjust(root.GetScrollAdjustment());
                };
                _roots.Add(root);
            }

            var timelineContent = new VisualElement();
            if (_roots.Count == 0)
            {
                Label label = new Label("No roots found");
                left.Add(label);
                return;
            }
            timelineContent.Add(_roots[0]);
            
            variablesViewer.OnVariableSelected = variable =>
            {
                foreach (FunctionItem f in _roots[0].Functions)
                {
                    bool toHighlight = f.Function.UsedVariables.Any(x => x == variable);
                            
                    if(toHighlight && !f.ClassListContains("contains-variable-global"))
                        f.AddToClassList("contains-variable-global");
                    if(!toHighlight && f.ClassListContains("contains-variable-global"))
                        f.RemoveFromClassList("contains-variable-global");
                }
            };
            
            var timelineScroll = new ScrollView(ScrollViewMode.Vertical);
            timelineScroll.AddToClassList("timeline");
            timelineScroll.style.flexGrow = 1;
            timelineScroll.Add(timelineContent);

            _popup = new PopupField<TimelineRoot>("Root keywords", _roots, 0, root => { return root.Keyword; }, root => { return root.Keyword; });
            _popup.RegisterValueChangedCallback(evt =>
            {
                timelineContent.Clear();
                timelineContent.Add(evt.newValue);
                functionViewer.SelectedItem = evt.newValue?.SelectedFunction?.Function;
                moduleViewer.SelectedItem = evt.newValue?.SelectedFunction?.Row.Module;
                templateViewer.SelectedItem = evt.newValue?.SelectedFunction?.Function.ShaderFunctionCode.Template;
                scroller.Adjust(evt.newValue.GetScrollAdjustment());
                scroller.value = 0;
                evt.newValue.Scroll(0);
            });
            
            scroller.Adjust(_popup.value.GetScrollAdjustment());

            timelineScroll.Add(timelineContent);
            
            left.Add(_popup);
            left.Add(timelineScroll);
            left.Add(scroller);
            left.Add(bot);
            right.Add(templateViewer);
            bot.Add(variablesViewer);
            bot.Add(functionViewer);
            bot.Add(moduleViewer);
        }
    }
}