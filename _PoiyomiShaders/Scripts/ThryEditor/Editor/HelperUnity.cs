// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class UnityHelper
    {
        /// <summary>
        /// return null if not found
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string FindPathOfAssetWithExtension(string filename)
        {
            string[] guids = AssetDatabase.FindAssets(filename.RemoveFileExtension());
            foreach (string s in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(s);
                if (path.EndsWith(filename))
                    return path;
            }
            return null;
        }

        public static List<string> FindAssetOfFilesWithExtension(string filename)
        {
            List<string> ret = new List<string>();
            string[] guids = AssetDatabase.FindAssets(filename.RemoveFileExtension());
            foreach (string s in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(s);
                if (path.EndsWith(filename))
                    ret.Add(path);
            }
            return ret;
        }

        public static void SetDefineSymbol(string symbol, bool active)
        {
            SetDefineSymbol(symbol, active, true);
        }

        public static void SetDefineSymbol(string symbol, bool active, bool refresh_if_changed)
        {
            try
            {
                string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                        BuildTargetGroup.Standalone);
                if (!symbols.Contains(symbol) && active)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(
                                  BuildTargetGroup.Standalone, symbols + ";" + symbol);
                    if(refresh_if_changed)
                        AssetDatabase.Refresh();
                }
                else if (symbols.Contains(symbol) && !active)
                {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(
                                  BuildTargetGroup.Standalone, Regex.Replace(symbols, @";?" + @symbol, ""));
                    if(refresh_if_changed)
                        AssetDatabase.Refresh();
                }
            }
            catch (Exception e)
            {
                e.ToString();
            }
        }

        public static void RepaintInspector(System.Type t)
        {
            Editor[] ed = (Editor[])Resources.FindObjectsOfTypeAll<Editor>();
            for (int i = 0; i < ed.Length; i++)
            {
                if (ed[i].GetType() == t)
                {
                    ed[i].Repaint();
                    return;
                }
            }
        }

        public static void RepaintEditorWindow(Type t)
        {
            EditorWindow window = FindEditorWindow(t);
            if (window != null) window.Repaint();
        }

        public static EditorWindow FindEditorWindow(System.Type t)
        {
            EditorWindow[] ed = (EditorWindow[])Resources.FindObjectsOfTypeAll<EditorWindow>();
            for (int i = 0; i < ed.Length; i++)
            {
                if (ed[i].GetType() == t)
                {
                    return ed[i];
                }
            }
            return null;
        }

        public static string GetGUID(UnityEngine.Object o)
        {
            return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(o));
        }

        public static int CalculateLengthOfText(string message, Font font= null)
        {
            if (font == null)
                font = GUI.skin.font;
            float totalLength = 0;

            CharacterInfo characterInfo = new CharacterInfo();

            char[] arr = message.ToCharArray();

            foreach (char c in arr)
            {
                font.GetCharacterInfo(c, out characterInfo, font.fontSize);
                totalLength += characterInfo.advance;
            }

            return (int)totalLength;
        }
    }

}