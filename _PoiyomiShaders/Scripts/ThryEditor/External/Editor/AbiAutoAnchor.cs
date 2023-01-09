#if CVR_CCK_EXISTS
using System;
using UnityEngine;
using UnityEditor;
using ABI.CCK.Scripts.Editor;

namespace Pumkin.UploadCallbacks
{
    [InitializeOnLoad]
    public class AbiAutoAnchor
    {
        static AbiAutoAnchor()
        {
            CCK_BuildUtility.PreAvatarBundleEvent.AddListener(OnPreBundleEvent);
        }

        static void OnPreBundleEvent(GameObject uploadedObject)
        {
            try
            {
                UploadAnchorOverrideSetter.SetAnchorOverrides(uploadedObject);
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
#endif