using System;
using System.IO;
using System.ComponentModel;
using System.Xml.Serialization;
using UnityEngine;
using ColossalFramework;


namespace ZoningAdjuster
{
    /// <summary>
    /// Static class to hold global mod settings.
    /// </summary>
    /// 
    [XmlRoot(ElementName = "ZoningAdjuster", Namespace = "")]
    public class ModSettings
    {
        // What's new notification version.
        [XmlIgnore]
        internal static string whatsNewVersion = "0.0";
        [XmlIgnore]
        internal static int whatsNewBetaVersion = 0;

        // Panel postion.
        [XmlIgnore]
        internal static float panelX = -1;
        [XmlIgnore]
        internal static float panelY = -1;

        // Button postion.
        [XmlIgnore]
        internal static float buttonX = -1;
        [XmlIgnore]
        internal static float buttonY = -1;

        // Panel visibility.
        [XmlIgnore]
        internal static bool showOnRoad = true;

        // Current zoning settings.
        [XmlIgnore]
        internal static bool disableZoning = false;
        [XmlIgnore]
        internal static bool forceZoning = false;

        // SavedInputKey reference for communicating with UUI.
        [XmlIgnore]
        private static readonly SavedInputKey uuiSavedKey = new SavedInputKey("Zoning Ajduster hotkey", "Zoning Ajduster hotkey", key: KeyCode.Z, control: false, shift: false, alt: true, false);

        // Settings file name
        [XmlIgnore]
        private static readonly string SettingsFileName = "ZoningAdjuster.xml";


        // Version.
        [XmlAttribute("Version")]
        public int version = 0;


        [XmlElement("WhatsNewVersion")]
        public string WhatsNewVersion { get => whatsNewVersion; set => whatsNewVersion = value; }


        [XmlElement("WhatsNewBetaVersion")]
        [DefaultValue(0)]
        public int WhatsNewBetaVersion { get => whatsNewBetaVersion; set => whatsNewBetaVersion = value; }


        // Language.
        [XmlElement("Language")]
        public string Language { get => Translations.Language; set => Translations.Language = value; }


        // Zoning tool hotkey.
        [XmlElement("PanelKey")]
        public KeyBinding PanelKey
        {
            get
            {
                return new KeyBinding
                {
                    keyCode = (int)PanelSavedKey.Key,
                    control = PanelSavedKey.Control,
                    shift = PanelSavedKey.Shift,
                    alt = PanelSavedKey.Alt
                };
            }
            set
            {
                uuiSavedKey.Key = (KeyCode)value.keyCode;
                uuiSavedKey.Control = value.control;
                uuiSavedKey.Shift = value.shift;
                uuiSavedKey.Alt = value.alt;

                // Save settings.
                Save();
            }
        }


        // Offset modifier key.
        [XmlElement("OffsetKey")]
        public int OffsetModifier { get => OffsetKeyThreading.offsetModifier; set => OffsetKeyThreading.offsetModifier = value; }


        // Show panel on road tool.
        [XmlElement("ShowOnRoad")]
        public bool ShowOnRoad { get => showOnRoad; set => showOnRoad = value; }


        // Panel X postion.
        [XmlElement("PanelX")]
        public float PanelX { get => panelX; set => panelX = value; }


        // Panel Y postion.
        [XmlElement("PanelY")]
        public float PanelY { get => panelY; set => panelY = value; }


        // Button X postion.
        [XmlElement("ButtonX")]
        public float ButtonX { get => buttonX; set => buttonX = value; }


        // Button Ypostion.
        [XmlElement("ButtonY")]
        public float ButtonY { get => buttonY; set => buttonY = value; }


        /// <summary>
        /// Panel hotkey as ColossalFramework SavedInputKey.
        /// </summary>
        [XmlIgnore]
        internal static SavedInputKey PanelSavedKey => uuiSavedKey;


        /// <summary>
        /// The current hotkey settings as ColossalFramework InputKey.
        /// </summary>
        /// </summary>
        [XmlIgnore]
        internal static InputKey CurrentHotkey
        {
            get => uuiSavedKey.value;

            set => uuiSavedKey.value = value;
        }


        /// <summary>
        /// Load settings from XML file.
        /// </summary>
        internal static void Load()
        {
            try
            {
                // Check to see if configuration file exists.
                if (File.Exists(SettingsFileName))
                {
                    // Read it.
                    using (StreamReader reader = new StreamReader(SettingsFileName))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModSettings));
                        if (!(xmlSerializer.Deserialize(reader) is ModSettings ZoningModSettingsFile))
                        {
                            Logging.Error("couldn't deserialize settings file");
                        }
                    }
                }
                else
                {
                    Logging.Message("no settings file found");
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception reading XML settings file");
            }
        }


        /// <summary>
        /// Save settings to XML file.
        /// </summary>
        internal static void Save()
        {
            try
            {
                // Pretty straightforward.  Serialisation is within GBRSettingsFile class.
                using (StreamWriter writer = new StreamWriter(SettingsFileName))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModSettings));
                    xmlSerializer.Serialize(writer, new ModSettings());
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception saving XML settings file");
            }
        }
    }


    /// <summary>
    /// Basic keybinding class - code and modifiers.
    /// </summary>
    public class KeyBinding
    {
        [XmlAttribute("KeyCode")]
        public int keyCode;

        [XmlAttribute("Control")]
        public bool control;

        [XmlAttribute("Shift")]
        public bool shift;

        [XmlAttribute("Alt")]
        public bool alt;
    }
}