using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Thry.ThryEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Thry
{
    public class ShaderHeader : ShaderGroup
    {

        public ShaderHeader(ShaderEditor shaderEditor) : base(shaderEditor)
        {
        }

        public ShaderHeader(ShaderEditor shaderEditor, MaterialProperty prop, MaterialEditor materialEditor, string displayName, int xOffset, string optionsRaw, int propertyIndex) : base(shaderEditor, prop, materialEditor, displayName, xOffset, optionsRaw, propertyIndex)
        {
        }

        public override void DrawInternal(GUIContent content, Rect? rect = null, bool useEditorIndent = false, bool isInHeader = false)
        {
            ActiveShaderEditor.CurrentProperty = this;
            EditorGUI.BeginChangeCheck();
            Rect position = GUILayoutUtility.GetRect(content, Styles.dropDownHeader);
            DrawHeader(position, content);
            Rect headerRect = DrawingData.LastGuiObjectHeaderRect;
            if (IsExpanded)
            {
                if (ShaderEditor.Active.IsSectionedPresetEditor)
                {
                    string presetName = Presets.GetSectionPresetName(ShaderEditor.Active.Materials[0], this.MaterialProperty.name);
                    EditorGUI.BeginChangeCheck();
                    presetName = EditorGUILayout.DelayedTextField("Preset Name:", presetName);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Presets.SetSectionPreset(ShaderEditor.Active.Materials[0], this.MaterialProperty.name, presetName);
                    }
                }

                EditorGUILayout.Space();
                EditorGUI.BeginDisabledGroup(DoDisableChildren);
                foreach (ShaderPart part in parts)
                {
                    part.Draw();
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Space();
            }
            if (EditorGUI.EndChangeCheck())
                HandleLinkedMaterials();
            DrawingData.LastGuiObjectHeaderRect = headerRect;
            DrawingData.LastGuiObjectRect = headerRect;
        }

        private void DrawHeader(Rect position, GUIContent label)
        {
            PropertyOptions options = ShaderEditor.Active.CurrentProperty.Options;
            Event e = Event.current;

            int xOffset_total = XOffset * 15 + 15;

            position.width -= xOffset_total - position.x;
            position.x = xOffset_total;

            DrawingData.LastGuiObjectHeaderRect = position;
            DrawBoxAndContent(position, e, label, options);

            Rect arrowRect = new Rect(position) { height = 18 };
            FoldoutArrow(arrowRect, e);

            HandleToggleInput(position);
        }

        private void DrawBoxAndContent(Rect rect, Event e, GUIContent content, PropertyOptions options)
        {
            if (options.reference_property != null && ShaderEditor.Active.PropertyDictionary.ContainsKey(options.reference_property))
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
                ShaderProperty refProperty = ShaderEditor.Active.PropertyDictionary[options.reference_property];

                EditorGUI.BeginChangeCheck();

                int xOffset = refProperty.XOffset;
                refProperty.SetTemporaryXOffset(0);
                refProperty.Draw(togglePropertyRect, new GUIContent(), isInHeader: true);
                refProperty.ResetTemporaryXOffset();
                EditorGUIUtility.fieldWidth = fieldWidth;

                // Change expand state if reference is toggled
                if (EditorGUI.EndChangeCheck() && Options.ref_float_toggles_expand)
                {
                    IsExpanded = refProperty.MaterialProperty.GetNumber() == 1;
                }
            }
            // else if(keyword != null)
            // {
            //     GUI.Box(rect, "     " + content.text, Styles.dropDownHeader);
            //     DrawIcons(rect, options, e);

            //     Rect togglePropertyRect = new Rect(rect);
            //     togglePropertyRect.x += 20;
            //     togglePropertyRect.width = 20;

            //     EditorGUI.BeginChangeCheck();
            //     bool keywordOn = EditorGUI.Toggle(togglePropertyRect, "", ShaderEditor.Active.Materials[0].IsKeywordEnabled(keyword));
            //     if (EditorGUI.EndChangeCheck())
            //     {
            //         MaterialHelper.ToggleKeyword(ShaderEditor.Active.Materials, keyword, keywordOn);
            //     }
            // }
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
            Rect buttonRect = new Rect(rect);
            buttonRect.y += 1;
            buttonRect.height -= 4;
            buttonRect.width = buttonRect.height;

            float right = rect.x + rect.width;
            buttonRect.x = right - 74;
            DrawPresetButton(buttonRect, options, e);
            buttonRect.x = right - 56;
            DrawHelpButton(buttonRect, options, e);
            buttonRect.x = right - 38;
            DrawLinkSettings(buttonRect, e);
            buttonRect.x = right - 20;
            DrawDowdownSettings(buttonRect, e);
        }

        private void DrawHelpButton(Rect rect, PropertyOptions options, Event e)
        {
            ButtonData button = options.button_help;
            if (button != null && button.condition_show.Test())
            {
                if (GUILib.Button(rect, Styles.icon_style_help))
                {
                    ShaderEditor.Input.Use();
                    if (button.action != null) button.action.Perform(ShaderEditor.Active?.Materials);
                }
            }
        }

        private void DrawPresetButton(Rect rect, PropertyOptions options, Event e)
        {
            bool hasPresets = Presets.DoesSectionHavePresets(this.MaterialProperty.name);
            if (hasPresets)
            {
                if (GUILib.Button(rect, Styles.icon_style_presets))
                {
                    ShaderEditor.Input.Use();
                    Presets.OpenPresetsMenu(rect, ActiveShaderEditor, true, this.MaterialProperty.name);
                }
            }
        }

        private void DrawDowdownSettings(Rect rect, Event e)
        {
            if (GUILib.Button(rect, Styles.icon_style_menu))
            {
                ShaderEditor.Input.Use();
                Rect buttonRect = new Rect(rect);
                buttonRect.width = 150;
                buttonRect.x = Mathf.Min(Screen.width - buttonRect.width, buttonRect.x);
                buttonRect.height = 60;
                float maxY = GUIUtility.ScreenToGUIPoint(new Vector2(0, EditorWindow.focusedWindow.position.y + Screen.height)).y - 2.5f * buttonRect.height;
                buttonRect.y = Mathf.Min(buttonRect.y - buttonRect.height / 2, maxY);

                ShowHeaderContextMenu(buttonRect, ShaderEditor.Active.CurrentProperty, ShaderEditor.Active.Materials[0]);
            }
        }

        private void DrawLinkSettings(Rect rect, Event e)
        {
            if (GUILib.Button(rect, Styles.icon_style_linked, Styles.COLOR_ICON_ACTIVE_CYAN, MaterialLinker.IsLinked(ShaderEditor.Active.CurrentProperty.MaterialProperty)))
            {
                ShaderEditor.Input.Use();
                List<Material> linked_materials = MaterialLinker.GetLinked(ShaderEditor.Active.CurrentProperty.MaterialProperty);
                MaterialLinker.Popup(rect, linked_materials, ShaderEditor.Active.CurrentProperty.MaterialProperty);
            }
        }

        void ShowHeaderContextMenu(Rect position, ShaderPart property, Material material)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Reset"), false, delegate ()
            {
                property.CopyFromMaterial(new Material(material.shader), true);
                List<Material> linked_materials = MaterialLinker.GetLinked(property.MaterialProperty);
                if (linked_materials != null)
                    foreach (Material m in linked_materials)
                        property.CopyToMaterial(m, true);
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
                    property.TransferFromMaterialAndGroup(Mediator.copy_material, Mediator.transfer_group, true);
                    List<Material> linked_materials = MaterialLinker.GetLinked(property.MaterialProperty);
                    if (linked_materials != null)
                        foreach (Material m in linked_materials)
                            property.CopyToMaterial(m, true);
                }
            });
            menu.AddItem(new GUIContent("Paste without Textures"), false, delegate ()
            {
                if (Mediator.copy_material != null || Mediator.transfer_group != null)
                {
                    var propsToIgnore = new MaterialProperty.PropType[] { MaterialProperty.PropType.Texture };
                    property.TransferFromMaterialAndGroup(Mediator.copy_material, Mediator.transfer_group, true, propsToIgnore);
                    List<Material> linked_materials = MaterialLinker.GetLinked(property.MaterialProperty);
                    if (linked_materials != null)
                        foreach (Material m in linked_materials)
                            property.CopyToMaterial(m, true, propsToIgnore);
                }
            });
            menu.DropDown(position);
        }

        private void HandleToggleInput(Rect rect)
        {
            //Ignore unity uses is cause disabled will use the event to prevent toggling
            if (ShaderEditor.Input.LeftClick_IgnoreLocked && rect.Contains(ShaderEditor.Input.mouse_position) && !ShaderEditor.Input.is_alt_down)
            {
                IsExpanded = !IsExpanded;
                ShaderEditor.Input.Use();
            }
        }
    }

}