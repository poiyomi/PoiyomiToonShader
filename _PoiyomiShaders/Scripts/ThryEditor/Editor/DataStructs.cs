// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    #region Constants
    public class PATH
    {
        public const string TEXTURES_DIR = "Assets/textures";
        public const string RSP_NEEDED_PATH = "Assets/";

        public const string DELETING_DIR = "Thry/trash";

        public const string PERSISTENT_DATA = "Thry/persistent_data";

        public const string GRADIENT_INFO_FILE = "Thry/gradients";

        public const string LINKED_MATERIALS_FILE = "Thry/linked_materials.json";
    }

    public class URL
    {
        public const string MODULE_COLLECTION = "https://raw.githubusercontent.com/Thryrallo/ThryEditorStreamedResources/main/packages.json";
        public const string SETTINGS_MESSAGE_URL = "https://raw.githubusercontent.com/Thryrallo/ThryEditorStreamedResources/main/Messages/settingsWindow.json";
        public const string COUNT_PROJECT = "http://thryeditor.thryrallo.de/count_project.php";
        public const string COUNT_USER = "http://thryeditor.thryrallo.de/count_user.php";
    }

    public class DEFINE_SYMBOLS
    {
        public const string IMAGING_EXISTS = "IMAGING_DLL_EXISTS";
    }

    public class RESOURCE_GUID
    {
        public const string RECT = "2329f8696fd09a743a5baf2a5f4986af";
        public const string ICON_LINK = "e85fd0a0e4e4fea46bb3fdeab5c3fb07";
        public const string ICON_THRY = "693aa4c2cdc578346a196469a06ddbba";
    }
    #endregion

    public class DrawingData
    {
        public static TextureProperty CurrentTextureProperty;
        public static Rect LastGuiObjectRect;
        public static Rect LastGuiObjectHeaderRect;
        public static Rect TooltipCheckRect;
        public static float IconsPositioningHeight;
        public static bool LastPropertyUsedCustomDrawer;
        public static bool LastPropertyDoesntAllowAnimation;
        public static DrawerType LastPropertyDrawerType;
        public static MaterialPropertyDrawer LastPropertyDrawer;
        public static List<MaterialPropertyDrawer> LastPropertyDecorators = new List<MaterialPropertyDrawer>();
        public static bool IsEnabled = true;
        public static bool IsCollectingProperties = false;

        public static ShaderPart LastInitiatedPart;

        public static void ResetLastDrawerData()
        {
            LastPropertyUsedCustomDrawer = false;
            LastPropertyDoesntAllowAnimation = false;
            LastPropertyDrawer = null;
            LastPropertyDrawerType = DrawerType.None;
            LastPropertyDecorators.Clear();
        }

        public static void RegisterDecorator(MaterialPropertyDrawer drawer)
        {
            if(IsCollectingProperties)
            {
                LastPropertyDecorators.Add(drawer);
            }
        }
    }

    public enum DrawerType
    {
        None
    }

    public class GradientData
    {
        public Texture PreviewTexture;
        public Gradient Gradient;
    }

    public enum TextureDisplayType
    {
        small, big, big_basic
    }

    //--------------Shader Data Structs--------------------

    #region In Shader Data
    public class PropertyOptions
    {
        public int offset = 0;
        public string tooltip = "";
        public DefineableAction altClick;
        public DefineableAction onClick;
        public DefineableCondition condition_show = new DefineableCondition();
        public string condition_showS;
        public DefineableCondition condition_enable = null;
        public DefineableCondition condition_enable_children = null;
        public PropertyValueAction[] on_value_actions;
        public string on_value;
        public DefineableAction[] actions;
        public ButtonData button_help;
        public TextureData texture;
        public string[] reference_properties;
        public string reference_property;
        public bool force_texture_options = false;
        public bool is_visible_simple = false;
        public string file_name;
        public string remote_version_url;
        public string generic_string;
        public bool never_lock;
        public float margin_top = 0;
        public string[] alts;
        public bool persistent_expand = true;
        public bool default_expand = false;
        public bool ref_float_toggles_expand = true;

        public static PropertyOptions Deserialize(string s)
        {
            if(s == null) return new PropertyOptions();
            s = s.Replace("''", "\"");
            PropertyOptions options = Parser.Deserialize<PropertyOptions>(s);
            if (options == null) return new PropertyOptions();
            // The following could be removed since the parser can now handle it. leaving it in for now /shrug
            if (options.condition_showS != null)
            {
                options.condition_show = DefineableCondition.Parse(options.condition_showS);
            }
            if (options.on_value != null)
            {
                options.on_value_actions = PropertyValueAction.ParseToArray(options.on_value);
            }
            return options;
        }
    }

    public class ButtonData
    {
        public string text = "";
        public TextureData texture = null;
        public DefineableAction action = new DefineableAction();
        public string hover = "";
        public bool center_position = false;
        public DefineableCondition condition_show = new DefineableCondition();
    }

    public class TextureData
    {
        public string name = null;
        public string guid = null;
        public int width = 128;
        public int height = 128;

        public char channel = 'r';

        public int ansioLevel = 1;
        public FilterMode filterMode = FilterMode.Bilinear;
        public TextureWrapMode wrapMode = TextureWrapMode.Repeat;
        public bool center_position = false;
        bool _isLoading;

        public void ApplyModes(Texture texture)
        {
            texture.filterMode = filterMode;
            texture.wrapMode = wrapMode;
            texture.anisoLevel = ansioLevel;
        }
        public void ApplyModes(string path)
        {
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.filterMode = filterMode;
            importer.wrapMode = wrapMode;
            importer.anisoLevel = ansioLevel;
            importer.SaveAndReimport();
        }

        static Dictionary<string, Texture> s_loaded_textures = new Dictionary<string, Texture>();
        public Texture loaded_texture
        {
            get
            {
                if(guid != null)
                {
                    if(!s_loaded_textures.ContainsKey(guid) || s_loaded_textures[guid] == null)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        if (path != null)
                            s_loaded_textures[guid] = AssetDatabase.LoadAssetAtPath<Texture>(path);
                        else
                            s_loaded_textures[guid] = Texture2D.whiteTexture;
                    }
                    return s_loaded_textures[guid];
                }else if(name != null)
                {
                    if(!s_loaded_textures.ContainsKey(name) || s_loaded_textures[name] == null)
                    {
                        // Retrieve downloaded image from sessionstate (base64 encoded)
                        if(SessionState.GetString(name, "") != "")
                        {
                            s_loaded_textures[name] = new Texture2D(1,1, TextureFormat.ARGB32, false);
                            ImageConversion.LoadImage((Texture2D)s_loaded_textures[name], Convert.FromBase64String(SessionState.GetString(name, "")), false);
                            return s_loaded_textures[name];
                        }

                        if(IsUrl())
                        {
                            if(!_isLoading)
                            {
                                s_loaded_textures[name] = Texture2D.whiteTexture;
                                WebHelper.DownloadBytesASync(name, (byte[] b) =>
                                {
                                    _isLoading = false;
                                    Texture2D tex = new Texture2D(1,1, TextureFormat.ARGB32, false);
                                    ImageConversion.LoadImage(tex, b, false);
                                    s_loaded_textures[name] = tex;
                                    SessionState.SetString(name, Convert.ToBase64String(((Texture2D)s_loaded_textures[name]).EncodeToPNG()));
                                });
                                _isLoading = true;
                            }
                        }else
                        {
                            string path = FileHelper.FindFile(name, "texture");
                            if (path != null)
                                s_loaded_textures[name] = AssetDatabase.LoadAssetAtPath<Texture>(path);
                            else
                                s_loaded_textures[name] = Texture2D.whiteTexture;
                        }
                    }
                    return s_loaded_textures[name];
                }
                return Texture2D.whiteTexture;
            }
        }

        private static TextureData ParseForThryParser(string s)
        {
            if(s.StartsWith("{") == false)
            {
                return new TextureData()
                {
                    name = s
                };
            }
            return Parser.Deserialize<TextureData>(s);
        }

        bool IsUrl()
        {
            return name.StartsWith("http") && (name.EndsWith(".jpg") || name.EndsWith(".png"));
        }
    }

    public class PropertyValueAction
    {
        public string value;
        public DefineableAction[] actions;

        public bool Execute(MaterialProperty p, Material[] targets)
        {
            if(
                (p.type == MaterialProperty.PropType.Float   && p.floatValue.ToString()   ==  value)          ||
#if UNITY_2022_1_OR_NEWER
                (p.type == MaterialProperty.PropType.Int     && p.intValue.ToString()     ==  value)          ||
#endif
                (p.type == MaterialProperty.PropType.Range   && p.floatValue.ToString()   ==  value)          ||
                (p.type == MaterialProperty.PropType.Color   && p.colorValue.ToString()   ==  value)          ||
                (p.type == MaterialProperty.PropType.Vector  && p.vectorValue.ToString()  ==  value)          ||
                (p.type == MaterialProperty.PropType.Texture && ((p.textureValue == null) == (value == "0"))) ||
                (p.type == MaterialProperty.PropType.Texture && ((p.textureValue != null) == (value == "1"))) ||
                (p.type == MaterialProperty.PropType.Texture && (p.textureValue != null && p.textureValue.name == value)) 
            )
                {;
                foreach (DefineableAction a in actions)
                    a.Perform(targets);
                return true;
            }
            return false;
        }
        
        private static PropertyValueAction ParseForThryParser(string s)
        {
            return Parse(s);
        }

        // value,property1=value1,property2=value2
        public static PropertyValueAction Parse(string s)
        {
            s = s.Trim();
            string[] valueAndActions = s.Split(new string[]{"=>"}, System.StringSplitOptions.RemoveEmptyEntries);
            if (valueAndActions.Length > 1)
            {
                PropertyValueAction propaction = new PropertyValueAction();
                propaction.value = valueAndActions[0];
                List<DefineableAction> actions = new List<DefineableAction>();
                string[] actionStrings = valueAndActions[1].Split(';');
                for (int i = 0; i < actionStrings.Length; i++)
                {
                    if(string.IsNullOrWhiteSpace(actionStrings[i]))
                        continue;
                    actions.Add(DefineableAction.Parse(actionStrings[i]));
                }
                propaction.actions = actions.ToArray();
                return propaction;
            }
            return null;
        }

        private static PropertyValueAction[] ParseToArrayForThryParser(string s)
        {
            return ParseToArray(s);
        }

        public static PropertyValueAction[] ParseToArray(string s)
        {
            //s := 0=>p1=v1;p2=v2;1=>p1=v3...
            List<PropertyValueAction> propactions = new List<PropertyValueAction>();
            string[] valueAndActionMatches = Regex.Matches(s, @"[^;]+=>.+?(?=(;[^;]+=>)|$)", RegexOptions.Multiline).Cast<Match>().Select(m => m.Value).ToArray();
            foreach (string p in valueAndActionMatches)
            {
                PropertyValueAction propertyValueAction = PropertyValueAction.Parse(p);
                if (propertyValueAction != null)
                    propactions.Add(propertyValueAction);
            }
            return propactions.ToArray();
        }
    }

    public class DefineableAction
    {
        public DefineableActionType type = DefineableActionType.NONE;
        public string data = "";
        public void Perform(Material[] targets)
        {
            switch (type)
            {
                case DefineableActionType.URL:
                    Application.OpenURL(data);
                    break;
                case DefineableActionType.SET_PROPERTY:
                    string[] set = Regex.Split(data, @"=");
                    if (set.Length > 1)
                        MaterialHelper.SetMaterialValue(set[0].Trim(), set[1].Trim());
                    break;
                case DefineableActionType.SET_TAG:
                    string[] keyValue = Regex.Split(data, @"=");
                    foreach (Material m in targets)
                        m.SetOverrideTag(keyValue[0].Trim(), keyValue[1].Trim());
                    break;
                case DefineableActionType.SET_SHADER:
                    Shader shader = Shader.Find(data);
                    if (shader != null)
                    {
                        foreach (Material m in targets)
                            m.shader = shader;
                    }
                    break;
                case DefineableActionType.OPEN_EDITOR:
                    System.Type t = Helper.FindTypeByFullName(data);
                    if (t != null)
                    {
                        try
                        {
                            EditorWindow window = EditorWindow.GetWindow(t);
                            window.titleContent = new GUIContent("TPS Setup Wizard");
                            window.Show();
                        }catch(System.Exception e)
                        {
                            Debug.LogError("[Thry] Couldn't open Editor Window of type" + data);
                            Debug.LogException(e);
                        }
                    }
                    break;
            }
        }

        private static DefineableAction ParseForThryParser(string s)
        {
            return Parse(s);
        }
        public static DefineableAction Parse(string s)
        {
            s = s.Trim();
            DefineableAction action = new DefineableAction();
            if (s.StartsWith("http", StringComparison.Ordinal) || s.StartsWith("www", StringComparison.Ordinal))
            {
                action.type = DefineableActionType.URL;
                action.data = s;
            }
            else if (s.StartsWith("tag::", StringComparison.Ordinal))
            {
                action.type = DefineableActionType.SET_TAG;
                action.data = s.Replace("tag::", "");
            }
            else if (s.StartsWith("shader=", StringComparison.Ordinal))
            {
                action.type = DefineableActionType.SET_SHADER;
                action.data = s.Replace("shader=", "");
            }
            else if (s.Contains("="))
            {
                action.type = DefineableActionType.SET_PROPERTY;
                action.data = s;
            }
            else if (Regex.IsMatch(s, @"\w+(\.\w+)"))
            {
                action.type = DefineableActionType.OPEN_EDITOR;
                action.data = s;
            }
            return action;
        }

        public static DefineableAction ParseDrawerParameter(string s)
        {
            s = s.Trim();
            DefineableAction action = new DefineableAction();
            if (s.StartsWith("youtube#", StringComparison.Ordinal))
            {
                action.type = DefineableActionType.URL;
                action.data = "https://www.youtube.com/watch?v="+s.Substring(8);
            }
            return action;
        }

        public override string ToString()
        {
            return $"{{{type},{data}}}";
        }
    }

    public enum DefineableActionType
    {
        NONE,
        URL,
        SET_PROPERTY,
        SET_SHADER,
        SET_TAG,
        OPEN_EDITOR,
    }

    public class DefineableCondition
    {
        public DefineableConditionType type = DefineableConditionType.NONE;
        public string data = "";
        public DefineableCondition condition1;
        public DefineableCondition condition2;

        CompareType _compareType;
        string _obj;
        ShaderProperty _propertyObj;
        Material _materialInsteadOfEditor;

        string _value;
        float _floatValue;

        bool _hasConstantValue;
        bool _constantValue;

        bool _isInit = false;
        public void Init()
        {
            if (_isInit) return;
            _hasConstantValue = true;
            if (type == DefineableConditionType.NONE) { _constantValue = true; }
            else if (type == DefineableConditionType.TRUE) { _constantValue = true; }
            else if (type == DefineableConditionType.FALSE) { _constantValue = false; }
            else
            {
                var (compareType, compareString) = GetComparetor();
                _compareType = compareType;

                string[] parts = Regex.Split(data, compareString);
                _obj = parts[0].Trim();
                _value = parts[parts.Length - 1].Trim();

                _floatValue = Parser.ParseFloat(_value);
                if (ShaderEditor.Active != null && ShaderEditor.Active.PropertyDictionary.ContainsKey(_obj))
                    _propertyObj = ShaderEditor.Active.PropertyDictionary[_obj];

                if (type == DefineableConditionType.EDITOR_VERSION) InitEditorVersion();
                else if (type == DefineableConditionType.VRC_SDK_VERSION) InitVRCSDKVersion();
                else _hasConstantValue = false;
            }
            
            _isInit = true;
        }

        void InitEditorVersion()
        {
            int c_ev = Helper.CompareVersions(Config.Singleton.verion, _value);
            if (_compareType == CompareType.EQUAL) _constantValue = c_ev == 0;
            if (_compareType == CompareType.NOT_EQUAL) _constantValue = c_ev != 0;
            if (_compareType == CompareType.SMALLER) _constantValue = c_ev == 1;
            if (_compareType == CompareType.BIGGER) _constantValue = c_ev == -1;
            if (_compareType == CompareType.BIGGER_EQ) _constantValue = c_ev == -1 || c_ev == 0;
            if (_compareType == CompareType.SMALLER_EQ) _constantValue = c_ev == 1 || c_ev == 0;
        }

        void InitVRCSDKVersion()
        {
            if (VRCInterface.Get().Sdk_information.type == VRCInterface.VRC_SDK_Type.NONE)
            {
                _constantValue = false;
                return;
            }
            int c_vrc = Helper.CompareVersions(VRCInterface.Get().Sdk_information.installed_version, _value);
            if (_compareType == CompareType.EQUAL) _constantValue = c_vrc == 0;
            if (_compareType == CompareType.NOT_EQUAL) _constantValue = c_vrc != 0;
            if (_compareType == CompareType.SMALLER) _constantValue = c_vrc == 1;
            if (_compareType == CompareType.BIGGER) _constantValue = c_vrc == -1;
            if (_compareType == CompareType.BIGGER_EQ) _constantValue = c_vrc == -1 || c_vrc == 0;
            if (_compareType == CompareType.SMALLER_EQ) _constantValue = c_vrc == 1 || c_vrc == 0;
        }

        public bool Test()
        {
            Init();
            if (_hasConstantValue) return _constantValue;
            
            MaterialProperty materialProperty = null;
            switch (type)
            {
                case DefineableConditionType.PROPERTY_BOOL:
                    materialProperty = GetMaterialProperty();
                    if (materialProperty == null) return false;
                    if (_compareType == CompareType.NONE) return materialProperty.GetNumber() == 1;
                    if (_compareType == CompareType.EQUAL) return materialProperty.GetNumber() == _floatValue;
                    if (_compareType == CompareType.NOT_EQUAL) return materialProperty.GetNumber() != _floatValue;
                    if (_compareType == CompareType.SMALLER) return materialProperty.GetNumber() < _floatValue;
                    if (_compareType == CompareType.BIGGER) return materialProperty.GetNumber() > _floatValue;
                    if (_compareType == CompareType.BIGGER_EQ) return materialProperty.GetNumber() >= _floatValue;
                    if (_compareType == CompareType.SMALLER_EQ) return materialProperty.GetNumber() <= _floatValue;
                    break;
                case DefineableConditionType.TEXTURE_SET:
                    materialProperty = GetMaterialProperty();
                    if (materialProperty == null) return false;
                    return (materialProperty.textureValue == null) == (_compareType == CompareType.EQUAL);
                case DefineableConditionType.DROPDOWN:
                    materialProperty = GetMaterialProperty();
                    if (materialProperty == null) return false;
                    if (_compareType == CompareType.NONE) return materialProperty.GetNumber() == 1;
                    if (_compareType == CompareType.EQUAL) return "" + materialProperty.GetNumber() == _value;
                    if (_compareType == CompareType.NOT_EQUAL) return "" + materialProperty.GetNumber() != _value;
                    break;
                case DefineableConditionType.PROPERTY_IS_ANIMATED:
                    return ShaderOptimizer.IsAnimated(_materialInsteadOfEditor, _obj);
                case DefineableConditionType.PROPERTY_IS_NOT_ANIMATED:
                    return !ShaderOptimizer.IsAnimated(_materialInsteadOfEditor, _obj);
                case DefineableConditionType.AND:
                    if(condition1!=null&&condition2!=null) return condition1.Test() && condition2.Test();
                    break;
                case DefineableConditionType.OR:
                    if(condition1 != null && condition2 != null) return condition1.Test() || condition2.Test();
                    break;
                case DefineableConditionType.NOT:
                    if(condition1 != null) return !condition1.Test();
                    break;
            }
            
            return true;
        }

        private MaterialProperty GetMaterialProperty()
        {
            if(_materialInsteadOfEditor) return MaterialEditor.GetMaterialProperty(new Material[]{_materialInsteadOfEditor}, _obj);
            if(_propertyObj != null) return _propertyObj.MaterialProperty;
            return null;
        }
        private (CompareType,string) GetComparetor()
        {
            if (data.Contains("=="))
                return (CompareType.EQUAL,"==");
            if (data.Contains("!="))
                return (CompareType.NOT_EQUAL,"!=");
            if (data.Contains(">="))
                return (CompareType.BIGGER_EQ,">=");
            if (data.Contains("<="))
                return (CompareType.SMALLER_EQ,"<=");
            if (data.Contains(">"))
                return (CompareType.BIGGER,">");
            if (data.Contains("<"))
                return (CompareType.SMALLER,"<");
            return (CompareType.NONE,"##");
        }

        public override string ToString()
        {
            switch (type)
            {
                case DefineableConditionType.PROPERTY_BOOL:
                    return data;
                case DefineableConditionType.EDITOR_VERSION:
                    return "EDITOR_VERSION" + data;
                case DefineableConditionType.VRC_SDK_VERSION:
                    return "VRC_SDK_VERSION" + data;
                case DefineableConditionType.TEXTURE_SET:
                    return "TEXTURE_SET" + data;
                case DefineableConditionType.DROPDOWN:
                    return "DROPDOWN" + data;
                case DefineableConditionType.PROPERTY_IS_ANIMATED:
                    return $"isAnimated({data})";
                case DefineableConditionType.PROPERTY_IS_NOT_ANIMATED:
                    return $"isNotAnimated({data})";
                case DefineableConditionType.AND:
                    if (condition1 != null && condition2 != null) return "("+condition1.ToString() + "&&" + condition2.ToString()+")";
                    break;
                case DefineableConditionType.OR:
                    if (condition1 != null && condition2 != null) return "("+condition1.ToString()+"||"+condition2.ToString()+")";
                    break;
                case DefineableConditionType.NOT:
                    if (condition1 != null) return "!"+condition1.ToString();
                    break;
            }
            return "";
        }

        private static DefineableCondition ParseForThryParser(string s)
        {
            return Parse(s);
        }

        private static readonly char[] ComparissionLiteralsToCheckFor = "*><=".ToCharArray();
        public static DefineableCondition Parse(string s, Material useThisMaterialInsteadOfOpenEditor = null, int start = 0, int end = -1)
        {
            if(end == -1) end = s.Length;
            DefineableCondition con;

            // Debug.Log("Parsing: " + s.Substring(start, end - start));

            int depth = 0;
            int bracketStart = -1;
            int bracketEnd = -1;
            for(int i = start; i < end; i++)
            {
                char c = s[i];
                if(c == '(')
                {
                    depth += 1;
                    if(depth == 1)
                    {
                        bracketStart = i;
                    }
                }else if(c == ')')
                {
                    if(depth == 1)
                    {
                        bracketEnd = i;
                    }
                    depth -= 1;
                }else if(depth == 0)
                {
                    if(c == '&')
                    {
                        con = new DefineableCondition();
                        con._materialInsteadOfEditor = useThisMaterialInsteadOfOpenEditor;

                        con.type = DefineableConditionType.AND;
                        con.condition1 = Parse(s, useThisMaterialInsteadOfOpenEditor, start, i);
                        con.condition2 = Parse(s, useThisMaterialInsteadOfOpenEditor, i + (s[i+1] == '&' ? 2 : 1), end);
                        return con;
                    }else if(c == '|')
                    {
                        
                        con = new DefineableCondition();
                        con._materialInsteadOfEditor = useThisMaterialInsteadOfOpenEditor;

                        con.type = DefineableConditionType.OR;
                        con.condition1 = Parse(s, useThisMaterialInsteadOfOpenEditor, start, i);
                        con.condition2 = Parse(s, useThisMaterialInsteadOfOpenEditor, i + (s[i + 1] == '|' ? 2 : 1), end);
                        return con;
                    }
                }
            }


            bool isInverted = IsInverted(s, ref start);

            // if no AND or OR was found, check for brackets
            if(bracketStart != -1 && bracketEnd != -1)
            {
                con = Parse(s, useThisMaterialInsteadOfOpenEditor, bracketStart + 1, bracketEnd);
            }else
            {
                con = ParseSingle(s.Substring(start, end - start), useThisMaterialInsteadOfOpenEditor);
            }

            if(isInverted)
            {
                DefineableCondition inverted = new DefineableCondition();
                inverted._materialInsteadOfEditor = useThisMaterialInsteadOfOpenEditor;
                inverted.type = DefineableConditionType.NOT;
                inverted.condition1 = con;
                return inverted;
            }

            return con;
        }

        static bool IsInverted(string s, ref int start)
        {
            for(int i = start; i < s.Length; i++)
            {
                if(s[i] == '!')
                {
                    start += 1;
                    return true;
                }
                if(s[i] != ' ')
                    return false;
            }
            return false;
        }

        static DefineableCondition ParseSingle(string s, Material useThisMaterialInsteadOfOpenEditor = null)
        {
            // Debug.Log("Parsing single: " + s);

            DefineableCondition con = new DefineableCondition
            {
                _materialInsteadOfEditor = useThisMaterialInsteadOfOpenEditor
            };

            if (s.IndexOfAny(ComparissionLiteralsToCheckFor) != -1)
            {
                //is a comparission
                con.data = s;
                con.type = DefineableConditionType.PROPERTY_BOOL;
                if (s.StartsWith("VRCSDK", StringComparison.Ordinal))
                {
                    con.type = DefineableConditionType.VRC_SDK_VERSION;
                    con.data = s.Replace("VRCSDK", "");
                }else if (s.StartsWith("ThryEditor", StringComparison.Ordinal))
                {
                    con.type = DefineableConditionType.EDITOR_VERSION;
                    con.data = s.Replace("ThryEditor", "");
                }else if(IsTextureNullComparission(s, useThisMaterialInsteadOfOpenEditor))
                {
                    con.type = DefineableConditionType.TEXTURE_SET;
                    con.data = s.Replace("TEXTURE_SET", "");
                }
                return con;
            }
            if(s.StartsWith("isNotAnimated(", StringComparison.Ordinal))
            {
                con.type = DefineableConditionType.PROPERTY_IS_NOT_ANIMATED;
                con.data = s.Replace("isNotAnimated(", "").TrimEnd(')');
                return con;
            }
            if(s.StartsWith("isAnimated(", StringComparison.Ordinal))
            {
                con.type = DefineableConditionType.PROPERTY_IS_ANIMATED;
                con.data = s.Replace("isAnimated(", "").TrimEnd(')');
                return con;
            }
            if(s.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                con.type = DefineableConditionType.TRUE;
                return con;
            }
            if(s.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                con.type = DefineableConditionType.FALSE;
                return con;
            }
            return con;
        }

        static bool IsTextureNullComparission(string data, Material useThisMaterialInsteadOfOpenEditor = null)
        {
            // Check if property is a texture property && is checking for null
            Material m = GetReferencedMaterial(useThisMaterialInsteadOfOpenEditor);
            if( m == null) return false;
            if(data.Length < 7) return false;
            if(data.EndsWith("null") == false) return false;
            string propertyName = data.Substring(0, data.Length - 6);
            if(m.HasProperty(propertyName) == false) return false;
            MaterialProperty p = MaterialEditor.GetMaterialProperty(new Material[]{m}, propertyName);
            return p.type == MaterialProperty.PropType.Texture;
        }

        static Material GetReferencedMaterial(Material useThisMaterialInsteadOfOpenEditor = null)
        {
            if( useThisMaterialInsteadOfOpenEditor != null ) return useThisMaterialInsteadOfOpenEditor;
            if( ShaderEditor.Active != null ) return ShaderEditor.Active.Materials[0];
            return null;
        }
    }

    enum CompareType { NONE,BIGGER,SMALLER,EQUAL,NOT_EQUAL,BIGGER_EQ,SMALLER_EQ }

    public enum DefineableConditionType
    {
        NONE,
        TRUE,
        FALSE,
        PROPERTY_BOOL,
        PROPERTY_IS_ANIMATED,
        PROPERTY_IS_NOT_ANIMATED,
        EDITOR_VERSION,
        VRC_SDK_VERSION,
        TEXTURE_SET,
        DROPDOWN,
        AND,
        OR,
        NOT
    }

    #endregion
}