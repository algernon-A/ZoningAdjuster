﻿using ColossalFramework;


namespace ZoningAdjuster
{
    // Zoning priority indexes.
    public enum PriorityIndexes
    {
        Older = 0,
        Newer,
        None,
        NumPriorities
    };

    // Zoning data flags.
    public enum ZoningAdjusterFlags
    {
        None = 0x0,
        PreserveOlder = 0x1,
        PreserveNewer = 0x2,
    }

    /// <summary>
    /// Static class containing 
    /// </summary>
    internal class ZoneBlockData
    {
        // Current build settings.
        private bool preserveOldZones = false;
        private bool preserveNewZones = false;

        // Instance reference.
        internal static ZoneBlockData Instance => instance;
        private static ZoneBlockData instance;

        // Zone block flag array.
        internal byte[] ZoneBlockFlags => zoneBlockFlags;
        private readonly byte[] zoneBlockFlags;


        /// <summary>
        /// Gets the current prority state as an index.
        /// </summary>
        /// <returns>Current priority state index</returns>
        internal int GetCurrentPriority()
        {
            if (preserveOldZones)
            {
                return (int)PriorityIndexes.Older;
            }
            else if (preserveNewZones)
            {
                return (int)PriorityIndexes.Newer;
            }
            else
            {
                return (int)PriorityIndexes.None;
            }
        }


        /// <summary>
        /// Sets the current prority state to an index.
        /// </summary>
        /// <returns>Current priority state index</returns>
        internal void SetCurrentPriority(int index)
        {
            switch (index)
            {
                case (int)PriorityIndexes.Older:
                    preserveOldZones = true;
                    preserveNewZones = false;
                    break;
                case (int)PriorityIndexes.Newer:
                    preserveOldZones = false;
                    preserveNewZones = true;
                    break;
                default:
                    preserveOldZones = false;
                    preserveNewZones = false;
                    break;
            }
        }


        /// <summary>
        /// Returns whether or not the given block is set to preserve older zoning.
        /// </summary>
        /// <param name="blockID">Block ID</param>
        /// <returns>True if older zoning is preserved for this block, false otherwise</returns>
        internal bool PreserveOlder(ushort blockID) => (zoneBlockFlags[blockID] & (ushort)ZoningAdjusterFlags.PreserveOlder) != 0;


        /// <summary>
        /// Returns whether or not the given block is set to preserve newer zoning.
        /// </summary>
        /// <param name="blockID">Block ID</param>
        /// <returns>True if newer zoning is preserved for this block, false otherwise</returns>
        internal bool PreserveNewer(ushort blockID) => (zoneBlockFlags[blockID] & (ushort)ZoningAdjusterFlags.PreserveNewer) != 0;


        /// <summary>
        /// Records the current zoning settings for the specified block.
        /// </summary>
        /// <param name="blockID"></param>
        internal void SetCurrentMode(ushort blockID)
        {
            if (preserveOldZones)
            {
                zoneBlockFlags[blockID] = (byte)ZoningAdjusterFlags.PreserveOlder;
            }
            else if (preserveNewZones)
            {
                zoneBlockFlags[blockID] = (byte)ZoningAdjusterFlags.PreserveNewer;
            }
            else
            {
                zoneBlockFlags[blockID] = (byte)ZoningAdjusterFlags.None;
            }
        }


        /// <summary>
        /// Populates the zone block data array with data from savefile deserialization.
        /// </summary>
        /// <param name="data"></param>
        internal void ReadData(byte[] data)
        {
            // Read length is the shortest of the internal array length or the data length.
            int arrayLength = System.Math.Min(zoneBlockFlags.Length, data.Length);

            // Populate internal array from savegame.
            for (int i = 0; i < arrayLength; ++i)
            {
                zoneBlockFlags[i] = data[i];
            }

            Logging.Message("assigned ", arrayLength.ToString(), " zone block records");
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        internal ZoneBlockData()
        {
            // Set instance reference.
            instance = this;

            // Initialise array.
            zoneBlockFlags = new byte[Singleton<ZoneManager>.instance.m_blocks.m_buffer.Length];
        }
    }
}