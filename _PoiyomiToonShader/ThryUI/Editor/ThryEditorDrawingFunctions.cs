// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Thry
{

    public class GuiHelper
    {

        public static void drawConfigTextureProperty(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor, bool scaleOffset)
        {
            if (Config.Get().useBigTextures) drawBigTextureProperty(position, prop, label, editor, scaleOffset);
            else drawSmallTextureProperty(position, prop, label, editor, scaleOffset);
        }

        public static void drawSmallTextureProperty(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor, bool scaleOffset, bool has_panning_field = false)
        {
            Rect thumbnailPos = position;
            thumbnailPos.x += scaleOffset ? 20 : 0;
            editor.TexturePropertyMiniThumbnail(thumbnailPos, prop,label.text, (scaleOffset? "Click here for scale / offset":"") + (label.tooltip != "" ? " | " : "") + label.tooltip);
            if (scaleOffset && DrawingData.currentTexProperty != null)
            {
                //draw dropdown triangle
                thumbnailPos.x += ThryEditor.currentlyDrawing.currentProperty.xOffset * 30;
                if (Event.current.type == EventType.Repaint)
                    EditorStyles.foldout.Draw(thumbnailPos, false, false, DrawingData.currentTexProperty.showScaleOffset, false);
                //test click and draw scale/offset
                if (DrawingData.currentTexProperty.showScaleOffset)
                {
                    ThryEditor.currentlyDrawing.editor.TextureScaleOffsetProperty(prop);
                    if (has_panning_field && ThryEditor.currentlyDrawing.currentProperty.options.reference_property != null)
                    {
                        ThryEditor.ShaderProperty pan_property = ThryEditor.currentlyDrawing.propertyDictionary[ThryEditor.currentlyDrawing.currentProperty.options.reference_property];
                        EditorGUI.indentLevel *= 2;
                        ThryEditor.currentlyDrawing.editor.ShaderProperty(GUILayoutUtility.GetRect(pan_property.content, Styles.Get().vectorPropertyStyle), pan_property.materialProperty, pan_property.content);
                        EditorGUI.indentLevel /= 2;
                    }
                }
                if (ThryEditor.MouseClick && position.Contains(Event.current.mousePosition))
                {
                    DrawingData.currentTexProperty.showScaleOffset = !DrawingData.currentTexProperty.showScaleOffset;
                    editor.Repaint();
                }
            }

            DrawingData.lastGuiObjectRect = position;
        }

        public static void drawBigTextureProperty(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor, bool scaleOffset)
        {
            GUILayoutUtility.GetRect(label, Styles.Get().bigTextureStyle);
            editor.TextureProperty(position, prop, label.text, label.tooltip, scaleOffset);
            DrawingData.lastGuiObjectRect = position;
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
            int newCustomQueueFieldInput = EditorGUILayout.IntField(customQueueFieldInput, GUILayout.MaxWidth(65));
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

        //draw single footer
        private static void drawFooter(ButtonData data)
        {
            Button(data,20);
        }

        public static void Button(ButtonData button)
        {
            Button(button, -1);
        }

        public static void Button(ButtonData button, int default_height)
        {
            GUIContent content;
            Rect cursorRect;
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

        public static void DrawHeader(ref bool enabled, ref bool options, GUIContent name)
        {
            var r = EditorGUILayout.BeginHorizontal("box");
            enabled = EditorGUILayout.Toggle(enabled, EditorStyles.radioButton, GUILayout.MaxWidth(15.0f));
            options = GUI.Toggle(r, options, GUIContent.none, new GUIStyle());
            EditorGUILayout.LabelField(name, Styles.Get().dropDownHeaderLabel);
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawMasterLabel(string shaderName, float y)
        {
            Rect rect = new Rect(0, y, Screen.width, 18);
            EditorGUI.LabelField(rect,"<size=16>" + shaderName + "</size>", Styles.Get().masterLabel);
        }
    }

    //-----------------------------------------------------------------

    public class ThryEditorHeader
    {
        private List<MaterialProperty> propertyes;
        private bool currentState;

        public ThryEditorHeader(MaterialEditor materialEditor, string propertyName)
        {
            this.propertyes = new List<MaterialProperty>();
            foreach (Material materialEditorTarget in materialEditor.targets)
            {
                UnityEngine.Object[] asArray = new UnityEngine.Object[] { materialEditorTarget };
                propertyes.Add(MaterialEditor.GetMaterialProperty(asArray, propertyName));
            }

            this.currentState = fetchState();
        }

        public bool fetchState()
        {
            foreach (MaterialProperty materialProperty in propertyes)
            {
                if (materialProperty.floatValue == 1)
                    return true;
            }



            return false;
        }

        public bool getState()
        {
            return this.currentState;
        }

        public void Toggle()
        {

            if (getState())
            {
                foreach (MaterialProperty materialProperty in propertyes)
                {
                    materialProperty.floatValue = 0;
                }
            }
            else
            {
                foreach (MaterialProperty materialProperty in propertyes)
                {
                    materialProperty.floatValue = 1;
                }
            }

            this.currentState = !this.currentState;
        }

        public void Foldout(int xOffset, GUIContent content, ThryEditor gui)
        {
            var style = new GUIStyle(Styles.Get().dropDownHeader);
            style.margin.left = 30 * xOffset;

            var rect = GUILayoutUtility.GetRect(16f + 20f, 22f, style);
            DrawingData.lastGuiObjectRect = rect;
            //rect with text
            GUI.Box(rect, content, style);

            PropertyOptions options = ThryEditor.currentlyDrawing.currentProperty.options;
            if (options.button_right!=null && options.button_right.condition_show.Test())
            {
                Rect buttonRect = new Rect(rect);
                GUIContent buttoncontent = new GUIContent(options.button_right.text, options.button_right.hover);
                float width = Styles.Get().dropDownHeaderButton.CalcSize(buttoncontent).x;
                width = width < rect.width/3 ? rect.width/3 : width;
                buttonRect.x += buttonRect.width-width-10;
                buttonRect.y += 2;
                buttonRect.width = width;
                if (GUI.Button(buttonRect, buttoncontent, Styles.Get().dropDownHeaderButton))
                    if(options.button_right.action!=null)
                        options.button_right.action.Perform();
                EditorGUIUtility.AddCursorRect(buttonRect, MouseCursor.Link);
            }

            var e = Event.current;

            var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            if (e.type == EventType.Repaint)
            {
                //small arrow
                EditorStyles.foldout.Draw(toggleRect, false, false, getState(), false);
            }

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition)&&!e.alt)
            {
                this.Toggle();
                e.Use();
            }
        }
    }
}
