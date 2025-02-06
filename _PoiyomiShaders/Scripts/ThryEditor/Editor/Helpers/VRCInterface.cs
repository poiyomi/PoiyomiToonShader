using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;

namespace Thry
{
    public class VRCInterface
    {
        private static VRCInterface _Instance;
        public static VRCInterface Get()
        {
            if (_Instance == null) _Instance = new VRCInterface();
            return _Instance;
        }
        public static void Update()
        {
            _Instance = new VRCInterface();
        }

        public SDK_Information Sdk_information;

        public class SDK_Information
        {
            public VRC_SDK_Type type;
            public string installed_version = "0";
        }

        public enum VRC_SDK_Type
        {
            NONE = 0,
            SDK_2 = 1,
            SDK_3_Avatar = 2,
            SDK_3_World = 3
        }

        private VRCInterface()
        {
            Sdk_information = new SDK_Information();
            Sdk_information.type = GetInstalledSDKType();
            InitInstalledSDKVersionAndPaths();
        }

        private void InitInstalledSDKVersionAndPaths()
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
                return;
            string persistent = PersistentData.Get("vrc_sdk_version");
            if (persistent != null)
                Sdk_information.installed_version = persistent;
            else
                Sdk_information.installed_version = Regex.Replace(FileHelper.ReadFileIntoString(path), @"\n?\r", "");
        }

        public static VRC_SDK_Type GetInstalledSDKType()
        {
#if VRC_SDK_VRCSDK3 && UDON
            return VRC_SDK_Type.SDK_3_World;
#elif VRC_SDK_VRCSDK3
            return VRC_SDK_Type.SDK_3_Avatar;
#elif VRC_SDK_VRCSDK2
            return VRC_SDK_Type.SDK_2;
#else
            return VRC_SDK_Type.NONE;
#endif
        }

        public static bool IsVRCSDKInstalled()
        {
#if VRC_SDK_VRCSDK3
            return true;
#elif VRC_SDK_VRCSDK2
            return true;
#else
            return false;
#endif
        }
    }

}