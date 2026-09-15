using System;
using UnityEngine;

namespace Poi.Tools
{
    /// <summary>
    /// Decouples poi-tools (the "Poi.Tools" asmdef) from the version-upgrade pipeline, which lives in the default
    /// editor assembly and so can't be referenced back across the asmdef boundary. The pipeline fills these delegates
    /// on load (see LegacyUpgradeBridgeInstaller in the Translators folder); code here invokes them if present.
    /// Everything is null-safe: if the pipeline isn't in the project, callers just fall back to their old behavior.
    /// </summary>
    public static class PoiLegacyUpgradeBridge
    {
        /// <summary>True if the material is a removed-version (pre-9.3) Poiyomi material the pipeline can upgrade.</summary>
        public static Func<Material, bool> IsLegacyMaterial;

        /// <summary>
        /// True only if the material is legacy AND its shader is genuinely missing (on the error shader, or a locked
        /// material whose source shader no longer resolves). False for legacy materials still rendering on a present
        /// pre-9.3 shader - use this to gate a "shader missing" prompt so it doesn't nag materials that still work.
        /// </summary>
        public static Func<Material, bool> IsLegacyShaderMissing;

        /// <summary>Upgrade the material onto Poiyomi 9.3, translating its values across. Returns true on success.</summary>
        public static Func<Material, bool> UpgradeToNine3;
    }
}
