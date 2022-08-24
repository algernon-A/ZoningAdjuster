// <copyright file="BuildingToolPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ZoningAdjuster
{
    using AlgernonCommons.Patching;
    using HarmonyLib;

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
            PatcherManager<Patcher>.Instance.UnpatchCreateZoneBlocks();
        }

        /// <summary>
        /// Harmony Prefix patch to BuildingTool.OnEnable to reappky zoning creation patches when the building tool is activated.
        /// </summary>
        [HarmonyPatch("OnDisable")]
        [HarmonyPrefix]
        public static void OnDisable()
        {
            PatcherManager<Patcher>.Instance.PatchCreateZoneBlocks();
        }
    }
}