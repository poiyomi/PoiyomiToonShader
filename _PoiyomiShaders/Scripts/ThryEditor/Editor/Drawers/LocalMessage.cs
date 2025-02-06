using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class LocalMessageDrawer : MaterialPropertyDrawer
    {
        protected ButtonData _buttonData;
        protected bool _isInit;
        protected virtual void Init(string s)
        {
            if (_isInit) return;
            _buttonData = Parser.Deserialize<ButtonData>(s);
            _isInit = true;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            Init(prop.displayName);
            if (_buttonData == null) return;
            if (_buttonData.text.Length > 0)
            {
                GUILayout.Label(new GUIContent(_buttonData.text, _buttonData.hover), _buttonData.center_position ? Styles.richtext_center : Styles.richtext);
                Rect r = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
                    _buttonData.action.Perform(ShaderEditor.Active?.Materials);
            }
            if (_buttonData.texture != null)
            {
                if (_buttonData.center_position) GUILayout.Label(new GUIContent(_buttonData.texture.loaded_texture, _buttonData.hover), EditorStyles.centeredGreyMiniLabel, GUILayout.MaxHeight(_buttonData.texture.height));
                else GUILayout.Label(new GUIContent(_buttonData.texture.loaded_texture, _buttonData.hover), GUILayout.MaxHeight(_buttonData.texture.height));
                Rect r = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
                    _buttonData.action.Perform(ShaderEditor.Active?.Materials);
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            ShaderProperty.RegisterDrawer(this);
            return 0;
        }
    }

}