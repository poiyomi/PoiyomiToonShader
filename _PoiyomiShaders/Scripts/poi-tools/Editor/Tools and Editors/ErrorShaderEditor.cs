using System;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Poi.Tools
{
    // CanEditMultipleObjects: without it Unity falls back to its plain MaterialEditor whenever more than one
    // material is selected, which meant neither the cheap Awake nor the drag throttle applied to multi-edits -
    // the case that suffers most. The recovery UI keys off the first target, as it always did; the normal
    // path is MaterialEditor's own multi-edit behaviour.
    [CustomEditor(typeof(Material)), CanEditMultipleObjects]
    public class ErrorShaderEditor : MaterialEditor
    {
        const string PoiyomiProUrl = "https://www.poiyomi.com/download#poiyomi-pro";
        const string ErrorShaderName = "Hidden/InternalErrorShader";
        const string LockedMaterialText = "Generated shader for this locked Poiyomi material is missing.\nYou can unlock this material by clicking below.";
        const string LockedMaterialText_Pro = "Generated shader for this locked Poiyomi Pro material is missing.\nYou can unlock this material by clicking below.";
        const string LockedMaterialText_ProMissing = "Generated shader for this locked Poiyomi Pro material is missing.\nTo unlock this material you need Poiyomi Pro which is missing from your project.";
        const string UnlockedMaterialText_ProMissing = "Poiyomi Pro material detected.\nTo use this material you need Poiyomi Pro which is missing from your project.";
        const string UnlockedMaterialText = "Poiyomi material detected.\nNot sure what happened but you can try to switch the shader to the latest toon below.";

        string originalShader;
        bool isErrorShader;
        bool isPoiyomiMaterial;
        bool isProMaterial;
        bool isLockedMaterial;
        bool isLegacy;

        static bool? ProjectHasPro
        {
            get
            {
                if (_projectHasPro == null)
                {
                    _projectHasPro = ShaderUtil.GetAllShaderInfo()
                        .Select(info => info.name)
                        .Where(name => !name.StartsWith("Hidden/"))
                        .Any(name => name.Contains("Poiyomi Pro"));
                }
                return (bool)_projectHasPro;
            }
        }
        static bool? _projectHasPro;

        Material targetMaterial;

        public override void Awake()
        {
            base.Awake();
            Initialize();
        }

        void Initialize()
        {
            targetMaterial = target as Material;
            if(targetMaterial == null || targetMaterial.shader == null)
                return;

            isErrorShader = targetMaterial.shader.name == ErrorShaderName;

            // Show the upgrade prompt only when a removed-version (pre-9.3) Poiyomi material's shader is genuinely
            // MISSING - on the error shader (unlocked, even without an OriginalShader tag), or a locked material whose
            // source shader no longer resolves. A legacy material still rendering on a present (e.g. restored) pre-9.3
            // shader is left alone here - it still works and can be upgraded from the Poiyomi menu.
            isLegacy = PoiLegacyUpgradeBridge.IsLegacyShaderMissing != null && PoiLegacyUpgradeBridge.IsLegacyShaderMissing(targetMaterial);

            // Tag-based fields, used for the non-legacy error-shader fallback (a locked 9.3/10.0 shader that was deleted).
            originalShader = targetMaterial.GetTag("OriginalShader", false);
            isPoiyomiMaterial = !string.IsNullOrWhiteSpace(originalShader);
            //Unity 2019 doesn't have .Contains(string, StringComparison) so using .IndexOf() instead
            isProMaterial = isPoiyomiMaterial && originalShader.IndexOf("Poiyomi Pro", System.StringComparison.CurrentCultureIgnoreCase) != -1;

            isLockedMaterial = false;
            // Only the error-shader recovery UI reads this, and finding it means walking every saved float through
            // SerializedProperty - ~19 ms and ~1 MB on a Poiyomi material. Awake runs every time Unity creates a
            // material editor, which it does for each thumbnail it regenerates, so a slider drag paid this per frame.
            if (!isErrorShader) return;
            SerializedProperty floatProps = serializedObject.FindProperty("m_SavedProperties")?.FindPropertyRelative("m_Floats");
            if (floatProps != null)
            {
                for (int i = 0; i < floatProps.arraySize; i++)
                {
                    var prop = floatProps.GetArrayElementAtIndex(i);
                    if (prop.displayName == "_ShaderOptimizerEnabled")
                    {
                        isLockedMaterial = Convert.ToBoolean(prop.FindPropertyRelative("second").floatValue);
                        break;
                    }
                }
            }
        }

        // --- Drag throttle -------------------------------------------------------------------------------
        //
        // The editor runs a full Layout pass and a full event pass of the whole material inspector for every
        // mouse event the OS delivers while a slider is dragged, and it does not coalesce them; a fast mouse
        // queues dozens per frame. MaterialEditor.OnInspectorGUI alone costs about 5 ms per pass on a Poiyomi
        // material (it fetches all ~4900 properties and updates the serialized object) before ThryEditor draws
        // anything, so the only place to skip the whole pair is here, above the base call.
        //
        // One drag pair per Repaint is processed in full; the rest reserve the height of the last full Repaint,
        // so the inspector's container never re-measures and nothing jumps. The pass after a placeholder Layout
        // is a placeholder too, which is what IMGUI requires of a Layout/event pair, so a misjudged Repaint costs
        // a dropped frame rather than an exception. ThryEditor applies the same one-per-repaint rule to its own
        // event passes, so the two never disagree about which drag gets processed.
        float _contentHeight = -1;
        bool _layoutWasPlaceholder;
        bool _dragPairSinceRepaint;
        double _lastFullDragPairTime;

        public override void OnInspectorGUI()
        {
            if (BeginThrottledPass()) return;
            Rect content = EditorGUILayout.BeginVertical();
            DrawInspector();
            EditorGUILayout.EndVertical();
            if (Event.current.type == EventType.Repaint) _contentHeight = content.height;
        }

        bool BeginThrottledPass()
        {
            Event e = Event.current;
            if (e.type == EventType.Layout)
            {
                _layoutWasPlaceholder = false;
                if (GUIUtility.hotControl == 0 || _contentHeight <= 0 || !Thry.ShaderEditor.IsDragEventIncoming())
                    return false;
                double now = EditorApplication.timeSinceStartup;
                if (_dragPairSinceRepaint && now - _lastFullDragPairTime < Thry.ShaderEditor.DragPassValveSeconds)
                {
                    _layoutWasPlaceholder = true;
                    DrawPlaceholder();
                    return true;
                }
                _dragPairSinceRepaint = true;
                _lastFullDragPairTime = now;
                return false;
            }
            if (e.type == EventType.Repaint) _dragPairSinceRepaint = false;
            if (!_layoutWasPlaceholder) return false;
            Thry.ShaderEditor.ConsumeIncomingDragEvent();
            DrawPlaceholder();
            return true;
        }

        void DrawPlaceholder()
        {
            EditorGUILayout.BeginVertical();
            GUILayoutUtility.GetRect(0f, _contentHeight, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndVertical();
        }

        void DrawInspector()
        {
            // Legacy (pre-9.3) material whose shader is missing/removed - offer the translating upgrade to 9.3. This
            // catches what the old tag/error-shader gating missed: unlocked-never-locked materials on the error shader
            // (no OriginalShader tag), AND locked materials still rendering on a baked shader whose source was removed.
            if (isLegacy)
            {
                bool upgraded = false;
                EditorGUILayout.BeginVertical();

                if (isProMaterial && !(bool)ProjectHasPro)
                {
                    // Pro material, but Poiyomi Pro isn't installed - there's no 9.3 Pro shader to upgrade onto.
                    EditorGUILayout.HelpBox(UnlockedMaterialText_ProMissing, MessageType.Warning);
                    if (GUILayout.Button("More info"))
                        Application.OpenURL(PoiyomiProUrl);
                }
                else
                {
                    EditorGUILayout.HelpBox("This legacy Poiyomi material's shader has been removed. Upgrade this to Poiyomi 9.3 by pressing the button below - your settings will be translated across. After doing so, you can translate it to 10.0 from the Poiyomi menu at your own discretion.", MessageType.Warning);
                    if (GUILayout.Button("Translate to Poiyomi 9.3"))
                    {
                        Undo.RegisterCompleteObjectUndo(targetMaterial, $"Translate {targetMaterial.name} to Poiyomi 9.3");
                        PoiLegacyUpgradeBridge.UpgradeToNine3?.Invoke(targetMaterial);
                        Initialize();
                        upgraded = true;
                    }
                }

                EditorGUILayout.EndVertical();

                // A locked material still renders on its baked shader - show its normal inspector below so it stays
                // usable while the upgrade prompt sits on top. (An error-shader material has nothing useful to draw.)
                if (!upgraded && !isErrorShader)
                {
                    EditorGUILayout.Space();
                    base.OnInspectorGUI();
                }
                return;
            }

            // Non-legacy: an error-shader Poiyomi material that isn't pre-9.3 (e.g. a locked 9.3/10.0 material whose
            // generated shader was deleted) keeps the original recovery UI; anything else draws normally.
            if (!isErrorShader || !isPoiyomiMaterial)
            {
                base.OnInspectorGUI();
                return;
            }

            EditorGUILayout.BeginVertical();

            if (isLockedMaterial)
            {
                if (isProMaterial && !(bool)ProjectHasPro)
                {
                    EditorGUILayout.HelpBox(LockedMaterialText_ProMissing, MessageType.Warning);
                    if (GUILayout.Button("More info"))
                        Application.OpenURL(PoiyomiProUrl);
                }
                else
                {
                    EditorGUILayout.HelpBox(isProMaterial ? LockedMaterialText_Pro : LockedMaterialText, MessageType.Warning);
                    if (GUILayout.Button("Unlock Material"))
                        SwitchShader(originalShader);
                }
            }
            else
            {
                if (isProMaterial && !(bool)ProjectHasPro)
                {
                    EditorGUILayout.HelpBox(UnlockedMaterialText_ProMissing, MessageType.Warning);
                    if(GUILayout.Button("More info"))
                        Application.OpenURL(PoiyomiProUrl);
                }
                else if (!isProMaterial)
                {
                    EditorGUILayout.HelpBox(UnlockedMaterialText, MessageType.Warning);
                    string URP = PoiHelpers.IsURP() ? "URP" : "";
                    if(GUILayout.Button("Switch to latest Toon"))
                        SwitchShader($".poiyomi/Poiyomi Toon {URP}".Trim());
                }
            }

            EditorGUILayout.EndVertical();
        }

        void SwitchShader(string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            if (!shader)
            {
                Debug.LogError($"Couldn't find shader {shaderName} in the project.");
                return;
            }

            serializedObject.FindProperty("m_Shader").objectReferenceValue = shader;
            serializedObject.ApplyModifiedProperties();
            Initialize();
        }
    }
}
