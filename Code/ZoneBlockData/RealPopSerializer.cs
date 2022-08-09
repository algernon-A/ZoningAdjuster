// <copyright file="RealPopSerializer.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ZoningAdjuster
{
    using System;
    using AlgernonCommons;
    using ColossalFramework.IO;

    /// <summary>
    ///  Savegame (de)serialisation for settings.
    /// </summary>
    public sealed class RealPopSerializer : IDataContainer
    {
        /// <summary>
        /// Serialise to savegame.
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
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
        /// <param name="serializer">Data serializer.</param>
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
        /// <param name="serializer">Data serializer.</param>
        public void AfterDeserialize(DataSerializer serializer)
        {
        }
    }
}