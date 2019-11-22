
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    namespace UwU
    {
        public class GifImporter
        {

            [MenuItem("Assets/Poiyomi/Texture Array/From GIF")]
            static void GifImport()
            {
                string path = AssetDatabase.GetAssetPath(Selection.activeObject);
                List<Texture2D> array = GetGifFrames(path);
                Texture2DArray arrayTexture = new Texture2DArray(array.First().width, array.First().height, array.Count, TextureFormat.RGBA32, true, false);
                for (int i = 0; i < array.Count; i++)
                {
                    arrayTexture.SetPixels(array[i].GetPixels(0), i, 0);
                }
                arrayTexture.Apply();
                AssetDatabase.CreateAsset(arrayTexture, path.Replace(".gif", ".asset"));
            }

            [MenuItem("Assets/Poiyomi/Texture Array/From GIF", true)]
            static bool ValidateGifImport()
            {
                if (Selection.activeObject == null)
                    return false;
                string path = AssetDatabase.GetAssetPath(Selection.activeObject).ToLower();
                return path.EndsWith(".gif");
            }

            public static List<Texture2D> GetGifFrames(string path)
            {
                List<Texture2D> gifFrames = new List<Texture2D>();
                var gifImage = Image.FromFile(path);
                var dimension = new FrameDimension(gifImage.FrameDimensionsList[0]);

                int frameCount = gifImage.GetFrameCount(dimension);
                for (int i = 0; i < frameCount; i++)
                {
                    gifImage.SelectActiveFrame(dimension, i);
                    var frame = new Bitmap(gifImage.Width, gifImage.Height);
                    System.Drawing.Graphics.FromImage(frame).DrawImage(gifImage, Point.Empty);
                    var frameTexture = new Texture2D(frame.Width, frame.Height);

                    for (int x = 0; x < frame.Width; x++)
                    {
                        for (int y = 0; y < frame.Height; y++)
                        {
                            System.Drawing.Color sourceColor = frame.GetPixel(x, y);
                            frameTexture.SetPixel(x,frame.Height - 1 - y, new Color32(sourceColor.R, sourceColor.G, sourceColor.B, sourceColor.A));
                        }
                    }

                    frameTexture.Apply();
                    gifFrames.Add(frameTexture);
                }
                return gifFrames;
            }

        }
    }