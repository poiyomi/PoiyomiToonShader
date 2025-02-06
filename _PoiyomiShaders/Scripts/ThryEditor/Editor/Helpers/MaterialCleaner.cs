using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    //Adapted from https://github.com/lukis101/VRCUnityStuffs/blob/master/Scripts/Editor/MaterialCleaner.cs
    //MIT License

    //Copyright (c) 2019 Dj Lukis.LT

    //Permission is hereby granted, free of charge, to any person obtaining a copy
    //of this software and associated documentation files (the "Software"), to deal
    //in the Software without restriction, including without limitation the rights
    //to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    //copies of the Software, and to permit persons to whom the Software is
    //furnished to do so, subject to the following conditions:

    //The above copyright notice and this permission notice shall be included in all
    //copies or substantial portions of the Software.

    //THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    //IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    //FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    //AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    //LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    //OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    //SOFTWARE.
    public class MaterialCleaner
    {
        public enum CleanPropertyType { Texture, Float, Color }

        private const string PropPath_Tex = "m_SavedProperties.m_TexEnvs";
        private const string PropPath_Float = "m_SavedProperties.m_Floats";
        private const string PropPath_Col = "m_SavedProperties.m_Colors";

        static string GetPath(CleanPropertyType type)
        {
            if (type == CleanPropertyType.Float) return PropPath_Float;
            if (type == CleanPropertyType.Color) return PropPath_Col;
            return PropPath_Tex;
        }
        public static int CountAllUnusedProperties(params Material[] materials)
        {
            ;
            return materials.Sum(m =>
            {
                int count = 0;
                SerializedObject serObj = new SerializedObject(m);
                count += CountUnusedProperties(m, serObj, CleanPropertyType.Texture);
                count += CountUnusedProperties(m, serObj, CleanPropertyType.Float);
                count += CountUnusedProperties(m, serObj, CleanPropertyType.Color);
                return count;
            });
        }
        private static int CountUnusedProperties(Material mat, SerializedObject serObj, CleanPropertyType type, List<string> list = null)
        {
            var properties = serObj.FindProperty(GetPath(type));
            int count = 0;
            if (properties != null && properties.isArray)
            {
                for (int i = 0; i < properties.arraySize; i++)
                {
                    string propName = properties.GetArrayElementAtIndex(i).displayName;
                    if (!mat.HasProperty(propName))
                    {
                        if (list != null) list.Add(propName);
                        count++;
                    }
                }
            }
            return count;
        }
        public static int ListUnusedProperties(CleanPropertyType type, params Material[] materials)
        {
            List<string> list = new List<string>();
            int count = materials.Sum(m => CountUnusedProperties(m, new SerializedObject(m), type, list));
            if (count > 0) ShaderEditor.Out($"Unbound properties of type {type}", list.Distinct().Select(s => $"↳{s}"));
            return count;
        }
        public static int CountUnusedProperties(CleanPropertyType type, params Material[] materials)
        {
            return materials.Sum(m => CountUnusedProperties(m, new SerializedObject(m), type));
        }

        private static int RemoveUnusedProperties(Material mat, SerializedObject serObj, CleanPropertyType type)
        {
            if (!mat.shader.isSupported)
            {
                Debug.LogWarning("Skipping \"" + mat.name + "\" cleanup because shader is unsupported!");
                return 0;
            }
            Undo.RecordObject(mat, "Material property cleanup");
            int removedprops = 0;
            string path = PropPath_Tex;
            if (type == CleanPropertyType.Float) path = PropPath_Float;
            if (type == CleanPropertyType.Color) path = PropPath_Col;
            var properties = serObj.FindProperty(path);
            if (properties != null && properties.isArray)
            {
                int amount = properties.arraySize;
                for (int i = amount - 1; i >= 0; i--) // reverse loop because array gets modified
                {
                    string propName = properties.GetArrayElementAtIndex(i).displayName;
                    if (!mat.HasProperty(propName))
                    {
                        properties.DeleteArrayElementAtIndex(i);
                        removedprops++;
                    }
                }
                if (removedprops > 0)
                    serObj.ApplyModifiedProperties();
            }
            return removedprops;
        }
        public static int RemoveUnusedProperties(CleanPropertyType type, params Material[] materials)
        {
            return materials.Sum(m => RemoveUnusedProperties(m, new SerializedObject(m), type));
        }
        private static int RemoveAllUnusedProperties(Material mat, SerializedObject serObj)
        {
            int removedprops = 0;
            removedprops += RemoveUnusedProperties(mat, serObj, CleanPropertyType.Texture);
            removedprops += RemoveUnusedProperties(mat, serObj, CleanPropertyType.Float);
            removedprops += RemoveUnusedProperties(mat, serObj, CleanPropertyType.Color);

            Debug.Log("Removed " + removedprops + " unused properties from " + mat.name);
            return removedprops;
        }
        public static int RemoveAllUnusedProperties(CleanPropertyType type, params Material[] materials)
        {
            return materials.Sum(m => RemoveAllUnusedProperties(m, new SerializedObject(m)));
        }
        private static void ClearKeywords(Material mat)
        {
            Undo.RecordObject(mat, "Material keyword clear");
            string[] keywords = mat.shaderKeywords;
            mat.shaderKeywords = new string[0];
        }
    }

}