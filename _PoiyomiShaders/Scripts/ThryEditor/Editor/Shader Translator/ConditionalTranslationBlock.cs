using System;

namespace Thry.ThryEditor.ShaderTranslations
{
    [Serializable]
    public class ConditionalTranslationBlock
    {
        public enum ConditionalBlockType
        {
            If
        }

        public ConditionalBlockType ConditionType = ConditionalBlockType.If;
        public string ConditionalExpression;
        public string MathExpression;
    }
}