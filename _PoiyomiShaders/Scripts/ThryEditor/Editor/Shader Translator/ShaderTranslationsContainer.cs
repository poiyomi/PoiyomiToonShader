using System;
using System.Collections.Generic;

namespace Thry.ThryEditor.ShaderTranslations
{
    [Serializable]
    public class ShaderTranslationsContainer
    {
        public string containerName = "Properties";
        public List<PropertyTranslation> PropertyTranslations = new List<PropertyTranslation>();
    }
}