// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Thry
{
    [InitializeOnLoad]
    public class OnCompileHandler
    {
        static OnCompileHandler()
        {
            //Init Editor Variables with paths
            ThryEditor.GetThryEditorDirectoryPath();

            VRCInterface.OnCompile();
            Config.OnCompile();
            ModuleHandler.OnCompile();
            ShaderImportFixer.OnCompile();
            TrashHandler.EmptyThryTrash();

            UnityFixer.CheckAPICompatibility(); //check that Net_2.0 is ApiLevel
            UnityFixer.CheckDrawingDll(); //check that drawing.dll is imported
        }
    }

    public class AssetChangeHandler : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (importedAssets.Length > 0)
                AssetsImported(importedAssets);
            if (deletedAssets.Length > 0)
                AssetsDeleted(deletedAssets);
            if (movedAssets.Length > 0)
                AssetsMoved(movedAssets, movedFromAssetPaths);
        }

        private static void AssetsImported(string[] assets)
        {
            VRCInterface.SetVRCDefineSybolIfSDKImported(assets);
            ShaderImportFixer.OnImport(assets);
            ShaderHelper.AssetsImported(assets);
        }

        private static void AssetsMoved(string[] movedAssets, string[] movedFromAssetPaths)
        {
            ShaderHelper.AssetsMoved(movedFromAssetPaths, movedAssets);
        }

        private static void AssetsDeleted(string[] assets)
        {
            VRCInterface.SetVRCDefineSybolIfSDKDeleted(assets);
            ShaderHelper.AssetsDeleted(assets);
            UnityFixer.OnAssetDeleteCheckDrawingDLL(assets);
            if (CheckForEditorRemove(assets))
            {
                Debug.Log("ThryEditor is being deleted.");
                Config.Get().verion = "0";
                Config.Get().save();
                ModuleHandler.OnEditorRemove();
                VRCInterface.RemoveDefineSymbols();
            }
        }

        private static bool CheckForEditorRemove(string[] assets)
        {
            string test_for = ThryEditor.GetThryEditorDirectoryPath() + "/Editor/ThryEditor.cs";
            foreach (string p in assets)
            {
                if (p== test_for)
                    return true;
            }
            return false;
        }
    }
}