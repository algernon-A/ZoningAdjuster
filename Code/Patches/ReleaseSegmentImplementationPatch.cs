// <copyright file="ReleaseSegmentImplementationPatch.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ZoningAdjuster
{
    using System;
    using HarmonyLib;

    /// <summary>
    ///  Harmony patch to release records related to a segment when a segment is released.
    /// </summary>
    [HarmonyPatch(typeof(NetManager), "ReleaseSegmentImplementation", new Type[] { typeof(ushort) })]
    public static class ReleaseSegmentImplementationPatch
    {
        /// <summary>
        /// Harmony Postfix patch to NetManager.ReleaseSegmentImplementation, to release records related to a block when a block is released.
        /// </summary>
        /// <param name="segment">Segment ID.</param>
        public static void Postfix(ushort segment)
        {
            // Clear recorded data for this segment ID.
            ZoneBlockData.Instance.ClearEntry(segment);
        }
    }
}