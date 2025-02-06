using System;

namespace Poi.Tools
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PoiExternalToolAttribute : Attribute
    {
        public string Id { get; private set; }

        public PoiExternalToolAttribute(string id)
        {
            Id = id;
        }
    }
}