using System;
using System.Reflection;
using ZoningAdjuster.MessageBox;


namespace ZoningAdjuster
{
    /// <summary>
    /// "What's new" message box.  Based on macsergey's code in Intersection Marking Tool (Node Markup) mod.
    /// </summary>
    internal static class WhatsNew
    {
        // List of versions and associated update message lines (as translation keys).
        private readonly static WhatsNewMessage[] WhatsNewMessages = new WhatsNewMessage[]
        {
            new WhatsNewMessage
            {
                version = new Version("1.0.0.0"),
                versionHeader = "",
                messageKeys = true,
                messages = new string[]
                {
                    "ZMD_100_0",
                    "ZMD_100_1",
                    "ZMD_100_2"
                }
            }
        };


        /// <summary>
        /// Close button action.
        /// </summary>
        /// <returns>True (always)</returns>
        internal static bool Confirm() => true;

        /// <summary>
        /// 'Don't show again' button action.
        /// </summary>
        /// <returns>True (always)</returns>
        internal static bool DontShowAgain()
        {
            // Save current version to settings file.
            ModSettings.whatsNewVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            // Save current version header as beta.
            ModSettings.whatsNewBetaVersion = WhatsNewMessages[0].betaVersion;
            ZoningModSettingsFile.SaveSettings();

            return true;
        }


        /// <summary>
        /// Check if there's been an update since the last notification, and if so, show the update.
        /// </summary>
        internal static void ShowWhatsNew()
        {
            // Get last notified version and current mod version.
            Version whatsNewVersion = new Version(ModSettings.whatsNewVersion);
            WhatsNewMessage latestMessage = WhatsNewMessages[0];

            // Don't show notification if we're already up to (or ahead of) the first what's new message.
            if (whatsNewVersion < latestMessage.version)
            {
                // Show messagebox.
                WhatsNewMessageBox messageBox = MessageBoxBase.ShowModal<WhatsNewMessageBox>();
                messageBox.Title = ZoningAdjusterMod.ModName + " " + ZoningAdjusterMod.Version;
                messageBox.DSAButton.eventClicked += (component, clickEvent) => DontShowAgain();
                messageBox.SetMessages(whatsNewVersion, WhatsNewMessages);
            }
        }
    }


    /// <summary>
    /// Version message struct.
    /// </summary>
    public struct WhatsNewMessage
    {
        public Version version;
        public string versionHeader;
        public int betaVersion;
        public bool messageKeys;
        public string[] messages;
    }
}