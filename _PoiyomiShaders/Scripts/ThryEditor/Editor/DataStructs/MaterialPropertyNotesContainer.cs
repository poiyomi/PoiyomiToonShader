using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Thry.ThryEditor
{
    [Serializable]
    public class MaterialPropertyNotesContainer : ISerializationCallbackReceiver
    {
        const string NotesTagKey = "Thry_MaterialPropertyNotes";
        const int MaxNoteCharacters = 256;
        
        Dictionary<string, string> PropertyNotes = new Dictionary<string, string>();

        [SerializeField] string[] _propertyNames;
        [SerializeField] string[] _propertyNotes;
        
        public bool HasNotes => PropertyNotes != null && PropertyNotes.Count > 0;

        Material OwnerMaterial { get; set; }

        private MaterialPropertyNotesContainer() {}

        private MaterialPropertyNotesContainer(Material ownerMaterial)
        {
            OwnerMaterial = ownerMaterial;
        }


        public void OnBeforeSerialize()
        {
            _propertyNames = PropertyNotes.Keys.ToArray();
            _propertyNotes = PropertyNotes.Values.ToArray();
        }

        public void OnAfterDeserialize()
        {
            PropertyNotes = new Dictionary<string, string>();
            
            if((_propertyNames == null || _propertyNotes == null) || (_propertyNames.Length != _propertyNotes.Length))
                return;
            
            for(int i = 0; i < _propertyNames.Length; i++)
                PropertyNotes[_propertyNames[i]] = _propertyNotes[i];
        }

        public void SaveNotesToMaterial()
        {
            string json = EditorJsonUtility.ToJson(this, false);
            OwnerMaterial.SetOverrideTag(NotesTagKey, json);
            EditorUtility.SetDirty(OwnerMaterial);
        }

        public static MaterialPropertyNotesContainer[] GetNoteContainersForMaterials(Material[] materials)
        {
            List<MaterialPropertyNotesContainer> noteContainers = new List<MaterialPropertyNotesContainer>();
            foreach(var material in materials)
                noteContainers.Add(GetNoteContainerForMaterial(material));
            return noteContainers.ToArray();
        }

        public static MaterialPropertyNotesContainer GetNoteContainerForMaterial(Material material)
        {
            MaterialPropertyNotesContainer newContainer = new MaterialPropertyNotesContainer(material);
            string json = material.GetTag(NotesTagKey, false, null);
            if(!string.IsNullOrWhiteSpace(json))
            {
                try
                {
                    EditorJsonUtility.FromJsonOverwrite(json, newContainer);
                }
                catch(Exception ex)
                {
                    Debug.LogError($"Failed to load notes for material {material.name}. Json:\n{json}");
                    Debug.LogException(ex);
                }
            }

            return newContainer;
        }

        public bool PropertyHasNote(string propertyName)
        {
            return PropertyNotes.ContainsKey(propertyName);
        }

        public bool TryGetNoteForProperty(string propertyName, out string note)
        {
            return PropertyNotes.TryGetValue(propertyName, out note);
        }

        public void SetNote(string propertyName, string note)
        {
            PropertyNotes[propertyName] = note.Length > MaxNoteCharacters ? note.Substring(0, MaxNoteCharacters) : note;
            SaveNotesToMaterial();
        }

        public void SetNoteWithoutSaving(string propertyName, string note)
        {
            PropertyNotes[propertyName] = note;
        }

        public void ClearAllNotes()
        {
            PropertyNotes.Clear();
            OwnerMaterial.SetOverrideTag(NotesTagKey, null);
        }
    }
}