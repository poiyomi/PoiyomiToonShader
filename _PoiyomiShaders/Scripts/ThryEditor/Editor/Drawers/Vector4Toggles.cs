using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class Vector4TogglesDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;
            EditorGUI.LabelField(position, label);
            position.x += EditorGUIUtility.labelWidth;
            position.width = (position.width - EditorGUIUtility.labelWidth) / 4;
            bool b1 = GUI.Toggle(position, prop.vectorValue.x == 1, "");
            position.x += position.width;
            bool b2 = GUI.Toggle(position, prop.vectorValue.y == 1, "");
            position.x += position.width;
            bool b3 = GUI.Toggle(position, prop.vectorValue.z == 1, "");
            position.x += position.width;
            bool b4 = GUI.Toggle(position, prop.vectorValue.w == 1, "");
            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = new Vector4(b1 ? 1 : 0, b2 ? 1 : 0, b3 ? 1 : 0, b4 ? 1 : 0);
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            ShaderProperty.RegisterDrawer(this);
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

}