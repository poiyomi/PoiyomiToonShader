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

//#define DEBUG_IF_DEF_REMOVAL

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
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
using Thry.ThryEditor.Helpers;
using Thry.ThryEditor.Drawers;
using JetBrains.Annotations;

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

namespace Thry.ThryEditor
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
        public const string TAG_ORIGINAL_SHADER_GUID = "OriginalShaderGUID";
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

        enum LightModeType { None, ForwardBase, ForwardAdd, ShadowCaster, Meta, DepthOnly, DepthNormals };
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
            "UnityStandardUtils.cginc",
            "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl",
            "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl",
            "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl",
            "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RealtimeLights.hlsl",
            "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl",
            "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl",
            "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ProbeVolumeVariants.hlsl",
            "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl",
            "Packages/com.unity.render-pipelines.core/ShaderLibrary/MetaPass.hlsl",
            "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MotionVectorsCommon.hlsl"
        };
        
        public static readonly HashSet<char> ValidSeparators = new HashSet<char>() { ' ', '\t', '\r', '\n', ';', ',', '.', '(', ')', '[', ']', '{', '}', '>', '<', '=', '!', '&', '|', '^', '+', '-', '*', '/', '#' };

        public static readonly HashSet<string> DontRemoveIfBranchesKeywords = new HashSet<string>() { "UNITY_SINGLE_PASS_STEREO", "FORWARD_BASE_PASS", "FORWARD_ADD_PASS", "POINT", "SPOT" };
        public static readonly HashSet<string> KeywordsUsedByPragmas = new HashSet<string>() {  };

        public static readonly HashSet<string> ValidPropertyDataTypes = new HashSet<string>()
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

        public enum RenderPipeline
        {
            BuiltIn,
            URP,
            Other
        };

        public static readonly string[] PreprocessStructureStart = new string[]
        {
            "CGINCLUDE",
            "CBUFFER_START(UnityPerMaterial)"
        };

        public static readonly string[] PreprocessStructureEnd = new string[]
        {
            "ENDCG",
            "CBUFFER_END"
        };

        public static readonly string[] CodeBlockStart = new string[]
        {
            "CGPROGRAM",
            "HLSLPROGRAM"
        };

        public static readonly string[] CodeBlockEnd = new string[]
        {
            "ENDCG",
            "ENDHLSL"
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

            public void ToCode(StringBuilder sb)
            {
                if (Config.Instance.enableDeveloperMode)
                {
                    sb.Append(" /*");
                    sb.Append(name);
                    sb.Append("*/");
                }
                switch (type)
                    {
                        case PropertyType.Float:
                            string constantValue;
                            // Special Handling for ints 
                            if (lastDeclarationType == "int")
                                constantValue = value.x.ToString("F0", CultureInfo.InvariantCulture);
                            else
                                constantValue = value.x.ToString("0.0####################", CultureInfo.InvariantCulture);

                            // Add comment with property name, for easier debug
                            sb.Append(constantValue);
                            break;
                        case PropertyType.Vector:
                            sb.Append("float4(");
                            sb.Append(value.x.ToString(CultureInfo.InvariantCulture));
                            sb.Append(",");
                            sb.Append(value.y.ToString(CultureInfo.InvariantCulture));
                            sb.Append(",");
                            sb.Append(value.z.ToString(CultureInfo.InvariantCulture));
                            sb.Append(",");
                            sb.Append(value.w.ToString(CultureInfo.InvariantCulture));
                            sb.Append(")");
                            break;
                    }
            }
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

        private struct ApplyStruct
        {
            public Material material;
            public Shader shader;
            public string newShaderName;
            public List<RenamingProperty> animatedPropsToRename;
            public List<RenamingProperty> animatedPropsToDuplicate;
            public string animPropertySuffix;
            public bool shared;
            public List<string> stripTextures;
        }

        private static string GetColorMaskString(int colorMask)
        {
            if (colorMask == 0) return "0";
            string mask = "";
            if ((colorMask & 8) != 0) mask += "R";
            if ((colorMask & 4) != 0) mask += "G";
            if ((colorMask & 2) != 0) mask += "B";
            if ((colorMask & 1) != 0) mask += "A";
            return mask;
        }

        private static Dictionary<Material, ApplyStruct> s_applyStructsLater = new Dictionary<Material, ApplyStruct>();

#region Public API
        /// <summary>
        /// The type of progress bar to show when locking/unlocking materials
        /// </summary>
        public enum ProgressBar{
            /// <summary>
            /// No progress bar
            /// </summary>
            None, 
            /// <summary>
            /// Progress bar with cancel button
            /// </summary>
            Cancellable,
            /// <summary>
            /// Progress bar without cancel button
            /// </summary>
            Uncancellable
        }
        
        /// <summary>
        /// Locks all given materials
        /// </summary>
        /// <param name="materials">List of materials to lock</param>
        /// <param name="progressBarType">What type of progress bar to show</param>
        /// <returns></returns>
        [PublicAPI]
        public static bool LockMaterials(IEnumerable<Material> materials, ProgressBar progressBarType = ProgressBar.None)
        {
            return SetLockedForAllMaterialsInternal(materials, 1, progressBarType != ProgressBar.None, false, progressBarType == ProgressBar.Cancellable);
        }

        /// <summary>
        /// Unlocks all given materials
        /// </summary>
        /// <param name="materials">List of materials to unlock</param>
        /// <param name="progressBarType">What type of progress bar to show</param>
        /// <returns></returns>
        [PublicAPI]
        public static bool UnlockMaterials(IEnumerable<Material> materials, ProgressBar progressBarType = ProgressBar.None)
        {
            return SetLockedForAllMaterialsInternal(materials, 0, progressBarType != ProgressBar.None, false, progressBarType == ProgressBar.Cancellable);
        }

#endregion
#region MenuItems
        // ifex indenting
        [MenuItem("Assets/Thry/Shaders/Ifex Indenting", false, 305)]
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
            SetLockForAllChildrenInternal(Selection.gameObjects, 0, true);
        }

        [MenuItem("GameObject/Thry/Materials/Lock All", false,0)]
        static void LockAllChildren()
        {
            SetLockForAllChildrenInternal(Selection.gameObjects, 1, true);
        }

        //---Asset Unlocking

        [MenuItem("Assets/Thry/Materials/Unlock All", false, 303)]
        static void UnlockAllMaterials()
        {
            SetLockedForAllMaterialsInternal(GetSelectedLockableMaterials(), 0, true);
        }

        [MenuItem("Assets/Thry/Materials/Unlock All", true)]
        static bool UnlockAllMaterialsValidator()
        {
            return AreSelectedObjectsMaterials();
        }

        //---Asset Locking

        [MenuItem("Assets/Thry/Materials/Lock All", false, 303)]
        static void LockAllMaterials()
        {
            SetLockedForAllMaterialsInternal(GetSelectedLockableMaterials(), 1, true);
        }

        [MenuItem("Assets/Thry/Materials/Lock All", true)]
        static bool LockAllMaterialsValidator()
        {
            return AreSelectedObjectsMaterials();
        }

        //This does not work for folders on the left side of the project explorer, because they are not exposed to Selection
        internal static IEnumerable<string> GetSelectedFolders()
        {
            return Selection.objects.Select(o => AssetDatabase.GetAssetPath(o)).Where(p => Directory.Exists(p));
        }

        internal static List<Material> FindMaterials(string folderPath, bool recursive = true)
        {
            List<Material> materials = new List<Material>();
            foreach (string f in Directory.GetFiles(folderPath))
            {
                if (AssetDatabase.GetMainAssetTypeAtPath(f) != typeof(Material)) continue;

                materials.Add(AssetDatabase.LoadAssetAtPath<Material>(f));
            }

            if (!recursive) return materials;

            foreach (string f in Directory.GetDirectories(folderPath)) materials.AddRange(FindMaterials(f, true));

            return materials;
        }

        internal static List<Material> FindMaterials(IEnumerable<string> folders, bool recursive = true)
        {
            List<Material> materials = new List<Material>();

            foreach (string f in folders) materials.AddRange(FindMaterials(f, recursive));

            return materials;
        }

        //----Folder Lock

        [MenuItem("Assets/Thry/Materials/Lock Folder", false, 303)]
        static void LockFolder()
        {
            SetLockedForAllMaterialsInternal(FindMaterials(GetSelectedFolders()), 1, true);
        }

        [MenuItem("Assets/Thry/Materials/Lock Folder", true)]
        static bool LockFolderValidator()
        {
            return GetSelectedFolders().Any();
        }

        //-----Folder Unlock
        [MenuItem("Assets/Thry/Materials/Unlock Folder", false, 303)]
        static void UnLockFolder()
        {
            SetLockedForAllMaterialsInternal(FindMaterials(GetSelectedFolders()), 0, true);
        }

        [MenuItem("Assets/Thry/Materials/Unlock Folder", true)]
        static bool UnLockFolderValidator()
        {
            return GetSelectedFolders().Any();
        }

        //----Folder Unlock

        static bool AreSelectedObjectsMaterials()
        {
            if (Selection.assetGUIDs != null && Selection.assetGUIDs.Length > 0)
            {
                return Selection.assetGUIDs.All(g => AssetDatabase.GetMainAssetTypeAtPath(AssetDatabase.GUIDToAssetPath(g)) == typeof(Material));
            }
            return false;
        }

        static IEnumerable<Material> GetSelectedLockableMaterials()
        {
            return Selection.assetGUIDs.Select(g => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g))).Where(m => m != null && IsShaderUsingThryOptimizer(m.shader));
        }
