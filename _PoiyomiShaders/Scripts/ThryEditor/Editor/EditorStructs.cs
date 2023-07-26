using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Thry.ThryEditor;
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
        int _button;
        bool _MouseClick;
        bool _MouseLeftClickIgnoreLocked;
        bool _MouseRightClickIgnoreLocked;
        bool _MouseLeftClick;
        bool _MouseRightClick;

        public bool is_alt_down;

        public bool is_drag_drop_event;
        public bool is_drop_event;

        public Vector2 mouse_position;
        public Vector2 screen_mouse_position;

        public void Update(bool isLockedMaterial)
        {
            Event e = Event.current;
            _button = e.button;
            _MouseClick = e.type == EventType.MouseDown && !isLockedMaterial;
            _MouseLeftClick = _MouseClick && _button == 0;
            _MouseRightClick = _MouseClick && _button == 1;
            _MouseLeftClickIgnoreLocked = e.type == EventType.MouseDown && _button == 0;
            _MouseRightClickIgnoreLocked = e.type == EventType.MouseDown && _button == 1;
            if (_MouseClick) HadMouseDown = _MouseClick;
            if (HadMouseDown && e.type == EventType.Repaint)
            {
                HadMouseDownRepaint = true;
                HadMouseDown = false;
            }
            is_alt_down = e.alt;
            mouse_position = e.mousePosition;
            screen_mouse_position = GUIUtility.GUIToScreenPoint(e.mousePosition);
            is_drop_event = e.type == EventType.DragPerform;
            is_drag_drop_event = is_drop_event || e.type == EventType.DragUpdated;
        }

        public void Use()
        {
            _MouseClick = false;
            _MouseLeftClick = false;
            _MouseRightClick = false;
            Event.current.Use();
        }

        public bool LeftClick_IgnoreUnityUses
        {
            get { return _MouseLeftClick;  }
        }

        public bool RightClick_IgnoreUnityUses
        {
            get { return _MouseRightClick; }
        }

        public bool LeftClick_IgnoreLocked
        {
            get { return _MouseLeftClickIgnoreLocked && Event.current.type != EventType.Used; }
        }

        public bool RightClick_IgnoreLocked
        {
            get { return _MouseRightClickIgnoreLocked && Event.current.type != EventType.Used; }
        }

        public bool LeftClick_IgnoreLockedAndUnityUses
        {
            get { return _MouseLeftClickIgnoreLocked; }
        }

        public bool RightClick_IgnoreLockedAndUnityUses
        {
            get { return _MouseRightClickIgnoreLocked; }
        }

        public bool Click
        {
            get { return _MouseClick && Event.current.type != EventType.Used; }
        }

        public bool RightClick
        {
            get { return _MouseRightClick && Event.current.type != EventType.Used; }
        }

        public bool LeftClick
        {
            get { return _MouseLeftClick && Event.current.type != EventType.Used; }
        }
    }

    public abstract class ShaderPart
    {
        public ShaderEditor ActiveShaderEditor;

        public int XOffset = 0;
        public GUIContent Content;
        public MaterialProperty MaterialProperty;
        public string PropertyIdentifier;
        public System.Object PropertyData = null;
        public bool DoReferencePropertiesExist = false;
        public bool DoesReferencePropertyExist = false;
        public bool IsHidden = false;
        public bool IsAnimatable = false;
        public bool IsPreset = false;
        public bool ExemptFromLockedDisabling = false;
        public bool IsAnimated = false;
        public bool IsRenaming = false;

        public BetterTooltips.Tooltip tooltip;

        public bool has_not_searchedFor = false; //used for property search

        protected string _optionsRaw;
        private PropertyOptions _options;
        private bool _doOptionsNeedInitilization = true;
        public PropertyOptions Options
        {
            get
            {
                if (_options == null)
                {
                    _options = PropertyOptions.Deserialize(_optionsRaw);
                }
                return _options;
            }
        }

        GenericMenu contextMenu;

        public ShaderPart(string propertyIdentifier, int xOffset, string displayName, string tooltip, ShaderEditor shaderEditor)
        {
            this._optionsRaw = null;
            this.ActiveShaderEditor = shaderEditor;
            this.PropertyIdentifier = propertyIdentifier;
            this.XOffset = xOffset;
            this.Content = new GUIContent(displayName);
            this.tooltip = new BetterTooltips.Tooltip(tooltip);
            this.IsPreset = shaderEditor.IsPresetEditor && Presets.IsPreset(shaderEditor.Materials[0], this);
        }

        public ShaderPart(ShaderEditor shaderEditor, MaterialProperty prop, int xOffset, string displayName, string optionsRaw)
        {
            this._optionsRaw = optionsRaw;
            this.ActiveShaderEditor = shaderEditor;
            this.MaterialProperty = prop;
            this.XOffset = xOffset;
            this.Content = new GUIContent(displayName);
            this.IsPreset = shaderEditor.IsPresetEditor && Presets.IsPreset(shaderEditor.Materials[0], this);

            if (prop == null)
                return;

            this.ExemptFromLockedDisabling |= ShaderOptimizer.IsPropertyExcemptFromLocking(prop);
        }

        protected virtual void InitOptions()
        {
            this.tooltip = new BetterTooltips.Tooltip(Options.tooltip);
            this.DoReferencePropertiesExist = Options.reference_properties != null && Options.reference_properties.Length > 0;
            this.DoesReferencePropertyExist = Options.reference_property != null;
            this.XOffset += Options.offset;
        }

        public void SetReferenceProperty(string s)
        {
            Options.reference_property = s;
            this.DoesReferencePropertyExist = Options.reference_property != null;
        }

        public void SetReferenceProperties(string[] properties)
        {
            Options.reference_properties = properties;
            this.DoReferencePropertiesExist = Options.reference_properties != null && Options.reference_properties.Length > 0;
        }

        public void SetTooltip(string tooltip)
        {
            this.tooltip.SetText(tooltip);
        }

        public abstract void DrawInternal(GUIContent content, CRect rect = null, bool useEditorIndent = false, bool isInHeader = false);
        public abstract void CopyFromMaterial(Material m, bool isTopCall = false);
        public abstract void CopyToMaterial(Material m, bool isTopCall = false);

        public abstract void TransferFromMaterialAndGroup(Material m, ShaderPart g, bool isTopCall = false);

        bool hasAddedDisabledGroup = false;
        public void Draw(CRect rect = null, GUIContent content = null, bool useEditorIndent = false, bool isInHeader = false)
        {
            if(_doOptionsNeedInitilization)
            {
                InitOptions();
                _doOptionsNeedInitilization = false;
            }

            if (has_not_searchedFor)
                return;
            if (DrawingData.IsEnabled && Options.condition_enable != null)
            {
                hasAddedDisabledGroup = !Options.condition_enable.Test();
                if(hasAddedDisabledGroup)
                {
                    DrawingData.IsEnabled = !hasAddedDisabledGroup;
                    EditorGUI.BeginDisabledGroup(true);
                }
            }
            if (Options.condition_show.Test())
            {
                PerformDraw(content, rect, useEditorIndent, isInHeader);
            }
            if (hasAddedDisabledGroup)
            {
                hasAddedDisabledGroup = false;
                DrawingData.IsEnabled = true;
                EditorGUI.EndDisabledGroup();
            }
        }

        public virtual void HandleRightClickToggles(bool isInHeader)
        {
            if (this is ShaderGroup) return;
            if (ShaderEditor.Input.RightClick_IgnoreLockedAndUnityUses && DrawingData.TooltipCheckRect.Contains(Event.current.mousePosition))
            {
                //Context menu
                //Show context menu, if not open.
                //If locked material only show menu for animated materials. Only show data retieving options in locked state
                if ( (!ShaderEditor.Active.IsLockedMaterial || IsAnimated)) {
                    contextMenu = new GenericMenu();
                    if (IsAnimatable && !ShaderEditor.Active.IsLockedMaterial)
                    {
                        contextMenu.AddItem(new GUIContent("Animated (when locked)"), IsAnimated, () => { SetAnimated(!IsAnimated, false); });
                        contextMenu.AddItem(new GUIContent("Renamed (when locked)"), IsAnimated && IsRenaming, () => { SetAnimated(true, !IsRenaming); });
                        contextMenu.AddItem(new GUIContent("Locking Explanation"), false, () => { Application.OpenURL("https://www.youtube.com/watch?v=asWeDJb5LAo&ab_channel=poiyomi"); });
                        contextMenu.AddSeparator("");
                    }
                    if (ShaderEditor.Active.IsPresetEditor )
                    {
                        contextMenu.AddItem(new GUIContent("Is part of preset"), IsPreset, ToggleIsPreset);
                        contextMenu.AddSeparator("");
                    }
                    contextMenu.AddItem(new GUIContent("Copy Property Name"), false, () => { EditorGUIUtility.systemCopyBuffer = MaterialProperty.name; });
                    contextMenu.AddItem(new GUIContent("Copy Animated Property Name"), false, () => { EditorGUIUtility.systemCopyBuffer = GetAnimatedPropertyName(); });
                    contextMenu.AddItem(new GUIContent("Copy Animated Property Path"), false, CopyPropertyPath );
                    contextMenu.AddItem(new GUIContent("Copy Property as Keyframe"), false, CopyPropertyAsKeyframe);
                    contextMenu.ShowAsContext();
                }
            }
        }

        void ToggleIsPreset()
        {
            IsPreset = !IsPreset;
            if(MaterialProperty != null) Presets.SetProperty(ActiveShaderEditor.Materials[0], this, IsPreset);
            ShaderEditor.RepaintActive();
        }

        void CopyPropertyPath()
        {
            string path = GetAnimatedPropertyName();
            Transform selected = Selection.activeTransform;
            Transform root = selected;
            while(root != null && root.GetComponent<Animator>() == null)
                root = root.parent;
            if (selected != null && root != null && selected != root)
                path = AnimationUtility.CalculateTransformPath(selected, root) + "/" + path;
            EditorGUIUtility.systemCopyBuffer = path;
        }

        string GetAnimatedPropertyName()
        {
            string propName = MaterialProperty.name;
            if (IsRenaming && !ShaderEditor.Active.IsLockedMaterial) propName = propName + "_" + ShaderEditor.Active.RenamedPropertySuffix;
            if (MaterialProperty.type == MaterialProperty.PropType.Texture) propName = propName + "_ST";
            return propName;
        }

        void CopyPropertyAsKeyframe()
        {
            string path = "";
            Transform selected = Selection.activeTransform;
            Transform root = selected;
            while (root != null && root.GetComponent<Animator>() == null)
                root = root.parent;
            if (selected != null && root != null && selected != root)
                path = AnimationUtility.CalculateTransformPath(selected, root);
            if(selected == null && root == null)
                return;

            Type rendererType = typeof(Renderer);
            if (selected.GetComponent<SkinnedMeshRenderer>()) rendererType = typeof(SkinnedMeshRenderer);
            if (selected.GetComponent<MeshRenderer>()) rendererType = typeof(MeshRenderer);

            Type animationStateType = typeof(AnimationUtility).Assembly.GetType("UnityEditorInternal.AnimationWindowState");
            Type animationKeyframeType = typeof(AnimationUtility).Assembly.GetType("UnityEditorInternal.AnimationWindowKeyframe");
            Type animationCurveType = typeof(AnimationUtility).Assembly.GetType("UnityEditorInternal.AnimationWindowCurve");

            FieldInfo clipboardField = animationStateType.GetField("s_KeyframeClipboard", BindingFlags.NonPublic | BindingFlags.Static);

            Type keyframeListType = typeof(List<>).MakeGenericType(animationKeyframeType);
            IList keyframeList = (IList)Activator.CreateInstance(keyframeListType);

            AnimationClip clip = new AnimationClip();

            string propertyname = "material." + GetAnimatedPropertyName();
            if (MaterialProperty.type == MaterialProperty.PropType.Float || MaterialProperty.type == MaterialProperty.PropType.Range)
            {
                clip.SetCurve(path, rendererType, propertyname, new AnimationCurve(new Keyframe(0, MaterialProperty.floatValue)));
                keyframeList.Add(ClipToKeyFrame(animationCurveType, clip, path, "", rendererType));
            }
            else if(MaterialProperty.type == MaterialProperty.PropType.Color)
            {
                clip.SetCurve(path, rendererType, propertyname + ".r", new AnimationCurve(new Keyframe(0, MaterialProperty.colorValue.r)));
                clip.SetCurve(path, rendererType, propertyname + ".g", new AnimationCurve(new Keyframe(0, MaterialProperty.colorValue.g)));
                clip.SetCurve(path, rendererType, propertyname + ".b", new AnimationCurve(new Keyframe(0, MaterialProperty.colorValue.b)));
                clip.SetCurve(path, rendererType, propertyname + ".a", new AnimationCurve(new Keyframe(0, MaterialProperty.colorValue.a)));
                keyframeList.Add(ClipToKeyFrame(animationCurveType, clip, path, ".r", rendererType));
                keyframeList.Add(ClipToKeyFrame(animationCurveType, clip, path, ".g", rendererType));
                keyframeList.Add(ClipToKeyFrame(animationCurveType, clip, path, ".b", rendererType));
                keyframeList.Add(ClipToKeyFrame(animationCurveType, clip, path, ".a", rendererType));
            }else if(MaterialProperty.type == MaterialProperty.PropType.Vector)
            {
                clip.SetCurve(path, rendererType, propertyname + ".x", new AnimationCurve(new Keyframe(0, MaterialProperty.vectorValue.x)));
                clip.SetCurve(path, rendererType, propertyname + ".y", new AnimationCurve(new Keyframe(0, MaterialProperty.vectorValue.y)));
                clip.SetCurve(path, rendererType, propertyname + ".z", new AnimationCurve(new Keyframe(0, MaterialProperty.vectorValue.z)));
                clip.SetCurve(path, rendererType, propertyname + ".w", new AnimationCurve(new Keyframe(0, MaterialProperty.vectorValue.w)));
                keyframeList.Add(ClipToKeyFrame(animationCurveType, clip, path, ".x", rendererType));
                keyframeList.Add(ClipToKeyFrame(animationCurveType, clip, path, ".y", rendererType));
                keyframeList.Add(ClipToKeyFrame(animationCurveType, clip, path, ".z", rendererType));
                keyframeList.Add(ClipToKeyFrame(animationCurveType, clip, path, ".w", rendererType));
            }else if(MaterialProperty.type == MaterialProperty.PropType.Texture)
            {
                clip.SetCurve(path, rendererType, propertyname + ".x", new AnimationCurve(new Keyframe(0, MaterialProperty.textureScaleAndOffset.x)));
                clip.SetCurve(path, rendererType, propertyname + ".y", new AnimationCurve(new Keyframe(0, MaterialProperty.textureScaleAndOffset.y)));
                clip.SetCurve(path, rendererType, propertyname + ".z", new AnimationCurve(new Keyframe(0, MaterialProperty.textureScaleAndOffset.z)));
                clip.SetCurve(path, rendererType, propertyname + ".w", new AnimationCurve(new Keyframe(0, MaterialProperty.textureScaleAndOffset.w)));
                keyframeList.Add(ClipToKeyFrame(animationCurveType, clip, path, ".x", rendererType));
                keyframeList.Add(ClipToKeyFrame(animationCurveType, clip, path, ".y", rendererType));
                keyframeList.Add(ClipToKeyFrame(animationCurveType, clip, path, ".z", rendererType));
                keyframeList.Add(ClipToKeyFrame(animationCurveType, clip, path, ".w", rendererType));
            }
            clipboardField.SetValue(null, keyframeList);
        }

        object ClipToKeyFrame(Type animationCurveType, AnimationClip clip, string path, string propertyPostFix, Type rendererType)
        {
            FieldInfo curvesField = animationCurveType.GetField("m_Keyframes", BindingFlags.Instance | BindingFlags.Public);

            object windowCurve = Activator.CreateInstance(animationCurveType, clip,
                EditorCurveBinding.FloatCurve(path, rendererType, "material." + GetAnimatedPropertyName() + propertyPostFix), typeof(float));
            IEnumerator enumerator = (curvesField.GetValue(windowCurve) as IList).GetEnumerator();
            enumerator.MoveNext();
            return enumerator.Current;
        }

        public void SetAnimated(bool animated, bool renamed)
        {
            if (IsAnimated == animated && IsRenaming == renamed) return;
            IsAnimated = animated;
            IsRenaming = renamed;
            ShaderOptimizer.SetAnimatedTag(MaterialProperty, IsAnimated ? (IsRenaming ? "2" : "1") : "");
        }

        private void PerformDraw(GUIContent content, CRect rect, bool useEditorIndent, bool isInHeader = false)
        {
            if (content == null)
                content = this.Content;
            EditorGUI.BeginChangeCheck();
            DrawInternal(content, rect, useEditorIndent, isInHeader);

            if(this is TextureProperty == false)
            {
                DrawingData.TooltipCheckRect = DrawingData.LastGuiObjectRect;
                DrawingData.IconsPositioningHeight = DrawingData.LastGuiObjectRect.y + DrawingData.LastGuiObjectRect.height - 14;
            } 
            DrawingData.TooltipCheckRect.width = EditorGUIUtility.labelWidth;

            HandleRightClickToggles(isInHeader);

            if (EditorGUI.EndChangeCheck())
            {
                OnPropertyValueChanged();
                ExecuteOnValueActions(ShaderEditor.Active.Materials);
                //Check if property is being animated
                if (this is ShaderProperty && ActiveShaderEditor.ActiveRenderer != null && ActiveShaderEditor.IsInAnimationMode && IsAnimatable && !IsAnimated)
                {
                    if (MaterialProperty.type == MaterialProperty.PropType.Texture ? 
                        AnimationMode.IsPropertyAnimated(ActiveShaderEditor.ActiveRenderer, "material." + MaterialProperty.name + "_ST.x" ) :
                        AnimationMode.IsPropertyAnimated(ActiveShaderEditor.ActiveRenderer, "material." + MaterialProperty.name))
                        SetAnimated(true, false);
                }
            }

            if (IsAnimatable && IsAnimated) DrawLockedAnimated();
            if (IsPreset) DrawPresetProperty();

            tooltip.ConditionalDraw(DrawingData.TooltipCheckRect);

            //Click testing
            if(Event.current.type == EventType.MouseDown && DrawingData.LastGuiObjectRect.Contains(ShaderEditor.Input.mouse_position))
            {
                if ((ShaderEditor.Input.is_alt_down && Options.altClick != null)) Options.altClick.Perform(ShaderEditor.Active.Materials);
                else if (Options.onClick != null) Options.onClick.Perform(ShaderEditor.Active.Materials);
            }
        }

        protected virtual void OnPropertyValueChanged()
        {

        }

        private void DrawLockedAnimated()
        {
            Rect r = new Rect(14, DrawingData.IconsPositioningHeight, 16, 16);
            //GUI.DrawTexture(r, is_renaming ? Styles.texture_animated_renamed : Styles.texture_animated, ScaleMode.StretchToFill, true);
            if (IsRenaming) GUI.Label(r, "RA", Styles.animatedIndicatorStyle);
            else GUI.Label(r, "A", Styles.animatedIndicatorStyle);
        }

        private void DrawPresetProperty()
        {
            Rect r = new Rect(3, DrawingData.IconsPositioningHeight, 8, 16);
            //GUI.DrawTexture(r, Styles.texture_preset, ScaleMode.StretchToFill, true);
            GUI.Label(r, "P", Styles.presetIndicatorStyle);
        }

        protected void ExecuteOnValueActions(Material[] targets)
        {
            if (Options.on_value_actions != null)
                foreach (PropertyValueAction action in Options.on_value_actions)
                {
                    action.Execute(MaterialProperty, targets);
                }
        }

        public abstract void FindUnusedTextures(List<string> unusedList, bool isEnabled);
    }

    public class ShaderGroup : ShaderPart
    {
        public List<ShaderPart> parts = new List<ShaderPart>();

        public ShaderGroup(ShaderEditor shaderEditor) : base(null, 0, "", null, shaderEditor)
        {

        }

        public ShaderGroup(ShaderEditor shaderEditor, string optionsRaw) : base(null, 0, "", null, shaderEditor)
        {
            this._optionsRaw = optionsRaw;
        }

        public ShaderGroup(ShaderEditor shaderEditor, MaterialProperty prop, MaterialEditor materialEditor, string displayName, int xOffset, string optionsRaw) : base(shaderEditor, prop, xOffset, displayName, optionsRaw)
        {

        }

        public void addPart(ShaderPart part)
        {
            parts.Add(part);
        }

        public override void CopyFromMaterial(Material m, bool isTopCall = false)
        {
            if (Options.reference_property != null)
                ActiveShaderEditor.PropertyDictionary[Options.reference_property].CopyFromMaterial(m);
            foreach (ShaderPart p in parts)
                p.CopyFromMaterial(m);
            if (isTopCall) ActiveShaderEditor.ApplyDrawers();
        }

        public override void CopyToMaterial(Material m, bool isTopCall = false)
        {
            if (Options.reference_property != null)
                ActiveShaderEditor.PropertyDictionary[Options.reference_property].CopyToMaterial(m);
            foreach (ShaderPart p in parts)
                p.CopyToMaterial(m);
            if (isTopCall) MaterialEditor.ApplyMaterialPropertyDrawers(m);
        }

        public override void DrawInternal(GUIContent content, CRect rect = null, bool useEditorIndent = false, bool isInHeader = false)
        {
            if(Options.margin_top > 0)
            {
                GUILayoutUtility.GetRect(0, Options.margin_top);
            }

            bool doExpand = Options.condition_expand.Test();
            if(Options.draw_border)
            {
                bool has_header = string.IsNullOrWhiteSpace(this.Content.text) == false;
                int headerXOffset = 0;
                ShaderProperty reference = Options.reference_property != null ? ActiveShaderEditor.PropertyDictionary[Options.reference_property] : null;

                Rect border = EditorGUILayout.BeginVertical();
                GUILayoutUtility.GetRect(0, 5 + (has_header ? 20 : 0));
                border = new RectOffset(this.XOffset * -15 - 12, 3, -2, -2).Add(border);
                if(doExpand)
                {
                    // Draw as border line
                    Vector4 borderWidths = new Vector4(3, (has_header ? 22 : 3), 3, 3);
                    GUI.DrawTexture(border, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Styles.COLOR_BACKGROUND_1, borderWidths, 10);
                }else
                {
                    // Draw as solid
                    GUI.DrawTexture(border, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, Styles.COLOR_BACKGROUND_1, Vector4.zero, 10);
                }
                if(reference != null)
                {
                    Rect referenceRect = new Rect(border.x + 16, border.y + 2, 20, 20);
                    reference.Draw(new CRect(referenceRect), new GUIContent(), isInHeader: true, useEditorIndent: true);
                    headerXOffset = 16;
                }
                if(has_header)
                {
                    Rect header = new Rect(border.x + 16 + headerXOffset, border.y, border.width - 16, 22);
                    GUI.Label(header, this.Content, EditorStyles.boldLabel);
                }
            }
            if(doExpand)
            {
                foreach (ShaderPart part in parts)
                {
                    part.Draw();
                }
            }
            if (Options.draw_border)
            {
                GUILayoutUtility.GetRect(0, 5);
                EditorGUILayout.EndVertical();
            }
        }

        public override void TransferFromMaterialAndGroup(Material m, ShaderPart p, bool isTopCall = false)
        {
            if (p is ShaderGroup == false) return;
            ShaderGroup group = p as ShaderGroup;
            if (Options.reference_property != null && group.Options.reference_property != null)
                ActiveShaderEditor.PropertyDictionary[Options.reference_property].TransferFromMaterialAndGroup(m, group.ActiveShaderEditor.PropertyDictionary[group.Options.reference_property]);
            for(int i=0;i<group.parts.Count && i < parts.Count; i++)
            {
                parts[i].TransferFromMaterialAndGroup(m, group.parts[i]);
            }
            if (isTopCall) ActiveShaderEditor.ApplyDrawers();
        }

        public override void FindUnusedTextures(List<string> unusedList, bool isEnabled)
        {
            if (isEnabled && Options.condition_enable != null)
            {
                isEnabled &= Options.condition_enable.Test();
            }
            foreach (ShaderPart p in (this as ShaderGroup).parts)
                p.FindUnusedTextures(unusedList, isEnabled);
        }
    }

    public class ShaderHeader : ShaderGroup
    {
        private ThryHeaderHandler _headerDrawer;

        public ShaderHeader(ShaderEditor shaderEditor) : base(shaderEditor)
        {
            this._headerDrawer = new ThryHeaderHandler();
        }

        public ShaderHeader(ShaderEditor shaderEditor, MaterialProperty prop, MaterialEditor materialEditor, string displayName, int xOffset, string optionsRaw) : base(shaderEditor, prop, materialEditor, displayName, xOffset, optionsRaw)
        {
            this._headerDrawer = new ThryHeaderHandler();
            this._headerDrawer.xOffset = xOffset;
        }

        public string GetEndProperty()
        {
            return _headerDrawer.GetEndProperty();
        }

        public override void DrawInternal(GUIContent content, CRect rect = null, bool useEditorIndent = false, bool isInHeader = false)
        {
            ActiveShaderEditor.CurrentProperty = this;
            EditorGUI.BeginChangeCheck();
            Rect position = GUILayoutUtility.GetRect(content, Styles.dropDownHeader);
            _headerDrawer.OnGUI(position, this.MaterialProperty, content, ActiveShaderEditor.Editor);
            Rect headerRect = DrawingData.LastGuiObjectHeaderRect;
            if (this._headerDrawer.IsExpanded)
            {
                EditorGUILayout.Space();
                EditorGUI.BeginDisabledGroup(_headerDrawer.DisableContent);
                foreach (ShaderPart part in parts)
                {
                    part.Draw();
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Space();
            }
            if (EditorGUI.EndChangeCheck())
                HandleLinkedMaterials();
            DrawingData.LastGuiObjectHeaderRect = headerRect;
            DrawingData.LastGuiObjectRect = headerRect;
        }

        private void HandleLinkedMaterials()
        {
            List<Material> linked_materials = MaterialLinker.GetLinked(MaterialProperty);
            if (linked_materials != null)
                foreach (Material m in linked_materials)
                    this.CopyToMaterial(m);
        }

        public override void FindUnusedTextures(List<string> unusedList, bool isEnabled)
        {
            if (isEnabled && Options.condition_enable != null)
            {
                isEnabled &= Options.condition_enable.Test();
            }
            isEnabled &= !_headerDrawer.DisableContent;
            foreach (ShaderPart p in (this as ShaderGroup).parts)
                p.FindUnusedTextures(unusedList, isEnabled);
        }
    }

    public class ShaderProperty : ShaderPart
    {
        public float setFloat;
        public bool updateFloat;

        public bool doCustomDrawLogic = false;
        public bool doForceIntoOneLine = false;
        public bool doDrawTwoFields = false;

        //Done for e.g. Vectors cause they draw in 2 lines for some fucking reasons
        public bool doCustomHeightOffset = false;
        public float customHeightOffset = 0;

        private int property_index = 0;

        public string keyword;

        protected MaterialPropertyDrawer[] _customDecorators;
        protected Rect[] _customDecoratorRects;
        protected bool _hasDrawer = false;

        bool _needsDrawerInitlization = true;

        public ShaderProperty(ShaderEditor shaderEditor, string propertyIdentifier, int xOffset, string displayName, string tooltip) : base(propertyIdentifier, xOffset, displayName, tooltip, shaderEditor)
        {

        }

        public ShaderProperty(ShaderEditor shaderEditor, MaterialProperty materialProperty, string displayName, int xOffset, string optionsRaw, bool forceOneLine, int property_index) : base(shaderEditor, materialProperty, xOffset, displayName, optionsRaw)
        {
            this.doCustomDrawLogic = false;
            this.doForceIntoOneLine = forceOneLine;

            this.property_index = property_index;
        }

        protected override void InitOptions()
        {
            base.InitOptions();
            this.doDrawTwoFields = Options.reference_property != null;
        }

        public override void CopyFromMaterial(Material m, bool isTopCall = false)
        {
            MaterialHelper.CopyPropertyValueFromMaterial(MaterialProperty, m);
            if (keyword != null) SetKeyword(ActiveShaderEditor.Materials, m.GetFloat(MaterialProperty.name)==1);
            if (IsAnimatable)
            {
                ShaderOptimizer.CopyAnimatedTagFromMaterial(m, MaterialProperty);
            }
            this.IsAnimated = IsAnimatable && ShaderOptimizer.GetAnimatedTag(MaterialProperty) != "";
            this.IsRenaming = IsAnimatable && ShaderOptimizer.GetAnimatedTag(MaterialProperty) == "2";

            ExecuteOnValueActions(ShaderEditor.Active.Materials);

            if (isTopCall) ActiveShaderEditor.ApplyDrawers();
        }

        public override void CopyToMaterial(Material m, bool isTopCall = false)
        {
            MaterialHelper.CopyPropertyValueToMaterial(MaterialProperty, m);
            if (keyword != null) SetKeyword(m, MaterialProperty.floatValue == 1);
            if (IsAnimatable)
                ShaderOptimizer.CopyAnimatedTagToMaterials(new Material[] { m }, MaterialProperty);

            ExecuteOnValueActions(new Material[] { m });

            if (isTopCall) MaterialEditor.ApplyMaterialPropertyDrawers(m);
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

        public void UpdateKeywordFromValue()
        {
            if (keyword != null) SetKeyword(ActiveShaderEditor.Materials, MaterialProperty.floatValue == 1);
        }

        void InitializeDrawers()
        {
            DrawingData.ResetLastDrawerData();
            DrawingData.IsCollectingProperties = true;
            ShaderEditor.Active.Editor.GetPropertyHeight(MaterialProperty, MaterialProperty.displayName);

            this.IsAnimatable = !DrawingData.LastPropertyDoesntAllowAnimation;
            this._hasDrawer = DrawingData.LastPropertyUsedCustomDrawer;

            if (MaterialProperty.type == MaterialProperty.PropType.Vector && doForceIntoOneLine == false)
            {
                this.doCustomHeightOffset = !DrawingData.LastPropertyUsedCustomDrawer;
                this.customHeightOffset = -EditorGUIUtility.singleLineHeight;
            }
            if(DrawingData.LastPropertyDecorators.Count > 0)
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

        public override void DrawInternal(GUIContent content, CRect rect = null, bool useEditorIndent = false, bool isInHeader = false)
        {
            ActiveShaderEditor.CurrentProperty = this;
            this.MaterialProperty = ActiveShaderEditor.Properties[property_index];

            if(_needsDrawerInitlization)
            {
                InitializeDrawers();
                _needsDrawerInitlization = false;
            }

            PreDraw();
            if (ActiveShaderEditor.IsLockedMaterial)
                EditorGUI.BeginDisabledGroup(!(IsAnimatable && (IsAnimated || IsRenaming)) && !ExemptFromLockedDisabling);
            int oldIndentLevel = EditorGUI.indentLevel;
            if (!useEditorIndent)
                EditorGUI.indentLevel = XOffset + 1;

            if(_customDecorators != null && doCustomDrawLogic)
            {
                for(int i= 0;i<_customDecorators.Length;i++)
                {
                    _customDecoratorRects[i] = EditorGUILayout.GetControlRect(false, GUILayout.Height(_customDecorators[i].GetPropertyHeight(MaterialProperty, content.text, ActiveShaderEditor.Editor)));
                }
            }

            if (doCustomDrawLogic)
            {
                DrawDefault();
            }
            else if (doDrawTwoFields)
            {
                Rect r = GUILayoutUtility.GetRect(content, Styles.vectorPropertyStyle);
                float labelWidth = (r.width - EditorGUIUtility.labelWidth) / 2; ;
                r.width -= labelWidth;
                ActiveShaderEditor.Editor.ShaderProperty(r, this.MaterialProperty, content);

                r.x += r.width;
                r.width = labelWidth;
                float prevLabelW = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 0;
                ActiveShaderEditor.PropertyDictionary[Options.reference_property].Draw(new CRect(r), new GUIContent());
                EditorGUIUtility.labelWidth = prevLabelW;
            }
            else if (doForceIntoOneLine)
            {
                ActiveShaderEditor.Editor.ShaderProperty(GUILayoutUtility.GetRect(content, Styles.vectorPropertyStyle), this.MaterialProperty, content);
            }else if (doCustomHeightOffset)
            {
                ActiveShaderEditor.Editor.ShaderProperty(
                    GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth, ActiveShaderEditor.Editor.GetPropertyHeight(this.MaterialProperty, content.text) + customHeightOffset)
                    , this.MaterialProperty, content);
            }
            else if (rect != null)
            {
                // Custom Drawing for Range, because it doesnt draw correctly if inside the big texture property
                if(!_hasDrawer && MaterialProperty.type == MaterialProperty.PropType.Range)
                {
                    MaterialProperty.floatValue = EditorGUI.Slider(rect.r, content, MaterialProperty.floatValue, 0, MaterialProperty.rangeLimits.y);
                }else
                {
                    ActiveShaderEditor.Editor.ShaderProperty(rect.r, this.MaterialProperty, content);
                }
            }
            else
            {
                ActiveShaderEditor.Editor.ShaderProperty(this.MaterialProperty, content);
            }

            if(_customDecorators != null && doCustomDrawLogic)
            {
                for(int i= 0;i<_customDecorators.Length;i++)
                {
                    _customDecorators[i].OnGUI(_customDecoratorRects[i], MaterialProperty, content, ShaderEditor.Active.Editor);
                }
            }

            EditorGUI.indentLevel = oldIndentLevel;
            if (rect == null) DrawingData.LastGuiObjectRect = GUILayoutUtility.GetLastRect();
            else DrawingData.LastGuiObjectRect = rect.r;
            if (ActiveShaderEditor.IsLockedMaterial)
                EditorGUI.EndDisabledGroup();
        }

        public virtual void PreDraw() { }

        public virtual void DrawDefault() { }

        public override void TransferFromMaterialAndGroup(Material m, ShaderPart p, bool isTopCall = false)
        {
            if (MaterialProperty.type != p.MaterialProperty.type) return;
            MaterialHelper.CopyMaterialValueFromProperty(MaterialProperty, p.MaterialProperty);
            if (keyword != null) SetKeyword(ActiveShaderEditor.Materials, m.GetFloat(p.MaterialProperty.name) == 1);
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

    public class TextureProperty : ShaderProperty
    {
        public bool showFoldoutProperties = false;
        public bool hasFoldoutProperties = false;
        public bool hasScaleOffset = false;
        public string VRAMString = "";
        bool _isVRAMDirty = true;

        public TextureProperty(ShaderEditor shaderEditor, MaterialProperty materialProperty, string displayName, int xOffset, string optionsRaw, bool hasScaleOffset, bool forceThryUI, int property_index) : base(shaderEditor, materialProperty, displayName, xOffset, optionsRaw, false, property_index)
        {
            doCustomDrawLogic = forceThryUI;
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
            this.doCustomDrawLogic = !this._hasDrawer;
            if (this._isVRAMDirty)
            {
                UpdateVRAM();
                _isVRAMDirty = false;
            }
        }

        public override void DrawDefault()
        {
            Rect pos = GUILayoutUtility.GetRect(Content, Styles.vectorPropertyStyle);
            GuiHelper.ConfigTextureProperty(pos, MaterialProperty, Content, ActiveShaderEditor.Editor, hasFoldoutProperties);
            DrawingData.LastGuiObjectRect = pos;
        }

        public override void CopyFromMaterial(Material m, bool isTopCall = false)
        {
            MaterialHelper.CopyPropertyValueFromMaterial(MaterialProperty, m);
            CopyReferencePropertiesFromMaterial(m);

            if (isTopCall) ActiveShaderEditor.ApplyDrawers();
        }

        public override void CopyToMaterial(Material m, bool isTopCall = false)
        {
            MaterialHelper.CopyPropertyValueToMaterial(MaterialProperty, m);
            CopyReferencePropertiesToMaterial(m);

            if (isTopCall) MaterialEditor.ApplyMaterialPropertyDrawers(m);
        }

        public override void TransferFromMaterialAndGroup(Material m, ShaderPart p, bool isTopCall = false)
        {
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

        private void CopyReferencePropertiesToMaterial(Material target)
        {
            if (Options.reference_properties != null)
                foreach (string r_property in Options.reference_properties)
                {
                    ShaderProperty property = ActiveShaderEditor.PropertyDictionary[r_property];
                    MaterialHelper.CopyPropertyValueToMaterial(property.MaterialProperty, target);
                }
        }

        private void CopyReferencePropertiesFromMaterial(Material source)
        {
            if (Options.reference_properties != null)
                foreach (string r_property in Options.reference_properties)
                {
                    ShaderProperty property = ActiveShaderEditor.PropertyDictionary[r_property];
                    MaterialHelper.CopyPropertyValueFromMaterial(property.MaterialProperty, source);
                }
        }
    }

    public class ShaderHeaderProperty : ShaderPart
    {
        public ShaderHeaderProperty(ShaderEditor shaderEditor, MaterialProperty materialProperty, string displayName, int xOffset, string optionsRaw, bool forceOneLine) : base(shaderEditor, materialProperty, xOffset, displayName, optionsRaw)
        {
            // guid is defined as <guid:x*>
            if(displayName.Contains("<guid="))
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

        public override void HandleRightClickToggles(bool isInHeader)
        {

        }

        public override void DrawInternal(GUIContent content, CRect rect = null, bool useEditorIndent = false, bool isInHeader = false)
        {
            if (rect == null)
            {
                if (Options.texture != null && Options.texture.name != null)
                {
                    //is texutre draw
                    content = new GUIContent(Options.texture.loaded_texture, content.tooltip);
                    int height = Options.texture.height;
                    int width = (int)((float)Options.texture.loaded_texture.width / Options.texture.loaded_texture.height * height);
                    Rect control = EditorGUILayout.GetControlRect(false, height-18);
                    Rect r = new Rect((control.width-width)/2,control.y,width, height);
                    GUI.DrawTexture(r, Options.texture.loaded_texture);
                }
            }
            else
            {
                //is text draw
                Rect headerrect = new Rect(0, rect.r.y, rect.r.width, 18);
                EditorGUI.LabelField(headerrect, "<size=16>" + this.Content.text + "</size>", Styles.masterLabel);
                DrawingData.LastGuiObjectRect = headerrect;
            }
        }

        public override void CopyFromMaterial(Material m, bool isTopCall = false)
        {
            throw new System.NotImplementedException();
        }

        public override void CopyToMaterial(Material m, bool isTopCall = false)
        {
            throw new System.NotImplementedException();
        }

        public override void TransferFromMaterialAndGroup(Material m, ShaderPart p, bool isTopCall = false)
        {
            throw new System.NotImplementedException();
        }

        public override void FindUnusedTextures(List<string> unusedList, bool isEnabled)
        {
        }
    }

    public class RenderQueueProperty : ShaderProperty
    {
        public RenderQueueProperty(ShaderEditor shaderEditor) : base(shaderEditor, "RenderQueue", 0, "", "Change the Queue at which the material is rendered.")
        {
            doCustomDrawLogic = true;
        }

        public override void DrawDefault()
        {
            ActiveShaderEditor.Editor.RenderQueueField();
        }

        public override void CopyFromMaterial(Material sourceM, bool isTopCall = false)
        {
            foreach (Material m in ActiveShaderEditor.Materials) m.renderQueue = sourceM.renderQueue;
        }
        public override void CopyToMaterial(Material targetM, bool isTopCall = false)
        {
            targetM.renderQueue = ActiveShaderEditor.Materials[0].renderQueue;
        }
    }
    public class VRCFallbackProperty : ShaderProperty
    {
        static string[] s_fallbackShaderTypes = { "Standard", "Toon", "Unlit", "VertexLit", "Particle", "Sprite", "Matcap", "MobileToon" };
        static string[] s_fallbackRenderTypes = { "Opaque", "Cutout", "Transparent", "Fade" };
        static string[] s_fallbackRenderTypesValues = { "", "Cutout", "Transparent", "Fade" };
        static string[] s_fallbackCullTypes = { "OneSided", "DoubleSided" };
        static string[] s_fallbackCullTypesValues = { "", "DoubleSided" };
        static string[] s_fallbackNoTypes = { "None", "Hidden" };
        static string[] s_fallbackNoTypesValues = { "", "Hidden" };
        static string[] s_vRCFallbackOptionsPopup = s_fallbackNoTypes.Union(s_fallbackShaderTypes.SelectMany(s => s_fallbackRenderTypes.SelectMany(r => s_fallbackCullTypes.Select(c => r + "/" + c).Select(rc => s + "/" + rc)))).ToArray();
        static string[] s_vRCFallbackOptionsValues = s_fallbackNoTypes.Union(s_fallbackShaderTypes.SelectMany(s => s_fallbackRenderTypesValues.SelectMany(r => s_fallbackCullTypesValues.Select(c => r + c).Select(rc => s + rc)))).ToArray();

        public VRCFallbackProperty(ShaderEditor shaderEditor) : base(shaderEditor, "VRCFallback", 0, "", "Select the shader VRChat should use when your shaders are being hidden.")
        {
            doCustomDrawLogic = true;
        }

        public override void DrawDefault()
        {
            string current = ActiveShaderEditor.Materials[0].GetTag("VRCFallback", false, "None");
            EditorGUI.BeginChangeCheck();
            int selected = EditorGUILayout.Popup("VRChat Fallback Shader", s_vRCFallbackOptionsValues.Select((f, i) => (f, i)).FirstOrDefault(f => f.f == current).i, s_vRCFallbackOptionsPopup);
            if (EditorGUI.EndChangeCheck())
                ActiveShaderEditor.Materials[0].SetOverrideTag("VRCFallback", s_vRCFallbackOptionsValues[selected]);
        }

        public override void CopyFromMaterial(Material sourceM, bool isTopCall = false)
        {
            string value = sourceM.GetTag("VRCFallback", false, "None");
            foreach (Material m in ActiveShaderEditor.Materials) m.SetOverrideTag("VRCFallback", value);
        }
        public override void CopyToMaterial(Material targetM, bool isTopCall = false)
        {
            string value = ActiveShaderEditor.Materials[0].GetTag("VRCFallback", false, "None");
            targetM.SetOverrideTag("VRCFallback", value);
        }
    }
    public class InstancingProperty : ShaderProperty
    {
        public InstancingProperty(ShaderEditor shaderEditor, MaterialProperty materialProperty, string displayName, int xOffset, string optionsRaw, bool forceOneLine) : base(shaderEditor, materialProperty, displayName, xOffset, optionsRaw, forceOneLine, 0)
        {
            doCustomDrawLogic = true;
        }

        public override void DrawDefault()
        {
            ActiveShaderEditor.Editor.EnableInstancingField();
        }
    }
    public class GIProperty : ShaderProperty
    {
        public GIProperty(ShaderEditor shaderEditor, MaterialProperty materialProperty, string displayName, int xOffset, string optionsRaw, bool forceOneLine) : base(shaderEditor, materialProperty, displayName, xOffset, optionsRaw, forceOneLine, 0)
        {
            doCustomDrawLogic = true;
        }

        public override void DrawDefault()
        {
            LightmapEmissionFlagsProperty(XOffset, false);
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
            MaterialGlobalIlluminationFlags giFlags = ActiveShaderEditor.Materials[0].globalIlluminationFlags & any_em;
            bool isMixed = false;
            for (int i = 1; i < ActiveShaderEditor.Materials.Length; i++)
            {
                if((ActiveShaderEditor.Materials[i].globalIlluminationFlags & any_em) != giFlags)
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
            foreach (Material mat in ActiveShaderEditor.Materials)
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
        public DSGIProperty(ShaderEditor shaderEditor, MaterialProperty materialProperty, string displayName, int xOffset, string optionsRaw, bool forceOneLine) : base(shaderEditor, materialProperty, displayName, xOffset, optionsRaw, forceOneLine, 0)
        {
            doCustomDrawLogic = true;
        }

        public override void DrawDefault()
        {
            ActiveShaderEditor.Editor.DoubleSidedGIField();
        }
    }
    public class LocaleProperty : ShaderProperty
    {
        public LocaleProperty(ShaderEditor shaderEditor, MaterialProperty materialProperty, string displayName, int xOffset, string optionsRaw, bool forceOneLine) : base(shaderEditor, materialProperty, displayName, xOffset, optionsRaw, forceOneLine, 0)
        {
            doCustomDrawLogic = true;
        }

        public override void DrawDefault()
        {
            ShaderEditor.Active.Locale.DrawDropdown();    
        }
    }
}