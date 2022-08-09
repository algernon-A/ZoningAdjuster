namespace ZoningAdjuster
{
    using System;
    using AlgernonCommons.Notifications;

    /// <summary>
    /// "What's new" update messages.
    /// </summary>
    internal class WhatsNewMessageListing
    {
        /// <summary>
        /// List of versions and associated update message lines (as translation keys).
        /// </summary>
        internal readonly WhatsNewMessage[] Messages = new WhatsNewMessage[]
        {
            new WhatsNewMessage
            {
                Version = new Version("1.4.0.0"),
                MessagesAreKeys = true,
                Messages = new string[]
                {
                    "ZMD_140_0"
                }
            },
            new WhatsNewMessage
            {
                Version = new Version("1.3.0.0"),
                MessagesAreKeys = true,
                Messages = new string[]
                {
                    "ZMD_130_0"
                }
            },
            new WhatsNewMessage
            {
                Version = new Version("1.2.0.0"),
                MessagesAreKeys = true,
                Messages = new string[]
                {
                    "ZMD_120_0",
                    "ZMD_120_1",
                    "ZMD_120_2",
                    "ZMD_120_3",
                    "ZMD_120_4",
                    "ZMD_120_5"
                }
            },
            new WhatsNewMessage
            {
                Version = new Version("1.1.1.0"),
                MessagesAreKeys = true,
                Messages = new string[]
                {
                    "ZMD_111_0",
                    "ZMD_111_1"
                }
            },
            new WhatsNewMessage
            {
                Version = new Version("1.1.0.0"),
                MessagesAreKeys = true,
                Messages = new string[]
                {
                    "ZMD_110_0",
                    "ZMD_110_1",
                    "ZMD_110_2"
                }
            },
            new WhatsNewMessage
            {
                Version = new Version("1.0.2.0"),
                MessagesAreKeys = true,
                Messages = new string[]
                {
                    "ZMD_102_0"
                }
            },
            new WhatsNewMessage
            {
                Version = new Version("1.0.0.0"),
                MessagesAreKeys = true,
                Messages = new string[]
                {
                    "ZMD_100_0",
                    "ZMD_100_1",
                    "ZMD_100_2"
                }
            }
        };
    }
}