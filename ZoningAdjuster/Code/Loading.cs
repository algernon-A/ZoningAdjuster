using ICities;
using ZoningAdjuster.MessageBox;


namespace ZoningAdjuster
{

	/// <summary>
	/// Main loading class: the mod runs from here.
	/// </summary>
	public class Loading : LoadingExtensionBase
    {
        // Status flags.
        private static bool harmonyLoaded = false;
        private static bool conflictingMod = false;


        /// <summary>
        /// Called by the game when the mod is initialised at the start of the loading process.
        /// </summary>
        /// <param name="loading">Loading mode (e.g. game, editor, scenario, etc.)</param>
        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);

            // Don't do anything if not in game (e.g. if we're going into an editor).
            if (loading.currentMode != AppMode.Game)
            {
                Logging.KeyMessage("not loading into game, skipping activation");

                // Set harmonyLoaded and PatchOperating flags to suppress Harmony warning when e.g. loading into editor.
                harmonyLoaded = true;

                // Unload Harmony patches and exit before doing anything further.
                Patcher.UnpatchAll();
                return;
            }

            // Ensure that Harmony patches have been applied.
            harmonyLoaded = Patcher.Patched;
            if (!harmonyLoaded)
            {
                Logging.Error("Harmony patches not applied; aborting");
                return;
            }

            // Check for mod conflicts.
            if (ModUtils.IsModConflict())
            {
                // Conflict detected.
                conflictingMod = true;

                // Unload Harmony patches and exit before doing anything further.
                Patcher.UnpatchAll();
                return;
            }
        }


        /// <summary>
        /// Called by the game when level loading is complete.
        /// </summary>
        /// <param name="mode">Loading mode (e.g. game, editor, scenario, etc.)</param>
        public override void OnLevelLoaded(LoadMode mode)
		{
			base.OnLevelLoaded(mode);

			// Check to see that Harmony 2 was properly loaded.
			if (!harmonyLoaded)
			{
				// Harmony 2 wasn't loaded.
				Logging.Error("Harmony patches not applied; aborting");

				// Display warning message.
				ListMessageBox harmonyBox = MessageBoxBase.ShowModal<ListMessageBox>();

				// Key text items.
				harmonyBox.AddParas(Translations.Translate("ERR_HAR0"), Translations.Translate("ZMD_ERR_HAR"), Translations.Translate("ZMD_ERR_FAT"), Translations.Translate("ERR_HAR1"));

				// List of dot points.
				harmonyBox.AddList(Translations.Translate("ERR_HAR2"), Translations.Translate("ERR_HAR3"));

				// Closing para.
				harmonyBox.AddParas(Translations.Translate("MES_PAGE"));

				// Don't do anything further.
				return;
			}

			// Check to see if a conflicting mod has been detected.
			if (conflictingMod)
			{
				// Mod conflict detected - display warning notification and exit.
				ListMessageBox modConflictBox = MessageBoxBase.ShowModal<ListMessageBox>();

				// Key text items.
				modConflictBox.AddParas(Translations.Translate("ERR_CON0"), Translations.Translate("ZMD_ERR_CON0"), Translations.Translate("ZMD_ERR_FAT"), Translations.Translate("ERR_CON1"));

				// Add conflicting mod name(s).
				modConflictBox.AddList(ModUtils.conflictingModNames.ToArray());

				// Closing para.
				modConflictBox.AddParas(Translations.Translate("ZMD_ERR_CON1"));

                // Don't do anything further.
                return;
            }

            // Initialise zoning tool.
            ToolsModifierControl.toolController.gameObject.AddComponent<ZoningTool>();

			// Add panel button.
			ZoningAdjusterButton.CreateButton();

			// Display update notification.
			WhatsNew.ShowWhatsNew();

            // Activate UI threads.
            OffsetKeyThreading.operating = true;
            ToolKeyThreading.operating = true;
		}
	}
}