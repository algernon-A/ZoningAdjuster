using System;
using UnityEngine;
using ColossalFramework.UI;


namespace ZoningAdjuster
{
    /// <summary>
    /// The zoning settings panel.
    /// </summary>
    public class ZoningSettingsPanel : UIPanel
    {
        // Layout constants.
        const float Margin = 8f;
        const float PanelWidth = 230f;
        const float TitleHeight = 40f;
        const float DisableCheckY = TitleHeight;
        const float OldCheckY = DisableCheckY + 30f;
        const float NewCheckY = OldCheckY + 20f;
        const float NoCheckY = NewCheckY + 20f;
        const float SetbackSliderY = NoCheckY + 25f;
        const float SliderLabelHeight = 20f;
        const float SliderPanelHeight = 36f;
        const float PanelHeight = SetbackSliderY + SliderLabelHeight + SliderPanelHeight + Margin;

        // Zoning age priority checkboxes.
        private readonly string[] priorityNames =
        {
            "ZMD_PNL_POZ",
            "ZMD_PNL_PNZ",
            "ZMD_PNL_PVZ"
        };
        private readonly float[] priorityCheckY =
        {
            OldCheckY,
            NewCheckY,
            NoCheckY
        };

        // Status flag.
        public static bool disableZoning = false;


        // Panel components.
        private readonly UICheckBox[] priorityChecks;
        private readonly UICheckBox disableCheck;

        // Instance references.
        private static GameObject uiGameObject;
        private static ZoningSettingsPanel panel;
        internal static ZoningSettingsPanel Panel => panel;


