using System.Collections.Generic;
using Poiyomi.ModularShaderSystem;
using UnityEngine;

namespace Poi.Tools
{
    public class PoiModulesTogglesModuleDictionary : ScriptableObject
    {
        public ShaderModule[] shaderModules;
        public string[] propertyToggles;
        public (ShaderModule[], string[]) GetOnlyDisabledModules()
        {
            List<ShaderModule> _shaderModules = new List<ShaderModule>();
            List<string> _propertyToggles = new List<string>();
            var disabledModules = PoiModulesToggles.moduleSettings.GetDisabledModules();
            if (disabledModules == null) return (null, null);
            for (int i = 0; i < shaderModules.Length; i++)
            {
                if (disabledModules.Contains(shaderModules[i]))
                {
                    _shaderModules.Add(shaderModules[i]);
                    _propertyToggles.Add(propertyToggles[i]);
                }
            }
            return (_shaderModules.ToArray(), _propertyToggles.ToArray());
        }
    }
}
