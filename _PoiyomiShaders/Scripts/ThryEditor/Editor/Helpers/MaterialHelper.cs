using UnityEditor;
using UnityEngine;

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
        public static void SetValueAdvanced(string key, string value)
        {
            Material[] materials = ShaderEditor.Active.Materials;
            if (ShaderEditor.Active.PropertyDictionary.TryGetValue(key, out ShaderProperty p))
            {
                MaterialHelper.SetValue(p.MaterialProperty, value);
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

        public static void SetValue(MaterialProperty p, string value)
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

        public static void CopyValue(Material source, MaterialProperty target)
        {
            if (!source.HasProperty(target.name)) return;
            object prev = null;
            switch (target.type)
            {
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    prev = target.floatValue;
                    target.floatValue = source.GetNumber(target);
                    break;
#if UNITY_2022_1_OR_NEWER
                case MaterialProperty.PropType.Int:
                    prev = target.intValue;
                    target.intValue = source.GetInt(target.name);
                    break;
#endif
                case MaterialProperty.PropType.Color:
                    prev = target.colorValue;
                    target.colorValue = source.GetColor(target.name);
                    break;
                case MaterialProperty.PropType.Vector:
                    prev = target.vectorValue;
                    target.vectorValue = source.GetVector(target.name);
                    break;
                case MaterialProperty.PropType.Texture:
                    prev = target.textureValue;
                    target.textureValue = source.GetTexture(target.name);
                    Vector2 offset = source.GetTextureOffset(target.name);
                    Vector2 scale = source.GetTextureScale(target.name);
                    target.textureScaleAndOffset = new Vector4(scale.x, scale.y, offset.x, offset.y);
                    break;
            }
            if (target.applyPropertyCallback != null)
                target.applyPropertyCallback.Invoke(target, 1, prev);
        }

        public static void CopyValue(MaterialProperty source, MaterialProperty target)
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

        public static void CopyValue(MaterialProperty source, params Material[] targets)
        {
            CopyValue(source, MaterialEditor.GetMaterialProperty(targets, source.name));
        }

        public static object GetValue(MaterialProperty property)
        {
            switch (property.type)
            {
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    return property.floatValue;
#if UNITY_2022_1_OR_NEWER
                case MaterialProperty.PropType.Int:
                    return property.intValue;
#endif
                case MaterialProperty.PropType.Color:
                    return property.colorValue;
                case MaterialProperty.PropType.Vector:
                    return property.vectorValue;
                case MaterialProperty.PropType.Texture:
                    return property.textureValue;
            }
            return null;
        }
    }

}