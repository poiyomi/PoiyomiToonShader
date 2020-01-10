using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class MaterialLinker
    {

        private static Dictionary<string, List<Material>> linked_materials;

        private static void Load()
        {
            if (linked_materials == null)
            {
                linked_materials = new Dictionary<string, List<Material>>();
                string raw = FileHelper.ReadFileIntoString(PATH.LINKED_MATERIALS_FILE);
                string[][] parsed = Parser.ParseToObject<string[][]>(raw);
                if(parsed!=null)
                    foreach (string[] material_cloud in parsed)
                    {
                        List<Material> materials = new List<Material>();
                        for (int i = 1; i < material_cloud.Length; i++)
                        {
                            string path = AssetDatabase.GUIDToAssetPath(material_cloud[i]);
                            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
                            if (m != null)
                                materials.Add(m);
                        }
                        foreach (Material m in materials)
                            linked_materials.Add(GetKey(m, material_cloud[0]), materials);
                    }
            }
        }

        private static void Save()
        {
            Dictionary<string, List<Material>> save_linked_materials = new Dictionary<string, List<Material>>(linked_materials);

            List<string[]> save_structre = new List<string[]>();
            HashSet<string> has_already_been_saved = new HashSet<string>();
            foreach (KeyValuePair<string,List<Material>> link in save_linked_materials)
            {
                if (has_already_been_saved.Contains(link.Key)) continue;
                string[] value = new string[link.Value.Count + 1];
                value[0] = System.Text.RegularExpressions.Regex.Split(link.Key,@"###")[1];
                int i = 1;
                foreach (Material m in link.Value) {
                    string guid = UnityHelper.GetGUID(m);
                    has_already_been_saved.Add(guid+"###"+value[0]);
                    value[i++] = guid;
                }
                save_structre.Add(value);
            }
            FileHelper.WriteStringToFile(Parser.ObjectToString(save_structre),PATH.LINKED_MATERIALS_FILE);
        }

        public static List<Material> GetLinked(MaterialProperty p)
        {
            return GetLinked((Material)p.targets[0], p);
        }

        public static List<Material> GetLinked(Material m, MaterialProperty p)
        {
            Load();
            string key = GetKey(m,p);
            if (linked_materials.ContainsKey(key))
                return linked_materials[key];
            return null;
        }

        public static void Link(Material master, Material add_to, MaterialProperty p)
        {
            Load();
            Debug.Log("link " + master.name + "," + add_to.name);
            string key1 = GetKey(master,p);
            string key2 = GetKey(add_to,p);
            bool containes_key1 = linked_materials.ContainsKey(key1);
            bool containes_key2 = linked_materials.ContainsKey(key2);

            if(containes_key1 && containes_key2)
            {
                Unlink(add_to, p);
                Link(master, add_to, p);
                return;
            }
            else if (containes_key1)
                AddToListIfMaterialAlreadyLinked(key1, key2, add_to);
            else if (containes_key2)
                AddToListIfMaterialAlreadyLinked(key2, key1, master);
            else
            {
                List<Material> value = new List<Material>();
                value.Add(master);
                value.Add(add_to);
                linked_materials[key1] = value;
                linked_materials[key2] = value;
            }
        }

        private static void AddToListIfMaterialAlreadyLinked(string existing_key, string add_key, Material add_material)
        {
            List<Material> value = linked_materials[existing_key];
            value.Add(add_material);
            linked_materials[add_key] = value;
        }

        public static void Unlink(Material m, MaterialProperty p)
        {
            Load();
            string key = GetKey(m,p);
            List<Material> value = linked_materials[key];
            value.Remove(m);
            linked_materials.Remove(key);
        }

        private static void UpdateLinkList(List<Material> new_linked_materials, MaterialProperty p)
        {
            string key = GetKey(p);
            if (linked_materials.ContainsKey(key))
            {
                List<Material> old_materials = linked_materials[key];
                foreach (Material m in old_materials)
                    linked_materials.Remove(GetKey(m, p));
            }
            foreach (Material m in new_linked_materials)
                linked_materials[GetKey(m, p)] = new_linked_materials;
        }

        public static void UnlinkAll(Material m)
        {
            string guid = UnityHelper.GetGUID(m);
            List<string> remove_keys = new List<string>();
            foreach (KeyValuePair<string, List<Material>> link_cloud in linked_materials)
            {
                if (link_cloud.Key.StartsWith(guid + "###"))
                {
                    link_cloud.Value.Remove(m);
                    remove_keys.Add(link_cloud.Key);
                }
            }
            foreach (string k in remove_keys)
                linked_materials.Remove(k);
            RemoveEmptyLinks();
            Save();
        }

        private static void RemoveEmptyLinks()
        {
            List<string> remove_keys = new List<string>();
            foreach (KeyValuePair<string, List<Material>> link_cloud in linked_materials)
            {
                if (link_cloud.Value.Count < 2)
                {
                    link_cloud.Value.Clear();
                    remove_keys.Add(link_cloud.Key);
                }
            }
            foreach (string k in remove_keys)
                linked_materials.Remove(k);
        }

        private static string GetKey(Material m, MaterialProperty p)
        {
            return GetKey(m, p.name);
        }

        private static string GetKey(MaterialProperty p)
        {
            return GetKey((Material)(p.targets[0]), p.name);
        }

        private static string GetKey(Material m, string p)
        {
            return UnityHelper.GetGUID(m) + "###" + p;
        }

        private static MaterialLinkerPopupWindow window;
        public static void Popup(Rect activeation_rect, List<Material> linked_materials, MaterialProperty p)
        {
            Vector2 pos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            pos.x = Mathf.Min(EditorWindow.focusedWindow.position.x + EditorWindow.focusedWindow.position.width - 250, pos.x);
            pos.y = Mathf.Min(EditorWindow.focusedWindow.position.y + EditorWindow.focusedWindow.position.height - 200, pos.y);

            Load();
            if (window != null)
                window.Close();
            window = ScriptableObject.CreateInstance<MaterialLinkerPopupWindow>();
            window.position = new Rect(pos.x, pos.y, 250, 200);
            window.Init(linked_materials, p);
            window.ShowPopup();
        }

        private class MaterialLinkerPopupWindow : EditorWindow
        {

            private List<Material> linked_materials;
            private MaterialProperty materialProperty;

            public void Init(List<Material> linked_materials, MaterialProperty p)
            {
                if (linked_materials == null)
                    linked_materials = new List<Material>();
                this.linked_materials = new List<Material>(linked_materials);

                string self_guid = UnityHelper.GetGUID((Material)p.targets[0]);
                for (int i = this.linked_materials.Count - 1; i >= 0; i--)
                {
                    if (UnityHelper.GetGUID(this.linked_materials[i]) == self_guid)
                        this.linked_materials.RemoveAt(i);
                }
                this.materialProperty = p;
            }

            public new Vector2 minSize = new Vector2(250, 200);

            void OnGUI()
            {
                GUILayout.Label("Linked Materials", EditorStyles.boldLabel);
                GuiHelper.DrawListField<Material>(linked_materials);
                //GUILayout.Box("Drag and Drop New Material", EditorStyles.helpBox);
                //Rect drag_rect = GUILayoutUtility.GetLastRect();
                Rect lastRect = GUILayoutUtility.GetLastRect();
                Rect drag_rect = new Rect(0, lastRect.y, Screen.width, Screen.height - lastRect.y - 30);
                Event e = Event.current;
                if ((e.type == EventType.DragPerform || e.type == EventType.DragUpdated) && drag_rect.Contains(e.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (e.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        HanldeDropEvent();
                    }
                }
                if (GUI.Button(new Rect(0,this.position.height-30,this.position.width,30),"Done"))
                    this.Close();
            }

            public void HanldeDropEvent()
            {
                string[] paths = DragAndDrop.paths;
                if (AssetDatabase.GetMainAssetTypeAtPath(paths[0]) == typeof(Material))
                {
                    linked_materials.Add(AssetDatabase.LoadAssetAtPath<Material>(paths[0]));
                }
            }

            void Awake()
            {
                
            }

            void OnDestroy()
            {
                //add itself
                bool contains_itself = false;
                string self_guid = UnityHelper.GetGUID((Material)materialProperty.targets[0]);
                for (int i = linked_materials.Count - 1; i >= 0; i--)
                {
                    if (UnityHelper.GetGUID(linked_materials[i]) == self_guid)
                        contains_itself = true;
                    if (linked_materials[i] == null)
                        linked_materials.RemoveAt(i);
                }
                if (linked_materials.Count>0 && !contains_itself)
                    linked_materials.Add((Material)materialProperty.targets[0]);

                UpdateLinkList(linked_materials, materialProperty);
                Save();
            }
        }
    }
}