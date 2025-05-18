using UnityEngine;
using UnityEditor;
using System;
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

            // New clear contents flag
            public bool clearContents;
        }

        public ThirdPartyInclude[] ThirdPartyIncludes;
    }

    [CustomEditor(typeof(ThirdPartyIncluderAsset))]
    public class ThirdPartyIncluderAssetEditor : UnityEditor.Editor
    {
        private ReorderableList list;
        GUIContent headerGC = new GUIContent("Third Party Includers");
        GUIContent expandAllGC = new GUIContent("Expand All");

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            if (list == null)
            {
                list = new ReorderableList(
                    serializedObject,
                    serializedObject.FindProperty(nameof(ThirdPartyIncluderAsset.ThirdPartyIncludes)),
                    true, true, true, true
                );

                list.drawHeaderCallback = rect =>
                {
                    float expandWidth = GUI.skin.label.CalcSize(expandAllGC).x;
                    var labelRect = new Rect(rect.x, rect.y, rect.width - expandWidth - 8f, rect.height);
                    GUI.Label(labelRect, headerGC);

                    var buttonRect = new Rect(rect.x + labelRect.width, rect.y, expandWidth + 8f, rect.height);
                    if (GUI.Button(buttonRect, expandAllGC))
                    {
                        for (int i = 0; i < list.count; i++)
                            list.serializedProperty.GetArrayElementAtIndex(i).isExpanded = true;
                    }
                };

                list.onAddCallback = _list =>
                {
                    int index = _list.index == -1 ? Mathf.Max(_list.count - 1, 0) : _list.index;
                    _list.serializedProperty.InsertArrayElementAtIndex(index);
                    _list.index = Mathf.Min(index + 1, _list.count - 1);
                    _list.serializedProperty.GetArrayElementAtIndex(_list.index).isExpanded = true;
                };

                list.onRemoveCallback = _list =>
                {
                    int idx = _list.index;
                    _list.serializedProperty.DeleteArrayElementAtIndex(idx);
                    _list.index = Mathf.Max(idx - 1, 0);
                };

                list.onReorderCallbackWithDetails = (_list, oldIndex, newIndex) =>
                {
                    var newElem = _list.serializedProperty.GetArrayElementAtIndex(newIndex);
                    var oldElem = _list.serializedProperty.GetArrayElementAtIndex(oldIndex);
                    bool wasExpanded = oldElem.isExpanded;
                    newElem.isExpanded = wasExpanded;
                    oldElem.isExpanded = wasExpanded;
                };

                list.elementHeightCallback = index =>
                {
                    var element = list.serializedProperty.GetArrayElementAtIndex(index);
                    if (!element.isExpanded)
                        return EditorGUIUtility.singleLineHeight;

                    var typeIndex = element.FindPropertyRelative("type").enumValueIndex;
                    int lines = 5; // default fallback
                    if (CompareType(typeIndex, ThirdPartyIncluderAsset.ThirdPartyIncludeType.FileCopy))
                        lines = 6;           // + clearContents
                    else if (CompareType(typeIndex, ThirdPartyIncluderAsset.ThirdPartyIncludeType.DefineIfExists))
                        lines = 7;           // + defineName + clearContents

                    return EditorGUIUtility.singleLineHeight * 1.1f * lines;
                };

                list.drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var element = list.serializedProperty.GetArrayElementAtIndex(index);
                    var typeProp = element.FindPropertyRelative(nameof(ThirdPartyIncluderAsset.ThirdPartyInclude.type));
                    var destProp = element.FindPropertyRelative(nameof(ThirdPartyIncluderAsset.ThirdPartyInclude.destinationPath));

                    // Foldout header
                    string destName = string.IsNullOrEmpty(destProp.stringValue)
                        ? ""
                        : System.IO.Path.GetFileName(destProp.stringValue);
                    string title = $"{index} ({typeProp.enumDisplayNames[typeProp.enumValueIndex]}) {destName}";
                    element.isExpanded = EditorGUI.Foldout(
                        new Rect(rect.x + 10f, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        element.isExpanded, title, true
                    );

                    if (!element.isExpanded)
                        return;

                    // Draw fields
                    float lineH = EditorGUIUtility.singleLineHeight * 1.1f;
                    float y = rect.y + lineH;
                    float width = rect.width;

                    // 1) Type
                    EditorGUI.PropertyField(new Rect(rect.x, y, width, EditorGUIUtility.singleLineHeight), typeProp, new GUIContent("Type"));

                    // 2) SourcePath
                    y += lineH;
                    var sourcePathProp = element.FindPropertyRelative(nameof(ThirdPartyIncluderAsset.ThirdPartyInclude.sourcePath));
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.PropertyField(new Rect(rect.x, y, width, EditorGUIUtility.singleLineHeight), sourcePathProp, new GUIContent("Source Path"));
                    if (EditorGUI.EndChangeCheck())
                    {
                        var guidProp = element.FindPropertyRelative(nameof(ThirdPartyIncluderAsset.ThirdPartyInclude.sourceGUID));
                        string newGUID = AssetDatabase.AssetPathToGUID(sourcePathProp.stringValue);
                        if (!string.IsNullOrEmpty(newGUID))
                            guidProp.stringValue = newGUID;
                    }

                    // 3) SourceGUID
                    y += lineH;
                    EditorGUI.PropertyField(new Rect(rect.x, y, width, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative(nameof(ThirdPartyIncluderAsset.ThirdPartyInclude.sourceGUID)),
                        new GUIContent("Source GUID")
                    );

                    // 4) DestinationPath
                    y += lineH;
                    EditorGUI.PropertyField(new Rect(rect.x, y, width, EditorGUIUtility.singleLineHeight),
                        destProp,
                        new GUIContent("Destination Path")
                    );

                    // 5) DefineName (if DefineIfExists)
                    if (CompareType(typeProp.enumValueIndex, ThirdPartyIncluderAsset.ThirdPartyIncludeType.DefineIfExists))
                    {
                        y += lineH;
                        EditorGUI.PropertyField(new Rect(rect.x, y, width, EditorGUIUtility.singleLineHeight),
                            element.FindPropertyRelative(nameof(ThirdPartyIncluderAsset.ThirdPartyInclude.defineName)),
                            new GUIContent("Define Name")
                        );
                    }

                    // 6) ClearContents toggle
                    y += lineH;
                    EditorGUI.PropertyField(new Rect(rect.x, y, width, EditorGUIUtility.singleLineHeight),
                        element.FindPropertyRelative(nameof(ThirdPartyIncluderAsset.ThirdPartyInclude.clearContents)),
                        new GUIContent("Clear Dest Contents on Export")
                    );
                };
            }

            // Finally draw the list
            list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        private bool CompareType(int enumValueIndex, ThirdPartyIncluderAsset.ThirdPartyIncludeType type)
        {
            if (Enum.IsDefined(typeof(ThirdPartyIncluderAsset.ThirdPartyIncludeType), enumValueIndex))
                return (ThirdPartyIncluderAsset.ThirdPartyIncludeType)enumValueIndex == type;
            return false;
        }
    }
}
