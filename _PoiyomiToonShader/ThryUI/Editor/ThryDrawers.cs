// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class SmallTextureDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            GuiHelper.drawSmallTextureProperty(position, prop, label, editor, ((ThryEditor.TextureProperty)ThryEditor.currentlyDrawing.currentProperty).hasScaleOffset);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.lastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

    public class BigTextureDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            GuiHelper.drawBigTextureProperty(position, prop, label, editor, ((ThryEditor.TextureProperty)ThryEditor.currentlyDrawing.currentProperty).hasScaleOffset);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.lastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

    public class PanningTextureDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            GuiHelper.drawSmallTextureProperty(position, prop, label, editor, ((ThryEditor.TextureProperty)ThryEditor.currentlyDrawing.currentProperty).hasScaleOffset,true);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.lastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

    public class Curve : MaterialPropertyDrawer
    {
        private class CurveData{
            public AnimationCurve curve;
            public EditorWindow window;
            public Texture2D texture;
            public bool saved = true;
            public ImageData imageData;
        }
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            CurveData data = (CurveData)ThryEditor.currentlyDrawing.currentProperty.property_data;
            if (data == null)
            {
                data = new CurveData();
                data.curve = new AnimationCurve();
                if (ThryEditor.currentlyDrawing.currentProperty.options.image == null)
                    data.imageData = new ImageData();
                else
                    data.imageData = ThryEditor.currentlyDrawing.currentProperty.options.image;
            }

            editor.TexturePropertyMiniThumbnail(position, prop, "", "");

            EditorGUI.BeginChangeCheck();
            data.curve = EditorGUI.CurveField(position, new GUIContent("       " + label.text, label.tooltip), data.curve);
            if (EditorGUI.EndChangeCheck())
            {
                data.texture = Converter.CurveToTexture(data.curve, data.imageData.width, data.imageData.height, data.imageData.channel);
                prop.textureValue = data.texture;
                data.saved = false;
            }

            string windowName = "";
            if (EditorWindow.focusedWindow != null)
                windowName = EditorWindow.focusedWindow.titleContent.text;
            bool isCurveEditor = windowName == "Curve";
            if (isCurveEditor)
                data.window = EditorWindow.focusedWindow;
            if(data.window==null && !data.saved)
            {
                Debug.Log(prop.textureValue.ToString());
                Texture saved_texture = Helper.SaveTextureAsPNG(data.texture, PATH.TEXTURES_DIR+ "curves/" + data.curve.GetHashCode() + ".png", null);
                prop.textureValue = saved_texture;
                data.saved = true;
            }

            ThryEditor.currentlyDrawing.currentProperty.property_data = data;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.lastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

    public class GradientDrawer : MaterialPropertyDrawer
    {
       GradientData data;
        bool is_init = false;

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (!is_init)
                Init(prop);

            EditorGUI.BeginChangeCheck();
            editor.TexturePropertyMiniThumbnail(position, prop, label.text, label.tooltip);
            if (EditorGUI.EndChangeCheck())
                Init(prop);

            Rect border_position = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, Screen.width - EditorGUIUtility.labelWidth - position.x - EditorGUI.indentLevel * 15 - 10, position.height);
            Rect gradient_position = new Rect(border_position.x + 1, border_position.y + 1, border_position.width - 2, border_position.height - 2);

            Texture2D backgroundTexture = Helper.GetBackgroundTexture();
            Rect texCoordsRect = new Rect(0, 0, gradient_position.width / backgroundTexture.width, gradient_position.height / backgroundTexture.height);
            GUI.DrawTextureWithTexCoords(gradient_position, backgroundTexture, texCoordsRect, false);

            if (data.preview_texture != null)
            {
                TextureWrapMode wrap_mode = data.preview_texture.wrapMode;
                data.preview_texture.wrapMode = TextureWrapMode.Clamp;
                GUI.DrawTexture(gradient_position, data.preview_texture, ScaleMode.StretchToFill, true);
                GUI.DrawTexture(border_position, data.preview_texture, ScaleMode.StretchToFill, false, 0, Color.grey, 1, 1);
                data.preview_texture.wrapMode = wrap_mode;
            }else
                GUI.DrawTexture(border_position, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, Color.grey, 1, 1);

            if (Event.current.type == EventType.MouseDown && border_position.Contains(Event.current.mousePosition))
                GradientEditor.Open(data, prop, !ThryEditor.currentlyDrawing.currentProperty.options.force_texture_options);
        }

        public void Init(MaterialProperty prop)
        {
            data = new GradientData();
            data.preview_texture = prop.textureValue;
            is_init = true;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.lastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

    public class TextTextureDrawer : MaterialPropertyDrawer
    {
        

        public struct TextData
        {
            public string text;
            public int selectedAlphabet;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            string text = "";
            int selectedAlphabet = 0;
            if (ThryEditor.currentlyDrawing.currentProperty.property_data == null)
                ThryEditor.currentlyDrawing.currentProperty.property_data = new TextData { text = Helper.LoadValueFromFile(editor.target.name + ":" + prop.name, PATH.TEXT_INFO_FILE), selectedAlphabet = 0 };
            text = ((TextData)ThryEditor.currentlyDrawing.currentProperty.property_data).text;
            selectedAlphabet = ((TextData)ThryEditor.currentlyDrawing.currentProperty.property_data).selectedAlphabet;

            string[] guids = AssetDatabase.FindAssets("alphabet t:texture");
            List<string> alphabetList = new List<string>();
            for (int i = 0; i < guids.Length; i++)
            {
                string p = AssetDatabase.GUIDToAssetPath(guids[i]);
                int index = p.LastIndexOf("/") + 1;
                int indexEnd = p.LastIndexOf(".");
                string name = p.Substring(index, indexEnd - index);
                if (!name.StartsWith("alphabet_")) continue;
                alphabetList.Add(name);
            }
            string[] alphabets = alphabetList.ToArray();

            Rect textPosition = position;
            textPosition.width *= 3f / 4;
            EditorGUI.BeginChangeCheck();
            text = EditorGUI.DelayedTextField(textPosition, new GUIContent("       " + label.text, label.tooltip), text);

            Rect popUpPosition = position;
            popUpPosition.width /= 4f;
            popUpPosition.x += popUpPosition.width * 3;
            selectedAlphabet = EditorGUI.Popup(popUpPosition, selectedAlphabet, alphabets);

            if (EditorGUI.EndChangeCheck())
            {
                foreach (Material m in ThryEditor.currentlyDrawing.materials)
                    Helper.SaveValueToFile(m.name + ":" + prop.name, text, PATH.TEXT_INFO_FILE);
                ThryEditor.currentlyDrawing.currentProperty.property_data = new TextData { text = text, selectedAlphabet = selectedAlphabet };
                prop.textureValue = Converter.TextToTexture(text, alphabets[selectedAlphabet]);
                Debug.Log("text '" + text + "' saved as texture.");
            }

            EditorGUI.BeginChangeCheck();
            editor.TexturePropertyMiniThumbnail(position, prop, "", "");
            if (EditorGUI.EndChangeCheck())
            {
                if (prop.textureValue.name.StartsWith("text_"))
                    text = prop.textureValue.name.Replace("text_", "").Replace("_", " ");
                else
                    text = "<texture>";
                ThryEditor.currentlyDrawing.currentProperty.property_data = text;
                foreach (Material m in ThryEditor.currentlyDrawing.materials)
                    Helper.SaveValueToFile(m.name + ":" + prop.name, "<texture>", PATH.TEXT_INFO_FILE);
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.lastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }

        public static void ResetMaterials()
        {
            foreach (Material m in ThryEditor.currentlyDrawing.materials)
            {
                Helper.SaveValueToFileKeyIsRegex(Regex.Escape(m.name) + @".*", "", PATH.TEXT_INFO_FILE);
            }
        }
    }

    public class MultiSliderDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            GuiHelper.MinMaxSlider(position, label, prop);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.lastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

    public class Vector3Drawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            Vector3 vec = new Vector3(prop.vectorValue.x, prop.vectorValue.y, prop.vectorValue.z);
            EditorGUI.BeginChangeCheck();
            vec = EditorGUILayout.Vector3Field(label, vec);
            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = new Vector4(vec.x, vec.y, vec.z, prop.vectorValue.w);
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.lastPropertyUsedCustomDrawer = true;
            return 0;
        }
    }

    public class Vector2Drawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            Vector2 vec = new Vector2(prop.vectorValue.x, prop.vectorValue.y);
            EditorGUI.BeginChangeCheck();
            vec = EditorGUILayout.Vector2Field(label, vec);
            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = new Vector4(vec.x, vec.y, prop.vectorValue.z, prop.vectorValue.w);
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.lastPropertyUsedCustomDrawer = true;
            return 0;
        }
    }

    public class TextureArrayDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            GuiHelper.drawConfigTextureProperty(position, prop, label, editor, true);

            string n = "";
            if (prop.textureValue != null) n = prop.textureValue.name;
            if (Event.current.type == EventType.DragExited && position.Contains(ThryEditor.lastDragPosition))
            {
                string[] paths = DragAndDrop.paths;
                if (AssetDatabase.GetMainAssetTypeAtPath(paths[0]) != typeof(Texture2DArray))
                {
                    Texture2DArray tex = Converter.PathsToTexture2DArray(paths);
                    Helper.UpdateTargetsValue(prop, tex);
                    if (ThryEditor.currentlyDrawing.currentProperty.options.reference_property != null)
                    {
                        ThryEditor.ShaderProperty p;
                        ThryEditor.currentlyDrawing.propertyDictionary.TryGetValue(ThryEditor.currentlyDrawing.currentProperty.options.reference_property, out p);
                        if (p != null)
                            Helper.UpdateTargetsValue(p.materialProperty, tex.depth);
                    }
                    prop.textureValue = tex;
                }
            }
            if (ThryEditor.currentlyDrawing.firstCall)
                ThryEditor.currentlyDrawing.textureArrayProperties.Add((ThryEditor.ShaderProperty)ThryEditor.currentlyDrawing.currentProperty);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.lastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }
}