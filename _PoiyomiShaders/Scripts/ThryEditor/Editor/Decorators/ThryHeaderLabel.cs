using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class ThryHeaderLabelDecorator : MaterialPropertyDrawer
    {
        readonly string _text;
        readonly int _size;
        GUIStyle _style;

        public ThryHeaderLabelDecorator(string text) : this(text, EditorStyles.standardFont.fontSize)
        {
        }
        public ThryHeaderLabelDecorator(string text, float size)
        {
            this._text = text;
            this._size = (int)size;
        }


        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            ShaderProperty.RegisterDecorator(this);
            return _size + 6;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            // Done here instead of constructor because else unity throws warnings
            if (_style == null)
            {
                _style = new GUIStyle(EditorStyles.boldLabel);
                _style.fontSize = this._size;
            }

            float offst = position.height;
            position = EditorGUI.IndentedRect(position);
            GUI.Label(position, _text, _style);
        }
    }

}