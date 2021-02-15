#if VRC_SDK_VRCSDK2 || VRC_SDK_VRCSDK3
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace One
{
    public class LockMaterialsOnUpload : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => 100;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            Thry.ShaderOptimizer.SetLockForAllChildren(avatarGameObject, 1);

            return true;
        }
    }
}
#endif