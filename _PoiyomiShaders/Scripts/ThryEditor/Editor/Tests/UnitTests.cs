using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor
{
    public class UnitTests
    {
        [MenuItem("Thry/ThryEditor/Dev Test/Run Timed Tests", priority = 100)]
        static void RunTimeTest()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for (int i = 0; i < 1000; i++)
            {
                RunUnitTests();
            }

            sw.Stop();

            Debug.Log($"Ran 1000 tests in {sw.ElapsedMilliseconds}ms");
        }

        [MenuItem("Thry/ThryEditor/Dev Test/Run Parser Tests", priority = 100)]
        public static void RunUnitTests()
        {
            int testCount = 0;
            int passedTests = 0;
            // Parser Tests
            List<(Type, string)> tests = GetParserTests();
            foreach((Type t, string data) test in tests)
            {
                Debug.Log($"Running test {test.t.Name}");
                object obj = null;
                object obj2 = null;
                string serialized1 = null;
                string serialized2 = null;
                try
                {
                    obj = Parser.Deserialize(test.data, test.t);
                    serialized1 = Parser.Serialize(obj);
                    obj2 = Parser.Deserialize(serialized1, test.t);
                    serialized2 = Parser.Serialize(obj2);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to deserialize {test.t.Name} with error {e.Message}");
                    continue;
                }
                bool passed = serialized1 == serialized2 && serialized1 != null;
                Debug.Assert(passed, $"Serialization of {test.t.Name} failed. \nSerialized1: {serialized1} \nSerialized2: {serialized2}");
                passedTests += passed ? 1 : 0;
                testCount++;
            }

            TextAsset prettyPrint = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath("8a14ffedc8094d1409692f3adda63884"));
            Person deserialized = Parser.Deserialize<Person>(prettyPrint.text);
            Debug.Log($"Deserialized: {deserialized.surname}");
            string serialized = Parser.Serialize(deserialized, prettyPrint: true);
            bool passedPrettyPrint = prettyPrint.text.Replace("\r","") == serialized;
            Debug.Assert(passedPrettyPrint, $"Pretty print test failed. Expected: {prettyPrint.text} Got: {serialized}");
            testCount ++;
            passedTests += passedPrettyPrint ? 1 : 0;

            if(testCount == passedTests)
            {
                Debug.Log($"<color=#00ff00ff>Passed all tests</color>");
            }else
            {
                Debug.Log($"<color=#ff7f00ff>Passed {passedTests}/{testCount} tests</color>");
            }
        }

        class Person
        {
            public string surname;
            public string name;
            public int age;
            public House house;
            public class House
            {
                public string address;
                public string[] rooms;
            }
        }

        static List<(Type, string)> GetParserTests()
        {
            TextAsset txt = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath("aaf371d691a1f4d428144aae9cec4b5f"));
            // Document is formated as follows:
            // ##ClassName
            // <data>
            List<(Type, string)> tests = new List<(Type, string)>();
            foreach(string line in txt.text.Replace("\r", "").Split('\n'))
            {
                if (line.StartsWith("##"))
                {
                    string className = line.Substring(2);
                    Type type = Type.GetType(className);
                    if (type == null)
                    {
                        Debug.LogError($"Could not find type {className}");
                        continue;
                    }
                    tests.Add((type, ""));
                }else
                {
                    (Type, string) last = tests[tests.Count - 1];
                    last.Item2 += line + "\n";
                    tests[tests.Count - 1] = last;
                }
            }
            return tests;
        }
    }
}