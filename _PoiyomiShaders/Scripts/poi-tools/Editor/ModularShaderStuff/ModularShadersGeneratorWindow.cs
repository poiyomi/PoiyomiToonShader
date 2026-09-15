using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.UIElements;
using Poiyomi.ModularShaderSystem;

#if UNITY_2019_4
using UnityEditor.Experimental.SceneManagement;
#endif

#if UNITY_2021_3
using UnityEditor.UIElements;
#endif

namespace Poi.Tools
{
    public class ModularShadersGeneratorElement : VisualElement
    {
        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isErrored) return;
                _isSelected = value;
                _toggle.SetValueWithoutNotify(_isSelected);
            }
        }

        public ModularShader Shader { get; set; }

        private readonly Toggle _toggle;
        private readonly bool _isErrored;
        public ModularShadersGeneratorElement(ModularShader shader)
        {
            Shader = shader;
            style.flexDirection = FlexDirection.Row;
            _toggle = new Toggle();
            _toggle.RegisterValueChangedCallback(evt => IsSelected = evt.newValue);
            Add(_toggle);
            var label = new Label(Shader.Name);
            label.style.flexGrow = 1;
            Add(label);
            var shaderObject = new UnityEditor.UIElements.ObjectField();
            shaderObject.objectType = typeof(ModularShader);
            shaderObject.value = shader;
            shaderObject.style.minWidth = new StyleLength(new Length(50f, LengthUnit.Percent));
            shaderObject.style.maxWidth = new StyleLength(new Length(50f, LengthUnit.Percent));
            Add(shaderObject);
            var issues = ShaderGenerator.CheckShaderIssues(shader);
            if (issues.Count > 0)
            {
                _isErrored = true;
                _toggle.SetEnabled(false);
                VisualElement element = new VisualElement();
                element.AddToClassList("error");
                element.tooltip = "Modular shader has the following errors: \n -" + string.Join("\n -", issues);
                Add(element);
            }
        }
    }

    [Serializable]
    public class ModularShadersGeneratorWindow : EditorWindow
    {
        private const string Pref_LimitMemory = "Poi_ModShaderGen_LimitMemory";
        private const string Pref_MaxWorkers = "Poi_ModShaderGen_MaxWorkers";
        private const string Pref_BatchSize = "Poi_ModShaderGen_BatchSize";

        private bool _limitMemory;
        private int _maxWorkers;
        private int _batchSize;

        // --- Async shader generation state (driven by EditorApplication.update) ---
        private const string GenProgressTitle = "Generating Modular Shaders";
        private bool _genIsRunning;
        private List<(ShaderGenerator.ShaderContext context, Dictionary<string, ShaderModule[]> duplicates)> _genContexts;
        private List<ShaderGenerator.ShaderContext> _genContextsList;
        private int _genCurrentIndex;
        private int _genBatchSize;
        private int _genTotalBatches;
        private int _genTaskCount;
        private ParallelOptions _genParallelOptions;
        private Task _genCurrentBatchTask;
        private int _genCurrentBatchTotal;
        private int _genCurrentBatchCompleted; // touched by worker threads via Interlocked
        private CancellationTokenSource _genCts;
        private bool _editorLocked;

        [MenuItem("Poi/Modular Shaders Generator")]
        private static void ShowWindow()
        {
            var window = GetWindow<ModularShadersGeneratorWindow>();
            window.titleContent = new GUIContent("Modular Shaders Generator");
            window.Show();
        }

        /// <summary>
        /// Regenerates the given modular shaders through the normal pipeline. Used by the Installed
        /// Modules window so it doesn't have to reimplement batching, progress and cancellation.
        /// Shaders the generator has flagged as errored are skipped.
        /// </summary>
        public static void RebuildShaders(IEnumerable<ModularShader> shaders)
        {
            var wanted = new HashSet<ModularShader>(shaders);
            if (wanted.Count == 0)
            {
                Debug.Log("No shaders to rebuild.");
                return;
            }

            var window = GetWindow<ModularShadersGeneratorWindow>();
            window.titleContent = new GUIContent("Modular Shaders Generator");
            window.Show();

            // CreateGUI populates _elements when the window is first shown, so defer a tick if
            // this is the call that opened it.
            void Run()
            {
                if (window._elements == null)
                {
                    Debug.LogError("Shader generator failed to populate its shader list; rebuild aborted.");
                    return;
                }

                foreach (var element in window._elements)
                    element.IsSelected = wanted.Contains(element.Shader);

                int selected = window._elements.Count(x => x.IsSelected);
                if (selected == 0)
                {
                    Debug.LogError("None of the requested shaders could be selected for rebuild (they may all have generator errors).");
                    return;
                }
                if (selected < wanted.Count)
                    Debug.LogWarning($"Rebuilding {selected} of {wanted.Count} requested shaders; the rest have generator errors.");

                window.GenerateShaders();
            }

            if (window._elements == null)
                EditorApplication.delayCall += Run;
            else
                Run();
        }

        private VisualElement _root;
        internal List<ModularShadersGeneratorElement> _elements;
        private bool _urpMode;

        private static bool IsUsingURP()
        {
            var currentRP = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            if (currentRP == null) return false;
            // Check if the render pipeline asset type name contains "Universal"
            return currentRP.GetType().Name.IndexOf("Universal", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void FilterElementsByUrp()
        {
            if (_elements == null) return;
            foreach (var element in _elements)
            {
                bool isUrpShader = element.Shader.Name.IndexOf("URP", StringComparison.OrdinalIgnoreCase) >= 0;
                bool shouldShow = _urpMode ? isUrpShader : !isUrpShader;
                element.style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
                // Deselect hidden elements
                if (!shouldShow) element.IsSelected = false;
            }
        }

        private void LoadMemPrefs()
        {
            _limitMemory = EditorPrefs.GetBool(Pref_LimitMemory, true);
            _maxWorkers = EditorPrefs.GetInt(Pref_MaxWorkers, Math.Max(1, Environment.ProcessorCount / 2));
            _batchSize = EditorPrefs.GetInt(Pref_BatchSize, 32);
        }

        private void SaveMemPrefs()
        {
            EditorPrefs.SetBool(Pref_LimitMemory, _limitMemory);
            EditorPrefs.SetInt(Pref_MaxWorkers, _maxWorkers);
            EditorPrefs.SetInt(Pref_BatchSize, _batchSize);
        }

        private void OnDestroy()
        {
            UnregisterCallbacks();
            if (_genIsRunning)
            {
                EditorApplication.update -= ProcessNextGenerationBatch;
                try { _genCts?.Cancel(); } catch { }
                EndGeneration();
            }
        }

        private void CreateGUI()
        {
            _root = rootVisualElement;
            Reload();
        }

        private void Reload()
        {
            _root.Clear();

            var styleSheet = Resources.Load<StyleSheet>("Poi/ModularShadersGeneratorStyle");
            _root.styleSheets.Add(styleSheet);

            var view = new ScrollView(ScrollViewMode.Vertical);
#if UNITY_2021_2_OR_NEWER
            view.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
#else
            view.horizontalScroller.visible = false;
#endif

            var selectButtons = new VisualElement();
            selectButtons.AddToClassList("buttons-area");

            var selectAll = new Button();
            selectAll.text = "Select all";
            selectAll.style.flexGrow = 1;
            selectAll.clicked += () =>
            {
                foreach (var element in _elements)
                    if (element.style.display != DisplayStyle.None)
                        element.IsSelected = true;
            };
            selectButtons.Add(selectAll);

            var deselectAll = new Button();
            deselectAll.text = "Deselect all";
            deselectAll.style.flexGrow = 1;
            deselectAll.clicked += () =>
            {
                foreach (var element in _elements)
                    element.IsSelected = false;
            };
            selectButtons.Add(deselectAll);

            var toggleSelections = new Button();
            toggleSelections.text = "Toggle selections";
            toggleSelections.style.flexGrow = 1;
            toggleSelections.clicked += () =>
            {
                foreach (var element in _elements)
                    if (element.style.display != DisplayStyle.None)
                        element.IsSelected = !element.IsSelected;
            };
            selectButtons.Add(toggleSelections);

            var reloadButton = new Button();
            reloadButton.text = "Refresh";
            reloadButton.style.flexGrow = 1;
            reloadButton.clicked += () =>
            {
                AssetDatabase.Refresh();
                Reload();
            };
            selectButtons.Add(reloadButton);

            view.Add(selectButtons);

            var typeButtons = new VisualElement();
            typeButtons.AddToClassList("buttons-area");

            var selectToon = new Button { text = "Toon" };
            selectToon.style.flexGrow = 1;
            selectToon.clicked += () => SelectByName("Toon", excludeUrp: !_urpMode, requireUrp: _urpMode);
            typeButtons.Add(selectToon);

            var selectPro = new Button { text = "Pro" };
            selectPro.style.flexGrow = 1;
            selectPro.clicked += () => SelectByName("Pro", excludeUrp: !_urpMode, requireUrp: _urpMode);
            typeButtons.Add(selectPro);

            // Auto-detect URP on first load
            _urpMode = IsUsingURP();

            var urpToggle = new Toggle("URP");
            urpToggle.value = _urpMode;
            urpToggle.style.flexGrow = 0;
            urpToggle.style.marginLeft = 8;
            urpToggle.RegisterValueChangedCallback(evt =>
            {
                _urpMode = evt.newValue;
                FilterElementsByUrp();
            });
            typeButtons.Add(urpToggle);

            view.Add(typeButtons);

            // Load all modular shaders
            _elements = new List<ModularShadersGeneratorElement>();
            foreach (var modularShader in FindAssetsByType<ModularShader>())
            {
                var element = new ModularShadersGeneratorElement(modularShader);
                _elements.Add(element);
                view.Add(element);
            }

            // Apply initial URP filter
            FilterElementsByUrp();

            var generateButton = new Button();
            generateButton.style.marginLeft = 6;
            generateButton.style.marginRight = 8;
            generateButton.text = "Generate Shaders";
            generateButton.clicked += GenerateShaders;

            VisualElement destinationsList = SetupDestinationsListView();

            view.Add(destinationsList);
            _root.Add(view);
            view.Add(generateButton);


            // Load Memory Prefs
            LoadMemPrefs();

            var memoryButtons = new VisualElement();
            memoryButtons.AddToClassList("buttons-area");

            var limitMemory = new Toggle("Limit Memory Usage");
            limitMemory.value = _limitMemory;
            limitMemory.style.flexGrow = 1;
            limitMemory.tooltip = "If enabled, will limit the amount of Workers and Batches to a specific amount during shader generation. It is recommended to keep this enabled in order to prevent crashing your PC due to insufficient RAM.";
            limitMemory.RegisterValueChangedCallback(evt =>
            {
                _limitMemory = evt.newValue;
                SaveMemPrefs();
            });
            memoryButtons.Add(limitMemory);

            var workersField = new IntegerField("Max Workers");
            workersField.value = _maxWorkers;
            workersField.style.flexGrow = 1;
            workersField.RegisterValueChangedCallback(evt =>
            {
                _maxWorkers = Mathf.Clamp(evt.newValue, 1, Environment.ProcessorCount);
                SaveMemPrefs();
            });
            memoryButtons.Add(workersField);

            var batchField = new IntegerField("Batching Size");
            batchField.value = _batchSize;
            batchField.style.flexGrow = 1;
            batchField.RegisterValueChangedCallback(evt =>
            {
                _batchSize = Mathf.Max(1, evt.newValue);
                SaveMemPrefs();
            });
            memoryButtons.Add(batchField);

            view.Add(memoryButtons);

            UnregisterCallbacks();
            RegisterCallbacks();
        }

        VisualElement SetupDestinationsListView()
        {
#if UNITY_2021_2_OR_NEWER
            ListView destinationsList = new ListView()
            {
                headerTitle = "Destinations",
                reorderable = true,
                showAddRemoveFooter = true,
                reorderMode = ListViewReorderMode.Animated,
                itemsSource = ShaderDestinationManager.Instance.destinations,
                fixedItemHeight = 80, // Increased height for the new field
                showBorder = true,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                makeItem = () => new ShaderDestinationListElement(),
                bindItem = (elem, index) =>
                {
                    (elem as ShaderDestinationListElement).BindListItem(ShaderDestinationManager.Instance.destinations[index]);
                },
                unbindItem = (elem, index) =>
                {
                    (elem as ShaderDestinationListElement).UnbindListItem();
                }
            };
#else
            VisualElement destinationsList = new IMGUIContainer(() =>
            {
                EditorGUILayout.HelpBox("Editing the destinations list is only supported in newer versions of Unity. Shaders will go to:\nAssets/_PoiyomiShaders/Shaders/9.0/Toon\nAssets/_PoiyomiShaders/Shaders/9.0/Pro", MessageType.Warning);
            });
#endif
            destinationsList.style.marginLeft = 6;
            destinationsList.style.marginRight = 8;
            destinationsList.style.marginTop = 10;

            return destinationsList;
        }

        void UnregisterCallbacks()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChange;
            EditorSceneManager.sceneOpened -= HandleSceneOpened;
            EditorSceneManager.newSceneCreated -= HandleNewScene;
#if UNITY_2021_2_OR_NEWER
            PrefabStage.prefabStageOpened -= HandlePrefabSceneOpenOrClose;
            PrefabStage.prefabStageClosing -= HandlePrefabSceneOpenOrClose;
#endif
        }

        void RegisterCallbacks()
        {
            EditorApplication.playModeStateChanged += HandlePlayModeStateChange;
            EditorSceneManager.sceneOpened += HandleSceneOpened;
            EditorSceneManager.newSceneCreated += HandleNewScene;
#if UNITY_2021_2_OR_NEWER
            PrefabStage.prefabStageOpened += HandlePrefabSceneOpenOrClose;
            PrefabStage.prefabStageClosing += HandlePrefabSceneOpenOrClose;
#endif
        }

#if UNITY_2021_2_OR_NEWER
        void HandlePrefabSceneOpenOrClose(PrefabStage obj) => Reload();
#endif

        void HandleNewScene(Scene scene, NewSceneSetup setup, NewSceneMode mode)
        {
            if (mode == NewSceneMode.Single)
                Reload();
        }

        void HandleSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (mode == OpenSceneMode.Single)
                Reload();
        }

        void HandlePlayModeStateChange(PlayModeStateChange obj) => Reload();

        void SelectByName(string filter, bool excludeUrp = false, bool requireUrp = false)
        {
            foreach (var element in _elements)
            {
                bool matches = element.Shader.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
                bool hasUrp = element.Shader.Name.IndexOf("URP", StringComparison.OrdinalIgnoreCase) >= 0;
                if (excludeUrp && hasUrp) matches = false;
                if (requireUrp && !hasUrp) matches = false;
                element.IsSelected = matches;
            }
        }

        public static void ShowFolderSelector(TextField textField)
        {
            string path = textField.value;
            if (Directory.Exists(path))
            {
                path = Directory.GetParent(path).FullName;
            }
            else
            {
                path = "Assets";
            }
            path = EditorUtility.OpenFolderPanel("Select folder to use", path, "");
            if (path.Length == 0)
                return;

            if (!Directory.Exists(path))
            {
                EditorUtility.DisplayDialog("Error", "The folder does not exist", "Ok");
                return;
            }

            textField.value = path.Replace(Application.dataPath, "Assets");
        }

        internal void GenerateShaders()
        {
            if (_genIsRunning)
            {
                Debug.LogWarning("Shader generation is already in progress.");
                return;
            }
            SessionState.SetBool("Poi_IsGeneratingShaders", true);
            _genIsRunning = true;

            // Lock the editor out of imports/reloading to prevent half-baked imports.
            // This will get released in EndGeneration().
            DoEditorLock();

            try
            {
                GenerateShadersInternal();
            }
            catch
            {
                EndGeneration();
                throw;
            }
        }

        private void GenerateShadersInternal()
        {
            EditorUtility.DisplayProgressBar(GenProgressTitle, "Collecting shader tasks...", 0f);

            var destinations = ShaderDestinationManager.Instance.destinations;
            var enabledDestinations = destinations.Where(dest => dest.enabled).ToArray();
            if (enabledDestinations.Length == 0)
            {
                Debug.LogError("Can't generate shaders if no destination folders are set");
                EndGeneration();
                return;
            }

            // --- Find the template file path ---
            string[] templateGuids = AssetDatabase.FindAssets("VRLT_PropsUIBoilerPlate l:poiTemplate");
            if (templateGuids.Length == 0)
            {
                Debug.LogError("Could not find VRLT_PropsUIBoilerPlate.poiTemplate. Ensure the file has the 'poiTemplate' asset label.");
                EndGeneration();
                return;
            }
            string templatePath = AssetDatabase.GUIDToAssetPath(templateGuids[0]);

            // --- Pre-processing phase (sequential, main thread) ---
            // Collect all shader generation tasks with their matched destinations
            var generationTasks = new List<(ModularShader shader, ShaderDestinationManager.ShaderDestination destination)>();

            foreach (ModularShadersGeneratorElement element in _elements.Where(x => x.IsSelected))
            {
                ShaderDestinationManager.ShaderDestination destinationRule = null;

                // Find the matching rule for the current shader element
                foreach (var rule in enabledDestinations)
                {
                    bool match = false;
                    switch (rule.matchType)
                    {
                        case ShaderDestinationManager.ShaderDestination.MatchType.Always: match = true; break;
                        case ShaderDestinationManager.ShaderDestination.MatchType.Contains: match = element.Shader.Name.IndexOf(rule.matchString, StringComparison.OrdinalIgnoreCase) >= 0; break;
                        case ShaderDestinationManager.ShaderDestination.MatchType.StartsWith: match = element.Shader.Name.StartsWith(rule.matchString, StringComparison.OrdinalIgnoreCase); break;
                        case ShaderDestinationManager.ShaderDestination.MatchType.EndsWith: match = element.Shader.Name.EndsWith(rule.matchString, StringComparison.OrdinalIgnoreCase); break;
                        case ShaderDestinationManager.ShaderDestination.MatchType.Equals: match = element.Shader.Name.Equals(rule.matchString, StringComparison.OrdinalIgnoreCase); break;
                        case ShaderDestinationManager.ShaderDestination.MatchType.Regex: match = Regex.IsMatch(element.Shader.Name, rule.matchString); break;
                    }
                    if (match)
                    {
                        destinationRule = rule;
                        break;
                    }
                }

                if (destinationRule == null)
                {
                    EditorUtility.DisplayDialog("Error", $"Couldn't match shader {element.Shader.Name} to any path.", "Ok");
                    continue;
                }

                // Check if destination folder exists
                if (!Directory.Exists(destinationRule.folderPath))
                {
                    bool createFolder = EditorUtility.DisplayDialog(
                        "Folder Does Not Exist",
                        $"The folder '{destinationRule.folderPath}' does not exist. Would you like to create it?",
                        "Create Folder",
                        "Skip Shader");

                    if (createFolder)
                    {
                        Directory.CreateDirectory(destinationRule.folderPath);
                    }
                    else
                    {
                        Debug.LogWarning($"Skipping shader '{element.Shader.Name}' because the destination folder does not exist.");
                        continue;
                    }
                }

                generationTasks.Add((element.Shader, destinationRule));
            }

            if (generationTasks.Count == 0)
            {
                Debug.Log("No shaders to generate.");
                EndGeneration();
                return;
            }

            // --- Setup phase (sequential, main thread) ---
            // Group tasks by versionOverride to handle template modifications correctly
            var groupedByVersion = generationTasks.GroupBy(t => t.destination.versionOverride ?? "").ToList();

            // Collect all prepared contexts with their duplicates for parallel generation
            var allPreparedContexts = new List<(ShaderGenerator.ShaderContext context, Dictionary<string, Poiyomi.ModularShaderSystem.ShaderModule[]> duplicates)>();

            // Phase 2: preparing contexts (2% -> 10%)
            int prepIndex = 0;
            int prepTotal = generationTasks.Count;
            bool cancelled = false;

            foreach (var group in groupedByVersion)
            {
                string versionOverride = group.Key;
                var tasksInGroup = group.ToList();

                // Apply version override once per group (modifies shared template file)
                if (!string.IsNullOrWhiteSpace(versionOverride))
                {
                    string templateContent = File.ReadAllText(templatePath);
                    string newLine = $"shader_master_label (\"<color=#E75898ff>Poiyomi {versionOverride}</color>\", Float) = 0";
                    string pattern = @"shader_master_label\s*\(""(?:<color=#E75898ff>)?Poiyomi\s*[^""]*(?:</color>)?"",\s*Float\)\s*=\s*0";
                    string newContent = Regex.Replace(templateContent, pattern, newLine);

                    File.WriteAllText(templatePath, newContent);
                    AssetDatabase.ImportAsset(templatePath, ImportAssetOptions.ForceUpdate);
                }

                // Prepare all shader contexts on main thread (Unity API calls happen here)
                foreach (var task in tasksInGroup)
                {
                    float prepProgress = 0.02f + 0.08f * (prepTotal > 0 ? (float)prepIndex / prepTotal : 0f);
                    if (EditorUtility.DisplayCancelableProgressBar(GenProgressTitle, $"Preparing {task.shader.Name} ({prepIndex + 1}/{prepTotal})", prepProgress))
                    {
                        cancelled = true;
                        break;
                    }

                    var (contexts, duplicates) = ShaderGenerator.PrepareShaderContexts(task.destination.folderPath, task.shader);
                    foreach (var ctx in contexts)
                        allPreparedContexts.Add((ctx, duplicates));
                    prepIndex++;
                }

                if (cancelled) break;
            }

            if (cancelled)
            {
                Debug.Log($"Shader generation cancelled by user during preparation. 0/{generationTasks.Count} shaders processed.");
                EndGeneration();
                return;
            }

            if (allPreparedContexts.Count == 0)
            {
                Debug.Log("No shader variants to generate.");
                EndGeneration();
                return;
            }

            // --- Hand off to async batch loop ---
            // Always batch (even in unlimited-memory mode) so the progress bar can tick.
            int batchSize = _limitMemory ? Mathf.Max(1, _batchSize) : Mathf.Max(1, Mathf.Min(allPreparedContexts.Count, 64));
            int maxWorkers = _limitMemory ? Mathf.Clamp(_maxWorkers, 1, Environment.ProcessorCount) : -1;

            _genContexts = allPreparedContexts;
            _genContextsList = allPreparedContexts.Select(x => x.context).ToList();
            _genCurrentIndex = 0;
            _genBatchSize = batchSize;
            _genTotalBatches = (allPreparedContexts.Count + batchSize - 1) / batchSize;
            _genTaskCount = generationTasks.Count;
            _genCts = new System.Threading.CancellationTokenSource();
            _genParallelOptions = new System.Threading.Tasks.ParallelOptions
            {
                MaxDegreeOfParallelism = maxWorkers,
                CancellationToken = _genCts.Token
            };
            _genCurrentBatchTask = null;
            _genCurrentBatchTotal = 0;
            _genCurrentBatchCompleted = 0;

            // Show initial state for the generation phase before yielding to the editor.
            EditorUtility.DisplayCancelableProgressBar(GenProgressTitle, $"Generating variants (0/{_genContexts.Count}) — batch 1/{_genTotalBatches}", 0.10f);

            EditorApplication.update += ProcessNextGenerationBatch;
        }

        private void ProcessNextGenerationBatch()
        {
            try
            {
                int total = _genContexts.Count;

                // === A batch is in flight — poll progress, update bar, check cancel ===
                if (_genCurrentBatchTask != null)
                {
                    int batchDone = System.Threading.Volatile.Read(ref _genCurrentBatchCompleted);
                    int globalDone = _genCurrentIndex + batchDone;
                    int batchIdx = (_genCurrentIndex / _genBatchSize) + 1;
                    float progress = 0.10f + 0.85f * (total > 0 ? (float)globalDone / total : 0f);

                    bool userCancelled = EditorUtility.DisplayCancelableProgressBar(
                        GenProgressTitle,
                        $"Generating variants ({globalDone}/{total}) — batch {batchIdx}/{_genTotalBatches}",
                        progress);

                    if (userCancelled && !_genCts.IsCancellationRequested)
                        _genCts.Cancel();

                    if (!_genCurrentBatchTask.IsCompleted)
                        return; // still running — wait for next editor tick

                    // Batch finished (success, cancel, or fault)
                    var finishedTask = _genCurrentBatchTask;
                    _genCurrentBatchTask = null;

                    if (_genCts.IsCancellationRequested)
                    {
                        EditorApplication.update -= ProcessNextGenerationBatch;
                        EditorUtility.DisplayProgressBar(GenProgressTitle, "Refreshing assets after cancel...", 0.97f);
                        AssetDatabase.Refresh();
                        Debug.Log($"Shader generation cancelled by user. {_genCurrentIndex}/{total} variants written (current batch discarded).");
                        EndGeneration();
                        return;
                    }

                    if (finishedTask.IsFaulted)
                    {
                        EditorApplication.update -= ProcessNextGenerationBatch;
                        Debug.LogException(finishedTask.Exception);
                        EndGeneration();
                        return;
                    }

                    // Success — advance to the next batch on a subsequent tick
                    _genCurrentIndex += _genCurrentBatchTotal;
                    return;
                }

                // === No batch in flight — either finish or kick off the next one ===
                if (_genCurrentIndex >= total)
                {
                    EditorApplication.update -= ProcessNextGenerationBatch;
                    FinalizeGenerationPhase();
                    EndGeneration();
                    return;
                }

                var batch = _genContexts.Skip(_genCurrentIndex).Take(_genBatchSize).ToList();
                _genCurrentBatchTotal = batch.Count;
                _genCurrentBatchCompleted = 0;
                var opts = _genParallelOptions;
                var token = _genCts.Token;

                _genCurrentBatchTask = System.Threading.Tasks.Task.Run(() =>
                {
                    System.Threading.Tasks.Parallel.ForEach(batch, opts, item =>
                    {
                        token.ThrowIfCancellationRequested();
                        item.context.GenerateShader(item.duplicates);
                        System.Threading.Interlocked.Increment(ref _genCurrentBatchCompleted);
                    });

                    token.ThrowIfCancellationRequested();

                    var batchContexts = batch.Select(b => b.context).ToList();
                    ShaderGenerator.WriteShaderFiles(batchContexts);
                    foreach (var ctx in batchContexts) ctx.ReleaseGeneratedText();
                }, token);
            }
            catch (Exception ex)
            {
                EditorApplication.update -= ProcessNextGenerationBatch;
                Debug.LogException(ex);
                EndGeneration();
            }
        }

        private void FinalizeGenerationPhase()
        {
            // The (context, duplicates) tuples in _genContexts hold DeepCopy'd ShaderModule
            // graphs that are no longer needed once the parallel work is done. Drop them now
            // so they can be collected during the rest of finalize.
            _genContexts = null;

            EditorUtility.DisplayProgressBar(GenProgressTitle, "Refreshing assets...", 0.95f);
            AssetDatabase.Refresh();

            EditorUtility.DisplayProgressBar(GenProgressTitle, "Applying default textures...", 0.97f);
            ShaderGenerator.ApplyDefaultTextures(_genContextsList);

            EditorUtility.DisplayProgressBar(GenProgressTitle, "Finalizing...", 0.99f);
            foreach (var context in _genContextsList)
            {
                context.Shader.LastGeneratedShaders.Add(AssetDatabase.LoadAssetAtPath<Shader>($"{context.FilePath}/" + context.VariantFileName));
            }

            // Free per-context heavy fields (modules, materials, etc.) now that all
            // post-generation Unity-side work that needs them is finished.
            foreach (var context in _genContextsList)
                context.ReleaseAll();

            AssetDatabase.Refresh();
            int finishedCount = _genContextsList.Count;
            Debug.Log($"Finished generating {finishedCount} shader variants from {_genTaskCount} modular shaders.");
            EditorApplication.delayCall += () =>
            {
                EditorApplication.delayCall += () =>
                {
                    Thry.ShaderEditor.ReloadActive();
                    InternalEditorUtility.RepaintAllViews();
                };
            };
        }

        // While generation is running, the editor stays live. To counteract against
        // half-baked imports and straying editor activity to deal with, lock the
        // ability to import or reload assemblies until the generator has finished
        // putting them together.
        // 
        // Make sure that the counters are balanced in the reverse order they were executed.
        private void DoEditorLock()
        {
            if (_editorLocked) return;
            _editorLocked = true;
            EditorApplication.LockReloadAssemblies();
            AssetDatabase.DisallowAutoRefresh();
        }
        private void ReleaseEditorLock()
        {
            if (!_editorLocked) return;
            _editorLocked = false;
            AssetDatabase.AllowAutoRefresh();
            EditorApplication.UnlockReloadAssemblies();
        }

        private void EndGeneration()
        {
            ReleaseEditorLock();
            EditorUtility.ClearProgressBar();
            _genContexts = null;
            _genContextsList = null;
            _genParallelOptions = null;
            _genCurrentBatchTask = null;
            _genCts = null;
            _genCurrentIndex = 0;
            _genCurrentBatchCompleted = 0;
            _genCurrentBatchTotal = 0;
            _genIsRunning = false;
            SessionState.SetBool("Poi_IsGeneratingShaders", false);

            // Encourage memory release after a generation. Deferred so this runs after
            // Unity's own shader-import finishes; otherwise UnloadUnusedAssets blocks
            // and we miss assets that are still in-flight.
            EditorApplication.delayCall += () =>
            {
                Resources.UnloadUnusedAssets();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            };
        }

        private static T[] FindAssetsByType<T>() where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();
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
    public class ModularShadersAutoGen : AssetPostprocessor
    {
        private const string IsGeneratingKey = "Poi_IsGeneratingShaders";

#if UNITY_2021_2_OR_NEWER
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
#else
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
#endif
        {
            if (SessionState.GetBool(IsGeneratingKey, false))
                return;

            bool update = false;
            foreach (var importedAssetPath in importedAssets)
            {
                // --- Add this block to ignore the specific file ---
                string fileName = System.IO.Path.GetFileName(importedAssetPath);
                if (fileName.Equals("VRLT_PropsUIBoilerPlate.poiTemplate", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue; // Skip this specific file
                }
                // --- End of new block ---

                string ext = System.IO.Path.GetExtension(importedAssetPath);
                if (ext == ".poiTemplateCollection" || ext == ".poiTemplate")
                {
                    update = true;
                    break;
                }
            }
            if (update)
            {
                var msgw = Resources.FindObjectsOfTypeAll<ModularShadersGeneratorWindow>().FirstOrDefault();
                if (msgw != null)
                {
                    if (msgw._elements != null && msgw._elements.Count(x => x.IsSelected) > 0)
                    {
                        EditorApplication.delayCall += () =>
                        {
                            SessionState.SetBool(IsGeneratingKey, true);
                            try
                            {
                                msgw.GenerateShaders();
                            }
                            finally
                            {
                                SessionState.SetBool(IsGeneratingKey, false);
                            }
                        };
                    }
                }
            }
        }
    }
}