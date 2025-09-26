// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System.IO;
using Thry.ThryEditor.Helpers;
using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor
{
    public class Config
    {
        private const string PATH_CONFIG_FILE = "Thry/Config.json";
        private static readonly Version INSTALLED_VERSION = "2.68.2";

        private static Config s_config;

        public static Config Instance
        {
            get
            {
                if (s_config == null)
                {
                    if(!LoadFromFile(ref s_config))
                        s_config = new Config().Save();
                }
                return s_config;
            }
        }

        [SerializeField] private Version lastVersion = null;
        public Version Version => INSTALLED_VERSION;

        //actual config class
        public LoggingLevel loggingLevel = LoggingLevel.Normal;
        public TextureDisplayType default_texture_type = TextureDisplayType.small;
        public bool showRenderQueue = true;
        public bool showManualReloadButton = false;
        public bool showColorspaceWarnings = true;
        public bool allowCustomLockingRenaming = false;
        public bool autoMarkPropertiesAnimated = true;
        public bool showStarNextToNonDefaultProperties = true;
        public bool showNotes = true;
        public TextureImporterFormat texturePackerCompressionWithAlphaOverwrite = TextureImporterFormat.Automatic;
        public TextureImporterFormat texturePackerCompressionNoAlphaOverwrite = TextureImporterFormat.Automatic;
        public TextureImporterFormat gradientEditorCompressionOverwrite = TextureImporterFormat.Automatic;

        public TextureSaveLocation inlinePackerSaveLocation = TextureSaveLocation.material;
        public string inlinePackerSaveLocationCustom = "Assets/Textures/Packed";
        public bool inlinePackerChrunchCompression = false;

        public string locale = "English";

        public string gradient_name = "gradient_<hash>.png";
        
        public bool autoSetAnchorOverride = true;
        public HumanBodyBones humanBoneAnchor = HumanBodyBones.Chest;
        public string anchorOverrideObjectName = "AutoAnchorObject";
        public bool autoSetAnchorAskedOnce = false;
        public bool enableDeveloperMode = false;
        public bool disableUnlockedShaderStrippingOnBuild = false;
        public bool forceAsyncCompilationPreview = true;
        public bool fixKeywordsWhenLocking = true;
        public bool saveAfterLockUnlock = true;

        public static void OnCompile()
        {
            if (Instance.lastVersion == null)
            {
                ThryLogger.Log("ThryEditor", "Installed version " + INSTALLED_VERSION);
                Instance.lastVersion = INSTALLED_VERSION;
                Instance.Save();
            }
            if (Instance.lastVersion < INSTALLED_VERSION)
            {
                ThryLogger.Log("ThryEditor", "Updated to version " + INSTALLED_VERSION);
                Instance.OnUpgrade(Instance.lastVersion, INSTALLED_VERSION);
                Instance.lastVersion = INSTALLED_VERSION;
                Instance.Save();
            }else if (Instance.lastVersion > INSTALLED_VERSION)
            {
                ThryLogger.LogWarn("ThryEditor", "Downgraded to version " + Instance.lastVersion);
                Settings.OpenDowngradePopup();
                Instance.lastVersion = INSTALLED_VERSION;
                Instance.Save();
            }
        }

        private static bool LoadFromFile(ref Config config)
        {
            if (!File.Exists(PATH_CONFIG_FILE)) return false;
            string data = FileHelper.ReadFileIntoString(PATH_CONFIG_FILE);
            if (string.IsNullOrWhiteSpace(data)) return false;
            config = JsonUtility.FromJson<Config>(data);
            return true;
        }

        public void ClearVersion()
        {
            lastVersion = null;
            Save();
        }

        public Config Save()
        {
            FileHelper.WriteStringToFile(JsonUtility.ToJson(this, true), PATH_CONFIG_FILE);
            return this;
        }

        private void OnUpgrade(Version oldVersion, Version newVersion)
        {
            //Upgrade locking valuesd from Animated property to tags
            if (newVersion >= "2.11.0" && oldVersion > "2.0" && oldVersion < "2.11.0")
            {
                ShaderOptimizer.UpgradeAnimatedPropertiesToTagsOnAllMaterials();
            }
        }
    }
    
    [System.Serializable]
    public class Version
    {
        [SerializeField]
        private string _stringValue = "0";

        public Version(string s)
        {
            if (string.IsNullOrEmpty(s)) _stringValue = "0";
            else _stringValue = s;
        }

        public static implicit operator Version(string s)
        {
            return new Version(s);
        }

        public static explicit operator string(Version v)
        {
            return v._stringValue;
        }

        public static bool operator ==(Version x, Version y)
        {
            bool xIsNull = x is null || x._stringValue is null || x._stringValue == "0";
            bool yIsNull = y is null || y._stringValue is null || y._stringValue == "0";
            if (xIsNull && yIsNull) return true;
            if (xIsNull || yIsNull) return false;
            return Helper.CompareVersions(x._stringValue, y._stringValue) == 0;
        }

        public static bool operator !=(Version x, Version y)
        {
            return !(x==y);
        }

        public static bool operator >(Version x, Version y)
        {
            return Helper.CompareVersions(x._stringValue, y._stringValue) == -1;
        }

        public static bool operator <(Version x, Version y)
        {
            return Helper.CompareVersions(x._stringValue, y._stringValue) == 1;
        }

        public static bool operator >=(Version x, Version y)
        {
            return Helper.CompareVersions(x._stringValue, y._stringValue) < 1;
        }

        public static bool operator <=(Version x, Version y)
        {
            return Helper.CompareVersions(x._stringValue, y._stringValue) > -1;
        }

        public override bool Equals(object o)
        {
            if (o is Version) return this == (o as Version);
            if (o is string) return this == (o as string);
            return false;
        }

        public override string ToString()
        {
            return _stringValue;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public enum TextureDisplayType
    {
        small, big, big_basic
    }

    public enum TextureSaveLocation
    {
        material, texture, prompt, custom
    }
}
