using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class GuiHelper
    {

        public static void drawConfigTextureProperty(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor, bool hasFoldoutProperties, bool skip_drag_and_drop_handling = false)
        {
            switch (Config.Get().default_texture_type)
            {
                case TextureDisplayType.small:
                    drawSmallTextureProperty(position, prop, label, editor, hasFoldoutProperties);
                    break;
                case TextureDisplayType.big:
                    if (DrawingData.currentTexProperty.reference_properties_exist)
                        drawStylizedBigTextureProperty(position, prop, label, editor, hasFoldoutProperties);
                    else
                        drawBigTextureProperty(position, prop, label, editor, DrawingData.currentTexProperty.hasScaleOffset);
                    break;

                case TextureDisplayType.stylized_big:
                    drawStylizedBigTextureProperty(position, prop, label, editor, hasFoldoutProperties, skip_drag_and_drop_handling);
                    break;
            }
        }

        public static void drawSmallTextureProperty(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor, bool hasFoldoutProperties)
        {
            Rect thumbnailPos = position;
            thumbnailPos.x += hasFoldoutProperties ? 20 : 0;
            editor.TexturePropertyMiniThumbnail(thumbnailPos, prop, label.text, (hasFoldoutProperties ? "Click here for extra properties" : "") + (label.tooltip != "" ? " | " : "") + label.tooltip);
            if (hasFoldoutProperties && DrawingData.currentTexProperty != null)
            {
                //draw dropdown triangle
                thumbnailPos.x += DrawingData.currentTexProperty.xOffset * 15;
                if (Event.current.type == EventType.Repaint)
                    EditorStyles.foldout.Draw(thumbnailPos, false, false, DrawingData.currentTexProperty.showFoldoutProperties, false);

                if (DrawingData.is_enabled)
                {
                    //test click and draw scale/offset
                    if (DrawingData.currentTexProperty.showFoldoutProperties)
                    {
                        if (DrawingData.currentTexProperty.hasScaleOffset)
                            ThryEditor.currentlyDrawing.editor.TextureScaleOffsetProperty(prop);

                        PropertyOptions options = DrawingData.currentTexProperty.options;
                        if (options.reference_properties != null)
                            foreach (string r_property in options.reference_properties)
                            {
                                ShaderProperty property = ThryEditor.currentlyDrawing.propertyDictionary[r_property];
                                EditorGUIUtility.labelWidth += EditorGUI.indentLevel * 15;
                                EditorGUI.indentLevel *= 2;
                                ThryEditor.currentlyDrawing.editor.ShaderProperty(property.materialProperty, property.content);
                                EditorGUI.indentLevel /= 2;
                                EditorGUIUtility.labelWidth -= EditorGUI.indentLevel * 15;
                            }
                    }
                    if (ThryEditor.input.MouseClick && position.Contains(Event.current.mousePosition))
                    {
                        DrawingData.currentTexProperty.showFoldoutProperties = !DrawingData.currentTexProperty.showFoldoutProperties;
                        editor.Repaint();
                    }
                }
            }

            DrawingData.lastGuiObjectHeaderRect = position;
            Rect object_rect = new Rect(position);
            object_rect.height = GUILayoutUtility.GetLastRect().y - object_rect.y + GUILayoutUtility.GetLastRect().height;
            DrawingData.lastGuiObjectRect = object_rect;
        }

        public static void drawBigTextureProperty(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor, bool scaleOffset)
        {
            Rect rect = GUILayoutUtility.GetRect(label, Styles.bigTextureStyle);
            float defaultLabelWidth = EditorGUIUtility.labelWidth;
            float defaultFieldWidth = EditorGUIUtility.fieldWidth;
            editor.SetDefaultGUIWidths();
            editor.TextureProperty(position, prop, label.text, label.tooltip, scaleOffset);
            EditorGUIUtility.labelWidth = defaultLabelWidth;
            EditorGUIUtility.fieldWidth = defaultFieldWidth;
            DrawingData.lastGuiObjectHeaderRect = position;
            Rect object_rect = new Rect(position);
            object_rect.height += rect.height;
            DrawingData.lastGuiObjectRect = object_rect;
        }

        static int texturePickerWindow = -1;
        static MaterialProperty texturePickerWindowProperty = null;
        public static void drawStylizedBigTextureProperty(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor, bool hasFoldoutProperties, bool skip_drag_and_drop_handling = false)
        {
            position.x += (EditorGUI.indentLevel) * 15;
            position.width -= (EditorGUI.indentLevel) * 15;
            Rect rect = GUILayoutUtility.GetRect(label, Styles.bigTextureStyle);
            rect.x += (EditorGUI.indentLevel) * 15;
            rect.width -= (EditorGUI.indentLevel) * 15;
            Rect border = new Rect(rect);
            border.position = new Vector2(border.x, border.y - position.height);
            border.height += position.height;

            if (DrawingData.currentTexProperty.reference_properties_exist)
            {
                border.height += 8;
                foreach (string r_property in DrawingData.currentTexProperty.options.reference_properties)
                {
                    border.height += editor.GetPropertyHeight(ThryEditor.currentlyDrawing.propertyDictionary[r_property].materialProperty);
                }
            }


            //background
            GUI.DrawTexture(border, Styles.rounded_texture, ScaleMode.StretchToFill, true);
            Rect quad = new Rect(border);
            quad.width = quad.height / 2;
            GUI.DrawTextureWithTexCoords(quad, Styles.rounded_texture, new Rect(0, 0, 0.5f, 1), true);
            quad.x += border.width - quad.width;
            GUI.DrawTextureWithTexCoords(quad, Styles.rounded_texture, new Rect(0.5f, 0, 0.5f, 1), true);

            quad.width = border.height - 4;
            quad.height = quad.width;
            quad.x = border.x + border.width - quad.width - 1;
            quad.y += 2;


            Rect preview_rect_border = new Rect(position);
            preview_rect_border.height = rect.height + position.height - 6;
            preview_rect_border.width = preview_rect_border.height;
            preview_rect_border.y += 3;
            preview_rect_border.x += position.width - preview_rect_border.width - 3;
            Rect preview_rect = new Rect(preview_rect_border);
            preview_rect.height -= 6;
            preview_rect.width -= 6;
            preview_rect.x += 3;
            preview_rect.y += 3;
            if (prop.hasMixedValue)
            {
                Rect mixedRect = new Rect(preview_rect);
                mixedRect.y -= 5;
                mixedRect.x += mixedRect.width / 2 - 4;
                GUI.Label(mixedRect, "_");
            }
            else if (prop.textureValue != null)
            {
                GUI.DrawTexture(preview_rect, prop.textureValue);
            }
            GUI.DrawTexture(preview_rect_border, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, Color.grey, 3, 5);

            //selection button and pinging
            Rect select_rect = new Rect(preview_rect);
            select_rect.height = 12;
            select_rect.y += preview_rect.height - 12;
            if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == texturePickerWindow && texturePickerWindowProperty == prop)
            {
                prop.textureValue = (Texture)EditorGUIUtility.GetObjectPickerObject();
                ThryEditor.repaint();
            }
            if (Event.current.commandName == "ObjectSelectorClosed" && EditorGUIUtility.GetObjectPickerControlID() == texturePickerWindow)
            {
                texturePickerWindow = -1;
                texturePickerWindowProperty = null;
            }
            if (GUI.Button(select_rect, "Select", EditorStyles.miniButton))
            {
                EditorGUIUtility.ShowObjectPicker<Texture>(prop.textureValue, false, "", 0);
                texturePickerWindow = EditorGUIUtility.GetObjectPickerControlID();
                texturePickerWindowProperty = prop;
            }
            else if (Event.current.type == EventType.MouseDown && preview_rect.Contains(Event.current.mousePosition))
            {
                EditorGUIUtility.PingObject(prop.textureValue);
            }

            if (!skip_drag_and_drop_handling)
                if ((ThryEditor.input.is_drag_drop_event) && preview_rect.Contains(ThryEditor.input.mouse_position) && DragAndDrop.objectReferences[0] is Texture)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (ThryEditor.input.is_drop_event)
                    {
                        DragAndDrop.AcceptDrag();
                        prop.textureValue = (Texture)DragAndDrop.objectReferences[0];
                    }
                }

            //scale offset rect
            if (hasFoldoutProperties)
            {
                if (DrawingData.currentTexProperty.hasScaleOffset)
                {
                    Rect scale_offset_rect = new Rect(position);
                    scale_offset_rect.y += 37;
                    scale_offset_rect.width -= 2 + preview_rect.width + 10;
                    editor.TextureScaleOffsetProperty(scale_offset_rect, prop);
                }

                PropertyOptions options = DrawingData.currentTexProperty.options;
                if (options.reference_properties != null)
                    foreach (string r_property in options.reference_properties)
                    {
                        ShaderProperty property = ThryEditor.currentlyDrawing.propertyDictionary[r_property];
                        EditorGUI.indentLevel *= 2;
                        ThryEditor.currentlyDrawing.editor.ShaderProperty(property.materialProperty, property.content);
                        EditorGUI.indentLevel /= 2;
                    }
            }

            Rect label_rect = new Rect(position);
            label_rect.x += 2;
            label_rect.y += 2;
            GUI.Label(label_rect, label);

            GUILayoutUtility.GetRect(0, 5);

            DrawingData.lastGuiObjectHeaderRect = position;
            DrawingData.lastGuiObjectRect = border;
        }

        const float kNumberWidth = 65;

        public static void MinMaxSlider(Rect settingsRect, GUIContent content, MaterialProperty prop)
        {
            bool changed = false;
            Vector4 vec = prop.vectorValue;
            Rect sliderRect = settingsRect;

            EditorGUI.LabelField(settingsRect, content);

            float capAtX = vec.x;
            float capAtY = vec.y;

            if (settingsRect.width > 160)
            {
                Rect numberRect = settingsRect;
                numberRect.width = kNumberWidth + (EditorGUI.indentLevel - 1) * 15;

                numberRect.x = EditorGUIUtility.labelWidth - (EditorGUI.indentLevel - 1) * 15;

                EditorGUI.BeginChangeCheck();
                vec.x = EditorGUI.FloatField(numberRect, vec.x, EditorStyles.textField);
                changed |= EditorGUI.EndChangeCheck();

                numberRect.x = settingsRect.xMax - numberRect.width;

                EditorGUI.BeginChangeCheck();
                vec.y = EditorGUI.FloatField(numberRect, vec.y);
                changed |= EditorGUI.EndChangeCheck();

                sliderRect.xMin = EditorGUIUtility.labelWidth - (EditorGUI.indentLevel - 1) * 15;
                sliderRect.xMin += (kNumberWidth + -8);
                sliderRect.xMax -= (kNumberWidth + -8);
            }

            vec.x = Mathf.Clamp(vec.x, vec.z, capAtY);
            vec.y = Mathf.Clamp(vec.y, capAtX, vec.w);

            EditorGUI.BeginChangeCheck();
            EditorGUI.MinMaxSlider(sliderRect, ref vec.x, ref vec.y, vec.z, vec.w);
            changed |= EditorGUI.EndChangeCheck();

            if (changed)
            {
                prop.vectorValue = vec;
            }
        }

        public static bool DrawListField<type>(List<type> list) where type : UnityEngine.Object
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add", EditorStyles.miniButton))
                list.Add(null);
            if (GUILayout.Button("Remove", EditorStyles.miniButton))
                if (list.Count > 0)
                    list.RemoveAt(list.Count - 1);
            GUILayout.EndHorizontal();

            for (int i = 0; i < list.Count; i++)
            {
                list[i] = (type)EditorGUILayout.ObjectField(list[i], typeof(type), false);
            }
            return false;
        }

        public static bool GUIDataStruct<t>(t data)
        {
            return GUIDataStruct<t>(data, new string[] { });
        }

        public static bool GUIDataStruct<t>(t data, string[] exclude)
        {
            Type type = data.GetType();
            bool changed = false;
            foreach (FieldInfo f in type.GetFields())
            {
                bool skip = false;
                foreach (string s in exclude)
                    if (s == f.Name)
                        skip = true;
                if (skip)
                    continue;

                if (f.FieldType.IsEnum)
                    changed |= GUIEnum(f, data);
                else if (f.FieldType == typeof(string))
                    changed |= GUIString(f, data);
                else if (f.FieldType == typeof(int))
                    changed |= GUIInt(f, data);
                else if (f.FieldType == typeof(float))
                    changed |= GUIFloat(f, data);
            }
            return changed;
        }

        private static bool GUIEnum(FieldInfo f, object o)
        {
            EditorGUI.BeginChangeCheck();
            Enum e = EditorGUILayout.EnumPopup(f.Name, (Enum)f.GetValue(o), GUILayout.ExpandWidth(false));
            bool changed = EditorGUI.EndChangeCheck();
            if (changed)
                f.SetValue(o, e);
            return changed;
        }

        private static bool GUIString(FieldInfo f, object o)
        {
            EditorGUI.BeginChangeCheck();
            string s = EditorGUILayout.TextField(f.Name, (string)f.GetValue(o), GUILayout.ExpandWidth(false));
            bool changed = EditorGUI.EndChangeCheck();
            if (changed)
                f.SetValue(o, s);
            return changed;
        }

        private static bool GUIInt(FieldInfo f, object o)
        {
            EditorGUI.BeginChangeCheck();
            int i = EditorGUILayout.IntField(f.Name, (int)f.GetValue(o), GUILayout.ExpandWidth(false));
            bool changed = EditorGUI.EndChangeCheck();
            if (changed)
                f.SetValue(o, i);
            return changed;
        }

        private static bool GUIFloat(FieldInfo f, object o)
        {
            EditorGUI.BeginChangeCheck();
            float i = EditorGUILayout.FloatField(f.Name, (float)f.GetValue(o), GUILayout.ExpandWidth(false));
            bool changed = EditorGUI.EndChangeCheck();
            if (changed)
                f.SetValue(o, i);
            return changed;
        }

        //draw the render queue selector
        public static int drawRenderQueueSelector(Shader defaultShader, int customQueueFieldInput)
        {
            EditorGUILayout.BeginHorizontal();
            if (customQueueFieldInput == -1) customQueueFieldInput = ThryEditor.currentlyDrawing.materials[0].renderQueue;
            int[] queueOptionsQueues = new int[] { defaultShader.renderQueue, 2000, 2450, 3000, customQueueFieldInput };
            string[] queueOptions = new string[] { "From Shader", "Geometry", "Alpha Test", "Transparency" };
            int queueSelection = 4;
            if (defaultShader.renderQueue == customQueueFieldInput) queueSelection = 0;
            else
            {
                string customOption = null;
                int q = customQueueFieldInput;
                if (q < 2000) customOption = queueOptions[1] + "-" + (2000 - q);
                else if (q < 2450) { if (q > 2000) customOption = queueOptions[1] + "+" + (q - 2000); else queueSelection = 1; }
                else if (q < 3000) { if (q > 2450) customOption = queueOptions[2] + "+" + (q - 2450); else queueSelection = 2; }
                else if (q < 5001) { if (q > 3000) customOption = queueOptions[3] + "+" + (q - 3000); else queueSelection = 3; }
                if (customOption != null) queueOptions = new string[] { "From Shader", "Geometry", "Alpha Test", "Transparency", customOption };
            }
            EditorGUILayout.LabelField("Render Queue", GUILayout.ExpandWidth(true));
            int newQueueSelection = EditorGUILayout.Popup(queueSelection, queueOptions, GUILayout.MaxWidth(100));
            int newQueue = queueOptionsQueues[newQueueSelection];
            if (queueSelection != newQueueSelection) customQueueFieldInput = newQueue;
            int newCustomQueueFieldInput = EditorGUILayout.DelayedIntField(customQueueFieldInput, GUILayout.MaxWidth(65));
            bool isInput = customQueueFieldInput != newCustomQueueFieldInput || queueSelection != newQueueSelection;
            customQueueFieldInput = newCustomQueueFieldInput;
            foreach (Material m in ThryEditor.currentlyDrawing.materials)
                if (customQueueFieldInput != m.renderQueue && isInput) m.renderQueue = customQueueFieldInput;
            if (customQueueFieldInput != ThryEditor.currentlyDrawing.materials[0].renderQueue && !isInput) customQueueFieldInput = ThryEditor.currentlyDrawing.materials[0].renderQueue;
            EditorGUILayout.EndHorizontal();
            return customQueueFieldInput;
        }

        //draw all collected footers
        public static void drawFooters(List<ButtonData> footers)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Space(2);
            foreach (ButtonData foot in footers)
            {
                drawFooter(foot);
                GUILayout.Space(2);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawLocaleSelection(GUIContent label, string[] locales, int selected)
        {
            EditorGUI.BeginChangeCheck();
            selected = EditorGUILayout.Popup(label.text, selected, locales);
            if (EditorGUI.EndChangeCheck())
            {
                ThryEditor.currentlyDrawing.propertyDictionary[ThryEditor.PROPERTY_NAME_LOCALE].materialProperty.floatValue = selected;
                ThryEditor.reload();
            }
        }

        //draw single footer
        private static void drawFooter(ButtonData data)
        {
            Button(data, 20);
        }

        public static void Button(ButtonData button)
        {
            Button(button, -1);
        }

        public static void Button(ButtonData button, int default_height)
        {
            GUIContent content;
            Rect cursorRect;
            if (button != null)
            {
                if (button.texture == null)
                {
                    content = new GUIContent(button.text, button.hover);
                    if (default_height != -1)
                    {
                        if (GUILayout.Button(content, GUILayout.ExpandWidth(false), GUILayout.Height(default_height)))
                            button.action.Perform();
                    }
                    else
                    {
                        if (GUILayout.Button(content, GUILayout.ExpandWidth(false)))
                            button.action.Perform();
                    }
                    cursorRect = GUILayoutUtility.GetLastRect();
                }
                else
                {
                    GUILayout.Space(4);
                    content = new GUIContent(button.texture.GetTextureFromName(), button.hover);
                    int height = (button.texture.height == 128 && default_height != -1) ? default_height : button.texture.height;
                    int width = (int)((float)button.texture.loaded_texture.width / button.texture.loaded_texture.height * height);
                    if (GUILayout.Button(new GUIContent(button.texture.loaded_texture, button.hover), new GUIStyle(), GUILayout.MaxWidth(width), GUILayout.Height(height)))
                        button.action.Perform();
                    cursorRect = GUILayoutUtility.GetLastRect();
                    GUILayout.Space(4);
                }
                EditorGUIUtility.AddCursorRect(cursorRect, MouseCursor.Link);
            }
        }

        public static void DrawHeader(ref bool enabled, ref bool options, GUIContent name)
        {
            var r = EditorGUILayout.BeginHorizontal("box");
            enabled = EditorGUILayout.Toggle(enabled, EditorStyles.radioButton, GUILayout.MaxWidth(15.0f));
            options = GUI.Toggle(r, options, GUIContent.none, new GUIStyle());
            EditorGUILayout.LabelField(name, Styles.dropDownHeaderLabel);
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawMasterLabel(string shaderName, float y)
        {
            Rect rect = new Rect(0, y, Screen.width, 18);
            EditorGUI.LabelField(rect, "<size=16>" + shaderName + "</size>", Styles.masterLabel);
        }
    }
}