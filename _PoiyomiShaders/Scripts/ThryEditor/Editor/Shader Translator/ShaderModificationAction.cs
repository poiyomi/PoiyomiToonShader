using System;

namespace Thry.ThryEditor.ShaderTranslations
{
    [Serializable]
    public class ShaderModificationAction
    {
        public enum ActionType
        {
            ChangeTargetShader,
            SetTargetPropertyValue,
        }

        public ActionType actionType;
        public string propertyName;
        public string targetValue;
    }
}