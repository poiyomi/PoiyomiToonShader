// ScriptedShaderTranslator.cs by Pumkin
// https://github.com/rurre/

using System;
using System.Collections.Generic;
using System.Linq;
using Thry;
using UnityEditor;
using UnityEngine;

namespace Poi.Tools.ShaderTranslator
{
    public abstract class ScriptedShaderTranslator
    {
        protected enum PoiShaderRenderingPreset
        {
            Opaque = 0,
            Cutout = 1,
            TransClipping = 9,
            Fade = 2,
            Transparent = 3,
            Additive = 4,
            SoftAdditive = 5,
            Multiplicative = 6,
            DoubleMultiplicative = 7
        }

        protected enum PoiUvMirrorMode { Off, Flip, LeftOnly, RightOnly, FlipRightOnly }

        protected enum PoiUvSymmetryMode { Off, Symmetry, Flipped }

        protected ScriptedShaderTranslator()
        {
            PropertyTranslations.AddRange(AddProperties());
        }

        protected ShaderRepresentation SourceShader { get; set; }
        protected ShaderRepresentation TargetShader { get; set; }

        protected List<PropertyTranslation> PropertyTranslations { get; set; } = new List<PropertyTranslation>();

        protected abstract List<PropertyTranslation> AddProperties();

        /// <summary>
        /// Returns target shader from name. Can be overridden to modify what shader is returned if needed.
        /// </summary>
        /// <param name="sourceMaterial">The material being translated</param>
        /// <param name="newShaderName">The name of the shader we're translating to</param>
        /// <returns></returns>
        protected virtual Shader GetTargetShader(Material sourceMaterial, string newShaderName)
        {
            return Shader.Find(newShaderName);
        }

        /// <summary>
        /// Conditional that's checked before everything else.
        /// </summary>
        /// <param name="sourceMaterial">Material to check</param>
        /// <returns>True to continue and false to cancel translation</returns>
        public virtual bool CanTranslateMaterial(Material sourceMaterial)
        {
            return true;
        }

        /// <summary>
        /// Switch shader and translate material
        /// </summary>
        /// <param name="sourceMaterial">Material to translate</param>
        /// <param name="newShaderName">Shader to translate to</param>
        public void Translate(Material sourceMaterial, string newShaderName)
        {
            if(!CanTranslateMaterial(sourceMaterial))
                return;

            Shader newShader = GetTargetShader(sourceMaterial, newShaderName);
            if(!newShader)
            {
                Debug.LogError($"Translation failed. Can't find shader <b>{newShader}</b> in project.");
                return;
            }

            SourceShader = new ShaderRepresentation(sourceMaterial.shader);
            TargetShader = new ShaderRepresentation(newShader);

#if UNITY_2022_1_OR_NEWER
            if(sourceMaterial.isVariant)
                sourceMaterial.parent = null;
#endif

            var context = new TranslationContext()
            {
                Material = sourceMaterial,
                originalRenderQueue = sourceMaterial.renderQueue,
                SourcePropertiesAndValues = SourceShader.GetPropertiesWithValues(sourceMaterial),
                ThryShaderEditor = new ShaderEditor()
            };

            context.ThryShaderEditor.SetShader(newShader, sourceMaterial.shader); // Seems like this needs to run first
            sourceMaterial.shader = newShader; // Then we need to actually switch shader

            context.ThryShaderEditor.FakePartialInitilizationForLocaleGathering(newShader);
            context.ThryShaderEditor.Materials[0] = sourceMaterial;

            Debug.Log($"Translating material <b>{sourceMaterial.name}</b> to <b>{newShaderName}</b>");

            DoBeforeTranslation(context);
            RunAutomaticTranslations(context);
            DoAfterTranslation(context);
        }

        /// <summary>
        /// Translate properties automatically, based on condition and OnTranslate delegates
        /// </summary>
        /// <param name="context"></param>
        protected virtual void RunAutomaticTranslations(TranslationContext context)
        {
            foreach(var propertyTranslation in PropertyTranslations)
            {
                var sourceProperty = SourceShader[propertyTranslation.SourceName];
                try
                {
                    if(sourceProperty == null || propertyTranslation.Condition?.Invoke(context) == false)
                        continue;
                }
                catch(Exception ex)
                {
                    Debug.LogError($"Condition for <b>{sourceProperty.name}</b> -> <b>{propertyTranslation.TargetName}</b> threw an exception");
                    Debug.LogException(ex);
                }

                try
                {
                    propertyTranslation.OnTranslate?.Invoke(sourceProperty, context);
                }
                catch(Exception ex)
                {
                    Debug.LogError($"OnTranslate for <b>{sourceProperty.name}</b> -> <b>{propertyTranslation.TargetName}</b> threw an exception");
                    Debug.LogException(ex);
                }

                if(!string.IsNullOrWhiteSpace(propertyTranslation.TargetName))
                    SetTargetPropertyValue(context, propertyTranslation.TargetName, GetSourcePropertyValue<object>(context, sourceProperty));
            }
        }

        /// <summary>
        /// Run this before any automatic translations. All the manual translation logic goes here. Since it runs before everything else, things can get overwritten by automatic translations.
        /// </summary>
        /// <param name="context"></param>
        protected virtual void DoBeforeTranslation(TranslationContext context)
        {
            // Do nothing by default
        }

        /// <summary>
        /// Run this after automatic translations. All the manual translation logic goes here if you want override anything the automatic translations did.
        /// </summary>
        /// <param name="context"></param>
        protected virtual void DoAfterTranslation(TranslationContext context)
        {
            // Do nothing by default
        }

