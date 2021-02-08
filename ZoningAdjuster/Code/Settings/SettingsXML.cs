using System;
using System.IO;
using System.Xml.Serialization;


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

        // Language.
        [XmlElement("Language")]
        public string Language { get => Translations.Language; set => Translations.Language = value; }


        // Panel position.
        [XmlElement("OffsetKey")]
        public int OffsetModifier { get => UIThreading.offsetModifier; set => UIThreading.offsetModifier = value; }


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