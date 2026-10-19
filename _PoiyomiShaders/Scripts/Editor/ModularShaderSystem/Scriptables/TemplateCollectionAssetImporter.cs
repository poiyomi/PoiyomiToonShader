using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using UnityEngine;

namespace Poiyomi.ModularShaderSystem
{
    [ScriptedImporter(3, MSSConstants.TEMPLATE_COLLECTION_EXTENSION)]
    public class TemplateColletionAssetImporter : ScriptedImporter
    {
        // Source references also retain the files during package export. Import them first so
        // those references are available on the very first import of a new collection.
        private static string[] GatherDependenciesFromSourceFile(string path)
        {
            var dependencies = new HashSet<string>();
            string ownerPath = Path.GetFullPath(path);
            TemplateSourceIncludes.Expand(path, dependency =>
            {
                if (dependency != ownerPath) dependencies.Add(ProjectRelativePath(dependency));
            });
            return dependencies.OrderBy(p => p, StringComparer.Ordinal).ToArray();
        }

        private static string ProjectRelativePath(string path)
        {
            string projectRoot = Path.GetFullPath(".").TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!path.StartsWith(projectRoot, StringComparison.Ordinal))
                throw new InvalidDataException("Template source includes must stay inside the Unity project: " + path);
            return path.Substring(projectRoot.Length).Replace('\\', '/');
        }

        public override void OnImportAsset(AssetImportContext ctx)
        {
            // Collections hold both types; register both dependencies before bailing out.
            bool bound = TemplateAssetImporter.RequireScript<TemplateCollectionAsset>(ctx, MSSConstants.TEMPLATE_COLLECTION_ASSET_SCRIPT_GUID);
            bound &= TemplateAssetImporter.RequireScript<TemplateAsset>(ctx, MSSConstants.TEMPLATE_ASSET_SCRIPT_GUID);
            if (!bound) return;
            
            // Resolve source fragments before creating subassets: queues, names/file IDs, keyword
            // discovery, duplication, locking and section provenance see the original complete template.
            string ownerPath = Path.GetFullPath(ctx.assetPath);
            var dependencies = new HashSet<string>();
            string source = TemplateSourceIncludes.Expand(ctx.assetPath, dependency =>
            {
                // The owner already reimports on its own changes; Unity disallows a dependency on itself.
                if (dependency == ownerPath) return;
                string path = ProjectRelativePath(dependency);
                ctx.DependsOnSourceAsset(path);
                dependencies.Add(path);
            });
            var subAsset = ScriptableObject.CreateInstance<TemplateCollectionAsset>();
            subAsset.SourceIncludes = dependencies.OrderBy(p => p, StringComparer.Ordinal)
                .Select(AssetDatabase.LoadAssetAtPath<TemplateCollectionAsset>).ToList();
            using (var sr = new StringReader(source))
            {
                var builder = new StringBuilder();
                string line;
                string name = "";
                bool deleteEmptyLine = false;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains("#T#"))
                    {
                        if (builder.Length > 0 && !string.IsNullOrWhiteSpace(name))
                            SaveSubAsset(ctx, subAsset, builder, name);
                        
                        builder = new StringBuilder();
                        name = line.Replace("#T#", "").Trim();
                        continue;
                    }

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

                    builder.AppendLine(line);
                }
                
                if (builder.Length > 0 && !string.IsNullOrWhiteSpace(name))
                    SaveSubAsset(ctx, subAsset, builder, name);
            }
            
            ctx.AddObjectToAsset("Collection", subAsset);
            ctx.SetMainObject(subAsset);
        }

        private static void SaveSubAsset(AssetImportContext ctx, TemplateCollectionAsset asset, StringBuilder builder, string name)
        {
            var templateAsset = ScriptableObject.CreateInstance<TemplateAsset>();
            templateAsset.Template = builder.ToString();
            templateAsset.name = name;
            
            MatchCollection mk = Regex.Matches(templateAsset.Template, @"#K#\w*", RegexOptions.Multiline);
            MatchCollection mki = Regex.Matches(templateAsset.Template, @"#KI#\w*", RegexOptions.Multiline);

            var mkr = new string[mk.Count + mki.Count]; 
            for (var i = 0; i < mk.Count; i++)
                mkr[i] = mk[i].Value;
            for (var i = 0; i < mki.Count; i++)
                mkr[mk.Count + i] = mki[i].Value;

            templateAsset.Keywords = mkr.Distinct().ToArray();
            
            ctx.AddObjectToAsset(name, templateAsset);
            asset.Templates.Add(templateAsset);
        }

        public override bool SupportsRemappedAssetType(Type type)
        {
            return type.IsAssignableFrom(typeof(TemplateAsset));
        }
    }
}
