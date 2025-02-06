
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