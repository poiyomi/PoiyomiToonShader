using UnityEditor;

namespace Poi.Tools.ShaderTranslator.VersionUpgrade
{
    /// <summary>
    /// Wires the legacy-upgrade pipeline into <see cref="PoiLegacyUpgradeBridge"/> so poi-tools code (a separate
    /// asmdef that can't reference this assembly) can trigger it - e.g. the ErrorShaderEditor "Upgrade to 9.3" button.
    /// </summary>
    [InitializeOnLoad]
    static class LegacyUpgradeBridgeInstaller
    {
        static LegacyUpgradeBridgeInstaller()
        {
            PoiLegacyUpgradeBridge.IsLegacyMaterial = LegacyMaterialDetector.NeedsLegacyUpgrade;
            PoiLegacyUpgradeBridge.IsLegacyShaderMissing = LegacyMaterialDetector.IsLegacyShaderMissing;
            PoiLegacyUpgradeBridge.UpgradeToNine3 = PoiyomiUpgrade_9_X_to_9_3.UpgradeToNine3;
        }
    }
}
