using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class TextureArrayDrawer : MaterialPropertyDrawer
    {
        private string framesProperty;

        public TextureArrayDrawer() { }

        public TextureArrayDrawer(string framesProperty)
        {
            this.framesProperty = framesProperty;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            ShaderProperty shaderProperty = (ShaderProperty)ShaderEditor.Active.CurrentProperty;
            GUILib.ConfigTextureProperty(position, prop, label, editor, true, true);

            if ((ShaderEditor.Input.is_drag_drop_event) && position.Contains(ShaderEditor.Input.mouse_position))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (ShaderEditor.Input.is_drop_event)
                {
                    DragAndDrop.AcceptDrag();
                    HanldeDropEvent(prop, shaderProperty);
                }
            }
            if (ShaderEditor.Active.IsFirstCall)
                ShaderEditor.Active.TextureArrayProperties.Add(shaderProperty);
        }

        public void HanldeDropEvent(MaterialProperty prop, ShaderProperty shaderProperty)
        {
            string[] paths = DragAndDrop.paths;
            Texture2DArray tex;
            if (AssetDatabase.GetMainAssetTypeAtPath(paths[0]) != typeof(Texture2DArray))
                tex = Converter.PathsToTexture2DArray(paths);
            else
                tex = AssetDatabase.LoadAssetAtPath<Texture2DArray>(paths[0]);
            prop.textureValue = tex;
            UpdateFramesProperty(prop, shaderProperty, tex);
            EditorGUIUtility.ExitGUI();
        }

        private void UpdateFramesProperty(MaterialProperty prop, ShaderProperty shaderProperty, Texture2DArray tex)
        {
            if (framesProperty == null)
                framesProperty = shaderProperty.Options.reference_property;

            if (framesProperty != null)
            {
                if (ShaderEditor.Active.PropertyDictionary.ContainsKey(framesProperty))
                    ShaderEditor.Active.PropertyDictionary[framesProperty].MaterialProperty.SetNumber(tex.depth);
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            ShaderProperty.RegisterDrawer(this);
            return base.GetPropertyHeight(prop, label, editor);
        }
    }

}