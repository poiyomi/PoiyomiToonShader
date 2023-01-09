#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
using System;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace Pumkin.UploadCallbacks
{
    public class VRCAutoAnchor : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => 0;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            try
            {
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