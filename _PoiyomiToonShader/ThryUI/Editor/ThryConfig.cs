// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class Config
    {
        // consts
        private const string PATH_CONFIG_FILE = "Thry/Config.json";
        private const string VERSION = "0.23.5";

        // static
        private static Config config;

        public static void OnCompile()
        {
            if (!File.Exists(PATH_CONFIG_FILE)) Settings.firstTimePopup();
            else
            {
                int versionComparision = Helper.compareVersions(VERSION, Get().verion);
                if (versionComparision != 0)
                {
                    config.verion = VERSION;
                    config.save();
                    Settings.updatedPopup(versionComparision);
                }
            }
        }

        //load the config from file
        private static Config LoadConfig()
        {
            if (File.Exists(PATH_CONFIG_FILE))
                return JsonUtility.FromJson<Config>(Helper.ReadFileIntoString(PATH_CONFIG_FILE));
            new Config().save();
            return new Config();
        }

        public static Config Get()
        {
            if (config == null) config = LoadConfig();
            return config;
        }

        //actual config class
        public bool useBigTextures = false;
        public bool showRenderQueue = true;
        public bool renderQueueShaders = true;

        public string gradient_name = "gradient_<hash>.png";

        public bool showImportPopup = false;
        public string verion = VERSION;

        public bool share_user_data = false;
        public bool share_installed_unity_version = true;
        public bool share_installed_editor_version = true;
        public bool share_used_shaders = true;

        public void save()
        {
            Helper.WriteStringToFile(JsonUtility.ToJson(this), PATH_CONFIG_FILE);
        }
    }
}