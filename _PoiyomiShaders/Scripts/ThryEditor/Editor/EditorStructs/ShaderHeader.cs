using System.Collections.Generic;
using System.Linq;
using Thry.ThryEditor.Helpers;
using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor
{
    public class ShaderHeader : ShaderGroup
    {

        public ShaderHeader(ShaderEditor shaderEditor) : base(shaderEditor)
        {
        }

        public ShaderHeader(ShaderEditor shaderEditor, MaterialProperty prop, MaterialEditor materialEditor, string displayName, int xOffset, string optionsRaw, int propertyIndex) : base(shaderEditor, prop, materialEditor, displayName, xOffset, optionsRaw, propertyIndex)
        {
        }

        protected override void DrawInternal(GUIContent content, Rect? rect = null, bool useEditorIndent = false, bool isInHeader = false)
        {
            MyShaderUI.CurrentProperty = this;
            EditorGUI.BeginChangeCheck();
            Rect position = GUILayoutUtility.GetRect(content, Styles.dropdownHeader);
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
                foreach (ShaderPart part in Children)
                {
                    part.Draw();
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Space();
            }
            if (EditorGUI.EndChangeCheck())
                UpdateLinkedMaterials();
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
                if(ShaderEditor.Active.Locale.EditInUI)
                {
                    GUI.Box(rect, new GUIContent("", MaterialProperty.name), Styles.dropdownHeader);
                    Rect translationRect = new Rect(rect);
                    translationRect.x += 40;
                    translationRect.y += 1;
                    translationRect.width -= 100;
                    translationRect.height -= 4;
                    EditorGUI.BeginChangeCheck();
                    string newTranslation = EditorGUI.DelayedTextField(translationRect, _content.text);
                    if(EditorGUI.EndChangeCheck())
                    {
                        Content = new GUIContent(newTranslation);
                        ShaderEditor.Active.Locale.Set(MaterialProperty.name, newTranslation);
                        ShaderEditor.Active.Locale.Save();
                    }
                }else
                {
                    GUI.Box(rect, new GUIContent("     " + content.text, content.tooltip), Styles.dropdownHeader);
                }
                
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

                refProperty.XOffset.SetTemporaryOffset(0);
                refProperty.Draw(togglePropertyRect, new GUIContent(), isInHeader: true);
                refProperty.XOffset.ResetTemporaryOffset();
                EditorGUIUtility.fieldWidth = fieldWidth;

                // Change expand state if reference is toggled
                if (EditorGUI.EndChangeCheck() && Options.ref_float_toggles_expand)
                {
                    IsExpanded = refProperty.MaterialProperty.GetNumber() == 1;
                }
            }
            else
            {
                GUI.Box(rect, content, Styles.dropdownHeader);
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
                if (GUILib.Button(rect, Icons.help))
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
                if (GUILib.Button(rect, Icons.presets))
                {
                    ShaderEditor.Input.Use();
                    Presets.OpenPresetsMenu(rect, MyShaderUI, true, this.MaterialProperty.name);
                }
            }
        }

        private void DrawDowdownSettings(Rect rect, Event e)
        {
            if (GUILib.Button(rect, Icons.menu))
            {
                ShaderEditor.Input.Use();
                Rect buttonRect = new Rect(rect);
                buttonRect.width = 150;
                buttonRect.x = Mathf.Min(Screen.width - buttonRect.width, buttonRect.x);
                buttonRect.height = 60;
                float maxY = GUIUtility.ScreenToGUIPoint(new Vector2(0, EditorWindow.focusedWindow.position.y + Screen.height)).y - 2.5f * buttonRect.height;
                buttonRect.y = Mathf.Min(buttonRect.y - buttonRect.height / 2, maxY);

                ShowHeaderContextMenu(buttonRect, this, ShaderEditor.Active.Materials);
            }
        }

        private void DrawLinkSettings(Rect rect, Event e)
        {
            if (GUILib.Button(rect, Icons.linked, Color.cyan, MaterialLinker.IsLinked(ShaderEditor.Active.CurrentProperty.MaterialProperty)))
            {
                ShaderEditor.Input.Use();
                IEnumerable<Material> linked_materials = MaterialLinker.GetLinked(ShaderEditor.Active.CurrentProperty.MaterialProperty);
                MaterialLinker.Popup(rect, linked_materials, ShaderEditor.Active.CurrentProperty.MaterialProperty);
            }
        }

        void ShowHeaderContextMenu(Rect position, ShaderHeader property, Material[] materials)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Reset"), false, delegate ()
            {
                ThryLogger.LogDetail("ShaderHeader", $"Resetting '{property.Content.text}' of {ShaderEditor.Active.Materials[0].name}");
                int undoGroup = Undo.GetCurrentGroup();

                property.CopyFrom(new Material(materials[0].shader), true);
                IEnumerable<Material> linked_materials = MaterialLinker.GetLinked(property.MaterialProperty);
                if (linked_materials != null)
                    foreach (Material m in linked_materials)
                        property.CopyTo(m, true);

                Undo.SetCurrentGroupName($"Reset {property.Content.text} of {ShaderEditor.Active.Materials[0].name}");
                Undo.CollapseUndoOperations(undoGroup);
            });
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Copy"), false, delegate ()
            {
                ThryLogger.LogDetail("ShaderHeader", $"Copying '{property.Content.text}' of {ShaderEditor.Active.Materials[0].name}");
                Mediator.copy_material = new Material(materials[0]);
                Mediator.copy_part = property;
            });
            menu.AddItem(new GUIContent("Paste"), false, delegate ()
            {
                if (Mediator.copy_material != null || Mediator.copy_part != null)
                {
                    ThryLogger.LogDetail("ShaderHeader", $"Pasting '{property.Content.text}' of {ShaderEditor.Active.Materials[0].name}");
                    int undoGroup = Undo.GetCurrentGroup();

                    property.CopyFrom(Mediator.copy_part);
                    property.UpdateLinkedMaterials();

                    Undo.SetCurrentGroupName($"Paste {property.Content.text} of {ShaderEditor.Active.Materials[0].name}");
                    Undo.CollapseUndoOperations(undoGroup);
                }
            });
            menu.AddItem(new GUIContent("Paste without Textures"), false, delegate ()
            {
                if (Mediator.copy_material != null || Mediator.copy_part != null)
                {
                    ThryLogger.LogDetail("ShaderHeader", $"Pasting* '{property.Content.text}' of {ShaderEditor.Active.Materials[0].name}");
                    int undoGroup = Undo.GetCurrentGroup();

                    var propsToIgnore = new HashSet<MaterialProperty.PropType> { MaterialProperty.PropType.Texture };
                    property.CopyFrom(Mediator.copy_part, skipPropertyTypes: propsToIgnore);
                    property.UpdateLinkedMaterials();

                    Undo.SetCurrentGroupName($"Paste* {property.Content.text} of {ShaderEditor.Active.Materials[0].name}");
                    Undo.CollapseUndoOperations(undoGroup);
                }
            });
            menu.AddItem(new GUIContent("Paste Special..."), false, () =>
            {
                if(Mediator.copy_material == null || Mediator.copy_part == null)
                    return;
                
                ThryLogger.LogDetail("ShaderHeader", $"Pasting** '{property.Content.text}' of {ShaderEditor.Active.Materials[0].name}");
                var popup = ScriptableObject.CreateInstance<PasteSpecialPopup>();
                popup.Init(Mediator.copy_part);
                popup.ShowUtility();
                
                popup.OnPasteClicked += (disabledPartsList) =>
                {
                    HashSet<string> ignoreProperties = new HashSet<string>(disabledPartsList.Select(p => p.MaterialProperty.name));
                    if (Mediator.copy_material != null || Mediator.copy_part != null)
                    {
                        int undoGroup = Undo.GetCurrentGroup();

                        property.CopyFrom(Mediator.copy_part, skipPropertyNames: ignoreProperties);
                        property.UpdateLinkedMaterials();

                        Undo.SetCurrentGroupName($"Paste** {property.Content.text} of {ShaderEditor.Active.Materials[0].name}");
                        Undo.CollapseUndoOperations(undoGroup);
                    }
                };
                
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