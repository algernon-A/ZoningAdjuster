namespace ZoningAdjuster
{
    /// <summary>
    /// Static class to hold global mod settings.
    /// </summary>
    internal static class ModSettings
    {
        // What's new notification version.
        internal static string whatsNewVersion = "0.0";
        internal static int whatsNewBetaVersion = 0;

        // Panel postion.
        internal static float panelX = -1;
        internal static float panelY = -1;

        // Button postion.
        internal static float buttonX = -1;
        internal static float buttonY = -1;

        // Panel visibility.
        internal static bool showOnRoad = true;

        // Current zoning settings.
        internal static bool disableZoning = false;
        internal static bool forceZoning = false;
    }
}