using System;
using System.IO;
using System.Linq;
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
    
    [ScriptedImporter(2, MSSConstants.TEMPLATE_EXTENSION)]
    public class TemplateAssetImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            if (!RequireScript<TemplateAsset>(ctx, MSSConstants.TEMPLATE_ASSET_SCRIPT_GUID)) return;

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

        // AddObjectToAsset throws while a script has no class bound (as after an interrupted package
        // update), and Unity keeps a thrown import cached even once the script recovers. Fail without
        // throwing instead, so the dependency on the script's import result brings this asset back.
        internal static bool RequireScript<T>(AssetImportContext ctx, string scriptGuid) where T : ScriptableObject
        {
            var guid = new GUID(scriptGuid);
            ctx.DependsOnArtifact(guid);
            // Unity only registers an artifact dependency that the import actually loads
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(guid));
            bool bound = script != null && script.GetClass() == typeof(T);
            if (!bound)
                ctx.LogImportError($"{typeof(T).Name}.cs has no class bound yet. This imports again once it does.");
            return bound;
        }

        public override bool SupportsRemappedAssetType(Type type)
        {
            return type.IsAssignableFrom(typeof(TemplateAsset));
        }
    }
}