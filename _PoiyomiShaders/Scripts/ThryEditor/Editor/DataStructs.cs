// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Thry
{
    #region Constants
    public class PATH
    {
        public const string TEXTURES_DIR = "Assets/textures";
        public const string RSP_NEEDED_PATH = "Assets/";

        public const string DELETING_DIR = "Thry/trash";

        public const string PERSISTENT_DATA = "Thry/persistent_data";
        public const string AFTER_COMPILE_DATA = "Thry/after_compile_data";
        public const string MATERIALS_BACKUP_FILE = "Thry/materialsBackup";
        public const string THRY_EDITOR_SHADERS = "Thry/shaders";

        public const string GRADIENT_INFO_FILE = "Thry/gradients";
        public const string TEXT_INFO_FILE = "Thry/text_textures";
        public const string MODULES_LOCATION__DATA = "Thry/modules_location_data";

        public const string LINKED_MATERIALS_FILE = "Thry/linked_materials.json";

        public const string TEMP_VRC_SDK_PACKAGE = "./vrc_sdk_package.unitypackage";
    }

    public class URL
    {
        public const string MODULE_COLLECTION = "https://raw.githubusercontent.com/Thryrallo/ThryEditorStreamedResources/main/modules.json";
        public const string SETTINGS_MESSAGE_URL = "https://raw.githubusercontent.com/Thryrallo/ThryEditorStreamedResources/main/Messages/settingsWindow.json";
        public const string COUNT_PROJECT = "http://thryeditor.thryrallo.de/count_project.php";
        public const string COUNT_USER = "http://thryeditor.thryrallo.de/count_user.php";
    }

    public class DEFINE_SYMBOLS
    {
        public const string IMAGING_EXISTS = "IMAGING_DLL_EXISTS";
    }

    public class RESOURCE_NAME
    {
        public const string RECT = "thry_rect";
        public const string ICON_NAME_LINK = "thryEditor_link";
        public const string ICON_NAME_THRY = "thryEditor_iconThry";
    }
    #endregion

    public class DrawingData
    {
        public static TextureProperty CurrentTextureProperty;
        public static Rect LastGuiObjectRect;
        public static Rect LastGuiObjectHeaderRect;
        public static Rect TooltipCheckRect;
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
        None, Header
    }

    public class GradientData
    {
        public Texture PreviewTexture;
        public Gradient Gradient;
    }

    public enum TextureDisplayType
    {
        small, big, stylized_big
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
        public PropertyValueAction[] on_value_actions;
        public string on_value;
        public DefineableAction[] actions;
        public ButtonData button_help;
        public TextureData texture;
        public string[] reference_properties;
        public string reference_property;
        public bool force_texture_options = false;
        public bool hide_in_inspector = false;
        public bool is_visible_simple = false;
        public string file_name;
        public string remote_version_url;
        public string generic_string;
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
                if(!s_loaded_textures.ContainsKey(name) || s_loaded_textures[name] == null)
                {
                    if(IsUrl())
                    {
                        if(!_isLoading)
                        {
                            WebHelper.DownloadBytesASync(name, (byte[] b) =>
                            {
                                _isLoading = false;
                                Texture2D tex = new Texture2D(1,1, TextureFormat.ARGB32, false);
                                ImageConversion.LoadImage(tex, b, false);
                                s_loaded_textures[name] = tex;
                            });
                            _isLoading = true;
                        }
                    }else
                    {
                        string path = FileHelper.FindFile(name, "texture");
                        if (path != null)
                            s_loaded_textures[name] = AssetDatabase.LoadAssetAtPath<Texture>(path);
                        else
                            s_loaded_textures[name] = new Texture2D(1, 1);
                    }
                    if(!s_loaded_textures.ContainsKey(name))
                    {
                        return null;
                    }
                }
                return s_loaded_textures[name];
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
            return Parser.ParseToObject<TextureData>(s);
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
                (p.type == MaterialProperty.PropType.Range   && p.floatValue.ToString()   ==  value)          ||
                (p.type == MaterialProperty.PropType.Color   && p.colorValue.ToString()   ==  value)          ||
                (p.type == MaterialProperty.PropType.Vector  && p.vectorValue.ToString()  ==  value)          ||
                (p.type == MaterialProperty.PropType.Texture && ((p.textureValue == null) == (value == "0"))) ||
                (p.type == MaterialProperty.PropType.Texture && ((p.textureValue != null) == (value == "1"))) ||
                (p.type == MaterialProperty.PropType.Texture && (p.textureValue != null && p.textureValue.name == value)) 
            )
                {
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
            string[] parts = s.Split(',');
            if (parts.Length > 0)
            {
                PropertyValueAction propaction = new PropertyValueAction();
                propaction.value = parts[0];
                List<DefineableAction> actions = new List<DefineableAction>();
                for (int i = 1; i < parts.Length; i++)
                {
                    actions.Add(DefineableAction.Parse(parts[i]));
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
            //s = v,p1=v1,p2=v2;v3
            List<PropertyValueAction> propactions = new List<PropertyValueAction>();
            string[] parts = s.Split(';');
            foreach (string p in parts)
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
                _obj = parts[0];
                _value = parts[parts.Length - 1];

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
                    if (_compareType == CompareType.NONE) return materialProperty.floatValue == 1;
                    if (_compareType == CompareType.EQUAL) return materialProperty.floatValue == _floatValue;
                    if (_compareType == CompareType.NOT_EQUAL) return materialProperty.floatValue != _floatValue;
                    if (_compareType == CompareType.SMALLER) return materialProperty.floatValue < _floatValue;
                    if (_compareType == CompareType.BIGGER) return materialProperty.floatValue > _floatValue;
                    if (_compareType == CompareType.BIGGER_EQ) return materialProperty.floatValue >= _floatValue;
                    if (_compareType == CompareType.SMALLER_EQ) return materialProperty.floatValue <= _floatValue;
                    break;
                case DefineableConditionType.TEXTURE_SET:
                    materialProperty = GetMaterialProperty();
                    if (materialProperty == null) return false;
                    return materialProperty.textureValue != null;
                case DefineableConditionType.DROPDOWN:
                    materialProperty = GetMaterialProperty();
                    if (materialProperty == null) return false;
                    if (_compareType == CompareType.NONE) return materialProperty.floatValue == 1;
                    if (_compareType == CompareType.EQUAL) return "" + materialProperty.floatValue == _value;
                    if (_compareType == CompareType.NOT_EQUAL) return "" + materialProperty.floatValue != _value;
                    break;
                case DefineableConditionType.PROPERTY_IS_ANIMATED:
                    return ShaderOptimizer.IsAnimated(_materialInsteadOfEditor, _obj);
                case DefineableConditionType.PROPERTY_IS_NOT_ANIMATED:
                    return !ShaderOptimizer.IsAnimated(_materialInsteadOfEditor, _obj);
                case DefineableConditionType.AND:
                    if(condition1!=null&&condition2!=null) return condition1.Test() && condition2.Test();
                    break;
                case DefineableConditionType.OR:
                    if (condition1 != null && condition2 != null) return condition1.Test() || condition2.Test();
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
            }
            return "";
        }

        private static DefineableCondition ParseForThryParser(string s)
        {
            return Parse(s);
        }

        private static readonly char[] ComparissionLiteralsToCheckFor = "*><=".ToCharArray();
        public static DefineableCondition Parse(string s, Material useThisMaterialInsteadOfOpenEditor = null)
        {
            DefineableCondition con = new DefineableCondition();
            con._materialInsteadOfEditor = useThisMaterialInsteadOfOpenEditor;

            s = Strip(s);

            int depth = 0;
            for (int i = 0; i < s.Length - 1; i++)
            {
                char c = s[i];
                char cc = s[i + 1];
                if (c == '(')
                    depth++;
                else if (c == ')')
                    depth--;

                if (depth == 0)
                {
                    if (c == '&' && cc == '&')
                    {
                        con.type = DefineableConditionType.AND;
                        con.condition1 = Parse(s.Substring(0, i), useThisMaterialInsteadOfOpenEditor);
                        con.condition2 = Parse(s.Substring(i + 2, s.Length - i - 2), useThisMaterialInsteadOfOpenEditor);
                        return con;
                    }
                    if (c == '|' && cc == '|')
                    {
                        con.type = DefineableConditionType.OR;
                        con.condition1 = Parse(s.Substring(0, i), useThisMaterialInsteadOfOpenEditor);
                        con.condition2 = Parse(s.Substring(i + 2, s.Length - i - 2), useThisMaterialInsteadOfOpenEditor);
                        return con;
                    }
                }
            }
            if(s.IndexOfAny(ComparissionLiteralsToCheckFor) != -1)
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
            return con;
        }

        private static string Strip(string s)
        {
            s = s.Trim();
            if (s.StartsWith("(", StringComparison.Ordinal) == false)
                return s;
            bool stripKlammer = true;
            int depth = 0;
            int i = 0;
            foreach (char c in s)
            {
                if (c == '(')
                    depth++;
                else if (c == ')')
                    depth--;
                if (depth == 0 && i != 0 && i != s.Length - 1)
                    stripKlammer = false;
                i++;
            }
            if (stripKlammer)
                return Strip(s.Substring(1, s.Length - 2));
            return s;
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
        OR
    }

    #endregion

    #region Module Data

    public class Module
    {
        public string id;
        public string url = "";
        public string author;
        public string path;
        public bool is_being_installed_or_removed = false;
        public bool available_requirement_fullfilled = true;
        public bool update_available = false;
        public ModuleLocationData location_data;
        public ModuleInfo available_module = null;
        public ModuleInfo installed_module = null;
        public bool ui_expanded = false;
    }

    public class ModuleInfo
    {
        public string name = "";
        public string version = "0";
        public string description = "";
        public string classname = "";
        public DefineableCondition requirement;
        public List<string> files;
    }

    public class ModuleLocationData
    {
        public string guid;
        public string classname;
        public string[] files;
    }

    #endregion
}