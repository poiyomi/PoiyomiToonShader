using UnityEngine;
using Poiyomi.ModularShaderSystem;
using System.Collections.Generic;

namespace Poi.Tools.ModularShaderSystem
{
    public class ModularShadersForThryEditorObject : ScriptableObject
    {
        public string Name;
        public Shader Shader;
        public ModularShader OriginalModularShader;
        public ModularShader ModularShader;
        public List<ShaderModule> Modules;

    }
}