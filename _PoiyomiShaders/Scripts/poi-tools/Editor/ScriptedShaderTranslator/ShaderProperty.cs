using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Poi.Tools.ShaderTranslator
{
    [Serializable]
    public class ShaderProperty
    {
        public string name;
        public string description;
#if UNITY_6000_2_OR_NEWER
        public UnityEngine.Rendering.ShaderPropertyType type;
#else
        public MaterialProperty.PropType type;
#endif
        public string[] attributes;

        public float defaultFloatValue;
        public int defaultIntValue;
        public Vector4 defaultVectorValue;
        public string defaultTextureName;
        public Vector2 rangeLimits;

        public override string ToString()
        {
            return $"({name}) {description}";
        }
    }
}