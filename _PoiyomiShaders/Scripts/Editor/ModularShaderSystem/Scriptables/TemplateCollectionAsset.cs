using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Poiyomi.ModularShaderSystem
{
    public class TemplateCollectionAsset : ScriptableObject
    {
        public List<TemplateAsset> Templates;

        public TemplateCollectionAsset()
        {
            Templates = new List<TemplateAsset>();
        }


        [MenuItem("Assets/Create/" + MSSConstants.CREATE_PATH + "/Template Collection", priority = 9)]
        private static void CreateTemplate()
        {
            Type projectWindowUtilType = typeof(ProjectWindowUtil);
            MethodInfo getActiveFolderPath = projectWindowUtilType.GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
            object obj = getActiveFolderPath.Invoke(null, new object[0]);
            string pathToCurrentFolder = obj.ToString();
            string uniquePath = AssetDatabase.GenerateUniqueAssetPath($"{pathToCurrentFolder}/Template.{MSSConstants.TEMPLATE_COLLECTION_EXTENSION}");
            
#if UNITY_6000_5_OR_NEWER
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(EntityId.None, ScriptableObject.CreateInstance<DoCreateNewAsset>(), uniquePath, null, null);
#else
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<DoCreateNewAsset>(), uniquePath, null, null);
#endif
        }
        
#if UNITY_6000_5_OR_NEWER
        internal class DoCreateNewAsset : AssetCreationEndAction
        {
            public override void Action(EntityId instanceId, string pathName, string resourceFile)
            {
                CreateTemplateCollectionAsset(pathName);
            }

            public override void Cancelled(EntityId instanceId, string pathName, string resourceFile) => Selection.activeObject = (Object) null;
        }
#else
        internal class DoCreateNewAsset : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                CreateTemplateCollectionAsset(pathName);
            }

            public override void Cancelled(int instanceId, string pathName, string resourceFile) => Selection.activeObject = (Object) null;
        }
#endif

        static void CreateTemplateCollectionAsset(string pathName)
        {
            File.WriteAllText(pathName, "");
            AssetDatabase.Refresh();
            Object o = AssetDatabase.LoadAssetAtPath<Object>(pathName);
            Selection.activeObject = o;
        }
    }
}