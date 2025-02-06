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
        public static bool IsEnabled = true;
    }

}