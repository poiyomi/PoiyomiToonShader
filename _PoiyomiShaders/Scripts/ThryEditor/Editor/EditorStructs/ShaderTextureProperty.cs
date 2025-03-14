using Thry.ThryEditor.Helpers;
using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor
{
    public class ShaderTextureProperty : ShaderProperty
    {
        public bool showFoldoutProperties = false;
        public bool hasFoldoutProperties = false;
        public bool hasScaleOffset = false;
        public string VRAMString = "";
        bool _isVRAMDirty = true;

        public ShaderTextureProperty(ShaderEditor shaderEditor, MaterialProperty materialProperty, string displayName, int xOffset, string optionsRaw, bool hasScaleOffset, bool forceThryUI, int property_index) : base(shaderEditor, materialProperty, displayName, xOffset, optionsRaw, false, property_index)
        {
            _doCustomDrawLogic = forceThryUI;
            this.hasScaleOffset = hasScaleOffset;
            PropertyValueChanged += (PropertyValueEventArgs args) => _isVRAMDirty = true;
        }

        protected override void InitOptions()
        {
            base.InitOptions();
            this.hasFoldoutProperties = hasScaleOffset || DoReferencePropertiesExist;
        }

        void UpdateVRAM()
        {
            if (MaterialProperty.textureValue != null)
            {
                var details = TextureHelper.VRAM.CalcSize(MaterialProperty.textureValue);
                this.VRAMString = $"{TextureHelper.VRAM.ToByteString(details.size)}";
            }
            else
            {
                VRAMString = null;
            }
        }

        protected override void PreDraw()
        {
            DrawingData.CurrentTextureProperty = this;
            this._doCustomDrawLogic = _drawer == null;
            if (this._isVRAMDirty)
            {
                UpdateVRAM();
                _isVRAMDirty = false;
            }
        }

        protected override void DrawDefault()
        {
            Rect pos = GUILayoutUtility.GetRect(Content, Styles.vectorPropertyStyle);
            GUILib.ConfigTextureProperty(pos, MaterialProperty, Content, MyMaterialEditor, hasFoldoutProperties);
            DrawingData.LastGuiObjectRect = pos;
        }
    }

}