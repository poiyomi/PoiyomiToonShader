using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

public class PoiTexturePacker : EditorWindow
{
    // --- Data Structures ---
    public enum SourceMode { Texture, Constant, Gradient }
    public enum ResizingMode { Tile, Stretch, Center }
    public enum GradientType { Linear, Radial }
    public enum BlendMode { Add, Subtract, Multiply, Divide, Average, Min, Max, Overlay }

    [System.Serializable]
    public class ChannelSettings
    {
        public SourceMode sourceMode = SourceMode.Texture;
        public int textureSourceIndex = 0;
        public int textureChannelIndex = 0;
        public float constantValue = 1f;
        public bool invert = false;
        
        public GradientType gradientType = GradientType.Linear;
        public float gradientAngle = 0f;
        public Gradient gradient = new Gradient();
        public BlendMode blendMode = BlendMode.Max; // Blend mode for this layer
    }

    // --- State Variables ---
    private Texture2D[] sourceTextures = new Texture2D[4];
    private List<ChannelSettings> rInputs = new List<ChannelSettings> { new ChannelSettings() };
    private List<ChannelSettings> gInputs = new List<ChannelSettings> { new ChannelSettings() };
    private List<ChannelSettings> bInputs = new List<ChannelSettings> { new ChannelSettings() };
    private List<ChannelSettings> aInputs = new List<ChannelSettings> { new ChannelSettings() { textureChannelIndex = 3 } };

    // Output
    private int resolutionSourceIndex = 0;
    private enum FileFormat { PNG, TGA, JPG, PNG_Optimized }
    private FileFormat outputFormat = FileFormat.PNG;
    private ResizingMode resizingMode = ResizingMode.Stretch;
    private bool isLinear = false;
    private int jpegQuality = 90;
    private int oxiPngLevel = 2;

    // Preview
    private Texture2D[] channelPreviews = new Texture2D[4];
    private bool needsPreviewUpdate = true;

    // --- Caching for Performance ---
    private Texture2D[] cachedReadableTextures = new Texture2D[4];
    private Color32[][] cachedPixels = new Color32[4][];
    private int[] cachedWidths = new int[4];
    private int[] cachedHeights = new int[4];

    // UI Styles
    private GUIStyle headerStyle, centeredLabel;
    private static readonly Color redColor = new Color(1f, 0.7f, 0.7f, 1f);
    private static readonly Color greenColor = new Color(0.7f, 1f, 0.7f, 1f);
    private static readonly Color blueColor = new Color(0.7f, 0.85f, 1f, 1f);
    private static readonly Color alphaColor = new Color(0.85f, 0.85f, 0.85f, 1f);
    private GUIContent clearIcon, resetIcon, addIcon, removeIcon;

    [MenuItem("Poi/Lossless Texture Packer", false, 1000)]
    public static void ShowWindow()
    {
        GetWindow<PoiTexturePacker>("Poi Texture Packer");
    }

    private void OnEnable()
    {
        clearIcon = EditorGUIUtility.IconContent("d_TreeEditor.Trash", "Clear all texture slots");
        resetIcon = EditorGUIUtility.IconContent("d_Refresh", "Reset all channel mappings");
        addIcon = EditorGUIUtility.IconContent("d_Toolbar Plus", "Add new input layer");
        removeIcon = EditorGUIUtility.IconContent("d_Toolbar Minus", "Remove input layer");
        Undo.undoRedoPerformed += OnUndoRedo;
        
        for (int i = 0; i < 4; i++)
        {
            UpdateTextureCache(i);
        }
        needsPreviewUpdate = true;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoRedo;
        for(int i = 0; i < channelPreviews.Length; i++)
        {
            if(channelPreviews[i] != null) DestroyImmediate(channelPreviews[i]);
        }
        ClearTextureCache();
    }

    private void OnUndoRedo()
    {
        needsPreviewUpdate = true;
        Repaint();
    }

