using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor.Helpers
{
    public static class NotesHelper
    {
        /// <summary>
        /// This script computes how much width (in pixels) to reserve on the right side of a header 
        /// so that the Notes label doesn't overlap the packed icon row and the author badge (if provided). 
        /// Mirrors current DrawIcons packing: (menu, link, help, presets, author)
        /// </summary>
        public static float GetPackedRightReservation(Rect rect, PropertyOptions options, string sectionPropertyName, GUIStyle authorLabelStyle = null, float iconGap = 2f, float safetyPad = 2f)
        {
            // Match icon sizing as DrawIcons
            float iconSize = Mathf.Max(0f, rect.height - 4f);
            float step = iconSize + iconGap;

            // Always: menu, link
            int count = 0;
            count += 2;

            bool hasHelp = options?.button_help != null && options.button_help.condition_show.Test();
            bool hasPresets = Presets.DoesSectionHavePresets(sectionPropertyName);
            bool hasAuthor = options?.button_author != null && options.button_author.condition_show.Test();

            if (hasHelp) count++;
            if (hasPresets) count++;
            if (hasAuthor) count++;

            float reserved = count * step;

            // Calculates dynamic width to accommodate author text badge if it exists
            if (hasAuthor)
            {
                string authorText = options.button_author.text ?? string.Empty;
                if (authorText.Length > 0)
                {
                    GUIStyle labelStyle = Styles.label_property_note;
                    Vector2 textSize = labelStyle.CalcSize(new GUIContent(authorText));
                    float labelPad = 2f;
                    reserved += textSize.x + labelPad;
                }
            }

            return reserved + safetyPad;
        }
    }
}