#endregion

#region Animiated Tagging
        public static void CopyAnimatedTag(MaterialProperty source, Material[] targets)
        {
            string val = (source.targets[0] as Material).GetTag(source.name + AnimatedTagSuffix, false, "");
            foreach (Material m in targets)
            {
                m.SetOverrideTag(source.name+ AnimatedTagSuffix, val);
            }
        }

        public static void CopyAnimatedTag(Material source, MaterialProperty target)
        {
            string val = source.GetTag(target.name + AnimatedTagSuffix, false, "");
            foreach (Material m in target.targets)
            {
                m.SetOverrideTag(target.name + AnimatedTagSuffix, val);
            }
        }

        public static void CopyAnimatedTag(MaterialProperty source, MaterialProperty target)
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
        #endregion

        #region Shader Property Helpers

        public static RenderPipeline GetActiveRenderPipeline()
        {
            var pipelineAsset = GraphicsSettings.defaultRenderPipeline;
            if (pipelineAsset != null)
            {
                if (pipelineAsset.GetType().Name == "UniversalRenderPipelineAsset")
                {
                    // URP
                    return RenderPipeline.URP;
                }
                else
                {
                    return RenderPipeline.Other;
                }
            }
            else
            {
                // Built-in pipeline
                return RenderPipeline.BuiltIn;
            }
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
            if(prop == null) return false;
            // if not a texture, but has non-modifiable texture data flag, is used as indicator to prevent locking
            return prop.displayName.EndsWith(ExemptFromLockingSuffix, StringComparison.Ordinal) 
                || (prop.type != MaterialProperty.PropType.Texture && prop.flags.HasFlag(MaterialProperty.PropFlags.NonModifiableTextureData))
                || GetAttributes(prop).Contains("DoNotLock");
        }

        public static bool IsPropertyExcemptFromLocking(ShaderPart part)
        {
            if(part.MaterialProperty == null) return false;
            return part.HasAttribute("DoNotLock")
            || (part.MaterialProperty.type != MaterialProperty.PropType.Texture && part.MaterialProperty.flags.HasFlag(MaterialProperty.PropFlags.NonModifiableTextureData))
            || part.MaterialProperty.displayName.EndsWith(ExemptFromLockingSuffix, StringComparison.Ordinal);
        }

        private static string[] GetAttributes(MaterialProperty prop)
        {
            Shader s = (prop.targets[0] as Material).shader;
            if(s == null) return new string[0];
            int index = s.FindPropertyIndex(prop.name);
            if(index < 0) return new string[0];
            return s.GetPropertyAttributes(index);
        }

        private static bool CopyProperty(Material material, MaterialProperty source, string targetName)
        {
            switch (source.type)
            {
                case MaterialProperty.PropType.Color:
                    material.SetColor(targetName, source.colorValue);
                    break;
                case MaterialProperty.PropType.Vector:
                    material.SetVector(targetName, source.vectorValue);
                    break;
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    material.SetFloat(targetName, source.floatValue);
                    break;
#if UNITY_2022_1_OR_NEWER
                case MaterialProperty.PropType.Int:
                    material.SetInt(targetName, source.intValue);
                    break;
#endif
                case MaterialProperty.PropType.Texture:
                    material.SetTexture(targetName, source.textureValue);
                    material.SetTextureScale(targetName, new Vector2(source.textureScaleAndOffset.x, source.textureScaleAndOffset.y));
                    material.SetTextureOffset(targetName, new Vector2(source.textureScaleAndOffset.z, source.textureScaleAndOffset.w));
                    break;
                default:
                    return false;
            }

            return true;
        }
