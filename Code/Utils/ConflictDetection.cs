namespace ZoningAdjuster
{
    using System.Collections.Generic;
    using System.Reflection;
    using AlgernonCommons;
    using ColossalFramework.Plugins;

    /// <summary>
    /// Mod conflict detection.
    /// </summary>
    internal static class ConflictDetection
    {
        // List of conflcting mod names.
        private static List<string> s_conflictingModNames;

        /// <summary>
        /// Gets the recorded list of conflicting mod names.
        /// </summary>
        internal static List<string> ConflictingModNames => s_conflictingModNames;

        /// <summary>
        /// Checks for any known fatal mod conflicts.
        /// </summary>
        /// <returns>True if a mod conflict was detected, false otherwise.</returns>
        internal static bool IsModConflict()
        {
            // Initialise flag and list of conflicting mods.
            bool conflictDetected = false;
            s_conflictingModNames = new List<string>();

            // Iterate through the full list of plugins.
            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                foreach (Assembly assembly in plugin.GetAssemblies())
                {
                    switch (assembly.GetName().Name)
                    {
                        case "NetworkExtensions2":
                            // Network Extensions 2 - check version.
                            if (assembly.GetName().Version <= new System.Version("1.0.0.0"))
                            {
                                conflictDetected = true;
                                s_conflictingModNames.Add("Network Extensions 2 v1.0");
                            }
                            break;
                        case "NetworkExtensions3":
                            // Network Extensions 3.
                            conflictDetected = true;
                            s_conflictingModNames.Add("Network Extensions 3");
                            break;
                        case "ZoneIt":
                            // Zone It.
                            conflictDetected = true;
                            s_conflictingModNames.Add("Zone It!");
                            break;
                        case "VanillaGarbageBinBlocker":
                            // Garbage Bin Controller
                            conflictDetected = true;
                            s_conflictingModNames.Add("Garbage Bin Controller");
                            break;
                        case "Painter":
                            // Painter - this one is trickier because both Painter and Repaint use Painter.dll (thanks to CO savegame serialization...)
                            if (plugin.userModInstance.GetType().ToString().Equals("Painter.UserMod"))
                            {
                                conflictDetected = true;
                                s_conflictingModNames.Add("Painter");
                            }
                            break;
                    }
                }
            }

            // Was a conflict detected?
            if (conflictDetected)
            {
                // Yes - log each conflict.
                foreach (string conflictingMod in s_conflictingModNames)
                {
                    Logging.Error("Conflicting mod found: ", conflictingMod);
                }

                Logging.Error("exiting due to mod conflict");
            }

            return conflictDetected;
        }
    }
}
