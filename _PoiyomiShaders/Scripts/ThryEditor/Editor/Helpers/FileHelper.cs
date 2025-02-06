using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class FileHelper
    {
        public static string FindFile(string name, string type = null)
        {
            string[] guids;
            if (type != null)
                guids = AssetDatabase.FindAssets(name + " t:" + type);
            else
                guids = AssetDatabase.FindAssets(name);
            if (guids.Length == 0)
                return null;
            return AssetDatabase.GUIDToAssetPath(guids[0]);
        }

        //-----------------------Value To File Saver----------------------

        private static Dictionary<string, Dictionary<string, string>> s_textFileData = new Dictionary<string, Dictionary<string, string>>();

        public static string LoadValueFromFile(string key, string path)
        {
            if (!s_textFileData.ContainsKey(path)) ReadFileIntoTextFileData(path);
            if (s_textFileData[path].ContainsKey(key))
                return s_textFileData[path][key];
            return null;
        }

        private static void ReadFileIntoTextFileData(string path)
        {
            string data = ReadFileIntoString(path);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            MatchCollection matchCollection = Regex.Matches(data, @".*\s*:=.*(?=\r?\n)");
            foreach (Match m in matchCollection)
            {
                string[] keyvalue = m.Value.Split(new string[] { ":=" }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (keyvalue.Length > 1)
                    dictionary[keyvalue[0]] = keyvalue[1];
            }
            s_textFileData[path] = dictionary;
        }

        public static bool SaveValueToFile(string key, string value, string path)
        {
            if (!s_textFileData.ContainsKey(path)) ReadFileIntoTextFileData(path);
            s_textFileData[path][key] = value;
            return SaveDictionaryToFile(path, s_textFileData[path]);
        }

        public static void RemoveValueFromFile(string key, string path)
        {
            if (!s_textFileData.ContainsKey(path)) ReadFileIntoTextFileData(path);
            if (s_textFileData[path].ContainsKey(key)) s_textFileData[path].Remove(key);
        }

        private static bool SaveDictionaryToFile(string path, Dictionary<string, string> dictionary)
        {
            s_textFileData[path] = dictionary;
            string data = s_textFileData[path].Aggregate("", (d1, d2) => d1 + d2.Key + ":=" + d2.Value + "\n");
            WriteStringToFile(data, path);
            return true;
        }

        //-----------------------File Interaction---------------------

        public static string FindFileAndReadIntoString(string fileName)
        {
            string[] guids = AssetDatabase.FindAssets(fileName);
            if (guids.Length > 0)
                return ReadFileIntoString(AssetDatabase.GUIDToAssetPath(guids[0]));
            else return "";
        }

        public static void FindFileAndWriteString(string fileName, string s)
        {
            string[] guids = AssetDatabase.FindAssets(fileName);
            if (guids.Length > 0)
                WriteStringToFile(s, AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        public static string ReadFileIntoString(string path)
        {
            if (!File.Exists(path))
            {
                CreateFileWithDirectories(path);
                return "";
            }
            StreamReader reader = new StreamReader(path);
            string ret = reader.ReadToEnd();
            reader.Close();
            return ret;
        }

        public static void WriteStringToFile(string s, string path)
        {
            if (!File.Exists(path)) CreateFileWithDirectories(path);
            StreamWriter writer = new StreamWriter(path, false);
            writer.Write(s);
            writer.Close();
        }

        public static bool WriteBytesToFile(byte[] bytes, string path)
        {
            if (!File.Exists(path)) CreateFileWithDirectories(path);
            try
            {
                using (var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fs.Write(bytes, 0, bytes.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Exception caught in process: " + ex.ToString());
                return false;
            }
        }

        public static void CreateFileWithDirectories(string path)
        {
            string dir_path = Path.GetDirectoryName(path);
            if (dir_path != "")
                Directory.CreateDirectory(dir_path);
            File.Create(path).Close();
        }
    }

}