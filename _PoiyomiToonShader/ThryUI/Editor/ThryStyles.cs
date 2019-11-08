// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class Styles
    {

        private static Styles stylesObject;

        public static Styles Get()
        {
            if (stylesObject == null) stylesObject = new Styles();
            return stylesObject;
        }

        public GUIStyle masterLabel;
        public GUIStyle dropDownHeader;
        public GUIStyle dropDownHeaderLabel;
        public GUIStyle dropDownHeaderButton;
        public GUIStyle bigTextureStyle;
        public GUIStyle vectorPropertyStyle;

        public GUIStyle redStyle;
        public GUIStyle yellowStyle;
        public GUIStyle greenStyle;

        private Styles()
        {
            InitMasterLabel();
            InitDropdownHeaderStyle();
            InitDropDownHeaderLabel();
            InitDropDownHeaderButton();
            InitBigTextureStyle();
            InitVectorProperty();
            InitColoredStyles();
        }

        private void InitMasterLabel()
        {
            masterLabel = new GUIStyle(GUI.skin.label);
            masterLabel.richText = true;
            masterLabel.alignment = TextAnchor.MiddleCenter;
        }

        private void InitDropdownHeaderStyle()
        {
            dropDownHeader = new GUIStyle("ShurikenModuleTitle");
            dropDownHeader.font = new GUIStyle(EditorStyles.label).font;
            dropDownHeader.border = new RectOffset(15, 7, 4, 4);
            dropDownHeader.fixedHeight = 22;
            dropDownHeader.contentOffset = new Vector2(20f, -2f);
        }

        private void InitDropDownHeaderLabel()
        {
            dropDownHeaderLabel = new GUIStyle(EditorStyles.boldLabel);
            dropDownHeaderLabel.alignment = TextAnchor.MiddleCenter;
        }

        private void InitDropDownHeaderButton()
        {
            dropDownHeaderButton = new GUIStyle(EditorStyles.toolbarButton);
        }

        private void InitBigTextureStyle()
        {
            bigTextureStyle = new GUIStyle();
            bigTextureStyle.fixedHeight = 48;
        }

        private void InitVectorProperty()
        {
            vectorPropertyStyle = new GUIStyle();
            vectorPropertyStyle.padding = new RectOffset(0, 0, 2, 2);
        }

        private void InitColoredStyles()
        {
            redStyle = new GUIStyle();
            redStyle.normal.textColor = Color.red;

            yellowStyle = new GUIStyle();
            yellowStyle.normal.textColor = new Color(1, 0.79f, 0);

            greenStyle = new GUIStyle();
            greenStyle.normal.textColor = new Color(0, 0.5f, 0);
        }

        private static Texture2D p_white_rounded_texture;
        public static Texture2D white_rounded_texture
        {
            get{
                if (p_white_rounded_texture == null)
                {
                    string[] guids = AssetDatabase.FindAssets("thry_white_rect t:texture");
                    if (guids.Length == 0)
                        p_white_rounded_texture = Texture2D.whiteTexture;
                    else
                        p_white_rounded_texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guids[0]));
                }
                return p_white_rounded_texture;
            }
        }
    }
}