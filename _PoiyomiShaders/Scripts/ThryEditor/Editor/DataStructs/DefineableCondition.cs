using System;
using Thry.ThryEditor.Helpers;
using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor
{
    public abstract class DefineableCondition
    {
        private enum CombinationType { AND, OR }
        private enum ComparisonType { NONE, BIGGER, SMALLER, EQUAL, NOT_EQUAL, BIGGER_EQ, SMALLER_EQ }

        public static DefineableCondition None => new BooleanCondition(null, BooleanCondition.BooleanType.NONE);

        protected class MaterialRenference
        {
            Material _material;
            bool _forceUseMaterialInsteadOfEditor;

            public Material Material => _forceUseMaterialInsteadOfEditor ? _material : ShaderEditor.Active?.Materials[0];

            static MaterialRenference _defaultMaterialRenference = new MaterialRenference(null);
            public static MaterialRenference Default => _defaultMaterialRenference;

            public MaterialRenference(Material useThisMaterialInsteadOfOpenEditor)
            {
                if (useThisMaterialInsteadOfOpenEditor != null)
                {
                    _material = useThisMaterialInsteadOfOpenEditor;
                    _forceUseMaterialInsteadOfEditor = true;
                }
                else
                {
                    _forceUseMaterialInsteadOfEditor = false;
                }
            }

            public MaterialProperty GetMaterialProperty(string key)
            {
                if (_forceUseMaterialInsteadOfEditor)
                {
                    return MaterialEditor.GetMaterialProperty(new Material[] { _material }, key);
                }
                else if (ShaderEditor.Active != null && ShaderEditor.Active.PropertyDictionary.TryGetValue(key, out ShaderProperty property))
                {
                    return property.MaterialProperty;
                }
                return null;
            }
        }

        private class ComparisonData
        {
            enum DataType { FLOAT, MATERIAL_PROPERTY, CONDITION, THRY_EDITOR_VERSION, VRC_SDK_VERSION, RENDERQUEUE }

            private DataType _type;
            private DefineableCondition _condition;
            private float _floatData;
            private bool _isConstant;
            private MaterialRenference _materialReference;
            private string _key;
            private bool _isErrorLogged = false;

            public object Value
            {
                get
                {
                    if (_type == DataType.FLOAT) return _floatData;
                    if (_type == DataType.CONDITION) return _condition.Test();
                    if (_type == DataType.THRY_EDITOR_VERSION) return Config.Instance.Version;
                    if (_type == DataType.VRC_SDK_VERSION)
                    {
                        if (VRCInterface.Get().Sdk_information.type == VRCInterface.VRC_SDK_Type.NONE) return 0f;
                        return VRCInterface.Get().Sdk_information.installed_version;
                    }
                    if (_type == DataType.RENDERQUEUE)
                    {
                        if (_materialReference.Material == null) return 0f;
                        return _materialReference.Material.renderQueue;
                    }
                    if (_type == DataType.MATERIAL_PROPERTY)
                    {
                        MaterialProperty prop = _materialReference.GetMaterialProperty(_key);
                        if (prop == null)
                        {
                            if (!_isErrorLogged)
                            {
                                _isErrorLogged = true;
                                ThryLogger.LogDetail(
                                    $"Failed to get material property '{_key}' from material '{_materialReference.Material?.name}' for condition. "
                                );
                            }
                            return 0f;
                        }
                        switch (prop.type)
                        {
                            case MaterialProperty.PropType.Float:
                            case MaterialProperty.PropType.Range:
                                return prop.floatValue;
                            case MaterialProperty.PropType.Texture:
                                return prop.textureValue != null ? prop.textureValue.name : "null";
                            default:
                                return prop.GetNumber();
                        }
                    }

                    return 0;
                }
            }
            public bool IsConstant => _isConstant;

            public ComparisonData(string data, MaterialRenference materialRenference)
            {
                _materialReference = materialRenference;
                if (float.TryParse(data, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _floatData))
                {
                    _type = DataType.FLOAT;
                    _isConstant = true;
                }
                else if (data == "VRCSDK")
                {
                    _type = DataType.VRC_SDK_VERSION;
                    _isConstant = true;
                }
                else if (data == "ThryEditor")
                {
                    _type = DataType.THRY_EDITOR_VERSION;
                    _isConstant = true;
                }
                else if (BooleanCondition.TryParse(data, out BooleanCondition condition, _materialReference))
                {
                    _condition = condition;
                    _type = DataType.CONDITION;
                    _isConstant = _condition.IsConstant;
                }
                else if (data.StartsWith("RenderQueue", StringComparison.OrdinalIgnoreCase))
                {
                    _type = DataType.RENDERQUEUE;
                    _isConstant = false;
                }
                else
                {
                    _type = DataType.MATERIAL_PROPERTY;
                    _isConstant = false;
                    _key = data;
                }
            }

            public ComparisonData(float value)
            {
                _type = DataType.FLOAT;
                _floatData = value;
                _isConstant = true;
            }

            public ComparisonData(DefineableCondition condition)
            {
                _type = DataType.CONDITION;
                _condition = condition;
                _isConstant = condition.IsConstant;
            }

            public override string ToString()
            {
                switch (_type)
                {
                    case DataType.FLOAT:
                        return _floatData.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    case DataType.MATERIAL_PROPERTY:
                        return _key;
                    case DataType.CONDITION:
                        return _condition.ToString();
                    case DataType.THRY_EDITOR_VERSION:
                        return "ThryEditor";
                    case DataType.VRC_SDK_VERSION:
                        return "VRCSDK";
                    case DataType.RENDERQUEUE:
                        return "RenderQueue";
                    default:
                        return "Unknown";
                }
            }
        }

        private class Comparison : DefineableCondition
        {
            private ComparisonData _left;
            private ComparisonData _right;
            private ComparisonType _compareType;
            private bool _isConstant;
            private bool _constant;

            protected override bool IsConstant => _isConstant;

            public Comparison(ComparisonData left, ComparisonData right, ComparisonType compareType)
            {
                _left = left;
                _right = right;
                _compareType = compareType;
                _isConstant = _left.IsConstant && _right.IsConstant;
                if (_isConstant) _constant = Evaluate();
            }

            public override bool Test()
            {
                if (_isConstant) return _constant;
                return Evaluate();
            }

            private bool Evaluate()
            {
                if (TryCompareGeneral(_left.Value, _right.Value, out int result))
                {
                    if (_compareType == ComparisonType.EQUAL) return result == 0;
                    if (_compareType == ComparisonType.NOT_EQUAL) return result != 0;
                    if (_compareType == ComparisonType.SMALLER) return result < 0;
                    if (_compareType == ComparisonType.BIGGER) return result > 0;
                    if (_compareType == ComparisonType.BIGGER_EQ) return result >= 0;
                    if (_compareType == ComparisonType.SMALLER_EQ) return result <= 0;
                }
                else
                {
                    ThryLogger.LogDetail(
                        $"Failed to compare values: {_left.Value} and {_right.Value}. Comparision: {this.ToString()}"
                    );
                }
                return false;
            }

            bool TryCompareGeneral(object a, object b, out int result)
            {
                result = 0;

                if (a == null || b == null)
                    return false;

                if (IsNumericType(a) && IsNumericType(b))
                {
                    double d1 = Convert.ToDouble(a);
                    double d2 = Convert.ToDouble(b);
                    result = d1.CompareTo(d2);
                    return true;
                }

                if (a.GetType() == b.GetType() && a is IComparable compA)
                {
                    result = compA.CompareTo(b);
                    return true;
                }

                return false;
            }

            bool IsNumericType(object obj)
            {
                return obj is byte || obj is sbyte
                || obj is short || obj is ushort
                || obj is int || obj is uint
                || obj is long || obj is ulong
                || obj is float || obj is double
                || obj is decimal;
            }

            public static ComparisonType TypeFromString(string s)
            {
                if (s == "==") return ComparisonType.EQUAL;
                if (s == "!=") return ComparisonType.NOT_EQUAL;
                if (s == ">=") return ComparisonType.BIGGER_EQ;
                if (s == "<=") return ComparisonType.SMALLER_EQ;
                if (s == ">") return ComparisonType.BIGGER;
                if (s == "<") return ComparisonType.SMALLER;
                return ComparisonType.NONE;
            }

            public static string TypeToString(ComparisonType type)
            {
                switch (type)
                {
                    case ComparisonType.EQUAL: return "==";
                    case ComparisonType.NOT_EQUAL: return "!=";
                    case ComparisonType.BIGGER_EQ: return ">=";
                    case ComparisonType.SMALLER_EQ: return "<=";
                    case ComparisonType.BIGGER: return ">";
                    case ComparisonType.SMALLER: return "<";
                    default: return "?";
                }
            }

            public override string ToString()
            {
                return $"{_left} {TypeToString(_compareType)} {_right}";
            }
        }

        private class BooleanCondition : DefineableCondition
        {
            public enum BooleanType
            {
                NONE,
                TRUE,
                FALSE,
                PROPERTY_IS_ANIMATED,
                PROPERTY_IS_NOT_ANIMATED
            }

            private BooleanType _type;
            private string _data;
            private MaterialRenference _materialReference;

            protected override bool IsConstant => _type == BooleanType.TRUE || _type == BooleanType.FALSE || _type == BooleanType.NONE;

            public BooleanCondition(bool value)
            {
                _type = value ? BooleanType.TRUE : BooleanType.FALSE;
            }

            public BooleanCondition(string data, BooleanType type, MaterialRenference materialReference = null)
            {
                _data = data;
                _type = type;
                _materialReference = materialReference ?? MaterialRenference.Default;
            }

            public override bool Test()
            {
                switch (_type)
                {
                    case BooleanType.TRUE:
                        return true;
                    case BooleanType.FALSE:
                        return false;
                    case BooleanType.NONE:
                        return true; // Default to true if no condition is set
                    case BooleanType.PROPERTY_IS_ANIMATED:
                        return ShaderOptimizer.IsAnimated(_materialReference.Material, _data);
                    case BooleanType.PROPERTY_IS_NOT_ANIMATED:
                        return !ShaderOptimizer.IsAnimated(_materialReference.Material, _data);
                    default:
                        throw new InvalidOperationException("Unknown boolean condition type");
                }
            }

            public static bool TryParse(string s, out BooleanCondition condition, MaterialRenference materialReference)
            {
                if (s.StartsWith("isNotAnimated(", StringComparison.Ordinal))
                {
                    condition = new BooleanCondition(
                        s.Replace("isNotAnimated(", "").TrimEnd(')', ' '),
                        BooleanCondition.BooleanType.PROPERTY_IS_NOT_ANIMATED,
                        materialReference
                    );
                    return true;
                }
                if (s.StartsWith("isAnimated(", StringComparison.Ordinal))
                {
                    condition = new BooleanCondition(
                        s.Replace("isAnimated(", "").TrimEnd(')', ' '),
                        BooleanCondition.BooleanType.PROPERTY_IS_ANIMATED,
                        materialReference
                    );
                    return true;
                }
                if (s.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    condition = new BooleanCondition(true);
                    return true;
                }
                if (s.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    condition = new BooleanCondition(false);
                    return true;
                }
                if (s.Equals("none", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(s))
                {
                    condition = new BooleanCondition(null, BooleanCondition.BooleanType.NONE);
                    return true;
                }
                condition = null;
                return false;
            }

            public override string ToString()
            {
                switch (_type)
                {
                    case BooleanType.TRUE:
                        return "true";
                    case BooleanType.FALSE:
                        return "false";
                    case BooleanType.PROPERTY_IS_ANIMATED:
                        return $"isAnimated({_data})";
                    case BooleanType.PROPERTY_IS_NOT_ANIMATED:
                        return $"isNotAnimated({_data})";
                    default:
                        return "none";
                }
            }
        }

        private class Combination : DefineableCondition
        {
            private DefineableCondition _condition1;
            private DefineableCondition _condition2;
            private CombinationType _type;
            private bool _isConstant;
            private bool _constant;

            protected override bool IsConstant => _isConstant;

            public Combination(CombinationType type, DefineableCondition condition1, DefineableCondition condition2)
            {
                _type = type;
                _condition1 = condition1;
                _condition2 = condition2;
                _isConstant = condition1.IsConstant && condition2.IsConstant;
                if (_isConstant) _constant = Evaluate();
            }

            public override bool Test()
            {
                if (_isConstant) return _constant;
                return Evaluate();
            }

            private bool Evaluate()
            {
                if (_type == CombinationType.AND) return _condition1.Test() && _condition2.Test();
                if (_type == CombinationType.OR) return _condition1.Test() || _condition2.Test();
                throw new InvalidOperationException("Invalid combination type");
            }

            public override string ToString()
            {
                string op = _type == CombinationType.AND ? "&&" : "||";
                return $"({_condition1} {op} {_condition2})";
            }
        }

        protected abstract bool IsConstant { get; }
        public abstract bool Test();

        private static DefineableCondition ParseForThryParser(string s)
        {
            return ParseInternal(s, MaterialRenference.Default);
        }

        public static DefineableCondition Parse(string s, Material useThisMaterialInsteadOfOpenEditor = null)
        {
            return ParseInternal(s, new MaterialRenference(useThisMaterialInsteadOfOpenEditor));
        }

        protected static DefineableCondition ParseInternal(string s, MaterialRenference materialRenference, int start = 0, int end = -1)
        {
            if (end == -1) end = s.Length;

            int depth = 0;
            int bracketStart = -1;
            int bracketEnd = -1;
            bool allPreviousCharsAreEmpty = true;
            for (int i = start; i < end; i++)
            {
                char c = s[i];
                if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
                {
                    if (allPreviousCharsAreEmpty) start += 1;
                    continue;
                }
                if (c == '(')
                {
                    depth += 1;
                    if (depth == 1 && allPreviousCharsAreEmpty)
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
                        return new Combination(
                            CombinationType.AND,
                            ParseInternal(s, materialRenference, start, i),
                            ParseInternal(s, materialRenference, i + (s[i + 1] == '&' ? 2 : 1), end)
                        );
                    }
                    else if (c == '|')
                    {
                        return new Combination(
                            CombinationType.OR,
                            ParseInternal(s, materialRenference, start, i),
                            ParseInternal(s, materialRenference, i + (s[i + 1] == '|' ? 2 : 1), end)
                        );
                    }
                }
                allPreviousCharsAreEmpty = false;
            }


            bool isInverted = IsInverted(s, ref start);

            // if no AND or OR was found, check for brackets
            DefineableCondition con;
            if (bracketStart != -1 && bracketEnd != -1)
            {
                con = ParseInternal(s, materialRenference, bracketStart + 1, bracketEnd);
            }
            else
            {
                con = ParseSingle(s.Substring(start, end - start), materialRenference);
            }

            if (isInverted)
            {
                return new Comparison(new ComparisonData(con), new ComparisonData(0), ComparisonType.EQUAL);
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

        private static readonly char[] ComparisonLiteralsToCheckFor = "!><=".ToCharArray();
        static DefineableCondition ParseSingle(string s, MaterialRenference materialRenference)
        {
            int compIndex = s.IndexOfAny(ComparisonLiteralsToCheckFor);
            int comLength = s[compIndex + 1] == '=' ? 2 : 1;
            if (compIndex != -1)
            {
                return new Comparison(
                    new ComparisonData(s.Substring(0, compIndex).Trim(), materialRenference),
                    new ComparisonData(s.Substring(compIndex + comLength).Trim(), materialRenference),
                    Comparison.TypeFromString(s.Substring(compIndex, comLength))
                );
            }
            if (BooleanCondition.TryParse(s, out BooleanCondition booleanCondition, materialRenference))
            {
                return booleanCondition;
            }
            return new BooleanCondition(s, BooleanCondition.BooleanType.NONE);
        }

        protected string SerializeForThryParser()
        {
            return this.ToString();
        }
    }
}