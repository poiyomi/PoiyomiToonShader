// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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

        private RemoteConfig remoteConfig;

        public SDK_Information sdk_information;

        public class SDK_Information
        {
            public VRC_SDK_Type type;
            public bool is_user_logged_in = false;
            public string installed_version = "0";
            public string available_version = "0";
            public string local_sdk_path;
            public string udon_path;
            public bool is_sdk_up_to_date = true;
        }

        public enum VRC_SDK_Type
        {
            NONE = 0,
            SDK_2= 1,
            SDK_3_Avatar = 2,
            SDK_3_World = 3
        }

        private VRCInterface()
        {
            sdk_information = new SDK_Information();
            sdk_information.type = GetInstalledSDKType();
            InitLocalInformation();
            InitSDKVersionVariables();
        }

        private void InitLocalInformation()
        {
            sdk_information.is_user_logged_in = EditorPrefs.HasKey("sdk#username");
            InitInstalledSDKVersionAndPaths();
        }

        private void InitInstalledSDKVersionAndPaths()
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
                return;

            sdk_information.local_sdk_path = Regex.Match(path, @".*\/").Value;
            if (u_path != null)
            {
                sdk_information.udon_path = Regex.Match(u_path, @".*\/").Value;
            }
            string persistent = PersistentData.Get("vrc_sdk_version");
            if (persistent != null)
                sdk_information.installed_version = persistent;
            else
                sdk_information.installed_version = Regex.Replace(FileHelper.ReadFileIntoString(path), @"\n?\r", "");
        }

        private void InitSDKVersionVariables()
        {
#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
            LoadRemoteConfig(delegate ()
            {
                if (sdk_information.type == VRC_SDK_Type.SDK_2)
                    sdk_information.available_version = UrlToVersion(remoteConfig.sdk2);
                else if(sdk_information.type == VRC_SDK_Type.SDK_3_Avatar)
                    sdk_information.available_version = UrlToVersion(remoteConfig.sdk3_avatars);
                else if (sdk_information.type == VRC_SDK_Type.SDK_3_World)
                    sdk_information.available_version = UrlToVersion(remoteConfig.sdk3_worlds);
                if (sdk_information.type != VRC_SDK_Type.NONE)
                    sdk_information.is_sdk_up_to_date = SDKIsUpToDate();
            });
#endif
        }

        private class RemoteConfig
        {
            public string sdk2;
            public string sdk3_worlds;
            public string sdk3_avatars;
        }

        private void LoadRemoteConfig(Action callback)
        {
            WebHelper2.DownloadStringASync("https://api.vrchat.cloud/api/1/config", delegate (string s)
            {
                Dictionary<object, object> remoteC = (Dictionary<object, object>)Parser.ParseJson(s);
                Dictionary<object, object> urls = (Dictionary<object, object>)remoteC["downloadUrls"];
                remoteConfig = new RemoteConfig();
                remoteConfig.sdk2 = (string)urls["sdk2"];
                remoteConfig.sdk3_worlds = (string)urls["sdk3-worlds"];
                remoteConfig.sdk3_avatars = (string)urls["sdk3-avatars"];
                callback();
            });
        }

        private static Task<RemoteConfig> LoadRemoteConfig()
        {
            return Task.Run(() =>
            {
                var t = new TaskCompletionSource<RemoteConfig>();

                WebHelper2.DownloadStringASync("https://api.vrchat.cloud/api/1/config", delegate (string s)
                {
                    Dictionary<string,object> remoteC = (Dictionary<string, object>)Parser.ParseJson(s);
                    Dictionary<string, object> urls = (Dictionary<string, object>)remoteC["downloadUrls"];
                    RemoteConfig remoteConfig = new RemoteConfig();
                    remoteConfig.sdk2 = (string)urls["sdk2"];
                    remoteConfig.sdk3_worlds = (string)urls["sdk3-worlds"];
                    remoteConfig.sdk3_avatars = (string)urls["sdk3-avatars"];
                    t.TrySetResult(remoteConfig);
                });
                return t.Task;
            });
        }

        private bool SDKIsUpToDate()
        {
            return Helper.compareVersions(sdk_information.installed_version, sdk_information.available_version) != 1;
        }

        public VRC_SDK_Type GetInstalledSDKType()
        {
#if VRC_SDK_VRCSDK3 && UDON
            return VRC_SDK_Type.SDK_3_World;
#elif VRC_SDK_VRCSDK3
            return VRC_SDK_Type.SDK_3_Avatar;
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
            if (Get().sdk_information.type == VRC_SDK_Type.NONE && FileHelper.LoadValueFromFile("update_vrc_sdk", PATH.AFTER_COMPILE_DATA) == "true")
                DownloadAndInstallVRCSDK((VRC_SDK_Type)int.Parse(FileHelper.LoadValueFromFile("update_vrc_sdk_type", PATH.AFTER_COMPILE_DATA)));
        }

        private static void DeleteVRCSDKFolder()
        {
            if (Get().sdk_information.local_sdk_path != null && Directory.Exists(Get().sdk_information.local_sdk_path))
            {
                Directory.Delete(Get().sdk_information.local_sdk_path, true);
                if(Get().GetInstalledSDKType()==VRC_SDK_Type.SDK_3_World)
                    Directory.Delete(Get().sdk_information.udon_path, true);
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

        public async static void DownloadAndInstallVRCSDK(VRC_SDK_Type type)
        {
            RemoteConfig remoteConfig = await LoadRemoteConfig();
            string url;
            if (type == VRC_SDK_Type.SDK_2)
                url = remoteConfig.sdk2;
            else if (type == VRC_SDK_Type.SDK_3_Avatar)
                url = remoteConfig.sdk3_avatars;
            else if (type == VRC_SDK_Type.SDK_3_World)
                url = remoteConfig.sdk3_worlds;
            else
                return;
            if (File.Exists(PATH.TEMP_VRC_SDK_PACKAGE))
                File.Delete(PATH.TEMP_VRC_SDK_PACKAGE);
            PersistentData.Set("vrc_sdk_version", UrlToVersion(url));
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

        private static string UrlToVersion(string url)
        {
            return Regex.Match(url, @"[\d\.]+\.[\d\.]+").Value;
        }
    }
}
