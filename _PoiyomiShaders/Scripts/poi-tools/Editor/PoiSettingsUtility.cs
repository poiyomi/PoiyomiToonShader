using System;
using System.IO;
using UnityEngine;

namespace Poi.Tools
{
    /// <summary>
    /// Utility class to save settings inside the project folder.
    /// </summary>
    public static class PoiSettingsUtility
    {
        const string SavePath = "Poi/Settings/";

        public static string SettingsFolder
        {
            get
            {
                if(_settingsFolder == null)
                    return _settingsFolder = $"{ProjectFolder}{SavePath}";
                return _settingsFolder;
            }
        }

        public static string ProjectFolder
        {
            get
            {
                if(_projectFolder == null)
                {
                    string dataPath = Application.dataPath;
                    _projectFolder = dataPath.Substring(0, dataPath.LastIndexOf("Assets"));
                }
                return _projectFolder;
            }
        }

        static string _projectFolder;
        static string _settingsFolder;

        /// <summary>
        /// Serializes an object to json and saves it to [ProjectFolder]/Poi/Settings/<paramref name="filename"/>
        /// </summary>
        /// <param name="filename">The name of your settings file, including extension</param>
        /// <param name="obj">The object to serialize</param>
        /// <param name="prettyPrint">Pretty print the json</param>
        public static void SaveSettings(string filename, object obj, bool prettyPrint = true)
        {
            if(!Directory.Exists(SettingsFolder))
                Directory.CreateDirectory(SettingsFolder);

            string json = JsonUtility.ToJson(obj, prettyPrint);
            File.WriteAllText($"{SettingsFolder}{filename}", json);
        }

        /// <summary>
        /// Tries to load a json settings file and deserialize it to type <typeparamref name="T"/>
        /// </summary>
        /// <param name="filename">The name of your settings file, including extension</param>
        /// <param name="obj">The deserialized object</param>
        /// <typeparam name="T">The type of the deserialized object</typeparam>
        /// <returns>True if loading succeeded</returns>
        public static bool TryLoadSettings<T>(string filename, out T obj) where T : class
        {
            obj = null;
            try
            {
                string path = $"{SettingsFolder}{filename}";
                if(!File.Exists(path))
                    return false;

                string json = File.ReadAllText(path);
                obj = JsonUtility.FromJson<T>(json);
                return true;
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }

        public static bool LoadSettingsOverwrite(string filename, object obj)
        {
            try
            {
                string path = $"{SettingsFolder}{filename}";
                if(!File.Exists(path))
                    return false;

                string json = File.ReadAllText(path);
                JsonUtility.FromJsonOverwrite(json, obj);
                return true;
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }
    }
}
