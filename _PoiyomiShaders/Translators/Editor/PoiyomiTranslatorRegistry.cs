// Automated Shader Translation designed by BluWizard LABS.
// https://github.com/BluWizard10

using UnityEngine;

namespace Poi.Tools.ShaderTranslator.Translations
{
    /// <summary>
    /// Single place that decides which translation handles a given source shader, so the context menus and the
    /// auto-translate watcher don't each need to know about every supported source. Add a new pipeline here and
    /// it becomes reachable from every entry point at once.
    ///
    /// Translators are cached because they build their property translation list in the constructor, and they
    /// hold no state between calls to Translate.
    /// </summary>
    public static class PoiyomiTranslatorRegistry
    {
        static LiltoonToPoiyomiToonTranslation _liltoonToon;
        static LiltoonToPoiyomiProTranslation _liltoonPro;
        // static UnityStandardToPoiyomiToonTranslation _unityStandardToon;
        // static UnityStandardToPoiyomiProTranslation _unityStandardPro;

        /// <summary>
        /// The translation that can handle <paramref name="sourceMaterial"/>, or null when none of them can.
        /// </summary>
        /// <param name="sourceMaterial">Material to translate, still on its original shader</param>
        /// <param name="isPro">True to target Poiyomi Pro, false for Poiyomi Toon</param>
        public static ScriptedShaderTranslator GetTranslator(Material sourceMaterial, bool isPro)
        {
            return sourceMaterial == null ? null : GetTranslatorForShader(sourceMaterial.shader, isPro);
        }

        /// <summary>
        /// The translation that can handle <paramref name="sourceShader"/>, or null when none of them can. Use
        /// this over the material overload whenever the material has already been swapped off the source shader,
        /// as happens when the user picks Poiyomi straight from the inspector's shader dropdown.
        /// </summary>
        /// <param name="sourceShader">The shader the material's values were authored against</param>
        /// <param name="isPro">True to target Poiyomi Pro, false for Poiyomi Toon</param>
        public static ScriptedShaderTranslator GetTranslatorForShader(Shader sourceShader, bool isPro)
        {
            if (sourceShader == null) return null;

            if (LiltoonToPoiyomiToonTranslation.IsLiltoonShader(sourceShader))
            {
                return isPro ? (ScriptedShaderTranslator)(_liltoonPro ?? (_liltoonPro = new LiltoonToPoiyomiProTranslation())) : (_liltoonToon ?? (_liltoonToon = new LiltoonToPoiyomiToonTranslation()));
            }

            /*
            if (UnityStandardToPoiyomiToonTranslation.IsUnityStandardShader(sourceShader))
            {
                return isPro ? (ScriptedShaderTranslator)(_unityStandardPro ?? (_unityStandardPro = new UnityStandardToPoiyomiProTranslation())) : (_unityStandardToon ?? (_unityStandardToon = new UnityStandardToPoiyomiToonTranslation()));
            }
            */

            return null;
        }

        /// <summary>
        /// Whether any translation supports this shader. The edition doesn't affect the answer, so callers that
        /// only need to filter a selection down to translatable materials can use this and skip the flag.
        /// </summary>
        public static bool CanTranslate(Shader sourceShader)
        {
            return GetTranslatorForShader(sourceShader, false) != null;
        }
    }
}
