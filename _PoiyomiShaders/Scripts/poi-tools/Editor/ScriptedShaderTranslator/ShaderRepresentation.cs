using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Poi.Tools.ShaderTranslator
{
    [Serializable]
    public class ShaderRepresentation
    {
        public Shader Shader { get; private set;}

        public List<ShaderProperty> Properties { get => _properties; private set => _properties = value; }
        [SerializeField] List<ShaderProperty> _properties;

        public ShaderRepresentation(Shader shader)
        {
            Shader = shader;
            Properties = new List<ShaderProperty>();
            int propertyCount = ShaderUtil.GetPropertyCount(shader);

            for(int i = 0; i < propertyCount; ++i)
            {
                var prop = new ShaderProperty
                {
                    name = ShaderUtil.GetPropertyName(shader, i),
                    description = ShaderUtil.GetPropertyDescription(shader, i),
                    type = (MaterialProperty.PropType)ShaderUtil.GetPropertyType(shader, i),
                    attributes = shader.GetPropertyAttributes(i),
                };

                ShaderProperty textureStProp = null;

                switch(prop.type)
                {
                    case MaterialProperty.PropType.Color:
                        break;
                    case MaterialProperty.PropType.Range:
                        prop.rangeLimits = shader.GetPropertyRangeLimits(i);
                        break;
                    case MaterialProperty.PropType.Vector:
                        prop.defaultVector2Value = shader.GetPropertyDefaultVectorValue(i);
                        break;
                    case MaterialProperty.PropType.Float:
                        prop.defaultFloatValue = shader.GetPropertyDefaultFloatValue(i);
                        break;
                    case MaterialProperty.PropType.Texture:
                        prop.defaultTextureName = shader.GetPropertyTextureDefaultName(i);
                        textureStProp = new ShaderProperty()
                        {
                            name = $"{prop.name}_ST",
                            type = prop.type,
                            description = $"{prop.description}_ST",
                        };
                        break;
#if UNITY_2021_1_OR_NEWER
                    case MaterialProperty.PropType.Int:
                        prop.defaultIntValue = Convert.ToInt32(shader.GetPropertyDefaultFloatValue(i));
                        break;
#elif UNITY_2022_1_OR_NEWER
                    case MaterialProperty.PropType.Int:
                        prop.defaultIntValue = shader.GetPropertyDefaultIntValue(i);
                        break;
#endif
                    default:
                        break;
                }

                Properties.Add(prop);
                if(textureStProp != null)
                    Properties.Add(textureStProp);
            }
        }

        public ShaderProperty this[string propertyName] => Properties.FirstOrDefault(x => x.name == propertyName);

        public Dictionary<ShaderProperty, object> GetPropertiesWithValues(Material material)
        {
            var dict = new Dictionary<ShaderProperty, object>();
            for(int i = 0; i < Properties.Count; i++)
            {
                ShaderProperty prop = Properties[i];
                switch(prop.type)
                {
                    case MaterialProperty.PropType.Color:
                        dict[prop] = material.GetColor(prop.name);
                        break;
                    case MaterialProperty.PropType.Vector:
                        dict[prop] = material.GetVector(prop.name);
                        break;
                    case MaterialProperty.PropType.Texture:
                        dict[prop] = material.GetTexture(prop.name);

                        // Grab the next property which should be the _ST property representing scale and offset
                        var stProp = Properties[++i];
                        var texScale = material.GetTextureScale(prop.name);
                        var texOffset = material.GetTextureOffset(prop.name);
                        dict[stProp] = new Vector4(texScale.x, texScale.y, texOffset.x, texOffset.y);
                        break;
#if UNITY_2022_1_OR_NEWER
                    case MaterialProperty.PropType.Int:
                        dict[prop] = material.GetInt(prop.name);
                        break;
#endif
                    case MaterialProperty.PropType.Float:
                    case MaterialProperty.PropType.Range:
                        dict[prop] = material.GetFloat(prop.name);
                        break;
                    default:
                        break;
                }
            }

            return dict;
        }
    }
}
