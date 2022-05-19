using HarmonyLib;


namespace ZoningAdjuster
{
    /// <summary>
    /// Harmony patches to the building tool to disable custom zoning options when the building tool is selected.
    /// </summary>
    [HarmonyPatch(typeof(BuildingTool))]
    public static class BuildingToolPatches
    {
        /// <summary>
        /// Harmony Prefix patch to BuildingTool.OnEnable to unapply zoning creation patches when the building tool is activated.
        /// </summary>
        [HarmonyPatch("OnEnable")]
        [HarmonyPrefix]
        public static void OnEnable()
        {
            Patcher.UnpatchCreateZoneBlocks();
        }


        /// <summary>
        /// Harmony Prefix patch to BuildingTool.OnEnable to reappky zoning creation patches when the building tool is activated.
        /// </summary>
        [HarmonyPatch("OnDisable")]
        [HarmonyPrefix]
        public static void OnDisable()
        {
            Patcher.PatchCreateZoneBlocks();
        }
    }
}