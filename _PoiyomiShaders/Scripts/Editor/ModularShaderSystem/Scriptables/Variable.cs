using System;
using System.Collections.Generic;
using UnityEngine;

namespace Poiyomi.ModularShaderSystem
{
    [Serializable]
    public class Variable : IEquatable<Variable>
    {
        public override int GetHashCode()
        {
            int hashCode =  (Name != null ? Name.GetHashCode() : 0);
            return hashCode;
        }

        public string Name;
        public VariableType Type;
        public string CustomType;

        public string GetDefinition()
        {
            switch (Type)
            {
                case VariableType.Half:
                    return $"half {Name};";
                case VariableType.Half2:
                    return $"half2 {Name};";
                case VariableType.Half3:
                    return $"half3 {Name};";
                case VariableType.Half4:
                    return $"half4 {Name};";
                case VariableType.Float:
                    return $"float {Name};";
                case VariableType.Float2:
                    return $"float2 {Name};";
                case VariableType.Float3:
                    return $"float3 {Name};";
                case VariableType.Float4:
                    return $"float4 {Name};";
                case VariableType.Sampler2D:
                    return $"sampler2D {Name};";
                case VariableType.SamplerCUBE:
                    return $"samplerCUBE {Name};";
                case VariableType.Sampler3D:
                    return $"sampler3D {Name};";
                case VariableType.Texture2D:
                    return $"Texture2D {Name};";
                case VariableType.Texture2DArray:
                    return $"Texture2DArray {Name};";
                case VariableType.Texture2DMS:
                    return $"Texture2DMS {Name};";
                case VariableType.TextureCube:
                    return $"TextureCube {Name};";
                case VariableType.TextureCubeArray:
                    return $"TextureCubeArray {Name};";
                case VariableType.Texture3D:
                    return $"Texture3D {Name};";
                case VariableType.UnityTex2D:
                    return $"UNITY_DECLARE_TEX2D({Name});";
                case VariableType.UnityTex2DNoSampler:
                    return $"UNITY_DECLARE_TEX2D_NOSAMPLER({Name});";
                case VariableType.UnityTexCube:
                    return $"UNITY_DECLARE_TEXCUBE({Name});";
                case VariableType.UnityTexCubeNoSampler:
                    return $"UNITY_DECLARE_TEXCUBE_NOSAMPLER({Name});";
                case VariableType.UnityTex3D:
                    return $"UNITY_DECLARE_TEX3D({Name});";
                case VariableType.UnityTex3DNoSampler:
                    return $"UNITY_DECLARE_TEX3D_NOSAMPLER({Name});";
                case VariableType.UnityTex2DArray:
                    return $"UNITY_DECLARE_TEX2DARRAY({Name});";
                case VariableType.UnityTex2DArrayNoSampler:
                    return $"UNITY_DECLARE_TEX2DARRAY_NOSAMPLER({Name});";
                case VariableType.UnityTexCubeArray:
                    return $"UNITY_DECLARE_TEXCUBEARRAY({Name});";
                case VariableType.UnityTexCubeArrayNoSampler:
                    return $"UNITY_DECLARE_TEXCUBEARRAY_NOSAMPLER({Name});";
                case VariableType.Int:
                    return $"int {Name};";
                case VariableType.Custom:
                    return $"{CustomType} {Name};";
            }

            return "";
        }
        
        public override bool Equals(object obj)
        {
            return Equals(obj as Variable);
        }

        public bool Equals(Variable other)
        {
            return other != null &&
                   Name.Equals(other.Name);
        }

        public static bool operator ==(Variable left, Variable right)
        {
            return EqualityComparer<Variable>.Default.Equals(left, right);
        }

        public static bool operator !=(Variable left, Variable right)
        {
            return !(left == right);
        }

        public Variable DeepCopy()
        {
            Variable copy = new Variable();
            copy.Name = Name;
            copy.Type = Type;
            copy.CustomType = CustomType;
            return copy;
        }
    }
    
    public enum VariableType
    {
        Half,
        Half2,
        Half3,
        Half4,
        Float,
        Float2,
        Float3,
        Float4,
        Sampler2D,
        SamplerCUBE,
        Sampler3D,
        Texture2D,
        Texture2DArray,
        Texture2DMS,
        TextureCube,
        TextureCubeArray,
        Texture3D,
        UnityTex2D,
        UnityTex2DNoSampler,
        UnityTexCube,
        UnityTexCubeNoSampler,
        UnityTex3D,
        UnityTex3DNoSampler,
        UnityTex2DArray,
        UnityTex2DArrayNoSampler,
        UnityTexCubeArray,
        UnityTexCubeArrayNoSampler,
        Int,
        Custom = 999
    }
}