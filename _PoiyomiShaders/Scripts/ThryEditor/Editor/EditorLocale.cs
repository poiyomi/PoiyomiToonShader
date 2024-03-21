using System.Collections.Generic;
using UnityEngine;

namespace Thry
{
    public class EditorLocale
    {
        const string EDITOR_LOCALE_NAME = "thry_editor_locale";

        private string[] languages;
        public int selected_locale_index = 0;
        private Dictionary<string, string[]> dictionary;

        public EditorLocale(string file_name)
        {
            LoadCSV(file_name);
        }

        public EditorLocale(string file_name, string selected_name)
        {
            LoadCSV(file_name);
            SetSelectedLocale(selected_name);
        }

        public void SetSelectedLocale(string name)
        {
            for (int i = 0; i < languages.Length; i++)
                if (languages[i].Equals(name))
                    selected_locale_index = i;
        }

        public string Get(string key)
        {
            if(dictionary.ContainsKey(key)) return dictionary[key][selected_locale_index];
            Debug.LogWarning("Locale[key] could not be found.");
            return key;
        }

        public bool Constains(string key)
        {
            return dictionary.ContainsKey(key) && string.IsNullOrEmpty(dictionary[key][selected_locale_index]) == false;
        }

        public string[] available_locales
        {
            get
            {
                return languages;
            }
        }

        public Dictionary<string,string[]>.KeyCollection GetAllKeys()
        {
            return dictionary.Keys;
        }

        public void LoadCSV(string file_name)
        {
            List<string> files = UnityHelper.FindAssetsWithFilename(file_name + ".csv");
            if (files.Count > 0)
                ParseCSV(FileHelper.ReadFileIntoString(files[0]));
            else
                throw new System.Exception("CVS File with name \"" + file_name + "\" could not be found.");
        }

        private static EditorLocale p_editor;
        public static EditorLocale editor
        {
            get
            {
                if (p_editor == null)
                    p_editor = new EditorLocale(EDITOR_LOCALE_NAME);
                return p_editor;
            }
        }

        private void ParseCSV(string text)
        {
            List<List<string>> lines = GetCVSFields(text);
            InitLanguages(lines);
            lines.RemoveAt(0);
            InitDictionary(lines);
        }

        private void InitLanguages(List<List<string>> lines)
        {
            languages = new string[lines[0].Count - 1];
            for (int i = 0; i < languages.Length; i++)
                languages[i] = lines[0][i + 1];
        }

        private void InitDictionary(List<List<string>> lines)
        {
            dictionary = new Dictionary<string, string[]>();
            foreach(List<string> line in lines)
            {
                string key = line[0];
                if (key == "")
                    continue;
                string[] value = new string[languages.Length];
                value[0] = "";
                for(int i = 0; i < value.Length; i++)
                {
                    if (line.Count > i + 1 && line[i + 1] != "")
                        value[i] = line[i + 1];
                    else
                        value[i] = value[0];
                    value[i] = value[i].Replace("\\n", "\n");
                }
                dictionary.Add(key, value);
            }
        }

        private static List<List<string>> GetCVSFields(string text)
        {
            char[] array = text.ToCharArray();
            List<List<string>> lines = new List<List<string>>();
            List<string> current_line = new List<string>();
            lines.Add(current_line);
            string current_value = "";
            bool in_apostrpoh = false;
            for (int i = 0; i < array.Length; i++)
            {
                if (!in_apostrpoh && (array[i] == '\r') && i + 1 < array.Length && (array[i + 1] == '\n'))
                    i += 1;
                if (!in_apostrpoh && (array[i] == '\n'))
                {
                    current_line.Add(current_value);
                    current_line = new List<string>();
                    lines.Add(current_line);
                    current_value = "";
                }
                else if (!in_apostrpoh && array[i] == ',')
                {
                    current_line.Add(current_value);
                    current_value = "";
                }
                else if (!in_apostrpoh && array[i] == '"')
                {
                    in_apostrpoh = true;
                }
                else if (in_apostrpoh && array[i] == '"' && (i == array.Length - 1 || array[i + 1] != '"'))
                {
                    in_apostrpoh = false;
                }
                else if (in_apostrpoh && array[i] == '"' && array[i + 1] == '"')
                {
                    current_value += '"';
                    i += 1;
                }
                else
                {
                    current_value += array[i];
                }
            }
            current_line.Add(current_value);
            return lines;
        }
    }
}
