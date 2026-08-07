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
#if UNITY_6000_2_OR_NEWER
            int propertyCount = shader.GetPropertyCount();
#else
            int propertyCount = ShaderUtil.GetPropertyCount(shader);
#endif

            for(int i = 0; i < propertyCount; ++i)
            {
                var prop = new ShaderProperty
                {
#if UNITY_6000_2_OR_NEWER
                    name = shader.GetPropertyName(i),
                    description = shader.GetPropertyDescription(i),
                    type = shader.GetPropertyType(i),
#else
                    name = ShaderUtil.GetPropertyName(shader, i),
                    description = ShaderUtil.GetPropertyDescription(shader, i),
                    type = (MaterialProperty.PropType)ShaderUtil.GetPropertyType(shader, i),
#endif
                    attributes = shader.GetPropertyAttributes(i),
                };

                ShaderProperty textureStProp = null;

                switch(prop.type)
                {
#if UNITY_6000_2_OR_NEWER
                    case UnityEngine.Rendering.ShaderPropertyType.Color:
#else
                    case MaterialProperty.PropType.Color:
#endif
                        break;
#if UNITY_6000_2_OR_NEWER
                    case UnityEngine.Rendering.ShaderPropertyType.Range:
#else
                    case MaterialProperty.PropType.Range:
#endif
                        prop.rangeLimits = shader.GetPropertyRangeLimits(i);
                        prop.defaultFloatValue = shader.GetPropertyDefaultFloatValue(i);
                        break;
#if UNITY_6000_2_OR_NEWER
                    case UnityEngine.Rendering.ShaderPropertyType.Vector:
#else
                    case MaterialProperty.PropType.Vector:
#endif
                        prop.defaultVectorValue = shader.GetPropertyDefaultVectorValue(i);
                        break;
#if UNITY_6000_2_OR_NEWER
                    case UnityEngine.Rendering.ShaderPropertyType.Float:
#else
                    case MaterialProperty.PropType.Float:
#endif
                        prop.defaultFloatValue = shader.GetPropertyDefaultFloatValue(i);
                        break;
#if UNITY_6000_2_OR_NEWER
                    case UnityEngine.Rendering.ShaderPropertyType.Texture:
#else
                    case MaterialProperty.PropType.Texture:
#endif
                        prop.defaultTextureName = shader.GetPropertyTextureDefaultName(i);
                        textureStProp = new ShaderProperty()
                        {
                            name = $"{prop.name}_ST",
                            type = prop.type,
                            description = $"{prop.description}_ST",
                        };
                        break;
#if UNITY_6000_2_OR_NEWER
                    case UnityEngine.Rendering.ShaderPropertyType.Int:
                        prop.defaultIntValue = shader.GetPropertyDefaultIntValue(i);
                        break;
#elif UNITY_2022_1_OR_NEWER
                    case MaterialProperty.PropType.Int:
                        prop.defaultIntValue = shader.GetPropertyDefaultIntValue(i);
                        break;
#elif UNITY_2021_1_OR_NEWER
                    case MaterialProperty.PropType.Int:
                        prop.defaultIntValue = Convert.ToInt32(shader.GetPropertyDefaultFloatValue(i));
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

        // Sets true for properties that exist only to drive ThryEditor UI-specific declarations.
        static bool IsNonTranslatableDecorator(string propertyName) => propertyName.StartsWith("s_start_", StringComparison.Ordinal) || propertyName.StartsWith("s_end_", StringComparison.Ordinal);

        public Dictionary<ShaderProperty, object> GetPropertiesWithValues(Material material)
        {
            var dict = new Dictionary<ShaderProperty, object>();

            // Lazily read the material's serialized property sheet (m_SavedProperties). We only need it when a source
            // property isn't declared by the material's current shader - e.g. an auto-upgrade reads the old shader's
            // layout off a material that has already been swapped to the new shader. The typed getters
            // (GetFloat/GetColor/...) are gated by the current shader and would spam "doesn't have a ... property" while
            // returning a default. So for those, we must pull the still-serialized old value directly instead.
            SerializedMaterialValues serialized = null;
            SerializedMaterialValues Serialized() => serialized ?? (serialized = SerializedMaterialValues.Read(material));

            for(int i = 0; i < Properties.Count; i++)
            {
                ShaderProperty prop = Properties[i];
                bool live = material.HasProperty(prop.name);

                switch(prop.type)
                {
#if UNITY_6000_2_OR_NEWER
                    case UnityEngine.Rendering.ShaderPropertyType.Color:
#else
                    case MaterialProperty.PropType.Color:
#endif
                        if (live) dict[prop] = material.GetColor(prop.name);
                        else if (Serialized().Colors.TryGetValue(prop.name, out Color color)) dict[prop] = color;
                        else dict[prop] = default(Color);
                        break;
#if UNITY_6000_2_OR_NEWER
                    case UnityEngine.Rendering.ShaderPropertyType.Vector:
#else
                    case MaterialProperty.PropType.Vector:
#endif
                        if (live) dict[prop] = material.GetVector(prop.name);
                        else if (Serialized().Colors.TryGetValue(prop.name, out Color vector)) dict[prop] = (Vector4)vector;
                        else dict[prop] = prop.defaultVectorValue;
                        break;
#if UNITY_6000_2_OR_NEWER
                    case UnityEngine.Rendering.ShaderPropertyType.Texture:
#else
                    case MaterialProperty.PropType.Texture:
#endif
                        // The _ST scale/offset pseudo-property is always added right after its texture.
                        var stProp = Properties[++i];
                        if (live)
                        {
                            dict[prop] = material.GetTexture(prop.name);
                            var texScale = material.GetTextureScale(prop.name);
                            var texOffset = material.GetTextureOffset(prop.name);
                            dict[stProp] = new Vector4(texScale.x, texScale.y, texOffset.x, texOffset.y);
                        }
                        else if (Serialized().Textures.TryGetValue(prop.name, out var texEnv))
                        {
                            dict[prop] = texEnv.Texture;
                            dict[stProp] = texEnv.ScaleOffset;
                        }
                        else
                        {
                            dict[prop] = null;
                            dict[stProp] = new Vector4(1, 1, 0, 0);
                        }
                        break;
#if UNITY_6000_2_OR_NEWER
                    case UnityEngine.Rendering.ShaderPropertyType.Int:
#else
                    case MaterialProperty.PropType.Int:
#endif
                        if (live) dict[prop] = material.GetInt(prop.name);
                        else if (Serialized().Floats.TryGetValue(prop.name, out float intValue)) dict[prop] = (int)intValue;
                        else dict[prop] = prop.defaultIntValue;
                        break;
#if UNITY_6000_2_OR_NEWER
                    case UnityEngine.Rendering.ShaderPropertyType.Float:
                    case UnityEngine.Rendering.ShaderPropertyType.Range:
#else
                    case MaterialProperty.PropType.Float:
                    case MaterialProperty.PropType.Range:
#endif
                        if (live) dict[prop] = material.GetFloat(prop.name);
                        else if(Serialized().Floats.TryGetValue(prop.name, out float floatValue)) dict[prop] = floatValue;
                        else dict[prop] = prop.defaultFloatValue;
                        break;
                    default:
                        break;
                }
            }

            return dict;
        }

        // Snapshot a material's serialized property sheet and read directly via SerializedObject
        // so values remain accessible even for properties the material's current shader no longer
        // declares. Unity's typed material getters are gated by teh current shader; the serialized
        // sheet does not.
        class SerializedMaterialValues
        {
            public readonly Dictionary<string, float> Floats = new Dictionary<string, float>();
            public readonly Dictionary<string, Color> Colors = new Dictionary<string, Color>();
            public readonly Dictionary<string, (Texture Texture, Vector4 ScaleOffset)> Textures = new Dictionary<string, (Texture, Vector4)>();

            public static SerializedMaterialValues Read(Material material)
            {
                var values = new SerializedMaterialValues();
                if (material == null) return values;

                var serializedObject = new SerializedObject(material);
                SerializedProperty saved = serializedObject.FindProperty("m_SavedProperties");
                if (saved == null) return values;

                ReadFloats(saved.FindPropertyRelative("m_Floats"), values.Floats);
                ReadFloats(saved.FindPropertyRelative("m_Ints"), values.Floats); // null-safe; older Unity keeps ints here
                ReadColors(saved.FindPropertyRelative("m_Colors"), values.Colors);
                ReadTextures(saved.FindPropertyRelative("m_TexEnvs"), values.Textures);

                return values;
            }

            static void ReadFloats(SerializedProperty array, Dictionary<string, float> into)
            {
                if (array == null || !array.isArray) return;

                for (int i = 0; i < array.arraySize; i++)
                {
                    SerializedProperty entry = array.GetArrayElementAtIndex(i);
                    string name = GetEntryName(entry);
                    if (string.IsNullOrEmpty(name)) continue;

                    SerializedProperty value = entry.FindPropertyRelative("second");
                    if (value == null) continue;

                    into[name] = value.propertyType == SerializedPropertyType.Integer ? value.intValue : value.floatValue;
                }
            }

            static void ReadColors(SerializedProperty array, Dictionary<string, Color> into)
            {
                if (array == null || !array.isArray) return;

                for (int i = 0; i < array.arraySize; i++)
                {
                    SerializedProperty entry = array.GetArrayElementAtIndex(i);
                    string name = GetEntryName(entry);
                    if (string.IsNullOrEmpty(name)) continue;

                    SerializedProperty value = entry.FindPropertyRelative("second");
                    if (value != null) into[name] = value.colorValue;
                }
            }

            static void ReadTextures(SerializedProperty array, Dictionary<string, (Texture, Vector4)> into)
            {
                if (array == null || !array.isArray) return;

                for (int i = 0; i < array.arraySize; i++)
                {
                    SerializedProperty entry = array.GetArrayElementAtIndex(i);
                    string name = GetEntryName(entry);
                    if (string.IsNullOrEmpty(name)) continue;

                    SerializedProperty value = entry.FindPropertyRelative("second");
                    if (value == null) continue;

                    var texture = value.FindPropertyRelative("m_Texture")?.objectReferenceValue as Texture;
                    Vector2 scale = value.FindPropertyRelative("m_Scale")?.vector2Value ?? Vector2.one;
                    Vector2 offset = value.FindPropertyRelative("m_Offset")?.vector2Value ?? Vector2.zero;
                    into[name] = (texture, new Vector4(scale.x, scale.y, offset.x, offset.y));
                }
            }

            static string GetEntryName(SerializedProperty entry)
            {
                SerializedProperty first = entry.FindPropertyRelative("first");
                if (first == null) return null;
                if (first.propertyType == SerializedPropertyType.String) return first.stringValue;

                SerializedProperty nameProp = first.FindPropertyRelative("name");
                return nameProp != null ? nameProp.stringValue : null;
            }
        }
    }
}
