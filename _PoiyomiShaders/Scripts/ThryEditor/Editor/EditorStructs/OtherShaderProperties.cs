using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Thry.ThryEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Thry
{
    public class RenderQueueProperty : ShaderProperty
    {
        public RenderQueueProperty(ShaderEditor shaderEditor) : base(shaderEditor, "RenderQueue", 0, "", "Change the Queue at which the material is rendered.", 0)
        {
            _doCustomDrawLogic = true;
            IsAnimatable = false;
            CustomStringTagID = "RenderQueue";
        }

        public override void DrawDefault()
        {
            ActiveShaderEditor.Editor.RenderQueueField();
        }

        public override void CopyFromMaterial(Material sourceM, bool isTopCall = false)
        {
            foreach (Material m in ActiveShaderEditor.Materials) m.renderQueue = sourceM.renderQueue;
        }
        public override void CopyToMaterial(Material targetM, bool isTopCall = false, MaterialProperty.PropType[] skipPropertyTypes = null)
        {
            targetM.renderQueue = ActiveShaderEditor.Materials[0].renderQueue;
        }
    }
    public class VRCFallbackProperty : ShaderProperty
    {
        static string[] s_fallbackShaderTypes = { "Standard", "Toon", "Unlit", "VertexLit", "Particle", "Sprite", "Matcap", "MobileToon" };
        static string[] s_fallbackRenderTypes = { "Opaque", "Cutout", "Transparent", "Fade" };
        static string[] s_fallbackRenderTypesValues = { "", "Cutout", "Transparent", "Fade" };
        static string[] s_fallbackCullTypes = { "OneSided", "DoubleSided" };
        static string[] s_fallbackCullTypesValues = { "", "DoubleSided" };
        static string[] s_fallbackNoTypes = { "None", "Hidden" };
        static string[] s_fallbackNoTypesValues = { "", "Hidden" };
        static string[] s_vRCFallbackOptionsPopup = s_fallbackNoTypes.Union(s_fallbackShaderTypes.SelectMany(s => s_fallbackRenderTypes.SelectMany(r => s_fallbackCullTypes.Select(c => r + "/" + c).Select(rc => s + "/" + rc)))).ToArray();
        static string[] s_vRCFallbackOptionsValues = s_fallbackNoTypes.Union(s_fallbackShaderTypes.SelectMany(s => s_fallbackRenderTypesValues.SelectMany(r => s_fallbackCullTypesValues.Select(c => r + c).Select(rc => s + rc)))).ToArray();

        public VRCFallbackProperty(ShaderEditor shaderEditor) : base(shaderEditor, "VRCFallback", 0, "", "Select the shader VRChat should use when your shaders are being hidden.", 0)
        {
            _doCustomDrawLogic = true;
            IsAnimatable = false;
            CustomStringTagID = "VRCFallback";
            IsExemptFromLockedDisabling = true;
        }

        public override void DrawDefault()
        {
            string current = ActiveShaderEditor.Materials[0].GetTag("VRCFallback", false, "None");
            EditorGUI.BeginChangeCheck();
            int selected = EditorGUILayout.Popup("VRChat Fallback Shader", s_vRCFallbackOptionsValues.Select((f, i) => (f, i)).FirstOrDefault(f => f.f == current).i, s_vRCFallbackOptionsPopup);
            if (EditorGUI.EndChangeCheck())
            {
                foreach (Material m in ActiveShaderEditor.Materials)
                {
                    m.SetOverrideTag("VRCFallback", s_vRCFallbackOptionsValues[selected]);
                    EditorUtility.SetDirty(m);
                }
            }
        }

        public override void CopyFromMaterial(Material sourceM, bool isTopCall = false)
        {
            string value = sourceM.GetTag("VRCFallback", false, "None");
            foreach (Material m in ActiveShaderEditor.Materials) m.SetOverrideTag("VRCFallback", value);
        }
        public override void CopyToMaterial(Material targetM, bool isTopCall = false, MaterialProperty.PropType[] skipPropertyTypes = null)
        {
            string value = ActiveShaderEditor.Materials[0].GetTag("VRCFallback", false, "None");
            targetM.SetOverrideTag("VRCFallback", value);
        }
    }
    public class InstancingProperty : ShaderProperty
    {
        public InstancingProperty(ShaderEditor shaderEditor, MaterialProperty materialProperty, string displayName, int xOffset, string optionsRaw, bool forceOneLine, int property_index) : base(shaderEditor, materialProperty, displayName, xOffset, optionsRaw, forceOneLine, property_index)
        {
            _doCustomDrawLogic = true;
            IsAnimatable = false;
        }

        public override void DrawDefault()
        {
            ActiveShaderEditor.Editor.EnableInstancingField();
        }
    }
    public class GIProperty : ShaderProperty
    {
        public GIProperty(ShaderEditor shaderEditor, MaterialProperty materialProperty, string displayName, int xOffset, string optionsRaw, bool forceOneLine, int property_index) : base(shaderEditor, materialProperty, displayName, xOffset, optionsRaw, forceOneLine, property_index)
        {
            _doCustomDrawLogic = true;
            IsAnimatable = false;
        }

        public override void DrawInternal(GUIContent content, Rect? rect = null, bool useEditorIndent = false, bool isInHeader = false)
        {
            base.DrawInternal(content, rect, useEditorIndent, isInHeader);
        }

        public override void DrawDefault()
        {
            LightmapEmissionFlagsProperty(XOffset, false);
        }

        public static readonly GUIContent lightmapEmissiveLabel = EditorGUIUtility.TrTextContent("Global Illumination", "Controls if the emission is baked or realtime.\n\nBaked only has effect in scenes where baked global illumination is enabled.\n\nRealtime uses realtime global illumination if enabled in the scene. Otherwise the emission won't light up other objects.");
        public static GUIContent[] lightmapEmissiveStrings = { EditorGUIUtility.TrTextContent("Realtime"), EditorGUIUtility.TrTextContent("Baked"), EditorGUIUtility.TrTextContent("None") };
        public static int[] lightmapEmissiveValues = { (int)MaterialGlobalIlluminationFlags.RealtimeEmissive, (int)MaterialGlobalIlluminationFlags.BakedEmissive, (int)MaterialGlobalIlluminationFlags.None };

        public static void FixupEmissiveFlag(Material mat)
        {
            if (mat == null)
                throw new System.ArgumentNullException("mat");

            mat.globalIlluminationFlags = FixupEmissiveFlag(mat.GetColor("_EmissionColor"), mat.globalIlluminationFlags);
        }

        public static MaterialGlobalIlluminationFlags FixupEmissiveFlag(Color col, MaterialGlobalIlluminationFlags flags)
        {
            if ((flags & MaterialGlobalIlluminationFlags.BakedEmissive) != 0 && col.maxColorComponent == 0.0f) // flag black baked
                flags |= MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            else if (flags != MaterialGlobalIlluminationFlags.EmissiveIsBlack) // clear baked flag on everything else, unless it's explicity disabled
                flags &= MaterialGlobalIlluminationFlags.AnyEmissive;
            return flags;
        }

        public void LightmapEmissionFlagsProperty(int indent, bool enabled)
        {
            LightmapEmissionFlagsProperty(indent, enabled, false);
        }

        public void LightmapEmissionFlagsProperty(int indent, bool enabled, bool ignoreEmissionColor)
        {
            // Calculate isMixed
            MaterialGlobalIlluminationFlags any_em = MaterialGlobalIlluminationFlags.AnyEmissive;
            MaterialGlobalIlluminationFlags giFlags = ActiveShaderEditor.Materials[0].globalIlluminationFlags & any_em;
            bool isMixed = false;
            for (int i = 1; i < ActiveShaderEditor.Materials.Length; i++)
            {
                if ((ActiveShaderEditor.Materials[i].globalIlluminationFlags & any_em) != giFlags)
                {
                    isMixed = true;
                    break;
                }
            }

            EditorGUI.BeginChangeCheck();

            // Show popup
            EditorGUI.showMixedValue = isMixed;
            giFlags = (MaterialGlobalIlluminationFlags)EditorGUILayout.IntPopup(lightmapEmissiveLabel, (int)giFlags, lightmapEmissiveStrings, lightmapEmissiveValues);
            EditorGUI.showMixedValue = false;

            // Apply flags. But only the part that this tool modifies (RealtimeEmissive, BakedEmissive, None)
            bool applyFlags = EditorGUI.EndChangeCheck();
            foreach (Material mat in ActiveShaderEditor.Materials)
            {
                mat.globalIlluminationFlags = applyFlags ? giFlags : mat.globalIlluminationFlags;
                if (!ignoreEmissionColor)
                {
                    FixupEmissiveFlag(mat);
                }
            }
        }
    }
    public class DSGIProperty : ShaderProperty
    {
        public DSGIProperty(ShaderEditor shaderEditor, MaterialProperty materialProperty, string displayName, int xOffset, string optionsRaw, bool forceOneLine, int property_index) : base(shaderEditor, materialProperty, displayName, xOffset, optionsRaw, forceOneLine, property_index)
        {
            _doCustomDrawLogic = true;
            IsAnimatable = false;
        }

        public override void DrawDefault()
        {
            ActiveShaderEditor.Editor.DoubleSidedGIField();
        }
    }
    public class LocaleProperty : ShaderProperty
    {
        public LocaleProperty(ShaderEditor shaderEditor, MaterialProperty materialProperty, string displayName, int xOffset, string optionsRaw, bool forceOneLine, int property_index) : base(shaderEditor, materialProperty, displayName, xOffset, optionsRaw, forceOneLine, property_index)
        {
            _doCustomDrawLogic = true;
            IsAnimatable = false;
        }

        public override void DrawInternal(GUIContent content, Rect? rect = null, bool useEditorIndent = false, bool isInHeader = false)
        {
            ShaderEditor.Active.Locale.DrawDropdown(rect.Value);
        }
    }
}