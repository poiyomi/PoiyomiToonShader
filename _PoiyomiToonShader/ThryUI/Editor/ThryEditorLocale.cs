using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Thry
{
    public class Locale
    {
        public const string DEFAULT_LOCALE = "English";

        private static bool is_init = false;
        private static void Init()
        {
            string dir_path = ThryEditor.GetThryEditorDirectoryPath()+"/Locales";
            string[] files = Directory.GetFiles(dir_path);
            List<string> locales = new List<string>();
            List<string> locales_paths = new List<string>();
            foreach(string f in files)
            {
                if (f.EndsWith(".txt"))
                {
                    locales.Add(f.RemovePath().RemoveFileExtension());
                    locales_paths.Add(f);
                }
            }
            s_available_locales = locales.ToArray();
            s_available_locales_paths = locales_paths.ToArray();
            is_init = true;
        }

        private static string[] s_available_locales;
        private static string[] s_available_locales_paths;
        public static string[] available_locales
        {
            get
            {
                if (!is_init)
                    Init();
                return s_available_locales;
            }
        }

        private static int i_selected_locale = -1;
        public static int selected_locale_index
        {
            get{
                if (i_selected_locale == -1)
                {
                    i_selected_locale = 0;
                    for (int i = 0; i < available_locales.Length; i++)
                        if (available_locales[i] == Config.Get().locale)
                            i_selected_locale = i;
                }
                return i_selected_locale;
            }
            set
            {
                i_selected_locale = value;
                loaded_locale = null;
            }
        }

        private static Dictionary<string, string> loaded_locale;
        public static Dictionary<string,string> locale
        {
            get
            {
                if (loaded_locale == null)
                    LoadSelectedLocale();
                return loaded_locale;
            }
        }

        private static void LoadSelectedLocale()
        {
            if (!is_init)
                Init();
            LoadDefaultLocale();
            string[] lines = Regex.Split(FileHelper.ReadFileIntoString(s_available_locales_paths[selected_locale_index]),@"\r?\n");
            foreach(string l in lines)
            {
                string line = l.Trim(new char[] { ' ' });
                if (line.Length > 0)
                {
                    string[] key_val = Regex.Split(line, @":=");
                    if (key_val.Length > 1)
                    {
                        string key = key_val[0].Trim(new char[] { ' ' });
                        if (loaded_locale.ContainsKey(key))
                            loaded_locale[key] = key_val[1].Trim(new char[] { ' ' });
                    }
                }
            }
        }

        private static int GetDefaultLocaleIndex()
        {
            for (int i = 0; i < available_locales.Length; i++)
                if (available_locales[i] == DEFAULT_LOCALE)
                    return i;
            return selected_locale_index;
        }

        private static void LoadDefaultLocale()
        {
            loaded_locale = new Dictionary<string, string>();
            string[] lines = Regex.Split(FileHelper.ReadFileIntoString(s_available_locales_paths[GetDefaultLocaleIndex()]),@"\r?\n");
            foreach(string l in lines)
            {
                string line = l.Trim(new char[] { ' ' });
                if (line.Length > 0)
                {
                    string[] key_val = Regex.Split(line, @":=");
                    if (key_val.Length > 1)
                        loaded_locale.Add(key_val[0].Trim(new char[] { ' ' }), key_val[1].Trim(new char[] { ' ' }));
                }
            }
        }
    }
}