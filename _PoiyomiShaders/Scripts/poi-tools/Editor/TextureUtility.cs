using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

//Made by Dreadrith#3238
//Server: https://discord.gg/ZsPfrGn
//Github: https://github.com/Dreadrith/DreadScripts
//Gumroad: https://gumroad.com/dreadrith

namespace Poi.TextureUtility
{
    public class TextureUtility : EditorWindow
    {
        private readonly string[] DimensionPresets = new string[]
        {
            "128x128",
            "256x256",
            "512x512",
            "1024x1024",
            "2048x2048",
            "4096x4096",
        };

        private static GUIContent resetIcon;
        private static Texture2D titleTexture;

        private static Texture2D mainTexture;
        private static Texture2D maskTexture;

        private static int jpgQuality = 75;
        private static int texWidth, texHeight;
        private static bool copyImport = true;
        private static bool pingTexture = true;

        private static bool rotating;
        private static TexRotation rotationType;

        private static bool inverting;
        private static bool maskInvert=true;
        private static bool invertRedS = true, invertGreenS = true, invertBlueS = true, invertAlphaS;

        private static bool unpacking=true,packing;
        private static bool editingTab=true, packingTab,creatingTab;

        
        private static ChannelTexture redChannel = new ChannelTexture("Red", 0);
        private static ChannelTexture greenChannel = new ChannelTexture("Green", 1);
        private static ChannelTexture blueChannel = new ChannelTexture("Blue", 2);
        private static ChannelTexture alphaChannel = new ChannelTexture("Alpha", 0);
        private static ChannelTexture[] channelTextures = new ChannelTexture[] {redChannel,greenChannel,blueChannel,alphaChannel };

        private static bool hueShifting;
        private static bool maskHueShift=true;
        private static float hueShiftFloat;

        private static bool saturating;
        private static bool maskSaturate=true;
        private static float saturationFloat;

        private static bool colorizing;
        private static bool maskColorize=true;
        private static bool textureColorize;
        private static bool alphaColorize;
        private static float colorizeFloat=0.5f;
        private static Color colorizeColor = Color.black;
        private static Texture2D colorizeTexture;

        private static Color originalGUIColor;

        private static TexEncoding encoding = TexEncoding.SaveAsPNG;
        public enum TexEncoding
        {
            SaveAsPNG,
            SaveAsJPG,
            SaveAsTGA
        }

        public enum TexRotation
        {
            Clockwise90,
            CClockwise90,
            Rotate180,
            FlipHorizontal,
            FlipVertical
        }

        #region Creating Tab Variables
        private static bool creatingCustomSize;
        private static bool creatingReverse;
        private static string creatingPath;
        private static Color solidColor=Color.black;
        private static Gradient gradientColor = new Gradient() { colorKeys = new GradientColorKey[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.black, 1) } };

        private static TextureCreatingMode creatingMode = TextureCreatingMode.SolidColor;

        private enum TextureCreatingMode
        {
            SolidColor,
            HorizontalGradient,
            VerticalGradient
        }
        #endregion

        [MenuItem("Poi/Texture Utility")]
        private static void showWindow()
        {
            EditorWindow w = GetWindow<TextureUtility>(false, "Texture Utility", true);
            if (!titleTexture)
            {
                titleTexture = GetColors((Texture2D)EditorGUIUtility.IconContent("Texture2D Icon").image, 16, 16, out _);
                titleTexture.Apply();
            }

            w.titleContent.image = titleTexture;
            w.minSize = new Vector2(423, 253);
        }

