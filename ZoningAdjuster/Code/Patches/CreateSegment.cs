using HarmonyLib;


namespace ZoningAdjuster
{
    /// <summary>
    ///  Harmony postfix for enforcing zoning creation.
    /// </summary>
    [HarmonyPatch(typeof(RoadAI), nameof(RoadAI.CreateSegment))]
    public static class CreateSegmentPatch
    {
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