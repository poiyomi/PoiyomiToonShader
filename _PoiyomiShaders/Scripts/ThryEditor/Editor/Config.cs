// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class Config
    {
        // consts
        private const string PATH_CONFIG_FILE = "Thry/Config.json";
        private const string VERSION = "2.11.3";

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
                int versionComparision = Helper.compareVersions(installedVersion, prevVersion);
                if (versionComparision != 0)
                {
                    config.verion = VERSION;
                    config.save();
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

        //load the config from file
        private static Config LoadConfig()
        {
            if (File.Exists(PATH_CONFIG_FILE))
                return JsonUtility.FromJson<Config>(FileHelper.ReadFileIntoString(PATH_CONFIG_FILE));
            new Config().save();
            return new Config();
        }

        public static Config Singleton
        {
            get
            {
                if (config == null) config = LoadConfig();
                return config;
            }
        }

        //actual config class
        public TextureDisplayType default_texture_type = TextureDisplayType.small;
        public bool showRenderQueue = true;
        public bool renameAnimatedProps = true;

        public string locale = "English";

        public string gradient_name = "gradient_<hash>.png";

        public string verion = VERSION;

        public void save()
        {
            FileHelper.WriteStringToFile(JsonUtility.ToJson(this), PATH_CONFIG_FILE);
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
            this.value = s;
        }

        public static bool operator ==(Version x, Version y)
        {
            return Helper.compareVersions(x.value, y.value) == 0;
        }

        public static bool operator !=(Version x, Version y)
        {
            return Helper.compareVersions(x.value, y.value) != 0;
        }

        public static bool operator >(Version x, Version y)
        {
            return Helper.compareVersions(x.value, y.value) == -1;
        }

        public static bool operator <(Version x, Version y)
        {
            return Helper.compareVersions(x.value, y.value) == 1;
        }

        public static bool operator >=(Version x, Version y)
        {
            return Helper.compareVersions(x.value, y.value) < 1;
        }

        public static bool operator <=(Version x, Version y)
        {
            return Helper.compareVersions(x.value, y.value) > -1;
        }

        public static bool operator ==(Version x, string y)
        {
            return Helper.compareVersions(x.value, y) == 0;
        }

        public static bool operator !=(Version x, string y)
        {
            return Helper.compareVersions(x.value, y) != 0;
        }

        public static bool operator >(Version x, string y)
        {
            return Helper.compareVersions(x.value, y) == -1;
        }

        public static bool operator <(Version x, string y)
        {
            return Helper.compareVersions(x.value, y) == 1;
        }

        public static bool operator >=(Version x, string y)
        {
            return Helper.compareVersions(x.value, y) < 1;
        }

        public static bool operator <=(Version x, string y)
        {
            return Helper.compareVersions(x.value, y) > -1;
        }
    }
}
