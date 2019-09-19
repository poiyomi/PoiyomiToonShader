using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
        public List<MaterialProperty> textureArrayProperties;
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

    public class GradientObject : ScriptableObject
    {
        public Gradient gradient = new Gradient();
    }

    public class GradientData
    {
        public GradientObject gradientObj;
        public SerializedProperty colorGradient;
        public SerializedObject serializedGradient;

        public Texture2D texture;
        public bool saved;
        public EditorWindow gradientWindow;
    }

    public struct MenuHeaderData
    {
        public bool hasRightButton;
        public ButtonData rightButton;
        public override string ToString() { return "{HasRightButton:" + hasRightButton + ",RightButton" + (hasRightButton ? rightButton.ToString() : "null") + "}"; }
    }

    public struct ButtonData
    {
        public string text;
        public DefinableAction action;
        public string hover;
        public DefineableCondition condition_show;
        public override string ToString(){ return "{text:" + text + ",hover:" + hover + ",action:"+action.ToString()+",condition_show:"+ condition_show .ToString()+ "}"; }
    }

    public class DefinableAction
    {
        public DefinableActionType type;
        public string data;
        public void Perform()
        {
            switch (type)
            {
                case DefinableActionType.URL:
                    Application.OpenURL(data);
                    break;
            }
        }
        public override string ToString(){ return "{type:" + type + ",data:" + data + "}";}
    }

    public enum DefinableActionType
    {
        NONE,
        URL
    }

    public class DefineableCondition
    {
        public DefineableConditionType type;
        public string data;
        public bool Test()
        {
            switch (type)
            {
                case DefineableConditionType.PROPERTY_BOOL:
                    ThryEditor.ShaderProperty prop = ThryEditor.currentlyDrawing.propertyDictionary[data];
                    if (prop != null) return prop.materialProperty.floatValue == 1;
                    break;
            }
            
            return false;
        }
        public override string ToString() { return "{type:" + type + ",data:" + data + "}"; }
    }

    public enum DefineableConditionType
    {
        NONE,
        PROPERTY_BOOL
    }

    public class ModuleHeader
    {
        public string name = "";
        public string version = "0";
        public string description = "";
        public string url = "";
        public string classname = "";
        public string settings_file_name = "";
        public bool is_being_installed_or_removed = false;
        public ModuleInfo installed_module = null;
    }

    public class ModuleInfo
    {
        public string version = "";
        public List<string> requirements;
        public List<string> files;
    }
}