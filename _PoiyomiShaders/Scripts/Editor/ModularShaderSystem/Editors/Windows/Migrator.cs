using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Poiyomi.ModularShaderSystem
{
    [Serializable]
    public class MigratedAssets
    {
        public List<MigratedTemplate> templates;
        public List<MigratedCollection> templateCollections;
        public List<MigratedShaderModule> shaderModules;
        public List<MigratedModularShader> modularShaders;

        public MigratedAssets()
        {
            templates = new List<MigratedTemplate>();
            templateCollections = new List<MigratedCollection>();
            shaderModules = new List<MigratedShaderModule>();
            modularShaders = new List<MigratedModularShader>();
        }
    }

    [Serializable]
    public class MigratedTemplate
    {
        public long id;
        public string path;
        public string content;
    }
    
    [Serializable]
    public class MigratedCollection
    {
        public long id;
        public string path;
        public string content;
    }
    
    [Serializable]
    public class MigratedShaderModule
    {
        public long id;
        public string path;
        public string moduleId;
        public string name;
        public string version;
        public string author;
        public string description;
        public List<EnableProperty> enableProperties;
        public List<Property> properties;
        public List<string> moduleDependencies;
        public List<string> incompatibleWith;
        public List<MigratedModuleTemplate> templates;
        public List<MigratedShaderFunction> functions;
        public string additionalSerializedData;
    }
    
    [Serializable]
    public class MigratedModularShader
    {
        public long id;
        public string path;
        public string shaderId;
        public string name;
        public string version;
        public string author;
        public string description;
        public bool useTemplatesForProperties;
        public long propertiesTemplateReference;
        public string propertiesCollectionSubId;
        public string shaderPath;
        public long shaderTemplateReference;
        public string shaderCollectionSubId;
        public string customEditor;
        public List<Property> properties;
        public List<long> baseModules;
        public List<long> additionalModules;
        public bool lockBaseModules;
        public List<Shader> lastGeneratedShaders;
        public string additionalSerializedData;
    }
    
    [Serializable]
    public class MigratedModuleTemplate 
    {
        public long templateReference;
        public string collectionSubId;
        public List<string> keywords;
        public bool needsVariant;
        public int queue;
    }
    
    [Serializable]
    public class MigratedShaderFunction 
    {
        public string name;
        public string appendAfter;
        public short queue;
        public long templateReference;
        public string collectionSubId;
        public List<Variable> usedVariables;
        public List<string> variableKeywords;
        public List<string> codeKeywords;
    }

    public class MigratedItemElement<T> : VisualElement
    {
        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                _toggle.SetValueWithoutNotify(_isSelected);
            }
        }

        
        private Migrator _window;
        public T ToggledItem;
        private string _name;
        private readonly Toggle _toggle;

        public MigratedItemElement(Migrator window, T toggledItem, string name)
        {
            _window = window;
            _name = name;
            ToggledItem = toggledItem;

            style.flexDirection = FlexDirection.Row;
            
            _toggle = new Toggle();
            _toggle.RegisterValueChangedCallback(evt =>
            {
                IsSelected = evt.newValue;
                _window.CheckRelationshipSelection(toggledItem, IsSelected);
            });
            Add(_toggle);
            Add(new Label(_name));
        }
    }
    
    public class Migrator : EditorWindow
    {
        [MenuItem(MSSConstants.WINDOW_PATH + "/Tools/Migrator", priority = 101)]
        private static void ShowWindow()
        {
            var window = GetWindow<Migrator>();
            window.titleContent = new GUIContent("Migrator");
            window.Show();
        }

        private List<MigratedItemElement<ModularShader>> _shaderElements;
        private List<MigratedItemElement<ShaderModule>> _moduleElements;
        private List<MigratedItemElement<TemplateAsset>> _templateElements;
        private List<MigratedItemElement<TemplateCollectionAsset>> _collectionElements;

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            
            var styleSheet = Resources.Load<StyleSheet>(MSSConstants.RESOURCES_FOLDER + (EditorGUIUtility.isProSkin ? "/MSSUIElements/MigratorDark" : "/MSSUIElements/MigratorLight"));
            root.styleSheets.Add(styleSheet);
            
            var buttonRow = new VisualElement();
            buttonRow.AddToClassList("button-tab-area");

            var selectedTab = new VisualElement();
            selectedTab.style.flexGrow = 1;
            
            VisualElement exportRoot = new VisualElement();
            exportRoot.style.flexGrow = 1;
            SetupExport(exportRoot);
            
            VisualElement importRoot = new VisualElement();
            importRoot.style.flexGrow = 1;
            SetupImport(importRoot);
            
            var tabButton = new Button();
            tabButton.text = "Export";
            tabButton.AddToClassList("button-tab");
            tabButton.clicked += () =>
            {
                foreach (var button in buttonRow.Children())
                    if(button.ClassListContains("button-tab-selected"))
                        button.RemoveFromClassList("button-tab-selected");
                    
                tabButton.AddToClassList("button-tab-selected");
                   
                selectedTab.Clear();
                selectedTab.Add(exportRoot);
            };
            buttonRow.Add(tabButton);
            
            var secondTabButton = new Button();
            secondTabButton.text = "Import";
            secondTabButton.AddToClassList("button-tab");
            secondTabButton.clicked += () =>
            {
                foreach (var button in buttonRow.Children())
                    if(button.ClassListContains("button-tab-selected"))
                        button.RemoveFromClassList("button-tab-selected");
                    
                secondTabButton.AddToClassList("button-tab-selected");
                   
                selectedTab.Clear();
                selectedTab.Add(importRoot);
            };
                
            buttonRow.Add(secondTabButton);
            
            selectedTab.Add(exportRoot);
            tabButton.AddToClassList("button-tab-selected");
            root.Add(buttonRow);
            root.Add(selectedTab);
        }
        
        private void SetupImport(VisualElement importRoot)
        {
            MigratedAssets assets = null;
            
            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1;
            
            var importButton = new Button();
            importButton.text = "Import";
            importButton.style.minHeight = 22;
            importButton.style.height = 22;
            importButton.SetEnabled(false);
            importButton.clicked += () => Import(assets);
            
            var loadButton = new Button();
            loadButton.style.minHeight = 22;
            loadButton.style.height = 22;
            loadButton.text = "Load file";
            loadButton.clicked += () =>
            {
                string assetPath = EditorUtility.OpenFilePanel("Export", "Assets", "json");
                if (string.IsNullOrWhiteSpace(assetPath)) return;

                assets = JsonUtility.FromJson<MigratedAssets>(File.ReadAllText(assetPath));
                
                importButton.SetEnabled(true);

                scrollView.Clear();
                var label = new Label("Modular Shaders");
                label.AddToClassList("title");
                scrollView.Add(label);
                
                foreach (var shader in assets.modularShaders)
                    scrollView.Add(new Label($"{shader.name} ({shader.shaderId})"));
                
                label = new Label("Shader Modules");
                label.AddToClassList("title");
                scrollView.Add(label);
                
                foreach (var module in assets.shaderModules)
                    scrollView.Add(new Label($"{module.name} ({module.moduleId})"));
                
                label = new Label("Template Assets");
                label.AddToClassList("title");
                scrollView.Add(label);
                
                foreach (var template in assets.templates)
                    scrollView.Add(new Label($"{Path.GetFileNameWithoutExtension(template.path)}"));
                
                label = new Label("Template Collection Assets");
                label.AddToClassList("title");
                scrollView.Add(label);
                
                foreach (var collection in assets.templateCollections)
                    scrollView.Add(new Label($"{Path.GetFileNameWithoutExtension(collection.path)}"));
            };
            
            importRoot.Add(loadButton);
            importRoot.Add(scrollView);
            importRoot.Add(importButton);
        }

        private static void Import(MigratedAssets assets)
        {
            foreach (var asset in assets.templates)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(asset.path));
                File.WriteAllText(Path.ChangeExtension(asset.path, MSSConstants.TEMPLATE_EXTENSION) ?? string.Empty, asset.content);
                if(File.Exists(asset.path))
                    File.Delete(asset.path);
            }

            foreach (var asset in assets.templateCollections)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(asset.path));
                File.WriteAllText(Path.ChangeExtension(asset.path, MSSConstants.TEMPLATE_COLLECTION_EXTENSION) ?? string.Empty, asset.content);
                if(File.Exists(asset.path))
                    File.Delete(asset.path);
            }

            AssetDatabase.Refresh();

            foreach (var asset in assets.shaderModules)
            {
                var module = CreateInstance<ShaderModule>();
                module.Id = asset.moduleId;
                module.Name = asset.name;
                module.Description = asset.description;
                module.Version = asset.version;
                module.Author = asset.author;
                module.ModuleDependencies = new List<string>(asset.moduleDependencies);
                module.IncompatibleWith = new List<string>(asset.incompatibleWith);
                module.AdditionalSerializedData = asset.additionalSerializedData;
                module.EnableProperties = new List<EnableProperty>(asset.enableProperties);
                module.Properties = new List<Property>(asset.properties);
                module.Templates = asset.templates.Select(x =>
                {
                    var template = new ModuleTemplate
                    {
                        Keywords = new List<string>(x.keywords),
                        Queue = x.queue,
                        NeedsVariant = x.needsVariant
                    };

                    if (string.IsNullOrWhiteSpace(x.collectionSubId))
                    {
                        var t = assets.templates.Find(y => y.id == x.templateReference);
                        if (t == null) return template;
                        var st = AssetDatabase.LoadAssetAtPath<TemplateAsset>(Path.ChangeExtension(t.path, MSSConstants.TEMPLATE_EXTENSION));
                        template.Template = st;
                    }
                    else
                    {
                        var t = assets.templateCollections.Find(y => y.id == x.templateReference);
                        if (t == null) return template;
                        var st = AssetDatabase.LoadAssetAtPath<TemplateCollectionAsset>(Path.ChangeExtension(t.path, MSSConstants.TEMPLATE_COLLECTION_EXTENSION));
                        template.Template = st.Templates.Find(y => x.collectionSubId.Equals(y.name));
                    }

                    return template;
                }).ToList();
                module.Functions = asset.functions.Select(x =>
                {
                    var function = new ShaderFunction
                    {
                        Name = x.name,
                        Queue = x.queue,
                        AppendAfter = x.appendAfter,
                        CodeKeywords = new List<string>(x.codeKeywords),
                        UsedVariables = new List<Variable>(x.usedVariables),
                        VariableKeywords = new List<string>(x.variableKeywords)
                    };

                    if (string.IsNullOrWhiteSpace(x.collectionSubId))
                    {
                        var t = assets.templates.Find(y => y.id == x.templateReference);
                        if (t == null) return function;
                        var st = AssetDatabase.LoadAssetAtPath<TemplateAsset>(Path.ChangeExtension(t.path, MSSConstants.TEMPLATE_EXTENSION));
                        function.ShaderFunctionCode = st;
                    }
                    else
                    {
                        var t = assets.templateCollections.Find(y => y.id == x.templateReference);
                        if (t == null) return function;
                        var st = AssetDatabase.LoadAssetAtPath<TemplateCollectionAsset>(Path.ChangeExtension(t.path, MSSConstants.TEMPLATE_COLLECTION_EXTENSION));
                        function.ShaderFunctionCode = st.Templates.Find(y => x.collectionSubId.Equals(y.name));
                    }

                    return function;
                }).ToList();
                Directory.CreateDirectory(Path.GetDirectoryName(asset.path));
                AssetDatabase.CreateAsset(module, asset.path);
            }

            AssetDatabase.Refresh();

            foreach (var asset in assets.modularShaders)
            {
                var shader = CreateInstance<ModularShader>();
                shader.Id = asset.shaderId;
                shader.Name = asset.name;
                shader.Description = asset.description;
                shader.Version = asset.version;
                shader.Author = asset.author;
                shader.ShaderPath = asset.shaderPath;
                shader.CustomEditor = asset.customEditor;
                shader.LockBaseModules = asset.lockBaseModules;
                shader.UseTemplatesForProperties = asset.useTemplatesForProperties;
                shader.Properties = new List<Property>(asset.properties);
                shader.BaseModules = asset.baseModules.Select(x => { return AssetDatabase.LoadAssetAtPath<ShaderModule>(assets.shaderModules.Find(y => y.id == x).path); }).ToList();

                shader.AdditionalModules = asset.additionalModules.Select(x => { return AssetDatabase.LoadAssetAtPath<ShaderModule>(assets.shaderModules.Find(y => y.id == x).path); }).ToList();

                if (string.IsNullOrWhiteSpace(asset.shaderCollectionSubId))
                {
                    var t = assets.templates.Find(y => y.id == asset.shaderTemplateReference);
                    if (t != null)
                    {
                        var st = AssetDatabase.LoadAssetAtPath<TemplateAsset>(Path.ChangeExtension(t.path, MSSConstants.TEMPLATE_EXTENSION));
                        shader.ShaderTemplate = st;
                    }
                }
                else
                {
                    var t = assets.templateCollections.Find(y => y.id == asset.shaderTemplateReference);
                    if (t != null)
                    {
                        var st = AssetDatabase.LoadAssetAtPath<TemplateCollectionAsset>(Path.ChangeExtension(t.path, MSSConstants.TEMPLATE_COLLECTION_EXTENSION));
                        shader.ShaderTemplate = st.Templates.Find(y => asset.shaderCollectionSubId.Equals(y.name));
                    }
                }

                if (string.IsNullOrWhiteSpace(asset.propertiesCollectionSubId))
                {
                    var t = assets.templates.Find(y => y.id == asset.propertiesTemplateReference);
                    if (t != null)
                    {
                        var st = AssetDatabase.LoadAssetAtPath<TemplateAsset>(Path.ChangeExtension(t.path, MSSConstants.TEMPLATE_EXTENSION));
                        shader.ShaderPropertiesTemplate = st;
                    }
                }
                else
                {
                    var t = assets.templateCollections.Find(y => y.id == asset.propertiesTemplateReference);
                    if (t != null)
                    {
                        var st = AssetDatabase.LoadAssetAtPath<TemplateCollectionAsset>(Path.ChangeExtension(t.path, MSSConstants.TEMPLATE_COLLECTION_EXTENSION));
                        shader.ShaderPropertiesTemplate = st.Templates.Find(y => asset.propertiesCollectionSubId.Equals(y.name));
                    }
                }

                Directory.CreateDirectory(Path.GetDirectoryName(asset.path));
                AssetDatabase.CreateAsset(shader, asset.path);
            }

            AssetDatabase.Refresh();
        }

        private void SetupExport(VisualElement exportRoot)
        {
            _shaderElements = new List<MigratedItemElement<ModularShader>>();
            _moduleElements = new List<MigratedItemElement<ShaderModule>>();
            _templateElements = new List<MigratedItemElement<TemplateAsset>>();
            _collectionElements = new List<MigratedItemElement<TemplateCollectionAsset>>();

            var templateAssets = FindAssetsByType<TemplateAsset>().Where(x => !AssetDatabase.IsSubAsset(x)).ToArray();
            var collectionAssets = FindAssetsByType<TemplateCollectionAsset>();
            var shaderModules = FindAssetsByType<ShaderModule>();
            var modularShaders = FindAssetsByType<ModularShader>();

            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            scrollView.style.flexGrow = 1;

            Foldout shadersFoldout = new Foldout();
            shadersFoldout.text = "Modular Shaders";
            foreach (var modularShader in modularShaders)
            {
                var element = new MigratedItemElement<ModularShader>(this, modularShader, $"{modularShader.Name} ({modularShader.name})");
                shadersFoldout.Add(element);
                _shaderElements.Add(element);
            }

            Foldout modulesFoldout = new Foldout();
            modulesFoldout.text = "Shader Modules";
            foreach (var shaderModule in shaderModules)
            {
                var element = new MigratedItemElement<ShaderModule>(this, shaderModule, $"{shaderModule.Name} ({shaderModule.name})");
                modulesFoldout.Add(element);
                _moduleElements.Add(element);
            }

            Foldout templatesFoldout = new Foldout();
            templatesFoldout.text = "Template Assets";
            foreach (var template in templateAssets)
            {
                var element = new MigratedItemElement<TemplateAsset>(this, template, template.name);
                templatesFoldout.Add(element);
                _templateElements.Add(element);
            }

            Foldout collectionsFoldout = new Foldout();
            collectionsFoldout.text = "Template Collection Assets";
            foreach (var collection in collectionAssets)
            {
                var element = new MigratedItemElement<TemplateCollectionAsset>(this, collection, collection.name);
                collectionsFoldout.Add(element);
                _collectionElements.Add(element);
            }

            var bottomRow = new VisualElement();
            bottomRow.style.minHeight = 26;
            bottomRow.style.height = 26;
            bottomRow.style.flexDirection = FlexDirection.Row;

            var b = new Button();
            b.text = "Select all";
            b.clicked += () =>
            {
                _shaderElements.ForEach(x => x.IsSelected = true);
                _moduleElements.ForEach(x => x.IsSelected = true);
                _templateElements.ForEach(x => x.IsSelected = true);
                _collectionElements.ForEach(x => x.IsSelected = true);
            };
            bottomRow.Add(b);
            
            b = new Button();
            b.text = "Unselect all";
            b.clicked += () =>
            {
                _shaderElements.ForEach(x => x.IsSelected = false);
                _moduleElements.ForEach(x => x.IsSelected = false);
                _templateElements.ForEach(x => x.IsSelected = false);
                _collectionElements.ForEach(x => x.IsSelected = false);
            };
            bottomRow.Add(b);
            var v = new VisualElement();
            v.style.flexGrow = 1;
            bottomRow.Add(v);
            b = new Button();
            b.text = "Save";
            b.clicked += ExportSelected;
            bottomRow.Add(b);
            
            
            
            scrollView.Add(shadersFoldout);
            scrollView.Add(modulesFoldout);
            scrollView.Add(templatesFoldout);
            scrollView.Add(collectionsFoldout);
            exportRoot.Add(scrollView);
            exportRoot.Add(bottomRow);
        }

        private void ExportSelected()
        {
            var assets = new MigratedAssets();

            string finalPath = EditorUtility.SaveFilePanel("Export", "Assets", "migrationData", "json");
            if (string.IsNullOrWhiteSpace(finalPath)) return;

            var idByTemplate = new Dictionary<TemplateAsset, long>();
            var idByCollection = new Dictionary<TemplateAsset, long>();
            var idByModule = new Dictionary<ShaderModule, long>();

            long currentId = 1;
            foreach (var asset in _templateElements.Where(x => x.IsSelected).Select(x => x.ToggledItem))
            {
                idByTemplate.Add(asset, currentId);
                string path = AssetDatabase.GetAssetPath(asset);
                assets.templates.Add(new MigratedTemplate
                {
                    id = currentId,
                    content = File.ReadAllText(path),
                    path = path
                });
                currentId++;
            }
            foreach (var asset in _collectionElements.Where(x => x.IsSelected).Select(x => x.ToggledItem))
            {
                foreach (var tmp in asset.Templates)
                {
                    idByCollection.Add(tmp, currentId);
                }
                string path = AssetDatabase.GetAssetPath(asset);
                assets.templateCollections.Add(new MigratedCollection
                {
                    id = currentId,
                    content = File.ReadAllText(path),
                    path = path
                });
                currentId++;
            }
            foreach (var asset in _moduleElements.Where(x => x.IsSelected).Select(x => x.ToggledItem))
            {
                idByModule.Add(asset, currentId);
                string path = AssetDatabase.GetAssetPath(asset);
                assets.shaderModules.Add(new MigratedShaderModule
                {
                    id = currentId,
                    path = path,
                    moduleId = asset.Id,
                    name = asset.Name,
                    version = asset.Version,
                    author = asset.Author,
                    description = asset.Description,
                    enableProperties = asset.EnableProperties,
                    properties = new List<Property>(asset.Properties),
                    moduleDependencies = new List<string>(asset.ModuleDependencies),
                    incompatibleWith = new List<string>(asset.IncompatibleWith),
                    additionalSerializedData = asset.AdditionalSerializedData,
                    templates = new List<MigratedModuleTemplate>(asset.Templates.Select(x => FromModuleTemplate(x, idByTemplate, idByCollection))),
                    functions = new List<MigratedShaderFunction>(asset.Functions.Select(x => FromModuleFunction(x, idByTemplate, idByCollection)))
                });
                currentId++;
            }
            foreach (var asset in _shaderElements.Where(x => x.IsSelected).Select(x => x.ToggledItem))
            {
                string path = AssetDatabase.GetAssetPath(asset);
                var shader = new MigratedModularShader
                {
                    id = currentId,
                    path = path,
                    shaderId = asset.Id,
                    name = asset.Name,
                    version = asset.Version,
                    author = asset.Author,
                    description = asset.Description,
                    properties = new List<Property>(asset.Properties),
                    additionalSerializedData = asset.AdditionalSerializedData,
                    customEditor = asset.CustomEditor,
                    shaderPath = asset.ShaderPath,
                    lockBaseModules = asset.LockBaseModules,
                    useTemplatesForProperties = asset.UseTemplatesForProperties,
                    baseModules = asset.BaseModules.Select(x => idByModule[x]).ToList(),
                    additionalModules = asset.AdditionalModules.Select(x => idByModule[x]).ToList()
                    
                };

                if (asset.ShaderTemplate == null) shader.shaderTemplateReference = 0;
                else if (idByTemplate.ContainsKey(asset.ShaderTemplate))
                {
                    shader.shaderTemplateReference = idByTemplate[asset.ShaderTemplate];
                }
                else if (idByCollection.ContainsKey(asset.ShaderTemplate))
                {
                    shader.shaderTemplateReference = idByTemplate[asset.ShaderTemplate];
                    shader.shaderCollectionSubId = asset.ShaderTemplate.name;
                }
                
                if (asset.ShaderPropertiesTemplate == null) shader.propertiesTemplateReference = 0;
                else if (idByTemplate.ContainsKey(asset.ShaderPropertiesTemplate))
                {
                    shader.propertiesTemplateReference = idByTemplate[asset.ShaderPropertiesTemplate];
                }
                else if (idByCollection.ContainsKey(asset.ShaderPropertiesTemplate))
                {
                    shader.propertiesTemplateReference = idByTemplate[asset.ShaderPropertiesTemplate];
                    shader.propertiesCollectionSubId = asset.ShaderPropertiesTemplate.name;
                }
                
                assets.modularShaders.Add(shader);
                
                currentId++;
            }
            
            File.WriteAllText(finalPath, JsonUtility.ToJson(assets));
        }
        
        private static MigratedModuleTemplate FromModuleTemplate(ModuleTemplate original, Dictionary<TemplateAsset, long> idByTemplate,  Dictionary<TemplateAsset, long> idByCollection)
        {
            
            var template = new MigratedModuleTemplate
            {
                keywords = new List<string>(original.Keywords),
                queue = original.Queue,
                needsVariant = original.NeedsVariant
            };

            if (original.Template == null) return template;
            
            if (idByTemplate.ContainsKey(original.Template))
            {
                template.templateReference = idByTemplate[original.Template];
            }
            else if (idByCollection.ContainsKey(original.Template))
            {
                template.templateReference = idByCollection[original.Template];
                template.collectionSubId = original.Template.name;
            }

            return template;
        }
        
        private static MigratedShaderFunction FromModuleFunction(ShaderFunction original, Dictionary<TemplateAsset, long> idByTemplate,  Dictionary<TemplateAsset, long> idByCollection)
        {
            
            var template = new MigratedShaderFunction
            {
                name = original.Name,
                queue = original.Queue,
                appendAfter = original.AppendAfter,
                usedVariables = original.UsedVariables,
                codeKeywords = original.CodeKeywords,
                variableKeywords = original.VariableKeywords,
            };
            if (original.ShaderFunctionCode == null) return template;

            if (idByTemplate.ContainsKey(original.ShaderFunctionCode))
            {
                template.templateReference = idByTemplate[original.ShaderFunctionCode];
            }
            else if (idByCollection.ContainsKey(original.ShaderFunctionCode))
            {
                template.templateReference = idByCollection[original.ShaderFunctionCode];
                template.collectionSubId = original.ShaderFunctionCode.name;
            }

            return template;
        }

        internal void CheckRelationshipSelection<T>(T item, bool isToggled)
        {
            switch (item)
            {
                case ModularShader s:
                    ToggleShaderDependencies(s, isToggled);
                    break;
                case ShaderModule m:
                    ToggleModuleDependencies(m, isToggled);
                    break;
                case TemplateAsset t:
                    ToggleTemplateDependencies(t, isToggled);
                    break;
                case TemplateCollectionAsset c:
                    ToggleCollectionDependencies(c, isToggled);
                    break;
            }
        }
        
        private void ToggleShaderDependencies(ModularShader shader, bool isToggled)
        {
            if (!isToggled) return;
            
            foreach (var element in _moduleElements.Where(x => shader.BaseModules.Concat(shader.AdditionalModules).Contains(x.ToggledItem)))
            {
                element.IsSelected = true;
                ToggleModuleDependencies(element.ToggledItem, true);
            }
            
            foreach (var element in _templateElements.Where(x => shader.ShaderTemplate == x.ToggledItem || shader.ShaderPropertiesTemplate == x.ToggledItem))
            {
                element.IsSelected = true;
            }
            
            foreach (var element in _collectionElements.Where(x => x.ToggledItem.Templates.Contains(shader.ShaderTemplate) || x.ToggledItem.Templates.Contains(shader.ShaderPropertiesTemplate)))
            {
                element.IsSelected = true;
            }
        }

        private void ToggleModuleDependencies(ShaderModule module, bool isToggled)
        {
            if (isToggled)
            {
                foreach (var element in _templateElements.Where(x => 
                             module.Templates.Select(y => y.Template).Concat(module.Functions.Select(z => z.ShaderFunctionCode)).Contains(x.ToggledItem)))
                {
                    element.IsSelected = true;
                }
                
                foreach (var element in _collectionElements.Where(x => 
                             module.Templates.Select(y => y.Template).Concat(module.Functions.Select(z => z.ShaderFunctionCode)).Any(y => x.ToggledItem.Templates.Contains(y))))
                {
                    element.IsSelected = true;
                }
            }
            else
            {
                foreach (var element in _shaderElements.Where(x => x.ToggledItem.BaseModules.Concat(x.ToggledItem.AdditionalModules).Contains(module)))
                {
                    element.IsSelected = false;
                }
            }
        }
        
        private void ToggleTemplateDependencies(TemplateAsset template, bool isToggled)
        {
            if (isToggled) return;
            
            foreach (var element in _moduleElements.Where(x => 
                         x.ToggledItem.Templates.Select(y => y.Template).Concat(x.ToggledItem.Functions.Select(z => z.ShaderFunctionCode)).Contains(template)))
            {
                element.IsSelected = false;
                ToggleModuleDependencies(element.ToggledItem, false);
            }
            
            foreach (var element in _shaderElements.Where(x => x.ToggledItem.ShaderTemplate == template || x.ToggledItem.ShaderPropertiesTemplate == template))
            {
                element.IsSelected = false;
            }
        }
        
        private void ToggleCollectionDependencies(TemplateCollectionAsset template, bool isToggled)
        {
            if (isToggled) return;
            
            foreach (var element in _moduleElements.Where(x => 
                         x.ToggledItem.Templates.Select(y => y.Template).Concat(x.ToggledItem.Functions.Select(z => z.ShaderFunctionCode)).Any(y => template.Templates.Contains(y))))
            {
                element.IsSelected = false;
                ToggleModuleDependencies(element.ToggledItem, false);
            }
            
            foreach (var element in _shaderElements.Where(x => template.Templates.Contains(x.ToggledItem.ShaderTemplate) ||template.Templates.Contains(x.ToggledItem.ShaderPropertiesTemplate)))
            {
                element.IsSelected = false;
            }
        }
        
        private static T[] FindAssetsByType<T>() where T : Object
        {
            List<T> assets = new List<T>();
            AssetDatabase.Refresh();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).ToString().Replace("UnityEngine.", "")}");
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                    assets.Add(asset);
            }
            return assets.ToArray();
        }
    }
}