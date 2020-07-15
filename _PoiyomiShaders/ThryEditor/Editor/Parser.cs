// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Thry
{
    public class Parser
    {

        public static object ParseJson(string input)
        {
            //input = input.Replace("\\n", "\n");
            return ParseJsonPart(input);
        }

        private static object ParseJsonPart(string input)
        {
            input = input.Trim();
            if (input.StartsWith("{"))
                return ParseObject(input);
            else if (input.StartsWith("["))
                return ParseArray(input);
            else
                return ParsePrimitive(input);
        }

        private static Dictionary<string, object> ParseObject(string input)
        {
            input = input.TrimStart(new char[] { '{' });
            int depth = 0;
            int variableStart = 0;
            bool isString = false;
            Dictionary<string, object> variables = new Dictionary<string, object>();
            for (int i = 0; i < input.Length; i++)
            {
                bool escaped = i != 0 && input[i - 1] == '\\';
                if (input[i] == '\"' && !escaped)
                    isString = !isString;
                if (!isString)
                {
                    if (i == input.Length - 1 || (depth == 0 && input[i] == ',' && !escaped))
                    {
                        string[] parts = input.Substring(variableStart, i - variableStart).Split(new char[] { ':' }, 2);
                        if (parts.Length < 2)
                            break;
                        string key = "" + ParsePrimitive(parts[0].Trim());
                        object value = ParseJsonPart(parts[1]);
                        variables.Add(key, value);
                        variableStart = i + 1;
                    }
                    else if ((input[i] == '{' || input[i] == '[') && !escaped)
                        depth++;
                    else if ((input[i] == '}' || input[i] == ']') && !escaped)
                        depth--;
                }

            }
            return variables;
        }

        private static List<object> ParseArray(string input)
        {
            input = input.Trim(new char[] { ' ' });
            int depth = 0;
            int variableStart = 1;
            List<object> variables = new List<object>();
            for (int i = 1; i < input.Length; i++)
            {
                if (i == input.Length - 1 || (depth == 0 && input[i] == ',' && (i == 0 || input[i - 1] != '\\')))
                {
                    variables.Add(ParseJsonPart(input.Substring(variableStart, i - variableStart)));
                    variableStart = i + 1;
                }
                else if (input[i] == '{' || input[i] == '[')
                    depth++;
                else if (input[i] == '}' || input[i] == ']')
                    depth--;
            }
            return variables;
        }

        private static object ParsePrimitive(string input)
        {
            if (input.StartsWith("\""))
                return input.Trim(new char[] { '"' });
            else if (input.ToLower() == "true")
                return true;
            else if (input.ToLower() == "false")
                return false;
            else if (input == "null" || input == "NULL" || input == "Null")
                return null;
            else
            {
                float floatValue;
                if (float.TryParse(input, out floatValue))
                {
                    if ((int)floatValue == floatValue)
                        return (int)floatValue;
                    return floatValue;
                }
            }
            return input;
        }

        public static type ParseToObject<type>(string input)
        {
            object parsed = ParseJson(input);
            object ret = null;
            try
            {
                ret = (type)ParsedToObject(parsed, typeof(type));
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                Debug.LogError(input + " cannot be parsed to object of type " + typeof(type).ToString());
                ret = Activator.CreateInstance(typeof(type));
            }
            return (type)ret;
        }

        public static type ConvertParsedToObject<type>(object parsed)
        {
            return (type)ParsedToObject(parsed, typeof(type));
        }

        private static object ParsedToObject(object parsed,Type objtype)
        {
            if (parsed == null) return null;
            if (Helper.IsPrimitive(objtype)) return PrimitiveToObject(parsed,objtype);
            if (parsed.GetType() == typeof(Dictionary<string, object>)) return DictionaryToObject(parsed, objtype);
            if (parsed.GetType() == typeof(List<object>))
            {
                if (objtype.IsArray)
                    return ArrayToObject(parsed, objtype);
                else
                    return ListToObject(parsed, objtype);
            }
            if (objtype.IsEnum && parsed.GetType() == typeof(string))
            {
                if (Enum.IsDefined(objtype, (string)parsed))
                    return Enum.Parse(objtype, (string)parsed);
                Debug.LogWarning("The specified enum for " + objtype.Name + " does not exist. Existing Values are: " + Converter.ArrayToString(Enum.GetValues(objtype)));
                return Enum.GetValues(objtype).GetValue(0);
            }
            return null; 
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
            foreach (PropertyInfo property in objtype.GetProperties())
            {
                if (property.CanWrite && property.CanRead && property.GetIndexParameters().Length == 0 && dict.ContainsKey(property.Name))
                {
                    property.SetValue(returnObject, ParsedToObject(Helper.GetValueFromDictionary<string, object>(dict, property.Name), property.PropertyType), null);
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

        private static object ArrayToObject(object parsed, Type objtype)
        {
            Type array_obj_type = objtype.GetElementType();
            List<object> list_strings = (List<object>)parsed;
            IList return_list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(array_obj_type));
            foreach (object s in list_strings)
            {
                object o = ParsedToObject(s, array_obj_type);
                if(o!=null)
                    return_list.Add(o);
            }
            object return_array = Activator.CreateInstance(objtype, return_list.Count);
            return_list.CopyTo(return_array as Array, 0);
            return return_array;
        }

        private static object PrimitiveToObject(object parsed, Type objtype)
        {
            if (typeof(String) == objtype)
                return parsed!=null?parsed.ToString():null;
            if (typeof(char) == objtype)
                return ((string)parsed)[0];
            return parsed;
        }

        public static string ObjectToString(object obj)
        {
            if (obj == null) return "null";
            if (Helper.IsPrimitive(obj.GetType())) return PrimitiveToString(obj);
            if (obj is IList) return ListToString(obj);
            if (obj.GetType().IsArray) return ListToString(obj);
            if (obj.GetType().IsEnum) return obj.ToString();
            if (obj.GetType().IsClass) return ClassObjectToString(obj);
            if (obj.GetType().IsValueType && !obj.GetType().IsEnum) return ClassObjectToString(obj);
            return "";
        }

        private static string ClassObjectToString(object obj)
        {
            string ret = "{";
            foreach(FieldInfo field in obj.GetType().GetFields())
            {
                if(field.IsPublic)
                    ret += "\""+field.Name + "\"" + ":" + ObjectToString(field.GetValue(obj)) + ",";
            }
            foreach (PropertyInfo property in obj.GetType().GetProperties())
            {
                if(property.CanWrite && property.CanRead && property.GetIndexParameters().Length==0)
                    ret += "\""+ property.Name + "\"" + ":" + ObjectToString(property.GetValue(obj,null)) + ",";
            }
            ret = ret.TrimEnd(new char[] { ',' });
            ret += "}";
            return ret;
        }

        private static string ListToString(object obj)
        {
            string ret = "[";
            foreach (object o in obj as IEnumerable)
            {
                ret += ObjectToString(o) + ",";
            }
            ret = ret.TrimEnd(new char[] { ',' });
            ret += "]";
            return ret;
        }

        private static string PrimitiveToString(object obj)
        {
            if (obj.GetType() == typeof(string))
                return "\"" + obj + "\"";
            return obj.ToString();
        }
    }
}
