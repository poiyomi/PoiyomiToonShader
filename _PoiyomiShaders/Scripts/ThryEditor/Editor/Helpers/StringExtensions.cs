using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace Thry
{
    static class StringExtensions
    {
        public static string ReplaceVariables(this string s, params object[] values)
        {
            for (int i = 0; i < values.Length; i++)
            {
                s = s.Replace("{" + i + "}", values[i].ToString());
            }
            return s;
        }
    }

}