        private void OnGUI()
        {
            originalGUIColor = GUI.backgroundColor;
            using (new GUILayout.HorizontalScope())
            {
                bool c = editingTab;

                SetColorIcon(editingTab);
                editingTab = GUILayout.Toggle(editingTab, "Editing", "toolbarbutton");
                if (!c && editingTab)
                {
                    packingTab = false;
                    creatingTab = false;
                }

                c = creatingTab;


                SetColorIcon(creatingTab);
                creatingTab = GUILayout.Toggle(creatingTab, "Creating", "toolbarbutton");
                if (!c && creatingTab)
                {
                    packingTab = false;
                    editingTab = false;
                }

                c = packingTab;

                SetColorIcon(packingTab);
                packingTab = GUILayout.Toggle(packingTab, "Packing", "toolbarbutton");
                if (!c && packingTab)
                {
                    editingTab = false;
                    creatingTab = false;
                }
                GUI.backgroundColor = originalGUIColor;
            }

            if (editingTab)
            {
                DrawEditingTab();
            }

            if (creatingTab)
            {
                DrawCreatingTab();
            }

            if (packingTab)
            {
                DrawPackingTab();
            }
            Credit();
        }

        
        private void DrawEditingTab()
        {
            using (new GUILayout.HorizontalScope())
            {
                using (new GUILayout.VerticalScope())
                {
                    using (new GUILayout.HorizontalScope("box"))
                        DrawDimensionsGUI();

                    using (new GUILayout.HorizontalScope("box"))
                    {
                        encoding = (TexEncoding)EditorGUILayout.EnumPopup(encoding, GUILayout.Width(95));

                        EditorGUI.BeginDisabledGroup(encoding != TexEncoding.SaveAsJPG);
                        EditorGUIUtility.labelWidth = 50;
                        jpgQuality = EditorGUILayout.IntSlider("Quality", jpgQuality, 1, 100);
                        EditorGUIUtility.labelWidth = 0;
                        EditorGUI.EndDisabledGroup();
                    }

                    using (new GUILayout.HorizontalScope("box"))
                    {
                        copyImport = EditorGUILayout.Toggle("Copy Import Settings", copyImport);
                        pingTexture = EditorGUILayout.Toggle(new GUIContent("Highlight Texture", "Highlight the newly created texture in Assets"), pingTexture);
                    }

                    using (new GUILayout.HorizontalScope("box"))
                    {
                        if (!rotating)
                        {
                            SetColorIcon(rotating);
                            rotating = GUILayout.Toggle(rotating, "Rotate", "toolbarbutton");
                            GUI.backgroundColor = originalGUIColor;
                        }
                        else
                        {
                            SetColorIcon(rotating);
                            rotating = GUILayout.Toggle(rotating, "", "toolbarbutton", GUILayout.Width(17), GUILayout.Height(17));
                            GUI.backgroundColor = originalGUIColor;

                            EditorGUI.BeginDisabledGroup(true);
                            GUILayout.Toggle(true, "M", EditorStyles.miniButton, GUILayout.Width(21), GUILayout.Height(16));
                            EditorGUI.EndDisabledGroup();

                            GUILayout.Label("Rotate");
                            rotationType = (TexRotation)EditorGUILayout.EnumPopup(GUIContent.none, rotationType);
                        }
                    }

                    using (new GUILayout.HorizontalScope("box"))
                    {
                        if (!inverting)
                        {
                            SetColorIcon(inverting);
                            inverting = GUILayout.Toggle(inverting, "Invert", "toolbarbutton");
                            GUI.backgroundColor = originalGUIColor;
                        }
                        else
                        {
                            SetColorIcon(inverting);
                            inverting = GUILayout.Toggle(inverting, "", "toolbarbutton", GUILayout.Width(17), GUILayout.Height(17));
                            GUI.backgroundColor = originalGUIColor;

                            maskInvert = GUILayout.Toggle(maskInvert, new GUIContent("M", "Use Mask"), EditorStyles.miniButton, GUILayout.Width(21), GUILayout.Height(16)); GUILayout.Label("Invert");
                            invertRedS = EditorGUILayout.ToggleLeft("R", invertRedS, GUILayout.Width(30));
                            invertGreenS = EditorGUILayout.ToggleLeft("G", invertGreenS, GUILayout.Width(30));
                            invertBlueS = EditorGUILayout.ToggleLeft("B", invertBlueS, GUILayout.Width(30));
                            invertAlphaS = EditorGUILayout.ToggleLeft("A", invertAlphaS, GUILayout.Width(30));

                        }
                    }

                    using (new GUILayout.HorizontalScope("box"))
                    {
                        if (!saturating)
                        {
                            SetColorIcon(saturating);
                            saturating = GUILayout.Toggle(saturating, "Saturate", "toolbarbutton");
                            GUI.backgroundColor = originalGUIColor;
                        }
                        else
                        {
                            SetColorIcon(saturating);
                            saturating = GUILayout.Toggle(saturating, "", "toolbarbutton", GUILayout.Width(17), GUILayout.Height(17));
                            GUI.backgroundColor = originalGUIColor;
                            maskSaturate = GUILayout.Toggle(maskSaturate, new GUIContent("M", "Use Mask"), EditorStyles.miniButton, GUILayout.Width(21), GUILayout.Height(16));
                            EditorGUIUtility.labelWidth = 65;
                            saturationFloat = EditorGUILayout.Slider("Saturate", saturationFloat, -1, 1);
                            EditorGUIUtility.labelWidth = 0;
                        }
                    }
                    using (new GUILayout.HorizontalScope("box"))
                    {
                        if (!hueShifting)
                        {
                            SetColorIcon(hueShifting);
                            hueShifting = GUILayout.Toggle(hueShifting, "Hue Shift", "toolbarbutton");
                            GUI.backgroundColor = originalGUIColor;
                        }
                        else
                        {
                            SetColorIcon(hueShifting);
                            hueShifting = GUILayout.Toggle(hueShifting, "", "toolbarbutton", GUILayout.Width(17), GUILayout.Height(17));
                            GUI.backgroundColor = originalGUIColor;

                            maskHueShift = GUILayout.Toggle(maskHueShift, new GUIContent("M", "Use Mask"), EditorStyles.miniButton, GUILayout.Width(21), GUILayout.Height(16));
                            EditorGUIUtility.labelWidth = 65;
                            hueShiftFloat = EditorGUILayout.Slider("Hue Shift", hueShiftFloat, 0, 1);
                            EditorGUIUtility.labelWidth = 0;
                        }
                    }

                    using (new GUILayout.HorizontalScope("box"))
                    {
                        if (!colorizing)
                        {
                            SetColorIcon(colorizing);
                            colorizing = GUILayout.Toggle(colorizing, "Colorize", "toolbarbutton");
                            GUI.backgroundColor = originalGUIColor;
                        }
                        else
                        {
                            SetColorIcon(colorizing);
                            colorizing = GUILayout.Toggle(colorizing, "", "toolbarbutton", GUILayout.Width(17), GUILayout.Height(17));
                            GUI.backgroundColor = originalGUIColor;

                            maskColorize = GUILayout.Toggle(maskColorize, new GUIContent("M", "Use Mask"), EditorStyles.miniButton, GUILayout.Width(21), GUILayout.Height(16));
                            EditorGUIUtility.labelWidth = 65;
                            colorizeFloat = EditorGUILayout.Slider("Colorize", colorizeFloat, 0, 1);
                            EditorGUIUtility.labelWidth = 0;
                            if (!textureColorize)
                                colorizeColor = EditorGUILayout.ColorField(new GUIContent(""), colorizeColor, true, alphaColorize, false, GUILayout.Width(70), GUILayout.Height(17));
                            else
                                colorizeTexture = (Texture2D)EditorGUILayout.ObjectField(colorizeTexture, typeof(Texture2D), false, GUILayout.Width(70), GUILayout.Height(17));
                            textureColorize = GUILayout.Toggle(textureColorize, new GUIContent("T", "Use Texture"), EditorStyles.miniButton, GUILayout.Width(19), GUILayout.Height(16));
                            alphaColorize = GUILayout.Toggle(alphaColorize, new GUIContent("A", "Use Alpha"), EditorStyles.miniButton, GUILayout.Width(19), GUILayout.Height(16));
                        }
                    }

                }
                using (new GUILayout.VerticalScope())
                {
                    using (new GUILayout.VerticalScope("box"))
                    {
                        EditorGUIUtility.labelWidth = 1;
                        GUILayout.Label("Main", GUILayout.Width(65));
                        EditorGUI.BeginChangeCheck();
                        mainTexture = (Texture2D)EditorGUILayout.ObjectField("", mainTexture, typeof(Texture2D), false, GUILayout.Width(65));
                        if (EditorGUI.EndChangeCheck())
                            ResetDimensions();
                        EditorGUIUtility.labelWidth = 0;
                    }
                    EditorGUI.BeginDisabledGroup(!(hueShifting || saturating || inverting || colorizing));
                    using (new GUILayout.VerticalScope("box"))
                    {
                        EditorGUIUtility.labelWidth = 1;
                        GUILayout.Label("Mask", GUILayout.Width(65));
                        maskTexture = (Texture2D)EditorGUILayout.ObjectField("", maskTexture, typeof(Texture2D), false, GUILayout.Width(65));
                        EditorGUIUtility.labelWidth = 0;
                    }
                    EditorGUI.EndDisabledGroup();
                }
            }
            EditorGUI.BeginDisabledGroup(!mainTexture);
            if (GUILayout.Button("Apply"))
            {
                ApplyTexture();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void DrawCreatingTab()
        {
            using (new GUILayout.HorizontalScope("box"))
            {
                if (!creatingCustomSize)
                {
                    SetColorIcon(creatingCustomSize);
                    creatingCustomSize = GUILayout.Toggle(inverting, "Custom Dimensions", "toolbarbutton");
                    GUI.backgroundColor = originalGUIColor;
                }
                else
                {
                    SetColorIcon(creatingCustomSize);
                    creatingCustomSize = GUILayout.Toggle(creatingCustomSize, "", "toolbarbutton", GUILayout.Width(17), GUILayout.Height(17));
                    GUI.backgroundColor = originalGUIColor;

                    DrawDimensionsGUI(false);
                }
            }

            using (new GUILayout.HorizontalScope("box"))
            {
                encoding = (TexEncoding)EditorGUILayout.EnumPopup(encoding);

                EditorGUI.BeginDisabledGroup(encoding != TexEncoding.SaveAsJPG);
                EditorGUIUtility.labelWidth = 50;
                jpgQuality = EditorGUILayout.IntSlider("Quality", jpgQuality, 1, 100);
                EditorGUIUtility.labelWidth = 0;
                EditorGUI.EndDisabledGroup();
            }

            using (new GUILayout.HorizontalScope("box"))
            {
                pingTexture = EditorGUILayout.Toggle(new GUIContent("Highlight Texture", "Highlight the newly created texture in Assets"), pingTexture);

                EditorGUI.BeginDisabledGroup(creatingMode != TextureCreatingMode.HorizontalGradient && creatingMode != TextureCreatingMode.VerticalGradient);
                creatingReverse = EditorGUILayout.Toggle("Reverse Texture", creatingReverse);
                EditorGUI.EndDisabledGroup();
            }

            using (new GUILayout.HorizontalScope("box"))
            {
                creatingMode = (TextureCreatingMode)EditorGUILayout.EnumPopup("Texture Mode", creatingMode);
            }

            switch ((int)creatingMode)
            {
                case 0:
                    solidColor = EditorGUILayout.ColorField(solidColor);
                    break;
                case 1:
                case 2:
                    gradientColor = EditorGUILayout.GradientField(gradientColor);
                    break;
            }
            if (GUILayout.Button("Create"))
            {
                CreateTexture();
            }
            AssetFolderPath(ref creatingPath, "Save To", "TextureUtilityCreatingPath");
        }

        private void DrawPackingTab()
        {

            using (new GUILayout.HorizontalScope("box"))
            {
                encoding = (TexEncoding)EditorGUILayout.EnumPopup(encoding);
                EditorGUI.BeginDisabledGroup(encoding != TexEncoding.SaveAsJPG);
                EditorGUIUtility.labelWidth = 50;
                jpgQuality = EditorGUILayout.IntSlider("Quality", jpgQuality, 1, 100);
                EditorGUIUtility.labelWidth = 0;
                EditorGUI.EndDisabledGroup();
            }
            using (new GUILayout.HorizontalScope("box"))
            {
                copyImport = EditorGUILayout.Toggle("Copy Import Settings", copyImport);
                pingTexture = EditorGUILayout.Toggle(new GUIContent("Highlight Texture", "Highlight the newly created texture in Assets"), pingTexture);
            }
            using (new GUILayout.HorizontalScope())
            {
                bool p = unpacking;
                SetColorIcon(unpacking);
                unpacking = GUILayout.Toggle(unpacking, "Unpack", "toolbarbutton");
                if (!p && unpacking)
                    packing = false;

                p = packing;
                SetColorIcon(packing);
                packing = GUILayout.Toggle(packing, "Pack", "toolbarbutton");
                if (!p && packing)
                    unpacking = false;

                GUI.backgroundColor = originalGUIColor;
            }
            if (packing)
            {
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUIUtility.labelWidth = 1;
                    redChannel.DrawGUI();
                    greenChannel.DrawGUI();
                    blueChannel.DrawGUI();
                    alphaChannel.DrawGUI();
                    EditorGUIUtility.labelWidth = 0;
                }
                EditorGUI.BeginDisabledGroup(!channelTextures.Any(c => c.texture));
                if (GUILayout.Button("Pack"))
                {
                    PackTexture(channelTextures);
                }
            }
            if (unpacking)
            {

                using (new GUILayout.VerticalScope("box"))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("Main Texture");
                        GUILayout.FlexibleSpace();
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        EditorGUIUtility.labelWidth = 1;
                        mainTexture = (Texture2D)EditorGUILayout.ObjectField("", mainTexture, typeof(Texture2D), false, GUILayout.Width(66));
                        EditorGUIUtility.labelWidth = 0;
                        GUILayout.FlexibleSpace();
                    }
                }
                EditorGUI.BeginDisabledGroup(!mainTexture);
                if (GUILayout.Button("Unpack"))
                {
                    UnpackTexture();
                }
                EditorGUI.EndDisabledGroup();
            }


