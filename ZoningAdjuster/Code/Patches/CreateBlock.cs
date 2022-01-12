using HarmonyLib;


namespace ZoningAdjuster
{
    /// <summary>
    ///  Harmony postfix for releasing block settings on block release.
    /// </summary>
    [HarmonyPatch(typeof(ZoneManager), nameof(ZoneManager.ReleaseBlock))]
    public static class ReleaseBlockPatch
    {
        public static void Postfix(ref ushort block)
        {
            // Clear recorded data for this zone block ID.
            ZoneBlockData.Instance.ClearEntry(block);
        }
    }
}