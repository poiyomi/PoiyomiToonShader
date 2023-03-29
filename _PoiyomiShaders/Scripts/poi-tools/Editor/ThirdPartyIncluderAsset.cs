using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using UnityEditorInternal;
using System.Collections.Generic;

namespace Poi.Tools
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
            public string sourceGUID;
            public string destinationPath;
            public string defineName;
        }
        public ThirdPartyInclude[] ThirdPartyIncludes;
    }

    [CustomEditor(typeof(ThirdPartyIncluderAsset))]
    public class ThirdPartyIncluderAssetEditor : UnityEditor.Editor
    {
        private ReorderableList list;
        private List<bool> listFoldouts = new List<bool>();
        GUIContent headerGC = new GUIContent("Third Party Includers");
        GUIContent typeGC = new GUIContent("Type");
        GUIContent sourcePathGC = new GUIContent("Source Path");
        GUIContent sourceGUIDGC = new GUIContent("Source GUID");
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
                        if (CompareTypeEnum(enumValue, ThirdPartyIncluderAsset.ThirdPartyIncludeType.FileCopy)) return EditorGUIUtility.singleLineHeight * 1.1f * 5f;
                        if (CompareTypeEnum(enumValue, ThirdPartyIncluderAsset.ThirdPartyIncludeType.DefineIfExists)) return EditorGUIUtility.singleLineHeight * 1.1f * 6f;
                        return EditorGUIUtility.singleLineHeight * 1.1f * 4f; // 4 for *now*
                    }
                    return EditorGUIUtility.singleLineHeight;
                };

                list.drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
                    var typeElement = element.FindPropertyRelative(nameof(ThirdPartyIncluderAsset.ThirdPartyInclude.type));
                    var destinationPathElement = element.FindPropertyRelative(nameof(ThirdPartyIncluderAsset.ThirdPartyInclude.destinationPath));
                    string destFileName = "";
                    if (!string.IsNullOrEmpty(destinationPathElement.stringValue))
                    {
                        destFileName = System.IO.Path.GetFileName(destinationPathElement.stringValue);
                    }
                    string name = $"{index.ToString()} ({typeElement.enumDisplayNames[typeElement.enumValueIndex]}) {destFileName}";
                    var nameGC = new GUIContent(name);
                    float nameWidth = GUI.skin.label.CalcSize(nameGC).x;
                    element.isExpanded = EditorGUI.Foldout(new Rect(rect.x + 10f, rect.y, nameWidth, EditorGUIUtility.singleLineHeight), element.isExpanded, nameGC, true);

                    if (element.isExpanded)
                    {
                        float oldLabelWidth = EditorGUIUtility.labelWidth;
                        // Change label width to the biggest label
                        EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(destinationPathGC).x + 4f; // plus some padding

                        var sourcePathElement = element.FindPropertyRelative(nameof(ThirdPartyIncluderAsset.ThirdPartyInclude.sourcePath));
                        var sourceGUIDElement = element.FindPropertyRelative(nameof(ThirdPartyIncluderAsset.ThirdPartyInclude.sourceGUID));
                        var defineNameElement = element.FindPropertyRelative(nameof(ThirdPartyIncluderAsset.ThirdPartyInclude.defineName));

                        EditorGUI.PropertyField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 1.1f * 1f, rect.width, EditorGUIUtility.singleLineHeight),
                            typeElement, typeGC);
                        if (CompareTypeEnum(typeElement.enumValueIndex, ThirdPartyIncluderAsset.ThirdPartyIncludeType.FileCopy))
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 1.1f * 2f, rect.width, EditorGUIUtility.singleLineHeight),
                                sourcePathElement, sourcePathGC);
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (!string.IsNullOrEmpty(sourcePathElement.stringValue))
                                {
                                    string sourceGUID = AssetDatabase.AssetPathToGUID(sourcePathElement.stringValue);
                                    if (!string.IsNullOrEmpty(sourceGUID))
                                    {
                                        sourceGUIDElement.stringValue = sourceGUID;
                                    }
                                }
                            }
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 1.1f * 3f, rect.width, EditorGUIUtility.singleLineHeight),
                                sourceGUIDElement, sourceGUIDGC);
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 1.1f * 4f, rect.width, EditorGUIUtility.singleLineHeight),
                                destinationPathElement, destinationPathGC);
                        }
                        if (CompareTypeEnum(typeElement.enumValueIndex, ThirdPartyIncluderAsset.ThirdPartyIncludeType.DefineIfExists))
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 1.1f * 2f, rect.width, EditorGUIUtility.singleLineHeight),
                                sourcePathElement, sourcePathGC);
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (!string.IsNullOrEmpty(sourcePathElement.stringValue))
                                {
                                    string sourceGUID = AssetDatabase.AssetPathToGUID(sourcePathElement.stringValue);
                                    if (!string.IsNullOrEmpty(sourceGUID))
                                    {
                                        sourceGUIDElement.stringValue = sourceGUID;
                                    }
                                }
                            }
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 1.1f * 3f, rect.width, EditorGUIUtility.singleLineHeight),
                                sourceGUIDElement, sourceGUIDGC);
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 1.1f * 4f, rect.width, EditorGUIUtility.singleLineHeight),
                                destinationPathElement, destinationPathGC);
                            EditorGUI.PropertyField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight * 1.1f * 5f, rect.width, EditorGUIUtility.singleLineHeight),
                                defineNameElement, defineNameGC);
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