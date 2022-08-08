namespace ZoningAdjuster
{
    using HarmonyLib;

    /// <summary>
    ///  Harmony patch to release records related to a block when a block is released.
    /// </summary>
    [HarmonyPatch(typeof(ZoneManager), nameof(ZoneManager.ReleaseBlock))]
    public static class ReleaseBlockPatch
    {
        /// <summary>
        /// Harmony Postfix patch to ZoneManager.ReleaseBlock, to release records related to a block when a block is released.
        /// </summary>
        /// <param name="block"></param>
        public static void Postfix(ref ushort block)
        {
            // Clear recorded data for this zone block ID.
            ZoneBlockData.Instance.ClearEntry(block);
        }
    }
}