    void InitStyles()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(0, 0, 0, 0)
            };
        }
        if (centeredLabel == null)
        {
            centeredLabel = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
        }
    }

    void OnGUI()
    {
        InitStyles();
        
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("Poi Texture Packer", headerStyle);
        EditorGUILayout.Space();
        
        DrawSourceTexturesUI();
        DrawChannelMappingUI();
        DrawOutputSettingsUI();

        EditorGUILayout.Space();

        if (GUILayout.Button("Save Packed Texture", GUILayout.Height(40)))
        {
            PackAndSaveTexture();
        }

        if(EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(this, "Texture Packer Change");
            needsPreviewUpdate = true;
        }

        if(needsPreviewUpdate)
        {
            UpdateChannelPreviews();
            needsPreviewUpdate = false;
        }
    }

    private void DrawSourceTexturesUI()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Source Textures", headerStyle);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(clearIcon, GUILayout.Width(30), GUILayout.Height(24)))
        {
            Undo.RecordObject(this, "Clear All Textures");
            for(int i = 0; i < sourceTextures.Length; i++)
            {
                sourceTextures[i] = null;
                UpdateTextureCache(i);
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < sourceTextures.Length; i++)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField($"Source {i + 1}", GUILayout.Width(80));
            EditorGUI.BeginChangeCheck();
            sourceTextures[i] = (Texture2D)EditorGUILayout.ObjectField("", sourceTextures[i], typeof(Texture2D), false, GUILayout.Width(80), GUILayout.Height(80));
            if(EditorGUI.EndChangeCheck())
            {
                UpdateTextureCache(i);
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.EndVertical();
    }

    private void DrawChannelMappingUI()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Output Channel Mapping", headerStyle);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(resetIcon, GUILayout.Width(30), GUILayout.Height(24)))
        {
            Undo.RecordObject(this, "Reset Mappings");
            rInputs = new List<ChannelSettings> { new ChannelSettings() };
            gInputs = new List<ChannelSettings> { new ChannelSettings() };
            bInputs = new List<ChannelSettings> { new ChannelSettings() };
            aInputs = new List<ChannelSettings> { new ChannelSettings() { textureChannelIndex = 3 } };
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        
        DrawChannelMappingSection("Red", rInputs, redColor, 0);
        DrawChannelMappingSection("Green", gInputs, greenColor, 1);
        DrawChannelMappingSection("Blue", bInputs, blueColor, 2);
        DrawChannelMappingSection("Alpha", aInputs, alphaColor, 3);

        EditorGUILayout.EndVertical();
    }

    private void DrawChannelMappingSection(string label, List<ChannelSettings> inputs, Color color, int previewIndex)
    {
        GUI.backgroundColor = color;
        EditorGUILayout.BeginHorizontal("box");
        
        EditorGUILayout.BeginVertical();
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        
        int indexToRemove = -1; 

        for (int i = 0; i < inputs.Count; i++)
        {
            if (i > 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                inputs[i].blendMode = (BlendMode)EditorGUILayout.EnumPopup(inputs[i].blendMode, GUILayout.Width(120));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            ChannelSettings settings = inputs[i];
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Input {i+1}", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            settings.invert = EditorGUILayout.ToggleLeft("Invert", settings.invert, GUILayout.Width(60));
            if(i > 0)
            {
                if(GUILayout.Button(removeIcon, GUILayout.Width(25), GUILayout.Height(20)))
                {
                    indexToRemove = i;
                }
            }
            EditorGUILayout.EndHorizontal();

            settings.sourceMode = (SourceMode)EditorGUILayout.EnumPopup("Source", settings.sourceMode);
            
            if (settings.sourceMode == SourceMode.Texture)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Texture", GUILayout.Width(60));
                string[] sourceOptions = { "1", "2", "3", "4" };
                settings.textureSourceIndex = GUILayout.SelectionGrid(settings.textureSourceIndex, sourceOptions, 4, EditorStyles.miniButton);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Channel", GUILayout.Width(60));
                string[] channelOptions = { "R", "G", "B", "A" };
                settings.textureChannelIndex = GUILayout.SelectionGrid(settings.textureChannelIndex, channelOptions, 4, EditorStyles.miniButton);
                EditorGUILayout.EndHorizontal();
            }
            else if (settings.sourceMode == SourceMode.Constant)
            {
                settings.constantValue = EditorGUILayout.Slider("Value", settings.constantValue, 0f, 1f);
            }
            else // Gradient
            {
                settings.gradientType = (GradientType)EditorGUILayout.EnumPopup("Type", settings.gradientType);
                settings.gradient = EditorGUILayout.GradientField("Gradient", settings.gradient);
                settings.gradientAngle = EditorGUILayout.Slider("Angle/Rotation", settings.gradientAngle, 0, 360);
            }
            EditorGUILayout.EndVertical();
        }

        if (indexToRemove != -1)
        {
            Undo.RecordObject(this, "Remove Channel Input");
            inputs.RemoveAt(indexToRemove);
        }
        
        if (GUILayout.Button(new GUIContent(" Add Input Layer", addIcon.image)))
        {
            Undo.RecordObject(this, "Add Channel Input");
            inputs.Add(new ChannelSettings());
        }

        EditorGUILayout.EndVertical();

        if(channelPreviews[previewIndex] != null)
        {
            GUILayout.Box(channelPreviews[previewIndex], GUILayout.Width(128), GUILayout.Height(128));
        }
        else
        {
            GUILayout.Space(134); // Reserve space to prevent layout shift
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawOutputSettingsUI()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Output Settings", headerStyle);
        EditorGUILayout.Space();
        
        string[] sourceOptions = { "Source 1", "Source 2", "Source 3", "Source 4" };
        
        resolutionSourceIndex = EditorGUILayout.Popup("Resolution Master", resolutionSourceIndex, sourceOptions);
        resizingMode = (ResizingMode)EditorGUILayout.EnumPopup(new GUIContent("Resizing Mode", "How to handle source textures that are a different size than the Resolution Master."), resizingMode);
        
        isLinear = !EditorGUILayout.Toggle(new GUIContent("sRGB (Color Texture)", "Uncheck this for masks, metallic, roughness, or other non-color data."), !isLinear);

        outputFormat = (FileFormat)EditorGUILayout.EnumPopup("Format", outputFormat);

        if (outputFormat == FileFormat.JPG)
        {
            jpegQuality = EditorGUILayout.IntSlider("JPEG Quality", jpegQuality, 1, 100);
        }
        
        if (outputFormat == FileFormat.PNG_Optimized)
        {
            EditorGUILayout.Space();
            oxiPngLevel = EditorGUILayout.IntSlider("Optimization Level", oxiPngLevel, 0, 6);
            EditorGUILayout.HelpBox("Uses OxiPNG (https://github.com/oxipng/oxipng). Higher levels are slower but provide better compression. Level 2 is a good balance.", MessageType.Info);
        }
        EditorGUILayout.EndVertical();
    }

    private void UpdateChannelPreviews()
    {
        GenerateChannelPreview(rInputs, 0);
        GenerateChannelPreview(gInputs, 1);
        GenerateChannelPreview(bInputs, 2);
        GenerateChannelPreview(aInputs, 3);
        Repaint();
    }

    private void GenerateChannelPreview(List<ChannelSettings> inputs, int previewIndex)
    {
        if(channelPreviews[previewIndex] == null)
        {
            channelPreviews[previewIndex] = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        }
        Texture2D preview = channelPreviews[previewIndex];
        Color[] pixels = new Color[128 * 128];
        for(int i = 0; i < pixels.Length; i++)
        {
            int x = i % 128;
            int y = i / 128;
            byte val = CalculateFinalChannelValue(inputs, x, y, 128, 128);
            float floatVal = val / 255f;
            pixels[i] = new Color(floatVal, floatVal, floatVal, 1);
        }
        preview.SetPixels(pixels);
        preview.Apply();
    }
    
    private void PackAndSaveTexture()
    {
        if (resolutionSourceIndex >= sourceTextures.Length || sourceTextures[resolutionSourceIndex] == null)
        {
            EditorUtility.DisplayDialog("Error", "A valid Resolution Master source texture is required.", "OK");
            return;
        }

        // Ensure cache is up-to-date before saving
        for(int i = 0; i < 4; i++) UpdateTextureCache(i);

        if (cachedReadableTextures[resolutionSourceIndex] == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not read the master texture file. Ensure it is a valid image.", "OK");
            return;
        }
        int outputWidth = cachedWidths[resolutionSourceIndex];
        int outputHeight = cachedHeights[resolutionSourceIndex];

        Texture2D outputTexture = new Texture2D(outputWidth, outputHeight, TextureFormat.RGBA32, false, isLinear);
        PackTextureRaw(outputTexture);

        string extension = outputFormat.ToString().ToLower().Replace("_optimized", "");
        if (outputFormat == FileFormat.PNG_Optimized) extension = "png";
            
        string savePath = EditorUtility.SaveFilePanel("Save Packed Texture", "Assets", "PackedTexture", extension);
            
        if (string.IsNullOrEmpty(savePath))
        {
            DestroyImmediate(outputTexture);
            return;
        }
            
        byte[] bytes;
        if (outputFormat == FileFormat.PNG_Optimized)
        {
            byte[] uncompressedBytes = outputTexture.EncodeToPNG();
            bytes = RunOxiPNG(uncompressedBytes, savePath);
        }
        else
        {
            if (outputFormat == FileFormat.PNG) bytes = outputTexture.EncodeToPNG();
            else if (outputFormat == FileFormat.TGA) bytes = outputTexture.EncodeToTGA();
            else bytes = outputTexture.EncodeToJPG(jpegQuality);
        }
            
        DestroyImmediate(outputTexture);
            
        FinishSaving(bytes, savePath);
    }
    
    private byte[] RunOxiPNG(byte[] inputPngBytes, string savePath)
    {
        MonoScript script = MonoScript.FromScriptableObject(this);
        string scriptPath = AssetDatabase.GetAssetPath(script);
        string scriptDirectory = Path.GetDirectoryName(scriptPath);
        string toolPath = Path.Combine(scriptDirectory, "oxipng.exe");

        if (!File.Exists(toolPath))
        {
            EditorUtility.DisplayDialog("Error", $"OxiPNG executable not found at: {toolPath}\nPlease place 'oxipng.exe' in the same folder as the script.", "OK");
            return null;
        }
        
        string fileName = Path.GetFileName(savePath);
        string tempPath = Path.Combine(Application.temporaryCachePath, fileName);
        File.WriteAllBytes(tempPath, inputPngBytes);

        Process process = new Process();
        process.StartInfo.FileName = toolPath;
        process.StartInfo.Arguments = $"-o {oxiPngLevel} --strip safe \"{tempPath}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        
        byte[] resultBytes = null;
        try
        {
            EditorUtility.DisplayProgressBar("Optimizing PNG with OxiPNG", "Running multithreaded optimization... This may take a moment.", 0.5f);
            process.Start();
            process.WaitForExit(); 
            if (process.ExitCode == 0) resultBytes = File.ReadAllBytes(tempPath);
            else UnityEngine.Debug.LogError($"OxiPNG failed with exit code {process.ExitCode}.");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
        return resultBytes;
    }

    private void FinishSaving(byte[] bytes, string path)
    {
        if (bytes == null) return;
        File.WriteAllBytes(path, bytes);

        if (path.StartsWith(Application.dataPath))
        {
            string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
            AssetDatabase.Refresh();
            TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
            if (importer != null)
            {
                importer.isReadable = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.sRGBTexture = !isLinear; // Apply colorspace setting
                importer.SaveAndReimport();
            }
        }
        EditorUtility.DisplayDialog("Success", $"Texture saved to {path}", "OK");
    }
    
    private void PackTextureRaw(Texture2D targetTexture)
    {
        Color32[] outputPixels = new Color32[targetTexture.width * targetTexture.height];
        
        for (int i = 0; i < outputPixels.Length; i++)
        {
            int x = i % targetTexture.width;
            int y = i / targetTexture.width;
            outputPixels[i] = new Color32(
                CalculateFinalChannelValue(rInputs, x, y, targetTexture.width, targetTexture.height),
                CalculateFinalChannelValue(gInputs, x, y, targetTexture.width, targetTexture.height),
                CalculateFinalChannelValue(bInputs, x, y, targetTexture.width, targetTexture.height),
                CalculateFinalChannelValue(aInputs, x, y, targetTexture.width, targetTexture.height)
            );
        }

        targetTexture.SetPixels32(outputPixels);
        targetTexture.Apply();
    }
    
    private float GetIndividualChannelValue(ChannelSettings settings, int x, int y, int outputWidth, int outputHeight)
    {
        float val = 0;
        if (settings.sourceMode == SourceMode.Constant)
        {
            val = settings.constantValue;
        }
        else if (settings.sourceMode == SourceMode.Gradient)
        {
            float t = 0;
            Vector2 uv = new Vector2((float)x / (outputWidth - 1), (float)y / (outputHeight - 1));

            if (settings.gradientType == GradientType.Linear)
            {
                float angleRad = settings.gradientAngle * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
                t = Vector2.Dot(uv - new Vector2(0.5f, 0.5f), direction) + 0.5f;
            }
            else // Radial
            {
                Vector2 center = new Vector2(0.5f, 0.5f);
                Vector2 fromCenter = uv - center;
                
                float angleRad = settings.gradientAngle * Mathf.Deg2Rad;
                float cos = Mathf.Cos(-angleRad);
                float sin = Mathf.Sin(-angleRad);

                float rotX = fromCenter.x * cos - fromCenter.y * sin;
                float rotY = fromCenter.x * sin + fromCenter.y * cos;
                
                t = Vector2.Distance(new Vector2(rotX, rotY), Vector2.zero) * 2;
            }

            val = settings.gradient.Evaluate(t).r;
        }
        else // Texture
        {
            int srcIdx = settings.textureSourceIndex;
            if (srcIdx >= cachedPixels.Length || cachedPixels[srcIdx] == null) return 0;

            int sourceWidth = cachedWidths[srcIdx];
            int sourceHeight = cachedHeights[srcIdx];
            int sx = x, sy = y;
            
            switch(resizingMode)
            {
                case ResizingMode.Stretch:
                    sx = (int)(((float)x / outputWidth) * sourceWidth);
                    sy = (int)(((float)y / outputHeight) * sourceHeight);
                    break;
                case ResizingMode.Center:
                    int xOffset = (outputWidth - sourceWidth) / 2;
                    int yOffset = (outputHeight - sourceHeight) / 2;
                    if(x < xOffset || x >= xOffset + sourceWidth || y < yOffset || y >= yOffset + sourceHeight) return 0;
                    sx = x - xOffset;
                    sy = y - yOffset;
                    break;
                case ResizingMode.Tile:
                default:
                    sx = x % sourceWidth;
                    sy = y % sourceHeight;
                    break;
            }

            Color32 p = cachedPixels[srcIdx][sy * sourceWidth + sx];
            
            switch (settings.textureChannelIndex)
            {
                case 0: val = p.r / 255f; break;
                case 1: val = p.g / 255f; break;
                case 2: val = p.b / 255f; break;
                case 3: val = p.a / 255f; break;
            }
        }

        if (settings.invert)
        {
            val = 1f - val;
        }
        
        return val;
    }

    private byte CalculateFinalChannelValue(List<ChannelSettings> inputs, int x, int y, int w, int h)
    {
        if (inputs.Count == 0) return 0;

        float finalVal = GetIndividualChannelValue(inputs[0], x, y, w, h);

        for (int i = 1; i < inputs.Count; i++)
        {
            float nextVal = GetIndividualChannelValue(inputs[i], x, y, w, h);
            switch (inputs[i].blendMode)
            {
                case BlendMode.Add: finalVal += nextVal; break;
                case BlendMode.Subtract: finalVal -= nextVal; break;
                case BlendMode.Multiply: finalVal *= nextVal; break;
                case BlendMode.Divide: if (nextVal != 0) finalVal /= nextVal; else finalVal = 1; break;
                case BlendMode.Average: finalVal = (finalVal + nextVal) / 2f; break;
                case BlendMode.Min: finalVal = Mathf.Min(finalVal, nextVal); break;
                case BlendMode.Max: finalVal = Mathf.Max(finalVal, nextVal); break;
                case BlendMode.Overlay:
                    finalVal = (finalVal < 0.5f) ? (2 * finalVal * nextVal) : (1 - 2 * (1 - finalVal) * (1 - nextVal));
                    break;
            }
        }

        return (byte)(Mathf.Clamp01(finalVal) * 255);
    }

    private void ClearTextureCache()
    {
        for(int i = 0; i < 4; i++)
        {
            if (cachedReadableTextures[i] != null)
            {
                DestroyImmediate(cachedReadableTextures[i]);
                cachedReadableTextures[i] = null;
            }
            cachedPixels[i] = null;
        }
    }
    
    private void UpdateTextureCache(int index)
    {
        if (cachedReadableTextures[index] != null)
        {
            DestroyImmediate(cachedReadableTextures[index]);
        }

        cachedReadableTextures[index] = GetReadableTexture(sourceTextures[index]);
        if(cachedReadableTextures[index] != null)
        {
            cachedPixels[index] = cachedReadableTextures[index].GetPixels32();
            cachedWidths[index] = cachedReadableTextures[index].width;
            cachedHeights[index] = cachedReadableTextures[index].height;
        }
        else
        {
            cachedPixels[index] = null;
        }
    }
    
    private Texture2D GetReadableTexture(Texture2D source)
    {
        if (source == null) return null;
        string path = AssetDatabase.GetAssetPath(source);
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return null;
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D readableTexture = new Texture2D(2, 2);
        readableTexture.LoadImage(fileData);
        return readableTexture;
    }
}
