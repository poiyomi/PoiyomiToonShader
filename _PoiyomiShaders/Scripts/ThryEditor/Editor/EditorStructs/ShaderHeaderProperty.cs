using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class ShaderHeaderProperty : ShaderPart
    {
        public ShaderHeaderProperty(ShaderEditor shaderEditor, MaterialProperty materialProperty, string displayName, int xOffset, string optionsRaw, bool forceOneLine, int propertyIndex) : base(shaderEditor, materialProperty, xOffset, displayName, optionsRaw, propertyIndex)
        {
            // guid is defined as <guid:x*>
            if (displayName.Contains("<guid="))
            {
                int start = displayName.IndexOf("<guid=");
                int end = displayName.IndexOf(">", start);
                string guid = displayName.Substring(start + 6, end - start - 6);
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string replacement = "";
                if (path != null && System.IO.File.Exists(path))
                {
                    replacement = System.IO.File.ReadAllText(path);
                }
                Content.text = displayName.Replace($"<guid={guid}>", replacement);
            }
        }

        protected override void HandleRightClickToggles(bool isInHeader)
        {
        }

        protected override void DrawInternal(GUIContent content, Rect? rect = null, bool useEditorIndent = false, bool isInHeader = false)
        {
            if (rect == null)
            {
                if (Options.texture != null && Options.texture.name != null)
                {
                    //is texutre draw
                    content = new GUIContent(Options.texture.loaded_texture, content.tooltip);
                    int height = Options.texture.height;
                    int width = (int)((float)Options.texture.loaded_texture.width / Options.texture.loaded_texture.height * height);
                    Rect control = EditorGUILayout.GetControlRect(false, height - 18);
                    Rect r = new Rect((control.width - width) / 2, control.y, width, height);
                    GUI.DrawTexture(r, Options.texture.loaded_texture);
                }
            }
            else
            {
                //is text draw
                EditorGUI.LabelField(rect.Value, "<size=16>" + this.Content.text + "</size>", Styles.masterLabel);
                DrawingData.LastGuiObjectRect = rect.Value;
            }
        }

        public override void FindUnusedTextures(List<string> unusedList, bool isEnabled)
        {
        }

        public override void CopyFrom(Material src, bool applyDrawers = true, bool deepCopy = true, HashSet<MaterialProperty.PropType> skipPropertyTypes = null, HashSet<string> skipPropertyNames = null)
        {
            throw new NotImplementedException();
        }

        public override void CopyFrom(ShaderPart src, bool applyDrawers = true, bool deepCopy = true, HashSet<MaterialProperty.PropType> skipPropertyTypes = null, HashSet<string> skipPropertyNames = null)
        {
            throw new NotImplementedException();
        }

        public override void CopyTo(Material[] targets, bool applyDrawers = true, bool deepCopy = true, HashSet<MaterialProperty.PropType> skipPropertyTypes = null, HashSet<string> skipPropertyNames = null)
        {
            throw new NotImplementedException();
        }

        public override void CopyTo(ShaderPart target, bool applyDrawers = true, bool deepCopy = true, HashSet<MaterialProperty.PropType> skipPropertyTypes = null, HashSet<string> skipPropertyNames = null)
        {
            throw new NotImplementedException();
        }

        public override bool Search(string searchTerm, List<ShaderGroup> foundGroups, bool isParentInSearch)
        {
            return true;
        }
    }

}