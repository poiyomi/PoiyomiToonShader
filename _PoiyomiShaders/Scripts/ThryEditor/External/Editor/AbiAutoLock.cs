#if CVR_CCK_EXISTS
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ABI.CCK.Scripts.Editor;
using ABI.CCK.Components;
using UnityEditor;
using System.Linq;

namespace Thry
{
    public class AbiAutoLock
    {
        [InitializeOnLoad]
        public class CVR_UploadLocking
        {
            static CVR_UploadLocking()
            {
                CCK_BuildUtility.PreAvatarBundleEvent.AddListener(OnPreBundleEvent);
                CCK_BuildUtility.PrePropBundleEvent.AddListener(OnPreBundleEvent);
            }

            static void OnPreBundleEvent(GameObject uploadedObject)
            {
                List<Material> materials = new List<Material>();
                CVRAvatar descriptor = uploadedObject.GetComponent<CVRAvatar>();

                // All renderers
                materials.AddRange(uploadedObject.GetComponentsInChildren<Renderer>(true).SelectMany(r => r.sharedMaterials));
                
                // Find animation clips
                IEnumerable<AnimationClip> clips = uploadedObject.GetComponentsInChildren<Animator>(true).Where(a => a != null && a.runtimeAnimatorController != null).
                    Select(a => a.runtimeAnimatorController).SelectMany(a => a.animationClips);
                if (descriptor != null && descriptor.overrides != null)
                    clips = clips.Concat(descriptor.overrides.animationClips);

                // Hanlde clips
                clips = clips.Distinct().Where(c => c != null);
                foreach (AnimationClip clip in clips)
                {
                    IEnumerable<Material> clipMaterials = AnimationUtility.GetObjectReferenceCurveBindings(clip).Where(b => b.isPPtrCurve && b.type.IsSubclassOf(typeof(Renderer)) && b.propertyName.StartsWith("m_Materials"))
                        .SelectMany(b => AnimationUtility.GetObjectReferenceCurve(clip, b)).Select(r => r.value as Material);
                    materials.AddRange(clipMaterials);
                }

                materials = materials.Distinct().ToList();
                ShaderOptimizer.SetLockedForAllMaterials(materials, 1, showProgressbar: true, showDialog: PersistentData.Get<bool>("ShowLockInDialog", true), allowCancel: false);
            }
        }
    }
}
#endif
