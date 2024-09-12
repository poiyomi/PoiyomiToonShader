using UnityEngine;
using UnityEngine.UIElements;

namespace Thry.ThryEditor.ShaderTranslations
{

    public class ConditionalTranslationBlockListItem : BindableElement
    {
#if UNITY_2022_1_OR_NEWER

        public ConditionalTranslationBlockListItem()
        {
            var uxml = Resources.Load<VisualTreeAsset>("Shader Translator/TranslatorConditionalListItem");
            uxml.CloneTree(this);
        }
#endif
    }
}