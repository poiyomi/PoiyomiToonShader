using JetBrains.Annotations;
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
    public class ShaderProperty : ShaderPart
    {
        protected bool _doCustomDrawLogic = false;
        protected bool _doForceIntoOneLine = false;
        protected bool _doDrawTwoFields = false;

        //Done for e.g. Vectors cause they draw in 2 lines for some fucking reasons
        public bool DoCustomHeightOffset { protected set; get; } = false;
        public float CustomHeightOffset { protected set; get; } = 0;

        public string Keyword { private set; get; }

        protected MaterialPropertyDrawer[] _customDecorators;
        protected Rect[] _customDecoratorRects;
        protected bool _hasDrawer = false;

        bool _needsDrawerInitlization = true;

        public ShaderProperty(ShaderEditor shaderEditor, string propertyIdentifier, int xOffset, string displayName, string tooltip, int propertyIndex) : base(propertyIdentifier, xOffset, displayName, tooltip, shaderEditor)
        {
            this.ShaderPropertyIndex = propertyIndex;
        }

        public ShaderProperty(ShaderEditor shaderEditor, MaterialProperty materialProperty, string displayName, int xOffset, string optionsRaw, bool forceOneLine, int propertyIndex) : base(shaderEditor, materialProperty, xOffset, displayName, optionsRaw, propertyIndex)
        {
            this._doCustomDrawLogic = false;
            this._doForceIntoOneLine = forceOneLine;
        }

        protected override void InitOptions()
        {
            base.InitOptions();
            this._doDrawTwoFields = Options.reference_property != null;
        }

        public void SetKeyword(string keyword)
        {
            this.Keyword = keyword;
        }

        [PublicAPI]
        public float FloatValue
        {
            get
            {
                return MaterialProperty.floatValue;
            }
            set
            {
                MaterialProperty.SetNumber(value);
                if (Keyword != null) SetKeyword(ShaderEditor.Active.Materials, MaterialProperty.GetNumber() == 1);
                ExecuteOnValueActions(ShaderEditor.Active.Materials);
                MaterialEditor.ApplyMaterialPropertyDrawers(ShaderEditor.Active.Materials);
            }
        }

        [PublicAPI]
        public Vector4 VectorValue
        {
            get
            {
                return MaterialProperty.vectorValue;
            }
            set
            {
                MaterialProperty.vectorValue = value;
                ExecuteOnValueActions(ShaderEditor.Active.Materials);
                MaterialEditor.ApplyMaterialPropertyDrawers(ShaderEditor.Active.Materials);
            }
        }

        [PublicAPI]
        public Color ColorValue
        {
            get
            {
                return MaterialProperty.colorValue;
            }
            set
            {
                MaterialProperty.colorValue = value;
                ExecuteOnValueActions(ShaderEditor.Active.Materials);
                MaterialEditor.ApplyMaterialPropertyDrawers(ShaderEditor.Active.Materials);
            }
        }

        [PublicAPI]
        public Texture TextureValue
        {
            get
            {
                return MaterialProperty.textureValue;
            }
            set
            {
                MaterialProperty.textureValue = value;
                MaterialEditor.ApplyMaterialPropertyDrawers(ShaderEditor.Active.Materials);
            }
        }

        public override void CopyFromMaterial(Material m, bool isTopCall = false)
        {
            MaterialHelper.CopyPropertyValueFromMaterial(MaterialProperty, m);
            CopyReferencePropertiesFromMaterial(m);

            if (Keyword != null) SetKeyword(ActiveShaderEditor.Materials, m.GetNumber(MaterialProperty) == 1);
            if (IsAnimatable)
            {
                ShaderOptimizer.CopyAnimatedTagFromMaterial(m, MaterialProperty);
            }
            this.IsAnimated = IsAnimatable && ShaderOptimizer.GetAnimatedTag(MaterialProperty) != "";
            this.IsRenaming = IsAnimatable && ShaderOptimizer.GetAnimatedTag(MaterialProperty) == "2";

            ExecuteOnValueActions(ShaderEditor.Active.Materials);

            if (isTopCall) ActiveShaderEditor.ApplyDrawers();
        }

        public override void CopyToMaterial(Material m, bool isTopCall = false, MaterialProperty.PropType[] skipPropertyTypes = null)
        {
            if (ShouldSkipProperty(MaterialProperty, skipPropertyTypes)) return;

            MaterialHelper.CopyPropertyValueToMaterial(MaterialProperty, m);
            CopyReferencePropertiesToMaterial(m);

            if (Keyword != null) SetKeyword(m, MaterialProperty.GetNumber() == 1);
            if (IsAnimatable)
                ShaderOptimizer.CopyAnimatedTagToMaterials(new Material[] { m }, MaterialProperty);

            ExecuteOnValueActions(new Material[] { m });

            if (isTopCall) MaterialEditor.ApplyMaterialPropertyDrawers(m);
        }

        private void SetKeyword(Material[] materials, bool enabled)
        {
            if (enabled) foreach (Material m in materials) m.EnableKeyword(Keyword);
            else foreach (Material m in materials) m.DisableKeyword(Keyword);
        }

        private void SetKeyword(Material m, bool enabled)
        {
            if (enabled) m.EnableKeyword(Keyword);
            else m.DisableKeyword(Keyword);
        }

        public void UpdateKeywordFromValue()
        {
            if (Keyword != null) SetKeyword(ActiveShaderEditor.Materials, MaterialProperty.GetNumber() == 1);
        }

        void InitializeDrawers()
        {
            DrawingData.ResetLastDrawerData();
            DrawingData.IsCollectingProperties = true;
            ShaderEditor.Active.Editor.GetPropertyHeight(MaterialProperty, MaterialProperty.displayName);

            this.IsAnimatable = !DrawingData.LastPropertyDoesntAllowAnimation && IsAnimatable; // &&, so that IsAnimatable can be set to false before InitializeDrawers
            this._hasDrawer = DrawingData.LastPropertyUsedCustomDrawer;

            if (MaterialProperty.type == MaterialProperty.PropType.Vector && _doForceIntoOneLine == false)
            {
                this.DoCustomHeightOffset = !DrawingData.LastPropertyUsedCustomDrawer;
                this.CustomHeightOffset = -EditorGUIUtility.singleLineHeight;
            }
            if (DrawingData.LastPropertyDecorators.Count > 0)
            {
                _customDecorators = DrawingData.LastPropertyDecorators.ToArray();
                _customDecoratorRects = new Rect[DrawingData.LastPropertyDecorators.Count];
            }

            // Animatable Stuff
            bool propHasDuplicate = ShaderEditor.Active.GetMaterialProperty(MaterialProperty.name + "_" + ShaderEditor.Active.RenamedPropertySuffix) != null;
            string tag = null;
            //If prop is og, but is duplicated (locked) dont have it animateable
            if (propHasDuplicate)
            {
                this.IsAnimatable = false;
            }
            else
            {
                //if prop is a duplicated or renamed get og property to check for animted status
                if (MaterialProperty.name.Contains(ShaderEditor.Active.RenamedPropertySuffix))
                {
                    string ogName = MaterialProperty.name.Substring(0, MaterialProperty.name.Length - ShaderEditor.Active.RenamedPropertySuffix.Length - 1);
                    tag = ShaderOptimizer.GetAnimatedTag(MaterialProperty.targets[0] as Material, ogName);
                }
                else
                {
                    tag = ShaderOptimizer.GetAnimatedTag(MaterialProperty);
                }
            }

            this.IsAnimated = IsAnimatable && tag != "";
            this.IsRenaming = IsAnimatable && tag == "2";

            DrawingData.IsCollectingProperties = false;
        }

        public override void DrawInternal(GUIContent content, Rect? rect = null, bool useEditorIndent = false, bool isInHeader = false)
        {
            ActiveShaderEditor.CurrentProperty = this;
            UpdatedMaterialPropertyReference();

            if (_needsDrawerInitlization)
            {
                InitializeDrawers();
                _needsDrawerInitlization = false;
            }

            PreDraw();
            if (ActiveShaderEditor.IsLockedMaterial)
                EditorGUI.BeginDisabledGroup(!(IsAnimatable && (IsAnimated || IsRenaming)) && !IsExemptFromLockedDisabling);

            int oldIndentLevel = EditorGUI.indentLevel;
            if (!useEditorIndent)
                EditorGUI.indentLevel = XOffset + 1;

            if (_customDecorators != null && _doCustomDrawLogic)
            {
                for (int i = 0; i < _customDecorators.Length; i++)
                {
                    _customDecoratorRects[i] = EditorGUILayout.GetControlRect(false, GUILayout.Height(_customDecorators[i].GetPropertyHeight(MaterialProperty, content.text, ActiveShaderEditor.Editor)));
                }
            }

            if (_doCustomDrawLogic)
            {
                DrawDefault();
            }
            else if (_doDrawTwoFields)
            {
                Rect r = GUILayoutUtility.GetRect(content, Styles.vectorPropertyStyle);
                float labelWidth = (r.width - EditorGUIUtility.labelWidth) / 2; ;
                r.width -= labelWidth;
                ActiveShaderEditor.Editor.ShaderProperty(r, this.MaterialProperty, content);

                r.x += r.width;
                r.width = labelWidth;
                float prevLabelW = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 0;
                ActiveShaderEditor.PropertyDictionary[Options.reference_property].Draw(r, new GUIContent());
                EditorGUIUtility.labelWidth = prevLabelW;
            }
            else if (_doForceIntoOneLine)
            {
                ActiveShaderEditor.Editor.ShaderProperty(GUILayoutUtility.GetRect(content, Styles.vectorPropertyStyle), this.MaterialProperty, content);
            }
            else if (DoCustomHeightOffset)
            {
                ActiveShaderEditor.Editor.ShaderProperty(
                    GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, ActiveShaderEditor.Editor.GetPropertyHeight(this.MaterialProperty, content.text) + CustomHeightOffset)
                    , this.MaterialProperty, content);
            }
            else if (rect != null)
            {
                // Custom Drawing for Range, because it doesnt draw correctly if inside the big texture property
                if (!_hasDrawer && MaterialProperty.type == MaterialProperty.PropType.Range)
                {
                    MaterialProperty.floatValue = EditorGUI.Slider(rect.Value, content, MaterialProperty.floatValue, 0, MaterialProperty.rangeLimits.y);
                }
                else
                {
                    ActiveShaderEditor.Editor.ShaderProperty(rect.Value, this.MaterialProperty, content);
                }
            }
            else
            {
                ActiveShaderEditor.Editor.ShaderProperty(this.MaterialProperty, content);
            }

            if (_customDecorators != null && _doCustomDrawLogic)
            {
                for (int i = 0; i < _customDecorators.Length; i++)
                {
                    _customDecorators[i].OnGUI(_customDecoratorRects[i], MaterialProperty, content, ShaderEditor.Active.Editor);
                }
            }

            EditorGUI.indentLevel = oldIndentLevel;
            if (rect == null) DrawingData.LastGuiObjectRect = GUILayoutUtility.GetLastRect();
            else DrawingData.LastGuiObjectRect = rect.Value;
            if (ActiveShaderEditor.IsLockedMaterial)
                EditorGUI.EndDisabledGroup();
        }

        public virtual void PreDraw() { }

        public virtual void DrawDefault() { }

        public override void TransferFromMaterialAndGroup(Material m, ShaderPart p, bool isTopCall = false, MaterialProperty.PropType[] skipPropertyTypes = null)
        {
            if (ShouldSkipProperty(p.MaterialProperty, skipPropertyTypes)) return;
            if (MaterialProperty.type != p.MaterialProperty.type) return;
            MaterialHelper.CopyMaterialValueFromProperty(MaterialProperty, p.MaterialProperty);
            if (Keyword != null) SetKeyword(ActiveShaderEditor.Materials, m.GetNumber(p.MaterialProperty) == 1);
            if (IsAnimatable && p.IsAnimatable)
                ShaderOptimizer.CopyAnimatedTagFromProperty(p.MaterialProperty, MaterialProperty);
            this.IsAnimated = IsAnimatable && ShaderOptimizer.GetAnimatedTag(MaterialProperty) != "";
            this.IsRenaming = IsAnimatable && ShaderOptimizer.GetAnimatedTag(MaterialProperty) == "2";

            if (isTopCall) ActiveShaderEditor.ApplyDrawers();
        }

        public override void FindUnusedTextures(List<string> unusedList, bool isEnabled)
        {
            if (isEnabled && Options.condition_enable != null)
            {
                isEnabled &= Options.condition_enable.Test();
            }
            if (!isEnabled && MaterialProperty != null && MaterialProperty.type == MaterialProperty.PropType.Texture && MaterialProperty.textureValue != null)
            {
                unusedList.Add(MaterialProperty.name);
            }
        }
    }

}