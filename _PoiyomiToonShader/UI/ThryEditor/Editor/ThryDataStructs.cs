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
        public override string ToString(){ return "{Text:" + text + ",Hover:" + hover + ",Action:"+action.ToString()+"}"; }
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
        public override string ToString(){ return "{Type:" + type + ",Data:" + data + "}";}
    }

    public enum DefinableActionType
    {
        NONE,
        URL
    }
}