using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Poiyomi.ModularShaderSystem.CibbiExtensions;
using UnityEditor;
using UnityEngine;

namespace Poiyomi.ModularShaderSystem
{
    public static class ShaderGenerator
    {
        const string ITERATOR_NAMING = @"\[IDX_\w+(\+\+)?]"; // do [IDX_{}++] to increase the iterator;

        public static void GenerateShader(string path, ModularShader shader, bool hideVariants = false)
        {
            GenerateShader(path, shader, null, hideVariants);
        }

        public static void GenerateShader(string path, ModularShader shader, Action<StringBuilder, ShaderContext> postGeneration, bool hideVariants = false)
        {
            var contexts = GenerateShaderBatched(path, shader, postGeneration, hideVariants);
            AssetDatabase.Refresh();
            ApplyDefaultTextures(contexts);

            foreach (var context in contexts)
                shader.LastGeneratedShaders.Add(AssetDatabase.LoadAssetAtPath<Shader>($"{context.FilePath}/" + context.VariantFileName));
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Generates shader files without refreshing the asset database. Use this for batching multiple shader generations.
        /// Call AssetDatabase.Refresh(), ApplyDefaultTextures(), and FinalizeGeneratedShaders() after all shaders are generated.
        /// </summary>
        public static List<ShaderContext> GenerateShaderBatched(string path, ModularShader shader, Action<StringBuilder, ShaderContext> postGeneration = null, bool hideVariants = false)
        {
            var (contexts, duplicates) = PrepareShaderContexts(path, shader, postGeneration, hideVariants);

            contexts.AsParallel().ForAll(x => x.GenerateShader(duplicates));

            WriteShaderFiles(contexts);

            return contexts;
        }

        /// <summary>
        /// Prepares shader contexts without generating them. Must be called on the main thread.
        /// Returns contexts and duplicates dictionary needed for generation.
        /// Use GenerateShaderContexts() to run the actual generation (can be parallelized).
        /// </summary>
        public static (List<ShaderContext> contexts, Dictionary<string, ShaderModule[]> duplicates) PrepareShaderContexts(
            string path, ModularShader shader, Action<StringBuilder, ShaderContext> postGeneration = null, bool hideVariants = false)
        {
            if(Path.IsPathRooted(path) && !Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = GetPathRelativeToProject(path);

            var modules = FindAllModules(shader);
            var duplicates = GenerateDuplicates(modules);
            ApplyDupliactes(modules, duplicates);

            var freshAssets = new Dictionary<TemplateAsset, TemplateAsset>();

            freshAssets.AddFreshShaderToList(shader.ShaderTemplate);
            freshAssets.AddFreshShaderToList(shader.ShaderPropertiesTemplate);

            foreach ((var template, var isCopy) in modules.SelectMany(x => x.Templates.Select(t => (t,x.IsCopy))))
                freshAssets.AddFreshShaderToList(template.Template, isCopy);

            foreach ((var function, var isCopy) in modules.SelectMany(x => x.Functions.Select(f => (f,x.IsCopy))))
                freshAssets.AddFreshShaderToList(function.ShaderFunctionCode, isCopy);

            var possibleVariants = GetShaderVariants(modules);
            var contexts = new List<ShaderContext>();
            var completePropertiesBlock = GetPropertiesBlock(shader, modules, freshAssets);

            foreach (var variant in possibleVariants)
            {
                contexts.Add(new ShaderContext
                {
                    Shader = shader,
                    PostGeneration = postGeneration,
                    ActiveEnablers = variant,
                    PreparedModules = FindActiveModules(shader, variant, duplicates),
                    DisabledSections = shader.DisabledSections?.ToArray() ?? Array.Empty<string>(),
                    FreshAssets = freshAssets,
                    FilePath = path,
                    PropertiesBlock = completePropertiesBlock,
                    AreVariantsHidden = hideVariants
                });
            }

            // Clear old generated shaders
            if (shader.LastGeneratedShaders != null)
            {
                foreach (Shader generatedShader in shader.LastGeneratedShaders.Where(x => x != null))
                {
                    string assetPath = AssetDatabase.GetAssetPath(generatedShader);
                    if (string.IsNullOrWhiteSpace(assetPath))
                        File.Delete(assetPath);
                }
            }
            shader.LastGeneratedShaders = new List<Shader>();

            return (contexts, duplicates);
        }

        /// <summary>
        /// Writes generated shader files to disk. Call after GenerateShader() has been called on all contexts.
        /// </summary>
        public static void WriteShaderFiles(List<ShaderContext> contexts)
        {
            foreach (var context in contexts)
            {
                string filePath = $"{context.FilePath}/" + context.VariantFileName;
                if (File.Exists(filePath))
                {
                    FileAttributes fileAttributes = File.GetAttributes(filePath) & ~FileAttributes.ReadOnly;
                    File.SetAttributes(filePath, fileAttributes);
                    File.WriteAllText(filePath, context.ShaderFile.ToString());
                    File.SetAttributes(filePath, fileAttributes | FileAttributes.ReadOnly);
                }
                else
                {
                    File.WriteAllText(filePath, context.ShaderFile.ToString());
                    File.SetAttributes(filePath, File.GetAttributes(filePath) | FileAttributes.ReadOnly);
                }
            }
        }

        private static string GetPathRelativeToProject(string path)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"The folder \"{path}\" is not found");

            // Already a relative path
            if (path.StartsWith("Assets") || path.StartsWith("Packages"))
                return path;

            // Get project root (parent of Assets folder)
            string projectRoot = Directory.GetParent(Application.dataPath).FullName.Replace("\\", "/");
            path = path.Replace("\\", "/");

            // Check if path is within the project
            if (!path.StartsWith(projectRoot))
                throw new DirectoryNotFoundException($"The folder \"{path}\" is not part of the unity project");

            // Convert to relative path
            path = path.Substring(projectRoot.Length).TrimStart('/');

            return path;
        }

        public static void GenerateMinimalShader(string path, ModularShader shader, IEnumerable<Material> materials, Action<StringBuilder, ShaderContext> postGeneration = null)
        {
            path = GetPathRelativeToProject(path);

            var modules = FindAllModules(shader);
            var possibleVariants = GetMinimalVariants(modules, materials);
            var contexts = new List<ShaderContext>();

            foreach (var (variant, variantMaterials) in possibleVariants)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(variantMaterials[0], out string guid, out long  _);
                contexts.Add(new ShaderContext
                {
                    Shader = shader,
                    PostGeneration = postGeneration,
                    ActiveEnablers = variant,
                    FilePath = path,
                    OptimizedShader = true,
                    Materials = variantMaterials,
                    Guid = guid
                });
            }

            contexts.GenerateMinimalShaders();
        }

        public static List<ShaderContext> EnqueueShadersToGenerate(string path, ModularShader shader, IEnumerable<Material> materials, Action<StringBuilder, ShaderContext> postGeneration = null)
        {
            path = GetPathRelativeToProject(path);

            var modules = FindAllModules(shader);
            var possibleVariants = GetMinimalVariants(modules, materials);
            var contexts = new List<ShaderContext>();

            foreach (var (variant, variantMaterials) in possibleVariants)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(variantMaterials[0], out string guid, out long  _);
                contexts.Add(new ShaderContext
                {
                    Shader = shader,
                    PostGeneration = postGeneration,
                    ActiveEnablers = variant,
                    FilePath = path,
                    OptimizedShader = true,
                    Materials = variantMaterials,
                    Guid = guid
                });
            }

