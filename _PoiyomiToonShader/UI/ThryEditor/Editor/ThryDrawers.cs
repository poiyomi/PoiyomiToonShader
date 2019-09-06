using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Thry
{
        public class TextureDrawer : MaterialPropertyDrawer
        {
            public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
            {
                ThryEditorGuiHelper.drawConfigTextureProperty(position, prop, label, editor, true);
            }

            public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
            {
                DrawingData.lastPropertyUsedCustomDrawer = true;
                return base.GetPropertyHeight(prop, label, editor);
            }
        }

        public class TextureNoSODrawer : MaterialPropertyDrawer
        {
            public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
            {
                ThryEditorGuiHelper.drawConfigTextureProperty(position, prop, label, editor, false);
            }

            public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
            {
                DrawingData.lastPropertyUsedCustomDrawer = true;
                return base.GetPropertyHeight(prop, label, editor);
            }
        }

        public class SmallTextureDrawer : MaterialPropertyDrawer
        {
            public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
            {
                ThryEditorGuiHelper.drawSmallTextureProperty(position, prop, label, editor, true);
            }

            public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
            {
                DrawingData.lastPropertyUsedCustomDrawer = true;
                return base.GetPropertyHeight(prop, label, editor);
            }
        }

        public class SmallTextureNoSODrawer : MaterialPropertyDrawer
        {
            public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
            {
                ThryEditorGuiHelper.drawSmallTextureProperty(position, prop, label, editor, false);
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
                ThryEditorGuiHelper.drawBigTextureProperty(position, prop, label, editor, true);
            }

            public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
            {
                DrawingData.lastPropertyUsedCustomDrawer = true;
                return base.GetPropertyHeight(prop, label, editor);
            }
        }

        public class BigTextureNoSODrawer : MaterialPropertyDrawer
        {
            public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
            {
                ThryEditorGuiHelper.drawBigTextureProperty(position, prop, label, editor, false);
            }

            public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
            {
                DrawingData.lastPropertyUsedCustomDrawer = true;
                return base.GetPropertyHeight(prop, label, editor);
            }
        }

        public class GradientDrawer : MaterialPropertyDrawer
        {
            const string GRADIENT_INFO_FILE_PATH = "Assets/.thry_gradients";

            public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
            {
                GradientData data;
                if (ThryEditor.currentlyDrawing.currentProperty.property_data != null) data = (GradientData)ThryEditor.currentlyDrawing.currentProperty.property_data;
                else { data = new GradientData(); data.saved = true; ThryEditor.currentlyDrawing.currentProperty.property_data = data; }

                if (data.gradientObj == null)
                {
                    data.gradientObj = GradientObject.CreateInstance<GradientObject>();
                    if (prop.textureValue != null)
                    {
                        data.texture = (Texture2D)prop.textureValue;
                        TextureUpdated(ref data);
                    }
                    else
                    {
                        data.texture = new Texture2D(256, 1);
                        data.serializedGradient = new SerializedObject(data.gradientObj);
                        data.colorGradient = data.serializedGradient.FindProperty("gradient");
                    }
                    data.saved = true;
                }
                EditorGUI.BeginChangeCheck();
                editor.TexturePropertyMiniThumbnail(position, prop, "", "");
                if (EditorGUI.EndChangeCheck())
                {
                    data.texture = (Texture2D)prop.textureValue;
                    TextureUpdated(ref data);
                }
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(position, data.colorGradient, new GUIContent("       " + label.text, label.tooltip));
                string windowName = "";
                if (EditorWindow.focusedWindow != null)
                    windowName = EditorWindow.focusedWindow.titleContent.text;
                bool isGradientEditor = windowName == "Gradient Editor";
                if (isGradientEditor)
                {
                    data.gradientWindow = EditorWindow.focusedWindow;
                }
                bool changed = EditorGUI.EndChangeCheck();
                if (changed)
                {
                    if (data.texture == prop.textureValue) data.texture = new Texture2D(256, 1);
                    data.serializedGradient.ApplyModifiedProperties();
                    Converter.GradientToTexture(ref data);
                    prop.textureValue = data.texture;
                    data.saved = false;
                }

                if (data.gradientWindow == null && !data.saved)
                {
                    byte[] encoding = data.texture.EncodeToPNG();

                    string gradient_data = Converter.GradientToString(ref data);
                    string gradient_name = Config.Get().gradient_name;
                    gradient_name = gradient_name.Replace("<material>", editor.target.name);
                    gradient_name = gradient_name.Replace("<hash>", "" + gradient_data.GetHashCode());
                    gradient_name = gradient_name.Replace("<prop>", prop.name);

                    string path = "Assets/Textures/Gradients/" + gradient_name;
                    Debug.Log("Gradient saved at \"" + path + "\".");
                    Helper.writeBytesToFile(encoding, path);

                    Helper.SaveValueToFile(gradient_name, gradient_data, GRADIENT_INFO_FILE_PATH);

                    AssetDatabase.ImportAsset(path);
                    Texture tex = (Texture)EditorGUIUtility.Load(path);
                    tex.wrapMode = TextureWrapMode.Clamp;
                    SetTextureImporterFormat((Texture2D)tex, true);
                    prop.textureValue = tex;
                    data.saved = true;
                }
            }

            private void TextureUpdated(ref GradientData data)
            {
                data.texture = SetTextureImporterFormat(data.texture, true);
                string gradientInfo = Helper.LoadValueFromFile(data.texture.name, GRADIENT_INFO_FILE_PATH);
                if (gradientInfo != null)
                {
                    Debug.Log("Gradient Data: " + gradientInfo);
                    Debug.Log("Load Gradient from save file.");
                    Converter.StringToGradient(ref data, gradientInfo);
                    data.serializedGradient = new SerializedObject(data.gradientObj);
                    data.colorGradient = data.serializedGradient.FindProperty("gradient");
                }
                else
                {
                    Converter.TextureToGradient(ref data);
                }
            }

            public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
            {
                DrawingData.lastPropertyUsedCustomDrawer = true;
                return base.GetPropertyHeight(prop, label, editor);
            }

            private string GradientFileName(ref GradientData data, string material_name)
            {
                string hash = "" + Converter.GradientToString(ref data).GetHashCode();
                return GradientFileName(hash, material_name);
            }

            private string GradientFileName(string hash, string material_name)
            {
                Config config = Config.Get();
                string ret = config.gradient_name;
                ret = Regex.Replace(ret, "<hash>", hash);
                ret = Regex.Replace(ret, "<material>", material_name);
                return ret;
            }

            public static Texture2D SetTextureImporterFormat(Texture2D texture, bool isReadable)
            {
                if (null == texture) return texture;
                string assetPath = AssetDatabase.GetAssetPath(texture);
                var tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (tImporter != null)
                {
                    tImporter.isReadable = isReadable;
                    tImporter.wrapMode = TextureWrapMode.Clamp;

                    AssetDatabase.ImportAsset(assetPath);
                    AssetDatabase.Refresh();

                    return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                }
                return texture;
            }
        }

        public class TextTextureDrawer : MaterialPropertyDrawer
        {
            const string TEXT_INFO_FILE_PATH = "Assets/.thry_text_textures";

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
                ThryEditor.currentlyDrawing.currentProperty.property_data = new TextData { text = Helper.LoadValueFromFile(editor.target.name + ":" + prop.name, TEXT_INFO_FILE_PATH), selectedAlphabet = 0 };
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
                        Helper.SaveValueToFile(m.name + ":" + prop.name, text, TEXT_INFO_FILE_PATH);
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
                        Helper.SaveValueToFile(m.name + ":" + prop.name, "<texture>", TEXT_INFO_FILE_PATH);
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
                    Helper.SaveValueToFileKeyIsRegex(Regex.Escape(m.name) + @".*", "", TEXT_INFO_FILE_PATH);
                }
            }
        }

        public class MultiSliderDrawer : MaterialPropertyDrawer
        {
            public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
            {
                Vector4 vec = prop.vectorValue;
                float left = vec.x;
                float right = vec.y;
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.MinMaxSlider(prop.displayName, ref left, ref right, vec.z, vec.w);
                if (EditorGUI.EndChangeCheck())
                {
                    vec.x = left;
                    vec.y = right;
                    prop.vectorValue = vec;
                }
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
            if (ThryEditor.currentlyDrawing.currentProperty.property_data == null)
            {
                if (label.text.Contains("--frameCountProp="))
                    ThryEditor.currentlyDrawing.currentProperty.property_data = label.text.Split(new string[]{"--frameCountProp="},System.StringSplitOptions.None);
                else
                    ThryEditor.currentlyDrawing.currentProperty.property_data = new string[] { label.text };
            }
            label.text = ((string[])ThryEditor.currentlyDrawing.currentProperty.property_data)[0];
            ThryEditorGuiHelper.drawConfigTextureProperty(position, prop, label, editor, true);

            string n = "";
            if (prop.textureValue != null) n = prop.textureValue.name;
            if (Event.current.type == EventType.DragExited && position.Contains(ThryEditor.lastDragPosition))
            {
                string[] paths = DragAndDrop.paths;
                if (AssetDatabase.GetMainAssetTypeAtPath(paths[0]) != typeof(Texture2DArray))
                {
                    Texture2DArray tex = Converter.PathsToTexture2DArray(paths);
                    Helper.UpdateTargetsValue(prop, tex);
                    string[] data = ((string[])ThryEditor.currentlyDrawing.currentProperty.property_data);
                    if (data.Length > 1)
                    {
                        ThryEditor.ShaderProperty p;
                        ThryEditor.currentlyDrawing.propertyDictionary.TryGetValue(data[1], out p);
                        if (p != null)
                        Helper.UpdateTargetsValue(p.materialProperty, tex.depth);
                    }
                    prop.textureValue = tex;
                }
            }
            if (ThryEditor.currentlyDrawing.firstCall)
                ThryEditor.currentlyDrawing.textureArrayProperties.Add(prop);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.lastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }
    }
}