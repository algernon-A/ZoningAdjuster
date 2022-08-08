using HarmonyLib;


namespace ZoningAdjuster
{
    /// <summary>
    /// Harmony patch to toggle settings panel on road prefab selection/deselection.
    /// </summary>
    [HarmonyPatch(typeof(NetTool))]
    public static class NetToolPatches
    {
        /// <summary>
        /// Harmony Postfix to NetTool.Prefab setter, to toggle settings panel status according to if the newly-selected prefab is a road.
        /// </summary>
        /// <param name="value"></param>
        [HarmonyPatch(nameof(NetTool.Prefab), MethodType.Setter)]
        [HarmonyPostfix]
        public static void SetPrefab(NetInfo value)
        {
            // Is this a road?
            if (value?.GetAI() is RoadAI)
            {
                // Yes - show panel if we've got the setting enabled.
                if (ModSettings.showOnRoad)
                {
                    ZoningSettingsPanel.Create();
                }
            }
            else
            {
                // Close panel on selection of any non-road prefab.
                ZoningSettingsPanel.Close();
            }
        }


        /// <summary>
        /// Harmony Postfix to NetTool.OnEnable to open settings panel when NetTool is selected with a road prefab.
        /// </summary>
        [HarmonyPatch("OnEnable")]
        [HarmonyPostfix]
        public static void OnEnable(NetTool __instance)
        {
            if (__instance.Prefab?.GetAI() is RoadAI && ModSettings.showOnRoad)
            {
                ZoningSettingsPanel.Create();
            }
        }


        /// <summary>
        /// Harmony Postfix to NetTool.OnDisable to close settings panel when NetTool is deselected.
        /// </summary>
        [HarmonyPatch("OnDisable")]
        [HarmonyPostfix]
        public static void OnDisable()
        {
            ZoningSettingsPanel.Close();
        }
    }
}