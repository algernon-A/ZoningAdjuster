// <copyright file="Patcher.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ZoningAdjuster
{
    using System.Reflection;
    using AlgernonCommons;
    using AlgernonCommons.Patching;
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
        public Patcher()
            : base()
        {
            _createZoneBlocks = typeof(RoadAI).GetMethod(nameof(RoadAI.CreateZoneBlocks));
            _zoneBlocksPatch = typeof(CreateZoneBlocks).GetMethod(nameof(CreateZoneBlocks.Prefix));
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
                    _zoneBlocksPatch = typeof(CreateZoneBlocks).GetMethod(nameof(CreateZoneBlocks.Prefix));
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

        /// <summary>
        /// Peforms any additional actions (such as custom patching) after PatchAll is called.
        /// </summary>
        /// <param name="harmonyInstance">Haromny instance for patching.</param>
        protected override void OnPatchAll(Harmony harmonyInstance) => PatchCreateZoneBlocks();
    }
}