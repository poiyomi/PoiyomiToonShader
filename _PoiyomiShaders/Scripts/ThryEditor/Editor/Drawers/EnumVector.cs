using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor.Drawers
{
	public class EnumVectorDrawer : MaterialPropertyDrawer
	{
		readonly string[] _labels = new string[4] { "X", "Y", "Z", "W" };
		int _count = 0;

		void Init(params string[] args)
		{
			int i = 0;
			for (; i < args.Length && i < 4; i++) _labels[i] = args[i];
			_count = Mathf.Clamp(i, 1, 4);
		}

		public EnumVectorDrawer(string a) { Init(a); }
		public EnumVectorDrawer(string a, string b) { Init(a, b); }
		public EnumVectorDrawer(string a, string b, string c) { Init(a, b, c); }
		public EnumVectorDrawer(string a, string b, string c, string d) { Init(a, b, c, d); }

		public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.BeginChangeCheck();
			Rect field = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
			float spacing = 4f;
			float w = (field.width - spacing * (_count - 1)) / _count;
			Vector4 v = prop.vectorValue;
			for (int i = 0; i < _count; i++)
			{
				Rect r = new Rect(field.x + i * (w + spacing), field.y, w, field.height);
				bool disable = string.Equals(_labels[i], "NA", System.StringComparison.InvariantCultureIgnoreCase) || _labels[i] == "-";
				bool on = v[i] > 0.5f;
				if (disable)
				{
					EditorGUI.BeginDisabledGroup(true);
					GUI.Toggle(r, false, _labels[i], "Button");
					EditorGUI.EndDisabledGroup();
					v[i] = 0f;
				}
				else
				{
					bool newOn = GUI.Toggle(r, on, _labels[i], "Button");
					v[i] = newOn ? 1f : 0f;
				}
			}
			if (EditorGUI.EndChangeCheck())
			{
				prop.vectorValue = v;
			}
		}

		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			ShaderProperty.RegisterDrawer(this);
			return base.GetPropertyHeight(prop, label, editor);
		}
	}
}

