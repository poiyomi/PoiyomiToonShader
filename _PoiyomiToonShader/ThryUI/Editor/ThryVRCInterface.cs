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
        private const string TEMP_VRC_SDK_PACKAGE_PATH = "./vrc_sdk_package.unitypackage";

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

        public bool sdk_is_installed;
        public bool sdk_is_up_to_date;
        public string installed_sdk_version;
        public string newest_sdk_version;
        public string sdk_path = null;

        public bool user_logged_in;

        public VRCInterface()
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
#if VRC_SDK_EXISTS
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
            foreach (string guid in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                if (p.Contains("VRCSDK/version"))
                    path = p;
            }
            if (path == null || !File.Exists(path))
                return "";
            sdk_path = Regex.Match(path, @".*\/").Value;
            return Helper.ReadFileIntoString(path);
        }

        private static string GetNewestSDKVersion()
        {
#if VRC_SDK_EXISTS
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

        private static bool IsVRCSDKInstalled()
        {
            return Helper.NameSpaceExists("VRCSDK2");
        }

        public void RemoveVRCSDK()
        {
            RemoveVRCSDK(true);
        }

        public void RemoveVRCSDK(bool refresh)
        {
            Helper.SaveValueToFile("delete_vrc_sdk", "true", ".thry_after_compile_data");
            Helper.SetDefineSymbol(Settings.DEFINE_SYMBOLE_VRC_SDK_INSTALLED, false);
            AssetDatabase.Refresh();
        }

        [InitializeOnLoad]
        public class Startup
        {
            static Startup()
            {
                if (Helper.LoadValueFromFile("delete_vrc_sdk",  ".thry_after_compile_data")=="true")
                    DeleteVRCSDKFolder();
            }
        }

        private static void DeleteVRCSDKFolder()
        {
            if (!Get().sdk_is_installed)
            {
                Helper.SaveValueToFile("delete_vrc_sdk", "false", ".thry_after_compile_data");
                if (Helper.LoadValueFromFile("update_vrc_sdk", ".thry_after_compile_data") == "true")
                    DownloadAndInstallVRCSDK();
                else
                    Settings.is_changing_vrc_sdk = false;
            }
            if (Get().sdk_path != null && Directory.Exists(Get().sdk_path))
            {
                Directory.Delete(Get().sdk_path, true);
                AssetDatabase.Refresh();
            }
            Update();
        }

        public void UpdateVRCSDK()
        {
            Helper.SaveValueToFile("update_vrc_sdk", "true", ".thry_after_compile_data");
            this.RemoveVRCSDK();
        }

        public static void DownloadAndInstallVRCSDK()
        {
            string url = "https://vrchat.net/download/sdk";

            if (File.Exists(TEMP_VRC_SDK_PACKAGE_PATH))
                File.Delete(TEMP_VRC_SDK_PACKAGE_PATH);
            Helper.DownloadFileASync(url, TEMP_VRC_SDK_PACKAGE_PATH, VRCSDKUpdateCallback);
        }

        public static void VRCSDKUpdateCallback(string data)
        {
            Helper.SaveValueToFile("update_vrc_sdk", "false", ".thry_after_compile_data");
            AssetDatabase.ImportPackage(TEMP_VRC_SDK_PACKAGE_PATH, false);
            File.Delete(TEMP_VRC_SDK_PACKAGE_PATH);
            Update();
        }
    }
}
