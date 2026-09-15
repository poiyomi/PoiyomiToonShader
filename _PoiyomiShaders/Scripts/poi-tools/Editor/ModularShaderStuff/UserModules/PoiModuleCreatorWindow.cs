using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Poiyomi.ModularShaderSystem;
using UnityEditor;
using UnityEngine;

namespace Poi.Tools
{
    /// <summary>
    /// Creates a working shader module from a short form, so authoring one doesn't mean hand-editing
    /// YAML fileIDs and guessing injection keywords.
    ///
    /// The output is a matched pair — a .poiTemplateCollection holding the HLSL and a ShaderModule
    /// asset pointing into it. The references are wired up by assigning the imported sub-assets
    /// directly, so Unity writes the fileIDs itself.
    ///
    /// What gets generated compiles and visibly does something on day one; the author then edits the
    /// HLSL rather than assembling the scaffolding.
    /// </summary>
    public class PoiModuleCreatorWindow : EditorWindow
    {
        // ------------------------------------------------------------------
        // Injection points
        //
        // Every sink below lives in the same frag() body, and all five Poi structs are declared at
        // FRAGMENT_<PASS>_DECLARATIONS before any of them. So scope is identical everywhere and one
        // function signature works for every hook — only the pass coverage and the useful field
        // differ. Coverage is transcribed from the pass programs in Poi_Passes_And_Programs.
        // ------------------------------------------------------------------

        static readonly string[] AllPasses = { "BASE", "ADD", "META", "SHADOW", "OUTLINE", "LILFUR", "DEPTH", "MOTION_VECTORS" };

        /// <summary>Passes that run a lighting model. Depth and motion vectors have no lighting or emission sinks.</summary>
        static readonly string[] LitPasses = { "BASE", "ADD", "META", "SHADOW", "OUTLINE", "LILFUR" };

        /// <summary>LIGHTING_ADD exists everywhere lighting does, except the shadow caster.</summary>
        static readonly string[] AddLightPasses = { "BASE", "ADD", "META", "OUTLINE", "LILFUR" };

        /// <summary>ALPHA_LATE only exists in passes that actually output alpha.</summary>
        static readonly string[] AlphaPasses = { "BASE", "ADD", "OUTLINE", "LILFUR" };

        /// <summary>Which struct field the generated starter code writes, per hook.</summary>
        enum Effect
        {
            Setup,
            BaseColor,
            Lighting,
            Emission,
            FinalColor,
            Alpha
        }

        class HookPoint
        {
            public string Label;
            public string Sink;
            public string[] Passes;
            public Effect Effect;
            public string Help;
        }

        /// <summary>Ordered the way they run inside the fragment program.</summary>
        static readonly HookPoint[] Hooks =
        {
            new HookPoint { Label = "Setup — early",        Sink = "INIT_EARLY",     Passes = AllPasses,      Effect = Effect.Setup,      Help = "Earliest possible point. Mesh data (UVs, normals) is NOT filled in yet — only pick this if you need to run before it is." },
            new HookPoint { Label = "Setup",                Sink = "INIT",           Passes = AllPasses,      Effect = Effect.Setup,      Help = "Mesh, light and camera data is filled in by this point. Good for changing UVs." },
            new HookPoint { Label = "Setup — late",         Sink = "INIT_LATE",      Passes = AllPasses,      Effect = Effect.Setup,      Help = "After all setup, before any color is sampled." },
            new HookPoint { Label = "Color — early",        Sink = "COLOR_EARLY",    Passes = AllPasses,      Effect = Effect.BaseColor,  Help = "Before the main texture is applied." },
            new HookPoint { Label = "Color",                Sink = "COLOR",          Passes = AllPasses,      Effect = Effect.BaseColor,  Help = "Alongside the main texture and decals. The usual choice." },
            new HookPoint { Label = "Color — late",         Sink = "COLOR_LATE",     Passes = AllPasses,      Effect = Effect.BaseColor,  Help = "After all color work, just before lighting." },
            new HookPoint { Label = "Lighting — early",     Sink = "LIGHTING_EARLY", Passes = LitPasses,      Effect = Effect.Lighting,   Help = "Before the lighting model runs." },
            new HookPoint { Label = "Lighting",             Sink = "LIGHTING",       Passes = LitPasses,      Effect = Effect.Lighting,   Help = "Alongside the lighting model." },
            new HookPoint { Label = "Lighting — late",      Sink = "LIGHTING_LATE",  Passes = LitPasses,      Effect = Effect.Lighting,   Help = "After the lighting model has run." },
            new HookPoint { Label = "Add Light",            Sink = "LIGHTING_ADD",   Passes = AddLightPasses, Effect = Effect.Lighting,   Help = "Dedicated slot for adding light on top. Rim lights and glows go here." },
            new HookPoint { Label = "Emission — early",     Sink = "EMISSION_EARLY", Passes = LitPasses,      Effect = Effect.Emission,   Help = "Before the emission modules run." },
            new HookPoint { Label = "Emission",             Sink = "EMISSION",       Passes = LitPasses,      Effect = Effect.Emission,   Help = "Alongside the emission modules." },
            new HookPoint { Label = "Emission — late",      Sink = "EMISSION_LATE",  Passes = LitPasses,      Effect = Effect.Emission,   Help = "After all emission has been added." },
            new HookPoint { Label = "Final Result",         Sink = "RETURN",         Passes = AllPasses,      Effect = Effect.FinalColor, Help = "Last chance to change the shaded result before alpha cutout." },
            new HookPoint { Label = "Alpha — after cutout", Sink = "ALPHA_LATE",     Passes = AlphaPasses,    Effect = Effect.Alpha,      Help = "After clipping. Only affects the alpha that gets written out." },
        };

