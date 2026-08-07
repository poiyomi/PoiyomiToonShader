#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Thry.ThryEditor;

namespace Poi.Tools
{
    /// <summary>
    /// Material property decorator that draws a "Bake Color Adjust" button.
    /// Usage in shader: [PoiBakeColorAdjust] _BakeColorAdjustButton ("", Float) = 0
    /// </summary>
    public class PoiBakeColorAdjustDecorator : MaterialPropertyDrawer
    {
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            ShaderProperty.RegisterDecorator(this);
            return 26;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            position = EditorGUI.IndentedRect(position);
            position.y += 2;
            position.height = 22;

            // Check if there are changes to bake
            Material material = editor.target as Material;
            bool hasChanges = material != null && PoiColorAdjustBaker.HasColorAdjustChanges(material);

            EditorGUI.BeginDisabledGroup(!hasChanges);
            if (GUI.Button(position, "Bake Color Adjust"))
            {
                // Support multi-material editing
                foreach (var target in editor.targets)
                {
                    if (target is Material mat)
                        PoiColorAdjustBaker.BakeColorAdjust(mat);
                }
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}
#endif
