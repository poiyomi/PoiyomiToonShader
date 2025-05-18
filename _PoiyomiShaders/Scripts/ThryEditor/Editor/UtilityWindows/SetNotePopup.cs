using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor
{
    public class SetNotePopup : EditorWindow
    {
        ShaderPart ShaderPart { get; set; }
        string TextFieldContent { get; set; }
        
        void Awake()
        {
            titleContent = new GUIContent("Set Note");
        }

        public void Init(ShaderPart shaderPart, Rect? rectOverride = null)
        {
            ShaderPart = shaderPart;
            TextFieldContent = shaderPart.Note;
            if(rectOverride != null)
                position = rectOverride.Value;
        }

        void OnGUI()
        {
            if(ShaderPart == null)
            {
                Close();
                return;
            }

            GUI.SetNextControlName(nameof(TextFieldContent));
            TextFieldContent = EditorGUILayout.TextField(TextFieldContent);
            EditorGUI.FocusTextInControl(nameof(TextFieldContent));
            
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Cancel", GUILayout.Height(30))) 
            {
                Close();
            }

            if(Event.current.isKey)
            {
                if(Event.current.keyCode == KeyCode.Return)
                    UpdateNoteAndClose(true);
                else if(Event.current.keyCode == KeyCode.Escape)
                    Close();
            }

            if(GUILayout.Button("Ok", GUILayout.Height(30)))
                UpdateNoteAndClose(false);
            
            EditorGUILayout.EndHorizontal();
        }

        void UpdateNoteAndClose(bool enterPressed)
        {
            if(enterPressed)
                Event.current.Use();
            
            ShaderPart.Note = TextFieldContent;
            Close();
        }
    }
}