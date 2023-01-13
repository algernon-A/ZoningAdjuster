// <copyright file="ZoneDepthPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ZoningAdjuster
{
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using AlgernonCommons;
    using HarmonyLib;

    /// <summary>
    ///  Harmony patches to implement variable zone depth.
    /// </summary>
    [HarmonyPatch(typeof(ZoneBlock))]
    public static class ZoneDepthPatches
    {
        /// <summary>
        /// Number of columns per zone block (always 4).
        /// </summary>
        public const uint ColumnCount = 4;

        /// <summary>
        /// Gets or sets the current zone depth setting (in cells).
        /// </summary>
        internal static byte ZoneDepth { get; set; } = 3;

        /// <summary>
        /// Harmony Transpiler for ZoneBlock.CalculateBlock1 to implement zone depth limiting.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Patched ILCode.</returns>
        [HarmonyPatch("CalculateBlock1")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Local field indexes.
            const int ValidIndex = 12;
            const int RowIndex = 14;
            const int ColumnIndex = 20;
            const int TargetIndex = 24;

            Logging.Message("transpiling CalculateBlock1");

            foreach (CodeInstruction instruction in instructions)
            {
                // Looking for stloc.s 24 as our target to insert call to custom method immediately afterwards.
                if (instruction.opcode == OpCodes.Stloc_S && instruction.operand is LocalBuilder localBuilder && localBuilder.LocalIndex == TargetIndex)
                {
                    Logging.KeyMessage("found stloc.s 24");

                    yield return instruction;

                    // Insert call to our custom method.
                    yield return new CodeInstruction(OpCodes.Ldarg, 0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ZoneBlock), nameof(ZoneBlock.m_segment)));
                    yield return new CodeInstruction(OpCodes.Ldloc_S, ValidIndex);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, RowIndex);
                    yield return new CodeInstruction(OpCodes.Ldloc_S, ColumnIndex);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ZoneDepthPatches), nameof(CheckZoneDepth)));
                    yield return new CodeInstruction(OpCodes.Stloc_S, ValidIndex);

                    continue;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Applies custom zone depth maximum to the given ZoneBlock valid mask for the given cell.
        /// </summary>
        /// <param name="segment">Network segment ID.</param>
        /// <param name="valid">ZoneBlock valid mask.</param>
        /// <param name="row">Cell row.</param>
        /// <param name="column">Cell column.</param>
        /// <returns>Updated valid mask.</returns>
        public static ulong CheckZoneDepth(ushort segment, ulong valid, int row, int column)
        {
            // Invalidate this cell if the column is beyond the maximum permitted zone depth.
            if (column > SegmentData.Instance.GetEffectiveDepth(segment))
            {
                return valid &= (ulong)~(1L << (row << 3 | column));
            }

            // If we got here, this column is valid; just return the unaltered mask.
            return valid;
        }

        /*
        /// <summary>
        /// Pre-emptive Harmony patch for ZoneManager.CalculateBlock1.
        /// Updates the "valid" bitmask for a zoning block; called by ZoneManager when blocks are created.
        /// Thanks to boformer for the analysis of the game code.
        /// </summary>
        /// <param name="__instance">Zone block instance reference.</param>
        /// <param name="blockID">Zone block ID.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch("CalculateBlock1")]
        [HarmonyPrefix]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
        public static bool CalculateBlock1(ref ZoneBlock __instance, ushort blockID)
        {
            // Don't do anything with zone blocks which are not in use.
            if (((int)__instance.m_flags & 3) != ZoneBlock.FLAG_CREATED)
            {
                // Never execute original method.
                return false;
            }

            // Width of this zone block.
            int rowCount = __instance.RowCount;

            // Directions of the rows and columns based on zone block angle, multiplied by 8 (cell size).
            Vector2 columnDirection = new Vector2(Mathf.Cos(__instance.m_angle), Mathf.Sin(__instance.m_angle)) * 8f;
            Vector2 rowDirection = new Vector2(columnDirection.y, -columnDirection.x);

            // Origin of the zone block.
            // This position is in the center of the 8x8 zone block (4 columns and 4 rows away from the lower corner).
            Vector2 positionXZ = VectorUtils.XZ(__instance.m_position);

            // area of the zone block (8x4 cells)
            Quad2 zoneBlockQuad = new Quad2()
            {
                a = positionXZ - (ColumnCount * columnDirection) - (ColumnCount * rowDirection),
                b = positionXZ + (0f * columnDirection) - (ColumnCount * rowDirection), // TODO change 0f to 4f to support 8 tiles deep zones
                c = positionXZ + (0f * columnDirection) + ((float)(rowCount - ColumnCount) * rowDirection), // TODO change 0f to 4f to support 8 tiles deep zones
                d = positionXZ - (ColumnCount * columnDirection) + ((float)(rowCount - ColumnCount) * rowDirection),
            };

            Vector2 quadMin = zoneBlockQuad.Min();
            Vector2 quadMax = zoneBlockQuad.Max();

            NetManager netManager = Singleton<NetManager>.instance;

            // Calculate which net segment grid cells are touched by this zone block.
            int gridMinX = Mathf.Max((int)((((double)quadMin.x - 64.0d) / 64.0d) + 135.0d), 0);
            int gridMinY = Mathf.Max((int)((((double)quadMin.y - 64.0d) / 64.0d) + 135.0d), 0);
            int gridMaxX = Mathf.Min((int)((((double)quadMax.x + 64.0d) / 64.0d) + 135.0d), 269);
            int gridMaxY = Mathf.Min((int)((((double)quadMax.y + 64.0d) / 64.0d) + 135.0d), 269);

            // This bitmask stores which which cells are "valid" and which are "invalid"
            // (e.g. colliding with existing buildings or height too steep)
            // This mask limits the maximum size of a zone block to 64 cells (e.g. 8x8)
            // Sort order: (row 8|col 8)(row 8|col 7)...(row 8|col 2)(row 8|col 1)(row 7|col 4)(row 7|col 3)...(row 1|col 1)
            ulong valid = ulong.MaxValue;
            bool quadOutOfArea = Singleton<GameAreaManager>.instance.QuadOutOfArea(zoneBlockQuad);

            // Mark cells which are on too steep terrain or outside of the purchased tiles as invalid.
            for (int row = 0; row < rowCount; ++row)
            {
                // Calculate 3 relative row positions:
                // * One in between 2 cell grid points
                // * one 0.1m from previous row
                // * one 0.1m from next row
                Vector2 rowMiddleLength = ((float)row - 3.5f) * rowDirection;
                Vector2 rowNearPreviousLength = ((float)row - 3.9f) * rowDirection;
                Vector2 rowNearNextLength = ((float)row - 3.1f) * rowDirection;

                // Calculate terrain height of the row (5 columns away from zone block origin, which is where the road is).
                float height = Singleton<TerrainManager>.instance.SampleRawHeightSmooth(VectorUtils.X_Y(positionXZ + rowMiddleLength - (5f * columnDirection)));

                for (int column = 0; (long)column < ColumnCount; ++column)
                {
                    // Calculate 2 relative column positions:
                    // * one 0.1m from previous column
                    // * one 0.1m from next column
                    Vector2 columnNearPreviousLength = ((float)column - 3.9f) * columnDirection;
                    Vector2 columnNearNextLength = ((float)column - 3.1f) * columnDirection;

                    // Calculate terrain height of the cell (row middle, side away from road).
                    float cellHeight = Singleton<TerrainManager>.instance.SampleRawHeightSmooth(VectorUtils.X_Y(positionXZ + rowMiddleLength + columnNearNextLength));

                    */
                    /*--- Start insert here ---*/
                    /*
                    // Mark any cells in columns above our set maxmimum zoning depth as invalid.
                    if (column > SegmentData.Instance.GetEffectiveDepth(__instance.m_segment))
                    {
                        valid &= (ulong)~(1L << (row << 3 | column));
                    }
                    */
                    /*--- Finish insert here ---*/
                    /*
                     *
                    // If the height difference between road and cell is greater than 8m, mark the cell as invalid.
                    if ((double)Mathf.Abs(cellHeight - height) > 8.0)
                    {
                        // TODO maybe this should be raised for 8 cell deep zones?
                        valid &= (ulong)~(1L << (row << 3 | column));
                    }
                    else if (quadOutOfArea)
                    {
                        // If the cell is outside of the purchased tiles, mark the cell as invalid.
                        if (Singleton<GameAreaManager>.instance.QuadOutOfArea(new Quad2()
                        {
                            a = positionXZ + columnNearPreviousLength + rowNearPreviousLength,
                            b = positionXZ + columnNearNextLength + rowNearPreviousLength,
                            c = positionXZ + columnNearNextLength + rowNearNextLength,
                            d = positionXZ + columnNearPreviousLength + rowNearNextLength,
                        }))
                        {
                            valid &= (ulong)~(1L << (row << 3 | column));
                        }
                    }
                }
            }

            // Mark cells which are overlapped by network segments as invalid.
            for (int gridY = gridMinY; gridY <= gridMaxY; ++gridY)
            {
                for (int gridX = gridMinX; gridX <= gridMaxX; ++gridX)
                {
                    // Cycle through all net segments in the grid cell.
                    ushort segmentID = netManager.m_segmentGrid[(gridY * 270) + gridX];
                    int counter = 0;
                    while ((int)segmentID != 0)
                    {
                        if (netManager.m_segments.m_buffer[(int)segmentID].Info.m_class.m_layer == ItemClass.Layer.Default)
                        {
                            ushort startNode = netManager.m_segments.m_buffer[(int)segmentID].m_startNode;
                            ushort endNode = netManager.m_segments.m_buffer[(int)segmentID].m_endNode;
                            Vector3 startNodePos = netManager.m_nodes.m_buffer[(int)startNode].m_position;
                            Vector3 endNodePos = netManager.m_nodes.m_buffer[(int)endNode].m_position;

                            // Check if the segment (one of its nodes) is in the area of zone block.
                            if ((double)Mathf.Max(Mathf.Max(quadMin.x - 64f - startNodePos.x, quadMin.y - 64f - startNodePos.z), Mathf.Max((float)((double)startNodePos.x - (double)quadMax.x - 64.0), (float)((double)startNodePos.z - (double)quadMax.y - 64.0))) < 0.0
                                || (double)Mathf.Max(Mathf.Max(quadMin.x - 64f - endNodePos.x, quadMin.y - 64f - endNodePos.z), Mathf.Max((float)((double)endNodePos.x - (double)quadMax.x - 64.0), (float)((double)endNodePos.z - (double)quadMax.y - 64.0))) < 0.0)
                            {
                                // Mark zone cells overlapped by network segments as invalid.
                                CalculateImplementation1(ref __instance, blockID, segmentID, ref netManager.m_segments.m_buffer[(int)segmentID], ref valid, quadMin.x, quadMin.y, quadMax.x, quadMax.y);
                            }
                        }

                        // Next segment in grid cell (linked list).
                        segmentID = netManager.m_segments.m_buffer[(int)segmentID].m_nextGridSegment;

                        // Replicate limit check from game method.
                        if (++counter >= 36864)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }

            // This part marks all cells as invalid which are behind existing invalid cells (so that there are no cells with no road access).
            // 0000 0100 0000 0100 0000 0100 0000 0100
            // 0000 0100 0000 0100 0000 0100 0000 0100
            ulong mask = 144680345676153346;
            for (int iteration = 0; iteration < 7; ++iteration)
            {
                valid = (ulong)((long)valid & ~(long)mask | (long)valid & (long)valid << 1 & (long)mask); // TODO switch to ulong for 8 cell support?
                mask <<= 1;
            }

            // Apply the new mask, reset shared mask.
            __instance.m_valid = valid;
            __instance.m_shared = 0UL;

            // Never execute original method.
            return false;
        }
        */
    }
}
