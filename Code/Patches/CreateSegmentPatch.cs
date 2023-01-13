// <copyright file="CreateSegmentPatch.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

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
        /// Harmony Postfix patch to RoadAI.CreateSegment, to force any zoning creation and record setttings at the time of creation.
        /// </summary>
        /// <param name="__instance">Road AI calling instance.</param>
        /// <param name="segmentID">Segment ID.</param>
        /// <param name="data">Segment data.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
        public static void Postfix(RoadAI __instance, ushort segmentID, ref NetSegment data)
        {
            // If road is set to not have zoning but we're set to force zoning, just manually call CreateZoneBlocks (bypassed in base game method).
            if (!__instance.m_enableZoning && ModSettings.ForceZoning)
            {
                __instance.CreateZoneBlocks(segmentID, ref data);
            }

            // Record current settings against this segment.
            SegmentData.Instance.UpdateCurrentMode(segmentID);
        }
    }
}