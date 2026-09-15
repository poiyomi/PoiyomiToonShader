using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Poiyomi.ModularShaderSystem
{
    [Serializable]
    public class ModuleTemplate 
    {
        public TemplateAsset Template;
        
        [FormerlySerializedAs("Keyword")] public List<string> Keywords;
        
        [FormerlySerializedAs("IsCGOnly")] public bool NeedsVariant;
        
        public int Queue = 100;

        public ModuleTemplate DeepCopy()
        {
            var copy = new ModuleTemplate();
            copy.Template = Template.DeepCopy();
            copy.Keywords = Keywords.ToList();
            copy.NeedsVariant = NeedsVariant;
            copy.Queue = Queue;
            return copy;
        }
    }
}