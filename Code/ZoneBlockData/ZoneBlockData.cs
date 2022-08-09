// <copyright file="ZoneBlockData.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ZoningAdjuster
{
    using AlgernonCommons;
    using ColossalFramework;

    /// <summary>
    /// Static class containing.
    /// </summary>
    internal class ZoneBlockData
    {
        // Instance reference.
        private static ZoneBlockData s_instance;

        // Zone block flag array.
        private readonly byte[] _zoneBlockFlags;

        // Current build settings.
        private bool preserveOldZones = false;
        private bool preserveNewZones = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZoneBlockData"/> class.
        /// </summary>
        internal ZoneBlockData()
        {
            // Set instance reference.
            s_instance = this;

            // Initialise array.
            _zoneBlockFlags = new byte[Singleton<ZoneManager>.instance.m_blocks.m_buffer.Length];
        }

        /// <summary>
        /// Zoning priority indexes.
        /// </summary>
        public enum PriorityIndexes
        {
            /// <summary>
            /// Older zone blocks have priority.
            /// </summary>
            Older = 0,

            /// <summary>
            /// Newer zone blocks have priority.
            /// </summary>
            Newer,

            /// <summary>
            /// No age priority.
            /// </summary>
            None,

            /// <summary>
            /// Number of priorities.
            /// </summary>
            NumPriorities,
        }

        /// <summary>
        /// Zoning data flags.
        /// </summary>
        private enum ZoningAdjusterFlags : byte
        {
            None = 0x00,
            PreserveOlder = 0x01,
            PreserveNewer = 0x02,

            // Below added in verison 2 data - legacy data will only have the above flags.  Don't rely just on created.
            Created = 0x04,
            DepthOne = 0x00,
            DepthTwo = 0x20,
            DepthThree = 0x40,
            DepthFour = 0x60,
        }

        /// <summary>
        /// Gets the active instance.
        /// </summary>
        internal static ZoneBlockData Instance => s_instance;

        /// <summary>
        /// Gets the active zone block flags array.
        /// </summary>
        internal byte[] ZoneBlockFlags => _zoneBlockFlags;

        /// <summary>
        /// Gets the current prority state as an index.
        /// </summary>
        /// <returns>Current priority state index.</returns>
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
        /// <param name="index">Current priority state.</param>
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
        /// <param name="blockID">Block ID.</param>
        /// <returns>True if older zoning is preserved for this block, false otherwise.</returns>
        internal bool PreserveOlder(ushort blockID) => (_zoneBlockFlags[blockID] & (ushort)ZoningAdjusterFlags.PreserveOlder) != 0;

        /// <summary>
        /// Returns whether or not the given block is set to preserve newer zoning.
        /// </summary>
        /// <param name="blockID">Block ID.</param>
        /// <returns>True if newer zoning is preserved for this block, false otherwise.</returns>
        internal bool PreserveNewer(ushort blockID) => (_zoneBlockFlags[blockID] & (ushort)ZoningAdjusterFlags.PreserveNewer) != 0;

        /// <summary>
        /// Records the current zoning settings for the specified block.
        /// </summary>
        /// <param name="blockID">Block ID.</param>
        internal void SetCurrentMode(ushort blockID)
        {
            ZoningAdjusterFlags newFlags = ZoningAdjusterFlags.Created;

            // Add preserve newer/older flags as appropriate.
            if (preserveOldZones)
            {
                newFlags |= ZoningAdjusterFlags.PreserveOlder;
            }
            else if (preserveNewZones)
            {
                newFlags |= ZoningAdjusterFlags.PreserveNewer;
            }

            // Record zone depth setting.
            newFlags |= (ZoningAdjusterFlags)(ZoneDepthPatches.ZoneDepth << 5);

            // Set created flag.
            _zoneBlockFlags[blockID] = (byte)newFlags;
        }

        /// <summary>
        /// Gets the effective depth of the given zone block.
        /// </summary>
        /// <param name="blockID">Block ID.</param>
        /// <returns>Effective depth od the specified block.</returns>
        internal int GetEffectiveDepth(ushort blockID)
        {
            // Get current flags setting.
            ZoningAdjusterFlags blockFlags = (ZoningAdjusterFlags)_zoneBlockFlags[blockID];

            // Check for formal created flag.
            if ((blockFlags & ZoningAdjusterFlags.Created) == ZoningAdjusterFlags.None)
            {
                // Not created - return current depth setting.
                return ZoneDepthPatches.ZoneDepth;
            }

            // If we got here, we retrieved a valid record; return it.
            return (byte)blockFlags >> 5;
        }

        /// <summary>
        /// Clears recorded data for the specified block.
        /// </summary>
        /// <param name="blockID">Block ID to clear.</param>
        internal void ClearEntry(ushort blockID)
        {
            _zoneBlockFlags[blockID] = (byte)ZoningAdjusterFlags.None;
        }

        /// <summary>
        /// Populates the zone block data array with data from savefile deserialization.
        /// </summary>
        /// <param name="data">Savefile data.</param>
        internal void ReadData(byte[] data)
        {
            // Read length is the shortest of the internal array length or the data length.
            int arrayLength = System.Math.Min(_zoneBlockFlags.Length, data.Length);

            // Populate internal array from savegame.
            for (int i = 0; i < arrayLength; ++i)
            {
                _zoneBlockFlags[i] = data[i];
            }

            Logging.Message("assigned ", arrayLength, " zone block records");
        }

        /// <summary>
        /// Populates block data with default depth setting - used when deserializing legacy data.
        /// </summary>
        internal void DefaultDepths()
        {
            // Local reference.
            ZoneBlock[] blockBuffer = Singleton<ZoneManager>.instance.m_blocks.m_buffer;

            // Iterate through all zone blocks in game buffer.
            for (int i = 0; i < blockBuffer.Length; ++i)
            {
                // If this block is active, set the zone block flags to include default depth (four cells).
                if (blockBuffer[i].m_flags != 0)
                {
                    Logging.Message("adding default depth flags to zone block ", i);
                    _zoneBlockFlags[i] |= (byte)(ZoningAdjusterFlags.DepthFour | ZoningAdjusterFlags.Created);
                }
            }
        }
    }
}