using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class DrawingData
    {
        public static ShaderTextureProperty CurrentTextureProperty;
        public static Rect LastGuiObjectRect;
        public static Rect LastGuiObjectHeaderRect;
        public static Rect TooltipCheckRect;
        public static float[] IconsPositioningHeights = new float[4];
        public static float IconsPositioningCount = 1;
        public static bool LastPropertyUsedCustomDrawer;
        public static bool LastPropertyDoesntAllowAnimation;
        public static MaterialPropertyDrawer LastPropertyDrawer;
        public static List<MaterialPropertyDrawer> LastPropertyDecorators = new List<MaterialPropertyDrawer>();
        public static bool IsEnabled = true;
        public static bool IsCollectingProperties = false;

        public static ShaderPart LastInitiatedPart;

        public static void ResetLastDrawerData()
        {
            LastPropertyUsedCustomDrawer = false;
            LastPropertyDoesntAllowAnimation = false;
            LastPropertyDrawer = null;
            LastPropertyDecorators.Clear();
        }

        public static void RegisterDecorator(MaterialPropertyDrawer drawer)
        {
            if (IsCollectingProperties)
            {
                LastPropertyDecorators.Add(drawer);
            }
        }
    }

}