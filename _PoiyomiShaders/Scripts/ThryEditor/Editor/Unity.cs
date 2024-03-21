// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class UnityHelper
    {
        [MenuItem("Assets/Thry/Copy GUID")]
        public static void CopyGUID()
        {
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(Selection.activeObject));
            EditorGUIUtility.systemCopyBuffer = guid;
        }

        public static List<string> FindAssetsWithFilename(string filename)
        {
            string[] guids = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(filename));
            return guids.Select(g => AssetDatabase.GUIDToAssetPath(g)).Where(p => p.EndsWith(filename)).ToList();
        }

        public static void SetDefineSymbol(string symbol, bool active)
        {
            SetDefineSymbol(symbol, active, true);
        }

        public static void SetDefineSymbol(string symbol, bool active, bool refresh_if_changed)
        {
            try
            {
                string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                        BuildTargetGroup.Standalone);
                if (!symbols.Contains(symbol) && active)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(
                                  BuildTargetGroup.Standalone, symbols + ";" + symbol);
                    if(refresh_if_changed)
                        AssetDatabase.Refresh();
                }
                else if (symbols.Contains(symbol) && !active)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(
                                  BuildTargetGroup.Standalone, Regex.Replace(symbols, @";?" + @symbol, ""));
                    if(refresh_if_changed)
                        AssetDatabase.Refresh();
                }
            }
            catch (Exception e)
            {
                e.ToString();
            }
        }

        public static void RemoveDefineSymbols()
        {
            UnityHelper.SetDefineSymbol(DEFINE_SYMBOLS.IMAGING_EXISTS, false);
        }

        public static void RepaintEditorWindow<T>() where T : EditorWindow
        {
            EditorWindow window = (EditorWindow)Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
            if (window != null) window.Repaint();
        }

        public static string GetGUID(UnityEngine.Object o)
        {
            return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(o));
        }

        public static WindowType FindOpenEditorWindow<WindowType>() where WindowType : EditorWindow
        {
            WindowType[] windows = Resources.FindObjectsOfTypeAll<WindowType>();
            if (windows != null && windows.Length > 0)
            {
                return windows[0];
            }
            return null;
        }

        public static EditorWindow FindOpenEditorWindow(Type type)
        {
            UnityEngine.Object[] windows = Resources.FindObjectsOfTypeAll(type);
            if (windows != null && windows.Length > 0)
            {
                return windows[0] as EditorWindow;
            }
            return null;
        }

        public static string GetCurrentAssetExplorerFolder()
        {
            if (Selection.activeObject) return "Assets";
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (Directory.Exists(path)) return path;
            else return Path.GetDirectoryName(path);
        }
        
        public static void AddShaderPropertyToSourceCode(string path, string property, string value)
        {
            string shaderCode = FileHelper.ReadFileIntoString(path);
            string pattern = @"Properties.*\n?\s*{";
            RegexOptions options = RegexOptions.Multiline;
            shaderCode = Regex.Replace(shaderCode, pattern, "Properties \r\n  {" + " \r\n      " + property + "=" + value, options);

            FileHelper.WriteStringToFile(shaderCode, path);
        }
    }

    public static class UnityExtensions
    {
        // MaterialProperty extension for setting floats / ints
        public static void SetNumber(this MaterialProperty prop, float value)
        {
#if UNITY_2022_1_OR_NEWER
            if(prop.type == MaterialProperty.PropType.Int)
                prop.intValue = (int)value;
            else
#endif
                prop.floatValue = value;
        }

        public static float GetNumber(this MaterialProperty prop)
        {
#if UNITY_2022_1_OR_NEWER
            if(prop.type == MaterialProperty.PropType.Int)
                return prop.intValue;
            else
#endif  
                return prop.floatValue;
        }

        public static void SetNumber(this Material mat, string name, float value)
        {
#if UNITY_2022_1_OR_NEWER
            MaterialProperty prop = MaterialEditor.GetMaterialProperty(new UnityEngine.Object[] { mat }, name);
            if(prop.type == MaterialProperty.PropType.Int)
                mat.SetInteger(name, (int)value);
            else
#endif
                mat.SetFloat(name, value);
        }
        
        public static float GetNumber(this Material mat, MaterialProperty prop)
        {
#if UNITY_2022_1_OR_NEWER
            if(prop.type == MaterialProperty.PropType.Int)
                return mat.GetInt(prop.name);
            else
#endif
                return mat.GetFloat(prop.name);
        }
    }

    public class UnityFixer
    {
        public const string RSP_DRAWING_DLL_CODE = "\n-r:System.Drawing.dll";
        public const string RSP_DRAWING_DLL_DEFINE_CODE = "\n-define:SYSTEM_DRAWING";
        public const string RSP_DRAWING_DLL_REGEX = @"-r:\s*System\.Drawing\.dll";
        public const string RSP_DRAWING_DLL_DEFINE_REGEX = @"-define:\s*SYSTEM_DRAWING";


#if UNITY_2019_1_OR_NEWER
        public const string RSP_FILENAME = "csc";
#else
        public const string RSP_FILENAME = "mcs";
#endif

        public static void OnAssetDeleteCheckDrawingDLL(string[] deleted_assets)
        {
            foreach (string path in deleted_assets)
            {
                if (path == PATH.RSP_NEEDED_PATH + RSP_FILENAME + ".rsp" || path.EndsWith("/System.Drawing.dll"))
                    UnityHelper.SetDefineSymbol(DEFINE_SYMBOLS.IMAGING_EXISTS, false, true);
            }
        }

        public static void CheckAPICompatibility()
        {
            ApiCompatibilityLevel level = PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone);
            if (level == ApiCompatibilityLevel.NET_2_0_Subset)
                PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_2_0);
        }

        public static void CheckDrawingDll()
        {
            string path = PATH.RSP_NEEDED_PATH + RSP_FILENAME + ".rsp";
            bool refresh = true;
            bool containsDLL = DoesRSPContainDrawingDLL(path);
            bool containsDefine = DoesRSPContainDrawingDLLDefine(path);
            if (!containsDefine && !containsDLL)
            {
                AddDrawingDLLToRSP(path);
                AddDrawingDLLDefineToRSP(path);
            }
            else if (!containsDLL)
                AddDrawingDLLToRSP(path);
            else if (!containsDefine)
                AddDrawingDLLDefineToRSP(path);
            else
                refresh = false;
            if (refresh)
                AssetDatabase.ImportAsset(path);
        }



        private static bool DoesRSPContainDrawingDLL(string rsp_path)
        {
            if (!File.Exists(rsp_path)) return false;
            string rsp_data = FileHelper.ReadFileIntoString(rsp_path);
            return (Regex.Match(rsp_data, RSP_DRAWING_DLL_REGEX).Success);
        }

        private static bool DoesRSPContainDrawingDLLDefine(string rsp_path)
        {
            if (!File.Exists(rsp_path)) return false;
            string rsp_data = FileHelper.ReadFileIntoString(rsp_path);
            return (Regex.Match(rsp_data, RSP_DRAWING_DLL_DEFINE_REGEX).Success);
        }

        private static void AddDrawingDLLToRSP(string rsp_path)
        {
            string rsp_data = FileHelper.ReadFileIntoString(rsp_path);
            rsp_data += RSP_DRAWING_DLL_CODE;
            FileHelper.WriteStringToFile(rsp_data, rsp_path);
        }

        private static void AddDrawingDLLDefineToRSP(string rsp_path)
        {
            string rsp_data = FileHelper.ReadFileIntoString(rsp_path);
            rsp_data += RSP_DRAWING_DLL_DEFINE_CODE;
            FileHelper.WriteStringToFile(rsp_data, rsp_path);
        }
    }

    [InitializeOnLoad]
    public class OnCompileHandler
    {
        static OnCompileHandler()
        {
            //Init Editor Variables with paths
            ShaderEditor.GetShaderEditorDirectoryPath();

            Config.OnCompile();
            TrashHandler.EmptyThryTrash();

            UnityFixer.CheckAPICompatibility(); //check that Net_2.0 is ApiLevel
            UnityFixer.CheckDrawingDll(); //check that drawing.dll is imported
        }
    }

    public class AssetChangeHandler : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (deletedAssets.Length > 0)
                AssetsDeleted(deletedAssets);
        }

        private static void AssetsDeleted(string[] assets)
        {
            UnityFixer.OnAssetDeleteCheckDrawingDLL(assets);
            if (CheckForEditorRemove(assets))
            {
                Debug.Log("[Thry] ShaderEditor is being deleted.");
                Config.Singleton.verion = "0";
                Config.Singleton.Save();
            }
        }

        private static bool CheckForEditorRemove(string[] assets)
        {
            string test_for = ShaderEditor.GetShaderEditorDirectoryPath() + "/Editor/ShaderEditor.cs";
            foreach (string p in assets)
            {
                if (p == test_for)
                    return true;
            }
            return false;
        }
    }
}