using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace Thry
{
    public class MaterialHelper
    {
        public static void ToggleKeyword(Material material, string keyword, bool turn_on)
        {
            bool is_on = material.IsKeywordEnabled(keyword);
            if (is_on && !turn_on)
                material.DisableKeyword(keyword);
            else if (!is_on && turn_on)
                material.EnableKeyword(keyword);
        }

        public static void ToggleKeyword(Material[] materials, string keyword, bool on)
        {
            foreach (Material m in materials)
                ToggleKeyword(m, keyword, on);
        }

        public static void ToggleKeyword(MaterialProperty p, string keyword, bool on)
        {
            ToggleKeyword(p.targets as Material[], keyword, on);
        }

        /// <summary>
        /// Set Material Property value or Renderqueue of current Editor.
        /// </summary>
        /// <param name="key">Property Name or "render_queue"</param>
        /// <param name="value"></param>
        public static void SetMaterialValue(string key, string value)
        {
            Material[] materials = ShaderEditor.Active.Materials;
            if (ShaderEditor.Active.PropertyDictionary.TryGetValue(key, out ShaderProperty p))
            {
                MaterialHelper.SetMaterialPropertyValue(p.MaterialProperty, value);
                p.UpdateKeywordFromValue();
            }
            else if (key == "render_queue")
            {
                int q = 0;
                if (int.TryParse(value, out q))
                {
                    foreach (Material m in materials) m.renderQueue = q;
                }
            }
            else if (key == "render_type")
            {
                foreach (Material m in materials) m.SetOverrideTag("RenderType", value);
            }
            else if (key == "preview_type")
            {
                foreach (Material m in materials) m.SetOverrideTag("PreviewType", value);
            }
            else if (key == "ignore_projector")
            {
                foreach (Material m in materials) m.SetOverrideTag("IgnoreProjector", value);
            }
        }

        public static void SetMaterialPropertyValue(MaterialProperty p, string value)
        {
            object prev = null;
            if (p.type == MaterialProperty.PropType.Texture)
            {
                prev = p.textureValue;
                p.textureValue = AssetDatabase.LoadAssetAtPath<Texture>(value);
            }
            else if (p.type == MaterialProperty.PropType.Float || p.type == MaterialProperty.PropType.Range)
            {
                prev = p.floatValue;
                p.floatValue = Parser.ParseFloat(value, p.floatValue);
            }
#if UNITY_2022_1_OR_NEWER
            else if (p.type == MaterialProperty.PropType.Int)
            {
                prev = p.intValue;
                p.intValue = (int)Parser.ParseFloat(value, p.intValue);
            }
#endif
            else if (p.type == MaterialProperty.PropType.Vector)
            {
                prev = p.vectorValue;
                p.vectorValue = Converter.StringToVector(value);
            }
            else if (p.type == MaterialProperty.PropType.Color)
            {
                prev = p.colorValue;
                p.colorValue = Converter.StringToColor(value);
            }
            if (p.applyPropertyCallback != null)
                p.applyPropertyCallback.Invoke(p, 1, prev);
        }

        public static void CopyPropertyValueFromMaterial(MaterialProperty p, Material source)
        {
            if (!source.HasProperty(p.name)) return;
            object prev = null;
            switch (p.type)
            {
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    prev = p.floatValue;
                    p.floatValue = source.GetNumber(p);
                    break;
#if UNITY_2022_1_OR_NEWER
                case MaterialProperty.PropType.Int:
                    prev = p.intValue;
                    p.intValue = source.GetInt(p.name);
                    break;
#endif
                case MaterialProperty.PropType.Color:
                    prev = p.colorValue;
                    p.colorValue = source.GetColor(p.name);
                    break;
                case MaterialProperty.PropType.Vector:
                    prev = p.vectorValue;
                    p.vectorValue = source.GetVector(p.name);
                    break;
                case MaterialProperty.PropType.Texture:
                    prev = p.textureValue;
                    p.textureValue = source.GetTexture(p.name);
                    Vector2 offset = source.GetTextureOffset(p.name);
                    Vector2 scale = source.GetTextureScale(p.name);
                    p.textureScaleAndOffset = new Vector4(scale.x, scale.y, offset.x, offset.y);
                    break;
            }
            if (p.applyPropertyCallback != null)
                p.applyPropertyCallback.Invoke(p, 1, prev);
        }

        public static void CopyMaterialValueFromProperty(MaterialProperty target, MaterialProperty source)
        {
            object prev = null;
            switch (target.type)
            {
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    prev = target.floatValue;
                    target.floatValue = source.floatValue;
                    break;
#if UNITY_2022_1_OR_NEWER
                case MaterialProperty.PropType.Int:
                    prev = target.intValue;
                    target.intValue = source.intValue;
                    break;
#endif
                case MaterialProperty.PropType.Color:
                    prev = target.colorValue;
                    target.colorValue = source.colorValue;
                    break;
                case MaterialProperty.PropType.Vector:
                    prev = target.vectorValue;
                    target.vectorValue = source.vectorValue;
                    break;
                case MaterialProperty.PropType.Texture:
                    prev = target.textureValue;
                    target.textureValue = source.textureValue;
                    target.textureScaleAndOffset = source.textureScaleAndOffset;
                    break;
            }
            if (target.applyPropertyCallback != null)
                target.applyPropertyCallback.Invoke(target, 1, prev);
        }

        public static void CopyPropertyValueToMaterial(MaterialProperty source, Material target)
        {
            CopyMaterialValueFromProperty(MaterialEditor.GetMaterialProperty(new Material[] { target }, source.name), source);
        }
    }

}