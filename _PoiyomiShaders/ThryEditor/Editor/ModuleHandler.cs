// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    public class ModuleHandler
    {
        

        private static List<ModuleHeader> modules;
        private static bool modules_are_being_loaded = false;

        public static List<ModuleHeader> GetModules()
        {
            if (!modules_are_being_loaded)
                LoadModules();
            return modules;
        }

        private static void LoadModules()
        {
            modules_are_being_loaded = true;
            WebHelper.DownloadStringASync(URL.PUBLIC_MODULES_COLLECTION, delegate (string s) {
                modules = new List<ModuleHeader>();
                List<string> module_urls = Parser.ParseToObject<List<string>>(s);
                foreach(string url in module_urls)
                {
                    WebHelper.DownloadStringASync(url, delegate (string data)
                     {
                         ModuleHeader new_module = new ModuleHeader();
                         new_module.url = url;
                         new_module.available_module = Parser.ParseToObject<ModuleInfo>(data);
                         InitInstalledModule(new_module);
                         if (new_module.available_module.requirement != null)
                             new_module.available_requirement_fullfilled = new_module.available_module.requirement.Test();
                         modules.Add(new_module);
                         //Debug.Log(Parser.ObjectToString(new_module));
                         UnityHelper.RepaintEditorWindow(typeof(Settings));
                     });
                }
            });
        }

        private static void InitInstalledModule(ModuleHeader m)
        {
            if (Helper.ClassExists(m.available_module.classname))
            {
                string module_path = GetModuleDirectoryPath(m);
                string path = module_path + "/module.json";
                if (File.Exists(path))
                {
                    m.installed_module = Parser.ParseToObject<ModuleInfo>(FileHelper.ReadFileIntoString(path));
                    m.path = module_path;
                }
            }
        }

        public static void InstallRemoveModule(ModuleHeader module, bool install)
        {
            if (install && !Helper.ClassExists(module.available_module.classname))
                InstallModule(module);
            else if (!install && Helper.ClassExists(module.available_module.classname))
                RemoveModule(module);
        }

        public static void OnCompile()
        {
            string url = FileHelper.LoadValueFromFile("update_module_url", PATH.AFTER_COMPILE_DATA);
            string name = FileHelper.LoadValueFromFile("update_module_name", PATH.AFTER_COMPILE_DATA);
            if (url != null && url.Length > 0 && name != null && name.Length > 0)
            {
                InstallModule(url, name);
                FileHelper.SaveValueToFile("update_module_url", "", PATH.AFTER_COMPILE_DATA);
                FileHelper.SaveValueToFile("update_module_url", "", PATH.AFTER_COMPILE_DATA);
            }
        }

        public static void UpdateModule(ModuleHeader module)
        {
            module.is_being_installed_or_removed = true;
            FileHelper.SaveValueToFile("update_module_url", module.url, PATH.AFTER_COMPILE_DATA);
            FileHelper.SaveValueToFile("update_module_name", module.available_module.name, PATH.AFTER_COMPILE_DATA);
            RemoveModule(module);
        }


        public static void InstallModule(ModuleHeader module)
        {

            module.is_being_installed_or_removed = true;
            InstallModule(module.url, module.available_module.name);
        }

        private static void InstallModule(string url, string name)
        {
            EditorUtility.DisplayProgressBar( name + " download progress", "", 0);
            WebHelper.DownloadStringASync(url, delegate (string s)
            {
                if (s.StartsWith("404"))
                {
                    Debug.LogWarning(s);
                    return;
                }
                //Debug.Log(s);
                ModuleInfo module_info = Parser.ParseToObject<ModuleInfo>(s);
                string thry_modules_path = ThryEditor.GetThryEditorDirectoryPath();
                string temp_path = "temp_" + name;
                if (thry_modules_path == null)
                    thry_modules_path = "Assets";
                thry_modules_path += "/thry_modules";
                string install_path = thry_modules_path + "/" + name;
                string base_url = url.RemoveFileName();
                FileHelper.WriteStringToFile(s, temp_path + "/module.json");
                int i = 0;
                foreach (string f in module_info.files)
                {
                    //Debug.Log(base_url + f);
                    WebHelper.DownloadFileASync(base_url + f, temp_path + "/" + f, delegate (string data)
                    {
                        i++;
                        EditorUtility.DisplayProgressBar("Downloading files for "+name, "Downloaded "+ base_url + f, (float)i / module_info.files.Count);
                        if (i == module_info.files.Count)
                        {
                            EditorUtility.ClearProgressBar();
                            if (!Directory.Exists(thry_modules_path))
                                Directory.CreateDirectory(thry_modules_path);
                            Directory.Move(temp_path, install_path);
                            AssetDatabase.Refresh();
                        }
                    });
                }
            });
        }

        public static void RemoveModule(ModuleHeader module)
        {
            module.is_being_installed_or_removed = true;
            foreach (Action f in pre_module_remove_functions)
                f.Invoke();
            TrashHandler.MoveDirectoryToTrash(module.path);
            AssetDatabase.Refresh();
        }

        private static string GetModuleDirectoryPath(ModuleHeader module)
        {
            string[] guids = AssetDatabase.FindAssets(module.available_module.settings_file_name.RemoveFileExtension());
            foreach(string g in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                if (path.EndsWith(module.available_module.settings_file_name.RemoveFileExtension() + ".cs"))
                {
                    path = path.GetDirectoryPath().RemoveOneDirectory();
                    if (Directory.Exists(path))
                        return path;
                }
            }
            return "";
        }

        private static List<Action> pre_module_remove_functions = new List<Action>();

        public static void RegisterPreModuleRemoveFunction(Action function)
        {
            pre_module_remove_functions.Add(function);
        }

        public static void OnEditorRemove()
        {
            string dir_path = ThryEditor.GetThryEditorDirectoryPath() + "/thry_modules";
            if (Directory.Exists(dir_path))
                TrashHandler.MoveDirectoryToTrash(dir_path);
        }
    }

}