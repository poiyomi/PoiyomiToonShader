using System.Collections.Generic;
using UnityEngine;

namespace Poiyomi.ModularShaderSystem
{
    [CreateAssetMenu(fileName = "ModularShader", menuName = MSSConstants.CREATE_PATH + "/Modular Shader", order = 0)]
    public class ModularShader : ScriptableObject
    {
        public string Id;
        
        public string Name;
        
        public string Version;
        
        public string Author;
        
        public string Description;
        
        public bool UseTemplatesForProperties;
        
        public TemplateAsset ShaderPropertiesTemplate;
        
        public string ShaderPath;
        
        public TemplateAsset ShaderTemplate;
        
        public string CustomEditor;
        
        public List<Property> Properties;
        
        public List<ShaderModule> BaseModules;
        
        [HideInInspector] public List<bool> BaseModulesEnabled = new List<bool>();
        
        [HideInInspector] public List<ShaderModule> AdditionalModules;
        
        public bool LockBaseModules;
        
        public List<Shader> LastGeneratedShaders;
        
        [HideInInspector] public string AdditionalSerializedData;

        // Header property names are stable even when feature modules are duplicated or reordered.
        [HideInInspector] public List<string> DisabledSections = new List<string>();
    }
}
