using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Poi.Tools
{
    [InitializeOnLoad]
    public static class PoiGraphicsAPICheck
    {
        const string LogPrefix = "[<color=#E75898ff>Poiyomi Graphics API Check</color>] ";
        const string NotifiedKey = "Poi.GraphicsApiCheck.Notified";
        const string MetalOptOutKey = "Poi.GraphicsApiCheck.MetalOptOut";
        const string DialogTitle = "Poiyomi: Unsupported Graphics API";

        // Null when editor runs on supported API.
        static readonly string Message;
        static readonly bool IsError;

        static PoiGraphicsAPICheck()
        {
            if (Application.isBatchMode) return;

            var api = SystemInfo.graphicsDeviceType;
            switch (api)
            {
                case GraphicsDeviceType.OpenGLCore:
                case GraphicsDeviceType.OpenGLES2:
                case GraphicsDeviceType.OpenGLES3:
                    IsError = true;
                    Message = $"Unity is running on {api}. Poiyomi Shaders does not support OpenGL and will not compile.\n\n{OpenGLFix()}";
                    break;
                case GraphicsDeviceType.Metal:
                    Message = "Unity is running on Metal. Metal has no geometry shader stage, so shaders that need one (Fur, Geom, Geometric Dissolve, ShatterWave and Particle) cannot be rendered.\n\n" + "Additionally, Unity on macOS does not support forcing a different Graphics API besides Metal.\n\n" + "We apologize for this inconvenience.";
                    break;
                case GraphicsDeviceType.Direct3D11:
                case GraphicsDeviceType.Vulkan:
                    break;
                default:
                    return;
            }

            if (SessionState.GetBool(NotifiedKey, false)) return;
            SessionState.SetBool(NotifiedKey, true);
            
            if (Message == null)
            {
                Debug.Log(LogPrefix + $"Unity is running on {api}.");
                return;
            }
            EditorApplication.delayCall += Notify;
        }

        static string OpenGLFix()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.LinuxEditor:
                    return "Please restart Unity on Vulkan by adding -force-vulkan to its Command-Line Arguments:\n" + "- ALCOM: Settings > Default Unity Command-Line Arguments\n" + "- Unity Hub: the project's ... menu > Add Command Line Arguments";
                default:
                    return "Poiyomi Shaders only compiles in the following APIs:\n" + "- Direct3D 11\n" + "- Vulkan (Experimental)";
            }
        }

        static void Notify()
        {
            if (IsError)
            {
                Debug.LogError(LogPrefix + Message);
                EditorUtility.DisplayDialog(DialogTitle, Message, "I Understand");
            }
            else
            {
                Debug.LogWarning(LogPrefix + Message);
                // Because macOS has no other Graphics API to switch to, show this message only once.
                EditorUtility.DisplayDialog(DialogTitle, Message, "I Understand", DialogOptOutDecisionType.ForThisMachine, MetalOptOutKey);
            }
        }

        internal static void DrawInspectorWarning(Material material)
        {
            if (Message == null || material == null || !IsPoiyomi(material.shader)) return;
            EditorGUILayout.HelpBox(Message, IsError ? MessageType.Error : MessageType.Warning);
        }

        static bool IsPoiyomi(Shader shader) => shader != null && (shader.name.StartsWith(".poiyomi/", StringComparison.OrdinalIgnoreCase) || shader.name.StartsWith("Hidden/Locked/.poiyomi/", StringComparison.OrdinalIgnoreCase));
    }
}
