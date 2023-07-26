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
        private static Color COLOR_ICON_GRAY = EditorGUIUtility.isProSkin ? COLOR_ICON_FONT : new Color(0.4f, 0.4f, 0.4f);
        public static Color COLOR_ICON_ACTIVE_CYAN = Color.cyan;
        private static Color COLOR_ICON_ACTIVE_RED = Color.red;
        public static Color COLOR_BACKGROUND_1 = EditorGUIUtility.isProSkin ? new Color(0.27f, 0.27f, 0.27f) : new Color(0.65f, 0.65f, 0.65f);
        public static Color COLOR_BACKGROUND_2 = EditorGUIUtility.isProSkin ? new Color(0.5f, 0.5f, 0.5f) : new Color(0.85f, 0.85f, 0.85f);

        public static GUIStyle dropDownHeaderLabel { get; private set; } = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter };
        public static GUIStyle label_align_right { get; private set; } = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.UpperRight };
        public static GUIStyle dropDownHeaderButton { get; private set; } = new GUIStyle(EditorStyles.toolbarButton);
        public static GUIStyle vectorPropertyStyle { get; private set; } = new GUIStyle() { padding = new RectOffset(0, 0, 2, 2) };
        public static GUIStyle greenStyle { get; private set; } = new GUIStyle() { normal = new GUIStyleState() { textColor = new Color(0, 0.5f, 0) } };
        public static GUIStyle animatedIndicatorStyle { get; private set; } = new GUIStyle() { normal = new GUIStyleState() { textColor = new Color(0.3f, 1, 0.3f) }, alignment = TextAnchor.MiddleRight };
        public static GUIStyle presetIndicatorStyle { get; private set; } = new GUIStyle() { normal = new GUIStyleState() { textColor = Color.cyan }, alignment = TextAnchor.MiddleRight };
        public static GUIStyle orangeStyle { get; private set; } = new GUIStyle() { normal = new GUIStyleState() { textColor = new Color(0.9f, 0.5f, 0) } };
        public static GUIStyle cyanStyle { get; private set; } = new GUIStyle() { normal = new GUIStyleState() { textColor = COLOR_ICON_ACTIVE_CYAN } };
        public static GUIStyle redStyle { get; private set; } = new GUIStyle() { normal = new GUIStyleState() { textColor = Color.red } };
        public static GUIStyle made_by_style { get; private set; } = new GUIStyle(EditorStyles.label) { fontSize = 10 };
        public static GUIStyle notification_style { get; private set; } = new GUIStyle(GUI.skin.box) { fontSize = 12, wordWrap = true, normal = new GUIStyleState() { textColor = Color.red } };

        public static GUIStyle style_toggle_left_richtext { get; private set; } = new GUIStyle(EditorStyles.label) { richText = true };
        public static GUIStyle richtext { get; private set; } = new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true };
        public static GUIStyle richtext_center { get; private set; } = new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true, alignment = TextAnchor.MiddleCenter };
        
        public static GUIStyle ButtonGreenText { get; private set; } = new GUIStyle(GUI.skin.button) { normal = new GUIStyleState() { textColor = new Color(0, 0.5f, 0) } };

        public static GUIStyle icon_style_help = CreateIconStyle(EditorGUIUtility.IconContent("_Help"));
        public static GUIStyle icon_style_menu = CreateIconStyle(EditorGUIUtility.IconContent("_Menu"));
        public static GUIStyle icon_style_settings = CreateIconStyle(EditorGUIUtility.IconContent("_Popup"));
        public static GUIStyle icon_style_search = CreateIconStyle(EditorGUIUtility.IconContent("Search Icon"));
        public static GUIStyle icon_style_presets = CreateIconStyle(EditorGUIUtility.IconContent("Preset.Context"));
        public static GUIStyle icon_style_add = CreateIconStyle(EditorGUIUtility.IconContent("PrefabOverlayAdded Icon"));
        public static GUIStyle icon_style_remove = CreateIconStyle(EditorGUIUtility.IconContent("PrefabOverlayRemoved Icon"));
        public static GUIStyle icon_style_refresh = CreateIconStyle(EditorGUIUtility.IconContent("d_Refresh"));
        public static GUIStyle icon_style_shaders = CreateIconStyle(EditorGUIUtility.IconContent("d_ShaderVariantCollection Icon"));
        public static GUIStyle icon_style_tools = CreateIconStyle(EditorGUIUtility.IconContent("d_SceneViewTools"));
        public static GUIStyle icon_style_linked = CreateIconStyle(LoadTextureByGUID(RESOURCE_GUID.ICON_LINK));
        public static GUIStyle icon_style_thryIcon = CreateIconStyle(LoadTextureByGUID(RESOURCE_GUID.ICON_THRY));

        public static Texture texture_icon_shaders = EditorGUIUtility.IconContent("d_ShaderVariantCollection Icon").image;

        static GUIStyle CreateIconStyle(GUIContent content)
        {
            return CreateIconStyle(content.image as Texture2D);
        }
        static GUIStyle CreateIconStyle(Texture2D texture)
        {
            return new GUIStyle()
            {
                stretchWidth = true,
                stretchHeight = true,
                fixedHeight = 0,
                fixedWidth = 0,
                normal = new GUIStyleState()
                {
                    background = texture
                }
            };
        }


        private static Texture2D LoadTextureByGUID(string guid)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if(path == null) return Texture2D.whiteTexture;
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}