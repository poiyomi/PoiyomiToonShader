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
    public class ThryIntRangeDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            var range = prop.rangeLimits;
            EditorGUI.BeginChangeCheck();
            var value = EditorGUI.IntSlider(position, label, (int)prop.GetNumber(), (int)range.x, (int)range.y);
            if (EditorGUI.EndChangeCheck())
                prop.SetNumber(value);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

}