            return contexts;
        }

        public static void GenerateMinimalShaders(this List<ShaderContext> contexts)
        {
            if (contexts == null || contexts.Count == 0) return;

            var alreadyDoneShaders = new List<ModularShader>();

            var freshAssets = new Dictionary<TemplateAsset, TemplateAsset>();

            foreach (var context in contexts)
            {
                context.FreshAssets = freshAssets;
                context.PreparedModules = FindActiveModules(context.Shader, context.ActiveEnablers, null);
                context.DisabledSections = context.Shader.DisabledSections?.ToArray() ?? Array.Empty<string>();
                if (alreadyDoneShaders.Contains(context.Shader)) continue;

                var shader = context.Shader;
                var modules = FindAllModules(shader);

                freshAssets.AddFreshShaderToList(shader.ShaderTemplate);
                freshAssets.AddFreshShaderToList(shader.ShaderPropertiesTemplate);

                foreach (var template in modules.SelectMany(x => x.Templates))
                    freshAssets.AddFreshShaderToList(template.Template);

                foreach (var function in modules.SelectMany(x => x.Functions))
                    freshAssets.AddFreshShaderToList(function.ShaderFunctionCode);

                alreadyDoneShaders.Add(shader);
            }

            EditorUtility.DisplayProgressBar("Generating Optimized Shaders", "generating shader files", 1 / (contexts.Count + 3));
            contexts.AsParallel().ForAll(x => x.GenerateShader());
            try
            {
                AssetDatabase.StartAssetEditing();
                int i = 0;
                foreach (var context in contexts)
                {
                    EditorUtility.DisplayProgressBar("Generating Optimized Shaders", "Saving " + context.VariantFileName, 1 + i / (contexts.Count + 3));
                    File.WriteAllText($"{context.FilePath}/" + context.VariantFileName, context.ShaderFile.ToString());
                    i++;
                }
            }
            finally
            {
                EditorUtility.DisplayProgressBar("Generating Optimized Shaders", "waiting for unity to compile shaders", contexts.Count - 2 / (contexts.Count + 3));
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }

            ApplyDefaultTextures(contexts);

            EditorUtility.DisplayProgressBar("Generating Optimized Shaders", "applying shaders to materials", contexts.Count - 1 / (contexts.Count + 3));
            foreach (var context in contexts)
            {
                var shader = Shader.Find(context.ShaderName);
                foreach (var material in context.Materials)
                {
                    material.shader = shader;
                }
            }

            EditorUtility.ClearProgressBar();
        }

        private static List<Dictionary<string, int>> GetShaderVariants(List<ShaderModule> modules)
        {
            var dictionary = new Dictionary<string, List<int>>();
            foreach (ShaderModule module in modules)
            {
                if (module == null) continue;
                foreach (EnableProperty property in module.EnableProperties)
                {
                    if (property == null || string.IsNullOrWhiteSpace(property.Name) ||
                        !(module.Templates?.Any(x => x.NeedsVariant) ?? false)) continue;

                    if (dictionary.ContainsKey(property.Name))
                        dictionary[property.Name].Add(property.EnableValue);
                    else
                        dictionary.Add(property.Name, new List<int>(new[] { property.EnableValue }));
                }
            }

            var keys = dictionary.Keys.ToList();

            foreach (KeyValuePair<string,List<int>> keyValuePair in dictionary)
                if(!keyValuePair.Value.Contains(0))
                    keyValuePair.Value.Insert(0,0);

            var states = new List<Dictionary<string, int>>();
            UnrollVariants(states, new Dictionary<string, int>(), dictionary, keys);

            return states;
        }

        private static List<(Dictionary<string, int>, List<Material>)> GetMinimalVariants(List<ShaderModule> modules, IEnumerable<Material> materials)
        {
            var enablers = new List<string>();
            foreach (ShaderModule module in modules)
            {
                if (module == null) continue;
                foreach (EnableProperty property in module.EnableProperties)
                {
                    if (property == null || string.IsNullOrWhiteSpace(property.Name)) continue;

                    enablers.Add(property.Name);
                }
            }

            enablers = enablers.Distinct().ToList();

            var states = new List<(Dictionary<string, int>, List<Material>)>();
            foreach (Material material in materials)
            {
                var state = new Dictionary<string, int>();
                foreach (string enabler in enablers)
                    state.Add(enabler, (int)material.GetFloat(enabler));

                var equalState = states.Where(x =>
                {
                    var keys = state.Keys;
                    foreach (string key in keys)
                        if (x.Item1[key] != state[key])
                            return false;

                    return true;
                }).FirstOrDefault();

                if(equalState == (null, null))
                    states.Add((state, new List<Material>(new [] {material})));
                else
                    equalState.Item2.Add(material);
            }

            return states;
        }

        private static void UnrollVariants(ICollection<Dictionary<string, int>> states, Dictionary<string, int> current, IReadOnlyDictionary<string, List<int>> dictionary, IReadOnlyList<string> keys)
        {
            if (current.Count == keys.Count)
            {
                states.Add(current);
                return;
            }
            foreach (var value in dictionary[keys[current.Count]])
            {
                var next = new Dictionary<string, int>(current);
                next[keys[current.Count]] = value;
                UnrollVariants(states, next, dictionary, keys);
            }
        }

        public static string GetVariantCode(Dictionary<string, int> activeEnablers)
        {
            var keys = activeEnablers.Keys.OrderBy(x => x).ToList();
            bool isAllZeroes = true;
            var b = new StringBuilder();
            foreach (string key in keys)
            {
                if (activeEnablers[key] != 0) isAllZeroes = false;
                b.Append($"-{activeEnablers[key]}");
            }

            return isAllZeroes ? "" : b.ToString();
        }

        private static void AddFreshShaderToList(this Dictionary<TemplateAsset, TemplateAsset> dictionary, TemplateAsset asset, bool isModuleCopy = false)
        {
            if ((object)asset == null) return;
            if (asset.Equals(null)) return;
            if (dictionary.ContainsKey(asset)) return;
            if(isModuleCopy)
            {
                dictionary.Add(asset, asset);
                return;
            }
            string assetName = asset.name;
            string assetPath = AssetDatabase.GetAssetPath(asset);
            var genericAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            TemplateAsset template = null;
            switch (genericAsset)
            {
                case TemplateCollectionAsset collection:
                    template = collection.Templates.FirstOrDefault(x => x.name.Equals(assetName));
                    break;
                case TemplateAsset t:
                    template = t;
                    break;
            }
            dictionary.Add(asset, template);
        }

        private static TemplateAsset GetTemplate(this Dictionary<TemplateAsset, TemplateAsset> dictionary, TemplateAsset asset)
        {
            if ((object)asset == null) return null;
            if (asset.Equals(null)) return null;
            return dictionary.TryGetValue(asset, out TemplateAsset result) ? result : null;
        }

        public class ShaderContext
        {
            public ModularShader Shader;
            public Dictionary<string, int> ActiveEnablers;
            public Dictionary<TemplateAsset, TemplateAsset> FreshAssets;
            public Action<StringBuilder, ShaderContext> PostGeneration;
            // Captured on the editor thread. Worker generation must not read local asset preferences.
            public List<ShaderModule> PreparedModules;
            public string[] DisabledSections = Array.Empty<string>();
            private List<EnableProperty> _liveUpdateEnablers;
            public string FilePath;
            public string VariantFileName;
            public string VariantName;
            public string ShaderName;
            public string PropertiesBlock;
            public bool AreVariantsHidden;
            public bool OptimizedShader;
            public List<Material> Materials;
            public StringBuilder ShaderFile;
            private List<ShaderModule> _modules;
            private List<ShaderFunction> _functions;
            private List<ShaderFunction> _reorderedFunctions;
            private Dictionary<ShaderFunction, ShaderModule> _modulesByFunctions;
            public string Guid;

            public List<ShaderModule> Modules => _modules;

            public void ReleaseGeneratedText()
            {
                ShaderFile = null;
                _functions = null;
                _reorderedFunctions = null;
                _modulesByFunctions = null;
                _liveUpdateEnablers = null;
                FreshAssets = null;
                PropertiesBlock = null;
                ActiveEnablers = null;
                PreparedModules = null;
                DisabledSections = null;
            }

            // Drop everything not needed once shader files are imported and default textures applied.
            // Call this after ApplyDefaultTextures has run.
            public void ReleaseAll()
            {
                ReleaseGeneratedText();
                _modules = null;
                Materials = null;
                PostGeneration = null;
            }

            public void GenerateShader(Dictionary<string, ShaderModule[]> duplicatedModules = null)
            {
                _modules = PreparedModules ?? FindActiveModules(Shader, ActiveEnablers, duplicatedModules);
                GetLiveUpdateEnablers();
                ShaderFile = new StringBuilder();
                VariantName = GetVariantCode(ActiveEnablers);
                VariantFileName = OptimizedShader ?
                    $"{Shader.Name}{(string.IsNullOrEmpty(VariantName) ? "" : $"-g-{Guid}")}.shader" :
                    $"{Shader.Name}{(string.IsNullOrEmpty(VariantName) ? "" : $"-v{VariantName}")}.shader";

                VariantFileName = string.Join("_", VariantFileName.Split(Path.GetInvalidFileNameChars()));

                if (OptimizedShader)
                    ShaderName = $"Hidden/{Shader.ShaderPath}-g-{Guid}";
                else if (AreVariantsHidden && !string.IsNullOrEmpty(VariantName))
                    ShaderName = $"Hidden/{Shader.ShaderPath}-v{VariantName}";
                else
                    ShaderName = $"{Shader.ShaderPath}{VariantName}";

                ShaderFile.AppendLine($"Shader \"{ShaderName}\"");

                ShaderFile.AppendLine("{");

                ShaderFile.Append(string.IsNullOrEmpty(PropertiesBlock) ? GetPropertiesBlock(Shader, _modules, FreshAssets, false) : PropertiesBlock);

                WriteShaderSkeleton();

                _functions = new List<ShaderFunction>();
                _reorderedFunctions = new List<ShaderFunction>();
                _modulesByFunctions = new Dictionary<ShaderFunction, ShaderModule>();
                foreach (var module in _modules)
                {
                    _functions.AddRange(module.Functions);
                    foreach (ShaderFunction function in module.Functions)
                    {
                        _modulesByFunctions.Add(function, module);
                    }
                }

                WriteVariablesToKeywords();
                WriteFunctionCallsToKeywords();
                WriteFunctionsToKeywords();

                if (!string.IsNullOrWhiteSpace(Shader.CustomEditor))
                    ShaderFile.AppendLine($"CustomEditor \"{Shader.CustomEditor}\"");
                ShaderFile.AppendLine("}");

                PostGeneration?.Invoke(ShaderFile, this);

                RemoveKeywords();
                HandleIterators();

                if (DisabledSections != null && DisabledSections.Length > 0)
                {
                    var repeatedIds = new HashSet<string>(_modules.GroupBy(m => m.Id).Where(g => g.Count() > 1).Select(g => g.Key));
                    ShaderFile = new StringBuilder(ShaderSectionFilter.Apply(ShaderFile.ToString(), DisabledSections,
                        _modules.Select((m, i) => new { module = m, index = i }).Where(x => repeatedIds.Contains(x.module.Id)).Select(x => x.index)));
                }
                else ShaderFile = new StringBuilder(ShaderSectionFilter.RemoveTemplateAnnotations(ShaderFile.ToString()));

                ShaderFile.Replace("\r\n", "\n");

                ShaderFile = CleanupShaderFile(ShaderFile);
            }

            private void HandleIterators()
            {
                string shader = ShaderFile.ToString();
                ShaderFile = new StringBuilder();

                Dictionary<string, int> iterators = new Dictionary<string, int>();
                MatchCollection m = Regex.Matches(shader, ITERATOR_NAMING);

                int startIdx = 0;
                foreach (Match match in m)
                {
                    ShaderFile.Append(shader.Substring(startIdx, match.Index - startIdx));
                    string idx_id = match.Value;
                    bool iterate = idx_id.Contains("++");
                    idx_id = idx_id.Substring(1, idx_id.Length - (iterate ? 3 : 1));
                    
                    if(!iterators.ContainsKey(idx_id))
                        iterators.Add(idx_id, 0);
                    ShaderFile.Append(iterators[idx_id].ToString());

                    if(iterate)
                        iterators[idx_id]++;

                    startIdx = match.Index + match.Length;
                }
                ShaderFile.Append(shader.Substring(startIdx));
            }

            private void GetLiveUpdateEnablers()
            {
                _liveUpdateEnablers = new List<EnableProperty>();
                var staticEnablers = ActiveEnablers.Keys.ToList();
                foreach (var property in _modules.SelectMany(x => x.EnableProperties))
                {
                    if(property != null && !string.IsNullOrWhiteSpace(property.Name) && !staticEnablers.Contains(property.Name))
                        _liveUpdateEnablers.Add(property);
                }

                _liveUpdateEnablers = _liveUpdateEnablers.Distinct().ToList();
            }

            private void WriteFunctionCallsToKeywords()
            {
                foreach (var startKeyword in _functions.Where(x => x.AppendAfter?.StartsWith("#K#") ?? false).Select(x => x.AppendAfter).Distinct())
                {
                    if (!ShaderFile.Contains(startKeyword)) continue;

                    var callSequence = new StringBuilder();
                    WriteFunctionCallSequence(callSequence, startKeyword);
                    var m = Regex.Matches(ShaderFile.ToString(), $@"{startKeyword}(\s|$)", RegexOptions.Multiline);
                    for (int i = m.Count - 1; i >= 0; i--)
                        ShaderFile.Insert(m[i].Index, callSequence.ToString());
                }
            }

            private void WriteShaderSkeleton()
            {
                ShaderFile.AppendLine("SubShader");
                ShaderFile.AppendLine("{");

                ShaderFile.AppendLine(FreshAssets.GetTemplate(Shader.ShaderTemplate).Template);

                Dictionary<ModuleTemplate, ShaderModule> moduleByTemplate = new Dictionary<ModuleTemplate, ShaderModule>();
                Dictionary<(string, string), string> convertedKeyword = new Dictionary<(string, string), string>();
                int instanceCounter = 0;

                foreach (var module in _modules)
                    foreach (var template in module.Templates)
                        moduleByTemplate.Add(template, module);

                    foreach (var template in _modules.SelectMany(x => x.Templates).OrderBy(x => x.Queue))
                    {
                        var freshTemplate = FreshAssets.GetTemplate(template.Template);
                        var module = moduleByTemplate[template];
                        if (freshTemplate == null) continue;
                        bool hasEnabler = module.EnableProperties.Any(x => x != null && !string.IsNullOrEmpty(x.Name));
                        bool isFilteredIn = hasEnabler && module.EnableProperties.All(x => (x == null || string.IsNullOrEmpty(x.Name)) || ActiveEnablers.TryGetValue(x.Name, out _));
                        bool needsIf = hasEnabler && !isFilteredIn && !template.NeedsVariant;
                        var tmp = new StringBuilder();

                        if (!needsIf)
                        {
                            tmp.AppendLine(freshTemplate.Template);
                        }

                        else
                        {
                            string condition = string.Join(" && ", module.EnableProperties
                                .Where(x => (x != null && !string.IsNullOrEmpty(x.Name)) && !ActiveEnablers.TryGetValue(x.Name, out _))
                                .Select(x => $"{x.Name} == {x.EnableValue}"));
                            tmp.AppendLine($"if({condition})");
                            tmp.AppendLine("{");
                            tmp.AppendLine(freshTemplate.Template);
                            tmp.AppendLine("}");
                        }

                        MatchCollection mki = Regex.Matches(tmp.ToString(), @"#KI#\S*", RegexOptions.Multiline);
                        for (int i = mki.Count - 1; i >= 0; i--)
                        {
                            string newKeyword;
                            if (convertedKeyword.TryGetValue((module.Id, mki[i].Value), out string replacedKeyword))
                            {
                                newKeyword = replacedKeyword;
                            }
                            else
                            {
                                newKeyword = $"{mki[i].Value}{instanceCounter++}";
                                convertedKeyword.Add((module.Id, mki[i].Value), newKeyword);
                            }
                            tmp.Replace(mki[i].Value, newKeyword);
                        }

                        // Temporary provenance survives nested #K# insertion, then is removed before
                        // writing the shader. Indices refer to the full expanded list, never a filtered list.
                        if (DisabledSections != null && DisabledSections.Length > 0)
                        {
                            int moduleIndex = _modules.IndexOf(module);
                            tmp.Insert(0, ShaderSectionFilter.BeginModule(moduleIndex));
                            tmp.Append(ShaderSectionFilter.EndModule(moduleIndex));
                        }

                        foreach (var keyword in template.Keywords.Count == 0 ? new[] { MSSConstants.DEFAULT_CODE_KEYWORD } : template.Keywords.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray())
                        {
                            MatchCollection m = Regex.Matches(ShaderFile.ToString(), $@"#K#{keyword}(\s|$)", RegexOptions.Multiline);
                            for (int i = m.Count - 1; i >= 0; i--)
                                ShaderFile.Insert(m[i].Index, tmp.ToString());

                            if (convertedKeyword.TryGetValue((module.Id, $@"#KI#{keyword}"), out string replacedKeyword))
                            {
                                m = Regex.Matches(ShaderFile.ToString(), $@"{replacedKeyword}(\s|$)", RegexOptions.Multiline);
                                for (int i = m.Count - 1; i >= 0; i--)
                                    ShaderFile.Insert(m[i].Index, tmp.ToString());
                            }
                        }
                    }
                    MatchCollection mkr = Regex.Matches(ShaderFile.ToString(), @"#KI#\S*", RegexOptions.Multiline);
                    for (int i = mkr.Count - 1; i >= 0; i--)
                        ShaderFile.Remove(mkr[i].Index, mkr[i].Length);

                ShaderFile.AppendLine("}");
            }

            private void WriteVariablesToKeywords()
            {
                var variableDeclarations = new Dictionary<string,List<Variable>>();

                foreach (ShaderFunction function in _functions)
                {
                    if (function.VariableKeywords.Count > 0)
                    {
                        foreach (string keyword in function.VariableKeywords)
                        {
                            if (!variableDeclarations.ContainsKey(keyword))
                                variableDeclarations.Add(keyword, new List<Variable>());

                            foreach (Variable variable in function.UsedVariables)
                                variableDeclarations[keyword].Add(variable);
                        }
                    }
                    else
                    {
                        if (!variableDeclarations.ContainsKey(MSSConstants.DEFAULT_VARIABLES_KEYWORD))
                            variableDeclarations.Add(MSSConstants.DEFAULT_VARIABLES_KEYWORD, new List<Variable>());

                        foreach (Variable variable in function.UsedVariables)
                            variableDeclarations[MSSConstants.DEFAULT_VARIABLES_KEYWORD].Add(variable);
                    }
                }

                foreach (var declaration in variableDeclarations)
                {
                    declaration.Value.AddRange(_liveUpdateEnablers.Select(x => x.ToVariable()));
                    var decCode = string.Join("\n", declaration.Value.Distinct().OrderBy(x => x.Type).Select(x => x.GetDefinition())) + "\n\n";
                    MatchCollection m = Regex.Matches(ShaderFile.ToString(), $@"#K#{declaration.Key}\s", RegexOptions.Multiline);
                    for (int i = m.Count - 1; i >= 0; i--)
                        ShaderFile.Insert(m[i].Index, decCode);
                }
            }

            private void WriteFunctionsToKeywords()
            {
                var keywordedCode = new Dictionary<string,(StringBuilder, List<TemplateAsset>)>();

                foreach (ShaderFunction function in _reorderedFunctions)
                {
                    var freshAsset = FreshAssets.GetTemplate(function.ShaderFunctionCode);
                    if (function.CodeKeywords.Count > 0)
                    {
                        foreach (string keyword in function.CodeKeywords)
                        {
                            if (!keywordedCode.ContainsKey(keyword))
                                keywordedCode.Add(keyword, (new StringBuilder(), new List<TemplateAsset>()));

                            if (freshAsset == null) continue;
                            (StringBuilder builder, List<TemplateAsset> assets) = keywordedCode[keyword];
                            if (assets.Contains(freshAsset)) continue;
                            builder.AppendLine(freshAsset.Template);
                            assets.Add(freshAsset);
                        }
                    }
                    else
                    {
                        if (!keywordedCode.ContainsKey(MSSConstants.DEFAULT_CODE_KEYWORD))
                            keywordedCode.Add(MSSConstants.DEFAULT_CODE_KEYWORD, (new StringBuilder(), new List<TemplateAsset>()));

                        if (freshAsset == null) continue;
                        (StringBuilder builder, List<TemplateAsset> assets) = keywordedCode[MSSConstants.DEFAULT_CODE_KEYWORD];
                        if (assets.Contains(freshAsset)) continue;
                        builder.AppendLine(freshAsset.Template);
                        assets.Add(freshAsset);
                    }
                }

                foreach (var code in keywordedCode)
                {
                    MatchCollection m = Regex.Matches(ShaderFile.ToString(), $@"#K#{code.Key}\s", RegexOptions.Multiline);
                    for (int i = m.Count - 1; i >= 0; i--)
                        ShaderFile.Insert(m[i].Index, code.Value.Item1.ToString());
                }
            }

            private void WriteFunctionCallSequence(StringBuilder callSequence, string appendAfter)
            {
                foreach (var function in _functions.Where(x => x.AppendAfter.Equals(appendAfter)).OrderBy(x => x.Queue))
                {
                    _reorderedFunctions.Add(function);
                    ShaderModule module = _modulesByFunctions[function];

                    bool hasEnabler = module.EnableProperties.Any(x => x != null && !string.IsNullOrEmpty(x.Name));
                    bool isFilteredIn = hasEnabler && module.EnableProperties.All(x => (x == null || string.IsNullOrEmpty(x.Name)) || ActiveEnablers.TryGetValue(x.Name, out _));
                    bool needsIf = hasEnabler && !isFilteredIn;

                    if (needsIf)
                    {
                        string condition = string.Join(" && ", module.EnableProperties
                            .Where(x => (x != null && !string.IsNullOrEmpty(x.Name)) && !ActiveEnablers.TryGetValue(x.Name, out _))
                            .Select(x => $"{x.Name} == {x.EnableValue}"));
                        callSequence.AppendLine($"if({condition})");
                        callSequence.AppendLine("{");
                    }

                    callSequence.AppendLine($"{function.Name}();");
                    WriteFunctionCallSequence(callSequence, function.Name);

                    if (needsIf)
                        callSequence.AppendLine("}");
                }
            }

            private void RemoveKeywords()
            {
                int current = 0;

                while (current < ShaderFile.Length)
                {
                    if (ShaderFile.Length >= current + 3 && ShaderFile[current] == '#' && ShaderFile[current + 1] == 'K' &&
                        ShaderFile[current + 2] == '#')
                    {
                        int end = current+3;
                        bool stillToRemove = true;
                        while (end < ShaderFile.Length)
                        {
                            if (char.IsWhiteSpace(ShaderFile[end]))
                            {
                                ShaderFile.Remove(current, end - current);
                                stillToRemove = false;
                                break;
                            }

                            end++;
                        }
                        if(stillToRemove)
                            ShaderFile.Remove(current, end - current);
                    }

                    current++;
                }
            }

            private static bool CheckPropertyBlockLine(StringBuilder builder, StringReader reader, string line, ref int tabs, ref bool deleteEmptyLine)
            {
                string ln = null;
                line = line.Trim();
                if (string.IsNullOrEmpty(line))
                {
                    if (deleteEmptyLine)
                        return false;
                    deleteEmptyLine = true;
                }
                else
                {
                    deleteEmptyLine = false;
                }

                if (line.StartsWith("}") && (ln = reader.ReadLine()) != null && ln.Trim().StartsWith("SubShader"))
                    tabs--;
                builder.AppendLineTabbed(tabs, line);

                if (!string.IsNullOrWhiteSpace(ln))
                    if (CheckPropertyBlockLine(builder, reader, ln, ref tabs, ref deleteEmptyLine))
                        return true;

                if (line.StartsWith("}") && ln != null && ln.Trim().StartsWith("SubShader"))
                    return true;
                return false;
            }

            private static StringBuilder CleanupShaderFile(StringBuilder shaderVariant)
            {
                var finalFile = new StringBuilder();
                using (var sr = new StringReader(shaderVariant.ToString()))
                {
                    string line;
                    int tabs = 0;
                    bool deleteEmptyLine = false;
                    while ((line = sr.ReadLine()) != null)
                    {
                        line = line.Trim();

                        if (string.IsNullOrEmpty(line))
                        {
                            if (deleteEmptyLine)
                                continue;
                            deleteEmptyLine = true;
                        }
                        else
                        {
                            deleteEmptyLine = false;
                        }

                        if (line.StartsWith("Properties"))
                        {
                            finalFile.AppendLineTabbed(tabs, line);
                            string ln = sr.ReadLine()?.Trim();      // When the previous line is the one containing "Properties" we always know
                            finalFile.AppendLineTabbed(tabs, ln);   // that the next line is "{" so we just write it down before increasing the tabs
                            tabs++;
                            while ((ln = sr.ReadLine()) != null)    // we should be escaping this loop way before actually meeting the condition, but you never know
                            {
                                if (CheckPropertyBlockLine(finalFile, sr, ln, ref tabs, ref deleteEmptyLine))
                                    break;
                            }
                            continue;
                        }
                        int openingId = line.IndexOf('{');
                        int closingId = line.IndexOf('}');
                        bool hasOpening = openingId != -1;
                        bool hasClosing = closingId != -1;
                        if (!line.StartsWith("//", StringComparison.Ordinal) && hasClosing && !hasOpening)
                            tabs--;

                        if (hasClosing && hasOpening && closingId < openingId)
                        {
                            finalFile.AppendLineTabbed(tabs - 1, line);
                        }
                        else
                        {
                            finalFile.AppendLineTabbed(tabs, line);
                        }

                        if (!line.StartsWith("//", StringComparison.Ordinal) && hasOpening && !hasClosing)
                            tabs++;
                    }
                }

                return finalFile;
            }
        }

        private static string GetPropertiesBlock(ModularShader shader, List<ShaderModule> modules, Dictionary<TemplateAsset, TemplateAsset> freshAssets, bool includeEnablers = true)
        {
            var block = new StringBuilder();
            block.AppendLine("Properties");
            block.AppendLine("{");

            if (shader.UseTemplatesForProperties)
            {
                var freshTemplate = freshAssets.GetTemplate(shader.ShaderPropertiesTemplate);
                if (freshTemplate != null)
                    block.AppendLine(freshTemplate.Template);

                block.AppendLine($"#K#{MSSConstants.TEMPLATE_PROPERTIES_KEYWORD}");
            }
            else
            {
                List<Property> properties = new List<Property>();

                properties.AddRange(shader.Properties.Where(x => !string.IsNullOrWhiteSpace(x.Name) || x.Attributes.Count > 0));

                foreach (var module in modules.Where(x => x != null))
                {
                    properties.AddRange(module.Properties.Where(x => !string.IsNullOrWhiteSpace(x.Name) || x.Attributes.Count > 0));
                    if (module.EnableProperties.Count > 0 && includeEnablers)
                        properties.AddRange(module.EnableProperties.Where(x => !string.IsNullOrWhiteSpace(x.Name)));

                }

                foreach (var prop in properties.Distinct())
                {
                    if (string.IsNullOrWhiteSpace(prop.Type) && !string.IsNullOrWhiteSpace(prop.Name))
                    {
                        prop.Type = "Float";
                        prop.DefaultValue = "0.0";
                    }

                    string attributes = prop.Attributes.Count == 0 ? "" : $"[{string.Join("][", prop.Attributes)}]";
                    block.AppendLine(string.IsNullOrWhiteSpace(prop.Name) ? attributes : $"{attributes} {prop.Name}(\"{prop.DisplayName}\", {prop.Type}) = {prop.DefaultValue}");
                }
            }

            block.AppendLine("}");
            return block.ToString();
        }

        public static void ApplyDefaultTextures(List<ShaderContext> contexts)
        {
            foreach (var context in contexts)
            {
                var importedShader = AssetImporter.GetAtPath($"{context.FilePath}/" + context.VariantFileName) as ShaderImporter;
                var customTextures = context.Modules.SelectMany(x => x.Properties).Where(x => x.DefaultTextureAsset != null).ToList();
                customTextures.AddRange(context.Shader.Properties.Where(x => x.DefaultTextureAsset != null).ToList());
                if (importedShader != null)
                {
                    importedShader.SetDefaultTextures(customTextures.Select(x => x.Name).ToArray(), customTextures.Select(x => x.DefaultTextureAsset).ToArray());
                    importedShader.SetNonModifiableTextures(customTextures.Select(x => x.Name).ToArray(), customTextures.Select(x => x.DefaultTextureAsset).ToArray());
                }
                AssetDatabase.ImportAsset($"{context.FilePath}/" + context.VariantFileName);
            }
        }

        public static List<ShaderModule> FindAllModules(ModularShader shader)
        {
            List<ShaderModule> modules = new List<ShaderModule>();
            if (shader == null) return modules;
            HashSet<string> collectionIDs = new HashSet<string>();

            FindModules(shader.BaseModules.Where(x => x != null), modules, collectionIDs);
            FindModules(shader.AdditionalModules.Where(x => x != null), modules, collectionIDs);
            return modules;
        }

        public static void FindModules(IEnumerable<ShaderModule> modules, List<ShaderModule> output, HashSet<string> collectionIDs)
        {
            foreach (var module in modules)
            {
                if (module == null) continue;
                switch (module)
                {
                    case ModuleCollection collection:
                        if (collectionIDs.Contains(collection.Id)) continue;
                        collectionIDs.Add(collection.Id);
                        FindModules(collection.Modules, output, collectionIDs);
                        break;
                    default: output.Add(module);
                        break;
                }
            }
        }



        struct IdReplacement
        {
            public string IndexKeyword;
            public int StartIndex;
            public bool SkipOriginal;
        }

        static Dictionary<string,ShaderModule[]> GenerateDuplicates(List<ShaderModule> modules)
        {
            Dictionary<string,ShaderModule[]> generatedDuplicates = new Dictionary<string, ShaderModule[]>();
            var duplicates = modules.GroupBy(x => x.Id).Where(x => x.Count() > 1 || x.First().ForceDuplicateLogic).Select(x => x.Key).ToList();
            foreach (var duplicateId in duplicates)
            {
                var dupeIndices = modules.Select((x, i) => (x, i)).Where(x => x.x.Id.Equals(duplicateId)).Select(x => x.i).ToList();

                ShaderModule[] moduleInstances = new ShaderModule[dupeIndices.Count];

                moduleInstances[0] = modules[dupeIndices[0]].DeepCopy();
                string allCode = string.Join("\n", moduleInstances[0].Templates.Select(x => x.Template.Template));
                if (moduleInstances.Length > 1)
                {
                    // Remove structs from duplicate code
                    moduleInstances[1] = moduleInstances[0].DeepCopy();
                    foreach (var template in moduleInstances[1].Templates)
                        template.Template.Template = RemoveStructs(template.Template.Template);
                }

                // Code that needs to differ between first and other instances
                string[] codeSectionKeywords = { "<ms_include_in_first>", "<ms_include_in_not_first>", "<ms_include_in_middle>", "<ms_include_in_last>" };
                
                if(moduleInstances.Length == 1)
                    RemoveCodeSections(moduleInstances[0], new bool[] { true, false, true, true });
                else
                    RemoveCodeSections(moduleInstances[0], new bool[] { true, false, false, false });
                
                if (moduleInstances.Length == 2)
                {
                    RemoveCodeSections(moduleInstances[1], new bool[] { false, true, true, true });
                }
                else if (moduleInstances.Length > 2)
                {
                    moduleInstances[moduleInstances.Length - 1] = moduleInstances[1].DeepCopy();
                    RemoveCodeSections(moduleInstances[1], new bool[] { false, true, true, false });
                    RemoveCodeSections(moduleInstances[moduleInstances.Length - 1], new bool[] { false, true, false, true });
                }

                for (int i = 2; i < moduleInstances.Length - 1; i++)
                    moduleInstances[i] = moduleInstances[1].DeepCopy();

                string[] automaticSuffixes = new string[moduleInstances.Length];
                automaticSuffixes[0] = "";
                for (int i = 1; i < moduleInstances.Length; i++)
                    automaticSuffixes[i] = $"__{i}";
                void RemoveCodeSections(ShaderModule module, bool[] includeSection)
                {
                    foreach (var template in module.Templates)
                    {
                        string code = template.Template.Template;
                        StringBuilder sb = new StringBuilder();
                        int codeIndex = 0;
                        do
                        {
                            int msIncludeIndex = code.IndexOf("<ms_include_in_", codeIndex, StringComparison.OrdinalIgnoreCase);
                            if (msIncludeIndex == -1) break;
                            int endIndex = code.IndexOf(">", msIncludeIndex, StringComparison.OrdinalIgnoreCase);
                            if (endIndex == -1) break;
                            string sectionName = code.Substring(msIncludeIndex, endIndex - msIncludeIndex + 1);
                            int sectionIndex = Array.IndexOf(codeSectionKeywords, sectionName);
                            
                            // Find section start and end
                            int sectionStart = code.IndexOf("{", endIndex + 1, StringComparison.OrdinalIgnoreCase);
                            int sectionEnd = sectionStart;
                            int depth = 0;
                            while (sectionEnd < code.Length)
                            {
                                if (code[sectionEnd] == '{')
                                    depth++;
                                else if (code[sectionEnd] == '}')
                                    depth--;
                                if (depth == 0)
                                    break;
                                sectionEnd++;
                            }
                            if (depth != 0) break; // Unmatched braces, exit loop

                            // include previous code
                            sb.Append(code.Substring(codeIndex, msIncludeIndex - codeIndex));
                            codeIndex = sectionEnd + 1; // Move past the closing brace

                            // include or remove code section
                            if (sectionIndex != -1 && includeSection[sectionIndex])
                            {
                                sb.Append(code.Substring(sectionStart + 1, sectionEnd - sectionStart - 1));
                            }
                            else
                            {
                                // Remove code section
                                if(code[codeIndex] == '\r')
                                    codeIndex += 2;
                                if(code[sectionEnd + 1] == '\n')
                                    codeIndex += 1;
                            }
                            
                        }while(true);
                        sb.Append(code.Substring(codeIndex));
                        template.Template.Template = sb.ToString();
                    }
                }
                
                string RemoveStructs(string code)
                {
                    int i = 0;
                    while((i = code.IndexOf("struct", i + 1, StringComparison.Ordinal)) != -1)
                    {
                        int depth = 0;
                        int checkJ = code.IndexOf(';', i);
                        int j = code.IndexOf('{', i);

                        // check if struct definition or use
                        if(checkJ < j || j == -1)
                            continue;

                        while(j < code.Length)
                        {
                            if(code[j] == '{')
                                depth++;
                            else if(code[j] == '}')
                                depth--;
                            if(depth == 0)
                                break;
                            j++;
                        }
                        // find line end
                        while(j < code.Length && code[j] != '\n')
                            j++;
                        if(j < code.Length)
                        {
                            code = code.Remove(i, j - i + 1);
                        }
                    }
                    return code;
                }

                // Find all property declarations
                MatchCollection matches = Regex.Matches(allCode, @"\w+(?=\s*\("")");
                IEnumerable<string> properties = matches.Cast<Match>().Select(x => x.Value).Distinct();
                matches = Regex.Matches(allCode, @"\w+(?=\s*\("".*2D)");
                IEnumerable<string> textures = matches.Cast<Match>().Select(x => x.Value).Distinct();
            
                matches = Regex.Matches(allCode, @"^( |\t)*(inline +)?\w+ +\w+(?=\s*\((\w|\s|,)*\)\s*{)", RegexOptions.Multiline);
                IEnumerable<string> functionNames = matches.Cast<Match>().Select(x => x.Value.Split(' ').Last()).Distinct();
                matches = Regex.Matches(allCode, @"(?<=#pragma shader_feature_local )\w+"); // Local Keywords
                IEnumerable<string> keywords = matches.Cast<Match>().Select(x => x.Value).Distinct();
                matches = Regex.Matches(allCode, @"(?<=#pragma shader_feature )\w+"); // Global Keywords
                keywords = keywords.Concat(matches.Cast<Match>().Select(x => x.Value).Distinct());
                matches = Regex.Matches(allCode, @"PROP_\w+"); // Shader Optimizer Texture Keywords
                keywords = keywords.Concat(matches.Cast<Match>().Select(x => x.Value).Distinct());

                matches = Regex.Matches(allCode, @"^( |\t)*\w+\s+\w+(?=\s*(=|;))(?![^;]*;\/\/<ms_no_postfix>)", RegexOptions.Multiline);
                IEnumerable<string> localVariables = matches.Cast<Match>().Select(x => x.Value.Split(' ').Last()).Distinct();
                localVariables = localVariables.Where(x => !properties.Contains(x) && !textures.Contains(x) && !functionNames.Contains(x) && !keywords.Contains(x));

                MatchCollection Find(string input, string keyword)
                {
                    return Regex.Matches(input, @"(?<!(\w|\.))" + keyword + @"(?!\w)");
                }
                string Replace(string input, string keyword, string newKeyword)
                {
                    int offset = 0;
                    foreach (Match match in Find(input, keyword))
                    {
                        input = input.Remove(match.Index + offset, keyword.Length).Insert(match.Index + offset, newKeyword);
                        offset += newKeyword.Length - keyword.Length;
                    }
                    return input;
                }

                void ReplaceInModule(ShaderModule module, string keyword, string replacement)
                {
                    foreach (var template in module.Templates)
                    {
                        template.Template.Template = Replace(template.Template.Template, keyword, replacement);
                    }
                }

                void ReplaceCustomKeywordId(string keyword, IdReplacement idReplacement)
                {
                    for(int i=0; i < moduleInstances.Length; i++)
                    {
                        string newKeyword;
                        if(idReplacement.SkipOriginal && i == 0)
                        {
                            #if UNITY_2021_1_OR_NEWER
                            newKeyword = keyword.Replace(idReplacement.IndexKeyword, "", StringComparison.OrdinalIgnoreCase);
                            #else
                            newKeyword = keyword.Replace(idReplacement.IndexKeyword, "");
                            #endif
                        }
                        else
                        {
                            #if UNITY_2021_1_OR_NEWER
                            newKeyword = keyword.Replace(idReplacement.IndexKeyword, (i + idReplacement.StartIndex).ToString(), StringComparison.OrdinalIgnoreCase);
                            #else
                            newKeyword = keyword.Replace(idReplacement.IndexKeyword, (i + idReplacement.StartIndex).ToString());
                            #endif
                        }

                        ReplaceInModule(moduleInstances[i], keyword, newKeyword);
                    }
                }

                void ReplaceAutomaticKeywordId(string matchPhrase, string keywordIn, Func<int, string, string> replacer)
                {
                    for(int i=1; i < moduleInstances.Length; i++)
                    {
                        ReplaceInModule(moduleInstances[i], matchPhrase, replacer(i, keywordIn));
                    }
                }
                
                // <id0> -> start at 0
                // <id1> -> start at 1
                // <id0_sO> -> start at 0, replace original with empty
                // <id1_sO> -> start at 1, replace original with empty
                IdReplacement[] idReplacementTypes = {
                    new IdReplacement() { IndexKeyword = "__mID__", StartIndex = 0, SkipOriginal = false },
                    new IdReplacement() { IndexKeyword = "__mID1__", StartIndex = 1, SkipOriginal = false },
                    new IdReplacement() { IndexKeyword = "__mIDx__", StartIndex = 0, SkipOriginal = true },
                    new IdReplacement() { IndexKeyword = "__mID1x__", StartIndex = 1, SkipOriginal = true }
                };

                void MakeUnique(IEnumerable<string> inputKeywords, Func<string,string> keywordTransformer, Func<int, string, string> replacer)
                {
                    foreach(var keywordIn in inputKeywords)
                    {
                        string matchPhrase = keywordTransformer == null ? keywordIn : keywordTransformer(keywordIn);
                        bool hasSpecifiedIdentifier = false;
                        
                        foreach (IdReplacement idReplacement in idReplacementTypes)
                        {
                            if (matchPhrase.IndexOf(idReplacement.IndexKeyword, StringComparison.OrdinalIgnoreCase) != -1)
                            {
                                ReplaceCustomKeywordId(matchPhrase, idReplacement);
                                hasSpecifiedIdentifier = true;
                                break;
                            }
                        }

                        if(!hasSpecifiedIdentifier)
                        {
                            ReplaceAutomaticKeywordId(matchPhrase, keywordIn, replacer);
                        }

                        
                    }
                }

                MakeUnique(properties, null,                      (i, x) => x + automaticSuffixes[i]);
                MakeUnique(textures,   x => x + "_ST",            (i, x) => x + automaticSuffixes[i] + "_ST");
                MakeUnique(textures,   x => x + "_TexelSize",     (i, x) => x + automaticSuffixes[i] + "_TexelSize");
                // I think this is already covered by the 'keywords' replacement
                // MakeUnique(textures,   x => "PROP" + x.ToUpper(), (i, x) => "PROP" + x.ToUpper() + automaticSuffixes[i].ToUpper());

                MakeUnique(functionNames,  null, (i, x) => x + automaticSuffixes[i]);
                MakeUnique(keywords,       null, (i, x) => x + automaticSuffixes[i].ToUpper());
                MakeUnique(localVariables, null, (i, x) => x + automaticSuffixes[i]);

                // StringBuilders
                Dictionary<string, StringBuilder> stringBuilders = new Dictionary<string, StringBuilder>();
                foreach(ShaderModule module in moduleInstances)
                {
                    foreach(var template in module.Templates)
                    {
                        int index=0;

                        while((index = template.Template.Template.IndexOf("<ms_stringbuilder::", index, StringComparison.OrdinalIgnoreCase)) != -1)
                        {
                            int idEnd = template.Template.Template.IndexOf(">", index, StringComparison.OrdinalIgnoreCase);
                            int lineEnd = template.Template.Template.IndexOf("\n", index, StringComparison.OrdinalIgnoreCase);
                            string id = template.Template.Template.Substring(index + 19, idEnd - index - 19);
                            string data = template.Template.Template.Substring(idEnd + 1, lineEnd - idEnd - 2).TrimEnd();
                            if(stringBuilders.TryGetValue(id, out StringBuilder sb))
                            {
                                sb.Append(data);
                            }
                            else
                            {
                                sb = new StringBuilder(data);
                                stringBuilders.Add(id, sb);
                            }
                            template.Template.Template = template.Template.Template.Remove(index, lineEnd - index + 1);
                        }
                    }
                }

                // String Writers
                foreach(var sb in stringBuilders)
                {
                    foreach(ShaderModule module in moduleInstances)
                    {
                        foreach(var template in module.Templates)
                        {
                            int index=-1;
                            while((index = template.Template.Template.IndexOf($"<ms_stringwriter::{sb.Key}>", index + 1, StringComparison.OrdinalIgnoreCase)) != -1)
                            {
                                template.Template.Template = template.Template.Template.Remove(index, sb.Key.Length + 19).Insert(index, sb.Value.ToString());
                            }
                        }
                    }
                }

                generatedDuplicates.Add(duplicateId, moduleInstances);
            }

            return generatedDuplicates;
        }

        static void ApplyDupliactes(List<ShaderModule> modules, Dictionary<string, ShaderModule[]> duplicates)
        {
            foreach (var duplicate in duplicates)
            {
                var dupeIndices = modules.Select((x, i) => (x, i)).Where(x => x.x.Id.Equals(duplicate.Key)).Select(x => x.i).ToList();
                for (int i = 0; i < dupeIndices.Count; i++)
                {
                    modules[dupeIndices[i]] = duplicate.Value[i];
                }
            }
        }

        public static List<Property> FindAllProperties(ModularShader shader)
        {
            List<Property> properties = new List<Property>();
            if (shader == null) return properties;

            properties.AddRange(shader.Properties.Where(x => !string.IsNullOrWhiteSpace(x.Name) || x.Attributes.Count == 0));

            foreach (var module in shader.BaseModules.Where(x => x != null))
            {
                properties.AddRange(module.Properties.Where(x => !string.IsNullOrWhiteSpace(x.Name) || x.Attributes.Count == 0));
                if (module.EnableProperties.Count > 0)
                    properties.AddRange(module.EnableProperties.Where(x => !string.IsNullOrWhiteSpace(x.Name)));
            }

            foreach (var module in shader.AdditionalModules.Where(x => x != null))
            {
                properties.AddRange(module.Properties.Where(x => !string.IsNullOrWhiteSpace(x.Name) || x.Attributes.Count == 0));
                if (module.EnableProperties.Count > 0)
                    properties.AddRange(module.EnableProperties.Where(x => !string.IsNullOrWhiteSpace(x.Name)));
            }

            return properties.Distinct().ToList();
        }

        public static List<ShaderFunction> FindAllFunctions(ModularShader shader)
        {
            var functions = new List<ShaderFunction>();
            if (shader == null) return functions;
            foreach (var module in shader.BaseModules)
                functions.AddRange(module.Functions);

            foreach (var module in shader.AdditionalModules)
                functions.AddRange(module.Functions);
            return functions;
        }

        public static List<ShaderModule> FindActiveModules(ModularShader shader, Dictionary<string, int> activeEnablers, Dictionary<string, ShaderModule[]> duplicatedModules)
        {
            List<ShaderModule> modules = new List<ShaderModule>();
            if (shader == null) return modules;
            HashSet<string> collectionIDs = new HashSet<string>();

            FindActiveModules(shader.BaseModules, activeEnablers, modules, collectionIDs, shader, true);
            FindActiveModules(shader.AdditionalModules, activeEnablers, modules, collectionIDs, shader, false);

            if(duplicatedModules != null)
                ApplyDupliactes(modules, duplicatedModules);

            return modules.Distinct().ToList();
        }

		private static void FindActiveModules(IEnumerable<ShaderModule> modules, Dictionary<string, int> activeEnablers, List<ShaderModule> output, HashSet<string> collectionIDs, ModularShader shader, bool isBaseModules)
		{
			var modulesList = modules.ToList();
			List<bool> shaderToggles = null;
			if (isBaseModules && shader != null)
				shaderToggles = ModuleTogglePreferences.GetToggles(shader, modulesList.Count);
			
			for (int i = 0; i < modulesList.Count; i++)
			{
				var module = modulesList[i];
				if (module == null) continue;
				
				if (shaderToggles != null && i < shaderToggles.Count && !shaderToggles[i])
					continue;
				
				if (module is ModuleCollection collection)
				{
					if (collectionIDs.Contains(collection.Id)) continue;
					collectionIDs.Add(collection.Id);
					var collectionModulesList = collection.Modules.ToList();
					var collectionToggles = ModuleTogglePreferences.GetToggles(collection, collectionModulesList.Count);
					var filteredModules = new List<ShaderModule>();
					for (int j = 0; j < collectionModulesList.Count; j++)
					{
						if (j < collectionToggles.Count && !collectionToggles[j])
							continue;
						filteredModules.Add(collectionModulesList[j]);
					}
					FindActiveModules(filteredModules, activeEnablers, output, collectionIDs, shader, false);
					continue;
				}
                bool hasEnabler = module.EnableProperties.Any(x => x != null && !string.IsNullOrEmpty(x.Name));
                bool hasKey = hasEnabler && module.EnableProperties.Any(x => activeEnablers.TryGetValue(x.Name, out _));
                if (!hasEnabler || !hasKey || (module.EnableProperties.All(x =>
                    {
                        if (x.Name == null || string.IsNullOrEmpty(x.Name)) return true;
                        if (!activeEnablers.TryGetValue(x.Name, out int value)) return true;
                        return x.EnableValue == value;
                    })))
                    output.Add(module);
            }
        }

        public static List<string> CheckShaderIssues(ModularShader shader)
        {
            List<string> errors = new List<string>();
            var modules = FindAllModules(shader);

            for (int i = 0; i < modules.Count; i++)
            {
                var dependencies = new List<string>(modules[i].ModuleDependencies);
                for (int j = 0; j < modules.Count; j++)
                {
                    if (modules[j].IncompatibleWith.Any(x => x.Equals(modules[i].Id)))
                        errors.Add($"Module \"{modules[j].Name}\" ({modules[j].Id}) is incompatible with module \"{modules[i].name}\" ({modules[i].Id}).");

                    if (i != j && modules[i].Id.Equals(modules[j].Id) && !modules[i].AllowDuplicates)
                        errors.Add($"Module \"{modules[i].Name}\" ({modules[i].Id}) is duplicate.");

                    if (modules[i] is CibbiExtensions.ModuleCollection)
                    {
                        foreach (ShaderModule module in ((CibbiExtensions.ModuleCollection)modules[i]).Modules)
                        {
                            if (module.Id.Equals(modules[j].Id) && !module.AllowDuplicates)
                                errors.Add($"Module \"{modules[i].Name}\" ({modules[i].Id}) is duplicate in ModuleCollection: {modules[i]?.Name} ({modules[i].Id})");
                        }
                    }

                    if (dependencies.Contains(modules[j].Id))
                        dependencies.Remove(modules[j].Id);
                }
                foreach (string t in dependencies)
                    errors.Add($"Module \"{modules[i].Name}\" ({modules[i].Id}) has missing dependency id \"{t}\".");
            }
            return errors;
        }

        public static List<string> CheckShaderIssues(List<ShaderModule> modules)
        {
            List<string> errors = new List<string>();

            for (int i = 0; i < modules.Count; i++)
            {
                var dependencies = new List<string>(modules[i].ModuleDependencies);
                for (int j = 0; j < modules.Count; j++)
                {
                    if (modules[j].IncompatibleWith.Any(x => x.Equals(modules[i].Id)))
                        errors.Add($"Module \"{modules[j].Name}\" ({modules[j].Id}) is incompatible with module \"{modules[i].name}\" ({modules[i].Id}).");

                    if (i != j && modules[i].Id.Equals(modules[j].Id))
                        errors.Add($"Module \"{modules[i].Name}\" ({modules[i].Id}) is duplicate.");

                    if (dependencies.Contains(modules[j].Id))
                        dependencies.Remove(modules[j].Id);
                }
                foreach (string t in dependencies)
                    errors.Add($"Module \"{modules[i].Name}\" ({modules[i].Id}) has missing dependency id \"{t}\".");
            }
            return errors;
        }
    }
}
