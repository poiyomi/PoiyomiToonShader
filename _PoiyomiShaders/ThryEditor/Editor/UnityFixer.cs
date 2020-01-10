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
        public const string RSP_DRAWING_DLL_REGEX = @"-r:\s*System\.Drawing\.dll";

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
            if (Type.GetType("System.Drawing.Image, System.Drawing") == null)
            {
                string filename = GetRSPFilename();
                RSP_State state = CheckRSPState(filename);
                switch (state)
                {
                    case RSP_State.missing:
                    case RSP_State.missing_drawing_dll:
                        AddDrawingDLLToRSP(PATH.RSP_NEEDED_PATH + filename + ".rsp");
                        break;
                }
                UnityFixer.CheckAPICompatibility();
            }
            UnityHelper.SetDefineSymbol(DEFINE_SYMBOLS.IMAGING_EXISTS, true, true);
        }

        private enum RSP_State { correct=2, missing=0, missing_drawing_dll=1};

        private static RSP_State CheckRSPState(string rsp_name)
        {
            string path = PATH.RSP_NEEDED_PATH + rsp_name + ".rsp";
            if (!File.Exists(path))
                return RSP_State.missing;
            else if (!DoesRSPContainDrawingDLL(path))
                return RSP_State.missing_drawing_dll;
            return RSP_State.correct;
        }

        private static bool DoesRSPContainDrawingDLL(string rsp_path)
        {
            if (!File.Exists(rsp_path)) return false;
            string rsp_data = FileHelper.ReadFileIntoString(rsp_path);
            return (Regex.Match(rsp_data, RSP_DRAWING_DLL_REGEX).Success);
        }

        private static void AddDrawingDLLToRSP(string rsp_path)
        {
            string rsp_data = FileHelper.ReadFileIntoString(rsp_path);
            rsp_data += RSP_DRAWING_DLL_CODE;
            FileHelper.WriteStringToFile(rsp_data, rsp_path);
        }

        public static void RemoveDefineSymbols()
        {
            UnityHelper.SetDefineSymbol(DEFINE_SYMBOLS.IMAGING_EXISTS, false);
        }
    }
}