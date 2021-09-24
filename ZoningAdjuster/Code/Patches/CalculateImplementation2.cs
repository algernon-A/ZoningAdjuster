using UnityEngine;
using ColossalFramework.Math;
using HarmonyLib;


namespace ZoningAdjuster
{
    /// <summary>
    ///  Harmony pre-emptive prefix for adjusting zone block creation.
    /// </summary>
    [HarmonyPatch(typeof(ZoneBlock), "CalculateImplementation2")]
    public static class CalcImpl2Patch
    {


        /// <summary>
        /// Harmony pre-emptive prefix for adjusting zone block creation when two zone blocks are overlapping.
        /// </summary>
        /// <param name="__instance">Instance reference</param>
        /// <param name="blockID">ID of this block</param>
        /// <param name="other">Overlapping zone block</param>
        /// <param name="valid">Zone block valid flags</param>
        /// <param name="shared">Zone block shared flags</param>
        /// <param name="minX">Maximum X bound</param>
        /// <param name="minZ">Minimum Z bound</param>
        /// <param name="maxX">Maximum X bound</param>
        /// <param name="maxZ">Maximum Z bound</param>
        /// <returns>ALways false (don't execute original method)</returns>
        public static bool Prefix(ref ZoneBlock __instance, ushort blockID, ref ZoneBlock other, ref ulong valid, ref ulong shared, float minX, float minZ, float maxX, float maxZ)
        {
            // 92 = sqrt(64^2+64^2)
            // if the other zone block is not marked as "created" or too far away, do nothing
            if (((int)other.m_flags & ZoneBlock.FLAG_CREATED) == 0 || (double)Mathf.Abs(other.m_position.x - __instance.m_position.x) >= 92.0 || (double)Mathf.Abs(other.m_position.z - __instance.m_position.z) >= 92.0)
            {
                return false;
            }

            // checks if the other zone block is marked as "deleted"
            bool deleted = ((int)other.m_flags & ZoneBlock.FLAG_DELETED) != 0;

            // width of block and other block
            int rowCount = __instance.RowCount;
            int otherRowCount = other.RowCount;

            // directions of the rows and columns of the block, multiplied by 8 (cell size)
            Vector2 columnDirection = new Vector2(Mathf.Cos(__instance.m_angle), Mathf.Sin(__instance.m_angle)) * 8f;
            Vector2 rowDirection = new Vector2(columnDirection.y, -columnDirection.x);

            // directions of the rows and columns of the other block, multiplied by 8 (cell size)
            Vector2 otherColumnDirection = new Vector2(Mathf.Cos(other.m_angle), Mathf.Sin(other.m_angle)) * 8f;
            Vector2 otherRowDirection = new Vector2(otherColumnDirection.y, -otherColumnDirection.x);

            // origin of the other block
            Vector2 otherPositionXZ = VectorUtils.XZ(other.m_position);

            // area of the other zone block
            Quad2 otherZoneBlockQuad = new Quad2
            {
                a = otherPositionXZ - 4f * otherColumnDirection - 4f * otherRowDirection,
                b = otherPositionXZ + 0f * otherColumnDirection - 4f * otherRowDirection, // TODO change 0f to 4f to support 8 tiles deep zones
                c = otherPositionXZ + 0f * otherColumnDirection + (float)(otherRowCount - 4) * otherRowDirection, // TODO change 0f to 4f to support 8 tiles deep zones
                d = otherPositionXZ - 4f * otherColumnDirection + (float)(otherRowCount - 4) * otherRowDirection
            };

            Vector2 otherQuadMin = otherZoneBlockQuad.Min();
            Vector2 otherQuadMax = otherZoneBlockQuad.Max();

            // return if there is no chance that the 2 quads collide
            if ((double)otherQuadMin.x > (double)maxX || (double)otherQuadMin.y > (double)maxZ || ((double)minX > (double)otherQuadMax.x || (double)minZ > (double)otherQuadMax.y))
            {
                return false;
            }

            // origin of the block
            Vector2 positionXZ = VectorUtils.XZ(__instance.m_position);

            // area of the zone block (8x4 cells)
            Quad2 zoneBlockQuad = new Quad2
            {
                a = positionXZ - 4f * columnDirection - 4f * rowDirection,
                b = positionXZ + 0f * columnDirection - 4f * rowDirection, // TODO change 0f to 4f to support 8 tiles deep zones
                c = positionXZ + 0f * columnDirection + (float)(rowCount - 4) * rowDirection, // TODO change 0f to 4f to support 8 tiles deep zones
                d = positionXZ - 4f * columnDirection + (float)(rowCount - 4) * rowDirection
            };

            // return if the quads are not intersecting
            if (!zoneBlockQuad.Intersect(otherZoneBlockQuad)) return false;

            for (int row = 0; row < rowCount; ++row)
            {
                // calculate 2 relative row positions: 
                // * one 0.01m from previous row
                // * one 0.01m from next row
                Vector2 rowNearPreviousLength = ((float)row - 3.99f) * rowDirection;
                Vector2 rowNearNextLength = ((float)row - 3.01f) * rowDirection;

                // set the quad to the row (4 cells)
                zoneBlockQuad.a = positionXZ - 4f * columnDirection + rowNearPreviousLength;
                zoneBlockQuad.b = positionXZ + 0f * columnDirection + rowNearPreviousLength; // TODO change 0f to 4f to support 8 tiles deep zones
                zoneBlockQuad.c = positionXZ + 0f * columnDirection + rowNearNextLength; // TODO change 0f to 4f to support 8 tiles deep zones
                zoneBlockQuad.d = positionXZ - 4f * columnDirection + rowNearNextLength;

                // Intersect the row quad with the other zone block quad
                if (zoneBlockQuad.Intersect(otherZoneBlockQuad))
                {
                    for (int column = 0; (long)column < ZoneDepthPatches.COLUMN_COUNT && ((long)valid & 1L << (row << 3 | column)) != 0L; ++column)
                    {
                        // calculate 2 relative column positions: 
                        // * one 0.01m from previous column
                        // * one 0.01m from next column
                        Vector2 columnNearPreviousLength = ((float)column - 3.99f) * columnDirection;
                        Vector2 columnNearNextLength = ((float)column - 3.01f) * columnDirection;

                        // middle position of the cell
                        Vector2 cellMiddlePos = positionXZ + (columnNearNextLength + columnNearPreviousLength + rowNearNextLength + rowNearPreviousLength) * 0.5f;

                        // check if the middle position of the cell is contained in the quad of the other zone block (1 cell tolerance)
                        if (Quad2.Intersect(otherZoneBlockQuad.a - otherColumnDirection - otherRowDirection, otherZoneBlockQuad.b + otherColumnDirection - otherRowDirection,
                            otherZoneBlockQuad.c + otherColumnDirection + otherRowDirection, otherZoneBlockQuad.d - otherColumnDirection + otherRowDirection, cellMiddlePos))
                        {
                            // Create a quad for the cell
                            Quad2 cellQuad = new Quad2
                            {
                                a = positionXZ + columnNearPreviousLength + rowNearPreviousLength,
                                b = positionXZ + columnNearNextLength + rowNearPreviousLength,
                                c = positionXZ + columnNearNextLength + rowNearNextLength,
                                d = positionXZ + columnNearPreviousLength + rowNearNextLength
                            };

                            // cycle through the cells of the other zone block
                            bool cellIsValid = true;
                            bool shareCell = false;
                            for (int otherRow = 0; otherRow < otherRowCount && cellIsValid; ++otherRow)
                            {
                                // calculate 2 relative row positions for the cell in the other zone block: 
                                // * one 0.01m from previous row
                                // * one 0.01m from next row
                                Vector2 otherRowNearPreviousLength = ((float)otherRow - 3.99f) * otherRowDirection;
                                Vector2 otherRowNearNextLength = ((float)otherRow - 3.01f) * otherRowDirection;

                                for (int otherColumn = 0; (long)otherColumn < ZoneDepthPatches.COLUMN_COUNT && cellIsValid; ++otherColumn)
                                {
                                    // checks if the cell is marked as valid in the valid mask of the other block, and that it is not contained in the shared mask
                                    if (((long)other.m_valid & ~(long)other.m_shared & 1L << (otherRow << 3 | otherColumn)) != 0L)
                                    {
                                        // calculate 2 relative column positions for the cell in the other zone block: 
                                        // * one 0.01m from previous column
                                        // * one 0.01m from next column
                                        Vector2 otherColumnNearPreviousLength = ((float)otherColumn - 3.99f) * otherColumnDirection;
                                        Vector2 otherColumnNearNextLength = ((float)otherColumn - 3.01f) * otherColumnDirection;

                                        // squared distance between the 2 cell middle positions
                                        float cellMiddleDist = Vector2.SqrMagnitude(otherPositionXZ + (otherColumnNearNextLength + otherColumnNearPreviousLength + otherRowNearNextLength + otherRowNearPreviousLength) * 0.5f - cellMiddlePos);

                                        // check if the 2 cells can touch
                                        if ((double)cellMiddleDist < 144.0)
                                        {
                                            if (!deleted) // other zone block not deleted:
                                            {
                                                // difference of 2 radian angles (360 deg = 2*PI * 0.6366197f = 4f)
                                                // that means an angle difference of 90 deg would result in 1f
                                                float angleDiff = Mathf.Abs(other.m_angle - __instance.m_angle) * 0.6366197f;
                                                float rightAngleDiff = angleDiff - Mathf.Floor(angleDiff); // difference from 90 deg

                                                // if the 2 cells are almost in the same spot with an angle difference of 0 90 180 270 deg, mark one of them as shared
                                                if ((double)cellMiddleDist < 0.00999999977648258 && ((double)rightAngleDiff < 0.00999999977648258 || (double)rightAngleDiff > 0.990000009536743))
                                                {
                                                    // The cell closer to road (or that was created earler) is kept, the other marked as shared
                                                    if (column < otherColumn || column == otherColumn && __instance.m_buildIndex < other.m_buildIndex)
                                                        other.m_shared |= (ulong)(1L << (otherRow << 3 | otherColumn));
                                                    else
                                                        shareCell = true;
                                                }
                                                // angles not right or not in the same place: Intersect the 2 cells
                                                else if (cellQuad.Intersect(new Quad2()
                                                {
                                                    a = otherPositionXZ + otherColumnNearPreviousLength + otherRowNearPreviousLength,
                                                    b = otherPositionXZ + otherColumnNearNextLength + otherRowNearPreviousLength,
                                                    c = otherPositionXZ + otherColumnNearNextLength + otherRowNearNextLength,
                                                    d = otherPositionXZ + otherColumnNearPreviousLength + otherRowNearNextLength
                                                }))
                                                {
                                                    // mark the cell which is further away from the road (or was created later) as invalid
                                                    // TODO adapt for 8 cell zones (low priority)
                                                    if (otherColumn >= 4 && column >= 4 || otherColumn < 4 && column < 4)
                                                    {
                                                        bool preserveOldZones = ZoneBlockData.Instance.PreserveOlder(blockID);
                                                        bool preserveNewZones = ZoneBlockData.Instance.PreserveNewer(blockID);

                                                        if ((preserveOldZones && __instance.m_buildIndex > other.m_buildIndex) || (preserveNewZones && __instance.m_buildIndex < other.m_buildIndex))
                                                        {
                                                            cellIsValid = false;
                                                        }
                                                        else if ((preserveOldZones && __instance.m_buildIndex < other.m_buildIndex) || (preserveNewZones && __instance.m_buildIndex > other.m_buildIndex))
                                                        {
                                                            other.m_valid &= (ulong)~(1L << (otherRow << 3 | otherColumn));
                                                        }
                                                        else if (otherColumn >= 2 && column >= 2 || otherColumn < 2 && column < 2)
                                                        {
                                                            if (__instance.m_buildIndex < other.m_buildIndex)
                                                                other.m_valid &= (ulong)~(1L << (otherRow << 3 | otherColumn));
                                                            else
                                                                cellIsValid = false;
                                                        }
                                                        else if (otherColumn < 2)
                                                            cellIsValid = false;
                                                        else
                                                            other.m_valid &= (ulong)~(1L << (otherRow << 3 | otherColumn));
                                                    }
                                                    else if (otherColumn < 4)
                                                        cellIsValid = false;
                                                    else
                                                        other.m_valid &= (ulong)~(1L << (otherRow << 3 | otherColumn));
                                                }
                                            }
                                            // distance between cell middle pos < 6 = cells colliding and both cells near road 
                                            // if the cell is unzoned, take over the zone type of the other one
                                            if ((double)cellMiddleDist < 36.0 && column < 4 && otherColumn < 4)
                                            {
                                                ItemClass.Zone zone1 = __instance.GetZone(column, row);
                                                ItemClass.Zone zone2 = other.GetZone(otherColumn, otherRow);
                                                if (zone1 == ItemClass.Zone.Unzoned)
                                                    __instance.SetZone(column, row, zone2);
                                                else if (zone2 == ItemClass.Zone.Unzoned && !deleted)
                                                    other.SetZone(otherColumn, otherRow, zone1);
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
                                shared |= (ulong)(1L << (row << 3 | column));
                        }
                    }
                }
            }

            return false;
        }
    }
}