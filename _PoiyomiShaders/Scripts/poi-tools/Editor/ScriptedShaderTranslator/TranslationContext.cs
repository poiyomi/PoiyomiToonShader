using System.Collections;
using System.Collections.Generic;
using Thry;
using UnityEngine;

namespace Poi.Tools.ShaderTranslator
{
    public struct TranslationContext
    {
        public Material Material;
        public Dictionary<ShaderProperty, object> SourcePropertiesAndValues;
        public ShaderEditor ThryShaderEditor;
        public int originalRenderQueue;
    }
}