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

    public class ThryEditorHeader
    {
        private MaterialProperty property;
        private bool currentState;

        public ThryEditorHeader(MaterialProperty prop)
        {
            this.property = prop;
            this.currentState = fetchState();
        }

        public bool fetchState()
        {
            return property.floatValue == 1;
        }

        public bool getState()
        {
            return this.currentState;
        }

        public void Toggle()
        {

            if (getState())
                property.floatValue = 0;
            else
                property.floatValue = 1;
            this.currentState = !this.currentState;
        }

        public void Foldout(int xOffset, GUIContent content, ThryEditor gui)
        {
            PropertyOptions options = ThryEditor.currentlyDrawing.currentProperty.options;
            Event e = Event.current;
            GUIStyle style = new GUIStyle(Styles.dropDownHeader);
            style.margin.left = 15 * xOffset + 15;

            Rect rect = GUILayoutUtility.GetRect(16f + 20f, 22f, style);
            DrawingData.lastGuiObjectHeaderRect = rect;

            DrawBoxAndContent(rect, e, content, options, style);
            
            DrawSmallArrow(rect,e);
            HandleToggleInput(e,rect);
        }

        private void DrawBoxAndContent(Rect rect, Event e, GUIContent content, PropertyOptions options, GUIStyle style)
        {
            if (options.reference_property != null)
            {
                GUI.Box(rect, "", style);
                DrawIcons(rect, e);
                DrawButton(rect, options, e, style);

                Rect togglePropertyRect = new Rect(rect);
                togglePropertyRect.x -= 11;
                togglePropertyRect.y += 2;
                ShaderProperty prop = ThryEditor.currentlyDrawing.propertyDictionary[options.reference_property];
                float labelWidth = EditorGUIUtility.labelWidth;
                float fieldWidth = EditorGUIUtility.fieldWidth;
                EditorGUIUtility.labelWidth = UnityHelper.CalculateLengthOfText(prop.content.text) + EditorGUI.indentLevel * 15 + 45;
                EditorGUIUtility.fieldWidth = 20;
                prop.Draw(new CRect(togglePropertyRect), content);
                EditorGUIUtility.labelWidth = labelWidth;
                EditorGUIUtility.fieldWidth = fieldWidth;
            }
            else
            {
                GUI.Box(rect, content, style);
                DrawIcons(rect, e);
                DrawButton(rect, options, e, style);
            }

        }

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
                float maxY = GUIUtility.ScreenToGUIPoint(new Vector2(0, EditorWindow.focusedWindow.position.y + Screen.height)).y - 2.5f*buttonRect.height;
                buttonRect.y = Mathf.Min(buttonRect.y- buttonRect.height/2, maxY);

                ShowHeaderContextMenu(buttonRect, ThryEditor.currentlyDrawing.currentProperty, ThryEditor.currentlyDrawing.materials[0]);
            }
        }

        private void DrawLinkSettings(Rect rect, Event e)
        {
            Rect buttonRect = new Rect(rect);
            buttonRect.width = 20;
            buttonRect.x += rect.width - 45;
            buttonRect.y += 1;
            buttonRect.height -= 4;
            List<Material> linked_materials = MaterialLinker.GetLinked(ThryEditor.currentlyDrawing.currentProperty.materialProperty);
            Texture2D icon = Styles.inactive_link_icon;
            if (linked_materials!=null)
                icon = Styles.active_link_icon;
            if (GUI.Button(buttonRect, icon, EditorStyles.largeLabel))
            {
                MaterialLinker.Popup(buttonRect,linked_materials, ThryEditor.currentlyDrawing.currentProperty.materialProperty);
                e.Use();
            }
        }

        void ShowHeaderContextMenu(Rect position,ShaderPart property, Material material)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Reset"), false, delegate()
            {
                property.CopyFromMaterial(new Material(material.shader));
                List<Material> linked_materials = MaterialLinker.GetLinked(property.materialProperty);
                if (linked_materials != null)
                    foreach (Material m in linked_materials)
                        property.CopyToMaterial(m);
            });
            menu.AddItem(new GUIContent("Copy"), false, delegate()
            {
                Mediator.copy_material = new Material(material);
            });
            menu.AddItem(new GUIContent("Paste"), false, delegate()
            {
                property.CopyFromMaterial(Mediator.copy_material);
                List<Material> linked_materials = MaterialLinker.GetLinked(property.materialProperty);
                if (linked_materials != null)
                    foreach (Material m in linked_materials)
                        property.CopyToMaterial(m);
            });
            menu.DropDown(position);
        }

        private void DrawSmallArrow(Rect rect, Event e)
        {
            if (e.type == EventType.Repaint)
            {
                var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
                EditorStyles.foldout.Draw(toggleRect, false, false, getState(), false);
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
}
