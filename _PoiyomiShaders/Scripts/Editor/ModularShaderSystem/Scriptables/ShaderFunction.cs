using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Poiyomi.ModularShaderSystem
{
    [Serializable]
    public class ShaderFunction 
    {
        public string Name;
        
        public string AppendAfter;
        
        [FormerlySerializedAs("Priority")] public short Queue = 100;
        
        public TemplateAsset ShaderFunctionCode;
        
        public List<Variable> UsedVariables;
        
        [FormerlySerializedAs("VariableSinkKeywords")] [FormerlySerializedAs("VariableSinkKeyword")] public List<string> VariableKeywords;
        
        [FormerlySerializedAs("CodeSinkKeywords")] [FormerlySerializedAs("CodeSinkKeyword")] public List<string> CodeKeywords;
    
        public ShaderFunction DeepCopy()
        {
            var copy = new ShaderFunction();
            copy.Name = Name;
            copy.AppendAfter = AppendAfter;
            copy.Queue = Queue;
            copy.ShaderFunctionCode = ShaderFunctionCode.DeepCopy();
            copy.UsedVariables = UsedVariables.Select(x => x.DeepCopy()).ToList();
            copy.VariableKeywords = VariableKeywords.ToList();
            copy.CodeKeywords = CodeKeywords.ToList();
            return copy;
        }
    }
}