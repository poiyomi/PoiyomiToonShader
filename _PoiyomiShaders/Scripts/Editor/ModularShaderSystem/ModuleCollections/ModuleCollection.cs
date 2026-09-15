using System;
using System.Collections.Generic;
using Poiyomi.ModularShaderSystem;
using UnityEngine;

namespace Poiyomi.ModularShaderSystem.CibbiExtensions
{
    [CreateAssetMenu(fileName = "ModuleCollection", menuName = MSSConstants.CREATE_PATH + "/Module Collection", order = 0)]
    public class ModuleCollection : ShaderModule
    {
        public List<ShaderModule> Modules;
        
        [HideInInspector] public List<bool> ModulesEnabled = new List<bool>();
    }
}