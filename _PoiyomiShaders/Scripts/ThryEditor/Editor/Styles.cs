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

        public static GUIStyle dropDownHeaderLabel { get; private set; } = CreateStyle(alignment: TextAnchor.MiddleCenter, baseStyle: EditorStyles.boldLabel);
        public static GUIStyle dropDownHeaderButton { get; private set; } = CreateStyle(baseStyle: EditorStyles.toolbarButton);
        public static GUIStyle bigTextureStyle { get; private set; } = CreateStyle(fontSize: 48);
        public static GUIStyle vectorPropertyStyle { get; private set; } = CreateStyle(padding: new RectOffset(0, 0, 2, 2));
        public static GUIStyle greenStyle { get; private set; } = CreateStyle(new Color(0, 0.5f, 0));
        public static GUIStyle yellowStyle { get; private set; } = CreateStyle(new Color(1, 0.79f, 0));
        public static GUIStyle redStyle { get; private set; } = CreateStyle(Color.red);
        public static GUIStyle made_by_style { get; private set; } = CreateStyle(fontSize: 10);
        public static GUIStyle notification_style { get; private set; } = CreateStyle(Color.red, fontSize: 12, worldWrap: true, baseStyle: GUI.skin.box);

        private static GUIStyle CreateStyle(System.Nullable<Color> color = null, int fontSize = -1 , RectOffset padding = null, TextAnchor alignment = TextAnchor.MiddleLeft,
            Texture2D backgroundTexture = null, bool worldWrap = true, GUIStyle baseStyle = null)
        {
            GUIStyle style = null;
            if (baseStyle == null)
                style = new GUIStyle();
            else
                style = new GUIStyle(baseStyle);
            if(color!=null)
                style.normal.textColor = color.Value;
            style.alignment = alignment;
            if(fontSize != -1)
                style.fontSize = fontSize;
            if (padding != null)
                style.padding = padding;
            if(backgroundTexture!=null)
                style.normal.background = backgroundTexture;
            style.wordWrap = worldWrap;
            return style;
        }

        public static Texture2D rounded_texture { get; private set; } = LoadTextureByNameAndEditorType(RESOURCE_NAME.WHITE_RECT, RESOURCE_NAME.DARK_RECT);
        public static Texture2D settings_icon { get; private set; } = LoadTextureByFileName(RESOURCE_NAME.SETTINGS_ICON_TEXTURE);
        public static Texture2D dropdown_settings_icon { get; private set; } = LoadTextureByFileName(RESOURCE_NAME.DROPDOWN_SETTINGS_TEXTURE);
        public static Texture2D active_link_icon { get; private set; } = LoadTextureByFileName(RESOURCE_NAME.ACTICE_LINK_ICON);
        public static Texture2D inactive_link_icon { get; private set; } = LoadTextureByFileName(RESOURCE_NAME.INACTICE_LINK_ICON);
        public static Texture2D visibility_icon { get; private set; } = LoadTextureByFileName(RESOURCE_NAME.VISIVILITY_ICON);
        public static Texture2D search_icon { get; private set; } = LoadTextureByFileName(RESOURCE_NAME.SEARCH_ICON);
        public static Texture2D presets_icon { get; private set; } = LoadTextureByFileName(RESOURCE_NAME.PRESETS_ICON);
        public static Texture2D t_arrow { get; private set; } = LoadTextureByFileName(RESOURCE_NAME.TEXTURE_ARROW);


        private static Texture2D LoadTextureByNameAndEditorType(string normalName, string proName)
        {
            if (EditorGUIUtility.isProSkin)
                return LoadTextureByFileName(proName);
            return LoadTextureByFileName(normalName);
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