using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class CRect
    {
        public Rect r;
        public CRect(Rect r)
        {
            this.r = r;
        }
    }

    public class InputEvent
    {
        public bool HadMouseDownRepaint;
        public bool HadMouseDown;
        public bool MouseClick;
        public bool MouseLeftClick;

        public bool is_alt_down;

        public bool is_drag_drop_event;
        public bool is_drop_event;

        public Vector2 mouse_position;

        public void Use()
        {
            HadMouseDownRepaint = false;
            HadMouseDown = false;
            MouseClick = false;
            MouseLeftClick = false;
        }
    }

    public abstract class ShaderPart
    {
        public ShaderEditor shaderEditor;

        public int xOffset = 0;
        public GUIContent content;
        public MaterialProperty materialProperty;
        public System.Object property_data = null;
        public PropertyOptions options;
        public bool reference_properties_exist = false;
        public bool reference_property_exists = false;
        public bool is_hidden = false;
        public bool is_animated = false;
        public bool is_animatable = false;
        public bool is_renaming = false;
        public bool exempt_from_locked_disabling = false;

        public BetterTooltips.Tooltip tooltip;

        public bool has_searchedFor = true; //used for property search

        public ShaderPart(ShaderEditor shaderEditor, MaterialProperty prop, int xOffset, string displayName, PropertyOptions options)
        {
            this.shaderEditor = shaderEditor;
            this.materialProperty = prop;
            this.xOffset = xOffset;
            this.options = options;
            this.content = new GUIContent(displayName);
            this.tooltip = new BetterTooltips.Tooltip(options.tooltip);
            this.reference_properties_exist = options.reference_properties != null && options.reference_properties.Length > 0;
            this.reference_property_exists = options.reference_property != null;

            if (prop == null)
                return;
            bool propHasDuplicate = ShaderEditor.active.GetMaterialProperty(prop.name + "_" + ShaderEditor.active.animPropertySuffix) != null;
            string tag = null;
            //If prop is og, but is duplicated (locked) dont have it animateable
            if (propHasDuplicate)
            {
                this.is_animatable = false;
            }
            else
            {
                //if prop is a duplicated or renamed get og property to check for animted status
                if (prop.name.Contains(ShaderEditor.active.animPropertySuffix))
                {
                    string ogName = prop.name.Substring(0, prop.name.Length - ShaderEditor.active.animPropertySuffix.Length - 1);
                    tag = ShaderOptimizer.GetAnimatedTag(materialProperty.targets[0] as Material, ogName);
                }
                else
                {
                    tag = ShaderOptimizer.GetAnimatedTag(materialProperty);
                }
                this.is_animatable = true;
            }
            
            this.is_animated = is_animatable && tag != "";
            this.is_renaming = is_animatable && tag == "2";
        }

        public void SetReferenceProperty(string s)
        {
            options.reference_property = s;
            this.reference_property_exists = options.reference_property != null;
        }

        public void SetReferenceProperties(string[] properties)
        {
            options.reference_properties = properties;
            this.reference_properties_exist = options.reference_properties != null && options.reference_properties.Length > 0;
        }

        public void SetTooltip(string tooltip)
        {
            this.tooltip.SetText(tooltip);
        }

        public abstract void DrawInternal(GUIContent content, CRect rect = null, bool useEditorIndent = false, bool isInHeader = false);
        public abstract void CopyFromMaterial(Material m);
        public abstract void CopyToMaterial(Material m);

        public abstract void TransferFromMaterialAndGroup(Material m, ShaderPart g);

        public void Draw(CRect rect = null, GUIContent content = null, bool useEditorIndent = false, bool isInHeader = false)
        {
            if (ShaderEditor.active.show_search_bar && !has_searchedFor)
                return;
            if (HeaderHider.IsHeaderHidden(this))
                return;
            bool addDisableGroup = options.condition_enable != null && DrawingData.is_enabled;
            if (addDisableGroup)
            {
                DrawingData.is_enabled = options.condition_enable.Test();
                EditorGUI.BeginDisabledGroup(!DrawingData.is_enabled);
            }
            if (options.condition_show.Test())
            {
                PerformDraw(content, rect, useEditorIndent, isInHeader);
            }
            if (addDisableGroup)
            {
                DrawingData.is_enabled = true;
                EditorGUI.EndDisabledGroup();
            }
        }

        public void HandleKajAnimatable()
        {
            Rect lastRect = GUILayoutUtility.GetLastRect();
            if (ShaderEditor.active.isLockedMaterial == false && Event.current.isMouse && Event.current.button == 1 && lastRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.control && Config.Singleton.renameAnimatedProps)
                {
                    if (!is_animated)
                    {
                        is_animated = true;
                    }

                    if (is_animated)
                    {
                        is_renaming = !is_renaming;
                    }
                }
                else
                {
                    is_animated = !is_animated;
                }
                ShaderOptimizer.SetAnimatedTag(materialProperty, is_animated ? (is_renaming ? "2" : "1") : "");
                ShaderEditor.Repaint();
            }
            if (is_animated)
            {
                Rect r = new Rect(8, lastRect.y + 2, 16, 16);
                GUI.DrawTexture(r, is_renaming ? Styles.texture_animated_renamed : Styles.texture_animated, ScaleMode.StretchToFill, true);
            }
        }

        private void PerformDraw(GUIContent content, CRect rect, bool useEditorIndent, bool isInHeader = false)
        {
            if (content == null)
                content = this.content;
            EditorGUI.BeginChangeCheck();
            DrawInternal(content, rect, useEditorIndent, isInHeader);

            DrawingData.tooltipCheckRect = DrawingData.lastGuiObjectRect;
            DrawingData.tooltipCheckRect.width = EditorGUIUtility.labelWidth;
            if (this is TextureProperty == false) tooltip.ConditionalDraw(DrawingData.tooltipCheckRect);

            if (EditorGUI.EndChangeCheck())
            {
                if (options.on_value_actions != null)
                {
                    foreach (PropertyValueAction action in options.on_value_actions)
                    {
                        action.Execute(materialProperty);
                    }
                }
            }
            Helper.testAltClick(DrawingData.lastGuiObjectRect, this);
        }
    }

    public class ShaderGroup : ShaderPart
    {
        public List<ShaderPart> parts = new List<ShaderPart>();

        public ShaderGroup(ShaderEditor shaderEditor) : base(shaderEditor, null, 0, "", new PropertyOptions())
        {

        }

        public ShaderGroup(ShaderEditor shaderEditor, PropertyOptions options) : base(shaderEditor, null, 0, "", new PropertyOptions())
        {
            this.options = options;
        }

        public ShaderGroup(ShaderEditor shaderEditor, MaterialProperty prop, MaterialEditor materialEditor, string displayName, int xOffset, PropertyOptions options) : base(shaderEditor, prop, xOffset, displayName, options)
        {

        }

        public void addPart(ShaderPart part)
        {
            parts.Add(part);
        }

        public override void CopyFromMaterial(Material m)
        {
            if (options.reference_property != null)
                ShaderEditor.active.propertyDictionary[options.reference_property].CopyFromMaterial(m);
            foreach (ShaderPart p in parts)
                p.CopyFromMaterial(m);
        }

        public override void CopyToMaterial(Material m)
        {
            if (options.reference_property != null)
                ShaderEditor.active.propertyDictionary[options.reference_property].CopyToMaterial(m);
            foreach (ShaderPart p in parts)
                p.CopyToMaterial(m);
        }

        public override void DrawInternal(GUIContent content, CRect rect = null, bool useEditorIndent = false, bool isInHeader = false)
        {
            foreach (ShaderPart part in parts)
            {
                part.Draw();
            }
        }

        public override void TransferFromMaterialAndGroup(Material m, ShaderPart p)
        {
            if (p is ShaderGroup == false) return;
            ShaderGroup group = p as ShaderGroup;
            if (options.reference_property != null && group.options.reference_property != null)
                ShaderEditor.active.propertyDictionary[options.reference_property].TransferFromMaterialAndGroup(m, group.shaderEditor.propertyDictionary[group.options.reference_property]);
            for(int i=0;i<group.parts.Count && i < parts.Count; i++)
            {
                parts[i].TransferFromMaterialAndGroup(m, group.parts[i]);
            }
        }
    }

    public class ShaderHeader : ShaderGroup
    {
        private ThryHeaderDrawer headerDrawer;
        private bool isLegacy;

        public ShaderHeader(ShaderEditor shaderEditor) : base(shaderEditor)
        {
            this.headerDrawer = new ThryHeaderDrawer();
        }

        public ShaderHeader(ShaderEditor shaderEditor, MaterialProperty prop, MaterialEditor materialEditor, string displayName, int xOffset, PropertyOptions options) : base(shaderEditor, prop, materialEditor, displayName, xOffset, options)
        {
            DrawingData.ResetLastDrawerData();
            materialEditor.GetPropertyHeight(prop);
            if(DrawingData.lastPropertyDrawerType == DrawerType.Header)
            {
                //new header setup with drawer
                this.headerDrawer = DrawingData.lastPropertyDrawer as ThryHeaderDrawer;
                options.is_hideable |= headerDrawer.isHideable;
            }
            else
            {
                //legacy setup with HideInInspector
                this.headerDrawer = new ThryHeaderDrawer();
                isLegacy = true;
            }
            this.headerDrawer.xOffset = xOffset;
        }

        public string GetEndProperty()
        {
            return headerDrawer.GetEndProperty();
        }

        public override void DrawInternal(GUIContent content, CRect rect = null, bool useEditorIndent = false, bool isInHeader = false)
        {
            ShaderEditor.active.currentProperty = this;
            EditorGUI.BeginChangeCheck();
            Rect position = GUILayoutUtility.GetRect(content, Styles.dropDownHeader);
            if (isLegacy) headerDrawer.OnGUI(position, this.materialProperty, content, ShaderEditor.active.editor);
            else ShaderEditor.active.editor.ShaderProperty(position, this.materialProperty, content);
            Rect headerRect = DrawingData.lastGuiObjectHeaderRect;
            if (this.headerDrawer.is_expanded)
            {
                EditorGUILayout.Space();
                foreach (ShaderPart part in parts)
                {
                    part.Draw();
                }
                EditorGUILayout.Space();
            }
            if (EditorGUI.EndChangeCheck())
                HandleLinkedMaterials();
            DrawingData.lastGuiObjectHeaderRect = headerRect;
            DrawingData.lastGuiObjectRect = headerRect;
        }

        private void HandleLinkedMaterials()
        {
            List<Material> linked_materials = MaterialLinker.GetLinked(materialProperty);
            if (linked_materials != null)
                foreach (Material m in linked_materials)
                    this.CopyToMaterial(m);
        }
    }

    public class ShaderProperty : ShaderPart
    {
        public bool drawDefault;

        public float setFloat;
        public bool updateFloat;

        public bool forceOneLine = false;

        private int property_index = 0;

        public string keyword;

        public ShaderProperty(ShaderEditor shaderEditor, MaterialProperty materialProperty, string displayName, int xOffset, PropertyOptions options, bool forceOneLine) : base(shaderEditor, materialProperty, xOffset, displayName, options)
        {
            drawDefault = false;
            this.forceOneLine = forceOneLine;

            property_index = System.Array.IndexOf(ShaderEditor.active.properties, materialProperty);
        }

        public override void CopyFromMaterial(Material m)
        {
            MaterialHelper.CopyPropertyValueFromMaterial(materialProperty, m);
            if (keyword != null) SetKeyword(ShaderEditor.active.materials, m.GetFloat(materialProperty.name)==1);
            if (is_animatable)
            {
                ShaderOptimizer.CopyAnimatedTagFromMaterial(m, materialProperty);
            }
            this.is_animated = is_animatable && ShaderOptimizer.GetAnimatedTag(materialProperty) != "";
            this.is_renaming = is_animatable && ShaderOptimizer.GetAnimatedTag(materialProperty) == "2";
        }

        public override void CopyToMaterial(Material m)
        {
            MaterialHelper.CopyPropertyValueToMaterial(materialProperty, m);
            if (keyword != null) SetKeyword(m, materialProperty.floatValue == 1);
            if (is_animatable)
                ShaderOptimizer.CopyAnimatedTagToMaterials(new Material[] { m }, materialProperty);
        }

        private void SetKeyword(Material[] materials, bool enabled)
        {
            if (enabled) foreach (Material m in materials) m.EnableKeyword(keyword);
            else foreach (Material m in materials) m.DisableKeyword(keyword);
        }

        private void SetKeyword(Material m, bool enabled)
        {
            if (enabled) m.EnableKeyword(keyword);
            else m.DisableKeyword(keyword);
        }

        public override void DrawInternal(GUIContent content, CRect rect = null, bool useEditorIndent = false, bool isInHeader = false)
        {
            PreDraw();
            ShaderEditor.active.currentProperty = this;
            this.materialProperty = ShaderEditor.active.properties[property_index];
            if (ShaderEditor.active.isLockedMaterial)
                EditorGUI.BeginDisabledGroup(!(is_animatable && (is_animated || is_renaming)) && !exempt_from_locked_disabling);
            int oldIndentLevel = EditorGUI.indentLevel;
            if (!useEditorIndent)
                EditorGUI.indentLevel = xOffset + 1;

            if (drawDefault)
                DrawDefault();
            else
            {
                //ShaderEditor.active.gui.BeginAnimatedCheck(materialProperty);
                if (forceOneLine)
                    ShaderEditor.active.editor.ShaderProperty(GUILayoutUtility.GetRect(content, Styles.vectorPropertyStyle), this.materialProperty, content);
                else if (rect != null)
                    ShaderEditor.active.editor.ShaderProperty(rect.r, this.materialProperty, content);
                else
                    ShaderEditor.active.editor.ShaderProperty(this.materialProperty, content);
                //ShaderEditor.active.gui.EndAnimatedCheck();
            }

            EditorGUI.indentLevel = oldIndentLevel;
            if (rect == null) DrawingData.lastGuiObjectRect = GUILayoutUtility.GetLastRect();
            else DrawingData.lastGuiObjectRect = rect.r;
            if (this is TextureProperty == false && is_animatable && isInHeader == false)
                HandleKajAnimatable();
            if (ShaderEditor.active.isLockedMaterial)
                EditorGUI.EndDisabledGroup();
        }

        public virtual void PreDraw() { }

        public virtual void DrawDefault() { }

        public override void TransferFromMaterialAndGroup(Material m, ShaderPart p)
        {
            if (materialProperty.type != p.materialProperty.type) return;
            MaterialHelper.CopyMaterialValueFromProperty(materialProperty, p.materialProperty);
            if (keyword != null) SetKeyword(ShaderEditor.active.materials, m.GetFloat(p.materialProperty.name) == 1);
            if (is_animatable && p.is_animatable)
                ShaderOptimizer.CopyAnimatedTagFromProperty(p.materialProperty, materialProperty);
            this.is_animated = is_animatable && ShaderOptimizer.GetAnimatedTag(materialProperty) != "";
            this.is_renaming = is_animatable && ShaderOptimizer.GetAnimatedTag(materialProperty) == "2";
        }
    }

    public class TextureProperty : ShaderProperty
    {
        public bool showFoldoutProperties = false;
        public bool hasFoldoutProperties = false;
        public bool hasScaleOffset = false;

        public TextureProperty(ShaderEditor shaderEditor, MaterialProperty materialProperty, string displayName, int xOffset, PropertyOptions options, bool hasScaleOffset, bool forceThryUI) : base(shaderEditor, materialProperty, displayName, xOffset, options, false)
        {
            drawDefault = forceThryUI;
            this.hasScaleOffset = hasScaleOffset;
            this.hasFoldoutProperties = hasScaleOffset || reference_properties_exist;
        }

        public override void PreDraw()
        {
            DrawingData.currentTexProperty = this;
        }

        public override void DrawDefault()
        {
            Rect pos = GUILayoutUtility.GetRect(content, Styles.vectorPropertyStyle);
            GuiHelper.drawConfigTextureProperty(pos, materialProperty, content, ShaderEditor.active.editor, hasFoldoutProperties);
            DrawingData.lastGuiObjectRect = pos;
        }

        public override void CopyFromMaterial(Material m)
        {
            MaterialHelper.CopyPropertyValueFromMaterial(materialProperty, m);
            CopyReferencePropertiesFromMaterial(m);
        }

        public override void CopyToMaterial(Material m)
        {
            MaterialHelper.CopyPropertyValueToMaterial(materialProperty, m);
            CopyReferencePropertiesToMaterial(m);
        }

        public override void TransferFromMaterialAndGroup(Material m, ShaderPart p)
        {
            if (materialProperty.type != p.materialProperty.type) return;
            MaterialHelper.CopyMaterialValueFromProperty(materialProperty, p.materialProperty);
            TransferReferencePropertiesToMaterial(m, p);
        }
        private void TransferReferencePropertiesToMaterial(Material target, ShaderPart p)
        {
            if (p.options.reference_properties == null || this.options.reference_properties == null) return;
            for (int i = 0; i < p.options.reference_properties.Length && i < options.reference_properties.Length; i++)
            {
                if (ShaderEditor.active.propertyDictionary.ContainsKey(this.options.reference_properties[i]) == false) continue;

                ShaderProperty targetP = ShaderEditor.active.propertyDictionary[this.options.reference_properties[i]];
                ShaderProperty sourceP = p.shaderEditor.propertyDictionary[p.options.reference_properties[i]];
                MaterialHelper.CopyMaterialValueFromProperty(targetP.materialProperty, sourceP.materialProperty);
            }
        }

        private void CopyReferencePropertiesToMaterial(Material target)
        {
            if (options.reference_properties != null)
                foreach (string r_property in options.reference_properties)
                {
                    ShaderProperty property = ShaderEditor.active.propertyDictionary[r_property];
                    MaterialHelper.CopyPropertyValueToMaterial(property.materialProperty, target);
                }
        }

        private void CopyReferencePropertiesFromMaterial(Material source)
        {
            if (options.reference_properties != null)
                foreach (string r_property in options.reference_properties)
                {
                    ShaderProperty property = ShaderEditor.active.propertyDictionary[r_property];
                    MaterialHelper.CopyPropertyValueFromMaterial(property.materialProperty, source);
                }
        }
    }

    public class ShaderHeaderProperty : ShaderPart
    {
        public ShaderHeaderProperty(ShaderEditor shaderEditor, MaterialProperty materialProperty, string displayName, int xOffset, PropertyOptions options, bool forceOneLine) : base(shaderEditor, materialProperty, xOffset, displayName, options)
        {
        }

        public override void DrawInternal(GUIContent content, CRect rect = null, bool useEditorIndent = false, bool isInHeader = false)
        {
            if (rect == null)
            {
                if (options.texture != null && options.texture.name != null)
                {
                    //is texutre draw
                    content = new GUIContent(options.texture.loaded_texture, content.tooltip);
                    int height = options.texture.height;
                    int width = (int)((float)options.texture.loaded_texture.width / options.texture.loaded_texture.height * height);
                    Rect control = EditorGUILayout.GetControlRect(false, height-18);
                    Rect r = new Rect((control.width-width)/2,control.y,width, height);
                    GUI.DrawTexture(r, options.texture.loaded_texture);
                }
            }
            else
            {
                //is text draw
                Rect headerrect = new Rect(0, rect.r.y, rect.r.width, 18);
                EditorGUI.LabelField(headerrect, "<size=16>" + this.content.text + "</size>", Styles.masterLabel);
                DrawingData.lastGuiObjectRect = headerrect;
            }
        }

        public override void CopyFromMaterial(Material m)
        {
            throw new System.NotImplementedException();
        }

        public override void CopyToMaterial(Material m)
        {
            throw new System.NotImplementedException();
        }

        public override void TransferFromMaterialAndGroup(Material m, ShaderPart p)
        {
            throw new System.NotImplementedException();
        }
    }

    public class InstancingProperty : ShaderProperty
    {
        public InstancingProperty(ShaderEditor shaderEditor, MaterialProperty materialProperty, string displayName, int xOffset, PropertyOptions options, bool forceOneLine) : base(shaderEditor, materialProperty, displayName, xOffset, options, forceOneLine)
        {
            drawDefault = true;
        }

        public override void DrawDefault()
        {
            ShaderEditor.active.editor.EnableInstancingField();
        }
    }
    public class GIProperty : ShaderProperty
    {
        public GIProperty(ShaderEditor shaderEditor, MaterialProperty materialProperty, string displayName, int xOffset, PropertyOptions options, bool forceOneLine) : base(shaderEditor, materialProperty, displayName, xOffset, options, forceOneLine)
        {
            drawDefault = true;
        }

        public override void DrawDefault()
        {
            LightmapEmissionFlagsProperty(xOffset, false);
        }

        public static readonly GUIContent lightmapEmissiveLabel = EditorGUIUtility.TrTextContent("Global Illumination", "Controls if the emission is baked or realtime.\n\nBaked only has effect in scenes where baked global illumination is enabled.\n\nRealtime uses realtime global illumination if enabled in the scene. Otherwise the emission won't light up other objects.");
        public static GUIContent[] lightmapEmissiveStrings = { EditorGUIUtility.TrTextContent("Realtime"), EditorGUIUtility.TrTextContent("Baked"), EditorGUIUtility.TrTextContent("None") };
        public static int[] lightmapEmissiveValues = { (int)MaterialGlobalIlluminationFlags.RealtimeEmissive, (int)MaterialGlobalIlluminationFlags.BakedEmissive, (int)MaterialGlobalIlluminationFlags.None };

        public static void FixupEmissiveFlag(Material mat)
        {
            if (mat == null)
                throw new System.ArgumentNullException("mat");

            mat.globalIlluminationFlags = FixupEmissiveFlag(mat.GetColor("_EmissionColor"), mat.globalIlluminationFlags);
        }

        public static MaterialGlobalIlluminationFlags FixupEmissiveFlag(Color col, MaterialGlobalIlluminationFlags flags)
        {
            if ((flags & MaterialGlobalIlluminationFlags.BakedEmissive) != 0 && col.maxColorComponent == 0.0f) // flag black baked
                flags |= MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            else if (flags != MaterialGlobalIlluminationFlags.EmissiveIsBlack) // clear baked flag on everything else, unless it's explicity disabled
                flags &= MaterialGlobalIlluminationFlags.AnyEmissive;
            return flags;
        }

        public void LightmapEmissionFlagsProperty(int indent, bool enabled)
        {
            LightmapEmissionFlagsProperty(indent, enabled, false);
        }

        public void LightmapEmissionFlagsProperty(int indent, bool enabled, bool ignoreEmissionColor)
        {
            // Calculate isMixed
            MaterialGlobalIlluminationFlags any_em = MaterialGlobalIlluminationFlags.AnyEmissive;
            MaterialGlobalIlluminationFlags giFlags = ShaderEditor.active.materials[0].globalIlluminationFlags & any_em;
            bool isMixed = false;
            for (int i = 1; i < ShaderEditor.active.materials.Length; i++)
            {
                if((ShaderEditor.active.materials[i].globalIlluminationFlags & any_em) != giFlags)
                {
                    isMixed = true;
                    break;
                }
            }

            EditorGUI.BeginChangeCheck();

            // Show popup
            EditorGUI.showMixedValue = isMixed;
            giFlags = (MaterialGlobalIlluminationFlags)EditorGUILayout.IntPopup(lightmapEmissiveLabel, (int)giFlags, lightmapEmissiveStrings, lightmapEmissiveValues);
            EditorGUI.showMixedValue = false;

            // Apply flags. But only the part that this tool modifies (RealtimeEmissive, BakedEmissive, None)
            bool applyFlags = EditorGUI.EndChangeCheck();
            foreach (Material mat in ShaderEditor.active.materials)
            {
                mat.globalIlluminationFlags = applyFlags ? giFlags : mat.globalIlluminationFlags;
                if (!ignoreEmissionColor)
                {
                    FixupEmissiveFlag(mat);
                }
            }
        }
    }
    public class DSGIProperty : ShaderProperty
    {
        public DSGIProperty(ShaderEditor shaderEditor, MaterialProperty materialProperty, string displayName, int xOffset, PropertyOptions options, bool forceOneLine) : base(shaderEditor, materialProperty, displayName, xOffset, options, forceOneLine)
        {
            drawDefault = true;
        }

        public override void DrawDefault()
        {
            ShaderEditor.active.editor.DoubleSidedGIField();
        }
    }
    public class LocaleProperty : ShaderProperty
    {
        public LocaleProperty(ShaderEditor shaderEditor, MaterialProperty materialProperty, string displayName, int xOffset, PropertyOptions options, bool forceOneLine) : base(shaderEditor, materialProperty, displayName, xOffset, options, forceOneLine)
        {
            drawDefault = true;
        }

        public override void DrawDefault()
        {
            GuiHelper.DrawLocaleSelection(this.content, ShaderEditor.active.gui.locale.available_locales, ShaderEditor.active.gui.locale.selected_locale_index);
        }
    }
}