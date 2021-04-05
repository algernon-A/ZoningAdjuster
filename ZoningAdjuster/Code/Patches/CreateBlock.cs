using HarmonyLib;


namespace ZoningAdjuster
{
    /// <summary>
    ///  Harmony postfix for recording current mod settings on block creation.
    /// </summary>
    [HarmonyPatch(typeof(ZoneManager), nameof(ZoneManager.CreateBlock))]
    public static class CreateBlockPatch
    {
        public static void Postfix(ref ushort block)
        {
            ZoneBlockData.Instance.SetCurrentMode(block); 
        }
    }
}