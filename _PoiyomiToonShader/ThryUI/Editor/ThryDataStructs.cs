using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Thry
{
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
        public bool draw_material_option_lightmap;
        public bool draw_material_option_instancing;
        public bool draw_material_option_dsgi;
    }

    public class DrawingData
    {
        public static ThryEditor.TextureProperty currentTexProperty;
        public static Rect lastGuiObjectRect;
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
        public DefineableCondition condition_show;
        public ButtonData button_right;
        public ImageData image;
        public TextureData texture;
        public string frameCountProp;
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
                string path = Helper.FindFile(name, "texture");
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
                    break;
                case DefineableConditionType.VRC_SDK_VERSION:
                    int c_vrc = Helper.compareVersions(VRCInterface.Get().installed_sdk_version, value);
                    if (comparator == "==") return c_vrc == 0;
                    if (comparator == "!=") return c_vrc != 0;
                    if (comparator == "<") return c_vrc == 1;
                    if (comparator == ">") return c_vrc == -1;
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
}