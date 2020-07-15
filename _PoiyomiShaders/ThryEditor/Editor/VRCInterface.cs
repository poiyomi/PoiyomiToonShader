// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class VRCInterface
    {
        private static VRCInterface instance;
        public static VRCInterface Get()
        {
            if (instance == null) instance = new VRCInterface();
            return instance;
        }
        public static void Update()
        {
            instance = new VRCInterface();
        }

        private const string SDK2_URL = "https://vrchat.com/download/sdk2";
        private const string SDK3_URL = "https://vrchat.com/download/sdk3";

        public bool sdk_is_installed;
        public bool sdk_is_up_to_date;
        public string installed_sdk_version;
        public string newest_sdk_version;
        public string sdk_path = null;
        public string udon_path = null;

        public bool user_logged_in;

        public enum VRC_SDK_Type
        {
            NONE = 1,
            SDK_2= 2,
            SDK_3 = 3
        }

        private VRCInterface()
        {
            sdk_is_installed = IsVRCSDKInstalled();
            InitSDKVersionVariables();
            InitUserVariables();
        }

        private void InitSDKVersionVariables()
        {
            if (!sdk_is_installed)
                return;
            installed_sdk_version = GetInstalledSDKVersionAndInitPath();
            newest_sdk_version = "0";
            sdk_is_up_to_date = true;
#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
            VRC.Core.RemoteConfig.Init(delegate ()
            {
                newest_sdk_version = GetNewestSDKVersion();
                sdk_is_up_to_date = SDKIsUpToDate();
            });
#endif
        }

        private void InitUserVariables()
        {
            user_logged_in = EditorPrefs.HasKey("sdk#username");
        }
        
        private string GetInstalledSDKVersionAndInitPath()
        {
            string[] guids = AssetDatabase.FindAssets("version");
            string path = null;
            string u_path = null;
            foreach (string guid in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                if (p.Contains("VRCSDK/version"))
                    path = p;
                if (p.Contains("Udon/version"))
                    u_path = p;
            }
            if (path == null || !File.Exists(path))
                return "";
            sdk_path = Regex.Match(path, @".*\/").Value;
            if(u_path!=null)
                udon_path = Regex.Match(u_path, @".*\/").Value;
            return FileHelper.ReadFileIntoString(path);
        }

        private static string GetNewestSDKVersion()
        {
#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
            string version = VRC.Core.RemoteConfig.GetString("devSdkVersion");
            if(version!=null && version!="")
                return Regex.Match(version, @"[\d\.]+").Value;
#endif
            return "0";
        }

        private bool SDKIsUpToDate()
        {
            return Helper.compareVersions(installed_sdk_version, newest_sdk_version) != 1;
        }

        public VRC_SDK_Type GetInstalledSDKType()
        {
#if VRC_SDK_VRCSDK3
            return VRC_SDK_Type.SDK_3;
#endif
#if VRC_SDK_VRCSDK2
            return VRC_SDK_Type.SDK_2;
#endif
            return VRC_SDK_Type.NONE;
        }

        private static bool IsVRCSDKInstalled()
        {
#if VRC_SDK_VRCSDK3
            return true;
#endif
#if VRC_SDK_VRCSDK2
            return true;
#endif
            return false;
        }

        public static void RemoveVRCSDK()
        {
            DeleteVRCSDKFolder();
        }

        public static void OnCompile()
        {
            if (!Get().sdk_is_installed && FileHelper.LoadValueFromFile("update_vrc_sdk", PATH.AFTER_COMPILE_DATA) == "true")
                DownloadAndInstallVRCSDK((VRC_SDK_Type)int.Parse(FileHelper.LoadValueFromFile("update_vrc_sdk_type", PATH.AFTER_COMPILE_DATA)));
        }

        private static void DeleteVRCSDKFolder()
        {
            if (Get().sdk_path != null && Directory.Exists(Get().sdk_path))
            {
                Directory.Delete(Get().sdk_path, true);
                if(Get().GetInstalledSDKType()==VRC_SDK_Type.SDK_3)
                    Directory.Delete(Get().udon_path, true);
                RemoveDefineSymbols();
                AssetDatabase.Refresh();
            }
            Update();
        }

        public static void UpdateVRCSDK()
        {
            FileHelper.SaveValueToFile("update_vrc_sdk", "true", PATH.AFTER_COMPILE_DATA);
            FileHelper.SaveValueToFile("update_vrc_sdk_type", ""+(int)VRCInterface.Get().GetInstalledSDKType(), PATH.AFTER_COMPILE_DATA);
            RemoveVRCSDK();
        }

        public static void DownloadAndInstallVRCSDK(VRC_SDK_Type type)
        {
            string url;
            if (type == VRC_SDK_Type.SDK_2)
                url = SDK2_URL;
            else if (type == VRC_SDK_Type.SDK_3)
                url = SDK3_URL;
            else
                return;
            if (File.Exists(PATH.TEMP_VRC_SDK_PACKAGE))
                File.Delete(PATH.TEMP_VRC_SDK_PACKAGE);
            WebHelper2.DownloadFileASync(url, PATH.TEMP_VRC_SDK_PACKAGE, VRCSDKUpdateCallback);
        }

        public static void VRCSDKUpdateCallback(string data)
        {
            FileHelper.SaveValueToFile("update_vrc_sdk", "false", PATH.AFTER_COMPILE_DATA);
            AssetDatabase.ImportPackage(PATH.TEMP_VRC_SDK_PACKAGE, false);
            File.Delete(PATH.TEMP_VRC_SDK_PACKAGE);
            Update();
        }

        public static void SetVRCDefineSybolIfSDKImported(string[] importedAssets)
        {
        }

        public static void SetVRCDefineSybolIfSDKDeleted(string[] importedAssets)
        {
            bool currently_deleteing_sdk = (FileHelper.LoadValueFromFile("delete_vrc_sdk", PATH.AFTER_COMPILE_DATA) == "true");
            if (!Settings.is_changing_vrc_sdk && !currently_deleteing_sdk && AssetsContainVRCSDK(importedAssets))
            {
                RemoveDefineSymbols();
                Update();
            }
        }

        public static void RemoveDefineSymbols()
        {
            //UnityHelper.SetDefineSymbol(DEFINE_SYMBOLS.VRC_SDK_INSTALLED, false);
            UnityHelper.SetDefineSymbol("VRC_SDK_VRCSDK3", false,false);
            UnityHelper.SetDefineSymbol("UDON", false,false);
            UnityHelper.SetDefineSymbol("VRC_SDK_VRCSDK2", false);
        }

        public static bool AssetsContainVRCSDK(string[] assets)
        {
            bool vrcImported = false;
            foreach (string s in assets) if (s.Contains("VRCSDK2.dll")|| s.Contains("VRCSDK3.dll")) vrcImported = true;
            return vrcImported; 
        }
    }
}
