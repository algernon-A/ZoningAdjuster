// <copyright file="ZoningSettingsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ZoningAdjuster
{
    using System;
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// The zoning settings panel.
    /// </summary>
    public class ZoningSettingsPanel : UIPanel
    {
        // Layout constants.
        private const float Margin = 8f;
        private const float CheckHeight = 20f;
        private const float PanelWidth = 230f;
        private const float TitleHeight = 40f;
        private const float SliderLabelHeight = 20f;
        private const float SliderPanelHeight = 36f;
        private const float DisableCheckY = TitleHeight;
        private const float ForceCheckY = DisableCheckY + CheckHeight;
        private const float OldCheckY = ForceCheckY + 30f;
        private const float NewCheckY = OldCheckY + CheckHeight;
        private const float NoCheckY = NewCheckY + CheckHeight;
        private const float SetbackSliderY = NoCheckY + 30f;
        private const float ZoneDepthSliderY = SetbackSliderY + SliderLabelHeight + SliderPanelHeight + (Margin * 2f);
        private const float PanelHeight = ZoneDepthSliderY + SliderLabelHeight + SliderPanelHeight + Margin;

        // Instance references.
        private static GameObject s_gameObject;
        private static ZoningSettingsPanel s_panel;

        // Zoning age priority checkboxes.
        private readonly string[] _priorityNames =
        {
            "ZMD_PNL_POZ",
            "ZMD_PNL_PNZ",
            "ZMD_PNL_PVZ",
        };

        private readonly float[] _priorityCheckY =
        {
            OldCheckY,
            NewCheckY,
            NoCheckY,
        };

        // Panel components.
        private readonly UICheckBox[] _priorityChecks;
        private readonly UICheckBox _disableCheck;
        private readonly UICheckBox _forceCheck;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZoningSettingsPanel"/> class.
        /// </summary>
        public ZoningSettingsPanel()
        {
            // Size and position.
            autoSize = false;
            size = new Vector2(PanelWidth, PanelHeight);
            SetPosition();

            // Appearance.
            atlas = UITextures.InGameAtlas;
            backgroundSprite = "SubcategoriesPanel";
            clipChildren = true;

            // Title.
            UILabel titleLabel = UILabels.AddLabel(this, 0f, Margin, Translations.Translate("ZMD_NAME"), PanelWidth - Margin - Margin);
            titleLabel.textAlignment = UIHorizontalAlignment.Center;
            titleLabel.width = PanelWidth;

            // Icon sprite.
            UISprite titleSprite = this.AddUIComponent<UISprite>();
            titleSprite.size = new Vector2(24f, 24f);
            titleSprite.relativePosition = new Vector2(4f, 5f);
            titleSprite.atlas = UITextures.LoadQuadSpriteAtlas("ZoningAdjusterButton");
            titleSprite.spriteName = "normal";

            // Drag bar (sizing takes place after priority checkboxes to allow for any width adjustments for longer translation strings).
            UIDragHandle dragHandle = AddUIComponent<UIDragHandle>();
            dragHandle.width = this.width;
            dragHandle.height = this.height;
            dragHandle.relativePosition = Vector3.zero;
            dragHandle.target = this;

            // Disable zoning checkbox.
            _disableCheck = UICheckBoxes.AddLabelledCheckBox(this, Margin, DisableCheckY, Translations.Translate("ZMD_PNL_DIS"), tooltip: Translations.Translate("ZMD_PNL_DIS_TIP"));
            _disableCheck.isChecked = ModSettings.DisableZoning;
            _disableCheck.eventCheckChanged += (control, isChecked) =>
            {
                ModSettings.DisableZoning = isChecked;

                // Disable forced zoning check if this is selected
                if (isChecked)
                {
                    _forceCheck.isChecked = false;
                }
            };

            // Force zoning checkbox.
            _forceCheck = UICheckBoxes.AddLabelledCheckBox(this, Margin, ForceCheckY, Translations.Translate("ZMD_PNL_FOR"), tooltip: Translations.Translate("ZMD_PNL_DIS_TIP"));
            _forceCheck.isChecked = ModSettings.ForceZoning;
            _forceCheck.eventCheckChanged += (control, isChecked) =>
            {
                ModSettings.ForceZoning = isChecked;

                // Disable disable zoning check if this is selected
                if (isChecked)
                {
                    _disableCheck.isChecked = false;
                }
            };

            // Accomodate longer translation strings.
            this.width = Mathf.Max(this.width, _forceCheck.width + (Margin * 2f), _disableCheck.width + (Margin * 2f));

            // Priority checkboxes.
            _priorityChecks = new UICheckBox[(int)SegmentData.PriorityIndexes.NumPriorities];
            int currentPriority = SegmentData.Instance.GetCurrentPriority();
            for (int i = 0; i < _priorityChecks.Length; ++i)
            {
                _priorityChecks[i] = UICheckBoxes.AddLabelledCheckBox(this, Margin, _priorityCheckY[i], Translations.Translate(_priorityNames[i]), tooltip: Translations.Translate(_priorityNames[i] + "_TIP"));
                _priorityChecks[i].objectUserData = i;
                _priorityChecks[i].isChecked = i == currentPriority;
                _priorityChecks[i].eventCheckChanged += PriorityCheckChanged;

                // Accomodate longer translation strings.
                this.width = Mathf.Max(this.width, _priorityChecks[i].width + (Margin * 2f));
            }

            // Drag bar sizing (after priority checkboxes to allow for any width adjustments for longer translation strings).
            dragHandle.width = this.width;
            dragHandle.height = this.height;

            // Setback slider control - same appearance as Fine Road Tool's, for consistency.
            UISlider setbackSlider = AddSlider("ZMD_PNL_SBK", SetbackSliderY, "ZMD_PNL_SBK_TIP", false);
            setbackSlider.eventValueChanged += (control, value) =>
            {
                CreateZoneBlocks.Setback = value;
            };

            // Zoning depth slider control - same appearance as Fine Road Tool's, for consistency.
            UISlider zoneDepthSlider = AddSlider("ZMD_PNL_DEP", ZoneDepthSliderY, "ZMD_PNL_DEP_TIP", true);
            zoneDepthSlider.eventValueChanged += (control, value) =>
            {
                ZoneDepthPatches.ZoneDepth = (byte)Mathf.Clamp(value - 1, 0f, 3f);
            };

            // Bring to front.
            BringToFront();

            // Save new position when moved.
            eventPositionChanged += PositionChanged;
        }

        /// <summary>
        /// Gets the active panel instance.
        /// </summary>
        internal static ZoningSettingsPanel Panel => s_panel;

        /// <summary>
        /// Creates the panel object in-game and displays it.
        /// </summary>
        internal static void Create()
        {
            try
            {
                Logging.Message("creating ZoningSettingsPanel");

                // If no GameObject instance already set, create one.
                if (s_gameObject == null)
                {
                    // Give it a unique name for easy finding with ModTools.
                    s_gameObject = new GameObject("ZoningSettingsPanel");
                    s_gameObject.transform.parent = UIView.GetAView().transform;

                    // Create new panel instance and add it to GameObject.
                    s_panel = s_gameObject.AddComponent<ZoningSettingsPanel>();
                    s_panel.transform.parent = s_gameObject.transform.parent;
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception creating ZoningSettingsPanel");
            }
        }

        /// <summary>
        /// Closes the panel by destroying the object (removing any ongoing UI overhead).
        /// </summary>
        internal static void Close()
        {
            // Don't do anything if no panel.
            if (s_panel == null)
            {
                return;
            }

            // Destroy game objects.
            Destroy(s_panel);
            Destroy(s_gameObject);

            // Let the garbage collector do its work (and also let us know that we've closed the object).
            s_panel = null;
            s_gameObject = null;
        }

        /// <summary>
        /// Sets the panel position according to previous state and mod settings.
        /// </summary>
        internal void SetPosition()
        {
            // Restore previous position if we had one ((ModSettings.panelX isn't negative), otherwise position it in default position above panel button.
            if (ModSettings.PanelX < 0)
            {
                // Default position is above the game's RoadOptionPanel, level with the zoning button's place (regardless of whether or not zoning button is shown).
                absolutePosition = GameUIComponents.RoadsOptionPanel.absolutePosition - new Vector3((-ZoningAdjusterButton.ButtonSize - 1f) * 2f, PanelHeight + Margin);
            }
            else
            {
                absolutePosition = new Vector2(ModSettings.PanelX, ModSettings.PanelY);
            }
        }

        /// <summary>
        /// Position changed event handler, to save new position.
        /// </summary>
        /// <param name="control">Calling component (unused).</param>
        /// <param name="position">New position (unused).</param>
        private void PositionChanged(UIComponent control, Vector2 position)
        {
            ModSettings.PanelX = this.absolutePosition.x;
            ModSettings.PanelY = this.absolutePosition.y;
            ModSettings.Save();
        }

        /// <summary>
        /// Priority check changed event handler.
        /// </summary>
        /// <param name="control">Calling component.</param>
        /// <param name="isChecked">New checked state.</param>
        private void PriorityCheckChanged(UIComponent control, bool isChecked)
        {
            // Get stored index from control user data.
            if (control.objectUserData is int index)
            {
                // Deselect other checkboxes if this one has been checked.
                if (isChecked)
                {
                    // Iterate through all checkboxes.
                    for (int i = 0; i < (int)SegmentData.PriorityIndexes.NumPriorities; ++i)
                    {
                        // If it's not this checkbox, uncheck it.
                        if (i != index)
                        {
                            _priorityChecks[i].isChecked = false;
                        }
                    }

                    // Set current priority.
                    SegmentData.Instance.SetCurrentPriority(index);
                }
                else
                {
                    // Checkbox has been deselected; make sure that at least one other is still selected, otherwise we re-select this one.
                    // Iterate through all checkboxes.
                    for (int i = 0; i < (int)SegmentData.PriorityIndexes.NumPriorities; ++i)
                    {
                        // If any are checked, we're done here; return.
                        if (_priorityChecks[i].isChecked)
                        {
                            return;
                        }
                    }

                    // If we got here, no other check is selected; re-select this one.
                    (control as UICheckBox).isChecked = true;
                }
            }
        }

        /// <summary>
        /// Adds a labelled slider control to the panel at the specified postion.
        /// </summary>
        /// <param name="textKey">Label translation key.</param>
        /// <param name="yPos">Relative Y position.</param>
        /// <param name="toolTipKey">Tooltip translation key.</param>
        /// <param name="isDepthSlider">True if this is a zone depth slider, false otherwise.</param>
        /// <returns>New UISlider with Fine Road Tool appearance.</returns>
        private UISlider AddSlider(string textKey, float yPos, string toolTipKey, bool isDepthSlider)
        {
            // Slider label.
            UILabel setbackLabel = UILabels.AddLabel(this, Margin, yPos, Translations.Translate(textKey), textScale: 0.9f);
            setbackLabel.SendToBack();

            // Slider panel.
            UIPanel sliderPanel = this.AddUIComponent<UIPanel>();
            sliderPanel.atlas = UITextures.InGameAtlas;
            sliderPanel.backgroundSprite = "GenericPanel";
            sliderPanel.color = new Color32(206, 206, 206, 255);
            sliderPanel.size = new Vector2(this.width - (Margin * 2), SliderPanelHeight);
            sliderPanel.relativePosition = new Vector2(Margin, yPos + SliderLabelHeight);
            sliderPanel.tooltip = Translations.Translate(toolTipKey);

            // Slider value label.
            UILabel valueLabel = UILabels.AddLabel(sliderPanel, sliderPanel.width - Margin, 10f, "0", textScale: 0.9f);
            valueLabel.atlas = UITextures.InGameAtlas;
            valueLabel.backgroundSprite = "TextFieldPanel";
            valueLabel.verticalAlignment = UIVerticalAlignment.Bottom;
            valueLabel.textAlignment = UIHorizontalAlignment.Center;
            valueLabel.autoSize = false;
            valueLabel.color = new Color32(91, 97, 106, 255);
            valueLabel.size = new Vector2(38, 15);
            valueLabel.relativePosition = new Vector2(sliderPanel.width - valueLabel.width - Margin, 10f);

            // Slider control - same appearance as Fine Road Tool's, for consistency.
            UISlider newSlider = UISliders.AddBudgetSlider(sliderPanel, Margin, 9f, sliderPanel.width - valueLabel.width - (Margin * 3), 4f);
            newSlider.name = "ZoningSetbackSlider";

            // Is this a zone depth slider?
            if (isDepthSlider)
            {
                // Event handler to update value.
                newSlider.eventValueChanged += (c, value) =>
                {
                    valueLabel.text = ((int)value).ToString();
                };

                // Depth slider - set values.
                newSlider.stepSize = 1;
                newSlider.minValue = 1;
                newSlider.maxValue = 4;

                // +1 to account for zero-basing.
                int initialValue = ZoneDepthPatches.ZoneDepth + 1;
                newSlider.value = initialValue;
                valueLabel.text = initialValue.ToString();
            }
            else
            {
                // Event handler to update value.
                newSlider.eventValueChanged += (c, value) =>
                {
                    valueLabel.text = value.ToString() + "m";
                };

                // Setback slider - set values.
                newSlider.stepSize = 0.5f;
                newSlider.minValue = -4f;
                newSlider.maxValue = 8f;
                newSlider.value = CreateZoneBlocks.Setback;
            }

            return newSlider;
        }
    }
}
