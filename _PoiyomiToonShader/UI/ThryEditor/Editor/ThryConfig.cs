using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class Config
    {
        //static methods
        private static Config config;
        private const string CONFIG_FILE_PATH = "./Assets/.ThryConfig.json";
        private const string VERSION = "0.10.2";

        [InitializeOnLoad]
        public class Startup
        {
            static Startup()
            {
                if (!File.Exists(CONFIG_FILE_PATH)) Settings.firstTimePopup();
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
        }

        //load the config from file
        private static Config LoadConfig()
        {
            Config config = null;
            if (File.Exists(CONFIG_FILE_PATH))
            {
                StreamReader reader = new StreamReader(CONFIG_FILE_PATH);
                config = JsonUtility.FromJson<Config>(reader.ReadToEnd());
                reader.Close();
            }
            else
            {
                File.CreateText(CONFIG_FILE_PATH).Close();
                config = new Config();
                config.save();
            }
            return config;
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

        public bool vrchatAutoFillAvatarDescriptor = false;
        public int vrchatDefaultAnimationSetFallback = 2;
        public bool vrchatForceFallbackAnimationSet = false;

        public bool showImportPopup = false;
        public string verion = "0";

        public void save()
        {
            StreamWriter writer = new StreamWriter(CONFIG_FILE_PATH, false);
            writer.WriteLine(this.SaveToString());
            writer.Close();
        }

        public string SaveToString()
        {
            return JsonUtility.ToJson(this);
        }
    }
}