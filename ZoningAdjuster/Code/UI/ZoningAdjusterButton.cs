using ColossalFramework.UI;
using UnityEngine;

/* Inspired by Node Contoller by kian.zarrin, which in turn was "A lot of copy-pasting from Crossings mod by Spectra and Roundabout Mod by Strad." */

namespace ZoningAdjuster
{
    public class ZoningAdjusterButton : UIButton
    {
        // Button size.
        private const float ButtonSize = 36f;

        // Previous tool.
        private ToolBase previousTool;


        // Instance reference.
        public static ZoningAdjusterButton Instance { get; private set; }

        
        /// <summary>
        /// Sets the button state to indicate when the tool is active.
        /// </summary>
        public static bool ToolActive
        {
            set
            {
                // Null check - tool may be created before button is initialised.
                if (Instance != null)
                {
                    if (value)
                    {
                        Instance.normalFgSprite = "hovered";
                    }
                    else
                    {
                        Instance.normalFgSprite = "normal";
                    }
                }
            }
        }


        /// <summary>
        /// Called by Unity before the first frame is rendered.
        /// Used here to perform initial setup when appropriate.
        /// </summary>
        public override void Start()
        {
            base.Start();

            // Instance.
            name = "ZoningAdjusterButton";
            Instance = this;

            // Size and posiition - consistent with road tool size and position, and positioned immediately to the left of the road options tool panel.
            relativePosition = new Vector2(-ButtonSize - 1f, 0);
            size = new Vector2(ButtonSize, ButtonSize);

            // Appearance and effects.
            atlas = Textures.ToolButtonSprites;
            focusedFgSprite = "focused";
            hoveredFgSprite = "hovered";
            disabledBgSprite = "disabled";
            tooltip = Translations.Translate("ZMD_NAME");
            playAudioEvents = true;

            // Normal sprite is focused if the tool is active, normal otherwise - tool may have been activated before button is initialized.
            normalFgSprite = ToolsModifierControl.toolController.CurrentTool == ZoningTool.Instance ? "focused" : "normal";

            // Event handler.
            eventClicked += (control, clickEvent) =>
            {
                // Activate zoning tool if it isn't already; if already active, deactivate it by selecting the previously active tool instead.
                if (ToolsModifierControl.toolController.CurrentTool != ZoningTool.Instance)
                {
                    // Record previous tool.
                    previousTool = ToolsModifierControl.toolController.CurrentTool;
                    ToolsModifierControl.toolController.CurrentTool = ZoningTool.Instance;

                    // Create zoning settings panel if it isn't already created.
                    ZoningSettingsPanel.Create();
                }
                else
                {
                    ToolsModifierControl.toolController.CurrentTool = previousTool;
                }
            };

            // Set initial visibility state and add event hook.
            VisibilityChanged(null, isVisible);
            eventVisibilityChanged += VisibilityChanged;
        }


        /// <summary>
        /// Visibility changed event handler, to toggle visibility of panel.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="isVisible">New visibility state</param>
        public static void VisibilityChanged(UIComponent control, bool isVisible)
        {
            Logging.Message("panel button visibility changed");
            // Toggle zoning settings panel state accordingly.
            if (isVisible)
            {
                ZoningSettingsPanel.Create();
            }
            else
            {
                ZoningSettingsPanel.Close();
            }
        }


        /// <summary>
        /// Creates the button and attaches it to the roads option panel.
        /// </summary>
        /// <returns>Button instance</returns>
        internal static ZoningAdjusterButton CreateButton()
        {
            return RoadsOptionPanel().AddUIComponent<ZoningAdjusterButton>();
        }


        /// <summary>
        /// Finds the game's RoadOptionPanel.
        /// </summary>
        /// <returns>RoadOptionPanel as UIComponent</returns>
        private static UIComponent RoadsOptionPanel()
        {
            foreach (UIComponent component in UnityEngine.Object.FindObjectsOfType<UIComponent>())
            {
                if (component.name.Equals("RoadsOptionPanel"))
                {
                    Logging.Message("found RoadsOptionPanel");
                    return component;
                }
            }

            return null;
        }
    }
}