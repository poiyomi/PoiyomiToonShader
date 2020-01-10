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

        private static GUIStyle s_masterLabel;
        private static GUIStyle s_dropDownHeader;
        private static GUIStyle s_dropDownHeaderLabel;
        private static GUIStyle s_dropDownHeaderButton;
        private static GUIStyle s_bigTextureStyle;
        private static GUIStyle s_vectorPropertyStyle;

        private static GUIStyle s_redStyle;
        private static GUIStyle s_yellowStyle;
        private static GUIStyle s_greenStyle;

        public static GUIStyle masterLabel
        {
            get
            {
                if (s_masterLabel == null)
                {
                    s_masterLabel = new GUIStyle(GUI.skin.label);
                    s_masterLabel.richText = true;
                    s_masterLabel.alignment = TextAnchor.MiddleCenter;
                }
                return s_masterLabel;
            }
        }

        public static GUIStyle dropDownHeader
        {
            get {
                if (s_dropDownHeader == null) {
                    s_dropDownHeader = new GUIStyle("ShurikenModuleTitle");
                    s_dropDownHeader.font = new GUIStyle(EditorStyles.label).font;
                    s_dropDownHeader.border = new RectOffset(15, 7, 4, 4);
                    s_dropDownHeader.fixedHeight = 22;
                    s_dropDownHeader.contentOffset = new Vector2(20f, -2f);
                }
                return s_dropDownHeader;
            }
        }

        public static GUIStyle dropDownHeaderLabel
        {
            get
            {
                if (s_dropDownHeaderLabel == null)
                {
                    s_dropDownHeaderLabel = new GUIStyle(EditorStyles.boldLabel);
                    s_dropDownHeaderLabel.alignment = TextAnchor.MiddleCenter;
                }
                return s_dropDownHeaderLabel;
            }
        }

        public static GUIStyle dropDownHeaderButton
        {
            get
            {
                if (s_dropDownHeaderButton == null)
                {
                    s_dropDownHeaderButton = new GUIStyle(EditorStyles.toolbarButton);
                }
                return s_dropDownHeaderButton;
            }
        }

        public static GUIStyle bigTextureStyle
        {
            get
            {
                if (s_bigTextureStyle == null)
                {
                    s_bigTextureStyle = new GUIStyle();
                    s_bigTextureStyle.fixedHeight = 48;
                }
                return s_bigTextureStyle;
            }
        }

        public static GUIStyle vectorPropertyStyle
        {
            get
            {
                if (s_vectorPropertyStyle == null)
                {
                    s_vectorPropertyStyle = new GUIStyle();
                    s_vectorPropertyStyle.padding = new RectOffset(0, 0, 2, 2);
                }
                return s_vectorPropertyStyle;
            }
        }

        public static GUIStyle greenStyle
        {
            get
            {
                if (s_greenStyle == null)
                {
                    s_greenStyle = new GUIStyle();
                    s_greenStyle.normal.textColor = new Color(0, 0.5f, 0);
                }
                return s_greenStyle;
            }
        }

        public static GUIStyle yellowStyle
        {
            get
            {
                if (s_yellowStyle == null)
                {
                    s_yellowStyle = new GUIStyle();
                    s_yellowStyle.normal.textColor = new Color(1, 0.79f, 0);
                }
                return s_yellowStyle;
            }
        }

        public static GUIStyle redStyle
        {
            get
            {
                if (s_redStyle == null)
                {
                    s_redStyle = new GUIStyle();
                    s_redStyle.normal.textColor = Color.red;
                }
                return s_redStyle;
            }
        }

        private static Texture2D p_rounded_texture;
        public static Texture2D rounded_texture
        {
            get{
                if (p_rounded_texture == null)
                {
                    string search_name = RESOURCE_NAME.WHITE_RECT;
                    if (EditorGUIUtility.isProSkin)
                        search_name = RESOURCE_NAME.DARK_RECT;
                    p_rounded_texture = LoadTextureByFileName(search_name);
                }
                return p_rounded_texture;
            }
        }

        private static Texture2D t_settings_icon;
        public static Texture2D settings_icon
        {
            get
            {
                if (t_settings_icon == null)
                    t_settings_icon = LoadTextureByFileName(RESOURCE_NAME.SETTINGS_ICON_TEXTURE);
                return t_settings_icon;
            }
        }

        private static Texture2D t_dropdown_settings_icon;
        public static Texture2D dropdown_settings_icon
        {
            get
            {
                if (t_dropdown_settings_icon == null)
                    t_dropdown_settings_icon = LoadTextureByFileName(RESOURCE_NAME.DROPDOWN_SETTINGS_TEXTURE);
                return t_dropdown_settings_icon;
            }
        }

        private static Texture2D t_ative_link_icon;
        public static Texture2D active_link_icon
        {
            get
            {
                if (t_ative_link_icon == null)
                    t_ative_link_icon = LoadTextureByFileName(RESOURCE_NAME.ACTICE_LINK_ICON);
                return t_ative_link_icon;
            }
        }

        private static Texture2D t_inactive_link_icon;
        public static Texture2D inactive_link_icon
        {
            get
            {
                if (t_inactive_link_icon == null)
                    t_inactive_link_icon = LoadTextureByFileName(RESOURCE_NAME.INACTICE_LINK_ICON);
                return t_inactive_link_icon;
            }
        }


        private static Texture2D LoadTextureByFileName(string search_name)
        {
            Texture2D tex;
            string[] guids = AssetDatabase.FindAssets(search_name + " t:texture");
            if (guids.Length == 0)
                tex = Texture2D.whiteTexture;
            else
                tex = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guids[0]));
            return tex;
        }
    }
}