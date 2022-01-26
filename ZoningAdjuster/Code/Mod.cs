using ICities;
using ColossalFramework.UI;
using CitiesHarmony.API;


namespace ZoningAdjuster
{
    public class ZoningAdjusterMod : IUserMod
    {
        // Public mod name and description.
        public string Name => ModName + " " + Version;
        public string Description => Translations.Translate("ZMD_DESC");

        // Internal and private name and version components.
        internal static string ModName => "Zoning Adjuster";
        internal static string Version => BaseVersion + " " + Beta;
        internal static string Beta => "";
        internal static int BetaVersion => 0;
        private static string BaseVersion => "1.5.0.1";


        /// <summary>
        /// Called by the game when the mod is disabled.
        /// </summary>
        public void OnDisabled()
        {
            // Unapply Harmony patches via Cities Harmony.
            if (HarmonyHelper.IsHarmonyInstalled)
            {
                Patcher.UnpatchAll();
            }
        }


        /// <summary>
        /// Called by the game when the mod is enabled.
        /// </summary>
        public void OnEnabled()
        {
            // Initialize zoneing block data.
            new ZoneBlockData();

            // Apply Harmony patches via Cities Harmony.
            // Called here instead of OnCreated to allow the auto-downloader to do its work prior to launch.
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());

            // Load the settings file.
            ModSettings.Load();
        }


        /// <summary>
        /// Called by the game when the mod options panel is setup.
        /// </summary>
        public void OnSettingsUI(UIHelperBase helper)
        {
            // Offset key.
            UIHelperBase keyGroup = helper.AddGroup(Translations.Translate("ZMD_OPT_KEY"));
            string[] keyOptions = new string[] { Translations.Translate("ZMD_SHIFT"), Translations.Translate("ZMD_CTRL"), Translations.Translate("ZMD_ALT") };
            UIDropDown offsetKeyDropDown = (UIDropDown)keyGroup.AddDropdown(Translations.Translate("ZMD_OPT_ZOK"), keyOptions, OffsetKeyThreading.offsetModifier, (value) => { OffsetKeyThreading.offsetModifier = value; ModSettings.Save(); });

            // Tool activation hotkey.
            offsetKeyDropDown.parent.parent.gameObject.AddComponent<OptionsKeymapping>();

            // Panel/button visibility.
            UIHelperBase showGroup = helper.AddGroup(Translations.Translate("ZMD_OPT_SHO"));
            showGroup.AddCheckbox(Translations.Translate("ZMD_OPT_SOR"), ModSettings.showOnRoad, (isChecked) => { ModSettings.showOnRoad = isChecked; ModSettings.Save(); });
            showGroup.AddCheckbox(Translations.Translate("ZMD_OPT_SPB"), ModSettings.ShowPanelButton, (isChecked) => { ModSettings.ShowPanelButton = isChecked; ModSettings.Save(); });

            // Reset panel and button positions.
            UIHelperBase positionGroup = helper.AddGroup(Translations.Translate("ZMD_OPT_POS"));
            positionGroup.AddButton(Translations.Translate("ZMD_OPT_RPP"), delegate { ModSettings.panelX = -1; ZoningSettingsPanel.Panel?.SetPosition(); ModSettings.panelX = -1; ModSettings.Save(); });
            positionGroup.AddButton(Translations.Translate("ZMD_OPT_RBP"), delegate { ModSettings.buttonX = -1; ZoningAdjusterButton.Instance?.SetPosition(); });

            // Language options.
            UIHelperBase languageGroup = helper.AddGroup(Translations.Translate("TRN_CHOICE"));
            UIDropDown languageDropDown = (UIDropDown)languageGroup.AddDropdown(Translations.Translate("TRN_CHOICE"), Translations.LanguageList, Translations.Index, (value) => { Translations.Index = value; ModSettings.Save(); });
            languageDropDown.autoSize = false;
            languageDropDown.width = 270f;
        }
    }
}
