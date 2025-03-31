using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor
{
    public class PasteSpecialPopup : EditorWindow
    {
        class ShaderPartUIAdapter
        {
            public ShaderPart ShaderPart { get; private set; }
            public bool HasChildren => children.Count > 0;
            public bool IsEnabled { get; set; } = true;

            List<ShaderPartUIAdapter> children = new List<ShaderPartUIAdapter>();

            void SetChildrenEnabled(bool enabled)
            {
                if(!HasChildren)
                    return;

                foreach(var child in children)
                    child.IsEnabled = enabled;
            }

            bool IsExpanded
            {
                get => HasChildren && _isExpanded;
                set => _isExpanded = value;
            }

            bool _isExpanded = false;

            private ShaderPartUIAdapter() {}

            public ShaderPartUIAdapter(ShaderPart shaderPart)
            {
                ShaderPart = shaderPart;
                if(shaderPart is ShaderGroup group)
                {
                    foreach(var child in group.Children)
                        children.Add(new ShaderPartUIAdapter(child));
                }
            }

            public void DrawUI()
            {
                if(ShaderPart == null)
                    return;

                using(new EditorGUILayout.VerticalScope(Styles.padding2pxHorizontal1pxVertical))
                {
                    if(!HasChildren)
                    {
                        float halfViewWidth = EditorGUIUtility.currentViewWidth * 0.5f; 
                        EditorGUILayout.BeginHorizontal();
                        IsEnabled = EditorGUILayout.ToggleLeft(ShaderPart.Content, IsEnabled, Styles.upperLeft_richText_wordWrap);
                        DrawShaderProperty(ShaderPart.MaterialProperty, GUILayout.Width(halfViewWidth));
                        EditorGUILayout.EndHorizontal();
                        return;
                    }

                    EditorGUILayout.BeginHorizontal();
                    var rect = EditorGUILayout.GetControlRect();

                    var foldoutRect = new Rect(rect.x, rect.y, rect.width, rect.height);
                    var toggleRect = new Rect(rect.x + 16f, rect.y, 14f, rect.height);
                    var labelRect = new Rect(rect.x + 32f, rect.y, rect.width - 32f, rect.height);
                    EditorGUI.LabelField(rect, GUIContent.none, Styles.dropdownHeader);
                    
                    IsEnabled = EditorGUI.Toggle(toggleRect, GUIContent.none, IsEnabled);
                    IsExpanded = EditorGUI.Foldout(foldoutRect, IsExpanded, string.Empty, true);
                    EditorGUI.LabelField(labelRect, ShaderPart.Content);
                    if(GUILayout.Button("None", GUILayout.MaxWidth(40f)))
                        SetChildrenEnabled(false);
                    if(GUILayout.Button("All", GUILayout.MaxWidth(40f)))
                        SetChildrenEnabled(true);
                    EditorGUILayout.EndHorizontal();
                    if(IsExpanded)
                    {
                        EditorGUI.indentLevel++;
                        foreach(var child in children)
                            child.DrawUI();
                        EditorGUI.indentLevel--;
                    }
                }
            }

            public void AddDisabledShaderPartsToListRecursive(ref List<ShaderPart> disabledParts)
            {
                if(!IsEnabled)
                    disabledParts.Add(ShaderPart);
                
                if(HasChildren)
                    foreach(var child in children)
                        child.AddDisabledShaderPartsToListRecursive(ref disabledParts);
            }
        }
        
        Vector2 scrollPosition = Vector2.zero;
        ShaderPartUIAdapter partAdapter;
        
        /// <summary>
        /// OnPasteClicked, comes with a list of shader parts the user left enabled
        /// </summary>
        public event Action<List<ShaderPart>> OnPasteClicked;

        void Awake()
        {
            minSize = new Vector2(480, 400);
            titleContent = new GUIContent("Paste Special");
        }

        public void Init(ShaderPart shaderPart)
        {
            partAdapter = new ShaderPartUIAdapter(shaderPart);
        }

        void OnGUI()
        {
            if(partAdapter?.ShaderPart == null)
            {
                Close();
                return;
            }

            using(var scroll = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                partAdapter.DrawUI();
                scrollPosition = scroll.scrollPosition;
            }

            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Cancel", GUILayout.Height(30))) 
            {
                Close();
            }

            if(GUILayout.Button("Paste Selected", GUILayout.Height(30)))
            {
                List<ShaderPart> disabledParts = new List<ShaderPart>();
                partAdapter.AddDisabledShaderPartsToListRecursive(ref disabledParts);
                OnPasteClicked?.Invoke(disabledParts);

                Close();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        static void DrawShaderProperty(MaterialProperty prop, GUILayoutOption propertyWidth)
        {
            using(new EditorGUI.DisabledScope(true))
            {
                switch(prop.type)
                {
                    case MaterialProperty.PropType.Color:
                        EditorGUILayout.ColorField(prop.colorValue, propertyWidth);
                        break;
                    case MaterialProperty.PropType.Vector:
                        EditorGUILayout.Vector4Field(GUIContent.none, prop.vectorValue, propertyWidth);
                        break;
#if UNITY_2021_1_OR_NEWER
                    case MaterialProperty.PropType.Int:
                        EditorGUILayout.IntField(prop.intValue, propertyWidth);
                        break;
#endif
                    case MaterialProperty.PropType.Range:
                        EditorGUILayout.Slider(GUIContent.none, prop.floatValue, prop.rangeLimits.x, prop.rangeLimits.y,
                            propertyWidth);
                        break;
                    case MaterialProperty.PropType.Float:
                        EditorGUILayout.FloatField(prop.floatValue, propertyWidth);
                        break;
                    case MaterialProperty.PropType.Texture:
                        EditorGUILayout.ObjectField(prop.textureValue, typeof(Texture), true, propertyWidth);
                        break;
                    default:
                        break;
                }
            }
        }        
    }
}
