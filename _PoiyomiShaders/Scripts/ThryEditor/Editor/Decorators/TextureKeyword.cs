using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor.Decorators
{
	// Enables/disables a keyword when a texture is assigned/cleared.
	// Usage: [TextureKeyword] or [TextureKeyword(MY_KEYWORD)] on a 2D/Cube/Array texture property.
	public class TextureKeywordDecorator : MaterialPropertyDrawer
	{
		private readonly string _keywordOverride;

		public TextureKeywordDecorator() { }

		public TextureKeywordDecorator(string keyword)
		{
			_keywordOverride = keyword;
		}

		public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			// no visual output; behavior-only like sRGBWarning
			if (Event.current.type == EventType.Repaint)
				UpdateKeyword(prop, editor);
		}

		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			ShaderProperty.RegisterDecorator(this);
			return 0;
		}

		private void UpdateKeyword(MaterialProperty prop, MaterialEditor editor)
		{
			if (prop == null) return;
			if (prop.type != MaterialProperty.PropType.Texture) return;
			if (editor?.targets == null) return;

			string keyword = GetKeywordName(prop);
			if (string.IsNullOrEmpty(keyword)) return;

			foreach (Object o in editor.targets)
			{
				var mat = o as Material;
				if (mat == null) continue;
				bool hasTexture = mat.GetTexture(prop.name) != null;
				if (hasTexture) mat.EnableKeyword(keyword);
				else mat.DisableKeyword(keyword);
			}
		}

		private string GetKeywordName(MaterialProperty prop)
		{
			if (!string.IsNullOrEmpty(_keywordOverride)) return _keywordOverride;
			// Default to PROP_<UPPERCASE_PROPERTY_NAME_WITHOUT_LEADING_UNDERSCORES>
			string name = prop.name;
			if (string.IsNullOrEmpty(name)) return null;
			int i = 0;
			while (i < name.Length && name[i] == '_') i++;
			string trimmed = name.Substring(i);
			return "PROP_" + trimmed.ToUpperInvariant();
		}
	}
}

