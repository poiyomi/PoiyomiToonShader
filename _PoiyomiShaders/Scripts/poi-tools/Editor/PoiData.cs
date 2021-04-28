using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Poi
{
    internal static class PoiPaths
    {
        public const string defaultResourcesPath = "Library/unity default resources/";
    }

    internal static class PoiStyles
    {
        public static GUIStyle BigButton
        {
            get
            {
                if(_bigButton == null)
                    _bigButton= new GUIStyle("button")
                    {
                        fixedHeight = 18 * EditorGUIUtility.pixelsPerPoint
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
                        fontSize = 15,
                        stretchHeight = true,
                        clipping = TextClipping.Overflow
                    };

                return _titleLabel;
            }
        }

        static GUIStyle _bigButton;
        static GUIStyle _titleLabel;
    }
}
