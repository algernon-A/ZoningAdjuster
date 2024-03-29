﻿// <copyright file="Serializer.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ZoningAdjuster
{
    using System.IO;
    using AlgernonCommons;
    using ColossalFramework.IO;
    using ICities;

    /// <summary>
    /// Handles savegame data saving and loading.
    /// </summary>
    public sealed class Serializer : SerializableDataExtensionBase
    {
        /// <summary>
        /// Current data version.
        /// </summary>
        internal const int CurrentDataVersion = 2;

        // Unique data ID.
        private readonly string oldDataID = "ZoningAdjuster";
        private readonly string newDataID = "ZoningAdjusterData";
        private readonly string v2DataID = "ZoningAdjusterData2";

        /// <summary>
        /// Serializes data to the savegame.
        /// Called by the game on save.
        /// </summary>
        public override void OnSaveData()
        {
            base.OnSaveData();

            using (MemoryStream stream = new MemoryStream())
            {
                // Serialise savegame settings.
                DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, CurrentDataVersion, new DataContainer());

                // Write to savegame.
                serializableDataManager.SaveData(v2DataID, stream.ToArray());

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
            byte[] data = serializableDataManager.LoadData(oldDataID);

            // Check to see if anything was read - start with trying original data ID.
            if (data != null && data.Length != 0)
            {
                Logging.Message("found legacy data");

                // Data was read - go ahead and deserialise.
                using (MemoryStream stream = new MemoryStream(data))
                {
                    // Deserialise savegame settings.
                    DataSerializer.Deserialize<RealPopSerializer>(stream, DataSerializer.Mode.Memory);

                    Logging.Message("read ", stream.Length);
                }
            }
            else
            {
                // Trying next generation data ID.
                data = serializableDataManager.LoadData(newDataID);

                // Check to see if anything was read.
                if (data != null && data.Length != 0)
                {
                    Logging.Message("found next gen legacy data");

                    // Data was read - go ahead and deserialise.
                    using (MemoryStream stream = new MemoryStream(data))
                    {
                        // Deserialise savegame settings.
                        DataSerializer.Deserialize<RealPopSerializer>(stream, DataSerializer.Mode.Memory);

                        Logging.Message("read ", stream.Length);
                    }
                }
                else
                {
                    // Try third generation data ID.
                    data = serializableDataManager.LoadData(v2DataID);

                    // Check to see if anything was read.
                    if (data != null && data.Length != 0)
                    {
                        Logging.Message("found v2 data");

                        // Data was read - go ahead and deserialise.
                        using (MemoryStream stream = new MemoryStream(data))
                        {
                            // Deserialise savegame settings.
                            DataSerializer.Deserialize<DataContainer>(stream, DataSerializer.Mode.Memory);

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
        }
    }
}