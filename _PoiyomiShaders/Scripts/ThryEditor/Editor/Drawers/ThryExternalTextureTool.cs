using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class ThryExternalTextureToolDrawer : MaterialPropertyDrawer
    {
        string _toolTypeName;
        string _toolHeader;

        Type t_ExternalToolType;
        MethodInfo _onGui;
        object _externalTool;
        MaterialProperty _prop;

        bool _isTypeLoaded;
        bool _doesExternalTypeExist;
        bool _isInit;
        bool _showTool;

        public ThryExternalTextureToolDrawer(string toolHeader, string toolTypeName)
        {
            this._toolTypeName = toolTypeName;
            this._toolHeader = toolHeader;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            LoadType();
            if (_doesExternalTypeExist)
            {
                _prop = prop;
                GUILib.SmallTextureProperty(position, prop, label, editor, DrawingData.CurrentTextureProperty.hasFoldoutProperties, ExternalGUI);
            }
            else
            {
                GUILib.SmallTextureProperty(position, prop, label, editor, DrawingData.CurrentTextureProperty.hasFoldoutProperties);
            }
        }

        void ExternalGUI()
        {
            if (GUI.Button(EditorGUI.IndentedRect(EditorGUILayout.GetControlRect()), _toolHeader)) _showTool = !_showTool;
            if (_showTool)
            {
                Init();

                int indent = EditorGUI.indentLevel;
                GUILib.BeginCustomIndentLevel(0);
                GUILayout.BeginHorizontal();
                GUILayout.Space(indent * 15);
                GUILayout.BeginVertical();
                _onGui.Invoke(_externalTool, new object[0]);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILib.EndCustomIndentLevel();
            }
        }

        public void LoadType()
        {
            if (_isTypeLoaded) return;
            t_ExternalToolType = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(_toolTypeName)).Where(t => t != null).FirstOrDefault();
            _doesExternalTypeExist = t_ExternalToolType != null;
            _isTypeLoaded = true;
        }

        public void Init()
        {
            if (_isInit) return;
            if (_isTypeLoaded && _doesExternalTypeExist)
            {
                _onGui = t_ExternalToolType.GetMethod("OnGUI", BindingFlags.NonPublic | BindingFlags.Instance);
                _externalTool = ScriptableObject.CreateInstance(t_ExternalToolType);
                EventInfo eventTextureGenerated = t_ExternalToolType.GetEvent("TextureGenerated");
                if (eventTextureGenerated != null)
                    eventTextureGenerated.AddEventHandler(_externalTool, new EventHandler(TextureGenerated));
            }
            _isInit = true;
        }

        void TextureGenerated(object sender, EventArgs args)
        {
            if (args != null && args.GetType().GetField("generated_texture") != null)
            {
                Texture2D generated = args.GetType().GetField("generated_texture").GetValue(args) as Texture2D;
                _prop.textureValue = generated;
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            ShaderProperty.RegisterDrawer(this);
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

}