using ColossalFramework.UI;


namespace ZoningAdjuster
{
    /// <summary>
    /// UIComponent to handle zoning settings panel visibility on road panel toggling.
    /// </summary>
    public class RoadOptionsToggle : UIComponent
    {
        // Instance reference.
        internal static bool IsVisible { get; private set; }


        /// <summary>
        /// Constructor.
        /// </summary>
        public RoadOptionsToggle()
        {
            // Set visibility flag.
            IsVisible = this.isVisible;
        }


        /// <summary>
        /// Event handler for game RoadOptionsPanel visibility change.  Used to display/hide the panel if the appropriate game setting is set.
        /// </summary>
        /// <param name="isVisible">New visibility state</param>
        protected override void OnVisibilityChanged()
        {
            Logging.Message("Road panel visibility changed to ", isVisible);

            // Show panel if RoadOptionsPanel is visible and we've got the relevant setting set.
            if (isVisible)
            {
                if (ModSettings.showOnRoad)
                {
                    ZoningSettingsPanel.Create();
                }
            }
            else
            {
                // Hide panel if RoadOptionsPanel is no longer visible and the tool isn't active.
                if (!ZoningTool.IsActiveTool)
                {
                    ZoningSettingsPanel.Close();
                }
            }


            // Update visibility flag.
            IsVisible = isVisible;
        }
    }
}
