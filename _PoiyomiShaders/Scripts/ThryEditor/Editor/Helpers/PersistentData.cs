
namespace Thry
{
    public class PersistentData
    {
        public static string Get(string key)
        {
            return FileHelper.LoadValueFromFile(key, PATH.PERSISTENT_DATA);
        }

        public static void Set(string key, string value)
        {
            FileHelper.SaveValueToFile(key, value, PATH.PERSISTENT_DATA);
        }

        public static T Get<T>(string key, T defaultValue)
        {
            string s = FileHelper.LoadValueFromFile(key, PATH.PERSISTENT_DATA);
            if (string.IsNullOrEmpty(s)) return defaultValue;
            T obj = Parser.Deserialize<T>(s);
            if (obj == null) return defaultValue;
            return obj;
        }

        public static void Set(string key, object value)
        {
            FileHelper.SaveValueToFile(key, Parser.Serialize(value), PATH.PERSISTENT_DATA);
        }
    }

}