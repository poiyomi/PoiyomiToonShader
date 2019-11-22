// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Thry
{
    public class PATH
    {
        public const string TEXTURES_DIR = "Assets/textures/";
        public const string RSP_NEEDED_PATH = "Assets/";

        public const string DELETING_DIR = "Thry/trash";

        public const string PERSISTENT_DATA = "Thry/persistent_data";
        public const string AFTER_COMPILE_DATA = "Thry/after_compile_data";
        public const string MATERIALS_BACKUP_FILE = "Thry/materialsBackup.txt";
        public const string THRY_EDITOR_SHADERS = "Thry/shaders";

        public const string GRADIENT_INFO_FILE = "Thry/gradients";
        public const string TEXT_INFO_FILE = "Thry/text_textures";

        public const string TEMP_VRC_SDK_PACKAGE = "./vrc_sdk_package.unitypackage";
    }

    public class URL
    {
        public const string PUBLIC_MODULES_COLLECTION = "https://raw.githubusercontent.com/Thryrallo/ThryEditor/master/modules.json";
        public const string SETTINGS_MESSAGE_URL = "http://thryeditor.thryrallo.de/message.json";

        public const string DATA_SHARE_SEND = "http://thryeditor.thryrallo.de/send_analytics.php";
        public const string DATA_SHARE_GET_MY_DATA = "https://thryeditor.thryrallo.de/get_my_data.php";
        public const string COUNT_PROJECT = "http://thryeditor.thryrallo.de/count_project.php";
        public const string COUNT_USER = "http://thryeditor.thryrallo.de/count_user.php";
    }

    public class DEFINE_SYMBOLS
    {
        public const string VRC_SDK_INSTALLED = "VRC_SDK_EXISTS";
        public const string API_NET_TWO = "DOT_NET_TWO_POINT_ZERO_OR_ABOVE";
        public const string IMAGING_EXISTS = "IMAGING_DLL_EXISTS";
    }

    public struct EditorData
    {
        public MaterialEditor editor;
        public MaterialProperty[] properties;
        public ThryEditor gui;
        public Material[] materials;
        public Shader shader;
        public Shader defaultShader;
        public ThryEditor.ShaderPart currentProperty;
        public Dictionary<string, ThryEditor.ShaderProperty> propertyDictionary;
        public List<ThryEditor.ShaderProperty> textureArrayProperties;
        public bool firstCall;
    }

    public class DrawingData
    {
        public static ThryEditor.TextureProperty currentTexProperty;
        public static Rect lastGuiObjectRect;
        public static Rect lastGuiObjectHeaderRect;
        public static bool lastPropertyUsedCustomDrawer;
    }

    public class GradientData
    {
        public Texture preview_texture;
        public Gradient gradient;
    }

    public class PropertyOptions
    {
        public int offset = 0;
        public string hover = "";
        public DefinableAction altClick;
        public DefineableCondition condition_show = new DefineableCondition();
        public ButtonData button_right;
        public ImageData image;
        public TextureData texture;
        public string[] reference_properties;
        public string reference_property;
        public bool force_texture_options = false;
        public string file_name;
    }

    public class ImageData
    {
        public int width = 128;
        public int height = 8;
        public char channel = 'r';
    }

    public class ButtonData
    {
        public string text = "";
        public TextureData texture = null;
        public DefinableAction action = new DefinableAction();
        public string hover = "";
        public DefineableCondition condition_show = new DefineableCondition();
    }

    public class TextureData
    {
        public string name = null;

        public int width = 128;
        public int height = 128;

        public int ansioLevel = 1;
        public FilterMode filterMode = FilterMode.Bilinear;
        public TextureWrapMode wrapMode = TextureWrapMode.Repeat;

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

        public Texture loaded_texture;
        public Texture GetTextureFromName()
        {
            if (loaded_texture == null)
            {
                string path = FileHelper.FindFile(name, "texture");
                if (path != null)
                    loaded_texture = AssetDatabase.LoadAssetAtPath<Texture>(path);
                else
                    loaded_texture = new Texture2D(1,1);
            }
            return loaded_texture;
        }
    }

    public class DefinableAction
    {
        public DefinableActionType type = DefinableActionType.NONE;
        public string data = "";
        public void Perform()
        {
            switch (type)
            {
                case DefinableActionType.URL:
                    Application.OpenURL(data);
                    break;
            }
        }
    }

    public enum DefinableActionType
    {
        NONE,
        URL
    }

    public class DefineableCondition
    {
        public DefineableConditionType type = DefineableConditionType.NONE;
        public string data = "";
        public DefineableCondition condition1;
        public DefineableCondition condition2;
        public bool Test()
        {
            if (type == DefineableConditionType.NONE)
                return true;
            string comparator = GetComparetor();
            string[] parts = Regex.Split(data, comparator);
            string obj = parts[0];
            string value = parts[parts.Length-1];
            switch (type)
            {
                case DefineableConditionType.PROPERTY_BOOL:
                    ThryEditor.ShaderProperty prop = ThryEditor.currentlyDrawing.propertyDictionary[obj];
                    if (prop == null) return false;
                    if (comparator == "##") return prop.materialProperty.floatValue == 1;
                    if (comparator == "==") return "" + prop.materialProperty.floatValue == parts[1];
                    if (comparator == "!=") return ""+prop.materialProperty.floatValue != parts[1];
                    break;
                case DefineableConditionType.EDITOR_VERSION:
                    int c_ev = Helper.compareVersions(Config.Get().verion, value);
                    if (comparator == "==") return c_ev == 0;
                    if (comparator == "!=") return c_ev != 0;
                    if (comparator == "<") return c_ev == 1;
                    if (comparator == ">") return c_ev == -1;
                    if (comparator == ">=") return c_ev == -1 || c_ev == 0;
                    if (comparator == "<=") return c_ev == 1 || c_ev == 0;
                    break;
                case DefineableConditionType.VRC_SDK_VERSION:
                    int c_vrc = Helper.compareVersions(VRCInterface.Get().installed_sdk_version, value);
                    if (comparator == "==") return c_vrc == 0;
                    if (comparator == "!=") return c_vrc != 0;
                    if (comparator == "<") return c_vrc == 1;
                    if (comparator == ">") return c_vrc == -1;
                    if (comparator == ">=") return c_vrc == -1 || c_vrc == 0;
                    if (comparator == "<=") return c_vrc == 1 || c_vrc == 0;
                    break;
                case DefineableConditionType.TEXTURE_SET:
                    ThryEditor.ShaderProperty shaderProperty = ThryEditor.currentlyDrawing.propertyDictionary[data];
                    if (shaderProperty == null) return false;
                    return shaderProperty.materialProperty.textureValue != null;
                case DefineableConditionType.DROPDOWN:
                    ThryEditor.ShaderProperty dropdownProperty = ThryEditor.currentlyDrawing.propertyDictionary[obj];
                    if (dropdownProperty == null) return false;
                    if (comparator == "##") return dropdownProperty.materialProperty.floatValue == 1;
                    if (comparator == "==") return "" + dropdownProperty.materialProperty.floatValue == parts[1];
                    if (comparator == "!=") return "" + dropdownProperty.materialProperty.floatValue != parts[1];
                    break;
                case DefineableConditionType.AND:
                    if(condition1!=null&&condition2!=null) return condition1.Test() && condition2.Test();
                    break;
                case DefineableConditionType.OR:
                    if (condition1 != null && condition2 != null) return condition1.Test() || condition2.Test();
                    break;
            }
            
            return true;
        }
        private string GetComparetor()
        {
            if (data.Contains("=="))
                return "==";
            if (data.Contains("!="))
                return "!=";
            if (data.Contains(">="))
                return ">=";
            if (data.Contains("<="))
                return "<=";
            if (data.Contains(">"))
                return ">";
            if (data.Contains("<"))
                return "<";
            return "##";
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
                case DefineableConditionType.AND:
                    if (condition1 != null && condition2 != null) return condition1.ToString() + "&&" + condition2.ToString();
                    break;
                case DefineableConditionType.OR:
                    if (condition1 != null && condition2 != null) return condition1.ToString()+"||"+condition2.ToString();
                    break;
            }
            return "";
        }
    }

    public enum DefineableConditionType
    {
        NONE,
        PROPERTY_BOOL,
        EDITOR_VERSION,
        VRC_SDK_VERSION,
        TEXTURE_SET,
        DROPDOWN,
        AND,
        OR
    }

    public class ModuleHeader
    {
        public string url = "";
        public bool is_being_installed_or_removed = false;
        public bool available_requirement_fullfilled = true;
        public ModuleInfo available_module = null;
        public ModuleInfo installed_module = null;
    }

    public class ModuleInfo
    {
        public string name = "";
        public string version = "0";
        public string description = "";
        public string classname = "";
        public string settings_file_name = "";
        public DefineableCondition requirement;
        public List<string> files;
    }

    public enum TextureDisplayType
    {
        small,big,stylized_big
    }
}