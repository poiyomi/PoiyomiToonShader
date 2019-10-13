// Material/Shader Inspector for Unity 2017/2018
// Copyright (C) 2019 Thryrallo

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
            VRCInterface.OnCompile();
            Config.OnCompile();
            ModuleHandler.OnCompile();
            Helper.TashHandler.EmptyThryTrash();
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
        }
    }
}