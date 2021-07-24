using ColossalFramework;


namespace ZoningAdjuster
{
    /// <summary>
    /// Static class to hold global mod settings.
    /// </summary>
    internal static class ModSettings
    {
        // C:S settings file name (for UUI).
        internal const string SettingsFileName = "ZoningAdjuster";

        // What's new notification version.
        internal static string whatsNewVersion = "0.0";
        internal static int whatsNewBetaVersion = 0;

        // Panel postion and behaviour.
        private static bool showPanel = true;
        internal static float panelX = -1;
        internal static float panelY = -1;

        // Button postion.
        internal static float buttonX = -1;
        internal static float buttonY = -1;

        internal static bool ShowPanel
        {
            get => showPanel;

            set
            {
                showPanel = value;

                if (ZoningSettingsPanel.Panel != null && Singleton<ToolController>.instance.CurrentTool != ZoningTool.Instance)
                {
                    ZoningSettingsPanel.Panel.isVisible = value;
                }
            }
        }
    }
}