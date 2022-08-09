namespace ZoningAdjuster
{
    using System;
    using System.IO;
    using System.Xml.Serialization;
    using AlgernonCommons;
    using AlgernonCommons.Keybinding;
    using AlgernonCommons.XML;
    using UnityEngine;

    /// <summary>
    /// Global mod settings.
    /// </summary>
    /// 
    [XmlRoot(ElementName = "ZoningAdjuster", Namespace = "")]
    public class ModSettings : SettingsXMLBase
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

        // UUI hotkey.
        [XmlIgnore]
        private static readonly UnsavedInputKey UUIKey = new UnsavedInputKey(name: "Zoning Adjuster hotkey", keyCode: KeyCode.Z, control: false, shift: false, alt: true);

        /// <summary>
        /// Gets or sets a value indicating whether the legacy panel button should be visible.
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

        /// <summary>
        /// Gets or sets the current configuration file version.
        /// </summary>
        [XmlAttribute("Version")]
        public int Version { get; set; } = 0;
        
        /// <summary>
        /// Gets or sets the tool hotkey.
        /// </summary>
        [XmlElement("PanelKey")]
        public Keybinding XMLToolKey
        {
            get => UUIKey.Keybinding;

            set => UUIKey.Keybinding = value;
        }

        /// <summary>
        /// Gets or sets the offset modifier key.
        /// </summary>
        [XmlElement("OffsetKey")]
        public int XMLOffsetModifier { get => OffsetKeyThreading.OffsetModifier; set => OffsetKeyThreading.OffsetModifier = value; }

        /// <summary>
        /// Gets or sets a value indicating whether the Zoning Adjuste panel should be shown when the road tool is active.
        /// </summary>
        [XmlElement("ShowOnRoad")]
        public bool XMLShowOnRoad { get => showOnRoad; set => showOnRoad = value; }

        /// <summary>
        /// Gets or sets the panel's saved X-position.
        /// </summary>
        [XmlElement("PanelX")]
        public float XMLPanelX { get => panelX; set => panelX = value; }

        /// <summary>
        /// Gets or sets the panel's saved Y-position.
        /// </summary>
        [XmlElement("PanelY")]
        public float XMLPanelY { get => panelY; set => panelY = value; }

        /// <summary>
        /// Gets or sets the panel buttons's saved X-position.
        /// </summary>
        [XmlElement("ButtonX")]
        public float XMLButtonX { get => buttonX; set => buttonX = value; }

        /// <summary>
        /// Gets or sets the panel buttons's saved Y-position.
        /// </summary>
        [XmlElement("ButtonY")]
        public float XMLButtonY { get => buttonY; set => buttonY = value; }

        /// <summary>
        /// Gets or sets a value indicating whether the legacy panel button should be visible.
        /// </summary>
        [XmlElement("ShowPanelButton")]
        public bool XMLShowPanelButton { get => ShowPanelButton; set => ShowPanelButton = value; }

        /// <summary>
        /// Gets the current hotkey as a UUI UnsavedInputKey.
        /// </summary>
        [XmlIgnore]
        internal static UnsavedInputKey ToolKey => UUIKey;

        /// <summary>
        /// Load settings from XML file.
        /// </summary>S
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
}