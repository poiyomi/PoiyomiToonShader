using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class DrawingData
    {
        public static ThryEditor.TextureProperty currentTexProperty;
        public static Rect lastGuiObjectRect;
        public static bool lastPropertyUsedCustomDrawer;
    }

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

    class GradientObject : ScriptableObject
    {
        public Gradient gradient = new Gradient();
    }

    public class GradientDrawer : MaterialPropertyDrawer
    {
        const string GRADIENT_INFO_FILE_PATH = "Assets/.thry_gradients";

        private class GradientData {
            public GradientObject gradientObj;
            public SerializedProperty colorGradient;
            public SerializedObject serializedGradient;

            public Texture2D texture;
            public bool saved;
            public EditorWindow gradientWindow;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            GradientData data;
            if (ThryEditor.currentlyDrawing.property_data != null) data = (GradientData)ThryEditor.currentlyDrawing.property_data;
            else { data = new GradientData(); data.saved = true; ThryEditor.currentlyDrawing.property_data = data; }
            
            if (data.gradientObj == null)
            {
                data.gradientObj = GradientObject.CreateInstance<GradientObject>();
                if (prop.textureValue!=null)
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
            editor.TexturePropertyMiniThumbnail(position, prop, "","");
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
                GradientToTexture(ref data);
                prop.textureValue = data.texture;
                data.saved = false;
            }
            
            if (data.gradientWindow == null && !data.saved)
            {
                byte[] encoding = data.texture.EncodeToPNG();

                string gradient_data = GradientToString(ref data);
                string gradient_name = Config.Get().gradient_name;
                gradient_name = gradient_name.Replace("<material>", editor.target.name);
                gradient_name = gradient_name.Replace("<hash>", ""+gradient_data.GetHashCode());
                gradient_name = gradient_name.Replace("<prop>", prop.name);

                string path = "Assets/Textures/Gradients/" + gradient_name;
                Debug.Log("Gradient saved at \""+ path + "\".");
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
            if (gradientInfo!=null)
            {
                Debug.Log("Gradient Data: " + gradientInfo);
                Debug.Log("Load Gradient from save file.");
                StringToGradient(ref data, gradientInfo);
                data.serializedGradient = new SerializedObject(data.gradientObj);
                data.colorGradient = data.serializedGradient.FindProperty("gradient");
            }
            else
            {
                TextureToGradient(ref data);
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            DrawingData.lastPropertyUsedCustomDrawer = true;
            return base.GetPropertyHeight(prop, label, editor);
        }

        private string GradientToString(ref GradientData data)
        {
            string ret = "";
            foreach (GradientColorKey key in data.gradientObj.gradient.colorKeys)
                ret += "c,"+key.color.r+","+key.color.g+","+key.color.b + ","+ key.time;
            foreach (GradientAlphaKey key in data.gradientObj.gradient.alphaKeys)
                ret += "a,"+key.alpha+"," + key.time;
            ret += "m"+((int)data.gradientObj.gradient.mode);
            return ret;
        }

        private string GradientFileName(ref GradientData data, string material_name)
        {
            string hash = ""+GradientToString(ref data).GetHashCode();
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

        private void StringToGradient(ref GradientData data, string s)
        {
            List<GradientColorKey> colorKeys = new List<GradientColorKey>();
            List<GradientAlphaKey> alphaKeys = new List<GradientAlphaKey>();
            MatchCollection colorMatches = Regex.Matches(s, @"c,\d+(\.\d+)?,\d+(\.\d+)?,\d+(\.\d+)?,\d+(\.?\d+)?");
            MatchCollection alphaMatches = Regex.Matches(s, @"a,\d+(\.\d+)?,\d+(\.\d+)?");
            Match blendMatch = Regex.Match(s, @"m\d+");
            foreach(Match m in colorMatches)
            {
                string[] graddata = Regex.Split(m.Value, @",");
                colorKeys.Add(new GradientColorKey(new Color(float.Parse(graddata[1]), float.Parse(graddata[2]), float.Parse(graddata[3]), 1), float.Parse(graddata[4])));
            }
            foreach(Match m in alphaMatches)
            {
                string[] graddata = Regex.Split(m.Value, @",");
                alphaKeys.Add(new GradientAlphaKey(float.Parse(graddata[1]), float.Parse(graddata[2])));
            }
            data.gradientObj.gradient.SetKeys(colorKeys.ToArray(), alphaKeys.ToArray());
            data.gradientObj.gradient.mode = (GradientMode)(int.Parse(blendMatch.Value.Replace("m","")));
        }

        private void GradientToTexture(ref GradientData data)
        {
            for (int x = 0; x < data.texture.width; x++)
            {
                Color col = data.gradientObj.gradient.Evaluate((float)x / data.texture.width);
                for (int y = 0; y < data.texture.height; y++) data.texture.SetPixel(x, y, col);
            }
            data.texture.Apply();
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

        private void TextureToGradient(ref GradientData data)
        {
            Debug.Log("Texture converted to gradient.");
            
            int d = (int)Mathf.Sqrt(Mathf.Pow(data.texture.width, 2) + Mathf.Pow(data.texture.height, 2));
            List<GradientColorKey> colorKeys = new List<GradientColorKey>();
            List<GradientAlphaKey> alphaKeys = new List<GradientAlphaKey>();
            colorKeys.Add(new GradientColorKey(data.texture.GetPixel(data.texture.width-1, data.texture.height-1), 1));
            alphaKeys.Add(new GradientAlphaKey(data.texture.GetPixel(data.texture.width-1, data.texture.height-1).a, 1));
            colorKeys.Add(new GradientColorKey(data.texture.GetPixel(0, 0), 0));
            alphaKeys.Add(new GradientAlphaKey(data.texture.GetPixel(0, 0).a, 0));
            int colKeys = 0;
            int alphaKeysCount = 0;

            bool isFlat = false;
            bool isNotFlat = false;

            float[][] prevSteps = new float[][]{ GetSteps(GetColorAtI(ref data, 0, d), GetColorAtI(ref data, 1, d)), GetSteps(GetColorAtI(ref data, 0, d), GetColorAtI(ref data, 1, d)) };

            bool wasFlat = false;
            int maxBetweenFlats = 3;
            int minFlat = 3;
            int flats = 0;
            int prevFlats = 0;
            int nonFlats = 0;
            float[][] steps = new float[d][];
            float[] alphaStep = new float[d];
            Color prevColor = GetColorAtI(ref data, 0, d);
            for (int i = 0; i < d; i++)
            {
                Color col = GetColorAtI(ref data, i, d);
                steps[i] = GetSteps(prevColor, col);
                alphaStep[i] = Mathf.Abs(prevColor.a - col.a);
                prevColor = col;
            }
            for(int r = 0; r < 1; r++)
            {
                for (int i = 1; i < d-1; i++)
                {
                    //Debug.Log(i+": "+steps[i][0] + "," + steps[i][1] + ","+steps[i][0]);
                    bool returnToOldVal = false;
                    if (!SameSteps(steps[i], steps[i + 1])&& SimilarSteps(steps[i], steps[i + 1], 0.1f))
                    {
                        int n = i;
                        while(++n < d && SimilarSteps(steps[i - 1], steps[n],0.1f) )
                            if (SameSteps(steps[i - 1], steps[n])) returnToOldVal = true;
                    }
                    if (returnToOldVal) steps[i] = steps[i - 1];

                    returnToOldVal = false;
                    //Debug.Log(i + ": " + steps[i][0] + "," + steps[i][1] + "," + steps[i][0]);
                }
            }


            Color lastStableColor = GetColorAtI(ref data, 0, d);
            float lastStableTime = 0;
            bool added = false;
            for (int i = 1; i < d; i ++)
            {
                Color col = GetColorAtI(ref data, i, d);
                float[] newColSteps = steps[i];
                float time = (float)(i)/d;

                float[] diff = new float[] { prevSteps[0][0] - newColSteps[0], prevSteps[0][1] - newColSteps[1], prevSteps[0][2] - newColSteps[2] };

                if (diff[0] == 0 && diff[1] == 0 && diff[2] == 0)
                {
                    lastStableColor = col;
                    lastStableTime = time;
                    added = false;
                }
                else
                {
                    if (added==false && colKeys++<6) colorKeys.Add(new GradientColorKey(lastStableColor, lastStableTime));
                    added = true;
                }

                float alphaDiff = Mathf.Abs(alphaStep[i-1] - alphaStep[i]);
                if (alphaDiff > 0.05 && ++alphaKeysCount < 6) alphaKeys.Add(new GradientAlphaKey(col.a, time));

                prevSteps[1] = prevSteps[0];
                prevSteps[0] = newColSteps;

                bool thisOneFlat = newColSteps[0] == 0 && newColSteps[1] == 0 && newColSteps[2] == 0;
                if (thisOneFlat) flats++;
                else if (!wasFlat && !thisOneFlat) nonFlats++;
                else if (wasFlat && !thisOneFlat) { prevFlats = flats; flats = 0; nonFlats = 1; }
                if (flats >= minFlat && prevFlats >= minFlat && nonFlats <= maxBetweenFlats) isFlat = true;
                if (nonFlats > maxBetweenFlats) isNotFlat = true;
                wasFlat = thisOneFlat;
            }
            data.gradientObj.gradient.SetKeys(colorKeys.ToArray(), alphaKeys.ToArray());
            if (isFlat && !isNotFlat) data.gradientObj.gradient.mode = GradientMode.Fixed;
            data.serializedGradient = new SerializedObject(data.gradientObj);
            data.colorGradient = data.serializedGradient.FindProperty("gradient");
            ThryEditor.repaint();
        }

        private bool SimilarSteps(float[] steps1, float[] steps2, float perc)
        {
            if (Mathf.Abs(steps1[0] - steps2[0]) > perc || Mathf.Abs(steps1[1] - steps2[1]) > perc || Mathf.Abs(steps1[2] - steps2[2]) > perc) return false;
            return steps1[0] == steps2[0] || steps1[1] == steps2[1] || steps1[2] == steps2[2];
        }

        private bool SameSteps(float[] steps1, float[] steps2)
        {
            return steps1[0] == steps2[0] && steps1[1] == steps2[1] && steps1[2] == steps2[2];
        }

        private float[] GetSteps(Color col1, Color col2)
        {
            return new float[] { col1.r - col2.r, col1.g - col2.g, col1.b - col2.b };
        }

        private Color GetColorAtI(ref GradientData data, int i,int d)
        {
            int y = (int)(((float)i) / d * data.texture.height);
            int x = (int)(((float)i) / d * data.texture.width);
            Color col = data.texture.GetPixel(x, y);
            return col;
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
            if (ThryEditor.currentlyDrawing.property_data == null)
                ThryEditor.currentlyDrawing.property_data = new TextData{ text=Helper.LoadValueFromFile(editor.target.name + ":" + prop.name, TEXT_INFO_FILE_PATH), selectedAlphabet=0};
            text = ((TextData)ThryEditor.currentlyDrawing.property_data).text;
            selectedAlphabet = ((TextData)ThryEditor.currentlyDrawing.property_data).selectedAlphabet;

            string[] guids = AssetDatabase.FindAssets("alphabet t:texture");
            List<string> alphabetList = new List<string>();
            for (int i = 0; i < guids.Length; i++)
            {
                string p = AssetDatabase.GUIDToAssetPath(guids[i]);
                int index = p.LastIndexOf("/")+1;
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
                foreach(Material m in ThryEditor.currentlyDrawing.materials)
                    Helper.SaveValueToFile(m.name + ":" + prop.name, text, TEXT_INFO_FILE_PATH);
                ThryEditor.currentlyDrawing.property_data = new TextData { text = text, selectedAlphabet = selectedAlphabet };
                prop.textureValue = Helper.TextToTexture(text, alphabets[selectedAlphabet]);
                Debug.Log("text '" + text + "' saved as texture.");
            }

            EditorGUI.BeginChangeCheck();
            editor.TexturePropertyMiniThumbnail(position, prop, "", "");
            if (EditorGUI.EndChangeCheck())
            {
                if (prop.textureValue.name.StartsWith("text_"))
                    text = prop.textureValue.name.Replace("text_", "").Replace("_"," ");
                else
                    text = "<texture>";
                ThryEditor.currentlyDrawing.property_data = text;
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
            foreach(Material m in ThryEditor.currentlyDrawing.materials)
            {
                Helper.SaveValueToFileKeyIsRegex(Regex.Escape(m.name)+@".*" ,"", TEXT_INFO_FILE_PATH);
            }
        }
    }

    public class MyToggleDrawer : MaterialPropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            // Setup
            bool value = (prop.floatValue != 0.0f);

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;

            // Show the toggle control
            value = EditorGUI.Toggle(position, label, value);

            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                // Set the new value if it has changed
                prop.floatValue = value ? 1.0f : 0.0f;
            }
        }
    }

    //-------------------------------------------------------------

    public class ThryEditorGuiHelper
    {

        public static void drawConfigTextureProperty(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor, bool scaleOffset)
        {
            if (Config.Get().useBigTextures) drawBigTextureProperty(position, prop, label, editor, scaleOffset);
            else drawSmallTextureProperty(position, prop, label, editor, scaleOffset);
        }

        public static void drawSmallTextureProperty(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor, bool scaleOffset)
        {
            editor.TexturePropertyMiniThumbnail(position, prop, label.text, "Click here for scale / offset" + (label.tooltip != "" ? " | " : "") + label.tooltip);
            if (scaleOffset && DrawingData.currentTexProperty != null)
            {
                if (DrawingData.currentTexProperty.showScaleOffset) ThryEditor.currentlyDrawing.editor.TextureScaleOffsetProperty(prop);
                if (ThryEditor.isMouseClick && position.Contains(Event.current.mousePosition))
                {
                    DrawingData.currentTexProperty.showScaleOffset = !DrawingData.currentTexProperty.showScaleOffset;
                    editor.Repaint();
                }
            }

            DrawingData.lastGuiObjectRect = position;
        }

        public static void drawBigTextureProperty(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor, bool scaleOffset)
        {
            GUILayoutUtility.GetRect(label, bigTextureStyle);
            editor.TextureProperty(position, prop, label.text, label.tooltip, scaleOffset);
            DrawingData.lastGuiObjectRect = position;
        }

        public static GUIStyle m_sectionStyle;
        public static GUIStyle bigTextureStyle;
        public static GUIStyle vectorPropertyStyle;

        //----------Idk what this does-------------
        public static void SetupStyle()
        {
            m_sectionStyle = new GUIStyle(EditorStyles.boldLabel);
            m_sectionStyle.alignment = TextAnchor.MiddleCenter;

            bigTextureStyle = new GUIStyle();
            bigTextureStyle.fixedHeight = 48;

            vectorPropertyStyle = new GUIStyle();
            vectorPropertyStyle.padding = new RectOffset(0, 0, 2, 2);
        }

        //draw the render queue selector
        public static int drawRenderQueueSelector(Shader defaultShader, int customQueueFieldInput)
        {
            EditorGUILayout.BeginHorizontal();
            if (customQueueFieldInput == -1) customQueueFieldInput = ThryEditor.currentlyDrawing.materials[0].renderQueue;
            int[] queueOptionsQueues = new int[] { defaultShader.renderQueue, 2000, 2450, 3000, customQueueFieldInput };
            string[] queueOptions = new string[] { "From Shader", "Geometry", "Alpha Test", "Transparency" };
            int queueSelection = 4;
            if (defaultShader.renderQueue == customQueueFieldInput) queueSelection = 0;
            else
            {
                string customOption = null;
                int q = customQueueFieldInput;
                if (q < 2000) customOption = queueOptions[1] + "-" + (2000 - q);
                else if (q < 2450) { if (q > 2000) customOption = queueOptions[1] + "+" + (q - 2000); else queueSelection = 1; }
                else if (q < 3000) { if (q > 2450) customOption = queueOptions[2] + "+" + (q - 2450); else queueSelection = 2; }
                else if (q < 5001) { if (q > 3000) customOption = queueOptions[3] + "+" + (q - 3000); else queueSelection = 3; }
                if (customOption != null) queueOptions = new string[] { "From Shader", "Geometry", "Alpha Test", "Transparency", customOption };
            }
            EditorGUILayout.LabelField("Render Queue", GUILayout.ExpandWidth(true));
            int newQueueSelection = EditorGUILayout.Popup(queueSelection, queueOptions, GUILayout.MaxWidth(100));
            int newQueue = queueOptionsQueues[newQueueSelection];
            if (queueSelection != newQueueSelection) customQueueFieldInput = newQueue;
            int newCustomQueueFieldInput = EditorGUILayout.IntField(customQueueFieldInput, GUILayout.MaxWidth(65));
            bool isInput = customQueueFieldInput != newCustomQueueFieldInput || queueSelection != newQueueSelection;
            customQueueFieldInput = newCustomQueueFieldInput;
            foreach (Material m in ThryEditor.currentlyDrawing.materials)
                if (customQueueFieldInput != m.renderQueue && isInput) m.renderQueue = customQueueFieldInput;
            if (customQueueFieldInput != ThryEditor.currentlyDrawing.materials[0].renderQueue && !isInput) customQueueFieldInput = ThryEditor.currentlyDrawing.materials[0].renderQueue;
            EditorGUILayout.EndHorizontal();
            return customQueueFieldInput;
        }

        //draw all collected footers
        public static void drawFooters(List<string> footers)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Space(2);
            foreach (string footNote in footers)
            {
                drawFooter(footNote);
                GUILayout.Space(2);
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        //draw single footer
        private static void drawFooter(string data)
        {
            string[] splitNote = data.TrimEnd(')').Split("(".ToCharArray(), 2);
            string value = splitNote[1];
            string type = splitNote[0];
            if (type == "linkButton")
            {
                string[] values = value.Split(",".ToCharArray());
                drawLinkButton(70, 20, values[0], values[1]);
            }
        }

        //draw a button with a link
        private static void drawLinkButton(int Width, int Height, string title, string link)
        {
            if (GUILayout.Button(title, GUILayout.Width(Width), GUILayout.Height(Height)))
            {
                Application.OpenURL(link);
            }
        }

        public static void DrawHeader(ref bool enabled, ref bool options, GUIContent name)
        {
            var r = EditorGUILayout.BeginHorizontal("box");
            enabled = EditorGUILayout.Toggle(enabled, EditorStyles.radioButton, GUILayout.MaxWidth(15.0f));
            options = GUI.Toggle(r, options, GUIContent.none, new GUIStyle());
            EditorGUILayout.LabelField(name, m_sectionStyle);
            EditorGUILayout.EndHorizontal();
        }

        public static void DrawMasterLabel(string shaderName)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.richText = true;
            style.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.LabelField("<size=16>" + shaderName + "</size>", style, GUILayout.MinHeight(18));
        }
    }

    //-----------------------------------------------------------------

    public class ThryEditorHeader
    {
        private List<MaterialProperty> propertyes;
        private bool currentState;

        public ThryEditorHeader(MaterialEditor materialEditor, string propertyName)
        {
            this.propertyes = new List<MaterialProperty>();
            foreach (Material materialEditorTarget in materialEditor.targets)
            {
                Object[] asArray = new Object[] { materialEditorTarget };
                propertyes.Add(MaterialEditor.GetMaterialProperty(asArray, propertyName));
            }

            this.currentState = fetchState();
        }

        public bool fetchState()
        {
            foreach (MaterialProperty materialProperty in propertyes)
            {
                if (materialProperty.floatValue == 1)
                    return true;
            }



            return false;
        }

        public bool getState()
        {
            return this.currentState;
        }

        public void Toggle()
        {

            if (getState())
            {
                foreach (MaterialProperty materialProperty in propertyes)
                {
                    materialProperty.floatValue = 0;
                }
            }
            else
            {
                foreach (MaterialProperty materialProperty in propertyes)
                {
                    materialProperty.floatValue = 1;
                }
            }

            this.currentState = !this.currentState;
        }

        public void Foldout(int xOffset, GUIContent content, ThryEditor gui)
        {
            var style = new GUIStyle("ShurikenModuleTitle");
            style.font = new GUIStyle(EditorStyles.label).font;
            style.border = new RectOffset(15, 7, 4, 4);
            style.fixedHeight = 22;
            style.contentOffset = new Vector2(20f, -2f);
            style.margin.left = 30 * xOffset;

            var rect = GUILayoutUtility.GetRect(16f + 20f, 22f, style);
            DrawingData.lastGuiObjectRect = rect;
            GUI.Box(rect, content, style);

            var e = Event.current;

            var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
            if (e.type == EventType.Repaint)
            {
                EditorStyles.foldout.Draw(toggleRect, false, false, getState(), false);
            }

            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition)&&!e.alt)
            {
                this.Toggle();
                e.Use();
            }
        }
    }
}