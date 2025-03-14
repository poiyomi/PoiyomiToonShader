using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using static UnityEditor.MaterialProperty;

namespace Thry.ThryEditor
{
    public class ShaderGroup : ShaderPart
    {
        public override bool IsPropertyValueDefault
        {
            get
            {
                if(_isPropertyValueDefault == null)
                {
                    _isPropertyValueDefault = Children.All(p => p.IsPropertyValueDefault);
                }
                return _isPropertyValueDefault.Value;
            }
        }

        private List<ShaderPart> _children = new List<ShaderPart>();
        private ReadOnlyCollection<ShaderPart> _readonlychildren => new ReadOnlyCollection<ShaderPart>(_children);
        [PublicAPI]
        public ReadOnlyCollection<ShaderPart> Children => _readonlychildren;

        protected bool _isExpanded;
        private bool _isSearchExpanded;

        public ShaderGroup(ShaderEditor shaderEditor) : base(null, 0, "", null, shaderEditor)
        {

        }

        public ShaderGroup(ShaderEditor shaderEditor, MaterialProperty prop, MaterialEditor materialEditor, string displayName, int xOffset, string optionsRaw, int propertyIndex) : base(shaderEditor, prop, xOffset, displayName, optionsRaw, propertyIndex)
        {
            PropertyValueChanged += (PropertyValueEventArgs args) => 
            {
                if(!_doOptionsNeedInitilization && Options.persistent_expand)
                    _isExpanded = this.MaterialProperty.GetNumber() == 1;
            };
        }

        protected override void InitOptions()
        {
            base.InitOptions();
            if (Options.persistent_expand) _isExpanded = this.MaterialProperty.GetNumber() == 1;
            else _isExpanded = Options.default_expand;
        }

        protected bool IsExpanded
        {
            get
            {
                return ShaderEditor.Active.IsInSearchMode ? _isSearchExpanded : _isExpanded;
            }
            set
            {
                if(ShaderEditor.Active.IsInSearchMode)
                {
                    _isSearchExpanded = value;
                    return;
                }
                if (Options.persistent_expand)
                {
                    if (AnimationMode.InAnimationMode())
                    {
#if UNITY_2020_1_OR_NEWER
                        // So we do this instead
                        _isExpanded = value;
#else
                        // This fails when unselecting the object in hirearchy
                        // Then reselecting it
                        // Don't know why
                        // It seems AnimationMode is not working properly in Unity 2022
                        // It worked fine in Unity 2019
                        
                        AnimationMode.StopAnimationMode();
                        this.MaterialProperty.SetNumber(value ? 1 : 0);
                        Undo.SetCurrentGroupName((value ? "Expand" : "Collapse") + $" {Content.text} of {ShaderEditor.Active.TargetName}");
                        RaisePropertyValueChanged();
                        AnimationMode.StartAnimationMode();
#endif
                    }
                    else
                    {
                        this.MaterialProperty.SetNumber(value ? 1 : 0);
                        Undo.SetCurrentGroupName((value ? "Expand" : "Collapse") + $" {Content.text} of {ShaderEditor.Active.TargetName}");
                        RaisePropertyValueChanged();
                    }
                }
                _isExpanded = value;
            }
        }

        public void SetSearchExpanded(bool value)
        {
            _isSearchExpanded = value;
        }

        protected bool DoDisableChildren
        {
            get
            {
                return Options.condition_enable_children != null && !Options.condition_enable_children.Test();
            }
        }

        public void AddPart(ShaderPart part)
        {
            part.SetParent(this);
            _children.Add(part);
        }

        public override void CopyFrom(Material src, bool applyDrawers = true, bool deepCopy = true, HashSet<PropType> skipPropertyTypes = null, HashSet<string> skipPropertyNames = null)
        {
            if(skipPropertyNames?.Contains(MaterialProperty.name) == true) return;
            CopyReferencePropertiesFrom(src, skipPropertyTypes, skipPropertyNames);

            if(deepCopy)
                foreach (ShaderPart p in Children)
                    p.CopyFrom(src, false, true, skipPropertyTypes, skipPropertyNames);

            if (applyDrawers) MyShaderUI.ApplyDrawers();
        }

