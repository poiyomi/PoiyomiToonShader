using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor.Drawers
{
    // Enum with normal editor width, rather than MaterialEditor Default GUI widths
    // Would be nice if Decorators could access Drawers too so this wouldn't be necessary for something to trivial
    // Adapted from Unity interal MaterialEnumDrawer https://github.com/Unity-Technologies/UnityCsReference/
    public class ThryMaskDrawer : MaterialPropertyDrawer
    {
        private string[] _options;

        public static bool RenderLabel = true;

        // internal Unity AssemblyHelper can't be accessed
        private Type[] TypesFromAssembly(Assembly a)
        {
            if (a == null)
                return new Type[0];
            try
            {
                return a.GetTypes();
            }
            catch (ReflectionTypeLoadException)
            {
                return new Type[0];
            }
        }
        public ThryMaskDrawer(string enumName)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(
                x => TypesFromAssembly(x)).ToArray();
            try
            {
                var enumType = types.FirstOrDefault(
                    x => x.IsEnum && (x.Name == enumName || x.FullName == enumName)
                );
                _options = enumType.GetEnumNames();
            }
            catch (Exception)
            {
                Debug.LogWarningFormat("Failed to create  FlagsEnum, enum {0} not found", enumName);
                throw;
            }

        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            EditorGUI.showMixedValue = prop.hasMixedValue;
            int mask = (int)prop.GetNumber();

            // Custom Change Check, so it triggers on reselect too
            EditorGUI.BeginChangeCheck();
            int newMask = mask;
            if (RenderLabel)
                newMask = EditorGUI.MaskField(position, label, newMask, _options);
            else
                newMask = EditorGUI.MaskField(position, newMask, _options);
            EditorGUI.showMixedValue = false;
            if (newMask == -1)
            {
                // -1 = Everything selected, create a mask with all bits set
                newMask = (int)Math.Pow(2, _options.Length) - 1;
            }
            if (EditorGUI.EndChangeCheck() && newMask != mask)
                {
                    // Set GUI.changed to true, so it triggers a change event, even on reselection
                    Debug.Log("new mask: " + newMask);
                    GUI.changed = true;
                    prop.SetNumber(newMask);
                }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            ShaderProperty.RegisterDrawer(this);
            return base.GetPropertyHeight(prop, label, editor);
        }
    }
}

namespace Thry
{
    public enum ColorMaskFlags
    {
        Alpha, // bit 0
        Blue, // bit 1
        Green, // bit 2
        Red, // bit 3
    }
}