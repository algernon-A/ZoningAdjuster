﻿using ColossalFramework.UI;


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

                    // Backup component reference; we're looking for RoadsOptionPanel(RoadsPanel) as first preference if it exists, with plain RoadsOptionPanel as the backup. 
                    UIComponent backupComponent = null;
                    foreach (UIComponent component in UnityEngine.Object.FindObjectsOfType<UIComponent>())
                    {
                        // Try to find RoadsPanel as first preference.
                        if (component.name.Equals("RoadsOptionPanel(RoadsPanel)"))
                        {
                            Logging.Message("found RoadsOptionPanel(RoadsPanel)");
                            _roadOptionsPanel = component;
                            return component;
                        }
                        // If that fails, our backup is generic RoadsOptionPanel.
                        else if (component.name.Equals("RoadsOptionPanel"))
                        {
                            Logging.Message("found RoadsOptionPanel)");
                            backupComponent = component;
                        }
                    }

                    // If we got here, then we didn't find RoadsOptionPanel(RoadsPanel); use the backup if we have it.
                    if (backupComponent != null)
                    {
                        return backupComponent;
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
