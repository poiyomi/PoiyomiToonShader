using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Thry
{
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

        public static void RemoveDefineSymbols()
        {
            UnityHelper.SetDefineSymbol(DEFINE_SYMBOLS.IMAGING_EXISTS, false);
        }
    }
}