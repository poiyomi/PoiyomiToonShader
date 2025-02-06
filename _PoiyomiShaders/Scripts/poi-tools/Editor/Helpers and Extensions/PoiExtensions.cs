using UnityEngine;

namespace Poi.Tools
{
    public static class PoiExtensions
    {
        public static Shader PackerShader => Shader.Find("Hidden/Poi/TexturePacker");

        public static Shader UnpackerShader => Shader.Find("Hidden/Poi/TextureUnpacker");

        /// <summary>
        /// Extension method that bakes a material to <paramref name="tex"/>
        /// </summary>
        /// <param name="tex">Texture to bake <paramref name="materialToBake"/> to</param>
        /// <param name="materialToBake">Material to bake to <paramref name="tex"/></param>
        public static void BakeMaterialToTexture(this Texture2D tex, Material materialToBake)
        {
            var res = new Vector2Int(tex.width, tex.height);

            RenderTexture renderTexture = RenderTexture.GetTemporary(res.x, res.y);
            Graphics.Blit(null, renderTexture, materialToBake);

            //transfer image from rendertexture to texture
            RenderTexture.active = renderTexture;
            tex.ReadPixels(new Rect(Vector2.zero, res), 0, 0);
            tex.Apply(false, false);

            //clean up variables
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(renderTexture);
        }

        /// <summary>
        /// Rounds vector to closest power of two. Optionally, if above ceiling, square root down by one power of two
        /// </summary>
        /// <param name="vec"></param>
        /// <param name="ceiling">Power of two ceiling. Will be rounded to power of two if not power of two already</param>
        /// <returns></returns>
        public static Vector2Int ClosestPowerOfTwo(this Vector2Int vec, int? ceiling = null)
        {
            int x = Mathf.ClosestPowerOfTwo(vec.x);
            int y = Mathf.ClosestPowerOfTwo(vec.y);

            if(ceiling != null)
            {
                int ceil = Mathf.ClosestPowerOfTwo((int)ceiling);

                x = Mathf.Clamp(x, x, ceil);
                y = Mathf.Clamp(y, y, ceil);
            }

            return new Vector2Int(x, y);
        }

        public static string SanitizePathString(this string path)
        {
            return PoiHelpers.ReplaceIllegalFilenameCharacters(path);
        }
    }
}