        const string PropertySkeletonAsset = "VRLTC_PoiPropertySkeleton";

        /// <summary>
        /// Only used if the skeleton can't be read. Sections added to the shader after this was
        /// written won't be here — which is exactly why the list is normally parsed instead.
        /// </summary>
        static readonly (string Label, string Keyword)[] FallbackUiSections =
        {
            ("Third Party", "THIRDPARTY_PROPERTIES"),
            ("Color & Normals", "MAIN_PROPERTIES"),
            ("Outlines", "OUTLINE_PROPERTIES"),
            ("Shading", "LIGHTING_PROPERTIES"),
            ("Special FX", "SPECIALFX_PROPERTIES"),
            ("Raymarching", "RAYMARCHING_PROPERTIES"),
            ("Vertex Options", "VERTEX_PROPERTIES"),
            ("Grab Pass", "GRABPASS_PROPERTIES"),
            ("Global Modifiers & Data", "MODIFIER_PROPERTIES"),
            ("Global Data and Masks", "GLOBAL_PROPERTIES"),
            ("UVs", "UV_PROPERTIES"),
            ("Post Processing", "POSTPROCESSING_PROPERTIES"),
            ("Extras", "EXTRAS_PROPERTIES"),
            ("Rendering", "RENDERING_PROPERTIES"),
        };

        static (string Label, string Keyword)[] _uiSections;

        /// <summary>
        /// Where the module's settings appear in the material inspector, read from the shader's own
        /// property skeleton so adding a section to the shader shows up here without a code change.
        /// </summary>
        static (string Label, string Keyword)[] UiSections
        {
            get
            {
                if (_uiSections != null)
                    return _uiSections;

                var parsed = ParsePropertySkeleton();
                return _uiSections = parsed.Count >= 3 ? parsed.ToArray() : FallbackUiSections;
            }
        }

