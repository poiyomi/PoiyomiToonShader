// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Thry.ThryEditor.Helpers;
using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor
{
    public class Parser
    {

        public static string Serialize(object o, bool prettyPrint = false)
        {
            return Serialize(o, prettyPrint, 0);
        }

        [System.Obsolete("Use Deserialize<T> instead")]
        public static string ObjectToString(object obj)
        {
            return Serialize(obj, false, 0);
        }

        public static T Deserialize<T>(string s)
        {
            return DeserializeInternal<T>(s);
        }

        public static object Deserialize(string s, Type t)
        {
            return DeserializeInternal(s, t);
        }

        private static string Serialize(object obj, bool prettyPrint, int indent)
        {
            if (obj == null) return "null";
            if (Helper.IsPrimitive(obj.GetType())) return SerializePrimitive(obj);
            if (obj is IList) return SerializeList(obj, prettyPrint, indent);
            if (obj.GetType().IsGenericType && obj.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>)) return SerializeDictionary(obj, prettyPrint, indent);
            if (obj.GetType().IsArray) return SerializeList(obj, prettyPrint, indent);
            if (obj.GetType().IsEnum) return obj.ToString();
            if (obj.GetType().IsClass) return SerializeClass(obj, prettyPrint, indent);
            if (obj.GetType().IsValueType && !obj.GetType().IsEnum) return SerializeClass(obj, prettyPrint, indent);
            return "";
        }

        private static T DeserializeInternal<T>(string s)
        {
            return (T)DeserializeInternal(s, typeof(T));
        }

        private static object DeserializeInternal(string s, Type t)
        {
            try
            {
                return ParseJsonPart(s, 0, s.Length, t, t.Name);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.ToString());
                Debug.LogWarning(s + " cannot be parsed to object of type " + t.ToString());
                return Activator.CreateInstance(t);
            }
        }

