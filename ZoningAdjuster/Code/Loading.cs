using ICities;
using ZoningAdjuster.MessageBox;


namespace ZoningAdjuster
{

	/// <summary>
	/// Main loading class: the mod runs from here.
	/// </summary>
	public class Loading : LoadingExtensionBase
	{
		/// <summary>
		/// Called by the game when level loading is complete.
		/// </summary>
		/// <param name="mode">Loading mode (e.g. game, editor, scenario, etc.)</param>
		public override void OnLevelLoaded(LoadMode mode)
		{
			base.OnLevelLoaded(mode);

			// Check to see that Harmony 2 was properly loaded.
			if (!Patcher.Patched)
			{
				// Harmony 2 wasn't loaded.
				Logging.Error("Harmony patches not applied; aborting");

				// Display warning message.
				ListMessageBox harmonyBox = MessageBoxBase.ShowModal<ListMessageBox>();

				// Key text items.
				harmonyBox.AddParas(Translations.Translate("ERR_HAR0"), Translations.Translate("ZAM_ERR_HAR"), Translations.Translate("ZAM_ERR_FAT"), Translations.Translate("ERR_HAR1"));

				// List of dot points.
				harmonyBox.AddList(Translations.Translate("ERR_HAR2"), Translations.Translate("ERR_HAR3"));

				// Closing para.
				harmonyBox.AddParas(Translations.Translate("MES_PAGE"));

				// Don't do anything further.
				return;
			}

			// Initialise zoning tool.
			ToolsModifierControl.toolController.gameObject.AddComponent<ZoningTool>();

			// Add panel button.
			ZoningAdjusterButton.CreateButton();

			// Display update notification.
			WhatsNew.ShowWhatsNew();
		}
	}
}