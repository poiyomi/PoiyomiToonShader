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

        public EventType OriginalEventType;

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
            OriginalEventType = e.type;
        }

        public void Use()
        {
            _MouseClick = false;
            _MouseLeftClick = false;
            _MouseRightClick = false;
            Event.current.Use();
        }

        // This is cursed. I need to look over this whole system at some point
        public void PowerUse()
        {
            Use();
            _MouseRightClickIgnoreLocked = false;
            _MouseLeftClickIgnoreLocked = false;
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
    }

    public abstract class ShaderPart
    {
        public ShaderEditor ActiveShaderEditor { protected set; get; }
        public MaterialProperty MaterialProperty { private set; get; }

        public GUIContent Content { protected set; get; }
        public BetterTooltips.Tooltip Tooltip { protected set; get; }
        public System.Object PropertyData { protected set; get; } = null;

        public string  PropertyIdentifier { protected set; get; }
        public string CustomStringTagID { protected set; get; } = null;

        public bool IsHidden { protected set; get; } = false;
        public bool IsPreset { protected set; get; } = false;

        public bool IsExemptFromLockedDisabling { protected set; get; } = false;
        public bool IsAnimatable { protected set; get; } = true;
        public bool IsAnimated { protected set; get; } = false;
        public bool IsRenaming { protected set; get; } = false;



        public bool DoReferencePropertiesExist { protected set; get; } = false;
        public bool DoesReferencePropertyExist { protected set; get; } = false;

        public int ShaderPropertyId { protected set; get; } = -1;
        public int ShaderPropertyIndex { protected set; get; } = -1;


        public bool has_not_searchedFor = false; //used for property search

        GenericMenu _contextMenu;

        protected string _optionsRaw;
        private bool _doOptionsNeedInitilization = true;

        private PropertyOptions _options;
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

        private int _xoffset = 0;
        private int _tempXOffset = -1;
        public int XOffset
        {
            protected set
            {
                _xoffset = value;
            }
            get
            {
                if (_tempXOffset != -1) return _tempXOffset;
                return _xoffset;
            }
        }

        public void SetTemporaryXOffset(int value)
        {
            _tempXOffset = value;
        }

        public void ResetTemporaryXOffset()
        {
            _tempXOffset = -1;
        }

        public void SetIsExemptFromLockedDisabling(bool b)
        {
            IsExemptFromLockedDisabling = b;
        }


        public ShaderPart(string propertyIdentifier, int xOffset, string displayName, string tooltip, ShaderEditor shaderEditor)
        {
            this._optionsRaw = null;
            this.ActiveShaderEditor = shaderEditor;
            this.PropertyIdentifier = propertyIdentifier;
            this.XOffset = xOffset;
            this.Content = new GUIContent(displayName);
            this.Tooltip = new BetterTooltips.Tooltip(tooltip);
            this.IsPreset = shaderEditor.IsPresetEditor && Presets.IsPreset(shaderEditor.Materials[0], this);
        }

        public ShaderPart(ShaderEditor shaderEditor, MaterialProperty prop, int xOffset, string displayName, string optionsRaw, int propertyIndex)
        {
            this._optionsRaw = optionsRaw;
            this.ActiveShaderEditor = shaderEditor;
            this.MaterialProperty = prop;
            this.XOffset = xOffset;
            this.Content = new GUIContent(displayName);
            this.IsPreset = shaderEditor.IsPresetEditor && Presets.IsPreset(shaderEditor.Materials[0], this);

            if (MaterialProperty == null)
                return;

            this.ShaderPropertyId = Shader.PropertyToID(MaterialProperty.name);
            this.ShaderPropertyIndex = propertyIndex;

            // Do parse options & check for alternative names if shader swap
            if (ShaderEditor.Active.DidSwapToNewShader)
            {
                if (Options.alts != null && Options.alts.Length > 0)
                    CopyAlternativeUpgradeValues();
            }

            this.IsExemptFromLockedDisabling |= ShaderOptimizer.IsPropertyExcemptFromLocking(prop);
        }

        protected void UpdatedMaterialPropertyReference()
        {
            this.MaterialProperty = ActiveShaderEditor.Properties[ShaderPropertyIndex];
        }

        private void CopyAlternativeUpgradeValues()
        {
            MaterialProperty.PropType type = this.MaterialProperty.type;
            if (type == MaterialProperty.PropType.Color) type = MaterialProperty.PropType.Vector;
            if (type == MaterialProperty.PropType.Range) type = MaterialProperty.PropType.Float;

            int index = ShaderEditor.Active.Shader.FindPropertyIndex(this.MaterialProperty.name);

            object defaultValue = null;
            if (type == MaterialProperty.PropType.Float)
                defaultValue = ShaderEditor.Active.Shader.GetPropertyDefaultFloatValue(index);
#if UNITY_2022_1_OR_NEWER
            else if (type == MaterialProperty.PropType.Int)
                defaultValue = ShaderEditor.Active.Shader.GetPropertyDefaultIntValue(index);
#endif
            else if (type == MaterialProperty.PropType.Vector)
                defaultValue = ShaderEditor.Active.Shader.GetPropertyDefaultVectorValue(index);
            else if (type == MaterialProperty.PropType.Texture)
                defaultValue = ShaderEditor.Active.Shader.GetPropertyTextureDefaultName(index);

            foreach (Material m in ShaderEditor.Active.Materials)
            {
                // Check if is not default value
                if (type == MaterialProperty.PropType.Float)
                {
                    if (m.GetNumber(this.MaterialProperty) != (float)defaultValue)
                        continue;
                }
#if UNITY_2022_1_OR_NEWER
                else if (type == MaterialProperty.PropType.Int)
                {
                    if (m.GetInt(this.MaterialProperty.name) != (int)defaultValue)
                        continue;
                }
#endif
                else if (type == MaterialProperty.PropType.Vector)
                {
                    if (m.GetVector(this.MaterialProperty.name) != (Vector4)defaultValue)
                        continue;
                }
                else if (type == MaterialProperty.PropType.Texture)
                {
                    if (m.GetTexture(this.MaterialProperty.name) != null &&
                        m.GetTexture(this.MaterialProperty.name).name != (string)defaultValue)
                        continue;
                }

                // Material as serializedObject
                SerializedObject serializedObject = new SerializedObject(m);
                foreach (string alt in Options.alts)
                {
                    SerializedProperty arrayProp = null;
                    if (type == MaterialProperty.PropType.Float)
                        arrayProp = serializedObject.FindProperty("m_SavedProperties.m_Floats.Array");
#if UNITY_2022_1_OR_NEWER
                    else if (type == MaterialProperty.PropType.Int)
                        arrayProp = serializedObject.FindProperty("m_SavedProperties.m_Ints.Array");
#endif
                    else if (type == MaterialProperty.PropType.Vector)
                        arrayProp = serializedObject.FindProperty($"m_SavedProperties.m_Colors.Array");
                    else if (type == MaterialProperty.PropType.Texture)
                        arrayProp = serializedObject.FindProperty($"m_SavedProperties.m_TexEnvs.Array");

                    if (arrayProp == null)
                        continue;

                    // Iterate through properties in prop array, find where .first is alt
                    SerializedProperty valueProp = null;
                    for (int i = 0; i < arrayProp.arraySize; i++)
                    {
                        SerializedProperty keyProp = arrayProp.GetArrayElementAtIndex(i);
                        if (keyProp.FindPropertyRelative("first").stringValue == alt)
                        {
                            valueProp = keyProp.FindPropertyRelative("second");
                            break;
                        }
                    }

                    if (valueProp == null)
                        continue;

                    if (type == MaterialProperty.PropType.Float)
                        this.MaterialProperty.floatValue = valueProp.floatValue;
#if UNITY_2022_1_OR_NEWER
                    else if (type == MaterialProperty.PropType.Int)
                        this.MaterialProperty.intValue = valueProp.intValue;
#endif
                    else if (type == MaterialProperty.PropType.Vector)
                        this.MaterialProperty.colorValue = valueProp.colorValue;
                    else if (type == MaterialProperty.PropType.Texture)
                    {
                        var texProperty = valueProp.FindPropertyRelative("m_Texture").objectReferenceValue as Texture;
                        var scaleProperty = valueProp.FindPropertyRelative("m_Scale").vector2Value;
                        var offsetProperty = valueProp.FindPropertyRelative("m_Offset").vector2Value;

                        this.MaterialProperty.textureValue = texProperty;
                        this.MaterialProperty.textureScaleAndOffset = new Vector4(scaleProperty.x, scaleProperty.y, offsetProperty.x, offsetProperty.y);
                    }
                }
            }
        }

        protected virtual void InitOptions()
        {
            this.Tooltip = new BetterTooltips.Tooltip(Options.tooltip);
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
            this.Tooltip.SetText(tooltip);
        }

        public abstract void DrawInternal(GUIContent content, Rect? rect = null, bool useEditorIndent = false, bool isInHeader = false);
        public abstract void CopyFromMaterial(Material m, bool isTopCall = false);
        public abstract void CopyToMaterial(Material m, bool isTopCall = false, MaterialProperty.PropType[] skipPropertyTypes = null);

        protected void CopyReferencePropertiesToMaterial(Material target)
        {
            if (Options.reference_properties != null)
                foreach (string r_property in Options.reference_properties)
                {
                    ShaderProperty property = ActiveShaderEditor.PropertyDictionary[r_property];
                    MaterialHelper.CopyPropertyValueToMaterial(property.MaterialProperty, target);
                }
            if (string.IsNullOrWhiteSpace(Options.reference_property) == false)
            {
                ShaderProperty property = ActiveShaderEditor.PropertyDictionary[Options.reference_property];
                MaterialHelper.CopyPropertyValueToMaterial(property.MaterialProperty, target);
            }
        }

        protected void CopyReferencePropertiesFromMaterial(Material source)
        {
            if (Options.reference_properties != null)
                foreach (string r_property in Options.reference_properties)
                {
                    ShaderProperty property = ActiveShaderEditor.PropertyDictionary[r_property];
                    MaterialHelper.CopyPropertyValueFromMaterial(property.MaterialProperty, source);
                }
            if (string.IsNullOrWhiteSpace(Options.reference_property) == false)
            {
                ShaderProperty property = ActiveShaderEditor.PropertyDictionary[Options.reference_property];
                MaterialHelper.CopyPropertyValueFromMaterial(property.MaterialProperty, source);
            }
        }

        public abstract void TransferFromMaterialAndGroup(Material m, ShaderPart g, bool isTopCall = false, MaterialProperty.PropType[] propertyTypesToSkip = null);

        bool hasAddedDisabledGroup = false;
        public void Draw(Rect? rect = null, GUIContent content = null, bool useEditorIndent = false, bool isInHeader = false)
        {
            if (_doOptionsNeedInitilization)
            {
                InitOptions();
                _doOptionsNeedInitilization = false;
            }

            if (has_not_searchedFor)
                return;
            if (DrawingData.IsEnabled && Options.condition_enable != null)
            {
                hasAddedDisabledGroup = !Options.condition_enable.Test();
            }
            if (hasAddedDisabledGroup)
            {
                DrawingData.IsEnabled = !hasAddedDisabledGroup;
                EditorGUI.BeginDisabledGroup(true);
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
            if (DrawingData.TooltipCheckRect.y < 25) return; // Happens in Layout event, with some dynamic properties
            if (ShaderEditor.Input.RightClick_IgnoreLockedAndUnityUses && ShaderEditor.Input.OriginalEventType != EventType.Layout && DrawingData.TooltipCheckRect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                //Context menu
                //Show context menu, if not open.
                //If locked material only show menu for animated materials. Only show data retieving options in locked state
                if (!ShaderEditor.Active.IsLockedMaterial || IsAnimated)
                {
                    _contextMenu = new GenericMenu();
                    if (IsAnimatable && !ShaderEditor.Active.IsLockedMaterial)
                    {
                        _contextMenu.AddItem(new GUIContent("Animated (when locked)"), IsAnimated, () => { SetAnimated(!IsAnimated, false); });
                        _contextMenu.AddItem(new GUIContent("Renamed (when locked)"), IsAnimated && IsRenaming, () => { SetAnimated(true, !IsRenaming); });
                        _contextMenu.AddItem(new GUIContent("Locking Explanation"), false, () => { Application.OpenURL("https://www.youtube.com/watch?v=asWeDJb5LAo&ab_channel=poiyomi"); });
                        _contextMenu.AddSeparator("");
                    }
                    if (ShaderEditor.Active.IsPresetEditor)
                    {
                        _contextMenu.AddItem(new GUIContent("Is part of preset"), IsPreset, ToggleIsPreset);
                        _contextMenu.AddSeparator("");
                    }
                    _contextMenu.AddItem(new GUIContent("Copy Property Name"), false, () => { EditorGUIUtility.systemCopyBuffer = MaterialProperty.name; });
                    _contextMenu.AddItem(new GUIContent("Copy Animated Property Name"), false, () => { EditorGUIUtility.systemCopyBuffer = GetAnimatedPropertyName(); });
                    _contextMenu.AddItem(new GUIContent("Copy Animated Property Path"), false, CopyPropertyPath);
                    _contextMenu.AddItem(new GUIContent("Copy Property as Keyframe"), false, CopyPropertyAsKeyframe);
#if UNITY_2022_1_OR_NEWER
                    bool isLockedInChildren = false;
                    bool isLockedByAncestor = false;
                    bool isOverriden = true;
                    foreach (Material target in ShaderEditor.Active.Materials)
                    {
                        if (target == null) continue;
                        int nameId = Shader.PropertyToID(MaterialProperty.name);
                        isLockedInChildren |= target.IsPropertyLocked(nameId);
                        isLockedByAncestor |= target.IsPropertyLockedByAncestor(nameId);
                        isOverriden &= target.IsPropertyOverriden(nameId);
                    }
                    DoVariantMenuStuff(_contextMenu, isOverriden, isLockedByAncestor, isLockedInChildren, ShaderEditor.Active.Materials, true);
#endif
                    _contextMenu.ShowAsContext();
                }
            }
        }

#if UNITY_2022_1_OR_NEWER
        // static Type s_PropertyData = typeof(MaterialProperty).GetNestedType("PropertyData", BindingFlags.NonPublic);
        // static MethodInfo s_HandleApplyRevert = s_PropertyData.GetMethod("HandleApplyRevert", BindingFlags.NonPublic | BindingFlags.Static);

        void DoVariantMenuStuff(GenericMenu menu, bool overriden, bool lockedByAncestor, bool isLockedInChildren, Material[] targets, bool allowLocking)
        {
            if (lockedByAncestor)
            {
                if (targets.Length != 1)
                    return;

                _contextMenu.AddSeparator("");
                menu.AddItem(Styles.lockOriginContent, false, () => GotoLockOriginAction(targets));
            }
            else if (GUI.enabled)
            {
                DoRegularMenu(menu, overriden, targets);
                DoLockPropertiesMenu(_contextMenu, !isLockedInChildren, ShaderEditor.Active.Materials, true);
            }
        }

        void DoRegularMenu(GenericMenu menu, bool isOverriden, Material[] targets)
        {
            var singleEditing = targets.Length == 1;

            if (isOverriden)
            {
                _contextMenu.AddSeparator("");
                HandleApplyRevert(menu, singleEditing, targets);
            }

            DisplayMode displayMode = GetDisplayMode(targets);
            if (displayMode == DisplayMode.Material)
            {
                menu.AddSeparator("");
                menu.AddItem(Styles.resetContent, false, ResetMaterialProperties);
            }
            else if (displayMode == DisplayMode.Variant)
                HandleRevertAll(menu, singleEditing, targets);
        }

        enum DisplayMode { Material, Variant, Mixed };
        static DisplayMode GetDisplayMode(Material[] targets)
        {
            int variantCount = GetVariantCount(targets);
            if (variantCount == 0)
                return DisplayMode.Material;
            if (variantCount == targets.Length)
                return DisplayMode.Variant;
            return DisplayMode.Mixed;
        }

        static int GetVariantCount(Material[] targets)
        {
            int count = 0;
            foreach (Material target in targets)
                count += target.isVariant ? 1 : 0;
            return count;
        }

        void DoLockPropertiesMenu(GenericMenu menu, bool lockValue, Material[] targets, bool allowLocking)
        {
            if (menu.GetItemCount() != 0)
                menu.AddSeparator("");

            if (allowLocking)
            {
                menu.AddItem(Styles.lockContent, !lockValue, () => { SetLockedProperty(targets, lockValue); });
            }
            else
            {
                menu.AddDisabledItem(Styles.lockContent);
            }
        }

        void SetLockedProperty(Material[] targets, bool value)
        {
            foreach (Material target in targets)
            {
                target.SetPropertyLock(ShaderPropertyId, value);
            }
        }

        void GotoLockOriginAction(Material[] targets)
        {
            Material origin = targets[0] as Material;
            while (origin = origin.parent)
            {
                if (origin.IsPropertyLocked(ShaderPropertyId))
                    break;
            }

            if (origin)
            {
                EditorGUIUtility.PingObject(origin);
            }
        }

        void HandleApplyRevert(GenericMenu menu, bool singleEditing, Material[] targets)
        {
            // Apply
            if (singleEditing)
            {
                Material source = (Material)targets[0];
                Material destination = (Material)targets[0];
                while (destination = destination.parent as Material)
                {
                    if (AssetDatabase.IsForeignAsset(destination))
                        continue;

                    var text = destination.isVariant ? Styles.applyToVariantText : Styles.applyToMaterialText;
                    var applyContent = new GUIContent(string.Format(text, destination.name));

                    menu.AddItem(applyContent, false, (object dest) =>
                    {
                        source.ApplyPropertyOverride((Material)dest, ShaderPropertyId);
                    }, destination);
                }
            }

            // Revert
            var content = singleEditing ? Styles.revertContent :
                new GUIContent(string.Format(Styles.revertMultiText, targets.Length));
            menu.AddItem(content, false, () =>
            {
                string displayName = MaterialProperty.displayName;
                string targetName = singleEditing ? targets[0].name : targets.Length + " Materials";
                Undo.RecordObjects(targets, "Revert " + displayName + " of " + targetName);

                foreach (Material target in targets)
                {
                    target.RevertPropertyOverride(ShaderPropertyId);
                }
            });
        }

        void HandleRevertAll(GenericMenu menu, bool singleEditing, Material[] targets)
        {
            foreach (Material target in targets)
            {
                if (target.isVariant)
                {
                    menu.AddSeparator("");

                    menu.AddItem(Styles.revertAllContent, false, () =>
                    {
                        string targetName = singleEditing ? targets[0].name : targets.Length + " Materials";
                        Undo.RecordObjects(targets, "Revert all overrides of " + targetName);

                        foreach (Material target in targets)
                            target.RevertAllPropertyOverrides();
                    });
                    break;
                }
            }
        }

        void ResetMaterialProperties()
        {
            MaterialProperty prop = MaterialProperty;
            Shader shader = ShaderEditor.Active.Shader;
            switch (prop.type)
            {
                case MaterialProperty.PropType.Float:
                case MaterialProperty.PropType.Range:
                    prop.floatValue = shader.GetPropertyDefaultFloatValue(ShaderPropertyIndex);
                    break;
                case MaterialProperty.PropType.Vector:
                    prop.vectorValue = shader.GetPropertyDefaultVectorValue(ShaderPropertyIndex);
                    break;
                case MaterialProperty.PropType.Color:
                    prop.colorValue = shader.GetPropertyDefaultVectorValue(ShaderPropertyIndex);
                    break;
                case MaterialProperty.PropType.Int:
                    prop.intValue = shader.GetPropertyDefaultIntValue(ShaderPropertyIndex);
                    break;
                case MaterialProperty.PropType.Texture:
                    Texture texture = null;
                    var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(shader)) as ShaderImporter;
                    if (importer != null)
                        texture = importer.GetDefaultTexture(prop.name);
                    prop.textureValue = texture;
                    prop.textureScaleAndOffset = new Vector4(1, 1, 0, 0);
                    break;
            }
        }
#endif

        void ToggleIsPreset()
        {
            IsPreset = !IsPreset;
            if (MaterialProperty != null) Presets.SetProperty(ActiveShaderEditor.Materials[0], this, IsPreset);
            ShaderEditor.RepaintActive();
        }

        void CopyPropertyPath()
        {
            string path = GetAnimatedPropertyName();
            Transform selected = Selection.activeTransform;
            Transform root = selected;
            while (root != null && root.GetComponent<Animator>() == null)
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
            if (selected == null && root == null)
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
#if UNITY_2022_1_OR_NEWER
            else if (MaterialProperty.type == MaterialProperty.PropType.Int)
            {
                clip.SetCurve(path, rendererType, propertyname, new AnimationCurve(new Keyframe(0, MaterialProperty.intValue)));
                keyframeList.Add(ClipToKeyFrame(animationCurveType, clip, path, "", rendererType));
            }
#endif
            else if (MaterialProperty.type == MaterialProperty.PropType.Color)
            {
                clip.SetCurve(path, rendererType, propertyname + ".r", new AnimationCurve(new Keyframe(0, MaterialProperty.colorValue.r)));
                clip.SetCurve(path, rendererType, propertyname + ".g", new AnimationCurve(new Keyframe(0, MaterialProperty.colorValue.g)));
                clip.SetCurve(path, rendererType, propertyname + ".b", new AnimationCurve(new Keyframe(0, MaterialProperty.colorValue.b)));
                clip.SetCurve(path, rendererType, propertyname + ".a", new AnimationCurve(new Keyframe(0, MaterialProperty.colorValue.a)));
                keyframeList.Add(ClipToKeyFrame(animationCurveType, clip, path, ".r", rendererType));
                keyframeList.Add(ClipToKeyFrame(animationCurveType, clip, path, ".g", rendererType));
                keyframeList.Add(ClipToKeyFrame(animationCurveType, clip, path, ".b", rendererType));
                keyframeList.Add(ClipToKeyFrame(animationCurveType, clip, path, ".a", rendererType));
            }
            else if (MaterialProperty.type == MaterialProperty.PropType.Vector)
            {
                clip.SetCurve(path, rendererType, propertyname + ".x", new AnimationCurve(new Keyframe(0, MaterialProperty.vectorValue.x)));
                clip.SetCurve(path, rendererType, propertyname + ".y", new AnimationCurve(new Keyframe(0, MaterialProperty.vectorValue.y)));
                clip.SetCurve(path, rendererType, propertyname + ".z", new AnimationCurve(new Keyframe(0, MaterialProperty.vectorValue.z)));
                clip.SetCurve(path, rendererType, propertyname + ".w", new AnimationCurve(new Keyframe(0, MaterialProperty.vectorValue.w)));
                keyframeList.Add(ClipToKeyFrame(animationCurveType, clip, path, ".x", rendererType));
                keyframeList.Add(ClipToKeyFrame(animationCurveType, clip, path, ".y", rendererType));
                keyframeList.Add(ClipToKeyFrame(animationCurveType, clip, path, ".z", rendererType));
                keyframeList.Add(ClipToKeyFrame(animationCurveType, clip, path, ".w", rendererType));
            }
            else if (MaterialProperty.type == MaterialProperty.PropType.Texture)
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

        private void PerformDraw(GUIContent content, Rect? rect, bool useEditorIndent, bool isInHeader = false)
        {
            if (content == null)
                content = this.Content;
            EditorGUI.BeginChangeCheck();

            DrawingData.IconsPositioningCount = 0;

            DrawInternal(content, rect, useEditorIndent, isInHeader);

            if (this is ShaderTextureProperty == false)
            {
                DrawingData.TooltipCheckRect = DrawingData.LastGuiObjectRect;
                if (DrawingData.IconsPositioningCount == 0)
                {
                    DrawingData.IconsPositioningCount = 1;
                    DrawingData.IconsPositioningHeights[0] = DrawingData.LastGuiObjectRect.y + DrawingData.LastGuiObjectRect.height - 14;
                }
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
                        AnimationMode.IsPropertyAnimated(ActiveShaderEditor.ActiveRenderer, "material." + MaterialProperty.name + "_ST.x") :
                        AnimationMode.IsPropertyAnimated(ActiveShaderEditor.ActiveRenderer, "material." + MaterialProperty.name))
                        SetAnimated(true, false);
                }
            }

            if (IsAnimatable && IsAnimated) DrawLockedAnimated();
            if (IsPreset) DrawPresetProperty();

            Tooltip.ConditionalDraw(DrawingData.TooltipCheckRect);

            //Click testing
            if (Event.current.type == EventType.MouseDown && DrawingData.LastGuiObjectRect.Contains(ShaderEditor.Input.mouse_position))
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
            for (int i = 0; i < DrawingData.IconsPositioningCount; i++)
            {
                Rect r = new Rect(14, DrawingData.IconsPositioningHeights[i], 16, 16);
                if (IsRenaming) GUI.Label(r, "RA", Styles.animatedIndicatorStyle);
                else GUI.Label(r, "A", Styles.animatedIndicatorStyle);
            }
        }

        private void DrawPresetProperty()
        {
            for (int i = 0; i < DrawingData.IconsPositioningCount; i++)
            {
                Rect r = new Rect(3, DrawingData.IconsPositioningHeights[i], 8, 16);
                GUI.Label(r, "P", Styles.presetIndicatorStyle);
            }
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

        protected bool ShouldSkipProperty(MaterialProperty property, MaterialProperty.PropType[] propertyTypesToSkip)
        {
            if (propertyTypesToSkip != null)
                foreach (MaterialProperty.PropType typeToSkip in propertyTypesToSkip)
                    if (property.type == typeToSkip)
                        return true;
            return false;
        }
    }
}