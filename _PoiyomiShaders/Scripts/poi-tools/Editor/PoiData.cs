using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Poi.Tools
{
    internal static class PoiPaths
    {
        public const string defaultResourcesPath = "Library/unity default resources/";
        public const string poiResourcesPath = "Poi/";
    }

    internal static class PoiStyles
    {
        public static GUIStyle BigButton
        {
            get
            {
                if(_bigButton == null)
                    _bigButton = new GUIStyle("button")
                    {
                        fixedHeight = 22 * EditorGUIUtility.pixelsPerPoint
                    };
                return _bigButton;
            }
        }
        
        public static GUIStyle TitleLabel
        {
            get
            {
                if(_titleLabel == null)
                    _titleLabel = new GUIStyle(EditorStyles.label)
                    {
                        fontSize = 15, stretchHeight = true, clipping = TextClipping.Overflow
                    };

                return _titleLabel;
            }
        }

        static GUIStyle _bigButton;
        static GUIStyle _titleLabel;
    }

    internal static class PoiIcons
    {
        public static Texture2D LinkIcon
        {
            get
            {
                if(!_linkIcon)
                {
                    string linkTexPath = EditorGUIUtility.isProSkin ? "icon_link_pro" : "icon_link";
                    _linkIcon = Resources.Load<Texture2D>(PoiPaths.poiResourcesPath + linkTexPath);
                }

                return _linkIcon;
            }
        }

        private static Texture2D _linkIcon;
    }
}


