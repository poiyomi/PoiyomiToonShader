// ScriptedShaderTranslator.cs by Pumkin
// https://github.com/rurre/
//
// Modified by BluWizard LABS
// https://github.com/BluWizard10

using System;
using System.Collections.Generic;
using System.Linq;
using Thry;
using Thry.ThryEditor.Helpers;
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
        /// Public accessor for GetTargetShader, used by upgrade controllers for deferred shader swapping
        /// </summary>
        public Shader ResolveTargetShader(Material sourceMaterial, string newShaderName = "")
        {
            return GetTargetShader(sourceMaterial, newShaderName);
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
            Translate(sourceMaterial, newShaderName, false);
        }

        /// <summary>
        /// Switch shader and translate material
        /// </summary>
        /// <param name="sourceMaterial">Material to translate</param>
        /// <param name="newShaderName">Shader to translate to</param>
        /// <param name="deferShaderSwap">If true, shader swap is deferred - call ApplyDeferredShaderSwap after all translations.
        /// When deferring, caller is responsible for validating CanTranslateMaterial before calling.</param>
        public void Translate(Material sourceMaterial, string newShaderName, bool deferShaderSwap)
        {
            Translate(sourceMaterial, newShaderName, deferShaderSwap, null);
        }

        /// <summary>
        /// Switch shader and translate material
        /// </summary>
        /// <param name="sourceMaterial">Material to translate</param>
        /// <param name="newShaderName">Shader to translate to</param>
        /// <param name="deferShaderSwap">If true, shader swap is deferred - call ApplyDeferredShaderSwap after all translations.
        /// When deferring, caller is responsible for validating CanTranslateMaterial before calling.</param>
        /// <param name="sourceShaderOverride">If provided, the source property layout is read from this shader instead of the
        /// material's current shader. Required when the source values must be read against a shader the material is no longer
        /// assigned to - e.g. mid-chain steps with a deferred swap, or after the inspector already swapped to the new shader.</param>
        public void Translate(Material sourceMaterial, string newShaderName, bool deferShaderSwap, Shader sourceShaderOverride)
        {
            // Skip CanTranslateMaterial when deferring - caller validates the chain
            if (!deferShaderSwap && !CanTranslateMaterial(sourceMaterial)) return;

            Shader newShader = GetTargetShader(sourceMaterial, newShaderName);
            if (!newShader)
            {
                ThryLogger.LogErr($"Translation failed. Can't find shader in project. Material: <b>{sourceMaterial.name}</b>, Source shader: <b>{sourceMaterial.shader.name}</b>");
                return;
            }

            // The source layout may differ from the material's current shader (mid-chain deferred steps, or an
            // already-applied inspector swap), so honour the override when one is supplied.
            Shader effectiveSourceShader = sourceShaderOverride != null ? sourceShaderOverride : sourceMaterial.shader;

            SourceShader = new ShaderRepresentation(effectiveSourceShader);
            TargetShader = new ShaderRepresentation(newShader);

#if UNITY_2022_1_OR_NEWER
            if (sourceMaterial.isVariant) sourceMaterial.parent = null;
#endif

            var context = new TranslationContext()
            {
                Material = sourceMaterial,
                originalRenderQueue = sourceMaterial.renderQueue,
                SourcePropertiesAndValues = SourceShader.GetPropertiesWithValues(sourceMaterial),
                ThryShaderEditor = new ShaderEditor(),
                DeferredTargetShader = deferShaderSwap ? newShader : null
            };

            context.ThryShaderEditor.SetShader(newShader, effectiveSourceShader);

            if(!deferShaderSwap) sourceMaterial.shader = newShader;

            context.ThryShaderEditor.FakePartialInitilizationForLocaleGathering(newShader);
            context.ThryShaderEditor.Materials[0] = sourceMaterial;

            ThryLogger.Log($"Translating material <b>{sourceMaterial.name}</b> to <b>{newShader.name}</b>{(deferShaderSwap ? " (shader swap deferred)" : "")}");

            DoBeforeTranslation(context);
            RunAutomaticTranslations(context);
            DoAfterTranslation(context);

            if(!deferShaderSwap)
            {
                ShaderEditor.FixKeywords(new Material[] { sourceMaterial });
                
                // Drop properties orphaned from the translation since the ShaderOptimizer does
                // was not designed to strip them away on lock. Important for VRAM!
                RemoveOrphanedProperties(sourceMaterial, newShader);
            }
        }

        /// <summary>
        /// Apply deferred shader swap and fix keywords. Call after all deferred translations are complete.
        /// </summary>
        public static void ApplyDeferredShaderSwap(Material material, Shader targetShader)
        {
            if(material == null || targetShader == null)
                return;

            int renderQueue = material.renderQueue;
            material.shader = targetShader;
            material.renderQueue = renderQueue;
            ShaderEditor.FixKeywords(new Material[] { material });

            // Drop properties orphaned by the swap so they don't linger on the material.
            RemoveOrphanedProperties(material, targetShader);
        }

        /// <summary>
        /// Translates a material that has already been swapped to "targetShader" - e.g. the user
        /// picked a Poiyomi shader from the inspector's shader dropdown while the material was still using lilToon. The
        /// old property values are still serialized on the material, so the source layout is read fromm"sourceShader"
        /// and the material is left on "targetShader" (no target re-resolution, no extra shader swap) - the shader
        /// the user picked is honoured exactly. Orphaned properties left behind by the swap are purged afterwards.
        /// </summary>
         public void TranslateAcrossShaderSwap(Material material, Shader sourceShader, Shader targetShader)
        {
            if (material == null || sourceShader == null || targetShader == null || sourceShader == targetShader) return;

            SourceShader = new ShaderRepresentation(sourceShader);
            TargetShader = new ShaderRepresentation(targetShader);

#if UNITY_2022_1_OR_NEWER
            if (material.isVariant) material.parent = null;
#endif

            var context = new TranslationContext()
            {
                Material = material,
                originalRenderQueue = material.renderQueue,
                SourcePropertiesAndValues = SourceShader.GetPropertiesWithValues(material),
                ThryShaderEditor = new ShaderEditor(),
                DeferredTargetShader = targetShader,
            };

            // Material already references targetShader, so read the source layout
            // from sourceShader and never swap again.
            context.ThryShaderEditor.SetShader(targetShader, sourceShader);
            context.ThryShaderEditor.FakePartialInitilizationForLocaleGathering(targetShader);
            context.ThryShaderEditor.Materials[0] = material;

            ThryLogger.Log($"Translating material {material.name} from {sourceShader.name} to {targetShader.name} (shader already swapped)");

            DoBeforeTranslation(context);
            RunAutomaticTranslations(context);
            DoAfterTranslation(context);

            // Material already points at the target shader. Fix keywords and Render Queue + purge orphans.
            ApplyDeferredShaderSwap(material, targetShader);
        }

        /// <summary>
        /// Translate properties automatically, based on condition and OnTranslate delegates
        /// </summary>
        /// <param name="context"></param>
        protected virtual void RunAutomaticTranslations(TranslationContext context)
        {
            foreach(var propertyTranslation in PropertyTranslations)
            {
                string label = $"<b>{propertyTranslation.SourceName}</b> -> <b>{(string.IsNullOrWhiteSpace(propertyTranslation.TargetName) ? "(custom logic)" : propertyTranslation.TargetName)}</b>";

                var sourceProperty = SourceShader[propertyTranslation.SourceName];
                if (sourceProperty == null)
                {
                    ThryLogger.LogDetail($"Skipped {label}: source property not present on this material.");
                    continue;
                }

                // A condition that throws or returns false must skip the translation entirely - it must not
                // fall through to OnTranslate or the value copy below.
                try
                {
                    if (propertyTranslation.Condition?.Invoke(context) == false)
                    {
                        ThryLogger.LogDetail($"Skipped {label}: condition returned false.");
                        continue;
                    }
                }
                catch(Exception ex)
                {
                    ThryLogger.LogErr($"Failed translating {label}: condition threw an exception! {ex.GetType().Name}.");
                    Debug.LogException(ex);
                    continue;
                }

                try
                {
                    propertyTranslation.OnTranslate?.Invoke(sourceProperty, context);
                }
                catch(Exception ex)
                {
                    ThryLogger.LogErr($"Failed translating {label}: OnTranslate threw an exception! {ex.GetType().Name}.");
                    Debug.LogException(ex);
                    continue;
                }

                if(!string.IsNullOrWhiteSpace(propertyTranslation.TargetName)) SetTargetPropertyValue(context, propertyTranslation.TargetName, GetSourcePropertyValue<object>(context, sourceProperty));

                ThryLogger.LogDetail($"Translation of property {label} successful!");
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

#if UNITY_6000_2_OR_NEWER
            var propType = thryProperty.MaterialProperty.propertyType;
#else
            var propType = thryProperty.MaterialProperty.type;
#endif
            switch (propType)
            {
#if UNITY_6000_2_OR_NEWER
                case UnityEngine.Rendering.ShaderPropertyType.Color:
#else
                case MaterialProperty.PropType.Color:
#endif
                    Color colorValue = (Color)value;
                    thryProperty.ColorValue = colorValue == Color.white ? Color.black : Color.white;
                    thryProperty.ColorValue = colorValue;
                    break;
#if UNITY_6000_2_OR_NEWER
                case UnityEngine.Rendering.ShaderPropertyType.Vector:
#else
                case MaterialProperty.PropType.Vector:
#endif
                    Vector4 vectorValue = default;

                    if (value is Vector2 vec2)
                        vectorValue = new Vector4(vec2.x, vec2.y, 0, 0);
                    else if (value is Vector3 vec3)
                        vectorValue = new Vector4(vec3.x, vec3.y, vec3.z, 0);
                    else if (value is Vector4 vec4)
                        vectorValue = vec4;
                    else if (value is Color col)
                        vectorValue = new Vector4(col.r, col.g, col.b, col.a);
                    else
                        throw new InvalidCastException($"Can't cast {value.GetType()} to {typeof(Vector4)}");
                    // Ok so, hear me out. Either Thry or Unity doesn't seem to like it when I'm setting a value that's 0 to 0, and when the stored shader value loads, it gets overwritten. Setting it to something else fixes it
                    thryProperty.VectorValue = vectorValue == Vector4.one ? Vector4.zero : Vector4.one; 
                    thryProperty.VectorValue = vectorValue;
                    break;
#if UNITY_6000_2_OR_NEWER
                case UnityEngine.Rendering.ShaderPropertyType.Texture:
#else
                case MaterialProperty.PropType.Texture:
#endif
                    if (isTextureSTValue)
                    {
                        var stValue = (Vector4)value;
                        thryProperty.MaterialProperty.textureScaleAndOffset = stValue == Vector4.one ? Vector4.zero : Vector4.one;
                        thryProperty.MaterialProperty.textureScaleAndOffset = stValue;
                    }
                    else
                    {
                        var textureValue = (Texture)value;
                        thryProperty.TextureValue = textureValue != null ? null : Texture2D.whiteTexture;
                        thryProperty.TextureValue = textureValue;
                    }
                    break;
#if UNITY_6000_2_OR_NEWER
                case UnityEngine.Rendering.ShaderPropertyType.Int:
#elif UNITY_2022_1_OR_NEWER
                case MaterialProperty.PropType.Int:
#endif
#if UNITY_6000_2_OR_NEWER
                case UnityEngine.Rendering.ShaderPropertyType.Float:
                case UnityEngine.Rendering.ShaderPropertyType.Range:
#else
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
#endif
                    // Ok so, hear me out. Either Thry or Unity doesn't seem to like it when I'm setting a value that's 0 to 0, and when the stored shader value loads, it gets overwritten. Setting it to something else fixes it
                    float floatValue = Convert.ToSingle(value);
                    thryProperty.FloatValue = floatValue == 0f ? 1f : 0f;
                    thryProperty.FloatValue = floatValue;
                    break;
                default:
                    ThryLogger.LogErr($"Tried setting value of invalid type <b>{propType}</b> for property <b>{propertyName}</b>");
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
            var property = context.SourcePropertiesAndValues
                .FirstOrDefault(x => x.Key.name == propertyName).Key;

            if (property == null)
                return default;

            return GetSourcePropertyValue<T>(context, property);
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

        /// <summary>
        /// Removes serialized material properties the current "shader" no longer declares. After a shader swap
        /// (e.g. lilToon -> Poiyomi) Unity keeps every previously-set value in m_SavedProperties, leaving
        /// orphaned textures/floats/colors that bloat the .mat and get pulled into build dependency scans even though
        /// the inspector, which is gated by the current shader, hides them. This prunes anything not on the shader.
        /// </summary>
        public static void RemoveOrphanedProperties(Material material, Shader shader)
        {
            if (material == null || shader == null) return;

            var validNames = new HashSet<string>(StringComparer.Ordinal);
#if UNITY_6000_0_OR_NEWER
            int propCount = shader.GetPropertyCount();
#else
            int propCount = ShaderUtil.GetPropertyCount(shader);
#endif
            for (int i = 0; i < propCount; i++)
            {
#if UNITY_6000_0_OR_NEWER
                validNames.Add(shader.GetPropertyName(i));
#else
                validNames.Add(ShaderUtil.GetPropertyName(shader, i));
#endif
            }

            var serializedObject = new SerializedObject(material);
            SerializedProperty saved = serializedObject.FindProperty("m_SavedProperties");
            if (saved == null) return;

            bool changed = false;
            changed |= PruneOrphanedEntries(saved.FindPropertyRelative("m_TexEnvs"), validNames);
            changed |= PruneOrphanedEntries(saved.FindPropertyRelative("m_Floats"), validNames);
            changed |= PruneOrphanedEntries(saved.FindPropertyRelative("m_Ints"), validNames);
            changed |= PruneOrphanedEntries(saved.FindPropertyRelative("m_Colors"), validNames);

            if (changed) serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Deletes entries from one of a material's serialized property arrays (m_TexEnvs/m_Floats/m_Ints/m_Colors)
        /// whose name isn't in "validNames". Unity built-ins (unity_*) are always kept. Returns true if anything
        /// was removed.
        /// </summary>
        static bool PruneOrphanedEntries(SerializedProperty array, HashSet<string> validNames)
        {
            if (array == null || !array.isArray) return false;

            bool changed = false;
            for (int i = array.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty first = array.GetArrayElementAtIndex(i).FindPropertyRelative("first");
                string name = first == null ? null : first.propertyType == SerializedPropertyType.String ? first.stringValue : first.FindPropertyRelative("name")?.stringValue;

                if (string.IsNullOrEmpty(name)) continue;
                if (name.StartsWith("unity_", StringComparison.Ordinal)) continue;
                if (validNames.Contains(name)) continue;

                array.DeleteArrayElementAtIndex(i);
                changed = true;
            }
            return changed;
        }
    }
}
