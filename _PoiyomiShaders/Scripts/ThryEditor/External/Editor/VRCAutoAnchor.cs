#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
using System;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace Thry.ThryEditor.UploadCallbacks // sry Pumkin for taking away your namespace. Just tring to tidy up a bit
{
    public class VRCAutoAnchor : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => 0;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            try
            {
                if(!UploadAnchorOverrideSetter.ShouldSkipAvatar(avatarGameObject))
                    UploadAnchorOverrideSetter.SetAnchorOverrides(avatarGameObject);
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }
            return true;
        }
    }
}
#endif
