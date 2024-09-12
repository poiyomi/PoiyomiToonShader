using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static Thry.GradientEditor;
using static Thry.TexturePacker;

namespace Thry
{
    public class GradientDrawer : MaterialPropertyDrawer
    {
        GradientData data;

        Rect border_position;
        Rect gradient_position;

        Dictionary<UnityEngine.Object, GradientData> _gradient_data = new Dictionary<UnityEngine.Object, GradientData>();

        private void Init(MaterialProperty prop, bool replace = false)
        {
            if (!replace && _gradient_data.ContainsKey(prop.targets[0]))
            {
                data = _gradient_data[prop.targets[0]];
                return;
            }
            data = new GradientData();
            data.PreviewTexture = prop.textureValue;
            _gradient_data[prop.targets[0]] = data;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            Init(prop);
            ShaderEditor.Active.Editor.EndAnimatedCheck(); // Fixes all dropdoen properties being animated / highlighted

            if (Config.Singleton.default_texture_type == TextureDisplayType.small)
            {
                UpdateRects(position, prop);
                if (ShaderEditor.Input.Click && border_position.Contains(Event.current.mousePosition))
                    Open(prop);
                GUILib.SmallTextureProperty(position, prop, label, editor, DrawingData.CurrentTextureProperty.hasFoldoutProperties);
                GradientField();
            }
            else
            {
                position = new RectOffset(-30, 0, 0, 0).Add(position);
                Rect top_bg_rect = new Rect(position);
                Rect label_rect = new Rect(position);
                Rect button_select = new Rect(position);
                top_bg_rect = new RectOffset(0, 0, 0, 25).Add(top_bg_rect);
                label_rect = new RectOffset(-5, 5, -3, 3).Add(label_rect);
                button_select = new RectOffset((int)button_select.width - 120, 20, 2, 0).Remove(button_select);

                GUILayoutUtility.GetRect(position.width, 30); // get space for gradient
                border_position = new Rect(position.x, position.y + position.height, position.width, 30);
                border_position = new RectOffset(3, 3, 0, 0).Remove(border_position);
                gradient_position = new RectOffset(1, 1, 1, 1).Remove(border_position);
                if (ShaderEditor.Input.Click && border_position.Contains(Event.current.mousePosition))
                    Open(prop);

                GUI.DrawTexture(top_bg_rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Styles.COLOR_BACKGROUND_1, 3, 10);

                if (DrawingData.CurrentTextureProperty.hasScaleOffset || DrawingData.CurrentTextureProperty.Options.reference_properties != null)
                {
                    Rect extraPropsBackground = EditorGUILayout.BeginVertical();
                    extraPropsBackground.x = position.x;
                    extraPropsBackground.width = position.width;
                    extraPropsBackground.y = extraPropsBackground.y - 25;
                    extraPropsBackground.height = extraPropsBackground.height + 25;
                    float propertyX = extraPropsBackground.x + 15;
                    float propertyWidth = extraPropsBackground.width - 30;
                    GUI.DrawTexture(extraPropsBackground, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Styles.COLOR_BACKGROUND_1, 3, 10);
                    Rect r;
                    if (DrawingData.CurrentTextureProperty.hasScaleOffset)
                    {
                        r = GUILayoutUtility.GetRect(propertyWidth, 48);
                        r.x = propertyX;
                        r.y -= 8;
                        r.width = propertyWidth;
                        editor.TextureScaleOffsetProperty(r, prop);
                    }
                    if (DrawingData.CurrentTextureProperty.Options.reference_properties != null)
                    {
                        float labelWidth = EditorGUIUtility.labelWidth;
                        EditorGUIUtility.labelWidth = 100;
                        propertyX -= 30;
                        foreach (string pName in DrawingData.CurrentTextureProperty.Options.reference_properties)
                        {
                            ShaderProperty property = ShaderEditor.Active.PropertyDictionary[pName];
                            if (property != null)
                            {
                                r = GUILayoutUtility.GetRect(propertyWidth, editor.GetPropertyHeight(property.MaterialProperty, property.Content.text) + 3);
                                r.x = propertyX;
                                r.width = propertyWidth;
                                property.Draw(r);
                            }
                        }
                        EditorGUIUtility.labelWidth = labelWidth;
                    }
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    GUILayoutUtility.GetRect(0, 5);
                    Rect backgroundBottom = new RectOffset(3, 3, -5, 10).Add(border_position);
                    GUI.DrawTexture(backgroundBottom, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Styles.COLOR_BACKGROUND_1, 3, 10);
                }

                bool changed = GUILib.HandleTexturePicker(prop);
                changed |= GUILib.AcceptDragAndDrop(border_position, prop);
                if (changed)
                    Init(prop, true);
                if (GUI.Button(button_select, "Select", EditorStyles.miniButton))
                {
                    GUILib.OpenTexturePicker(prop);
                }

                GradientField();
                GUI.Label(label_rect, label);

                GUILayoutUtility.GetRect(0, 5);
            }

            ShaderEditor.Active.Editor.BeginAnimatedCheck(prop);
        }

        private void Open(MaterialProperty prop)
        {
            ShaderEditor.Input.Use();
            PropertyOptions options = ShaderEditor.Active.CurrentProperty.Options;
            GradientEditor.Open(data, prop, options.texture, options.force_texture_options, !options.force_texture_options);
        }

        private void UpdateRects(Rect position, MaterialProperty prop)
        {
            border_position = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth - GUILib.GetSmallTextureVRAMWidth(prop), position.height);
            gradient_position = new Rect(border_position.x + 1, border_position.y + 1, border_position.width - 2, border_position.height - 2);
        }

        private void GradientField()
        {
            DrawBackgroundTexture();
            if (data.PreviewTexture != null)
                DrawGradientTexture();
            else
                GUI.DrawTexture(border_position, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, Color.grey, 1, 1);
        }

        private void DrawBackgroundTexture()
        {
            Texture2D backgroundTexture = TextureHelper.GetBackgroundTexture();
            Rect texCoordsRect = new Rect(0, 0, gradient_position.width / backgroundTexture.width, gradient_position.height / backgroundTexture.height);
            GUI.DrawTextureWithTexCoords(gradient_position, backgroundTexture, texCoordsRect, false);
        }

        private void DrawGradientTexture()
        {
            TextureWrapMode wrap_mode = data.PreviewTexture.wrapMode;
            data.PreviewTexture.wrapMode = TextureWrapMode.Clamp;
            bool vertical = data.PreviewTexture.height > data.PreviewTexture.width;
            Vector2 pivot = new Vector2();
            if (vertical)
            {
                pivot = new Vector2(gradient_position.x, gradient_position.y + gradient_position.height);
                GUIUtility.RotateAroundPivot(-90, pivot);
                gradient_position.y += gradient_position.height;
                float h = gradient_position.width;
                gradient_position.width = gradient_position.height;
                gradient_position.y += h;
                gradient_position.height = -h;
            }
            GUI.DrawTexture(gradient_position, data.PreviewTexture, ScaleMode.StretchToFill, true);
            if (vertical)
            {
                GUIUtility.RotateAroundPivot(90, pivot);
            }
            GUI.DrawTexture(border_position, data.PreviewTexture, ScaleMode.StretchToFill, false, 0, Color.grey, 1, 1);
            data.PreviewTexture.wrapMode = wrap_mode;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.LastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

}