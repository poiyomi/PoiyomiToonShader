using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor.Drawers
{
	public class ThryVectorDrawer : MaterialPropertyDrawer
	{
		public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			if (prop.type != MaterialProperty.PropType.Vector)
			{
				EditorGUI.HelpBox(position, "[ThryVector] requires a Vector property", MessageType.Warning);
				return;
			}

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;

			int oldIndent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			Rect indented = EditorGUI.IndentedRect(position);
			Rect fieldR = EditorGUI.PrefixLabel(indented, GUIUtility.GetControlID(FocusType.Passive), label);

			Vector4 v = prop.vectorValue;
			float[] values = new float[4] { v.x, v.y, v.z, v.w };
			GUIContent[] labels = new GUIContent[4]
			{
				new GUIContent("X"),
				new GUIContent("Y"),
				new GUIContent("Z"),
				new GUIContent("W")
			};

			EditorGUI.MultiFloatField(fieldR, labels, values);
			EditorGUI.indentLevel = oldIndent;

			if (EditorGUI.EndChangeCheck())
			{
				prop.vectorValue = new Vector4(values[0], values[1], values[2], values[3]);
			}
		}

		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			ShaderProperty.RegisterDrawer(this);
			return base.GetPropertyHeight(prop, label, editor);
		}
	}
}


