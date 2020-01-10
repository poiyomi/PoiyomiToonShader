/// Date	: 12/06/2018
/// Company	: Fantastic, yes
/// Author	: Maximilian Rötzler
/// License	: This code is licensed under MIT license

using UnityEngine;
using UnityEditor;
using System;

namespace UwU
{
    public class Texture2DArrayData : ScriptableObject
    {
        #region Create Asset Menu
        [MenuItem("Assets/Poiyomi/Texture Array/From Images", false, 303)]
        private static void TextureArrayItem()
        {
            Texture2D[] wew = Selection.GetFiltered<Texture2D>(SelectionMode.TopLevel);
            Array.Sort(wew, (UnityEngine.Object one, UnityEngine.Object two) => one.name.CompareTo(two.name));
            Selection.objects = wew;
            Texture2DArray texture2DArray = new Texture2DArray(wew[0].width, wew[0].height, wew.Length, wew[0].format, true);

            string assetPath = AssetDatabase.GetAssetPath(wew[0]);
            assetPath = assetPath.Remove(assetPath.LastIndexOf('/')) + "/Texture2DArray.asset";

            for (int i = 0; i < wew.Length; i++)
            {
                for (int m = 0; m < wew[i].mipmapCount; m++)
                {
                    Graphics.CopyTexture(wew[i], 0, m, texture2DArray, i, m);
                }
            }

            texture2DArray.anisoLevel = wew[0].anisoLevel;
            texture2DArray.wrapModeU = wew[0].wrapModeU;
            texture2DArray.wrapModeV = wew[0].wrapModeV;
            texture2DArray.Apply(false, true);

            AssetDatabase.CreateAsset(texture2DArray, assetPath);
            AssetDatabase.SaveAssets();

            Selection.activeObject = texture2DArray;
        }

        [MenuItem("Assets/Poiyomi/Create Texture Array", true)]
        private static bool TextureArrayItemValidation()
        {
            return Selection.GetFiltered<Texture2D>(SelectionMode.TopLevel).Length > 0;
        }
        #endregion
    }
}