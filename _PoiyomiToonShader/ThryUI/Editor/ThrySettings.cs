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
        //this is dope: this.ShowNotification(new GUIContent(s));

        // Add menu named "My Window" to the Window menu
        [MenuItem("Thry/Settings")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            Settings window = (Settings)EditorWindow.GetWindow(typeof(Settings));
            window.Show();
        }

        public static void firstTimePopup()
        {
            Settings window = (Settings)EditorWindow.GetWindow(typeof(Settings));
            window.isFirstPopop = true;
            window.is_data_share_expanded = true;
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
        }

        public const string RSP_DRAWING_DLL_CODE = "-r:System.Drawing.dll";
        public const string RSP_DRAWING_DLL_REGEX = @"-r:\s*System\.Drawing\.dll";

        public static Shader activeShader = null;
        public static Material activeShaderMaterial = null;
        public static PresetHandler presetHandler = null;

        public ModuleSettings[] moduleSettings;

        private bool isFirstPopop = false;
        private int updatedVersion = 0;

        private bool is_init = false;

        public static bool is_changing_vrc_sdk = false;
        
        public static ButtonData thry_message = null;

        private static string[][] SETTINGS_CONTENT = new string[][]
        {
        new string[]{ "Big Texture Fields", "Show big texure fields instead of small ones" },
        new string[]{ "Use Render Queue", "enable a render queue selector" },
        new string[]{ "Show popup on shader import", "This popup gives you the option to try to restore materials if they broke on importing" },
        new string[]{ "Render Queue Shaders", "Have the render queue selector work with vrchat by creating seperate shaders for the different queues" },
        new string[]{ "Gradient Save File Names", "configures the way gradient texture files are named. use <material>, <hash> and <prop> to identify the texture." },
        new string[]{ "Default Texture Display", "Select how your textures should be displayed if the property doesn't force the type." }
        };
        enum SETTINGS_IDX
        {
            bigTexFields = 0, render_queue = 1, show_popup_on_import = 2, render_queue_shaders = 3, gradient_file_name = 4, default_texture_type = 5
        };

        //------------------Message Calls-------------------------

        public void OnDestroy()
        {
            if ((isFirstPopop|| updatedVersion!=0) && Config.Get().share_user_data)
                Helper.SendAnalytics();
            if (!EditorPrefs.GetBool("thry_has_counted_user", false))
            {
                Helper.DownloadStringASync(URL.COUNT_USER, delegate (string s)
                {
                    if (s == "true")
                        EditorPrefs.SetBool("thry_has_counted_user", true);
                });
            }
            
            string projectPrefix = PlayerSettings.companyName + "." +PlayerSettings.productName;
            if (!EditorPrefs.GetBool(projectPrefix+"_thry_has_counted_project", false))
            {
                Helper.DownloadStringASync(URL.COUNT_PROJECT, delegate (string s)
                {
                    if (s == "true")
                        EditorPrefs.SetBool(projectPrefix+"_thry_has_counted_project", true);
                });
            }
        }

        //---------------------Stuff checkers and fixers-------------------

        //checks if slected shaders is using editor
        private void OnSelectionChange()
        {
            string[] selectedAssets = Selection.assetGUIDs;
            if (selectedAssets.Length == 1)
            {
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(selectedAssets[0]));
                if (obj.GetType() == typeof(Shader))
                {
                    Shader shader = (Shader)obj;
                    Material m = new Material(shader);
                    if (m.HasProperty(Shader.PropertyToID(ThryEditor.PROPERTY_NAME_USING_THRY_EDITOR)))
                    {
                        setActiveShader(shader);
                    }
                }
            }
            this.Repaint();
        }

        public void Awake()
        {
            InitVariables();
        }

        private void InitVariables()
        {
            is_changing_vrc_sdk = (Helper.LoadValueFromFile("delete_vrc_sdk", PATH.AFTER_COMPILE_DATA) == "true") || (Helper.LoadValueFromFile("update_vrc_sdk", PATH.AFTER_COMPILE_DATA) == "true");

            CheckAPICompatibility(); //check that Net_2.0 is ApiLevel
            CheckDrawingDll(); //check that drawing.dll is imported
            CheckVRCSDK();

            List<Type> subclasses = typeof(ModuleSettings).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(ModuleSettings))).ToList<Type>();
            moduleSettings = new ModuleSettings[subclasses.Count];
            int i = 0;
            foreach(Type classtype in subclasses)
            {
                moduleSettings[i++] = (ModuleSettings)Activator.CreateInstance(classtype);
            }

            is_init = true;

            if (thry_message == null)
                Helper.DownloadStringASync(Thry.URL.SETTINGS_MESSAGE_URL, delegate (string s) { thry_message = Parser.ParseToObject<ButtonData>(s); });
        }

        private static void CheckVRCSDK()
        {
            if (!Settings.is_changing_vrc_sdk)
                Helper.SetDefineSymbol(DEFINE_SYMBOLS.VRC_SDK_INSTALLED, VRCInterface.Get().sdk_is_installed);
        }

        private static void CheckAPICompatibility()
        {
            ApiCompatibilityLevel level = PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone);
            if (level == ApiCompatibilityLevel.NET_2_0_Subset)
                PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_2_0);
            Helper.SetDefineSymbol(DEFINE_SYMBOLS.API_NET_TWO, true, true);
        }

        private static void CheckDrawingDll()
        {
            string rsp_path = null;
            //change to decision based on .net version
            string filename = "mcs";
            if (Helper.compareVersions("2018", Application.unityVersion) == 1)
                filename = "csc";

            bool rsp_good = false;
            if (DoesRSPExisit(filename, ref rsp_path))
            {
                if (ISRSPAtCorrectPath(filename,rsp_path))
                {
                    if (DoesRSPContainDrawingDLL(rsp_path))
                        rsp_good = true;
                    else
                        AddDrawingDLLToRSP(rsp_path);
                }else
                    AssetDatabase.MoveAsset(rsp_path, PATH.RSP_NEEDED_PATH + filename + ".rsp");
            }else
                AddDrawingDLLToRSP(PATH.RSP_NEEDED_PATH + filename + ".rsp");

            Helper.SetDefineSymbol(DEFINE_SYMBOLS.IMAGING_EXISTS, rsp_good, true);
        }

        private static bool DoesRSPExisit(string rsp_name,ref string rsp_path)
        {
            foreach (string id in AssetDatabase.FindAssets(rsp_name))
            {
                string path = AssetDatabase.GUIDToAssetPath(id);
                if (path.Contains(rsp_name + ".rsp"))
                {
                    rsp_path = path;
                    return true;
                }
            }
            return false;
        }

        private static bool ISRSPAtCorrectPath(string rsp_name, string rsp_path)
        {
            return rsp_path.Contains(PATH.RSP_NEEDED_PATH + rsp_name + ".rsp");
        }

        private static bool DoesRSPContainDrawingDLL(string rsp_path)
        {
            string rsp_data = Helper.ReadFileIntoString(rsp_path);
            return (Regex.Match(rsp_data, RSP_DRAWING_DLL_REGEX).Success);
        }

        private static void AddDrawingDLLToRSP(string rsp_path)
        {
            string rsp_data = Helper.ReadFileIntoString(rsp_path);
            rsp_data += RSP_DRAWING_DLL_CODE;
            Helper.WriteStringToFile(rsp_data,rsp_path);
        }

        //------------------Helpers----------------------------

        public static void setActiveShader(Shader shader)
        {
            if (shader != activeShader)
            {
                activeShader = shader;
                presetHandler = new PresetHandler(shader);
                activeShaderMaterial = new Material(shader);
            }
        }

        public static Settings getInstance()
        {
            Settings instance = (Settings)Helper.FindEditorWindow(typeof(Settings));
            if (instance == null) instance = ScriptableObject.CreateInstance<Settings>();
            return instance;
        }

        //------------------Main GUI
        void OnGUI()
        {
            if (!is_init || moduleSettings==null) InitVariables();
            GUILayout.Label("ThryEditor v" + Config.Get().verion);

            GUINotification();
            drawLine();
            GUIMessage();
            GUIVRC();
            GUIEditor();
            drawLine();
            GUIExtras();
            drawLine();
            GUIShareData();
            drawLine();
            foreach(ModuleSettings s in moduleSettings)
            {
                s.Draw();
                drawLine();
            }
            GUIModulesInstalation();
        }

        //--------------------------GUI Helpers-----------------------------

        private static void drawLine()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
        }

        private void GUINotification()
        {
            if (isFirstPopop)
                GUILayout.Label(" Thry Shader Editor successfully installed. This is the editor settings window.", Styles.Get().greenStyle);
            else if (updatedVersion == -1)
                GUILayout.Label(" Thry editor has been updated", Styles.Get().greenStyle);
            else if (updatedVersion == 1)
                GUILayout.Label(" Warning: The Version of Thry Editor has declined", Styles.Get().yellowStyle);
        }

        private void GUIMessage()
        {
            if(thry_message!=null && thry_message.text.Length > 0)
            {
                GUIStyle style = new GUIStyle();
                style.richText = true;
                style.margin = new RectOffset(7, 0, 0, 0);
                style.wordWrap = true;
                GUILayout.Label(new GUIContent(thry_message.text,thry_message.hover), style);
                Rect r = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
                    thry_message.action.Perform();
                drawLine();
            }
        }

        private void GUIVRC()
        {
            if (VRCInterface.Get().sdk_is_installed)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("VRC Sdk version: " + VRCInterface.Get().installed_sdk_version + (VRCInterface.Get().sdk_is_up_to_date ? " (newest version)" : ""));
                RemoveVRCSDKButton();
                GUILayout.EndHorizontal();
                if (!VRCInterface.Get().sdk_is_up_to_date)
                {
                    GUILayout.Label("Newest VRC SDK version: " + VRCInterface.Get().newest_sdk_version);
                    UpdateVRCSDKButton();
                }
                if (VRCInterface.Get().user_logged_in)
                {
                    GUILayout.Label("VRChat user: " + EditorPrefs.GetString("sdk#username"));
                }
            }
            else
            {
                InstallVRCSDKButton();
            }
            drawLine();
        }

        private void InstallVRCSDKButton()
        {
            EditorGUI.BeginDisabledGroup(is_changing_vrc_sdk);
            if (GUILayout.Button("Install VRC SDK"))
            {
                is_changing_vrc_sdk = true;
                VRCInterface.DownloadAndInstallVRCSDK();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void RemoveVRCSDKButton()
        {
            EditorGUI.BeginDisabledGroup(is_changing_vrc_sdk);
            if (GUILayout.Button("Remove VRC SDK", GUILayout.ExpandWidth(false)))
            {
                is_changing_vrc_sdk = true;
                VRCInterface.Get().RemoveVRCSDK(true);
            }
            EditorGUI.EndDisabledGroup();
        }

        private void UpdateVRCSDKButton()
        {
            EditorGUI.BeginDisabledGroup(is_changing_vrc_sdk);
            if (GUILayout.Button("Update VRC SDK"))
            {
                is_changing_vrc_sdk = true;
                VRCInterface.Get().UpdateVRCSDK();
            }
            EditorGUI.EndDisabledGroup();
        }

        bool is_editor_expanded = true;
        private void GUIEditor()
        {
            is_editor_expanded = Foldout("Editor", is_editor_expanded);
            if (is_editor_expanded)
            {
                EditorGUI.indentLevel += 2;
                Dropdown("default_texture_type", SETTINGS_CONTENT[(int)SETTINGS_IDX.default_texture_type]);
                Toggle("showRenderQueue", SETTINGS_CONTENT[(int)SETTINGS_IDX.render_queue]);
                if (Config.Get().showRenderQueue)
                    Toggle("renderQueueShaders", SETTINGS_CONTENT[(int)SETTINGS_IDX.render_queue_shaders]);
                GUIGradients();
                EditorGUI.indentLevel -= 2;
            }
        }

        private static void GUIGradients()
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            Text("gradient_name", SETTINGS_CONTENT[(int)SETTINGS_IDX.gradient_file_name], false);
            string gradient_name = Config.Get().gradient_name;
            if (gradient_name.Contains("<hash>"))
                GUILayout.Label("Good naming.", Styles.Get().greenStyle, GUILayout.ExpandWidth(false));
            else if (gradient_name.Contains("<material>"))
                if (gradient_name.Contains("<prop>"))
                    GUILayout.Label("Good naming.", Styles.Get().greenStyle, GUILayout.ExpandWidth(false));
                else
                    GUILayout.Label("Consider adding <hash> or <prop>.", Styles.Get().yellowStyle, GUILayout.ExpandWidth(false));
            else if (gradient_name.Contains("<prop>"))
                GUILayout.Label("Consider adding <material>.", Styles.Get().yellowStyle, GUILayout.ExpandWidth(false));
            else
                GUILayout.Label("Add <material> <hash> or <prop> to destingish between gradients.", Styles.Get().redStyle, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }

        bool is_extras_expanded = false;
        private void GUIExtras()
        {
            is_extras_expanded = Foldout("Extras", is_extras_expanded);
            if (is_extras_expanded)
            {
                EditorGUI.indentLevel += 2;
                Toggle("showImportPopup", SETTINGS_CONTENT[(int)SETTINGS_IDX.show_popup_on_import]);
                EditorGUI.indentLevel -= 2;
            }
        }

        bool is_data_share_expanded = false;
        private void GUIShareData()
        {
            is_data_share_expanded = Foldout("User Data Collection", is_data_share_expanded);
            if (is_data_share_expanded)
            {
                EditorGUI.indentLevel += 2;
                Toggle("share_user_data", "Share Anonomyous Data for usage statistics", "", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("The data is identified by a hash of your macaddress. This is to make sure we don't log any user twice, while still keeping all data anonymous.");
                if (Config.Get().share_user_data)
                {
                    Toggle("share_installed_unity_version", "Share my installed Unity Version", "");
                    Toggle("share_installed_editor_version", "Share my installed Thry Editor Version", "");
                    Toggle("share_used_shaders", "Share the names of installed shaders using thry editor", "");
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(EditorGUI.indentLevel * 15);
                    if (GUILayout.Button("Show all data collected about me", GUILayout.ExpandWidth(false)))
                    {
                        Helper.DownloadStringASync(URL.DATA_SHARE_GET_MY_DATA+"?hash="+Helper.GetMacAddress().GetHashCode(), delegate(string s){
                            TextPopup popup = ScriptableObject.CreateInstance<TextPopup>();
                            popup.position = new Rect(Screen.width / 2, Screen.height / 2, 512, 480);
                            popup.titleContent = new GUIContent("Your Data");
                            popup.text = s;
                            popup.ShowUtility();
                        });
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel -= 2;
            }
        }

        private class TextPopup : EditorWindow
        {
            public string text = "";
            private Vector2 scroll;
            void OnGUI()
            {
                EditorGUILayout.SelectableLabel("This is all data collected on your hashed mac address: ", EditorStyles.boldLabel);
                Rect last = GUILayoutUtility.GetLastRect();
                
                Rect data_rect = new Rect(0, last.height, Screen.width, Screen.height - last.height);
                scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Width(data_rect.width), GUILayout.Height(data_rect.height));
                GUILayout.TextArea(text);
                EditorGUILayout.EndScrollView();
            }
        }

        private void GUIModulesInstalation()
        {
            if (ModuleHandler.GetModules() == null)
                return;
            if (ModuleHandler.GetModules().Count > 0)
                GUILayout.Label("Extra Modules", EditorStyles.boldLabel);
            bool disabled = false;
            foreach (ModuleHeader module in ModuleHandler.GetModules())
                if (module.is_being_installed_or_removed)
                    disabled = true;
            EditorGUI.BeginDisabledGroup(disabled);
            foreach (ModuleHeader module in ModuleHandler.GetModules())
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(!module.available_requirement_fullfilled);
                EditorGUI.BeginChangeCheck();
                bool is_installed = Helper.ClassExists(module.available_module.classname);
                bool update_available = is_installed;
                if (module.installed_module != null)
                    update_available = Helper.compareVersions(module.installed_module.version, module.available_module.version) == 1;
                string displayName = module.available_module.name;
                if (module.installed_module != null)
                    displayName += " v" + module.installed_module.version;

                bool install = GUILayout.Toggle(is_installed, new GUIContent(displayName, module.available_module.description), GUILayout.ExpandWidth(false));
                if (EditorGUI.EndChangeCheck())
                    ModuleHandler.InstallRemoveModule(module,install);
                if(update_available)
                    if (GUILayout.Button("update to v"+module.available_module.version, GUILayout.ExpandWidth(false)))
                        ModuleHandler.UpdateModule(module);
                EditorGUI.EndDisabledGroup();
                if (module.available_module.requirement != null && (update_available || !is_installed))
                {
                    if(module.available_requirement_fullfilled)
                        GUILayout.Label("Requirements: " + module.available_module.requirement.ToString(), Styles.Get().greenStyle);
                    else
                        GUILayout.Label("Requirements: " + module.available_module.requirement.ToString(), Styles.Get().redStyle);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.EndDisabledGroup();
        }

        private static void Text(string configField, string[] content)
        {
            Text(configField, content, true);
        }

        private static void Text(string configField, string[] content, bool createHorizontal)
        {
            Config config = Config.Get();
            System.Reflection.FieldInfo field = typeof(Config).GetField(configField);
            if (field != null)
            {
                string value = (string)field.GetValue(config);
                if (createHorizontal)
                    GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
                GUILayout.Space(57);
                GUILayout.Label(new GUIContent(content[0], content[1]), GUILayout.ExpandWidth(false));
                EditorGUI.BeginChangeCheck();
                value = EditorGUILayout.DelayedTextField("", value, GUILayout.ExpandWidth(false));
                if (EditorGUI.EndChangeCheck())
                {
                    field.SetValue(config, value);
                    config.save();
                }
                if (createHorizontal)
                    GUILayout.EndHorizontal();
            }
        }

        private static void Toggle(string configField, string[] content, GUIStyle label_style = null)
        {
            Toggle(configField, content[0], content[1], label_style);
        }

        private static void Toggle(string configField, string label, string hover, GUIStyle label_style = null)
        {
            Config config = Config.Get();
            System.Reflection.FieldInfo field = typeof(Config).GetField(configField);
            if (field != null)
            {
                bool value = (bool)field.GetValue(config);
                if (Toggle(value, label, hover, label_style) != value)
                {
                    field.SetValue(config, !value);
                    config.save();
                    ThryEditor.repaint();
                }
            }
        }

        private static void Dropdown(string configField, string[] content)
        {
            Dropdown(configField, content[0], content[1]);
        }

        private static void Dropdown(string configField, string label, string hover, GUIStyle label_style = null)
        {
            Config config = Config.Get();
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
                    config.save();
                    ThryEditor.repaint();
                }
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
            var rect = GUILayoutUtility.GetRect(16f + 20f, 22f, Styles.Get().dropDownHeader);
            GUI.Box(rect, content, Styles.Get().dropDownHeader);
            var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            Event e = Event.current;
            if (e.type == EventType.Repaint)
                EditorStyles.foldout.Draw(toggleRect, false, false, expanded, false);
            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition) && !e.alt)
            {
                expanded = !expanded;
                e.Use();
            }
            return expanded;
        }
    }
}