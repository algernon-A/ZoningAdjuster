using ColossalFramework.UI;


namespace ZoningAdjuster
{
    /// <summary>
    /// Static class to get references to game UI components.
    /// </summary>
    class GameUIComponents
    {
        // Game's RoadOptionsPanel.

        /// <summary>
        /// Returns the game's RoadOptionPanel.
        /// </summary>
        /// <returns>RoadOptionPanel as UIComponent (null if not found)</returns>
        internal static UIComponent RoadsOptionPanel
        {
            get
            {
                // Check to see if we've already got a reference; if so, return it.
                if (_roadOptionsPanel != null)
                {
                    return _roadOptionsPanel;
                }
                else
                {
                    // No stored reference; try to find it.
                    foreach (UIComponent component in UnityEngine.Object.FindObjectsOfType<UIComponent>())
                    {
                        if (component.name.Equals("RoadsOptionPanel"))
                        {
                            Logging.Message("found RoadsOptionPanel");
                            _roadOptionsPanel = component;
                            return component;
                        }
                    }
                }

                // If we got here, we didn't get a reference; return null.
                Logging.Message("couldn't find RoadsOptionPanel");
                return null;
            }
        }
        private static UIComponent _roadOptionsPanel;
    }
}
