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
        public MaterialProperty.PropType type;
        public string[] attributes;

        public float defaultFloatValue;
        public int defaultIntValue;
        public Vector2 defaultVector2Value;
        public string defaultTextureName;
        public Vector2 rangeLimits;

        public override string ToString()
        {
            return $"({name}) {description}";
        }
    }
}