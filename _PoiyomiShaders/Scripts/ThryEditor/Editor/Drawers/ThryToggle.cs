using System;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class ThryToggleDrawer : MaterialPropertyDrawer
    {
        public string keyword;
        private bool isFirstGUICall = true;
        public bool left = false;
        private bool hasKeyword = false;

        public ThryToggleDrawer()
        {
        }

        //the reason for weird string thing here is that you cant have bools as params for drawers
        public ThryToggleDrawer(string keywordLeft)
        {
            if (keywordLeft == "true") left = true;
            else if (keywordLeft == "false") left = false;
            else keyword = keywordLeft;
            hasKeyword = keyword != null;
        }

        public ThryToggleDrawer(string keyword, string left)
        {
            this.keyword = keyword;
            this.left = left == "true";
            hasKeyword = keyword != null;
        }

        protected void SetKeyword(MaterialProperty prop, bool on)
        {
            if (ShaderOptimizer.IsMaterialLocked(prop.targets[0] as Material)) return;
            SetKeywordInternal(prop, on, "_ON");
        }

        protected void CheckKeyword(MaterialProperty prop)
        {
            if (ShaderEditor.Active != null && ShaderOptimizer.IsMaterialLocked(prop.targets[0] as Material)) return;
            if (prop.hasMixedValue)
            {
                foreach (Material m in prop.targets)
                {
                    if (m.GetNumber(prop) == 1)
                        m.EnableKeyword(keyword);
                    else
                        m.DisableKeyword(keyword);
                }
            }
            else
            {
                foreach (Material m in prop.targets)
                {
                    if (prop.GetNumber() == 1)
                        m.EnableKeyword(keyword);
                    else
                        m.DisableKeyword(keyword);
                }
            }
        }

        static bool IsPropertyTypeSuitable(MaterialProperty prop)
        {
            return prop.type == MaterialProperty.PropType.Float
                   || prop.type == MaterialProperty.PropType.Range
#if UNITY_2022_1_OR_NEWER
                   || prop.type == MaterialProperty.PropType.Int;
#endif
            ;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (!IsPropertyTypeSuitable(prop))
            {
                return EditorGUIUtility.singleLineHeight * 2.5f;
            }
            if (hasKeyword)
            {
                CheckKeyword(prop);
                ShaderProperty.DisallowAnimation();
            }
            ShaderProperty.RegisterDrawer(this);
            return base.GetPropertyHeight(prop, label, editor);
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (!IsPropertyTypeSuitable(prop))
            {
                return;
            }
            if (isFirstGUICall && !ShaderEditor.Active.IsLockedMaterial)
            {
                if (hasKeyword) CheckKeyword(prop);
                isFirstGUICall = false;
            }
            //why is this not inFirstGUICall ? cause it seems drawers are kept between different openings of the shader editor, so this needs to be set again every time the shader editor is reopened for that material
            ShaderEditor.Active.PropertyDictionary[prop.name].SetKeyword(keyword);

            if (hasKeyword)
                ShaderEditor.Active.Editor.EndAnimatedCheck();
            
            EditorGUI.BeginChangeCheck();

            bool value = (Math.Abs(prop.GetNumber()) > 0.001f);
            EditorGUI.showMixedValue = prop.hasMixedValue;
            if (left) value = EditorGUI.ToggleLeft(position, label, value, Styles.style_toggle_left_richtext);
            else value = EditorGUI.Toggle(position, label, value);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                bool wasRecording = UnityHelper.InAnimationRecording();
                if (hasKeyword && wasRecording)
                    UnityHelper.StopAnimationRecording();
                
                prop.SetNumber(value ? 1.0f : 0.0f);
                if (hasKeyword)
                    SetKeyword(prop, value);

                if (hasKeyword && wasRecording)
                    UnityHelper.StartAnimationRecording();
            }

            ShaderEditor.Active.Editor.BeginAnimatedCheck(prop);
        }

        public override void Apply(MaterialProperty prop)
        {
            base.Apply(prop);
            if (!IsPropertyTypeSuitable(prop))
                return;

            if (prop.hasMixedValue)
                return;

            if (hasKeyword) SetKeyword(prop, (Math.Abs(prop.GetNumber()) > 0.001f));
        }

        protected void SetKeywordInternal(MaterialProperty prop, bool on, string defaultKeywordSuffix)
        {
            // if no keyword is provided, use <uppercase property name> + defaultKeywordSuffix
            string kw = string.IsNullOrEmpty(keyword) ? prop.name.ToUpperInvariant() + defaultKeywordSuffix : keyword;
            // set or clear the keyword
            foreach (Material material in prop.targets)
            {
                if (on)
                    material.EnableKeyword(kw);
                else
                    material.DisableKeyword(kw);
            }
        }
    }

}