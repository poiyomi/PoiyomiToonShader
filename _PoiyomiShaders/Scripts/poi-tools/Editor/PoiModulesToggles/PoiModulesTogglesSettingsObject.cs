using System;
using System.Linq;
using System.Collections.Generic;
using Poiyomi.ModularShaderSystem;
using UnityEditor;

namespace Poi.Tools
{
    [Serializable]
    public class PoiModulesTogglesSettings
    {
        public string[] proShaderModulesInternal;
        public bool[] proShaderModulesEnabled;
        public string[] freeShaderModulesInternal;
        public bool[] freeShaderModulesEnabled;

        [NonSerialized]
        public ShaderModule[] proShaderModules;
        [NonSerialized]
        public ShaderModule[] freeShaderModules;

        public bool IsEnabled(ShaderModule shaderModule)
        {
            for (int i = 0; i < proShaderModules.Length; i++)
            {
                if (proShaderModules[i] == shaderModule)
                {
                    return proShaderModulesEnabled[i];
                }
            }
            for (int i = 0; i < freeShaderModules.Length; i++)
            {
                if (freeShaderModules[i] == shaderModule)
                {
                    return freeShaderModulesEnabled[i];
                }
            }
            return false;
        }
        public HashSet<ShaderModule> GetDisabledModules()
        {
            if (proShaderModules == null) return null;
            if (freeShaderModules == null) return null;
            HashSet<ShaderModule> disabledModules = new HashSet<ShaderModule>();
            for (int i = 0; i < proShaderModules.Length; i++)
            {
                if (!proShaderModulesEnabled[i])
                {
                    disabledModules.Add(proShaderModules[i]);
                }
            }
            for (int i = 0; i < freeShaderModules.Length; i++)
            {
                if (!freeShaderModulesEnabled[i])
                {
                    disabledModules.Add(freeShaderModules[i]);
                }
            }
            return disabledModules;
        }
    }
}
