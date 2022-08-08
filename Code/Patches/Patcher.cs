using HarmonyLib;
using CitiesHarmony.API;
using System.Reflection;


namespace ZoningAdjuster
{
    /// <summary>
    /// Class to manage the mod's Harmony patches.
    /// </summary>
    public static class Patcher
    {
        // Unique harmony identifier.
        private const string harmonyID = "algernon-A.csl.zoningadjuster";

        // CreateZoneBlock patch target method.

        private static MethodInfo createZoneBlocks, zoneBlocksPatch;

        // Flag.
        internal static bool Patched => _patched;
        private static bool _patched = false;


        /// <summary>
        /// Apply all Harmony patches.
        /// </summary>
        public static void PatchAll()
        {
            // Don't do anything if already patched.
            if (!_patched)
            {
                // Ensure Harmony is ready before patching.
                if (HarmonyHelper.IsHarmonyInstalled)
                {
                    Logging.Message("deploying Harmony patches");

                    // Apply all annotated patches and update flag.
                    Harmony harmonyInstance = new Harmony(harmonyID);
                    harmonyInstance.PatchAll();
                    _patched = true;

                    // Apply CreateZoneBlocks patch.
                    createZoneBlocks = typeof(RoadAI).GetMethod(nameof(RoadAI.CreateZoneBlocks));
                    zoneBlocksPatch = typeof(ZoneBlockPatch).GetMethod(nameof(ZoneBlockPatch.Prefix));
                    PatchCreateZoneBlocks();
                }
                else
                {
                    Logging.Message("Harmony not ready");
                }
            }
        }


        /// <summary>
        /// Remove all Harmony patches.
        /// </summary>
        public static void UnpatchAll()
        {
            // Only unapply if patches appplied.
            if (_patched)
            {
                Logging.Message("reverting Harmony patches");

                // Unapply patches, but only with our HarmonyID.
                Harmony harmonyInstance = new Harmony(harmonyID);
                harmonyInstance.UnpatchAll(harmonyID);
                _patched = false;
            }
        }


        /// <summary>
        /// Applys the CreateZoneBlocks Harmony patch.
        /// </summary>
        public static void PatchCreateZoneBlocks()
        {
            Logging.KeyMessage("patching UnpatchCreateZoneBlocks");
            Harmony harmonyInstance = new Harmony(harmonyID);
            harmonyInstance.Patch(createZoneBlocks, prefix: new HarmonyMethod(zoneBlocksPatch));
        }


        /// <summary>
        /// Removes the CreateZoneBlocks Harmony patch.
        /// </summary>
        public static void UnpatchCreateZoneBlocks()
        {
            Logging.KeyMessage("unpatching UnpatchCreateZoneBlocks");
            Harmony harmonyInstance = new Harmony(harmonyID);
            harmonyInstance.Unpatch(createZoneBlocks, HarmonyPatchType.Prefix, harmonyID);
        }
    }
}