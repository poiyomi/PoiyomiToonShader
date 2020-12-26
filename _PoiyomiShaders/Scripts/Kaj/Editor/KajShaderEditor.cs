using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using UnityEditor;
using System;
using System.IO;
using System.Text.RegularExpressions;

// Material property drawers and a shader GUI because the base material inspector only needs slight improvement
// Meant to serve as a functionally-complete foundation for the base shader GUI with no shader-specific logic

namespace Kaj
{
    // Full colormask enum because UnityEngine.Rendering.ColorWriteMask doesn't have every option
    public enum ColorMask
    {
        None,
        Alpha,
        Blue,
        BA,
        Green,
        GA,
        GB,
        GBA,
        Red,
        RA,
        RB,
        RBA,
        RG,
        RGA,
        RGB,
        RGBA
    }

    // DX11 only blend operations
    public enum BlendOp
    {
        Add,
        Subtract,
        ReverseSubtract,
        Min,
        Max,
        LogicalClear,
        LogicalSet,
        LogicalCopy,
        LogicalCopyInverted,
        LogicalNoop,
        LogicalInvert,
        LogicalAnd,
        LogicalNand,
        LogicalOr,
        LogicalNor,
        LogicalXor,
        LogicalEquivalence,
        LogicalAndReverse,
        LogicalAndInverted,
        LogicalOrReverse,
        LogicalOrInverted
    }

    // Reusable enum for texture UV modes, technically shader specific but
    // it's either add this or remake the regular width Enum drawer
    public enum UVMapping
    {
        UV0,
        UV1,
        UV2,
        UV3,
        WorldTriplanar,
        ObjectTriplanar,
        XYWorldPlanar,
        YZWorldPlanar,
        ZXWorldPlanar,
        XYObjectPlanar,
        YZObjectPlanar,
        ZXObjectPlanar,
        Screenspace,
        Panosphere
    }

    // Simple indent and unindent decorators
    // 2px padding is still added around each decorator, might change to -2 height later
    public class IndentDecorator : MaterialPropertyDrawer
    {
        public override void OnGUI (Rect position, MaterialProperty prop, String label, MaterialEditor editor)
        {
            EditorGUI.indentLevel += 1;
        }

        public override float GetPropertyHeight (MaterialProperty prop, string label, MaterialEditor editor)
        {
            return 0f;
        }
    }
    public class UnIndentDecorator : MaterialPropertyDrawer
    {
        public override void OnGUI (Rect position, MaterialProperty prop, String label, MaterialEditor editor)
        {
            EditorGUI.indentLevel -= 1;
        }

        public override float GetPropertyHeight (MaterialProperty prop, string label, MaterialEditor editor)
        {
            return 0f;
        }
    }

    // Vector2 with a prefix to not cause conflicts with other (i.e. Thry's) drawers
    public class KajVector2Drawer : MaterialPropertyDrawer
    {
        public override void OnGUI (Rect position, MaterialProperty prop, String label, MaterialEditor editor)
        {
            EditorGUIUtility.labelWidth = 0;
            Vector2 v = new Vector2(prop.vectorValue.x, prop.vectorValue.y);
            EditorGUI.showMixedValue = prop.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            v = EditorGUI.Vector2Field(position, label, v);
            if (EditorGUI.EndChangeCheck())
                prop.vectorValue = new Vector4(v.x, v.y, prop.vectorValue.z, prop.vectorValue.w);
            EditorGUI.showMixedValue = false;
            editor.SetDefaultGUIWidths();
        }
    }

    // Vector3 with a prefix to not cause conflicts with other (i.e. Thry's) drawers
    public class KajVector3Drawer : MaterialPropertyDrawer
    {
        private readonly bool normalize = false;

        public KajVector3Drawer() { }
        public KajVector3Drawer(string f1) : this(new[] {f1}) {}
        public KajVector3Drawer(string[] flags)
        {
            foreach (string flag in flags)
            {
                if (flag == "Normalize") normalize = true;
            }
        }

        public override void OnGUI (Rect position, MaterialProperty prop, String label, MaterialEditor editor)
        {
            EditorGUIUtility.labelWidth = 0;
            Vector3 v = new Vector3(prop.vectorValue.x, prop.vectorValue.y, prop.vectorValue.z);
            EditorGUI.showMixedValue = prop.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            v = EditorGUI.Vector3Field(position, label, v);
            if (EditorGUI.EndChangeCheck())
            {
                if (normalize) v.Normalize();
                prop.vectorValue = new Vector4(v.x, v.y, v.z, prop.vectorValue.w);
            }
            EditorGUI.showMixedValue = false;
            editor.SetDefaultGUIWidths();
        }
    }

