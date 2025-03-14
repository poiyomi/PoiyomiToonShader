#if CVR_CCK_EXISTS
using System;
using UnityEngine;
using UnityEditor;
using ABI.CCK.Scripts.Editor;

namespace Thry.ThryEditor.UploadCallbacks // sry Pumkin for taking away your namespace. Just tring to tidy up a bit
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
                if(!UploadAnchorOverrideSetter.ShouldSkipAvatar(uploadedObject))
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
