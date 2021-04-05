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
        const float OldCheckY = TitleHeight;
        const float NewCheckY = OldCheckY + 20f;
        const float SliderLabelY = NewCheckY + 25f;
        const float SliderLabelHeight = 20f;
        const float SliderPanelY = SliderLabelY + SliderLabelHeight;
        const float SliderPanelHeight = 36f;
        const float PanelHeight = SliderPanelY + SliderPanelHeight + Margin;

        // Panel components.
        private readonly UILabel setbackDepthLabel;

        // Last position.
        private static float lastX = -1, lastY;

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

            // Store current position.
            lastX = Panel.absolutePosition.x;
            lastY = Panel.absolutePosition.y;

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
            autoSize = false;
            size = new Vector2(PanelWidth, PanelHeight);

            // Restore previous position if we had one (lastX isn't negative), otherwise position it in default position above panel button.
            if (lastX < 0)
            {
                absolutePosition = new Vector2(ZoningAdjusterButton.Instance.absolutePosition.x, ZoningAdjusterButton.Instance.absolutePosition.y - PanelHeight - Margin);
            }
            else
            {
                absolutePosition = new Vector2(lastX, lastY);
            }

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

            // Controls.
            UICheckBox oldAgeCheck = UIControls.LabelledCheckBox(this, Margin, OldCheckY, Translations.Translate("ZMD_PNL_POZ"), tooltip: Translations.Translate("ZMD_PNL_POZ_TIP"));
            oldAgeCheck.isChecked = ZoneBlockData.preserveOldZones;

            UICheckBox newAgeCheck = UIControls.LabelledCheckBox(this, Margin, NewCheckY, Translations.Translate("ZMD_PNL_PNZ"), tooltip: Translations.Translate("ZMD_PNL_PNZ_TIP"));
            newAgeCheck.isChecked = ZoneBlockData.preserveNewZones;

            // Checkbox event handlers.
            oldAgeCheck.eventCheckChanged += (control, isChecked) =>
            {
                ZoneBlockData.preserveOldZones = isChecked;
                if (isChecked && newAgeCheck.isChecked)
                {
                    newAgeCheck.isChecked = false;
                }
            };

            newAgeCheck.eventCheckChanged += (control, isChecked) =>
            {
                ZoneBlockData.preserveNewZones = isChecked;
                if (isChecked && oldAgeCheck.isChecked)
                {
                    oldAgeCheck.isChecked = false;
                }
            };

            // Setback slider - same appearance as Fine Road Tool's, for consistency.
            // Setback slider label.
            UILabel setbackLabel = this.AddUIComponent<UILabel>();
            setbackLabel.textScale = 0.9f;
            setbackLabel.text = Translations.Translate("ZMD_PNL_SBK");
            setbackLabel.relativePosition = new Vector2(Margin, SliderLabelY);
            setbackLabel.SendToBack();

            // Setback slider panel.
            UIPanel sliderPanel = this.AddUIComponent<UIPanel>();
            sliderPanel.atlas = TextureUtils.InGameAtlas;
            sliderPanel.backgroundSprite = "GenericPanel";
            sliderPanel.color = new Color32(206, 206, 206, 255);
            sliderPanel.size = new Vector2(this.width - (Margin * 2), SliderPanelHeight);
            sliderPanel.relativePosition = new Vector2(Margin, SliderPanelY);
            sliderPanel.tooltip = Translations.Translate("ZMD_PNL_SBK_TIP");

            // Setback slider value label.
            setbackDepthLabel = sliderPanel.AddUIComponent<UILabel>();
            setbackDepthLabel.atlas = TextureUtils.InGameAtlas;
            setbackDepthLabel.backgroundSprite = "TextFieldPanel";
            setbackDepthLabel.verticalAlignment = UIVerticalAlignment.Bottom;
            setbackDepthLabel.textAlignment = UIHorizontalAlignment.Center;
            setbackDepthLabel.textScale = 0.7f;
            setbackDepthLabel.text = ZoneBlockPatch.setback.ToString() + "m";
            setbackDepthLabel.autoSize = false;
            setbackDepthLabel.color = new Color32(91, 97, 106, 255);
            setbackDepthLabel.size = new Vector2(38, 15);
            setbackDepthLabel.relativePosition = new Vector2(sliderPanel.width - setbackDepthLabel.width - Margin, 10f);

            // Setback slider control - same appearance as Fine Road Tool's, for consistency.
            UISlider setbackSlider = sliderPanel.AddUIComponent<UISlider>();
            setbackSlider.name = "ZoningSetbackSlider";
            setbackSlider.size = new Vector2(sliderPanel.width - setbackDepthLabel.width - (Margin * 3), 18f);
            setbackSlider.relativePosition = new Vector2(Margin, Margin);

            // Setback slider track.
            UISlicedSprite sliderSprite = setbackSlider.AddUIComponent<UISlicedSprite>();
            sliderSprite.atlas = TextureUtils.InGameAtlas;
            sliderSprite.spriteName = "BudgetSlider";
            sliderSprite.size = new Vector2(setbackSlider.width, 9f);
            sliderSprite.relativePosition = new Vector2(0f, 4f);

            // Setback slider thumb.
            UISlicedSprite sliderThumb = setbackSlider.AddUIComponent<UISlicedSprite>();
            sliderThumb.atlas = TextureUtils.InGameAtlas;
            sliderThumb.spriteName = "SliderBudget";
            setbackSlider.thumbObject = sliderThumb;

            // Setback slider values.
            setbackSlider.stepSize = 0.5f;
            setbackSlider.minValue = 0f;
            setbackSlider.maxValue = 8f;
            setbackSlider.value = 0f;

            setbackSlider.eventValueChanged += (control, value) =>
            {
                ZoneBlockPatch.setback = value;
                setbackDepthLabel.text = ZoneBlockPatch.setback.ToString() + "m";
            };

            // Bring to front.
            BringToFront();
        }
    }
}
