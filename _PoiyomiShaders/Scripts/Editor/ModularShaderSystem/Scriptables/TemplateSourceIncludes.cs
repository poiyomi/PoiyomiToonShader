using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Poiyomi.ModularShaderSystem
{
    /// <summary>
    /// Composes named source sections before importing a template collection. Included code belongs
    /// to the including template, preserving its asset identity, queue, and section-filter ownership.
    /// </summary>
    public static class TemplateSourceIncludes
    {
        // #TI# "relative/Mode.poiTemplateCollection" SectionName
        private static readonly Regex Include = new Regex(
            @"^\s*#TI#\s+""(?<path>[^""]+)""\s+(?<section>\w+)\s*$");

        public static string Expand(string assetPath, Action<string> dependsOnSource = null)
        {
            return new Expansion(dependsOnSource).ExpandFile(Path.GetFullPath(assetPath), null);
        }

        private sealed class Expansion
        {
            private readonly Action<string> _dependsOnSource;
            private readonly Dictionary<string, string> _sources = new Dictionary<string, string>();
            private readonly HashSet<string> _stack = new HashSet<string>();

            public Expansion(Action<string> dependsOnSource) => _dependsOnSource = dependsOnSource;

            public string ExpandFile(string path, string section)
            {
                string key = path + "#" + section;
                if (!_stack.Add(key))
                    throw new InvalidDataException("Cyclic template source include: " + key);
                try
                {
                    if (!_sources.TryGetValue(path, out string source))
                    {
                        source = File.ReadAllText(path);
                        _sources.Add(path, source);
                    }
                    if (section != null) source = ReadSection(source, section, path);
                    var result = new StringBuilder();
                    using (var reader = new StringReader(source))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (!line.TrimStart().StartsWith("#TI#", StringComparison.Ordinal))
                            {
                                result.AppendLine(line);
                                continue;
                            }
                            Match match = Include.Match(line);
                            if (!match.Success)
                                throw new InvalidDataException("Invalid template source include in " + key + ": " + line);
                            string relative = match.Groups["path"].Value.Replace('\\', '/');
                            if (Path.IsPathRooted(relative) || relative.Contains(":"))
                                throw new InvalidDataException("Template source includes must use relative paths: " + line);
                            string included = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), relative));
                            if (!string.Equals(Path.GetExtension(included), "." + MSSConstants.TEMPLATE_COLLECTION_EXTENSION, StringComparison.Ordinal))
                                throw new InvalidDataException("Template source includes must reference a template collection: " + line);
                            // Report the dependency before reading it, so a missing source being restored reimports its owner.
                            _dependsOnSource?.Invoke(included);
                            result.Append(ExpandFile(included, match.Groups["section"].Value));
                        }
                    }
                    return result.ToString();
                }
                finally { _stack.Remove(key); }
            }

            private static string ReadSection(string source, string section, string path)
            {
                var result = new StringBuilder();
                bool selected = false;
                int matches = 0;
                using (var reader = new StringReader(source))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Match the collection importer's existing section delimiter semantics.
                        if (line.Contains("#T#"))
                        {
                            selected = line.Replace("#T#", "").Trim() == section;
                            if (selected) matches++;
                        }
                        else if (selected) result.AppendLine(line);
                    }
                }
                if (matches != 1)
                    throw new InvalidDataException("Expected exactly one template section '" + section + "' in " + path + "; found " + matches + ".");
                return result.ToString();
            }
        }
    }
}
