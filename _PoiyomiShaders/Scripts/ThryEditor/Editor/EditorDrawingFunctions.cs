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
            this.expanded = prop.floatValue==1;
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
            
            DrawSmallArrow(rect,e);
            HandleToggleInput(e,rect);
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
                togglePropertyRect.width = GUI.skin.font.fontSize*3;
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
                float maxY = GUIUtility.ScreenToGUIPoint(new Vector2(0, EditorWindow.focusedWindow.position.y + Screen.height)).y - 2.5f*buttonRect.height;
                buttonRect.y = Mathf.Min(buttonRect.y- buttonRect.height/2, maxY);

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
            if (linked_materials!=null)
                icon = Styles.active_link_icon;
            if (GUI.Button(buttonRect, icon, EditorStyles.largeLabel))
            {
                MaterialLinker.Popup(buttonRect,linked_materials, ShaderEditor.currentlyDrawing.currentProperty.materialProperty);
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
}
