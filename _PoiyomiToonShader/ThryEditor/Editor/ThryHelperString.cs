// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Thry
{
    public static class StringExpensions
    {
        public static string RemovePath(this string url)
        {
            Match m = Regex.Match(url, @"(?<=\/|\\|^)[^\/\\]+$");
            if (m.Success)
                return m.Value;
            return url;
        }

        /// <summary>
        /// returns string up to (excluding) last '.'
        /// </summary>
        /// <param name="file"></param>
        /// <returns>returns input string if not possible</returns>
        public static string RemoveFileExtension(this string file)
        {
            Match m = Regex.Match(file, @".+?(?=\.|$)");
            if (m.Success)
                return m.Value;
            return file;
        }

        /// <summary>
        /// returns string up to (including) last '/'
        /// </summary>
        /// <param name="file"></param>
        /// <returns>returns input string if not possible</returns>
        public static string RemoveFileName(this string file)
        {
            Match m = Regex.Match(file, @".+\/");
            if (m.Success)
                return m.Value;
            return file;
        }

        /// <summary>
        /// returns string up to (excluding) last '/'
        /// </summary>
        /// <param name="file"></param>
        /// <returns>returns emtpy string if not possible</returns>
        public static string GetDirectoryPath(this string file)
        {
            Match m = Regex.Match(file, @".+(?=\/)");
            if (m.Success)
                return m.Value;
            return "";
        }

        public static bool EndsOnFileExtension(this string s)
        {
            Match m = Regex.Match(s, @"(?<=\/|^)[^\/.]+$");
            return !m.Success;
        }

        /// <summary>
        /// removes the last directory from path. dont call on path with file
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string RemoveOneDirectory(this string s)
        {
            Match m = Regex.Match(s, @"^.*(?=\/[^\/]*)");
            if (m.Success)
                return m.Value;
            return s;
        }

    }

    public class StringHelper
    {
        public static string GetBetween(string value, string prefix, string postfix)
        {
            return GetBetween(value, prefix, postfix, value);
        }

        public static string GetBetween(string value, string prefix, string postfix, string fallback)
        {
            string pattern = @"(?<=" + prefix + ").*?(?=" + postfix + ")";
            Match m = Regex.Match(value, pattern);
            if (m.Success)
                return m.Value;
            return fallback;
        }

        //returns data for name:{data} even if data containss brakets
        public static string GetBracket(string data, string bracketName)
        {
            Match m = Regex.Match(data, bracketName + ":");
            if (m.Success)
            {
                int startIndex = m.Index + bracketName.Length + 2;
                int i = startIndex;
                int depth = 0;
                while (++i < data.Length)
                {
                    if (data[i] == '{')
                        depth++;
                    else if (data[i] == '}')
                    {
                        if (depth == 0)
                            break;
                        depth--;
                    }
                }
                return data.Substring(startIndex, i - startIndex);
            }
            return data;
        }
    }
}
