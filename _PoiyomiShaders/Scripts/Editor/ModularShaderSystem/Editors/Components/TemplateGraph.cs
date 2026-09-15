using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Poiyomi.ModularShaderSystem.UI;


namespace Poiyomi.ModularShaderSystem.Debug
{
    public class TemplateGraph : IModularShaderDebuggerTab
    {
        public VisualElement TabContainer { get; set; }
        public string TabName { get; set; }

        internal TemplateGraphView _graph;

        public TemplateGraph()
        {
            TabName = "Template graph";
            TabContainer = new VisualElement();
            TabContainer.StretchToParentSize();
            var styleSheet = Resources.Load<StyleSheet>(MSSConstants.RESOURCES_FOLDER + "/MSSUIElements/TemplateGraphStyle");
            TabContainer.styleSheets.Add(styleSheet);
        }
        
        public void UpdateTab(ModularShader shader)
        {
            TabContainer.Clear();
            if (shader == null) return;
            _graph = new TemplateGraphView(shader);
            
            
            
            TabContainer.Add(_graph);
        }
    }

    internal class TemplateGraphView : GraphView
    {
        public List<TemplateNode> Nodes;
        public List<TemplateNode> BaseNodes;
        
        private List<ShaderModule> _modules;
        private ModularShader _shader;
        
        private static TextPopup _popup;

        public TemplateGraphView(ModularShader shader)
        {
            Nodes = new List<TemplateNode>();
            BaseNodes = new List<TemplateNode>();

            _modules = shader.BaseModules.Concat(shader.AdditionalModules).ToList();
            _shader = shader;
            
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            var grid = new GridBackground();

            Insert(0, grid);
            grid.StretchToParentSize();
            this.StretchToParentSize();
            
            AddBaseTemplateNode("Shader", _shader.ShaderTemplate);

            if (_shader.UseTemplatesForProperties)
            {
                var keywords = new []{"#K#" + MSSConstants.TEMPLATE_PROPERTIES_KEYWORD};
                AddBaseTemplateNode("ShaderPropertiesRoot", new TemplateAsset{ Template = "", Keywords = keywords, name = "Properties Template Root"});
                if (_shader.ShaderPropertiesTemplate != null) AddTemplateNode("ShaderPropertiesTemplate", _shader.ShaderTemplate, keywords);
                
            }
            
            var moduleByTemplate = new Dictionary<ModuleTemplate, ShaderModule>();
            foreach (var module in _shader.BaseModules.Concat(_shader.AdditionalModules))
            foreach (var template in module.Templates)
                moduleByTemplate.Add(template, module);
            
            foreach (var template in  _shader.BaseModules.Concat(_shader.AdditionalModules).SelectMany(x => x.Templates).OrderBy(x => x.Queue))
            {
                if (template.Template == null) continue;
                var module = moduleByTemplate[template];
                AddTemplateNode(module.Id, template);
            }
            
            ScheduleNodesPositionReset();
        }

        public void AddBaseTemplateNode(string moduleId, TemplateAsset template)
        {
            var baseNode = new TemplateNode(moduleId, template, "");
            AddElement(baseNode);
            Nodes.Add(baseNode);
            BaseNodes.Add(baseNode);
        }

        public void AddTemplateNode(string moduleId, ModuleTemplate template)
        {
            
            var tempList = new List<TemplateNode>();
            foreach ((TemplateNode parent, string key) in Nodes.SelectMany(item =>  template.Keywords.Where(y => IsKeywordValid(moduleId, item, y)).Select(y => (item, y))).Where(x => !string.IsNullOrEmpty(x.Item2)))
            {
                var node = new TemplateNode(moduleId, template, key);
                AddElement(node);
                tempList.Add(node);
                LinkTemplateNodes(parent, node, key);
            }
            Nodes.AddRange(tempList);
        }
        
        public void AddTemplateNode(string moduleId, TemplateAsset template, string[] keywords)
        {
            var tempList = new List<TemplateNode>();
            foreach ((TemplateNode parent, string key) in Nodes.SelectMany(item =>  keywords.Where(y => IsKeywordValid(moduleId, item, y)).Select(y => (item, y))).Where(x => !string.IsNullOrEmpty(x.Item2)))
            {
                var node = new TemplateNode(moduleId, template, key);
                AddElement(node);
                tempList.Add(node);
                LinkTemplateNodes(parent, node, key);
            }
            Nodes.AddRange(tempList);
        }

