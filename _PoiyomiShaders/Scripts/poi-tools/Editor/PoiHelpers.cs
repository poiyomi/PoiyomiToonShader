using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Poi.Tools
{
    public static class PoiHelpers
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
            AssetDatabase.Refresh();
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
        /// Draws a GUI line
        /// </summary>
        /// <param name="spaceBefore"></param>
        /// <param name="spaceAfter"></param>
        public static void DrawLine(bool spaceBefore = true, bool spaceAfter = true)
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
        public static void DestroyAppropriate(UnityEngine.Object obj)
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
        public static string AbsolutePathToLocalAssetsPath(string path)
        {
            if(path.StartsWith(Application.dataPath))
                path = "Assets" + path.Substring(Application.dataPath.Length);
            return path;
        }

        /// <summary>
        /// Selects and highlights the asset in your unity Project tab
        /// </summary>
        /// <param name="path"></param>
        public static void PingAssetAtPath(string path)
        {
            var inst = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path).GetInstanceID();
            EditorGUIUtility.PingObject(inst);
        }

        public static void DrawWithLabelWidth(float width, Action action)
        {
            if(action == null)
                return;
            float old = EditorGUIUtility.labelWidth;
            action.Invoke();
            EditorGUIUtility.labelWidth = old;
        }

        static HashSet<char> IllegalFilenameChars
        {
            get
            {
                if(_illegalFilenameChars == null)
                    _illegalFilenameChars = new HashSet<char>(Path.GetInvalidFileNameChars());
                return _illegalFilenameChars;
            }
        }
        static HashSet<char> _illegalFilenameChars;

        public static string ReplaceIllegalFilenameCharacters(string original, char replacement = '_')
        {
            var sb = new StringBuilder();
            foreach(var current in original)
            {
                if(IllegalFilenameChars.Contains(current))
                    sb.Append(replacement);
                else
                    sb.Append(current);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Duplicates a material and outs the new path. Returns true if succeeded
        /// </summary>
        /// <param name="materialAsset">Asset to duplicate</param>
        /// <param name="newMaterialAssetName">New asset name with or without file extension</param>
        /// <param name="materialCopyPath">The new asset path</param>
        /// <returns>True if succeeded</returns>
        public static bool TryDuplicateMaterialAsset(Material materialAsset, string newMaterialAssetName, out string materialCopyPath)
        {
            materialCopyPath = null;
            try
            {
                string materialAssetPath = AssetDatabase.GetAssetPath(materialAsset);
                if(string.IsNullOrWhiteSpace(materialAssetPath))
                    return false;

                string assetDirectory = Path.GetDirectoryName(materialAssetPath);
                string newPath = $"{assetDirectory}\\{newMaterialAssetName}{(newMaterialAssetName.EndsWith(".mat") ? "" : ".mat")}";
                materialCopyPath = AssetDatabase.GenerateUniqueAssetPath(newPath);

                if(AssetDatabase.IsSubAsset(materialAsset) || materialAssetPath.StartsWith("Resources/unity_builtin_extra"))
                {
                    AssetDatabase.CreateAsset(new Material(materialAsset), materialCopyPath);
                    return true;
                }
                return AssetDatabase.CopyAsset(materialAssetPath, materialCopyPath);
            }
            catch
            {
                return false;
            }
        }
    }

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
                int ceil = Mathf.ClosestPowerOfTwo((int) ceiling);

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