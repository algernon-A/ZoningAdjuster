using System;
using System.IO;
using System.Xml.Serialization;
using System.ComponentModel;


namespace ZoningAdjuster
{
    /// <summary>
    /// XML settings file.
    /// </summary>
    [XmlRoot(ElementName = "ZoningAdjuster", Namespace = "")]
    public class ZoningModSettingsFile
    {
        [XmlIgnore]
        private static readonly string SettingsFileName = "ZoningAdjuster.xml";

        // Version.
        [XmlAttribute("Version")]
        public int version = 0;

        [XmlElement("WhatsNewVersion")]
        public string WhatsNewVersion { get => ModSettings.whatsNewVersion; set => ModSettings.whatsNewVersion = value; }

        [XmlElement("WhatsNewBetaVersion")]
        [DefaultValue(0)]
        public int WhatsNewBetaVersion { get => ModSettings.whatsNewBetaVersion; set => ModSettings.whatsNewBetaVersion = value; }

        // Language.
        [XmlElement("Language")]
        public string Language { get => Translations.Language; set => Translations.Language = value; }


        // Offset hotkey/
        [XmlElement("OffsetKey")]
        public int OffsetModifier { get => UIThreading.offsetModifier; set => UIThreading.offsetModifier = value; }

        // Show panel on road tool.
        [XmlElement("ShowOnRoad")]
        public bool ShowOnRoad { get => ModSettings.showOnRoad; set => ModSettings.showOnRoad = value; }

        // Panel postion.
        [XmlElement("PanelX")]
        public float PanelX { get => ModSettings.panelX; set => ModSettings.panelX = value; }

        // Panel postion.
        [XmlElement("PanelY")]
        public float PanelY { get => ModSettings.panelY; set => ModSettings.panelY = value; }

        // Button postion.
        [XmlElement("ButtonX")]
        public float ButtonX { get => ModSettings.buttonX; set => ModSettings.buttonX = value; }
        [XmlElement("ButtonY")]
        public float ButtonY { get => ModSettings.buttonY; set => ModSettings.buttonY = value; }


        /// <summary>
        /// Load settings from XML file.
        /// </summary>
        internal static void LoadSettings()
        {
            try
            {
                // Check to see if configuration file exists.
                if (File.Exists(SettingsFileName))
                {
                    // Read it.
                    using (StreamReader reader = new StreamReader(SettingsFileName))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(ZoningModSettingsFile));
                        if (!(xmlSerializer.Deserialize(reader) is ZoningModSettingsFile gbrSettingsFile))
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
        internal static void SaveSettings()
        {
            try
            {
                // Pretty straightforward.  Serialisation is within GBRSettingsFile class.
                using (StreamWriter writer = new StreamWriter(SettingsFileName))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(ZoningModSettingsFile));
                    xmlSerializer.Serialize(writer, new ZoningModSettingsFile());
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception saving XML settings file");
            }
        }
    }
}