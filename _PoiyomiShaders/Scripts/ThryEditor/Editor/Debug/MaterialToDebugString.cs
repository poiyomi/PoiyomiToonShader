using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor
{
    public static class MaterialToDebugString
    {
        [Serializable]
        struct MaterialDebugInfo
        {
            [Serializable]
            public struct MaterialPropertyInfo
            {
                public string propertyName;
                public object propertyValue;
                public bool propertyValueIsDefault;
                
                public int propertyDepth;

                public string IndentString
                {
                    get
                    {
                        if(_indentString == null)
                            _indentString = new string(' ', propertyDepth);
                        return _indentString;
                    }

                }

                string _indentString;

                public List<MaterialPropertyInfo> childProperties;

                public bool HasChildren => childProperties?.Count > 0;
                public override string ToString()
                {
                    if(!HasChildren)
                        return $"{IndentString}{propertyName}: {propertyValue}";
                    
                    StringBuilder sb = new StringBuilder($"{propertyName}: {propertyValue}\n");
                    foreach(var child in childProperties)
                        sb.AppendLine($"{child.IndentString}{child}");
                    return sb.ToString();
                }
            }

            [Serializable]
            public struct MaterialMetaInfo
            {
                public string unityVersion;
                public string shaderName;
                public string shaderGuid;
                public string materialName;
                
                public override string ToString()
                {
                    return $"Unity: {unityVersion}\nShader: {shaderName}\nShaderGuid: {shaderGuid}\nMaterial: {materialName}";
                }
            }

            public MaterialMetaInfo metaInfo;
            public List<MaterialPropertyInfo> materialProperties;

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(metaInfo.ToString());
                sb.AppendLine();
                
                foreach(var child in materialProperties)
                    sb.AppendLine(child.ToString());
                
                return sb.ToString();
            }
        }

        public static string ConvertMaterialToDebugString(Material material, bool onlyNonDefaultProperties)
        {
            var editor = Editor.CreateEditor(material) as MaterialEditor;
            var shaderGui = editor.customShaderGUI as ShaderEditor;
            shaderGui.SetShader(material.shader);
            shaderGui.FakePartialInitilizationForLocaleGathering(material.shader);
            
            return ConvertMaterialToDebugString(shaderGui, onlyNonDefaultProperties);
        }

        public static string ConvertMaterialToDebugString(ShaderEditor thryEditor, bool onlyNonDefaultProperties)
        {
            var material = thryEditor.Materials[0];
            var info = new MaterialDebugInfo
            {
                metaInfo =
                {
                    unityVersion = Application.unityVersion,
                    shaderName = material.shader.name,
                    shaderGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(material.shader)),
                    materialName = material.name,
                }
            };
            
            info.materialProperties = thryEditor.ShaderParts
                .Where(x => x is ShaderGroup)
                .Where(x => IsValidShaderPart(x, onlyNonDefaultProperties))
                .Select(x => ShaderPartToMaterialPropertyInfo(x, onlyNonDefaultProperties))
                .ToList();
            
            return info.ToString();
        }

        static bool IsValidShaderPart(ShaderPart shaderPart, bool onlyNonDefaultProperties)
        {
            if(shaderPart.IsHidden)
                return false;
            if(shaderPart.MaterialProperty == null)
                return false;
            if(shaderPart.HasAttribute("HelpBox"))
                return false;
            if(onlyNonDefaultProperties && ShaderPartIsDefault(shaderPart))
                return false;
            
            return true;
        }

        static bool ShaderPartIsDefault(ShaderPart part)
        {
            if(part is ShaderGroup group)
                return group.IsPropertyValueDefault && group.Children.All(child => child.IsPropertyValueDefault);
            return part.IsPropertyValueDefault;
        }

        static MaterialDebugInfo.MaterialPropertyInfo ShaderPartToMaterialPropertyInfo(ShaderPart shaderPart, bool onlyNonDefaultProperties)
        {
            string propertyDisplayName;
            if(shaderPart.MaterialProperty != null)
            {
                int dashIndex = shaderPart.MaterialProperty.displayName.IndexOf("--");
                if(dashIndex == -1)
                    propertyDisplayName = shaderPart.MaterialProperty.displayName;
                else
                    propertyDisplayName = shaderPart.MaterialProperty.displayName.Substring(0,dashIndex);
            }
            else
            {
                propertyDisplayName = shaderPart.PropertyIdentifier;
            }

            var partInfo = new MaterialDebugInfo.MaterialPropertyInfo()
            {
                propertyName = propertyDisplayName,
                propertyValue = shaderPart.PropertyValue,
            };

            ShaderPart parentShaderPart = shaderPart.Parent;
            while(parentShaderPart != null)
            {
                parentShaderPart = parentShaderPart.Parent;
                partInfo.propertyDepth++;
            }

            if(shaderPart is ShaderGroup group && group.Children != null)
            {
                partInfo.childProperties = group.Children
                    .Where(x => IsValidShaderPart(x, onlyNonDefaultProperties))
                    .Select(x => ShaderPartToMaterialPropertyInfo(x, onlyNonDefaultProperties))
                    .ToList();
            }

            return partInfo;
        }
    }
}