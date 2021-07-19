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
        public static GUIStyle dropDownHeader { get; private set; } = new GUIStyle(new GUIStyle("ShurikenModuleTitle"))
        {
            font = new GUIStyle(EditorStyles.label).font,
            fontSize = GUI.skin.font.fontSize,
            border = new RectOffset(15, 7, 4, 4),
            fixedHeight = 22,
            contentOffset = new Vector2(20f, -2f)
        };

        public static Color backgroundColor { get; private set; } = (EditorGUIUtility.isProSkin) ? new Color(0.4f, 0.4f, 0.4f) : new Color(0.8f, 0.8f, 0.8f);
        public static Color backgroundColorTransparent { get; private set; } = (EditorGUIUtility.isProSkin) ? new Color(0.4f, 0.4f, 0.4f, 0.3f) : new Color(0.8f, 0.8f, 0.8f, 0.3f);
        public static Color forgroundColor { get; private set; } = (EditorGUIUtility.isProSkin) ? new Color(0.8f, 0.8f, 0.8f) : Color.black;

        public static GUIStyle dropDownHeaderLabel { get; private set; } = new GUIStyle(EditorStyles.boldLabel) { alignment= TextAnchor.MiddleCenter };
        public static GUIStyle dropDownHeaderButton { get; private set; } = new GUIStyle(EditorStyles.toolbarButton);
        public static GUIStyle bigTextureStyle { get; private set; } = new GUIStyle() { fontSize= 48 };
        public static GUIStyle vectorPropertyStyle { get; private set; } = new GUIStyle() { padding = new RectOffset(0, 0, 2, 2) };
        public static GUIStyle greenStyle { get; private set; } = new GUIStyle() { normal = new GUIStyleState() { textColor = new Color(0, 0.5f, 0) } };
        public static GUIStyle yellowStyle { get; private set; } = new GUIStyle() { normal = new GUIStyleState() { textColor = new Color(1, 0.79f, 0) } };
        public static GUIStyle redStyle { get; private set; } = new GUIStyle() { normal = new GUIStyleState() { textColor = Color.red } };
        public static GUIStyle made_by_style { get; private set; } = new GUIStyle() { fontSize = 10 };
        public static GUIStyle notification_style { get; private set; } = new GUIStyle(GUI.skin.box) { fontSize = 12, wordWrap = true, normal = new GUIStyleState() { textColor = Color.red } };

        public static GUIStyle none { get; private set; } = new GUIStyle();

        public static GUIStyle style_toolbar { get; private set; } = new GUIStyle(Styles.dropDownHeader) { };
        public static GUIStyle style_toolbar_toggle_active { get; private set; } = new GUIStyle(Styles.dropDownHeader) { normal = new GUIStyleState() {
            background = MultiplyTextureWithColor(Styles.dropDownHeader.normal.background, new Color(1, 1, 1, 1)),
            textColor = Color.white}, contentOffset = new Vector2(0, -2), alignment = TextAnchor.MiddleCenter};
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
                            background = MultiplyTextureWithColor(Styles.dropDownHeader.normal.background, new Color(1, 1, 1, 1))
                        }
                    };
                }
                return style_toolbar_toggle_active;
            }
            return style_toolbar_toggle_unactive;
        }

        public static Texture2D rounded_texture { get; private set; } = LoadTextureByNameAndEditorType(RESOURCE_NAME.WHITE_RECT, RESOURCE_NAME.DARK_RECT);
        public static Texture2D settings_icon { get; private set; } = LoadTextureByFileName(RESOURCE_NAME.SETTINGS_ICON_TEXTURE);
        public static Texture2D icon_menu { get; private set; } = LoadTextureByFileName(RESOURCE_NAME.ICON_NAME_MENU);
        public static Texture2D icon_help { get; private set; } = LoadTextureByFileName(RESOURCE_NAME.ICON_NAME_HELP);
        public static Texture2D icon_link_active { get; private set; } = OverrideTextureWithColor(LoadTextureByFileName(RESOURCE_NAME.ICON_NAME_LINK), Color.cyan);
        public static Texture2D icon_link_inactive { get; private set; } = LoadTextureByFileName(RESOURCE_NAME.ICON_NAME_LINK);
        public static Texture2D visibility_icon { get; private set; } = LoadTextureByFileName(RESOURCE_NAME.VISIVILITY_ICON);
        public static Texture2D search_icon { get; private set; } = LoadTextureByFileName(RESOURCE_NAME.SEARCH_ICON);
        public static Texture2D presets_icon { get; private set; } = LoadTextureByFileName(RESOURCE_NAME.PRESETS_ICON);
        public static Texture2D t_arrow { get; private set; } = LoadTextureByFileName(RESOURCE_NAME.TEXTURE_ARROW);
        public static Texture2D texture_animated { get; private set; } = LoadTextureByFileName(RESOURCE_NAME.TEXTURE_ANIMTED);
        private static Texture2D t_texture_animated_renamed;
        public static Texture2D texture_animated_renamed { 
            get { 
                if(t_texture_animated_renamed == null)
                    t_texture_animated_renamed = OverrideTextureWithColor(LoadTextureByFileName(RESOURCE_NAME.TEXTURE_ANIMTED), Color.red);
                return t_texture_animated_renamed;
            } 
        }

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

        private static Texture2D CreateColorTexture(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        private static Texture2D MultiplyTextureWithColor(Texture2D ogtex, Color color)
        {
            Texture2D tex = TextureHelper.GetReadableTexture(ogtex);
            for(int x = 0; x < tex.width; x++)
            {
                for (int y = 0; y < tex.height; y++)
                {
                    Color oColor = tex.GetPixel(x, y);
                    tex.SetPixel(x, y, oColor * color);
                }
            }
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
                    Color oColor = tex.GetPixel(x, y);
                    if (oColor.a == 0f)
                        continue;
                    tex.SetPixel(x, y, color);
                }
            }
            tex.Apply();
            return tex;
        }
    }
}