namespace ZoningAdjuster
{
    using HarmonyLib;

    /// <summary>
    ///  Harmony postfix for recording current mod settings on block creation.
    /// </summary>
    [HarmonyPatch(typeof(ZoneManager), nameof(ZoneManager.CreateBlock))]
    public static class CreateBlockPatch
    {
        /// <summary>
        /// Harmony Postifx patch for ZoneManager.CreateBlock to record current mod settings in effect when the block is created.
        /// </summary>
        /// <param name="block"></param>
        public static void Postfix(ref ushort block)
        {
            // Record current mod settings against this zone block ID.
            ZoneBlockData.Instance.SetCurrentMode(block); 
        }
    }
}