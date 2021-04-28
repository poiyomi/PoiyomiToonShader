// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class UnityHelper
    {
        /// <summary>
        /// return null if not found
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string FindPathOfAssetWithExtension(string filename)
        {
            string[] guids = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(filename));
            foreach (string s in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(s);
                if (path.EndsWith(filename))
                    return path;
            }
            return null;
        }

        public static List<string> FindAssetOfFilesWithExtension(string filename)
        {
            List<string> ret = new List<string>();
            string[] guids = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(filename));
            foreach (string s in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(s);
                if (path.EndsWith(filename))
                    ret.Add(path);
            }
            return ret;
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

        public static void RepaintInspector(System.Type t)
        {
            Editor[] ed = (Editor[])Resources.FindObjectsOfTypeAll<Editor>();
            for (int i = 0; i < ed.Length; i++)
            {
                if (ed[i].GetType() == t)
                {
                    ed[i].Repaint();
                    return;
                }
            }
        }

        public static void RepaintEditorWindow(Type t)
        {
            EditorWindow window = FindEditorWindow(t);
            if (window != null) window.Repaint();
        }

        public static EditorWindow FindEditorWindow(System.Type t)
        {
            EditorWindow[] ed = (EditorWindow[])Resources.FindObjectsOfTypeAll<EditorWindow>();
            for (int i = 0; i < ed.Length; i++)
            {
                if (ed[i].GetType() == t)
                {
                    return ed[i];
                }
            }
            return null;
        }

        public static string GetGUID(UnityEngine.Object o)
        {
            return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(o));
        }

        public static int CalculateLengthOfText(string message, Font font= null)
        {
            if (font == null)
                font = GUI.skin.font;
            float totalLength = 0;

            CharacterInfo characterInfo = new CharacterInfo();

            char[] arr = message.ToCharArray();

            foreach (char c in arr)
            {
                font.GetCharacterInfo(c, out characterInfo, font.fontSize);
                totalLength += characterInfo.advance;
            }

            return (int)totalLength;
        }
    }

    public class UnityFixer
    {
        public const string RSP_DRAWING_DLL_CODE = "\n-r:System.Drawing.dll";
        public const string RSP_DRAWING_DLL_DEFINE_CODE = "\n-define:SYSTEM_DRAWING";
        public const string RSP_DRAWING_DLL_REGEX = @"-r:\s*System\.Drawing\.dll";
        public const string RSP_DRAWING_DLL_DEFINE_REGEX = @"-define:\s*SYSTEM_DRAWING";

        public static void OnAssetDeleteCheckDrawingDLL(string[] deleted_assets)
        {
            foreach (string path in deleted_assets)
            {
                if (path == PATH.RSP_NEEDED_PATH + GetRSPFilename() + ".rsp" || path.EndsWith("/System.Drawing.dll"))
                    UnityHelper.SetDefineSymbol(DEFINE_SYMBOLS.IMAGING_EXISTS, false, true);
            }
        }

        public static void CheckAPICompatibility()
        {
            ApiCompatibilityLevel level = PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone);
            if (level == ApiCompatibilityLevel.NET_2_0_Subset)
                PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_2_0);
        }

        private static string GetRSPFilename()
        {
            if (Helper.compareVersions("2018", Application.unityVersion) == 1)
                return "csc";
            return "mcs";
        }

        public static void CheckDrawingDll()
        {
            string filename = GetRSPFilename();
            string path = PATH.RSP_NEEDED_PATH + filename + ".rsp";
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
            ModuleHandler.OnCompile();
            TrashHandler.EmptyThryTrash();

            UnityFixer.CheckAPICompatibility(); //check that Net_2.0 is ApiLevel
            UnityFixer.CheckDrawingDll(); //check that drawing.dll is imported
        }
    }

    public class AssetChangeHandler : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (importedAssets.Length > 0)
                AssetsImported(importedAssets);
            if (deletedAssets.Length > 0)
                AssetsDeleted(deletedAssets);
            if (movedAssets.Length > 0)
                AssetsMoved(movedAssets, movedFromAssetPaths);
        }

        private static void AssetsImported(string[] assets)
        {
            ShaderHelper.AssetsImported(assets);
        }

        private static void AssetsMoved(string[] movedAssets, string[] movedFromAssetPaths)
        {
            ShaderHelper.AssetsMoved(movedFromAssetPaths, movedAssets);
        }

        private static void AssetsDeleted(string[] assets)
        {
            ShaderHelper.AssetsDeleted(assets);
            UnityFixer.OnAssetDeleteCheckDrawingDLL(assets);
            if (CheckForEditorRemove(assets))
            {
                Debug.Log("ShaderEditor is being deleted.");
                Config.Singleton.verion = "0";
                Config.Singleton.save();
                ModuleHandler.OnEditorRemove();
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