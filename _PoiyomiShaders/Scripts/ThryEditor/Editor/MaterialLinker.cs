using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class MaterialLinker
    {
        private static Dictionary<(Material,string), List<Material>> linked_materials;

        private static void Load()
        {
            if (linked_materials == null)
            {
                linked_materials = new Dictionary<(Material,string), List<Material>>();
                string raw = FileHelper.ReadFileIntoString(PATH.LINKED_MATERIALS_FILE);
                string[][] parsed = Parser.Deserialize<string[][]>(raw);
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
                            if(linked_materials.ContainsKey((m, material_cloud[0])) == false)
                                linked_materials.Add((m, material_cloud[0]), materials);
                    }
            }
        }

        private static void Save()
        {
            List<string[]> save_structre = new List<string[]>();
            HashSet<(Material,string)> has_already_been_saved = new HashSet<(Material,string)>();
            foreach (KeyValuePair<(Material,string),List<Material>> link in linked_materials)
            {
                if (has_already_been_saved.Contains(link.Key)) continue;
                string[] value = new string[link.Value.Count + 1];
                value[0] = link.Key.Item2;
                int i = 1;
                foreach (Material m in link.Value) {
                    has_already_been_saved.Add((m, link.Key.Item2));
                    value[i++] = UnityHelper.GetGUID(m);
                }
                save_structre.Add(value);
            }
            FileHelper.WriteStringToFile(Parser.ObjectToString(save_structre),PATH.LINKED_MATERIALS_FILE);
        }

        public static bool IsLinked(MaterialProperty p)
        {
            Load();
            return linked_materials.ContainsKey(((Material)p.targets[0], p.name));
        }

        public static List<Material> GetLinked(MaterialProperty p)
        {
            return GetLinked((Material)p.targets[0], p);
        }

        public static List<Material> GetLinked(Material m, MaterialProperty p)
        {
            Load();
            if (linked_materials.ContainsKey((m,p.name)))
                return linked_materials[(m,p.name)];
            return null;
        }

        public static void Link(Material master, Material add_to, MaterialProperty p)
        {
            Load();
            Debug.Log("link " + master.name + "," + add_to.name);
            bool containes_key1 = linked_materials.ContainsKey((master,p.name));
            bool containes_key2 = linked_materials.ContainsKey((add_to,p.name));

            if(containes_key1 && containes_key2)
            {
                Unlink(add_to, p);
                Link(master, add_to, p);
                return;
            }
            else if (containes_key1)
                AddToListIfMaterialAlreadyLinked(master, add_to, p);
            else if (containes_key2)
                AddToListIfMaterialAlreadyLinked(add_to, master, p);
            else
            {
                List<Material> value = new List<Material>();
                value.Add(master);
                value.Add(add_to);
                linked_materials[(master,p.name)] = value;
                linked_materials[(add_to,p.name)] = value;
            }
        }

        private static void AddToListIfMaterialAlreadyLinked(Material existing, Material add, MaterialProperty p)
        {
            List<Material> value = linked_materials[(existing,p.name)];
            value.Add(add);
            linked_materials[(add,p.name)] = value;
        }

        public static void Unlink(Material m, MaterialProperty p)
        {
            Load();
            List<Material> value = linked_materials[(m,p.name)];
            value.Remove(m);
            linked_materials.Remove((m,p.name));
        }

        private static void UpdateLinkList(List<Material> new_linked_materials, MaterialProperty p)
        {
            var key = (p.targets[0] as Material, p.name);
            if (linked_materials.ContainsKey(key))
            {
                List<Material> old_materials = linked_materials[key];
                foreach (Material m in old_materials)
                    linked_materials.Remove((m, p.name));
            }
            foreach (Material m in new_linked_materials)
                linked_materials[(m, p.name)] = new_linked_materials;
        }

        public static void UnlinkAll(Material m)
        {
            List<(Material, string)> remove_keys = new List<(Material, string)>();
            foreach (KeyValuePair<(Material,string), List<Material>> link_cloud in linked_materials)
            {
                if (link_cloud.Key.Item1 == m)
                {
                    link_cloud.Value.Remove(m);
                    remove_keys.Add(link_cloud.Key);
                }
            }
            foreach ((Material, string) k in remove_keys)
                linked_materials.Remove(k);
            RemoveEmptyLinks();
            Save();
        }

        private static void RemoveEmptyLinks()
        {
            List<(Material, string)> remove_keys = new List<(Material, string)>();
            foreach (KeyValuePair<(Material,string), List<Material>> link_cloud in linked_materials)
            {
                if (link_cloud.Value.Count < 2)
                {
                    link_cloud.Value.Clear();
                    remove_keys.Add(link_cloud.Key);
                }
            }
            foreach ((Material, string) k in remove_keys)
                linked_materials.Remove(k);
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
            private Vector2 scrollPos;
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
                float listMaxHeight = this.position.height - 110;
                GuiHelper.DrawListField<Material>(linked_materials, listMaxHeight, ref scrollPos);
                GUILayout.Box("Drag and Drop new Material", EditorStyles.helpBox, GUILayout.MinHeight(30));
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
                foreach (string path in DragAndDrop.paths)
                {
                    if (AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(Material))
                    {
                        linked_materials.Add(AssetDatabase.LoadAssetAtPath<Material>(path));
                    }
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