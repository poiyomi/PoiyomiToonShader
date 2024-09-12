using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static Thry.GradientEditor;
using static Thry.TexturePacker;

namespace Thry
{
    public class sRGBWarningDecorator : MaterialPropertyDrawer
    {
        bool _isSRGB = true;

        public sRGBWarningDecorator()
        {
            _isSRGB = false;
        }

        public sRGBWarningDecorator(string shouldHaveSRGB)
        {
            this._isSRGB = shouldHaveSRGB.ToLower() == "true";
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (Config.Singleton.showColorspaceWarnings)
                GUILib.ColorspaceWarning(prop, _isSRGB);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.RegisterDecorator(this);
            return 0;
        }
    }

}