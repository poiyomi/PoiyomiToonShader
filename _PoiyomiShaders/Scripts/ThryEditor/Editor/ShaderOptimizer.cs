//Original Code from https://github.com/DarthShader/Kaj-Unity-Shaders
/**MIT License

Copyright (c) 2020 DarthShader

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.**/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using Object = UnityEngine.Object;
using System.Reflection;
#if VRC_SDK_VRCSDK3
using VRC.SDKBase;
#endif
#if VRC_SDK_VRCSDK2
using VRCSDK2;
#endif
#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
using VRC.SDKBase.Editor.BuildPipeline;
#endif
#if VRC_SDK_VRCSDK3 && !UDON
using static VRC.SDK3.Avatars.Components.VRCAvatarDescriptor;
using VRC.SDK3.Avatars.Components;
#endif

namespace Thry
{
    
    public enum LightMode
    {
        Always=1,
        ForwardBase=2,
        ForwardAdd=4,
        Deferred=8,
        ShadowCaster=16,
        MotionVectors=32,
        PrepassBase=64,
        PrepassFinal=128,
        Vertex=256,
        VertexLMRGBM=512,
        VertexLM=1024
    }

    // Static methods to generate new shader files with in-place constants based on a material's properties
    // and link that new shader to the material automatically
    public class ShaderOptimizer
    {
        // Tags
        public const string TAG_ORIGINAL_SHADER = "OriginalShader";
        public const string TAG_ALL_MATERIALS_GUIDS_USING_THIS_LOCKED_SHADER = "AllLockedGUIDS";
        //When locking don't include code from define blocks that are not enabled
        const bool REMOVE_UNUSED_IF_DEFS = true;

        // For some reason, 'if' statements with replaced constant (literal) conditions cause some compilation error
        // So until that is figured out, branches will be removed by default
        // Set to false if you want to keep UNITY_BRANCH and [branch]
        public static bool RemoveUnityBranches = true;

        // LOD Crossfade Dithing doesn't have multi_compile keyword correctly toggled at build time (its always included) so
        // this hard-coded material property will uncomment //#pragma multi_compile _ LOD_FADE_CROSSFADE in optimized .shader files
        public static readonly string LODCrossFadePropertyName = "_LODCrossfade";

        // IgnoreProjector and ForceNoShadowCasting don't work as override tags, so material properties by these names
        // will determine whether or not //"IgnoreProjector"="True" etc. will be uncommented in optimized .shader files
        public static readonly string IgnoreProjectorPropertyName = "_IgnoreProjector";
        public static readonly string ForceNoShadowCastingPropertyName = "_ForceNoShadowCasting";

        // Material property suffix that controls whether the property of the same name gets baked into the optimized shader
        // e.g. if _Color exists and _ColorAnimated = 1, _Color will not be baked in
        public static readonly string AnimatedPropertySuffix = "Animated";
        public static readonly string AnimatedTagSuffix = "Animated";
        public static readonly string ExemptFromLockingSuffix = "NL";

        // Currently, Material.SetShaderPassEnabled doesn't work on "ShadowCaster" lightmodes,
        // and doesn't let "ForwardAdd" lights get turned into vertex lights if "ForwardAdd" is simply disabled
        // vs. if the pases didn't exist at all in the shader.
        // The Optimizer will take a mask property by this name and attempt to correct these issues
        // by hard-removing the shadowcaster and fwdadd passes from the shader being optimized.
        public static readonly string DisabledLightModesPropertyName = "_LightModes";

        // Property that determines whether or not to evaluate KSOInlineSamplerState comments.
        // Inline samplers can be used to get a wider variety of wrap/filter combinations at the cost
        // of only having 1x anisotropic filtering on all textures
        public static readonly string UseInlineSamplerStatesPropertyName = "_InlineSamplerStates";
        private static bool UseInlineSamplerStates = true;

        // Material properties are put into each CGPROGRAM as preprocessor defines when the optimizer is run.
        // This is mainly targeted at culling interpolators and lines that rely on those interpolators.
        // (The compiler is not smart enough to cull VS output that isn't used anywhere in the PS)
        // Additionally, simply enabling the optimizer can define a keyword, whose name is stored here.
        // This keyword is added to the beginning of all passes, right after CGPROGRAM
        public static readonly string OptimizerEnabledKeyword = "OPTIMIZER_ENABLED";

        // Mega shaders are expected to have geometry and tessellation shaders enabled by default,
        // but with the ability to be disabled by convention property names when the optimizer is run.
        // Additionally, they can be removed per-lightmode by the given property name plus 
        // the lightmode name as a suffix (e.g. group_toggle_GeometryShadowCaster)
        // Geometry and Tessellation shaders are REMOVED by default, but if the main gorups
        // are enabled certain pass types are assumed to be ENABLED
        public static readonly string GeometryShaderEnabledPropertyName = "GeometryShader_Enabled";
        public static readonly string TessellationEnabledPropertyName = "Tessellation_Enabled";
        private static bool UseGeometry = false;
        private static bool UseGeometryForwardBase = true;
        private static bool UseGeometryForwardAdd = true;
        private static bool UseGeometryShadowCaster = true;
        private static bool UseGeometryMeta = true;
        private static bool UseTessellation = false;
        private static bool UseTessellationForwardBase = true;
        private static bool UseTessellationForwardAdd = true;
        private static bool UseTessellationShadowCaster = true;
        private static bool UseTessellationMeta = false;

        // Tessellation can be slightly optimized with a constant max tessellation factor attribute
        // on the hull shader.  A non-animated property by this name will replace the argument of said
        // attribute if it exists.
        public static readonly string TessellationMaxFactorPropertyName = "_TessellationFactorMax";

        enum LightModeType { None, ForwardBase, ForwardAdd, ShadowCaster, Meta };
        private static LightModeType CurrentLightmode = LightModeType.None;

        // In-order list of inline sampler state names that will be replaced by InlineSamplerState() lines
        public static readonly string[] InlineSamplerStateNames = new string[]
        {
            "_linear_repeat",
            "_linear_clamp",
            "_linear_mirror",
            "_linear_mirroronce",
            "_point_repeat",
            "_point_clamp",
            "_point_mirror",
            "_point_mirroronce",
            "_trilinear_repeat",
            "_trilinear_clamp",
            "_trilinear_mirror",
            "_trilinear_mirroronce"
        };

        // Would be better to dynamically parse the "C:\Program Files\UnityXXXX\Editor\Data\CGIncludes\" folder
        // to get version specific includes but eh
        public static readonly HashSet<string> DefaultUnityShaderIncludes = new HashSet<string>()
        {
            "UnityUI.cginc",
            "AutoLight.cginc",
            "GLSLSupport.glslinc",
            "HLSLSupport.cginc",
            "Lighting.cginc",
            "SpeedTreeBillboardCommon.cginc",
            "SpeedTreeCommon.cginc",
            "SpeedTreeVertex.cginc",
            "SpeedTreeWind.cginc",
            "TerrainEngine.cginc",
            "TerrainSplatmapCommon.cginc",
            "Tessellation.cginc",
            "UnityBuiltin2xTreeLibrary.cginc",
            "UnityBuiltin3xTreeLibrary.cginc",
            "UnityCG.cginc",
            "UnityCG.glslinc",
            "UnityCustomRenderTexture.cginc",
            "UnityDeferredLibrary.cginc",
            "UnityDeprecated.cginc",
            "UnityGBuffer.cginc",
            "UnityGlobalIllumination.cginc",
            "UnityImageBasedLighting.cginc",
            "UnityInstancing.cginc",
            "UnityLightingCommon.cginc",
            "UnityMetaPass.cginc",
            "UnityPBSLighting.cginc",
            "UnityShaderUtilities.cginc",
            "UnityShaderVariables.cginc",
            "UnityShadowLibrary.cginc",
            "UnitySprites.cginc",
            "UnityStandardBRDF.cginc",
            "UnityStandardConfig.cginc",
            "UnityStandardCore.cginc",
            "UnityStandardCoreForward.cginc",
            "UnityStandardCoreForwardSimple.cginc",
            "UnityStandardInput.cginc",
            "UnityStandardMeta.cginc",
            "UnityStandardParticleInstancing.cginc",
            "UnityStandardParticles.cginc",
            "UnityStandardParticleShadow.cginc",
            "UnityStandardShadow.cginc",
            "UnityStandardUtils.cginc"
        };
        
        public static readonly HashSet<char> ValidSeparators = new HashSet<char>() { ' ', '\t', '\r', '\n', ';', ',', '.', '(', ')', '[', ']', '{', '}', '>', '<', '=', '!', '&', '|', '^', '+', '-', '*', '/', '#' };

        public static readonly HashSet<string> DontRemoveIfBranchesKeywords = new HashSet<string>() { "UNITY_SINGLE_PASS_STEREO", "FORWARD_BASE_PASS", "FORWARD_ADD_PASS", "POINT", "SPOT" };
        public static readonly HashSet<string> KeywordsUsedByPragmas = new HashSet<string>() {  };

        public static readonly string[] ValidPropertyDataTypes = new string[]
        {
            "int",
            "float",
            "float2",
            "float3",
            "float4",
            "half",
            "half2",
            "half3",
            "half4",
            "fixed",
            "fixed2",
            "fixed3",
            "fixed4"
        };

        public static readonly HashSet<string> IllegalPropertyRenames = new HashSet<string>()
        {
            "_MainTex",
            "_Color",
            "_EmissionColor",
            "_BumpScale",
            "_Cutoff",
            "_DetailNormalMapScale",
            "_DstBlend",
            "_GlossMapScale",
            "_Glossiness",
            "_GlossyReflections",
            "_Metallic",
            "_Mode",
            "_OcclusionStrength",
            "_Parallax",
            "_SmoothnessTextureChannel",
            "_SpecularHighlights",
            "_SrcBlend",
            "_UVSec",
            "_ZWrite"
        };
        
        public static readonly HashSet<string> PropertiesToSkipInMaterialEquallityComparission = new HashSet<string>
        {
            "shader_master_label",
            "shader_is_using_thry_editor"
        };

        public enum PropertyType
        {
            Vector,
            Float
        }

        public class PropertyData
        {
            public PropertyType type;
            public string name;
            public Vector4 value;
            public string lastDeclarationType;
        }

        public class Macro
        {
            public string name;
            public string[] args;
            public string contents;
        }

        public class ParsedShaderFile
        {
            public string filePath;
            public string[] lines;
        }

        public class TextureProperty
        {
            public string name;
            public Texture texture;
            public int uv;
            public Vector2 scale;
            public Vector2 offset;
        }

        public class GrabPassReplacement
        {
            public string originalName;
            public string newName;
        }

        public static void CopyAnimatedTagToMaterials(Material[] targets, MaterialProperty source)
        {
            string val = (source.targets[0] as Material).GetTag(source.name + AnimatedTagSuffix, false, "");
            foreach (Material m in targets)
            {
                m.SetOverrideTag(source.name+ AnimatedTagSuffix, val);
            }
        }

        public static void CopyAnimatedTagFromMaterial(Material source, MaterialProperty target)
        {
            string val = source.GetTag(target.name + AnimatedTagSuffix, false, "");
            foreach (Material m in target.targets)
            {
                m.SetOverrideTag(target.name + AnimatedTagSuffix, val);
            }
        }

        public static void CopyAnimatedTagFromProperty(MaterialProperty source, MaterialProperty target)
        {
            string val = (source.targets[0] as Material).GetTag(source.name + AnimatedTagSuffix, false, "");
            foreach (Material m in target.targets)
            {
                m.SetOverrideTag(target.name + AnimatedTagSuffix, val);
            }
        }

        public static void SetAnimatedTag(MaterialProperty prop, string value)
        {
            foreach (Material m in prop.targets)
            {
                m.SetOverrideTag(prop.name + AnimatedTagSuffix, value);
            }
        }

        public static string GetAnimatedTag(MaterialProperty prop)
        {
            return (prop.targets[0] as Material).GetTag(prop.name + AnimatedTagSuffix, false, "");
        }

        public static string GetAnimatedTag(Material m, string prop)
        {
            return m.GetTag(prop + AnimatedTagSuffix, false, "");
        }

        public static bool IsAnimated(Material m, string prop)
        {
            return m.GetTag(prop + AnimatedTagSuffix, false, "0") != "0";
        }

