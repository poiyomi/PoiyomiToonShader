using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace Thry.ThryEditor
{
    internal static class UIElementsHelpers
    {
        public static void SetTextFieldReadonly(TextField field, bool isReadOnly)
        {
            field.isReadOnly = isReadOnly;
            field.style.opacity = isReadOnly ? 0.5f : 1f;
        }
    }
}
