// <copyright file="SegmentData.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ZoningAdjuster
{
    using AlgernonCommons;
    using ColossalFramework;

    /// <summary>
    /// Zone block stat class.
    /// </summary>
    internal class SegmentData
    {
        // Instance reference.
        private static SegmentData s_instance;

        // Zone block flag array.
        private readonly byte[] _zoneBlockFlags;

        // Current build settings.
        private bool preserveOldZones = false;
        private bool preserveNewZones = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SegmentData"/> class.
        /// </summary>
        internal SegmentData()
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
            NoZone = 0x08,
            ForceZone = 0x10,
            DepthOne = 0x00,
            DepthTwo = 0x20,
            DepthThree = 0x40,
            DepthFour = 0x60,
        }

        /// <summary>
        /// Gets the active instance.
        /// </summary>
        internal static SegmentData Instance => s_instance;

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
        /// Returns whether or not the given network segment is set to preserve older zoning.
        /// </summary>
        /// <param name="segmentID">Network segment ID.</param>
        /// <returns>True if older zoning is preserved for this block, false otherwise.</returns>
        internal bool PreserveOlder(ushort segmentID) => (_zoneBlockFlags[segmentID] & (ushort)ZoningAdjusterFlags.PreserveOlder) != 0;

        /// <summary>
        /// Returns whether or not the given network segment is set to preserve newer zoning.
        /// </summary>
        /// <param name="segmentID">Network segment ID.</param>
        /// <returns>True if newer zoning is preserved for this segment, false otherwise.</returns>
        internal bool PreserveNewer(ushort segmentID) => (_zoneBlockFlags[segmentID] & (ushort)ZoningAdjusterFlags.PreserveNewer) != 0;

        /// <summary>
        /// Returns whether or not the given network segment is set to not have zoning.
        /// </summary>
        /// <param name="segmentID">Network segment ID.</param>
        /// <returns>True if zoning is blocked for this segment, false otherwise.</returns>
        internal bool NoZoning(ushort segmentID)
        {
            // Check for current record.
            byte flags = _zoneBlockFlags[segmentID];
            if ((flags & (byte)ZoningAdjusterFlags.Created) != 0)
            {
                // Current record found; return it.
                return (_zoneBlockFlags[segmentID] & (ushort)ZoningAdjusterFlags.NoZone) != 0;
            }

            // No current record found; return current settings.
            return ModSettings.DisableZoning;
        }

        /// <summary>
        /// Returns whether or not the given network segment is set to force zoning.
        /// </summary>
        /// <param name="segmentID">Network segment ID.</param>
        /// <returns>True if zoning is forced for this segment, false otherwise.</returns>
        internal bool ForceZoning(ushort segmentID)
        {
            // Check for current record.
            byte flags = _zoneBlockFlags[segmentID];
            if ((flags & (byte)ZoningAdjusterFlags.Created) != 0)
            {
                // Current record found; return it.
                return (_zoneBlockFlags[segmentID] & (ushort)ZoningAdjusterFlags.ForceZone) != 0;
            }

            // No current record found; return current settings.
            return ModSettings.ForceZoning;
        }

        /// <summary>
        /// Records the current zoning settings for the specified network segment, if there are no existing active settings.
        /// </summary>
        /// <param name="segmentID">Network segment ID.</param>
        internal void UpdateCurrentMode(ushort segmentID)
        {
            if ((_zoneBlockFlags[segmentID] & (ushort)ZoningAdjusterFlags.Created) == 0)
            {
                SetCurrentMode(segmentID);
            }
        }

        /// <summary>
        /// Records the current zoning settings for the specified network segment, overwriting any existing settings.
        /// </summary>
        /// <param name="segmentID">Network segment ID.</param>
        internal void SetCurrentMode(ushort segmentID)
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

            // Record no-zoning status.
            if (ModSettings.DisableZoning)
            {
                newFlags |= ZoningAdjusterFlags.NoZone;
            }

            // Record forced zoning status.
            if (ModSettings.ForceZoning)
            {
                newFlags |= ZoningAdjusterFlags.ForceZone;
            }

            // Record zone depth setting.
            newFlags |= (ZoningAdjusterFlags)(CalcBlock1Patch.ZoneDepth << 5);

            // Record new flags.
            _zoneBlockFlags[segmentID] = (byte)newFlags;

            Logging.KeyMessage("recorded data for segment ", segmentID);
        }

        /// <summary>
        /// Gets the effective depth of the given segment.
        /// </summary>
        /// <param name="segmentID">Network segment ID.</param>
        /// <returns>Effective depth of the specified segment.</returns>
        internal int GetEffectiveDepth(ushort segmentID)
        {
            // Only return custom values for recorded segment IDs.
            // Zone blocks created before 1.15 won't have linked segment IDs - any with ZA settings should have had IDs added by the legacy deserializer.
            if (segmentID != 0)
            {
                // Get current flags setting.
                ZoningAdjusterFlags blockFlags = (ZoningAdjusterFlags)_zoneBlockFlags[segmentID];

                // Check for formal created flag.
                if ((blockFlags & ZoningAdjusterFlags.Created) != 0)
                {
                    // We retrieved a valid record; return it.
                    return (byte)blockFlags >> 5;
                }

                // Not created - is this the current segment selected by the tool?
                if (ZoningTool.CurrentSegment == segmentID)
                {
                    // Yes - return current depth setting.
                    return CalcBlock1Patch.ZoneDepth;
                }
            }

            // If we got here, vanilla zone block depth should be used (no custom settings or not currently selected segment).
            return 3;
        }

        /// <summary>
        /// Clears recorded data for the specified segment.
        /// </summary>
        /// <param name="segmentID">Segment ID to clear.</param>
        internal void ClearEntry(ushort segmentID)
        {
            _zoneBlockFlags[segmentID] = (byte)ZoningAdjusterFlags.None;
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
        /// Populates the zone block data array with data from savefile deserialization.
        /// </summary>
        /// <param name="data">Savefile data.</param>
        internal void ReadLegacyData(byte[] data)
        {
            Logging.KeyMessage("reading legacy data");

            // Read length is the shortest of the internal array length or the data length.
            int arrayLength = System.Math.Min(_zoneBlockFlags.Length, data.Length);

            // Zone block flags.
            byte[] legacyFlags = new byte[arrayLength];

            // Populate internal array from savegame.
            for (int i = 0; i < arrayLength; ++i)
            {
                legacyFlags[i] = data[i];
            }

            Logging.KeyMessage("read ", arrayLength, " legacy zone block records");

            // Map legacy zone indexes to network indexes.
            int assignedCount = 0;
            NetSegment[] segments = Singleton<NetManager>.instance.m_segments.m_buffer;
            ZoneBlock[] zoneBlocks = Singleton<ZoneManager>.instance.m_blocks.m_buffer;
            for (ushort i = 0; i < segments.Length; ++i)
            {
                // Look for valid segments.
                if ((segments[i].m_flags & NetSegment.Flags.Created) != 0)
                {
                    ConvertToSegment(i, segments[i].m_blockStartLeft, legacyFlags, zoneBlocks);
                    ConvertToSegment(i, segments[i].m_blockStartRight, legacyFlags, zoneBlocks);
                    ConvertToSegment(i, segments[i].m_blockEndLeft, legacyFlags, zoneBlocks);
                    ConvertToSegment(i, segments[i].m_blockEndRight, legacyFlags, zoneBlocks);
                }
            }

            Logging.Message("allocated ", assignedCount, " new segment records");

            for (int i = 0; i < legacyFlags.Length; ++i)
            {
                if (legacyFlags[i] != 0)
                {
                    Logging.Message("found legacy flags of ", legacyFlags[i], " for zone block ", i);
                }
            }
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

        /// <summary>
        /// Converts legacy block data to segment data.
        /// </summary>
        /// <param name="segmentID">Network segment ID.</param>
        /// <param name="blockID">ZoneBlock ID.</param>
        /// <param name="legacyFlags">Legacy zone flag array.</param>
        /// <param name="zoneBlocks">ZoneBlock data array.</param>
        private void ConvertToSegment(ushort segmentID, ushort blockID, byte[] legacyFlags, ZoneBlock[] zoneBlocks)
        {
            // Check if there's a valid legacy entry.
            if (blockID != 0 && legacyFlags[blockID] != 0)
            {
                // Legacy entry found - apply to owning segment.
                _zoneBlockFlags[segmentID] = legacyFlags[blockID];

                // Update zone block record to point to segment if no segmentID is already present.
                if (zoneBlocks[blockID].m_segment == 0)
                {
                    zoneBlocks[blockID].m_segment = segmentID;
                }
            }
        }
    }
}