        public static string CleanStringForPropertyNames(string s)
        {
            s = s.Trim().Replace(" ", "");
            var nameByteArray = System.Text.Encoding.UTF8.GetBytes(s);
            string cleaned = "";
            for (var i = 0; i < nameByteArray.Length; i++)
            {
                if ((nameByteArray[i] >= 65 && nameByteArray[i] <= 122 && nameByteArray[i] != 91 && nameByteArray[i] != 92 && nameByteArray[i] != 93 && nameByteArray[i] != 94 && nameByteArray[i] != 96) || // word characters
                    (nameByteArray[i] >= 48 && nameByteArray[i] <= 57)) // numbers
                {
                    cleaned += System.Text.Encoding.UTF8.GetString(new byte[] { nameByteArray[i] });
                }
                else
                {
                    cleaned += nameByteArray[i].ToString("X2");
                }
            }
            return cleaned;
        }


        public static string GetRenamedPropertySuffix(Material m)
        {
            return CleanStringForPropertyNames(m.GetTag("thry_rename_suffix", false, m.name));
        }

        public static bool HasCustomRenameSuffix(Material m)
        {
            string cleanedMaterialName = CleanStringForPropertyNames(m.name);
            string suffix = m.GetTag("thry_rename_suffix", false, cleanedMaterialName);
            return suffix != cleanedMaterialName;
        }

        struct RenamingProperty
        {
            public MaterialProperty Prop;
            public string Keyword;
            public string Replace;
            public RenamingProperty(MaterialProperty prop, string keyword, string replace)
            {
                this.Prop = prop;
                this.Keyword = keyword;
                this.Replace = replace;
            }
        }

        public static bool IsPropertyExcemptFromLocking(MaterialProperty prop)
        {
            // if not a texture, but has non-modifiable texture data flag, is used as indicator to prevent locking
            return prop.displayName.EndsWith(ExemptFromLockingSuffix, StringComparison.Ordinal) || (prop.type != MaterialProperty.PropType.Texture && prop.flags.HasFlag(MaterialProperty.PropFlags.NonModifiableTextureData));
        }

