using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class Helper
    {
        static bool s_didTryRegsiterThisSession = false;

        public static bool ClassWithNamespaceExists(string classname)
        {
            return (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    from type in assembly.GetTypes()
                    where type.FullName == classname
                    select type).Count() > 0;
        }

        public static Type FindTypeByFullName(string fullname)
        {
            return (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    from type in assembly.GetTypes()
                    where type.FullName == fullname
                    select type).FirstOrDefault();
        }

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long GetCurrentUnixTimestampMillis()
        {
            return (long)(DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
        }

        public static long DatetimeToUnixSeconds(DateTime time)
        {
            return (long)(time - UnixEpoch).TotalSeconds;
        }

        public static long GetUnityStartUpTimeStamp()
        {
            return GetCurrentUnixTimestampMillis() - (long)EditorApplication.timeSinceStartup * 1000;
        }

        public static void RegisterEditorUse()
        {
            if (s_didTryRegsiterThisSession) return;
            if (!EditorPrefs.GetBool("thry_has_counted_user", false))
            {
                WebHelper.DownloadStringASync(URL.COUNT_USER, delegate (string s)
                {
                    if (s == "true")
                        EditorPrefs.SetBool("thry_has_counted_user", true);
                });
            }

            string projectPrefix = PlayerSettings.companyName + "." + PlayerSettings.productName;
            if (!EditorPrefs.GetBool(projectPrefix + "_thry_has_counted_project", false))
            {
                WebHelper.DownloadStringASync(URL.COUNT_PROJECT, delegate (string s)
                {
                    if (s == "true")
                        EditorPrefs.SetBool(projectPrefix + "_thry_has_counted_project", true);
                });
            }
            s_didTryRegsiterThisSession = true;
        }

        //-------------------Comparetors----------------------

        public static int CompareVersions(string v1, string v2)
        {
            //fix the string
            v1 = v1.Replace(",", ".");
            v2 = v2.Replace(",", ".");
            Match v1_match = Regex.Match(v1, @"(a|b)?\d+((\.|a|b)\d+)*(a|b)?");
            Match v2_match = Regex.Match(v2, @"(a|b)?\d+((\.|a|b)\d+)*(a|b)?");
            if (!v1_match.Success && !v2_match.Success) return 0;
            else if (!v1_match.Success) return 1;
            else if (!v2_match.Success) return -1;
            v1 = v1_match.Value;
            v2 = v2_match.Value;

            int index_v1 = 0;
            int index_v2 = 0;
            string chunk_v1;
            string chunk_v2;
            while (index_v1 < v1.Length || index_v2 < v2.Length)
            {
                //get a chunk of the strings
                if (index_v1 < v1.Length)
                {
                    chunk_v1 = "";
                    if (v1[index_v1] == 'a')
                        chunk_v1 = "-2";
                    else if (v1[index_v1] == 'b')
                        chunk_v1 = "-1";
                    else
                    {
                        while (index_v1 < v1.Length && v1[index_v1] != 'a' && v1[index_v1] != 'b' && v1[index_v1] != '.')
                            chunk_v1 += v1[index_v1++];
                        if (index_v1 < v1.Length && (v1[index_v1] == 'a' || v1[index_v1] == 'b'))
                            index_v1--;
                    }
                    index_v1++;
                }
                else
                    chunk_v1 = "0";

                if (index_v2 < v2.Length)
                {
                    chunk_v2 = "";
                    if (v2[index_v2] == 'a')
                        chunk_v2 = "-2";
                    else if (v2[index_v2] == 'b')
                        chunk_v2 = "-1";
                    else
                    {
                        while (index_v2 < v2.Length && v2[index_v2] != 'a' && v2[index_v2] != 'b' && v2[index_v2] != '.')
                            chunk_v2 += v2[index_v2++];
                        if (index_v2 < v2.Length && (v2[index_v2] == 'a' || v2[index_v2] == 'b'))
                            index_v2--;
                    }
                    index_v2++;
                }
                else
                    chunk_v2 = "0";

                //compare chunks
                int v1P = int.Parse(chunk_v1);
                int v2P = int.Parse(chunk_v2);
                if (v1P > v2P) return -1;
                else if (v1P < v2P) return 1;
            }
            return 0;
        }

        public static bool IsPrimitive(Type t)
        {
            return t.IsPrimitive || t == typeof(Decimal) || t == typeof(String);
        }

        public static string GetStringBetweenBracketsAndAfterId(string input, string id, char[] brackets)
        {
            string[] parts = Regex.Split(input, id);
            if (parts.Length > 1)
            {
                char[] behind_id = parts[1].ToCharArray();
                int i = 0;
                int begin = 0;
                int end = behind_id.Length - 1;
                int depth = 0;
                bool escaped = false;
                while (i < behind_id.Length)
                {
                    if (behind_id[i] == brackets[0] && !escaped)
                    {
                        if (depth == 0)
                            begin = i;
                        depth++;
                    }
                    else if (behind_id[i] == brackets[1] && !escaped)
                    {
                        depth--;
                        if (depth == 0)
                        {
                            end = i;
                            break;
                        }
                    }

                    if (behind_id[i] == '\\')
                        escaped = !escaped;
                    else
                        escaped = false;
                    i++;
                }
                return parts[1].Substring(begin, end);
            }
            return input;
        }

        public static float SolveMath(string exp, float parameter)
        {
            exp = exp.Replace("x", parameter.ToString(CultureInfo.InvariantCulture));
            exp = exp.Replace(" ", "");
            float f;
            if (ExpressionEvaluator.Evaluate<float>(exp, out f)) return f;
            return 0;
        }

        public static float Mod(float a, float b)
        {
            return a - b * Mathf.Floor(a / b);
        }

        // This code is an implementation of the pseudocode from the Wikipedia,
        // showing a naive implementation.
        // You should research an algorithm with better space complexity.
        public static int LevenshteinDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];
            if (n == 0)
            {
                return m;
            }
            if (m == 0)
            {
                return n;
            }
            for (int i = 0; i <= n; d[i, 0] = i++)
                ;
            for (int j = 0; j <= m; d[0, j] = j++)
                ;
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }

        // Start of Detour methods
        // Modified from: https://github.com/apkd/UnityStaticBatchingSortingPatch/blob/e83bed8cf31fc98097586c4e47af77fa79d9bed5/StaticBatchingSortingPatch.cs
        // Modified by Behemoth/hill
        static Dictionary<MethodInfo, byte[]> s_patchedData = new Dictionary<MethodInfo, byte[]>();
        public static unsafe void TryDetourFromTo(MethodInfo src, MethodInfo dst)
        {
#if UNITY_EDITOR_WIN
            try
            {
                if (IntPtr.Size == sizeof(Int64))
                {
                    // 64-bit systems use 64-bit absolute address and jumps
                    // 12 byte destructive

                    // Get function pointers
                    long Source_Base = src.MethodHandle.GetFunctionPointer().ToInt64();
                    long Destination_Base = dst.MethodHandle.GetFunctionPointer().ToInt64();

                    // Backup Source Data
                    IntPtr Source_IntPtr = src.MethodHandle.GetFunctionPointer();
                    var backup = new byte[0xC];
                    Marshal.Copy(Source_IntPtr, backup, 0, 0xC);
                    s_patchedData.Add(src, backup);

                    // Native source address
                    byte* Pointer_Raw_Source = (byte*)Source_Base;

                    // Pointer to insert jump address into native code
                    long* Pointer_Raw_Address = (long*)(Pointer_Raw_Source + 0x02);

                    // Insert 64-bit absolute jump into native code (address in rax)
                    // mov rax, immediate64
                    // jmp [rax]
                    *(Pointer_Raw_Source + 0x00) = 0x48;
                    *(Pointer_Raw_Source + 0x01) = 0xB8;
                    *Pointer_Raw_Address = Destination_Base; // ( Pointer_Raw_Source + 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 )
                    *(Pointer_Raw_Source + 0x0A) = 0xFF;
                    *(Pointer_Raw_Source + 0x0B) = 0xE0;
                }
                else
                {
                    // 32-bit systems use 32-bit relative offset and jump
                    // 5 byte destructive

                    // Get function pointers
                    int Source_Base = src.MethodHandle.GetFunctionPointer().ToInt32();
                    int Destination_Base = dst.MethodHandle.GetFunctionPointer().ToInt32();

                    // Backup Source Data
                    IntPtr Source_IntPtr = src.MethodHandle.GetFunctionPointer();
                    var backup = new byte[0x5];
                    Marshal.Copy(Source_IntPtr, backup, 0, 0x5);
                    s_patchedData.Add(src, backup);

                    // Native source address
                    byte* Pointer_Raw_Source = (byte*)Source_Base;

                    // Pointer to insert jump address into native code
                    int* Pointer_Raw_Address = (int*)(Pointer_Raw_Source + 1);

                    // Jump offset (less instruction size)
                    int offset = (Destination_Base - Source_Base) - 5;

                    // Insert 32-bit relative jump into native code
                    *Pointer_Raw_Source = 0xE9;
                    *Pointer_Raw_Address = offset;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Unable to detour: {src?.Name ?? "UnknownSrc"} -> {dst?.Name ?? "UnknownDst"}\n{ex}");
                throw;
            }
#endif
        }

        public static unsafe void RestoreDetour(MethodInfo src)
        {
#if UNITY_EDITOR_WIN
            var Source_IntPtr = src.MethodHandle.GetFunctionPointer();
            var backup = s_patchedData[src];
            Marshal.Copy(backup, 0, Source_IntPtr, backup.Length);
            s_patchedData.Remove(src);
#endif
        }
        // End of Detour Methods
    }

}