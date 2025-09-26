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
                }
                else
                {
                    GUI.Box(rect, new GUIContent("     " + content.text, content.tooltip), Styles.dropdownHeader);
                    if (Config.Instance.showNotes && !string.IsNullOrWhiteSpace(Note))
                    {
                        Rect noteRect = new Rect(rect);
                        float reserved = NotesHelper.GetPackedRightReservation(rect, options, this.MaterialProperty.name, Styles.label_property_note);
                        noteRect.width = Mathf.Max(0f, noteRect.width - reserved);
                        GUI.Label(noteRect, Note, Styles.label_property_note);
                    }
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
                if(Config.Instance.showNotes && !string.IsNullOrWhiteSpace(Note))
                {
                    Rect noteRect = new Rect(rect);
                    float reserved = NotesHelper.GetPackedRightReservation(rect, options, this.MaterialProperty.name, Styles.label_property_note);
                    noteRect.width = Mathf.Max(0f, noteRect.width - reserved);
                    GUI.Label(noteRect, Note, Styles.label_property_note);
                }
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
            float step = buttonRect.width + 2f;

            // Pack from right to left without gaps
            buttonRect.x = right - step;
            DrawDowdownSettings(buttonRect, e);

            buttonRect.x -= step;
            DrawLinkSettings(buttonRect, e);

            bool hasHelp = options.button_help != null && options.button_help.condition_show.Test();
            if (hasHelp)
            {
                buttonRect.x -= step;
                DrawHelpButton(buttonRect, options, e);
            }

            bool hasPresets = Presets.DoesSectionHavePresets(this.MaterialProperty.name);
            if (hasPresets)
            {
                buttonRect.x -= step;
                DrawPresetButton(buttonRect, options, e);
            }

            bool hasAuthor = options.button_author != null && options.button_author.condition_show.Test();
            if (hasAuthor)
            {
                buttonRect.x -= step;
                DrawAuthorButton(buttonRect, options, e);
            }
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

        private void DrawAuthorButton(Rect rect, PropertyOptions options, Event e)
        {
            ButtonData button = options.button_author;
            if (button != null && button.condition_show.Test())
            {
                // Compute combined clickable area (label + icon)
                GUIContent nameContent = null;
                Vector2 nameSize = Vector2.zero;
                // Author label style: 1pt larger than miniLabel
                GUIStyle authorLabelStyle;
                {
                    int baseSize = EditorStyles.miniLabel.fontSize > 0 ? EditorStyles.miniLabel.fontSize : GUI.skin.label.fontSize;
                    authorLabelStyle = new GUIStyle(EditorStyles.miniLabel) { fontSize = baseSize + 1 };
                }
                bool hasText = !string.IsNullOrEmpty(button.text);
                if (hasText)
                {
                    nameContent = new GUIContent(button.text, button.hover);
                    nameSize = authorLabelStyle.CalcSize(nameContent);
                }

                float padding = hasText ? 2f : 0f;
                Rect nameRect = new Rect(rect);
                nameRect.x -= nameSize.x + padding;
                nameRect.width = nameSize.x;
                nameRect.y += Mathf.Max(0, (rect.height - EditorGUIUtility.singleLineHeight) * 0.5f);
                nameRect.height = EditorGUIUtility.singleLineHeight;

                Rect combinedRect = new Rect(nameRect);
                if (!hasText)
                    combinedRect = rect;
                else
                {
                    combinedRect.x = nameRect.x;
                    combinedRect.width = (rect.xMax - nameRect.x);
                    combinedRect.y = Mathf.Min(nameRect.y, rect.y);
                    combinedRect.height = Mathf.Max(nameRect.height, rect.height);
                }

                // Draw visuals
                if (hasText) GUI.Label(nameRect, nameContent, authorLabelStyle);
                GUI.Button(rect, GUIContent.none, Icons.github);

                // Single clickable hit area
                EditorGUIUtility.AddCursorRect(combinedRect, MouseCursor.Link);
                if (GUI.Button(combinedRect, GUIContent.none, GUIStyle.none))
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
            menu.AddSeparator("");
            if(Config.Instance.showNotes)
            {
                menu.AddItem(new GUIContent("Set Note"), false, () =>
                {
                    var popup = ScriptableObject.CreateInstance<SetNotePopup>();
                    popup.Init(this, new Rect());
                    popup.ShowUtility();
                });
                //menu.AddItem(new GUIContent("Clear Note"), false, () => { Note = null; }); // Too easy to missclick when there's no undo?
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Set Note"));
                //menu.AddDisabledItem(new GUIContent("Clear Note"));
            }

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