        private static bool Lock(Material material, MaterialProperty[] props, bool applyShaderLater = false)
        {
            // File filepaths and names
            Shader shader = material.shader;
            string shaderFilePath = AssetDatabase.GetAssetPath(shader);
            string materialFilePath = AssetDatabase.GetAssetPath(material);
            string materialFolder = Path.GetDirectoryName(materialFilePath);
            string guid = AssetDatabase.AssetPathToGUID(materialFilePath);
            string newShaderName = "Hidden/Locked/" + shader.name + "/" + guid;
            string shaderOptimizerButtonDrawerName = $"[{nameof(ThryShaderOptimizerLockButtonDrawer).Replace("Drawer", "")}]";
            //string newShaderDirectory = materialFolder + "/OptimizedShaders/" + material.name + "-" + smallguid + "/";
            string newShaderDirectory = materialFolder + "/OptimizedShaders/" + material.name + "/";

            // suffix for animated properties when renaming is enabled
            string animPropertySuffix = GetRenamedPropertySuffix(material);

            // Get collection of all properties to replace
            // Simultaneously build a string of #defines for each CGPROGRAM
            List<(string name,string value)> defines = new List<(string,string)>();
            // Append all keywords active on the material
            foreach (string keyword in material.shaderKeywords)
            {
                if (keyword == "") continue; // idk why but null keywords exist if _ keyword is used and not removed by the editor at some point
                defines.Add((keyword,""));
            }

            KeywordsUsedByPragmas.Clear();

            List<PropertyData> constantProps = new List<PropertyData>();
            List<RenamingProperty> animatedPropsToRename = new List<RenamingProperty>();
            List<RenamingProperty> animatedPropsToDuplicate = new List<RenamingProperty>();
            foreach (MaterialProperty prop in props)
            {
                if (prop == null) continue;
                // Every property gets turned into a preprocessor variable
                switch (prop.type)
                {
                    case MaterialProperty.PropType.Texture:
                        if (prop.textureValue != null)
                        {
                            defines.Add(($"PROP{prop.name.ToUpperInvariant()}", ""));
                        }
                        break;
                }

                if (prop.name.EndsWith(AnimatedPropertySuffix, StringComparison.Ordinal)) continue;
                else if (prop.name == UseInlineSamplerStatesPropertyName)
                {
                    UseInlineSamplerStates = (prop.floatValue == 1);
                    continue;
                }
                else if (prop.name.StartsWith(GeometryShaderEnabledPropertyName, StringComparison.Ordinal))
                {
                    if (prop.name == GeometryShaderEnabledPropertyName)
                        UseGeometry = (prop.floatValue == 1);
                    else if (prop.name == GeometryShaderEnabledPropertyName + "ForwardBase")
                        UseGeometryForwardBase = (prop.floatValue == 1);
                    else if (prop.name == GeometryShaderEnabledPropertyName + "ForwardAdd")
                        UseGeometryForwardAdd = (prop.floatValue == 1);
                    else if (prop.name == GeometryShaderEnabledPropertyName + "ShadowCaster")
                        UseGeometryShadowCaster = (prop.floatValue == 1);
                    else if (prop.name == GeometryShaderEnabledPropertyName + "Meta")
                        UseGeometryMeta = (prop.floatValue == 1);
                }
                else if (prop.name.StartsWith(TessellationEnabledPropertyName, StringComparison.Ordinal))
                {
                    if (prop.name == TessellationEnabledPropertyName)
                        UseTessellation = (prop.floatValue == 1);
                    else if (prop.name == TessellationEnabledPropertyName + "ForwardBase")
                        UseTessellationForwardBase = (prop.floatValue == 1);
                    else if (prop.name == TessellationEnabledPropertyName + "ForwardAdd")
                        UseTessellationForwardAdd = (prop.floatValue == 1);
                    else if (prop.name == TessellationEnabledPropertyName + "ShadowCaster")
                        UseTessellationShadowCaster = (prop.floatValue == 1);
                    else if (prop.name == TessellationEnabledPropertyName + "Meta")
                        UseTessellationMeta = (prop.floatValue == 1);
                }

                string animateTag = material.GetTag(prop.name + AnimatedTagSuffix, false, "");
                if(string.IsNullOrEmpty(animateTag) == false)
                {
                    // check if we're renaming the property as well
                    if (animateTag == "2")
                    {
                        if (!prop.name.EndsWith("UV", StringComparison.Ordinal) && !prop.name.EndsWith("Pan", StringComparison.Ordinal)) // this property might be animated, but we're not allowed to rename it. this will break things.
                        {
                            if (IllegalPropertyRenames.Contains(prop.name))
                                animatedPropsToDuplicate.Add(new RenamingProperty(prop, prop.name, prop.name + "_" + animPropertySuffix));
                            else
                                animatedPropsToRename.Add(new RenamingProperty(prop, prop.name, prop.name + "_" + animPropertySuffix));
                            if (prop.type == MaterialProperty.PropType.Texture)
                            {
                                animatedPropsToRename.Add(new RenamingProperty(prop, prop.name + "_ST", prop.name + "_" + animPropertySuffix + "_ST"));
                                animatedPropsToRename.Add(new RenamingProperty(prop, prop.name + "_TexelSize", prop.name + "_" + animPropertySuffix + "_TexelSize"));
                            }
                        }
                    }

                    continue;
                }

                if (IsPropertyExcemptFromLocking(prop)) continue;

                PropertyData propData;
                switch(prop.type)
                {
                    case MaterialProperty.PropType.Color:
                        propData = new PropertyData();
                        propData.type = PropertyType.Vector;
                        propData.name = prop.name;
                        if ((prop.flags & MaterialProperty.PropFlags.HDR) != 0)
                        {
                            if ((prop.flags & MaterialProperty.PropFlags.Gamma) != 0)
                                propData.value = prop.colorValue.linear;
                            else propData.value = prop.colorValue;
                        }
                        else if ((prop.flags & MaterialProperty.PropFlags.Gamma) != 0)
                            propData.value = prop.colorValue;
                        else propData.value = prop.colorValue.linear;
                        if (PlayerSettings.colorSpace == ColorSpace.Gamma) propData.value = prop.colorValue;
                        constantProps.Add(propData);
                        break;
                    case MaterialProperty.PropType.Vector:
                        propData = new PropertyData();
                        propData.type = PropertyType.Vector;
                        propData.name = prop.name;
                        propData.value = prop.vectorValue;
                        constantProps.Add(propData);
                        break;
                    case MaterialProperty.PropType.Float:
                    case MaterialProperty.PropType.Range:
                        propData = new PropertyData();
                        propData.type = PropertyType.Float;
                        propData.name = prop.name;
                        propData.value = new Vector4(prop.floatValue, 0, 0, 0);
                        constantProps.Add(propData);
                        break;
                    case MaterialProperty.PropType.Texture:
                        PropertyData ST = new PropertyData();
                        ST.type = PropertyType.Vector;
                        ST.name = prop.name + "_ST";
                        Vector2 offset = material.GetTextureOffset(prop.name);
                        Vector2 scale = material.GetTextureScale(prop.name);
                        ST.value = new Vector4(scale.x, scale.y, offset.x, offset.y);
                        constantProps.Add(ST);

                        PropertyData TexelSize = new PropertyData();
                        TexelSize.type = PropertyType.Vector;
                        TexelSize.name = prop.name + "_TexelSize";
                        Texture t = prop.textureValue;
                        if (t != null)
                            TexelSize.value = new Vector4(1.0f / t.width, 1.0f / t.height, t.width, t.height);
                        else TexelSize.value = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                        constantProps.Add(TexelSize);
                        break;
                }
            }

            // Get list of lightmode passes to delete
            List<string> disabledLightModes = new List<string>();
            var disabledLightModesProperty = Array.Find(props, x => x.name == DisabledLightModesPropertyName);
            if (disabledLightModesProperty != null)
            {
                int lightModesMask = (int)disabledLightModesProperty.floatValue;
                if ((lightModesMask & (int)LightMode.ForwardAdd) != 0)
                    disabledLightModes.Add("ForwardAdd");
                if ((lightModesMask & (int)LightMode.ShadowCaster) != 0)
                    disabledLightModes.Add("ShadowCaster");
            }
                
            // Parse shader and cginc files, also gets preprocessor macros
            List<ParsedShaderFile> shaderFiles = new List<ParsedShaderFile>();
            List<Macro> macros = new List<Macro>();
            if (!ParseShaderFilesRecursive(shaderFiles, newShaderDirectory, shaderFilePath, macros, material))
                return false;

            // Remove all defines where name if not in shader files
            List<(string,string)> definesToRemove = new List<(string,string)>();
            foreach((string name,string) def in defines)
            {
                if (shaderFiles.All(x => x.lines.Any(l => l.Contains(def.name)) == false))
                    definesToRemove.Add(def);
            }
            defines.RemoveAll(x => definesToRemove.Contains(x));
            // Append convention OPTIMIZER_ENABLED keyword
            defines.Add((OptimizerEnabledKeyword,""));
            string optimizerDefines = "";
            if(defines.Count > 0)
                optimizerDefines = defines.Select(m => $"\r\n #define {m.name} {m.value}").Aggregate((s1, s2) => s1 + s2);

            int commentKeywords = 0;

            Dictionary<string,PropertyData> constantPropsDictionary = constantProps.GroupBy(x => x.name).Select(g => g.First()).ToDictionary(x => x.name);
            Macro[] macrosArray = macros.ToArray();

            List<GrabPassReplacement> grabPassVariables = new List<GrabPassReplacement>();
            // Loop back through and do macros, props, and all other things line by line as to save string ops
            // Will still be a massive n2 operation from each line * each property
            foreach (ParsedShaderFile psf in shaderFiles)
            {
                // replace property names when prop is animated
                for (int i = 0; i < psf.lines.Length; i++)
                {
                    foreach (var animProp in animatedPropsToRename)
                    {
                        // don't have to match if that prop does not even exist in that line
                        if (psf.lines[i].Contains(animProp.Keyword))
                        {
                            string pattern = animProp.Keyword + @"(?!(\w|\d))";
                            psf.lines[i] = Regex.Replace(psf.lines[i], pattern, animProp.Replace, RegexOptions.Multiline);
                        }
                    }
                    foreach (var animProp in animatedPropsToDuplicate)
                    {
                        if (psf.lines[i].Contains(animProp.Keyword))
                        {
                            //if Line is property definition duplicate it
                            bool isDefinition = Regex.Match(psf.lines[i], animProp.Keyword + @"\s*\(""[^""]+""\s*,\s*\w+\)\s*=").Success;
                            string og = null;
                            if (isDefinition)
                                og = psf.lines[i];

                            string pattern = animProp.Keyword + @"(?!(\w|\d))";
                            psf.lines[i] = Regex.Replace(psf.lines[i], pattern, animProp.Replace, RegexOptions.Multiline);

                            if (isDefinition)
                                psf.lines[i] = og + "\r\n" + psf.lines[i];
                        }
                    }
                }
                

                // Shader file specific stuff
                if (psf.filePath.EndsWith(".shader", StringComparison.Ordinal))
                {
                    for (int i=0; i<psf.lines.Length;i++)
                    {
                        string trimmedLine = psf.lines[i].TrimStart();

                        if (trimmedLine.StartsWith("Shader", StringComparison.Ordinal))
                        {
                            string originalSgaderName = psf.lines[i].Split('\"')[1];
                            psf.lines[i] = psf.lines[i].Replace(originalSgaderName, newShaderName);
                        }else if (trimmedLine.StartsWith(shaderOptimizerButtonDrawerName))
                        {
                            psf.lines[i] = Regex.Replace(psf.lines[i], @"\d+\w*$", "1");
                        }
                        else if (trimmedLine.StartsWith("//#pragmamulti_compile_LOD_FADE_CROSSFADE", StringComparison.Ordinal))
                        {
                            MaterialProperty crossfadeProp = Array.Find(props, x => x.name == LODCrossFadePropertyName);
                            if (crossfadeProp != null && crossfadeProp.floatValue == 1)
                                psf.lines[i] = psf.lines[i].Replace("//#pragma", "#pragma");
                        }
                        else if (trimmedLine.StartsWith("//\"IgnoreProjector\"=\"True\"", StringComparison.Ordinal))
                        {
                            MaterialProperty projProp = Array.Find(props, x => x.name == IgnoreProjectorPropertyName);
                            if (projProp != null && projProp.floatValue == 1)
                                psf.lines[i] = psf.lines[i].Replace("//\"IgnoreProjector", "\"IgnoreProjector");
                        }
                        else if (trimmedLine.StartsWith("//\"ForceNoShadowCasting\"=\"True\"", StringComparison.Ordinal))
                        {
                            MaterialProperty forceNoShadowsProp = Array.Find(props, x => x.name == ForceNoShadowCastingPropertyName);
                            if (forceNoShadowsProp != null && forceNoShadowsProp.floatValue == 1)
                                psf.lines[i] = psf.lines[i].Replace("//\"ForceNoShadowCasting", "\"ForceNoShadowCasting");
                        }
                        else if (trimmedLine.StartsWith("GrabPass {", StringComparison.Ordinal))
                        {
                            GrabPassReplacement gpr = new GrabPassReplacement();
                            string[] splitLine = trimmedLine.Split('\"');
                            if (splitLine.Length == 1)
                                gpr.originalName = "_GrabTexture";
                            else
                                gpr.originalName = splitLine[1];
                            gpr.newName = material.GetTag("GrabPass" + grabPassVariables.Count, false, "_GrabTexture");
                            psf.lines[i] = "GrabPass { \"" + gpr.newName + "\" }";
                            grabPassVariables.Add(gpr);
                        }
                        else if (trimmedLine.StartsWith("CGINCLUDE", StringComparison.Ordinal))
                        {
                            for (int j=i+1; j<psf.lines.Length;j++)
                                if (psf.lines[j].TrimStart().StartsWith("ENDCG", StringComparison.Ordinal))
                                {
                                    ReplaceShaderValues(material, psf.lines, i+1, j, props, constantPropsDictionary, macrosArray, grabPassVariables.ToArray());
                                    break;
                                }
                        }
                        else if (trimmedLine.StartsWith("CGPROGRAM", StringComparison.Ordinal))
                        {
                            if(commentKeywords == 0)
                                psf.lines[i] += optimizerDefines;
                            for (int j=i+1; j<psf.lines.Length;j++)
                                if (psf.lines[j].TrimStart().StartsWith("ENDCG", StringComparison.Ordinal))
                                {
                                    ReplaceShaderValues(material, psf.lines, i+1, j, props, constantPropsDictionary, macrosArray, grabPassVariables.ToArray());
                                    break;
                                }
                        }
                        // Lightmode based pass removal, requires strict formatting
                        else if (trimmedLine.StartsWith("Tags", StringComparison.Ordinal))
                        {
                            string lineFullyTrimmed = trimmedLine.Replace(" ", "").Replace("\t", "");
                            // expects lightmode tag to be on the same line like: Tags { "LightMode" = "ForwardAdd" }
                            if (lineFullyTrimmed.Contains("\"LightMode\"=\""))
                            {
                                string lightModeName = lineFullyTrimmed.Split('\"')[3];
                                // Store current lightmode name in a static, useful for per-pass geometry and tessellation removal
                                if (lightModeName == "ForwardBase") CurrentLightmode = LightModeType.ForwardBase;
                                else if (lightModeName == "ForwardAdd") CurrentLightmode = LightModeType.ForwardAdd;
                                else if (lightModeName == "ShadowCaster") CurrentLightmode = LightModeType.ShadowCaster;
                                else if (lightModeName == "Meta") CurrentLightmode = LightModeType.Meta;
                                else CurrentLightmode = LightModeType.None;
                                if (disabledLightModes.Contains(lightModeName))
                                {
                                    // Loop up from psf.lines[i] until standalone "Pass" line is found, delete it
                                    int j=i-1;
                                    for (;j>=0;j--)
                                        if (psf.lines[j].Replace(" ", "").Replace("\t", "") == "Pass")
                                            break;
                                    // then delete each line until a standalone ENDCG line is found
                                    for (;j<psf.lines.Length;j++)
                                    {
                                        if (psf.lines[j].Replace(" ", "").Replace("\t", "") == "ENDCG")
                                            break;
                                        psf.lines[j] = "";
                                    }
                                    // then delete each line until a standalone '}' line is found
                                    for (;j<psf.lines.Length;j++)
                                    {
                                        string temp = psf.lines[j];
                                        psf.lines[j] = "";
                                        if (temp.Replace(" ", "").Replace("\t", "") == "}")
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
                else // CGINC file
                    ReplaceShaderValues(material, psf.lines, 0, psf.lines.Length, props, constantPropsDictionary, macrosArray, grabPassVariables.ToArray());

                // Recombine file lines into a single string
                int totalLen = psf.lines.Length*2; // extra space for newline chars
                foreach (string line in psf.lines)
                    totalLen += line.Length;
                StringBuilder sb = new StringBuilder(totalLen);
                // This appendLine function is incompatible with the '\n's that are being added elsewhere
                foreach (string line in psf.lines)
                    sb.AppendLine(line);
                string output = sb.ToString();

                //cull shader file path
                string fileName = Path.GetFileName(psf.filePath);
                // Write output to file
                (new FileInfo(newShaderDirectory + fileName)).Directory.Create();
                try
                {
                    StreamWriter sw = new StreamWriter(newShaderDirectory + fileName);
                    sw.Write(output);
                    sw.Close();
                    AssetDatabase.ImportAsset(newShaderDirectory + fileName);
                }
                catch (IOException e)
                {
                    Debug.LogError("[Shader Optimizer] Processed shader file " + newShaderDirectory + fileName + " could not be written.  " + e.ToString());
                    return false;
                }
            }
            
            AssetDatabase.Refresh();

            ApplyStruct applyStruct = new ApplyStruct();
            applyStruct.material = material;
            applyStruct.shader = shader;
            applyStruct.smallguid = guid;
            applyStruct.newShaderName = newShaderName;
            applyStruct.animatedPropsToRename = animatedPropsToRename;
            applyStruct.animatedPropsToDuplicate = animatedPropsToDuplicate;
            applyStruct.animPropertySuffix = animPropertySuffix;

            if (applyShaderLater)
            {
                //Debug.Log("Apply later: "+applyStructsLater.Count+ ", "+material.name);
                applyStructsLater[material] = applyStruct;
                return true;
            }
            return LockApplyShader(applyStruct);
        }

        private static Dictionary<Material, ApplyStruct> applyStructsLater = new Dictionary<Material, ApplyStruct>();

        private struct ApplyStruct
        {
            public Material material;
            public Shader shader;
            public string smallguid;
            public string newShaderName;
            public List<RenamingProperty> animatedPropsToRename;
            public List<RenamingProperty> animatedPropsToDuplicate;
            public string animPropertySuffix;
            public bool shared;
        }

        private static bool LockApplyShader(Material material)
        {
            if (applyStructsLater.ContainsKey(material) == false) return false;
            ApplyStruct applyStruct = applyStructsLater[material];
            if (applyStruct.shared)
            {
                material.shader = applyStruct.material.shader;
                return true;
            }
            //applyStructsLater.Remove(material);
            return LockApplyShader(applyStruct);
        }

        public static void ApplyMaterialPropertyDrawersPatch(Material material) {}
        static MethodInfo ApplyMaterialPropertyDrawersOriginalMethodInfo = typeof(MaterialEditor).GetMethod("ApplyMaterialPropertyDrawers", new Type[] {typeof(Material)});
        static MethodInfo ApplyMaterialPropertyDrawersPatchMethodInfo = typeof(ShaderOptimizer).GetMethod(nameof(ApplyMaterialPropertyDrawersPatch), BindingFlags.Public | BindingFlags.Static);

        public static void DetourApplyMaterialPropertyDrawers()
        {
            Helper.TryDetourFromTo(ApplyMaterialPropertyDrawersOriginalMethodInfo, ApplyMaterialPropertyDrawersPatchMethodInfo);
        }

        public static void RestoreApplyMaterialPropertyDrawers()
        {
            Helper.RestoreDetour(ApplyMaterialPropertyDrawersOriginalMethodInfo);
        }

        private static bool LockApplyShader(ApplyStruct applyStruct)
        {
            Material material = applyStruct.material;
            Shader shader = applyStruct.shader;
            string newShaderName = applyStruct.newShaderName;
            List<RenamingProperty> animatedPropsToRename = applyStruct.animatedPropsToRename;
            List<RenamingProperty> animatedPropsToDuplicate = applyStruct.animatedPropsToDuplicate;
            string animPropertySuffix = applyStruct.animPropertySuffix;

            // Write original shader to override tag
            material.SetOverrideTag(TAG_ORIGINAL_SHADER, shader.name);
            // Write the new shader folder name in an override tag so it will be deleted

            // For some reason when shaders are swapped on a material the RenderType override tag gets completely deleted and render queue set back to -1
            // So these are saved as temp values and reassigned after switching shaders
            string renderType = material.GetTag("RenderType", false, "");
            int renderQueue = material.renderQueue;

            // Actually switch the shader
            Shader newShader = Shader.Find(newShaderName);
            if (newShader == null)
            {
                Debug.LogError("[Shader Optimizer] Generated shader " + newShaderName + " could not be found");
                return false;
            }
            DetourApplyMaterialPropertyDrawers();
            material.shader = newShader;
            RestoreApplyMaterialPropertyDrawers();
            //ShaderEditor.reload();
            material.SetOverrideTag("RenderType", renderType);
            material.renderQueue = renderQueue;

            material.SetOverrideTag("OriginalKeywords", string.Join(" ", material.shaderKeywords));
            // Remove ALL keywords
            foreach (string keyword in material.shaderKeywords)
                if(material.IsKeywordEnabled(keyword)) material.DisableKeyword(keyword);

            foreach (var animProp in animatedPropsToRename)
            {
                var newName = animProp.Prop.name + "_" + animPropertySuffix;
                switch (animProp.Prop.type)
                {
                    case MaterialProperty.PropType.Color:
                        material.SetColor(newName, animProp.Prop.colorValue);
                        break;
                    case MaterialProperty.PropType.Vector:
                        material.SetVector(newName, animProp.Prop.vectorValue);
                        break;
                    case MaterialProperty.PropType.Float:
                        material.SetFloat(newName, animProp.Prop.floatValue);
                        break;
                    case MaterialProperty.PropType.Range:
                        material.SetFloat(newName, animProp.Prop.floatValue);
                        break;
                    case MaterialProperty.PropType.Texture:
                        material.SetTexture(newName, animProp.Prop.textureValue);
                        material.SetTextureScale(newName, new Vector2(animProp.Prop.textureScaleAndOffset.x, animProp.Prop.textureScaleAndOffset.y));
                        material.SetTextureOffset(newName, new Vector2(animProp.Prop.textureScaleAndOffset.z, animProp.Prop.textureScaleAndOffset.w));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(material), "This property type should not be renamed and can not be set.");
                }
            }

            foreach (var animProp in animatedPropsToDuplicate)
            {
                var newName = animProp.Prop.name + "_" + animPropertySuffix;
                switch (animProp.Prop.type)
                {
                    case MaterialProperty.PropType.Color:
                        material.SetColor(newName, animProp.Prop.colorValue);
                        break;
                    case MaterialProperty.PropType.Vector:
                        material.SetVector(newName, animProp.Prop.vectorValue);
                        break;
                    case MaterialProperty.PropType.Float:
                        material.SetFloat(newName, animProp.Prop.floatValue);
                        break;
                    case MaterialProperty.PropType.Range:
                        material.SetFloat(newName, animProp.Prop.floatValue);
                        break;
                    case MaterialProperty.PropType.Texture:
                        material.SetTexture(newName, animProp.Prop.textureValue);
                        material.SetTextureScale(newName, new Vector2(animProp.Prop.textureScaleAndOffset.x, animProp.Prop.textureScaleAndOffset.y));
                        material.SetTextureOffset(newName, new Vector2(animProp.Prop.textureScaleAndOffset.z, animProp.Prop.textureScaleAndOffset.w));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(material), "This property type should not be renamed and can not be set.");
                }
            }
            return true;
        }

        /** <summary>Find longest common directoy</summary> */
        public static int GetLongestCommonDirectoryLength(string[] s)
        {
            int k = s[0].Length;
            for (int i = 1; i < s.Length; i++)
            {
                k = Math.Min(k, s[i].Length);
                for (int j = 0; j < k; j++)
                    if ( AreCharsInPathEqual(s[i][j] , s[0][j]) == false)
                    {
                        k = j;
                        break;
                    }
            }
            string p = s[0].Substring(0, k);
            if (Directory.Exists(p)) return p.Length;
            else return Path.GetDirectoryName(p).Length;
        }

        private static bool AreCharsInPathEqual(char c1, char c2)
        {
            return (c1 == c2) || ((c1 == '/' || c1 == '\\') && (c2 == '/' || c2 == '\\'));
        }

        // Preprocess each file for macros and includes
        // Save each file as string[], parse each macro with //KSOEvaluateMacro
        // Only editing done is replacing #include "X" filepaths where necessary
        // most of these args could be private static members of the class
        private static bool ParseShaderFilesRecursive(List<ParsedShaderFile> filesParsed, string newTopLevelDirectory, string filePath, List<Macro> macros, Material material)
        {
            // Infinite recursion check
            if (filesParsed.Exists(x => x.filePath == filePath)) return true;

            ParsedShaderFile psf = new ParsedShaderFile();
            psf.filePath = filePath;
            filesParsed.Add(psf);

            // Read file
            string fileContents = null;
            try
            {
                StreamReader sr = new StreamReader(filePath);
                fileContents = sr.ReadToEnd();
                sr.Close();
            }
            catch (FileNotFoundException e)
            {
                Debug.LogError("[Shader Optimizer] Shader file " + filePath + " not found.  " + e.ToString());
                return false;
            }
            catch (IOException e)
            {
                Debug.LogError("[Shader Optimizer] Error reading shader file.  " + e.ToString());
                return false;
            }

            // Parse file line by line
            List<String> macrosList = new List<string>();
            string[] fileLines = fileContents.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            List<string> includedLines = new List<string>();

            bool isIncluded = true;
            int isNotIncludedAtDepth = 0;
            int ifStacking = 0;
            Stack<bool> removeEndifStack = new Stack<bool>();

            bool isCommentedOut = false;

            int currentExcludeDepth = 0;
            bool doExclude = false;
            int excludeStartDepth = 0;

            for (int i=0; i<fileLines.Length; i++)
            {
                string lineParsed = fileLines[i].TrimStart();

                if(lineParsed.StartsWith("//", StringComparison.Ordinal))
                {
                    //Exclusion logic
                    if(lineParsed.StartsWith("//ifex", StringComparison.Ordinal))
                    {
                        if(!doExclude){ // If already excluding, only track depth
                            var condition = DefineableCondition.Parse(lineParsed.Substring(6), material);
                            if(condition.Test())
                            {
                                doExclude = true;
                                excludeStartDepth = currentExcludeDepth;
                            }
                        }
                        currentExcludeDepth++;
                    }
                    else if(lineParsed.StartsWith("//endex", StringComparison.Ordinal))
                    {
                        if(currentExcludeDepth == 0)
                        {
                            Debug.LogError("[Shader Optimizer] Number of 'endex' statements does not match number of 'ifex' statements."
                                +$"\nError found in file '{filePath}' line {i+1}");
                        }
                        else
                        {
                            currentExcludeDepth--;
                            if(currentExcludeDepth == excludeStartDepth)
                            {
                                doExclude = false;
                            }
                        }
                    }
                    // Specifically requires no whitespace between // and KSOEvaluateMacro
                    else if (lineParsed == "//KSOEvaluateMacro")
                    {
                        string macro = "";
                        string lineTrimmed = null;
                        do
                        {
                            i++;
                            lineTrimmed = fileLines[i].TrimEnd();
                            if (lineTrimmed.EndsWith("\\", StringComparison.Ordinal))
                                macro += lineTrimmed.TrimEnd('\\') + Environment.NewLine; // keep new lines in macro to make output more readable
                            else macro += lineTrimmed;
                        } 
                        while (lineTrimmed.EndsWith("\\", StringComparison.Ordinal));
                        macrosList.Add(macro);
                    }
                    continue;
                }
                if (doExclude) continue;

                //removes empty lines
                if (string.IsNullOrEmpty(lineParsed)) continue;
                //removes code that is commented
                if (lineParsed== "*/")
                {
                    isCommentedOut = false;
                    continue;
                }
                else if (lineParsed == "/*")
                {
                    isCommentedOut = true;
                    continue;
                }
                if (isCommentedOut) continue;

                //Removed code from defines blocks
                if (REMOVE_UNUSED_IF_DEFS)
                {
                    //Check if Line contains #ifs
                    if (lineParsed.StartsWith("#if", StringComparison.Ordinal))
                    {
                        bool hasMultiple = lineParsed.Contains('&') || lineParsed.Contains('|');
                        if (!hasMultiple && lineParsed.StartsWith("#ifdef", StringComparison.Ordinal))
                        {
                            string keyword = lineParsed.Substring(6).Trim().Split(' ')[0];
                            bool allowRemoveal = (DontRemoveIfBranchesKeywords.Contains(keyword) == false) && KeywordsUsedByPragmas.Contains(keyword);
                            bool isRemoved = false;
                            if (isIncluded && allowRemoveal)
                            {
                                if ((material.IsKeywordEnabled(keyword) == false))
                                {
                                    isIncluded = false;
                                    isNotIncludedAtDepth = ifStacking;
                                    isRemoved = true;
                                }
                            }
                            ifStacking++;
                            removeEndifStack.Push(isRemoved);
                            if (isRemoved) continue;
                        }
                        else if (!hasMultiple && lineParsed.StartsWith("#ifndef", StringComparison.Ordinal))
                        {
                            string keyword = lineParsed.Substring(7).Trim().Split(' ')[0];
                            bool allowRemoveal = DontRemoveIfBranchesKeywords.Contains(keyword) == false && KeywordsUsedByPragmas.Contains(keyword);
                            bool isRemoved = false;
                            if (isIncluded && allowRemoveal)
                            {
                                if (material.IsKeywordEnabled(keyword) == true)
                                {
                                    isIncluded = false;
                                    isNotIncludedAtDepth = ifStacking;
                                    isRemoved = true;
                                }
                            }
                            ifStacking++;
                            removeEndifStack.Push(isRemoved);
                            if (isRemoved) continue;
                        }
                        else
                        {
                            ifStacking++;
                            removeEndifStack.Push(false);
                        }
                    }
                    else if (lineParsed.StartsWith("#else"))
                    {
                        if(removeEndifStack.Count == 0)
                        {
                            Debug.LogError("[Shader Optimizer] Number of 'endif' statements does not match number of 'if' statements."
                                +$"\nError found in file '{filePath}' line {i+1}. Current output copied to clipboard.");
                            GUIUtility.systemCopyBuffer = string.Join(Environment.NewLine, includedLines);
                        }
                        if (isIncluded && removeEndifStack.Peek()) isIncluded = false;
                        if (!isIncluded && ifStacking - 1 == isNotIncludedAtDepth) isIncluded = true;
                        if (removeEndifStack.Peek()) continue;
                    }
                    else if (lineParsed.StartsWith("#endif", StringComparison.Ordinal))
                    {
                        ifStacking--;
                        if (ifStacking == isNotIncludedAtDepth)
                        {
                            isIncluded = true;
                        }
                        // for debugging
                        if(removeEndifStack.Count == 0)
                        {
                            Debug.LogError("[Shader Optimizer] Number of 'endif' statements does not match number of 'if' statements."
                                +$"\nError found in file '{filePath}' line {i+1}. Current output copied to clipboard.");
                            GUIUtility.systemCopyBuffer = string.Join(Environment.NewLine, includedLines);
                        }
                        if (removeEndifStack.Pop()) continue;
                    }
                    if (!isIncluded) continue;
                }

                //Remove pragmas
                if (lineParsed.StartsWith("#pragma shader_feature", StringComparison.Ordinal))
                {
                    string trimmed = lineParsed.Replace("#pragma shader_feature_local", "").Replace("#pragma shader_feature", "").TrimStart();
                    
                    string[] keywords = trimmed.Split(' ');
                    foreach (string keyword in keywords)
                    {
                        string kw = keyword.Trim();
                        if (KeywordsUsedByPragmas.Contains(kw) == false) KeywordsUsedByPragmas.Add(kw);
                    }
                    continue;
                }

                // Specifically requires no whitespace between # and include, as it should be
                if (lineParsed.StartsWith("#include", StringComparison.Ordinal))
                {
                    int firstQuotation = lineParsed.IndexOf('\"',0);
                    int lastQuotation = lineParsed.IndexOf('\"',firstQuotation+1);
                    string includeFilename = lineParsed.Substring(firstQuotation+1, lastQuotation-firstQuotation-1);

                    // Skip default includes
                    if (DefaultUnityShaderIncludes.Contains(includeFilename) == false)
                    {
                        string includeFullpath = includeFilename;
                        if (includeFilename.StartsWith("Assets/", StringComparison.Ordinal) == false)//not absolute
                        {
                            includeFullpath = GetFullPath(includeFilename, Path.GetDirectoryName(filePath));
                        }
                        if (!ParseShaderFilesRecursive(filesParsed, newTopLevelDirectory, includeFullpath, macros, material))
                            return false;
                        //Change include to be be ralative to only one directory up, because all files are moved into the same folder
                        fileLines[i] = fileLines[i].Replace(includeFilename, "/"+includeFilename.Split('/').Last());
                    }
                }

                includedLines.Add(fileLines[i]);
            }

            // Prepare the macros list into pattern matchable structs
            // Revise this later to not do so many string ops
            foreach (string macroString in macrosList)
            {
                string m = macroString.TrimStart();
                Macro macro = new Macro();

                if (!m.StartsWith("#define", StringComparison.Ordinal)) continue;
                m = m.Remove(0, "#define".Length).TrimStart();
                
                string allArgs = "";
                if (m.Contains('('))
                {
                    macro.name = m.Split('(')[0];
                    m = m.Remove(0, macro.name.Length + "(".Length);
                    allArgs = m.Split(')')[0];
                    allArgs = allArgs.Trim().Replace(" ","").Replace("\t","");
                    macro.args = allArgs.Split(',');
                    m = m.Remove(0, allArgs.Length + ")".Length).TrimStart();
                    macro.contents = m;
                }
                else continue;
                macros.Add(macro);
            }

            // Save psf lines to list
            psf.lines = includedLines.ToArray();
            return true;
        }

        // error CS1501: No overload for method 'Path.GetFullPath' takes 2 arguments
        // Thanks Unity
        // Could be made more efficent with stringbuilder
        public static string GetFullPath(string relativePath, string basePath)
        {
            while (relativePath.StartsWith("./"))
                relativePath = relativePath.Remove(0, "./".Length);
            while (relativePath.StartsWith("../"))
            {
                basePath = basePath.Remove(basePath.LastIndexOf(Path.DirectorySeparatorChar), basePath.Length - basePath.LastIndexOf(Path.DirectorySeparatorChar));
                relativePath = relativePath.Remove(0, "../".Length);
            }
            return basePath + '/' + relativePath;
        }
 
        // Replace properties! The meat of the shader optimization process
        // For each constantProp, pattern match and find each instance of the property that isn't a declaration
        // most of these args could be private static members of the class
        private static void ReplaceShaderValues(Material material, string[] lines, int startLine, int endLine, 
        MaterialProperty[] props, Dictionary<string,PropertyData> constants, Macro[] macros, GrabPassReplacement[] grabPassVariables)
        {
            List <TextureProperty> uniqueSampledTextures = new List<TextureProperty>();

            // Outside loop is each line
            for (int i=startLine;i<endLine;i++)
            {
                string lineTrimmed = lines[i].TrimStart();
                // tokenize line
                string[] tokens = lineTrimmed.Split(new char[]{' ', '\t', '(', ')', '[', ']', '+', '-', '*', '/', '.', ',', ';', '=', '!'}, StringSplitOptions.RemoveEmptyEntries);
            
                if (lineTrimmed.StartsWith("#pragma geometry", StringComparison.Ordinal))
                {
                    if (!UseGeometry)
                        lines[i] = "//" + lines[i];
                    else
                    {
                        switch (CurrentLightmode)
                        {
                            case LightModeType.ForwardBase:
                                if (!UseGeometryForwardBase)
                                    lines[i] = "//" + lines[i];
                                break;
                            case LightModeType.ForwardAdd:
                                if (!UseGeometryForwardAdd)
                                    lines[i] = "//" + lines[i];
                                break;
                            case LightModeType.ShadowCaster:
                                if (!UseGeometryShadowCaster)
                                    lines[i] = "//" + lines[i];
                                break;
                            case LightModeType.Meta:
                                if (!UseGeometryMeta)
                                    lines[i] = "//" + lines[i];
                                break;
                        }
                    }
                }
                else if (lineTrimmed.StartsWith("#pragma hull", StringComparison.Ordinal) || lineTrimmed.StartsWith("#pragma domain", StringComparison.Ordinal))
                {
                    if (!UseTessellation)
                        lines[i] = "//" + lines[i];
                    else
                    {
                        switch (CurrentLightmode)
                        {
                            case LightModeType.ForwardBase:
                                if (!UseTessellationForwardBase)
                                    lines[i] = "//" + lines[i];
                                break;
                            case LightModeType.ForwardAdd:
                                if (!UseTessellationForwardAdd)
                                    lines[i] = "//" + lines[i];
                                break;
                            case LightModeType.ShadowCaster:
                                if (!UseTessellationShadowCaster)
                                    lines[i] = "//" + lines[i];
                                break;
                            case LightModeType.Meta:
                                if (!UseTessellationMeta)
                                    lines[i] = "//" + lines[i];
                                break;
                        }
                    }
                }
                // Replace inline smapler states
                else if (UseInlineSamplerStates && lineTrimmed.StartsWith("//KSOInlineSamplerState", StringComparison.Ordinal))
                {
                    string lineParsed = lineTrimmed.Replace(" ","").Replace("\t","");
                    // Remove all whitespace
                    int firstParenthesis = lineParsed.IndexOf('(');
                    int lastParenthesis = lineParsed.IndexOf(')');
                    string argsString = lineParsed.Substring(firstParenthesis+1, lastParenthesis - firstParenthesis-1);
                    string[] args = argsString.Split(',');
                    MaterialProperty texProp = Array.Find(props, x => x.name == args[1]);
                    if (texProp != null)
                    {
                        Texture t = texProp.textureValue;
                        int inlineSamplerIndex = 0;
                        if (t != null)
                        {
                            switch (t.filterMode)
                            {
                                case FilterMode.Bilinear:
                                    break;
                                case FilterMode.Point:
                                    inlineSamplerIndex += 1 * 4;
                                    break;
                                case FilterMode.Trilinear:
                                    inlineSamplerIndex += 2 * 4;
                                    break;
                            }
                            switch (t.wrapMode)
                            {
                                case TextureWrapMode.Repeat:
                                    break;
                                case TextureWrapMode.Clamp:
                                    inlineSamplerIndex += 1;
                                    break;
                                case TextureWrapMode.Mirror:
                                    inlineSamplerIndex += 2;
                                    break;
                                case TextureWrapMode.MirrorOnce:
                                    inlineSamplerIndex += 3;
                                    break;
                            }
                        }

                        // Replace the token on the following line
                        lines[i+1] = lines[i+1].Replace(args[0], InlineSamplerStateNames[inlineSamplerIndex]);
                    }
                }
                else if (lineTrimmed.StartsWith("//KSODuplicateTextureCheckStart", StringComparison.Ordinal))
                {
                    // Since files are not fully parsed and instead loosely processed, each shader function needs to have
                    // its sampled texture list reset somewhere before KSODuplicateTextureChecks are made.
                    // As long as textures are sampled in-order inside a single function, this method will work.
                    uniqueSampledTextures = new List<TextureProperty>();
                }
                else if (lineTrimmed.StartsWith("//KSODuplicateTextureCheck", StringComparison.Ordinal))
                {
                    // Each KSODuplicateTextureCheck line gets evaluated when the shader is optimized
                    // If the texture given has already been sampled as another texture (i.e. one texture is used in two slots)
                    // AND has been sampled with the same UV mode - as indicated by a convention UV property,
                    // AND has been sampled with the exact same Tiling/Offset values
                    // AND has been logged by KSODuplicateTextureCheck, 
                    // then the variable corresponding to the first instance of that texture being 
                    // sampled will be assigned to the variable corresponding to the given texture.
                    // The compiler will then skip the duplicate texture sample since its variable is overwritten before being used
                    
                    // Parse line for argument texture property name
                    string lineParsed = lineTrimmed.Replace(" ", "").Replace("\t", "");
                    int firstParenthesis = lineParsed.IndexOf('(');
                    int lastParenthesis = lineParsed.IndexOf(')');
                    string argName = lineParsed.Substring(firstParenthesis+1, lastParenthesis-firstParenthesis-1);
                    // Check if texture property by argument name exists and has a texture assigned
                    if (Array.Exists(props, x => x.name == argName))
                    {
                        MaterialProperty argProp = Array.Find(props, x => x.name == argName);
                        if (argProp.textureValue != null)
                        {
                            // If no convention UV property exists, sampled UV mode is assumed to be 0 
                            // Any UV enum or mode indicator can be used for this
                            int UV = 0;
                            if (Array.Exists(props, x => x.name == argName + "UV"))
                                UV = (int)(Array.Find(props, x => x.name == argName + "UV").floatValue);

                            Vector2 texScale = material.GetTextureScale(argName);
                            Vector2 texOffset = material.GetTextureOffset(argName);

                            // Check if this texture has already been sampled
                            if (uniqueSampledTextures.Exists(x => (x.texture == argProp.textureValue) 
                                                               && (x.uv == UV)
                                                               && (x.scale == texScale)
                                                               && x.offset == texOffset))
                            {
                                string texName = uniqueSampledTextures.Find(x => (x.texture == argProp.textureValue) && (x.uv == UV)).name;
                                // convention _var variables requried. i.e. _MainTex_var and _CoverageMap_var
                                lines[i] = argName + "_var = " + texName + "_var;";
                            }
                            else
                            {
                                // Texture/UV/ST combo hasn't been sampled yet, add it to the list
                                TextureProperty tp = new TextureProperty();
                                tp.name = argName;
                                tp.texture = argProp.textureValue;
                                tp.uv = UV;
                                tp.scale = texScale;
                                tp.offset = texOffset;
                                uniqueSampledTextures.Add(tp);
                            }
                        }
                    }
                }
                else if (lineTrimmed.StartsWith("[maxtessfactor(", StringComparison.Ordinal))
                {
                    MaterialProperty maxTessFactorProperty = Array.Find(props, x => x.name == TessellationMaxFactorPropertyName);
                    if (maxTessFactorProperty != null)
                    {
                        float maxTessellation = maxTessFactorProperty.floatValue;
                        string animateTag = material.GetTag(TessellationMaxFactorPropertyName + AnimatedTagSuffix, false, "0");
                        if (animateTag != "" && animateTag == "1")
                            maxTessellation = 64.0f;
                        lines[i] = "[maxtessfactor(" + maxTessellation.ToString(".0######") + ")]";
                    }
                }

                // then replace macros
                foreach (Macro macro in macros)
                {
                    // Expects only one instance of a macro per line!
                    int macroIndex;
                    if ((macroIndex = lines[i].IndexOf(macro.name + "(", StringComparison.Ordinal)) != -1)
                    {
                        // Macro exists on this line, make sure its not the definition
                        string lineParsed = lineTrimmed.Replace(" ","").Replace("\t","");
                        if (lineParsed.StartsWith("#define", StringComparison.Ordinal)) continue;

                        // parse args between first '(' and first ')'
                        int firstParenthesis = macroIndex + macro.name.Length;
                        int lastParenthesis = lines[i].IndexOf(')', macroIndex + macro.name.Length+1);
                        string allArgs = lines[i].Substring(firstParenthesis+1, lastParenthesis-firstParenthesis-1);
                        string[] args = allArgs.Split(',');
                        
                        // Replace macro parts
                        string newContents = macro.contents;
                        for (int j=0; j<args.Length;j++)
                        {
                            args[j] = args[j].Trim();
                            int argIndex;
                            int lastIndex = 0;
                            while ((argIndex = newContents.IndexOf(macro.args[j], lastIndex, StringComparison.Ordinal)) != -1)
                            {
                                lastIndex = argIndex+1;
                                char charLeft = ' ';
                                if (argIndex-1 >= 0)
                                    charLeft = newContents[argIndex-1];
                                char charRight = ' ';
                                if (argIndex+macro.args[j].Length < newContents.Length)
                                    charRight = newContents[argIndex+macro.args[j].Length];
                                if (ValidSeparators.Contains(charLeft) && ValidSeparators.Contains(charRight))
                                {
                                    // Replcae the arg!
                                    StringBuilder sbm = new StringBuilder(newContents.Length - macro.args[j].Length + args[j].Length);
                                    sbm.Append(newContents, 0, argIndex);
                                    sbm.Append(args[j]);
                                    sbm.Append(newContents, argIndex + macro.args[j].Length, newContents.Length - argIndex - macro.args[j].Length);
                                    newContents = sbm.ToString();
                                }
                            }
                        }
                        newContents = newContents.Replace("##", ""); // Remove token pasting separators
                        // Replace the line with the evaluated macro
                        StringBuilder sb = new StringBuilder(lines[i].Length + newContents.Length);
                        sb.Append(lines[i], 0, macroIndex);
                        sb.Append(newContents);
                        sb.Append(lines[i], lastParenthesis+1, lines[i].Length - lastParenthesis-1);
                        lines[i] = sb.ToString();
                    }
                }

                foreach(string token in tokens)
                {
                    if(constants.ContainsKey(token))
                    {
                        PropertyData constant = constants[token];

                        int constantIndex;
                        int lastIndex = 0;
                        bool declarationFound = false;
                        while ((constantIndex = lines[i].IndexOf(constant.name, lastIndex, StringComparison.Ordinal)) != -1)
                        {
                            lastIndex = constantIndex+1;
                            char charLeft = ' ';
                            if (constantIndex-1 >= 0)
                                charLeft = lines[i][constantIndex-1];
                            char charRight = ' ';
                            if (constantIndex + constant.name.Length < lines[i].Length)
                                charRight = lines[i][constantIndex + constant.name.Length];
                            // Skip invalid matches (probably a subname of another symbol)
                            if (!(ValidSeparators.Contains(charLeft) && ValidSeparators.Contains(charRight)))
                                continue;
                            // Skip inline comments
                            if (charLeft == '*' && charRight == '*' && constantIndex >= 2 && lines[i][constantIndex - 2] == '/')
                                continue;
                            
                            // Skip basic declarations of unity shader properties i.e. "uniform float4 _Color;"
                            if (!declarationFound)
                            {
                                string precedingText = lines[i].Substring(0, constantIndex-1).TrimEnd(); // whitespace removed string immediately to the left should be float or float4
                                string restOftheFile = lines[i].Substring(constantIndex + constant.name.Length).TrimStart(); // whitespace removed character immediately to the right should be ;
                                if (Array.Exists(ValidPropertyDataTypes, x => precedingText.EndsWith(x, StringComparison.Ordinal)) && restOftheFile.StartsWith(";", StringComparison.Ordinal))
                                {
                                    constant.lastDeclarationType = precedingText.TrimStart();
                                    declarationFound = true;
                                    continue;
                                }
                            }

                            // Replace with constant!
                            // This could technically be more efficient by being outside the IndexOf loop
                            StringBuilder sb = new StringBuilder(lines[i].Length * 2);
                            sb.Append(lines[i], 0, constantIndex);
                            switch (constant.type)
                            {
                                case PropertyType.Float:
                                    string constantValue;
                                    // Special Handling for ints 
                                    if(constant.lastDeclarationType == "int")
                                        constantValue = constant.value.x.ToString("F0", CultureInfo.InvariantCulture);
                                    else
                                        constantValue = constant.value.x.ToString("0.0####################", CultureInfo.InvariantCulture);

                                    // Add comment with property name, for easier debug
                                    sb.Append($"({constantValue} /*{constant.name}*/)");
                                    break;
                                case PropertyType.Vector:
                                    sb.Append("float4("+constant.value.x.ToString(CultureInfo.InvariantCulture)+","
                                                    +constant.value.y.ToString(CultureInfo.InvariantCulture)+","
                                                    +constant.value.z.ToString(CultureInfo.InvariantCulture)+","
                                                    +constant.value.w.ToString(CultureInfo.InvariantCulture)+")");
                                    break;
                            }
                            sb.Append(lines[i], constantIndex+constant.name.Length, lines[i].Length-constantIndex-constant.name.Length);
                            lines[i] = sb.ToString();

                            // Check for Unity branches on previous line here?
                        }
                    }
                }

                // Then replace grabpass variable names
                foreach (GrabPassReplacement gpr in grabPassVariables)
                {
                    // find indexes of all instances of gpr.originalName that exist on this line
                    int lastIndex = 0;
                    int gbIndex;
                    while ((gbIndex = lines[i].IndexOf(gpr.originalName, lastIndex, StringComparison.Ordinal)) != -1)
                    {
                        lastIndex = gbIndex+1;
                        char charLeft = ' ';
                        if (gbIndex-1 >= 0)
                            charLeft = lines[i][gbIndex-1];
                        char charRight = ' ';
                        if (gbIndex + gpr.originalName.Length < lines[i].Length)
                            charRight = lines[i][gbIndex + gpr.originalName.Length];
                        // Skip invalid matches (probably a subname of another symbol)
                        if (!(ValidSeparators.Contains(charLeft) && ValidSeparators.Contains(charRight)))
                            continue;
                        
                        // Replace with new variable name
                        // This could technically be more efficient by being outside the IndexOf loop
                        StringBuilder sb = new StringBuilder(lines[i].Length * 2);
                        sb.Append(lines[i], 0, gbIndex);
                        sb.Append(gpr.newName);
                        sb.Append(lines[i], gbIndex+gpr.originalName.Length, lines[i].Length-gbIndex-gpr.originalName.Length);
                        lines[i] = sb.ToString();
                    }
                }

                // Then remove Unity branches
                if (RemoveUnityBranches)
                    lines[i] = lines[i].Replace("UNITY_BRANCH", "").Replace("[branch]", "");
            }
        }

        public enum UnlockSuccess { hasNoSavedShader, wasNotLocked, couldNotFindOriginalShader, couldNotDeleteLockedShader,
            success}
        private static void Unlock(Material material, MaterialProperty shaderOptimizer = null)
        {
            //if unlock success set floats. not done for locking cause the sucess is checked later when applying the shaders
            UnlockSuccess success = ShaderOptimizer.UnlockConcrete(material);
            if (success == UnlockSuccess.success || success == UnlockSuccess.wasNotLocked
                || success == UnlockSuccess.couldNotDeleteLockedShader)
            {
                if (shaderOptimizer != null) shaderOptimizer.floatValue = 0;
                else material.SetFloat(GetOptimizerPropertyName(material.shader), 0);
            }
        }
        public static bool GuessShader(Shader locked, out Shader shader)
        {
            string name = locked.name;
            name = Regex.Match(name.Substring(7), @".*(?=\/)").Value;
            ShaderInfo[] allShaders = ShaderUtil.GetAllShaderInfo();
            int closestDistance = int.MaxValue;
            string closestShaderName = null;
            foreach (ShaderInfo s in allShaders)
            {
                if (!s.supported) continue;
                int d = Helper.LevenshteinDistance(s.name, name);
                if(d < closestDistance)
                {
                    closestDistance = d;
                    closestShaderName = s.name;
                }
            }
            shader = Shader.Find(closestShaderName);
            return shader != null && closestDistance < name.Length / 2;
        }
        private static UnlockSuccess UnlockConcrete (Material material)
        {
            Shader lockedShader = material.shader;
            // Revert to original shader
            string originalShaderName = material.GetTag(TAG_ORIGINAL_SHADER, false, "");
            Shader orignalShader = null;
            if (originalShaderName == "" && !GuessShader(lockedShader, out orignalShader))
            {
                if (material.shader.name.StartsWith("Hidden/"))
                {
                    if (EditorUtility.DisplayDialog("Unlock Material", $"The original shader for {material.name} could not be resolved.\nPlease select a shader manually.", "Ok")) { }
                    Debug.LogError("[Shader Optimizer] Original shader not saved to material, could not unlock shader");
                    return UnlockSuccess.hasNoSavedShader;
                }
                else
                {
                    Debug.LogWarning("[Shader Optimizer] Original shader not saved to material, but material also doesnt seem to be locked.");
                    return UnlockSuccess.wasNotLocked;
                }

            }
            if(orignalShader == null) orignalShader = Shader.Find(originalShaderName);
            if (orignalShader == null && !GuessShader(lockedShader, out orignalShader))
            {
                if (material.shader.name.StartsWith("Hidden/"))
                {
                    if (EditorUtility.DisplayDialog("Unlock Material", $"The original shader for {material.name} could not be resolved.\nPlease select a shader manually.", "Ok")) { }
                    Debug.LogError("[Shader Optimizer] Original shader " + originalShaderName + " could not be found");
                    return UnlockSuccess.couldNotFindOriginalShader;
                }
                else
                {
                    Debug.LogWarning("[Shader Optimizer] Original shader not saved to material, but material also doesnt seem to be locked.");
                    return UnlockSuccess.wasNotLocked;
                }
            }

            // For some reason when shaders are swapped on a material the RenderType override tag gets completely deleted and render queue set back to -1
            // So these are saved as temp values and reassigned after switching shaders
            string renderType = material.GetTag("RenderType", false, "");
            int renderQueue = material.renderQueue;
            string unlockedMaterialGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(material));
            DetourApplyMaterialPropertyDrawers();
            material.shader = orignalShader;
            RestoreApplyMaterialPropertyDrawers();
            material.SetOverrideTag("RenderType", renderType);
            material.renderQueue = renderQueue;
            material.shaderKeywords = material.GetTag("OriginalKeywords", false, string.Join(" ", material.shaderKeywords)).Split(' ');

            // Delete the variants folder and all files in it, as to not orhpan files and inflate Unity project
            // But only if no other material is using the locked shader
            string[] lockedMaterials = material.GetTag(TAG_ALL_MATERIALS_GUIDS_USING_THIS_LOCKED_SHADER, false, "").Split(',');
            string newTag = string.Join(",", lockedMaterials.Where(guid => guid != unlockedMaterialGUID).ToArray());
            bool isOtherMaterialUsingLockedShader = false;
            foreach(string guid in lockedMaterials)
            {
                if (string.IsNullOrWhiteSpace(guid)) continue;
                Material m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if (m != null)
                {
                    isOtherMaterialUsingLockedShader |= m.shader == lockedShader;
                    m.SetOverrideTag(TAG_ALL_MATERIALS_GUIDS_USING_THIS_LOCKED_SHADER, newTag);
                }
            }
            if (!isOtherMaterialUsingLockedShader)
            {
                string materialFilePath = AssetDatabase.GetAssetPath(lockedShader);
                string lockedFolder = Path.GetDirectoryName(materialFilePath);
                FileUtil.DeleteFileOrDirectory(lockedFolder);
                FileUtil.DeleteFileOrDirectory(lockedFolder + ".meta");
            }
            //AssetDatabase.Refresh();

            return UnlockSuccess.success;
        }

        public static void DeleteTags(Material[] materials)
        {
            foreach(Material m in materials)
            {
                var it = new SerializedObject(m).GetIterator();
                while (it.Next(true))
                {
                    if (it.name == "stringTagMap")
                    {
                        for (int i = 0; i < it.arraySize; i++)
                        {
                            string tagName = it.GetArrayElementAtIndex(i).displayName;
                            if (tagName.EndsWith(AnimatedTagSuffix))
                            {
                                m.SetOverrideTag(tagName, "");
                            }
                        }
                    }
                }
            }
        }

        #region Upgrade

        public static void UpgradeAnimatedPropertiesToTagsOnAllMaterials()
        {
            IEnumerable<Material> materials = Resources.FindObjectsOfTypeAll<Material>();
            UpgradeAnimatedPropertiesToTags(materials);
            Debug.Log("[Thry][Optimizer] Update animated properties of all materials to tags.");
        }

        public static void UpgradeAnimatedPropertiesToTags(IEnumerable<Material> iMaterials)
        {
            IEnumerable<Material> materialsToChange = iMaterials.Where(m => m != null &&
                string.IsNullOrEmpty(AssetDatabase.GetAssetPath(m)) == false && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(m.shader)) == false
                && IsShaderUsingThryOptimizer(m.shader)).Distinct().OrderBy(m => m.shader.name);

            int i = 0;
            foreach (Material m in materialsToChange)
            {
                if(EditorUtility.DisplayCancelableProgressBar("Upgrading Materials", "Upgrading animated tags of " + m.name, (float)i / materialsToChange.Count()))
                {
                    break;
                }

                string path = AssetDatabase.GetAssetPath(m);
                StreamReader reader = new StreamReader(path);
                string line;
                while((line = reader.ReadLine()) != null)
                {
                    if (line.Contains(AnimatedPropertySuffix) && line.Length > 6)
                    {
                        string[] parts = line.Substring(6, line.Length - 6).Split(':');
                        float f;
                        if (float.TryParse(parts[1], out f))
                        {
                            if( f != 0)
                            {
                                string name = parts[0].Substring(0, parts[0].Length - AnimatedPropertySuffix.Length);
                                m.SetOverrideTag(name + AnimatedTagSuffix, "" + f);
                            }
                        }
                    }
                }
                reader.Close();
                i++;
            }

            EditorUtility.ClearProgressBar();
        }

        static void ClearConsole()
        {
            var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");

            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            clearMethod.Invoke(null, null);
        }

        #endregion

        // ifex indenting
        [MenuItem("Assets/Thry/Shaders/Ifex Indenting", false, 303)]
        static void IfExIndenting()
        {
            Shader s = Selection.objects[0] as Shader;
            if (s == null) return;
            string path = AssetDatabase.GetAssetPath(s);
            if(string.IsNullOrEmpty(path)) return;
            // Load the shader file
            string[] lines = File.ReadAllLines(path);
            int indent = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Contains("//endex")) indent = Mathf.Max(0,indent - 1);
                lines[i] = new string(' ', indent * 4) + line;
                if (line.StartsWith("//ifex")) indent++;
            }
            GUIUtility.systemCopyBuffer = string.Join("\n", lines);
        }

