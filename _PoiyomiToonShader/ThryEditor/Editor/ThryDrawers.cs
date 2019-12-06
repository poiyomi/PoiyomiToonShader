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

    public class StylizedBigTextureDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            GuiHelper.drawStylizedBigTextureProperty(position, prop, label, editor, ((ThryEditor.TextureProperty)ThryEditor.currentlyDrawing.currentProperty).hasScaleOffset);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.lastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

    public class CurveDrawer : MaterialPropertyDrawer
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
                Texture saved_texture = TextureHelper.SaveTextureAsPNG(data.texture, PATH.TEXTURES_DIR+ "curves/" + data.curve.GetHashCode() + ".png", null);
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
            GuiHelper.drawSmallTextureProperty(position, prop, label, editor, DrawingData.currentTexProperty.hasFoldoutProperties);
            if (EditorGUI.EndChangeCheck())
                Init(prop);

            Rect border_position = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, Screen.width - EditorGUIUtility.labelWidth - position.x - EditorGUI.indentLevel * 15 - 10, position.height);
            Rect gradient_position = new Rect(border_position.x + 1, border_position.y + 1, border_position.width - 2, border_position.height - 2);

            Texture2D backgroundTexture = TextureHelper.GetBackgroundTexture();
            Rect texCoordsRect = new Rect(0, 0, gradient_position.width / backgroundTexture.width, gradient_position.height / backgroundTexture.height);
            GUI.DrawTextureWithTexCoords(gradient_position, backgroundTexture, texCoordsRect, false);

            if (data.preview_texture != null)
            {
                TextureWrapMode wrap_mode = data.preview_texture.wrapMode;
                data.preview_texture.wrapMode = TextureWrapMode.Clamp;
                bool vertical = data.preview_texture.height > data.preview_texture.width;
                Vector2 pivot = new Vector2();
                if (vertical)
                {
                    pivot = new Vector2(gradient_position.x, gradient_position.y + gradient_position.height);
                    GUIUtility.RotateAroundPivot(-90, pivot);
                    gradient_position.y += gradient_position.height;
                    float h = gradient_position.width;
                    gradient_position.width = gradient_position.height;
                    gradient_position.y += h;
                    gradient_position.height = -h;
                }
                GUI.DrawTexture(gradient_position, data.preview_texture, ScaleMode.StretchToFill, true);
                if (vertical)
                {
                    GUIUtility.RotateAroundPivot(90, pivot);
                }
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
            vec = EditorGUI.Vector3Field(position, label, vec);
            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = new Vector4(vec.x, vec.y, vec.z, prop.vectorValue.w);
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.lastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

    public class Vector2Drawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            Vector2 vec = new Vector2(prop.vectorValue.x, prop.vectorValue.y);
            EditorGUI.BeginChangeCheck();
            vec = EditorGUI.Vector2Field(position, label, vec);
            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = new Vector4(vec.x, vec.y, prop.vectorValue.z, prop.vectorValue.w);
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.lastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

    public class TextureArrayDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            GuiHelper.drawConfigTextureProperty(position, prop, label, editor, true, true);

            string n = "";
            if (prop.textureValue != null) n = prop.textureValue.name;
            if ((ThryEditor.input.is_drag_drop_event) && DrawingData.lastGuiObjectRect.Contains(ThryEditor.input.mouse_position))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (ThryEditor.input.is_drop_event)
                {
                    DragAndDrop.AcceptDrag();
                    HanldeDropEvent(prop);
                }
            }
            if (ThryEditor.currentlyDrawing.firstCall)
                ThryEditor.currentlyDrawing.textureArrayProperties.Add((ThryEditor.ShaderProperty)ThryEditor.currentlyDrawing.currentProperty);
        }

        public void HanldeDropEvent(MaterialProperty prop)
        {
            string[] paths = DragAndDrop.paths;
            if (AssetDatabase.GetMainAssetTypeAtPath(paths[0]) != typeof(Texture2DArray))
            {
                Texture2DArray tex = Converter.PathsToTexture2DArray(paths);
                MaterialHelper.UpdateTargetsValue(prop, tex);
                if (ThryEditor.currentlyDrawing.currentProperty.options.reference_property != null)
                {
                    ThryEditor.ShaderProperty p;
                    ThryEditor.currentlyDrawing.propertyDictionary.TryGetValue(ThryEditor.currentlyDrawing.currentProperty.options.reference_property, out p);
                    if (p != null)
                        MaterialHelper.UpdateFloatValue(p.materialProperty, tex.depth);
                }
                prop.textureValue = tex;
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.lastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

    public class HelpboxDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            EditorGUILayout.HelpBox(label.text, MessageType.Info);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.lastPropertyUsedCustomDrawer = true;
            return 0;
        }
    }
}