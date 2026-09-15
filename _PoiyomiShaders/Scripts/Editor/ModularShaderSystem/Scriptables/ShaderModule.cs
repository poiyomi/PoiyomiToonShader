using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Poiyomi.ModularShaderSystem
{
    [CreateAssetMenu(fileName = "ShaderModule", menuName = MSSConstants.CREATE_PATH + "/Shader Module", order = 0)]
    public class ShaderModule : ScriptableObject
    {
        public string Id;
        
        public string Name;
        
        public string Version;
        
        public string Author;
        
        public string Description;
        public bool AllowDuplicates;
        public bool ForceDuplicateLogic;

        public List<EnableProperty> EnableProperties;
        
        public List<Property> Properties;
        
        public List<string> ModuleDependencies;
        
        public List<string> IncompatibleWith;
        
        public List<ModuleTemplate> Templates;
        
        public List<ShaderFunction> Functions;
        
        [HideInInspector] public string AdditionalSerializedData;

        private bool _isCopy = false;
        public bool IsCopy => _isCopy;

        public ShaderModule DeepCopy()
        {
            var copy = CreateInstance<ShaderModule>();
            copy.Id = Id;
            copy.Name = Name;
            copy.Version = Version;
            copy.Author = Author;
            copy.Description = Description;
            copy.AllowDuplicates = AllowDuplicates;
            copy.ForceDuplicateLogic = ForceDuplicateLogic;
            copy.EnableProperties = EnableProperties.Select(x => x.DeepCopy()).ToList();
            copy.Properties = Properties.Select(x => x.DeepCopy()).ToList();
            copy.ModuleDependencies = ModuleDependencies.ToList();
            copy.IncompatibleWith = IncompatibleWith.ToList();
            copy.Templates = Templates.Select(x => x.DeepCopy()).ToList();
            copy.Functions = Functions.Select(x => x.DeepCopy()).ToList();
            copy.AdditionalSerializedData = AdditionalSerializedData;
            copy._isCopy = true;
            return copy;
        }
    }
}