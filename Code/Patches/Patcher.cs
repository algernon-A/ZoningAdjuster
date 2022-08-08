namespace ZoningAdjuster
{
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using System.Reflection;
    using CitiesHarmony.API;
    using HarmonyLib;

    /// <summary>
    /// Class to manage the mod's Harmony patches.
    /// </summary>
    public class Patcher : PatcherBase
    {
        // CreateZoneBlock patch and target methods.
        private MethodInfo _createZoneBlocks;
        private MethodInfo _zoneBlocksPatch;

        /// <summary>
        /// Initializes a new instance of the <see cref="Patcher"/> class.
        /// </summary>
        /// <param name="harmonyID">This mod's unique Harmony identifier.</param>
        public Patcher(string harmonyID)
            : base(harmonyID)
        {
        }

        /// <summary>
        /// Gets the active instance reference.
        /// </summary>
        public static new Patcher Instance
        {
            get
            {
                // Auto-initializing getter.
                if (s_instance == null)
                {
                    s_instance = new Patcher(PatcherMod.Instance.HarmonyID);
                }

                return s_instance as Patcher;
            }
        }

        /// <summary>
        /// Apply all Harmony patches.
        /// </summary>
        public override void PatchAll()
        {
            // Don't do anything if already patched.
            if (!Patched)
            {
                // Ensure Harmony is ready before patching.
                if (HarmonyHelper.IsHarmonyInstalled)
                {
                    Logging.Message("deploying Harmony patches");

                    // Apply all annotated patches and update flag.
                    Harmony harmonyInstance = new Harmony(HarmonyID);
                    harmonyInstance.PatchAll();
                    Patched = true;

                    // Apply CreateZoneBlocks patch.
                    _createZoneBlocks = typeof(RoadAI).GetMethod(nameof(RoadAI.CreateZoneBlocks));
                    _zoneBlocksPatch = typeof(ZoneBlockPatch).GetMethod(nameof(ZoneBlockPatch.Prefix));
                    PatchCreateZoneBlocks();
                }
                else
                {
                    Logging.Message("Harmony not ready");
                }
            }
        }

        /// <summary>
        /// Applys the CreateZoneBlocks Harmony patch.
        /// </summary>
        public void PatchCreateZoneBlocks()
        {
            Logging.KeyMessage("patching UnpatchCreateZoneBlocks");
            Harmony harmonyInstance = new Harmony(HarmonyID);
            harmonyInstance.Patch(_createZoneBlocks, prefix: new HarmonyMethod(_zoneBlocksPatch));
        }

        /// <summary>
        /// Removes the CreateZoneBlocks Harmony patch.
        /// </summary>
        public void UnpatchCreateZoneBlocks()
        {
            Logging.KeyMessage("unpatching UnpatchCreateZoneBlocks");
            Harmony harmonyInstance = new Harmony(HarmonyID);
            harmonyInstance.Unpatch(_createZoneBlocks, HarmonyPatchType.Prefix, HarmonyID);
        }
    }
}