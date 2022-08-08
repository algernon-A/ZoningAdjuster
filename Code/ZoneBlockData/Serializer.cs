using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ICities;
using ColossalFramework.IO;


namespace ZoningAdjuster
{
    /// <summary>
    /// Handles savegame data saving and loading.
    /// </summary>
    public class Serializer : SerializableDataExtensionBase
    {
        // Unique data ID.
        private readonly string dataID = "ZoningAdjuster";
        internal const int CurrentDataVersion = 1;


        /// <summary>
        /// Serializes data to the savegame.
        /// Called by the game on save.
        /// </summary>
        public override void OnSaveData()
        {
            base.OnSaveData();


            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();

                // Serialise savegame settings.
                DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, CurrentDataVersion, new RealPopSerializer());

                // Write to savegame.
                serializableDataManager.SaveData(dataID, stream.ToArray());

                Logging.Message("wrote ", stream.Length);
            }
        }


        /// <summary>
        /// Deserializes data from a savegame (or initialises new data structures when none available).
        /// Called by the game on load (including a new game).
        /// </summary>
        public override void OnLoadData()
        {
            Logging.Message("reading data from save file");
            base.OnLoadData();

            // Read data from savegame.
            byte[] data = serializableDataManager.LoadData(dataID);

            // Check to see if anything was read.
            if (data != null && data.Length != 0)
            {
                // Data was read - go ahead and deserialise.
                using (MemoryStream stream = new MemoryStream(data))
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    // Deserialise savegame settings.
                    DataSerializer.Deserialize<RealPopSerializer>(stream, DataSerializer.Mode.Memory);

                    Logging.Message("read ", stream.Length);
                }
            }
            else
            {
                // No data read.
                Logging.Message("no data read");
            }
        }
    }


    /// <summary>
    ///  Savegame (de)serialisation for settings.
    /// </summary>
    public class RealPopSerializer : IDataContainer
    {
        /// <summary>
        /// Serialise to savegame.
        /// </summary>
        /// <param name="serializer">Data serializer</param>
        public void Serialize(DataSerializer serializer)
        {
            Logging.Message("writing data to save file");

            // Write data version.
            serializer.WriteInt32(Serializer.CurrentDataVersion);

            // Write block data.
            serializer.WriteByteArray(ZoneBlockData.Instance.ZoneBlockFlags);
        }


        /// <summary>
        /// Deseralise from savegame.
        /// </summary>
        /// <param name="serializer">Data serializer</param>
        public void Deserialize(DataSerializer serializer)
        {
            Logging.Message("deserializing data from save file");

            try
            {
                // Read data version.
                int dataVersion = serializer.ReadInt32();
                Logging.Message("read data version ", dataVersion.ToString());

                // Make sure we have a matching data version.
                if (dataVersion <= Serializer.CurrentDataVersion)
                {
                    ZoneBlockData.Instance.ReadData(serializer.ReadByteArray());
                }

                // If data version is 0, then populate existing zone block depths with default (4).
                if (dataVersion == 0)
                {
                    ZoneBlockData.Instance.DefaultDepths();
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception deserializing savegame data");
            }
        }


        /// <summary>
        /// Performs post-serialization data management.  Nothing to do here (yet).
        /// </summary>
        /// <param name="serializer">Data serializer</param>
        public void AfterDeserialize(DataSerializer serializer)
        {
        }
    }
}