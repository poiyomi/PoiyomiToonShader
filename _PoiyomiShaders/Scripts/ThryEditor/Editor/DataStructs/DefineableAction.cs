using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class DefineableAction
    {
        public DefineableActionType type = DefineableActionType.NONE;
        public string data = "";
        public void Perform(Material[] targets)
        {
            switch (type)
            {
                case DefineableActionType.URL:
                    Application.OpenURL(data);
                    break;
                case DefineableActionType.SET_PROPERTY:
                    string[] set = Regex.Split(data, @"=");
                    if (set.Length > 1)
                        MaterialHelper.SetValueAdvanced(set[0].Trim(), set[1].Trim());
                    break;
                case DefineableActionType.SET_TAG:
                    string[] keyValue = Regex.Split(data, @"=");
                    foreach (Material m in targets)
                        m.SetOverrideTag(keyValue[0].Trim(), keyValue[1].Trim());
                    break;
                case DefineableActionType.SET_SHADER:
                    Shader shader = Shader.Find(data);
                    if (shader != null)
                    {
                        foreach (Material m in targets)
                            m.shader = shader;
                    }
                    break;
                case DefineableActionType.OPEN_EDITOR:
                    System.Type t = Helper.FindTypeByFullName(data);
                    if (t != null)
                    {
                        try
                        {
                            EditorWindow window = EditorWindow.GetWindow(t);
                            window.titleContent = new GUIContent("TPS Setup Wizard");
                            window.Show();
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError("[Thry] Couldn't open Editor Window of type" + data);
                            Debug.LogException(e);
                        }
                    }
                    break;
            }
        }

        private static DefineableAction ParseForThryParser(string s)
        { 
            return Parse(s);
        }
        public static DefineableAction Parse(string s)
        {
            s = s.Trim(' ', '"');
            DefineableAction action = new DefineableAction();
            if (s.StartsWith("http", StringComparison.Ordinal) || s.StartsWith("www", StringComparison.Ordinal))
            {
                action.type = DefineableActionType.URL;
                action.data = s;
            }
            else if (s.StartsWith("tag::", StringComparison.Ordinal))
            {
                action.type = DefineableActionType.SET_TAG;
                action.data = s.Replace("tag::", "");
            }
            else if (s.StartsWith("shader=", StringComparison.Ordinal))
            {
                action.type = DefineableActionType.SET_SHADER;
                action.data = s.Replace("shader=", "");
            }
            else if (s.Contains("="))
            {
                action.type = DefineableActionType.SET_PROPERTY;
                action.data = s;
            }
            else if (Regex.IsMatch(s, @"\w+(\.\w+)"))
            {
                action.type = DefineableActionType.OPEN_EDITOR;
                action.data = s;
            }
            return action;
        }

        public static DefineableAction ParseDrawerParameter(string s)
        {
            s = s.Trim();
            DefineableAction action = new DefineableAction();
            if (s.StartsWith("youtube#", StringComparison.Ordinal))
            {
                action.type = DefineableActionType.URL;
                action.data = "https://www.youtube.com/watch?v=" + s.Substring(8);
            }
            return action;
        }

        public override string ToString()
        {
            return $"{{{type},{data}}}";
        }
    }

    public enum DefineableActionType
    {
        NONE,
        URL,
        SET_PROPERTY,
        SET_SHADER,
        SET_TAG,
        OPEN_EDITOR,
    }


}