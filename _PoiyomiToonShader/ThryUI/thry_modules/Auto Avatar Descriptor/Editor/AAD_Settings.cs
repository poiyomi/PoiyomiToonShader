using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class AAD_Settings : ModuleSettings
    {
        private static bool ValuesInit;
        private static AAD_Data data;

        private readonly string[] fallback_animation_options = { "Male", "Female", "None" };

        public AAD_Settings()
        {
            if (!ValuesInit)
                InitValues();
        }

        public class AAD_Data
        {
            public bool auto_fill = false;
            public int animation_fallback = 2;
            public bool force_fallback = false;
        }

        public static AAD_Data GetData()
        {
            if (!ValuesInit)
                InitValues();
            return data;
        }

        public override void Draw()
        {
            GUILayout.Label("Auto Avatar Descriptor",EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            data.auto_fill = GUILayout.Toggle(data.auto_fill,new GUIContent("Auto setup avatar descriptor", "Automatically setup the vrc_avatar_descriptor after adding it to a gameobject"));

            GUILayout.BeginHorizontal();
            data.animation_fallback = EditorGUILayout.Popup(data.animation_fallback, fallback_animation_options, GUILayout.MaxWidth(60));
            GUILayout.Label(new GUIContent(" Fallback Default Animation Set", "is applied by auto avatar descriptor if gender of avatar couldn't be determend"));
            GUILayout.EndHorizontal();

            data.force_fallback = GUILayout.Toggle(data.force_fallback, new GUIContent("Force Fallback Default Animation Set", "always set default animation set as fallback set"));

            if (EditorGUI.EndChangeCheck())
                Helper.SaveValueToFile("aap", Parser.ObjectToString(data), ModuleSettings.MODULES_CONFIG);
        }
        
        private static void InitValues()
        {
            string stringData = Helper.LoadValueFromFile("aap", ModuleSettings.MODULES_CONFIG);
            if (stringData != null)
                data = Parser.ParseToObject<AAD_Data>(stringData);
            else
                data = new AAD_Data();
            ValuesInit = true;
        }
    }
}