#region Json to Object Parser
        private static object ParseJsonPart(string input, int start, int end, Type t, string debugName)
        {
            if (input == null) return null;
            if (Helper.IsPrimitive(t)) return ParseToPrimitive(input, start, end, t);
            if (t.IsGenericType && t.GetInterfaces().Contains(typeof(IList))) return ParseToList(input, start, end, t, debugName);
            // if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>)) return ParseToDictionary(input, start, end, t);
            if (t.IsArray) return ParseToArray(input, start, end, t, debugName);
            if (t.IsEnum) return ParseToEnum(input, start, end, t);
            if (t.IsClass) return ParseToObject(input, start, end, t, debugName);
            if (t.IsValueType && !t.IsEnum) return ParseToObject(input, start, end, t, debugName);
            return null;
        }

        // Primitive
        private static object ParseToPrimitive(string input, int start, int end, Type t)
        {
            int rawStart = start;
            int rawEnd = end;

            while(input[start] == ' ' || input[start] == '\t' || input[start] == '\n' || input[start] == '\r')
                start++;
            while(input[end - 1] == ' ' || input[end - 1] == '\t' || input[end - 1] == '\n' || input[end - 1] == '\r')
                end--;

            bool isInQuotes = false;
            if(input[start] == '"' && input[end - 1] == '"')
            {
                start++;
                end--;
                isInQuotes = true;
            }
            string trimmedStr = input.Substring(start, end - start);
            
            switch
            (Type.GetTypeCode(t))
            {
                case TypeCode.Boolean:
                    return trimmedStr.ToLower() == "true" || trimmedStr == "1";
                case TypeCode.Int32:
                    return (int)ParseFloat(trimmedStr);
                case TypeCode.Single:
                    return ParseFloat(trimmedStr);
                case TypeCode.Char:
                    return trimmedStr[0];
                case TypeCode.String:
                    if(!isInQuotes && trimmedStr == "null") return null;
                    return trimmedStr;
                default:
                    return trimmedStr;
            }
        }

        public static float ParseFloat(string s, float defaultF = 0)
        {
            float f;
            if(float.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out f))
            {
                return f;
            }
            return defaultF;
        }

        // Object

        private static Dictionary<Type, MethodInfo> thryObjectMethodCache = new Dictionary<Type, MethodInfo>();
        private static bool TryThryParser(string input, int start, int end, Type objtype, out object returnObject)
        {
            returnObject = null;
            MethodInfo method = null;
            if (!thryObjectMethodCache.TryGetValue(objtype, out method))
            {
                method = objtype.GetMethod("ParseForThryParser", BindingFlags.Static | BindingFlags.NonPublic);
                thryObjectMethodCache.Add(objtype, method);
            }
            if (method == null) return false;
            returnObject = method.Invoke(null, new object[] { input.Substring(start, end - start) });
            return true;
        }

        private static Dictionary<Type, Dictionary<string, FieldInfo>> fieldCache = new Dictionary<Type, Dictionary<string, FieldInfo>>();
        private static Dictionary<Type,Dictionary<string,PropertyInfo>> propertyCache = new Dictionary<Type,Dictionary<string,PropertyInfo>>();
        static Dictionary<string, FieldInfo> GetFields(Type t)
        {
            if (fieldCache.TryGetValue(t, out Dictionary<string, FieldInfo> fields))
                return fields;

            fields = new Dictionary<string, FieldInfo>();
            foreach (FieldInfo field in t.GetFields())
                fields.Add(field.Name, field);
            fieldCache.Add(t, fields);
            return fields;
        }

        static Dictionary<string, PropertyInfo> GetProperties(Type t)
        {
            if (propertyCache.TryGetValue(t, out Dictionary<string, PropertyInfo> properties))
                return properties;

            properties = new Dictionary<string, PropertyInfo>();
            foreach (PropertyInfo property in t.GetProperties().Where(p => p.CanWrite && p.CanRead && p.GetIndexParameters().Length == 0))
                properties.Add(property.Name, property);
            propertyCache.Add(t, properties);
            return properties;
        }

        private static object ParseToObject(string input, int start, int end, Type t, string debugName)
        {
            while (start < end && (input[start] == ' ' || input[start] == '\t' || input[start] == '\n' || input[start] == '\r'))
                start++;
            while (end > start && (input[end - 1] == ' ' || input[end - 1] == '\t' || input[end - 1] == '\n' || input[end - 1] == '\r'))
                end--;
            if (end - start == 4 && input.Substring(start, 4) == "null")
                return null;
            object returnObject;
            if (input[start] != '{' || input[end - 1] != '}')
            {
                if (TryThryParser(input, start, end, t, out returnObject))
                    return returnObject;
                return null;
            }
            start += 1;
            end -= 1;

            // If type is abstract find concrete type that matches the most Fields/Properties
            if (t.IsAbstract)
            {
                ThryLogger.LogDetail($"ThryParser", $"Cannot parse abstract type {t.Name}. Please use a concrete type instead. (Input: '{input.Substring(start, end - start)}' FieldName: {debugName})");
                return null;
            }

            Dictionary<string, FieldInfo> fields = GetFields(t);
            Dictionary<string, PropertyInfo> properties = GetProperties(t);

            returnObject = Activator.CreateInstance(t);

            void parseVariable(int varStart, int varEnd)
            {
                int seperatorIndex = input.IndexOf(':', varStart, varEnd - varStart);
                if (seperatorIndex == -1)
                    return;

                int keyStart = varStart;
                int keyEnd = seperatorIndex;
                while (input[keyStart] == ' ' || input[keyStart] == '\t' || input[keyStart] == '\n' || input[keyStart] == '\r')
                    keyStart++;
                while (input[keyEnd - 1] == ' ' || input[keyEnd - 1] == '\t' || input[keyEnd - 1] == '\n' || input[keyEnd - 1] == '\r')
                    keyEnd--;
                if (input[keyStart] == '\"')
                    keyStart++;
                if (input[keyEnd - 1] == '\"')
                    keyEnd--;
                string key = input.Substring(keyStart, keyEnd - keyStart);
                if (fields.TryGetValue(key, out FieldInfo field))
                {
                    object value = ParseJsonPart(input, seperatorIndex + 1, varEnd, field.FieldType, debugName + "." + field.Name);
                    if(value != null)
                        field.SetValue(returnObject, value);
                }
                else if (properties.TryGetValue(key, out PropertyInfo property))
                {
                    object value = ParseJsonPart(input, seperatorIndex + 1, varEnd, property.PropertyType, debugName + "." + property.Name);
                    if(value != null)
                        property.SetValue(returnObject, value, null);
                }
            }

            int depth = 0;
            int variableStart = start;
            bool isString = false;
            for (int i = start; i < end; i++)
            {
                bool escaped = i != 0 && input[i - 1] == '\\';
                if (input[i] == '\"' && !escaped)
                    isString = !isString;
                if (!isString)
                {
                    if ((depth == 0 && input[i] == ',' && !escaped) || (!escaped && depth == 0 && input[i] == '}'))
                    {
                        parseVariable(variableStart, i);
                        variableStart = i + 1;
                    }
                    else if (i == end - 1)
                    {
                        parseVariable(variableStart, i + 1);
                    }
                    else if ((input[i] == '{' || input[i] == '[') && !escaped)
                        depth++;
                    else if ((input[i] == '}' || input[i] == ']') && !escaped)
                        depth--;
                }

            }
            return returnObject;
        }

        private static Dictionary<Type, MethodInfo> thryArrayMethodCache = new Dictionary<Type, MethodInfo>();
        private static bool TryThryArrayParser(string input, int start, int end, Type objtype, out object returnObject)
        {
            returnObject = null;
            if (objtype.BaseType != typeof(System.Array)) return false;
            MethodInfo method = null;
            if (!thryArrayMethodCache.TryGetValue(objtype, out method))
            {
                method = objtype.GetMethod("ParseToArrayForThryParser", BindingFlags.Static | BindingFlags.NonPublic);
                thryArrayMethodCache.Add(objtype, method);
            }
            if (method == null) return false;

            int searchIndex = start;
            while (searchIndex < end && input[searchIndex] != '[')
                if(input[searchIndex] != ' ' && input[searchIndex] != '\t' && input[searchIndex] != '\n' && input[searchIndex] != '\r')
                    return false;
                else
                    searchIndex++;

            returnObject = method.Invoke(null, new object[] { input.Substring(start, end - start) });
            return true;
        }

        private static object ParseToArray(string input, int start, int end, Type t, string debugName)
        {
            if(TryThryArrayParser(input, start, end, t, out object returnObject))
                return returnObject;

            IList list = (IList)ParseToList(input, start, end, t, debugName);
            if(list == null) return null;
            object return_array = Activator.CreateInstance(t, list.Count);
            list.CopyTo(return_array as Array, 0);       

            return return_array;
        }
        
        private static object ParseToList(string input, int start, int end, Type t, string debugName)
        {
            while(start < end && input[start] != '[')
                start++;
            while(end > start && input[end-1] != ']')
                end--;

            if(start == end)
                return null;

            start += 1;
            end -= 1;

            Type array_obj_type = t.GetElementType();
            IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(array_obj_type));

            int depth = 0;
            int variableStart = start;
            for (int i = start; i < end; i++)
            {
                if(depth == 0 && input[i] == ',' && (i == 0 || input[i - 1] != '\\'))
                {
                    list.Add(ParseJsonPart(input, variableStart, i, array_obj_type, debugName + "[" + (list.Count) + "]"));
                    variableStart = i + 1;
                }else if(i == end - 1)
                {
                    list.Add(ParseJsonPart(input, variableStart, i + 1, array_obj_type, debugName + "[" + (list.Count) + "]"));
                }
                else if (input[i] == '{' || input[i] == '[')
                    depth++;
                else if (input[i] == '}' || input[i] == ']')
                    depth--;
            }
            return list;
        }
        
        private static object ParseToEnum(string input, int start, int end, Type objtype)
        {
            input = input.Substring(start, end - start).Trim();
#if UNITY_2021_1_OR_NEWER
            if(Enum.TryParse(objtype, input, out object result))
                return result;
#else
            if(Enum.IsDefined(objtype, input))
                return Enum.Parse(objtype, input);
#endif
            Debug.LogWarning("The specified enum for " + objtype.Name + " does not exist. Existing Values are: " + Converter.ArrayToString(Enum.GetValues(objtype)));
            return Enum.GetValues(objtype).GetValue(0);
        }
        

