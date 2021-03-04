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

        // Components.
        private UIPanel zoningOptionsPanel;
        private UILabel setbackDepthLabel;

        // Instance reference.
        public static ZoningAdjusterButton Instance { get; private set; }


        // Flag.
        bool panelClick = false;


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
            normalFgSprite = focusedFgSprite = hoveredFgSprite = disabledBgSprite = "normal";
            tooltip = Translations.Translate("ZMD_NAME");
            playAudioEvents = true;

            // Create panel.
            CreateOptionPanel();

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


        /// <summary>
        /// Creates the zoning adjuster button options panel.
        /// </summary>
        private void CreateOptionPanel()
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


            // Attach directly to button, so visibility and position are consistent.
            zoningOptionsPanel = Instance.AddUIComponent(typeof(UIPanel)) as UIPanel;
            zoningOptionsPanel.name = "ZoningOptionsPanel";
            zoningOptionsPanel.autoSize = false;
            zoningOptionsPanel.size = new Vector2(PanelWidth, PanelHeight);
            zoningOptionsPanel.relativePosition = new Vector2(0f, -PanelHeight - Margin);

            // Appearance.
            zoningOptionsPanel.atlas = TextureUtils.InGameAtlas;
            zoningOptionsPanel.backgroundSprite = "SubcategoriesPanel";
            zoningOptionsPanel.clipChildren = true;

            // Title.
            UILabel titleLabel = UIControls.AddLabel(zoningOptionsPanel, 0f, Margin, Translations.Translate("ZMD_NAME"));
            titleLabel.textAlignment = UIHorizontalAlignment.Center;
            titleLabel.width = PanelWidth;


            // Controls.
            UICheckBox zoneAgeCheck = UIControls.LabelledCheckBox(zoningOptionsPanel, Margin, CheckY, Translations.Translate("ZMD_PNL_POZ"));
            zoneAgeCheck.isChecked = CalcImpl2Patch.preserveOldZones;
            zoneAgeCheck.eventCheckChanged += (control, isChecked) => CalcImpl2Patch.preserveOldZones = isChecked;

            // Setback slider - same appearance as Fine Road Tool's, for consistency.
            // Setback slider label.
            UILabel setbackLabel = zoningOptionsPanel.AddUIComponent<UILabel>();
            setbackLabel.textScale = 0.9f;
            setbackLabel.text = Translations.Translate("ZMD_PNL_SBK");
            setbackLabel.relativePosition = new Vector2(Margin, SliderLabelY);
            setbackLabel.SendToBack();

            // Setback slider panel.
            UIPanel sliderPanel = zoningOptionsPanel.AddUIComponent<UIPanel>();
            sliderPanel.atlas = TextureUtils.InGameAtlas;
            sliderPanel.backgroundSprite = "GenericPanel";
            sliderPanel.color = new Color32(206, 206, 206, 255);
            sliderPanel.size = new Vector2(zoningOptionsPanel.width - (Margin * 2), SliderPanelHeight);
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
            zoningOptionsPanel.Hide();

            zoningOptionsPanel.eventClicked += (control, clickEvent) =>
            {
                panelClick = true;
                clickEvent.Use();
            };
        }
    }
}