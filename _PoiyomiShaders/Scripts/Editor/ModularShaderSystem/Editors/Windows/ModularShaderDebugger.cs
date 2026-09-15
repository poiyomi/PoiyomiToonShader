using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Poiyomi.ModularShaderSystem.Debug
{
    public interface IModularShaderDebuggerTab
    { 
        VisualElement TabContainer { get; set; }
        
        string TabName { get; set; }

        void UpdateTab(ModularShader shader);
    }
    
    public class ModularShaderDebugger : EditorWindow
    {
        [MenuItem(MSSConstants.WINDOW_PATH + "/Modular Shader Debugger", priority = 5)]
        public static void ShowExample()
        {
            ModularShaderDebugger wnd = GetWindow<ModularShaderDebugger>();
            wnd.titleContent = new GUIContent("Modular Shader Debugger");
            
            if (wnd.position.width < 400 || wnd.position.height < 400)
            {
                Rect size = wnd.position;
                size.width = 1280;
                size.height = 720;
                wnd.position = size;
            }
            
            wnd.Show();
        }
        
        private ObjectField _modularShaderField;
        private ModularShader _modularShader;
        private VisualElement _selectedTab;

        private List<IModularShaderDebuggerTab> _tabs;

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            var styleSheet = Resources.Load<StyleSheet>(MSSConstants.RESOURCES_FOLDER + "/MSSUIElements/ModularShaderDebuggerStyle");
            var darkThemeStyleSheet = EditorGUIUtility.Load("StyleSheets/Generated/DefaultCommonDark_inter.uss.asset") as StyleSheet;
            root.styleSheets.Add(darkThemeStyleSheet);
            root.styleSheets.Add(styleSheet);
            root.style.backgroundColor = new Color(0.2196079f, 0.2196079f, 0.2196079f, 1);
            _modularShaderField = new ObjectField("Shader");
            _modularShaderField.style.flexGrow = 1;
            _modularShaderField.objectType = typeof(ModularShader);
            _modularShaderField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(e =>
            {
                if (_modularShaderField.value != null)
                    _modularShader = (ModularShader)_modularShaderField.value;
                else
                    _modularShader = null;
                
                UpdateTabs();
            });

            _tabs = new List<IModularShaderDebuggerTab>();

            var topArea = new VisualElement();
            topArea.AddToClassList("top-area");
            var refreshButton = new Button();
            refreshButton.AddToClassList("refresh-button");
            refreshButton.clicked += () => UpdateTabs();

            var buttonRow = new VisualElement();
            buttonRow.AddToClassList("button-tab-area");

            _selectedTab = new VisualElement();
            _selectedTab.style.flexGrow = 1;

            topArea.Add(_modularShaderField);
            topArea.Add(refreshButton);

            root.Add(topArea);
            root.Add(buttonRow);
            root.Add(_selectedTab);
            
            var tabTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.GetInterface(typeof(IModularShaderDebuggerTab).FullName) != null)
                .OrderBy(x => x.Name)
                .ToList();

            foreach (var type in tabTypes)
            {
                var tab = Activator.CreateInstance(type) as IModularShaderDebuggerTab;
                
                var tabButton = new Button();
                tabButton.text = tab?.TabName;
                tabButton.AddToClassList("button-tab");
                
                tabButton.clicked += () =>
                {
                    foreach (var button in buttonRow.Children())
                        if(button.ClassListContains("button-tab-selected"))
                            button.RemoveFromClassList("button-tab-selected");
                    
                    tabButton.AddToClassList("button-tab-selected");
                   
                    _selectedTab.Clear();
                    _selectedTab.Add(tab.TabContainer);
                };
                
                buttonRow.Add(tabButton);
                _tabs.Add(tab);
            }

            if (_tabs.Count == 0) return;
            var graph = _tabs.FirstOrDefault(x => x.GetType() == typeof(TemplateGraph));
            var timeline = _tabs.FirstOrDefault(x => x.GetType() == typeof(FunctionTimeline));

            if (timeline != null)
            {
                var index = _tabs.IndexOf(timeline);
                var button = buttonRow[index];
                _tabs.RemoveAt(index);
                buttonRow.RemoveAt(index);
                _tabs.Insert(0, timeline);
                buttonRow.Insert(0, button);
            }
            if (graph != null)
            {
                var index = _tabs.IndexOf(graph);
                var button = buttonRow[index];
                _tabs.RemoveAt(index);
                buttonRow.RemoveAt(index);
                _tabs.Insert(0, graph);
                buttonRow.Insert(0, button);
            }
            
            buttonRow[0].AddToClassList("button-tab-selected");
            _selectedTab.Add(_tabs[0].TabContainer);
        }

        private void UpdateTabs()
        {
            foreach (IModularShaderDebuggerTab tab in _tabs)
            {
                tab.UpdateTab(_modularShader);
            }
        }
    }
}