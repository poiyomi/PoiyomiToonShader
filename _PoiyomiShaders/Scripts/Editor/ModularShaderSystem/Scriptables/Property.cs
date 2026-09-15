using System;
using System.Collections.Generic;
using UnityEngine;
using Poiyomi.ModularShaderSystem.UI;

namespace Poiyomi.ModularShaderSystem
{
    public enum PropertyType
    {
        Float,
        Int,
        Range,
        Vector,
        Color,
        Texture2D,
        Texture2DArray,
        Cube,
        CubeArray,
        Texture3D
    }
    
    [Serializable]
    public class Property : IEquatable<Property>
    {
        public string Name;
        
        public string DisplayName;
        
        public string Type;
        
        public string DefaultValue;

        public Texture DefaultTextureAsset;
        
        [PropertyAttribute]
        public List<string> Attributes;

        public virtual Variable ToVariable()
        {
            var variable = new Variable();
            variable.Name = Name;

            switch(Type)
            {
                case "Float": variable.Type = VariableType.Float; break;
                case "Int": variable.Type = VariableType.Float; break;
                case "Color": variable.Type = VariableType.Float4; break;
                case "Vector": variable.Type = VariableType.Float4; break;
                case "2D": variable.Type = VariableType.Texture2D; break;
                case "3D": variable.Type = VariableType.Texture3D; break;
                case "Cube": variable.Type = VariableType.TextureCube; break;
                case "2DArray": variable.Type = VariableType.Texture2DArray; break;
                case "CubeArray": variable.Type = VariableType.TextureCubeArray; break;
                default: variable.Type = Type.StartsWith("Range") ? VariableType.Float : VariableType.Custom; break;
            }

            return variable;
        }

        public override bool Equals(object obj)
        {
            if (obj is Property other)
                return Name == other.Name;

            return false;
        }
        
        bool IEquatable<Property>.Equals(Property other)
        {
            return Equals(other);
        }

        public static bool operator == (Property left, Property right)
        {
            return left?.Equals(right) ?? right is null;
        }

        public static bool operator !=(Property left, Property right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            int hashCode = (Name != null ? Name.GetHashCode() : 0);
            return hashCode;
        }

        public Property DeepCopy()
        {
            Property copy = new Property();
            copy.Name = Name;
            copy.DisplayName = DisplayName;
            copy.Type = Type;
            copy.DefaultValue = DefaultValue;
            copy.DefaultTextureAsset = DefaultTextureAsset;
            copy.Attributes = new List<string>(Attributes);
            return copy;
        }
    }
}