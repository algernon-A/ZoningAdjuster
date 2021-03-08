using UnityEngine;
using ColossalFramework.UI;


namespace ZoningAdjuster
{
    /// <summary>
    /// The control panel for the zoning adapter tool.
    /// </summary>
    public class ToolControlPanel : UIPanel
    {
        // Layout constants.
        const float Margin = 8f;
        const float PanelWidth = 230f;
        const float TitleHeight = 40f;
        const float CheckY = TitleHeight;
        const float SliderLabelY = CheckY + 30f;
        const float SliderLabelHeight = 20f;
        const float SliderPanelY = SliderLabelY + SliderLabelHeight;
        const float SliderPanelHeight = 36f;
        const float PanelHeight = SliderPanelY + SliderPanelHeight + Margin;

        // Panel components.
        private UILabel setbackDepthLabel;

        /// <summary>
        /// Constructor.
        /// </summary>
        public void Setup()
        {
            name = "ZoningOptionsPanel";
            autoSize = false;
            size = new Vector2(PanelWidth, PanelHeight);
            relativePosition = new Vector2(0f, -PanelHeight - Margin);

            // Appearance.
            atlas = TextureUtils.InGameAtlas;
            backgroundSprite = "SubcategoriesPanel";
            clipChildren = true;
             
            // Title.
            UILabel titleLabel = UIControls.AddLabel(this, 0f, Margin, Translations.Translate("ZMD_NAME"));
            titleLabel.textAlignment = UIHorizontalAlignment.Center;
            titleLabel.width = PanelWidth;

            // Drag bar.
            UIDragHandle dragHandle = AddUIComponent<UIDragHandle>();
            dragHandle.width = this.width;
            dragHandle.height = this.height;
            dragHandle.relativePosition = Vector3.zero;
            dragHandle.target = this;

            // Controls.
            UICheckBox zoneAgeCheck = UIControls.LabelledCheckBox(this, Margin, CheckY, Translations.Translate("ZMD_PNL_POZ"));
            zoneAgeCheck.isChecked = CalcImpl2Patch.preserveOldZones;
            zoneAgeCheck.eventCheckChanged += (control, isChecked) => CalcImpl2Patch.preserveOldZones = isChecked;

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

            // Sart hidden.
            Hide();

            // Stop clicks on panel triggering parent button click events (because using the events doesn't work).
            eventClicked += (control, clickEvent) =>
            {
                ZoningAdjusterButton.Instance.panelClick = true;
            };
        }
    }
}
