using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ThrySettings : EditorWindow
{
    // Add menu named "My Window" to the Window menu
    [MenuItem("Thry/Settings")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        ThrySettings window = (ThrySettings)EditorWindow.GetWindow(typeof(ThrySettings));
        window.Show();
    }

    public static void firstTimePopup()
    {
        ThrySettings window = (ThrySettings)EditorWindow.GetWindow(typeof(ThrySettings));
        window.isFirstPopop = true;
        window.Show();
    }

    public static void updatedPopup(int compare)
    {
        ThrySettings window = (ThrySettings)EditorWindow.GetWindow(typeof(ThrySettings));
        window.updatedVersion = compare;
        window.Show();
    }

    public static Shader activeShader = null;
    public static Material activeShaderMaterial = null;
    public static ThryPresetHandler presetHandler = null;

    private bool isFirstPopop = false;
    private int updatedVersion = 0;

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
                if (m.HasProperty(Shader.PropertyToID("shader_is_using_thry_editor")))
                {
                    setActiveShader(shader);
                }
            }
        }
        this.Repaint();
    }

    public class VRChatSdkImportTester : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            bool vrcImported = false;
            foreach (string s in importedAssets) if (s.Contains("VRCSDK2.dll")) vrcImported = true;

            bool hasVRCSdk = System.Type.GetType("VRC.AccountEditorWindow") != null;

            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                BuildTargetGroup.Standalone);
            if ((vrcImported | hasVRCSdk) && !symbols.Contains("VRC_SDK_EXISTS")) PlayerSettings.SetScriptingDefineSymbolsForGroup(
                          BuildTargetGroup.Standalone, symbols + ";VRC_SDK_EXISTS");
            else if (!hasVRCSdk && symbols.Contains("VRC_SDK_EXISTS")) PlayerSettings.SetScriptingDefineSymbolsForGroup(
                 BuildTargetGroup.Standalone, symbols.Replace(";VRC_SDK_EXISTS", ""));
        }
    }

    public static void setActiveShader(Shader shader)
    {
        if (shader != activeShader)
        {
            activeShader = shader;
            presetHandler = new ThryPresetHandler(shader);
            activeShaderMaterial = new Material(shader);
        }
    }

    private void drawLine()
    {
        Rect rect = EditorGUILayout.GetControlRect(false, 1);
        rect.height = 1;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
    }

    void OnGUI()
    {
        GUILayout.Label("ThryEditor v" + ThryConfig.VERSION);
        ThryConfig.Config config = ThryConfig.GetConfig();

        GUIStyle redInfostyle = new GUIStyle();
        redInfostyle.normal.textColor = Color.red;
        redInfostyle.fontSize = 16;

        GUIStyle normal = new GUIStyle();

        if (isFirstPopop)
            GUILayout.Label(" Please review your thry editor configuration", redInfostyle);
        else if (updatedVersion==-1)
            GUILayout.Label(" Thry editor has been updated", redInfostyle);
        else if (updatedVersion == 1)
            GUILayout.Label(" Warning: Thry editor version has declined", redInfostyle);

        drawLine();

        bool hasVRCSdk = System.Type.GetType("VRC.AccountEditorWindow") != null;
        bool vrcIsLoggedIn = EditorPrefs.HasKey("sdk#username");

        if (hasVRCSdk)
        {
            //VRC.AccountEditorWindow window = (VRC.AccountEditorWindow)EditorWindow.GetWindow(typeof(VRC.AccountEditorWindow));
            //window.Show();

            EditorGUILayout.BeginHorizontal();

            if (vrcIsLoggedIn)
            {
                GUILayout.Label("VRChat user: " + EditorPrefs.GetString("sdk#username"));
            }
            else
            {
                //GUILayout.Label("Not logged in", GUILayout.ExpandWidth(false));
            }
            EditorGUILayout.EndHorizontal();

            drawLine();
        }

        GUILayout.Label("Editor", EditorStyles.boldLabel);

        if (Toggle(config.useBigTextures, "Big Texture Fields", "Show big texure fields instead of small ones") != config.useBigTextures)
        {
            config.useBigTextures = !config.useBigTextures;
            config.save();
            ThryEditor.repaint();
        }

        GUILayout.BeginHorizontal();
        int newMaterialValuesUpdateRate = EditorGUILayout.IntField("",config.materialValuesUpdateRate,GUILayout.MaxWidth(50));
        GUILayout.Label(new GUIContent("Slider Update Rate (in milliseconds)", "change the update rate of float sliders to get a smoother editor experience"));
        GUILayout.EndHorizontal();
        if (newMaterialValuesUpdateRate != config.materialValuesUpdateRate)
        {
            config.materialValuesUpdateRate = newMaterialValuesUpdateRate;
            config.save();
            ThryEditor.reload();
            ThryEditor.repaint();
        }

        if (Toggle(config.showRenderQueue, "Use Render Queue" ,"enable a render queue selector") != config.showRenderQueue)
        {
            config.showRenderQueue = !config.showRenderQueue;
            config.save();
            ThryEditor.repaint();
        }

        drawLine();
        GUILayout.Label("Extras", EditorStyles.boldLabel);

        if (Toggle(config.showImportPopup, "Show popup on shader import", "This popup gives you the option to try to restore materials if they broke on importing") != config.showImportPopup)
        {
            config.showImportPopup = !config.showImportPopup;
            config.save();
            ThryEditor.repaint();
        }

        if (config.showRenderQueue)
            if (Toggle(config.renderQueueShaders, "Render Queue Shaders", "Have the render queue selector work with vrchat by creating seperate shaders for the different queues") != config.renderQueueShaders)
            {
                config.renderQueueShaders = !config.renderQueueShaders;
                config.save();
                ThryEditor.repaint();
            }

        drawLine();

        if (hasVRCSdk)
        {
            GUILayout.Label("VRChat features", EditorStyles.boldLabel);

            if (Toggle(config.vrchatAutoFillAvatarDescriptor, "Auto setup avatar descriptor", "Automatically setup the vrc_avatar_descriptor after adding it to a gameobject") != config.vrchatAutoFillAvatarDescriptor)
            {
                config.vrchatAutoFillAvatarDescriptor = !config.vrchatAutoFillAvatarDescriptor;
                config.save();
            }

            string[] options = new string[] { "Male", "Female", "None" };
            GUILayout.BeginHorizontal();
            int newVRCFallbackAnimationSet = EditorGUILayout.Popup(config.vrchatDefaultAnimationSetFallback, options, GUILayout.MaxWidth(45));
            if (newVRCFallbackAnimationSet != config.vrchatDefaultAnimationSetFallback)
            {
                config.vrchatDefaultAnimationSetFallback = newVRCFallbackAnimationSet;
                config.save();
            }
            GUILayout.Label(new GUIContent(" Fallback Default Animation Set", "is applied by auto avatar descriptor if gender of avatar couldn't be determend"), normal);
            GUILayout.EndHorizontal();

            if (Toggle(config.vrchatForceFallbackAnimationSet, "Force Fallback Default Animation Set", "always set default animation set as fallback set") != config.vrchatForceFallbackAnimationSet)
            {
                config.vrchatForceFallbackAnimationSet = !config.vrchatForceFallbackAnimationSet;
                config.save();
            }

            drawLine();
        }
    }

    private static bool Toggle(bool val, string text)
    {
        return Toggle(val, text, "");
    }

        private static bool Toggle(bool val, string text, string tooltip)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(35);
        val = GUILayout.Toggle(val, new GUIContent("",tooltip), GUILayout.ExpandWidth(false));
        GUILayout.Label(new GUIContent(text,tooltip));
        GUILayout.EndHorizontal();
        return val;
    }

    public static ThrySettings getInstance()
    {
        ThrySettings instance = (ThrySettings)ThryHelper.FindEditorWindow(typeof(ThrySettings));
        if (instance == null) instance = ScriptableObject.CreateInstance<ThrySettings>();
        return instance;
    }
}