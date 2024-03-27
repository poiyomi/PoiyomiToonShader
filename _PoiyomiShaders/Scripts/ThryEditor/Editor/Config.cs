// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class Config
    {
        // consts
        private const string PATH_CONFIG_FILE = "Thry/Config.json";
        private const string VERSION = "2.51.7";

        // static
        private static Config config;

        public static void OnCompile()
        {
            if (!File.Exists(PATH_CONFIG_FILE))
            {
                //Settings.firstTimePopup();
            }
            else
            {
                string prevVersion = Singleton.verion;
                string installedVersion = VERSION;
                int versionComparision = Helper.CompareVersions(installedVersion, prevVersion);
                if (versionComparision != 0)
                {
                    config.verion = VERSION;
                    config.Save();
                }
                if (versionComparision == 1)
                {
                    Settings.updatedPopup(versionComparision);
                }
                else if (versionComparision == -1)
                {
                    config.OnUpgrade(prevVersion);
                    Debug.Log(">>> Thry Editor has been updated to version " + installedVersion);
                }
            }
        }

        public static Config Singleton
        {
            get
            {
                if (config == null)
                {
                    if (File.Exists(PATH_CONFIG_FILE))
                        config = JsonUtility.FromJson<Config>(FileHelper.ReadFileIntoString(PATH_CONFIG_FILE));
                    else
                        config = new Config().Save();
                }
                return config;
            }
        }

        //actual config class
        public TextureDisplayType default_texture_type = TextureDisplayType.small;
        public bool showRenderQueue = true;
        public bool showManualReloadButton = false;
        public bool allowCustomLockingRenaming = false;
        public bool autoMarkPropertiesAnimated = true;
        public TextureImporterFormat texturePackerCompressionWithAlphaOverwrite = TextureImporterFormat.Automatic;
        public TextureImporterFormat texturePackerCompressionNoAlphaOverwrite = TextureImporterFormat.Automatic;
        public TextureImporterFormat gradientEditorCompressionOverwrite = TextureImporterFormat.Automatic;

        public string locale = "English";

        public string gradient_name = "gradient_<hash>.png";
        
        public bool autoSetAnchorOverride = true;
        public HumanBodyBones humanBoneAnchor = HumanBodyBones.Spine;
        public string anchorOverrideObjectName = "AutoAnchorObject";
        public bool autoSetAnchorAskedOnce = false;
        public bool enableDeveloperMode = false;
        public bool disableUnlockedShaderStrippingOnBuild = false;
        public bool forceAsyncCompilationPreview = true;
        public bool fixKeywordsWhenLocking = true;
        public bool saveAfterLockUnlock = true;

        public string verion = VERSION;

        public Config Save()
        {
            FileHelper.WriteStringToFile(JsonUtility.ToJson(this, true), PATH_CONFIG_FILE);
            return this;
        }

        private void OnUpgrade(string oldVersionString)
        {
            Version newVersion = new Version(VERSION);
            Version oldVersion = new Version(oldVersionString);

            //Upgrade locking valuesd from Animated property to tags
            if (newVersion >= "2.11.0" && oldVersion > "2.0" && oldVersion < "2.11.0")
            {
                ShaderOptimizer.UpgradeAnimatedPropertiesToTagsOnAllMaterials();
            }
        }
    }

    public class Version
    {
        private string value;

        public Version(string s)
        {
            if (string.IsNullOrEmpty(s)) s = "0";
            this.value = s;
        }

        public static bool operator ==(Version x, Version y)
        {
            return Helper.CompareVersions(x.value, y.value) == 0;
        }

        public static bool operator !=(Version x, Version y)
        {
            return Helper.CompareVersions(x.value, y.value) != 0;
        }

        public static bool operator >(Version x, Version y)
        {
            return Helper.CompareVersions(x.value, y.value) == -1;
        }

        public static bool operator <(Version x, Version y)
        {
            return Helper.CompareVersions(x.value, y.value) == 1;
        }

        public static bool operator >=(Version x, Version y)
        {
            return Helper.CompareVersions(x.value, y.value) < 1;
        }

        public static bool operator <=(Version x, Version y)
        {
            return Helper.CompareVersions(x.value, y.value) > -1;
        }

        public static bool operator ==(Version x, string y)
        {
            return Helper.CompareVersions(x.value, y) == 0;
        }

        public static bool operator !=(Version x, string y)
        {
            return Helper.CompareVersions(x.value, y) != 0;
        }

        public static bool operator >(Version x, string y)
        {
            return Helper.CompareVersions(x.value, y) == -1;
        }

        public static bool operator <(Version x, string y)
        {
            return Helper.CompareVersions(x.value, y) == 1;
        }

        public static bool operator >=(Version x, string y)
        {
            return Helper.CompareVersions(x.value, y) < 1;
        }

        public static bool operator <=(Version x, string y)
        {
            return Helper.CompareVersions(x.value, y) > -1;
        }

        public override bool Equals(object o)
        {
            if (o is Version) return this == (o as Version);
            if (o is string) return this == (o as string);
            return false;
        }

        public override string ToString()
        {
            return value;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
