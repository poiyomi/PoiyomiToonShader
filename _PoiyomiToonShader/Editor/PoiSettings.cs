using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PoiSettings : EditorWindow
{

    PoiHelper.Config config;
    public static Shader activeShader = null;
    public static PoiPresetHandler presetHandler;

    public static readonly int[] COMMON_QUEUES = new int[] { 0, 10, 20, 30, 100, 200, 300, 1000, 1990, 1995, 1996, 1997, 1998, 1999, 2000, 2001, 2002, 2003, 2004, 2005, 2010, 2440, 2445, 2446, 2447, 2448, 2449, 2450, 2451, 2452, 2453, 2454, 2455, 2460, 2990, 2995, 2996, 2997, 2998, 2999, 3000, 3001, 3002, 3004, 3005, 3010 };

    // Add menu named "My Window" to the Window menu
    [MenuItem("PoiToon/Poi Settings")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        PoiSettings window = (PoiSettings)EditorWindow.GetWindow(typeof(PoiSettings));
        window.Show();
    }

    int createShadersFrom = 2000;
    int createShadersTo = 2010;

    private void OnSelectionChange()
    {
        string[] selectedAssets = Selection.assetGUIDs;
        if (selectedAssets.Length == 1)
        {
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(selectedAssets[0]));
            if (obj.GetType() == typeof(Shader))
            {
                Shader shader = (Shader)obj;
                Material m = new Material(shader);
                if (m.HasProperty(Shader.PropertyToID("shader_eable_poi_settings_selection")))
                {
                    MaterialProperty[] props = MaterialEditor.GetMaterialProperties(new Material[] { m });
                    activeShader = shader;
                    presetHandler = new PoiPresetHandler(props);
                }
            }
        }
        this.Repaint();
    }

    private void drawLine()
    {
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        rect.height = 1;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
    }

    private Vector2 scrollPos;

    int selectedShader = 0;
    string[] poiShaders;

    bool reload = true;

    void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        GUILayout.Label("Config", EditorStyles.boldLabel);
        config = PoiHelper.GetConfig();

        if (GUILayout.Toggle(config.bigTextures, "Big Texture Fields") != config.bigTextures)
        {
            config.bigTextures = !config.bigTextures;
            config.save();
            PoiHelper.RepaintAllMaterialEditors();
        }

        if (GUILayout.Toggle(config.useRenderQueueSelection, "Use Render Queue Selection") != config.useRenderQueueSelection)
        {
            config.useRenderQueueSelection = !config.useRenderQueueSelection;
            config.save();
            PoiHelper.RepaintAllMaterialEditors();
        }

        drawLine();


        if (poiShaders == null || reload)
        {

            string[] shaderGuids = AssetDatabase.FindAssets("t:shader");
            List<string> poiShaders = new List<string>();
            foreach (string g in shaderGuids)
            {
                Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath(g));
                Material m = new Material(shader);
                if (m.HasProperty(Shader.PropertyToID("shader_eable_poi_settings_selection")))
                {
                    string defaultShaderName = PoiHelper.getDefaultShaderName(shader.name);
                    if (!poiShaders.Contains(defaultShaderName)) poiShaders.Add(defaultShaderName);
                }
            }
            this.poiShaders = new string[poiShaders.Count + 1];
            for (int i = 0; i < poiShaders.Count; i++) this.poiShaders[i + 1] = poiShaders[i];
        }

        if (GUILayout.Button("Backup Poi Materials", GUILayout.MaxWidth(150))) PoiHelper.saveAllPoiMaterials();
        if (GUILayout.Button("Restore Poi Materials", GUILayout.MaxWidth(150))) PoiHelper.restorePoiMaterials();
        drawLine();

        if (activeShader != null) poiShaders[0] = PoiHelper.getDefaultShaderName(activeShader.name);
        else poiShaders[0] = "None";
        int newSelectShader = EditorGUILayout.Popup(0, poiShaders, GUILayout.MaxWidth(200));
        if (newSelectShader != selectedShader)
        {
            selectedShader = newSelectShader;
            activeShader = Shader.Find(poiShaders[newSelectShader]);
            presetHandler = new PoiPresetHandler(activeShader);
        }

        if (reload) presetHandler = new PoiPresetHandler(activeShader);

        if (activeShader != null)
        {
            string defaultShaderName = PoiHelper.getDefaultShaderName(activeShader.name); ;
            Shader defaultShader = Shader.Find(defaultShaderName);

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.richText = true;
            EditorGUILayout.LabelField("<size=16>" + defaultShaderName + "</size>", style, GUILayout.MinHeight(18));

            GUILayout.Label("Generate Render Queue Shaders", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate All Queues", GUILayout.MaxWidth(200)))
            {
                for (int i = createShadersFrom; i <= createShadersTo; i++) { PoiHelper.createRenderQueueShaderIfNotExists(defaultShader, i, false); }
                AssetDatabase.Refresh();
            }
            GUILayout.Label("from", GUILayout.MaxWidth(30));
            createShadersFrom = EditorGUILayout.IntField(createShadersFrom, GUILayout.MaxWidth(50));
            GUILayout.Label("to", GUILayout.MaxWidth(15));
            createShadersTo = EditorGUILayout.IntField(createShadersTo, GUILayout.MaxWidth(50));
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Generate most common Queues", GUILayout.MaxWidth(200)))
            {
                foreach (int i in COMMON_QUEUES) { PoiHelper.createRenderQueueShaderIfNotExists(defaultShader, i, false); }
                AssetDatabase.Refresh();
            }
            if (presetHandler != null) presetHandler.drawPresetsSettings();
        }
        GUILayout.EndScrollView();
        reload = false;
        if (GUILayout.Button("ReLoad")) reload = true;
    }

    public static PoiSettings getInstance()
    {
        PoiSettings instance = (PoiSettings)PoiHelper.FindEditorWindow(typeof(PoiSettings));
        if (instance == null) instance = ScriptableObject.CreateInstance<PoiSettings>();
        return instance;
    }
}