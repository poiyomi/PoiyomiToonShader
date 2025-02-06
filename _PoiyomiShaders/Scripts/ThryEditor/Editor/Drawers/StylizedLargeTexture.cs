using UnityEditor;
using UnityEngine;

namespace Thry
{
    [System.Obsolete("Use StylizedLargeTextureDrawer instead")]
    public class StylizedBigTextureDrawer : StylizedLargeTextureDrawer
    {
        
    }

    public class StylizedLargeTextureDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            GUILib.StylizedBigTextureProperty(position, prop, label, editor, ((ShaderTextureProperty)ShaderEditor.Active.CurrentProperty).hasScaleOffset);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            ShaderProperty.RegisterDrawer(this);
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

}