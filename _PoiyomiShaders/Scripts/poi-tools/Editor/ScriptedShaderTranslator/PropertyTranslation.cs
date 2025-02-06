using System;
using System.Collections;

namespace Poi.Tools.ShaderTranslator
{
    /// <summary>
    /// Property Translation representation
    /// </summary>
    public class PropertyTranslation
    {
        /// <summary>
        /// Property name in the source shader
        /// </summary>
        public string SourceName { get; private set; }

        /// <summary>
        /// Property name in the target shader
        /// </summary>
        public string TargetName { get; private set; }

        /// <summary>
        /// Condition that needs to return true for the property translation to happen
        /// </summary>
        public Predicate<TranslationContext> Condition { get; private set; } = _ => true;

        /// <summary>
        /// Runs during translation, right before the value is copied from the translation context. Can be used to transform the value inside the translation context or do other stuff like set other properties. Provided is the ShaderProperty representation and the TranslationContext.
        /// </summary>
        public Action<ShaderProperty, TranslationContext> OnTranslate { get; private set; }

        private PropertyTranslation() { }

        /// <summary>
        /// A property translation without a target. This does nothing with the value but still runs <paramref name="onTranslate"/> when <paramref name="sourceName"/> is encountered.
        /// </summary>
        /// <param name="sourceName">Name of property in source shader</param>
        /// <param name="onTranslate">Run during translation. Can be used to transform the value of the shader property. Provided is the ShaderProperty representation and the TranslationContext.</param>
        public PropertyTranslation(string sourceName, Action<ShaderProperty, TranslationContext> onTranslate) : this(sourceName, targetName:null, onTranslate)
        {
            OnTranslate = onTranslate;
        }

        /// <summary>
        /// A property translation without a target. This does nothing with the value but if <paramref name="condition"/> returns true, <paramref name="onTranslate"/> gets run when <paramref name="sourceName"/> is encountered.
        /// </summary>
        /// <param name="sourceName">Name of property in source shader</param>
        /// <param name="condition">Condition predicate that needs to return true for translation to happen</param>
        /// <param name="onTranslate">Run during translation. Can be used to transform the value of the shader property. Provided is the ShaderProperty representation and the TranslationContext.</param>
        public PropertyTranslation(string sourceName, Predicate<TranslationContext> condition, Action<ShaderProperty, TranslationContext> onTranslate) : this(sourceName, targetName: null, condition, onTranslate)
        {
            OnTranslate = onTranslate;
            Condition = condition;
        }

        /// <summary>
        /// A property translation that's used to copy the value of <paramref name="sourceName"/> to <paramref name="targetName"/>.
        /// </summary>
        /// <param name="sourceName">Name of property in source shader</param>
        /// <param name="targetName">Name of property in target shader</param>
        public PropertyTranslation(string sourceName, string targetName)
        {
            SourceName = sourceName;
            TargetName = targetName;
        }

        /// <summary>
        /// A property translation that's used to copy the value of <paramref name="sourceName"/> to <paramref name="targetName"/> if <paramref name="condition"/> returns true.
        /// </summary>
        /// <param name="sourceName">Name of property in source shader</param>
        /// <param name="targetName">Name of property in target shader</param>
        /// <param name="condition">Condition predicate that needs to return true for translation to happen</param>
        public PropertyTranslation(string sourceName, string targetName, Predicate<TranslationContext> condition) : this(sourceName, targetName)
        {
            Condition = condition;
        }

        /// <summary>
        /// A property translation that's used to copy the value of <paramref name="sourceName"/> to <paramref name="targetName"/>. The value gets copied from the translation context to <paramref name="targetName"/>.
        /// </summary>
        /// <param name="sourceName">Name of property in source shader</param>
        /// <param name="targetName">Name of property in target shader</param>
        /// <param name="onTranslate">Run during translation. Can be used to transform the value of the shader property. Provided is the ShaderProperty representation and the TranslationContext.</param>
        public PropertyTranslation(string sourceName, string targetName, Action<ShaderProperty, TranslationContext> onTranslate) : this(sourceName, targetName)
        {
            OnTranslate = onTranslate;
        }

        /// <summary>
        /// A property translation that's used to copy the value of <paramref name="sourceName"/> to <paramref name="targetName"/>. If <paramref name="condition"/> returns true, <paramref name="onTranslate"/> runs, then the value gets copied from the translation context to <paramref name="targetName"/>.
        /// </summary>
        /// <param name="sourceName">Name of property in source shader</param>
        /// <param name="targetName">Name of property in target shader</param>
        /// <param name="condition">Condition predicate that needs to return true for translation to happen</param>
        /// <param name="onTranslate">Run during translation. Can be used to transform the value of the shader property. Provided is the ShaderProperty representation and the TranslationContext.</param>
        public PropertyTranslation(string sourceName, string targetName, Predicate<TranslationContext> condition, Action<ShaderProperty, TranslationContext> onTranslate) : this(sourceName, targetName)
        {
            OnTranslate = onTranslate;
            Condition = condition;
        }

        public override string ToString()
        {
            return $"{SourceName} -> {TargetName}";
        }
    }
}