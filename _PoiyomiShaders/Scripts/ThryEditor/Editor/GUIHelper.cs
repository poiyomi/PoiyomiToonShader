using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
                    if (DrawingData.currentTexProperty.reference_properties_exist || DrawingData.currentTexProperty.reference_property_exists)
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
            if (DrawingData.currentTexProperty.reference_property_exists)
            {
                ShaderProperty property = ShaderEditor.currentlyDrawing.propertyDictionary[DrawingData.currentTexProperty.options.reference_property];
                Rect r = position;
                r.x += EditorGUIUtility.labelWidth - CurrentIndentWidth();
                r.width -= EditorGUIUtility.labelWidth - CurrentIndentWidth();
                property.Draw(new CRect(r), new GUIContent());
            }
            if (hasFoldoutProperties && DrawingData.currentTexProperty != null)
            {
                //draw dropdown triangle
                thumbnailPos.x += DrawingData.currentTexProperty.xOffset * 15;
                //This is an invisible button with zero functionality. But it needs to be here so that the triangle click reacts fast
                if (GUI.Button(thumbnailPos, "", Styles.none));
                if (Event.current.type == EventType.Repaint)
                    EditorStyles.foldout.Draw(thumbnailPos, false, false, DrawingData.currentTexProperty.showFoldoutProperties, false);

                if (DrawingData.is_enabled)
                {
                    //test click and draw scale/offset
                    if (DrawingData.currentTexProperty.showFoldoutProperties)
                    {
                        EditorGUI.indentLevel += 2;
                        if (DrawingData.currentTexProperty.hasScaleOffset)
                        {
                            ShaderEditor.currentlyDrawing.editor.TextureScaleOffsetProperty(prop);
                            if(DrawingData.currentTexProperty.is_animatable)
                                DrawingData.currentTexProperty.HandleKajAnimatable();
                        }

                        PropertyOptions options = DrawingData.currentTexProperty.options;
                        if (options.reference_properties != null)
                            foreach (string r_property in options.reference_properties)
                            {
                                ShaderProperty property = ShaderEditor.currentlyDrawing.propertyDictionary[r_property];
                                property.Draw(useEditorIndent: true);
                            }
                        EditorGUI.indentLevel -= 2;
                    }
                    if (ShaderEditor.input.MouseLeftClick && position.Contains(Event.current.mousePosition))
                    {
                        ShaderEditor.input.Use();
                        DrawingData.currentTexProperty.showFoldoutProperties = !DrawingData.currentTexProperty.showFoldoutProperties;
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
                    border.height += editor.GetPropertyHeight(ShaderEditor.currentlyDrawing.propertyDictionary[r_property].materialProperty);
                }
            }
            if (DrawingData.currentTexProperty.reference_property_exists)
            {
                border.height += editor.GetPropertyHeight(ShaderEditor.currentlyDrawing.propertyDictionary[DrawingData.currentTexProperty.options.reference_property].materialProperty);
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
            if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == texturePickerWindow && texturePickerWindowProperty.name == prop.name)
            {
                prop.textureValue = (Texture)EditorGUIUtility.GetObjectPickerObject();
                ShaderEditor.repaint();
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
                if ((ShaderEditor.input.is_drag_drop_event) && preview_rect.Contains(ShaderEditor.input.mouse_position) && DragAndDrop.objectReferences[0] is Texture)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (ShaderEditor.input.is_drop_event)
                    {
                        DragAndDrop.AcceptDrag();
                        prop.textureValue = (Texture)DragAndDrop.objectReferences[0];
                    }
                }

            //scale offset rect

            if (hasFoldoutProperties)
            {
                EditorGUI.indentLevel += 2;

                if (DrawingData.currentTexProperty.hasScaleOffset)
                {
                    Rect scale_offset_rect = new Rect(position);
                    scale_offset_rect.y += 37;
                    scale_offset_rect.width -= 2 + preview_rect.width + 10 + 30;
                    scale_offset_rect.x += 30;
                    editor.TextureScaleOffsetProperty(scale_offset_rect, prop);
                    if (DrawingData.currentTexProperty.is_animatable)
                        DrawingData.currentTexProperty.HandleKajAnimatable();
                }
                float oldLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 128;

                PropertyOptions options = DrawingData.currentTexProperty.options;
                if (options.reference_property != null)
                {
                    ShaderProperty property = ShaderEditor.currentlyDrawing.propertyDictionary[options.reference_property];
                    ShaderEditor.currentlyDrawing.editor.ShaderProperty(property.materialProperty, property.content);
                }
                if (options.reference_properties != null)
                    foreach (string r_property in options.reference_properties)
                    {
                        ShaderProperty property = ShaderEditor.currentlyDrawing.propertyDictionary[r_property];
                        ShaderEditor.currentlyDrawing.editor.ShaderProperty(property.materialProperty, property.content);
                        if (DrawingData.currentTexProperty.is_animatable)
                            property.HandleKajAnimatable();
                    }
                EditorGUIUtility.labelWidth = oldLabelWidth;
                EditorGUI.indentLevel -= 2;
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
        public static bool DrawListField<type>(List<type> list, float maxHeight, ref Vector2 scrollPosition) where type : UnityEngine.Object
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add", EditorStyles.miniButton))
                list.Add(null);
            if (GUILayout.Button("Remove", EditorStyles.miniButton))
                if (list.Count > 0)
                    list.RemoveAt(list.Count - 1);
            GUILayout.EndHorizontal();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(maxHeight));
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = (type)EditorGUILayout.ObjectField(list[i], typeof(type), false);
            }
            GUILayout.EndScrollView();
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
            if (customQueueFieldInput == -1) customQueueFieldInput = ShaderEditor.currentlyDrawing.materials[0].renderQueue;
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
            foreach (Material m in ShaderEditor.currentlyDrawing.materials)
                if (customQueueFieldInput != m.renderQueue && isInput) m.renderQueue = customQueueFieldInput;
            if (customQueueFieldInput != ShaderEditor.currentlyDrawing.materials[0].renderQueue && !isInput) customQueueFieldInput = ShaderEditor.currentlyDrawing.materials[0].renderQueue;
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
                ShaderEditor.currentlyDrawing.propertyDictionary[ShaderEditor.PROPERTY_NAME_LOCALE].materialProperty.floatValue = selected;
                ShaderEditor.reload();
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

        public static void DrawMasterLabel(string shaderName, Rect parent)
        {
            Rect rect = new Rect(0, parent.y, parent.width, 18);
            EditorGUI.LabelField(rect, "<size=16>" + shaderName + "</size>", Styles.masterLabel);
        }

        public static void DrawNotificationBox(Rect position, int width, int height, string text)
        {
            Rect box_position = new Rect(position.x- width + position.width, position.y + position.height + 50, width,height);
            Rect arrow_position = new Rect(position.x - 25, position.y + position.height, 50, 50);
            GUI.DrawTexture(arrow_position, Styles.t_arrow, ScaleMode.ScaleToFit, true, 0, Color.red, 0, 0);
            GUI.Box(box_position, text, Styles.notification_style);
        }

        public static float CurrentIndentWidth()
        {
            return EditorGUI.indentLevel * 15;
        }
    }

    public class ShaderEditorHeader
    {
        private MaterialProperty property;

        private bool expanded;

        //private string DATA_KEY;

        public ShaderEditorHeader(MaterialProperty prop)
        {
            //DATA_KEY = "Header_" + prop.name + "_State";
            //this.expanded = PersistentData.Get(DATA_KEY) == "expanded";
            this.property = prop;
            this.expanded = prop.floatValue == 1;
        }

        public bool is_expanded
        {
            get
            {
                return expanded;
            }
        }

        public void Toggle()
        {
            expanded = !expanded;
            if (!ShaderEditor.AnimationIsRecording)
            {
                if (expanded)
                    property.floatValue = 1;
                else
                    property.floatValue = 0;
            }
        }

        public void Foldout(int xOffset, GUIContent content, ShaderEditor gui)
        {
            PropertyOptions options = ShaderEditor.currentlyDrawing.currentProperty.options;
            Event e = Event.current;
            GUIStyle style = new GUIStyle(Styles.dropDownHeader);
            style.margin.left = 15 * xOffset + 15;

            Rect rect = GUILayoutUtility.GetRect(16f + 20f, 22f, style);
            DrawingData.lastGuiObjectHeaderRect = rect;

            DrawBoxAndContent(rect, e, content, options, style);

            DrawSmallArrow(rect, e);
            HandleToggleInput(e, rect);
        }

        private void DrawBoxAndContent(Rect rect, Event e, GUIContent content, PropertyOptions options, GUIStyle style)
        {
            if (options.reference_property != null)
            {
                GUI.Box(rect, new GUIContent("     " + content.text, content.tooltip), style);
                DrawIcons(rect, e);
                DrawButton(rect, options, e, style);

                Rect togglePropertyRect = new Rect(rect);
                togglePropertyRect.x += 5;
                togglePropertyRect.y += 2;
                togglePropertyRect.height -= 4;
                togglePropertyRect.width = GUI.skin.font.fontSize * 3;
                float fieldWidth = EditorGUIUtility.fieldWidth;
                EditorGUIUtility.fieldWidth = 20;
                ShaderProperty prop = ShaderEditor.currentlyDrawing.propertyDictionary[options.reference_property];

                int xOffset = prop.xOffset;
                prop.xOffset = 0;
                prop.Draw(new CRect(togglePropertyRect), new GUIContent());
                prop.xOffset = xOffset;
                EditorGUIUtility.fieldWidth = fieldWidth;
            }
            else
            {
                GUI.Box(rect, content, style);
                DrawIcons(rect, e);
                DrawButton(rect, options, e, style);
            }

        }

        /// <summary>
        /// Draws extra buttons in the header
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="options"></param>
        /// <param name="e"></param>
        /// <param name="style"></param>
        private void DrawButton(Rect rect, PropertyOptions options, Event e, GUIStyle style)
        {
            if (options.button_right != null && options.button_right.condition_show.Test())
            {
                Rect buttonRect = new Rect(rect);
                GUIContent buttoncontent = new GUIContent(options.button_right.text, options.button_right.hover);
                float width = Styles.dropDownHeaderButton.CalcSize(buttoncontent).x;
                width = width < rect.width / 3 ? rect.width / 3 : width;
                buttonRect.x += buttonRect.width - width - 50;
                buttonRect.y += 2;
                buttonRect.width = width;
                if (GUI.Button(buttonRect, buttoncontent, Styles.dropDownHeaderButton))
                {
                    e.Use();
                    if (options.button_right.action != null)
                        options.button_right.action.Perform();
                }
                EditorGUIUtility.AddCursorRect(buttonRect, MouseCursor.Link);
            }
        }

        /// <summary>
        /// Draws the icons for ShaderEditor features like linking and copying
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="e"></param>
        private void DrawIcons(Rect rect, Event e)
        {
            DrawDowdownSettings(rect, e);
            DrawLinkSettings(rect, e);
        }

        private void DrawDowdownSettings(Rect rect, Event e)
        {
            Rect buttonRect = new Rect(rect);
            buttonRect.width = 20;
            buttonRect.x += rect.width - 25;
            buttonRect.y += 1;
            buttonRect.height -= 4;
            if (GUI.Button(buttonRect, Styles.dropdown_settings_icon, EditorStyles.largeLabel))
            {
                e.Use();

                buttonRect.width = 150;
                buttonRect.x = Mathf.Min(Screen.width - buttonRect.width, buttonRect.x);
                buttonRect.height = 60;
                float maxY = GUIUtility.ScreenToGUIPoint(new Vector2(0, EditorWindow.focusedWindow.position.y + Screen.height)).y - 2.5f * buttonRect.height;
                buttonRect.y = Mathf.Min(buttonRect.y - buttonRect.height / 2, maxY);

                ShowHeaderContextMenu(buttonRect, ShaderEditor.currentlyDrawing.currentProperty, ShaderEditor.currentlyDrawing.materials[0]);
            }
        }

        private void DrawLinkSettings(Rect rect, Event e)
        {
            Rect buttonRect = new Rect(rect);
            buttonRect.width = 20;
            buttonRect.x += rect.width - 45;
            buttonRect.y += 1;
            buttonRect.height -= 4;
            List<Material> linked_materials = MaterialLinker.GetLinked(ShaderEditor.currentlyDrawing.currentProperty.materialProperty);
            Texture2D icon = Styles.inactive_link_icon;
            if (linked_materials != null)
                icon = Styles.active_link_icon;
            if (GUI.Button(buttonRect, icon, EditorStyles.largeLabel))
            {
                MaterialLinker.Popup(buttonRect, linked_materials, ShaderEditor.currentlyDrawing.currentProperty.materialProperty);
                e.Use();
            }
        }

        void ShowHeaderContextMenu(Rect position, ShaderPart property, Material material)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Reset"), false, delegate ()
            {
                property.CopyFromMaterial(new Material(material.shader));
                List<Material> linked_materials = MaterialLinker.GetLinked(property.materialProperty);
                if (linked_materials != null)
                    foreach (Material m in linked_materials)
                        property.CopyToMaterial(m);
            });
            menu.AddItem(new GUIContent("Copy"), false, delegate ()
            {
                Mediator.copy_material = new Material(material);
            });
            menu.AddItem(new GUIContent("Paste"), false, delegate ()
            {
                if (Mediator.copy_material != null)
                {
                    property.CopyFromMaterial(Mediator.copy_material);
                    List<Material> linked_materials = MaterialLinker.GetLinked(property.materialProperty);
                    if (linked_materials != null)
                        foreach (Material m in linked_materials)
                            property.CopyToMaterial(m);
                }
            });
            menu.DropDown(position);
        }

        private void DrawSmallArrow(Rect rect, Event e)
        {
            if (e.type == EventType.Repaint)
            {
                var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
                EditorStyles.foldout.Draw(toggleRect, false, false, expanded, false);
            }
        }

        private void HandleToggleInput(Event e, Rect rect)
        {
            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition) && !e.alt)
            {
                this.Toggle();
                e.Use();
            }
        }
    }

    public class HeaderHider{

        public enum HeaderHidingType
        {
            simple = 1,
            show_all = 2,
            custom=3
        }

        private static Dictionary<string,bool> headerHiddenSaved;
        public static HeaderHidingType state { get; private set; }
        private static void LoadHiddenHeaderNames()
        {
            string data = PersistentData.Get("HiddenHeaderNames");
            if (data == null)
                headerHiddenSaved = new Dictionary<string, bool>();
            else
                headerHiddenSaved = Parser.Deserialize<Dictionary<string, bool>>(data);
            data = PersistentData.Get("HeaderHiderState");
            if (data == null)
                state = HeaderHidingType.simple;
            else
                state = (HeaderHidingType)Enum.Parse(typeof(HeaderHidingType),data);
        }

        public static bool InitHidden(ShaderHeader header)
        {
            if (headerHiddenSaved == null)
                LoadHiddenHeaderNames();
            if (header.options.is_hideable == false)
                return false;
            bool is_hidden = false;
            if (headerHiddenSaved.ContainsKey(header.materialProperty.name))
                is_hidden =  headerHiddenSaved[header.materialProperty.name];
            else
                headerHiddenSaved[header.materialProperty.name] = is_hidden;
            header.is_hidden = is_hidden;
            return is_hidden;
        }

        public static void SetHidden(ShaderHeader header, bool set_hidden, bool save=true)
        {
            bool contains = headerHiddenSaved.ContainsKey(header.materialProperty.name);
            if (!contains || (contains && headerHiddenSaved[header.materialProperty.name] != set_hidden))
            {
                headerHiddenSaved[header.materialProperty.name] = set_hidden;
                header.is_hidden = set_hidden;
                if(save)
                    PersistentData.Set("HiddenHeaderNames", Parser.Serialize(headerHiddenSaved));
            }
            UpdateValues();
        }
        public static void SetHidden(List<ShaderPart> parts, bool set_hidden)
        {
            foreach (ShaderPart part in parts)
            {
                if (part.GetType() == typeof(ShaderHeader) && part.options.is_hideable)
                {
                    SetHidden((ShaderHeader)part, set_hidden, false);
                }
            }
            PersistentData.Set("HiddenHeaderNames", Parser.Serialize(headerHiddenSaved));
            UpdateValues();
        }

        private static void UpdateValues()
        {
            foreach (ShaderPart part in ShaderEditor.currentlyDrawing.shaderParts)
            {
                if (part.options.is_hideable == false)
                    continue;
                bool is_hidden = part.is_hidden;
            }
        }

        private static void SetType(HeaderHidingType newstate)
        {
            state = newstate;
            PersistentData.Set("HeaderHiderState", state.ToString());
        }

        public static bool IsHeaderHidden(ShaderPart header)
        {
            return header.options.is_hideable && ((header.is_hidden && state == HeaderHidingType.custom) || (state == HeaderHidingType.simple && !header.options.is_visible_simple));
        }

        public static void HeaderHiderGUI(EditorData editorData)
        {
            EditorGUILayout.BeginHorizontal(Styles.style_toolbar);
            if (GUILayout.Button("Simple", Styles.style_toolbar_toggle(state == HeaderHidingType.simple)))
                SetType(HeaderHidingType.simple);
            if (GUILayout.Button("Advanced", Styles.style_toolbar_toggle(state == HeaderHidingType.show_all)))
                SetType(HeaderHidingType.show_all);
            Rect right = GUILayoutUtility.GetRect(10, 20);
            Rect arrow = new Rect(right.x + right.width - 20, right.y, 20, 20);
            if (GUI.Button(arrow, Styles.dropdown_settings_icon, EditorStyles.largeLabel))
                DrawHeaderHiderMenu(arrow, editorData.shaderParts);
            if (GUI.Button(right, "Custom", Styles.style_toolbar_toggle(state == HeaderHidingType.custom)))
                SetType(HeaderHidingType.custom);

            GUI.Button(arrow, Styles.dropdown_settings_icon, EditorStyles.largeLabel);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }

        public static void DrawHeaderHiderMenu(Rect position, List<ShaderPart> shaderParts)
        {
            position.y -= 5;
            position.width = 150;
            position.x = Mathf.Min(Screen.width - position.width, position.x);
            position.height = 60;
            float maxY = GUIUtility.ScreenToGUIPoint(new Vector2(0, EditorWindow.focusedWindow.position.y + Screen.height)).y - 2.5f * position.height;
            position.y = Mathf.Min(position.y - position.height / 2, maxY);

            var menu = new GenericMenu();

            bool allHidden = true;
            bool allShown = true;
            foreach (ShaderPart part in shaderParts)
            {
                if (part.GetType() == typeof(ShaderHeader) && part.options.is_hideable)
                {
                    if (part.is_hidden)
                        allShown = false;
                    else
                        allHidden = false;
                }
            }
            menu.AddItem(new GUIContent("Everything"), allShown, delegate ()
            {
                SetHidden(shaderParts, false);
            });
            menu.AddItem(new GUIContent("Nothing"), allHidden, delegate ()
            {
                SetHidden(shaderParts, true);
            });
            foreach (ShaderPart part in shaderParts)
            {
                if (part.GetType() == typeof(ShaderHeader) && part.options.is_hideable)
                {
                    menu.AddItem(new GUIContent(part.content.text), !part.is_hidden, delegate ()
                    {
                        SetHidden((ShaderHeader)part, !part.is_hidden);
                    });
                }
            }
            menu.DropDown(position);
        }

    }
}