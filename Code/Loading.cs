// <copyright file="Loading.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ZoningAdjuster
{
    using AlgernonCommons;
    using AlgernonCommons.Notifications;
    using AlgernonCommons.Translation;
    using ICities;

    /// <summary>
    /// Main loading class: the mod runs from here.
    /// </summary>
    public sealed class Loading : LoadingExtensionBase
    {
        // Status flags.
        private static bool harmonyLoaded = false;
        private static bool conflictingMod = false;

        /// <summary>
        /// Gets a value indicating whether the mod has finished loading.
        /// </summary>
        internal static bool IsLoaded { get; private set; } = false;

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
                Patcher.Instance.UnpatchAll();
                return;
            }

            // Ensure that Harmony patches have been applied.
            harmonyLoaded = Patcher.Instance.Patched;
            if (!harmonyLoaded)
            {
                Logging.Error("Harmony patches not applied; aborting");
                return;
            }

            // Check for mod conflicts.
            if (ConflictDetection.IsModConflict())
            {
                // Conflict detected.
                conflictingMod = true;

                // Unload Harmony patches and exit before doing anything further.
                Patcher.Instance.UnpatchAll();
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
                ListNotification harmonyNotification = NotificationBase.ShowNotification<ListNotification>();

                // Key text items.
                harmonyNotification.AddParas(Translations.Translate("ERR_HAR0"), Translations.Translate("ZMD_ERR_HAR"), Translations.Translate("ZMD_ERR_FAT"), Translations.Translate("ERR_HAR1"));

                // List of dot points.
                harmonyNotification.AddList(Translations.Translate("ERR_HAR2"), Translations.Translate("ERR_HAR3"));

                // Closing para.
                harmonyNotification.AddParas(Translations.Translate("MES_PAGE"));

                // Don't do anything further.
                return;
            }

            // Check to see if a conflicting mod has been detected.
            if (conflictingMod)
            {
                // Mod conflict detected - display warning notification and exit.
                ListNotification modConflictNotification = NotificationBase.ShowNotification<ListNotification>();

                // Key text items.
                modConflictNotification.AddParas(Translations.Translate("ERR_CON0"), Translations.Translate("ZMD_ERR_CON0"), Translations.Translate("ZMD_ERR_FAT"), Translations.Translate("ERR_CON1"));

                // Add conflicting mod name(s).
                modConflictNotification.AddList(ConflictDetection.ConflictingModNames.ToArray());

                // Closing para.
                modConflictNotification.AddParas(Translations.Translate("ZMD_ERR_CON1"));

                // Don't do anything further.
                return;
            }

            // Initialise zoning tool.
            ToolsModifierControl.toolController.gameObject.AddComponent<ZoningTool>();

            // Set loaded status.
            IsLoaded = true;

            // Add panel button.
            ZoningAdjusterButton.CreateButton();

            // Display update notification.
            WhatsNew.ShowWhatsNew();

            Logging.Message("loading complete");
        }

        /// <summary>
        /// Called by the game when exiting a level.
        /// </summary>
        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();

            // Clear loading status.
            IsLoaded = false;

            // Destroy panel button.
            ZoningAdjusterButton.DestroyButton();
        }
    }
}