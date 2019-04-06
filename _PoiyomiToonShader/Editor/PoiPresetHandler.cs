using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PoiPresetHandler {

    //variables for the presets
    private bool hasPresets = false;
    private bool presetsLoaded = false;
    private string presetsFilePath = null;
    private Dictionary<string, List<string[]>> presets = new Dictionary<string, List<string[]>>(); //presets

    //variabled for the preset selector
    public int selectedPreset = 0;
    private string[] presetOptions;
    string newPresetName = "Preset Name";

    public PoiPresetHandler(MaterialProperty[] props)
    {
        testPresetsChanged(props);
    }

    //test if the path to the presets has changed
    public void testPresetsChanged(MaterialProperty[] props)
    {
        MaterialProperty presetsProperty = PoiToon.FindProperty(props, "shader_presets");
        if (!(presetsProperty == null))
        {
            string newPath = stringToFilePath(presetsProperty.displayName);
            if (presetsFilePath != newPath)
            {
                presetsFilePath = newPath;
                if (hasPresets) { loadPresets(); }
            }
        }else {
            hasPresets = false;
        }
    }

    //get the path to the presets from file name
    private string stringToFilePath(string name)
    {
        string[] guid = AssetDatabase.FindAssets(name, null);
        if (guid.Length > 0)
        {
            hasPresets = true;
            return AssetDatabase.GUIDToAssetPath(guid[0]);
        }
        return null;
    }

    //loads presets from file
    public void loadPresets()
    {
        presets.Clear();
        StreamReader reader = new StreamReader(presetsFilePath);
        string line;
        List<string[]> currentPreset = null;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.Length > 0 && !line.StartsWith("//"))
            {
                if (line.Contains("="))
                {
                    currentPreset.Add(line.Split(new string[] { " = " }, System.StringSplitOptions.None));
                }
                else
                {
                    currentPreset = new List<string[]>();
                    presets.Add(line, currentPreset);
                }
            }
        }
        reader.Close();
        presetOptions = new string[presets.Count + 2];
        presetOptions[0] = "Presets";
        presetOptions[presets.Count + 1] = "+ New +";
        int i = 1;
        foreach (string k in presets.Keys) presetOptions[i++] = k;
        presetsLoaded = true;
    }

    //draws presets if exists
    public void drawPresets(MaterialEditor materialEditor, MaterialProperty[] props)
    {
        if (hasPresets&&presetsLoaded)
        {
            int newSelectedPreset = EditorGUILayout.Popup(selectedPreset, presetOptions, GUILayout.MaxWidth(100));
            if (newSelectedPreset != selectedPreset)
            {
                selectedPreset = newSelectedPreset;
                if (selectedPreset != presetOptions.Length - 1) applyPreset(presetOptions[selectedPreset], materialEditor, props);
            }
            if (selectedPreset == presetOptions.Length - 1) drawNewPreset(props);
        }
    }

    public void drawNewPreset(MaterialProperty[] props)
    {
        newPresetName = GUILayout.TextField(newPresetName, GUILayout.MaxWidth(100));

        if (GUILayout.Button("Add", GUILayout.Width(40), GUILayout.Height(20)))
        {
            addNewPreset(newPresetName, props);
        }
    }

    public void addNewPreset(string name, MaterialProperty[] props)
    {
        //find all non default values

        //add to presets list
        List<string[]> sets = new List<string[]>();

        foreach (MaterialProperty p in props)
        {
            string[] set = new string[] { p.name, "" };
            bool empty = false;
            switch (p.type)
            {
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    set[1] = "" + p.floatValue;
                    break;
                case MaterialProperty.PropType.Texture:
                    if (p.textureValue == null) empty = true;
                    else set[1] = "" + p.textureValue.name;
                    break;
                case MaterialProperty.PropType.Color:
                    set[1] = "" + p.colorValue.r + "," + p.colorValue.g + "," + p.colorValue.b;
                    break;
            }
            if (p.flags != MaterialProperty.PropFlags.HideInInspector && !empty) sets.Add(set);
        }

        //fix all preset variables
        presets.Add(name, sets);
        string[] newPresetOptions = new string[presetOptions.Length + 1];
        for (int i = 0; i < presetOptions.Length; i++) newPresetOptions[i] = presetOptions[i];
        newPresetOptions[newPresetOptions.Length - 1] = presetOptions[newPresetOptions.Length - 2];
        newPresetOptions[newPresetOptions.Length - 2] = name;
        presetOptions = newPresetOptions;
        newPresetName = "Preset Name";

        //save all presets into file
        StreamWriter writer = new StreamWriter(presetsFilePath, false);
        foreach (KeyValuePair<string, List<string[]>> preset in presets)
        {
            writer.WriteLine(preset.Key);
            foreach (string[] set in preset.Value) writer.WriteLine(set[0] + " = " + set[1]);
            writer.WriteLine("");
        }
        writer.Close();
    }

    public void applyPreset(string presetName, MaterialEditor materialEditor, MaterialProperty[] props)
    {
        List<string[]> sets;
        if (presets.TryGetValue(presetName, out sets))
        {
            foreach (string[] set in sets)
            {
                MaterialProperty p = PoiToon.FindProperty(props, set[0]);
                if (p != null)
                {
                    if (p.type == MaterialProperty.PropType.Texture)
                    {
                        string[] guids = AssetDatabase.FindAssets(set[1] + " t:Texture", null);
                        if (guids.Length == 0) Debug.LogError("Couldn't find texture: " + set[1]);
                        else
                        {
                            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                            Texture tex = (Texture)EditorGUIUtility.Load(path);
#pragma warning disable CS0618 // Type or member is obsolete
                            materialEditor.SetTexture(set[0], tex);
                        }
                    }
                    else if (p.type == MaterialProperty.PropType.Float || p.type == MaterialProperty.PropType.Range)
                    {
                        float value;
                        if (float.TryParse(set[1], out value)) materialEditor.SetFloat(set[0], value);
                    }
                    else if (p.type == MaterialProperty.PropType.Color)
                    {
                        float[] rgb = new float[3];
                        string[] rgbString = set[1].Split(',');
                        float.TryParse(rgbString[0], out rgb[0]);
                        float.TryParse(rgbString[1], out rgb[1]);
                        float.TryParse(rgbString[2], out rgb[2]);
                        materialEditor.SetColor(set[0], new Color(rgb[0], rgb[1], rgb[2]));
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                }
            }
        }
    }
}
