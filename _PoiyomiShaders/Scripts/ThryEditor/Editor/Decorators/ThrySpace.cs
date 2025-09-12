using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor.Decorators
{
    public class ThrySpaceDecorator : MaterialPropertyDrawer
    {
        float _space = 10;

        public ThrySpaceDecorator() { }
        public ThrySpaceDecorator(float space)
        {
            this._space = space;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            ShaderProperty.RegisterDecorator(this);
            return _space;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
        }
    }
}