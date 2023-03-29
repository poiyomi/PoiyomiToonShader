using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Poi.Tools
{
    static class PoiHelpers
    {
        static readonly string suffixSeparator = "_";

        /// <summary>
        /// Changes a path in Assets to an absolute windows path
        /// </summary>
        /// <param name="localPath"></param>
        /// <returns></returns>
        public static string LocalAssetsPathToAbsolutePath(string localPath)
        {
            localPath = NormalizePathSlashes(localPath);
            const string assets = "Assets/";
            if(localPath.StartsWith(assets))
            {
                localPath = localPath.Remove(0, assets.Length);
                localPath = $"{Application.dataPath}/{localPath}";
            }
            return localPath;
        }

        /// <summary>
        /// Replaces all forward slashes \ with back slashes /
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string NormalizePathSlashes(string path)
        {
            if(!string.IsNullOrEmpty(path))
                path = path.Replace('\\', '/');
            return path;
        }

        /// <summary>
        /// Ensures directory exists inside the assets folder
        /// </summary>
        /// <param name="assetPath"></param>
        public static void EnsurePathExistsInAssets(string assetPath)
        {
            Directory.CreateDirectory(LocalAssetsPathToAbsolutePath(assetPath));
        }

        /// <summary>
        /// Adds a suffix to the end of the string then returns it
        /// </summary>
        /// <param name="str"></param>
        /// <param name="suffixes"></param>
        /// <returns></returns>
        public static string AddSuffix(string str, params string[] suffixes)
        {
            bool ignoreSeparatorOnce = string.IsNullOrWhiteSpace(str);
            StringBuilder sb = new StringBuilder(str);
            foreach(var suff in suffixes)
            {
                if(ignoreSeparatorOnce)
                {
                    sb.Append(suff);
                    ignoreSeparatorOnce = false;
                    continue;
                }
                sb.Append(suffixSeparator + suff);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Draws a GUI ilne
        /// </summary>
        /// <param name="spaceBefore"></param>
        /// <param name="spaceAfter"></param>
        internal static void DrawLine(bool spaceBefore = true, bool spaceAfter = true)
        {
            float spaceHeight = 3f;
            if(spaceBefore)
                GUILayout.Space(spaceHeight);

            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));

            if(spaceAfter)
                GUILayout.Space(spaceHeight);
        }

        /// <summary>
        /// Destroys an object with DestroyImmediate in object mode and Destroy in play mode
        /// </summary>
        /// <param name="obj"></param>
        internal static void DestroyAppropriate(UnityEngine.Object obj)
        {
            if(EditorApplication.isPlaying)
                UnityEngine.Object.Destroy(obj);
            else
                UnityEngine.Object.DestroyImmediate(obj);
        }

        /// <summary>
        /// Changes path from full windows path to a local path in the Assets folder
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Path starting with Assets</returns>
        internal static string AbsolutePathToLocalAssetsPath(string path)
        {
            if(path.StartsWith(Application.dataPath))
                path = "Assets" + path.Substring(Application.dataPath.Length);
            return path;
        }

        /// <summary>
        /// Selects and highlights the asset in your unity Project tab
        /// </summary>
        /// <param name="path"></param>
        internal static void PingAssetAtPath(string path)
        {
            var inst = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path).GetInstanceID();
            EditorGUIUtility.PingObject(inst);
        }

        internal static void DrawWithLabelWidth(float width, Action action)
        {
            if(action == null)
                return;
            float old = EditorGUIUtility.labelWidth;
            action.Invoke();
            EditorGUIUtility.labelWidth = old;
        }
    }

    internal static class PoiExtensions
    {
        public static Shader PackerShader => Shader.Find("Hidden/Poi/TexturePacker");

        public static Shader UnpackerShader => Shader.Find("Hidden/Poi/TextureUnpacker");

        /// <summary>
        /// Extension method that bakes a material to <paramref name="tex"/>
        /// </summary>
        /// <param name="tex">Texture to bake <paramref name="materialToBake"/> to</param>
        /// <param name="materialToBake">Material to bake to <paramref name="tex"/></param>
        internal static void BakeMaterialToTexture(this Texture2D tex, Material materialToBake)
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
        internal static Vector2Int ClosestPowerOfTwo(this Vector2Int vec, int? ceiling = null)
        {
            int x = Mathf.ClosestPowerOfTwo(vec.x);
            int y = Mathf.ClosestPowerOfTwo(vec.y);

            if(ceiling != null)
            {
                int ceil = Mathf.ClosestPowerOfTwo((int) ceiling);

                x = Mathf.Clamp(x, x, ceil);
                y = Mathf.Clamp(y, y, ceil);
            }

            return new Vector2Int(x, y);
        }
    }
}