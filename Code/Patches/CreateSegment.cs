namespace ZoningAdjuster
{
    using HarmonyLib;

    /// <summary>
    ///  Harmony postfix for enforcing zoning creation.
    /// </summary>
    [HarmonyPatch(typeof(RoadAI), nameof(RoadAI.CreateSegment))]
    public static class CreateSegmentPatch
    {
        /// <summary>
        /// Harmony Postfix patch to RoadAI.CreateSegment, to force any zoning creation.
        /// </summary>
        /// <param name="__instance">Road AI calling instance.</param>
        /// <param name="segmentID">Segment ID.</param>
        /// <param name="data">Segment data.</param>
        public static void Postfix(RoadAI __instance, ushort segmentID, ref NetSegment data)
        {
            // If road is set to not have zoning but we're set to force zoning, just manually call CreateZoneBlocks (bypassed in base game method).
            if (!__instance.m_enableZoning && ModSettings.forceZoning)
            {
                __instance.CreateZoneBlocks(segmentID, ref data);
            }
        }
    }
}