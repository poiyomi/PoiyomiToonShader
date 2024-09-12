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

        protected override void OnPropertyValueChanged()
        {
            base.OnPropertyValueChanged();
            _isVRAMDirty = true;
        }

        public override void PreDraw()
        {
            DrawingData.CurrentTextureProperty = this;
            this._doCustomDrawLogic = !this._hasDrawer;
            if (this._isVRAMDirty)
            {
                UpdateVRAM();
                _isVRAMDirty = false;
            }
        }

        public override void DrawDefault()
        {
            Rect pos = GUILayoutUtility.GetRect(Content, Styles.vectorPropertyStyle);
            GUILib.ConfigTextureProperty(pos, MaterialProperty, Content, ActiveShaderEditor.Editor, hasFoldoutProperties);
            DrawingData.LastGuiObjectRect = pos;
        }

        public override void TransferFromMaterialAndGroup(Material m, ShaderPart p, bool isTopCall = false, MaterialProperty.PropType[] skipPropertyTypes = null)
        {
            if (ShouldSkipProperty(p.MaterialProperty, skipPropertyTypes)) return;
            if (MaterialProperty.type != p.MaterialProperty.type) return;
            MaterialHelper.CopyMaterialValueFromProperty(MaterialProperty, p.MaterialProperty);
            TransferReferencePropertiesToMaterial(m, p);
        }
        private void TransferReferencePropertiesToMaterial(Material target, ShaderPart p)
        {
            if (p.Options.reference_properties == null || this.Options.reference_properties == null) return;
            for (int i = 0; i < p.Options.reference_properties.Length && i < Options.reference_properties.Length; i++)
            {
                if (ActiveShaderEditor.PropertyDictionary.ContainsKey(this.Options.reference_properties[i]) == false) continue;

                ShaderProperty targetP = ActiveShaderEditor.PropertyDictionary[this.Options.reference_properties[i]];
                ShaderProperty sourceP = p.ActiveShaderEditor.PropertyDictionary[p.Options.reference_properties[i]];
                MaterialHelper.CopyMaterialValueFromProperty(targetP.MaterialProperty, sourceP.MaterialProperty);
            }
        }
    }

}