using UnityEditor;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace Poi
{
    public class PoiExportPackage : Editor
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
                EditorUtility.DisplayDialog("Poi Export, File already exists", $"{fileName} already exists, did you forget to change the version number?", "Yes");
                return;
            }
            var guids = AssetDatabase.FindAssets("t:Object", new[] {"Assets/_PoiyomiShaders"});
            var filesToExport = guids.Select(x => AssetDatabase.GUIDToAssetPath(x)).ToList();
            foreach (var fileToRemove in filesToRemove)
            {
                filesToExport.RemoveAll(x => x.StartsWith(fileToRemove));
            }
            AssetDatabase.ExportPackage(filesToExport.ToArray(), fileName);
            EditorUtility.DisplayDialog("Poi Export, Package exported", $"Package exported to {Path.GetFullPath(fileName)}", "Continue");
        }
    }
}
