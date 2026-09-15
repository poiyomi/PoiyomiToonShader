using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Poi.Tools
{
    [Serializable]
    public class ShaderDestinationManager : ScriptableObject
    {
        [Serializable]
        public class ShaderDestination
        {
            public enum MatchType
            {
                Always,
                Contains,
                StartsWith,
                EndsWith,
                Equals,
                Regex,
            }

            public string matchString;
            public string folderPath = "Assets/";
            public bool enabled = true;
            public MatchType matchType = MatchType.Always;
            public string versionOverride = string.Empty; // Added field
        }

        static SerializedObject _serializedObject;

        public static ShaderDestinationManager Instance
        {
            get
            {
                if(!_instance)
                {
#if UNITY_6000_2_OR_NEWER
                    var manager = FindAnyObjectByType<ShaderDestinationManager>();
#else
                    var manager = FindObjectOfType<ShaderDestinationManager>();
#endif
                    if(manager == null)
                        manager = CreateInstance<ShaderDestinationManager>();
                    _instance = manager;
                    _serializedObject = new SerializedObject(_instance);
                }
                else
                    _serializedObject.UpdateIfRequiredOrScript();
                return _instance;
            }
        }
        static ShaderDestinationManager _instance;

        private void Awake()
        {
            try
            {
                PoiSettingsUtility.LoadSettingsOverwrite(SettingsFileName, this);
            }
            catch(Exception ex)
            {
                Debug.LogError(ex);
            };
            DontDestroyOnLoad(this);
            _serializedObject = new SerializedObject(this);

            EnsureListInitialized();
        }

        void EnsureListInitialized()
        {
            if(destinations == null)
                destinations = new List<ShaderDestination>();
            if(destinations.Count == 0)
            {
                destinations.Add(new ShaderDestination()
                {
                    folderPath = "Assets/_PoiyomiShaders/Shaders/10.0/Pro",
                    matchString = "Poiyomi Pro",
                    matchType = ShaderDestination.MatchType.StartsWith
                });
                destinations.Add(new ShaderDestination()
                {
                    folderPath = "Assets/_PoiyomiShaders/Shaders/10.0/Toon",
                    matchString = "Poiyomi Toon",
                    matchType = ShaderDestination.MatchType.StartsWith
                });
                destinations.Add(new ShaderDestination()
                {
                    folderPath = "Assets",
                    matchType = ShaderDestination.MatchType.Always
                });
            }
        }

        private void OnDestroy()
        {
            PoiSettingsUtility.SaveSettings(SettingsFileName, this);
        }

        const string SettingsFileName = "PoiShaderDestinations.json";

        public List<ShaderDestination> destinations;

#if UNITY_2020_1_OR_NEWER
        public string GetDestinationFromShaderName(string shaderName)
        {
            string pathResult = null;
            foreach(var destination in destinations)
            {
                if(!destination.enabled)
                    continue;

                string matchString = destination.matchString;
                switch(destination.matchType)
                {
                    case ShaderDestination.MatchType.Contains:
                        if(shaderName.IndexOf(matchString, StringComparison.CurrentCultureIgnoreCase) != -1)
                            pathResult = destination.folderPath;
                        break;
                    case ShaderDestination.MatchType.StartsWith:
                        if(shaderName.StartsWith(matchString, StringComparison.CurrentCultureIgnoreCase))
                            pathResult = destination.folderPath;
                        break;
                    case ShaderDestination.MatchType.EndsWith:
                        if(shaderName.EndsWith(matchString, StringComparison.CurrentCultureIgnoreCase))
                            pathResult = destination.folderPath;
                        break;
                    case ShaderDestination.MatchType.Regex:
                        if(Regex.IsMatch(destination.folderPath, matchString))
                            pathResult = destination.folderPath;
                        break;
                    case ShaderDestination.MatchType.Equals:
                        if(shaderName.Equals(matchString, StringComparison.CurrentCultureIgnoreCase))
                            pathResult = destination.folderPath;
                        break;
                    case ShaderDestination.MatchType.Always:
                        pathResult = destination.folderPath;
                        break;
                    default:
                        pathResult = null;
                        break;
                }
                if(pathResult != null)
                    break;
            }
            return pathResult;
        }
#else // Hardcoded paths in 2019
        public string GetDestinationFromShaderName(string shaderName)
        {
            string suffix;
            if(shaderName.StartsWith("Poiyomi Toon"))
                suffix = "/Toon";
            else if(shaderName.StartsWith("Poiyomi Pro"))
                suffix = "/Pro";
            else
                suffix = "/Other";

            return "Assets/_PoiyomiShaders/Shaders/9.0" + suffix;
        }
#endif
    }
}