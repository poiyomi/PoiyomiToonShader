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
    public class ThrySeperatorDecorator : MaterialPropertyDrawer
    {
        Color _color = Styles.COLOR_FG;

        public ThrySeperatorDecorator() { }
        public ThrySeperatorDecorator(string c)
        {
            ColorUtility.TryParseHtmlString(c, out _color);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.RegisterDecorator(this);
            return 1;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            position = EditorGUI.IndentedRect(position);
            EditorGUI.DrawRect(position, _color);
        }
    }

}