using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Poiyomi.ModularShaderSystem
{
    /// <summary>
    /// Removes section contributions after duplicate expansion. Property declarations stay intact so
    /// disabling a layer never renumbers another layer or loses its saved material values.
    /// Shared numeric inputs become constants at their disabled/default values.
    /// </summary>
    public static class ShaderSectionFilter
    {
        private static readonly Regex Marker = new Regex(@"/\*poi-section:(?<end>/)?(?<id>\d+)\*/", RegexOptions.Compiled);
        private static readonly Regex Property = new Regex(@"(?<name>[A-Za-z_]\w*)\s*\(\s*""(?<label>(?:\\.|[^""\\])*)""\s*,", RegexOptions.Compiled);
        private static readonly Regex Reference = new Regex(@"reference_propert(?:y|ies)\s*:\s*(?:\[(?<names>[^\]]*)\]|(?<names>[_A-Za-z]\w*))", RegexOptions.Compiled);
        private static readonly Regex Identifier = new Regex(@"\b[_A-Za-z]\w*\b", RegexOptions.Compiled);
        private static readonly Regex DefaultValue = new Regex(@"\G\s*(?<type>Range\s*\([^)]*\)|Float|Int|Integer|Vector|Color|2D|3D|Cube|2DArray|CubeArray)\s*\)\s*=\s*(?<value>\([^)]*\)|""[^""]*""|[-+\d.eE]+)", RegexOptions.Compiled);
        private static readonly Regex Declaration = new Regex(@"(?m)^(?<indent>[ \t]*)(?<type>(?:(?:uniform|static|const)\s+)*(?:half|float|fixed|int|uint|bool)[1-4]?(?:x[1-4])?)\s+(?<names>[^;{}\r\n]+);[ \t]*\r?$", RegexOptions.Compiled);
        private static readonly Regex Shared = new Regex(@"/\*poi-section-shared-begin\*/(?<code>.*?)/\*poi-section-shared-end\*/|/\*poi-section-fallback:(?<code>.*?)\*/", RegexOptions.Singleline | RegexOptions.Compiled);

        public static string BeginModule(int index) => "/*poi-section:" + index + "*/";
        public static string EndModule(int index) => "/*poi-section:/" + index + "*/";

        internal static string RemoveTemplateAnnotations(string source)
        {
            // These comments are generator metadata, not shader source. In particular, the
            // material optimizer treats a one-line block comment as opening a multiline block.
            return Marker.Replace(Shared.Replace(source, m => m.Value.StartsWith("/*poi-section-fallback:", StringComparison.Ordinal)
                ? "" : m.Groups["code"].Value), "");
        }

        public static bool IsSection(string name)
        {
            return !string.IsNullOrEmpty(name) && !IsEnd(name) &&
                (name.StartsWith("m_", StringComparison.Ordinal) || name.StartsWith("s_start", StringComparison.Ordinal) ||
                 name.StartsWith("ss_start", StringComparison.Ordinal) || name.StartsWith("g_start", StringComparison.Ordinal));
        }

        private static bool IsEnd(string name) => name.StartsWith("m_end", StringComparison.Ordinal) ||
            name.StartsWith("s_end", StringComparison.Ordinal) || name.StartsWith("ss_end", StringComparison.Ordinal) ||
            name.StartsWith("g_end", StringComparison.Ordinal);

        public static string Apply(string source, IEnumerable<string> disabledSections, IEnumerable<int> repeatedModules = null)
        {
            var disabled = new HashSet<string>(disabledSections ?? Enumerable.Empty<string>(), StringComparer.Ordinal);
            if (disabled.Count == 0) return RemoveTemplateAnnotations(source);
            var propertiesMatch = Regex.Match(source, @"\bProperties\s*\{");
            if (!propertiesMatch.Success) throw new InvalidOperationException("The generated shader has no Properties block.");
            int open = source.IndexOf('{', propertiesMatch.Index);
            int close = FindClosingBrace(source, open);
            string properties = source.Substring(open + 1, close - open - 1);

            var excludedProperties = new HashSet<string>(StringComparer.Ordinal);
            var moduleProperties = new Dictionary<int, HashSet<string>>();
            var zeroEnablers = new HashSet<string>(StringComparer.Ordinal);
            var defaults = new Dictionary<string, string>(StringComparer.Ordinal);
            var sections = new Stack<bool>();
            var moduleStack = new Stack<int>();
            // Property text and provenance are traversed in their final insertion order.
            var tokens = Property.Matches(properties).Cast<Match>().Select(m => (match: m, marker: false))
                .Concat(Marker.Matches(properties).Cast<Match>().Select(m => (match: m, marker: true)))
                .OrderBy(t => t.match.Index);
            foreach (var token in tokens)
            {
                Match m = token.match;
                if (token.marker)
                {
                    if (m.Groups["end"].Success) { if (moduleStack.Count > 0) moduleStack.Pop(); }
                    else moduleStack.Push(int.Parse(m.Groups["id"].Value));
                    continue;
                }
                string name = m.Groups["name"].Value;
                var defaultMatch = DefaultValue.Match(properties, m.Index + m.Length);
                if (defaultMatch.Success && !defaultMatch.Groups["value"].Value.StartsWith("\"", StringComparison.Ordinal))
                    defaults[name] = defaultMatch.Groups["value"].Value;
                if (IsEnd(name)) { if (sections.Count > 0) sections.Pop(); continue; }
                if (IsSection(name))
                {
                    if (name.StartsWith("m_", StringComparison.Ordinal) && !name.StartsWith("m_start", StringComparison.Ordinal))
                        sections.Clear();
                    bool off = disabled.Contains(name) || (sections.Count > 0 && sections.Peek());
                    sections.Push(off);
                    if (off)
                        foreach (Match reference in Reference.Matches(m.Groups["label"].Value))
                            foreach (Match id in Identifier.Matches(reference.Groups["names"].Value))
                                zeroEnablers.Add(id.Value);
                    continue;
                }
                if (moduleStack.Count > 0)
                {
                    int owner = moduleStack.Peek();
                    if (!moduleProperties.TryGetValue(owner, out var names))
                        moduleProperties[owner] = names = new HashSet<string>(StringComparer.Ordinal);
                    names.Add(name);
                }
                if (sections.Count > 0 && sections.Peek()) excludedProperties.Add(name);
            }

            // A module can contribute properties to several categories. Only remove its entire code
            // when all its properties are excluded; nested partial removal uses the template guards.
            var excludedModules = new HashSet<int>(moduleProperties.Where(p => p.Value.Count > 0 && p.Value.All(excludedProperties.Contains)).Select(p => p.Key));
            // Repeated features also carry first/last shared contributions. Their individual guards
            // remove the selected instance while retaining those wrappers for surviving instances.
            if (repeatedModules != null) excludedModules.ExceptWith(repeatedModules);
            foreach (string enabler in zeroEnablers)
            {
                excludedProperties.Add(enabler);
                defaults[enabler] = "0";
            }
            string implementation = StripModules(source.Substring(close + 1), excludedModules);
            implementation = StripGuards(implementation, zeroEnablers);
            implementation = RemoveTemplateAnnotations(implementation);
            implementation = FreezeDefaults(implementation, source.Substring(close + 1), excludedProperties, defaults);
            return Marker.Replace(source.Substring(0, close + 1), "") + implementation;
        }

        private static string StripModules(string source, HashSet<int> disabled)
        {
            var output = new StringBuilder(source.Length);
            var stack = new Stack<(bool excluded, bool keepShared)>();
            int position = 0;
            foreach (Match m in Marker.Matches(source))
            {
                string fragment = source.Substring(position, m.Index - position);
                if (stack.Count == 0 || !stack.Peek().excluded) output.Append(fragment);
                else if (stack.Peek().keepShared)
                    foreach (Match shared in Shared.Matches(fragment)) output.Append(shared.Groups["code"].Value);
                if (m.Groups["end"].Success) { if (stack.Count > 0) stack.Pop(); }
                else
                {
                    bool parentExcluded = stack.Count > 0 && stack.Peek().excluded;
                    bool excluded = disabled.Contains(int.Parse(m.Groups["id"].Value));
                    stack.Push((excluded || parentExcluded, excluded && !parentExcluded));
                }
                position = m.Index + m.Length;
            }
            if (stack.Count == 0 || !stack.Peek().excluded) output.Append(source, position, source.Length - position);
            return output.ToString();
        }

        private static string StripGuards(string source, HashSet<string> zeroEnablers)
        {
            var output = new StringBuilder(source.Length);
            var stack = new Stack<bool>();
            foreach (string line in source.Split('\n'))
            {
                string trimmed = line.TrimStart();
                bool parentExcluded = stack.Count > 0 && stack.Peek();
                if (trimmed.StartsWith("//ifex ", StringComparison.Ordinal))
                {
                    bool excluded = parentExcluded || EvaluateExclusion(trimmed.Substring(7), zeroEnablers) == true;
                    stack.Push(excluded);
                    if (!excluded) output.Append(line).Append('\n');
                }
                else if (trimmed.StartsWith("//endex", StringComparison.Ordinal))
                {
                    bool excluded = stack.Count > 0 && stack.Pop();
                    if (!excluded) output.Append(line).Append('\n');
                }
                else if (!parentExcluded) output.Append(line).Append('\n');
            }
            return output.ToString();
        }

        // Unknown material values must stay unknown. Only a provably true exclusion may remove code.
        internal static bool? EvaluateExclusion(string expression, HashSet<string> zeroEnablers)
        {
            expression = expression.Trim();
            while (expression.StartsWith("(") && MatchingOuterParentheses(expression))
                expression = expression.Substring(1, expression.Length - 2).Trim();
            int op = FindOperator(expression, "||");
            if (op >= 0)
            {
                bool? left = EvaluateExclusion(expression.Substring(0, op), zeroEnablers);
                bool? right = EvaluateExclusion(expression.Substring(op + 2), zeroEnablers);
                return left == true || right == true ? true : left == false && right == false ? (bool?)false : null;
            }
            op = FindOperator(expression, "&&");
            if (op >= 0)
            {
                bool? left = EvaluateExclusion(expression.Substring(0, op), zeroEnablers);
                bool? right = EvaluateExclusion(expression.Substring(op + 2), zeroEnablers);
                return left == false || right == false ? false : left == true && right == true ? (bool?)true : null;
            }
            var match = Regex.Match(expression, @"^(?<name>[_A-Za-z]\w*)\s*(?<op>==|!=|<=|>=|<|>)\s*(?<value>-?\d+(?:\.\d+)?)$");
            if (!match.Success || !zeroEnablers.Contains(match.Groups["name"].Value)) return null;
            double value = double.Parse(match.Groups["value"].Value, System.Globalization.CultureInfo.InvariantCulture);
            switch (match.Groups["op"].Value)
            {
                case "==": return value == 0;
                case "!=": return value != 0;
                case "<=": return 0 <= value;
                case ">=": return 0 >= value;
                case "<": return 0 < value;
                case ">": return 0 > value;
                default: return null;
            }
        }

        private static int FindOperator(string value, string op)
        {
            int depth = 0;
            for (int i = 0; i < value.Length - 1; i++)
            {
                if (value[i] == '(') depth++;
                if (value[i] == ')') depth--;
                if (depth == 0 && value.Substring(i, 2) == op) return i;
            }
            return -1;
        }

        private static bool MatchingOuterParentheses(string value)
        {
            int depth = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '(') depth++;
                if (value[i] == ')' && --depth == 0) return i == value.Length - 1;
            }
            return false;
        }

        private static string FreezeDefaults(string source, string original, HashSet<string> properties, Dictionary<string, string> defaults)
        {
            var types = new Dictionary<string, string>();
            foreach (Match declaration in Declaration.Matches(original))
                foreach (string variable in declaration.Groups["names"].Value.Split(','))
                {
                    var id = Regex.Match(variable.Trim(), @"^[_A-Za-z]\w*");
                    if (id.Success) types[id.Value] = Regex.Replace(declaration.Groups["type"].Value, @"\b(uniform|static|const)\s+", "");
                }
            var values = new Dictionary<string, string>();
            foreach (string name in properties)
            {
                if (defaults.TryGetValue(name, out var value)) values[name] = value;
                if (types.ContainsKey(name + "_ST")) values[name + "_ST"] = "(1,1,0,0)";
                if (types.ContainsKey(name + "_TexelSize")) values[name + "_TexelSize"] = "(1,1,1,1)";
            }
            source = Declaration.Replace(source, m =>
            {
                var declarations = m.Groups["names"].Value.Split(',');
                if (declarations.Any(d => d.Contains("(") || d.Contains(")"))) return m.Value;
                var remaining = declarations.Where(d =>
                {
                    var id = Regex.Match(d.Trim(), @"^[_A-Za-z]\w*");
                    return !values.ContainsKey(id.Value);
                }).ToArray();
                return remaining.Length == declarations.Length ? m.Value : remaining.Length == 0 ? "" :
                    m.Groups["indent"].Value + m.Groups["type"].Value + " " + string.Join(",", remaining) + ";";
            });
            // Render-state properties also need to ignore saved values when their section is omitted.
            source = Regex.Replace(source, @"(?m)^(?<command>[ \t]*(?:Cull|ZTest|ZWrite|ZClip|AlphaToMask|BlendOp|Blend|ColorMask|Offset|Ref|ReadMask|WriteMask|Comp(?:Front|Back)?|Pass(?:Front|Back)?|Fail(?:Front|Back)?|ZFail(?:Front|Back)?)\b)(?<args>[^\r\n]*)", line =>
                line.Groups["command"].Value + Regex.Replace(line.Groups["args"].Value, @"\[(?<name>[_A-Za-z]\w*)\]", m =>
                    values.TryGetValue(m.Groups["name"].Value, out var value) && !value.StartsWith("(")
                        ? RenderStateValue(line.Groups["command"].Value.Trim(), value) : m.Value));
            // Locking evaluates these comments using saved material values. Frozen inputs must
            // remain frozen there too, even if the material still has the feature enabled.
            source = Regex.Replace(source, @"(?m)^(?<prefix>[ \t]*//ifex )(?<condition>[^\r\n]*)", line =>
            {
                string condition = Regex.Replace(line.Groups["condition"].Value, @"isNotAnimated\(\s*(?<name>[_A-Za-z]\w*)\s*\)", m =>
                    values.ContainsKey(m.Groups["name"].Value) ? "true" : m.Value);
                condition = Identifier.Replace(condition, m => values.TryGetValue(m.Value, out var value) && !value.StartsWith("(") ? value : m.Value);
                return line.Groups["prefix"].Value + condition;
            });
            var used = new HashSet<string>(Identifier.Matches(source).Cast<Match>().Select(m => m.Value));
            var frozen = values.Where(e => used.Contains(e.Key)).OrderBy(e => e.Key).ToArray();
            if (frozen.Length == 0) return source;
            // Unique names prevent the material optimizer from replacing a constant declaration
            // (or its uses) with the saved material value.
            source = Identifier.Replace(source, m => values.ContainsKey(m.Value) ? "poiSectionDefault" + m.Value : m.Value);
            var constants = new StringBuilder("\n#ifndef POI_SECTION_DEFAULTS\n#define POI_SECTION_DEFAULTS\n");
            foreach (var entry in frozen)
            {
                string type = types.TryGetValue(entry.Key, out var declared) ? declared : entry.Value.StartsWith("(") ? "float4" : "float";
                string value = entry.Value;
                if (value.StartsWith("("))
                {
                    var components = value.Trim('(', ')').Split(',').Select(s => s.Trim()).ToList();
                    int count = char.IsDigit(type[type.Length - 1]) ? type[type.Length - 1] - '0' : 1;
                    while (components.Count < count) components.Add("1");
                    value = count == 1 ? components[0] : type + "(" + string.Join(",", components.Take(count)) + ")";
                }
                constants.Append("static const ").Append(type).Append(" poiSectionDefault").Append(entry.Key).Append(" = ").Append(value).Append(";\n");
            }
            constants.Append("#endif\n");
            // A shared include and each program may be combined by Unity. The guard prevents duplicate definitions.
            return Regex.Replace(source, @"(?m)^(\s*)(CGINCLUDE|HLSLINCLUDE|CGPROGRAM|HLSLPROGRAM)\b", m => m.Value + constants);
        }

        private static string RenderStateValue(string command, string value)
        {
            if (!double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var number)) return value;
            int index = (int)number;
            string[] choices = null;
            if (command == "ZWrite" || command == "ZClip" || command == "AlphaToMask") return index == 0 ? "Off" : "On";
            if (command == "Cull") choices = new[] { "Off", "Front", "Back" };
            if (command == "ZTest" || command.StartsWith("Comp")) choices = new[] { "Always", "Never", "Less", "Equal", "LEqual", "Greater", "NotEqual", "GEqual", "Always" };
            if (command == "Blend") choices = new[] { "Zero", "One", "DstColor", "SrcColor", "OneMinusDstColor", "SrcAlpha", "OneMinusSrcColor", "DstAlpha", "OneMinusDstAlpha", "SrcAlphaSaturate", "OneMinusSrcAlpha" };
            if (command == "BlendOp") choices = new[] { "Add", "Sub", "RevSub", "Min", "Max" };
            if (command.StartsWith("Pass") || command.StartsWith("Fail") || command.StartsWith("ZFail")) choices = new[] { "Keep", "Zero", "Replace", "IncrSat", "DecrSat", "Invert", "IncrWrap", "DecrWrap" };
            if (command == "ColorMask") return index == 0 ? "0" : string.Concat("RGBA".Where((c, i) => (index & (8 >> i)) != 0));
            return choices != null && index >= 0 && index < choices.Length ? choices[index] : value;
        }

        private static int FindClosingBrace(string text, int open)
        {
            int depth = 0;
            bool quoted = false, lineComment = false, blockComment = false;
            for (int i = open; i < text.Length; i++)
            {
                char c = text[i];
                char next = i + 1 < text.Length ? text[i + 1] : '\0';
                if (lineComment) { if (c == '\n') lineComment = false; continue; }
                if (blockComment) { if (c == '*' && next == '/') { blockComment = false; i++; } continue; }
                if (quoted) { if (c == '\\') i++; else if (c == '"') quoted = false; continue; }
                if (c == '/' && next == '/') { lineComment = true; i++; continue; }
                if (c == '/' && next == '*') { blockComment = true; i++; continue; }
                if (c == '"') { quoted = true; continue; }
                if (c == '{') depth++;
                if (c == '}' && --depth == 0) return i;
            }
            throw new InvalidOperationException("Unbalanced generated Properties block.");
        }
    }
}