        /// <summary>
        /// Pulls (label, keyword) pairs out of VRLTC_PoiPropertySkeleton, in inspector order.
        ///
        /// Each #T# block declares one section: a category property carrying the display name,
        /// then the #K# sink. Some blocks nest other sections inside them — the modifier block holds
        /// UVs and Post Processing — so a nested sink takes the nearest label above it, while the
        /// sink matching the block's own name takes the block's first label. Without that split,
        /// the outer section inherits the last nested label, since its sink is written last.
        /// </summary>
        static List<(string Label, string Keyword)> ParsePropertySkeleton()
        {
            var sections = new List<(string, string)>();
            try
            {
                string path = AssetDatabase.FindAssets(PropertySkeletonAsset)
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .FirstOrDefault(x => Path.GetFileNameWithoutExtension(x) == PropertySkeletonAsset);
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    return sections;

                // Matches m_mainCategory ("Color & Normals--{...}" and m_start_PoiUVCategory ("UVs".
                // Requiring "Category" in the name skips sub-headers like m_start_PoiTimeOptions.
                var labelPattern = new System.Text.RegularExpressions.Regex(
                    @"m_\w*Category\s*\(\s*""\s*([^""\-]+)");
                var sinkPattern = new System.Text.RegularExpressions.Regex(@"#K#(\w+_PROPERTIES)");

                string blockName = null;
                string firstLabel = null;
                string latestLabel = null;

                foreach (string line in File.ReadAllLines(path))
                {
                    if (line.Contains("#T#"))
                    {
                        blockName = line.Replace("#T#", "").Trim();
                        firstLabel = null;
                        latestLabel = null;
                        continue;
                    }

                    var label = labelPattern.Match(line);
                    if (label.Success)
                    {
                        latestLabel = label.Groups[1].Value.Trim();
                        if (firstLabel == null) firstLabel = latestLabel;
                        continue;
                    }

                    var sink = sinkPattern.Match(line);
                    if (!sink.Success) continue;

                    string keyword = sink.Groups[1].Value;
                    string chosen = keyword == blockName ? firstLabel : latestLabel;
                    sections.Add((chosen ?? Prettify(keyword), keyword));
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Poiyomi] Couldn't read the inspector sections from {PropertySkeletonAsset}, " +
                                 $"falling back to the built-in list. {ex.Message}");
                sections.Clear();
            }
            return sections;
        }

        /// <summary>"SPECIALFX_PROPERTIES" becomes "Specialfx", for sections with no category label.</summary>
        static string Prettify(string keyword)
        {
            string trimmed = keyword.Replace("_PROPERTIES", "").Replace('_', ' ').Trim();
            if (trimmed.Length == 0) return keyword;
            return char.ToUpperInvariant(trimmed[0]) + trimmed.Substring(1).ToLowerInvariant();
        }

        static readonly HashSet<string> PassesOnByDefault =
            new HashSet<string> { "BASE", "ADD", "META", "SHADOW", "OUTLINE", "LILFUR" };

        string _displayName = "My Module";
        string _author = "";
        string _version = "1.0";
        string _description = "";
        int _uiSection;
        int _hookIndex = Array.FindIndex(Hooks, h => h.Sink == "COLOR");
        bool[] _passEnabled = AllPasses.Select(PassesOnByDefault.Contains).ToArray();
        string _outputFolder = "Assets/PoiyomiModules";
        Vector2 _scroll;

        HookPoint SelectedHook => Hooks[Mathf.Clamp(_hookIndex, 0, Hooks.Length - 1)];

        [MenuItem("Poi/Create New Module", priority = 21)]
        public static void Open()
        {
            var window = GetWindow<PoiModuleCreatorWindow>();
            window.titleContent = new GUIContent("Create Module");
            window.minSize = new Vector2(440, 520);
            window.Show();
        }

        // ------------------------------------------------------------------
        // Naming
        // ------------------------------------------------------------------

        /// <summary>
        /// The display name reduced to something usable as an HLSL identifier and asset name.
        /// "My Cool Effect" becomes "MyCoolEffect".
        /// </summary>
        string Identifier
        {
            get
            {
                var sb = new StringBuilder();
                foreach (char c in _displayName ?? "")
                    if (char.IsLetterOrDigit(c))
                        sb.Append(sb.Length == 0 && char.IsDigit(c) ? '_' : c);
                return sb.ToString();
            }
        }

        /// <summary>Prefix for every shader property the module declares, e.g. "_MyCoolEffect".</summary>
        string Prefix => "_" + Identifier;

        string ModuleFolder => $"{_outputFolder}/{Identifier}";

        bool IsPassAvailable(string pass) => SelectedHook.Passes.Contains(pass);

        /// <summary>Ticked passes the selected hook can actually reach.</summary>
        List<string> ActivePasses =>
            AllPasses.Where((p, i) => _passEnabled[i] && IsPassAvailable(p)).ToList();

