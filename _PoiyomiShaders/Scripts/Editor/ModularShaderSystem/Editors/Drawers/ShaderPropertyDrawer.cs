using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.Analytics;

namespace Poiyomi.ModularShaderSystem.UI
{
    public enum DefaultTextureValue
    {
        White,
        Black,
        Gray,
        Bump,
    }
    
    [CustomPropertyDrawer(typeof(Property))]
    public class ShaderPropertyDrawer : PropertyDrawer
    {
        private VisualElement _root;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            _root = new VisualElement();

            var visualTree = Resources.Load<VisualTreeAsset>(MSSConstants.RESOURCES_FOLDER + "/MSSUIElements/ShaderPropertyDrawer");
            VisualElement template = visualTree.CloneTree();
            var foldout = new Foldout();
            foldout.text = property.displayName;
            foldout.RegisterValueChangedCallback((e) => property.isExpanded = e.newValue);
            foldout.value = property.isExpanded;

            var nameField = template.Q<TextField>("Name");
            nameField.RegisterValueChangedCallback(evt => foldout.text = evt.newValue);
            var enumField = template.Q<EnumField>("TypeField");
            var valueContainer = template.Q<VisualElement>("ValueContainer");

            var type = property.FindPropertyRelative("Type");
            var defaultValue = property.FindPropertyRelative("DefaultValue");

            var propType = GetPropertyTypeFromSerializedProperty(type.stringValue);

            enumField.Init(propType);
            
            enumField.value = propType;
            
            enumField.RegisterValueChangedCallback(e =>
            {
                SetPropType(type, (PropertyType)e.newValue);
                SetPropDefaultValue(defaultValue,"");
                UpdateValueContainer(property, defaultValue, type, (PropertyType)e.newValue, type.stringValue, defaultValue.stringValue, valueContainer);
            });

            UpdateValueContainer(property, defaultValue, type, propType, type.stringValue, defaultValue.stringValue, valueContainer);

            foldout.Add(template);
            _root.Add(foldout);
            return _root;
        }

        private void SetPropDefaultValue(SerializedProperty defaultValue, string v)
        {
            defaultValue.stringValue = v;
            defaultValue.serializedObject.ApplyModifiedProperties();
        }
        
        private void SetPropType(SerializedProperty type, string v)
        {
            type.stringValue = v;
            type.serializedObject.ApplyModifiedProperties();
        }
        
        private void SetPropType(SerializedProperty propType, PropertyType type)
        {
            string typeString = "";
            switch (type)
            {
                case PropertyType.Int: typeString = "Int"; break;
                case PropertyType.Float: typeString = "Float"; break;
                case PropertyType.Range: typeString = "Range(0, 1)"; break;
                case PropertyType.Vector: typeString = "Vector"; break;
                case PropertyType.Color: typeString = "Color"; break;
                case PropertyType.Texture2D: typeString = "2D"; break;
                case PropertyType.Texture2DArray: typeString = "2DArray"; break;
                case PropertyType.Cube: typeString = "Cube"; break;
                case PropertyType.CubeArray: typeString = "CubeArray"; break;
                case PropertyType.Texture3D: typeString = "3d"; break;
            }

            propType.stringValue = typeString;
            propType.serializedObject.ApplyModifiedProperties();
        }