        [MenuItem("Assets/Thry/Shaders/Ifex Indenting", true)]
        static bool IfExIndentingValidator()
        {
            return Selection.objects.Length == 1 && Selection.objects[0] is Shader;
        }

        //---GameObject + Children Locking

        [MenuItem("GameObject/Thry/Materials/Unlock All", false,0)]
        static void UnlockAllChildren()
        {
            SetLockForAllChildren(Selection.gameObjects, 0, true);
        }

        [MenuItem("GameObject/Thry/Materials/Lock All", false,0)]
        static void LockAllChildren()
        {
            SetLockForAllChildren(Selection.gameObjects, 1, true);
        }

        //---Asset Unlocking

        [MenuItem("Assets/Thry/Materials/Unlock All", false, 303)]
        static void UnlockAllMaterials()
        {
            IEnumerable<Material> mats = Selection.assetGUIDs.Select(g => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g)));
            SetLockedForAllMaterials(mats, 0, true);
        }

        [MenuItem("Assets/Thry/Materials/Unlock All", true)]
        static bool UnlockAllMaterialsValidator()
        {
            return SelectedObjectsAreLockableMaterials();
        }

        //---Asset Locking

        [MenuItem("Assets/Thry/Materials/Lock All", false, 303)]
        static void LockAllMaterials()
        {
            IEnumerable<Material> mats = Selection.assetGUIDs.Select(g => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g)));
            SetLockedForAllMaterials(mats, 1, true);
        }

        [MenuItem("Assets/Thry/Materials/Lock All", true)]
        static bool LockAllMaterialsValidator()
        {
            return SelectedObjectsAreLockableMaterials();
        }

        //----Folder Lock

        //This does not work for folders on the left side of the project explorer, because they are not exposed to Selection
        [MenuItem("Assets/Thry/Materials/Lock Folder", false, 303)]
        static void LockFolder()
        {
            IEnumerable<string> folderPaths = Selection.objects.Select(o => AssetDatabase.GetAssetPath(o)).Where(p => Directory.Exists(p));
            List<Material> materials = new List<Material>();
            foreach (string f in folderPaths) FindMaterialsRecursive(f, materials);
            SetLockedForAllMaterials(materials, 1, true);
        }

        [MenuItem("Assets/Thry/Materials/Lock Folder", true)]
        static bool LockFolderValidator()
        {
            return Selection.objects.Select(o => AssetDatabase.GetAssetPath(o)).Where(p => Directory.Exists(p)).Count() == Selection.objects.Length;
        }

        //-----Folder Unlock

        [MenuItem("Assets/Thry/Materials/Unlock Folder", false, 303)]
        static void UnLockFolder()
        {
            IEnumerable<string> folderPaths = Selection.objects.Select(o => AssetDatabase.GetAssetPath(o)).Where(p => Directory.Exists(p));
            List<Material> materials = new List<Material>();
            foreach (string f in folderPaths) FindMaterialsRecursive(f, materials);
            SetLockedForAllMaterials(materials, 0, true);
        }

        [MenuItem("Assets/Thry/Materials/Unlock Folder", true)]
        static bool UnLockFolderValidator()
        {
            return Selection.objects.Select(o => AssetDatabase.GetAssetPath(o)).Where(p => Directory.Exists(p)).Count() == Selection.objects.Length;
        }

        private static void FindMaterialsRecursive(string folderPath, List<Material> materials)
        {
            foreach(string f in Directory.GetFiles(folderPath))
            {
                if(AssetDatabase.GetMainAssetTypeAtPath(f) == typeof(Material))
                {
                    materials.Add(AssetDatabase.LoadAssetAtPath<Material>(f));
                }
            }
            foreach(string f in Directory.GetDirectories(folderPath)){
                FindMaterialsRecursive(f, materials);
            }
        }

        //----Folder Unlock

        static bool SelectedObjectsAreLockableMaterials()
        {
            if (Selection.assetGUIDs != null && Selection.assetGUIDs.Length > 0)
            {
                return Selection.assetGUIDs.All(g =>
                {
                    if (AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GUIDToAssetPath(g)) != typeof(Material))
                        return false;
                    Material m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g));
                    return IsShaderUsingThryOptimizer(m.shader);
                });
            }
            return false;
        }

        //----VRChat Callback to force Locking on upload