        /// <summary>Blocking problem with the current form, or null when it's good to generate.</summary>
        string Validate()
        {
            if (string.IsNullOrWhiteSpace(_displayName))
                return "Give the module a name.";
            if (Identifier.Length == 0)
                return "The name needs at least one letter or number.";
            if (string.IsNullOrWhiteSpace(_outputFolder))
                return "Pick an output folder.";
            if (!_outputFolder.Replace('\\', '/').StartsWith("Assets"))
                return "The output folder has to be inside Assets.";
            if (ActivePasses.Count == 0)
                return "Select at least one pass this hook can reach.";
            if (Directory.Exists(ModuleFolder))
                return $"\"{ModuleFolder}\" already exists. Pick a different name or folder.";

            string id = Identifier;
            bool idTaken = AssetDatabase.FindAssets("t:ShaderModule")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ShaderModule>)
                .Any(x => x != null && string.Equals(x.Id, id, StringComparison.OrdinalIgnoreCase));
            if (idTaken)
                return $"A module with the id \"{id}\" already exists. Pick a different name.";

            return null;
        }

        // ------------------------------------------------------------------
        // GUI
        // ------------------------------------------------------------------

        void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            GUILayout.Space(4);
            EditorGUILayout.LabelField("Create a New Module", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Generates a working module you can edit. It shows up in Installed Modules when you're done.",
                EditorStyles.wordWrappedMiniLabel);
            GUILayout.Space(6);

            _displayName = EditorGUILayout.TextField("Name", _displayName);
            _author = EditorGUILayout.TextField("Author", _author);
            _version = EditorGUILayout.TextField("Version", _version);
            _description = EditorGUILayout.TextField("Description", _description);

            GUILayout.Space(8);
            _uiSection = EditorGUILayout.Popup(
                new GUIContent("Inspector Section", "Which section of the material inspector the module's settings appear under."),
                Mathf.Clamp(_uiSection, 0, UiSections.Length - 1),
                UiSections.Select(x => x.Label).ToArray());

            _hookIndex = EditorGUILayout.Popup(
                new GUIContent("Hooks Into", "Where in the fragment program your code runs."),
                _hookIndex,
                Hooks.Select(x => x.Label).ToArray());
            EditorGUILayout.LabelField(" ", SelectedHook.Help, EditorStyles.wordWrappedMiniLabel);

            GUILayout.Space(8);
            EditorGUILayout.LabelField("Passes", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            for (int i = 0; i < AllPasses.Length; i++)
            {
                bool available = IsPassAvailable(AllPasses[i]);
                using (new EditorGUI.DisabledScope(!available))
                {
                    string label = available
                        ? PassLabel(AllPasses[i])
                        : $"{PassLabel(AllPasses[i])} — not in this pass";
                    bool value = EditorGUILayout.ToggleLeft(label, _passEnabled[i] && available);
                    if (available)
                        _passEnabled[i] = value;
                }
            }
            EditorGUI.indentLevel--;

            GUILayout.Space(8);
            using (new EditorGUILayout.HorizontalScope())
            {
                _outputFolder = EditorGUILayout.TextField("Output Folder", _outputFolder);
                if (GUILayout.Button("...", GUILayout.Width(30)))
                    BrowseForOutputFolder();
            }

            if (Identifier.Length > 0)
            {
                EditorGUILayout.LabelField(" ", $"{ModuleFolder}/", EditorStyles.miniLabel);
                EditorGUILayout.LabelField(" ", $"Properties named {Prefix}Enabled, {Prefix}Color, …", EditorStyles.miniLabel);
            }

            GUILayout.Space(10);
            string error = Validate();
            if (error != null)
                EditorGUILayout.HelpBox(error, MessageType.Warning);

            using (new EditorGUI.DisabledScope(error != null))
            {
                if (GUILayout.Button("Create Module", GUILayout.Height(30)))
                    Create();
            }

            GUILayout.Space(4);
            EditorGUILayout.EndScrollView();
        }

        static string PassLabel(string pass)
        {
            switch (pass)
            {
                case "BASE": return "Base (the main forward pass)";
                case "ADD": return "Add (extra realtime lights)";
                case "META": return "Meta (lightmap baking)";
                case "SHADOW": return "Shadow caster";
                case "OUTLINE": return "Outline";
                case "LILFUR": return "Fur";
                case "DEPTH": return "Depth / Depth-Normals";
                case "MOTION_VECTORS": return "Motion vectors";
                default: return pass;
            }
        }

        void BrowseForOutputFolder()
        {
            string picked = EditorUtility.OpenFolderPanel("Output Folder", _outputFolder, "");
            if (string.IsNullOrEmpty(picked))
                return;

            picked = picked.Replace('\\', '/');
            string dataPath = Application.dataPath.Replace('\\', '/');
            if (!picked.StartsWith(dataPath))
            {
                EditorUtility.DisplayDialog("Outside Project", "Pick a folder inside this project's Assets folder.", "Ok");
                return;
            }
            _outputFolder = "Assets" + picked.Substring(dataPath.Length);
            GUI.FocusControl(null);
        }

        // ------------------------------------------------------------------
        // Generation
        // ------------------------------------------------------------------

        void Create()
        {
            try
            {
                string modulePath = Generate();
                var module = AssetDatabase.LoadAssetAtPath<ShaderModule>(modulePath);
                Selection.activeObject = module;
                EditorGUIUtility.PingObject(module);

                Debug.Log($"[Poiyomi] Created module \"{_displayName}\" at {ModuleFolder}. " +
                          "It's now listed in Poi > Installed Modules — tick it and hit Apply to try it out.");

                EditorUtility.DisplayDialog(
                    "Module Created",
                    $"\"{_displayName}\" was created in {ModuleFolder}.\n\n" +
                    "Edit the .poiTemplateCollection file to write your effect.\n\n" +
                    "To try it: Poi > Installed Modules, tick it, then Apply & Rebuild Shaders.",
                    "Ok");

                Close();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("Module Creation Failed", ex.Message, "Ok");
            }
        }

        /// <summary>Writes the collection and module assets. Returns the module asset path.</summary>
        string Generate()
        {
            string id = Identifier;
            Directory.CreateDirectory(ModuleFolder);

            string collectionPath = $"{ModuleFolder}/VRLTC_{id}.poiTemplateCollection";
            File.WriteAllText(collectionPath, BuildCollectionText());
            AssetDatabase.ImportAsset(collectionPath, ImportAssetOptions.ForceUpdate);

            // The importer splits each #T# section into its own TemplateAsset sub-asset, named after
            // the section. Assigning those objects directly lets Unity serialize the fileID links.
            var templates = AssetDatabase.LoadAllAssetRepresentationsAtPath(collectionPath)
                .OfType<TemplateAsset>()
                .ToDictionary(x => x.name, x => x);

            TemplateAsset Section(string suffix)
            {
                string name = id + suffix;
                if (!templates.TryGetValue(name, out var asset))
                    throw new InvalidOperationException(
                        $"Template section \"{name}\" was not produced by the importer. " +
                        $"Sections found: {string.Join(", ", templates.Keys)}");
                return asset;
            }

            var activePasses = ActivePasses;
            string sink = SelectedHook.Sink;

            var module = ScriptableObject.CreateInstance<ShaderModule>();
            module.Id = id;
            module.Name = _displayName;
            module.Version = string.IsNullOrWhiteSpace(_version) ? "1.0" : _version;
            module.Author = _author ?? "";
            module.Description = _description ?? "";
            module.EnableProperties = new List<EnableProperty>();
            module.Properties = new List<Property>();
            module.ModuleDependencies = new List<string>();
            module.IncompatibleWith = new List<string>();
            module.Functions = new List<ShaderFunction>();
            module.Templates = new List<ModuleTemplate>
            {
                new ModuleTemplate
                {
                    Template = Section("Properties"),
                    Keywords = new List<string> { UiSections[Mathf.Clamp(_uiSection, 0, UiSections.Length - 1)].Keyword },
                    Queue = 100
                },
                new ModuleTemplate
                {
                    Template = Section("VariablesExposed"),
                    Keywords = activePasses.Select(p => $"{p}_PROPERTY_VARIABLES_EXPOSED").ToList(),
                    Queue = 100
                },
                new ModuleTemplate
                {
                    Template = Section("Functions"),
                    Keywords = activePasses.Select(p => $"FRAGMENT_{p}_FUNCTIONS").ToList(),
                    Queue = 100
                },
                new ModuleTemplate
                {
                    Template = Section("FunctionCalls"),
                    Keywords = activePasses.Select(p => $"FRAGMENT_{p}_{sink}").ToList(),
                    Queue = 100
                },
            };

            string modulePath = $"{ModuleFolder}/VRLM_{id}.asset";
            AssetDatabase.CreateAsset(module, modulePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return modulePath;
        }

        /// <summary>
        /// The .poiTemplateCollection body. Each #T# header becomes a separately addressable
        /// template. The //ifex ... //endex pairs let the optimizer strip the whole feature out of
        /// locked materials that have it turned off.
        /// </summary>
        string BuildCollectionText()
        {
            string id = Identifier;
            string p = Prefix;
            string name = _displayName;
            var sb = new StringBuilder();

            sb.AppendLine($"#T#{id}Properties");
            sb.AppendLine($"//ifex {p}Enabled==0");
            sb.AppendLine($"[HideInInspector] m_start_{id} (\" {name}--{{reference_property:{p}Enabled}}\", Float) = 0");
            sb.AppendLine($"[ThryHideInInspector][ToggleUI]{p}Enabled (\"Enable\", Float) = 0");
            sb.AppendLine($"{p}Color (\"Color\", Color) = (1, 1, 1, 1)");
            sb.AppendLine($"{p}Strength (\"Strength\", Range(0, 1)) = 1");
            sb.AppendLine($"[HideInInspector] m_end_{id} (\"{name}\", Float) = 0");
            sb.AppendLine("//endex");
            sb.AppendLine();

            sb.AppendLine($"#T#{id}VariablesExposed");
            sb.AppendLine($"//ifex {p}Enabled==0");
            sb.AppendLine($"float {p}Enabled;");
            sb.AppendLine($"float4 {p}Color;");
            sb.AppendLine($"float {p}Strength;");
            sb.AppendLine("//endex");
            sb.AppendLine();

            // Everything is passed inout so the starter can be edited to touch any of it without
            // having to change the signature and the call site in step.
            sb.AppendLine($"#T#{id}Functions");
            sb.AppendLine($"//ifex {p}Enabled==0");
            sb.AppendLine($"void Apply{id}(inout PoiFragData poiFragData, inout PoiMesh poiMesh, inout PoiLight poiLight, inout PoiCam poiCam, inout PoiMods poiMods)");
            sb.AppendLine("{");
            sb.AppendLine($"    if ({p}Enabled < 0.5) return;");
            sb.AppendLine();
            foreach (string line in StarterBody(SelectedHook.Effect, p))
                sb.AppendLine("    " + line);
            sb.AppendLine("}");
            sb.AppendLine("//endex");
            sb.AppendLine();

            sb.AppendLine($"#T#{id}FunctionCalls");
            sb.AppendLine($"//ifex {p}Enabled==0");
            sb.AppendLine($"Apply{id}(poiFragData, poiMesh, poiLight, poiCam, poiMods);");
            sb.AppendLine("//endex");

            return sb.ToString();
        }

        /// <summary>
        /// Placeholder effect matched to the hook, so a freshly generated module does something
        /// visible instead of nothing. Every field written here is one the struct actually has.
        /// </summary>
        static IEnumerable<string> StarterBody(Effect effect, string p)
        {
            switch (effect)
            {
                case Effect.Setup:
                    yield return "// Your effect goes here. This runs before any color is sampled,";
                    yield return "// so it's a good spot to change UVs or set up masks.";
                    yield return "// Note: mesh data is only filled in from the \"Setup\" hook onward,";
                    yield return "// so this does nothing if you picked \"Setup - early\".";
                    yield return $"poiMesh.uv[0] += {p}Strength * 0.1;";
                    break;

                case Effect.BaseColor:
                    yield return "// Your effect goes here. This starter tints the material color.";
                    yield return $"poiFragData.baseColor = lerp(poiFragData.baseColor, poiFragData.baseColor * {p}Color.rgb, {p}Strength);";
                    break;

                case Effect.Lighting:
                    yield return "// Your effect goes here. This starter adds a flat glow on top of the lighting.";
                    yield return $"poiLight.finalLightAdd += {p}Color.rgb * {p}Strength;";
                    break;

                case Effect.Emission:
                    yield return "// Your effect goes here. This starter adds flat emission.";
                    yield return $"poiFragData.emission += {p}Color.rgb * {p}Strength;";
                    break;

                case Effect.FinalColor:
                    yield return "// Your effect goes here. This starter tints the fully shaded result.";
                    yield return $"poiFragData.finalColor = lerp(poiFragData.finalColor, poiFragData.finalColor * {p}Color.rgb, {p}Strength);";
                    break;

                case Effect.Alpha:
                    yield return "// Your effect goes here. This starter fades the material out.";
                    yield return $"poiFragData.alpha *= lerp(1, {p}Color.a, {p}Strength);";
                    break;
            }
        }
    }
}
