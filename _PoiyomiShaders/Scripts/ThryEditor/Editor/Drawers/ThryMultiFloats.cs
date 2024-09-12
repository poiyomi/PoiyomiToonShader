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
    public class ThryMultiFloatsDrawer : MaterialPropertyDrawer
    {
        string[] _otherProperties;
        MaterialProperty[] _otherMaterialProps;
        bool _displayAsToggles;

        public ThryMultiFloatsDrawer(string displayAsToggles, string p1, string p2, string p3, string p4, string p5, string p6, string p7) : this(displayAsToggles, new string[] { p1, p2, p3, p4, p5, p6, p7 }) { }
        public ThryMultiFloatsDrawer(string displayAsToggles, string p1, string p2, string p3, string p4, string p5, string p6) : this(displayAsToggles, new string[] { p1, p2, p3, p4, p5, p6 }) { }
        public ThryMultiFloatsDrawer(string displayAsToggles, string p1, string p2, string p3, string p4, string p5) : this(displayAsToggles, new string[] { p1, p2, p3, p4, p5 }) { }
        public ThryMultiFloatsDrawer(string displayAsToggles, string p1, string p2, string p3, string p4) : this(displayAsToggles, new string[] { p1, p2, p3, p4 }) { }
        public ThryMultiFloatsDrawer(string displayAsToggles, string p1, string p2, string p3) : this(displayAsToggles, new string[] { p1, p2, p3 }) { }
        public ThryMultiFloatsDrawer(string displayAsToggles, string p1, string p2) : this(displayAsToggles, new string[] { p1, p2 }) { }
        public ThryMultiFloatsDrawer(string displayAsToggles, string p1) : this(displayAsToggles, new string[] { p1 }) { }

        public ThryMultiFloatsDrawer(string displayAsToggles, params string[] extraProperties)
        {
            _displayAsToggles = displayAsToggles.ToLower() == "true" || displayAsToggles == "1";
            _otherProperties = extraProperties;
            _otherMaterialProps = new MaterialProperty[extraProperties.Length];
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            Rect labelR = new Rect(position);
            labelR.width = EditorGUIUtility.labelWidth;
            Rect contentR = new Rect(position);
            contentR.width = (contentR.width - labelR.width) / (_otherProperties.Length + 1);
            contentR.x += labelR.width;

            for (int i = 0; i < _otherProperties.Length; i++)
                _otherMaterialProps[i] = ShaderEditor.Active.PropertyDictionary[_otherProperties[i]].MaterialProperty;
            EditorGUI.BeginChangeCheck();

            EditorGUI.LabelField(labelR, label);
            int indentLevel = EditorGUI.indentLevel; //else it double indents
            EditorGUI.indentLevel = 0;
            PropGUI(prop, contentR, 0);
            if (ShaderEditor.Active.IsInAnimationMode)
                MaterialEditor.PrepareMaterialPropertiesForAnimationMode(_otherMaterialProps, true);
            for (int i = 0; i < _otherProperties.Length; i++)
            {
                PropGUI(_otherMaterialProps[i], contentR, i + 1);
            }
            EditorGUI.indentLevel = indentLevel;

            //If edited in animation mode mark as animated (needed cause other properties isnt checked in draw)
            if (EditorGUI.EndChangeCheck() && ShaderEditor.Active.IsInAnimationMode && !ShaderEditor.Active.CurrentProperty.IsAnimated)
                ShaderEditor.Active.CurrentProperty.SetAnimated(true, false);
            //make sure all are animated together
            bool animated = ShaderEditor.Active.CurrentProperty.IsAnimated;
            bool renamed = ShaderEditor.Active.CurrentProperty.IsRenaming;
            for (int i = 0; i < _otherProperties.Length; i++)
                ShaderEditor.Active.PropertyDictionary[_otherProperties[i]].SetAnimated(animated, renamed);
        }

        void PropGUI(MaterialProperty prop, Rect contentRect, int index)
        {
            contentRect.x += contentRect.width * index;
            contentRect.width -= 5;

            float val = prop.floatValue;
            EditorGUI.showMixedValue = prop.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            if (_displayAsToggles) val = EditorGUI.Toggle(contentRect, val == 1) ? 1 : 0;
            else val = EditorGUI.FloatField(contentRect, val);
            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = val;
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

}