using UnityEngine;
using ICities;


namespace ZoningAdjuster
{
    public class UIThreading : ThreadingExtensionBase
    {
        public enum ModifierEnum
        {
            shift = 0,
            ctrl,
            alt
        }


        // Flags.
        internal static bool operating = true;
        private bool _processed = false;

        // Hotkey setting.
        internal static int offsetModifier;

        // State tracker.
        internal static bool shiftOffset = false;

        /// <summary>
        /// Look for keypress to open GUI.
        /// </summary>
        /// <param name="realTimeDelta"></param>
        /// <param name="simulationTimeDelta"></param>
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            // Don't do anything if not active.
            if (operating)
            {
                // Has hotkey been pressed?
                bool offsetKeyPressed = false;
                switch (offsetModifier)
                {
                    case (int)ModifierEnum.shift:
                        offsetKeyPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                        break;
                    case (int)ModifierEnum.ctrl:
                        offsetKeyPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                        break;
                    case (int)ModifierEnum.alt:
                        offsetKeyPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr);
                        break;
                }

                // Process keystroke.
                if (offsetKeyPressed)
                {
                    // Cancel if key input is already queued for processing.
                    if (_processed)
                    {
                        return;
                    }

                    // Set processed state.
                    _processed = true;

                    // Toggle zoning offset modifier.
                    shiftOffset = true;
                }
                else
                {
                    // Relevant keys aren't pressed anymore; this keystroke is over, so reset and continue.
                    _processed = false;

                    // Toggle zoning offset modifier.
                    shiftOffset = false;
                }
            }
        }
    }
}