    // Basic single-line decorator-like label, uses property display name.  Kaj prefix to avoid name clashes
    public class KajLabelDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            position.y += 8; // This spacing could be an argument
            position = EditorGUI.IndentedRect(position);
            GUI.Label(position, label, EditorStyles.label);
        }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return EditorGUIUtility.singleLineHeight * 2f;
        }
    }

    // Enum with normal editor width, rather than MaterialEditor Default GUI widths
    // Would be nice if Decorators could access Drawers too so this wouldn't be necessary for something to trivial
    // Adapted from Unity interal MaterialEnumDrawer https://github.com/Unity-Technologies/UnityCsReference/
    public class WideEnumDrawer : MaterialPropertyDrawer
    {
        private readonly GUIContent[] names;
        private readonly float[] values;

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
        public WideEnumDrawer(string enumName)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(
                x => TypesFromAssembly(x)).ToArray();
            try
            {
                var enumType = types.FirstOrDefault(
                    x => x.IsEnum && (x.Name == enumName || x.FullName == enumName)
                );
                var enumNames = Enum.GetNames(enumType);
                names = new GUIContent[enumNames.Length];
                for (int i=0; i<enumNames.Length; ++i)
                    names[i] = new GUIContent(enumNames[i]);

                var enumVals = Enum.GetValues(enumType);
                values = new float[enumVals.Length];
                for (int i=0; i<enumVals.Length; ++i)
                    values[i] = (int)enumVals.GetValue(i);
            }
            catch (Exception)
            {
                Debug.LogWarningFormat("Failed to create  WideEnum, enum {0} not found", enumName);
                throw;
            }

        }
        
        public WideEnumDrawer(string n1, float v1) : this(new[] {n1}, new[] {v1}) {}
        public WideEnumDrawer(string n1, float v1, string n2, float v2) : this(new[] { n1, n2 }, new[] { v1, v2 }) {}
        public WideEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3) : this(new[] { n1, n2, n3 }, new[] { v1, v2, v3 }) {}
        public WideEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4) : this(new[] { n1, n2, n3, n4 }, new[] { v1, v2, v3, v4 }) {}
        public WideEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5) : this(new[] { n1, n2, n3, n4, n5 }, new[] { v1, v2, v3, v4, v5 }) {}
        public WideEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6) : this(new[] { n1, n2, n3, n4, n5, n6 }, new[] { v1, v2, v3, v4, v5, v6 }) {}
        public WideEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6, string n7, float v7) : this(new[] { n1, n2, n3, n4, n5, n6, n7 }, new[] { v1, v2, v3, v4, v5, v6, v7 }) {}
        public WideEnumDrawer(string[] enumNames, float[] vals)
        {
            names = new GUIContent[enumNames.Length];
            for (int i=0; i<enumNames.Length; ++i)
                names[i] = new GUIContent(enumNames[i]);

            values = new float[vals.Length];
            for (int i=0; i<vals.Length; ++i)
                values[i] = vals[i];
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            EditorGUI.showMixedValue = prop.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            var value = prop.floatValue;
            int selectedIndex = -1;
            for (int i=0; i<values.Length; i++)
                if (values[i] == value)
                {
                    selectedIndex = i;
                    break;
                }
 
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 0f;
            var selIndex = EditorGUI.Popup(position, label, selectedIndex, names);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
                prop.floatValue = values[selIndex];
             EditorGUIUtility.labelWidth = labelWidth;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

    // WideEnum but with keyword toggles, uses keywords in the property's displayname instead of
    // _PROPNAME_ENUMVALUE like the regular KeywordEnum
    public class WideKeywordEnumDrawer : MaterialPropertyDrawer
    {
        private readonly GUIContent[] names;
        const string keywordSeparatorChar = ";";
        
        public WideKeywordEnumDrawer(string n1) : this(new[] { n1 }) { }
        public WideKeywordEnumDrawer(string n1, string n2) : this(new[] { n1, n2 }) { }
        public WideKeywordEnumDrawer(string n1, string n2, string n3) : this(new[] { n1, n2, n3 }) { }
        public WideKeywordEnumDrawer(string n1, string n2, string n3, string n4) : this(new[] { n1, n2, n3, n4 }) { }
        public WideKeywordEnumDrawer(string n1, string n2, string n3, string n4, string n5) : this(new[] { n1, n2, n3, n4, n5 }) { }
        public WideKeywordEnumDrawer(string n1, string n2, string n3, string n4, string n5, string n6) : this(new[] { n1, n2, n3, n4, n5, n6 }) { }
        public WideKeywordEnumDrawer(string n1, string n2, string n3, string n4, string n5, string n6, string n7) : this(new[] { n1, n2, n3, n4, n5, n6, n7 }) { }
        public WideKeywordEnumDrawer(string[] enumNames)
        {
            names = new GUIContent[enumNames.Length];
            for (int i=0; i<enumNames.Length; ++i)
                names[i] = new GUIContent(enumNames[i]);
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            EditorGUI.showMixedValue = prop.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 0f;
            int oldValue = (int)prop.floatValue;
            var value = EditorGUI.Popup(position, label, oldValue, names);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = value;
                string[] split = prop.displayName.Split(keywordSeparatorChar[0]);
                foreach (Material m in editor.targets)
                {
                    m.DisableKeyword(split[oldValue+1]);
                    m.EnableKeyword(split[value+1]);
                }
            }
             EditorGUIUtility.labelWidth = labelWidth;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

    // Range drawer that looks for a property with the same name minus a 'Max' suffix if it exists or a matching
    // 'Min' property.  Then forces that value to be equal to this drawer's newly assigned value
    public class RangeMaxDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            EditorGUI.showMixedValue = prop.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            editor.RangeProperty(prop, label);
            if (EditorGUI.EndChangeCheck())
            {
                MaterialProperty[] props = MaterialEditor.GetMaterialProperties(editor.targets);
                MaterialProperty min = null;

                // Looping through this myself because MaterialEditor.GetMaterialProperty returns a broken prop
                if (prop.name.EndsWith("Max"))
                {
                    string baseName = prop.name.Remove(prop.name.Length - 3, 3);
                    foreach (MaterialProperty mp in props)
                        if (mp.name == baseName || mp.name == baseName + "Min")
                        {
                            min = mp;
                            break;
                        }
                }
                else
                {
                    foreach (MaterialProperty mp in props)
                        if (mp.name == prop.name + "Min")
                        {
                            min = mp;
                            break;
                        }
                }
                
                if (min != null)
                    if (min.floatValue > prop.floatValue)
                        min.floatValue = prop.floatValue;
            }
            EditorGUI.showMixedValue = false;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
           return -2f; // Remove the extra drawer padding
        }
    }

    // Range drawer that looks for a property with the same name minus a 'Min' suffix if it exists or a matching
    // 'Max' property.  Then forces that value to equal to this drawer's newly assigned value
    public class RangeMinDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            EditorGUI.showMixedValue = prop.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            editor.RangeProperty(prop, label);
            if (EditorGUI.EndChangeCheck())
            {
                MaterialProperty[] props = MaterialEditor.GetMaterialProperties(editor.targets);
                MaterialProperty max = null;

                // Looping through this myself because MaterialEditor.GetMaterialProperty returns a broken prop
                if (prop.name.EndsWith("Min"))
                {
                    string baseName = prop.name.Remove(prop.name.Length - 3, 3);
                    foreach (MaterialProperty mp in props)
                        if (mp.name == baseName || mp.name == baseName + "Max")
                        {
                            max = mp;
                            break;
                        }
                }
                else
                {
                    foreach (MaterialProperty mp in props)
                        if (mp.name == prop.name + "Max")
                        {
                            max = mp;
                            break;
                        }
                }
                
                if (max != null)
                    if (max.floatValue < prop.floatValue)
                        max.floatValue = prop.floatValue;
            }
            EditorGUI.showMixedValue = false;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return -2f; // Remove the extra drawer padding
        }
    }

    // Simple auto-laid out information box, uses materialproperty display name as text
    public class HelpBoxDrawer : MaterialPropertyDrawer
    {
        readonly MessageType type;

        public HelpBoxDrawer()
        {
            type = MessageType.Info;
        }

        public HelpBoxDrawer(float f)
        {
            type = (MessageType)(int)f;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            EditorGUILayout.HelpBox(label, type);
        }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
           return -4f; // Remove the extra drawer padding + helpbox extra padding
        }
    }

    // Regular float field but with a definable minimum for nonzero stuff and power exponents
    public class MinimumFloatDrawer : MaterialPropertyDrawer
    {
        private readonly float min;

        public MinimumFloatDrawer(float min)
        {
            this.min = min;
        }
        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            EditorGUI.showMixedValue = prop.hasMixedValue;
            EditorGUI.BeginChangeCheck();
            float f = editor.FloatProperty(prop, label);
            if (EditorGUI.EndChangeCheck())
            {
                if (f < min)
                   prop.floatValue = min;
            }
            EditorGUI.showMixedValue = false;
        }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
           return -2;
        }
    }

    // ToggleUI drawer with left side checkbox
    public class ToggleUILeftDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            EditorGUI.BeginChangeCheck();
            bool value = (prop.floatValue == 1);
            EditorGUI.showMixedValue = prop.hasMixedValue;
            value = EditorGUILayout.ToggleLeft(label, value);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
                prop.floatValue = value ? 1.0f : 0.0f;
        }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return 0;
        }
    }

    // Regular texture slot that enables a keyword when used
    public class KeywordTexDrawer : MaterialPropertyDrawer
    {
        private readonly string keyword;

        public KeywordTexDrawer(string keyword)
        {
            this.keyword = keyword;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;
            Texture t = editor.TextureProperty(position, prop, label, true);
            EditorGUI.showMixedValue = false;
            if (EditorGUI.EndChangeCheck())
            {
                if (t != null)
                    foreach (Material m in editor.targets)
                        m.EnableKeyword(keyword);
                else
                    foreach (Material m in editor.targets)
                        m.DisableKeyword(keyword);
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return base.GetPropertyHeight(prop, label, editor) + 54; // afaik this is the correct defualt texture height
        }
    }

    // Realtime GI flags drawer
    // Each of these "internal material settings" could be decorators, but saving them as material properties
    // allows shaders to access the setting values, just in case 
    public class GIFlagsDrawer : MaterialPropertyDrawer
    {
        readonly string[] enumNames;

        public GIFlagsDrawer()
        {
            enumNames = Enum.GetNames(typeof(MaterialGlobalIlluminationFlags));
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor materialEditor)
        {
            EditorGUI.showMixedValue = prop.hasMixedValue;
            var giFlags = (MaterialGlobalIlluminationFlags)prop.floatValue;
            EditorGUIUtility.labelWidth = 0f;
            EditorGUI.BeginChangeCheck();
            giFlags = (MaterialGlobalIlluminationFlags)EditorGUILayout.Popup(label, (int)giFlags, enumNames);
            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = (float)giFlags;
                foreach (Material m in materialEditor.targets)
                    m.globalIlluminationFlags = giFlags;
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return -2;
        }
    }

    // Disabled lightmodes mask drawer.
    public class DisabledLightModesDrawer : MaterialPropertyDrawer
    {
        readonly string[] enumNames;

        public DisabledLightModesDrawer()
        {
            enumNames = Enum.GetNames(typeof(LightMode));
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor materialEditor)
        {
            EditorGUI.showMixedValue = prop.hasMixedValue;
            EditorGUIUtility.labelWidth = 0f;
            EditorGUI.BeginChangeCheck();
            int lightModesMask = EditorGUILayout.MaskField(label, (int)prop.floatValue, enumNames);
            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = lightModesMask;
                SetMaterialsLightMode(materialEditor, lightModesMask);
            }
        }

        private void SetMaterialsLightMode(MaterialEditor materialEditor, int lightModesMask)
        {
            if ((lightModesMask & (int)LightMode.Always) != 0)
                SetMaterialsLightModeEnabled(materialEditor.targets, "Always", false);
            else SetMaterialsLightModeEnabled(materialEditor.targets, "Always", true);
            if ((lightModesMask & (int)LightMode.ForwardBase) != 0)
                SetMaterialsLightModeEnabled(materialEditor.targets, "ForwardBase", false);
            else SetMaterialsLightModeEnabled(materialEditor.targets, "ForwardBase", true);
            if ((lightModesMask & (int)LightMode.ForwardAdd) != 0)
                SetMaterialsLightModeEnabled(materialEditor.targets, "ForwardAdd", false);
            else SetMaterialsLightModeEnabled(materialEditor.targets, "ForwardAdd", true);
            if ((lightModesMask & (int)LightMode.Deferred) != 0)
                SetMaterialsLightModeEnabled(materialEditor.targets, "Deferred", false);
            else SetMaterialsLightModeEnabled(materialEditor.targets, "Deferred", true);
            if ((lightModesMask & (int)LightMode.ShadowCaster) != 0)
                SetMaterialsLightModeEnabled(materialEditor.targets, "ShadowCaster", false);
            else SetMaterialsLightModeEnabled(materialEditor.targets, "ShadowCaster", true);
            if ((lightModesMask & (int)LightMode.MotionVectors) != 0)
                SetMaterialsLightModeEnabled(materialEditor.targets, "MotionVectors", false);
            else SetMaterialsLightModeEnabled(materialEditor.targets, "MotionVectors", true);
            if ((lightModesMask & (int)LightMode.PrepassBase) != 0)
                SetMaterialsLightModeEnabled(materialEditor.targets, "PrepassBase", false);
            else SetMaterialsLightModeEnabled(materialEditor.targets, "PrepassBase", true);
            if ((lightModesMask & (int)LightMode.PrepassFinal) != 0)
                SetMaterialsLightModeEnabled(materialEditor.targets, "PrepassFinal", false);
            else SetMaterialsLightModeEnabled(materialEditor.targets, "PrepassFinal", true);
            if ((lightModesMask & (int)LightMode.Vertex) != 0)
                SetMaterialsLightModeEnabled(materialEditor.targets, "Vertex", false);
            else SetMaterialsLightModeEnabled(materialEditor.targets, "Vertex", true);
            if ((lightModesMask & (int)LightMode.VertexLMRGBM) != 0)
                SetMaterialsLightModeEnabled(materialEditor.targets, "VertexLMRGBM", false);
            else SetMaterialsLightModeEnabled(materialEditor.targets, "VertexLMRGBM", true);
            if ((lightModesMask & (int)LightMode.VertexLM) != 0)
                SetMaterialsLightModeEnabled(materialEditor.targets, "VertexLM", false);
            else SetMaterialsLightModeEnabled(materialEditor.targets, "VertexLM", true);
        }
        
        private void SetMaterialsLightModeEnabled(UnityEngine.Object[] mats, string pass, bool enabled)
        {
            foreach (Material m in mats)
                m.SetShaderPassEnabled(pass, enabled);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return -2;
        }
    }

    // Disable Batching enum drawer
    // Override tag drawers like DisableBatching, PreviewType, IgnoreProjector, and ForceNoShadowCasting COULD be one drawer type,
    // but without intruding into property name or displayName parsing, the arguments for the drawer would need to be 6 and 9 strings long!
    // Easier to hard code DisableBatching and PreviewType imo
    public class DisableBatchingDrawer : MaterialPropertyDrawer
    {
        enum DisableBatchingFlags
        {
            False,
            True,
            LODFading
        }

        readonly string[] enumNames;

        public DisableBatchingDrawer()
        {
            enumNames = Enum.GetNames(typeof(DisableBatchingFlags));
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor materialEditor)
        {
            EditorGUI.showMixedValue = prop.hasMixedValue;
            var batchingFlag = (DisableBatchingFlags)prop.floatValue;
            EditorGUIUtility.labelWidth = 0f;
            EditorGUI.BeginChangeCheck();
            batchingFlag = (DisableBatchingFlags)EditorGUILayout.Popup(label, (int)batchingFlag, enumNames);
            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = (float)batchingFlag;
                switch (batchingFlag)
                {
                    case DisableBatchingFlags.False:
                        foreach (Material m in materialEditor.targets)
                            m.SetOverrideTag("DisableBatching", "False");
                        break;
                    case DisableBatchingFlags.True:
                        foreach (Material m in materialEditor.targets)
                            m.SetOverrideTag("DisableBatching", "True");
                        break;
                    case DisableBatchingFlags.LODFading:
                        foreach (Material m in materialEditor.targets)
                            m.SetOverrideTag("DisableBatching", "LODFading");
                        break;
                }
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return -2;
        }
    }

    public class PreviewTypeDrawer : MaterialPropertyDrawer
    {
        public enum PreviewType
        {
            Sphere, // Actually called 'Mesh' internally, but regular users recognize the sphere/don't use other meshes
            Plane,
            Skybox
        }

        readonly string[] enumNames;

        public PreviewTypeDrawer()
        {
            enumNames = Enum.GetNames(typeof(PreviewType));
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor materialEditor)
        {
            EditorGUI.showMixedValue = prop.hasMixedValue;
            var previewTypeFlag = (PreviewType)prop.floatValue;
            EditorGUIUtility.labelWidth = 0f;
            EditorGUI.BeginChangeCheck();
            previewTypeFlag = (PreviewType)EditorGUILayout.Popup(label, (int)previewTypeFlag, enumNames);
            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = (float)previewTypeFlag;
                switch (previewTypeFlag)
                {
                    case PreviewType.Sphere:
                        foreach (Material m in materialEditor.targets)
                            m.SetOverrideTag("PreviewType", "");
                        break;
                    case PreviewType.Plane:
                        foreach (Material m in materialEditor.targets)
                            m.SetOverrideTag("PreviewType", "Plane");
                        break;
                    case PreviewType.Skybox:
                        foreach (Material m in materialEditor.targets)
                            m.SetOverrideTag("PreviewType", "Skybox");
                        break;
                }
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return -2;
        }
    }

    // IgnoreProjector, ForceNoShadowCasting, and CanUseSpriteAtlas drawer
    public class OverrideTagToggleDrawer : MaterialPropertyDrawer
    {
        readonly string tag;

        public OverrideTagToggleDrawer(string tag)
        {
            this.tag = tag;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor materialEditor)
        {
            EditorGUI.showMixedValue = prop.hasMixedValue;
            var flag = prop.floatValue;
            EditorGUIUtility.labelWidth = 0f;
            EditorGUI.BeginChangeCheck();
            flag = EditorGUILayout.Toggle(label, flag == 1) ? 1 : 0;
            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = flag;
                if (flag == 1)
                    foreach (Material m in materialEditor.targets)
                        m.SetOverrideTag(tag, "True");
                else 
                    foreach (Material m in materialEditor.targets)
                        m.SetOverrideTag(tag, "False");
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return -2;
        }
    }

    // GrabPass drawer as decorator since its entirely tag based
    public class GrabPassNamesDecorator : MaterialPropertyDrawer
    {
        bool m_FirstTimeApply = true;

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor materialEditor)
        {
            Material material = (Material)materialEditor.target;
            // Material-varying cacheing can't be done in the constructor because this will vary
            if (m_FirstTimeApply)
            {
                // If a new shader has been switched to, search the .shader file for
                // grabpass information and current shader name for override tags
                string currentShaderTag = material.GetTag("CurrentShader", false, "");
                // Skip the grabpass file scan if the information has already been set. However,
                // this will break if a shader changes its grabpass count/names with an update
                if (currentShaderTag == "" || currentShaderTag != material.shader.name)
                {
                    string shaderFilePath = AssetDatabase.GetAssetPath(material.shader);
                    string fileContents = null;
                    try
                    {
                        StreamReader sr = new StreamReader(shaderFilePath);
                        fileContents = sr.ReadToEnd();
                        sr.Close();
                    }
                    catch (IOException e)
                    {
                        Debug.LogError("[Kaj Shader Editor] Error reading shader file.  " + e.ToString());
                    }

                    // Quick scan, log all grabpasses
                    // Strict formatting requirements to make the scan fast; example: GrabPass { "_GrabTexture2000" }
                    int grabpassCount = 0;
                    int grabpassIndex = 0;
                    while ((grabpassIndex = fileContents.IndexOf("GrabPass {", grabpassIndex)+1) != 0)
                    {
                        int firstBraceIndex = fileContents.IndexOf("{", grabpassIndex);
                        int lastBraceIndex = fileContents.IndexOf("}", grabpassIndex);
                        string grabpassName = fileContents.Substring(firstBraceIndex, lastBraceIndex-firstBraceIndex);
                        string[] grabpassNameSplit = grabpassName.Split('\"');
                        if (grabpassNameSplit.Length > 1)
                        {
                            // Named grabpass, only non-empty tags get saved
                            grabpassName = grabpassNameSplit[1];
                            foreach (Material m in materialEditor.targets)
                            {
                                m.SetOverrideTag("GrabPass" + grabpassCount, grabpassName);
                                m.SetOverrideTag("GrabPassDefault" + grabpassCount, grabpassName);
                            }
                        }
                        grabpassCount++;
                    }

                    // Save current shader name
                    foreach (Material m in materialEditor.targets)
                    {
                        m.SetOverrideTag("GrabPassCount", grabpassCount.ToString());
                        m.SetOverrideTag("CurrentShader", material.shader.name);
                    }
                }
                m_FirstTimeApply = false;
            }

            int gbCount = Int32.Parse(material.GetTag("GrabPassCount", false, "0"));
            EditorGUIUtility.labelWidth = 0f;
            for (int i=0; i<gbCount; i++)
            {
                bool gbMixedValue = false;
                string grabpassName = material.GetTag("GrabPass" + i, false, "");
                foreach (Material m in materialEditor.targets)
                    if (m.GetTag("GrabPass" + i, false, "") != grabpassName)
                    {
                        gbMixedValue = true;
                        break;
                    }
                EditorGUI.showMixedValue = gbMixedValue;
                EditorGUI.BeginChangeCheck();
                string newName = EditorGUILayout.DelayedTextField("GrabPass" + i + " Name:", grabpassName);
                if (EditorGUI.EndChangeCheck())
                {
                    newName = Regex.Replace(newName, "[^A-Za-z0-9_]", "");

                    if (newName == "") // No name means reset to default
                        foreach (Material m in materialEditor.targets)
                            m.SetOverrideTag("GrabPass" + i, material.GetTag("GrabPassDefault" + i, false, ""));
                    else
                    {
                        // force start custom names with an underscore
                        if (!newName.StartsWith("_"))
                            newName = "_" + newName;
                        foreach (Material m in materialEditor.targets)
                            m.SetOverrideTag("GrabPass" + i, newName);
                    }
                }
                EditorGUI.showMixedValue = false;
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return -2;
        }
    }

    // Presets drawer, assigns groups of material properties/settings interpreted from the property's displayName
    // Primarily meant for Standard-style 'Rendering Mode' dropdown
    public class PresetsEnumDrawer : MaterialPropertyDrawer
    {
        string[] modeNames;
        string displayName;
        bool m_FirstTimeApply = true;

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor materialEditor)
        {
            if (m_FirstTimeApply)
            {
                // loop through displayname to parse actual displayname and enum option names
                // First is the display name, then each preset separated by ;
                // Individual preset values are separated by , with the preset name at the start
                string[] split = prop.displayName.Split(';');
                displayName = split[0];
                modeNames = new string[split.Length-1];
                for (int i=1;i<split.Length;i++)
                    modeNames[i-1] = split[i].Split(',')[0];
                m_FirstTimeApply = false;
            }

            EditorGUI.showMixedValue = prop.hasMixedValue;
            var mode = prop.floatValue;
            EditorGUIUtility.labelWidth = 0f;
            EditorGUI.BeginChangeCheck();
            mode = EditorGUILayout.Popup(displayName, (int)mode, modeNames);
            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = mode;
                string[] split = prop.displayName.Split(';');
                string[] renderModeSettings = split[(int)mode+1].Split(',');
                MaterialProperty[] props = MaterialEditor.GetMaterialProperties(materialEditor.targets);
                for (int i=1; i<renderModeSettings.Length; i++)
                {
                    string[] renderModeSetting = renderModeSettings[i].Split('=');
                    if (renderModeSetting[0] == "RenderQueue")
                        foreach (Material m in materialEditor.targets)
                            m.renderQueue = Int32.Parse(renderModeSetting[1]);
                    else if (renderModeSetting[0] == "RenderType")
                        foreach (Material m in materialEditor.targets)
                            m.SetOverrideTag("RenderType", renderModeSetting[1]);
                    else if (renderModeSetting[0].StartsWith("GrabPass"))
                        foreach (Material m in materialEditor.targets)
                            m.SetOverrideTag(renderModeSetting[0], renderModeSetting[1]);
                    else Array.Find(props, x => x.name == renderModeSetting[0]).floatValue = Single.Parse(renderModeSetting[1]);
                }
            }

        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return -2;
        }
    }

    // Drag and drop unity light slot specifically for Shader Lights
    // Technically the heirarchy path to the light could be saved in override tags so the light stays linked,
    // but there's no efficient way to have the material update in realtime when the light changes, so that
    // isn't implemented right now.
    public class ShaderLightDecorator : MaterialPropertyDrawer
    {
        readonly string propertyPrefix;
        Light light;

        public ShaderLightDecorator(string propertyPrefix)
        {
            this.propertyPrefix = propertyPrefix;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor materialEditor)
        {
            EditorGUIUtility.labelWidth = 0f;
            EditorGUI.BeginChangeCheck();
            light = (Light)EditorGUILayout.ObjectField("Copy a Light", light, typeof(Light), true);
            if (EditorGUI.EndChangeCheck())
            {
                if (light == null) return;
                // Assign values to concention properties with the given property prefix
                // Loop through matierlproperties manually because GetMaterialProperty is broken
                MaterialProperty[] props = MaterialEditor.GetMaterialProperties(materialEditor.targets);
                foreach (MaterialProperty p in props)
                {
                    if (p.name == propertyPrefix + "Color")
                        p.colorValue = light.color;
                    else if (p.name == propertyPrefix + "Mode")
                        switch (light.type)
                        {
                            case LightType.Point:
                                p.floatValue = 0;
                                break;
                            case LightType.Spot:
                                p.floatValue = 1;
                                break;
                            case LightType.Directional:
                                p.floatValue = 2;
                                break;
                        }
                    else if (p.name == propertyPrefix + "Specular")
                        switch (light.renderMode)
                        {
                            case LightRenderMode.Auto:
                            case LightRenderMode.ForcePixel:
                                p.floatValue = 1;
                                break;
                            case LightRenderMode.ForceVertex:
                                p.floatValue = 0;
                                break;
                        }
                    else if (p.name == propertyPrefix + "Position")
                        p.vectorValue = light.transform.position;
                    else if (p.name == propertyPrefix + "Direction")
                        p.vectorValue = light.transform.forward;
                    else if (p.name == propertyPrefix + "Angle")
                        p.floatValue = light.spotAngle;
                    else if (p.name == propertyPrefix + "Range")
                        p.floatValue = light.range;
                    else if (p.name == propertyPrefix + "Intensity")
                        p.floatValue = light.intensity;
                }
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return -2;
        }
    }

    // Minimalistic shader editor aimed at extending the base inspector with grouped foldouts, toggle foldouts,
    // and shader optimizer button+property disabling.  Also contains support for the above drawers if necessary.
    public class ShaderEditor : ShaderGUI
    {
        const string shaderOptimizerPropertyName = "_ShaderOptimizerEnabled";
        const string AnimatedPropertySuffix = "Animated";
        const string groupPrefix = "group_";
        const string togglePrefix = "toggle_"; // foldout combined with a checkbox i.e. group_toggle_Parallax
        const string endPrefix = "end_";
        const string keywordSeparatorChar = ";";
        GUIStyle foldoutStyle;
        bool m_FirstTimeApply = true;
        bool afterShaderOptimizerButton = false;
        MaterialProperty shaderOptimizer;
        bool[] propertyAnimated;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            shaderOptimizer = FindProperty(shaderOptimizerPropertyName, props, false);

            if (m_FirstTimeApply)
            {
                // Cache and initialize some stuff.  This could be done in the constructor
                // but the constructor doesn't get called again at domain reload afaik
                // (Domain reload resets variables but doesn't call the constructor again)
                foldoutStyle = new GUIStyle("ShurikenModuleTitle");
                foldoutStyle.font = new GUIStyle(EditorStyles.label).font;
                foldoutStyle.border = new RectOffset(15, 7, 4, 4);
                foldoutStyle.fixedHeight = 22;
                foldoutStyle.contentOffset = new Vector2(20f, -2f);

                // Clear all keywords to begin with, in case there are conflicts with different shaders
                foreach (Material m in materialEditor.targets)
                    foreach (string keyword in m.shaderKeywords)
                        m.DisableKeyword(keyword);

                // Dynamically check for float and texture properties with shader keywords corresponding to
                // a float value (toggles and keyword enums) or a texture slot being filled.  
                // Keywords are stored in the displayname with starting and ending semicolons like ";ALBEDO_USED;Albedo"
                // Keywords used this way are meant to make mega shaders more performant before being locked in and having
                // all keywords removed.  This also requires property drawers to enable these keywords.
                // Properties with ;KEYWORD; displaynames have the keywords and semicolons ignored when
                // they are drawn with this editor
                // Keywords aren't assigned if the optimizer is enabled
                if (shaderOptimizer!= null && shaderOptimizer.floatValue != 1)
                    foreach (Material m in materialEditor.targets)
                        foreach (MaterialProperty p in props)
                        {
                            if (!p.displayName.StartsWith(keywordSeparatorChar)) continue;

                            switch (p.type)
                            {
                                case MaterialProperty.PropType.Float:
                                case MaterialProperty.PropType.Range:
                                    m.EnableKeyword(p.displayName.Split(keywordSeparatorChar[0])[(int)p.floatValue+1]);
                                    break;
                                case MaterialProperty.PropType.Texture:
                                    if (m.GetTexture(p.name) != null)
                                        m.EnableKeyword(p.displayName.Split(keywordSeparatorChar[0])[2]);
                                    break;
                            }
                        }
                
                // Cache the animated state of each property to exclude them from being disabled when the material is locked
                if (propertyAnimated == null)
                    propertyAnimated = new bool[props.Length];
                for (int i=0;i<props.Length;i++)
                {
                    propertyAnimated[i] = false;
                    string animatedPropertyName = props[i].name + AnimatedPropertySuffix;
                    MaterialProperty animProp = FindProperty(animatedPropertyName, props, false);
                    if (animProp != null)
                        propertyAnimated[i] = (animProp.floatValue == 1);
                }

                m_FirstTimeApply = false;
            }

            materialEditor.SetDefaultGUIWidths();
            afterShaderOptimizerButton = false;
            DrawPropertiesGUIRecursive(materialEditor, props, 0, props.Length, 0);
            materialEditor.RenderQueueField();
            materialEditor.EnableInstancingField();
            materialEditor.DoubleSidedGIField();
        }

        // Recursive to easily deal with foldouts
        // the group header property can store a toggle value, and the end property stores whether or not the foldout is expanded
        // Any properties listed after the convention shader optimizer button are disabled when the optimizer is enabled
        protected void DrawPropertiesGUIRecursive(MaterialEditor materialEditor, MaterialProperty[] props, int startIndex, int propCount, int indentLevel)
        {
            // Hard reset each indent level because [UnIndent] decorator doesn't affect hidden properties i.e. group headers
            EditorGUI.indentLevel = indentLevel;

            for (var i=startIndex; i<startIndex+propCount; i++)
            {
                // Check for groups and toggle groups
                bool toggle = false;
                if (props[i].name.StartsWith(groupPrefix) && // props[i] is group property
                    ((props[i].flags & MaterialProperty.PropFlags.HideInInspector) != 0))
                {
                    if (props[i].name.StartsWith(groupPrefix + togglePrefix))
                        toggle = true;
                    
                    // Find matching end tag
                    string groupName;
                    if (toggle)
                        groupName = props[i].name.Substring(groupPrefix.Length+togglePrefix.Length);
                    else
                        groupName = props[i].name.Substring(groupPrefix.Length);
                    string endName = endPrefix + groupName;
                    int j = i+1;
                    bool foundEnd = false;
                    for (; j<startIndex+propCount; j++)
                        if (props[j].name == endName && // props[j] is group end property
                            ((props[j].flags & MaterialProperty.PropFlags.HideInInspector) != 0))
                            {
                                foundEnd = true;
                                break;
                            }
                    if (!foundEnd)
                    {
                        Debug.LogWarning("[Kaj Shader Editor] Group end for " + groupName + " not found! Skipping.");
                        continue;
                    }

                    // Draw particle system styled header with indentation applied
                    float indentPixels = indentLevel * 13f;
                    var rect = GUILayoutUtility.GetRect(0, 22f, foldoutStyle);
                    rect.width -= indentPixels;
                    rect.x += indentPixels;
                    if (afterShaderOptimizerButton && shaderOptimizer.floatValue == 1 && !propertyAnimated[i])
                        EditorGUI.BeginDisabledGroup(true);
                    if (toggle)
                        GUI.Box(rect, "", foldoutStyle);
                    else
                        GUI.Box(rect, props[i].displayName, foldoutStyle);
                    if (afterShaderOptimizerButton && shaderOptimizer.floatValue == 1 && !propertyAnimated[i])
                        EditorGUI.EndDisabledGroup();

                    // Draw foldout arrow
                    var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
                    bool expanded = props[j].floatValue == 1;
                    var e = Event.current;
                    if (e.type == EventType.Repaint)
                        EditorStyles.foldout.Draw(toggleRect, false, false, expanded, false);

                    // Toggle property
                    // Technically drawers besides Toggles work, but are VERY wonky and not resized properly
                    if (toggle)
                    {
                        // Toggle alignment from Thry's
                        Rect togglePropertyRect = new Rect(rect);
                        // Add 18 to skip foldout arrow, shift by indents because the original box rect is being used
                        togglePropertyRect.x += 18 - ((indentLevel) * 13);
                        togglePropertyRect.y += 1;
                        float labelWidth = EditorGUIUtility.labelWidth;
                        EditorGUIUtility.labelWidth = 0;
                        if (afterShaderOptimizerButton && shaderOptimizer.floatValue == 1 && !propertyAnimated[i])
                            EditorGUI.BeginDisabledGroup(true);
                        if (props[i].displayName.StartsWith(keywordSeparatorChar)) // skip keywords embedded into displaynames
                        {
                            string[] split = props[i].displayName.Split(keywordSeparatorChar[0]);
                            materialEditor.ShaderProperty(togglePropertyRect, props[i], split[split.Length-1]);
                        }
                        else materialEditor.ShaderProperty(togglePropertyRect, props[i], props[i].displayName);
                        if (afterShaderOptimizerButton && shaderOptimizer.floatValue == 1 && !propertyAnimated[i])
                            EditorGUI.EndDisabledGroup();
                        EditorGUIUtility.labelWidth = labelWidth;
                    }

                    // Activate foldout
                    // Set the value individually in each material rather than through props[j]'s floatValue because that is recorded
                    // in the animation clip editor.  Also doesn't make an undo set for it.
                    if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
                    {
                        if (props[j].floatValue == 1)
                            foreach (Material mat in materialEditor.targets)
                                mat.SetFloat(props[j].name, 0);
                        else 
                            foreach (Material mat in materialEditor.targets)
                                mat.SetFloat(props[j].name, 1);
                        e.Use();
                    }

                    // Recurse on props in this group
                    if (expanded)
                    {
                        int originalIndentLevel = EditorGUI.indentLevel;
                        EditorGUILayout.Space();
                        DrawPropertiesGUIRecursive(materialEditor, props, i+1, j-i-1, indentLevel+1);
                        EditorGUILayout.Space();
                        // Hard reset to original indent level because [UnIndent] decorators may need the hidden group end property, which isn't drawn
                        EditorGUI.indentLevel = originalIndentLevel;
                    }

                    // Continue past subgroup props
                    i += j-i; 
                }

                // Derived from MaterialEditor.PropertiesDefaultGUI https://github.com/Unity-Technologies/UnityCsReference/
                if ((props[i].flags & MaterialProperty.PropFlags.HideInInspector) != 0) 
                    continue;
                
                // Check the shader optimizer property before and after, it if changed, firstTimeApply needs to be done again
                float shaderOptimizerValue = 0;
                if (props[i].name == shaderOptimizerPropertyName)
                    shaderOptimizerValue = props[i].floatValue;

                float h = materialEditor.GetPropertyHeight(props[i], props[i].displayName);
                Rect r = EditorGUILayout.GetControlRect(true, h, EditorStyles.layerMaskField);
                if (afterShaderOptimizerButton && shaderOptimizer.floatValue == 1 && !propertyAnimated[i])
                    EditorGUI.BeginDisabledGroup(true);
                if (props[i].displayName.StartsWith(keywordSeparatorChar)) // skip keywords embedded into displaynames
                {
                    string[] split = props[i].displayName.Split(keywordSeparatorChar[0]);
                    materialEditor.ShaderProperty(r, props[i], split[split.Length-1]);
                }
                else materialEditor.ShaderProperty(r, props[i], props[i].displayName); // something throwing a warning here
                if (afterShaderOptimizerButton && shaderOptimizer.floatValue == 1 && !propertyAnimated[i])
                    EditorGUI.EndDisabledGroup();
                
                if (props[i].name == shaderOptimizerPropertyName)
                {
                    afterShaderOptimizerButton = true;
                    if (shaderOptimizerValue != props[i].floatValue)
                        m_FirstTimeApply = true;
                }
            }

        }
    }
}