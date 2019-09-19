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
        private const string THRY_MODULES_URL = "https://raw.githubusercontent.com/Thryrallo/ThryEditor/master/modules.txt";

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
            Helper.getStringFromUrl(THRY_MODULES_URL, delegate (string s) {
                modules = Parsers.ParseToObject<List<ModuleHeader>>(s);
                InitModulesVersions();
                Helper.RepaintEditorWindow(typeof(Settings));
            });
        }

        private static void InitModulesVersions()
        {
            foreach(ModuleHeader m in modules)
            {
                if (Helper.ClassExists(m.classname))
                {
                    string path = GetModuleDirectoryPath(m) + "/module.thry";
                    if (File.Exists(path))
                    {
                        m.installed_module = Parsers.ParseToObject<ModuleInfo>(Helper.ReadFileIntoString(path));
                    }
                }
            }
        }

        public static void InstallRemoveModule(ModuleHeader module, bool install)
        {
            if (install && !Helper.ClassExists(module.classname))
                InstallModule(module);
            else if (!install && Helper.ClassExists(module.classname))
                RemoveModule(module);
        }

        [InitializeOnLoad]
        public class Startup
        {
            static Startup()
            {
                string url = Helper.LoadValueFromFile("update_module_url", ".thry_after_compile_data");
                string name = Helper.LoadValueFromFile("update_module_name", ".thry_after_compile_data");
                if (url != null && url.Length > 0 && name != null && name.Length > 0)
                {
                    InstallModule(url, name);
                    Helper.SaveValueToFile("update_module_url", "", ".thry_after_compile_data");
                    Helper.SaveValueToFile("update_module_url", "", ".thry_after_compile_data");
                }
            }
        }

        public static void UpdateModule(ModuleHeader module)
        {
            module.is_being_installed_or_removed = true;
            Helper.SaveValueToFile("update_module_url", module.url, ".thry_after_compile_data");
            Helper.SaveValueToFile("update_module_name", module.name, ".thry_after_compile_data");
            RemoveModule(module);
        }


        public static void InstallModule(ModuleHeader module)
        {

            module.is_being_installed_or_removed = true;
            InstallModule(module.url, module.name);
        }

        private static void InstallModule(string url, string name)
        {
            Helper.getStringFromUrl(url, delegate (string s)
            {
                if (s.StartsWith("404"))
                {
                    Debug.LogWarning(s);
                    return;
                }
                //Debug.Log(s);
                ModuleInfo module_info = Parsers.ParseToObject<ModuleInfo>(s);
                string thry_modules_path = ThryEditor.GetThryEditorDirectoryPath();
                string temp_path = "temp_" + name;
                if (thry_modules_path == null)
                    thry_modules_path = "Assets";
                thry_modules_path += "/thry_modules";
                string install_path = thry_modules_path + "/" + name;
                string base_url = url.RemoveFileName();
                Helper.WriteStringToFile(s, temp_path + "/module.thry");
                int i = 0;
                foreach (string f in module_info.files)
                {
                    //Debug.Log(base_url + f);
                    Helper.DownloadBytesToPath(base_url + f, temp_path + "/" + f, delegate (string data)
                    {
                        i++;
                        if (i == module_info.files.Count)
                        {
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
            string path = GetModuleDirectoryPath(module);
            int i = 0;
            if (!Directory.Exists(Helper.DELETING_FOLDER))
                Directory.CreateDirectory(Helper.DELETING_FOLDER);
            string newpath = Helper.DELETING_FOLDER + "/" + module.name + i;
            while (Directory.Exists(newpath))
                newpath = Helper.DELETING_FOLDER + "/" + module.name + (++i);
            //Debug.Log(path + "," + newpath);
            Directory.Move(path, newpath);
            AssetDatabase.Refresh();
        }

        private static void DeleteDirectory(string path)
        {
            foreach(string p in Directory.GetFiles(path))
            {
                File.SetAttributes(p, FileAttributes.Normal);
                File.Delete(p);
            }
            foreach (string p in Directory.GetDirectories(path))
                DeleteDirectory(p);
            Directory.Delete(path);
        }

        private static string GetModuleDirectoryPath(ModuleHeader module)
        {
            string[] guids = AssetDatabase.FindAssets(module.settings_file_name.RemoveFileExtension());
            foreach(string g in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(g);
                if (path.EndsWith(module.settings_file_name.RemoveFileExtension() + ".cs"))
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
    }

}