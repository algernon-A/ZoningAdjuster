// <copyright file="CalcImpl2Patch.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>
// Based on code by Colossal Order as analysed and reverse-engineered by boformer, to whom I owe a great debt for his work on this.
// Most of the commentary is by boformer.

namespace ZoningAdjuster
{
    using ColossalFramework;
    using ColossalFramework.Math;
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    ///  Harmony pre-emptive prefix for adjusting zone block creation.
    /// </summary>
    [HarmonyPatch(typeof(ZoneBlock), "CalculateImplementation2")]
    public static class CalcImpl2Patch
    {
        /// <summary>
        /// Harmony pre-emptive prefix for adjusting zone block creation when two zone blocks are overlapping.
        /// </summary>
        /// <param name="__instance">Calling ZoneBlock instance.</param>
        /// <param name="blockID">ID of this block.</param>
        /// <param name="other">Overlapping zone block.</param>
        /// <param name="valid">Zone block valid flags.</param>
        /// <param name="shared">Zone block shared flags.</param>
        /// <param name="minX">Minimum X bound.</param>
        /// <param name="minZ">Minimum Z bound.</param>
        /// <param name="maxX">Maximum X bound.</param>
        /// <param name="maxZ">Maximum Z bound.</param>
        /// <returns>ALways false (don't execute original method).</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
        public static bool Prefix(ref ZoneBlock __instance, ushort blockID, ref ZoneBlock other, ref ulong valid, ref ulong shared, float minX, float minZ, float maxX, float maxZ)
        {
            // 92 = sqrt(64^2+64^2)
            // if the other zone block is not marked as "created" or too far away, do nothing.
            if (((int)other.m_flags & ZoneBlock.FLAG_CREATED) == 0 || (double)Mathf.Abs(other.m_position.x - __instance.m_position.x) >= 92.0 || (double)Mathf.Abs(other.m_position.z - __instance.m_position.z) >= 92.0)
            {
                return false;
            }

            // Checks to seee if the other zone block is marked as "deleted".
            bool deleted = ((int)other.m_flags & ZoneBlock.FLAG_DELETED) != 0;

            // Get the width of this block and the other block.
            int rowCount = __instance.RowCount;
            int otherRowCount = other.RowCount;

            // Directions of the rows and columns of the block, multiplied by 8 (cell size).
            Vector2 columnDirection = new Vector2(Mathf.Cos(__instance.m_angle), Mathf.Sin(__instance.m_angle)) * 8f;
            Vector2 rowDirection = new Vector2(columnDirection.y, -columnDirection.x);

            // Directions of the rows and columns of the other block, multiplied by 8 (cell size).
            Vector2 otherColumnDirection = new Vector2(Mathf.Cos(other.m_angle), Mathf.Sin(other.m_angle)) * 8f;
            Vector2 otherRowDirection = new Vector2(otherColumnDirection.y, -otherColumnDirection.x);

            // Origin of the other block.
            Vector2 otherPositionXZ = VectorUtils.XZ(other.m_position);

            // Area of the other zone block.
            Quad2 otherZoneBlockQuad = new Quad2
            {
                a = otherPositionXZ - (4f * otherColumnDirection) - (4f * otherRowDirection),
                b = otherPositionXZ + (0f * otherColumnDirection) - (4f * otherRowDirection), // TODO change 0f to 4f to support 8 tiles deep zones
                c = otherPositionXZ + (0f * otherColumnDirection) + ((otherRowCount - 4) * otherRowDirection), // TODO change 0f to 4f to support 8 tiles deep zones
                d = otherPositionXZ - (4f * otherColumnDirection) + ((otherRowCount - 4) * otherRowDirection),
            };

            Vector2 otherQuadMin = otherZoneBlockQuad.Min();
            Vector2 otherQuadMax = otherZoneBlockQuad.Max();

            // Return if there is no chance that the 2 quads collide.
            if ((double)otherQuadMin.x > (double)maxX || (double)otherQuadMin.y > (double)maxZ || ((double)minX > (double)otherQuadMax.x || (double)minZ > (double)otherQuadMax.y))
            {
                return false;
            }

            // Origin of this block.
            Vector2 positionXZ = VectorUtils.XZ(__instance.m_position);

            // Area of this zone block (8x4 cells).
            Quad2 zoneBlockQuad = new Quad2
            {
                a = positionXZ - (4f * columnDirection) - (4f * rowDirection),
                b = positionXZ + (0f * columnDirection) - (4f * rowDirection), // TODO change 0f to 4f to support 8 tiles deep zones
                c = positionXZ + (0f * columnDirection) + ((rowCount - 4) * rowDirection), // TODO change 0f to 4f to support 8 tiles deep zones
                d = positionXZ - (4f * columnDirection) + ((rowCount - 4) * rowDirection),
            };

            // Return if the quads are not intersecting.
            if (!zoneBlockQuad.Intersect(otherZoneBlockQuad))
            {
                return false;
            }

            // Mod insert - segments.
            NetManager netManger = Singleton<NetManager>.instance;
            NetSegment[] segments = netManger.m_segments.m_buffer;
            ref NetSegment thisSegment = ref segments[__instance.m_segment];
            ref NetSegment otherSegment = ref segments[other.m_segment];

            // Iterate through each row in this block.
            for (int row = 0; row < rowCount; ++row)
            {
                // Calculate 2 relative row positions:
                // * one 0.01m from previous row
                // * one 0.01m from next row
                Vector2 rowNearPreviousLength = ((float)row - 3.99f) * rowDirection;
                Vector2 rowNearNextLength = ((float)row - 3.01f) * rowDirection;

                // set the quad to the row (4 cells)
                zoneBlockQuad.a = positionXZ - (4f * columnDirection) + rowNearPreviousLength;
                zoneBlockQuad.b = positionXZ + (0f * columnDirection) + rowNearPreviousLength; // TODO change 0f to 4f to support 8 tiles deep zones
                zoneBlockQuad.c = positionXZ + (0f * columnDirection) + rowNearNextLength; // TODO change 0f to 4f to support 8 tiles deep zones
                zoneBlockQuad.d = positionXZ - (4f * columnDirection) + rowNearNextLength;

                // Intersect the row quad with the other zone block quad.
                if (zoneBlockQuad.Intersect(otherZoneBlockQuad))
                {
                    // Iterate through each valid column.
                    for (int column = 0; (long)column < ZoneDepthPatches.ColumnCount && ((long)valid & 1L << (row << 3 | column)) != 0L; ++column)
                    {
                        // Calculate 2 relative column positions:
                        // * one 0.01m from previous column
                        // * one 0.01m from next column
                        Vector2 columnNearPreviousLength = ((float)column - 3.99f) * columnDirection;
                        Vector2 columnNearNextLength = ((float)column - 3.01f) * columnDirection;

                        // Middle position of the cell.
                        Vector2 cellMiddlePos = positionXZ + ((columnNearNextLength + columnNearPreviousLength + rowNearNextLength + rowNearPreviousLength) * 0.5f);

                        // Check if the middle position of the cell is contained in the quad of the other zone block (1 cell tolerance).
                        if (Quad2.Intersect(
                            otherZoneBlockQuad.a - otherColumnDirection - otherRowDirection,
                            otherZoneBlockQuad.b + otherColumnDirection - otherRowDirection,
                            otherZoneBlockQuad.c + otherColumnDirection + otherRowDirection,
                            otherZoneBlockQuad.d - otherColumnDirection + otherRowDirection,
                            cellMiddlePos))
                        {
                            // Create a quad for the cell.
                            Quad2 cellQuad = new Quad2
                            {
                                a = positionXZ + columnNearPreviousLength + rowNearPreviousLength,
                                b = positionXZ + columnNearNextLength + rowNearPreviousLength,
                                c = positionXZ + columnNearNextLength + rowNearNextLength,
                                d = positionXZ + columnNearPreviousLength + rowNearNextLength,
                            };

                            // Cycle through the cells of the other zone block.
                            bool cellIsValid = true;
                            bool shareCell = false;
                            for (int otherRow = 0; otherRow < otherRowCount && cellIsValid; ++otherRow)
                            {
                                // Calculate 2 relative row positions for the cell in the other zone block:
                                // * one 0.01m from previous row
                                // * one 0.01m from next row
                                Vector2 otherRowNearPreviousLength = ((float)otherRow - 3.99f) * otherRowDirection;
                                Vector2 otherRowNearNextLength = ((float)otherRow - 3.01f) * otherRowDirection;

                                // Iterate through each column of the other zone block.
                                for (int otherColumn = 0; (long)otherColumn < ZoneDepthPatches.ColumnCount && cellIsValid; ++otherColumn)
                                {
                                    // Checks if the cell is marked as valid in the valid mask of the other block, and that it is not contained in the shared mask.
                                    if (((long)other.m_valid & ~(long)other.m_shared & 1L << (otherRow << 3 | otherColumn)) != 0L)
                                    {
                                        // Calculate 2 relative column positions for the cell in the other zone block:
                                        // * one 0.01m from previous column
                                        // * one 0.01m from next column
                                        Vector2 otherColumnNearPreviousLength = ((float)otherColumn - 3.99f) * otherColumnDirection;
                                        Vector2 otherColumnNearNextLength = ((float)otherColumn - 3.01f) * otherColumnDirection;

                                        // Squared distance between the 2 cell middle positions.
                                        float cellMiddleDist = Vector2.SqrMagnitude(otherPositionXZ + ((otherColumnNearNextLength + otherColumnNearPreviousLength + otherRowNearNextLength + otherRowNearPreviousLength) * 0.5f) - cellMiddlePos);

                                        // Check if the 2 cells can touch/.
                                        if ((double)cellMiddleDist < 144.0)
                                        {
                                            // Make sure the other zone block is not deleted:
                                            if (!deleted)
                                            {
                                                // Difference of 2 radian angles (360 deg = 2*PI * 0.6366197f = 4f).
                                                // That means an angle difference of 90 deg would result in 1f.
                                                float angleDiff = Mathf.Abs(other.m_angle - __instance.m_angle) * 0.6366197f;
                                                float rightAngleDiff = angleDiff - Mathf.Floor(angleDiff); // difference from 90 deg

                                                // if the 2 cells are almost in the same spot with an angle difference of 0 90 180 270 deg, mark one of them as shared
                                                if ((double)cellMiddleDist < 0.00999999977648258 && ((double)rightAngleDiff < 0.00999999977648258 || (double)rightAngleDiff > 0.990000009536743))
                                                {
                                                    // The cell closer to road (or that was created earler) is kept, the other marked as shared
                                                    if (column < otherColumn || (column == otherColumn && __instance.m_buildIndex < other.m_buildIndex))
                                                    {
                                                        other.m_shared |= (ulong)(1L << (otherRow << 3 | otherColumn));
                                                    }
                                                    else
                                                    {
                                                        shareCell = true;
                                                    }
                                                }

                                                // Angles not right or not in the same place: Intersect the 2 cells.
                                                else if (cellQuad.Intersect(new Quad2()
                                                {
                                                    a = otherPositionXZ + otherColumnNearPreviousLength + otherRowNearPreviousLength,
                                                    b = otherPositionXZ + otherColumnNearNextLength + otherRowNearPreviousLength,
                                                    c = otherPositionXZ + otherColumnNearNextLength + otherRowNearNextLength,
                                                    d = otherPositionXZ + otherColumnNearPreviousLength + otherRowNearNextLength,
                                                }))
                                                {
                                                    // Mark the cell which is further away from the road (or was created later) as invalid.
                                                    // TODO adapt for 8 cell zones (low priority)
                                                    if ((otherColumn >= 4 && column >= 4) || (otherColumn < 4 && column < 4))
                                                    {
                                                        // Mod insert: checking for age precedence.
                                                        bool preserveOldZones = SegmentData.Instance.PreserveOlder(__instance.m_segment);
                                                        bool preserveNewZones = SegmentData.Instance.PreserveNewer(__instance.m_segment);

                                                        if ((preserveOldZones && thisSegment.m_buildIndex > otherSegment.m_buildIndex) || (preserveNewZones && thisSegment.m_buildIndex < otherSegment.m_buildIndex))
                                                        {
                                                            cellIsValid = false;
                                                        }
                                                        else if ((preserveOldZones && thisSegment.m_buildIndex < otherSegment.m_buildIndex) || (preserveNewZones && thisSegment.m_buildIndex > otherSegment.m_buildIndex))
                                                        {
                                                            other.m_valid &= (ulong)~(1L << (otherRow << 3 | otherColumn));
                                                        }
                                                        else if ((otherColumn >= 2 && column >= 2) || (otherColumn < 2 && column < 2))
                                                        {
                                                            // By default, the cell within the first two rows of its block takes precesence over cells that aren't.
                                                            if (__instance.m_buildIndex < other.m_buildIndex)
                                                            {
                                                                other.m_valid &= (ulong)~(1L << (otherRow << 3 | otherColumn));
                                                            }
                                                            else
                                                            {
                                                                cellIsValid = false;
                                                            }
                                                        }
                                                        else if (otherColumn < 2)
                                                        {
                                                            // By default, the cell within the first two rows of its block takes precesence over cells that aren't.
                                                            cellIsValid = false;
                                                        }
                                                        else
                                                        {
                                                            other.m_valid &= (ulong)~(1L << (otherRow << 3 | otherColumn));
                                                        }
                                                    }
                                                    else if (otherColumn < 4)
                                                    {
                                                        cellIsValid = false;
                                                    }
                                                    else
                                                    {
                                                        other.m_valid &= (ulong)~(1L << (otherRow << 3 | otherColumn));
                                                    }
                                                }
                                            }

                                            // Distance between cell middle pos < 6 = cells colliding and both cells near road.
                                            // If the cell is unzoned, take over the zone type of the other one.
                                            if ((double)cellMiddleDist < 36.0 && column < 4 && otherColumn < 4)
                                            {
                                                ItemClass.Zone zone1 = __instance.GetZone(column, row);
                                                ItemClass.Zone zone2 = other.GetZone(otherColumn, otherRow);
                                                if (zone1 == ItemClass.Zone.Unzoned)
                                                {
                                                    __instance.SetZone(column, row, zone2);
                                                }
                                                else if (zone2 == ItemClass.Zone.Unzoned && !deleted)
                                                {
                                                    other.SetZone(otherColumn, otherRow, zone1);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (!cellIsValid)
                            {
                                valid &= (ulong)~(1L << (row << 3 | column));
                                break;
                            }

                            if (shareCell)
                            {
                                shared |= (ulong)(1L << (row << 3 | column));
                            }
                        }
                    }
                }
            }

            // Pre-empt original method.
            return false;
        }
    }
}