        private void UpdateValueContainer(SerializedProperty property, SerializedProperty defaultValue, SerializedProperty type, PropertyType propType, string propTypeString, string propValue, VisualElement element)
        {
            VisualElement field = null;
            switch (propType)
            {
                case PropertyType.Float:
                    float floatValue = 0;
                    if (float.TryParse(propValue, out float f)) floatValue = f;
                    else SetPropDefaultValue(defaultValue,"" + floatValue);
                    var flfield = new FloatField{ value = floatValue, label = "Default value"};
                    flfield.RegisterValueChangedCallback(e => SetPropDefaultValue(defaultValue, "" + e.newValue));
                    field = flfield;
                    break;
                case PropertyType.Range:

                    field = new VisualElement();
                    var rangeLimits = new Vector2(0, 1);
                    float rangeValue = 0;
                    
                    string[] prt = propTypeString.Replace("Range(","").Replace(")","").Split(',');
                    float[] prv = new float[2];
                    bool pfi = true;
                    for (int i = 0; i < 2; i++)
                    {
                        if (float.TryParse(prt[i], out float v))
                        {
                            prv[i] = v;
                        }
                        else
                        {
                            pfi = false;
                            break;
                        }
                    }
                    if (pfi) rangeLimits = new Vector2(prv[0], prv[1]);
                    else SetPropType(type, $"Range({prv[0]}, {prv[1]})");

                    var limits = new Vector2Field { label = "Range limits", value = rangeLimits };

                    if (float.TryParse(propValue, out float r)) rangeValue = r;
                    else SetPropDefaultValue(defaultValue,"" + rangeValue);
                    var horizontalElement = new VisualElement();
                    horizontalElement.style.flexDirection = FlexDirection.Row;
                    
                    var valueSlider = new Slider
                    {
                        value = rangeValue, 
                        lowValue = Math.Min(rangeLimits[0], rangeLimits[1]), 
                        highValue = Math.Max(rangeLimits[0], rangeLimits[1]),
                        
                        label = "Default value"
                    };
                    valueSlider.style.flexGrow = 1;
                    var valueField = new FloatField { value = rangeValue };
                    valueField.style.width = 30;

                    limits.RegisterValueChangedCallback(e =>
                    {
                        valueSlider.lowValue = Math.Min(e.newValue[0], e.newValue[1]);
                        valueSlider.highValue = Math.Max(e.newValue[0], e.newValue[1]);
                        SetPropType(type,$"Range({valueSlider.lowValue}, {valueSlider.highValue})");
                    });

                    valueField.RegisterValueChangedCallback(e =>
                    {
                        if (e.newValue > valueSlider.highValue || e.newValue < valueSlider.lowValue)
                        {
                            e.StopImmediatePropagation();
#if UNITY_6000_2_OR_NEWER
                            e.StopPropagation();
#else
                            e.PreventDefault();
#endif
                            valueField.SetValueWithoutNotify(e.previousValue);
                            return;
                        }
                        valueSlider.SetValueWithoutNotify(e.newValue);
                        SetPropDefaultValue(defaultValue,"" + e.newValue);
                    });
                    valueSlider.RegisterValueChangedCallback(e =>
                    {
                        valueField.SetValueWithoutNotify(e.newValue);
                        SetPropDefaultValue(defaultValue,"" + e.newValue);
                    });
                    
                    field.Add(limits);
                    horizontalElement.Add(valueSlider);
                    horizontalElement.Add(valueField);
                    field.Add(horizontalElement);
                    
                    break;
                case PropertyType.Int:
                    int intValue = 0;
                    if (int.TryParse(propValue, out int iv)) intValue = iv;
                    else SetPropDefaultValue(defaultValue,"" + intValue);
                    var ivfield = new IntegerField{ value = intValue, label = "Default value"};
                    field = ivfield;
                    ivfield.RegisterValueChangedCallback(e => SetPropDefaultValue(defaultValue,"" + e.newValue));
                    break;
                case PropertyType.Color:
                    Color colorValue = Color.white;
                    string[] clvl = propValue.Replace("(","").Replace(")","").Split(',');
                    float[] fv = new float[4];
                    bool vfi = true;
                    for (int i = 0; i < 4; i++)
                    {
                        if (float.TryParse(clvl[i], out float v))
                        {
                            fv[i] = v;
                        }
                        else
                        {
                            vfi = false;
                            break;
                        }
                    }

                    if (vfi) colorValue = new Color(fv[0], fv[1], fv[2], fv[3]);
                    else SetPropDefaultValue(defaultValue,$"({colorValue[0]}, {colorValue[1]}, {colorValue[2]}, {colorValue[3]})");
                    var clfield = new ColorField { value = colorValue, label = "Default value" };
                    field = clfield;
                    clfield.RegisterValueChangedCallback(e => SetPropDefaultValue(defaultValue,$"({e.newValue[0]}, {e.newValue[1]}, {e.newValue[2]}, {e.newValue[3]})"));
                    break;
                case PropertyType.Vector:
                    Vector4 vectorValue = Vector4.zero;
                    string[] vvl = propValue.Replace("(","").Replace(")","").Split(',');
                    float[] vv = new float[4];
                    bool vvi = true;
                    for (int i = 0; i < 4; i++)
                    {
                        if (float.TryParse(vvl[i], out float v))
                        {
                            vv[i] = v;
                        }
                        else
                        {
                            vvi = false;
                            break;
                        }
                    }
                    if (vvi) vectorValue = new Vector4(vv[0], vv[1], vv[2] ,vv[3]);
                    else SetPropDefaultValue(defaultValue,$"({vv[0]}, {vv[1]}, {vv[2]}, {vv[3]})");
                    var vlfield = new Vector4Field{ value = vectorValue, label = "Default value" };
                    field = vlfield;
                    vlfield.RegisterValueChangedCallback(e => SetPropDefaultValue(defaultValue,$"({e.newValue[0]}, {e.newValue[1]}, {e.newValue[2]}, {e.newValue[3]})"));
                    break;
                case PropertyType.Texture2D:
                    var texValue = DefaultTextureValue.White;
                    if (propValue.Contains("white")) texValue = DefaultTextureValue.White;
                    if (propValue.Contains("gray")) texValue = DefaultTextureValue.Gray;
                    if (propValue.Contains("black")) texValue = DefaultTextureValue.Black;
                    if (propValue.Contains("bump")) texValue = DefaultTextureValue.Bump;
                    SetPropDefaultValue(defaultValue,$"\"{Enum.GetName(typeof(DefaultTextureValue), texValue)?.ToLower()}\" {{}}");
                    var txfield = new EnumField { label = "Default value" };
                    txfield.Init(texValue);
                    var textureAsset = new PropertyField(property.FindPropertyRelative("DefaultTextureAsset"), "Texture Override");
                    textureAsset.Bind(property.serializedObject);
                    var vl = new VisualElement();
                    vl.Add(txfield);
                    vl.Add(textureAsset);
                    field = vl;
                    txfield.RegisterValueChangedCallback(e => SetPropDefaultValue(defaultValue,$"\"{Enum.GetName(typeof(DefaultTextureValue), e.newValue)?.ToLower()}\" {{}}"));
                    break;
                case PropertyType.Texture2DArray:
                case PropertyType.CubeArray:
                case PropertyType.Texture3D:
                    SetPropDefaultValue(defaultValue,"\"\"{}");
                    break;
                case PropertyType.Cube:
                    SetPropDefaultValue(defaultValue,"\"\"{}");
                    var textureCubeAsset = new PropertyField(property.FindPropertyRelative("DefaultTextureAsset"), "Texture Override");
                    textureCubeAsset.Bind(property.serializedObject);
                    field = textureCubeAsset; 
                    break;
            }
            
            element.Clear();
            if (field != null) element.Add(field);
        }
        
        private static PropertyType GetPropertyTypeFromSerializedProperty(string propType)
        {
            switch (propType.Trim())
            {
                case "Float": return PropertyType.Float;
                case "Int": return PropertyType.Int;
                case "Color": return PropertyType.Color;
                case "Vector": return PropertyType.Vector;
                case "2D": return PropertyType.Texture2D;
                case "3D": return PropertyType.Texture3D;
                case "Cube": return PropertyType.Cube;
                case "2DArray": return PropertyType.Texture2DArray;
                case "CubeArray": return PropertyType.CubeArray;
                default: return propType.Trim().StartsWith("Range") ? PropertyType.Range : PropertyType.Float;
            }
        }
    }
}