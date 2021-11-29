// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class Styles
    {
        public static GUIStyle masterLabel { get; private set; } = new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter };
        public static GUIStyle EDITOR_LABEL_HEADER { get; private set; } = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
        public static GUIStyle dropDownHeader { get; private set; } = new GUIStyle(new GUIStyle("ShurikenModuleTitle"))
        {
            font = new GUIStyle(EditorStyles.label).font,
            fontSize = GUI.skin.font.fontSize,
            border = new RectOffset(15, 7, 4, 4),
            fixedHeight = 22,
            contentOffset = new Vector2(20f, -2f)
        };

        public static Color COLOR_BG { get; private set; } = (EditorGUIUtility.isProSkin) ? new Color(0.4f, 0.4f, 0.4f) : new Color(0.8f, 0.8f, 0.8f);
        public static Color COLOR_FG { get; private set; } = (EditorGUIUtility.isProSkin) ? new Color(0.8f, 0.8f, 0.8f) : Color.black;

        private static Color COLOR_ICON_FONT = GUI.skin.label.normal.textColor;
        private static Color COLOR_ICON_GRAY = EditorGUIUtility.isProSkin? COLOR_ICON_FONT: new Color(0.4f, 0.4f, 0.4f);
        private static Color COLOR_ICON_ACTIVE_CYAN = Color.cyan;
        private static Color COLOR_ICON_ACTIVE_RED = Color.red;
        private static Color COLOR_BACKGROUND_1 = EditorGUIUtility.isProSkin ? new Color(0.27f, 0.27f, 0.27f) : new Color(0.65f, 0.65f, 0.65f);
        private static Color COLOR_BACKGROUND_2 = EditorGUIUtility.isProSkin ? new Color(0.5f, 0.5f, 0.5f) : new Color(0.85f, 0.85f, 0.85f);

        public static GUIStyle dropDownHeaderLabel { get; private set; } = new GUIStyle(EditorStyles.boldLabel) { alignment= TextAnchor.MiddleCenter };
        public static GUIStyle dropDownHeaderButton { get; private set; } = new GUIStyle(EditorStyles.toolbarButton);
        public static GUIStyle bigTextureStyle { get; private set; } = new GUIStyle() { fontSize= 48 };
        public static GUIStyle vectorPropertyStyle { get; private set; } = new GUIStyle() { padding = new RectOffset(0, 0, 2, 2) };
        public static GUIStyle greenStyle { get; private set; } = new GUIStyle() { normal = new GUIStyleState() { textColor = new Color(0, 0.5f, 0) } };
        public static GUIStyle orangeStyle { get; private set; } = new GUIStyle() { normal = new GUIStyleState() { textColor = new Color(0.9f, 0.5f, 0) } };
        public static GUIStyle redStyle { get; private set; } = new GUIStyle() { normal = new GUIStyleState() { textColor = Color.red } };
        public static GUIStyle made_by_style { get; private set; } = new GUIStyle(EditorStyles.label) { fontSize = 10 };
        public static GUIStyle notification_style { get; private set; } = new GUIStyle(GUI.skin.box) { fontSize = 12, wordWrap = true, normal = new GUIStyleState() { textColor = Color.red } };
        
        public static GUIStyle style_toggle_left_richtext  { get; private set; } = new GUIStyle(EditorStyles.label ) { richText= true };

        public static GUIStyle none { get; private set; } = new GUIStyle();

        public static GUIStyle style_toolbar { get; private set; } = new GUIStyle(Styles.dropDownHeader) { };
        public static GUIStyle style_toolbar_toggle_active { get; private set; } = new GUIStyle(Styles.dropDownHeader) { normal = new GUIStyleState() {
            background = OverrideTextureWithColor(Styles.dropDownHeader.normal.background, COLOR_BACKGROUND_2)
        }, contentOffset = new Vector2(0, -2), alignment = TextAnchor.MiddleCenter};
        public static GUIStyle style_toolbar_toggle_unactive { get; private set; } = new GUIStyle(Styles.dropDownHeader) { alignment = TextAnchor.MiddleCenter, contentOffset = new Vector2(0, -2) };
        public static GUIStyle style_toolbar_toggle(bool active)
        {
            //hack fix. for some people bg texture seems to dissapear, i cant figure out why, so ill just check here and set it if it's gone
            if (active)
            {
                if (style_toolbar_toggle_active.normal.background == null)
                {
                    style_toolbar_toggle_active = new GUIStyle(Styles.dropDownHeader)
                    {
                        contentOffset = new Vector2(0, -2),
                        alignment = TextAnchor.MiddleCenter,
                        normal = new GUIStyleState()
                        {
                            textColor = Color.white,
                            background = OverrideTextureWithColor(Styles.dropDownHeader.normal.background, COLOR_BACKGROUND_2)
                        }
                    };
                }
                return style_toolbar_toggle_active;
            }
            return style_toolbar_toggle_unactive;
        }

        public static Texture2D t_arrow { get; private set; } = LoadTextureByFileName(RESOURCE_NAME.TEXTURE_ARROW);

        public static Texture2D rounded_texture          { get { return GetTextureOverwrittinWithColor(2,  RESOURCE_NAME.RECT          , COLOR_BACKGROUND_1     ); } }

        public static Texture2D icon_settings            { get { return GetTextureOverwrittinWithColor(3,  RESOURCE_NAME.ICON_SETTINGS , COLOR_ICON_GRAY             ); } }
        public static Texture2D icon_menu                { get { return GetTextureOverwrittinWithColor(4,  RESOURCE_NAME.ICON_NAME_MENU, COLOR_ICON_FONT             ); } }
        public static Texture2D icon_help                { get { return GetTextureOverwrittinWithColor(5,  RESOURCE_NAME.ICON_NAME_HELP, COLOR_ICON_FONT             ); } }
        public static Texture2D icon_search              { get { return GetTextureOverwrittinWithColor(6,  RESOURCE_NAME.ICON_SEARCH   , COLOR_ICON_GRAY             ); } }
        public static Texture2D icon_link_inactive       { get { return GetTextureOverwrittinWithColor(7,  RESOURCE_NAME.ICON_NAME_LINK, COLOR_ICON_FONT             ); } }
        public static Texture2D icon_link_active         { get { return GetTextureOverwrittinWithColor(8,  RESOURCE_NAME.ICON_NAME_LINK, COLOR_ICON_ACTIVE_CYAN ); } }

        public static Texture2D texture_animated         { get { return GetTextureOverwrittinWithColor(9,  RESOURCE_NAME.TEXTURE_ANIMTED, COLOR_ICON_FONT            ); } }
        public static Texture2D texture_animated_renamed { get { return GetTextureOverwrittinWithColor(10, RESOURCE_NAME.TEXTURE_ANIMTED, COLOR_ICON_ACTIVE_RED ); } }

        private static Texture2D[] _colorsWithTextures = new Texture2D[0];
        private static Texture2D GetTextureOverwrittinWithColor(int id, string textureName, Color c)
        {
            if(id >= _colorsWithTextures.Length)
            {
                Texture2D[] temp = new Texture2D[id + 1];
                Array.Copy(_colorsWithTextures, temp, _colorsWithTextures.Length);
                _colorsWithTextures = temp;
            }
            if(_colorsWithTextures[id] == null) _colorsWithTextures[id] = OverrideTextureWithColor(LoadTextureByFileName(textureName), c);
            return _colorsWithTextures[id];
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

        private static Texture2D CreateColorTexture(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        private static Texture2D OverrideTextureWithColor(Texture2D ogtex, Color color)
        {
            Texture2D tex = TextureHelper.GetReadableTexture(ogtex);
            for (int x = 0; x < tex.width; x++)
            {
                for (int y = 0; y < tex.height; y++)
                {
                    color.a = tex.GetPixel(x, y).a;
                    tex.SetPixel(x, y, color);
                }
            }
            tex.Apply();
            return tex;
        }
    }
}