using System;
using System.Collections.Generic;
using Thry.ThryEditor.Helpers;
using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor.TexturePacker
{
    public enum TextureChannelIn { R, G, B, A, Max, None }
    public enum TextureChannelOut { R, G, B, A, None }
    public enum BlendMode { Add, Multiply, Max, Min }
    public enum InvertMode { None, Invert}
    public enum SaveType { PNG, JPG, EXR}
    public enum InputType { Texture, Color, Gradient }
    public enum GradientDirection { Horizontal, Vertical }
    public enum KernelPreset { None, Custom, EdgeDetection, Sharpen, GaussianBlur3x3, GaussianBlur5x5 }
    public enum RemapMode { None, RangeToRange }

    public static class SaveTypeExtensions
    {
        public static string GetTypeEnding(this SaveType type) => type switch
        {
            SaveType.PNG => ".png",
            SaveType.JPG => ".jpg",
            SaveType.EXR => ".exr",
            _ => ".png"
        };
    }
    
    public abstract class IPackerUIDragable
    {
        public Vector2 UIPosition;
    }

    [Serializable]
    public class KernelSettings
    {
        public bool SplitVerticalHorizontal = true;
        public float[] X = GetKernelPreset(KernelPreset.None, true);
        public float[] Y = GetKernelPreset(KernelPreset.None, false);
        public int Loops = 1;
        public float Strength = 1;
        public bool TwoPass = false;
        public bool GrayScale = false;
        public bool[] Channels = new bool[4] { true, true, true, true };

        public void LoadPreset(KernelPreset preset)
        {
            Loops = 1;
            Strength = 1;
            TwoPass = false;
            GrayScale = false;
            Channels = new bool[] { true, true, true, true };
            if (preset == KernelPreset.GaussianBlur3x3 || preset == KernelPreset.GaussianBlur5x5) Loops = 10;
            if (preset == KernelPreset.EdgeDetection) TwoPass = true;
            if (preset == KernelPreset.EdgeDetection) Channels = new bool[] { true, true, true, false };
            X = GetKernelPreset(preset, true);
            Y = GetKernelPreset(preset, false);
        }

        public static float[] GetKernelPreset(KernelPreset preset, bool isXKernel)
        {
            // return a 5x5 kernel. always 25 values
            switch (preset)
            {
                case KernelPreset.Sharpen: return new float[] { 0, 0, 0, 0, 0, 0, 0, -0.5f, 0, 0, 0, -0.5f, 3, -0.5f, 0, 0, 0, -0.5f, 0, 0, 0, 0, 0, 0, 0 };
                case KernelPreset.EdgeDetection:
                    if (isXKernel) return new float[] { 0, 0, 0, 0, 0, 0, -1, 0, 1, 0, 0, -2, 0, 2, 0, 0, -1, 0, 1, 0, 0, 0, 0, 0, 0 };
                    else return new float[] { 0, 0, 0, 0, 0, 0, -1, -2, -1, 0, 0, 0, 0, 0, 0, 0, 1, 2, 1, 0, 0, 0, 0, 0, 0 };
                case KernelPreset.GaussianBlur3x3: return new float[] { 0, 0, 0, 0, 0, 0, 0.0625f, 0.125f, 0.0625f, 0, 0, 0.125f, 0.25f, 0.125f, 0, 0, 0.0625f, 0.125f, 0.0625f, 0, 0, 0, 0, 0, 0 };
                case KernelPreset.GaussianBlur5x5: return new float[] { 0.003f, 0.0133f, 0.0219f, 0.0133f, 0.003f, 0.0133f, 0.0596f, 0.0983f, 0.0596f, 0.0133f, 0.0219f, 0.0983f, 0.1621f, 0.0983f, 0.0219f, 0.0133f, 0.0596f, 0.0983f, 0.0596f, 0.0133f, 0.003f, 0.0133f, 0.0219f, 0.0133f, 0.003f };
            }
            return new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        }

        public float[] GetKernel(KernelPreset preset, bool isXKernel)
        {
            if (preset == KernelPreset.Custom)
            {
                return isXKernel ? X : Y;
            }
            return GetKernelPreset(preset, isXKernel);
        }

        public override bool Equals(object obj)
        {
            return obj is KernelSettings settings &&
                   SplitVerticalHorizontal == settings.SplitVerticalHorizontal &&
                   EqualityComparer<float[]>.Default.Equals(X, settings.X) &&
                   EqualityComparer<float[]>.Default.Equals(Y, settings.Y) &&
                   Loops == settings.Loops &&
                   Strength == settings.Strength &&
                   TwoPass == settings.TwoPass &&
                   GrayScale == settings.GrayScale &&
                   EqualityComparer<bool[]>.Default.Equals(Channels, settings.Channels);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SplitVerticalHorizontal, X, Y, Loops, Strength, TwoPass, GrayScale, Channels);
        }
    }

    [Serializable]
    public class FileOutput
    {
        public string SaveFolder;
        public string FileName;
        public SaveType SaveType;
        public ColorSpace ColorSpace;
        public FilterMode FilterMode;
        public bool AlphaIsTransparency;
        public int SaveQuality;
        public Vector2Int Resolution;

        public FileOutput(string saveFolder, string fileName, SaveType saveType, ColorSpace colorSpace, FilterMode filterMode, bool alphaIsTransparency, int saveQuality, Vector2Int resolution)
        {
            SaveFolder = saveFolder;
            FileName = fileName;
            SaveType = saveType;
            ColorSpace = colorSpace;
            FilterMode = filterMode;
            AlphaIsTransparency = alphaIsTransparency;
            SaveQuality = saveQuality;
            Resolution = resolution;
        }

        public FileOutput Copy()
        {
            return new FileOutput(
                SaveFolder,
                FileName,
                SaveType,
                ColorSpace,
                FilterMode,
                AlphaIsTransparency,
                SaveQuality,
                Resolution
            );
        }
    }

    [Serializable]
    public class ImageAdjust : IPackerUIDragable
    {
        public float Brightness = 1;
        public float Hue = 0;
        public float Saturation = 1;
        public float Rotation = 0;
        public Vector2 Scale = Vector2.one;
        public Vector2 Offset = Vector2Int.zero;
        public bool ChangeCheck = false;

        public override bool Equals(object obj)
        {
            if (obj is ImageAdjust other)
            {
                return Brightness == other.Brightness &&
                       Hue == other.Hue &&
                       Saturation == other.Saturation &&
                       Rotation == other.Rotation &&
                       Scale == other.Scale &&
                       Offset == other.Offset;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Brightness, Hue, Saturation, Rotation, Scale, Offset);
        }
    }


    [Serializable]
    public struct OutputTarget
    {
        public BlendMode BlendMode;
        public InvertMode Invert;
        public float Fallback;
        
        public OutputTarget(BlendMode blendMode = BlendMode.Max, InvertMode invert = InvertMode.None, float fallback = 0)
        {
            BlendMode = blendMode;
            Invert = invert;
            Fallback = fallback;
        }
    }

    [Serializable]
    public class PackerSource : IPackerUIDragable
    {
        public FilterMode FilterMode;
        public Color Color;
        public Gradient Gradient;
        public GradientDirection GradientDirection;

        public Texture2D GradientTexture;
        public Texture2D ImageTexture;
        public Texture2D ColorTexture;
        public InputType InputType = InputType.Texture;
        [NonSerialized] public Vector2[] ChannelPositions = new Vector2[5];
        [NonSerialized] public Rect[] ChannelRects = new Rect[5];
        
        public Texture2D Texture
        {
            get
            {
                if (InputType == InputType.Texture) return ImageTexture;
                if (InputType == InputType.Gradient) return GradientTexture;
                if (InputType == InputType.Color) return ColorTexture;
                return null;
            }
        }

        public PackerSource()
        {
        }

        public void SetInputTexture(Texture2D tex)
        {
            ImageTexture = tex;
            FilterMode = tex != null ? tex.filterMode : FilterMode.Bilinear;
            if (tex != null) InputType = InputType.Texture;
        }

        public void FixImageTexture()
        {
            if(ImageTexture == null) return;
            string path = AssetDatabase.GetAssetPath(ImageTexture);
            if (string.IsNullOrEmpty(path))
            {
                ThryLogger.LogWarn("TexturePacker", $"Removing faulty input texture {ImageTexture.name} as it could not be found in the project");
                SetInputTexture(null);
            }
            else if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path) is Texture2D == false)
            {
                ThryLogger.LogWarn("TexturePacker", $"Removing faulty input texture {path} as it is not a Texture2D");
                SetInputTexture(null);
            }
        }

        public void UpdateGradientTexture(Vector2Int size)
        {
            if (InputType != InputType.Gradient) return;
            if (GradientTexture != null && GradientTexture.width == size.x && GradientTexture.height == size.y) return;
            if (Gradient == null) Gradient = new Gradient();
            GradientTexture = Converter.GradientToTexture(Gradient, size.x, size.y, GradientDirection == GradientDirection.Vertical);
        }

        public void UpdateColorTexture()
        {
            if (InputType != InputType.Color) return;
            if (ColorTexture != null && ColorTexture.GetPixel(0,0) == Color) return;
            ColorTexture = Converter.ColorToTexture(Color, 16, 16);
        }

        public bool HasBeenModifiedExternally()
        {
            if (InputType != InputType.Texture) return false;
            if (Texture == null) return false;
            return _cachedTextureLastModifiedTime.TryGetValue(Texture, out DateTime cachedLastModified)
                && cachedLastModified != TextureHelper.GetLastModifiedTime(Texture);
        }

        static Dictionary<Texture2D, Texture2D> _cachedUncompressedTextures = new Dictionary<Texture2D, Texture2D>();
        static Dictionary<Texture2D, DateTime> _cachedTextureLastModifiedTime = new Dictionary<Texture2D, DateTime>();
        public Texture2D UncompressedTexture
        {
            get
            {
                if (_cachedUncompressedTextures.ContainsKey(Texture) == false
                    || _cachedUncompressedTextures[Texture] == null
                    || _cachedTextureLastModifiedTime[Texture] != TextureHelper.GetLastModifiedTime(Texture)
                    )
                {
                    string path = AssetDatabase.GetAssetPath(Texture);
                    if (path.EndsWith(".png") || path.EndsWith(".jpg"))
                    {
                        EditorUtility.DisplayProgressBar("Loading Raw PNG", "Loading " + path, 0.5f);
                        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
                        tex.LoadImage(System.IO.File.ReadAllBytes(path));
                        tex.filterMode = Texture.filterMode;
                        _cachedUncompressedTextures[Texture] = tex;
                        EditorUtility.ClearProgressBar();
                    }
                    else if (path.EndsWith(".tga"))
                    {
                        Texture2D tex = TextureHelper.LoadTGA(path, true);
                        tex.filterMode = Texture.filterMode;
                        _cachedUncompressedTextures[Texture] = tex;
                    }
                    else
                    {
                        _cachedUncompressedTextures[Texture] = Texture;
                    }
                    _cachedTextureLastModifiedTime[Texture] = TextureHelper.GetLastModifiedTime(Texture);

                    if(_cachedUncompressedTextures[Texture] == null)
                    {
                        ThryLogger.LogErr("[TexturePacker]", $"Texture {Texture.name} could not be loaded. Make sure it is a readable texture.");
                    }
                }
                return _cachedUncompressedTextures[Texture];
            }
        }

        public Texture2D ComputeShaderTexture
        {
            get
            {
                if (Texture == null) return Texture2D.whiteTexture;
                if (InputType == InputType.Texture) return UncompressedTexture;
                return Texture;
            }
        }

        public bool ComputeShaderTextureIsValid
        {
            get
            {
                if (Texture == null) return false;
                if (InputType == InputType.Texture && UncompressedTexture == null) return false;
                return true;
            }
        }

        public void FindMaxSize(ref int width, ref int height)
        {
            if (Texture == null) return;
            width = Mathf.Max(width, UncompressedTexture.width);
            height = Mathf.Max(height, UncompressedTexture.height);
        }
    }

    [Serializable]
    public struct Connection
    {
        public int FromTextureIndex;
        public TextureChannelIn FromChannel;
        public TextureChannelOut ToChannel;
        public RemapMode RemappingMode;
        public Vector4 Remapping;

        public Connection(int fromTex = -1, TextureChannelIn from = TextureChannelIn.None,
                      TextureChannelOut to = TextureChannelOut.None, RemapMode remapMode = RemapMode.None,
                      Vector4 remap = default)
        {
            FromTextureIndex = fromTex;
            FromChannel = from;
            ToChannel = to;
            RemappingMode = remapMode;
            Remapping = remap == default ? new Vector4(0, 1, 0, 1) : remap;
        }
    }

    struct ConnectionBezierPoints
    {
        public Vector3 Start;
        public Vector3 End;
        public Vector3 StartTangent;
        public Vector3 EndTangent;
        public ConnectionBezierPoints(Connection c, PackerSource[] sources, Vector2[] positionsOut)
        {
            Start = sources[c.FromTextureIndex].ChannelPositions[(int)c.FromChannel];
            End = positionsOut[(int)c.ToChannel];
            StartTangent = Start + Vector3.right * 50;
            EndTangent = End + Vector3.left * 50;
        }
    }

    struct InteractionWithConnection
    {
        public int ListIndex;
        public Connection Data;
        public float DistanceX;
    }
}