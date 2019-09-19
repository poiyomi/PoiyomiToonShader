using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Thry
{
    public class Parsers
    {
        //{text:Test Button,action:{type:url,data:thryrallo.de/megood},hover:hover text,arraytest:[object1,object2],arraytest2:[{type:url,data:thryrallo.de},1]}
        //1
        //string
        public static object Parse(string input)
        {
            input = Regex.Replace(input, @"^\s+|\s+$","");
            if (input.StartsWith("{"))
                 return ParseObject(input);
            else if (input.StartsWith("["))
                return ParseArray(input);
            else
                return ParsePrimitive(input);
        }

        private static Dictionary<string,object> ParseObject(string input)
        {
            input = input.Trim(new char[] { ' ' });
            input = input.TrimStart(new char[] { '{' });
            int depth = 0;
            int variableStart = 0;
            Dictionary<string, object> variables = new Dictionary<string, object>();
            for(int i = 0; i < input.Length; i++)
            {
                if (i == input.Length-1 || (depth == 0 && input[i] == ','))
                {
                    string[] parts = input.Substring(variableStart, i - variableStart).Split(new char[] { ':' }, 2);
                    string key = parts[0];
                    object value = Parse(parts[1]);
                    variables.Add(key, value);
                    variableStart = i + 1;
                }
                else if (input[i] == '{' || input[i] == '[')
                    depth++;
                else if (input[i] == '}' || input[i] == ']')
                    depth--;
                
            }
            return variables;
        }

        private static List<object> ParseArray(string input)
        {
            input = input.Trim(new char[] { ' ' });
            input = input.TrimStart(new char[] { '[' });
            int depth = 0;
            int variableStart = 0;
            List<object> variables = new List<object>();
            for (int i = 0; i < input.Length; i++)
            {
                if (i == input.Length-1 || (depth == 0 && input[i] == ','))
                {
                    variables.Add(Parse(input.Substring(variableStart, i - variableStart)));
                    variableStart = i + 1;
                }
                else if (input[i] == '{' || input[i] == '[')
                    depth++;
                else if (input[i] == '}' || input[i] == ']')
                    depth--;
            }
            return variables;
        }

        public static string ToObjectString(object parsed)
        {
            string ret = "";
            if (parsed == null) return ret;
            if (parsed.GetType() == typeof(Dictionary<string, object>))
            {
                ret += "{";
                Dictionary<string, object> dict = ((Dictionary<string, object>)parsed);
                Dictionary<string, object>.Enumerator enumerator = dict.GetEnumerator();
                while (enumerator.MoveNext())
                    ret += enumerator.Current.Key + ":" + ToObjectString(enumerator.Current.Value) + ",";
                ret = ret.TrimEnd(new char[] { ',' }) + "}";
            }
            else if (parsed.GetType() == typeof(List<object>))
            {
                ret += "[";
                foreach (object o in ((List<object>)parsed))
                    ret += ToObjectString(o) + ",";
                ret = ret.TrimEnd(new char[] { ',' }) + "]";
            }
            else
                ret += parsed.ToString();
            return ret;
        }

        private static object ParsePrimitive(string input)
        {
            input = input.Trim(new char[] { ' ' });
            float floatValue;
            string value = input.TrimStart(new char[] { ' ' });
            if (float.TryParse(value, out floatValue))
            {
                if ((int)floatValue == floatValue)
                    return (int)floatValue;
                return floatValue;
            }
            else if (input.ToLower() == "true")
                return true;
            else if (input.ToLower() == "false")
                return false;
            return value;
        }

        public static type ParseToObject<type>(string input)
        {
            object parsed = Parse(input);
            return (type)ParsedToObject(parsed,typeof(type));
        }

        public static type ConvertParsedToObject<type>(object parsed)
        {
            return (type)ParsedToObject(parsed, typeof(type));
        }

        private static object ParsedToObject(object parsed,Type objtype)
        {
            if (Helper.IsPrimitive(objtype)) return PrimitiveToObject(parsed,objtype);
            if (parsed.GetType() == typeof(Dictionary<string, object>)) return DictionaryToObject(parsed, objtype);
            if (parsed.GetType() == typeof(List<object>)) return ListToObject(parsed, objtype);
            if (objtype.IsEnum && parsed.GetType() == typeof(string))
            {
                if (Enum.IsDefined(objtype, (string)parsed))
                    return Enum.Parse(objtype, (string)parsed);
                Debug.LogWarning("The specified enum for " + objtype.Name + " does not exist. Existing Values are: " + Helper.ArrayToString(Enum.GetValues(objtype)));
                return Enum.GetValues(objtype).GetValue(0);
            }
            return parsed; 
        }

        private static object DictionaryToObject(object parsed, Type objtype)
        {
            object returnObject = Activator.CreateInstance(objtype);
            Dictionary<string, object> dict = (Dictionary<string, object>)parsed;
            foreach (FieldInfo field in objtype.GetFields())
            {
                if (dict.ContainsKey(field.Name))
                {
                    field.SetValue(returnObject, ParsedToObject(Helper.GetValueFromDictionary<string, object>(dict, field.Name), field.FieldType));
                }
            }
            return returnObject;
        }

        private static object ListToObject(object parsed, Type objtype)
        {
            Type list_obj_type = objtype.GetGenericArguments()[0];
            List<object> list_strings = (List<object>)parsed;
            IList return_list = (IList)Activator.CreateInstance(objtype);
            foreach (object s in list_strings)
                return_list.Add(ParsedToObject(s, list_obj_type));
            return return_list;
        }

        private static object PrimitiveToObject(object parsed, Type objtype)
        {
            if (typeof(String) == objtype)
                return parsed.ToString();
            return parsed;
        }

        public static string ObjectToString(object obj)
        {
            if (Helper.IsPrimitive(obj.GetType())) return obj.ToString();
            if (obj.GetType() == typeof(List<object>)) return ListToString(obj);
            if (obj.GetType().IsEnum)
            {
                return (string)obj;
            }
            return ClassObjectToString(obj);
        }

        private static string ClassObjectToString(object obj)
        {
            string ret = "{";
            foreach(FieldInfo field in obj.GetType().GetFields())
            {
                ret += field.Name + ":" + ObjectToString(field.GetValue(obj)) + ",";
            }
            ret = ret.TrimEnd(new char[] { ',' });
            ret += "}";
            return ret;
        }

        private static string ListToString(object obj)
        {
            string ret = "[";
            foreach (object o in (List<object>)obj)
            {
                ret += ObjectToString(o) + ",";
            }
            ret = ret.TrimEnd(new char[] { ',' });
            ret += "]";
            return ret;
        }
    }
}
