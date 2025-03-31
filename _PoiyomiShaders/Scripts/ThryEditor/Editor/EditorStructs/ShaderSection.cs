using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor
{
    public class ShaderSection : ShaderGroup
    {

        const int BORDER_WIDTH = 3;
        const int HEADER_HEIGHT = 20;
        const int CHECKBOX_OFFSET = 20;

        public ShaderSection(ShaderEditor shaderEditor, MaterialProperty prop, MaterialEditor materialEditor, string displayName, int xOffset, string optionsRaw, int propertyIndex) : base(shaderEditor, prop, materialEditor, displayName, xOffset, optionsRaw, propertyIndex)
        {
        }

        protected override void DrawInternal(GUIContent content, Rect? rect = null, bool useEditorIndent = false, bool isInHeader = false)
        {
            if (Options.margin_top > 0)
            {
                GUILayoutUtility.GetRect(0, Options.margin_top);
            }

            ShaderProperty reference = Options.reference_property != null ? MyShaderUI.PropertyDictionary[Options.reference_property] : null;
            bool has_header = string.IsNullOrWhiteSpace(this.Content.text) == false || reference != null;

            int headerTextX = 18;
            int height = (has_header ? HEADER_HEIGHT : 0) + 4; // 4 for border margin

            // Draw border
            Rect border = EditorGUILayout.BeginVertical();
            border = new RectOffset(this.XOffset * -15 - 12, 3, -2, -2).Add(border);
            if (IsExpanded)
            {
                // Draw as border line
                Vector4 borderWidths = new Vector4(3, (has_header ? HEADER_HEIGHT : 3), 3, 3);
                GUI.DrawTexture(border, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Colors.backgroundDark, borderWidths, 5);
            }
            else
            {
                // Draw as solid
                GUI.DrawTexture(border, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Colors.backgroundDark, Vector4.zero, 5);
            }

            // Draw Reference
            Rect clickCheckRect = GUILayoutUtility.GetRect(0, height);
            if (reference != null)
            {
                EditorGUI.BeginChangeCheck();
                Rect referenceRect = new Rect(border.x + CHECKBOX_OFFSET, border.y + 1, HEADER_HEIGHT - 2, HEADER_HEIGHT - 2);
                reference.Draw(referenceRect, new GUIContent(), isInHeader: true, useEditorIndent: true);
                headerTextX = CHECKBOX_OFFSET + HEADER_HEIGHT;
                // Change expand state if reference is toggled
                if (EditorGUI.EndChangeCheck() && Options.ref_float_toggles_expand)
                {
                    IsExpanded = reference.MaterialProperty.GetNumber() == 1;
                }
            }

            // Draw Header (GUIContent)
            Rect top_border = new Rect(border.x, border.y - 2, border.width - 16, 22);
            if (has_header)
            {
                Rect header_rect = new RectOffset(headerTextX, 0, 0, 0).Remove(top_border);
                GUI.Label(header_rect, this.Content, EditorStyles.label);
            }

            // Toggling + Draw Arrow
            FoldoutArrow(top_border, Event.current);
            if (Event.current.type == EventType.MouseDown && clickCheckRect.Contains(Event.current.mousePosition))
            {
                IsExpanded = !IsExpanded;
                Event.current.Use();
            }

            // Draw Children
            if (IsExpanded)
            {
                EditorGUI.BeginDisabledGroup(DoDisableChildren);
                foreach (ShaderPart part in Children)
                {
                    part.Draw();
                }
                EditorGUI.EndDisabledGroup();
                GUILayoutUtility.GetRect(0, 5);
            }
            EditorGUILayout.EndVertical();
        }
    }

}