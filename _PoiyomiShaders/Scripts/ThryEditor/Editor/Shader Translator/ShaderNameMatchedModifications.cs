using System;
using System.Collections.Generic;
using UnityEngine;

namespace Thry.ThryEditor.ShaderTranslations
{
    [Serializable]
    public class ShaderNameMatchedModifications
    {
        public bool negateCondition;

        public enum ConditionOperator
        {
            Equals,
            Contains,
            StartsWith,
            EndsWith,
        }

        [SerializeField] string shaderNameMatch;
        public ConditionOperator conditionOperator;

        public List<ShaderModificationAction> propertyModifications = new List<ShaderModificationAction>();

        public bool IsShaderNameMatch(string name)
        {
            bool? result = null;
            switch(conditionOperator)
            {
                case ConditionOperator.Equals:
                    result = name.Equals(shaderNameMatch, StringComparison.CurrentCultureIgnoreCase);
                    break;
                case ConditionOperator.Contains:
                    result = name.IndexOf(shaderNameMatch, StringComparison.CurrentCultureIgnoreCase) != -1;
                    break;
                case ConditionOperator.StartsWith:
                    result = name.StartsWith(shaderNameMatch, StringComparison.CurrentCultureIgnoreCase);
                    break;
                case ConditionOperator.EndsWith:
                    result = name.EndsWith(shaderNameMatch, StringComparison.CurrentCultureIgnoreCase);
                    break;
            }

            if(result == null)
                return false;

            return negateCondition ? !(bool)result : (bool)result;
        }
    }
}