        public override void CopyFrom(ShaderPart srcPart, bool applyDrawers = true, bool deepCopy = true, HashSet<PropType> skipPropertyTypes = null, HashSet<string> skipPropertyNames = null)
        {
            if(skipPropertyNames?.Contains(MaterialProperty.name) == true) return;
            if(skipPropertyNames?.Contains(srcPart.MaterialProperty.name) == true) return;
            if (srcPart is ShaderGroup == false) return;
            ShaderGroup src = srcPart as ShaderGroup;
            CopyReferencePropertiesFrom(src, skipPropertyTypes, skipPropertyNames);

            for (int i = 0; deepCopy && i < src.Children.Count && i < Children.Count; i++)
                Children[i].CopyFrom(src.Children[i], false, true, skipPropertyTypes, skipPropertyNames);

            if (applyDrawers) MyShaderUI.ApplyDrawers();
        }

        public override void CopyTo(Material[] targets, bool applyDrawers = true, bool deepCopy = true, HashSet<PropType> skipPropertyTypes = null, HashSet<string> skipPropertyNames = null)
        {
            if(skipPropertyNames?.Contains(MaterialProperty.name) == true) return;
            CopyReferencePropertiesTo(targets, skipPropertyTypes, skipPropertyNames);

            if(deepCopy)
                foreach (ShaderPart p in Children)
                    p.CopyTo(targets, false, true, skipPropertyTypes, skipPropertyNames);

            if (applyDrawers) MaterialEditor.ApplyMaterialPropertyDrawers(targets);
        }

        public override void CopyTo(ShaderPart targetPart, bool applyDrawers = true, bool deepCopy = true, HashSet<PropType> skipPropertyTypes = null, HashSet<string> skipPropertyNames = null)
        {
            if(skipPropertyNames?.Contains(MaterialProperty.name) == true) return;
            if(skipPropertyNames?.Contains(targetPart.MaterialProperty.name) == true) return;
            if (targetPart is ShaderGroup == false) return;
            ShaderGroup target = targetPart as ShaderGroup;
            CopyReferencePropertiesTo(target, skipPropertyTypes, skipPropertyNames);
            
            for(int i = 0; deepCopy && i < Children.Count && i < target.Children.Count; i++)
                Children[i].CopyTo(target.Children[i], false, true, skipPropertyTypes, skipPropertyNames);

            if (applyDrawers) MaterialEditor.ApplyMaterialPropertyDrawers(target.MaterialProperty.targets);
        }

        protected override void DrawInternal(GUIContent content, Rect? rect = null, bool useEditorIndent = false, bool isInHeader = false)
        {
            if (Options.margin_top > 0)
            {
                GUILayoutUtility.GetRect(0, Options.margin_top);
            }
            foreach (ShaderPart part in Children)
            {
                part.Draw();
            }
        }

        public override void FindUnusedTextures(List<string> unusedList, bool isEnabled)
        {
            if (isEnabled && Options.condition_enable != null)
            {
                isEnabled &= Options.condition_enable.Test();
            }
            foreach (ShaderPart p in (this as ShaderGroup).Children)
                p.FindUnusedTextures(unusedList, isEnabled);
        }

        protected void UpdateLinkedMaterials()
        {
            if(ShaderEditor.Active.IsInAnimationMode) return;
            IEnumerable<Material> linked_materials = MaterialLinker.GetLinked(MaterialProperty);
            if (linked_materials != null)
                this.CopyTo(linked_materials.ToArray());
        }

        protected void FoldoutArrow(Rect rect, Event e)
        {
            if (e.type == EventType.Repaint)
            {
                Rect arrowRect = new RectOffset(4, 0, 0, 0).Remove(rect);
                arrowRect.width = 13;
                EditorStyles.foldout.Draw(arrowRect, false, false, IsExpanded, false);
            }
        }

        public override bool Search(string searchTerm, List<ShaderGroup> foundHeaders, bool isParentInSearch = false)
        {
            bool found = isParentInSearch
                || this.Content.text.IndexOf(searchTerm, System.StringComparison.OrdinalIgnoreCase) >= 0
                || this.MaterialProperty?.name.IndexOf(searchTerm, System.StringComparison.OrdinalIgnoreCase) >= 0;
            bool foundInChild = false;
            foreach (ShaderPart p in Children)
            {
                if (p.Search(searchTerm, foundHeaders, isParentInSearch || found))
                    foundInChild = true;
            }
            found |= foundInChild;
            if(found && this is ShaderHeader) foundHeaders.Add(this);
            this.has_not_searchedFor = !found;
            return found;
        }
    }

}