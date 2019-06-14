using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ThryConfig : MonoBehaviour {

    public const string CONFIG_FILE_PATH = "./Assets/.ThryConfig.json";
    public const string VERSION = "0.6.1";
    private static Config config;



    [InitializeOnLoad]
    public class Startup
    {
        static Startup()
        {
            if (!File.Exists(CONFIG_FILE_PATH)) ThrySettings.firstTimePopup();
            else
            {
                int versionComparision = ThryHelper.compareVersions(VERSION, GetConfig().verion);
                if (versionComparision != 0)
                {
                    config.verion = VERSION;
                    config.save();
                    ThrySettings.updatedPopup(versionComparision);
                }
            }
        }
    }

    public class Config
    {
        public bool useBigTextures = false;
        public bool showRenderQueue = true;
        public bool renderQueueShaders = true;

        public bool vrchatAutoFillAvatarDescriptor = false;
        public int vrchatDefaultAnimationSetFallback = 2;
        public bool vrchatForceFallbackAnimationSet = false;

        public bool showImportPopup = false;
        public int materialValuesUpdateRate = 33;
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

    public static Config GetConfig()
    {
        if (config == null) config = LoadConfig();
        return config;
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
}
