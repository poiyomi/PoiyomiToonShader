// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class PresetEditor : EditorWindow
    {
        // Add menu named "My Window" to the Window menu
        [MenuItem("Thry/Editor Tools/Preset Editor")]
        static void Init()
        {
            setupStyle();
            // Get existing open window or if none, make a new one:
            PresetEditor window = (PresetEditor)EditorWindow.GetWindow(typeof(PresetEditor));
            window.Show();
            window.loadActiveShader();
        }

        private static void setupStyle()
        {
            propertyBackground = new GUIStyle();
            Texture2D bg = new Texture2D(1, 1);
            bg.SetPixel(1, 1, Color.yellow);
            propertyBackground.normal.background = bg;
        }

        public static void open()
        {
            PresetEditor.Init();
        }

        private string[] shaders;
        private static int selectedShaderIndex = -1;

        private bool newPreset = false;
        private string newPresetName;

        

        private static GUIStyle propertyBackground;

        private void loadActiveShader()
        {
            Shader activeShader = Mediator.active_shader;
            if (activeShader != null && this.shaders != null) for (int i = 0; i < this.shaders.Length; i++) if (this.shaders[i] == activeShader.name) selectedShaderIndex = i;
        }

        private int selectedPreset = 0;
        private int addPropertyIndex = 0;
        Vector2 scrollPos;
        private List<string[]> properties = new List<string[]>();
        private string[] unusedProperties;
        bool reloadProperties = true;

        void OnGUI()
        {
            if (shaders == null)
            {
                this.shaders = ShaderHelper.GetThryEditorShaderNames();
                if(Mediator.active_shader != null)
                    for(int i= 0;i < shaders.Length;i++)
                        if (shaders[i] == Mediator.active_shader.name) selectedShaderIndex = i;
            }
            if (propertyBackground == null) setupStyle();
            Shader activeShader = Mediator.active_shader;
            int newIndex = EditorGUILayout.Popup(selectedShaderIndex, shaders, GUILayout.MaxWidth(500));
            if (selectedShaderIndex == -1) newIndex = 0;
            if (newIndex != selectedShaderIndex)
            {
                selectedShaderIndex = newIndex;
                selectedPreset = 0;
                Mediator.SetActiveShader(Shader.Find(shaders[selectedShaderIndex]));
                activeShader = Mediator.active_shader;
                reloadProperties = true;
            }
            if (activeShader != null)
            {
                PresetHandler presetHandler = Mediator.active_shader_preset_handler;
                if (presetHandler.shaderHasPresetPath())
                {
                    Dictionary<string, List<string[]>> presets = presetHandler.getPresets();
                    string[] presetStrings = new string[presets.Count + 1];
                    int i = 0;
                    foreach (KeyValuePair<string, List<string[]>> entry in presets) presetStrings[i++] = entry.Key;
                    presetStrings[presets.Count] = Locale.editor.Get("new_preset2");
                    GUILayout.BeginHorizontal();
                    int newSelectedPreset = EditorGUILayout.Popup(selectedPreset, presetStrings, GUILayout.MaxWidth(500));
                    if (newSelectedPreset != selectedPreset || reloadProperties)
                    {
                        this.selectedPreset = newSelectedPreset;
                        if (newSelectedPreset == presetStrings.Length - 1)
                        {
                            newPreset = true;
                            newPresetName = Locale.editor.Get("new_preset_name2");
                            properties = null;
                        }
                        else
                        {
                            this.properties = presetHandler.getPropertiesOfPreset(presetStrings[selectedPreset]);
                            List<string> unusedProperties = new List<string>();
                            foreach (string pName in presetHandler.getPropertyNames())
                            {
                                bool unused = true;
                                foreach (string[] p in this.properties) if (p[0] == pName) unused = false;
                                if (unused) unusedProperties.Add(pName);
                            }
                            this.unusedProperties = unusedProperties.ToArray();
                            reloadProperties = false;
                            newPreset = false;
                        }
                    }
                    if (GUILayout.Button(Locale.editor.Get("delete"), GUILayout.MaxWidth(80)))
                    {
                        presetHandler.removePreset(presetStrings[selectedPreset]);
                        reloadProperties = true;
                        Repaint();
                    }
                    GUILayout.EndHorizontal();
                    if (newPreset)
                    {
                        GUILayout.BeginHorizontal();
                        newPresetName = GUILayout.TextField(newPresetName, GUILayout.MaxWidth(150));
                        if (GUILayout.Button(Locale.editor.Get("add_preset"), GUILayout.MaxWidth(80)))
                        {
                            presetHandler.addNewPreset(newPresetName);
                            reloadProperties = true;
                            Repaint();
                            selectedPreset = presetStrings.Length - 1;
                        }
                        GUILayout.EndHorizontal();
                    }
                    scrollPos = GUILayout.BeginScrollView(scrollPos);
                    if (properties != null)
                    {
                        for (i = 0; i < properties.Count; i++)
                        {
                            if (i % 2 == 0) GUILayout.BeginHorizontal(propertyBackground);
                            else GUILayout.BeginHorizontal();
                            //properties[i][0] = GUILayout.TextField(properties[i][0], GUILayout.MaxWidth(200));
                            GUILayout.Label(properties[i][0], GUILayout.MaxWidth(150));

                            bool typeFound = false;
                            ShaderUtil.ShaderPropertyType propertyType = ShaderUtil.ShaderPropertyType.Float;
                            for (int p = 0; p < ShaderUtil.GetPropertyCount(activeShader); p++)
                                if (ShaderUtil.GetPropertyName(activeShader, p) == properties[i][0])
                                {
                                    propertyType = ShaderUtil.GetPropertyType(activeShader, p);
                                    typeFound = true;
                                    break;
                                }
                            if (typeFound)
                            {
                                switch (propertyType)
                                {
                                    case ShaderUtil.ShaderPropertyType.Color:
                                        float[] rgba = new float[4] { 1, 1, 1, 1 };
                                        string[] rgbaString = properties[i][1].Split(',');
                                        if (rgbaString.Length > 0) float.TryParse(rgbaString[0], out rgba[0]);
                                        if (rgbaString.Length > 1) float.TryParse(rgbaString[1], out rgba[1]);
                                        if (rgbaString.Length > 2) float.TryParse(rgbaString[2], out rgba[2]);
                                        if (rgbaString.Length > 3) float.TryParse(rgbaString[3], out rgba[3]);
                                        Color p = EditorGUI.ColorField(EditorGUILayout.GetControlRect(GUILayout.MaxWidth(204)), new GUIContent(), new Color(rgba[0], rgba[1], rgba[2], rgba[3]), true, true, true, new ColorPickerHDRConfig(0, 1000, 0, 1000));
                                        properties[i][1] = "" + p.r + "," + p.g + "," + p.b + "," + p.a;
                                        break;
                                    case ShaderUtil.ShaderPropertyType.TexEnv:
                                        Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(properties[i][1]);
#pragma warning disable CS0618 // Type or member is obsolete
                                        texture = (Texture)EditorGUI.ObjectField(EditorGUILayout.GetControlRect(GUILayout.MaxWidth(100)), texture, typeof(Texture));
                                        properties[i][1] = AssetDatabase.GetAssetPath(texture);
#pragma warning restore CS0618 // Type or member is obsolete
                                        GUILayout.Label("(" + properties[i][1] + ")", GUILayout.MaxWidth(100));
                                        break;
                                    case ShaderUtil.ShaderPropertyType.Vector:
                                        Vector4 vector = Converter.stringToVector(properties[i][1]);
                                        vector = EditorGUI.Vector4Field(EditorGUILayout.GetControlRect(GUILayout.MaxWidth(204)), "", vector);
                                        properties[i][1] = "" + vector.x + "," + vector.y + "," + vector.z + "," + vector.w;
                                        break;
                                    default:
                                        properties[i][1] = GUILayout.TextField(properties[i][1], GUILayout.MaxWidth(204));
                                        break;
                                }
                            }
                            else
                            {
                                properties[i][1] = GUILayout.TextField(properties[i][1], GUILayout.MaxWidth(204));
                            }
                            if (GUILayout.Button(Locale.editor.Get("delete"), GUILayout.MaxWidth(80)))
                            {
                                properties.RemoveAt(i);
                                this.reloadProperties = true;
                                saveProperties(presetHandler, presetStrings);
                            }
                            GUILayout.EndHorizontal();
                        }
                        //new preset gui
                        GUILayout.BeginHorizontal();
                        addPropertyIndex = EditorGUILayout.Popup(addPropertyIndex, unusedProperties, GUILayout.MaxWidth(150));
                        if (GUILayout.Button(Locale.editor.Get("add"), GUILayout.MaxWidth(80)))
                        {
                            this.reloadProperties = true;
                            properties.Add(new string[] { unusedProperties[addPropertyIndex], "" });
                            saveProperties(presetHandler, presetStrings);
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndScrollView();
                    if (GUILayout.Button(Locale.editor.Get("save"), GUILayout.MinWidth(50))) saveProperties(presetHandler, presetStrings);
                    Event e = Event.current;
                    if (e.isKey)
                    {
                        if (Event.current.keyCode == (KeyCode.Return))
                        {
                            saveProperties(presetHandler, presetStrings);
                        }
                    }
                }
            }
        }

        private void saveProperties(PresetHandler presetHandler, string[] presetStrings)
        {
            Debug.Log(Locale.editor.Get("preset_saved"));
            presetHandler.setPreset(presetStrings[selectedPreset], properties);
            Repaint();
        }
    }
}
