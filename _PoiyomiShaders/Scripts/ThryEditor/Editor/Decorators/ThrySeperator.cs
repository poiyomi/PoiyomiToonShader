using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor.Decorators
{
    public class ThrySeperatorDecorator : MaterialPropertyDrawer
    {
        readonly Color _color;
        readonly float _thickness;
        readonly float _paddingTop;
        readonly float _paddingBottom;

        public ThrySeperatorDecorator() : this(1, 0, 0)
        {
        }
        
        public ThrySeperatorDecorator(float thickness) : this(thickness, 5, 5)
        {
        }
        
        public ThrySeperatorDecorator(float thickness, float padding) : this(thickness, padding, padding)
        {
        }
        
        public ThrySeperatorDecorator(float thickness, float paddingTop, float paddingBottom)
        {
            this._color = Colors.backgroundDark;
            this._thickness = thickness;
            this._paddingTop = paddingTop;
            this._paddingBottom = paddingBottom;
        }
        
        public ThrySeperatorDecorator(string c) : this(c, 1, 0, 0)
        {
        }
        
        public ThrySeperatorDecorator(string c, float thickness) : this(c, thickness, 5, 5)
        {
        }
        
        public ThrySeperatorDecorator(string c, float thickness, float padding) : this(c, thickness, padding, padding)
        {
        }
        
        public ThrySeperatorDecorator(string c, float thickness, float paddingTop, float paddingBottom)
        {
            if (!c.StartsWith("#"))
                c = "#" + c;
            ColorUtility.TryParseHtmlString(c, out _color);
            this._thickness = thickness;
            this._paddingTop = paddingTop;
            this._paddingBottom = paddingBottom;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            ShaderProperty.RegisterDecorator(this);
            return _thickness + _paddingTop + _paddingBottom;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            Rect inner = EditorGUI.IndentedRect(position);
            inner.y += _paddingTop;
            inner.height = _thickness;

            // Adjust side padding symmetrically relative to indented rect
            const float expandLeft = 3f;
            const float expandRight = 3f;
            inner.x -= expandLeft;
            inner.width += (expandLeft + expandRight);

            EditorGUI.DrawRect(inner, _color);
        }
    }

}