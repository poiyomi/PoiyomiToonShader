using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class DefineableCondition
    {
        public DefineableConditionType type = DefineableConditionType.NONE;
        public string data = "";
        public DefineableCondition condition1;
        public DefineableCondition condition2;

        CompareType _compareType;
        string _obj;
        ShaderProperty _propertyObj;
        Material _materialInsteadOfEditor;

        string _value;
        float _floatValue;

        bool _hasConstantValue;
        bool _constantValue;

        bool _isInit = false;
        public void Init()
        {
            if (_isInit) return;
            _hasConstantValue = true;
            if (type == DefineableConditionType.NONE) { _constantValue = true; }
            else if (type == DefineableConditionType.TRUE) { _constantValue = true; }
            else if (type == DefineableConditionType.FALSE) { _constantValue = false; }
            else
            {
                var (compareType, compareString) = GetComparetor();
                _compareType = compareType;

                string[] parts = Regex.Split(data, compareString);
                _obj = parts[0].Trim();
                _value = parts[parts.Length - 1].Trim();

                _floatValue = Parser.ParseFloat(_value);
                if (ShaderEditor.Active != null && ShaderEditor.Active.PropertyDictionary.ContainsKey(_obj))
                    _propertyObj = ShaderEditor.Active.PropertyDictionary[_obj];

                if (type == DefineableConditionType.EDITOR_VERSION) InitEditorVersion();
                else if (type == DefineableConditionType.VRC_SDK_VERSION) InitVRCSDKVersion();
                else _hasConstantValue = false;
            }

            _isInit = true;
        }

        void InitEditorVersion()
        {
            int c_ev = Helper.CompareVersions(Config.Singleton.verion, _value);
            if (_compareType == CompareType.EQUAL) _constantValue = c_ev == 0;
            if (_compareType == CompareType.NOT_EQUAL) _constantValue = c_ev != 0;
            if (_compareType == CompareType.SMALLER) _constantValue = c_ev == 1;
            if (_compareType == CompareType.BIGGER) _constantValue = c_ev == -1;
            if (_compareType == CompareType.BIGGER_EQ) _constantValue = c_ev == -1 || c_ev == 0;
            if (_compareType == CompareType.SMALLER_EQ) _constantValue = c_ev == 1 || c_ev == 0;
        }

        void InitVRCSDKVersion()
        {
            if (VRCInterface.Get().Sdk_information.type == VRCInterface.VRC_SDK_Type.NONE)
            {
                _constantValue = false;
                return;
            }
            int c_vrc = Helper.CompareVersions(VRCInterface.Get().Sdk_information.installed_version, _value);
            if (_compareType == CompareType.EQUAL) _constantValue = c_vrc == 0;
            if (_compareType == CompareType.NOT_EQUAL) _constantValue = c_vrc != 0;
            if (_compareType == CompareType.SMALLER) _constantValue = c_vrc == 1;
            if (_compareType == CompareType.BIGGER) _constantValue = c_vrc == -1;
            if (_compareType == CompareType.BIGGER_EQ) _constantValue = c_vrc == -1 || c_vrc == 0;
            if (_compareType == CompareType.SMALLER_EQ) _constantValue = c_vrc == 1 || c_vrc == 0;
        }

        public bool Test()
        {
            Init();
            if (_hasConstantValue) return _constantValue;

            MaterialProperty materialProperty = null;
            switch (type)
            {
                case DefineableConditionType.PROPERTY_BOOL:
                    materialProperty = GetMaterialProperty();
                    if (materialProperty == null) return false;
                    if (_compareType == CompareType.NONE) return materialProperty.GetNumber() == 1;
                    if (_compareType == CompareType.EQUAL) return materialProperty.GetNumber() == _floatValue;
                    if (_compareType == CompareType.NOT_EQUAL) return materialProperty.GetNumber() != _floatValue;
                    if (_compareType == CompareType.SMALLER) return materialProperty.GetNumber() < _floatValue;
                    if (_compareType == CompareType.BIGGER) return materialProperty.GetNumber() > _floatValue;
                    if (_compareType == CompareType.BIGGER_EQ) return materialProperty.GetNumber() >= _floatValue;
                    if (_compareType == CompareType.SMALLER_EQ) return materialProperty.GetNumber() <= _floatValue;
                    break;
                case DefineableConditionType.TEXTURE_SET:
                    materialProperty = GetMaterialProperty();
                    if (materialProperty == null) return false;
                    return (materialProperty.textureValue == null) == (_compareType == CompareType.EQUAL);
                case DefineableConditionType.DROPDOWN:
                    materialProperty = GetMaterialProperty();
                    if (materialProperty == null) return false;
                    if (_compareType == CompareType.NONE) return materialProperty.GetNumber() == 1;
                    if (_compareType == CompareType.EQUAL) return "" + materialProperty.GetNumber() == _value;
                    if (_compareType == CompareType.NOT_EQUAL) return "" + materialProperty.GetNumber() != _value;
                    break;
                case DefineableConditionType.PROPERTY_IS_ANIMATED:
                    return ShaderOptimizer.IsAnimated(_materialInsteadOfEditor, _obj);
                case DefineableConditionType.PROPERTY_IS_NOT_ANIMATED:
                    return !ShaderOptimizer.IsAnimated(_materialInsteadOfEditor, _obj);
                case DefineableConditionType.AND:
                    if (condition1 != null && condition2 != null) return condition1.Test() && condition2.Test();
                    break;
                case DefineableConditionType.OR:
                    if (condition1 != null && condition2 != null) return condition1.Test() || condition2.Test();
                    break;
                case DefineableConditionType.NOT:
                    if (condition1 != null) return !condition1.Test();
                    break;
            }

            return true;
        }

        private MaterialProperty GetMaterialProperty()
        {
            if (_materialInsteadOfEditor) return MaterialEditor.GetMaterialProperty(new Material[] { _materialInsteadOfEditor }, _obj);
            if (_propertyObj != null) return _propertyObj.MaterialProperty;
            return null;
        }
        private (CompareType, string) GetComparetor()
        {
            if (data.Contains("=="))
                return (CompareType.EQUAL, "==");
            if (data.Contains("!="))
                return (CompareType.NOT_EQUAL, "!=");
            if (data.Contains(">="))
                return (CompareType.BIGGER_EQ, ">=");
            if (data.Contains("<="))
                return (CompareType.SMALLER_EQ, "<=");
            if (data.Contains(">"))
                return (CompareType.BIGGER, ">");
            if (data.Contains("<"))
                return (CompareType.SMALLER, "<");
            return (CompareType.NONE, "##");
        }

        public override string ToString()
        {
            switch (type)
            {
                case DefineableConditionType.PROPERTY_BOOL:
                    return data;
                case DefineableConditionType.EDITOR_VERSION:
                    return "EDITOR_VERSION" + data;
                case DefineableConditionType.VRC_SDK_VERSION:
                    return "VRC_SDK_VERSION" + data;
                case DefineableConditionType.TEXTURE_SET:
                    return "TEXTURE_SET" + data;
                case DefineableConditionType.DROPDOWN:
                    return "DROPDOWN" + data;
                case DefineableConditionType.PROPERTY_IS_ANIMATED:
                    return $"isAnimated({data})";
                case DefineableConditionType.PROPERTY_IS_NOT_ANIMATED:
                    return $"isNotAnimated({data})";
                case DefineableConditionType.AND:
                    if (condition1 != null && condition2 != null) return "(" + condition1.ToString() + "&&" + condition2.ToString() + ")";
                    break;
                case DefineableConditionType.OR:
                    if (condition1 != null && condition2 != null) return "(" + condition1.ToString() + "||" + condition2.ToString() + ")";
                    break;
                case DefineableConditionType.NOT:
                    if (condition1 != null) return "!" + condition1.ToString();
                    break;
            }
            return "";
        }

        private static DefineableCondition ParseForThryParser(string s)
        {
            return Parse(s);
        }

        private static readonly char[] ComparissionLiteralsToCheckFor = "*><=".ToCharArray();
        public static DefineableCondition Parse(string s, Material useThisMaterialInsteadOfOpenEditor = null, int start = 0, int end = -1)
        {
            if (end == -1) end = s.Length;
            DefineableCondition con;

            // Debug.Log("Parsing: " + s.Substring(start, end - start));

            int depth = 0;
            int bracketStart = -1;
            int bracketEnd = -1;
            for (int i = start; i < end; i++)
            {
                char c = s[i];
                if (c == '(')
                {
                    depth += 1;
                    if (depth == 1)
                    {
                        bracketStart = i;
                    }
                }
                else if (c == ')')
                {
                    if (depth == 1)
                    {
                        bracketEnd = i;
                    }
                    depth -= 1;
                }
                else if (depth == 0)
                {
                    if (c == '&')
                    {
                        con = new DefineableCondition();
                        con._materialInsteadOfEditor = useThisMaterialInsteadOfOpenEditor;

                        con.type = DefineableConditionType.AND;
                        con.condition1 = Parse(s, useThisMaterialInsteadOfOpenEditor, start, i);
                        con.condition2 = Parse(s, useThisMaterialInsteadOfOpenEditor, i + (s[i + 1] == '&' ? 2 : 1), end);
                        return con;
                    }
                    else if (c == '|')
                    {

                        con = new DefineableCondition();
                        con._materialInsteadOfEditor = useThisMaterialInsteadOfOpenEditor;

                        con.type = DefineableConditionType.OR;
                        con.condition1 = Parse(s, useThisMaterialInsteadOfOpenEditor, start, i);
                        con.condition2 = Parse(s, useThisMaterialInsteadOfOpenEditor, i + (s[i + 1] == '|' ? 2 : 1), end);
                        return con;
                    }
                }
            }


            bool isInverted = IsInverted(s, ref start);

            // if no AND or OR was found, check for brackets
            if (bracketStart != -1 && bracketEnd != -1)
            {
                con = Parse(s, useThisMaterialInsteadOfOpenEditor, bracketStart + 1, bracketEnd);
            }
            else
            {
                con = ParseSingle(s.Substring(start, end - start), useThisMaterialInsteadOfOpenEditor);
            }

            if (isInverted)
            {
                DefineableCondition inverted = new DefineableCondition();
                inverted._materialInsteadOfEditor = useThisMaterialInsteadOfOpenEditor;
                inverted.type = DefineableConditionType.NOT;
                inverted.condition1 = con;
                return inverted;
            }

            return con;
        }

        static bool IsInverted(string s, ref int start)
        {
            for (int i = start; i < s.Length; i++)
            {
                if (s[i] == '!')
                {
                    start += 1;
                    return true;
                }
                if (s[i] != ' ')
                    return false;
            }
            return false;
        }

        static DefineableCondition ParseSingle(string s, Material useThisMaterialInsteadOfOpenEditor = null)
        {
            // Debug.Log("Parsing single: " + s);

            DefineableCondition con = new DefineableCondition
            {
                _materialInsteadOfEditor = useThisMaterialInsteadOfOpenEditor
            };

            if (s.IndexOfAny(ComparissionLiteralsToCheckFor) != -1)
            {
                //is a comparission
                con.data = s;
                con.type = DefineableConditionType.PROPERTY_BOOL;
                if (s.StartsWith("VRCSDK", StringComparison.Ordinal))
                {
                    con.type = DefineableConditionType.VRC_SDK_VERSION;
                    con.data = s.Replace("VRCSDK", "");
                }
                else if (s.StartsWith("ThryEditor", StringComparison.Ordinal))
                {
                    con.type = DefineableConditionType.EDITOR_VERSION;
                    con.data = s.Replace("ThryEditor", "");
                }
                else if (IsTextureNullComparission(s, useThisMaterialInsteadOfOpenEditor))
                {
                    con.type = DefineableConditionType.TEXTURE_SET;
                    con.data = s.Replace("TEXTURE_SET", "");
                }
                return con;
            }
            if (s.StartsWith("isNotAnimated(", StringComparison.Ordinal))
            {
                con.type = DefineableConditionType.PROPERTY_IS_NOT_ANIMATED;
                con.data = s.Replace("isNotAnimated(", "").TrimEnd(')');
                return con;
            }
            if (s.StartsWith("isAnimated(", StringComparison.Ordinal))
            {
                con.type = DefineableConditionType.PROPERTY_IS_ANIMATED;
                con.data = s.Replace("isAnimated(", "").TrimEnd(')');
                return con;
            }
            if (s.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                con.type = DefineableConditionType.TRUE;
                return con;
            }
            if (s.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                con.type = DefineableConditionType.FALSE;
                return con;
            }
            return con;
        }

        static bool IsTextureNullComparission(string data, Material useThisMaterialInsteadOfOpenEditor = null)
        {
            // Check if property is a texture property && is checking for null
            Material m = GetReferencedMaterial(useThisMaterialInsteadOfOpenEditor);
            if (m == null) return false;
            if (data.Length < 7) return false;
            if (data.EndsWith("null") == false) return false;
            string propertyName = data.Substring(0, data.Length - 6);
            if (m.HasProperty(propertyName) == false) return false;
            MaterialProperty p = MaterialEditor.GetMaterialProperty(new Material[] { m }, propertyName);
            return p.type == MaterialProperty.PropType.Texture;
        }

        static Material GetReferencedMaterial(Material useThisMaterialInsteadOfOpenEditor = null)
        {
            if (useThisMaterialInsteadOfOpenEditor != null) return useThisMaterialInsteadOfOpenEditor;
            if (ShaderEditor.Active != null) return ShaderEditor.Active.Materials[0];
            return null;
        }
    }

    enum CompareType { NONE, BIGGER, SMALLER, EQUAL, NOT_EQUAL, BIGGER_EQ, SMALLER_EQ }

    public enum DefineableConditionType
    {
        NONE,
        TRUE,
        FALSE,
        PROPERTY_BOOL,
        PROPERTY_IS_ANIMATED,
        PROPERTY_IS_NOT_ANIMATED,
        EDITOR_VERSION,
        VRC_SDK_VERSION,
        TEXTURE_SET,
        DROPDOWN,
        AND,
        OR,
        NOT
    }

}