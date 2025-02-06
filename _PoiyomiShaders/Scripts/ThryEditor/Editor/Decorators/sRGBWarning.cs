using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class sRGBWarningDecorator : MaterialPropertyDrawer
    {
        private ColorSpace _targetColorSpace = ColorSpace.Linear;

        public sRGBWarningDecorator(){}

        public sRGBWarningDecorator(string colorSpace)
        {
            if(colorSpace == "gamma" || colorSpace == "true")
                _targetColorSpace = ColorSpace.Gamma;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (Config.Singleton.showColorspaceWarnings)
                GUILib.ColorspaceWarning(prop, _targetColorSpace == ColorSpace.Gamma);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            ShaderProperty.RegisterDecorator(this);
            return 0;
        }
    }

}