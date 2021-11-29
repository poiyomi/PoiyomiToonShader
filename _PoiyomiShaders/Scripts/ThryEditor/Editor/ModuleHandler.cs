// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public abstract class ModuleSettings
    {
        public const string MODULES_CONFIG = "Thry/modules_config";

        public abstract void Draw();
    }

    public class ModuleHandler
    {
        private static List<Module> modules;
        private static List<Module> third_party_modules;
        private static bool modules_are_being_loaded = false;

        private class ModuleCollectionInfo
        {
            public string id = null;
            public string url = null;
            public string author = null;
        }

        private class ModuleCollection
        {
            public List<ModuleCollectionInfo> first_party = null;
            public List<ModuleCollectionInfo> third_party = null;
        }

        public static List<Module> GetModules()
        {
            if (!modules_are_being_loaded)
                LoadModules();
            return modules;
        }

        public static List<Module> GetThirdPartyModules()
        {
            if (!modules_are_being_loaded)
                LoadModules();
            return third_party_modules;
        }

        private static void LoadModules()
        {
            modules_are_being_loaded = true;
            WebHelper.DownloadStringASync(URL.MODULE_COLLECTION, delegate (string s) {
                modules = new List<Module>();
                third_party_modules = new List<Module>();
                ModuleCollection module_collection = Parser.ParseToObject<ModuleCollection>(s);
                foreach(ModuleCollectionInfo info in module_collection.first_party)
                {
                    LoadModule(info,modules);
                }
                foreach (ModuleCollectionInfo info in module_collection.third_party)
                {
                    LoadModule(info, third_party_modules);
                }
            });
        }

        private static void LoadModule(ModuleCollectionInfo info, List<Module> modules)
        {
            WebHelper.DownloadStringASync(info.url, delegate (string data)
            {
                Module new_module = new Module();
                new_module.url = info.url;
                new_module.author = info.author;
                new_module.id = info.id;
                new_module.available_module = Parser.ParseToObject<ModuleInfo>(data);
                new_module.available_module.version = new_module.available_module.version.Replace(",", ".");
                bool module_installed = LoadModuleLocationData(new_module);
                if (module_installed)
                    InitInstalledModule(new_module);
                else if (Helper.ClassWithNamespaceExists(new_module.available_module.classname))
                    CheckForUnregisteredInstall(new_module);
                if(new_module.installed_module != null)
                    new_module.installed_module.version = new_module.installed_module.version.Replace(",", ".");
                if (new_module.available_module.requirement != null)
                    new_module.available_requirement_fullfilled = new_module.available_module.requirement.Test();
                if (new_module.available_requirement_fullfilled && new_module.installed_module != null && Helper.compareVersions(new_module.installed_module.version, new_module.available_module.version) == 1)
                    new_module.update_available = true;
                modules.Add(new_module);
                UnityHelper.RepaintEditorWindow(typeof(Settings));
            });
        }

        private static bool LoadModuleLocationData(Module m)
        {
            string data = FileHelper.LoadValueFromFile(m.id,PATH.MODULES_LOCATION__DATA);
            if (string.IsNullOrEmpty(data))
            {
                return false;
            }
            m.location_data = Parser.ParseToObject<ModuleLocationData>(data);
            if (AssetDatabase.GUIDToAssetPath(m.location_data.guid) == "")
            {
                m.location_data = null;
                return false;
            }
            return true;
        }

        private static void SaveModuleLocationData(Module m, string guid)
        {
            ModuleLocationData locationData = new ModuleLocationData();
            locationData.guid = guid;
            locationData.classname = m.installed_module.classname;
            locationData.files = m.installed_module.files.ToArray();
            FileHelper.SaveValueToFile(m.id, Parser.ObjectToString(locationData), PATH.MODULES_LOCATION__DATA);
        }

        private static void CheckForUnregisteredInstall(Module module)
        {
            //Debug.Log(module.available_module.classname + ":" + Helper.ClassWithNamespaceExists(module.available_module.classname));
            if (Helper.ClassWithNamespaceExists(module.available_module.classname))
            {
                module.path = ResolveFilesToDirectory(module.available_module.files.ToArray());
                if (string.IsNullOrEmpty(module.path) == false)
                {
                    module.installed_module = Parser.ParseToObject<ModuleInfo>(FileHelper.ReadFileIntoString(FindModuleFilePath(module.path)));
                    SaveModuleLocationData(module,AssetDatabase.AssetPathToGUID(module.path));
                }
            }
        }

        //TODO save location data on install
        //     delete location data on remove
        //     destingish between public and private modules

        private static void InitInstalledModule(Module m)
        {
            bool remove = false;
            if (Helper.ClassWithNamespaceExists(m.location_data.classname))
            {
                m.path = GetModuleDirectory(m);
                if (string.IsNullOrEmpty(m.path) == false)
                {
                    Debug.Log(m.path);
                    m.installed_module = Parser.ParseToObject<ModuleInfo>(FileHelper.ReadFileIntoString(FindModuleFilePath(m.path)));
                    Debug.Log(m.path);
                    string calced_guid = AssetDatabase.AssetPathToGUID(m.path);
                    if (m.location_data.guid != calced_guid)
                        SaveModuleLocationData(m, calced_guid);
                }
                else
                {
                    remove = true;
                }
            }
            if (remove)
            {
                FileHelper.RemoveValueFromFile(m.id, PATH.MODULES_LOCATION__DATA);
                m.location_data = null;
            }
        }

        private static string GetModuleDirectory(Module m)
        {
            string path = null;
            if(m.location_data != null)
            {
                path = AssetDatabase.GUIDToAssetPath(m.location_data.guid);
                if(path == "" || path == null || !Directory.Exists(path))
                {
                    path = ResolveFilesToDirectory(m.location_data.files);
                }
            }
            if (!Directory.Exists(path))
                path = null;
            return path;
        }

        private static string ResolveFilesToDirectory(string[] files)
        {
            Dictionary<string, int> path_refernces = new Dictionary<string, int>();
            foreach (string file in files)
            {
                string[] refernces = ResolveFilesToDirectoryFindAllReferneces(file);
                foreach(string p in refernces)
                {
                    string found_dir = p.Replace(file, "");
                    if (path_refernces.ContainsKey(found_dir))
                        path_refernces[found_dir] = path_refernces[found_dir] + 1;
                    else
                        path_refernces[found_dir] = 1;
                }
            }
            int most_refernces = 0;
            string path = null;
            foreach(KeyValuePair<string,int> pair in path_refernces)
            {
                if (pair.Value > most_refernces)
                {
                    most_refernces = pair.Value;
                    path = pair.Key;
                }
            }
            return path;
        }

        private static string[] ResolveFilesToDirectoryFindAllReferneces(string file_sub_path)
        {
            List<string> valid_paths = new List<string>();
            string[] found_paths = UnityHelper.FindAssetOfFilesWithExtension(Path.GetFileName(file_sub_path)).ToArray();
            foreach (string p in found_paths)
            {
                if (p.EndsWith(file_sub_path))
                    valid_paths.Add(p);
            }
            return valid_paths.ToArray();
        }

        private static string FindModuleFilePath(string directory_path)
        {
            string module_path = null;
            int likelyness = -1;
            foreach(string f in Directory.GetFiles(directory_path)){
                string file_name = Path.GetFileName(f);
                int l = 0;
                if (file_name.Contains("module")) l++;
                if (file_name.Contains("thry")) l++;
                if (file_name.Contains(".json")) l++;
                if (l > likelyness)
                {
                    likelyness = l;
                    module_path = f;
                }
            }
            return module_path;
        }

        public static void InstallRemoveModule(Module module, bool install)
        {
            if (install && module.installed_module == null)
                InstallModule(module);
            else if (!install && module.installed_module != null)
                RemoveModule(module);
        }

        public static void OnCompile()
        {
            string url = FileHelper.LoadValueFromFile("update_module_url", PATH.AFTER_COMPILE_DATA);
            string id = FileHelper.LoadValueFromFile("update_module_id", PATH.AFTER_COMPILE_DATA);
            if (url != null && url.Length > 0 && id != null && id.Length > 0)
            {
                InstallModule(url, id);
                FileHelper.SaveValueToFile("update_module_id", "", PATH.AFTER_COMPILE_DATA);
                FileHelper.SaveValueToFile("update_module_url", "", PATH.AFTER_COMPILE_DATA);
            }
        }

        public static void UpdateModule(Module module)
        {
            module.is_being_installed_or_removed = true;
            FileHelper.SaveValueToFile("update_module_url", module.url, PATH.AFTER_COMPILE_DATA);
            FileHelper.SaveValueToFile("update_module_id", module.id, PATH.AFTER_COMPILE_DATA);
            RemoveModule(module);
        }

        public static void InstallModule(string url, string id)
        {
            WebHelper.DownloadStringASync(url, delegate (string data)
            {
                Module new_module = new Module();
                new_module.url = url;
                new_module.id = id;
                new_module.available_module = Parser.ParseToObject<ModuleInfo>(data);
                InstallModule(new_module);
            });
        }

        public static void InstallModule(Module module)
        {
            module.is_being_installed_or_removed = true;
            string temp_path = InstallModuleGetTempDir(module);
            InstallModuleDownloadFiles(module,temp_path);
        }

        private static string InstallModuleGetTempDir(Module module)
        {
            return "temp_module_" + module.id;
        }

        private static string GetThryModulesDirectoryPath()
        {
            string editor_path = ShaderEditor.GetShaderEditorDirectoryPath();
            if (editor_path == null)
                editor_path = "Assets";
            return editor_path+ "/thry_modules";
        }

        private static void InstallModuleDownloadFiles(Module module, string temp_path)
        {
            EditorUtility.DisplayProgressBar(module.available_module.name+ " download progress", "", 0);
            string base_url = Path.GetDirectoryName(module.url);
            int i = 0;
            foreach (string file_path in module.available_module.files)
            {
                WebHelper.DownloadFileASync(base_url + "/"+ file_path, temp_path + "/" + file_path, delegate (string data)
                {
                    i++;
                    EditorUtility.DisplayProgressBar("Downloading files for " + module.available_module.name, "Downloaded " + base_url + file_path, (float)i / module.available_module.files.Count);
                    if (i == module.available_module.files.Count)
                    {
                        EditorUtility.ClearProgressBar();
                        InstallModuleFilesDownloaded(module,temp_path);
                    }
                });
            }
        }

        private static void InstallModuleFilesDownloaded(Module module, string temp_dir)
        {
            string modules_path = GetThryModulesDirectoryPath();
            if (!Directory.Exists(modules_path))
                Directory.CreateDirectory(modules_path);
            string install_path = modules_path + "/" + module.id;
            module.installed_module = module.available_module;
            string guid = AssetDatabase.CreateFolder(modules_path, module.id);
            SaveModuleLocationData(module,guid);

            FileHelper.WriteStringToFile(Parser.ObjectToString(module.available_module), temp_dir + "/module.json");
            foreach(string d in Directory.GetDirectories(temp_dir))
            {
                Directory.Move(d, install_path + "/" + Path.GetFileName(d));
            }
            foreach (string f in Directory.GetFiles(temp_dir))
            {
                File.Move(f, install_path + "/" + Path.GetFileName(f));
            }
            Directory.Delete(temp_dir);
            AssetDatabase.Refresh();
        }

        public static void RemoveModule(Module module)
        {
            module.is_being_installed_or_removed = true;
            FileHelper.RemoveValueFromFile(module.id, PATH.MODULES_LOCATION__DATA);
            foreach (Action f in pre_module_remove_functions)
                f.Invoke();
            TrashHandler.MoveDirectoryToTrash(module.path);
            AssetDatabase.Refresh();
        }

        private static List<Action> pre_module_remove_functions = new List<Action>();

        public static void RegisterPreModuleRemoveFunction(Action function)
        {
            pre_module_remove_functions.Add(function);
        }

        public static void OnEditorRemove()
        {
            string dir_path = ShaderEditor.GetShaderEditorDirectoryPath() + "/thry_modules";
            if (Directory.Exists(dir_path))
                TrashHandler.MoveDirectoryToTrash(dir_path);
        }
    }

}