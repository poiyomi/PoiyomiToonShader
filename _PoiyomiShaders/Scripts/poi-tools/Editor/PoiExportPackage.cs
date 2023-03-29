using UnityEditor;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace Poi.Tools
{
    public class PoiExportPackage : UnityEditor.Editor
    {
        static string pattern = @"\d+(\.\d+)+";
        static string[] filesToRemove = new[] {
            "Assets/_PoiyomiShaders/Scripts/poi-tools/Editor/PoiExportPackage.cs",
        };

        [MenuItem("Poi/Tools/Export")]
        private static void Export()
        {
            ThirdPartyIncluder.mi_ThirdPartyIncluderCleanDestAssets();
            string firstLine = File.ReadLines("Assets/_PoiyomiShaders/ModularShader/Editor/Poi_UIBoilerPlater/VRLT_PropsUIBoilerPlate.poiTemplate").First();
            var reg = new Regex(pattern);
            string version = reg.Match(firstLine).Value;
            string fileName = $"poi_pro_7.3.50_and_{version}.unitypackage";
            if (File.Exists(fileName))
            {
                bool result = EditorUtility.DisplayDialog("Poi Export, File already exists", $"{fileName} already exists, did you forget to change the version number.\nDo you want to override it?", "Yes", "No");
                if (!result) return;
            }
            try
            {
                EditorUtility.DisplayProgressBar("Package Export", "Searching for assets", 1/4.0f);
                var guids = AssetDatabase.FindAssets("t:Object", new[] {"Assets/_PoiyomiShaders"});
                EditorUtility.DisplayProgressBar("Package Export", "Getting their paths", 2/4.0f);
                var filesToExport = guids.Select(x => AssetDatabase.GUIDToAssetPath(x)).ToList();
                EditorUtility.DisplayProgressBar("Package Export", "Removing denylisted files", 3/4.0f);
                foreach (var fileToRemove in filesToRemove)
                {
                    filesToExport.RemoveAll(x => x.StartsWith(fileToRemove));
                }
                EditorUtility.DisplayProgressBar("Package Export", "Exporting Package!", 4/4.0f);
                AssetDatabase.ExportPackage(filesToExport.ToArray(), fileName);
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Poi Export, Package exported", $"Package exported to {Path.GetFullPath(fileName)}", "Continue");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
