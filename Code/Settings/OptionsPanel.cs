// <copyright file="OptionsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ZoningAdjuster
{
    using AlgernonCommons.Keybinding;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using ICities;

    /// <summary>
    /// The mod's options panel.
    /// </summary>
    public class OptionsPanel : UIPanel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsPanel"/> class.
        /// </summary>
        public OptionsPanel()
        {
            // Auto layout.
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            UIHelper helper = new UIHelper(this);

            // Language options.
            UIHelperBase languageGroup = helper.AddGroup(Translations.Translate("TRN_CHOICE"));
            UIDropDown languageDropDown = (UIDropDown)languageGroup.AddDropdown(Translations.Translate("TRN_CHOICE"), Translations.LanguageList, Translations.Index, (value) =>
            {
                Translations.Index = value;
                OptionsPanelManager<OptionsPanel>.LocaleChanged();
            });
            languageDropDown.autoSize = false;
            languageDropDown.width = 270f;

            // Offset key.
            UIHelper keyGroup = helper.AddGroup(Translations.Translate("ZMD_OPT_KEY")) as UIHelper;
            string[] keyOptions = new string[] { Translations.Translate("ZMD_SHIFT"), Translations.Translate("ZMD_CTRL"), Translations.Translate("ZMD_ALT") };
            UIDropDown offsetKeyDropDown = (UIDropDown)keyGroup.AddDropdown(Translations.Translate("ZMD_OPT_ZOK"), keyOptions, OffsetKeyThreading.OffsetModifier, (value) => OffsetKeyThreading.OffsetModifier = value);

            // Hotkey control.
            OptionsKeymapping keyMapping = (keyGroup.self as UIComponent).gameObject.AddComponent<UUIKeymapping>();

            // Panel/button visibility.
            UIHelperBase showGroup = helper.AddGroup(Translations.Translate("ZMD_OPT_SHO"));
            showGroup.AddCheckbox(Translations.Translate("ZMD_OPT_SOR"), ModSettings.ShowOnRoad, (isChecked) => ModSettings.ShowOnRoad = isChecked);
            showGroup.AddCheckbox(Translations.Translate("ZMD_OPT_SPB"), ModSettings.ShowPanelButton, (isChecked) => ModSettings.ShowPanelButton = isChecked);

            // Reset panel and button positions.
            UIHelperBase positionGroup = helper.AddGroup(Translations.Translate("ZMD_OPT_POS"));
            positionGroup.AddButton(Translations.Translate("ZMD_OPT_RPP"), () =>
            {
                ModSettings.PanelX = -1;
                ZoningSettingsPanel.Panel?.SetPosition();
            });
            positionGroup.AddButton(Translations.Translate("ZMD_OPT_RBP"), () =>
            {
                ModSettings.ButtonX = -1;
                ZoningAdjusterButton.Instance?.SetPosition();
            });
        }
    }
}