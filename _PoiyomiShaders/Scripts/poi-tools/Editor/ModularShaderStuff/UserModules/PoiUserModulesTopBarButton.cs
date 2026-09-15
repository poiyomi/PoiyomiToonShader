using Thry.ThryEditor;
using UnityEditor;
using UnityEngine;

namespace Poi.Tools
{
    /// <summary>
    /// Puts a modules button in the material inspector's top bar that opens Installed Modules, so the
    /// module list is reachable from where people actually are — editing a material — rather than
    /// only from the Poi menu.
    ///
    /// Registered rather than called directly: ThryEditor can't reference this assembly, since the
    /// dependency runs the other way. The action lives here and uses the UI package's shared icons.
    /// </summary>
    [InitializeOnLoad]
    public static class PoiUserModulesTopBarButton
    {
        static PoiUserModulesTopBarButton()
        {
            TopBarButtons.Register(IconStyle, "Installed Modules", () => PoiUserModulesWindow.Open());
        }

        /// <summary>
        /// Built on demand rather than at registration — the top bar resolves this during OnGUI,
        /// and building GUIStyles before Unity's GUI is up is not safe.
        /// </summary>
        static GUIStyle IconStyle()
        {
            return ToolbarIcons.Modules;
        }
    }
}
