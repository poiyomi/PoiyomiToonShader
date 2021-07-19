using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class GuiHelper
    {

        public static void drawConfigTextureProperty(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor, bool hasFoldoutProperties, bool skip_drag_and_drop_handling = false)
        {
            switch (Config.Singleton.default_texture_type)
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
            editor.TexturePropertyMiniThumbnail(thumbnailPos, prop, label.text, label.tooltip);
            DrawingData.currentTexProperty.tooltip.ConditionalDraw(thumbnailPos);
            if (DrawingData.currentTexProperty.reference_property_exists)
            {
                ShaderProperty property = ShaderEditor.active.propertyDictionary[DrawingData.currentTexProperty.options.reference_property];
                Rect r = position;
                r.x += EditorGUIUtility.labelWidth - CurrentIndentWidth();
                r.width -= EditorGUIUtility.labelWidth - CurrentIndentWidth();
                property.Draw(new CRect(r), new GUIContent());
                property.tooltip.ConditionalDraw(r);
            }
            if (hasFoldoutProperties && DrawingData.currentTexProperty != null)
            {
                //draw dropdown triangle
                thumbnailPos.x += DrawingData.currentTexProperty.xOffset * 15;
                //This is an invisible button with zero functionality. But it needs to be here so that the triangle click reacts fast
                if (GUI.Button(thumbnailPos, "", Styles.none)) { }
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
                            ShaderEditor.active.editor.TextureScaleOffsetProperty(prop);
                            if(DrawingData.currentTexProperty.is_animatable)
                                DrawingData.currentTexProperty.HandleKajAnimatable();
                        }

                        PropertyOptions options = DrawingData.currentTexProperty.options;
                        if (options.reference_properties != null)
                            foreach (string r_property in options.reference_properties)
                            {
                                ShaderProperty property = ShaderEditor.active.propertyDictionary[r_property];
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
            Rect object_rect = new Rect(position);
            object_rect.height += rect.height;
            DrawingData.lastGuiObjectRect = object_rect;
            DrawingData.currentTexProperty.tooltip.ConditionalDraw(object_rect);
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
                    border.height += editor.GetPropertyHeight(ShaderEditor.active.propertyDictionary[r_property].materialProperty);
                }
            }
            if (DrawingData.currentTexProperty.reference_property_exists)
            {
                border.height += 8;
                border.height += editor.GetPropertyHeight(ShaderEditor.active.propertyDictionary[DrawingData.currentTexProperty.options.reference_property].materialProperty);
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

            DrawingData.currentTexProperty.tooltip.ConditionalDraw(border);

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
                ShaderEditor.Repaint();
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

            if (hasFoldoutProperties || DrawingData.currentTexProperty.options.reference_property != null)
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
                    ShaderProperty property = ShaderEditor.active.propertyDictionary[options.reference_property];
                    property.Draw(useEditorIndent: true);
                }
                if (options.reference_properties != null)
                    foreach (string r_property in options.reference_properties)
                    {
                        ShaderProperty property = ShaderEditor.active.propertyDictionary[r_property];
                        property.Draw(useEditorIndent: true);
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
            
            DrawingData.lastGuiObjectRect = border;
        }

        const float kNumberWidth = 65;

        public static void MinMaxSlider(Rect settingsRect, GUIContent content, MaterialProperty prop)
        {
            bool changed = false;
            Vector4 vec = prop.vectorValue;
            Rect sliderRect = settingsRect;

            EditorGUI.LabelField(settingsRect, content);

            if (settingsRect.width > 160)
            {
                Rect numberRect = settingsRect;
                numberRect.width = kNumberWidth + (EditorGUI.indentLevel - 1) * 15;

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
                sliderRect.xMin += (kNumberWidth + -8);
                sliderRect.xMax -= (kNumberWidth + -8);
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
                ShaderEditor.active.propertyDictionary[ShaderEditor.PROPERTY_NAME_LOCALE].materialProperty.floatValue = selected;
                ShaderEditor.reload();
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
        // Mimics the normal map import warning - written by Orels1
        static bool TextureImportWarningBox(string message){
            GUILayout.BeginVertical(new GUIStyle(EditorStyles.helpBox));
            EditorGUILayout.LabelField(message, new GUIStyle(EditorStyles.label) {
                fontSize = 9, wordWrap = true
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
        public static void sRGBWarning(MaterialProperty tex){
            if (tex.textureValue){
                string sRGBWarning = "This texture is marked as sRGB, but should not contain color information.";
                string texPath = AssetDatabase.GetAssetPath(tex.textureValue);
                TextureImporter texImporter;
                var importer = TextureImporter.GetAtPath(texPath) as TextureImporter;
                if (importer != null){
                    texImporter = (TextureImporter)importer;
                    if (texImporter.sRGBTexture){
                        if (TextureImportWarningBox(sRGBWarning)){
                            texImporter.sRGBTexture = false;
                            texImporter.SaveAndReimport();
                        }
                    }
                }
            }
        }
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
                EditorGUI.DrawRect(containerRect, Styles.backgroundColor);
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
                    data.action.Perform();
                }
                cursorRect = GUILayoutUtility.GetLastRect();
                GUILayout.Space(8);
            }
            else
            {
                if (GUILayout.Button(content, GUILayout.ExpandWidth(false), GUILayout.Height(texture_height)))
                    data.action.Perform();
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

    public class ThryHeaderDrawer : MaterialPropertyDrawer
    {
        private MaterialProperty property;

        private bool expanded;

        private string keyword;
        private string end;

        public bool isHideable;

        public int xOffset = 0;

        private ButtonData button;

        public ThryHeaderDrawer(string end, string keyword, string buttonText, string buttonHover, string buttonAction, float isHideable)
        {
            this.end = end;
            this.keyword = keyword;

            button = new ButtonData();
            button.text = buttonText;
            button.hover = buttonHover;
            button.action = DefineableAction.ParseDrawerParameter(buttonAction);

            this.isHideable = isHideable == 1;
        }
        
        public ThryHeaderDrawer(string end, string keyword, string buttonText, string buttonHover, string buttonAction) : this(end, keyword, buttonText, buttonHover, buttonAction, 0          ) { }
        public ThryHeaderDrawer(string end, string keyword, float isHideable) :                                           this(end, keyword, null,       null,        null,         isHideable ) { }
        public ThryHeaderDrawer(string end, string keyword) :                                                             this(end, keyword, null,       null,        null,         0          ) { }

        public ThryHeaderDrawer(float isHideable) :                                                                       this(null,null,    null,       null,        null,         isHideable ) { }
        public ThryHeaderDrawer(float isHideable, string end) :                                                           this(end, null,    null,       null,        null,         isHideable ) { }
        public ThryHeaderDrawer(float isHideable, string buttonText, string buttonHover, string buttonAction) :           this(null,null,    buttonText, buttonHover, buttonAction, 0          ) { }
        public ThryHeaderDrawer(float isHideable, string end, string buttonText, string buttonHover, string buttonAction):this(end, null,    buttonText, buttonHover, buttonAction, isHideable ) { }

        public ThryHeaderDrawer(){}

        public string GetEndProperty()
        {
            return end;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.lastPropertyUsedCustomDrawer = true;
            DrawingData.lastPropertyDrawerType = DrawerType.Header;
            DrawingData.lastPropertyDrawer = this;
            return base.GetPropertyHeight(prop, label, editor);
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
            foreach (Material m in ShaderEditor.active.materials) m.SetFloat(property.name, expanded ? 1 : 0);
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (this.property == null)
            {
                this.property = prop;
                this.expanded = prop.floatValue == 1;
            }

            PropertyOptions options = ShaderEditor.active.currentProperty.options;
            Event e = Event.current;

            int offset = 15 * xOffset + 15;
            position.width -= offset - position.x;
            position.x = offset;

            DrawingData.lastGuiObjectHeaderRect = position;

            DrawBoxAndContent(position, e, label, options);

            DrawSmallArrow(position, e);
            HandleToggleInput(e, position);
        }

        private void DrawBoxAndContent(Rect rect, Event e, GUIContent content, PropertyOptions options)
        {
            if (options.reference_property != null && ShaderEditor.active.propertyDictionary.ContainsKey(options.reference_property))
            {
                GUI.Box(rect, new GUIContent("     " + content.text, content.tooltip), Styles.dropDownHeader);
                DrawIcons(rect, options, e);

                Rect togglePropertyRect = new Rect(rect);
                togglePropertyRect.x += 5;
                togglePropertyRect.y += 1;
                togglePropertyRect.height -= 4;
                togglePropertyRect.width = GUI.skin.font.fontSize * 3;
                float fieldWidth = EditorGUIUtility.fieldWidth;
                EditorGUIUtility.fieldWidth = 20;
                ShaderProperty prop = ShaderEditor.active.propertyDictionary[options.reference_property];

                int xOffset = prop.xOffset;
                prop.xOffset = 0;
                prop.Draw(new CRect(togglePropertyRect), new GUIContent(), isInHeader: true);
                prop.xOffset = xOffset;
                EditorGUIUtility.fieldWidth = fieldWidth;
            }else if(keyword != null)
            {
                GUI.Box(rect, "     " + content.text, Styles.dropDownHeader);
                DrawIcons(rect, options, e);

                Rect togglePropertyRect = new Rect(rect);
                togglePropertyRect.x += 20;
                togglePropertyRect.width = 20;

                EditorGUI.BeginChangeCheck();
                bool keywordOn = EditorGUI.Toggle(togglePropertyRect, "", ShaderEditor.active.materials[0].IsKeywordEnabled(keyword));
                if (EditorGUI.EndChangeCheck())
                {
                    MaterialHelper.ToggleKeyword(ShaderEditor.active.materials, keyword, keywordOn);
                }
            }
            else
            {
                GUI.Box(rect, content, Styles.dropDownHeader);
                DrawIcons(rect, options, e);
            }

        }

        /// <summary>
        /// Draws the icons for ShaderEditor features like linking and copying
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="e"></param>
        private void DrawIcons(Rect rect, PropertyOptions options, Event e)
        {
            DrawHelpButton(rect, options, e);
            DrawDowdownSettings(rect, e);
            DrawLinkSettings(rect, e);
        }

        private void DrawHelpButton(Rect rect, PropertyOptions options, Event e)
        {
            ButtonData button = this.button != null ? this.button : options.button_help;
            if (button != null && button.condition_show.Test())
            {
                Rect buttonRect = new Rect(rect);
                buttonRect.width = 20;
                buttonRect.x += rect.width - 65;
                buttonRect.y += 1;
                buttonRect.height -= 4;
                if (GUI.Button(buttonRect, Styles.icon_help, EditorStyles.largeLabel))
                {
                    e.Use();
                    if (button.action != null) button.action.Perform();
                }
            }
        }

        private void DrawDowdownSettings(Rect rect, Event e)
        {
            Rect buttonRect = new Rect(rect);
            buttonRect.width = 20;
            buttonRect.x += rect.width - 25;
            buttonRect.y += 1;
            buttonRect.height -= 4;
            if (GUI.Button(buttonRect, Styles.icon_menu, EditorStyles.largeLabel))
            {
                e.Use();

                buttonRect.width = 150;
                buttonRect.x = Mathf.Min(Screen.width - buttonRect.width, buttonRect.x);
                buttonRect.height = 60;
                float maxY = GUIUtility.ScreenToGUIPoint(new Vector2(0, EditorWindow.focusedWindow.position.y + Screen.height)).y - 2.5f * buttonRect.height;
                buttonRect.y = Mathf.Min(buttonRect.y - buttonRect.height / 2, maxY);

                ShowHeaderContextMenu(buttonRect, ShaderEditor.active.currentProperty, ShaderEditor.active.materials[0]);
            }
        }

        private void DrawLinkSettings(Rect rect, Event e)
        {
            Rect buttonRect = new Rect(rect);
            buttonRect.width = 20;
            buttonRect.x += rect.width - 45;
            buttonRect.y += 1;
            buttonRect.height -= 4;
            List<Material> linked_materials = MaterialLinker.GetLinked(ShaderEditor.active.currentProperty.materialProperty);
            Texture2D icon = Styles.icon_link_inactive;
            if (linked_materials != null)
                icon = Styles.icon_link_active;
            if (GUI.Button(buttonRect, icon, EditorStyles.largeLabel))
            {
                MaterialLinker.Popup(buttonRect, linked_materials, ShaderEditor.active.currentProperty.materialProperty);
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
                Mediator.transfer_group = property;
            });
            menu.AddItem(new GUIContent("Paste"), false, delegate ()
            {
                if (Mediator.copy_material != null || Mediator.transfer_group != null)
                {
                    property.TransferFromMaterialAndGroup(Mediator.copy_material, Mediator.transfer_group);
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
            foreach (ShaderPart part in ShaderEditor.active.shaderParts)
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

        public static void HeaderHiderGUI(ShaderEditor editor)
        {
            EditorGUILayout.BeginHorizontal(Styles.style_toolbar);
            if (GUILayout.Button("Simple", Styles.style_toolbar_toggle(state == HeaderHidingType.simple)))
                SetType(HeaderHidingType.simple);
            if (GUILayout.Button("Show All", Styles.style_toolbar_toggle(state == HeaderHidingType.show_all)))
                SetType(HeaderHidingType.show_all);
            Rect right = GUILayoutUtility.GetRect(10, 20);
            Rect arrow = new Rect(right.x + right.width - 20, right.y, 20, 20);
            if (GUI.Button(arrow, Styles.icon_menu, EditorStyles.largeLabel))
                DrawHeaderHiderMenu(arrow, editor.shaderParts);
            if (GUI.Button(right, "Custom", Styles.style_toolbar_toggle(state == HeaderHidingType.custom)))
                SetType(HeaderHidingType.custom);

            GUI.Button(arrow, Styles.icon_menu, EditorStyles.largeLabel);

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