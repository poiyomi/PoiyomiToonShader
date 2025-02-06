using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Poi.Tools
{
    public class PoiExternalToolRegistry
    {
        public const string ExternalPoiToolPackageName = "com.poiyomi.tools";
        public const string PoiFontToolId = "poi.msdf-font-converter";

        static Dictionary<string, IPoiExternalTool> ExternalTools
        {
            get
            {
                if(_externalTools == null || _externalTools.Count == 0)
                {
                    _externalTools = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(ass => ass.GetTypes())
                        .Where(t => typeof(IPoiExternalTool).IsAssignableFrom(t) && t.GetCustomAttribute<PoiExternalToolAttribute>() != null)
                        .Select(t =>
                        {
                            var attribute = t.GetCustomAttribute<PoiExternalToolAttribute>();
                            var toolInstance = (IPoiExternalTool)Activator.CreateInstance(t);
                            return new { attribute.Id, Tool = toolInstance };
                        })
                        .ToDictionary(x => x.Id, x => x.Tool);
                }
                return _externalTools;
            }
        }
        static Dictionary<string, IPoiExternalTool> _externalTools;

        public static bool TryGetTool(string id, out IPoiExternalTool foundTool)
        {
            return ExternalTools.TryGetValue(id, out foundTool);
        }
    }
}