#endregion
#region Lock Managing
        public static void ToggleLockFromPropertyButton(MaterialProperty prop)
        {
            SetLockedForAllMaterialsInternal(prop.targets.Select(t => t as Material), prop.GetNumber() == 1 ? 0 : 1, true, false, false, prop);
        }

        [Obsolete("Use ShaderOptimizer.LockMaterials or ShaderOptimizer.UnlockMaterials instead")]
        public static bool SetLockForAllChildren(GameObject[] objects, int lockState, bool showProgressbar = false, bool showDialog = false, bool allowCancel = true)
        {
            return SetLockForAllChildrenInternal(objects, lockState, showProgressbar, showDialog, allowCancel);
        }

        [Obsolete("Use ShaderOptimizer.LockMaterials or ShaderOptimizer.UnlockMaterials instead")]
        public static bool SetLockedForAllMaterials(IEnumerable<Material> materials, int lockState, bool showProgressbar = false, bool showDialog = false, bool allowCancel = true, MaterialProperty shaderOptimizerProp = null)
        {
            return SetLockedForAllMaterialsInternal(materials, lockState, showProgressbar, showDialog, allowCancel, shaderOptimizerProp);
        }

        private static bool SetLockForAllChildrenInternal(GameObject[] objects, int lockState, bool showProgressbar = false, bool showDialog = false, bool allowCancel = true)
        {
            IEnumerable<Material> materials = objects.Select(o => o.GetComponentsInChildren<Renderer>(true)).SelectMany(rA => rA.SelectMany(r => r.sharedMaterials));
            return SetLockedForAllMaterialsInternal(materials, lockState, showProgressbar, showDialog);
        }

        private static Dictionary<string, List<Material>> s_shaderPropertyCombinations = new Dictionary<string, List<Material>>();
        private static bool SetLockedForAllMaterialsInternal(IEnumerable<Material> materials, int lockState, bool showProgressbar = false, bool showDialog = false, bool allowCancel = true, MaterialProperty shaderOptimizerProp = null)
        {
            Helper.RegisterEditorUse();
            //first the shaders are created. compiling is suppressed with start asset editing
            AssetDatabase.StartAssetEditing();

            bool isLocking = lockState == 1;

            //Get cleaned materia list
            // The GetPropertyDefaultFloatValue is changed from 0 to 1 when the shader is locked in
            IEnumerable<Material> materialsToChangeLock = materials.Where(m => m != null
                && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(m))
                && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(m.shader))
                && IsShaderUsingThryOptimizer(m.shader)
                &&  (   m.shader.name.StartsWith("Hidden/Locked/")
                    || (m.shader.name.StartsWith("Hidden/") && m.GetTag("OriginalShader",false,"") != "" 
                        && m.shader.GetPropertyDefaultFloatValue(m.shader.FindPropertyIndex(GetOptimizerPropertyName(m.shader))) == 1)
                    ) != isLocking)
                .Distinct();

            // Make sure keywords are set correctly for materials to be locked. If unlocking, do this after the shaders are unlocked
            if(isLocking && Config.Instance.fixKeywordsWhenLocking)
                ShaderEditor.FixKeywords(materialsToChangeLock);

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

            Object[] prevTargets = Selection.objects;
            if (ShaderEditor.Active != null)
            {
                Selection.objects = new Object[0];
            }

            //Create shader assets
            foreach (Material m in materialsToChangeLock.ToList()) //have to call ToList() here otherwise the Unlock Shader button in the ShaderGUI doesn't work
            {
                //do progress bar
                if (showProgressbar)
                {
                    if (allowCancel)
                    {
                        if (EditorUtility.DisplayCancelableProgressBar(isLocking ? "Locking Materials" : "Unlocking Materials", m.name, i / length)) break;
                    }
                    else
                    {
                        EditorUtility.DisplayProgressBar(isLocking ? "Locking Materials" : "Unlocking Materials", m.name, i / length);
                    }
                }
                //create the assets
                try
                {
                    if (isLocking)
                    {
                        string hash = MaterialToShaderPropertyHash(m);
                        // Check that shader has already been created for this hash and still exists
                        // Or that the shader is being created for this has during this session
                        Material reference = null;
                        if (s_shaderPropertyCombinations.ContainsKey(hash))
                        {
                            s_shaderPropertyCombinations[hash].RemoveAll(m2 => m2 == null);
                            reference = s_shaderPropertyCombinations[hash].FirstOrDefault(m2 => m2 != m && (materialsToChangeLock.Contains(m2) || Shader.Find(s_applyStructsLater[m2].newShaderName) != null));
                        }
                        if (reference != null)
                        {
                            // Reuse existing shader and struct
                            ApplyStruct applyStruct = s_applyStructsLater[reference];
                            applyStruct.material = m;
                            s_applyStructsLater[m] = applyStruct;
                            //Disable shader keywords
                            foreach (string keyword in m.shaderKeywords)
                                if (m.IsKeywordEnabled(keyword)) m.DisableKeyword(keyword);

                        }
                        // Create new locked shader
                        else
                        {
                            Lock(m,
                                MaterialEditor.GetMaterialProperties(new Object[] { m }),
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
                    else if (!isLocking)
                    {
                        Unlock(m, shaderOptimizerProp);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    string position = e.StackTrace.Split('\n').FirstOrDefault(l => l.Contains("ThryEditor"));
                    if (position != null)
                    {
                        position = position.Split(new string[] { "ThryEditor" }, StringSplitOptions.None).LastOrDefault();
                        Debug.LogError("Could not un-/lock material " + m.name + " | Error thrown at " + position + "\n" + e.StackTrace);
                    }
                    else
                    {
                        Debug.LogError("Could not un-/lock material " + m.name + "\n" + e.StackTrace);
                    }
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.StopAssetEditing();
                    return false;
                }
                i++;
            }

            EditorUtility.ClearProgressBar();

            // In case any keywords were messed up in the material following unlock, fix them now
            if (!isLocking && Config.Instance.fixKeywordsWhenLocking)
                ShaderEditor.FixKeywords(materialsToChangeLock);

            AssetDatabase.StopAssetEditing();
            //unity now compiles all the shaders

            //now all new shaders are applied. this has to happen after unity compiled the shaders
            if (isLocking)
            {
                AssetDatabase.Refresh();
                //Apply new shaders
                foreach (Material m in materialsToChangeLock)
                {
                    if (ShaderOptimizer.LockApplyShader(m))
                    {
                        m.SetNumber(GetOptimizerPropertyName(m.shader), 1);
                    }
                }
            }
            AssetDatabase.Refresh();

            // Make sure things get saved after a cycle. This prevents thumbnails from getting stuck
            if(Config.Instance.saveAfterLockUnlock)
                EditorApplication.update += QueueSaveAfterLockUnlock;

            if (ShaderEditor.Active != null)
            {
                Selection.objects = prevTargets;
            }
                
            return true;
        }

        // This is just a wrapper so that it waits a cycle before saving
        static void QueueSaveAfterLockUnlock()
        {
            EditorApplication.update -= QueueSaveAfterLockUnlock;
            EditorApplication.update += SaveAfterLockUnlock;
        }

        static void SaveAfterLockUnlock()
        {
            if (ShaderUtil.anythingCompiling)
                return;

            EditorApplication.update -= SaveAfterLockUnlock;
            AssetDatabase.SaveAssets();
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
#if UNITY_2022_1_OR_NEWER
                        case MaterialProperty.PropType.Int:
                            stringBuilder.Append(m.GetInteger(propName)
                                .ToString(CultureInfo.InvariantCulture));
                            break;
#endif
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
            byte[] bytes = Encoding.ASCII.GetBytes(stringBuilder.ToString());
            using (var sha = new MD5CryptoServiceProvider())
                return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLower();
        }
#endregion
#region Locking
        private static bool Lock(Material material, MaterialProperty[] props, bool applyShaderLater = false)
        {
            RenderPipeline pipeline = GetActiveRenderPipeline();
            if (pipeline == RenderPipeline.Other)
            {
                Debug.LogError("Locking is not supported for this render pipeline. Please use the built-in pipeline or URP.");
                return false;
            }
            // File filepaths and names
            Shader shader = material.shader;
            string shaderFilePath = AssetDatabase.GetAssetPath(shader);
            string materialFilePath = AssetDatabase.GetAssetPath(material);
            string materialFolder = Path.GetDirectoryName(materialFilePath);
            bool isSubAsset = AssetDatabase.IsSubAsset(material);
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(material, out string guid, out long fileId))
            {
                guid = AssetDatabase.AssetPathToGUID(materialFilePath);
                isSubAsset = false;
            }



            string newShaderName = "Hidden/Locked/" + shader.name + "/" + guid + (isSubAsset ? $"_{fileId}" : "");
            string shaderOptimizerButtonDrawerName = $"[{nameof(ThryShaderOptimizerLockButtonDrawer).Replace("Drawer", "")}]";
            //string newShaderDirectory = materialFolder + "/OptimizedShaders/" + material.name + "-" + smallguid + "/";
            // unity path stuff (https://docs.unity3d.com/Manual/SpecialFolders.html)
            // ~ & . hides the folder in the editor and unity will not be able to find the shader
            string subfoldername = material.name;
            while(subfoldername.StartsWith("."))
                subfoldername = subfoldername.Substring(1) + "_dot_";
            while(subfoldername.EndsWith("~"))
                subfoldername = subfoldername.Substring(0, subfoldername.Length - 1) + "_tilde_";
            string newShaderDirectory = materialFolder + "/OptimizedShaders/" + subfoldername + "/";

            // if directory already exists swap to using the guid
            if (Directory.Exists(newShaderDirectory))
            {
                newShaderDirectory = materialFolder + "/OptimizedShaders/" + guid + (isSubAsset ? $"_{fileId}" : "") + "/";
            }
            

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
            List<string> stripTextures = new List<string>();
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
                    UseInlineSamplerStates = (prop.GetNumber() == 1);
                    continue;
                }
                else if (prop.name.StartsWith(GeometryShaderEnabledPropertyName, StringComparison.Ordinal))
                {
                    if (prop.name == GeometryShaderEnabledPropertyName)
                        UseGeometry = (prop.GetNumber() == 1);
                    else if (prop.name == GeometryShaderEnabledPropertyName + "ForwardBase")
                        UseGeometryForwardBase = (prop.GetNumber() == 1);
                    else if (prop.name == GeometryShaderEnabledPropertyName + "ForwardAdd")
                        UseGeometryForwardAdd = (prop.GetNumber() == 1);
                    else if (prop.name == GeometryShaderEnabledPropertyName + "ShadowCaster")
                        UseGeometryShadowCaster = (prop.GetNumber() == 1);
                    else if (prop.name == GeometryShaderEnabledPropertyName + "Meta")
                        UseGeometryMeta = (prop.GetNumber() == 1);
                }
                else if (prop.name.StartsWith(TessellationEnabledPropertyName, StringComparison.Ordinal))
                {
                    if (prop.name == TessellationEnabledPropertyName)
                        UseTessellation = (prop.GetNumber() == 1);
                    else if (prop.name == TessellationEnabledPropertyName + "ForwardBase")
                        UseTessellationForwardBase = (prop.GetNumber() == 1);
                    else if (prop.name == TessellationEnabledPropertyName + "ForwardAdd")
                        UseTessellationForwardAdd = (prop.GetNumber() == 1);
                    else if (prop.name == TessellationEnabledPropertyName + "ShadowCaster")
                        UseTessellationShadowCaster = (prop.GetNumber() == 1);
                    else if (prop.name == TessellationEnabledPropertyName + "Meta")
                        UseTessellationMeta = (prop.GetNumber() == 1);
                }

                string animateTag = material.GetTag(prop.name + AnimatedTagSuffix, false, "");
                if (!string.IsNullOrEmpty(animateTag))
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
#if UNITY_2022_1_OR_NEWER
                    case MaterialProperty.PropType.Int:
                        propData = new PropertyData();
                        propData.type = PropertyType.Float;
                        propData.name = prop.name;
                        propData.value = new Vector4(prop.intValue, 0, 0, 0);
                        constantProps.Add(propData);
                        break;
#endif
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
                int lightModesMask = (int)disabledLightModesProperty.GetNumber();
                if ((lightModesMask & (int)LightMode.ForwardAdd) != 0)
                    disabledLightModes.Add("ForwardAdd");
                if ((lightModesMask & (int)LightMode.ShadowCaster) != 0)
                    disabledLightModes.Add("ShadowCaster");
            }
                
            // Parse shader and cginc files, also gets preprocessor macros
            List<ParsedShaderFile> shaderFiles = new List<ParsedShaderFile>();
            List<Macro> macros = new List<Macro>();
            if (!ParseShaderFilesRecursive(shaderFiles, newShaderDirectory, shaderFilePath, macros, material, stripTextures))
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
                if (psf.filePath.EndsWith(".shader", StringComparison.Ordinal) ||
                    psf.filePath.EndsWith(".hlsl", StringComparison.Ordinal))
                {
                    for (int i=0; i<psf.lines.Length;i++)
                    {
                        string trimmedLine = psf.lines[i].TrimStart();

                        if (trimmedLine.StartsWith("Shader", StringComparison.Ordinal))
                        {
                            string originalSgaderName = psf.lines[i].Split('\"')[1];
                            psf.lines[i] = psf.lines[i].Replace(originalSgaderName, newShaderName);
                        }
                        else if (trimmedLine.StartsWith(shaderOptimizerButtonDrawerName))
                        {
                            psf.lines[i] = Regex.Replace(psf.lines[i], @"\d+\w*$", "1");
                        }
                        else if (trimmedLine.StartsWith("//#pragmamulti_compile_LOD_FADE_CROSSFADE", StringComparison.Ordinal))
                        {
                            MaterialProperty crossfadeProp = Array.Find(props, x => x.name == LODCrossFadePropertyName);
                            if (crossfadeProp != null && crossfadeProp.GetNumber() == 1)
                                psf.lines[i] = psf.lines[i].Replace("//#pragma", "#pragma");
                        }
                        else if (trimmedLine.StartsWith("//\"IgnoreProjector\"=\"True\"", StringComparison.Ordinal))
                        {
                            MaterialProperty projProp = Array.Find(props, x => x.name == IgnoreProjectorPropertyName);
                            if (projProp != null && projProp.GetNumber() == 1)
                                psf.lines[i] = psf.lines[i].Replace("//\"IgnoreProjector", "\"IgnoreProjector");
                        }
                        else if (trimmedLine.StartsWith("//\"ForceNoShadowCasting\"=\"True\"", StringComparison.Ordinal))
                        {
                            MaterialProperty forceNoShadowsProp = Array.Find(props, x => x.name == ForceNoShadowCastingPropertyName);
                            if (forceNoShadowsProp != null && forceNoShadowsProp.GetNumber() == 1)
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
                        else if (trimmedLine.StartsWith(PreprocessStructureStart[(int) pipeline], StringComparison.Ordinal))
                        {
                            for (int j = i + 1; j < psf.lines.Length; j++)
                                if (psf.lines[j].TrimStart().StartsWith(PreprocessStructureEnd[(int) pipeline], StringComparison.Ordinal))
                                {
                                    ReplaceShaderValues(material, psf.lines, i + 1, j, props, constantPropsDictionary, macrosArray, grabPassVariables.ToArray());
                                    break;
                                }
                        }
                        else if (trimmedLine.StartsWith(CodeBlockStart[(int) pipeline], StringComparison.Ordinal))
                        {
                            if (commentKeywords == 0)
                                psf.lines[i] += optimizerDefines;
                            for (int j = i + 1; j < psf.lines.Length; j++)
                                if (psf.lines[j].TrimStart().StartsWith(CodeBlockEnd[(int) pipeline], StringComparison.Ordinal))
                                {
                                    ReplaceShaderValues(material, psf.lines, i + 1, j, props, constantPropsDictionary, macrosArray, grabPassVariables.ToArray());
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
                                    int j = i - 1;
                                    for (; j >= 0; j--)
                                        if (psf.lines[j].Replace(" ", "").Replace("\t", "") == "Pass")
                                            break;
                                    // then delete each line until a standalone ENDCG line is found
                                    for (; j < psf.lines.Length; j++)
                                    {
                                        if (psf.lines[j].Replace(" ", "").Replace("\t", "") == "ENDCG")
                                            break;
                                        psf.lines[j] = "";
                                    }
                                    // then delete each line until a standalone '}' line is found
                                    for (; j < psf.lines.Length; j++)
                                    {
                                        string temp = psf.lines[j];
                                        psf.lines[j] = "";
                                        if (temp.Replace(" ", "").Replace("\t", "") == "}")
                                            break;
                                    }
                                }
                            }
                        }
                        else if (trimmedLine.StartsWith("ColorMask", StringComparison.Ordinal))
                        {
                            Match regMatch = Regex.Match(trimmedLine, @"\[\w+\]");
                            if(regMatch.Success)
                            {
                                string trimmedRegMatch = regMatch.Value.Trim('[', ']');
                                PropertyData colorMaskProp = constantProps.FirstOrDefault(x => x.name == trimmedRegMatch);
                                if (colorMaskProp != null)
                                {
                                    psf.lines[i] = psf.lines[i].Replace(regMatch.Value, GetColorMaskString((int)colorMaskProp.value.x));
                                }
                            }
                        }
                        else if (trimmedLine.StartsWith("Cull", StringComparison.OrdinalIgnoreCase))
                        {
                            Match regMatch = Regex.Match(trimmedLine, @"\[\w+\]");
                            if(regMatch.Success)
                            {
                                string trimmedRegMatch = regMatch.Value.Trim('[', ']');
                                PropertyData cullModeProp = constantProps.FirstOrDefault(x => x.name == trimmedRegMatch);
                                if (cullModeProp != null)
                                {
                                    psf.lines[i] = psf.lines[i].Replace(regMatch.Value, ((UnityEngine.Rendering.CullMode)cullModeProp.value.x).ToString());
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
            applyStruct.newShaderName = newShaderName;
            applyStruct.animatedPropsToRename = animatedPropsToRename;
            applyStruct.animatedPropsToDuplicate = animatedPropsToDuplicate;
            applyStruct.animPropertySuffix = animPropertySuffix;
            applyStruct.stripTextures = stripTextures;

            if (applyShaderLater)
            {
                //Debug.Log("Apply later: "+applyStructsLater.Count+ ", "+material.name);
                s_applyStructsLater[material] = applyStruct;
                return true;
            }
            return LockApplyShader(applyStruct);
        }

        private static bool LockApplyShader(Material material)
        {
            if (s_applyStructsLater.ContainsKey(material) == false) return false;
            ApplyStruct applyStruct = s_applyStructsLater[material];
            if (applyStruct.shared)
            {
                material.shader = applyStruct.material.shader;
                return true;
            }
            //applyStructsLater.Remove(material);
            return LockApplyShader(applyStruct);
        }

        public static void ApplyMaterialPropertyDrawersPatch(Material material) {}
        public static void ApplyMaterialPropertyDrawersFromNativePatch(Material material) {}
        static MethodInfo ApplyMaterialPropertyDrawersOriginalMethodInfo = typeof(MaterialEditor).GetMethod("ApplyMaterialPropertyDrawers", new Type[] {typeof(Material)});
        static MethodInfo ApplyMaterialPropertyDrawersFromNativeOriginalMethodInfo = typeof(MaterialEditor).GetMethod("ApplyMaterialPropertyDrawersFromNative", BindingFlags.NonPublic | BindingFlags.Static);
        static MethodInfo ApplyMaterialPropertyDrawersPatchMethodInfo = typeof(ShaderOptimizer).GetMethod(nameof(ApplyMaterialPropertyDrawersPatch), BindingFlags.Public | BindingFlags.Static);
        static MethodInfo ApplyMaterialPropertyDrawersFromNativePatchMethodInfo = typeof(ShaderOptimizer).GetMethod(nameof(ApplyMaterialPropertyDrawersFromNativePatch), BindingFlags.Public | BindingFlags.Static);
        

        public static void DetourApplyMaterialPropertyDrawers()
        {
        // Unity 2022 Crashes on apple silicon when detouring ApplyMaterialPropertyDrawers
            Helper.TryDetourFromTo(ApplyMaterialPropertyDrawersOriginalMethodInfo, ApplyMaterialPropertyDrawersPatchMethodInfo);
#if UNITY_2022_1_OR_NEWER
            Helper.TryDetourFromTo(ApplyMaterialPropertyDrawersFromNativeOriginalMethodInfo, ApplyMaterialPropertyDrawersFromNativePatchMethodInfo);
#endif
        }

        public static void RestoreApplyMaterialPropertyDrawers()
        {
            Helper.RestoreDetour(ApplyMaterialPropertyDrawersOriginalMethodInfo);
#if UNITY_2022_1_OR_NEWER
            Helper.RestoreDetour(ApplyMaterialPropertyDrawersFromNativeOriginalMethodInfo);
#endif
        }

        private static bool LockApplyShader(ApplyStruct applyStruct)
        {
            Material material = applyStruct.material;
            Shader shader = applyStruct.shader;
            string newShaderName = applyStruct.newShaderName;
            List<RenamingProperty> animatedPropsToRename = applyStruct.animatedPropsToRename;
            List<RenamingProperty> animatedPropsToDuplicate = applyStruct.animatedPropsToDuplicate;
            string animPropertySuffix = applyStruct.animPropertySuffix;

            string shaderGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(shader));

            // Write original shader to override tag
            material.SetOverrideTag(TAG_ORIGINAL_SHADER, shader.name);
            material.SetOverrideTag(TAG_ORIGINAL_SHADER_GUID, shaderGUID);
            // Write the new shader folder name in an override tag so it will be deleted

            // For some reason when shaders are swapped on a material the RenderType override tag gets completely deleted and render queue set back to -1
            // So these are saved as temp values and reassigned after switching shaders
            string renderType = material.GetTag("RenderType", false, "");
            int renderQueue = material.renderQueue;

            // Strip removed textures
            SerializedObject serializedObject = new SerializedObject(material);
            SerializedProperty serializedTexProperties = serializedObject.FindProperty("m_SavedProperties.m_TexEnvs");
            List<(string tag,string guid)> savedTextures = new List<(string,string)>();
            for(int i=0;i<serializedTexProperties.arraySize;i++)
            {
                SerializedProperty prop = serializedTexProperties.GetArrayElementAtIndex(i);
                string propName = prop.FindPropertyRelative("first").stringValue;
                Object propTex = prop.FindPropertyRelative("second.m_Texture").objectReferenceValue;
                bool doStrip = applyStruct.stripTextures.Contains(propName) && propTex != null;
                if (doStrip || propTex == null)
                {
                    if(doStrip)
                    {
                        savedTextures.Add(("_stripped_tex_" + propName, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(material.GetTexture(propName)))));
                    }
                    serializedTexProperties.DeleteArrayElementAtIndex(i);
                    i -= 1;
                }
            }
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            foreach ((string tag, string guid) in savedTextures)
                material.SetOverrideTag(tag, guid);

            // Actually switch the shader
            Shader newShader = Shader.Find(newShaderName);
            if (newShader == null)
            {
                Debug.LogError("[Shader Optimizer] Generated shader " + newShaderName + " could not be found");
                return false;
            }
            // Detour ApplyMaterialPropertyDrawers to prevent it from running, for performance reasons
            DetourApplyMaterialPropertyDrawers();
            material.shader = newShader;
            RestoreApplyMaterialPropertyDrawers();
            material.SetOverrideTag("RenderType", renderType);
            material.renderQueue = renderQueue;
            

            material.SetOverrideTag("OriginalKeywords", string.Join(" ", material.shaderKeywords));
            // Remove ALL keywords
            foreach (string keyword in material.shaderKeywords)
                if(material.IsKeywordEnabled(keyword)) material.DisableKeyword(keyword);

            var propertiesToCopy = animatedPropsToRename.Union(animatedPropsToDuplicate);
            foreach (var animProp in propertiesToCopy)
            {
                if(!CopyProperty(material, animProp.Prop, $"{animProp.Prop.name}_{animPropertySuffix}"))
                    throw new ArgumentOutOfRangeException(nameof(material), "This property type should not be renamed and can not be set.");
            }

            return true;
        }

        // Preprocess each file for macros and includes
        // Save each file as string[], parse each macro with //KSOEvaluateMacro
        // Only editing done is replacing #include "X" filepaths where necessary
        // most of these args could be private static members of the class
        private static bool ParseShaderFilesRecursive(List<ParsedShaderFile> filesParsed, string newTopLevelDirectory, string filePath, List<Macro> macros, Material material, List<string> stripTextures)
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

            Stack<string> removeEndifStackIfLines = new Stack<string>();
            StringBuilder removeEndifStackDebugging = new StringBuilder();

            bool isCommentedOut = false;

            int currentExcludeDepth = 0;
            bool doExclude = false;
            int excludeStartDepth = 0;

            for (int i=0; i<fileLines.Length; i++)
            {
                string lineParsed = fileLines[i].TrimStart();

                if (lineParsed.StartsWith("//", StringComparison.Ordinal))
                {
                    // Exclusion logic
                    if (lineParsed.StartsWith("//ifex", StringComparison.Ordinal))
                    {
                        if (!doExclude) // if already excluding, only track depth
                        {
                            var condition = DefineableCondition.Parse(lineParsed.Substring(6), material);
                            if (condition.Test())
                            {
                                doExclude = true;
                                excludeStartDepth = currentExcludeDepth;
                            }
                        }
                        currentExcludeDepth++;
                    }
                    else if (lineParsed.StartsWith("//endex", StringComparison.Ordinal))
                    {
                        if (currentExcludeDepth == 0)
                        {
                            Debug.LogError("[Shader Optimizer] Number of 'endex' statements does not match number of 'ifex' statements."
                                +$"\nError found in file '{filePath}' line {i+1}");
                        }
                        else
                        {
                            currentExcludeDepth--;
                            if (currentExcludeDepth == excludeStartDepth) doExclude = false;
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
                if (doExclude)
                {
                    // check for texture property definitions, remove textures later
                    // needs specific naming
                    if (lineParsed.EndsWith("{ }", StringComparison.Ordinal) && lineParsed.IndexOf("2D)", StringComparison.Ordinal) >= 0)
                    {
                        lineParsed = lineParsed.Substring(0, lineParsed.IndexOf('"'));
                        if (lineParsed.IndexOf("]", StringComparison.Ordinal) >= 0) // Unity 2019 doesn't like string.Contains(string, StringComparison)
                        {
                            lineParsed = lineParsed.Substring(lineParsed.LastIndexOf(']') + 1);
                        }
                        lineParsed = lineParsed.Substring(0, lineParsed.IndexOf('('));
                        lineParsed = lineParsed.Trim();
                        stripTextures.Add(lineParsed);
                    }
                    continue;
                }

                // Remove empty lines
                if (string.IsNullOrEmpty(lineParsed)) continue;
                // Remove code that is commented
                if (isCommentedOut && lineParsed.EndsWith("*/", StringComparison.OrdinalIgnoreCase))
                {
                    isCommentedOut = false;
                    continue;
                }
                else if (lineParsed.StartsWith("/*", StringComparison.OrdinalIgnoreCase))
                {
                    isCommentedOut = true;
                    continue;
                }
                if (isCommentedOut) continue;

                // Remove code from defines blocks
                if (REMOVE_UNUSED_IF_DEFS)
                {
                    // Check if line contains a preprocessor conditional (e.g., #if, #ifdef, #ifndef)
                    if (lineParsed.StartsWith("#if", StringComparison.Ordinal))
                    {
                        bool hasMultiple = lineParsed.Contains('&') || lineParsed.Contains('|');

#if DEBUG_IF_DEF_REMOVAL
                        removeEndifStackDebugging.AppendLine($"push {ifStacking}" + lineParsed);
                        removeEndifStackIfLines.Push(lineParsed);
#endif

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
                        if (removeEndifStack.Count == 0)
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
                        if (ifStacking == isNotIncludedAtDepth) isIncluded = true;
                        // For debugging
                        if (removeEndifStack.Count == 0)
                        {
                            Debug.LogError("[Shader Optimizer] Number of 'endif' statements does not match number of 'if' statements."
                                +$"\nError found in file '{filePath}' line {i+1}. Current output copied to clipboard.");
                            Debug.LogError(removeEndifStackDebugging.ToString());
                            GUIUtility.systemCopyBuffer = string.Join(Environment.NewLine, includedLines);
                        }
#if DEBUG_IF_DEF_REMOVAL
                            fileLines[i] += $" // {removeEndifStackIfLines.Peek()}";
                            removeEndifStackDebugging.AppendLine($"pop {ifStacking}" + removeEndifStackIfLines.Pop());
#endif

                        if (removeEndifStack.Pop()) continue;
                    }
                    if (!isIncluded) continue;
                }

                // Remove pragmas
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
                        if (includeFilename.StartsWith("Assets/", StringComparison.Ordinal) == false && includeFilename.StartsWith("Packages/", StringComparison.Ordinal) == false) // not absolute
                            includeFullpath = GetFullPath(includeFilename, Path.GetDirectoryName(filePath));
                        if (!ParseShaderFilesRecursive(filesParsed, newTopLevelDirectory, includeFullpath, macros, material, stripTextures))
                            return false;
                        // Change include to be be ralative to only one directory up, because all files are moved into the same folder
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
#if DEBUG_IF_DEF_REMOVAL
            // print macros and constants
            string c = string.Join("\n" , constants.Select(m => m.Key + " : " + m.Value));
            GUIUtility.systemCopyBuffer = c;
#endif

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
                        float maxTessellation = maxTessFactorProperty.GetNumber();
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

                for(int t=0;t<tokens.Length;t++)
                {
                    string token = tokens[t];
                    if (constants.ContainsKey(token))
                    {
                        PropertyData constant = constants[token];

                        int constantIndex;
                        int lastIndex = 0;
                        bool declarationFound = false;
                        while ((constantIndex = lines[i].IndexOf(constant.name, lastIndex, StringComparison.Ordinal)) != -1)
                        {
                            lastIndex = constantIndex + 1;
                            char charLeft = ' ';
                            if (constantIndex - 1 >= 0)
                                charLeft = lines[i][constantIndex - 1];
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
                            if (!declarationFound && t > 0)
                            {
                                if (ValidPropertyDataTypes.Contains(tokens[t-1]) && lines[i].Substring(constantIndex + constant.name.Length).TrimStart().StartsWith(";", StringComparison.Ordinal))
                                {
                                    constant.lastDeclarationType = tokens[t-1];
                                    declarationFound = true;
                                    continue;
                                }
                            }

                            // Replace with constant!
                            // This could technically be more efficient by being outside the IndexOf loop
                            StringBuilder sb = new StringBuilder(lines[i].Length * 2);
                            sb.Append(lines[i], 0, constantIndex);
                            constant.ToCode(sb);
                            sb.Append(lines[i], constantIndex + constant.name.Length, lines[i].Length - constantIndex - constant.name.Length);
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
#endregion

#region Unlocking
        public enum UnlockSuccess { hasNoSavedShader, wasNotLocked, couldNotFindOriginalShader, couldNotDeleteLockedShader,
            success}
        private static void Unlock(Material material, MaterialProperty shaderOptimizer = null)
        {
            //if unlock success set floats. not done for locking cause the sucess is checked later when applying the shaders
            UnlockSuccess success = UnlockConcrete(material);
            if (success == UnlockSuccess.success || success == UnlockSuccess.wasNotLocked
                || success == UnlockSuccess.couldNotDeleteLockedShader)
            {
                if (shaderOptimizer != null) shaderOptimizer.floatValue = 0;
                else material.SetNumber(GetOptimizerPropertyName(material.shader), 0);
            }
        }

        public static bool GuessShader(Shader locked, out Shader shader)
        {
            return GuessShader(locked?.name, out shader);
        }

        public static bool GuessShader(string name, out Shader shader)
        {
            shader = null;
            if (string.IsNullOrEmpty(name)) return false;

            if (name.StartsWith("Hidden/"))
                name = name.Substring(7); // Remove "Hidden/" prefix
            if (name.StartsWith("Locked/"))
                name = name.Substring(7); // Remove "Locked/" prefix
            name = Regex.Match(name, @".*(?=\/)").Value;

            ShaderInfo[] allShaders = ShaderUtil.GetAllShaderInfo();
            int closestDistance = int.MaxValue;
            string closestShaderName = null;
            foreach (ShaderInfo s in allShaders)
            {
                if (!s.supported) continue;
                int d = Helper.LevenshteinDistance(s.name, name);
                if (d < closestDistance)
                {
                    closestDistance = d;
                    closestShaderName = s.name;
                }
            }

            // Debug.Log(closestDistance + " < " + (name.Length * 0.5f) + " " + closestShaderName + " " + name);
            shader = Shader.Find(closestShaderName);
            return shader != null && closestDistance < name.Length * 0.5f;
        }

        private static UnlockSuccess UnlockConcrete(Material material)
        {
            Shader lockedShader = material.shader;
            // Check if shader is Hidden
            if (!lockedShader.name.StartsWith("Hidden/", StringComparison.Ordinal))
            {
                Debug.LogWarning("[Shader Optimizer] Shader " + lockedShader.name + " is not locked.");
                return UnlockSuccess.wasNotLocked;
            }

            Shader originalShader = GetOriginalShader(material);
            if (originalShader == null)
            {
                Debug.LogError("[Shader Optimizer] Original shader not saved to material, could not unlock shader");
                if(EditorUtility.DisplayDialog("Unlock Material", $"The original shader for {material.name} could not be resolved.\nPlease select a shader manually.", "Ok")) {}
                return UnlockSuccess.hasNoSavedShader;
            }

            // Build list of renamed properties
            string animPropertySuffix = $"_{GetRenamedPropertySuffix(material)}";
            List<MaterialProperty> renamedProperties = new List<MaterialProperty>();
            MaterialProperty[] props = MaterialEditor.GetMaterialProperties(new Object[] { material });
            foreach (MaterialProperty prop in props)
            {
                if (prop == null ||
                    !prop.name.EndsWith(animPropertySuffix, StringComparison.Ordinal)) continue;

                string propName = prop.name.Substring(0, prop.name.Length - animPropertySuffix.Length);

                string animateTag = material.GetTag(propName + AnimatedTagSuffix, false, "");
                if (string.IsNullOrEmpty(animateTag) || animateTag != "2" || // Property was not renamed.
                    propName.EndsWith("UV", StringComparison.Ordinal) || propName.EndsWith("Pan", StringComparison.Ordinal) || // Skip over stuff that doesn't get renamed.
                    IllegalPropertyRenames.Contains(propName)) continue; // This stuff gets duplicated instead of renamed, should this still get reverted?

                renamedProperties.Add(prop); // Properties fetched now retain their value after switching to the unlocked shader.
            }

            // For some reason when shaders are swapped on a material the RenderType override tag gets completely deleted and render queue set back to -1
            // So these are saved as temp values and reassigned after switching shaders
            string renderType = material.GetTag("RenderType", false, "");
            int renderQueue = material.renderQueue;
            string unlockedMaterialGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(material));
            DetourApplyMaterialPropertyDrawers();
            if (ShaderEditor.Active != null) ShaderEditor.Active.SetShader(originalShader);
            material.shader = originalShader;
            RestoreApplyMaterialPropertyDrawers();
            material.SetOverrideTag("RenderType", renderType);
            material.renderQueue = renderQueue;
            material.shaderKeywords = material.GetTag("OriginalKeywords", false, string.Join(" ", material.shaderKeywords)).Split(' ');

            // Restore stripped textures
            foreach (string tex in material.GetTexturePropertyNames())
            {
                string guid = material.GetTag("_stripped_tex_" + tex, false);
                if (!string.IsNullOrWhiteSpace(guid))
                {
                    material.SetOverrideTag("_stripped_tex_" + tex, "");
                    material.SetTexture(tex, AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(guid)));
                }
            }

            // Restore values from renamed properties
            foreach (MaterialProperty prop in renamedProperties)
            {
                if (prop == null) continue; // Shouldn't happen but included just in case Unity decides otherwise, maybe raise a warning?

                string propName = prop.name.Substring(0, prop.name.Length - animPropertySuffix.Length);
                if (!material.HasProperty(propName))
                {
                    Debug.LogError($"The expected property ({propName}) for renamed property \"{prop.name}\" was not found on the unlocked shader ({originalShader.name}).");

                    continue;
                }

                CopyProperty(material, prop, propName);
            }

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

            return UnlockSuccess.success;
        }

        private static Shader GetOriginalShaderByName(Material material)
        {
            string originalShaderName = material.GetTag(TAG_ORIGINAL_SHADER, false, string.Empty);
            if (string.IsNullOrEmpty(originalShaderName))
            {
                Debug.LogWarning($"[Shader Optimizer] Original shader name not saved to material ({material.name}).");

                return null;
            }

            Shader originalShader = Shader.Find(originalShaderName);
            Debug.LogWarning($"[Shader Optimizer] Original shader name \"{originalShaderName}\" could not be found for material \"{material.name}\".");

            return originalShader;
        }

        private static Shader GetOriginalShaderByGUID(Material material)
        {
            string originalShaderGUID = material.GetTag(TAG_ORIGINAL_SHADER_GUID, false, string.Empty);
            if (string.IsNullOrEmpty(originalShaderGUID))
            {
                Debug.LogWarning($"[Shader Optimizer] Original shader GUID not saved to material ({material.name}).");

                return null;
            }

            Shader originalShader = null;

            string originalShaderPath = AssetDatabase.GUIDToAssetPath(originalShaderGUID);
            if (!string.IsNullOrWhiteSpace(originalShaderPath))
                originalShader = AssetDatabase.LoadAssetAtPath<Shader>(originalShaderPath);

            if (originalShader == null)
                Debug.LogWarning($"[Shader Optimizer] Original shader GUID {originalShaderGUID} could not be found for material \"{material.name}\".");

            return originalShader;
        }

        public static Shader GetOriginalShader(Material material)
        {
            if (material == null) return null;

            // Check for original shader by GUID
            Shader originalShader = GetOriginalShaderByGUID(material);
            if (originalShader != null) return originalShader;

            // Check for original shader by exact name
            originalShader = GetOriginalShaderByName(material);
            if (originalShader != null) return originalShader;

            // Nothing to go by.
            if (material.shader == null)
            {
                Debug.LogWarning($"[Shader Optimizer] Original shader not saved to material ({material.name}) and the current shader is missing.");

                return null;
            }

            // Check for original shader by guessing name
            if (GuessShader(material.shader, out originalShader))
            {
                Debug.LogWarning($"[Shader Optimizer] Original shader not saved to material ({material.name}).\n" +
                    $"Guessed shader name from current shader ({material.shader.name}) to be \"{originalShader.name}\".");
            }
            else
            {
                Debug.LogWarning($"[Shader Optimizer] Original shader not saved to material ({material.name}).\n" +
                    $"Guessing shader name from current shader ({material.shader.name}) failed.");
            }

            return originalShader;
        }
#endregion

        public static void DeleteTags(Material[] materials)
        {
            foreach(Material m in materials)
            {
                var it = new SerializedObject(m).GetIterator();
                while (it.Next(true))
                {
                    if (it.name != "stringTagMap") continue;
                    
                    for (int i = 0; i < it.arraySize; i++)
                    {
                        string tagName = it.GetArrayElementAtIndex(i).displayName;
                        if (!tagName.EndsWith(AnimatedTagSuffix)) continue;

                        m.SetOverrideTag(tagName, "");
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
                !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(m)) && !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(m.shader))
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
                        if (float.TryParse(parts[1], out f) && f != 0)
                        {
                            string name = parts[0].Substring(0, parts[0].Length - AnimatedPropertySuffix.Length);
                            m.SetOverrideTag(name + AnimatedTagSuffix, "" + f);
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

        

        //----VRChat Callback to force Locking on upload

#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
        public class LockMaterialsOnUpload : IVRCSDKPreprocessAvatarCallback
        {
            public int callbackOrder => 100;

            public bool OnPreprocessAvatar(GameObject avatarGameObject)
            {
                if(Application.isPlaying) return true;
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
                if(SetLockedForAllMaterialsInternal(materials, 1, showProgressbar: true, showDialog: PersistentData.Get<bool>("ShowLockInDialog", true), allowCancel: false) == false)
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
                    SetLockedForAllMaterialsInternal(materials, 1, showProgressbar: true, showDialog: PersistentData.Get<bool>("ShowLockInDialog", true), allowCancel: false);
                }
                return true;
            }
        }
#endif
#region Stripping
        const string DidStripUnlockedShadersSessionStateKey = "ShaderOptimizerDidStripUnlockedShaders";
        public class StripUnlockedShadersFromBuild : UnityEditor.Build.IPreprocessShaders
        {
            // Thanks to z3y for this function
            public int callbackOrder => 4;

            public void OnProcessShader(Shader shader, UnityEditor.Rendering.ShaderSnippetData snippet, IList<UnityEditor.Rendering.ShaderCompilerData> data)
            {
                // Don't strip if the user disabled it (developer mode only)
                if(Config.Instance.enableDeveloperMode && Config.Instance.disableUnlockedShaderStrippingOnBuild)
                    return;

                // Strip shaders from the build under the following conditions:
                // - Has the property "shader_is_using_thry_editor", which should be present on all shaders using ThryEditor (even if it's not using the optimizer)
                // - Has the property "_ShaderOptimizerEnabled", indicating the shader is using the optimizer
                // - Doesn't have a name starting with "Hidden/Locked/", indicating the shader is unlocked
                bool shouldStrip = shader.FindPropertyIndex("shader_is_using_thry_editor") >= 0 && shader.FindPropertyIndex("_ShaderOptimizerEnabled") >= 0 && !shader.name.StartsWith("Hidden/Locked/");

                if (shouldStrip)
                {
                    // Try to warn the user if there's an unlocked shader
                    if (!SessionState.GetBool(DidStripUnlockedShadersSessionStateKey, false))
                    {
                        EditorUtility.DisplayDialog("Shader Optimizer: Unlocked Shader", 
                            "An Unlocked shader was found, and will not be included in the build (this will cause pink materials).\n" + 
                            "This shouldn't happen. Make sure all lockable materials are Locked, and try again.\n" +
                            "If it happens again, please report the issue via GitHub or Discord!"
                            , "OK");
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
#endregion

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
}