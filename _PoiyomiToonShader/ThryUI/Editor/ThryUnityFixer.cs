using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class UnityFixer
    {
        public const string RSP_DRAWING_DLL_CODE = "-r:System.Drawing.dll";
        public const string RSP_DRAWING_DLL_REGEX = @"-r:\s*System\.Drawing\.dll";

        public static void OnAssetDeleteCheckDrawingDLL(string[] deleted_assets)
        {
            foreach (string path in deleted_assets)
            {
                if (path == PATH.RSP_NEEDED_PATH + GetRSPFilename() + ".rsp")
                    UnityHelper.SetDefineSymbol(DEFINE_SYMBOLS.IMAGING_EXISTS, false, true);
            }
        }

        public static void CheckAPICompatibility()
        {
            ApiCompatibilityLevel level = PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone);
            if (level == ApiCompatibilityLevel.NET_2_0_Subset)
                PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_2_0);
            UnityHelper.SetDefineSymbol(DEFINE_SYMBOLS.API_NET_TWO, true, true);
        }

        private static string GetRSPFilename()
        {
            if (Helper.compareVersions("2018", Application.unityVersion) == 1)
                return "csc";
            return "mcs";
        }

        public static void CheckDrawingDll()
        {
            string rsp_path = null;
            string filename = GetRSPFilename();

            RSP_State state = CheckRSPState(filename, ref rsp_path);
            switch (state)
            {
                case RSP_State.missing:
                case RSP_State.missing_drawing_dll:
                    AddDrawingDLLToRSP(PATH.RSP_NEEDED_PATH + filename + ".rsp");
                    break;
            }

            UnityHelper.SetDefineSymbol(DEFINE_SYMBOLS.IMAGING_EXISTS, true, true);
        }

        private enum RSP_State { correct=2, missing=0, missing_drawing_dll=1};

        private static RSP_State CheckRSPState(string rsp_name, ref string rsp_path)
        {
            int state = 0;
            foreach (string id in AssetDatabase.FindAssets(rsp_name))
            {
                string path = AssetDatabase.GUIDToAssetPath(id);
                int new_state = 0;
                bool correctPath = path == PATH.RSP_NEEDED_PATH + rsp_name + ".rsp";
                bool includesDrawingDLL = DoesRSPContainDrawingDLL(rsp_path);

                if (correctPath && includesDrawingDLL) new_state = 2;
                else if (correctPath) new_state = 1;

                if (new_state > state)
                {
                    state = new_state;
                    rsp_path = path;
                }
            }
            return (RSP_State)state;
        }

        private static bool DoesRSPContainDrawingDLL(string rsp_path)
        {
            string rsp_data = FileHelper.ReadFileIntoString(rsp_path);
            return (Regex.Match(rsp_data, RSP_DRAWING_DLL_REGEX).Success);
        }

        private static void AddDrawingDLLToRSP(string rsp_path)
        {
            string rsp_data = FileHelper.ReadFileIntoString(rsp_path);
            rsp_data += RSP_DRAWING_DLL_CODE;
            FileHelper.WriteStringToFile(rsp_data, rsp_path);
        }

    }
}