        public void ScheduleNodesPositionReset()
        {
            RegisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            var items = evt.menu.MenuItems();
            for (int i = 0; i < items.Count; i++)
                evt.menu.RemoveItemAt(0);

            if (evt.target is TemplateNode node)
            {
                evt.menu.InsertAction(0,"View template code", action =>
                {
                    if (_popup != null) _popup.Close();
                    _popup = ScriptableObject.CreateInstance<TextPopup>();
                     _popup.Text = node.TemplateAsset.Template;
                    var position = GUIUtility.GUIToScreenRect(node.worldBound);
                    int lineCount =   _popup.Text == null ? 5 :  _popup.Text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).Length;
                    _popup.ShowAsDropDown(position, new Vector2(600, Math.Min(lineCount * 16, 800)));
                });
                if (node.ModuleId.Equals("Shader") || node.ModuleId.Equals("ShaderPropertiesRoot"))
                {
                    evt.menu.InsertAction(1, "Select relative modular shader asset", action =>
                    {
                        Selection.SetActiveObjectWithContext(_shader, _shader);
                    });
                }
                else
                {
                    evt.menu.InsertAction(1, "Select relative module asset", action =>
                    {
                        var module = _modules.Find(x => x.Id.Equals(node.ModuleId));
                        if(module != null)
                            Selection.SetActiveObjectWithContext(module, module);
                    });
                }
                
            }
        }
        
        private void GeometryChangedCallback(GeometryChangedEvent evt)
        {
            UnregisterCallback<GeometryChangedEvent>(GeometryChangedCallback);

            if(BaseNodes.Count == 0) return;
            
            float top = 0;
            foreach (TemplateNode node in BaseNodes)
                top += node.Reposition(2, top, 100*(Nodes.Count/70 + 1));

            var newPosition = new Vector3(BaseNodes[0].style.left.value.value + viewport.resolvedStyle.width/2 - BaseNodes[0].resolvedStyle.width/2, 
                -BaseNodes[0].style.top.value.value + viewport.resolvedStyle.height/2 - BaseNodes[0].resolvedStyle.height/2, 
                viewTransform.position.z);
            viewTransform.position = newPosition;
        }
        
        private static bool IsKeywordValid(string moduleId, TemplateNode item, string y)
        {
            if (item.ContainsKeyword("#K#"+y)) return true;
            return item.ContainsKeyword("#KI#"+y) && moduleId.Equals(item.ModuleId);
        }
        
        private void LinkTemplateNodes(TemplateNode left, TemplateNode right, string key)
        {
            var edge = new Edge
            {
                output = left.Outputs[key],
                input = right.Input
            };
            
            edge.input.Connect(edge);
            edge.output.Connect(edge);
            edge.SetEnabled(false);
            Add(edge);
        }
    }

    internal sealed class TemplateNode : Node
    {
        public TemplateAsset TemplateAsset { get; set; }
        public string ModuleId { get; set; }
        public Port Input { get; set; }
        public Dictionary<string, Port> Outputs { get; set; }
        
        private ModuleTemplate _template;

        public TemplateNode(string moduleId, ModuleTemplate template, string key) : this (moduleId, template.Template, key)
        {
            _template = template;

            if (_template == null) return;
            var priority = new Label("" +_template.Queue);
            priority.AddToClassList("node-header-queue");
            titleContainer.Add(priority);
        }
        
        public TemplateNode(string moduleId, TemplateAsset templateAsset, string key)
        {
            ModuleId = moduleId;
            TemplateAsset = templateAsset;
            title = TemplateAsset.name;
            var idLabel = new Label($"({ModuleId})");
            idLabel.AddToClassList("node-header-id");
            titleContainer.Insert(1, idLabel);
            Outputs = new Dictionary<string, Port>();

            if (!string.IsNullOrEmpty(key))
            {
                Input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(string));
                Input.portName = key;
                Input.portColor = Color.cyan;
                Input.edgeConnector.activators.Clear();
                inputContainer.Add(Input);
            }

            foreach (string keyword in TemplateAsset.Keywords)
            {
                var port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(string));
                
                string sanitizedKeyword = keyword.Replace("#K#", "").Replace("#KI#", "");
                bool isInternal = keyword.StartsWith("#KI#");
                port.portName = sanitizedKeyword + (isInternal ? "(i)" : "");
                port.portColor = Color.cyan;
                port.edgeConnector.activators.Clear();
                Outputs.Add(sanitizedKeyword, port);
                outputContainer.Add(port);
            }

            RefreshExpandedState();
            RefreshPorts();
        }

        public float Reposition(float right, float top, float offset)
        {
            float width = resolvedStyle.width;
            float height = resolvedStyle.height;

            float childrenHeight = 0;
            float newTop = top;

            foreach (var output in Outputs.Values)
            {
                foreach (var edge in output.connections)
                {
                    var node = edge.input.node as TemplateNode;
                    if (node != null)
                    {
                        childrenHeight += node.Reposition(right + width + offset, newTop, offset);
                        newTop = top + childrenHeight;
                    }
                }
            }
            SetPosition(new Rect(right, top + Math.Max((childrenHeight - height)/2, 0), 100, 100));
            RefreshExpandedState();
            RefreshPorts();

            return Math.Max(height, childrenHeight) + 4;
        }

        public bool ContainsKeyword(string keyword)
        {
            return TemplateAsset.Keywords.Contains(keyword);
        }
    }
}