#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
        public class LockMaterialsOnUpload : IVRCSDKPreprocessAvatarCallback
        {
            public int callbackOrder => 100;

            public bool OnPreprocessAvatar(GameObject avatarGameObject)
            {
                List<Material> materials = avatarGameObject.GetComponentsInChildren<Renderer>(true).SelectMany(r => r.sharedMaterials).ToList();
#if VRC_SDK_VRCSDK3  && !UDON
                VRCAvatarDescriptor descriptor = avatarGameObject.GetComponent<VRCAvatarDescriptor>();
                if(descriptor != null)
                {
                    IEnumerable<AnimationClip> clips = descriptor.baseAnimationLayers.Select(l => l.animatorController).Where(a => a != null).SelectMany(a => a.animationClips).Distinct();
                    foreach (AnimationClip clip in clips)
                    {
                        IEnumerable<Material> clipMaterials = AnimationUtility.GetObjectReferenceCurveBindings(clip).Where(b => b.isPPtrCurve && b.type.IsSubclassOf(typeof(Renderer)) && b.propertyName.StartsWith("m_Materials"))
                            .SelectMany(b => AnimationUtility.GetObjectReferenceCurve(clip, b)).Select(r => r.value as Material);
                        materials.AddRange(clipMaterials);
                    }
                }
                
#endif
                if(SetLockedForAllMaterials(materials, 1, showProgressbar: true, showDialog: PersistentData.Get<bool>("ShowLockInDialog", true), allowCancel: false) == false)
                    return false;
                //returning true all the time, because build process cant be stopped it seems
                return true;
            }
        }
