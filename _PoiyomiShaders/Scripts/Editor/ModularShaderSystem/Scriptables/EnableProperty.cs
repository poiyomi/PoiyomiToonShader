using System;
using System.Collections.Generic;

namespace Poiyomi.ModularShaderSystem
{
    [Serializable]
    public class EnableProperty : Property, IEquatable<EnableProperty>
    {
        public int EnableValue;

        public EnableProperty(string name, string displayName, int enableValue)
        {
            Name = name;
            DisplayName = displayName;
            Type = "Float";
            DefaultValue = "0.1";
            Attributes = new List<string>();

            EnableValue = enableValue;
        }
        
        public override Variable ToVariable()
        {
            Variable variable = new Variable();
            variable.Name = Name;
            variable.Type = VariableType.Float;
            return variable;
        }

        public EnableProperty(string name, int enableValue) : this(name, name, enableValue){}

        bool IEquatable<EnableProperty>.Equals(EnableProperty other)
        {
            return Equals(other);
        }
        
        public override bool Equals(object obj)
        {
            if (obj is Property other)
                return Name == other.Name;

            return false;
        }

        public static bool operator == (EnableProperty left, EnableProperty right)
        {
            return left?.Equals(right) ?? right is null;
        }

        public static bool operator !=(EnableProperty left, EnableProperty right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public new EnableProperty DeepCopy()
        {
            EnableProperty copy = new EnableProperty(Name, DisplayName, EnableValue);
            copy.Name = Name;
            copy.DisplayName = DisplayName;
            copy.Type = Type;
            copy.DefaultValue = DefaultValue;
            copy.DefaultTextureAsset = DefaultTextureAsset;
            copy.Attributes = new List<string>(Attributes);
            copy.EnableValue = EnableValue;
            return copy;
        }
    }
}