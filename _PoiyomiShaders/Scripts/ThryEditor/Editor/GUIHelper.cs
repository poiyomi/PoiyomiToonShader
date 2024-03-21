using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class GuiHelper
    {
        public const float SMALL_TEXTURE_VRAM_DISPLAY_WIDTH = 80;

        public static void ConfigTextureProperty(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor, bool hasFoldoutProperties, bool skip_drag_and_drop_handling = false)
        {
            switch (Config.Singleton.default_texture_type)
            {
                case TextureDisplayType.small:
                    SmallTextureProperty(position, prop, label, editor, hasFoldoutProperties);
                    break;
                case TextureDisplayType.big:
                    StylizedBigTextureProperty(position, prop, label, editor, hasFoldoutProperties, skip_drag_and_drop_handling);
                    break;
                case TextureDisplayType.big_basic:
                    BigTexturePropertyBasic(position, prop, label, editor, hasFoldoutProperties, skip_drag_and_drop_handling);
                    break;
            }
        }

        public static float GetSmallTextureVRAMWidth(MaterialProperty textureProperty)
        {
            if (textureProperty.textureValue != null) return SMALL_TEXTURE_VRAM_DISPLAY_WIDTH;
            return 0;
        }

        public static void SmallTextureProperty(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor, bool hasFoldoutProperties, Action extraFoldoutGUI = null)
        {
            // Border Code start
            bool isFoldedOut = hasFoldoutProperties && DrawingData.IsEnabled && DrawingData.CurrentTextureProperty.showFoldoutProperties;
            if(isFoldedOut)
            {
                Rect border = EditorGUILayout.BeginVertical();
                GUILayoutUtility.GetRect(0, 5);
                border = new RectOffset(EditorGUI.indentLevel * -15 - 26, 3, -3, -3).Add(border);
                GUI.DrawTexture(border, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Styles.COLOR_BACKGROUND_1, 3, 10);
            }
            // Border Code end
                

            Rect thumbnailPos = position;
            Rect foloutClickCheck = position;
            Rect tooltipRect = position;
            if (hasFoldoutProperties)
            {
                thumbnailPos.x += 20;
                thumbnailPos.width -= 20;
            }
            editor.TexturePropertyMiniThumbnail(thumbnailPos, prop, label.text, label.tooltip);
            float iconsPositioningHeight = thumbnailPos.y;
            //VRAM
            Rect vramPos = Rect.zero;
            if (DrawingData.CurrentTextureProperty.MaterialProperty.textureValue != null)
            {
                GUIContent content = new GUIContent(DrawingData.CurrentTextureProperty.VRAMString);
                vramPos = thumbnailPos;
                vramPos.x += thumbnailPos.width - SMALL_TEXTURE_VRAM_DISPLAY_WIDTH;
                vramPos.width = SMALL_TEXTURE_VRAM_DISPLAY_WIDTH;
                GUI.Label(vramPos, content, Styles.label_align_right);
            }
            //Prop right next to texture
            if (DrawingData.CurrentTextureProperty.DoesReferencePropertyExist)
            {
                ShaderProperty property = ShaderEditor.Active.PropertyDictionary[DrawingData.CurrentTextureProperty.Options.reference_property];
                Rect r = position;
                r.x += EditorGUIUtility.labelWidth - CurrentIndentWidth();
                r.width -= EditorGUIUtility.labelWidth - CurrentIndentWidth();
                r.width -= vramPos.width;
                foloutClickCheck.width -= r.width;
                property.Draw(new CRect(r), new GUIContent());
                property.tooltip.ConditionalDraw(r);
            }
            //Foldouts
            if (hasFoldoutProperties && DrawingData.CurrentTextureProperty != null)
            {
                //draw dropdown triangle
                Rect trianglePos = thumbnailPos;
                trianglePos.x += DrawingData.CurrentTextureProperty.XOffset * 15 - 2;
                //This is an invisible button with zero functionality. But it needs to be here so that the triangle click reacts fast
                if (GUI.Button(trianglePos, "", GUIStyle.none)) { }
                if (Event.current.type == EventType.Repaint)
                    EditorStyles.foldout.Draw(trianglePos, false, false, DrawingData.CurrentTextureProperty.showFoldoutProperties, false);

                if (DrawingData.IsEnabled)
                {
                    //sub properties
                    if (DrawingData.CurrentTextureProperty.showFoldoutProperties)
                    {
                        EditorGUI.indentLevel += 2;
                        extraFoldoutGUI?.Invoke();
                        if (DrawingData.CurrentTextureProperty.hasScaleOffset)
                        {
                            EditorGUI.showMixedValue = ShaderEditor.Active.Materials.Select(m => m.GetTextureScale(prop.name)).Distinct().Count() > 1 || ShaderEditor.Active.Materials.Select(m => m.GetTextureOffset(prop.name)).Distinct().Count() > 1;
                            ShaderEditor.Active.Editor.TextureScaleOffsetProperty(prop);
                            Rect lastRect = GUILayoutUtility.GetLastRect();
                            tooltipRect.height = (lastRect.y - tooltipRect.y) + lastRect.height;
                            iconsPositioningHeight = lastRect.y;
                        }
                        //In case of locked material end disabled group here to allow editing of sub properties
                        if (ShaderEditor.Active.IsLockedMaterial) EditorGUI.EndDisabledGroup();

                        PropertyOptions options = DrawingData.CurrentTextureProperty.Options;
                        if (options.reference_properties != null)
                            foreach (string r_property in options.reference_properties)
                            {
                                ShaderProperty property = ShaderEditor.Active.PropertyDictionary[r_property];
                                property.Draw(useEditorIndent: true);
                            }

                        //readd disabled group
                        if (ShaderEditor.Active.IsLockedMaterial) EditorGUI.BeginDisabledGroup(false);

                        EditorGUI.indentLevel -= 2;
                    }
                    if (ShaderEditor.Input.LeftClick_IgnoreLockedAndUnityUses && foloutClickCheck.Contains(Event.current.mousePosition))
                    {
                        ShaderEditor.Input.Use();
                        DrawingData.CurrentTextureProperty.showFoldoutProperties = !DrawingData.CurrentTextureProperty.showFoldoutProperties;
                    }
                }
            }

            Rect object_rect = new Rect(position);
            object_rect.height = GUILayoutUtility.GetLastRect().y - object_rect.y + GUILayoutUtility.GetLastRect().height;
            DrawingData.LastGuiObjectRect = object_rect;
            DrawingData.TooltipCheckRect = tooltipRect;            
            DrawingData.IconsPositioningHeight = iconsPositioningHeight;

            // Border Code start
            if(isFoldedOut)
            {
                GUILayoutUtility.GetRect(0, 5);
                EditorGUILayout.EndVertical();
            }
            // Border Code end
        }


        static int s_texturePickerWindow = -1;
        static MaterialProperty s_texturePickerWindowProperty = null;
        public static void StylizedBigTextureProperty(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor, bool hasFoldoutProperties, bool skip_drag_and_drop_handling = false)
        {
            // add some padding at the top
            position.y += 5;

            Rect border = new Rect(position);
            border.x += (EditorGUI.indentLevel) * 15;
            border.width -= (EditorGUI.indentLevel) * 15;
            border.height = 80; // for texture & offset

            Rect[] additionRects = new Rect[(DrawingData.CurrentTextureProperty.DoesReferencePropertyExist ? 1 : 0) +
                (DrawingData.CurrentTextureProperty.DoReferencePropertiesExist ? DrawingData.CurrentTextureProperty.Options.reference_properties.Length : 0)];
            int i = 0;

            if (DrawingData.CurrentTextureProperty.DoReferencePropertiesExist)
            {
                foreach (string r_property in DrawingData.CurrentTextureProperty.Options.reference_properties)
                {
                    float height = editor.GetPropertyHeight(ShaderEditor.Active.PropertyDictionary[r_property].MaterialProperty);
                    additionRects[i++] = new Rect(position.x + 30, border.y + border.height - 8, border.width , height);
                    border.height += height + 3; // add a little padding
                }
            }
            if (DrawingData.CurrentTextureProperty.DoesReferencePropertyExist)
            {
                float height = editor.GetPropertyHeight(ShaderEditor.Active.PropertyDictionary[DrawingData.CurrentTextureProperty.Options.reference_property].MaterialProperty);
                additionRects[i++] = new Rect(position.x + 30, border.y + border.height, border.width , height);
                border.height += height + 3; // add a little padding
            }
            Rect vramRect = new Rect(border.x + 30, border.y + border.height - 6, border.width , EditorStyles.label.lineHeight);
            border.height += EditorStyles.label.lineHeight;

            // Reserve space
            GUILayoutUtility.GetRect(0, border.height - position.height - 5);

            GUI.DrawTexture(border, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, Styles.COLOR_BACKGROUND_1, 3, 10);

            Rect previewSide = new Rect(border);
            Rect optionsSide = new Rect(border);
            previewSide.width = Mathf.Max(50, Mathf.Min(previewSide.height, previewSide.width - EditorGUIUtility.labelWidth - 50));
            previewSide.x += optionsSide.width - previewSide.width;
            optionsSide.width -= previewSide.width;

            Rect previewRectBorder = new Rect(previewSide);
            previewRectBorder.height = previewRectBorder.width;
            Rect previewRect = new RectOffset(3, 3, 3, 3).Remove(previewRectBorder);

            Rect buttonSelectRect = new RectOffset(20, 20, 0, 0).Remove(previewRectBorder);
            buttonSelectRect.height = 20;
            buttonSelectRect.y = previewRect.y + previewRect.height - buttonSelectRect.height + 2;

            if (prop.hasMixedValue)
            {
                Rect mixedRect = new Rect(previewRect);
                mixedRect.y -= 5;
                mixedRect.x += mixedRect.width / 2 - 4;
                GUI.Label(mixedRect, "_");
            }
            else if (prop.textureValue != null)
            {
                if(prop.textureValue is Cubemap)
                {
                    editor.TextureProperty(previewRect, prop, "", false);
                }
                else
                {
                    GUI.DrawTexture(previewRect, prop.textureValue);
                }
            }
            GUI.DrawTexture(previewRectBorder, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, Styles.COLOR_BACKGROUND_1, 3, 10);

            //selection button and pinging
            if (GUI.Button(buttonSelectRect, "Select", EditorStyles.miniButton))
            {
                OpenTexturePicker(prop);
            }
            else if (Event.current.type == EventType.MouseDown && previewRect.Contains(Event.current.mousePosition))
            {
                EditorGUIUtility.PingObject(prop.textureValue);
            }
            HandleTexturePicker(prop);

            if (!skip_drag_and_drop_handling)
                AcceptDragAndDrop(previewRect, prop);

            //Change indent & label width
            EditorGUI.indentLevel += 2;
            float oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 80;
            

            //scale offset rect + foldout properties
            Rect scale_offset_rect = new Rect();
            scale_offset_rect = new RectOffset(30, 5, 37, 0).Remove(optionsSide);
            scale_offset_rect.height = position.height;
            if (hasFoldoutProperties || DrawingData.CurrentTextureProperty.Options.reference_property != null)
            {
                if (DrawingData.CurrentTextureProperty.hasScaleOffset)
                {
                    EditorGUI.showMixedValue = ShaderEditor.Active.Materials.Select(m => m.GetTextureScale(prop.name)).Distinct().Count() > 1 || ShaderEditor.Active.Materials.Select(m => m.GetTextureOffset(prop.name)).Distinct().Count() > 1;
                    editor.TextureScaleOffsetProperty(scale_offset_rect, prop);
                }

                //In case of locked material end disabled group here to allow editing of sub properties
                if (ShaderEditor.Active.IsLockedMaterial) EditorGUI.EndDisabledGroup();

                PropertyOptions options = DrawingData.CurrentTextureProperty.Options;
                float labelWith = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = EditorGUI.indentLevel * 15 + 35;
                if (options.reference_property != null)
                {
                    ShaderProperty property = ShaderEditor.Active.PropertyDictionary[options.reference_property];
                    Rect r = additionRects[additionRects.Length - 1];
                    r.width = optionsSide.width;
                    property.Draw(new CRect(r));
                }
                if (options.reference_properties != null)
                {
                    i = 0;
                    foreach (string r_property in options.reference_properties)
                    {
                        ShaderProperty property = ShaderEditor.Active.PropertyDictionary[r_property];
                        Rect r = additionRects[i++];
                        r.width = optionsSide.width;
                        property.Draw(new CRect(r));
                    }
                }
                EditorGUIUtility.labelWidth = labelWith;

                //readd disabled group
                if (ShaderEditor.Active.IsLockedMaterial) EditorGUI.BeginDisabledGroup(false);
            }

            //VRAM
            if (DrawingData.CurrentTextureProperty.MaterialProperty.textureValue != null)
            {
                GUI.Label(vramRect, "VRAM:");
                vramRect.x += EditorGUIUtility.labelWidth - 15;
                GUI.Label(vramRect, DrawingData.CurrentTextureProperty.VRAMString);
            }

            //reset indent + label width
            EditorGUI.indentLevel -= 2;
            EditorGUIUtility.labelWidth = oldLabelWidth;

            Rect label_rect = new RectOffset(-5, 0, -2, 0).Add(border);
            label_rect.height = EditorGUIUtility.singleLineHeight;
            GUI.Label(label_rect, label);

            DrawingData.LastGuiObjectRect = border;
            DrawingData.TooltipCheckRect = Rect.MinMaxRect(border.x, border.y, scale_offset_rect.xMax, scale_offset_rect.yMax);
            DrawingData.IconsPositioningHeight = scale_offset_rect.y;
        }

        public static void BigTexturePropertyBasic(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor, bool hasFoldoutProperties, bool skip_drag_and_drop_handling = false)
        {
            string text = label.text;
            //VRAM
            if (DrawingData.CurrentTextureProperty.MaterialProperty.textureValue != null)
            {
                text += "   (VRAM: " + DrawingData.CurrentTextureProperty.VRAMString + ")";
            }
            GUILayoutUtility.GetRect(0, EditorGUIUtility.singleLineHeight * 3 - 5);
            editor.TextureProperty(position, prop, text);

            Rect tooltipCheckRect =  position;
            tooltipCheckRect.height += EditorGUIUtility.singleLineHeight * 3 - 5;

            float iconsPositioningHeight = position.y;
            if(DrawingData.CurrentTextureProperty.hasScaleOffset)
                iconsPositioningHeight += position.height + EditorGUIUtility.singleLineHeight - 5;
            
            
            // Reference properties
            EditorGUI.indentLevel += 1;
            PropertyOptions options = DrawingData.CurrentTextureProperty.Options;
            if (options.reference_property != null)
            {
                ShaderProperty property = ShaderEditor.Active.PropertyDictionary[options.reference_property];
                property.Draw(useEditorIndent: true);
            }
            if (options.reference_properties != null)
                foreach (string r_property in options.reference_properties)
                {
                    ShaderProperty property = ShaderEditor.Active.PropertyDictionary[r_property];
                    property.Draw(useEditorIndent: true);
                }
            EditorGUI.indentLevel -= 1;

            DrawingData.LastGuiObjectRect = position;
            DrawingData.TooltipCheckRect = tooltipCheckRect;
            DrawingData.IconsPositioningHeight = iconsPositioningHeight;
        }

        public static void OpenTexturePicker(MaterialProperty prop)
        {
            EditorGUIUtility.ShowObjectPicker<Texture>(prop.textureValue, false, "", 0);
            s_texturePickerWindow = EditorGUIUtility.GetObjectPickerControlID();
            s_texturePickerWindowProperty = prop;
        }

        public static bool HandleTexturePicker(MaterialProperty prop)
        {
            if (Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID() == s_texturePickerWindow && s_texturePickerWindowProperty.name == prop.name)
            {
                prop.textureValue = (Texture)EditorGUIUtility.GetObjectPickerObject();
                ShaderEditor.RepaintActive();
                return true;
            }
            if (Event.current.commandName == "ObjectSelectorClosed" && EditorGUIUtility.GetObjectPickerControlID() == s_texturePickerWindow)
            {
                s_texturePickerWindow = -1;
                s_texturePickerWindowProperty = null;
            }
            return false;
        }

        public static bool AcceptDragAndDrop(Rect r, MaterialProperty prop)
        {
            if ((ShaderEditor.Input.is_drag_drop_event) && r.Contains(ShaderEditor.Input.mouse_position) && DragAndDrop.objectReferences[0] is Texture)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (ShaderEditor.Input.is_drop_event)
                {
                    DragAndDrop.AcceptDrag();
                    prop.textureValue = (Texture)DragAndDrop.objectReferences[0];
                    return true;
                }
            }
            return false;
        }

        static Stack<int> s_previousIndentLevels = new Stack<int>();
        public static void BeginCustomIndentLevel(int indent)
        {
            s_previousIndentLevels.Push(EditorGUI.indentLevel);
            EditorGUI.indentLevel = indent;
        }

        public static void EndCustomIndentLevel()
        {
            EditorGUI.indentLevel = s_previousIndentLevels.Pop();
        }

        public static void MinMaxSlider(Rect settingsRect, GUIContent content, MaterialProperty prop)
        {
            bool changed = false;
            Vector4 vec = prop.vectorValue;
            Rect sliderRect = settingsRect;

            EditorGUI.LabelField(settingsRect, content);

            if (settingsRect.width > 160)
            {
                Rect numberRect = settingsRect;
                numberRect.width = 65 + (EditorGUI.indentLevel - 1) * 15;

                numberRect.x = EditorGUIUtility.labelWidth - (EditorGUI.indentLevel - 1) * 15;

                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = prop.hasMixedValue;
                vec.x = EditorGUI.FloatField(numberRect, vec.x, EditorStyles.textField);
                changed |= EditorGUI.EndChangeCheck();

                numberRect.x = settingsRect.xMax - numberRect.width;

                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = prop.hasMixedValue;
                vec.y = EditorGUI.FloatField(numberRect, vec.y);
                changed |= EditorGUI.EndChangeCheck();

                sliderRect.xMin = EditorGUIUtility.labelWidth - (EditorGUI.indentLevel - 1) * 15;
                sliderRect.xMin += (65 + -8);
                sliderRect.xMax -= (65 + -8);
            }

            vec.x = Mathf.Clamp(vec.x, vec.z, vec.y);
            vec.y = Mathf.Clamp(vec.y, vec.x, vec.w);

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

        public static void DrawLocaleSelection(GUIContent label, string[] locales, int selected)
        {
            EditorGUI.BeginChangeCheck();
            selected = EditorGUILayout.Popup(label.text, selected, locales);
            if (EditorGUI.EndChangeCheck())
            {
                ShaderEditor.Active.PropertyDictionary[ShaderEditor.PROPERTY_NAME_LOCALE].MaterialProperty.SetNumber(selected);
                ShaderEditor.ReloadActive();
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

        public static float CurrentIndentWidth()
        {
            return EditorGUI.indentLevel * 15;
        }
        // Mimics the normal map import warning - written by Orels1
        static bool TextureImportWarningBox(string message) {
            GUILayout.BeginVertical(new GUIStyle(EditorStyles.helpBox));
            GUILayout.Label(message, new GUIStyle(EditorStyles.label) {
                fontSize = 10, wordWrap = true
            });
            EditorGUILayout.BeginHorizontal(new GUIStyle() {
                alignment = TextAnchor.MiddleRight
            }, GUILayout.Height(24));
            EditorGUILayout.Space();
            bool buttonPress = GUILayout.Button("Fix Now", new GUIStyle("button") {
                stretchWidth = false,
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(9, 9, 0, 0)
            }, GUILayout.Height(22));
            EditorGUILayout.EndHorizontal();
            GUILayout.EndVertical();
            return buttonPress;
        }

        public static void ColorspaceWarning(MaterialProperty tex, bool shouldHaveSRGB) {
            if (tex.textureValue) {
                string texPath = AssetDatabase.GetAssetPath(tex.textureValue);
                TextureImporter texImporter;
                var importer = TextureImporter.GetAtPath(texPath) as TextureImporter;
                if (importer != null) {
                    texImporter = (TextureImporter)importer;
                    if (texImporter.sRGBTexture != shouldHaveSRGB) {
                        if (TextureImportWarningBox(shouldHaveSRGB ? EditorLocale.editor.Get("colorSpaceWarningSRGB") : EditorLocale.editor.Get("colorSpaceWarningLinear"))) {
                            texImporter.sRGBTexture = shouldHaveSRGB;
                            texImporter.SaveAndReimport();
                        }
                    }
                }
            }
        }

        public class CustomGUIColor : IDisposable
        {
            Color _prev;
            public CustomGUIColor(Color color)
            {
                _prev = GUI.color;
                GUI.color = color;
            }

            public void Dispose()
            {
                GUI.color = _prev;
            }
        }

        public static bool Button(Rect r, GUIStyle style)
        {
            return GUI.Button(r, GUIContent.none, style);
        }

        public static bool Button(Rect r, string tooltip, GUIStyle style)
        {
            return GUI.Button(r, new GUIContent("", tooltip), style);
        }

        public static bool Button(GUIStyle style, int width, int height)
        {
            Rect r = GUILayoutUtility.GetRect(width, height);
            return Button(r, style);
        }
        
        public static bool ButtonWithCursor(GUIStyle style, int width, int height)
        {
            Rect r = GUILayoutUtility.GetRect(width, height);
            EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
            return Button(r, style);
        }
        
        public static bool ButtonWithCursor(GUIStyle style, string tooltip, int width, int height)
        {
            Rect r = GUILayoutUtility.GetRect(width, height);
            EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
            return Button(r, tooltip, style);
        }

        public static bool ButtonWithCursor(GUIStyle style, string tooltip, int width, int height, out Rect r)
        {
            r = GUILayoutUtility.GetRect(width, height);
            EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);
            return Button(r, tooltip, style);
        }

        public static bool Button(Rect r, string tooltip, GUIStyle style, Color c)
        {
            Color prevColor = GUI.backgroundColor;
            GUI.backgroundColor = c;
            bool b = GuiHelper.Button(r, tooltip, style);
            GUI.backgroundColor = prevColor;
            return b;
        }

        public static bool Button(GUIStyle style, int width, int height, Color c)
        {
            Color prevColor = GUI.backgroundColor;
            GUI.backgroundColor = c;
            bool b = GuiHelper.Button(style, width, height);
            GUI.backgroundColor = prevColor;
            return b;
        }

        public static bool Button(Rect r, GUIStyle style, Color c, bool doColor)
        {
            Color prevColor = GUI.backgroundColor;
            if(doColor) GUI.backgroundColor = c;
            bool b = GuiHelper.Button(r, style);
            GUI.backgroundColor = prevColor;
            return b;
        }

        #region SearchableEnumPopup
        public class SearchableEnumPopup : EditorWindow
        {
            private static SearchableEnumPopup window;
            public static void CreateSearchableEnumPopup(string[] options, string selected, Action<string> setter)
            {
                Vector2 pos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                pos.x = Mathf.Min(EditorWindow.focusedWindow.position.x + EditorWindow.focusedWindow.position.width - 250, pos.x);
                pos.y = Mathf.Min(EditorWindow.focusedWindow.position.y + EditorWindow.focusedWindow.position.height - 200, pos.y);

                if (window != null)
                    window.Close();
                window = ScriptableObject.CreateInstance<SearchableEnumPopup>();
                window.position = new Rect(pos.x, pos.y, 250, 200);
                window._options = options;
                window._selected = selected;
                window._setter = setter;
                window._searchedFor = "";
                window.ShowPopup();
            }

            private SearchableEnumPopup() { }

            string[] _options;
            string _selected;
            string _searchedFor;
            Action<string> _setter;

            bool first = true;

            private void OnGUI()
            {
                if (GUILayout.Button("Close")) this.Close();
                GUI.SetNextControlName("SearchTextField");
                _searchedFor = GUILayout.TextField(_searchedFor);
                string seachTerm = _searchedFor.ToLowerInvariant().TrimStart('_');
                string[] filteredOptions = _options.Where(o => o.TrimStart('_').ToLowerInvariant().StartsWith(seachTerm)).ToArray();
                for (int i = 0; i < 7 && i < filteredOptions.Length; i++)
                {
                    if (GUILayout.Button(filteredOptions[i]))
                    {
                        _selected = filteredOptions[i];
                        _setter.Invoke(_selected);
                        this.Close();
                    }
                }
                if (filteredOptions.Length > 7)
                {
                    GUILayout.Label("... More");
                }
                if (first)
                {
                    GUI.FocusControl("SearchTextField");
                    first = false;
                }
            }
        }

        #endregion
    }

    public class BetterTooltips
    {
        private static Tooltip activeTooltip;

        public class Tooltip
        {
            private GUIContent content;
            private bool empty;

            public bool isSelected { get; private set; } = false;

            private Rect containerRect;
            private Rect contentRect;

            readonly static Vector2 PADDING = new Vector2(10, 10);

            public Tooltip(string text)
            {
                content = new GUIContent(text);
                empty = string.IsNullOrWhiteSpace(text);
            }

            public Tooltip(string text, Texture texture)
            {
                content = new GUIContent(text, texture);
                empty = string.IsNullOrWhiteSpace(text) && texture == null;
            }

            public void SetText(string text)
            {
                content.text = text;
                empty &= string.IsNullOrWhiteSpace(text);
            }

            public void ConditionalDraw(Rect hoverOverRect)
            {
                if (empty) return;
                bool isSelected = hoverOverRect.Contains(Event.current.mousePosition);
                if (isSelected )
                {
                    CalculatePositions(hoverOverRect);
                    activeTooltip = this;
                    this.isSelected = true;
                }
            }

            private void CalculatePositions(Rect hoverOverRect)
            {
                Vector2 contentSize = EditorStyles.label.CalcSize(content);
                Vector2 containerPosition = new Vector2(Event.current.mousePosition.x - contentSize.x / 2 - PADDING.x / 2, hoverOverRect.y - contentSize.y - PADDING.y - 3);

                containerPosition.x = Mathf.Max(0, containerPosition.x);
                containerPosition.x = Mathf.Min(EditorGUIUtility.currentViewWidth - contentSize.x - PADDING.x, containerPosition.x);

                contentRect = new Rect(containerPosition + new Vector2(PADDING.x/2, PADDING.y/2), contentSize);
                containerRect = new Rect(containerPosition, contentSize + new Vector2(PADDING.x, PADDING.y));
            }

            public void Draw()
            {
                EditorGUI.DrawRect(containerRect, Styles.COLOR_BG);
                EditorGUI.LabelField(contentRect, content);
                isSelected = false;
            }
        }

        public static void DrawActive()
        {
            if(activeTooltip != null)
            {
                if (activeTooltip.isSelected)
                {
                    activeTooltip.Draw();
                }
                else
                {
                    activeTooltip = null;
                }
            }
        }
    }

    public class FooterButton
    {
        private GUIContent content;
        private bool isTextureContent;
        const int texture_height = 40;
        int texture_width;
        private ButtonData data;

        public FooterButton(ButtonData data)
        {
            this.data = data;
            if (data != null)
            {
                if (data.texture == null)
                {
                    content = new GUIContent(data.text, data.hover);
                    isTextureContent = false;
                }
                else
                {
                    texture_width = (int)((float)data.texture.loaded_texture.width / data.texture.loaded_texture.height * texture_height);
                    content = new GUIContent(data.texture.loaded_texture, data.hover);
                    isTextureContent = true;
                }
            }
            else
            {
                content = new GUIContent();
            }
        }

        public void Draw()
        {
            Rect cursorRect;
            if (isTextureContent)
            {
                if(GUILayout.Button(content, new GUIStyle(), GUILayout.MaxWidth(texture_width), GUILayout.Height(texture_height))){
                    data.action.Perform(ShaderEditor.Active?.Materials);
                }
                cursorRect = GUILayoutUtility.GetLastRect();
                GUILayout.Space(8);
            }
            else
            {
                if (GUILayout.Button(content, GUILayout.ExpandWidth(false), GUILayout.Height(texture_height)))
                    data.action.Perform(ShaderEditor.Active?.Materials);
                cursorRect = GUILayoutUtility.GetLastRect();
                GUILayout.Space(2);
            }
            EditorGUIUtility.AddCursorRect(cursorRect, MouseCursor.Link);
        }

        public static void DrawList(List<FooterButton> list)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Space(2);
            foreach (FooterButton b in list)
            {
                b.Draw();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
    }
}