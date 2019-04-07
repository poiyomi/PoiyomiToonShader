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
    public void drawPresets(MaterialEditor materialEditor, MaterialProperty[] props, Material material)
    {
        if (hasPresets&&presetsLoaded)
        {
            int newSelectedPreset = EditorGUILayout.Popup(selectedPreset, presetOptions, GUILayout.MaxWidth(100));
            if (newSelectedPreset != selectedPreset)
            {
                selectedPreset = newSelectedPreset;
                if (selectedPreset != presetOptions.Length - 1) applyPreset(presetOptions[selectedPreset], materialEditor, props,material);
            }
            if (selectedPreset == presetOptions.Length - 1) drawNewPreset(props, material);
        }
    }

    public void drawNewPreset(MaterialProperty[] props, Material material)
    {
        newPresetName = GUILayout.TextField(newPresetName, GUILayout.MaxWidth(100));

        if (GUILayout.Button("Add", GUILayout.Width(40), GUILayout.Height(20)))
        {
            addNewPreset(newPresetName, props, material);
        }
    }

    public void addNewPreset(string name, MaterialProperty[] props, Material material)
    {
        //find all non default values

        //add to presets list
        List<string[]> sets = new List<string[]>();

        foreach (MaterialProperty p in props)
        {
            string[] set = new string[] { p.name, "" };
            bool empty = false;
            Material defaultValues = new Material(material.shader);
            switch (p.type)
            {
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    if (defaultValues != null && defaultValues.GetFloat(Shader.PropertyToID(set[0]))== p.floatValue) empty = true;
                    set[1] = "" + p.floatValue;
                    break;
                case MaterialProperty.PropType.Texture:
                    if (p.textureValue == null) empty = true;
                    else set[1] = "" + p.textureValue.name;
                    break;
                case MaterialProperty.PropType.Vector:
                    if (defaultValues != null && defaultValues.GetVector(Shader.PropertyToID(set[0])).Equals(p.vectorValue)) empty = true;
                    set[1] = "" + p.vectorValue.x + "," + p.vectorValue.y + "," + p.vectorValue.z + "," + p.vectorValue.w;
                    break;
                case MaterialProperty.PropType.Color:
                    if (defaultValues != null && defaultValues.GetColor(Shader.PropertyToID(set[0])).Equals(p.colorValue)) empty = true;
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

    public void applyPreset(string presetName, MaterialEditor materialEditor, MaterialProperty[] props, Material material)
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
                            material.SetTexture(Shader.PropertyToID(set[0]), tex);
                        }
                    }
                    else if (p.type == MaterialProperty.PropType.Float || p.type == MaterialProperty.PropType.Range)
                    {
                        float value;
                        if (float.TryParse(set[1], out value)) material.SetFloat(Shader.PropertyToID(set[0]), value);
                    }
                    else if (p.type == MaterialProperty.PropType.Vector)
                    {
                        string[] xyzw = set[1].Split(",".ToCharArray());
                        Vector4 vector = new Vector4(float.Parse(xyzw[0]), float.Parse(xyzw[1]), float.Parse(xyzw[2]), float.Parse(xyzw[3]));
                        material.SetVector(Shader.PropertyToID(set[0]), vector);
                    }
                    else if (p.type == MaterialProperty.PropType.Color)
                    {
                        float[] rgb = new float[3];
                        string[] rgbString = set[1].Split(',');
                        float.TryParse(rgbString[0], out rgb[0]);
                        float.TryParse(rgbString[1], out rgb[1]);
                        float.TryParse(rgbString[2], out rgb[2]);
                        material.SetColor(Shader.PropertyToID(set[0]), new Color(rgb[0], rgb[1], rgb[2]));
                    }
                }else if (set[0] == "render_queue")
                {
                    int q = 0;
                    Debug.Log(set[0] + "," + set[1]);
                    if(int.TryParse(set[1],out q)){
                        material.renderQueue = q;
                    }
                }
            }
        }
    }
}
