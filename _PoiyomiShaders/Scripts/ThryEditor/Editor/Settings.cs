// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class Settings : EditorWindow
    {

        public static void firstTimePopup()
        {
            Settings window = (Settings)EditorWindow.GetWindow(typeof(Settings));
            window.isFirstPopop = true;
            window.Show();
        }

        public static void updatedPopup(int compare)
        {
            Settings window = (Settings)EditorWindow.GetWindow(typeof(Settings));
            window.updatedVersion = compare;
            window.Show();
        }

        public new void Show()
        {
            base.Show();
            this.titleContent = new GUIContent("Thry Settings");
        }

        public ModuleSettings[] moduleSettings;

        private bool isFirstPopop = false;
        private int updatedVersion = 0;

        private bool is_init = false;
        
        public static ButtonData thry_message = null;

        //---------------------Stuff checkers and fixers-------------------

        public void Awake()
        {
            InitVariables();
        }

        private void InitVariables()
        {
            List<Type> subclasses = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(type => type.IsSubclassOf(typeof(ModuleSettings))).ToList();
            moduleSettings = new ModuleSettings[subclasses.Count];
            int i = 0;
            foreach(Type classtype in subclasses)
            {
                moduleSettings[i++] = (ModuleSettings)Activator.CreateInstance(classtype);
            }

            is_init = true;

            if (thry_message == null)
                WebHelper.DownloadStringASync(Thry.URL.SETTINGS_MESSAGE_URL, delegate (string s) { thry_message = Parser.ParseToObject<ButtonData>(s); });
        }

        //------------------Main GUI
        void OnGUI()
        {
            if (!is_init || moduleSettings==null) InitVariables();
            GUILayout.Label("ShaderUI v" + Config.Singleton.verion);

            GUINotification();
            DrawHorizontalLine();
            GUIMessage();
            LocaleDropdown();
            GUIEditor();
            DrawHorizontalLine();
            foreach(ModuleSettings s in moduleSettings)
            {
                s.Draw();
                DrawHorizontalLine();
            }
            GUIModulesInstalation();
        }

        //--------------------------GUI Helpers-----------------------------

        private static void DrawHorizontalLine()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }

        private void GUINotification()
        {
            if (isFirstPopop)
                GUILayout.Label(" " + Locale.editor.Get("first_install_message"), Styles.greenStyle);
            else if (updatedVersion == -1)
                GUILayout.Label(" " + Locale.editor.Get("update_message"), Styles.greenStyle);
            else if (updatedVersion == 1)
                GUILayout.Label(" " + Locale.editor.Get("downgrade_message"), Styles.orangeStyle);
        }

        private void GUIMessage()
        {
            if(thry_message!=null)
            {
                bool doDrawLine = false;
                if(thry_message.text.Length > 0)
                {
                    doDrawLine = true;
                    GUILayout.Label(new GUIContent(thry_message.text,thry_message.hover), thry_message.center_position?Styles.richtext_center: Styles.richtext);
                    Rect r = GUILayoutUtility.GetLastRect();
                    if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
                        thry_message.action.Perform(ShaderEditor.Active?.Materials);
                }
                if(thry_message.texture != null)
                {
                    doDrawLine = true;
                    if(thry_message.center_position) GUILayout.Label(new GUIContent(thry_message.texture.loaded_texture, thry_message.hover), EditorStyles.centeredGreyMiniLabel, GUILayout.MaxHeight(thry_message.texture.height));
                    else GUILayout.Label(new GUIContent(thry_message.texture.loaded_texture, thry_message.hover), GUILayout.MaxHeight(thry_message.texture.height));
                    Rect r = GUILayoutUtility.GetLastRect();
                    if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
                        thry_message.action.Perform(ShaderEditor.Active?.Materials);
                }
                if(doDrawLine)
                    DrawHorizontalLine();
            }
        }

        bool is_editor_expanded = true;
        private void GUIEditor()
        {
            is_editor_expanded = Foldout(Locale.editor.Get("header_editor"), is_editor_expanded);
            if (is_editor_expanded)
            {
                EditorGUI.indentLevel += 2;
                Dropdown("default_texture_type");
                Toggle("showRenderQueue");
                Toggle("showManualReloadButton");

                EditorGUILayout.Space();
                Toggle("autoMarkPropertiesAnimated");
                Toggle("allowCustomLockingRenaming");
                GUIGradients();
                EditorGUILayout.Space();

                Toggle("autoSetAnchorOverride");
                Dropdown("humanBoneAnchor");
                Text("anchorOverrideObjectName");
                
                EditorGUI.indentLevel -= 2;
            }
        }

        private static void GUIGradients()
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            Text("gradient_name", false);
            string gradient_name = Config.Singleton.gradient_name;
            if (gradient_name.Contains("<hash>"))
                GUILayout.Label(Locale.editor.Get("gradient_good_naming"), Styles.greenStyle, GUILayout.ExpandWidth(false));
            else if (gradient_name.Contains("<material>"))
                if (gradient_name.Contains("<prop>"))
                    GUILayout.Label(Locale.editor.Get("gradient_good_naming"), Styles.greenStyle, GUILayout.ExpandWidth(false));
                else
                    GUILayout.Label(Locale.editor.Get("gradient_add_hash_or_prop"), Styles.orangeStyle, GUILayout.ExpandWidth(false));
            else if (gradient_name.Contains("<prop>"))
                GUILayout.Label(Locale.editor.Get("gradient_add_material"), Styles.orangeStyle, GUILayout.ExpandWidth(false));
            else
                GUILayout.Label(Locale.editor.Get("gradient_add_material_or_prop"), Styles.redStyle, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }

        private class TextPopup : EditorWindow
        {
            public string text = "";
            private Vector2 scroll;
            void OnGUI()
            {
                EditorGUILayout.SelectableLabel(Locale.editor.Get("my_data_header"), EditorStyles.boldLabel);
                Rect last = GUILayoutUtility.GetLastRect();
                
                Rect data_rect = new Rect(0, last.height, Screen.width, Screen.height - last.height);
                scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Width(data_rect.width), GUILayout.Height(data_rect.height));
                GUILayout.TextArea(text);
                EditorGUILayout.EndScrollView();
            }
        }

        private void GUIModulesInstalation()
        {
            if (ModuleHandler.GetFirstPartyModules() == null)
                return;
            if (ModuleHandler.GetFirstPartyModules().Count > 0) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(Locale.editor.Get("header_modules"), EditorStyles.boldLabel);
                if (GUILayout.Button("Reload"))
                    ModuleHandler.ForceReloadModules();
                EditorGUILayout.EndHorizontal();
            }
            bool disabled = ModuleHandler.GetFirstPartyModules().Any(m => m.is_being_installed_or_removed);
            disabled |= ModuleHandler.GetThirdPartyModules().Any(m => m.is_being_installed_or_removed);
            EditorGUI.BeginDisabledGroup(disabled);
            foreach (Module module in ModuleHandler.GetFirstPartyModules())
            {
                ModuleUI(module);
            }
            GUILayout.Label(Locale.editor.Get("header_thrird_party"), EditorStyles.boldLabel);
            foreach (Module module in ModuleHandler.GetThirdPartyModules())
            {
                ModuleUI(module);
            }
            EditorGUI.EndDisabledGroup();
        }

        private void ModuleUI(Module module)
        {
            string text = "      " + module.available_module.name;
            if (module.update_available)
                text = "                  " + text;
            module.ui_expanded = Foldout(text, module.ui_expanded);
            Rect rect = GUILayoutUtility.GetLastRect();
            rect.x += 20;
            rect.y += 1;
            rect.width = 20;
            rect.height -= 4;

            bool is_installed = module.installed_module != null;

            EditorGUI.BeginDisabledGroup(!module.available_requirement_fullfilled);
            EditorGUI.BeginChangeCheck();
            bool install = GUI.Toggle(rect, is_installed, "");
            if(EditorGUI.EndChangeCheck()){
                ModuleHandler.InstallRemoveModule(module, install);
            }
            if (module.update_available)
            {
                rect.x += 20;
                rect.width = 55;
                GUIStyle style = new GUIStyle(EditorStyles.miniButton);
                style.fixedHeight = 17;
                if (GUI.Button(rect, "Update",style))
                    ModuleHandler.UpdateModule(module);
            }
            //add update notification
            if (module.ui_expanded)
            {
                EditorGUI.indentLevel += 1;
                ModuleUIDetails(module);
                EditorGUI.indentLevel -= 1;
            }

            EditorGUI.EndDisabledGroup();
        }

        private void ModuleUIDetails(Module module)
        {
            float prev_label_width = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 130;

            EditorGUILayout.HelpBox(module.available_module.description, MessageType.Info);
            if (module.installed_module != null)
                EditorGUILayout.LabelField("Installed Version: ", module.installed_module.version);
            EditorGUILayout.LabelField("Available Version: ", module.available_module.version);
            if (module.available_module.requirement != null)
            {
                if (module.available_requirement_fullfilled)
                    EditorGUILayout.LabelField(Locale.editor.Get("requirements") + ": ", module.available_module.requirement.ToString(), Styles.greenStyle);
                else
                    EditorGUILayout.LabelField(Locale.editor.Get("requirements") + ": ", module.available_module.requirement.ToString(), Styles.redStyle);
            }
            EditorGUILayout.LabelField("Url: ", module.url);
            if (module.author != null)
                EditorGUILayout.LabelField("Author: ", module.author);

            EditorGUIUtility.labelWidth = prev_label_width;
        }

        private static void Text(string configField, bool createHorizontal = true)
        {
            Text(configField, Locale.editor.Get(configField), Locale.editor.Get(configField + "_tooltip"), createHorizontal);
        }

        private static void Text(string configField, string[] content, bool createHorizontal=true)
        {
            Text(configField, content[0], content[1], createHorizontal);
        }

        private static void Text(string configField, string text, string tooltip, bool createHorizontal)
        {
            Config config = Config.Singleton;
            System.Reflection.FieldInfo field = typeof(Config).GetField(configField);
            if (field != null)
            {
                string value = (string)field.GetValue(config);
                if (createHorizontal)
                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                GUILayout.Space(57);
                GUILayout.Label(new GUIContent(text, tooltip), GUILayout.ExpandWidth(false));
                EditorGUI.BeginChangeCheck();
                value = EditorGUILayout.DelayedTextField("", value, GUILayout.ExpandWidth(false));
                if (EditorGUI.EndChangeCheck())
                {
                    field.SetValue(config, value);
                    config.Save();
                }
                if (createHorizontal)
                    GUILayout.EndHorizontal();
            }
        }

        private static void Toggle(string configField, GUIStyle label_style = null)
        {
            Toggle(configField, Locale.editor.Get(configField), Locale.editor.Get(configField + "_tooltip"), label_style);
        }

        private static void Toggle(string configField, string[] content, GUIStyle label_style = null)
        {
            Toggle(configField, content[0], content[1], label_style);
        }

        private static void Toggle(string configField, string label, string hover, GUIStyle label_style = null)
        {
            Config config = Config.Singleton;
            System.Reflection.FieldInfo field = typeof(Config).GetField(configField);
            if (field != null)
            {
                bool value = (bool)field.GetValue(config);
                if (Toggle(value, label, hover, label_style) != value)
                {
                    field.SetValue(config, !value);
                    config.Save();
                    ShaderEditor.RepaintActive();
                }
            }
        }

        private static void Dropdown(string configField)
        {
            Dropdown(configField, Locale.editor.Get(configField),Locale.editor.Get(configField+"_tooltip"));
        }

        private static void Dropdown(string configField, string[] content)
        {
            Dropdown(configField, content[0], content[1]);
        }

        private static void Dropdown(string configField, string label, string hover, GUIStyle label_style = null)
        {
            Config config = Config.Singleton;
            System.Reflection.FieldInfo field = typeof(Config).GetField(configField);
            if (field != null)
            {
                Enum value = (Enum)field.GetValue(config);
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(57);
                GUILayout.Label(new GUIContent(label, hover), GUILayout.ExpandWidth(false));
                value = EditorGUILayout.EnumPopup(value,GUILayout.ExpandWidth(false));
                EditorGUILayout.EndHorizontal();
                if(EditorGUI.EndChangeCheck())
                {
                    field.SetValue(config, value);
                    config.Save();
                    ShaderEditor.RepaintActive();
                }
            }
        }

        private static void LocaleDropdown()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(Locale.editor.Get("locale"), Locale.editor.Get("locale_tooltip")), GUILayout.ExpandWidth(false));
            Locale.editor.selected_locale_index = EditorGUILayout.Popup(Locale.editor.selected_locale_index, Locale.editor.available_locales, GUILayout.ExpandWidth(false));
            if(Locale.editor.Get("translator").Length>0)
                GUILayout.Label(Locale.editor.Get("translation") +": "+Locale.editor.Get("translator"), GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();
            if(EditorGUI.EndChangeCheck())
            {
                Config.Singleton.locale = Locale.editor.available_locales[Locale.editor.selected_locale_index];
                Config.Singleton.Save();
                ShaderEditor.ReloadActive();
            }
        }

        private static bool Toggle(bool val, string text, GUIStyle label_style = null)
        {
            return Toggle(val, text, "",label_style);
        }

        private static bool Toggle(bool val, string text, string tooltip, GUIStyle label_style=null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(35);
            val = GUILayout.Toggle(val, new GUIContent("", tooltip), GUILayout.ExpandWidth(false));
            if(label_style==null)
                GUILayout.Label(new GUIContent(text, tooltip));
            else
                GUILayout.Label(new GUIContent(text, tooltip),label_style);
            GUILayout.EndHorizontal();
            return val;
        }

        private static bool Foldout(string text, bool expanded)
        {
            return Foldout(new GUIContent(text), expanded);
        }

        private static bool Foldout(GUIContent content, bool expanded)
        {
            var rect = GUILayoutUtility.GetRect(16f + 20f, 22f, Styles.dropDownHeader);
            rect = EditorGUI.IndentedRect(rect);
            GUI.Box(rect, content, Styles.dropDownHeader);
            var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            Event e = Event.current;
            if (e.type == EventType.Repaint)
                EditorStyles.foldout.Draw(toggleRect, false, false, expanded, false);
            if (e.type == EventType.MouseDown && toggleRect.Contains(e.mousePosition) && !e.alt)
            {
                expanded = !expanded;
                e.Use();
            }
            return expanded;
        }
    }
}