            EditorGUI.EndDisabledGroup();
        }

        private void DrawDimensionsGUI(bool drawReset=true)
        {
            GUIStyle iconStyle = new GUIStyle(GUI.skin.label) { padding = new RectOffset(), margin = new RectOffset(), imagePosition = ImagePosition.ImageOnly };
            
                EditorGUI.BeginDisabledGroup(!mainTexture && !creatingTab);
                if (drawReset)
                {
                    if (GUILayout.Button(resetIcon, iconStyle, GUILayout.Height(16), GUILayout.Width(16)))
                        ResetDimensions();
                }
                EditorGUIUtility.labelWidth = 20;
                texWidth = EditorGUILayout.IntField(new GUIContent("W","Width"), texWidth);
                texHeight = EditorGUILayout.IntField(new GUIContent("H", "Height"), texHeight);
                EditorGUIUtility.labelWidth = 0;

                int dummy = -1;
                EditorGUI.BeginChangeCheck();
                dummy = EditorGUILayout.Popup(dummy, DimensionPresets,GUILayout.Width(17));
                if (EditorGUI.EndChangeCheck())
                {
                string[] dimensions = ((string)DimensionPresets.GetValue(dummy)).Split('x');
                texWidth = int.Parse(dimensions[0]);
                texHeight = int.Parse(dimensions[1]);
                }

                EditorGUI.EndDisabledGroup();
            
        }

        public static Texture2D GetColors(Texture2D texture, out Color[] Colors, bool unloadTempTexture = false)
        {
            return GetColors(texture, texture.width, texture.height, out Colors, unloadTempTexture);
        }

        public static Texture2D GetColors(Texture2D texture, int width, int height, out Color[] Colors,bool unloadTempTexture = false)
        {
            //Thanks to
            //https://gamedev.stackexchange.com/questions/92285/unity3d-resize-texture-without-corruption
            texture.filterMode = FilterMode.Point;
            RenderTexture rt = RenderTexture.GetTemporary(width, height);
            
            rt.filterMode = FilterMode.Point;
            RenderTexture.active = rt;
            Graphics.Blit(texture, rt);
            Texture2D newTexture = new Texture2D(width, height);
            newTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            Color[] myColors = newTexture.GetPixels();
            RenderTexture.active = null;
            /////////////////////
            Colors = myColors;
            if (unloadTempTexture)
            {
                DestroyImmediate(newTexture);
                return null;
            }
            return newTexture;
        }

        private static void SaveTexture(byte[] textureEncoding, string path, bool refresh=false, bool ping=false)
        {
            System.IO.FileStream stream = System.IO.File.Create(path);
            stream.Write(textureEncoding, 0, textureEncoding.Length);
            stream.Dispose();
            if (refresh)
            {
                AssetDatabase.Refresh();
                if (ping)
                {
                    Ping(path);
                }
            }

        }
        private static void Ping(string path)
        {
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(path));
        }

        private void ApplyTexture()
        {
            if (colorizing && !colorizeTexture && textureColorize)
            {
                Debug.LogError("Cannot Colorize using a texture without a texture!");
                return;
            }

            string destinationPath = GetDestinationFolder(mainTexture);
            string texPath = AssetDatabase.GetAssetPath(mainTexture);

            Texture2D newTexture = GetColors(mainTexture, texWidth, texHeight, out Color[] myColors);

            if (rotating)
            {
                List<Color> rotatedColors = new List<Color>();
                switch (rotationType)
                {
                    case TexRotation.Clockwise90:
                        for (int i = texWidth-1; i >=0; i--)
                        {
                            rotatedColors.AddRange(newTexture.GetPixels(i, 0, 1, texHeight));
                        }
                        myColors = rotatedColors.ToArray();
                        newTexture = new Texture2D(texHeight, texWidth);
                        break;

                    case TexRotation.CClockwise90:
                        for (int i = 0; i < texWidth; i++)
                        {
                            rotatedColors.AddRange(ReverseArray(newTexture.GetPixels(i, 0, 1, texHeight)));
                        }
                        myColors = rotatedColors.ToArray();
                        newTexture = new Texture2D(texHeight, texWidth);
                        break;

                    case TexRotation.Rotate180:
                        myColors = ReverseArray(myColors);
                        break;

                    case TexRotation.FlipHorizontal:
                        for (int i = 0; i < texHeight; i++)
                        {
                            rotatedColors.AddRange(ReverseArray(newTexture.GetPixels(0, i, texWidth, 1)));
                        }
                        myColors = rotatedColors.ToArray();
                        break;

                    case TexRotation.FlipVertical:
                        for (int i = texHeight - 1; i >= 0; i--)
                        {
                            rotatedColors.AddRange(newTexture.GetPixels(0, i, texWidth, 1));
                        }
                        myColors = rotatedColors.ToArray();
                        break;
                }
                
            }

            bool colorInverting = (invertRedS || invertGreenS || invertBlueS || invertAlphaS) && inverting;
            bool HSVEditing = hueShifting || saturating;
            bool colorEditing = HSVEditing || colorizing;
            bool editing = colorEditing || colorInverting || unpacking;
            bool masking = ((maskColorize && colorizing) || (maskInvert && colorInverting) || (maskSaturate && saturating) || (maskHueShift && hueShifting)) && maskTexture;

            Color[] maskColors;
            if (masking)
            {
                GetColors(maskTexture, texWidth, texHeight, out maskColors, true);
            }
            else
                maskColors = null;

            Color[] colorizeTextureColors;
            if (colorizing && textureColorize)
            {
                GetColors(colorizeTexture, texWidth, texHeight, out colorizeTextureColors, true);
            }
            else
                colorizeTextureColors = null;


            Color[] newColors = new Color[myColors.Length];
            if (editing)
            {
                for (int i = 0; i < myColors.Length; i++)
                {
                    Color currentColor = myColors[i];

                    if (colorEditing)
                    {
                        if (HSVEditing)
                        {
                            Color.RGBToHSV(currentColor, out float h, out float s, out float v);
                            currentColor = Color.HSVToRGB(hueShifting ? (Mathf.Repeat(h + (hueShiftFloat * (maskTexture && maskHueShift ? maskColors[i].r : 1)), 1)) : h, saturating ? (Mathf.Clamp01(s + (saturationFloat * (maskTexture && maskSaturate ? maskColors[i].r : 1)))) : s, v);
                            currentColor.a = myColors[i].a;
                        }
                        if (colorizing)
                        {
                            float oga = currentColor.a;
                            currentColor = Color.Lerp(currentColor, textureColorize ? colorizeTextureColors[i] : colorizeColor, colorizeFloat * (maskColorize && maskTexture ? maskColors[i].r : 1));

                            if (!alphaColorize)
                                currentColor.a = oga;
                        }
                    }

                    float r = colorInverting && invertRedS ? currentColor.r - ((currentColor.r - (1 - currentColor.r)) * (maskInvert && maskTexture ? maskColors[i].r : 1)) : currentColor.r;
                    float g = colorInverting && invertGreenS ? currentColor.g - ((currentColor.g - (1 - currentColor.g)) * (maskInvert && maskTexture ? maskColors[i].g : 1)) : currentColor.g;
                    float b = colorInverting && invertBlueS ? currentColor.b - ((currentColor.b - (1 - currentColor.b)) * (maskInvert && maskTexture ? maskColors[i].b : 1)) : currentColor.b;
                    float a = colorInverting && invertAlphaS ? currentColor.a - ((currentColor.a - (1 - currentColor.a)) * (maskInvert && maskTexture ? maskColors[i].a : 1)) : currentColor.a;

                    newColors[i] = new Color(r, g, b, a);
                }
            }
            newTexture.SetPixels(editing ? newColors : myColors);
            newTexture.Apply();

            GetEncoding(newTexture, encoding, out byte[] data, out string ext);

            string newTexturePath = AssetDatabase.GenerateUniqueAssetPath(destinationPath + "/" + mainTexture.name
                + (colorInverting ? " Inverted" : "") + ext);
            
            SaveTexture(data, newTexturePath, true, pingTexture);

            if (copyImport)
            {
                CopyTextureSettings(texPath, newTexturePath);
            }
        }

        private static void GetEncoding(Texture2D texture, TexEncoding encodingType, out byte[] data, out string ext)
        {
            switch ((int)encodingType)
            {
                default:
                    ext = ".png";
                    data = texture.EncodeToPNG();
                    break;
                case 1:
                    ext = ".jpg";
                    data = texture.EncodeToJPG(jpgQuality);
                    break;
                case 2:
                    ext = ".tga";
                    data = texture.EncodeToTGA();
                    break;
            }
        }


        private void CreateTexture()
        {
            Texture2D newTexture = null;
            int w = creatingCustomSize ? texWidth : 0;
            int h = creatingCustomSize ? texHeight : 0;

            Color[] myColors = null;
            switch ((int)creatingMode)
            {
                case 0:
                    if (!creatingCustomSize)
                    {
                        w = h = 4;
                    }
                    newTexture = new Texture2D(w, h);

                    myColors = CreateFilledArray(solidColor, w * h);
                    newTexture.SetPixels(0, 0, w, h, myColors);
                    break;
                case 1:
                    {
                        if (!creatingCustomSize)
                        {
                            w = 256;
                            h = 4;
                        }
                        newTexture = new Texture2D(w, h);

                        int i = creatingReverse ? w - 1 : 0;
                        int istep = creatingReverse ? -1 : 1;

                        float xstepValue = (1f / w);
                        float xcurrentStep = 0;
                        for (; i < w && i >= 0; i += istep)
                        {
                            newTexture.SetPixels(i, 0, 1, h, CreateFilledArray(gradientColor.Evaluate(xcurrentStep), h));
                            xcurrentStep += xstepValue;
                        }
                    }
                    break;
                case 2:
                    {
                        if (!creatingCustomSize)
                        {
                            w = 4;
                            h = 256;
                        }
                        newTexture = new Texture2D(w, h);

                        int i = creatingReverse ? h - 1 : 0;
                        int istep = creatingReverse ? -1 : 1;

                        float ystepValue = 1f / h;
                        float ycurrentStep = 0;
                        for (; i < h && i >= 0; i += istep)
                        {
                            newTexture.SetPixels(0, i, w, 1, CreateFilledArray(gradientColor.Evaluate(ycurrentStep), w));
                            ycurrentStep += ystepValue;
                        }
                    }
                    break;
            }

            GetEncoding(newTexture, encoding, out byte[] data, out string ext);

            RecreateFolders(creatingPath);
            SaveTexture(data, AssetDatabase.GenerateUniqueAssetPath(creatingPath +"/Generated Texture"+ext), true, pingTexture);
        }

        private void UnpackTexture()
        {
            string destinationPath = GetDestinationFolder(mainTexture);
            string texPath = AssetDatabase.GetAssetPath(mainTexture);
            int x = mainTexture.width, y = mainTexture.height;
            Texture2D newTexture = GetColors(mainTexture, x, y, out Color[] myColors);
            List<System.Tuple<string, string>> copyFromTo = new List<System.Tuple<string, string>>();

            bool isRedPass = true, isGreenPass, isBluePass, isAlphaPass;
            isGreenPass = isBluePass = isAlphaPass = false;
            try
            {
                AssetDatabase.StartAssetEditing();

                do
                {
                    Color[] newColors = new Color[myColors.Length];

                    bool hasAlpha = false;
                    for (int i = 0; i < myColors.Length; i++)
                    {
                        Color currentColor = myColors[i];

                        float r = currentColor.r;
                        float g = currentColor.g;
                        float b = currentColor.b;
                        float a = currentColor.a;

                        if (isRedPass)
                        {
                            g = b = r;
                            a = 1;
                        }
                        if (isGreenPass)
                        {
                            r = b = g;
                            a = 1;
                        }
                        if (isBluePass)
                        {
                            r = g = b;
                            a = 1;
                        }
                        if (isAlphaPass)
                        {
                            r = g = b = a;
                            if (a != 1)
                                hasAlpha = true;
                        }

                        newColors[i] = new Color(r, g, b, a);
                    }

                    if (isAlphaPass && !hasAlpha)
                    {
                        isAlphaPass = false;
                        goto Skip;
                    }

                    newTexture.SetPixels(newColors);
                    newTexture.Apply();

                    GetEncoding(newTexture, encoding, out byte[] data, out string ext);

                    string newTexturePath = AssetDatabase.GenerateUniqueAssetPath(destinationPath + "/" + mainTexture.name
                        + (isRedPass ? "-Red" : isGreenPass ? "-Green" : isBluePass ? "-Blue" : "-Alpha") + ext);

                    SaveTexture(data, newTexturePath);

                    if (copyImport)
                    {
                        copyFromTo.Add(new System.Tuple<string, string>(texPath, newTexturePath));
                    }

                    if (isAlphaPass)
                        isAlphaPass = false;
                    if (isBluePass)
                    {
                        isBluePass = false;
                        isAlphaPass = true;
                    }
                    if (isGreenPass)
                    {
                        isGreenPass = false;
                        isBluePass = true;
                    }
                    if (isRedPass)
                    {
                        isRedPass = false;
                        isGreenPass = true;
                    }

                    if (unpacking)
                        newTexture = new Texture2D(x, y);

                    Skip:;

                } while (isRedPass || isGreenPass || isBluePass || isAlphaPass);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
            AssetDatabase.Refresh();
            if (copyImport)
            {
                for (int i = 0; i < copyFromTo.Count; i++)
                {
                    CopyTextureSettings(copyFromTo[i].Item1, copyFromTo[i].Item2);
                }
            }
        }

        public void PackTexture(ChannelTexture[] channels)
        {
            int firstIndex = 0;
            for (int i = 3; i >= 0; i--)
            {
                if (channels[i].texture)
                    firstIndex = i;
            }
            ChannelTexture firstChannel = channels[firstIndex];
            int w = firstChannel.texture.width;
            int h = firstChannel.texture.height;
            PackTexture(channels, AssetDatabase.GetAssetPath(firstChannel.texture), w, h, encoding);
        }

        public static string PackTexture(ChannelTexture[] channels, TexEncoding encodingType, bool refresh=true, bool copyImportSettings=true)
        {
            int firstIndex = -1;
            for (int i = 3; i >= 0; i--)
            {
                if (channels[i].texture)
                    firstIndex = i;
            }
            if (firstIndex < 0)
                return string.Empty;
            ChannelTexture firstChannel = channels[firstIndex];
            int w = firstChannel.texture.width;
            int h = firstChannel.texture.height;
            return PackTexture(channels, AssetDatabase.GetAssetPath(firstChannel.texture), w, h, encodingType,refresh,false,copyImportSettings);
        }

        public static string PackTexture(ChannelTexture[] channels, string destination,int width, int height, TexEncoding encodingType, bool refresh=true,bool overwrite=false, bool copyImportSettings=true)
        {
            int firstIndex = -1;
            for (int i = 3; i >= 0; i--)
            {
                if (channels[i].texture)
                    firstIndex = i;
            }
            if (firstIndex < 0)
                return string.Empty;

            ChannelTexture firstChannel = channels[firstIndex];

            
            Texture2D newTexture = new Texture2D(width, height);
            channels[0].GetChannelColors(width, height, out float[] reds, true);
            channels[1].GetChannelColors(width, height, out float[] greens, true);
            channels[2].GetChannelColors(width, height, out float[] blues, true);
            channels[3].GetChannelColors(width, height, out float[] alphas, true);
            Color[] finalColors = new Color[width*height];

            for (int i=0;i< finalColors.Length;i++)
            {
                finalColors[i].r = (reds!=null) ? reds[i] : 0;
                finalColors[i].g = (greens != null) ? greens[i] : 0;
                finalColors[i].b = (blues != null) ? blues[i] : 0;
                finalColors[i].a = (alphas != null) ? alphas[i] : 1;
            }
            newTexture.SetPixels(finalColors);
            newTexture.Apply();

            GetEncoding(newTexture, encodingType, out byte[] data, out string ext);

            string newTexturePath = GetDestinationFolder(destination)+"/"+System.IO.Path.GetFileNameWithoutExtension(destination)+ext;
            if (!overwrite)
                newTexturePath = AssetDatabase.GenerateUniqueAssetPath(newTexturePath);
            SaveTexture(data, newTexturePath);
            DestroyImmediate(newTexture);
            if (refresh)
                AssetDatabase.Refresh();
            

            if (copyImportSettings)
            {
                CopyTextureSettings(AssetDatabase.GetAssetPath(firstChannel.texture), newTexturePath);
            }
            return newTexturePath;
        }

        private static void CopyTextureSettings(string from, string to)
        {
            TextureImporter source = (TextureImporter)AssetImporter.GetAtPath(from);
            TextureImporterSettings sourceSettings = new TextureImporterSettings();
            source.ReadTextureSettings(sourceSettings);
            
            TextureImporter destination = (TextureImporter)AssetImporter.GetAtPath(to);
            destination.SetTextureSettings(sourceSettings);
            destination.maxTextureSize = source.maxTextureSize;
            destination.textureCompression = source.textureCompression;
            destination.crunchedCompression = source.crunchedCompression;
            destination.SaveAndReimport();
        }

        private static string GetDestinationFolder(Object o)
        {
            string path = AssetDatabase.GetAssetPath(o);
            return GetDestinationFolder(path);
        }
        private static string GetDestinationFolder(string path)
        {
            return path.Substring(0, path.LastIndexOf('/'));
        }

        private void ResetDimensions()
        {
            if (mainTexture)
            {
                texHeight = mainTexture.height;
                texWidth = mainTexture.width;
            }
        }

        private void SetColorIcon(bool value)
        {
            if (value)
                GUI.backgroundColor = Color.green;
            else
                GUI.backgroundColor = Color.grey;
        }

        private void OnEnable()
        {
            resetIcon = new GUIContent(EditorGUIUtility.IconContent("d_Refresh")) { tooltip = "Reset Dimensions" };
            creatingPath = PlayerPrefs.GetString("TextureUtilityCreatingPath", "Assets/DreadScripts/Texture Utility/Generated Assets");

            for (int i=0;i<channelTextures.Length;i++)
            {
                channelTextures[i].SetMode(EditorPrefs.GetInt("TextureUtilityChannel" + channelTextures[i].name, (int)channelTextures[i].mode));
            }
        }

        private static T[] CreateFilledArray<T>(T variable,int length)
        {
            T[] myArray = new T[length];
            for (int i=0;i< myArray.Length;i++)
            {
                myArray[i] = variable;
            }
            return myArray;
        }

        private static T[] ReverseArray<T>(T[] array)
        {
            T[] reversed = new T[array.Length];
            int index = array.Length - 1;
            for (int i = 0; i < reversed.Length; i++)
            {
                reversed[i] = array[index];
                index--;
            }
            return reversed;
        }

        #region Extracted From DS_CommonMethods
        public static void AssetFolderPath(ref string variable, string title, string playerpref)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(title, variable);
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string dummyPath = EditorUtility.OpenFolderPanel(title, variable, "");
                if (string.IsNullOrEmpty(dummyPath))
                    return;

                if (!dummyPath.Contains("Assets"))
                {
                    Debug.LogWarning("New Path must be a folder within Assets!");
                    return;
                }
                variable = FileUtil.GetProjectRelativePath(dummyPath);
                PlayerPrefs.SetString(playerpref, variable);
            }
            EditorGUILayout.EndHorizontal();
        }

        public static void RecreateFolders(string fullPath)
        {
            string[] folderNames = fullPath.Split('/');
            string[] folderPaths = new string[folderNames.Length];
            for (int i = 0; i < folderNames.Length; i++)
            {
                folderPaths[i] = folderNames[0];
                for (int j = 1; j <= i; j++)
                {
                    folderPaths[i] = folderPaths[i] + "/" + folderNames[j];
                }
            }
            for (int i = 0; i < folderPaths.Length; i++)
            {
                if (!AssetDatabase.IsValidFolder(folderPaths[i]))
                {
                    AssetDatabase.CreateFolder(folderPaths[i].Substring(0, folderPaths[i].LastIndexOf('/')), folderPaths[i].Substring(folderPaths[i].LastIndexOf('/') + 1, folderPaths[i].Length - folderPaths[i].LastIndexOf('/') - 1));
                }

            }
        }
        #endregion

        private static void Credit()
        {
            GUIStyle creditLabelStyle = new GUIStyle(GUI.skin.label) { richText = true };
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("<b>Made by Dreadrith#3238</b>",creditLabelStyle))
                {
                    Application.OpenURL("https://github.com/Dreadrith/DreadScripts");
                }
                
            }
        }
    }

    [System.Serializable]
    public class ChannelTexture
    {
        public string name;
        public Texture2D texture;
        public bool invert;
        public ColorMode mode = ColorMode.Red;
        public enum ColorMode
        {
            Red,
            Green,
            Blue,
            Alpha
        }
        public ChannelTexture(string n, int mode)
        {
            name = n;
            SetMode(mode, true);
        }

        public void SetMode(int i, bool ignoreSave = false)
        {
            switch (i)
            {
                case 0:
                    mode = ColorMode.Red;
                    break;
                case 1:
                    mode = ColorMode.Green;
                    break;
                case 2:
                    mode = ColorMode.Blue;
                    break;
                case 3:
                    mode = ColorMode.Alpha;
                    break;
            }
            if (!ignoreSave)
            {
                EditorPrefs.SetInt("TextureUtilityChannel" + name, i);
            }
        }

        public Texture2D GetChannelColors(int width, int height, out float[] colors, bool unloadTempTexture)
        {
            if (!texture)
            {
                colors = null;
                return null;
            }
            else
            {
                Texture2D newTexture = TextureUtility.GetColors(texture, width, height, out Color[] myColors, unloadTempTexture);
                colors = myColors.Select(c =>
                {
                    if (mode == ColorMode.Red)
                        return c.r;
                    if (mode == ColorMode.Green)
                        return c.g;
                    if (mode == ColorMode.Blue)
                        return c.b;

                    return c.a;
                }).ToArray();
                if (invert)
                {
                    for (int i = 0; i < colors.Length; i++)
                    {
                        colors[i] = 1 - colors[i];
                    }
                }
                return newTexture;
            }
        }

        public void DrawGUI()
        {
            GUIStyle buttonGroupStyle = new GUIStyle(GUI.skin.GetStyle("toolbarbutton")) { padding = new RectOffset(1, 1, 1, 1), margin = new RectOffset(0, 0, 1, 1) };
            using (new GUILayout.VerticalScope("box"))
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(name, "boldlabel");
                    GUILayout.FlexibleSpace();
                }
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    bool dummy;
                    EditorGUI.BeginChangeCheck();
                    dummy = GUILayout.Toggle(mode == ColorMode.Red, "R", buttonGroupStyle, GUILayout.Width(16));
                    if (EditorGUI.EndChangeCheck())
                        if (dummy)
                            SetMode(0);

                    EditorGUI.BeginChangeCheck();
                    dummy = GUILayout.Toggle(mode == ColorMode.Green, "G", buttonGroupStyle, GUILayout.Width(16));
                    if (EditorGUI.EndChangeCheck())
                        if (dummy)
                            SetMode(1);

                    EditorGUI.BeginChangeCheck();
                    dummy = GUILayout.Toggle(mode == ColorMode.Blue, "B", buttonGroupStyle, GUILayout.Width(16));
                    if (EditorGUI.EndChangeCheck())
                        if (dummy)
                            SetMode(2);

                    EditorGUI.BeginChangeCheck();
                    dummy = GUILayout.Toggle(mode == ColorMode.Alpha, "A", buttonGroupStyle, GUILayout.Width(16));
                    if (EditorGUI.EndChangeCheck())
                        if (dummy)
                            SetMode(3);
                    GUILayout.FlexibleSpace();
                }
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    texture = (Texture2D)EditorGUILayout.ObjectField("", texture, typeof(Texture2D), false, GUILayout.Width(66));
                    GUILayout.FlexibleSpace();
                }
                invert = GUILayout.Toggle(invert, "Invert", "toolbarbutton");
            }
        }

       
    }
}