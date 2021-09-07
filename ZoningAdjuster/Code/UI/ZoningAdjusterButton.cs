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
        internal static ZoningAdjusterButton Instance { get; private set; }

        
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
            Logging.Message("ZoningAdjusterButton start");

            base.Start();

            // Instance.
            name = "ZoningAdjusterButton";
            Instance = this;

            // Size and posiition - consistent with road tool size and position, and positioned immediately to the left of the road options tool panel by default.
            size = new Vector2(ButtonSize, ButtonSize);
            SetPosition();

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

                    // Create zoning settings panel if it isn't already created, and in any case make sure it's visible.
                    ZoningSettingsPanel.Create();
                }
                else
                {
                    // Revert to previously selected tool.
                    ToolsModifierControl.toolController.CurrentTool = previousTool;

                    // Hide panel if necessary.
                    if (!ModSettings.showOnRoad)
                    {
                        ZoningSettingsPanel.Close();
                    }
                }
            };

            // Set initial visibility state and add event hook.
            VisibilityChanged(null, isVisible);
            eventVisibilityChanged += VisibilityChanged;

            // Add drag handle.
            UIDragHandle dragHandle = this.AddUIComponent<UIDragHandle>();
            dragHandle.target = this;
            dragHandle.relativePosition = Vector3.zero;
            dragHandle.size = this.size;

            // Save new position when moved.
            eventPositionChanged += PositionChanged;
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
        /// Sets the button position according to previous state and mod settings.
        /// </summary>
        internal void SetPosition()
        {
            // Restore previous position if we had one (ModSettings.buttonX isn't negative), otherwise position it in default position above panel button.
            if (ModSettings.buttonX < 0)
            {
                relativePosition = new Vector2((-ButtonSize - 1f) * 2f, 0);
            }
            else
            {
                absolutePosition = new Vector2(ModSettings.buttonX, ModSettings.buttonY);
            }
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


        /// <summary>
        /// Visibility changed event handler, to toggle visibility of panel.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="isVisible">New visibility state</param>
        private static void VisibilityChanged(UIComponent control, bool isVisible)
        {
            Logging.Message("panel button visibility changed");
            // Toggle zoning settings panel state accordingly.
            if (isVisible)
            {
                // Only show if appropriate setting is enabled.
                if (ModSettings.showOnRoad)
                {
                    ZoningSettingsPanel.Create();
                }
            }
            else
            {
                ZoningSettingsPanel.Close();
            }
        }


        /// <summary>
        /// Position changed event handler, to save new position
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="position">New position (unused)</param>
        private void PositionChanged(UIComponent control, Vector2 position)
        {
            Logging.Message("setting button position");
            ModSettings.buttonX = this.absolutePosition.x;
            ModSettings.buttonY = this.absolutePosition.y;
            ZoningModSettingsFile.SaveSettings();
        }
    }
}