#endregion
#region Converters

        // private static object ConvertToDictionary(object parsed, Type objtype)
        // {
        //     var returnObject = (dynamic)Activator.CreateInstance(objtype);
        //     Dictionary<object, object> dict = (Dictionary<object, object>)parsed;
        //     foreach (KeyValuePair<object, object> keyvalue in dict)
        //     {
        //         dynamic key = ParsedToObject(keyvalue.Key, objtype.GetGenericArguments()[0]);
        //         dynamic value = ParsedToObject(keyvalue.Value, objtype.GetGenericArguments()[1]);
        //         returnObject.Add(key , value );
        //     }
        //     return returnObject;
        // }
#endregion
#region Serializer
        //Serilizer
        private static string PrintIndent(int indent) => new string(' ', indent * 4);
        private static string SerializeDictionary(object obj, bool prettyPrint = false, int indent = 0)
        {
            indent += 1;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("{");
            foreach (KeyValuePair<object,object> item in (dynamic)obj)
            {
                if (prettyPrint)
                {
                    stringBuilder.Append("\n");
                    stringBuilder.Append(PrintIndent(indent));
                }
                stringBuilder.Append(Serialize(item.Key, prettyPrint, indent) + ": " + Serialize(item.Value, prettyPrint, indent) + ",");
            }
            if (stringBuilder.Length > 1)
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            if (prettyPrint)
            {
                stringBuilder.Append("\n");
                stringBuilder.Append(PrintIndent(indent-1));
            }
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }

        private static string SerializeClass(object obj, bool prettyPrint = false, int indent = 0)
        {
            // if it has SerializeForThryParser method, use ToString method. Include methods out of parent classes
            MethodInfo serializeMethod = obj.GetType().GetMethod("SerializeForThryParser", BindingFlags.NonPublic | BindingFlags.Instance);
            if (serializeMethod != null)
            {
                return (string)serializeMethod.Invoke(obj, new object[] { });
            }
                
            indent += 1;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("{");
            foreach(FieldInfo field in obj.GetType().GetFields())
            {
                if(prettyPrint)
                {
                    stringBuilder.Append("\n");
                    stringBuilder.Append(PrintIndent(indent));
                }
                if(field.IsPublic)
                    stringBuilder.Append("\""+field.Name + "\"" + ": " + Serialize(field.GetValue(obj), prettyPrint, indent) + ",");
            }
            foreach (PropertyInfo property in obj.GetType().GetProperties())
            {
                if (prettyPrint)
                {
                    stringBuilder.Append("\n");
                    stringBuilder.Append(PrintIndent(indent));
                }
                if(property.CanWrite && property.CanRead && property.GetIndexParameters().Length==0)
                    stringBuilder.Append("\""+ property.Name + "\"" + ": " + Serialize(property.GetValue(obj,null), prettyPrint, indent) + ",");
            }
            if (stringBuilder.Length > 1)
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            if (prettyPrint)
            {
                stringBuilder.Append("\n");
                stringBuilder.Append(PrintIndent(indent-1));
            }
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }

        private static string SerializeList(object obj, bool prettyPrint = false, int indent = 0)
        {
            indent += 1;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("[");
            foreach (object o in obj as IEnumerable)
            {
                if(prettyPrint)
                {
                    stringBuilder.Append("\n");
                    stringBuilder.Append(PrintIndent(indent));
                }
                stringBuilder.Append(Serialize(o, prettyPrint, indent));
                stringBuilder.Append(",");
            }
            if(stringBuilder.Length > 1)
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            if (prettyPrint)
            {
                stringBuilder.Append("\n");
                stringBuilder.Append(PrintIndent(indent-1));
            }
            stringBuilder.Append("]");
            return stringBuilder.ToString();
        }

        private static string SerializePrimitive(object obj)
        {
            if (obj.GetType() == typeof(string))
                return "\"" + obj + "\"";
            return obj.ToString().Replace(",", "."); ;
        }
