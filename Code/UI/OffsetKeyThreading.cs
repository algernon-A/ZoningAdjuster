// <copyright file="OffsetKeyThreading.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ZoningAdjuster
{
    using ICities;
    using UnityEngine;

    /// <summary>
    /// UI thread for offset key processing.
    /// </summary>
    public sealed class OffsetKeyThreading : ThreadingExtensionBase
    {
        // Hotkey setting.
        private static int s_offsetModifier;

        // State tracker.
        private static bool s_shiftOffset = false;

        /// <summary>
        /// Enum for offset modifier key.
        /// </summary>
        public enum ModifierEnum
        {
            /// <summary>
            /// Shift key.
            /// </summary>
            Shift = 0,

            /// <summary>
            /// Control key.
            /// </summary>
            Ctrl,

            /// <summary>
            /// Alt key/
            /// </summary>
            Alt,
        }

        /// <summary>
        /// Gets a value indicating whether the offset modfifier key is currently pressed.
        /// </summary>
        public static bool ShiftOffset => s_shiftOffset;

        /// <summary>
        /// Gets or sets the offset modifier key.
        /// </summary>
        public static int OffsetModifier { get => s_offsetModifier; set => s_offsetModifier = value; }

        /// <summary>
        /// Look for offset modifier keypress.
        /// </summary>
        /// <param name="realTimeDelta">Real time elapsed since last update.</param>
        /// <param name="simulationTimeDelta">Simulation time elapsed since last update.</param>
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            // Check offset key status.
            switch (s_offsetModifier)
            {
                case (int)ModifierEnum.Shift:
                    s_shiftOffset = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                    break;
                case (int)ModifierEnum.Ctrl:
                    s_shiftOffset = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                    break;
                case (int)ModifierEnum.Alt:
                    s_shiftOffset = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr);
                    break;
            }
        }
    }
}