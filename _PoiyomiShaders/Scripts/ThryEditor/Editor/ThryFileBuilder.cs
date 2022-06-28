using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Thry {
    public class ThryFileCreator {

        [MenuItem("Thry/ShaderUI/UI Creator Helper/Create Label Boiler", false, priority = 40)]
        public static void CreateLabel()
        {
            string[] names = GetProperties();
            string data = names.Aggregate("", (n1, n2) => n1 + n2 + ":=" + n2 + "--{tooltip:}\n");
            Save(data, "_label.txt");
        }
        [MenuItem("Thry/ShaderUI/UI Creator Helper/Create Label Boiler", true, priority = 40)]
        static bool CreateLabelVaildate()
        {
            return ValidateSelection();
        }

        [MenuItem("Thry/ShaderUI/UI Creator Helper/Create Label Boiler + Locale Boiler", false, priority = 40)]
        public static void CreateLabelLocale()
        {
            string[] names = GetProperties();
            string label_data = names.Aggregate("", (n1, n2) => n1 + 
            n2 + ":=locale::" + n2 + "_text--{tooltip:locale::" + n2 + "_tooltip}\n");
            string locale_data = names.Aggregate(",English\n", (n1, n2) => n1 +
            n2 + "_text," + n2 + "\n"+
            n2 + "_tooltip,\n");
            Save(label_data, "_label.txt");
            Save(locale_data, "_locale.txt");
        }
        [MenuItem("Thry/ShaderUI Creator Helper/Create Label Boiler + Locale Boiler", true, priority = 40)]
        static bool CreateLabelLocaleValidate()
        {
            return ValidateSelection();
        }

        private static bool ValidateSelection()
        {
            if (Selection.activeObject == null)
                return false;
            string path = AssetDatabase.GetAssetPath(Selection.activeObject).ToLower();
            return path.EndsWith(".shader");
        }

        private static string[] GetProperties()
        {
            Shader shader = (Shader)Selection.activeObject;
            return MaterialEditor.GetMaterialProperties(new Material[] { new Material(shader) }).Select(p => p.name).ToArray();
        }

        private static void Save(string data, string add_string)
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            path = Path.GetDirectoryName(path)+ "/"+ Path.GetFileNameWithoutExtension(path) + add_string;
            FileHelper.WriteStringToFile(data, path);
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(path));
        }
    }
}