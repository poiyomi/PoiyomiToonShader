using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class Vector31Drawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            string[] labels = label.text.Split('|');

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;
            Vector4 vec = EditorGUI.Vector3Field(position, labels[0], prop.vectorValue);
            float single = EditorGUI.FloatField(EditorGUILayout.GetControlRect(), labels.Length > 1 ? labels[1] : labels[0], prop.vectorValue.w);
            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = new Vector4(vec.x, vec.y, vec.z, single);
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            ShaderProperty.RegisterDrawer(this);
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

}