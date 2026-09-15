using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using UnityEngine;

namespace Poiyomi.ModularShaderSystem
{
    [ScriptedImporter(1, MSSConstants.TEMPLATE_COLLECTION_EXTENSION)]
    public class TemplateColletionAssetImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var subAsset = ScriptableObject.CreateInstance<TemplateCollectionAsset>();
            

            
            using (var sr = new StringReader(File.ReadAllText(ctx.assetPath)))
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