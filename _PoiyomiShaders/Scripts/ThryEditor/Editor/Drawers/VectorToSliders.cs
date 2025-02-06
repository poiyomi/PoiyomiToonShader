using System;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class VectorToSlidersDrawer : MaterialPropertyDrawer
    {
        class SliderConfig
        {
            public string Label;
            public float Min;
            public float Max;

            public SliderConfig(string l, string min, string max)
            {
                Label = l;
                Min = Parse(min);
                Max = Parse(max);
            }

            public SliderConfig(string l, float min, float max)
            {
                Label = l;
                Min = min;
                Max = max;
            }

            private float Parse(string s)
            {
                if (s.StartsWith("n", StringComparison.Ordinal))
                    return -float.Parse(s.Substring(1), System.Globalization.CultureInfo.InvariantCulture);
                return float.Parse(s.Substring(1), System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        SliderConfig _slider1;
        SliderConfig _slider2;
        SliderConfig _slider3;
        SliderConfig _slider4;
        bool _twoMinMaxDrawers;

        VectorToSlidersDrawer(SliderConfig slider1, SliderConfig slider2, SliderConfig slider3, SliderConfig slider4, float twoMinMaxDrawers)
        {
            _slider1 = slider1;
            _slider2 = slider2;
            _slider3 = slider3;
            _slider4 = slider4;
            _twoMinMaxDrawers = twoMinMaxDrawers == 1;
        }

        public VectorToSlidersDrawer(string label1, string min1, string max1, string label2, string min2, string max2, string label3, string min3, string max3, string label4, string min4, string max4) :
            this(new SliderConfig(label1, min1, max1), new SliderConfig(label2, min2, max2), new SliderConfig(label3, min3, max3), new SliderConfig(label4, min4, max4), 0)
        { }
        public VectorToSlidersDrawer(string label1, string min1, string max1, string label2, string min2, string max2, string label3, string min3, string max3) :
            this(new SliderConfig(label1, min1, max1), new SliderConfig(label2, min2, max2), new SliderConfig(label3, min3, max3), null, 0)
        { }
        public VectorToSlidersDrawer(string label1, string min1, string max1, string label2, string min2, string max2) :
            this(new SliderConfig(label1, min1, max1), new SliderConfig(label2, min2, max2), null, null, 0)
        { }
        public VectorToSlidersDrawer(float twoMinMaxDrawers, string label1, string min1, string max1, string label2, string min2, string max2) :
            this(new SliderConfig(label1, min1, max1), new SliderConfig(label2, min2, max2), null, null, twoMinMaxDrawers)
        { }

        public VectorToSlidersDrawer(string label1, float min1, float max1, string label2, float min2, float max2, string label3, float min3, float max3, string label4, float min4, float max4) :
            this(new SliderConfig(label1, min1, max1), new SliderConfig(label2, min2, max2), new SliderConfig(label3, min3, max3), new SliderConfig(label4, min4, max4), 0)
        { }
        public VectorToSlidersDrawer(string label1, float min1, float max1, string label2, float min2, float max2, string label3, float min3, float max3) :
            this(new SliderConfig(label1, min1, max1), new SliderConfig(label2, min2, max2), new SliderConfig(label3, min3, max3), null, 0)
        { }
        public VectorToSlidersDrawer(string label1, float min1, float max1, string label2, float min2, float max2) :
            this(new SliderConfig(label1, min1, max1), new SliderConfig(label2, min2, max2), null, null, 0)
        { }
        public VectorToSlidersDrawer(float twoMinMaxDrawers, string label1, float min1, float max1, string label2, float min2, float max2) :
            this(new SliderConfig(label1, min1, max1), new SliderConfig(label2, min2, max2), null, null, twoMinMaxDrawers)
        { }

        private float GetIconHeight()
        {
            return GUILayoutUtility.GetLastRect().y + GUILayoutUtility.GetLastRect().height - 14;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            Vector4 vector = prop.vectorValue;

            Rect fullRect = EditorGUILayout.BeginVertical();
            DrawingData.IconsPositioningCount = 2;

            EditorGUI.BeginChangeCheck();
            if (_twoMinMaxDrawers)
            {
                float min1 = vector.x;
                float max1 = vector.y;
                float min2 = vector.z;
                float max2 = vector.w;
                EditorGUI.showMixedValue = prop.hasMixedValue;
                EditorGUILayout.MinMaxSlider(_slider1.Label, ref min1, ref max1, _slider1.Min, _slider1.Max);
                DrawingData.IconsPositioningHeights[0] = GetIconHeight();
                EditorGUI.showMixedValue = prop.hasMixedValue;
                EditorGUILayout.MinMaxSlider(_slider2.Label, ref min2, ref max2, _slider2.Min, _slider2.Max);
                DrawingData.IconsPositioningHeights[1] = GetIconHeight();
                vector = new Vector4(min1, max1, min2, max2);
            }
            else
            {
                EditorGUI.showMixedValue = prop.hasMixedValue;
                vector.x = EditorGUILayout.Slider(_slider1.Label, vector.x, _slider1.Min, _slider1.Max);
                DrawingData.IconsPositioningHeights[0] = GetIconHeight();
                EditorGUI.showMixedValue = prop.hasMixedValue;
                vector.y = EditorGUILayout.Slider(_slider2.Label, vector.y, _slider2.Min, _slider2.Max);
                DrawingData.IconsPositioningHeights[1] = GetIconHeight();
                if (_slider3 != null)
                {
                    EditorGUI.showMixedValue = prop.hasMixedValue;
                    vector.z = EditorGUILayout.Slider(_slider3.Label, vector.z, _slider3.Min, _slider3.Max);
                    DrawingData.IconsPositioningHeights[2] = GetIconHeight();

                    DrawingData.IconsPositioningCount = 3;
                }
                if (_slider4 != null)
                {
                    EditorGUI.showMixedValue = prop.hasMixedValue;
                    vector.w = EditorGUILayout.Slider(_slider4.Label, vector.w, _slider4.Min, _slider4.Max);
                    DrawingData.IconsPositioningHeights[3] = GetIconHeight();

                    DrawingData.IconsPositioningCount = 4;
                }
            }
            if (EditorGUI.EndChangeCheck())
                prop.vectorValue = vector;

            EditorGUILayout.EndVertical();
            DrawingData.TooltipCheckRect = fullRect;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            ShaderProperty.RegisterDrawer(this);
            return base.GetPropertyHeight(prop, label, editor) - EditorGUIUtility.singleLineHeight;
        }
    }

}