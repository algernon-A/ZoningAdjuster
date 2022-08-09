namespace ZoningAdjuster
{
    using AlgernonCommons.Notifications;
    using AlgernonCommons.Patching;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using ICities;

    /// <summary>
    /// The base mod class for instantiation by the game.
    /// </summary>
    public sealed class Mod : PatcherMod, IUserMod
    {
        /// <summary>
        /// Gets the mod's base display name (name only).
        /// </summary>
        public override string BaseName => "Zoning Adjuster";

        /// <summary>
        /// Gets the mod's unique Harmony identfier.
        /// </summary>
        public override string HarmonyID => "algernon-A.csl.zoningadjuster";

        /// <summary>
        /// Gets the mod's description for display in the content manager.
        /// </summary>
        public string Description => Translations.Translate("ZMD_DESC");

        /// <summary>
        /// Gets the mod's what's new message array.
        /// </summary>
        public override WhatsNewMessage[] WhatsNewMessages => new WhatsNewMessageListing().Messages;

        /// <summary>
        /// Called by the game when the mod is enabled.
        /// </summary>
        public override void OnEnabled()
        {
            // Initialize zoning block data.
            new ZoneBlockData();

            base.OnEnabled();

            // Add the options panel event handler for the start screen (to enable/disable options panel based on visibility).
            // First, check to see if UIView is ready.
            if (UIView.GetAView() != null)
            {
                // It's ready - attach the hook now.
                OptionsPanelManager<OptionsPanel>.OptionsEventHook();
            }
            else
            {
                // Otherwise, queue the hook for when the intro's finished loading.
                LoadingManager.instance.m_introLoaded += OptionsPanelManager<OptionsPanel>.OptionsEventHook;
            }
        }

        /// <summary>
        /// Called by the game when the mod options panel is setup.
        /// </summary>
        /// <param name="helper">UI helper instance.</param>
        public void OnSettingsUI(UIHelperBase helper)
        {
            // Create options panel.
            OptionsPanelManager<OptionsPanel>.Setup(helper);
        }

        /// <summary>
        /// Saves settings file.
        /// </summary>
        public override void SaveSettings() => ModSettings.Save();

        /// <summary>
        /// Loads settings file.
        /// </summary>
        public override void LoadSettings() => ModSettings.Load();

        /// <summary>
        /// Apply Harmony patches.
        /// </summary>
        protected override void ApplyPatches() => Patcher.Instance.PatchAll();

        /// <summary>
        /// Remove Harmony patches.
        /// </summary>
        protected override void RemovePatches() => Patcher.Instance.UnpatchAll();
    }
}
