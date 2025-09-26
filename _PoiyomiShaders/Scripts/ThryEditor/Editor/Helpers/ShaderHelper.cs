using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor.Helpers
{
    public class ShaderHelper
    {

        private static Dictionary<Shader, Dictionary<string, string[]>> shader_property_drawers = new Dictionary<Shader, Dictionary<string, string[]>>();
        public static string[] GetDrawer(MaterialProperty property)
        {
            Shader shader = ((Material)property.targets[0]).shader;

            if (!shader_property_drawers.ContainsKey(shader))
                LoadShaderPropertyDrawers(shader);

            Dictionary<string, string[]> property_drawers = shader_property_drawers[shader];
            if (property_drawers.ContainsKey(property.name))
                return property_drawers[property.name];
            return null;
        }

        public static void LoadShaderPropertyDrawers(Shader shader)
        {
            string path = AssetDatabase.GetAssetPath(shader);
            string code = FileHelper.ReadFileIntoString(path);
            code = Helper.GetStringBetweenBracketsAndAfterId(code, "Properties", new char[] { '{', '}' });
            MatchCollection matchCollection = Regex.Matches(code, @"\[.*\].*(?=\()");
            Dictionary<string, string[]> property_drawers = new Dictionary<string, string[]>();
            foreach (Match match in matchCollection)
            {
                string[] drawers_or_flag_code = GetDrawersFlagsCode(match.Value);
                string drawer_code = GetNonFlagDrawer(drawers_or_flag_code);
                if (drawer_code == null)
                    continue;

                string property_name = Regex.Match(match.Value, @"(?<=\])[^\[]*$").Value.Trim();

                List<string> drawer_and_parameters = new List<string>();
                drawer_and_parameters.Add(Regex.Split(drawer_code, @"\(")[0]);

                GetDrawerParameters(drawer_code, drawer_and_parameters);

                property_drawers[property_name] = drawer_and_parameters.ToArray();
            }
            shader_property_drawers[shader] = property_drawers;
        }

        private static void GetDrawerParameters(string code, List<string> list)
        {
            MatchCollection matchCollection = Regex.Matches(code, @"(?<=\(|,).*?(?=\)|,)");
            foreach (Match m in matchCollection)
                list.Add(m.Value);
        }

        private static string GetNonFlagDrawer(string[] codes)
        {
            foreach (string c in codes)
                if (!DrawerIsFlag(c))
                    return c;
            return null;
        }

        private static bool DrawerIsFlag(string code)
        {
            return (code == "HideInInspector" || code == "NoScaleOffset" || code == "Normal" || code == "HDR" || code == "Gamma" || code == "PerRendererData");
        }

        private static string[] GetDrawersFlagsCode(string line)
        {
            MatchCollection matchCollection = Regex.Matches(line, @"(?<=\[).*?(?=\])");
            string[] codes = new string[matchCollection.Count];
            int i = 0;
            foreach (Match m in matchCollection)
                codes[i++] = m.Value;
            return codes;
        }
        //------------Track ShaderEditor shaders-------------------
        
        public static bool IsShaderUsingThryEditor(MaterialEditor materialEditor)
        {
            return materialEditor != null && materialEditor.target != null && IsShaderUsingThryEditor((Material)materialEditor.target);
        }
        public static bool IsShaderUsingThryEditor(Material material)
        {
            return material != null && material.shader != null && IsShaderUsingThryEditor(material.shader);
        }
        public static bool IsShaderUsingThryEditor(Shader shader)
        {
            PropertyInfo shaderGUIProperty = typeof(Shader).GetProperty("customEditor", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo getter = shaderGUIProperty.GetGetMethod(nonPublic: true);
            string customEditorName = (string)getter.Invoke(shader, null);
            return customEditorName == typeof(ShaderEditor).FullName;
        }

        internal static List<(string prop, List<string> keywords)> GetPropertyKeywordsForShader(Shader s)
        {
            List<(string prop, List<string> keywords)> list = new List<(string prop, List<string> keywords)>();

            for (int i = 0; i < s.GetPropertyCount(); i++)
            {
                if (s.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Float)
                {
                    string prop = s.GetPropertyName(i);
                    List<string> keywords = null;
                    keywords = GetKeywordsFromShaderProperty(s, prop);

                    if (keywords.Count == 0)
                        continue;
                    else
                        list.Add((prop, keywords));
                }
                else if (s.GetPropertyType(i) == UnityEngine.Rendering.ShaderPropertyType.Texture)
                {
                    string prop = s.GetPropertyName(i);
                    string textureKeyword = GetPropTextureKeywordFromAttributes(s, prop);
                    if (string.IsNullOrEmpty(textureKeyword) == false)
                    {
                        // Use a single-element list to mark texture keyword association
                        list.Add((prop, new List<string> { textureKeyword }));
                    }
                }
            }

            return list;
        }

        // Logic Adapted from unity's reference implementation
        /// <summary> Returns a list of keywords for a given shader property. </summary>
        internal static List<string> GetKeywordsFromShaderProperty(Shader shader, string propertyName)
        {
            List<string> keywords = new List<string>();
            if (string.IsNullOrEmpty(propertyName))
                return keywords;

            int propertyIndex = shader.FindPropertyIndex(propertyName);
            if (propertyIndex < 0)
                return keywords;

            string[] attributes = shader.GetPropertyAttributes(propertyIndex);
            if (attributes.Length == 0 || attributes == null)
                return keywords;

            foreach (string attribute in attributes)
            {
                string args = "";
                // Regex based on Unity's reference implementation: Match a string of the form Keyword(Argument) and capture its components
                //   (\w+)    - Match a word (keyword name)
                //   \s*\(\s* - Match an opening parenthesis, allowing whitespace before and after
                //   ([^)]*)  - Match any number of characters that are not a closing parenthesis, and capture them into a group
                //   \s*\)    - Match a closing parenthesis, allowing whitespace before and after
                const string propertyDrawerRegex = @"(\w+)\s*\(\s*([^)]*)\s*\)";

                Match regexMatch = Regex.Match(attribute, propertyDrawerRegex);
                if (regexMatch.Success)
                {
                    string className = regexMatch.Groups[1].Value;
                    args = regexMatch.Groups[2].Value.Trim();

                    // Note that we don't handle ToggleOff as it would require extra logic to differentiate
                    if (className == "Toggle") // Unity Toggle drawer, toggles a keyword directly if provided as [Toggle(KEYWORD)] and toggles PropertyName
                    {
                        if (string.IsNullOrEmpty(args))
                            keywords.Add(GetUnityKeywordName(propertyName, "ON"));
                        else
                            keywords.Add(args);
                        break;
                    }
                    else if (className == "ThryToggle") // Thry Toggle drawer, toggles a keyword directly if provided as [Toggle(KEYWORD)]
                    {
                        // We only care about the first argument, the second is for UI
                        if (args.Contains(","))
                            args = args.Split(',')[0];

                        // Ignore ThryToggle's bools, since otherwise we get keywords that have the same name as HLSL language keywords
                        if (args != "false" && args != "true")
                            keywords.Add(args);

                        break;
                    }
                    else if (className == "KeywordEnum") // Keyword enum, enables one keyword out of a list of keywords provided as [KeywordEnum(KEYWORD1,KEYWORD2,KEYWORD3)]
                    {
                        string[] enumArgs = args.Split(',');
                        foreach (var enumArg in enumArgs)
                        {
                            keywords.Add(GetUnityKeywordName(propertyName, enumArg.Trim()));
                        }

                        break;
                    }
                }
            }
            return keywords;
        }

        // Reads [TextureKeyword] attribute, optionally with override name; returns PROP_ style keyword
        private static string GetPropTextureKeywordFromAttributes(Shader shader, string propertyName)
        {
            int propertyIndex = shader.FindPropertyIndex(propertyName);
            if (propertyIndex < 0) return null;
            string[] attributes = shader.GetPropertyAttributes(propertyIndex);
            if (attributes == null || attributes.Length == 0) return null;

            foreach (string attribute in attributes)
            {
                // Match TextureKeyword or TextureKeyword(OVERRIDE)
                const string regex = @"^\s*TextureKeyword\s*(?:\(\s*([^)]*)\s*\))?\s*$";
                Match m = Regex.Match(attribute, regex);
                if (m.Success)
                {
                    string overrideName = m.Groups[1].Success ? m.Groups[1].Value.Trim() : null;
                    if (string.IsNullOrEmpty(overrideName) == false)
                        return overrideName;

                    // Build default PROP_ keyword from property name without leading underscores
                    string n = propertyName;
                    int i = 0; while (i < n.Length && n[i] == '_') i++;
                    string trimmed = n.Substring(i);
                    return ("PROP_" + trimmed).ToUpperInvariant();
                }
            }
            return null;
        }

        // Logic from Unity defaults
        /// <summary> Gets a formatted Keyword name from a shader property name and a keyword name. </summary>
        private static string GetUnityKeywordName(string propertyName, string keywordName) => $"{propertyName}_{keywordName}".Replace(' ', '_').ToUpperInvariant();
    }

}