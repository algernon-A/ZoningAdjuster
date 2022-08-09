// <copyright file="ZoningAdjusterButton.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ZoningAdjuster
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// The Zoning Adjuster panel button.
    /// Inspired by Node Contoller by kian.zarrin, which in turn was "A lot of copy-pasting from Crossings mod by Spectra and Roundabout Mod by Strad".
    /// </summary>
    public class ZoningAdjusterButton : UIButton
    {
        /// <summary>
        /// Button size.
        /// </summary>
        internal const float ButtonSize = 36f;

        /// <summary>
        /// Gets the button instance.
        /// </summary>
        public static ZoningAdjusterButton Instance { get; private set; }

        /// <summary>
        /// Sets a value indicating whether the tool is active.
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

            // Size and posiition - consistent with road tool size and position, and positioned immediately to the left of the road options tool panel by default.
            size = new Vector2(ButtonSize, ButtonSize);
            SetPosition();

            // Appearance and effects.
            atlas = UITextures.LoadQuadSpriteAtlas("ZoningAdjusterButton");
            focusedFgSprite = "focused";
            hoveredFgSprite = "hovered";
            disabledBgSprite = "disabled";
            tooltip = Translations.Translate("ZMD_NAME");
            playAudioEvents = true;

            // Normal sprite is focused if the tool is active, normal otherwise - tool may have been activated before button is initialized.
            normalFgSprite = ToolsModifierControl.toolController.CurrentTool == ZoningTool.Instance ? "focused" : "normal";

            // Event handler.
            eventClicked += (control, clickEvent) => ZoningTool.ToggleTool();

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
        internal static void CreateButton()
        {
            // Don't do anything if already created or not ingame, and only create if setting is enabled.
            if (Instance == null && Loading.IsLoaded && ModSettings.ShowPanelButton)
            {
                GameUIComponents.RoadsOptionPanel?.AddUIComponent<ZoningAdjusterButton>();
            }
        }

        /// <summary>
        /// Destroys the button.
        /// </summary>
        internal static void DestroyButton()
        {
            if (Instance != null)
            {
                Destroy(Instance);
                Instance = null;
            }
        }

        /// <summary>
        /// Sets the button position according to previous state and mod settings.
        /// </summary>
        internal void SetPosition()
        {
            // Restore previous position if we had one (ModSettings.buttonX isn't negative), otherwise position it in default position above panel button.
            Vector2 screenSize = GetUIView().GetScreenResolution();
            if (ModSettings.ButtonX < 0 || (ModSettings.ButtonX > screenSize.x - this.height || ModSettings.ButtonY > screenSize.y - this.width))
            {
                relativePosition = new Vector2((-ButtonSize - 1f) * 2f, 0);
            }
            else
            {
                absolutePosition = new Vector2(ModSettings.ButtonX, ModSettings.ButtonY);
            }
        }

        /// <summary>
        /// Check that icon is still visible when the screen resolution changes.
        /// Called by the game when the screen resolution changes.
        /// </summary>
        /// <param name="previousResolution">Previous screen resolution.</param>
        /// <param name="currentResolution">Current screen resolution.</param>
        protected override void OnResolutionChanged(Vector2 previousResolution, Vector2 currentResolution)
        {
            base.OnResolutionChanged(previousResolution, currentResolution);

            // Reset button position.
            SetPosition();
        }

        /// <summary>
        /// Position changed event handler, to save new position.
        /// </summary>
        /// <param name="control">Calling component (unused).</param>
        /// <param name="position">New position (unused).</param>
        private void PositionChanged(UIComponent control, Vector2 position)
        {
            Logging.Message("setting button position");
            ModSettings.ButtonX = this.absolutePosition.x;
            ModSettings.ButtonY = this.absolutePosition.y;
            ModSettings.Save();
        }
    }
}