using UnityEngine;
using ICities;


namespace ZoningAdjuster
{
    /// <summary>
    /// UI thread for offset key processing.
    /// </summary>
    public class OffsetKeyThreading : ThreadingExtensionBase
    {
        // Enum for offset modifier key.
        public enum ModifierEnum
        {
            shift = 0,
            ctrl,
            alt
        }


        // Flags.
        internal static bool operating = false; // Activated when required after successful level load.
        private bool processed = false;

        // Hotkey setting.
        internal static int offsetModifier;

        // State tracker.
        internal static bool shiftOffset = false;

        /// <summary>
        /// Look for offset modifier keypress.
        /// </summary>
        /// <param name="realTimeDelta">Real time elapsed since last update</param>
        /// <param name="simulationTimeDelta">Simulation time elapsed since last update</param>
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
                    if (processed)
                    {
                        return;
                    }

                    // Set processed state.
                    processed = true;

                    // Toggle zoning offset modifier.
                    shiftOffset = true;
                }
                else
                {
                    // Relevant keys aren't pressed anymore; this keystroke is over, so reset and continue.
                    processed = false;

                    // Toggle zoning offset modifier.
                    shiftOffset = false;
                }
            }
        }
    }


    /// <summary>
    /// UI thread for tool hotkey processing.
    /// </summary>
    public class ToolKeyThreading : ThreadingExtensionBase
    {
        // Hotkey settings.
        public static KeyCode hotKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), "Z");
        public static bool hotCtrl = false;
        public static bool hotAlt = true;
        public static bool hotShift = false;

        // Flags.
        internal static bool operating = false; // Activated when required after successful level load.
        private bool processed = false;


        /// <summary>
        /// Look for tool hotkey keypress.
        /// </summary>
        /// <param name="realTimeDelta">Real time elapsed since last update</param>
        /// <param name="simulationTimeDelta">Simulation time elapsed since last update</param>
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            // Don't do anything if not active.
            if (operating)
            {
                // Has hotkey been pressed?
                if (hotKey != KeyCode.None && Input.GetKey(hotKey))
                {
                    // Check modifier keys according to settings.
                    bool altPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr);
                    bool ctrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                    bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

                    // Modifiers have to *exactly match* settings, e.g. "alt-Z" should not trigger on "ctrl-alt-Z".
                    bool altOkay = altPressed == hotAlt;
                    bool ctrlOkay = ctrlPressed == hotCtrl;
                    bool shiftOkay = shiftPressed == hotShift;

                    // Process keystroke.
                    if (altOkay && ctrlOkay && shiftOkay)
                    {
                        // Only process if we're not already doing so.
                        if (!processed)
                        {
                            // Set processed flag.
                            processed = true;

                            // Toggle tool state.
                            ZoningTool.ToggleTool();
                        }
                    }
                    else
                    {
                        // Relevant keys aren't pressed anymore; this keystroke is over, so reset and continue.
                        processed = false;
                    }
                }
                else
                {

                    // Relevant keys aren't pressed anymore; this keystroke is over, so reset and continue.
                    processed = false;
                }
            }
        }
    }
}