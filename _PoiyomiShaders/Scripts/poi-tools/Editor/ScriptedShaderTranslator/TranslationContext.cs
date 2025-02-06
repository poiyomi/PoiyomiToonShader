using System.Collections;
using System.Collections.Generic;
using Thry;
using UnityEngine;
using UnityEngine.Rendering;

namespace Poi.Tools.ShaderTranslator
{
    public struct TranslationContext
    {
        /// <summary>
        /// The material we're translating
        /// </summary>
        public Material Material;
        /// <summary>
        /// A dictionary of material properties and their values
        /// </summary>
        public Dictionary<ShaderProperty, object> SourcePropertiesAndValues;
        /// <summary>
        /// Thry shader editor for when we switch the material to poiyomi.
        /// </summary>
        public ShaderEditor ThryShaderEditor;
        /// <summary>
        /// The original render queue, before switched the shader to poiyomi
        /// </summary>
        public int originalRenderQueue;
    }
}