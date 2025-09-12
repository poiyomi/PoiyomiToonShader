// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor
{
    public class Styles
    {
        public static readonly GUIStyle masterLabel = new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter };
        public static readonly GUIStyle editorHeaderLabel = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
        public static readonly GUIStyle dropdownHeader = new GUIStyle(new GUIStyle("ShurikenModuleTitle"))
        {
            font = new GUIStyle(EditorStyles.label).font,
            fontSize = GUI.skin.font.fontSize,
            border = new RectOffset(15, 7, 4, 4),
            fixedHeight = 22,
            contentOffset = new Vector2(20f, -2f)
        };

        public static readonly GUIStyle animatedIndicatorStyle = new GUIStyle() { normal = new GUIStyleState() { textColor = EditorGUIUtility.isProSkin ? new Color(0.3f, 1f, 0.3f) : new Color(0f, 0.5f, 0f) }, alignment = TextAnchor.MiddleRight };
        public static readonly GUIStyle presetIndicatorStyle = new GUIStyle() { normal = new GUIStyleState() { textColor = EditorGUIUtility.isProSkin ? new Color(0f, 1f, 1f) : new Color(0f, 0.5f, 0.71f) }, alignment = TextAnchor.MiddleRight };
        public static readonly GUIStyle madeByLabel = new GUIStyle(EditorStyles.label) { fontSize = 10 };
        public static readonly GUIStyle notification = new GUIStyle(GUI.skin.box) { fontSize = 12, wordWrap = true, normal = new GUIStyleState() { textColor = Color.red } };
        public static GUIStyle label_property_note { get; private set; } = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleRight, 
            padding = new RectOffset(0, 0, 0, 4), 
            normal = new GUIStyleState { textColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.6f) : new Color(0f, 0f, 0f, 1f) },
        };

        public static readonly GUIStyle vectorPropertyStyle = new GUIStyle() { padding = new RectOffset(0, 0, 2, 2) };

        public static readonly GUIStyle orangeStyle = new GUIStyle() { normal = new GUIStyleState() { textColor = new Color(0.9f, 0.5f, 0) } };
        public static readonly GUIStyle cyanStyle = new GUIStyle() { normal = new GUIStyleState() { textColor = Color.cyan } };
        public static readonly GUIStyle redStyle = new GUIStyle() { normal = new GUIStyleState() { textColor = Color.red } };
        public static readonly GUIStyle greenStyle = new GUIStyle() { normal = new GUIStyleState() { textColor = new Color(0, 0.5f, 0) } };

        
        public static readonly GUIStyle upperRight = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.UpperRight };
        public static readonly GUIStyle upperLeft_richText = new GUIStyle(EditorStyles.label) { richText = true };
        public static readonly GUIStyle upperLeft_richText_wordWrap = new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true };
        public static readonly GUIStyle middleCenter_richText_wordWrap = new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true, alignment = TextAnchor.MiddleCenter };
        
        public static readonly GUIStyle padding2pxHorizontal1pxVertical = new GUIStyle() { padding = new RectOffset(2, 2, 1, 1) };

        // Variant Stuff
        public static readonly GUIContent revertContent = EditorGUIUtility.TrTextContent("Revert");
        public static readonly GUIContent revertAllContent = EditorGUIUtility.TrTextContent("Revert all Overrides");
        public static readonly GUIContent lockContent = EditorGUIUtility.TrTextContent("Lock in children");
        public static readonly GUIContent lockOriginContent = EditorGUIUtility.TrTextContent("See lock origin");
        public static string revertMultiText = L10n.Tr("Revert on {0} Material(s)");
        public static string applyToMaterialText = L10n.Tr("Apply to Material '{0}'");
        public static string applyToVariantText = L10n.Tr("Apply as Override in Variant '{0}'");
        public static readonly GUIContent resetContent = EditorGUIUtility.TrTextContent("Reset");
    }

    public class Colors
    {
        public static readonly Color foreground = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f) : Color.black;

        public static readonly Color backgroundDark = EditorGUIUtility.isProSkin ? new Color(0.27f, 0.27f, 0.27f) : new Color(0.65f, 0.65f, 0.65f);
        public static readonly Color backgroundLight = EditorGUIUtility.isProSkin ? new Color(0.5f, 0.5f, 0.5f) : new Color(0.85f, 0.85f, 0.85f);
    }

    public class Icons
    {
        public static readonly GUIStyle help = CreateIconStyle(EditorGUIUtility.IconContent("_Help@2x"));
        public static readonly GUIStyle menu = CreateIconStyle(EditorGUIUtility.IconContent("_Menu"));
        public static readonly GUIStyle settings = CreateIconStyle(EditorGUIUtility.IconContent("_Popup@2x"));
        public static readonly GUIStyle search = CreateIconStyle(EditorGUIUtility.IconContent("Search Icon"));
        public static readonly GUIStyle presets = CreateIconStyle(EditorGUIUtility.IconContent("Preset.Context"));
        public static readonly GUIStyle add = CreateIconStyle(EditorGUIUtility.IconContent("PrefabOverlayAdded Icon"));
        public static readonly GUIStyle remove = CreateIconStyle(EditorGUIUtility.IconContent("PrefabOverlayRemoved Icon"));
        public static readonly GUIStyle refresh = CreateIconStyle(EditorGUIUtility.IconContent("d_Refresh"));
        public static readonly GUIStyle shaders = CreateIconStyle(EditorGUIUtility.IconContent("d_ShaderVariantCollection Icon"));
        public static readonly GUIStyle tools = CreateIconStyle(EditorGUIUtility.IconContent("d_SceneViewTools@2x"));
        public static readonly GUIStyle linked = CreateIconStyle(LoadTextureByGUID(RESOURCE_GUID.ICON_LINK));
        public static readonly GUIStyle thryIcon = CreateIconStyle(LoadTextureByGUID(RESOURCE_GUID.ICON_THRY));
        public static readonly GUIStyle github = CreateIconStyle(LoadTextureByGUID(RESOURCE_GUID.ICON_GITHUB));

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