        /// <summary>
        /// Creates the panel object in-game and displays it.
        /// </summary>
        internal static void Create()
        {
            try
            {
                Logging.Message("creating ZoningSettingsPanel");

                // If no GameObject instance already set, create one.
                if (uiGameObject == null)
                {
                    // Give it a unique name for easy finding with ModTools.
                    uiGameObject = new GameObject("ZoningSettingsPanel");
                    uiGameObject.transform.parent = UIView.GetAView().transform;

                    // Create new panel instance and add it to GameObject.
                    panel = uiGameObject.AddComponent<ZoningSettingsPanel>();
                    panel.transform.parent = uiGameObject.transform.parent;
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
            if (panel == null )
            {
                return;
            }

            // Destroy game objects.
            GameObject.Destroy(panel);
            GameObject.Destroy(uiGameObject);

            // Let the garbage collector do its work (and also let us know that we've closed the object).
            panel = null;
            uiGameObject = null;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public ZoningSettingsPanel()
        {
            // Size and position.
            autoSize = false;
            size = new Vector2(PanelWidth, PanelHeight);
            SetPosition();

            // Appearance.
            atlas = TextureUtils.InGameAtlas;
            backgroundSprite = "SubcategoriesPanel";
            clipChildren = true;
             
            // Title.
            UILabel titleLabel = UIControls.AddLabel(this, 0f, Margin, Translations.Translate("ZMD_NAME"));
            titleLabel.textAlignment = UIHorizontalAlignment.Center;
            titleLabel.width = PanelWidth;

            // Icon sprite.
            UISprite titleSprite = this.AddUIComponent<UISprite>();
            titleSprite.size = new Vector2(24f, 24f);
            titleSprite.relativePosition = new Vector2(4f, 5f);
            titleSprite.atlas = Textures.ToolButtonSprites;
            titleSprite.spriteName = "normal";

            // Drag bar.
            UIDragHandle dragHandle = AddUIComponent<UIDragHandle>();
            dragHandle.width = this.width;
            dragHandle.height = this.height;
            dragHandle.relativePosition = Vector3.zero;
            dragHandle.target = this;

            // Disable zoning checkbox.
            disableCheck = UIControls.LabelledCheckBox(this, Margin, DisableCheckY, Translations.Translate("ZMD_PNL_DIS"), tooltip: Translations.Translate("ZMD_PNL_DIS_TIP"));
            disableCheck.isChecked = disableZoning;
            disableCheck.eventCheckChanged += (control, isChecked) => disableZoning = isChecked;

            // Priority checkboxes.
            priorityChecks = new UICheckBox[(int)PriorityIndexes.NumPriorities];
            int currentPriority = ZoneBlockData.Instance.GetCurrentPriority();
            for (int i = 0; i < priorityChecks.Length; ++i)
            {
                priorityChecks[i] = UIControls.LabelledCheckBox(this, Margin, priorityCheckY[i], Translations.Translate(priorityNames[i]), tooltip: Translations.Translate(priorityNames[i] + "_TIP"));
                priorityChecks[i].objectUserData = i;
                priorityChecks[i].isChecked = i == currentPriority;
                priorityChecks[i].eventCheckChanged += PriorityCheckChanged;
            }

            // Setback slider control - same appearance as Fine Road Tool's, for consistency.
            UISlider setbackSlider = AddSlider("ZMD_PNL_SBK", SetbackSliderY, "ZMD_PNL_SBK_TIP");
            setbackSlider.eventValueChanged += (control, value) =>
            {
                ZoneBlockPatch.setback = value;
            };

            // Bring to front.
            BringToFront();

            // Save new position when moved.
            eventPositionChanged += PositionChanged;
        }


        /// <summary>
        /// Sets the panel position according to previous state and mod settings.
        /// </summary>
        internal void SetPosition()
        {
            // Restore previous position if we had one ((ModSettings.panelX isn't negative), otherwise position it in default position above panel button.
            if (ModSettings.panelX < 0)
            {
                absolutePosition = new Vector2(ZoningAdjusterButton.Instance.absolutePosition.x, ZoningAdjusterButton.Instance.absolutePosition.y - PanelHeight - Margin);
            }
            else
            {
                absolutePosition = new Vector2(ModSettings.panelX, ModSettings.panelY);
            }
        }


        /// <summary>
        /// Position changed event handler, to save new position
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="position">New position (unused)</param>
        private void PositionChanged(UIComponent control, Vector2 position)
        {
            ModSettings.panelX = this.absolutePosition.x;
            ModSettings.panelY = this.absolutePosition.y;
            ZoningModSettingsFile.SaveSettings();
        }


        /// <summary>
        /// Priority check changed event handler.
        /// </summary>
        /// <param name="control">Calling component</param>
        /// <param name="isChecked">New checked state</param>
        private void PriorityCheckChanged(UIComponent control, bool isChecked)
        {
            // Get stored index from control user data.
            if (control.objectUserData is int index)
            {
                // Deselect other checkboxes if this one has been checked.
                if (isChecked)
                {
                    // Iterate through all checkboxes.
                    for (int i = 0; i < (int)PriorityIndexes.NumPriorities; ++i)
                    {
                        // If it's not this checkbox, uncheck it.
                        if (i != index)
                        {
                            priorityChecks[i].isChecked = false;
                        }
                    }

                    // Set current priority.
                    ZoneBlockData.Instance.SetCurrentPriority(index);
                }
                else
                {
                    // Checkbox has been deselected; make sure that at least one other is still selected, otherwise we re-select this one.
                    // Iterate through all checkboxes.
                    for (int i = 0; i < (int)PriorityIndexes.NumPriorities; ++i)
                    {
                        // If any are checked, we're done here; return.
                        if (priorityChecks[i].isChecked)
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
        /// <param name="textKey">Label translation key</param>
        /// <param name="yPos">Relative Y position</param>
        /// <param name="toolTipKey">Tooltip translation key</param>
        /// <returns>New UISlider with Fine Road Tool appearance</returns>
        private UISlider AddSlider(string textKey, float yPos, string toolTipKey)
        {
            // Slider label.
            UILabel setbackLabel = this.AddUIComponent<UILabel>();
            setbackLabel.textScale = 0.9f;
            setbackLabel.text = Translations.Translate(textKey);
            setbackLabel.relativePosition = new Vector2(Margin, yPos);
            setbackLabel.SendToBack();

            // Slider panel.
            UIPanel sliderPanel = this.AddUIComponent<UIPanel>();
            sliderPanel.atlas = TextureUtils.InGameAtlas;
            sliderPanel.backgroundSprite = "GenericPanel";
            sliderPanel.color = new Color32(206, 206, 206, 255);
            sliderPanel.size = new Vector2(this.width - (Margin * 2), SliderPanelHeight);
            sliderPanel.relativePosition = new Vector2(Margin, yPos + SliderLabelHeight);
            sliderPanel.tooltip = Translations.Translate(toolTipKey);

            // Slider value label.
            UILabel valueLabel = sliderPanel.AddUIComponent<UILabel>();
            valueLabel.atlas = TextureUtils.InGameAtlas;
            valueLabel.backgroundSprite = "TextFieldPanel";
            valueLabel.verticalAlignment = UIVerticalAlignment.Bottom;
            valueLabel.textAlignment = UIHorizontalAlignment.Center;
            valueLabel.textScale = 0.7f;
            valueLabel.text = ZoneBlockPatch.setback.ToString() + "m";
            valueLabel.autoSize = false;
            valueLabel.color = new Color32(91, 97, 106, 255);
            valueLabel.size = new Vector2(38, 15);
            valueLabel.relativePosition = new Vector2(sliderPanel.width - valueLabel.width - Margin, 10f);

            // Slider control - same appearance as Fine Road Tool's, for consistency.
            UISlider newSlider = sliderPanel.AddUIComponent<UISlider>();
            newSlider.name = "ZoningSetbackSlider";
            newSlider.size = new Vector2(sliderPanel.width - valueLabel.width - (Margin * 3), 18f);
            newSlider.relativePosition = new Vector2(Margin, Margin);

            // Setback slider track.
            UISlicedSprite sliderSprite = newSlider.AddUIComponent<UISlicedSprite>();
            sliderSprite.atlas = TextureUtils.InGameAtlas;
            sliderSprite.spriteName = "BudgetSlider";
            sliderSprite.size = new Vector2(newSlider.width, 9f);
            sliderSprite.relativePosition = new Vector2(0f, 4f);

            // Setback slider thumb.
            UISlicedSprite sliderThumb = newSlider.AddUIComponent<UISlicedSprite>();
            sliderThumb.atlas = TextureUtils.InGameAtlas;
            sliderThumb.spriteName = "SliderBudget";
            newSlider.thumbObject = sliderThumb;

            // Setback slider values.
            newSlider.stepSize = 0.5f;
            newSlider.minValue = -4f;
            newSlider.maxValue = 8f;
            newSlider.value = ZoneBlockPatch.setback;

            // Event handler to update value.
            newSlider.eventValueChanged += (control, value) =>
            {
                valueLabel.text = value.ToString() + "m";
            };

            return newSlider;
        }
    }
}
