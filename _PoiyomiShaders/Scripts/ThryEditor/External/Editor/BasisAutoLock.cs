#if BASIS_SDK_EXISTS
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Thry.ThryEditor.UploadCallbacks
{
    public class BasisAutoLock
    {
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            try
            {
                // Subscribe to the event
                BasisBundleBuild.PreBuildBundleEvents += HandlePreBuildEvent;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to hook to Basis Prebuild Events: {e.Message}");
                Debug.LogWarning("ThryEditor will not auto-lock materials for Basis builds. Please ensure you have the Basis SDK installed and configured correctly.");
            }
        }

        private static Task HandlePreBuildEvent(BasisContentBase basisContentBase, List<BuildTarget> targets)
        {
            var avatarObj = basisContentBase.gameObject;
            if (avatarObj == null) return Task.CompletedTask;

            Debug.Log($"Auto-locking materials for {avatarObj.name}");

            List<Material> materials = new List<Material>();
            materials.AddRange(avatarObj.GetComponentsInChildren<Renderer>(true)
                .SelectMany(r => r.sharedMaterials));
            
            // Find animation clips
            IEnumerable<AnimationClip> clips = avatarObj.GetComponentsInChildren<Animator>(true)
                .Where(a => a != null && a.runtimeAnimatorController != null)
                .Select(a => a.runtimeAnimatorController)
                .SelectMany(a => a.animationClips);

            var animator = avatarObj.GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                clips = clips.Concat(animator.runtimeAnimatorController.animationClips);
            }

            // Handle clips
            clips = clips.Distinct().Where(c => c != null);
            foreach (AnimationClip clip in clips)
            {
                IEnumerable<Material> clipMaterials = AnimationUtility.GetObjectReferenceCurveBindings(clip)
                    .Where(b => b.isPPtrCurve && b.type.IsSubclassOf(typeof(Renderer)) && b.propertyName.StartsWith("m_Materials"))
                    .SelectMany(b => AnimationUtility.GetObjectReferenceCurve(clip, b))
                    .Select(r => r.value as Material);
                materials.AddRange(clipMaterials);
            }

            materials = materials.Distinct().ToList();
            ShaderOptimizer.LockMaterials(materials);

            return Task.CompletedTask;
        }

        private static void Cleanup()
        {
            BasisBundleBuild.PreBuildBundleEvents -= HandlePreBuildEvent;
        }
    }
}
#endif