#endregion
    }

#region Animation Parser
    public class AnimationParser
    {
        public class Animation
        {
            public PPtrCurve[] pPtrCurves;
        }

        public class PPtrCurve
        {
            public PPtrType curveType;
            public PPtrKeyframe[] keyframes;
        }

        public enum PPtrType
        {
            None,Material
        }

        public class PPtrKeyframe
        {
            public float time;
            public string guid;
            public int type;
        }

        public static Animation Parse(AnimationClip clip)
        {
            return Parse(AssetDatabase.GetAssetPath(clip));
        }

        public static Animation Parse(string path)
        {
            string data = FileHelper.ReadFileIntoString(path);

            List<PPtrCurve> pPtrCurves = new List<PPtrCurve>();
            int pptrIndex;
            int lastIndex = 0;
            while ((pptrIndex = data.IndexOf("m_PPtrCurves", lastIndex)) != -1)
            {
                lastIndex = pptrIndex + 1;
                int pptrEndIndex = data.IndexOf("  m_", pptrIndex);

                int curveIndex;
                int lastCurveIndex = pptrIndex;
                //find all curves
                while((curveIndex = data.IndexOf("  - curve:", lastCurveIndex, pptrEndIndex- lastCurveIndex)) != -1)
                {
                    lastCurveIndex = curveIndex + 1;
                    int curveEndIndex = data.IndexOf("    script: ", curveIndex);

                    PPtrCurve curve = new PPtrCurve();
                    List<PPtrKeyframe> keyframes = new List<PPtrKeyframe>();

                    int keyFrameIndex;
                    int lastKeyFrameIndex = curveIndex;
                    while((keyFrameIndex = data.IndexOf("    - time:", lastKeyFrameIndex, curveEndIndex - lastKeyFrameIndex)) != -1)
                    {
                        lastKeyFrameIndex = keyFrameIndex + 1;
                        int keyFrameEndIndex = data.IndexOf("}", keyFrameIndex);

                        PPtrKeyframe keyframe = new PPtrKeyframe();
                        keyframe.time = float.Parse(data.Substring(keyFrameIndex, data.IndexOf("\n", keyFrameIndex, keyFrameEndIndex)));
                        keyframes.Add(keyframe);
                    }

                    curve.curveType = data.IndexOf("    attribute: m_Materials", lastKeyFrameIndex, curveEndIndex - lastKeyFrameIndex) != -1 ? PPtrType.Material : PPtrType.None;
                    curve.keyframes = keyframes.ToArray();
                    pPtrCurves.Add(curve);
                }
            }
            Animation animation = new Animation();
            animation.pPtrCurves = pPtrCurves.ToArray();
            Debug.Log(Parser.Serialize(animation));
            return animation;
        }
    }
#endregion
}
