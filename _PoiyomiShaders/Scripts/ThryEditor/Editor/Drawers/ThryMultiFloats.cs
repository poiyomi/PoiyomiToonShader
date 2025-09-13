using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor.Drawers
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
            {
                if (!ShaderEditor.Active.PropertyDictionary.TryGetValue(_otherProperties[i], out var sProp))
                {
                    // TODO: log error?
                    _otherMaterialProps[i] = null;

                    continue;
                }

                sProp.UpdatedMaterialPropertyReference();
                _otherMaterialProps[i] = sProp.MaterialProperty;
            }

            EditorGUI.LabelField(labelR, label);

            bool anyChanged;
            using (var change_scope = new EditorGUI.ChangeCheckScope())
            {
                using(var temp_indent = new GUILib.IndentOverrideScope(0))
                {
                    PropGUI(prop, editor, contentR, 0);
                    editor.EndAnimatedCheck();
                    for (int i = 0; i < _otherMaterialProps.Length; i++)
                    {
                        if (_otherMaterialProps[i] == null)
                            continue;

                        PropGUI(_otherMaterialProps[i], editor, contentR, i + 1);
                    }
                    editor.BeginAnimatedCheck(prop);
                }

                anyChanged = change_scope.changed;
            }

            //If edited in animation mode mark as animated (needed cause other properties isnt checked in draw)
            if (anyChanged && ShaderEditor.Active.IsInAnimationMode && !ShaderEditor.Active.CurrentProperty.IsAnimated)
                ShaderEditor.Active.CurrentProperty.SetAnimated(true, false);

            //make sure all are animated together
            bool animated = ShaderEditor.Active.CurrentProperty.IsAnimated;
            bool renamed = ShaderEditor.Active.CurrentProperty.IsRenaming;
            for (int i = 0; i < _otherProperties.Length; i++)
            {
                if (ShaderEditor.Active.PropertyDictionary.TryGetValue(_otherProperties[i], out var sProp))
                    sProp.SetAnimated(animated, renamed);
            }
        }

        void PropGUI(MaterialProperty prop, MaterialEditor editor, Rect contentRect, int index)
        {
            contentRect.x += contentRect.width * index;
            contentRect.width -= 5;

            using (new GUILib.AnimationScope(editor, prop))
            {
                using(var change_scope = new EditorGUI.ChangeCheckScope())
                {
                    float val = prop.floatValue;
                    EditorGUI.showMixedValue = prop.hasMixedValue;
                    if (_displayAsToggles) val = EditorGUI.Toggle(contentRect, val == 1) ? 1 : 0;
                    else val = EditorGUI.FloatField(contentRect, val);

                    if(change_scope.changed)
                    {
                        prop.floatValue = val;
                    }
                }
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            ShaderProperty.RegisterDrawer(this);
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

}
