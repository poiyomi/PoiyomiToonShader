using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using UnityEngine;

namespace Poiyomi.ModularShaderSystem
{
    
    [ScriptedImporter(1, MSSConstants.TEMPLATE_EXTENSION)]
    public class TemplateAssetImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var subAsset = ScriptableObject.CreateInstance<TemplateAsset>();
            subAsset.Template = File.ReadAllText(ctx.assetPath);
            
            MatchCollection mk = Regex.Matches(subAsset.Template, @"#K#\w*", RegexOptions.Multiline);
            MatchCollection mki = Regex.Matches(subAsset.Template, @"#KI#\w*", RegexOptions.Multiline);

            var mkr = new string[mk.Count + mki.Count]; 
            for (var i = 0; i < mk.Count; i++)
                mkr[i] = mk[i].Value;
            for (var i = 0; i < mki.Count; i++)
                mkr[mk.Count + i] = mki[i].Value;

            subAsset.Keywords = mkr.Distinct().ToArray();
            
            ctx.AddObjectToAsset("Template", subAsset/*, icon*/);
            ctx.SetMainObject(subAsset);
        }

        public override bool SupportsRemappedAssetType(Type type)
        {
            return type.IsAssignableFrom(typeof(TemplateAsset));
        }
    }
}