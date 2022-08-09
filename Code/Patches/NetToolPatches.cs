// <copyright file="NetToolPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ZoningAdjuster
{
    using HarmonyLib;

    /// <summary>
    /// Harmony patch to toggle settings panel on road prefab selection/deselection.
    /// </summary>
    [HarmonyPatch(typeof(NetTool))]
    public static class NetToolPatches
    {
        /// <summary>
        /// Harmony Postfix to NetTool.Prefab setter, to toggle settings panel status according to if the newly-selected prefab is a road.
        /// </summary>
        /// <param name="value">Selected network prefab.</param>
        [HarmonyPatch(nameof(NetTool.Prefab), MethodType.Setter)]
        [HarmonyPostfix]
        public static void SetPrefab(NetInfo value)
        {
            // Is this a road?
            if (value?.GetAI() is RoadAI)
            {
                // Yes - show panel if we've got the setting enabled.
                if (ModSettings.ShowOnRoad)
                {
                    ZoningSettingsPanel.Create();
                }
            }
            else
            {
                // Close panel on selection of any non-road prefab.
                ZoningSettingsPanel.Close();
            }
        }

        /// <summary>
        /// Harmony Postfix to NetTool.OnEnable to open settings panel when NetTool is selected with a road prefab.
        /// </summary>
        /// <param name="__instance">Calling NetTool instance.</param>
        [HarmonyPatch("OnEnable")]
        [HarmonyPostfix]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
        public static void OnEnable(NetTool __instance)
        {
            if (__instance.Prefab?.GetAI() is RoadAI && ModSettings.ShowOnRoad)
            {
                ZoningSettingsPanel.Create();
            }
        }

        /// <summary>
        /// Harmony Postfix to NetTool.OnDisable to close settings panel when NetTool is deselected.
        /// </summary>
        [HarmonyPatch("OnDisable")]
        [HarmonyPostfix]
        public static void OnDisable()
        {
            ZoningSettingsPanel.Close();
        }
    }
}