#endif

#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
        public class LockMaterialsOnWorldUpload : IVRCSDKBuildRequestedCallback
        {
            public int callbackOrder => 100;

            bool IVRCSDKBuildRequestedCallback.OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
            {
                List<Material> materials = new List<Material>();
                if (requestedBuildType == VRCSDKRequestedBuildType.Scene)
                {
                    if (UnityEngine.Object.FindObjectsOfType(typeof(VRC_SceneDescriptor)) is VRC_SceneDescriptor[] descriptors && descriptors.Length > 0){
                        var renderers = UnityEngine.Object.FindObjectsOfType<Renderer>();
                        foreach (var rend in renderers)
                        {
                            foreach (var mat in rend.sharedMaterials){
                                materials.Add(mat);
                            }
                        }
                    }
                    SetLockedForAllMaterials(materials, 1, showProgressbar: true, showDialog: PersistentData.Get<bool>("ShowLockInDialog", true), allowCancel: false);
                }
                return true;
            }
        }
#endif

        const string DidStripUnlockedShadersSessionStateKey = "ShaderOptimizerDidStripUnlockedShaders";
        public class StripUnlockedShadersFromBuild : UnityEditor.Build.IPreprocessShaders
        {
            // Thanks to z3y for this function
            public int callbackOrder => 4;

            public void OnProcessShader(Shader shader, UnityEditor.Rendering.ShaderSnippetData snippet, IList<UnityEditor.Rendering.ShaderCompilerData> data)
            {
                // Strip if the shader should be optimized (has the optimizer property) but isn't (name doesn't start with Hidden/Locked/)
                bool shouldStrip = shader.FindPropertyIndex("_ShaderOptimizerEnabled") >= 0 && !shader.name.StartsWith("Hidden/Locked/");

                if (shouldStrip)
                {
                    // Try to warn the user if there's an unlocked shader
                    if (!SessionState.GetBool(DidStripUnlockedShadersSessionStateKey, false))
                    {
                        EditorUtility.DisplayDialog("Shader Optimizer: Unlocked Shader", "An Unlocked shader was found, and will not be included in the build (this will cause pink materials).\nThis shouldn't happen. Make sure all lockable materials are Locked, and try again.\nIf it happens again, please report the issue via GitHub or Discord!", "OK");
                        SessionState.SetBool(DidStripUnlockedShadersSessionStateKey, true);
                    }

                    data.Clear();
                }
            }
        }

        [InitializeOnLoad]
        public static class ResetStrippedShaderWarning
        {
            static ResetStrippedShaderWarning()
            {
                EditorApplication.update -= ResetWarning;
                EditorApplication.update += ResetWarning;
            }

            private static void ResetWarning()
            {
                if(SessionState.GetBool(DidStripUnlockedShadersSessionStateKey, false))
                {
                    Debug.LogError($"[Shader Optimizer] Unlocked shaders were removed from build. Materials will be pink. Use Thry -> Lock All on hierarchy items to ensure materials are locked.");
                    SessionState.SetBool(DidStripUnlockedShadersSessionStateKey, false);
                }
            }
        }

        static string MaterialToShaderPropertyHash(Material m)
        {
            StringBuilder stringBuilder = new StringBuilder(m.shader.name);

            foreach (MaterialProperty prop in
                     MaterialEditor.GetMaterialProperties(new Object[] { m }))
            {
                string propName = prop.name;

                if (PropertiesToSkipInMaterialEquallityComparission.Contains(propName)) continue;

                string isAnimated = GetAnimatedTag(m, propName);

                if (isAnimated == "1")
                {
                    stringBuilder.Append(isAnimated);
                }
                else if(isAnimated == "2")
                {
                    //This is because materials with renaming should not share shaders
                    stringBuilder.Append(m.name);
                }
                else
                {

                    switch (prop.type)
                    {
                        case MaterialProperty.PropType.Color:
                            stringBuilder.Append(m.GetColor(propName).ToString());
                            break;
                        case MaterialProperty.PropType.Vector:
                            stringBuilder.Append(m.GetVector(propName).ToString());
                            break;
                        case MaterialProperty.PropType.Range:
                        case MaterialProperty.PropType.Float:
                            stringBuilder.Append(m.GetFloat(propName)
                                .ToString(CultureInfo.InvariantCulture));
                            break;
                        case MaterialProperty.PropType.Texture:
                            Texture t = m.GetTexture(propName);
                            Vector4 texelSize = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                            if (t != null)
                                texelSize = new Vector4(1.0f / t.width, 1.0f / t.height, t.width, t.height);

                            stringBuilder.Append(m.GetTextureOffset(propName).ToString());
                            stringBuilder.Append(m.GetTextureScale(propName).ToString());
                            break;
                    }
                }
            }

            // https://forum.unity.com/threads/hash-function-for-game.452779/
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] bytes = encoding.GetBytes(stringBuilder.ToString());
            var sha = new MD5CryptoServiceProvider();
            return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLower();
        }

        public static bool SetLockForAllChildren(GameObject[] objects, int lockState, bool showProgressbar = false, bool showDialog = false, bool allowCancel = true)
        {
            IEnumerable<Material> materials = objects.Select(o => o.GetComponentsInChildren<Renderer>(true)).SelectMany(rA => rA.SelectMany(r => r.sharedMaterials));
            return SetLockedForAllMaterials(materials, lockState, showProgressbar, showDialog);
        }

        static Dictionary<string, List<Material>> s_shaderPropertyCombinations = new Dictionary<string, List<Material>>();
        public static bool SetLockedForAllMaterials(IEnumerable<Material> materials, int lockState, bool showProgressbar = false, bool showDialog = false, bool allowCancel = true, MaterialProperty shaderOptimizer = null)
        {
            Helper.RegisterEditorUse();
            //first the shaders are created. compiling is suppressed with start asset editing
            AssetDatabase.StartAssetEditing();

            bool seekedIsLocked = lockState == 1;

            //Get cleaned materia list
            // The GetPropertyDefaultFloatValue is changed from 0 to 1 when the shader is locked in
            IEnumerable<Material> materialsToChangeLock = materials.Where(m => m != null 
                && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(m)) == false 
                && string.IsNullOrEmpty(AssetDatabase.GetAssetPath(m.shader)) == false
                && IsShaderUsingThryOptimizer(m.shader)
                &&  (   m.shader.name.StartsWith("Hidden/Locked/")
                    || (m.shader.name.StartsWith("Hidden/") && m.GetTag("OriginalShader",false,"") != "" 
                        && m.shader.GetPropertyDefaultFloatValue(m.shader.FindPropertyIndex(GetOptimizerPropertyName(m.shader))) == 1)
                    ) != seekedIsLocked)
                .Distinct();

            float i = 0;
            float length = materialsToChangeLock.Count();

            //show popup dialog if defined
            if (showDialog && length > 0)
            {
                if(EditorUtility.DisplayDialog("Locking Materials", EditorLocale.editor.Get("auto_lock_dialog").ReplaceVariables(length), "More information","OK"))
                {
                    Application.OpenURL("https://www.youtube.com/watch?v=asWeDJb5LAo");
                }
                PersistentData.Set("ShowLockInDialog", false);
            }

            //Create shader assets
            foreach (Material m in materialsToChangeLock.ToList()) //have to call ToList() here otherwise the Unlock Shader button in the ShaderGUI doesn't work
            {
                //do progress bar
                if (showProgressbar)
                {
                    if (allowCancel)
                    {
                        if (EditorUtility.DisplayCancelableProgressBar((lockState == 1) ? "Locking Materials" : "Unlocking Materials", m.name, i / length)) break;
                    }
                    else
                    {
                        EditorUtility.DisplayProgressBar((lockState == 1) ? "Locking Materials" : "Unlocking Materials", m.name, i / length);
                    }
                }
                //create the assets
                try
                {
                    if (lockState == 1)
                    {
                        string hash = MaterialToShaderPropertyHash(m);
                        // Check that shader has already been created for this hash and still exists
                        // Or that the shader is being created for this has during this session
                        Material reference = null;
                        if(s_shaderPropertyCombinations.ContainsKey(hash))
                        {

                            reference = s_shaderPropertyCombinations[hash].FirstOrDefault(m2 => m2 != m && (materialsToChangeLock.Contains(m2) || Shader.Find(applyStructsLater[m2].newShaderName) != null));
                        }
                        if (reference != null)
                        {
                            // Reuse existing shader and struct
                            ApplyStruct applyStruct = applyStructsLater[reference];
                            applyStruct.material = m;
                            applyStructsLater[m] = applyStruct;
                            //Disable shader keywords
                            foreach (string keyword in m.shaderKeywords)
                                if (m.IsKeywordEnabled(keyword)) m.DisableKeyword(keyword);

                        }
                        // Create new locked shader
                        else
                        {
                            ShaderOptimizer.Lock(m,
                                MaterialEditor.GetMaterialProperties(new UnityEngine.Object[] { m }),
                                applyShaderLater: true);
                            s_shaderPropertyCombinations[hash] = new List<Material>();
                        }
                        // Add material to list of materials with same shader property hash
                        s_shaderPropertyCombinations[hash].Add(m);
                        // Update TAG_ALL_MATERIALS_GUIDS_USING_THIS_LOCKED_SHADER of all materials with same shader property hash
                        string tag = string.Join(",", s_shaderPropertyCombinations[hash].Select(m2 => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(m2))));
                        foreach (Material m2 in s_shaderPropertyCombinations[hash])
                            m2.SetOverrideTag(TAG_ALL_MATERIALS_GUIDS_USING_THIS_LOCKED_SHADER, tag);
                    }
                    else if (lockState == 0)
                    {
                        ShaderOptimizer.Unlock(m, shaderOptimizer);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    string position = e.StackTrace.Split('\n').FirstOrDefault(l => l.Contains("ThryEditor"));
                    if(position != null)
                    {
                        position = position.Split(new string[]{ "ThryEditor" }, StringSplitOptions.None).LastOrDefault();
                        Debug.LogError("Could not un-/lock material " + m.name + " | Error thrown at " + position+ "\n"+e.StackTrace);
                    }else
                    {
                        Debug.LogError("Could not un-/lock material " + m.name + "\n"+e.StackTrace);
                    }
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.StopAssetEditing();
                    return false;
                }
                i++;
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.StopAssetEditing();
            //unity now compiles all the shaders

            //now all new shaders are applied. this has to happen after unity compiled the shaders
            if (lockState == 1)
            {
                AssetDatabase.Refresh();
                //Apply new shaders
                foreach (Material m in materialsToChangeLock)
                {
                    if (ShaderOptimizer.LockApplyShader(m))
                    {
                        m.SetFloat(GetOptimizerPropertyName(m.shader), 1);
                    }
                }
            }
            AssetDatabase.Refresh();
            if (ShaderEditor.Active != null && ShaderEditor.Active.IsDrawing)
            {
                GUIUtility.ExitGUI();
            }
            return true;
        }

        public static string GetOptimizerPropertyName(Shader shader)
        {
            if (isShaderUsingThryOptimizer.ContainsKey(shader))
            {
                if (isShaderUsingThryOptimizer[shader] == false) return null;
                return shaderThryOptimizerPropertyName[shader];
            }
            else
            {
                if (IsShaderUsingThryOptimizer(shader) == false) return null;
                return shaderThryOptimizerPropertyName[shader];
            }
        }

        private static Dictionary<Shader, string> shaderThryOptimizerPropertyName = new Dictionary<Shader, string>();
        private static Dictionary<Shader, bool> isShaderUsingThryOptimizer = new Dictionary<Shader, bool>();
        public static bool IsShaderUsingThryOptimizer(Shader shader)
        {
            if (isShaderUsingThryOptimizer.ContainsKey(shader))
            {
                return isShaderUsingThryOptimizer[shader];
            }
            SerializedObject shaderObject = new SerializedObject(shader);
            SerializedProperty props = shaderObject.FindProperty("m_ParsedForm.m_PropInfo.m_Props");
            if (props != null)
            {
                foreach (SerializedProperty p in props)
                {
                    SerializedProperty at = p.FindPropertyRelative("m_Attributes");
                    if (at.arraySize > 0)
                    {
                        if (at.GetArrayElementAtIndex(0).stringValue == "ThryShaderOptimizerLockButton")
                        {
                            //Debug.Log(shader.name + " found to use optimizer ");
                            isShaderUsingThryOptimizer[shader] = true;
                            shaderThryOptimizerPropertyName[shader] = p.displayName;
                            return true;
                        }
                    }
                }
            }
            isShaderUsingThryOptimizer[shader] = false;
            return false;
        }

        public static bool IsMaterialLocked(Material material)
        {
            return material.shader.name.StartsWith("Hidden/") && material.GetTag(TAG_ORIGINAL_SHADER, false, "") != "";
        }

        private static Dictionary<Shader, int> shaderUsedTextureReferencesCount = new Dictionary<Shader, int>();
        public static int GetUsedTextureReferencesCount(Shader s)
        {
            //Shader.m_ParsedForm.m_SubShaders[i].m_Passes[j].m_Programs[k].m_SubPrograms[l].m_Parameters[m].m_TextureParams[n]
            //m_Programs not avaiable in unity 2019
            return 0;
            /*if (shaderUsedTextureReferencesCount.ContainsKey(s)) return shaderUsedTextureReferencesCount[s];
            SerializedObject shaderObject = new SerializedObject(s);
            SerializedProperty m_SubShaders = shaderObject.FindProperty("m_ParsedForm.m_SubShaders");
            for (int i_subShader = 0; i_subShader < m_SubShaders.arraySize; i_subShader++)
            {
                SerializedProperty m_Passes = m_SubShaders.GetArrayElementAtIndex(i_subShader).FindPropertyRelative("m_Passes");
                for (int i_passes = 0; i_passes < m_Passes.arraySize; i_passes++)
                {
                    SerializedProperty m_Programs = m_Passes.GetArrayElementAtIndex(i_passes);
                    foreach (SerializedProperty p in m_Programs) Debug.Log(p.displayName);
                }
            }
            return 0;*/
        }
    }

    public class UnlockedMaterialsList : EditorWindow
    {
        private Vector2 scrollPosition = Vector2.zero;

        static Dictionary<Shader, List<Material>> unlockedMaterialsByShader = new Dictionary<Shader, List<Material>>();
        static Dictionary<Shader, List<Material>> lockedMaterialsByShader = new Dictionary<Shader, List<Material>>();
        static Dictionary<Material, Shader> lockedMaterialsByOriginalShader = new Dictionary<Material, Shader>();
        static Dictionary<String, bool> unlockedFoldouts = new Dictionary<String, bool>();
        static Dictionary<String, bool> lockedFoldouts = new Dictionary<String, bool>();
        string searchTerm = "";

        private void OnEnable()
        {
            UpdateList();
            scrollPosition = Vector2.zero;
        }

        bool LockAllWarning(List<Material> materialsToLock)
        {
            return EditorUtility.DisplayDialog("Lock All Materials", $"You're about to lock {materialsToLock.Count} materials. This might take a while. Are you sure you want to proceed?", "Lock All", "Cancel");
        }

        bool UnlockAllWarning(List<Material> materialsToUnlock)
        {
            return EditorUtility.DisplayDialog("Unlock All Materials", $"You're about to unlock {materialsToUnlock.Count} materials. This might cause crashes if over 64 textures are used in all materials on a single shader.\n\nAre you sure you want to proceed?", "Unlock All", "Cancel");
        }

        void UpdateList()
        {
            lockedMaterialsByOriginalShader.Clear();
            unlockedMaterialsByShader.Clear();
            lockedMaterialsByShader.Clear();
            string[] guids = AssetDatabase.FindAssets($"t:material {searchTerm}");
            float step = 1.0f / guids.Length;
            float f = 0;
            EditorUtility.DisplayProgressBar("Searching materials...", "", f);

            List<Material> unlockedMaterials = new List<Material>();
            List<Material> lockedMaterials = new List<Material>();

            foreach (string g in guids)
            {
                Material m = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g));
                if (m != null && m.shader != null && ShaderOptimizer.IsShaderUsingThryOptimizer(m.shader))
                {
                    if(ShaderOptimizer.IsMaterialLocked(m))
                        lockedMaterials.Add(m);
                    else
                        unlockedMaterials.Add(m);
                }
                f = f + step;
                EditorUtility.DisplayProgressBar("Searching materials...", m.name, f);
            }
            foreach (IGrouping<Shader, Material> materials in unlockedMaterials.GroupBy(m => m.shader))
            {
                unlockedMaterialsByShader.Add(materials.Key, materials.ToList());
                if(!unlockedFoldouts.ContainsKey(materials.Key.name))
                    unlockedFoldouts.Add(materials.Key.name, false);
            }

            foreach (Material material in lockedMaterials)
            {
                string originalShaderName = material.GetTag(ShaderOptimizer.TAG_ORIGINAL_SHADER, false, "");
                Shader originalShader = null;
                if (originalShaderName == "" && originalShader == null) ShaderOptimizer.GuessShader(material.shader, out originalShader);
                if (originalShader == null) originalShader = Shader.Find(originalShaderName);

                if (originalShader != null)
                {
                    if (!lockedMaterialsByShader.ContainsKey(originalShader))
                        lockedMaterialsByShader[originalShader] = new List<Material>();
                    lockedMaterialsByShader[originalShader].Add(material);

                    if (!lockedFoldouts.ContainsKey(originalShader.name))
                        lockedFoldouts.Add(originalShader.name, false);
                }
            }
            EditorUtility.ClearProgressBar();
        }

        private void OnGUI()
        {
            List<Material> materialsToLock = new List<Material>();
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            searchTerm = EditorGUILayout.DelayedTextField(searchTerm);
            if (GUILayout.Button("Update/Search") || EditorGUI.EndChangeCheck()) 
                UpdateList();
            EditorGUILayout.EndHorizontal();
            int unlockedMaterials = unlockedMaterialsByShader.Values.SelectMany(col => col).ToList().Count;
            int lockedMaterials = lockedMaterialsByShader.Values.SelectMany(col => col).ToList().Count;

            EditorGUILayout.Space(10, true);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.LabelField($"Unlocked Materials ({unlockedMaterials})", Styles.EDITOR_LABEL_HEADER);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Lock All")) 
            {
                List<Material> allUnlockedMaterials = unlockedMaterialsByShader.Values.SelectMany(col => col).ToList();
                if(LockAllWarning(allUnlockedMaterials))
                    materialsToLock = allUnlockedMaterials;
            }
            if (GUILayout.Button("Expand All")) unlockedFoldouts.Keys.ToList().ForEach(f => unlockedFoldouts[f] = true);
            if (GUILayout.Button("Collapse All")) unlockedFoldouts.Keys.ToList().ForEach(f => unlockedFoldouts[f] = false);

            EditorGUILayout.EndHorizontal();

            if (unlockedMaterialsByShader.Count == 0)
                GUILayout.Label("No Locked materials found for search term.", Styles.greenStyle);

            foreach (KeyValuePair<Shader, List<Material>> shaderMaterials in unlockedMaterialsByShader)
            {
                EditorGUILayout.Space();

                unlockedFoldouts[shaderMaterials.Key.name] = EditorGUILayout.BeginFoldoutHeaderGroup(unlockedFoldouts[shaderMaterials.Key.name], $"{shaderMaterials.Key.name} ({shaderMaterials.Value.Count.ToString()})");
                if (unlockedFoldouts[shaderMaterials.Key.name])
                {
                    if (GUILayout.Button("Lock All")) 
                    {
                        if(LockAllWarning(shaderMaterials.Value))
                            materialsToLock = shaderMaterials.Value;
                    }

                    foreach (Material m in shaderMaterials.Value)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(m, typeof(Material), false);
                        //EditorGUILayout.IntField(ShaderOptimizer.GetUsedTextureReferencesCount(m.shader));
                        if (GUILayout.Button("Lock"))
                            materialsToLock.Add(m);

                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            if (materialsToLock.Count > 0)
            {
                ShaderOptimizer.SetLockedForAllMaterials(materialsToLock, 1, true, false, true);
                materialsToLock.Clear();
                UpdateList();
            }

            EditorGUILayout.Space(10, true);

            EditorGUILayout.LabelField($"Locked Materials ({lockedMaterials})", Styles.EDITOR_LABEL_HEADER);
            List<Material> materialsToUnlock = new List<Material>();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Unlock All"))
            {
                List<Material> allLockedMaterials = lockedMaterialsByShader.Values.SelectMany(col => col).ToList();
                if (UnlockAllWarning(allLockedMaterials))
                {
                    materialsToUnlock = allLockedMaterials;
                }
            }
            if (GUILayout.Button("Expand All")) lockedFoldouts.Keys.ToList().ForEach(f => lockedFoldouts[f] = true);
            if (GUILayout.Button("Collapse All")) lockedFoldouts.Keys.ToList().ForEach(f => lockedFoldouts[f] = false);

            EditorGUILayout.EndHorizontal();

            if (lockedMaterialsByShader.Count == 0)
                GUILayout.Label("No Unlocked materials found for search term.", Styles.greenStyle);

            foreach (KeyValuePair<Shader, List<Material>> shaderMaterials in lockedMaterialsByShader)
            {
                EditorGUILayout.Space();

                lockedFoldouts[shaderMaterials.Key.name] = EditorGUILayout.BeginFoldoutHeaderGroup(lockedFoldouts[shaderMaterials.Key.name], $"{shaderMaterials.Key.name} ({shaderMaterials.Value.Count.ToString()})");
                if (lockedFoldouts[shaderMaterials.Key.name])
                {
                    if (GUILayout.Button("Unlock All")) 
                    {
                        if(UnlockAllWarning(shaderMaterials.Value))
                            materialsToUnlock = shaderMaterials.Value;
                    }

                    foreach (Material m in shaderMaterials.Value)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.ObjectField(m, typeof(Material), false);
                        //EditorGUILayout.IntField(ShaderOptimizer.GetUsedTextureReferencesCount(m.shader));
                        if (GUILayout.Button("Unlock"))
                            materialsToUnlock.Add(m);

                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            if (materialsToUnlock.Count > 0)
            {
                ShaderOptimizer.SetLockedForAllMaterials(materialsToUnlock, 0, true, false, true);
                materialsToUnlock.Clear();
                UpdateList();
            }
            EditorGUILayout.EndScrollView();
        }
    }
}
