using UnityEngine;
using UnityEngine.UIElements;

namespace Thry.ThryEditor.ShaderTranslations
{
    public class ShaderNameMatchedModificationsEditor : BindableElement
    {
        public ShaderNameMatchedModificationsEditor()
        {
            var treeAsset = Resources.Load<VisualTreeAsset>("Shader Translator/ShaderNameMatchedModificationsListItem");
            treeAsset.CloneTree(this);

            var propertyMods = this.Q<ListView>("modsList");
            propertyMods.makeItem += () => new PropertyModificationListItem();
        }
    }
}