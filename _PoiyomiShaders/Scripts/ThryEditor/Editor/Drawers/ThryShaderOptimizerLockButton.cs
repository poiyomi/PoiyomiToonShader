using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class ThryShaderOptimizerLockButtonDrawer : MaterialPropertyDrawer
    {
        public override void OnGUI(Rect position, MaterialProperty shaderOptimizer, string label, MaterialEditor materialEditor)
        {
            Material material = shaderOptimizer.targets[0] as Material;
            Shader shader = material.shader;
            // The GetPropertyDefaultFloatValue is changed from 0 to 1 when the shader is locked in
            bool isLocked = shader.name.StartsWith("Hidden/Locked/") ||
                (shader.name.StartsWith("Hidden/") && material.GetTag("OriginalShader", false, "") != "" && shader.GetPropertyDefaultFloatValue(shader.FindPropertyIndex(shaderOptimizer.name)) == 1);
            //this will make sure the button is unlocked if you manually swap to an unlocked shader
            //shaders that have the ability to be locked shouldnt really be hidden themself. at least it wouldnt make too much sense
            if (shaderOptimizer.hasMixedValue == false && shaderOptimizer.GetNumber() == 1 && isLocked == false)
            {
                shaderOptimizer.SetNumber(0);
            }
            else if (shaderOptimizer.hasMixedValue == false && shaderOptimizer.GetNumber() == 0 && isLocked)
            {
                shaderOptimizer.SetNumber(1);
            }

            bool disabled = false;
#if UNITY_2022_1_OR_NEWER
            disabled |= ShaderEditor.Active.Materials[0].isVariant;
#endif
            EditorGUI.BeginDisabledGroup(disabled); // for variant materials

            // Theoretically this shouldn't ever happen since locked in materials have different shaders.
            // But in a case where the material property says its locked in but the material really isn't, this
            // will display and allow users to fix the property/lock in
            ShaderEditor.Active.IsLockedMaterial = shaderOptimizer.GetNumber() == 1;
            if (shaderOptimizer.hasMixedValue)
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.Button(EditorLocale.editor.Get("lockin_button_multi").ReplaceVariables(materialEditor.targets.Length));
                if (EditorGUI.EndChangeCheck())
                {
                    SaveChangeStack();
                    ShaderOptimizer.SetLockedForAllMaterials(shaderOptimizer.targets.Select(t => t as Material), shaderOptimizer.floatValue == 1 ? 0 : 1, true, false, false, shaderOptimizer);
                    RestoreChangeStack();
                }
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                if (shaderOptimizer.GetNumber() == 0)
                {
                    if (materialEditor.targets.Length == 1)
                        RectifiedLayout.Button(EditorLocale.editor.Get("lockin_button_single"));
                    else RectifiedLayout.Button(EditorLocale.editor.Get("lockin_button_multi").ReplaceVariables(materialEditor.targets.Length));
                }
                else
                {
                    if (materialEditor.targets.Length == 1)
                        RectifiedLayout.Button(EditorLocale.editor.Get("unlock_button_single"));
                    else RectifiedLayout.Button(EditorLocale.editor.Get("unlock_button_multi").ReplaceVariables(materialEditor.targets.Length));
                }
                if (EditorGUI.EndChangeCheck())
                {
                    SaveChangeStack();
                    ShaderOptimizer.SetLockedForAllMaterials(shaderOptimizer.targets.Select(t => t as Material), shaderOptimizer.GetNumber() == 1 ? 0 : 1, true, false, false, shaderOptimizer);
                    RestoreChangeStack();
                }
            }
            if (Config.Singleton.allowCustomLockingRenaming || ShaderEditor.Active.HasCustomRenameSuffix)
            {
                EditorGUI.BeginDisabledGroup(!Config.Singleton.allowCustomLockingRenaming || ShaderEditor.Active.IsLockedMaterial);
                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = ShaderEditor.Active.HasMixedCustomPropertySuffix;
                ShaderEditor.Active.RenamedPropertySuffix = EditorGUILayout.TextField("Locked property suffix: ", ShaderEditor.Active.RenamedPropertySuffix);
                if (EditorGUI.EndChangeCheck())
                {
                    // Make sure suffix that is saved is valid
                    ShaderEditor.Active.RenamedPropertySuffix = ShaderOptimizer.CleanStringForPropertyNames(ShaderEditor.Active.RenamedPropertySuffix.Replace(" ", "_"));
                    foreach (Material m in ShaderEditor.Active.Materials)
                        m.SetOverrideTag("thry_rename_suffix", ShaderEditor.Active.RenamedPropertySuffix);
                    if (ShaderEditor.Active.RenamedPropertySuffix == "")
                        ShaderEditor.Active.RenamedPropertySuffix = ShaderOptimizer.GetRenamedPropertySuffix(ShaderEditor.Active.Materials[0]);
                    ShaderEditor.Active.HasCustomRenameSuffix = ShaderOptimizer.HasCustomRenameSuffix(ShaderEditor.Active.Materials[0]);
                }
                if (!Config.Singleton.allowCustomLockingRenaming)
                {
                    EditorGUILayout.HelpBox("This feature is disabled in the config file. You can enable it by setting allowCustomLockingRenaming to true.", MessageType.Info);
                }
                EditorGUI.EndDisabledGroup();
            }

            EditorGUI.EndDisabledGroup(); // for variant materials
        }

        //This code purly exists cause Unity 2019 is a piece of shit that looses it's internal change stack on locking CAUSE FUCK IF I KNOW
        static System.Reflection.FieldInfo changeStack = typeof(EditorGUI).GetField("s_ChangedStack", BindingFlags.Static | BindingFlags.NonPublic);
        static int preLockStackSize = 0;
        private static void SaveChangeStack()
        {
            if (changeStack != null)
            {
                Stack<bool> stack = (Stack<bool>)changeStack.GetValue(null);
                if (stack != null)
                {
                    preLockStackSize = stack.Count();
                }
            }
        }

        private static void RestoreChangeStack()
        {
            if (changeStack != null)
            {
                Stack<bool> stack = (Stack<bool>)changeStack.GetValue(null);
                if (stack != null)
                {
                    int postLockStackSize = stack.Count();
                    //Restore change stack from before lock / unlocking
                    for (int i = postLockStackSize; i < preLockStackSize; i++)
                    {
                        EditorGUI.BeginChangeCheck();
                    }
                }
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            ShaderProperty.RegisterDrawer(this);
            ShaderProperty.DisallowAnimation();
            ShaderEditor.Active.DoUseShaderOptimizer = true;
            return -2;
        }
    }

}