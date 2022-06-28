using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor
{
    public class Presets
    {
        const string TAG_IS_PRESET = "isPreset";
        const string TAG_POSTFIX_IS_PRESET = "_isPreset";
        const string TAG_PRESET_NAME = "presetName";

        static Dictionary<Material, (Material, Material)> appliedPresets = new Dictionary<Material, (Material, Material)>();

        static string[] p_presetNames;
        static Material[] p_presetMaterials;
        static string[] presetNames { get
            {
                if (p_presetNames == null)
                {
                    p_presetMaterials = AssetDatabase.FindAssets("t:material")
                        .Select(g => AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g)))
                        .Where(m => IsPreset(m)).ToArray();
                    p_presetNames = p_presetMaterials.Select(m => m.GetTag(TAG_PRESET_NAME,false,m.name)).Prepend("").ToArray();
                }
                return p_presetNames;
            }
        }

        private static PresetsPopupGUI window;
        public static void PresetGUI(ShaderEditor shaderEditor)
        {
            if(GuiHelper.ButtonWithCursor(Styles.icon_style_presets, "Presets", 25, 25))
            {
                Event.current.Use();
                if (Event.current.button == 0)
                {
                    Vector2 pos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                    pos.x = Mathf.Min(EditorWindow.focusedWindow.position.x + EditorWindow.focusedWindow.position.width - 250, pos.x);
                    pos.y = Mathf.Min(EditorWindow.focusedWindow.position.y + EditorWindow.focusedWindow.position.height - 200, pos.y);

                    if (window != null)
                        window.Close();
                    window = ScriptableObject.CreateInstance<PresetsPopupGUI>();
                    window.position = new Rect(pos.x, pos.y, 250, 200);
                    string[] names = presetNames;
                    window.Init(names, p_presetMaterials, shaderEditor);
                    window.titleContent = new GUIContent("Preset List");
                    window.ShowUtility();
                }
                else
                {
                    EditorUtility.DisplayCustomMenu(GUILayoutUtility.GetLastRect(), presetNames.Select(s => new GUIContent(s)).ToArray(), 0, ApplyQuickPreset, shaderEditor);
                }
            }
        }

        static void ApplyQuickPreset(object userData, string[] options, int selected)
        {
            Apply(p_presetMaterials[selected - 1], userData as ShaderEditor);
        }

        public static void PresetEditorGUI(ShaderEditor shaderEditor)
        {
            if (shaderEditor.IsPresetEditor)
            {
                EditorGUILayout.LabelField(Locale.editor.Get("preset_material_notify"), Styles.greenStyle);
                string name = shaderEditor.Materials[0].GetTag(TAG_PRESET_NAME, false, "");
                EditorGUI.BeginChangeCheck();
                name = EditorGUILayout.TextField(Locale.editor.Get("preset_name"), name);
                if (EditorGUI.EndChangeCheck())
                {
                    shaderEditor.Materials[0].SetOverrideTag(TAG_PRESET_NAME, name);
                    p_presetNames = null;
                }
            }
            if (appliedPresets.ContainsKey(shaderEditor.Materials[0]))
            {
                if(GUILayout.Button(Locale.editor.Get("preset_revert")+appliedPresets[shaderEditor.Materials[0]].Item1.name))
                {
                    Revert(shaderEditor);
                }
            }
        }

        public static void Apply(Material preset, ShaderEditor shaderEditor)
        {
            appliedPresets[shaderEditor.Materials[0]] = (preset, new Material(shaderEditor.Materials[0]));
            foreach (ShaderPart prop in shaderEditor.ShaderParts)
            {
                if (IsPreset(preset, prop))
                {
                    prop.CopyFromMaterial(preset);
                }
            }
            foreach (Material m in shaderEditor.Materials)
                MaterialEditor.ApplyMaterialPropertyDrawers(m);
        }

        static void Revert(ShaderEditor shaderEditor)
        {
            Material key = shaderEditor.Materials[0];
            Material preset = appliedPresets[key].Item1;
            Material prePreset = appliedPresets[key].Item2;
            foreach (ShaderPart prop in shaderEditor.ShaderParts)
            {
                if (IsPreset(preset, prop))
                {
                    prop.CopyFromMaterial(prePreset);
                }
            }
            foreach (Material m in shaderEditor.Materials)
                MaterialEditor.ApplyMaterialPropertyDrawers(m);
            appliedPresets.Remove(key);
        }

        public static void ApplyList(ShaderEditor shaderEditor, Material[] originals, List<Material> presets)
        {
            for(int i=0;i<shaderEditor.Materials.Length && i < originals.Length;i++)
                shaderEditor.Materials[i].CopyPropertiesFromMaterial(originals[i]);
            foreach (Material preset in presets)
            {
                foreach (ShaderPart prop in shaderEditor.ShaderParts)
                {
                    if (IsPreset(preset, prop))
                    {
                        prop.CopyFromMaterial(preset);
                    }
                }
            }
            MaterialEditor.ApplyMaterialPropertyDrawers(shaderEditor.Materials);
            shaderEditor.Reload();
        }

        public static void SetProperty(Material m, ShaderPart prop, bool value)
        {
            if (prop.MaterialProperty != null)   m.SetOverrideTag(prop.MaterialProperty.name + TAG_POSTFIX_IS_PRESET, value ? "true" : "");
            if (prop.PropertyIdentifier != null) m.SetOverrideTag(prop.PropertyIdentifier    + TAG_POSTFIX_IS_PRESET, value ? "true" : "");
        }

        public static bool IsPreset(Material m, ShaderPart prop)
        {
            if (prop.MaterialProperty != null)   return m.GetTag(prop.MaterialProperty.name + TAG_POSTFIX_IS_PRESET, false, "") == "true";
            if (prop.PropertyIdentifier != null) return m.GetTag(prop.PropertyIdentifier    + TAG_POSTFIX_IS_PRESET, false, "") == "true";
            return false;
        }

        public static bool ArePreset(Material[] mats)
        {
            return mats.All(m => IsPreset(m));
        }

        public static bool IsPreset(Material m)
        {
            return m.GetTag(TAG_IS_PRESET, false, "false") == "true";
        }

        [MenuItem("Assets/Thry/Mark as preset")]
        static void MarkAsPreset()
        {
            IEnumerable<Material> mats = Selection.assetGUIDs.Select(g => AssetDatabase.GUIDToAssetPath(g)).
                Where(p => AssetDatabase.GetMainAssetTypeAtPath(p) == typeof(Material)).Select(p => AssetDatabase.LoadAssetAtPath<Material>(p));
            foreach (Material m in mats)
            {
                m.SetOverrideTag(TAG_IS_PRESET, "true");
                if (m.GetTag("presetName", false, "") == "") m.SetOverrideTag("presetName", m.name);
            }
            p_presetNames = null;
        }

        [MenuItem("Assets/Thry/Mark as preset", true)]
        static bool MarkAsPresetValid()
        {
            return Selection.assetGUIDs.Select(g => AssetDatabase.GUIDToAssetPath(g)).
                All(p => AssetDatabase.GetMainAssetTypeAtPath(p) == typeof(Material));
        }

        [MenuItem("Assets/Thry/Remove as preset")]
        static void RemoveAsPreset()
        {
            IEnumerable<Material> mats = Selection.assetGUIDs.Select(g => AssetDatabase.GUIDToAssetPath(g)).
                Where(p => AssetDatabase.GetMainAssetTypeAtPath(p) == typeof(Material)).Select(p => AssetDatabase.LoadAssetAtPath<Material>(p));
            foreach (Material m in mats)
            {
                m.SetOverrideTag(TAG_IS_PRESET, "");
            }
            p_presetNames = null;
        }

        [MenuItem("Assets/Thry/Remove as preset", true)]
        static bool RemoveAsPresetValid()
        {
            return Selection.assetGUIDs.Select(g => AssetDatabase.GUIDToAssetPath(g)).
                All(p => AssetDatabase.GetMainAssetTypeAtPath(p) == typeof(Material));
        }
    }

    public class PresetsPopupGUI : EditorWindow
    {
        class PresetStruct
        {
            public Dictionary<string,PresetStruct> dict;
            string name;
            Material preset;
            bool isOpen = false;
            bool isOn;
            public PresetStruct(string name)
            {
                this.name = name;
                dict = new Dictionary<string, PresetStruct>();
            }

            public PresetStruct GetSubStruct(string name)
            {
                name = name.Trim();
                if (dict.ContainsKey(name) == false)
                    dict.Add(name, new PresetStruct(name));
                return dict[name];
            }
            public void SetPreset(Material m)
            {
                preset = m;
            }
            public void StructGUI(PresetsPopupGUI popupGUI)
            {
                if(preset != null)
                {
                    EditorGUI.BeginChangeCheck();
                    isOn = EditorGUILayout.ToggleLeft(name, isOn);
                    if (EditorGUI.EndChangeCheck())
                    {
                        popupGUI.ToggelPreset(preset, isOn);
                    }
                }
                if(dict.Count > 0)
                {
                    Rect r = GUILayoutUtility.GetRect(new GUIContent(), Styles.dropDownHeader);
                    r.x = EditorGUI.indentLevel * 15;
                    r.width -= r.x;
                    GUI.Box(r, name, Styles.dropDownHeader);
                    if (Event.current.type == EventType.Repaint)
                    {
                        var toggleRect = new Rect(r.x + 4f, r.y + 2f, 13f, 13f);
                        EditorStyles.foldout.Draw(toggleRect, false, false, isOpen, false);
                    }
                    if (Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                    {
                        isOpen = !isOpen;
                        ShaderEditor.Input.Use();
                    }
                    if (isOpen)
                    {
                        EditorGUI.indentLevel += 1;
                        foreach (KeyValuePair<string, PresetStruct> struc in dict)
                        {
                            struc.Value.StructGUI(popupGUI);
                        }
                        EditorGUI.indentLevel -= 1;
                    }
                }
                
            }

            public void Reset()
            {
                isOn = false;
                foreach (KeyValuePair<string, PresetStruct> struc in dict)
                    struc.Value.Reset();
            }
        }

        Material[] beforePreset;
        List<Material> tickedPresets = new List<Material>();
        PresetStruct mainStruct;
        ShaderEditor shaderEditor;
        public void Init(string[] names, Material[] presets, ShaderEditor shaderEditor)
        {
            this.shaderEditor = shaderEditor;
            this.beforePreset = shaderEditor.Materials.Select(m => new Material(m)).ToArray();
            mainStruct = new PresetStruct("");
            backgroundTextrure = new Texture2D(1,1);
            if (EditorGUIUtility.isProSkin) backgroundTextrure.SetPixel(0, 0, new Color(0.18f, 0.18f, 0.18f, 1));
            else backgroundTextrure.SetPixel(0, 0, new Color(0.9f, 0.9f, 0.9f, 1));
            backgroundTextrure.Apply();
            for (int i = 1; i < names.Length; i++)
            {
                string[] path = names[i].Split('/');
                PresetStruct addUnder = mainStruct;
                for (int j=0;j<path.Length; j++)
                {
                    addUnder = addUnder.GetSubStruct(path[j]);
                }
                addUnder.SetPreset(presets[i-1]);
            }
        }

        void ToggelPreset(Material m, bool on)
        {
            if (tickedPresets.Contains(m) && !on) tickedPresets.Remove(m);
            if (!tickedPresets.Contains(m) && on) tickedPresets.Add(m);
            Presets.ApplyList(shaderEditor, beforePreset, tickedPresets);
        }

        static Texture2D backgroundTextrure;

        Vector2 scroll;
        bool _save;
        void OnGUI()
        {
            if (mainStruct == null) { this.Close(); return; }

            GUILayout.BeginHorizontal();
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(position.height - 55));

            GUILayoutUtility.GetRect(10, 5);
            TopStructGUI();

            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();

            if (GUI.Button(new Rect(5, this.position.height - 35, this.position.width / 2 - 5, 30), "Apply"))
            {
                _save = true;
                this.Close();
            }
                
            if (GUI.Button(new Rect(this.position.width / 2, this.position.height - 35, this.position.width / 2 - 5, 30), "Discard"))
            {
                Revert();
            }
        }
        private void OnDestroy()
        {
            if (!_save)
            {
                Revert();
            }
        }

        void TopStructGUI()
        {
            foreach (KeyValuePair<string, PresetStruct> struc in mainStruct.dict)
            {
                struc.Value.StructGUI(this);
            }
        }

        void Revert()
        {
            for (int i = 0; i < shaderEditor.Materials.Length; i++)
            {
                shaderEditor.Materials[i].CopyPropertiesFromMaterial(beforePreset[i]);
                MaterialEditor.ApplyMaterialPropertyDrawers(shaderEditor.Materials[i]);
            }
            mainStruct.Reset();
            shaderEditor.Repaint();
        }
    }
}