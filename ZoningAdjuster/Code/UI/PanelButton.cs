using ColossalFramework.UI;
using System;
using UnityEngine;

/* Inspired by Node Contoller by kian.zarrin, which in turn was "A lot of copy-pasting from Crossings mod by Spectra and Roundabout Mod by Strad." */

namespace ZoningAdjuster
{
    public class ZoningAdjusterButton : UIButton
    {
        // Button size.
        private const float ButtonSize = 36f;


        // Flag.
        internal bool panelClick = false;

        // Components.
        private ToolControlPanel zoningOptionsPanel;



        // Instance reference.
        public static ZoningAdjusterButton Instance { get; private set; }

        
        /// <summary>
        /// Sets the button state to indicate when the tool is active.
        /// </summary>
        public bool ToolActive
        {
            set
            {
                if (value)
                {
                    normalFgSprite = "hovered";
                }
                else
                {
                    normalFgSprite = "normal";
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
            normalFgSprite = "normal";
            focusedFgSprite = "focused";
            hoveredFgSprite = "hovered";
            disabledBgSprite = "disabled";
            tooltip = Translations.Translate("ZMD_NAME");
            playAudioEvents = true;

            // Create panel - attach directly to button, so visibility and position are consistent.
            zoningOptionsPanel = Instance.AddUIComponent(typeof(ToolControlPanel)) as ToolControlPanel;
            zoningOptionsPanel.Setup();

            // Event handler.
            eventClicked += (control, clickEvent) =>
            {
                if (panelClick)
                {
                    panelClick = false;
                }
                else
                {
                    zoningOptionsPanel.isVisible = !zoningOptionsPanel.isVisible;
                    if (zoningOptionsPanel.isVisible)
                    {
                        zoningOptionsPanel.BringToFront();

                        ToolsModifierControl.toolController.CurrentTool = ZoningTool.Instance;
                    }
                    else
                    {

                        // Deactivate zoning tool by activating the default tool instead.
                        ToolsModifierControl.SetTool<DefaultTool>();
                    }
                }
            };
        }


        /// <summary>
        /// Creates the button and attaches it to the roads option panel.
        /// </summary>
        /// <returns></returns>
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