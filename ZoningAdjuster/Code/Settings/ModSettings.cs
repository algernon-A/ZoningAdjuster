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
        // Settings file name.
        [XmlIgnore]
        private static readonly string SettingsFileName = "ZoningAdjuster.xml";

        // User settings directory.
        [XmlIgnore]
        private static readonly string UserSettingsDir = ColossalFramework.IO.DataLocation.localApplicationData;

        // Full userdir settings file name.
        [XmlIgnore]
        private static readonly string SettingsFile = Path.Combine(UserSettingsDir, SettingsFileName);

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


        /// <summary>
        /// Sets panel button visibility.
        /// </summary>
        [XmlIgnore]
        internal static bool ShowPanelButton
        {
            get => _showPanelButton;

            set
            {
                _showPanelButton = value;

                // Create or destroy panel button based on new state.
                if (value)
                {
                    ZoningAdjusterButton.CreateButton();
                }
                else
                {
                    ZoningAdjusterButton.DestroyButton();
                }
            }
        }
        [XmlIgnore]
        private static bool _showPanelButton = true;


        // Version.
        [XmlAttribute("Version")]
        public int version = 0;


        [XmlElement("WhatsNewVersion")]
        public string XMLWhatsNewVersion { get => whatsNewVersion; set => whatsNewVersion = value; }


        [XmlElement("WhatsNewBetaVersion")]
        [DefaultValue(0)]
        public int XMLWhatsNewBetaVersion { get => whatsNewBetaVersion; set => whatsNewBetaVersion = value; }


        // Language.
        [XmlElement("Language")]
        public string XMLLanguage { get => Translations.Language; set => Translations.Language = value; }


        // Zoning tool hotkey.
        [XmlElement("PanelKey")]
        public KeyBinding XMLPanelKey
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
            }
        }


        // Offset modifier key.
        [XmlElement("OffsetKey")]
        public int XMLOffsetModifier { get => OffsetKeyThreading.offsetModifier; set => OffsetKeyThreading.offsetModifier = value; }


        // Show panel on road tool.
        [XmlElement("ShowOnRoad")]
        public bool XMLShowOnRoad { get => showOnRoad; set => showOnRoad = value; }


        // Panel X postion.
        [XmlElement("PanelX")]
        public float XMLPanelX { get => panelX; set => panelX = value; }


        // Panel Y postion.
        [XmlElement("PanelY")]
        public float XMLPanelY { get => panelY; set => panelY = value; }


        // Button X postion.
        [XmlElement("ButtonX")]
        public float XMLButtonX { get => buttonX; set => buttonX = value; }


        // Button Ypostion.
        [XmlElement("ButtonY")]
        public float XMLButtonY { get => buttonY; set => buttonY = value; }


        // Show panel button.
        [XmlElement("ShowPanelButton")]
        public bool XMLShowPanelButton { get => ShowPanelButton; set => ShowPanelButton = value; }


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
                // Attempt to read new settings file (in user settings directory).
                string fileName = SettingsFile;
                if (!File.Exists(fileName))
                {
                    // No settings file in user directory; use application directory instead.
                    fileName = SettingsFileName;

                    if (!File.Exists(fileName))
                    {
                        Logging.Message("no settings file found");
                        return;
                    }
                }

                // Read settings file.
                using (StreamReader reader = new StreamReader(fileName))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModSettings));
                    if (!(xmlSerializer.Deserialize(reader) is ModSettings settingsFile))
                    {
                        Logging.Error("couldn't deserialize settings file");
                    }
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
                // Pretty straightforward.
                using (StreamWriter writer = new StreamWriter(SettingsFile))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ModSettings));
                    xmlSerializer.Serialize(writer, new ModSettings());
                }

                // Cleaning up after ourselves - delete any old config file in the application direcotry.
                if (File.Exists(SettingsFileName))
                {
                    File.Delete(SettingsFileName);
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