        /// <summary>
        /// Directly sets the value of a target property using Thry's editor. This ignores the property value in the <paramref name="context"/>
        /// </summary>
        /// <param name="context">Translation context containing all properties and values</param>
        /// <param name="propertyName">Name of property in target material</param>
        /// <param name="value">The value</param>
        protected void SetTargetPropertyValue(TranslationContext context, string propertyName, object value)
        {
            bool isTextureSTValue = propertyName.EndsWith("_ST");
            if(isTextureSTValue)
                propertyName = propertyName.Substring(0, propertyName.Length - 3);

            if(!context.ThryShaderEditor.PropertyDictionary.TryGetValue(propertyName, out var thryProperty))
                return;

            var propType = thryProperty.MaterialProperty.type;
            switch(propType)
            {
                case MaterialProperty.PropType.Color:
                    thryProperty.ColorValue = (Color)value;
                    break;
                case MaterialProperty.PropType.Vector:
                    Vector4 vectorValue = default;

                    if(value is Vector2 vec2)
                        vectorValue = new Vector4(vec2.x, vec2.y, 0, 0);
                    else if(value is Vector3 vec3)
                        vectorValue = new Vector4(vec3.x, vec3.y, vec3.z, 0);
                    else if(value is Vector4 vec4)
                        vectorValue = vec4;
                    else if(value is Color col)
                        vectorValue = new Vector4(col.r, col.g, col.b, col.a);
                    else
                        throw new InvalidCastException($"Can't cast {value.GetType()} to {typeof(Vector4)}");
                    thryProperty.VectorValue = Vector4.one; // Ok so, hear me out. Either Thry or Unity doesn't seem to like it when I'm setting a value that's 0 to 0, and when the stored shader value loads, it gets overwritten. Setting it to something else fixes it

                    thryProperty.VectorValue = vectorValue;
                    break;
                case MaterialProperty.PropType.Texture:
                    if(isTextureSTValue)
                        thryProperty.MaterialProperty.textureScaleAndOffset = (Vector4)value;
                    else
                        thryProperty.TextureValue = (Texture)value;
                    break;
#if UNITY_2022_1_OR_NEWER
                case MaterialProperty.PropType.Int:
#endif
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    thryProperty.FloatValue = 1f; // Ok so, hear me out. Either Thry or Unity doesn't seem to like it when I'm setting a value that's 0 to 0, and when the stored shader value loads, it gets overwritten. Setting it to something else fixes it
                    thryProperty.FloatValue = Convert.ToSingle(value);
                    break;
                default:
                    Debug.LogError($"Tried setting value of invalid type <b>{propType}</b> for property <b>{propertyName}</b>");
                    break;
            }
        }

        /// <summary>
        /// Gets the value of the property in the source shader and casts it to <typeparamref name="T"/>. The value is taken from the <paramref name="context"/>
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="context">The context of our translation</param>
        /// <param name="property">The property being translated</param>
        /// <returns></returns>
        protected T GetSourcePropertyValue<T>(TranslationContext context, ShaderProperty property)
        {
            if(typeof(IConvertible).IsAssignableFrom(typeof(T)))
            {
                var value = context.SourcePropertiesAndValues[property];
                return (T)Convert.ChangeType(value, typeof(T));
            }
            return (T)context.SourcePropertiesAndValues[property];
        }

        /// <summary>
        /// Gets the value of the property by name in the source shader and casts it to <typeparamref name="T"/>. The value is taken from the <paramref name="context"/>
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="context">The context of our translation</param>
        /// <param name="propertyName">The property being translated</param>
        /// <returns></returns>
        protected T GetSourcePropertyValue<T>(TranslationContext context, string propertyName)
        {
            return GetSourcePropertyValue<T>(context, context.SourcePropertiesAndValues.First(x => x.Key.name == propertyName).Key);
        }

        /// <summary>
        /// Set the value of our property in the translation context. This is used by automatic translations and other properties.
        /// WARNING: Since this overwrites the value in the context, properties that check the context for the value see the new value and not the original one from the shader.
        /// You should probably use SetContextPropertyValue to set the value in the new material directly instead. 
        /// </summary>
        /// <param name="context">The context of our translation</param>
        /// <param name="property">The property to set the value of</param>
        /// <param name="value">The value to set</param>
        protected void SetContextPropertyValue(TranslationContext context, ShaderProperty property, object value)
        {
            context.SourcePropertiesAndValues[property] = value;
        }

        /// <summary>
        /// Set the value of our property in the translation context by name. This is used by automatic translations and other properties
        /// </summary>
        /// <param name="context">The context our translation</param>
        /// <param name="propertyName">The property name to search for</param>
        /// <param name="value">The value to set</param>
        protected void SetContextPropertyValue(TranslationContext context, string propertyName, object value)
        {
            var property = context.SourcePropertiesAndValues.First(x => x.Key.name == propertyName).Key;
            context.SourcePropertiesAndValues[property] = value;
        }

        /// <summary>
        /// Sets the render queue of the material
        /// </summary>
        /// <param name="context">The context of our translation</param>
        /// <param name="queue">Render queue value</param>
        protected void SetTargetRenderQueue(TranslationContext context, int queue)
        {
            context.Material.renderQueue = queue;
        }

        /// <summary>
        /// Sets the rendering preset of our target material. Optionally only in the ui dropdown
        /// </summary>
        /// <param name="context">The context of our translation</param>
        /// <param name="renderPreset">The rendering preset</param>
        protected void SetTargetRenderingPreset(TranslationContext context, PoiShaderRenderingPreset renderPreset)
        {
            context.ThryShaderEditor.ShaderRenderingPreset = (int)renderPreset;
        }
    }
}
