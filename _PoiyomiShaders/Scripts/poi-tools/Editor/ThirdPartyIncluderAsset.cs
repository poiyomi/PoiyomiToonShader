using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using UnityEditorInternal;
using System.Collections.Generic;

namespace Poi
{
    [CreateAssetMenu(fileName = "ThirdPartyIncluderAsset", menuName = "Poi/ThirdPartyIncluderAsset", order = 2)]
    public class ThirdPartyIncluderAsset : ScriptableObject
    {
        public enum ThirdPartyIncludeType
        {
            FileCopy,
            DefineIfExists
        }
        [Serializable]
        public struct ThirdPartyInclude
        {
            public ThirdPartyIncludeType type;
            public string sourcePath;
            public string destinationPath;
            public string defineName;
        }
        public ThirdPartyInclude[] ThirdPartyIncludes;
    }

    [CustomEditor(typeof(ThirdPartyIncluderAsset))]
    public class ThirdPartyIncluderAssetEditor : Editor
    {
        private ReorderableList list;
        private List<bool> listFoldouts = new List<bool>();
        GUIContent headerGC = new GUIContent("Third Party Includers");
        GUIContent typeGC = new GUIContent("Type");
        GUIContent sourcePathGC = new GUIContent("Source Path");
        GUIContent destinationPathGC = new GUIContent("Destination Path");
        GUIContent filePathGC = new GUIContent("File Path");
        GUIContent defineNameGC = new GUIContent("Define Name");
        GUIContent expandAllGC = new GUIContent("Expand All");
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            if (list == null)
            {
                list = new ReorderableList(serializedObject, serializedObject.FindProperty(nameof(ThirdPartyIncluderAsset.ThirdPartyIncludes)), true, true, true, true);

                list.drawHeaderCallback = rect =>
                {
                    var expandAllWidth = GUI.skin.label.CalcSize(expandAllGC).x;
                    var labelRect = new Rect(rect);
                    labelRect.width -= expandAllWidth;
                    GUI.Label(labelRect, headerGC);
                    var buttonRect = new Rect(rect);
                    buttonRect.x = labelRect.width;
                    buttonRect.width = expandAllWidth + 8f;
                    if (GUI.Button(buttonRect, expandAllGC))
                    {
                        for (int i = 0; i < list.count; i++)
                        {
                            list.serializedProperty.GetArrayElementAtIndex(i).isExpanded = true;
                        }
                    }
                };

                list.onAddCallback = _list =>
                {
                    int index = _list.index == -1 ? _list.count - 1 : _list.index;
                    _list.serializedProperty.InsertArrayElementAtIndex(index);
                    _list.index = Mathf.Min(index + 1, _list.count - 1);
                    _list.serializedProperty.GetArrayElementAtIndex(_list.index).isExpanded = true;
                };
                list.onRemoveCallback = _list =>
                {
                    int index = _list.index;
                    _list.serializedProperty.DeleteArrayElementAtIndex(index);
                    _list.index = Mathf.Max(index - 1, 0);
                };
                list.onReorderCallbackWithDetails = (_list, oldElementIndex, newElementIndex) =>
                {
                    SerializedProperty newElement = _list.serializedProperty.GetArrayElementAtIndex(newElementIndex);
                    SerializedProperty oldElement = _list.serializedProperty.GetArrayElementAtIndex(oldElementIndex);
                    bool active = newElement.isExpanded;
                    newElement.isExpanded = oldElement.isExpanded;
                    oldElement.isExpanded = active;
                };

                list.elementHeightCallback = index =>
                {
                    SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
                    if (element.isExpanded)
                    {
                        var enumValue = element.FindPropertyRelative("type").enumValueIndex;
                        if (CompareTypeEnum(enumValue, ThirdPartyIncluderAsset.ThirdPartyIncludeType.FileCopy)) return EditorGUIUtility.singleLineHeight * 1.1f * 4f;
                        if (CompareTypeEnum(enumValue, ThirdPartyIncluderAsset.ThirdPartyIncludeType.DefineIfExists)) return EditorGUIUtility.singleLineHeight * 1.1f * 5f;
                        return EditorGUIUtility.singleLineHeight * 1.1f * 4f; // 4 for *now*
                    }
                    return EditorGUIUtility.singleLineHeight;
                };

                list.drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
                    int indextoo = index;
                    string name = indextoo.ToString();
                    var nameGC = new GUIContent(name);
                    float nameWidth = GUI.skin.label.CalcSize(nameGC).x;
                    element.isExpanded = EditorGUI.Foldout(new Rect(rect.x + 10f, rect.y, nameWidth, EditorGUIUtility.singleLineHeight), element.isExpanded, nameGC, true);

                    if (element.isExpanded)
                    {
                        float oldLabelWidth = EditorGUIUtility.labelWidth;
                        // Change label width to the biggest label
                        EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(destinationPathGC).x + 4f; // plus some padding

                        var typeElement = element.FindPropertyRelative("type");
                        EditorGUI.PropertyField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 1.1f * 1f, rect.width, EditorGUIUtility.singleLineHeight),
                            typeElement, typeGC);
                        if (CompareTypeEnum(typeElement.enumValueIndex, ThirdPartyIncluderAsset.ThirdPartyIncludeType.FileCopy))
                        {
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 1.1f * 2f, rect.width, EditorGUIUtility.singleLineHeight),
                                element.FindPropertyRelative("sourcePath"), sourcePathGC);
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 1.1f * 3f, rect.width, EditorGUIUtility.singleLineHeight),
                                element.FindPropertyRelative("destinationPath"), destinationPathGC);
                        }
                        if (CompareTypeEnum(typeElement.enumValueIndex, ThirdPartyIncluderAsset.ThirdPartyIncludeType.DefineIfExists))
                        {
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 1.1f * 2f, rect.width, EditorGUIUtility.singleLineHeight),
                                element.FindPropertyRelative("sourcePath"), filePathGC);
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 1.1f * 3f, rect.width, EditorGUIUtility.singleLineHeight),
                                element.FindPropertyRelative("destinationPath"), destinationPathGC);
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 1.1f * 4f, rect.width, EditorGUIUtility.singleLineHeight),
                                element.FindPropertyRelative("defineName"), defineNameGC);
                        }
                        // Revert label width
                        EditorGUIUtility.labelWidth = oldLabelWidth;
                    }
                };
            }

            list.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        bool CompareTypeEnum(int value, ThirdPartyIncluderAsset.ThirdPartyIncludeType type)
        {
            if (System.Enum.IsDefined(typeof(ThirdPartyIncluderAsset.ThirdPartyIncludeType), value))
            {
                return (ThirdPartyIncluderAsset.ThirdPartyIncludeType)value == type;
            }
            return false;
        }
    }
}